using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerCheck : MonoBehaviour
{
    public InputActionReference rightTrigger;
    public InputActionReference leftTrigger;
    public WeaponInput weaponInputright;
    public WeaponInput weaponInputLeft;

    void Update()
    {
        float triggerValueR = rightTrigger.action.ReadValue<float>();

        float triggerValueL = leftTrigger.action.ReadValue<float>();

        bool isHeldr = triggerValueR > 0.1f;
        bool isHeldl = triggerValueL > 0.1f;

        weaponInputright.SetVRButtonState(isHeldr);
        weaponInputLeft.SetVRButtonState(isHeldl);
    }
}
