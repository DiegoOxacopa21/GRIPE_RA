using System.Diagnostics;
using UnityEngine;
using Vuforia;

public class GroundPlaneActivator : MonoBehaviour
{
    [Header("Asignar desde el Inspector")]
    public GameObject simulationPrefab;        // Prefab SimulacionMaqueta (el root)
    public ContentPositioningBehaviour contentPositioner;
    public PlaneFinderBehaviour planeFinder;

    private bool placed = false;

    private void OnEnable()
    {
        contentPositioner.OnContentPlaced.AddListener(HandleContentPlaced);
    }

    private void OnDisable()
    {
        contentPositioner.OnContentPlaced.RemoveListener(HandleContentPlaced);
    }

    private void HandleContentPlaced(GameObject content)
    {
        if (placed) return;
        placed = true;

        // 1. Activar la maqueta
        simulationPrefab.SetActive(true);

        // 2. Moverla al anchor donde Vuforia colocó el contenido
        simulationPrefab.transform.position = content.transform.position;
        simulationPrefab.transform.rotation = content.transform.rotation;

        // 3. Activar la simulación
        var simManager = simulationPrefab.GetComponentInChildren<SimulationManager>();
        if (simManager != null)
            simManager.enabled = true;

        var spawner = simulationPrefab.GetComponentInChildren<NPCSpawner>();
        if (spawner != null)
            spawner.enabled = true;

        // 4. Desactivar PlaneFinder (previsualización + detección)
        planeFinder.gameObject.SetActive(false);

        UIManager.Instance?.Log("GroundPlaneActivator: Simulación activada correctamente.");
        UIManager.Instance?.Log(" Simulación iniciada por Ground Plane.");

    }
}
