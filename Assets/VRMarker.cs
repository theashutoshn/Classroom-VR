using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class VRMarker : MonoBehaviour
{
    public GameObject whiteboard;
    public GameObject brushPrefab;
    public ActionBasedController markerController;
    public float brushSize = 0.02f;
    public InputActionProperty triggerAction;

    private bool isDrawing = false;
    private LineRenderer currentLine;
    private Vector3 lastPos;
    private GameObject currentBrushInstance;

    private void OnEnable()
    {
        triggerAction.action.started += StartDrawing;
        triggerAction.action.canceled += EndDrawing;
    }

    private void OnDisable()
    {
        triggerAction.action.started -= StartDrawing;
        triggerAction.action.canceled -= EndDrawing;
    }

    private void Update()
    {
        if (isDrawing)
        {
            ContinueDrawing();
        }
    }

    private void StartDrawing(InputAction.CallbackContext context)
    {
        if (!isDrawing && currentBrushInstance == null)
        {
            currentBrushInstance = Instantiate(brushPrefab);
            currentLine = currentBrushInstance.GetComponent<LineRenderer>();
            if (currentLine == null)
            {
                Debug.LogError("LineRenderer component not found on brush prefab!");
                Destroy(currentBrushInstance);
                return;
            }
            currentLine.startWidth = brushSize;
            currentLine.endWidth = brushSize;
            
            Vector3 markerPos = markerController.transform.position;
            currentLine.positionCount = 1;
            currentLine.SetPosition(0, markerPos);
            lastPos = markerPos;
            isDrawing = true;
        }
    }

    private void ContinueDrawing()
    {
        if (currentLine != null)
        {
            Vector3 markerPos = markerController.transform.position;
            if (Vector3.Distance(lastPos, markerPos) > 0.01f)
            {
                currentLine.positionCount++;
                currentLine.SetPosition(currentLine.positionCount - 1, markerPos);
                lastPos = markerPos;
            }
        }
    }

    private void EndDrawing(InputAction.CallbackContext context)
    {
        currentLine = null;
        currentBrushInstance = null;
        isDrawing = false;
    }
}