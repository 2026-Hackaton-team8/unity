using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class ModelOutlineController : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material outlineMaskMaterial;

    private readonly List<GameObject> outlineObjects = new List<GameObject>();

    public void SetHighlighted(Transform target)
    {
        Clear();
        if (target == null)
            return;

        if (outlineMaterial == null || outlineMaskMaterial == null)
        {
            Debug.LogWarning("ModelOutlineController needs outline and silhouette-mask materials.", this);
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
        GameObject maskObject = CreateProxy(
            sourceRenderer,
            mesh,
            outlineMaskMaterial,
            " Selection Outline Mask");
        GameObject outlineObject = CreateProxy(
            sourceRenderer,
            mesh,
            outlineMaterial,
            " Selection Outline");

        outlineObjects.Add(maskObject);
        outlineObjects.Add(outlineObject);
    }

    private static GameObject CreateProxy(Renderer sourceRenderer, Mesh mesh, Material material, string suffix)
    {
        var proxy = new GameObject(
            sourceRenderer.name + suffix,
            typeof(MeshFilter),
            typeof(MeshRenderer));
        proxy.hideFlags = HideFlags.HideInHierarchy;
        proxy.layer = LayerMask.NameToLayer("Ignore Raycast");
        proxy.transform.SetParent(sourceRenderer.transform, false);

        MeshFilter filter = proxy.GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer outlineRenderer = proxy.GetComponent<MeshRenderer>();
        outlineRenderer.sharedMaterial = material;
        outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;
        outlineRenderer.lightProbeUsage = LightProbeUsage.Off;
        outlineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        outlineRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        outlineRenderer.sortingOrder = sourceRenderer.sortingOrder;

        return proxy;
    }
}
