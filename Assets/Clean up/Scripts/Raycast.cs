using UnityEngine;
using UnityEngine.InputSystem;

public class Raycast : MonoBehaviour
{
    // --VARIABLES-- //

    // Input from the VR controller, the main to trigger the raycast system
    [SerializeField] private InputActionReference controllerActionTrigger;

    // Is the input pressed?
    [HideInInspector] public bool isTriggerPressed = false;

    // References from EnemyVariables.cs, current enemy set to null initially to set up properly
    [HideInInspector] public EnemyVariables currentEnemy = null;

    // How long the player holds the trigger and aim at the enemy
    private float sessionHoldTime = 0f;

    // Total score of the level
    [HideInInspector]public float totalPlayerScore = 0f;

    // Live score, for visuals only
    [HideInInspector]public float liveScore = 0f;

    // DEBUGGING ONLY, this is the timer
    private float scoreDisplayTimer = 0f;

    // DEBUGGING ONLY, how many seconds needed for next iteration, this to prevent spamming in the console
    private readonly float scoreDisplayInterval = 0.1f;

    // -- SUB FUNCTIONS -- //

    // Subscribe to the input events here to start listening!
    private void OnEnable()
    {
        if (controllerActionTrigger != null)
        {
            controllerActionTrigger.action.performed += OnTriggerPressed;
            controllerActionTrigger.action.canceled += OnTriggerReleased;
        }
    }

    // Unsubscribe from the input events here to stop listening!
    private void OnDisable()
    {
        if (controllerActionTrigger != null)
        {
            controllerActionTrigger.action.performed -= OnTriggerPressed;
            controllerActionTrigger.action.canceled -= OnTriggerReleased;
        }
    }

    // Function to check if the input activates, this case the trigger of the VR controller
    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        isTriggerPressed = true;
    }

    // Function to check if the input deactivates, or released
    private void OnTriggerReleased(InputAction.CallbackContext context)
    {
        isTriggerPressed = false;

        if (currentEnemy != null && !currentEnemy.IsDrained)
        {
            liveScore = currentEnemy.maxPoints * Mathf.Clamp01(sessionHoldTime / currentEnemy.maxHoldTime);
            totalPlayerScore += liveScore;
            // Debug.Log($"Total: {totalPlayerScore}, Rounded: {Mathf.RoundToInt(totalPlayerScore)}");

            sessionHoldTime = 0f;
        }
    }

    // Function to check if the raycast is entering the enemy
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interaction Outside") && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyVariables>(out var enemy))
            {
                currentEnemy = enemy;
                sessionHoldTime = 0f;
            }
        }
    }

    // Function to check if the raycast is exiting the enemy
    private void OnTriggerExit(Collider other)
    {
        if (currentEnemy != null && other.gameObject == currentEnemy.gameObject)
        {
            if (!currentEnemy.IsDrained)
            {
                liveScore = currentEnemy.maxPoints * Mathf.Clamp01(sessionHoldTime / currentEnemy.maxHoldTime);
                totalPlayerScore += liveScore;
                // Debug.Log($"Total: {totalPlayerScore}, Rounded: {Mathf.RoundToInt(totalPlayerScore)}");
            }

            ResetInteraction();
        }
    }

    // Function to reset the interaction, so set hold time to 0 and set current enemy to none
    private void ResetInteraction()
    {
        sessionHoldTime = 0f;
        currentEnemy = null;
    }

    // -- MAIN FUNCTION -- //
    private void Update()
    {
        if (currentEnemy != null && isTriggerPressed)
        {
            sessionHoldTime += Time.deltaTime;
            currentEnemy.accumulatedHoldTime += Time.deltaTime;

            float progress = Mathf.Clamp01(currentEnemy.accumulatedHoldTime / currentEnemy.maxHoldTime);
            currentEnemy.transform.localScale = Vector3.Lerp(currentEnemy.originalScale, currentEnemy.minScale, progress);

            // Update the timer
            scoreDisplayTimer += Time.deltaTime;
            if (scoreDisplayTimer >= scoreDisplayInterval)
            {
                scoreDisplayTimer = 0f; // reset timer

                liveScore = currentEnemy.maxPoints * Mathf.Clamp01(sessionHoldTime / currentEnemy.maxHoldTime);

                // Show live score in console
                // Debug.Log($"Live additionals: {liveScore}");
            }

            if (currentEnemy.IsDrained)
            {
                // Debug.Log("Enemy is down");
                liveScore = currentEnemy.maxPoints * Mathf.Clamp01(sessionHoldTime / currentEnemy.maxHoldTime);
                totalPlayerScore += liveScore;
                // Debug.Log($"Total: {totalPlayerScore}, Rounded: {Mathf.RoundToInt(totalPlayerScore)}");

                if (currentEnemy.isDestroyable)
                {
                    Destroy(currentEnemy.gameObject);
                    ResetInteraction();
                }
                else
                {
                    currentEnemy.GetComponent<Collider>().enabled = false;
                    ResetInteraction();
                }
            }
        }
    }
}
