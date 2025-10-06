using UnityEngine;
using UnityEngine.InputSystem;

public class RadarController : MonoBehaviour
{

    [SerializeField] private Material scannerMaterial;
    [SerializeField] private Transform targetTransform;

    [SerializeField] private AnimationCurve expansionCurve;
    [SerializeField] private AnimationCurve fadeCurve;

    [SerializeField] private float maxRange;
    [SerializeField] private float timeToEnd;
    [SerializeField] private float timeToFade;
    [SerializeField] private float scanningCooldown;

    private float expansionTimer;
    private float fadeTimer;
    private float cooldownTimer;

    private bool canScan = true;
    private bool isScanning = false;

    #region Input

    private void OnRadarScan(InputValue value) 
    {
        if (value.isPressed) 
        {
            if (!canScan) return;

            Follow();
            isScanning = true;
            canScan = false;
        }
    }

    #endregion

    #region Unity

    private void Start()
    {
                
    }

    private void Update()
    {
        if (isScanning) 
        {
            Expand(Time.deltaTime);
            Fade(Time.deltaTime);
        }

        if (cooldownTimer < scanningCooldown && isScanning) 
        {
            cooldownTimer += Time.deltaTime;

            if(cooldownTimer > scanningCooldown) 
            {
                ResetScan();
            }
        }
    }

    #endregion

    #region Functions

    private void Follow() 
    {
        scannerMaterial.SetVector("_Terrain_Scan_Origin", targetTransform.position);
    }

    private void Expand(float deltaTime) 
    {
        if(expansionTimer < timeToEnd) 
        {
            expansionTimer += deltaTime;

            float expansionTimer01 = (expansionTimer - 0) / (timeToEnd - 0);
            float scannerRadius = Mathf.Lerp(0, maxRange, expansionCurve.Evaluate(expansionTimer01));

            scannerMaterial.SetFloat("_Terrain_Scan_Range", scannerRadius);
        }
    }

    private void Fade(float deltaTime) 
    {
        if (fadeTimer < timeToFade)
        {
            fadeTimer += deltaTime;

            float fadeTimer01 = (fadeTimer - 0) / (timeToFade - 0);
            float scannerVisibility = Mathf.Lerp(1, 0, fadeCurve.Evaluate(fadeTimer01));

            scannerMaterial.SetFloat("_Global_Visibility", scannerVisibility);
        }
    }

    private void ResetScan()
    {
        expansionTimer = 0;
        fadeTimer = 0;
        cooldownTimer = 0;
        isScanning = false;
        canScan = true;
    }

    #endregion
}
