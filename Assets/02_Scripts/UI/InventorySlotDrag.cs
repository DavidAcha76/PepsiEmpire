using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public InventorySlot slot;     
    public Image dragGhostPrefab;  

    Canvas _canvas;
    CanvasGroup _ghostCG;
    Image _ghost;
    Transform _root;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _root = _canvas ? _canvas.transform : transform;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (slot == null || slot.IsEmpty()) return;

        _ghost = Instantiate(dragGhostPrefab, _root);
        _ghost.raycastTarget = false;
        _ghost.sprite = slot.itemImage.sprite;
        _ghost.color = slot.itemImage.color;

        _ghostCG = _ghost.gameObject.GetComponent<CanvasGroup>();
        if (!_ghostCG) _ghostCG = _ghost.gameObject.AddComponent<CanvasGroup>();
        _ghostCG.alpha = 0.9f;

        UpdateGhostPos(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (_ghost) UpdateGhostPos(e);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (_ghost) Destroy(_ghost.gameObject);
        _ghost = null;
    }

    void UpdateGhostPos(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _root as RectTransform, e.position, e.pressEventCamera, out var local);
        (_ghost.transform as RectTransform).anchoredPosition = local;
    }
}
