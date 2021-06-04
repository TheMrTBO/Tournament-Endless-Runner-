using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
  public float LANE_DISTANCE = 3f;
  public float TURN_SPEED = 0.005f;

  private bool isRunning = false;

  //Animation
  public Animator anim;

  // Movement
  public CharacterController controller;
  public float jumpForce = 4.0f;
  public float gravity = 12.0f;
  public float slideTiming = 0.5f;
  public float colliderDividerSlide = 4;
  public float verticalVelocity;
  public int desiredLane = 1 ;//0 = left, 1 = middle, 2 = right

  //Speed Modifier
  public float originalSpeed = 7.0f;
  private float speed;
  private float speedIncreaseLastTick;
  private float speedIncreaseTime = 2.5f;
  private float speedIncreaseAmount = 0.1f;

  void Start()
  {
    speed = originalSpeed;
    //For the character control
    controller = GetComponent<CharacterController>();
    //For the animation controller
    anim = GetComponent<Animator>();
  }

  void Update()
  {
    if(!isRunning)//tap to run
      return;
    if (Time.time - speedIncreaseLastTick > speedIncreaseTime)
    {
      speedIncreaseLastTick = Time.time;
      speed += speedIncreaseAmount;
      //change the modifierText
      GameManager.Instance.UpdateModifier(speed - originalSpeed);
    }

    //Gather inputs on which lane we should be
    if (MobileInput.Instance.SwipeLeft)
    {
      MoveLane(false);
    }

    if (MobileInput.Instance.SwipeRight)
    {
      MoveLane(true);
    }

    //Calculate were we should be
    Vector3 targetPosition = transform.position.z * Vector3.forward;
    if(desiredLane == 0)
    {
      targetPosition += Vector3.left * LANE_DISTANCE;//LAN_DISTANCE is a constant
    }
    else if (desiredLane == 2)
    {
      targetPosition += Vector3.right * LANE_DISTANCE;
    }

    //Let's calculate our move delta
    Vector3 moveVector = Vector3.zero;
    moveVector.x = (targetPosition - transform.position).normalized.x* speed;

    bool isGrounded = IsGrounded();
    anim.SetBool("Grounded",isGrounded);

    //Calculate Y
    if(IsGrounded())//if Grounded
    {
      //verticalVelocity = -0.1f;
      Jump();
      Slide();
    }
    else
    {
      //Fast falling mechanic
      if(MobileInput.Instance.SwipeDown)
      {
        if(!isGrounded)
        { 
          FastFalling();
        }
      }
      verticalVelocity -= gravity * Time.deltaTime;
      //follow with a slide
      Slide();
    }
    //correction gravity after a slide
    if (verticalVelocity <0 && IsGrounded())
      verticalVelocity = -0.1f;

    moveVector.y = verticalVelocity; //gravity
    moveVector.z = speed;

    //move the Character
    controller.Move(moveVector * Time.deltaTime);

    //rotate de player to were he is going
    Vector3 dir = controller.velocity;
    if(dir != Vector3.zero)
    {
      dir.y = 0;
      transform.forward = Vector3.Lerp(transform.forward, dir, TURN_SPEED);
    }

  }

  private void MoveLane(bool goingLeft)
  {
    //going left
    //because it's like a mirror
    if(!goingLeft)
    {
      desiredLane--;
      if(desiredLane == -1)
        desiredLane =0;
    }
    else
    {
      desiredLane++;
      if(desiredLane == 3)
        desiredLane = 2;
    }
  }
  private void Jump()
  {
    if (MobileInput.Instance.SwipeUp)
    {
      //jump
      //To trigger the jump Animation
      anim.SetTrigger("Jump");
      anim.SetBool("Sliding", false);
      verticalVelocity = jumpForce;
    }
  }

  private void FastFalling()
  {
    verticalVelocity = -2 * jumpForce;
  }

  //Making our own Grounded function
  private bool IsGrounded()
  {
    //ray for the bounds box
    Ray groundRay = new Ray(new Vector3(controller.bounds.center.x,(controller.bounds.center.y - controller.bounds.extents.y) + 0.2f,controller.bounds.center.z),Vector3.down);
    //to check
    Debug.DrawRay(groundRay.origin, groundRay.direction,Color.cyan, 1.0f);

    //True if grounded
    return(Physics.Raycast(groundRay, 0.2f + 0.1f));

  }

  public void StartRunning()
  {
    isRunning = true;
    anim.SetTrigger("StartRunning");
  }

  private void Slide()
  {
    if(MobileInput.Instance.SwipeDown)
    {
      // Slide
      StartSliding();
      StartCoroutine(StopSlidingTimer());
    }
  }

  private void StartSliding()
  {
    //For the size of the controllerCollider
    //shrink it when sliding
    controller.height /= colliderDividerSlide;
    controller.center = new Vector3(controller.center.x, controller.center.y / colliderDividerSlide, controller.center.z);
    anim.SetBool("Sliding", true);
  }

  private void StopSliding()
  {
    //For the size of the controllerCollider
    //get the collider size back after sliding
    controller.height *= colliderDividerSlide;
    controller.center = new Vector3(controller.center.x, controller.center.y * colliderDividerSlide, controller.center.z);
    anim.SetBool("Sliding", false);
  }

  private void Crash()
  {
    anim.SetTrigger("Death");
    isRunning = false;
    GameManager.Instance.OnDeath();
  }

  public void Revive()
  {
    Vector3 moveVector = Vector3.zero;
    moveVector.z = originalSpeed - 4.0f;// give the time to the player by lowering the speed
    desiredLane = 1; // respawn at the center line
    anim.SetTrigger("Revive");
    StartRunning();
  }

  private void OnControllerColliderHit(ControllerColliderHit hit)
  {
    switch (hit.gameObject.tag)
    {
      case "Obstacle":
      Crash();
      break;
    }
  }
  public IEnumerator StopSlidingTimer()
  {
    yield return new WaitForSeconds(slideTiming);
    StopSliding();
  }
}
