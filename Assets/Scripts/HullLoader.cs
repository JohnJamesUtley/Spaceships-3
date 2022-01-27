using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Spaceship))]
public class HullLoader : MonoBehaviour {
    public Texture2D hullMap;
    Spaceship ship;
    public Color walls;
    public Color floors;
    private void Awake() {
        ship = GetComponent<Spaceship>();
    }
    void Start() {
        LoadHull();
    }
    void LoadHull() {
        int width = hullMap.width;
        int height = hullMap.height;
        ship.SetMapSize(width, height);
        Color[] pixels = hullMap.GetPixels();
        bool[] wallarray = new bool[pixels.Length];
        bool[] floorarray = new bool[pixels.Length];
        bool[] constructarray = new bool[pixels.Length];
        for (int i = 0; i < pixels.Length; i++) {
            constructarray[i] = false;
            if (pixels[i] == walls) {
                wallarray[i] = true;
                constructarray[i] = true;
            } else {
                wallarray[i] = false;
            }
            if (pixels[i] == floors) {
                floorarray[i] = true;
                constructarray[i] = true;
            } else {
                floorarray[i] = false;
            }
        }
        ship.SetShipMap(wallarray, floorarray, constructarray);
    }
}
