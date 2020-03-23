using System;
using System.Drawing;
//using REAL = System.Single; //System.Double

namespace ExpertMultimedia {
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
			RConvert.RgbToHsv(out H, out S, out V, ref r, ref g, ref b);
			A=a;
		}
		public void FromRgb(byte r, byte g, byte b) {
			RConvert.RgbToHsv(out H, out S, out V, ref r, ref g, ref b);
			A=255;
		}
		public float HueDeg {
			get { return RConvert.ByteToDegF(H); }
			set { H=RConvert.DegToByte((float)value); }
		}
		public float HueMultiplier {
			get { return RConvert.ToFloat_255As1(H); }
			set { H=RConvert.ToByte_1As255((float)value); }
		}
	}//end PixelHsvaStruct
	public struct Percent {
		//static Percent() {
		//	SetSignificantDecimalPlaces(2);
		//}//end static constructor
		//private static void SetSignificantDecimalPlaces(int SignificantDecimalPlacesNew, int iMaxPercentage100is1Whole, int iMinPercentage1is1Percent) {
		//	SignificantDecimalPlaces=SignificantDecimalPlacesNew;
		//	DecimalPlaces=SignificantDecimalPlaces+1
		//	ToWholeDivisor=Math.Pow(10,DecimalPlaces);
		//	Entire=100*ToWholeDivisor;
		//	dEntire=(double)Entire;
		//	EntireLength=Entire.ToString();
		//	MaximumPercent=iMaxPercentage100is1Whole;
		//	MinimumPercent=iMinPercentage1is1Percent;
		//	MaximumInternalValue=MaximumPercent*ToWholeDivisor;
		//	MaximumInternalValue=MinimumPercent*ToWholeDivisor;
		//	InternalValueStringBuildLength= MaximumInternalValue.Length>MinimumInternalValue.Length ? MaximumInternalValue.Length : MinimumInternalValue.Length;
		//}
		private const int DecimalPlaces=3;
		public const int SignificantDecimalPlaces=2;
		public static readonly int ToWholeDivisor=RMath.SafePow(10,DecimalPlaces);
		//i.e. Pow(10,3) = 1000 ; causing 100 percent to be stored as 100000 as in percentage class (10,000 is accurate enough for the screen [i.e. still allows subpixels at 6400pixels], but extra digit is needed for rounding during math operations)
		public static readonly int Entire=100*ToWholeDivisor;
		private static readonly double dEntire=100.0*(double)ToWholeDivisor;
		private static readonly int EntireLength=Entire.ToString().Length;
		private const int MaximumPercent=100;//limiter, independent of all other math
		private const int MinimumPercent=0;//limiter, independent of all other math
		private static readonly int MaximumInternalValue=MaximumPercent*ToWholeDivisor;
		private static readonly int MinimumInternalValue=MinimumPercent*ToWholeDivisor;
		private static readonly int InternalValueStringBuildLength= MaximumInternalValue.ToString().Length>MinimumInternalValue.ToString().Length ? MaximumInternalValue.ToString().Length : MinimumInternalValue.ToString().Length;
		//public static readonly double DecimalToInternalValueMultiplier=Entire;
		private int iInternalValue;
		//public int DecimalPlaces {
		//	get { return SignificantDecimalPlaces; }
		//}
		//public Percent() {
		//	iInternalValue=0;
		//}
		Percent(double TrueDecimalMultiplier1is1Whole) {
			iInternalValue=RMath.IRound(TrueDecimalMultiplier1is1Whole*dEntire);//FromDecimal1is1Whole(TrueDecimalMultiplier1is1Whole);
		}
		public void Set_1As100Percent(double val) {//FromDecimal1is1Whole
			iInternalValue=RMath.IRound(val*dEntire);
		}
		public void Set(double val) {//FromDecimal100is1Whole
			iInternalValue=RMath.IRound((val/100.0d)*dEntire);
		}
		public void Set(string val) {
			if (val!=null&&val.Length>0) {
				if (val!="%") {
					if (val[val.Length-1]=='%') Set(  double.Parse( val.Substring(0,val.Length-1) )  );
					else Set( double.Parse(val) );
				}
				else {
					iInternalValue=0;
					RReporting.Warning("Warning: Set Percent using the string \"%\" so setting to 0!");
				}
			}
			else {
				RReporting.Warning("Warning: Set Percent to "+(val==null?"null":(val.Length.ToString()+"-length"))+" string!");
			}
		}
		///<summary>
		///Returns this percentage of the value
		///</summary>
		public int Multiply(int val) {
		// ~.16 percent (~.0016 -- exactly .0015625) is 1 pixel in 640x480
		// .125 percent (exactly .00125 ) is 1 pixel in 800x600
			int iInternalValueTemp=val*iInternalValue; //i.e. 800 * 50000 = 40000000
			return RMath.SafeDivideRound(iInternalValueTemp,Entire,int.MaxValue); //i.e. 40000000 / 100000 = 400 //debug performance
			
			//TODO: +(iNewInternal%Entire>=/*something*/?1:0); //(+ is for rounding)
		}
		//TODO: public Percent Multiply(Percent perc2) {
			
		//}
		public bool CompareData(int InternalValueTest_SymbolicData) {
			return iInternalValue==InternalValueTest_SymbolicData;
		}
		public void From(ref Percent val) {
			val.CopyTo(ref this);
		}
		public override string ToString() {
			return ToString(0);
		}
		public bool Equals(Percent val) {
			return val.CompareData(iInternalValue);
		}
		public Percent Copy() {
			Percent percReturn=new Percent();
			percReturn.SetData(iInternalValue);
			return percReturn;
		}
		public void SetData(int InternalValueNew_SymbolicData) {
			iInternalValue=InternalValueNew_SymbolicData;
		}
		public void CopyTo(ref Percent dest) {
			dest.SetData(iInternalValue);
		}
		public string ToString(int iDecimalPlaces) { //TODO: , bool bAsTrueDecimal) {
			int iBuildLengthNow=InternalValueStringBuildLength;
			if (iDecimalPlaces>DecimalPlaces) iBuildLengthNow+=(iDecimalPlaces-DecimalPlaces);
			RString sReturn=new RString(iInternalValue.ToString(),iBuildLengthNow+1); //+1 for decimal point
			//TODO: replace StringBuilder with RString
			//stringbuilder constructors: capacity,maxcapacity; string,capacity
			bool bNeg=iInternalValue<0;
			if (bNeg) sReturn=sReturn.Remove(0,1);
			while (sReturn.Length<InternalValueStringBuildLength) sReturn=sReturn.Insert(0,'0'); //InternalValueStringBuildLength+1 not used since no decimal point yet
			sReturn.Insert(sReturn.Length-DecimalPlaces,'.');
			while (sReturn.Length<iBuildLengthNow) sReturn=sReturn.Append('0');
			if (iDecimalPlaces>0) {
				sReturn.Length=(sReturn.ToString().IndexOf(".")+1+iDecimalPlaces); //+1 to include dot
			}
			else {
				sReturn.Length=(sReturn.ToString().IndexOf("."));
			}
			while (sReturn.Length>1&&sReturn[0]=='0') sReturn=sReturn.Remove(0,1);
			if (bNeg) sReturn=sReturn.Insert(0,"0");
			return sReturn.ToString();
		}//end ToString
	}//end Percent struct
	
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
			return new Pixel32(RMath.ByRound(a),RMath.ByRound(r),RMath.ByRound(g),RMath.ByRound(b));
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
			return new Pixel32Struct(RMath.ByRound(a),RMath.ByRound(r),RMath.ByRound(g),RMath.ByRound(b));
		}
	}//end Pixel32Struct
	public class PixelYhs {
		public float Y;
		public float H;
		public float S;
		public PixelYhs() {
			Reset();
		}
		public PixelYhs(float y, float h, float s) {
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
			if (pxDest==null) pxDest=new PixelYhsa(Y,H,S,1.0f);
			else pxDest.Set(Y,H,S,1.0f);
		}
		public void Reset() {
			Y=0;
			H=0;
			S=0;
		}
		public void Set(float y, float h, float s) {
			Y=y;
			H=h;
			S=s;
		}
		public void Set(byte y, byte h, byte s) {
			Y=RConvert.ByteToFloat(y);
			H=RConvert.ByteToFloat(h);
			S=RConvert.ByteToFloat(s);
		}
		public void Get(out byte y, out byte h, out byte s) {
			y=RConvert.DecimalToByte(Y);
			h=RConvert.DecimalToByte(H);
			s=RConvert.DecimalToByte(S);
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
			RConvert.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//RConvert.RgbToYhs(out Y, out H, out S, (float)r, (float)g, (float)b);
		}
	}//end PixelYhs
	public class PixelYhsa {
		public float Y;
		public float H;
		public float S;
		public float A;
		public PixelYhsa() {
			Reset();
		}
		public PixelYhsa(float y, float h, float s, float a) {
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
		public void Set(float y, float h, float s, float a) {
			Y=y;
			H=h;
			S=s;
			A=a;
		}
		public void Set(byte y, byte h, byte s, byte a) {
			Y=RConvert.ByteToFloat(y);
			H=RConvert.ByteToFloat(h);
			S=RConvert.ByteToFloat(s);
			A=RConvert.ByteToFloat(a);
		}
		public void Get(out byte y, out byte h, out byte s, out byte a) {
			y=RConvert.DecimalToByte(Y);
			h=RConvert.DecimalToByte(H);
			s=RConvert.DecimalToByte(S);
			a=RConvert.DecimalToByte(A);
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
			A=1.0f;
		}
		public void FromArgb(byte a, byte r, byte g, byte b) {
			RConvert.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//RConvert.RgbToYhs(out Y, out H, out S, (float)r, (float)g, (float)b);
			A=RConvert.DecimalToByte(((float)a/255.0f) );
		}
		public void FromRgb(byte r, byte g, byte b) {
			RConvert.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//RConvert.RgbToYhs(out Y, out H, out S, (float)r, (float)g, (float)b);
			A=1.0f;
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
 	public struct MPoint {
 		public decimal X;
 		public decimal Y;
 	}
	public struct FPoint3D {
		public float x;
		public float y;
		public float z;
	}
	public struct DPoint3D {
		public double x;
		public double y;
		public double z;
	}
 	public struct MPoint3D {
 		public decimal x;
		public decimal y;
 		public decimal z;
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
				RReporting.ShowExn(exn,"constructing/setting point using other point","IPoint Set(IPoint)");
			}
		}
		public void Set(int xSet, int ySet) {
			X=xSet;
			Y=ySet;
		}
		public static string ToString(IPoint pX) {
			return pX!=null?pX.ToString():"null";
		}
		public static string ToString(int x, int y) {
			return "("+x.ToString()+","+y.ToString()+")";
		}
		public override string ToString() {
			return ToString(X,Y);
		}
		public IPoint Copy() {
			IPoint ipReturn=null;
			try {
				ipReturn=new IPoint();
				ipReturn.X=X;
				ipReturn.Y=Y;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","IPoint copy");
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
		public int BottomInclusive { get { return (Height>0)?(Y+(Height-1)):Y; } }
		public int RightInclusive { get { return (Width>0)?(X+(Width-1)):X; } }
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
				RReporting.ShowExn(exn,"accessing source rectangle", "IRect From(Rectangle)");
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
				RReporting.ShowExn(exn,"setting framework rectangle using IRect","IRect set");
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
			return ToString(X,Y,Width,Height);
		}
		public static string ToString(IRect rectX) {
			return rectX!=null?rectX.ToString():"null";
		}
		public static string ToString(int xNow, int yNow, int iWidthNow, int iHeightNow) {
			return "["+iWidthNow.ToString()+"x"+iHeightNow.ToString()+"@("+xNow.ToString()+","+yNow.ToString()+")]";
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
							RReporting.ShowErr("Warning: points are being truncated","truncating points","IPoly set Maximum{"
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
					if (value<0) RReporting.ShowErr("WARNING: IPoly Maximum set to negative value","checking value","IPoly set Maximum="+value.ToString()+" (IPoly Corruption)");
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
			int iNew=iLoc+iLoc/2+1;
			if (iNew>Maximum) Maximum=iNew;
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
					//iLineRelationship=RMath.IntersectionAndRelationship(ref xTemp, ref yTemp, int.MaxValue, y, parr[iNow].X, parrNow[iNow].Y, pPrev.X, pPrev.Y);
					//if (iLineRelationship==Base.LineRelationshipIntersectionNotTouchingEndsOfLineB
					//	||iLineRelationship==Base.LineRelationshipLineBPoint1IsOnLineA) {
					//}
					if ( RMath.Intersection(out xTemp, out yTemp, x, y, int.MaxValue, y, parr[iNow].X, parr[iNow].Y, pPrev.X, pPrev.Y, false)==RMath.IntersectionYes
						&& RMath.IsInBoxEx(xTemp, yTemp, parr[iNow].X, parr[iNow].Y, pPrev.X, pPrev.Y) //DOES re-order its copies of locations
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
		///<summary>
		///Gets the outer points of this Poly and resets+modifies OR creates rectReturn boundary
		///</summary>
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
				RReporting.ShowExn(exn,"Poly GetBounds(out IRect)");
			}
			return bGood;
		}//end GetBounds(IRect)
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
				RReporting.ShowExn(exn,"setting/constructing zone using other zone","IZone Set(IZone)");
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
					else RReporting.ShowErr("Source rect height was "+rectFrom.Height.ToString()+" while setting IZone");
					if (rectFrom.Width>0) Right-=1;
					else RReporting.ShowErr("Source rect width was "+rectFrom.Width.ToString()+" while setting IZone");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"IZone From(IRect)");
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
			return ToString(Left,Top,Right,Bottom);
		}
		public static string ToString(int left, int top, int right, int bottom) {
			return "[("+left.ToString()+","+top.ToString()+"):("+right.ToString()+","+bottom.ToString()+")]";
		}
		public static string ToString(IZone zoneX) {
			return zoneX!=null?zoneX.ToString():"null";
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
}//end namespace