using System.IO;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class endCardManager : MonoBehaviour
{

    [SerializeField] private string levelName;
    [SerializeField] private GameObject endCard;
    [SerializeField] private RaceLine goals;
    [SerializeField] private raceTimer curTimer;
    
    [Header("Stars")]
    [SerializeField] private Image firstStar;
    [SerializeField] private Image secondStar;
    [SerializeField] private Image thirdStar;

    [Header("Time Fields")]
    [SerializeField] private TMP_Text yourTimeText;
    [SerializeField] private TMP_Text bronzeTime;
    [SerializeField] private TMP_Text silverTime;
    [SerializeField] private TMP_Text goldTime;

    [Header("Control Pauses")]
    [SerializeField] private GameObject pauseMenu;
    private bool isPaused = false;
    [SerializeField] private CarController carController;
    [SerializeField] private CinemachineInputAxisController camInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenu.SetActive(false);
        enforceDirectory();
        endCard.SetActive(false);
        firstStar.enabled = false;
        secondStar.enabled = false;
        thirdStar.enabled = false;
    }

    public static void enforceDirectory()
    {
        string thisPath = Path.Combine(Application.persistentDataPath, "/levels");
        if (!Directory.Exists(thisPath))
        {
            Directory.CreateDirectory(thisPath);
        }
    }

    private void OnEnable()
    {
        RaceLine.NoStar += NoStars;
        RaceLine.OneStar += OneStar;
        RaceLine.TwoStars += TwoStars;
        RaceLine.ThreeStars += ThreeStars;
    }

    private void OnDisable()
    {
        RaceLine.NoStar -= NoStars;
        RaceLine.OneStar -= OneStar;
        RaceLine.TwoStars -= TwoStars;
        RaceLine.ThreeStars -= ThreeStars;
    }

    // Buttons
    public void Hub()
    {
        SceneManager.LoadScene("city");
    }

    public void Retry()
    {
        string curScene = SceneManager.GetActiveScene().name;
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(curScene);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            togglePause();
        }
    }

    private void togglePause()
    {
        if (isPaused)
        {
            AudioListener.pause = false;
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isPaused = false;
            
        }
        else
        {
            AudioListener.pause = true;
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
            isPaused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
            
    }

    private void handleWin()
    {
        endCard.SetActive(true);
        SetTimes();
        PrimeMenu();
        SaveData();
        
    }

    private void SetTimes()
    {
        yourTimeText.text += curTimer.currentTime.ToString("n2");
        bronzeTime.text = goals.OneStarsTime.ToString("n2");
        silverTime.text = goals.TwoStarsTime.ToString("n2");
        goldTime.text = goals.ThreeStarsTime.ToString("n2");
    }

    public void PrimeMenu()
    {
        AudioListener.pause = true;
        carController.enabled = false;
        camInput.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SaveData()
    {
        int numStars = 0;
        if (thirdStar.enabled)
        {
            numStars++;
        }
        if (secondStar.enabled)
        {
            numStars++;
        }
        if (thirdStar)
        {
            numStars++;
        }

        var curData = new SaveData();
        curData.Name = levelName;
        curData.numStars = numStars;
        curData.bestTime = curTimer.currentTime;


        string jsonData = JsonUtility.ToJson(curData);
        string fileName = string.Format("/levels/{0}.json", levelName);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log(path);

        int historicNumStars = getNumStars(levelName);
        float historicBestTime = getBestTime(levelName);

        // Star Number overwrite handler
        if (historicNumStars != -1)
        {
            if (numStars > historicNumStars)
            {
                curData.numStars = historicNumStars;
            }
        }
        // Best Time overwrite handler
        if (historicBestTime != -1f)
        {
            if (curData.bestTime < historicBestTime)
            {
                curData.bestTime = curTimer.currentTime;
            }
        }

        File.WriteAllText(path, jsonData);

        

    }

 

    // Returns the best number of stars for a specified level, returning -1 if no file is found
    public static int getNumStars(string lvlName)
    {
        string fileName = string.Format("/levels/{0}.json", lvlName);
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            string dataString = File.ReadAllText(path);
            var saveData = JsonUtility.FromJson<SaveData>(dataString);

            return saveData.numStars;
        }
        else
        {
            return -1;
        }
    }

    // Returns the best time for a specified level, returning -1 if no file is found
    public static float getBestTime(string lvlName)
    {
        string fileName = string.Format("/levels/{0}.json", lvlName);
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            string dataString = File.ReadAllText(path);
            var saveData = JsonUtility.FromJson<SaveData>(dataString);

            return saveData.bestTime;
        }
        else
        {
            return -1f;
        }
    }

    private void NoStars()
    {
        handleWin();
    }

    private void OneStar()
    {
        firstStar.enabled = true;
        handleWin();
    }

    private void TwoStars()
    {
        firstStar.enabled = true;
        secondStar.enabled = true;
        handleWin();
    }

    private void ThreeStars()
    {
        firstStar.enabled = true;
        secondStar.enabled= true;
        thirdStar.enabled = true;
        handleWin();
    }

    public void Quit()
    {
        Debug.Log("Player has Quit the game.");
        Application.Quit();
    }
}

public struct SaveData
{
    public string Name;
    public int numStars;
    public float bestTime;
}
