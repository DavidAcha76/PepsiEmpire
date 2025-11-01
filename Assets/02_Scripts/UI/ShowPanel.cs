using Rewired;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ShowPanel : MonoBehaviour
{
    [Header("Refs")]
    public GameObject panel;
    public CameraTargetSwitcher cameraTargetSwitcher;

    private bool isPlayerClose = false;
    private bool isOpen = false;
    private Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = ReInput.players.GetPlayer(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlayerClose) return;

        // ABRIR PANEL
        if (!isOpen && player.GetButtonDown("Interact"))
        {
            panel.SetActive(true);
            cameraTargetSwitcher.LookAtOtherTarget();
            isOpen = true;
        }
        // CERRAR PANEL MANUALMENTE
        else if (isOpen && player.GetButtonDown("Close"))
        {
            panel.SetActive(false);
            cameraTargetSwitcher.ReturnToPlayer();
            isOpen = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerClose = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerClose = false;
    }
}
