using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class toyBoxLoader : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Toybox");
        }
    }
}
