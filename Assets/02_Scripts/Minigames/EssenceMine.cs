using UnityEngine;
using System.Collections;

public class EssenceMine : MonoBehaviour
{
    [Header("Configuración")]
    public ItemData itemGenerado;
    public int cantidadPorCiclo = 1;
    public float tiempoGeneracion = 10f;
    public int maxCapacidad = 5;

    [Header("Estado actual (runtime)")]
    public int cantidadActual = 0;
    private bool generando = false;
    private Coroutine generacionCo;

    [Header("Referencias")]
    public PickupSlot pickupPrefab;
    public CameraTargetSwitcher cameraTargetSwitcher;
    private GameObject spawnUIParent;
    private PickupSlot pickupActual;

    private void Awake()
    {
        spawnUIParent = GameObject.FindWithTag("RootUI");
        if (!cameraTargetSwitcher)
            cameraTargetSwitcher = FindAnyObjectByType<CameraTargetSwitcher>();
    }

    private void OnEnable() => StartGeneracion();
    private void OnDisable() => StopGeneracion();

    private void Update()
    {
        if (pickupActual && pickupActual.IsEmpty() && pickupActual.gameObject.activeSelf && cantidadActual > 0)
        {
            if (cameraTargetSwitcher)
                cameraTargetSwitcher.ReturnToPlayer();

            VaciarMina();
            return;
        }

        if (pickupActual && pickupActual.gameObject.activeSelf && cantidadActual > 0 && itemGenerado != null)
        {
            pickupActual.SetItem(itemGenerado, cantidadActual);
        }
    }

    // -----------------------------------------
    // Generación automática
    // -----------------------------------------
    public void StartGeneracion()
    {
        if (generando || itemGenerado == null) return;
        generando = true;
        generacionCo = StartCoroutine(CicloGeneracion());
    }

    public void StopGeneracion()
    {
        generando = false;
        if (generacionCo != null) StopCoroutine(generacionCo);
        generacionCo = null;
    }

    private IEnumerator CicloGeneracion()
    {
        while (generando)
        {
            yield return new WaitForSeconds(tiempoGeneracion);

            if (cantidadActual < maxCapacidad)
            {
                cantidadActual += cantidadPorCiclo;

                if (pickupActual && pickupActual.gameObject.activeSelf && itemGenerado != null)
                {
                    pickupActual.SetItem(itemGenerado, cantidadActual);
                }
            }
        }
    }

    // -----------------------------------------
    // Acelerar producción
    // -----------------------------------------
    public void AcelerarProduccion(float factor)
    {
        tiempoGeneracion = Mathf.Max(0.5f, tiempoGeneracion * factor);
    }

    // -----------------------------------------
    // Mostrar / Ocultar Pickup
    // -----------------------------------------
    public void MostrarPickup()
    {
        if (!pickupActual)
            pickupActual = Instantiate(pickupPrefab, spawnUIParent.transform, false);

        pickupActual.gameObject.SetActive(true);
        if (cantidadActual > 0 && itemGenerado != null)
        {
            pickupActual.SetItem(itemGenerado, cantidadActual);
        }
        else
        {
            var img = pickupActual.GetComponent<UnityEngine.UI.Image>();
            if (img) img.enabled = true; 
        }
    }

    public void OcultarPickup()
    {
        if (pickupActual)
            pickupActual.gameObject.SetActive(false);
    }

    // -----------------------------------------
    // Vaciar la mina (cuando se recoge el ítem)
    // -----------------------------------------
    private void VaciarMina()
    {
        cantidadActual = 0;

        if (pickupActual)
        {
            Destroy(pickupActual.gameObject);
            pickupActual = null;
        }
    }
}
