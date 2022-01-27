using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewManager : MonoBehaviour
{
    Spaceship ship;
    List<Crew> crew;
    public bool[] walkableTiles;
    public List<Order> orders;
    public bool redoAssignmentsRequest;
    private void Awake() {
        ship = GetComponent<Spaceship>();
    }
    private void Start() {
        crew = new List<Crew>();
        orders = new List<Order>();
    }
    public void AddOrder(Order newOrder) {
        bool inserted = false;
        for(int i = 0; i < orders.Count; i++) {
            if (orders[i].priority <= newOrder.priority) {
                orders.Insert(i, newOrder);
                inserted = true;
                break;
            }
        }
        if (!inserted) {
            orders.Add(newOrder);
        }
        UpdateAssignments();
    }
    public void RemoveOrder(Order toRemove) {
        toRemove.name = "";
        orders.Remove(toRemove);
        UpdateAssignments();
    }
    public void UpdateAssignments() {
        redoAssignmentsRequest = false;
        List<Crew> avaliableCrew = new List<Crew>();
        foreach (Crew x in crew) {
            if (x.currentOrder.name == "")
                avaliableCrew.Add(x);
        }
        for (int i = 0; i < orders.Count; i++) {
            int currentlyAssigned = orders[i].assignedCrew.Count;
            for (int k = 0; k < orders[i].neededCrew - currentlyAssigned; k++) {
                bool assignedAvaliableCrew = false;
                foreach (Crew x in avaliableCrew) {
                    if (orders[i].PassesBlacklist(x)) {
                        x.SetOrder(orders[i]);
                        avaliableCrew.Remove(x);
                        assignedAvaliableCrew = true;
                        break;
                    }
                }
                if (!assignedAvaliableCrew) {
                    for (int j = orders.Count - 1; j >= 0; j--) {
                        if (orders[j].priority < orders[i].priority) {
                            bool assigned = false;
                            foreach (Crew x in orders[j].assignedCrew) {
                                if (orders[i].PassesBlacklist(x)) {
                                    x.SetOrder(orders[i]);
                                    assigned = true;
                                    break;
                                }
                            }
                            if (assigned)
                                break;
                        } else {
                            break;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < orders.Count; i++) {
            orders[i].RemoveBlacklistedCrew();
        }
        if (redoAssignmentsRequest)
            UpdateAssignments();
    }
    void ClearBlacklists() {
        Debug.LogError("BlackList Cleared");
        foreach (Order x in orders)
            x.blackListedCrew.Clear();
    }
    public void AddCrewMember(Vector2 arrayPos) {
        Crew newCrew = Instantiate(Resources.Load<GameObject>("Crew")).GetComponent<Crew>();
        crew.Add(newCrew);
        newCrew.transform.parent = transform.GetChild(3);
        newCrew.transform.localPosition = ship.ArrayToLocal(arrayPos);
        newCrew.ship = ship;
        newCrew.crewManager = this;
        UpdateAssignments();
    }
    public void MoveAllCrewMembers(Vector2 arrayPos) {
        foreach (Crew x in crew) {
            x.MoveToPos(arrayPos);
        }
    }
    public void RecalculatePaths() {
        ClearBlacklists();
        foreach (Crew x in crew) {
            x.MoveToPos(x.arrayPathMovementTarget);
        }
        UpdateAssignments();
    }
}
