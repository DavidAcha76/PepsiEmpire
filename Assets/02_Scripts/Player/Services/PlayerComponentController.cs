using Unity.Cinemachine;
using UnityEngine;

public class PlayerComponentController : MonoBehaviour
{
    [Header("Desactivar/activar")]
    public Behaviour[] behaviours;   
    public Renderer[] renderers;
    public PlayerMovement movement;

    bool isDisabled;

    private GameObject interactUI;

    public void DisableAll()
    {
        if (isDisabled) return; isDisabled = true;
        foreach (var b in behaviours) if (b) b.enabled = false;
        foreach (var r in renderers) if (r) r.enabled = false;
        movement.Stop(true);

        interactUI = GameObject.Find("InteractuarUI(Clone)");
        if (interactUI)
        {
            interactUI.SetActive(false);
        }
    }

    public void EnableAll()
    {
        if (!isDisabled) return; isDisabled = false;
        foreach (var b in behaviours) if (b) b.enabled = true;
        foreach (var r in renderers) if (r) r.enabled = true;

        if (interactUI)
        {
            interactUI.SetActive(true);
            interactUI = null;
        }
    }

    public void DisableAllWithoutRenders()
    {
        if (isDisabled) return; isDisabled = true;
        foreach (var b in behaviours) if (b) b.enabled = false;
        movement.Stop(true);
    }

    public void EnableAllWithoutRenders()
    {
        if (!isDisabled) return; isDisabled = false;
        foreach (var b in behaviours) if (b) b.enabled = true;
        foreach (var r in renderers) if (r) r.enabled = true;
    }
}
