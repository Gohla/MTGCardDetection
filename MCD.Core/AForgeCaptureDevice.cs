using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Reactive.Subjects;

namespace MCD.Core
{
    public class AForgeCaptureDevice : ICaptureDevice
    {
        private Subject<Bitmap> _frames = new Subject<Bitmap>();
        private VideoCaptureDevice _videoDevice;

        public IObservable<Bitmap> Frames { get { return _frames; } }

        public AForgeCaptureDevice(String monikerString)
        {
            _videoDevice = new VideoCaptureDevice(monikerString);
            _videoDevice.NewFrame += NewFrame;
        }

        public void Start()
        {
            _videoDevice.Start();
        }

        public void Stop()
        {
            _videoDevice.Stop();
        }

        private void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _frames.OnNext(new Bitmap(eventArgs.Frame));
        }
    }
}
