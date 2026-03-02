using UnityEngine;

/// <summary>
/// Holds unit data: name, level, HP, SP.
/// Defense buff reduces next incoming damage by 50%.
/// </summary>
public class Unit
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int MaxHP { get; set; }
    public int HP { get; set; }
    public int MaxSP { get; set; }
    public int SP { get; set; }

    /// <summary>When true, next damage taken is halved, then cleared.</summary>
    public bool HasDefenseBuff { get; set; }

    public bool IsAlive => HP > 0;

    public float HPRatio => MaxHP > 0 ? (float)HP / MaxHP : 0f;
    public float SPRatio => MaxSP > 0 ? (float)SP / MaxSP : 0f;

    public Unit(string name, int level, int maxHp, int maxSp)
    {
        Name = name;
        Level = level;
        MaxHP = maxHp;
        HP = maxHp;
        MaxSP = maxSp;
        SP = maxSp;
        HasDefenseBuff = false;
    }

    public void TakeDamage(int rawDamage)
    {
        int damage = HasDefenseBuff ? Mathf.RoundToInt(rawDamage * 0.5f) : rawDamage;
        HasDefenseBuff = false;
        HP = Mathf.Max(0, HP - damage);
    }

    public void SpendSP(int amount)
    {
        SP = Mathf.Max(0, SP - amount);
    }

    public void ApplyDefenseBuff()
    {
        HasDefenseBuff = true;
    }
}
