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
    public AudioSource sfxSource;
    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        foreach (var stage in stagesList)
        {
            if (stage != null) stage.gameObject.SetActive(false);
        }

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
        // --- CORRECCIÓN 1: LIMPIEZA DE AUDIO ---
        // Detenemos cualquier narración que esté sonando antes de cerrar la etapa
        if (sfxSource.isPlaying) sfxSource.Stop();

        // 1. Cerrar etapa actual
        if (currentStage != null)
        {
            currentStage.EndStage();
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
            // Opcional: Detener música también si el juego acaba
            // if (musicSource.isPlaying) musicSource.Stop();
        }
    }

    private void ActivarEtapa(int index)
    {
        currentStage = stagesList[index];
        currentStage.gameObject.SetActive(true);
        currentStage.InitStage();
    }

    // --- LÓGICA DE AUDIO GLOBAL CORREGIDA ---

    public void ReproducirSFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            // --- CORRECCIÓN 2: MODO NARRADOR ---
            // En lugar de PlayOneShot (que mezcla sonidos), usamos Play() normal.
            // Esto reemplaza el clip actual con el nuevo, cortando automáticamente el anterior.
            // Ideal para diálogos de ElevenLabs para que no se atropellen.

            sfxSource.Stop();        // Detiene el anterior por seguridad
            sfxSource.clip = clip;   // Asigna el nuevo
            sfxSource.Play();        // Reproduce
        }
    }

    public void ReproducirMusicaFondo(AudioClip clip)
    {
        if (musicSource != null)
        {
            if (clip != null)
            {
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