using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class raceTimer : MonoBehaviour
{
    private bool _timerActive;
    private float _currentTime;
    [SerializeField] private TMP_Text _text;

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
            _currentTime += Time.deltaTime;
        }
        _text.text = _currentTime.ToString("n2");
        
    }

    public void ActivateTimer()
    {
        _timerActive = true;
    }

    public void DeactivateTimer()
    {
        _timerActive = false;
    }

    public void ResetTimer()
    {
        _currentTime = 0;
    }
}
