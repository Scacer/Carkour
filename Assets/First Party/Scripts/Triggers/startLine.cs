using System;
using UnityEngine;
using UnityEngine.UI;

public class RaceLine : MonoBehaviour
{
    public static event Action TimerStart;
    public static event Action TimerStop;

    [SerializeField] private bool isStartLine;
    [SerializeField] private bool isFinishLine;

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
                TimerStop?.Invoke();
            }        
        }
    }
}
