using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static int totalEnemiesKilled = 0;  // Track total kills
    
    public int health = 5;
    public float speed = 2f;
    public int goldMultiplier = 1;

    // Enemies now target the base instead of towers
    public global::BaseHealth baseTarget;
    public bool isPendingDeath;

    public void InitializeFromMaster(Enemy master)
    {
        if (master == null)
        {
            Debug.LogWarning("[Enemy] Master prefab is null!");
            return;
        }

        health = master.health;
        speed = master.speed;
        goldMultiplier = master.goldMultiplier;

    // prefer master-provided base target, otherwise find the singleton BaseHealth
    baseTarget = FindObjectOfType<global::BaseHealth>() ?? global::BaseHealth.Instance;

        Debug.Log($"[Enemy] Initialized from master: HP={health}, Speed={speed}, Gold={goldMultiplier}");
    }

    private void Awake()
    {
    if (baseTarget == null) baseTarget = global::BaseHealth.Instance ?? FindObjectOfType<global::BaseHealth>();
        Debug.Log($"[Enemy] Awake: HP={health}, Speed={speed}, Gold={goldMultiplier}");
    }

    private void Update()
    {
        if (isPendingDeath || baseTarget == null) return;

        Vector3 dir = (baseTarget.transform.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, baseTarget.transform.position) < 0.5f)
        {
            global::BaseHealth.Instance?.TakeDamage(health);
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isPendingDeath) return;

        // Clamp to not deal more damage than remaining HP
        int clampedDamage = Mathf.Min(dmg, health);

        health -= clampedDamage;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddMoney(clampedDamage * goldMultiplier);

        if (health <= 0)
        {
            isPendingDeath = true;
            totalEnemiesKilled++;  // Increment kill counter
            Debug.Log($"[Enemy] Killed! Total kills: {totalEnemiesKilled}");
            Destroy(gameObject);
        }

        Debug.Log($"[Enemy] Took {clampedDamage} damage. HP={health}, Gold={clampedDamage * goldMultiplier}");
    }


    // Upgrades modify the prefab's values directly
    public void ApplyHealthUpgrade(float multiplier)
    {
        health = Mathf.RoundToInt(health * multiplier);
        Debug.Log($"[Upgrade] Health now {health}");
    }

    public void ApplySpeedUpgrade(float addSpeed)
    {
        speed += addSpeed;
        Debug.Log($"[Upgrade] Speed now {speed}");
    }

    public void ApplyGoldUpgrade(int addGold)
    {
        goldMultiplier += addGold;
        Debug.Log($"[Upgrade] Gold now {goldMultiplier}");
    }
}
