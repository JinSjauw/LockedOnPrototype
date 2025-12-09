#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;

public class LitStencilExtensionEditor : ExtendLitShader
{
    // keep a reference to the material being edited
    MaterialEditor matEditor;
    MaterialProperty[] props;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // store for our own control
        matEditor = materialEditor;
        props = properties;

        // 2.  draw every normal Lit section (base map, metallic, emission, advanced...)
        base.OnGUI(materialEditor, properties);

        // 3.  append our extra control
        DrawExtensionSection();
    }

    void DrawExtensionSection()
    {
        MaterialProperty stencilRef = FindProperty("_StencilRefID", props);
        if (stencilRef == null) return;          // safety

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Extension", EditorStyles.boldLabel);

        matEditor.ShaderProperty(stencilRef, "Stencil Reference");
    }
}
#endif