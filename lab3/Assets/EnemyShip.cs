using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy ship behaviour, built only from allowed math:
///  - Orbits the player (tangent via Cross + radius correction via vector arithmetic)
///  - Faces the player manually (Mathf.Atan2 + Rad2Deg + Quaternion.Euler + Slerp)
///  - Speed scales with distance to the player using sqrMagnitude (no sqrt for the check)
///  - Simple separation-based collision avoidance (stretch goal)
///
/// NOT used: LookAt, RotateAround, LookRotation, RotateTowards, FromToRotation.
/// </summary>
public class EnemyShip : MonoBehaviour
{
    // Registry so enemies can see each other for avoidance without physics queries.
    private static readonly List<EnemyShip> All = new List<EnemyShip>();

    [Header("Target")]
    public Transform player;

    [Header("Orbit")]
    public float orbitRadius = 5f;
    public float orbitSpeed = 3f;          // tangential speed (units/sec)
    public float radiusCorrectionSpeed = 3f; // how hard we pull toward the orbit radius
    [Tooltip("+1 = counter-clockwise, -1 = clockwise")]
    public int orbitDirection = 1;

    [Header("Speed by distance (sqrMagnitude)")]
    [Tooltip("Inside this distance the enemy uses the 'near' multiplier.")]
    public float nearDistance = 3f;
    [Tooltip("Beyond this distance the enemy uses the 'far' multiplier.")]
    public float farDistance = 12f;
    public float nearSpeedMultiplier = 0.6f; // slower when close
    public float farSpeedMultiplier = 2.0f;  // faster when far away (catches up)

    [Header("Facing")]
    [Tooltip("Rotation needed so the sprite's nose points along +X. " +
             "Sprite drawn pointing UP => -90. Pointing RIGHT => 0. Pointing DOWN => 90.")]
    public float spriteAngleOffset = -90f;
    public float turnSpeed = 6f;

    [Header("Collision Avoidance (stretch)")]
    public bool avoidOthers = true;
    public float avoidRadius = 1.6f;
    public float avoidStrength = 6f;

    [Header("Smoothing")]
    public float velocitySmoothing = 5f;

    private Vector3 velocity;

    private void OnEnable()  { All.Add(this); }
    private void OnDisable() { All.Remove(this); }

    private void Update()
    {
        if (player == null) return;

        float dt = Time.deltaTime;

        // ---- Vector from enemy to player (2D game: flatten Z) ----
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.z = 0f;

        // sqrMagnitude: cheap distance comparison, no square root.
        float sqrDist = toPlayer.sqrMagnitude;

        // Guard against a zero-length vector (enemy exactly on the player).
        Vector3 dirToPlayer = sqrDist > 0.0001f ? toPlayer.normalized : Vector3.up;

        // ---------------------------------------------------------------
        // 1) Speed multiplier from distance, using sqrMagnitude only
        // ---------------------------------------------------------------
        float nearSqr = nearDistance * nearDistance;
        float farSqr  = farDistance * farDistance;
        float t = Mathf.InverseLerp(nearSqr, farSqr, sqrDist);          // 0 (near) -> 1 (far)
        float speedMultiplier = Mathf.Lerp(nearSpeedMultiplier, farSpeedMultiplier, t);

        // ---------------------------------------------------------------
        // 2) Orbit: tangent (perpendicular to the radius) + radius correction
        // ---------------------------------------------------------------
        // Cross(forward, dir) gives the 90-degree-rotated vector in the XY plane: (-y, x, 0)
        Vector3 tangent = Vector3.Cross(Vector3.forward, dirToPlayer) * orbitDirection;

        // Positive when we're too far (move toward player), negative when too close (move away).
        float dist = Mathf.Sqrt(sqrDist);
        float radiusError = Mathf.Clamp(dist - orbitRadius, -1f, 1f);
        Vector3 radial = dirToPlayer * (radiusError * radiusCorrectionSpeed);

        Vector3 desiredVelocity = (tangent * orbitSpeed + radial) * speedMultiplier;

        // ---------------------------------------------------------------
        // 3) Collision avoidance: push away from nearby enemies
        // ---------------------------------------------------------------
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
                    // Closer neighbours push harder (weight 1 at contact -> 0 at edge of radius).
                    float weight = 1f - sq / avoidSqr;
                    separation += away.normalized * weight;
                }
            }

            desiredVelocity += separation * avoidStrength;
        }

        // Smooth the velocity so direction changes aren't jerky.
        velocity = Vector3.Lerp(velocity, desiredVelocity, 1f - Mathf.Exp(-velocitySmoothing * dt));
        transform.position += velocity * dt;

        // ---------------------------------------------------------------
        // 4) Face the player WITHOUT LookAt / LookRotation
        // ---------------------------------------------------------------
        float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * dt);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        // Draw the orbit ring around the player (approximated with line segments).
        const int segments = 48;
        Vector3 prev = player.position + new Vector3(orbitRadius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = i * (Mathf.PI * 2f / segments);
            Vector3 next = player.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * orbitRadius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, avoidRadius);
    }
#endif
}
