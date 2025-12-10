using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Video; // Necesario para Video

public class Stage_Virus : StageBase
{
    [Header("--- REFERENCIAS VUFORIA (EXISTENTES) ---")]
    [Tooltip("El Plane Finder que ya usas en la Fase A.")]
    public GameObject vuforiaPlaneFinder;
    [Tooltip("El Anchor Stage (padre del virus) que ya usas.")]
    public GameObject midAirStageAnchor;
    public GameObject panelInstruccionMidAir;

    [Header("--- REFERENCIAS UI FASE A (Anatomía) ---")]
    public GameObject panelSliderAnatomia;
    public Slider sliderAnatomia;
    public TextMeshProUGUI textoNombreParte;
    public GameObject botonAnalizarTipos;

    [Header("--- REFERENCIAS UI FASE B (Tipos) ---")]
    public GameObject panelFaseTipos;
    public TextMeshProUGUI textoCombinacionVirus;

    [Header("--- MODELO 3D Y RENDERERS ---")]
    public Transform modeloParaRotar;
    public float velocidadRotacion = 0.4f;
    public GameObject[] partesDelVirus;
    public GameObject[] etiquetasFlotantes;
    public Renderer rendererPicosH;
    public Renderer rendererPicosN;

    [Header("--- CONFIGURACIÓN COLORES FASE B ---")]
    public Color[] coloresH;
    public Color[] coloresN;

    [Header("--- AUDIOS ---")]
    public AudioClip audioLoopEscaneo;
    public AudioClip audioAlPosicionar;
    public AudioClip audioCambioTipo;
    public AudioClip audioIntroFaseB;
    public AudioClip audioH1N1Encontrado;

    [Header("--- AUDIOS FASE A ---")]
    public AudioClip[] audiosExplicativos;
    public AudioClip audioFaseCompletada;

    [Header("--- FASE C: INFECCIÓN ---")]
    public GameObject prefabVirusEnemigo;
    public GameObject panelAlerta;
    public float radioSpawn = 1.5f;
    public int cantidadTotalParaColapso = 20;

    [Header("--- AUDIOS FASE C ---")]
    public AudioClip audioAlertaMultiplicacion;
    public AudioClip audioFalloSistema;
    public AudioClip audioImpactoPantalla;

    [Header("--- FASE D: FINAL EXPLICATIVO (NUEVO) ---")]
    [Tooltip("Prefab del TV/Pantalla con VideoPlayer y scripts Billboard/Arrastrar.")]
    public GameObject prefabVideoFinal;
    [Tooltip("Prefab del Panel 3D de info con scripts Billboard/Arrastrar.")]
    public GameObject prefabPanelInfoFinal;

    // Variables internas
    private int pasoAnterior = -1;
    private bool posicionamientoConfirmado = false;
    private int indiceH = 1;
    private int indiceN = 1;
    private bool faseBCompletada = false;

    // Variables Control Fase D
    private bool enFaseFinal = false;
    private int contadorSpawnsFaseD = 0;

    // ---------------------------------------------------------
    // INICIO
    // ---------------------------------------------------------
    public override void InitStage()
    {
        base.InitStage();

        // Limpiezas
        if (panelFaseTipos != null) panelFaseTipos.SetActive(false);
        if (panelAlerta != null) panelAlerta.SetActive(false);
        enFaseFinal = false;
        contadorSpawnsFaseD = 0;

        // Setup Fase A
        if (panelSliderAnatomia != null) panelSliderAnatomia.SetActive(false);
        if (vuforiaPlaneFinder != null) vuforiaPlaneFinder.SetActive(true);
        if (botonAnalizarTipos != null) botonAnalizarTipos.SetActive(false);

        ActualizarVisualesFaseA(0);
        StartCoroutine(RutinaEsperarAudioIntro());
    }

    public override void UpdateStage()
    {
        base.UpdateStage();
        if (posicionamientoConfirmado && !enFaseFinal) ManejarRotacion();
    }

