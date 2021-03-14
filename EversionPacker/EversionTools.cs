using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.GZip;
using System.Linq;

namespace ShinyTools
{
	public class ChaArchive
	{
		
		/* Grab an archive's header. */
		[System.Obsolete("Legacy; Use GetFromArchive instead.")]
		public static byte[] GetArchiveHeader(string path)
		{
			//We create a buffer to hold the relevant decompressed data.
			byte[] _buffer = new byte[0x40];
			try
			{
				//We unpack the gzip and copy it to a more friendly stream.
				MemoryStream _compressed = new MemoryStream(File.ReadAllBytes(path));
				MemoryStream _decompressed = new MemoryStream();
				GZip.Decompress(_compressed, _decompressed, true);
				Array.Copy(_decompressed.ToArray(), _buffer, 0x40);
				_compressed.Close();
				_decompressed.Close();
			}
			catch
			{
				//If the file isn't gzipped, like in early Shiny games, or has already been extracted we just read the file.
				Array.Copy(File.ReadAllBytes(path), _buffer, 0x40);
			}
			return _buffer;
		}
		/* Grab the first sprite's header. */
		[System.Obsolete("Legacy; Use GetFromArchive instead.")]
		public static byte[] GetSpriteHeader(string path)
		{
			//We create a buffer to hold the relevant decompressed data.
			byte[] _buffer = new byte[0x10];
			try
			{
				//We unpack the gzip and copy it to a more friendly stream.
				MemoryStream _compressed = new MemoryStream(File.ReadAllBytes(path));
				MemoryStream _decompressed = new MemoryStream();
				GZip.Decompress(_compressed, _decompressed, true);
				Array.Copy(_decompressed.ToArray(), 0x40, _buffer, 0, 0x10);
				_compressed.Close();
				_decompressed.Close();
			}
			catch
			{
				//If the file isn't gzipped, like in early Shiny games, or has already been extracted we just read the file.
				Array.Copy(File.ReadAllBytes(path), 0x40, _buffer, 0, 0x10);
			}
			return _buffer;
		}
		/* Grab a range of bytes from an archive. (intended for archive and sprite headers or single sprite data) */
		public static byte[] GetFromArchive(string path, int index, int length)
		{

			//We create a buffer to hold the relevant decompressed data.
			byte[] _buffer = new byte[length];
			try
			{
				//We unpack the gzip and copy it to a more friendly stream.
				MemoryStream _compressed = new MemoryStream(File.ReadAllBytes(path));
				MemoryStream _decompressed = new MemoryStream();
				GZip.Decompress(_compressed, _decompressed, true);
				Array.Copy(_decompressed.ToArray(), index, _buffer, 0, length);
				_compressed.Close();
				_decompressed.Close();
			}
			catch
			{
				//If the file isn't gzipped, like in early Shiny games, or has already been extracted we just read the file.
				Array.Copy(File.ReadAllBytes(path), index, _buffer, 0, length);
			}
			return _buffer;
		}
															/*
															 *=================================================
															 * CHA HEADER BYTE ARRAY
															 * ------------------------------------------------
															 * these are variables of various types and are
															 * still under investigation. like any other
															 * variable in the files, they are stored in
															 * little endian byte order.
															 *=================================================
															 */
		public byte[] CHAHeader {get; private set;}			//	this is where we deposit the values when we're done
		public short HeadSignature = 0x0200;				//  0x00-0x01	(little endian short)	--	always little endian '0x0200'
		public short HeadUn2t3 = 0;							//  0x02-0x03	(little endian short)	--	unused?
		public short SpriteWidth = 0x2000;					//  0x04-0x05	(little endian short)	--	sprite width
		public short SpriteHeight = 0x2000;					//  0x06-0x07	(little endian short)	--	sprite height
		public short SpriteXOffset = 0;						//  0x08-0x09	(little endian short)	--	unused?, matches SpriteVisXOffset in separator
		public short SpriteYOffset = 0;						//  0x0A-0x0B	(little endian short)	--	unused?, matches SpriteVisYOffset in separator
		public short ColliderWidth = 0x2000;				//  0x0C-0x0D	(little endian short)	--	horizontal rightward collision offset, matches SpriteColXOffset in separator
		public short ColliderHeight = 0x2000;				//  0x0E-0x0F	(little endian short)	--	vertical downward collision offset, matches SpriteColYOffset in separator
		public short SpriteCount = 0;						//  0x10-0x11	(little endian short)	--	how many sprites are in the archive
		public short HeadUn12t13 = 0;						//  0x12-0x13	(little endian short)	--	unused?, game crashes at greather than 4, always 0
		public byte[] HeadUn14t1F = new byte[0xC];			//  0x14-0x1F	(???)					--	unused?
		public short HeadUn20t21 = 0x0300;					//	0x20-0x21	(little endian short)	--	always 3
		public short HeadUn22t23 = 0;						//	0x22-0x23	(little endian short)	--	always 0
		public int SpriteTransparentColour = 0x40004000;	//  0x24-0x27	(little endian colour)	--	little endian is bgra
		public byte[] HeadUn28t3F = new byte[0x18];         //	0x28-0x3F	(???)					--	unused?

