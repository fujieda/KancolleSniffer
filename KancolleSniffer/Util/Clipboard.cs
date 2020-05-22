using System.Runtime.InteropServices;

namespace KancolleSniffer.Util
{
    public static class Clipboard
    {
        public static void SetText(string text)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetText(text);
            }
            catch (ExternalException)
            {
            }
        }
    }
}