using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class ModelOutlineController : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;

    private readonly List<GameObject> outlineObjects = new List<GameObject>();

    public void SetHighlighted(Transform target)
    {
        Clear();
        if (target == null)
            return;

        if (outlineMaterial == null)
        {
            Debug.LogWarning("ModelOutlineController needs an outline material.", this);
            return;
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(false);
        foreach (Renderer sourceRenderer in renderers)
        {
            if (sourceRenderer == null || !sourceRenderer.enabled)
                continue;
            if ((sourceRenderer.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                continue;

            if (!(sourceRenderer is MeshRenderer))
                continue;

            MeshFilter sourceFilter = sourceRenderer.GetComponent<MeshFilter>();
            Mesh mesh = sourceFilter != null ? sourceFilter.sharedMesh : null;
            if (mesh == null)
                continue;

            CreateOutlineProxy(sourceRenderer, mesh);
        }
    }

    public void Clear()
    {
        foreach (GameObject outlineObject in outlineObjects)
        {
            if (outlineObject != null)
                Destroy(outlineObject);
        }

        outlineObjects.Clear();
    }

    private void CreateOutlineProxy(Renderer sourceRenderer, Mesh mesh)
    {
        Transform sourceTransform = sourceRenderer.transform;
        var outlineObject = new GameObject(
            sourceRenderer.name + " Selection Outline",
            typeof(MeshFilter),
            typeof(MeshRenderer));
        outlineObject.hideFlags = HideFlags.HideInHierarchy;
        outlineObject.transform.SetParent(sourceTransform, false);

        MeshFilter filter = outlineObject.GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer outlineRenderer = outlineObject.GetComponent<MeshRenderer>();
        outlineRenderer.sharedMaterial = outlineMaterial;
        outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;
        outlineRenderer.lightProbeUsage = LightProbeUsage.Off;
        outlineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        outlineRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        outlineRenderer.sortingOrder = sourceRenderer.sortingOrder;

        outlineObjects.Add(outlineObject);
    }
}
