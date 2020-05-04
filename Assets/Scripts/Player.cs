

public class Player
{
    public bool IsHuman { get; set; }
    public enum Level
    {
        Conservative,
        Aggressive,
        Neutral                 // Human Player
    }
    public Level PlayerLevel { get; set; }
    public bool IsShooter = false;
    public int Money { get; set; }
    public string PlayerName;
    public int BetAmount;
    public Player()
    {
        IsHuman = false;
        PlayerLevel = Level.Neutral;
        IsShooter = false;
        Money = 0;
        PlayerName = "";
        BetAmount = 0;
    }

}
