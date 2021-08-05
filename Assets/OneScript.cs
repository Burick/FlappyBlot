using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class OneScript : MonoBehaviour
{
    public bool isParicle;
    public bool isPlayer;
    public bool isBlot;

    public float speed;
    public float jumpForce;
    public float defoulGravity;

    public Transform respawnPos;

    public GameObject blotParticle;

    private bool isPlaying;
    private bool isDead;

    private Rigidbody2D rb;

    private SpriteRenderer mySR;

    private Animator anim;



    //camera
    //позиція за якою рухатись
    private Transform targetPlayer;

    //стандартно нульовий вектор руху
    Vector3 velocity = Vector3.zero;

    //скорість плавної затримки 
    public float smoothTime = .15f;

    //значення мінімального Y значення
    public bool YMinEnable = false;
    public float YMinValue = 0f;
    //значення максимального Y значення
    public bool YMaxEnable = false;
    public float YMaxValue = 0f;

    //значення мінімального Х значення
    public bool XMinEnable = false;
    public float XMinValue = 0f;

    //значення максимального Х значення
    public bool XMaxEnable = false;
    public float XMaxValue = 0f;

    public GameObject theCamera;

    //public AudioSource musicSource;

    void Start()
    {
        if (isPlayer)
        {
            rb = GetComponent<Rigidbody2D>();
            mySR = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();

            playerPositions = new List<Vector3>();
           
            
            ResetLifeCount();
            NullifyScore();
            //PlayerPrefs.SetInt("ColorRand", -1);
            
            SetPilars();
            //Debug.Log(PlayerPrefs.GetInt("ColorRand"));

            Invoke("LetsStartSpeack", 1.75f);
            
        }


        if (isParicle)
        {
            randColorNum = PlayerPrefs.GetInt("ColorRand");

            particle = GetComponent<ParticleSystem>();
            var main = particle.main;
            main.startColor = blotColor[randColorNum];
        }


        if (isBlot)
        {
            blotSR = GetComponent<SpriteRenderer>();
            material = GetComponent<SpriteRenderer>().material;
            int randBlotSprite = Random.Range(0, randomBlot.Length);
            blotSR.sprite = randomBlot[randBlotSprite];

            randColor = PlayerPrefs.GetInt("ColorRand");
            blotSR.color = blotColor[randColor];
        }

    }

    private void ExitGame()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    public Transform chiterstvoDliaRazrabotki;

    void Update()
    {
        if (isPlayer)
        {
            ExitGame();
            if (!isPlaying && !isDead && !isSpeaking & !isStartSpeaking && !cantMove && Input.GetButtonDown("Jump"))
            {
                isPlaying = true;
                rb.gravityScale = defoulGravity;
                if (howToStartText.enabled)
                {
                    howToStartText.enabled = false;
                    musicAS.Play();
                }
            }

            if (isPlaying && !isDead && Input.GetButtonDown("Jump") )
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                anim.SetTrigger("jump");
                PlayJumpSound();
            }

           
            DoSlowmotion();
            OutOfSlowmotion();

            if (Input.GetKeyDown(KeyCode.R) && !isDead && !isReadyToNext && !isTotalWin)
            {
                RestartStage();
            }

            CheckNextReadyInput();

            TotalWintThingsCheck();

            ControlSpeackText();


            StartSpeachControll();

            if (Input.GetKeyDown(KeyCode.J) && Input.GetButton("Jump"))
            {
                transform.position = chiterstvoDliaRazrabotki.position;
            }
        }


        if (isParicle)
        {
            TimePassed += Time.deltaTime;
            if (TimePassed > SoundCapResetSpeed)
            {
                soundsPlayed = 0;
                TimePassed = 0;
            }
        }


        if (isBlot)
        {
            BlotController();
        }
    }

    private Vector3 targetPos;
    private void FixedUpdate()
    {
        if (isPlayer)
        {
            if (isPlaying && !isDead)
            {
                rb.velocity = new Vector2(speed, rb.velocity.y);
            }

            

          
                if (targetPlayer == null)
                {
                    targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;

                }
                //target position
                targetPos = targetPlayer.position;
           

             if(isTotalWin && theCamera.transform.position.x >= 362f && !XMaxEnable)
            {
                XMaxEnable = true;
                XMinEnable = true;
                // targetPlayer = GameObject.FindGameObjectWithTag("TotalWin").transform;
                activeBlots = GameObject.FindGameObjectsWithTag("Blot");
                for (int i = 0; i < activeBlots.Length; i++)
                {
                    Destroy(activeBlots[i]);
                }
                OffObstacles();
                //targetPos = targetPlayer.position;
                theCamera.transform.position = Vector3.SmoothDamp(theCamera.transform.position, new Vector3(362f, 0, 0), ref velocity, smoothTime);

                winSpeachText.gameObject.SetActive(true);
                winSpeachText.color = blotColor[randColorNum];
            }
           

            //verticle
            if (YMinEnable && YMaxEnable)
                targetPos.y = Mathf.Clamp(targetPlayer.position.y, YMinValue, YMaxValue);

            else if (YMinEnable)
                targetPos.y = Mathf.Clamp(targetPlayer.position.y, YMinValue, targetPlayer.position.y);

            else if (YMinEnable)
                targetPos.y = Mathf.Clamp(targetPlayer.position.y, targetPlayer.position.y, YMaxValue);

            //horizontal	
            if (XMinEnable && XMaxEnable)
                targetPos.x = Mathf.Clamp(targetPlayer.position.x, XMinValue, XMaxValue);

            else if (XMinEnable)
                targetPos.x = Mathf.Clamp(targetPlayer.position.x, XMinValue, targetPlayer.position.x);

            else if (XMinEnable)
                targetPos.x = Mathf.Clamp(targetPlayer.position.x, targetPlayer.position.x, XMaxValue);



            
            if (!XMaxEnable)
            {
                theCamera.transform.position = Vector3.SmoothDamp(theCamera.transform.position, targetPos + new Vector3(4, 0, 0), ref velocity, smoothTime);
            }
            

            if (isRewinding)
            {
                DoRewindPos();
            }
            else
            {
                RecordMovement();

            }
        }



    }

    private void Die()
    {
        if(stageNum <= 11)
        {
            MinusLife(1);
        }
        
        isDead = true;
        isPlaying = false;
        mySR.enabled = false;

        rb.gravityScale = 0;
        rb.velocity = new Vector2(0, 0);

        //spawn particle
        Instantiate(blotParticle, transform.position, blotParticle.transform.rotation);
        PlayDeadSound();

        playerPositions.Clear();
    }

    private void Respawn()
    {
        if (!isTotalWin)
        {
            if (currentLife <= 0)
            {
                Invoke("RestartStage", .15f);
            }

            NullifyScore();
            transform.position = respawnPos.position;
            mySR.enabled = true;
            isDead = false;
        }
        else
        {
            transform.position = new Vector3(345f, 0, 0);
            mySR.enabled = true;
            isDead = false;
        }
        

        
    }

    private GameObject[] activeBlots;
    private void RestartStage()
    {
        activeBlots = GameObject.FindGameObjectsWithTag("Blot");
        for (int i = 0; i < activeBlots.Length; i++)
        {
            Destroy(activeBlots[i]);
        }
        SetPilars();
        ResetLifeCount();
        NullifyScore();

        transform.position = respawnPos.position;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(0, 0);

        isDead = false;
        isPlaying = false;

    }

    private void NextStageSet()
    {
        stageNum++;
        
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (isPlayer)
        {
            if (coll.tag == "Obstacle")
            {
                //dead & respawn
                Die();
                Invoke("Respawn", 2.5f);

            }

            if (coll.tag == "Score")
            {
                if (Time.time >= lastAddScoreTime + addScoreCD && isPlaying)
                {
                    AddScore(1);
                }
               
            }

            if (coll.tag == "Finish")
            {
                Win();
            }

            if (coll.tag == "TotalWin")
            {
                transform.position = new Vector3(345f, 0, 0) ;
                ChangeColorsInTotalWin();
            }
        }

    }

    private void Win()
    {
        if (isPlaying)
        {
            isDoSlowMo = true;
            NextStageSet();
        }
        
    }


    public float multiplieTime;
    public Sprite[] randomBlot;
    private Material material;
    private bool isDissolve = true;
    public float fade;

    private SpriteRenderer blotSR;

    private int randColor;

    private void BlotController()
    {
        if (isDissolve)
        {
            fade += Time.deltaTime * multiplieTime;

            if (fade >= 1f)
            {
                fade = 1f;
                isDissolve = false;
            }

            material.SetFloat("_Fade", fade);
        }
        else
        {
            var myScript = GetComponent<OneScript>();
            //myScript.enabled = false;

            Destroy(myScript);
        }
    }


    private ParticleSystem particle;

    public GameObject blot, hitParticle;
    private Transform splatHolder;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    public AudioSource audioSource;
    public AudioClip[] sounds;
    public float SoundCapResetSpeed = 0.55f;
    public int MaxSounds = 5;


    private float TimePassed;
    private int
        soundsPlayed,
        numberOfBlot;

    private void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(particle, other, collisionEvents);

        int count = collisionEvents.Count;

        for (int i = 0; i < count; i++)
        {
            Instantiate(hitParticle, collisionEvents[i].intersection, Quaternion.LookRotation(collisionEvents[i].normal));
            Instantiate(blot, collisionEvents[i].intersection, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)), splatHolder);

            if (soundsPlayed < MaxSounds)
            {
                soundsPlayed += 1;
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(sounds[Random.Range(0, sounds.Length)], Random.Range(0.1f, 0.35f));
            }
        }
    }


    public int[] maxLife;
    private int currentLife;

    private int curScore;

    public Text scoreText;
    public Text liveCountText;
    public Text curLvlText;

    public Text restartText, howToStartText;

    public float addScoreCD;
    private float lastAddScoreTime;
    private void AddScore(int toAdd)
    {

        lastAddScoreTime = Time.time;
        curScore += toAdd;
        scoreText.text = curScore.ToString() + " / " + numActivePilars[stageNum - 1].ToString();
        //sound
        PlayScoreSound();
        
        
    }
    private void NullifyScore()
    {
        curScore = 0;
        scoreText.text = curScore.ToString() + " / " + numActivePilars[stageNum - 1].ToString();
    }

    private void MinusLife(int toMinus)
    {
        currentLife -= toMinus;
        liveCountText.text = currentLife.ToString();


    }

    private void ResetLifeCount()
    {
        currentLife = maxLife[stageNum - 1];
        liveCountText.text = currentLife.ToString();
    }


    public  int stageNum;
    public int[] numActivePilars;

    public Vector2 minMaxY;

    public GameObject winPilars;
    public GameObject[] obstaclePilars;

    public int minColorNum, maxColorNum;
    private int randColorNum;
    public Color[] blotColor;
    public Color[] obstacleColor;

    private GameObject[] obstacleSR;
    public SpriteRenderer[] winPilarSR;
    public Camera mainCamera;

    
    private void SetPilars()
    {
        for (int i = 0; i < obstaclePilars.Length; i++)
        {
            if(i  <= numActivePilars[stageNum -1] - 1)
            {
                obstaclePilars[i].SetActive(true);

                float randY = Random.Range(minMaxY.x, minMaxY.y);
                obstaclePilars[i].transform.position = new Vector3(obstaclePilars[i].transform.position.x, randY, obstaclePilars[i].transform.position.z);
            }
            else
            {
                obstaclePilars[i].SetActive(false);
            }

            if (i <= numActivePilars[stageNum - 1])
            {
                float randY = Random.Range(minMaxY.x, minMaxY.y);
                winPilars.transform.position = new Vector3(obstaclePilars[i].transform.position.x, randY, 0);
            }

            if (i <= numActivePilars[stageNum - 1] && stageNum == 11)
            {
                float randY = Random.Range(minMaxY.x, minMaxY.y);
                winPilars.transform.position = new Vector3(obstaclePilars[i].transform.position.x + 7f, randY, 0);
            }
        }

        obstacleSR = GameObject.FindGameObjectsWithTag("Obstacle");

        PlayerPrefs.SetInt("ColorRand", Random.Range(minColorNum, maxColorNum));
        
        randColorNum = PlayerPrefs.GetInt("ColorRand");


        if (randColorNum == PlayerPrefs.GetInt("RandColorNumber"))
        {
            PlayerPrefs.SetInt("ColorRand", Random.Range(minColorNum, maxColorNum));
            randColorNum = PlayerPrefs.GetInt("ColorRand");
        }
        PlayerPrefs.SetInt("lastRandColorNumber", randColorNum);


        PlaySwitchSound();

        mySR.color = blotColor[randColorNum];
        var cam = mainCamera.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = obstacleColor[randColorNum];

        for (int i = 0; i < obstacleSR.Length; i++)
        {
            obstacleSR[i].GetComponent<SpriteRenderer>().color = obstacleColor[randColorNum];
        }

        for (int i = 0; i < winPilarSR.Length; i++)
        {
            winPilarSR[i].color = blotColor[randColorNum];
        }

        liveCountText.color = blotColor[randColorNum];
        scoreText.color = blotColor[randColorNum];
        curLvlText.text = "LVL " + (stageNum - 1).ToString();
        curLvlText.color = blotColor[randColorNum];

        restartText.color = blotColor[randColorNum];
        howToStartText.color = blotColor[randColorNum];
        readyToNextText.color = blotColor[randColorNum];

        textDisplay.color = blotColor[randColorNum];

        
    }

    
    //timeManager
    private bool isDoSlowMo;

    public float slowdownFactor = 0.05f;
    public float slowdownLength = 2f;

    public float musicSlowdownPitchSpeed;
    private void DoSlowmotion()
    {
        if (isDoSlowMo)
        {
            Time.timeScale -= slowdownFactor * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
            Time.fixedDeltaTime = Time.timeScale * .02f;

            if(musicAS.pitch > 0)
            {
                musicAS.pitch -= musicSlowdownPitchSpeed * Time.unscaledDeltaTime;
            }
            else if(musicAS.pitch <= 0)
            {
                musicAS.pitch = 0;
            }
            
        }
        if (isDoSlowMo && Time.timeScale <= .1f )
        {
            Time.timeScale = 0;
            isDoSlowMo = false;
            AskAfterWin();

            Invoke("PlayWinSound", .65f);
            Invoke("StartSpeackAfterWin", 2f);
        }

    }


    private void StartSpeackAfterWin()
    {
        isSpeaking = true;
        textDisplay.gameObject.SetActive(true);
        textDisplay.text = "";
        StartCoroutine(Type());
    }


    public GameObject winParticle;
    public AudioClip winSound;
    public GameObject heartsImagesGO;
    private void PlayWinSound()
    {
        scoreAS.clip = winSound;
        scoreAS.Play();
        winParticle.GetComponent<ParticleSystem>().Play();


        if(stageNum <= 11)
        {
            isReadyToNext = true;
            ////////////////////////readyToNextText.gameObject.SetActive(true);
        }
        else if(stageNum > 11)
        {
            isTotalWin = true;

           

            liveCountText.gameObject.SetActive(false);
            scoreText.gameObject.SetActive(false);
            curLvlText.gameObject.SetActive(false);
            restartText.gameObject.SetActive(false);
            heartsImagesGO.SetActive(false);
        }
        
    }

    private bool isTotalWin;
    private bool isGoTheEnd;
    private void TotalWintThingsCheck()
    {
        if (isTotalWin)
        {
            if (Input.GetButtonDown("Jump") && !isPlaying && !isGoTheEnd)
            {
                isGoTheEnd = true;

                rb.isKinematic = false;
                rb.gravityScale = 0;
                rb.velocity = new Vector2(0, 0);
                isPlaying = false;

                Time.fixedDeltaTime = Time.timeScale * .02f;

                musicAS.pitch = 1;
                isDead = false;
                //targetPlayer = null;
            }
        }
    }

    public Text readyToNextText;
    private void AskAfterWin()
    {
        rb.gravityScale = 0;
        rb.velocity = new Vector2(0, 0);

        isPlaying = false;
        isDead = true;
        isOutOfSlowMo = true;

        
    }

    private bool isReadyToNext;

    private void CheckNextReadyInput()
    {
        if(isReadyToNext && !isSpeaking && Input.GetButtonDown("Jump"))
        {
            readyToNextText.gameObject.SetActive(false);
            isReadyToNext = false;
            StartRewinding();
        }
    }

    private bool isOutOfSlowMo;
    private void OutOfSlowmotion()
    {
        if (isOutOfSlowMo)
        {
            Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        }
        if (isOutOfSlowMo && Time.timeScale == 1)
        {
            isOutOfSlowMo = false;
        }
        
    }

    //Rewind time

    private bool isRewinding;
    List<Vector3> playerPositions;
    
    private void RecordMovement()
    {
        if(isPlaying && !isDead)
        {
            playerPositions.Insert(0, transform.position);
        }
        
    }

    private void DoRewindPos()
    {
        if(playerPositions.Count > 0)
        {
            transform.position = playerPositions[0];
            playerPositions.RemoveAt(0);
        }
        else
        {
            StopRewinding();
        }
        
    }

    public float rewindSoundSpeed;
    public AudioSource musicAS;

    private void StartRewinding()
    {
        isRewinding = true;
        rb.isKinematic = true;
        Time.fixedDeltaTime = 0.003f;

        musicAS.pitch = -rewindSoundSpeed;
        
    }

    private void StopRewinding()
    {
        isRewinding = false;
        rb.isKinematic = false;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(0, 0);
        isPlaying = false;

        Time.fixedDeltaTime = Time.timeScale * .02f;

        musicAS.pitch = 1;
        //calnest stage
        Invoke("RestartStage", 0.5f);
    }


    public AudioSource playerAS;
    public AudioClip switchSound;

    public AudioClip jumpSound;

    public AudioClip[] deadSound;
    private void PlaySwitchSound()
    {
        playerAS.clip = switchSound;
        playerAS.Play();
    }

    private void PlayJumpSound()
    {

        //int rand = Random.Range(0, jumpSound.Length);
        playerAS.clip = jumpSound;
        playerAS.Play();
    }


    private void PlayDeadSound()
    {
        int rand = Random.Range(0, deadSound.Length);

        playerAS.clip = deadSound[rand];
        playerAS.Play();
    }

    public AudioClip scoreSound;

    public AudioSource scoreAS;
    private void PlayScoreSound()
    {

        scoreAS.clip = scoreSound;
        scoreAS.Play();
    }

    public GameObject Obstacles;
    private void OffObstacles()
    {
        Obstacles.SetActive(false);
    }
    public Text winSpeachText;
    public string[] wichText;
    private int numberOfWichText;

    private int JumpOutScreenCount;
    private void ChangeColorsInTotalWin()
    {
        if (isTotalWin)
        {


            numberOfWichText++;
            //if some

            if(numberOfWichText > wichText.Length - 1)
            {
                JumpOutScreenCount++;
                winSpeachText.text = "Не уходи, друг..." + "   " + JumpOutScreenCount.ToString();
            }
            else if(numberOfWichText <= wichText.Length - 1)
            {
                winSpeachText.text = wichText[numberOfWichText];
            }
            



            PlayerPrefs.SetInt("ColorRand", Random.Range(minColorNum, maxColorNum));
            
            randColorNum = PlayerPrefs.GetInt("ColorRand");


            PlaySwitchSound();

            mySR.color = blotColor[randColorNum];
            var cam = mainCamera.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = obstacleColor[randColorNum];

            activeBlots = GameObject.FindGameObjectsWithTag("Blot");
            for (int i = 0; i < activeBlots.Length; i++)
            {
                activeBlots[i].GetComponent<SpriteRenderer>().color = blotColor[randColorNum];
            }

            obstacleSR = GameObject.FindGameObjectsWithTag("Obstacle");
            for (int i = 0; i < obstacleSR.Length; i++)
            {
                obstacleSR[i].GetComponent<SpriteRenderer>().color = obstacleColor[randColorNum];
            }


            winSpeachText.color = blotColor[randColorNum];
        }

       
    }

    public TextMeshProUGUI textDisplay;
    public string[] sentences;
    private int indexT;
    public float typingSpeed;

    public int[] maxSentencesInThisStage;

    private bool isSpeaking;
    //private bool isSpeackTextTyping, isSpeackTextTypeReady;
    IEnumerator Type()
    {
        yield return new WaitForSeconds(.03f);

        foreach (char letter in sentences[indexT].ToCharArray())
        {
            if (textDisplay.text != sentences[indexT])
            {
                textDisplay.text += letter;

                int rand = Random.Range(0, speackSounds.Length);
                speackAS.clip = speackSounds[rand];
                speackAS.Play();
                yield return new WaitForSeconds(typingSpeed);
            }
                
        }
    }

    private void ControlSpeackText()
    {
        if (isSpeaking)
        {
            if (Input.GetButtonDown("Jump") && textDisplay.text != sentences[indexT])
            {
                StopCoroutine(Type());
                textDisplay.text = sentences[indexT];
            }
            else if (Input.GetButtonDown("Jump") && textDisplay.text == sentences[indexT])
            {
                NextSentence();
            }
            
        }
    }

    private void NextSentence()
    {

        if(indexT  >= maxSentencesInThisStage[stageNum - 2])
        {
            indexT = maxSentencesInThisStage[stageNum - 2];
            if (stageNum <= 11)
            {
                readyToNextText.gameObject.SetActive(true);
            }
            
            isSpeaking = false;
            textDisplay.gameObject.SetActive(false);
            indexT++;
        }
        else if(indexT < maxSentencesInThisStage[stageNum - 2])
        {
            indexT++;
            textDisplay.text = "";
            StartCoroutine(Type());

        }
        
    }


    private bool isStartSpeaking = false, cantMove = true;
    public string[] startSpeackText;

    private void LetsStartSpeack()
    {
        textDisplay.gameObject.SetActive(true);
        isStartSpeaking = true;
        StartCoroutine(TypeStarts());
    }

    private void StartSpeachControll()
    {
        if (isStartSpeaking)
        {
            if (Input.GetButtonDown("Jump") && textDisplay.text != startSpeackText[indexT])
            {
                textDisplay.text = startSpeackText[indexT];
                StopCoroutine(TypeStarts());
                
            }
            else if (Input.GetButtonDown("Jump") && textDisplay.text == startSpeackText[indexT])
            {
                if (indexT >= startSpeackText.Length -1)
                {
                    textDisplay.gameObject.SetActive(false);
                    indexT = 0;
                    isStartSpeaking = false;
                    cantMove = false;


                    howToStartText.gameObject.SetActive(true);
                    
                }
                else if (indexT < startSpeackText.Length - 1)
                {
                    indexT++;
                    textDisplay.text = "";
                    StartCoroutine(TypeStarts());

                }
            }

        }
    }

    public AudioSource speackAS;
    public AudioClip[] speackSounds;

    IEnumerator TypeStarts()
    {
        yield return new WaitForSeconds(.03f);

        foreach (char letter in startSpeackText[indexT].ToCharArray())
        {
            if(textDisplay.text != startSpeackText[indexT])
            {
                textDisplay.text += letter;
                int rand = Random.Range(0, speackSounds.Length);
                speackAS.clip = speackSounds[rand];
                speackAS.Play();
                yield return new WaitForSeconds(typingSpeed);
            }
            
        }
    }


   
}
