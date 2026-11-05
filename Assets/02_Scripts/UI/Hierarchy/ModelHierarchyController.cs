using System.Collections.Generic;
using UnityEngine;

public sealed class ModelHierarchyController : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ModelFocusController focusController;
    [SerializeField] private HierarchyPanelView hierarchyPanel;

    private readonly HashSet<Transform> selectedTargets = new HashSet<Transform>();
    private Transform currentModelRoot;

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        if (focusController == null)
            focusController = FindFirstObjectByType<ModelFocusController>();
        if (hierarchyPanel == null)
            hierarchyPanel = FindFirstObjectByType<HierarchyPanelView>();

        if (hierarchyPanel == null)
            hierarchyPanel = HierarchyPanelView.Create(SelectFromHierarchy);
        else
            hierarchyPanel.Initialize(SelectFromHierarchy);
    }

    private void LateUpdate()
    {
        RefreshCurrentModel();
    }

    private void RefreshCurrentModel()
    {
        Transform modelRoot = FindActiveModelRoot();
        if (modelRoot == currentModelRoot)
            return;

        currentModelRoot = modelRoot;
        selectedTargets.Clear();
        hierarchyPanel.SetModel(modelRoot);

        if (focusController != null)
            focusController.SetModelRoot(modelRoot);
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

    public void SelectFromWorld(Transform target)
    {
        SelectTarget(target, false);
    }

    public void SelectFromWorld(Transform target, bool additiveSelection)
    {
        SelectTarget(target, additiveSelection);
    }

    public bool SelectFromExternal(Transform target)
    {
        RefreshCurrentModel();
        if (target == null || currentModelRoot == null)
            return false;
        if (target != currentModelRoot && !target.IsChildOf(currentModelRoot))
            return false;

        SelectTarget(target, false);
        return true;
    }

    private void SelectFromHierarchy(Transform target, bool additiveSelection)
    {
        SelectTarget(target, additiveSelection);
    }

    private void SelectTarget(Transform target, bool additiveSelection)
    {
        if (target == null || currentModelRoot == null)
            return;
        if (target != currentModelRoot && !target.IsChildOf(currentModelRoot))
            return;

        if (!additiveSelection)
            selectedTargets.Clear();
        selectedTargets.Add(target);

        hierarchyPanel.SetSelectedTargets(selectedTargets, target);
        if (focusController != null)
            focusController.FocusOn(target, selectedTargets);
    }
}
