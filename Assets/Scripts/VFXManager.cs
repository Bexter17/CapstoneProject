using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    #region Variables 

    [Header("VFX Spawn Position", order = 0)]
    [Tooltip("Takes position of where this parent object is; this is where the effect will instantiate from.")]
    public GameObject bjornFullBody;
    public GameObject bjornHammerHitBox;
    public GameObject bjornFeet;
    public GameObject aboveBjorn;
    public GameObject bjornCOG;
    public GameObject hammerSmashSpawn;
/*    public GameObject rayCastFrom;*/
    public LayerMask ignoreRC;
    [Space(order = 1)]

    public SkinnedMeshRenderer skinnedMesh;
    public GameObject hammer;
    Material hammerMaterial; 

    //VFX Prefabs are contained here
    public VFXAssetPlayer playerVFXData;

    bool hammerGlowSmash = false;
    bool hammerGlowSpin = false;
    bool hammerGlowCharge = false;

    float glowValueSmash = 0;
    float glowValueSpin = 0;
    float glowValueCharge = 0;

    public bool tookDamage = false;
    public Vector3 collision = Vector3.zero;
    //Variables for Collider Detection


    /*    //Positions are stored in these variables and these are used for where the VFX will be spawned
        private Vector3 hammerPos;
        private Vector3 bjornFeetPos;
        private Vector3 aboveBjornPos;*/

    #endregion

    private void Start()
    {
        hammerMaterial = hammer.GetComponent<Renderer>().material;
    }

    #region Position Updates
    void Update()
    {
        /*        hammerPos = bjornHammerHitBox.transform.position;
                bjornFeetPos = bjornFeet.transform.position;
                aboveBjornPos = aboveBjorn.transform.position;*/

        //Vector3 closestToBjorn = bjornFullBody.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        //collisionDrawCheck(closestToBjorn, 0.1f);

        /*        var ray = new Ray(rayCastFrom.transform.position, rayCastFrom.transform.forward);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 100, ignoreRC))
                {
                    collision = hit.point;
                    Debug.DrawRay(rayCastFrom.transform.position, rayCastFrom.transform.forward * 10, Color.blue, 2f);
                    if (hit.transform.gameObject.CompareTag("Enemy"))
                    {
                        Debug.Log(hit.transform.name);
                    }

                }*/
        //if (hammerGlowSmash)
        //{
        //    HammerGlow("smash", true);
        //} 
        
        //if(hammerGlowSpin) 
        //{
        //    HammerGlow("spin", true);
        //}
        
        //if(hammerGlowCharge) 
        //{
        //    HammerGlow("charge", true);
        //}

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(collision, 0.2f);
    }

    #endregion

    /*    private void OnCollisionStay(Collision collision)
        {
            ContactPoint[] contacts = new ContactPoint[10];

            int numContacts = collision.GetContacts(contacts);
            for(int index = 0; index < numContacts; index++)
            {
                if(Vector3.Distance(contacts[index].point, bjornFullBody.transform.position) < .2f)
                {
                    collisionDrawCheck(contacts[index].point, 1f);
                    Debug.Log("COLLIDED WITH BJORN");
                }
            }

        }*/

    #region VFX Functions

    /*    void collisionDrawCheck(Vector3 point, float scale)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * scale;
            sphere.transform.position = point;
            sphere.transform.parent = transform.parent;
            sphere.GetComponent<Collider>().enabled = false;
            Destroy(sphere, 2f);
        }*/

    #region Bjorn VFX
    void HammerTrail() 
    {
        InstantiateVFX(playerVFXData.hammerTrailDefault, bjornHammerHitBox.transform);
    }

    void HammerTrailSmash()
    {
        InstantiateVFX(playerVFXData.hammerTrailSmash, bjornHammerHitBox.transform);
    }

    void AbilityTrail()
    {
        InstantiateVFX(playerVFXData.abilityTrail, bjornCOG.transform);
    }

    void TripleAttackFirst(int attackLayer)
    {
        GameObject tripleFirstClone = CheckForClone(playerVFXData.tripleAttackFirst, aboveBjorn.transform);

        if (attackLayer == 1)
        {
            //Enable Grow
            tripleFirstClone.transform.GetChild(0).gameObject.SetActive(true);
        } else if (attackLayer == 2)
        {
            //Hold Overhead
            tripleFirstClone.transform.GetChild(1).position = aboveBjorn.transform.position + new Vector3(0,1,0);
            tripleFirstClone.transform.GetChild(1).gameObject.SetActive(true);
        } else if (attackLayer == 3)
        {
/*            //Final Swing Down
            tripleFirstClone.transform.GetChild(2).position = bjornHammerHitBox.transform.position;
            tripleFirstClone.transform.GetChild(2).gameObject.SetActive(true);*/
        }
    }

    void TripleAttackSecond()
    {

    }

    void TripleAttackThird(int attackLayer)
    {
        InstantiateVFX(playerVFXData.tripleAttackThird, bjornCOG.transform);

        if (attackLayer == 1)
        {
            //Ring
            GameObject.Find(playerVFXData.tripleAttackThird.name + "(Clone)").transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    void HammerCharge()
    {
        hammerGlowCharge = true;
        SetSkinnedMeshAndPlay(playerVFXData.hammerCharge);
    }

    void Hammerspin()
    {
        hammerGlowSpin = true;
        InstantiateVFX(playerVFXData.hammerSpin, bjornHammerHitBox.transform);
    }

    void HammerSmash()
    {
        hammerGlowSmash = true;

        Vector3 positionOfVFX = new Vector3(hammerSmashSpawn.transform.position.x, hammerSmashSpawn.transform.position.y, hammerSmashSpawn.transform.position.z);
        Instantiate(playerVFXData.hammerSmash, positionOfVFX, Quaternion.identity);
    }

    void Throw()
    {

    }

    void BjornDamageTaken() {
        //Calculate where collision occurred in world space
        // Instatiate an object in world space at that location spawning the fluff VFX
    }

    #endregion

    # region Material VFX
    
/*    void HammerGlow(string ability, bool state)
    {
        float smashMax = 2;
        float spinMax = 8;
        float chargeMax = 5;
        bool glowActive = state;

        Debug.Log(glowActive);

        if (ability.Equals("smash") && glowActive)
        {

            glowValueSmash += Time.deltaTime * playerVFXData.glowSpeed;
            hammerMaterial.SetFloat("_smash", glowValueSmash);
            if (glowValueSmash >= smashMax)
            {
                HammerGlow("smash", false);
            }

        } else if (ability.Equals("smash") && !glowActive) {
 
            glowValueSmash -= Time.deltaTime * playerVFXData.glowSpeed;
            hammerMaterial.SetFloat("_smash", glowValueSmash);

            if (glowValueSmash <= 0)
            {
                glowValueSmash = 0;
                hammerGlowSmash = false;
            }

        } 

        if (ability.Equals("spin")) {
            

            if (glowValueSpin < spinMax)
            {
                glowValueSpin += Time.deltaTime * playerVFXData.glowSpeed;
                hammerMaterial.SetFloat("_spin", glowValueSpin);
            }
            else if (glowValueSpin >= spinMax)
            {
                glowValueSpin -= Time.deltaTime * playerVFXData.glowSpeed;
                hammerMaterial.SetFloat("_spin", glowValueSpin);

                if (glowValueSpin <= 0)
                {
                    glowValueSpin = 0;
                    hammerGlowSpin = false;
                }
            }

        }
        
        if(ability.Equals("charge")) {
            

            if (glowValueCharge < chargeMax)
            {
                glowValueCharge += Time.deltaTime * playerVFXData.glowSpeed;
                hammerMaterial.SetFloat("_charge", glowValueCharge);
            }
            else if (glowValueCharge >= chargeMax)
            {
                glowValueCharge -= Time.deltaTime * playerVFXData.glowSpeed;
                hammerMaterial.SetFloat("_charge", glowValueCharge);

                if (glowValueCharge <= 0)
                {
                    glowValueCharge = 0;
                    hammerGlowCharge = false;
                }
            }

        }

    }*/

    #endregion

#endregion

    #region VFX Handler Functions
    void VFXToScene(GameObject vfx, Transform location)
    {
        InstantiateVFX(vfx, location);
    }

    public void setDamageTrigger(bool d)
    {
        tookDamage = d;
    }

    void InstantiateVFX(GameObject vfxObject, Transform objectParented)
    {
        Instantiate(vfxObject, objectParented.position, Quaternion.identity, objectParented);
    }

    GameObject FindVFXInScene(string vfxName)
    {
        GameObject foundVFX = GameObject.Find(vfxName + "(Clone)");
        return foundVFX;
    }

    GameObject CheckForClone(GameObject originalVFX, Transform location)
    {
        if (FindVFXInScene(CloneName(originalVFX)) == null)
        {
            VFXToScene(originalVFX, location);
            return GameObject.Find(originalVFX.name + "(Clone)");
        }
        else
        {
            return null;
        }
    }

    string CloneName(GameObject oriPrefabName)
    {
        return oriPrefabName.name + "(Clone)";
    }
    #endregion

    #region Other Functions

    /* Function Name: DestroyVFX
    * Use: Finds every instance of VFX called in the scene, gets the longest playing VFX for that particular 
    * effect and kills that entire object after a set amount of time.
    * 
    * Input: Takes the tag of the vfx tag to be found and destroyed
    * Output: No Output
    */
    void DestroyVFX(float timeUntilDead)
    {
        GameObject[] vfxToDestroy = GameObject.FindGameObjectsWithTag("VFX_Bjorn");

        for (int i = 0; i < vfxToDestroy.Length; i++)
        {
            Destroy(vfxToDestroy[i], timeUntilDead);
        }
    }

    /* Function Name: GatherPSInScene
     * Use: Gathers every particle system of every child of the vfxList so they can be referenced
     * 
     * Input: Takes an array of GameObjects of all VFX objects instantiated in the scene at that instance
     * Output: Outputs an array of every Particle System for earch of the VFX Game Objects
     */
    ParticleSystem[] GatherPSInScene(GameObject[] vfxList)
    {
        ParticleSystem[] vfxPS = new ParticleSystem[vfxList.Length];

        for (int i = 0; i < vfxList.Length; i++)
        {
            vfxPS[i] = vfxList[i].GetComponent<ParticleSystem>();
        }

        return vfxPS;
    }

    void SetSkinnedMeshAndPlay(GameObject vfxToSkin)
    {
        GameObject vfx = vfxToSkin;
        ParticleSystem[] ps = new ParticleSystem[5];
        int index = 0;

        ps = vfx.GetComponentsInChildren<ParticleSystem>();

        while(index < ps.Length && ps[index] != null)
        {
            var psShape = ps[index].shape;
            psShape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;

            psShape.skinnedMeshRenderer = skinnedMesh;

            index++;
        }

        vfx = Instantiate(vfxToSkin, skinnedMesh.transform.position, skinnedMesh.transform.rotation, skinnedMesh.transform);
    }
    #endregion

}
