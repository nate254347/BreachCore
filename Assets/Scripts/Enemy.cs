using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 5;
    public float speed = 2f;
    public int goldMultiplier = 1;

    public Tower tower;
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

        tower = master.tower != null ? master.tower : FindObjectOfType<Tower>();

        Debug.Log($"[Enemy] Initialized from master: HP={health}, Speed={speed}, Gold={goldMultiplier}");
    }

    private void Awake()
    {
        if (tower == null) tower = FindObjectOfType<Tower>();
        Debug.Log($"[Enemy] Awake: HP={health}, Speed={speed}, Gold={goldMultiplier}");
    }

    private void Update()
    {
        if (isPendingDeath || tower == null) return;

        Vector3 dir = (tower.transform.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, tower.transform.position) < 0.5f)
        {
            tower.TakeDamage(health);
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
