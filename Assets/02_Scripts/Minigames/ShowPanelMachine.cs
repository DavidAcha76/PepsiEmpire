using Rewired;
using UnityEngine;
using System.Collections;

public class ShowPanelMachine : MonoBehaviour
{
    [Header("Refs")]
    public EssenceMine essenceMine;
    public CameraTargetSwitcher cameraTargetSwitcher;

    private Player player;
    private bool isPlayerClose = false;
    private bool isOpen = false;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(0);
    }

    void Update()
    {
        if (isPlayerClose && player.GetButtonDown("Interact"))
        {
            essenceMine.MostrarPickup();
            cameraTargetSwitcher.LookAtOtherTarget();
            isOpen = true;
        }
        else if (isPlayerClose && player.GetButtonDown("Close"))
        {
            essenceMine.OcultarPickup();
            cameraTargetSwitcher.ReturnToPlayer();
            isOpen = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerClose = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerClose = false;
    }
}
