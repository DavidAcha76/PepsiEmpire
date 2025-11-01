using Rewired;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class ShopInteract : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject rootUI;
    public GameObject panelShop;
    public CinemachineBridgeCamera camera_;
    public GameObject playerInstance;

    private Player player;
    private GameObject instancePanel;
    private bool isPlayerCloser = false;
    private bool shopOpen;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        if (isPlayerCloser && !shopOpen && player.GetButtonDown("Interact"))
        {
            OpenShop();
        }

        if (shopOpen && player.GetButtonDown("Close"))
        {
            CloseShop();
        }
    }

    private void OpenShop()
    {

        instancePanel = Instantiate(panelShop, rootUI.transform, false);


        camera_.GetComponent<CinemachineCamera>().enabled = false;
        playerInstance.GetComponent<PlayerController>().enabled = false;
        //playerInstance.GetComponent<PlayerJump>().enabled = false;
        playerInstance.GetComponent<PlayerMovement>().Stop(true);

        shopOpen = true;
    }

    private void CloseShop()
    {
        if (instancePanel) Destroy(instancePanel);
        instancePanel = null;

        camera_.GetComponent<CinemachineCamera>().enabled = true;
        playerInstance.GetComponent<PlayerController>().enabled = true;
        //playerInstance.GetComponent<PlayerJump>().enabled = true;

        shopOpen = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerCloser = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerCloser = false;
        }
    }
}
