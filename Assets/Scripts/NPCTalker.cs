using UnityEngine;

public class NPCTalker : MonoBehaviour
{
    public float talkDuration = 3.5f;

    private NPCController myController;
    private NPCController otherController;

    private bool isTalking = false;
    private float timer = 0f;

    void Start()
    {
        myController = GetComponentInParent<NPCController>();
        if (myController == null)
            Debug.LogError("NPCTalker: No se encontró NPCController en el padre.");
    }

    void Update()
    {
        if (!isTalking) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            EndConversation();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTalking) return;
        if (myController == null) return;

        // Evita que converse con sí mismo
        if (other.transform == transform.parent) return;

        NPCController oc = other.GetComponentInParent<NPCController>();
        if (oc == null) return;

        // Si el otro NPC ya está hablando  no iniciar
        if (oc.IsTalking()) return;
        if (myController.IsTalking()) return;

        StartConversation(oc);
    }

    void StartConversation(NPCController target)
    {
        if (myController == null || target == null) return;

        isTalking = true;
        otherController = target;

        myController.StartTalking(target.transform);
        target.StartTalking(transform.parent);

        timer = talkDuration;

        // DEBUG CUBO conversación (azul)
        Vector3 mid = (myController.transform.position + target.transform.position) / 2f;
        myController.DebugCube(mid, Color.blue, 0.5f, 3f);

        // Contagio
        if (myController.isInfected && !target.isInfected)
            target.Infect();
        else if (target.isInfected && !myController.isInfected)
            myController.Infect();
    }

    void EndConversation()
    {
        if (myController != null) myController.StopTalking();
        if (otherController != null) otherController.StopTalking();

        // DEBUG cubo fin conversación (amarillo)
        myController.DebugCube(myController.transform.position, Color.yellow, 0.4f, 2f);

        isTalking = false;
        otherController = null;
    }
}
