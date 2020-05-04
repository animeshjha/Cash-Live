using System;
using UnityEngine;

// HELPER CLASS
public class GameTimer : MonoBehaviour
{
    #region Variables
    private DateTime endTime;
    private bool active;

    // We could just use 0 but this will enable 
    // the event to fire in the frame before the timer expires if the timer is very close to firing.
    public int TimeLeft;
    private float TIMER_START;
    private float TIMER_END;
    const float TIME_DELTA = 0.01f;

    public delegate void Update_GameController(int[] data);
    public static event Update_GameController TimedEvent;
    public int callerID;
    public bool TimeOut;
    #endregion
    private void Awake()
    {
        active = false;
        callerID = 0;
        TimeLeft = 0;
    }

    public void StartTimer(float timeInSeconds)
    {
        //Debug.Log("Timer Called by: " + callerID);
        endTime = DateTime.Now + TimeSpan.FromSeconds(timeInSeconds);
        active = true;
    }

    void CheckTimer()
    {
        TimeLeft = (int)(endTime - DateTime.Now).TotalSeconds;

        if (TimeLeft < TIME_DELTA)
        {
            active = false;
            TimedEvent(new int[] {callerID});
        }
    }

    void Update()
    {
        if (active)
            CheckTimer();
    }
}
