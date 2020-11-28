using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player" && !col.gameObject.Equals(transform.parent.gameObject)) {
            col.GetComponent<Player>().TakeDamage(transform.parent.gameObject, 0.05f, 1);
        }
    }
}
