using UnityEngine;
using UnityEngine.AI;

namespace Script
{
    public class EnemyAI : MonoBehaviour
    {
        enum AIState
        {
            Idle,
            Patrol,
            Chase,
        }

        [Header("Patrol")] 
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float waitAtPoint = 1f;
        private int currentWaypoint;
        private float waitCounter;

        [Header("Components")] 
        NavMeshAgent agent;

        [Header("AI States")] 
        [SerializeField] private AIState currentState;
        [Header("Chasing")] 
        [SerializeField] private float chaseRange = 7f;
        [SerializeField] private float attackRange = 3f;

        [Header("Suspicion")] 
        [SerializeField] private float suspicionTime = 1.5f;
        private float timeSinceLastSuspicion;
        [Header("Player")]
        [SerializeField] private GameObject player;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            var component = GetComponent<SphereCollider>();
            if (component != null) component.radius = attackRange;
            waitCounter = waitAtPoint;
            timeSinceLastSuspicion = suspicionTime;
        }

        // Update is called once per frame
        void Update()
        {
            float distToPlayer = Vector3.Distance(player.transform.position, transform.position);
            switch (currentState)
            {
                case AIState.Idle:
                    if (waitCounter > 0)
                    {
                        waitCounter -= Time.deltaTime;
                    }
                    else
                    {
                        currentState = AIState.Patrol;
                        agent.SetDestination(patrolPoints[currentWaypoint].position);
                    }
                
                    if(distToPlayer <= chaseRange)
                    {
                        currentState = AIState.Chase;
                    }
                    break;
                case AIState.Patrol:
                    if (agent.remainingDistance <= 0.2f)
                    {
                        currentWaypoint++;
                        if (currentWaypoint >= patrolPoints.Length)
                        {
                            currentWaypoint = 0;
                        }

                        currentState = AIState.Idle;
                        waitCounter = waitAtPoint;
                    }

                    if (distToPlayer <= chaseRange)
                    {
                        currentState = AIState.Chase;
                    }
                    break;
                case AIState.Chase:
                    agent.SetDestination(player.transform.position);
                    if (distToPlayer > chaseRange)
                    {
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                        timeSinceLastSuspicion -= Time.deltaTime;
                        if (timeSinceLastSuspicion <= 0)
                        {
                            currentState = AIState.Idle;
                            waitCounter = waitAtPoint;
                            timeSinceLastSuspicion = suspicionTime;
                        }
                    }
                    break;
            }
        }
    }
}