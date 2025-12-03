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
            UIManager.Instance?.Log("ERROR NPCTalker: No se encontró NPCController en el padre.");
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

        // DEBUG: informe al UIManager en vez de crear objetos 3D
        //UIManager.Instance?.Log($"NPCTalker: Conversación iniciada entre '{myController.name}' y '{target.name}'.");

        // Contagio
        if (myController.isInfected && !target.isInfected)
        {
            target.Infect();
            UIManager.Instance?.Log($"NPCTalker: {myController.name} contagió a {target.name}.");
        }
        else if (target.isInfected && !myController.isInfected)
        {
            myController.Infect();
            UIManager.Instance?.Log($"NPCTalker: {target.name} contagió a {myController.name}.");
        }
    }

    void EndConversation()
    {
        if (myController != null) myController.StopTalking();
        if (otherController != null) otherController.StopTalking();

        //UIManager.Instance?.Log($"NPCTalker: Conversación finalizada para '{myController?.name}'.");

        isTalking = false;
        otherController = null;
    }
}
