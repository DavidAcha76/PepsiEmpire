using Rewired;
using UnityEngine;

public class StartGameController : MonoBehaviour
{
    public CameraTargetSwitcher cameraSwicher;

    private Player player;
    private bool isPlayerCloser = false;
    private bool gameOpen = false;
    

    private void Awake()
    {
        player = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        if (isPlayerCloser && !gameOpen && player.GetButtonDown("Interact"))
        {
            gameOpen = true;
            cameraSwicher.LookAtOtherTarget();
        }
        else if(gameOpen && player.GetButtonDown("Close"))
        {
            gameOpen = false;
            cameraSwicher.ReturnToPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            isPlayerCloser = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            isPlayerCloser = false;
    }
}
