using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AICompliance.ThinkBase.Process.Models
{
    public class InteractResponse
    {
        public DarlVar response { get; set; }

        public string darl { get; set; }

        public List<object> matches { get; set; }
    }
}
