using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    [Header("Shop Settings")]
    public int basePrice = 10;
    private int currentPrice;

    [Header("UI References")]
    public TextMeshProUGUI priceText;       // Shows current price
    public GameObject coverObject;          // The "locked" overlay
    public TextMeshProUGUI unlockText;      // Text on the cover showing how much more is needed

    [Header("Tower Reference")]
    public Tower tower;                     // Assign your tower in the Inspector

    [Header("Upgrade Settings")]
    public bool isAttackUpgrade;            // true = attack upgrade, false = speed upgrade

    [Header("Update Rate")]
    public float updateRate = 0.1f;         // Seconds between UI checks
    private float timer;

    private void Start()
    {
        currentPrice = basePrice;
        UpdateUI();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            timer = 0f;
            UpdateUI();
        }
    }

    public void TryPurchase()
    {
        int playerMoney = CurrencyManager.Instance.GetMoney();

        if (playerMoney >= currentPrice)
        {
            // Spend the money
            CurrencyManager.Instance.SpendMoney(currentPrice);

            // Apply the upgrade
            if (isAttackUpgrade)
            {
                tower.damage += 1;
            }
            else
            {
                tower.shootingSpeed += 1;
                tower.rotationSpeed += 5;
            }

            // Increase the cost
            currentPrice *= 5;

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (priceText != null)
            priceText.text = $"${currentPrice}";

        int playerMoney = CurrencyManager.Instance.GetMoney();

        if (playerMoney < currentPrice)
        {
            if (coverObject != null) coverObject.SetActive(true);

            if (unlockText != null)
                unlockText.text = $"Need ${currentPrice - playerMoney}";
        }
        else
        {
            if (coverObject != null) coverObject.SetActive(false);
        }
    }
}
