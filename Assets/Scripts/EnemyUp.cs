using UnityEngine;
using TMPro;

public class ShopEnemyUpgradeButton : MonoBehaviour
{
    public enum UpgradeType { Health, Speed }
    public UpgradeType upgradeType;

    public Enemy masterPrefab;
    public int basePrice = 50;
    private int currentPrice;

    public TextMeshProUGUI priceText;
    public GameObject coverObject;
    public TextMeshProUGUI unlockText;

    private void Start()
    {
        currentPrice = basePrice;
        UpdateUI();
    }
    private void Update()
    {
        UpdateUI(); // Check affordability every frame
    }
    public void TryPurchase()
    {
        if (CurrencyManager.Instance == null || masterPrefab == null) return;
        int playerMoney = CurrencyManager.Instance.GetMoney();
        if (playerMoney < currentPrice) return;

        CurrencyManager.Instance.SpendMoney(currentPrice);

        switch (upgradeType)
        {
            case UpgradeType.Health:
                masterPrefab.ApplyHealthUpgrade(2f); // double HP
                break;
            case UpgradeType.Speed:
                    masterPrefab.ApplySpeedUpgrade(0.02f); // Increase speed by 10%
                break;
        }

        masterPrefab.ApplyGoldUpgrade(1); // +1 gold multiplier

        currentPrice *= 5;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (priceText == null || CurrencyManager.Instance == null) return;

        int playerMoney = CurrencyManager.Instance.GetMoney();
        priceText.text = $"${currentPrice}";

        bool affordable = playerMoney >= currentPrice;
        if (coverObject != null) coverObject.SetActive(!affordable);
        if (unlockText != null)
        {
            unlockText.gameObject.SetActive(!affordable);
            unlockText.text = affordable ? "" : $"Need ${currentPrice - playerMoney}";
        }
    }
}
