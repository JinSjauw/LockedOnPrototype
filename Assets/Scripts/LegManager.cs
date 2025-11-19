using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class LegManager : MonoBehaviour
{
    [SerializeField] private LegController[] legGroupA;
    [SerializeField] private LegController[] legGroupB;

    private List<LegController> allLegs;

    [SerializeField] private Transform bodyTransform;
    [SerializeField] private float heightOffset;

    [SerializeField] private int legsMoved = 0;
    [SerializeField] private LegGroups groupToCheck = LegGroups.LEG_A;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allLegs = new List<LegController>();

        for (int i = 0; i < legGroupA.Length; i++) 
        {
            legGroupA[i].OnMoveFinished += CheckLegs;
            allLegs.Add(legGroupA[i]);
        }

        for (int i = 0; i < legGroupB.Length; i++)
        {
            legGroupB[i].OnMoveFinished += CheckLegs;
            allLegs.Add(legGroupB[i]);
        }

        EnableLegGroup(LegGroups.LEG_A);
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate & Set body height based on average from legPositions
        UpdateHeight();
        UpdateRotation();
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

    private void UpdateHeight() 
    {
        float totalHeight = 0;

        for(int i = 0; i < allLegs.Count; i++) 
        {
            totalHeight += allLegs[i].GetTargetPosition().y;
        }

        totalHeight = totalHeight / allLegs.Count;

        Vector3 newBodyPosition = bodyTransform.position;
        newBodyPosition.y = totalHeight + heightOffset;
        bodyTransform.position = newBodyPosition;
    }

    private void UpdateRotation() 
    {
        List<Vector3> localLegs = new List<Vector3>();
        Vector3 center = Vector3.zero;

        for (int i = 0; i < allLegs.Count; i++) 
        {
            Vector3 worldPosition = allLegs[i].GetTargetPosition();
            Vector3 localPosition = bodyTransform.parent.InverseTransformPoint(worldPosition);
            
            center += localPosition;

            localLegs.Add(localPosition);
        }

        center = center / localLegs.Count;

        float averageXSlope = 0;
        float averageZSlope = 0;

        int xSlopeAdded = 0;
        int zSlopeAdded = 0;

        for (int i = 0;i < allLegs.Count; i++) 
        {
            Vector3 position = localLegs[i];

            Vector3 toPoint = position - center;
            float heightDiff = position.y - center.y;

            if (toPoint.x < 0.1f) 
            {
                averageXSlope += heightDiff / toPoint.x;
                xSlopeAdded += 1;
            }

            if(toPoint.z < 0.1f) 
            {
                averageZSlope += heightDiff / toPoint.z;
                zSlopeAdded += 1;
            }
        }

        averageXSlope /= xSlopeAdded;
        averageZSlope /= zSlopeAdded;

        float rotationX = Mathf.Atan(averageZSlope) * Mathf.Rad2Deg;
        float rotationZ = Mathf.Atan(averageXSlope) * Mathf.Rad2Deg;

        //Quaternion currentRotation = bodyTransform.rotation;
        Quaternion tiltRotation = Quaternion.Euler(rotationX, 0, -rotationZ);
        bodyTransform.rotation = Quaternion.Euler(0, bodyTransform.rotation.eulerAngles.y, 0) * tiltRotation;
        
        //bodyTransform.rotation = Quaternion.Euler(0, bodyTransform.rotation.eulerAngles.y, 0) * tiltRotation;
    }

    private void CheckLegs(object sender, int legID)
    {
        if(groupToCheck == LegGroups.LEG_A) 
        {
            for (int i = 0; i < legGroupA.Length; i++)
            {
                Debug.Log("LegID: " + legID + " " + legGroupA[i].GetID());
                if (legID == legGroupA[i].GetID())
                {
                    legsMoved++;
                }
            }

            if (legsMoved >= legGroupA.Length)
            {
                legsMoved = 0;
                groupToCheck = LegGroups.LEG_B;
                EnableLegGroup(groupToCheck);
            }
        }
        
        if(groupToCheck == LegGroups.LEG_B)
        {
            for (int i = 0; i < legGroupB.Length; i++)
            {
                if (legID == legGroupB[i].GetID())
                {
                    legsMoved++;
                }
            }

            if (legsMoved >= legGroupB.Length)
            {
                legsMoved = 0;
                groupToCheck = LegGroups.LEG_A;
                EnableLegGroup(groupToCheck);
            }
        }
    }

    private void EnableLegGroup(LegGroups groupToEnable) 
    {
        for (int i = 0; i < legGroupA.Length; i++)
        {
            legGroupA[i].SetCanUpdate(groupToEnable == LegGroups.LEG_A);
        }

        for (int i = 0; i < legGroupB.Length; i++)
        {
            legGroupB[i].SetCanUpdate(groupToEnable == LegGroups.LEG_B);
        }
    }
}

public enum LegGroups 
{
    LEG_A = 0,
    LEG_B = 1,
}