		public ChaSprite[] Sprites;

		//	This sucks ass
		public void SetHeader()
		{
			byte[] _HeadSignatureBA = BitConverter.GetBytes(HeadSignature);
			byte[] _HeadUn2t3BA = BitConverter.GetBytes(HeadUn2t3);
			byte[] _SpriteWidthBA = BitConverter.GetBytes(SpriteWidth);
			byte[] _SpriteHeightBA = BitConverter.GetBytes(SpriteHeight);
			byte[] _SpriteXOffsetBA = BitConverter.GetBytes(SpriteXOffset);
			byte[] _SpriteYOffsetBA = BitConverter.GetBytes(SpriteYOffset);
			byte[] _HeadColWidthBA = BitConverter.GetBytes(ColliderWidth);
			byte[] _HeadColHeightBA = BitConverter.GetBytes(ColliderHeight);
			byte[] _SpriteCountBA = BitConverter.GetBytes(SpriteCount);
			byte[] _HeadUn12t13BA = BitConverter.GetBytes(HeadUn12t13);
			byte[] _HeadUn14t1FBA = HeadUn14t1F;	//	This is just zeroes
			byte[] _HeadUn20t21BA = BitConverter.GetBytes(HeadUn20t21);
			byte[] _HeadUn22t23BA = BitConverter.GetBytes(HeadUn22t23);
			byte[] _SpriteTransparentColourBA = BitConverter.GetBytes(SpriteTransparentColour);
			byte[] _HeadUn28t3FBA = HeadUn28t3F;    //	This is just zeroes
			/*
			//	Since we are packing, we want to reverse each variable's bytes to fit within the little-endianness of the game when we are big-endian..
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_HeadSignatureBA);
				Array.Reverse(_HeadUn2t3BA);
				Array.Reverse(_SpriteWidthBA);
				Array.Reverse(_SpriteHeightBA);
				Array.Reverse(_SpriteXOffsetBA);
				Array.Reverse(_SpriteYOffsetBA);
				Array.Reverse(_HeadColWidthBA);
				Array.Reverse(_HeadColHeightBA);
				Array.Reverse(_SpriteCountBA);
				Array.Reverse(_HeadUn12t13BA);
				Array.Reverse(_HeadUn14t1FBA);	//	Why am I reversing an array full of zeroes?
				Array.Reverse(_HeadUn20t21BA);
				Array.Reverse(_HeadUn22t23BA);
				Array.Reverse(_SpriteTransparentColourBA);
				Array.Reverse(_HeadUn28t3FBA);	//	Why am I reversing an array full of zeroes?
			}*/
			CHAHeader = new byte[0x40];
			Array.Copy(_HeadSignatureBA, 0, CHAHeader, 0x0, 0x2);
			Array.Copy(_HeadUn2t3BA, 0, CHAHeader, 0x2, 0x2);
			Array.Copy(_SpriteWidthBA, 0, CHAHeader, 0x4, 0x2);
			Array.Copy(_SpriteHeightBA, 0, CHAHeader, 0x6, 0x2);
			Array.Copy(_SpriteXOffsetBA, 0, CHAHeader, 0x8, 0x2);
			Array.Copy(_SpriteYOffsetBA, 0, CHAHeader, 0xA, 0x2);
			Array.Copy(_HeadColWidthBA, 0, CHAHeader, 0xC, 0x2);
			Array.Copy(_HeadColHeightBA, 0, CHAHeader, 0xE, 0x2);
			Array.Copy(_SpriteCountBA, 0, CHAHeader, 0x10, 0x2);
			Array.Copy(_HeadUn12t13BA, 0, CHAHeader, 0x12, 0x2);
			Array.Copy(_HeadUn14t1FBA, 0, CHAHeader, 0x14, 0xC);	//	This is just zeroes
			Array.Copy(_HeadUn20t21BA, 0, CHAHeader, 0x20, 0x2);
			Array.Copy(_HeadUn22t23BA, 0, CHAHeader, 0x22, 0x2);
			Array.Copy(_SpriteTransparentColourBA, 0, CHAHeader, 0x24, 4);
			Array.Copy(_HeadUn28t3FBA, 0, CHAHeader, 0x28, 0x18);   //	This is just zeroes

			if (!BitConverter.IsLittleEndian)
			{
				CHAHeader = Helpers.Parsers.SwapEndianness(CHAHeader, 0x2);
			}
		}
		//I wanted to do this entirely by using arrays and Array.Copy() but it became too much of a hassle for a no-sleep brain.
		public byte[] Serialize()
		{
			List<byte> ChaResult = new List<byte>();
			for (int i = 0; i < CHAHeader.Length; i++)
			{
				ChaResult.Add(CHAHeader[i]);
			}
			foreach (var item in Sprites)
			{
				for(int h = 0; h < item.SpriteHeader.Length; h++)
				{
					ChaResult.Add(item.SpriteHeader[h]);
				}
				for (int s = 0; s < item.SpriteData.Length; s++)
				{
					ChaResult.Add(item.SpriteData[s]);
				}
			}
			return ChaResult.ToArray();
		}
	}
	public class ChaSprite
	{
																	/*
																	 *=================================================
																	 * CHA SPRITE SEPARATOR BYTE ARRAY
																	 * ------------------------------------------------
																	 * these are ints that set truncated to signed
																	 * shorts by the game. Only the last two bytes get
																	 * read (or first two in little endian order).
																	 *=================================================
																	 */
		public byte[] SpriteHeader { get; private set; }			//	this is where we deposit the values when we're done
		public int SpriteVisXOffset;								//  0x0-0x3 (little endian int)	--	equivalent to HeadUn8t9
		public int SpriteVisYOffset;								//  0x4-0x7 (little endian int)	--	equivalent to HeadUnAtB
		public int ColliderXOffset;									//  0x8-0xB (little endian int)	--	equivalent to HeadColWidth
		public int ColliderYOffset;									//  0xC-0xF (little endian int)	--	equivalent to HeadColHeight
		public byte[] SpriteData;									//	this is where the sprite gets redrawn

