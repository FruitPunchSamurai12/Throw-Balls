
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.InputSystem;

public class GenericDragAndDrop3D_Manager : MonoBehaviour
{
    [SerializeField] protected Transform movementPlanePosition;
    [SerializeField] protected Transform dropPlanePosition;
    [SerializeField] protected LayerMask draggablesLayerMask;
    [SerializeField] protected float movementDuration = 0.5f;
    [SerializeField] protected Ease movementEase;
    [SerializeField] protected float dropDuration = 0.75f;
    [SerializeField] protected Ease dropEase;
    [SerializeField] protected BoxCollider dropBounds;
    protected Plane movementPlane;
    protected Plane dropPlane;
    protected bool isDragging = false;
    //private bool isDropping = false;
    protected Vector3 initialWorldPosition;
    protected Vector3 initialMousePosition;
    protected Camera mainCamera;
    protected GenericDragAndDrop3D_GameObject currentDraggableObject;
    protected GenericDragAndDrop3D_GameObject hoverDraggableObject;
    protected GenericDragAndDrop3D_DropArea currentHoverDropArea;
    protected DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> movementTween;
    protected DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> dropTween;
    protected Ray ray;
    protected RaycastHit hit;

    protected GenericDragAndDrop3D_GameObject[] draggableObjects;

    void Start()
    {
        mainCamera = Camera.main;
        if(movementPlanePosition!=null)
            movementPlane = new Plane(movementPlanePosition.up, movementPlanePosition.position);

        if (dropPlanePosition!=null)
            dropPlane = new Plane(Vector3.up, dropPlanePosition.position);
        draggableObjects = GetComponentsInChildren<GenericDragAndDrop3D_GameObject>();
    }

    virtual protected void Update()
    {
        ray = mainCamera.ScreenPointToRay(GetInputPosition());
        if (!isDragging)
        {
            if (Physics.Raycast(ray, out hit, 100f, draggablesLayerMask))
            {
                var objectOnHover = hit.transform.GetComponentInParent<GenericDragAndDrop3D_GameObject>();
                if (hoverDraggableObject != null && objectOnHover != hoverDraggableObject)
                {
                    hoverDraggableObject.OnHover(false);
                    hoverDraggableObject = null;
                }
                if (objectOnHover != null && draggableObjects.Contains(objectOnHover))
                {
                    if (hoverDraggableObject != objectOnHover)
                    {
                        hoverDraggableObject = objectOnHover;
                        hoverDraggableObject.OnHover(true);
                    }
                }
            }
            else
            {
                if (hoverDraggableObject != null)
                {
                    hoverDraggableObject.OnHover(false);
                }
                hoverDraggableObject = null;
            }
        }

        if (IsInputDown())
        {
            BeginDragging();
        }

        if (isDragging && currentDraggableObject != null)
        {
            OnDragging();
        }

        if (IsInputUp())
        {
            if (isDragging && currentDraggableObject != null)
            {
                EndDragging();
            }
        }
    }

    virtual protected void OnDragging()
    {
        Vector3 mousePosition = GetInputPosition();
        mousePosition.z = mainCamera.WorldToScreenPoint(currentDraggableObject.transform.position).z;
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        newPosition = movementPlane.ClosestPointOnPlane(newPosition);
        if (movementTween != null)
        {
            movementTween.Kill();
        }
        movementTween = currentDraggableObject.transform.DOMove(newPosition, movementDuration).SetEase(movementEase);
        currentDraggableObject.OnDrag();

        if (Physics.Raycast(ray, out hit, 100f, draggablesLayerMask,QueryTriggerInteraction.Collide))
        {
            Debug.Log($"hit {hit.collider.name}");
            GenericDragAndDrop3D_DropArea dropArea = hit.transform.GetComponentInParent<GenericDragAndDrop3D_DropArea>();
            if (dropArea != null)
            {
                if (currentHoverDropArea != null && currentHoverDropArea!=dropArea)
                {
                    currentHoverDropArea.OnHoverEnd();
                    currentHoverDropArea = null;
                }
                else if (currentHoverDropArea != null && currentHoverDropArea == dropArea)
                {
                    return;
                }
                currentHoverDropArea = dropArea;
                currentHoverDropArea.OnHoverObject(currentDraggableObject);
            }
            else
            {
                if (currentHoverDropArea != null)
                {
                    currentHoverDropArea.OnHoverEnd();
                    currentHoverDropArea = null;
                }
            }
        }
        else
        {
            if (currentHoverDropArea != null)
            {
                currentHoverDropArea.OnHoverEnd();
                currentHoverDropArea = null;
            }
		}
	}
    
    virtual protected void BeginDragging()
    {
        if(currentHoverDropArea!=null)
        {
            currentHoverDropArea.OnHoverEnd();
            currentHoverDropArea = null;
        }
        currentDraggableObject = hoverDraggableObject;
        if (currentDraggableObject != null)
        {
            isDragging = true;
            DOTween.Kill(currentDraggableObject.transform);
            initialWorldPosition = currentDraggableObject.transform.position;
            initialMousePosition = Input.mousePosition;
            currentDraggableObject.OnBeginDrag();
        }
    }

    virtual protected void EndDragging()
    {
        if (currentHoverDropArea != null)
        {
            currentHoverDropArea.OnHoverEnd();
            currentHoverDropArea = null;
        }
        isDragging = false;

        if (Physics.Raycast(ray, out hit, 100f, draggablesLayerMask, QueryTriggerInteraction.Collide))
        {
            GenericDragAndDrop3D_DropArea dropArea = hit.transform.GetComponentInParent<GenericDragAndDrop3D_DropArea>();
            if (dropArea != null)
            {
                dropArea.OnDropObject(currentDraggableObject);
                currentDraggableObject = null;
                return;
            }
        }
        Vector3 dropPosition = currentDraggableObject.transform.position;
        if (dropBounds != null)
        {
            bool isInsideBounds = dropBounds.Raycast(ray, out hit, Mathf.Infinity);
            if (isInsideBounds)
            {
                float distance;
                if (dropPlane.Raycast(ray, out distance))
                {
                    dropPosition = ray.GetPoint(distance);
                }
                else
                {
                    dropPosition = initialWorldPosition;
                }
            }
            else
            {
                dropPosition = initialWorldPosition;
            }
        }
        else
        {
            dropPosition = initialWorldPosition;
        }
        if (movementTween != null && movementTween.IsPlaying())
        {
            movementTween.Kill();
        }

        dropTween = currentDraggableObject.transform.DOMove(dropPosition, dropDuration).SetEase(dropEase);
        currentDraggableObject.OnEndDrag();
        currentDraggableObject = null;
    }

    protected Vector2 GetInputPosition()
    {
        // Check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        // Fallback to mouse input
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        // Default to zero if no input device is detected
        return Vector2.zero;
    }

    protected bool IsInputDown()
    {
        // Check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        // Fallback to mouse input
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    protected bool IsInputUp()
    {
        // Check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            return true;
        }

        // Fallback to mouse input
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return true;
        }

        return false;
    }
}
