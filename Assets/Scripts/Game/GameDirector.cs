﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CountDownEvent : UnityEvent<int> {}

[System.Serializable]
public class TimeUpdateEvent : UnityEvent<float> {}

[System.Serializable]
public class StateUpdateEvent : UnityEvent<GameDirector.GameState> {}

[System.Serializable]
public class SpeedUpdateEvent : UnityEvent<string, string> {}

[System.Serializable]
public class ParticipantChangeEvent : UnityEvent<int> {}

[System.Serializable]
public class TestChangeEvent : UnityEvent<int> {}

/*
Base class of the game. Launches and stops the game. Contains the different game's parameters.
*/

public class GameDirector : MonoBehaviour
{
    public enum GameState {CountDown, Paused, Playing, Stopped}

    [SerializeField]
    public BasicPointer basicPointer;

    [SerializeField]
    public SoundManager soundManager;

    [SerializeField]
    private WallManager wallManager;

    [SerializeField]
    private LoggingManager loggingManager;

    [SerializeField]
    private ProfileManager profileManager;

    [SerializeField]
    private PupilLabs.RecordingController gazeRecorder;

    //temporarily serialized field for game test
    [SerializeField]
    private float gameDuration;

    private float gameDefaultDuration;
    //temporarily serialized field for game test
    [SerializeField]
    private string gameDifficulty;

    [SerializeField]
    private float gameWarmUpTime = 3f;

    [SerializeField]
    private float moleExpiringDuration = .2f;

    [SerializeField]
    public TimeUpdateEvent timeUpdate;

    [SerializeField]
    public StateUpdateEvent stateUpdate;

    [SerializeField]
    public CountDownEvent countUpdate;

    private Dictionary<string, float> difficultySettings;
    private Coroutine spawnTimer;
    private float currentGameTimeLeft;
    private float currentMoleTimeLeft;
    private GameState gameState = GameState.Stopped;
    private LoggerNotifier loggerNotifier;
    private PatternManager patternManager;
    private ModifiersManager modifiersManager;
    private Constraint constraint;
    private SpeedUpdateEvent speedUpdateEvent = new SpeedUpdateEvent();
    public ParticipantChangeEvent participantChanged = new ParticipantChangeEvent();
    public TestChangeEvent testChanged = new TestChangeEvent();
    private int participantId = 0;
    private int testId = 0;

    // This will hold the time when the mole appears
    private float moleAppearTime;

    // This will hold the position of the player when the mole appears
    private Vector3 mappedPayerPosition;

    // This will hold the reaction times of the player
    //private List<float> reactionTimes = new List<float>();

    // This will hold the distances between the player and the mole
    private List<float> distances = new List<float>();


    public class MoleData
    {
        public int MoleId { get; set; }
        public Mole Mole { get; set; }
        public float Distance { get; set; }
        public float? ReactionTime { get; set; }
        public float? Speed { get; set; }
    }


    public Dictionary<int, MoleData> moleDataDict = new Dictionary<int, MoleData>();

    private Dictionary<string, Dictionary<string, float>> difficulties = new Dictionary<string, Dictionary<string, float>>(){
        {"Slow", new Dictionary<string, float>(){
            {"spawnRate", 3.5f},
            {"spawnVariance", .1f},
            {"lifeTime", 5f},
            {"fakeCoeff", .1f},
        }},
        {"Normal", new Dictionary<string, float>(){
            {"spawnRate", 2.25f},
            {"spawnVariance", .3f},
            {"lifeTime", 4f},
            {"fakeCoeff", .2f},
        }},
        {"Fast", new Dictionary<string, float>(){
            {"spawnRate", 1f},
            {"spawnVariance", .5f},
            {"lifeTime", 3f},
            {"fakeCoeff", .3f},
        }}
    };

    void Awake()
    {
        patternManager = FindObjectOfType<PatternManager>();
        modifiersManager = FindObjectOfType<ModifiersManager>();
        gameDefaultDuration = gameDuration;
        constraint = FindObjectOfType<Constraint>();
    }

