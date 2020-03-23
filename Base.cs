//Created 2006-09-26 11:47PM
//Credits:
//-Jake Gustafson www.expertmultimedia.com
//Requirements:
//-Vars.cs (which does have cross-dependency)
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Drawing;//ONLY for imageformat
using System.Drawing.Imaging;//ONLY for imageformat
using System.Windows.Forms;//ONLY for MessageBox
//using REAL = System.Double; //System.Single
using REAL = System.Single; //System.Double
using System.Net;
/*
 * TODO: Math stuff to use for fun patterns:
 * -Lissajous curve
 * -Brownian motion
 *		-Brownian motion is random(?) (NOT psuedorandom) but accurately simulates particles in a fluid
 * -Fourier series
 *		-Converts any curve to a series of sine functions!!!!!!!!!!!!!!!!!!!!!!!!!!
 * 		-Approximating a square function makes it ANALOG-LIKE!!!!!!!!!!!!!!!!!!!!
 * 			-called the Gibbs phenomenon!
 */
namespace ExpertMultimedia {
	#region simple types
	public struct Pixel24Struct {
		public byte B;
		public byte G;
		public byte R;
	}
	public struct PixelHsvaStruct {
		public byte H;
		public byte S;
		public byte V;
		public byte A;
		//public PixelHsvaStruct() {
		//	Set(0,0,0,0);
		//}
		public PixelHsvaStruct(byte h, byte s, byte v, byte a) {
			H=h; S=s; V=v; A=a;
		}
		public void Set(byte h, byte s, byte v, byte a) {
			H=h; S=s; V=v; A=a;
		}
		public void FromArgb(byte a, byte r, byte g, byte b) {
			Base.RgbToHsv(out H, out S, out V, ref r, ref g, ref b);
			A=a;
		}
		public void FromRgb(byte r, byte g, byte b) {
			Base.RgbToHsv(out H, out S, out V, ref r, ref g, ref b);
			A=255;
		}
		public float HueDeg {
			get { return Base.ByteToDegF(H); }
			set { H=Base.DegToByte((float)value); }
		}
		public float HueMultiplier {
			get { return SafeConvert.ToFloat_255As1(H); }
			set { H=SafeConvert.ToByte_1As255((float)value); }
		}
	}
	public class Pixel32 {
		public byte B;
		public byte G;
		public byte R;
		public byte A;
		public Pixel32(byte a, byte r, byte g, byte b) {
			Set(a,r,g,b);
		}
		public Pixel32(Pixel32 colorNow) {
			Set(colorNow);
		}
		public Pixel32(Color colorNow) {
			Set(colorNow);
		}
		public void Set(byte a, byte r, byte g, byte b) {
			A=a;
			R=r;
			G=g;
			B=b;
		}
		public void Set(Pixel32 colorNow) {
			A=colorNow.A;
			R=colorNow.R;
			G=colorNow.G;
			B=colorNow.B;
		}
		public void Set(Color colorNow) {
			A=colorNow.A;
			R=colorNow.R;
			G=colorNow.G;
			B=colorNow.B;
		}
		public static Pixel32 FromArgb(byte a, byte r, byte g, byte b) {
			return new Pixel32(a,r,g,b);
		}
		public static Pixel32 FromArgb(float a, float r, float g, float b) {
			return new Pixel32(Base.ByRound(a),Base.ByRound(r),Base.ByRound(g),Base.ByRound(b));
		}
	}//end Pixel32
	public class Pixel32Struct {
		public byte B;
		public byte G;
		public byte R;
		public byte A;
		public Pixel32Struct() {
			Set(0,0,0,0);
		}
		public Pixel32Struct(byte a, byte r, byte g, byte b) {
			Set(a,r,g,b);
		}
		public Pixel32Struct(Pixel32Struct colorNow) {
			Set(colorNow);
		}
		public Pixel32Struct(Color colorNow) {
			Set(colorNow);
		}
		public void Set(byte a, byte r, byte g, byte b) {
			A=a;
			R=r;
			G=g;
			B=b;
		}
		public void Set(Pixel32Struct colorNow) {
			A=colorNow.A;
			R=colorNow.R;
			G=colorNow.G;
			B=colorNow.B;
		}
		public void Set(Color colorNow) {
			A=colorNow.A;
			R=colorNow.R;
			G=colorNow.G;
			B=colorNow.B;
		}
		public static Pixel32Struct FromArgb(byte a, byte r, byte g, byte b) {
			return new Pixel32Struct(a,r,g,b);
		}
		public static Pixel32Struct FromArgb(float a, float r, float g, float b) {
			return new Pixel32Struct(Base.ByRound(a),Base.ByRound(r),Base.ByRound(g),Base.ByRound(b));
		}
	}//end Pixel32Struct
	public class PixelYhs {
		public REAL Y;
		public REAL H;
		public REAL S;
		public PixelYhs() {
			Reset();
		}
		public PixelYhs(REAL y, REAL h, REAL s) {
			Set(y,h,s);
		}
		public PixelYhs(PixelYhsa pxNow) {
			From(pxNow);
		}
		public PixelYhs Copy() {
			PixelYhs pxReturn=new PixelYhs();
			pxReturn.Set(Y,H,S);
			return pxReturn;
		}
		public void CopyTo(ref PixelYhs pxDest) {
			if (pxDest==null) pxDest=new PixelYhs(Y,H,S);
			else pxDest.Set(Y,H,S);
		}
		public void CopyTo(ref PixelYhsa pxDest) {
			if (pxDest==null) pxDest=new PixelYhsa(Y,H,S,Base.r1);
			else pxDest.Set(Y,H,S,Base.r1);
		}
		public void Reset() {
			Y=0;
			H=0;
			S=0;
		}
		public void Set(REAL y, REAL h, REAL s) {
			Y=y;
			H=h;
			S=s;
		}
		public void Set(byte y, byte h, byte s) {
			Y=Base.ByteToReal(y);
			H=Base.ByteToReal(h);
			S=Base.ByteToReal(s);
		}
		public void Get(out byte y, out byte h, out byte s) {
			y=Base.DecimalToByte(Y);
			h=Base.DecimalToByte(H);
			s=Base.DecimalToByte(S);
		}
		public void From(PixelYhs pxNow) {
			Y=pxNow.Y;
			H=pxNow.H;
			S=pxNow.S;
		}
		public void From(PixelYhsa pxNow) {
			Y=pxNow.Y;
			H=pxNow.H;
			S=pxNow.S;
		}
		public void FromRgb(byte r, byte g, byte b) {
			Base.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//Base.RgbToYhs(out Y, out H, out S, (REAL)r, (REAL)g, (REAL)b);
		}
	}//end PixelYhs
	public class PixelYhsa {
		public REAL Y;
		public REAL H;
		public REAL S;
		public REAL A;
		public PixelYhsa() {
			Reset();
		}
		public PixelYhsa(REAL y, REAL h, REAL s, REAL a) {
			Set(y,h,s,a);
		}
		public PixelYhsa(PixelYhs pxNow) {
			From(pxNow);
		}
		public PixelYhsa Copy() {
			PixelYhsa pxReturn=new PixelYhsa();
			pxReturn.Set(Y,H,S,A);
			return pxReturn;
		}
		public void CopyTo(ref PixelYhsa pxDest) {
			if (pxDest==null) pxDest=new PixelYhsa(Y,H,S,A);
			else pxDest.Set(Y,H,S,A);
		}
		public void CopyTo(ref PixelYhs pxDest) {
			if (pxDest==null) pxDest=new PixelYhs(Y,H,S);
			else pxDest.Set(Y,H,S);
		}
		public void Reset() {
			Y=0;
			H=0;
			S=0;
			A=0;
		}
		public void Set(REAL y, REAL h, REAL s, REAL a) {
			Y=y;
			H=h;
			S=s;
			A=a;
		}
		public void Set(byte y, byte h, byte s, byte a) {
			Y=Base.ByteToReal(y);
			H=Base.ByteToReal(h);
			S=Base.ByteToReal(s);
			A=Base.ByteToReal(a);
		}
		public void Get(out byte y, out byte h, out byte s, out byte a) {
			y=Base.DecimalToByte(Y);
			h=Base.DecimalToByte(H);
			s=Base.DecimalToByte(S);
			a=Base.DecimalToByte(A);
		}
		public void From(PixelYhsa pxNow) {
			Y=pxNow.Y;
			H=pxNow.H;
			S=pxNow.S;
			A=pxNow.A;
		}
		public void From(PixelYhs pxNow) {
			Y=pxNow.Y;
			H=pxNow.H;
			S=pxNow.S;
			A=Base.r1;
		}
		public void FromArgb(byte a, byte r, byte g, byte b) {
			Base.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//Base.RgbToYhs(out Y, out H, out S, (REAL)r, (REAL)g, (REAL)b);
			A=Base.DecimalToByte( (REAL)((REAL)a/(REAL)255.0) );
		}
		public void FromRgb(byte r, byte g, byte b) {
			Base.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//Base.RgbToYhs(out Y, out H, out S, (REAL)r, (REAL)g, (REAL)b);
			A=Base.r1;
		}
	}//end PixelYhsa
	
	public struct FPx {
		public float B;
		public float G;
		public float R;
		public float A;
	}
	public struct DPx {
		public double B;
		public double G;
		public double R;
		public double A;
	}
	public struct DPoint {
		public double X;
		public double Y;
		public DPoint(double xSet, double ySet) { X=xSet; Y=ySet; }
		public void Set(double xSet, double ySet) { X=xSet; Y=ySet; }
	}
	public struct FPoint {
		public float X;
		public float Y;
	}
	public struct RPoint {
		public REAL X;
		public REAL Y;
	}
	public struct DPoint3D {
		public double x;
		public double y;
		public double z;
	}
	public struct FPoint3D {
		public float x;
		public float y;
		public float z;
	}
	public struct RPoint3D {
		public REAL x;
		public REAL y;
		public REAL z;
	}

