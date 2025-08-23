using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Script
{
    public class EnemyAI : MonoBehaviour
    {
        private static readonly int IsWalking = Animator.StringToHash("isWalking");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
        private static readonly int Death1 = Animator.StringToHash("Death");

        public enum AIState
        {
            Idle,
            Patrol,
            Chase,
        }

        public Animator animator;
        [Header("Patrol")] [SerializeField] private Transform patrolArea;
        [SerializeField] private float waitAtPoint = 1f;
        private int currentWaypoint;
        private float waitCounter;

        [Header("Components")] public NavMeshAgent agent;

        [Header("AI States")] [SerializeField] public AIState currentState;
        [Header("Chasing")] [SerializeField] private float chaseRange = 7f;
        [SerializeField] private float attackRange = 3f;

        [Header("Suspicion")] [SerializeField] private float suspicionTime = 1.5f;
        private float timeSinceLastSuspicion;
        [Header("Player")] [SerializeField] private GameObject player;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            var component = GetComponent<SphereCollider>();
            if (component) component.radius = attackRange;
            waitCounter = waitAtPoint;
            timeSinceLastSuspicion = suspicionTime;
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            float distToPlayer = Vector3.Distance(player.transform.position, transform.position);

            ControlAI(distToPlayer);
        }

        void ControlAI(float distToPlayer)
        {
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
                        agent.SetDestination(patrolArea.GetChild(currentWaypoint).position);
                        if (animator)
                        {
                            animator?.SetBool(IsWalking, true);
                        }
                    }

                    if (distToPlayer <= chaseRange)
                    {
                        currentState = AIState.Chase;
                        if (animator)
                        {
                            animator?.SetBool(IsWalking, true);
                        }
                    }

                    break;
                case AIState.Patrol:
                    if (agent.remainingDistance <= 0.4f)
                    {
                        currentWaypoint++;
                        if (currentWaypoint >= patrolArea.childCount)
                        {
                            currentWaypoint = 0;
                        }

                        currentState = AIState.Idle;
                        if (animator)
                        {
                            animator?.SetBool(IsWalking, false);
                        }

                        waitCounter = waitAtPoint;
                    }

                    if (distToPlayer <= chaseRange)
                    {
                        currentState = AIState.Chase;
                        if (animator)
                        {
                            animator?.SetBool(IsWalking, true);
                        }
                    }

                    break;
                case AIState.Chase:
                    agent.SetDestination(player.transform.position);
                    if (distToPlayer > chaseRange - attackRange)
                    {
                        agent.isStopped = true;
                        timeSinceLastSuspicion -= Time.deltaTime;
                        if (animator)
                        {
                            animator?.SetBool(IsWalking, false);
                        }

                        agent.velocity = Vector3.zero;
                        if (timeSinceLastSuspicion <= 0)
                        {
                            currentState = AIState.Idle;
                            waitCounter = waitAtPoint;
                            timeSinceLastSuspicion = suspicionTime;
                            agent.isStopped = false;
                        }
                    }
                    else
                    {
                        agent.isStopped = false;
                        if (animator)
                        {
                            animator?.SetBool(IsWalking, true);
                        }

                        timeSinceLastSuspicion = suspicionTime;
                        agent.isStopped = (distToPlayer <= attackRange);
                        if (animator)
                        {
                            animator?.SetBool(IsAttacking, distToPlayer <= attackRange);
                        }

                        if (distToPlayer <= attackRange)
                        {
                            var lookDirection = player.transform.position - transform.position;
                            lookDirection.y = 0;
                            transform.rotation = Quaternion.LookRotation(lookDirection);
                        }
                    }

                    break;
            }
        }

        public IEnumerator Death()
        {
            agent.isStopped = true;
            Die();
            yield return new WaitForSeconds(2f);
            Destroy(gameObject);
        }

        void Die()
        {
            animator.SetBool(Death1, true);
        }
    }
}