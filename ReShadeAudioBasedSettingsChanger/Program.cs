using CSCore.SoundIn;
using CSCore.Streams;
using CSCore;
using System.Numerics;
using ShaderSettingsChangerTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;
using SharpDX.XInput;
using System.Runtime.CompilerServices;

class Program
{
    private static IConfiguration appConfig;
    private static AudioConfig audioConfig;
    private static string[] presetLines = null!;

    private static float[] sampleBuffer = [];
    private static int sampleIndex;

    private static LimitedBuffer<float> lastExtraOrdanarySampleBuffer;

    private static float lastWrittenUniformValue = -1.0f;
    private static float lastAproxMaxFreq = -1.0f;
    private static int lastAproxMaxFreqEval = -1;

    private static WasapiLoopbackCapture currentAudioCaputre = null!;
    private static Task audioDetectionTask = null!;
    private static CancellationTokenSource lastCancelationSource = null!;
    private static bool isInitalFrame = false;

    private static Controller controller;
    private static bool wasControllerDisconnected = false;

    static void Main()
    {
        controller = new Controller(UserIndex.One);
        appConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        ChangeToken.OnChange(() => appConfig.GetReloadToken(), () => ReloadConfigValues());
        ReloadConfigValues();
        Console.WriteLine("Listening to audio. Press Enter to stop...");
        Console.ReadLine();
        StopFrequencyDetection();
    }

    private static void ReloadConfigValues(bool reload = false)
    {
        try
        {
            var loadedConfig = appConfig.GetSection("appConfig").Get<AudioConfig>()!;
            if (loadedConfig.Equals(audioConfig) && !reload)
                return;

            audioConfig = loadedConfig;
            StopFrequencyDetection();
            audioConfig = appConfig.GetSection("appConfig").Get<AudioConfig>()!;
            InitPresetFile();

            sampleBuffer = new float[audioConfig.FftSize];
            sampleIndex = 0;
            isInitalFrame = true;

            lastExtraOrdanarySampleBuffer = new LimitedBuffer<float>(audioConfig.LastExtraOrdanarySampleBufferSize);
            StartFrequencyDetection();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to start frequency detection: " + ex.ToString());
            Console.WriteLine("\nPress enter to try again...");
            Console.ReadLine();
            ReloadConfigValues(true);
        }
    }

    private static void InitPresetFile()
    {
        if (audioConfig.ShaderConfig?.Enabled ?? false)
        {
            presetLines = File.ReadAllLines(audioConfig.ShaderConfig.PresetFilePath);

            string? currentSection = null;
            string sectionRegex = @"\[(.*?)\]";
            int index = 0;

            foreach (var line in presetLines)
            {
                Match match = Regex.Match(line, sectionRegex);

                if (match.Success && match.Groups.Count == 2)
                {
                    currentSection = match.Groups[1].Value;
                } else if (!string.IsNullOrEmpty(currentSection))
                {
                    var config = audioConfig.ShaderConfig?.UniformConfigs?.FirstOrDefault(u =>
                        u.Section.Equals(currentSection, StringComparison.OrdinalIgnoreCase) &&
                        line.StartsWith(u.Uniform, StringComparison.OrdinalIgnoreCase));

                    if (config != null)
                    {
                        config.LineIndex = index;
                    }
                }

                index++;
            }
        }
    }