    void Start()
    {

        // Initialization of the LoggerNotifier. Here we will only pass parameters to PersistentEvent, even if we will also raise Events.
        loggerNotifier = new LoggerNotifier(persistentEventsHeadersDefaults: new Dictionary<string, string>(){
            {"GameState", "NULL"},
            {"GameDuration", "NULL"},
            {"GameSpeed", "NULL"},
            {"GameTimeSpent", "NULL"},
            {"GameTimeLeft", "NULL"},
            {"ParticipantId", "NULL"},
            {"TestId", "NULL"}
        });
        // Initialization of the starting values of the parameters.
        loggerNotifier.InitPersistentEventParameters(new Dictionary<string, object>(){
            {"GameState", System.Enum.GetName(typeof(GameDirector.GameState), gameState)},
            {"GameDuration", gameDuration.ToString()},
            {"GameSpeed", gameDifficulty},
            {"GameTimeSpent", 0},
            {"GameTimeLeft",gameDuration},
            {"ParticipantId", 0},
            {"TestId", 0}
        });
    }

    public void CountDownGame() {
        if (gameState == GameState.Playing) return;
        UpdateState(GameState.CountDown);

        if (patternManager.PatternLoaded()) {
            // Treatment Programs build their own countdowns.
            StartGame();
        } else {
            StartCoroutine(WaitStartGame(gameWarmUpTime));
        }
    }

    // Starts the game.
    public void StartGame()
    {
        //constraint.SetReset();

        if (gameState == GameState.Playing) return;
        LoadDifficulty();
        modifiersManager.LogState();

        if(patternManager.PlayPattern())
        {
            gameDuration = patternManager.GetPatternDuration();
            loggerNotifier.NotifyLogger(overrideEventParameters: new Dictionary<string, object>()
            {
                {"GameDuration", patternManager.GetPatternDuration()}
            });
            StartCoroutine(WaitEndGame(patternManager.GetPatternDuration()));
        }
        else
        {
            gameDuration = gameDefaultDuration;
            loggerNotifier.NotifyLogger(overrideEventParameters: new Dictionary<string, object>()
            {
                {"GameDuration", gameDuration}
            });
            wallManager.Enable();
            StartMoleTimer(gameWarmUpTime);
            StartCoroutine(WaitEndGame(gameDuration));
        }
        
        UpdateState(GameState.Playing);
        if (gazeRecorder != null) gazeRecorder.StartRecording();
        loggerNotifier.NotifyLogger("Game Started", EventLogger.EventType.GameEvent, new Dictionary<string, object>()
        {
            {"GameState", System.Enum.GetName(typeof(GameDirector.GameState), gameState)}
        });
    }

    // Stops the game.
    public void StopGame()
    {
        if (gameState == GameState.Stopped) return;

        var props = profileManager.GetSelectedProfileProperties();
        var metaLog = new Dictionary<string, object>()
        {
            {"SessionDuration", gameDuration - currentGameTimeLeft},
            {"SessionState", "Interrupted"},
            {"ProfileName", props["Name"]},
            {"Age", props["Age"]},
            {"Gender", props["Gender"]},
            {"Handedness", props["Handedness"]},
            {"InjuryDate", props["InjuryDate"]},
            {"ProfileGroup", props["Group"]},
            {"ProfileID", profileManager.GetSelectedProfileId()},
            {"ParticipantID", participantId},
            {"testID", testId}
        };
        loggingManager.Log("Meta", metaLog);
        loggerNotifier.NotifyLogger("Game Stopped", EventLogger.EventType.GameEvent, new Dictionary<string, object>()
        {
            {"GameState", System.Enum.GetName(typeof(GameDirector.GameState), gameState)}
        });
        FinishGame();
    }

    // Pauses/unpauses the game.
    public void PauseUnpauseGame()
    {
        if (gameState == GameState.Stopped) return;

        if(gameState == GameState.Playing)
        {
            patternManager.PauseUnpausePattern(true);
            UpdateState(GameState.Paused);
            wallManager.SetPauseMole(true);
            loggerNotifier.NotifyLogger("Game Paused", EventLogger.EventType.GameEvent, new Dictionary<string, object>()
            {
                {"GameState", System.Enum.GetName(typeof(GameDirector.GameState), gameState)}
            });
        }
        else if(gameState == GameState.Paused)
        {
            patternManager.PauseUnpausePattern(false);
            UpdateState(GameState.Playing);
            wallManager.SetPauseMole(false);
            loggerNotifier.NotifyLogger("Game Unpaused", EventLogger.EventType.GameEvent, new Dictionary<string, object>()
            {
                {"GameState", System.Enum.GetName(typeof(GameDirector.GameState), gameState)}
            });
        }
    }

