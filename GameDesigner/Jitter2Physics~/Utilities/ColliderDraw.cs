using UnityEngine;

public static class ColliderDraw
{
    public static void DrawCapsule(Vector3 start, Vector3 end, Color color, float radius = 1)
    {
        Vector3 up = (end - start).normalized * radius;
        Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
        Vector3 right = Vector3.Cross(up, forward).normalized * radius;

        Color oldColor = Gizmos.color;
        Gizmos.color = color;

        float height = (start - end).magnitude;
        float sideLength = Mathf.Max(0, (height * 0.5f) - radius);
        Vector3 middle = (end + start) * 0.5f;

        start = middle + ((start - middle).normalized * sideLength);
        end = middle + ((end - middle).normalized * sideLength);

        //Radial circles
        DrawCircle(start, up, color, radius);
        DrawCircle(end, -up, color, radius);

        //Side lines
        Gizmos.DrawLine(start + right, end + right);
        Gizmos.DrawLine(start - right, end - right);

        Gizmos.DrawLine(start + forward, end + forward);
        Gizmos.DrawLine(start - forward, end - forward);

        for (int i = 1; i < 26; i++)
        {

            //Start endcap
            Gizmos.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + start, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + start);
            Gizmos.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + start, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + start);
            Gizmos.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + start, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + start);
            Gizmos.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + start, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + start);

            //End endcap
            Gizmos.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + end, Vector3.Slerp(right, up, (i - 1) / 25.0f) + end);
            Gizmos.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + end, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + end);
            Gizmos.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + end, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + end);
            Gizmos.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + end, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + end);
        }

        Gizmos.color = oldColor;
    }

    public static void DrawCircle(Vector3 position, Vector3 up, Color color, float radius = 1.0f)
    {
        up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
        Vector3 _forward = Vector3.Slerp(up, -up, 0.5f);
        Vector3 _right = Vector3.Cross(up, _forward).normalized * radius;

        Matrix4x4 matrix = new Matrix4x4();

        matrix[0] = _right.x;
        matrix[1] = _right.y;
        matrix[2] = _right.z;

        matrix[4] = up.x;
        matrix[5] = up.y;
        matrix[6] = up.z;

        matrix[8] = _forward.x;
        matrix[9] = _forward.y;
        matrix[10] = _forward.z;

        Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
        Vector3 _nextPoint = Vector3.zero;

        Color oldColor = Gizmos.color;
        Gizmos.color = (color == default(Color)) ? Color.white : color;

        for (var i = 0; i < 91; i++)
        {
            _nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
            _nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
            _nextPoint.y = 0;

            _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

            Gizmos.DrawLine(_lastPoint, _nextPoint);
            _lastPoint = _nextPoint;
        }

        Gizmos.color = oldColor;
    }
}
