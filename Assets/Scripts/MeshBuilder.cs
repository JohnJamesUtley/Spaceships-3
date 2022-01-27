using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MeshBuilder : MonoBehaviour
{
    public int width;
    public int height;
    public bool[] meshArray;
    public int tiles;
    int nextVertID;
    List<Vector3> verts;
    List<int> tris;
    List<EdgePoints> brokenEdges;
    float dist;
    List<EdgeCollider2D> colliders;
    public void SetMeshSize(int width, int height) {
        this.width = width;
        this.height = height;
        meshArray = new bool[width * height];
    }
    public void ApplyMeshLayout(bool[] meshArray, float dist) {
        this.dist = dist;
        if (meshArray.Length == this.meshArray.Length) {
            for (int i = 0; i < meshArray.Length; i++)
                this.meshArray[i] = meshArray[i];
        } else {
            Debug.LogError("Mesh Application Failed -- Array Lengths Don't Match");
        }
        ConstructMesh();
    }
    public void ApplyCollidersLayout(bool[] meshArray, float dist) {
        this.dist = dist;
        if (meshArray.Length == this.meshArray.Length) {
            for (int i = 0; i < meshArray.Length; i++)
                this.meshArray[i] = meshArray[i];
        } else {
            Debug.LogError("Mesh Application Failed -- Array Lengths Don't Match");
        }
        ConstructColliders();
    }
    void ConstructMesh() {
        Mesh shipMesh = new Mesh();
        nextVertID = 0;
        verts = new List<Vector3>();
        tris = new List<int>();
        for (int y = height - 1; y >= 1; y--) {
            for (int x = 0; x < width - 1; x++) {
                string code = AcquireCode(x, y);
                tris.AddRange(MarchingSquare(code, new Vector2(x * dist - dist * width / 2f, y * dist - dist * height / 2f)));
            }
        }
        Vector2[] uvs = new Vector2[verts.Count];
        for(int i = 0; i < verts.Count; i++) {
            float percentX = Mathf.InverseLerp(-height * dist / 2, height * dist / 2, verts[i].x) * tiles;
            float percentY = Mathf.InverseLerp(-height * dist/2, height * dist/2, verts[i].y) * tiles;
            uvs[i] = new Vector2(percentX,percentY);
        }
        shipMesh.vertices = verts.ToArray();
        shipMesh.triangles = tris.ToArray();
        shipMesh.uv = uvs;
        GetComponent<MeshFilter>().mesh = shipMesh;
    }
    void ConstructColliders() {
        nextVertID = 0;
        brokenEdges = new List<EdgePoints>();
        verts = new List<Vector3>();
        brokenEdges = new List<EdgePoints>();
        for (int y = height - 1; y >= 1; y--) {
            for (int x = 0; x < width - 1; x++) {
                string code = AcquireCode(x, y);
                ColliderMarchingSquare(code, new Vector2(x * dist - dist * width / 2f, y * dist - dist * height / 2f));
            }
        }
        CreateColliders();
    }
    void CreateColliders() {
        if(colliders != null)
            foreach(EdgeCollider2D x in colliders) {
                GameObject.Destroy(x);
            }
        colliders = new List<EdgeCollider2D>();
        while (brokenEdges.Count > 0) {
            bool LineComplete = false;
            EdgePoints seed = brokenEdges[0];
            while (!LineComplete) {
                bool addedALine = false;
                for (int i = 0; i < brokenEdges.Count; i++) {
                    if (!brokenEdges[i].Equals(seed)) {
                        if (seed.AddLine(brokenEdges[i], verts)) {
                            brokenEdges.Remove(brokenEdges[i]);
                            addedALine = true;
                            break;
                        }
                    }
                }
                if (!addedALine)
                    LineComplete = true;
            }
            EdgeCollider2D test = gameObject.AddComponent<EdgeCollider2D>();
            //test.isTrigger = true;
            colliders.Add(test);
            List<Vector2> linePoints = new List<Vector2>();
            for (int i = 0; i < brokenEdges[0].points.Count; i++) {
                linePoints.Add(verts[brokenEdges[0].points[i]]);
            }
            brokenEdges.RemoveAt(0);
            test.points = linePoints.ToArray();
        }
    }
    class EdgePoints {
        public List<int> points;
        public EdgePoints(List<int> points) {
            this.points = points;
        }
        public bool AddLine(EdgePoints added, List<Vector3> verts) {
            if(verts[added.points[0]] == verts[points[points.Count - 1]]) {
                points.Add(added.points[1]);
                return true;
            }
            if (verts[added.points[1]] == verts[points[0]]) {
                points.Insert(0, added.points[0]);
                return true;
            }
            if (verts[added.points[1]] == verts[points[points.Count - 1]]) {
                points.Add(added.points[0]);
                return true;
            }
            if (verts[added.points[0]] == verts[points[0]]) {
                points.Insert(0, added.points[1]);
                return true;
            }
            return false;
        }
    }
    void AddVert(Vector3 pos) {
        verts.Add(pos);
        nextVertID++;
    }
    int[] MarchingSquare(string code, Vector2 worldPos) {
        int rotation = 0;
        int[] localTris = null;
        bool localTrisSet = false;
        int loop = 0;
        while (!localTrisSet) {
            loop++;
            switch (code) {
                case "0000":
                    localTris = CreateTris(new int[] { }, rotation , worldPos);
                    localTrisSet = true;
                    break;
                case "1000":
                    localTris = CreateTris(new int[] { 0, 1, 7 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
                case "1001":
                    localTris = CreateTris(new int[] { 0, 1, 6, 5, 6, 1 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
                case "1010":
                    localTris = CreateTris(new int[] { 0, 1, 3, 0, 3, 4, 0, 4, 5, 0, 5, 7 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
                case "1011":
                    localTris = CreateTris(new int[] { 0, 4, 6, 0, 1, 4, 1, 3, 4 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
                case "1111":
                    localTris = CreateTris(new int[] { 0, 2, 6, 2, 4, 6 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
            }
            if (!localTrisSet) {
                code = RotateCode(code);
                rotation += 2;
            }
        }
        return localTris;
    }
    int[] CreateTris(int[] original, int rot, Vector2 worldPos) {
        int[] rotated = new int[original.Length];
        for (int i = 0; i < original.Length; i++) {
            rotated[i] = original[i] + rot;
            if (rotated[i] > 7)
                rotated[i] -= 8;
        }
        List<int> tris = new List<int>();
        for (int i = 0; i < rotated.Length; i++) {
            tris.Add(nextVertID);
            switch (rotated[i]) {
                case 0:
                    AddVert(new Vector3(worldPos.x, worldPos.y, 0)); // 0
                    break;
                case 1:
                    AddVert(new Vector3(worldPos.x + 0.5f * dist, worldPos.y, 0)); // 1
                    break;
                case 2:
                    AddVert(new Vector3(worldPos.x + dist, worldPos.y, 0)); // 2
                    break;
                case 3:
                    AddVert(new Vector3(worldPos.x + dist, worldPos.y - 0.5f * dist, 0)); // 3
                    break;
                case 4:
                    AddVert(new Vector3(worldPos.x + dist, worldPos.y - dist, 0)); // 4
                    break;
                case 5:
                    AddVert(new Vector3(worldPos.x + 0.5f * dist, worldPos.y - dist, 0)); // 5
                    break;
                case 6:
                    AddVert(new Vector3(worldPos.x, worldPos.y - dist, 0)); // 6
                    break;
                case 7:
                    AddVert(new Vector3(worldPos.x, worldPos.y - 0.5f * dist, 0)); // 7
                    break;
                default:
                    Debug.LogError("Invalid Vert");
                    break;
            }
        }
        return tris.ToArray();
    }
    void ColliderMarchingSquare(string code, Vector2 worldPos) {
        int rotation = 0;
        bool localTrisSet = false;
        int loop = 0;
        while (!localTrisSet) {
            loop++;
            switch (code) {
                case "0000":
                    localTrisSet = true;
                    break;
                case "1000":
                    SetEdges(new int[] { 1, 7 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
                case "1001":
                    SetEdges(new int[] { 1, 5 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
                case "1010":
                    SetEdges(new int[] { 1, 3, 7, 5 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
                case "1011":
                    SetEdges(new int[] { 1, 3 }, rotation, worldPos);
                    localTrisSet = true;
                    break;
                case "1111":
                    localTrisSet = true;
                    break;
            }
            if (!localTrisSet) {
                code = RotateCode(code);
                rotation += 2;
            }
        }
    }
    void SetEdges(int[] edgeVerts, int rot, Vector2 worldPos) {
        List<int> createdEdges = new List<int>();
        int[] rotatedEdge = new int[edgeVerts.Length];
        for (int i = 0; i < edgeVerts.Length; i++) {
            rotatedEdge[i] = edgeVerts[i] + rot;
            if (rotatedEdge[i] > 7)
                rotatedEdge[i] -= 8;
        }
        for (int i = 0; i < rotatedEdge.Length; i++) {
            createdEdges.Add(nextVertID);
            switch (rotatedEdge[i]) {
                case 0:
                    AddVert(new Vector3(worldPos.x, worldPos.y, 0)); // 0
                    break;
                case 1:
                    AddVert(new Vector3(worldPos.x + 0.5f * dist, worldPos.y, 0)); // 1
                    break;
                case 2:
                    AddVert(new Vector3(worldPos.x + dist, worldPos.y, 0)); // 2
                    break;
                case 3:
                    AddVert(new Vector3(worldPos.x + dist, worldPos.y - 0.5f * dist, 0)); // 3
                    break;
                case 4:
                    AddVert(new Vector3(worldPos.x + dist, worldPos.y - dist, 0)); // 4
                    break;
                case 5:
                    AddVert(new Vector3(worldPos.x + 0.5f * dist, worldPos.y - dist, 0)); // 5
                    break;
                case 6:
                    AddVert(new Vector3(worldPos.x, worldPos.y - dist, 0)); // 6
                    break;
                case 7:
                    AddVert(new Vector3(worldPos.x, worldPos.y - 0.5f * dist, 0)); // 7
                    break;
                default:
                    Debug.LogError("Invalid Vert");
                    break;
            }
        }
        for (int i = 0; i < createdEdges.Count; i += 2) {
            brokenEdges.Add(new EdgePoints(new List<int>() { createdEdges[i], createdEdges[i + 1] }));
        }
    }
    string AcquireCode(int x, int y){
        string code = "";
        if (meshArray[y * width + x]) {
            code += "1";
        } else {
            code += "0";
        }
        if (meshArray[y * width + x + 1]) {
            code += "1";
        } else {
            code += "0";
        }
        if (meshArray[(y - 1) * width + x + 1]) {
            code += "1";
        } else {
            code += "0";
        }
        if (meshArray[(y - 1) * width + x]) {
            code += "1";
        } else {
            code += "0";
        }
        return code;
    }
    string RotateCode(string code) {
        code = code + code[0];
        string mod = "";
        for (int i = 1; i < 5; i++) {
            mod += code[i];
        }
        return mod;
    }
}
