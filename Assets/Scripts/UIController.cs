using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{

    #region EDITOR Inspector Fields
    // UI Elements placed into Inspector Fields (populated through the Editor)

    public TMP_Text DiceTotalTextField;
    public GameObject DiceTextPanel;

    public GameObject PlayerBettingPanel;
    public TMP_Text PlayerBettingAmount;

    public GameObject RedShooter;
    public GameObject BlueShooter;
    public GameObject HumanShooter;
    public GameObject GlobalWagerAmount;
    public TMP_Text GlobalWagerAmountText;
    public GameObject CasinoBot;

    public Button Button_10;
    public Button Button_20;
    public Button Button_50;
    public Button Button_100;

    public GameObject PointNumber_Panel;
    public TMP_Text PointNumber_TextField;
    public GameObject MessageBox;
    public TMP_Text MessageBoxTextField;

    public GameObject ExitButton;
    public GameObject ExitPanel;
    public Button RollDiceButton;

    public GameObject RedPlayer_FA_Panel;
    public GameObject BluePlayer_FA_Panel;
    public Toggle RedPlayerPlayFor;
    public Toggle RedPlayerPlayAgainst;
    public Toggle BluePlayerPlayFor;
    public Toggle BluePlayerPlayAgainst;
    public Toggle HumanPlayerPlayFor;
    public Toggle HumanPlayerPlayAgainst;

    public Button PlaceBetButton;

    public GameObject RedPlayerPanel;
    public TMP_Text RedPlayer_BetAmount;
    public GameObject BluePlayerPanel;
    public TMP_Text BluePlayer_BetAmount;
    public TMP_Text HumanPlayerBetAmount;
    public TMP_Text HumanPlayerBalance;

    public Image[] BetAmountImage;

    public Image RedPlayerStatus;
    public Image BluePlayerStatus;
    public Image HumanPlayerStatus;

    public Color BetSelectionColor;

    private int[] gameControllerData;
    private bool animateRedPlayer;
    private bool animateBluePlayer;
    private bool animateHumanPlayer;
    #endregion

    public delegate void Update_GameController(int[] data);
    public static event Update_GameController UpdateGameController;

    public delegate void Update_GameController_PlaceBet();
    public static event Update_GameController_PlaceBet UpdateGameController_BetPlaced;

    public delegate void Update_GameController_PlayNext();
    public static event Update_GameController_PlayNext PlayNextTurn;

    public static UIController Instance;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
     
    }
    
    void Start()
    {
        gameControllerData = new int[2] { 0, 0 };
        GameController.UpdateUI_GameController += UpdateUI_GameController;
    }

    public void LoadStartScene()
    {
        SceneManager.LoadScene("Start Scene", LoadSceneMode.Single);
    }

    public string Message
    {
        set { MessageBoxTextField.text = value; }
    }

    public int PointNumber
    {
        set { PointNumber_TextField.text = value.ToString(); }
    }

    public void OnClick_PlaceBet()
    {
        UpdateGameController_BetPlaced();       // Transfer's Click even to Game Controller
    }

    public void DisableRoll()
    {
        RollDiceButton.interactable = false;
    }

    public void EnableRoll()
    {
        RollDiceButton.interactable = true;
    }

    public void IdentifyShooter(int id)
    {
        RedShooter.SetActive(false);
        BlueShooter.SetActive(false);
        HumanShooter.SetActive(false);

        if (id == 0)
            RedShooter.SetActive(true);

        if(id == 1)
            BlueShooter.SetActive(true);

        if(id == 2)
            HumanShooter.SetActive(true);

     }

    public void FA_Panel(int ID, bool val)
    {
        if (ID == 0)
            RedPlayer_FA_Panel.SetActive(val);

        if (ID == 1)
            BluePlayer_FA_Panel.SetActive(val);
    }

    public void PlayForAgainstAI(int ID, bool val)
    {
        if(ID == 0)
        {
            if (val)
            {
                RedPlayerPlayFor.isOn = true;
                RedPlayerPlayAgainst.isOn = false;
            }
            else
            {
                RedPlayerPlayFor.isOn = false;
                RedPlayerPlayAgainst.isOn = true;
            }
        }

        if(ID == 1)
        {
            if (val)
            {
                BluePlayerPlayFor.isOn = true;
                BluePlayerPlayAgainst.isOn = false;
            }
            else
            {
                BluePlayerPlayFor.isOn = false;
                BluePlayerPlayAgainst.isOn = true;
            }
        }

        // When Human player is Automated:
        if(ID == 2)
        {
            if (val)
            {
                HumanPlayerPlayFor.isOn = true;
                HumanPlayerPlayAgainst.isOn = false;
            }
            else
            {
                HumanPlayerPlayFor.isOn = false;
                HumanPlayerPlayAgainst.isOn = true;
            }

        }    
    }

    void UpdateUI_GameController(int[] data)
    {
        //Debug.Log(data[0] + " | "+ data[1] + " | " + data[2] + " | " + data[3] + " | " + data[4]);
        // INDEX [0]: Player.BetAmount 

        if (GameController.ActivePlayerID == 0)
            RedPlayer_BetAmount.text = data[0].ToString();

        if (GameController.ActivePlayerID == 1)
            BluePlayer_BetAmount.text = data[0].ToString();

        //if (GameController.ActivePlayerID == 2)
        HumanPlayerBetAmount.text = data[4].ToString();
         
        // INDEX [3]: Player.Money // Balance
        Button_10.interactable = Button_20.interactable = Button_50.interactable = Button_100.interactable = true;

        if (data[2] < 10)
            Button_10.interactable = false;

        if (data[2] < 20)
            Button_20.interactable = false;

        if (data[2] < 50)
            Button_50.interactable = false;

        if (data[2] < 100)
            Button_100.interactable = false;

        HumanPlayerBalance.text = data[2].ToString();

    }

    public void NewGameButtonPressed()
    {
        GameController.GameStatus = GameController.Status.IsActive;
    }

    public void ReplayButtonPressed()
    {
        GameController.GameStatus = GameController.Status.IsActive;
    }

    public void Exit()
    {
        GameController.GameStatus = GameController.Status.Paused;

    }
        
    public void GetBetAmount(int amount)
    {
        //Debug.Log("Betting Amount: " + amount);

        gameControllerData[0] = amount;
        UpdateGameController(gameControllerData);
    }

    public void AnimatePlayerPanel(int ID)
    {

        RedPlayerStatus.fillAmount = 0;
        BluePlayerStatus.fillAmount = 0;
        HumanPlayerStatus.fillAmount = 0;

        switch (ID)
        {
            case 0:

                animateRedPlayer = true;
                animateBluePlayer = false;
                animateHumanPlayer = false;
                Debug.Log("<color=red> RED THINKING </color>");
                break;

            case 1:

                animateRedPlayer = false;
                animateBluePlayer = true;
                animateHumanPlayer = false;
                Debug.Log("<color=blue> BLUE THINKING </color>");
                break;

            case 2:

                animateRedPlayer = false;
                animateBluePlayer = false;
                animateHumanPlayer = true;
                Debug.Log("<color=green> HUMAN THINKING </color>");
                break;
        }

    }

    public bool DiceLabelVisiblity
    {
        set
        {
            DiceTextPanel.SetActive(value);
        }
    }

    public void UpdateDiceValue(int val)
    {   
        DiceTotalTextField.text = val.ToString();
    }

    // Update is called once per frame
    void Update()
    {   

        if(GameController.GameStatus == GameController.Status.IsActive)
        {
            float t = 0.005f;
            if (animateRedPlayer)
                RedPlayerStatus.fillAmount += Mathf.Clamp(t, 0, 1);

            if (animateBluePlayer)
                BluePlayerStatus.fillAmount += Mathf.Clamp(t, 0, 1);

            if (animateHumanPlayer)
                HumanPlayerStatus.fillAmount += Mathf.Clamp(t, 0, 1);
                          
        }
        
    }

    // Out of Scope for Lack of time,
    // This Method goes in Update Loop, it moves the AI player panels in and out according to turns.
    // Doesn't not work well with timed update events and was a prototype hence disabled.
    private void AnimatePlayerPanels_OUTOFSCOPE()
    {
        if (GameController.ActivePlayerID == 0)
        {
            if (animateRedPlayer)
            {
                var posX = RedPlayerPanel.transform.position.x;
                RedPlayerPanel.transform.position = new Vector3(Mathf.Lerp(posX, -66, 0.05f), RedPlayerPanel.transform.position.y, RedPlayerPanel.transform.position.z);
                //if(Vector3.Distance())
                //PlayerRed.transform.Translate(Vector3.left * Time.deltaTime);
            }
            else
            {
                var posX = RedPlayerPanel.transform.position.x;
                RedPlayerPanel.transform.position = new Vector3(Mathf.Lerp(posX, 66, 0.05f), RedPlayerPanel.transform.position.y, RedPlayerPanel.transform.position.z);
            }
        }


        if (GameController.ActivePlayerID == 1)
        {

            if (animateBluePlayer)
            {
                var posX = BluePlayerPanel.transform.position.x;
                BluePlayerPanel.transform.position = new Vector3(Mathf.Lerp(posX, -66, 0.05f), RedPlayerPanel.transform.position.y, RedPlayerPanel.transform.position.z);
            }
            else
            {

                var posRX = RedPlayerPanel.transform.position.x;
                RedPlayerPanel.transform.position = new Vector3(Mathf.Lerp(posRX, -66, 0.05f), RedPlayerPanel.transform.position.y, RedPlayerPanel.transform.position.z);
            }
        }


        if (GameController.ActivePlayerID == 2)
        {
            if (!animateBluePlayer && !animateRedPlayer)
            {
                var posBX = BluePlayerPanel.transform.position.x;
                BluePlayerPanel.transform.position = new Vector3(Mathf.Lerp(posBX, 485, 0.05f), RedPlayerPanel.transform.position.y, RedPlayerPanel.transform.position.z);
            }
        }
    }
}
