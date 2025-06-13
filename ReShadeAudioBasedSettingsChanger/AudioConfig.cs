using ReShadeAudioBasedSettingsChanger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderSettingsChangerTest
{
    public class AudioConfig
    {
        public required ShaderConfig ShaderConfig { get; set; }
        public required ControllerConfig ControllerConfig { get; set; }
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
        public bool PrintDebugInfos { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is AudioConfig config &&
                   ShaderConfig.Equals(config.ShaderConfig) &&
                   ControllerConfig.Equals(config.ControllerConfig) &&
                   LoopBackDeviceLatency == config.LoopBackDeviceLatency &&
                   FftSize == config.FftSize &&
                   LastExtraOrdanarySampleBufferSize == config.LastExtraOrdanarySampleBufferSize &&
                   MinFreq == config.MinFreq &&
                   MaxFreq == config.MaxFreq &&
                   MinFreqAmplitude == config.MinFreqAmplitude &&
                   BelowMinFreqAmplitudeFunctionFactor == config.BelowMinFreqAmplitudeFunctionFactor &&
                   MaxFreqAmplitudeIncreaseRatio == config.MaxFreqAmplitudeIncreaseRatio &&
                   MaxFreqAmplitudeDecreaseRatio == config.MaxFreqAmplitudeDecreaseRatio &&
                   MaxFreqAmplitudeTTL == config.MaxFreqAmplitudeTTL &&
                   MaxFreqAmplitudeProlongerThreshholdPercent == config.MaxFreqAmplitudeProlongerThreshholdPercent &&
                   MaxFreqAmplitudeDecayRate == config.MaxFreqAmplitudeDecayRate &&
                   PercentDiffFromMaxToBeExtraOrdanary == config.PercentDiffFromMaxToBeExtraOrdanary &&
                   PrintDebugInfos == config.PrintDebugInfos;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(ShaderConfig);
            hash.Add(ControllerConfig);
            hash.Add(LoopBackDeviceLatency);
            hash.Add(FftSize);
            hash.Add(LastExtraOrdanarySampleBufferSize);
            hash.Add(MinFreq);
            hash.Add(MaxFreq);
            hash.Add(MinFreqAmplitude);
            hash.Add(BelowMinFreqAmplitudeFunctionFactor);
            hash.Add(MaxFreqAmplitudeIncreaseRatio);
            hash.Add(MaxFreqAmplitudeDecreaseRatio);
            hash.Add(MaxFreqAmplitudeTTL);
            hash.Add(MaxFreqAmplitudeProlongerThreshholdPercent);
            hash.Add(MaxFreqAmplitudeDecayRate);
            hash.Add(PercentDiffFromMaxToBeExtraOrdanary);
            hash.Add(PrintDebugInfos);
            return hash.ToHashCode();
        }
    }
}
