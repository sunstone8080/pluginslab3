using System.Collections.Generic;
using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    //enemies can see each other for avoidance without physics
    private static readonly List<EnemyShip> All = new List<EnemyShip>();

   
    public Transform player;
    public float orbitRadius = 5f;
    public float orbitSpeed = 3f;         
    public float radiusCorrectionSpeed = 3f; 
    //+1 = counter-clockwise -1 = clockwise
    public int orbitDirection = 1;
    public float nearDistance = 3f;
    public float farDistance = 12f;
    public float nearSpeedMultiplier = 0.6f; 
    public float farSpeedMultiplier = 2.0f; 

    public float spriteAngleOffset = -90f;
    public float turnSpeed = 6f;

    public bool avoidOthers = true;
    public float avoidRadius = 1.6f;
    public float avoidStrength = 6f;

    public float velocitySmoothing = 5f;

    private Vector3 velocity;

    private void OnEnable()  { All.Add(this); }
    private void OnDisable() { All.Remove(this); }

    private void Update()
    {
        if (player == null) return;

        float dt = Time.deltaTime;

        //vector from enemy to player
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.z = 0f;

        //sqrMagnitude distance comparison
        float sqrDist = toPlayer.sqrMagnitude;

        //no vector = 0
        Vector3 dirToPlayer = sqrDist > 0.0001f ? toPlayer.normalized : Vector3.up;

        
        //speed multiplier from distance
     
        float nearSqr = nearDistance * nearDistance;
        float farSqr  = farDistance * farDistance;
        float t = Mathf.InverseLerp(nearSqr, farSqr, sqrDist); //0 near 1 far
        float speedMultiplier = Mathf.Lerp(nearSpeedMultiplier, farSpeedMultiplier, t);

    
        //orbit: tangent + radius correction
       
        //cross product with normal gives a vector that is 90 deg from dirtoPlayer
        Vector3 tangent = Vector3.Cross(Vector3.forward, dirToPlayer) * orbitDirection;

        //real distance to player
        float dist = Mathf.Sqrt(sqrDist);
        //clamp to correct to -1 to 1
        float radiusError = Mathf.Clamp(dist - orbitRadius, -1f, 1f);
        //correct if to far or too close
        Vector3 radial = dirToPlayer * (radiusError * radiusCorrectionSpeed);

        Vector3 desiredVelocity = (tangent * orbitSpeed + radial) * speedMultiplier;

        //collision avoidance
        if (avoidOthers)
        {
            Vector3 separation = Vector3.zero;
            float avoidSqr = avoidRadius * avoidRadius;

            for (int i = 0; i < All.Count; i++)
            {
                EnemyShip other = All[i];
                if (other == this) continue;

                Vector3 away = transform.position - other.transform.position;
                away.z = 0f;
                float sq = away.sqrMagnitude;

                if (sq < avoidSqr && sq > 0.0001f)
                {
                    //closer they are the harder they push
                    float weight = 1f - sq / avoidSqr;
                    separation += away.normalized * weight;
                }
            }

            desiredVelocity += separation * avoidStrength;
        }

        //smooth the velocity for direction changes
        velocity = Vector3.Lerp(velocity, desiredVelocity, 1f - Mathf.Exp(-velocitySmoothing * dt));
        transform.position += velocity * dt;
        float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * dt);
    }


}
