using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public float DeadZoneRange { get => deadZoneRange; }
    public Vector2 HorizontalClamp
    {
        get => horizontalClamping;
        set => horizontalClamping = value;
    }
                        

    [Header("Follow Parameters")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private float followSpeed = 10;

    [Header("Clamping Parameters")]
    [SerializeField] private float deadZoneRange = 0.05f;
    [SerializeField] private Vector2 horizontalClamping = new Vector2(-1, 1);


    private Camera camera;
    private const float ORTHO_SIZE_ADJUST_VALUE = 0.308125f;

    private void OnValidate()
    {
        //if (horizontalDeadZone.x < 0)
        //    horizontalDeadZone.x = 0;

        //if (horizontalDeadZone.y < 0.1)
        //    horizontalDeadZone.y = 0.1f;

        //if (horizontalDeadZone.y > 1)
        //    horizontalDeadZone.y = 1;

        //if (horizontalDeadZone.x >= horizontalDeadZone.y)
        //    horizontalDeadZone.x = horizontalDeadZone.y - 0.1f;


        //if (verticalDeadZone.x < 0)
        //    verticalDeadZone.x = 0;

        //if (verticalDeadZone.y < 0.1)
        //    verticalDeadZone.y = 0.1f;

        //if (verticalDeadZone.y > 1)
        //    verticalDeadZone.y = 1;

        //if (verticalDeadZone.x >= verticalDeadZone.y)
        //    verticalDeadZone.x = verticalDeadZone.y - 0.1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (followTarget == null)
            return;

        if (IsTargetInDeadZone(followTarget))
            return;
        FollowTarget(followTarget);

    }

    private void FollowTarget(Transform target)
    {
        // If the camera is beyond the clamped zones
        if (IsBeyondClampZone(camera, out int dir))
        {
            // Get the direction where the camera overstepped
            float clampValue = dir == 1 ? horizontalClamping.x : horizontalClamping.y;

            // Set the clamped position as the target position
            Vector3 adjustTargetPosition = new Vector3(clampValue + (dir * (camera.orthographicSize + ORTHO_SIZE_ADJUST_VALUE)),
                                                        target.position.y,
                                                        -10);

            transform.position = Vector3.MoveTowards(transform.position, adjustTargetPosition, Time.deltaTime * followSpeed);
        }
        else
        {
            // Get the moved position
            Vector3 movedPosition = Vector3.MoveTowards(transform.position, target.position + (Vector3.forward * -10), Time.deltaTime * followSpeed);

            // Check if that moved position would result in the camera going beyond the clamped zones
            if (IsBeyondClampZone(movedPosition))
                return;

            // Move the camera
            transform.position = movedPosition;
        }
    }

    // Returns true if the indicated position oversteps the clamped zone.
    private bool IsBeyondClampZone(Vector3 newPosition)
    {
        float orthoSize = camera.orthographicSize;

        float leftPos = newPosition.x - orthoSize - ORTHO_SIZE_ADJUST_VALUE;
        float rightPos = newPosition.x + orthoSize + ORTHO_SIZE_ADJUST_VALUE;

        return leftPos < horizontalClamping.x || rightPos > horizontalClamping.y;
    }

    // Returns if the camera has reached the clamped zone and also returns the direction
    // where the camera overstepped the clamped zones
    private bool IsBeyondClampZone(Camera camera, out int direction)
    {

        float currLeftPosition = camera.transform.position.x - camera.orthographicSize - ORTHO_SIZE_ADJUST_VALUE;
        float currRightPosition = camera.transform.position.x + camera.orthographicSize + ORTHO_SIZE_ADJUST_VALUE;

        if (currLeftPosition < horizontalClamping.x)
            direction = -1;
        else if (currRightPosition > horizontalClamping.y)
            direction = 1;
        else
            direction = 0;

        return currLeftPosition < horizontalClamping.x || currRightPosition > horizontalClamping.y;
    }

    // Returns true if the target is within the designated dead zone
    private bool IsTargetInDeadZone(Transform target)
    {
        Vector2 screenPosition = Camera.main.WorldToViewportPoint(target.position);


        // Adjust the center of the screen position
        screenPosition.x -= 0.5f;
        screenPosition.y -= 0.5f;


        return (Vector2.Distance(Vector2.zero, screenPosition) < deadZoneRange);
    }
}