using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float targetPositionY;
    private Vector3 targetRotation;

    private bool onFocus = true;

    [Header("Pan movement")]
    public float panEdgeProp = 0.1f;
    public float panSpeed = 10f;
    public float panSpeedMult = 2f;
    [Header("Zoom values")]
    public float currentZoom = 0f;
    public Vector2 zoomHeightRange = new Vector2(-10f, 10f);
    public Vector2 zoomRotationRange = new Vector2(33f, 70f);
    public float zoomMouseWheelMult = 1000f;
    public float updateZoomMouseWheelMult = 5f;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        targetPositionY = transform.position.y;
        targetRotation  = transform.eulerAngles;
    }

    void LateUpdate()
    {
        float currentPanSpeed = panSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentPanSpeed *= panSpeedMult;
        }

        // pan movement
        if (!Application.isEditor && onFocus)
        {
            // move the camera up
            if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height * (1 - panEdgeProp))
            {
                transform.Translate(Vector3.forward * currentPanSpeed * Time.deltaTime, Space.World);
            }
            // move the camera down
            if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= Screen.height * panEdgeProp)
            {
                transform.Translate(Vector3.forward * -currentPanSpeed * Time.deltaTime, Space.World);
            }
            // move the camera left
            if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= Screen.width * panEdgeProp)
            {
                transform.Translate(Vector3.right * -currentPanSpeed * Time.deltaTime, Space.World);
            }
            // move the camera right
            if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width * (1 - panEdgeProp))
            {
                transform.Translate(Vector3.right * currentPanSpeed * Time.deltaTime, Space.World);
            }
        }
        else // camera control in the Editor
        {
            // move the camera up
            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.Translate(Vector3.forward * currentPanSpeed * Time.deltaTime, Space.World);
            }
            // move the camera down
            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.Translate(Vector3.forward * -currentPanSpeed * Time.deltaTime, Space.World);
            }
            // move the camera left
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Translate(Vector3.right * -currentPanSpeed * Time.deltaTime, Space.World);
            }
            // move the camera right
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Translate(Vector3.right * currentPanSpeed * Time.deltaTime, Space.World);
            }
        }

        // zoom
        // mouse wheel movement
        float zoomToApply = Input.GetAxis("Mouse ScrollWheel") * zoomMouseWheelMult * Time.deltaTime;
        currentZoom -= zoomToApply;
        currentZoom = Mathf.Clamp(currentZoom, zoomHeightRange.x, zoomHeightRange.y);

        // change in camera height
        targetPositionY = transform.position.y - (transform.position.y - (initialPosition.y + currentZoom));

        transform.position = new Vector3(
            transform.position.x,
            Mathf.Lerp(transform.position.y, targetPositionY, updateZoomMouseWheelMult * Time.deltaTime),
            transform.position.z
        );

        // X rotation update
        float zoomWidth = zoomHeightRange.y - zoomHeightRange.x;
        float zoomProp = (currentZoom - zoomHeightRange.x) / zoomWidth;

        float zoomXRotation = Mathf.Lerp(zoomRotationRange.x, zoomRotationRange.y, zoomProp);

        targetRotation = new Vector3(
            zoomXRotation,
            transform.eulerAngles.y,
            transform.eulerAngles.z
        );

        transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, targetRotation, updateZoomMouseWheelMult * Time.deltaTime);
    }

    private void OnApplicationFocus(bool focus)
    {
        onFocus = focus;
    }

}
