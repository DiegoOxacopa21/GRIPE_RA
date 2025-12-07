using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Flujo de Etapas")]
    public List<StageBase> stagesList;
    private int currentStageIndex = 0;
    private StageBase currentStage;

    [Header("Canales de Audio (GameManager)")]
    // Estos son INDEPENDIENTES del AudioSource que tienes en UIManager
    public AudioSource sfxSource;   // Arrastra aquí el AudioSource para narraciones/efectos
    public AudioSource musicSource; // Arrastra aquí el AudioSource para música de fondo

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Apagar todas al inicio por seguridad
        foreach (var stage in stagesList)
        {
            if (stage != null) stage.gameObject.SetActive(false);
        }

        // Iniciar la primera
        if (stagesList.Count > 0)
        {
            currentStageIndex = 0;
            ActivarEtapa(currentStageIndex);
        }
        else
        {
            UIManager.Instance?.Log("ERROR: La lista de etapas está vacía en GameManager.");
        }
    }

    private void Update()
    {
        if (currentStage != null)
        {
            currentStage.UpdateStage();
        }
    }

    public void AvanzarEtapa()
    {
        // 1. Cerrar etapa actual
        if (currentStage != null)
        {
            currentStage.EndStage(); // Aquí sonará el 'audioAlTerminar'
            currentStage.gameObject.SetActive(false);
        }

        // 2. Mover índice
        currentStageIndex++;

        // 3. Verificar siguiente
        if (currentStageIndex < stagesList.Count)
        {
            ActivarEtapa(currentStageIndex);
        }
        else
        {
            UIManager.Instance?.Log("--- JUEGO TERMINADO ---");
            // Aquí podrías llamar a UIManager para abrir panel de fin o volver al menú
        }
    }

    private void ActivarEtapa(int index)
    {
        currentStage = stagesList[index];
        currentStage.gameObject.SetActive(true);
        currentStage.InitStage(); // Aquí sonará 'audioAlIniciar' y 'musicaDeFondo'
    }

    // --- LÓGICA DE AUDIO GLOBAL ---

    public void ReproducirSFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void ReproducirMusicaFondo(AudioClip clip)
    {
        if (musicSource != null)
        {
            if (clip != null)
            {
                // Solo cambiamos si es una canción diferente para evitar cortes
                if (musicSource.clip != clip)
                {
                    musicSource.clip = clip;
                    musicSource.loop = true;
                    musicSource.Play();
                }
            }
            else
            {
                musicSource.Stop();
            }
        }
    }
}