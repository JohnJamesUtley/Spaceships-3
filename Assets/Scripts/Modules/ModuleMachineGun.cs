using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleMachineGun : Module
{
    public int mannedPositions;
    Order orderToMan;
    float timeAlive;
    float gunSpeed = 1;
    GameObject target;
    private void Update() {
        timeAlive += Time.deltaTime;
        if(mannedPositions == 2) {
            if(target == null) {
                HuntForTarget();
            } else {

            }
        }
    }
    public override void OnCreation() {
        orderToMan = new OrderManMachineGun("Man Machine Gun", 2, 3, crewManager, this);
        crewManager.AddOrder(orderToMan);
    }
    public override void Destroy() {
        base.Destroy();
        orderToMan.FinishOrder();
    }
    public override void UnDestroy() {
        base.UnDestroy();
        orderToMan = new OrderManMachineGun("Man Machine Gun", 2, 3, crewManager, this);
        crewManager.AddOrder(orderToMan);
    }
    void HuntForTarget() {
        transform.GetChild(1).localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Sin(timeAlive * gunSpeed) * 30));
    }
}
