#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FactoryEquipmentAssetSetup
{
    private static readonly string[] EquipmentIds =
    {
        "ROBOT-001", "CNC-001", "CONVEYOR-001", "PRESS-001", "COMPRESSOR-001", "PUMP-001"
    };

    private const string ModelFolder = "Assets/05_Models";
    private const string TextureFolder = "Assets/10_Textures";
    private const string MaterialFolder = "Assets/04_Materials/FactoryEquipment";
    private const string PrefabFolder = "Assets/03_Prefabs/FactoryEquipment";

    [MenuItem("Assets/Factory Equipment/Create Materials and Prefabs")]
    private static void CreateMaterialsAndPrefabs()
    {
        EnsureFolderExists(MaterialFolder);
        EnsureFolderExists(PrefabFolder);

        int created = 0;
        foreach (string id in EquipmentIds)
        {
            try
            {
                Material material = CreateMaterial(id);
                CreatePrefab(id, material);
                created++;
                Debug.Log($"Factory equipment ready: {id}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Could not create assets for {id}: {exception}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Factory equipment",
            $"Created {created} of {EquipmentIds.Length} materials and prefabs. See the Console for details.",
            "OK");
    }

    private static Material CreateMaterial(string id)
    {
        string textureDir = $"{TextureFolder}/{id}";
        string basePath = $"{textureDir}/{id}_BaseColor.png";
        string metallicPath = $"{textureDir}/{id}_Metallic.png";
        string roughnessPath = $"{textureDir}/{id}_Roughness.png";
        string normalPath = $"{textureDir}/{id}_Normal.png";
        string packedPath = $"{textureDir}/{id}_MetallicSmoothness.png";

        RequireTexture(basePath);
        RequireTexture(metallicPath);
        RequireTexture(roughnessPath);
        RequireTexture(normalPath);

        ConfigureTextureImporter(basePath, true, TextureImporterType.Default);
        ConfigureTextureImporter(metallicPath, false, TextureImporterType.Default);
        ConfigureTextureImporter(roughnessPath, false, TextureImporterType.Default);
        ConfigureTextureImporter(normalPath, false, TextureImporterType.NormalMap);
        CreateMetallicSmoothnessMap(metallicPath, roughnessPath, packedPath);

        Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath);
        Texture2D normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        Texture2D metallicSmoothnessMap = AssetDatabase.LoadAssetAtPath<Texture2D>(packedPath);
        if (baseMap == null || normalMap == null || metallicSmoothnessMap == null)
            throw new InvalidOperationException($"One or more texture maps failed to import for {id}.");

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) throw new InvalidOperationException("No URP/Lit or Standard shader is available.");

        string materialPath = $"{MaterialFolder}/{id}_MAT.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(shader);
            material.name = id + "_MAT";
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else
        {
            material.shader = shader;
        }

        SetTexture(material, "_BaseMap", "_MainTex", baseMap);
        SetColor(material, "_BaseColor", "_Color", Color.white);
        SetTexture(material, "_MetallicGlossMap", "_MetallicGlossMap", metallicSmoothnessMap);
        SetTexture(material, "_BumpMap", "_BumpMap", normalMap);
        SetFloat(material, "_Metallic", 1f);
        SetFloat(material, "_Smoothness", 1f);
        SetFloat(material, "_Glossiness", 1f);
        SetFloat(material, "_BumpScale", 0.5f);
        material.EnableKeyword("_METALLICGLOSSMAP");
        material.EnableKeyword("_NORMALMAP");
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateMetallicSmoothnessMap(string metallicPath, string roughnessPath, string outputPath)
    {
        Texture2D metallic = ReadPng(metallicPath);
        Texture2D roughness = ReadPng(roughnessPath);
        Texture2D packed = new Texture2D(metallic.width, metallic.height, TextureFormat.RGBA32, false, true);

        for (int y = 0; y < metallic.height; y++)
        {
            for (int x = 0; x < metallic.width; x++)
            {
                float u = (x + 0.5f) / metallic.width;
                float v = (y + 0.5f) / metallic.height;
                float metallicValue = metallic.GetPixel(x, y).r;
                float smoothness = 1f - roughness.GetPixelBilinear(u, v).r;
                packed.SetPixel(x, y, new Color(metallicValue, metallicValue, metallicValue, smoothness));
            }
        }

        packed.Apply(false, false);
        File.WriteAllBytes(Path.GetFullPath(outputPath), packed.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(metallic);
        UnityEngine.Object.DestroyImmediate(roughness);
        UnityEngine.Object.DestroyImmediate(packed);
        AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
        ConfigureTextureImporter(outputPath, false, TextureImporterType.Default);

        TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        if (importer != null)
        {
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.SaveAndReimport();
        }
    }

    private static Texture2D ReadPng(string assetPath)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        byte[] bytes = File.ReadAllBytes(Path.GetFullPath(assetPath));
        if (!ImageConversion.LoadImage(texture, bytes, false))
            throw new InvalidOperationException($"Could not read {assetPath}.");
        return texture;
    }

    private static void CreatePrefab(string id, Material material)
    {
        string modelPath = $"{ModelFolder}/{id}.fbx";
        GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (sourceModel == null) throw new FileNotFoundException($"Model not found: {modelPath}");

        var root = new GameObject(id);
        try
        {
            GameObject modelInstance = PrefabUtility.InstantiatePrefab(sourceModel) as GameObject;
            if (modelInstance == null) modelInstance = UnityEngine.Object.Instantiate(sourceModel);
            modelInstance.name = id + "_Model";
            modelInstance.transform.SetParent(root.transform, false);

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] slots = renderer.sharedMaterials;
                if (slots == null || slots.Length == 0)
                {
                    renderer.sharedMaterials = new[] { material };
                }
                else
                {
                    for (int i = 0; i < slots.Length; i++) slots[i] = material;
                    renderer.sharedMaterials = slots;
                }
            }

            string prefabPath = $"{PrefabFolder}/{id}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out bool success);
            if (!success) throw new InvalidOperationException($"Prefab save failed: {prefabPath}");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void ConfigureTextureImporter(string assetPath, bool sRGB, TextureImporterType type)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) throw new InvalidOperationException($"Texture importer not found: {assetPath}");
        if (importer.sRGBTexture == sRGB && importer.textureType == type) return;
        importer.sRGBTexture = sRGB;
        importer.textureType = type;
        importer.SaveAndReimport();
    }

    private static void RequireTexture(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath) == null)
            throw new FileNotFoundException($"Texture not found: {assetPath}");
    }

    private static void EnsureFolderExists(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath)) return;
        string parent = Path.GetDirectoryName(assetPath).Replace("\\", "/");
        EnsureFolderExists(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(assetPath));
    }

    private static void SetTexture(Material material, string preferred, string fallback, Texture texture)
    {
        if (material.HasProperty(preferred)) material.SetTexture(preferred, texture);
        else if (material.HasProperty(fallback)) material.SetTexture(fallback, texture);
    }

    private static void SetColor(Material material, string preferred, string fallback, Color color)
    {
        if (material.HasProperty(preferred)) material.SetColor(preferred, color);
        else if (material.HasProperty(fallback)) material.SetColor(fallback, color);
    }

    private static void SetFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property)) material.SetFloat(property, value);
    }
}
#endif
