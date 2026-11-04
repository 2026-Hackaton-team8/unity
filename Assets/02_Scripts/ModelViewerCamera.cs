using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class ModelViewerCamera : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 1f;
    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float minZoom = 0.4f;
    [SerializeField] private float maxZoom = 3f;

    [SerializeField] private CinemachineOrbitalFollow orbit;

    private bool isRotating;
    private bool cursorWasVisible;

    private void Awake()
    {
        if (!orbit)
            orbit = GetComponent<CinemachineOrbitalFollow>();
    }

    private void Update()
    {
        Input();
    }

    private void Input()
    {
        Mouse mouse = Mouse.current;
        if (!Application.isFocused || mouse == null || !orbit)
        {
            RestoreCursor();
            return;
        }

        // 좌클릭 드래그: 카메라 회전
        if (mouse.leftButton.isPressed)
        {
            if (!isRotating)
            {
                isRotating = true;
                cursorWasVisible = Cursor.visible;
                Cursor.visible = false;
            }

            Vector2 delta = mouse.delta.ReadValue();

            orbit.HorizontalAxis.Value += delta.x * rotateSpeed;

            orbit.VerticalAxis.Value = Mathf.Clamp(
                orbit.VerticalAxis.Value - delta.y * rotateSpeed,
                -89f, 89f
            );
        }
        else
        {
            RestoreCursor();
        }

        // 마우스 휠: 확대 및 축소
        float scroll = mouse.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            orbit.RadialAxis.Value = Mathf.Clamp(
                orbit.RadialAxis.Value - scroll * zoomSpeed,
                minZoom, maxZoom
            );
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            RestoreCursor();
    }

    private void OnDisable()
    {
        RestoreCursor();
    }

    private void RestoreCursor()
    {
        if (!isRotating) return;

        Cursor.visible = cursorWasVisible;
        isRotating = false;
    }
}
