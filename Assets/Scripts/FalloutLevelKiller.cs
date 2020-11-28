using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalloutLevelKiller : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Player") {
            col.GetComponent<Player>().Die();
        }

        if (col.tag == "IceBall"){
            Destroy(col.gameObject);
        }
    }
}
