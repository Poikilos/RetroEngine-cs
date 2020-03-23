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
		//i.e. Pow(10,3) = 1000 ; causing 100 percent to be stored as 100000 as in percentage section (10,000 is accurate enough for the screen [i.e. still allows subpixels at 6400pixels], but extra digit is needed for rounding during math operations)
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
		public static string ToString(DPoint pX) {
			return pX.ToString();//return pX!=null?pX.ToString():"null";
		}
		public static string ToString(double x, double y) {
			return "("+x.ToString()+","+y.ToString()+")";
		}
		public override string ToString() {
			return ToString(X,Y);
		}
	}
	public struct FPoint {
		public float X;
		public float Y;
		public static string ToString(FPoint pX) {
			return pX.ToString();//return pX!=null?pX.ToString():"null";
		}
		public static string ToString(float x, float y) {
			return "("+x.ToString()+","+y.ToString()+")";
		}
		public override string ToString() {
			return ToString(X,Y);
		}
	}
 	public struct MPoint {
 		public decimal X;
 		public decimal Y;
		public static string ToString(MPoint pX) {
			return pX.ToString();//return pX!=null?pX.ToString():"null";
		}
		public static string ToString(decimal x, decimal y) {
			return "("+x.ToString()+","+y.ToString()+")";
		}
		public override string ToString() {
			return ToString(X,Y);
		}
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
	}//end FTrimmedAxis section 
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
	
	public struct FRect {
		public float X;
		public float Y;
		public float Width;
		public float Height;
		public float Right {
			get {
				return X+Width;
			}
		}
		public float Bottom {
			get {
				return Y+Height;
			}
		}
		public FRect(float iSetX, float iSetY, float iSetWidth, float iSetHeight) {
			X=iSetX;
			Y=iSetY;
			Width=iSetWidth;
			Height=iSetHeight;
		}
		public FRect Copy() {
			FRect rectReturn=new FRect();
			rectReturn.X=this.X;
			rectReturn.Y=this.Y;
			rectReturn.Width=this.Width;
			rectReturn.Height=this.Height;
			return rectReturn;
		}
		public Rectangle ToRectangle() {
			Rectangle rectReturn=new Rectangle();
			rectReturn.X=RConvert.ToInt(X);
			rectReturn.Y=RConvert.ToInt(Y);
			rectReturn.Width=RConvert.ToInt(Width);
			rectReturn.Height=RConvert.ToInt(Height);
			return rectReturn;
		}
		public System.Drawing.Point ToPoint() {
			System.Drawing.Point pointReturn=new System.Drawing.Point();
			pointReturn.X=RConvert.ToInt(X);
			pointReturn.Y=RConvert.ToInt(Y);
			return pointReturn;
		}
		public DPoint ToDPoint() {
			DPoint pointReturn;
			pointReturn.X=(double)X;
			pointReturn.Y=(double)Y;
			return pointReturn;
		}
		public FPoint ToFPoint() {
			FPoint pointReturn;
			pointReturn.X=(float)X;
			pointReturn.Y=(float)Y;
			return pointReturn;
		}
		public override string ToString() {
			return ToString(X,Y,Width,Height);
		}
		public static string ToString(FRect rectX) {
			return rectX.ToString();//return rectX!=null?rectX.ToString():"null";
		}
		public static string ToString(float xNow, float yNow, float WidthNow, float HeightNow) {
			return "["+WidthNow.ToString()+"x"+HeightNow.ToString()+"@("+xNow.ToString()+","+yNow.ToString()+")]";
		}
		public void Reset() {
			this.X=0;
			this.Y=0;
		}
		public bool Contains(float xLoc, float yLoc) {
			return xLoc>=X&&xLoc<Right&&yLoc>=Y&&yLoc<Bottom;
		}
	}//end FRect

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
			rectReturn.X=RConvert.ToInt(X);
			rectReturn.Y=RConvert.ToInt(Y);
			rectReturn.Width=RConvert.ToInt(Width);
			rectReturn.Height=RConvert.ToInt(Height);
			return rectReturn;
		}
		public System.Drawing.Point ToPoint() {
			System.Drawing.Point pointReturn=new System.Drawing.Point();
			pointReturn.X=RConvert.ToInt(X);
			pointReturn.Y=RConvert.ToInt(Y);
			return pointReturn;
		}
		public FPoint ToFPoint() {
			FPoint pointReturn;
			pointReturn.X=RConvert.ToFloat(X);
			pointReturn.Y=RConvert.ToFloat(Y);
			return pointReturn;
		}
		public DPoint ToDPoint() {
			DPoint pointReturn;
			pointReturn.X=X;
			pointReturn.Y=Y;
			return pointReturn;
		}
		public override string ToString() {
			return ToString(X,Y,Width,Height);
		}
		public static string ToString(DRect rectX) {
			return rectX.ToString();//return rectX!=null?rectX.ToString():"null";
		}
		public static string ToString(double xNow, double yNow, double WidthNow, double HeightNow) {
			return "["+WidthNow.ToString()+"x"+HeightNow.ToString()+"@("+xNow.ToString()+","+yNow.ToString()+")]";
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
			//int iIntersectType=RMath.LineRelationshipNone;
			int xTemp,yTemp;
			if (iUsed>=3) {
				IPoint pPrev=parr[iUsed-1];//using last point as prev assumes solid figure
				for (int iNow=0; iNow<iUsed; iNow++) {
					//iLineRelationship=RMath.IntersectionAndRelationship(ref xTemp, ref yTemp, int.MaxValue, y, parr[iNow].X, parrNow[iNow].Y, pPrev.X, pPrev.Y);
					//if (iLineRelationship==RMath.LineRelationshipIntersectionNotTouchingEndsOfLineB
					//	||iLineRelationship==RMath.LineRelationshipLineBPoint1IsOnLineA) {
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
	public class RTimeLength {
		//AVISynth 30000/1001 oddities: "Video formats don't match - help!" <http://forum.doom9.org/archive/index.php/t-148124.html> accessed 2010-08-25
//wayback: 1st July 2009, 03:56
//Here's the avs script:
//
//a=ImageSource("Club209.bmp", end=240, fps = 29.97). fadeio(28)
//d=directshowsource("e:\liveart\timelapse.avi")
//a+d
//
//VDubMod says video formats don't match.
//
//a is a 640x480, 24-bit, bmp image.
//d is a 640x480, 29.97, progressive Camstudio lossless codec avi file, with no audio.
//
//So it looks like the frame rate and frame size are the same. Both a and d work fine when played alone.
//
//Is this a colorspace problem? How would I fix it?
//.
//.
//.
//
//Gavino: 1st July 2009, 09:54
//Most probably, the frame rate of your video is not exactly 29.97, but 30000/1001, the standard NTSC rate. To match an image to a video, the most reliable way is like this:
//
//d=directshowsource("e:\liveart\timelapse.avi")
//a=ImageSource("Club209.bmp", end=240, fps=Framerate(d), pixel_type="RGB32").fadeio(28)
//a+d
//
//As shown, you can also specify the RGB32 directly in the ImageSource rather than converting afterwards.
//
//wayback: 2nd July 2009, 04:23
//Most probably, the frame rate of your video is not exactly 29.97, but 30000/1001, the standard NTSC rate. To match an image to a video, the most reliable way is like this:
//
//d=directshowsource("e:\liveart\timelapse.avi")
//a=ImageSource("Club209.bmp", end=240, fps=Framerate(d), pixel_type="RGB32").fadeio(28)
//a+d
//
//As shown, you can also specify the RGB32 directly in the ImageSource rather than converting afterwards.
//
//Thanks. Using framerate(d) works fine. But specifying pixel_type that way doesn't work for me. I still get the video format mismatch. Even without using the fadeio. But converting it after does work. I don't know why pixel_type doesn't work. It should.

		//NOTES:
		//--actual SMPTE uses BCD (Binary Coded Decimal), which uses a nibble for each digit of the decimal number, for each number in the timecode.  99 is max for any part of HH:MM:SS:FF
		//--30000/1001 is actual NTSC, 29.97 is only approximate.  To get current FPS of this object, Multiply darrToSecondMultiplier[TimeType_Frame]
		//--for 29.97fps, seconds per frame is 0.03336670003336670003336670003337
		//--for 30000/1001 (29.97002997002997002997002997003) fps, seconds per frame is 0.03336666666666666666666666666667
		//
		/// <summary>
		/// Value in seconds.
		/// </summary>
		public decimal Value=0.0M;
		public const int Parsing_Number=1;
		public const int Parsing_Delimiter=2;//':' or one of the carrTimeType characters
		public const int Parsing_Space=0;
		public const int TimeType_Day=0;
		public const int TimeType_Hour=1;
		public const int TimeType_Minute=2;
		public const int TimeType_Second=3;
		public const int TimeType_Frame=4;
		public static readonly char[] carrTimeType=new char[]{'d','h','m','s','f','.'}; //. is a placeholder for MS and number to left of it is assumed to be SECONDS
		public static readonly char[] carrTimeType_ToUpper=new char[]{'D','H','M','S','F','.'}; //. is a placeholder for MS and number to left of it is assumed to be SECONDS
		public static readonly string[] sarrTimeType_Alt0=new string[]{"day","hr","min","sec","frame","ms"}; //. is a placeholder for MS and number to left of it is assumed to be SECONDS
		public static readonly string[] sarrTimeType_Alt1=new string[]{"days","hrs","mins","secs","frames","msecs"}; //. is a placeholder for MS and number to left of it is assumed to be SECONDS
		public static readonly string[] sarrTimeType_Alt2=new string[]{"days","hours","minutes","seconds","frames","milliseconds"}; //. is a placeholder for MS and number to left of it is assumed to be SECONDS
		/// <summary>
		/// must correspond to carrTimeType, carrTimeType_ToUpper, and sarrTimeType
		/// </summary>
		public static readonly decimal[] darrToSecondMultiplier=new decimal[]{60.0M*60.0M*24.0M, 60.0M*60.0M, 60.0M, 1.0M, 1.0M/29.97M, 1.0M/1000.0M};
		public static readonly decimal[] darrFromSecondDivisor=new decimal[]{1/60.0M*60.0M*24.0M, 1/60.0M*60.0M, 1/60.0M, 1.0M, 29.97M, 1000.0M};
		public static int TimeDelimiterToTimeType(char val) {
			int iReturn=-1;
			if (val!=':') {
				if (val=='.') val='s'; //since value to left of decimal is ALWAYS seconds (e.g. 00:00:00.000)
				for (int i=0; i<carrTimeType.Length; i++) {
					if (val==carrTimeType[i] || val==carrTimeType_ToUpper[i]) {
						iReturn=i;
						break;
					}
				}
			}
			return iReturn;
		}
		public static int TimeDelimiterToTimeType(string val) {
			int iReturn=-1;
			if (val.Length==1) iReturn=TimeDelimiterToTimeType(val[0]);
			else {
				val=val.ToLower();
				for (int i=0; i<sarrTimeType_Alt0.Length; i++) {
					if (val==sarrTimeType_Alt0[i] || val==sarrTimeType_Alt1[i] || val==sarrTimeType_Alt2[i]) {
						iReturn=i;
						break;
					}
				}
			}
			return iReturn;
		}
		public static bool IsValidTimeType(int iTimeTypeNow) {
			return (iTimeTypeNow>=0) && (iTimeTypeNow<carrTimeType.Length);
		}
		public bool FromDHMSF(string sDHMSF) {
			Value=SecondsFromDHMSF(sDHMSF,29.97m,FrameRate_NTSC_DropFrame);//TODO: use stored rate and uint?
			return (Value!=decimal.MinValue);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sText">Time length formatted as [D]:[H]:[M]:[S.MS], [D]:[H]:[M]:[S]:[F], [H]:[M]:[S];[F], [S.MS], [F],
		/// numbers in arbitrary order but labeled as 0d0h0m0s0ms0f or where
		/// items in brackets are not in brackets but are instead numbers, and where [D] (days) and anything
		/// else to the left of [S] is optional.</param>
		/// <returns>Seconds, or if fails, returns decimal.MinValue instead.</returns>
		public static decimal SecondsFromDHMSF(string sText, decimal dFrameRate, uint FrameRate_override) {
			decimal dReturn=0.0M;
			bool bGood=false;
			RReporting.sParticiple="trimming DHMSF timecode string";
			try {
				darrToSecondMultiplier[TimeType_Frame]=1.0M/dFrameRate;
				darrFromSecondDivisor[TimeType_Frame]=dFrameRate;
				if (sText==null) sText="";
				if (sText.Length>0) {
					int iMSTotal=0;
					//string[] sarrNum=new String[sText.Length];
					//string[] sarrType=new String[sText.Length];
					//for (int iNow=0; iNow<sText.Length; iNow++) {
					//	sarrNum[iNow]="";
					//	sarrType[iNow]="";
					//}
					int iFirst=0;
					int iLast=sText.Length-1;
					while (RString.IsWhiteSpace(sText[iFirst])) {
						iFirst++;
					}
					while (RString.IsWhiteSpace(sText[iLast])) {
						iLast--;
					}
					if (iFirst>0||(iLast!=(sText.Length-1))) {
						if (iLast>=iFirst) {
							sText=RString.SafeSubstringByInclusiveEnder(sText,iFirst,iLast);
						}
						else sText="";
					}
					//int iNums=0;
					RReporting.sParticiple="checking DHMSF timecode string";
					int iDigits=0;
					int iLastDecimal=-1;
					int iLastColon=-1;
					for (int i=0; i<sText.Length; i++) {
						if (RString.IsDigit(sText[i])) iDigits++;
						else if (sText[i]=='.') iLastDecimal=i;
						else if (sText[i]==':') iLastColon=i;
					}
					bool bHasDecimalInLastSegment=iLastDecimal>iLastColon;
					int TimeType_Now=TimeType_Frame; //since SMTPE timecode is [hh]:[mm]:[ss]:[ff] (vegas is [hh]:[mm]:[ss];[ff])
					if (bHasDecimalInLastSegment) {
						TimeType_Now=TimeType_Second;//to account timecode with decimal seconds e.g. [hh]:[mm]:[ss].[_ms]
					}
					int iChar=sText.Length-1;
					bGood=true;
					int ParsingWhat=Parsing_Delimiter;
					int SegmentNow_EndBefore=iChar+1;
					if (RString.IsDigit(sText[iChar])) ParsingWhat=Parsing_Number; //ok to use iChar since this whole process only happens if sText.Length>0
					while (iChar>=-1) {
						if (ParsingWhat==Parsing_Space) {
							if (iChar==-1) {
								//do nothing
							}
							else if (!RString.IsWhiteSpace(sText[iChar])) {
								SegmentNow_EndBefore=iChar+1;//+1 since NOT whitespace
								if (RString.IsDigit(sText[iChar])||(sText[iChar]=='.')) {
									ParsingWhat=Parsing_Number;
									//sarrNum[iNums]+=sText.Substring(iChar,1);
								}
								else {
									ParsingWhat=Parsing_Delimiter;
								}
							}
						}
						else if (ParsingWhat==Parsing_Delimiter) {
							if (iChar<0) {
								RReporting.SourceErr("Warning: unknown text found at start of time string","",sText);
							}
							else if (RString.IsDigit(sText[iChar])||(sText[iChar]=='.')||(RString.IsWhiteSpace(sText[iChar]))) {
								int TimeType_Temp=-1;
								//if (sText[iChar]=='.') {
								//HANDLED BELOW (TimeType_Now decremented twice)
								//	TimeType_Temp=TimeType_Second 
								//}
								//else {
								if (SegmentNow_EndBefore-(iChar+1) == 1) TimeType_Temp=TimeDelimiterToTimeType(sText[iChar+1]); //+1 since [iChar] is NOT delimiter
								else TimeType_Temp=TimeDelimiterToTimeType(RString.SafeSubstringByExclusiveEnder(sText,iChar+1,SegmentNow_EndBefore)); //+1 since [iChar] is NOT delimiter
								//}
								if (TimeType_Temp<0) {//assume generic delimiter //if (sText[iChar]==":"||sText[iChar]==";") {
									if (carrTimeType[TimeType_Now]=='.') TimeType_Now--;//skip f if on ms
									TimeType_Now--;
								}
								else TimeType_Now=TimeType_Temp;
								
								SegmentNow_EndBefore=iChar+1;//+1 since sText[iChar] is NOT delimiter
								if (RString.IsWhiteSpace(sText[iChar])) {
									ParsingWhat=Parsing_Space;
								}
								else {//digit or '.'
									ParsingWhat=Parsing_Number;
								}
								//sarrNum[iNums]+=sText.Substring(iChar,1);
							}
						}
						else if (ParsingWhat==Parsing_Number) {
							if (  iChar<0  ||  ( !RString.IsDigit(sText[iChar]) && !(sText[iChar]=='.') )  ) {
								if (IsValidTimeType(TimeType_Now)) {
									//Each hour is 108000 non-drop frames (30 * 60 * 60) or 107892 drop frames (drop 108 frames). In real time,
									//each hour is 107892.108 frames.
									//Each "deci-minute" is 18000 non-drop frames (30 * 60 * 10) or 17982 drop frames (drop 18 frames, or 2 frames
									//for nine out of every ten minutes). In real time, 10 minutes is 17982.018 frames.
									//Each "single-minute" is 1800 non-drop frames (30 * 60) or 1798 drop frames (drop 2 frames for every minute, or
									//0 frames when multiplying by 0). In real time, a minute is 1798.202 frames.
									//Each second is 30 frames (both timebases), or 29.970 frames real-time.
									if (TimeType_Now==TimeType_Frame) {
										
									}
									else dReturn+=RConvert.ToDecimal( RString.SafeSubstringByExclusiveEnder(sText,iChar+1,SegmentNow_EndBefore) ) * darrToSecondMultiplier[TimeType_Now]; //+1 since [iChar] is NOT digit
								}
								if (!(iChar<0)) {
									SegmentNow_EndBefore=iChar+1;//+1 since [iChar] is NOT part of number
									if (RString.IsWhiteSpace(sText[iChar])) {
										ParsingWhat=Parsing_Space;
									}
									else {
										ParsingWhat=Parsing_Delimiter;
									}
								}//if not out of range
							}
							//else if (sText[iChar]=='.' && TimeType_Now==TimeType_Millisecond) TimeType_Now=TimeType_Second
						}
						else { //ParsingWhat does not have valid opcode
							bGood=false;
							RReporting.ShowErr("DHMSF Parsing Corruption");
						}
						iChar--;
					}//end while iChar>=-1
				}//end if sText.Length>0
				else {
					bGood=true; //good since it is ok to be blank
					dReturn=0.0M;
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn);
			}
			if (!bGood) dReturn=decimal.MinValue;
			return dReturn;
		}//end From(sDHMSF)
		public static bool AddTo(ref RTimeLength rtlTotal, RTimeLength rtlNow) {
			bool bGood=false;
			try {
				rtlTotal.Value+=rtlNow.Value;
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return bGood;
		}//end AddTo(ref,...)
		public override string ToString() {
			return ToString_DHMS("",0.0M,false,FrameRate_NTSC_NonDropFrame);//TODO: use stored decimal and uint?
		}
		/// <summary>
		/// Value of FrameRate_override that does not override the decimal param.
		/// This is to be used for simple framerates such as where decimal frames per second is 30.
		/// For any other value of FrameRate_override, the method ignores the decimal framerate.
		/// </summary>
		public const uint FrameRate_DoNotOverrideDecimal = 0;
		public const uint FrameRate_NTSC_DropFrame = 1;
		public const uint FrameRate_NTSC_NonDropFrame = 2;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sDelmiter_BlankForDHMS">Symbol placed between hours,minutes,seconds, and frames or milliseconds.  Special cases:
		/// if "vegas", return is in [hh]:[mm]:[ss];[ff] for drop-frame,  [hh]:[mm]:[ss]:[ff] for nondrop-frame (REQUIRES FramesPerSecond_ZeroOrLessForDecimalSeconds GREATER than 0 else frame in return will be "??")
		/// if ".000", return is in [hh]:[mm]:[ss].[000] format where [000] is always 3 characters and is in milliseconds;
		/// if blank, result is [d]d[h]h[m]m[s]s[f]f format, or [d]d[h]h[m]m[s]s[ms]ms if FramesPerSecond_ZeroOrLessForDecimalSeconds is less than or equal to 0.
		/// </param>
		/// <param name="FramesPerSecond_ZeroOrLessForDecimalSeconds">This is ignored if the FrameRate_override is not FrameRate_DoNotOverrideDecimal.
		/// If this number is zero or less, and FrameRate_override is FrameRate_DoNotOverrideDecimal,
		/// then decimal seconds will be written after seconds instead of frames.</param>
		/// <param name="bIncludeDays">If true, days will be shown if above 0.  If this is false, total hours will always be written
		/// (if more than zero) even if greater than 24.</param>
		/// <returns></returns>
		public string ToString_DHMS(string sDelimiter_BlankForDHMS, decimal FramesPerSecond_ZeroOrLessForDecimalSeconds, bool bIncludeDays, uint FrameRate_override) {
			string sReturn="";
			if (sDelimiter_BlankForDHMS==null) sDelimiter_BlankForDHMS="";
			else if (sDelimiter_BlankForDHMS.ToLower()=="vegas") sDelimiter_BlankForDHMS="vegas";
			decimal Value_Destructable=Value;
			int Return_Days=0;
			if (bIncludeDays) {
				Return_Days=RMath.IRound(Value_Destructable/darrToSecondMultiplier[0]);
				Value_Destructable-=darrToSecondMultiplier[0]*(decimal)Return_Days;
			}
			int Return_Hours=0;
			Return_Hours=RMath.IRound(Value_Destructable/darrToSecondMultiplier[1]);
			Value_Destructable-=darrToSecondMultiplier[1]*(decimal)Return_Hours;
			int Return_Minutes=0;
			Return_Minutes=RMath.IRound(Value_Destructable/darrToSecondMultiplier[2]);
			Value_Destructable-=darrToSecondMultiplier[2]*(decimal)Return_Minutes;
			int Return_Seconds=0;
			Return_Seconds=RMath.IRound(Value_Destructable/darrToSecondMultiplier[3]);
			Value_Destructable-=darrToSecondMultiplier[3]*(decimal)Return_Seconds;
			
			bool bFoundFirstPart=false;
			if (Return_Days>0) {
				bFoundFirstPart=true;
				if (sDelimiter_BlankForDHMS=="") {
					sReturn+=RString.SequenceDigits(Return_Days,2);
					sReturn+="d";
				}
				else if (sDelimiter_BlankForDHMS=="vegas") {
					Return_Hours+=24*Return_Days;
				}
				else if (sDelimiter_BlankForDHMS==".000") {
					Return_Hours+=24*Return_Days;
				}
				else {
					sReturn+=RString.SequenceDigits(Return_Days,2);
					sReturn+=sDelimiter_BlankForDHMS;
				}
			}
			if (Return_Hours>0||bFoundFirstPart) {
				bFoundFirstPart=true;
				sReturn+=RString.SequenceDigits(Return_Hours,2); //ok for "vegas" and ".000" since days were added to hours above in those cases.
				if (sDelimiter_BlankForDHMS=="") {
					sReturn+="h";
				}
				else if (sDelimiter_BlankForDHMS=="vegas") {
					sReturn+=":";
				}
				else if (sDelimiter_BlankForDHMS==".000") {
					sReturn+=":";
				}
				else {
					sReturn+=sDelimiter_BlankForDHMS;
				}
			}
			if (Return_Minutes>0||bFoundFirstPart) {
				bFoundFirstPart=true;
				sReturn+=RString.SequenceDigits(Return_Minutes,2); //ok for "vegas" and ".000" since days were added to hours above in those cases.
				if (sDelimiter_BlankForDHMS=="") {
					sReturn+="m";
				}
				else if (sDelimiter_BlankForDHMS=="vegas") {
					sReturn+=":";
				}
				else if (sDelimiter_BlankForDHMS==".000") {
					sReturn+=":";
				}
				else {
					sReturn+=sDelimiter_BlankForDHMS;
				}
			}
			if (Return_Seconds>0||bFoundFirstPart) {
				bFoundFirstPart=true;
				sReturn+=RString.SequenceDigits(Return_Seconds,2); //ok for "vegas" and ".000" since days were added to hours above in those cases.
				if (sDelimiter_BlankForDHMS=="") {
					sReturn+="s";
				}
				else if (sDelimiter_BlankForDHMS=="vegas") {
					sReturn+=";";
				}
				else if (sDelimiter_BlankForDHMS==".000") {
					sReturn+=".";
				}
				else {
					if (FramesPerSecond_ZeroOrLessForDecimalSeconds>0) sReturn+=".";//sDelimiter_BlankForDHMS;
					else sReturn+=sDelimiter_BlankForDHMS;//[ms] will follow below, and will still be identifiable since will always have 3 digits
				}
			}
			
			
			if (FramesPerSecond_ZeroOrLessForDecimalSeconds>0 && sDelimiter_BlankForDHMS!=".000") {
				decimal dSecondsPerFrame=1.0M/FramesPerSecond_ZeroOrLessForDecimalSeconds;
				//int iReturn_Frames=RMath.IRound(Value_Destructable/dSecondsPerFrame);
				sReturn+=RString.SequenceDigits( RMath.IRound( RMath.SafeDivide(Value_Destructable /*+ dSecondsPerFrame/2.0M*/,dSecondsPerFrame,(decimal)int.MaxValue)), 2 ); //+ dSecondsPerFrame/2.0M to get to "middle" of frame (?)
				sReturn+="f";
			}
			else {
				if (sDelimiter_BlankForDHMS=="vegas") {
					sReturn+="??";//since no FPS specified (FramesPerSecond_ZeroOrLessForDecimalSeconds<=0)
				}
				else {
					//int Return_Milliseconds=RMath.IRound(1000.0M*Value_Destructable);
					sReturn+=RString.SequenceDigits( RMath.IRound(1000.0M*Value_Destructable), 3 );
				}
			}
			return sReturn;
		}//end ToString_DHMS
	}//end section RTimeLength
}//end namespace