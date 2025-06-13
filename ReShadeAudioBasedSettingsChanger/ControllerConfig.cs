using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReShadeAudioBasedSettingsChanger
{
    public class ControllerConfig
    {
        public required bool Enabled { get; set; }
        public required float RumbleFactor { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ControllerConfig config &&
                   Enabled == config.Enabled &&
                   RumbleFactor == config.RumbleFactor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Enabled, RumbleFactor);
        }
    }
}
