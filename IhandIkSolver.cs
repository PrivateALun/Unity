using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IhandIkSolver:IikBase{

    Vector3 getLeftHandIkPos();
    Vector3 getRightHandIkPos();
    Quaternion getLeftHandIkRot();
    Quaternion getRightHandIkRot();
}

