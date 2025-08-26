using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages the navigation and ai behaviour of the enemy
/// </summary>
public class EnemyAi : MonoBehaviour
{
    #region Private Variables
    private ITargetSeeker seeker;
    private NavMeshAgent thisAgent;
    #endregion


    #region Unity Callbacks
    private void Start()
    {
        // Gets the Target Seeker interface and NavMeshAgent from this gameobject
        seeker = GetComponent<ITargetSeeker>();
        thisAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // If there's no target, seeker or agent, retrun early
        if (!seeker?.target || !thisAgent)
            return;

        // Go to the target position
        thisAgent.SetDestination(seeker.target.position);
    }
    #endregion
}
