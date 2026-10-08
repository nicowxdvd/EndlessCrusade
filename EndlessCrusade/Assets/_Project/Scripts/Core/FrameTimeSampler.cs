using System;

namespace EC.Core
{
    public class FrameTimeSampler
    {
        readonly float[] samples;
        int count;
        int next;

        public FrameTimeSampler(int capacity)
        {
            samples = new float[capacity];
        }

        public int Count => count;

        public void Add(float milliseconds)
        {
            samples[next] = milliseconds;
            next = (next + 1) % samples.Length;
            if (count < samples.Length)
                count++;
        }

        public float Percentile(float fraction)
        {
            if (count == 0)
                return 0f;
            var sorted = new float[count];
            Array.Copy(samples, sorted, count);
            Array.Sort(sorted);
            var index = (int)Math.Ceiling(fraction * count) - 1;
            return sorted[Math.Max(0, Math.Min(count - 1, index))];
        }

        public float Average()
        {
            if (count == 0)
                return 0f;
            var sum = 0f;
            for (int i = 0; i < count; i++)
                sum += samples[i];
            return sum / count;
        }
    }
}
