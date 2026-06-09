using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RaceLine : MonoBehaviour
{
    public static event Action TimerStart;
    public static event Action TimerStop;

    [SerializeField] private bool isStartLine;
    [SerializeField] private bool isFinishLine;
    [SerializeField] private raceTimer ctxTimer;

    // Score Management
    public static event Action ThreeStars;
    public static event Action TwoStars;
    public static event Action OneStar;
    public static event Action NoStar;

    [SerializeField] public float ThreeStarsTime = 3f;
    [SerializeField] public float TwoStarsTime = 2f;
    [SerializeField] public float OneStarsTime = 1f;

    private void Start()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isStartLine)
            {
                TimerStart?.Invoke();
            }
            else if (isFinishLine)
            {
                HandleFinish();
            }        
        }
    }

    private void HandleFinish()
    {
        TimerStop?.Invoke();
        float timeAchieved = ctxTimer.currentTime;

        if (timeAchieved < ThreeStarsTime)
        {
            ThreeStars?.Invoke();
        }
        else if (timeAchieved <  TwoStarsTime)
        {
            TwoStars?.Invoke();
        }
        else if (timeAchieved < OneStarsTime)
        {
            OneStar?.Invoke();
        }
        else
        {
            NoStar?.Invoke();
        }
    }
}
