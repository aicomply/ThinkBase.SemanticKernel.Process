namespace ThinkBase.Process.Demo.Models
{
    public class Prompts
    {
        public const string IntentInstructions =
"""
        Capture information provided by the user for their request.
                Never provide a direct answer to the user's request.
                Decide if the user's intent fits one of the IntentResponse values, and set only that one to true. 
                For instance, anything about emails, Email should be set to true. Similarly for Calendar, anything about calendars, events or appointments should set Calendar to true, and far TaskList, any requests abot tasks or jobs should set TaskList to true.
                If unsure choose the Information value to set to true.
        """;
        public static string SimpleInteractionInstructions =
                        """
                You are an agent responding to simple requests using the user information, the chat history and your general knowledge. Use the user's given name where possible.
                If you require more information set Complete to false and ask the user for more information.
                Otherwise, set Complete to true and provide a response.
                """;
    }
}
