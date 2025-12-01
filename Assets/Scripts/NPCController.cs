using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("Movimiento")]
    public float wanderRadius = 1.2f;
    public float minIdleTime = 1.0f;
    public float maxIdleTime = 2.0f;

    [Header("Contagio")]
    public bool isInfected = false;
    public Color infectedColor = Color.red;

    private NavMeshAgent agent;
    private Animator animator;
    private Renderer[] renderers;

    private float idleTimer = 0f;
    private bool isTalking = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = Random.Range(0.25f, 0.45f);  // lento NATURAL para escala 0.1
        agent.acceleration = 10f;
        agent.angularSpeed = 110f;
        agent.updateRotation = false; // rotación manual evita caminar hacia atrás
    }

    void Start()
    {
        Invoke(nameof(ResolveAnimator), 0.05f);
    }

    void ResolveAnimator()
    {
        var loader = GetComponent<NPCModelLoader>();

        if (loader != null)
        {
            animator = loader.GetModelAnimator();
            renderers = loader.GetModelRenderers();
        }
    }

    void Update()
    {
        if (isTalking) return;
        if (animator == null) return;

        UpdateMovementState();

        // Movimiento + Idle
        if (!agent.pathPending && agent.remainingDistance <= 0.05f)
        {
            idleTimer -= Time.deltaTime;

            if (idleTimer <= 0)
                SetRandomDestination();
        }

        // Rotación manual (para evitar caminar hacia atrás)
        if (agent.velocity.magnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 3f);
        }
    }

    // -------------------------------------------------------
    // DEBUG VISUAL CON CUBOS
    // -------------------------------------------------------

    public void DebugCube(Vector3 pos, Color color, float size = 0.3f, float duration = 2f)
    {
        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.transform.position = pos + Vector3.up * 1.2f;
        c.transform.localScale = Vector3.one * size;

        Renderer r = c.GetComponent<Renderer>();
        r.material.color = color;

        Destroy(c, duration);
    }

    // -------------------------------------------------------

    public bool IsTalking()
    {
        return isTalking;
    }

    void UpdateMovementState()
    {
        float v = agent.velocity.magnitude;
        animator.SetFloat("MoveSpeed", v);

        if (v < 0.05f)
            animator.SetBool("IsTalking", false);
    }

    public void SetRandomDestination()
    {
        idleTimer = Random.Range(minIdleTime, maxIdleTime);

        Vector3 dir = Random.insideUnitSphere * wanderRadius;
        dir += transform.position;
        dir.y = transform.position.y;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(dir, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);

            // DEBUG cubo verde en destino
            DebugCube(hit.position, Color.green, 0.25f, 2f);
        }
    }

    // -------------------------------------------------------
    // CONVERSACIÓN
    // -------------------------------------------------------

    public void StartTalking(Transform lookTarget)
    {
        isTalking = true;
        agent.isStopped = true;

        if (lookTarget != null)
        {
            Vector3 d = lookTarget.position - transform.position;
            d.y = 0;

            if (d != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(d);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 8f * Time.deltaTime);
            }
        }

        animator.SetBool("IsTalking", true);
        animator.SetFloat("MoveSpeed", 0f);
    }

    public void StopTalking()
    {
        isTalking = false;
        agent.isStopped = false;
        animator.SetBool("IsTalking", false);

        idleTimer = 0f;
        SetRandomDestination();
    }

    // -------------------------------------------------------
    // INFECCIÓN
    // -------------------------------------------------------

    public void Infect()
    {
        if (isInfected) return;

        isInfected = true;

        // Cubo ROJO debug infección
        DebugCube(transform.position, Color.red, 0.5f, 3f);

        if (renderers != null)
        {
            foreach (var r in renderers)
                r.material.color = infectedColor;
        }
    }
}
