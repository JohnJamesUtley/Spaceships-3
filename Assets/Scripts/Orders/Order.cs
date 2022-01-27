using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Order {
    public string name;
    public List<Crew> assignedCrew;
    public List<Crew> blackListedCrew;
    public int neededCrew;
    public int priority;
    public CrewManager manager;
    public Order(string name, int neededCrew, int priority, CrewManager manager) {
        this.name = name;
        this.neededCrew = neededCrew;
        this.priority = priority;
        this.manager = manager;
        assignedCrew = new List<Crew>();
        blackListedCrew = new List<Crew>();
    }
    public virtual void CrewAssigned(Crew crew) {
        assignedCrew.Add(crew);
    }
    public virtual void PathingFailed(Crew crew) {
        Debug.Log("BlackList");
        blackListedCrew.Add(crew);
        manager.redoAssignmentsRequest = true;
    }
    public virtual void ArrivedFromPath(Crew crew) {}
    public virtual void FinishedInteraction(Crew crew) {}
    public void RemoveBlacklistedCrew() {
        List<Crew> assignedCrewBefore = new List<Crew>();
        foreach (Crew x in assignedCrew) {
            assignedCrewBefore.Add(x);
        }
        foreach (Crew x in assignedCrewBefore) {
            foreach (Crew y in blackListedCrew) {
                if (x == y)
                    CrewRemoved(x);
            }
        }
    }
    public virtual void CrewRemoved(Crew crew) {
        if(crew.interacting)
            crew.CancelInteracting();
        assignedCrew.Remove(crew);
        crew.currentOrder = new Order("", 0, -1, manager);
    }
    public virtual void FinishOrder() {
        manager.RemoveOrder(this);
    }
    public bool PassesBlacklist(Crew crew) {
        foreach(Crew x in blackListedCrew)
            if(x == crew)
                return false;
        return true;
    }
}
