using System;
using UnityEngine;
using UnityEngine.UI;

public class DiceController : MonoBehaviour
{
    #region Variable Declaration
    private static GameObject DiceOne;
    private static GameObject DiceTwo;

    public UIController UI_Controller;
    public GameObject DebugPanel;
    //public GameController currentGameController;

    public Image DiceImageColor;
    [SerializeField] private Color DiceDebugMode_OFF;
    [SerializeField] private Color DiceDebugMode_ON;

    public float thrust = 2f;
    bool simulate = false;

    [SerializeField] float simulationTime = 1.5f;

    public int[] GetDiceValue { get; private set; }
    private int diceTotal;
    public bool PlayerPlayedDice = false;
    public int NumberOfDice;

    public static bool DiceLandedStatic = false;

    public bool DiceLanded = false;
    public static event Action OnDiceLanded = delegate { };
    public delegate void Update_GameController(int[] data);
    public static event Update_GameController UpdateGameController;
    #endregion
    private void Awake()
    {
        // Tag's are less resouce intensive
        DiceOne = GameObject.FindGameObjectWithTag("Dice1");
        DiceTwo = GameObject.FindGameObjectWithTag("Dice2");
    }

    void Start()
    {
        NumberOfDice = 2;
        GetDiceValue = new int[NumberOfDice];
    }

    public int GetDiceTotal()
    {
        if (GameController.GameStatus == GameController.Status.IsActive)
            if (DiceOne.GetComponent<Dice>().Landed && DiceOne.GetComponent<Dice>().Landed)
                return diceTotal;

        return 0;
    }

    public void RollDice()
    {
        simulate = true;
        simulationTime = 1.5f;
        DiceLanded = false;
        DiceLandedStatic = false;
        UI_Controller.DiceLabelVisiblity = true;
        PlayerPlayedDice = true;

    }

    // Two axis are parallel if DOT PRODUCT is more than 1
    void GetDiceCount()
    {
        int diceOneValue = 0;
        int diceTwoValue = 0;

        if (Mathf.Round(Vector3.Dot(DiceOne.transform.forward, Vector3.up)) >= 1)   // Z (Blue) Axis
            diceOneValue = 1;
        if (Mathf.Round(Vector3.Dot(-DiceOne.transform.forward, Vector3.up)) >= 1)  // Z (Blue) Axis
            diceOneValue = 2;
        if (Mathf.Round(Vector3.Dot(DiceOne.transform.up, Vector3.up)) >= 1)        
            diceOneValue = 6;
        if (Mathf.Round(Vector3.Dot(-DiceOne.transform.up, Vector3.up)) >= 1)
            diceOneValue = 4;
        if (Mathf.Round(Vector3.Dot(DiceOne.transform.right, Vector3.up)) >= 1)     // X (Red) Axis
            diceOneValue = 3;
        if (Mathf.Round(Vector3.Dot(-DiceOne.transform.right, Vector3.up)) >= 1)    // X (Red) Axis
            diceOneValue = 5;


        if (Mathf.Round(Vector3.Dot(DiceTwo.transform.forward, Vector3.up)) >= 1)   // Z (Blue) Axis
            diceTwoValue = 1;
        if (Mathf.Round(Vector3.Dot(-DiceTwo.transform.forward, Vector3.up)) >= 1)  // Z (Blue) Axis
            diceTwoValue = 2;
        if (Mathf.Round(Vector3.Dot(DiceTwo.transform.up, Vector3.up)) >= 1)
            diceTwoValue = 6;
        if (Mathf.Round(Vector3.Dot(-DiceTwo.transform.up, Vector3.up)) >= 1)
            diceTwoValue = 4;
        if (Mathf.Round(Vector3.Dot(DiceTwo.transform.right, Vector3.up)) >= 1)     // X (Red) Axis
            diceTwoValue = 3;
        if (Mathf.Round(Vector3.Dot(-DiceTwo.transform.right, Vector3.up)) >= 1)    // X (Red) Axis
            diceTwoValue = 5;

        GetDiceValue[0] = diceOneValue;
        GetDiceValue[1] = diceTwoValue;
        diceTotal = diceOneValue + diceTwoValue;
        UI_Controller.UpdateDiceValue((diceOneValue+diceTwoValue));


        // Debug Statements for Inspection of DOT Products        
        //Debug.Log("1: " + Mathf.Round(Vector3.Dot(DiceOne.transform.forward, Vector3.up)));
        //Debug.Log("2: " + Mathf.Round(Vector3.Dot(-DiceOne.transform.forward, Vector3.up)));
        //Debug.Log("3: " + Mathf.Round(Vector3.Dot(DiceOne.transform.right, Vector3.up)));
        //Debug.Log("4: " + Mathf.Round(Vector3.Dot(-DiceOne.transform.up, Vector3.up)));
        //Debug.Log("5: " + Mathf.Round(Vector3.Dot(-DiceOne.transform.right, Vector3.up)));
        //Debug.Log("6: " + Mathf.Round(Vector3.Dot(DiceOne.transform.up, Vector3.up)));
        //Debug.Log("Dice: [" + GetDiceValue[0] + " , " + GetDiceValue[1] + "]" + "Total: "+ (diceOneValue + diceTwoValue).ToString());

    }

    public void RollDiceSimulate()
    {

        DiceOne.GetComponent<Rigidbody>().AddTorque(new Vector3(UnityEngine.Random.Range(100, 1000), UnityEngine.Random.Range(500, 1000), UnityEngine.Random.Range(100, 1000)), ForceMode.Impulse);
        DiceOne.GetComponent<Rigidbody>().AddForce(transform.up * thrust);

        DiceTwo.GetComponent<Rigidbody>().AddTorque(new Vector3(UnityEngine.Random.Range(100, 1000), UnityEngine.Random.Range(500, 1000), UnityEngine.Random.Range(100, 1000)), ForceMode.Impulse);
        DiceTwo.GetComponent<Rigidbody>().AddForce(transform.up * thrust);

    }

    private void FixedUpdate()
    {
        if (GameController.GameStatus == GameController.Status.GameOver || GameController.GameStatus == GameController.Status.Paused)
            return;


            if (simulate)
            {
                RollDiceSimulate();
                simulationTime -= 0.1f;

                if (simulationTime < 0.1f || DiceLanded)
                {

                    simulationTime = 0;
                    simulate = false;
                }
            }
            else
            {       
                if(DiceOne != null)
                    if (DiceOne.GetComponent<Dice>().Landed && DiceTwo.GetComponent<Dice>().Landed)
                        DiceLanded = true;

                if (!DebugPanel.gameObject.activeSelf)
                    if (DiceLanded)
                        GetDiceCount();

                DiceLandedStatic = true;

                //if (DiceLanded)
                //    OnDiceLanded();
            }            
    }

    // Update is called once per frame
    void Update()
    {
        if(DebugPanel.gameObject.activeSelf)
        {
            DiceImageColor.color = DiceDebugMode_ON;
        }
        else
        {
            DiceImageColor.color = DiceDebugMode_OFF;
        }
    }
}
