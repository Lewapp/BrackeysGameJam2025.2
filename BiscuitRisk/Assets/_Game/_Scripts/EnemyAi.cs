using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages the navigation and AI behaviour of the enemy
/// </summary>
public class EnemyAi : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] private Transform leftCannon;  // Left cannon transform for shooting
    [SerializeField] private Transform rightCannon; // Right cannon transform for shooting
    [SerializeField] private GameObject laserBullet; // Laser bullet prefab
    [SerializeField] private float shootingDistance; // Distance at which enemy starts shooting
    [SerializeField] private float fireRate;        // Time between shots
    [SerializeField] private float laserSpeed = 5;  // Speed of the laser projectile
    #endregion

    #region Private Variables
    private ITargetSeeker seeker;                   // Interface for acquiring target
    private NavMeshAgent thisAgent;                 // NavMeshAgent for movement
    private float timeSinceLastShot;                // Timer to track fire rate
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        // Get components for target seeking and navigation
        seeker = GetComponent<ITargetSeeker>();
        thisAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Exit early if no target or agent exists
        if (!seeker?.target || !thisAgent)
            return;

        // Move toward the target
        thisAgent.SetDestination(seeker.target.position);

        // Increment shooting timer
        timeSinceLastShot += Time.deltaTime;

        // Check if enemy can shoot
        if (timeSinceLastShot >= fireRate && Vector3.Distance(transform.position, seeker.target.position) <= shootingDistance)
        {
            ShootAtTarget();      // Fire lasers from both cannons
            timeSinceLastShot = 0f; // Reset shot timer
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Shoots two lasers, one from each cannon towards the target.
    /// </summary>
    private void ShootAtTarget()
    {
        // Fire laser from left and right cannons
        FireLaser(leftCannon);
        FireLaser(rightCannon);
    }

    /// <summary>
    /// Handles teh spawning of the laser from the chosen cannon.
    /// </summary>
    private void FireLaser(Transform _cannon)
    {
        // Ensure prefab and target exist
        if (!laserBullet || !seeker.target)
            return;

        // Calculate normalized direction toward the target
        Vector3 _direction = (seeker.target.position - _cannon.position).normalized;

        // Instantiate laser and rotate it to face the target
        GameObject _laser = Instantiate(laserBullet, _cannon.position, Quaternion.LookRotation(_direction));

        // Apply velocity along forward direction
        Rigidbody _rb = _laser.GetComponent<Rigidbody>();
        if (_rb)
            _rb.linearVelocity = _laser.transform.forward * laserSpeed;
    }
    #endregion
}