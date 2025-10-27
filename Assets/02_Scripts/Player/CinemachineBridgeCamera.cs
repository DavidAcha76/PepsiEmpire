using UnityEngine;
using Unity.Cinemachine;
using Rewired;


public class CinemachineBridgeCamera : MonoBehaviour
{
    [Header("Rewired")]
    public int playerId = 0;
    public string lookHorizontal = "Camera Horizontal";
    public string lookVertical = "Camera Vertical";

    [Header("Sensibilidad")]
    public float horizontalSpeed = 200f;
    public float verticalSpeed = 2f;
    public float zoomSpeed = 2f;
    public float minRadius = 1.5f;
    public float maxRadius = 6f;

    private Player player;
    private CinemachineCamera cineCam;
    private CinemachineOrbitalFollow orbital;

    void Awake()
    {
        player = ReInput.players.GetPlayer(playerId);
        cineCam = GetComponent<CinemachineCamera>();
        orbital = cineCam.GetComponent<CinemachineOrbitalFollow>();
    }

    void LateUpdate()
    {
        if (player == null || orbital == null) return;

        float lookX = player.GetAxis(lookHorizontal);
        float lookY = player.GetAxis(lookVertical);

        // --- ROTACIÓN HORIZONTAL ---
        var h = orbital.HorizontalAxis;          // copia del struct
        h.Value += lookX * horizontalSpeed * Time.deltaTime;
        orbital.HorizontalAxis = h;              // reasigna

        // --- ROTACIÓN VERTICAL ---
        var v = orbital.VerticalAxis;
        v.Value = Mathf.Clamp(
            v.Value - lookY * verticalSpeed * Time.deltaTime,
            -80f, 80f
        );
        orbital.VerticalAxis = v;
    }
}
