using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Vector3 targetPosition; // Reference to the surface or object to face
    private float distance = 30.0f; // Distance from the target

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setCamera(Vector2[] vertices)
    {
        var center = CalculateCentroid(vertices);
        targetPosition = new Vector3(center.x, 4f, center.y);

        // Calculate the camera position based on the target position and distance
        Vector3 cameraPosition = targetPosition - Vector3.forward * distance;

        // Set the camera's position to the calculated position
        transform.position = cameraPosition;

        // Calculate the direction from the camera to the target position
        Vector3 lookDirection = targetPosition - transform.position;

        // Calculate the rotation based on the direction
        Quaternion rotation = Quaternion.LookRotation(lookDirection);

        // Set the camera's rotation to face the target position
        transform.rotation = rotation;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = targetPosition;

    }
    public Vector2 CalculateCentroid(Vector2[] vertices)
    {
        Vector2 centroid = Vector2.zero;
        float signedArea = 0f;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 currentVertex = vertices[i];
            Vector2 nextVertex = vertices[(i + 1) % vertices.Length]; // Circular indexing

            float crossProduct = (currentVertex.x * nextVertex.y) - (nextVertex.x * currentVertex.y);
            signedArea += crossProduct;

            centroid.x += (currentVertex.x + nextVertex.x) * crossProduct;
            centroid.y += (currentVertex.y + nextVertex.y) * crossProduct;
        }

        signedArea *= 0.5f;
        centroid /= (6f * signedArea);

        return centroid;
    }
}
