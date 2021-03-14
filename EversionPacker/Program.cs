using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using System.Drawing;
using System.Text.RegularExpressions;
using ShinyTools;

namespace EversionPacker
{
    class Program
    {
        //static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    return EmbeddedAssembly.Get(args.Name);
        //}

        static string QuitResult = "This program is dedicated to \"Stormystar\".";
        static string ArchivePath;
        static string GraphicsPath;
        static string TransparentColour;

        static void Main(string[] args)
        {
            //string GZipResource = "EversionPacker.ICSharpCode.SharpZipLib.dll";
            //string DrawingResource = "EversionPacker.System.Drawing.Common.dll";
            //EmbeddedAssembly.Load(GZipResource, "ICSharpCode.SharpZipLib.dll");
            //EmbeddedAssembly.Load(DrawingResource, "System.Drawing.Common.dll");
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            //Console.Write(
            ShinyTools.Helpers.Console.WriteColor("E", ConsoleColor.Yellow);
            ShinyTools.Helpers.Console.WriteColor("V", ConsoleColor.Green);
            ShinyTools.Helpers.Console.WriteColor("E", ConsoleColor.Cyan);
            ShinyTools.Helpers.Console.WriteColor("R", ConsoleColor.Blue);
            ShinyTools.Helpers.Console.WriteColor("S", ConsoleColor.DarkMagenta);
            ShinyTools.Helpers.Console.WriteColor("I", ConsoleColor.Magenta);
            ShinyTools.Helpers.Console.WriteColor("O", ConsoleColor.Red);
            ShinyTools.Helpers.Console.WriteColor("N", ConsoleColor.DarkRed);
            Console.Write(" Packer v0.6.2 by ");
                ShinyTools.Helpers.Console.WriteColor("[hy]\n", ConsoleColor.Magenta);
                Console.Write("with research provided by ");
                ShinyTools.Helpers.Console.WriteColor("shrubbyfrog\n\n", ConsoleColor.Green);
                Console.WriteLine("FILES SUPPORTED ARE:\n" +
                "-.cha\n" +
                //  One day I'll get ZRS files implemented.
                //"-.zrs\n\n" +
                "GAMES SUPPORTED:\n" +
                "-Eversion\n" +
                "-Eversion HD\n" +
                "-Eversion HD (Steam)\n\n" +
                "====\n"
            );

            /* Setup */
            args = new string[3];
            if (string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Enter the path to the CHA file you wish to replace. If empty, one will be generated for you.");
                args[0] = Console.ReadLine();
                if (!string.IsNullOrEmpty(args[0]))
                {
                    ArchivePath = args[0];
                }
            }
            if (string.IsNullOrEmpty(args[1]))
            {
                Console.WriteLine("Enter the path to the folder containing the graphics you wish to pack.");
                do
                {
                    args[1] = Console.ReadLine();
                    GraphicsPath = args[1];
                    if (args[1].Length <= 0 || args[1] == "\n") {
                        args[1] = "";
                        Console.CursorTop -= 1; 
                    }
                }
                while (string.IsNullOrEmpty(args[1]));
            }
            if (string.IsNullOrEmpty(args[2]))
            {
                Console.WriteLine("What are the graphics\' transparent colour? (hexadecimal RGB (e.g. 400040))" /*If empty, it will use the upper-leftmost pixel's colour.*/);
                do
                {
                    args[2] = Console.ReadLine();
                    TransparentColour = args[2];

                }
                while (string.IsNullOrEmpty(args[2]));
            }
            Func(ArchivePath, GraphicsPath, TransparentColour);
            QuitResult = "OK!";
            ShinyTools.Helpers.Application.Quit(QuitResult, 1200);
        }

