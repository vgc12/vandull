using UnityEditor;
using UnityEngine;

public class VandullShaderGUI : ShaderGUI
{
    private bool cellShadingFoldout = true;
    private bool mainTexturesFoldout = true;
    private bool normalEffectsFoldout = true;
    private bool outlineFoldout = true;
    private bool renderingFoldout = true;
    private bool xRayFoldout;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var material = materialEditor.target as Material;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Vandull Shader", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Main Textures Section
        mainTexturesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(mainTexturesFoldout, "Main Textures");
        if (mainTexturesFoldout)
        {
            EditorGUI.indentLevel++;
            DrawTextureSection(materialEditor, properties, material);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);

        // Cell Shading Section
        cellShadingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(cellShadingFoldout, "Cell Shading");
        if (cellShadingFoldout)
        {
            EditorGUI.indentLevel++;
            DrawCellShadingSection(materialEditor, properties);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);

        // Outline Section
        outlineFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(outlineFoldout, "Outline");
        if (outlineFoldout)
        {
            EditorGUI.indentLevel++;
            DrawOutlineSection(materialEditor, properties, material);
            EditorGUI.indentLevel--;
        }


        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);


        if (material && material.shader.name.ToLower().Contains("xray"))
        {
            xRayFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(xRayFoldout, "XRay");
            if (xRayFoldout)
            {
                EditorGUI.indentLevel++;
                DrawXRaySection(materialEditor, properties);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);
        }

        // Normal Effects Section
        normalEffectsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(normalEffectsFoldout, "Normal Effects");
        if (normalEffectsFoldout)
        {
            EditorGUI.indentLevel++;
            DrawNormalEffectsSection(materialEditor, properties);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);

        // Rendering Section
        renderingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(renderingFoldout, "Rendering");
        if (renderingFoldout)
        {
            EditorGUI.indentLevel++;
            DrawRenderingSection(materialEditor, properties);
            EditorGUI.indentLevel--;
        }


        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTextureSection(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
    {
        // Albedo
        EditorGUILayout.LabelField("Albedo", EditorStyles.boldLabel);
        var albedoMap = FindProperty("_AlbedoMap", properties);
        var albedoTint = FindProperty("_Albedo", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("Albedo Map"), albedoMap, albedoTint);

        EditorGUILayout.Space(3);

        // Normal
        EditorGUILayout.LabelField("Normal", EditorStyles.boldLabel);
        var normalMap = FindProperty("_NormalMap", properties);
        var normalStrength = FindProperty("_NormalStrength", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), normalMap);
        if (material.GetTexture("_NormalMap") != null)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(normalStrength, "Strength");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(3);

        // Workflow Mode Selection
        EditorGUILayout.LabelField("Workflow Mode", EditorStyles.boldLabel);
        var workflowMode = FindProperty("_WorkflowMode", properties);

        EditorGUI.BeginChangeCheck();
        materialEditor.ShaderProperty(workflowMode, "Workflow");
        if (EditorGUI.EndChangeCheck())
        {
            // Update shader keywords
            if (workflowMode.floatValue == 0) // Metallic
            {
                material.DisableKeyword("_WORKFLOWMODE_SPECULAR");
                material.EnableKeyword("_WORKFLOWMODE_METALLIC");
            }
            else // Specular
            {
                material.DisableKeyword("_WORKFLOWMODE_METALLIC");
                material.EnableKeyword("_WORKFLOWMODE_SPECULAR");
            }
        }

        EditorGUILayout.Space(3);

        // Show appropriate workflow properties
        var isMetallicWorkflow = workflowMode.floatValue == 0;

        if (isMetallicWorkflow)
        {
            // Metallic Workflow
            EditorGUILayout.LabelField("Metallic", EditorStyles.boldLabel);
            var metallicMap = FindProperty("_MetallicMap", properties);
            var metallic = FindProperty("_Metallic", properties);

            materialEditor.TexturePropertySingleLine(new GUIContent("Metallic Map"), metallicMap, metallic);
        }
        else
        {
            // Specular Workflow
            EditorGUILayout.LabelField("Specular", EditorStyles.boldLabel);
            var specularMap = FindProperty("_SpecularMap", properties);
            var specularColor = FindProperty("_SpecularColor", properties);

            materialEditor.TexturePropertySingleLine(new GUIContent("Specular Map"), specularMap, specularColor);
            EditorGUILayout.HelpBox("Specular workflow uses RGB specular color instead of metallic value.",
                MessageType.Info);
        }

        EditorGUILayout.Space(3);

        // Roughness (common to both workflows)
        EditorGUILayout.LabelField("Roughness", EditorStyles.boldLabel);
        var roughnessMap = FindProperty("_RoughnessMap", properties);
        var roughness = FindProperty("_Roughness", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("Roughness Map"), roughnessMap, roughness);

        EditorGUILayout.Space(3);

        // AO
        EditorGUILayout.LabelField("Ambient Occlusion", EditorStyles.boldLabel);
        var aoMap = FindProperty("_AOMap", properties);
        var ao = FindProperty("_AO", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("AO Map"), aoMap);
        if (material.GetTexture("_AOMap") != null)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(ao, "Strength");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(3);

        // Emission
        EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
        var emissionMap = FindProperty("_EmissionMap", properties);
        var emissionColor = FindProperty("_EmissionColor", properties);
        var emissionStrength = FindProperty("_EmissionStrength", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("Emission Map"), emissionMap, emissionColor);
        if (material.GetTexture("_EmissionMap") != null || emissionColor.colorValue != Color.black)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(emissionStrength, "Strength");
            EditorGUI.indentLevel--;
        }
    }

    private void DrawXRaySection(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var xRayColor = FindProperty("_XRayColor", properties);
        var xRayIntensity = FindProperty("_XRayIntensity", properties);
        var xRayEnabled = FindProperty("_XRayEnabled", properties);
        var alpha = FindProperty("_Alpha", properties);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        materialEditor.ShaderProperty(xRayColor, "X-Ray Color");
        materialEditor.ShaderProperty(xRayIntensity, "X-Ray Intensity");
        materialEditor.ShaderProperty(xRayEnabled, "Enable X-Ray");
        materialEditor.ShaderProperty(alpha, "Alpha");
        EditorGUILayout.HelpBox("Enables an X-Ray effect that highlights edges and silhouettes.", MessageType.Info);
        EditorGUILayout.EndVertical();
    }

    private void DrawCellShadingSection(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var celBands = FindProperty("_VandullCelBandsRadiance", properties);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        materialEditor.ShaderProperty(celBands, "Cel Shading Bands");
        EditorGUILayout.HelpBox("Controls the number of lighting bands. Lower = more stylized, Higher = smoother.",
            MessageType.Info);
        EditorGUILayout.EndVertical();
    }

    private void DrawOutlineSection(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
    {
        var useOutline = FindProperty("_UseOutline", properties);
        var outlineColor = FindProperty("_OutlineColor", properties);
        var outlineWidth = FindProperty("_OutlineWidth", properties);
        var outlineMethod = FindProperty("_OutlineMethod", properties);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        materialEditor.ShaderProperty(useOutline, "Enable Outline");

        if (useOutline.floatValue > 0.5f)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(outlineColor, "Color");
            materialEditor.ShaderProperty(outlineWidth, "Width");
            materialEditor.ShaderProperty(outlineMethod, "Extrude From Normals");

            if (outlineMethod.floatValue > 0.5f)
            {
                EditorGUILayout.HelpBox("Uses vertex normals for extrusion. Better for organic shapes.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Uses uniform scaling. Better for hard-surface models.", MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        // Quick preset buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Thin Outline"))
        {
            outlineWidth.floatValue = 0.01f;
        }

        if (GUILayout.Button("Medium Outline"))
        {
            outlineWidth.floatValue = 0.02f;
        }

        if (GUILayout.Button("Thick Outline"))
        {
            outlineWidth.floatValue = 0.05f;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawNormalEffectsSection(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var normalEffectsColor = FindProperty("_NormalEffectsColor", properties);
        var normalThreshold = FindProperty("_NormalThreshold", properties);
        var colorX = FindProperty("_ColorX", properties);
        var colorY = FindProperty("_ColorY", properties);
        var colorZ = FindProperty("_ColorZ", properties);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        materialEditor.ShaderProperty(normalEffectsColor, "Color");
        materialEditor.ShaderProperty(normalThreshold, "Threshold");

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Apply to Axes", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        materialEditor.ShaderProperty(colorX, "X Axis");
        materialEditor.ShaderProperty(colorY, "Y Axis");
        materialEditor.ShaderProperty(colorZ, "Z Axis");
        EditorGUI.indentLevel--;

        EditorGUILayout.HelpBox("Applies color based on surface normal direction.", MessageType.Info);

        EditorGUILayout.EndVertical();
    }

    private void DrawRenderingSection(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var zTestMode = FindProperty("_ZTestMode", properties);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        materialEditor.ShaderProperty(zTestMode, "Z Test Mode");
        EditorGUILayout.HelpBox("Controls depth testing. Default: LEqual (4)", MessageType.Info);
        EditorGUILayout.EndVertical();

        // Render queue display
        var material = materialEditor.target as Material;
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Render Queue", material.renderQueue.ToString());
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        // Store old texture references
        var albedo = material.GetTexture("_MainTex");
        var normal = material.GetTexture("_BumpMap");
        var metallic = material.GetTexture("_MetallicGlossMap");
        var specular = material.GetTexture("_SpecGlossMap");
        var occlusion = material.GetTexture("_OcclusionMap");
        var emission = material.GetTexture("_EmissionMap");

        var albedoColor = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
        var emissionColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;
        var specularColorOld = material.HasProperty("_SpecColor")
            ? material.GetColor("_SpecColor")
            : new Color(0.2f, 0.2f, 0.2f, 1f);
        var metallicValue = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
        var smoothness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.5f;
        var normalScale = material.HasProperty("_BumpScale") ? material.GetFloat("_BumpScale") : 1f;
        var occlusionStrength =
            material.HasProperty("_OcclusionStrength") ? material.GetFloat("_OcclusionStrength") : 1f;

        // Detect old workflow mode
        var wasSpecularWorkflow = oldShader != null && oldShader.name.Contains("Specular");

        // Assign new shader
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        // Map textures to your shader's properties
        if (albedo != null)
        {
            material.SetTexture("_AlbedoMap", albedo);
        }

        if (normal != null)
        {
            material.SetTexture("_NormalMap", normal);
        }

        if (wasSpecularWorkflow)
        {
            // Coming from specular workflow
            material.SetFloat("_WorkflowMode", 1f);
            if (specular != null)
            {
                material.SetTexture("_SpecularMap", specular);
            }

            material.SetColor("_SpecularColor", specularColorOld);
            material.DisableKeyword("_WORKFLOWMODE_METALLIC");
            material.EnableKeyword("_WORKFLOWMODE_SPECULAR");
        }
        else
        {
            // Coming from metallic workflow
            material.SetFloat("_WorkflowMode", 0f);
            if (metallic != null)
            {
                material.SetTexture("_MetallicMap", metallic);
                material.SetTexture("_RoughnessMap", metallic);
            }

            material.SetFloat("_Metallic", metallicValue);
            material.EnableKeyword("_WORKFLOWMODE_METALLIC");
            material.DisableKeyword("_WORKFLOWMODE_SPECULAR");
        }

        if (occlusion != null)
        {
            material.SetTexture("_AOMap", occlusion);
        }

        if (emission != null)
        {
            material.SetTexture("_EmissionMap", emission);
        }

        // Map property values
        material.SetColor("_Albedo", albedoColor);
        material.SetColor("_EmissionColor", emissionColor);
        material.SetFloat("_Roughness", 1.0f - smoothness);
        material.SetFloat("_NormalStrength", normalScale);
        material.SetFloat("_AO", occlusionStrength);

        // Set default values for new properties
        if (!material.HasProperty("_VandullCelBandsRadiance"))
        {
            material.SetFloat("_VandullCelBandsRadiance", 6f);
        }

        if (!material.HasProperty("_UseOutline"))
        {
            material.SetFloat("_UseOutline", 1f);
        }

        if (!material.HasProperty("_OutlineWidth"))
        {
            material.SetFloat("_OutlineWidth", 0.02f);
        }
    }
}