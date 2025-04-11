using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlappyBirdController : MonoBehaviour
{
    public ParticleSystem FogParticleSystem;
    public GameObject Bird;
    public GameObject PipePrefab;
    public GameObject AirplanePrefab;  
    public TextMeshProUGUI ScoreText;
    public GameObject WingsLeft;
    public GameObject WingsRight;
    public Button StartButton;
    public Button RestartButton;
    public float Gravity = 30;
    public float Jump = 10;
    public float PipeSpawnInterval = 2;
    public float AirplaneSpawnInterval = 6;  
    public float PipesSpeed = 5;
    public float AirplaneSpeed = 7;  
    public float SpeedIncreaseAmount = 2;
    public float SpawnRateDecreaseAmount = 0.2f;
    public int LevelThreshold = 5;
    public Material[] SkyboxMaterials;
    
    public AudioClip FlapSound;  
    public AudioClip ThemeMusic;  // Background theme music

    private AudioSource flapAudioSource;
    private AudioSource musicAudioSource;

    private float VerticalSpeed;
    private float PipeSpawnCountdown;
    private float AirplaneSpawnCountdown;  
    private GameObject PipesHolder;
    private GameObject AirplanesHolder;  
    private int PipeCount;
    private int Score;
    private int CurrentLevel = 1;
    private bool isGameRunning = false;

    void Start()
    {
        // Set up button listeners
        StartButton.onClick.AddListener(StartGame);
        RestartButton.onClick.AddListener(RestartGame);

        // Hide restart button initially
        RestartButton.gameObject.SetActive(false);

        InitializeGame();

        // Initialize audio sources
        flapAudioSource = gameObject.AddComponent<AudioSource>();
        flapAudioSource.playOnAwake = false;
        flapAudioSource.clip = FlapSound;

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.playOnAwake = false;
        musicAudioSource.clip = ThemeMusic;
        musicAudioSource.loop = true;  // Set to loop so the music continues playing
    }

    private void InitializeGame()
    {
        Score = 0;
        ScoreText.text = Score.ToString();
        PipeCount = 0;
        CurrentLevel = 0;

        if (FogParticleSystem.isPlaying)
        {
            FogParticleSystem.Stop();
        }

        Destroy(PipesHolder);
        PipesHolder = new GameObject("PipesHolder");
        PipesHolder.transform.parent = this.transform;

        Destroy(AirplanesHolder);
        AirplanesHolder = new GameObject("AirplanesHolder");
        AirplanesHolder.transform.parent = this.transform;

        VerticalSpeed = 0;
        Bird.transform.position = Vector3.up * 5;
        PipeSpawnCountdown = 0;
        AirplaneSpawnCountdown = AirplaneSpawnInterval;
        CurrentLevel = 1;

        if (SkyboxMaterials.Length > 0)
        {
            RenderSettings.skybox = SkyboxMaterials[0];
        }
    }

    void Update()
    {
        if (!isGameRunning) return;

        VerticalSpeed -= Gravity * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            VerticalSpeed = 0;
            VerticalSpeed += Jump;

            // Play the flap sound
            if (flapAudioSource != null && FlapSound != null)
            {
                flapAudioSource.Play();
            }
        }

        Bird.transform.position += Vector3.up * VerticalSpeed * Time.deltaTime;
        PipeSpawnCountdown -= Time.deltaTime;

        if (PipeSpawnCountdown <= 0)
        {
            PipeSpawnCountdown = PipeSpawnInterval;
            GameObject pipe = Instantiate(PipePrefab);
            pipe.transform.parent = PipesHolder.transform;
            pipe.transform.name = (++PipeCount).ToString();
            pipe.transform.position += Vector3.right * 30;
            pipe.transform.position += Vector3.up * Mathf.Lerp(4, 9, Random.value);
        }

        if (CurrentLevel >= 3)
        {
            AirplaneSpawnCountdown -= Time.deltaTime;
            if (AirplaneSpawnCountdown <= 0)
            {
                AirplaneSpawnCountdown = AirplaneSpawnInterval;
                GameObject airplane = Instantiate(AirplanePrefab);
                airplane.transform.parent = AirplanesHolder.transform;
                airplane.transform.position = new Vector3(30, Random.Range(7, 10), 0);  
            }
        }

        PipesHolder.transform.position += Vector3.left * PipesSpeed * Time.deltaTime;
        AirplanesHolder.transform.position += Vector3.left * AirplaneSpeed * Time.deltaTime;

        float speedTo01Range = Mathf.InverseLerp(-10, 10, VerticalSpeed);
        float noseAngle = Mathf.Lerp(-30, 30, speedTo01Range);
        Bird.transform.rotation = Quaternion.Euler(Vector3.forward * noseAngle) * Quaternion.Euler(Vector3.up * 20);

        float flapSpeed = (VerticalSpeed > 0) ? 30 : 5;
        float wingAngle = Mathf.Sin(Time.time * flapSpeed) * 45;
        WingsLeft.transform.localRotation = Quaternion.Euler(Vector3.left * wingAngle);
        WingsRight.transform.localRotation = Quaternion.Euler(Vector3.right * wingAngle);

        CheckLevelProgression();

        foreach (Transform pipe in PipesHolder.transform)
        {
            if (pipe.position.x < 0)
            {
                int pipeId = int.Parse(pipe.name);
                if (pipeId > Score)
                {
                    Score = pipeId;
                    ScoreText.text = Score.ToString();
                }
            }
            if (pipe.position.x < -30)
            {
                Destroy(pipe.gameObject);
            }
        }

        foreach (Transform airplane in AirplanesHolder.transform)
        {
            if (airplane.position.x < -30)
            {
                Destroy(airplane.gameObject);
            }
        }
    }

    private void StartGame()
    {
        isGameRunning = true;
        StartButton.gameObject.SetActive(false);
        InitializeGame();

        // Play theme music when the game starts
        if (musicAudioSource != null && ThemeMusic != null)
        {
            musicAudioSource.Play();
        }
    }

    private void RestartGame()
    {
        isGameRunning = true;
        RestartButton.gameObject.SetActive(false);
        InitializeGame();

        // Restart theme music on game restart
        if (musicAudioSource != null)
        {
            musicAudioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        isGameRunning = false;
        RestartButton.gameObject.SetActive(true);

        // Stop the theme music when the game is over
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
        }
    }

    private void CheckLevelProgression()
    {
        if (Score >= CurrentLevel * LevelThreshold)
        {
            CurrentLevel++;

            if (CurrentLevel <= 6)
            {
                PipesSpeed += SpeedIncreaseAmount;
            }

            PipeSpawnInterval = Mathf.Max(0.5f, PipeSpawnInterval - SpawnRateDecreaseAmount);

            ScoreText.text = "Score: " + Score.ToString() + " | Level: " + CurrentLevel;

            if (SkyboxMaterials.Length > 0)
            {
                int skyboxIndex = (CurrentLevel - 1) % SkyboxMaterials.Length;
                RenderSettings.skybox = SkyboxMaterials[skyboxIndex];
            }

            if (CurrentLevel >= 5 && (CurrentLevel - 5) % 2 == 0)
            {
                if (!FogParticleSystem.isPlaying)
                {
                    FogParticleSystem.Play();
                }
            }
            else
            {
                if (FogParticleSystem.isPlaying)
                {
                    FogParticleSystem.Stop();
                }
            }
        }
    }
}
