using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RocketTower : MonoBehaviour
{
    [Header("Combat")]
    public float rotationSpeed = 60f; // Slower rotation than laser tower
    public float shootingSpeed = 0.5f; // Slower firing rate
    public float damage = 25f; // Higher damage per rocket
    public float maxRange = 15f; // Longer range
    public float explosionRadius = 3f; // Radius of explosion
    public TargetingMode targetingMode = TargetingMode.Closest;

    // Per-tower health removed; base health is managed by BaseHealth singleton

    [Header("Rocket")]
    public GameObject rocketPrefab;   // Assign rocket prefab in inspector
    public Transform rocketSpawnPoint; // Point where rockets spawn

    private float shootCooldown = 0f;
    private Enemy target;

    public enum TargetingMode
    {
        Closest,
        Furthest,
        MostHealth,
        LeastHealth,
        Fastest,
        Slowest
    }

    void Start()
    {
        // No per-tower health initialization; base health is global
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
            if (angle < 10f) // Wider angle tolerance than laser tower
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

        target = sorted.FirstOrDefault();
    }

    void Shoot()
    {
        if (target != null && rocketPrefab != null)
        {
            Vector3 spawnPosition = rocketSpawnPoint != null ? rocketSpawnPoint.position : transform.position;
            GameObject rocketObj = Instantiate(rocketPrefab, spawnPosition, transform.rotation);
            Rocket rocket = rocketObj.GetComponent<Rocket>();
            if (rocket != null)
            {
                // Use target's current position as the rocket's destination (detonate when it reaches that point)
                Vector2 targetPos = target.transform.position;
                // Prefer the prefab/instance explosion radius on the rocket itself so prefab settings aren't overridden by the tower
                float rocketRadius = rocket.explosionRadius;
                rocket.Initialize(damage, rocketRadius, targetPos);
            }
        }
    }

    // Tower-specific TakeDamage removed — base health is the single source of truth
}