    // ---------------------------------------------------------
    // LÓGICA FASE A (Anatomía)
    // ---------------------------------------------------------
    private IEnumerator RutinaEsperarAudioIntro()
    {
        if (audioAlIniciar != null) yield return new WaitForSeconds(audioAlIniciar.length - 0.5f);
        if (!posicionamientoConfirmado)
        {
            if (panelInstruccionMidAir != null) MostrarPanelPulsante(panelInstruccionMidAir);
            if (audioLoopEscaneo != null && gameManager.sfxSource != null)
            {
                gameManager.sfxSource.clip = audioLoopEscaneo;
                gameManager.sfxSource.loop = true;
                gameManager.sfxSource.Play();
            }
        }
    }

    // Esta función se llama en Fase A al confirmar posición
    public void ConfirmarPosicionamiento()
    {
        if (enFaseFinal) return; // Si estamos en el final, ignoramos este botón

        posicionamientoConfirmado = true;
        if (vuforiaPlaneFinder != null) vuforiaPlaneFinder.SetActive(false);
        if (panelInstruccionMidAir != null) panelInstruccionMidAir.SetActive(false);
        if (gameManager.sfxSource != null) { gameManager.sfxSource.Stop(); gameManager.sfxSource.loop = false; }
        if (audioAlPosicionar != null) gameManager.ReproducirSFX(audioAlPosicionar);

        AlternarPanel(panelSliderAnatomia);
        if (sliderAnatomia != null)
        {
            sliderAnatomia.wholeNumbers = true;
            sliderAnatomia.minValue = 0; sliderAnatomia.maxValue = 5; sliderAnatomia.value = 0;
            sliderAnatomia.onValueChanged.AddListener(OnSliderCambio);
        }
    }

    public void OnSliderCambio(float valor)
    {
        int pasoActual = Mathf.RoundToInt(valor);
        if (pasoActual == pasoAnterior) return;

        ReproducirSonidoInteraccion();
        ActualizarVisualesFaseA(pasoActual);

        if (gameManager.sfxSource != null) gameManager.sfxSource.Stop();
        if (pasoActual > 0 && audiosExplicativos.Length > pasoActual - 1)
            gameManager.ReproducirSFX(audiosExplicativos[pasoActual - 1]);

        if (pasoActual == 5)
        {
            if (botonAnalizarTipos != null && !botonAnalizarTipos.activeSelf)
            {
                botonAnalizarTipos.SetActive(true);
                gameManager.ReproducirSFX(audioFaseCompletada);
            }
        }
        else { if (botonAnalizarTipos != null) botonAnalizarTipos.SetActive(false); }
        pasoAnterior = pasoActual;
    }

    private void ActualizarVisualesFaseA(int paso)
    {
        for (int i = 0; i < partesDelVirus.Length; i++)
        {
            if (partesDelVirus[i] != null) partesDelVirus[i].SetActive(i < paso);
            bool etiquetaActiva = (i == (paso - 1) && paso > 0);
            if (etiquetasFlotantes != null && i < etiquetasFlotantes.Length && etiquetasFlotantes[i] != null)
                etiquetasFlotantes[i].SetActive(etiquetaActiva);
        }
        if (textoNombreParte != null)
        {
            switch (paso)
            {
                case 0: textoNombreParte.text = "Desliza para analizar"; break;
                case 1: textoNombreParte.text = "ARN"; break;
                case 2: textoNombreParte.text = "Capa Proteica"; break;
                case 3: textoNombreParte.text = "Membrana Lipídica"; break;
                case 4: textoNombreParte.text = "Hemaglutinina (H)"; break;
                case 5: textoNombreParte.text = "Neuraminidasa (N)"; break;
            }
        }
    }

    // ---------------------------------------------------------
    // LÓGICA FASE B (Tipos)
    // ---------------------------------------------------------
    public void IniciarFaseB()
    {
        if (panelSliderAnatomia != null) panelSliderAnatomia.SetActive(false);
        if (botonAnalizarTipos != null) botonAnalizarTipos.SetActive(false);
        foreach (var etiqueta in etiquetasFlotantes) if (etiqueta) etiqueta.SetActive(false);
        if (panelFaseTipos != null) AlternarPanel(panelFaseTipos);

        indiceH = 2; indiceN = 1;
        ActualizarVisualesFaseB();
        if (audioIntroFaseB != null) gameManager.ReproducirSFX(audioIntroFaseB);
    }

