using AForge.Video.DirectShow;
using MCD.Interfaces;

namespace MCD.Core
{
    public class AForgeCaptureDeviceFactory
    {
        public ICaptureDevice First()
        {
            foreach (FilterInfo device in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                return new AForgeCaptureDevice(device.MonikerString);
            }
            return null;
        }
    }
}
