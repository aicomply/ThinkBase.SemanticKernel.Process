// Copyright (c) 2025 AI Compliance inc. Licensed under the MIT License.using System.Collections.Generic;
using System.ComponentModel;

namespace AICompliance.ThinkBase.Process.Models
{
    public class KGMetadata
    {
        [Description("The name of the knowledge graph")]
        public string? Name { get; set; }
        [Description("Description of what the knowledge graph does")]
        public string? Description { get; set; }
        [Description("Initial text to send to the knowledge graph to start the inference process")]
        public string? InitialText { get; set; }
    }
}
