using TMPro;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    public static BaseHealth Instance { get; private set; }

    [Header("Health")]
    public int maxHealth = 20;
    public int currentHealth;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple BaseHealth instances found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHealthDisplay();

        Debug.Log($"[BaseHealth] Took {dmg} damage. HP={currentHealth}/{maxHealth}");

        if (currentHealth == 0)
        {
            OnBaseDestroyed();
        }
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            // Display health with heart suffix as requested (e.g. "100 <3")
            healthText.text = currentHealth.ToString() + " <3";
        }
    }

    void OnBaseDestroyed()
    {
        Debug.Log("[BaseHealth] Base destroyed! Game over.");
        // TODO: trigger game over flow (stop spawners, show UI etc.)
    }
}
