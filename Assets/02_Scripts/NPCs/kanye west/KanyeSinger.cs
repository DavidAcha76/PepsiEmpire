using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class KanyeSinger : MonoBehaviour
{
    [Header("🎵 Canciones de Kanye")]
    public AudioClip[] canciones;

    [Header("🎬 Animación de canto")]
    public Animator animator;
    public string cantarTrigger = "Sing"; // deja vacío si solo tiene una animación fija

    [Header("🎚️ Configuración")]
    public float radioAudible = 12f;
    public float cambioCancionCada = 30f;
    public bool reproducirLoop = true;

    private AudioSource audioSource;
    private Transform player;
    private float tiempoProximaCancion;
    private bool playerCerca = false;
    private bool estaCantando = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.maxDistance = radioAudible;
        audioSource.loop = false;

        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radioAudible;
    }

    IEnumerator Start()
    {
        // Esperar hasta que el Player aparezca
        while (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null && p != this.gameObject) // 👈 evita auto-detectarse
            {
                player = p.transform;
                Debug.Log("[KanyeSinger] Player detectado: " + player.name);
                break;
            }
            yield return new WaitForSeconds(0.25f);
        }

        if (player == null)
            Debug.LogWarning("[KanyeSinger] No se encontró Player después de esperar.");
    }


    void Update()
    {
        if (player == null) return; // seguridad
        if (!playerCerca || canciones.Length == 0) return;

        // ⏭️ Cambiar canción si terminó o pasó el tiempo
        if (!audioSource.isPlaying && reproducirLoop)
            ReproducirCancionRandom();

        if (Time.time >= tiempoProximaCancion && reproducirLoop)
            ReproducirCancionRandom();
    }

    void OnTriggerEnter(Collider other)
    {
        if (player == null) return;
        if (other.transform == player)
        {
            playerCerca = true;
            if (!estaCantando)
            {
                ReproducirCancionRandom();
                ActivarAnimacion();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (player == null) return;
        if (other.transform == player)
        {
            playerCerca = false;
            audioSource.Stop();
            DetenerAnimacion();
            estaCantando = false;
        }
    }

    void ReproducirCancionRandom()
    {
        if (canciones.Length == 0) return;
        int randomIndex = Random.Range(0, canciones.Length);
        audioSource.clip = canciones[randomIndex];
        audioSource.Play();
        tiempoProximaCancion = Time.time + cambioCancionCada;
        estaCantando = true;
    }

    void ActivarAnimacion()
    {
        if (animator && !string.IsNullOrEmpty(cantarTrigger))
        {
            animator.SetTrigger(cantarTrigger);
        }
    }

    void DetenerAnimacion()
    {
        if (animator)
        {
            animator.ResetTrigger(cantarTrigger);
        }
    }
}
