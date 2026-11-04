using UnityEngine;

public class SelectModel : MonoBehaviour
{
    [SerializeField] private GameObject[] models;
    [SerializeField] private GameObject spawnPoint;

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

        if (models == null || index < 0 || index >= models.Length || models[index] == null)
        {
            Debug.LogWarning($"No model is configured at index {index}.", this);
            return;
        }

        ClearSpawnPoint();

        GameObject instance = Instantiate(
            models[index],
            spawnPoint.transform.position,
            spawnPoint.transform.rotation,
            spawnPoint.transform);

        CenterRenderBounds(instance.transform, spawnPoint.transform.position);
    }

    private void ClearSpawnPoint()
    {
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
