using UnityEngine;

public class CollectibleCount : MonoBehaviour
{
    TMPro.TMP_Text text;
    int count;

    private void Awake()
    {
        text = GetComponent<TMPro.TMP_Text>();
        text.text = "0 / 3";
    }

    void OnEnable() => Collectible.OnCollected += OnCollectibleCollected;
    
    void OnCollectibleCollected()
    {
        text.text = (++count).ToString() + " / 3";
    }
}
