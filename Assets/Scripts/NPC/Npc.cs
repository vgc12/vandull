using System;
using System.Collections;
using General;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPC
{
    using UnityEngine;
    using System.Collections;

    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class Npc : MonoBehaviour, IDamageable, IKillable
    {
        [SerializeField] private float health = 100;

        public float Health => health;

        protected StateMachine.StateMachine StateMachine;

        protected NavMeshAgent NavMeshAgent;

        protected RagdollController RagdollController;

        protected Animator Animator;

        protected bool CanWalk = true;

        [SerializeField] protected float minIdleTime = 2f;
        [SerializeField] protected float maxIdleTime = 5f;

        protected abstract void InitializeStateMachine();

        private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
        private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");

        private Collider[] _colliders;


        protected virtual void Awake()
        {
            StateMachine = new StateMachine.StateMachine();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            Animator = GetComponentInChildren<Animator>();
            RagdollController = GetComponent<RagdollController>();
            InitializeStateMachine();
        }

        public virtual void TakeDamage(float amount)
        {
            health -= amount;
            if (health <= 0)
            {
                Die();
            }
        }

        public virtual void Die()
        {
            RagdollController.EnableRagdoll(true);
            NavMeshAgent.enabled = false;
        }

        public void WalkToPoint(Vector3 point)
        {
            NavMeshAgent.SetDestination(point);
        }


        public bool MoveToRandomPositionAtDistance(float targetDistance, int maxAttempts)
        {
            if (!NavMeshAgent.isActiveAndEnabled) return false;
            Vector3 startPosition = transform.position;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Generate random direction
                Vector3 randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0; // Keep on same Y level


                // Calculate target position
                Vector3 targetPosition = startPosition + randomDirection * targetDistance;

                // Check if position is on NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
                {
                    // Verify the actual distance is close to desired
                    float actualDistance = Vector3.Distance(startPosition, hit.position);

                    if (Mathf.Abs(actualDistance - targetDistance) < 0.5f)
                    {
                        NavMeshAgent.SetDestination(hit.position);
                        return true;
                    }
                }
            }

            return false;
        }


        public void WalkToRandomPoint(float range)
        {
            if (!NavMeshAgent.isActiveAndEnabled) return;
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * range;
            randomDirection += transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, range, -1);
            NavMeshAgent.SetDestination(navHit.position);
        }

        public virtual void StopMoving()
        {
            if (!NavMeshAgent.isActiveAndEnabled) return;
            NavMeshAgent.SetDestination(transform.position);
        }

        public void IdleWaitBeforeMoving()
        {
            float waitTime = UnityEngine.Random.Range(minIdleTime, maxIdleTime);
            StartCoroutine(WaitBeforeMoving(waitTime));
        }

        private IEnumerator WaitBeforeMoving(float time)
        {
            CanWalk = false;
            yield return new WaitForSeconds(time);
            CanWalk = true;
        }


        protected virtual void Update()
        {
            StateMachine.Update();
        }

        protected virtual void FixedUpdate()
        {
            StateMachine.FixedUpdate();
        }


        public void HandleAnimation()
        {
            Vector3 velocity = NavMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity).normalized;
            float speed = velocity.magnitude;


            if (speed > 0.01f)
            {
                Animator.SetFloat(HorizontalMovement, localVelocity.x / 2);
                Animator.SetFloat(VerticalMovement, localVelocity.z / 2);
            }
            else
            {
                Animator.SetFloat(HorizontalMovement, 0);
                Animator.SetFloat(VerticalMovement, 0);
            }
        }
    }
}