using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float force;
    public float fullDamageSpeed;
    public float noDamageSpeed;
    public float speedLoss;
    public float maxDamage;
    Rigidbody2D body;
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space))
            body.AddForce(transform.up * force);
    }
    private void FixedUpdate() {
        CheckForCollisions();
    }
    void CheckForCollisions() {
        Vector2 dir = ((Vector3)body.velocity) * Time.fixedDeltaTime;
        Vector2 endPoint = (Vector2)transform.position + dir;
        Debug.DrawLine(transform.position, endPoint, Color.red);
        bool blocked = true;
        int breakout = 0;
        while (blocked && breakout < 25) {
            dir = ((Vector3)body.velocity) * Time.fixedDeltaTime;
            endPoint = (Vector2)transform.position + dir;
            blocked = false;
            breakout++;
            RaycastHit2D[] items = Physics2D.RaycastAll(transform.position, dir, Vector2.Distance(transform.position, endPoint));
            for(int i = 0; i < items.Length; i++) {
                GameObject hit = items[i].collider.gameObject;
                if (hit != gameObject) {
                    if (hit.name == "Walls") {
                        OnHitSpaceShip(hit.GetComponentInParent<Spaceship>(), items[i].point);
                        blocked = true;
                    }
                }
            }
        }
    }
    void OnHitSpaceShip(Spaceship ship, Vector2 point) {
        if (body.velocity.magnitude > noDamageSpeed) {
            float damagePercent = Mathf.InverseLerp(noDamageSpeed, fullDamageSpeed, body.velocity.magnitude);
            float damageSize = Mathf.Lerp(0, maxDamage, damagePercent);
            ship.DestroyInRadius(ship.LocalToArray(ship.WorldToLocal(point)), damageSize, 0.15f);
        }
        body.velocity = body.velocity.normalized * Mathf.Lerp(body.velocity.magnitude,0,speedLoss);
    }
}
