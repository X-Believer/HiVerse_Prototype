using UnityEngine;
using UnityEngine.AI;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.GraphDesigner.Runtime.Variables;

public class MoveToBuildingEntrance : Action
{
    public SharedVariable<GameObject> Agent;
    public SharedVariable<string> TargetBuildingName;
    private GameObject TargetEntrance;

    private NavMeshAgent _navMeshAgent;

    public override void OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            Debug.LogWarning("Agent 未设置");
            return;
        }

        _navMeshAgent = Agent.Value.GetComponent<NavMeshAgent>();

        if (_navMeshAgent == null)
        {
            Debug.LogWarning("Agent 没有 NavMeshAgent");
            return;
        }

        _navMeshAgent.isStopped = false;

        GameObject entranceGO = null;
        var entrances = GameObject.FindObjectsOfType<BuildingEntrance>();

        foreach (var e in entrances)
        {
            if (e.building != null && e.building.buildingName == TargetBuildingName.Value)
            {
                entranceGO = e.gameObject;
                break;
            }
        }

        if (entranceGO == null)
        {
            Debug.LogWarning($"未找到建筑入口: {TargetBuildingName.Value}");
            return;
        }

        TargetEntrance = entranceGO;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(TargetEntrance.transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(hit.position);
            Debug.Log(
                $"onNavMesh:{_navMeshAgent.isOnNavMesh} " +
                $"hasPath:{_navMeshAgent.hasPath} " +
                $"remaining:{_navMeshAgent.remainingDistance} " +
                $"velocity:{_navMeshAgent.velocity}"
            );
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (_navMeshAgent == null || TargetEntrance == null)
            return TaskStatus.Failure;

        if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            return TaskStatus.Success; // 到达目标
        }

        return TaskStatus.Running;
    }
}