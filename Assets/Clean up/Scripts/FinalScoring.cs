using UnityEngine;
using TMPro;
using DG.Tweening;

public class FinalScoring : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TotalScoreText;
    [SerializeField] private TextMeshProUGUI LiveScoreText;
    [SerializeField] private TextMeshProUGUI ScoreTitle;
    [SerializeField] private TextMeshProUGUI Grading;
    [SerializeField] private TextMeshProUGUI PercentageText;
    [SerializeField] private UpdateScore updateScoreScript;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Fade out Total and Live Score
        TotalScoreText.DOFade(0f, 0.25f).SetEase(Ease.OutQuad);
        LiveScoreText.DOFade(0f, 0.5f).SetEase(Ease.OutQuad);

        // Set values for grade and percentage (hidden initially)
        float percentage = updateScoreScript.percentage;
        PercentageText.text = $"[ {percentage}% ]";
        Grading.text = GetGrade(percentage);

        // // Start with fully transparent texts
        // ScoreTitle.alpha = 0f;
        // Grading.alpha = 0f;
        // PercentageText.alpha = 0f;

        // Sequence for ordered display
        Sequence revealSequence = DOTween.Sequence();

        revealSequence
            .Append(ScoreTitle.DOFade(1f, 0.5f))   // Show ScoreTitle
            .AppendInterval(1f)                    // Wait 1 second
            .Append(Grading.DOFade(1f, 0.5f))      // Show Grading
            .AppendInterval(2f)                    // Wait 2 seconds
            .Append(PercentageText.DOFade(1f, 0.5f)); // Show PercentageText
    }

    private string GetGrade(float percentage)
    {
        if (percentage >= 95f) return "S+";
        else if (percentage >= 90f) return "S";
        else if (percentage >= 80f) return "A";
        else if (percentage >= 65f) return "B";
        else if (percentage >= 50f) return "C";
        else if (percentage >= 30f) return "D";
        else return "NUL";
    }
}
