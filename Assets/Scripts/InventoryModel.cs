using System;
using UnityEngine;

/// <summary>
/// Единственный источник данных об инвентаре: выбранный предмет, семена, вода, морковь.
/// Все остальные скрипты (картинка персонажа, панель, будущие действия) только читают отсюда.
/// </summary>
public class InventoryModel : MonoBehaviour
{
    /// <summary>Порядок ячеек: клавиши 1-5.</summary>
    public static readonly ItemType[] Slots =
    {
        ItemType.Hoe,
        ItemType.Seeds,
        ItemType.WateringCan,
        ItemType.Shovel,
        ItemType.Carrot
    };

    [Header("Стартовые значения теста")]
    [SerializeField] private ItemType startItem = ItemType.Hoe;
    [SerializeField] private int startSeeds = 5;
    [SerializeField] private int startCarrots = 0;
    [SerializeField] private int maxWater = 5;
    [SerializeField] private int startWater = 5;

    public ItemType Selected { get; private set; }
    public int Seeds { get; private set; }
    public int Carrots { get; private set; }
    public int Water { get; private set; }
    public int MaxWater => maxWater;

    /// <summary>
    /// Есть ли что-то в руках. Морковь в руках только если выбрана И её больше нуля.
    /// </summary>
    public bool IsHoldingSomething => Selected != ItemType.Carrot || Carrots > 0;

    /// <summary>Вызывается после любого изменения выбора или количества.</summary>
    public event Action Changed;

    private void Awake()
    {
        ResetToStart();
    }

    public void ResetToStart()
    {
        Selected = startItem;
        Seeds = Mathf.Max(0, startSeeds);
        Carrots = Mathf.Max(0, startCarrots);
        Water = Mathf.Clamp(startWater, 0, maxWater);
        Changed?.Invoke();
    }

    /// <summary>Выбрать предмет. Повторный выбор того же ничего не делает. Ничего не расходует.</summary>
    public bool Select(ItemType item)
    {
        if (item == Selected)
            return false;

        Selected = item;
        Changed?.Invoke();
        return true;
    }

    // ----- Методы для следующих этапов (посадка, полив, сбор) -----

    public void AddSeeds(int amount)
    {
        if (amount <= 0) return;
        Seeds += amount;
        Changed?.Invoke();
    }

    public bool TrySpendSeed()
    {
        if (Seeds <= 0) return false;
        Seeds--;
        Changed?.Invoke();
        return true;
    }

    public bool TrySpendWater()
    {
        if (Water <= 0) return false;
        Water--;
        Changed?.Invoke();
        return true;
    }

    public void RefillWater()
    {
        Water = maxWater;
        Changed?.Invoke();
    }

    public void AddCarrot()
    {
        Carrots++;
        Changed?.Invoke();
    }

    // ----- Отладка: правый клик на заголовке компонента в Play Mode -----

    [ContextMenu("Debug: семена = 0")]
    private void DebugSeedsZero() { Seeds = 0; Changed?.Invoke(); }

    [ContextMenu("Debug: семена = 5")]
    private void DebugSeedsFive() { Seeds = 5; Changed?.Invoke(); }

    [ContextMenu("Debug: вода = 0")]
    private void DebugWaterZero() { Water = 0; Changed?.Invoke(); }

    [ContextMenu("Debug: вода = максимум")]
    private void DebugWaterFull() { Water = maxWater; Changed?.Invoke(); }

    [ContextMenu("Debug: морковь = 0")]
    private void DebugCarrotsZero() { Carrots = 0; Changed?.Invoke(); }

    [ContextMenu("Debug: морковь + 1")]
    private void DebugCarrotPlusOne() { Carrots++; Changed?.Invoke(); }
}
