using UnityEngine;
using UnityEngine.UI;

public class TimerButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timerDuration = 5f;

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image fillImage;

    private float timer;
    private bool isRunning;

    private void Start()
    {
        if (button != null)
            button.onClick.AddListener(StartTimer);

        ResetFill();
    }

    private void Update()
    {
        if (isRunning)
        {
            timer -= Time.deltaTime;

            if (fillImage != null)
                fillImage.fillAmount = Mathf.Clamp01(timer / timerDuration);

            if (timer <= 0f)
                EndTimer();
        }
    }

    private void StartTimer()
    {
        if (isRunning) return;

        timer = timerDuration;
        isRunning = true;

        if (button != null)
            button.interactable = false;
    }

    private void EndTimer()
    {
        isRunning = false;

        if (button != null)
            button.interactable = true;

        ResetFill();
    }

    private void ResetFill()
    {
        if (fillImage != null)
            fillImage.fillAmount = 0f;
    }
}
