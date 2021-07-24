using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;


[Serializable]
[AddRandomizerMenu("Perception/Robot Arm Object Pose Randomizer")]
public class RobotArmObjectPoseRandomizer : Randomizer
{
    /*  Chooses poses on the plane for placing all objects with the corresponding tag.
     *      - Each object has a radius (defined on the tag, computed per-object based on its bounds)
     *      - No object will be close enough to the edge of the plane to fall off
     *      - All objects will be within the min and max RobotReachability distance to the robot base link (as measured 
     *          on the surface of the plane).
     *      - No object will be close enough to another tagged object to collide with it
     *      
     *  Example use case: placing objects on a table with a robot arm, at random valid poses
     *  where they can be reached by the robot arm. 
     *  
     *  The plane can be manipulated in the editor for easy visualization of the placement surface.
     *  
     *  Assumptions:
     *      - The placement surface is parallel to the global x-z plane. 
     *      - The robot arm is sitting on the placement surface
     */

    public GameObject plane;
    public FloatParameter rotationRange = new FloatParameter { value = new UniformSampler(0f, 360f)}; // in range (0, 1)
    public int maxPlacementTries = 100;

    private FloatParameter random = new FloatParameter {value = new UniformSampler(0f, 1f)};

    private BoundSurfaceObjectPlacer placer;
    

    protected override void OnScenarioStart()
    {
        //BoundConstraint inConstraint = CreateBoundConstraint();
        placer = new BoundSurfaceObjectPlacer(plane, random, maxPlacementTries);
    }


    protected override void OnIterationStart()
    {
        placer.IterationStart();

        IEnumerable<RobotArmObjectPoseRandomizerTag> tags = tagManager.Query<RobotArmObjectPoseRandomizerTag>();

        foreach (RobotArmObjectPoseRandomizerTag tag in tags)
        {
            GameObject obj = tag.gameObject;
            bool success = placer.PlaceObject(obj, rotationRange);
            if (!success)
            {
                return;
            }
        }
    }



}