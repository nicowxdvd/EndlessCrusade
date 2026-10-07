namespace EC.Data
{
    [System.Serializable]
    public class SpawnEntry
    {
        public EnemyDefinition enemy;
        public int count = 1;
        public float interval = 1f;
        public float startDelay;
    }
}
