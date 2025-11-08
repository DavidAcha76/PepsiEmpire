using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)] public string text;
    public AudioClip voiceClip; // 🎙️ voz por línea (opcional)
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Portrait Settings")]
    public Image portraitImage;
    public CanvasGroup portraitCanvas;
    public Sprite guideSprite;

    [Header("Audio Settings")]
    public AudioSource voiceSource;
    public float voiceVolume = 1f;

    [Header("Configuración")]
    public float typingSpeed = 0.03f;
    public float autoAdvanceDelay = 1.0f; // tiempo entre líneas si es automático
    public bool autoAdvance = true;       // activar avance automático
    public DialogueLine[] startingDialogue;

    private Queue<DialogueLine> lines = new Queue<DialogueLine>();
    private bool isTyping = false;
    private Coroutine typingRoutine;
    private Coroutine fadeRoutine;
    private bool waitingForAdvance = false;
    public bool isPausedByTutorial = false;
    private string currentLineText = "";

    void Start()
    {
        if (portraitCanvas != null)
            portraitCanvas.alpha = 0;

        if (voiceSource != null)
        {
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.volume = voiceVolume;
        }

        if (startingDialogue.Length > 0)
            StartDialogue(startingDialogue);
    }

    public void StartDialogue(DialogueLine[] dialogueLines)
    {
        dialoguePanel.SetActive(true);
        lines.Clear();

        foreach (var line in dialogueLines)
            lines.Enqueue(line);

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (isPausedByTutorial || isTyping) return;

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        var line = lines.Dequeue();
        speakerNameText.text = line.speakerName;
        string speaker = line.speakerName.Trim().ToLower();

        // --- Retrato de la Guía ---
        if (portraitImage && portraitCanvas)
        {
            bool isGuide = (speaker.Contains("guia") || speaker.Contains("guía"));
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            if (isGuide)
            {
                portraitImage.sprite = guideSprite;
                fadeRoutine = StartCoroutine(FadeInPortrait());
            }
            else fadeRoutine = StartCoroutine(FadeOutPortrait());
        }

        // --- Reproducir voz ---
        if (voiceSource != null && line.voiceClip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = line.voiceClip;
            voiceSource.Play();
        }

        // --- Mostrar texto letra a letra ---
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLineCoroutine(line.text));

        // --- Detección de pasos del tutorial ---
        if (speaker.Contains("guia") || speaker.Contains("guía"))
        {
            string texto = line.text.ToLower();
            var tutorial = FindFirstObjectByType<TutorialManager>();
            if (tutorial != null)
            {
                if (texto.Contains("máquina de energía") || texto.Contains("maquina de energia"))
                {
                    tutorial.StartStep("MAquinaVolteos");
                    PauseDialogue();
                }
                else if (texto.Contains("máquina de esencia") || texto.Contains("maquina de esencia"))
                {
                    tutorial.StartStep("minaEsenciaLvl1");
                    PauseDialogue();
                }
                else if (texto.Contains("máquina de ingredientes") || texto.Contains("maquina de ingredientes"))
                {
                    tutorial.StartStep("InventarioIngredientes");
                    PauseDialogue();
                }
                else if (texto.Contains("mezcladora"))
                {
                    tutorial.StartStep("pepsiMezcladoraLvl1");
                    PauseDialogue();
                }
                else if (texto.Contains("embotelladora"))
                {
                    tutorial.StartStep("pepsienvasadoraLvl1");
                    PauseDialogue();
                }
            }
        }
    }

    IEnumerator TypeLineCoroutine(string text)
    {
        isTyping = true;
        waitingForAdvance = false;
        currentLineText = text;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Esperar a que el audio termine
        if (voiceSource != null && voiceSource.isPlaying)
            yield return new WaitWhile(() => voiceSource.isPlaying);

        // Activar espera para avanzar (manual o automática)
        waitingForAdvance = true;

        if (autoAdvance)
            StartCoroutine(AutoAdvanceCoroutine());
    }

    IEnumerator AutoAdvanceCoroutine()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        if (!isPausedByTutorial)
        {
            waitingForAdvance = false;
            DisplayNextLine();
        }
    }

    public void PauseDialogue()
    {
        isPausedByTutorial = true;
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Pause();
    }

    public void ResumeDialogue()
    {
        isPausedByTutorial = false;
        if (voiceSource != null && voiceSource.clip != null)
            voiceSource.UnPause();

        DisplayNextLine();
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf || isPausedByTutorial) return;

        // Espacio para avanzar manualmente
        if (waitingForAdvance && Input.GetKeyDown(KeyCode.Space))
        {
            waitingForAdvance = false;
            DisplayNextLine();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (portraitCanvas != null)
            portraitCanvas.alpha = 0;

        if (voiceSource != null)
            voiceSource.Stop();

        // 🎬 Fade out cinematográfico antes de pasar a la siguiente escena
        StartCoroutine(FadeAndLoadNextScene());
    }

    IEnumerator FadeAndLoadNextScene()
    {
        // Crear overlay temporal de fade
        GameObject fadeObj = new GameObject("FadeCanvas");
        Canvas canvas = fadeObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasGroup cg = fadeObj.AddComponent<CanvasGroup>();

        Image img = new GameObject("FadeImage").AddComponent<Image>();
        img.transform.SetParent(fadeObj.transform, false);
        img.color = Color.black;
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.rectTransform.offsetMin = Vector2.zero;
        img.rectTransform.offsetMax = Vector2.zero;

        float duration = 1.5f;
        float t = 0f;
        float startVolume = AudioListener.volume;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            AudioListener.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        cg.alpha = 1f;
        AudioListener.volume = 0f;
        yield return new WaitForSeconds(0.4f);

        SceneManager.LoadScene("LaFabrica"); // ⚙️ cambia el nombre según tu juego
    }

    IEnumerator FadeInPortrait()
    {
        while (portraitCanvas.alpha < 1f)
        {
            portraitCanvas.alpha += Time.deltaTime * 2f;
            yield return null;
        }
        portraitCanvas.alpha = 1f;
    }

    IEnumerator FadeOutPortrait()
    {
        while (portraitCanvas.alpha > 0f)
        {
            portraitCanvas.alpha -= Time.deltaTime * 2f;
            yield return null;
        }
        portraitCanvas.alpha = 0f;
    }
}
