using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GenericIK : MonoBehaviour
{
    #region Components

    //Creates a charactercontoller variable named "controller"
    CharacterController controller;

    //creates a script accessable variable of the Animator component
    Animator animator;

    Rigidbody rb;

    [SerializeField] GameObject playerModel;

    #region Body Parts

    [Header("Body Parts")]
    public CentralJointClass CentralJoints;
    public LeftJointClass LeftJoints;
    public RightJointClass RightJoints;


    #region Central Joints 
    [System.Serializable]
    public class CentralJointClass
    {
        [Header("Central Body Parts")]

        [SerializeField] public Transform ctrl_root;

        [SerializeField] public Transform t_root;

        [SerializeField] public Transform t_Pelvis;

        [SerializeField] public Transform t_Spine1;

        [SerializeField] public Transform t_Spine2;

        [SerializeField] public Transform t_Spine3;

        [SerializeField] public Transform t_Spine4;

        [SerializeField] public Transform t_CSpine;

        [SerializeField] public Transform t_Neck1;

        [SerializeField] public Transform t_Neck2;

        [SerializeField] public Transform t_Neck3;

        [SerializeField] public Transform t_Head;

        [SerializeField] public Transform t_Tail1;
        
        [SerializeField] public Transform t_Tail2;

        [SerializeField] public Transform t_Tail3;
    }

    #endregion

    #region Left Joints

    [System.Serializable]
    public class LeftJointClass
    {
        [Header("Left Body Parts")]

        [SerializeField] public Transform t_L_Clavicle;

        [SerializeField] public Transform t_L_Shoulder;

        [SerializeField] public Transform t_L_Elbow;

        [SerializeField] public Transform t_L_Wrist;

        [SerializeField] public Transform t_L_Hip;

        //[SerializeField] Transform t_L_Leg;

        [SerializeField] public Transform t_L_Knee1;

        [SerializeField] public Transform t_L_Knee2;

        [SerializeField] public Transform t_L_Foot;
    }
    #endregion

    #region Right Joints

    [System.Serializable]
    public class RightJointClass
    {
        [Header("Right Body Parts")]

        [SerializeField] public Transform t_R_Clavicle;

        [SerializeField] public Transform t_R_Shoulder;

        [SerializeField] public Transform t_R_Elbow;

        [SerializeField] public Transform t_R_Wrist;

        [SerializeField] public Transform t_R_Hip;

        //[SerializeField] Transform t_R_Leg;

        [SerializeField] public Transform t_R_Knee1;

        [SerializeField] public Transform t_R_Knee2;

        [SerializeField] public Transform t_R_Foot;
    }
    #endregion

    #endregion

    #endregion

    #region Variables

    [Header("Debug Toggle")]
    [SerializeField] bool IKDebug;

    [Header("Debug Toggle")]
    [SerializeField] int IKSearchLength;

    float anim_speed;

    [SerializeField] float rotationSpeed;

    #endregion

    #region Humanoid Variables

    //[SerializeField] private bool showSolverDebug;

    //[SerializeField] private bool enableFeetIK = true;

    //[SerializeField] private Transform rightFoot;

    //[SerializeField] private Transform leftFoot;

    //private Vector3 rightFootPos, leftFootPos, rightFootIKPos, leftFootIKPos;

    //private Quaternion leftFootIKRot, rightFootIKRot;

    //private float lastPelvisPosY, lastRightFootPosY, lastLeftFootPosY;

    //[Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;

    //[Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;

    ////[SerializeField] private LayerMask environmentLayer;

    //[SerializeField] private float pelvisOffset = 0f;

    //[Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.2f;

    //[Range(0, 1)] [SerializeField] private float feetToIKPosSpeed = 0.5f;

    //[SerializeField] private string leftFootVariableName = "Left Foot Curve";

    //[SerializeField] private string rightFootVariableName = "Right Foot Curve";

    //public bool useProIKFeatures = true;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        animator = this.transform.GetComponent<Animator>();

        rb = this.transform.GetComponent<Rigidbody>();

        if (rotationSpeed == 0)
            rotationSpeed = 8;
    }

    // Update is called once per frame
    void Update()
    {
        if (!rb)
            return;

        float speed = rb.velocity.magnitude;

        anim_speed = Mathf.Lerp(anim_speed, speed / 5, Time.deltaTime * 5);

        //animator.SetFloat("Speed", anim_speed);

        #region Debug Log

        if (IKDebug)
        {
            Debug.Log("Generic IK: speed = " + speed);

            Debug.Log("Generic IK: anim_speed = " + anim_speed);
        }

        #endregion
    }

    private void LateUpdate()
    {
        updateCharacterBones();
    }

    #region Generic IK Methods

    void updateCharacterBones()
    {
        RaycastHit LHip_Hit;
        RaycastHit RHip_Hit;

        if(Physics.Raycast(LeftJoints.t_L_Hip.position, Vector3.down, out LHip_Hit, IKSearchLength) && Physics.Raycast(RightJoints.t_R_Hip.position, Vector3.down, out RHip_Hit, IKSearchLength))
        {
            Debug.DrawLine(LeftJoints.t_L_Hip.position, LHip_Hit.point, Color.red);
            Debug.DrawLine(RightJoints.t_R_Hip.position, RHip_Hit.point, Color.red);

            float l = Vector3.Distance(LHip_Hit.point, RHip_Hit.point);
            float h = RHip_Hit.point.y - LHip_Hit.point.y;
            float angle = Mathf.Asin(h / l) * 180 / Mathf.PI;

            if (IKDebug)
                Debug.Log("IK: angle = " + angle);

            if (angle > Mathf.Epsilon)
            {
                angle = Mathf.Clamp(angle, -20, 20);
                Vector3 currentEulerAngle = new Vector3(0, 0, angle) * Time.deltaTime * rotationSpeed;
                //CentralJoints.t_root.localEulerAngles = new Vector3(0, 0, angle);
                //CentralJoints.ctrl_root.localEulerAngles = new Vector3(0, 0, angle);
                CentralJoints.t_root.localEulerAngles = currentEulerAngle;
                CentralJoints.ctrl_root.localEulerAngles = currentEulerAngle;
                //CentralJoints.t_root.Rotate(newRot * Time.deltaTime * rotationSpeed);
                //CentralJoints.ctrl_root.Rotate(newRot * Time.deltaTime * rotationSpeed);
                //CentralJoints.t_root.localRotation = Quaternion.Slerp(this.transform.rotation, newRot, Time.deltaTime);
                //CentralJoints.ctrl_root.localRotation = Quaternion.Slerp(this.transform.rotation, newRot, Time.deltaTime);
                //this.transform.localEulerAngles = new Vector3(this.transform.rotation.x, this.transform.rotation.y, angle);
                //Quaternion newRotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y, angle, this.transform.rotation.w);
                //playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, newRotation, Time.deltaTime);
            }
        }
    }

    #endregion

    #region Old Feet Grounding

    //private void FixedUpdate()
    //{
    //    #region IK System Check

    //    if (enableFeetIK == false)
    //    {
    //        return;
    //    }

    //    if (animator == null)
    //    {
    //        return;
    //    }

    //    #endregion

    //    #region Adjust Feet

    //    adjustFeetTarget(ref rightFootPos, rightFoot);

    //    adjustFeetTarget(ref leftFootPos, leftFoot);

    //    feetPositionSolver(rightFootPos, ref rightFootIKPos, ref rightFootIKRot); //Handles the solver for the right foot

    //    feetPositionSolver(leftFootPos, ref leftFootIKPos, ref leftFootIKRot); //Handles the solver for the left foot

    //    #endregion
    //}

    //private void OnAnimatorIK(int layerIndex)
    //{
    //    if (enableFeetIK == false)
    //    {
    //        return;
    //    }

    //    if (animator == null)
    //    {
    //        return;
    //    }

    //    movePelvisHeight();

    //    //right foot ik position & rotation -- utilize pro features here
    //    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

    //    if (useProIKFeatures)
    //    {
    //        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(rightFootVariableName));
    //    }

    //    moveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPos, rightFootIKRot, ref lastRightFootPosY);

    //    //left foot ik position & rotation -- utilize pro features here
    //    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

    //    if (useProIKFeatures)
    //    {
    //        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(leftFootVariableName));
    //    }

    //    moveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPos, leftFootIKRot, ref lastLeftFootPosY);
    //}

    #endregion

    #region Old Feet Grounding Methods

    //private void moveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
    //{
    //    Vector3 targetIKPosition = animator.GetIKPosition(foot);

    //    if (positionIKHolder != Vector3.zero)
    //    {
    //        targetIKPosition = transform.InverseTransformPoint(targetIKPosition);

    //        positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

    //        float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPosSpeed);

    //        targetIKPosition.y += yVariable;

    //        lastFootPositionY = yVariable;

    //        targetIKPosition = transform.TransformPoint(targetIKPosition);

    //        animator.SetIKPosition(foot, targetIKPosition);
    //    }

    //    animator.SetIKPosition(foot, targetIKPosition);
    //}

    //private void movePelvisHeight()
    //{
    //    if (rightFootIKPos == Vector3.zero || leftFootIKPos == Vector3.zero || lastPelvisPosY == 0)
    //    {
    //        lastPelvisPosY = animator.bodyPosition.y;
    //        return;
    //    }

    //    float lOffsetPos = leftFootIKPos.y - transform.position.y;

    //    float rOffsetPos = rightFootIKPos.y - transform.position.y;

    //    float totalOffset = (lOffsetPos < rOffsetPos) ? lOffsetPos : rOffsetPos;

    //    Vector3 newPelvisPos = animator.bodyPosition + Vector3.up * totalOffset;

    //    newPelvisPos.y = Mathf.Lerp(lastPelvisPosY, newPelvisPos.y, pelvisUpAndDownSpeed);

    //    animator.bodyPosition = newPelvisPos;

    //    lastPelvisPosY = animator.bodyPosition.y;
    //}

    //private void feetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
    //{
    //    //raycast handling section
    //    RaycastHit feetOutHit;

    //    if (showSolverDebug)
    //        Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.blue);

    //    if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast))
    //    {
    //        feetIKPositions = fromSkyPosition;

    //        feetIKPositions.y = feetOutHit.point.y + pelvisOffset;

    //        feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

    //        return;
    //    }

    //    feetIKPositions = Vector3.zero;
    //}

    //private void adjustFeetTarget(ref Vector3 feetPositions, Transform foot)
    //{
    //    feetPositions = foot.position;

    //    feetPositions.y = transform.position.y + heightFromGroundRaycast;
    //}

    #endregion

}