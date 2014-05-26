using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using MCD.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using MCD.Interfaces;

namespace MCD.UI
{
    public partial class MainWindow : Window
    {
        private ICaptureDevice _captureDevice;
        private AForgeCardDetector _cardDetector;

        public AForgeCardDetector CardDetector { get { return _cardDetector; } }

        public MainWindow()
        {
            _cardDetector = new AForgeCardDetector();

            DataContext = this;

            InitializeComponent();

            AForgeCaptureDeviceFactory captureDeviceFactory = new AForgeCaptureDeviceFactory();
            _captureDevice = captureDeviceFactory.First();
            if (_captureDevice == null)
                return;

            _captureDevice.Frames
                //.Sample(TimeSpan.FromSeconds(0.1))
                .Select(Process)
                .ObserveOn(Webcam)
                .Subscribe(Display);

            _captureDevice.Start();
        }

        private Bitmap Process(Bitmap bitmap)
        {
            return _cardDetector.Detect(bitmap);
        }

        private void Display(Bitmap bitmap)
        {
            Webcam.Source = bitmap.ToBitmapImage();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_captureDevice != null)
                _captureDevice.Stop();
            base.OnClosing(e);
        }
    }
}
