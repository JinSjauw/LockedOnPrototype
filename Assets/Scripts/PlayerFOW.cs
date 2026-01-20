using FischlWorks_FogWar;
using UnityEngine;
using static FischlWorks_FogWar.csFogWar;

public class PlayerFOW : MonoBehaviour
{
    private csFogWar fogWar;
    [SerializeField] private FogRevealer fogRevealer;

    private void Awake()
    {
        fogWar = FindFirstObjectByType<csFogWar>();

        if (fogWar) 
        {
            fogWar.AddFogRevealer(fogRevealer);
        }
    }

    private void OnDisable()
    {
        fogWar?.RemoveFogRevealer(fogRevealer);
    }
}
