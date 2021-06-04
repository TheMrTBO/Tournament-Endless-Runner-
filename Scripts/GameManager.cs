using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //for the UI
using UnityEngine.Advertisements;
using TMPro;//using textMeshPro

public class GameManager : MonoBehaviour
{
    public const int COIN_SCORE_AMOUNT = 5;

    public static GameManager Instance { set; get;}

    public bool IsDead { set; get;}
    private bool isGameStarted = false;
    private PlayerMotor motor; //to keep track of our player

    //UI and the UI fields import from UnityEngine.UI
    public Animator gameCanvas,homeMenuAnim,coinFeedBackAnim,reviveTimerAnim,reviveButtonDeathMenuAnim;
    public TextMeshProUGUI hiscoreText,totalCoinsText,scoreText, coinText, modifierText;
    private float score, coinScore, modifierScore;
    private int lastScore;

    //Assets of the users Coins
    private int totalCoins, CoinsInThePocket ;

    //Death menu
    public Animator deathMenuAnim;
    public TextMeshProUGUI deadScoreText, deadCoinText;

    //for Revive
    public Vector3 lastCheckPointPos;
    public Button ReviveButton;
    private int timesRevive = 0;

    //Ads
    private string gameId;
    public bool testMode = true;
    private string placementId_video = "Video";
    private string placementId_rewardedVideo = "rewardedVideo";

    //Timer
    public float waitInSeconds = 3.0f;
    public float startTime;

    private void Awake()
    {
      Instance = this;
      //initialize the modifier score
      modifierScore = 1;
      //To initialize the motor player and find him with the tag
      motor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();

      //Update the score
      modifierText.text = "x" + modifierScore.ToString("0.0");
      coinText.text = coinScore.ToString("0");
      scoreText.text = score.ToString("0");

      //value saved on the device
      hiscoreText.text = PlayerPrefs.GetInt("Hiscore",0).ToString();
      totalCoinsText.text = PlayerPrefs.GetInt("TotalCoins",0).ToString();

      
      
      


      //Ads

      if (Application.platform == RuntimePlatform.IPhonePlayer)
        gameId = "3908910";
      else if (Application.platform == RuntimePlatform.Android)
        gameId = "3908911";

      //gameId = "3908910";
      Advertisement.Initialize(gameId,testMode); //App Store ID

    }
	
    void Start()
    {
      // Make the game run as fast as possible
      if (Application.platform == RuntimePlatform.IPhonePlayer)
        Application.targetFrameRate = 60;//we fix to 60FPS
      if (Application.platform == RuntimePlatform.Android)
        Application.targetFrameRate = 60;//we fix to 60FPS
    }

    void Update()
    {
      if (MobileInput.Instance.Tap && !isGameStarted)
      {
        isGameStarted = true;
        motor.StartRunning();
        FindObjectOfType<BackGroundSpawner>().IsScrolling = true;//for the background to start scrolling
        FindObjectOfType<CameraMotor>().IsMoving = true;//begin moving the camera

        gameCanvas.SetTrigger("Show");
        homeMenuAnim.SetTrigger("Hide");
      }

      if (isGameStarted && !IsDead)
      {
        //Increase the score up
        score += (Time.deltaTime * modifierScore);

        if (lastScore != (int)score)
        {
          lastScore = (int)score;
          scoreText.text = score.ToString("0"); // only print 1 digit
        }

      }
    }

    public void GetCoin()
    {
      coinScore ++;
      coinFeedBackAnim.SetTrigger("CoinFeedBack");
      coinText.text = coinScore.ToString("0");
      score += COIN_SCORE_AMOUNT;
      scoreText.text = score.ToString("0");
    }

    public void UpdateModifier(float modifierAmount)
    {
      modifierScore = 1.0f + modifierAmount;
      modifierText.text = "x" + modifierScore.ToString("0.0");
    }

    public void OnPlayButton()
    {
      UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void OnDeath()
    {
        IsDead = true;
        FindObjectOfType<BackGroundSpawner>().IsScrolling = false;//for the background to stop scrolling

        deadScoreText.text = score.ToString("0");
        deadCoinText.text = coinScore.ToString("0");

        if(timesRevive >= 1)//no more than 1 revive
        {
          ReviveButton.interactable = false;
          reviveButtonDeathMenuAnim.SetTrigger("NoRevive");
        }

        deathMenuAnim.SetTrigger("Dead");//name of the trigger
		    gameCanvas.SetTrigger("Hide");//Hide the game Score when we die

        //Check if this is the highiest score 
        if (score > PlayerPrefs.GetInt("Hiscore"))
        {
          float s = score;
          if (s % 1 == 0)
            s +=1;
          PlayerPrefs.SetInt("Hiscore",(int)s); //change score to int from float 
        }

        //save the number of coins collected in the user pocket
        CoinsInThePocket = (int)PlayerPrefs.GetInt("TotalCoins");
        totalCoins = (int)coinScore + CoinsInThePocket;
        PlayerPrefs.SetInt("TotalCoins",totalCoins);

    }

    public void OnReviveButton()
    {
      RequestRevive(); // for the ads before the revive
    }

    public void PlayerRevivePosition()
    {
      FindObjectOfType<PlayerPosition>().NewPlayerPosition();
    }

    public void RequestRevive()
    {
      ShowOptions so = new ShowOptions();
      so.resultCallback = CallRevive; //call the function Revive 

      Advertisement.Show(placementId_rewardedVideo,so); //ID for the ad reward Placement "Reward"
    }

    public void CallRevive(ShowResult sr)
    {
      if(sr == ShowResult.Finished)//If ads played successfully
      {
        //foreach (BackGroundSpawner bs in FindObjectOfType<BackGroundSpawner>()) //FindObjectOfType<BackGroundSpawner>().IsScrolling 
        //  bs.IsScrolling = true;

        reviveTimerAnim.SetTrigger("ReviveTimer");//Revive timer
        deathMenuAnim.SetTrigger("Alive");//remove the death screen
        PlayerRevivePosition();
        StartCoroutine(Wait3SecAndRevive());//Call the coroutine where we wait 3sec before spawning
      }
      else
      {
        OnPlayButton();
      }
    }

    public IEnumerator Wait3SecAndRevive()
    {
      yield return new WaitForSeconds(3.0f);
      
      gameCanvas.SetTrigger("Show"); //Show the score
      FindObjectOfType<PlayerMotor>().Revive();
      IsDead = false;
      FindObjectOfType<BackGroundSpawner>().IsScrolling = true; //activate the moving background
      timesRevive ++;
    }
}
