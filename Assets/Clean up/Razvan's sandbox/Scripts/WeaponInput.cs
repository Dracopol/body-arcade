using UnityEngine;

[RequireComponent(typeof(Weapon))]
public class WeaponInput : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [Header("PC Input (Keyboard Only)")]
    [SerializeField] private string inputName = "R";
    private KeyCode key;

    [Header("VR Input")]
    [HideInInspector] public bool vrButtonHeld = false;
    private bool vrPressedThisFrame = false;

    private void Awake()
    {
        if (!System.Enum.TryParse(inputName, true, out key))
        {
            Debug.LogWarning($"Invalid key '{inputName}' — defaulting to R.");
            key = KeyCode.R;
        }
    }

    private void Update()
    {
        bool pcHeld = Input.GetKey(key);
        bool pcPressed = Input.GetKeyDown(key);

        if (weapon.Automatic)
        {
            if (pcHeld || vrButtonHeld)
                weapon.TryShoot();
        }
        else
        {
            if (pcPressed || vrPressedThisFrame)
                weapon.FireOnce();
        }

        vrPressedThisFrame = false;
    }

    // Called by VR script
    public void SetVRButtonState(bool held)
    {
        if (held && !vrButtonHeld)
        {
            vrPressedThisFrame = true;
        }

        vrButtonHeld = held;
    }
}
