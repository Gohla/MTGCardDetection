using MCD.Core;
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;

namespace MCD.UI
{
    public partial class MainWindow : Window
    {
        private ICaptureDevice _captureDevice;

        public MainWindow()
        {
            InitializeComponent();

            AForgeCaptureDeviceFactory captureDeviceFactory = new AForgeCaptureDeviceFactory();
            _captureDevice = captureDeviceFactory.First();
            if (_captureDevice == null)
                return;

            _captureDevice.Frames
                .ObserveOn(Webcam)
                .Subscribe(image => { Webcam.Source = image.ToBitmapImage(); });
            _captureDevice.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_captureDevice != null)
                _captureDevice.Stop();
            base.OnClosing(e);
        }
    }
}
