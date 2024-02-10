namespace Net.Entities
{
    public interface IEntityAwake
    {
        void Awake();
    }

    public interface IEntityStart
    {
        void Start();
    }

    public interface IEntityUpdate
    {
        void Update();
    }

    public interface IEntityDestroy
    {
        void Destroy();
    }
}
