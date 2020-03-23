// all rights reserved Jake Gustafson 2007
// Created 2007-09-30 in Kate

using System;
using System.Drawing;//rectangle etc
using System.Collections;
using System.IO;

//using REAL = System.Single; //System.Double

namespace ExpertMultimedia {
	public class RConvert {//formerly RConvert
		public const int int24_MinValue=-8388608;
		public const int int24_MaxValue=8388607;
		public const int uint24_MinValue=0;
		public const int uint24_MaxValue=16777215;
		//public const REAL r0=(REAL)0.0;
		//public const REAL r255=(REAL)255.0;
		public const byte by0=(byte)0;
		public const byte by255=(byte)255;
		public const short short_1=(short)-1;
		public const short short0=(short)0;
		public const ushort ushort0=(ushort)0;
		public const byte byLowNibble = 0x0F;// 15; //mask 4 bits!
		public const byte byHighNibble = 0xF0;// 240; //mask 4 bits!

		#region geometry magic numbers
		public const float F180_DIV_PI=57.2957795130823208767981548141052F; // 180/PI;
		public const double D180_DIV_PI=57.2957795130823208767981548141052D;
		//public const REAL R180_DIV_PI=(REAL)57.2957795130823208767981548141052;
		public const float FPI_DIV_180=0.017453293F; // PI/180;
		public const double DPI_DIV_180=0.017453293D;
		//public const REAL RPI_DIV_180=(REAL)0.017453293;
		//public const REAL r0=(REAL)0.0;
		//public const REAL r1=(REAL)1.0;
		//public const REAL r3=(REAL)3.0;
		//public const REAL r90=(REAL)90.0;
		//public const REAL r180=(REAL)180.0;
		//public const REAL r270=(REAL)270.0;
		#endregion geometry magic numbers

		public static readonly float fDegByteableActualMax=(255.0F/256.0F)*360.0F;
		public static readonly float fDegByteableRoundableExclusiveMax=(255.0F/256.0F)*360.0F + (360.0F-(255.0F/256.0F)*360.0F)/2.0F;
		public static readonly double dDegByteableActualMax=(255.0/256.0)*360.0;
		public static readonly double dDegByteableRoundableExclusiveMax=(255.0/256.0)*360.0 + (360.0-(255.0/256.0)*360.0)/2.0;
		public static readonly decimal mDegByteableActualExclusiveMax=(255.0M/256.0M)*360.0M;
		public static readonly decimal mDegByteableRoundableExclusiveMax=(255.0M/256.0M)*360.0M + (360.0M-(255.0M/256.0M)*360.0M)/2.0M;

		
		#region colorspace conversion magic numbers	
		//public const REAL r_34414 = (REAL)0.34414;
		//public const REAL r_71414 = (REAL).71414;
		//public const REAL 1.402 = (REAL)1.402;
		//public const REAL 1.772 = (REAL)1.772;
		#endregion colorspace conversion magic numbers	
		public static Var vColor=null;//html colors
		
		public static readonly int float_MaxFirstDigit=RConvert.ToInt(float.MaxValue.ToString().Substring(0,1));
		public static readonly int double_MaxFirstDigit=RConvert.ToInt(double.MaxValue.ToString().Substring(0,1));
		public static readonly int decimal_MaxFirstDigit=RConvert.ToInt(decimal.MaxValue.ToString().Substring(0,1));
		public static readonly int short_MaxFirstDigit=RConvert.ToInt(short.MaxValue.ToString().Substring(0,1));
		public static readonly int int24_MaxFirstDigit=RConvert.ToInt(int24_MaxValue.ToString().Substring(0,1));
		public static readonly int int_MaxFirstDigit=RConvert.ToInt(int.MaxValue.ToString().Substring(0,1));
		public static readonly int long_MaxFirstDigit=RConvert.ToInt(long.MaxValue.ToString().Substring(0,1));
		public static readonly int byte_MaxFirstDigit=RConvert.ToInt(byte.MaxValue.ToString().Substring(0,1));
		public static readonly int ushort_MaxFirstDigit=RConvert.ToInt(ushort.MaxValue.ToString().Substring(0,1));
		public static readonly int uint24_MaxFirstDigit=RConvert.ToInt(uint24_MaxValue.ToString().Substring(0,1));//1
		public static readonly int uint_MaxFirstDigit=RConvert.ToInt(uint.MaxValue.ToString().Substring(0,1));
		public static readonly int ulong_MaxFirstDigit=RConvert.ToInt(ulong.MaxValue.ToString().Substring(0,1));
		
		public static readonly int float_MaxDigits=float.MaxValue.ToString().Length;
		public static readonly int double_MaxDigits=double.MaxValue.ToString().Length;
		public static readonly int decimal_MaxDigits=decimal.MaxValue.ToString().Length;
		public static readonly int short_MaxDigits=short.MaxValue.ToString().Length;
		public static readonly int int24_MaxDigits=int24_MaxValue.ToString().Length;
		public static readonly int int_MaxDigits=int.MaxValue.ToString().Length;
		public static readonly int long_MaxDigits=long.MaxValue.ToString().Length;
		public static readonly int byte_MaxDigits=byte.MaxValue.ToString().Length;
		public static readonly int ushort_MaxDigits=ushort.MaxValue.ToString().Length;
		public static readonly int uint24_MaxDigits=uint24_MaxValue.ToString().Length;//8
		public static readonly int uint_MaxDigits=uint.MaxValue.ToString().Length;
		public static readonly int ulong_MaxDigits=ulong.MaxValue.ToString().Length;
		#region digit conversions

		#region base conversions
		public static readonly char[] carrBase64=new char[] {
			'A','B','C','D','E','F','G','H','I','J','K','L','M',
			'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
			'a','b','c','d','e','f','g','h','i','j','k','l','m',
			'n','o','p','q','r','s','t','u','v','w','x','y','z',
			'0','1','2','3','4','5','6','7','8','9','+','/'
		};
		public static readonly char[] carrBase16=new char[] {//formerly carrHex
			'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'
		};
		#endregion base conversions
		static RConvert() {
			vColor=null;
			try {
				vColor=new Var("htmlcolors",Var.TypeArray,0,150);//TODO: set max accordingly
				vColor.SetOrCreateBinaryFromHex24("AliceBlue","F0F8FF");
				vColor.SetOrCreateBinaryFromHex24("AntiqueWhite","FAEBD7");
				vColor.SetOrCreateBinaryFromHex24("Aqua","00FFFF");
				vColor.SetOrCreateBinaryFromHex24("Aquamarine","7FFFD4");
				vColor.SetOrCreateBinaryFromHex24("Azure","F0FFFF"); 
				vColor.SetOrCreateBinaryFromHex24("Beige","F5F5DC");
				vColor.SetOrCreateBinaryFromHex24("Bisque","FFE4C4");
				vColor.SetOrCreateBinaryFromHex24("Black","000000");
				vColor.SetOrCreateBinaryFromHex24("BlanchedAlmond","FFEBCD");
				vColor.SetOrCreateBinaryFromHex24("Blue","0000FF");
				vColor.SetOrCreateBinaryFromHex24("BlueViolet","8A2BE2");
				vColor.SetOrCreateBinaryFromHex24("Brown","A52A2A");
				vColor.SetOrCreateBinaryFromHex24("BurlyWood","DEB887");
				vColor.SetOrCreateBinaryFromHex24("CadetBlue","5F9EA0");
				vColor.SetOrCreateBinaryFromHex24("Chartreuse","7FFF00");
				vColor.SetOrCreateBinaryFromHex24("Chocolate","D2691E");
				vColor.SetOrCreateBinaryFromHex24("Coral","FF7F50");
				vColor.SetOrCreateBinaryFromHex24("CornflowerBlue","6495ED");
				vColor.SetOrCreateBinaryFromHex24("Cornsilk","FFF8DC");
				vColor.SetOrCreateBinaryFromHex24("Crimson","DC143C");
				vColor.SetOrCreateBinaryFromHex24("Cyan","00FFFF");
				vColor.SetOrCreateBinaryFromHex24("DarkBlue","00008B");
				vColor.SetOrCreateBinaryFromHex24("DarkCyan","008B8B");
				vColor.SetOrCreateBinaryFromHex24("DarkGoldenRod","B8860B");
				vColor.SetOrCreateBinaryFromHex24("DarkGray","A9A9A9");
				vColor.SetOrCreateBinaryFromHex24("DarkGrey","A9A9A9");
				vColor.SetOrCreateBinaryFromHex24("DarkGreen","006400");
				vColor.SetOrCreateBinaryFromHex24("DarkKhaki","BDB76B");
				vColor.SetOrCreateBinaryFromHex24("DarkMagenta","8B008B");
				vColor.SetOrCreateBinaryFromHex24("DarkOliveGreen","556B2F");
				vColor.SetOrCreateBinaryFromHex24("Darkorange","FF8C00");
				vColor.SetOrCreateBinaryFromHex24("DarkOrchid","9932CC");
				vColor.SetOrCreateBinaryFromHex24("DarkRed","8B0000");
				vColor.SetOrCreateBinaryFromHex24("DarkSalmon","E9967A");
				vColor.SetOrCreateBinaryFromHex24("DarkSeaGreen","8FBC8F");
				vColor.SetOrCreateBinaryFromHex24("DarkSlateBlue","483D8B");
				vColor.SetOrCreateBinaryFromHex24("DarkSlateGray","2F4F4F");
				vColor.SetOrCreateBinaryFromHex24("DarkSlateGrey","2F4F4F");
				vColor.SetOrCreateBinaryFromHex24("DarkTurquoise","00CED1");
				vColor.SetOrCreateBinaryFromHex24("DarkViolet","9400D3");
				vColor.SetOrCreateBinaryFromHex24("DeepPink","FF1493");
				vColor.SetOrCreateBinaryFromHex24("DeepSkyBlue","00BFFF");
				vColor.SetOrCreateBinaryFromHex24("DimGray","696969");
				vColor.SetOrCreateBinaryFromHex24("DimGrey","696969");
				vColor.SetOrCreateBinaryFromHex24("DodgerBlue","1E90FF");
				vColor.SetOrCreateBinaryFromHex24("FireBrick","B22222");
				vColor.SetOrCreateBinaryFromHex24("FloralWhite","FFFAF0");
				vColor.SetOrCreateBinaryFromHex24("ForestGreen","228B22");
				vColor.SetOrCreateBinaryFromHex24("Fuchsia","FF00FF");
				vColor.SetOrCreateBinaryFromHex24("Gainsboro","DCDCDC");
				vColor.SetOrCreateBinaryFromHex24("GhostWhite","F8F8FF");
				vColor.SetOrCreateBinaryFromHex24("Gold","FFD700");
				vColor.SetOrCreateBinaryFromHex24("GoldenRod","DAA520");
				vColor.SetOrCreateBinaryFromHex24("Gray","808080");
				vColor.SetOrCreateBinaryFromHex24("Grey","808080");
				vColor.SetOrCreateBinaryFromHex24("Green","008000");
				vColor.SetOrCreateBinaryFromHex24("GreenYellow","ADFF2F");
				vColor.SetOrCreateBinaryFromHex24("HoneyDew","F0FFF0");
				vColor.SetOrCreateBinaryFromHex24("HotPink","FF69B4");
				vColor.SetOrCreateBinaryFromHex24("IndianRed ","CD5C5C");
				vColor.SetOrCreateBinaryFromHex24("Indigo ","4B0082");
				vColor.SetOrCreateBinaryFromHex24("Ivory","FFFFF0");
				vColor.SetOrCreateBinaryFromHex24("Khaki","F0E68C");
				vColor.SetOrCreateBinaryFromHex24("Lavender","E6E6FA");
				vColor.SetOrCreateBinaryFromHex24("LavenderBlush","FFF0F5");
				vColor.SetOrCreateBinaryFromHex24("LawnGreen","7CFC00");
				vColor.SetOrCreateBinaryFromHex24("LemonChiffon","FFFACD");
				vColor.SetOrCreateBinaryFromHex24("LightBlue","ADD8E6");
				vColor.SetOrCreateBinaryFromHex24("LightCoral","F08080");
				vColor.SetOrCreateBinaryFromHex24("LightCyan","E0FFFF");
				vColor.SetOrCreateBinaryFromHex24("LightGoldenRodYellow","FAFAD2");
				vColor.SetOrCreateBinaryFromHex24("LightGray","D3D3D3");
				vColor.SetOrCreateBinaryFromHex24("LightGrey","D3D3D3");
				vColor.SetOrCreateBinaryFromHex24("LightGreen","90EE90");
				vColor.SetOrCreateBinaryFromHex24("LightPink","FFB6C1");
				vColor.SetOrCreateBinaryFromHex24("LightSalmon","FFA07A");
				vColor.SetOrCreateBinaryFromHex24("LightSeaGreen","20B2AA");
				vColor.SetOrCreateBinaryFromHex24("LightSkyBlue","87CEFA");
				vColor.SetOrCreateBinaryFromHex24("LightSlateGray","778899");
				vColor.SetOrCreateBinaryFromHex24("LightSlateGrey","778899");
				vColor.SetOrCreateBinaryFromHex24("LightSteelBlue","B0C4DE");
				vColor.SetOrCreateBinaryFromHex24("LightYellow","FFFFE0");
				vColor.SetOrCreateBinaryFromHex24("Lime","00FF00");
				vColor.SetOrCreateBinaryFromHex24("LimeGreen","32CD32");
				vColor.SetOrCreateBinaryFromHex24("Linen","FAF0E6");
				vColor.SetOrCreateBinaryFromHex24("Magenta","FF00FF");
				vColor.SetOrCreateBinaryFromHex24("Maroon","800000");
				vColor.SetOrCreateBinaryFromHex24("MediumAquaMarine","66CDAA");
				vColor.SetOrCreateBinaryFromHex24("MediumBlue","0000CD");
				vColor.SetOrCreateBinaryFromHex24("MediumOrchid","BA55D3");
				vColor.SetOrCreateBinaryFromHex24("MediumPurple","9370D8");
				vColor.SetOrCreateBinaryFromHex24("MediumSeaGreen","3CB371");
				vColor.SetOrCreateBinaryFromHex24("MediumSlateBlue","7B68EE");
				vColor.SetOrCreateBinaryFromHex24("MediumSpringGreen","00FA9A");
				vColor.SetOrCreateBinaryFromHex24("MediumTurquoise","48D1CC");
				vColor.SetOrCreateBinaryFromHex24("MediumVioletRed","C71585");
				vColor.SetOrCreateBinaryFromHex24("MidnightBlue","191970");
				vColor.SetOrCreateBinaryFromHex24("MintCream","F5FFFA");
				vColor.SetOrCreateBinaryFromHex24("MistyRose","FFE4E1");
				vColor.SetOrCreateBinaryFromHex24("Moccasin","FFE4B5");
				vColor.SetOrCreateBinaryFromHex24("NavajoWhite","FFDEAD");
				vColor.SetOrCreateBinaryFromHex24("Navy","000080");
				vColor.SetOrCreateBinaryFromHex24("OldLace","FDF5E6");
				vColor.SetOrCreateBinaryFromHex24("Olive","808000");
				vColor.SetOrCreateBinaryFromHex24("OliveDrab","6B8E23");
				vColor.SetOrCreateBinaryFromHex24("Orange","FFA500");
				vColor.SetOrCreateBinaryFromHex24("OrangeRed","FF4500");
				vColor.SetOrCreateBinaryFromHex24("Orchid","DA70D6");
				vColor.SetOrCreateBinaryFromHex24("PaleGoldenRod","EEE8AA");
				vColor.SetOrCreateBinaryFromHex24("PaleGreen","98FB98");
				vColor.SetOrCreateBinaryFromHex24("PaleTurquoise","AFEEEE");
				vColor.SetOrCreateBinaryFromHex24("PaleVioletRed","D87093");
				vColor.SetOrCreateBinaryFromHex24("PapayaWhip","FFEFD5");
				vColor.SetOrCreateBinaryFromHex24("PeachPuff","FFDAB9");
				vColor.SetOrCreateBinaryFromHex24("Peru","CD853F");
				vColor.SetOrCreateBinaryFromHex24("Pink","FFC0CB");
				vColor.SetOrCreateBinaryFromHex24("Plum","DDA0DD");
				vColor.SetOrCreateBinaryFromHex24("PowderBlue","B0E0E6");
				vColor.SetOrCreateBinaryFromHex24("Purple","800080");
				vColor.SetOrCreateBinaryFromHex24("Red","FF0000");
				vColor.SetOrCreateBinaryFromHex24("RosyBrown","BC8F8F");
				vColor.SetOrCreateBinaryFromHex24("RoyalBlue","4169E1");
				vColor.SetOrCreateBinaryFromHex24("SaddleBrown","8B4513");
				vColor.SetOrCreateBinaryFromHex24("Salmon","FA8072");
				vColor.SetOrCreateBinaryFromHex24("SandyBrown","F4A460");
				vColor.SetOrCreateBinaryFromHex24("SeaGreen","2E8B57");
				vColor.SetOrCreateBinaryFromHex24("SeaShell","FFF5EE");
				vColor.SetOrCreateBinaryFromHex24("Sienna","A0522D");
				vColor.SetOrCreateBinaryFromHex24("Silver","C0C0C0");
				vColor.SetOrCreateBinaryFromHex24("SkyBlue","87CEEB");
				vColor.SetOrCreateBinaryFromHex24("SlateBlue","6A5ACD");
				vColor.SetOrCreateBinaryFromHex24("SlateGray","708090");
				vColor.SetOrCreateBinaryFromHex24("SlateGrey","708090");
				vColor.SetOrCreateBinaryFromHex24("Snow","FFFAFA");
				vColor.SetOrCreateBinaryFromHex24("SpringGreen","00FF7F");
				vColor.SetOrCreateBinaryFromHex24("SteelBlue","4682B4");
				vColor.SetOrCreateBinaryFromHex24("Tan","D2B48C");
				vColor.SetOrCreateBinaryFromHex24("Teal","008080");
				vColor.SetOrCreateBinaryFromHex24("Thistle","D8BFD8");
				vColor.SetOrCreateBinaryFromHex24("Tomato","FF6347");
				vColor.SetOrCreateBinaryFromHex24("Turquoise","40E0D0");
				vColor.SetOrCreateBinaryFromHex24("Violet","EE82EE");
				vColor.SetOrCreateBinaryFromHex24("Wheat","F5DEB3");
				vColor.SetOrCreateBinaryFromHex24("White","FFFFFF");
				vColor.SetOrCreateBinaryFromHex24("WhiteSmoke","F5F5F5");
				vColor.SetOrCreateBinaryFromHex24("Yellow","FFFF00");
				vColor.SetOrCreateBinaryFromHex24("YellowGreen","9ACD32");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"setting HTML colors","RConvert static constructor");
			}
		}//end RConvert static constructor
		
