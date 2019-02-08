namespace Betlln
{
    public interface IConsoleScreen
    {
        void UpdateProgress(string message);
        void UpdateProgress(string format, string variableWidthValue, params object[] otherValues);
        void UpdateProgressComplete(string message = null);
        void ConfirmExit();
    }
}