using System;

namespace MCD.Interfaces
{
    public interface IReferenceCard
    {
        int ID { get; }
        String Language { get; }
        String Name { get; }
        String Cost { get; }
        String Type { get; }
    }
}
