using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VFX / Enemy")]
public class VFXAssetEnemy : ScriptableObject
{
    /*
     Things needed for this object:

    Material of Enemy
    Shader of Enemy
    Amount of Objects
    Objects to Dissolve

    Spawn States

    Fade Value

    Fade Speed

    Trasform of enemy GEO group main

    VFX of spawn and death
     */

    #region Variables
    [Header("Materials", order = 0)]
    public Material[] materials;

    //Array to store all the materials that need the dissolve triggered
    Material[] material;

    //Fade parameter; 1.9f is completely dissolved; -13 is fully visable
    public float fade = 1.9f;

/*    [Header("Speed of Dissolve", order = 2)]
    public float speed = 10;
    public float spawnSpeedDivider = 2.2f;
    private float deathSpeedDivider = 2;

    [Header("Enemy VFX Transform", order = 3)]
    public GameObject spawnFrom;
    Vector3 deathFrom = new Vector3(0, 1, 0);*/

    [Header("Enemy VFX Prefabs", order = 4)]
    public GameObject spawnSand;
    public GameObject deathSand;

/*    [Header("Material", order = 5)]
    public Material enemyMaterial;*/
    #endregion

    private void Awake()
    {
        // Fill required values of the object

        //Set materials and set skinned meshes
        

    }

    #region Functions



    #endregion
}
