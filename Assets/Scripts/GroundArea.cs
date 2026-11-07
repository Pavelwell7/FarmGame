using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Область обычной проходимой земли (например, объект Dirt). Нужна для полива пустой земли:
/// стена, забор и пустота за краем карты не являются целью полива.
/// Область задаётся рамкой картинки (Sprite Renderer) этого объекта.
/// </summary>
public class GroundArea : MonoBehaviour
{
    private static readonly List<GroundArea> All = new List<GroundArea>();

    [SerializeField] private Renderer areaRenderer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        All.Clear();
    }

    private void Reset()
    {
        areaRenderer = GetComponent<Renderer>();
    }

    private void Awake()
    {
        if (areaRenderer == null)
            areaRenderer = GetComponent<Renderer>();

        if (areaRenderer == null)
        {
            Debug.LogError("GroundArea: на объекте нет Sprite Renderer (или другого Renderer).", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (!All.Contains(this))
            All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    private bool Contains(Vector2 point)
    {
        Bounds b = areaRenderer.bounds;
        return point.x >= b.min.x && point.x <= b.max.x
            && point.y >= b.min.y && point.y <= b.max.y;
    }

    /// <summary>Найти область земли, в которую попадает точка (или null).</summary>
    public static GroundArea Find(Vector2 point)
    {
        foreach (GroundArea area in All)
        {
            if (area != null && area.areaRenderer != null && area.Contains(point))
                return area;
        }

        return null;
    }
}

/// <summary>
/// Цель "обычная земля вне грядок". Единственное действие: полив лейкой (расходует воду).
/// Подсветки и текста у неё нет (по ТЗ).
/// </summary>
public class GroundTarget : IInteractable
{
    public Vector2Int Cell { get; private set; }
    public Transform Root { get; private set; }

    public void Set(Vector2Int cell, Transform root)
    {
        Cell = cell;
        Root = root;
    }

    public bool ShowsHighlight(InventoryModel inventory)
    {
        return false;
    }

    public string GetPrompt(InventoryModel inventory)
    {
        return null;
    }

    public string Interact(InventoryModel inventory)
    {
        if (inventory.Selected != ItemType.WateringCan)
            return null;

        return inventory.TrySpendWater() ? null : "Лейка пуста";
    }
}
