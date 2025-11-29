using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    [SerializeField] List<Material> outlineMaterials;
    [SerializeField] List<MeshRenderer> meshRenderers;

    private float outlineTimer;
    private bool isScanned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();

        for (int i = 0; i < meshRenderers.Count; i++) 
        {
            Material material;

            if (meshRenderers[i].materials.Length > 1) 
            {
                material = meshRenderers[i].materials[1];
            }
            else 
            {
                material = meshRenderers[i].materials[0];
            }
            
            outlineMaterials.Add(material);
        }

        //ChangeStencilID(0);
    }

    private void Update()
    {
        //Time for outline to dissappear
        if(outlineTimer < 2 && isScanned) 
        {
            outlineTimer += Time.deltaTime;
        }
        else 
        {
            isScanned = false;
            outlineTimer = 0;
            ChangeStencilID(0);
        }
    }

    public void ChangeStencilID(int ID) 
    {
        isScanned = true;
        outlineTimer = 0;

        for(int i = 0; i < outlineMaterials.Count; i++) 
        {
            outlineMaterials[i].SetInteger("_StencilRefID", ID);
        }
    }
}
