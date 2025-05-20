namespace ThinkBase.Process.Demo.Models
{
    public class Prompts
    {
        public const string IntentInstructions =
"""
        Capture information provided by the user for their request.
                Never provide a direct answer to the user's request.
                Look at descriptions and names for each item provided below:

                {{ KGMetadataPlugin.GetKGInfo }}

                Decide if one of the items could answer the user's query.
                If so, set KnowledgeGraph to 'true' and set the name and initial text to the name and initial text of the item.
                Otherwise choose the Interaction value to set to 'true'.
        """;
        public const string SimpleInteractionInstructions =
                        """
                You are an agent responding to simple requests using the user information, the chat history and your general knowledge. Use the user's given name where possible.
                If you require more information set Complete to false and ask the user for more information.
                Otherwise, set Complete to true and provide a response.
                """;

        public const string KGraphInfoInstructions =
            """
                You are an agent responding to requests for information about the Knowledge Graphs available.
                Summarize the following items using the name and description and format them to describe what intelligent knowledge graphs are available.

                 {{ KGMetadataPlugin.GetKGInfo }}

                 Don't mention the Initial texts as they are for internal use only.
                """;
    }
}
