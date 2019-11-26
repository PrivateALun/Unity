//Copyright Filmstorm (C) 2018 一Movement Controller for Root Motion and built in IK solver
//modified by A1un 2019

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IkManager))]
[RequireComponent(typeof(Animator))]

public class FootIK : MonoBehaviour,IfootIkSolver
{
    #region Variable
  
    public Animator anim; //Animator

    private Vector3 rightFootPosition, leftFootPosition, leftFootIkPosition, rightFootIkPosition;
    private Quaternion leftFootIkRotation, rightFootIkRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY, pelvisIkPositionY;

    [Header("Feet Grounder")]
    public bool enableFeetIk = true;
    
    [Range(0, 2)]
    [SerializeField] private float heightFromGroundRaycast = 1.14f;
    [Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float pelvisOffset = 0f;
    [Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.28f;

    public bool showSolverDebug = true;

    private float pelvisOffsetY;

    #endregion

    #region Initialization
    // Initialization of variables
    void Start()
    {
        anim = this.GetComponent<Animator>();
        if (anim == null)
            Debug.LogError("We require " + transform.name + " game object to have an animator. This will allow for Foot IK to funct ion");
    }

    #endregion

    #region FeetGroundingMethods


    private void MovePelvisHeight()
    {

        if (rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero ){
            pelvisOffsetY = 0;
            return;
        }
        float lOffsetPosition = leftFootIkPosition.y - transform.position.y;
        float rOffsetPosition = rightFootIkPosition.y - transform.position.y;
        float total0ffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;
        
        pelvisOffsetY  = Mathf.Lerp(pelvisOffsetY, total0ffset, pelvisUpAndDownSpeed);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromSkyPosition"></param>
    /// <param name="feetIkPositions"></param>
    /// <param name="feetIkRotations"></param>
    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {

        //raycast handling section 
        RaycastHit feetOutHit;
        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);
        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer)){
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
            feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;
            return;
        }
        feetIkPositions = Vector3.zero; //it didn't work :(
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="feetPositions"></param>
    /// <param name="foot"></param>
    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = anim.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }


    #endregion

    #region interface

    public bool isEnabled()
    {
        return enableFeetIk;
    }
    public void inFixedUpdate()
    {
        if (enableFeetIk == false) { return; }
        if (anim == null) { return; }

        //find and raycast to the ground to find positions 
        FeetPositionSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation); // handle the solver for right foot
        FeetPositionSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation); //handle the solver for the left foot
        MovePelvisHeight();
    }
    public void inAnimatorIK()
    {
        if (enableFeetIk == false) { return; }
        if (anim == null) { return; }
        lastPelvisPositionY = anim.bodyPosition.y;
        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);
        
    }
    public float getPelvisIkPosY()
    {
        return pelvisIkPositionY;
    }
    public float getPelvisOffsetPosY()
    {
        return pelvisOffsetY;
    }
    public Vector3 getLeftFootIkPos()
    {
        return leftFootIkPosition;
    }
    public Vector3 getRightFootIkPos()
    {
        return rightFootIkPosition;
    }
    public Quaternion getLeftFootIkRot()
    {
        return leftFootIkRotation;
    }
    public Quaternion getRightFootIkRot()
    {
        return rightFootIkRotation;
    }

    #endregion
}




