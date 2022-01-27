using UnityEngine;

public class Crew : MonoBehaviour
{
    public Spaceship ship;
    public CrewManager crewManager;
    public Vector2 arrayMovementTarget;
    public Vector2 arrayPathMovementTarget;
    public float speed;
    public Order currentOrder;
    bool moving = false;
    Pathfinder path;
    private void Start() {
    }
    private void Update() {
        if (moving)
            MoveToTarget(arrayMovementTarget);
        if (interacting)
            InteractionStep();
    }
    public void SetOrder(Order newOrder) {
        currentOrder.CrewRemoved(this);
        currentOrder = newOrder;
        currentOrder.CrewAssigned(this);
    }
    public void MoveToPos(Vector2 arrayPos) {
        Debug.Log(arrayPos);
        moving = true;
        SetPath(ship.LocalToArray(ship.WorldToLocal(transform.position)), arrayPos);
        arrayPathMovementTarget = arrayPos;
    }
    public void SetPath(Vector2 arrayStart, Vector2 arrayFinish) {
        if (arrayStart != arrayFinish) {
            path = new Pathfinder(arrayStart, arrayFinish, crewManager, ship);
            if (path.arrayPath.Count > 0) {
                arrayMovementTarget = path.arrayPath[0];
            } else {
                moving = false;
                currentOrder.PathingFailed(this);
            }
        }
    }
    void MoveToTarget(Vector2 arrayPos) {
        Vector2 localTarget = ship.ArrayToLocal(arrayPos);
        if (Vector2.Distance(localTarget, transform.localPosition) < speed * Time.deltaTime) {
            GetNextTarget();
            return;
        }
        transform.localPosition += ((Vector3)localTarget - transform.localPosition).normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, ship.LocalToWorld(localTarget) - (Vector2)transform.position);
    }
    void GetNextTarget() {
        if(path.arrayPath.Count > 0) {
            arrayMovementTarget = path.arrayPath[0];
            path.arrayPath.RemoveAt(0);
        } else {
            if (currentOrder.name != "")
                currentOrder.ArrivedFromPath(this);
            moving = false;
        }
    }
    public bool interacting = false;
    float interactionTime;
    float interactionCompletionTime;
    public void Interact(GameObject toInteractWith) {
        interactionTime = 0;
        interactionCompletionTime = -1f;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, toInteractWith.transform.position - transform.position);
        interacting = true;
    }
    public void Interact(GameObject toInteractWith, float timeToComplete) {
        interactionTime = 0;
        interactionCompletionTime = timeToComplete;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, toInteractWith.transform.position - transform.position);
        interacting = true;
    }
    void InteractionStep() {
        interactionTime += Time.deltaTime;
        if (interactionCompletionTime != -1f) {
            if (interactionTime >= interactionCompletionTime) {
                FinishInteraction();
            }
        }
    }
    public void CancelInteracting() {
        interacting = false;
    }
    void FinishInteraction() {
        interacting = false;
        currentOrder.FinishedInteraction(this);
    }
}
