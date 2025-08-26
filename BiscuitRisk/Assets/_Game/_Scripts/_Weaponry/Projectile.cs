using System;
using UnityEngine;

/// <summary>
/// Handles projectile behaviour, including dealing damage on collision and optional self-destruction.
/// </summary>
public class Projectile : MonoBehaviour
{
    #region Public Variables
    [HideInInspector] public GameObject sourceObject;
    #endregion

    #region Inspector Variables
    [SerializeField] private ProjectileStats stats; // Configurable stats for this projectile
    [SerializeField] private LayerMask ignoreLayer; // Collision Layers to ignore
    #endregion

    #region Unity Callbacks
    private void OnCollisionEnter(Collision collision)
    {
        if ((ignoreLayer.value & (1 << collision.gameObject.layer)) != 0)
            return;

        // Attempt to get IDamageable component from collided object
        foreach (var _damageable in collision.gameObject.GetComponents<IDamageable>())
        {
            _damageable.TakeDamage(stats.damage, sourceObject); // Apply damage
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