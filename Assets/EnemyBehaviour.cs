using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public GameObject Target;// IN THIS CASE, Player is target
    public NavMeshAgent Agent;

    [Header("MovementDetails")]
    [Range(0, 100)]
    public float MoveForce = 10f;
    [Range(0, 100)]
    public float MaxSpeed = 5f;
    [Range(0, 360)]
    public float AngleOffset;
    [Range(0, 100)]
    public float TurningSpeed;

    [Header("AnimationDetails")]
    public Animator EnemyAnimator;

    [Header("Combat")]
    [Range(0, 100)]
    public float AttackRange = 2.0f;  

    public Rigidbody Rigidbody;

    public enum EnemyState
    {
        Moving,
        Attack,
        Die
    }
    [Header("EnemyStates")]
    public EnemyState ES;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Target = GameObject.FindGameObjectWithTag("Player");

        Agent.updatePosition = false;
        Agent.updateRotation = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (ES)
        {
            // Find and Chase
            case EnemyState.Moving:
                if (Target == null) return;
                EnemyAnimator.SetBool("Run",true);

                Agent.SetDestination(Target.transform.position);
                Agent.nextPosition = Rigidbody.position;

                // Directions
                Vector3 dir = Agent.desiredVelocity.normalized;

                //Speed
                Rigidbody.AddForce(dir * MoveForce, ForceMode.Acceleration);
                //MaxSpeedRestriction
                if (Rigidbody.linearVelocity.magnitude > MaxSpeed)
                {
                    Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * MaxSpeed;
                }
                //Facing To Player
                Vector3 direction = Target.transform.position - transform.position;
                direction.y = 0f;

                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                angle += 180f;
                Quaternion targetRotation = Quaternion.Euler(0f, angle, 0f);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.unscaledDeltaTime * TurningSpeed);

                //EnterAttackMode
                float atkDist = (Target.transform.position - transform.position).sqrMagnitude;
                if (atkDist <= AttackRange)
                {
                    EnterAttack();
                }
            break;
                //Attack and Stop
            case EnemyState.Attack:
                //End Run Animation
                EnemyAnimator.SetBool("Run", false);
                //Facing To Player
                Vector3 direction2 = Target.transform.position - transform.position;
                direction2.y = 0f;

                float angle2 = Mathf.Atan2(direction2.x, direction2.z) * Mathf.Rad2Deg;
                angle2 += 180f;
                Quaternion targetRotation2 = Quaternion.Euler(0f, angle2, 0f);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation2, Time.unscaledDeltaTime * TurningSpeed);

                // Engage;

                //QuitAttackMode
                float qtaDist = (Target.transform.position - transform.position).sqrMagnitude;
                if (qtaDist > AttackRange)
                {
                    QuitAttack();
                }
                break;
                //╪дак
            case EnemyState.Die:
                 Agent.enabled = false;
            break;
        }
       
    }
    public void EnterAttack()
    {
        if (ES == EnemyState.Attack) return;
        ES = EnemyState.Attack;
        //Break
        Rigidbody.linearVelocity = Vector3.zero;

     //   if (_attackRoutine == null)
      //      _attackRoutine = StartCoroutine(AttackLoop());
    }
    public void QuitAttack()
    {
        ES = EnemyState.Moving;
    }
}