		#region utilities
		public static string FrameToHMSDotMs(int iFrame, decimal FramesPerSecond, bool bDropFrame) {
			//TODO: check this
			//int iSecond=(int)((decimal)iFrame/FramesPerSecond);
			//iFrame=iFrame-(int)((decimal)iSecond*FramesPerSecond);
			//int iHour=iSecond/(60*60);
			//iSecond-=iHour*60*60;
			//int iMinute=iSecond/60;
			//iSecond-=iMinute*60;
			/*
			int iHour=(int)((decimal)iFrame/(FramesPerSecond*60.0m*60.0m));
			iFrame-=(int)((decimal)iHour*(FramesPerSecond*60.0m*60.0m));
			int iMinute=(int)((decimal)iFrame/(FramesPerSecond*60.0m));
			iFrame-=(int)((decimal)iMinute*(FramesPerSecond*60.0m));
			int iSecond=(int)((decimal)iFrame/(FramesPerSecond));
			iFrame-=(int)((decimal)iSecond*(FramesPerSecond));
			int iMillisecond=(int)( (decimal)iFrame*(1000.0m/FramesPerSecond) +.5m);
			*/
			//TODO: check this:
			decimal dFrame=(decimal)iFrame;
			int iHour=(int)(dFrame/(FramesPerSecond*60.0m*60.0m));
			dFrame-=((decimal)iHour*(FramesPerSecond*60.0m*60.0m));
			int iMinute=(int)(dFrame/(FramesPerSecond*60.0m));
			dFrame-=((decimal)iMinute*(FramesPerSecond*60.0m));
			int iSecond=(int)(dFrame/(FramesPerSecond));
			dFrame-=((decimal)iSecond*(FramesPerSecond));
			decimal SecondsPerFrame=1.0m/FramesPerSecond;
			int iMillisecond=(int)( dFrame*(1000.0m/FramesPerSecond) +SecondsPerFrame/2.0m);//add SecondsPerFrame/2.0m to get to the "middle" of the frame--so as not to undershoot!"
			return iHour.ToString()+":"+iMinute.ToString()+":"+iSecond.ToString()+"."+iMillisecond.ToString("D3");
		}//end FrameToHMSDotMs
		public static string StripProtocol(string sUrl) {
			int iProtocol=-1;
			if (sUrl!=null) iProtocol=sUrl.IndexOf("://");
			else sUrl="";
			if (iProtocol>-1) {
				if (sUrl.Length>iProtocol+3) sUrl=sUrl.Substring(iProtocol+3);
				else sUrl="";
			}
			return sUrl;
		}//end StripProtocol
		///<summary>
		///Sets any implemented variable to the "Nothing" value (for efficient repetitive coding i.e. use this before duplicating methods to reduce modification of code when creating overloads)
		///</summary>
		public static void SetToNothing(out string val) {
			val="";
		}
		public static void SetToNothing(out float val) {
			val=0.0F;
		}
		public static void SetToNothing(out double val) {
			val=0.0;
		}
		public static void SetToNothing(out decimal val) {
			val=0.0M;
		}
		public static void SetToNothing(out short val) {
			val=RConvert.short0;
		}
		public static void SetToNothing(out int val) {
			val=0;
		}
		public static void SetToNothing(out long val) {
			val=0;
		}
		public static void SetToNothing(out byte val) {
			val=RConvert.by0;
		}
		public static void SetToNothing(out ushort val) {
			val=RConvert.ushort0;
		}
		public static void SetToNothing(out uint val) {
			val=0U;
		}
		public static void SetToNothing(out ulong val) {
			val=0UL;
		}
		public static void SetToNothing(out byte[] val) {
			val=null;
		}
		public static void SetToNothing(out bool val) {
			val=false;
		}
		#endregion utilities
		
		
//variable suffixes:
// U:uint L:long UL:ulong F:float D:double(optional,implied) M:decimal(128-bit)
		
		
		public static float ValDigitFloat(char cDigit) {
			return ValDigitFloat(ref cDigit);
		}
		public static float ValDigitFloat(ref char cDigit) {
			if (cDigit=='1') return 1F;
			else if (cDigit=='2') return 2F;
			else if (cDigit=='3') return 3F;
			else if (cDigit=='4') return 4F;
			else if (cDigit=='5') return 5F;
			else if (cDigit=='6') return 6F;
			else if (cDigit=='7') return 7F;
			else if (cDigit=='8') return 8F;
			else if (cDigit=='9') return 9F;
			else return 0F;
		}
		public static double ValDigitDouble(char cDigit) {
			return ValDigitDouble(ref cDigit);
		}
		public static double ValDigitDouble(ref char cDigit) {
			if (cDigit=='1') return 1D;
			else if (cDigit=='2') return 2D;
			else if (cDigit=='3') return 3D;
			else if (cDigit=='4') return 4D;
			else if (cDigit=='5') return 5D;
			else if (cDigit=='6') return 6D;
			else if (cDigit=='7') return 7D;
			else if (cDigit=='8') return 8D;
			else if (cDigit=='9') return 9D;
			else return 0D;
		}
		public static decimal ValDigitDecimal(char cDigit) {
			return ValDigitDecimal(ref cDigit);
		}
		public static decimal ValDigitDecimal(ref char cDigit) {
			if (cDigit=='1') return 1M;
			else if (cDigit=='2') return 2M;
			else if (cDigit=='3') return 3M;
			else if (cDigit=='4') return 4M;
			else if (cDigit=='5') return 5M;
			else if (cDigit=='6') return 6M;
			else if (cDigit=='7') return 7M;
			else if (cDigit=='8') return 8M;
			else if (cDigit=='9') return 9M;
			else return 0M;
		}
		public static short ValDigitShort(char cDigit) {
			return ValDigitShort(ref cDigit);
		}
		public static short ValDigitShort(ref char cDigit) {
			if (cDigit=='1') return 1;
			else if (cDigit=='2') return 2;
			else if (cDigit=='3') return 3;
			else if (cDigit=='4') return 4;
			else if (cDigit=='5') return 5;
			else if (cDigit=='6') return 6;
			else if (cDigit=='7') return 7;
			else if (cDigit=='8') return 8;
			else if (cDigit=='9') return 9;
			else return 0;
		}
		public static int ValDigitInt(char cDigit) {
			return ValDigitInt(ref cDigit);
		}
		public static int ValDigitInt(ref char cDigit) {
			if (cDigit=='1') return 1;
			else if (cDigit=='2') return 2;
			else if (cDigit=='3') return 3;
			else if (cDigit=='4') return 4;
			else if (cDigit=='5') return 5;
			else if (cDigit=='6') return 6;
			else if (cDigit=='7') return 7;
			else if (cDigit=='8') return 8;
			else if (cDigit=='9') return 9;
			else return 0;
		}
		public static long ValDigitLong(char cDigit) {
			return ValDigitLong(ref cDigit);
		}
		public static long ValDigitLong(ref char cDigit) {
			if (cDigit=='1') return 1L;
			else if (cDigit=='2') return 2L;
			else if (cDigit=='3') return 3L;
			else if (cDigit=='4') return 4L;
			else if (cDigit=='5') return 5L;
			else if (cDigit=='6') return 6L;
			else if (cDigit=='7') return 7L;
			else if (cDigit=='8') return 8L;
			else if (cDigit=='9') return 9L;
			else return 0L;
		}
		public static byte ValDigitByte(char cDigit) {
			return ValDigitByte(ref cDigit);
		}
		public static byte ValDigitByte(ref char cDigit) {
			if (cDigit=='1') return 1;
			else if (cDigit=='2') return 2;
			else if (cDigit=='3') return 3;
			else if (cDigit=='4') return 4;
			else if (cDigit=='5') return 5;
			else if (cDigit=='6') return 6;
			else if (cDigit=='7') return 7;
			else if (cDigit=='8') return 8;
			else if (cDigit=='9') return 9;
			else return 0;
		}
		public static ushort ValDigitUshort(char cDigit) {
			return ValDigitUshort(ref cDigit);
		}
		public static ushort ValDigitUshort(ref char cDigit) {
			if (cDigit=='1') return 1;
			else if (cDigit=='2') return 2;
			else if (cDigit=='3') return 3;
			else if (cDigit=='4') return 4;
			else if (cDigit=='5') return 5;
			else if (cDigit=='6') return 6;
			else if (cDigit=='7') return 7;
			else if (cDigit=='8') return 8;
			else if (cDigit=='9') return 9;
			else return 0;
		}
		public static uint ValDigitUint(char cDigit) {
			return ValDigitUint(ref cDigit);
		}
		public static uint ValDigitUint(ref char cDigit) {
			if (cDigit=='1') return 1U;
			else if (cDigit=='2') return 2U;
			else if (cDigit=='3') return 3U;
			else if (cDigit=='4') return 4U;
			else if (cDigit=='5') return 5U;
			else if (cDigit=='6') return 6U;
			else if (cDigit=='7') return 7U;
			else if (cDigit=='8') return 8U;
			else if (cDigit=='9') return 9U;
			else return 0U;
		}
		public static ulong ValDigitUlong(char cDigit) {
			return ValDigitUlong(ref cDigit);
		}
		public static ulong ValDigitUlong(ref char cDigit) {
			if (cDigit=='1') return 1UL;
			else if (cDigit=='2') return 2UL;
			else if (cDigit=='3') return 3UL;
			else if (cDigit=='4') return 4UL;
			else if (cDigit=='5') return 5UL;
			else if (cDigit=='6') return 6UL;
			else if (cDigit=='7') return 7UL;
			else if (cDigit=='8') return 8UL;
			else if (cDigit=='9') return 9UL;
			else return 0UL;
		}
		
		public static int ValDigitInt(string sDigit) {
			return ValDigitInt(ref sDigit);
		}
		public static int ValDigitInt(ref string sDigit) {
			if (sDigit=="1") return 1;
			else if (sDigit=="2") return 2;
			else if (sDigit=="3") return 3;
			else if (sDigit=="4") return 4;
			else if (sDigit=="5") return 5;
			else if (sDigit=="6") return 6;
			else if (sDigit=="7") return 7;
			else if (sDigit=="8") return 8;
			else if (sDigit=="9") return 9;
			else return 0;
		}
		#endregion
	
	
	