    // Sets the game duration.
    public void SetGameDuration(float duration)
    {
        if (gameState == GameState.Playing) return;
        gameDefaultDuration = duration;
    }

    public void SetParticipant(int participant)
    {
        participantId = participant;
        loggerNotifier.NotifyLogger("Participant ID Updated", EventLogger.EventType.DefaultEvent, new Dictionary<string, object>()
        {
            {"ParticipantId", 0},
        });
        participantChanged.Invoke(participantId);
    }

    public int GetParticipant()
    {
        return participantId;
    }

    public void SetTest(int test)
    {
        testId = test;
        loggerNotifier.NotifyLogger("Test ID Updated", EventLogger.EventType.DefaultEvent, new Dictionary<string, object>()
        {
            {"TestId", 0},
        });
        testChanged.Invoke(testId);
    }

    public int GetTest() {
        return testId;
    }

    // Sets the game difficulty.
    public void SetDifficulty(string difficulty)
    {
        if (difficulty == gameDifficulty) return;
        if (!difficulties.ContainsKey(difficulty)) return;
        gameDifficulty = difficulty;
        LoadDifficulty();

        loggerNotifier.NotifyLogger("Game Speed Changed To "+gameDifficulty, EventLogger.EventType.ModifierEvent, new Dictionary<string, object>()
            {
                {"GameSpeed", gameDifficulty}
            });
        
        speedUpdateEvent.Invoke("GameSpeed", gameDifficulty);
    }

    // Returns the Mole expiring duration
    public float GetMoleExpiringDuration()
    {
        return moleExpiringDuration;
    }

    // Returns the difficulty update event
    public UnityEvent<string, string> GetDifficultyUpdateEvent()
    {
        return speedUpdateEvent;
    }

    // Loads the difficulty.
    private void LoadDifficulty()
    {
        difficulties.TryGetValue(gameDifficulty, out difficultySettings);
    }

    // Updates the state of the game (playing, stopped, paused) and raises an event to notify any listener (UI, logger...).
    private void UpdateState(GameState newState)
    {
        gameState = newState;
        stateUpdate.Invoke(gameState);
    }

    private void FinishGame()
    {
        if (gameState == GameState.Stopped) return;
        UpdateState(GameState.Stopped);
        if (gazeRecorder != null) gazeRecorder.StopRecording();
        patternManager.StopPattern();
        StopAllCoroutines();
        wallManager.Disable();
        modifiersManager.SetDefaultModifiers();
    }

    private void SpawnMole(float lifeTime, bool fakeCoeff)
    {
        Mole.MoleType type = Mole.MoleType.Target;
        if (fakeCoeff) {
            if (((int)currentGameTimeLeft) % 2 == 0) {
                type = Mole.MoleType.DistractorLeft;
            } else {
                type = Mole.MoleType.DistractorRight;
            }
        }

        //find the position of the player when the mole appears
         mappedPayerPosition = basicPointer.MappedPosition;
        //playerPositionWhenMoleAppears = playerTransform.position;
         Mole selectedMole = wallManager.ActivateRandomMole(lifeTime, moleExpiringDuration, type);
        Vector3 coordonateOfMole = selectedMole.transform.position;
        //find the distance between the player and the mole
        float distance = Vector3.Distance(mappedPayerPosition, coordonateOfMole);
        MoleData moleData = new MoleData
        {
            MoleId = selectedMole.GetInstanceID(),
            Mole = selectedMole,
            Distance = distance
        };
        moleDataDict[moleData.MoleId] = moleData;
        distances.Add(distance);
        basicPointer.onMoleHit.AddListener((hitmole, hittime) => HandleMoleHit(hitmole, hittime));
    }

    void HandleMoleHit(Mole hitMole, float hitTime)
    {
        float reactionTime = hitTime - moleAppearTime;
        MoleData moleData;
        if (moleDataDict.TryGetValue(hitMole.GetInstanceID(), out moleData))
        {
            moleData.ReactionTime = reactionTime;
            moleData.Speed = moleData.Distance / reactionTime;
            Debug.Log("ID : "+ moleData.MoleId + "Speed: " + moleData.Speed + "reactionTime = " + reactionTime + "Distance" + moleData.Distance);

        }
        //float speed = distance / reactionTime;
        //speeds.Add(speed);

        basicPointer.onMoleHit.RemoveListener(HandleMoleHit);

    }


