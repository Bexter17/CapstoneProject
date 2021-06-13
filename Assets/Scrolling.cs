using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrolling : MonoBehaviour
{
    public Vector3 speed = new Vector3(0, 0.5f, 0);
    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Time.deltaTime);
    }




}
