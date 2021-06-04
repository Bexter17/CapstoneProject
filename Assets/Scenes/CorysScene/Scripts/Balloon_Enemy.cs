using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon_Enemy : MonoBehaviour
{
    public ParticleSystem balloonPopParticles;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("PlayerRanged"))
        {
            Destroy();
        }

        if (collision.collider.CompareTag("WhirlwindAOE"))
        {
            Destroy();
        }
        
        if (collision.collider.CompareTag("HammerSmashAOE"))
        {
            Destroy();
        }
    }

    public void Destroy()
    {
       Instantiate(balloonPopParticles, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
