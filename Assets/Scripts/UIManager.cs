using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds all battle UI in code: Canvas, HP/SP bars, turn text, ability buttons, capture button.
/// No inspector references; receives BattleManager and wires buttons in code.
/// </summary>
public class UIManager : MonoBehaviour
{
    private BattleManager _battleManager;
    private TurnManager _turnManager;

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

        _turnText = CreateText(root, "TurnLabel", "Turn: Player 1", 36,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(400, 80));

        var playerPanel = CreatePanel(root, "PlayerPanel",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(160, 0), new Vector2(320, 600));
        _p1Label = CreateText(playerPanel, "P1Label", "P1 Lv1", 22,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(280, 56));
        _p1HpBarFill = CreateFilledBar(playerPanel, "P1HP", Color.green, new Vector2(0.5f, 1f), new Vector2(0, -105), new Vector2(280, 24));
        _p1SpBarFill = CreateFilledBar(playerPanel, "P1SP", Color.blue, new Vector2(0.5f, 1f), new Vector2(0, -145), new Vector2(280, 16));
        _p2Label = CreateText(playerPanel, "P2Label", "P2 Lv1", 22,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -220), new Vector2(280, 56));
        _p2HpBarFill = CreateFilledBar(playerPanel, "P2HP", Color.green, new Vector2(0.5f, 1f), new Vector2(0, -285), new Vector2(280, 24));
        _p2SpBarFill = CreateFilledBar(playerPanel, "P2SP", Color.blue, new Vector2(0.5f, 1f), new Vector2(0, -325), new Vector2(280, 16));

        var enemyPanel = CreatePanel(root, "EnemyPanel",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-160, 0), new Vector2(320, 200));
        _enemyLabel = CreateText(enemyPanel, "EnemyLabel", "Enemy Lv1", 22,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(280, 56));
        _enemyHpBarFill = CreateFilledBar(enemyPanel, "EnemyHP", Color.red, new Vector2(0.5f, 1f), new Vector2(0, -95), new Vector2(280, 24));
        _enemySpBarFill = CreateFilledBar(enemyPanel, "EnemySP", Color.cyan, new Vector2(0.5f, 1f), new Vector2(0, -135), new Vector2(280, 16));

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
