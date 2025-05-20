// Copyright (c) 2025 AI Compliance inc. Licensed under the MIT License.using System;

namespace AICompliance.ThinkBase.Process.Models
{
    public class InteractResponse
    {
        public DarlVar response { get; set; }

        public string darl { get; set; }

        public List<object> matches { get; set; }
    }
}
