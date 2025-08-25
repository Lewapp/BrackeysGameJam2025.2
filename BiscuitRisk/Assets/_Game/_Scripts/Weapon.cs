using System;
using UnityEngine;

/// <summary>
/// Handles weapon shooting mechanics, including firing bullets toward the centre of the screen.
/// Manages fire rate and projectile speed using WeaponStats.
/// </summary>
public class Weapon : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] private GameObject bulletPrefab; // Prefab of the bullet to spawn when shooting
    [SerializeField] private Transform spawnPoint;    // Point from which the bullet will be instantiated
    [SerializeField] private WeaponStats stats;       // Contains stats for the weapon
    #endregion

    #region Private Variables
    private float timeSinceLastShot = 0f;            // Timer to enforce fire rate between shots
    #endregion

    #region Unity Callbacks
    private void Update()
    {
        timeSinceLastShot += Time.deltaTime; // Increment timer each frame
    }
    #endregion

    #region Methods
    /// <summary>
    /// Shoots a bullet toward the centre of the screen
    /// Respects the weapon's fire rate and requires a bullet prefab and spawn point.
    /// </summary>
    public virtual void Shoot()
    {
        // Ensure the weapon has a bullet prefab and spawn point
        if (!bulletPrefab || !spawnPoint)
            return;

        // Check fire rate cooldown
        if (timeSinceLastShot < stats.fireRate)
            return;

        // Reset fire timer
        timeSinceLastShot = 0f;

        // Create a ray from the centre of the screen
        Ray _ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 _direction = Vector3.zero;

        // Perform raycast to see if the bullet should target a hit point
        if (Physics.Raycast(_ray, out RaycastHit hit, 1000f))
        {
            // Calculate normalised direction from spawn point to hit point
            _direction = (hit.point - spawnPoint.position).normalized;
        }
        else
        {
            // If nothing is hit, shoot straight forward along the ray
            _direction = _ray.direction;
        }

        // Instantiate the bullet at the spawn point with correct rotation
        GameObject _bullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.LookRotation(_direction));

        // Apply velocity to the bullet if it has a Rigidbody
        Rigidbody _rb = _bullet.GetComponent<Rigidbody>();
        if (_rb)
        {
            _rb.linearVelocity = _direction * stats.projectileSpeed;
        }
    }
    #endregion

    #region Sub-Classes
    /// <summary>
    /// Serialisable class to hold weapon statistics.
    /// Can be edited in the Inspector.
    /// </summary>
    [Serializable]
    public class WeaponStats
    {
        public int damage;             // Amount of damage each bullet deals
        public float fireRate;         // Minimum time between shots (in seconds)
        public float projectileSpeed;  // Speed at which the bullet travels
    }
    #endregion
}