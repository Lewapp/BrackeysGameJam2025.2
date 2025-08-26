using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    private ITargetSeeker seeker;
    private NavMeshAgent thisAgent;

    private void Start()
    {
        seeker = GetComponent<ITargetSeeker>();
        thisAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!seeker?.target || !thisAgent)
            return;

        thisAgent.SetDestination(seeker.target.position);
    }
}
