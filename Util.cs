using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace OWArcadeDataToolConverter
{
    public class Util
    {

        public static void ClearImageOutputDirectory(string ImageOutputDirectory)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(ImageOutputDirectory);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            Console.WriteLine("--- Cleared image directory ---");
        }

        public static void CopyFileAs(string imageDirectory, string ImageOutputDirectory, string currentFileName, string newFileName)
        {
            try
            {
                File.Copy(imageDirectory + "/" + currentFileName, ImageOutputDirectory + "/" + newFileName);
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine("File not found: {0}", currentFileName);
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("Something went wrong with file" + currentFileName);
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(input)))
                    .Replace("-", string.Empty);
            }
        }

        public static string CalculateImageFilename(JObject modeObj)
        {
            // First calculate filename (i.e Total Mayhem_D.JPG, Mirrored Deathmatch _tx0C0000000001FEE9__A8.JPG)
            string GUID = ((string)modeObj["GUID"]).TrimStart(new char[] { '0' }); // Remove leading zeros
            GUID = GUID.Substring(0, GUID.LastIndexOf('.')); // Select everything leading till 

            return ((string)modeObj["Name"]).Replace(":", "_").Replace("<", "_").Replace(">", "_").TrimEnd() + "_" + GUID + ".JPG";
        }

        public static string[] RemoveShitFromName(JObject modeObj)
        {
            var test = CalculateImageFilename(modeObj);
            string newimageFileName = test.Replace(" _tx0C0000000001FEE9_", "");
            string newModeName = ((string)modeObj["Name"]).Replace(" <tx0C0000000001FEE9>", "");

            return new string[] { newimageFileName, newModeName };
        }
    }
}