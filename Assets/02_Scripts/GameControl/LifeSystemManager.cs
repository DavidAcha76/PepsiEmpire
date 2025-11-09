using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class LifeSystemManager : MonoBehaviour
{

    // ========== SINGLETON ==========

    public static LifeSystemManager Instance { get; private set; }

    // ========== CONFIGURACIÓN UI ==========

    [Header("▼ Referencias de Corazones (Images)")]

    [Tooltip("Arrastra aquí los 5 componentes Image de los corazones")]

    public Image[] heartImages = new Image[5];

    [Header("▼ Sprites de Vida")]

    [Tooltip("Sprite cuando el corazón está lleno")]

    public Sprite heartFullSprite;

    [Tooltip("Sprite cuando el corazón está vacío")]

    public Sprite heartEmptySprite;

    // ========== CONFIGURACIÓN DEL SISTEMA ==========

    [Header("▼ Configuración de Vidas")]

    [SerializeField] private int maxLives = 5;

    [SerializeField] private int currentLives = 5;

    [Header("▼ Efectos Visuales")]

    [SerializeField] private bool useAnimation = true;

    [SerializeField] private float animationDuration = 0.3f;

    public AudioSource audioSource;
    public AudioClip clip;

    // ========== EVENTOS ==========

    [Header("▼ Eventos del Sistema")]

    public UnityEvent OnLifeLost;              // Cuando pierde una vida

    public UnityEvent<int> OnLivesChanged;     // Cuando cambian las vidas (devuelve cantidad)

    public UnityEvent OnGameOver;              // Cuando llega a 0 vidas

    // ========== ESTADO INTERNO ==========

    private bool isGameOver = false;

    // ==========================================

    // INICIALIZACIÓN

    // ==========================================

    void Awake()

    {

        // Configurar Singleton

        if (Instance != null && Instance != this)

        {

            Destroy(gameObject);

            return;

        }

        Instance = this;

        ValidateReferences();

        InitializeLives();

    }

    void Start()

    {

        Debug.Log($"💚 [LifeSystem] Sistema inicializado con {currentLives}/{maxLives} vidas");

    }

    private void ValidateReferences()

    {

        bool hasErrors = false;

        if (heartFullSprite == null)

        {

            Debug.LogError("❌ [LifeSystem] ¡Falta asignar Heart Full Sprite!");

            hasErrors = true;

        }

        if (heartEmptySprite == null)

        {

            Debug.LogError("❌ [LifeSystem] ¡Falta asignar Heart Empty Sprite!");

            hasErrors = true;

        }

        for (int i = 0; i < heartImages.Length; i++)

        {

            if (heartImages[i] == null)

            {

                Debug.LogError($"❌ [LifeSystem] ¡Falta asignar Heart Image #{i + 1}!");

                hasErrors = true;

            }

        }

        if (!hasErrors)

        {

            Debug.Log("✅ [LifeSystem] Todas las referencias están correctas");

        }

    }

    private void InitializeLives()

    {

        currentLives = maxLives;

        UpdateVisualization();

        OnLivesChanged?.Invoke(currentLives);

    }

    // ==========================================

    // MÉTODOS PÚBLICOS - LLAMAR DESDE TU JUEGO

    // ==========================================

    /// <summary>

    /// Llama esto cuando el jugador se equivoca en un pedido

    /// </summary>

    public void LoseLifeByOrderError()

    {

        if (isGameOver) return;

        Debug.Log("❌ [LifeSystem] Vida perdida por ERROR de pedido");

        LoseLife("Error de pedido");

    }

    /// <summary>

    /// Llama esto cuando el NPC se va por timeout

    /// </summary>

    public void LoseLifeByTimeout()

    {

        if (isGameOver) return;

        Debug.Log("⏱️ [LifeSystem] Vida perdida por TIMEOUT del NPC");

        LoseLife("NPC esperó demasiado");

    }

    /// <summary>

    /// Método genérico para perder vida

    /// </summary>

    public void LoseLife(string reason = "")
    {

        if (isGameOver || currentLives <= 0) return;

        currentLives--;

        audioSource.PlayOneShot(clip);
        Debug.Log($"💔 [LifeSystem] Vida perdida" +

                  (string.IsNullOrEmpty(reason) ? "" : $" - {reason}") +

                  $". Vidas restantes: {currentLives}/{maxLives}");

        UpdateVisualization();

        OnLifeLost?.Invoke();

        OnLivesChanged?.Invoke(currentLives);

        // Animación del corazón que se pierde

        if (useAnimation && currentLives >= 0 && currentLives < heartImages.Length)

        {

            AnimateHeartLoss(currentLives);

        }

        // Verificar Game Over

        if (currentLives <= 0)
        {

            TriggerGameOver();
            SceneLoader.Instance.LoadScene("GameOver");

        }

    }

    /// <summary>

    /// Recupera una vida (power-ups, rewards, etc.)

    /// </summary>

    public void GainLife()

    {

        if (isGameOver || currentLives >= maxLives)

        {

            Debug.LogWarning("[LifeSystem] No se puede recuperar vida (ya está al máximo o Game Over)");

            return;

        }

        currentLives++;

        Debug.Log($"💚 [LifeSystem] Vida recuperada. Vidas actuales: {currentLives}/{maxLives}");

        UpdateVisualization();

        OnLivesChanged?.Invoke(currentLives);

    }

    /// <summary>

    /// Establece un número específico de vidas

    /// </summary>

    public void SetLives(int amount)

    {

        int previous = currentLives;

        currentLives = Mathf.Clamp(amount, 0, maxLives);

        Debug.Log($"[LifeSystem] Vidas establecidas: {previous} → {currentLives}");

        UpdateVisualization();

        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0 && !isGameOver)

        {

            TriggerGameOver();

        }

    }

    /// <summary>

    /// Reinicia el sistema al máximo de vidas

    /// </summary>

    public void ResetLives()

    {

        isGameOver = false;

        currentLives = maxLives;

        UpdateVisualization();

        OnLivesChanged?.Invoke(currentLives);

        Debug.Log($"🔄 [LifeSystem] Sistema reiniciado a {maxLives} vidas");

    }

    // ==========================================

    // MÉTODOS DE CONSULTA

    // ==========================================

    /// <summary>

    /// Obtiene las vidas actuales

    /// </summary>

    public int GetCurrentLives() => currentLives;

    /// <summary>

    /// Obtiene las vidas máximas

    /// </summary>

    public int GetMaxLives() => maxLives;

    /// <summary>

    /// Verifica si el jugador está vivo

    /// </summary>

    public bool IsAlive() => currentLives > 0 && !isGameOver;

    /// <summary>

    /// Verifica si está en Game Over

    /// </summary>

    public bool IsGameOver() => isGameOver;

    /// <summary>

    /// Obtiene el porcentaje de vida (0.0 a 1.0)

    /// </summary>

    public float GetLifePercentage()

    {

        if (maxLives == 0) return 0f;

        return (float)currentLives / maxLives;

    }

    // ==========================================

    // LÓGICA INTERNA

    // ==========================================

    private void UpdateVisualization()

    {

        for (int i = 0; i < heartImages.Length; i++)

        {

            if (heartImages[i] == null) continue;

            // Si el índice es menor que currentLives, mostrar corazón lleno

            bool isFull = i < currentLives;

            heartImages[i].sprite = isFull ? heartFullSprite : heartEmptySprite;

            heartImages[i].enabled = true;

            // Resetear escala por si había animación previa

            heartImages[i].transform.localScale = Vector3.one;

        }

    }

    private void AnimateHeartLoss(int heartIndex)

    {

        if (heartIndex < 0 || heartIndex >= heartImages.Length) return;

        if (heartImages[heartIndex] == null) return;

        Transform heartTransform = heartImages[heartIndex].transform;

        StartCoroutine(PulseAnimation(heartTransform));

    }

    private IEnumerator PulseAnimation(Transform target)

    {

        Vector3 originalScale = Vector3.one;

        float halfDuration = animationDuration / 2f;

        // Expandir

        float elapsed = 0f;

        while (elapsed < halfDuration)

        {

            elapsed += Time.deltaTime;

            float t = elapsed / halfDuration;

            target.localScale = Vector3.Lerp(originalScale, originalScale * 1.3f, t);

            yield return null;

        }

        // Contraer

        elapsed = 0f;

        while (elapsed < halfDuration)

        {

            elapsed += Time.deltaTime;

            float t = elapsed / halfDuration;

            target.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale * 0.85f, t);

            yield return null;

        }

        target.localScale = originalScale * 0.85f;

    }

    private void TriggerGameOver()

    {

        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("💀 [LifeSystem] ¡GAME OVER! No quedan vidas");

        OnGameOver?.Invoke();

    }

    // ==========================================

    // DEBUG (Solo en Editor)

    // ==========================================

#if UNITY_EDITOR

    [ContextMenu("Test: Perder 1 Vida")]

    private void TestLoseLife()

    {

        LoseLife("Test manual");

    }

    [ContextMenu("Test: Perder Vida (Error Pedido)")]

    private void TestOrderError()

    {

        LoseLifeByOrderError();

    }

    [ContextMenu("Test: Perder Vida (Timeout)")]

    private void TestTimeout()

    {

        LoseLifeByTimeout();

    }

    [ContextMenu("Test: Recuperar Vida")]

    private void TestGainLife()

    {

        GainLife();

    }

    [ContextMenu("Test: Perder Todas las Vidas")]

    private void TestLoseAll()

    {

        SetLives(0);

    }

    [ContextMenu("Test: Reiniciar Sistema")]

    private void TestReset()

    {

        ResetLives();

    }

#endif

}

