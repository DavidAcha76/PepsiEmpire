using Rewired;
using UnityEngine;

public class StartGameController : MonoBehaviour
{
    [Header("Refs")]
    public CameraTargetSwitcher cameraSwicher;
    public GameObject rootUI;
    public GameObject panelMix;


    private Player player;
    private bool isPlayerCloser = false;
    private bool gameOpen = false;
    private GameObject instancePanel;
    

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
            instancePanel = Instantiate(panelMix, rootUI.transform, false);
        }
        else if(gameOpen && player.GetButtonDown("Close"))
        {
            gameOpen = false;
            cameraSwicher.ReturnToPlayer();
            if(instancePanel) Destroy(instancePanel);
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
