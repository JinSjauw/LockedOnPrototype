using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private UIEvents uiEvents;

    [SerializeField] private Image mainAmmoFillImage;
    [SerializeField] private TextMeshProUGUI mainAmmoAmountText;

    [SerializeField] private Image interceptAmmoFillImage;
    [SerializeField] private TextMeshProUGUI interceptAmmoAmountText;

    [SerializeField] private TextMeshProUGUI healthText;

    private int ammoAmount;
    private float reloadTime;
    private float reloadTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiEvents.reloadMainGunEvent += SetMainReloadUI;
        uiEvents.mainGunAmmoChangedEvent += SetMainAmmoUI;

        uiEvents.reloadInterceptGunEvent += SetInterceptReloadUI;
        uiEvents.interceptGunAmmoChangedEvent += SetInterceptAmmoUI;

        uiEvents.healthChangedEvent += SetHealthUI;
    }

    private void SetMainAmmoUI(object sender, int amount)
    {
        ammoAmount = amount;
        mainAmmoAmountText.text = ammoAmount.ToString();
    }

    private void SetMainReloadUI(object sender, float time)
    {
        reloadTimer = time;
        reloadTime = time;
    }

    private void SetInterceptReloadUI(object sender, float timeAlpha)
    {
        timeAlpha = Mathf.Clamp01(timeAlpha);
        interceptAmmoFillImage.fillAmount = timeAlpha;
    }

    private void SetInterceptAmmoUI(object sender, int amount)
    {
        interceptAmmoAmountText.text = amount.ToString();
    }
    private void SetHealthUI(object sender, float health)
    {
        healthText.text = health.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(reloadTimer > 0) 
        {
            reloadTimer -= Time.deltaTime;

            float reloadAlpha = (reloadTimer - 0) / (reloadTime - 0);
            reloadAlpha = Mathf.Clamp01(reloadAlpha);
            mainAmmoFillImage.fillAmount = 1 - reloadAlpha;
        }
        else 
        {
            mainAmmoFillImage.fillAmount = 1;
        }
    }
}