    static void StartFrequencyDetection()
    {
        lastCancelationSource = new CancellationTokenSource();
        audioDetectionTask = new Task(() =>
        { 
            try
            {
                currentAudioCaputre = new WasapiLoopbackCapture(audioConfig.LoopBackDeviceLatency, null, ThreadPriority.Highest);
                var dev = currentAudioCaputre.Device;
                currentAudioCaputre.Initialize();

                var soundInSource = new SoundInSource(currentAudioCaputre)
                {
                    FillWithZeros = false
                };

                var sampleSource = soundInSource.ToSampleSource();
                currentAudioCaputre.DataAvailable += (s, e) =>
                {
                    try
                    {
                        int bytesPerSample = e.Format.BytesPerSample;
                        int channelCount = e.Format.Channels;
                        int sampleCount = e.ByteCount / bytesPerSample;
                        int sampleCountPerChannel = sampleCount / channelCount;

                        List<int> activeChannelIdexes = new List<int>();
                        float[][] samplesPerChannel = new float[channelCount][];
                        for (int c = 0; c < channelCount; c++)
                        {
                            samplesPerChannel[c] = new float[sampleCountPerChannel];
                            bool isActiveChannel = false;
                            for (int i = 0; i < sampleCountPerChannel; i++)
                            {
                                int sampleIndex = (i * channelCount + c) * bytesPerSample;
                                float val = BitConverter.ToSingle(e.Data, sampleIndex);
                                if (val != 0.0f)
                                {
                                    isActiveChannel = true;
                                    samplesPerChannel[c][i] = val;
                                }
                            }

                            if (isActiveChannel)
                                activeChannelIdexes.Add(c);
                        }

                        for (int i = 0; i < sampleCountPerChannel; i++)
                        {
                            float sum = 0.0f;
                            activeChannelIdexes.ForEach(ac =>
                            {
                                sum += samplesPerChannel[ac][i];
                            });

                            sampleBuffer[sampleIndex++] = sum / activeChannelIdexes.Count;

                            if (sampleIndex >= audioConfig.FftSize)
                            {
                                ProcessFrame(sampleBuffer, e.Format.SampleRate);
                                sampleIndex = 0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.ToString());
                    }
                };

                currentAudioCaputre.Start();
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }, lastCancelationSource.Token);

        audioDetectionTask.Start();
    }

    private static void StopFrequencyDetection()
    {

        if (currentAudioCaputre != null && currentAudioCaputre.RecordingState != RecordingState.Stopped)
        {
            currentAudioCaputre.Stop();
            currentAudioCaputre.Dispose();
            currentAudioCaputre = null;
        }

        if (audioDetectionTask != null && !audioDetectionTask.IsCompleted && lastCancelationSource != null)
        {
            lastCancelationSource.Cancel();
            lastCancelationSource = null;
        }
    }

    static void ProcessFrame(float[] samples, int sampleRate)
    {
        int bufferSize = audioConfig.FftSize;
        var fftBuffer = new Complex[bufferSize];
        var freqBuffer = new float[bufferSize / 2];

        for (int i = 0; i < bufferSize && i < samples.Length; i++)
        {
            fftBuffer[i] = new Complex(samples[i], 0);
        }

        FFT(fftBuffer);

        int frequencyOfBin = sampleRate / bufferSize;
        int minFreqPin = (int)Math.Round(audioConfig.MinFreq / (double)frequencyOfBin);
        int maxFreqPin = (int)Math.Round(audioConfig.MaxFreq / (double)frequencyOfBin) + 1;

        for (int i = minFreqPin; i < maxFreqPin; i++)
        {
            freqBuffer[i] = (float)fftBuffer[i].Magnitude;
        }

        if (isInitalFrame)
        {
            Console.WriteLine($"Actual frequency range: {minFreqPin * frequencyOfBin}hz - {(maxFreqPin - 1) * frequencyOfBin}hz");
            isInitalFrame = false;
        }

        float maxFreq = freqBuffer.Max();
        if (maxFreq > lastAproxMaxFreq)
        {
            lastAproxMaxFreq = (lastAproxMaxFreq + maxFreq * (audioConfig.MaxFreqAmplitudeIncreaseRatio - 1)) / audioConfig.MaxFreqAmplitudeIncreaseRatio;
            lastAproxMaxFreqEval = Environment.TickCount;
        } 
        else if (maxFreq > lastAproxMaxFreq - lastAproxMaxFreq * audioConfig.MaxFreqAmplitudeProlongerThreshholdPercent)
        {
            lastAproxMaxFreq = (lastAproxMaxFreq * (audioConfig.MaxFreqAmplitudeDecreaseRatio - 1) + maxFreq) / audioConfig.MaxFreqAmplitudeDecreaseRatio;
            lastAproxMaxFreqEval = Environment.TickCount;
            maxFreq = lastAproxMaxFreq;
        } 
        else if (Environment.TickCount - lastAproxMaxFreqEval > audioConfig.MaxFreqAmplitudeTTL)
        {
            lastAproxMaxFreq *= (1.0f - audioConfig.MaxFreqAmplitudeDecayRate);
        }

        if (maxFreq < lastAproxMaxFreq - lastAproxMaxFreq * audioConfig.PercentDiffFromMaxToBeExtraOrdanary || !lastExtraOrdanarySampleBuffer.Items.Any())
            lastExtraOrdanarySampleBuffer.Add(maxFreq);

        float avg = lastExtraOrdanarySampleBuffer.Items.Average();

        float adjustedFreqValue;
        if (maxFreq > audioConfig.MinFreqAmplitude)
        {
            adjustedFreqValue = maxFreq;
        }
        else
        {
            float scaleFactor = audioConfig.BelowMinFreqAmplitudeFunctionFactor;
            adjustedFreqValue = audioConfig.MinFreqAmplitude - 
                (1.0f / scaleFactor) +
                (1.0f / scaleFactor) *
                (float)Math.Exp(scaleFactor * (maxFreq - audioConfig.MinFreqAmplitude));

            adjustedFreqValue = Math.Max(0.0f, adjustedFreqValue);
        }

        if (audioConfig.PrintDebugInfos)
            Console.WriteLine(maxFreq);

        float uniformValue = Math.Max(0, (adjustedFreqValue - avg) / lastAproxMaxFreq);

        if (lastWrittenUniformValue != uniformValue)
        {
            SetControllerVibration(controller, uniformValue);
            WriteValueToUniforms(uniformValue);
        }

        lastWrittenUniformValue = uniformValue;
    }

    private static void WriteValueToUniforms(float uniformValue)
    {
        if (audioConfig.ShaderConfig.Enabled)
        {
            if (presetLines == null)
            {
                InitPresetFile();
            }

            bool changed = false;

            foreach (var uniformConfig in audioConfig.ShaderConfig.UniformConfigs.Where(c => c.LineIndex != -1))
            {
                presetLines[uniformConfig.LineIndex] = ($"{uniformConfig.Uniform}={uniformValue * uniformConfig.Factor}").Replace(',', '.');
                changed = true;
            }

            if (changed)
                File.WriteAllLines(audioConfig.ShaderConfig.PresetFilePath, presetLines);
        }
    }

    static void FFT(Complex[] buffer)
    {
        MathNet.Numerics.IntegralTransforms.Fourier.Forward(buffer, MathNet.Numerics.IntegralTransforms.FourierOptions.Matlab);
    }

    static void SetControllerVibration(Controller controller, float motorStrength)
    {
        if (audioConfig?.ControllerConfig?.Enabled ?? false && controller.IsConnected)
        {
            motorStrength = Math.Clamp(motorStrength * audioConfig.ControllerConfig.RumbleFactor, 0.0f, 1.0f);
            var vibration = new Vibration
            {
                LeftMotorSpeed = (ushort)(motorStrength * 65535),
                RightMotorSpeed = (ushort)(motorStrength * 65535)
            };

            try
            {
                controller.SetVibration(vibration);
                wasControllerDisconnected = false;
            } 
            catch (Exception ex)
            {
                if (!wasControllerDisconnected)
                {
                    wasControllerDisconnected = true;
                    Console.WriteLine("Controller disconnected!");
                } 
            }            
        }     
    }
}