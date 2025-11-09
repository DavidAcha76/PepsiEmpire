using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FloatingUI : MonoBehaviour
{
    [Header("Movimiento flotante")]
    public float amplitude = 0.25f;     
    public float frequency = 2f;        

    [Header("Mirar cámara")]
    public bool faceCamera = true;

    public TextMeshProUGUI textMeshProUGUI;
    public Image image;

    private Vector3 startPos;
    Rewired.Player player;

    private void Start()
    {
        startPos = transform.localPosition;
        // Obtener el jugador (por defecto el jugador 0)
        player = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        // Rebote
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        if (faceCamera && Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
        }

        
        // Obtener el último controlador activo
        Controller lastActiveController = player.controllers.GetLastActiveController();
        // Verificar el tipo de controlador
        if (lastActiveController != null)
        {
            if (lastActiveController.type == ControllerType.Keyboard)
            {
                Debug.Log("Usando Teclado");
                textMeshProUGUI.gameObject.SetActive(true);
                image.gameObject.SetActive(false);
            }
            else if (lastActiveController.type == ControllerType.Joystick)
            {
                Debug.Log("Usando Mando/Gamepad");
                textMeshProUGUI.gameObject.SetActive(false);
                image.gameObject.SetActive(true);
                // Tu código para input de gamepad
            }
            else if (lastActiveController.type == ControllerType.Mouse)
            {
                Debug.Log("Usando Mouse");
                textMeshProUGUI.gameObject.SetActive(true);
                image.gameObject.SetActive(false);
                // Tu código para input de mouse
            }
        }

    }
}
