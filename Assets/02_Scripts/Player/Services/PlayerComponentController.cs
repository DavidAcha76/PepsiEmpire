using Unity.Cinemachine;
using UnityEngine;

public class PlayerComponentController : MonoBehaviour
{
    [Header("Desactivar/activar")]
    public Behaviour[] behaviours;   
    //public Collider[] colliders;     
    public Renderer[] renderers;
    public CinemachineOrbitalFollow camera_;

    bool isDisabled;

    public void DisableAll()
    {
        if (isDisabled) return; isDisabled = true;
        foreach (var b in behaviours) if (b) b.enabled = false;
        camera_.enabled = false;
        //foreach (var c in colliders) if (c) c.enabled = false;
        foreach (var r in renderers) if (r) r.enabled = false;
    }

    public void EnableAll()
    {
        if (!isDisabled) return; isDisabled = false;
        foreach (var b in behaviours) if (b) b.enabled = true;
        camera_.enabled = true;
        //foreach (var c in colliders) if (c) c.enabled = true;
        foreach (var r in renderers) if (r) r.enabled = true;
    }
}
