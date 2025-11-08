using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Referencias")]
    public DialogueManager dialogueManager;     // Asigna el DialogueManager en el Inspector
    public GameObject markerPrefab;             // Prefab del marcador (tu flecha o icono flotante)

    private GameObject activeMarker;
    private Transform currentTarget;

    /// <summary>
    /// Inicia un paso del tutorial buscando el objeto por nombre.
    /// Ejemplo: StartStep("MaquinaEnergia");
    /// </summary>
    public void StartStep(string objectName)
    {
        GameObject target = null;

        // 🔹 1. Buscar primero en toda la escena (por si está fuera del FactoryMachinesRoot)
        target = GameObject.Find(objectName);

        // 🔹 2. Si no lo encuentra directamente, busca dentro de FactoryMachinesRoot
        if (target == null)
        {
            Transform factoryRoot = GameObject.Find("FactoryMachinesRoot")?.transform;
            if (factoryRoot != null)
            {
                foreach (Transform child in factoryRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.Equals(objectName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        target = child.gameObject;
                        break;
                    }
                }
            }
        }

        // 🔹 3. Si aún no lo encuentra, hace una búsqueda global más flexible (por contiene)
        if (target == null)
        {
            foreach (var obj in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj.name.ToLower().Contains(objectName.ToLower()))
                {
                    target = obj;
                    break;
                }
            }
        }

        // 🔹 4. Si lo encuentra, muestra marcador; si no, continúa el diálogo
        if (target != null)
        {
            ShowMarker(target.transform);
            Debug.Log($"🟢 Tutorial: mostrando marcador sobre {target.name}");
        }
        else
        {
            Debug.LogWarning($"❌ No se encontró el objeto '{objectName}' en la escena.");
            dialogueManager.ResumeDialogue();
        }
    }



    /// <summary>
    /// Muestra el marcador sobre el objetivo.
    /// </summary>
    public void ShowMarker(Transform target)
    {
        HideMarker();

        currentTarget = target;
        activeMarker = Instantiate(markerPrefab);

        // 🧭 Calcula dirección hacia la cámara principal
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("❌ No se encontró la cámara principal.");
            return;
        }

        // Vector que apunta desde el objeto hacia la cámara
        Vector3 dirToCamera = (cam.transform.position - target.position).normalized;

        // 🔹 Empuja el marcador hacia la cámara (nunca dentro del muro)
        // La altura (2.5f) puedes ajustar según el tamaño de la máquina
        Vector3 offset = dirToCamera * 0.7f + Vector3.up * 2.5f;

        // Posición final
        activeMarker.transform.position = target.position + offset;

        // 🔹 Hacer que el marcador mire siempre hacia la cámara
        activeMarker.transform.LookAt(cam.transform);
        activeMarker.transform.rotation = Quaternion.LookRotation(-activeMarker.transform.forward, Vector3.up);

        // 🔹 Escala normal del prefab (no hereda nada raro)
        activeMarker.transform.localScale = Vector3.one;

        Debug.Log($"🟢 Marcador colocado frente a {target.name}, mirando a la cámara");
    }



    /// <summary>
    /// Llamado cuando el jugador completa el paso (desde el trigger de la máquina).
    /// </summary>
    public void CompleteStep()
    {
        HideMarker();

        if (dialogueManager != null)
        {
            dialogueManager.ResumeDialogue();
            Debug.Log("✅ Paso completado, diálogo reanudado.");
        }
        else
        {
            // En caso de no tener referencia directa, lo busca (nuevo método Unity 6)
            DialogueManager dm = FindFirstObjectByType<DialogueManager>();
            if (dm != null)
            {
                dm.ResumeDialogue();
                Debug.Log("✅ Paso completado (encontrado por tipo).");
            }
        }
    }

    /// <summary>
    /// Destruye el marcador activo, si lo hay.
    /// </summary>
    public void HideMarker()
    {
        if (activeMarker != null)
        {
            Destroy(activeMarker);
            activeMarker = null;
        }
    }
}
