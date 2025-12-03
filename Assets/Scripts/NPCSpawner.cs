using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Base")]
    public GameObject npcBasePrefab;
    public List<GameObject> modelosDisponibles;
    public int cantidadNPC = 4;
    public int infectadosIniciales = 1;

    [Header("Spawn Area")]
    public float spawnRadius = 0.35f;

    private List<NPCController> npcs = new List<NPCController>();
    private Transform suelo;

    void Start()
    {
        UIManager.Instance?.Log("NPCSpawner: Start ejecutado");
        suelo = transform.Find("Suelo");
        if (suelo == null)
        {
            Debug.LogError("NPCSpawner: No se encontró 'Suelo'.");
            return;
        }

        SpawnNPCs();
        InfectRandom();
    }

    void SpawnNPCs()
    {
        for (int i = 0; i < cantidadNPC; i++)
        {
            Vector3 pos = GetRandomPointOnNavMesh();

            GameObject npc = Instantiate(npcBasePrefab, pos, Quaternion.identity, transform);

            NPCController ctrl = npc.GetComponent<NPCController>();
            NPCModelLoader loader = npc.GetComponent<NPCModelLoader>();

            if (ctrl == null || loader == null)
            {
                Debug.LogError("NPCSpawner: NPC_Base necesita NPCController y NPCModelLoader.");
                continue;
            }

            npcs.Add(ctrl);

            // asignar modelo
            GameObject modelPrefab = modelosDisponibles[
                Random.Range(0, modelosDisponibles.Count)
            ];
            loader.ApplyModel(modelPrefab);

            // iniciar movimiento
            ctrl.SetRandomDestination();
        }
    }

    Vector3 GetRandomPointOnNavMesh()
    {
        Vector3 rnd = Random.insideUnitSphere * spawnRadius;
        rnd += suelo.position;
        rnd.y = suelo.position.y + 0.05f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rnd, out hit, 0.4f, NavMesh.AllAreas))
            return hit.position;

        return suelo.position + new Vector3(0, 0.05f, 0);
    }

    void InfectRandom()
    {
        if (infectadosIniciales <= 0) return;

        for (int i = 0; i < infectadosIniciales; i++)
        {
            int index = Random.Range(0, npcs.Count);
            npcs[index].Infect();
        }
    }
}
