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

    // NUEVO: Controla si se pueden mover o no
    public bool simulacionActiva = false;

    private NavMeshAgent agent;
    private Animator animator;
    private Renderer[] renderers;

    private float idleTimer = 0f;
    private bool isTalking = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Configuración original
        agent.speed = Random.Range(0.25f, 0.45f);
        agent.acceleration = 10f;
        agent.angularSpeed = 110f;
        agent.updateRotation = false; // rotación manual
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
        else
        {
            UIManager.Instance?.Log($"NPCController: No se encontró NPCModelLoader en {name}.");
        }

        // Si nace infectado, actualizar color visualmente aunque esté quieto
        if (isInfected) ActualizarColorInfeccion();
    }

    void Update()
    {
        // 1. NUEVO: Si la simulación no ha iniciado, no hacemos lógica de movimiento/idle
        if (!simulacionActiva) return;

        // 2. Si está hablando, no se mueve
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
        // NUEVO: Doble seguridad para no moverse si está pausado
        if (!simulacionActiva) return;

        idleTimer = Random.Range(minIdleTime, maxIdleTime);

        Vector3 dir = Random.insideUnitSphere * wanderRadius;
        dir += transform.position;
        dir.y = transform.position.y;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(dir, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            // UIManager.Instance?.Log($"NPCController: {name} -> nuevo destino.");
        }
    }

    // NUEVO: Método para que NPCSpawner active a este personaje
    public void ActivarMovimiento()
    {
        simulacionActiva = true;
        SetRandomDestination(); // Empezar a caminar inmediatamente
    }

    // -------------------------------------------------------
    // CONVERSACIÓN (Restaurado)
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
                // Rotación instantánea o suavizada hacia quien habla
                transform.rotation = Quaternion.LookRotation(d);
            }
        }

        if (animator != null)
        {
            animator.SetBool("IsTalking", true);
            animator.SetFloat("MoveSpeed", 0f);
        }
    }

    public void StopTalking()
    {
        isTalking = false;

        // Solo reanudamos el agente si la simulación sigue activa
        if (agent.isActiveAndEnabled) agent.isStopped = false;

        if (animator != null)
            animator.SetBool("IsTalking", false);

        idleTimer = 0f;

        // Si la simulación sigue activa, buscamos nuevo destino
        if (simulacionActiva)
            SetRandomDestination();
    }

    // -------------------------------------------------------
    // INFECCIÓN
    // -------------------------------------------------------

    public void Infect()
    {
        if (isInfected) return;

        isInfected = true;

        UIManager.Instance?.Log($"NPCController: {name} Infectado.");
        ActualizarColorInfeccion();
    }

    void ActualizarColorInfeccion()
    {
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                r.material.color = infectedColor;
            }
        }
    }
}