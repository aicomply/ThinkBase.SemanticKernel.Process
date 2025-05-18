namespace AICompliance.ThinkBase.Process.Models
{
    public class KGraphDescription
    {
        public string name { get; set; } = string.Empty;
        public KGraphMetaData model { get; set; } = new();
    }

    public class KGraphMetaData
    {
        public string? description { get; set; }
        public string? initialText { get; set; }
        //        public DateDisplay? dateDisplay { get; set; } = DateDisplay.recent;
        //        public InferenceTime? inferenceTime { get; set; } = InferenceTime.now;
        //       public DarlTime? fixedTime { get; set; }
        public string? author { get; set; }
        public string? copyright { get; set; }
        public string? licenseUrl { get; set; }
        public string? defaultTarget { get; set; }
        public string? defaultDocumentTarget { get; set; }

    }
}