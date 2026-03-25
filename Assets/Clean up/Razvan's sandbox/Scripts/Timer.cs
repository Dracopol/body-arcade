using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using TMPro;

public class Timer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeDuration = 60f;
    public bool startOnAwake = true;
    public bool countDown = true;

    [Header("UI (Optional)")]
    public Text timerText;

    [Header("Events")]
    public UnityEvent onTimerComplete;

    public Scoresystem temp;
    public GameObject tempFullUi;
    public TextMeshProUGUI tempGradeUi;
    public TextMeshProUGUI tempScoreUi;
    public TextMeshProUGUI tempPercentageUi;

    private float currentTime;
    private bool isRunning = false;

    private void Awake()
    {
        tempFullUi.SetActive(false);
        if (startOnAwake)
            StartTimer();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (countDown)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                StopTimer();
                onTimerComplete.Invoke();
                var (grade, score, percentage) = temp.GetScoreDetails();
                tempFullUi.SetActive(true);
                tempGradeUi.text = $"{grade}";
                tempScoreUi.text = $"{score}";
                tempPercentageUi.text = $"[ {percentage}% ]";

            }
        }
        else
        {
            currentTime += Time.deltaTime;
            if (currentTime >= timeDuration)
            {
                currentTime = timeDuration;
                StopTimer();
                onTimerComplete.Invoke();
            }
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = currentTime.ToString("F2");
    }

    public void StartTimer()
    {
        currentTime = countDown ? timeDuration : 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = countDown ? timeDuration : 0f;
        UpdateTimerUI();
    }
}
