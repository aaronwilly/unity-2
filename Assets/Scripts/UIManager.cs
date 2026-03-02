using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds all battle UI in code: Canvas, background, HP/SP bars, turn text, ability buttons, capture button.
/// No inspector references; receives BattleManager and wires buttons in code.
/// </summary>
public class UIManager : MonoBehaviour
{
    private BattleManager _battleManager;
    private TurnManager _turnManager;

    private RectTransform _backgroundRect;
    private Image _backgroundImage;
    private RectTransform[] _parallaxLayers;
    private RectTransform[] _depthOrbs;
    private Image[] _depthOrbImages;
    private RectTransform _enemyPanelRect;
    private RectTransform _ally1PanelRect;
    private RectTransform _ally2PanelRect;

    private Text _turnText;
    private GameObject _p1HpBarFill;
    private GameObject _p1SpBarFill;
    private Text _p1Label;
    private GameObject _p2HpBarFill;
    private GameObject _p2SpBarFill;
    private Text _p2Label;
    private GameObject _enemyHpBarFill;
    private GameObject _enemySpBarFill;
    private Text _enemyLabel;
    private Button _attackBtn;
    private Button _defendBtn;
    private Button _specialBtn;
    private Button _captureBtn;
    private GameObject _captureButtonObj;

    private const int RefWidth = 1080;
    private const int RefHeight = 1920;

    // Portrait layout (reference resolution 1080x1920)
    private const float EnemyPanelWidth = 320f;
    private const float EnemyPanelHeight = 200f;
    private const float AllyPanelWidth = 320f;
    private const float AllyPanelHeight = 220f;
    private const float MarginSide = 80f;
    private const float MarginBottom = 140f;
    private const float EnemyTopOffset = 180f;
    private const float ParallaxAmplitude = 12f;
    private const float BackgroundColorSpeed = 0.4f;
    private const int ParallaxLayerCount = 4;
    private const int DepthOrbCount = 6;

    private static Font _defaultFont;

    private static Font GetDefaultFont()
    {
        if (_defaultFont == null)
            _defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return _defaultFont;
    }

    public void Initialize(BattleManager battleManager, TurnManager turnManager)
    {
        _battleManager = battleManager;
        _turnManager = turnManager;
        if (_battleManager != null)
            _battleManager.OnUnitStatsChanged += RefreshAll;
        EnsureEventSystem();
        BuildCanvasAndUI();
        SubscribeToTurn();
        RefreshAll();
        StartCoroutine(RunSpawnAnimation());
    }

