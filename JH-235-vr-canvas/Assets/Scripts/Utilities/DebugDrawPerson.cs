using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrawPerson : MonoBehaviour
{
    [SerializeField][Range(1f,2f)]
    float height = 1.8f;
    [SerializeField]
    Color color = Color.blue;

    private const float headRadius = 0.175f;

    private const float chestWidth = 0.5f;
    private const float chestDepth = 0.25f;

    private const float legWidth = 0.35f;
    private const float legDepth = 0.25f;


    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 legSize = new Vector3(legWidth, height * 0.8f, legDepth);
        Vector3 legPos = Vector3.zero + new Vector3(0f, (height * 0.8f) / 2f, 0);
        Gizmos.DrawCube(legPos, legSize);

        Vector3 chestSize = new Vector3(chestWidth, height * 0.3f, chestDepth);
        Vector3 chestPos = Vector3.zero + new Vector3(0f, height * 0.65f, 0);
        Gizmos.DrawCube(chestPos, chestSize);

        Vector3 headPos = Vector3.zero + new Vector3(0, height - headRadius, 0);
        Gizmos.DrawSphere(headPos, headRadius);
    }
}
