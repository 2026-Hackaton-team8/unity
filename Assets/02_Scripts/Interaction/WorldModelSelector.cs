using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(1000)]
public sealed class WorldModelSelector : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ModelHierarchyController hierarchyController;

    private Camera selectionCamera;
    private Transform currentModelRoot;
    private Renderer[] modelRenderers = System.Array.Empty<Renderer>();
    private PickCandidate[] pickCandidates = System.Array.Empty<PickCandidate>();
    private static bool hasLoggedColliderWarning;

    private void Awake()
    {
        selectionCamera = Camera.main;
    }

    private void LateUpdate()
    {
        RefreshModelRenderers();

        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;
        if (!Application.isFocused || selectionCamera == null)
            return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.IsPointerOverGameObject())
            return;

        Ray ray = selectionCamera.ScreenPointToRay(mouse.position.ReadValue());
        Transform hitTarget = FindClosestRenderer(ray);
        if (hitTarget != null && hierarchyController != null)
            hierarchyController.SelectFromWorld(hitTarget);
    }

    private void RefreshModelRenderers()
    {
        Transform modelRoot = FindActiveModelRoot();
        if (modelRoot == currentModelRoot)
            return;

        currentModelRoot = modelRoot;
        modelRenderers = currentModelRoot != null
            ? currentModelRoot.GetComponentsInChildren<Renderer>(false)
            : System.Array.Empty<Renderer>();

        var candidates = new List<PickCandidate>(modelRenderers.Length);
        foreach (Renderer renderer in modelRenderers)
        {
            if (renderer == null || (renderer.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                continue;

            Collider collider = renderer.GetComponent<Collider>();
            if (collider == null && renderer is MeshRenderer)
                collider = AddMeshCollider(renderer);

            candidates.Add(new PickCandidate(renderer, collider));
        }

        pickCandidates = candidates.ToArray();
    }

    private Transform FindActiveModelRoot()
    {
        if (spawnPoint == null)
            return null;

        for (int i = 0; i < spawnPoint.childCount; i++)
        {
            Transform child = spawnPoint.GetChild(i);
            if (child.gameObject.activeInHierarchy
                && (child.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0)
                return child;
        }

        return null;
    }

    private Transform FindClosestRenderer(Ray ray)
    {
        Transform closestTarget = null;
        float closestDistance = selectionCamera.farClipPlane;
        bool needsBoundsFallback = false;

        Physics.SyncTransforms();
        foreach (PickCandidate candidate in pickCandidates)
        {
            Renderer renderer = candidate.Renderer;
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;
            if ((renderer.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                continue;

            if (candidate.Collider == null || !candidate.Collider.enabled)
            {
                needsBoundsFallback = true;
                continue;
            }

            if (!candidate.Collider.Raycast(ray, out RaycastHit hit, closestDistance))
                continue;

            closestDistance = hit.distance;
            closestTarget = renderer.transform;
        }

        if (closestTarget != null)
            return closestTarget;
        if (!needsBoundsFallback)
            return null;

        foreach (PickCandidate candidate in pickCandidates)
        {
            Renderer renderer = candidate.Renderer;
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;
            if ((renderer.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                continue;
            if (candidate.Collider != null && candidate.Collider.enabled)
                continue;

            if (!renderer.bounds.IntersectRay(ray, out float distance) || distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = renderer.transform;
        }

        return closestTarget;
    }

    private static MeshCollider AddMeshCollider(Renderer renderer)
    {
        MeshFilter filter = renderer.GetComponent<MeshFilter>();
        if (filter == null || filter.sharedMesh == null)
            return null;

        MeshCollider meshCollider = null;
        try
        {
            meshCollider = renderer.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
            meshCollider.convex = false;
            meshCollider.isTrigger = false;
            return meshCollider;
        }
        catch (UnityException exception)
        {
            if (meshCollider != null)
                UnityEngine.Object.Destroy(meshCollider);

            if (!hasLoggedColliderWarning)
            {
                Debug.LogWarning($"Mesh collision picking is unavailable for some model parts. Renderer bounds will be used there. {exception.Message}", renderer);
                hasLoggedColliderWarning = true;
            }

            return null;
        }
    }

    private readonly struct PickCandidate
    {
        public readonly Renderer Renderer;
        public readonly Collider Collider;

        public PickCandidate(Renderer renderer, Collider collider)
        {
            Renderer = renderer;
            Collider = collider;
        }
    }
}
