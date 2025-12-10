using UnityEngine;
using UnityEngine.Video;

public class LookAtCamera : MonoBehaviour
{
    [Tooltip("Bloquear eje Y para que solo rote horizontalmente (útil para paneles en el suelo)")]
    public bool bloquearEjeY = false;

    private Camera camaraPrincipal;
    /// <summary>
    /// ///
    /// </summary>
    private Vector3 mOffset;
    private float mZCoord;
    // Arrastra aquí el objeto PADRE completo que quieres que se mueva (ej. todo el TV)
    public Transform objetoAMover;
    /// <summary>
    /// 
    /// </summary>


    //VIDEOO
    public VideoPlayer miVideoPlayer;
    public GameObject iconoPlay;  // Opcional: para mostrar si está en pausa
    public GameObject iconoPause; // Opcional


    void Start()
    {
        camaraPrincipal = Camera.main;
    }

    void LateUpdate() // LateUpdate evita tirones
    {
        if (camaraPrincipal != null)
        {
            // Calculamos la rotación para mirar a la cámara
            Quaternion rotacionDeseada = Quaternion.LookRotation(transform.position - camaraPrincipal.transform.position);

            // Corregimos la orientación (a veces LookRotation invierte el objeto dependiendo del modelo)
            // Normalmente UI en WorldSpace necesita mirar "hacia atrás" de la cámara o viceversa.
            // Si te sale al revés, usa: transform.LookAt(transform.position + camaraPrincipal.transform.rotation * Vector3.forward);

            // Versión Billboard simple: Copiar rotación de cámara
            transform.rotation = camaraPrincipal.transform.rotation;

            if (bloquearEjeY)
            {
                // Si queremos que el panel esté siempre "de pie" y no se incline mirando al techo
                Vector3 euler = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(0, euler.y, 0);
            }
        }
    }




    // ARRASTRAR
    void OnMouseDown()
    {
        if (objetoAMover == null) return;
        mZCoord = Camera.main.WorldToScreenPoint(objetoAMover.position).z;
        mOffset = objetoAMover.position - GetMouseAsWorldPoint();
    }

    void OnMouseDrag()
    {
        if (objetoAMover == null) return;
        // Movemos el objeto siguiendo el dedo
        Vector3 nuevaPos = GetMouseAsWorldPoint() + mOffset;
        // Forzamos que se mantenga a la altura del suelo (Y) original para que no vuele
        nuevaPos.y = objetoAMover.position.y;
        objetoAMover.position = nuevaPos;
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }


    //VIDEOOOOO
    public void AlternarReproduccion()
    {
        if (miVideoPlayer == null) return;

        if (miVideoPlayer.isPlaying)
        {
            miVideoPlayer.Pause();
            if (iconoPlay) iconoPlay.SetActive(true);
            if (iconoPause) iconoPause.SetActive(false);
        }
        else
        {
            miVideoPlayer.Play();
            if (iconoPlay) iconoPlay.SetActive(false);
            if (iconoPause) iconoPause.SetActive(true);
        }
    }
}