namespace EC.Core
{
    public enum QualityTier { Low, Medium, High }

    public static class QualityPolicy
    {
        public const int MediumMemoryMb = 3500;
        public const int HighMemoryMb = 5500;
        public const int MediumGraphicsLevel = 2;
        public const int HighGraphicsLevel = 3;

        public static QualityTier Choose(int systemMemoryMb, int graphicsMemoryMb, int processorCount)
        {
            if (systemMemoryMb >= HighMemoryMb && processorCount >= 8)
                return QualityTier.High;
            if (systemMemoryMb >= MediumMemoryMb && processorCount >= 6)
                return QualityTier.Medium;
            return QualityTier.Low;
        }

        public static QualityTier Resolve(int qualityOverride, QualityTier automatic)
        {
            return qualityOverride >= 1 && qualityOverride <= 3 ? (QualityTier)(qualityOverride - 1) : automatic;
        }

        public static float RenderScale(QualityTier tier)
        {
            switch (tier)
            {
                case QualityTier.High: return 1f;
                case QualityTier.Medium: return 0.85f;
                default: return 0.7f;
            }
        }

        public static int Msaa(QualityTier tier)
        {
            switch (tier)
            {
                case QualityTier.High: return 4;
                case QualityTier.Medium: return 2;
                default: return 1;
            }
        }

        public static bool BloomEnabled(QualityTier tier) { return tier == QualityTier.High; }
        public static bool VignetteEnabled(QualityTier tier) { return tier != QualityTier.Low; }
    }
}
