using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundSpawner : MonoBehaviour
{
  //we will do a paralaxe effect
  private const float DISTANCE_TO_RESPAWN = 10.0f;

  public float scrollSpeed = -2f;
  public float totalLength;
  public bool IsScrolling {set; get;}

  private float scrollLocation;//how far are we in the player scroll
  private Transform playerTransform;//where is the player

  private void Start()
  {
    playerTransform = GameObject.FindGameObjectWithTag("Player").transform;//.transform because we don't need to know were the player is right now
  }

  private void Update()
  {
    if(!IsScrolling)
      return;

    scrollLocation += scrollSpeed * Time.deltaTime;
    Vector3 newLocation = (playerTransform.position.z + scrollLocation) * Vector3.forward; // * Vector3 to make sure is a vector3
    transform.position = newLocation;

    if(transform.GetChild(0).transform.position.z < playerTransform.position.z - DISTANCE_TO_RESPAWN)
    {
      transform.GetChild(0).localPosition += Vector3.forward * totalLength;
      transform.GetChild(0).SetSiblingIndex(transform.childCount);

      transform.GetChild(0).localPosition += Vector3.forward * totalLength;
      transform.GetChild(0).SetSiblingIndex(transform.childCount);
    }
  }

}