        /* Main Function */
        static string[] SupportedImageTypes = new string[]
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".bmp",
            ".gif"
        };
        static byte[] ArchiveHeader = new byte[0x40];
        static byte[] SpriteHeader = new byte[0x10];
        static void Func(string _archive, string _graphics, string _TransparentColour)
		{
            var InvalidChars = Path.GetInvalidPathChars();
            foreach (char invalid in InvalidChars)
            {
                _archive = _archive.Replace(invalid.ToString(), "");
                _graphics = _graphics.Replace(invalid.ToString(), "");
            }

            //We will be using these in the loop
            ChaArchive _OutCha = new ChaArchive();
            List<ChaSprite> Sprites = new List<ChaSprite>(); //This will get converted into an array to then be put into the ChaArchive type variable.
            Bitmap _image;
            short _count = 0;

            //We want to get all the files in the graphics directory then sort them so they'll be in the proper order
            List<string> files = Directory.GetFiles(_graphics).ToList();
            //files.Sort(new ShinyTools.Helpers.Comparers.IndexComparer());
            Func<string, object> convert = str =>
            {
                try { return int.Parse(str); }
                catch { return str; }
            };
            Console.WriteLine("Grabbing sprites...");
            files = files.OrderBy(
                str => Regex.Split(str.Replace(" ", ""), "([0-9]+)").Select(convert),
                new ShinyTools.Helpers.Comparers.EnumerableComparer<object>()).ToList();

            //create an indicator that the file was made with EP
            _OutCha.HeadSignature = BitConverter.ToInt16(Encoding.ASCII.GetBytes("hy"),0);
            foreach (var item in files)
			{
                if (SupportedImageTypes.Contains(Path.GetExtension(item).ToLower()))
				{
                    _count++;
                    _image = new Bitmap(item);
                    byte[] sx = new byte[0x4], sy = new byte[0x4], cx = new byte[0x4], cy = new byte[0x4];
                    //If this is the first image we're working with, we initialise header junk.
                    if (_count == 1)
					{
                        if (!string.IsNullOrEmpty(_archive) && CheckArchive(_archive))
						{
							/* Get the archive header */
							#region Grab archive header
							//byte[] sw,sh,soffx,soffy,coffx,coffy,sc,magic,transparent;
							byte[] sw = new byte[0x2], sh = new byte[0x2], soffx = new byte[0x2], soffy = new byte[0x2], coffx = new byte[0x2], coffy = new byte[0x2], sc = new byte[0x2], magic = new byte[0x2], render = new byte[0x2];
                            byte[] transparent = new byte[0x4];
                            //Copy from the header to these intermediary variables.
                            Array.Copy(ArchiveHeader, 0x4, sw, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0x6, sh, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0x8, soffx, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0xA, soffy, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0xC, coffx, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0xE, coffy, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0x10, sc, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0x12, magic, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0x20, render, 0, 0x2);
                            Array.Copy(ArchiveHeader, 0x24, transparent, 0, 0x4);
                            
                            if (!BitConverter.IsLittleEndian)
							{
                                sw = ShinyTools.Helpers.Parsers.SwapEndianness(sw, 0x2);
                                sh = ShinyTools.Helpers.Parsers.SwapEndianness(sh, 0x2);
                                soffx = ShinyTools.Helpers.Parsers.SwapEndianness(soffx, 0x2);
                                soffy = ShinyTools.Helpers.Parsers.SwapEndianness(soffy, 0x2);
                                coffx = ShinyTools.Helpers.Parsers.SwapEndianness(coffx, 0x2);
                                coffy = ShinyTools.Helpers.Parsers.SwapEndianness(coffy, 0x2);
                                sc = ShinyTools.Helpers.Parsers.SwapEndianness(sc, 0x2);
                                magic = ShinyTools.Helpers.Parsers.SwapEndianness(magic, 0x2);
                                render = ShinyTools.Helpers.Parsers.SwapEndianness(magic, 0x2);
                                transparent = ShinyTools.Helpers.Parsers.SwapEndianness(magic, 0x4);
							}

                            //Now we set the values to the reconstructed archive
                            _OutCha.SpriteWidth = BitConverter.ToInt16(sw, 0);
                            _OutCha.SpriteHeight = BitConverter.ToInt16(sh, 0);
                            _OutCha.SpriteXOffset = BitConverter.ToInt16(soffx, 0);
                            _OutCha.SpriteYOffset = BitConverter.ToInt16(soffy, 0);
                            _OutCha.ColliderWidth = BitConverter.ToInt16(coffx, 0);
                            _OutCha.ColliderHeight = BitConverter.ToInt16(coffy, 0);
                            _OutCha.SpriteCount = BitConverter.ToInt16(sc, 0); //Tentatively set the header to the original's sprite count (they shouldn't differ in practice for now)
                            _OutCha.HeadUn12t13 = BitConverter.ToInt16(magic, 0);
                            _OutCha.HeadUn20t21 = BitConverter.ToInt16(render, 0);
                            _OutCha.SpriteTransparentColour = BitConverter.ToInt32(transparent, 0);
                            #endregion
                            #region Grab sprite header
                            Array.Copy(SpriteHeader, 0, sx, 0, 0x4);
                            Array.Copy(SpriteHeader, 0x4, sy, 0, 0x4);
                            Array.Copy(SpriteHeader, 0x8, cx, 0, 0x4);
                            Array.Copy(SpriteHeader, 0xC, cy, 0, 0x4);
							if (!BitConverter.IsLittleEndian)
							{
								sx = ShinyTools.Helpers.Parsers.SwapEndianness(sx, 0x4);
								sy = ShinyTools.Helpers.Parsers.SwapEndianness(sy, 0x4);
								cx = ShinyTools.Helpers.Parsers.SwapEndianness(cx, 0x4);
								cy = ShinyTools.Helpers.Parsers.SwapEndianness(cy, 0x4);
							}
                            #endregion
                        }
						else
						{
                            //set cha header to this image's size
                            _OutCha.SpriteWidth = (short)_image.Width;
                            _OutCha.SpriteHeight = (short)_image.Height;
                            _OutCha.SpriteXOffset = (short)0;
                            _OutCha.SpriteYOffset = (short)0;
                            _OutCha.ColliderWidth = (short)_image.Width;
                            _OutCha.ColliderHeight = (short)_image.Height;
                            _OutCha.SpriteCount = (short)0;
                            _OutCha.HeadUn12t13 = (short)0;
                            if (!string.IsNullOrEmpty(_TransparentColour)) _OutCha.SpriteTransparentColour = ChaSprite.GetSpriteTransparentColour(_TransparentColour);
							else
							{
                                var p = _image.GetPixel(0, 0);
                                _OutCha.SpriteTransparentColour = (p.R << 16) + (p.G << 8) + (p.B << 0);
							}
                            //set sprite header info to fallback
                            sx = BitConverter.GetBytes(_OutCha.SpriteXOffset);
                            sy = BitConverter.GetBytes(_OutCha.SpriteYOffset);
                            cx = BitConverter.GetBytes(_OutCha.SpriteXOffset);
                            cy = BitConverter.GetBytes(_OutCha.SpriteYOffset);
						}
					}
                    //Now we convert the image into sprite data.
                    var _sprite = new ChaSprite();
                    _sprite.SpriteVisXOffset = BitConverter.ToInt32(sx, 0);
                    _sprite.SpriteVisYOffset = BitConverter.ToInt32(sy, 0);
                    _sprite.ColliderXOffset = BitConverter.ToInt32(cx, 0);
                    _sprite.ColliderYOffset = BitConverter.ToInt32(cy, 0);
                    _sprite.SetSpriteHeader();
                    _sprite.SpriteData = ChaSprite.ConvertImageToSpriteData(_image, _TransparentColour);
                    Sprites.Add(_sprite);
				}
			}
            _OutCha.Sprites = Sprites.ToArray();
            _OutCha.SpriteCount = _count;
            _OutCha.SetHeader();

            //output to file in directory of graphics folder rather than the directory of the graphics themselves
            //(e.g. C:/Users/Zee Tee/Desktop/My Custom Graphics rather C:/Users/Zee Tee/Desktop/My Custom Graphics/player)
            string _dir, _name;
			if (string.IsNullOrEmpty(_archive) || _archive == "")
			{
                if (string.IsNullOrEmpty(_graphics) || _graphics == "")
				{
                    _dir = Directory.GetCurrentDirectory();
                    _name = "My Custom Sprites";
				}
				else
				{
                    _dir = _graphics;
                    _name = Path.GetFileNameWithoutExtension(_dir);
				}
			}
			else
			{
                _dir = Path.GetDirectoryName(_archive);
                _name = Path.GetFileNameWithoutExtension(_archive);
            }
            //combine header and all sprites into a new byte array
            byte[] ChaResult = _OutCha.Serialize();
            //gzip this byte array
            byte[] output = CompressArchive(ChaResult);

            SaveVerification(output, _dir, _name, ".cha", true);
        }

