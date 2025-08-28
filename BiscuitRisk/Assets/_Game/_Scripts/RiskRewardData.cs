using UnityEngine;

/// <summary>
/// ScriptableObject that defines a risk & reward scenario.
/// Can be used to describe how certain actions or events affect
/// either the player or enemies by modifying a specific stat.
/// </summary>
[CreateAssetMenu(fileName = "RiskRewardData", menuName = "Game/Risk & Reward Info", order = 0)]
public class RiskRewardData : ScriptableObject
{
    [Tooltip("General description of the context or scenario. Example: 'Opening this chest may spawn enemies but drop rare loot.'")]
    [TextArea(2, 5)]
    public string description;

    [Tooltip("Which actor is affected by this risk/reward? Options: None, Player, Enemy.")]
    public Actor effected;

    [Tooltip("Which stat is modified by this risk/reward? Options include MaxHealth, Damage, BulletSpeed, FireRate.")]
    public Stat stat;

    [Tooltip("Percentage change applied to the selected stat. Use positive values for buffs/rewards and negative values for debuffs/risks. Example: -0.2 = reduce by 20%, 0.3 = increase by 30%.")]
    [Range(-1f, 1f)]
    public float changePercent;

    /// <summary>
    /// Defines which type of actor this risk/reward affects.
    /// </summary>
    public enum Actor
    {
        None,   // No effect
        Player, // Affects the player
        Enemy,  // Affects enemies
    }

    /// <summary>
    /// Defines which stat is modified by this risk/reward.
    /// </summary>
    public enum Stat
    {
        None,        // No stat change
        MaxHealth,   // Maximum health value
        Damage,      // Damage dealt
        BulletSpeed, // Projectile speed
        FireRate,    // Rate of fire (shots per second)
    }
}