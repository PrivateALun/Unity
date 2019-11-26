//Copyright A1un (C) 2019 IK Manager
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class IkManager : MonoBehaviour {

    #region Variable

    public List<IikBase> iIkBases = new List<IikBase>();
    public IhandIkSolver iHandIkSolver;
    public IlookIkSolver iLookIkSolver;
    public IfootIkSolver iFootIkSolver;

    public Animator anim; //Animator
    public bool enableIk = true;
    [Header("Ik Weight Options")]

    [Range(0, 1)] [SerializeField] private float targetHeadIkWeight = 1f;
    [Range(0, 1)] [SerializeField] private float targetBodyIkWeight = 0.2f;
    [Range(0, 1)] [SerializeField] private float targetPelvisIkWeight = 1f;
    [Range(0, 1)] [SerializeField] private float targetLeftHandIkWeight = 1f;
    [Range(0, 1)] [SerializeField] private float targetRightHandIkWeight = 1f;
    [Range(0, 1)] [SerializeField] private float targetLeftFootIkWeight = 1f;
    [Range(0, 1)] [SerializeField] private float targetRightFootIkWeight = 1f;

    private float headIkWeight = 1f;
    private float bodyIkWeight = 1f;
    private float pelvisIkWeight = 1f;
    private float leftHandIkWeight = 1f;
    private float rightHandIkWeight = 1f;
    private float leftFootIkWeight = 1f;
    private float rightFootIkWeight = 1f;


    [Header("Ik Weight Curve Options")]
    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";
    public bool useProIkFeature = false;

    [Header("Ik Speed Options")]
    [Range(0, 1)] [SerializeField] private float handToIkPositionSpeed = 0.5f;
    [Range(0, 1)] [SerializeField] private float handToIkRotationSpeed = 0.5f;
    [Range(0, 1)] [SerializeField] private float feetToIkPositionSpeed = 0.5f;

    [Header("Debug")]
    public bool showSolverDebug = true;

    private float bodyIkPosY, lastBodyPosOffsetY;
    private Vector3 SpinePositon, leftHandPos, rightHandPos, rightHandIkPos, leftHandIkPos, lastRightHandPos, lastLeftHandPos, LookAtPos;
    private Quaternion leftHandRot, rightHandRot, rightHandIkRot, leftHandIkRot, lastRightHandRot, lastLeftHandRot;


    private Vector3 rightFootPos, leftFootPos, leftFootIkPos, rightFootIkPos;
    private Quaternion leftFootIkRot, rightFootIkRot;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

    #endregion

    #region Initialization
    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
        iHandIkSolver = GetComponent<IhandIkSolver>();
        iLookIkSolver = GetComponent<IlookIkSolver>();
        iFootIkSolver = GetComponent<IfootIkSolver>();
        foreach (IikBase ib in GetComponents<IikBase>())
        {
            iIkBases.Add(ib);
        }
    }
    #endregion

    #region Update

    private void FixedUpdate()
    {
        if (GetComponent<CharacterController>().isGrounded)
        {
            setTargetPelvisWeight(1);
        }
        else
        {
            setTargetPelvisWeight(0);
        }

        if (isLeftFootGrounded())
            setTargetFeetWeight(1, -1);
        else
            setTargetFeetWeight(0, -1);

        if (isRightFootGrounded())
            setTargetFeetWeight(-1, 1);
        else
            setTargetFeetWeight(-1,0);

        headIkWeight = Mathf.Lerp(targetHeadIkWeight, 1, 0.3f);
        bodyIkWeight = Mathf.Lerp(targetBodyIkWeight, 1, 0.3f);
        pelvisIkWeight = Mathf.Lerp(targetPelvisIkWeight, 1, 0.3f);
        leftFootIkWeight = Mathf.Lerp(targetLeftFootIkWeight, 1, 0.3f);
        rightFootIkWeight = Mathf.Lerp(targetRightFootIkWeight, 1, 0.3f);

        PosSolverByWeight();
        foreach (IikBase iis in iIkBases)
        {
            if (iis.isEnabled() == false)
                continue;
            iis.inFixedUpdate();
        }
    }
    private void applyInAnimatorIk(IikBase iis)
    {
        if (iis.isEnabled() == false)
            return;
        iis.inAnimatorIK();
    }
	private void OnAnimatorIK()
    {

        applyInAnimatorIk(iFootIkSolver);
        AnimBodyPosIkUpdate();
        applyInAnimatorIk(iHandIkSolver);
        AnimIkUpdate();
    }
    // Update is called once per frame
    private void LateUpdate () {
        SetHandRotation();
	}
    #endregion

    #region UpdateMethods
    void AnimBodyPosIkUpdate()
    {
        Vector3 lastBodyPos = anim.bodyPosition;
        Vector3 bodyIkPos = anim.bodyPosition;
        bodyIkPos.y += bodyIkPosY;
        Vector3 newVector3;
        newVector3 = Vector3.Lerp(lastBodyPos, bodyIkPos, pelvisIkWeight);
        anim.bodyPosition = newVector3;

        lastBodyPosOffsetY = newVector3.y - lastBodyPos.y;
    }
    /// <summary>
    /// 
    /// </summary>
    void AnimIkUpdate()
    {
        if (enableIk == false) { return; }
        if (anim == null) { return; }

        anim.SetLookAtPosition(LookAtPos);
        anim.SetLookAtWeight(headIkWeight, bodyIkWeight);


        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIkWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIkWeight);
        MoveHandToIkPoint(AvatarIKGoal.LeftHand, leftHandIkPos, leftHandIkRot, ref lastLeftHandPos);

        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIkWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIkWeight);
        MoveHandToIkPoint(AvatarIKGoal.RightHand, rightHandIkPos, rightHandIkRot, ref lastRightHandPos);

        //right foot ik position and rotation -- utilise the pro features in here
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootIkWeight);
        if (useProIkFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));
        }
        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPos, rightFootIkRot, ref lastRightFootPositionY);
        //left foot ik position and rotation -- utilise the pro features in here
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootIkWeight);
        if (useProIkFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
        }
        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPos, leftFootIkRot, ref lastLeftFootPositionY);


    }
    #endregion

    #region IkMethods
    private void applyIkResult(IhandIkSolver iis)
    {
        if (iis.isEnabled() == false)
            return;
        leftHandIkPos = iis.getLeftHandIkPos();
        leftHandIkRot = iis.getLeftHandIkRot();
        rightHandIkPos = iis.getRightHandIkPos();
        rightHandIkRot = iis.getRightHandIkRot();
    }
    private void applyIkResult(IlookIkSolver iis)
    {
        if (iis.isEnabled() == false)
            return;
        LookAtPos = iis.getLookAtPos();
    }
    private void applyIkResult(IfootIkSolver iis)
    {
        if (iis.isEnabled() == false)
            return;
        leftFootIkPos = iis.getLeftFootIkPos();
        leftFootIkRot = iis.getLeftFootIkRot();
        rightFootIkPos = iis.getRightFootIkPos();
        rightFootIkRot = iis.getRightFootIkRot();
        bodyIkPosY = iis.getPelvisOffsetPosY();
        //bodyIkPosY = iis.getPelvisIkPosY();
    }
    /// <summary>
    /// 
    /// </summary>
    private void PosSolverByWeight()
    {
        applyIkResult(iHandIkSolver);
        applyIkResult(iLookIkSolver);
        applyIkResult(iFootIkSolver);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="positionIkHolder"></param>
    /// <param name="rotationIkHolder"></param>
    /// <param name="lastHandPosition"></param>
    private void MoveHandToIkPoint(AvatarIKGoal hand, Vector3 localPosIkHolder, Quaternion rotationIkHolder, ref Vector3 lastHandPosition)
    {
        Vector3 targetIkPosition = anim.GetIKPosition(hand);
        if (localPosIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            Vector3 Variable = Vector3.Lerp(lastHandPosition, localPosIkHolder, handToIkPositionSpeed);
            targetIkPosition = Variable;
            lastHandPosition = Variable;
            targetIkPosition = transform.TransformPoint(targetIkPosition);
            anim.SetIKRotation(hand, rotationIkHolder);
        }
        Vector3 newPelvisVectorOffset = Vector3.zero;
        newPelvisVectorOffset.y=getBodyIkPosOffsetY();
        anim.SetIKPosition(hand, targetIkPosition+ newPelvisVectorOffset);
    }
    /// <summary>
    /// Moves the feet to ik point.
    /// </summary>
    /// <param name="foot"></param>
    /// <param name="positionIkHolder"></param>
    /// <param name="rotationIkHolder"></param>
    /// <param name="lastFootPositionY"></param>
    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
    {
        Vector3 targetIkPosition = anim.GetIKPosition(foot);

        if (positionIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);
            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
            targetIkPosition.y += yVariable;
            lastFootPositionY = yVariable;
            targetIkPosition = transform.TransformPoint(targetIkPosition);
            anim.SetIKRotation(foot, rotationIkHolder);
        }

        anim.SetIKPosition(foot, targetIkPosition);

    }
    /// <summary>
    /// 
    /// </summary>
    private void SetHandRotation()
    {
        if (enableIk == false) { return; }
        if (anim == null) { return; }


        lastLeftHandRot = anim.GetBoneTransform(HumanBodyBones.LeftHand).rotation;
        lastRightHandRot = anim.GetBoneTransform(HumanBodyBones.RightHand).rotation;

        if (leftHandIkWeight >0)
            anim.GetBoneTransform(HumanBodyBones.LeftHand).rotation = Quaternion.Slerp(lastLeftHandRot, leftHandIkRot, leftHandIkWeight);

        if (rightHandIkWeight > 0f)
            anim.GetBoneTransform(HumanBodyBones.RightHand).rotation = Quaternion.Slerp(lastRightHandRot, rightHandIkRot, rightHandIkWeight);


    }
    #endregion

    #region publicFun

    public void setTargetLookWeight(float head, float body)
    {
        if(head >= 0)
            targetHeadIkWeight = head;
        if(body >= 0)
            targetBodyIkWeight = body;
    }
    public void setTargetPelvisWeight(float pelvis)
    {
        if(pelvis>=0)
            targetPelvisIkWeight = pelvis;
    }
    public void setTargetHandWeight(float left, float right)
    {
        if (left >= 0)
            targetLeftHandIkWeight = left;
        if (right >=0)
            targetRightHandIkWeight = right;
    }
    public void setTargetFeetWeight(float left, float right)
    {
        if (left >= 0)
            targetLeftFootIkWeight = left;
        if (right >= 0)
            targetRightFootIkWeight = right;
    }
    public float getBodyIkPosOffsetY()
    {
        return lastBodyPosOffsetY;
    }
    public bool isLeftFootGrounded()
    {
        return leftFootIkPos != Vector3.zero;
    }
    public bool isRightFootGrounded()
    {
        return rightFootIkPos != Vector3.zero;
    }
    #endregion
}
