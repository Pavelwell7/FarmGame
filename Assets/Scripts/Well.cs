using UnityEngine;

/// <summary>
/// Колодец. Одна клетка взаимодействия (клетка, где стоит объект, или Interaction Point, если задан).
/// Только с лейкой: наполняет до максимума. Землю под собой не поливает.
/// </summary>
public class Well : MonoBehaviour, IInteractable
{
    [Tooltip("Необязательно. Если пусто, клетка взаимодействия = клетка, где стоит колодец.")]
    [SerializeField] private Transform interactionPoint;

    public Vector2Int Cell { get; private set; }
    public Transform Root => transform;

    private void Awake()
    {
        Transform point = interactionPoint != null ? interactionPoint : transform;
        Cell = FarmGrid.WorldToCell(point.position);

        Vector2 center = FarmGrid.CellCenter(Cell);
        if (Vector2.Distance(center, point.position) > 0.02f)
        {
            Debug.LogWarning("Well '" + name + "': центр не в центре клетки. " +
                             "Выделите объект и выберите Farm -> Snap Selected To Cell Center.", this);
        }
    }

    private void OnEnable()
    {
        InteractableRegistry.Register(this);
    }

    private void OnDisable()
    {
        InteractableRegistry.Unregister(this);
    }

    // По ТЗ рамка подсветки нужна только грядкам; у колодца есть текст действия.
    public bool ShowsHighlight(InventoryModel inventory)
    {
        return false;
    }

    public string GetPrompt(InventoryModel inventory)
    {
        return inventory.Selected == ItemType.WateringCan ? "E — набрать воду" : null;
    }

    public string Interact(InventoryModel inventory)
    {
        // С другим предметом наполнения нет.
        if (inventory.Selected != ItemType.WateringCan)
            return null;

        if (inventory.Water >= inventory.MaxWater)
            return "Лейка полная";

        inventory.RefillWater(); // сразу максимум, а не "плюс 5"
        return null;
    }
}
