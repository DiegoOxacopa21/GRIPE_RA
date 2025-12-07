using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Base")]
    public GameObject npcBasePrefab;
    public List<GameObject> modelosDisponibles;
    public int maxNPCs = 6; // Límite de botones

    // Cuántos infectaremos al dar al botón "Iniciar"
    public int infectadosAlIniciar = 1;

    [Header("Spawn Area")]
    public float spawnRadius = 0.35f;

    private List<NPCController> npcs = new List<NPCController>();
    private Transform suelo;

    void Start()
    {
        // YA NO spawneamos automáticamente aquí.
        // Solo buscamos referencias.
        UIManager.Instance?.Log("NPCSpawner: Listo (Esperando comandos de Stage).");
        suelo = transform.Find("Suelo");
    }

    // ---------------------------------------------------------
    // MÉTODOS NUEVOS PARA LOS BOTONES
    // ---------------------------------------------------------

    // Se llama con el Botón 1 (Agregar NPC)
    public void SpawnSingleNPC()
    {
        if (npcs.Count >= maxNPCs)
        {
            UIManager.Instance?.Log("NPCSpawner: Máximo de NPCs alcanzado.");
            return;
        }

        if (suelo == null) return;

        Vector3 pos = GetRandomPointOnNavMesh();
        GameObject npc = Instantiate(npcBasePrefab, pos, Quaternion.identity, transform);

        NPCController ctrl = npc.GetComponent<NPCController>();
        NPCModelLoader loader = npc.GetComponent<NPCModelLoader>();

        if (ctrl != null && loader != null)
        {
            // 1. Guardar referencia
            npcs.Add(ctrl);

            // 2. Asignar modelo aleatorio
            GameObject modelPrefab = modelosDisponibles[Random.Range(0, modelosDisponibles.Count)];
            loader.ApplyModel(modelPrefab);

            // 3. IMPORTANTE: Asegurar que nazca QUIETO
            ctrl.simulacionActiva = false;

            UIManager.Instance?.Log($"NPCSpawner: NPC agregado ({npcs.Count}/{maxNPCs}).");
        }
    }

    // Se llama con el Botón 2 (Iniciar Infección)
    public void IniciarSimulacionMasiva()
    {
        UIManager.Instance?.Log("NPCSpawner: Iniciando movimiento e infección...");

        // 1. Infectar aleatoriamente a X personajes
        InfectRandom(infectadosAlIniciar);

        // 2. Despertar a todos
        foreach (var npc in npcs)
        {
            if (npc != null)
            {
                npc.ActivarMovimiento(); // Pasa a TRUE y busca destino
            }
        }
    }

    // ---------------------------------------------------------
    // UTILIDADES (Igual que antes)
    // ---------------------------------------------------------

    Vector3 GetRandomPointOnNavMesh()
    {
        Vector3 rnd = Random.insideUnitSphere * spawnRadius;
        if (suelo != null)
        {
            rnd += suelo.position;
            rnd.y = suelo.position.y + 0.05f;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rnd, out hit, 0.4f, NavMesh.AllAreas))
            return hit.position;

        return (suelo != null) ? suelo.position : transform.position;
    }

    void InfectRandom(int cantidad)
    {
        if (npcs.Count == 0) return;

        // Mezclamos un poco para que no siempre sea el primero
        for (int k = 0; k < cantidad; k++)
        {
            int index = Random.Range(0, npcs.Count);
            // Intentamos infectar (si ya está, no pasa nada)
            npcs[index].Infect();
        }
    }
}