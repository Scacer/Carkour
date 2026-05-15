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

    public static event Action DoubleJumpCollected;
    public static event Action MagnetCollected;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Animate();
    }

    // Collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (itemType)
            {
                case 0:
                    DoubleJumpCollected?.Invoke(); break;
                case 1:
                    MagnetCollected?.Invoke(); break;
            }
            Color topLidColour = topLid.GetComponent<MeshRenderer>().material.color;
            Color itemColour = item.GetComponent<MeshRenderer>().material.color;

            topLid.GetComponent<MeshRenderer>().material.color = new Color(topLidColour.r, topLidColour.g, topLidColour.b, 0f);
            item.GetComponent<MeshRenderer>().material.color = new Color(itemColour.r, itemColour.g, itemColour.b, 0f);

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
        
    }
}
