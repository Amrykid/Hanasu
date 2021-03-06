﻿using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.BackgroundAudio;
using IpcWrapper;
using Microsoft.Devices;

namespace HanasuWP8.AudioPlaybackAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        private NamedEvent connectedEvent = null;
        private NamedEvent errorEvent = null;

        public AudioPlayer()
        {
            connectedEvent = new NamedEvent(IPCConsts.CONNECTED_EVENT_NAME, true);
            errorEvent = new NamedEvent(IPCConsts.ERROR_EVENT_NAME, true);
        }

        /// <remarks>
        /// AudioPlayer instances can share the same process.
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        static AudioPlayer()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
            else
                e.Handled = true;
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback.
        ///
        /// Notable playstate events:
        /// (a) TrackEnded: invoked when the player has no current track. The agent can set the next track.
        /// (b) TrackReady: an audio track has been set and it is now ready for playack.
        ///
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            switch (playState)
            {
                case PlayState.TrackEnded:
                    //player.Track = GetPreviousTrack();
                    player.Track = null;
                    break;
                case PlayState.TrackReady:
                    player.Play();
                    connectedEvent.Set();

                    if (track != null && track.Tag != null)
                    {
                        var data = track.Tag.ToString().Split('$');
                        var url = data[data.Length - 1];
                        var title = data[1];
                        var type = data[2];

                        //#region from http://stackoverflow.com/questions/7159900/detect-application-launch-from-universal-volume-control
                        //MediaHistoryItem nowPlaying = new MediaHistoryItem();
                        //nowPlaying.Title = title;
                        //nowPlaying.PlayerContext.Add("station", title);
                        //MediaHistory.Instance.NowPlaying = nowPlaying;
                        //#endregion
                    }
                    break;
                case PlayState.Shutdown:
                    // TODO: Handle the shutdown state here (e.g. save state)
                    break;
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    break;
                case PlayState.Paused:
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.BufferingStarted:
                    break;
                case PlayState.BufferingStopped:
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.FastForwarding:
                    break;
            }

            NotifyComplete();
        }

        /// <summary>
        /// Called when the user requests an action using application/system provided UI
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported.
        ///
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            switch (action)
            {
                case UserAction.Play:
                    if (track != null && track.Tag != null)
                    {
                        var data = track.Tag.ToString().Split('$');
                        var url = data[data.Length - 1];

                        var type = data[2];

                        if (type.ToLower() != "shoutcast")
                        {
                            track.Source = new Uri(url);
                        }
                    }

                    //player.Track = new AudioTrack(null, "", "", "", null, track.Tag, EnabledPlayerControls.Pause);
                    if (player.SafeGetPlayerState() != PlayState.Playing)
                    {
                        player.Play();
                    }
                    break;
                case UserAction.Stop:
                    if (player.SafeGetPlayerState() == PlayState.Playing)
                        player.Stop();
                    break;
                case UserAction.Pause:
                    if (player.SafeGetPlayerState() == PlayState.Playing)
                        player.Pause();
                    break;
                case UserAction.FastForward:
                    //player.FastForward();
                    break;
                case UserAction.Rewind:
                    //player.Rewind();
                    break;
                case UserAction.Seek:
                    //player.Position = (TimeSpan)param;
                    break;
                case UserAction.SkipNext:
                    //player.Track = GetNextTrack();
                    break;
                case UserAction.SkipPrevious:
                    //AudioTrack previousTrack = GetPreviousTrack();
                    //if (previousTrack != null)
                    //{
                    //    player.Track = previousTrack;
                    //}
                    break;
            }

            NotifyComplete();
        }

        /// <summary>
        /// Implements the logic to get the next AudioTrack instance.
        /// In a playlist, the source can be from a file, a web request, etc.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if the playback is completed</returns>
        private AudioTrack GetNextTrack()
        {
            // TODO: add logic to get the next audio track

            AudioTrack track = null;

            // specify the track

            return track;
        }

        /// <summary>
        /// Implements the logic to get the previous AudioTrack instance.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if previous track is not allowed</returns>
        private AudioTrack GetPreviousTrack()
        {
            // TODO: add logic to get the previous audio track

            AudioTrack track = null;

            // specify the track

            return track;
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            errorEvent.Set();

            if (isFatal)
            {
                Abort();
            }
            else
            {
                NotifyComplete();
            }

        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        /// <remarks>
        /// Once the request is Cancelled, the agent gets 5 seconds to finish its work,
        /// by calling NotifyComplete()/Abort().
        /// </remarks>
        protected override void OnCancel()
        {
            connectedEvent.Set();
            connectedEvent.Dispose();
            errorEvent.Dispose();

            NotifyComplete();
        }
    }
}