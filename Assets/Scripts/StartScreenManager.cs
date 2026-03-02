using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Start screen: full-screen panel with Start button. Fades out via CanvasGroup when pressed.
/// All UI created in code; no scene switching; no inspector references.
/// </summary>
public class StartScreenManager : MonoBehaviour
{
    private CanvasGroup _panelCanvasGroup;
    private GameObject _canvasGo;
    private const int RefWidth = 1080;
    private const int RefHeight = 1920;
    private const float FadeOutDuration = 0.5f;
    private static Font _font;

    public void Initialize(Action onStartPressed)
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Exclude) == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }
        BuildStartScreen(onStartPressed);
    }

    private static Font GetDefaultFont()
    {
        if (_font == null)
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return _font;
    }

    private void BuildStartScreen(Action onStartPressed)
    {
        _canvasGo = new GameObject("StartScreenCanvas");
        var canvas = _canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = _canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
        scaler.matchWidthOrHeight = 0.5f;
        _canvasGo.AddComponent<GraphicRaycaster>();

        var root = _canvasGo.transform;

        var panelGo = new GameObject("StartPanel");
        panelGo.transform.SetParent(root, false);
        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.06f, 0.12f, 1f);
        _panelCanvasGroup = panelGo.AddComponent<CanvasGroup>();
        _panelCanvasGroup.alpha = 1f;
        _panelCanvasGroup.blocksRaycasts = true;
        _panelCanvasGroup.interactable = true;

        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(panelGo.transform, false);
        var titleRect = titleGo.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.65f);
        titleRect.anchorMax = new Vector2(0.5f, 0.65f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(500, 80);
        var titleText = titleGo.AddComponent<Text>();
        titleText.text = "BATTLE";
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 0.95f, 1f);
        if (GetDefaultFont() != null) titleText.font = GetDefaultFont();

        var btnGo = new GameObject("StartButton");
        btnGo.transform.SetParent(panelGo.transform, false);
        var btnRect = btnGo.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.4f);
        btnRect.anchorMax = new Vector2(0.5f, 0.4f);
        btnRect.anchoredPosition = Vector2.zero;
        btnRect.sizeDelta = new Vector2(280, 70);
        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.5f, 0.35f, 1f);
        var btn = btnGo.AddComponent<Button>();
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var btnText = textGo.AddComponent<Text>();
        btnText.text = "Start";
        btnText.fontSize = 32;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        if (GetDefaultFont() != null) btnText.font = GetDefaultFont();
        btn.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            if (_panelCanvasGroup == null) return;
            btn.interactable = false;
            StartCoroutine(FadeOutAndNotify(onStartPressed));
        });
    }

    private IEnumerator FadeOutAndNotify(Action onStartPressed)
    {
        float elapsed = 0f;
        while (elapsed < FadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / FadeOutDuration);
            if (_panelCanvasGroup != null)
                _panelCanvasGroup.alpha = 1f - t;
            yield return null;
        }
        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.alpha = 0f;
            _panelCanvasGroup.blocksRaycasts = false;
            _panelCanvasGroup.interactable = false;
        }
        onStartPressed?.Invoke();
        if (_canvasGo != null)
            _canvasGo.SetActive(false);
    }
}
