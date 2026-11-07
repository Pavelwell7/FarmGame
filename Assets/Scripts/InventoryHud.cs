using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Панель из 5 ячеек внизу экрана. Весь интерфейс создаётся кодом при запуске,
/// настраивать Canvas вручную не нужно. Только читает данные из InventoryModel.
/// Пока вместо иконок используются цветные прямоугольники и названия (разрешено ТЗ).
/// </summary>
public class InventoryHud : MonoBehaviour
{
    [SerializeField] private InventoryModel inventory;
    [SerializeField] private float slotSize = 110f;
    [SerializeField] private float spacing = 12f;
    [SerializeField] private float bottomMargin = 24f;

    private class Slot
    {
        public ItemType item;
        public Color baseColor;
        public Image background;
        public GameObject frame;
        public Text count;
    }

    private Slot[] slots;
    private RectTransform panelRect;

    private static readonly string[] Names = { "Мотыга", "Семена", "Лейка", "Лопата", "Морковь" };

    private static readonly Color[] Colors =
    {
        new Color(0.55f, 0.35f, 0.20f), // мотыга
        new Color(0.45f, 0.70f, 0.25f), // семена
        new Color(0.25f, 0.50f, 0.85f), // лейка
        new Color(0.55f, 0.55f, 0.60f), // лопата
        new Color(0.95f, 0.55f, 0.10f)  // морковь
    };

    private void Awake()
    {
        BuildUi();
    }

    private void Start()
    {
        if (inventory == null)
        {
            Debug.LogError("InventoryHud: не назначен Inventory.", this);
            enabled = false;
            return;
        }

        inventory.Changed += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.Changed -= Refresh;
        PointerUi.Unregister(panelRect);
    }

    private void BuildUi()
    {
        var canvasObject = new GameObject("InventoryCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        int n = InventoryModel.Slots.Length;
        float totalWidth = n * slotSize + (n - 1) * spacing;

        var panel = new GameObject("Panel", typeof(RectTransform)).GetComponent<RectTransform>();
        panel.SetParent(canvasObject.transform, false);
        panel.anchorMin = new Vector2(0.5f, 0f);
        panel.anchorMax = new Vector2(0.5f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.sizeDelta = new Vector2(totalWidth, slotSize);
        panel.anchoredPosition = new Vector2(0f, bottomMargin);

        // Над панелью E не должна действовать на объекты мира.
        panelRect = panel;
        PointerUi.Register(panelRect);

        slots = new Slot[n];

        for (int i = 0; i < n; i++)
        {
            var slot = new Slot { item = InventoryModel.Slots[i], baseColor = Colors[i] };

            var root = new GameObject("Slot_" + Names[i], typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(panel, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.zero;
            root.pivot = Vector2.zero;
            root.sizeDelta = new Vector2(slotSize, slotSize);
            root.anchoredPosition = new Vector2(i * (slotSize + spacing), 0f);

            // Рамка выделения (рисуется первой, значит под фоном; торчит по краям на 6 пикселей).
            var frame = CreateImage("Frame", root, new Color(1f, 0.92f, 0.2f));
            frame.rectTransform.offsetMin = new Vector2(-6f, -6f);
            frame.rectTransform.offsetMax = new Vector2(6f, 6f);
            slot.frame = frame.gameObject;

            slot.background = CreateImage("Background", root, slot.baseColor);

            var title = CreateText("Title", root, 18, TextAnchor.UpperCenter);
            title.text = (i + 1) + ". " + Names[i];

            slot.count = CreateText("Count", root, 30, TextAnchor.LowerCenter);

            slots[i] = slot;
        }
    }

    private void Refresh()
    {
        foreach (var slot in slots)
        {
            slot.frame.SetActive(inventory.Selected == slot.item);

            switch (slot.item)
            {
                case ItemType.Seeds:
                    slot.count.text = inventory.Seeds.ToString();
                    break;
                case ItemType.WateringCan:
                    slot.count.text = inventory.Water + "/" + inventory.MaxWater;
                    break;
                case ItemType.Carrot:
                    slot.count.text = inventory.Carrots.ToString();
                    break;
                default:
                    slot.count.text = "";
                    break;
            }

            // Морковь = 0: ячейка остаётся, показывает 0, но выглядит "пустой".
            bool empty = slot.item == ItemType.Carrot && inventory.Carrots <= 0;
            slot.background.color = empty ? Color.Lerp(slot.baseColor, Color.black, 0.65f) : slot.baseColor;
        }
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = color;

        var rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return image;
    }

    private static Text CreateText(string name, Transform parent, int fontSize, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var rt = text.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(4f, 4f);
        rt.offsetMax = new Vector2(-4f, -4f);
        return text;
    }
}