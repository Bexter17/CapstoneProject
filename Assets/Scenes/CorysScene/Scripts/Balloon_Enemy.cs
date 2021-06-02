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
    }

    public void Destroy()
    {
       Instantiate(balloonPopParticles, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
