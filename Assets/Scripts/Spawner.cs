using UnityEngine;
using TMPro;

public class EnemySpawnerButton : MonoBehaviour
{
    public Enemy enemyPrefab;   // The prefab itself
    public Transform gameSpace;
    public Tower tower;
    public Transform spawnArea;
    public TextMeshProUGUI buttonText;

    private void Start()
    {
        if (buttonText != null && enemyPrefab != null)
            buttonText.text = enemyPrefab.name;

        if (enemyPrefab == null) Debug.LogWarning("[Spawner] Enemy prefab not assigned!");
        if (gameSpace == null) Debug.LogWarning("[Spawner] Game space not assigned!");
        if (tower == null) Debug.LogWarning("[Spawner] Tower not assigned!");
        if (spawnArea == null) Debug.LogWarning("[Spawner] Spawn area not assigned!");
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null || gameSpace == null || tower == null || spawnArea == null)
        {
            Debug.LogWarning("[Spawner] Cannot spawn enemy. Missing references!");
            return;
        }

        Vector3 areaSize = spawnArea.localScale;
        float halfWidth = areaSize.x / 2f;
        float halfHeight = areaSize.y / 2f;

        Vector3 spawnPos = Vector3.zero;
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: spawnPos = new Vector3(Random.Range(-halfWidth, halfWidth), halfHeight, 0f); break;
            case 1: spawnPos = new Vector3(Random.Range(-halfWidth, halfWidth), -halfHeight, 0f); break;
            case 2: spawnPos = new Vector3(-halfWidth, Random.Range(-halfHeight, halfHeight), 0f); break;
            case 3: spawnPos = new Vector3(halfWidth, Random.Range(-halfHeight, halfHeight), 0f); break;
        }
        spawnPos += spawnArea.position;

        GameObject newEnemyObj = Instantiate(enemyPrefab.gameObject, spawnPos, Quaternion.identity, gameSpace);
        Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();
        if (newEnemy != null)
        {
            newEnemy.InitializeFromMaster(enemyPrefab);

            Vector3 dirToTower = (tower.transform.position - newEnemyObj.transform.position).normalized;
            float angle = Mathf.Atan2(dirToTower.y, dirToTower.x) * Mathf.Rad2Deg - 90f;
            newEnemyObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Debug.Log($"[Spawner] Spawned {newEnemy.name} with HP={newEnemy.health}, Speed={newEnemy.speed}, Gold={newEnemy.goldMultiplier}");
        }
    }
}
