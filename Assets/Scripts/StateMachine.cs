using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StateMachine : MonoBehaviour
{
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Searching,
        Attacking,
        Retreating
    }

    public Transform[] patrolWaypoints;
    private int currentWaypointIndex;

    public Color patrollingColor = Color.blue;
    public Color chasingColor = Color.red;
    public Color searchingColor = Color.yellow;
    public Color attackingColor = Color.magenta;
    public Color retreatingColor = Color.green;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float retreatSpeed = 3f;

    public float patrolDuration = 10f;
    public float searchDuration = 5f;
    public float retreatDuration = 8f;
    public float detectionDistance = 30f;

    public float attackDistance = 10f;

    private NavMeshAgent agent;
    private Transform player;
    private Vector3 lastKnownPlayerPosition;
    private float stateTimer;
    private EnemyState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        SetState(EnemyState.Patrolling);
        currentWaypointIndex = 0;
        SetPatrolDestination();
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        switch (currentState)
        {
            case EnemyState.Patrolling:
                PatrollingUpdate();
                break;
            case EnemyState.Chasing:
                ChasingUpdate();
                break;
            case EnemyState.Searching:
                SearchingUpdate();
                break;
            case EnemyState.Attacking:
                AttackingUpdate();
                break;
            case EnemyState.Retreating:
                RetreatingUpdate();
                break;
        }

        UpdateColor();
    }

    void SetState(EnemyState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    void PatrollingUpdate()
    {
        agent.speed = patrolSpeed;

        if (agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            SetPatrolDestination();
        }

        if (Vector3.Distance(transform.position, player.position) < detectionDistance)
        {
            SetState(EnemyState.Chasing);
        }
    }

    void SetPatrolDestination()
    {
        agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
    }

    void ChasingUpdate()
    {
        agent.speed = chaseSpeed;

        if (Vector3.Distance(transform.position, player.position) < detectionDistance)
        {
            lastKnownPlayerPosition = player.position;

            agent.SetDestination(lastKnownPlayerPosition);

            while (Vector3.Distance(transform.position, player.position) < attackDistance)
            {
                SetState(EnemyState.Attacking);
                return;
            }
        }
        else
        {
            SetState(EnemyState.Searching);
        }
    }

    void SearchingUpdate()
    {
        agent.speed = patrolSpeed;

        agent.SetDestination(lastKnownPlayerPosition);

        if (Vector3.Distance(transform.position, player.position) < detectionDistance)
        {
            SetState(EnemyState.Chasing);
        }
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
        {
            SetState(EnemyState.Retreating);
        }
    }

    void AttackingUpdate()
    {
        agent.speed = 0f;

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            Debug.Log("Enemy is attacking the player!");
        }
        else
        {
            SetState(EnemyState.Chasing);
        }
    }

    void RetreatingUpdate()
    {
        agent.speed = retreatSpeed;

        agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);

        if (agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
        }

        stateTimer += Time.deltaTime;

        if (stateTimer >= retreatDuration)
        {
            SetState(EnemyState.Patrolling);
            stateTimer = 0f;
        }
    }

    void UpdateColor()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                GetComponent<Renderer>().material.color = patrollingColor;
                break;
            case EnemyState.Chasing:
                GetComponent<Renderer>().material.color = chasingColor;
                break;
            case EnemyState.Searching:
                GetComponent<Renderer>().material.color = searchingColor;
                break;
            case EnemyState.Attacking:
                GetComponent<Renderer>().material.color = attackingColor;
                break;
            case EnemyState.Retreating:
                GetComponent<Renderer>().material.color = retreatingColor;
                break;
        }
    }
}