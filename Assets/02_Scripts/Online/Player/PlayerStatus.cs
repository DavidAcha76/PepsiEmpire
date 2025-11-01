using Fusion;
using UnityEngine;

/// <summary>
/// Estado básico de un jugador. El host actualiza IsFinished cuando el jugador termina.
/// </summary>
[DisallowMultipleComponent]
public class PlayerStatus : NetworkBehaviour
{
    /// <summary>
    /// True si este jugador ya terminó y pasa a modo espectador.
    /// </summary>
    [Networked] public bool IsFinished { get; set; }
}
