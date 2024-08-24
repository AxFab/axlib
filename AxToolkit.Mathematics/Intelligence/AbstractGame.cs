// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
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
