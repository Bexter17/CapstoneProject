using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

//Combat Script
public class EnemyCarny : MonoBehaviour
{
    //June 2, 2021
    //Most Recent Change: Only deal damage when weapon collides with player / 0 rotation during attack animations


    //remaining Distance instead of vector3.distance
    #region Variables
    //TODO removed most of serializefields to a minimum

    //Essentials
    private Rigidbody rb;
    private Transform target;
    public NavMeshAgent agent;
    private Animator eAnim;
    private CombatManager CombatScript;
    [SerializeField] private CarnyWeapon WeaponScript;
    private CharacterMechanics cm;
    //used to track the player for giveDamage function
    private GameObject Player;
    private bool enemyDissolveIn = false;

    //HP
    [Header("Essentials")]
    [SerializeField] private int hp;
    private int maxHP;
    private Image hpBar;
    public bool death = false;
    private bool randNumGenerated = false;
    [SerializeField] private GameObject ragdoll;


    /*
     Craig's Modifications
    */
    [SerializeField] bool combatDebug = true;

    [SerializeField] float basicKnockbackForce;

    [SerializeField] float smashKnockbackForce;

    [SerializeField] float dashKnockbackForce;

    [SerializeField] float whirlKnockbackForce;

    [SerializeField] float rangeKnockbackForce;


    //STATES
    enum EnemyState { Start, Patrol, Chase, Attack, Stun, lockChase };
    EnemyState myEnemy;
    
    [SerializeField] private float knockDistanceModifier = 400;
    private float knockDuration = 0.3f;
    private float knockPause = 1;

    // The distance the enemy will begin to chase player
    private float punchRange = 3;
    public float chaseRange = 10;
    private float checkStackRange = 10;

    // Amount of damage done by enemy to player
    public int dmgDealt = 2;
    private bool ableToDamage = false;
    //bool isPatrolling = false;
    bool getCalled = false;
    [SerializeField] private int numberOfAttacks;

    private GameObject waypoint1;
    private GameObject waypoint2;
    private GameObject[] potentialWaypoints;
    [Header("Patrol (Only needs waypoints if Advanced)")]
    public GameObject[] waypoints;
    public bool advancedPatrol;
    private int patrolNumber = 0;
    private int patrolIterator = 1;

    // How fast enemy moves
    [Header("Speed")]
    [SerializeField] private float enemyMovement;
    // multiplies by walk enemyMovement speed for chasing speed
    private int enemyRunMultiplier = 2;
    private float rotationSpeed = 6;
    private bool isRotating = true;
    private float startingMovementSpeed;

    private int basicStaggerCounter = 0;
    //1 / attackStaggerCount of basic attacks stagger
    private int attackStaggerCount = 4;



    //Used to stun the enemy, wait a few seconds for AOE then return to normal function. 
    //Had to use IEnumerator because I couldn't get yield return new WaitForSeconds() to work anywhere else in script. Would like to 
    //plug this into an official state as you can see I have inerted Stun into the enemyState and began the other necessary fields.
    //TODO does enumerator need to be here?
    IEnumerator Stun()
    {
        Debug.Log("ENEMY HAS BEEN STUNNED FOR 6 SECONDS BY HAMMER SMASH");
        //Stop enemy movement
        //Stop enemy attack
        agent.isStopped = true;
        //Damage enemy
        hp -= 2;
        //takeDamage(1); - Had to change the hp variable alone because takeDamage was applying knockback.
        //> There's a BUG where this method seems to stack effect and instant kill or send enemy flying.
        //TODO rename functions to be more descriptive
        AgentStop();
        //WAIT for AOE
        yield return new WaitForSeconds(6);
        //Return enemy movement and attack to normal
        AgentStart();
        enemyMovement = 5;
    }

    // Start is called before the first frame update
    #endregion

    #region EncircleVariables
    private GameObject[] circlePoints;
    private int encircleNum = -1;
    private float circleDist = .5f;
    private bool onStack = false;
    public EnemyStack stackTracker;
    //Used to ensure onStack only activates once
    private bool hasStacked = true;
    
    #endregion

