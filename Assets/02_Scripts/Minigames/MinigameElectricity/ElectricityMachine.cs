using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElectricityMachine : MonoBehaviour
{
    [Header("Progreso / Energía")]
    [Range(0f, 1f)] public float charge = 1f;  // 0–1
    public float decayRate = 0.05f;            // baja por segundo
    public float chargeRate = 0.25f;           // sube por segundo cuando se mantiene

    [Header("UI")]
    public Slider chargeBar;                   // referencia a la barra
    public Image barFill;                      // opcional para cambiar color
    public Button holdButton;                  // botón de mantener presionado
    public GameObject panel;

    [Header("Colores de estado")]
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color lowColor = Color.red;

    [Header("Eventos")]
    public UnityEngine.Events.UnityEvent OnFullyCharged;
    public UnityEngine.Events.UnityEvent OnPowerDepleted;

    private bool isHolding = false;

    void Start()
    {
        // asegurar que la barra empiece bien
        UpdateUI();

        // si el botón está asignado, aseguramos que no sea interactivo por clicks normales
        if (holdButton != null)
            holdButton.onClick.RemoveAllListeners(); // evitamos acciones por click suelto
    }

    void Update()
    {
        // 🔹 Baja energía constantemente
        charge -= decayRate * Time.deltaTime;

        // 🔹 Sube si el jugador mantiene presionado el botón
        if (isHolding)
            charge += chargeRate * Time.deltaTime;

        // 🔹 Limitar entre 0 y 1
        charge = Mathf.Clamp01(charge);

        UpdateUI();

        // 🔹 Eventos
        if (charge <= 0f)
            OnPowerDepleted.Invoke();

        if (charge >= 1f)
            OnFullyCharged.Invoke();

        if (!panel.activeInHierarchy && isHolding)
        {
            isHolding = false;
            Debug.Log("🪫 Panel cerrado, reseteando estado de recarga.");
        }
    }

    public void OnPointerDown()
    {

            isHolding = true;
            Debug.Log("⚡ MANTENIENDO botón de electricidad...");
        
    }

    public void OnPointerUp()
    {

            isHolding = false;
            Debug.Log("🪫 Soltaste el botón de electricidad");
        
    }

    private void UpdateUI()
    {
        if (chargeBar) chargeBar.value = charge;

        if (barFill)
        {
            if (charge > 0.6f) barFill.color = fullColor;
            else if (charge > 0.3f) barFill.color = midColor;
            else barFill.color = lowColor;
        }
    }
}
