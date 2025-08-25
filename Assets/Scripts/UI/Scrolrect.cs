using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectYClamp : MonoBehaviour
{
    [Header("Y Position Limits")]
    public float minY = -100f; // Lowest allowed anchored Y position
    public float maxY = 100f;  // Highest allowed anchored Y position

    private ScrollRect scrollRect;
    private RectTransform content;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    private void LateUpdate()
    {
        Vector2 pos = content.anchoredPosition;

        // Clamp the Y position
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        content.anchoredPosition = pos;
    }
}
