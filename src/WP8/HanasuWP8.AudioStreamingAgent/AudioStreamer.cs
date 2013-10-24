﻿using Microsoft.Phone.BackgroundAudio;
using System;

namespace HanasuWP8.AudioStreamAgent
{
    /// <summary>
    /// A background agent that performs per-track streaming for playback
    /// </summary>
    public class AudioTrackStreamer : AudioStreamingAgent
    {
        private Silverlight.Media.ShoutcastMediaStreamSource mms = null;


        /// <summary>
        /// Called when a new track requires audio decoding
        /// (typically because it is about to start playing)
        /// </summary>
        /// <param name="track">
        /// The track that needs audio streaming
        /// </param>
        /// <param name="streamer">
        /// The AudioStreamer object to which a MediaStreamSource should be
        /// attached to commence playback
        /// </param>
        /// <remarks>
        /// To invoke this method for a track set the Source parameter of the AudioTrack to null
        /// before setting  into the Track property of the BackgroundAudioPlayer instance
        /// property set to true;
        /// otherwise it is assumed that the system will perform all streaming
        /// and decoding
        /// </remarks>
        protected override void OnBeginStreaming(AudioTrack track, AudioStreamer streamer)
        {
            //TODO: Set the SetSource property of streamer to a MSS source

            var data = track.Tag.ToString().Split('$');
            var url = data[data.Length - 1];



            mms = new Silverlight.Media.ShoutcastMediaStreamSource(new Uri(url), true);
            //track.Title = "Moo";
            streamer.SetSource(mms);
            mms.MetadataChanged += mms_MetadataChanged;
            mms.Connected += mms_Connected;
            mms.Closed += mms_Closed;
        }

        void mms_Closed(object sender, EventArgs e)
        {
            NotifyComplete();
        }

        void mms_Connected(object sender, EventArgs e)
        {

        }

        void mms_MetadataChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var mms = (Silverlight.Media.ShoutcastMediaStreamSource)sender;

            var track = BackgroundAudioPlayer.Instance.Track;
            track.BeginEdit();
            try
            {
                var data = mms.CurrentMetadata.Title.Split(new string[] { " - " }, StringSplitOptions.None);
                track.Artist = data[0];
                track.Title = data[1];
            }
            catch (Exception) { }
            track.EndEdit();
            //NotifyComplete();
        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// The call to base.OnCancel() is necessary to release the background streaming resources
        /// </summary>
        protected override void OnCancel()
        {
            if (mms != null)
            {
                try
                {
                    mms.Connected -= mms_Connected;
                    mms.MetadataChanged -= mms_MetadataChanged;
                    mms.Dispose();
                    mms.Closed -= mms_Closed;
                }
                catch (Exception) { }
            }

            base.OnCancel();
        }
    }
}
