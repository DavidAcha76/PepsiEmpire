using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndCinematicToMenu : MonoBehaviour
{
    [Header("🎞️ Video y escena destino")]
    public VideoPlayer videoPlayer;           // Asigna tu VideoPlayer aquí
    public string menuSceneName = "Menu";     // Nombre exacto de tu escena de menú

    void Start()
    {
        // Si no lo asignas manualmente, lo busca automáticamente
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Se ejecuta cuando el video termina
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnCinematicEnd;
        else
            Debug.LogWarning("No se encontró un VideoPlayer en este objeto.");
    }

    private void OnCinematicEnd(VideoPlayer vp)
    {
        Debug.Log("🎬 Cinemática terminada. Volviendo al menú...");
        SceneManager.LoadScene(menuSceneName);
    }
}
