//Copyright A1un (C) 2019 Aim IK
//calculate the Ik pos and rot of hands ,head and body when looking at the target

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(IkManager))]
public class AimIK : MonoBehaviour, IhandIkSolver,IlookIkSolver
{
    #region Variable
    public IkManager ikManager;
    public Animator anim; //Animator
    private Vector3 SpinePositon, leftHandPos,rightHandPos, localLeftHandPos, localRightHandPos, rightHandIkPos,leftHandIkPos,lastRightHandPos,lastLeftHandPos;
    private Quaternion leftHandRot, rightHandRot, rightHandIkRot, leftHandIkRot;
    private float lastBodyPositionY;

    public bool enableAimIk = true;
    public Transform LookAtTarget;
    public Transform AimSpine;
    [SerializeField] private Vector3 aimOffsetDir;
    public bool showSolverDebug = true;

    #endregion

    #region Initialization
    // Initialization of variables
    void Start()
    {
        ikManager = this.GetComponent<IkManager>();
        anim = this.GetComponent<Animator>();
        if (anim == null)
            Debug.LogError("We require " + transform.name + " game object to have an animator. This will allow for Aim IK to funct ion");
    }

    #endregion

    #region UpdateMethods
    /// <summary>
    /// 
    /// </summary>
    private void AnimUpdate()
    {
        if (enableAimIk == false) { return; }
        if (LookAtTarget == null) { return; }

        AdjustHandTarget(ref localLeftHandPos, ref leftHandRot, HumanBodyBones.LeftHand);
        AdjustHandTarget(ref localRightHandPos, ref rightHandRot, HumanBodyBones.RightHand);
        SpinePositon = AimSpine.position;
    }
    #endregion

    #region AimIkMehthods
    /// <summary>
    /// 
    /// </summary>
    void SolverUpdate()
    {
        if (enableAimIk == false) { return; }
        if (anim == null) { return; }



        leftHandPos = transform.TransformPoint(localLeftHandPos);
        rightHandPos = transform.TransformPoint(localRightHandPos);
        HandPositionSolver(leftHandPos, leftHandRot, SpinePositon, ref leftHandIkPos, ref leftHandIkRot);
        HandPositionSolver(rightHandPos, rightHandRot, SpinePositon, ref rightHandIkPos, ref rightHandIkRot);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handPosition"></param>
    /// <param name="handRotation"></param>
    /// <param name="SpinePositon"></param>
    /// <param name="handIkPostion"></param>
    /// <param name="handIkRotation"></param>
    private void HandPositionSolver(Vector3 handPosition,Quaternion handRotation, Vector3 SpinePositon,ref Vector3 handIkPosition,ref Quaternion handIkRotation)
    {
        if (showSolverDebug)
        {
            Debug.DrawLine(LookAtTarget.position, SpinePositon);
           //Debug.DrawLine(aimOffsetDir.normalized*1f+SpinePositon, SpinePositon, Color.blue);
        }

        handIkPosition = handPosition;
       
        if (AimSpine == null || LookAtTarget== null) { return; }

        Vector3 dirFromSpineToTarget = LookAtTarget.position - SpinePositon;
        Vector3 dirFromSpineToHand = handPosition - SpinePositon;

        Quaternion rotOffset = Quaternion.FromToRotation(transform.forward, aimOffsetDir);
        Vector3 dirFaceForward =rotOffset *transform.forward;

        Quaternion rot = Quaternion.FromToRotation(dirFaceForward, dirFromSpineToTarget);

        dirFromSpineToHand = rot * dirFromSpineToHand;
        handIkPosition = SpinePositon + dirFromSpineToHand;

        handIkPosition = transform.InverseTransformPoint(handIkPosition);

        handIkRotation =rot * handRotation;
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handPosition"></param>
    /// <param name="handRotattion"></param>
    /// <param name="hand"></param>
    private void AdjustHandTarget(ref Vector3 handPosition, ref Quaternion handRotattion, HumanBodyBones hand)
    {
        Transform bone = anim.GetBoneTransform(hand);
        handPosition = transform.InverseTransformPoint(bone.position);
        handRotattion = bone.rotation;
    }
    #endregion

    #region intefaces
    public bool isEnabled()
    {
        return enableAimIk;
    }
    public void inFixedUpdate()
    {
        if (enableAimIk == false) { return; }
        if (anim == null) { return; }
        SolverUpdate();

    }
    public void inAnimatorIK()
    {
        if (enableAimIk == false) { return; }
        if (anim == null) { return; }
        AnimUpdate();
    }
    public Vector3 getLeftHandIkPos() {
        return leftHandIkPos;
    }
    public Vector3 getRightHandIkPos() {
        return rightHandIkPos;
    }
    public Quaternion getLeftHandIkRot()
    {
        return leftHandIkRot;
    }
    public Quaternion getRightHandIkRot()
    {
        return rightHandIkRot;
    }
    public Vector3 getLookAtPos()
    {
        return LookAtTarget.position;
    }
    #endregion
}

