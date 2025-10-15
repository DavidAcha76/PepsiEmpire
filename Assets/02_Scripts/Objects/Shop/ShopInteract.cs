using Rewired;
using Unity.Cinemachine;
using UnityEngine;

public class ShopInteract : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject rootUI;
    public GameObject panelShop;
    public CinemachineBridgeCamera camera_;

    private Player player;
    private GameObject instancePanel;



    private void Update()
    {
        if (player!=null && !instancePanel)
        {
            if (player.GetButtonDown("Interact"))
            {
                instancePanel = Instantiate(panelShop);
                instancePanel.transform.SetParent(rootUI.transform, false);
                camera_.GetComponent<CinemachineCamera>().enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player = ReInput.players.GetPlayer(0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(instancePanel);
            instancePanel = null;
            player = null;
            camera_.GetComponent<CinemachineCamera>().enabled = true;
        }
    }
}
