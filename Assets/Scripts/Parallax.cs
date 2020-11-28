using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{

    private float length;
    private float startpos;
    public GameObject cam;
    public float parallaxEffect;

    void Start(){
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate(){
        float distance = (cam.transform.position.x * parallaxEffect);
        transform.position = new Vector3(startpos + distance, transform.position.y, transform.position.z);
    }
    // float initCamPosX = 0f;
    // float initThisPosX = 0f;
    // public float intensity = 1f;
    // void Start()
    // {
    //     initCamPosX = Camera.main.transform.position.x;
    //     initThisPosX = transform.position.x;
    // }

    // void Update()
    // {
    //     transform.position = new Vector3((Camera.main.transform.position.x - initCamPosX) * intensity + initThisPosX, transform.position.y);
    // }
    
}

