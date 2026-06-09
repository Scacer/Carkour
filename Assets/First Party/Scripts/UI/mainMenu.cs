using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class mainMenu : MonoBehaviour
{
    [SerializeField] Animation fade;
    // Load Scene
    public void Play()
    {
        
        SceneManager.LoadScene("city");
    }

    // Quit Game
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Player has Quit the game.");
    }

    private void Start()
    {

        endCardManager.enforceDirectory();
    }
}
