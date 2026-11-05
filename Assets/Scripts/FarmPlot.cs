using System;
using UnityEngine;

/// <summary>
/// Одна грядка. Хранит своё состояние S0-S4 и показывает его картинкой/цветом.
/// Клетка-цель определяется положением объекта (центр грядки должен быть в центре клетки).
/// </summary>
public class FarmPlot : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlotState startState = PlotState.Normal;

    [Header("Картинки S0-S4 (необязательно; пока пусто - используются оттенки)")]
    [SerializeField] private Sprite[] stateSprites = new Sprite[5];

    [Header("Оттенки S0-S4: множитель к исходному цвету грядки")]
    [SerializeField] private Color[] stateTints =
    {
        new Color(1.00f, 1.00f, 1.00f), // S0 обычная земля (как сейчас)
        new Color(0.55f, 0.55f, 0.55f), // S1 вспаханная (темнее)
        new Color(0.75f, 1.00f, 0.60f), // S2 сухой посев
        new Color(0.55f, 0.75f, 1.00f), // S3 политый посев
        new Color(1.00f, 0.85f, 0.30f)  // S4 зрелая морковь
    };

    private Color baseColor = Color.white;

    public PlotState State { get; private set; }
    public Vector2Int Cell { get; private set; }
    public Transform Root => transform;

    public event Action<FarmPlot> StateChanged;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("FarmPlot: на объекте нет Sprite Renderer.", this);
            enabled = false;
            return;
        }

        baseColor = spriteRenderer.color;
        Cell = FarmGrid.WorldToCell(transform.position);

        Vector2 center = FarmGrid.CellCenter(Cell);
        if (Vector2.Distance(center, transform.position) > 0.02f)
        {
            Debug.LogWarning("FarmPlot '" + name + "': центр грядки не в центре клетки. " +
                             "Выделите её и выберите Farm -> Snap Selected To Cell Center.", this);
        }

        State = startState;
        ApplyVisual();
    }

    private void OnEnable()
    {
        // Cell уже посчитана в Awake.
        InteractableRegistry.Register(this);
    }

    private void OnDisable()
    {
        InteractableRegistry.Unregister(this);
    }

    public bool ShowsHighlight(InventoryModel inventory)
    {
        return PlotRules.Resolve(State, inventory.Selected) != PlotAction.None;
    }

    public string GetPrompt(InventoryModel inventory)
    {
        switch (PlotRules.Resolve(State, inventory.Selected))
        {
            case PlotAction.Till: return "E — вспахать";
            default:              return null;
        }
    }

    public void Interact(InventoryModel inventory)
    {
        switch (PlotRules.Resolve(State, inventory.Selected))
        {
            case PlotAction.Till:
                SetState(PlotState.Tilled); // вспашка ничего не расходует
                break;

            default:
                break; // недопустимое сочетание: ничего не меняется, ошибки нет
        }
    }

    private void SetState(PlotState newState)
    {
        if (newState == State)
            return;

        State = newState;
        ApplyVisual();
        StateChanged?.Invoke(this);
    }

    private void ApplyVisual()
    {
        int index = (int)State;

        if (stateSprites != null && index < stateSprites.Length && stateSprites[index] != null)
            spriteRenderer.sprite = stateSprites[index];

        Color tint = Color.white;
        if (stateTints != null && index < stateTints.Length)
            tint = stateTints[index];

        spriteRenderer.color = baseColor * tint;
    }
}
