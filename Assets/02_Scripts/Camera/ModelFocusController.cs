using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera), typeof(CinemachineOrbitalFollow))]
public sealed class ModelFocusController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private ModelOutlineController outlineController;
    [SerializeField] private ModelViewerCamera viewerCamera;
    [SerializeField, Min(0.05f)] private float focusDuration = 0.55f;
    [SerializeField, Min(1f)] private float framingPadding = 1.15f;
    [SerializeField, Min(0.01f)] private float minRadialAxis = 0.1f;
    [SerializeField, Min(0.1f)] private float maxRadialAxis = 3f;

    private Transform focusTarget;
    private Transform focusedNode;
    private bool isAnimating;
    private float animationTime;
    private float startingRadialAxis;
    private float targetRadialAxis;
    private Vector3 startingPosition;
    private Vector3 targetPosition;

    private void Awake()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineCamera>();
        if (orbitalFollow == null)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        if (outlineController == null)
            outlineController = GetComponent<ModelOutlineController>();
        if (viewerCamera == null)
            viewerCamera = GetComponent<ModelViewerCamera>();

        Transform currentTarget = virtualCamera != null ? virtualCamera.Follow : null;
        var pivot = new GameObject("Hierarchy Focus Target");
        pivot.hideFlags = HideFlags.HideInHierarchy;
        focusTarget = pivot.transform;
        focusTarget.position = currentTarget != null ? currentTarget.position : transform.position;

        if (virtualCamera != null)
            virtualCamera.Follow = focusTarget;
    }

    private void LateUpdate()
    {
        if (focusTarget == null || orbitalFollow == null)
            return;

        if (focusedNode != null && focusedNode.gameObject.activeInHierarchy)
            targetPosition = GetBounds(focusedNode).center;

        if (!isAnimating)
        {
            if (focusedNode != null && focusedNode.gameObject.activeInHierarchy)
                focusTarget.position = targetPosition;
            return;
        }

        animationTime += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(animationTime / Mathf.Max(0.05f, focusDuration));
        float easedTime = 1f - Mathf.Pow(1f - normalizedTime, 3f);

        focusTarget.position = Vector3.Lerp(startingPosition, targetPosition, easedTime);
        SetRadialAxis(Mathf.Lerp(startingRadialAxis, targetRadialAxis, easedTime));

        if (normalizedTime >= 1f)
        {
            focusTarget.position = targetPosition;
            SetRadialAxis(targetRadialAxis);
            isAnimating = false;
        }
    }

    public void SetModelRoot(Transform modelRoot)
    {
        if (outlineController != null)
            outlineController.Clear();

        if (modelRoot == null)
        {
            focusedNode = null;
            isAnimating = false;
            return;
        }

        focusedNode = modelRoot;
        Bounds bounds = GetBounds(modelRoot);
        BeginFocus(bounds);
    }

    public void FocusOn(Transform target)
    {
        if (target == null)
            return;

        if (outlineController != null)
            outlineController.Clear();

        Bounds bounds = GetBounds(target);

        if (outlineController != null)
            outlineController.SetHighlighted(target);

        focusedNode = target;
        BeginFocus(bounds);
    }

    public void FocusOn(Transform target, IEnumerable<Transform> highlightedTargets)
    {
        if (target == null)
            return;

        Bounds bounds = GetBounds(target);

        if (outlineController != null)
            outlineController.SetHighlighted(highlightedTargets);

        focusedNode = target;
        BeginFocus(bounds);
    }

    private void BeginFocus(Bounds bounds)
    {
        if (focusTarget == null || orbitalFollow == null)
            return;

        if (viewerCamera != null)
            viewerCamera.ResetPanForFocus();

        animationTime = 0f;
        startingPosition = focusTarget.position;
        targetPosition = bounds.center;
        startingRadialAxis = orbitalFollow.RadialAxis.Value;
        targetRadialAxis = CalculateRadialAxis(bounds);
        isAnimating = true;
    }

    private float CalculateRadialAxis(Bounds bounds)
    {
        float radius = Mathf.Max(bounds.extents.magnitude, 0.05f);
        Camera outputCamera = Camera.main;
        float fieldOfView = outputCamera != null ? outputCamera.fieldOfView : 60f;
        float halfFovRadians = Mathf.Clamp(fieldOfView, 10f, 120f) * 0.5f * Mathf.Deg2Rad;
        float distance = radius * framingPadding / Mathf.Max(0.1f, Mathf.Sin(halfFovRadians));
        float orbitRadius = Mathf.Max(0.01f, orbitalFollow.Radius);
        return Mathf.Clamp(distance / orbitRadius, minRadialAxis, maxRadialAxis);
    }

    private void SetRadialAxis(float value)
    {
        var radialAxis = orbitalFollow.RadialAxis;
        radialAxis.Value = value;
        orbitalFollow.RadialAxis = radialAxis;
    }

    private static Bounds GetBounds(Transform target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(false);
        bool hasBounds = false;
        Bounds bounds = new Bounds(target.position, Vector3.zero);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;
            if ((renderer.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
            bounds = new Bounds(target.position, Vector3.one * 0.25f);

        return bounds;
    }
}
