using UnityEngine;

public abstract class GenericDragAndDrop3D_DropArea : MonoBehaviour
{
    public abstract void OnDropObject(GenericDragAndDrop3D_GameObject obj);
    public abstract void OnHoverObject(GenericDragAndDrop3D_GameObject obj);
    public abstract void OnHoverEnd();
}