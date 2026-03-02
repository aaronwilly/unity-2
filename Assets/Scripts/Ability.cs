using UnityEngine;

/// <summary>
/// Defines an ability: id, display name, SP cost, and damage multiplier.
/// </summary>
public class Ability
{
    public string Id { get; }
    public string DisplayName { get; }
    public int SPCost { get; }
    public float DamageMultiplier { get; }

    public Ability(string id, string displayName, int spCost, float damageMultiplier)
    {
        Id = id;
        DisplayName = displayName;
        SPCost = spCost;
        DamageMultiplier = damageMultiplier;
    }

    public static readonly Ability BasicAttack = new Ability("basic", "Attack", 0, 1f);
    public static readonly Ability DefensiveMove = new Ability("defense", "Defend", 0, 0f);
    public static readonly Ability SpecialSkill = new Ability("special", "Special", 20, 2f);
}
