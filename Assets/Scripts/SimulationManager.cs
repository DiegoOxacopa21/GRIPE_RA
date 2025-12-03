using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public NPCSpawner npcSpawner;

    void Start()
    {
        UIManager.Instance?.Log("SimulationManager: Start ejecutado");
    }

    public void ConfigurarDespuesDeInstanciar(GameObject maqueta)
    {
        Transform suelo = maqueta.transform.Find("Suelo");

        if (suelo == null)
        {
            UIManager.Instance?.Log("ERROR SimulationManager: no hay Suelo.");
            return;
        }

        if (npcSpawner == null)
            npcSpawner = maqueta.GetComponentInChildren<NPCSpawner>();

        UIManager.Instance?.Log("SimulationManager: Maqueta configurada (spawn desactivado/esperando).");
        // El NPCSpawner seguirá su propio Start (está habilitado por GroundPlaneActivator)
    }
}
