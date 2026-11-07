using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Подсказка "E - ..." над панелью предметов и короткие сообщения ("Нет семян" и т.п.).
/// Сообщение видно 2 секунды, новое заменяет старое. Интерфейс строится кодом.
/// </summary>
public class PromptHud : MonoBehaviour
{
    [SerializeField] private float messageSeconds = 2f;

    private Text promptText;
    private Text messageText;
    private Text dayText;
    private float messageTimer;

    private void Awake()
    {
        BuildUi();
    }

    private void Update()
    {
        if (messageTimer <= 0f)
            return;

        messageTimer -= Time.deltaTime;
        if (messageTimer <= 0f)
            messageText.gameObject.SetActive(false);
    }

    /// <summary>Показать номер дня в левом верхнем углу.</summary>
    public void SetDay(int day)
    {
        dayText.text = "День " + day;
    }

    /// <summary>Показать подсказку действия. null или пустая строка скрывает её.</summary>
    public void SetPrompt(string text)
    {
        string value = text ?? "";

        if (promptText.text != value)
            promptText.text = value;

        bool visible = value.Length > 0;
        if (promptText.gameObject.activeSelf != visible)
            promptText.gameObject.SetActive(visible);
    }

    /// <summary>Показать сообщение на 2 секунды. Не блокирует управление.</summary>
    public void ShowMessage(string text)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);
        messageTimer = messageSeconds;
    }

    private void BuildUi()
    {
        var canvasObject = new GameObject("PromptCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        promptText = CreateText("Prompt", canvasObject.transform, 30, Color.white, 170f);
        messageText = CreateText("Message", canvasObject.transform, 30, new Color(1f, 0.85f, 0.3f), 225f);

        promptText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);

        dayText = CreateText("Day", canvasObject.transform, 40, Color.white, 0f);
        dayText.text = "День 1";
        dayText.alignment = TextAnchor.MiddleLeft;

        var dayRect = dayText.rectTransform;
        dayRect.anchorMin = new Vector2(0f, 1f);
        dayRect.anchorMax = new Vector2(0f, 1f);
        dayRect.pivot = new Vector2(0f, 1f);
        dayRect.sizeDelta = new Vector2(500f, 60f);
        dayRect.anchoredPosition = new Vector2(30f, -24f);
    }

    private static Text CreateText(string name, Transform parent, int fontSize, Color color, float y)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var outline = go.GetComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);

        var rt = text.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(900f, 50f);
        rt.anchoredPosition = new Vector2(0f, y);
        return text;
    }
}