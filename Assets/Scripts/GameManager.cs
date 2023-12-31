using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set;}
    [SerializeField] Transform endScreen;
    [SerializeField] Transform tutorialScreen;

    [SerializeField] TextMeshProUGUI turnCounterVisual;

    [SerializeField] TextMeshProUGUI roundCounterVisual;
    public string pfpResourcePath = "sample_avatar";
    private int turnCounter = 1;
    private int targetCounter = 1;

    private UserProfile currentTarget;
    private UserProfileDB usersDB;
    private TimelineTweetDB timelineTweetDB;
    private SelectableTweetDB selectableTweetDB;
    private TraitDB traitDB;
    
    private List<Tweet> bangerTweets;
    private List<UserAgendaCompletion> metTargets;



    private int agendaScore = 0;
    private int boredomLevel = 0;

    private const int MAX_AGENDA_SCORE = 6;
    private const int MIN_AGENDA_SCORE = 0;
    private const int MAX_BOREDOM_LEVEL = 15;
    private const int MIN_BOREDOM_LEVEL = 0;
    private bool baseTimerFlag = false;
    private float baseTimer = 0;
    private float BASE_TIMER_TIME = 20;    
    private bool boredomTimerFlag = false;
    private float boredomTimer = 0;
    private float BOREDOM_TIMER_TIME = 15;  
    private void Awake() {
        if ( Instance != null)
        {
            Debug.LogError("There's more than once GameManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentTarget = new UserProfile();
        usersDB = new UserProfileDB();
        timelineTweetDB = new TimelineTweetDB();
        selectableTweetDB = new SelectableTweetDB();
        traitDB = new TraitDB();
        usersDB.StartDB();
        timelineTweetDB.StartDB();
        selectableTweetDB.StartDB();
        traitDB.StartDB();

        bangerTweets = new List<Tweet>();
        metTargets = new List<UserAgendaCompletion>();
    }
    private void Update() {
        if (baseTimerFlag) 
        {
            baseTimer -= Time.deltaTime;
            if (baseTimer < 0)
            {
                EndBaseTimer();
            }
        }

        if (boredomTimerFlag)
        {
            boredomTimer -= Time.deltaTime;
            if (boredomTimer < 0)
            {
                ResetBoredomTimer();
            }
        }
    }

    private void StartBaseTimer()
    {
        baseTimerFlag = true;
        baseTimer = BASE_TIMER_TIME;
    }

    private void StopBaseTimer()
    {
        baseTimerFlag = false;
    }
    private void EndBaseTimer()
    {
        Debug.Log("BaseTimer timeout");
        StopBaseTimer();
        StartBoredomTimer();
    }

    private void StartBoredomTimer()
    {
        boredomTimerFlag = true;
        boredomTimer = BOREDOM_TIMER_TIME;
    }

    private void ResetBoredomTimer()
    {
        Debug.Log("Boredom timeout");
        boredomTimer = BOREDOM_TIMER_TIME;
        boredomLevel += 1;
        TargetInformationUI.Instance.UpdateBars(agendaScore, boredomLevel);
        if (boredomLevel == MAX_BOREDOM_LEVEL)
        {
            // User lost this target
            TimelineManager.Instance.CleanTimelineManager();
            SelectableTweetsManager.Instance.CleanSelectableTweetsManager();
            EndRound();
            return;
        }
    }

    private void StopBoredomTimer()
    {
        boredomTimerFlag = false;
    }
    private void Start() {
        tutorialScreen.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        tutorialScreen.gameObject.SetActive(false);
        StartRound();
    }

    public void StartRound()
    {
        // Setup new target
        SetNewTarget();

        // Reset turn counter
        turnCounter = 1;
        // Start turn -> load tweets to use
        StartTurn();
        roundCounterVisual.text = "TARGET INFORMATION \n ROUND " +  targetCounter.ToString() + "/3";
        Debug.Log("New round started");
    }
    public void StartTurn()
    {
        StopBaseTimer();
        StopBoredomTimer();
        StartBaseTimer();
        // Prepare new selectable tweets;
        List<Tweet> selectableTweets = new List<Tweet>();

        // selectableTweets.Add(selectableTweetDB.GetSpecificTweet(currentTarget.GetTraits()));
        foreach(Tweet tweet in selectableTweetDB.GetNumberOfSelectableTweets(5))
        {
            selectableTweets.Add(tweet);
        }

        SelectableTweetsManager.Instance.StartSelectableTweetsManager(selectableTweets);

        // Prepare timeline
        // Get mock Tweets
        List<Tweet> nonInteractableTweets = timelineTweetDB.GetNumberofTimelineTweets(3);

        // Set mock tweets
        TimelineManager.Instance.SetNonInteractableTweets(nonInteractableTweets);

        // Start timeline manager
        TimelineManager.Instance.StartTimelineManager();

        // Update round counter
        turnCounterVisual.text = "TARGET TIMELINE - TURN " +  turnCounter.ToString() + "/3";
    }

    public void SetNewTarget()
    {
        // Get new target from database
        currentTarget = usersDB.PopUserProfile();
        string[] traitsForTarget = traitDB.GetNumOfTraits(3);
        currentTarget.traits = traitsForTarget;

        TargetInformationUI.Instance.SetTargetUser(currentTarget);
        
        // Reset agenda and boredom levels
        agendaScore = 0;
        switch (targetCounter)
        {
            case 1:
                boredomLevel = 0;
                break;

            case 2:
                boredomLevel = 3;
                break;

            case 3:
                boredomLevel = 6;    
                break;

            default:
                break;
        }
        TargetInformationUI.Instance.UpdateBars(agendaScore, boredomLevel);
    }
    public void EndTurn()
    {
        // Add points to meters    
        List<Tweet> selectedTweets = TimelineManager.Instance.GetListOfTimelineTweets();

        // Calculate and add agenda
        int addAgendaScore = 0;
        foreach(Tweet tweet in selectedTweets)
        {
            if (tweet.GetAgendaScore() == 2)
            {
                bangerTweets.Add(tweet);
            }
            addAgendaScore += tweet.GetAgendaScore();
        }
        Debug.Log($"Add Agenda score at the end of turn {addAgendaScore}");
        agendaScore += addAgendaScore;
        agendaScore = Mathf.Clamp(agendaScore, MIN_AGENDA_SCORE, MAX_AGENDA_SCORE);
        Debug.Log($"Agenda score at the end of turn {agendaScore}");

        // Calculate and add boredom level
        int countMatches = 0;
        int finalCount = 0;
        foreach(Tweet tweet in selectedTweets)
        {
            foreach(string trait in currentTarget.GetTraits())
            {
                string[] tweetTraits = tweet.GetTraits();
                if ( Array.IndexOf(tweetTraits, trait) > -1)
                {
                    countMatches++;
                }
            }
        }

        // Max 9 mistmaches - whatever match they had
        finalCount = 9 - countMatches;
        Debug.Log($"Count of traits at the end of turn {finalCount}");
        boredomLevel += finalCount;
        Debug.Log($"Boredom level {boredomLevel}");
        boredomLevel = Mathf.Clamp(boredomLevel, MIN_BOREDOM_LEVEL, MAX_BOREDOM_LEVEL);

        TargetInformationUI.Instance.UpdateBars(agendaScore, boredomLevel);
        // Clean Timeline and selectable data
        TimelineManager.Instance.CleanTimelineManager();
        SelectableTweetsManager.Instance.CleanSelectableTweetsManager();
        if (boredomLevel == MAX_BOREDOM_LEVEL)
        {
            // User lost this target
            TimelineManager.Instance.CleanTimelineManager();
            SelectableTweetsManager.Instance.CleanSelectableTweetsManager();
            TargetInformationUI.Instance.UpdateBars(0, boredomLevel);
            EndRound();
            return;
        }

        if (agendaScore >= 4)
        {
            // User already won?
            TargetInformationUI.Instance.UpdateBars(agendaScore, boredomLevel);
            EndRound();
            return;
        }
        turnCounter++;
        Debug.Log($"Turn counter: {turnCounter}");

        
        if (turnCounter == 4)
        {
            EndRound();
        }
        else
        {
            StartTurn();
        }
    }

    public void EndRound()
    {
        // Check if all targets done
        Debug.Log("Round ended");
        Debug.Log($"Agenda score at the end of round {agendaScore}");
        targetCounter++;
        // Check different agendaLevels, set flags
        if (agendaScore >= 4)
        {
            metTargets.Add(new UserAgendaCompletion(currentTarget, true));
        }
        else
        {
            metTargets.Add(new UserAgendaCompletion(currentTarget, false));
        }

        // If some stuff like enough targets done -> end game 
        if (targetCounter == 4)
        {
            StopBoredomTimer();
            EndGame();
        }
        else
        {
            StartRound();
        }
        // Else start new round
        

    }

    public void EndGame()
    {
        // Prepare outcome?
        int countWins = 0;
        foreach (UserAgendaCompletion user in metTargets)
        {
            if (user.influenced)
            {
                countWins++;
            }
        }

        EndScreenManager end = endScreen.GetComponent<EndScreenManager>();
        if (countWins >= 2)
        {
            end.SetData(bangerTweets, metTargets, true);
        }
        else
        {
            end.SetData(bangerTweets, metTargets, false);
        }

        end.SetEnabled(true);
        
    }

    public void GoBackToMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Main");
    }
}

public struct UserAgendaCompletion
{
    public UserAgendaCompletion(UserProfile user, bool inf)
    {
        userProfile = user;
        influenced = inf;
    }

    public UserProfile userProfile { get; }
    public bool influenced { get; }

    public override string ToString() => $"({userProfile.userName}, {influenced})";
}