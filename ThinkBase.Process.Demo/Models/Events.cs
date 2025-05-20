namespace ThinkBase.Process.Demo.Models
{
    public class Events
    {
        public static readonly string Exit = nameof(Exit);
        public static readonly string InteractionRequest = nameof(InteractionRequest);
        public static readonly string WaitForUserInput = nameof(WaitForUserInput);
        public static readonly string UserInputReceived = nameof(UserInputReceived);
        public static readonly string StartProcess = nameof(StartProcess);
        public static readonly string DescribeKGraphRequest = nameof(DescribeKGraphRequest);

    }
}
