using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour
{
    public ModuleManager manager;
    public Spaceship ship;
    public CrewManager crewManager;
    public string modName;
    public Vector2 arrayOffset;
    public Vector2 dimensions;
    public List<TileStatus> placementRequirements;
    public Vector2 arrayPos;
    public int neededCrewPos;
    public float repairTime;
    public List<GameObject> crewPos;
    public bool placed = false;
    public bool destroyed = false;
    private void Start() {
        SetArrayOffset();
    }
    public void SetCrewPos() {
        Transform[] crewPosArray = gameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform x in crewPosArray) {
            if (x.parent.gameObject.name == "Crew") {
                int index = ship.ArrayToIndex(ship.LocalToArray(ship.WorldToLocal(x.position)));
                if (placed) {
                    if (manager.modMap[index] == null && ship.shipMap[index] == TileStatus.Floor) {
                        crewPos.Add(x.gameObject);
                        manager.modMap[index] = this;
                    } else {
                        Destroy(x.gameObject);
                    }
                } else {
                    crewPos.Add(x.gameObject);
                }
            }
        }
    }
    public virtual void OnCreation() {}
    void SetArrayOffset(){
        Vector2 newOffset = dimensions * 0.5f;
        arrayOffset = new Vector2(newOffset.x - 0.5f, newOffset.y - 0.5f);
    }
    public virtual void Delete(){
        for (int y = 0; y < dimensions.y; y++) {
            for (int x = 0; x < dimensions.x; x++) {
                int index = ship.ArrayToIndex(new Vector2(x + arrayPos.x, y + arrayPos.y));
                manager.modMap[index] = null;
                if(ship.shipMap[index] == TileStatus.Floor)
                    crewManager.walkableTiles[index] = true;
            }
        }
        crewManager.RecalculatePaths();
        manager.allModules.Remove(this);
        Destroy(gameObject);
    }
    public virtual void Destroy() {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.1f, 0, 0.75f);
        SpriteRenderer[] childRends = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer x in childRends) {
            x.color = new Color(0.7f, 0.1f, 0, 0.75f);
        }
        crewManager.AddOrder(new OrderRepairModule("Repair " + modName, 1, 5, crewManager, this));
        destroyed = true;
    }
    public virtual void UnDestroy() {
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        SpriteRenderer[] childRends = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer x in childRends) {
            x.color = Color.white;
        }
        destroyed = false;
    }
    public void Rotate() {
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, gameObject.transform.localRotation.eulerAngles.z - 90);
        Vector2 newDimensions = new Vector2(dimensions.y, dimensions.x);
        TileStatus[] newPlacementRequirements = new TileStatus[placementRequirements.Count];
        for (int y = 0; y < newDimensions.y; y++) {
            for (int x = 0; x < newDimensions.x; x++) {
                newPlacementRequirements[(int)(y * newDimensions.x + x)] = placementRequirements[(int)(x * dimensions.x + (newDimensions.y - y - 1))];
            }
        }
        placementRequirements = new List<TileStatus>();
        for (int i = 0; i < newPlacementRequirements.Length; i++)
            placementRequirements.Add(newPlacementRequirements[i]);
        dimensions = newDimensions;
        SetArrayOffset();
    }
}
