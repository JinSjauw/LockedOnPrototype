using UnityEngine;
using FOVMapping;
using System.Collections.Generic;
using System.Linq;

namespace FOVMapping
{
	//When dynamiclly spawning agents. need to register with the FOV manager; Events?
	//Objectpooling objects likely get their old position before being reused sampled by the FOW. Results in a brief flicker when spawned in and enabled

	// Attach this component to 'eyes' of the field of view.
	// Works only when this component is enabled.
	public class FOVAgent : MonoBehaviour
	{
		[Tooltip("Is this agent an eye(set to true for friendly agents and false for hostile agents)?")]
		[SerializeField]
		private bool _contributeToFOV = true;
		public bool contributeToFOV { get => _contributeToFOV; set => _contributeToFOV = value; }

		[Tooltip("How far can an agent see? This value must be equal to or less than the samplingRange of a generated FOV map.")]
		[SerializeField]
		[Range(0.0f, 1000.0f)]
		private float _sightRange = 50.0f;
		public float sightRange { get => _sightRange; set => _sightRange = value; }

		[Tooltip("How widely can an agent see?")]
		[SerializeField]
		[Range(0.0f, 360.0f)]
		private float _sightAngle = 240.0f;
		public float sightAngle { get => _sightAngle; set => _sightAngle = value; }

		[Tooltip("Will this agent disappear if it is in a fog of war(set to true for hostile agents and false for friendly agents)?")]
		[SerializeField]
		private bool _disappearInFOW = false;
		public bool disappearInFOW { get => _disappearInFOW; set => _disappearInFOW = value; }

		[Tooltip("On the boundary of a field of view, if an agent with `disappearInFOW` set to true is under a fog of war whose opacity is larger than this value, the agent disappears.")]
		[SerializeField]
		[Range(0.0f, 1.0f)]
		private float _disappearAlphaThreshold = 0.1f;
		public float disappearAlphaThreshold { get => _disappearAlphaThreshold; set => _disappearAlphaThreshold = value; }
		private bool isUnderFOW = false;

		[SerializeField] private Transform parentRenderObject;
		private List<MeshRenderer> meshRenderers;

		//SO used for communications across FOV components
		[SerializeField] private FOVEvents fovEvents;

		[SerializeField] private bool forceInvisible = true;
		[SerializeField] private float invisibilityTime = .2f;
		[SerializeField] private float invisibilityTimer;

        private bool isInvisible;
		public bool ForceInvisible { get => forceInvisible; set => forceInvisible = value; }

        private void Awake()
		{
			if(parentRenderObject != null) 
			{
				meshRenderers = parentRenderObject.GetComponentsInChildren<MeshRenderer>().ToList();
			}
			else 
			{
                meshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();
            }
		}

        private void Start()
        {
            if(fovEvents != null) 
			{
				fovEvents.OnAgentRegister(this);
			}
        }

        private void OnEnable()
        {
			if (forceInvisible) 
			{
                invisibilityTimer = 0;
                isInvisible = true;
                SetUnderFOW(false);
			}
			else //This code might not be needed
			{
				isInvisible = false;
                SetVisible(true);
            }
        }

        private void Update()
        {
			if (invisibilityTimer < invisibilityTime && forceInvisible) 
			{
				invisibilityTimer += Time.deltaTime;
			}
			else 
			{
				isInvisible = false;
			}
        }

        private void OnDisable()
        {
			this.enabled = false;
            invisibilityTimer = 0;
            isInvisible = true;
        }

		private void OnDestroy() 
		{
			meshRenderers.Clear();
		}
		public void SetVisible(bool visible) 
		{
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                meshRenderers[i].enabled = true;
            }

			this.enabled = true;
        }

        [HideInInspector]
		public void SetUnderFOW(bool isUnder)
		{
			if (isInvisible) 
			{
                for (int i = 0; i < meshRenderers.Count; i++)
                {
                    meshRenderers[i].enabled = false;
                }
                return;
            }

			isUnderFOW = isUnder;

			if (disappearInFOW)
			{
				for (int  i = 0; i < meshRenderers.Count; i++) 
				{
					meshRenderers[i].enabled = isUnder;
				}
			}
		}

		public bool IsVisible()
		{
			return isUnderFOW;
		}

        
    }
}