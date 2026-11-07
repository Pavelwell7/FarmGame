using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Номер дня и переход дня (сон). Один вызов SleepNow = один переход дня.
/// </summary>
public class DayManager : MonoBehaviour
{
    [SerializeField] private PromptHud hud;

    public int Day { get; private set; } = 1;

    public event Action Changed;

    private void Awake()
    {
        Day = 1;
    }

    private void Start()
    {
        if (hud == null)
        {
            Debug.LogError("DayManager: не назначен Hud (объект с Prompt Hud).", this);
            return;
        }

        hud.SetDay(Day);
    }

    /// <summary>
    /// Фаза 1: для каждой грядки считаем результат по состоянию ДО сна.
    /// Фаза 2: применяем всё сразу. День +1. Сон ничего не добавляет в запас.
    /// </summary>
    public void SleepNow()
    {
        IReadOnlyList<FarmPlot> all = FarmPlot.All;
        FarmPlot[] plots = new FarmPlot[all.Count];
        for (int i = 0; i < plots.Length; i++)
            plots[i] = all[i];

        PlotState[] results = new PlotState[plots.Length];

        for (int i = 0; i < plots.Length; i++)
            results[i] = PlotRules.AfterSleep(plots[i].State);

        for (int i = 0; i < plots.Length; i++)
            plots[i].ApplySleepResult(results[i]);

        Day++;

        if (hud != null)
            hud.SetDay(Day);

        Changed?.Invoke();
    }
}
