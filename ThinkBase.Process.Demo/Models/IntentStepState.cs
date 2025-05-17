namespace ThinkBase.Process.Demo.Models
{
    public class IntentStepState
    {
        public bool InSubProcess { get; set; } = false;
        public string? NextEvent { get; set; } = null;

    }
}
