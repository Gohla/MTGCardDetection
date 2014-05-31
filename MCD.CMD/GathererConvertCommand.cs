using ManyConsole;
using MCD.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace MCD.CMD
{
    public class GathererConvertCommand : ConsoleCommand
    {
        private class GathererCardData
        {
            public int id;
            public String lang = null;
            public String name = null;
            public String altart = null;
            public String cost = null;
            public String color = null;
            public String type = null;
            public String set = null;
            public String rarity = null;
            public String power = null;
            public String toughness = null;
            public String rules = null;
            public String printedname = null;
            public String printedtype = null;
            public String printedrules = null;
            public String flavor = null;
            public String watermark = null;
            public String cardnum = null;
            public String artist = null;
            public String sets = null;
            public String rulings = null;
        }


        public String GathererDatabasePath;
        public String GathererImagesPath;
        public String ExportDatabasePath;
        public String ExportDetectorPath;


        public GathererConvertCommand()
        {
            this.IsCommand("gatherer-convert", "Converts JSON database and images from Gatherer Downloader to MCD format.");

            this.HasRequiredOption<String>("idb|input-database-path=", "Path to input JSON database file.", p => GathererDatabasePath = p);
            this.HasRequiredOption<String>("iimg|input-images-path=", "Path to input images directory.", p => GathererImagesPath = p);
            this.HasRequiredOption<String>("odb|output-database-path=", "Path to output card database file.", p => ExportDatabasePath = p);
            this.HasRequiredOption<String>("odt|output-detector-path=", "Path to output card detector file.", p => ExportDetectorPath = p);
        }


        public override int Run(String[] remainingArguments)
        {
            ReferenceCardDatabase database = new ReferenceCardDatabase();
            ReferenceCardRadialHashDetector detector = new ReferenceCardRadialHashDetector();

            using (StreamReader streamReader = File.OpenText(GathererDatabasePath))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                JObject root = JToken.ReadFrom(reader) as JObject;
                JObject cards = root.GetValue("MTGCardInfo") as JObject;
                Dictionary<int, GathererCardData> cardDictionary = cards.ToObject<Dictionary<int, GathererCardData>>();
                foreach (KeyValuePair<int, GathererCardData> pair in cardDictionary)
                {
                    ReferenceCard card = new ReferenceCard
                    {
                        Name = pair.Value.name
                    };

                    database.Add(pair.Key, card);
                }
            }

            String[] paths = Directory.GetFiles(GathererImagesPath, "*.jpg", SearchOption.AllDirectories);
            foreach (String path in paths)
            {
                String imageIDStr = Path.GetFileNameWithoutExtension(path);
                int imageID;
                if (int.TryParse(imageIDStr, out imageID))
                {
                    detector.AddHash(imageID, path);
                }
            }

            using (Stream stream = File.OpenWrite(ExportDatabasePath))
            {
                database.Export(stream);
            }
            using (Stream stream = File.OpenWrite(ExportDetectorPath))
            {
                detector.Export(stream);
            }

            return 0;
        }
    }
}
