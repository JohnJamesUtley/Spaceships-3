using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]
[RequireComponent(typeof(CrewManager))]
public class Spaceship : MonoBehaviour {
    public TileStatus[] shipMap;
    public bool draw;
    public bool crewDraw;
    public float mapSpacing;
    public int width;
    public int height;
    MeshBuilder wallMeshBuilder;
    MeshBuilder floorMeshBuilder;
    MeshBuilder constructMeshBuilder;
    CrewManager crewManager;
    ModuleManager moduleManager;
    private void Awake() {
        wallMeshBuilder = transform.GetChild(0).gameObject.GetComponent<MeshBuilder>();
        floorMeshBuilder = transform.GetChild(1).gameObject.GetComponent<MeshBuilder>();
        constructMeshBuilder = transform.GetChild(2).gameObject.GetComponent<MeshBuilder>();
        crewManager = GetComponent<CrewManager>();
        moduleManager = GetComponentInChildren<ModuleManager>();
    }
    private void Update() {
        Drawing();
        UpdateMeshes();
    }
    bool wallUpdate, floorUpdate;
    void UpdateMeshes() {
        if (wallUpdate) {
            wallMeshBuilder.ApplyMeshLayout(WallMap(), mapSpacing);
        }
        if (floorUpdate) {
            floorMeshBuilder.ApplyMeshLayout(FloorMap(), mapSpacing);
        }
        if(wallUpdate || floorUpdate) {
            crewManager.RecalculatePaths();
        }
        wallUpdate = false;
        floorUpdate = false;
    }
    public void SetShipMap(bool[] walls, bool[] floors, bool[] construct){
        for (int i = 0; i < walls.Length; i++) {
            if(walls[i])
                ChangeTile(i, TileStatus.Wall);
            if (floors[i]) {
                ChangeTile(i, TileStatus.Floor);
            }
        }
        wallMeshBuilder.ApplyMeshLayout(walls, mapSpacing);
        wallMeshBuilder.ApplyCollidersLayout(walls, mapSpacing);
        floorMeshBuilder.ApplyMeshLayout(floors, mapSpacing);
        constructMeshBuilder.ApplyMeshLayout(construct, mapSpacing);
    }
    public void SetMapSize(int width, int height) {
        this.height = height;
        this.width = width;
        shipMap = new TileStatus[width * height];
        crewManager.walkableTiles = new bool[width * height];
        wallMeshBuilder.SetMeshSize(width, height);
        floorMeshBuilder.SetMeshSize(width, height);
        constructMeshBuilder.SetMeshSize(width, height);
        moduleManager.SetModMapSize(width, height);
        for (int i = 0; i < shipMap.Length; i++) {
            ChangeTile(i, TileStatus.None);
        }
    }
    bool[] WallMap() {
        bool[] walls = new bool[shipMap.Length];
        for(int i = 0; i < walls.Length; i++) {
            if(shipMap[i] == TileStatus.Wall) {
                walls[i] = true;
            } else {
                walls[i] = false;
            }
        }
        return walls;
    }
    bool[] FloorMap() {
        bool[] floor = new bool[shipMap.Length];
        for (int i = 0; i < floor.Length; i++) {
            if (shipMap[i] == TileStatus.Floor) {
                floor[i] = true;
            } else {
                floor[i] = false;
            }
        }
        return floor;
    }
    void Drawing() {
        if (Input.GetKeyDown(KeyCode.C)) {
            if(draw == false) {
                draw = true;
            } else {
                draw = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            if (crewDraw == false) {
                crewDraw = true;
            } else {
                crewDraw = false;
            }
        }
        if (draw) {
            if (Input.GetMouseButton(0)) {
                    Vector2 mousePos = (LocalToArray(WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition))));
                    ChangeTile(mousePos, TileStatus.Wall);
                    wallMeshBuilder.ApplyCollidersLayout(WallMap(), mapSpacing);
                    wallUpdate = true;
                    floorUpdate = true;
            }
            if (Input.GetMouseButton(1)) {
                    Vector2 mousePos = (LocalToArray(WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition))));
                    ChangeTile(mousePos, TileStatus.Floor);
                    wallMeshBuilder.ApplyCollidersLayout(WallMap(), mapSpacing);
                    wallUpdate = true;
                    floorUpdate = true;
            }
                if (Input.GetMouseButton(2)) {
                    Vector2 mousePos = (LocalToArray(WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition))));
                    DestroyTile(mousePos);
                    wallMeshBuilder.ApplyCollidersLayout(WallMap(), mapSpacing);
                    wallUpdate = true;
                    floorUpdate = true;
                }
        }
        if (crewDraw) {
            if (Input.GetMouseButtonDown(0)) {
                Vector2 mousePos = (LocalToArray(WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition))));
                if (shipMap[ArrayToIndex(mousePos)] == TileStatus.Floor)
                    crewManager.AddCrewMember(mousePos);
            }
            if (Input.GetMouseButtonDown(1)) {
                Vector2 mousePos = (LocalToArray(WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition))));
                if (shipMap[ArrayToIndex(mousePos)] == TileStatus.Floor)
                    crewManager.MoveAllCrewMembers(mousePos);
            }
        }
    }
    public void DestroyInRadius(Vector2 pos, float dist, float chance) {
        List<Vector2> nearby = GetNearbyTiles(pos, dist);
        for (int i = 0; i < nearby.Count; i++) {
            DestroyTile(nearby[i]);
        }
        List<Vector2> nearbyChanced = GetNearbyTiles(pos, dist + 1);
        for (int i = 0; i < nearbyChanced.Count; i++) {
            if (Random.value < chance) {
                DestroyTile(nearbyChanced[i]);
            }
        }
        wallMeshBuilder.ApplyCollidersLayout(WallMap(), mapSpacing);
        wallUpdate = true;
        floorUpdate = true;
    }
    public void DestroyInRadius(Vector2 pos, float dist) {
        List<Vector2> nearby = GetNearbyTiles(pos, dist);
        for (int i = 0; i < nearby.Count; i++) {
            DestroyTile(nearby[i]);
        }
        wallMeshBuilder.ApplyCollidersLayout(WallMap(), mapSpacing);
        wallUpdate = true;
    }
    void DestroyTile(Vector2 arrayPos) {
        if (shipMap[ArrayToIndex(arrayPos)] == TileStatus.Wall) {
            ChangeTile(arrayPos, TileStatus.DamagedWall);
        } else if (shipMap[ArrayToIndex(arrayPos)] == TileStatus.Floor) {
            ChangeTile(arrayPos, TileStatus.DamagedFloor);
        }
    }

    void ChangeTile(Vector2 arrayPos, TileStatus newStatus) {
        ChangeTile(ArrayToIndex(arrayPos), newStatus);
    }
    void ChangeTile(int index, TileStatus newStatus) {
        shipMap[index] = newStatus;
        switch (newStatus) {
            case TileStatus.Wall:
                crewManager.walkableTiles[index] = false;
                break;
            case TileStatus.Floor:
                crewManager.walkableTiles[index] = true;
                break;
            case TileStatus.DamagedWall:
                crewManager.walkableTiles[index] = false;
                break;
            case TileStatus.DamagedFloor:
                crewManager.walkableTiles[index] = false;
                break;
            case TileStatus.None:
                crewManager.walkableTiles[index] = false;
                break;
        }
    }
    List<Vector2> GetNearbyTiles(int xPos, int yPos, float dist) {
        List<Vector2> nearby = new List<Vector2>();
        int distTesting = Mathf.RoundToInt(dist) + 1;
        for (int y = -distTesting; y < distTesting + 1; y++) {
            for (int x = -distTesting; x < distTesting + 1; x++) {
                    Vector2 pointTested = new Vector2(xPos + x, yPos + y);
                if (pointTested.x > 0 && pointTested.x < width && pointTested.y > 0 && pointTested.y < height) {
                    if (Vector2.Distance(pointTested, new Vector2(xPos, yPos)) <= dist)
                        nearby.Add(pointTested);
                }
            }
        }
        return nearby;
    }
    List<Vector2> GetNearbyTiles(Vector2 pos, float dist) {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);
        return GetNearbyTiles(x, y, dist);
    }
    public Vector2 WorldToLocal(Vector2 world) {
        return transform.InverseTransformPoint(world);
    }
    public Vector2 LocalToWorld(Vector2 local) {
        return transform.TransformPoint(local);
    }
    public Vector2 LocalToArray(Vector2 local) {
        float x = local.x;
        if (width % 2 == 1) {
            x += mapSpacing * 0.5f;
        }
        float y = local.y;
        if(height % 2 == 1) {
            y += mapSpacing * 0.5f;
        }
        Vector2 unrounded = (new Vector2(x, y) / mapSpacing);
        x = Mathf.RoundToInt(unrounded.x);
        y = Mathf.RoundToInt(unrounded.y);
        return new Vector2(x, y) + new Vector2((width / 2), (height / 2));
    }
    public Vector2 ArrayToLocal(Vector2 array) {
        float x = array.x;
        if (width % 2 == 1) {
            x -= 0.5f;
        }
        float y = array.y;
        if (height % 2 == 1) {
            y -= 0.5f;
        }
        Vector2 adjusted = new Vector2(x, y);
        return (adjusted - new Vector2((width / 2), (height / 2))) * mapSpacing;
    }
    public int CordsToIndex(int x, int y) {
        return y * width + x;
    }
    public int ArrayToIndex(Vector2 pos) {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);
        if (x >= 0 && x < width && y >= 0 && y < height) {
            return CordsToIndex(x, y);
        }else {
            //Debug.LogWarning("Out of Bounds - Default 0");
            return 0;
        }
    }
}
public enum TileStatus { None, Wall, Floor, DamagedWall, DamagedFloor, Object }

