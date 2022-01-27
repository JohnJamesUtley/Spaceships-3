using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderRepairModule : Order
{
    //public string name;
    //public List<Crew> assignedCrew;
    //public int neededCrew;
    //public int priority;
    //public CrewManager manager;
    Module toRepair;
    public OrderRepairModule(string name, int neededCrew, int priority, CrewManager manager, Module toRepair) : base(name, neededCrew, priority, manager) {
        this.toRepair = toRepair;
    }
    public override void CrewAssigned(Crew crew) {
        base.CrewAssigned(crew);
        Debug.Log("Repair Path");
        crew.MoveToPos(toRepair.ship.LocalToArray(toRepair.ship.WorldToLocal(toRepair.crewPos[0].transform.position)));
    }
    public override void CrewRemoved(Crew crew) {
        base.CrewRemoved(crew);
    }
    public override void ArrivedFromPath(Crew crew) {
        if (!crew.interacting) {
            crew.Interact(toRepair.gameObject, toRepair.repairTime);
        }
    }
    public override void FinishedInteraction(Crew crew) {
        toRepair.UnDestroy();
        FinishOrder();
    }
    public override void FinishOrder() {
        base.FinishOrder();
    }

}
