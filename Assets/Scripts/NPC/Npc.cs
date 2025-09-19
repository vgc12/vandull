using System;
using System.Collections;
using General;
using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public abstract class Npc : MonoBehaviour, IDamageable, IKillable
    {
        [SerializeField] private float health = 100;

        public float Health => health;
    
        protected StateMachine.StateMachine StateMachine;

        protected NavMeshAgent NavMeshAgent;

        protected Animator Animator;

        protected bool CanWalk = true;
        
        [SerializeField] protected float minIdleTime = 2f;
        [SerializeField] protected float maxIdleTime = 5f;
        
        protected abstract void InitializeStateMachine();
        
        private static readonly int RunBlendTree = Animator.StringToHash("RunBlendTree");
        private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
        private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
        
        
        protected virtual void Awake()
        {
            StateMachine = new StateMachine.StateMachine();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
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
            Destroy(gameObject);
        }
        
        public void WalkToPoint(Vector3 point)
        {
            NavMeshAgent.SetDestination(point);
        }
        
        public void WalkToRandomPoint(float range)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * range;
            randomDirection += transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, range, -1);
            NavMeshAgent.SetDestination(navHit.position);
        }

        public virtual void StopMoving()
        {
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

        public void CheckRemainingDistance()
        {
            if(NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
            {
                NavMeshAgent.isStopped = true;
                 CanWalk = false;
            }
        }
        
        public void HandleAnimation()
        {
            Vector3 velocity = NavMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = velocity.magnitude;

            
        
            if (speed > 0.01f)
            {
                Animator.SetFloat(HorizontalMovement, 1/localVelocity.x);
                Animator.SetFloat(VerticalMovement, 1/localVelocity.z);
                Animator.SetBool(RunBlendTree, true);
            }
            else
            {
                Animator.SetFloat(HorizontalMovement, 0);
                Animator.SetFloat(VerticalMovement, 0);
            }
        }
    }
}