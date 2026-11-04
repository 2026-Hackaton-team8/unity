using System.Collections.Generic;
using UnityEngine;

public sealed class ModelExploder : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float spreadMultiplier = 0.5f;

    private readonly List<ExplodedPart> parts = new List<ExplodedPart>();
    private Transform currentModelRoot;
    private float explosionAmount;

    private void LateUpdate()
    {
        Transform modelRoot = GetModelRoot();
        if (modelRoot == currentModelRoot)
            return;

        CacheParts(modelRoot);
        ApplyExplosion();
    }

    // Assign this method to the Slider's On Value Changed (Single) event.
    public void SetExplosionAmount(float amount)
    {
        explosionAmount = Mathf.Clamp01(amount);

        Transform modelRoot = GetModelRoot();
        if (modelRoot != currentModelRoot)
            CacheParts(modelRoot);

        ApplyExplosion();
    }

    private Transform GetModelRoot()
    {
        return transform.childCount > 0 ? transform.GetChild(0) : null;
    }

    private void CacheParts(Transform modelRoot)
    {
        currentModelRoot = modelRoot;
        parts.Clear();

        if (modelRoot == null)
            return;

        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(false);
        bool hasBounds = false;
        Bounds modelBounds = default;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            if (!hasBounds)
            {
                modelBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                modelBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
            return;

        var partTransforms = new HashSet<Transform>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            Transform part = renderer.transform;
            if (part == modelRoot || HasRendererAncestor(part, modelRoot))
                continue;

            if (!partTransforms.Add(part))
                continue;

            Bounds partBounds = renderer.bounds;
            Vector3 direction = partBounds.center - modelBounds.center;
            if (direction.sqrMagnitude < 0.0001f)
                direction = part.position - modelBounds.center;
            if (direction.sqrMagnitude < 0.0001f)
                direction = GetFallbackDirection(parts.Count);
            else
                direction.Normalize();

            parts.Add(new ExplodedPart(
                part,
                part.position,
                direction,
                modelBounds.extents.magnitude * spreadMultiplier));
        }
    }

    private static bool HasRendererAncestor(Transform part, Transform modelRoot)
    {
        for (Transform ancestor = part.parent; ancestor != null && ancestor != modelRoot; ancestor = ancestor.parent)
        {
            if (ancestor.GetComponent<Renderer>() != null)
                return true;
        }

        return false;
    }

    private static Vector3 GetFallbackDirection(int index)
    {
        const float goldenAngle = 2.39996323f;
        float y = 1f - 2f * ((index + 0.5f) / Mathf.Max(index + 1f, 2f));
        float radius = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
        float angle = index * goldenAngle;
        return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
    }

    private void ApplyExplosion()
    {
        foreach (ExplodedPart part in parts)
        {
            if (part.Transform == null)
                continue;

            part.Transform.position = part.OriginalPosition + part.Direction * part.MaxDistance * explosionAmount;
        }
    }

    private readonly struct ExplodedPart
    {
        public readonly Transform Transform;
        public readonly Vector3 OriginalPosition;
        public readonly Vector3 Direction;
        public readonly float MaxDistance;

        public ExplodedPart(Transform transform, Vector3 originalPosition, Vector3 direction, float maxDistance)
        {
            Transform = transform;
            OriginalPosition = originalPosition;
            Direction = direction;
            MaxDistance = maxDistance;
        }
    }
}
