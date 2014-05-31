using ManyConsole;
using MCD.Core;
using MCD.Interfaces;
using System;
using System.IO;

namespace MCD.CMD
{
    public class DetectCommand : ConsoleCommand
    {
        public String DatabasePath;
        public String DetectorPath;
        public String ImagePath;


        public DetectCommand()
        {
            this.IsCommand("detect", "Detects the name of an image of a MTG card");

            this.HasRequiredOption<String>("db|database-path=", "Path to card database file", p => DatabasePath = p);
            this.HasRequiredOption<String>("dt|detector-path=", "Path to card detector file", p => DetectorPath = p);
            this.HasRequiredOption<String>("i|image=", "Path to image file", p => ImagePath = p);
        }


        public override int Run(String[] remainingArguments)
        {
            ReferenceCardDatabase database = new ReferenceCardDatabase();
            using (Stream stream = File.OpenRead(DatabasePath))
            {
                database.Import(stream);
            }
            ReferenceCardRadialHashDetector detector = new ReferenceCardRadialHashDetector();
            using (Stream stream = File.OpenRead(DetectorPath))
            {
                detector.Import(stream);
            }

            double similarity;
            int cardID = detector.Detect(ImagePath, out similarity);
            if (cardID == -1)
            {
                Console.WriteLine("No card detected");
                return 1;
            }

            IReferenceCard card = database.Get(cardID);
            Console.WriteLine("Card detected: " + cardID + " - " + card.Name + ", similarity " + similarity);
            return 0;
        }
    }
}
