using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VFX / Player")]
public class VFXAssetPlayer : ScriptableObject
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

    [Header("Bjorn VFX Prefabs", order = 4)]
    public GameObject hammerTrailDefault;
    public GameObject hammerTrailSmash;
    public GameObject abilityTrail;
    public GameObject tripleAttackFirst;
    public GameObject tripleAttackSecond;
    public GameObject tripleAttackThird;
    public GameObject hammerCharge;
    public GameObject hammerSpin;
    public GameObject hammerSmash;
    public GameObject throwAxe;
    public GameObject bjornDeath;
    public GameObject takeDamage;
    public int glowSpeed = 2;
    #endregion

    private void Awake()
    {     

    }

    #region Functions



    #endregion
}
