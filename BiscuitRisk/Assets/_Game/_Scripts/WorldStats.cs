using UnityEngine;

/// <summary>
/// ScriptableObject to define base stats and modified stats
/// for both Player and Enemy characters.
/// </summary>
[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Game/Character Stats", order = 1)]
public class WorldStats : ScriptableObject
{
    [Tooltip("Is this stats profile for a Player or Enemy?")]
    public Type type;

    [Header("Base Stats")]
    [Tooltip("Base maximum health value before buffs or modifiers.")]
    public float baseMaxHealth = 100f;

    [Tooltip("Base damage dealt per attack.")]
    public float baseDamage = 10f;

    [Tooltip("Base fire rate (attacks per second).")]
    public float baseFireRate = 1f;

    [Tooltip("Base movement speed.")]
    public float baseMoveSpeed = 5f;

    [Tooltip("Base movement speed.")]
    public float baseProjectileSpeed = 10f;

    [Header("Increased Stats")]
    public float maxHealthBoost;
    public float damageBoost;
    public float firerateBoost;
    public float moveSpeedBoost;

    /// <summary>
    /// Defines whether these stats belong to a Player or Enemy.
    /// </summary>
    public enum Type
    {
        Player,
        Enemy
    }
}