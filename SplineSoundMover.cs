using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;  // Required for float3 support

public class SplineSoundMover : MonoBehaviour
{
    [Tooltip("Spline Container to follow")]
    public SplineContainer splineContainer;

    [Tooltip("Character to track")]
    public GameObject player;

    [Tooltip("Speed at which the sound source moves smoothly (higher is faster)")]
    [Range(0.1f, 50f)]
    public float lerpSpeed = 5f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Update()
    {
        MoveSoundSourceAlongSpline();
        SmoothMoveAndRotate();
    }

    private void MoveSoundSourceAlongSpline()
    {
        // Find the closest point on the spline relative to the player's position
        float closestT = GetClosestTOnSpline(player.transform.position);

        // Convert the evaluated position from local space to world space
        targetPosition = splineContainer.transform.TransformPoint(
            splineContainer.Spline.EvaluatePosition(closestT)
        );

        // Convert float3 tangent to Vector3 and normalize it for rotation calculation
        float3 tangentFloat3 = splineContainer.Spline.EvaluateTangent(closestT);
        Vector3 tangent = new Vector3(tangentFloat3.x, tangentFloat3.y, tangentFloat3.z).normalized;

        // Calculate the target rotation along the spline tangent
        targetRotation = Quaternion.LookRotation(tangent);
    }

    private void SmoothMoveAndRotate()
    {
        // Smoothly interpolate position and rotation towards the target
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpSpeed * Time.deltaTime);
    }

    private float GetClosestTOnSpline(Vector3 position)
    {
        float closestT = 0f;
        float closestDistance = float.MaxValue;

        // Transform player position into the local space of the spline for accurate comparison
        Vector3 localPlayerPos = splineContainer.transform.InverseTransformPoint(position);

        // Iterate through the spline points to find the closest T (progress value)
        for (float t = 0; t <= 1; t += 0.01f)
        {
            Vector3 point = splineContainer.Spline.EvaluatePosition(t);
            float distance = Vector3.Distance(localPlayerPos, point);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestT = t;
            }
        }

        return closestT;
    }
}
