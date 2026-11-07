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
/// Что можно сделать с грядкой. Полив и сбор добавим на этапах 4-5.
/// </summary>
public enum PlotAction
{
    None,
    Till,           // вспахать: S0 -> S1
    Plant,          // посадить: S1 -> S2, семена минус 1
    ClearTilled,    // лопата на S1: S1 -> S0, семена не выдаются
    RemoveSeedling  // лопата на S2/S3: -> S0, семя возвращается
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
            case PlotState.Normal:
                if (item == ItemType.Hoe) return PlotAction.Till;
                break;

            case PlotState.Tilled:
                if (item == ItemType.Seeds) return PlotAction.Plant;
                if (item == ItemType.Shovel) return PlotAction.ClearTilled;
                break;

            case PlotState.PlantedDry:
            case PlotState.PlantedWatered:
                if (item == ItemType.Shovel) return PlotAction.RemoveSeedling;
                break;
        }

        return PlotAction.None;
    }
}