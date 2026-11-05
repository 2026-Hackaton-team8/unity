using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectModel : MonoBehaviour
{
    [SerializeField] private EquipmentProductData[] products;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private ModelHierarchyController hierarchyController;

    public EquipmentProductData CurrentProduct { get; private set; }
    public Transform CurrentModelRoot { get; private set; }
    public IReadOnlyList<EquipmentProductData> Products => products;

    private void Awake()
    {
        if (hierarchyController == null)
            hierarchyController = FindFirstObjectByType<ModelHierarchyController>();
    }

    [ContextMenu("Select FirstModel")]
    public void SelectFirstModel()
    {
        if (products == null || products.Length == 0 || products[0] == null)
        {
            Debug.LogWarning("No product data is configured.", this);
            return;
        }

        SpawnModel(products[0]);
    }

    /// <summary>Selects and spawns a model by its prefab name, product name, or stable product ID.</summary>
    public bool SelectModelByName(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName) || products == null)
            return false;

        EquipmentProductData match = null;
        foreach (EquipmentProductData product in products)
        {
            if (product == null || !MatchesModelName(product, modelName))
                continue;

            if (match != null)
            {
                Debug.LogWarning($"Model name '{modelName}' matches more than one product. Use a unique prefab name or product ID.", this);
                return false;
            }

            match = product;
        }

        if (match == null)
        {
            Debug.LogWarning($"No product is configured for model name '{modelName}'.", this);
            return false;
        }

        return SpawnModel(match);
    }

    private bool SpawnModel(EquipmentProductData product)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("SelectModel needs a spawn point.", this);
            return false;
        }

        if (product == null)
        {
            Debug.LogWarning("No product data is configured for the requested model.", this);
            return false;
        }

        if (product.ModelPrefab == null)
        {
            Debug.LogWarning($"No model prefab is configured for product {product.ProductId}.", product);
            return false;
        }

        ClearSpawnPoint();

        GameObject instance = Instantiate(
            product.ModelPrefab,
            spawnPoint.transform.position,
            spawnPoint.transform.rotation,
            spawnPoint.transform);

        CenterRenderBounds(instance.transform, spawnPoint.transform.position);
        CurrentProduct = product;
        CurrentModelRoot = instance.transform;
        return true;
    }

    public bool SelectModelById(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId) || products == null)
            return false;

        for (int i = 0; i < products.Length; i++)
        {
            if (products[i] == null || !string.Equals(products[i].ProductId, productId, System.StringComparison.OrdinalIgnoreCase))
                continue;

            return SpawnModel(products[i]);
        }

        return false;
    }

    private static bool MatchesModelName(EquipmentProductData product, string modelName)
    {
        return string.Equals(product.ProductId, modelName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(product.ProductName, modelName, StringComparison.OrdinalIgnoreCase)
            || (product.ModelPrefab != null
                && string.Equals(product.ModelPrefab.name, modelName, StringComparison.OrdinalIgnoreCase));
    }

    public bool SelectPartById(string partId)
    {
        if (CurrentProduct == null || CurrentModelRoot == null)
        {
            Debug.LogWarning("Select an equipment model before selecting one of its parts.", this);
            return false;
        }

        if (!CurrentProduct.TryGetPart(partId, out EquipmentPartData part))
        {
            Debug.LogWarning($"Part ID '{partId}' is not configured for equipment '{CurrentProduct.ProductId}'.", this);
            return false;
        }

        Transform target = FindPartTransform(part.ModelPath);
        if (target == null)
        {
            Debug.LogWarning(
                $"Could not find model path '{part.ModelPath}' for part '{part.PartId}' in '{CurrentProduct.ProductId}'.",
                CurrentProduct);
            return false;
        }

        if (hierarchyController == null)
            hierarchyController = FindFirstObjectByType<ModelHierarchyController>();

        if (hierarchyController != null)
            return hierarchyController.SelectFromExternal(target);

        ModelFocusController focusController = FindFirstObjectByType<ModelFocusController>();
        if (focusController == null)
            return false;

        focusController.FocusOn(target);
        return true;
    }

    public bool TryGetPartForTransform(Transform target, out EquipmentPartData matchedPart)
    {
        matchedPart = null;
        if (target == null || CurrentProduct == null || CurrentModelRoot == null || CurrentProduct.Parts == null)
            return false;

        int closestPartDistance = int.MaxValue;
        foreach (EquipmentPartData part in CurrentProduct.Parts)
        {
            if (part == null)
                continue;

            Transform partRoot = FindPartTransform(part.ModelPath);
            if (partRoot == null || (target != partRoot && !target.IsChildOf(partRoot)))
                continue;

            int distance = 0;
            Transform ancestor = target;
            while (ancestor != null && ancestor != partRoot)
            {
                distance++;
                ancestor = ancestor.parent;
            }

            if (ancestor != partRoot || distance >= closestPartDistance)
                continue;

            closestPartDistance = distance;
            matchedPart = part;
        }

        return matchedPart != null;
    }

    private Transform FindPartTransform(string modelPath)
    {
        if (string.IsNullOrWhiteSpace(modelPath) || CurrentModelRoot == null)
            return null;

        Transform target = CurrentModelRoot.Find(modelPath);
        if (target != null || modelPath.IndexOf('/') >= 0)
            return target;

        Transform uniqueMatch = null;
        Transform[] nodes = CurrentModelRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform node in nodes)
        {
            if (!string.Equals(node.name, modelPath, StringComparison.Ordinal))
                continue;

            if (uniqueMatch != null)
            {
                Debug.LogWarning($"Model path '{modelPath}' matches more than one node. Set a full relative path in the product data.", this);
                return null;
            }

            uniqueMatch = node;
        }

        return uniqueMatch;
    }

    private void ClearSpawnPoint()
    {
        CurrentProduct = null;
        CurrentModelRoot = null;
        Transform spawnTransform = spawnPoint.transform;
        for (int i = spawnTransform.childCount - 1; i >= 0; i--)
        {
            GameObject child = spawnTransform.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private static void CenterRenderBounds(Transform modelRoot, Vector3 targetPosition)
    {
        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>();
        bool hasBounds = false;
        Bounds combinedBounds = default;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            if (!hasBounds)
            {
                combinedBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        if (hasBounds)
            modelRoot.position += targetPosition - combinedBounds.center;
    }
}
