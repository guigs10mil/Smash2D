using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FollowTargetGroupElements : MonoBehaviour
{
    public GameObject objectToFollow;
    public Vector2 boundSize = Vector2.one;
    public float weight = 0;

    bool objectAssigned = false;

    void Update()
    {
        if (objectToFollow) {
            Vector3 newPosition = objectToFollow.transform.position;

            if (newPosition.x > boundSize.x/2 - weight) {
                newPosition.x = boundSize.x/2 - weight;
            }
            if (newPosition.x < -boundSize.x/2 + weight) {
                newPosition.x = -boundSize.x/2 + weight;
            }
            if (newPosition.y > boundSize.y/2 - weight) {
                newPosition.y = boundSize.y/2 - weight;
            }
            if (newPosition.y < -boundSize.y/2 + weight) {
                newPosition.y = -boundSize.y/2 + weight;
            }

            transform.position = newPosition;

            objectAssigned = true;
        }
        
        else if (objectAssigned) {
            Destroy(gameObject);
        }
    }
}
