namespace KancolleSniffer.View
{
    public interface IUpdateable
    {
        UpdateContext Context { set; }
        void UpdateTimers();
    }
}