    public void CambiarH()
    {
        if (faseBCompletada) return;
        indiceH++; if (indiceH >= coloresH.Length) indiceH = 0;
        if (audioCambioTipo != null) gameManager.sfxSource.PlayOneShot(audioCambioTipo);
        ActualizarVisualesFaseB(); VerificarVictoria();
    }

    public void CambiarN()
    {
        if (faseBCompletada) return;
        indiceN++; if (indiceN >= coloresN.Length) indiceN = 0;
        if (audioCambioTipo != null) gameManager.sfxSource.PlayOneShot(audioCambioTipo);
        ActualizarVisualesFaseB(); VerificarVictoria();
    }

    private void ActualizarVisualesFaseB()
    {
        if (rendererPicosH != null && coloresH.Length > 0) rendererPicosH.material.color = coloresH[indiceH];
        if (rendererPicosN != null && coloresN.Length > 0) rendererPicosN.material.color = coloresN[indiceN];
        if (textoCombinacionVirus != null) textoCombinacionVirus.text = $"H{indiceH + 1}N{indiceN + 1}";
    }

    private void VerificarVictoria()
    {
        if (indiceH == 0 && indiceN == 0) // H1N1
        {
            faseBCompletada = true;
            if (audioH1N1Encontrado != null) gameManager.ReproducirSFX(audioH1N1Encontrado);
            Invoke("IniciarFaseC", 4f);
        }
    }

    // ---------------------------------------------------------
    // LÓGICA FASE C (Infección)
    // ---------------------------------------------------------
    public void IniciarFaseC()
    {
        if (panelFaseTipos != null) panelFaseTipos.SetActive(false);
        if (textoCombinacionVirus != null) textoCombinacionVirus.gameObject.SetActive(false);
        if (modeloParaRotar != null) modeloParaRotar.gameObject.SetActive(false);
        StartCoroutine(RutinaOleadaInfecciosa());
    }

    private IEnumerator RutinaOleadaInfecciosa()
    {
        if (audioAlertaMultiplicacion != null) gameManager.ReproducirSFX(audioAlertaMultiplicacion);
        int virusGenerados = 0;
        float tiempoEntreSpawns = 2.5f;

        while (virusGenerados < cantidadTotalParaColapso)
        {
            SpawnearEnemigo();
            virusGenerados++;
            tiempoEntreSpawns *= 0.95f;
            if (tiempoEntreSpawns < 0.4f) tiempoEntreSpawns = 0.4f;
            yield return new WaitForSeconds(tiempoEntreSpawns);
        }
        IniciarAtaqueFinal();
    }

    private void SpawnearEnemigo()
    {
        if (prefabVirusEnemigo == null || Camera.main == null) return;
        Vector3 posicionRandom = Random.onUnitSphere * radioSpawn;
        Vector3 posicionSpawn = Camera.main.transform.position + posicionRandom;
        GameObject nuevoVirus = Instantiate(prefabVirusEnemigo, posicionSpawn, Quaternion.identity);
        Stage_Virus_Enemy scriptEnemy = nuevoVirus.GetComponent<Stage_Virus_Enemy>();
        if (scriptEnemy != null)
        {
            Color colorH = coloresH[Random.Range(0, coloresH.Length)];
            Color colorN = coloresN[Random.Range(0, coloresN.Length)];
            scriptEnemy.ConfigurarColores(colorH, colorN);
        }
    }

    private void IniciarAtaqueFinal()
    {
        UIManager.Instance?.Log("¡COLAPSO! ATAQUE MASIVO.");
        if (audioFalloSistema != null) { gameManager.sfxSource.Stop(); gameManager.ReproducirSFX(audioFalloSistema); }
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");
        foreach (GameObject enemigo in enemigos)
        {
            Stage_Virus_Enemy script = enemigo.GetComponent<Stage_Virus_Enemy>();
            if (script != null) script.IniciarAtaqueKamikaze();
        }
        Invoke("EfectoImpactoFinal", 1.5f);
    }

