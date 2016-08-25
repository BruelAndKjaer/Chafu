using System;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using UIKit;

namespace Chafu
{
    public enum PlaybackState
    {
        Stopped = 0,
        Playing,
        Paused,
        Failed
    }

    public enum BufferingState
    {
        Unknown = 0,
        Ready,
        Delayed
    }

    public class VideoPlayerView : UIView
    {
        private PlaybackState _playbackState;
        private BufferingState _bufferingState;
        private AVPlayerItem _playerItem;
        private AVAsset _asset;

        // KVO contexts

        private int PlayerObserverContext = 0;
        private int PlayerItemObserverContext = 0;
        private int PlayerLayerObserverContext = 0;

        // KVO player keys

        private string PlayerTracksKey = "tracks";
        private string PlayerPlayableKey = "playable";
        private string PlayerDurationKey = "duration";
        private string PlayerRateKey = "rate";

        // KVO player item keys

        private string PlayerStatusKey = "status";
        private string PlayerEmptyBufferKey = "playbackBufferEmpty";
        private string PlayerKeepUp = "playbackLikelyToKeepUp";
        private string PlayerLoadedTimeRanges = "loadedTimeRanges";

        // KVO player layer keys

        private string PlayerReadyForDisplay = "readyForDisplay";
        private NSObject _timeObserver;

        public event EventHandler Ready;
        public event EventHandler<PlaybackState> PlaybackStateChanged;
        public event EventHandler<BufferingState> BufferingStateChanged;
        public event EventHandler<double> CurrentTimeChanged;
        public event EventHandler PlaybackWillStartFromBeginning;
        public event EventHandler PlaybackEnded;
        public event EventHandler Looped;
       

        public AVPlayer Player
        {
            get { return PlayerLayer?.Player; }
            set
            {
                var player = PlayerLayer?.Player;
                if (player != null && !player.Equals(value))
                    ((AVPlayerLayer) Layer).Player = value;
            }
        }

        public AVPlayerLayer PlayerLayer => Layer as AVPlayerLayer;

        public bool Muted
        {
            get { return Player.Muted; }
            set { Player.Muted = value; }
        }

        public AVLayerVideoGravity FillMode
        {
            get { return PlayerLayer.VideoGravity; }
            set { PlayerLayer.VideoGravity = value; }
        }

        public bool Loops
        {
            get { return Player.ActionAtItemEnd == AVPlayerActionAtItemEnd.None; }
            set { Player.ActionAtItemEnd = value ? AVPlayerActionAtItemEnd.None : AVPlayerActionAtItemEnd.Pause; }
        }

        public bool FreezesAtEnd { get; set; }

        public PlaybackState PlaybackState
        {
            get { return _playbackState; }
            set
            {
                if (_playbackState != value || !PlaybackEdgeTriggered)
                    PlaybackStateChanged?.Invoke(this, _playbackState);

                _playbackState = value;
            }
        }

        public BufferingState BufferingState
        {
            get { return _bufferingState; }
            set
            {
                if (_bufferingState != value || !PlaybackEdgeTriggered)
                    BufferingStateChanged?.Invoke(this, _bufferingState);

                _bufferingState = value;
            }
        }

        public bool PlaybackEdgeTriggered { get; private set; } = true;
        public double BufferSize { get; private set; } = 10.0;

        public double MaximumDuration => _playerItem?.Duration.ToDouble() ?? 0;
        public double CurrentTime => _playerItem?.CurrentTime.ToDouble() ?? 0;
        public CGSize NaturalSize => 
            _playerItem?.Asset?.TracksWithMediaType(AVMediaType.Video)?[0]?.NaturalSize ?? CGSize.Empty; // ??????

        public void SetUrl(NSUrl url)
        {
            if (PlaybackState == PlaybackState.Playing)
                Pause();

            SetupPlayerItem(null);
            var asset = new AVUrlAsset(url);
            SetupAsset(asset);
        }

