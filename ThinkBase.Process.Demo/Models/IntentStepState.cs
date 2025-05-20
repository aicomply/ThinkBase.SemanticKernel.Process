namespace ThinkBase.Process.Demo.Models
{
    public class IntentStepState
    {
        public bool InSubProcess { get; set; } = false;
        public string? NextEvent { get; set; } = null;
        public string? GraphName { get; set; } = null;
        public string? InitialText { get; set; } = null;

    }
}
