using UnityEngine;
using Rewired;
using System.Collections.Generic;

[AddComponentMenu("Input/Rewired InputController")]
public class InputController : MonoBehaviour
{
    [Tooltip("Player ID en Rewired (0 = primer jugador configurado en Rewired)")]
    public int playerId = 0;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private Player _player;              
    private bool _isInitialized = false; 

    private static bool _idsCached = false;
    public static int IdMoveH, IdMoveV;
    public static int IdLookH, IdLookV;
    public static int IdJump, IdRun, IdCrouch, IdInteract;
    public static int IdPause;

    public bool IsInitialized => _isInitialized;

    private void Awake()
    {
        if (!ReInput.isReady)
        {
            StartCoroutine(InitializeWhenReady());
            return;
        }
        Initialize();
    }

    private System.Collections.IEnumerator InitializeWhenReady()
    {
        while (!ReInput.isReady)
            yield return null;

        Initialize();
    }

    private void Initialize()
    {
        _player = ReInput.players.GetPlayer(playerId);

        if (_player == null)
        {
            Debug.LogError($"❌ InputController: No se encontró Player con ID {playerId}. Revisa tu Rewired Input Manager.");
            enabled = false;
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"✓ Rewired Player {playerId} listo");
            Debug.Log($"  - Keyboard: {_player.controllers.hasKeyboard}");
            Debug.Log($"  - Mouse: {_player.controllers.hasMouse}");
        }

        List<ControllerMap> keyboardMaps = new List<ControllerMap>();
        _player.controllers.maps.GetAllMaps(ControllerType.Keyboard, keyboardMaps);

        if (showDebugLogs)
        {
            Debug.Log($"📋 Keyboard Maps encontrados: {keyboardMaps.Count}");
            if (keyboardMaps.Count == 0)
                Debug.LogWarning("⚠ No hay keyboard maps cargados para este Player. Verifica tus Rewired Maps.");
        }

        if (!_idsCached)
        {
            try
            {
                IdMoveH = ReInput.mapping.GetActionId("Move Horizontal");
                IdMoveV = ReInput.mapping.GetActionId("Move Vertical");
                IdLookH = ReInput.mapping.GetActionId("Look Horizontal");
                IdLookV = ReInput.mapping.GetActionId("Look Vertical");
                IdJump = ReInput.mapping.GetActionId("Jump");
                IdRun = ReInput.mapping.GetActionId("Run");
                IdCrouch = ReInput.mapping.GetActionId("Crouch");
                IdInteract = ReInput.mapping.GetActionId("Interact");
                IdPause = ReInput.mapping.GetActionId("Pause");

                _idsCached = true;

                if (showDebugLogs)
                    Debug.Log("✓ Action IDs cacheados correctamente.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error cacheando Action IDs de Rewired: {e.Message}\n" +
                               "¿Los nombres de acciones coinciden exactamente?");
                enabled = false;
                return;
            }
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Devuelve el valor del eje (float) para una acción dada por Id.
    /// </summary>
    public float GetAxis(int actionId)
    {
        if (_player == null) return 0f;
        return _player.GetAxis(actionId);
    }

    /// <summary>
    /// Devuelve true si la acción está presionada (mantenida).
    /// </summary>
    public bool GetButton(int actionId)
    {
        if (_player == null) return false;
        return _player.GetButton(actionId);
    }

    /// <summary>
    /// Devuelve true solo el frame que la acción fue presionada.
    /// </summary>
    public bool GetButtonDown(int actionId)
    {
        if (_player == null) return false;
        return _player.GetButtonDown(actionId);
    }

    /// <summary>
    /// Heurística por si quieres escalar deltas con deltaTime cuando el último controlador activo no es mouse.
    /// </summary>
    public bool ShouldScaleWithDeltaTime()
    {
        if (_player == null) return true;
        var c = _player.controllers.GetLastActiveController();
        return c == null || c.type != ControllerType.Mouse;
    }
}
