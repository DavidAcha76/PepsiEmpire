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
    public Button upgradeButton;
    public Transform progressContainer; // contenedor con franjas
    public GameObject progressPrefab;   // prefab de franja roja

    [Header("Lógica")]
    public UpgradeData upgradeData; // Scriptable con niveles

    private int currentLevel = 0;

    void Start()
    {
        RefreshUI();
        upgradeButton.onClick.AddListener(OnUpgrade);
    }

    void RefreshUI()
    {
        title.text = upgradeData.title;
        description.text = upgradeData.descriptions[currentLevel];
        costText.text = "$" + upgradeData.costs[currentLevel].ToString();

        // limpiar y regenerar franjas
        foreach (Transform child in progressContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < currentLevel; i++)
            Instantiate(progressPrefab, progressContainer);

        // desactivar botón si está al máximo
        upgradeButton.interactable = currentLevel < upgradeData.maxLevel;
    }

    void OnUpgrade()
    {
        if (currentLevel >= upgradeData.maxLevel) return;

        // pagar costo (aquí integrar tu sistema de dinero)
        // if (!PlayerStats.TrySpend(upgradeData.costs[currentLevel])) return;

        currentLevel++;
        RefreshUI();

        // Cambiar prefab en escena (llamada a manager)
        FactoryUpgradeManager.Instance.ApplyUpgrade(upgradeData, currentLevel);

        Debug.Log($"🔧 Mejora '{upgradeData.title}' aplicada. Nivel: {currentLevel}");
    }
}
