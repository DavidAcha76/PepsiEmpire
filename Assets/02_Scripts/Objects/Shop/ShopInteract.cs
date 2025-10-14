using Rewired;
using UnityEngine;

public class ShopInteract : MonoBehaviour
{
    public GameObject rootUI;
    public GameObject panelShop;

    private Player player;
    private GameObject instancePanel;

    private void Update()
    {
        if (player!=null)
        {
            if (player.GetButtonDown("Interact"))
            {
                instancePanel = Instantiate(panelShop);
                instancePanel.transform.SetParent(rootUI.transform, false);
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
            player = null;
            Destroy(instancePanel);
            instancePanel = null;

        }
    }
}
