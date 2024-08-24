using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Mathematics.Intelligence;

public abstract class AbstractIA
{
    private readonly Semaphore _inputSem = new Semaphore(0, 100);
    private readonly Semaphore _outputSem = new Semaphore(0, 100);
    private readonly Queue<string> _inputBuffer = new Queue<string>();
    private readonly Queue<string> _outputBuffer = new Queue<string>();
    public Thread Thread { get; private set; }

    public string Name { get; }
    public AbstractIA(string name)
    {
        Name = name;
    }

    public string Read()
    {
        if (!_inputSem.WaitOne())
            throw new Exception();
        lock (_inputBuffer)
        {
            // Console.WriteLine($"{_name} IN Read {_inputBuffer.Peek()}");
            return _inputBuffer.Dequeue();
        }
    }

    public void Write(string line)
    {
        // Console.WriteLine($"{_name} OUT Write {line}");
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
            // Console.WriteLine($"{_name} IN Write {line}");
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
                // Console.WriteLine($"{_name} OUT Read {_outputBuffer.Peek()}");
                list.Add(_outputBuffer.Dequeue());
            }
        }
        watch.Stop();
        // if (watch.ElapsedMilliseconds > timeMs)
        //  throw new Exception("Player didn't respond in the allocated time");
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
