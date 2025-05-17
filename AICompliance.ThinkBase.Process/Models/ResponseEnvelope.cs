using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AICompliance.ThinkBase.Process.Models
{
    internal class ResponseEnvelope
    {
        public List<InteractResponse> interactKnowledgeGraph { get; set; } = [];
    }
}
