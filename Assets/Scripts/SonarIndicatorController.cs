using UnityEditor;
using UnityEngine;

namespace ActiveSensing 
{
    public class SonarIndicatorController : MonoBehaviour
    {
        [Header("Prefab Refs")]
        [SerializeField] private GameObject indicatorPrefab;

        [Header("Indicators")]
        [SerializeField] private Transform indicatorParent;
        [SerializeField] private float startAngle;
        [SerializeField] private float radius;
        [SerializeField] private int segmentCount;
        [SerializeField] private IndicatorSegment[] indicatorArray;

        [Header("Debug Transforms")]
        [SerializeField] private Transform[] testTargets; //TEMP -> fill this with entries from event listeners.

        #region Unity

        private void Awake()
        {
            DeleteIndicators();

            SpawnIndicators(segmentCount, radius, startAngle);

            for (int i = 0; i < indicatorArray.Length; i++) 
            {
                indicatorArray[i].gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            //Check if point is in slice
            UpdateIndicators();
        }

        #endregion

        #region Private

        private void UpdateIndicators() 
        {
            for (int segmentIndex = 0; segmentIndex < indicatorArray.Length; segmentIndex++) 
            {
                IndicatorSegment currentSegment = indicatorArray[segmentIndex];

                for (int targetIndex = 0; targetIndex < testTargets.Length; targetIndex++) 
                {
                   Transform target = testTargets[targetIndex];

                    if (InsideSegmentSlice(target.position, transform.position, transform.up, 25, 0, currentSegment.AngleOne, currentSegment.AngleTwo)) 
                    {
                        currentSegment.AddDetection(target);
                    }
                    else if(currentSegment.Contains(target))
                    {
                        currentSegment.Remove(target);
                    }
                }

                currentSegment.gameObject.SetActive(currentSegment.HasTargets());
            }
        }

        private bool InsideSegmentSlice(Vector3 target, Vector3 center, Vector3 up, float outerRadius, float innerRadius, float angleOne, float angleTwo) 
        {
            Vector3 toQuery = target - center;
            float dist = Vector3.Dot(toQuery, up);      // height off plane
            toQuery = toQuery - up * dist;           // flatten
            float detectionRadius = toQuery.magnitude;

            if (detectionRadius < innerRadius || detectionRadius > outerRadius) return false;  // wrong ring width

            float angle = Vector3.SignedAngle(Vector3.forward, toQuery, up); // -180..180
            if (angle < 0) angle += 360f;                            // 0..360

            if (angleOne <= angleTwo)
            {
                return angle >= angleOne && angle <= angleTwo;
            }
            else 
            { 
                return angle >= angleOne || angle <= angleTwo; 
            }
        }

        private void SpawnIndicators(int segmentCount, float radius, float startAngle, bool inEditor = false) 
        {
            if (indicatorPrefab == null) { Debug.LogError("Assign a prefab"); return; }

            indicatorArray = new IndicatorSegment[segmentCount];

            float angleStep = 360f / segmentCount;
            for (int i = 0; i < segmentCount; ++i)
            {
                float angleOne = startAngle + i * angleStep;
                float angleTwo = startAngle + (i + 1) * angleStep;
                float mid = Mathf.LerpAngle(angleOne, angleTwo, 0.5f);

                Vector3 pos = transform.position +
                              Quaternion.AngleAxis(mid, transform.up) * Vector3.forward * radius;

                GameObject spawnedIndicator = Instantiate(indicatorPrefab, pos, Quaternion.LookRotation(pos - transform.position, transform.up), indicatorParent); // parent under this object
                spawnedIndicator.name = "Indicator: " + i;

                if(spawnedIndicator.TryGetComponent(out IndicatorSegment segment)) 
                {
                    segment.InitSegment(NormalizeAngle(angleOne), NormalizeAngle(angleTwo));
                    indicatorArray[i] = segment;
                }
            }
        }

        float NormalizeAngle(float angle)
        {
            angle = Mathf.Repeat(angle, 360f);   // 0 ≤ a < 360
            if (angle < 0) angle += 360f;
            return angle;
        }

        #endregion

        #region Public

        public void SpawnIndicators() 
        {
            SpawnIndicators(segmentCount, radius, startAngle, true);
        }

        public void DeleteIndicators() 
        {
            for (int i = 0;i < indicatorArray.Length; ++i) 
            {
                DestroyImmediate(indicatorArray[i]);
                indicatorArray[i] = null;
            }
        }

        #endregion
    }


#if UNITY_EDITOR
    
    [CustomEditor(typeof(SonarIndicatorController))]
    class RingSegmentPlacerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Create Indicators"))
                ((SonarIndicatorController)target).SpawnIndicators();

            if (GUILayout.Button("Delete Indicators"))
                ((SonarIndicatorController)target).DeleteIndicators();
        }
    }

#endif
}