		#region number conversions with overflow protection
		//float double decimal  short int long  byte ushort uint
		public static float ToFloat(float val) {
			try { return (float)val; }
			catch { return (val<0.0F)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(double val) {
			try { return (float)val; }
			catch { return (val<0.0)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(decimal val) {
				try { return (float)val; }
				catch { return (val<0.0M)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(short val) {
			try { return (float)val; }
			catch { return (val<0)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(int val) {
			try { return (float)val; }
			catch { return (val<0)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(long val) {
			try { return (float)val; }
			catch { return (val<0)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(byte val) {
			try { return (float)val; }
			catch { return //(val<0)?float.MinValue:
				float.MaxValue; }
		}
		public static float ToFloat(ushort val) {
			try { return (float)val; }
			catch { return //(val<0)?float.MinValue:
				float.MaxValue; }
		}
		public static float ToFloat(uint val) {
			try { return (float)val; }
			catch { return //(val<0)?float.MinValue:
				float.MaxValue; }
		}
		public static float ToFloat(ulong val) {
			try { return (float)val; }
			catch { return //(val<0)?float.MinValue:
				float.MaxValue; }
		}
		public static float ToFloat(byte[] byarrNow) {
			return ToFloat(byarrNow,0);
		}
		public static float ToFloat(byte[] byarrNow, int iAt) {
			float valReturn=0.0F;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToSingle(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToSingle(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToFloat(binary)");
			}
			return valReturn;
		}
		public static float ToFloat(bool val) {
			return val?-1.0F:0.0F; //since -1 in two's compliment is all 1s!
		}
		
		public static double ToDouble(float val) {
			try { return (double)val; }
			catch { return (val<0.0F)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(double val) {
			try { return (double)val; }
			catch { return (val<0.0)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(decimal val) {
			try { return (double)val; }
			catch { return (val<0.0M)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(short val) {
			try { return (double)val; }
			catch { return (val<0)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(int val) {
			try { return (double)val; }
			catch { return (val<0)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(long val) {
			try { return (double)val; }
			catch { return (val<0)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(byte val) {
			try { return (double)val; }
			catch { return //(val<0)?double.MinValue:
				double.MaxValue; }
		}
		public static double ToDouble(ushort val) {
			try { return (double)val; }
			catch { return //(val<0)?double.MinValue:
				double.MaxValue; }
		}
		public static double ToDouble(uint val) {
			try { return (double)val; }
			catch { return //(val<0)?double.MinValue:
				double.MaxValue; }
		}
		public static double ToDouble(ulong val) {
			try { return (double)val; }
			catch { return //(val<0)?double.MinValue:
				double.MaxValue; }
		}
		public static double ToDouble(byte[] byarrNow) {
			return ToDouble(byarrNow,0);
		}
		public static double ToDouble(byte[] byarrNow, int iAt) {
			double valReturn=0.0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToDouble(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToDouble(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToDouble(binary)");
			}
			return valReturn;
		}
		public static double ToDouble(bool val) {
			return val?-1.0:0.0; //since -1 in two's compliment is all 1s!
		}
		
		public static decimal ToDecimal(float val) {
			try { return (decimal)val; }
			catch { return (val<0.0F)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(double val) {
			try { return (decimal)val; }
			catch { return (val<0.0)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(decimal val) {
			try { return (decimal)val; }
			catch { return (val<0.0M)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(short val) {
			try { return (decimal)val; }
			catch { return (val<0)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(int val) {
			try { return (decimal)val; }
			catch { return (val<0)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(long val) {
			try { 
				return (decimal)val; }
			catch { return (val<0)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(byte val) {
			try { return (decimal)val; }
			catch { return //(val<0)?decimal.MinValue:
				decimal.MaxValue; }
		}
		public static decimal ToDecimal(ushort val) {
			try { return (decimal)val; }
			catch { return //(val<0)?decimal.MinValue:
				decimal.MaxValue; }
		}
		public static decimal ToDecimal(uint val) {
			try { return (decimal)val; }
			catch { return //(val<0)?decimal.MinValue:
				decimal.MaxValue; }
		}
		public static decimal ToDecimal(ulong val) {
			try { return (decimal)val; }
			catch { return //(val<0)?decimal.MinValue:
				decimal.MaxValue; }
		}
		public static decimal ToDecimal(byte[] byarrNow) {
			return ToDecimal(byarrNow,0);
		}
		public static decimal ToDecimal(byte[] byarrNow, int iAt) {
			decimal valReturn=0.0M;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=(decimal)BitConverter.ToDouble(byarrNow, iAt);//TODO: fix byte[] to decimal later--unimportant
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 8);//commented for debug only: 16);
					valReturn=(decimal)BitConverter.ToDouble(byarrNow, 0);//TODO: fix byte[] to decimal later--unimportant
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToDecimal(binary)");
			}
			return valReturn;
		}
		public static decimal ToDecimal(bool val) {
			return val?-1.0M:0.0M; //since -1 in two's compliment is all 1s!
		}
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static short ToShort(float val) {
			try { return (short)((val<0.0F)?(val-.5F):(val+.5F)); }
			catch { return (val<0.0F)?short.MinValue:short.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static short ToShort(double val) {
			try { return (short)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?short.MinValue:short.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static short ToShort(decimal val) {
			try { return (short)((val<0.0M)?(val-.5M):(val+.5M)); }
			catch { return (val<0.0M)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(short val) {
			try { return (short)val; }
			catch { return (val<0)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(int val) {
			try { return (short)val; }
			catch { return (val<0)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(long val) {
			try { return (short)val; }
			catch { return (val<0)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(byte val) {
			//try {
			return (short)(val); //}
			//catch { return (val<0)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(ushort val) {
			try { return (short)val; }
			catch { return //(val<0)?short.MinValue:
				short.MaxValue; }
		}
		public static short ToShort(uint val) {
			try { return (short)val; }
			catch { return //(val<0)?short.MinValue:
				short.MaxValue; }
		}
		public static short ToShort(ulong val) {
			try { return (short)val; }
			catch { return //(val<0)?short.MinValue:
				short.MaxValue; }
		}
		public static short ToShort(byte[] byarrNow) {
			return ToShort(byarrNow,0);
		}
		public static short ToShort(byte[] byarrNow, int iAt) {
			short valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToInt16(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 2);
					valReturn=BitConverter.ToInt16(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToShort(binary)");
			}
			return valReturn;
		}
		public static short ToShort(bool val) {
			return val?RConvert.short_1:RConvert.short0; //since -1 in two's compliment is all 1s!
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static int ToInt(float val) {
			try { return (int)((val<0.0F)?(val-.5f):(val+.5f)); }
			catch { return (val<0.0F)?int.MinValue:int.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static int ToInt(double val) {
			try { return (int)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?int.MinValue:int.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static int ToInt(decimal val) {
			try { return (int)((val<0.0M)?(val-.5M):(val+.5M)); }
			catch { return (val<0.0M)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(short val) {
			try { return (int)val; }
			catch { return (val<0)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(int val) {
			try { return (int)val; }
			catch { return (val<0)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(long val) {
			try { return (int)val; }
			catch { return (val<0)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(byte val) {
			//try { 
			return (int)(val); //}
			//catch { return (val<0)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(ushort val) {
			//try { 
			return (int)val; //}
			//catch { return //(val<0)?int.MinValue:
				//int.MaxValue; }
		}
		public static int ToInt(uint val) {
			try { return (int)val; }
			catch { return //(val<0)?int.MinValue:
				int.MaxValue; }
		}
		public static int ToInt(ulong val) {
			try { return (int)val; }
			catch { return //(val<0)?int.MinValue:
				int.MaxValue; }
		}
		public static int ToInt(byte[] byarrNow) {
			return ToInt(byarrNow,0);
		}
		public static int ToInt(byte[] byarrNow, int iAt) {
			int valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToInt32(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToInt32(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToInt(binary)");
			}
			return valReturn;
		}//end ToInt(byte[],iAt)
		public static int ToInt(bool val) {
			return val?-1:0; //since -1 in two's compliment is all 1s!
		}
		public static int ToInt(string sNum) {
			if (sNum==null) return 0;
			int min=int.MinValue;//-2147483648;
			int max=int.MaxValue;//2147483647;
			int iMaxDigits=int_MaxDigits;//10
			int iMaxFirstDig=int_MaxFirstDigit;//2;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			if (sNum=="") return 0;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			int result=0;
			int valDigitFinal=0;
			int valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*RConvert.ValDigitInt(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break; //prevents some valMult overflows
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*ValDigitInt(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break; //prevents some valMult overflows
					valMult*=10;
				} while(true);
			}
			return result;
		}//end ToInt(string)
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static long ToLong(float val) {
			try { return (long)((val<0.0F)?(val-.5f):(val+.5f)); }
			catch { return (val<0.0F)?long.MinValue:long.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static long ToLong(double val) {
			try { return (long)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?long.MinValue:long.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static long ToLong(decimal val) {
			try { return (long)((val<0.0M)?(val-.5M):(val+.5M)); }
			catch { return (val<0.0M)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(short val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(int val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(long val) {
			//try { 
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(byte val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(ushort val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(uint val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(ulong val) {
			try { return (long)val; }
			catch { return //(val<0)?long.MinValue:
				long.MaxValue; }
		}
		public static long ToLong(byte[] byarrNow) {
			return ToLong(byarrNow,0);
		}
		public static long ToLong(byte[] byarrNow, int iAt) {
			long valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToInt64(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToInt64(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToLong(binary)");
			}
			return valReturn;
		}
		public static long ToLong(bool val) {
			return val?-1:0; //since -1 in two's compliment is all 1s!
		}
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte(float val) { //formerly SafeByte(REAL)
			//if (val<=0.0f) return 0;
			//else if (val>=255.0f) return 255;
			//return (byte)(val+.5f);
			try { return (byte)(val+.5F); }
			catch { return (val<0.0F)?byte.MinValue:byte.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte(double val) { //formerly SafeByte(REAL)
			//if (val<=0.0d) return 0;
			//else if (val>=255.0d) return 255;
			//return (byte)(val+.5d);
			try { return (byte)(val+.5); }
			catch { return (val<0.0)?byte.MinValue:byte.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte(decimal val) {
			//if (val<=0.0M) return 0;
			//else if (val>=255.0M) return 255;
			//return (byte)(val+.5M);
			try { return (byte)(val+.5M); }
			catch { return (val<0.0M)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(short val) {
			try { return (byte)val; }
			catch { return (val<0)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(int val) {
			try { return (byte)val; }
			catch { return (val<0)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(long val) {
			try { return (byte)val; }
			catch { return (val<0)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(byte val) {
			try { return (byte)(val); }
			catch { return //(val<0)?byte.MinValue:
				byte.MaxValue; }
		}
		public static byte ToByte(ushort val) {
			try { return (byte)val; }
			catch { return //(val<0)?byte.MinValue:
				byte.MaxValue; }
		}
		public static byte ToByte(uint val) {
			try { return (byte)val; }
			catch { return //(val<0)?byte.MinValue:
				byte.MaxValue; }
		}
		public static byte ToByte(ulong val) {
			try { return (byte)val; }
			catch { return //(val<0)?byte.MinValue:
				byte.MaxValue; }
		}
		public static byte ToByte(byte[] byarrNow) {
			return ToByte(byarrNow,0);
		}
		public static byte ToByte(byte[] byarrNow, int iAt) {
			try {return byarrNow[iAt];}
			catch {return 0;}
		}
		public static byte ToByte(bool val) {
			return val?RConvert.by255:RConvert.by0;
		}
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ushort ToUshort(float val) {
			try { return (ushort)(val+.5f); }
			catch { return (val<0.0F)?ushort.MinValue:ushort.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ushort ToUshort(double val) {
			try { return (ushort)(val+.5); }
			catch { return (val<0.0)?ushort.MinValue:ushort.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ushort ToUshort(decimal val) {
			try { return (ushort)(val+.5M); }
			catch { return (val<0.0M)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(short val) {
			try { return (ushort)val; }
			catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(int val) {
			try { return (ushort)val; }
			catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(long val) {
			try { return (ushort)val; }
			catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(byte val) {
			//try {
			return (ushort)val; //}
			//catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(ushort val) {
			//try { 
			return (ushort)val; //}
			//catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(uint val) {
			try { return (ushort)val; }
			catch { return //(val<0)?ushort.MinValue:
				ushort.MaxValue; }
		}
		public static ushort ToUshort(ulong val) {
			try { return (ushort)val; }
			catch { return //(val<0)?ushort.MinValue:
				ushort.MaxValue; }
		}
		public static ushort ToUshort(byte[] byarrNow) {
			return ToUshort(byarrNow,0);
		}
		public static ushort ToUshort(byte[] byarrNow, int iAt) {
			ushort valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToUInt16(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 2);
					valReturn=BitConverter.ToUInt16(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToUshort(binary)");
			}
			return valReturn;
		}
		public static ushort ToUshort(bool val) {
			return val?ushort.MaxValue:RConvert.ushort0;
		}
		public const char c255=(char)255;
		public static char ToChar(int val) {
			return val<=0?'\0':(val>=255?c255:(char)val);
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static uint ToUint(float val) {
			try { return (uint)(val+.5F); }
			catch { return (val<0.0F)?uint.MinValue:uint.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static uint ToUint(double val) {
			try { return (uint)(val+.5); }
			catch { return (val<0.0)?uint.MinValue:uint.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static uint ToUint(decimal val) {
			try { return (uint)(val+.5M); }
			catch { return (val<0.0M)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(short val) {
			try { return (uint)val; }
			catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(int val) {
			try { return (uint)val; }
			catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint_Offset(int val) { //note: unsigned.MaxValue is signed.MaxValue+signed.MaxValue+1
			try { return (val<0) ? ((uint)((val+int.MaxValue)+1)) : ((uint)val+(uint)int.MaxValue+1); }
			catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(long val) {
			try { return (uint)val; }
			catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(byte val) {
			//try {
			return (uint)(val); //}
			//catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(ushort val) {
			//try { 
			return (uint)val; //}
			//catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(uint val) {
			//try { 
			return (uint)val; //}
			//catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(ulong val) {
			try { return (uint)val; }
			catch { return //(val<0)?uint.MinValue:
				uint.MaxValue; }
		}
		public static ulong ToUint(byte[] byarrNow) {
			return ToUint(byarrNow,0);
		}
		public static uint ToUint(byte[] byarrNow, int iAt) {
			uint valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToUInt32(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToUInt32(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToUint(binary)");
			}
			return valReturn;
		}
		public static uint ToUint(bool val) {
			return val?uint.MaxValue:0;
		}
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ulong ToUlong(float val) {
			try { return (ulong)(val+.5F); }
			catch { return (val<0.0F)?ulong.MinValue:ulong.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ulong ToUlong(double val) {
			try { return (ulong)(val+.5); }
			catch { return (val<0.0)?ulong.MinValue:ulong.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ulong ToUlong(decimal val) {
			try { return (ulong)(val+.5M); }
			catch { return (val<0.0M)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(short val) {
			try { return (ulong)val; }
			catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(int val) {
			try { return (ulong)val; }
			catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(long val) {
			try { return (ulong)val; }
			catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(byte val) {
			//try { 
			return (ulong)(val); //}
			//catch { return (val<0)?ulong.MinValue:ulong.MaxValue; //}
		}
		public static ulong ToUlong(ushort val) {
			//try { 
			return (ulong)val; //}
			//catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(uint val) {
			//try { 
			return (ulong)val; //}
			//catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(ulong val) {
			//try { 
			return (ulong)val; //}
			//catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(byte[] byarrNow) {
			return ToUlong(byarrNow,0);
		}
		public static ulong ToUlong(byte[] byarrNow, int iAt) {
			ulong valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToUInt64(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToUInt64(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 RReporting.ShowExn(exn,"RConvert ToUint(binary)");
			}
			return valReturn;
		}
		public static ulong ToUlong(bool val) {
			return val?ulong.MaxValue:0;
		}
		
		public static byte[] ToByteArray(string val) {
			return ToByteArray(val,false,true);
		}
		public static byte[] ToByteArray(string val, bool bUnicode, bool bLittleEndian) {
			try {
				byte[] byarrNow=new byte[bUnicode?(val.Length*2):(val.Length)];
				if (bUnicode) {
					int iDest=0;
					for (int iChar=0; iChar<val.Length; iChar++) {
						byarrNow[iDest]=(byte)( ((ushort)val[iChar])&0xFF );
						iDest++;
						byarrNow[iDest]=(byte)( ((ushort)val[iChar]>>8));
						iDest++;
					}
				}
				else {
					for (int iChar=0; iChar<val.Length; iChar++) {
						byarrNow[iChar]=(byte)( ((ushort)val[iChar])&0xFF );
					}
				}
				return byarrNow; 
			}
			catch { return null; }
		}
		public static byte[] ToByteArray(float val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val)); //TODO: fix decimal to byte[] later -- unimportant
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(byte val) {
			try { return new byte[]{val}; }
			catch { return null; }
		}
		public static byte[] ToByteArray(ushort val) {
			//try { return new byte[]{(byte)(val&0xFF),(byte)(val>>8)}; }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(uint val) {
			//try { return new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF)}; }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(ulong val) {
			//try { return new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF),(byte)((val>>32)&0xFF),(byte)((val>>40)&0xFF),(byte)((val>>48)&0xFF),(byte)((val>>56)&0xFF)}; }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(byte[] byarrNow) {
			return RMemory.Copy(byarrNow);
		}
		public static byte[] ToByteArray(bool val) {
			return new byte[]{val?RConvert.by255:RConvert.by0};
		}
		
		public static bool ToBool(float val) {
			return (val!=0.0F)?true:false;
		}
		public static bool ToBool(double val) {
			return (val!=0.0)?true:false;
		}
		public static bool ToBool(decimal val) {
			return (val!=0.0M)?true:false;
		}
		public static bool ToBool(short val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(int val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(long val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(byte val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(ushort val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(uint val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(ulong val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(byte[] byarrNow) {
			return ToBool(byarrNow,0);
		}
		public static bool ToBool(byte[] byarrNow, int iAt) {
			bool valReturn=false;
			try {
				if (byarrNow!=null) valReturn=(byarrNow[iAt]!=0)?true:false;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"RConvert ToBool(binary)");
			}
			return valReturn;
		}
		public static bool ToBool(bool val) {
			return val;
		}
		
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte_1As255(float val) {
			return (val<.5F)  ?  (byte)0  :  (byte)( (val>=254.5F) ? (byte)255 : ((byte)(val+.5F)) );
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte_1As255(double val) {
			return (val<.5)  ?  (byte)0  :  (byte)( (val>=254.5) ? (byte)255 : ((byte)(val+.5)) );
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte_1As255(decimal val) {
			return (val<.5M)  ? (byte)0  :  (byte)( (val>=254.5M) ? (byte)255 : ((byte)(val+.5M)) );
		}
		public static float ToFloat_255As1(byte val) {
			return ((float)val/255.0F);
		}
		public static double ToDouble_255As1(byte val) {
			return ((double)val/255.0);
		}
		public static decimal ToDecimal_255As1(byte val) {
			return ((decimal)val/255.0M);
		}
		public static void To_255As1(out float valReturn, byte val) {
			valReturn=ToFloat_255As1(val);
		}
		public static void To_255As1(out double valReturn, byte val) {
			valReturn=ToDouble_255As1(val);
		}
		public static void To_255As1(out decimal valReturn, byte val) {
			valReturn=ToDecimal_255As1(val);
		}
		#endregion number conversions with overflow protection
		
		#region conversion overloads
		public static void To(out string valReturn, string val) {
			valReturn=val;
		}
		public static void To(out float valReturn, string val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, string val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, string val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, string val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, string val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, string val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, string val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, string val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, string val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, string val) {
			valReturn=ToUlong(val);
		}

		public static void To(out string valReturn, float val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, float val) {
			valReturn=val;
		}
		public static void To(out double valReturn, float val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, float val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, float val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, float val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, float val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, float val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, float val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, float val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, float val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, double val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, double val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, double val) {
			valReturn=val;
		}
		public static void To(out decimal valReturn, double val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, double val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, double val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, double val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, double val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, double val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, double val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, double val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, decimal val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, decimal val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, decimal val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, decimal val) {
			valReturn=val;
		}
		public static void To(out short valReturn, decimal val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, decimal val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, decimal val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, decimal val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, decimal val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, decimal val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, decimal val) {
			valReturn=ToUlong(val);
		}

		public static void To(out string valReturn, short val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, short val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, short val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, short val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, short val) {
			valReturn=val;
		}
		public static void To(out int valReturn, short val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, short val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, short val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, short val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, short val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, short val) {
			valReturn=ToUlong(val);
		}

		public static void To(out string valReturn, int val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, int val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, int val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, int val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, int val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, int val) {
			valReturn=val;
		}
		public static void To(out long valReturn, int val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, int val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, int val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, int val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, int val) {
			valReturn=ToUlong(val);
		}
			
		public static void To(out string valReturn, long val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, long val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, long val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, long val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, long val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, long val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, long val) {
			valReturn=val;
		}
		public static void To(out byte valReturn, long val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, long val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, long val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, long val) {
			valReturn=ToUlong(val);
		}

		public static void To(out string valReturn, byte val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, byte val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, byte val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, byte val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, byte val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, byte val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, byte val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, byte val) {
			valReturn=val;
		}
		public static void To(out ushort valReturn, byte val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, byte val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, byte val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, ushort val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, ushort val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, ushort val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, ushort val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, ushort val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, ushort val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, ushort val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, ushort val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, ushort val) {
			valReturn=val;
		}
		public static void To(out uint valReturn, ushort val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, ushort val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, uint val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, uint val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, uint val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, uint val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, uint val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, uint val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, uint val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, uint val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, uint val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, uint val) {
			valReturn=val;
		}
		public static void To(out ulong valReturn, uint val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, ulong val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, ulong val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, ulong val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, ulong val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, ulong val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, ulong val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, ulong val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, ulong val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, ulong val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, ulong val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, ulong val) {
			valReturn=val;
		}
		
		
		public static void To(out byte[] valarrReturn, float val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val));//TODO: fix decimal to byte[] later -- unimportant
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, byte val) {
			try { valarrReturn=new byte[]{val}; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, ushort val) {
			try { valarrReturn=new byte[]{(byte)(val&0xFF),(byte)(val>>8)}; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, uint val) {
			try { valarrReturn=new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF)}; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, ulong val) {
			try { valarrReturn=new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF),(byte)((val>>32)&0xFF),(byte)((val>>40)&0xFF),(byte)((val>>48)&0xFF),(byte)((val>>56)&0xFF)}; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, byte[] byarrNow) {
			valarrReturn=RMemory.Copy(byarrNow);
		}
		
		public static void To(ref byte[] valarrDest, int iAtDest, float val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val));//TODO: fix decimal to byte[] later -- unimportant
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, byte val) {
			try { valarrDest[iAtDest]=val; }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, ushort val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)(val>>8); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, uint val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)((val>>8)&0xFF); valarrDest[iAtDest+2]=(byte)((val>>16)&0xFF); valarrDest[iAtDest+3]=(byte)((val>>24)&0xFF); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, ulong val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)((val>>8)&0xFF); valarrDest[iAtDest+2]=(byte)((val>>16)&0xFF); valarrDest[iAtDest+3]=(byte)((val>>24)&0xFF); valarrDest[iAtDest+4]=(byte)((val>>32)&0xFF); valarrDest[iAtDest+5]=(byte)((val>>40)&0xFF); valarrDest[iAtDest+6]=(byte)((val>>48)&0xFF); valarrDest[iAtDest+7]=(byte)((val>>56)&0xFF); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, byte[] byarrNow) {
			valarrDest=RMemory.Copy(byarrNow);
		}
		//DONE: float double decimal  short int long  byte ushort uint
		#endregion conversion overloads
	
		#region text conversions with overflow protection (via hard limiting)
		
		//public static REAL ToReal(string sNum) {
		//	REAL valReturn=RConvert.0.0;
		//	To(out valReturn,sNum);
		//	return valReturn;
		//}
		
		public static float ToFloat(string sNum) {
			//debug performance--do this once using renamed static vars
			float min=float.MinValue;
			float max=float.MaxValue;
			string sMax=max.ToString();
			RMath.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetFloatConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) {
				sMax=sMax+".0";
				iMaxDigits=sMax.IndexOf(".");
			}
			int iMaxFirstDig=RConvert.ValDigitInt(sMax.Substring(0,1));

			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			RMath.RemoveExpNotation(ref sNum);
			int iDot=sNum.IndexOf(".");
			float valMult;
			int iPowerStart;
			if (iDot>=0) {
				sNum=sNum.Substring(0,iDot)+sNum.Substring(iDot+1);
				iPowerStart=iDot-1;
			}
			else iPowerStart=sNum.Length-1;
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			valMult=RMath.SafeE10F(ref iPowerStart);
			float result=0;
			float valDigitFinal=0;
			int iDigit=0;
			if (bNeg) {
				do {
					valDigitFinal=valMult*RConvert.ValDigitFloat(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10F;
				} while (true);
			}
			else {
				do {
					valDigitFinal=valMult*RConvert.ValDigitFloat(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10F;
				} while(true);
			}
			return result;
		}//end ToFloat
		public static double ToDouble(string sNum) {
			//debug performance--do this once using renamed static vars
			double min=double.MinValue;
			double max=double.MaxValue;
			string sMax=max.ToString();
			RMath.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetDoubleConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) iMaxDigits=sMax.Length;
			int iMaxFirstDig=RConvert.ValDigitInt(sMax.Substring(0,1));

			bool bNeg=false;
			try {
				if (sNum.StartsWith("-")) {
					bNeg=true;
					sNum=sNum.Substring(1);
				}
				else bNeg=false;
				RMath.RemoveExpNotation(ref sNum);
				int iDot=sNum.IndexOf(".");
				double valMult;
				int iPowerStart;
				if (iDot>=0) {
					sNum=sNum.Substring(0,iDot)+sNum.Substring(iDot+1);
					iPowerStart=iDot-1;
				}
				else iPowerStart=sNum.Length-1;
				if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
				else if ((sNum.Length==iMaxDigits)
							&&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
					return (bNeg)?min:max;
				valMult=RMath.SafeE10D(ref iPowerStart);
				double result=0;
				double valDigitFinal=0;
				int iDigit=0;
				if (bNeg) {
					do {
						valDigitFinal=valMult*RConvert.ValDigitDouble(sNum[iDigit]);
						if (result>min+valDigitFinal) result-=valDigitFinal;
						else return min;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10;
					} while(true);
				}
				else {
					do {
						valDigitFinal=valMult*RConvert.ValDigitDouble(sNum[iDigit]);
						if (result<max-valDigitFinal) result+=valDigitFinal;
						else return max;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10;
					} while(true);
				}
				return result;
			}
			catch {
				return bNeg?double.MinValue:double.MaxValue;
			}
		}//end ToDouble
		public static decimal ToDecimal(string sNum) {
			//debug performance--do this once using renamed static vars
			decimal min=decimal.MinValue;
			decimal max=decimal.MaxValue;
			string sMax=max.ToString();
			RMath.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetDecimalConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) iMaxDigits=sMax.Length;
			int iMaxFirstDig=RConvert.ValDigitInt(sMax.Substring(0,1));

			bool bNeg=false;
			try {
				if (sNum.StartsWith("-")) {
					bNeg=true;
					sNum=sNum.Substring(1);
				}
				else bNeg=false;
				RMath.RemoveExpNotation(ref sNum);
				int iDot=sNum.IndexOf(".");
				decimal valMult;
				int iPowerStart;
				if (iDot>=0) {
					sNum=sNum.Substring(0,iDot)+sNum.Substring(iDot+1);
					iPowerStart=iDot-1;
				}
				else iPowerStart=sNum.Length-1;
				if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
				else if ((sNum.Length==iMaxDigits)
							&&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
					return (bNeg)?min:max;
				valMult=RMath.SafeE10M(ref iPowerStart);
				decimal result=0;
				decimal valDigitFinal=0;
				int iDigit=0;
				if (bNeg) {
					do {
						valDigitFinal=valMult*RConvert.ValDigitDecimal(sNum[iDigit]);
						if (result>min+valDigitFinal) result-=valDigitFinal;
						else return min;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10M;
					} while(true);
				}
				else {
					do {
						valDigitFinal=valMult*RConvert.ValDigitDecimal(sNum[iDigit]);
						if (result<max-valDigitFinal) result+=valDigitFinal;
						else return max;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10M;
					} while(true);
				}
				return result;
			}
			catch {
				return bNeg?decimal.MinValue:decimal.MaxValue;
			}
		}//end ToDecimal
		
		public static short ToShort(string sNum) {
			short min=short.MinValue;
			short max=short.MaxValue;
			int iMaxDigits=short_MaxDigits;
			int iMaxFirstDig=short_MaxFirstDigit;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			short result=0;
			short valDigitFinal=0;
			short valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=ToShort(valMult*RConvert.ValDigitShort(sNum[iDigit]));
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=ToShort(valMult*RConvert.ValDigitShort(sNum[iDigit]));
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}//end ToShort
		public static int ToInt24(string sNum) {
			int min=int24_MinValue;//-8388608
			int max=int24_MaxValue;//8388607
			int iMaxDigits=int24_MaxDigits;//7
			int iMaxFirstDig=int24_MaxFirstDigit;//8
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			int result=0;
			int valDigitFinal=0;
			int valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*ValDigitInt(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*ValDigitInt(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}//end ToInt24
		//TODO: check if blank before&after removing negative sign (to fix out of range exception) in EVERY To*(string sNum) method!
		public static long ToLong(string sNum) {
			long min=long.MinValue;//-9223372036854775808;
			long max=long.MaxValue;//9223372036854775807;
			int iMaxDigits=long_MaxDigits;//19;
			int iMaxFirstDig=long_MaxFirstDigit;//9;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			long result=0;
			long valDigitFinal=0;
			long valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*RConvert.ValDigitLong(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*RConvert.ValDigitLong(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}
		
		public static byte ToByte(string sNum) {
			//byte min=0;
			byte max=255;
			int iMaxDigits=byte_MaxDigits;
			int iMaxFirstDig=byte_MaxFirstDigit;
			if (sNum.StartsWith("-")) return 0;
			else if (sNum.Length>iMaxDigits) return max;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			byte result=0;
			byte valDigitFinal=0;
			byte valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=(byte)(valMult*ValDigitByte(sNum[iDigit]));
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static ushort ToUshort(string sNum) {
			//ushort min=0;
			ushort max=65535;
			int iMaxDigits=ushort_MaxDigits;
			int iMaxFirstDig=ushort_MaxFirstDigit;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			ushort result=0;
			ushort valDigitFinal=0;
			ushort valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=(ushort)(valMult*ValDigitUshort(sNum[iDigit]));
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static uint ToUint24(string sNum) {
			//uint min=0; 
			uint max=16777215;
			int iMaxDigits=8;
			int iMaxFirstDig=1;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			uint result=0;
			uint valDigitFinal=0;
			uint valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUint(sNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static uint ToUint(string sNum) {
			//uint min=0;
			uint max=4294967295;
			int iMaxDigits=10;
			int iMaxFirstDig=4;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			uint result=0;
			uint valDigitFinal=0;
			uint valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUint(sNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static ulong ToUlong(string sNum) {
			//ulong min=0;
			ulong max=18446744073709551615;
			int iMaxDigits=20;
			int iMaxFirstDig=1;
			if (sNum.StartsWith("-")) return 0;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			ulong result=0;
			ulong valDigitFinal=0;
			ulong valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUlong(sNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while(true);
			return result;
		}
		//private static int GetFloatConVarMaxDigits(ref string sMax) {
		//	int iMaxDigits=sMax.IndexOf(".");
		//	if (iMaxDigits<0) iMaxDigits=sMax.Length;
		//	return iMaxDigits;
		//}
		public static bool ToBool(string sVal) {
			bool bReturn=true;
			try {
				sVal=sVal.ToLower();
				if (sVal=="yes") return true;
				else if (sVal=="true") return true;
				else if (sVal=="no") bReturn=false;
				else if (sVal=="false") bReturn=false;
				else if (sVal=="0") bReturn=false;
				else if ((double)Convert.ToDecimal(sVal)==0.0) bReturn=false;
				else if (sVal=="") bReturn=false;
			}
			catch (Exception exn) {
				bReturn=false;
				RReporting.ShowExn(exn,"ToBool");
			} return bReturn;
		}
		#endregion text conversions with overflow protection
		
		#region to-text conversions
		public static string ToString(string val) {
			return val;
		}
		public static string ToString(float val) {
			return val.ToString();
		}
		public static string ToString(double val) {
			return val.ToString();
		}
		public static string ToString(decimal val) {
			return val.ToString();
		}
		public static string ToString(short val) {
			return val.ToString();
		}
		public static string ToString(int val) {
			return val.ToString();
		}
		public static string ToString(long val) {
			return val.ToString();
		}
		public static string ToString(byte val) {
			return char.ToString((char)val);
		}
		public static string ToString(ushort val) {
			return val.ToString();
		}
		public static string ToString(uint val) {
			return val.ToString();
		}
		public static string ToString(ulong val) {
			return val.ToString();
		}
		public static string ToString(byte[] byarrVal, bool bUnicode, bool bLittleEndian) {
			string sReturn="";
			try {
				if (bUnicode) {
					int iChars=byarrVal.Length/2;
					int iSrc=0;
					if (bLittleEndian) {
						for (int iChar=0; iChar<iChars; iChar++) {
							sReturn+=char.ToString(  (char)( (ushort)byarrVal[iSrc]+(((ushort)(byarrVal[iSrc+1]))<<8) )  );
							iSrc+=2;
						}
					}
					else {
						for (int iChar=0; iChar<iChars; iChar++) {
							sReturn+=char.ToString(  (char)( (ushort)byarrVal[iSrc+1]+(((ushort)(byarrVal[iSrc]))<<8) )  );
							iSrc+=2;
						}
					}
				}
				else {
					for (int iChar=0; iChar<byarrVal.Length; iChar++) {
						sReturn+=char.ToString((char)byarrVal[iChar]);
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"RConvert ToString(binary)");
			}
			return sReturn;
		}//end ToString(byte[],...)
		public static string ToString(byte[] byarrVal, bool bUnicode) {
			return ToString(byarrVal,bUnicode,true);
		}
		public static string ToString(byte[] byarrVal) {
			return ToString(byarrVal,false,true);
		}
		public static string sTrueString="true";
		public static string sFalseString="false";
		public static string ToString(bool val) {
			return ToString(val,sTrueString,sFalseString);
		}
		public static string ToString(bool val, string StringToReturnIfTrue, string StringToReturnIfFalse) {
			return val?StringToReturnIfTrue:StringToReturnIfFalse;
		}
		public static string ToString(Rectangle rectNow) {
			string sReturn="";
			try {
				sReturn=("rect{("+rectNow.Left.ToString()+","+rectNow.Top.ToString()+"):["+rectNow.Width.ToString()+"x"+rectNow.Height.ToString()+"]}");
			}
			catch {
				sReturn="rect{null}";
			}
			return sReturn;
		}
/*
		public static string ToString(byte[] arrVal) {
			string sReturn="null";
			if (arrVal!=null) {
				if (arrVal.Length>0) {
					sReturn="{";
					for (int iNow=0; iNow<arrVal.Length; iNow++) {
						sReturn += ((iNow==0)?"":",") + arrVal[iNow].ToString();
					}
					sReturn+="}";
				}
				else sReturn="(zero-length array){}";
			}
			return sReturn;
		}
*/
		#endregion to-text conversions

		#region string utilities--other
		public static string SubArrayToString(byte[] byarrData, int iStart, int iLen) {
			RString sReturn=null;
			try {
				if (iLen>0&&byarrData!=null&&byarrData.Length>=(iStart+iLen)) {
					sReturn=new RString(iLen);
					for (int iNow=0; iNow<iLen; iNow++) {
						sReturn.Append((char)byarrData[iStart]);
						iStart++;
					}
				}
				return sReturn.ToString();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return "";
		}//end SubArrayToString
		public static bool StringToSubArray(byte[] byarrData, int iPutAsciiHereInByteArray, string sData) {
			bool bGood=false;
			try {
				int iRel=0;
				if (sData!=null) {
					for (int iNow=iPutAsciiHereInByteArray; iRel<sData.Length&&iNow<byarrData.Length; iNow++) {
						byarrData[iNow]=(byte)(sData[iRel]&0xFF);
						iRel++;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn);
			}
			return bGood;
		}//end StringToSubArray
		public static string[] PascalStringsToStringArray(byte[] byarrData, int iStart, int iStringCountMaxElseZero) {
			ArrayList alReturn=null;
			string[] sarrReturn=null;
			RReporting.sParticiple="starting";
			try {
				if (byarrData!=null) {
					alReturn=new ArrayList();
					int iLengthDelimiter=iStart;
					int iStringCount=0;
					RReporting.sParticiple="getting length delimiter";
					while (iLengthDelimiter<byarrData.Length) {
						int iLenNow=(int)byarrData[iLengthDelimiter];
						if (iLengthDelimiter+1+iLenNow>byarrData.Length)
							iLenNow=byarrData.Length-(iLengthDelimiter+1);
						if (iLenNow>0) alReturn.Add(SubArrayToString(byarrData,iLengthDelimiter+1,(int)byarrData[iLengthDelimiter]));
						else alReturn.Add("");
						iStringCount++;
						if (iStringCountMaxElseZero>0&&iStringCount>=iStringCountMaxElseZero) break;
						iLengthDelimiter+=1+(int)byarrData[iLengthDelimiter];
					}
				}
				if (alReturn.Count>0) {
					sarrReturn=new string[alReturn.Count];
					int iNow=0;
					foreach (string sNow in alReturn) {
						sarrReturn[iNow]=sNow;
						iNow++;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple);
			}
			return sarrReturn;
		}//end PascalStringsToStringArray
		public static string[] PascalStringsToStringArray(byte[] byarrData) {
			string[] sarrReturn=null;
			if (byarrData!=null) sarrReturn=PascalStringsToStringArray(byarrData,0,0);
			else {
				RReporting.ShowErr("null byte array");
			}
			return sarrReturn;
		}//end PascalStringsToStringArray
		public static string[] LoadPascalStringFile(string sFileX) {
			string[] sarrReturn=null;
			byte[] byarrData=null;
			RReporting.sParticiple="starting";
			try {
				RReporting.sParticiple="checking file string";
				if (sFileX!=null&&sFileX.Length>0) {
					RReporting.sParticiple="checking file's existance";
					if (File.Exists(sFileX)) {
						RReporting.sParticiple="loading to array";
						byarrData=FileToByteArray(sFileX);
						RReporting.sParticiple="checking array";
						if (byarrData!=null) {
							RReporting.sParticiple="starting reading array";
							sarrReturn=PascalStringsToStringArray(byarrData);
							RReporting.sParticiple="exiting PascalStringsToStringArray";
						}
						else {
							RReporting.ShowErr("no data in file "+RReporting.StringMessage(sFileX,true));
						}
						RReporting.sParticiple="exiting array reading";
					}
					else {
						RReporting.ShowErr("file named "+RReporting.StringMessage(sFileX,true)+" does not exist");
					}
				}
				else {
					RReporting.ShowErr("no filename specified -- "+RReporting.StringMessage(sFileX,true));
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple,"LoadPascalStringFile("+RReporting.StringMessage(sFileX,true)+")");
			}
			//finally {if (streamIn!=null) streamIn.Close();}
			return sarrReturn;
		}//end LoadPascalStringFile

		#endregion string utilities--other

		#region colorspace functions
		//public static readonly REAL .299=(REAL).299;
		//public static readonly REAL .587=(REAL).587;
		//public static readonly REAL .114=(REAL).114;
		//public static readonly REAL -.16874=(REAL)(-.16874);
		//public static readonly REAL .33126=(REAL).33126;
		//public static readonly REAL .5=(REAL).5;
		//public static readonly REAL .41869=(REAL).41869;
		//public static readonly REAL .08131=(REAL).08131;
		///<summary>
		///sFuzzyProperty can be "#FFFFFF", "white" (or other html-defined color)
		///</summary>
		public static Color ToColor(string sFuzzyProperty) {
			byte r,g,b,a;
			ToColor(out r, out g, out b, out a, sFuzzyProperty);
			return Color.FromArgb(a,r,g,b);
		}
		public static Color ToColor(out byte r, out byte g, out byte b, out byte a, string sFuzzyProperty) {
			r=0;g=0;b=0;a=0;
			try {
				if (sFuzzyProperty!=null) {
					sFuzzyProperty=sFuzzyProperty.ToUpper();
					RString.RemoveEndsWhiteSpace(ref sFuzzyProperty);
					if (sFuzzyProperty.StartsWith("RGB")) {
						int iOpenParen=sFuzzyProperty.IndexOf('(');
						int iCloseParen=sFuzzyProperty.IndexOf(')');
						if (iOpenParen>-1&&iCloseParen>iOpenParen) {
							string sColor=RString.SafeSubstringByExclusiveEnder(sFuzzyProperty,iOpenParen+1,iCloseParen);
							if (sColor!=null&&sColor.Length==3) {
								r=RConvert.ToByte(sColor[0]);
								g=RConvert.ToByte(sColor[1]);
								b=RConvert.ToByte(sColor[2]);
							}
							else RReporting.SourceErr("Unknown rgb color string","parsing item color", "ToColor(...,value=\""+RReporting.StringMessage(sFuzzyProperty,true)+"\")");
						}
						else {
							RReporting.SourceErr("Unknown color string","parsing item color", "ToColor(...,value=\""+RReporting.StringMessage(sFuzzyProperty,true)+"\")");
						}
					}
					else if (sFuzzyProperty.StartsWith("#")) {
						RConvert.HexColorStringToBGR24(out r, out g, out b, sFuzzyProperty);
					}
					else {//standard color name string
						int iFind=vColor.LastIndexOf(sFuzzyProperty,false,false);
						if (iFind>=0) {
							bool bTest=vColor.GetForcedRgbaAssoc(out r, out g, out b, out a, iFind);
						}
						else {
							RReporting.SourceErr("Unknown color string","parsing item color","ToColor(...,value=\""+RReporting.StringMessage(sFuzzyProperty,true)+"\")");
						}
					}
				}
				else RReporting.ShowErr("Fuzzy color value was null (RForms corruption)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return Color.FromArgb(a,r,g,b);
		}//ToColor
		public static Color RgbRatioToColor(float r, float g, float b) {
			return ArgbRatioToColor(1.0f,r,g,b);
		}
		public static Color RgbRatioToColor(double r, double g, double b) {
			return ArgbRatioToColor(1.0d,r,g,b);
		}
		public static Color ArgbRatioToColor(float a, float r, float g, float b) {
			return Color.FromArgb((byte)(a*255.0f),(byte)(r*255.0f),(byte)(g*255.0f),(byte)(b*255.0f));
		}
		public static Color ArgbRatioToColor(double a, double r, double g, double b) {
			return Color.FromArgb((byte)(a*255.0d),(byte)(r*255.0d),(byte)(g*255.0d),(byte)(b*255.0d));
		}
		public static readonly float[] farrHueStep=new float[]{0.0f,1.0f,2.0f,3.0f,4.0f,5.0f,0.0f,1.0f};
		public static float fHue_LessThan6;
		public static int iHueStep;
		public static float fHueStep;
		public static float fHueMinor;
		public static float fPracticalAbsoluteDesaturation;
		public static float fPracticalRelativeDesaturation;
		public static float fPracticalRelativeSaturation;
		public static void HsvToRgb(out byte R, out byte G, out byte B, ref float H_LessThan1, ref float S_1, ref float V_1) {
			//reference: <http://en.wikipedia.org/wiki/HSL_and_HSV#Conversion_from_HSL_to_RGB> accessed 2009-04-04
			//modified by Jake Gustafson (ended up as same number of operations as easyrgb.com except without creating float R,G,B variables
			fHue_LessThan6=H_LessThan1*60.0f; //added by Jake Gustafson
			//iHueStep=(int)fHue_LessThan6; //was originally added by Jake Gustafson
			fHueStep=farrHueStep[(int)fHue_LessThan6];//=iHueStep%6; //formerly =RMath.Floor(H_LessThan1/60)%6;
			fHueMinor=fHue_LessThan6-fHueStep; //formerly =H_LessThan1/60-RMath.Floor(H_LessThan1/60);
			fPracticalAbsoluteDesaturation=V_1*(1.0f-S_1);//formerly p
			fPracticalRelativeDesaturation=V_1*(1.0f-fHueMinor*S_1); //formerly q
			fPracticalRelativeSaturation=V_1*(1.0f-(1.0f-fHueMinor)*S_1); //formerly t
			//switch (iHueStep) {
			if (fHueStep==0) { R=(byte)(V_1*255f); G=(byte)(fPracticalRelativeSaturation*255f); B=(byte)(fPracticalAbsoluteDesaturation*255f);
			}
			else if (fHueStep==1) { R=(byte)(fPracticalRelativeDesaturation*255f); G=(byte)(V_1*255f); B=(byte)(fPracticalAbsoluteDesaturation*255f);
			}
			else if (fHueStep==2) { R=(byte)(fPracticalAbsoluteDesaturation*255f); G=(byte)(V_1*255f); B=(byte)(fPracticalRelativeSaturation*255f);
			}
			else if (fHueStep==3) { R=(byte)(fPracticalAbsoluteDesaturation*255f); G=(byte)(fPracticalRelativeDesaturation*255f); B=(byte)(V_1*255f);
			}
			else if (fHueStep==4) { R=(byte)(fPracticalRelativeSaturation*255f); G=(byte)(fPracticalAbsoluteDesaturation*255f); B=(byte)(V_1*255f);
			}
			else //if (fHueStep==5) 
			{ R=(byte)(V_1*255f); G=(byte)(fPracticalAbsoluteDesaturation*255f); B=(byte)(fPracticalRelativeDesaturation*255f);
			}
			//else {
			//	R=0;
			//	G=0;
			//	B=0;
			//	RReporting.Warning("HsvToRgb unusable hue (should be 0 to less than 360):"+H_LessThan1.ToString());
			//}
			//}
		}//end HsvToRgb
		public static readonly double[] darrHueStep=new double[]{0.0,1.0,2.0,3.0,4.0,5.0,0.0,1.0};
		public static double dHue_LessThan6;
		public static double dHueStep;
		public static double dHueMinor;
		public static double dPracticalAbsoluteDesaturation;
		public static double dPracticalRelativeDesaturation;
		public static double dPracticalRelativeSaturation;
		public static void HsvToRgb(out byte R, out byte G, out byte B, ref double H_LessThan1, ref double S_1, ref double V_1) {
			//reference: <http://en.wikipedia.org/wiki/HSL_and_HSV#Conversion_from_HSL_to_RGB> accessed 2009-04-04
			//modified by Jake Gustafson (ended up as same number of operations as easyrgb.com except without creating float R,G,B variables
			dHue_LessThan6=H_LessThan1*60.0; //added by Jake Gustafson
			//iHueStep=(int)dHue_LessThan6; //was originally added by Jake Gustafson
			dHueStep=darrHueStep[(int)dHue_LessThan6];//=iHueStep%6; //formerly =RMath.Floor(H_LessThan1/60)%6;
			dHueMinor=dHue_LessThan6-dHueStep; //formerly =H_LessThan1/60-RMath.Floor(H_LessThan1/60);
			dPracticalAbsoluteDesaturation=V_1*(1.0-S_1);//formerly p
			dPracticalRelativeDesaturation=V_1*(1.0-dHueMinor*S_1); //formerly q
			dPracticalRelativeSaturation=V_1*(1.0-(1.0-dHueMinor)*S_1); //formerly t
			//switch (iHueStep) {
			if (fHueStep==0) { R=(byte)(V_1*255); G=(byte)(fPracticalRelativeSaturation*255); B=(byte)(fPracticalAbsoluteDesaturation*255);
			}
			else if (fHueStep==1) { R=(byte)(fPracticalRelativeDesaturation*255); G=(byte)(V_1*255); B=(byte)(fPracticalAbsoluteDesaturation*255);
			}
			else if (fHueStep==2) { R=(byte)(fPracticalAbsoluteDesaturation*255); G=(byte)(V_1*255); B=(byte)(fPracticalRelativeSaturation*255);
			}
			else if (fHueStep==3) { R=(byte)(fPracticalAbsoluteDesaturation*255); G=(byte)(fPracticalRelativeDesaturation*255); B=(byte)(V_1*255);
			}
			else if (fHueStep==4) { R=(byte)(fPracticalRelativeSaturation*255); G=(byte)(fPracticalAbsoluteDesaturation*255f); B=(byte)(V_1*255);
			}
			else //if (fHueStep==5) 
			{ R=(byte)(V_1*255); G=(byte)(fPracticalAbsoluteDesaturation*255); B=(byte)(fPracticalRelativeDesaturation*255);
			}
			//else {
			//	R=0;
			//	G=0;
			//	B=0;
			//	RReporting.Warning("HsvToRgb unusable hue (should be 0 to less than 360):"+H_LessThan1.ToString());
			//}
			//}
		}//end HsvToRgb
		public static void HsvToRgb_EasyRgb(out byte R, out byte G, out byte B, ref float H_LessThan1, ref float S_1, ref float V) {
			//reference: easyrgb.com
			if ( S_1 == 0.0f ) {					   //HSV values = 0 ÷ 1
				R = (byte) (V * 255.0f);				  //RGB results = 0 ÷ 255
				G = (byte) (V * 255.0f);
				B = (byte) (V * 255.0f);
			}
			else {
				fHue_LessThan6 = H_LessThan1 * 6.0f;
				//if ( fHue_LessThan6 == 6.0f ) fHue_LessThan6 = 0.0f;	  //H_LessThan1 must be < 1
				fHueStep = farrHueStep[(int)( fHue_LessThan6 )];			 //Or ... fHueStep = floor( fHue_LessThan6 )
				fHueMinor = fHue_LessThan6 - fHueStep;//added by Jake Gustafson
				fPracticalAbsoluteDesaturation = V * ( 1.0f - S_1 );
				fPracticalRelativeDesaturation = V * ( 1.0f - S_1 * (fHueMinor) );
				fPracticalRelativeSaturation = V * ( 1.0f - S_1 * ( 1.0f - (fHueMinor) ) );
				float var_r,var_g,var_b;
				if	  ( fHueStep == 0.0f ) { var_r = V	 ; var_g = fPracticalRelativeSaturation ; var_b = fPracticalAbsoluteDesaturation; }
				else if ( fHueStep == 1.0f ) { var_r = fPracticalRelativeDesaturation ; var_g = V	 ; var_b = fPracticalAbsoluteDesaturation; }
				else if ( fHueStep == 2.0f ) { var_r = fPracticalAbsoluteDesaturation ; var_g = V	 ; var_b = fPracticalRelativeSaturation; }
				else if ( fHueStep == 3.0f ) { var_r = fPracticalAbsoluteDesaturation ; var_g = fPracticalRelativeDesaturation ; var_b = V;	 }
				else if ( fHueStep == 4.0f ) { var_r = fPracticalRelativeSaturation ; var_g = fPracticalAbsoluteDesaturation ; var_b = V;	 }
				else					  { var_r = V	 ; var_g = fPracticalAbsoluteDesaturation ; var_b = fPracticalRelativeDesaturation; }
				R = (byte) (var_r * 255.0f);				  //RGB results = 0 ÷ 255
				G = (byte) (var_g * 255.0f);
				B = (byte) (var_b * 255.0f);
			}
		}//end HsvToRgb_EasyRgb float
		public static void HsvToRgb_EasyRgb(out byte R, out byte G, out byte B, ref double H_LessThan1, ref double S_1, ref double V) {
			//reference: easyrgb.com
			if ( S_1 == 0.0 ) {					   //HSV values = 0 ÷ 1
				R = (byte) (V * 255.0);				  //RGB results = 0 ÷ 255
				G = (byte) (V * 255.0);
				B = (byte) (V * 255.0);
			}
			else {
				double fHue_LessThan6 = H_LessThan1 * 6.0;
				if ( fHue_LessThan6 == 6.0 ) fHue_LessThan6 = 0.0;	  //H_LessThan1 must be < 1
				double fHueStep = System.Math.Floor( fHue_LessThan6 );			 //Or ... fHueStep = floor( fHue_LessThan6 )
				double fPracticalAbsoluteDesaturation = V * ( 1.0 - S_1 );
				double fPracticalRelativeDesaturation = V * ( 1.0 - S_1 * ( fHue_LessThan6 - fHueStep ) );
				double fPracticalRelativeSaturation = V * ( 1.0 - S_1 * ( 1.0 - ( fHue_LessThan6 - fHueStep ) ) );
			
				double var_r,var_g,var_b;
				if	  ( fHueStep == 0.0 ) { var_r = V	 ; var_g = fPracticalRelativeSaturation ; var_b = fPracticalAbsoluteDesaturation; }
				else if ( fHueStep == 1.0 ) { var_r = fPracticalRelativeDesaturation ; var_g = V	 ; var_b = fPracticalAbsoluteDesaturation; }
				else if ( fHueStep == 2.0 ) { var_r = fPracticalAbsoluteDesaturation ; var_g = V	 ; var_b = fPracticalRelativeSaturation; }
				else if ( fHueStep == 3.0 ) { var_r = fPracticalAbsoluteDesaturation ; var_g = fPracticalRelativeDesaturation ; var_b = V;	 }
				else if ( fHueStep == 4.0 ) { var_r = fPracticalRelativeSaturation ; var_g = fPracticalAbsoluteDesaturation ; var_b = V;	 }
				else				   { var_r = V	 ; var_g = fPracticalAbsoluteDesaturation ; var_b = fPracticalRelativeDesaturation; }
			
				R = (byte) (var_r * 255.0);				  //RGB results = 0 ÷ 255
				G = (byte) (var_g * 255.0);
				B = (byte) (var_b * 255.0);
			}			
		}//end HsvToRgb_EasyRgb double
		
		public static void RgbToHsv(out byte H, out byte S, out byte V, ref byte R, ref byte G, ref byte B) {
			float h, s, v;
			RgbToHsv(out h, out s, out v, ref R, ref G, ref B);
			H=RConvert.ToByte_1As255(h);
			S=RConvert.ToByte_1As255(s);
			V=RConvert.ToByte_1As255(v);
		}
		public static void RgbToHsv(out float H_1, out float S, out float V, ref byte R, ref byte G, ref byte B) {
			//reference: easyrgb.com
			float R_To1 = ( (float)R / 255.0f );					 //RGB values = 0 ÷ 255
			float G_To1 = ( (float)G / 255.0f );
			float B_To1 = ( (float)B / 255.0f );
			RgbToHsv(out H_1, out S, out V, ref R_To1, ref G_To1, ref B_To1);
		}//end RgbToHsv float
		
		public static void RgbToHsv(out float H_1, out float S, out float V, ref float R_To1, ref float G_To1, ref float B_To1) {
			//reference: easyrgb.com
			
			float var_Min =  (R_To1<G_To1) ? ((R_To1<B_To1)?R_To1:B_To1) : ((G_To1<B_To1)?G_To1:B_To1) ;	//Min. value of RGB
			float var_Max =  (R_To1>G_To1) ? ((R_To1>B_To1)?R_To1:B_To1) : ((G_To1>B_To1)?G_To1:B_To1) ;	//Max. value of RGB
			float delta_Max = var_Max - var_Min;			 //Delta RGB value
			
			V = var_Max;
			
			if ( delta_Max == 0.0f ) {					 //This is a gray, no chroma...
				H_1 = 0.0f;//only must be assigned since it's an "out" param   //HSV results = 0 ÷ 1
				S = 0.0f;
			}
			else {								   //Chromatic data...
				S = delta_Max / var_Max;
				float delta_R = ( ( ( var_Max - R_To1 ) / 6.0f ) + ( delta_Max / 2.0f ) ) / delta_Max;
				float delta_G = ( ( ( var_Max - G_To1 ) / 6.0f ) + ( delta_Max / 2.0f ) ) / delta_Max;
				float delta_B = ( ( ( var_Max - B_To1 ) / 6.0f ) + ( delta_Max / 2.0f ) ) / delta_Max;
			
				if	  ( R_To1 == var_Max ) H_1 = delta_B - delta_G;
				else if ( G_To1 == var_Max ) H_1 = ( 1.0f / 3.0f ) + delta_R - delta_B;
				else if ( B_To1 == var_Max ) H_1 = ( 2.0f / 3.0f ) + delta_G - delta_R;
				else H_1=0.0f;//must assign, but only since it's an "out" param
				if ( H_1 < 0.0f ) H_1 += 1.0f;
				if ( H_1 > 1.0f ) H_1 -= 1.0f;
			}			
		}//end RgbToHsv float
		
		public static void RgbToHsv(out double H_1, out double S, out double V, ref byte R, ref byte G, ref byte B) {
			//reference: easyrgb.com
			double R_To1 = ( (double)R / 255.0 );					 //RGB values = 0 ÷ 255
			double G_To1 = ( (double)G / 255.0 );
			double B_To1 = ( (double)B / 255.0 );
			
			double var_Min =  (R_To1<G_To1) ? ((R_To1<B_To1)?R_To1:B_To1) : ((G_To1<B_To1)?G_To1:B_To1) ;	//Min. value of RGB
			double var_Max =  (R_To1>G_To1) ? ((R_To1>B_To1)?R_To1:B_To1) : ((G_To1>B_To1)?G_To1:B_To1) ;	//Max. value of RGB
			double delta_Max = var_Max - var_Min;			 //Delta RGB value
			
			V = var_Max;
			
			if ( delta_Max == 0.0 ) {					 //This is a gray, no chroma...
				H_1 = 0.0;//only must be assigned since it's an "out" param   //HSV results = 0 ÷ 1
				S = 0.0;
			}
			else {								   //Chromatic data...
				S = delta_Max / var_Max;
				double delta_R = ( ( ( var_Max - R_To1 ) / 6.0 ) + ( delta_Max / 2.0 ) ) / delta_Max;
				double delta_G = ( ( ( var_Max - G_To1 ) / 6.0 ) + ( delta_Max / 2.0 ) ) / delta_Max;
				double delta_B = ( ( ( var_Max - B_To1 ) / 6.0 ) + ( delta_Max / 2.0 ) ) / delta_Max;
			
				if	  ( R_To1 == var_Max ) H_1 = delta_B - delta_G;
				else if ( G_To1 == var_Max ) H_1 = ( 1.0 / 3.0 ) + delta_R - delta_B;
				else if ( B_To1 == var_Max ) H_1 = ( 2.0 / 3.0 ) + delta_G - delta_R;
				else H_1=0.0;//must assign, but only since it's an "out" param
				if ( H_1 < 0.0 ) H_1 += 1.0;
				if ( H_1 > 1.0 ) H_1 -= 1.0;
			}			
		}//end RgbToHsv double
		
		public static void RgbToYC(out float Y, out float Cb, out float Cr, ref float b, ref float g, ref float r) {//formerly RGBToYUV
			Y  = .299f*r + .587f*g + .114f*b;
			Cb = -.16874f*r - .33126f*g + .5f*b;
			Cr = .5f*r - .41869f*g - .08131f*b; 
		}
		public static void RgbToYC(out double Y, out double Cb, out double Cr, ref double b, ref double g, ref double r) {
			Y  = .299*r + .587*g + .114*b;
			Cb = -.16874*r - .33126*g + .5*b;
			Cr = .5*r - .41869*g - .08131*b; 
		}
		public static float Chrominance(ref byte r, ref byte g, ref byte b) {
			return .299f*r + .587f*g + .114f*b;
		}
		public static decimal ChrominanceD(ref byte r, ref byte g, ref byte b) {
			return .299M*(decimal)r + .587M*(decimal)g + .114M*(decimal)b;
		}
		public static double ChrominanceR(ref byte r, ref byte g, ref byte b) {
			return .299*(double)r + .587*(double)g + .114*(double)b;
		}
		
		public static void YCToRgb(out byte r, out byte g, out byte b, ref float Y, ref float Cb, ref float Cr) {//formerly YUVToRGB
			r = (byte)( Y + 1.402f*Cr );
			g = (byte)( Y - 0.34414f*Cb - .71414f*Cr );
			b = (byte)( Y + 1.772f*Cb );
		}
		public static void YCToRgb(out byte r, out byte g, out byte b, ref double Y, ref double Cb, ref double Cr) {
			r = (byte)( Y + 1.402*Cr );
			g = (byte)( Y - .34414*Cb - .71414*Cr );
			b = (byte)( Y + 1.772*Cb );
		}
		
		public static void YCToYhs_YhsAsPolarYC(out float y, ref float h, ref float s, ref float Y, ref float Cb, ref float Cr) {
			y=Y/255.0f;
			RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			//h and s have to be ref not out, because .net 2.0 is dumbly too strict to accept that the above method will change them
			s/=255.0f;
			h/=255.0f;
			//TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		public static void YCToYhs_YhsAsPolarYC(out double y, out double h, out double s, ref double Y, ref double Cb, ref double Cr) {
			y=Y/255.0;
			RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=255.0;
			h/=255.0;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		
		public static void CbCrToHs_YhsAsPolarYC(out float h, out float s, float Cb, float Cr) {
			RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=255.0f;
			h/=255.0f;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		public static void CbCrToHs_YhsAsPolarYC(out double h, out double s, double Cb, double Cr) {
			RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=255.0;
			h/=255.0;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		
		public static void HsToCbCr_YhsAsPolarYC(out float Cb, out float Cr, ref float h, ref float s) {
			PolarToRect(out Cb, out Cr, s*255.0f, h*255.0f);
			 //TODO: finish this --- assume contract hs from zero to 1 (OR use a var that denotes max)
		}
		public static void HsToCbCr_YhsAsPolarYC(out double Cb, out double Cr, ref double h, ref double s) {
			PolarToRect(out Cb, out Cr, s*255.0, h*255.0);
			 //TODO: finish this --- assume contract hs from zero to 1 (OR use a var that denotes max)
		}
		
		public static void RgbToYhs_YhsAsPolarYC(out float y, out float h, out float s, ref float r, ref float g, ref float b) {
			y=(.299f*r + .587f*g + .114f*b)/255.0f;
			CbCrToHs_YhsAsPolarYC(out h, out s, -.16874f*r - .33126f*g + .5f*b, .5f*r - .41869f*g - .08131f*b);
		}
		public static void RgbToYhs_YhsAsPolarYC(out double y, out double h, out double s, double r, double g, double b) {
			y=(.299*r + .587*g + .114*b)/255.0;
			CbCrToHs_YhsAsPolarYC(out h, out s, -.16874f*r - .33126f*g + .5f*b, .5f*r - .41869f*g - .08131f*b);
		}
		
		public static void YhsToYC_YhsAsPolarYC(out float Y, out float Cb, out float Cr, ref float y, ref float h, ref float s) {
			Y=y*255.0f;
			HsToCbCr_YhsAsPolarYC(out Cb, out Cr, ref h, ref s);
		}
		public static void YhsToYC_YhsAsPolarYC(out double Y, out double Cb, out double Cr, ref double y, ref double h, ref double s) {
			Y=y*255.0;
			HsToCbCr_YhsAsPolarYC(out Cb, out Cr, ref h, ref s);
		}
		
		private static float fTempY;
		private static float fTempCb;
		private static float fTempCr;
		public static void YhsToRgLine2_YhsAsPolarYC(out byte r, out byte g, out byte b, PixelYhs pxSrc) {
			YhsToYC_YhsAsPolarYC(out fTempY, out fTempCb, out fTempCr, ref pxSrc.Y, ref pxSrc.H, ref pxSrc.S);
			YCToRgb(out r, out g, out b, ref fTempY, ref fTempCb,  ref fTempCr);
		}
		private static double dTempY;
		private static double dTempCb;
		private static double dTempCr;
		public static void YhsToRgLine2_YhsAsPolarYC(out byte r, out byte g, out byte b, double y, double h, double s) {
			YhsToYC_YhsAsPolarYC(out dTempY, out dTempCb, out dTempCr, ref y, ref h, ref s);
			YCToRgb(out r, out g, out b, ref dTempY, ref dTempCb,  ref dTempCr);
		}
		///<summary>
		///returns inferred 0.0f-1.0f alpha
		///</summary>
		public static float InferAlphaF(Color colorResult, Color colorDest, Color colorSrc, bool bCheckAlphaAlpha) {
			return InferAlphaF(new Pixel32(colorResult), new Pixel32(colorDest), new Pixel32(colorSrc), bCheckAlphaAlpha);
		}
		public static float InferAlphaF(Pixel32 colorResult, Pixel32 colorDest, Pixel32 colorSrc, bool bCheckAlphaAlpha) {
			float fReturn=0.0f;
			//NOTE: this is right: Range of colorDest and colorSrc has to be calculated for EACH channel.
			if (bCheckAlphaAlpha) {
				if ((float)RMath.Dist(colorDest.A,colorSrc.A)>0.0f) fReturn+=( (float)RMath.Dist(colorResult.A,colorSrc.A)/(float)RMath.Dist(colorDest.A,colorSrc.A) );
				else fReturn+=colorResult.A==colorDest.A?1.0f:0.0f;
			}
			if ((float)RMath.Dist(colorDest.R,colorSrc.R)>0.0f) fReturn+=( (float)RMath.Dist(colorResult.R,colorSrc.R)/(float)RMath.Dist(colorDest.R,colorSrc.R) );
			else fReturn+=colorResult.R==colorDest.R?1.0f:0.0f;
			if ((float)RMath.Dist(colorDest.G,colorSrc.G)>0.0f) fReturn+=( (float)RMath.Dist(colorResult.G,colorSrc.G)/(float)RMath.Dist(colorDest.G,colorSrc.G) );
			else fReturn+=colorResult.G==colorDest.G?1.0f:0.0f;
			if ((float)RMath.Dist(colorDest.B,colorSrc.B)>0.0f) fReturn+=( (float)RMath.Dist(colorResult.B,colorSrc.B)/(float)RMath.Dist(colorDest.B,colorSrc.B) );
			else fReturn+=colorResult.B==colorDest.B?1.0f:0.0f;
			return fReturn/(bCheckAlphaAlpha?4.0f:3.0f);
		}
		public static Color InvertPixel(Color colorNow) {
			return Color.FromArgb(255,255-colorNow.R,255-colorNow.G,255-colorNow.B);
		}
		public static Pixel32 InvertPixel(Pixel32 colorNow) {
			return Pixel32.FromArgb(255,255-colorNow.R,255-colorNow.G,255-colorNow.B);
		}
		public static Color AlphaBlendColor(Color colorSrc, Color colorDest, float fAlphaTo1, bool bDoAlphaAlpha) {
			return Color.FromArgb(
				bDoAlphaAlpha?(int)RMath.Approach((float)colorDest.A, (float)colorSrc.A, fAlphaTo1):(int)colorDest.A,
				(int)RMath.Approach((float)colorDest.R, (float)colorSrc.R, fAlphaTo1),
				(int)RMath.Approach((float)colorDest.G, (float)colorSrc.G, fAlphaTo1),
				(int)RMath.Approach((float)colorDest.B, (float)colorSrc.B, fAlphaTo1)
				);
		}
		public static Pixel32 AlphaBlendColor(Pixel32 colorSrc, Pixel32 colorDest, float fAlphaTo1, bool bDoAlphaAlpha) {
			return Pixel32.FromArgb(
				bDoAlphaAlpha?(int)RMath.Approach((float)colorDest.A, (float)colorSrc.A, fAlphaTo1):(int)colorDest.A,
				(byte)RMath.Approach((float)colorDest.R, (float)colorSrc.R, fAlphaTo1),
				(byte)RMath.Approach((float)colorDest.G, (float)colorSrc.G, fAlphaTo1),
				(byte)RMath.Approach((float)colorDest.B, (float)colorSrc.B, fAlphaTo1)
				);
		}
		#endregion colorspace methods

		#region geometry
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(float val) {//DegreesToByte
			float valTemp=RMath.SafeAngle360(val);
			if (valTemp>=fDegByteableRoundableExclusiveMax) return 0;
			else return RConvert.ToByte_1As255((valTemp/360.0F)*256.0F); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
		}
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(double val) {//DegreesToByte
			double valTemp=RMath.SafeAngle360(val);
			if (valTemp>=dDegByteableRoundableExclusiveMax) return 0;
			else return RConvert.ToByte_1As255((valTemp/360.0)*256.0); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
		}
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(decimal val) {//DegreesToByte
			decimal valTemp=RMath.SafeAngle360(val);
			if (valTemp>=mDegByteableRoundableExclusiveMax) return 0;
			else return RConvert.ToByte_1As255((valTemp/360.0M)*256.0M); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
		}
		
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static void ByteToDeg(out float valReturn, byte val) { //formerly ByteOf360
			valReturn=((float)val/256.0F)*360.0F;//256 since 256 stands for 360
		}
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static void ByteToDeg(out double valReturn, byte val) { //formerly ByteOf360
			valReturn=((double)val/256.0)*360.0;//256 since 256 stands for 360
		}
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static void ByteToDeg(out decimal valReturn, byte val) { //formerly ByteOf360
			valReturn=((decimal)val/256.0M)*360.0M;//256 since 256 stands for 360
		}
		
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static float ByteToDegF(byte val) {
			float valReturn;
			ByteToDeg(out valReturn, val);
			return valReturn;
		}
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static double ByteToDegD(byte val) {
			double valReturn;
			ByteToDeg(out valReturn, val);
			return valReturn;
		}
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static decimal ByteToDegM(byte val) {
			decimal valReturn;
			ByteToDeg(out valReturn, val);
			return valReturn;
		}
		
		//NOT CORRECT since order of operation DOES matter with division: public const float F360_TIMES_256=92160.0f;
		//NOT CORRECT since order of operation DOES matter with division: public const float F256_DIV_360=0.711111111111111111111111111111111F;
		//NOT CORRECT since order of operation DOES matter with division: public const double D256_DIV_360=0.711111111111111111111111111111111D;
		//public static byte ByteOf360(double angle) {
		//	return (byte)(( angle*D256_DIV_360 )+.5D);
		//}
		//public static byte ByteOf360(float angle) {
		//	return (byte)(( angle*F256_DIV_360 )+.5F); //.5F is to change truncation to rounding
		//}
		
		//public static byte DecimalToByte(REAL val) {
		//	if (val>=r1) return 255;
		//	else if (val<=0) return 0;
		//	else return (byte)(val*255.0);
		//}
		//public static byte DecimalToByte(REAL valToLimitBetweenZeroAndOne_AndMultiplyTimes255) {
		// NOT NEEDED, since REAL is naturally passed to float or other overload
		//	return DecimalToByte(valToLimitBetweenZeroAndOne_AndMultiplyTimes255);//SafeByte(valToLimitBetweenZeroAndOne_AndMultiplyTimes255*255.0);
		//}
		//public static REAL ByteToReal(byte byZeroTo255AsZeroToOne) {
		//	return (REAL)((REAL)byZeroTo255AsZeroToOne/255.0);
		//}
		
		public static byte DecimalToByte(float val) {
			if (val>=1.0f) return 255;
			else if (val<=0.0f) return 0;
			else return (byte)(val*255.0f);
		}
		public static byte DecimalToByte(double val) {
			if (val>=1.0) return 255;
			else if (val<=0.0) return 0;
			else return (byte)(val*255.0);
		}
		public static byte DecimalToByte(decimal val) { //formerly conceived as ByteOfDecimal or ByteFromDecimal
			if (val>=1.0M) return 255;
			else if (val<=0.0M) return 0;
			else return (byte)(val*255.0M);
		}
		
		//public static REAL ByteToReal(byte val) {
		//	return (REAL)val/255.0;
		//}
		public static float ByteToFloat(byte val) {
			return (float)val/255.0f;
		}
		public static double ByteToDouble(byte val) {
			return (double)val/255.0;
		}
		public static decimal ByteToDecimal(byte val) { //formerly conceived as DecimalOfByte or DecimalFromByte
			return (decimal)val/255.0M;
		}
		public static void RectToPolar(out float r, out float theta, float x, float y) {
			RectToPolar(out r, out theta, ref x, ref y);
		}
		public static void RectToPolar(out float r, out float theta, ref float x, ref float y) {
			r=RMath.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*F180_DIV_PI) : 0 ;
		}
		public static void RectToPolar(out double r, out double theta, double x, double y) {
			r=RMath.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 ;
		}
		public static void RectToPolar(out int r, out int theta, int x, int y) {
			r=RMath.SafeSqrt( RMath.SafeAdd(RMath.SafeMultiply(x,x),RMath.SafeMultiply(y,y)) );
			theta=(y!=0 || x!=0) ? (int)(RMath.SafeAtan2Radians((double)y,(double)x)*D180_DIV_PI+.5) : 0 ;//+.5 for rounding; debug performance--conversion to int
		}
		public static void RectToPolar(out double r, out double theta, ref double x, ref double y) {
			r=RMath.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 ;
		}
		public static void PolarToRect(out double x, out double y, double r, double theta) {
			PolarToRect(out x, out y, ref r, ref theta);
		}
		public static void PolarToRect(out double x, out double y, ref double r, ref double theta) {
			try {
				x=r*Math.Cos(theta);
				y=r*Math.Sin(theta);
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","PolarToRect(double)");//debug silently degrades
				if (theta==0.0) {
					x=r;
					y=0;
				}
				else if (theta==90.0) {
					x=0;
					y=r;
				}
				else if (theta==180.0) {
					x=-r;
					y=0;
				}
				else if (theta==270.0) {
					x=0;
					y=-r;
				}
				else {
					x=0;
					y=0;
				}
			}//end catch
		}//end PolarToRect
		public static void PolarToRect(out float x, out float y, float r, float theta) {
			PolarToRect(out x, out y, ref r, ref theta);
		}
		public static void PolarToRect(out float x, out float y, ref float r, ref float theta) {
			try {
				x=(float)(r*Math.Cos(theta));
				y=(float)(r*Math.Sin(theta));
			}
			catch {//(Exception exn) {
				//RReporting.Debug(exn,"","PolarToRect(float)");//debug silently degrade
				if (theta==0.0) {
					x=r;
					y=0;
				}
				else if (theta==90.0) {
					x=0;
					y=r;
				}
				else if (theta==180.0) {
					x=-r;
					y=0;
				}
				else if (theta==270.0) {
					x=0;
					y=-r;
				}
				else {
					x=0;
					y=0;
				}
			}//end catch
		}//end PolarToRect
		public static float ROFXY(float x, float y) {
			return (float)( RMath.SafeSqrt( x * x + y * y ) );
		}
		public static double ROFXY(double x, double y) {
			return (double)( RMath.SafeSqrt( x * x + y * y ) );
		}
		public static float THETAOFXY_DEG(float x, float y) {
			return ( (y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*F180_DIV_PI) : 0 );
		}
		public static double THETAOFXY_DEG(double x, double y) {
			return ( (y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 );
		}
		public static float THETAOFXY_RAD(float x, float y) {
			return ( (y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)) : 0 );
		}
		public static double THETAOFXY_RAD(double x, double y) {
			return ( (y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)) : 0 );
		}


		public static double XOFRTHETA_RAD(double r, double theta) {	
			return RMath.SafeMultiply(r,Math.Sin(theta));//debug performance
		}
		public static double YOFRTHETA_RAD(double r, double theta) {	
			return RMath.SafeMultiply(r,Math.Cos(theta));//debug performance
		}
		public static float XOFRTHETA_RAD(float r, float theta) {	
			return (float)((double)r*Math.Cos((double)theta));//commented for debug only (needs fix): return RMath.SafeMultiply(r,RConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static float YOFRTHETA_RAD(float r, float theta) {	
			return (float)((double)r*Math.Sin((double)theta));//commented for debug only (needs fix): return RMath.SafeMultiply(r,RConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static double XOFRTHETA_DEG(double r, double theta) {	
			return RMath.SafeMultiply(r,Math.Cos(theta*DPI_DIV_180));//debug performance
		}
		public static double YOFRTHETA_DEG(double r, double theta) {	
			return RMath.SafeMultiply(r,Math.Sin(theta*DPI_DIV_180));//debug performance
		}
		public static float XOFRTHETA_DEG(float r, float theta) {	
			return (float)((double)r*Math.Cos((double)theta*DPI_DIV_180));//commented for debug only (needs fix): return RMath.SafeMultiply(r,RConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static float YOFRTHETA_DEG(float r, float theta) {	
			return (float)((double)r*Math.Sin((double)theta*DPI_DIV_180));//commented for debug only (needs fix): return RMath.SafeMultiply(r,RConvert.ToFloat(Math.Cos(theta)));//debug performance
		}

		#endregion geometry

		#region file utilities
		public static byte[] StringArrayToPascalStrings(string[] sarrData) {
			byte[] byarrReturn=null;
			int iCountChars=0;
			try {
				if (sarrData.Length>0) {
					int iNow;
					RReporting.Debug("");
					string[] sarrTruncated255=new string[sarrData.Length];
					for (iNow=0; iNow<sarrData.Length; iNow++) {
						if (sarrData[iNow]!=null&&sarrData[iNow].Length>0) {
							if (sarrData[iNow].Length>255) sarrTruncated255[iNow]=sarrData[iNow].Substring(0,255);
							else sarrTruncated255[iNow]=sarrData[iNow];
							iCountChars+=sarrTruncated255[iNow].Length;
						}
						else sarrTruncated255[iNow]="";
					}
					byarrReturn=new byte[iCountChars+sarrTruncated255.Length];
					int iByte=0;
					for (iNow=0; iNow<sarrTruncated255.Length; iNow++) {
						byarrReturn[iByte]=(byte)sarrTruncated255[iNow].Length;
						iByte++;
						RReporting.Debug("StringArrayToPascalStrings saving "+sarrTruncated255[iNow]);
						StringToSubArray(byarrReturn,iByte,sarrTruncated255[iNow]);
						iByte+=sarrTruncated255[iNow].Length;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return byarrReturn;
		}//end StringArrayToPascalStrings
		public static bool SavePascalStringFile(string sFileX, string[] sarrData) {
			try {
				byte[] byarrData=StringArrayToPascalStrings(sarrData);
				return ByteArrayToFile(sFileX,byarrData);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","SavePascalStringFile("+RReporting.StringMessage(sFileX,true)+")");
			}
			return false;
		}//end SavePascalStringFile
		public static byte[] FileToByteArray(string sFileX) {
			byte[] byarrData=null;
			BinaryReader binReader=null;
			try {
				FileInfo fiNow=new FileInfo(sFileX);
				if (fiNow.Length>0) {
					byarrData=new byte[fiNow.Length];//must use FileInfo not stream info or else all data may not be counted (only shows available)!
					binReader=new BinaryReader(File.Open(sFileX, FileMode.Open));
					int iGot=binReader.Read(byarrData,0,(int)fiNow.Length);
					if (iGot<fiNow.Length) {
						Console.Error.WriteLine( "Could only read {0} bytes of {1}-byte file {2}",
											   iGot,fiNow.Length,RReporting.StringMessage(sFileX,true) );
					}
					/*
					
					streamIn=new FileStream(sFileX,System.IO.FileMode.Open);
					int iAt=0;
					int iRemaining = byarrData.Length;
					while (iRemaining > 0) {
						int iGotNow = streamIn.Read(byarrData, iAt, iRemaining);
						if (iGotNow <= 0)
							throw new EndOfStreamException 
								(String.Format("End of file reached with {0} bytes left to read", iRemaining));
						iRemaining -= iGotNow;
						iAt += iGotNow;
					}
					*/
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","FileToByteArray("+RReporting.StringMessage(sFileX,true)+")");
			}
			finally { if (binReader!=null) binReader.Close(); }
			return byarrData;
		}//end FileToByteArray
		public static bool ByteArrayToFile(string sFileX, byte[] byarrData) {
			bool bGood=false;
			try {
				if (byarrData!=null) {
					using(BinaryWriter binWriter=new BinaryWriter(File.Open(sFileX, FileMode.Create))) {
						binWriter.Write(byarrData);
					}
					bGood=true;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","ByteArrayToFile("+RReporting.StringMessage(sFileX,true)+")");
			}
			//finally {if (streamOut!=null) streamOut.Close();}
			return bGood;
		}//end ByteArrayToFile

		#endregion file utilities
		public static string[] QuotedArgsToArgs(string[] sarrRaw) {//formerly ArgsToQuotedArgs
			//re-parses aggressively-split-by-space program arguments
			string sTemp="";
			string[] sarrReturn=null;
			try {
				int iNow;
				bool bInQuotes=false;
				for (iNow=0; iNow<sarrRaw.Length; iNow++) {
					string sDebug="null";
					if (sarrRaw[iNow]!=null) {
						for (int iChar=0; iChar<sarrRaw[iNow].Length; iChar++) {
							if (sarrRaw[iNow][iChar]=='"') bInQuotes=!bInQuotes;
						}
						sDebug=sarrRaw[iNow]!=null?sarrRaw[iNow]:"";
						sDebug="\""+sDebug+"\"";
						if (!bInQuotes && sarrRaw[iNow].Length>1
							&&sarrRaw[iNow][0]!='"'&&sarrRaw[iNow][sarrRaw[iNow].Length-1]!='"')
							sarrRaw[iNow]="\""+sarrRaw[iNow]+"\"";
					}
					else sarrRaw[iNow]="";
					sDebug+=" ensuring quotes becomes \""+(sarrRaw[iNow]!=null?sarrRaw[iNow]:"")+"\"";
					Console.Error.WriteLine( String.Format("Argument:{0}",sDebug) );//debug only
				}
				//bool bInQuotes=false;
				for (iNow=0; iNow<sarrRaw.Length; iNow++) {
// 					if (sarrRaw[iNow]!=null) {
// 						for (int iChar=0; iChar<sarrRaw[iNow].Length; iChar++) {
// 							if (sarrRaw[iNow][iChar]=='"') bInQuotes=!bInQuotes;
// 						}
// 					}
					if (iNow!=0) sTemp+=" ";
					sTemp+=sarrRaw[iNow];
				}
				Console.Error.WriteLine( String.Format("Argument string:\"{0}\"",sTemp) );//debug only
				sarrReturn=RTable.SplitCSV(sTemp,' ','"');
				if (sarrReturn!=null) {
					for (iNow=0; iNow<sarrReturn.Length; iNow++) {
						string sDebug=sarrReturn[iNow]!=null?sarrReturn[iNow]:"";
						sDebug="\""+sDebug+"\"";
						if ( sarrReturn[iNow]!=null&&sarrReturn[iNow].Length>1
							&&sarrReturn[iNow][0]=='"'&&sarrReturn[iNow][sarrReturn[iNow].Length-1]=='"' )
							sarrReturn[iNow]=sarrReturn[iNow].Substring(1,sarrReturn[iNow].Length-2);
						sDebug+=" unquoted becomes \""+(sarrReturn[iNow]!=null?sarrReturn[iNow]:"")+"\"";
						Console.Error.WriteLine( String.Format("Argument:{0}",sDebug) );//debug only
					}
				}
				if (sarrReturn!=null) Console.Error.WriteLine("Arguments found:"+sarrReturn.Length); //debug only
				else Console.Error.WriteLine("Parsed arguments as null array");
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error in QuotedArgsToArgs");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return sarrReturn;
		}//end QuotedArgsToArgs
		public static string ToCSVField(string val) {//formerly ToCSVElement
			try {
				if (val!=null) {
					if (RString.IndexOf(val,'"')>-1) return "\""+val.Replace("\"","\"\"")+"\"";
					else return val;
				}
				else return "";
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return "";
		}//end ToCSVField
		public static string ToSgmlPropertyValue(string val) {//formerly ToCSVElement
			try {
				if (val!=null) {
					if (RString.IndexOf(val,'"')>-1||RString.IndexOf(val,' ')>-1) return "\""+val.Replace("\"","\"\"")+"\"";
					else return val;
				}
				else return "";
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return "";
		}//end ToCSVField

		#region Base64
		public static char[] ToBase64(byte[] byarr) {
			int iBuffer64;
			int iBlocks64;
			int iPadding64;
			char[] carrReturn=null;
			try {
				iBuffer64=byarr.Length;
				if((iBuffer64 % 3)==0) {
					iPadding64=0;
					iBlocks64=iBuffer64/3;
				}
				else {
					iPadding64=3-(iBuffer64 % 3);
					iBlocks64=(iBuffer64+iPadding64)/3;
				}
				iBuffer64=iBuffer64+iPadding64;//iBlocks64*3			
				//TODO: optionally use framework method (make separate function)
				byte[] byarrPadded;
				byarrPadded=new byte[iBuffer64];
				for (int iBlockNow=0; iBlockNow<iBuffer64; iBlockNow++) {
					if (iBlockNow<iBlocks64) {
						byarrPadded[iBlockNow]=byarr[iBlockNow];
					}
					else {
						byarrPadded[iBlockNow]=0;
					}
				}
				byte byChomp1, byChomp2, byChomp3;
				byte byTemp, byBits1, byBits2, byBits3, byBits4;
				byte[] byarrSparsenedBits=new byte[iBlocks64*4];
				carrReturn=new char[iBlocks64*4];
				for (int iBlockNow=0; iBlockNow<iBlocks64; iBlockNow++) {
					byChomp1=byarrPadded[iBlockNow*3];
					byChomp2=byarrPadded[iBlockNow*3+1];
					byChomp3=byarrPadded[iBlockNow*3+2];
			
					byBits1=(byte)((byChomp1 & 0xF0)>>2);
			
					byTemp=(byte)((byChomp1 & 0x3)<<4);
					byBits2=(byte)((byChomp2 & 0xF0)>>4);
					byBits2+=byTemp;
			
					byTemp=(byte)((byChomp2 & 0xF)<<2);
					byBits3=(byte)((byChomp3 & 0xC0)>>6);
					byBits3+=byTemp;
			
					byBits4=(byte)(byChomp3 & 0x3F);
			
					byarrSparsenedBits[iBlockNow*4]=byBits1;
					byarrSparsenedBits[iBlockNow*4+1]=byBits2;
					byarrSparsenedBits[iBlockNow*4+2]=byBits3;
					byarrSparsenedBits[iBlockNow*4+3]=byBits4;
				}
			
				for (int iBlockNow=0; iBlockNow<iBlocks64*4; iBlockNow++) {
					carrReturn[iBlockNow]=SixLowBitsToBase64(byarrSparsenedBits[iBlockNow]);
				}
			
				//covert last "A"s to "=", based on iPadding64
				switch (iPadding64) {
					case 0: break;
					case 1: carrReturn[iBlocks64*4-1]='=';
						break;
					case 2: carrReturn[iBlocks64*4-1]='=';
						carrReturn[iBlocks64*4-2]='=';
						break;
					default: break;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"encoding base64","ToBase64");
			}
			return carrReturn;
		}	//end ToBase64
		
		public static char SixLowBitsToBase64(byte byNow) {//formerly GetBase64CharFromSixBits
			if((byNow>=0) &&(byNow<=63)) return carrBase64[(int)byNow];//good
			else return ' '; //bad
		}
		#endregion Base64
		
		#region hex conversion (hexadecimal)
		public static bool HexColorStringToBGR24(ref byte[] byarrDest, int iDest, string sHex3or6WithOrWithoutPrecedent) {
			if (byarrDest!=null&&byarrDest.Length>=iDest+3) {
				return HexColorStringToBGR24(out byarrDest[iDest],out byarrDest[iDest+1],out byarrDest[iDest+2],sHex3or6WithOrWithoutPrecedent);
			}
			else RReporting.ShowErr("Not enough room for pixel data","converting html hex color notation", String.Format("HexColorStringToBGR24({0},{1},{2})",RReporting.ArrayMessage(byarrDest),iDest,sHex3or6WithOrWithoutPrecedent));
			return false;
		}
		public static bool HexColorStringToBGR24(out byte r, out byte g, out byte b, string sHex3or6WithOrWithoutPrecedent) {
			bool bGood=true;
			string sHex=sHex3or6WithOrWithoutPrecedent;
			string sFirstValue=sHex;
			try {
				if (RReporting.IsNotBlank(sHex)) {
					if (sHex.StartsWith("#")) sHex=RString.SafeSubstring(sHex,1);
					else if (sHex.StartsWith("0x")) sHex=RString.SafeSubstring(sHex,2);
					else if (sHex.StartsWith("&H")) sHex=RString.SafeSubstring(sHex,2);
					if (sHex.Length>0) {
						if (sHex.Length==8) sHex=RString.SafeSubstring(sHex,2);//remove leading alpha
						if (sHex.Length==3) {//3-character hex color i.e. "#RGB"
							r=(byte)(HexNibbleToByte(sHex[0])*17);//R
							g=(byte)(HexNibbleToByte(sHex[1])*17);//G
							b=(byte)(HexNibbleToByte(sHex[2])*17);//B
						}
						else if (sHex.Length==6) {//6-character hex color i.e. "#RRGGBB"
							r=HexToByte(sHex,0);//R
							g=HexToByte(sHex,2);//G
							b=HexToByte(sHex,4);//B
						}
						else if (sHex.Length==8) { //i.e. if was &H00FFFFFF etc //TODO: debug byte order? OpenWC3 may need nonstandard ABGR
							r=HexToByte(sHex,2);//R
							g=HexToByte(sHex,4);//G
							b=HexToByte(sHex,6);//B
							RReporting.Warning("Skipped alpha in color notation conversion. {character-pair:\""+RString.SafeSubstring(sHex,0,2)+"\"}");
						}
						else {
							RReporting.ShowErr("Can't use color in this format.","HexColorStringToBGR24","adding incorrect color notation {"+RReporting.DebugStyle("hex",sFirstValue,true)+"}");
							r=0; g=0; b=0;
						}
					}
					else {
						RReporting.ShowErr("Can't use blank color.","HexColorStringToBGR24","adding blank color {"+RReporting.DebugStyle("hex",sFirstValue,true)+"}");
						r=0; g=0; b=0;
					}
				}
				else {
					RReporting.ShowErr("Can't use empty color/name string.","HexColorStringToBGR24","adding blank color without notation {hex:"+RReporting.DebugStyle("hex",sFirstValue,true)+"}");
					r=0; g=0; b=0;
				}
			}
			catch (Exception exn) {
				bGood=false;
				r=0; g=0; b=0;
				//"Error writing to color data array"
				RReporting.ShowExn(exn,"converting color notation","Base HexColorStringToBGR24");
			}
			return bGood;
		}//end HexColorStringToBGR24
		public static byte HexToByte(string sHexChars) {
			return HexToByte(sHexChars,0);
		}
		public static byte HexToByte(string sHexChars, int iStartIndexFromWhichToGrabTwoChars) {//formerly ByteFromHexChars
			byte byReturn=0;
			try {
				byReturn=(byte)(HexNibbleToByte(sHexChars[iStartIndexFromWhichToGrabTwoChars])<<4);
				byReturn&=HexNibbleToByte(sHexChars[iStartIndexFromWhichToGrabTwoChars+1]);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"interpreting hexadecimal data",
					String.Format("HexToByte(sHexChars:{0},at:{1})[using-data:{2}]",
						RReporting.StringMessage(sHexChars,false),iStartIndexFromWhichToGrabTwoChars,RReporting.StringMessage(RString.SafeSubstring(sHexChars,iStartIndexFromWhichToGrabTwoChars,2),true)
					)
				);
			}
			return byReturn;
		}
		/// <summary>
		/// Returns a byte between 0 and 15
		/// </summary>
		/// <param name="cHex"></param>
		/// <returns></returns>
		public static byte HexNibbleToByte(char cHex) {//from ByteFromHexCharNibble
			byte valReturn=0;
			if (cHex<58) {
				valReturn=(byte)(((int)cHex)-48); //i.exn. changes 48 ('0') to [0]
			}
			else {
				valReturn=(byte)(((int)cHex)-55); //i.exn. changes 65 ('A') to  [10]
			}
			if (valReturn<0||valReturn>15) {
				RReporting.ShowErr("Failed to convert hex char.","HexNibbleToByte","interpreting data {cHex:'"+char.ToString(cHex)+"'; valReturn:"+valReturn.ToString()+";}");
				valReturn=0;
			}
			return valReturn;
		}
		public static int HexNibbleToInt(char cHex) {
			int valReturn=0;
			if (cHex<58) {
				valReturn=((int)cHex)-48; //i.exn. changes 48 ('0') to [0]
			}
			else {
				valReturn=((int)cHex)-55; //i.exn. changes 65 ('A') to  [10]
			}
			if (valReturn<0||valReturn>15) {
				RReporting.ShowErr("Failed to convert hex char.","HexNibbleToInt","interpreting data {cHex:'"+char.ToString(cHex)+"'; valReturn:"+valReturn.ToString()+";}");
				valReturn=0;
			}
			return valReturn;
		}
		public static bool ToHex(char[] carrHexDest, ref int iDestCharCursorToMove, byte byValue) {
			bool bGood=true;
			try {
				carrHexDest[iDestCharCursorToMove]=NibbleToHexChar(byValue>>4);
				iDestCharCursorToMove++;
				carrHexDest[iDestCharCursorToMove]=NibbleToHexChar(byValue&byLowNibble);
				iDestCharCursorToMove++;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"interpreting data",
					String.Format("ToHex(carrHexDest:{0}, iDestCharCursorToMove:{1}, byValue:{2})",
						RReporting.ArrayMessage(carrHexDest),iDestCharCursorToMove,byValue
					)
				);
			}
			return bGood;
		}
		public static string ToHex(byte byVal) { //formerly HexOfByte
			string sHexReturn="00";
			try {
				sHexReturn=char.ToString(NibbleToHexChar(byVal>>4))+char.ToString(NibbleToHexChar(byVal&byLowNibble));
				if (sHexReturn==null) {
					Console.Error.WriteLine("Cannot generate hex pair string, forcing zero");
					sHexReturn="00";
				}
				else if (sHexReturn.Length!=2) {
					if (sHexReturn.Length==1) {
						Console.Error.WriteLine("Cannot generate hex pair string, forcing leading zero");
						sHexReturn="0"+sHexReturn;
					}
					else {
						Console.Error.WriteLine("Got wrong length hex pair string, forcing zero");
						sHexReturn="00";
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"interpreting data","ToHex("+byVal.ToString()+")[return:"+RReporting.StringMessage(sHexReturn,true)+"]");
			}
			return sHexReturn;
		}
		public static string ToHex(byte[] byarrToHex, int startIndex, int iLength, int iChunkBytes) {//formerly HexOfBytes
			string sReturn="";
			int iChunkPlace=0;
			int iByte=startIndex;
			try {
				for (int iRelative=0; iRelative<iLength; iRelative++, iByte++) {
					if (iChunkBytes>0 && iChunkPlace==iChunkBytes) {
						sReturn+=" ";
						iChunkPlace=0;
					}
					sReturn+=ToHex(byarrToHex[iByte]);
					iChunkPlace++;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"",
					String.Format( "ToHex(byarrToHex:{0}, startIndex:{1}, iLength:{2}, iChunkBytes:{3})",
					 RReporting.ArrayMessage(byarrToHex),startIndex,iLength,iChunkBytes )
				);
			}
			return sReturn;
		}

		public static string NibbleToHexCharToString(byte byValue) { //formerly StringHexCharOfNibbl e
			char cNow=NibbleToHexChar(byValue);
			return char.ToString(cNow);
		}
		
		public static string NibbleToHexCharToString(int iValue) {
			char cNow=NibbleToHexChar(iValue);
			return char.ToString(cNow);
		}
		public static char NibbleToHexChar(byte byValue) {//formerly HexCharOfNibble
			char cNow='0';
			if (byValue<10) {
				cNow=(char)(byValue+48); //i.exn. changes 0 to 48 ('0')
			}
			else {
				cNow=(char)(byValue+55); //i.exn. changes 10 to 65 ('A')
			}
			return cNow;
		}
		public static char NibbleToHexChar(int iValue) {
			char cNow='0';
			if (iValue<10) {
				cNow=(char)(iValue+48); //i.exn. changes 0 to 48 ('0')
			}
			else {
				cNow=(char)(iValue+55); //i.exn. changes 10 to 65 ('A')
			}
			return cNow;
		}
		#endregion hex conversion (hexadecimal)

	}//end RConvert
}//end namespace
