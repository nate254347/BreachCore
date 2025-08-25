using TMPro;
using UnityEngine;

public class PurchaseUnlocker : MonoBehaviour
{
    [Header("Purchase Settings")]
    public int price = 50;                 // Cost to unlock
    public GameObject[] objectsToEnable;   // Objects to activate when purchased

    [Header("UI References")]
    public TextMeshProUGUI priceText;      // Shows price or "Purchased"
    public GameObject coverObject;         // Overlay when locked
    public TextMeshProUGUI unlockText;     // Text on cover showing money needed

    [Header("Update Rate")]
    public float updateRate = 0.1f;
    private float timer;

    private bool purchased = false;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (purchased) return;

        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            timer = 0f;
            UpdateUI();
        }
    }

    public void TryPurchase()
    {
        if (purchased) return;

        int playerMoney = CurrencyManager.Instance.GetMoney();

        if (playerMoney >= price)
        {
            // Spend money
            CurrencyManager.Instance.SpendMoney(price);

            // Activate objects
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null) obj.SetActive(true);
            }

            purchased = true;

            // Update UI to Purchased
            if (priceText != null)
            {
                priceText.text = "Purchased";
                priceText.color = Color.green;
            }

            if (coverObject != null) coverObject.SetActive(false);
            if (unlockText != null) unlockText.text = "";
        }
    }

    private void UpdateUI()
    {
        if (purchased) return;

        if (priceText != null)
        {
            priceText.text = $"${price}";
           
        }

        int playerMoney = CurrencyManager.Instance.GetMoney();

        if (playerMoney < price)
        {
            if (coverObject != null) coverObject.SetActive(true);

            if (unlockText != null)
                unlockText.text = $"Need ${price - playerMoney}";
        }
        else
        {
            if (coverObject != null) coverObject.SetActive(false);
        }
    }
}
