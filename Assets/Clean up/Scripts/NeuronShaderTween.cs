using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.Splines;

public class NeuronShaderTween : MonoBehaviour
{
    [SerializeField] private TargetSpawner spawner;
    public Renderer targetRenderer;

    public float animationDuration;
    public float minDelay;
    public float maxDelay;
    public float targetValue;
    public float badNeuronChance;
    [SerializeField] private SplineContainer splineContainer;

    private Color defaultEmission;
    private Color badEmission;

    private Material mat;

    bool isBadNeuron;

    public void Initialize()
    {
        mat = targetRenderer.material;
        defaultEmission = mat.GetColor("_Emission_Color");
        badEmission = mat.GetColor("_Bad_Emission_Color");

        isBadNeuron = CompareTag("BadNeuron");
        StartCoroutine(TweenLoop());
    }

    private IEnumerator TweenLoop()
    {
        while (true)
        {
            // Random delay BEFORE each animation
            float startDelay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(startDelay);

            // Reset spread
            mat.SetFloat("_Animation_Spread", -0.5f);

            // Determine if this pulse is bad or good
            bool isBadPulse = isBadNeuron && Random.value < badNeuronChance;

            if (isBadPulse)
            {
                mat.SetColor("_Emission_Color", badEmission);
                //Debug.Log($"{gameObject.name} - Starting BAD pulse!");
            }
            else
            {
                mat.SetColor("_Emission_Color", defaultEmission);
                //Debug.Log($"{gameObject.name} - Starting GOOD pulse!");
            }

            // Pulsing animation
            var tween = DOTween.To(
                () => mat.GetFloat("_Animation_Spread"),
                x => mat.SetFloat("_Animation_Spread", x),
                targetValue,
                animationDuration
            ).SetEase(Ease.Linear);

            // Log when tween finishes
            tween.OnComplete(() =>
            {
                //Debug.Log($"{gameObject.name} - Finished {(isBadPulse ? "BAD" : "GOOD")} pulse!");//how can i listen to this to do smf in TargetSpawner
                // Reset color if it was a bad pulse
                if (isBadPulse)
                {
                    mat.SetColor("_Emission_Color", defaultEmission);
                    if (splineContainer != null)
                    {
                        spawner.SpawnEnemyOnSpline(splineContainer, spawner.movementSpeed);
                    }
                }
                else
                {
                    if (splineContainer != null)
                    {
                        spawner.SpawnAllyOnSpline(splineContainer, spawner.movementSpeed);
                    }
                }

            });

            // Wait until tween is done before looping
            yield return tween.WaitForCompletion();
        }
    }
}
