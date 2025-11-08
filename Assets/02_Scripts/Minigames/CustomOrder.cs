using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Rewired;

public class CustomOrder : MonoBehaviour
{
    [Header("Referencias")]
    public ClawMinigameController clawMinigame;
    public ItemData requestedItem;
    public Slider timerSlider;
    public GameObject pedidoInfoPanel;   // Panel que muestra qué quiere el NPC
    public Image pedidoIcon;             // Imagen del item pedido
    public Button closeInfoButton;
    public Button startEntregaButton;    // Botón para lanzar el minijuego
    public CameraTargetSwitcher cameraTargetSwitcher;

    [Header("Configuración")]
    public float tiempoPedido = 15f;

    private Player player;
    private bool playerClose = false;
    private bool pausado = false;
    private bool completado = false;
    private bool pedidoVisto = false;
    private float timeLeft;
    private bool orderActive = false;

    void Awake()
    {
        player = ReInput.players.GetPlayer(0);
        if (closeInfoButton) closeInfoButton.onClick.AddListener(CerrarInfoPedido);
        if (startEntregaButton) startEntregaButton.onClick.AddListener(IniciarEntrega);
    }

    void Start()
    {
        // Inicia el pedido al aparecer el NPC
        AsignarPedido(requestedItem);
    }

    void Update()
    {
        if (!orderActive || completado) return;

        // Temporizador
        if (!pausado)
        {
            timeLeft -= Time.deltaTime;
            if (timerSlider) timerSlider.value = timeLeft / tiempoPedido;
            if (timeLeft <= 0)
            {
                Debug.Log("❌ Tiempo agotado. Pedido fallido.");
                orderActive = false;
                return;
            }
        }

        // Interactuar con el NPC
        if (playerClose && player.GetButtonDown("Interact"))
        {
            if (!pedidoVisto)
            {
                MostrarInfoPedido();
                pedidoVisto = true;
            }
            else
            {
                MostrarPanelEntrega();
            }
        }
    }

    // ------------------------------
    // MOSTRAR PEDIDO
    // ------------------------------
    void MostrarInfoPedido()
    {
        pausado = true;
        pedidoInfoPanel.SetActive(true);
        if (pedidoIcon) pedidoIcon.sprite = requestedItem.icon;
        Debug.Log($"📜 NPC quiere: {requestedItem.itemName}");
    }

    void CerrarInfoPedido()
    {
        pausado = false;
        pedidoInfoPanel.SetActive(false);
        Debug.Log("📥 Cerrado panel de pedido, continúa el tiempo.");
    }

    // ------------------------------
    // INICIAR ENTREGA
    // ------------------------------
    void MostrarPanelEntrega()
    {
        pausado = true;
        pedidoInfoPanel.SetActive(true);
        startEntregaButton.gameObject.SetActive(true);
        if (pedidoIcon) pedidoIcon.sprite = requestedItem.icon;
        Debug.Log("🚚 Listo para entregar, presiona 'Iniciar Entrega'.");
    }

    void IniciarEntrega()
    {
        pedidoInfoPanel.SetActive(false);
        Debug.Log("🎮 Iniciando minijuego de la garra...");
        //clawMinigame.StartClawGame(this);
    }

    // ------------------------------
    // RESULTADO DE LA ENTREGA
    // ------------------------------
    public void CompletarPedido(ItemData entregado)
    {
        pedidoInfoPanel.SetActive(false);

        if (entregado == requestedItem)
        {
            Debug.Log($"✅ Pedido correcto: {entregado.itemName}");
            completado = true;
        }
        else
        {
            Debug.Log($"❌ Pedido incorrecto. Esperaba {requestedItem.itemName}");
        }
    }

    // ------------------------------
    // INICIO DE PEDIDO
    // ------------------------------
    public void AsignarPedido(ItemData item)
    {
        requestedItem = item;
        timeLeft = tiempoPedido;
        orderActive = true;
        pausado = false;
        completado = false;
        pedidoVisto = false;

        if (timerSlider) timerSlider.value = 1f;
        if (pedidoIcon) pedidoIcon.sprite = item.icon;

        Debug.Log($"🧾 Pedido asignado: {item.itemName}");
    }

    // ------------------------------
    // TRIGGERS
    // ------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerClose = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = false;
            // cerrar cualquier panel si el jugador se aleja
            pedidoInfoPanel.SetActive(false);
        }
    }
}
