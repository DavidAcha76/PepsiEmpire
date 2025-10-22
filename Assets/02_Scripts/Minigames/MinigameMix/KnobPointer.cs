using UnityEngine;
using UnityEngine.EventSystems;

public class KnobPointer : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Refs")]
    public RectTransform handle;
    public RectTransform knobRoot;
    public float radius = 80f;

    public System.Action<Vector2> OnDirection;
    public System.Action OnDown;
    public System.Action OnUp;

    private bool dragging;
    private Vector2 lastDir;

    // ==========================
    //   EVENTOS UI
    // ==========================
    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;

        Vector2 local = ScreenToLocal(eventData);
        PositionHandleLocal(local);
        UpdateDirection(local);

        OnDown?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        Vector2 local = ScreenToLocal(eventData);
        PositionHandleLocal(local);
        UpdateDirection(local);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;

        // Reset visual del handle
        if (handle)
        {
            handle.anchoredPosition = Vector2.zero;
            handle.localEulerAngles = Vector3.zero;
        }

        lastDir = Vector2.right;
        OnUp?.Invoke();
    }

    // ==========================
    //   MÉTODOS DE APOYO
    // ==========================
    private Vector2 ScreenToLocal(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            knobRoot,
            eventData.position,
            eventData.pressEventCamera,
            out var local
        );
        return local;
    }

    private void PositionHandleLocal(Vector2 local)
    {
        if (!handle) return;
        Vector2 clamped = local.normalized * Mathf.Min(local.magnitude, radius);
        handle.anchoredPosition = clamped;
        float ang = Mathf.Atan2(clamped.y, clamped.x) * Mathf.Rad2Deg;
        handle.localEulerAngles = new Vector3(0, 0, ang);
    }

    private void UpdateDirection(Vector2 local)
    {
        if (local.sqrMagnitude < 0.0001f)
            local = Vector2.right;

        Vector2 dir = local.normalized;
        OnDirection?.Invoke(dir);
        lastDir = dir;
    }
}
