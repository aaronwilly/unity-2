using System.Collections;
using UnityEngine;

/// <summary>
/// Entry point: attach to a single empty GameObject in an empty scene.
/// Creates units, managers, UI, and wires everything in code. No inspector refs.
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
    private bool _battleEnded;

    private void Awake()
    {
        CreateUnits();
        CreateManagers();
        CreateUI();
        WireCaptureManager();
        _turnManager.OnTurnChanged += OnTurnChanged;
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
            _battleManager.ExecuteAbility(Ability.BasicAttack, _enemy, target);
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
        _uiManager.Initialize(_battleManager, _turnManager);
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
