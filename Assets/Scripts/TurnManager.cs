using System;
using UnityEngine;

/// <summary>
/// Manages turn order: Player1 -> Player2 -> Enemy -> repeat.
/// Index 0,1 = players; 2 = enemy.
/// </summary>
public class TurnManager
{
    public int CurrentTurnIndex { get; private set; }
    public const int PlayerCount = 2;
    public const int EnemyIndex = 2;
    public const int TotalActors = 3;

    /// <summary>Fired when turn changes (0=P1, 1=P2, 2=Enemy). Used for turn indicator and glow.</summary>
    public event Action<int> OnTurnChanged;

    public TurnManager()
    {
        CurrentTurnIndex = 0;
    }

    public bool IsPlayerTurn => CurrentTurnIndex < PlayerCount;
    public bool IsEnemyTurn => CurrentTurnIndex == EnemyIndex;

    public void NextTurn()
    {
        CurrentTurnIndex = (CurrentTurnIndex + 1) % TotalActors;
        OnTurnChanged?.Invoke(CurrentTurnIndex);
    }

    public int GetCurrentTurnIndex()
    {
        return CurrentTurnIndex;
    }

    /// <summary>Reset to first turn (Player 1) for battle restart.</summary>
    public void Reset()
    {
        CurrentTurnIndex = 0;
        OnTurnChanged?.Invoke(CurrentTurnIndex);
    }
}
