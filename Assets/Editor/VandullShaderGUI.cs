using UnityEditor;
using UnityEngine;

public class VandullShaderGUI : ShaderGUI
{
    private static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
    private static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");
    private static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
    private static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
    private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
    private static readonly int Metallic = Shader.PropertyToID("_Metallic");
    private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
    private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");
    private static readonly int OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");
    private static readonly int AlbedoMap = Shader.PropertyToID("_AlbedoMap");
    private static readonly int WorkflowMode = Shader.PropertyToID("_WorkflowMode");
    private static readonly int SpecularMap = Shader.PropertyToID("_SpecularMap");
    private static readonly int SpecularColor = Shader.PropertyToID("_SpecularColor");
    private static readonly int MetallicMap = Shader.PropertyToID("_MetallicMap");
    private static readonly int RoughnessMap = Shader.PropertyToID("_RoughnessMap");
    private static readonly int UseOutline = Shader.PropertyToID("_UseOutline");
    private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
    private static readonly int VandullCelBandsRadiance = Shader.PropertyToID("_VandullCelBandsRadiance");
    private static readonly int AO = Shader.PropertyToID("_AO");
    private static readonly int NormalStrength = Shader.PropertyToID("_NormalStrength");
    private static readonly int Roughness = Shader.PropertyToID("_Roughness");
    private static readonly int Albedo = Shader.PropertyToID("_Albedo");
    private static readonly int AOMap = Shader.PropertyToID("_AOMap");
    
