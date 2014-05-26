using System;
using System.Drawing;

namespace MCD.Interfaces
{
    public interface ICaptureDevice
    {
        IObservable<Bitmap> Frames { get; }

        void Start();
        void Stop();
    }
}
