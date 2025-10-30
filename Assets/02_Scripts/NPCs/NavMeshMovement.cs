// NavMeshMovement.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshMovement : MonoBehaviour, INPCMovement
{
    public event Action ReachedDestination;
    private NavMeshAgent _agent;
    private Coroutine _pathRoutine;

    public bool IsMoving => _agent && !_agent.isStopped && _agent.remainingDistance > _agent.stoppingDistance;

    public void SetAgent(Transform owner)
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = true;
        _agent.updateUpAxis = true;
    }

    public void MoveTo(Vector3 position)
    {
        if (_pathRoutine != null) StopCoroutine(_pathRoutine);
        _agent.isStopped = false;
        _agent.SetDestination(position);
        _pathRoutine = StartCoroutine(WaitToArrive());
    }

    public void FollowPath(IReadOnlyList<Vector3> positions)
    {
        if (_pathRoutine != null) StopCoroutine(_pathRoutine);
        _pathRoutine = StartCoroutine(FollowPathRoutine(positions));
    }

    public void Stop()
    {
        if (_pathRoutine != null) StopCoroutine(_pathRoutine);
        if (_agent) _agent.isStopped = true;
    }

    private IEnumerator FollowPathRoutine(IReadOnlyList<Vector3> positions)
    {
        if (positions == null || positions.Count == 0) yield break;
        foreach (var p in positions)
        {
            _agent.isStopped = false;
            _agent.SetDestination(p);
            yield return WaitToArrive();
        }
        ReachedDestination?.Invoke();
    }

    private IEnumerator WaitToArrive()
    {
        while (_agent && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance))
            yield return null;
        ReachedDestination?.Invoke();
    }
}
