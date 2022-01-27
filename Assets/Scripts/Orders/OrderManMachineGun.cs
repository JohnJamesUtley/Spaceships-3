using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManMachineGun : Order
{
    //public string name;
    //public List<Crew> assignedCrew;
    //public int neededCrew;
    //public int priority;
    //public CrewManager manager;
    ModuleMachineGun gun;
    bool[] openAccessPoints;
    List<CrewPosCombo> crewPosCombos;
    public OrderManMachineGun(string name, int neededCrew, int priority, CrewManager manager, ModuleMachineGun gun) : base(name, neededCrew, priority, manager) {
        this.gun = gun;
        openAccessPoints = new bool[gun.crewPos.Count];
        for (int i = 0; i < openAccessPoints.Length; i++)
            openAccessPoints[i] = true;
        crewPosCombos = new List<CrewPosCombo>();
    }
    public override void CrewAssigned(Crew crew) {
        base.CrewAssigned(crew);
        for (int i = 0; i < openAccessPoints.Length; i++) {
            if (openAccessPoints[i]) {
                Debug.Log("Machine Gun Path");
                crew.MoveToPos(gun.ship.LocalToArray(gun.ship.WorldToLocal(gun.crewPos[i].transform.position)));
                crewPosCombos.Add(new CrewPosCombo(i, crew));
                openAccessPoints[i] = false;
                break;
            }
        }
    }
    public override void CrewRemoved(Crew crew) {
        foreach (CrewPosCombo x in crewPosCombos)
            if (x.crew == crew)
                openAccessPoints[x.accessPoint] = true;
        if (crew.interacting) {
            gun.mannedPositions -= 1;
        }
        base.CrewRemoved(crew);
    }
    public override void ArrivedFromPath(Crew crew) {
        if (!crew.interacting) {
            crew.Interact(gun.gameObject);
            gun.mannedPositions += 1;
        }
    }
    public override void FinishOrder() {
        base.FinishOrder();
    }
    class CrewPosCombo {
        public int accessPoint;
        public Crew crew;
        public CrewPosCombo(int accessPoint, Crew crew) {
            this.accessPoint = accessPoint;
            this.crew = crew;
        }
    }
}
