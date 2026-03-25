using UnityEngine;

public class EnemyVariables : MonoBehaviour
{
    // -- VARIABLES -- //
    public float maxPoints = 10f;
    public float maxHoldTime = 5f;
    [HideInInspector] public float accumulatedHoldTime = 0f;
    [HideInInspector] public Vector3 originalScale;
    public Vector3 minScale = new(0f, 0f, 0f);
    public bool isDestroyable = true;
    public bool IsDrained => accumulatedHoldTime >= maxHoldTime;

    // -- MAIN FUNCTION -- //
    private void Awake()
    {
        minScale = Vector3.Max(minScale, Vector3.one * 0.01f);
        originalScale = transform.localScale;
    }
}