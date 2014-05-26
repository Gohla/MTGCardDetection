using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Core
{
    public class AForgeCardDetector
    {
        private volatile String _smoothMode = "None";
        private volatile String _edgeMode = "Canny";
        private volatile String _drawMode = "Original";

        private volatile int _threshold = 0;
        private volatile int _minHeight = 10;
        private volatile int _minWidth = 10;
        private volatile int _minDistance = 5;
        private volatile int _minArea = 1000;


        public String SmoothMode { set { _smoothMode = value; } get { return _smoothMode; } }
        public String EdgeMode { set { _edgeMode = value; } get { return _edgeMode; } }
        public String DrawMode { set { _drawMode = value; } get { return _drawMode; } }

        public int Threshold { set { _threshold = value; } get { return _threshold; } }
        public int MinHeight { set { _minHeight = value; } get { return _minHeight; } }
        public int MinWidth { set { _minWidth = value; } get { return _minWidth; } }
        public int MinDistance { set { _minDistance = value; } get { return _minDistance; } }
        public int MinArea { set { _minArea = value; } get { return _minArea; } }


        public AForgeCardDetector()
        {

        }

        public Bitmap Detect(Bitmap bitmap)
        {
            Bitmap grayscaleBitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);

            IFilter smoothingFilter = null;
            switch (_smoothMode)
            {
                case "None": smoothingFilter = null; break;
                case "Mean": smoothingFilter = new Mean(); break;
                case "Median": smoothingFilter = new Median(); break;
                case "Conservative": smoothingFilter = new ConservativeSmoothing(); break;
                case "Adaptive": smoothingFilter = new AdaptiveSmoothing(); break;
                case "Bilateral": smoothingFilter = new BilateralSmoothing(); break;
            }
            Bitmap smoothBitmap = smoothingFilter != null ? smoothingFilter.Apply(grayscaleBitmap) : grayscaleBitmap;

            IFilter edgeFilter = null;
            switch (_edgeMode)
            {
                case "Homogenity": edgeFilter = new HomogenityEdgeDetector(); break;
                case "Difference": edgeFilter = new DifferenceEdgeDetector(); break;
                case "Sobel": edgeFilter = new SobelEdgeDetector(); break;
                case "Canny": edgeFilter = new CannyEdgeDetector(); break;
            }
            Bitmap edgeBitmap = edgeFilter != null ? edgeFilter.Apply(smoothBitmap) : smoothBitmap;

            IFilter threshholdFilter = new Threshold(_threshold);
            Bitmap thresholdBitmap = _threshold == 0 ? edgeBitmap : threshholdFilter.Apply(edgeBitmap);

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = _minHeight;
            blobCounter.MinWidth = _minWidth;
            blobCounter.ProcessImage(thresholdBitmap);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            Bitmap outputBitmap = new Bitmap(thresholdBitmap.Width, thresholdBitmap.Height, PixelFormat.Format24bppRgb);
            Graphics bitmapGraphics = Graphics.FromImage(outputBitmap);
            Bitmap inputBitmap = null;
            switch (_drawMode)
            {
                case "Original": inputBitmap = bitmap; break;
                case "Grayscale": inputBitmap = grayscaleBitmap; break;
                case "Smooth": inputBitmap = smoothBitmap; break;
                case "Edge": inputBitmap = edgeBitmap; break;
                case "Threshold": inputBitmap = thresholdBitmap; break;
            }
            if (inputBitmap != null)
                bitmapGraphics.DrawImage(inputBitmap, 0, 0);

            Pen nonConvexPen = new Pen(Color.Red, 2);
            Pen nonRectPen = new Pen(Color.Orange, 2);
            Pen cardPen = new Pen(Color.Blue, 2);

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            List<IntPoint> cardPositions = new List<IntPoint>();

            for (int i = 0; i < blobs.Length; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                List<IntPoint> corners;

                if (shapeChecker.IsConvexPolygon(edgePoints, out corners))
                {
                    PolygonSubType subType = shapeChecker.CheckPolygonSubType(corners);

                    if ((subType == PolygonSubType.Parallelogram || subType == PolygonSubType.Rectangle) && corners.Count == 4)
                    {
                        // Check if its sideways, if so rearrange the corners so it's vertical.
                        RearrangeCorners(corners);

                        // Prevent detecting the same card twice by comparing distance against other detected cards.
                        bool sameCard = false;
                        foreach (IntPoint point in cardPositions)
                        {
                            if (corners[0].DistanceTo(point) < _minDistance)
                            {
                                sameCard = true;
                                break;
                            }
                        }
                        if (sameCard)
                            continue;

                        // Hack to prevent it from detecting smaller sections of the card instead of the whole card.
                        if (GetArea(corners) < _minArea)
                            continue;

                        cardPositions.Add(corners[0]);

                        bitmapGraphics.DrawPolygon(cardPen, ToPointsArray(corners));
                    }
                    else
                    {
                        foreach (IntPoint point in edgePoints.Take(300))
                        {
                            bitmapGraphics.DrawEllipse(nonRectPen, point.X, point.Y, 1, 1);
                        }
                    }
                }
                else
                {
                    foreach (IntPoint point in edgePoints.Take(300))
                    {
                        bitmapGraphics.DrawEllipse(nonConvexPen, point.X, point.Y, 1, 1);
                    }
                }
            }

            bitmapGraphics.Dispose();
            nonConvexPen.Dispose();
            nonRectPen.Dispose();
            cardPen.Dispose();

            return outputBitmap;
        }

        private double GetDeterminant(double x1, double y1, double x2, double y2)
        {
            return x1 * y2 - x2 * y1;
        }

        private double GetArea(IList<IntPoint> vertices)
        {
            if (vertices.Count < 3)
            {
                return 0;
            }
            double area = GetDeterminant(vertices[vertices.Count - 1].X, vertices[vertices.Count - 1].Y, vertices[0].X, vertices[0].Y);
            for (int i = 1; i < vertices.Count; i++)
            {
                area += GetDeterminant(vertices[i - 1].X, vertices[i - 1].Y, vertices[i].X, vertices[i].Y);
            }
            return area / 2;
        }

        private void RearrangeCorners(List<IntPoint> corners)
        {
            float[] pointDistances = new float[4];

            for (int x = 0; x < corners.Count; x++)
            {
                IntPoint point = corners[x];

                pointDistances[x] = point.DistanceTo((x == (corners.Count - 1) ? corners[0] : corners[x + 1]));
            }

            float shortestDist = float.MaxValue;
            Int32 shortestSide = Int32.MaxValue;

            for (int x = 0; x < corners.Count; x++)
            {
                if (pointDistances[x] < shortestDist)
                {
                    shortestSide = x;
                    shortestDist = pointDistances[x];
                }
            }

            if (shortestSide != 0 && shortestSide != 2)
            {
                IntPoint endPoint = corners[0];
                corners.RemoveAt(0);
                corners.Add(endPoint);
            }
        }

        private System.Drawing.Point[] ToPointsArray(List<IntPoint> points)
        {
            System.Drawing.Point[] array = new System.Drawing.Point[points.Count];

            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new System.Drawing.Point(points[i].X, points[i].Y);
            }

            return array;
        }
    }
}
