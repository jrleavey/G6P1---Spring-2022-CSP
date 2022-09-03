using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ZombieAI))]

public class ZombieAIEditor : Editor
{
    private void OnSceneGUI()
    {
        ZombieAI fov = (ZombieAI)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.radius);

        Vector3 viewAngle01 = DirectionFromAngle(fov.transform.eulerAngles.y, -fov.angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fov.transform.eulerAngles.y, fov.angle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle01 * fov.radius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle02 * fov.radius);

        if (fov._isChasingPlayer)
        {
            Handles.color = Color.green;
            Handles.DrawLine(fov.transform.position, fov._player.transform.position);
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}