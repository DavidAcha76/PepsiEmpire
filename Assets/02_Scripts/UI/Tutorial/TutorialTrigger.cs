using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    private bool stepCompleted = false;
    private DialogueManager dialogue;

    void Awake()
    {
        dialogue = FindFirstObjectByType<DialogueManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Cuando entra al trigger, completa el paso, pero NO reanuda todavía
        if (!stepCompleted)
        {
            var tutorial = FindFirstObjectByType<TutorialManager>();
            if (tutorial != null)
            {
                tutorial.CompleteStep();
                Debug.Log($"[TutorialTrigger] Paso completado en {gameObject.name}");
            }
            stepCompleted = true;
        }

        // Pausar mientras esté dentro (por si acaso)
        if (dialogue != null)
        {
            dialogue.PauseDialogue();
            Debug.Log($"[TutorialTrigger] Diálogo pausado al entrar en {gameObject.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Reanudar cuando salga del trigger
        if (dialogue != null)
        {
            dialogue.ResumeDialogue();
            Debug.Log($"[TutorialTrigger] Diálogo reanudado al salir de {gameObject.name}");
        }
    }
}
