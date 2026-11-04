using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class ModelViewerCamera : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 1f;
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float minZoom = 0.1f;
    [SerializeField] private float maxZoom = 3f;

    [SerializeField] private CinemachineOrbitalFollow orbit;
    [SerializeField] private CinemachineRotationComposer aim;

    private bool isRotating;
    private bool cursorWasVisible;
    private Vector3 initialOrbitOffset;
    private Vector3 initialAimOffset;
    private Vector3 panOffsetWorld;
    private TrackerSettings initialTrackerSettings;
    private Vector2 initialAimDamping;
    private bool isPanning;

    private void Awake()
    {
        if (!orbit)
            orbit = GetComponent<CinemachineOrbitalFollow>();

        if (!aim)
            aim = GetComponent<CinemachineRotationComposer>();

        if (orbit)
        {
            initialOrbitOffset = orbit.TargetOffset;
            initialTrackerSettings = orbit.TrackerSettings;
        }

        if (aim)
        {
            initialAimOffset = aim.TargetOffset;
            initialAimDamping = aim.Damping;
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        Mouse mouse = Mouse.current;
        if (!Application.isFocused || mouse == null || !orbit)
        {
            RestoreCursor();
            RestorePanDamping();
            return;
        }

        Vector2 delta = mouse.delta.ReadValue();

        // 우클릭 드래그: 카메라 회전
        if (mouse.rightButton.isPressed)
        {
            RestorePanDamping();
            HideCursorForRotation();
            orbit.HorizontalAxis.Value += delta.x * rotateSpeed;
            orbit.VerticalAxis.Value = Mathf.Clamp(
                orbit.VerticalAxis.Value - delta.y * rotateSpeed,
                -89f,
                89f);
        }
        // 휠 버튼(가운데 버튼) 드래그: 화면 평면으로 패닝
        else if (mouse.middleButton.isPressed)
        {
            RestoreCursor();
            DisablePanDamping();
            Pan(delta);
        }
        else
        {
            RestoreCursor();
            RestorePanDamping();
        }

        // 스크롤 휠: 확대 및 축소
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            orbit.RadialAxis.Value = Mathf.Clamp(
                orbit.RadialAxis.Value - scroll * zoomSpeed,
                minZoom,
                maxZoom);
        }
    }

    private void Pan(Vector2 pointerDelta)
    {
        if (aim == null || pointerDelta.sqrMagnitude <= 0f)
            return;

        Camera outputCamera = Camera.main;
        if (outputCamera == null)
            return;

        float pixelHeight = Mathf.Max(1f, outputCamera.pixelHeight);
        float worldUnitsPerPixel = outputCamera.orthographic
            ? (2f * outputCamera.orthographicSize / pixelHeight)
            : (2f * orbit.Radius * orbit.RadialAxis.Value
                * Mathf.Tan(outputCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) / pixelHeight);

        Vector3 worldOffset = (-outputCamera.transform.right * pointerDelta.x
            - outputCamera.transform.up * pointerDelta.y)
            * (worldUnitsPerPixel * panSpeed);

        panOffsetWorld += worldOffset;

        Transform trackingTarget = orbit.FollowTarget;
        if (trackingTarget != null)
            orbit.TargetOffset = initialOrbitOffset + trackingTarget.InverseTransformVector(panOffsetWorld);

        Transform lookAtTarget = aim.LookAtTarget;
        if (lookAtTarget != null)
            aim.TargetOffset = initialAimOffset + lookAtTarget.InverseTransformVector(panOffsetWorld);
    }

    public void ResetPanForFocus()
    {
        RestorePanDamping();
        panOffsetWorld = Vector3.zero;

        if (orbit != null)
            orbit.TargetOffset = initialOrbitOffset;

        if (aim != null)
            aim.TargetOffset = initialAimOffset;
    }

    private void DisablePanDamping()
    {
        if (isPanning)
            return;

        isPanning = true;
        TrackerSettings settings = orbit.TrackerSettings;
        settings.PositionDamping = Vector3.zero;
        settings.RotationDamping = Vector3.zero;
        settings.QuaternionDamping = 0f;
        orbit.TrackerSettings = settings;

        if (aim != null)
            aim.Damping = Vector2.zero;
    }

    private void RestorePanDamping()
    {
        if (!isPanning)
            return;

        if (orbit)
            orbit.TrackerSettings = initialTrackerSettings;

        if (aim)
            aim.Damping = initialAimDamping;

        isPanning = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            RestoreCursor();
            RestorePanDamping();
        }
    }

    private void OnDisable()
    {
        RestoreCursor();
        RestorePanDamping();

        if (orbit)
            orbit.TargetOffset = initialOrbitOffset;

        if (aim)
            aim.TargetOffset = initialAimOffset;

        panOffsetWorld = Vector3.zero;
    }

    private void HideCursorForRotation()
    {
        if (isRotating)
            return;

        isRotating = true;
        cursorWasVisible = Cursor.visible;
        Cursor.visible = false;
    }

    private void RestoreCursor()
    {
        if (!isRotating)
            return;

        Cursor.visible = cursorWasVisible;
        isRotating = false;
    }
}
