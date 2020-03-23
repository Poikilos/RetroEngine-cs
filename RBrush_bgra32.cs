/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 8/2/2005
 * Time: 6:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
 //TODO: blend src>>2+dest>>2 if 127 OR 128
 ///TODO: create RImageVSHA.cs but still call the object a RImage
 ///-combinations allowed: 88 VA16, 844 VSH16, and 8448 VSHA24

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace ExpertMultimedia {
	public class RBrush {
		public byte[] data32=null;
		public byte[] data32Copied64=null;
		public byte B { get { return data32[0]; }
			set { data32[0]=value; data32Copied64[0]=value; data32Copied64[4]=value; } }
		public byte G { get { return data32[1]; }
			set { data32[1]=value; data32Copied64[1]=value; data32Copied64[5]=value; } }
		public byte R { get { return data32[2]; }
			set { data32[2]=value; data32Copied64[2]=value; data32Copied64[6]=value; } }
		public byte A { get { return data32[3]; }
			set { data32[3]=value; data32Copied64[3]=value; data32Copied64[7]=value; } }
		#region constructors
		RBrush() {
			data32=new byte[4];
			data32Copied64=new byte[8];
		}
		RBrush Copy() {
			RBrush rbReturn=new RBrush();
			rbReturn.From(this);
		}
		RBrush FromArgb(byte a, byte r, byte g, byte b) {
			RBrush brushReturn=new Brush();
			brushReturn.SetArgb(a,r,g,b);
			return brushReturn;
		}
		RBrush FromRgb(byte r, byte g, byte b) {
			RBrush brushReturn=new Brush();
			brushReturn.SetRgb(r,g,b);
			return brushReturn;
		}
		RBrush FromRgbRatio(float r, float g, float b) {
			RBrush brushReturn=new Brush();
			brushReturn.SetRgbRatio(r,g,b);
			return brushReturn;
		}
		RBrush FromArgbRatio(float a, float r, float g, float b) {
			RBrush brushReturn=new Brush();
			brushReturn.SetArgbRatio(a,r,g,b);
			return brushReturn;
		}
		RBrush FromRgbRatio(double r, double g, double b) {
			RBrush brushReturn=new Brush();
			brushReturn.SetRgbRatio(r,g,b);
			return brushReturn;
		}
		RBrush FromArgbRatio(double a, double r, double g, double b) {
			RBrush brushReturn=new Brush();
			brushReturn.SetArgbRatio(a,r,g,b);
			return brushReturn;
		}
		public static RBrush From(RBrush brushX, double multiplier) {
			RBrush brushReturn=new RBrush();
			if (brushX!=null) brushReturn.SetArgb(brushX.A, RMath.ByRound((double)brushX.R*multiplier), RMath.ByRound((double)brushX.G*multiplier), RMath.ByRound((double)brushX.B*multiplier));
			return brushReturn;
		}
		#endregion constructors
		///<summary>
		///Multiplies and clamps values to between 0 & 255.
		///</summary>
		public void Multiply(double multiplier) {
			SetRgb_IgnoreAlpha(RMath.ByRound((double)R*multiplier), RMath.ByRound((double)G*multiplier), RMath.ByRound((double)B*multiplier));
		}
		public static void Multiply(RBrush brushOut, RBrush brushX, double multiplier) {
			brushOut.SetArgb(brushX.A, RMath.ByRound((double)brushX.R*multiplier), RMath.ByRound((double)brushX.G*multiplier), RMath.ByRound((double)brushX.B*multiplier));
		}
		public bool SetRgb(string sHexCode) {
			bool bGood=true;
			try {
				if (sHexCode.StartsWith("#")) sHexCode=sHexCode.Substring(1);
				if (sHexCode.Length<6) {
					RReporting.ShowErr("This hex color code in the file is not complete","","RImage SetRgb("+sHexCode+")");
					bGood=false;
				}
				else {
					sHexCode=sHexCode.ToUpper();
					//TODO: allow alpha here
					if (!SetRgba(Base.HexToByte(sHexCode.Substring(0,2)),
					               Base.HexToByte(sHexCode.Substring(2,2)),
					               Base.HexToByte(sHexCode.Substring(4,2)), 255)) {
						bGood=false;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"interpreting the specified hex color code","RImage SetRgb("+sHexCode+")");
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
		public bool SetRgb_IgnoreAlpha(byte r, byte g, byte b) {//formerly SkipAlpha
			data32[0]=b;data32[1]=g;data32[2]=r;
			fixed (byte* lp64=data32Copied64, lp32=data32) {
				byte* lp64Now=lp64;
				*((UInt32*)lp64Now) = *((UInt32*)lp32);
				lp64Now+=4;
				*((UInt32*)lp64Now) = *((UInt32*)lp32);
			}
		}
		///<summary>
		///Copies data by value from brushX to this brush
		///</summary>
		public void Set(RBrush brushX) {
			//return SetArgb(brushX.A,brushX.R,brushX.G,brushX.B);
			RMemory.CopyFastVoid(brushX.data32, data32);
			RMemory.CopyFastVoid(brushX.data32Copied64, data32Copied64);
		}
		public bool Set(Color colorX) {
			return SetArgb(colorX.A,colorX.R,colorX.G,colorX.B);
		}
		public bool Set(Brush brushX, float multiplier) {
			return SetArgb(brushX.A, RMath.ByRound((double)brushX.R*multiplier), RMath.ByRound((double)brushX.G*multiplier), RMath.ByRound((double)brushX.B*multiplier));
		}
		public bool Set(Brush brushX, double multiplier) {
			return SetArgb(brushX.A, RMath.ByRound((double)brushX.R*multiplier), RMath.ByRound((double)brushX.G*multiplier), RMath.ByRound((double)brushX.B*multiplier));
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
				RReporting.ShowExn(exn,"","RImage SetArgb(a,r,g,b)");
				return false;
			}
			return true;
		}//end SetArgb (primary overload)
		public bool SetHsva(float h, float s, float v, float aTo1) {
			byte r,g,b,a;
			a=RConvert.ToByte(aTo1*255.0f);
			RConvert.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
			return SetArgb(a,r,g,b);
		}
		public bool SetHsva(double h, double s, double v, double aTo1) {
			byte r,g,b,a;
			a=RConvert.ToByte(aTo1*255.0);
			RConvert.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
			return SetArgb(a,r,g,b);
		}
		//public unsafe bool Paint(RImage destination, int x, int y) {
		//}
	}//end RBrush
}//end namespace