	public class FTrimmedAxis {
		public float NearLower;
		public float NearUpper;
		public float FarLower;
		public float FarUpper;
		public float abs;
		public float middle;
		private float temp;
		public float rationalaxis {
			get {
				temp=cropped;
				if (temp<middle) {
					return (temp-FarLower)/(NearLower-FarLower);
				}
				else if (temp>middle) {
					return (temp-NearUpper)/(FarUpper-NearUpper);
				}
				return 0;
			}
		}
		public FTrimmedAxis() {
			middle=0;
			FarLower=middle-1;
			NearLower=middle;
			NearUpper=middle;
			FarUpper=middle+1;
			abs=middle;
		}
		public float cropped {
			get {
				if (abs<middle) {
					if (abs>NearLower) return NearLower;
					else if (abs<FarLower) return FarLower;
				}
				else if (abs>middle) {
					if (abs<NearUpper) return NearUpper;
					else if (abs>FarUpper) return FarUpper;
				}
				return abs;
			}
		}
	}//end FTrimmedAxis class 
	public class FRange {
		public float min;
		public float max;
		public float abs;
		private float temp;
		public float ratio { //ratio forward from min toward max
			get {
				FixAbs();
				return (abs-min)/(max-min); 
				//i.e. if max=-1 and min=-4 and abs=-2, then ratio=2/3
				//i.e. if 
			}
		}
		public float ratioback { //ratio backward from max toward min
			get {
				FixAbs();
				temp=max-min;
				return (temp-(abs-min))/temp;
				//i.e. if max=-1 and min=-4 and abs=-2, then ratio=1/3
			}
		}
		public float range {
			get {
				return max-min;
			}
		}
		private void FixAbs() {
			if (abs<min) abs=min;
			else if (abs>max) abs=max;
		}
	}
	public class IPoint {
		public int X;
		public int Y;
		public IPoint() {
			X=0;
			Y=0;
		}
		public IPoint(int xSet, int ySet) {
			Set(xSet,ySet);
		}
		public IPoint(IPoint ipSet) {
			Set(ipSet);
		}
		public IPoint(IZone izoneTopLeft) {
			Set(izoneTopLeft.Left,izoneTopLeft.Top);
		}
		public void Set(IPoint ipSet) {
			try {
				X=ipSet.X;
				Y=ipSet.Y;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IPoint Set(IPoint)","constructing/setting point using other point");
			}
		}
		public void Set(int xSet, int ySet) {
			X=xSet;
			Y=ySet;
		}
		public override string ToString() {
			return "("+X.ToString()+","+Y.ToString()+")";
		}
		public static string Description(int x, int y) {
			return "("+x.ToString()+","+y.ToString()+")";
		}
		public string Description() {
			return Description(X,Y);
		}
		public IPoint Copy() {
			IPoint ipReturn=null;
			try {
				ipReturn=new IPoint();
				ipReturn.X=X;
				ipReturn.Y=Y;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IPoint copy");
			}
			return ipReturn;
		}
	}//end IPoint
	public class ILine {
		public int X1;
		public int Y1;
		public int X2;
		public int Y2;
		public ILine() {
			Set(0,0,0,0);
		}
		public ILine(int x1, int y1, int x2, int y2) {
			Set(x1, y1, x2, y2);
		}
		public void Set(int x1, int y1, int x2, int y2) {
			X1=x1; Y1=y1; X2=x2; Y2=y2;
		}
	}
	public class IRect {
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public int Right { get { return X+Width; } }
		public int Bottom { get { return Y+Height; } }
		public IRect() {
			Set(0,0,0,0);
		}
		public IRect(int iSetX, int iSetY, int iSetWidth, int iSetHeight) {
			Set(iSetX,iSetY,iSetWidth,iSetHeight);
		}
		public IRect Copy() {
			IRect rectReturn=new IRect(this.X,this.Y,this.Width,this.Height);
			//rectReturn.X=this.X;
			//rectReturn.Y=this.Y;
			//rectReturn.Width=this.Width;
			//rectReturn.Height=this.Height;
			return rectReturn;
		}
		public void Set(int xSet, int ySet, int iSetW, int iSetH) {
			X=xSet;
			Y=ySet;
			Width=iSetW;
			Height=iSetH;
		}
		public void Shrink(int iBy) {
			ShrinkRL(iBy);
			ShrinkBT(iBy);
		}
		public void ShrinkRL(int iBy) {
			if (iBy*2>Width) { iBy=Width/2; Width=0; }
			else Width-=iBy*2;
			X+=iBy;
		}
		public void ShrinkBT(int iBy) {
			if (iBy*2>Height) { iBy=Height/2; Height=0; }
			else Height-=iBy*2;
			Y+=iBy;
		}
		public void From(Rectangle rectNow) {
			try {
				X=rectNow.X;
				Y=rectNow.Y;
				Width=rectNow.Width;
				Height=rectNow.Height;
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "IRect From(Rectangle)","setting irect from framework rectangle");
			}
		}
		public static void Set(Rectangle rectNow, int xSet, int ySet, int iSetW, int iSetH) {
			try {
				rectNow.X=xSet;
				rectNow.Y=ySet;
				rectNow.Width=iSetW;
				rectNow.Height=iSetH;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IRect set","setting framework rectangle using IRect");
			}
		}
		public Rectangle ToRectangle() {
			Rectangle rectReturn=new Rectangle();
			rectReturn.X=X;
			rectReturn.Y=Y;
			rectReturn.Width=Width;
			rectReturn.Height=Height;
			return rectReturn;
		}
		public System.Drawing.Point ToPoint() {
			System.Drawing.Point pointReturn=new System.Drawing.Point();
			pointReturn.X=X;
			pointReturn.Y=Y;
			return pointReturn;
		}
		public override string ToString() {
			return "("+X.ToString()+","+Y.ToString()+","+Width.ToString()+","+Height.ToString()+")";
		}
		public static string Description(int xNow, int yNow, int iWidthNow, int iHeightNow) {
			return "["+iWidthNow.ToString()+"x"+iHeightNow.ToString()+"@("+xNow.ToString()+","+yNow.ToString()+")]";
		}
		public string Description() {
			return Description(X,Y,Width,Height);
		}
		public void Reset() {
			this.X=0;
			this.Y=0;
		}
		public bool Contains(int xLoc, int yLoc) {
			return xLoc>=X&&xLoc<Right&&yLoc>=Y&&yLoc<Bottom;
		}
	}//end IRect
	public struct DRect {
		public double X;
		public double Y;
		public double Width;
		public double Height;
		public double Right {
			get {
				return X+Width;
			}
		}
		public double Bottom {
			get {
				return Y+Height;
			}
		}
		public DRect(double iSetX, double iSetY, double iSetWidth, double iSetHeight) {
			X=iSetX;
			Y=iSetY;
			Width=iSetWidth;
			Height=iSetHeight;
		}
		public DRect Copy() {
			DRect rectReturn=new DRect();
			rectReturn.X=this.X;
			rectReturn.Y=this.Y;
			rectReturn.Width=this.Width;
			rectReturn.Height=this.Height;
			return rectReturn;
		}
		public Rectangle ToRectangle() {
			Rectangle rectReturn=new Rectangle();
			rectReturn.X=(int)X;
			rectReturn.Y=(int)Y;
			rectReturn.Width=(int)Width;
			rectReturn.Height=(int)Height;
			return rectReturn;
		}
		public System.Drawing.Point ToPoint() {
			System.Drawing.Point pointReturn=new System.Drawing.Point();
			pointReturn.X=(int)X;
			pointReturn.Y=(int)Y;
			return pointReturn;
		}
		public DPoint ToDPoint() {
			DPoint pointReturn;
			pointReturn.X=X;
			pointReturn.Y=Y;
			return pointReturn;
		}
		public void Reset() {
			this.X=0;
			this.Y=0;
		}
		public bool Contains(double xLoc, double yLoc) {
			return xLoc>=X&&xLoc<Right&&yLoc>=Y&&yLoc<Bottom;
		}
	}//end DRect
	public class IPoly {
		//Why to keep this: Region is hard to work with -- hard to get points out of it.
		#region vars
		IPoint[] parr=null;
		int iUsed;
		public int Maximum {
			get {
				return (parr!=null)?parr.Length:0;
			}
			set {
				if (value>0) {
					if (parr==null) {
						parr=new IPoint[value];
						for (int iNow=0; iNow<value; iNow++) parr[iNow]=null;
					}
					else {
						if (value<iUsed) {
							Base.ShowErr("Warning: points are being truncated","IPoly set Maximum","truncating points {"
								+"value:"+value.ToString()+"; "
								+"iUsed:"+iUsed.ToString()+"; "
								+"}");
							iUsed=value;
						}
						IPoint[] parrNew=new IPoint[value];
						for (int iNow=0; iNow<value; iNow++) {
							parrNew[iNow]=iNow<iUsed?parr[iNow]:null;
						}
						parr=parrNew;
					}
				}
				else {
					parr=null;
					if (value<0) Base.ShowErr("WARNING: IPoly Maximum set to negative value","IPoly set Maximum","setting maximum to negative number {value:"+value.ToString()+"}");
				}
			}//end set Maximum
		}//end Maximum
		#endregion vars
		#region constructors
		public IPoly() {
			Init(16);
		}
		public void MakeEmpty() {
			iUsed=0;
		}
		public void Init(int iMin) {
			iUsed=0;
			Maximum=iMin;
		}
		public void SetFuzzyMaxByLocation(int iLoc) {
			Maximum=Base.LocationToFuzzyMaximum(Maximum,iLoc);
		}
		#endregion constructors
		
		///<summary>
		///Checks if the figure contains point (x,y) assuming that the figure is 
		///implicitly (and not explicitly) closed nor has any duplicate points.
		///</summary>
		public bool Contains(int x, int y) {
			int iCount=0;
			//int iIntersectType=Base.LineRelationshipNone;
			int xTemp,yTemp;
			if (iUsed>=3) {
				IPoint pPrev=parr[iUsed-1];//using last point as prev assumes solid figure
				for (int iNow=0; iNow<iUsed; iNow++) {
					//iLineRelationship=Base.IntersectionAndRelationship(ref xTemp, ref yTemp, int.MaxValue, y, parr[iNow].X, parrNow[iNow].Y, pPrev.X, pPrev.Y);
					//if (iLineRelationship==Base.LineRelationshipIntersectionNotTouchingEndsOfLineB
					//	||iLineRelationship==Base.LineRelationshipLineBPoint1IsOnLineA) {
					//}
					if ( Base.Intersection(out xTemp, out yTemp, x, y, int.MaxValue, y, parr[iNow].X, parr[iNow].Y, pPrev.X, pPrev.Y, false)==Base.IntersectionYes
						&& Base.IsInBoxEx(xTemp, yTemp, parr[iNow].X, parr[iNow].Y, pPrev.X, pPrev.Y) //DOES re-order its copies of locations
						) iCount++;//ok since IsInBox prevents repetition
					pPrev=parr[iNow];
				}
			}
			return (iCount%2==1);
		}
		public bool Add(IPoint ipToKeep) {
			if (iUsed==Maximum) SetFuzzyMaxByLocation(iUsed);
			bool bGood=iUsed<Maximum;
			if (bGood) {
				parr[iUsed]=ipToKeep;
				iUsed++;
			}
			return bGood;
		}
		public bool AddCopy(IPoint ipToCopy) {
			if (iUsed==Maximum) SetFuzzyMaxByLocation(iUsed);
			bool bGood=iUsed<Maximum;
			if (bGood) {
				if (parr[iUsed]==null) parr[iUsed]=new IPoint(ipToCopy.X,ipToCopy.Y);
				else parr[iUsed].Set(ipToCopy);
				iUsed++;
			}
			return bGood;
		}
		public bool GetBounds(ref IRect rectReturn) {
			bool bGood=false;
			if (rectReturn==null) rectReturn=new IRect();
			int xMax=int.MinValue;
			int yMax=int.MinValue;
			rectReturn.Y=int.MaxValue;
			rectReturn.X=int.MaxValue;
			try {
				if (iUsed>0) {
					for (int iNow=0; iNow<iUsed; iNow++) {
						if (parr[iNow].X>xMax) xMax=parr[iNow].X;
						if (parr[iNow].Y>yMax) yMax=parr[iNow].Y;
						if (parr[iNow].X<rectReturn.X) rectReturn.X=parr[iNow].X;
						if (parr[iNow].Y<rectReturn.Y) rectReturn.Y=parr[iNow].Y;
					}
					bGood=true;
					rectReturn.Width=(xMax-rectReturn.X)+1;
					rectReturn.Height=(yMax-rectReturn.Y)+1;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IPoly GetBounds");
			}
			return bGood;
		}
		public IRect GetBounds() {
			IRect rectNow=new IRect();
			GetBounds(ref rectNow);
			return rectNow;
		}
	}//end IPoly
	
	
	
	
	
	
	public class IZone {
		public int Top;
		public int Left;
		public int Bottom;
		public int Right;
		public int Width {
			get { return Right-Left; }
		}
		public int Height {
			get { return Bottom-Top; }
		}
		public IZone() {
			Top=0;
			Left=0;
			Bottom=0;
			Right=0;
		}
		public IZone(IZone zoneSet) {
			Set(zoneSet);
		}
		public void Set(IZone zoneSet) {
			try {
				Left=zoneSet.Left;
				Top=zoneSet.Top;
				Right=zoneSet.Right;
				Bottom=zoneSet.Bottom;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IZone Set(IZone)","setting/constructing zone using other zone");
			}
		}
		public IZone(int iTop, int iLeft, int iRight, int iBottom) {
			Set(iLeft, iTop, iRight, iBottom);
		}
		public void Set(int iLeft, int iTop, int iRight, int iBottom) {
			Left=iLeft;
			Top=iTop;
			Right=iRight;
			Bottom=iBottom;
		}
		public void FromRect(int rect_X, int rect_Y, int rect_Width, int rect_Height) {
			Left=rect_X;
			Top=rect_Y;
			Right=rect_X+rect_Width;
			Bottom=rect_Y+rect_Height;
		}
		public IZone(IRect rectFrom) {
			From(rectFrom);
		}
		public void CopyTo(ref IRect rectNow) {
			if (rectNow==null) rectNow=new IRect(Left,Top,Right-Left,Bottom-Top);
			else {
				rectNow.X=Left;
				rectNow.Y=Top;
				rectNow.Width=Right-Left;
				rectNow.Height=Bottom-Top;
			}
		}
		public void From(IRect rectFrom) {
			From(rectFrom,false);
		}
		public void From(IRect rectFrom, bool bToInclusiveZone) {
			try {
				Left=rectFrom.X;
				Top=rectFrom.Y;
				Right=rectFrom.X+rectFrom.Width; //i.e. Width 1 makes exclusive rect of 1 width but inclusive rect of 2 width (fixed below)
				Bottom=rectFrom.Y+rectFrom.Height;
				if (bToInclusiveZone) {
					if (rectFrom.Height>0) Bottom-=1;
					else Base.ShowErr("Source rect height was "+rectFrom.Height.ToString()+" while setting IZone");
					if (rectFrom.Width>0) Right-=1;
					else Base.ShowErr("Source rect width was "+rectFrom.Width.ToString()+" while setting IZone");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IZone From(IRect)");
			}
		}
		public Rectangle ToRectangle() {
			Rectangle rectReturn=new Rectangle();
			rectReturn.X=Left;
			rectReturn.Y=Top;
			rectReturn.Width=Right-Left;
			if (rectReturn.Width<0) rectReturn.Width=0;
			rectReturn.Height=Top-Bottom;
			if (rectReturn.Height<0) rectReturn.Height=0;
			return rectReturn;
		}
		public System.Drawing.Point ToPoint() {
			System.Drawing.Point pointReturn=new System.Drawing.Point();
			pointReturn.X=Left;
			pointReturn.Y=Top;
			return pointReturn;
		}
		public override string ToString() {
			return "(("+Left.ToString()+","+Top.ToString()+"),("+Right.ToString()+","+Bottom.ToString()+")) ";
		}
		public static string Description(int left, int top, int right, int bottom) {
			return "[("+left.ToString()+","+top.ToString()+"):("+right.ToString()+","+bottom.ToString()+")]";
		}
		public string Description() {
			return Description(Top,Left,Right,Bottom);
		}
		public void CopyTo(ref IZone zoneX) {
			if (zoneX==null) zoneX=new IZone(this);
			else {
				zoneX.Top=Top;
				zoneX.Left=Left;
				zoneX.Bottom=Bottom;
				zoneX.Right=Right;
			}
		}
		public IZone Copy() {
			return new IZone(Top,Left,Right,Bottom);
		}//end Copy
		public bool Contains(int xLoc, int yLoc) {
			return xLoc>=Left&&xLoc<Right&&yLoc>=Top&&yLoc<Bottom;
		}
		public bool ContainsInclusiveBR(int xLoc, int yLoc) {
			return xLoc>=Left&&xLoc<=Right&&yLoc>=Top&&yLoc<=Bottom;
		}
		public bool Shrink(int iBy) {	
			return Shrink(iBy,false);
		}
		public bool Shrink(int iBy, bool bAsInclusiveZone) {
			return ShrinkRL(iBy, bAsInclusiveZone)&&ShrinkBT(iBy, bAsInclusiveZone);
		}
		public bool ShrinkRL(int iBy, bool bAsInclusiveZone) {
			bool bGood=true;
			if ( (Right-(bAsInclusiveZone?1:0)) - Left >= iBy*2) {
				Left+=iBy;
				Right-=iBy;
			}
			else {
				FlattenRL(bAsInclusiveZone);
				bGood=false;
			}
			return bGood;
		}
		public bool ShrinkBT(int iBy, bool bAsInclusiveZone) {
			bool bGood=true;
			if ( (Bottom-(bAsInclusiveZone?1:0)) - Top >= iBy*2) {
				Top+=iBy;
				Bottom-=iBy;
			}
			else {
				FlattenBT(bAsInclusiveZone);
				bGood=false;
			}
			return bGood;
		}
		public void FlattenBT(bool bAsInclusiveZone) {
			int iByNew=(Bottom-Top)/2;
			Bottom-=iByNew+((Bottom-Top)%2);
			Top+=iByNew;
			if (!bAsInclusiveZone) Bottom+=1;
		}
		public void FlattenRL(bool bAsInclusiveZone) {
			int iByNew=(Right-Left)/2;
			Right-=iByNew+((Right-Left)%2);
			Left+=iByNew;
			if (!bAsInclusiveZone) Right+=1;
		}
	} //end IZone
	
	#endregion simple types
	
	public class Base {
		//TODO:  add the string Base.sThrowTag to somewhere in ALL THROW STATEMENTS.
		#region variables
		public static bool bShowErrors=true;
		public static bool bShowWarnings=true;
		public static bool bWriteWarnings=false;
		public static bool bShowDebug=true;
		public static bool bWriteDebug=false;
		public const string sThrowTag="(manually thrown)";
		public static int iMaxAllocation=268435456;
			//1MB = 1048576 bytes
		public static Var settings=null; //global program settings //TODO: implement this EVERYWHERE
		public static readonly string[] sarrDigit=new string[] {"0","1","2","3","4","5","6","7","8","9"};
		public static readonly char[] carrDigit=new char[] {'0','1','2','3','4','5','6','7','8','9'};
		public static readonly char[] carrAlphabetUpper=new char[] {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
		public static readonly string[] sarrAlphabetUpper=new string[] {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"};
		public static readonly char[] carrAlphabetLower=new char[] {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
		public static readonly string[] sarrAlphabetLower=new string[] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"};
		public static MyCallback mcbNULL=null;
		public static string sRemoveSpacingBeforeNewLines1=" "+Environment.NewLine;
		public static string sRemoveSpacingBeforeNewLines2="\t"+Environment.NewLine;
		public static readonly string[] sarrConsonantLower=new string[] {"b","c","d","f","g","h","j","k","l","m","n","p","q","r","s","t","v","w","x","y","z"};
		public static readonly string[] sarrConsonantUpper=new string[] {"B","C","D","F","G","H","J","K","L","M","N","P","Q","R","S","T","V","W","X","Y","Z"};
		public static readonly string[] sarrVowelLower=new string[] {"a","e","i","o","u"};
		public static readonly string[] sarrVowelUpper=new string[] {"A","E","I","O","U"};
		public static readonly string[] sarrWordDelimiter=new string[] {"...","--",",",";",":","/","&","#","@","$","%","_","+","=","(",")","{","}","[","]","*",">","<","|","\"","'"," ","`","^","~"}; //CAN HAVE "'" since will simply be dealt differently when contraction, after words are split apart
		public static readonly char[] carrSentenceDelimiter=new char[] {'.','!','?'};//formerly string[] sarrSentenceDelimiter
		public static readonly char[] carrBase64=new char[] {
			'A','B','C','D','E','F','G','H','I','J','K','L','M',
			'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
			'a','b','c','d','e','f','g','h','i','j','k','l','m',
			'n','o','p','q','r','s','t','u','v','w','x','y','z',
			'0','1','2','3','4','5','6','7','8','9','+','/'
		};
		public static readonly char[] carrBase16=new char[] {
			'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'
		};
		public const byte byLowNibble = 0x0F;// 15; //mask 4 bits!
		public const byte byHighNibble = 0xF0;// 240; //mask 4 bits!
		//"[rX]" vars below exist to avoid type conversion
		public const short short_1=(short)-1;
		public const short short0=(short)0;
		public const ushort ushort0=(ushort)0;
		public const REAL r0=(REAL)0.0;
		public const REAL r_34414 = (REAL)0.34414;
		public const REAL r_71414 = (REAL).71414;
		public const REAL r1=(REAL)1.0;
		public const REAL r1_402 = (REAL)1.402;
		public const REAL r1_772 = (REAL)1.772;
		public const REAL r3=(REAL)3.0;
		public const REAL r90=(REAL)90.0;
		public const REAL r180=(REAL)180.0;
		public const REAL r255=(REAL)255.0;
		public const REAL r270=(REAL)270.0;
		public const float F180_DIV_PI=57.2957795130823208767981548141052F; // 180/PI;
		public const double D180_DIV_PI=57.2957795130823208767981548141052D;
		public const REAL R180_DIV_PI=(REAL)57.2957795130823208767981548141052;
		public const float FPI_DIV_180=0.017453293F; // 180/PI;
		public const double DPI_DIV_180=0.017453293D;
		public const REAL RPI_DIV_180=(REAL)0.017453293;
		public const int IntersectionError=-1;
		public const int IntersectionNo=0;
		public const int IntersectionYes=1;
		public const int IntersectionBeyondSegment=2;
		public const int LineRelationshipNone=0;
		public const int LineRelationshipParallelDifferentLine=1;
		public const int LineRelationshipParallelSameLine=2;
		public const int LineRelationshipIntersectionNotTouchingEndsOfLineB=3;
		public const int LineRelationshipLineBPoint1IsOnLineA=4;
		public const int LineRelationshipLineBPoint2IsOnLineA=5;
		public const int LineRelationshipIntersectionOutOfRange=6;
		private static int iElementsMaxDefault; //formerly iMaximumSeqDefault
		public static byte[][] SubtractBytes=null;//static constructor creates array of 65536 results of NON-WRAPPED byte subtraction operations
		public static int ElementsMaxDefault {
			get {
				return iElementsMaxDefault;
			}
			set {
				iElementsMaxDefault=value;
				if (settings==null) settings=new Var("settings",Var.TypeArray);
				settings.Set("ElementsMaxDefault",value);
			}
		}
		private static int iAbsoluteMaximumScriptArray; //formerly iElementsMaxDefault;
		public static int AbsoluteMaximumScriptArray {
			get {
				return iAbsoluteMaximumScriptArray;
			}
			set {
				iAbsoluteMaximumScriptArray=value;
				if (settings==null) settings=new Var("settings",Var.TypeArray);
				settings.Set("AbsoluteMaximumScriptArray",value);
			}
		}
		#endregion variables
		
		#region constructors
		//static constructor
		public static string sFileOutput="1.Output.txt";
		public static string sFileErrors="1.Error.txt";
		public static readonly string sFileSettings="base.ini";
		static Base() {
			settings=new Var("settings",Var.TypeArray);
			settings.Root=settings;
			try {
				if (File.Exists(sFileSettings)) {
					settings.LoadIni(sFileSettings);
				}
				if (File.Exists(sFileOutput)) File.Delete(sFileOutput);
				if (File.Exists(sFileErrors)) File.Delete(sFileErrors);
				
				settings.CreateOrIgnore("iVarsDefault",(int)32);
				settings.CreateOrIgnore("iVarsLimit",(int)65536);
				//GetOrCreate is only used for speed.  Others are accessed by Var conversion.
				Base.iElementsMaxDefault=100;
				settings.GetOrCreate(ref Base.iElementsMaxDefault, "ElementsMaxDefault");
				Base.iAbsoluteMaximumScriptArray=65536;
				settings.GetOrCreate(ref Base.iAbsoluteMaximumScriptArray, "AbsoluteMaximumScriptArray");
				settings.CreateOrIgnore("iLinesDefault",(int)32768);
				settings.CreateOrIgnore("iLinesLimit",(int)(int.MaxValue/2));
				settings.Comment("AutoGrowVarArray is not yet implemented.");
				settings.CreateOrIgnore("AutoGrowVarArray",true);//TODO: implement this
				settings.CreateOrIgnore("iCSVRowsLimit",(int)8000);
				settings.CreateOrIgnore("WriteTypesOnSecondRow",false);//TODO: implement this--row after first row, containing types i.e. int or int[], or decimal for money
				settings.CreateOrIgnore("ReadTypesOnSecondRow",true);
				settings.CreateOrIgnore("StringStackDefaultStartSize",(int)256);
				settings.CreateOrIgnore("StringQueueDefaultMaximumSize",(int)8192);
				settings.CreateOrIgnore("DefaultFont","./Library/Fonts/thepixone-12x16");
				settings.CreateOrIgnore("DefaultFontHeight",16);
				Base.Debug("Saving settings...");
				settings.SaveIni(sFileSettings);
				Base.Debug("done saving settings to \""+sFileSettings+"\".");
				SubtractBytes=new byte[256][];
				int iResult;
				for (int i1=0; i1<256; i1++) {//must be int because byte will never get to 256!
					SubtractBytes[i1]=new byte[256];
					for (int i2=0; i2<256; i2++) {//must be int because byte will never get to 256!
						iResult=i1-i2;
						SubtractBytes[i1][i2]=(iResult>=0)?(byte)iResult:by0;
					}
				}
				Base.Debug("Done Base init.");
			}
			catch (Exception exn) {//do not report this
				Base.IgnoreExn(exn,"","initializing utilities class");
			}
		}
		#endregion constructors

		#region retroengine-isms
		public static string VariableMessage(IRect val) {
			try {
				return (val!=null) ? val.Description() : "null" ;
			}
			catch {//do not report this
				return "incorrectly-initialized-irect";
			}
		}
		public static string VariableMessage(IZone val) {
			try {
				return (val!=null) ? val.Description() : "null" ;
			}
			catch {//do not report this
				return "incorrectly-initialized-izone";
			}
		}
		//NOTE: public static string VariableMessage(GBuffer val) { is in GBuffer*.cs!
		public static string VariableMessage(byte[] val) {
			try {
				return (val!=null) ? val.Length.ToString() : "null" ;
			}
			catch {//do not report this
				return "incorrectly-initialized-byte-array";
			}
		}
		public static string VariableMessageStyleOperatorAndValue(byte[] byarrNow) {
			string sMsg=VariableMessage(byarrNow);
			if (IsNumeric(sMsg,false,false)) sMsg=".Length:"+sMsg;
			else sMsg=":"+sMsg;
			return sMsg;
		}
		public static string VariableMessageStyleOperatorAndValue(string val, bool bShowStringIfValid) {
			string sMsg;//=VariableMessage(byarrToHex);
			sMsg=VariableMessage(val,bShowStringIfValid);
			if (!bShowStringIfValid && IsNumeric(sMsg,false,false)) sMsg=".Length:"+sMsg;
			else sMsg=":"+sMsg;
			return sMsg;
		}
		//public static string VariableMessage(string val) {
		//	return VariableMessage(val,false);
		//}
		public static string VariableMessage(string val, bool bShowStringIfValid) {
			try {
				return (val!=null)  
					?  ( bShowStringIfValid ? ("\""+val+"\"") : val.Length.ToString() )
					:  "null";
			}
			catch {//do not report this
				return "incorrectly-initialized-string";
			}
		}
		public static int LocationToFuzzyMaximum(int iCurrentMax, int iLoc) {
			if (iLoc<1) iLoc=2;//2 since multiplying by 1.5 later
			iLoc++;
			if (iCurrentMax<0) iCurrentMax=0;
			if (iLoc>iCurrentMax) iCurrentMax=SafeConvert.ToInt(SafeMultiply(SafeConvert.ToDouble(iLoc),(double)1.5));
			return iCurrentMax;
		}
		
		public static void CopySafe(byte[] byarrDest, byte[] byarrSrc) {
			try {
				if (byarrSrc!=null) {
					if (byarrDest==null||byarrDest.Length!=byarrSrc.Length) {
						byarrDest=new byte[byarrSrc.Length];
					}
					for (int iNow=0; iNow<byarrSrc.Length; iNow++) {
						byarrDest[iNow]=byarrSrc[iNow];
					}
				}
				else byarrDest=null;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Base CopySafe","copying byte array");
				byarrDest=null;
			}
		}
		/// <summary>
		/// Returns whether the string is a good retroengine return
		/// </summary>
		/// <param name="sResult">Must be a string returned by a RetroEngineFunction via an
		/// "out string" parameter.</param>
		/// <returns>Returns whether the variable indicated a successful run of a 
		/// function, assuming that your string was sent to a retroengine function
		/// that accepted it as "out string sResult"</returns>
		public static bool IsGoodReturn(string sResult) {
			return (sResult==null 
			        || sResult=="" 
			        || (
			            sResult.ToLower().StartsWith("success")
			            && !(sResult.ToLower().IndexOf("exception")>-1)
			           )
			       );
		}
		/// <summary>
		/// Returns whether the string is an exception string
		/// </summary>
		/// <param name="sResult">Must be a string returned by a RetroEngineFunction via an
		/// "out string" parameter.</param>
		/// <returns>Returns whether the variable indicates that an exception 
		/// occured, assuming that your string was sent to a retroengine function
		/// that accepted it as "out string sResult"</returns>
		public static bool IsException(string sResult) {
			return (sResult!=null 
			        && (sResult.ToLower().IndexOf("exception")>-1));
		}
		/// <summary>
		/// Returns whether the string is a exception string that came about through
		/// an intentionally-thrown retroengine exception.
		/// </summary>
		/// <param name="sResult">Must be a string returned by a RetroEngineFunction via an
		/// "out string" parameter.</param>
		/// <returns>Returns whether the variable indicates that an exception was
		/// thrown intentionally--assuming that your string was sent to a retroengine function
		/// that accepted it as "out string sResult"</returns>
		public static bool IsPredefinedExceptionResultString(string sResult) {
			return (sResult!=null 
			        && (sResult.IndexOf(Base.sThrowTag)>-1));
			//INTENTIONALLY case-sensitive considering how the Base.sThrowTag comes to exist in sResult
		}
		public static string GoodOutResultString() {
			return "";
		}
		public static int iErrors=0;
		public static int iMaxErrors=200;
		public static int iOutputs=0;
		public static int iMaxOutputs=200;
		public static int iMarker=0;
		public static bool bCreatedOutput=false;
		public static bool bCreatedError=false;
		private static string sErrNow="";
		public static string sLastFile="(no files loaded yet)";
		///<summary>
		///gets the last engine error
		///</summary>
		public static string sLastErr {
			get {
				string sReturn=sErrNow;
				sErrNow="";
				return sReturn;
			}
		}
		public static bool HasErr() {
			return IsUsedString(sErrNow);
		}
		public static void ClearErr() {
			sErrNow="";
		}
		//public static bool ShowErr() { //whether to show an error
		//	return ShowError();
		//}
		//public static bool ShowError() {
		//	try {
		//		Base.mcbStatus
		//	}
		//	catch (Exception exn) {Base.IgnoreExn(exn,"ShowErr");
		//	}
		//}
		public static void Write(string sMsg) {//TODO: remove this completely
			StreamWriter swNow;
			try {
				iOutputs++;
				if (iOutputs<iMaxOutputs) {
					swNow=File.AppendText(sFileOutput);
					swNow.Write(Marker+sMsg);
					swNow.Close();
				}
				else if (iOutputs==iMaxOutputs) {
					swNow=File.AppendText(sFileOutput);
					swNow.Write(Marker+"MAXIMUM MESSAGES REACHED--This is the last message that will be shown: "+sMsg);
					swNow.Close();
				}
			}
			catch (Exception exn) {
				try {
					Base.IgnoreExn(exn,"Base Write","trying to append output text file");
					if (!bCreatedOutput) {
						swNow=File.CreateText(sFileOutput);
						swNow.Write(Marker+sMsg);
						swNow.Close();
					}
					bCreatedOutput=true;
				}
				catch (Exception exn2) {
					Base.IgnoreExn(exn2,"Base Write","trying to create new output text file");//ignore since "error error"
				}
			}
		}//end Write
		public static void WriteLine() {
			Write(Environment.NewLine);
		}
		public static void WriteLine(string sMsg) {
			Write(sMsg+Environment.NewLine);
		}
		private static string Marker {
			get {
				int iReturn=iMarker;
				iMarker++;
				//return "/*"+iReturn.ToString()+"*/";
				return "";
			}
		}
		public static bool CompareAt(string sHaystack, char cNeedle, int iAtHaystack) {
			return sHaystack!=null&&iAtHaystack>=0&&iAtHaystack<sHaystack.Length&&sHaystack[iAtHaystack]==cNeedle;
		}
		public static bool CompareAt(string sHaystack, string sNeedle, int iAtHaystack) {
			bool bReturn=false;
			int iAbs=iAtHaystack;
			int iMatches=0;
			try {
				//if (sHaystack!=null && sNeedle!=null) {
				if (sNeedle!=null&&sNeedle!="") {
					for (int iRel=0; iRel<sNeedle.Length; iRel++) {
						if (sNeedle[iRel]==sHaystack[iAbs]) iMatches++;
						else break;
						iAbs++;
					}
					if (iMatches==sNeedle.Length) bReturn=true;
				}
				//}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Base CompareAt(string,...)","comparing text substring");
				bReturn=false;
			}
			return bReturn;
		}//end CompareAt(string,...)
		//public static bool CompareAt(char[] carrHaystack, char[] carrNeedle, int iAtHaystack) {
		//	bool bReturn=false;
		//	int iAbs=iAtHaystack;
		//	int iMatches=0;
		//	try {
		//		//if (carrHaystack!=null && carrNeedle!=null) {
		//		if (carrNeedle!=null) {
		//			for (int iRel=0; iRel<carrNeedle.Length; iRel++) {
		//				if (carrNeedle[iRel]==carrHaystack[iAbs]) iMatches++;
		//				else break;
		//				iAbs++;
		//			}
		//			if (iMatches==carrNeedle.Length) {
		//				bReturn=true;
		//			}
		//		}
		//		//}
		//	}
		//	catch (Exception exn) {
		//		Base.ShowExn(exn,"Base CompareAt(character array,...)","comparing text substring");
		//		bReturn=false;
		//	}
		//	return bReturn;
		//}//end CompareAt(char array...);
		public static bool ReadLine(out string sReturn, string sAllData, ref int iMoveMe) {
			bool HasALine=false;
			bool bNewLine=false;
			sReturn="";
			try {
				
				int iStart=iMoveMe;
				if (iMoveMe<sAllData.Length) HasALine=true;
				while (iMoveMe<sAllData.Length) {//i.e. could be starting at 0 when length is 1!
					//string sTemp=SafeSubstring(sAllData,iMoveMe,Environment.NewLine.Length);
					if (CompareAt(sAllData,Environment.NewLine,iMoveMe)) {
						bNewLine=true;
						break;
					}
					else iMoveMe++;
				}
				if (!bNewLine) iMoveMe=sAllData.Length;//run to end if started after last newline (or there is no newline)
				sReturn=SafeSubstring(sAllData,iStart,iMoveMe-iStart);
				//Base.Debug("Base Read line ["+iStart.ToString()+"]toExcludingChar["+iMoveMe.ToString()+"]:"+sReturn);
				if (bNewLine) iMoveMe+=Environment.NewLine.Length;
			}
			catch (Exception exn) {
				Base.ShowException(exn,"Base.ReadLine");
			}
			return HasALine;
		}
		public static void StringToConsoleError(string sMsg) {
			string sLine="";
			int iCursor=0;
			if (Contains(sMsg,'\n')||Contains(sMsg,'\r')) {
				while (Base.ReadLine(out sLine, sMsg, ref iCursor)) {
					Console.Error.WriteLine(sMsg);
				}
			}
			else Console.Error.Write(sMsg);
		}
		public static void Error_Write(string sMsg) {//TODO: make a new object to avoid underscore garbage
			StreamWriter swNow;
			try {
				if (iErrors<iMaxErrors) {
					swNow=File.AppendText(sFileErrors);
					swNow.Write(Marker+sMsg);
					if (bShowErrors) StringToConsoleError(sMsg);
					swNow.Close();
				}
				else if (iErrors==iMaxErrors) {
					swNow=File.AppendText(sFileErrors);
					swNow.Write(Marker+"MAXIMUM MESSAGES REACHED--This is the last message that will be shown: "+sMsg);
					swNow.Close();
				}
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base Error_Write","appending error text file");
				try {
					if (!bCreatedError) {
						swNow=File.CreateText(sFileErrors);
						swNow.Write(Marker+sMsg);
						swNow.Close();
					}
					bCreatedError=true;
				}
				catch (Exception exn2) {
					Base.IgnoreExn(exn2,"Base Error_Write","creating new error text file instead");//ignore since "error error"
				}
			}
			iErrors++;
		}//end Error_Write
		public static void Error_WriteLine() {
			Error_Write(Environment.NewLine);
		}
		public static void Error_WriteLine(string sMsg) {
			Error_Write(sMsg+Environment.NewLine);
		}
		public static bool ShowErr() {
			bool bGood=true;
			Error_WriteLine();
			return bGood;
		}
		public static bool ShowError() {
			return ShowErr();
		}
		public static void Warning(string sMsg) {
			Warning(sMsg,"");
		}
		public static void Warning(string sMsg, string sAnyPertinentVarValuesAsCSSCurlyBracesBlock) {
			try {
				if (sAnyPertinentVarValuesAsCSSCurlyBracesBlock!=""&&!sAnyPertinentVarValuesAsCSSCurlyBracesBlock.StartsWith("{")&&!sAnyPertinentVarValuesAsCSSCurlyBracesBlock.StartsWith(" {")) sAnyPertinentVarValuesAsCSSCurlyBracesBlock="{"+sAnyPertinentVarValuesAsCSSCurlyBracesBlock+"}";
				if (!sMsg.ToUpper().StartsWith("WARNING")) sMsg="WARNING:"+sMsg+Base.PrecedeByIfNotBlank(" ",Base.SafeString(sAnyPertinentVarValuesAsCSSCurlyBracesBlock,false));
				if (bWriteWarnings) Error_WriteLine(sMsg);
				if (bShowWarnings) Console.WriteLine(sMsg);				
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Warning","showing warning");
			}
		}
		public static void Debug(string sMsg) {
			Debug(sMsg,"");
		}
		public static void Debug(string sMsg, string sAnyPertinentVarValuesAsCSSCurlyBracesBlock) {
			try {
				if (sAnyPertinentVarValuesAsCSSCurlyBracesBlock!=""&&!sAnyPertinentVarValuesAsCSSCurlyBracesBlock.StartsWith("{")&&!sAnyPertinentVarValuesAsCSSCurlyBracesBlock.StartsWith(" {")) sAnyPertinentVarValuesAsCSSCurlyBracesBlock="{"+sAnyPertinentVarValuesAsCSSCurlyBracesBlock+"}";
				if (!sMsg.ToUpper().StartsWith("#")) sMsg="#"+sMsg+Base.PrecedeByIfNotBlank(" ",Base.SafeString(sAnyPertinentVarValuesAsCSSCurlyBracesBlock,false));
				if (bWriteDebug) Error_WriteLine(sMsg);
				if (bShowDebug) Console.WriteLine(sMsg);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Debug","showing debug message");
			}
		}
		public static void ShowErr(string sMsg) {
			ShowErr(sMsg,"","");
		}
		public static void ShowError(string sMsg) {
			ShowErr(sMsg);
		}
		public static void ShowErr(string sMsg, string sFuncNow) {
			//Error_WriteLine("Error in "+sFuncNow+": "+sMsg);
			ShowErr(sMsg,sFuncNow,"");
		}
		public static void ShowError(string sMsg, string sFuncNow) {
			ShowErr(sMsg, sFuncNow);
		}
		public static void ShowErr(string sMsg, string sFuncNow, string sVerbNow) {
			string sErrString="";
			if (sFuncNow!="") sErrString=(Contains(sMsg,"Warning")?"Warning":"Error")+InWhileString(sFuncNow,sVerbNow)+sMsg;
			else if (sVerbNow!=null&&sVerbNow!="") sErrString="Error while "+sVerbNow+": "+sMsg;
			else sErrString=sMsg;//if (sMsg!="") sErrString=(sMsg);
			if (sErrString!="") {
				sErrNow+=sErrString;
				if (sErrNow.EndsWith(".")) sErrNow+=".  ";
				else sErrNow+="  ";
			}
			else sErrNow+="Problem reporting an error.  ";
			Error_WriteLine(sLastFile!=""?("[Last file: \""+sLastFile+"\"] "+sErrString):sErrString);
			sLastFile="";
		}
		public static void ShowError(string sMsg, string sFuncNow, string sVerbNow) {
			ShowError(sMsg, sFuncNow, sVerbNow);
		}
		public static bool IsUsedString(string sNow) {
			return sNow!=null&&sNow!="";
		}
		public static string InWhileString(string sFuncNow, string sVerbNow) {
			return (IsUsedString(sFuncNow)?" in "+sFuncNow:"")+(IsUsedString(sVerbNow)?(" while "+sVerbNow+": "):((IsUsedString(sFuncNow)||IsUsedString(sVerbNow))?": ":""));
		}
		public static void ShowExn(Exception exn, string sFuncNow, string sVerbNow) {
			string sErrString="";
			if (exn!=null) sErrString="Exception Error"+InWhileString(sFuncNow,sVerbNow)+": "+exn.ToString();
			if (IsUsedString(sErrString)) {
				sErrNow+=sErrString;
				if (sErrNow.EndsWith(".")) sErrNow+=".  ";
				else sErrNow+="  ";
			}
			else sErrNow+="Problem reporting exception"+(IsUsedString(sFuncNow)?(" in "+sFuncNow):"")+(IsUsedString(sVerbNow)?(" while "+sVerbNow):"")+")";
			Error_WriteLine(sLastFile!=""?("[Last file: \""+sLastFile+"\"] "+sErrString):sErrString);
			sLastFile="";
		}
		public static void ShowException(Exception exn, string sFuncNow, string sVerbNow) {
			ShowExn(exn,sFuncNow,sVerbNow);
		}
		public static void ShowExn(Exception exn, string sFuncNow) {
			ShowExn(exn,sFuncNow,"");//Error_WriteLine("Exception Error in "+sFuncNow+": "+exn.ToString());
		}
		public static void ShowException(Exception exn, string sFuncNow) {
			ShowExn(exn, sFuncNow);
		}
		public static void IgnoreExn(Exception exn) {
			IgnoreExn(exn, "unrecorded method", "");
		}
		public static void IgnoreExn(Exception exn, string sFuncNow) {
			IgnoreExn(exn, sFuncNow, "");
		}
		public static void IgnoreExn(Exception exn, string sFuncNow, string sVerbNow) {
		}
		//public static void ShowException(Exception exn, string sFunction) {
		//	ShowException(exn,sFunction,"");
		//}
		//public static void ShowException(Exception exn, string sFunction, string sVerbNow) {
		//	try {
		//		string sMsg="Exception error";
		//		if (sFunction!=null && sFunction.Length>0) sMsg+=" in "+sFunction;
		//		if (sVerbNow!=null && sVerbNow.Length>0) sMsg+=" while "+sVerbNow;
		//		if (exn!=null) sMsg=": "+exn.ToString();
		//		Console.Error.WriteLine(sMsg);
		//	}
		//	catch (Exception exn2) {
		//		Console.Error.WriteLine("Self-exception in ShowException: "+exn2.ToString());
		//	}
		//}
		public static readonly float fDegByteableActualMax=(255.0F/256.0F)*360.0F;
		public static readonly float fDegByteableRoundableExclusiveMax=(255.0F/256.0F)*360.0F + (360.0F-(255.0F/256.0F)*360.0F)/2.0F;
		public static readonly double dDegByteableActualMax=(255.0/256.0)*360.0;
		public static readonly double dDegByteableRoundableExclusiveMax=(255.0/256.0)*360.0 + (360.0-(255.0/256.0)*360.0)/2.0;
		public static readonly decimal mDegByteableActualExclusiveMax=(255.0M/256.0M)*360.0M;
		public static readonly decimal mDegByteableRoundableExclusiveMax=(255.0M/256.0M)*360.0M + (360.0M-(255.0M/256.0M)*360.0M)/2.0M;
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(float val) {//DegreesToByte
			float valTemp=SafeAngle360(val);
			if (valTemp>=fDegByteableRoundableExclusiveMax) return 0;
			else return SafeConvert.ToByte_1As255((valTemp/360.0F)*256.0F); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
		}
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(double val) {//DegreesToByte
			double valTemp=SafeAngle360(val);
			if (valTemp>=dDegByteableRoundableExclusiveMax) return 0;
			else return SafeConvert.ToByte_1As255((valTemp/360.0)*256.0); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
		}
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(decimal val) {//DegreesToByte
			decimal valTemp=SafeAngle360(val);
			if (valTemp>=mDegByteableRoundableExclusiveMax) return 0;
			else return SafeConvert.ToByte_1As255((valTemp/360.0M)*256.0M); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
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
		//	else return (byte)(val*r255);
		//}
		//public static byte DecimalToByte(REAL valToLimitBetweenZeroAndOne_AndMultiplyTimes255) {
		// NOT NEEDED, since REAL is naturally passed to float or other overload
		//	return DecimalToByte(valToLimitBetweenZeroAndOne_AndMultiplyTimes255);//SafeByte(valToLimitBetweenZeroAndOne_AndMultiplyTimes255*r255);
		//}
		//public static REAL ByteToReal(byte byZeroTo255AsZeroToOne) {
		//	return (REAL)((REAL)byZeroTo255AsZeroToOne/r255);
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
		
		public static REAL ByteToReal(byte val) {
			return (REAL)val/r255;
		}
		public static float ByteToFloat(byte val) {
			return (float)val/255.0f;
		}
		public static double ByteToDouble(byte val) {
			return (double)val/255.0;
		}
		public static decimal ByteToDecimal(byte val) { //formerly conceived as DecimalOfByte or DecimalFromByte
			return (decimal)val/255.0M;
		}
		#endregion retroengine-isms
		
		
		#region colorspace functions
		public static readonly REAL r_299=(REAL).299;
		public static readonly REAL r_587=(REAL).587;
		public static readonly REAL r_114=(REAL).114;
		public static readonly REAL r__16874=(REAL)(-.16874);
		public static readonly REAL r_33126=(REAL).33126;
		public static readonly REAL r_5=(REAL).5;
		public static readonly REAL r_41869=(REAL).41869;
		public static readonly REAL r_08131=(REAL).08131;
		
		public static readonly byte by0=(byte)0;
		public static readonly byte by255=(byte)255;
		public static float SafeAngle360(float valNow) {
			return Wrap(valNow, 0, 360);
		}
		public static double SafeAngle360(double valNow) {
			return Wrap(valNow, 0, 360);
		}
		public static decimal SafeAngle360(decimal valNow) {
			return Wrap(valNow, 0, 360);
		}
		public static void HsvToRgb(out byte R, out byte G, out byte B, ref float H, ref float S, ref float V) {
			//reference: easyrgb.com
			if ( S == 0.0f ) {                       //HSV values = 0 ÷ 1
				R = (byte) (V * 255.0f);                  //RGB results = 0 ÷ 255
				G = (byte) (V * 255.0f);
				B = (byte) (V * 255.0f);
			}
			else {
				float var_h = H * 6.0f;
				if ( var_h == 6.0f ) var_h = 0.0f;      //H must be < 1
				float var_i = Floor( var_h );             //Or ... var_i = floor( var_h )
				float var_1 = V * ( 1.0f - S );
				float var_2 = V * ( 1.0f - S * ( var_h - var_i ) );
				float var_3 = V * ( 1.0f - S * ( 1.0f - ( var_h - var_i ) ) );
				float var_r,var_g,var_b;
				if      ( var_i == 0.0f ) { var_r = V     ; var_g = var_3 ; var_b = var_1; }
				else if ( var_i == 1.0f ) { var_r = var_2 ; var_g = V     ; var_b = var_1; }
				else if ( var_i == 2.0f ) { var_r = var_1 ; var_g = V     ; var_b = var_3; }
				else if ( var_i == 3.0f ) { var_r = var_1 ; var_g = var_2 ; var_b = V;     }
				else if ( var_i == 4.0f ) { var_r = var_3 ; var_g = var_1 ; var_b = V;     }
				else                      { var_r = V     ; var_g = var_1 ; var_b = var_2; }
				R = (byte) (var_r * 255.0f);                  //RGB results = 0 ÷ 255
				G = (byte) (var_g * 255.0f);
				B = (byte) (var_b * 255.0f);
			}			
		}//end HsvToRgb float
		public static void HsvToRgb(out byte R, out byte G, out byte B, ref double H, ref double S, ref double V) {
			//reference: easyrgb.com
			if ( S == 0.0 ) {                       //HSV values = 0 ÷ 1
				R = (byte) (V * 255.0);                  //RGB results = 0 ÷ 255
				G = (byte) (V * 255.0);
				B = (byte) (V * 255.0);
			}
			else {
				double var_h = H * 6.0;
				if ( var_h == 6.0 ) var_h = 0.0;      //H must be < 1
				double var_i = System.Math.Floor( var_h );             //Or ... var_i = floor( var_h )
				double var_1 = V * ( 1.0 - S );
				double var_2 = V * ( 1.0 - S * ( var_h - var_i ) );
				double var_3 = V * ( 1.0 - S * ( 1.0 - ( var_h - var_i ) ) );
			
				double var_r,var_g,var_b;
				if      ( var_i == 0.0 ) { var_r = V     ; var_g = var_3 ; var_b = var_1; }
				else if ( var_i == 1.0 ) { var_r = var_2 ; var_g = V     ; var_b = var_1; }
				else if ( var_i == 2.0 ) { var_r = var_1 ; var_g = V     ; var_b = var_3; }
				else if ( var_i == 3.0 ) { var_r = var_1 ; var_g = var_2 ; var_b = V;     }
				else if ( var_i == 4.0 ) { var_r = var_3 ; var_g = var_1 ; var_b = V;     }
				else                   { var_r = V     ; var_g = var_1 ; var_b = var_2; }
			
				R = (byte) (var_r * 255.0);                  //RGB results = 0 ÷ 255
				G = (byte) (var_g * 255.0);
				B = (byte) (var_b * 255.0);
			}			
		}//end HsvToRgb double
		
		public static void RgbToHsv(out byte H, out byte S, out byte V, ref byte R, ref byte G, ref byte B) {
			float h, s, v;
			RgbToHsv(out h, out s, out v, ref R, ref G, ref B);
			H=SafeConvert.ToByte_1As255(h);
			S=SafeConvert.ToByte_1As255(s);
			V=SafeConvert.ToByte_1As255(v);
		}
		public static void RgbToHsv(out float H, out float S, out float V, ref byte R, ref byte G, ref byte B) {
			//reference: easyrgb.com
			float R_To1 = ( (float)R / 255.0f );                     //RGB values = 0 ÷ 255
			float G_To1 = ( (float)G / 255.0f );
			float B_To1 = ( (float)B / 255.0f );
			RgbToHsv(out H, out S, out V, ref R_To1, ref G_To1, ref B_To1);
		}//end RgbToHsv float
		
		public static void RgbToHsv(out float H, out float S, out float V, ref float R_To1, ref float G_To1, ref float B_To1) {
			//reference: easyrgb.com
			
			float var_Min =  (R_To1<G_To1) ? ((R_To1<B_To1)?R_To1:B_To1) : ((G_To1<B_To1)?G_To1:B_To1) ;    //Min. value of RGB
			float var_Max =  (R_To1>G_To1) ? ((R_To1>B_To1)?R_To1:B_To1) : ((G_To1>B_To1)?G_To1:B_To1) ;    //Max. value of RGB
			float del_Max = var_Max - var_Min;             //Delta RGB value
			
			V = var_Max;
			
			if ( del_Max == 0.0f ) {                     //This is a gray, no chroma...
				H = 0.0f;//only must be assigned since it's an "out" param   //HSV results = 0 ÷ 1
				S = 0.0f;
			}
			else {                                   //Chromatic data...
				S = del_Max / var_Max;
				float del_R = ( ( ( var_Max - R_To1 ) / 6.0f ) + ( del_Max / 2.0f ) ) / del_Max;
				float del_G = ( ( ( var_Max - G_To1 ) / 6.0f ) + ( del_Max / 2.0f ) ) / del_Max;
				float del_B = ( ( ( var_Max - B_To1 ) / 6.0f ) + ( del_Max / 2.0f ) ) / del_Max;
			
				if      ( R_To1 == var_Max ) H = del_B - del_G;
				else if ( G_To1 == var_Max ) H = ( 1.0f / 3.0f ) + del_R - del_B;
				else if ( B_To1 == var_Max ) H = ( 2.0f / 3.0f ) + del_G - del_R;
				else H=0.0f;//must assign, but only since it's an "out" param
				if ( H < 0.0f ) H += 1.0f;
				if ( H > 1.0f ) H -= 1.0f;
			}			
		}//end RgbToHsv float
		
		public static void RgbToHsv(out double H, out double S, out double V, ref byte R, ref byte G, ref byte B) {
			//reference: easyrgb.com
			double R_To1 = ( (double)R / 255.0 );                     //RGB values = 0 ÷ 255
			double G_To1 = ( (double)G / 255.0 );
			double B_To1 = ( (double)B / 255.0 );
			
			double var_Min =  (R_To1<G_To1) ? ((R_To1<B_To1)?R_To1:B_To1) : ((G_To1<B_To1)?G_To1:B_To1) ;    //Min. value of RGB
			double var_Max =  (R_To1>G_To1) ? ((R_To1>B_To1)?R_To1:B_To1) : ((G_To1>B_To1)?G_To1:B_To1) ;    //Max. value of RGB
			double del_Max = var_Max - var_Min;             //Delta RGB value
			
			V = var_Max;
			
			if ( del_Max == 0.0 ) {                     //This is a gray, no chroma...
				H = 0.0;//only must be assigned since it's an "out" param   //HSV results = 0 ÷ 1
				S = 0.0;
			}
			else {                                   //Chromatic data...
				S = del_Max / var_Max;
				double del_R = ( ( ( var_Max - R_To1 ) / 6.0 ) + ( del_Max / 2.0 ) ) / del_Max;
				double del_G = ( ( ( var_Max - G_To1 ) / 6.0 ) + ( del_Max / 2.0 ) ) / del_Max;
				double del_B = ( ( ( var_Max - B_To1 ) / 6.0 ) + ( del_Max / 2.0 ) ) / del_Max;
			
				if      ( R_To1 == var_Max ) H = del_B - del_G;
				else if ( G_To1 == var_Max ) H = ( 1.0 / 3.0 ) + del_R - del_B;
				else if ( B_To1 == var_Max ) H = ( 2.0 / 3.0 ) + del_G - del_R;
				else H=0.0;//must assign, but only since it's an "out" param
				if ( H < 0.0 ) H += 1.0;
				if ( H > 1.0 ) H -= 1.0;
			}			
		}//end RgbToHsv double
		
		public static void RgbToYCF(out float Y, out float Cb, out float Cr, ref float b, ref float g, ref float r) {
			Y  = .299f*r + .587f*g + .114f*b;
			Cb = -.16874f*r - .33126f*g + .5f*b;
			Cr = .5f*r - .41869f*g - .08131f*b; 
		}
		public static void RgbToYC(out REAL Y, out REAL Cb, out REAL Cr, ref REAL b, ref REAL g, ref REAL r) {
			Y  = r_299*r + r_587*g + r_114*b;
			Cb = r__16874*r - r_33126*g + r_5*b;
			Cr = r_5*r - r_41869*g - r_08131*b; 
		}
		public static float Chrominance(ref byte r, ref byte g, ref byte b) {
			return .299f*r + .587f*g + .114f*b;
		}
		public static decimal ChrominanceD(ref byte r, ref byte g, ref byte b) {
			return .299M*(decimal)r + .587M*(decimal)g + .114M*(decimal)b;
		}
		public static REAL ChrominanceR(ref byte r, ref byte g, ref byte b) {
			return r_299*(REAL)r + r_587*(REAL)g + r_114*(REAL)b;
		}
		
		public static void YCToRgbF(out byte r, out byte g, out byte b, ref float Y, ref float Cb, ref float Cr) {
			r = (byte)( Y + 1.402f*Cr );
			g = (byte)( Y - 0.34414f*Cb - .71414f*Cr );
			b = (byte)( Y + 1.772f*Cb );
		}
		public static void YCToRgb(out byte r, out byte g, out byte b, ref REAL Y, ref REAL Cb, ref REAL Cr) {
			r = (byte)( Y + r1_402*Cr );
			g = (byte)( Y - r_34414*Cb - r_71414*Cr );
			b = (byte)( Y + r1_772*Cb );
		}
		
		public static void YCToYhsF_YhsAsPolarYC(out float y, out float h, out float s, ref float Y, ref float Cb, ref float Cr) {
			y=Y/255.0f;
			RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=255.0f;
			h/=255.0f;
			//TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		public static void YCToYhs_YhsAsPolarYC(out REAL y, out REAL h, out REAL s, ref REAL Y, ref REAL Cb, ref REAL Cr) {
			y=Y/r255;
			RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=r255;
			h/=r255;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		
		public static void CbCrToHsF_YhsAsPolarYC(out float h, out float s, float Cb, float Cr) {
			RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=255.0f;
			h/=255.0f;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		public static void CbCrToHs_YhsAsPolarYC(out REAL h, out REAL s, REAL Cb, REAL Cr) {
			RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=r255;
			h/=r255;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		
		public static void HsToCbCrF_YhsAsPolarYC(out float Cb, out float Cr, ref float h, ref float s) {
			PolarToRect(out Cb, out Cr, s*255.0f, h*255.0f);
			 //TODO: finish this --- assume contract hs from zero to 1 (OR use a var that denotes max)
		}
		public static void HsToCbCr_YhsAsPolarYC(out REAL Cb, out REAL Cr, ref REAL h, ref REAL s) {
			PolarToRect(out Cb, out Cr, s*r255, h*r255);
			 //TODO: finish this --- assume contract hs from zero to 1 (OR use a var that denotes max)
		}
		
		public static void RgbToYhsF_YhsAsPolarYC(out float y, out float h, out float s, ref float r, ref float g, ref float b) {
			y=(.299f*r + .587f*g + .114f*b)/255.0f;
			CbCrToHs_YhsAsPolarYC(out h, out s, -.16874f*r - .33126f*g + .5f*b, .5f*r - .41869f*g - .08131f*b);
		}
		public static void RgbToYhs_YhsAsPolarYC(out REAL y, out REAL h, out REAL s, REAL r, REAL g, REAL b) {
			y=((REAL).299*r + (REAL).587*g + (REAL).114*b)/r255;
			CbCrToHs_YhsAsPolarYC(out h, out s, -.16874f*r - .33126f*g + .5f*b, .5f*r - .41869f*g - .08131f*b);
		}
		
		public static void YhsToYCF_YhsAsPolarYC(out float Y, out float Cb, out float Cr, ref float y, ref float h, ref float s) {
			Y=y*255.0f;
			HsToCbCr_YhsAsPolarYC(out Cb, out Cr, ref h, ref s);
		}
		public static void YhsToYC_YhsAsPolarYC(out REAL Y, out REAL Cb, out REAL Cr, ref REAL y, ref REAL h, ref REAL s) {
			Y=y*r255;
			HsToCbCr_YhsAsPolarYC(out Cb, out Cr, ref h, ref s);
		}
		
		private static REAL rTempY;
		private static REAL rTempCb;
		private static REAL rTempCr;
		public static void YhsToRgLine2_YhsAsPolarYC(out byte r, out byte g, out byte b, REAL y, REAL h, REAL s) {
			YhsToYC_YhsAsPolarYC(out rTempY, out rTempCb, out rTempCr, ref y, ref h, ref s);
			YCToRgb(out r, out g, out b, ref rTempY, ref rTempCb,  ref rTempCr);
		}
		public static void YhsToRgLine2_YhsAsPolarYC(out byte r, out byte g, out byte b, PixelYhs pxSrc) {
			YhsToYC_YhsAsPolarYC(out rTempY, out rTempCb, out rTempCr, ref pxSrc.Y, ref pxSrc.H, ref pxSrc.S);
			YCToRgb(out r, out g, out b, ref rTempY, ref rTempCb,  ref rTempCr);
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
				if ((float)Base.Dist(colorDest.A,colorSrc.A)>0.0f) fReturn+=( (float)Base.Dist(colorResult.A,colorSrc.A)/(float)Base.Dist(colorDest.A,colorSrc.A) );
				else fReturn+=colorResult.A==colorDest.A?1.0f:0.0f;
			}
			if ((float)Base.Dist(colorDest.R,colorSrc.R)>0.0f) fReturn+=( (float)Base.Dist(colorResult.R,colorSrc.R)/(float)Base.Dist(colorDest.R,colorSrc.R) );
			else fReturn+=colorResult.R==colorDest.R?1.0f:0.0f;
			if ((float)Base.Dist(colorDest.G,colorSrc.G)>0.0f) fReturn+=( (float)Base.Dist(colorResult.G,colorSrc.G)/(float)Base.Dist(colorDest.G,colorSrc.G) );
			else fReturn+=colorResult.G==colorDest.G?1.0f:0.0f;
			if ((float)Base.Dist(colorDest.B,colorSrc.B)>0.0f) fReturn+=( (float)Base.Dist(colorResult.B,colorSrc.B)/(float)Base.Dist(colorDest.B,colorSrc.B) );
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
				bDoAlphaAlpha?(int)Base.Approach((float)colorDest.A, (float)colorSrc.A, fAlphaTo1):(int)colorDest.A,
				(int)Base.Approach((float)colorDest.R, (float)colorSrc.R, fAlphaTo1),
				(int)Base.Approach((float)colorDest.G, (float)colorSrc.G, fAlphaTo1),
				(int)Base.Approach((float)colorDest.B, (float)colorSrc.B, fAlphaTo1)
				);
		}
		public static Pixel32 AlphaBlendColor(Pixel32 colorSrc, Pixel32 colorDest, float fAlphaTo1, bool bDoAlphaAlpha) {
			return Pixel32.FromArgb(
				bDoAlphaAlpha?(int)Base.Approach((float)colorDest.A, (float)colorSrc.A, fAlphaTo1):(int)colorDest.A,
				(byte)Base.Approach((float)colorDest.R, (float)colorSrc.R, fAlphaTo1),
				(byte)Base.Approach((float)colorDest.G, (float)colorSrc.G, fAlphaTo1),
				(byte)Base.Approach((float)colorDest.B, (float)colorSrc.B, fAlphaTo1)
				);
		}
		#endregion colorspace methods
	
		#region text functions (#region string methods)
		public static string ToString(Rectangle rectNow) {
			return (rectNow!=null) ?
				("rect{("+rectNow.Left.ToString()+","+rectNow.Top.ToString()+"):["+rectNow.Width.ToString()+"x"+rectNow.Height.ToString()+"]}")
				: "{nullrect}";
		}
		public string[] ArgsToQuotedArgs(string[] sarrRaw) {
			//re-parses aggressively-split-by-space program arguments
			string sTemp="";
			int iFound=0;
			string[] sarrReturn=null;
			try {
				int iNow;
				for (iNow=0; iNow<sarrRaw.Length; iNow++) {
					if (iNow!=0) sTemp+=" ";
					sTemp+=sarrRaw[iNow];
				}
				sarrReturn=Base.SplitCSV(sTemp,' ','\"');
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ArgsToQuotedArgs");
			}
			return sarrReturn;
		}
		public static int CountCSVElements(string sLine, char cFieldDelimiter, char cTextDelimiter) {
			int iFound=0;
			int iChar=0;
			bool bInQuotes=false;
			int iStartNow=0;
			int iEnderNow=-1;
			if (sLine!=null) {
				if (sLine!="") {
					while (iChar<=sLine.Length) {//intentionally <=
						if ( iChar==sLine.Length || sLine[iChar]==cFieldDelimiter ) {
							iEnderNow=iChar;
							iFound++;
						}
						else if (sLine[iChar]==cTextDelimiter) bInQuotes=!bInQuotes;
						iChar++;
					}
				}
				else iFound=1;
			}
			return iFound;
		}//CountNonQuotedElements
		public static string[] SplitCSV(string sLine, char cFieldDelimiter, char cTextDelimiter) {
			string[] sarrReturn=null;
			int iElements=CountCSVElements(sLine,cFieldDelimiter,cTextDelimiter);
			if (iElements>0) {
				sarrReturn=new string[iElements];
				int iFound=0;
				int iChar=0;
				bool bInQuotes=false;
				int iStartNow=0;
				int iEnderNow=-1;
				if (sLine!=null) {
					if (sLine!="") {
						while (iChar<=sLine.Length) {//intentionally <=
							if ( iChar==sLine.Length || sLine[iChar]==cFieldDelimiter ) {
								iEnderNow=iChar;
								sarrReturn[iFound]=Base.SafeSubstringByExclusiveEnder(sLine,iStartNow,iEnderNow);
								iFound++;
								iStartNow=iEnderNow++;
							}
							else if (sLine[iChar]==cTextDelimiter) bInQuotes=!bInQuotes;
							iChar++;
						}
					}
					else return new string[]{""};
				}
			}
			return sarrReturn;
		}//SplitCSV
		public static string IntersectionToString(int IntersectionA) {
			if (IntersectionA==IntersectionYes) return "Intersection Found";
			else if (IntersectionA==IntersectionNo) return "No Intersection";
			else if (IntersectionA==IntersectionBeyondSegment) return "Lines Only Intersect Beyond Segment";
			else if (IntersectionA==IntersectionError) return "Intersection Error";
			else return "Unknown Intersection Type #"+IntersectionA.ToString();
		}//IntersectionToString

		public static void SplitFileName(out string sFirstPart, out string sExtension, string sFileName) {
			try {
				//if (File.Exists(sFileName)) {
					
				//}
				//else {
					int iDot=sFileName.LastIndexOf(".");
					if (iDot<0) {
						sFirstPart=sFileName;
						sExtension="";
					}
					else {
						sFirstPart=SafeSubstring(sFileName,0,iDot);
						sExtension=SafeSubstring(sFileName,iDot+1);
					}
				//}
			}
			catch {
				sFirstPart=sFileName;
				sExtension="";
			}
		}
		public static void ShrinkByRef(ref string sToTruncate, int iBy) {
			try {
				if (iBy>=sToTruncate.Length) sToTruncate="";
				else sToTruncate=sToTruncate.Substring(0,sToTruncate.Length-iBy);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Base ShrinkByRef");
				sToTruncate="";
			}
		}
		public static string ShrinkBy(string sToTruncate, int iBy) {
			string sReturn=sToTruncate;
			ShrinkByRef(ref sReturn,iBy);
			return sReturn;
		}
		public static void StyleAppend(ref string sStyle, string sName, int iValue) {
			StyleAppend(ref sStyle, sName, iValue.ToString());
		}
		public static void StyleAppend(ref string sStyle, string sName, decimal dValue) {
			StyleAppend(ref sStyle, sName, dValue.ToString());
		}
		public static void StyleAppend(ref string sStyle, string sName, string sValue) {
			if (sStyle.IndexOf("{")<0) sStyle="{"+sStyle;
			sStyle+=sName+":"+sValue+"; ";
		}
		public static void StyleBegin(ref string sStyle) {
			sStyle="{";
		}
		public static void StyleEnd(ref string sStyle) {
			if (sStyle.IndexOf("{")<0) sStyle="{"+sStyle;
			sStyle+="}";
		}
		public static string SafeSubstringByExclusiveEnder(string sVal, int iStart, int iEnderExcluded) { //formerly SafeSubstringExcludingEnder
			return Base.SafeSubstring(sVal, iStart, (iEnderExcluded-iStart));
		}
		public static string SafeSubstringByInclusiveEnder(string sVal, int iStart, int iEndIncluded) {//formerly SafeSubstringByInclusiveLocations
			return SafeSubstringByExclusiveEnder(sVal,iStart,iEndIncluded+1);
		}
		public static bool StyleSplit(out string[] sarrName, out string[] sarrValue, string sStyleWithoutCurlyBraces) {
			bool bGood=true;
			sarrName=null;
			sarrValue=null;
			try {
				int iStartNow=0;
				int iStartNext=0;
				int iValSeperator;
				int iValEnder;
				int iFound=0;
				string sNameNow="";
				string sValNow="";
				ArrayList alNames=new ArrayList();
				ArrayList alValues=new ArrayList();
				while (iStartNext>-1) {
					iValSeperator=sStyleWithoutCurlyBraces.Substring(iStartNow).IndexOf(":");
					iValEnder=sStyleWithoutCurlyBraces.Substring(iStartNow).IndexOf(";");
					iStartNext=iValEnder;
					if (iValSeperator>-1) {
						iValSeperator+=iStartNow;
						if (iValEnder>-1) {
							iValEnder+=iStartNow;
						}
						else { //may be ok since last value doesn't require ending ';'
							iValEnder=sStyleWithoutCurlyBraces.Length; //note: iStartNext is already -1 now
						}

						if (iValEnder>iValSeperator) { //if everything is okay
						}
						else {
							Base.ShowErr("Null style value in \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".","StyleSplit");
							iStartNext=sStyleWithoutCurlyBraces.Substring(iValSeperator).IndexOf(";");
							int iEnder=0;
							bGood=false;
							if (iStartNext>-1) {
								iEnder=iStartNext+iValSeperator; //this is an ACTUAL location since iValSeperator was already incremented by iStartNow
								sNameNow=Base.SafeSubstringByInclusiveEnder(sStyleWithoutCurlyBraces, iStartNow, iEnder-1);
							}
							else sNameNow="";
							sValNow="";
						}
						Base.RemoveEndsSpacing(ref sValNow);
						Base.RemoveEndsSpacing(ref sNameNow);
						if (sNameNow.Length>0) {
							alNames.Add(sNameNow);
							alValues.Add(sValNow);
							iFound++;
						}
						else {
							if (iStartNext!=-1) {
								iStartNext=-1;
								Base.ShowErr("Variable name expected in: \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".","StyleSplit");
								bGood=false;
							}
						}
					}
					else {
						bGood=false;
						Base.ShowErr("Missing style colon in \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".","StyleSplit");
						break;
					}
					iStartNow=iStartNext;
				}
				if (iFound>0) {
					if (alValues.Count==alNames.Count) {
						sarrName=new string[iFound];
						sarrValue=new string[iFound];
						for (int iPop=0; iPop<iFound; iPop++) {
							sarrName[iPop]=alNames[iPop].ToString();
							sarrValue[iPop]=alValues[iPop].ToString();
						}
					}
					else {
						bGood=false;
						string sErr="Values/Names count do not match--";
						Base.StyleBegin(ref sErr);
						Base.StyleAppend(ref sErr, "alNames_Count", alNames.Count);
						Base.StyleAppend(ref sErr, "alValues_Count", alValues.Count);
						Base.StyleEnd(ref sErr);
						Base.ShowErr(sErr,"StyleSplit");
					}
				}
				else {
					sarrName=null;
					sarrValue=null;
					Base.ShowErr("No style variables in \""+sStyleWithoutCurlyBraces+"\"!","StyleSplit");
					bGood=false;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"StyleSplit");
				bGood=false;
			}
			return bGood;
		}//end StyleSplit
		public static string sFieldDelimiter=",";
		public static string sTextDelimiter="\"";
		
		/// <summary>
		/// Splits one CSV row.
		/// </summary>
		public static string[] SplitCSV(string sData) {
			return SplitCSV(sData,0,SafeLength(sData));
		}
		/// <summary>
		/// this is actually just a SplitCSV function that also accounts for sStarter and sEnder,
		/// and gets the location of the content of column at index
		/// --for string notation such as: sField="{{1,2,3},a,b,"Hello, There",c}"
		/// (where: sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
		/// --index 4 would return "c"
		/// </summary>
		/// <returns>whether subsection index was found</returns>
		public static string TextSubArray(string sField, int index) {
			int iStart;
			int iLen;
			bool bTest=SubSection(out iStart, out iLen, sField, 0, sField.Length, "{", "}", index);
			return Base.SafeSubstring(sField, iStart, iLen);
		}
		/// <summary>
		/// this is actually just a SplitCSV function that also accounts for sStarter and sEnder,
		/// and gets the location of the content of column at index
		/// --for string notation such as: sField="{{1,2,3},a,b,"Hello, There",c}"
		/// (where: sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
		/// --index 4 would return integers such that sField.Substring(iReturnStart,iReturnLen)=="c"
		/// </summary>
		/// <returns>whether subsection index was found</returns>
		public static bool TextSubArray(out int iReturnStart, out int iReturnLen, string sField, int index) {
			bool bTest=SubSection(out iReturnStart, out iReturnLen, sField, 0, sField.Length, "{", "}", index);
			return bTest;
		}
		
		public static bool bAllowNewLineInQuotes=true;//TODO: allow changing of this var\
		
		public static bool SubSection(out int iReturnStart, out int iReturnLen, string sData, int iStart, int iLen, string sStarter, string sEnder, int index) { //debug does NOT allow escape characters!
			return SubSection(out iReturnStart, out iReturnLen, sData, iStart, iLen, sStarter, sEnder, index, sTextDelimiter, sFieldDelimiter);
		}
		/// <summary>
		/// this is actually just a SplitCSV function that also accounts for sStarter and sEnder,
		/// and gets the location of the content of column at index
		/// --for string notation such as: sData="{{1,2,3},a,b,"Hello, There",c}"
		/// (where: sStarter="{"; sEnder="}"; sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
		/// --index 4 would yield integers such that sData.Substring(iReturnStart,iReturnLen)=="c"
		/// </summary>
		/// <returns>whether subsection index was found</returns>
		public static bool SubSection(out int iReturnStart, out int iReturnLen, string sData, int iStart, int iLen, string sStarter, string sEnder, int index, string sTextDelimiterNow, string sFieldDelimiterNow) { //debug does NOT allow escape characters!
			bool bFound=false;
			int iAbs=iStart;
			bool bInQuotes=false;
			int indexNow=0;
			int iDepth=0;
			int iStartNow=iStart;//only changed to the location after a zero-iDepth comma
			iReturnStart=0;
			iReturnLen=0;
			try {
				for (int iRel=0; iRel<=iLen; iRel++) { //necessary special case iRel==iLen IS safe below
					if (  iRel==iLen  ||  (bAllowNewLineInQuotes?(!bInQuotes&&CompareAt(sData,Environment.NewLine,iAbs)) :CompareAt(sData,Environment.NewLine,iAbs))  ) { //if non-text NewLine
						if (index==indexNow) {
							bFound=true;
						}
						else {
							Base.ShowErr("Text field subsection was not found on the same line (column index ["+indexNow.ToString()+"])", "Base SubSection");
						}
						break;
					}
					else if (CompareAt(sData,sTextDelimiterNow,iAbs)) { //text delimiter
						bInQuotes=!bInQuotes;
					}
					else if ((!bInQuotes) && iDepth==0 && CompareAt(sData,sFieldDelimiterNow,iAbs)) { //end field delimiter zero-level only
						if (indexNow==index) {
							bFound=true;
							break;
						}
						else {
							iStartNow=iAbs+sFieldDelimiterNow.Length; //i.e. still works if sStarter is not found next
							indexNow++;
						}
					}
					else if ((!bInQuotes) && CompareAt(sData,sStarter,iAbs)) { //opening bracket
						iDepth++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sEnder,iAbs)) { //closing bracket
						iDepth--;
					}
					if (!bFound) iAbs++;
				}
				if (bFound) {
					iReturnStart=iStartNow;
					iReturnLen=iAbs-iStartNow;//iLen=iLenNow;//iLenNow=iAbs-iStartNow;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Base SubSection");
			}
			if (!bFound) {
				iReturnStart=iStartNow;
				iReturnLen=0;
			}
			return bFound;
		}//end Base SubSection
		public static int SubSections(string sData, int iStart, int iLen, string sStarter, string sEnder) {
			return SubSections(sData, iStart, iLen, sStarter, sEnder, sTextDelimiter, sFieldDelimiter);
		}
		/// <summary>
		/// Uses SplitCSV logic that also accounts for sStarter and sEnder to
		/// get the count of the zero-level array
		/// --for string notation such as: sData="{{1,2,3},a,b,"Hello, There",c}"
		/// (where: sStarter="{"; sEnder="}"; sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
		/// --would yield 5
		/// </summary>
		/// <returns>number of indeces in sData</returns>
		public static int SubSections(string sData, int iStart, int iLen, string sStarter, string sEnder, string sTextDelimiterNow, string sFieldDelimiterNow) { //debug does NOT allow escape characters!
			int iFound=0;
			int iAbs=iStart;
			bool bInQuotes=false;
			int iDepth=0;
			int iStartNow=iStart;//only changed to the location after a zero-iDepth comma
			try {
				for (int iRel=0; iRel<=iLen; iRel++) { //necessary special case iRel==iLen IS safe below
					if (  iRel==iLen  ||  (bAllowNewLineInQuotes?(!bInQuotes&&CompareAt(sData,Environment.NewLine,iAbs)) :CompareAt(sData,Environment.NewLine,iAbs))  ) { //if non-text NewLine
						iFound++;
						break;
					}
					else if (CompareAt(sData,sTextDelimiterNow,iAbs)) { //text delimiter
						bInQuotes=!bInQuotes;
					}
					else if ((!bInQuotes) && iDepth==0 && CompareAt(sData,sFieldDelimiterNow,iAbs)) { //end field delimiter zero-level only
						iFound++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sStarter,iAbs)) { //opening bracket
						iDepth++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sEnder,iAbs)) { //closing bracket
						iDepth--;
					}
					iAbs++;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Base SubSections");
			}
			return iFound;
		}//end Base SubSections
		/// <summary>
		/// Split random area of CSV Data to fields (normally, send location and length of a row).  
		/// Be sure to check the return for double-quotes before determining whether to remove enclosing quotes after determining datatype!
		/// </summary>
		public static string[] SplitCSV(string sData, int iStart, int iChars) { //formerly CSVSplit
			bool bGood=false;
			string[] sarrReturn=null;
			//ArrayList alResult=new ArrayList();
			int iFields=0;
			try {
				int[] iarrEnder=null;
				bGood=CSVGetFieldEnders(iarrEnder, sData, iStart, iChars);
				sarrReturn=new string[iarrEnder.Length];
				int iFieldStart=iStart;
				for (int iNow=0; iNow<iarrEnder.Length; iNow++) {
					sarrReturn[iNow]=Base.SafeSubstring(sData,iFieldStart,iarrEnder[iNow]-iFieldStart);
					RemoveEndsSpacing(ref sarrReturn[iNow]);
					iFieldStart=iarrEnder[iNow]+sFieldDelimiter.Length; //ok since only other ender is NewLine which is at the end of the line
				}
				/*
				int iAbs=iStart;
				bool bInQuotes=false;
				int iFieldStart=iStart;
				int iFieldLen=0;
				for (int iRel=0; iRel<iChars; iRel++) {
					if (CompareAt(sData,Environment.NewLine,iAbs)) {//NewLine
						alResult.Add(Base.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFields++;
						break;
					}
					else if (CompareAt(sData,sTextDelimiter,iAbs)) {//Text Delimiter, or Text Delimiter as Text
						bInQuotes=!bInQuotes;
						iFieldLen++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sFieldDelimiter,iAbs)) {//Field Delimiter
						alResult.Add(Base.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFieldStart=iAbs+1;
						iFieldLen=0;
						iFields++;
					}
					else iFieldLen++; //Text
					
					if (iRel+1==iChars) { //ok since stopped already if newline
						//i.e. if iChars==1 then: if [0]==sFieldDelimiter iFields=2 else iFields=1
						alResult.Add(Base.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFields++;
					}
					iAbs++;
				}//end for iRel
				if (iChars==0) iFields++;
				
				if (alResult.Count>0) {
					if (alResult.Count!=iFields) Base.ShowErr("Field count does not match field ArrayList");
					sarrReturn=new string[alResult.Count];
					int iNow=0;
					Base.Write("found: ");//debug only
					foreach (string sNow in alResult) {
						RemoveEndsSpacing(ref sNow);
						sarrReturn[iNow]=sNow;
						Base.Write(sarrReturn[iNow]+" ");//debug only
						iNow++;
					}
					Base.WriteLine();//debug only
				}
				else {
					sarrReturn=new string[1];
					sarrReturn[0]="";
				}-
				*/
			}
			catch (Exception exn) {
				sarrReturn=null;
				bGood=false;
				Base.ShowExn(exn,"Base SplitCSV","reading columns");
			}
			return sarrReturn;
		}//end SplitCSV
		///<summary>
		///Gets locations of field enders, INCLUDING last field ending at newline OR end of data.
		///</summary>
		public static bool CSVGetFieldEnders(int[] iarrReturn, string sData, int iStart, int iChars) {
			bool bGood=true;
			int iFields=0;
			ArrayList alResult=new ArrayList();
			try {
				int iAbs=iStart;
				bool bInQuotes=false;
				for (int iRel=0; iRel<iChars; iRel++) {
					if (bAllowNewLineInQuotes?(!bInQuotes&&CompareAt(sData,Environment.NewLine,iAbs)) :CompareAt(sData,Environment.NewLine,iAbs)) {
						iFields++;
						alResult.Add(iAbs);
						//iAbs+=Environment.NewLine.Length-1; //irrelevant since break statement is below
						//iRel+=Environment.NewLine.Length-1; //irrelevant since loop below
						break;
					}
					else if (CompareAt(sData,sTextDelimiter,iAbs)) {	
						bInQuotes=!bInQuotes;
					}
					else if ((!bInQuotes) && CompareAt(sData,sFieldDelimiter,iAbs)) {
						alResult.Add(iAbs);
						iFields++;
					}
					
					if (iRel+1==iChars) { //ok since stopped already if newline
						alResult.Add(iAbs+1);
						//i.e. if iChars==1 then: if [0]==sFieldDelimiter result is 2 else result is 1
						iFields++;
					}
					iAbs++;
				}//end for iRel
				if (iChars==0) iFields++;
				if (alResult.Count>0) {
					if (iFields!=alResult.Count) {
						bGood=false;
						Base.ShowErr("Field count ("+iFields.ToString()+") does not match field list length ("+alResult.Count.ToString()+")","Base CSVGetFieldEnders");
					}
					int iNow=0;
					if (iarrReturn==null||iarrReturn.Length!=alResult.Count) iarrReturn=new int[alResult.Count];
					foreach (int iVal in alResult) {
						iarrReturn[iNow]=iVal;
						iNow++;
					}
				}
				else {
					iarrReturn=null;//fixed below
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Base CSVGetFieldEnders","separating columns");
			}
			if (iarrReturn==null) {
				iarrReturn=new int[1];
				iarrReturn[0]=1;//1 since ender at [Length] when no fields found
			}
			return bGood;
		}//end CSVGetFieldEnders
		public static int CSVCountCols(string sData, int iStart, int iChars) {
			int iReturn=0;
			try {
				int iAbs=iStart;
				bool bInQuotes=false;
				//string sCharX;
				//string sCharXY;
				for (int iRel=0; iRel<iChars; iRel++) {
					//sCharX=Base.SafeSubstring(sData,iAbs,1);
					//sCharXY=Base.SafeSubstring(sData,iAbs,2);
					//if (Environment.NewLine.Length>1?(sCharXY==Environment.NewLine):(sCharX==Environment.NewLine)) {
					//}
					if (CompareAt(sData,Environment.NewLine,iAbs)) {
						iReturn++;
						break;
					}
					else if (CompareAt(sData,sTextDelimiter,iAbs)) {	
						bInQuotes=!bInQuotes;
					}
					else if ((!bInQuotes) && CompareAt(sData,sFieldDelimiter,iAbs)) {
						iReturn++;
					}
					
					if (iRel+1==iChars) { //ok since stopped already if newline
						//i.e. if iChars==1 then: if [0]==sFieldDelimiter returns 2 else returns 1
						iReturn++;
					}
					iAbs++;
				}
				if (iChars==0) iReturn++;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Base CSVCountCols","counting columns");
				iReturn=0;
			}
			return iReturn;
		}//end CSVCountCols
		public static string DateTimePathString(bool bIncludeMilliseconds) {
			return DateTimeString(bIncludeMilliseconds,"_","_at_",".");
		}
		public static string DateTimeString(bool bIncludeMilliseconds, string sDateDelimiter, string sDateTimeSep, string sTimeDelimiter) {
			string sReturn;
			try {
				System.DateTime dtX;
				dtX=DateTime.Now;
				sReturn=dtX.Year+sDateDelimiter;
				if (dtX.Month<10) sReturn+="0";
				sReturn+=dtX.Month.ToString()+sDateDelimiter;
				if (dtX.Day<10) sReturn+="0";
				sReturn+=dtX.Day.ToString()+sDateTimeSep;
				if (dtX.Hour<10) sReturn+="0";
				sReturn+=dtX.Hour.ToString()+sTimeDelimiter;
				if (dtX.Minute<10)sReturn+="0";
				sReturn+=dtX.Minute.ToString()+sTimeDelimiter;
				if (dtX.Second<10)sReturn+="0";
				sReturn+=dtX.Second.ToString()+"s"+sTimeDelimiter;
				if (bIncludeMilliseconds) {
					int iMilNow=dtX.Millisecond;
					if (iMilNow<10) sReturn+="0";
					if (iMilNow<100) sReturn+="0";
					sReturn+=iMilNow.ToString()+"ms";
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"DateTimeString [now]");
				sReturn="UnknownTime";
			}
			return sReturn;
		}//end DateTimeString
		
		public static string TimeString() {
			return TimeString(false,":",false);
		}
		public static string TimePathString(bool bIncludeMilliseconds) {
			return TimeString(bIncludeMilliseconds,".",true);
		}
		public static string TimeString(bool bIncludeMilliseconds, string sTimeDelimiter, bool bEquidistant) {
			string sReturn="";
			try {
				System.DateTime dtX;
				dtX=DateTime.Now;
				//sReturn=dtX.Year+sDateDelimiter;
				//if (dtX.Month<10) sReturn+="0";
				//sReturn+=dtX.Month.ToString()+sDateDelimiter;
				//if (dtX.Day<10) sReturn+="0";
				//sReturn+=dtX.Day.ToString()+sDateTimeSep;
				if (bEquidistant && dtX.Hour<10) sReturn+="0";
				sReturn+=dtX.Hour.ToString()+sTimeDelimiter;
				if (bEquidistant && dtX.Minute<10)sReturn+="0";
				sReturn+=dtX.Minute.ToString()+sTimeDelimiter;
				if (bEquidistant && dtX.Second<10)sReturn+="0";
				sReturn+=dtX.Second.ToString()+"s"+sTimeDelimiter;
				if (bIncludeMilliseconds) {
					int iMilNow=dtX.Millisecond;
					if (iMilNow<10) sReturn+="0";
					if (iMilNow<100) sReturn+="0";
					sReturn+=iMilNow.ToString()+"ms";
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"DateTimeString [now]");
				sReturn="UnknownTime";
			}
			return sReturn;
		}//end TimeString
		
		public static string SequenceDigits(long lFrame) {
			return SequenceDigits(lFrame, 4);
		}
		public static string SequenceDigits(long lFrame, int iMinDigits) {
			string sDigits;
			long lFrameDestructible=lFrame;
			long lDigit;
			long lMod=10;
			long lDivisor=1;
			sDigits="";
			while (lFrameDestructible>0) {
				lDigit=lFrameDestructible%lMod;
				lFrameDestructible-=lDigit;
				lDigit/=lDivisor;
				sDigits=lDigit.ToString()+sDigits;
				lMod*=10;
				lDivisor*=10;
			}
			while (sDigits.Length<iMinDigits) sDigits="0"+sDigits;
			return sDigits;
		}
		/*
WebClient  Client = new WebClient();
Client.UploadFile("http://www.csharpfriends.com/Members/index.aspx", 
     "c:\wesiteFiles\newfile.aspx");

byte [] image;

//code to initialise image so it contains all the binary data for some jpg file
client.UploadData("http://www.csharpfriends.com/Members/images/logocc.jpg", image);
		*/
		
		/*msdn:
		C#
		//Create a new WebClient instance.
		WebClient myWebClient = new WebClient();
		//Download home page data. 
		Console.WriteLine("Accessing {0} ...",  uriString);                        
		//Open a stream to point to the data stream coming from the Web resource.
		Stream myStream = myWebClient.OpenRead(uriString);
		Console.WriteLine("\nDisplaying Data :\n");
		StreamReader sr = new StreamReader(myStream);
		Console.WriteLine(sr.ReadToEnd());
		//Close the stream. 
		myStream.Close();
		*/
		public static string DownloadToString(string sUrl) {
			return DownloadToString(sUrl, Environment.NewLine);
		}
		public static string DownloadToString(string sUrl, string sInsertMeAtNewLine) {
			Stream streamNow=null;
			StreamReader srNow=null;
			//WebRequest wrNow = new WebRequest ();
			WebClient wcNow=null;
			string sReturn="";
			//try {
			try {
				wcNow=new WebClient();
				streamNow=wcNow.OpenRead(sUrl);
				srNow=new StreamReader(streamNow);
				try {
					string sLine="";
					while ( (sLine=srNow.ReadLine()) != null ) {
						sReturn+=sLine+sInsertMeAtNewLine;
					}
					streamNow.Close();
					//wcNow.Close();
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"DownloadToString("+sUrl+",...)","reading site");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"DownloadToString("+sUrl+",...)","accessing site");
			}
			//}
			//catch (Exception exn) {
			//	Base.ShowExn(exn,"DownloadToString("+sUrl+",...)","reading site");
			//}
			return sReturn;
		}
		public static string FileToString(string sFile) {
			return FileToString(sFile, Environment.NewLine);
		}
		public static string FileToString(string sFile, string sInsertMeAtNewLine) {
			return FileToString(sFile, sInsertMeAtNewLine, false);
		}
		public static string FileToString(string sFile, string sInsertMeAtNewLine, bool bAllowLoadingEndingNewLines) {
			Base.sLastFile=sFile;
			StreamReader sr;
			string sDataX="";
			string sLine;
			try {
				sr=new StreamReader(sFile);
				//bool bFirst=true;
				while ( (sLine=sr.ReadLine()) != null ) {
					//if (bFirst==true) {
					//	sDataX=sLine;
					//	bFirst=false;
					//}
					//else 
					sDataX+=sLine+sInsertMeAtNewLine;
				}
				sDataX=sDataX.Substring(0,sDataX.Length-(Environment.NewLine.Length));
				if (!bAllowLoadingEndingNewLines) {
					while (sDataX.EndsWith(Environment.NewLine)) {
						sDataX=sDataX.Substring(0,sDataX.Length-Environment.NewLine.Length);
					}
				}
				sr.Close();
				//StringToFile(sFile+".AsLoadedToString.TEST.dmp",sDataX);
				//while (sDataX.EndsWith(Environment.NewLine))
				//	sDataX=sDataX.Substring(0,sDataX.Length-(Environment.NewLine.Length));
				//bFirst=false;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"StringFromFile");
				sDataX="";
			}
			return sDataX;
		}
		/// <summary>
		/// Calls StringFromFile inserting Environment.NewLine where newline is read.
		/// </summary>
		/// <param name="sFile"></param>
		/// <returns></returns>
		public static string StringFromFile(string sFile) {
			return FileToString(sFile);
		}
		public static string StringFromFile(string sFile, string sInsertMeAtNewLine) {
			return FileToString(sFile, sInsertMeAtNewLine);
		}
		
		public static string StringFromFile(string sFile, string sInsertMeAtNewLine, bool bAllowLoadingEndingNewLines) {
			return FileToString(sFile, sInsertMeAtNewLine, bAllowLoadingEndingNewLines);
		}//end StringFromFile
		public static bool WriteLine(ref string sToModify) {
			if (sToModify==null) sToModify="";
			sToModify+=Environment.NewLine;
			return true;
		}
		public static bool WriteLine(ref string sToModify, string sDataChunk) {
			if (sToModify==null) sToModify="";
			if (sDataChunk==null) sDataChunk="";
			sToModify+=sDataChunk+Environment.NewLine;
			return true;
		}
		public static bool Write(ref string sToModify, string sDataChunk) {
			if (sToModify==null) sToModify="";
			if (sDataChunk==null) sDataChunk="";
			sToModify+=sDataChunk;
			return true;
		}
		public static bool StringToFile(string sFile, string sAllDataX) {
			StreamWriter swX=null;
			bool bGood=false;
			//string sLine;//TODO:? implement this, ensuring newline chars are correct?
			try {
				swX=new StreamWriter(sFile);
				swX.Write(sAllDataX);
				swX.Close();
				bGood=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"StringToFile","{sFile:"+sFile+"; sAllDataX"+VariableMessageStyleOperatorAndValue(sAllDataX,false)+";}");
				bGood=false;
			}
			return bGood;
		}//end StringToFile
		public static void AppendForeverWrite(string sFileNow, string sMsg) {
			StreamWriter swNow;
			try {
				//iOutputs++;
				//if (iOutputs<iMaxOutputs) {
					swNow=File.AppendText(sFileNow);
					swNow.Write(sMsg);
					swNow.Close();
				//}
				//else if (iOutputs==iMaxOutputs) {
				//	swNow=File.AppendText(sFileNow);
				//	swNow.Write(Marker+"MAXIMUM MESSAGES REACHED--This is the last message that will be shown: "+sMsg);
				//	swNow.Close();
				//}
			}
			catch (Exception exn) {
				try {
					Base.IgnoreExn(exn,"Base Write","trying to append output text file");
					if (!File.Exists(sFileNow)) {
						swNow=File.CreateText(sFileNow);
						swNow.Write(sMsg);
						swNow.Close();
					}
				}
				catch (Exception exn2) {
					Base.IgnoreExn(exn2,"Base AppendForeverWrite","trying to create new output text file");//ignore since "error error"
				}
			}
			
		}
		public static void AppendForeverWriteLine(string sFileNow) {
			AppendForeverWrite(sFileNow,Environment.NewLine);
		}
		public static void AppendForeverWriteLine(string sFileNow, string sMsg) {
			if (sMsg==null) sMsg="";
			AppendForeverWrite(sFileNow,sMsg+Environment.NewLine);
		}
		public static bool ByteArrayToFile(string sFile, byte[] data) {
			bool bGood=false;
			FileStream fsNow=null;
			BinaryWriter bwNow=null;
			try {
				try {
					fsNow=new FileStream(sFile, FileMode.CreateNew);
				}
				catch (Exception exn) {
					Base.IgnoreExn(exn);
					fsNow=new FileStream(sFile,FileMode.Create);//TODO: show warning?
				}
				bwNow=new BinaryWriter(fsNow);
				bwNow.Write(data);
				bwNow.Close();
				fsNow.Close();
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn, "ByteArrayToFile");
			}
			return bGood;
		}//end ByteArrayToFile
		public static long FileSize(string sFile) {
			FileInfo fiNow=null;
			long iReturn=-2;
			try {
				fiNow=new FileInfo(sFile);
				iReturn=fiNow.Length;
			}
			catch (Exception exn) {
				iReturn=-3;
				Base.ShowExn(exn,"FileSize");
			}
			return iReturn;
		}
		public static long SizeOfFile(string sFile) {
			return FileSize(sFile);
		}
		public static bool FileToByteArray(out byte[] byarrData, string sFile) {
			bool bGood=false;
			byarrData=null;
			FileInfo fiNow=null;
			FileStream fsNow=null;
			BinaryReader brNow=null;
			long numBytes=-2;
			try {
				fiNow=new FileInfo(sFile);
				numBytes=fiNow.Length;
				fsNow=new FileStream(sFile, FileMode.Open, FileAccess.Read);
				brNow=new BinaryReader(fsNow);
				byarrData=brNow.ReadBytes((int)numBytes);
				brNow.Close();
				fsNow.Close();
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn, "FileToByteArray");
				byarrData=null;
			}
			return bGood;
		}//end FileToByteArray
		public static bool LikeWildCard(string input, string pattern, bool bCaseSensitive) {
			if (bCaseSensitive) return LikeWildCard(input, pattern, RegexOptions.None);
			else return LikeWildCard(input, pattern, RegexOptions.IgnoreCase);
		}
		public static bool LikeWildCard(string input, string pattern, RegexOptions regexoptions) {
			if (input==null) input="";
			if (pattern==null) pattern="";
			if (input==pattern) return true;
			if (input=="") return false;
			if (pattern=="") return false;
			try {
				return Regex.IsMatch(input, pattern, regexoptions);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"LikeWildCard");
				return false;
			}
		}
		public static bool SafeCompare(string sValue, string sHaystack, int iAtHaystackIndex) {
			bool bFound=false;
			try {
				if ( sValue==sHaystack.Substring(iAtHaystackIndex, sValue.Length) ) {
					bFound=true;
				}
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base SafeCompare");
				bFound=false;
			}
			return bFound;
		}
		public static int SafeLength(string sValue) {	
			try {
				if (sValue!=null&&sValue!="") return sValue.Length;
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base SafeLength(string)");
			}
			return 0;
		}
		public static int SafeLength(string[] val) {	
			try {
				if (val!=null) return val.Length;
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base SafeLength(string[])");
			}
			return 0;
		}
		public static int SafeLength(int[] val) {	
			try {
				if (val!=null) return val.Length;
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base SafeLength(int[])");
			}
			return 0;
		}
		public static int SafeLength(PictureBox[] val) {	
			try {
				if (val!=null) return val.Length;
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base SafeLength(PictureBox[])");
			}
			return 0;
		}
		public static string SafeSubstring(string sValue, int iStart) {
			if (sValue==null) return "";
			if (iStart<0) return ""; 
			try {
				if (iStart<sValue.Length) return sValue.Substring(iStart);
				else {
					return "";
					Base.Debug("Tried to return SafeSubstring(\""+sValue+"\","+iStart.ToString()+") (past end).");
				}
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base SafeSubstring(string,int)");
				return "";
			}
		}
		public static string SafeRemove(string sValue, int iExcludeAt, int iExcludeLen) {
			return SafeSubstring(sValue,0,iExcludeAt)+Base.SafeSubstring(sValue,iExcludeAt+iExcludeLen);
		}
		public static string SafeRemoveExcludingEnder(string sVal, int iExcludeAt, int iExcludeEnder) {
			return SafeRemove(sVal,iExcludeAt,iExcludeEnder-iExcludeAt);
		}
		public static string SafeRemoveIncludingEnder(string sVal, int iExcludeAt, int iAlsoRemoveEnder) {
			return SafeRemoveExcludingEnder(sVal, iExcludeAt, iAlsoRemoveEnder+1);
		}
		public static string SafeInsert(string sValue, int iAt, string sInsert) {
			return Base.SafeSubstring(sValue,0,iAt)+((sInsert==null)?Base.SafeSubstring(sValue,iAt):(sInsert+Base.SafeSubstring(sValue,iAt)));
		}
		public static string SafeSubstring(string sValue, int iStart, int iLen) {
			if (sValue==null) return "";
			if (iStart<0) return "";
			if (iLen<1) return "";
			try {
				if (iStart<sValue.Length) {
					if ((iStart+iLen)<=sValue.Length) return sValue.Substring(iStart, iLen);
					else {
						Base.Debug("Tried to return SafeSubstring(\"" + sValue+"\"," + iStart.ToString() + "," + iLen.ToString() + ") (area ending past end of string).");
						return sValue.Substring(iStart);
					}
					   //it is okay that the "else" also handles (iStart+iLen)==sValue.Length
				}
				else {
					Base.Debug("Tried to return SafeSubstring(\""+sValue+"\","+iStart.ToString()+","+iLen.ToString()+") (starting past end).");
					return "";
				}
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base SafeSubstring(string,int,int)");
				return "";
			}
		}
		public static void ReplaceNewLinesWithSpaces(ref string sDataNow) {
			string sCumulative="";
			if (sDataNow!=""&&sDataNow!=null) {
				int iCursor=0;
				int iCount=0;
				string sLine;
				while (ReadLine(out sLine, sDataNow, ref iCursor)) {
					if (iCount==0) sCumulative+=sLine;
					else sCumulative+=" "+sLine;
				}
				if (sLine==""&&sCumulative.EndsWith(" ")) sCumulative=SafeSubstring(sCumulative,0,sCumulative.Length-1); //remove any trailing space that was ADDED ABOVE to a blank line.
			}
			if (!IsUsedString(sCumulative)&&IsUsedString(sDataNow)) Base.Debug("ReplaceNewLinesWithSpaces got empty string \""+sCumulative+"\" from used string \""+sDataNow+"\".");
			sDataNow=sCumulative;
		}
		/// <summary>
		/// Gets the non-null equivalent of a null, empty, or nonempty string.
		/// </summary>
		/// <param name="val"></param>
		/// <returns>If Null Then return is "NULLSTRING"; if "" then return is "", otherwise
		/// val is returned.</returns>
		public static string SafeString(string val, bool bReturnWhyIfNotSafe) {
			try {
				return (val!=null)
					?val
					:(bReturnWhyIfNotSafe?"null-string":"");
			}
			catch {//do not report this
				return bReturnWhyIfNotSafe?"incorrectly-initialized-string":"";
			}
		}
		public static byte SafeByte(REAL val) {
			if (val<=(REAL)0.0) return 0;
			else if (val>=r255) return 255;
			return (byte)val;
		}
		public static int ReplaceAll(ref string sData, string sFrom, string sTo) {
			int iReturn=0;
			try {
				if (sData.Length==0) {
					Base.ShowErr("There is no text in which to search for replacement.","ReplaceAll");
					//still returns true (0) though
				}
				else {
					int iPlace=sData.IndexOf(sFrom);
					int iReplaced=0;
					while (iPlace>-1) {
						sData=sData.Remove(iPlace,sFrom.Length);
						sData=sData.Insert(iPlace,sTo);
						if (iPlace>=0) iReplaced++;
						iReturn++;
						iPlace=sData.IndexOf(sFrom);
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Base ReplaceAll [text]");
			}
			return iReturn;
		}//end ReplaceAll
		public static int ReplaceAll(string sFrom, string sTo, string[] sarrHaystack) {
			int iReturn=0;
			try {
				if (sarrHaystack!=null) {
					for (int iNow=0; iNow<sarrHaystack.Length; iNow++) {
						iReturn+=Base.ReplaceAll(ref sarrHaystack[iNow], sFrom, sTo);
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ReplaceAll [in string array]");
			}
			return iReturn;
		}
		public static void RemoveSpacingBeforeNewLines(ref string sData) {
			RemoveSpacingBeforeNewLines(ref sData, ref mcbNULL);
		}
		public static void RemoveSpacingBeforeNewLines(ref string sData, ref MyCallback mcbNow) {
			try {
				int iFound=1;
				while (iFound>0) {
					iFound=0;
					iFound+=ReplaceAll(ref sData, sRemoveSpacingBeforeNewLines1, Environment.NewLine);
					iFound+=ReplaceAll(ref sData, sRemoveSpacingBeforeNewLines2, Environment.NewLine);
					if (mcbNow!=null) mcbNow.UpdateStatus("Removing "+iFound.ToString()+", "+iFound.ToString()+" total...");
				}
				if (mcbNow!=null) mcbNow.UpdateStatus("Done with "+iFound.ToString()+" total spacing before newlines removed.");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RemoveSpacingBeforeNewLines");
			}
		}
		public static void RemoveBlankLines(ref string sData) {
			RemoveBlankLines(ref sData, ref mcbNULL, false);
		}
		public static void RemoveBlankLines(ref string sData, ref MyCallback mcbNow) {
			RemoveBlankLines(ref sData, ref mcbNow, false);
		}
		public static void RemoveBlankLines(ref string sData, ref MyCallback mcbNow, bool bAllowTrailingNewLines) {
			try {
				int iFoundNow=1;
				string sRemove=Environment.NewLine+Environment.NewLine;
				while (iFoundNow>0) {
					iFoundNow=0;
					iFoundNow+=ReplaceAll(ref sData, sRemove, Environment.NewLine);
					if (mcbNow!=null) mcbNow.UpdateStatus("Removing "+iFoundNow.ToString()+", "+iFoundNow.ToString()+" total...");
				}
				if (!bAllowTrailingNewLines) {
					while (sData.EndsWith(Environment.NewLine)) {
						sData=sData.Substring(0,sData.Length-Environment.NewLine.Length);
						iFoundNow++;
						if (mcbNow!=null) mcbNow.UpdateStatus("Removing "+iFoundNow.ToString()+", "+iFoundNow.ToString()+" total...removed trailing blank line...");
					}
				}
				if (mcbNow!=null) mcbNow.UpdateStatus("Done with "+iFoundNow.ToString()+" total blank lines removed.");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RemoveSpacingBeforeNewLines");
			}
		}//end RemoveBlankLines
		public static string AlphabeticalByNumber(int iStartingWithZeroAsA) {
			return AlphabeticalByNumber(iStartingWithZeroAsA, false);
		}
		/// <summary>
		/// Gets alphabetical character, starting with 0 as 'a', or 'A' if bUpperCase.
		/// </summary>
		/// <param name="bUpperCase">whether returned char should be uppercase</param>
		/// <param name="iStartingWithZeroAsA">alphabetical index from 0 to 25, otherwise return is a space</param>
		/// <returns>If within range of alphabet (i.e. 0 to 25),
		///  returns alphabetical character (uppercase if bUpperCase); otherwise
		///  returns a [space] character.</returns>
		public static string AlphabeticalByNumber(int iStartingWithZeroAsA, bool bUpperCase) {
			string sReturn=" ";
			if (iStartingWithZeroAsA>-1 && iStartingWithZeroAsA<carrAlphabetUpper.Length) 
				sReturn=char.ToString( (bUpperCase)?
				                      carrAlphabetUpper[iStartingWithZeroAsA]
				                      :carrAlphabetLower[iStartingWithZeroAsA] );
			return sReturn;
		}
		public static int IndexInSubstring(string sNow, int iStart, char cNeedle) {
			int iReturn=-1;
			if (sNow!=null&&sNow!=""&&iStart<sNow.Length) {
				for (int iChar=iStart; iChar<sNow.Length; iChar++) {
					if (sNow[iChar]==cNeedle) {
						iReturn=iChar;
						break;
					}
				}
			}
			return iReturn;
		}
		public static int CountInstances(string sHaystack, char cNeedle) {
			int iCount=0;
			int iLocNow=0;
			int iStartNow=0;
			try {
				if (sHaystack!=null&&sHaystack!="") {
					for (int iChar=0; iChar<sHaystack.Length; iChar++) {
						if (sHaystack[iChar]==cNeedle) iCount++;
					}
				}
				else Base.ShowErr("Tried to count matching characters in blank string!","CountInstances(string,char)");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CountInstances(string,char)");
			}
			return iCount;
			
		}
		public static int CountInstances(string sHaystack, string sNeedle) {
			int iCount=0;
			int iLocNow=0;
			int iStartNow=0;
			try {
				if (sNeedle.Length!=0) {
					while (iLocNow>-1) {
						iLocNow=sHaystack.Substring(iStartNow).IndexOf(sNeedle);
						if (iLocNow>-1) {
							iCount++;
							iStartNow+=iLocNow+sNeedle.Length;
						}
					}
				}
				else Base.ShowErr("Tried to find blank string in \""+sHaystack+"\"!","CountInstances");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CountInstances");
			}
			return iCount;
		} //end CountInstances
		public static bool CompareSub(string sHaystack, int iAtHaystack, string sNeedle) {
			int iMatches=0;
			if (IsUsedString(sHaystack)&&IsUsedString(sNeedle)) {
				int iAbs=iAtHaystack;
				for (int iRel=0; iRel<sNeedle.Length&&iAbs<sHaystack.Length; iRel++) {
					if (sHaystack[iAbs]==sNeedle[iRel]) iMatches++;
					else return false;
					iAbs++;
				}
				return iMatches==sNeedle.Length;
			}
			else return false;
		}
		public static string[] StringToLines(string sVal) {
			string[] sarrReturn=null;
			if (sVal==null) sVal="";
			if (sVal=="") {
				sarrReturn=new string[1];
				sarrReturn[0]="";
			}
			else {
				//ArrayList alNow=new ArrayList();
				//ReplaceAll(ref sVal, Environment.NewLine,"\n");
				//ReplaceAll(ref sVal, "\r","");
				int iLines=1;
				int iTest=0;
				iTest=CountInstances(sVal,Environment.NewLine);
				if (iTest+1>iLines) iLines=iTest+1;
				iTest=CountInstances(sVal,"\n")+CountInstances(sVal,"\r");//this is ok for fault tolerance
				if (iTest+1>iLines) iLines=iTest+1;
				string[] sarrTemp=new string[iLines];//sarrReturn=new string[CountInstances(sVal,'\n')+1];
				int iLineX=0;
				int iStartNow=0;
				for (int iChar=0; iChar<=sVal.Length; iChar++) {
					if (iChar==sVal.Length) {
						sarrTemp[iLineX]=SafeSubstring(sVal,iStartNow,iChar-iStartNow);
						//alNow.Add(SafeSubstring(sVal,iStartNow,iChar-iStartNow));
						////iStartNow=iChar+1;
						iLineX++;
					}
					else if ( (iChar+1<sVal.Length) && (CompareSub(sVal,iChar,Environment.NewLine)) ) { //&&sVal.Substring(iChar, 2)==Environment.NewLine
						sarrTemp[iLineX]=SafeSubstring(sVal,iStartNow,iChar-iStartNow);
						iStartNow=iChar+Environment.NewLine.Length;
						iChar+=Environment.NewLine.Length-1;//this is right since the loop will add another one
						iLineX++;
					}
					else if (sVal[iChar]=='\r') {
						sarrTemp[iLineX]=SafeSubstring(sVal,iStartNow,iChar-iStartNow);
						//alNow.Add(SafeSubstring(sVal,iStartNow,iChar-iStartNow));
						iStartNow=iChar+1;
						iLineX++;
					}
					else if (sVal[iChar]=='\n') {
						sarrTemp[iLineX]=SafeSubstring(sVal,iStartNow,iChar-iStartNow);
						//alNow.Add(SafeSubstring(sVal,iStartNow,iChar-iStartNow));
						iStartNow=iChar+1;
						iLineX++;
					}
				}
				if (iLineX>0) {
					sarrReturn=new string[iLineX];
					for (int iNow=0; iNow<iLineX; iNow++) sarrReturn[iNow]=sarrTemp[iNow];
				}
			}//end else not blank
			return sarrReturn;
		}//end StringToLines
		/// <summary>
		/// Returns a list of values
		/// </summary>
		/// <param name="sField">Values Separated by mark</param>
		/// <param name="sMark">Mark that Separates Values</param>
		/// <returns></returns>
		public static string[] StringsFromMarkedList(string sField, string sMark) {
			string[] sarrNew=null;
			int index=0;
			try {
				if (sField!="") {
					while (sField.StartsWith(sMark)) {
						if (sField==sMark) {
							sField="";
							break;
						}
						else sField=sField.Substring(1);
					}
				}
				//if still !="", continue
				if (sField!="") {
					while (sField.EndsWith(sMark)) {
						if (sField==sMark) {
							sField="";
							break;
						}
						else sField=sField.Substring(0, sField.Length-1);
					}
				}
				//now continue as if we started here:
				if (sField!="") {
					int iMarks=CountInstances(sField, sMark);
					if (iMarks>0) {
						sarrNew=new string[iMarks];
						int iMark=-1;
						int iMarksNow=iMarks;
						while (iMarksNow>0) {
							iMark=sField.IndexOf(sMark);
							sarrNew[index]=sField.Substring(0,iMark);
							index++;
							sField=sField.Substring(iMark+1);
							iMarksNow--;
						}
						sarrNew[index]=sField;
						index++;//not used after this though
					}
					else {
						sarrNew=new string[1];
						sarrNew[0]=sField;
					}
				}
				else {
					sarrNew=new string[1];
					sarrNew[0]=sField;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Base StringsFromMarkedList");
				sarrNew=new string[1];
				sarrNew[0]=sField;
			}
			return sarrNew;
		}
		public static bool SelectTo(out int iSelLen, string sAllText, int iFirstChar, int iLastCharInclusive) {
			iSelLen=0;
			bool bGood=false;
			try {
				if (iFirstChar<0) {
					Base.ShowErr("Tried to select beyond beginning of file, from "
						+iFirstChar+" to "+iLastCharInclusive
						+", so selecting nothing instead.","SelectTo");
					iSelLen=0;
				}
				else if (iFirstChar>=sAllText.Length) {
					throw new ApplicationException("Selection start was past end of data {iFirstChar:"+iFirstChar.ToString()+"; Length:"+sAllText.Length.ToString()+"}");
				}
				else {
					if (iLastCharInclusive<iFirstChar) {
						iSelLen=0;
						throw new ApplicationException("Target is before start of " +
							"selection, so defaulting to iSelLen=0");
					}
					else if (iLastCharInclusive==iFirstChar) iSelLen=0;
					else {
						if (iLastCharInclusive>=sAllText.Length) {
							Base.ShowErr("Tried to select beyond end of file, from "
								+iFirstChar+" to "+iLastCharInclusive
								+", so selecting to end by default instead.","SelectTo");
							iLastCharInclusive=sAllText.Length-1;
						}
						iSelLen=(iLastCharInclusive-iFirstChar)+1;
					}
					bGood=true;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"SelectTo","selecting from "+iFirstChar.ToString()
					+" to "+iLastCharInclusive.ToString()+" inclusively {Length:"+sAllText.Length.ToString()+"}");
			}
			return bGood;
		}
		public static bool MoveToOrStayAtSpacingOrString(ref int iMoveMe, string sData, string sFindIfBeforeSpacing) {
			bool bGood=true;
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (IsSpacing(sData[iMoveMe]))
						break;
					else if (sData.Substring(iMoveMe,sFindIfBeforeSpacing.Length)==sFindIfBeforeSpacing)
						break;
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) Base.WriteLine("Reached end of page (MoveToOrStayAtSpacingOrString).");
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"MoveToOrStayAtSpacingOrString");
			}
			return bGood;
		}
		public static bool MoveBackToOrStayAt(ref int iMoveMe, string sData, string sFind) {
			bool bGood=false;
			try {
				int iEOF=-1;
				while (iMoveMe>iEOF) {
					if (sData.Substring(iMoveMe,sFind.Length)==sFind) {
						bGood=true;
						break;
					}
					else iMoveMe--;
				}
				if (iMoveMe<=iEOF) {
					Base.ShowErr("Reached beginning of string.","MoveBackToOrStayAt sFind");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"MoveBackToOrStayAt sFind text");
				bGood=false;
			}
			return bGood;
		}
		public static bool MoveToOrStayAt(ref int iMoveMe, string sData, string sFind) {
			bool bGood=false;
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (sData.Substring(iMoveMe,sFind.Length)==sFind) {
						bGood=true;
						break;
					}
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) {
					Base.WriteLine("Reached end of page (MoveBackToOrStayAt).");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"MoveToOrStayAt(...,...,sFind)","searching for text");
				bGood=false;
			}
			return bGood;
		}
		public static bool MoveToOrStayAtSpacing(ref int iMoveMe, string sData) {
			bool bGood=false;
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (Base.IsSpacing(sData,iMoveMe)) {
						bGood=true;
						break;
					}
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) Base.WriteLine("Reached end of string (MoveToOrStayAtSpacing).");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"MoveToOrStayAtSpacing","searching for text");
			}
			return bGood;
		}
		//public static bool IsSpacingExceptNewLine(char val) {
		//{}
		public static bool IsSpacingExceptNewLine(char val) {
			bool bYes=false;
			try {
				if (val==' ') bYes=true;
				else if (val=='\t') bYes=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IsSpacingExceptNewLine","checking for spacing");
			}
			return bYes;
		}
		//public static readonly char[] carrNewLine=new char[] {'\n', '\r', Environment.NewLine[0], Environment.NewLine[Environment.NewLine.Length-1]};
		public static int IsNewLineAndGetLength(string sText, int iChar) {	
			if (sText!=null&&iChar>=0&&iChar<sText.Length) {
				if (CompareAt(sText,Environment.NewLine,iChar)) return Environment.NewLine.Length;
				else if (sText[iChar]=='\n') return 1;
				else if (sText[iChar]=='\r') return 1;
			}
			return 0;
		}
		public static bool IsNewLine(char val) {
			bool bYes=false;
			try {
				if (val=='\r') bYes=true;
				else if (val=='\n') bYes=true;
				else if (Base.Contains(Environment.NewLine,val)) bYes=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IsNewLine","checking for newline");
			}
			return bYes;
		}
		public static readonly char[] carrLineBreakerInvisible=new char[]{' ','\t'};
		public static readonly char[] carrLineBreakerVisible=new char[]{'-',';',':','>'};
		public static bool PreviousLineBreakerExceptNewLine(string sText, ref int iReturnBreaker_ElseNeg1, out bool bVisibleBreaker) {
			bool bGood=true;
			bVisibleBreaker=false;
			try {
				while (iReturnBreaker_ElseNeg1>-1) {
					if (IsInvisibleLineBreaker_Unsafe(sText[iReturnBreaker_ElseNeg1])) {
						bVisibleBreaker=false;
						return true;//bGood=true;
					}
					iReturnBreaker_ElseNeg1--;
				}
				while (iReturnBreaker_ElseNeg1>-1) {
					if (IsVisibleLineBreaker_Unsafe(sText[iReturnBreaker_ElseNeg1])) {
						bVisibleBreaker=true;
						return true;//bGood=true;
					}
					iReturnBreaker_ElseNeg1--;
				}
			}
			catch (Exception exn) {	
				bGood=false;
				Base.ShowExn(exn,"PreviousLineBreakerExceptNewLine","{sText"+VariableMessageStyleOperatorAndValue(sText,false)+"; iReturnBreaker_ElseNeg1:"+iReturnBreaker_ElseNeg1.ToString()+";}");
			}
			return bGood;
		}
		private static bool IsVisibleLineBreaker_Unsafe(char val) {
			int iNow;
			for (iNow=0; iNow<carrLineBreakerVisible.Length; iNow++) {
				if (carrLineBreakerVisible[iNow]==val) {
					return true;
				}
			}
			return false;
		}
		private static bool IsInvisibleLineBreaker_Unsafe(char val) {
			int iNow;
			for (iNow=0; iNow<carrLineBreakerInvisible.Length; iNow++) {
				if (carrLineBreakerInvisible[iNow]==val) {
					return true;
				}
			}
			return false;
		}
		public static bool IsLineBreakerExceptNewLine(char val) {
			int iNow;
			for (iNow=0; iNow<carrLineBreakerInvisible.Length; iNow++) {
				if (carrLineBreakerInvisible[iNow]==val) {
					return true;
				}
			}
			for (iNow=0; iNow<carrLineBreakerVisible.Length; iNow++) {
				if (carrLineBreakerVisible[iNow]==val) {
					return true;
				}
			}
			return false;
		}
		public static bool IsLineBreaker(char val) {
			return IsLineBreakerExceptNewLine(val)||IsNewLine(val);
		}
		public static bool IsLineBreaker(string val, int iChar) {
			return iChar>=0&&val!=null&&iChar<val.Length&&IsLineBreaker(val[iChar]);
		}
		public static bool IsLineBreakerExceptNewLine(string val, int iChar) {
			return iChar>=0&&val!=null&&iChar<val.Length&&IsLineBreakerExceptNewLine(val[iChar]);
		}
		public static bool IsSpacing(string val, int iChar) {
			return (IsSpacingExceptNewLine(val[iChar])||IsNewLine(val[iChar]));
		}
		public static bool IsSpacing(char val) {
			return (IsSpacingExceptNewLine(val)||IsNewLine(val));
		}
		public static bool RemoveEndsSpacingExceptNewLine(ref string val) {
			try {
				if (val==null) val="";
				int iStart=0;
				int iEnder=val.Length-1;
				int iLength=val.Length;
				while (iLength>0&&(val[iStart]=='\t'||val[iStart]==' ')) {iStart++;iLength--;}
				while (iLength>0&&(val[iEnder]=='\t'||val[iEnder]==' ')) {iEnder--;iLength--;}
				if (iLength!=val.Length) val=SafeSubstring(val,iStart,iLength);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RemoveEndsSpacingExceptNewLine");
				return false;
			}
			return true;
// 			try {
// 				if (val==null) val="";
// 				while (val.Length>0&&val.StartsWith(" ")) val=val.Substring(0,val.Length-1);
// 				while (val.Length>0&&val.EndsWith(" ")) val=val.Substring(0,val.Length-1);
// 				while (val.Length>0&&val.StartsWith("\t")) val=val.Substring(0,val.Length-1);
// 				while (val.Length>0&&val.EndsWith("\t")) val=val.Substring(0,val.Length-1);
// 			}
// 			catch (Exception exn) {
// 				Base.ShowExn(exn,"RemoveEndsSpacingExceptNewLine");
// 				return false;
// 			}
// 			return true;
		}
		public static int Length(int iStart, int iInclusiveEnder) {
			return LengthEx(iStart,iInclusiveEnder+1);
		}
		public static int LengthEx(int iStart, int iExclusiveEnder) {
			return iExclusiveEnder-iStart;
		}
		public static bool RemoveEndsNewLines(ref string val) {
			try {
				if (val==null) val="";
				int iStart=0;
				int iEnder=val.Length-1;
				int iLength=val.Length;
				int iNewLine=Environment.NewLine.Length;
				while (iLength>0&&CompareAt(val, Environment.NewLine, iStart)) {iStart+=iNewLine; iLength-=iNewLine;}
				while (iLength>0&&CompareAt(val, Environment.NewLine, iEnder-(iNewLine-1))) {iEnder-=iNewLine; iLength-=iNewLine;}
				while (iLength>0&&(val[iStart]=='\n'||val[iStart]=='\r')) {iStart++;iLength--;}
				while (iLength>0&&(val[iEnder]=='\n'||val[iEnder]=='\r')) {iEnder--;iLength--;}
				if (iLength!=val.Length) val=SafeSubstring(val,iStart,iLength);
				/*
				while (val.Length>0&&val.StartsWith(Environment.NewLine)) val=val.Substring(0,val.Length-1);
				while (val.Length>0&&val.EndsWith(Environment.NewLine)) val=val.Substring(0,val.Length-1);
				while (val.Length>0&&val.StartsWith("\n")) val=val.Substring(0,val.Length-1);
				while (val.Length>0&&val.EndsWith("\n")) val=val.Substring(0,val.Length-1);
				while (val.Length>0&&val.StartsWith("\r")) val=val.Substring(0,val.Length-1);
				while (val.Length>0&&val.EndsWith("\r")) val=val.Substring(0,val.Length-1);
				while (val.Length>0&&val.StartsWith("\n")) val=val.Substring(0,val.Length-1);
				while (val.Length>0&&val.EndsWith("\n")) val=val.Substring(0,val.Length-1);
				*/
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RemoveEndsNewLines");
				return false;
			}
			return true;
		}
		public static bool RemoveEndsSpacing(ref string val) {
			return RemoveEndsNewLines(ref val) && RemoveEndsSpacingExceptNewLine(ref val);
		}
		/// <summary>
		/// Counts lines assuming Environment.NewLine is used; includes
		/// blank lines at end (i.e. even if last line ends with newline)
		/// </summary>
		/// <param name="sAllData">data in which to search</param>
		/// <returns>how many lines</returns>
		public static int LineCount(string sAllData) {
			return CountInstances(sAllData, Environment.NewLine)+1;
		}
		public static int SafeIndexOf(string sHaystack, string sNeedle) {
			return SafeIndexOf(sHaystack, sNeedle, 0);
		}
		public static int SafeIndexOf(string sHaystack, string sNeedle, int iStart) {
			int iReturn= (sHaystack==null || sHaystack.Length<1 || sNeedle==null || sNeedle.Length<1) ?
				-1
				:((iStart>0)?SafeSubstring(sHaystack,iStart):sHaystack).IndexOf(sNeedle);
			if (iReturn>-1) iReturn+=iStart;
			return iReturn;
		}
		public static bool SafeCompare(string[] sarrMatchAny, string sHaystack, int iHaystack) {
			bool bFound=false;
			try {
				int iNow=0;
				while (iNow<sarrMatchAny.Length) {
					if (SafeCompare(sarrMatchAny[iNow], sHaystack, iHaystack)) {
						bFound=true;
						break;
					}
				}
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"SafeCompare(sarrMatchAny...)");//do not report this, just say "no"
				bFound=false;
			}
			return bFound;
		}
		public static bool MoveToOrStayAtAttrib(ref int indexToGet, uint[] dwarrAttribToSearch, uint bitsToFindAnyOfThem) {
			bool bFound=false;
			try {
				while (indexToGet<dwarrAttribToSearch.Length) {
					if ((dwarrAttribToSearch[indexToGet]&bitsToFindAnyOfThem)!=0) {
						bFound=true;
						break;
					}
					indexToGet++;
				}
			}
			catch (Exception exn) {
				bFound=false;
				Base.ShowExn(exn,"MoveToOrStayAtAttrib");
			}
			return bFound;
		}//end MoveToOrStayAtAttrib
		public static readonly uint UintMask=0xFFFFFFFF;//bitmask for uint bits
		public static readonly uint dwMask=0xFFFFFFFF;//bitmask for uint bits
		public static bool Contains(string Haystack, string Needle) {
			try {
				return Haystack.IndexOf(Needle) >= 0;
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Contains(string,string)");
				return false;
			}
		}
		public static bool Contains(string Haystack, char Needle) {
			try {
				return IndexInSubstring(Haystack,0,Needle) >= 0;
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Contains(string,char)");
				return false;
			}
		}
		public static string PrecedeByIfNotBlank(string sStringToPrecedeValue_IfValueNotBlank, string sValue) {
			if (sValue!=null&&sValue!="") return sStringToPrecedeValue_IfValueNotBlank+sValue;
			else return "";
		}
		public static string ElipsisIfOver(string sDataOriginal, int iMaxLength) {
			try {
				if (iMaxLength>=3) {
					if (sDataOriginal.Length>iMaxLength) return SafeSubstring(sDataOriginal,0,iMaxLength-3)+"...";
					else return sDataOriginal;
				}
				else {
					if (sDataOriginal.Length>iMaxLength) return SafeSubstring(sDataOriginal,0,iMaxLength-1)+"~";
					else return sDataOriginal;
				}
			}
			catch {
				return "";
			}
		}
		public static string Repeat(string val, int iCount) {
			string sReturn="";
			if (val!=null) {
				for (int iNow=0; iNow<iCount; iNow++) {
					sReturn+=val;
				}
			}
			return sReturn;
		}
		public static string FixedWidth(int val, int iLength) {
			return FixedWidth(val.ToString(),iLength);
		}
		
		public static string FixedWidth(string val, int iLength) {
			return FixedWidth(val,iLength," ");
		}
		public static string FixedWidth(int val, int iLength, string sFillerChar) {
			return FixedWidth(val,iLength,sFillerChar);
		}
		public static string FixedWidth(string val, int iLength, string sFillerChar) {
			try {
				if (sFillerChar.Length<1) sFillerChar=" ";
				else if (sFillerChar.Length>1) sFillerChar=sFillerChar.Substring(0,1);
				if (val.Length>iLength) return ElipsisIfOver(val,iLength);
				else return Repeat(sFillerChar,iLength-val.Length)+val;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"FixedWidth","setting fixed width string {val"+VariableMessageStyleOperatorAndValue(val,false)+"}");
			}
			return Repeat("~",iLength);
		}
		#endregion text functions (#endregion string methods)
	
		#region digit functions
		public static bool IsDigit(char cDigit) {
			if ( cDigit=='0'|| cDigit=='1'||cDigit=='2'||cDigit=='3'||cDigit=='4'||cDigit=='5'
				 ||cDigit=='6'||cDigit=='7'||cDigit=='8'||cDigit=='9'
				 //||(bAllowDecimalDelimiter && cDigit='.')
				 //||(bAllowNumberTriadVisualSeparator && cDigit=',')
			   ) return true;
			else return false;
		}
		public static bool IsNumeric(string sNumber, bool bAllowDecimalDelimiter, bool bAllowNumberTriadVisualSeparator) {
			char[] carrNow=sNumber.ToCharArray();
			int iDecimalDelimiters=0;
			bool bReturn=true;
			for (int iChar=0; iChar<carrNow.Length; iChar++) {
				if (carrNow[iChar]=='.') {
					if (!bAllowDecimalDelimiter) {
						bReturn=false;
						break;
					}
					iDecimalDelimiters++;
					if (iDecimalDelimiters>1) {
						bReturn=false;
						break;
					}
				}
				else if (carrNow[iChar]==',') {
					if (!bAllowNumberTriadVisualSeparator) {
						bReturn=false;
						break;
					}
				}
				else if (!IsDigit(carrNow[iChar])) {
					bReturn=false;
					break;
				}
			}
			return bReturn;
		}
		#endregion digit functions
		


		#region geometry
		public static void ToRectPoints(ref int x1, ref int y1, ref int x2, ref int y2) {
			if (y2<y1) Base.Swap(ref y1, ref y2);
			if (x2<x1) Base.Swap(ref x1, ref x2);
		}
		public static bool IsInBox(int x, int y, int x1, int y1, int x2, int y2) {
			ToRectPoints(ref x1, ref y1, ref x2, ref y2);
			return (x>=x1 && x<=x2 && y>=y1 && y<=y2);
		}
		public static bool IsInBoxEx(int x, int y, int x1, int y1, int x2, int y2) {
			ToRectPoints(ref x1, ref y1, ref x2, ref y2);
			return (x>=x1 && x<x2 && y>=y1 && y<y2);
		}
		///<summary>
		///makes point 2 to the right (may remain above or below point 1)
		///</summary>
		public static void OrderPointsLR(ref int x1, ref int y1, ref int x2, ref int y2) {
			//if (y1==y2) {
			//	if (x2<x1) Base.Swap(ref x1, ref x2);
			//}
			//else if (x1==x2) {
			//	if (y2<y1) Base.Swap(ref y1, ref y2);
			//}
			//else
			if (x2<x1) {
				Base.Swap(ref y1, ref y2);
				Base.Swap(ref x1, ref x2);
			}
			//else if (y2<y1) {
			//	Base.Swap(ref y1, ref y2);
			//	Base.Swap(ref x1, ref x2);
			//}
		}//end OrderPointsLR
		public static bool PointIsOnLine(int x, int y, int x1, int y1, int x2, int y2, bool bLinePointOrderHasBeenFixedAlready) {
			return PointIsOnLine(x,y,x1,y1,x2,y2,bLinePointOrderHasBeenFixedAlready,true);
		}
		
		public static bool PointIsOnLine(int x, int y, int x1, int y1, int x2, int y2, bool bLinePointOrderHasBeenFixedAlready, bool bOnlyTrueIfOnSegment) {
			return PointIsOnLine(x, y, x1, y1, x2, y2, bLinePointOrderHasBeenFixedAlready, bOnlyTrueIfOnSegment, 0, 0);
		}
		///<summary>
		///send line_r of zero in order to calculate r and theta for relative line polar coords.
		///</summary>
		public static bool PointIsOnLine(int x, int y, int x1, int y1, int x2, int y2, bool bLinePointOrderHasBeenFixedAlready, bool bOnlyTrueIfOnSegment, int line_r, int line_theta) {
			if (!bLinePointOrderHasBeenFixedAlready) OrderPointsLR(ref x1, ref y1, ref x2, ref y2);
			int relative_r,relative_theta;
			bool bReturn=false;
			if (bOnlyTrueIfOnSegment) {
				if (!IsInBox(x,y,x1,x2,y1,y2))
					return false;//MUST force return now for this to work
			}
			if (line_r==0) RectToPolar(out line_r, out line_theta, x2-x1, y2-y1);
			OrderPointsLR(ref x1, ref y1, ref x, ref y);//makes point 2 to the right (may be above or below)
			RectToPolar(out relative_r, out relative_theta, x-x1, y-y1);//this is right --subtraction is right since x was placed AFTER x1
			if (relative_theta==line_theta) bReturn=true;
			return bReturn;
		}//end PointIsOnLine
		public static int Intersection(out int x, out int y, ILine line1, ILine line2) {
			return Intersection(out x, out y, line1.X1, line1.Y1, line1.X2, line1.Y2,
														 line2.X1, line2.Y1, line2.X2, line2.Y2);
		}
		public static int Intersection(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2) {
			return Intersection(out x, out y, Line1_x1, Line1_y1, Line1_x2, Line1_y2, Line2_x1, Line2_y1, Line2_x2, Line2_y2, true);
		}
		public static int Intersection(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2, bool bReturn1IfWithinSegmentElseIfNotThen2_IfThisVarIsFalseOrMissingAndDoesIntersectThenReturn1) {
			x=0;
			y=0;
			int iReturn=IntersectionError;
			try {
	
				int Line1_A, Line1_B, Line1_C, Line2_A, Line2_B, Line2_C;//line in format Ax+Bx==C
				Line1_A = Line1_y2-Line1_y1;
				Line1_B = Line1_x1-Line1_x2;
				Line1_C = Line1_A*Line1_x1+Line1_B*Line1_y1;
				Line2_A = Line2_y2-Line2_y1;
				Line2_B = Line2_x1-Line2_x2;
				Line2_C = Line2_A*Line2_x1+Line2_B*Line2_y1;
				int iDeterminant = Line1_A*Line2_B - Line2_A*Line1_B;
				if (iDeterminant==0) {
					iReturn=IntersectionNo;
				}
				else {
					x=(Line2_B*Line1_C - Line1_B*Line2_C)/iDeterminant;
					y=(Line1_A*Line2_C - Line2_A*Line1_C)/iDeterminant;
					if (bReturn1IfWithinSegmentElseIfNotThen2_IfThisVarIsFalseOrMissingAndDoesIntersectThenReturn1) {
						if (  IsInBox(x,y,Line1_x1,Line1_y1,Line1_x2,Line1_y2)
							&& IsInBox(x,y,Line2_x1,Line2_y1,Line2_x2,Line2_y2) ) iReturn=IntersectionYes;
						else iReturn=IntersectionBeyondSegment;
					}
					else iReturn=IntersectionYes;
				}
			}
			catch (Exception exn) {
				iReturn=IntersectionError;
				Base.ShowExn(exn,"Base Intersection","checking line intersection {"
					+"Line1:("+Line1_x1.ToString()+","+Line1_y1.ToString()+")to("+Line1_x2.ToString()+","+Line1_y2.ToString()+"); "
					+"Line2:("+Line2_x1.ToString()+","+Line2_y1.ToString()+")to("+Line2_x2.ToString()+","+Line2_y2.ToString()+"); "
					+"}");
			}
			return iReturn;
		}//end Intersection
		public static int SafeIntersection(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2) {
			return SafeIntersection(out x, out y, Line1_x1, Line1_y1, Line1_x2, Line1_y2, Line2_x1, Line2_y1, Line2_x2, Line2_y2, true);
		}
		public static int SafeIntersection(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2, bool bReturn1IfWithinSegmentElseIfNotThen2_IfThisVarIsFalseOrMissingAndDoesIntersectThenReturn1) {
			x=0;
			y=0;
			int iReturn=IntersectionError;//should NEVER happen!
			int Line1_A, Line1_B, Line1_C, Line2_A, Line2_B, Line2_C;//line in format Ax+Bx==C
			Line1_A = SafeSubtract(Line1_y2,Line1_y1);
			Line1_B = SafeSubtract(Line1_x1,Line1_x2);
			Line1_C = SafeAdd(SafeMultiply(Line1_A,Line1_x1),SafeMultiply(Line1_B,Line1_y1));
			Line2_A = SafeSubtract(Line2_y2,Line2_y1);
			Line2_B = SafeSubtract(Line2_x1,Line2_x2);
			Line2_C = SafeAdd(SafeMultiply(Line2_A,Line2_x1),SafeMultiply(Line2_B,Line2_y1));
			int iDeterminant=SafeSubtract(SafeMultiply(Line1_A,Line2_B),SafeMultiply(Line2_A,Line1_B));
			if (iDeterminant==0) {
				iReturn=IntersectionNo;
			}
			else {
				x=SafeDivide( SafeSubtract(SafeMultiply(Line2_B,Line1_C),SafeMultiply(Line1_B,Line2_C)) , iDeterminant,int.MaxValue );
				y=SafeDivide( SafeSubtract(SafeMultiply(Line1_A,Line2_C),SafeMultiply(Line2_A,Line1_C)), iDeterminant,int.MaxValue );
				if (bReturn1IfWithinSegmentElseIfNotThen2_IfThisVarIsFalseOrMissingAndDoesIntersectThenReturn1) {
					if (  IsInBox(x,y,Line1_x1,Line1_y1,Line1_x2,Line1_y2)
						&& IsInBox(x,y,Line1_x1,Line1_y1,Line1_x2,Line1_y2) ) iReturn=IntersectionYes;
					else iReturn=IntersectionBeyondSegment;
				}
				else iReturn=IntersectionYes;
			}
			return iReturn;
		}//end SafeIntersection
		
		//TODO: test this
		public static int IntersectionAndRelationship(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2) {
			int Line1_r, Line1_theta, Line2_r, Line2_theta, relative_r, relative_theta;
			int iIntersection=Intersection(out x, out y, Line1_x1, Line1_y1, Line1_x2, Line1_y2, Line2_x1, Line2_y1, Line2_x2, Line2_y2);
			x=0; y=0;
			if (iIntersection!=IntersectionYes) {//Line1_theta==Line2_theta
				if (Line1_x1==Line2_x2&&Line1_y1==Line2_y2) {
					return LineRelationshipParallelSameLine;
				}
				else {
					RectToPolar(out Line1_r, out Line1_theta, Line1_x2-Line1_x1, Line1_y2-Line1_y1);
					RectToPolar(out relative_r, out relative_theta, Line2_x2-Line1_x1, Line2_y2-Line1_y1);
					if (relative_theta==Line1_theta) {
						return LineRelationshipParallelSameLine;//another way of finding if same line
					}
				}
				return LineRelationshipParallelDifferentLine;
			}
			else {
				OrderPointsLR(ref Line1_x1, ref Line1_y1, ref Line1_x2, ref Line1_y2);
				OrderPointsLR(ref Line2_x1, ref Line2_y1, ref Line2_x2, ref Line2_y2);
				RectToPolar(out Line1_r, out Line1_theta, Line1_x2-Line1_x1, Line1_y2-Line1_y1);
				RectToPolar(out Line2_r, out Line2_theta, Line2_x2-Line2_x1, Line2_y2-Line2_y1);
				try {
					if (Base.PointIsOnLine(Line2_x1,Line2_y1,Line1_x1,Line1_y1,Line1_x2,Line1_y2,true,true,Line1_theta,Line1_r)) {
						x=Line2_x1;
						y=Line2_y1;
						return LineRelationshipLineBPoint1IsOnLineA;
					}
					else if (Base.PointIsOnLine(Line2_x2,Line2_y2,Line1_x1,Line1_y1,Line1_x2,Line1_y2,true,true,Line1_theta,Line1_r)) {
						x=Line2_x2;
						y=Line2_y2;
						return LineRelationshipLineBPoint2IsOnLineA;
					}
					else if (iIntersection==IntersectionYes) {
						if (IsInBox(x,y,Line1_x1,Line1_y1,Line1_x2,Line1_y2)&&IsInBox(x,y,Line2_x1,Line2_y1,Line2_x2,Line2_y2)) return LineRelationshipIntersectionNotTouchingEndsOfLineB;
						else return LineRelationshipIntersectionOutOfRange;
					}
					else {
						return LineRelationshipIntersectionOutOfRange;
					}
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"Base IntersectionAndRelationship","checking line relationship {"
						+"Line1:("+Line1_x1.ToString()+","+Line1_y1.ToString()+")to("+Line1_x2.ToString()+","+Line1_y2.ToString()+"); "
						+"Line2:("+Line2_x1.ToString()+","+Line2_y1.ToString()+")to("+Line2_x2.ToString()+","+Line2_y2.ToString()+"); "
						+"}");
					return LineRelationshipIntersectionOutOfRange;
				}
			}
			return LineRelationshipNone;
		}//end IntersectionAndRelationship
		public static void RectToPolar(out float r, out float theta, float x, float y) {
			RectToPolar(out r, out theta, ref x, ref y);
		}
		public static void RectToPolar(out float r, out float theta, ref float x, ref float y) {
			r=Base.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (Base.SafeAtan2Radians(y,x)*F180_DIV_PI) : 0 ;
		}
		public static void RectToPolar(out double r, out double theta, double x, double y) {
			r=Base.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (Base.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 ;
		}
		public static void RectToPolar(out int r, out int theta, int x, int y) {
			r=Base.SafeSqrt( SafeAdd(SafeMultiply(x,x),SafeMultiply(y,y)) );
			theta=(y!=0 || x!=0) ? (int)(Base.SafeAtan2Radians((double)y,(double)x)*D180_DIV_PI+.5) : 0 ;//+.5 for rounding; debug performance--conversion to int
		}
		public static double XOFRTHETA_RAD(double r, double theta) {	
			return SafeMultiply(r,Math.Sin(theta));//debug performance
		}
		public static double YOFRTHETA_RAD(double r, double theta) {	
			return SafeMultiply(r,Math.Cos(theta));//debug performance
		}
		public static float XOFRTHETA_RAD(float r, float theta) {	
			return (float)((double)r*Math.Cos((double)theta));//commented for debug only (needs fix): return SafeMultiply(r,SafeConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static float YOFRTHETA_RAD(float r, float theta) {	
			return (float)((double)r*Math.Sin((double)theta));//commented for debug only (needs fix): return SafeMultiply(r,SafeConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static double XOFRTHETA_DEG(double r, double theta) {	
			return SafeMultiply(r,Math.Cos(theta*DPI_DIV_180));//debug performance
		}
		public static double YOFRTHETA_DEG(double r, double theta) {	
			return SafeMultiply(r,Math.Sin(theta*DPI_DIV_180));//debug performance
		}
		public static float XOFRTHETA_DEG(float r, float theta) {	
			return (float)((double)r*Math.Cos((double)theta*DPI_DIV_180));//commented for debug only (needs fix): return SafeMultiply(r,SafeConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static float YOFRTHETA_DEG(float r, float theta) {	
			return (float)((double)r*Math.Sin((double)theta*DPI_DIV_180));//commented for debug only (needs fix): return SafeMultiply(r,SafeConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static void RectToPolar(out double r, out double theta, ref double x, ref double y) {
			r=Base.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (Base.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 ;
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
				Base.IgnoreExn(exn,"PolarToRect(double)");//debug silently degrades
				if (theta==r0) {
					x=r;
					y=0;
				}
				else if (theta==r90) {
					x=0;
					y=r;
				}
				else if (theta==r180) {
					x=-r;
					y=0;
				}
				else if (theta==r270) {
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
			catch (Exception exn) {
				Base.IgnoreExn(exn,"PolarToRect(float)");//debug silently degrade
				if (theta==r0) {
					x=r;
					y=0;
				}
				else if (theta==r90) {
					x=0;
					y=r;
				}
				else if (theta==r180) {
					x=-r;
					y=0;
				}
				else if (theta==r270) {
					x=0;
					y=-r;
				}
				else {
					x=0;
					y=0;
				}
			}//end catch
		}//end PolarToRect
		/// <summary>
		/// Safely returns arctangent of y/x, correcting
		/// for the error domain.
		/// </summary>
		/// <param name="y"></param>
		/// <param name="x"></param>
		/// <returns>arctangent of y/x, correcting
		/// for the error domain</returns>
		public static float SafeAtan2Radians(float y, float x) {
			if (x==0&&y==0) return 0;
			else return (float)Math.Atan2((double)y,(double)x);//debug assumes no need for SafeConvert.ToFloat since started with float range (?)
		}
		//public static REAL SafeAtan2Radians(REAL y, REAL x) {
		//	if (x==0&&y==0) return 0;
		//	else return (REAL)Math.Atan2((double)y,(double)x);
		//}
		public static double SafeAtan2Radians(double y, double x) {
			if (x==0&&y==0) return 0;
			else return Math.Atan2(y,x);
		}
		#endregion geometry
		
		#region math
		public static int AlignC(int widthOuter, int widthInner) {//formerly Centered
			return (widthOuter-widthInner)/2;
		}
		public static float AlignC(float widthOuter, float widthInner) {
			return (widthOuter-widthInner)/2.0f;
		}
		public static double AlignC(double widthOuter, double widthInner) {
			return (widthOuter-widthInner)/2.0;
		}
		public static decimal AlignC(decimal widthOuter, decimal widthInner) {
			return (widthOuter-widthInner)/2.0M;
		}
		//TODO: test this--all wrapping methods!
		public static int Wrap(int iWrap, int Length) {
			return (iWrap>=Length) ? (iWrap%=Length) : ((iWrap<0)?(Length-((-iWrap)%Length)):(iWrap)) ;
		}
		public static float Wrap(float val, float start, float endexclusive) {
			float range=endexclusive-start;
			if (val>=endexclusive) {
				val-=SafeConvert.ToFloat(range*(System.Math.Floor((val-endexclusive)/range)+1.0F));
			}
			else if (val<start) {
				val+=SafeConvert.ToFloat(range*(System.Math.Floor((start-val)/range)+1.0F)); //i.e. -256.5 from 0 excluding 256 is:
				//-256.5+256*(Floor((0[--a.k.a.+]256.5)/256)+1)==-256.5+256*(Floor(256.5/256)+1)==-256.5+256*(1+1)==-256.5+512==255.5
			}
			return val;
		}
		public static double Wrap(double val, double start, double endexclusive) {
			double range=endexclusive-start;
			if (val>=endexclusive) {
				val-=range*(System.Math.Floor((val-endexclusive)/range)+1);
			}
			else if (val<start) {
				val+=range*(System.Math.Floor((start-val)/range)+1); //i.e. -256.5 from 0 excluding 256 is:
				//-256.5+256*(Floor((0[--a.k.a.+]256.5)/256)+1)==-256.5+256*(Floor(256.5/256)+1)==-256.5+256*(1+1)==-256.5+512==255.5
			}
			return val;
		}
		public static decimal Wrap(decimal val, decimal start, decimal endexclusive) {
			decimal range=endexclusive-start;
			if (val>=endexclusive) {
				val-=range*(Floor((val-endexclusive)/range)+1.0M);
			}
			else if (val<start) {
				val+=range*(Floor((start-val)/range)+1.0M); //i.e. -256.5 from 0 excluding 256 is:
				//-256.5+256*(Floor((0[--a.k.a.+]256.5)/256)+1)==-256.5+256*(Floor(256.5/256)+1)==-256.5+256*(1+1)==-256.5+512==255.5
			}
			return val;
		}
		public static decimal Floor(decimal val) {//debug performance VERY SLOW
			string sVal=val.ToString();
			SafeConvert.RemoveExpNotation(ref sVal);
			int iDot=sVal.IndexOf(".");
			if (iDot>=0) {
				sVal=SafeSubstring(sVal,0,iDot);
				return SafeConvert.ToDecimal(sVal);
			}
			else return val;
		}
		public static int GetSignedCropped(uint uiNow) {
			return (int)((uiNow>2147483647)?2147483647:uiNow);
			//1111111 11111111 11111111 11111111
		}
		public static ushort GetUnsignedLossless(short val) {
			if (val==short.MinValue) return ushort.MaxValue;//prevents overflow! (in -1*val below)
			else if (val<0) return (ushort)((ushort)short.MaxValue+(ushort)(-1*val));//since approaches 0x7FFF+0xFFFF (that overflow prevented above)
			else return (ushort) val;
		}
		public static byte Dist(byte by1, byte by2) {
			return (byte)( (by1>by2)?by1-by2:by2-by1 );
		}
		public static double Dist(ref DPoint p1, ref DPoint p2) {
			try {
				return Base.SafeSqrt(System.Math.Abs(p2.X-p1.X)+System.Math.Abs(p2.Y-p1.Y));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"double Dist()");
			}
			return 0;
		}
		public static double Dist(double x1, double y1, double x2, double y2) {
			try {
				return Base.SafeSqrt(System.Math.Abs(x2-x1)+System.Math.Abs(y2-y1));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"double Dist()");
			}
			return 0;
		}
		public static float Dist(float x1, float y1, float x2, float y2) {
			try {
				return Base.SafeSqrt(System.Math.Abs(x2-x1)+System.Math.Abs(y2-y1));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"double Dist()");
			}
			return 0;
		}
		public static int Dist(int x1, int y1, int x2, int y2) {
			try {
				return Base.SafeSqrt(System.Math.Abs(x2-x1)+System.Math.Abs(y2-y2));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"double Dist()");
			}
			return 0;
		}
		public static float Dist(ref FPoint p1, ref FPoint p2) {
			try {
				return Base.SafeSqrt(System.Math.Abs(p2.X-p1.X)+System.Math.Abs(p2.Y-p1.Y));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"float Dist()");
			}
			return 0;
		}
		public static REAL Dist(ref RPoint p1, ref RPoint p2) {
			try {
				return Base.SafeSqrt(System.Math.Abs(p2.X-p1.X)+System.Math.Abs(p2.Y-p1.Y));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"REAL Dist()");
			}
			return 0;
		}
		public static double SafeDivide(double val, double valDivisor, double valMax) {
			return SafeDivide(val,valDivisor,valMax,-valMax);
		}
		public static double SafeDivide(double val, double valDivisor, double valMax, double valMin) {
			try {
				bool bSameSign=(val<0.0&&valDivisor<0.0)?true:((val>=0.0&&valDivisor>=0.0)?true:false);
				if (valDivisor==0) {
					if (val>0) return valMax;
					else if (val<0) return valMin;
					else return 0;
				}
				//replaced by +inf and -inf below //else if (double.IsInfinity(val)) return valMax;
				else if (double.IsPositiveInfinity(val)) {
					if (double.IsPositiveInfinity(valDivisor)) return 1.0;
					else if (bSameSign) {
						return valMax;
					}
					else {
						if (double.IsNegativeInfinity(valDivisor)) return -1;
						else return valMin; //since not same sign
					}
				}
				else if (double.IsNegativeInfinity(val)) {
					if (double.IsNegativeInfinity(valDivisor)) return 1.0;
					else if (bSameSign) {
						return valMax;
					}
					else {
						if (double.IsPositiveInfinity(valDivisor)) return -1;
						else return valMin; //since not same sign (i.e. -inf/2.0)
					}
				}
				else if (double.IsPositiveInfinity(valDivisor)) {
					if (double.IsPositiveInfinity(val)) return 1.0;
					else if (bSameSign) {
						return valMax;
					}
					else {
						if (double.IsNegativeInfinity(val)) return -1;
						else return valMin;
					}
				}
				else if (double.IsNegativeInfinity(valDivisor)) {
					if (double.IsNegativeInfinity(val)) return 1.0;
					else if (bSameSign) {
						return valMax;
					}
					else {
						if (double.IsNegativeInfinity(val)) return 1;
						else return valMin;
					}
				}
					//TODO: finish this (cases of inf or -inf denominator)
				//TODO: output error if NaN?
				else if (double.IsNaN(val)) return 0;
				else if (double.IsNaN(valDivisor)) return 0;
				else return val/valDivisor;
			}
			catch (Exception exn)  {
				Base.ShowExn(exn,"SafeDivide("+val.ToString()+","+valDivisor.ToString()+","+valMax.ToString()+")");
			}
			return 0;
		} //end SafeDivide
		public static int SafeAbs(int val) {
			SafeAbsByRef(ref val);
			return val;
		}
		public static void SafeAbsByRef(ref int val) {
			if (val<0) {
				if (val<int.MaxValue*-1) val=int.MaxValue;
				else val*=-1;
			}
		}
		public static float SafeAbs(float val) {
			SafeAbsByRef(ref val);
			return val;
		}
		public static void SafeAbsByRef(ref float val) {
			if (val<0) {
				if (val<float.MaxValue*-1.0f) val=float.MaxValue;
				else val*=-1;
			}
		}
		public static int Negative(int val) {
			NegativeByRef(ref val);
			return val;
		}
		public static void NegativeByRef(ref int val) {
			if (val>0) {
				val*=-1;
			}
		}
		public static int SafeDivideRound(int val, int valDivisor, int valMax) {
			int iReturn=SafeDivide(val,valDivisor,valMax,0);
			bool bNeg=false;
			if (val<0) bNeg=!bNeg;
			if (valDivisor<0) bNeg=!bNeg;
			if (!bNeg) {
				if (val%valDivisor>SafeAbs(SafeDivide(valDivisor,2,valMax,0))) iReturn++;
			}
			else {
				if (val%valDivisor>Negative(SafeDivide(valDivisor,2,valMax,0))) iReturn--;
			}
			return iReturn;
		}
		public static int SafeDivideRound(int val, int valDivisor, int valMax, int valMin) {
			int iReturn=SafeDivide(val,valDivisor,valMax,valMin);
			bool bNeg=false;
			if (val<0) bNeg=!bNeg;
			if (valDivisor<0) bNeg=!bNeg;
			if (!bNeg) {
				if (val%valDivisor>SafeAbs(SafeDivide(valDivisor,2,valMax,valMin))) iReturn++;
			}
			else {
				if (val%valDivisor>Negative(SafeDivide(valDivisor,2,valMax,valMin))) iReturn--;
			}
			return iReturn;
		}
		public static int SafeDivide(int val, int valDivisor, int valMax) {
			return SafeDivide(val,valDivisor,valMax,-valMax);
		}
		public static int SafeDivide(int val, int valDivisor, int valMax, int valMin) {
			bool bNeg=false;
			try {
				bool bSameSign=(val<0.0&&valDivisor<0.0)?true:((val>=0.0&&valDivisor>=0.0)?true:false);
				if (valDivisor==0) {
					if (val>0) return valMax;
					else if (val<0) return valMin;
					else return 0;
				}
				else {
					if (val<0) {
						val*=-1;
						bNeg=!bNeg;
					}
					if (valDivisor<0) {
						valDivisor*=-1;
						bNeg=!bNeg;
					}
				}
				try {
					return bNeg?(val/valDivisor*-1):(val/valDivisor);
				}
				catch {
					return bNeg? ((val>valDivisor?valMax:valMin)*-1) : (val>valDivisor?valMax:valMin);
				}
			}
			catch (Exception exn)  {//should NEVER happen
				Base.ShowExn(exn,"SafeDivide","dividing "+val.ToString()+" by "+valDivisor.ToString()+" (unexpected crash) {min:"+valMin.ToString()+"; max="+valMax.ToString()+"}");
			}
			return 0;
		} //end SafeDivide(int...)
		//public const float MaxFloat=;
		//public const float MinFloatAbsVal=;
		public static float FixFloat(float var) {
			if (float.IsNegativeInfinity(var)) {
				var=float.MinValue;
			}
			else if (float.IsPositiveInfinity(var)) {
				var=float.MaxValue;
			}
			else if (float.IsNaN(var)) {
				var=0;
			}
			return var;
		}
		//public const double MaxDouble;
		public static double FixDouble(double var) {
			if (double.IsNegativeInfinity(var)) {
				var=double.MinValue;
			}
			else if (double.IsPositiveInfinity(var)) {
				var=double.MaxValue;
			}
			else if (double.IsNaN(var)) {
				var=0;
			}
			return var;
		}
		public static uint Bit(int iBit) {
			return Base.SafePow((uint)2,iBit);
		}
		//can't be const, since passed by ref
		private static int i10=10;
		private static long l10=10L;
		private static float f10=10F;
		private static double d10=10D;
		private static decimal m10=10M;
		
		public static int SafeE10I(int exp) {
			return SafePow(ref i10, ref exp);
		}
		public static long SafeE10L(int exp) {
			return SafePow(ref l10, ref exp);
		}
		public static float SafeE10F(ref int exp) {
			return SafePow(ref f10, ref exp);
		}
		public static double SafeE10D(ref int exp) {
			return SafePow(ref d10, ref exp);
		}
		public static decimal SafeE10M(ref int exp) {
			return SafePow(ref m10, ref exp);
		}
		public static int SafePow(int basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static int SafePow(ref int basenum, ref int exp) {
			if (basenum==0) return 0;
			int result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				try {
					for (int iCount=1; iCount<exp; iCount++) {
						//if (result<int.MaxValue-basenum) //useless
							result*=basenum;
						//else return int.MaxValue;
					}
				}
				catch {
					return int.MaxValue;
				}
			}
			if (bNeg) {
				result=1/result;
				exp*=-1;//leaves it the way we found it
			}
			return result;
		}
		public static uint SafePow(uint basenum, int exp) {
			return SafePow(ref basenum,ref exp);
		}
		public static uint SafePow(ref uint basenum, ref int exp) {
			if (basenum==0) return 0;
			uint result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				try {
					for (uint iCount=1; iCount<exp; iCount++) {
						//if (result<uint.MaxValue-basenum) //useless
							result*=basenum;
						//else return uint.MaxValue;
					}
				}
				catch {
					return uint.MaxValue;
				}
			}
			if (bNeg) {
				result=1/result;
				exp*=-1;//leaves it the way we found it
			}
			return result;
		}
		public static long SafePow(long basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static long SafePow(ref long basenum, ref int exp) {
			if (basenum==0) return 0;
			long result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				try {
					for (int iCount=1; iCount<exp; iCount++) {
						//if (result<long.MaxValue-basenum) //useless
							result*=basenum;
						//else return long.MaxValue;
					}
				}
				catch {
					return long.MaxValue;
				}
			}
			if (bNeg) {
				result=1L/result;
				exp*=-1;//leaves it the way we found it
			}
			return result;
		}
		public static float SafePow(float basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static float SafePow(ref float basenum, ref int exp) {
			float result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				try {
					for (int iCount=1; iCount<exp; iCount++) {
						//if (result<float.MaxValue-basenum) //useless
							result*=basenum;
						//else return float.MaxValue;
					}
				}
				catch {
					return float.MaxValue;
				}
			}
			if (bNeg) {
				if (result!=0.0f) result=1F/result; //check now since may have degraded to zero if started at <1.0f
				exp*=-1;//leaves it the way we found it
			}
			return result;
		}
		public static double SafePow(double basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static double SafePow(ref double basenum, ref int exp) {
			double result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				try {
					for (int iCount=1; iCount<exp; iCount++) {
						//if (result<double.MaxValue-basenum) //useless
							result*=basenum;
						//else return double.MaxValue;
					}
				}
				catch {
					return double.MaxValue;
				}
			}
			if (bNeg) {
				if (result!=0.0D) result=1D/result; //check now since may have degraded to zero if started at <1.0				
				exp*=-1; //leaves it the way we found it
			}
			return result;
		}
		public static decimal SafePow(decimal basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static decimal SafePow(ref decimal basenum, ref int exp) {
			decimal result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1M;
			else {
				bNeg=false;
				result=basenum;
				try {
					for (int iCount=1; iCount<exp; iCount++) {//start at once since set result=basenum already
						//if (result<decimal.MaxValue-basenum) //useless
							result*=basenum;
						//else return decimal.MaxValue;
					}
				}
				catch {
					return decimal.MaxValue;
				}
			}
			if (bNeg) {
				if (result!=0.0M) result=1M/result; //check now since may have degraded to zero if started at <1.0				
				exp*=-1; //leaves it the way we found it
			}
			return result;
		}
		public static void Rotate(ref float xToMove, ref float yToMove, float xCenter, float yCenter, float fRotate) {
			xToMove-=xCenter;
			yToMove-=yCenter;
			float rTemp=ROFXY(xToMove,yToMove), thetaTemp=THETAOFXY_RAD(xToMove,yToMove);
			thetaTemp+=fRotate;
			xToMove=XOFRTHETA_RAD(rTemp,thetaTemp);
			yToMove=YOFRTHETA_RAD(rTemp,thetaTemp);
			xToMove+=xCenter;
			yToMove+=yCenter;
		}
		public static void Rotate(ref float xToMove, ref float yToMove, float fRotate) {
			float rTemp=ROFXY(xToMove,yToMove), thetaTemp=THETAOFXY_RAD(xToMove,yToMove);
			thetaTemp+=fRotate;
			xToMove=XOFRTHETA_RAD(rTemp,thetaTemp);
			yToMove=YOFRTHETA_RAD(rTemp,thetaTemp);
		}
		public static byte Approach(byte start, byte toward, float factor) {
			try {
				return ByRound(((float)start)-(((float)start)-((float)toward))*(factor));
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base Approach(byte)");
				//TODO: make this more accurate
				return toward; //check this--may be correct since overflow means too big in the formula above
			}
		}
		public static float Approach(float start, float toward, float factor) {
			try {
				return ((start)-((start)-(toward))*(factor));
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Base Approach");
				//TODO: make this more accurate
				return toward; //check this--may be correct since overflow means too big in the formula above
			}
		}
		public static double Approach(double start, double toward, double factor) {
			try {
				return ((start)-((start)-(toward))*(factor));
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"Approach");
				//TODO: make this more accurate
				return toward; //check this--may be correct since overflow means too big in the formula above
			}
		}
		//public static float Mod(float num, float div) {
			//float result=num/div;
			//Floor(ref result);
			//result=num-result;
			//return result;
		//}
		public static float Mod(float val, float divisor) { //formerly FMOD
			return ((val>divisor) ? ( val - Floor(val/divisor)*divisor) : 0 );
		}
		public static double Mod(double val, double divisor) { //formerly DMOD
			return ((val>divisor) ? ( val - System.Math.Floor(val/divisor)*divisor) : 0 );
		}
		
		public static float Floor(float num) {
			Floor(ref num);
			return num;
		}
		public static float FloorRefToNonRef(ref float num) {
			float numNew=num;
			Floor(ref numNew);
			return numNew;
		}
		public static void Floor(ref float num) {
			//bool bOverflow=false;//TODO: check for overflow
			if (num!=0F) {
				long whole=(long)num;
				num=(float)whole;
			}
		}
		//public static double Floor(double num) {//use System.Math.Floor, which has double and decimal
		//	Floor(ref num);
		//	return num;
		//}
		//public static double FloorRefToNonRef(ref double num) {
		//	double numNew=num;
		//	Floor(ref numNew);
		//	return numNew;
		//}
		//public static void Floor(ref double num) {
		//	//bool bOverflow=false;//TODO: check for overflow
		//	if (num!=0F) {
		//		ulong whole=(ulong)num;
		//		num=(double)whole;
		//	}
		//}
		public static byte SafeSubtract(byte var, byte subtract) {
			return SubtractBytes[(int)var][(int)subtract];
		}
		public static float SafeSubtract(float var, float subtract) {
			PrepareToBePos(ref subtract);
			return SafeAdd(var, -1*subtract);
		}
		public static float SafeAdd(float var1, float var2) {
			if (var1<0) {
				if (var2<0) {
					if (float.MinValue-var1>var2) return var1+var2;
					else return float.MinValue;
				}
				else { //var2>=0
					return var1+var2;
				}
			}
			else { //var1>=0
				if (var2<0) {
					return var1+var2;
				}
				else { //var2>=0
					if (float.MaxValue-var1>var2) return var1+var2;
					else return float.MaxValue;
				}
			}
		}//end SafeAdd
		public static double SafeSubtract(double var, double subtract) {
			PrepareToBePos(ref subtract);
			return SafeAdd(var, -1*subtract);
		}
		public static double SafeAdd(double var1, double var2) {
			if (var1<0) {
				if (var2<0) {
					if (double.MinValue-var1>var2) return var1+var2;
					else return double.MinValue;
				}
				else { //var2>=0
					return var1+var2;
				}
			}
			else { //var1>=0
				if (var2<0) {
					return var1+var2;
				}
				else { //var2>=0
					if (double.MaxValue-var1>var2) return var1+var2;
					else return double.MaxValue;
				}
			}
		}//end SafeAdd
		public static int SafeSubtract(int var, int subtract) {
			PrepareToBePos(ref subtract);
			return SafeAdd(var, -1*subtract);
		}
		public static int SafeAdd(int var1, int var2) {
			if (var1<0) {
				if (var2<0) {
					if (int.MinValue-var1>var2) return var1+var2;
					else return int.MinValue;
				}
				else { //var2>=0
					return var1+var2;
				}
			}
			else { //var1>=0
				if (var2<0) {
					return var1+var2;
				}
				else { //var2>=0
					if (int.MaxValue-var1>var2) return var1+var2;
					else return int.MaxValue;
				}
			}
		}//end SafeAdd
		public static int SafeAddWrappedTowardZero(int var1, int var2) {
			if (var1<0) {
				if (var2<0) {
					if (int.MinValue-var1>var2) return var1+var2;
					else {
						//return int.MinValue;
						return (var1-int.MinValue)+var2;
					}
				}
				else { //var2>=0
					return var1+var2;
				}
			}
			else { //var1>=0
				if (var2<0) {
					return var1+var2;
				}
				else { //var2>=0
					if (int.MaxValue-var1>var2) return var1+var2;
					else {
						return (var1-int.MaxValue)+var2;//(var1+var2)-(int.MaxValue);
					}
				}
			}
		}//end SafeAddWrappedTowardZero
		public static void SafeSignChangeByRef(ref int val) {
			if (val==int.MinValue) val=int.MaxValue;
			else val*=-1;
		}
		public static void SafeSignChangeByRef(ref long val) {
			if (val==long.MinValue) val=long.MaxValue;
			else val*=-1;
		}
		public static void SafeSignChangeByRef(ref float val) {
			if (val==float.MinValue) val=float.MaxValue;
			else val*=-1;
		}
		public static void SafeSignChangeByRef(ref double val) {
			if (val==double.MinValue) val=double.MaxValue;
			else val*=-1;
		}
		public static void SafeSignChangeByRef(ref decimal val) {
			if (val==decimal.MinValue) val=decimal.MaxValue;
			else val*=-1;
		}
		public static int SafeMultiply(int val1, int val2) {
			int valReturn=0;
			bool bNeg=false;
			if (val1<0) {
				SafeSignChangeByRef(ref val1);
				bNeg=!bNeg;
			}
			if (val2<0) {
				SafeSignChangeByRef(ref val2);
				bNeg=!bNeg;
			}
			for (int iNow=0; iNow<val2; iNow++) {
				valReturn=SafeAdd(valReturn, val1);//i.e. if val2 is 1, this happens once
			}
			if (bNeg) SafeSignChangeByRef(ref valReturn);
			return valReturn;
		}//end int SafeMultiply
		public static float SafeMultiply(float val1, float val2) {
			float valReturn=0;
			bool bNeg=false;
			if (val1<0) {
				SafeSignChangeByRef(ref val1);
				bNeg=!bNeg;
			}
			if (val2<0) {
				SafeSignChangeByRef(ref val2);
				bNeg=!bNeg;
			}
			int iLimiter=(int)val2;
			for (int iNow=0; iNow<iLimiter; iNow++) {
				valReturn=SafeAdd(valReturn, val1);//i.e. if val2 is one, this happens once
			}
			valReturn=SafeAdd(valReturn, val1*(val2-(float)iLimiter));//remainder
			if (bNeg) SafeSignChangeByRef(ref valReturn);
			return valReturn;
		}//end float SafeMultiply
		public static double SafeMultiply(double val1, double val2) {
			double valReturn=0;
			bool bNeg=false;
			if (val1<0) {
				SafeSignChangeByRef(ref val1);
				bNeg=!bNeg;
			}
			if (val2<0) {
				SafeSignChangeByRef(ref val2);
				bNeg=!bNeg;
			}
			int iLimiter=(int)val2;
			for (int iNow=0; iNow<iLimiter; iNow++) {
				valReturn=SafeAdd(valReturn, val1);//i.e. if val2 is one, this happens once
			}
			valReturn=SafeAdd(valReturn, val1*(val2-(double)iLimiter));//remainder
			if (bNeg) SafeSignChangeByRef(ref valReturn);
			return valReturn;
		}//end double SafeMultiply
		public const uint BIT1		=1;
		public const uint BIT2		=2;
		public const uint BIT3		=4;
		public const uint BIT4		=8;
		public const uint BIT5		=16;
		public const uint BIT6		=32;
		public const uint BIT7		=64;
		public const uint BIT8		=128;
		public const uint BIT9		=256;
		public const uint BIT10		=512;
		public const uint BIT11		=1024;
		public const uint BIT12		=2048;
		public const uint BIT13		=4096;
		public const uint BIT14		=8192;
		public const uint BIT15		=16384;
		public const uint BIT16		=32768;
		public const uint BIT17		=65536;
		public const uint BIT18		=131072;
		public const uint BIT19		=262144;
		public const uint BIT20		=524288;
		public const uint BIT21		=1048576;
		public const uint BIT22		=2097152;
		public const uint BIT23		=4194304;
		public const uint BIT24		=8388608;
		public const uint BIT25		=16777216;
		public const uint BIT26		=33554432;
		public const uint BIT27		=67108864;
		public const uint BIT28		=134217728;
		public const uint BIT29		=268435456;
		public const uint BIT30		=536870912;
		public const uint BIT31		=1073741824;
		public const uint BIT32		=2147483648;
		public static byte LOWNIBBLE(byte by) {
			return (byte)(by%16); //or by&byLowNibble;
		}
		public static byte HIGHNIBBLE(byte by) {
			return (byte)(by/16); //or by>>4;
		}
		public static void SETLOWNIBBLE(ref byte byTarget, byte byNibbleValueMustBeLessthan16) {
			byTarget=(byte)((byte)((byte)(byTarget/16)*16) + byNibbleValueMustBeLessthan16); //assumes by2<16
		}
		public static void SETHIGHNIBBLE(ref byte byTarget, byte byNibbleValueMustBeLessthan16) {
			byTarget=(byte)((byte)(byTarget%16) + (byNibbleValueMustBeLessthan16*16)); //assumes by1<16
		}
		public static byte ByRound(float val) {
			if (val>255) return 255; //> is okay since it will be truncated below anyway
			else if (val<0) return 0;
			return (byte)(val+.5f);
		}
		public static byte ByRound(double val) {
			if (val>255) return 255;
			else if (val<0) return 0;
			return (byte)(val+.5f);
		}
		public static int IRound(float val) {
			if (val>((float)int.MaxValue)-.5) return int.MaxValue;
			else if (val<((float)int.MinValue)+.5) return int.MinValue;
			return (int)(val+.5f);
		}
		public static int IRound(double val) {
			if (val>((double)int.MaxValue)-.5) return int.MaxValue;
			else if (val<((double)int.MinValue)+.5) return int.MinValue;
			return (int)(val+.5f);
		}
		public static int ICeiling(double val) {
			if (val>((double)int.MaxValue)) return int.MaxValue;
			else if (val<((double)int.MinValue)) return int.MinValue;
			return ((double)val>((double)((int)val)))  
				?  ((val>((double)int.MaxValue-1))?int.MaxValue:((int)val+1))  
				:  (int)val;
		}
		public static float ROFXY(float x, float y) {
			return (float)( Base.SafeSqrt( x * x + y * y ) );
		}
		public static double ROFXY(double x, double y) {
			return (double)( Base.SafeSqrt( x * x + y * y ) );
		}
		public static float THETAOFXY_DEG(float x, float y) {
			return ( (y!=0 || x!=0) ? (Base.SafeAtan2Radians(y,x)*F180_DIV_PI) : 0 );
		}
		public static double THETAOFXY_DEG(double x, double y) {
			return ( (y!=0 || x!=0) ? (Base.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 );
		}
		public static float THETAOFXY_RAD(float x, float y) {
			return ( (y!=0 || x!=0) ? (Base.SafeAtan2Radians(y,x)) : 0 );
		}
		public static double THETAOFXY_RAD(double x, double y) {
			return ( (y!=0 || x!=0) ? (Base.SafeAtan2Radians(y,x)) : 0 );
		}
		
		public static void PrepareToBePos(ref int val) {
		//limits value since IEEE positive range is narrower (PrepareToBeNeg is not needed!)
			if (val<(-1*int.MaxValue)) val=-1*int.MaxValue;
		}
		public static void PrepareToBePos(ref long val) {
			if (val>(-1*long.MaxValue)) val=-1*long.MaxValue;
		}
		public static void PrepareToBePos(ref float val) {
			if (val>(-1*float.MaxValue)) val=-1*float.MaxValue;
		}
		public static void PrepareToBePos(ref double val) {
			if (val>(-1*double.MaxValue)) val=-1*double.MaxValue;
		}
		
		public static float SafeSqrt(float val) {
			if (val>0) return (float)Math.Sqrt(val);
			else if (val<0) { //debug should not actually return a real number
				PrepareToBePos(ref val);
				return (float)(-1F*Math.Sqrt((-1*val)));
			}
			else return 0;
		}
		public static double SafeSqrt(double val) {
			if (val>0) return Math.Sqrt(val);
			else if (val<0) { //debug should not actually return a real number
				PrepareToBePos(ref val);
				return -1D*Math.Sqrt((double)(-1D*val));
			}
			else return 0;
		}
		//public static REAL SafeSqrt(REAL val) {
		//	if (val>0) return Math.Sqrt(val);
		//	else if (val<0) {
		//		PrepareToBeNeg(ref val);
		//		return (REAL)-1*Math.Sqrt((double)((REAL)-1*val));
		//	}
		//	else return 0;
		//}
		public static int SafeSqrt(int val) {
			if (val>0) return (int)Math.Sqrt(val);
			else if (val<0) { //debug should not actually return a real number
				PrepareToBePos(ref val);
				return -1*(int)Math.Sqrt((int)(-1*val));
			}
			else return 0;
		}
		public static long SafeSqrt(long val) {
			if (val>0) return (long)Math.Sqrt(val);
			else if (val<0) { //debug should not actually return a real number
				PrepareToBePos(ref val);
				return -1L*(long)Math.Sqrt((long)(-1L*val));
			}
			else return 0;
		}
		public static float FractionPartOf(float val) {
			return val-Floor(val);
		}
		public static double FractionPartOf(double val) {
			return val-System.Math.Floor(val);
		}
		#endregion math
		
		#region buffer manipulation
		
		public static byte[] SubArray(ref byte[] byarrNow, int iLocNow, int iLen) {
			byte[] byarrNew=null;
			try {
				byarrNew=new byte[iLen];
				int iByteNew=0;
				//TODO: check for bad values
				while (iByteNew<iLen) {
					byarrNew[iByteNew]=byarrNow[iLocNow];
					iByteNew++;
					iLocNow++;
				}
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"SubArray");//TODO: handle exception
			}
			return byarrNew;
		}//end SubArray
		public static byte[] SubArrayReversed(byte[] byarrNow) {
			try {
				return SubArrayReversed(byarrNow,0,byarrNow.Length);
			}
			catch (Exception exn) {	
				Base.ShowExn(exn,"SubArrayReversed(byte array)");
			}
			return null;
		}
		public static byte[] SubArrayReversed(byte[] byarrNow, int iStart) {
			try {
				return SubArrayReversed(byarrNow,iStart,byarrNow.Length-iStart);
			}
			catch (Exception exn) {	
				Base.ShowExn(exn,"SubArrayReversed(byte array, start)");
			}
			return null;
		}
		public static byte[] SubArrayReversed(byte[] byarrNow, int iLocNow, int iLen) {
			byte[] byarrNew=null;
			try {
				byarrNew=new byte[iLen];
				int iByteNew=0;
				//TODO: check for bad values
				int iLastIndex=iLen-1;
				while (iByteNew<iLen) {
					byarrNew[iLastIndex-iByteNew]=byarrNow[iLocNow];
					iByteNew++;
					iLocNow++;
				}
			}
			catch (Exception exn) {
				Base.IgnoreExn(exn,"SubArrayReversed");//TODO: handle exception
			}
			return byarrNew;
		}//end SubArrayReversed
		#endregion buffer manipulation
		
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
				Base.ShowExn(exn,"ToBase64","encoding base64");
			}
			return carrReturn;
		}	//end ToBase64
		
		public static char SixLowBitsToBase64(byte byNow) {//formerly GetBase64CharFromSixBits
			if((byNow>=0) &&(byNow<=63)) return carrBase64[(int)byNow];//good
			else return ' '; //bad
		}
		#endregion Base64
		
		#region hex conversion (hexadecimal)
		public static bool HexColorStringToBGR24(ref byte[] byarrDest, int iDest, string sHex3Or6WithOrWithoutPrecedent) {
			bool bGood=true;
			string sHex=sHex3Or6WithOrWithoutPrecedent;
			string sFirstValue=sHex;
			try {
				if (Base.IsUsedString(sHex)) {
					if (sHex.StartsWith("#")) sHex=Base.SafeSubstring(sHex,1);
					else if (sHex.StartsWith("0x")) sHex=Base.SafeSubstring(sHex,2);
					else if (sHex.StartsWith("&H")) sHex=Base.SafeSubstring(sHex,2);
					if (sHex.Length>0) {
						if (sHex.Length==8) sHex=Base.SafeSubstring(sHex,2);//remove leading alpha
						if (sHex.Length==3) {
							byarrDest[2]=(byte)(HexNibbleToByte(sHex[0])*17);//R
							byarrDest[1]=(byte)(HexNibbleToByte(sHex[1])*17);//G
							byarrDest[0]=(byte)(HexNibbleToByte(sHex[2])*17);//B
						}
						else if (sHex.Length==6) {
							byarrDest[2]=HexToByte(sHex,0);//R
							byarrDest[1]=HexToByte(sHex,2);//G
							byarrDest[0]=HexToByte(sHex,4);//B
						}
						else if (sHex.Length==8) { //i.e. if was &H00FFFFFF etc //TODO: debug byte order? OpenWC3 may need nonstandard ABGR
							byarrDest[2]=HexToByte(sHex,2);//R
							byarrDest[1]=HexToByte(sHex,4);//G
							byarrDest[0]=HexToByte(sHex,6);//B
							Base.Warning("Skipped alpha in color notation conversion.","{character-pair:\""+Base.SafeSubstring(sHex,0,2)+"\"}");
						}
						else Base.ShowErr("Can't use color in this format.","HexColorStringToBGR24","adding incorrect color notation {hex:"+Base.VariableMessage(sFirstValue,true)+"}");
					}
					else Base.ShowErr("Can't use blank color.","HexColorStringToBGR24","adding blank color {hex:"+Base.VariableMessage(sFirstValue,true)+"}");
				}
				else Base.ShowErr("Can't use empty color/name string.","HexColorStringToBGR24","adding blank color without notation {hex:"+Base.VariableMessage(sFirstValue,true)+"}");
			}
			catch (Exception exn) {
				bGood=false;
				//"Error writing to color data array"
				Base.ShowExn(exn,"Base HexColorStringToBGR24","converting color notation");
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
				Base.ShowExn(exn,"HexToByte","interpreting data {HexChars"+ (sHexChars==null?":null":(".Length:\""+sHexChars.Length.ToString()+"\"")) +"; iStartIndexFromWhichToGrabTwoChars:"+iStartIndexFromWhichToGrabTwoChars.ToString()+"; result:\""+Base.SafeSubstring(sHexChars,iStartIndexFromWhichToGrabTwoChars,2)+"\"}");
			}
			return byReturn;
		}
		public static byte HexNibbleToByte(char cHex) {//from ByteFromHexCharNibble
			byte valReturn=0;
			if (cHex<58) {
				valReturn=(byte)(((int)cHex)-48); //i.exn. changes 48 ('0') to [0]
			}
			else {
				valReturn=(byte)(((int)cHex)-55); //i.exn. changes 65 ('A') to  [10]
			}
			if (valReturn<0||valReturn>15) {
				Base.ShowErr("Failed to convert hex char.","HexNibbleToByte","interpreting data {cHex:'"+char.ToString(cHex)+"'; valReturn:"+valReturn.ToString()+";}");
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
				Base.ShowErr("Failed to convert hex char.","HexNibbleToInt","interpreting data {cHex:'"+char.ToString(cHex)+"'; valReturn:"+valReturn.ToString()+";}");
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
				Base.ShowExn(exn,"ToHex("+byValue.ToString()+")","interpreting data {carrHexDest"+((carrHexDest==null)?":null":".Length:"+carrHexDest.Length.ToString())+"; DestCharCursorToMove:"+iDestCharCursorToMove.ToString()+"}");
			}
			return bGood;
		}
		public static string ToHex(byte byVal) { //formerly HexOfByte
			string sHexReturn="00";
			try {
				sHexReturn=char.ToString(NibbleToHexChar(byVal>>4))+char.ToString(NibbleToHexChar(byVal&byLowNibble));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ToHex("+byVal.ToString()+")","interpreting data {sHexReturn:"+sHexReturn+"; byVal:"+byVal.ToString()+"}");
			}
			return sHexReturn;
		}
		public static string ToHex(byte[] byarrToHex, int iStart, int iLength, int iChunkBytes) {//formerly HexOfBytes
			string sReturn="";
			int iChunkPlace=0;
			int iByte=iStart;
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
				Base.ShowExn(exn,"","interpreting data {byarrToHex"+VariableMessageStyleOperatorAndValue(byarrToHex)+"}");
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
		
		
		#region utilities
		public static bool Redim(ref PictureBox[] arrNow, int iSetSize, string sSender_ForErrorTrackingOnly) {
			bool bGood=false;
			if (iSetSize!=SafeLength(arrNow)) {
				if (iSetSize<=0) { arrNow=null; bGood=true; }
				else {
					try {
						//bool bGood=false;
						PictureBox[] arrNew=new PictureBox[iSetSize];
						for (int iNow=0; iNow<arrNew.Length; iNow++) {
							if (iNow<SafeLength(arrNow)) arrNew[iNow]=arrNow[iNow];
							else arrNew[iNow]=null;//new PictureBox();//null;//Var.Create("",TypeNULL);
						}
						arrNow=arrNew;
						//bGood=true;
						//if (!bGood) Base.ShowErr("No vars were found while trying to set MaximumSeq!");
						bGood=true;
					}
					catch (Exception exn) {
						bGood=false;
						Base.ShowExn(exn,"Base Redim(PictureBox array)",sSender_ForErrorTrackingOnly+" setting PictureBox array Maximum");
					}
				}
			}
			else bGood=true;
			return bGood;
		}//Redim
		public static void Swap(ref int i1, ref int i2) {
			int iTemp=i1;
			i1=i2;
			i2=iTemp;
		}
		public static ImageFormat ImageFormatFromExt(string sSetFileExt) {
			string sLower=sSetFileExt.ToLower();
			if (sLower==("png")) return ImageFormat.Png;
			else if (sLower==("jpg")) return ImageFormat.Jpeg;
			else if (sLower==("jpe")) return ImageFormat.Jpeg;
			else if (sLower==("jpeg"))return ImageFormat.Jpeg;
			else if (sLower==("gif")) return ImageFormat.Gif;
			else if (sLower==("exi")) return ImageFormat.Exif;
			else if (sLower==("exif"))return ImageFormat.Exif;
			else if (sLower==("emf")) return ImageFormat.Emf;
			else if (sLower==("tif")) return ImageFormat.Tiff;
			else if (sLower==("tiff"))return ImageFormat.Tiff;
			else if (sLower==("ico")) return ImageFormat.Icon;
			else if (sLower==("wmf")) return ImageFormat.Wmf;
			else return ImageFormat.Bmp;
		}
		public static ImageFormat ImageFormatFromNameElseCapitalizedPng(ref string sNameToTruncate, out string sExt) {
			sExt=KnownExtensionFromNameElseBlank(sNameToTruncate);
			if (sExt=="") sExt="PNG";//return ImageFormat.Png;
			else Base.ShrinkByRef(ref sNameToTruncate, sExt.Length+1);
			if (sExt=="png"||sExt=="PNG") { return ImageFormat.Png; }
			else if (sExt=="jpg") { return ImageFormat.Jpeg;}
			else if (sExt=="jpe") { return ImageFormat.Jpeg;}
			else if (sExt=="jpeg"){ return ImageFormat.Jpeg;}
			else if (sExt=="gif") { return ImageFormat.Gif; }
			else if (sExt=="exi") { return ImageFormat.Exif;}
			else if (sExt=="exif"){ return ImageFormat.Exif;}
			else if (sExt=="emf") { return ImageFormat.Emf; }
			else if (sExt=="tif") { return ImageFormat.Tiff;}
			else if (sExt=="tiff"){ return ImageFormat.Tiff;}
			else if (sExt=="ico") { return ImageFormat.Icon;}
			else if (sExt=="wmf") { return ImageFormat.Wmf; }
			else if (sExt=="bmp") { return ImageFormat.Bmp; }
			else {
				sExt="PNG";
				return ImageFormat.Png;
			}
		}//end ImageFormatFromNameElseCapitalizedPng
		public static string KnownExtensionFromNameElseBlank(string sName) {
			string sReturn="";
			try {
			if (sName!="" && sName.Length>2) {
				string sLower=sName.ToLower();
				if (sName.ToLower().EndsWith(".png")) { sReturn="png"; }
				else if (sLower.EndsWith(".jpg")) { sReturn="jpg"; }
				else if (sLower.EndsWith(".jpe")) { sReturn="jpe"; }
				else if (sLower.EndsWith(".jpeg")){ sReturn="jpeg";}
				else if (sLower.EndsWith(".gif")) { sReturn="gif"; }
				else if (sLower.EndsWith(".exi")) { sReturn="exi"; }
				else if (sLower.EndsWith(".exif")){ sReturn="exif";}
				else if (sLower.EndsWith(".emf")) { sReturn="emf"; }
				else if (sLower.EndsWith(".tif")) { sReturn="tif"; }
				else if (sLower.EndsWith(".tiff")){ sReturn="tiff";}
				else if (sLower.EndsWith(".ico")) { sReturn="ico"; }
				else if (sLower.EndsWith(".wmf")) { sReturn="wmf"; }
				else if (sLower.EndsWith(".bmp")) { sReturn="bmp"; }
				else if (sLower.EndsWith(".txt")) { sReturn="txt"; }
				else if (sLower.EndsWith(".raw")) { sReturn="raw"; }
				else if (sLower.EndsWith(".ogg")) { sReturn="ogg"; }
				else if (sLower.EndsWith(".mp3")) { sReturn="mp3"; }
				else if (sLower.EndsWith(".wav")) { sReturn="wav"; }
				else if (sLower.EndsWith(".ini")) { sReturn="ini"; }
				else if (sLower.EndsWith(".html")) { sReturn="html"; }
				else if (sLower.EndsWith(".htm")) { sReturn="htm"; }
				else if (sLower.EndsWith(".php")) { sReturn="php"; }
				else if (sLower.EndsWith(".js")) { sReturn="js"; }
				else sReturn="";
			}
			}
			catch (Exception exn) {
				sReturn="";
				Base.ShowExn(exn,"KnownExtensionFromNameElseBlank","detecting file extension");
			}
			return sReturn;
		}//end KnownExtensionFromNameElseBlank
		///// <summary>
		///// Crops iToChange to iMin and iExclusiveMax, and returns how many linear whole number increments were lost.
		///// <summary>
		///// <returns>Loss, if cropped iToChange, else zero.</returns>
		//public static int CropAndGetLossEx(ref int iToChange, int iMin, int iExclusiveMax) {
		//	int iReturn=0;
		//	if (iToChange<iMin) {
		//		iReturn=iMin-iToChange;
		//		iToChange=iMin;
		//	}
		//	else if (iToChange>=iExclusiveMax) {
		//		iReturn=iToChange-(iExclusiveMax-1);
		//		iToChange=iExclusiveMax-1;
		//	}
		//	return iReturn;
		//}
		public static void CropEx(ref int iToChange, int iMin, int iExclusiveMax) {
			if (iToChange<iMin) iToChange=iMin;
			else if (iToChange>=iExclusiveMax) iToChange=iExclusiveMax-1;
		}
		public static void CropZone(ref int zone_Left, ref int zone_Top, ref int zone_Right, ref int zone_Bottom, int xScreen, int yScreen, int iScreenWidth, int iScreenHeight) {
			int iTemp;
			int screen_Right=xScreen+iScreenWidth;
			int screen_Bottom=yScreen+iScreenHeight;
			if (zone_Left<xScreen) {
				iTemp=xScreen-zone_Left;
				zone_Left+=iTemp;
				zone_Right-=iTemp;
			}
			if (zone_Top<yScreen) {
				iTemp=yScreen-zone_Top;
				zone_Top+=iTemp;
				zone_Bottom-=iTemp;
			}
			if (zone_Bottom>screen_Bottom) {
				zone_Bottom-=(zone_Bottom-screen_Bottom);
			}
			if (zone_Right>screen_Right) {
				zone_Right-=(zone_Right-screen_Right);
			}
			if (zone_Right<=zone_Left) zone_Right=zone_Left+1;
			if (zone_Bottom<=zone_Top) zone_Bottom=zone_Top+1;
		}
		public static void CropRect(ref int rect_X, ref int rect_Y, ref int rect_Width, ref int rect_Height, int xScreen, int yScreen, int iScreenWidth, int iScreenHeight) {
			int zone_Right=rect_X+rect_Width;
			int zone_Bottom=rect_Y+rect_Height;
			CropZone(ref rect_X, ref rect_Y, ref zone_Right, ref zone_Bottom, xScreen, yScreen, iScreenWidth, iScreenHeight);
			rect_Width=(zone_Right-rect_X)-1; //-1 since exclusive zone
			rect_Height=(zone_Bottom-rect_Y)-1; //-1 since exclusive zone
			if (rect_Width<0) rect_Width=0;
			if (rect_Height<0) rect_Height=0;
		}
		/*public static void CropRect(ref int rect_X, ref int rect_Y, ref int rect_Width, ref int rect_Height, int xScreen, int yScreen, int iScreenWidth, int iScreenHeight) {
			if (rect_Width>0&&rect_Height>0) {
				rect_Width-=CropAndGetLossEx(ref rect_X, xScreen, xScreen+iScreenWidth);
				rect_Height-=CropAndGetLossEx(ref rect_Y, yScreen, yScreen+iScreenHeight);
				if (rect_Width>0) {
					if (rect_X+rect_Width>iScreenWidth) {
						rect_Width-=((rect_X+rect_Width)-iScreenWidth);
					}
				}
				if (rect_Height>0) {
					if (rect_Y+rect_Height>iScreenHeight) {
						rect_Height-=((rect_Y+rect_Height)-iScreenHeight);
					}
				}
			}
			else {
				rect_Width=0;
				rect_Height=0;
				CropEx(ref rect_X, xScreen, xScreen+iScreenWidth);
				CropEx(ref rect_Y, yScreen, yScreen+iScreenHeight);
			}
		}
		*/
		#endregion utilities
		
		#region file methods
		public static bool SafeDelete(string sFile) {
			bool bGood=true;
			try {
				if (File.Exists(sFile)) File.Delete(sFile);
			}
			catch {
				bGood=false;
			}
			return bGood;
		}
		#endregion file methods
		
		#region platform
		public static bool PlatformIsWindows() {
			return Environment.OSVersion.ToString().IndexOf("Win")>=0;
		}
		public static string sDirSep {
			get { return char.ToString(Path.DirectorySeparatorChar); }
		}
		public static bool Terminate(string sProcessName) {
			if (PlatformIsWindows()) {
				Base.ShowErr("Terminating processes in Windows is not yet implemented");
			}
			else {
				return Run("killall "+sProcessName);
			}
			return false;
		}
		public static bool Run(string sLine) {
			bool bGood=false;
			string sCommand="";
			string sArgs="";
			int iSpace=-2;
			try {
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.EnableRaisingEvents=false;
				iSpace=sLine.IndexOf(" ");
				if (iSpace>-1) {
					sCommand=sLine.Substring(0,iSpace);
					sArgs=sLine.Substring(iSpace+1);
					proc.StartInfo.FileName=sCommand;
					proc.StartInfo.Arguments=sArgs;
				}
				else {
					proc.StartInfo.FileName=sLine;
				}
				proc.Start();
				//proc.WaitForExit();
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Run","running system command {sLine:('"+sLine+"'); sCommand:"+sCommand+"; sArgs:"+sArgs+"}");
			}
			return bGood;
		}//end Run
		#endregion platform
		
		#region graphics
	
		#endregion graphics
		
		#region UI methods
		public static void AutoSize(out int iWidthInner, out int iHeightInner, int iWidthOuter, int iHeightOuter, float fPercent) {
			iWidthInner=(int)((float)iWidthOuter*fPercent+.5f);//.5f for rounding
			iHeightInner=(int)((float)iHeightOuter*fPercent+.5f);//.5f for rounding
		}
		public static int AutoInnerPosition(int iSizeOuter, float fPercent) {
			return (int)((float)iSizeOuter*fPercent+.5f);//.5f for rounding
		}
		#endregion UI methods
		
		
		#region unused methods
		//public static string SeparateSyllablesLower(string sWord, string sSeparator, uint[] dwDictSymbols) {
		//	int iStartNow=0;
		//	int iStartNext;
		//	string sReturn=sWord;
		//	bool bFound=true;
		//	ArrayList alSep=new ArrayList();//keep this to prevent new additions from interfering with rules
		//	
		//	while (bFound) {
		//		bFound=false;
		//		int iVowelThenDoubleConsonantThenVowel=SafeIndexOfVowelThenDoubleConsonantThenVowelLower(sWord, iStartNow);
		//		if (iVowelThenDoubleConsonantThenVowel>-1) {
		//			alSep.Add(iVowelThenDoubleConsonantThenVowel+2); //+2 to get placement of separator
		//			bFound=true;
		//		}
		//	}
		//	//finish: other rules--checking long vowels etc by using dwDictSymbols
		//
		//	int iOffset=0;
		//	foreach (int iSep in alSep) {
		//		sReturn.Insert(iSep+iOffset,sSeparator);
		//		iOffset+=sSeparator.Length;
		//	}
		//	return sReturn;
		//}
		//public static bool SeparateSyllablesLower(ref string[] sarrWords, string sSeparator) {
		//	bool bGood=true; try {
		//		if (sarrWords!=null) {
		//			for (int iNow=0; iNow<sarrWords.Length; iNow++) {
		//				sarrWords[iNow]=SeparateSyllablesLower(sarrWords[iNow]);
		//			}
		//		}
		//		else {bGood=false; Base.ShowErr("null words array sent to SeparateSyllables");}
		//	}
		//	catch (Exception exn) {
		//		Base.ShowExn(exn,"SeparateSyllables"); bGood=false;
		//	}
		//	return true;
		//}
		#endregion unused methods
	}//end class Base
							/// PseudoRandF class ///
	class PseudoRandF {
		private PseudoRandF prandDoubler;
		public float fibboprev;
		public float fibbo;
		private float tempprev;
		private float xDualFibboOffset;
		private float max;//CHANGING THIS WILL CHANGE DETERMINISTIC OUTCOME
		public float DualFibboOffset {
			get {
				return xDualFibboOffset;
			}
		}
		private int iDualIterationOffset=10;
		public int DualFibboIterationOffset {
			get {
				return iDualIterationOffset;
			}
		}
		public PseudoRandF() {
			tempprev=0;
			fibbo=1;
			fibboprev=0;
			xDualFibboOffset=10;
			prandDoubler=null;
			max=float.MaxValue;
			int iTest=0;
			while (Base.FractionPartOf(max)==0) {
				max/=2;
				iTest=Base.SafeAdd(iTest,1);
			}
			if (iTest>0) max*=2;
			
			ResetFibbo();
			//WOULD CAUSE INFINITE RECURSION:
			//ResetDualFibbo(0,xDualFibboOffset); //DON'T DO NOW!
		}
		public void ResetFibbo() {
			fibboprev=0;
			fibbo=1;
		}
		public void ResetFibboToPseudoRandom(float limit) {
			fibbo=1;
			fibboprev=Fibbo(0F,limit);
		}
		public float Fibbo() { //always positive
			tempprev=fibboprev;
			fibboprev=fibbo;
			if (fibbo<float.MaxValue-tempprev) fibbo+=tempprev;
			else ResetFibboToPseudoRandom(9F);
			return fibboprev;
		}
		public float Fibbo(float min, float max) { //can be negative
			return Base.Mod(Fibbo(),max-min)+min;
		}
		public void Iterate(int iIterations) {
			for (int iNow=0; iNow<iIterations; iNow++) {
				tempprev=Fibbo();
			}
		}
		public void ResetDualFibbo(int iIterations, float offset) {
			if (offset<0) offset=0;
			iDualIterationOffset=iIterations;
			prandDoubler=new PseudoRandF();
			prandDoubler.fibboprev=offset;
			prandDoubler.Iterate(iDualIterationOffset);
			xDualFibboOffset=offset;
		}
		public void ResetDualFibboToPseudoRandom(int iIterations, float limit) {
			if (limit<0) limit=0;
			iDualIterationOffset=iIterations;
			if (prandDoubler==null) prandDoubler=new PseudoRandF();
			prandDoubler.ResetFibboToPseudoRandom(limit);
			prandDoubler.Iterate(iIterations);
			xDualFibboOffset=Base.SafeSubtract(prandDoubler.fibboprev, fibboprev);
		}
		public float DualFibbo() {
			if (prandDoubler==null) {
				prandDoubler=new PseudoRandF();
				prandDoubler.Iterate(iDualIterationOffset);
			}
			return Base.SafeAdd(prandDoubler.Fibbo(), Fibbo());
		}
	}//end class PseudoRandF
							/// PseudoRandI class ///
	class PseudoRandI {
		private PseudoRandI prandDoubler;
		public int fibboprev;
		public int fibbo;
		private int tempprev;
		private int xDualFibboOffset;
		private int max;//CHANGING THIS WILL CHANGE DETERMINISTIC OUTCOME
		public int DualFibboOffset {
			get {
				return xDualFibboOffset;
			}
		}
		private int iDualIterationOffset=10;
		public int DualFibboIterationOffset {
			get {
				return iDualIterationOffset;
			}
		}
		public PseudoRandI() {
			tempprev=0;
			fibbo=1;
			fibboprev=0;
			xDualFibboOffset=10;
			prandDoubler=null;
			max=int.MaxValue;
			//int iTest=0;
			//while (Base.FractionPartOf(max)==0) {
			//	max/=2;
			//  iTest=Base.SafeAdd(iTest,1);
			//}
			//if (iTest>0) max*=2;
			
			ResetFibbo();
			//WOULD CAUSE INFINITE RECURSION:
			//ResetDualFibbo(0,xDualFibboOffset); //DON'T DO NOW!
		}
		public void ResetFibbo() {
			fibboprev=0;
			fibbo=1;
		}
		public void ResetFibboToPseudoRandom(int limit) {
			fibbo=1;
			fibboprev=Fibbo(0,limit);
		}
		public int Fibbo() { //always positive
			tempprev=fibboprev;
			fibboprev=fibbo;
			if (fibbo<int.MaxValue-tempprev) fibbo+=tempprev;
			else ResetFibboToPseudoRandom(9);
			return fibboprev;
		}
		public int Fibbo(int min, int max) { //can be negative
			return (Fibbo()%(max-min))+min;
		}
		public void Iterate(int iIterations) {
			for (int iNow=0; iNow<iIterations; iNow++) {
				tempprev=Fibbo();
			}
		}
		public void ResetDualFibbo(int iIterations, int offset) {
			if (offset<0) offset=0;
			iDualIterationOffset=iIterations;
			prandDoubler=new PseudoRandI();
			prandDoubler.fibboprev=offset;
			prandDoubler.Iterate(iDualIterationOffset);
			xDualFibboOffset=offset;
		}
		public void ResetDualFibboToPseudoRandom(int iIterations, int limit) {
			if (limit<0) limit=0;
			iDualIterationOffset=iIterations;
			if (prandDoubler==null) prandDoubler=new PseudoRandI();
			prandDoubler.ResetFibboToPseudoRandom(limit);
			prandDoubler.Iterate(iIterations);
			xDualFibboOffset=Base.SafeSubtract(prandDoubler.fibboprev, fibboprev);
		}
		public int DualFibbo() {
			if (prandDoubler==null) {
				prandDoubler=new PseudoRandI();
				prandDoubler.Iterate(iDualIterationOffset);
			}
			return Base.SafeAdd(prandDoubler.Fibbo(), Fibbo());
		}
	}//end class PseudoRandI
}//end namespace

