using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleVerticalDrag : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public RectTransform target;   // UI element to move
    public Canvas canvas;          // Reference to your Canvas (assign in inspector)
    public float maxY = 500f;      // Max vertical position in local canvas units

    private float minY;            // Fixed min Y, set once on Awake
    private Vector2 pointerStartLocalPos;
    private float targetStartY;

    void Awake()
    {
        if (target != null)
            minY = target.anchoredPosition.y;
        else
            Debug.LogError("Target RectTransform not assigned.");

        if (canvas == null)
            Debug.LogError("Canvas reference is missing.");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out pointerStartLocalPos);

        targetStartY = target.anchoredPosition.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPointerLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out currentPointerLocalPos);

        float deltaY = currentPointerLocalPos.y - pointerStartLocalPos.y;

        float newY = Mathf.Clamp(targetStartY + deltaY, minY, maxY);
        target.anchoredPosition = new Vector2(target.anchoredPosition.x, newY);
    }
}
