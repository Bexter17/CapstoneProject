using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class RangedEnemy : MonoBehaviour
{
    //June 2, 2021
    //Most Recent Change: added trigger box that is only enabled during player abilities for basic attack spam damage bug / hammersmashaoe doing damage
    #region Variables

    [Header("Essentials")]
    //HP
    public int hp = 5;
    private int maxHP;
    private Image hpBar;
    [SerializeField] private BoxCollider abilityCollider;

    public Rigidbody rb;
    public Transform target;
    [SerializeField] private GameObject ragdoll;
    [SerializeField] Rigidbody projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float attackTimer = 0;
    private bool enemyDissolveIn = false;

    [Header("Knockback")]
    [SerializeField] float knockDistanceModifier;
    [SerializeField] float knockDuration;
    [SerializeField] float knockPause;
    [SerializeField] float basicKnockbackForce;
    [SerializeField] float smashKnockbackForce;
    [SerializeField] float dashKnockbackForce;
    [SerializeField] float whirlKnockbackForce;
    [SerializeField] float rangeKnockbackForce;

    [Header("Debugs")]
    [SerializeField] bool stateDebug;
    [SerializeField] bool combatDebug;
    [SerializeField] bool knockbackDebug;

    NavMeshAgent agent;

    //Death
    public bool death = false;

    CharacterMechanics cm;

    //used to track the player for giveDamage function 
    private GameObject Player;
    enum EnemyState { Start, Patrol, Attack, Chase};
    EnemyState myEnemyClown;
    // The player that the enemy will chase
    //public Vector3 initialPos;
    //bool isInitPos = true;

    private int basicStaggerCounter = 0;
    //1 / attackStaggerCount of basic attacks stagger
    private int attackStaggerCount = 4;

    IEnumerator Stun()
    {
        Debug.Log("ENEMY HAS BEEN STUNNED FOR 6 SECONDS BY HAMMER SMASH");
        //Stop enemy movement
        enemyMovement = 0;
        //Stop enemy attack
        AgentStop();
        //Damage Enemy - Had to change hp variable instead of takeDamage because it applies knockback when we want a stun effect
        hp -= 2;
        //takeDamage(1); 
        //-> There's a BUG where this method seems to stack effect and instant kill or send enemy flying.
        //WAIT for AOE
        yield return new WaitForSeconds(6);
        //Return enemy movement and attack to normal
        AgentStart();
        enemyMovement = 5;
    }

    //Animations
    Animator eAnim;
    [Header("Speed")]
    // How fast enemy moves
    [SerializeField] float enemyMovement;
    // multiplies by walk enemyMovement speed for chasing speed
    [SerializeField] int enemyRunMultiplier;
    public float rotationSpeed;

    [Header("Ranges")]
    // The distance the enemy will begin to chase player
    public float chaseRange;
    public float attackRange;

    [Header("Patrol (only need waypoints if !isStationary)")]
    [SerializeField] bool isStationary;
    public Transform waypoint1;
    public Transform waypoint2;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region Get Components
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        eAnim = gameObject.GetComponent<Animator>();
        hpBar = transform.Find("Clown/Canvas/Enemy HP Bar").GetComponent<Image>();

        Player = GameObject.FindGameObjectWithTag("Player");
        target = Player.transform;
        cm = Player.GetComponent<CharacterMechanics>();
        #endregion

        #region default values
        //sets maxHP to beginning hp in order to get the correct fill amount for hpbar
        int maxHP = hp;
        myEnemyClown = EnemyState.Start;
        rb.isKinematic = true;

        // Default values
        if (enemyMovement <= 0)
        {
            enemyMovement = 3f;
        }
        if (chaseRange <= 0)
        {
            chaseRange = 5f;
        }
        if (attackRange <= 0)
        {
            attackRange = 3f;
        }
        if (enemyRunMultiplier <= 0)
        {
            enemyRunMultiplier = 4;
        }
        if (!isStationary)
        {
            Patrol();
        }


        #endregion
        if (isStationary)
        {
            eAnim.SetBool("Stationary", true);
        }
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Enemy State:" + myEnemyClown);
        //if (Input.GetKeyDown("t"))
        //{
        //    Debug.Log("Enemy has lost 1 hp");
        //    takeDamage(1);
        //}

        if (!enemyDissolveIn && !death)
        {
            gameObject.GetComponent<VFXenemies>().spawnIn = true;
            enemyDissolveIn = true;
        }

        #region AI States
        if (agent.enabled)
        {

            if(myEnemyClown == EnemyState.Chase || myEnemyClown == EnemyState.Attack)
            {
                //To ensure spam basic attack damage bug isn't happening / hammersmashaoe works
                if (cm.isUsingAbilities)
                {
                    //Debug.Log("Big Bear using abilities");
                    abilityCollider.enabled = true;
                }
                else
                {
                    abilityCollider.enabled = false;
                }
            }
            Vector3 targetPosition = agent.destination;
            targetPosition.y = transform.position.y;

            if (!isStationary)
            {
                if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange)
                {
                    myEnemyClown = EnemyState.Chase;
                    Chase();
                }
                // If enemy within attackrange stop moving and attack
                // If enemy within chaserange chase player
                // else go back to patrol route
                if (myEnemyClown == EnemyState.Patrol)
                {
                    Patrol();
                    agent.isStopped = false;
                }
                else
                {
                    eAnim.SetBool("PlayerSpotted", true);
                    //if (Vector3.Distance(target.position, gameObject.transform.position) < attackRange)
                    //{
                    //    myEnemyClown = EnemyState.Chase;
                    //    Chase();
                    //    //Debug.LogWarning("");

                        //}
/*                    else */if (Vector3.Distance(target.position, gameObject.transform.position) <= attackRange)
                    {
                        eAnim.SetBool("Chase", false);
                        agent.isStopped = true;
                        myEnemyClown = EnemyState.Attack;
                        Attack();
                    }
                }
            }
            //IsStationary
            else
            {
                rb.isKinematic = true;
                if (Vector3.Distance(Player.transform.position, gameObject.transform.position) <= attackRange)
                {
                    eAnim.SetBool("Chase", false);
                    agent.isStopped = true;
                    //Sets Destination to player so that the enemy will turn towards player
                    //Will not move is agent.isStopped = true
                    //agent.SetDestination(Player.transform.position);
                    targetPosition = Player.transform.position;
                    myEnemyClown = EnemyState.Attack;
                    Attack();
                }
            }
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            float str = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
            //transform.LookAt(targetPosition);
        }
        #endregion
    }
    #region health/death
    public void takeDamage(int dmg)
    {
        //Debug.Log(dmg + "Damage Taken");
        agent.isStopped = true;
        hp -= dmg;
        if (hp <= 0 && !death)
        {
            transform.GetComponent<CapsuleCollider>().enabled = false;
            Vector3 ragdollPos = transform.position;
            ragdollPos.y -= 1.5f;
            GameObject temp = Instantiate(ragdoll, ragdollPos, transform.rotation);
            temp.transform.localScale = new Vector3(3.25f, 3.25f, 3.25f);
            RagdollPhysics ragdollPhysics = temp.GetComponent<RagdollPhysics>();
            Vector3 currentVelocity = rb.velocity;
            ragdollPhysics.GetVelocity(currentVelocity);
            Destroy(gameObject);
            death = true;
            //Debug.Log("Enemy has been killed");
            //agent.isStopped = true;
            //eAnim.SetTrigger("Death");
        }
        hpBar.fillAmount = (float)(hp * 0.02);

        //KNOCKBACK
        // Gets the difference between enemy and player position
        // To knockback enemy away from player
        rb.isKinematic = false;
        //agent.enabled = false;
        rb.AddForce(-transform.forward * knockDistanceModifier);
        //rb.AddForce(transform.up * knockHeightModifier);

        Debug.Log("Knockback");
        //Invokes once enemy is no longer being knocked back and pauses movement
        Invoke("AgentStop", knockDuration);
    }
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
    #endregion

    #region stun/knockback
    private void AgentStop()
    {
        //Enemy briefly pauses after being knocked back
        //Where it's velocity is 0
        Invoke("AgentStart", knockPause);
    }
    private void AgentStart()
    {
        rb.isKinematic = true;
        //Enemy continues moving
        //agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector3.zero;
            agent.isStopped = true;
            AgentStop();
        }
        else if (collision.gameObject.CompareTag("HammerSmashAOE"))
        {
            #region Debug Log
            Debug.Log("Ranged enemy has been hit by hammer smash!");
            #endregion
            
            rb.velocity = Vector3.zero;
            //Stop attacking
            takeDamage(35);
            StartCoroutine(Stun());

            if (rb)
            {
                AnimStagger();
                Vector3 direction = transform.position - collision.transform.position;
                direction.y = 0;

                rb.AddForce(direction.normalized * smashKnockbackForce, ForceMode.Impulse);
            }

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Knocked Back!");
            }
        }
        if(collision.gameObject.CompareTag("WhirlwindAOE"))
        {
            takeDamage(20);
        }
        if(collision.gameObject.CompareTag("Attack Zone"))
        {
            takeDamage(3);
        }
        if (collision.gameObject.CompareTag("Hammer"))
        {
            if (cm.isAttacking)
            {
                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Hit by Basic Attack!");
                    Debug.Log(this.transform.name + " Damage Applied!");
                }

                takeDamage(5);

                if (rb)
                {
                    basicStaggerCounter++;
                    if(basicStaggerCounter >= attackStaggerCount)
                    {
                        basicStaggerCounter = 0;
                        AnimStagger();
                    }
                    
                    Vector3 direction = transform.position - collision.transform.position;
                    direction.y = 0;

                    rb.AddForce(direction.normalized * basicKnockbackForce, ForceMode.Impulse);
                }

                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Knocked Back!");
                }
            }

            if (cm.isSpinning)
            {
                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Hit by Whirlwind!");
                    Debug.Log(this.transform.name + " Damage Applied!");
                }

                takeDamage(4);

                if (rb)
                {
                    AnimStagger();
                    Vector3 direction = transform.position - collision.transform.position;
                    direction.y = 0;

                    rb.AddForce(direction.normalized * whirlKnockbackForce, ForceMode.Impulse);
                }

                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Knocked Back!");
                }
            }
        }

        if (collision.gameObject.CompareTag("Dash Collider"))
        {
            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Hit by Dash!");
                Debug.Log(this.transform.name + " Damage Applied!");
            }

            takeDamage(7);

            if (rb)
            {
                AnimStagger();
                Vector3 direction = transform.position - collision.transform.position;
                direction.y = 0;

                rb.AddForce(direction.normalized * dashKnockbackForce, ForceMode.Impulse);
            }

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Knocked Back!");
            }
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("HammerSmashAOE"))
        {
            #region Debug Log
            //Debug.Log("Ranged enemy has regained it's speed!");
            #endregion
            //Give enemies back their speed after hammer smash AOE
            //enemyMovement = 5;
        }
    }

    private void AnimStagger()
    {
        eAnim.SetTrigger("staggerBack");
    }

   
    #endregion
    #region States Chase, Patrol, Attack (Ranged)
    public void Chase()
    {
        agent.isStopped = false;
        eAnim.SetBool("Chase", true);
        // Sets player as destination
        //agent.SetDestination(target.transform.position);
        agent.SetDestination(Player.transform.position);
    }

    private void Patrol()
    {
        // At the beginning of patrolling sets first patrol destination
        if (myEnemyClown != EnemyState.Patrol)
        {
            myEnemyClown = EnemyState.Patrol;
            agent.SetDestination(waypoint1.position);
        }
    }
    //Ranged Attack 
    private void Attack()
    {
        //Two raycasts to ensure clown will attack even if its not at equal height as player
        RaycastHit highHit;
        RaycastHit lowHit;
        Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.TransformDirection(Vector3.forward), out lowHit);
        Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), transform.TransformDirection(Vector3.forward), out highHit);

        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), transform.TransformDirection(Vector3.forward) * 20);
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.TransformDirection(Vector3.forward) * 20);
        if (Time.time - attackTimer > 1.0f)
        {
            if(lowHit.collider.tag == "Player" || highHit.collider.tag == "Player")
            {
                eAnim.SetTrigger("Attack");
                attackTimer = Time.time;
            }
        }
    }

    // Used for enemy animations and patrolling between waypoints
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Clown Ontrigger");
        // During patrol alternate going between Waypoint1 and Waypoint2
        // On collision with waypoint sets other as destination
        if (myEnemyClown == EnemyState.Patrol)
        {
            if (other.CompareTag("WayPoint1"))
            {
                //Debug.Log("Waypoint");
                agent.SetDestination(waypoint2.position);
            }
            else if (other.CompareTag("WayPoint2"))
            {
                agent.SetDestination(waypoint1.position);
            }
        }
        if (other.gameObject.tag == "PlayerRanged")
        {
            Debug.Log("Hit with Ranged");
            takeDamage(8);
        }
        if (other.gameObject.CompareTag("HammerSmashAOE"))
        {
            #region Debug Log
            Debug.Log("Ranged enemy has been hit by hammer smash!");
            #endregion

            rb.velocity = Vector3.zero;
            //Stop attacking
            takeDamage(35);
            StartCoroutine(Stun());

            if (rb)
            {
                AnimStagger();
                Vector3 direction = transform.position - other.transform.position;
                direction.y = 0;

                rb.AddForce(direction.normalized * smashKnockbackForce, ForceMode.Impulse);
            }

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Knocked Back!");
            }
        }
        if (other.CompareTag("Hammer2"))
        {
            if (cm.isAttacking)
            {
                takeDamage(2);
            }
            

            if (cm.isSpinning)
            {
                Debug.Log("Hit by whirlwind");
                takeDamage(4);
            }
        }
    }

    //Gets called by Throw animation on frame 12
    public void SpawnProjectile()
    {
        if (projectilePrefab)
        {
            Rigidbody rb = Instantiate(projectilePrefab,
                projectileSpawnPoint.position,
                projectileSpawnPoint.rotation) as Rigidbody;

            rb.AddForce(transform.forward * projectilePrefab.GetComponent<Projectiles>().projectileSpeed, ForceMode.Impulse);
        }
    }
    #endregion
}
