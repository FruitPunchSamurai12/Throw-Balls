using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericDragAndDrop3D_GameObject : MonoBehaviour
{
    protected bool isDragging = false;

    public virtual void OnHover(bool isHovering)
    {
        
    }

    public virtual void OnBeginDrag()
    {
        isDragging = true;
    }


    public virtual void OnDrag()
    {
        
    }

    public virtual void OnEndDrag()
    {
        isDragging = false;
    }
}
