using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovementStrategy : IMovement
{
    private NavMeshAgent agent;

    public NavMeshMovementStrategy(NavMeshAgent agent)
    {
        this.agent = agent;
    }

    public void MoveTo(Vector3 position)
    {
        agent.isStopped = false;
        agent.SetDestination(position);
    }
  
    public void StopMoving()
    {
        agent.isStopped = true;
    }
}
