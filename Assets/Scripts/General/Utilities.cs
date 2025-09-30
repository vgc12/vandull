using System;

namespace General
{
    public class Utilities
    {
        public static void CopyValues<T>(T @base, T copy)
        {
            var type = @base.GetType();
            foreach (var field in type.GetFields()) field.SetValue(copy, field.GetValue(@base));
        }
    }

    public abstract class Timer
    {
        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        protected Timer(float value)
        {
            InitialTime = value;
            IsRunning = false;
        }

        public float InitialTime { get; set; }
        protected float Time { get; set; }
        public bool IsRunning { get; protected set; }

        public float Progress => Time / InitialTime;

        public void Start()
        {
            Time = InitialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OnTimerStart.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerStop.Invoke();
            }
        }

        public void Resume()
        {
            IsRunning = true;
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public abstract void Tick(float deltaTime);
    }

    public class CountdownTimer : Timer
    {
        public CountdownTimer(float value) : base(value)
        {
        }

        public bool IsFinished => Time <= 0;

        public override void Tick(float deltaTime)
        {
            if (IsRunning && Time > 0) Time -= deltaTime;

            if (IsRunning && Time <= 0) Stop();
        }

        public void Reset()
        {
            Time = InitialTime;
        }

        public void Reset(float newTime)
        {
            InitialTime = newTime;
            Reset();
        }
    }

    public class StopwatchTimer : Timer
    {
        public StopwatchTimer() : base(0)
        {
        }

        public override void Tick(float deltaTime)
        {
            if (IsRunning) Time += deltaTime;
        }

        public void Reset()
        {
            Time = 0;
        }

        public float GetTime()
        {
            return Time;
        }
    }
}