    void Awake()
    {       
        #region Components
        //ESSENTIALS
        rb = GetComponent<Rigidbody>();
        hpBar = transform.Find("Carny/Canvas/Enemy HP Bar").GetComponent<Image>();
        cm = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();
        agent = GetComponent<NavMeshAgent>();
        eAnim = gameObject.GetComponent<Animator>();

        Player = GameObject.FindGameObjectWithTag("Player");
        target = GameObject.FindGameObjectWithTag("Player").transform;
        CombatScript = GameObject.Find("GameManager").GetComponent<CombatManager>();
        #endregion
        #region SetWaypoints
        //STATES
        if (advancedPatrol)
        {
            for(int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                {
                    Debug.Log(gameObject + "Missing PatrolNode:" + i);
                }
            }
            for(int i = 0; i < waypoints.Length - 1; i++)
            {
                Debug.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position, Color.cyan, 999);
            }
        }
        else
        {
            potentialWaypoints = GameObject.FindGameObjectsWithTag("WayPoint1");
            waypoint1 = potentialWaypoints[0];
            for (int i = 0; i < potentialWaypoints.Length; i++)
            {
                if (Vector3.Distance(transform.position, potentialWaypoints[i].transform.position) < Vector3.Distance(transform.position, waypoint1.transform.position))
                {
                    waypoint1 = potentialWaypoints[i];
                }
            }
            potentialWaypoints = GameObject.FindGameObjectsWithTag("WayPoint2");
            waypoint2 = potentialWaypoints[0];
            for (int i = 0; i < potentialWaypoints.Length; i++)
            {
                if (Vector3.Distance(transform.position, potentialWaypoints[i].transform.position) < Vector3.Distance(transform.position, waypoint2.transform.position))
                {
                    waypoint2 = potentialWaypoints[i];
                }
            }
            //waypoint1 = GameObject.FindGameObjectWithTag("WayPoint1");
            //waypoint2 = GameObject.FindGameObjectWithTag("WayPoint2");
        }

        #endregion
        #region default values
        //sets maxHP to beginning hp in order to get the correct fill amount for hpbar
        int maxHP = hp;

        myEnemy = EnemyState.Start;
        rb.isKinematic = true;

        // Default values
        if (enemyMovement <= 0)
        {
            enemyMovement = 3f;
        }
        if (punchRange <= 0)
        {
            punchRange = 2;
        }
        if (chaseRange <= 0)
        {
            chaseRange = 5f;
        }
        if (dmgDealt <= 0)
        {
            dmgDealt = 2;
        }
        if (enemyRunMultiplier <= 0)
        {
            enemyRunMultiplier = 4;
        }

        if (basicKnockbackForce <= 0)
            basicKnockbackForce = 3;

        if (smashKnockbackForce <= 0)
            smashKnockbackForce = 5;

        if (dashKnockbackForce <= 0)
            dashKnockbackForce = 4;

        if (whirlKnockbackForce <= 0)
            whirlKnockbackForce = 4;

        if (rangeKnockbackForce <= 0)
            rangeKnockbackForce = 2;

        startingMovementSpeed = enemyMovement;

        Patrol();
        #endregion
        #region stackTracker
        stackTracker = GameObject.Find("Enemy Stack Tracker").GetComponent<EnemyStack>();
        //if(GameObject.FindGameObjectWithTag("Enemy Slot 1") != null)
        //{
        //    circlePoints[0] = GameObject.FindGameObjectWithTag("Enemy Slot 1");
        //    circlePoints[1] = GameObject.FindGameObjectWithTag("Enemy Slot 2");
        //    circlePoints[2] = GameObject.FindGameObjectWithTag("Enemy Slot 3");
        //    circlePoints[3] = GameObject.FindGameObjectWithTag("Enemy Slot 4");
        //}

