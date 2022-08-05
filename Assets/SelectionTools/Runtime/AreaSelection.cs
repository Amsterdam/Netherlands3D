using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AreaSelection : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputActionMap areaSelectionActionMap;
    private MeshRenderer boundsMeshRenderer;

    [Header("Invoke")]
    [SerializeField] private BoolEvent selectingArea;
    [SerializeField] private BoundsEvent selectedAreaBounds;

    [Header("Settings")]
    [SerializeField] private float gridSize = 100;
    [SerializeField] private float maxSelectionDistanceFromCamera = 10000;
    [SerializeField] private bool useWorldSpace = false;

    private InputAction pointerAction;
    private InputAction tapAction;
    private InputAction clickAction;
    private InputAction modifierAction;

    private Vector3 selectionStartPosition;

    private Plane worldPlane;

    [SerializeField] private GameObject gridHighlight;
    [SerializeField] private GameObject selectionBlock;

    private bool drawingArea = false;

    void Awake()
    {
        areaSelectionActionMap = inputActionAsset.FindActionMap("AreaSelection");
        tapAction = areaSelectionActionMap.FindAction("Tap");
        clickAction = areaSelectionActionMap.FindAction("Click");
        pointerAction = areaSelectionActionMap.FindAction("Point");
        modifierAction = areaSelectionActionMap.FindAction("Modifier");

        tapAction.performed += context => Tap();
        clickAction.performed += context => StartClick();
        clickAction.canceled += context => Release();

        worldPlane = (useWorldSpace) ? new Plane(Vector3.up, Vector3.zero) : new Plane(this.transform.up, this.transform.position);

        boundsMeshRenderer = selectionBlock.GetComponent<MeshRenderer>();
    }

    private void OnValidate()
    {
        selectionBlock.transform.localScale = Vector3.one * gridSize;
        gridHighlight.transform.localScale = Vector3.one * gridSize;
    }

    private void OnEnable()
    {
        areaSelectionActionMap.Enable();
    }

    private void OnDisable()
    {
        drawingArea = false;
        selectingArea.started.Invoke(false);
        areaSelectionActionMap.Disable();
    }

    private void Update()
    {
        var currentPointerPosition = pointerAction.ReadValue<Vector2>();
        var currentWorldCoordinate = GetGridPosition(GetCoordinateInWorld(currentPointerPosition));
        gridHighlight.transform.position = currentWorldCoordinate;

        if (!drawingArea && clickAction.IsPressed() && modifierAction.IsPressed())
        {
            drawingArea = true;
            selectingArea.started.Invoke(true);
        }
        else if(drawingArea && !clickAction.IsPressed())
        {
            drawingArea = false;
            selectingArea.started.Invoke(false);
        }

        if(drawingArea)
        {
            DrawSelectionArea(selectionStartPosition, currentWorldCoordinate);
        }
    }
    
    private void Tap()
    {
        var currentPointerPosition = pointerAction.ReadValue<Vector2>();
        var tappedPosition = GetGridPosition(GetCoordinateInWorld(currentPointerPosition));
        DrawSelectionArea(tappedPosition, tappedPosition);
        MakeSelection();
    }

    private void StartClick()
    {
        var currentPointerPosition = pointerAction.ReadValue<Vector2>();
        selectionStartPosition = GetGridPosition(GetCoordinateInWorld(currentPointerPosition));
    }

    private void Release()
    {
        var currentPointerPosition = pointerAction.ReadValue<Vector2>();
        var selectionEndPosition = GetGridPosition(GetCoordinateInWorld(currentPointerPosition));

        if(drawingArea)
        {
            DrawSelectionArea(selectionStartPosition ,selectionEndPosition);
            MakeSelection();
        }
    }

    private void MakeSelection()
    {
        Debug.Log($"Make selection based on the visual selection area");
        var bounds = boundsMeshRenderer.bounds;
        selectedAreaBounds.started.Invoke(bounds);
    }

    /// <summary>
    /// Get a rounded position using the grid size
    /// </summary>
    /// <param name="samplePosition">The position to round to grid position</param>
    /// <returns></returns>
    private Vector3Int GetGridPosition(Vector3 samplePosition)
    {
        samplePosition.x += (gridSize * 0.5f);
        samplePosition.z += (gridSize * 0.5f);

        samplePosition.x = (Mathf.Round(samplePosition.x / gridSize) * gridSize) - (gridSize * 0.5f);
        samplePosition.z = (Mathf.Round(samplePosition.z / gridSize) * gridSize) - (gridSize * 0.5f);

        Vector3Int roundedPosition = new Vector3Int
        {
            x = Mathf.RoundToInt(samplePosition.x),
            y = Mathf.RoundToInt(samplePosition.y),
            z = Mathf.RoundToInt(samplePosition.z)
        };

        return roundedPosition;
    }

    /// <summary>
    /// Draw selection area by scaling the block
    /// </summary>
    /// <param name="currentWorldCoordinate">Current pointer position in world</param>
    private void DrawSelectionArea(Vector3 startWorldCoordinate, Vector3 currentWorldCoordinate)
    {
        selectionBlock.SetActive(true);

        var xDifference = (currentWorldCoordinate.x - startWorldCoordinate.x);
        var zDifference = (currentWorldCoordinate.z - startWorldCoordinate.z);

        selectionBlock.transform.position = startWorldCoordinate;
        selectionBlock.transform.Translate(xDifference / 2.0f, 0, zDifference / 2.0f);
        selectionBlock.transform.localScale = new Vector3(
                (currentWorldCoordinate.x - startWorldCoordinate.x) + ((xDifference < 0) ? -gridSize : gridSize),
                gridSize,
                (currentWorldCoordinate.z - startWorldCoordinate.z) + ((zDifference < 0) ? -gridSize : gridSize));
    }

    /// <summary>
    /// Get the position of a screen point in world coordinates ( on a plane )
    /// </summary>
    /// <param name="screenPoint">The point in screenpoint coordinates</param>
    /// <returns></returns>
    private Vector3 GetCoordinateInWorld(Vector3 screenPoint)
    {
        var screenRay = Camera.main.ScreenPointToRay(screenPoint);

        worldPlane.Raycast(screenRay, out float distance);
        var samplePoint = screenRay.GetPoint(Mathf.Min(maxSelectionDistanceFromCamera, distance));

        return samplePoint;
    }
}
