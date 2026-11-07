using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Объект, который подсвечивается автоматически, когда игрок рядом (зрелая морковь).
/// Предмет, направление взгляда и курсор для этой подсветки не важны.
/// </summary>
public interface IAutoHighlight
{
    bool IsAutoHighlighted { get; }
}

/// <summary>
/// Одна грядка. Хранит своё состояние S0-S4 и показывает его картинкой/цветом.
/// Клетка-цель определяется положением объекта (центр грядки должен быть в центре клетки).
/// </summary>
public class FarmPlot : MonoBehaviour, IInteractable, IAutoHighlight
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlotState startState = PlotState.Normal;

    [Header("Картинки S0-S4 (необязательно; пока пусто - используются оттенки)")]
    [SerializeField] private Sprite[] stateSprites = new Sprite[5];

    [Header("Оттенки S0-S4: множитель к исходному цвету грядки")]
    [SerializeField]
    private Color[] stateTints =
    {
        new Color(1.00f, 1.00f, 1.00f), // S0 обычная земля (как сейчас)
        new Color(0.55f, 0.55f, 0.55f), // S1 вспаханная (темнее)
        new Color(0.75f, 1.00f, 0.60f), // S2 сухой посев
        new Color(0.55f, 0.75f, 1.00f), // S3 политый посев
        new Color(1.00f, 0.85f, 0.30f)  // S4 зрелая морковь
    };

    private static readonly List<FarmPlot> AllPlots = new List<FarmPlot>();

    /// <summary>Все включённые грядки сцены (нужны для сна).</summary>
    public static IReadOnlyList<FarmPlot> All => AllPlots;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        AllPlots.Clear();
    }

    private Color baseColor = Color.white;

    // True, если в этой грядке лежит семя, которое реально было списано при посадке.
    // Только за такое семя лопата возвращает одно семя.
    private bool seedWasSpent;

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
        seedWasSpent = false;
        ApplyVisual();
    }

    private void OnEnable()
    {
        // Cell уже посчитана в Awake.
        InteractableRegistry.Register(this);

        if (!AllPlots.Contains(this))
            AllPlots.Add(this);
    }

    private void OnDisable()
    {
        InteractableRegistry.Unregister(this);
        AllPlots.Remove(this);
    }

    /// <summary>Зрелую морковь подсвечиваем автоматически, когда игрок рядом.</summary>
    public bool IsAutoHighlighted => State == PlotState.Mature;

    /// <summary>Применить результат сна (его считает DayManager сразу для всех грядок).</summary>
    public void ApplySleepResult(PlotState result)
    {
        if (result == PlotState.Mature)
            seedWasSpent = false; // у зрелой моркови семя не возвращается

        SetState(result);
    }

    private bool IsPlanted => State == PlotState.PlantedDry || State == PlotState.PlantedWatered;

    /// <summary>
    /// Рамка подсветки: обычная земля с мотыгой, свободная вспаханная с семенами, посев с лейкой.
    /// Лопата рамку не даёт (по ТЗ), но текст действия показывается.
    /// Полив пустой земли рамку и текст не даёт, но E при этом поливает (расходует воду).
    /// Наличие ресурса на рамку не влияет: при нуле семян или воды рамка остаётся.
    /// </summary>
    public bool ShowsHighlight(InventoryModel inventory)
    {
        PlotAction action = PlotRules.Resolve(State, inventory.Selected);
        return action == PlotAction.Till
            || action == PlotAction.Plant
            || (action == PlotAction.Water && IsPlanted);
    }

    public string GetPrompt(InventoryModel inventory)
    {
        switch (PlotRules.Resolve(State, inventory.Selected))
        {
            case PlotAction.Till: return "E — вспахать";
            case PlotAction.Plant: return "E — посадить";
            case PlotAction.Water: return IsPlanted ? "E — полить" : null;
            case PlotAction.Harvest: return "E — собрать";
            case PlotAction.ClearTilled:
            case PlotAction.RemoveSeedling: return "E — убрать грядку";
            default: return null;
        }
    }

    public string Interact(InventoryModel inventory)
    {
        switch (PlotRules.Resolve(State, inventory.Selected))
        {
            case PlotAction.Till:
                SetState(PlotState.Tilled); // вспашка ничего не расходует
                return null;

            case PlotAction.Plant:
                // Списание семени и появление посева - одно действие:
                // нет семени - ничего не меняем, есть - списали и посадили сразу.
                if (!inventory.TrySpendSeed())
                    return "Нет семян";

                seedWasSpent = true;
                SetState(PlotState.PlantedDry); // посев всегда сухой
                return null;

            case PlotAction.Water:
                // Допустимое применение лейки расходует одну воду, даже на пустой земле.
                if (!inventory.TrySpendWater())
                    return "Лейка пуста";

                // Сухой посев становится политым; политый остаётся политым (роста нет).
                if (State == PlotState.PlantedDry)
                    SetState(PlotState.PlantedWatered);
                return null;

            case PlotAction.Harvest:
                // Сбор важнее инструмента: ни семя, ни вода не тратятся.
                inventory.AddCarrot(); // ровно одна морковь прямо в запас
                seedWasSpent = false;  // растение удалено, семя не возвращается
                SetState(PlotState.Tilled); // грядка -> S1, можно сразу сажать
                return null;

            case PlotAction.ClearTilled:
                SetState(PlotState.Normal); // семена не выдаются
                return null;

            case PlotAction.RemoveSeedling:
                // Возврат одного семени только за фактически посаженное.
                if (seedWasSpent)
                    inventory.AddSeeds(1);

                seedWasSpent = false; // данные посева очищены
                SetState(PlotState.Normal);
                return null;

            default:
                return null; // недопустимое сочетание: ничего не меняется, ошибки нет
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