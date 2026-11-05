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
/// Что можно сделать с грядкой. На этапе 2 только вспашка; остальное добавим на этапах 3-5.
/// </summary>
public enum PlotAction
{
    None,
    Till
}

/// <summary>
/// Таблица "состояние x предмет -> действие" из ТЗ (раздел 3).
/// Чистая логика без Unity: удобно проверять тестами.
/// </summary>
public static class PlotRules
{
    public static PlotAction Resolve(PlotState state, ItemType item)
    {
        // Этап 2: мотыга на обычной земле. Остальное - в следующих этапах.
        if (state == PlotState.Normal && item == ItemType.Hoe)
            return PlotAction.Till;

        return PlotAction.None;
    }
}
