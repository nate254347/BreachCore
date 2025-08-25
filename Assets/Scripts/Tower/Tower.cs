using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public enum TargetingMode
    {
        Closest,
        Furthest,
        MostHealth,
        LeastHealth,
        Fastest,
        Slowest
    }

    [Header("Combat")]
    public float rotationSpeed = 90f;
    public float shootingSpeed = 1f;
    public float damage = 1f;
    public float maxRange = 10f;
    public TargetingMode targetingMode = TargetingMode.Closest;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("Laser")]
    public GameObject laserPrefab;   // Assign prefab in inspector
    public Transform laserParent;    // Assign the "Lasers" object

    private float shootCooldown = 0f;
    private Enemy target;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    void Update()
    {
        FindTarget();

        if (target != null)
        {
            // Rotate toward target
            Vector3 dir = (target.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

            float angle = Quaternion.Angle(transform.rotation, lookRotation);

            // Only shoot when mostly facing target
            if (angle < 5f)
            {
                shootCooldown -= Time.deltaTime;
                if (shootCooldown <= 0f)
                {
                    Shoot();
                    shootCooldown = 1f / shootingSpeed;
                }
            }
        }
    }

    void FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        enemies = enemies.Where(e => Vector2.Distance(transform.position, e.transform.position) <= maxRange).ToArray();

        if (enemies.Length == 0)
        {
            target = null;
            return;
        }

        // Base selection depending on targeting mode
        IEnumerable<Enemy> sorted = enemies;
        switch (targetingMode)
        {
            case TargetingMode.Closest:
                sorted = enemies.OrderBy(e => Vector2.Distance(transform.position, e.transform.position));
                break;
            case TargetingMode.Furthest:
                sorted = enemies.OrderByDescending(e => Vector2.Distance(transform.position, e.transform.position));
                break;
            case TargetingMode.MostHealth:
                sorted = enemies.OrderByDescending(e => e.health);
                break;
            case TargetingMode.LeastHealth:
                sorted = enemies.OrderBy(e => e.health);
                break;
            case TargetingMode.Fastest:
                sorted = enemies.OrderByDescending(e => e.speed);
                break;
            case TargetingMode.Slowest:
                sorted = enemies.OrderBy(e => e.speed);
                break;
        }

        // Get the best "value" among enemies (top of sort list)
        var best = sorted.FirstOrDefault();
        if (best == null)
        {
            target = null;
            return;
        }

        // Collect all enemies that share the same priority "value"
        float bestValue = 0f;
        switch (targetingMode)
        {
            case TargetingMode.Closest:
            case TargetingMode.Furthest:
                bestValue = Vector2.Distance(transform.position, best.transform.position);
                break;
            case TargetingMode.MostHealth:
            case TargetingMode.LeastHealth:
                bestValue = best.health;
                break;
            case TargetingMode.Fastest:
            case TargetingMode.Slowest:
                bestValue = best.speed;
                break;
        }

        // Get all ties (within small epsilon for floats)
        var tied = enemies.Where(e =>
        {
            switch (targetingMode)
            {
                case TargetingMode.Closest:
                case TargetingMode.Furthest:
                    return Mathf.Approximately(Vector2.Distance(transform.position, e.transform.position), bestValue);
                case TargetingMode.MostHealth:
                case TargetingMode.LeastHealth:
                    return e.health == bestValue;
                case TargetingMode.Fastest:
                case TargetingMode.Slowest:
                    return Mathf.Approximately(e.speed, bestValue);
            }
            return false;
        }).ToList();

        if (tied.Count == 1)
        {
            target = tied[0];
            return;
        }

        // Break tie: pick enemy closest to where tower is pointing
        Vector3 forward = transform.up; // Tower faces "up" in 2D
        target = tied.OrderBy(e =>
        {
            Vector3 dir = (e.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(forward, dir);
            return angle;
        }).FirstOrDefault();
    }


    void Shoot()
    {
        if (target != null)
        {
            // Spawn laser first
            GameObject laserObj = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            Laser laser = laserObj.GetComponent<Laser>();
            laser.Initialize(transform, target.transform, laserParent, damage);
        }
    }


    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHealthDisplay();

        if (currentHealth == 0)
        {
            Debug.Log("Tower destroyed!");
        }
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString() + " <3";
        }
    }
}
