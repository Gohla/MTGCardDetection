using System;

namespace MCD.Interfaces
{
    public interface IReferenceCardDetector
    {
        int Detect(String imagePath, out double similarity);
    }
}
