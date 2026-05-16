using UnityEngine;
using UnityEngine.UI;

public class itemBoxHandler : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private Image primaryItemBox;
    [SerializeField] private Image secondaryItemBox;
    [SerializeField] private Image itemTimer;
    [SerializeField] private CarController player;

    [Header("Item Sprites")]
    [SerializeField] private Sprite doubleJump;
    [SerializeField] private Sprite magnet;

    private float magnetTimer = 0f;

    private Sprite[] jumpBoostDummyItems = new Sprite[2];
    private float jumpBoostTimer = 0f;
    private float jumpBoostTimerDuration = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemTimer.enabled = false;
        loadPowerupIcons();
    }

    private void OnEnable()
    {
        // Item display is managed when items are collected or used
        CarController.itemCollected += loadPowerupIcons;
        CarController.itemUsed += loadPowerupIcons;

        // Controls "flair" when jump boost is used
        CarController.itemUsed += startJumpBoostTimer;

        // Controls magnet duration display
        CarController.wallRideBegin += startMagnetTimer;    
    }

    // Update is called once per frame
    void Update()
    {
        checkEmptyBox();
        jumpBoostTimerHandler();
        magnetTimerHandler();
    }

#region Timer Logic
    private void startMagnetTimer()
    {
        magnetTimer = Time.time; 
        itemTimer.enabled = true;
    }

    private void magnetTimerHandler()
    {
        // If timer has been activated, animate the cooldown graphic
        if (magnetTimer != 0f && Time.time - magnetTimer < player.magnetDuration)
        {
            itemTimer.fillAmount = 1 - (Time.time - magnetTimer)/player.magnetDuration;
        }
        // If the timer has exceeded the limit, set timer back to 0
        else if (magnetTimer != 0 && Time.time - magnetTimer > player.magnetDuration)
        {
            magnetTimer = 0f;
            itemTimer.enabled = false;
        }

    }

    private void startJumpBoostTimer()
    {
        jumpBoostTimer = Time.time;
    }

    private void jumpBoostTimerHandler()
    {
        if (jumpBoostTimer != 0f && Time.time - jumpBoostTimer < jumpBoostTimerDuration) {
            Debug.Log(itemTimer.enabled);
            // Prevents sprite being set every frame
            if (!primaryItemBox.enabled)
            {
                primaryItemBox.enabled = true;
                primaryItemBox.sprite = jumpBoostDummyItems[0];
            }
            // Prevents sprite being set every frame
            else if (!secondaryItemBox.enabled)
            {
                secondaryItemBox.enabled = true;
                secondaryItemBox.sprite = jumpBoostDummyItems[1];
            }
            // Keeps jump boost "flair" visible for its duration
            else if (itemTimer.enabled == false) { itemTimer.enabled = true; }
        }
        if(jumpBoostTimer != 0f && Time.time - jumpBoostTimer > jumpBoostTimerDuration)
        {
            jumpBoostTimer = 0f;
            itemTimer.enabled = false;
            loadPowerupIcons();
        }
    }
    #endregion

    private void checkEmptyBox()
    {
        if (primaryItemBox.sprite == null) {primaryItemBox.color = new Color(primaryItemBox.color.r, primaryItemBox.color.g, primaryItemBox.color.b, 0f);}
        if (secondaryItemBox.sprite == null) {secondaryItemBox.color = new Color(secondaryItemBox.color.r, secondaryItemBox.color.g, secondaryItemBox.color.b, 0f);}
    }

    void loadPowerupIcons()
    {
        // Before any changes are made, save the previous item set for use in jumpBoost's lingering "flair"
        jumpBoostDummyItems[0] = primaryItemBox.sprite; 
        jumpBoostDummyItems[1] = secondaryItemBox.sprite;

        // Obtain the contents of the player's item boxes
        CarController.Powerup[] curItems = player.collectedItems;
        // First Item Box
        primaryItemBox.enabled = true; primaryItemBox.color = new Color(primaryItemBox.color.r, primaryItemBox.color.g, primaryItemBox.color.b, 1f);
        switch ((int)curItems[0])
        {
            case 0:
                // If no item, the sprite is disabled
                primaryItemBox.sprite = null;break;
            case 1:
                // If item is a double jump, load its sprite
                primaryItemBox.sprite = doubleJump;break;
            case 2:
                // If item is a magnet, load its sprite
                primaryItemBox.sprite = magnet;break;
            default:
                break;
        }

        // Second Item Box
        secondaryItemBox.enabled = true; secondaryItemBox.color = new Color(secondaryItemBox.color.r, secondaryItemBox.color.g, secondaryItemBox.color.b, 1f);
        switch ((int)curItems[1])
        {
            case 0:
                secondaryItemBox.sprite = null;break;
            case 1:
                secondaryItemBox.sprite = doubleJump;break;
            case 2:
                secondaryItemBox.sprite = magnet;break;
            default:
                break;
        }
    }
}
