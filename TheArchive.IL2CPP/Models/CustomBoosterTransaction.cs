using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Models
{
    public class CustomBoosterTransaction
    {
        public uint MaxBackendTemplateId { get; set; }
        public uint[] AcknowledgeIds { get; set; }
        public uint[] TouchIds { get; set; }
        public uint[] DropIds { get; set; }
        public Missed AcknowledgeMissed { get; set; }

        public class Missed
        {
            public int Basic { get; set; }
            public int Advanced { get; set; }
            public int Specialized { get; set; }
        }

    }
}
