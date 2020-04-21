using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Circle
{
    private float radius;
    private int numPoints;
    private Vector3[] points;
    private float angleBetweenPoints;

    public Circle(float a_radius, int a_numPoints)
    {
        radius = a_radius;
        numPoints = a_numPoints;
        points = new Vector3[numPoints];
        angleBetweenPoints = 360.0f / numPoints;
        CreateCircle();
    }

    private void CreateCircle()
    {
        Quaternion incrementalQuaternion = Quaternion.Euler(0, 0, -angleBetweenPoints);

        Vector2 direction = new Vector2(1.0f, 0.0f);

        Vector2 position;

        for (int i = 0; i < points.Length; i++)
        {
            position = direction * radius;

            points[i] = position;

            direction = incrementalQuaternion * direction;
        }
    }

    public void SetRadius(float a_radius)
    {
        radius = a_radius;
        CreateCircle();
    }

    public Vector3[] GetPoints()
    {
        return points;
    }
}
