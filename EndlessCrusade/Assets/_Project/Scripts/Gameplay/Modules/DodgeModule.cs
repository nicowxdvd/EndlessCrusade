using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EC.Gameplay
{
    public class DodgeModule : MonoBehaviour, IEnemyModule, IDamageModifier
    {
        [Range(0f, 1f)] public float dodgeChance = 0.25f;

        Func<float> randomSource = DefaultRandom;

        public Func<float> RandomSource
        {
            get => randomSource;
            set => randomSource = value ?? DefaultRandom;
        }

        static float DefaultRandom()
        {
            return Random.value;
        }

        public void Initialize(EnemyBrain brain)
        {
            brain.Health.AddModifier(this);
        }

        public void Tick(float deltaTime) { }

        public int Modify(int amount, GameObject source)
        {
            return randomSource() < dodgeChance ? 0 : amount;
        }
    }
}
