using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OWArcadeBackend.Models.Overwatch;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OWArcadeDataToolConverter
{
    class Program : Util
    {
        public static string rootDirectory = "C:\\Data\\Projects\\OverwatchArcade\\OWArcadeDataToolConverter";
        public static string inDirectory = rootDirectory + "\\in\\";
        public static string OutDirectory = rootDirectory + "\\out\\";
        public static string imageInDirectory = inDirectory + "\\images\\";
        public static string imageOutDirectory = OutDirectory + "\\images\\";
        static void Main(string[] args)
        {
            ClearImageOutputDirectory(imageOutDirectory);

            List<ArcadeMode> modes = JsonConvert.DeserializeObject<List<ArcadeMode>>(File.ReadAllText(inDirectory + "modes.json"), new ArcadeModeJsonConverter());
            modes = modes.Where(x => x.Players != null).ToList();
            using (var writer = new StreamWriter(OutDirectory + "modes.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                //csv.Configuration.Reg<ArcadeModeMap>();
                csv.WriteRecords(modes);
            }

            Console.WriteLine();
        }

        public class ArcadeModeJsonConverter : JsonConverter<ArcadeMode>
        {
            public override ArcadeMode ReadJson(JsonReader reader, Type objectType, [AllowNull] ArcadeMode existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                ArcadeMode mode = new ArcadeMode();


                mode.Name = (string)obj["Name"];
                mode.Description = ((string)obj["Description"]);
                mode.Game = 0;
                mode.Image = CalculateImageFilename(obj);
                // Sometimes modes contain weird characters. We have to rename mode name and the associated file.
                if (mode.Name.Contains("tx0C0000000001FEE9"))
                {
                    string[] cleanFileNames = RemoveShitFromName(obj);
                    // Copy filee with correct naming to output directory
                    CopyFileAs(imageInDirectory, imageInDirectory, mode.Image, cleanFileNames[0]);
                    mode.Image = cleanFileNames[0];
                    mode.Name = cleanFileNames[1];
                }
                if (obj["About"].Count() > 0)
                {
                    mode.Players = (string)obj["About"][0];
                }
                else
                {
                    mode.Players = "-";
                }

                // Rename image file to md5 file (modename + modeplayers)
                string md5ImageName = CreateMD5(mode.Name + mode.Players) + ".jpg";
                CopyFileAs(imageInDirectory, imageOutDirectory, mode.Image, md5ImageName);
                mode.Image = md5ImageName;
                return mode;
            }

            public override void WriteJson(JsonWriter writer, [AllowNull] ArcadeMode value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class ArcadeModeMap : ClassMap<ArcadeMode>
        {
            public ArcadeModeMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.Id).Ignore();
            }
        }
    }
}
