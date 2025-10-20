using Rewired.Components;
using Rewired.Demos;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelGamepadMouse : MonoBehaviour
{
    private PlayerMouse playerMouse;   
    public GameObject uiPointer;      

    [Tooltip("Ocultar el cursor físico mientras se usa el mouse virtual del mando")]
    public bool lockHardwareCursor = true;

    private void OnEnable()
    {
        if (playerMouse == null)
            playerMouse = FindAnyObjectByType<PlayerMouse>(FindObjectsInactive.Include);

        if (uiPointer == null)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "UIPointer" && go.scene.isLoaded)
                {
                    uiPointer = go;
                    break;
                }
            }
        }

        if (playerMouse != null)
            playerMouse.enabled = true;
        else
            Debug.LogWarning("No se encontró PlayerMouse en la escena.");

        if(uiPointer != null)
            uiPointer.SetActive(true);
        else
            Debug.LogWarning("No se encontró un objeto llamado 'UIPointer' en la escena.");

        if (lockHardwareCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void OnDisable()
    {
        if (playerMouse != null)
            playerMouse.enabled = false;

        if (uiPointer != null)
            uiPointer.SetActive(false);

        if (lockHardwareCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
