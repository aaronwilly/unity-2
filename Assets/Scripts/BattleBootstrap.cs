using System.Collections;
using UnityEngine;

/// <summary>
/// Entry point: attach to a single empty GameObject in an empty scene.
/// Creates units, managers, start screen, then battle UI (hidden until Start). No inspector refs.
/// </summary>
public class BattleBootstrap : MonoBehaviour
{
    private Unit _player1;
    private Unit _player2;
    private Unit _enemy;
    private TurnManager _turnManager;
    private CaptureManager _captureManager;
    private BattleManager _battleManager;
    private UIManager _uiManager;
    private StartScreenManager _startScreenManager;
    private bool _battleEnded;

    private void Awake()
    {
        CreateSoundManager();
        CreateScreenShakeManager();
        CreateUnits();
        CreateManagers();
        CreateUI();
        CreateStartScreen();
        WireCaptureManager();
        _turnManager.OnTurnChanged += OnTurnChanged;
    }

    private void CreateSoundManager()
    {
        var go = new GameObject("SoundManager");
        go.AddComponent<SoundManager>();
    }

    private void CreateScreenShakeManager()
    {
        var go = new GameObject("ScreenShakeManager");
        go.AddComponent<ScreenShakeManager>();
    }

    private void OnTurnChanged(int turnIndex)
    {
        if (_battleEnded) return;
        if (turnIndex == TurnManager.EnemyIndex)
            StartCoroutine(EnemyTurnAfterDelay());
    }

    private IEnumerator EnemyTurnAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (_battleEnded || !_enemy.IsAlive) yield break;
        Unit target = UnityEngine.Random.Range(0, 2) == 0 ? _player1 : _player2;
        if (!target.IsAlive) target = _player1.IsAlive ? _player1 : _player2;
        if (target != null && target.IsAlive)
        {
            yield return _uiManager.StartCoroutine(_uiManager.PlayAttackAnimationCoroutine(_enemy, target, null));
            _battleManager.ExecuteAbility(Ability.BasicAttack, _enemy, target);
        }
    }

    private void CreateUnits()
    {
        _player1 = new Unit("Hero A", 1, 100, 50);
        _player2 = new Unit("Hero B", 1, 100, 50);
        _enemy = new Unit("Slime", 2, 80, 40);
    }

    private void CreateManagers()
    {
        _turnManager = new TurnManager();
        var captureGo = new GameObject("CaptureManager");
        _captureManager = captureGo.AddComponent<CaptureManager>();
        _battleManager = new BattleManager(_player1, _player2, _enemy, _turnManager, _captureManager);
        _captureManager.Initialize(_battleManager);
    }

    private void CreateUI()
    {
        var uiGo = new GameObject("UIManager");
        _uiManager = uiGo.AddComponent<UIManager>();
        _uiManager.Initialize(_battleManager, _turnManager, RestartBattle);
    }

    /// <summary>Resets entire battle state without scene reload. Called when Restart is pressed.</summary>
    private void RestartBattle()
    {
        _battleEnded = false;
        _player1.Reset();
        _player2.Reset();
        _enemy.Reset();
        _turnManager.Reset();
        _uiManager.ResetAfterRestart();
    }

    private void CreateStartScreen()
    {
        var startGo = new GameObject("StartScreenManager");
        _startScreenManager = startGo.AddComponent<StartScreenManager>();
        _startScreenManager.Initialize(OnStartPressed);
    }

    private void OnStartPressed()
    {
        _uiManager.FadeInBattle(null);
    }

    private void WireCaptureManager()
    {
        _battleManager.OnBattleEnd += () =>
        {
            _battleEnded = true;
            Debug.Log("Battle ended.");
        };
    }
}
