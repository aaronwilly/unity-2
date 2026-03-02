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
    private CanvasGroup _transitionCanvasGroup;
    private GameObject _canvasGo;
    private GameObject _startBgGo;
    private const int RefWidth = 1080;
    private const int RefHeight = 1920;
    private const float FadeOutDuration = 0.5f;
    private const float TransitionFadeInDuration = 0.25f;
    private const float TransitionHoldDuration = 0.4f;
    private const float TransitionFadeOutDuration = 0.35f;
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

    private static Sprite LoadSpriteFromResources(string path)
    {
        var tex = Resources.Load<Texture2D>(path);
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    // Load from Assets/Resources/Images/StartBackground.png
    private static Sprite GetStartBackgroundSprite()
    {
        Sprite s = Resources.Load<Sprite>("Images/StartBackground");
        if (s != null) return s;
        s = Resources.Load<Sprite>("StartBackground");
        if (s != null) return s;
        Texture2D tex = Resources.Load<Texture2D>("Images/StartBackground");
        if (tex == null) tex = Resources.Load<Texture2D>("StartBackground");
        if (tex != null) return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return null;
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

        // Full-screen background: StartBackground.png behind title and Start button
        _startBgGo = new GameObject("StartBackground");
        _startBgGo.transform.SetParent(root, false);
        var bgRect = _startBgGo.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = _startBgGo.AddComponent<Image>();
        Sprite bgSprite = GetStartBackgroundSprite();
        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.color = Color.white;
            bgImage.type = Image.Type.Simple;
            bgImage.preserveAspect = false;
        }
        else
        {
            bgImage.color = new Color(1f, 1f, 1f, 0f);
        }
        bgImage.raycastTarget = false;

        var panelGo = new GameObject("StartPanel");
        panelGo.transform.SetParent(root, false);
        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(1f, 1f, 1f, 0f);
        panelImg.raycastTarget = true;
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
        titleText.text = "RAID RUSH";
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

        CreateTransitionOverlay(root);
    }

    private void CreateTransitionOverlay(Transform root)
    {
        var transGo = new GameObject("TransitionOverlay");
        transGo.transform.SetParent(root, false);
        var transRect = transGo.AddComponent<RectTransform>();
        transRect.anchorMin = Vector2.zero;
        transRect.anchorMax = Vector2.one;
        transRect.offsetMin = Vector2.zero;
        transRect.offsetMax = Vector2.zero;
        var transImg = transGo.AddComponent<Image>();
        Sprite transSprite = LoadSpriteFromResources("Images/Transition");
        if (transSprite != null)
        {
            transImg.sprite = transSprite;
            transImg.color = Color.white;
            transImg.type = Image.Type.Simple;
            transImg.preserveAspect = false;
        }
        else
            transImg.color = new Color(0.1f, 0.06f, 0.02f, 1f);
        transImg.raycastTarget = false;
        _transitionCanvasGroup = transGo.AddComponent<CanvasGroup>();
        _transitionCanvasGroup.alpha = 0f;
        _transitionCanvasGroup.blocksRaycasts = false;
        transGo.SetActive(true);
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

        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.gameObject.SetActive(true);
            elapsed = 0f;
            while (elapsed < TransitionFadeInDuration)
            {
                elapsed += Time.deltaTime;
                _transitionCanvasGroup.alpha = Mathf.Clamp01(elapsed / TransitionFadeInDuration);
                yield return null;
            }
            _transitionCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(TransitionHoldDuration);
            elapsed = 0f;
            while (elapsed < TransitionFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                _transitionCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / TransitionFadeOutDuration);
                yield return null;
            }
            _transitionCanvasGroup.alpha = 0f;
        }

        onStartPressed?.Invoke();
        if (_canvasGo != null)
            _canvasGo.SetActive(false);
    }
}