    private void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Exclude) == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    /// <summary>Creates a 3D-style battle background: base gradient, multiple parallax layers, and floating depth orbs. All via code.</summary>
    private void CreateBattleBackground(Transform root)
    {
        var bgRoot = new GameObject("BattleBackground");
        bgRoot.transform.SetParent(root, false);
        _backgroundRect = bgRoot.AddComponent<RectTransform>();
        _backgroundRect.anchorMin = Vector2.zero;
        _backgroundRect.anchorMax = Vector2.one;
        _backgroundRect.offsetMin = Vector2.zero;
        _backgroundRect.offsetMax = Vector2.zero;
        _backgroundImage = bgRoot.AddComponent<Image>();
        _backgroundImage.color = new Color(0.06f, 0.04f, 0.12f, 1f);
        _backgroundImage.raycastTarget = false;

        // Simulated gradient: top (darker) and bottom (lighter) bands for depth
        var topBand = CreateBgLayer(bgRoot.transform, "TopBand", new Color(0.08f, 0.05f, 0.15f, 0.9f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(RefWidth + 200, RefHeight * 0.6f));
        var bottomBand = CreateBgLayer(bgRoot.transform, "BottomBand", new Color(0.12f, 0.08f, 0.2f, 0.85f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 0), new Vector2(RefWidth + 200, RefHeight * 0.5f));

        // Multiple parallax layers (back to front) for 3D depth
        _parallaxLayers = new RectTransform[ParallaxLayerCount];
        var layerColors = new[]
        {
            new Color(0.15f, 0.1f, 0.28f, 0.35f),
            new Color(0.18f, 0.12f, 0.32f, 0.3f),
            new Color(0.12f, 0.08f, 0.22f, 0.4f),
            new Color(0.08f, 0.06f, 0.15f, 0.5f)
        };
        for (int i = 0; i < ParallaxLayerCount; i++)
        {
            var layer = CreateBgLayer(bgRoot.transform, "ParallaxLayer" + i, layerColors[i],
                Vector2.zero, Vector2.one, Vector2.zero, new Vector2(RefWidth + 400, RefHeight + 200));
            _parallaxLayers[i] = (RectTransform)layer;
        }

        // Floating "depth orbs" for 3D atmosphere (soft glow quads)
        _depthOrbs = new RectTransform[DepthOrbCount];
        _depthOrbImages = new Image[DepthOrbCount];
        float[] orbX = { 0.15f, 0.45f, 0.75f, 0.25f, 0.6f, 0.85f };
        float[] orbY = { 0.2f, 0.35f, 0.25f, 0.65f, 0.55f, 0.7f };
        int[] orbSize = { 180, 120, 200, 100, 160, 140 };
        for (int i = 0; i < DepthOrbCount; i++)
        {
            var orb = new GameObject("DepthOrb" + i);
            orb.transform.SetParent(bgRoot.transform, false);
            var rect = orb.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(orbX[i], orbY[i]);
            rect.anchorMax = new Vector2(orbX[i], orbY[i]);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(orbSize[i], orbSize[i]);
            var img = orb.AddComponent<Image>();
            img.color = new Color(0.35f, 0.2f, 0.5f, 0.12f);
            img.raycastTarget = false;
            _depthOrbs[i] = rect;
            _depthOrbImages[i] = img;
        }
    }

    private Transform CreateBgLayer(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go.transform;
    }

    private void UpdateBackgroundVisuals()
    {
        float time = Time.time;

        // Base color slow pulse
        if (_backgroundImage != null)
        {
            float t = Mathf.Sin(time * BackgroundColorSpeed) * 0.5f + 0.5f;
            _backgroundImage.color = Color.Lerp(
                new Color(0.05f, 0.03f, 0.1f, 1f),
                new Color(0.08f, 0.06f, 0.14f, 1f),
                t);
        }

        // Multi-layer parallax: each layer moves at different speed/phase/amplitude for 3D effect
        if (_parallaxLayers != null)
        {
            float[] speeds = { 0.3f, 0.5f, 0.7f, 0.45f };
            float[] phases = { 0f, 1.2f, 2.5f, 0.8f };
            float[] ampY = { 8f, 14f, 20f, 11f };
            float[] ampX = { 3f, 6f, 4f, 5f };
            for (int i = 0; i < _parallaxLayers.Length && i < speeds.Length; i++)
            {
                if (_parallaxLayers[i] == null) continue;
                float y = Mathf.Sin(time * speeds[i] + phases[i]) * ampY[i];
                float x = Mathf.Sin(time * speeds[i] * 0.7f + phases[i] * 1.3f) * ampX[i];
                _parallaxLayers[i].anchoredPosition = new Vector2(x, y);
            }
        }

        // Depth orbs: gentle float + opacity pulse
        if (_depthOrbs != null && _depthOrbImages != null)
        {
            float[] orbSpeeds = { 0.4f, 0.6f, 0.35f, 0.55f, 0.45f, 0.5f };
            float[] orbPhases = { 0f, 2f, 1f, 3f, 1.5f, 2.5f };
            float[] orbAmp = { 15f, 25f, 20f, 18f, 22f, 12f };
            for (int i = 0; i < _depthOrbs.Length && i < orbSpeeds.Length; i++)
            {
                if (_depthOrbs[i] != null)
                {
                    float y = Mathf.Sin(time * orbSpeeds[i] + orbPhases[i]) * orbAmp[i];
                    float x = Mathf.Sin(time * orbSpeeds[i] * 0.6f + orbPhases[i] * 0.9f) * (orbAmp[i] * 0.4f);
                    _depthOrbs[i].anchoredPosition = new Vector2(x, y);
                    float scale = 0.92f + Mathf.Sin(time * 0.5f + i) * 0.08f;
                    _depthOrbs[i].localScale = Vector3.one * scale;
                }
                if (_depthOrbImages != null && i < _depthOrbImages.Length && _depthOrbImages[i] != null)
                {
                    float a = 0.08f + Mathf.Sin(time * 0.4f + i * 0.7f) * 0.04f;
                    var c = _depthOrbImages[i].color;
                    _depthOrbImages[i].color = new Color(c.r, c.g, c.b, Mathf.Clamp01(a));
                }
            }
        }
    }

    /// <summary>Scale-in animation for unit panels. Keeps everything code-driven.</summary>
    private IEnumerator RunSpawnAnimation()
    {
        const float duration = 0.35f;
        const float startScale = 0.3f;
        RectTransform[] panels = { _enemyPanelRect, _ally1PanelRect, _ally2PanelRect };
        foreach (var rect in panels)
        {
            if (rect != null) rect.localScale = Vector3.one * startScale;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = 1f - (1f - t) * (1f - t);
            float s = Mathf.Lerp(startScale, 1f, t);
            foreach (var rect in panels)
            {
                if (rect != null) rect.localScale = Vector3.one * s;
            }
            yield return null;
        }
        foreach (var rect in panels)
        {
            if (rect != null) rect.localScale = Vector3.one;
        }
    }

    private void BuildCanvasAndUI()
    {
        var canvasGo = new GameObject("BattleCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(RefWidth, RefHeight);
        canvasGo.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var root = canvasGo.transform;

        CreateBattleBackground(root);

        // Enemy: top center (portrait layout)
        var enemyPanel = CreatePanel(root, "EnemyPanel",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -EnemyTopOffset), new Vector2(EnemyPanelWidth, EnemyPanelHeight));
        _enemyPanelRect = (RectTransform)enemyPanel;
        _enemyPanelRect.pivot = new Vector2(0.5f, 1f);
        _enemyLabel = CreateText(enemyPanel, "EnemyLabel", "Enemy Lv1", 22,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(280, 56));
        _enemyHpBarFill = CreateFilledBar(enemyPanel, "EnemyHP", Color.red, new Vector2(0.5f, 1f), new Vector2(0, -95), new Vector2(280, 24));
        _enemySpBarFill = CreateFilledBar(enemyPanel, "EnemySP", Color.cyan, new Vector2(0.5f, 1f), new Vector2(0, -135), new Vector2(280, 16));

        // Ally 1: bottom left
        var ally1Panel = CreatePanel(root, "Ally1Panel",
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(MarginSide, MarginBottom), new Vector2(AllyPanelWidth, AllyPanelHeight));
        _ally1PanelRect = (RectTransform)ally1Panel;
        _ally1PanelRect.pivot = new Vector2(0f, 0f);
        _p1Label = CreateText(ally1Panel, "P1Label", "P1 Lv1", 22,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(280, 56));
        _p1HpBarFill = CreateFilledBar(ally1Panel, "P1HP", Color.green, new Vector2(0.5f, 1f), new Vector2(0, -95), new Vector2(280, 24));
        _p1SpBarFill = CreateFilledBar(ally1Panel, "P1SP", Color.blue, new Vector2(0.5f, 1f), new Vector2(0, -135), new Vector2(280, 16));

        // Ally 2: bottom right
        var ally2Panel = CreatePanel(root, "Ally2Panel",
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-MarginSide, MarginBottom), new Vector2(AllyPanelWidth, AllyPanelHeight));
        _ally2PanelRect = (RectTransform)ally2Panel;
        _ally2PanelRect.pivot = new Vector2(1f, 0f);
        _p2Label = CreateText(ally2Panel, "P2Label", "P2 Lv1", 22,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(280, 56));
        _p2HpBarFill = CreateFilledBar(ally2Panel, "P2HP", Color.green, new Vector2(0.5f, 1f), new Vector2(0, -95), new Vector2(280, 24));
        _p2SpBarFill = CreateFilledBar(ally2Panel, "P2SP", Color.blue, new Vector2(0.5f, 1f), new Vector2(0, -135), new Vector2(280, 16));

        _turnText = CreateText(root, "TurnLabel", "Turn: Player 1", 36,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(400, 80));

        var abilityPanel = CreatePanel(root, "AbilityPanel",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 120), new Vector2(RefWidth - 80, 100));
        _attackBtn = CreateButton(abilityPanel, "Attack", "Attack", () => OnAbilityClicked(Ability.BasicAttack));
        SetRect(_attackBtn.transform as RectTransform, new Vector2(0.2f, 0.5f), new Vector2(160, 70));
        _defendBtn = CreateButton(abilityPanel, "Defend", "Defend", () => OnAbilityClicked(Ability.DefensiveMove));
        SetRect(_defendBtn.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(160, 70));
        _specialBtn = CreateButton(abilityPanel, "Special", "Special", () => OnAbilityClicked(Ability.SpecialSkill));
        SetRect(_specialBtn.transform as RectTransform, new Vector2(0.8f, 0.5f), new Vector2(160, 70));

        _captureButtonObj = new GameObject("CaptureButton");
        _captureButtonObj.transform.SetParent(root, false);
        var capRect = _captureButtonObj.AddComponent<RectTransform>();
        capRect.anchorMin = new Vector2(0.5f, 0f);
        capRect.anchorMax = new Vector2(0.5f, 0f);
        capRect.anchoredPosition = new Vector2(0, 40);
        capRect.sizeDelta = new Vector2(200, 50);
        _captureBtn = _captureButtonObj.AddComponent<Button>();
        var capImg = _captureButtonObj.AddComponent<Image>();
        capImg.color = new Color(0.8f, 0.4f, 0.2f);
        var capTextGo = new GameObject("Text");
        capTextGo.transform.SetParent(_captureButtonObj.transform, false);
        var capText = capTextGo.AddComponent<Text>();
        capText.text = "Capture";
        capText.fontSize = 24;
        capText.alignment = TextAnchor.MiddleCenter;
        capText.color = Color.white;
        if (GetDefaultFont() != null) capText.font = GetDefaultFont();
        var capTextRect = (RectTransform)capTextGo.transform;
        capTextRect.anchorMin = Vector2.zero;
        capTextRect.anchorMax = Vector2.one;
        capTextRect.offsetMin = Vector2.zero;
        capTextRect.offsetMax = Vector2.zero;
        _captureBtn.onClick.AddListener(OnCaptureClicked);
        _captureButtonObj.SetActive(false);

        const float spawnStartScale = 0.3f;
        if (_enemyPanelRect != null) _enemyPanelRect.localScale = Vector3.one * spawnStartScale;
        if (_ally1PanelRect != null) _ally1PanelRect.localScale = Vector3.one * spawnStartScale;
        if (_ally2PanelRect != null) _ally2PanelRect.localScale = Vector3.one * spawnStartScale;
    }

    private Transform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        return go.transform;
    }

    private Text CreateText(Transform parent, string name, string content, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        if (GetDefaultFont() != null) text.font = GetDefaultFont();
        return text;
    }

    /// <summary>Creates a bar: grey background (full width) + colored fill (child, width updated by ApplyBarFill). Returns the fill GameObject.</summary>
    private GameObject CreateFilledBar(Transform parent, string name, Color color, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var bg = new GameObject(name + "Bg");
        bg.transform.SetParent(parent, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = anchor;
        bgRect.anchorMax = anchor;
        bgRect.anchoredPosition = pos;
        bgRect.sizeDelta = size;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);

        var fill = new GameObject(name);
        fill.transform.SetParent(bg.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = color;
        fillImg.raycastTarget = false;
        var fillRect = (RectTransform)fill.transform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        return fill;
    }

    /// <summary>Set bar to show amount (0-1): fill width = amount, rest is grey background. HP bars get color by amount (green/yellow/red).</summary>
    private static void ApplyBarFill(GameObject fillGo, float amount, bool useHpColor, Color? spColor = null)
    {
        if (fillGo == null) return;
        float ratio = Mathf.Clamp01(amount);
        var rect = fillGo.transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(ratio, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        var img = fillGo.GetComponent<Image>();
        if (img == null) return;
        if (useHpColor)
        {
            if (ratio > 0.5f) img.color = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f);
            else if (ratio > 0.25f) img.color = Color.Lerp(Color.red, Color.yellow, (ratio - 0.25f) * 4f);
            else img.color = Color.red;
        }
        else if (spColor.HasValue)
            img.color = spColor.Value;
    }

    private Button CreateButton(Transform parent, string name, string label, Action onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.5f, 0.3f);
        var btn = go.AddComponent<Button>();
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.AddComponent<Text>();
        text.text = label;
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        if (GetDefaultFont() != null) text.font = GetDefaultFont();
        var textRect = (RectTransform)textGo.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        btn.onClick.AddListener(() => onClick?.Invoke());
        return btn;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorPos, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = anchorPos;
        rect.anchorMax = anchorPos;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private void SubscribeToTurn()
    {
        if (_turnManager != null)
            _turnManager.OnTurnChanged += _ => { RefreshAll(); };
    }

    private void OnAbilityClicked(Ability ability)
    {
        if (_battleManager == null || _turnManager == null) return;
        if (!_turnManager.IsPlayerTurn) return;

        Unit caster = _battleManager.GetCurrentUnit();
        Unit target = _battleManager.Enemy;
        if (ability.Id == Ability.DefensiveMove.Id)
            target = caster;
        _battleManager.ExecuteAbility(ability, caster, target);
        RefreshAll();
        UpdateCaptureButtonVisibility();
    }

    private void OnCaptureClicked()
    {
        _battleManager?.TryCapture();
        RefreshAll();
        UpdateCaptureButtonVisibility();
    }

    private void Update()
    {
        UpdateBackgroundVisuals();
        RefreshAll();
        UpdateCaptureButtonVisibility();
    }

    private void UpdateCaptureButtonVisibility()
    {
        if (_captureButtonObj != null && _battleManager != null)
            _captureButtonObj.SetActive(_battleManager.CanCapture());
    }

    private void RefreshAll()
    {
        if (_battleManager == null) return;

        int idx = _turnManager.GetCurrentTurnIndex();
        string turnName = idx == 0 ? "Player 1" : idx == 1 ? "Player 2" : "Enemy";
        if (_turnText != null) _turnText.text = "Turn: " + turnName;

        var p1 = _battleManager.Player1;
        var p2 = _battleManager.Player2;
        var en = _battleManager.Enemy;

        ApplyBarFill(_p1HpBarFill, p1.HPRatio, true, null);
        ApplyBarFill(_p1SpBarFill, p1.SPRatio, false, Color.blue);
        if (_p1Label != null) _p1Label.text = p1.Name + " Lv" + p1.Level + "\nHP " + p1.HP + "/" + p1.MaxHP + "  SP " + p1.SP + "/" + p1.MaxSP;

        ApplyBarFill(_p2HpBarFill, p2.HPRatio, true, null);
        ApplyBarFill(_p2SpBarFill, p2.SPRatio, false, Color.blue);
        if (_p2Label != null) _p2Label.text = p2.Name + " Lv" + p2.Level + "\nHP " + p2.HP + "/" + p2.MaxHP + "  SP " + p2.SP + "/" + p2.MaxSP;

        ApplyBarFill(_enemyHpBarFill, en.HPRatio, true, null);
        ApplyBarFill(_enemySpBarFill, en.SPRatio, false, Color.cyan);
        if (_enemyLabel != null) _enemyLabel.text = en.Name + " Lv" + en.Level + "\nHP " + en.HP + "/" + en.MaxHP + "  SP " + en.SP + "/" + en.MaxSP;

        bool playerTurn = _turnManager.IsPlayerTurn;
        Unit current = _battleManager.GetCurrentUnit();
        bool canAffordSpecial = current != null && current.SP >= Ability.SpecialSkill.SPCost;
        if (_attackBtn != null) _attackBtn.interactable = playerTurn;
        if (_defendBtn != null) _defendBtn.interactable = playerTurn;
        if (_specialBtn != null) _specialBtn.interactable = playerTurn && canAffordSpecial;
    }
}
