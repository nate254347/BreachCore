using UnityEngine;
using UnityEngine.UI; // Needed for Toggle

public class SwitchActivator : MonoBehaviour
{
    [SerializeField] private Toggle switchToggle; // Reference to your UI Toggle
    [SerializeField] private GameObject targetObject; // The object to turn on/off

    private void Start()
    {
        if (switchToggle != null)
        {
            // Sync initial state
            targetObject.SetActive(switchToggle.isOn);

            // Add listener so it updates whenever switched
            switchToggle.onValueChanged.AddListener(OnSwitchChanged);
        }
    }

    private void OnSwitchChanged(bool isOn)
    {
        if (targetObject != null)
            targetObject.SetActive(isOn);
    }
}
