using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    [Header("Resolution Settings")]
    [SerializeField] private int targetWidth = 400;
    [SerializeField] private int targetHeight = 800;
    [SerializeField] private bool fullScreen = true;
    [SerializeField] private bool setOnStart = true;

    private void Start()
    {
        if (setOnStart)
        {
            SetResolution();
        }
    }

    public void SetResolution()
    {
        // Force the resolution change
        Screen.SetResolution(targetWidth, targetHeight, fullScreen);
        Debug.Log($"[ResolutionManager] Set resolution to {targetWidth}x{targetHeight}, Fullscreen: {fullScreen}");
    }

    // Optional: Public methods to change resolution at runtime
    public void SetResolution(int width, int height, bool isFullScreen)
    {
        targetWidth = width;
        targetHeight = height;
        fullScreen = isFullScreen;
        SetResolution();
    }

    // Optional: Method to toggle fullscreen
    public void ToggleFullscreen()
    {
        fullScreen = !fullScreen;
        SetResolution();
    }
}