    private void StartMoleTimer(float setTime = -1)
    {
        if (setTime == -1)
        {
            float variance = Random.Range(-difficultySettings["spawnVariance"], difficultySettings["spawnVariance"]);
            spawnTimer = StartCoroutine(WaitSpawnMole(difficultySettings["spawnRate"] + variance));
        }
        else
        {
            spawnTimer = StartCoroutine(WaitSpawnMole(setTime));
        }
    }

    // Waits a given time before starting the game
    private IEnumerator WaitStartGame(float duration) {
        float currentCountDownLeft = duration;
        int currentCountDownLeftRounded = -1;
        int prevCount = -1;

        while (currentCountDownLeft > -0.9) {
            prevCount = currentCountDownLeftRounded;

            if (gameState == GameState.CountDown) {
                currentCountDownLeft -= Time.deltaTime;
            }
            currentCountDownLeftRounded = (int) Mathf.Ceil(currentCountDownLeft);
            
            if (currentCountDownLeftRounded != prevCount) {
                var data = new Dictionary<string, object>()
                {
                    {"CountDown", currentCountDownLeftRounded}
                };
                countUpdate.Invoke(currentCountDownLeftRounded);
                soundManager.PlaySound(gameObject, SoundManager.Sound.countdown);
                loggerNotifier.NotifyLogger("CountDown " + currentCountDownLeftRounded.ToString(), EventLogger.EventType.GameEvent);
            }
            yield return null;
        }

        StartGame();
    }

    // Waits a given time before activating a new Mole
    private IEnumerator WaitSpawnMole(float duration)
    {
        currentMoleTimeLeft = duration;
        while (currentMoleTimeLeft > 0)
        {
            if (gameState == GameState.Playing)
            {
                currentMoleTimeLeft -= Time.deltaTime;
            }
            yield return null;
        }
        OnSpawnMoleTimeout();
    }

    private void OnSpawnMoleTimeout()
    {
        SpawnMole(difficultySettings["lifeTime"], Random.Range(0f, 1f) <= difficultySettings["fakeCoeff"]);
        moleAppearTime = Time.time;
        StartMoleTimer();
    }

    // Waits a given time before stopping the game
    private IEnumerator WaitEndGame(float duration)
    {
        currentGameTimeLeft = duration;
        float currentGameDuration = duration;

        while (currentGameTimeLeft > 0)
        {
            if (gameState == GameState.Playing)
            {
                currentGameTimeLeft -= Time.deltaTime;
                timeUpdate.Invoke(currentGameTimeLeft);
                loggerNotifier.NotifyLogger(overrideEventParameters: new Dictionary<string, object>()
                {
                    {"GameTimeSpent", currentGameDuration - currentGameTimeLeft},
                    {"GameTimeLeft", currentGameTimeLeft}
                });
            }
            yield return null;
        }
        timeUpdate.Invoke(0f);

        loggerNotifier.NotifyLogger(overrideEventParameters: new Dictionary<string, object>()
        {
            {"GameTimeSpent", gameDuration},
            {"GameTimeLeft", 0f}
        });

        OnGameEndTimeout();
    }

    private void OnGameEndTimeout()
    {
        var props = profileManager.GetSelectedProfileProperties();

        var metaLog = new Dictionary<string, object>()
        {
            {"SessionDuration", gameDuration - currentGameTimeLeft},
            {"ProfileName", props["Name"]},
            {"Age", props["Age"]},
            {"Gender", props["Gender"]},
            {"Handedness", props["Handedness"]},
            {"InjuryDate", props["InjuryDate"]},
            {"ProfileGroup", props["Group"]},
            {"ProfileID", profileManager.GetSelectedProfileId()},
            {"SessionState", "Finished"},
            {"ParticipantID", participantId},
            {"TestID", testId}
        };
        loggingManager.Log("Meta", metaLog);
        loggerNotifier.NotifyLogger("Game Finished", EventLogger.EventType.GameEvent, new Dictionary<string, object>()
        {
            {"GameState", System.Enum.GetName(typeof(GameDirector.GameState), gameState)}
        });
        FinishGame();
    }

    void OnApplicationQuit() {
        gazeRecorder.StopRecording();
    }

}
