using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Unity.Cinemachine;
using Unity.IntegerTime;
using System.IO;

public class LevelMenu : MonoBehaviour
{
    [Header("Stage Images")]
    [SerializeField] private Sprite ToyboxImage;

    [Header("Menu Content")]
    [SerializeField] private GameObject ctx;
    [SerializeField] private Image levelImage;
    [SerializeField] private TMP_Text lvlTitle;
    [SerializeField] private TMP_Text lvlTime;

    [Header("Stars")]
    [SerializeField] private Image firstStar;
    [SerializeField] private Image secondStar;
    [SerializeField] private Image thirdStar;
    [SerializeField] private TMP_Text starCount;

    [Header("MenuIdle")]
    [SerializeField] private CarController carController;
    [SerializeField] private CinemachineInputAxisController camInput;

    [Header("Internal References")]
    private string curLevel;

    public static event Action LevelMenuClosed;
    public static event Action FirstBarrierBroken;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endCardManager.enforceDirectory();
        setStarCount();
        ResetMenu();
    }

    private void ShowMenu()
    {
        ctx.SetActive(true);
        carController.enabled = false;
        camInput.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResetMenu()
    {
        LevelMenuClosed?.Invoke();
        ctx.SetActive(false);
        firstStar.enabled = false;
        secondStar.enabled = false;
        thirdStar.enabled = false;
        carController.enabled = true;

        carController.enabled = true;
        camInput.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void setStarCount()
    {
        string lvlPath = Path.Combine(Application.persistentDataPath, "/levels");
        if (Directory.Exists(lvlPath))
        {
            int count = 0;
            foreach(var file in Directory.EnumerateFiles(lvlPath, "*.json"))
            {
                string dataString = File.ReadAllText(file);
                var saveData = JsonUtility.FromJson<SaveData>(dataString);
                count += saveData.numStars;
            }
            starCount.text = string.Format("x{0}", count);
            if (count > 0)
            {
                FirstBarrierBroken?.Invoke();
            }
        }
        else
        {
            starCount.text = "x0";
        }
    }

    private void toyBoxHandler()
    {
        curLevel = "Toybox";
        int numStars = endCardManager.getNumStars("Toybox");
        float bestTime = endCardManager.getBestTime("ToyBox");

        // Load Image
        levelImage.sprite = ToyboxImage;

        // Load Level Data
        lvlTitle.text = "Toybox";
        // Star Check
        if (numStars > -1)
        {
            if (numStars > 0)
            {
                firstStar.enabled = true;
            }
            if (numStars > 1)
            {
                secondStar.enabled = true;
            }
            if (numStars > 2)
            {
                thirdStar.enabled = true;
            }
        }
        // Time Check
        if (bestTime > -1)
        {
            lvlTime.text = string.Format("Best Time:\n{0}", bestTime.ToString("n2"));
        }
        else
        {
            lvlTime.text = string.Format("Best Time:\n{0}", 0f.ToString("n2"));
        }


        // Show Menu
        ShowMenu();

    }



    public void PlayLevel()
    {
        Time.timeScale = 1f;
        ResetMenu();
        SceneManager.LoadSceneAsync(curLevel);
    }

    private void OnEnable()
    {
        levelLoader.loadToybox += toyBoxHandler;
    }

    private void OnDisable()
    {
        levelLoader.loadToybox -= toyBoxHandler;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
