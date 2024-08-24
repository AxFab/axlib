using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Mathematics.Intelligence;

public abstract class AbstractGame
{
    protected readonly Random _rand;
    protected AbstractGame() : this(0) { }
    protected AbstractGame(int seed)
    {
        _rand = new Random(seed);
    }

    public abstract int MinPlayers { get; }
    public abstract int MaxPlayers { get; }
    public int Round { get; set; }
    public bool GameOver { get; set; }
    public List<AbstractIA> Players { get; } = new List<AbstractIA>();

    public void Run()
    {
        if (Players.Count < MinPlayers || Players.Count > MaxPlayers)
            throw new Exception("Wrong count of players");
        GameOver = false;
        foreach (var player in Players)
            player.Start();
        try
        {
            BeforePlay();
            for (Round = 0; !GameOver; ++Round)
                PlayRound();
            AfterPlay();
        }
        finally
        {
            foreach (var player in Players)
                player.Kill();
        }
    }

    public abstract void BeforePlay();
    public abstract void PlayRound();
    public abstract void AfterPlay();

    public string[] Exchange(int player, int expected, int timeMs, params string[] output)
    {
        return Players[player].Exchange(timeMs, expected, output);
    }

    public abstract object BuildGlobal();
}
