using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string text;
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

    [Header("Configuración")]
    public float typingSpeed = 0.03f;
    public bool autoStart = true;
    public DialogueLine[] startingDialogue;

    private Queue<DialogueLine> lines = new Queue<DialogueLine>();
    private bool isTyping = false;
    private Coroutine typingRoutine;
    private Coroutine fadeRoutine;
    public bool isPausedByTutorial = false;
    private string currentLineText = "";


    void Start()
    {
        if (portraitCanvas != null)
            portraitCanvas.alpha = 0;

        if (autoStart && startingDialogue.Length > 0)
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

        // --- Mostrar retrato de la Guía ---
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

        // --- Mostrar texto ---
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLineCoroutine(line.text));

        // --- Detectar instrucciones del tutorial ---
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
                else if (texto.Contains("máquina de ingredientes") || texto.Contains("máquina de ingredientes"))
                {
                    tutorial.StartStep("MaquinaIngrediente");
                    PauseDialogue();
                }
                else if (texto.Contains("mezcladora") || texto.Contains("mezcladora"))
                {
                    tutorial.StartStep("pepsiMezcladoraLvl1");
                    PauseDialogue();
                }
                else if (texto.Contains("embotelladora") || texto.Contains("embotelladora"))
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
        currentLineText = text;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }


    public void PauseDialogue() => isPausedByTutorial = true;

    public void ResumeDialogue()
    {
        isPausedByTutorial = false;
        DisplayNextLine();
    }

    void Update()
    {
        // Solo responde si el panel del diálogo está activo
        if (!dialoguePanel.activeSelf) return;

        // Detecta clic izquierdo del mouse (o tap en pantalla)
        if (Input.GetMouseButtonDown(0))
        {
            // Si el texto se está escribiendo aún, terminarlo instantáneamente
            if (isTyping)
            {
                StopCoroutine(typingRoutine);
                dialogueText.text = currentLineText;
                isTyping = false;
            }
            else
            {
                DisplayNextLine();
            }
        }
    }


    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        if (portraitCanvas != null)
            portraitCanvas.alpha = 0;
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
