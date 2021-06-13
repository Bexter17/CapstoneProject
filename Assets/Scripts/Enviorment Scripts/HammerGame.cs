using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerGame : MonoBehaviour
{

    /*When player uses their hammer smash ability on the Test Your Strength Game, the box will fly up and hit the bell!
    This will open a new area for the player to explore.
    Target will be the bell, this script is attached to the moving part of the game at bottom. (Square) For now.*/
    public GameObject target;
    public bool isHit = false;

    /*
     * Put the wall opening from hammer game code below here 
     */
    public bool wallOpen = false;
    public GameObject gate;
    public bool wallDestroy = false; 

    AudioManager am;
    public new AudioSource audio;
    public Rigidbody rb;
    public GameObject Axe;
    

    // Start is called before the first frame update
    void Start()
    {
        am = this.transform.GetComponent<AudioManager>();
        audio = this.GetComponent<AudioSource>();
        rb = Axe.GetComponent<Rigidbody>();

        rb.detectCollisions = false;
        rb.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isHit == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 10f * Time.deltaTime);
        }

        if (wallOpen == true && wallDestroy == false)
        {
            #region Debug Log
            Debug.Log("Gate is being opened to the DARK CARNIVAL!");
                #endregion
            Destroy(gate, 8);

            var sandWall = gate.GetComponent<ParticleSystem>().main;

            sandWall.loop = false;
            wallDestroy = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        //isHit = true;
       // Debug.Log("Trigger has been hit!");
        //Check if player has used the hammer smash to play game
        if (other.gameObject.tag == "HammerSmashAOE") {
            Debug.Log("Box is heading to bell");
            #region Debug Log
            Debug.Log("Hit Hammer Game with hammer smashAOE!");
            #endregion
            isHit = true;
        }

        //Check if player has won the game and opened the new area
        if (other.gameObject.tag == "Bell") {
            #region Debug Log
            Debug.Log("Player has won Hammer Game!");
            #endregion
            audio.Play();
            wallOpen = true;
            rb.detectCollisions = true;
            rb.isKinematic = false;
        }
    }
}
