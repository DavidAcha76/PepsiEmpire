// INPCMovement.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public interface INPCMovement
{
    event Action ReachedDestination;
    void SetAgent(Transform owner);
    void MoveTo(Vector3 position);
    void Stop();
    bool IsMoving { get; }
    void FollowPath(IReadOnlyList<Vector3> positions);
}
