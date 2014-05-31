using MCD.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.IO;

namespace MCD.Core
{
    public class ReferenceCardDatabase : IReferenceCardDatabase
    {
        private Dictionary<int, IReferenceCard> _cardsByID = new Dictionary<int, IReferenceCard>();
        private JsonSerializer _jsonSerializer = new JsonSerializer();


        public ReferenceCardDatabase()
        {
            _jsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
        }


        public void Import(Stream stream)
        {
            using (BsonReader reader = new BsonReader(stream))
            {
                _jsonSerializer.Populate(reader, _cardsByID);
            }
        }

        public void Export(Stream stream)
        {
            using (BsonWriter writer = new BsonWriter(stream))
            {
                _jsonSerializer.Serialize(writer, _cardsByID);
            }
        }


        public IReferenceCard Get(int id)
        {
            return _cardsByID[id];
        }

        public void Add(int id, IReferenceCard card)
        {
            _cardsByID.Add(id, card);
        }
    }
}
