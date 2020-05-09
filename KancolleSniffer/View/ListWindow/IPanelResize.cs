namespace KancolleSniffer.View.ListWindow
{
    public interface IPanelResize
    {
        bool Visible { get; }
        void ApplyResize();
    }
}