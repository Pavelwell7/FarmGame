using UnityEngine;

/// <summary>
/// Кровать. Одна клетка взаимодействия. С любым предметом в руках E открывает вопрос «Закончить день?».
/// </summary>
public class Bed : MonoBehaviour, IInteractable
{
    [SerializeField] private SleepDialog sleepDialog;

    [Tooltip("Необязательно. Если пусто, клетка взаимодействия = клетка, где стоит кровать.")]
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
            Debug.LogWarning("Bed '" + name + "': центр не в центре клетки. " +
                             "Выделите объект и выберите Farm -> Snap Selected To Cell Center.", this);
        }

        if (sleepDialog == null)
            Debug.LogError("Bed: не назначен Sleep Dialog (объект Game).", this);
    }

    private void OnEnable()
    {
        InteractableRegistry.Register(this);
    }

    private void OnDisable()
    {
        InteractableRegistry.Unregister(this);
    }

    public bool ShowsHighlight(InventoryModel inventory)
    {
        return false;
    }

    public string GetPrompt(InventoryModel inventory)
    {
        return "E — закончить день"; // с любым предметом
    }

    public string Interact(InventoryModel inventory)
    {
        if (sleepDialog != null)
            sleepDialog.Open();

        return null;
    }
}
