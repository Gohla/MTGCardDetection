using System;
using System.Drawing;

namespace MCD.Core
{
    public interface ICaptureDevice
    {
        IObservable<Bitmap> Frames { get; }

        void Start();
        void Stop();
    }
}
