/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 8/2/2005
 * Time: 6:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace ExpertMultimedia {
	public class RPaint { //formerly RBrush
		public byte[] data32=null;
		public byte[] data32Copied64=null;
		public byte[] data24Copied48=null;
		public byte B { get { return data32[0]; }
			set { data32[0]=value; data32Copied64[0]=value; data32Copied64[4]=value; data24Copied48[0]=value; data24Copied48[3]=value; } }
		public byte G { get { return data32[1]; }
			set { data32[1]=value; data32Copied64[1]=value; data32Copied64[5]=value; data24Copied48[1]=value; data24Copied48[4]=value; } }
		public byte R { get { return data32[2]; }
			set { data32[2]=value; data32Copied64[2]=value; data32Copied64[6]=value; data24Copied48[2]=value; data24Copied48[5]=value; } }
		public byte A { get { return data32[3]; }
			set { data32[3]=value; data32Copied64[3]=value; data32Copied64[7]=value; } }
		#region constructors
		public RPaint() {
			InitUninitializedNonNull();
			SetArgb(0,0,0,0);
		}
		/// <summary>
		/// Only sets to zero optionally, otherwise arrays are uninitialized and non-null
		/// </summary>
		/// <param name="bInitializeValues"></param>
		public RPaint(bool bInitializeValues) {
			InitUninitializedNonNull();
			if (bInitializeValues) SetArgb(0,0,0,0);
		}
		public RPaint(Color colorNow) {
			InitUninitializedNonNull();
			Set(colorNow);
		}
		public RPaint(uint dwPixelBGRA_LittleEndian_LeastSignificantByteIsAlpha) {
			InitUninitializedNonNull();
			Set(dwPixelBGRA_LittleEndian_LeastSignificantByteIsAlpha);
		}
		//public RPaint (byte a, byte r, byte g, byte b) {
		// this constructor has been eliminated to avoid byte order confusion--use RPaint.From instead
		//	InitUninitializedNonNull();
		//	SetArgb(a,r,g,b);
		//}
		/// <summary>
		/// Assumes BGRA, BGR, or grayscale (255 will be used as alpha and gray value as color)
		/// </summary>
		/// <param name="byarrPixelData"></param>
		public RPaint (byte[] byarrPixelData, int iStart, int iChannels) {
			InitUninitializedNonNull();
			if (byarrPixelData!=null) {
				if ( iStart+iChannels<=byarrPixelData.Length
				    && iStart>=0
				    && iStart<byarrPixelData.Length
				    && iChannels>=1 ) {
					if (iChannels==1) {
						SetArgb(255,byarrPixelData[iStart],byarrPixelData[iStart],byarrPixelData[iStart]);
					}
					else if (iChannels==3) {
						SetArgb(255,byarrPixelData[iStart+2],byarrPixelData[iStart+1],byarrPixelData[iStart]);
					}
					else if (iChannels==4) {
						SetArgb(byarrPixelData[iStart+3],byarrPixelData[iStart+2],byarrPixelData[iStart+1],byarrPixelData[iStart]);
					}
					else {
						RReporting.ShowErr("Error: unrecognized channel count was was ignored (must be 4 for BGRA, 3 for BGR, or 1 for grayscale) so new RPaint was set to Argb(0,0,0,0)","creating RPaint object {byarrPixelData"+((byarrPixelData!=null)?(".Length:"+byarrPixelData.Length.ToString()):":null")+"; iStart="+iStart.ToString()+"; iChannels="+iChannels.ToString()+"}");
						SetArgb(0,0,0,0);
					}
				}
				else {
					RReporting.ShowErr("Error: out-of-range array access was was ignored so new RPaint was set to Argb(0,0,0,0)","creating RPaint object {byarrPixelData"+((byarrPixelData!=null)?(".Length:"+byarrPixelData.Length.ToString()):":null")+"; iStart="+iStart.ToString()+"; iChannels="+iChannels.ToString()+"}");
					SetArgb(0,0,0,0);
				}
			}
			else {
				RReporting.ShowErr("Error: null byte array was ignored so new RPaint was set to Argb(0,0,0,0)");
				SetArgb(0,0,0,0);
			}
		}
		private void InitUninitializedNonNull() {
			data32=new byte[4];
			data32Copied64=new byte[8];
			data24Copied48=new byte[6];
		}
		public RPaint Copy() {
			RPaint paintReturn=new RPaint();
			paintReturn.SetArgb(A,R,G,B);
			return paintReturn;
		}
		public static RPaint FromArgb(byte a, byte r, byte g, byte b) {
			RPaint paintReturn=new RPaint(false);
			paintReturn.SetArgb(a,r,g,b);
			return paintReturn;
		}
		public static RPaint FromRgb(byte r, byte g, byte b) {
			RPaint paintReturn=new RPaint(false);
			paintReturn.SetArgb(255,r,g,b);
			return paintReturn;
		}
		public static RPaint FromRgbRatio(float r, float g, float b) {
			RPaint paintReturn=new RPaint(false);
			paintReturn.SetArgbRatio(1.0f,r,g,b);
			return paintReturn;
		}
		public static RPaint FromArgbRatio(float a, float r, float g, float b) {
			RPaint paintReturn=new RPaint(false);
			paintReturn.SetArgbRatio(a,r,g,b);
			return paintReturn;
		}
		public static RPaint FromRgbRatio(double r, double g, double b) {
			RPaint paintReturn=new RPaint(false);
			paintReturn.SetArgbRatio(1.0,r,g,b);
			return paintReturn;
		}
		public static RPaint FromArgbRatio(double a, double r, double g, double b) {
			RPaint paintReturn=new RPaint(false);
			paintReturn.SetArgbRatio(a,r,g,b);
			return paintReturn;
		}
		/// <summary>
		/// Multiplies everything EXCEPT a by multiplier then returns a resulting rpaint object
		/// </summary>
		/// <param name="paintReturn">Set to paintSource, or argb(0,0,0,0) if paintSource is null</param>
		/// <param name="paintSource">source</param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		public static bool From(ref RPaint paintReturn, RPaint paintSource, double multiplier) {
			bool bGood=true;
			if (paintReturn==null) paintReturn=new RPaint(false);
			if (paintSource!=null) paintReturn.SetArgb(paintSource.A, RMath.ByRound((double)paintSource.R*multiplier), RMath.ByRound((double)paintSource.G*multiplier), RMath.ByRound((double)paintSource.B*multiplier));
			else {
				bGood=false;
				paintReturn.SetArgb(0,0,0,0);//if source is null set dest to 0s
			}
			return bGood;
		}
		public static RPaint From(Color color) {
			RPaint paintReturn=new RPaint();
			paintReturn.Set(color);
			return paintReturn;
		}
		#endregion constructors
		///<summary>
		///Multiplies and clamps values to between 0 & 255.  Does not modify alpha.
		///</summary>
		public void Multiply(double multiplier) {
			SetRgb_IgnoreAlpha(RMath.ByRound((double)R*multiplier), RMath.ByRound((double)G*multiplier), RMath.ByRound((double)B*multiplier));
		}
		public static void Multiply(ref RPaint rpaintOut, RPaint paintSource, double multiplier) {
			if (rpaintOut==null) rpaintOut=new RPaint();
			rpaintOut.SetArgb(paintSource.A, RMath.ByRound((double)paintSource.R*multiplier), RMath.ByRound((double)paintSource.G*multiplier), RMath.ByRound((double)paintSource.B*multiplier));
		}
		/// <summary>
		/// Set by 6-character (rgb), 8-character (rgba), or 3-character (rgb) hex string (may start with # or 0x, x and hex chars are case-insensitive).
		/// </summary>
		/// <param name="sHexCode"></param>
		/// <returns></returns>
		public bool SetRgb(string sHexCode) {
			bool bGood=true;
			try {
				sHexCode=sHexCode.ToUpper();
				if (sHexCode.StartsWith("#")) sHexCode=sHexCode.Substring(1);
				else if (sHexCode.StartsWith("0X")) sHexCode=sHexCode.Substring(2);
				if (sHexCode.Length<6) {
					if (sHexCode.Length==3 ) {
						if (!SetRgba(RConvert.ToByte(RConvert.HexNibbleToInt(sHexCode[0])<<4),//*17
									   RConvert.ToByte(RConvert.HexNibbleToInt(sHexCode[1])<<4),//*17
									   RConvert.ToByte(RConvert.HexNibbleToInt(sHexCode[2])<<4),//*17
									   255
									  )) {
							bGood=false;
						}
						
					}
					else {
						RReporting.ShowErr("This hex color code in the file is not complete","","RBrush SetRgb("+sHexCode+")");
						bGood=false;
					}
				}
				else {
					if (sHexCode.Length>=8) {
						if (!SetRgba(RConvert.HexToByte(sHexCode.Substring(0,2)),
									   RConvert.HexToByte(sHexCode.Substring(2,2)),
									   RConvert.HexToByte(sHexCode.Substring(4,2)),
									   RConvert.HexToByte(sHexCode.Substring(6,2))
									  )) {
							bGood=false;
						}
					}
					else {
						if (!SetRgba(RConvert.HexToByte(sHexCode.Substring(0,2)),
									   RConvert.HexToByte(sHexCode.Substring(2,2)),
									   RConvert.HexToByte(sHexCode.Substring(4,2)), 255)) {
							bGood=false;
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"interpreting the specified hex color code","RBrush SetRgb("+sHexCode+")");
				bGood=false;
			}
			return bGood;
		}//end SetRgb(Hex)
		public bool SetArgbRatio(float r, float g, float b, float a) {
			return SetArgb((byte)(a*255.0f),(byte)(r*255.0f),(byte)(g*255.0f),(byte)(b*255.0f));
		}
		public bool SetArgbRatio(double r, double g, double b, double a) {
			return SetArgb((byte)(a*255.0d),(byte)(r*255.0d),(byte)(g*255.0d),(byte)(b*255.0d));
		}
		public bool SetRgbRatio(float r, float g, float b) {
			return SetArgbRatio(1.0f,r,g,b);
		}
		public bool SetRgbRatio(double r, double g, double b) {
			return SetArgbRatio(1.0d,r,g,b);
		}
		public bool SetRgba(byte r, byte g, byte b, byte a) {
			return SetArgb(a,r,g,b);
		}
		public bool SetRgb(byte r, byte g, byte b) {
			return SetArgb(255,r,g,b);
		}
		public unsafe bool SetRgb_IgnoreAlpha(byte r, byte g, byte b) {//formerly SkipAlpha
			data32[0]=b;data32[1]=g;data32[2]=r;
			data24Copied48[0]=b; data24Copied48[1]=g; data24Copied48[2]=r;
			data24Copied48[3]=b; data24Copied48[4]=g; data24Copied48[5]=r;
			fixed (byte* lp64=data32Copied64, lp32=data32) {
				byte* lp64Now=lp64;
				*((UInt32*)lp64Now) = *((UInt32*)lp32);
				lp64Now+=4;
				*((UInt32*)lp64Now) = *((UInt32*)lp32);
			}
			return true; //debug no exception handling (assumes method can't fail)
		}
		///<summary>
		///Copies data by value from paintSource to this rpaint
		///</summary>
		public void Set(RPaint paintSource) {
			//return SetArgb(paintSource.A,paintSource.R,paintSource.G,paintSource.B);
			RMemory.CopyFastVoid(ref paintSource.data32, ref data32);
			RMemory.CopyFastVoid(ref paintSource.data32Copied64, ref data32Copied64);
			RMemory.CopyFastVoid(ref paintSource.data24Copied48, ref data24Copied48);
		}
		public bool Set(Color colorX) {
			return SetArgb(colorX.A,colorX.R,colorX.G,colorX.B);
		}
		public bool Set(uint dwPixel32BGRA_LittleEndian_LeastSignificantByteAsA) {
			return this.SetArgb(
				RConvert.ToByte(dwPixel32BGRA_LittleEndian_LeastSignificantByteAsA&0xFF),
				RConvert.ToByte((dwPixel32BGRA_LittleEndian_LeastSignificantByteAsA>>8)&0xFF),
				RConvert.ToByte((dwPixel32BGRA_LittleEndian_LeastSignificantByteAsA>>16)&0xFF),
				RConvert.ToByte((dwPixel32BGRA_LittleEndian_LeastSignificantByteAsA>>24)&0xFF)
				);
		}
		public bool SetBigEndian(uint dwPixel32_BigEndian_MostSignificantByteAsA) {
			return this.SetArgb(
				RConvert.ToByte((dwPixel32_BigEndian_MostSignificantByteAsA>>24)&0xFF),
				RConvert.ToByte((dwPixel32_BigEndian_MostSignificantByteAsA>>16)&0xFF),
				RConvert.ToByte((dwPixel32_BigEndian_MostSignificantByteAsA>>8)&0xFF),
				RConvert.ToByte(dwPixel32_BigEndian_MostSignificantByteAsA&0xFF)
				);
		}
		public bool Set(RPaint paintSource, float multiplier) {
			return SetArgb(paintSource.A, RMath.ByRound((double)paintSource.R*multiplier), RMath.ByRound((double)paintSource.G*multiplier), RMath.ByRound((double)paintSource.B*multiplier));
		}
		public bool Set(RPaint paintSource, double multiplier) {
			return SetArgb(paintSource.A, RMath.ByRound((double)paintSource.R*multiplier), RMath.ByRound((double)paintSource.G*multiplier), RMath.ByRound((double)paintSource.B*multiplier));
		}
		public bool Set(Color colorX, double multiplier) {
			return SetArgb(colorX.A, RMath.ByRound((double)colorX.R*multiplier), RMath.ByRound((double)colorX.G*multiplier), RMath.ByRound((double)colorX.B*multiplier));
		}
		///<summary>
		///Primary Set overload (all overloads call this)
		///</summary>
		public unsafe bool SetArgb(byte a, byte r, byte g, byte b) {
			try {
				//bool bMake=false;
				//if (data32==null) {data32=new byte[4]; bMake=true; }
				//else if ( data32[0]!=b
				//	||data32[1]!=g
				//	||data32[2]!=r
				//	||data32[3]!=a
				//	) {
				//	bMake=true;
				//}
				//if (bMake) {
				data32[0]=b;
				data32[1]=g;
				data32[2]=r;
				data32[3]=a;
				data24Copied48[0]=b; data24Copied48[1]=g; data24Copied48[2]=r;
				data24Copied48[3]=b; data24Copied48[4]=g; data24Copied48[5]=r;
				//if (data32Copied64==null) data32Copied64=new byte[8];
				fixed (byte* lp64=data32Copied64, lp32=data32) {
					byte* lp64Now=lp64;
					*((UInt32*)lp64Now) = *((UInt32*)lp32);
					lp64Now+=4;
					*((UInt32*)lp64Now) = *((UInt32*)lp32);
				}
				//}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RBrush SetArgb(a,r,g,b)");
				return false;
			}
			return true;
		}//end SetArgb (primary overload)
		public bool SetHsva(float h, float s, float v, float aTo1) {
			byte r,g,b,a;
			a=RConvert.ToByte(aTo1*255.0f);
			RImage.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
			return SetArgb(a,r,g,b);
		}
		public bool SetHsva(double h, double s, double v, double aTo1) {
			byte r,g,b,a;
			a=RConvert.ToByte(aTo1*255.0);
			RImage.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
			return SetArgb(a,r,g,b);
		}
		//public unsafe bool Paint(RImage destination, int x, int y) {
		//}
	}//end RPaint
}//end namespace
