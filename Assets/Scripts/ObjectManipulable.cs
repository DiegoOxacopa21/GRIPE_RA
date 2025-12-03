using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManipulable : MonoBehaviour
{
    [Header("Ajustes de Manipulación")]
    public float sensibilidadRotacion = 1.0f; // Multiplicador de la velocidad de rotación.
    public float velocidadEscalado = 0.01f;  // Multiplicador de la velocidad de escalado.

    private float zDistance;
    private Vector3 positionOffset;
    private float previousTwoFingerDistance;

    private enum ManipulationState { None, Moving, Scaling, Rotating }
    private ManipulationState currentState = ManipulationState.None;

    void Update()
    {
        int touchCount = Input.touchCount;

        if (touchCount == 1)
        {
            HandleOneFingerMovement();
        }
        else if (touchCount == 2)
        {
            HandleTwoFingerScaling();
        }
        else if (touchCount == 3)
        {
            HandleThreeFingerRotation();
        }
        else if (touchCount == 0)
        {
            currentState = ManipulationState.None;
        }
    }

    private void HandleOneFingerMovement()
    {
        Touch touch = Input.GetTouch(0);
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;

        if (touch.phase == TouchPhase.Began)
        {
            if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform == this.transform)
            {
                currentState = ManipulationState.Moving;
                zDistance = hitInfo.distance;
                positionOffset = transform.position - hitInfo.point;
            }
        }
        else if (touch.phase == TouchPhase.Moved && currentState == ManipulationState.Moving)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, zDistance));
            transform.position = newPosition + positionOffset;
        }
    }

    private void HandleTwoFingerScaling()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            if (CanBeginManipulation(touch0, touch1))
            {
                currentState = ManipulationState.Scaling;
                previousTwoFingerDistance = Vector2.Distance(touch0.position, touch1.position);
            }
            else
            {
                currentState = ManipulationState.None;
            }
        }
        else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
        {
            if (currentState == ManipulationState.Scaling)
            {
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                float scaleFactor = currentDistance / previousTwoFingerDistance;
                transform.localScale *= scaleFactor;
                previousTwoFingerDistance = currentDistance;
            }
        }
    }

    private void HandleThreeFingerRotation()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        Touch touch2 = Input.GetTouch(2);

        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            if (CanBeginManipulation(touch0, touch1))
            {
                currentState = ManipulationState.Rotating;
            }
        }
        else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            if (currentState == ManipulationState.Rotating)
            {
                Vector2 previousDirection = (touch0.position - touch0.deltaPosition) - (touch1.position - touch1.deltaPosition);
                Vector2 currentDirection = touch0.position - touch1.position;
                float angleDelta = Vector2.SignedAngle(previousDirection, currentDirection);
                transform.Rotate(0f, -angleDelta * sensibilidadRotacion, 0f, Space.Self);
            }
        }
    }

    private bool CanBeginManipulation(Touch t0, Touch t1)
    {
        RaycastHit hit0, hit1;
        Ray ray0 = Camera.main.ScreenPointToRay(t0.position);
        Ray ray1 = Camera.main.ScreenPointToRay(t1.position);

        return Physics.Raycast(ray0, out hit0) && hit0.transform == this.transform &&
               Physics.Raycast(ray1, out hit1) && hit1.transform == this.transform;
    }
}
