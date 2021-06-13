using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectiles : MonoBehaviour
{
    public float projectileSpeed;
    public float lifeTime;
    public int dmgDealt;
    private GameObject Player;
    //private CharacterMechanics cm;
    CombatManager CombatScript;
    CharacterMechanics CharacterMechanicsScript;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        //cm = Player.GetComponent<CharacterMechanics>();
        CombatScript = GameObject.Find("GameManager").GetComponent<CombatManager>();
        CharacterMechanicsScript = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();

        if (lifeTime <= 0)
            lifeTime = 3.0f;

        if (dmgDealt <= 0)
            dmgDealt = 3;

        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            CharacterMechanicsScript.takeDamage(this.transform, dmgDealt);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
