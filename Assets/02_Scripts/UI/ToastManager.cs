using UnityEngine;
using UnityEngine.WSA;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance;
    public ToastUI toastPrefab;
    public Transform toastParent; // Asigna el Canvas principal

    private ToastUI activeToast;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void ShowToast(string message, Color color)
    {
        if (activeToast == null)
            activeToast = Instantiate(toastPrefab, toastParent);

        activeToast.Show(message, color);
    }

    // 🔹 Atajos
    public void ShowSuccess(string message)
        => ShowToast(message, new Color(0.2f, 0.8f, 0.2f, 0.8f)); // Verde

    public void ShowError(string message)
        => ShowToast(message, new Color(0.8f, 0.1f, 0.1f, 0.8f)); // Rojo

    public void ShowInfo(string message)
        => ShowToast(message, new Color(0.2f, 0.4f, 1f, 0.8f)); // Azul
}