		//	This sucks ass
		public void SetSpriteHeader()
		{
			byte[] _SpriteVisXOffsetBA = BitConverter.GetBytes(SpriteVisXOffset);
			byte[] _SpriteVisYOffsetBA = BitConverter.GetBytes(SpriteVisYOffset);
			byte[] _SpriteColXOffsetBA = BitConverter.GetBytes(ColliderXOffset);
			byte[] _SpriteColYOffsetBA = BitConverter.GetBytes(ColliderYOffset);
			/*
			//	Since we are packing, we want to reverse each variable's bytes to fit within the little-endianness of the game.
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_SpriteVisXOffsetBA);
				Array.Reverse(_SpriteVisYOffsetBA);
				Array.Reverse(_SpriteColXOffsetBA);
				Array.Reverse(_SpriteColYOffsetBA);
			}
			*/
			SpriteHeader = new byte[0x10];
			Array.Copy(_SpriteVisXOffsetBA, 0, SpriteHeader, 0x0, 4);
			Array.Copy(_SpriteVisYOffsetBA, 0, SpriteHeader, 0x4, 4);
			Array.Copy(_SpriteColXOffsetBA, 0, SpriteHeader, 0x8, 4);
			Array.Copy(_SpriteColYOffsetBA, 0, SpriteHeader, 0xC, 4);

			if (!BitConverter.IsLittleEndian)
			{
				SpriteHeader = Helpers.Parsers.SwapEndianness(SpriteHeader, 0x2);
			}
		}
		public static int GetSpriteTransparentColour(string transparentColour)
		{
			byte _R;
			byte _G;
			byte _B;
			var _transparent = transparentColour.Substring(0, 6);
			if (byte.TryParse(_transparent, out byte result))
			{
				switch (_transparent.Length)
				{
					case 1:
						_R = _G = _B = (byte)((result << 4) + result);
						break;
					case 2:
						_R = (byte)(((result & 0xF0) >> 4) + (result & 0xF0));
						_B = (byte)(((result & 0x0F) << 4) + (result & 0x0F));
						_G = result;
						break;
					case 3:
						_R = (byte)(((result & 0xF00) >> 8) + (result & 0xF00));
						_G = (byte)(((result & 0x0F0) >> 4) + (result & 0x0F0));
						_B = (byte)(((result & 0x00F) >> 0) + (result & 0x00F));
						break;
					case 4:
						_R = (byte)(((result & 0xF000) >> 12) + (result & 0xF000));
						_G = (byte)(((result & 0x0F00) >> 8) + (result & 0x0F00));
						_B = (byte)(((result & 0x00F0) >> 4) + (result & 0x00F0));
						break;
					case 6:
						_R = (byte)((result & 0xFF0000) >> 24);
						_G = (byte)((result & 0x00FF00) >> 16);
						_B = (byte)((result & 0x0000FF) >> 0);
						break;
					default:
						_R = 0x40;
						_G = 0x0;
						_B = 0x40;
						break;
				}
			}
			else
			{
				_R = 0x40;
				_G = 0x0;
				_B = 0x40;
			}
			return ((_R << 16) + (_G << 8) + (_B << 0));
		}
		public static byte[] ConvertImageToSpriteData(Bitmap sprite, string transparentColour)
		{
			byte[] _SpriteData = new byte[sprite.Width * sprite.Height * 3];
			for (int y = 0; y < sprite.Height; y++)
			{
				for(int x = 0; x < sprite.Width; x++)
				{
					var _colour = sprite.GetPixel(x, y);
					byte _R;
					byte _G;
					byte _B;
					if (_colour.A <= 0x10)
					{
						_R = (byte)((GetSpriteTransparentColour(transparentColour) & 0xFF0000) >> 16);
						_G = (byte)((GetSpriteTransparentColour(transparentColour) & 0x00FF00) >> 8);
						_B = (byte)((GetSpriteTransparentColour(transparentColour) & 0x0000FF) >> 0);
					}
					else
					{
						_R = _colour.R;
						_G = _colour.G;
						_B = _colour.B;
					}
					_SpriteData[((y * 3) * sprite.Width) + (x * 3) + 0] = _R;
					_SpriteData[((y * 3) * sprite.Width) + (x * 3) + 1] = _G;
					_SpriteData[((y * 3) * sprite.Width) + (x * 3) + 2] = _B;
				}
			}
			return _SpriteData;
		}
	}
}
