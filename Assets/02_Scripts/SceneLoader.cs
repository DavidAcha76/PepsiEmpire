using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    void Awake()
    {
        // ?? Asegura una sola instancia global
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // persiste entre escenas
    }

    /// <summary>
    /// Carga una escena por nombre.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("? [SceneLoader] Nombre de escena vacío o nulo.");
            return;
        }

        Debug.Log($"?? Cargando escena: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Carga una escena por índice del Build Settings.
    /// </summary>
    public void LoadScene(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"? [SceneLoader] Índice inválido: {buildIndex}");
            return;
        }

        Debug.Log($"?? Cargando escena (índice): {buildIndex}");
        SceneManager.LoadScene(buildIndex);
    }

    /// <summary>
    /// Carga la escena actual desde cero (reiniciar nivel).
    /// </summary>
    public void ReloadCurrentScene()
    {
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Debug.Log($"?? Reiniciando escena: {currentScene.name}");
    }

    /// <summary>
    /// Carga una escena de manera asíncrona (con barra de carga opcional).
    /// </summary>
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    private System.Collections.IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            Debug.Log($"? Progreso de carga: {op.progress * 100f:F1}%");
            yield return null;
        }

        Debug.Log($"? Escena {sceneName} cargada completamente.");
    }
}
