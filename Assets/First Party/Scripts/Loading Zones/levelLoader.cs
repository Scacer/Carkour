using System;
using UnityEngine;

public class levelLoader : MonoBehaviour
{
    private float rotSpeed = 50f;

    public static event Action loadToybox;

    [SerializeField] private string lvlName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Animate();
    }

    private void Animate()
    {
        transform.localRotation = Quaternion.Euler(0, Time.time * rotSpeed, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            handleTrigger();
        }
    }

    private void handleTrigger()
    {
        if (lvlName == "Toybox")
        {
            loadToybox?.Invoke();
        }
    }
}
