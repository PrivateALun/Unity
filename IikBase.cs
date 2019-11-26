using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IikBase {
    bool isEnabled();
    void inFixedUpdate();
    void inAnimatorIK();
}
