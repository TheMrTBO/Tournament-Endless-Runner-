using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    private GameManager gm;

    void Start(){
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();//get the object withe the tag
    }
    void OnTriggerEnter(Collider other)
    {
    if (other.tag == "Player")//to check if it's the player that collides
    {
        gm.lastCheckPointPos = transform.position;
    }
    }
}
