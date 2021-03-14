using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EversionPacker
{
    class HeaderSeparatorBytes
    {
        static public void WriteToFile(string path, string filename, int start, int byteslength)
        {
            List<string> AllOut = new List<string>();

            if (File.Exists($"{path}/{filename}.txt")) File.Delete($"{path}/{filename}.txt");
            var newfile = File.CreateText($"{path}/{filename}.txt");
            foreach (var filepath in Directory.GetFiles(path))
            {
                var file = Path.GetFileNameWithoutExtension(filepath);
                var extension = Path.GetExtension(filepath);
                if (extension == ".cha")
                {
                    MemoryStream compressedStream = new MemoryStream(File.ReadAllBytes(filepath));
                    MemoryStream uncompressedStream = new MemoryStream();
                    ICSharpCode.SharpZipLib.GZip.GZipInputStream GZipOut = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(compressedStream);
                    GZipOut.CopyTo(uncompressedStream);
                    compressedStream.Close();
                    GZipOut.Close();

                    var buffer = uncompressedStream.ToArray();

                    //int SpriteCount = SpriteBuffer.Length / (SpriteRegionLength + SpriteSeparatorLength);
                    //var iterations = buffer.Length / (offsettonext + byteslength);
                    var iterations = 1;

                    for (int i = 0; i < iterations; i++)
                    {
                        Console.WriteLine($"Accessing {file}{i}...");
                        //offsettonext *= i;
                        string bytestring = "[";
                        byte[] ass = new byte[byteslength];
                        Array.Copy(buffer, ass, byteslength);
                        bytestring += BitConverter.ToString(ass);
                        bytestring += "]";
                        bytestring.Replace("-", " ");
                        var Output = $"header bytes for {file} is: \t" + bytestring + Environment.NewLine;
                        AllOut.Add(Output);
                        Console.WriteLine($"Wrote {file}{i}");
                    }
                    uncompressedStream.Close();
                }
            }
            for (int i = 0; i < AllOut.Count; i++)
                newfile.Write(AllOut[i]);
            newfile.Close();
        }
    }
}
