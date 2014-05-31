using MCD.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using pHash.Net;
using System;
using System.Collections.Generic;
using System.IO;

namespace MCD.Core
{
    public class ReferenceCardRadialHashDetector : IReferenceCardDetector
    {
        private class ReferenceCardRadialHash
        {
            public int ID { get; set; }
            public byte[] Hash { get; set; }
        }


        private List<ReferenceCardRadialHash> _hashes = new List<ReferenceCardRadialHash>();
        private JsonSerializer _jsonSerializer = new JsonSerializer();


        public ReferenceCardRadialHashDetector()
        {
            _jsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
        }


        public void AddHash(int cardID, String imagePath)
        {
            NativeStructures.Digest digest;
            if (Hash(imagePath, out digest))
            {
                _hashes.Add(new ReferenceCardRadialHash { ID = cardID, Hash = digest.Coeffs });
            }
        }

        public void Import(Stream stream)
        {
            using (BsonReader reader = new BsonReader(stream))
            {
                reader.ReadRootValueAsArray = true;
                _jsonSerializer.Populate(reader, _hashes);
            }
        }

        public void Export(Stream stream)
        {
            using (BsonWriter writer = new BsonWriter(stream))
            {
                _jsonSerializer.Serialize(writer, _hashes);
            }
        }


        public int Detect(String imagePath, out double similarity)
        {
            similarity = 1.0;
            NativeStructures.Digest imageDigest;
            if (Hash(imagePath, out imageDigest))
            {
                int bestCardID = -1;

                foreach (ReferenceCardRadialHash hash in _hashes)
                {
                    NativeStructures.Digest cardDigest = new NativeStructures.Digest();
                    double newSimilarity = 1.0;

                    unsafe
                    {
                        fixed (byte* ptr = hash.Hash)
                        {
                            cardDigest.coeffs = (IntPtr)ptr;
                            cardDigest.size = hash.Hash.Length;
                            newSimilarity = Similarity(ref imageDigest, ref cardDigest);
                        }
                    }

                    if (newSimilarity < similarity)
                    {
                        similarity = newSimilarity;
                        bestCardID = hash.ID;
                    }
                }
                return bestCardID;
            }
            return -1;
        }

        private bool Hash(String imagePath, out NativeStructures.Digest hash)
        {
            return NativeFunctions.ph_image_digest(imagePath, 1.0, 1.0, out hash) != -1;
        }

        private double Similarity(ref NativeStructures.Digest digest1, ref NativeStructures.Digest digest2)
        {
            double result;
            NativeFunctions.ph_crosscorr(ref digest1, ref digest2, out result);
            return 1f - result;
        }
    }
}
