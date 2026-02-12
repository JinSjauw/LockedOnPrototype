using UnityEngine;

namespace ActiveSensing 
{
    public class SeismicSonarSegmentSelector : MonoBehaviour
    {
        private IndicatorSegment[] indicatorArray;
        public void SetIndicatorArray(IndicatorSegment[] indicatorArray) 
        {
            this.indicatorArray = indicatorArray;
        }

        public int SelectIndicatorIndex(Vector3 direction)
        {
            Vector2 input = new Vector2(direction.x, direction.z);

            if (input.sqrMagnitude < 0.01f) return -1;

            // Convert input to angle
            float inputAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
            if (inputAngle < 0) inputAngle += 360f;

            // Find closest point
            int smallestIndex = 0;
            float smallest = float.MaxValue;

            for (int i = 0; i < indicatorArray.Length; i++)
            {
                float angleDiff = Mathf.Abs(inputAngle - indicatorArray[i].MidAngle);
                if (angleDiff > 180f) angleDiff = 360f - angleDiff; // Wrap around

                if (angleDiff < smallest)
                {
                    smallest = angleDiff;
                    smallestIndex = i;
                }
            }

            // Save selected
            return smallestIndex;
        }
    }
}

