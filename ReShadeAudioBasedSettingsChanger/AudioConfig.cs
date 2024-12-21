using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderSettingsChangerTest
{
    public class AudioConfig
    {
        public required string PresetFilePath { get; set; }
        public required List<UniformConfig> UniformConfigs { get; set; }
        public required int LoopBackDeviceLatency { get; set; }
        public required int FftSize { get; set; }
        public required int LastExtraOrdanarySampleBufferSize { get; set; }
        public required int MinFreq { get; set; }
        public required int MaxFreq { get; set; }
        public required int MinFreqAmplitude { get; set; }
        public required float BelowMinFreqAmplitudeFunctionFactor { get; set; }
        public required int MaxFreqAmplitudeIncreaseRatio { get; set; }
        public required int MaxFreqAmplitudeDecreaseRatio { get; set; }
        public required int MaxFreqAmplitudeTTL { get; set; }
        public required float MaxFreqAmplitudeProlongerThreshholdPercent { get; set; }
        public required float MaxFreqAmplitudeDecayRate { get; set; }
        public required float PercentDiffFromMaxToBeExtraOrdanary { get; set; }
    }
}
