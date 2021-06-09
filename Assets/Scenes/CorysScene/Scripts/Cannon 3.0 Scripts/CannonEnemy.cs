using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonEnemy : MonoBehaviour
{
    private Transform target;

    [Header("Attributes")]
    
    public float range = 15f;
    public float fireRate = 1f;
    private float fireCountdown = 0f;
   
    [Header("Fields")]

    public string playerTag = "Player";

    public Transform partToRotate;
    public float turnSpeed = 10f;

    public GameObject cannonBallPrefab;
    public Transform firePoint;
    


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
        
    }

    // Update is called once per frame
    void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestPlayer = null;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < shortestDistance)
            {
                shortestDistance = distanceToPlayer;
                nearestPlayer = player;
            }

            if (nearestPlayer != null && shortestDistance <= range)
            {
                target = nearestPlayer.transform;
            }

            else
            {
                target = null;
            }
        }
        
        
    }

    void Update()
    {
        if (target == null)
            return;
        
        //Locks Cannon to Player Target
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        //Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        Vector3 rotation = lookRotation.eulerAngles;
        partToRotate.rotation = Quaternion.Euler (-45f, rotation.y, -90f);


        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void Shoot()
    {
        GameObject cannonProjectileGO = (GameObject)Instantiate(cannonBallPrefab, firePoint.position, firePoint.rotation);
        CannonProjectile cannonProjectile = cannonProjectileGO.GetComponent<CannonProjectile>();

        if (cannonProjectile != null)
            cannonProjectile.Seek(target);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
