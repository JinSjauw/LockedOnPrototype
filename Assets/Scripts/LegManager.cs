using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LegManager : MonoBehaviour
{
    [SerializeField] private LegController[] legGroupA;
    [SerializeField] private LegController[] legGroupB;

    private int legsMoved = 0;
    private LegGroups groupToCheck = LegGroups.LEG_A;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < legGroupA.Length; i++) 
        {
            legGroupA[i].OnMoveFinished += CheckLegs;
        }

        for (int i = 0; i < legGroupB.Length; i++)
        {
            legGroupB[i].OnMoveFinished += CheckLegs;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        for (int i = 0; i < legGroupA.Length; i++)
        {
            legGroupA[i].OnMoveFinished -= CheckLegs;
        }

        for (int i = 0; i < legGroupB.Length; i++)
        {
            legGroupB[i].OnMoveFinished -= CheckLegs;
        }
    }

    private void CheckLegs(object sender, int legID)
    {
        if(groupToCheck == LegGroups.LEG_A) 
        {
            for (int i = 0; i < legGroupA.Length; i++)
            {
                if (legID == legGroupA[i].GetID())
                {
                    legsMoved++;
                }
            }

            if (legsMoved > legGroupA.Length)
            {
                legsMoved = 0;
                EnableLegGroup(false);
                groupToCheck = LegGroups.LEG_B;
            }
        }
        else if(groupToCheck == LegGroups.LEG_B)
        {
            for (int i = 0; i < legGroupB.Length; i++)
            {
                if (legID == legGroupB[i].GetID())
                {
                    legsMoved++;
                }
            }

            if (legsMoved > legGroupB.Length)
            {
                legsMoved = 0;
                EnableLegGroup(true);
                groupToCheck = LegGroups.LEG_A;
            }
        }
    }

    private void EnableLegGroup(bool enableA) 
    {
        for (int i = 0; i < legGroupA.Length; i++)
        {
            legGroupA[i].SetCanUpdate(enableA);
        }

        for (int i = 0; i < legGroupB.Length; i++)
        {
            legGroupB[i].SetCanUpdate(!enableA);
        }
    }
}

public enum LegGroups 
{
    LEG_A = 0,
    LEG_B = 1,
}
