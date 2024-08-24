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
using System.Diagnostics;

namespace AxToolkit.Mathematics.Intelligence;

public abstract class AbstractIA
{
    private readonly Semaphore _inputSem = new Semaphore(0, 100);
    private readonly Semaphore _outputSem = new Semaphore(0, 100);
    private readonly Queue<string> _inputBuffer = new Queue<string>();
    private readonly Queue<string> _outputBuffer = new Queue<string>();
    public Thread Thread { get; private set; }

    public string Name { get; }
    public bool UseTimeout { get; private set; }

    protected AbstractIA(string name)
    {
        Name = name;
    }

    public string Read()
    {
        if (!_inputSem.WaitOne())
            throw new Exception();
        lock (_inputBuffer)
        {
            return _inputBuffer.Dequeue();
        }
    }

    public void Write(string line)
    {
        lock (_outputBuffer)
            _outputBuffer.Enqueue(line);
        _outputSem.Release();
    }

    public void Push(string line)
    {
        lock (_inputBuffer)
            _inputBuffer.Enqueue(line);
        _inputSem.Release();
    }

    public string[] Exchange(int timeMs, int expected, params string[] output)
    {
        foreach (var line in output)
        {
            lock (_inputBuffer)
                _inputBuffer.Enqueue(line);
            _inputSem.Release();
        }

        Stopwatch watch = Stopwatch.StartNew();
        var list = new List<string>();
        while (expected-- > 0)
        {
            if (!_outputSem.WaitOne())
                throw new Exception();
            lock (_outputBuffer)
            {
                list.Add(_outputBuffer.Dequeue());
            }
        }
        watch.Stop();
        if (UseTimeout && watch.ElapsedMilliseconds > timeMs)
            throw new Exception("Player didn't respond in the allocated time");
        return list.ToArray();
    }

    public void Start()
    {
        Thread = new Thread(() =>
        {
            Thread.Name = $"{GetType().Name}_{Name}";
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{GetType().Name}_{Name} : {ex.Message}");
            }
        });
        Thread.Start();
    }
    public void Kill()
    {
        Thread.Interrupt();
    }

    public abstract void Run();
}
