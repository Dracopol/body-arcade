using UnityEngine;

public class Scoresystem : MonoBehaviour
{
    private float maxScore = 0f;
    private float currentScore = 0f;

    private float percentageScore;

    private string scoreGrade;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            var (grade, score, percentage) = GetScoreDetails();
            Debug.Log($"Score Grade: {grade}, Score: {score}, Percentage: {percentage}%");
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            GameObject[] enemies = GetObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Debug.Log(enemy.name);
            }
        }
    }

    public void showStuff()
    {
        var (grade, score, percentage) = GetScoreDetails();
        Debug.Log($"Score Grade: {grade}, Score: {score}, Percentage: {percentage}%");
    }

    public (string, int, int) GetScoreDetails()
    {
        percentageScore = CalculateScorePercentage();
        scoreGrade = DetermineScoreGrade(percentageScore);
        return (scoreGrade, Mathf.RoundToInt(currentScore), Mathf.RoundToInt(percentageScore * 100f));
    }

    private GameObject[] GetObjectsWithTag(string tag)
    {
        return GameObject.FindGameObjectsWithTag(tag);
    }

    private float CalculateScorePercentage()
    {
        GameObject[] enemies = GetObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            maxScore += enemy.GetComponent<Target>().maxHealth;
            float damageTaken = enemy.GetComponent<Target>().maxHealth - enemy.GetComponent<Target>().currentHealth;
            currentScore += damageTaken;
        }
        return (currentScore / maxScore);
    }

    private string DetermineScoreGrade(float percentage)
    {
        if (percentage >= 1f)
            return "S";
        else if (percentage >= 0.75f)
            return "A";
        else if (percentage >= 0.5f)
            return "B";
        else if (percentage >= 0.25f)
            return "C";
        else if (percentage > 0f)
            return "D";
        else
            return "F";
    }
}