    private bool _cellShadingFoldout = true;
    private bool _mainTexturesFoldout = true;
    private bool _tilingFoldout = true;
    private bool _normalEffectsFoldout = true;
    private bool _outlineFoldout = true;
    private bool _renderingFoldout = true;
    private bool _xRayFoldout;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var material = materialEditor.target as Material;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Vandull Shader", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Main Textures Section
        _mainTexturesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_mainTexturesFoldout, "Main Textures");
        if (_mainTexturesFoldout)
        {
            EditorGUI.indentLevel++;
            DrawTextureSection(materialEditor, properties, material);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);
        
        _tilingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_tilingFoldout, "Texture Tiling");
        if (_tilingFoldout)
        {
            EditorGUI.indentLevel++;
            DrawTilingSection(materialEditor, properties);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);
        
        // Cell Shading Section
        _cellShadingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_cellShadingFoldout, "Cell Shading");
        if (_cellShadingFoldout)
        {
            EditorGUI.indentLevel++;
            DrawCellShadingSection(materialEditor, properties);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);

        // Outline Section
        _outlineFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_outlineFoldout, "Outline");
        if (_outlineFoldout)
        {
            EditorGUI.indentLevel++;
            DrawOutlineSection(materialEditor, properties, material);
            EditorGUI.indentLevel--;
        }


        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);


        if (material && material.shader.name.ToLower().Contains("xray"))
        {
            _xRayFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_xRayFoldout, "XRay");
            if (_xRayFoldout)
            {
                EditorGUI.indentLevel++;
                DrawXRaySection(materialEditor, properties);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);
        }

        // Normal Effects Section
        _normalEffectsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_normalEffectsFoldout, "Normal Effects");
        if (_normalEffectsFoldout)
        {
            EditorGUI.indentLevel++;
            DrawNormalEffectsSection(materialEditor, properties);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5);

        // Rendering Section
        _renderingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_renderingFoldout, "Rendering");
        if (_renderingFoldout)
        {
            EditorGUI.indentLevel++;
            DrawRenderingSection(materialEditor, properties);
            EditorGUI.indentLevel--;
        }


        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    private void DrawTilingSection(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var textureTiling = FindProperty("_TextureTiling", properties);
        var textureOffset = FindProperty("_TextureOffset", properties);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Tiling", EditorStyles.boldLabel);
        Vector4 tiling = textureTiling.vectorValue;
        Vector2 tilingXY = new Vector2(tiling.x, tiling.y);
        tilingXY = EditorGUILayout.Vector2Field("", tilingXY);
        textureTiling.vectorValue = new Vector4(tilingXY.x, tilingXY.y, 0, 0);

        EditorGUILayout.Space(3);

        EditorGUILayout.LabelField("Offset", EditorStyles.boldLabel);
        Vector4 offset = textureOffset.vectorValue;
        Vector2 offsetXY = new Vector2(offset.x, offset.y);
        offsetXY = EditorGUILayout.Vector2Field("", offsetXY);
        textureOffset.vectorValue = new Vector4(offsetXY.x, offsetXY.y, 0, 0);

        EditorGUILayout.Space(3);
        EditorGUILayout.HelpBox("Applies the same tiling and offset to all textures.", MessageType.Info);

        // Quick preset buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
        {
            textureTiling.vectorValue = new Vector4(1, 1, 0, 0);
            textureOffset.vectorValue = new Vector4(0, 0, 0, 0);
        }
        if (GUILayout.Button("2x Tile"))
        {
            textureTiling.vectorValue = new Vector4(2, 2, 0, 0);
        }
        if (GUILayout.Button("4x Tile"))
        {
            textureTiling.vectorValue = new Vector4(4, 4, 0, 0);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
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
        if (material.GetTexture(NormalMap) != null)
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
        var albedo = material.GetTexture(MainTex);
        var normal = material.GetTexture(BumpMap);
        var metallic = material.GetTexture(MetallicGlossMap);
        var specular = material.GetTexture(SpecGlossMap);
        var occlusion = material.GetTexture(OcclusionMap);
        var emission = material.GetTexture(EmissionMap);

        var albedoColor = material.HasProperty(Color1) ? material.GetColor(Color1) : Color.white;
        var emissionColor = material.HasProperty(EmissionColor) ? material.GetColor(EmissionColor) : Color.black;
        var specularColorOld = material.HasProperty(SpecColor)
            ? material.GetColor(SpecColor)
            : new Color(0.2f, 0.2f, 0.2f, 1f);
        var metallicValue = material.HasProperty(Metallic) ? material.GetFloat(Metallic) : 0f;
        var smoothness = material.HasProperty(Glossiness) ? material.GetFloat(Glossiness) : 0.5f;
        var normalScale = material.HasProperty(BumpScale) ? material.GetFloat(BumpScale) : 1f;
        var occlusionStrength =
            material.HasProperty(OcclusionStrength) ? material.GetFloat(OcclusionStrength) : 1f;

        // Detect old workflow mode
        var wasSpecularWorkflow = oldShader != null && oldShader.name.Contains("Specular");

        // Assign new shader
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        // Map textures to your shader's properties
        if (albedo != null)
        {
            material.SetTexture(AlbedoMap, albedo);
        }

        if (normal != null)
        {
            material.SetTexture(NormalMap, normal);
        }

        if (wasSpecularWorkflow)
        {
            // Coming from specular workflow
            material.SetFloat(WorkflowMode, 1f);
            if (specular != null)
            {
                material.SetTexture(SpecularMap, specular);
            }

            material.SetColor(SpecularColor, specularColorOld);
            material.DisableKeyword("_WORKFLOWMODE_METALLIC");
            material.EnableKeyword("_WORKFLOWMODE_SPECULAR");
        }
        else
        {
            // Coming from metallic workflow
            material.SetFloat(WorkflowMode, 0f);
            if (metallic != null)
            {
                material.SetTexture(MetallicMap, metallic);
                material.SetTexture(RoughnessMap, metallic);
            }

            material.SetFloat(Metallic, metallicValue);
            material.EnableKeyword("_WORKFLOWMODE_METALLIC");
            material.DisableKeyword("_WORKFLOWMODE_SPECULAR");
        }

        if (occlusion != null)
        {
            material.SetTexture(AOMap, occlusion);
        }

        if (emission != null)
        {
            material.SetTexture(EmissionMap, emission);
        }

        // Map property values
        material.SetColor(Albedo, albedoColor);
        material.SetColor(EmissionColor, emissionColor);
        material.SetFloat(Roughness, 1.0f - smoothness);
        material.SetFloat(NormalStrength, normalScale);
        material.SetFloat(AO, occlusionStrength);

        // Set default values for new properties
        if (!material.HasProperty(VandullCelBandsRadiance))
        {
            material.SetFloat(VandullCelBandsRadiance, 6f);
        }

        if (!material.HasProperty(UseOutline))
        {
            material.SetFloat(UseOutline, 1f);
        }

        if (!material.HasProperty(OutlineWidth))
        {
            material.SetFloat(OutlineWidth, 0.02f);
        }
    }
}