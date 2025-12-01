using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManipulable : MonoBehaviour
{
    // -------------------------------------------------------------------
    // I. CONFIGURACIÓN (Ajustable en el Inspector)
    // -------------------------------------------------------------------

    [Header("Ajustes de Manipulación")]
    public float sensibilidadRotacion = 1.0f; // Multiplicador de la velocidad de rotación.
    public float velocidadEscalado = 0.01f;  // Multiplicador de la velocidad de escalado.

    // -------------------------------------------------------------------
    // II. ESTADO INTERNO
    // -------------------------------------------------------------------

    // Para la Traslación (Movimiento de 1 dedo)
    private float zDistance;
    private Vector3 positionOffset;

    // Para la Rotación y Escala (Gestos de 2/3 dedos)
    private float previousTwoFingerDistance;

    // Control de estado de selección para evitar conflictos entre objetos.
    private enum ManipulationState { None, Moving, Scaling, Rotating }
    private ManipulationState currentState = ManipulationState.None;

    // -------------------------------------------------------------------
    // III. BUCLE PRINCIPAL
    // -------------------------------------------------------------------

    void Update()
    {
        // El Raycast solo debe detectar objetos con Collider y sin IsTrigger, como ya configuraste.

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
            // Resetea el estado cuando no hay toques.
            currentState = ManipulationState.None;
        }
    }

    // -------------------------------------------------------------------
    // IV. MANEJADORES DE GESTOS
    // -------------------------------------------------------------------

    // A. MOVIMIENTO (1 DEDO)
    private void HandleOneFingerMovement()
    {
        Touch touch = Input.GetTouch(0);
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;

        if (touch.phase == TouchPhase.Began)
        {
            // Solo empezamos a mover si el toque inicia en ESTE objeto.
            if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform == this.transform)
            {
                currentState = ManipulationState.Moving;
                zDistance = hitInfo.distance;
                positionOffset = transform.position - hitInfo.point;
            }
        }
        else if (touch.phase == TouchPhase.Moved && currentState == ManipulationState.Moving)
        {
            // Traslación: Mantiene la distancia Z y el offset.
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, zDistance));
            transform.position = newPosition + positionOffset;
        }
        // El fin del movimiento se maneja en el bloque touchCount == 0
    }

    // B. ESCALADO (2 DEDOS)
    private void HandleTwoFingerScaling()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        // La manipulación de 2 dedos solo debe ocurrir si ambos inician o están en movimiento.
        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            if (CanBeginManipulation(touch0, touch1))
            {
                currentState = ManipulationState.Scaling;
                previousTwoFingerDistance = Vector2.Distance(touch0.position, touch1.position);
            }
            else
            {
                // Si el gesto de dos dedos no es válido sobre este objeto, ignorarlo.
                currentState = ManipulationState.None;
            }
        }
        else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
        {
            if (currentState == ManipulationState.Scaling)
            {
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                // Calculamos el factor de escalado (distancia actual / distancia anterior)
                float scaleFactor = currentDistance / previousTwoFingerDistance;

                // Aplicamos la escala uniformemente.
                transform.localScale *= scaleFactor;

                // Actualizamos para el siguiente frame.
                previousTwoFingerDistance = currentDistance;
            }
        }
    }

    // C. ROTACIÓN (3 DEDOS)
    private void HandleThreeFingerRotation()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        Touch touch2 = Input.GetTouch(2);

        // Usaremos los dos primeros dedos para calcular la rotación.

        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            // Implementamos CanBeginManipulation usando 2 dedos para simplificar la validación.
            if (CanBeginManipulation(touch0, touch1))
            {
                currentState = ManipulationState.Rotating;
            }
        }
        else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            if (currentState == ManipulationState.Rotating)
            {
                // Vector del frame anterior: (Posición anterior del dedo 0 - Posición anterior del dedo 1)
                Vector2 previousDirection = (touch0.position - touch0.deltaPosition) - (touch1.position - touch1.deltaPosition);
                // Vector del frame actual: (Posición actual del dedo 0 - Posición actual del dedo 1)
                Vector2 currentDirection = touch0.position - touch1.position;

                // El SignedAngle nos da el cambio de ángulo en grados.
                float angleDelta = Vector2.SignedAngle(previousDirection, currentDirection);

                // Aplicamos la rotación en el vertical (Eje Y) con la sensibilidad.
                transform.Rotate(0f, -angleDelta * sensibilidadRotacion, 0f, Space.Self);
            }
        }
    }

    // D. UTILIDAD DE RAYCAST (CLEAN CODE)
    private bool CanBeginManipulation(Touch t0, Touch t1)
    {
        RaycastHit hit0, hit1;
        Ray ray0 = Camera.main.ScreenPointToRay(t0.position);
        Ray ray1 = Camera.main.ScreenPointToRay(t1.position);

        // La manipulación solo inicia si AMBOS toques golpean ESTE objeto.
        return Physics.Raycast(ray0, out hit0) && hit0.transform == this.transform &&
               Physics.Raycast(ray1, out hit1) && hit1.transform == this.transform;
    }
}