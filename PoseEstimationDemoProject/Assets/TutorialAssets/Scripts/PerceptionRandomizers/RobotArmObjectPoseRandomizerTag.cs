using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;

[AddComponentMenu("Perception/RandomizerTags/RobotArmObjectPoseRandomizerTag")]
[RequireComponent(typeof(Transform))]
public class RobotArmObjectPoseRandomizerTag : RandomizerTag
{
    public bool mustBeReachable;
}
