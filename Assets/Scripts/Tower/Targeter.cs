using UnityEngine;
using TMPro;

public class Targeter : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI modeText;  // Assign the TextMeshProUGUI object
    public Tower tower;                // Assign the tower to control

    public void CycleTargetMode()
    {
        if (tower == null || modeText == null) return;

        // Get current mode as int, increment, wrap around
        int nextMode = ((int)tower.targetingMode + 1) % System.Enum.GetNames(typeof(Tower.TargetingMode)).Length;
        tower.targetingMode = (Tower.TargetingMode)nextMode;

        // Update text
        modeText.text = tower.targetingMode.ToString();
    }
}
