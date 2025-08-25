using UnityEngine;

/// <summary>
/// Smoothly rotates the GameObject to face a target provided by an ITargetSeeker.
/// </summary>
public class SmoothLookAt : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation towards the target
    #endregion

    #region Private Variables
    private ITargetSeeker seeker; // Reference to a component that provides a target
    #endregion

    #region Unity Callbacks

    private void Start()
    {
        // Get the ITargetSeeker component on start.
        seeker = GetComponent<ITargetSeeker>();
    }

    private void Update()
    {
        // Skip if there is no seeker or target
        if (seeker == null || seeker.target == null)
            return;

        // Calculate direction vector from this object to the target
        Vector3 direction = seeker.target.position - transform.position;
        direction.y = 0f;

        // Skip if the target is extremely close to avoid jitter
        if (direction.sqrMagnitude < 0.001f)
            return;

        // Calculate the desired rotation to look at the target
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly interpolate from current rotation to target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    #endregion
}