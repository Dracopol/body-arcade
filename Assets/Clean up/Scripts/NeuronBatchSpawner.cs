using UnityEngine;

public class NeuronBatchSpawner : MonoBehaviour
{
    [Header("Global Neuron Settings")]

    [Tooltip("Duration in seconds for a single neuron pulse.")]
    [SerializeField] private float animationDuration = 1.5f;

    [Tooltip("Minimum random delay before a neuron starts its pulse.")]
    [SerializeField] private float minDelay = 0.1f;

    [Tooltip("Maximum random delay before a neuron starts its pulse.")]
    [SerializeField] private float maxDelay = 2f;

    [Tooltip("Target value of the animation spread in the shader.")]
    [Range(-0.5f, 1f)]
    [SerializeField] private float targetValue = 1f;

    [Header("For Bad Neurons")]

    [Tooltip("Chance (0-1) for a bad neuron to emit a red pulse.")]
    [Range(0f, 1f)]
    [SerializeField] private float badNeuronChance = 0.75f;

    void Start()
    {
        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            if (!rend.gameObject.TryGetComponent<NeuronShaderTween>(out var tween))
                tween = rend.gameObject.AddComponent<NeuronShaderTween>();
            tween.targetRenderer = rend;

            // pass values
            tween.animationDuration = animationDuration;
            tween.minDelay = minDelay;
            tween.maxDelay = maxDelay;
            tween.targetValue = targetValue;
            tween.badNeuronChance = badNeuronChance;

            // start
            tween.Initialize();
        }
    }
}