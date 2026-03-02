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
    private Image _p1HpBar;
    private Image _p1SpBar;
    private Text _p1Label;
    private Image _p2HpBar;
    private Image _p2SpBar;
    private Text _p2Label;
    private Image _enemyHpBar;
    private Image _enemySpBar;
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
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120, 0), new Vector2(320, 600));
        _p1Label = CreateText(playerPanel, "P1Label", "P1 Lv1", 24,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(280, 40));
        _p1HpBar = CreateFilledBar(playerPanel, "P1HP", Color.green, new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(280, 24));
        _p1SpBar = CreateFilledBar(playerPanel, "P1SP", Color.blue, new Vector2(0.5f, 1f), new Vector2(0, -130), new Vector2(280, 16));
        _p2Label = CreateText(playerPanel, "P2Label", "P2 Lv1", 24,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -200), new Vector2(280, 40));
        _p2HpBar = CreateFilledBar(playerPanel, "P2HP", Color.green, new Vector2(0.5f, 1f), new Vector2(0, -260), new Vector2(280, 24));
        _p2SpBar = CreateFilledBar(playerPanel, "P2SP", Color.blue, new Vector2(0.5f, 1f), new Vector2(0, -300), new Vector2(280, 16));

        var enemyPanel = CreatePanel(root, "EnemyPanel",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120, 0), new Vector2(320, 200));
        _enemyLabel = CreateText(enemyPanel, "EnemyLabel", "Enemy Lv1", 24,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(280, 40));
        _enemyHpBar = CreateFilledBar(enemyPanel, "EnemyHP", Color.red, new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(280, 24));
        _enemySpBar = CreateFilledBar(enemyPanel, "EnemySP", Color.cyan, new Vector2(0.5f, 1f), new Vector2(0, -120), new Vector2(280, 16));

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

    private Image CreateFilledBar(Transform parent, string name, Color color, Vector2 anchor, Vector2 pos, Vector2 size)
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
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = color;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;
        return fillImg;
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

        if (_p1HpBar != null) _p1HpBar.fillAmount = _battleManager.Player1.HPRatio;
        if (_p1SpBar != null) _p1SpBar.fillAmount = _battleManager.Player1.SPRatio;
        if (_p1Label != null) _p1Label.text = _battleManager.Player1.Name + " Lv" + _battleManager.Player1.Level + " " + _battleManager.Player1.HP + "/" + _battleManager.Player1.MaxHP;

        if (_p2HpBar != null) _p2HpBar.fillAmount = _battleManager.Player2.HPRatio;
        if (_p2SpBar != null) _p2SpBar.fillAmount = _battleManager.Player2.SPRatio;
        if (_p2Label != null) _p2Label.text = _battleManager.Player2.Name + " Lv" + _battleManager.Player2.Level + " " + _battleManager.Player2.HP + "/" + _battleManager.Player2.MaxHP;

        if (_enemyHpBar != null) _enemyHpBar.fillAmount = _battleManager.Enemy.HPRatio;
        if (_enemySpBar != null) _enemySpBar.fillAmount = _battleManager.Enemy.SPRatio;
        if (_enemyLabel != null) _enemyLabel.text = _battleManager.Enemy.Name + " Lv" + _battleManager.Enemy.Level + " " + _battleManager.Enemy.HP + "/" + _battleManager.Enemy.MaxHP;

        bool playerTurn = _turnManager.IsPlayerTurn;
        Unit current = _battleManager.GetCurrentUnit();
        bool canAffordSpecial = current != null && current.SP >= Ability.SpecialSkill.SPCost;
        if (_attackBtn != null) _attackBtn.interactable = playerTurn;
        if (_defendBtn != null) _defendBtn.interactable = playerTurn;
        if (_specialBtn != null) _specialBtn.interactable = playerTurn && canAffordSpecial;
    }
}
