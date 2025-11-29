using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private UIEvents uiEvents;
    [SerializeField] private Image ammoFillImage;
    [SerializeField] private TextMeshProUGUI ammoAmountText;
    
    private int ammoAmount;
    private float reloadTime;
    private float reloadTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiEvents.onReloadEvent += SetReloadUI;
        uiEvents.onAmmoChangedEvent += SetAmmoUI;
    }

    private void SetAmmoUI(object sender, int amount)
    {
        ammoAmount = amount;
        ammoAmountText.text = ammoAmount.ToString();
    }

    private void SetReloadUI(object sender, float time)
    {
        reloadTimer = time;
        reloadTime = time;
    }

    // Update is called once per frame
    void Update()
    {
        if(reloadTimer > 0) 
        {
            reloadTimer -= Time.deltaTime;

            float reloadAlpha = (reloadTimer - 0) / (reloadTime - 0);
            reloadAlpha = Mathf.Clamp01(reloadAlpha);
            ammoFillImage.fillAmount = 1 - reloadAlpha;
        }
        else 
        {
            ammoFillImage.fillAmount = 1;
        }
    }
}
