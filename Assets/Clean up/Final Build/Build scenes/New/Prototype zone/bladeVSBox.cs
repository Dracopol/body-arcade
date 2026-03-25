using UnityEngine;
using UnityEngine.InputSystem;

public class bladeVSBox : MonoBehaviour
{
    [SerializeField] private GameObject bladeRight;
    [SerializeField] private GameObject bladeLeft;
    [SerializeField] private GameObject boxLeft;
    [SerializeField] private GameObject boxRight;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit3Key.wasPressedThisFrame) switchToBox();
        if (kb.digit4Key.wasPressedThisFrame) switchToBlade();
    }

    private void switchToBox()
    {
        bladeRight.SetActive(false);
        bladeLeft.SetActive(false);
        boxLeft.SetActive(true);
        boxRight.SetActive(true);
    }

    private void switchToBlade()
    {
        bladeRight.SetActive(true);
        bladeLeft.SetActive(true);
        boxLeft.SetActive(false);
        boxRight.SetActive(false);
    }
}