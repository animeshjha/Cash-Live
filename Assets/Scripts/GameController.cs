
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    #region Variable Declartions

    public enum Status
    {
        IsActive,
        Paused,
        GameOver
    }

    public static Status GameStatus { get; set; }

    private Player[] Players;
    public int BetAmount_INPUT { get; set; }
    private GameTimer timerEvents;

    [SerializeField] private string SessionID;
    private UIController currentUIController;
    private DiceController currentDiceController;

    [SerializeField] private TMP_Text DebugDiceValue;
    [SerializeField] private GameObject StartGamePanel;
    [SerializeField] private TMP_Text StartGameTimer;
    [SerializeField] private GameObject StartGameTimerPanel;

    // Dice and Player Data
    public static int ActivePlayerID;               // Always holds, current active player info
    private int[] UI_PlayerData;
    private int[] DiceSum;
    private int[] betCashAmounts;
    private int pointNumber;
    private bool ComeOutRoll = false;
    private int currentPlayerBalance;
    private bool AI_PlacedBet = false;

    //Variables for Automatic Gameplay
    private bool wagerMatched = false;
    [SerializeField] private int GlobalWagerAmount = 200;
    private bool AutomateHumanTasks = true;

    private bool HumanPlayerBetPlaced;
    private int DebugDice_Value;

    // DELEGATE EVENTS, INCOMING
    public delegate void UpdateUI_GC(int[] data);
    public static event UpdateUI_GC UpdateUI_GameController;

    public static GameController Instance { get; private set; }

    #endregion

    // Start is called before the first frame update, INIT() equivalent
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        GameStatus = Status.GameOver;

        ActivePlayerID = 0;
        pointNumber = 0;
        HumanPlayerBetPlaced = false;
        // timerEvents = gameObject.AddComponent<GameTimer>();
        timerEvents = GetComponent<GameTimer>();
        DebugDice_Value = 0;
        betCashAmounts = new int[4] { 10, 20, 50, 100 };
        DiceSum = new int[3] { 0, 0, 0 };
        UI_PlayerData = new int[5] { 0, 0, 0, 0, 0 };              // Equivalent of JSON, used to pass data Async between GameController and UI Controller
        // 0 -> Bet Amount
        // 1 -> UnUsed
        // 2 -> Player Bet AMount
        // 3 -> Balance Amounts                                                       

        currentUIController = UIController.Instance;
        currentDiceController = GetComponent<DiceController>();
        if (currentDiceController == null)
            Debug.Log("Error: Please add Dice Controller script to Game Controller Object ");

        // INITIALIZE PLAYER DATA
        Players = new Player[3];

        Players[0] = new Player() { };
        Players[0].IsHuman = false;
        Players[0].PlayerLevel = Player.Level.Aggressive;        // Player one is Aggressive since childhood :)
        Players[0].Money = 1000;
        Players[0].IsShooter = false;
        Players[0].PlayerName = "Maniac";
        Players[0].BetAmount = 0;

        Players[1] = new Player() { };
        Players[1].IsHuman = false;
        Players[1].PlayerLevel = Player.Level.Conservative;      // Sibling
        Players[1].Money = 1000;
        Players[1].IsShooter = false;
        Players[1].PlayerName = "Scrooge";
        Players[1].BetAmount = 0;

        Players[2] = new Player() { };
        Players[2].IsHuman = true;
        Players[2].PlayerLevel = Player.Level.Neutral;           // Human / You
        Players[2].Money = currentPlayerBalance = 1000;
        Players[2].IsShooter = false;
        Players[2].PlayerName = "You";
        Players[2].BetAmount = 0;

        // ATTACH DELEGATE EVENTS
        DiceController.OnDiceLanded += PlayNextTurn;
        //DiceController.UpdateGameController += ReceivingFromDiceController;
        UIController.UpdateGameController += ReceivingFromUIController;
        UIController.UpdateGameController_BetPlaced += BetPlaced;
        UIController.PlayNextTurn += PlayNextTurn;
        GameTimer.TimedEvent += TimeOut;
    }

    public void PlayAutoGame()
    {
        AutomateHumanTasks = true;
        StartGamePanel.SetActive(false);
        Debug.Log("<color=cyan>------------- Starting Automated Game -----------------</color>");
        currentUIController.CasinoBot.SetActive(true);
        StartGameTimerPanel.SetActive(true);
        timerEvents.callerID = 7;
        timerEvents.StartTimer(4);
    }

    public void PlayNormalGame()
    {
        AutomateHumanTasks = false;
        GlobalWagerAmount = 0;
        StartGamePanel.SetActive(false);
        Debug.Log("<color=cyan>------------- Starting Normal Game -----------------</color>");
        currentUIController.CasinoBot.SetActive(true);
        StartGameTimerPanel.SetActive(true);
        timerEvents.callerID = 7;
        timerEvents.StartTimer(4);

    }

    public void NewGame()
    {
       // Debug.Log("<color=cyan>------------- Starting Normal Game -----------------</color>");
        SessionID = Random.Range(1, 100).ToString();             // Equivalent of Generating GUID per unique session  
        GameStatus = Status.IsActive;
        ResetVariableState();
        ResetVariableValues();
        IdentifyShooter();
    }

    void ResetVariableState()
    {
        HumanPlayerBetPlaced = false;
        AI_PlacedBet = false;
        ComeOutRoll = false;
        currentDiceController.PlayerPlayedDice = false;
        currentUIController.PointNumber_Panel.SetActive(false);
        currentUIController.ExitButton.SetActive(true);
        currentUIController.CasinoBot.SetActive(true);
    }

    void ResetVariableValues()
    {
        ActivePlayerID = 0;
        pointNumber = 0;
        DebugDice_Value = 0;
        currentUIController.Message = " ";
        DiceSum[0] = DiceSum[1] = DiceSum[2] = 0;
        currentUIController.PointNumber = 0;
        Players[0].Money = Players[1].Money = Players[2].Money = 1000;
        Players[0].BetAmount = Players[1].BetAmount = Players[2].BetAmount = 0;
        UI_PlayerData[0] = UI_PlayerData[1] = UI_PlayerData[2] = UI_PlayerData[3] = UI_PlayerData[4] = 0;
    }

    public void PlayNextTurn()
    {
        if (GameStatus == Status.IsActive)
            PlayDice();
    }

    public void IdentifyShooter()
    {
        if (GameStatus == Status.GameOver || GameStatus == Status.Paused)           // Don't play turn if Game is paused or over
            return;

        int rand = Random.Range(0, 3); // Identify Shooter randomly from three players
        string message = "";


        switch(rand)
        {
            case 0:
                message = "Maniac is the Shooter ";
                Players[0].IsShooter = true;
                Players[1].IsShooter = false;
                Players[2].IsShooter = false;
                break;
            case 1:
                message = "Scrooge is the Shooter ";
                Players[0].IsShooter = false;
                Players[1].IsShooter = true;
                Players[2].IsShooter = false;
                break;
            case 2:
                message = "You are the Shooter " ;
                Players[0].IsShooter = false;
                Players[1].IsShooter = false;
                Players[2].IsShooter = true;
                break;
        }

        // DEbug .. Force human player true
        //rand = 2;
        ActivePlayerID = rand;
        currentUIController.IdentifyShooter(rand);
        currentUIController.Message = message;

        Betting();
    }

    public void Betting()
    {
        AI_PlacedBet = false;
        currentUIController.AnimatePlayerPanel(ActivePlayerID);
        currentUIController.GlobalWagerAmount.SetActive(true);
        //Debug.Log("Betting: " + ActivePlayerID);
        switch (ActivePlayerID)
        { 
            case 0:
                //currentUIController.MessageBox.SetActive(false);
                timerEvents.callerID = 3;
                timerEvents.StartTimer(2);
                Debug.Log("Red Betting: " + ActivePlayerID);
                break;

            case 1:
                //currentUIController.MessageBox.SetActive(false);
                timerEvents.callerID = 4;
                timerEvents.StartTimer(2);
                Debug.Log("Blue Betting: " + ActivePlayerID);

                break;

            case 2:

                if (AutomateHumanTasks)
                {
                    //currentUIController.MessageBox.SetActive(false);
                    timerEvents.callerID = 5;
                    timerEvents.StartTimer(2);
                    Debug.Log("HUman AUto Betting: " + ActivePlayerID);
                }
                else
                {
                    Debug.Log(" Human player, wait for input ");
                    currentUIController.Message = "Please place bet... ";
                }
                break;
        }
    }

    void PlayerBets()
    {

        //Debug.Log("PLayerBets():  Active["+ ActivePlayerID +"] 0: "+ Players[0].Money + " 1: " + Players[1].Money + " 1: " + Players[2].Money);

        if (Players[ActivePlayerID].IsShooter && !AI_PlacedBet)
        {
            //wagerAmount = betCashAmounts[Random.Range(2, 3)];          
            if (ActivePlayerID == 0)
            {
                Players[ActivePlayerID].BetAmount += betCashAmounts[Random.Range(2, 4)];
                AI_PlacedBet = true;
            }


            if (ActivePlayerID == 1)
            {
                AI_PlacedBet = true;
                Players[ActivePlayerID].BetAmount += betCashAmounts[Random.Range(0, 1)];
            }
                

            if (ActivePlayerID == 2)
            {
                currentUIController.FA_Panel(0, true);
                currentUIController.FA_Panel(1, true);
                currentUIController.PlayForAgainstAI(2, true); // Human always plays FOR

                if (AutomateHumanTasks)
                {
                    //Debug.Log(" ADD AUTOMATIC BET AMOUNT ");
                    AI_PlacedBet = true;
                    Players[ActivePlayerID].BetAmount += betCashAmounts[Random.Range(0, 4)];
                    GlobalWagerAmount = Players[ActivePlayerID].BetAmount;
                }
                else
                {
                    if (!HumanPlayerBetPlaced)
                    {
                        Debug.Log("<color=red> Human Player not placed bet ");
                        //currentUIController.MessageBox.SetActive(true);
                        currentUIController.Message = "Please place Bet ";
                        return;
                    }
                }
            }
            else
                currentUIController.FA_Panel(ActivePlayerID, false);

            Debug.Log("Player Playing (Shooter) : " + ActivePlayerID + " Amount: " + Players[ActivePlayerID].BetAmount);
            // TODO: FIx wager to shooter
            GlobalWagerAmount = Players[ActivePlayerID].BetAmount;

            UI_PlayerData[0] = Players[ActivePlayerID].BetAmount; 
            UI_PlayerData[2] = Players[2].Money;
            UI_PlayerData[3] = Players[ActivePlayerID].Money - Players[ActivePlayerID].BetAmount;
            UI_PlayerData[4] = Players[2].BetAmount;
            UpdateUI_GameController(UI_PlayerData);

        }
        else
        {
            Debug.Log("Player Playing (Non Shooter) : " + ActivePlayerID);

            if (Players[ActivePlayerID].BetAmount < GlobalWagerAmount)
            {
                if (ActivePlayerID == 0)
                    Players[ActivePlayerID].BetAmount += betCashAmounts[Random.Range(2, 4)];

                if (ActivePlayerID == 1)
                    Players[ActivePlayerID].BetAmount += betCashAmounts[Random.Range(0, 1)];

                if(ActivePlayerID == 2)
                {
                    if (AutomateHumanTasks)
                    {

                        Debug.Log(" ADD AUTOMATIC BET AMOUNT ");
                        Players[ActivePlayerID].BetAmount += betCashAmounts[Random.Range(0, 4)];

                    }
                    else
                    {
                        HumanPlayerBetPlaced = false;
                        //currentUIController.MessageBox.SetActive(true);
                        currentUIController.Message = "Please place Bet ";
                        return;
                    }
                }
               

            }


            if (Players[ActivePlayerID].BetAmount <= GlobalWagerAmount)
                wagerMatched = false;

            if (Players[ActivePlayerID].BetAmount >= GlobalWagerAmount)
                wagerMatched = true;

            Debug.Log("Player Playing (Non Shooter) : " + ActivePlayerID + "BetAmount: " + Players[ActivePlayerID].BetAmount);
            
            currentUIController.FA_Panel(ActivePlayerID, true);

            bool BettingFor;
            int random = Random.Range(0, 5);     // Generate chance of betting against
            if (random % 2 == 0)
                BettingFor = true;
            else
                BettingFor = false;

     
            currentUIController.PlayForAgainstAI(ActivePlayerID, BettingFor);

            if (!AutomateHumanTasks)
                currentUIController.PlayForAgainstAI(ActivePlayerID, true); // Default is always for

            UI_PlayerData[0] = Players[ActivePlayerID].BetAmount;
            UI_PlayerData[2] = Players[2].Money;
            UI_PlayerData[3] = Players[ActivePlayerID].Money - Players[ActivePlayerID].BetAmount;
            UI_PlayerData[4] = Players[2].BetAmount;
            UpdateUI_GameController(UI_PlayerData);

        }

        if (ActivePlayerID == 2)
            currentUIController.PlayForAgainstAI(2, true); // Human always plays FOR

        currentUIController.GlobalWagerAmountText.text = GlobalWagerAmount.ToString();

        Debug.Log("<color=magenta> Wager Amount: " +GlobalWagerAmount + " Matched: " + wagerMatched +"</color>");

        //Debug.Assert(wagerMatched == true, "Wagermatched" );
        if (!wagerMatched)
        {
            if (ActivePlayerID == 2)
            {
                if (!AutomateHumanTasks)
                {
                    if (!HumanPlayerBetPlaced)
                    {
                        currentUIController.Message = "Please place Bet";
                        return;
                    }
                }                 
                else
                {
                    currentUIController.Message = "Wager not Matched, continue Betting..";
                    TurnEnd(); // Human player is Automated
                }

            }
            else
            {
                currentUIController.Message = "Wager not Matched, continue Betting..";
                TurnEnd();

            }
        }            
        else
        {
            Debug.Log(" ----- WAGER MATCHED  -------  BETTING ROUND COMPLETE: " + GlobalWagerAmount);

            // IMP: Identification of active player is needed since, betting can stop at any player.
            for (int i = 0; i < 3; i++)
            {
                if (Players[i].IsShooter == true)
                    ActivePlayerID = i;
            }

            // Stop rolling human player, Debug
            PlayDice();
        }

    }

    public void TurnEnd()
    {
        Debug.Log(" Turn end for player "+ ActivePlayerID);
        if (ActivePlayerID < 2)
            ActivePlayerID++;
        else
            ActivePlayerID = 0;
        Debug.Log(" Turn Begin for player " + ActivePlayerID);
        //PlayerBets();
        Betting();
    }

    public void PlayDice()
    {
        if (GameStatus == Status.GameOver || GameStatus == Status.Paused)           // Don't play turn if Game is paused or over
            return;

        currentUIController.Message = "";
        //currentUIController.MessageBox.SetActive(false);
        Debug.Log(" Playing Dice for player: " + ActivePlayerID);
        switch (ActivePlayerID)
        {
            case 0: // RED PLAYER
                {

                    if (Players != null)
                        if (Players[0].IsShooter)
                        {
                            
                            currentDiceController.RollDice();

                            currentUIController.AnimatePlayerPanel(ActivePlayerID);
                            timerEvents.callerID = 0;
                            timerEvents.StartTimer(4);

                        }
                }
                break;

            case 1:  // BLUE PLAYER
                {

                    if (Players != null)
                        if (Players[1].IsShooter)
                        {
                            currentDiceController.RollDice();

                            currentUIController.AnimatePlayerPanel(ActivePlayerID);
                            timerEvents.callerID = 1;
                            timerEvents.StartTimer(4);

                        }
                }

                break;

            case 2: // HUMAN PLAYER
               
                if (!Players[2].IsShooter)
                    currentUIController.DisableRoll();
                else
                {
                    //Debug.Log("Auto Bet Roll Dice: ");
                    if(AutomateHumanTasks)
                    {
                        currentDiceController.RollDice();
                        currentUIController.AnimatePlayerPanel(ActivePlayerID);
                        timerEvents.callerID = 2;
                        timerEvents.StartTimer(4);
                    }                        
                    else
                    {
                        //currentUIController.AnimatePlayerPanel(ActivePlayerID);
                       
                        currentUIController.EnableRoll();
                        //currentUIController.MessageBox.SetActive(true);
                        currentUIController.Message = "Please roll the Dice";
                        currentUIController.AnimatePlayerPanel(ActivePlayerID);
                        //currentDiceController.PlayerPlayedDice = false;

                        timerEvents.callerID = 6;
                        timerEvents.StartTimer(4);


                        if (!currentDiceController.PlayerPlayedDice)
                        {
                            currentDiceController.RollDice();
                            //timerEvents.callerID = 6;
                            //timerEvents.StartTimer(3);

                        }
                        //if (GameStatus == Status.IsActive)
                        //{
                        //    PlayDice();

                        //}
                    }

                    // TODO: CHeck if gameflow continues
                }

                break;
        }

        return;
        //Debug.Log("Dice Values: [" + DiceSum[0] + "],[" + DiceSum[1] + "],[" + DiceSum[2] + "]");

    }

    public void ReceivingFromUIController(int[] val)
    {
       
        if (currentPlayerBalance > 0)
        {

            BetAmount_INPUT += val[0];

            currentPlayerBalance = Players[2].Money - BetAmount_INPUT;

            UI_PlayerData[4] = BetAmount_INPUT;

            UI_PlayerData[2] = currentPlayerBalance;

            UpdateUI_GameController(UI_PlayerData);
        }
        Debug.Log(" 0: " + Players[0].Money + " 1: " + Players[1].Money + " 1: " + Players[2].Money);
        Debug.Log(" Receiving data from UI Controller, Bet:" + Players[2].BetAmount + " Balance: " + currentPlayerBalance);

    }

    public void BetPlaced()
    {

        //if (ActivePlayerID != 2)
        //    return;

        Debug.Log(" HUMAN PLAYER BET PLACED ");
        // TODO: CHeck bugs on shooter
        if (Players[2].IsShooter)
            GlobalWagerAmount = BetAmount_INPUT;

       
        Players[2].BetAmount = BetAmount_INPUT;

        //var Balance = Players[2].Money - BetAmount_INPUT;
        //UI_PlayerData[1] = BetAmount_INPUT;
        UI_PlayerData[2] = Players[2].Money = currentPlayerBalance;

        BetAmount_INPUT = 0;
        UI_PlayerData[4] = 0; // Dice Sum

        UpdateUI_GameController(UI_PlayerData);
        currentUIController.GlobalWagerAmountText.text = GlobalWagerAmount.ToString();
        HumanPlayerBetPlaced = true;
        //Debug.Log(" 0: " + Players[0].Money + " 1: " + Players[1].Money + " 1: " + Players[2].Money);
        TurnEnd();
    }

    void TimeOut(int[] EventData)
    {
        //Debug.Log("Game controller Timer Receive:  " + EventData[0]);

        if (EventData[0] == 7)
        {
            if(StartGameTimerPanel != null)
            StartGameTimerPanel.SetActive(false);
            NewGame();
        }

        if (EventData[0] == 3)
        {
            Debug.Log("Red player Betting ");
            PlayerBets();
        }

        if (EventData[0] == 4)
        {
            Debug.Log("Blue player Betting ");
            PlayerBets();
        }

        if (EventData[0] == 5)
        {
            Debug.Log("Human player Betting ");
            PlayerBets();
        }

        // Continue Rolling after Timed Event

        if (EventData[0] == 0)
        {
            DiceSum[0] = currentDiceController.GetDiceValue[0] + currentDiceController.GetDiceValue[1];

            UI_PlayerData[1] = DiceSum[0];

            EvaluateRoll();

            UpdateUI_GameController(UI_PlayerData);

            if (GameStatus == Status.IsActive)
            {
                PlayDice();
            }
        }

        if (EventData[0] == 1)
        {
            DiceSum[1] = currentDiceController.GetDiceValue[0] + currentDiceController.GetDiceValue[1];

            UI_PlayerData[1] = DiceSum[1];

            EvaluateRoll();

            UpdateUI_GameController(UI_PlayerData);

            if (GameStatus == Status.IsActive)
            {
                PlayDice();
            }
        }

        if (EventData[0] == 2)
        {
            DiceSum[2] = currentDiceController.GetDiceValue[0] + currentDiceController.GetDiceValue[1];


            UI_PlayerData[1] = DiceSum[2];
            EvaluateRoll();
            UpdateUI_GameController(UI_PlayerData);


            if (GameStatus == Status.IsActive)
            {
                if (AutomateHumanTasks)
                    PlayDice();
                //else
                //{
                //    currentUIController.MessageBox.SetActive(true);
                //    currentUIController.Message = "Please Re-roll";
                //    timerEvents.callerID = 2;
                //    timerEvents.StartTimer(3);
                //}

            }
        }

        if (EventData[0] == 6)
        {
            Debug.Log("Human player Betting: Dice value " + currentDiceController.PlayerPlayedDice);
            //PlayerDiceLaunched = true;

            if (currentDiceController.PlayerPlayedDice)
            {
                // currentDiceController.PlayerPlayedDice = false;
                if (GameStatus == Status.IsActive)
                {
                    //currentDiceController.PlayerPlayedDice = false;
                    PlayDice();
                }

                DiceSum[2] = currentDiceController.GetDiceValue[0] + currentDiceController.GetDiceValue[1];

                UI_PlayerData[1] = DiceSum[2];
                EvaluateRoll();
                UpdateUI_GameController(UI_PlayerData);

            }
            else
                PlayDice();

            Debug.Log("Human player Betting AFTER: Dice value " + currentDiceController.PlayerPlayedDice);
        }


    }

    public void DebugDice(Slider slider)
    {
        DebugDice_Value = (int)slider.value;
        DebugDiceValue.text = slider.value.ToString();
    }

    public void SetDiceValue_Debug()
    {
        DiceSum[ActivePlayerID] = DebugDice_Value;
        currentUIController.UpdateDiceValue(DiceSum[ActivePlayerID]);
    }

    void EvaluateRoll()
    {

        if (GameStatus == Status.GameOver || GameStatus == Status.Paused)
            return;

        int val = 0;
        
        Debug.Log("[" + ActivePlayerID + "] Dice: [" + DiceSum[ActivePlayerID] + "] comeout: " + ComeOutRoll + " POINT : " + pointNumber);

        currentUIController.UpdateDiceValue(DiceSum[ActivePlayerID]);

        if (!ComeOutRoll)
        {
            ComeOutRoll = true;

            if (DiceSum[ActivePlayerID] == 7 || DiceSum[ActivePlayerID] == 11)
                val = 0;

            if (DiceSum[ActivePlayerID] == 2 ||
                DiceSum[ActivePlayerID] == 3 ||
                DiceSum[ActivePlayerID] == 12)
                val = 1;

            if (DiceSum[ActivePlayerID] == 4 ||
                DiceSum[ActivePlayerID] == 5 ||
                DiceSum[ActivePlayerID] == 6 ||
                DiceSum[ActivePlayerID] == 8 ||
                DiceSum[ActivePlayerID] == 9 ||
                DiceSum[ActivePlayerID] == 10)
                val = 2;

            switch (val)
            {
                case 0:

                       // currentUIController.MessageBox.SetActive(true);
                        currentUIController.Message = Players[ActivePlayerID].PlayerName + " win the Game";
                        GameStatus = Status.GameOver;
                        StartGamePanel.SetActive(true);
                        currentUIController.ExitButton.SetActive(false);
                        //CasinoBot.SetActive(false);
                    break;
                case 1:

                       // currentUIController.MessageBox.SetActive(true);
                        currentUIController.Message = Players[ActivePlayerID].PlayerName + " lost the Game";
                        GameStatus = Status.GameOver;
                        StartGamePanel.SetActive(true);
                        currentUIController.ExitButton.SetActive(false);
                        //CasinoBot.SetActive(false);

                    break;

                case 2:

                        Debug.Log("------------------------ POINT : "+DiceSum[ActivePlayerID] +"--------------------------");
                       // currentUIController.MessageBox.SetActive(true);
                        currentUIController.Message = " POINT, Re Roll ";
                        currentUIController.PointNumber_Panel.SetActive(true);
                        currentUIController.PointNumber = DiceSum[ActivePlayerID];
                        pointNumber = DiceSum[ActivePlayerID];
                        GameStatus = Status.IsActive;

                    break;

                 
            }
        }
        else
        {
            Debug.Log("------------------------ COME OUT DONE ----------------------------- POINT: " + pointNumber);
            if (DiceSum[ActivePlayerID] == 7)
            {
                //currentUIController.MessageBox.SetActive(true);
                currentUIController.Message = Players[ActivePlayerID].PlayerName + " lost the Game";
                GameStatus = Status.GameOver;
                StartGamePanel.SetActive(true);
                currentUIController.ExitButton.SetActive(false);
                //CasinoBot.SetActive(false);
            }
            else
            {
                if(DiceSum[ActivePlayerID] == pointNumber)
                {
                    //currentUIController.MessageBox.SetActive(true);
                    currentUIController.Message = Players[ActivePlayerID].PlayerName + " win the Game";
                    GameStatus = Status.GameOver;
                    StartGamePanel.SetActive(true);
                    currentUIController.ExitButton.SetActive(false);
                    //CasinoBot.SetActive(false);
                }
                else
                if (DiceSum[ActivePlayerID] == 2 ||
                    DiceSum[ActivePlayerID] == 3 ||
                    DiceSum[ActivePlayerID] == 4 ||
                    DiceSum[ActivePlayerID] == 5 ||
                    DiceSum[ActivePlayerID] == 6 ||
                    DiceSum[ActivePlayerID] == 8 ||
                    DiceSum[ActivePlayerID] == 9 ||
                    DiceSum[ActivePlayerID] == 10 ||
                    DiceSum[ActivePlayerID] == 11 ||
                    DiceSum[ActivePlayerID] == 12)
                {   
                    //currentUIController.MessageBox.SetActive(true);
                    currentUIController.Message = " POINT, Re Roll ";
                    currentUIController.PointNumber_Panel.SetActive(true);
                    GameStatus = Status.IsActive;
                }
            }          

        }       


    }

    private void Update()
    {
        StartGameTimer.text = (timerEvents.TimeLeft).ToString();

        // TEST CASE for Roll EVALUATION, Press Space bar after hittng play key
        //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //    var random = Random.Range(2, 13);
        //    ActivePlayerID = 1;
        //    DiceSum[ActivePlayerID] = random;
        //    Debug.Log(random);            
        //    EvaluateRoll();
        //}
    }

    //void ReceivingFromDiceController(int[] data)
    //{
    //    //Debug.Log(" Recieving from dice controller");
    //    ////return;
    //    //if (ActivePlayerID != 2)
    //    //    return;
    //    //PlayerDiceLaunched = true; 
    //    //DiceSum[ActivePlayerID] = data[0] + data[1];
    //}
}
