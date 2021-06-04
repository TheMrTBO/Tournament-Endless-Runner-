using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPosition : MonoBehaviour
{
    private GameManager gm;
    public void NewPlayerPosition()
    {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        transform.position = gm.lastCheckPointPos;
    }
}
