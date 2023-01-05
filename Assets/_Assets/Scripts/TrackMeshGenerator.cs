using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class TrackMeshGenerator : MonoBehaviour
{
    [SerializeField]
    private SplineContainer trackSplineComponent;

    private Spline trackSpline;

    [SerializeField]
    private float subdivisionSize;

    //private SplineData<float> trackWidth;

    private float trackWidth = 80.0f;

    private List<Vector3> trackMeshVertices;
    private List<int> trackMeshTris;

    void Start()
    {
        trackMeshVertices = new List<Vector3>();
        trackMeshTris = new List<int>();        

        trackSpline = trackSplineComponent.Spline;

        float normalizedT = 0.0f;
        float3 positionF3;
        float3 tangentF3;
        float3 upDirectionF3;

        SplineUtility.Evaluate(trackSpline, normalizedT, out positionF3, out tangentF3, out upDirectionF3);

        Vector3 position = (Vector3) positionF3;
        Vector3 currentForward = (Vector3) tangentF3;
        Vector3 currentUp = (Vector3) upDirectionF3;

        Vector3 currentRight = Vector3.Cross(currentForward, currentUp).normalized;

        trackMeshVertices.Add(position + (currentUp * -2.0f) + (currentRight * trackWidth * -0.5f));
        trackMeshVertices.Add(position + (currentUp * -2.0f) + (currentRight * trackWidth * 0.5f));
        trackMeshVertices.Add(position + (currentUp * 2.0f) + (currentRight * trackWidth * 0.5f));
        trackMeshVertices.Add(position + (currentUp * 2.0f) + (currentRight * trackWidth * -0.5f));

        float splineLength = trackSpline.GetLength();
        
        float maxIndex = splineLength / subdivisionSize;

        Debug.Log("max: " + maxIndex);

        int i;

        for(i = 1; i < maxIndex; i++) {
            SplineUtility.GetPointAtLinearDistance(trackSpline, normalizedT, subdivisionSize, out normalizedT);
            if(normalizedT >= 1.0f) {
                Debug.Log("Brokey");
                break;
            }
            SplineUtility.Evaluate(trackSpline, normalizedT, out positionF3, out tangentF3, out upDirectionF3);

            position = (Vector3) positionF3;
            currentForward = (Vector3) tangentF3;
            currentUp = (Vector3) upDirectionF3;

            currentRight = Vector3.Cross(currentForward, currentUp).normalized;

            trackMeshVertices.Add(position + (currentUp * -2.0f) + (currentRight * trackWidth * -0.5f));
            trackMeshVertices.Add(position + (currentUp * -2.0f) + (currentRight * trackWidth * 0.5f));
            trackMeshVertices.Add(position + (currentUp * 2.0f) + (currentRight * trackWidth * 0.5f));
            trackMeshVertices.Add(position + (currentUp * 2.0f) + (currentRight * trackWidth * -0.5f));

            // bottom right mod 4 = 0, bottom left mod 4 = 1, top left mod 4 = 2, top right mod 4 = 3
            
            int[] currentVertexIndices = { (4 * i), (4 * i) + 1, (4 * i) + 2, (4 * i) + 3 };
            int[] previousVertexIndices = new int[4];
            
            for(int j = 0; j < 4; j++) { previousVertexIndices[j] = currentVertexIndices[j] - 4; }

            // right face - (old bottom right, old top right, new top right) & (new top right, new bottom right, old bottom right)
            trackMeshTris.AddRange(new List<int>() {previousVertexIndices[0], previousVertexIndices[3], currentVertexIndices[3]});
            trackMeshTris.AddRange(new List<int>() {currentVertexIndices[3], currentVertexIndices[0], previousVertexIndices[0]});

            // top face - (old top left, new top left, new top right) & (new top right, old top right, old top left)
            trackMeshTris.AddRange(new List<int>() {previousVertexIndices[2], currentVertexIndices[2], currentVertexIndices[3]});
            trackMeshTris.AddRange(new List<int>() {currentVertexIndices[3], previousVertexIndices[3], previousVertexIndices[2]});

            // left face - (old bottom left, new bottom left, new top left) & (new top left, old top left, old bottm left)  FIX ORDER
            trackMeshTris.AddRange(new List<int>() {previousVertexIndices[1], currentVertexIndices[1], currentVertexIndices[2]});
            trackMeshTris.AddRange(new List<int>() {currentVertexIndices[2], previousVertexIndices[2], previousVertexIndices[1]});

            // bottom face - (old bottom right, new bottom right, new bottom left) & (new bottom left, old bottom left, old bottm left)  FIX ORDER
            trackMeshTris.AddRange(new List<int>() {previousVertexIndices[0], currentVertexIndices[0], currentVertexIndices[1]});
            trackMeshTris.AddRange(new List<int>() {currentVertexIndices[1], previousVertexIndices[1], previousVertexIndices[0]});
            Debug.Log(i);
        }

        i--;

        // bottom right mod 4 = 0, bottom left mod 4 = 1, top left mod 4 = 2, top right mod 4 = 3

        int[] finalVertexIndices = { (4 * i), (4 * i) + 1, (4 * i) + 2, (4 * i) + 3 };

        if (trackSpline.Closed) {
            // right face - (old bottom right, old top right, new top right) & (new top right, new bottom right, old bottom right)
            trackMeshTris.AddRange(new List<int>() {finalVertexIndices[0], finalVertexIndices[3], 3});
            trackMeshTris.AddRange(new List<int>() {3, 0, finalVertexIndices[0]});

            // top face - (old top left, new top left, new top right) & (new top right, old top right, old top left)
            trackMeshTris.AddRange(new List<int>() {finalVertexIndices[2], 2, 3});
            trackMeshTris.AddRange(new List<int>() {3, finalVertexIndices[3], finalVertexIndices[2]});

            // left face - (old bottom left, new bottom left, new top left) & (new top left, old top left, old bottm left)  FIX ORDER
            trackMeshTris.AddRange(new List<int>() {finalVertexIndices[1], 1, 2});
            trackMeshTris.AddRange(new List<int>() {2, finalVertexIndices[2], finalVertexIndices[1]});

            // bottom face - (old bottom right, new bottom right, new bottom left) & (new bottom left, old bottom left, old bottm left)  FIX ORDER
            trackMeshTris.AddRange(new List<int>() {finalVertexIndices[0], 0, 1});
            trackMeshTris.AddRange(new List<int>() {1, finalVertexIndices[1], finalVertexIndices[0]});
        } else {
            // start face - (bottom left, top left, top right) & (top right, bottom right, bottom left)
            trackMeshTris.AddRange(new List<int>() {1, 2, 3});
            trackMeshTris.AddRange(new List<int>() {3, 0, 1});

            // end face - (bottom right, top right, top left) & (top left, bottom left, bottom right)
            trackMeshTris.AddRange(new List<int>() {finalVertexIndices[0], finalVertexIndices[3], finalVertexIndices[2]});
            trackMeshTris.AddRange(new List<int>() {finalVertexIndices[2], finalVertexIndices[1], finalVertexIndices[0]});
        }

        Vector3[] vertexArray = trackMeshVertices.ToArray();
        int[] trisArray = trackMeshTris.ToArray();

        Mesh trackMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = trackMesh;
        GetComponent<MeshCollider>().sharedMesh = trackMesh;
        trackMesh.vertices = vertexArray;
        trackMesh.triangles = trisArray;

        Debug.Log(trisArray.Length / 3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