    private void EfectoImpactoFinal()
    {
        if (audioImpactoPantalla != null) gameManager.ReproducirSFX(audioImpactoPantalla);
        if (panelAlerta != null) MostrarPanelPulsante(panelAlerta);

        // Limpiar enemigos y preparar final
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");
        foreach (GameObject e in enemigos) Destroy(e);

        Invoke("PrepararFaseDFinal", 2.5f);
    }

    // ---------------------------------------------------------
    // LÓGICA FASE D: GESTOR DE SPAWNEO (NUEVO)
    // ---------------------------------------------------------
    private void PrepararFaseDFinal()
    {
        // 1. Limpieza
        if (panelAlerta != null) panelAlerta.SetActive(false);
        enFaseFinal = true;
        contadorSpawnsFaseD = 0;

        // 2. IMPORTANTE: Aseguramos que el MidAirStage (el virus original) esté oculto
        // pero necesitamos el objeto "padre" activo para saber la posición. 
        // Como 'modeloParaRotar' es hijo, ya lo desactivamos en Fase C, así que el Anchor está invisible pero funcional.

        // 3. Reactivar el Plane Finder existente
        if (vuforiaPlaneFinder != null)
        {
            vuforiaPlaneFinder.SetActive(true);
            UIManager.Instance?.Log("Toque 1: Desplegar Video. Toque 2: Desplegar Info.");
            if (panelInstruccionMidAir != null) MostrarPanelPulsante(panelInstruccionMidAir);
        }
    }

    // ¡ASIGNA ESTA FUNCIÓN AL EVENTO 'ON CONTENT PLACED' DEL PLANE FINDER!
    public void OnFaseDFinal_SpawnContent()
    {
        // Si no estamos en la fase final, el Plane Finder está actuando para la Fase A, así que no hacemos nada aquí.
        if (!enFaseFinal) return;

        // Si el anchor es null, no sabemos donde ponerlo
        if (midAirStageAnchor == null) return;

        // Obtenemos la posición donde Vuforia acaba de mover el cursor (Anchor)
        Vector3 posicionSpawn = midAirStageAnchor.transform.position;
        Quaternion rotacionSpawn = Quaternion.identity;
        // Nota: La rotación la manejará el script Billboard del prefab, así que identity está bien.

        if (contadorSpawnsFaseD == 0)
        {
            // TOQUE 1: VIDEO
            if (prefabVideoFinal != null)
            {
                Instantiate(prefabVideoFinal, posicionSpawn, rotacionSpawn);
                if (audioLoopEscaneo != null && gameManager.sfxSource != null) gameManager.sfxSource.Stop();
                UIManager.Instance?.Log("Video desplegado. Toca otra vez para Info.");
            }
        }
        else if (contadorSpawnsFaseD == 1)
        {
            // TOQUE 2: PANEL INFO
            if (prefabPanelInfoFinal != null)
            {
                Instantiate(prefabPanelInfoFinal, posicionSpawn, rotacionSpawn);
                UIManager.Instance?.Log("Info desplegada. Fin de Spawneo.");
            }

            // DESACTIVAR EL FINDER para que no salgan más cosas
            if (vuforiaPlaneFinder != null) vuforiaPlaneFinder.SetActive(false);
            if (panelInstruccionMidAir != null) panelInstruccionMidAir.SetActive(false);
        }

        contadorSpawnsFaseD++;
    }

    // Helpers
    private void ManejarRotacion()
    {
        if (EsToqueEnUI()) return;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                float rotacionX = -touch.deltaPosition.x * velocidadRotacion;
                if (modeloParaRotar != null) modeloParaRotar.Rotate(Vector3.up, rotacionX, Space.World);
            }
        }
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            float rotacionX = -Input.GetAxis("Mouse X") * velocidadRotacion * 20f;
            if (modeloParaRotar != null) modeloParaRotar.Rotate(Vector3.up, rotacionX, Space.World);
        }
#endif
    }

    private bool EsToqueEnUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;
        return false;
    }
}