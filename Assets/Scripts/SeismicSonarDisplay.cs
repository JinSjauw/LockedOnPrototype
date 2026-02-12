using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ActiveSensing 
{
    public class SeismicSonarDisplay : MonoBehaviour
    {
        //Circular buffer of Segments
        [SerializeField] private IndicatorSegment[] indicatorArray;
        [SerializeField] private int maxAmount;

        //TEMP
        [SerializeField] private Transform displayQuadTransform;
        [SerializeField] private float segmentHalfWidth;
        private float segmentHalfWidthRad;
        private Bounds drawBounds;

        [SerializeField] private Material blipMaterial;
        [SerializeField] private Mesh blipMesh;
        //[SerializeField] private IndicatorSegment[] loadedArray;

        private ComputeBuffer blipBuffer;
        private ComputeBuffer argsBuffer;

        private Vector2 originXZ;
        private MaterialPropertyBlock blipProperty;

        //When scrolling just lerp the base angle in the blip draw shader!!

        #region Unity

        private void Awake()
        {
            maxAmount = 8 * 4; // segments * detections per segment
            blipBuffer = new ComputeBuffer(maxAmount, 2 * sizeof(float), ComputeBufferType.Default);
            argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

            originXZ = new Vector2(transform.position.x, transform.position.z);

            blipProperty = new MaterialPropertyBlock();
            segmentHalfWidthRad = segmentHalfWidth * Mathf.Deg2Rad;

            float maxPossibleRange = 30 * 2f;
            drawBounds = new Bounds(
                transform.position,
                Vector3.one * maxPossibleRange
            );

            blipProperty.SetFloat("_AngleHalfWidth", segmentHalfWidthRad);
            blipProperty.SetBuffer("_BlipDataBuffer", blipBuffer);
            blipProperty.SetMatrix("_DisplayMatrix", displayQuadTransform.localToWorldMatrix);
            blipProperty.SetMatrix("_DisplayMatrixInv", displayQuadTransform.localToWorldMatrix.inverse);

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            //loadedArray = new IndicatorSegment[3];
        }

        private void Update()
        {
            originXZ = new Vector2(transform.position.x, transform.position.z);
            Graphics.DrawMeshInstancedIndirect(blipMesh, 0, blipMaterial, drawBounds, argsBuffer, 0, blipProperty, castShadows: ShadowCastingMode.Off, receiveShadows: false);
        }

        private void OnDestroy()
        {
            blipBuffer?.Release();
        }

        #endregion

        #region Private

        private Vector2 ConvertToPolar(Vector3 worldPosition) 
        {
            //Calculate delta/distance
            Vector2 enemyXZ = new Vector2(worldPosition.x, worldPosition.z);

            Vector2 delta = enemyXZ - originXZ;
            float distance = delta.magnitude;
            float angle = Mathf.Atan2(delta.x, delta.y);

            if(angle < 0) 
            {
                angle += 2 * Mathf.PI;
            }

            Vector2 result = new Vector2(angle, distance);

            return result;
        }

        private void LoadVisibleBlips(float centerAngleRad) 
        {
            blipProperty.SetFloat("_CenterAngle", centerAngleRad);

            //Load visible blips based on angle cutoff
            Vector2[] segmentDataArray = new Vector2[maxAmount];
            int blipIndex = 0;

            Debug.Log("CenterAngle: " + centerAngleRad);

            for (int i = 0; i < indicatorArray.Length; i++)
            {
                List<Transform> detections = indicatorArray[i].GetDetections();
                for (int j = 0; j < detections.Count; j++)
                {
                    Vector2 blipPolarPos = ConvertToPolar(detections[j].position);
                    
                    if(IsBlipOnScreen(centerAngleRad, blipPolarPos.x)) 
                    {
                        segmentDataArray[blipIndex] = blipPolarPos;

                        Debug.Log("Blip Added: " + blipPolarPos + " Indicator: " + " ID: " + indicatorArray[i].name + " : " + indicatorArray[i].MidAngleRad);

                        blipIndex++;
                    }
                }
            }

            blipBuffer.SetData(segmentDataArray);

            LoadArgs(blipIndex);
        }

        private bool IsBlipOnScreen(float centerAngle, float blipAngle) 
        {
            float delta = blipAngle - centerAngle;
            delta = (delta + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;

            if(Mathf.Abs(delta) <= segmentHalfWidthRad) 
            {
                return true;
            }

            return false;
        }

        private void LoadArgs(int visibileCount) 
        {
            //Construct arg buffer
            uint[] args = new uint[5];
            args[0] = (uint)blipMesh.GetIndexCount(0);
            args[1] = (uint)visibileCount;

            argsBuffer.SetData(args);

            blipProperty.SetMatrix("_DisplayMatrix", displayQuadTransform.localToWorldMatrix);
        }

        #endregion

        #region Public

        public void SetIndicatorArray(IndicatorSegment[] indicatorArray) 
        {
            this.indicatorArray = indicatorArray;
        }

        public void LoadSegments(int selectedIndex) 
        {
            //for (int i = 0; i < loadedArray.Length; i++) 
            //{
            //    loadedArray[i] = indicatorArray[selectedIndices[i]];
            //}
        }

        public void LoadDisplay(float centerAngle)
        {
            LoadVisibleBlips(centerAngle);
        }

        #endregion
    }
}

