using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;



public class BoundSurfaceObjectPlacer
{
    private GameObject plane;
    private FloatParameter random; //[0, 1]
    //private BoundConstraint boundReachability;

    private int maxPlacementTries = 100;


    private List<PlacementConstraint> collisionConstraints = new List<PlacementConstraint>();


    public BoundSurfaceObjectPlacer(
        GameObject plane,
        FloatParameter random,
        int maxPlacementTries)
    {
        this.plane = plane;
        this.random = random;
        this.maxPlacementTries = maxPlacementTries;
    }


    public void IterationStart()
    {
        collisionConstraints = new List<PlacementConstraint>();
    }

    public bool PlaceObject(GameObject obj, FloatParameter rotationRange)
    {
        Bounds planeBounds = plane.GetComponent<Renderer>().bounds;
        if (obj.activeInHierarchy)
        {
            //Debug.Log("script : "+ obj.name+" Tag:"+obj.tag);
            //MonoBehaviour[] objs = obj.GetComponentsInChildren<MonoBehaviour>();
            //RosSharp.Urdf.UrdfVisuals[] objs2 = obj.GetComponentsInChildren<RosSharp.Urdf.UrdfVisuals>();
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            
            // try to sample a valid point
            //Bounds objBounds = obj.GetComponent<Renderer>().bounds;
            Bounds objBounds = renderers[0].bounds;
            float radius = objBounds.extents.magnitude;
            float heightAbovePlane = Mathf.Min(objBounds.extents.x, objBounds.extents.y, objBounds.extents.z);

            List<PlacementConstraint> constraints = GetAllConstraints();
            Vector3? point = SampleValidGlobalPointOnPlane(radius, constraints, planeBounds);

            if (point.HasValue)
            {
                // place object
                Vector3 foundPoint = point ?? Vector3.zero;
                obj.transform.position = new Vector3(foundPoint.x, foundPoint.y + heightAbovePlane, foundPoint.z);
                obj.transform.rotation = Quaternion.Euler(0,rotationRange.Sample(), 0);

                // update constraints so subsequently placed object cannot collide with this one
                CollisionConstraint newConstraint = new CollisionConstraint();
                newConstraint.x = foundPoint.x;
                newConstraint.z = foundPoint.z;
                newConstraint.radius = radius;
                collisionConstraints.Add(newConstraint);

            }
            else
            {
                return false;
            }
        }
        return true;

    }

    // PRIVATE HELPERS

    private Vector3? SampleValidGlobalPointOnPlane(float objectRadius, List<PlacementConstraint> constraints, Bounds planeBounds)
    {
        // return a valid point and if not found one it return null 
        int tries = 0;

        while (tries < maxPlacementTries)
        {
            Vector3 point = SampleGlobalPointOnPlane(objectRadius, planeBounds);
            bool valid = PassesConstraints(point, objectRadius, constraints);
            if (valid) { return point; }

            tries += 1;
        }
        return null;
    }

    private List<PlacementConstraint> GetAllConstraints()
    {
        // return a list of all the constraints: combination of permanent constraint and additional constraint like the maxReachabilityConstraint or the 
        // collision constraint 
        List<PlacementConstraint> allConstraints = new List<PlacementConstraint>();
        allConstraints.AddRange(collisionConstraints);
        //allConstraints.Add(planeBoundConstraint);
        return allConstraints;
    }

    private Vector3 SampleGlobalPointOnPlane(float minEdgeDistance, Bounds planeBounds)
    {
        Rect planePlacementZone = PlanePlacementZone(planeBounds, minEdgeDistance);
        Vector2 randomPlanePoint = RandomPointInRect(planePlacementZone);
        Vector3 globalPt = new Vector3(randomPlanePoint.x, planeBounds.center.y, randomPlanePoint.y);
        return globalPt;
    }

    private static Rect PlanePlacementZone(Bounds planeBounds, float minEdgeDistance)
    {
        float x = planeBounds.center.x - planeBounds.extents.x + minEdgeDistance;
        float z = planeBounds.center.z - planeBounds.extents.z + minEdgeDistance;
        float dx = (planeBounds.extents.x - minEdgeDistance) * 2;
        float dz = (planeBounds.extents.z - minEdgeDistance) * 2;
        return new Rect(x, z, dx, dz);
    }

    private Vector2 RandomPointInRect(Rect rect)
    {
        float x = random.Sample() * rect.width + rect.xMin;
        float y = random.Sample() * rect.height + rect.yMin;
        return new Vector2(x, y);
    }

    private static Rect Intersection(Rect rectA, Rect rectB)
    {
        float minX = Mathf.Max(rectA.xMin, rectB.xMin);
        float maxX = Mathf.Min(rectA.xMax, rectB.xMax);
        float minY = Mathf.Max(rectA.yMin, rectB.yMin);
        float maxY = Mathf.Min(rectA.yMax, rectB.yMax);

        bool xValid = (minX < maxX);
        bool yValid = (minY < maxY);
        bool valid = (xValid && yValid);
        if (!valid)
        {
            throw new Exception("Rectangles have no intersection!");
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);

    }

    private static bool PassesConstraints(Vector3 point, float objectRadius, List<PlacementConstraint> constraints)
    {
        /* Checks if sampled point on plane passes all provided constraints. */

        foreach (PlacementConstraint constraint in constraints)
        {
            bool pass = constraint.Passes(point.x, point.z, objectRadius);
            if (!pass) { return false; }
        }
        return true;
    }


}