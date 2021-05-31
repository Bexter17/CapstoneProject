using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon_Projectile : MonoBehaviour
{
    public Rigidbody cannonBall;
    public GameObject cursor;
    public Transform targetPoint;
    public LayerMask layer;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        LaunchProjectile();
    }

    void LaunchProjectile()
    {
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(camRay, out hit, 100f, layer))
        {
            cursor.SetActive(true);
            cursor.transform.position = hit.point + Vector3.up * 0.1f;

            Vector3 Vo = CalculateVelocity(hit.point, targetPoint.position, 1f);

            transform.rotation = Quaternion.LookRotation(Vo);

            if (Input.GetMouseButtonDown(0))
            {
                Rigidbody obj = Instantiate(cannonBall, targetPoint.position, Quaternion.identity);
                obj.velocity = Vo;
            }

            else
            {
                cursor.SetActive(false);
            }
        }
    }
    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        // Distance of X and Y
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;

        // Float for distance

        float Sy = distance.y;
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }
}
