using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventorySlot[] slots; 
    public int maxStack = 20;

    public void AddItem(ItemData item, int cantidad)
    {
        //Intentar apilar en slots que ya tengan el mismo item
        foreach (var slot in slots)
        {
            if (slot.CanStack(item))
            {
                int espacioLibre = maxStack - slot.cantidadActual;
                int cantidadAAgregar = Mathf.Min(espacioLibre, cantidad);
                slot.AddToStack(cantidadAAgregar);
                cantidad -= cantidadAAgregar;

                if (cantidad <= 0) return;
            }
        }

        //Si aún queda cantidad, poner en un slot vacío
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                int cantidadAColocar = Mathf.Min(maxStack, cantidad);
                slot.SetItem(item, cantidadAColocar);
                cantidad -= cantidadAColocar;

                if (cantidad <= 0) return;
            }
        }

        //Si aún queda, inventario lleno
        if (cantidad > 0)
        {
            Debug.Log("Inventario lleno. No se pudo agregar todo el item.");
        }
    }
}
