using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class RangedEnemy : MonoBehaviour
{
    //June 9, 2021
    //Most Recent Change: Clowns raycast now targets player so that regardless of elevation of player/clown enemy will still attack(though attacks need to be rotated towards player)
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
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    private Vector3 playerDirection;
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

    //TakeDamage Cooldown
    private float damageInterval = 0;
    private bool canTakeDamage = true;
    private float takeDamageCooldown = .8f;

    public new AudioSource audio;

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
        if(projectilePrefab == null)
        {
            agent.enabled = false;
        }
        eAnim = gameObject.GetComponent<Animator>();

        hpBar = transform.Find("Clown/Canvas/Enemy HP Bar").GetComponent<Image>();

        Player = GameObject.FindGameObjectWithTag("Player");
        target = Player.transform;
        cm = Player.GetComponent<CharacterMechanics>();

        audio = this.GetComponent<AudioSource>();

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

        hpBar.enabled = false;
        //abilityCollider.enabled = false;
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
        //Attempt at making hp bar rotate with player
        //if (hpBar.isActiveAndEnabled)
        //{
        //    Vector3 hpLook = target.position;

        //    hpLook.x = 0;
        //    Vector3 hpPos = hpBar.transform.position;
        //    hpPos.x = 0;
        //    //hpBar.transform.LookAt(hpLook);
        //    hpBar.transform.TransformDirection(hpLook);
        //    hpBar.transform.rotation = Quaternion.Euler(hpLook - hpPos);

        //    //targetPosition = Player.transform.position;
        //    //Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
        //    //float str = rotationSpeed * Time.deltaTime;
        //    //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
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
                //if (cm.isUsingAbilities)
                //{
                //    //Debug.Log("Big Bear using abilities");
                //    abilityCollider.enabled = true;
                //}
                //else
                //{
                //    abilityCollider.enabled = false;
                //}
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
                    if(projectilePrefab != null)
                    {
                        eAnim.SetBool("PlayerSpotted", true);
                    }
                    
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
            else if(projectilePrefab != null)
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

            targetPosition = Player.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            float str = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
            //transform.LookAt(targetPosition);
        }
        //AgentDisabled
        else
        {
            if (projectilePrefab == null)
            {
                rb.constraints = RigidbodyConstraints.FreezePosition;
            }
            Vector3 targetPosition = Player.transform.position;
            targetPosition.y = transform.position.y;
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            float str = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
        }
        #endregion
    }
    #region health/death

    private void FixedUpdate()
    {
        if (canTakeDamage == false)
        {
            damageInterval += Time.deltaTime;
            if (damageInterval >= takeDamageCooldown)
            {
                damageInterval = 0;
                canTakeDamage = true;
            }
            else
            {
                canTakeDamage = false;
            }
        }
    }
    public void takeDamage(int dmg)
    {
        if (canTakeDamage)
        {
            if (!hpBar.isActiveAndEnabled)
            {
                hpBar.enabled = true;
            }
            EnemyOnHitSFX();
            Debug.Log("Clown Damage Taken: " + dmg + "Current TIme:" + Time.time);
            agent.isStopped = true;
            hp -= dmg;
            canTakeDamage = false;
        }
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
        //rb.isKinematic = false;
        //agent.enabled = false;
        //rb.AddForce(-transform.forward * knockDistanceModifier);
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

            if (rb && !isStationary)
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

                takeDamage(10);

                if (rb && !isStationary)
                {

                    
                    Vector3 direction = transform.position - collision.transform.position;
                    direction.y = 0;

                    rb.AddForce(direction.normalized * basicKnockbackForce, ForceMode.Impulse);
                }
                if (rb)
                {
                    basicStaggerCounter++;
                    if (basicStaggerCounter >= attackStaggerCount)
                    {
                        basicStaggerCounter = 0;
                        AnimStagger();
                    }
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

                if (rb && !isStationary)
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

            if (rb && !isStationary)
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
        //Raycast to ensure clown will attack even if its not at equal height as player
        RaycastHit Hit;
        Vector3 rayLook = transform.position;
        rayLook.y += 2.5f;
        Vector3 rayTarget = target.position;

        playerDirection = rayTarget - rayLook;
        Physics.Raycast(rayLook, playerDirection, out Hit);
        Debug.DrawRay(rayLook, playerDirection, Color.red);

        if (Time.time - attackTimer > 1.0f)
        {
            if(Hit.collider.tag == "Player")
            {
                eAnim.SetTrigger("Attack");
                attackTimer = Time.time;
            }
        }

        //RaycastHit sphereHit;
        //Physics.SphereCast(transform.position, 10, transform.TransformDirection(Vector3.forward), out sphereHit);
        //if(sphereHit.collider.tag == "Player")
        //{
        //    playerInRange = true;
        //}
        //else
        //{
        //    playerInRange = false;
        //}

        //Collider[] hitColliders = Physics.OverlapSphere(transform.position, 15);
        //Collider[] hitcoll = Physics.OverlapBox(transform.position, transform.localScale * 5, Quaternion.identity, playerMask);
        //foreach (var hitCollider in hitColliders)
        //{
        //    if (hitCollider.CompareTag("Player"))
        //    {
        //        playerInRange = true;
        //        break;
        //    }
        //    else
        //    {
        //        playerInRange = false;
        //    }
        //}

        //Physics.SphereCast()
    }

    // Used for enemy animations and patrolling between waypoints
    private void OnTriggerEnter(Collider other)
    {
        //Debug.LogWarning("Clown Ontrigger");
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

            if (rb && !isStationary)
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
                //Big Bear: Basic Damage
                takeDamage(10);
                if (rb)
                {
                    basicStaggerCounter++;
                    if (basicStaggerCounter >= attackStaggerCount)
                    {
                        basicStaggerCounter = 0;
                        AnimStagger();
                    }
                }
            }
            

            if (cm.isSpinning)
            {
                Debug.Log("Hit by whirlwind");
                //Big Bear: Whirlwind Damage
                takeDamage(25);
            }
        }
    }

    //Gets called by Throw animation on frame 12
    public void SpawnProjectile()
    {
        if (projectilePrefab)
        {

            GameObject projectile = Instantiate(projectilePrefab, new Vector3(projectileSpawnPoint.position.x, projectileSpawnPoint.position.y + .75f, projectileSpawnPoint.position.z), projectileSpawnPoint.rotation);
            Vector3 projPlayerDirection = new Vector3(playerDirection.x, playerDirection.y += 2.5f, playerDirection.z);

            projectile.GetComponent<Rigidbody>().AddForce(projPlayerDirection.normalized * projectilePrefab.GetComponent<Projectiles>().projectileSpeed, ForceMode.Impulse);
        }
    }

    public void EnemyOnHitSFX(float volume = 1f)
    {
        //Debug.Log("Carny Hit Sound");
        audio.PlayOneShot((AudioClip)Resources.Load("Body Hit"));
        audio.volume = volume;
    }
    #endregion
}
