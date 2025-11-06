using UnityEngine;
using UnityEngine.UI;

public class TimerButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timerDuration = 5f;
    [SerializeField] private int requiredKills = 10;

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject coverObject;         // The "locked" overlay
    [SerializeField] private TMPro.TextMeshProUGUI unlockText;      // Text showing kills needed

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
        bool hasEnoughKills = Enemy.totalEnemiesKilled >= requiredKills;

        if (!hasEnoughKills)
        {
            if (button != null)
                button.interactable = false;
            if (fillImage != null)
                fillImage.fillAmount = 0f;
            
            // Show cover and update text
            if (coverObject != null)
                coverObject.SetActive(true);
            if (unlockText != null)
                unlockText.text = $"Need {requiredKills - Enemy.totalEnemiesKilled} Kills";
                
            return;
        }
        else
        {
            // Hide cover when requirement is met
            if (coverObject != null)
                coverObject.SetActive(false);
            if (unlockText != null)
                unlockText.text = "";
        }

        if (isRunning)
        {
            timer -= Time.deltaTime;

            if (fillImage != null)
                fillImage.fillAmount = Mathf.Clamp01(timer / timerDuration);

            if (timer <= 0f)
                EndTimer();
        }
        else if (button != null)
        {
            button.interactable = true;
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
