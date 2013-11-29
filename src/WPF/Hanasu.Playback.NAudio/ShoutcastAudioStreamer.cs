using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Reflection;
using System.Text;
using System.Threading;
using NAudio.Wave;

namespace Hanasu.Playback.NAudio
{
    //modified version of: https://gist.github.com/markheath/3301840
    internal class ShoutcastAudioStreamer : IAudioStreamer
    {
        IMp3FrameDecompressor decompressor = null;
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private HttpWebRequest webRequest;
        private VolumeWaveProvider16 volumeProvider;

        private Action<string> songCallback = null;

        // Enable/disable useUnsafeHeaderParsing.
        // See http://o2platform.wordpress.com/2010/10/20/dealing-with-the-server-committed-a-protocol-violation-sectionresponsestatusline/
        public static bool ToggleAllowUnsafeHeaderParsing(bool enable)
        {
            //Get the assembly that contains the internal class
            Assembly assembly = Assembly.GetAssembly(typeof(SettingsSection));
            if (assembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (settingsSectionType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created already invoking the property will create it for us.
                    object anInstance = settingsSectionType.InvokeMember("Section",
                    BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework if unsafe header parsing is allowed
                        FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
                            return true;
                        }

                    }
                }
            }
            return false;
        }


        internal ShoutcastAudioStreamer(Action<string> SongCallback)
        {
            songCallback = SongCallback;

            ToggleAllowUnsafeHeaderParsing(true);
        }

        public void Stream(string url)
        {
            webRequest = (HttpWebRequest)WebRequest.Create(url + "/;");

            int metaInt = 0; // blocksize of mp3 data

            webRequest.Headers.Clear();
            webRequest.Method = "GET";
            // needed to receive metadata informations
            webRequest.Headers.Add("Icy-MetaData", "1");
            webRequest.UserAgent = "WinampMPEG/5.09";

            HttpWebResponse resp = null;
            try
            {
                resp = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.RequestCanceled)
                {
                    //ShowError(e.Message);
                }
                return;
            }
            byte[] buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

            try
            {
                // read blocksize to find metadata block
                metaInt = Convert.ToInt32(resp.GetResponseHeader("icy-metaint"));

            }
            catch
            {
            }

            decompressor = null;
            try
            {
                using (var responseStream = resp.GetResponseStream())
                {
                    var readFullyStream = new ReadFullyStream(responseStream);
                    readFullyStream.metaInt = metaInt;

                    System.Threading.Tasks.Task.Factory.StartNew(() => Thread.Sleep(10000)).ContinueWith(x =>
                        {
                            volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);

                            waveOut = new WaveOut();
                            waveOut.Init(volumeProvider);
                        });

                    do
                    {
                        if (bufferedWaveProvider != null && bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4)
                        {
                            Debug.WriteLine("Buffer getting full, taking a break");
                            Thread.Sleep(500);
                        }
                        else
                        {
                            Mp3Frame frame = null;
                            try
                            {

                                frame = Mp3Frame.LoadFromStream(readFullyStream, true);

                                if (metaInt > 0)
                                    songCallback(readFullyStream.SongName);
                                else
                                    songCallback("No Song Info in Stream...");


                            }
                            catch (EndOfStreamException)
                            {
                                this.fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                            catch (WebException)
                            {
                                // probably we have aborted download from the GUI thread
                                break;
                            }
                            if (decompressor == null)
                            {
                                // don't think these details matter too much - just help ACM select the right codec
                                // however, the buffered provider doesn't know what sample rate it is working at
                                // until we have a frame
                                WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate);
                                decompressor = new AcmMp3FrameDecompressor(waveFormat);
                                this.bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                this.bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves
                                //this.bufferedWaveProvider.BufferedDuration = 250;
                            }
                            int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                            //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                            bufferedWaveProvider.AddSamples(buffer, 0, decompressed);


                        }

                    } while (playbackState != StreamingPlaybackState.Stopped);
                    Debug.WriteLine("Exiting");
                    // was doing this in a finally block, but for some reason
                    // we are hanging on response stream .Dispose so never get there
                    Dispose();
                }
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (decompressor != null)
            {
                decompressor.Dispose();
            }
        }
    }
}
