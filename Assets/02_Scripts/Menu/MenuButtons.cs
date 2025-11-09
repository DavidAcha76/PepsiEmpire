using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    // Botón: JUGAR
    public void PlayTutorial()
    {
        // Asegúrate que la escena "Tutorial" está en Build Settings
        SceneManager.LoadScene("Tutorial");
        // Si prefieres por índice: SceneManager.LoadScene(1);
    }

    // Botón: SALIR
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
