using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleManager : MonoBehaviour
{
    Spaceship ship;
    CrewManager crewManager;
    public List<Module> allModules;
    public Module[] modMap;
    public bool modulePlacing;
    bool validPlacement;
    public List<GameObject> placableMods;
    int selectedModule = 0;
    bool placing;
    GameObject placer;
    Module modPlacer;
    SpriteRenderer rendPlacer;
    public Color colValid;
    public Color colInValid;
    void Start()
    {
        ship = GetComponentInParent<Spaceship>();
        crewManager = GetComponentInParent<CrewManager>();
    }
    void Update()
    {
        if (modulePlacing)
            PlaceModules();
    }
    public void SetModMapSize(int width, int height) {
        modMap = new Module[width * height];
        for(int i = 0; i < modMap.Length; i++) {
            modMap[i] = null;
        }
    }
    void PlaceModules() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (placing) {
                StopPlacing();
            } else {
                StartPlacing();
            }
        }
        if (placing) {
            Vector2 arrayPos = ship.LocalToArray(ship.WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            CheckPlacement(arrayPos);
            placer.transform.localPosition = (Vector3)ship.ArrayToLocal(arrayPos + modPlacer.arrayOffset) + Vector3.forward * -0.3f;
            if (Input.GetMouseButtonDown(0)) {
                Place();
            }
            if(Input.GetMouseButtonDown(1)){
                Delete();
            }
            if (Input.GetMouseButtonDown(2)) {
                Destroy();
            }
            if (Input.GetKeyDown(KeyCode.R)) {
                modPlacer.Rotate();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                SwitchPlaced(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                SwitchPlaced(1);
            }
        }
    }
    void Place() {
        if (validPlacement) {
            Vector2 arrayPos = ship.LocalToArray(ship.WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            GameObject newMod = Instantiate(placableMods[selectedModule]);
            newMod.transform.parent = transform;
            newMod.transform.localRotation = modPlacer.transform.localRotation;
            Module newModModule = newMod.GetComponent<Module>();
            allModules.Add(newModModule);
            newModModule.manager = this;
            newModModule.ship = ship;
            newModModule.arrayPos = arrayPos;
            newModModule.crewManager = crewManager;
            newModModule.placed = true;
            newMod.transform.localPosition = (Vector3)ship.ArrayToLocal(arrayPos + modPlacer.arrayOffset) + Vector3.forward * -0.3f;
            for (int y = 0; y < modPlacer.dimensions.y; y++) {
                for (int x = 0; x < modPlacer.dimensions.x; x++) {
                    int index = ship.ArrayToIndex(new Vector2(x + arrayPos.x, y + arrayPos.y));
                    modMap[index] = newModModule;
                    crewManager.walkableTiles[index] = false;
                }
            }
            crewManager.RecalculatePaths();
            newModModule.SetCrewPos();
            newModModule.OnCreation();
        } else {
            Debug.LogWarning("Invalid Placement");
        }
    }
    void Delete() {
        Vector2 arrayPos = ship.LocalToArray(ship.WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        int index = ship.ArrayToIndex(arrayPos);
        Module mod = modMap[index];
        if(mod != null) {
            mod.Delete();
        } else {
            Debug.LogWarning("Nothing To Delete");
        }
    }
    void Destroy() {
        Vector2 arrayPos = ship.LocalToArray(ship.WorldToLocal(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        int index = ship.ArrayToIndex(arrayPos);
        Module mod = modMap[index];
        if (mod != null) {
            mod.Destroy();
        } else {
            Debug.LogWarning("Nothing To Destroy");
        }
    }
    void StartPlacing() {
        placing = true;
        placer = Instantiate(placableMods[selectedModule]);
        placer.transform.parent = transform;
        placer.transform.localRotation = Quaternion.Euler(0, 0, 0);
        rendPlacer = placer.GetComponent<SpriteRenderer>();
        validPlacement = false;
        modPlacer = placer.GetComponent<Module>();
        modPlacer.ship = ship;
        modPlacer.SetCrewPos();
    }
    void SwitchPlaced(int selected) {
        selectedModule = selected;
        GameObject.Destroy(placer);
        StartPlacing();
    }
    void StopPlacing() {
        placing = false;
        Destroy(placer);
    }
    void CheckPlacement(Vector2 arrayPos) {
        bool valid = true;
        for(int y = 0; y < modPlacer.dimensions.y; y++) {
            for (int x = 0; x < modPlacer.dimensions.x; x++) {
                int index = ship.ArrayToIndex(new Vector2(x + arrayPos.x, y + arrayPos.y));
                if (ship.shipMap[index] != modPlacer.placementRequirements[(int)(y * modPlacer.dimensions.x + x)] || modMap[index] != null) {
                    valid = false;
                }
            }
        }
        int totalAvaliableCrewPos = 0;
        foreach (GameObject x in modPlacer.crewPos) {
            Vector2 pos = ship.LocalToArray(ship.WorldToLocal(x.transform.position));
            if(ship.shipMap[ship.ArrayToIndex(pos)] == TileStatus.Floor && modMap[ship.ArrayToIndex(pos)] == null) {
                totalAvaliableCrewPos++;
            }
        }
        if(totalAvaliableCrewPos < modPlacer.neededCrewPos) {
            valid = false;
        }
        if (valid) {
            rendPlacer.color = colValid;
            rendPlacer.sortingOrder = 1;
            SpriteRenderer[] rends = placer.transform.GetComponentsInChildren<SpriteRenderer>();
            foreach(SpriteRenderer x in rends) {
                if (x.transform.parent.name != "Crew") {
                    x.color = colValid;
                    x.sortingOrder = 1;
                }
            }
        } else {
            rendPlacer.color = colInValid;
            rendPlacer.sortingOrder = 1;
            SpriteRenderer[] rends = placer.transform.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer x in rends) {
                if (x.transform.parent.name != "Crew") {
                    x.color = colInValid;
                    x.sortingOrder = 1;
                }
            }
        }
        validPlacement = valid;
    }
}
