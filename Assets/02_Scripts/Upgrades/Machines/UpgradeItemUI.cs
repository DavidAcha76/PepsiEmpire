using TMPro;
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
    public Transform progressContainer;
    public GameObject progressPrefab;
    public Button upgradeButton;

    [Header("Referencias de Lógica")]
    public MachineController machine;

    private int currentLevel => machine.GetCurrentLevel();
    private int imageCont = 0;

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
        costText.text = "$" + data.costs[currentLevel - 1];
        if (currentLevel == 3 || currentLevel == 6)
        {
            icon.sprite = data.icons[imageCont];
            imageCont++;
        }

        if (currentLevel != 1) MoneyController.Instance.RemoveMoney(data.costs[currentLevel - 2]);



        foreach (Transform child in progressContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < currentLevel; i++)
            Instantiate(progressPrefab, progressContainer);

        upgradeButton.interactable = currentLevel < data.maxLevel;
    }

    void OnUpgrade()
    {
        FactoryUpgradeManager.Instance.UpgradeMachine(machine);
        RefreshUI();
    }
}
