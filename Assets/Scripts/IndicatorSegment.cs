using System.Collections.Generic;
using UnityEngine;

namespace ActiveSensing
{
    public class IndicatorSegment : MonoBehaviour
    {
        [SerializeField] private List<Transform> detectionsList;

        private float angleOne;
        private float angleTwo;

        public float AngleOne { get => angleOne; }
        public float AngleTwo { get => angleTwo; }

        public float MidAngle 
        {
            get 
            {
                float start = angleOne;
                float end = angleTwo;

                if (start > end)
                {
                    // Wrap-around: add 360 to end, then normalize result
                    end += 360f;
                    float mid = (start + end) / 2f;
                    return mid % 360f;
                }

                return (start + end) / 2f;
            }
        }

        public void InitSegment(float angleOne, float angleTwo)
        {
            this.angleOne = angleOne;
            this.angleTwo = angleTwo;

            detectionsList = new List<Transform>();
        }

        public void AddDetection(Transform target) 
        {
            if (detectionsList.Contains(target)) return;

            detectionsList.Add(target);
        }

        public bool Contains(Transform target)
        {
            return detectionsList.Contains(target);
        }

        public void Remove(Transform target)
        {
            detectionsList.Remove(target);
        }

        public bool HasTargets()
        {
            return detectionsList.Count > 0;
        }
    }
}
