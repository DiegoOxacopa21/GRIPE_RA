using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    

    [SerializeField] private TextMeshProUGUI debugText;

    public AudioSource audioSource;
    // REFERENCIAS PARA LOS SONIDOS ABRIR Y CERRAR PANELES CANVA
    [Header("SONIDOS PARA ABRIR Y CERRAR PANEL")]
    public AudioClip abrir_panel;
    public AudioClip cerrar_panel;

    // REFERENCIAS PARA ABRIR ESCENA
    [Header("SONIDOS PARA ABRIR Y CERRAR ESCENA")]
    public AudioClip abrir_escena;
    public AudioClip cerrar_escena;

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

    public void AbrirEscenaExplorar()
    {
        if (audioSource != null && abrir_escena != null)
        {
            audioSource.PlayOneShot(abrir_escena);
        }

        // Cargar la escena
        SceneManager.LoadSceneAsync("PRUEBAS");
    }

    // Método llamado por el botón 2
    public void AbrirEscenaNarracion()
    {
        if (audioSource != null && abrir_escena != null)
        {
            audioSource.PlayOneShot(abrir_escena);
        }

        SceneManager.LoadSceneAsync("SampleScene");
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

}
