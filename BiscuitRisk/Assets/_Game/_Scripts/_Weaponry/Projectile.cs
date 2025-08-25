using System;
using UnityEngine;

/// <summary>
/// Handles projectile behaviour, including dealing damage on collision and optional self-destruction.
/// </summary>
public class Projectile : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] private ProjectileStats stats; // Configurable stats for this projectile
    #endregion

    #region Unity Callbacks
    private void OnCollisionEnter(Collision collision)
    {
        // Attempt to get IDamageable component from collided object
        if (collision.gameObject.TryGetComponent<IDamageable>(out var _damageable))
        {
            _damageable.TakeDamage(stats.damage, gameObject); // Apply damage
        }

        // Destroy projectile if flagged to do so on collision
        if (stats.destroyOnCollision)
            Destroy(gameObject);
    }
    #endregion

    #region Sub-Classes
    /// <summary>
    /// Serializsble class containing projectile-specific stats.
    /// Can be edited in the Inspector.
    /// </summary>
    [Serializable]
    public class ProjectileStats
    {
        public bool destroyOnCollision = true; // Should the projectile destroy itself on collision?
        public float damage;                   // Amount of damage this projectile deals
    }
    #endregion
}