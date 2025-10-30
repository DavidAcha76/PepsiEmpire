// WaypointPath.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [SerializeField] private List<Transform> points = new();

    public IReadOnlyList<Vector3> ForwardPositions()
    {
        var list = new List<Vector3>(points.Count);
        foreach (var p in points) if (p) list.Add(p.position);
        return list;
    }

    public IReadOnlyList<Vector3> ReversePositions()
    {
        var fwd = ForwardPositions();
        fwd.Reverse();
        return fwd;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < points.Count; i++)
        {
            if (!points[i]) continue;
            Gizmos.DrawSphere(points[i].position, 0.15f);
            if (i + 1 < points.Count && points[i + 1])
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
        }
    }
#endif
}
