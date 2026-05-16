using System;
using UnityEngine;

public class GachaItem : MonoBehaviour
{
    [SerializeField] private int itemType;
    private float rotSpeed = 50f;
    private float posSpeed = 2f;
    private float height = 0.00125f;

    [Header("Components")]
    [SerializeField] private GameObject topLid;
    [SerializeField] private GameObject item;

    private bool isCollected = false;
    private float timeCollected = -1f;

    public static event Action DoubleJumpCollected;
    public static event Action MagnetCollected;

    [Header("Respawn Timer")]
    [SerializeField] private float respawnDuration = 5;
    [SerializeField] private Camera follow;
    [SerializeField] private SpriteRenderer sprite;

    public static event Action ItemRespawned;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isCollected = false;
        sprite.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Animate();
        Respawn();
    }

    // Collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isCollected == false)
        {
            switch (itemType)
            {
                case 1:
                    DoubleJumpCollected?.Invoke(); break;
                case 2:
                    MagnetCollected?.Invoke(); break;
            }

            SetTransparency(0f, 0f);

            isCollected = true;
            timeCollected = Time.time;
            sprite.enabled = true;

        }


    }

    // Animation
    private void Animate()
    {
        transform.localRotation = Quaternion.Euler(15, Time.time * rotSpeed, 0);
        // Up and Down Transform
        Vector3 pos = transform.position;
        float newY = (Mathf.Sin(Time.time * posSpeed) * height) + pos.y;
        transform.localPosition = new Vector3(pos.x, newY, pos.z);

        // Reload Wheel animations
        sprite.transform.forward = follow.transform.forward;

        if (isCollected == true)
        {
            int radialFillFraction = (int)((Time.time - timeCollected) / respawnDuration * 360);
            sprite.GetComponent<Renderer>().sharedMaterial.SetInt("_Arc2", 360 - radialFillFraction);
        }
        
    }

    // Respawn
    private void Respawn()
    {
        // If the respawn timer is reached
        if (isCollected == true && timeCollected > 0f && Time.time - timeCollected > respawnDuration)
        {
            ItemRespawned?.Invoke();
            SetTransparency(0.5f, 1f);

            isCollected = false;
            timeCollected = -1f;
            sprite.enabled = false;
        }
    }

    private void SetTransparency(float lidTransparency, float itemTransparency)
    {
        Color topLidColour = topLid.GetComponent<MeshRenderer>().material.color;
        Color itemColour = item.GetComponent<MeshRenderer>().material.color;

        topLid.GetComponent<MeshRenderer>().material.color = new Color(topLidColour.r, topLidColour.g, topLidColour.b, lidTransparency);
        item.GetComponent<MeshRenderer>().material.color = new Color(itemColour.r, itemColour.g, itemColour.b, itemTransparency);
    }
}
