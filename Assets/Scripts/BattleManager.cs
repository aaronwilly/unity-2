using System;
using UnityEngine;

/// <summary>
/// Orchestrates battle: applies abilities, deals damage, checks win/lose.
/// No inspector refs; receives units and UI from bootstrap.
/// </summary>
public class BattleManager
{
    public Unit Player1 { get; }
    public Unit Player2 { get; }
    public Unit Enemy { get; }

    public TurnManager TurnManager { get; }
    public CaptureManager CaptureManager { get; }

    public event Action OnBattleEnd;
    public event Action OnCaptureAttempt;
    public event Action<bool> OnCaptureResult;
    /// <summary>Fired when any unit's HP/SP changes so UI can refresh immediately.</summary>
    public event Action OnUnitStatsChanged;

    private const int BaseDamage = 15;
    private const float CaptureHpThreshold = 0.3f;

    public BattleManager(Unit player1, Unit player2, Unit enemy, TurnManager turnManager, CaptureManager captureManager)
    {
        Player1 = player1;
        Player2 = player2;
        Enemy = enemy;
        TurnManager = turnManager;
        CaptureManager = captureManager;
    }

    public Unit GetUnitByTurnIndex(int index)
    {
        if (index == 0) return Player1;
        if (index == 1) return Player2;
        if (index == 2) return Enemy;
        return null;
    }

    public Unit GetCurrentUnit()
    {
        return GetUnitByTurnIndex(TurnManager.GetCurrentTurnIndex());
    }

    public bool CanCapture()
    {
        if (Enemy == null || !Enemy.IsAlive) return false;
        float hpRatio = Enemy.HPRatio;
        return hpRatio < CaptureHpThreshold;
    }

    public void ExecuteAbility(Ability ability, Unit caster, Unit target)
    {
        if (ability == null || caster == null || target == null) return;
        if (!caster.IsAlive || !target.IsAlive) return;

        if (ability.SPCost > 0 && caster.SP < ability.SPCost) return;

        if (ability.Id == Ability.DefensiveMove.Id)
        {
            caster.ApplyDefenseBuff();
            OnUnitStatsChanged?.Invoke();
            TurnManager.NextTurn();
            return;
        }

        if (ability.Id == Ability.BasicAttack.Id || ability.Id == Ability.SpecialSkill.Id)
        {
            if (ability.SPCost > 0)
                caster.SpendSP(ability.SPCost);

            int damage = Mathf.RoundToInt(BaseDamage * ability.DamageMultiplier);
            target.TakeDamage(damage);
            OnUnitStatsChanged?.Invoke();

            if (!target.IsAlive)
            {
                bool enemyDied = (target == Enemy);
                bool allPlayersDead = !Player1.IsAlive && !Player2.IsAlive;
                if (enemyDied || allPlayersDead)
                    OnBattleEnd?.Invoke();
                if (!enemyDied)
                    TurnManager.NextTurn();
                return;
            }

            TurnManager.NextTurn();
        }
    }

    public void TryCapture()
    {
        if (!CanCapture()) return;

        OnCaptureAttempt?.Invoke();
        float chance = CaptureManager.ComputeCaptureChance(Enemy);
        bool success = UnityEngine.Random.Range(0f, 100f) < chance;
        OnCaptureResult?.Invoke(success);

        if (success)
        {
            Enemy.HP = 0;
            OnUnitStatsChanged?.Invoke();
            OnBattleEnd?.Invoke();
        }
        else
        {
            OnUnitStatsChanged?.Invoke();
            TurnManager.NextTurn();
        }
    }
}
