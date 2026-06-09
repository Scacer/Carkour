using UnityEngine;

public class BarrierDestruction : MonoBehaviour
{

    [SerializeField] GameObject firstBarrier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        firstBarrier.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        LevelMenu.FirstBarrierBroken += FirstBarrier;
    }

    private void OnDisable()
    {
        LevelMenu.FirstBarrierBroken -= FirstBarrier;
    }

    private void FirstBarrier()
    {
        firstBarrier.SetActive(false);
    }
}
