using System;
using UnityEngine;

public interface IProcessor<in TIn, out TOut>
{
    TOut Process(TIn point);
}

public abstract class FluentChain<TIn, TOut, TDerived> where TDerived : FluentChain<TIn, TOut, TDerived>
{
    public IProcessor<TIn, TOut> Processor;

    protected FluentChain(IProcessor<TIn, TOut> processor) =>
        Processor = processor ?? throw new ArgumentNullException(nameof(processor));

    protected TNextSelf Then<TNext, TNextSelf, TProcessor>(TProcessor processor,
        ChainFactory<TIn, TNext, TNextSelf> factory) where TNextSelf : FluentChain<TIn, TNext, TNextSelf>
        where TProcessor : class, IProcessor<TOut, TNext>
    {
        if (processor == null) throw new ArgumentNullException(nameof(processor));
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        var combined = new Combined<TIn, TOut, TNext>(Processor, processor);
        return factory(combined);
    }

    public TOut Run(TIn input)
    {
        if (Processor == null)
            throw new InvalidOperationException("Processor is not set. Use Chain.Start to initialize the chain.");
        return Processor.Process(input);
    }

    public ProcessorDelegate<TIn, TOut> Compile()
    {
        if (Processor == null)
            throw new InvalidOperationException("Processor is not set. Use Chain.Start to initialize the chain.");
        return input => Processor.Process(input); // Turns into a reusable function
    }
}

public delegate TChain ChainFactory<out TIn, in TOut, out TChain>(IProcessor<TIn, TOut> processor)
    where TChain : FluentChain<TIn, TOut, TChain>;


public delegate TOut ProcessorDelegate<in TIn, out TOut>(TIn input);

public class ThresholdFilter : IProcessor<float, bool>
{
    private readonly Func<float> getThreshold;

    public ThresholdFilter(Func<float> getThreshold) => this.getThreshold = getThreshold;

    public bool Process(float score) => score >= getThreshold();
}

public class ScoredChain : FluentChain<Vector3, float, ScoredChain>
{
    public ScoredChain(IProcessor<Vector3, float> processor) : base(processor)
    {
    }

    private static FilteredChain CreateFilteredChain(IProcessor<Vector3, bool> processor) => new(processor);

    public ScoredChain WithMaxDistance(float maxDistance)
    {
        Processor = new Combined<Vector3, float, float>(Processor, new ClampByMaxDistance(maxDistance));
        return new ScoredChain(Processor);
    }

    public FilteredChain Then<TProcessor>(TProcessor filter) where TProcessor : class, IProcessor<float, bool> =>
        Then<bool, FilteredChain, TProcessor>(filter, CreateFilteredChain);
}

public class FilteredChain : FluentChain<Vector3, bool, FilteredChain>
{
    public FilteredChain(IProcessor<Vector3, bool> processor) : base(processor)
    {
    }

    public FilteredChain LogToConsole(string system)
    {
        Debug.Log($"#{system}# Filtered Chain!");
        return this;
    }
}


public class ClampByMaxDistance : IProcessor<float, float>
{
    private readonly float _maxDistanceScoreThreshold;

    public ClampByMaxDistance(float maxDistance) => _maxDistanceScoreThreshold = 1f / (1f + maxDistance);

    public float Process(float score) => score < _maxDistanceScoreThreshold ? 0f : score;
}

public class DistanceChain : FluentChain<Vector3, float, DistanceChain>
{
    public DistanceChain(IProcessor<Vector3, float> processor) : base(processor)
    {
    }

    private static ScoredChain CreateScoredChain(IProcessor<Vector3, float> processor) => new(processor);

    public ScoredChain Then<TProcessor>(TProcessor scorer) where TProcessor : class, IProcessor<float, float> =>
        Then<float, ScoredChain, TProcessor>(scorer, CreateScoredChain);
}

public static class Chain
{
    public static DistanceChain FromTransform(Transform transform) => new(new DistanceFromTransform(transform));

    public static DistanceChain Start<TProcessor>(TProcessor processor) where TProcessor : IProcessor<Vector3, float> =>
        new(processor);
}


internal class Combined<TA, TB, TC> : IProcessor<TA, TC>
{
    private readonly IProcessor<TA, TB> _first;
    private readonly IProcessor<TB, TC> _second;

    public Combined(IProcessor<TA, TB> first, IProcessor<TB, TC> second)
    {
        _first = first;
        _second = second;
    }

    public TC Process(TA input) => _second.Process(_first.Process(input));
}


public class DistanceScorer : IProcessor<float, float>
{
    public float Process(float distance) => 1f / (1f + distance);
}

public class DistanceFromTransform : IProcessor<Vector3, float>
{
    private readonly Transform _transform;

    public DistanceFromTransform(Transform transform) => _transform = transform;

    public float Process(Vector3 point) => Vector3.Distance(_transform.position, point);
}