using UnityEngine;

/// <summary>
/// Handles the lifetime of a GameObject and optionally destroys it upon collision or trigger events.
/// Useful for projectiles, temporary effects, or any object that should self-destruct after a set time.
/// </summary>
public class Lifetime : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] private float lifetime = 5f;   // The total time (in seconds) before this GameObject is automatically destroyed
    [SerializeField] private bool destroyOnCollision;   // If true, the object will be destroyed when it collides with another collider
    [SerializeField] private bool destroyOnTrigger; // If true, the object will be destroyed when it enters a trigger collider
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        Destroy(gameObject, lifetime); // Schedule automatic destruction
    }

    /// <summary>
    /// Will destroy the object if destroyOnCollision is enabled.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (destroyOnCollision)
            Destroy(gameObject);
    }

    /// <summary>
    /// Will destroy the object if destroyOnTrigger is enabled.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (destroyOnTrigger)
            Destroy(gameObject);
    }
    #endregion
}