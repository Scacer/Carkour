using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class raceTimer : MonoBehaviour
{
    private bool _timerActive;
    public float currentTime;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Image timerBack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetTimer();
    }

    void OnEnable()
    {
        RaceLine.TimerStart += ActivateTimer;
        RaceLine.TimerStop += DeactivateTimer;
    }

    // Update is called once per frame
    void Update()
    {
        if (_timerActive)
        {
            currentTime += Time.deltaTime;
        }
        _text.text = currentTime.ToString("n2");
        
    }

    public void ActivateTimer()
    {
        _timerActive = true;
        _text.enabled = true;
        timerBack.enabled = true;
    }

    public void DeactivateTimer()
    {
        _timerActive = false;
        _text.enabled = false;
        timerBack.enabled = false;
    }

    public void ResetTimer()
    {
        currentTime = 0;
        _text.enabled = false;
        timerBack.enabled = false;
    }
}
