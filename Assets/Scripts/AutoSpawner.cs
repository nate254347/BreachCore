using UnityEngine;
using TMPro;
using System.Collections;

public class AutoSpawnerShopButton : MonoBehaviour
{
    public Enemy masterPrefab;
    public Transform gameSpace;
    public Tower tower;
    public Transform spawnArea;
    public float spawnInterval = 5f;

    public int basePrice = 100;
    private int currentPrice;
    private bool purchased = false;

    public TextMeshProUGUI priceText;
    public GameObject coverObject;
    public TextMeshProUGUI unlockText;

    private void Start()
    {
        currentPrice = basePrice;
        UpdateUI();
    }

    public void TryPurchase()
    {
        if (CurrencyManager.Instance == null || purchased) return;
        if (CurrencyManager.Instance.GetMoney() < currentPrice) return;

        CurrencyManager.Instance.SpendMoney(currentPrice);
        purchased = true;

        StartCoroutine(SpawnLoop());
        UpdateUI();
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnEnemy()
    {
        if (masterPrefab == null || gameSpace == null || tower == null || spawnArea == null)
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
            case 0: // top
                spawnPos = new Vector3(Random.Range(-halfWidth, halfWidth), halfHeight, 0f);
                break;
            case 1: // bottom
                spawnPos = new Vector3(Random.Range(-halfWidth, halfWidth), -halfHeight, 0f);
                break;
            case 2: // left
                spawnPos = new Vector3(-halfWidth, Random.Range(-halfHeight, halfHeight), 0f);
                break;
            case 3: // right
                spawnPos = new Vector3(halfWidth, Random.Range(-halfHeight, halfHeight), 0f);
                break;
        }

        spawnPos += spawnArea.position;

        GameObject obj = Instantiate(masterPrefab.gameObject, spawnPos, Quaternion.identity, gameSpace);
        Enemy e = obj.GetComponent<Enemy>();
        if (e == null)
        {
            Debug.LogWarning("[Spawner] Spawned object missing Enemy component!");
            return;
        }

        e.InitializeFromMaster(masterPrefab);

        Vector3 dir = (tower.transform.position - obj.transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        obj.transform.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"[Spawner] Spawned {e.name} HP={e.health} Speed={e.speed} Gold={e.goldMultiplier}");
    }


    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (priceText == null || CurrencyManager.Instance == null) return;

        int playerMoney = CurrencyManager.Instance.GetMoney();
        if (purchased)
        {
            priceText.text = "Purchased";
            priceText.color = Color.green;
            if (coverObject != null) coverObject.SetActive(false);
            if (unlockText != null) unlockText.gameObject.SetActive(false);
        }
        else
        {
            priceText.text = $"${currentPrice}";
            if (playerMoney < currentPrice)
            {
                if (coverObject != null) coverObject.SetActive(true);
                if (unlockText != null)
                {
                    unlockText.gameObject.SetActive(true);
                    unlockText.text = $"Need ${currentPrice - playerMoney}";
                }
            }
            else
            {
                if (coverObject != null) coverObject.SetActive(false);
                if (unlockText != null) unlockText.gameObject.SetActive(false);
            }
        }
    }
}
