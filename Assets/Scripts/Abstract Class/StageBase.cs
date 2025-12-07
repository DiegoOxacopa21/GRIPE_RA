using UnityEngine;

public abstract class StageBase : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [Tooltip("Sonido narrativo o SFX al iniciar esta etapa.")]
    public AudioClip audioAlIniciar;

    [Tooltip("Sonido al completar la etapa (antes de cambiar).")]
    public AudioClip audioAlTerminar;

    [Tooltip("Sonido para eventos específicos (ej: al detectar una imagen).")]
    public AudioClip audioInteracciones;

    [Tooltip("Audio extra con información o instrucciones de la etapa.")]
    public AudioClip audioInformacionEtapa; // <--- NUEVA VARIABLE

    [Tooltip("Música de fondo que sonará en bucle durante esta etapa.")]
    public AudioClip musicaDeFondo;

    protected GameManager gameManager;

    // Se llama automáticamente al arrancar la etapa
    public virtual void InitStage()
    {
        gameManager = GameManager.Instance;

        // Logs usando TU UIManager
        UIManager.Instance?.Log($"[Stage] Iniciando: {gameObject.name}");

        // 1. Música de Fondo
        if (musicaDeFondo != null)
            gameManager.ReproducirMusicaFondo(musicaDeFondo);

        // 2. Audio de Inicio
        if (audioAlIniciar != null)
            gameManager.ReproducirSFX(audioAlIniciar);

        // (Opcional) Si quisieras que la info suene automáticamente tras el inicio,
        // podrías invocarlo aquí o dejar que el usuario lo active con un botón.
    }

    public virtual void UpdateStage()
    {
        // Lógica loop
    }

    // Se llama antes de destruir/apagar la etapa
    public virtual void EndStage()
    {
        UIManager.Instance?.Log($"[Stage] Finalizando: {gameObject.name}");

        // Audio de despedida de la etapa
        if (audioAlTerminar != null)
            gameManager.ReproducirSFX(audioAlTerminar);
    }

    // --- MÉTODOS DE AYUDA (HELPERS) PARA LAS CLASES HIJAS ---

    // 1. Finalizar Etapa
    protected void FinishStage()
    {
        gameManager.AvanzarEtapa();
    }

    // 2. Audio Interacción
    protected void ReproducirSonidoInteraccion()
    {
        if (audioInteracciones != null)
        {
            gameManager.ReproducirSFX(audioInteracciones);
            UIManager.Instance?.Log("Audio interacción reproducido.");
        }
    }

    // 3. Audio Información (NUEVO)
    protected void ReproducirSonidoInformacion()
    {
        if (audioInformacionEtapa != null)
        {
            gameManager.ReproducirSFX(audioInformacionEtapa);
            UIManager.Instance?.Log("Audio información reproducido.");
        }
    }

    // --- MÉTODOS PUENTE PARA LA UI (NUEVOS) ---

    // Llama a esto desde tu etapa hija así: AlternarPanel(miPanelInfo);
    protected void AlternarPanel(GameObject canvasObject)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ActivarDesactivarPanelCanva(canvasObject);
        }
    }

    // Llama a esto desde tu etapa hija así: MostrarPanelPulsante(miPanelGuia);
    protected void MostrarPanelPulsante(GameObject canvasObject)
    {
        if (UIManager.Instance != null)
        {
            // NOTA: Asegúrate de haber copiado el método ActivarPanelPulsanteYDesactivar 
            // en tu script UIManager.cs para que esto funcione.
            UIManager.Instance.ActivarPanelPulsanteYDesactivar(canvasObject);
        }
    }
}