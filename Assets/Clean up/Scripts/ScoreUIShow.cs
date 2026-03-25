using TMPro;
using UnityEngine;
using System.Collections;

public class ScoreUIShow : MonoBehaviour
{
    // -- VARIABLES -- //
    [SerializeField] private TextMeshProUGUI totalScoreText;

    // -- MAIN FUNCTIONS -- //
    public void TriggerShowScoreUI()
    {
        StartCoroutine(BlinkScoreUI());
    }

// There's no comment here
    private IEnumerator BlinkScoreUI()
    {
        for (int i = 0; i < 5; i++)
        {
            totalScoreText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.05f);

            totalScoreText.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.05f);
        }

        // Enable UI again
        totalScoreText.gameObject.SetActive(true);
    }
}