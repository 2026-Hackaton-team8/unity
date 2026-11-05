using UnityEngine;

public class SelectModel : MonoBehaviour
{
    [SerializeField] private EquipmentProductData[] products;
    [SerializeField] private GameObject spawnPoint;

    public EquipmentProductData CurrentProduct { get; private set; }
    public Transform CurrentModelRoot { get; private set; }

    [ContextMenu("Select FirstModel")]
    public void SelectFirstModel()
    {
        SelectModelByIndex(0);
    }

    public void SelectModelByIndex(int index)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("SelectModel needs a spawn point.", this);
            return;
        }

        if (products == null || index < 0 || index >= products.Length || products[index] == null)
        {
            Debug.LogWarning($"No product data is configured at index {index}.", this);
            return;
        }

        EquipmentProductData product = products[index];
        if (product.ModelPrefab == null)
        {
            Debug.LogWarning($"No model prefab is configured for product {product.ProductId}.", product);
            return;
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
    }

    public bool SelectModelById(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId) || products == null)
            return false;

        for (int i = 0; i < products.Length; i++)
        {
            if (products[i] == null || !string.Equals(products[i].ProductId, productId, System.StringComparison.OrdinalIgnoreCase))
                continue;

            SelectModelByIndex(i);
            return CurrentProduct == products[i];
        }

        return false;
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
