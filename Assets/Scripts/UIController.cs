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

    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private TextMeshProUGUI enemiesAmountText;

    private int maxEnemies;

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

        uiEvents.newWaveEvent += SetWaveNumberUI;
        uiEvents.enemyAmountChangedEvent += SetEnemyAmountUI;
    }

    private void SetEnemyAmountUI(object sender, int amount)
    {
        enemiesAmountText.text = amount.ToString() + "/" + maxEnemies;
    }

    private void SetWaveNumberUI(int waveNumber, int maxAmount)
    {
        waveNumberText.text = "Wave " + waveNumber.ToString();
        maxEnemies = maxAmount;
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
        healthText.text = ((int)health).ToString();
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
