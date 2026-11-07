using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Окно «Закончить день?» с кнопками «Да» и «Нет». Строится кодом.
/// Кнопки нажимаются только мышью. Пока окно открыто, ввод в мире заблокирован (InputGate).
/// Кнопки обрабатываются вручную (без EventSystem), поэтому ничего настраивать не нужно.
/// </summary>
public class SleepDialog : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;

    private GameObject canvasObject;
    private RectTransform yesRect;
    private RectTransform noRect;
    private Image yesImage;
    private Image noImage;
    private bool isOpen;

    private static readonly Color YesColor = new Color(0.25f, 0.60f, 0.30f);
    private static readonly Color YesHover = new Color(0.35f, 0.75f, 0.40f);
    private static readonly Color NoColor = new Color(0.55f, 0.25f, 0.25f);
    private static readonly Color NoHover = new Color(0.75f, 0.35f, 0.35f);

    private void Awake()
    {
        if (dayManager == null)
            dayManager = GetComponent<DayManager>();

        if (dayManager == null)
            Debug.LogError("SleepDialog: не найден Day Manager.", this);

        BuildUi();
        canvasObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Если окно открыто, а объект выключили: не оставляем ввод заблокированным.
        if (isOpen)
        {
            isOpen = false;
            InputGate.Unblock();
        }
    }

    /// <summary>Открыть окно. Повторное открытие игнорируется.</summary>
    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;
        canvasObject.SetActive(true);
        InputGate.Block();
    }

    private void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        canvasObject.SetActive(false);
        InputGate.Unblock();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        Vector2 mouse = GameInput.MouseScreenPosition();
        bool overYes = RectTransformUtility.RectangleContainsScreenPoint(yesRect, mouse, null);
        bool overNo = RectTransformUtility.RectangleContainsScreenPoint(noRect, mouse, null);

        yesImage.color = overYes ? YesHover : YesColor;
        noImage.color = overNo ? NoHover : NoColor;

        if (!GameInput.LeftClickPressedThisFrame())
            return;

        if (overYes)
            Confirm();
        else if (overNo)
            Close();
    }

    private void Confirm()
    {
        // Окно закрывается ДО перехода дня: второй щелчок по "Да" уже ничего не подтвердит.
        Close();

        if (dayManager != null)
            dayManager.SleepNow();
    }

    private void BuildUi()
    {
        canvasObject = new GameObject("SleepCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Затемнение на весь экран.
        var overlay = CreateBox("Overlay", canvasObject.transform, new Color(0f, 0f, 0f, 0.55f), Vector2.zero, Vector2.zero);
        overlay.rectTransform.anchorMin = Vector2.zero;
        overlay.rectTransform.anchorMax = Vector2.one;
        overlay.rectTransform.offsetMin = Vector2.zero;
        overlay.rectTransform.offsetMax = Vector2.zero;

        var panel = CreateBox("Panel", canvasObject.transform, new Color(0.12f, 0.12f, 0.14f, 0.97f),
            new Vector2(600f, 300f), Vector2.zero);

        CreateLabel("Title", panel.transform, "Закончить день?", 46, new Vector2(0f, 70f), new Vector2(560f, 80f));

        yesImage = CreateBox("YesButton", panel.transform, YesColor, new Vector2(220f, 80f), new Vector2(-130f, -65f));
        yesRect = yesImage.rectTransform;
        CreateLabel("Label", yesImage.transform, "Да", 38, Vector2.zero, new Vector2(220f, 80f));

        noImage = CreateBox("NoButton", panel.transform, NoColor, new Vector2(220f, 80f), new Vector2(130f, -65f));
        noRect = noImage.rectTransform;
        CreateLabel("Label", noImage.transform, "Нет", 38, Vector2.zero, new Vector2(220f, 80f));
    }

    private static Image CreateBox(string name, Transform parent, Color color, Vector2 size, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = color;

        var rt = image.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        return image;
    }

    private static void CreateLabel(string name, Transform parent, string content, int fontSize, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = content;

        var rt = text.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
    }
}
