using UnityEngine;

public class Dice : MonoBehaviour
{

    public int DiceID = 0;
    public bool Landed { get; private set; }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Floor")
            Landed = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Floor")
            Landed = false;
    }
}
