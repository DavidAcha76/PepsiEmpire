using Rewired;
using UnityEngine;

public class StartGameController : MonoBehaviour
{
    [Header("Refs")]
    public CameraTargetSwitcher cameraSwicher;
    public GameObject rootUI;
    public MixingMinigameController panelMixPrefab; 

    private Player player;
    private bool isPlayerCloser = false;
    private bool gameOpen = false;
    private MixingMinigameController instancePanel;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        if (!isPlayerCloser) return;

        if (gameOpen && panelMixPrefab.pickupInstance && panelMixPrefab.pickupInstance.IsEmpty())
        {
            Debug.Log("[MixGame] Pickup recogido → cerrando panel automáticamente.");
            cameraSwicher.ReturnToPlayer();
            panelMixPrefab.gameObject.SetActive(false);
            gameOpen = false;

            Destroy(panelMixPrefab.pickupInstance.gameObject);
            panelMixPrefab.pickupInstance = null;
            return;
        }

        // ---- ABRIR ----
        if (!gameOpen && player.GetButtonDown("Interact"))
        {
            // Si hay una referencia pero el objeto ya fue destruido, limpiar
            /*if (instancePanel == null)
            {
                instancePanel = null;
            }*/

            // Si ya hay un pickup pendiente, mostrarlo directamente
            if (instancePanel && panelMixPrefab.pickupInstance && !panelMixPrefab.pickupInstance.IsEmpty())
            {
                panelMixPrefab.pickupInstance.gameObject.SetActive(true);
                cameraSwicher.LookAtOtherTarget();
                gameOpen = true;
                return;
            }

            // Si no hay instancia (porque se destruyó al terminar ciclo), crear una nueva
            if (instancePanel == null)
            {
                //instancePanel = Instantiate(panelMixPrefab, rootUI.transform, false);
                panelMixPrefab.gameObject.SetActive(true);
            }

            // Si la instancia existe pero está desactivada, solo reactivarla
            /*if (!instancePanel.gameObject.activeSelf)
            {
                instancePanel.gameObject.SetActive(true);
            }*/

            cameraSwicher.LookAtOtherTarget();
            gameOpen = true;
        }

        // ---- CERRAR ----
        else if (gameOpen && player.GetButtonDown("Close"))
        {
            if (!panelMixPrefab.running)
            {
                gameOpen = false;
                cameraSwicher.ReturnToPlayer();

                if (panelMixPrefab)
                {
                    // Caso A: mezcla completada → pickup activo
                    if (panelMixPrefab.pickupInstance && !panelMixPrefab.pickupInstance.IsEmpty())
                    {
                        panelMixPrefab.pickupInstance.gameObject.SetActive(false);
                    }
                    // Caso B: mezcla aún en curso
                    else
                    {
                        panelMixPrefab.gameObject.SetActive(false);
                    }

                    if (panelMixPrefab.pickupInstance && panelMixPrefab.pickupInstance.IsEmpty())
                    {
                        Destroy(panelMixPrefab.pickupInstance.gameObject);
                        panelMixPrefab.pickupInstance = null;
                    }
                }
            }
        }
    }

    public void ClosePanel()
    {
        if (gameOpen)
        {
            if (!panelMixPrefab.running)
            {
                gameOpen = false;
                cameraSwicher.ReturnToPlayer();

                if (panelMixPrefab)
                {
                    // Caso A: mezcla completada → pickup activo
                    if (panelMixPrefab.pickupInstance && !panelMixPrefab.pickupInstance.IsEmpty())
                    {
                        panelMixPrefab.pickupInstance.gameObject.SetActive(false);
                    }
                    // Caso B: mezcla aún en curso
                    else
                    {
                        panelMixPrefab.gameObject.SetActive(false);
                    }

                    if (panelMixPrefab.pickupInstance && panelMixPrefab.pickupInstance.IsEmpty())
                    {
                        Destroy(panelMixPrefab.pickupInstance.gameObject);
                        panelMixPrefab.pickupInstance = null;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerCloser = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerCloser = false;
    }
}
