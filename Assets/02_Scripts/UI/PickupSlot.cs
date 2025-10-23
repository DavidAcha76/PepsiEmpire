using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickupSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image itemImage;
    public TextMeshProUGUI cantidadText;

    [Header("Ghost prefab")]
    public Image dragGhostPrefab;

    [Header("Runtime")]
    public ItemData currentItem;
    public int cantidadActual = 0;

    private GameObject _canvas;
    private Transform _root;
    private Image _ghost;

    private void Awake()
    {
        _canvas = GameObject.FindGameObjectWithTag("RootUI");
        _root = _canvas ? _canvas.transform : transform;


        if (cantidadText) cantidadText.text = "";
    }

    // --------------------
    //   LOGICA DE ITEM
    // --------------------
    public void SetItem(ItemData item, int cantidad = 1)
    {
        currentItem = item;
        cantidadActual = Mathf.Max(1, cantidad);

        if (itemImage)
        {
            itemImage.sprite = item.icon;
            itemImage.enabled = true;
            itemImage.color = Color.white;
        }
        if (cantidadText)
            cantidadText.text = cantidadActual > 1 ? cantidadActual.ToString() : "";
    }

    public void Clear()
    {
        currentItem = null;
        cantidadActual = 0;
        if (itemImage) itemImage.enabled = false;
        if (cantidadText) cantidadText.text = "";
    }

    public bool IsEmpty() => currentItem == null;

    // --------------------
    //   DRAG / DROP
    // --------------------
    public void OnBeginDrag(PointerEventData e)
    {
        if (IsEmpty()) return;

        _ghost = Instantiate(dragGhostPrefab, _root);
        _ghost.raycastTarget = false;
        _ghost.sprite = itemImage.sprite;
        _ghost.color = itemImage.color;

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
        //_cg.blocksRaycasts = true; // <- reactiva interacción normal
    }

    void UpdateGhostPos(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _root as RectTransform, e.position, e.pressEventCamera, out var local);
        (_ghost.transform as RectTransform).anchoredPosition = local;
    }

}
