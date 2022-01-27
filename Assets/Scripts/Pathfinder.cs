using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder {
    public List<Vector2> arrayPath;
    List<PathNode> openNodes;
    PathNode finalNode;
    PathNode[] arrayNodes;
    Spaceship ship;
    Vector2 finish;
    CrewManager crewManager;
    public Pathfinder(Vector2 arrayStart, Vector2 arrayFinish, CrewManager crewManager, Spaceship ship) {
        finalNode = null;
        this.ship = ship;
        finish = arrayFinish;
        this.crewManager = crewManager;
        arrayPath = new List<Vector2>();
        arrayNodes = new PathNode[ship.width * ship.height];
        PathNode startingNode = new PathNode(FindHCost(arrayStart, arrayFinish), 0, arrayStart, null);
        arrayNodes[ship.ArrayToIndex(arrayStart)] = startingNode;
        openNodes = new List<PathNode> {startingNode};
        while (openNodes.Count > 0) {
            PathNode nextNode = FindNextNode();
            if (ExplorePathNode(nextNode)) {
                finalNode = nextNode;
                break;
            };
        }
        if(finalNode == null) {
            Debug.LogWarning("No Path");
        } else {
            AddNodeToPath(finalNode);
        }
    }
    void AddNodeToPath(PathNode Node) {
        arrayPath.Insert(0, Node.pos);
        if(Node.origin != null) {
            AddNodeToPath(Node.origin);
        }
    }
    PathNode FindNextNode() {
        PathNode least = openNodes[0];
        for (int i = 0; i < openNodes.Count; i++) {
            if(openNodes[i].FCost < least.FCost) {
                least = openNodes[i];
            } else if (openNodes[i].FCost == least.FCost) {
                if(openNodes[i].HCost < least.HCost) {
                    least = openNodes[i];
                }
            }
        }
        return least;
    }
    bool ExplorePathNode(PathNode ToExplore) {
        FindConnectedNodes(ToExplore);
        openNodes.Remove(ToExplore);
        if(ToExplore.pos == finish) {
            return true;
        } else {
            return false;
        }
    }
    void FindConnectedNodes(PathNode origin) {
        //Adjacent
        if (origin.pos.x != ship.width - 1)
            if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(1, 0))])
                FindNode(origin.pos + new Vector2(1, 0), origin, 10);
        if (origin.pos.x != 0)
            if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(-1, 0))])
                FindNode(origin.pos + new Vector2(-1, 0), origin, 10);
        if (origin.pos.y != ship.height - 1)
            if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(0, 1))])
                FindNode(origin.pos + new Vector2(0, 1), origin, 10);
        if (origin.pos.y != 0)
            if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(0, -1))])
                FindNode(origin.pos + new Vector2(0, -1), origin, 10);
        //Corners
        //Up Right
        if (origin.pos.x != 0 && origin.pos.y != 0)
            if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(-1, -1))])
                if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(0, -1))] && crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(-1, 0))])
                    FindNode(origin.pos + new Vector2(-1, -1), origin, 14);
        //Down Left
        if (origin.pos.x != ship.width - 1 && origin.pos.y != ship.height - 1)
            if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(1, 1))])
                if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(0, 1))] && crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(1, 0))])
                    FindNode(origin.pos + new Vector2(1, 1), origin, 14);
        //Up Left
        if (origin.pos.x != 0 && origin.pos.y != ship.height - 1)
            if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(-1, 1))])
                if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(0, 1))] && crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(-1, 0))])
                    FindNode(origin.pos + new Vector2(-1, 1), origin, 14);
        //Down Right
        if (origin.pos.x != ship.width - 1 && origin.pos.y != 0)
            if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(1, -1))])
                if (crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(0, -1))] && crewManager.walkableTiles[ship.ArrayToIndex(origin.pos + new Vector2(1, 0))])
                    FindNode(origin.pos + new Vector2(1, -1), origin, 14);
    }
    void FindNode(Vector2 pos, PathNode origin, int addedGCost) {
        if (arrayNodes[ship.ArrayToIndex(pos)] == null) {
            PathNode newNode = new PathNode(FindHCost(pos, finish), origin.GCost + addedGCost, pos, origin);
            arrayNodes[ship.ArrayToIndex(pos)] = newNode;
            openNodes.Add(newNode);
        } else {
            int purposedGCost = origin.GCost + addedGCost;
            if (purposedGCost < arrayNodes[ship.ArrayToIndex(pos)].GCost) {
                arrayNodes[ship.ArrayToIndex(pos)].SetNewOrigin(purposedGCost, origin);
                FindConnectedNodes(arrayNodes[ship.ArrayToIndex(pos)]);
            }
        }
    }
    int FindHCost(Vector2 pos, Vector2 final) {
        Vector2 difference = pos - final;
        int HCost = 0;
        difference = new Vector2(Mathf.Abs(difference.x), Mathf.Abs(difference.y));
        while (difference.x > 0 && difference.y > 0) {
            difference = new Vector2(difference.x - 1, difference.y - 1);
            HCost += 14;
        }
        HCost += (int)(difference.x * 10) + (int)(difference.y * 10);
        return HCost;
    }
    class PathNode {
        public int HCost;
        public int GCost;
        public int FCost;
        public Vector2 pos;
        public PathNode origin;
        public PathNode(int HCost, int GCost, Vector2 pos, PathNode origin) {
            this.HCost = HCost;
            this.GCost = GCost;
            FCost = HCost + GCost;
            this.pos = pos;
            this.origin = origin;
        }
        public void SetNewOrigin(int GCost, PathNode origin) {
            this.origin = origin;
            this.GCost = GCost;
            FCost = HCost + GCost;
        }
    }
}
