using ShaderSettingsChangerTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReShadeAudioBasedSettingsChanger
{
    public class ShaderConfig
    {
        public required bool Enabled { get; set; }
        public required string PresetFilePath { get; set; }
        public required List<UniformConfig> UniformConfigs { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ShaderConfig config &&
                   Enabled == config.Enabled &&
                   PresetFilePath == config.PresetFilePath &&
                   EqualityComparer<List<UniformConfig>>.Default.Equals(UniformConfigs, config.UniformConfigs);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Enabled, PresetFilePath, UniformConfigs);
        }
    }
}
