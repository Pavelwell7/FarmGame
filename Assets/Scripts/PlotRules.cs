/// <summary>
/// Состояния грядки из ТЗ: S0-S4.
/// </summary>
public enum PlotState
{
    Normal = 0,         // S0 обычная земля
    Tilled = 1,         // S1 вспаханная свободная
    PlantedDry = 2,     // S2 посев сухой
    PlantedWatered = 3, // S3 посев политый
    Mature = 4          // S4 зрелая морковь
}

/// <summary>
/// Что можно сделать с грядкой.
/// </summary>
public enum PlotAction
{
    None,
    Till,           // вспахать: S0 -> S1
    Plant,          // посадить: S1 -> S2, семена минус 1
    Water,          // лейка: S0/S1 вода -1; S2 -> S3 вода -1; S3 вода -1
    ClearTilled,    // лопата на S1: S1 -> S0, семена не выдаются
    RemoveSeedling, // лопата на S2/S3: -> S0, семя возвращается
    Harvest         // S4 с любым предметом: морковь +1, грядка -> S1
}

/// <summary>
/// Таблица "состояние x предмет -> действие" из ТЗ (раздел 3).
/// Чистая логика без Unity: удобно проверять тестами.
/// Проверка наличия ресурса (есть ли семена) делается позже, при выполнении действия.
/// </summary>
public static class PlotRules
{
    public static PlotAction Resolve(PlotState state, ItemType item)
    {
        switch (state)
        {
            case PlotState.Mature:
                // Сбор важнее любого инструмента и ничего не расходует.
                return PlotAction.Harvest;

            case PlotState.Normal:
                if (item == ItemType.Hoe) return PlotAction.Till;
                if (item == ItemType.WateringCan) return PlotAction.Water; // полив пустой земли допустим
                break;

            case PlotState.Tilled:
                if (item == ItemType.Seeds) return PlotAction.Plant;
                if (item == ItemType.WateringCan) return PlotAction.Water; // полив пустой грядки допустим
                if (item == ItemType.Shovel) return PlotAction.ClearTilled;
                break;

            case PlotState.PlantedDry:
            case PlotState.PlantedWatered:
                if (item == ItemType.WateringCan) return PlotAction.Water;
                if (item == ItemType.Shovel) return PlotAction.RemoveSeedling;
                break;
        }

        return PlotAction.None;
    }

    /// <summary>
    /// Результат сна для грядки (ТЗ, раздел 6):
    /// S0 -> S0, S1 -> S0, S2 -> S2, S3 -> S4, S4 -> S4.
    /// </summary>
    public static PlotState AfterSleep(PlotState state)
    {
        switch (state)
        {
            case PlotState.Tilled: return PlotState.Normal;
            case PlotState.PlantedWatered: return PlotState.Mature;
            default: return state;
        }
    }
}