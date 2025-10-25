using Rewired;
using UnityEngine;

public class ShowSealerMachine : MonoBehaviour
{
    [Header("Refs")]
    public SealerMinigameController sealerMinigame;
    public CameraTargetSwitcher cameraTargetSwitcher;

    private Player player;
    private bool isPlayerClose = false;
    private bool isOpen = false;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        if (!isPlayerClose) return;

        // ABRIR PANEL
        if (!isOpen && player.GetButtonDown("Interact"))
        {
            sealerMinigame.gameObject.SetActive(true);
            cameraTargetSwitcher.LookAtOtherTarget();
            isOpen = true;
            Debug.Log("[ShowSealerMachine] Abierto (panel activo).");
        }
        // CERRAR PANEL MANUALMENTE
        else if (isOpen && player.GetButtonDown("Close"))
        {
            // Probar cierre seguro (bloquea si está en juego)
            bool closed = sealerMinigame.TryClosePanel();

            // Si el controlador dejó el panel inactivo (sea porque permitió cerrar
            // o porque ya estaba inactivo con pickup oculto), devolvemos cámara.
            if (closed || !sealerMinigame.gameObject.activeSelf)
            {
                cameraTargetSwitcher.ReturnToPlayer();
                isOpen = false;
                Debug.Log("[ShowSealerMachine] Cerrado. Cámara vuelve al jugador.");
            }
            else
            {
                Debug.Log("[ShowSealerMachine] Cierre bloqueado (minijuego en curso).");
            }
        }

        // AUTO-CIERRE cuando el controller se haya desactivado por su cuenta
        if (isOpen && !sealerMinigame.gameObject.activeSelf)
        {
            cameraTargetSwitcher.ReturnToPlayer();
            isOpen = false;
            Debug.Log("[ShowSealerMachine] Detectado panel inactivo → cámara vuelve.");
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
