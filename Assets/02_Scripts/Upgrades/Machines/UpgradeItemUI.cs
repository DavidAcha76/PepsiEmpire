using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI infoText;
    public Transform progressContainer;
    public GameObject progressPrefab;
    public Button upgradeButton;

    [Header("Referencias de Lógica")]
    public MachineController machine;

    private int currentLevel => machine.GetCurrentLevel();
    private int imageCont = 0;
    private bool isPoor = false;

    void Start()
    {
        RefreshUI();
        upgradeButton.onClick.AddListener(OnUpgrade);
    }

    void RefreshUI()
    {
        var data = machine.upgradeData;

        title.text = data.machineName;
        levelText.text = currentLevel.ToString();
        description.text = data.descriptions[currentLevel - 1];
        costText.text = "$" + data.costs[Mathf.Clamp(currentLevel - 1, 0, data.costs.Length - 1)];

        if (currentLevel == 3 || currentLevel == 6)
        {
            icon.sprite = data.icons[imageCont];
            imageCont++;
        }

        // Mostrar progreso visual
        foreach (Transform child in progressContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < currentLevel; i++)
            Instantiate(progressPrefab, progressContainer);

        // Activar/desactivar botón según dinero
        /*bool tieneDinero = MoneyController.Instance.HasEnoughMoney(data.costs[currentLevel - 1]);
        upgradeButton.interactable = currentLevel < data.maxLevel && tieneDinero;
        isPoor = !tieneDinero;*/
    }

    void OnUpgrade()
    {
        var data = machine.upgradeData;
        int cost = data.costs[currentLevel - 1];

        if (!MoneyController.Instance.HasEnoughMoney(cost))
        {
            ToastManager.Instance.ShowError("Dinero insuficiente para la compra");
            Debug.Log("💸 No hay suficiente dinero.");
            return;
        }

        // Descuenta el dinero
        MoneyController.Instance.RemoveMoney(cost);

        // Realiza la mejora
        FactoryUpgradeManager.Instance.UpgradeMachine(machine);

        ToastManager.Instance.ShowSuccess($"{machine.upgradeData.machineName} mejorada a nivel {currentLevel + 1}");
        RefreshUI();
    }
}
