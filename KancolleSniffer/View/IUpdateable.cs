namespace KancolleSniffer.View
{
    public interface IUpdateContext
    {
        UpdateContext Context { set; }
    }

    public interface IUpdateTimers : IUpdateContext
    {
        void UpdateTimers();
    }
}