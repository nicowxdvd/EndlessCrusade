namespace EC.Data
{
    public interface ISaveService
    {
        SaveData Current { get; }
        void Load();
        void Save();
        void Reset();
    }
}
