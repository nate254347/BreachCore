using UnityEngine;

public class EnemyPrefabInitializer : MonoBehaviour
{
    [Header("Prefab to initialize")]
    public Enemy enemyPrefab;

    [Header("Stats to set")]
    public int health;
    public float speed;
    public int goldMultiplier;

    private bool initialized = false;

    private void Awake()
    {
        if (initialized) return;
        initialized = true;

        if (enemyPrefab == null)
        {
            Debug.LogWarning("[EnemyPrefabInitializer] No prefab assigned!");
            return;
        }

        enemyPrefab.health = health;
        enemyPrefab.speed = speed;
        enemyPrefab.goldMultiplier = goldMultiplier;

        Debug.Log($"[EnemyPrefabInitializer] Initialized {enemyPrefab.name}: HP={health}, Speed={speed}, Gold={goldMultiplier}");
    }
}
