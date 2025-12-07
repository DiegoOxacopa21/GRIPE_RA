using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para usar Coroutines

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;


    [SerializeField] private TextMeshProUGUI debugText;


    //TRANSICION ENTRE ETAPAS
    [Header("UI TRANSICIÓN ENTRE ETAPAS")]
    public GameObject panelTransicion; // Arrastra aquí tu Canvas/Panel de "Siguiente Etapa"

    public AudioSource audioSource;
    // REFERENCIAS PARA LOS SONIDOS ABRIR Y CERRAR PANELES CANVA
    [Header("SONIDOS PARA ABRIR Y CERRAR PANEL")]
    public AudioClip abrir_panel;
    public AudioClip cerrar_panel;

    // REFERENCIAS PARA ABRIR ESCENA
    [Header("SONIDOS PARA ABRIR Y CERRAR ESCENA")]
    public AudioClip abrir_escena;
    public AudioClip cerrar_escena;

    // REFERENCIAS PARA INDICATOR TARGET PULSANTE

    [Header("REFERENCIAS INDICATOR TARGET (no pongas nada)")]
    private readonly Vector3 escalaNormal = Vector3.one;
    private readonly Vector3 escalaGrande = new Vector3(1.1f, 1.1f, 1.1f);



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Log(string message)
    {
        if (debugText != null)
        {
            debugText.text += "\n" + message;
        }
    }


    // BOTONES PARA ABRIR ESCENAS

    public void AbrirEscenaAprendeExplorando()
    {
        if (audioSource != null && abrir_escena != null)
        {
            audioSource.PlayOneShot(abrir_escena);
        }

        // Cargar la escena
        SceneManager.LoadSceneAsync("AprendeExplorando");
    }

    // Método llamado por el botón 2
    public void AbrirEscenaConoceTuCuerpo()
    {
        if (audioSource != null && abrir_escena != null)
        {
            audioSource.PlayOneShot(abrir_escena);
        }

        SceneManager.LoadSceneAsync("PRUEBAS");
    }

    public void AbrirEscenaInicio()
    {
        if (audioSource != null && abrir_escena != null)
        {
            audioSource.PlayOneShot(cerrar_escena);
        }
        SceneManager.LoadSceneAsync("MenuInicio");
    }

    // ACTIVAR/DESACTIVAR PANELES CANVA

    // Función que alterna el estado activo de un GameObject y reproduce sonidos
    public void ActivarDesactivarPanelCanva(GameObject canvasObject)
    {
        if (canvasObject == null || audioSource == null) return;

        bool isActive = canvasObject.activeSelf;
        canvasObject.SetActive(!isActive);

        if (!isActive && abrir_panel != null)
            audioSource.PlayOneShot(abrir_panel);
        else if (isActive && cerrar_panel != null)
            audioSource.PlayOneShot(cerrar_panel);
    }

    //ABRIR PANELES INDICACION TARGET, PULSANTE, Y CERRAR

    public void ActivarPanelPulsanteYDesactivar(GameObject canvasObject)
    {
        if (canvasObject == null) return;

        // Evita reiniciar el efecto si ya está activo
        if (canvasObject.activeSelf) return;

        // 1. Activación
        canvasObject.SetActive(true);

        // 2. Inicio del Efecto Temporizado (Corrutina)
        StartCoroutine(PulsarYDesactivarRutina(canvasObject));
    }

    private IEnumerator PulsarYDesactivarRutina(GameObject targetObject)
    {
        // Se requiere el RectTransform para escalar un objeto UI
        RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("El GameObject no tiene RectTransform y no puede escalarse. Abortando.");
            targetObject.SetActive(false);
            yield break;
        }

        // 3f ES 3 SEGUNDOS !!!!!!!!!

        float tiempoInicio = Time.time;
        float tiempoFin = tiempoInicio + 4f;

        // Bucle de animación que dura 3 segundos
        while (Time.time < tiempoFin)
        {
            // Calcula un valor 't' que oscila entre 0 y 1 de forma suave (efecto pulsante)
            float t = (Mathf.Sin(Time.time * 8f) + 1f) / 2f;

            // Interpola la escala entre la normal y la grande
            rectTransform.localScale = Vector3.Lerp(escalaNormal, escalaGrande, t);

            yield return null; // Espera al siguiente frame
        }

        // 3. Desactivación Final
        // Asegura que la escala quede normal antes de desactivar
        rectTransform.localScale = escalaNormal;
        targetObject.SetActive(false);
    }



}
