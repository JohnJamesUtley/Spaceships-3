using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject target;
    public float camSpeed;
    float camSize = 5;
    public float camSizeSpeed;
    private void Update() {
        Camera.main.orthographicSize = camSize;
        //transform.position = Vector2.Lerp(transform.position, target.transform.position, camSpeed);
        if (Input.GetKey(KeyCode.UpArrow)) {
            camSize += Time.deltaTime * camSizeSpeed;
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            camSize -= Time.deltaTime * camSizeSpeed;
        }
    }
}
