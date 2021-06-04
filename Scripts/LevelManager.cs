using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
  public static LevelManager Instance {set; get;}

  public bool SHOW_COLLIDER = true;//$$

  //Level Spawning
  public float DISTANCE_BEFORE_SPAWN = 80.0f;//distance we see in front of use
  public const int INITIAL_SEGMENTS = 10;
  public const int INITIAL_TRANSITION_SEGMENTS = 2;
  public int MAX_SEGMENTS_ON_SCREEN = 15;//the number of segments before spawning
  private Transform cameraContainer;//were we are in the map
  private int amoutOfActiveSegments;
  private int continiousSegments;//to know when to put the transition segment
  private int currentSpawnZ;
  private int currentlevel;// are we on the floor ? the train ? it helps us with that
  private int y1, y2, y3;


  //List of PieceSpawner for the pooling mechanic
  public List<Piece> ramps = new List<Piece>();
  public List<Piece> longblocks = new List<Piece>();
  public List<Piece> jumps = new List<Piece>();
  public List<Piece> slides = new List<Piece>();
  [HideInInspector]
  public List<Piece> pieces = new List<Piece>(); //All the pieces in the pool

  //List of segments
  public List<Segment> availableSegments = new List<Segment>();
  public List<Segment> availableTransitions = new List<Segment>();
  [HideInInspector]
  public List<Segment> segments = new List<Segment>();

  //Gameplay
  private bool isMoving = false;

  private void Awake()
  {
    Instance = this;
    cameraContainer = Camera.main.transform;// need the camera to have the "mainCamera" tag in the game
    currentSpawnZ = 0;
    currentlevel = 0;
  }

  private void Start()
  {
    for (int i = 0; i<INITIAL_SEGMENTS; i++)
      if ( i < INITIAL_TRANSITION_SEGMENTS)
      //to begin with some room to run
        SpawnTransition();
      else
      //Generate segments
        GenerateSegment();
  }

  private void Update()
  {
    //To spawn the segments
    if (currentSpawnZ - cameraContainer.position.z < DISTANCE_BEFORE_SPAWN)
      GenerateSegment();

    //To despawn the segments
    if (amoutOfActiveSegments >= MAX_SEGMENTS_ON_SCREEN)
    {
      segments[amoutOfActiveSegments - 1].DeSpawn();
      amoutOfActiveSegments--;
    }


  }

  private void GenerateSegment()
  {
    SpawnSegment();

    if (Random.Range(0f, 1f) < (continiousSegments * 0.125f))//to have a transition segment when ...
    {
      //Spawn transition segment
      SpawnTransition();
      //initialize continious segment to 0
      continiousSegments = 0;
    }
    else
    {
      continiousSegments++;
    }
  }

  private void SpawnSegment()//becarefull of the level of the player
  {
    List<Segment> possibleSeg = availableSegments.FindAll(x => x.beginY1 == y1 || x.beginY2 == y2 || x.beginY3 == y3);
    int id = Random.Range(0, possibleSeg.Count);

    Segment s = GetSegment(id, false);

    y1 = s.endY1;
    y2 = s.endY2;
    y3 = s.endY3;

    s.transform.SetParent(transform);
    s.transform.localPosition = Vector3.forward * currentSpawnZ;

    currentSpawnZ += s.length;
    amoutOfActiveSegments++;
    s.Spawn();


  }

  private void SpawnTransition()
  {
    //Find all the available segments
    List<Segment> possibleTransition = availableTransitions.FindAll(x => x.beginY1 == y1 || x.beginY2 == y2 || x.beginY3 == y3);
    int id = Random.Range(0, possibleTransition.Count);

    Segment s = GetSegment(id, true);

    y1 = s.endY1;
    y2 = s.endY2;
    y3 = s.endY3;

    s.transform.SetParent(transform);
    s.transform.localPosition = Vector3.forward * currentSpawnZ;

    currentSpawnZ += s.length;
    amoutOfActiveSegments++;
    s.Spawn();
  }

  public Segment GetSegment (int id, bool transition)
  {
    Segment s = null;
    s = segments.Find(x => x.SegId == id && x.transition == transition && !x.gameObject.activeSelf);

    if(s == null)
    {
      GameObject go = Instantiate ((transition) ? availableTransitions[id].gameObject : availableSegments[id].gameObject) as GameObject;
      s = go.GetComponent <Segment>();

      s.SegId = id;
      s.transition = transition;

      segments.Insert(0,s);
    }

    else
    {
      segments.Remove(s);
      segments.Insert(0, s); //add at the very beginning of the segment list
    }
    return s;
  }

  public Piece GetPiece(PieceType pt, int visualIndex)
  {
    //Find the correct object in the pool not already used
    Piece p = pieces.Find(x => x.type == pt && x.visualIndex == visualIndex && !x.gameObject.activeSelf);

    if (p == null)//If we don't find any piece WE have to spawn it
    {
      GameObject go = null;

      if(pt == PieceType.ramp)
        go = ramps[visualIndex].gameObject;//Spawn a ramp

      else if(pt == PieceType.longblock)
        go = longblocks[visualIndex].gameObject;//Spawn a longblock

      else if(pt == PieceType.jump)
        go = jumps[visualIndex].gameObject;//Spawn a jump

      else if(pt == PieceType.slide)
        go = slides[visualIndex].gameObject;//Spawn a slide

      go = Instantiate(go);//cause it's only in the prefab for the moment and no where on the scene
      p = go.GetComponent<Piece>();//grabing the piece on top of the gameobject
      pieces.Add(p);//add it to the pool


    }

    return p;//return the piece we selected
  }
}
