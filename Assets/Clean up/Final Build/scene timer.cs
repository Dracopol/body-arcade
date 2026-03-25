using UnityEngine;
using UnityEngine.Events;

public class scenetimer : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onTimerComplete;

    [Header("Timer Settings")]
    public float timeDuration = 60f;
    public bool startOnAwake = true;
    public bool countDown = true;


    private float currentTime;
    private bool isRunning = false;

    private void Awake()
    {
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
    }
}
