using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon_Controller : MonoBehaviour
{

    //Cannon Rotation
    public int speed;
    public float friction;
    public float lerpSpeed;
    float xDegrees;
    float yDegrees;
    Quaternion fromRotation;
    Quaternion toRoatation;
    new Camera camera;

    //Cannon Firing
    public GameObject cannonBall;
    Rigidbody cannonballRB;
    public Transform shotPos;
    public GameObject explosion;
    public float firePower;
    public int powerMultiplier = 100;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        firePower *= powerMultiplier;

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.tag == "Cannon")
            {
                if (Input.GetMouseButton(0))
                {
                    xDegrees -= Input.GetAxis("Mouse Y") * speed * friction;
                    yDegrees -= Input.GetAxis("Mouse X") * speed * friction;
                    fromRotation = transform.rotation;
                    toRoatation = Quaternion.Euler(xDegrees, yDegrees, 0);
                    transform.rotation = Quaternion.Lerp(fromRotation, toRoatation, Time.deltaTime * lerpSpeed);
                }
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            FireCannon();
        }
        
    }

  

    public void FireCannon()
    {
        //shotPos.rotation = transform.rotation;
        //firePower *= powerMultiplier;
        GameObject cannonBallCopy = Instantiate(cannonBall, shotPos.position, shotPos.rotation) as GameObject;
        cannonballRB = cannonBallCopy.GetComponent<Rigidbody>();
        cannonballRB.AddForce(transform.forward * firePower);
        Instantiate(explosion, shotPos.position, shotPos.rotation);

    }
}