        private void Initialize()
        {
            Player = new AVPlayer {ActionAtItemEnd = AVPlayerActionAtItemEnd.Pause};
            Player.AddObserver(this, PlayerRateKey, NSKeyValueObservingOptions.OldNew, IntPtr.Zero);

            _timeObserver = Player.AddPeriodicTimeObserver(new CMTime(1, 100), DispatchQueue.MainQueue, time =>
            {
                CurrentTimeChanged?.Invoke(this, time.ToDouble());
            });

            FreezesAtEnd = false;

            PlayerLayer.BackgroundColor = UIColor.Black.CGColor;
            FillMode = AVLayerVideoGravity.ResizeAspect;
            PlayerLayer.Hidden = true;
            PlayerLayer.AddObserver(this, PlayerReadyForDisplay, NSKeyValueObservingOptions.OldNew, IntPtr.Zero);

            UIApplication.Notifications.ObserveWillResignActive(ApplicationWillResignActive);
            UIApplication.Notifications.ObserveDidEnterBackground(ApplicationDidEnterBackground);
            UIApplication.Notifications.ObserveWillEnterForeground(ApplicationWillEnterForeground);
        }

        private void ApplicationWillEnterForeground(object sender, NSNotificationEventArgs nsNotificationEventArgs)
        {
            if (PlaybackState == PlaybackState.Paused)
                PlayFromCurrentTime();
        }

        private void ApplicationDidEnterBackground(object sender, NSNotificationEventArgs nsNotificationEventArgs)
        {
            if (PlaybackState == PlaybackState.Playing)
                Pause();
        }

        private void ApplicationWillResignActive(object sender, NSNotificationEventArgs nsNotificationEventArgs)
        {
            if (PlaybackState == PlaybackState.Playing)
                Pause();
        }

        public void PlayFromBeginning()
        {
            PlaybackWillStartFromBeginning?.Invoke(this, EventArgs.Empty);
            Player.Seek(CMTime.Zero);
            PlayFromCurrentTime();
        }

        private void PlayFromCurrentTime()
        {
            PlaybackState = PlaybackState.Playing;
            Player.Play();
        }

        public void Pause()
        {
            if (PlaybackState != PlaybackState.Playing)
                return;

            Player.Pause();
            PlaybackState = PlaybackState.Paused;
        }

        public void Stop()
        {
            if (PlaybackState == PlaybackState.Stopped)
                return;

            Player.Pause();
            PlaybackState = PlaybackState.Stopped;
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }

        public void SeekToTime(CMTime time)
        {
            _playerItem.Seek(time);
        }

        private void SetupAsset(AVAsset asset)
        {
            if (PlaybackState == PlaybackState.Playing)
                Pause();

            BufferingState = BufferingState.Unknown;

            _asset = asset;
            if (asset != null)
                SetupPlayerItem(null);

            var keys = new[] {PlayerTracksKey, PlayerPlayableKey, PlayerDurationKey};

            _asset?.LoadValuesAsynchronously(keys, () =>
            {
                DispatchQueue.MainQueue.DispatchSync(() =>
                {
                    foreach (var key in keys)
                    {
                        NSError error;
                        var status = _asset.StatusOfValue(key, out error);
                        if (status == AVKeyValueStatus.Failed)
                            PlaybackState = PlaybackState.Failed;
                        return;
                    }

                    if (!_asset.Playable)
                    {
                        PlaybackState = PlaybackState.Failed;
                        return;
                    }

                    var playerItem = new AVPlayerItem(_asset);
                    SetupPlayerItem(playerItem);
                });
            });
        }

        private void SetupPlayerItem(AVPlayerItem playerItem)
        {
            
        }

        public VideoPlayerView()
        {
            Initialize();
        }

        public VideoPlayerView(IntPtr handle) : base(handle) { Initialize(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Player?.RemoveTimeObserver(_timeObserver);
                Player?.RemoveObserver(this, PlayerRateKey);
                PlayerLayer?.RemoveObserver(this, PlayerReadyForDisplay);
                Player?.Pause();
                SetupPlayerItem(null);
            }

            base.Dispose(disposing);
        }
    }
}
