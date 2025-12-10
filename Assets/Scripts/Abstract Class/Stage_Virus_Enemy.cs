using UnityEngine;

public class Stage_Virus_Enemy : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public Renderer rendererH;
    public Renderer rendererN;

    [Header("Configuración")]
    public float velocidadAtaque = 5f;

   
    // Variables para controlar la intensidad de animación
    private float velocidadRotacion = 30f; // Grados por segundo (Lento y constante)
    private float frecuenciaLatido = 3f;   // Veces por segundo (Más lento, tipo respiración)
    private float fuerzaLatido = 0.2f;     // 20% del tamaño original (Proporcional)

    public AudioClip audioMuerte;

    // Estados internos
    private bool estaAtacando = false;
    private Vector3 escalaOriginal;
    private int tipoAnimacion; // 0=Nada, 1=Rotar, 2=Latir, 3=Vibrar

    // Referencia al GameManager para reproducir sonidos sin AudioSource propio
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        escalaOriginal = transform.localScale;

        // 1. Decidir animación aleatoria al nacer
        // Tipos: 0=Quieto, 1=Rotar, 2=Latir(Escala), 3=Ambos
        tipoAnimacion = Random.Range(0, 4);

        // 2. Orientarse hacia el jugador
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }

    // Método llamado por Stage_Virus al crear este clon
    public void ConfigurarColores(Color colorH, Color colorN)
    {
        if (rendererH != null) rendererH.material.color = colorH;
        if (rendererN != null) rendererN.material.color = colorN;
    }

    void Update()
    {
        if (estaAtacando)
        {
            ComportamientoAtaque();
        }
        else
        {
            ComportamientoIdle();
        }
    }

    void ComportamientoIdle()
    {
        // 1. ROTAR (Suave)
        if (tipoAnimacion == 1 || tipoAnimacion == 3)
        {
            // Rotamos suavemente en varios ejes para que se vea orgánico
            transform.Rotate(new Vector3(10, 20, 5) * Time.deltaTime * 0.5f);
        }

        // 2. LATIR / VIBRAR (Proporcional al tamaño de 10cm)
        if (tipoAnimacion == 2 || tipoAnimacion == 3)
        {
            // Usamos Seno para un movimiento de "respiración" suave (-1 a 1)
            float factor = Mathf.Sin(Time.time * frecuenciaLatido) * fuerzaLatido;

            // Fórmula Clave: Tamaño = Original * (1 + porcentaje)
            // Esto asegura que si mide 10cm, crezca a 12cm, no a 1 metro.
            transform.localScale = escalaOriginal * (1f + factor);
        }
    }

    void ComportamientoAtaque()
    {
        if (Camera.main != null)
        {
            Vector3 direccion = Camera.main.transform.position - transform.position;

            // Movemos al enemigo
            transform.position += direccion.normalized * velocidadAtaque * Time.deltaTime;

            // Distancia de impacto ajustada a tu escala (10cm = 0.1f)
            // Si el virus está a 15cm de la cámara, cuenta como impacto.
            if (Vector3.Distance(transform.position, Camera.main.transform.position) < 0.15f)
            {
                Destroy(gameObject);
            }
        }
    }

    // Detectar clic del usuario
    private void OnMouseDown()
    {
        if (estaAtacando) return; // Opcional: ¿Se pueden matar mientras atacan?

        // Reproducir sonido de muerte a través del GameManager
        if (gameManager != null && audioMuerte != null)
            gameManager.ReproducirSFX(audioMuerte);

        // Efecto de partículas aquí si quisieras

        Destroy(gameObject);
    }

    public void IniciarAtaqueKamikaze()
    {
        estaAtacando = true;
    }
}