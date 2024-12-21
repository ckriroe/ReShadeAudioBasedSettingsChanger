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

        public override bool Equals(object? obj)
        {
            return obj is UniformConfig config &&
                   Section == config.Section &&
                   Uniform == config.Uniform &&
                   Factor == config.Factor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Section, Uniform, Factor);
        }
    }
}
