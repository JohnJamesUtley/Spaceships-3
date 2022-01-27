using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSpaceController : MonoBehaviour
{
    Rigidbody2D body;
    private void Start() {
        body = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) {
            body.AddForce(transform.up * 70000 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S)) {
            body.AddForce(transform.up * -70000 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A)) {
            body.AddTorque(-70 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D)) {
            body.AddTorque(70 * Time.deltaTime);
        }
    }
}
