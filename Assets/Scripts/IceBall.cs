using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBall : MonoBehaviour
{
    public int parentIndex;
    public float dir;

    public float MoveSpeed = 8.0f;
 
    private float frequency = 15f;  // Speed of sine movement
    private float magnitude = 0.15f;   // Size of sine movement
    private Vector3 axis;
    private Vector3 pos;
    private float initialTime;

    void Start () {
        pos = transform.position;
        axis = transform.up;  // May or may not be the axis you want
        initialTime = Time.time;
    }

    void Update () {
        pos += transform.right * Time.deltaTime * MoveSpeed * dir;
        transform.position = pos + axis * Mathf.Sin ((Time.time - initialTime) * frequency) * magnitude;
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            Player p = col.GetComponent<Player>();
            if(p.user.index != parentIndex){
                p.TakeDamageFromIceBall(0.1f);
                Destroy(gameObject);
            }
        }
    }

}
