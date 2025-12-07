using UnityEngine;
using Vuforia;
using System.Collections;

public class Stage_Simulacion : StageBase
{
    [Header("Referencias Vuforia")]
    public PlaneFinderBehaviour planeFinder;
    public ContentPositioningBehaviour contentPositioner;

    [Header("Referencias Simulación")]
    public GameObject contenedorMaqueta;
    public NPCSpawner npcSpawner; // Referencia directa para llamar a sus métodos nuevos

    [Header("UI Control")]
    [Tooltip("Panel que pulsa 'Escaneando'.")]
    public GameObject panelEscaneo;
    [Tooltip("Panel con los botones 'Agregar NPC' e 'Iniciar'.")]
    public GameObject panelControlSimulacion;

    [Header("Audios Específicos")]
    public AudioClip audioEsperaActivador; // Suena tras la intro, mientras busca suelo
    public AudioClip audioInformacionEtapa2; // Suena al iniciar el contagio (final)

    public Stage_Simulacion(AudioClip audioInformacionEtapa)
    {
        this.audioInformacionEtapa2 = audioInformacionEtapa;
    }

    private bool ubicacionEstablecida = false;

    // ---------------------------------------------------------
    // 1. INICIO
    // ---------------------------------------------------------
    public override void InitStage()
    {
        base.InitStage(); // Reproduce audioAlIniciar

        contenedorMaqueta.SetActive(false);
        if (panelControlSimulacion != null) panelControlSimulacion.SetActive(false);

        // Iniciamos la rutina para esperar a que termine el audio de intro antes de pedir escanear
        StartCoroutine(RutinaInicio());
    }

    private IEnumerator RutinaInicio()
    {
        // Esperamos lo que dure el audio de inicio (si existe)
        float espera = (audioAlIniciar != null) ? audioAlIniciar.length : 0f;
        yield return new WaitForSeconds(espera);

        // 1. Activar Vuforia
        if (planeFinder != null) planeFinder.gameObject.SetActive(true);

        // 2. Mostrar Panel Pulsante (Instrucción de UIManager)
        UIManager.Instance?.ActivarPanelPulsanteYDesactivar(panelEscaneo);

        // 3. Audio de espera (loop o ambiente mientras busca)
        if (audioEsperaActivador != null)
            gameManager.ReproducirSFX(audioEsperaActivador);

        // 4. Suscribir evento de toque
        if (contentPositioner != null)
            contentPositioner.OnContentPlaced.AddListener(AlColocarContenido);
    }

    // ---------------------------------------------------------
    // 2. POSICIONAMIENTO
    // ---------------------------------------------------------
    private void AlColocarContenido(GameObject anchorResult)
    {
        if (ubicacionEstablecida) return;
        ubicacionEstablecida = true;

        // A. Colocar Maqueta
        contenedorMaqueta.transform.position = anchorResult.transform.position;
        contenedorMaqueta.transform.rotation = anchorResult.transform.rotation;
        contenedorMaqueta.SetActive(true);

        // --- CORRECCIÓN AQUÍ ---
        // Forzamos la activación del script para que se ejecute su Start() 
        // y encuentre el objeto "Suelo".
        if (npcSpawner != null)
        {
            npcSpawner.enabled = true;
        }
        // -----------------------

        // B. Apagar Vuforia y Panel Escaneo
        if (planeFinder != null) planeFinder.gameObject.SetActive(false);
        if (panelEscaneo != null) panelEscaneo.SetActive(false);

        // C. Feedback
        ReproducirSonidoInteraccion();

        // D. Abrir panel de control
        UIManager.Instance?.ActivarDesactivarPanelCanva(panelControlSimulacion);
    }

    // ---------------------------------------------------------
    // 3. MÉTODOS PARA TUS BOTONES (Asignar en Inspector al OnClick)
    // ---------------------------------------------------------

    // Botón 1: "Agregar NPC"
    public void UI_Boton_AgregarNPC()
    {
        if (npcSpawner != null)
        {
            npcSpawner.SpawnSingleNPC(); // Método nuevo en NPCSpawner
        }
    }

    // Botón 2: "Iniciar Contagio"
    public void UI_Boton_IniciarSimulacion()
    {
        // 1. Cerrar el panel de control
        UIManager.Instance?.ActivarDesactivarPanelCanva(panelControlSimulacion);

        // 2. Activar movimiento y contagio en los NPCs
        if (npcSpawner != null)
        {
            npcSpawner.IniciarSimulacionMasiva(); // Método nuevo
        }

        // 3. Reproducir audio final y terminar etapa
        StartCoroutine(RutinaFinEtapa());
    }

    private IEnumerator RutinaFinEtapa()
    {
        // Reproducir audio informativo
        if (audioInformacionEtapa != null)
        {
            gameManager.ReproducirSFX(audioInformacionEtapa);
            yield return new WaitForSeconds(audioInformacionEtapa.length);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Terminar la etapa (pasa a la siguiente o muestra fin)
        FinishStage();
    }

    public override void EndStage()
    {
        if (contentPositioner != null) contentPositioner.OnContentPlaced.RemoveListener(AlColocarContenido);
        base.EndStage();
    }
}