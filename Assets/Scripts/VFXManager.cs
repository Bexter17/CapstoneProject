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
    public GameObject VFX_Storage;
    /*    public GameObject rayCastFrom;*/
    public LayerMask ignoreRC;
    [Space(order = 1)]

    public SkinnedMeshRenderer skinnedMesh;
    public GameObject hammer;
    Material hammerMaterial;

    //VFX Prefabs are contained here
    public VFXAssetPlayer playerVFXData;

    bool hammerGlowIn = false;
    bool hammerGlowOut = false;

    float glowIntensity = 0;
    float timeUntilDeath = 4;

    public bool tookDamage = false;
    public Vector3 collision = Vector3.zero;

    List<GameObject> vfxAlive = new List<GameObject>();

    #endregion

    private void Start()
    {
        hammerMaterial = hammer.GetComponent<Renderer>().material;
    }

    #region Position Updates
    void Update()
    {
        #region Hammer Glow

        if (hammerGlowIn)
        {
            HammerGlowing();
        }
        else if (hammerGlowOut)
        {
            if (hammerMaterial.GetFloat("_smash") == 1)
            {
                HammerDeGlow("smash");
            }

            if (hammerMaterial.GetFloat("_spin") == 1)
            {
                HammerDeGlow("spin");
            }

            if (hammerMaterial.GetFloat("_charge") == 1)
            {
                HammerDeGlow("charge");
            }
        }

        #endregion

    }

    #endregion


    #region VFX Functions

    #region Bjorn VFX
    void HammerTrail(int attackLayer)
    {
        //GameObject hammerTrailClone = CheckForClone(playerVFXData.hammerTrailDefault, bjornFullBody.transform);

        GameObject hammerTrailClone = InstantiateVFX(playerVFXData.hammerTrailDefault, bjornFullBody.transform);

        hammerTrailClone.transform.position = new Vector3(bjornFullBody.transform.position.x, bjornFullBody.transform.position.y + 1f, bjornFullBody.transform.position.z + 0.1f);

        if (attackLayer == 1)
        {
            hammerTrailClone.transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (attackLayer == 2)
        {
            hammerTrailClone.transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (attackLayer == 3)
        {
            hammerTrailClone.transform.GetChild(2).gameObject.SetActive(true);
        }

        Destroy(hammerTrailClone, timeUntilDeath);
    }

    void HammerTrailSmash()
    {
        InstantiateVFX(playerVFXData.hammerTrailSmash, bjornHammerHitBox.transform, timeUntilDeath);
    }

    void TripleAttackFirst(int attackLayer)
    {
        GameObject tripleFirstClone = InstantiateVFX(playerVFXData.tripleAttackFirst, bjornFullBody.transform);

        if (attackLayer == 1)
        {
            //Enable Grow
            tripleFirstClone.transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (attackLayer == 2)
        {
            //Hold Overhead
            tripleFirstClone.transform.GetChild(1).position = bjornFullBody.transform.position + new Vector3(0, 4, 0);
            tripleFirstClone.transform.GetChild(1).gameObject.SetActive(true);
        }

        Destroy(tripleFirstClone, timeUntilDeath);
    }

    void TripleAttackSecond()
    {
        InstantiateVFX(playerVFXData.tripleAttackSecond, bjornHammerHitBox.transform, timeUntilDeath);
    }

    void TripleAttackThird(int attackLayer)
    {
        InstantiateVFX(playerVFXData.tripleAttackThird, bjornCOG.transform, timeUntilDeath);

        if (attackLayer == 1)
        {
            //Ring
            GameObject.Find(playerVFXData.tripleAttackThird.name + "(Clone)").transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    void HammerCharge()
    {
        GlowReset();
        hammerMaterial.SetFloat("_charge", 1);
        hammerGlowIn = true;
        //SkinnedHammerCharge();
        GameObject clone = InstantiateVFX(playerVFXData.hammerCharge, bjornCOG.transform);

        clone.transform.position += new Vector3(0, 1.5f, -0.5f);
        Destroy(clone, timeUntilDeath);
    }

    void Hammerspin()
    {
        GlowReset();
        hammerMaterial.SetFloat("_spin", 1);
        hammerGlowIn = true;
        InstantiateVFX(playerVFXData.hammerSpin, bjornHammerHitBox.transform, timeUntilDeath);
        //clone.transform.position += new Vector3(0, 0.7f, 0);
    }

    void HammerSmash()
    {
        GlowReset();
        hammerMaterial.SetFloat("_smash", 1);
        hammerGlowIn = true;

        Vector3 positionOfVFX = new Vector3(hammerSmashSpawn.transform.position.x, hammerSmashSpawn.transform.position.y, hammerSmashSpawn.transform.position.z);
        GameObject clone = Instantiate(playerVFXData.hammerSmash, positionOfVFX, Quaternion.identity);

        Destroy(clone, timeUntilDeath);
    }

    public void BjornDamageTaken()
    {
        // This needs to get called in the character mechanics script in the takeDamage function
        GameObject clone =  Instantiate(playerVFXData.takeDamage, bjornFullBody.transform.position + new Vector3(0,0.5f,0), bjornFullBody.transform.rotation, VFX_Storage.transform);
        Destroy(clone, timeUntilDeath);
    }

    #endregion

    #region Material VFX

    void HammerGlow(string ability)
    {
        if (hammerGlowIn)
        {
            HammerGlowing();
        }

        if (hammerGlowOut)
        {
            HammerDeGlow(ability);
        }
        Debug.Log(glowIntensity);
    }

    void HammerGlowing()
    {
        glowIntensity += Time.deltaTime * playerVFXData.glowSpeed;

        if (glowIntensity >= 1)
        {
            glowIntensity = 1;
            hammerGlowIn = false;
            hammerGlowOut = true;
        }

        hammerMaterial.SetFloat("_intensity", glowIntensity);
    }

    void HammerDeGlow(string abilityCalled)
    {
        glowIntensity -= Time.deltaTime * playerVFXData.glowSpeed;

        if (glowIntensity <= 0)
        {
            glowIntensity = 0;
            hammerMaterial.SetFloat("_" + abilityCalled, 0);
            hammerGlowOut = false;
        }

        hammerMaterial.SetFloat("_intensity", glowIntensity);
    }

    void GlowReset()
    {
        glowIntensity = 0;
        hammerMaterial.SetFloat("_intensity", glowIntensity);
    }

    #endregion

    #endregion

    #region VFX Handler Functions
    void VFXToScene(GameObject vfx, Transform location)
    {
        InstantiateVFX(vfx, location);
    }

    public void SetDamageTrigger(bool d)
    {
        tookDamage = d;
    }

    void InstantiateVFX(GameObject vfxObject, Transform objectParented, float time)
    {
        GameObject clone = Instantiate(vfxObject, objectParented.position, objectParented.rotation, objectParented);
        Destroy(clone, time);
    }

    GameObject InstantiateVFX(GameObject vfxObject, Transform objectParented)
    {
        return Instantiate(vfxObject, objectParented.position, objectParented.rotation, objectParented);
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
            InstantiateVFX(originalVFX, location, 4);
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

    void VFXToKill()
    {
        List<GameObject> copy;

        copy = new List<GameObject>(vfxAlive);

        foreach (GameObject vfx in vfxAlive)
        {
            vfxAlive.Remove(vfx);
        }

        foreach (GameObject vfx in copy)
        {
            Destroy(vfx, 2);
        }
       
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
    void VFXToPool()
    {
        GameObject[] vfxToDestroy = GameObject.FindGameObjectsWithTag("VFX_Bjorn");

        for (int i = 0; i < vfxToDestroy.Length; i++)
        {
            vfxAlive.Add(vfxToDestroy[i]);
        }

        VFXToKill();
    }

    /* Function Name: GatherPSInScene
     * Use: Gathers every particle system of every child of the vfxList so they can be referenced
     * 
     * Input: Takes an array of GameObjects of all VFX objects instantiated in the scene at that instance
     * Output: Outputs an array of every Particle System for earch of the VFX Game Objects
     */
    /*    ParticleSystem[] GatherPSInScene(GameObject[] vfxList)
    {
        ParticleSystem[] vfxPS = new ParticleSystem[vfxList.Length];

        for (int i = 0; i < vfxList.Length; i++)
        {
            vfxPS[i] = vfxList[i].GetComponent<ParticleSystem>();
        }

        return vfxPS;
    }

    void SkinnedHammerCharge()
    {
        GameObject vfx = playerVFXData.hammerCharge;
        ParticleSystem[] ps;
        int index = 0;

        GameObject vfxToSkin = playerVFXData.hammerCharge.transform.GetChild(0).gameObject;

        ps = vfxToSkin.GetComponentsInChildren<ParticleSystem>();

        do
        {
            var psShape = ps[index].shape;
            psShape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;

            psShape.skinnedMeshRenderer = skinnedMesh;

            index++;
        } while (index < ps.Length && ps[index] != null);

        Instantiate(vfx, bjornCOG.transform.position, bjornCOG.transform.rotation, bjornCOG.transform);
    }

    void SkinnedHammerSpin()
    {
        GameObject vfx = playerVFXData.hammerSpin;
        ParticleSystem[] ps;
        int index = 0;

        ps = vfx.GetComponentsInChildren<ParticleSystem>();

        do
        {
            var psShape = ps[index].shape;
            psShape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;

            //psShape.skinnedMeshRenderer = skinnedMesh;
            psShape.skinnedMeshRenderer = hammer.GetComponent<SkinnedMeshRenderer>();

            index++;
        } while (index < ps.Length && ps[index] != null);

        Instantiate(vfx, hammer.transform.position, hammer.transform.rotation, hammer.transform);
    }*/
    #endregion

}