        // circle points 4 different points around the player where the enemies will go to attack
        //circlePoints = new Vector3[4];

        
        #endregion
    }
    // Update is called once per frame
    void Update()
    {
        //Used for testing enemy death
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
        if (agent.enabled && !death)
        {
            Vector3 targetPosition = agent.destination;
            targetPosition.y = transform.position.y;

            if (onStack && !ableToDamage)
            {
                agent.SetDestination(target.transform.position);
            }
            //Only activates once
            if (onStack && hasStacked)
            {
                agent.isStopped = false;
                hasStacked = false;
            }
            NavMeshHit hit;
            if (Vector3.Distance(target.position, gameObject.transform.position) < checkStackRange && !agent.Raycast(target.position, out hit))
            {
                //Debug.Log("Enemy To Stack");
                if (!onStack)
                {
                    int stackNum = stackTracker.AddStack(gameObject);
                    if (stackNum == 5)
                    {
                        onStack = false;
                    }
                    else
                    {
                        encircleNum = stackNum;
                        onStack = true;
                    }
                    agent.isStopped = true;
                }

            }
            else if (myEnemy == EnemyState.lockChase)
            {
                editChase();
                Debug.Log("1111111111111111111111111111111111111");
            }
            else if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange && !agent.Raycast(target.position, out hit))
            {
                Chase();
                agent.isStopped = false;
                myEnemy = EnemyState.Chase;
                if(Vector3.Distance(target.position, gameObject.transform.position) < chaseRange - (enemyMovement*enemyRunMultiplier*0.5))
                {
                    getCalled = false;
                }
            }
            else if (myEnemy != EnemyState.Patrol && !getCalled)
            {
                Patrol();
                agent.isStopped = false;
                myEnemy = EnemyState.Patrol;
            }
            if (isRotating)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
                float str = rotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
            }

            if (myEnemy == EnemyState.Patrol)
            {
                Patrol();
                agent.speed = startingMovementSpeed;
            }
            else if (myEnemy == EnemyState.Chase)
            {
                Chase();
                agent.speed = enemyRunMultiplier * startingMovementSpeed;
                //Debug.Log("Run");
            }
            else if(myEnemy == EnemyState.Attack)
            {
                //Generates random number once per attack from 1-3 to randomly choose 1 of 3 attacks
                //Will generate number once on the main tree and can do so again after each attack
                if(eAnim.GetCurrentAnimatorStateInfo(0).IsName("Chase Tree") && !randNumGenerated)
                {
                    //1-3
                    //Set to 1, 4 once third animation is added
                    eAnim.SetInteger("randAttk", Random.Range(1, numberOfAttacks+1));
                    randNumGenerated = true;
                    isRotating = true;
                }
                else if (eAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack 1") || eAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack 2") || eAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack 3"))
                {
                    randNumGenerated = false;
                    isRotating = false;
                }
                eAnim.SetBool("isAttacking", true);
                eAnim.SetTrigger("Attack");
            }
            if(myEnemy != EnemyState.Attack)
            {
                eAnim.SetBool("isAttacking", false);
            }
        }
        #endregion
    }
    //COLLISIONS
    #region Collisions
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && agent.isOnNavMesh)
        {
            rb.velocity = Vector3.zero;
            agent.isStopped = true;
            AgentStop();
        }
        else if (collision.gameObject.CompareTag("HammerSmashAOE"))
        {
            #region Debug Log

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Hit by Hammersmash!");
                Debug.Log(this.transform.name + " Damage Applied!");
            }

            #endregion
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
        else if (collision.gameObject.CompareTag("WhirlwindAOE"))
        {
            //Deals small knockback from takeDamage function
            #region Debug Log
            Debug.Log("Enemy has been hit by whirlwind!");
            #endregion
            takeDamage(20);
        }
        else if (collision.gameObject.CompareTag("Attack Zone"))
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
            //Debug.Log("Enemy has been hit by hammer smash!");
            #endregion
            //Give enemies back their speed after hammer smash AOE
            //movementSpeed = 5;
        }
    }
    private void OnTriggerEnter(Collider other)
    {

        // During patrol alternate going between Waypoint1 and Waypoint2
        // On collision with waypoint sets other as destination
        // Patrolling now works regardless of what order waypoints are in

        if (myEnemy == EnemyState.Patrol)
        {
            if (advancedPatrol)
            {
                if (other.gameObject.transform == waypoints[patrolNumber].transform)
                {
                    if (patrolNumber == waypoints.Length - 1 && patrolIterator == 1)
                    {
                        patrolIterator = -1;
                    }
                    else if (patrolNumber == 0 && patrolIterator == -1)
                    {
                        patrolIterator = 1;
                    }
                    patrolNumber += patrolIterator;
                    agent.SetDestination(waypoints[patrolNumber].transform.position);
                }
            }
            else
            {
                if (other.gameObject.transform == waypoint1.transform)
                {
                    agent.SetDestination(waypoint2.transform.position);
                }
                else if (other.gameObject.transform == waypoint2.transform)
                {
                    agent.SetDestination(waypoint1.transform.position);
                }

            }

        }
        else
        {

            if (other.CompareTag("Player"))
            {
                eAnim.SetBool("cancelAttk", false);
                if (myEnemy == EnemyState.Chase && onStack)
                {
                    myEnemy = EnemyState.Attack;
                }
                //Debug.LogWarning("Enemy Start Collision With Player");
                rb.isKinematic = false;
                ableToDamage = true;
                agent.isStopped = true;
                eAnim.SetFloat("Speed", 0);
            }
        }

        //TODO + parameter to take damage to edit knockback
        //So that ranged attack doesn't knockback as much as melee
        if (other.gameObject.tag == "PlayerRanged")
        {

            #region Debug Log

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Hit by Ranged!");

                Debug.Log(this.transform.name + " Damage Applied!");
            }

            #endregion

            takeDamage(8);

            if (rb)
            {
                Vector3 direction = transform.position - other.transform.position;
                direction.y = 0;

                rb.AddForce(direction.normalized * rangeKnockbackForce, ForceMode.Impulse);
            }

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Knocked Back!");
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && myEnemy != EnemyState.Attack)
        {
            eAnim.SetBool("cancelAttk", false);
            if (myEnemy == EnemyState.Chase/* && onStack*/)
            {
                myEnemy = EnemyState.Attack;
            }
            //Debug.LogWarning("Enemy Start Collision With Player");
            rb.isKinematic = false;
            ableToDamage = true;
            agent.isStopped = true;
            eAnim.SetFloat("Speed", 0);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            eAnim.SetBool("cancelAttk", true);
            ableToDamage = false;
            agent.isStopped = false;
            eAnim.SetFloat("Speed", 2);
            myEnemy = EnemyState.Chase;
        }
    }
    #endregion

    #region damage
    public void takeDamage(int dmg)
    {
        //Debug.Log(dmg + "Damage Taken");
        agent.isStopped = true;
        hp -= dmg;
        if (hp <= 0 && !death)
        {
            //transform.GetComponent<CapsuleCollider>().enabled = false;
            Vector3 ragdollPos = transform.position;
            ragdollPos.y -= 3.25f;
            GameObject temp = Instantiate(ragdoll, ragdollPos, transform.rotation);
            //temp.transform.localScale = new Vector3(3.25f, 3.25f, 3.25f);
            RagdollPhysics ragdollPhysics = temp.GetComponent<RagdollPhysics>();
            Vector3 currentVelocity = rb.velocity;
            ragdollPhysics.GetVelocity(currentVelocity);

            Destroy(gameObject);
            death = true;
            stackTracker.RemoveStack(gameObject);
            agent.speed = 0;
            Debug.Log("Enemy has been killed");
            enemyMovement = 0;
            rotationSpeed = 0;

            // so that enemy doesn't move after dying
            eAnim.SetBool("IsDying", true);
            eAnim.SetTrigger("IsDead");

            //Destroy(gameObject);   Destroy object is called in EnemyAI1 when the death animation is played
        }
        hpBar.fillAmount = (float)(hp * 0.016f);

        //KNOCKBACK
        rb.isKinematic = false;

        //Invokes once enemy is no longer being knocked back and pauses movement
        Invoke("AgentStop", knockDuration);
    }
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
    #endregion
    //TODO rename to be more descriptive
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
        enemyMovement = startingMovementSpeed;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }
  
    #region init States
    public void Chase()
    {
        myEnemy = EnemyState.Chase;
        eAnim.SetFloat("Speed", 2);
        // Sets player as destination
        agent.SetDestination(target.transform.position);
        agent.isStopped = false;


    }
    //using this funciton to set a chase destination to spawned enemy
    public void editChase()
    {
        if(agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if(Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        agent.isStopped = false;
        myEnemy = EnemyState.lockChase;
        agent.SetDestination(Player.transform.position);

    }
    public int getstatus()
    {
        if (myEnemy == EnemyState.Patrol)
        {
            return 1;
        }
        else if (myEnemy == EnemyState.Chase)
        {
            return 2;
        }
        else if (myEnemy == EnemyState.lockChase)
        {
            return 3;
        }
        else if (myEnemy == EnemyState.Attack)
        {
            return 4;
        }
        return 0;
    }
    public void Called()
    {
        getCalled = true;
        Chase();
    }



    // Turns around and continues
    private void Patrol()
    {
        //Debug.Log("PATROL");
        // At the beginning of patrolling sets first patrol destination
        if (advancedPatrol)
        {
            if (myEnemy != EnemyState.Patrol)
            {
                myEnemy = EnemyState.Patrol;
                agent.SetDestination(waypoints[0].transform.position);
            }
        }
        else
        {
            if (myEnemy != EnemyState.Patrol)
            {
                myEnemy = EnemyState.Patrol;
                agent.SetDestination(waypoint1.transform.position);
            }
        }

    }

    // Used for enemy animations and patrolling between waypoints
    #endregion

    private void AnimStagger()
    {
        eAnim.SetTrigger("staggerBack");
    }

    public void changeStackrange(float i)
    {
        checkStackRange = i;
    }

    public void Attack()
    {
        if (WeaponScript.GetWeaponContact())
        {
            CombatScript.GivePlayerDamage(this.transform, dmgDealt);
        }
    }

    public void DoubleAttack()
    {
        if (WeaponScript.GetWeaponContact())
        {
            CombatScript.GivePlayerDamage(this.transform, dmgDealt / 2);
        }
    }
}
