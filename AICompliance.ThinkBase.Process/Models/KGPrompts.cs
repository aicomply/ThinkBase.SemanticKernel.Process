using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AICompliance.ThinkBase.Process.Models
{
    public class KGPrompts
    {
        public const string InitialKGText =
    """
                I'm calling a [ThinkBase Intelligent Knowledge Graph](https://thinkbase.ai) to answer that. It'll ask a series of questions to get the information it needs.
                You can stop it at any time by typing 'quit'.
                """;
    }
}
