using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderSettingsChangerTest
{
    public class UniformConfig
    {
        public required string Section { get; set; }
        public required string Uniform { get; set; }
        public required float Factor { get; set; }
        public int LineIndex { get; set; } = -1;
    }
}
