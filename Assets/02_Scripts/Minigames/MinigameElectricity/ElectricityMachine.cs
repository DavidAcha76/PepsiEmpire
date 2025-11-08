using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElectricityMachine : MonoBehaviour
{
    

    [Header("Progreso / Energía")]
    [Range(0f, 1f)] public float charge = 1f;
    public float decayRate = 0.005f;
    public float chargeRate = 0.25f;

    [Header("Referencias externas")]
    public TimeManager timeManager;

    [Header("UI")]
    public Slider chargeBar;
    public Image barFill;
    public Button holdButton;
    public GameObject panel;
    public GameObject vfxElectricity;

    [Header("Colores de estado")]
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color lowColor = Color.red;

    [Header("Luces")]
    public LightGroupFlicker groupA;
    public LightGroupFlicker groupB;
    public LightGroupFlicker groupC;

    [Header("Eventos")]
    public UnityEngine.Events.UnityEvent OnFullyCharged;
    public UnityEngine.Events.UnityEvent OnPowerDepleted;

    private bool isHolding = false;
    private int lastDay = -1;
    private bool hasTriggeredDepletion = false;

    void Start()
    {
        

        UpdateUI();
        if (holdButton != null)
            holdButton.onClick.RemoveAllListeners();
    }

    void Update()
    {

        int currentDay = timeManager.GetCurrentDay();
        if (currentDay != lastDay)
        {
            lastDay = currentDay;
            UpdateDecayRateForDay(currentDay);
        }

        // Energía base
        charge -= decayRate * Time.deltaTime;
        if (isHolding) charge += chargeRate * Time.deltaTime;
        charge = Mathf.Clamp01(charge);

        UpdateUI();
        UpdateLights();

        // Eventos
        if (charge <= 0f && !hasTriggeredDepletion)
        {
            hasTriggeredDepletion = true;
            OnPowerDepleted.Invoke();
            Debug.Log($"⚡ [ElectricityMachine] ENERGÍA AGOTADA (día {currentDay}) — decayRate={decayRate}");
        }

        if (charge >= 1f)
        {
            hasTriggeredDepletion = false;
            OnFullyCharged.Invoke();
            

        }

        if (!panel.activeInHierarchy && isHolding)
        {
            isHolding = false;
            vfxElectricity.SetActive(false);
        }
    }

    private void UpdateDecayRateForDay(int day)
    {
        switch (day)
        {
            case 1: decayRate = 0.0042f; break;
            case 2: decayRate = 0.005f; break;
            case 3: decayRate = 0.0062f; break;
            case 4: decayRate = 0.0083f; break;
            case 5: decayRate = 0.011f; break;
            default: decayRate = 0.011f; break; // días posteriores = extremo
        }

        Debug.Log($"[ElectricityMachine] Día {day}: decayRate = {decayRate}");
    }

    private void UpdateLights()
    {
        if (charge > 0.6f)
        {
            groupA.SetLightsOn(true);
            groupB.SetLightsOn(true);
            groupC.SetLightsOn(true);
        }
        else if (charge > 0.3f)
        {
            groupA.SetFlickerActive(true,1.5f);
            groupB.SetLightsOn(true);
            groupC.SetLightsOn(true);
        }
        else if (charge > 0.1f)
        {
            groupA.SetLightsOn(false);
            groupB.SetFlickerActive(true,2f);
            groupC.SetLightsOn(true);
        }
        else
        {
            groupA.SetLightsOn(false);
            groupB.SetLightsOn(false);
            groupC.SetFlickerActive(true, 3f); 
        }
    }

    public void OnPointerDown()
    {
        isHolding = true;
        vfxElectricity.SetActive(true);
    }

    public void OnPointerUp()
    {
        isHolding = false;
        vfxElectricity.SetActive(false);
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
