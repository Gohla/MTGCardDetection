namespace MCD.Interfaces
{
    public interface IReferenceCardDatabase
    {
        IReferenceCard Get(int id);
        void Add(int id, IReferenceCard card);
    }
}
