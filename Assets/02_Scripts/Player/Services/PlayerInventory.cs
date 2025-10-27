using Rewired;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventorySlot[] slots;
    public int defaultMaxStack = 20;

    private Rewired.Player player; 
    private int selectItemIndex = 0;
    private int previousIndex = 0;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(0);
        /*for (int i = 0; i < slots.Length; i++)
            slots[i].SetHighlight(i == selectItemIndex);*/
    }

    private void Update()
    {
        SelectItem();
    }

    public void AddItem(ItemData item, int cantidad)
    {
        if (item == null || cantidad <= 0) return;
        int maxStack = item.maxStack > 0 ? item.maxStack : defaultMaxStack;

        // 1) Intentar apilar
        foreach (var slot in slots)
        {
            if (slot.CanStack(item))
            {
                int espacioLibre = (item.maxStack > 0 ? item.maxStack : defaultMaxStack) - slot.cantidadActual;
                int aAgregar = Mathf.Min(espacioLibre, cantidad);
                slot.AddToStack(aAgregar);
                cantidad -= aAgregar;
                if (cantidad <= 0) return;
            }
        }

        // 2) Slots vacíos
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                int aColocar = Mathf.Min(maxStack, cantidad);
                slot.SetItem(item, aColocar);
                cantidad -= aColocar;
                if (cantidad <= 0) return;
            }
        }

        // 3) Lleno
        if (cantidad > 0)
            Debug.Log("Inventario lleno. No se pudo agregar todo el item.");
    }

    public bool HasSpaceFor(ItemData item, int cantidad)
    {
        if (item == null || cantidad <= 0) return false;
        int maxStack = item.maxStack > 0 ? item.maxStack : defaultMaxStack;
        int restante = cantidad;

        foreach (var s in slots)
        {
            if (s.currentItem == item)
            {
                int libre = maxStack - s.cantidadActual;
                if (libre > 0)
                {
                    int use = Mathf.Min(libre, restante);
                    restante -= use;
                    if (restante <= 0) return true;
                }
            }
        }
        foreach (var s in slots)
        {
            if (s.IsEmpty())
            {
                int use = Mathf.Min(maxStack, restante);
                restante -= use;
                if (restante <= 0) return true;
            }
        }
        return restante <= 0;
    }

    public ItemData GetSelectedItem(out int cantidad)
    {
        var slot = slots[selectItemIndex];
        cantidad = slot.cantidadActual;
        return slot.currentItem;
    }

    public bool TryConsumeSelected(int amount)
    {
        if (amount <= 0) return false;
        var slot = slots[selectItemIndex];
        if (slot.IsEmpty() || slot.cantidadActual < amount) return false;
        slot.RemoveFromStack(amount);
        return true;
    }

    public InventorySlot GetSelectedSlot() => slots[selectItemIndex];

    public void SelectItem()
    {
        float scroll = player.controllers.Mouse.GetAxisRaw(2);
        if (player.GetButtonDown("Right Inventory") || scroll > 0.1f) NextItem();
        if (player.GetButtonDown("Left Inventory")  || scroll < -0.1f) PrevItem();

        for (int i = 0; i < slots.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectSlot(i);
        }
    }

    private void NextItem()
    {
        previousIndex = selectItemIndex;
        selectItemIndex = (selectItemIndex + 1) % slots.Length;
        UpdateItem();
    }

    private void PrevItem()
    {
        previousIndex = selectItemIndex;
        selectItemIndex = (selectItemIndex - 1 + slots.Length) % slots.Length;
        UpdateItem();
    }

    void SelectSlot(int i)
    {
        if (i < 0 || i >= slots.Length) return;
        previousIndex = selectItemIndex;
        selectItemIndex = i;
        UpdateItem();
    }

    private void UpdateItem()
    {
        if (previousIndex >= 0 && previousIndex < slots.Length)
            slots[previousIndex].SetHighlight(false);

        if (selectItemIndex >= 0 && selectItemIndex < slots.Length)
            slots[selectItemIndex].SetHighlight(true);
    }
}
