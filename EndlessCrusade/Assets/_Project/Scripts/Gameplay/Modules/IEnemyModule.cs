namespace EC.Gameplay
{
    public interface IEnemyModule
    {
        void Initialize(EnemyBrain brain);
        void Tick(float deltaTime);
    }
}
