using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public NPCSpawner npcSpawner;

    void Start()
    {
        UIDebugger.Instance?.Log("SimulationManager: Start ejecutado");
    }

    public void ConfigurarDespuesDeInstanciar(GameObject maqueta)
    {
        Transform suelo = maqueta.transform.Find("Suelo");

        if (suelo == null)
        {
            Debug.LogError("SimulationManager: no hay Suelo.");
            return;
        }

        if (npcSpawner == null)
            npcSpawner = maqueta.GetComponentInChildren<NPCSpawner>();

        // simplemente dejar que el spawner haga su Start normal
    }
}