        /* Prepare archive. */
        static bool CheckArchive(string path)
		{
            bool result;
            if (!File.Exists(path))
			{
                Console.WriteLine($"An archive was not found at path: {path}\nAn archive will be generated for you.");
                result = false;
            }
			else
			{
				try
				{
                    ArchiveHeader = ChaArchive.GetFromArchive(path, 0, 0x40);
                    SpriteHeader = ChaArchive.GetFromArchive(path, 0x40, 0x10);
                    Console.WriteLine("Archive passed.");
                    result = true;
				}
				catch
				{
                    Console.WriteLine("Could not grab header from archive. Check that the file is a valid Eversion graphics archive and is not corrupt.");
                    Console.WriteLine("An archive will be generated for you.");
                    result = false;
                }
			}

            return result;
		}
        /* Compress back to gzip */
        static byte[] CompressArchive(byte[] buffer)
		{
            MemoryStream _gzstream = new MemoryStream(buffer);
            MemoryStream _out = new MemoryStream();
            GZip.Compress(_gzstream, _out, true);

            return _out.ToArray();
		}

        /* Write file */
        static void SaveVerification(byte[] data, string directory, string name, string extension, bool overwrite)
		{
            string path = directory + name + extension;
            if (File.Exists(path))
			{
                if (overwrite)
				{
                    File.Delete(path);
                    ShinyTools.Helpers.IO.Write(data, directory, name, extension);
				}
				else
				{
                    ShinyTools.Helpers.Console.WriteColor("ERROR: Could not create the file as it already exists.\nPlease remove the file and try again.\n", ConsoleColor.Red);
                    Console.WriteLine("Press any key to quit the application.");
                    Console.ReadKey(true);
                    ShinyTools.Helpers.Application.Quit(QuitResult, 100, 80);
                }
			}
            else ShinyTools.Helpers.IO.Write(data, directory, name, extension);
        }
    }
}
