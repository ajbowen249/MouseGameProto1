using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeededRandom
{
    private int _seed;
    private System.Random _random;

    public SeededRandom()
    {
        Seed = new System.Random().Next();
    }

    public SeededRandom(int seed)
    {
        Seed = seed;
    }

    public int Seed
    {
        get
        {
            return _seed;
        }
        set
        {
            _seed = value;
            _random = new System.Random(_seed);
        }
    }

    public System.Random Random { get { return _random; } }
}

public static class RandomInstances
{
    private static Dictionary<string, SeededRandom> _instances = new Dictionary<string, SeededRandom>();

    public static SeededRandom GetInstance(string name)
    {
        if (!_instances.ContainsKey(name))
        {
            _instances[name] = new SeededRandom();
        }

        return _instances[name];
    }

    public static void SetSeed(string name, int seed)
    {
        var instance = GetInstance(name);
        instance.Seed = seed;
    }

    public static class Names
    {
        public const string Generator = "generator";
    }
}

public static class RandomUtil
{
    public static T RandomElement<T>(IList<T> elements, string instanceName)
    {
        return RandomElement(elements, RandomInstances.GetInstance(instanceName).Random);
    }

    public static T RandomElement<T>(IList<T> elements, System.Random random)
    {
        var randomIndex = random.Next(elements.Count);
        return elements[randomIndex];
    }
}
