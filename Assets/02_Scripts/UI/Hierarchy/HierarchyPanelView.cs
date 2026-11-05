using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class HierarchyPanelView : MonoBehaviour
{
    private const float PanelWidth = 360f;
    private const float PanelHeight = 680f;

    private readonly List<NodeRow> rows = new List<NodeRow>();
    private readonly HashSet<Transform> selectedTargets = new HashSet<Transform>();
    private RectTransform content;
    private RectTransform viewport;
    private ScrollRect scrollRect;
    private TextMeshProUGUI emptyMessage;
    private Action<Transform, bool> onNodeSelected;
    private bool initialized;

    public static HierarchyPanelView Create(Action<Transform, bool> selectionCallback)
    {
        var canvasObject = new GameObject(
            "Model Hierarchy Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        HierarchyPanelView view = canvasObject.AddComponent<HierarchyPanelView>();
        view.Initialize(selectionCallback);
        return view;
    }

    public void SetModel(Transform modelRoot)
    {
        ClearRows();
        selectedTargets.Clear();

        NodeData root = modelRoot != null ? BuildNodeData(modelRoot) : null;
        if (root == null)
        {
            emptyMessage.gameObject.SetActive(true);
            emptyMessage.text = "Select a model to show its hierarchy.";
            return;
        }

        emptyMessage.gameObject.SetActive(false);
        AddRows(root, 0, -1);
        UpdateRowVisibility();
    }

    public void SetSelectedTargets(IEnumerable<Transform> targets, Transform activeTarget)
    {
        selectedTargets.Clear();
        if (targets != null)
        {
            foreach (Transform target in targets)
            {
                if (target != null)
                    selectedTargets.Add(target);
            }
        }

        NodeRow activeRow = null;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].Target != activeTarget)
                continue;

            activeRow = rows[i];
            int parent = rows[i].ParentIndex;
            while (parent >= 0)
            {
                rows[parent].Expanded = true;
                parent = rows[parent].ParentIndex;
            }

            break;
        }

        UpdateRowVisibility();
        if (activeRow != null)
        {
            Canvas.ForceUpdateCanvases();
            ScrollToRow(activeRow);
        }
    }

    public void Initialize(Action<Transform, bool> selectionCallback)
    {
        if (initialized)
            return;

        initialized = true;
        onNodeSelected = selectionCallback;

        RectTransform panel = CreateRect("Hierarchy Panel", transform);
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(24f, -24f);
        panel.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        Image panelBackground = panel.gameObject.AddComponent<Image>();
        panelBackground.color = new Color(0f, 0f, 0f, 0.78f);

        VerticalLayoutGroup panelLayout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(14, 14, 14, 14);
        panelLayout.spacing = 8f;
        panelLayout.childAlignment = TextAnchor.UpperLeft;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        TextMeshProUGUI heading = CreateText("Heading", panel, "MODEL HIERARCHY", 17f, FontStyles.Bold);
        heading.color = new Color(0.96f, 0.97f, 1f, 1f);
        LayoutElement headingLayout = heading.gameObject.AddComponent<LayoutElement>();
        headingLayout.preferredHeight = 30f;
        headingLayout.flexibleHeight = 0f;

        RectTransform divider = CreateRect("Divider", panel);
        Image dividerImage = divider.gameObject.AddComponent<Image>();
        dividerImage.color = new Color(1f, 1f, 1f, 0.22f);
        LayoutElement dividerLayout = divider.gameObject.AddComponent<LayoutElement>();
        dividerLayout.preferredHeight = 1f;
        dividerLayout.flexibleHeight = 0f;

        RectTransform scrollObject = CreateRect("Hierarchy Scroll View", panel);
        LayoutElement scrollLayout = scrollObject.gameObject.AddComponent<LayoutElement>();
        scrollLayout.flexibleHeight = 1f;
        scrollLayout.minHeight = 120f;

        scrollRect = scrollObject.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = 28f;

        RectTransform scrollbarRect = CreateRect("Vertical Scrollbar", scrollObject);
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = Vector2.one;
        scrollbarRect.pivot = new Vector2(1f, 0.5f);
        scrollbarRect.anchoredPosition = Vector2.zero;
        scrollbarRect.sizeDelta = new Vector2(12f, 0f);

        UnityEngine.UI.Image scrollbarTrack = scrollbarRect.gameObject.AddComponent<UnityEngine.UI.Image>();
        scrollbarTrack.color = new Color(1f, 1f, 1f, 0.12f);
        scrollbarTrack.raycastTarget = true;

        UnityEngine.UI.Scrollbar verticalScrollbar = scrollbarRect.gameObject.AddComponent<UnityEngine.UI.Scrollbar>();
        verticalScrollbar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;

        RectTransform slidingArea = CreateRect("Sliding Area", scrollbarRect);
        slidingArea.anchorMin = Vector2.zero;
        slidingArea.anchorMax = Vector2.one;
        slidingArea.offsetMin = new Vector2(2f, 2f);
        slidingArea.offsetMax = new Vector2(-2f, -2f);

        RectTransform handle = CreateRect("Handle", slidingArea);
        Stretch(handle);
        UnityEngine.UI.Image handleImage = handle.gameObject.AddComponent<UnityEngine.UI.Image>();
        handleImage.color = new Color(0.86f, 0.88f, 0.92f, 0.8f);
        handleImage.raycastTarget = true;

        verticalScrollbar.handleRect = handle;
        verticalScrollbar.targetGraphic = handleImage;

        viewport = CreateRect("Viewport", scrollObject);
        Stretch(viewport);
        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.025f);
        viewportImage.raycastTarget = true;
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        content = CreateRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 2f;
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.viewport = viewport;
        scrollRect.content = content;
        scrollRect.verticalScrollbar = verticalScrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = 2f;

        emptyMessage = CreateText("Empty Message", content, "Select a model to show its hierarchy.", 13f, FontStyles.Normal);
        emptyMessage.color = new Color(0.74f, 0.77f, 0.82f, 1f);
        emptyMessage.margin = new Vector4(6f, 8f, 6f, 8f);
        LayoutElement emptyLayout = emptyMessage.gameObject.AddComponent<LayoutElement>();
        emptyLayout.preferredHeight = 36f;
    }

    private void ClearRows()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        rows.Clear();
        emptyMessage = CreateText("Empty Message", content, "", 13f, FontStyles.Normal);
        emptyMessage.color = new Color(0.74f, 0.77f, 0.82f, 1f);
        emptyMessage.margin = new Vector4(6f, 8f, 6f, 8f);
        LayoutElement emptyLayout = emptyMessage.gameObject.AddComponent<LayoutElement>();
        emptyLayout.preferredHeight = 36f;
        emptyMessage.gameObject.SetActive(false);
    }

    private int AddRows(NodeData node, int depth, int parentIndex)
    {
        int rowIndex = rows.Count;
        var row = new NodeRow(node.Target, depth, parentIndex, node.Children.Count > 0);
        rows.Add(row);
        CreateRow(row, node.Target.name, node.HasOwnRenderer);

        foreach (NodeData child in node.Children)
            AddRows(child, depth + 1, rowIndex);

        return rowIndex;
    }

    private void CreateRow(NodeRow row, string label, bool hasOwnRenderer)
    {
        RectTransform rowRect = CreateRect("Hierarchy Row", content);
        row.RowObject = rowRect.gameObject;

        LayoutElement rowLayout = rowRect.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 28f;

        HorizontalLayoutGroup rowGroup = rowRect.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowGroup.padding = new RectOffset(6 + row.Depth * 15, 4, 2, 2);
        rowGroup.spacing = 3f;
        rowGroup.childAlignment = TextAnchor.MiddleLeft;
        rowGroup.childControlWidth = true;
        rowGroup.childControlHeight = true;
        rowGroup.childForceExpandWidth = false;
        rowGroup.childForceExpandHeight = true;

        Button foldButton = CreateButton("Expand", rowRect);
        LayoutElement foldLayout = foldButton.gameObject.AddComponent<LayoutElement>();
        foldLayout.preferredWidth = 20f;
        foldLayout.flexibleWidth = 0f;
        row.FoldLabel = CreateText("Expand Label", foldButton.transform, row.HasChildren ? "v" : "", 14f, FontStyles.Normal);
        row.FoldLabel.alignment = TextAlignmentOptions.Center;
        row.FoldLabel.color = new Color(0.68f, 0.72f, 0.78f, 1f);
        Stretch(row.FoldLabel.rectTransform);
        foldButton.interactable = row.HasChildren;
        row.FoldButton = foldButton;
        foldButton.onClick.AddListener(() => ToggleExpanded(row));

        Button labelButton = CreateButton("Select Node", rowRect);
        LayoutElement labelLayout = labelButton.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;
        row.LabelBackground = labelButton.GetComponent<Image>();
        row.Label = CreateText("Node Label", labelButton.transform, label, 13f, hasOwnRenderer ? FontStyles.Bold : FontStyles.Normal);
        row.Label.alignment = TextAlignmentOptions.MidlineLeft;
        row.Label.overflowMode = TextOverflowModes.Ellipsis;
        row.Label.margin = new Vector4(6f, 0f, 4f, 0f);
        row.Label.color = new Color(0.9f, 0.92f, 0.96f, 1f);
        Stretch(row.Label.rectTransform);
        labelButton.onClick.AddListener(() => SelectRow(row));
    }

    private void SelectRow(NodeRow row)
    {
        Keyboard keyboard = Keyboard.current;
        bool additiveSelection = keyboard != null
            && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        onNodeSelected?.Invoke(row.Target, additiveSelection);
    }

    private void ToggleExpanded(NodeRow row)
    {
        row.Expanded = !row.Expanded;
        UpdateRowVisibility();
    }

    private void UpdateRowVisibility()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            NodeRow row = rows[i];
            bool visible = true;
            int parent = row.ParentIndex;
            while (parent >= 0)
            {
                if (!rows[parent].Expanded)
                {
                    visible = false;
                    break;
                }

                parent = rows[parent].ParentIndex;
            }

            row.RowObject.SetActive(visible);
            row.FoldLabel.text = !row.HasChildren ? "" : row.Expanded ? "v" : ">";
        }

        UpdateSelectionColors();
    }

    private void UpdateSelectionColors()
    {
        foreach (NodeRow row in rows)
        {
            row.LabelBackground.color = selectedTargets.Contains(row.Target)
                ? new Color(0.74f, 0.12f, 0.1f, 0.48f)
                : new Color(1f, 1f, 1f, 0f);
        }
    }

    private void ScrollToRow(NodeRow row)
    {
        float scrollableHeight = content.rect.height - viewport.rect.height;
        if (scrollableHeight <= 0f)
            return;

        float distanceFromTop = -row.RowObject.GetComponent<RectTransform>().anchoredPosition.y;
        scrollRect.verticalNormalizedPosition = 1f - Mathf.Clamp01(distanceFromTop / scrollableHeight);
    }

    private static NodeData BuildNodeData(Transform target)
    {
        if (target == null || (target.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0)
            return null;

        var children = new List<NodeData>();
        for (int i = 0; i < target.childCount; i++)
        {
            NodeData child = BuildNodeData(target.GetChild(i));
            if (child != null)
                children.Add(child);
        }

        Renderer renderer = target.GetComponent<Renderer>();
        bool hasOwnRenderer = renderer != null && renderer.enabled;
        if (!hasOwnRenderer && children.Count == 0)
            return null;

        return new NodeData(target, hasOwnRenderer, children);
    }

    private static Button CreateButton(string name, Transform parent)
    {
        RectTransform rect = CreateRect(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.11f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        return button;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string value, float size, FontStyles style)
    {
        RectTransform rect = CreateRect(name, parent);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = Color.white;
        text.raycastTarget = false;
        text.enableWordWrapping = false;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var child = new GameObject(name, typeof(RectTransform));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private sealed class NodeData
    {
        public readonly Transform Target;
        public readonly bool HasOwnRenderer;
        public readonly List<NodeData> Children;

        public NodeData(Transform target, bool hasOwnRenderer, List<NodeData> children)
        {
            Target = target;
            HasOwnRenderer = hasOwnRenderer;
            Children = children;
        }
    }

    private sealed class NodeRow
    {
        public readonly Transform Target;
        public readonly int Depth;
        public readonly int ParentIndex;
        public readonly bool HasChildren;
        public bool Expanded;
        public GameObject RowObject;
        public Button FoldButton;
        public TextMeshProUGUI FoldLabel;
        public TextMeshProUGUI Label;
        public Image LabelBackground;

        public NodeRow(Transform target, int depth, int parentIndex, bool hasChildren)
        {
            Target = target;
            Depth = depth;
            ParentIndex = parentIndex;
            HasChildren = hasChildren;
            Expanded = depth < 2;
        }
    }
}
