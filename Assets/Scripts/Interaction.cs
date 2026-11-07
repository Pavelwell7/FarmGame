using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Всё, с чем игрок может взаимодействовать по E (грядка, потом колодец и кровать).
/// </summary>
public interface IInteractable
{
    /// <summary>Единственная клетка-цель этого объекта.</summary>
    Vector2Int Cell { get; }

    /// <summary>Корневой объект: его коллайдеры не считаются препятствием перед ним.</summary>
    Transform Root { get; }

    /// <summary>Нужна ли рамка подсветки при текущем предмете.</summary>
    bool ShowsHighlight(InventoryModel inventory);

    /// <summary>Текст "E - ..." или null, если действия нет.</summary>
    string GetPrompt(InventoryModel inventory);

    /// <summary>
    /// Выполнить одно действие. Если сочетание недопустимо, ничего не меняется.
    /// Возвращает сообщение для игрока (например, "Нет семян") или null.
    /// </summary>
    string Interact(InventoryModel inventory);
}

/// <summary>
/// Таблица "клетка -> объект". Один объект на клетку.
/// </summary>
public static class InteractableRegistry
{
    private static readonly Dictionary<Vector2Int, IInteractable> Map = new Dictionary<Vector2Int, IInteractable>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        Map.Clear();
    }

    public static void Register(IInteractable interactable)
    {
        IInteractable existing;
        if (Map.TryGetValue(interactable.Cell, out existing) && existing != interactable)
        {
            Debug.LogError("Две цели взаимодействия в одной клетке " + interactable.Cell +
                           ". Передвиньте один из объектов на свободную клетку.");
            return;
        }

        Map[interactable.Cell] = interactable;
    }

    public static void Unregister(IInteractable interactable)
    {
        IInteractable existing;
        if (Map.TryGetValue(interactable.Cell, out existing) && existing == interactable)
            Map.Remove(interactable.Cell);
    }

    public static IInteractable Get(Vector2Int cell)
    {
        IInteractable result;
        Map.TryGetValue(cell, out result);
        return result;
    }
}

/// <summary>
/// Области интерфейса, над которыми E не должна действовать на мир.
/// </summary>
public static class PointerUi
{
    private static readonly List<RectTransform> Rects = new List<RectTransform>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        Rects.Clear();
    }

    public static void Register(RectTransform rect)
    {
        if (rect != null && !Rects.Contains(rect))
            Rects.Add(rect);
    }

    public static void Unregister(RectTransform rect)
    {
        Rects.Remove(rect);
    }

    public static bool IsOver(Vector2 screenPosition)
    {
        foreach (RectTransform rect in Rects)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy)
                continue;

            // camera = null подходит для Canvas в режиме Screen Space - Overlay.
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, null))
                return true;
        }

        return false;
    }
}