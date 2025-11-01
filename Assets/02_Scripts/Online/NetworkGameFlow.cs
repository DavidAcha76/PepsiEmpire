using Fusion;
using UnityEngine;

/// <summary>
/// Coordina el flujo de juego: cuando TODOS los jugadores activos terminaron,
/// el Host carga la escena NextScene para todos.
/// Debe existir UNA instancia con StateAuthority (el Host).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject))]
public class NetworkGameFlow : NetworkBehaviour
{
    [Header("Escena destino al finalizar todos")]
    [Tooltip("Escena a cargar cuando todos los jugadores tengan IsFinished = true")]
    public SceneRef NextScene;

    [Header("Opciones")]
    [SerializeField] private bool requireAtLeastOnePlayer = true;

    // Guard para evitar llamados múltiples
    private bool _advanceTriggered;

    private static NetworkGameFlow _instance;
    public static NetworkGameFlow Instance => _instance;

    private void Awake()
    {
        // Singleton plano (solo referencia, no persiste entre escenas)
        _instance = this;
    }

    /// <summary>
    /// Llamado por el Host al marcar un jugador como terminado.
    /// Revisa si todos terminaron y, si es así, carga la escena NextScene.
    /// </summary>
    public void Host_TryAdvanceIfAllFinished()
    {
        if (!Object || !Object.HasStateAuthority) return;
        if (_advanceTriggered) return; // ya disparado

        // Validar Runner
        var runner = Runner;
        if (runner == null) return;

        int total = 0;
        int finished = 0;

        foreach (var p in runner.ActivePlayers)
        {
            if (!runner.TryGetPlayerObject(p, out var no) || no == null)
                continue;

            var st = no.GetComponent<PlayerStatus>();
            if (st == null)
                continue;

            total++;
            if (st.IsFinished)
                finished++;
        }

        if (requireAtLeastOnePlayer && total == 0)
            return;

        if (total > 0 && finished == total)
        {
            _advanceTriggered = true;
            if (NextScene.IsValid)
            {
                // Carga sincronizada para todos
                runner.LoadScene(NextScene);
            }
            else
            {
                Debug.LogWarning("[NetworkGameFlow] NextScene no asignada o inválida.");
                _advanceTriggered = false; // permite reintentar si asignas después (opcional)
            }
        }
    }
}
