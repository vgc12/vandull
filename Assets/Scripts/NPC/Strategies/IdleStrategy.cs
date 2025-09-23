using General;
using NPC;
using UnityEngine;

public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true;
    
    public bool Complete {get; private set;}

    private readonly CountdownTimer _timer;
    

    public IdleStrategy(float duration)
    {
        Complete = false;

        _timer = new CountdownTimer(duration);
        _timer.OnTimerStart += () => Complete = false;
        _timer.OnTimerStop += () => Complete = true;
       
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Update(float deltaTime)
    {
        _timer.Tick(deltaTime);
     
    }
}