using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IfootIkSolver : IikBase
{
    Vector3 getLeftFootIkPos();
    Vector3 getRightFootIkPos();
    Quaternion getLeftFootIkRot();
    Quaternion getRightFootIkRot();
    float getPelvisOffsetPosY();
    float getPelvisIkPosY();
}
