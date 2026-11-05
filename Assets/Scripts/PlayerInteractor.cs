using UnityEngine;

/// <summary>
/// Выбор цели и действие по E.
/// Цель - ровно одна соседняя клетка по направлению взгляда, при условиях:
/// курсор на этой клетке, расстояние до её центра не больше maxDistance, между ногами и целью нет препятствия.
/// Точка ног - transform.position персонажа (pivot картинки стоит у ног) или feetPoint, если задан.
/// Направление берётся из PlayerMovement, предмет - из InventoryModel.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private InventoryModel inventory;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PromptHud promptHud;
    [SerializeField] private Camera worldCamera;
    [Tooltip("Необязательно. Если пусто, точка ног = позиция объекта.")]
    [SerializeField] private Transform feetPoint;
    [Tooltip("Слои, которые блокируют взаимодействие (стены, заборы).")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float maxDistance = 1.5f;

    private const float DistanceTolerance = 0.0001f;

    private Transform highlight;

    private Vector2 FeetPosition
    {
        get { return feetPoint != null ? (Vector2)feetPoint.position : (Vector2)transform.position; }
    }

    private void Start()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (worldCamera == null) worldCamera = Camera.main;

        if (inventory == null || movement == null || promptHud == null || worldCamera == null)
        {
            Debug.LogError("PlayerInteractor: не назначены Inventory, Movement, Prompt Hud или камера. Проверьте компонент в инспекторе.", this);
            enabled = false;
            return;
        }

        if (obstacleMask.value == 0)
            Debug.LogWarning("PlayerInteractor: Obstacle Mask пуст, заборы и стены не будут блокировать действие.", this);

        CreateHighlight();
    }

    private void OnDisable()
    {
        if (highlight != null) highlight.gameObject.SetActive(false);
        if (promptHud != null) promptHud.SetPrompt(null);
    }

    private void Update()
    {
        IInteractable target = FindTarget();

        // Условия проверяются заново в момент нажатия.
        if (target != null && GameInput.InteractPressedThisFrame())
        {
            target.Interact(inventory);
            target = FindTarget(); // сразу обновить подсветку и текст
        }

        UpdatePresentation(target);
    }

    private IInteractable FindTarget()
    {
        Vector2 feet = FeetPosition;
        Vector2Int feetCell = FarmGrid.WorldToCell(feet);
        Vector2Int targetCell = feetCell + FarmGrid.DirectionToOffset(movement.Facing);

        // Курсор над панелью интерфейса: мир не получает действие.
        Vector2 mouseScreen = GameInput.MouseScreenPosition();
        if (PointerUi.IsOver(mouseScreen))
            return null;

        // Курсор должен быть на клетке-цели.
        Vector3 mouseWorld = worldCamera.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, -worldCamera.transform.position.z));
        if (FarmGrid.WorldToCell(mouseWorld) != targetCell)
            return null;

        // Расстояние до центра клетки-цели.
        Vector2 center = FarmGrid.CellCenter(targetCell);
        if (Vector2.Distance(feet, center) > maxDistance + DistanceTolerance)
            return null;

        IInteractable target = InteractableRegistry.Get(targetCell);
        if (target == null)
            return null;

        if (IsBlocked(feet, center, target.Root))
            return null;

        return target;
    }

    private bool IsBlocked(Vector2 from, Vector2 to, Transform targetRoot)
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(from, to, obstacleMask);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            Transform t = hit.collider.transform;

            // Коллайдер самой цели и самого игрока не считаются препятствием.
            if (t.IsChildOf(targetRoot) || t.IsChildOf(transform))
                continue;

            return true;
        }

        return false;
    }

    private void UpdatePresentation(IInteractable target)
    {
        if (target != null && target.ShowsHighlight(inventory))
        {
            highlight.position = FarmGrid.CellCenter(target.Cell);
            highlight.gameObject.SetActive(true);
        }
        else
        {
            highlight.gameObject.SetActive(false);
        }

        promptHud.SetPrompt(target != null ? target.GetPrompt(inventory) : null);
    }

    private void CreateHighlight()
    {
        var go = new GameObject("TargetHighlight");
        highlight = go.transform;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateFrameSprite();
        renderer.color = new Color(1f, 0.95f, 0.3f, 1f);
        renderer.sortingOrder = 2;

        go.SetActive(false);
    }

    /// <summary>Квадратная рамка 1x1 единица, создаётся кодом, отдельный рисунок не нужен.</summary>
    private static Sprite CreateFrameSprite()
    {
        const int size = 32;
        const int border = 2;

        var pixels = new Color32[size * size];
        var white = new Color32(255, 255, 255, 255);
        var clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool edge = x < border || y < border || x >= size - border || y >= size - border;
                pixels[y * size + x] = edge ? white : clear;
            }
        }

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels32(pixels);
        texture.Apply();

        // pixelsPerUnit = size, значит спрайт ровно 1 x 1 единица.
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // В окне Scene (персонаж выделен, идёт игра): жёлтая клетка-цель и линия от ног.
    private void OnDrawGizmosSelected()
    {
        if (movement == null)
            return;

        Vector2 feet = FeetPosition;
        Vector2Int cell = FarmGrid.WorldToCell(feet) + FarmGrid.DirectionToOffset(movement.Facing);
        Vector2 center = FarmGrid.CellCenter(cell);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(feet, center);
        Gizmos.DrawWireCube(center, Vector3.one);
    }
}
