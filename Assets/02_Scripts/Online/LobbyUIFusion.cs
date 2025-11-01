using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIFusion : MonoBehaviour
{
    [Header("Referencias")]
    public NetworkGameLauncher launcher;  // arrástralo desde NetworkSystems
    public Canvas mainCanvas;             // tu Canvas principal

    [Header("UI")]
    public Button hostButton;
    public Button joinButton;
    public Button quickJoinButton;
    public TMP_InputField roomNameInput;  // Arrastra aquí tu InputField desde el Inspector

    [Header("Opciones")]
    public string defaultRoomName = "Room-01";

    void Awake()
    {
        if (launcher == null) launcher = FindObjectOfType<NetworkGameLauncher>(includeInactive: true);
        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>(includeInactive: true);
    }

    void Start()
    {
        if (hostButton)
        {
            hostButton.onClick.RemoveAllListeners();
            hostButton.onClick.AddListener(async () => await Host());
        }

        if (joinButton)
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(async () => await Join());
        }

        if (quickJoinButton)
        {
            quickJoinButton.onClick.RemoveAllListeners();
            quickJoinButton.onClick.AddListener(async () => await QuickJoin());
        }

        // Configura el placeholder si existe
        if (roomNameInput != null && roomNameInput.placeholder != null)
        {
            var placeholderText = roomNameInput.placeholder.GetComponent<TextMeshProUGUI>();
            if (placeholderText != null)
            {
                placeholderText.text = "Escribe el nombre de la sala";
            }
        }
    }

    // ===== Obtener nombre de sala =====
    private string RoomNameOrDefault()
    {
        string name = roomNameInput ? roomNameInput.text : null;
        return string.IsNullOrWhiteSpace(name) ? defaultRoomName : name.Trim();
    }

    // ===== Acciones =====
    private async Task Host()
    {
        if (launcher == null)
        {
            Debug.LogError("[LobbyUI] Falta NetworkGameLauncher.");
            return;
        }
        SetInteractable(false);
        await launcher.StartHost(RoomNameOrDefault());
        SetInteractable(true);
    }

    private async Task Join()
    {
        if (launcher == null)
        {
            Debug.LogError("[LobbyUI] Falta NetworkGameLauncher.");
            return;
        }
        SetInteractable(false);
        await launcher.StartClientAndJoin(RoomNameOrDefault());
        SetInteractable(true);
    }

    private async Task QuickJoin()
    {
        if (launcher == null)
        {
            Debug.LogError("[LobbyUI] Falta NetworkGameLauncher.");
            return;
        }
        SetInteractable(false);
        await launcher.QuickJoinOrCreate(RoomNameOrDefault());
        SetInteractable(true);
    }

    private void SetInteractable(bool value)
    {
        if (hostButton) hostButton.interactable = value;
        if (joinButton) joinButton.interactable = value;
        if (quickJoinButton) quickJoinButton.interactable = value;
        if (roomNameInput) roomNameInput.interactable = value;
    }
}
