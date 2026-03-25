using DG.Tweening;
using TMPro;
using UnityEngine;

public class UpdateScore : MonoBehaviour
{
    // -- VARIABLES -- //
    [SerializeField] private Raycast raycastScript;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI liveScoreText;
    [HideInInspector] public float percentage = 0f;

    private float totalPossibleScore = 0f;
    private Tween liveTextFadeTween;

    // -- MAIN FUNCTIONS -- //
    void Start()
    {
        // Live score text as invisible
        var color = liveScoreText.color;
        liveScoreText.color = new Color(color.r, color.g, color.b, 0f);

        // Calculate total possible score from all enemies
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy.TryGetComponent(out EnemyVariables ev))
            {
                totalPossibleScore += ev.maxPoints;
            }
        }
    }

    void Update()
    {
        // Update percentage score
        if (totalScoreText != null)
        {
            percentage = Mathf.RoundToInt(raycastScript.totalPlayerScore / totalPossibleScore * 100f);
            totalScoreText.text = $"{raycastScript.totalPlayerScore:F0}";
            // Debug.Log(percentage);
        }

        
        // Show live score when trigger is pressed and enemy is hit
        if (raycastScript.isTriggerPressed && raycastScript.currentEnemy != null)
        {
            float currentLiveScore = raycastScript.liveScore;
            liveScoreText.text = $"+{currentLiveScore:F1}";

            if (liveScoreText.color.a < 1f)
            {
                liveTextFadeTween?.Kill();
                liveTextFadeTween = liveScoreText.DOFade(1f, 0.01f).SetEase(Ease.OutQuad);

                // "Pop" effect
                var rect = liveScoreText.rectTransform;
                rect.DOKill();
                rect.localScale = Vector3.one;
                rect.DOScale(1.2f, 0.05f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        rect.DOScale(1f, 0.05f).SetEase(Ease.InQuad);
                    });
            }
        }
        else
        {
            // Fade out live score
            if (liveScoreText.color.a > 0f)
            {
                liveTextFadeTween?.Kill();
                liveTextFadeTween = liveScoreText.DOFade(0f, 0.25f).SetEase(Ease.InQuad);
            }
        }
    }
}
