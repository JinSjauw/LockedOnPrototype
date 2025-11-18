using TMPro;
using UnityEngine;

public class RadarHologram : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textID;

    public void SetID(string id) 
    {
        textID.text = id;
    }
}
