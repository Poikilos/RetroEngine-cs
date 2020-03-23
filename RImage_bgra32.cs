/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 8/2/2005
 * Time: 6:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
 //TODO: blend src>>2+dest>>2 if 127 OR 128

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace ExpertMultimedia {
	/// <summary>
	/// For simple graphics buffers used as images, variable-size frames, or graphics surfaces.
	/// </summary>
	public class GBuffer32BGRA {
		public static readonly string[] sarrDrawMode=new string[] {"DrawModeCopyAlpha", "DrawModeAlpha","DrawModeAlphaQuickEdge","DrawModeAlphaHardEdge","DrawModeGreaterAlpha","DrawModeKeepDestAlpha"};
		public const int DrawModeCopyAlpha			= 0;
		public const int DrawModeAlpha			= 1;
		public const int DrawModeAlphaQuickEdge	= 2;//QuickEdge
		public const int DrawModeAlphaHardEdge	= 3;
		public const int DrawModeKeepGreaterAlpha	= 4;
		public const int DrawModeKeepDestAlpha		= 5;
		private const int INDEX_TL=0;
		private const int INDEX_TR=1;
		private const int INDEX_BL=2;
		private const int INDEX_BR=3;
		
		public Bitmap bmpLoaded=null;//keep this in order to retain any metadata upon save
		public byte[] byarrData=null;
		public int iWidth;
		public int iHeight;
		public int Width {
			get {
				return iWidth;
			}
		}
		public int Height {
			get {
				return iHeight;
			}
		}
		public int iBytesPP;
		public int iStride;
		public int iBytesTotal;
		public int iPixelsTotal;
		public static byte[] byarrBrush=null;
		public static byte[] byarrBrush32Copied64=null;
		public string sPathFileBaseName="1.untitled";
		public string sFileExt="raw";
		public static string sPixel32StyleIsAlways { get { return "bgra"; } } //assumes 32-bit
		#region constructors
		public GBuffer32BGRA() {
			InitNull();
		}
		public GBuffer32BGRA(string sFileImage) {
			if(!Load(sFileImage,4)) {
				iBytesTotal=0;
				iPixelsTotal=0;
			}
		}
		public GBuffer32BGRA(string sFileImage,int iAsBytesPP) {
			if(!Load(sFileImage,iAsBytesPP)) {
				iBytesTotal=0;
				iPixelsTotal=0;
			}
		}
		public GBuffer32BGRA(int iWidthNow, int iHeightNow, int iBytesPPNow) {
			Init(iWidthNow, iHeightNow, iBytesPPNow, true);
		}
		public GBuffer32BGRA(int iWidthNow, int iHeightNow, int iBytesPPNow, bool bInitializeBuffer) {
			Init(iWidthNow, iHeightNow, iBytesPPNow, bInitializeBuffer);
		}
		public void Init(int iWidthNow, int iHeightNow, int iBytesPPNow, bool bInitializeBuffer) {
			iBytesPP=iBytesPPNow;
			iWidth=iWidthNow;
			iHeight=iHeightNow;
			iStride=iWidth*iBytesPP;
			iBytesTotal=iStride*iHeight;
			iPixelsTotal=iWidth*iHeight;
			if (byarrBrush==null) {
				byarrBrush=new byte[4];
				byarrBrush32Copied64=new byte[8];
			}
			if (bInitializeBuffer) {
				try {
					byarrData=new byte[iBytesTotal];
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"GBuffer32BGRA Init");
					iBytesTotal=0;//debug, this is currently used to denote fatal buffer creation errors
					iPixelsTotal=0;
				}
			}
		}
		public bool IsOk {
			get {
				bool bGood=false;
				try {
					if (byarrData!=null) byarrData[0]=byarrData[0];
					else bGood=false;
				}
				catch {
					bGood=false;
				}
				return bGood;
			}
		}

		private void InitNull() {
			bmpLoaded=null;
			byarrData=null; //RGB buffer (NOT Alpha)
			byarrBrush=null; //TODO: implement this
			byarrBrush32Copied64=null; //TODO: implement this
			iWidth=0;
			iHeight=0;
			iBytesTotal=0;
			iPixelsTotal=0;
			iStride=0;
			iBytesPP=0;
		}
		public bool CopyTo(GBuffer32BGRA gbReturn) {
			bool bGood=false;
			try {
				if (!IsSameAs(gbReturn)) gbReturn=new GBuffer32BGRA(iWidth,iHeight,iBytesPP);
				for (int iNow=0; iNow<iBytesTotal; iNow++) {
					gbReturn.byarrData[iNow]=byarrData[iNow];
				}
				//Base.CopySafe(gbReturn.byarrBrush,byarrBrush); //commented since static
				//Base.CopySafe(gbReturn.byarrBrush32Copied64,byarrBrush32Copied64); //commented since static
				gbReturn.sPathFileBaseName=sPathFileBaseName+" (Copy)";
				gbReturn.sFileExt=sFileExt;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA CopyTo");
				gbReturn=null;
			}
			return bGood;
		}//end CopyTo
		public GBuffer32BGRA Copy() {
			GBuffer32BGRA gbReturn;
			bool bTest=false;
			try {
				gbReturn=new GBuffer32BGRA(iWidth,iHeight,iBytesPP);
				CopyTo(gbReturn);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA Copy");
				gbReturn=null;
			}
			if (!bTest) gbReturn=null;
			return gbReturn;
		}//end Copy
		public GBuffer32BGRA CreateFromZoneEx(int iFromLeft, int iFromTop, int iFromRight, int iFromBottom) {
			GBuffer32BGRA gbReturn;
			bool bTest=false;
			try {
				int iWidthNow=iFromRight-iFromLeft;
				int iHeightNow=iFromBottom-iFromTop;
				gbReturn=new GBuffer32BGRA(iWidthNow,iHeightNow,iBytesPP);
				IRect rectNow=new IRect(iFromLeft,iFromTop,iWidthNow,iHeightNow);
				bTest=gbReturn.Draw(gbReturn.ToRect(),this,rectNow);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA CreateFromZoneEx","{zone:"+IZone.Description(iFromLeft,iFromTop,iFromRight,iFromBottom)+"; from:"+Description()+"}");
				gbReturn=null;
			}
			if (!bTest) gbReturn=null;
			return gbReturn;
		}
		#endregion constructors
		#region file operations
		public unsafe bool Load(string sFile, int iAsBytesPP) {
			bool bGood=true;
			try {
				if (!File.Exists(sFile)) {
					Base.ShowErr("Missing resource \""+sFile+"\"","GBuffer32BGRA Load");
					return false;
				}
				bmpLoaded=new Bitmap(sFile);
				iWidth=bmpLoaded.Width;
				iHeight=bmpLoaded.Height;
				iStride=iWidth*iAsBytesPP;
				iBytesPP=iStride/iWidth;//!!Stride is relative, determined upon locking above!! //ByteDepthFromPixelFormat();//iBytesPP=iStride/iWidth;
				iBytesTotal=iStride*iHeight;
				iPixelsTotal=iWidth*iHeight;
				if (iBytesTotal>0 && iBytesPP!=2 && iBytesPP<=4) {
					byarrData=new byte[iBytesTotal];
					int iNow=0;
					Color colorNow;
					for (int y=0; y<iHeight; y++) {
						for (int x=0; x<iWidth; x++) {
							colorNow=bmpLoaded.GetPixel(x,y);
							if (iAsBytesPP==4) {
								byarrData[iNow]=colorNow.B;
								iNow++;
								byarrData[iNow]=colorNow.G;
								iNow++;
								byarrData[iNow]=colorNow.R;
								iNow++;
								byarrData[iNow]=colorNow.A;
								iNow++;
							}
							else if (iAsBytesPP==3) {
								byarrData[iNow]=colorNow.B;
								iNow++;
								byarrData[iNow]=colorNow.G;
								iNow++;
								byarrData[iNow]=colorNow.R;
								iNow++;
							}
							else if (iAsBytesPP==1) {
								byarrData[iNow]=(byte)( ((double)(colorNow.B+colorNow.G+colorNow.R))/3.0  );
								iNow++;
							}
							else {
								Base.ShowErr("Failed to load "+(iAsBytesPP*8).ToString()+"-bit buffer.","GBuffer32BGRA Load");
								bGood=false;
								break;
							}
						}//end for x
					}//end for y
				}
				else Base.ShowErr("Can't create a "+iWidth.ToString()+"x"+iHeight.ToString()+"x"+iAsBytesPP.ToString()+"-bit buffer","GBuffer32BGRA Load");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA Load");
				bGood=false;
			}
			return bGood;
		}//end Load
		public bool Save(string sSetFile) {
			//TODO:? check for tga extension
			Base.SplitFileName(out sPathFileBaseName, out sFileExt, sSetFile);
			return Save(sPathFileBaseName+"."+sFileExt, Base.ImageFormatFromNameElseCapitalizedPng(ref sPathFileBaseName, out sFileExt));
		}
		public bool Save(string sSetFileBase, string sSetExt) {
			//TODO:? check for tga extension
			sPathFileBaseName=sSetFileBase;
			sFileExt=sSetExt;
			return Save(sSetFileBase+"."+sSetExt, Base.ImageFormatFromExt(sFileExt));
		}
		public bool Save(string sFileNow, ImageFormat imageformatNow) {
			bool bGood=true;
			try {
				bmpLoaded=new Bitmap(iWidth, iHeight, PixelFormatNow());
				if (iBytesPP==1) SetGrayPalette();
				int iNow=0;
				for (int y=0; y<iHeight; y++) {
					for (int x=0; x<iWidth; x++) {
						if (iBytesPP==1) bmpLoaded.SetPixel(x,y,Color.FromArgb(255,byarrData[iNow],byarrData[iNow],byarrData[iNow]));
						else if (iBytesPP==3) bmpLoaded.SetPixel(x,y,Color.FromArgb(255,byarrData[iNow+2],byarrData[iNow+1],byarrData[iNow]));
						else if (iBytesPP==4) bmpLoaded.SetPixel(x,y,Color.FromArgb(byarrData[iNow+3],byarrData[iNow+2],byarrData[iNow+1],byarrData[iNow]));
						else {
							Base.ShowErr("Failed to save "+(iBytesPP*8).ToString()+"-bit buffer.","GBuffer32BGRA Save");
							bGood=false;
							break;
						}
						iNow+=iBytesPP;
					}
				}
				bmpLoaded.Save(sFileNow, imageformatNow);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA Save(\""+sFileNow+"\", "+imageformatNow.ToString()+")","saving image from 32-bit buffer");
				bGood=false;
			}
			return bGood;
		}//end Save
		public bool SaveRaw(string sFileNow) {
			bool bGood=true;
			try {
				Byter byterTemp=new Byter(iBytesTotal);
				if (!byterTemp.Write(ref byarrData, iBytesTotal)) {
					bGood=false;
					Base.ShowErr("Failed to write raw data to buffer","GBuffer32BGRA SaveRaw("+sFileNow+")");
				}
				if (!byterTemp.Save(sFileNow)) {
					bGood=false;
					Base.ShowErr("Failed to save raw data to file","GBuffer32BGRA SaveRaw("+sFileNow+")");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA SaveRaw("+sFileNow+")");
				bGood=false;
			}
			return bGood;
		}//end SaveRaw
		/*
		public unsafe bool Save(string sFileNow, ImageFormat imageformatNow) {
			bool bGood=true;
			BitmapData bmpdata;
			GraphicsUnit gunit;
			RectangleF rectNowF;
			Rectangle rectNow;
			try {
				bmpLoaded=new Bitmap(iWidth, iHeight, PixelFormatNow());
				gunit = GraphicsUnit.Pixel;
				rectNowF = bmpLoaded.GetBounds(ref gunit);
				rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
									(int)rectNowF.Width, (int)rectNowF.Height);
				bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.WriteOnly, PixelFormatNow());
				//MessageBox.Show("Saving: iBytesPP="+iBytesPP.ToString()+" PixelFormatNow():"+PixelFormatNow());
				byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
				for (int iBy=0; iBy<iBytesTotal; iBy++) {
					*lpbyNow=byarrData[iBy];
					lpbyNow++;
				}
				bmpLoaded.UnlockBits(bmpdata);
				bmpLoaded.Save(sFileNow, imageformatNow);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA Save(\""+sFileNow+"\", "+imageformatNow.ToString()+")");
				bGood=false;
			}
			return bGood;
		}
		*/
		/*
		public unsafe bool Load(string sFile, int iAsBytesPP) {
			bool bGood=true;
			BitmapData bmpdata;
			GraphicsUnit gunit;
			RectangleF rectNowF;
			Rectangle rectNow;
			try {//TODO: re-implement exception handling
				bmpLoaded=new Bitmap(sFile);
				gunit = GraphicsUnit.Pixel;
				rectNowF = bmpLoaded.GetBounds(ref gunit);
				rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
									(int)rectNowF.Width, (int)rectNowF.Height);
				bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.ReadOnly,
				                             bmpLoaded.PixelFormat);//TODO: change to custom pixel format (OR use a softer get method)?????
				iStride=bmpdata.Stride;
				iWidth=rectNow.Width;
				iHeight=rectNow.Height;
				iBytesPP=iStride/iWidth;//!!Stride is relative, determined upon locking above!! //ByteDepthFromPixelFormat();//iBytesPP=iStride/iWidth;
				iBytesTotal=iStride*iHeight;
				iPixelsTotal=iWidth*iHeight;
				byarrData=new byte[iBytesTotal];
				byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
				for (int iBy=0; iBy<iBytesTotal; iBy++) {
					byarrData[iBy]=*lpbyNow;
					lpbyNow++;
				}
				if (iAsBytesPP==2 && iBytesPP==4) {
					//int iBytesPPNew=2;
					int iBytesTotalNew=iWidth*iHeight*2;
					int iSrc;
					byte r=255,g=255,b=255;
					ushort wVal;
					decimal dMaxY=Base.ChrominanceD(ref r, ref g, ref b);
					decimal dMaxShort=0xFFFF;
					Byter byterShorts=new Byter(iBytesTotalNew);
					int iDest;
					for (int iDestPix=0; iDestPix<iPixelsTotal; iDestPix++) {
						iSrc=iDestPix*iBytesPP;
						wVal=(ushort)((Base.ChrominanceD(ref byarrData[iSrc+2], ref byarrData[iSrc+1], ref byarrData[iSrc])/dMaxY)*dMaxShort);
						byterShorts.Write(ref wVal);
					}
					Init(iWidth,iHeight,iAsBytesPP,false);//sets iBytesPP etc
					byarrData=byterShorts.byarr;
				}
				bmpLoaded.UnlockBits(bmpdata);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"unsafe GBuffer32BGRA Load");
				bGood=false;
			}
			return bGood;
		}//end Load
		*/
		#endregion file operations
		public bool IsSameAs(GBuffer32BGRA gbTest) {
			bool bReturn=false;
			if (gbTest!=null) {
				if ( gbTest.iWidth==iWidth
					&& gbTest.iHeight==iHeight
					&& gbTest.Channels()==Channels() 
					&& gbTest.iBytesTotal==iBytesTotal)
					bReturn=true;
			}
			return bReturn;
		}
		#region utilities
		public IRect ToRect() {
			return new IRect(0,0,Width,Height);
		}
		public void ToRect(ref IRect rectDest) {
			if (rectDest==null) rectDest=new IRect(0,0,Width,Height);
			else rectDest.Set(0,0,Width,Height);
		}
		private int XYToLocation(int x, int y) {
			if (x>=0&&y>=0&&x<Width&&y<Height) return y*iStride+x*iBytesPP;
			else {
				Base.ShowErr("("+x.ToString()+","+y.ToString()+") is outside of "+Width.ToString()+"x"+Height.ToString()+" image.");
				return -1;
			}
		}
		public static string DrawModeToString(int iDrawMode) {
			string sReturn="uninitialized-drawmode";
			try {
				sReturn=sarrDrawMode[iDrawMode];
			}
			catch {
				sReturn="nonexistant-drawmode-\""+iDrawMode.ToString()+"\"";
			}
			return sReturn;
		}
		public byte Alpha(int x, int y) {
			return ChannelValue(x,y,iBytesPP-1);//debug silently degrades (fixes) if grayscale (since uses iBytesPP-1)
		}
		public byte ChannelValue(int x, int y, int iChannel) {
			byte byReturn=0;
			try {
				//TODO: wrapping & other out-of-range handling modes go here
				return byarrData[y*iStride+x*iBytesPP+iChannel];
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA ChannelValue");
			}
			return byReturn;
		}

		public void Dump(string sFile) {
			string sData=DumpStyle();
			sData+=Environment.NewLine;
			int iLineStart=0;
			for (int iLine=0; iLine<iHeight; iLine++, iLineStart+=iStride) {
				//sStatus="Dumping line "+iLine.ToString();
				sData+=Base.ToHex(this.byarrData, iLineStart, iStride, iBytesPP);
				sData+=Environment.NewLine;
			}
			if (sData.EndsWith(Environment.NewLine))
				sData=sData.Substring(0,sData.Length-Environment.NewLine.Length);
			Base.StringToFile(sFile, sData);
		}
		public string Description() {
			return iWidth.ToString()+"x"+iHeight.ToString()+"x"+iBytesPP.ToString();
		}
		public string DumpStyle() {
			string sReturn="";
			Base.StyleBegin(ref sReturn);
			Base.StyleAppend(ref sReturn, "iWidth",iWidth);
			Base.StyleAppend(ref sReturn, "iHeight",iHeight);
			Base.StyleAppend(ref sReturn, "iBytesPP",iBytesPP);
			Base.StyleAppend(ref sReturn, "iStride",iStride);
			Base.StyleAppend(ref sReturn, "iBytesTotal",iBytesTotal);
			Base.StyleAppend(ref sReturn, "iPixelsTotal",iPixelsTotal);
			Base.StyleEnd(ref sReturn);
			return sReturn;
		}
		//public unsafe bool FromTarga(string sFile) {
			//TODO: finish this: targa loader
		//}
		public int Channels() {
			return iBytesPP;
		}
		public PixelFormat PixelFormatNow() {
			if (this.iBytesPP==1) return PixelFormat.Format8bppIndexed;//assumes no grayscale in framework
			else if (this.iBytesPP==3) return PixelFormat.Format24bppRgb;//assumes BGR, though says Rgb
			else if (this.iBytesPP==2) return PixelFormat.Format16bppGrayScale;//assumes no 16bit color
			return PixelFormat.Format32bppArgb;
		}
		public void SetGrayPalette() {
			SetGrayPalette(ref bmpLoaded);
		}
		public static void SetGrayPalette(ref Bitmap bmpNow) {
			try {
				if (bmpNow!=null) {
					for (int index=0; index<256; index++) {
						bmpNow.Palette.Entries.SetValue(Color.FromArgb(index,index,index,index), index);
					}
				}
				else Base.ShowErr("Image not initialized.","GBuffer32BGRA SetGrayPalette");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA SetGrayPalette");
			}
		}
		public static bool SetBrushRgb(string sHexCode) {
			bool bGood=true;
			try {
				if (sHexCode.StartsWith("#")) sHexCode=sHexCode.Substring(1);
				if (sHexCode.Length<6) {
					Base.ShowErr("This hex color code in the file is not complete","GBuffer32BGRA SetBrushRgb("+sHexCode+")");
					bGood=false;
				}
				else {
					sHexCode=sHexCode.ToUpper();
					//TODO: allow alpha here
					if (!SetBrushRgba(Base.HexToByte(sHexCode.Substring(0,2)),
					               Base.HexToByte(sHexCode.Substring(2,2)),
					               Base.HexToByte(sHexCode.Substring(4,2)), 255)) {
						bGood=false;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA SetBrushRgb("+sHexCode+")","interpreting the specified hex color code");
				bGood=false;
			}
			return bGood;
		}
		public static bool SetBrushRgba(byte r, byte g, byte b, byte a) {
			return SetBrushArgb(a,r,g,b);
		}
		public static bool SetBrushRgb(byte r, byte g, byte b) {
			return SetBrushArgb(255,r,g,b);
		}
		public static unsafe bool SetBrushArgb(byte a, byte r, byte g, byte b) {
			try {
				bool bMake=false;
				if (byarrBrush==null) {byarrBrush=new byte[4]; bMake=true; }
				else if ( byarrBrush[0]!=b
					||byarrBrush[1]!=g
					||byarrBrush[2]!=r
					||byarrBrush[3]!=a
					) {
					bMake=true;
				}
				if (bMake) {
					byarrBrush[0]=b;
					byarrBrush[1]=g;
					byarrBrush[2]=r;
					byarrBrush[3]=a;
					if (byarrBrush32Copied64==null) byarrBrush32Copied64=new byte[8];
					fixed (byte* lp64=byarrBrush32Copied64, lp32=byarrBrush) {
						byte* lp64Now=lp64;
						*((UInt32*)lp64Now) = *((UInt32*)lp32);
						lp64Now+=4;
						*((UInt32*)lp64Now) = *((UInt32*)lp32);
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA SetBrushRgba(r,g,b,a)");
				return false;
			}
			return true;
		}
		public static bool SetBrushHsva(float h, float s, float v, float aTo1) {
			byte r,g,b,a;
			a=SafeConvert.ToByte(aTo1*255.0f);
			Base.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
			return SetBrushArgb(a,r,g,b);
		}
		public static bool SetBrushHsva(double h, double s, double v, double aTo1) {
			byte r,g,b,a;
			a=SafeConvert.ToByte(aTo1*255.0);
			Base.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
			return SetBrushArgb(a,r,g,b);
		}
		public bool IsTransparentVLine(int x, int y, int iPixelCount) {
			int iFound=0;
			try {
				int iBuffer=XYToLocation(x,y);
				if (iBuffer>-1&&iBuffer<iBytesTotal) {
					for (int iNow=0; iNow<iPixelCount; iNow++) {
						if (byarrData[iBuffer+3]==0) iFound++; //+3 assumes 32-bit BGRA
						iBuffer+=iStride;
					}
				}
				else {
					Base.ShowErr("No line of pixels exists at location.","IsTransparentVLine","checking for transparent line {x:"+x.ToString()+"; y:"+y.ToString()+"; iPixelCount:"+iPixelCount.ToString()+"; iFound:"+iFound.ToString()+"}");
				}
			}
			catch (Exception exn) {
				//do not modify bReturn, since exception implies nonexistent pixel
				Base.ShowExn(exn,"IsTransparentVLine","checking for transparent line {x:"+x.ToString()+"; y:"+y.ToString()+"; iPixelCount:"+iPixelCount.ToString()+"; iFound:"+iFound.ToString()+"}");
			}
			return iFound==iPixelCount;
		}
		public static int SafeLength(GBuffer32BGRA[] val) {	
			int iReturn=0;
			try {
				if (val!=null) iReturn=val.Length;
			}
			catch (Exception exn) {
				iReturn=0;
				Base.IgnoreExn(exn,"GBuffer32BGRA SafeLength(GBuffer32BGRA[])");
			}
			return iReturn;
		}
			
		public static bool Redim(ref GBuffer32BGRA[] valarr, int iSetSize, string sSender_ForErrorTracking) {
			bool bGood=false;
			if (iSetSize==0) valarr=null;
			else if (iSetSize>0) {
				if (iSetSize!=SafeLength(valarr)) {
					GBuffer32BGRA[] valarrNew=new GBuffer32BGRA[iSetSize];
					for (int iNow=0; iNow<iSetSize; iNow++) {
						if (iNow<SafeLength(valarr)) valarrNew[iNow]=valarr[iNow];
						else valarrNew[iNow]=null;
					}
					valarr=valarrNew;
					bGood=true;
				}
			}
			else Base.ShowErr("Tried to set "+sSender_ForErrorTracking+" maximum GBuffer32BGRAs to less than zero",sSender_ForErrorTracking+" set maximum GBuffer32BGRAs","setting "+sSender_ForErrorTracking+" to negative maximum {iSetSize:"+iSetSize.ToString()+"}");
			return bGood;
		}
		#endregion utilities
				
		#region editing
		public static bool RawCropSafer(ref GBuffer32BGRA gbDest, ref IPoint ipSrc, ref GBuffer32BGRA gbSrc) {
			bool bGood=true;
			int iByDest=0;
			int iBySrc;//=ipSrc.Y*gbSrc.iStride+ipSrc.X*gbSrc.iBytesPP;
			int yDest;
			int xDest;
			if (gbDest.iBytesPP!=gbSrc.iBytesPP) throw new ApplicationException("Mismatched image bitdepths, couldn't RawCrop!");
			try {
				for (yDest=0; yDest<gbDest.iHeight; yDest++) {
					for (xDest=0; xDest<gbDest.iWidth; xDest++) {
						iBySrc=(yDest+ipSrc.Y)*gbSrc.iStride+(xDest+ipSrc.X)*gbSrc.iBytesPP;
						for (int iComponent=0; iComponent<gbDest.iBytesPP; iComponent++) {
							if (iBySrc>=0&&iBySrc<gbSrc.iBytesTotal) gbDest.byarrData[iByDest]=gbSrc.byarrData[iBySrc];
							else gbDest.byarrData[iByDest]=0;
							iByDest++;
							iBySrc++;
						}
						//iByDest+=gbDest.iBytesPP;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA RawCropSafer");
				bGood=false;
			}
			return bGood;
		}
		
		/// <summary>
		/// gbDest must be true color 24- or 32-bit for the raw source
		/// to be represented correctly.
		/// </summary>
		/// <param name="byarrSrc"></param>
		/// <param name="gbDest"></param>
		/// <param name="iSrcWidth"></param>
		/// <param name="iSrcHeight"></param>
		/// <param name="iSrcBytesPP"></param>
		/// <returns></returns>
		public static bool RawOverlayNoClipToBig(ref GBuffer32BGRA gbDest, ref IPoint ipAt, ref byte[] byarrSrc, int iSrcWidth, int iSrcHeight, int iSrcBytesPP) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			int iDestAdder;
			try {
				if (iSrcBytesPP==16) {
					Base.ShowErr("16-bit source isn't implemented in this function","GBuffer32BGRA RawOverlayNoClipToBig");
				}
				iDestByte=ipAt.Y*gbDest.iStride+ipAt.X*gbDest.iBytesPP;
				GBuffer32BGRA gbSrc=new GBuffer32BGRA(iSrcWidth, iSrcHeight, iSrcBytesPP, false);
				gbSrc.byarrData=byarrSrc;
				iDestAdder=gbDest.iStride - gbSrc.iWidth*gbDest.iBytesPP;//intentionally gbDest.iBytesPP
				iSrcByte=0;
				int iSlack=(gbSrc.iBytesPP>gbDest.iBytesPP)?(gbSrc.iBytesPP-gbDest.iBytesPP):1;
						//offset of next source pixel after loop
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
						for (int iChannel=0; iChannel<gbDest.iBytesPP; iChannel++) {
							gbDest.byarrData[iDestByte]=gbSrc.byarrData[iSrcByte];
							if ((iChannel+1)<gbSrc.iBytesPP) iSrcByte++;//don't advance to next pixel
							iDestByte++;
						}
				        iSrcByte+=iSlack;
					}
					iDestByte+=iDestAdder;
				}
				if (!bGood) {
					Base.ShowErr("Error copying graphics buffer data","GBuffer32BGRA RawOverlayNoClipToBig");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA RawOverlayNoClipToBig");
				bGood=false;
			}
			return bGood;
		}//end RawOverlayNoClipToBig

		/// <summary>
		/// Gradient version of Alpha overlay
		/// </summary>
		/// <param name="gbDest"></param>
		/// <param name="gbSrc"></param>
		/// <param name="ipDest"></param>
		/// <param name="gradNow"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBig(ref GBuffer32BGRA gbDest, ref IPoint ipDest, ref GBuffer32BGRA gbSrc, ref Gradient32BGRA gradNow, int iSrcChannel) {
			int iSrcByte;
			int iDestByte;
			int iDestAdder;
			bool bGood=true;
			try {
				iDestByte=ipDest.Y*gbDest.iStride+ipDest.X*gbDest.iBytesPP;
				iSrcByte=(iSrcChannel<gbSrc.iBytesPP)?iSrcChannel:gbSrc.iBytesPP-1;
				iDestAdder=gbDest.iStride - gbDest.iBytesPP*gbSrc.iWidth;//intentionally the dest BytesPP
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
						if (!gradNow.ShadeAlpha(ref gbDest.byarrData, iDestByte, gbSrc.byarrData[iSrcByte])) {
							bGood=false;
						}
						iSrcByte+=gbSrc.iBytesPP;
						iDestByte+=gbDest.iBytesPP;
					}
					iDestByte+=iDestAdder;
				}
				if (!bGood) {
					Base.ShowErr("Error accessing gradient","GBuffer32BGRA OverlayNoClipToBig gradient to "+ipDest.ToString());
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA OverlayNoClipToBig gradient to "+ipDest.ToString());
				bGood=false;
			}
			return bGood;
		}//end OverlayNoClipToBig, using gradient
		
		
		/// <summary>
		/// CopyAlpha (no blending) overlay, using gradient
		/// </summary>
		/// <param name="gbDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="gbSrc"></param>
		/// <param name="gradNow"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBigCopyAlpha(ref GBuffer32BGRA gbDest, ref IPoint ipAt, ref GBuffer32BGRA gbSrc, ref Gradient32BGRA gradNow, int iSrcChannel) {
			int iSrcByte;
			int iDestByte;
			int iDestAdder;
			bool bGood=true;
			try {
				iDestByte=ipAt.Y*gbDest.iStride+ipAt.X*gbDest.iBytesPP;
				iSrcByte=(iSrcChannel<gbSrc.iBytesPP)?iSrcChannel:gbSrc.iBytesPP-1;
				iDestAdder=gbDest.iStride - gbSrc.iWidth*gbDest.iBytesPP;//intentionally the dest BytesPP
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
						if (!gradNow.Shade(ref gbDest.byarrData, iDestByte, gbSrc.byarrData[iSrcByte])) {
							bGood=false;
						}
						iSrcByte+=gbSrc.iBytesPP;
						iDestByte+=gbDest.iBytesPP;
					}
					iDestByte+=iDestAdder;
				}
				if (!bGood) {
					Base.ShowErr("Error copying graphics buffer data","GBuffer32BGRA OverlayNoClipToBigCopyAlpha gradient");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA OverlayNoClipToBigCopyAlpha gradient");
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlpha gradient
		
		/// <summary>
		/// CopyAlpha overlay function.  
		/// "ToBig" functions must overlay small
		/// image to big image without cropping else unexpected results occur.
		/// </summary>
		/// <param name="gbDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="gbSrc"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBigCopyAlpha(ref GBuffer32BGRA gbDest, ref IPoint ipAt, ref GBuffer32BGRA gbSrc) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			try {
				iDestByte=ipAt.Y*gbDest.iStride+ipAt.X*gbDest.iBytesPP;
				iSrcByte=0;
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					if (!Memory.CopyFast(ref gbDest.byarrData, ref gbSrc.byarrData, iDestByte, iSrcByte, gbSrc.iStride)) {
						bGood=false;
					}
					iSrcByte+=gbSrc.iStride;
					iDestByte+=gbDest.iStride;
				}
				if (!bGood) {
					Base.ShowErr("Error copying graphics buffer data","GBuffer32BGRA OverlayNoClipToBigCopyAlpha");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA OverlayNoClipToBigCopyAlpha");
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlpha
		
		public static bool OverlayNoClipToBigCopyAlphaSafe(ref GBuffer32BGRA gbDest, ref IPoint ipAt, ref GBuffer32BGRA gbSrc) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			int iSrcByteNow;
			int iDestByteNow;
			int iPastLine;//the byte location after the end of the line
			try {
				iDestByte=ipAt.Y*gbDest.iStride+ipAt.X*gbDest.iBytesPP;
				iSrcByte=0;
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					if ((iSrcByte+gbSrc.iStride)-1 >= gbSrc.iBytesTotal) {
					//Fix overflow:
						iDestByteNow=iDestByte;
						iPastLine=iSrcByte+gbSrc.iStride;
						for (iSrcByteNow=iSrcByte; iSrcByteNow<iPastLine; iSrcByteNow++) {
							if (iSrcByteNow>=gbSrc.iBytesTotal || iSrcByteNow<0) gbDest.byarrData[iDestByteNow]=0;
							else gbDest.byarrData[iDestByteNow]=gbSrc.byarrData[iSrcByteNow];
						}
					}
					else if (iSrcByte<0) {
					//Fix underflow:
						iDestByteNow=iDestByte;
						iPastLine=iSrcByte+gbSrc.iStride;
						for (iSrcByteNow=iSrcByte; iSrcByteNow<iPastLine; iSrcByteNow++) {
							if (iSrcByteNow<0) gbDest.byarrData[iDestByteNow]=0;
							else gbDest.byarrData[iDestByteNow]=gbSrc.byarrData[iSrcByteNow];
						}
					}
					else {//just copy if within bounds
						if (!Memory.CopyFast(ref gbDest.byarrData, ref gbSrc.byarrData, iDestByte, iSrcByte, gbSrc.iStride)) {
							bGood=false;
						}
					}
					iSrcByte+=gbSrc.iStride;
					iDestByte+=gbDest.iStride;
				}
				if (!bGood) {
					Base.ShowErr("Error copying graphics buffer data","GBuffer32BGRA OverlayNoClipToBigCopyAlpha");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA OverlayNoClipToBigCopyAlpha");
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlphaSafe
		public static bool MaskFromChannel(ref GBuffer32BGRA gbDest, ref GBuffer32BGRA gbSrc, int iByteInPixel) {
			int iDestByte=0;
			int iSrcByte=iByteInPixel;
			int iBytesCopy;
			int iBytesPPOffset;
			try {
				if (gbDest==null) {
					gbDest=new GBuffer32BGRA(gbSrc.iWidth, gbSrc.iHeight, 1);
				}
				iBytesCopy=gbDest.iBytesTotal;
				iBytesPPOffset=gbSrc.iBytesPP;
				for (iDestByte=0; iDestByte<iBytesCopy; iDestByte++) {
					gbDest.byarrData[iDestByte]
						= gbSrc.byarrData[iSrcByte];
					iDestByte++;
					iSrcByte+=iBytesPPOffset;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA MaskFromChannel","creating mask "
					+"{"+Environment.NewLine
					+"  "+"iByteInPixel:"+iByteInPixel.ToString()
					//+"; iDestCharPitch:"+iDestCharPitch.ToString()
					//+"; iChar1:"+iChar1.ToString()
					//+"; iCharNow:"+iCharNow.ToString()
					//+"; yNow:"+yNow.ToString()
					//+"; xNow:"+xNow.ToString()
					+"; iSrcByte:"+iSrcByte.ToString()
					+"; iDestByte:"+iDestByte.ToString() +"}");
				return false;
			}
			return true;
		}
		public static bool MaskFromValue(ref GBuffer32BGRA gbDest, ref GBuffer32BGRA gbSrc) {
			int iDestByte=0;
			int iSrcByte=0;
			int iPixels;
			int iBytesPPOffset;
			try {
				if (gbDest==null) {
					gbDest=new GBuffer32BGRA(gbSrc.iWidth, gbSrc.iHeight, 1);
				}
				iPixels=gbSrc.iWidth*gbSrc.iHeight;
				iBytesPPOffset=gbSrc.iBytesPP;
				for (iDestByte=0; iDestByte<iPixels; iDestByte++) {
					gbDest.byarrData[iDestByte]=(byte)(((float)gbSrc.byarrData[iSrcByte]
							+(float)gbSrc.byarrData[iSrcByte+1]
							+(float)gbSrc.byarrData[iSrcByte+2])/3.0f);
					iSrcByte+=iBytesPPOffset;
					//if (gbDest.byarrData[iDestByte]>0) Base.Write(gbDest.byarrData[iDestByte].ToString()+" ");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"MaskFromValue", "creating mask "
					+" (make sure source bitmap is 24-bit or 32-bit) {"+Environment.NewLine
					//+"  "+"iByteInPixel:"+iByteInPixel.ToString()
					//+"; iDestCharPitch:"+iDestCharPitch.ToString()
					//+"; iChar1:"+iChar1.ToString()
					//+"; iCharNow:"+iCharNow.ToString()
					//+"; yNow:"+yNow.ToString()
					//+"; xNow:"+xNow.ToString()
					+"; iSrcByte:"+iSrcByte.ToString()
					+"; iDestByte:"+iDestByte.ToString() +"}");
				return false;
			}
			return true;
		}//end MaskFromValue
		public const double dDiagonalUnit = 1.4142135623730950488016887242097D;//the Sqrt. of 2, dist of diagonal pixel
		public static bool InterpolatePixel(ref GBuffer32BGRA gbDest, ref GBuffer32BGRA gbSrc, int iDest, ref DPoint dpSrc) {
			bool bGood=false;
			bool bOnX;
			bool bOnY;
			double dWeightNow;
			double dWeightTotal;
			double dHeavyChannel;
			DPoint[] dparrQuad; //rounded
			int iSrcRoundX;
			int iSrcRoundY;
			double dSrcRoundX;
			double dSrcRoundY;
			//int iSampleQuadIndex;
			double dMaxX;
			double dMaxY;
			int iQuad;
			int iChan;
			int iDestNow;
			int iTotal=0;
			int[] iarrLocOfQuad;
			try {
				iarrLocOfQuad=new int[4];
				dMaxX=(double)gbSrc.iWidth-1.0d;
				dMaxY=(double)gbSrc.iHeight-1.0d;
				//iDest=gbDest.iStride*ipDest.Y+gbDest.iBytesPP*ipDest.X;
				dWeightNow=0;
				dWeightTotal=0;
				dparrQuad=new DPoint[4];
				iSrcRoundX=(int)(dpSrc.X+.5);
				iSrcRoundY=(int)(dpSrc.Y+.5);
				dSrcRoundX=(double)iSrcRoundX;
				dSrcRoundY=(double)iSrcRoundY;
				if (dSrcRoundX<dpSrc.X) {
					if (dSrcRoundY<dpSrc.Y) {
						//iSampleQuadIndex=0;
						dparrQuad[0].X=dSrcRoundX;		dparrQuad[0].Y=dSrcRoundY;
						dparrQuad[1].X=dSrcRoundX+1.0d;	dparrQuad[1].Y=dSrcRoundY;
						dparrQuad[2].X=dSrcRoundX;		dparrQuad[2].Y=dSrcRoundY+1.0d;
						dparrQuad[3].X=dSrcRoundX+1.0d;	dparrQuad[3].Y=dSrcRoundY+1.0d;
					}
					else {
						//iSampleQuadIndex=2;
						dparrQuad[0].X=dSrcRoundX;		dparrQuad[0].Y=dSrcRoundY-1.0d;
						dparrQuad[1].X=dSrcRoundX+1.0d;	dparrQuad[1].Y=dSrcRoundY-1.0d;
						dparrQuad[2].X=dSrcRoundX;		dparrQuad[2].Y=dSrcRoundY;
						dparrQuad[3].X=dSrcRoundX+1.0d;	dparrQuad[3].Y=dSrcRoundY;
					}
				}
				else {
					if (dSrcRoundY<dpSrc.Y) {
						//iSampleQuadIndex=1;
						dparrQuad[0].X=dSrcRoundX-1.0d;	dparrQuad[0].Y=dSrcRoundY;
						dparrQuad[1].X=dSrcRoundX;		dparrQuad[1].Y=dSrcRoundY;
						dparrQuad[2].X=dSrcRoundX-1.0d;	dparrQuad[2].Y=dSrcRoundY+1.0d;
						dparrQuad[3].X=dSrcRoundX;		dparrQuad[3].Y=dSrcRoundY+1.0d;
					}
					else {
						//iSampleQuadIndex=3;
						dparrQuad[0].X=dSrcRoundX-1.0d;	dparrQuad[0].Y=dSrcRoundY-1.0d;
						dparrQuad[1].X=dSrcRoundX;		dparrQuad[1].Y=dSrcRoundY-1.0d;
						dparrQuad[2].X=dSrcRoundX-1.0d;	dparrQuad[2].Y=dSrcRoundY;
						dparrQuad[3].X=dSrcRoundX;		dparrQuad[3].Y=dSrcRoundY;
					}
				}
				if (dpSrc.X<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].X<0) dparrQuad[iQuad].X=0;
					}
				}
				else if (dpSrc.X>dMaxX) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].X>dMaxX) dparrQuad[iQuad].X=dMaxX;
					}
				}
				if (dpSrc.Y<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].Y<0) dparrQuad[iQuad].Y=0;
					}
				}
				else if (dpSrc.Y>dMaxY) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].Y>dMaxY) dparrQuad[iQuad].Y=dMaxY;
					}
				}
				if (dpSrc.X==(double)iSrcRoundX) bOnX=true;
				else bOnX=false;
				if (dpSrc.Y==(double)iSrcRoundY) bOnY=true;
				else bOnY=false;
				
				if (bOnY&&bOnX) {
					Memory.CopyFastVoid(ref gbDest.byarrData, ref gbSrc.byarrData, iDest, iSrcRoundY*gbSrc.iStride+iSrcRoundX*gbSrc.iBytesPP, gbDest.iBytesPP);
				}
				else {
					iDestNow=iDest;
					for (iQuad=0; iQuad<4; iQuad++) {
						iarrLocOfQuad[iQuad]=gbSrc.iStride*(int)dparrQuad[iQuad].Y + gbSrc.iBytesPP*(int)dparrQuad[iQuad].X;
					}
					for (iChan=0; iChan<gbSrc.iBytesPP; iChan++, iTotal++) {
						dHeavyChannel=0;
						dWeightTotal=0;
						for (iQuad=0; iQuad<4; iQuad++) {
							dWeightNow=dDiagonalUnit-Base.Dist(ref dpSrc, ref dparrQuad[iQuad]);
							dWeightTotal+=dWeightNow; //debug performance, this number is always the same theoretically
							dHeavyChannel+=(double)gbSrc.byarrData[iarrLocOfQuad[iQuad]+iChan]*dWeightNow;
						}
						gbDest.byarrData[iDestNow]=(byte)(dHeavyChannel/dWeightTotal);
						iDestNow++;
					}
				}
				bGood=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA InterpolatePixel");
				bGood=false; //debug show error
			}
			return bGood;
		}//end InterpolatePixel
		/// <summary>
		/// Fakes motion blur.
		///   Using a byDecayTotal of 255 makes the blur trail fade to transparent.
		/// </summary>
		public static bool EffectMoBlurSimModWidth(ref GBuffer32BGRA gbDest, ref GBuffer32BGRA gbSrc, int xOffsetTotal, byte byDecayTotal) {
			bool bGood=true;
			int xDirection;
			int xLength;
			int iDestByteStart;
			if (xOffsetTotal<0) {
				xDirection=-1;
				xLength=xOffsetTotal*-1;
			}
			else {
				xDirection=1;
				xLength=xOffsetTotal;
			}
			try {
				try {
					gbDest.iWidth=gbSrc.iWidth+xLength;
					gbDest.iBytesPP=gbSrc.iBytesPP;
					gbDest.iStride=gbSrc.iStride;
					gbDest.iHeight=gbSrc.iHeight;
					gbDest.iBytesTotal=gbDest.iStride*gbDest.iHeight;
					gbDest.iPixelsTotal=gbDest.iWidth*gbDest.iHeight;
					if (gbDest.byarrData==null || (gbDest.byarrData.Length!=gbDest.iBytesTotal))
						gbDest.byarrData=new byte[gbDest.iBytesTotal];
				}
				catch (Exception exn) {
					try {
						gbDest=new GBuffer32BGRA(gbSrc.iWidth+xLength, gbSrc.iHeight, gbSrc.iBytesPP);
					}
					catch (Exception exn2) {
						Base.ShowExn(exn2,"EffectMoBlurSimModWidth","trying to recover from exception { "+exn.ToString()+" }");
					}
				}
				//int iHeight2=gbDest.iHeight;
				//int iWidth2=gbDest.iWidth;
				int iHeight1=gbSrc.iHeight;//TODO: eliminate this var and use gbSrc.iHeight
				//int iWidth1=gbSrc.iWidth;
				int iStride=gbSrc.iStride;
				int iStride2=gbDest.iStride;
				int iSrcByte=0;
				iDestByteStart=0;
				if (xDirection<0) {
					iDestByteStart=xLength;
				}
				int iDestByte=iDestByteStart;
				bool bTest=true;
				int yNow;
				for (yNow=0; yNow<iHeight1; yNow++) {
					bTest=Memory.CopyFast(ref gbDest.byarrData,
								 	ref gbSrc.byarrData,
								  	iDestByte, iSrcByte, iStride);
					if (!bTest) {
						Base.ShowErr("Error precopying blur data","GBuffer32BGRA EffectMoBlurSimModWidth");
						break;
					}
					iSrcByte+=iStride;
					iDestByte+=iStride2;
				}
				int iOffsetEnder=xLength;
				if (xDirection<0) {
					iOffsetEnder=-1;
				}
				//debug float precision error on super-high res?
				float fMultiplier=1.0f;
				float fPixNow=0;
				float fMaxPix=(float)(xLength-1);
				float fDecayTotal=(float)byDecayTotal;
				//bTest=true;
				for (int iOffsetNow=iDestByteStart; fPixNow<=fMaxPix; iOffsetNow+=xDirection) {
					if (!bTest) break;
					iSrcByte=0;
					iDestByte=iOffsetNow;
					for (yNow=0; yNow<iHeight1; yNow++) {
						bTest=GBuffer32BGRA.EffectLightenOnly(ref gbDest.byarrData,
							ref gbSrc.byarrData,iDestByte, iSrcByte, iStride, fMultiplier);
						if (!bTest) {
							Base.ShowErr("Error overlaying blur data.","GBuffer32BGRA EffectMoBlurSimModWidth");
							break;
						}
						iSrcByte+=iStride;
						iDestByte+=iStride2;
					}
					fPixNow++;
					fMultiplier=(fDecayTotal/255.0f)*(fPixNow/fMaxPix);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA EffectMoBlurSimModWidth","compositing blur data");
				bGood=false;
			}
			return bGood;
		}//EffectMoBlurSimModWidth
		public static bool EffectSkewModWidth(ref GBuffer32BGRA gbDest, ref GBuffer32BGRA gbSrc, int xOffsetBottom) {
			bool bGood=true;
			int iDestLine;
			double xDirection;
			double dHeight;
			double yNow;
			double dWidthDest;
			double dWidthSrc;
			double xNow;
			double xAdd;
			double dMaxY;
			int iDestIndex;
			DPoint dpSrc;
			int iDestByte;
			int iSrcByte;
			if (xOffsetBottom<0) {
				xDirection=-1;
				xAdd=(double)(xOffsetBottom*-1);
			}
			else {
				xDirection=1;
				xAdd=(double)xOffsetBottom;
			}
			try {
				try {
					gbDest.iWidth=gbSrc.iWidth+((xOffsetBottom<0)?xOffsetBottom*-1:xOffsetBottom);
					gbDest.iBytesPP=gbSrc.iBytesPP;
					gbDest.iStride=gbSrc.iStride;
					gbDest.iHeight=gbSrc.iHeight;
					gbDest.iBytesTotal=gbDest.iStride*gbDest.iHeight;
					gbDest.iPixelsTotal=gbDest.iWidth*gbDest.iHeight;
					if (gbDest.byarrData==null || (gbDest.byarrData.Length!=gbDest.iBytesTotal))
						gbDest.byarrData=new byte[gbDest.iBytesTotal];
				}
				catch (Exception exn) {
					try {
						gbDest=new GBuffer32BGRA(gbSrc.iWidth+xOffsetBottom, gbSrc.iHeight, gbSrc.iBytesPP);
					}
					catch (Exception exn2) {
						Base.ShowExn(exn2,"GBuffer32BGRA EffectSkewModWidth","trying to recover from exception { "+exn.ToString()+" }");
					}
				}
				iSrcByte=0;
				iDestByte=0;//iDestByteStart;//TODO: Uncomment, and separate the blur code here and make alpha overlay version
				bool bTest=true;
				iDestLine=0;
				dpSrc=new DPoint();
				dpSrc.Y=0;
				dHeight=(double)gbDest.iHeight;
				dWidthDest=(double)gbDest.iWidth;
				//dWidthSrc=(double)gbSrc.iWidth;
				dMaxY=dHeight-1.0d;
				iDestIndex=0;
				for (yNow=0; yNow<dHeight; yNow+=1.0d) {
					dpSrc.X=(yNow/dMaxY)*xAdd;
					if (xOffsetBottom<0) dpSrc.X=(xAdd-dpSrc.X);
					for (xNow=0; xNow<dWidthDest; xNow+=1.0d) {
						if (dpSrc.X>-1.0d) {
							if (dpSrc.X<dWidthDest)
								bTest=GBuffer32BGRA.InterpolatePixel(ref gbDest, ref gbSrc, iDestIndex, ref dpSrc);
						}
						if (!bTest) {
							bGood=false;
							break;
						}
						iDestIndex+=gbSrc.iBytesPP;
					}
					if (!bGood) break;
					//iDestLine+=gbDest.iStride;
					dpSrc.Y+=1.0d;
				}
				if (!bGood) {
					Base.ShowErr("Error calculating skew data.","GBuffer32BGRA EffectSkewModWidth","interpolating pixel");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA EffectSkewModWidth","calculating skew data");
				bGood=false;
			}
			return bGood;
		}//end EffectSkewModWidth
		public static bool EffectLightenOnly(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			try {
				for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
					if (byarrSrc[iSrcByte]>byarrDest[iDestByte]) byarrDest[iDestByte]=byarrSrc[iSrcByte];
					iDestByte++;
					iSrcByte++;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA EffectLightenOnly");
				return false;
			}
			return true;
		}
		public static bool EffectLightenOnly(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes, float fMultiplySrc) {
			if (fMultiplySrc>1.0f) fMultiplySrc=1.0f;
			byte bySrc;
			float fVal;
			try {
				for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
					fVal=((float)byarrSrc[iSrcByte]*fMultiplySrc);
					if (fVal>255.0) fVal=255;
					bySrc=(byte)fVal;
					if (bySrc>byarrDest[iDestByte]) byarrDest[iDestByte]=bySrc;
					iDestByte++;
					iSrcByte++;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA EffectLightenOnly");
				return false;
			}
			return true;
		}
		#endregion editing
		
		#region Draw Overlay methods
		static bool bFirstRunNeedsToCropToFitInto=true;
		public bool NeedsToCropToFitInto(GBuffer32BGRA gbDest,int xDest, int yDest) {
			bool bReturn=true;
			if (bFirstRunNeedsToCropToFitInto) Base.Write("check NeedsToCropToFitInto...");
			if (iBytesTotal==0) {
				Base.ShowErr("Getting status on zero-length buffer!","NeedsToCropToFitInto");
			}
			try {
				if (xDest>=0 && yDest>=0
					&& xDest+iWidth<=gbDest.iWidth
					&& yDest+iHeight<=gbDest.iHeight) bReturn=false;
			}
			catch {bReturn=true;}
			if (bFirstRunNeedsToCropToFitInto) Base.Write(((bReturn)?"yes...":"no..."));
			bFirstRunNeedsToCropToFitInto=false;
			return bReturn;
		}
		static bool bFirstRunDrawSmallerWithoutCropElseCancel=true;
		static bool bFirstCancellationOfDrawSmallerWithoutCropElseCancel=true;
		
		//public bool DrawToLargerWithoutCropElseCancel(GBuffer32BGRA gbDest, int xDest, int yDest, int iDrawMode) {
		//	try {
		//		return gbDest.DrawSmallerWithoutCropElseCancel(xDest, yDest, this, iDrawMode);
		//	}
		//	catch {
		//	}
		//	return null;
		//}
		public bool DrawSmallerWithoutCropElseCancel(int xDest, int yDest, GBuffer32BGRA gbSrc) {
			return DrawSmallerWithoutCropElseCancel(xDest,yDest,gbSrc,DrawModeCopyAlpha);
		}
		public bool CanFit(GBuffer32BGRA gbSrc, int xDest, int yDest) {
			bool bReturn=false;
			try {
				if (gbSrc!=null) {
					bReturn=xDest>=0&&yDest>=0
						&&xDest+gbSrc.Width<=Width
						&&yDest+gbSrc.Height<=Height;
				}
			}
			catch {
				bReturn=false;
			}
			return bReturn;
		}
		public bool DrawSmallerWithoutCropElseCancel(int xDest, int yDest, GBuffer32BGRA gbSrc, int iDrawMode) {
			return DrawSmallerWithoutCropElseCancel(this, xDest, yDest, gbSrc, iDrawMode);
		}
		public static bool DrawSmallerWithoutCropElseCancel(GBuffer32BGRA gbDest, int xDest, int yDest, GBuffer32BGRA gbSrc, int iDrawMode) {
			bool bGood=true;
			int x=0,y=0;
			if (gbDest==null||gbDest.byarrData==null) {
				Base.ShowErr("Tried to draw to null buffer!","DrawSmallerWithoutCropElseCancel");
				return false;
			}
			else if (gbSrc==null||gbSrc.byarrData==null) {
				Base.ShowErr("Tried to draw null buffer!","DrawSmallerWithoutCropElseCancel");
				return false;
			}
			try {
				byte[] gbDest_byarrData=gbDest.byarrData;
				byte[] gbSrc_byarrData=gbSrc.byarrData;
				//int gbSrc_iBytesPP=gbSrc.iBytesPP;
				//int gbDest_iBytesPP=gbDest.iBytesPP;
				if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("DrawSmallerWithoutCropElseCancel...");
				if (!gbDest.CanFit(gbSrc,xDest,yDest)) {//if (gbSrc.NeedsToCropToFitInto(this,xDest,yDest)) {
					if (bFirstCancellationOfDrawSmallerWithoutCropElseCancel
						|| bFirstRunDrawSmallerWithoutCropElseCancel) {
						Base.Write("failed since not in bounds("+gbSrc.iWidth.ToString()+"x"+gbSrc.iHeight.ToString()+" to "+gbDest.iWidth.ToString()+"x"+gbDest.iHeight.ToString()+" at ("+xDest.ToString()+","+yDest.ToString()+") )...");
						bFirstCancellationOfDrawSmallerWithoutCropElseCancel=false;
					}
					Base.Warning("Cancelling DrawSmallerWithoutCropElseCancel","{xDest:"+xDest.ToString()+"; yDest:"+yDest.ToString()+"; gbSrc:"+gbSrc.Description()+"; gbDest:"+gbDest.Description()+"}");
					bGood=false;
				}
				if (bGood) {
					if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("offset...");
					int iLocDestLine=yDest*gbDest.iStride+xDest*gbDest.iBytesPP;
					int iDestPix;
					int iLocSrcLine=0;
					int iSrcPix;
					//byte* lpDestLine=&gbDest_byarrData[yDest*gbDest.iStride+xDest*gbDest.iBytesPP];
					//byte* lpDestPix;
					//byte* lpSrcLine=gbSrc_byarrData;
					//byte* lpSrcPix;
					int iStrideMin=(gbSrc.iStride<gbDest.iStride)?gbSrc.iStride:gbDest.iStride;
					if (gbDest.iBytesPP==4 && gbSrc.iBytesPP==4) {
						switch (iDrawMode) {
						case DrawModeCopyAlpha:
								if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("DrawModeCopyAlpha...");
							//if (gbDest.iStride==gbSrc.iStride && gbDest.iBytesPP==iBytesPP && xDest==0 && yDest==0) {
							if (gbSrc.IsSameAs(gbDest) && xDest==0 && yDest==0) {
								Memory.CopyFast(ref gbDest_byarrData, ref gbSrc_byarrData, 0, 0, gbSrc.iBytesTotal);
							}
							else {
								for (y=0; y<gbSrc.iHeight; y++) {
									Memory.CopyFast(ref gbDest_byarrData, ref gbSrc_byarrData, iLocDestLine, iLocSrcLine, iStrideMin);
									iLocDestLine+=gbDest.iStride;//lpDestLine+=gbDest.iStride;
									iLocSrcLine+=gbSrc.iStride;//lpSrcLine+=gbSrc.iStride;
								}
							}
							break;
						case DrawModeAlpha:
								if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("DrawModeAlpha(...)");
								//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("("+gbSrc.iWidth.ToString()+"x"+gbSrc.iHeight.ToString()+"x"+iBytesPP.ToString()+" to "+gbDest.iWidth.ToString()+"x"+gbDest.iHeight.ToString()+")...");
							//alpha result: ((Source-Dest)*alpha/255+Dest)
							float fCookedAlpha;
							//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.WriteLine();
							for (y=0; y<gbSrc.iHeight; y++) {
								//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write(y.ToString());
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<gbSrc.iWidth; x++) {
									//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write(".");
									if (gbSrc_byarrData[iSrcPix+3]==0) {
										iSrcPix+=4; iDestPix+=4;//iSrcPix+=gbSrc.iBytesPP; iDestPix+=gbDest.iBytesPP;//assumes 32-bit
									}
									else if (gbSrc_byarrData[iSrcPix+3]==255) {
										Memory.CopyFast(ref gbDest_byarrData,ref gbSrc_byarrData,iDestPix, iSrcPix, 3);
										iSrcPix+=4; iDestPix+=4;//iSrcPix+=gbSrc.iBytesPP; iDestPix+=gbDest.iBytesPP;//assumes 32-bit
									}
									else {
										fCookedAlpha=(float)gbSrc_byarrData[iSrcPix+3]/255.0f;
										gbDest_byarrData[iDestPix]=Base.ByRound(((float)(gbSrc_byarrData[iSrcPix]-gbDest_byarrData[iDestPix]))*fCookedAlpha+gbDest_byarrData[iDestPix]); //B
										iSrcPix++; iDestPix++;
										gbDest_byarrData[iDestPix]=Base.ByRound(((float)(gbSrc_byarrData[iSrcPix]-gbDest_byarrData[iDestPix]))*fCookedAlpha+gbDest_byarrData[iDestPix]); //G
										iSrcPix++; iDestPix++;
										gbDest_byarrData[iDestPix]=Base.ByRound(((float)(gbSrc_byarrData[iSrcPix]-gbDest_byarrData[iDestPix]))*fCookedAlpha+gbDest_byarrData[iDestPix]); //R
										iSrcPix+=2; iDestPix+=2;//assumes 32-bit
									}
								}
								iLocDestLine+=gbDest.iStride;
								iLocSrcLine+=gbSrc.iStride;
										//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.WriteLine();
							}
							break;
						case DrawModeAlphaQuickEdge:
								if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("DrawModeAlphaQuickEdge(...)");
								//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("("+gbSrc.iWidth.ToString()+"x"+gbSrc.iHeight.ToString()+"x"+iBytesPP.ToString()+" to "+gbDest.iWidth.ToString()+"x"+gbDest.iHeight.ToString()+")...");
							//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.WriteLine();
							for (y=0; y<gbSrc.iHeight; y++) {
								//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write(y.ToString());
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<gbSrc.iWidth; x++) {
									//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write(".");
									if (gbSrc_byarrData[iSrcPix+3]<=85) {
										iSrcPix+=4; iDestPix+=4;//assumes 32-bit
									}
									else if (gbSrc_byarrData[iSrcPix+3]>170) {
										Memory.CopyFast(ref gbDest_byarrData,ref gbSrc_byarrData, iDestPix, iSrcPix, 3);
										iSrcPix+=4; iDestPix+=4;//assumes 32-bit
									}
									else {
										gbDest_byarrData[iDestPix]=(byte)( (gbSrc_byarrData[iSrcPix]>>2) + (gbDest_byarrData[iDestPix]>>2) ); //B
										iSrcPix++; iDestPix++;
										gbDest_byarrData[iDestPix]=(byte)( (gbSrc_byarrData[iSrcPix]>>2) + (gbDest_byarrData[iDestPix]>>2) ); //G
										iSrcPix++; iDestPix++;
										gbDest_byarrData[iDestPix]=(byte)( (gbSrc_byarrData[iSrcPix]>>2) + (gbDest_byarrData[iDestPix]>>2) ); //R
										iSrcPix+=2; iDestPix+=2;//assumes 32-bit
									}
								}
								iLocDestLine+=gbDest.iStride;
								iLocSrcLine+=gbSrc.iStride;
										//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.WriteLine();
							}
							break;
						case DrawModeAlphaHardEdge:
								if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("DrawModeAlphaHardEdge(...)");
								//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("("+gbSrc.iWidth.ToString()+"x"+gbSrc.iHeight.ToString()+"x"+iBytesPP.ToString()+" to "+gbDest.iWidth.ToString()+"x"+gbDest.iHeight.ToString()+")...");
							//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.WriteLine();
							for (y=0; y<gbSrc.iHeight; y++) {
								//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write(y.ToString());
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<gbSrc.iWidth; x++) {
									//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write(".");
									if (gbSrc_byarrData[iSrcPix+3]<128) {
										iSrcPix+=4; iDestPix+=4;//assumes 32-bit
									}
									else {
										Memory.CopyFast(ref gbDest_byarrData,ref gbSrc_byarrData, iDestPix, iSrcPix, 3);
										iSrcPix+=4; iDestPix+=4;//assumes 32-bit
									}
								}
								iLocDestLine+=gbDest.iStride;
								iLocSrcLine+=gbSrc.iStride;
										//if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.WriteLine();
							}
							break;
						case DrawModeKeepGreaterAlpha:
								if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("DrawModeKeepGreaterAlpha...");
							//alpha result: ((Source-Dest)*alpha/255+Dest)
							for (y=0; y<gbSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<gbSrc.iWidth; x++) {
									gbDest_byarrData[iDestPix]=(gbSrc_byarrData[iSrcPix]>gbDest_byarrData[iDestPix])?gbSrc_byarrData[iSrcPix]:gbDest_byarrData[iDestPix]; //B
									iSrcPix++; iDestPix++;
									gbDest_byarrData[iDestPix]=(gbSrc_byarrData[iSrcPix]>gbDest_byarrData[iDestPix])?gbSrc_byarrData[iSrcPix]:gbDest_byarrData[iDestPix]; //G
									iSrcPix++; iDestPix++;
									gbDest_byarrData[iDestPix]=(gbSrc_byarrData[iSrcPix]>gbDest_byarrData[iDestPix])?gbSrc_byarrData[iSrcPix]:gbDest_byarrData[iDestPix]; //R
									iSrcPix+=2; iDestPix+=2; //assumes 32-bit
								}
								iLocDestLine+=gbDest.iStride;
								iLocSrcLine+=gbSrc.iStride;
							}
							break;
						case DrawModeKeepDestAlpha:
								if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.Write("DrawModeKeepDestAlpha...");
							for (y=0; y<gbSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<gbSrc.iWidth; x++) {
									Memory.CopyFast(ref gbDest_byarrData, ref gbSrc_byarrData,iDestPix,iSrcPix,3);
									iDestPix+=4; iSrcPix+=4; //assumes 32-bit
								}
								iLocDestLine+=gbDest.iStride;
								iLocSrcLine+=gbSrc.iStride;
							}
							break;
						}//end switch
					}
					else {
						Base.ShowError("Can't Draw unless both GBuffers are 32-bit BGRA.  The GBuffer32BGRA class is designed for speed only.","GBuffer32BGRA DrawSmallerWithoutCropElseCancel {"+"gbDest.iBytesPP:"+gbDest.iBytesPP.ToString()+ "; iBytesPP:"+gbSrc.iBytesPP.ToString()+"}");
					}
				}//end if does not need to crop
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"GBuffer32BGRA DrawSmallerWithoutCropElseCancel");
			}
			if (bFirstRunDrawSmallerWithoutCropElseCancel) Base.WriteLine(bGood?"DrawSmallerWithoutCropElseCancel Success...":"DrawSmallerWithoutCropElseCancel failed...");
			bFirstRunDrawSmallerWithoutCropElseCancel=false;
			return bGood;
		}//end DrawSmallerWithoutCropElseCancel
		#endregion Draw Overlay methods
		#region Draw methods
		public bool SetPixelRgb(int x, int y, byte r, byte g, byte b) {
			return SetPixelArgb(x,y,255,r,g,b);
		}
		public bool SetPixelArgb(int x, int y, byte a, byte r, byte g, byte b) {
			bool bGood=false;
			try {
				if (iBytesPP==4) {
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow++]=r;
					byarrData[iDestNow]=a;
				}
				else if (iBytesPP==3) {
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow]=r;
				}
				else if (iBytesPP==1) {
					byarrData[XYToLocation(x,y)]=(byte)(((double)r+(double)g+(double)b)/3.0);
				}
				else Base.ShowErr("Can't draw Argb pixel if GBuffer32BGRA is "+iBytesPP.ToString()+"-bit");
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				Base.ShowExn(exn,"DrawPixelArgb","drawing pixel {x:"+x.ToString()+"; y:"+y.ToString()+";}");
			}
			return bGood;
		}//end SetPixelArgb
		public bool SetPixelR(int x, int y, byte r) {
			bool bGood=false;
			try {
				if (iBytesPP==4||iBytesPP==3) {
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow+2]=r;
				}
				else if (iBytesPP==1) {
					byarrData[XYToLocation(x,y)]=r;
				}
				else Base.ShowErr("Can't draw pixel red channel if GBuffer32BGRA is "+iBytesPP.ToString()+"-bit");
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				Base.ShowExn(exn,"DrawPixelR","drawing pixel red channel {x:"+x.ToString()+"; y:"+y.ToString()+";}");
			}
			return bGood;
		}//end SetPixelR
		public bool SetPixelG(int x, int y, byte g) {
			bool bGood=false;
			try {
				if (iBytesPP==4||iBytesPP==3) {
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow+1]=g;
				}
				else if (iBytesPP==1) {
					byarrData[XYToLocation(x,y)]=g;
				}
				else Base.ShowErr("Can't draw pixel green channel if GBuffer32BGRA is "+iBytesPP.ToString()+"-bit");
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				Base.ShowExn(exn,"DrawPixelG","drawing pixel green channel {x:"+x.ToString()+"; y:"+y.ToString()+";}");
			}
			return bGood;
		}//end SetPixelG
		public bool SetPixelB(int x, int y, byte b) {
			bool bGood=false;
			try {
				if (iBytesPP==4||iBytesPP==3) {
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow]=b;
				}
				else if (iBytesPP==1) {
					byarrData[XYToLocation(x,y)]=b;
				}
				else Base.ShowErr("Can't draw pixel blue channel if GBuffer32BGRA is "+iBytesPP.ToString()+"-bit");
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				Base.ShowExn(exn,"DrawPixelB","drawing pixel blue channel {x:"+x.ToString()+"; y:"+y.ToString()+";}");
			}
			return bGood;
		}//end SetPixelB
		public bool SetPixelHsva(int x, int y, double h, double s, double v, double a0To1) {
			bool bGood=false;
			try {
				if (iBytesPP==4) {
					byte r,g,b;
					//NOTE: Hsv is NOT H<360 it is H<1.0
					Base.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow++]=r;
					byarrData[iDestNow]=(byte)(a0To1*255.0);
				}
				else if (iBytesPP==3) {
					byte r,g,b;
					Base.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow]=r;
				}
				else if (iBytesPP==1) {
					byarrData[XYToLocation(x,y)]=(byte)(v*255.0);
				}
				else Base.ShowErr("Can't draw HSL pixel if GBuffer32BGRA is "+iBytesPP.ToString()+"-bit");
				bGood=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"DrawPixelHSV");
			}
			return bGood;
		}//end SetPixelHsva
		public bool SetPixelHsva(int x, int y, float h, float s, float v, float a0To1) {
			bool bGood=false;
			try {
				if (iBytesPP==4) {
					byte r,g,b;
					//NOTE: Hsv is NOT H<360 it is H<1.0
					Base.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow++]=r;
					byarrData[iDestNow]=(byte)(a0To1*255.0);
				}
				else if (iBytesPP==3) {
					byte r,g,b;
					Base.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow]=r;
				}
				else if (iBytesPP==1) {
					byarrData[XYToLocation(x,y)]=(byte)(v*255.0);
				}
				else Base.ShowErr("Can't draw HSL pixel if GBuffer32BGRA is "+iBytesPP.ToString()+"-bit");
				bGood=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"DrawPixelHSV");
			}
			return bGood;
		}//end SetPixelHsva
		public bool DrawRectCropped(IZone zoneNow) {
			return DrawRectCropped(zoneNow.Left,zoneNow.Top,zoneNow.Width,zoneNow.Height);
		}
		public bool DrawRectCropped(IRect rectNow) {
			return DrawRectCropped(rectNow.X,rectNow.Y,rectNow.Width,rectNow.Height);
		}
		public bool DrawRectCropped(int xDest, int yDest, int iWidth, int iHeight) {
			Base.CropRect(ref xDest, ref yDest, ref iWidth, ref iHeight, 0, 0, Width, Height);
			if (iWidth>0&&iHeight>0) return DrawRect(xDest,yDest,iWidth,iHeight,"DrawCroppedRect");
			else return true;
		}
		public bool DrawRect(int xDest, int yDest, int iWidth, int iHeight, string sSender_ForErrorTracking) {
			bool bGood=true;
			if (xDest>=0&&yDest>=0) {
				if (!DrawHorzLine(xDest,yDest,iWidth,sSender_ForErrorTracking)) bGood=false;
				iHeight--;
				if (!DrawHorzLine(xDest,yDest+iHeight,iWidth,sSender_ForErrorTracking)) bGood=false;
				yDest++;
				iHeight--;//skip the verticle lines' other end too
				if (iHeight>0) {
					if (!DrawVertLine(xDest,yDest,iHeight, sSender_ForErrorTracking)) bGood=false;
					iWidth--;
					if (!DrawVertLine(xDest+iWidth,yDest,iHeight,sSender_ForErrorTracking)) bGood=false;
				}
			}
			else {
				Base.Warning(sSender_ForErrorTracking+" skipped drawing out-of-range rect.","{rect:"+IRect.Description(xDest,yDest,iWidth,iHeight)+"}");
				bGood=false;
			}
			return bGood;
		}
		public bool DrawRect(ref IZone izoneExclusive, string sSender_ForErrorTracking) {
			return DrawRect(izoneExclusive.Left, izoneExclusive.Top,
						 izoneExclusive.Right-izoneExclusive.Left,
						 izoneExclusive.Bottom-izoneExclusive.Top, sSender_ForErrorTracking);
		}
		public bool DrawRect(IRect rectRect, string sSender_ForErrorTracking) {
			return DrawRect(rectRect.X, rectRect.Y, rectRect.Width, rectRect.Height, sSender_ForErrorTracking);
		}
		public unsafe bool DrawRectFilled(int xDest, int yDest, int iWidth, int iHeight, string sSender_ForErrorTracking) {
			//TODO: implement overlay modes here
			if ((iWidth<1)||(iHeight<1)) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=byarrBrush32Copied64) { //keeps GC at bay
					byte* lpDestNow;
					byte* lpDestStart=lpDest;
					for (int yNow=0; yNow<iHeight; yNow++) {
						lpDestNow=lpDestStart;
						for (int i=iWidth/2; i!=0; i--) {
							*((UInt64*)lpDestNow) = *((UInt64*)lpSrc);
							lpDestNow+=8;
						}
						if ((iWidth%2)!=0) {
							*((UInt32*)lpDestNow) = *((UInt32*)lpSrc);
						}
						lpDestStart+=iStride;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"GBuffer32BGRA DrawRectFilled","drawing filled rectangle {sender:"+sSender_ForErrorTracking+"; xDest:"+xDest.ToString()+"; yDest:"+yDest.ToString()+"; iWidth:"+iWidth.ToString()+"; iHeight:"+iHeight.ToString()+";}");
			}
			return bGood;
		} //DrawRectFilled
		public bool DrawRectCroppedFilled(int xDest, int yDest, int iWidth, int iHeight) {
			Base.CropRect(ref xDest, ref yDest, ref iWidth, ref iHeight, 0, 0, Width, Height);
			if (iWidth>0&&iHeight>0) return DrawRectFilled(xDest,yDest,iWidth,iHeight,"DrawRectCroppedFilled");
			else return true;
		}
		public bool DrawRectFilled(IRect rectRect, string sSender_ForErrorTracking) {
			return DrawRectFilled(rectRect.X, rectRect.Y, rectRect.Width, rectRect.Height, sSender_ForErrorTracking);
		}
		//public bool DrawRectFilledHsva(IRect rectDest, float h, float s, float v, float a) {
		//	return DrawRectFilledHsva(rectDest.X,rectDest.Y,rectDest.Width,rectDest.Height,h,s,v,a);
		//}
		//public bool DrawRectFilledHsva(IRect rectDest, double h, double s, double v, double a) {
		//	return DrawRectFilledHsva(rectDest.X,rectDest.Y,rectDest.Width,rectDest.Height,h,s,v,a);
		//}
		//public bool DrawRectFilledHsva(int xDest, int yDest, int iWidth, int iHeight, float h, float s, float v, float a) {
		//	SetBrushHsva(h,s,v,a);
		//	return DrawRectFilled(xDest,yDest,iWidth,iHeight);
		//}
		//public bool DrawRectFilledHsva(int xDest, int yDest, int iWidth, int iHeight, double h, double s, double v, double a) {
		//	SetBrushHsva(h,s,v,a);
		//	return DrawRectFilled(xDest,yDest,iWidth,iHeight);
		//}
		//public bool DrawRectCroppedFilledHsva(int xDest, int yDest, int iWidth, int iHeight, double h, double s, double v, double a) {
		//	SetBrushHsva(h,s,v,a);
		//	return DrawRectCroppedFilled(xDest,yDest,iWidth,iHeight);
		//}
		/// <summary>
		/// DrawRectBorder horizontally and vertically symmetrical
		/// </summary>
		/// <param name="rectRect"></param>
		/// <param name="rectHole"></param>
		/// <returns></returns>
		public bool DrawRectBorderSym(IRect rectRect, IRect rectHole, string sSender_ForErrorTracking) {
			bool bGood=true;
			int xNow;
			int yNow;
			int iWidthNow;
			int iHeightNow;
			try {
				xNow=rectRect.X;
				yNow=rectRect.Y;
				iWidthNow=rectRect.Width;
				iHeightNow=rectHole.Y-rectRect.Y;
				bool bTest=DrawRectFilled(xNow, yNow, iWidthNow, iHeightNow, sSender_ForErrorTracking);//Top full width
				if (!bTest) bGood=false;
				yNow+=rectHole.Height+iHeightNow;
				//would need to change iHeightNow here if asymmetrical
				bTest=DrawRectFilled(xNow, yNow, iWidthNow, iHeightNow, sSender_ForErrorTracking);//Bottom full width
				if (!bTest) bGood=false;
				yNow-=rectHole.Height;
				iWidthNow=rectHole.X-rectRect.X;
				iHeightNow=rectHole.Height;
				bTest=DrawRectFilled(xNow, yNow, iWidthNow, iHeightNow, sSender_ForErrorTracking);//Left remaining height
				if (!bTest) bGood=false;
				xNow+=rectHole.Width+iWidthNow;
				//would need to change iWidthNow here if asymmetrical
				bTest=DrawRectFilled(xNow, yNow, iWidthNow, iHeightNow, sSender_ForErrorTracking);//Right remaining height
				if (!bTest) bGood=false;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"GBuffer32BGRA DrawRectBorderSym");
			}
			return bGood;
		} //end DrawRectBorderSym
		public bool DrawRectBorder(int xDest, int yDest, int iWidth, int iHeight, int iThick) {
			IRect rectOuter=new IRect(xDest,yDest,iWidth,iHeight);
			IRect rectInner=new IRect(xDest+iThick,yDest+iThick,iWidth-(iThick*2),iHeight-(iThick*2));
			if ((rectInner.Width<1) || (rectInner.Height<1)) {
				return DrawRectFilled(rectOuter,"DrawRectBorder");
			}
			else return DrawRectBorderSym(rectOuter, rectInner, "DrawRectBorder");
		}//DrawRectBorder
		public unsafe bool DrawVertLine(int xDest, int yDest, int iPixelCopies, string sSender_ForErrorTracking) {
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=byarrBrush) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					//byte* lpSrcNow=lpSrc;
					//lpSrcNow+=iSrcByte;
					//lpDestNow+=iDestByte;
					for (int i=iPixelCopies; i!=0; i--) {
						*((UInt32*)lpDestNow) = *((UInt32*)lpSrc);
						lpDestNow+=iStride;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"GBuffer32BGRA DrawVertLine","drawing line {sender: "+sSender_ForErrorTracking+"; xDest:"+xDest.ToString()+"; yDest:"+yDest.ToString()+"; downward-run:"+iPixelCopies.ToString()+"}");
			}
			return bGood;
		}//DrawVertLine
		public unsafe bool DrawHorzLine(int xDest, int yDest, int iPixelCopies, string sSender_ForErrorTracking) {
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=byarrBrush32Copied64) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					for (int i=iPixelCopies/2; i!=0; i--) {
						*((UInt64*)lpDestNow) = *((UInt64*)lpSrc);
						lpDestNow+=8;
					}
					if ((iPixelCopies%2)!=0) {
						*((UInt32*)lpDestNow) = *((UInt32*)lpSrc);
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"GBuffer32BGRA DrawHorzLine","drawing line {sender:"+sSender_ForErrorTracking+"; xDest:"+xDest.ToString()+"; yDest:"+yDest.ToString()+"; run:"+iPixelCopies.ToString()+"}");
			}
			return bGood;
		}//end DrawHorzLine
		/*
		public bool DrawFractal(IRect rectDest, DPoint pPixelOrigin, double rPixelsPerUnit) {
			return DrawFractal(rectDest,pPixelOrigin,rPixelsPerUnit,0.0);
		}
		public bool DrawFractal(IRect rectDest, DPoint pPixelOrigin, double rPixelsPerUnit, double rSeed) {
			try {
				if (fracNow==null) fracNow=new Fractal(Width,Height);
				fracNow.SetView(pPixelOrigin.X,pPixelOrigin.Y,rPixelsPerUnit,rSeed);
				fracNow.DrawIncrement(this);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA DrawFractal");
			}
		}
		*/
		
		
		public bool DrawRainbowBurst(IRect rectDest, DPoint pOrigin, double rScale) {
			bool bGood=false;
			try {
				//int xDest=rectDest.Left;
				//int yDest=rectDest.Top;
				int xEnder=rectDest.X+rectDest.Width;
				int yEnder=rectDest.Y+rectDest.Height;
				double rInverseScale=1.0/rScale;
				double xSrcStart=-(pOrigin.X*rInverseScale);
				double xSrc;
				double rSpeed;
				double ySrc=-(pOrigin.Y*rInverseScale);
				for (int yDest=rectDest.Y; yDest<yEnder; yDest++) {
					xSrc=xSrcStart;
					for (int xDest=rectDest.X; xDest<xEnder; xDest++) {
						//rSpeed=;
						this.SetPixelHsva(xDest,yDest,Base.ROFXY(xSrc,ySrc)/360.0,1.0,1.0,1.0);
						xSrc+=rScale;
					}
					ySrc+=rScale;
				}
				bGood=true;
			}
			catch (Exception exn) {	
				Base.ShowExn(exn,"DrawRainbowBurst");
			}
			return bGood;
		}//end DrawRainbowBurst
		public bool DrawWavyThing(IRect rectDest, DPoint pOrigin, double rScale) {
			bool bGood=false;
			try {
				//int xDest=rectDest.Left;
				//int yDest=rectDest.Top;
				int xEnder=rectDest.X+rectDest.Width;
				int yEnder=rectDest.Y+rectDest.Height;
				double rInverseScale=1.0/rScale;
				double xSrcStart=-(pOrigin.X*rInverseScale);
				double xSrc;
				double rSpeed;
				double ySrc=-(pOrigin.Y*rInverseScale);
				for (int yDest=rectDest.Y; yDest<yEnder; yDest++) {
					xSrc=xSrcStart;
					for (int xDest=rectDest.X; xDest<xEnder; xDest++) {
						//rSpeed=;
						this.SetPixelHsva(xDest,yDest,Base.ROFXY(xSrc,ySrc),1.0,.5,1.0);
						xSrc+=rScale;
					}
					ySrc+=rScale;
				}
				bGood=true;
			}
			catch (Exception exn) {	
				Base.ShowExn(exn,"DrawWavyThing");
			}
			return bGood;
		}//end DrawWavyThing
		
		void DrawAlphaPix(int xPix, int yPix, byte r, byte g, byte b, byte a) {
			try {
				int iChannel=yPix*iStride+xPix*iBytesPP;
				//The ++ operators are right:
				float fAlphaTo1;
				if ((iChannel+2>=0) && (iChannel+2<iStride*iHeight))
				if (((iChannel+3)/4)<(iWidth*iBytesPP*iHeight)) {	
					fAlphaTo1=(float)a/255.0f;
					byarrData[iChannel]=(byte)Base.Approach((float)byarrData[iChannel], (float)b, fAlphaTo1);//TODO:? by3dAlphaLookup[b][byarrData[iChannel]][a];
					byarrData[++iChannel]=(byte)Base.Approach((float)byarrData[iChannel], (float)g, fAlphaTo1);//TODO:? by3dAlphaLookup[g][byarrData[iChannel]][a];
					byarrData[++iChannel]=(byte)Base.Approach((float)byarrData[iChannel], (float)r, fAlphaTo1);//TODO:? by3dAlphaLookup[r][byarrData[iChannel]][a];
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"DrawAlphaPix","drawing transparent pixel using 4 channel values");
			}
		}//end DrawAlphaPix
		public void DrawVectorDot(float xDot, float yDot, Pixel32 pixelColor) {
			try {
				//TODO: finish this (finish Vector accuracy)
				// Begin header fields in order of writing //
				//Targa struct reference:
				//bySizeofID byMapType byTgaType wMapOrigin wMapLength byMapBitDepth
				//xImageOrigin yImageOrigin width height byBitDepth bitsDescriptor sTag
				//*byarrColorMap *byarrData footer;
				bool bGood=true;
				//LPIPOINT *lpipointarrNow=malloc(4*sizeof(LPIIPOINT));
	
				int xMin=(int)xDot;//FLOOR
				int xMax=(int)System.Math.Ceiling(xDot);//TODO: make sure 1 doesn't return 2???
				int yMin=(int)yDot;//FLOOR
				int yMax=(int)System.Math.Ceiling(yDot);//TODO: make sure 1 doesn't return 2???
				float xfMin=(float)xMin;
				float xfMax=(float)xMax;
				float yfMin=(float)yMin;
				float yfMax=(float)yMax;
				//int iBytesPP=byBitDepth/8;
				//int iStride=iWidth*iBytesPP;
				//int iStart=yMin*iStride+xMin*iBytesPP;
				float xEccentric,yEccentric,xNormal,yNormal;
				xNormal=1.0f-Base.SafeAbs(xDot-xfMin);
				xEccentric=1.0f-Base.SafeAbs(xDot-xfMax);
				yNormal=1.0f-Base.SafeAbs(yDot-yfMin);
				yEccentric=1.0f-Base.SafeAbs(yDot-yfMax);
				DrawAlphaPix(xMin,yMin,pixelColor.R,pixelColor.G,pixelColor.B,Base.ByRound(pixelColor.A*xNormal*yNormal));
				DrawAlphaPix(xMax,yMin,pixelColor.R,pixelColor.G,pixelColor.B,Base.ByRound(pixelColor.A*xEccentric*yNormal));
				DrawAlphaPix(xMin,yMax,pixelColor.R,pixelColor.G,pixelColor.B,Base.ByRound(pixelColor.A*xNormal*yEccentric));
				DrawAlphaPix(xMax,yMax,pixelColor.R,pixelColor.G,pixelColor.B,Base.ByRound(pixelColor.A*xEccentric*yEccentric));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"DrawVectorDot","drawing pixel to Vector");
			}
		}//end DrawVectorDot
		public void DrawVectorDot(float xDot, float yDot, byte[] lpbySrcPixel) {
			try {
				if (lpbySrcPixel==null) {
					lpbySrcPixel=new byte[4];
					lpbySrcPixel[0]=0;
					lpbySrcPixel[1]=0;
					lpbySrcPixel[2]=0;
					lpbySrcPixel[3]=0;
				}
				Pixel32 pixelNow=new Pixel32(lpbySrcPixel[3],lpbySrcPixel[2],lpbySrcPixel[1],lpbySrcPixel[0]); //debug performance
				DrawVectorDot(xDot,yDot,pixelNow);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"DrawVectorDot","drawing pixel data to Vector");
			}
		}//end DrawVectorDot(float xDot, float yDot, byte* lpbySrcPixel)
		public void DrawVectorLine(ILine line1, Pixel32 pixelStart, Pixel32 pixelEndOrNull, float fSubpixelPrecisionIncrement) {
			DrawVectorLine(SafeConvert.ToFloat(line1.X1), SafeConvert.ToFloat(line1.Y1), SafeConvert.ToFloat(line1.X2), SafeConvert.ToFloat(line1.Y2), pixelStart, pixelEndOrNull, fSubpixelPrecisionIncrement);
		}
		public byte BrushR() {
			try { if (byarrBrush!=null&&byarrBrush.Length>=3) return byarrBrush[2]; }
			catch { }
			return 0;
		}
		public byte BrushG() {
			try { if (byarrBrush!=null&&byarrBrush.Length>=2) return byarrBrush[1]; }
			catch { }
			return 0;
		}
		public byte BrushB() {
			try { if (byarrBrush!=null&&byarrBrush.Length>=1) return byarrBrush[0]; }
			catch { }
			return 0;
		}
		public bool DrawRectFilledSafe(int xDest, int yDest, int iDestW, int iDestH) {
			bool bGood=false;
			if (byarrData!=null) {
				try {
					int xAbs=xDest;
					int yAbs=yDest;
					int xRel=0;
					//int yRel=0;
					int yDestEnder=yDest+iDestH;
					byte byR=BrushR();
					byte byG=BrushG();
					byte byB=BrushB();
					int iDestLine=XYToLocation(xAbs,yAbs);
					int iDestNow=iDestLine;
					if (iDestW>0&&iDestH>0) {
						while (yAbs<yDestEnder&&yAbs<iHeight) {//yRel<iDestH) {
							if (xAbs>=0&&yAbs>=0&&xAbs<iWidth) {//&&yAbs<iHeight) {
								if (iDestNow>=0 && iDestNow+2<iBytesTotal) {
									byarrData[iDestNow]=byB;
									byarrData[iDestNow+1]=byG;
									byarrData[iDestNow+2]=byR;
									iDestNow+=iBytesPP;
									bGood=true;
								}
								else {
									Base.Warning("In-range pixel with out-of-range result in DrawRectFilledSafe");
									break;
								}
							}
							else {
								Base.Warning("Out-of-range pixel in DrawRectFilledSafe","{location:"+IPoint.Description(xAbs,yAbs)+"; start-location:"+IPoint.Description(xDest,yDest)+"; total-size:"+IPoint.Description(iDestW,iDestH)+"; destination-size:"+IPoint.Description(iWidth,iHeight)+"}");
								break;
							}
							xRel++;
							xAbs++;
							if (xRel>=iDestW||xAbs>=iWidth) {
								iDestLine+=iStride;
								iDestNow=iDestLine;
								yAbs++;
								//yRel++;
								xAbs=0;
								xRel=0;
							}
						}
						//bGood=iDestNow!=XYToLocation(xDest,yDest);
						if (iDestNow==XYToLocation(xDest,yDest)) Base.Warning("DrawRectFilledSafe skipped out-of-range rect","{location:"+IPoint.Description(xAbs,yAbs)+"; start-location:"+IPoint.Description(xDest,yDest)+"; total-size:"+IPoint.Description(iDestW,iDestH)+"; destination-size:"+IPoint.Description(iWidth,iHeight)+"}");
						else if (yDest>=yDestEnder) {
							Base.Warning("DrawRectFilledSafe skipped 'y' out-of-range rect","{location:"+IPoint.Description(xAbs,yAbs)+"; start-location:"+IPoint.Description(xDest,yDest)+"; total-size:"+IPoint.Description(iDestW,iDestH)+"; destination-size:"+IPoint.Description(iWidth,iHeight)+"; yDestEnder:"+yDestEnder.ToString()+"}");
						}
						else if (!bGood) {
							Base.Warning("DrawRectFilledSafe skipped rect for unknown reason.","{location:"+IPoint.Description(xAbs,yAbs)+"; start-location:"+IPoint.Description(xDest,yDest)+"; total-size:"+IPoint.Description(iDestW,iDestH)+"; destination-size:"+IPoint.Description(iWidth,iHeight)+"}");
						}
					}
					else Base.Warning("Negative rect skipped in DrawRectFilledSafe.");
				}
				catch {//(Exception exn) {
					Base.Warning("DrawRectFilledSafe failed.");
				}
			}
			return bGood;
		}//end DrawRectFilledSafe
		public void DrawVectorLine(float xStart, float yStart, float xEnd, float yEnd,
				Pixel32 pixelStart, Pixel32 pixelEndOrNull, float fSubpixelPrecisionIncrement) {
			int iLoops=0;
			int iStartTick=Environment.TickCount;
			int iMaxTicks=50;//i.e. 20 per second minimum (soft minimum because of loop limit below)
			int iMaxLoops=1000000; //debug hard-coded limitation
			float xNow, yNow, xRelMax, yRelMax, rRelMax, theta, rRel;
			bool bLimited=false;
			xNow=xStart;
			yNow=yStart;
			xRelMax=xEnd-xStart;
			yRelMax=yEnd-yStart;
			rRelMax=Base.ROFXY(xRelMax,yRelMax);
			theta=Base.THETAOFXY_RAD(xRelMax,yRelMax);
			rRel=0;
			rRel-=fSubpixelPrecisionIncrement; //the "0th" value
			Pixel32 pixelColor=new Pixel32(pixelStart);
			while (rRel<rRelMax && iLoops<iMaxLoops) {
				rRel+=fSubpixelPrecisionIncrement;
				if (pixelEndOrNull!=null) {
					pixelColor.R=Base.Approach(pixelStart.R,pixelEndOrNull.R,rRel/rRelMax);
					pixelColor.G=Base.Approach(pixelStart.G,pixelEndOrNull.G,rRel/rRelMax);
					pixelColor.B=Base.Approach(pixelStart.B,pixelEndOrNull.B,rRel/rRelMax);
					pixelColor.A=Base.Approach(pixelStart.A,pixelEndOrNull.A,rRel/rRelMax);
				}
				xNow=(Base.XOFRTHETA_RAD(rRel,theta))+xStart;
				yNow=(Base.YOFRTHETA_RAD(rRel,theta))+yStart;
				if (xNow>0&&yNow>0&&xNow<iWidth&&yNow<iHeight)
					DrawVectorDot(xNow, yNow, pixelColor);
				iLoops++;
				if ((iLoops>=iMaxLoops)&&(Environment.TickCount-iStartTick>iMaxTicks)) {
					bLimited=true;
					break;//if (iLoops>=iMaxLoops) break;
				}
			}//end while drawing line
			if (bLimited) {
				Base.ShowErr("Line drawing loop overflow","DrawVectorLine","drawing beyond line drawing safe limit");
			}
		}//end DrawVectorLine
	
		public void DrawVectorLine(FPoint pointStart, FPoint pointEnd,
				Pixel32 pixelStart, Pixel32 pixelEndOrNull, float fSubpixelPrecisionIncrement) {
			DrawVectorLine(pointStart.X, pointStart.Y, pointEnd.X, pointEnd.Y,
				pixelStart, pixelEndOrNull, fSubpixelPrecisionIncrement);
		}//DrawVectorLine
		public float PixelsPerDegAt(float rPixelsRadius) {
			float x1,y1,x2,y2;
			x1=(Base.XOFRTHETA_DEG(rPixelsRadius,0));
			y1=-(Base.YOFRTHETA_DEG(rPixelsRadius,0));//negative to flip to non-cartesian monitor loc
			x2=(Base.XOFRTHETA_DEG(rPixelsRadius,1));
			y2=-(Base.YOFRTHETA_DEG(rPixelsRadius,1));//negative to flip to non-cartesian monitor loc
			return Base.Dist(x1,y1,x2,y2);
		}
		public double PixelsPerDegAt(double rPixelsRadius) {
			double x1,y1,x2,y2;
			x1=(Base.XOFRTHETA_DEG(rPixelsRadius,0));
			y1=-(Base.YOFRTHETA_DEG(rPixelsRadius,0));//negative to flip to non-cartesian monitor loc
			x2=(Base.XOFRTHETA_DEG(rPixelsRadius,1));
			y2=-(Base.YOFRTHETA_DEG(rPixelsRadius,1));//negative to flip to non-cartesian monitor loc
			return Base.Dist(x1,y1,x2,y2);
		}
		public float PixelsPerRadAt(float rPixelsRadius) {
			float x1,y1,x2,y2;
			x1=(Base.XOFRTHETA_RAD(rPixelsRadius,0));
			y1=-(Base.YOFRTHETA_RAD(rPixelsRadius,0));//negative to flip to non-cartesian monitor loc
			x2=(Base.XOFRTHETA_RAD(rPixelsRadius,1));
			y2=-(Base.YOFRTHETA_RAD(rPixelsRadius,1));//negative to flip to non-cartesian monitor loc
			return Base.Dist(x1,y1,x2,y2);
		}
		public double PixelsPerRadAt(double rPixelsRadius) {
			double x1,y1,x2,y2;
			x1=(Base.XOFRTHETA_RAD(rPixelsRadius,0));
			y1=-(Base.YOFRTHETA_RAD(rPixelsRadius,0));//negative to flip to non-cartesian monitor loc
			x2=(Base.XOFRTHETA_RAD(rPixelsRadius,1));
			y2=-(Base.YOFRTHETA_RAD(rPixelsRadius,1));//negative to flip to non-cartesian monitor loc
			return Base.Dist(x1,y1,x2,y2);
		}
		public void DrawVectorArc(float xCenter, float yCenter,
				float fRadius, float fWidthMultiplier, float fRotate,
				float fDegStart, float fDegEnd,
				Pixel32 pixelColor,
				float fSubpixelPrecisionIncrement, float fPushSpiralPixPerRotation) {
			float fTemp,xNow,yNow;
			///TODO: make the fSubpixelPrecisionIncrement a pixel increment to match other Vector draw functions
			if (fDegStart>fDegEnd) {
				fTemp=fDegStart;
				fDegStart=fDegEnd;
				fDegEnd=fTemp;
			}
			int iLoops=0;
			int iMaxLoops=1000000;
			float fPrecisionIncrementDeg=fSubpixelPrecisionIncrement*(1.0f/PixelsPerDegAt(fRadius));
			for (float fNow=fDegStart; fNow<fDegEnd; fNow+=fPrecisionIncrementDeg) {
				xNow=(Base.XOFRTHETA_DEG(fRadius,fNow));
				yNow=-(Base.YOFRTHETA_DEG(fRadius,fNow));//negative to flip to non-cartesian monitor loc
				xNow*=fWidthMultiplier;
				Base.Rotate(ref xNow,ref yNow,fRotate);
				xNow+=xCenter;
				yNow+=yCenter;
				if (xNow>0&&yNow>0&&xNow<iWidth&&yNow<iHeight)
					DrawVectorDot(xNow, yNow, pixelColor);
				iLoops++;
				if (iLoops>=iMaxLoops) break;
				if (fPushSpiralPixPerRotation!=0.0f) {
					fRadius+=fPushSpiralPixPerRotation/360.0f;
					fPrecisionIncrementDeg=PixelsPerDegAt(fRadius);
				}
			}
			if (iLoops>=iMaxLoops) {
				Base.ShowErr("DrawVectorArc loop overflow!","DrawVectorArc","drawing arc past time limit performance setting");
			}
		}//DrawVectorArc
		
		public void Fill(byte byGrayVal) {
			Memory.Fill(ref byarrData,byGrayVal,0,iBytesTotal);
		}
		public void Fill(uint dwPixelBGRA) {
			if (iBytesPP==4) {
				Memory.Fill(ref byarrData,dwPixelBGRA,0,iBytesTotal/4);
			}
			else Base.ShowErr("Filling "+(iBytesPP*8).ToString()+"-bit surface with 32-bit color value is not implemented");
		}
		public void Fill(byte[] byarrPixel32bit) {
			if (iBytesPP==4) {
				Memory.Fill4(ref byarrData,ref byarrPixel32bit,0,0,iBytesTotal/4);
			}
			else Base.ShowErr("Filling "+(iBytesPP*8).ToString()+"-bit surface with 32-bit color is not implemented");
		}
		public static bool DrawPixel(GBuffer32BGRA gbDest, int iDestBufferLoc, GBuffer32BGRA gbSrc, int iSrcBufferLoc, int iDrawMode) {
			bool bGood=true;
			int iBytesPPMin=-1;//must start as -1
			float fCookedAlpha=-1.0f;//must start as -1
			try {
				byte[] gbSrc_byarrData=gbSrc.byarrData;
				byte[] gbDest_byarrData=gbDest.byarrData;
				switch (iDrawMode) {
					case DrawModeCopyAlpha:
						iBytesPPMin=(gbSrc.iBytesPP<gbDest.iBytesPP)?gbSrc.iBytesPP:gbDest.iBytesPP;
						if (iBytesPPMin>=3) {
							bGood=Memory.CopyFast(ref gbDest_byarrData, ref gbSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
						}
						else {
							if (gbDest.iBytesPP==4&&gbSrc.iBytesPP==1) {	
								gbDest_byarrData[iDestBufferLoc+3]=gbSrc_byarrData[iSrcBufferLoc];
							}
							else if (gbDest.iBytesPP==1&&gbSrc.iBytesPP==4) {
								gbDest_byarrData[iDestBufferLoc]=gbSrc_byarrData[iSrcBufferLoc+3];
							}
							else bGood=Memory.CopyFast(ref gbDest_byarrData, ref gbSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
						}
						break;
					case DrawModeAlpha:
						if (gbSrc.iBytesPP==4&&gbDest.iBytesPP>=3) {
							fCookedAlpha=(float)gbSrc_byarrData[iSrcBufferLoc+3]/255.0f;
							gbDest_byarrData[iDestBufferLoc]=Base.ByRound(((float)(gbSrc_byarrData[iSrcBufferLoc]-gbDest_byarrData[iDestBufferLoc]))*fCookedAlpha+gbDest_byarrData[iDestBufferLoc]); //B
							iSrcBufferLoc++; iDestBufferLoc++;
							gbDest_byarrData[iDestBufferLoc]=Base.ByRound(((float)(gbSrc_byarrData[iSrcBufferLoc]-gbDest_byarrData[iDestBufferLoc]))*fCookedAlpha+gbDest_byarrData[iDestBufferLoc]); //G
							iSrcBufferLoc++; iDestBufferLoc++;
							gbDest_byarrData[iDestBufferLoc]=Base.ByRound(((float)(gbSrc_byarrData[iSrcBufferLoc]-gbDest_byarrData[iDestBufferLoc]))*fCookedAlpha+gbDest_byarrData[iDestBufferLoc]); //R
						}
						else {
							Base.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","DrawPixel");
						}
						break;
					case DrawModeAlphaQuickEdge:
						if (gbSrc.iBytesPP>=4&&gbDest.iBytesPP>=3) {
							byte byAlpha=gbSrc_byarrData[iSrcBufferLoc+3];
							if (byAlpha<=85) {//do nothing
							}
							else if (byAlpha>170) {
								gbDest_byarrData[iDestBufferLoc]=gbSrc_byarrData[iSrcBufferLoc];
								gbDest_byarrData[iDestBufferLoc+1]=gbSrc_byarrData[iSrcBufferLoc+1];
								gbDest_byarrData[iDestBufferLoc+2]=gbSrc_byarrData[iSrcBufferLoc+2];
							}
							else {
								gbDest_byarrData[iDestBufferLoc]=(byte)( (gbSrc_byarrData[iSrcBufferLoc]>>2) + (gbDest_byarrData[iDestBufferLoc]>>2) ); //B
								iSrcBufferLoc++; iDestBufferLoc++;
								gbDest_byarrData[iDestBufferLoc]=(byte)( (gbSrc_byarrData[iSrcBufferLoc]>>2) + (gbDest_byarrData[iDestBufferLoc]>>2) ); //G
								iSrcBufferLoc++; iDestBufferLoc++;
								gbDest_byarrData[iDestBufferLoc]=(byte)( (gbSrc_byarrData[iSrcBufferLoc]>>2) + (gbDest_byarrData[iDestBufferLoc]>>2) ); //R
							}
						}
						else {
							Base.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","DrawPixel");
						}
						break;
					case DrawModeAlphaHardEdge:
						if (gbSrc.iBytesPP>=4&&gbDest.iBytesPP>=3) {
							byte byAlpha=gbSrc_byarrData[iSrcBufferLoc+3];
							if (byAlpha<128) {//do nothing
							}
							else {
								gbDest_byarrData[iDestBufferLoc]=gbSrc_byarrData[iSrcBufferLoc];
								gbDest_byarrData[iDestBufferLoc+1]=gbSrc_byarrData[iSrcBufferLoc+1];
								gbDest_byarrData[iDestBufferLoc+2]=gbSrc_byarrData[iSrcBufferLoc+2];
							}
						}
						else {
							Base.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","DrawPixel");
						}
						break;
					case DrawModeKeepGreaterAlpha:
						if (gbSrc.iBytesPP>=4&&gbDest.iBytesPP>=3) {
							gbDest_byarrData[iDestBufferLoc]=(gbSrc_byarrData[iSrcBufferLoc]>gbDest_byarrData[iDestBufferLoc])?gbSrc_byarrData[iSrcBufferLoc]:gbDest_byarrData[iDestBufferLoc]; //B
							iSrcBufferLoc++; iDestBufferLoc++;
							gbDest_byarrData[iDestBufferLoc]=(gbSrc_byarrData[iSrcBufferLoc]>gbDest_byarrData[iDestBufferLoc])?gbSrc_byarrData[iSrcBufferLoc]:gbDest_byarrData[iDestBufferLoc]; //G
							iSrcBufferLoc++; iDestBufferLoc++;
							gbDest_byarrData[iDestBufferLoc]=(gbSrc_byarrData[iSrcBufferLoc]>gbDest_byarrData[iDestBufferLoc])?gbSrc_byarrData[iSrcBufferLoc]:gbDest_byarrData[iDestBufferLoc]; //R
						}
						else {
							Base.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","DrawPixel");
						}
						break;
					case DrawModeKeepDestAlpha:
						//if (gbSrc.iBytesPP>=4&&gbDest.iBytesPP>=3) {
						//	iBytesPPMin=(gbSrc.iBytesPP<gbDest.iBytesPP)?gbSrc.iBytesPP:gbDest.iBytesPP;
						//	if (iBytesPPMin>=4) iBytesPPMin=3;
						//	Memory.CopyFast(ref gbDest_byarrData, ref gbSrc_byarrData,iDestPix,iSrcPix,iBytesPPMin);
						//else {
						//	Base.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","DrawPixel");
						//}
						//break;
						iBytesPPMin=(gbSrc.iBytesPP<gbDest.iBytesPP)?gbSrc.iBytesPP:gbDest.iBytesPP;
						if (iBytesPPMin>=3) {
							if (iBytesPPMin>3) iBytesPPMin=3;//since DrawModeKeepDestAlpha
							bGood=Memory.CopyFast(ref gbDest_byarrData, ref gbSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
						}
						else {
							if (gbDest.iBytesPP==4&&gbSrc.iBytesPP==1) {	
								Base.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" when source is an alpha mask and destination is 32-bit.","DrawPixel");
								//gbDest_byarrData[iDestBufferLoc+3]=gbSrc_byarrData[iSrcBufferLoc];
							}
							else if (gbDest.iBytesPP==1) {//&&gbSrc.iBytesPP==4) {
								Base.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" when destination is an alpha mask.","DrawPixel");
								//gbDest_byarrData[iDestBufferLoc]=gbSrc_byarrData[iSrcBufferLoc+3];
							}
							else {
								if (iBytesPPMin>3) iBytesPPMin=3;//since DrawModeKeepDestAlpha
								bGood=Memory.CopyFast(ref gbDest_byarrData, ref gbSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
							}
						}
						break;
					default:
						Base.Warning("DrawPixel mode "+iDrawMode.ToString()+" is not implemented");
						bGood=false;
						break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"DrawPixel","drawing pixel {gbDest:"+gbDest.Description()+"; gbSrc:"+gbSrc.Description()+"; iDestBufferLoc:"+iDestBufferLoc.ToString()+"; iSrcBufferLoc:"+iSrcBufferLoc.ToString()+"; iDrawMode:"+iDrawMode.ToString()+"; iBytesPPMin:"+((iBytesPPMin!=-1)?iBytesPPMin.ToString():"unused")+"; fCookedAlpha:"+((fCookedAlpha!=-1.0f)?fCookedAlpha.ToString():"unused")+"}");
			}
			return bGood;
		}
		/// <summary>
		/// Non-scaled rect-to-rect draw method
		/// </summary>
		public static bool Draw(GBuffer32BGRA gbDest, IRect rectDest, GBuffer32BGRA gbSrc, IRect rectSrc, int iDrawMode) {
			bool bGood=false;
			try {
				int xSrc=rectSrc.X;
				int ySrc=rectSrc.Y;
				int xDest=rectDest.X;
				int yDest=rectDest.Y;
				int xDestRel=0;
				int yDestRel=0;
				int iDestLine=gbDest.XYToLocation(xDest,yDest);
				int iSrcLine=gbSrc.XYToLocation(xSrc,ySrc);
				int iDest=iDestLine;
				int iSrc=iSrcLine;
				if (iDestLine>=0&&iSrcLine>=0) {
					while (yDestRel<rectDest.Height) {
						if (xDestRel>=rectDest.Width) {
							iDestLine+=gbDest.iStride;
							iSrcLine+=gbSrc.iStride;
							iDest=iDestLine;
							iSrc=iSrcLine;
							xDestRel=0;
							yDestRel++;
							ySrc++;
							yDest++;
							xSrc=rectSrc.X;
							xDest=rectDest.X;
						}
						DrawPixel(gbDest,iDest,gbSrc,iSrc,iDrawMode);
						xDestRel++;
						xSrc++;
						xDest++;
						iSrc+=gbSrc.iBytesPP;
						iDest+=gbDest.iBytesPP;
					}
					bGood=true;
				}
				else {
					Base.ShowErr("Cannot draw image at specified overlay location.","Draw(gbDest,rectDest,gbSrc,rectSrc,iDrawMode)","{rectDest:"+rectDest.Description()+"; rectSrc:"+rectSrc.Description()+"}");
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Draw(gbDest,rectDest,gbSrc,rectSrc,iDrawMode)");
			}
			return bGood;
		}
		public bool Draw(IRect rectDest, GBuffer32BGRA gbSrc, IRect rectSrc) {
			return Draw(this, rectDest, gbSrc, rectSrc, DrawModeCopyAlpha);
		}
		public bool Draw(IRect rectDest, GBuffer32BGRA gbSrc, IRect rectSrc, int iDrawMode) {
			return Draw(this,rectDest,gbSrc,rectSrc,iDrawMode);
		}
		public bool Draw(IRect rectDest, GBuffer32BGRA gbSrc) {
			return Draw(rectDest, gbSrc, DrawModeCopyAlpha);
		}
		public bool Draw(IRect rectDest, GBuffer32BGRA gbSrc, int iDrawMode) {
			bool bGood=false;
			try {
				bGood=DrawSmallerWithoutCropElseCancel(rectDest.X,rectDest.Y,gbSrc,iDrawMode);
				//bGood=gbSrc.DrawToLargerWithoutCropElseCancel(this,rectDest.X,rectDest.Y,iDrawMode);
				//TODO: finish this--add cropping capability
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Draw(GBuffer32BGRA gbSrc,...)");
			}
			return bGood;
		}
		#endregion Draw methods
	}//end class GBuffer32BGRA

	public class GBuffer32BGRAStack { //pack Stack -- array, order left(First) to right(Last)
		private GBuffer32BGRA[] gbarr=null;
		private int Maximum {
			get {
				return (gbarr==null)?0:gbarr.Length;
			}
			set {
				GBuffer32BGRA.Redim(ref gbarr,value,"GBuffer32BGRAStack");
			}
		}
		private int iCount;
		private int LastIndex {	get { return iCount-1; } }
		private int NewIndex { get  { return iCount; } }
		//public bool IsFull { get { return (iCount>=Maximum) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public GBuffer32BGRA Element(int iElement) {
			return (iElement<iCount&&iElement>=0&&gbarr!=null)?gbarr[iElement]:null;
		}
		public int Count {
			get {
				return iCount;
			}
		}
		/////<summary>
		/////
		/////</summary>
		//public int CountInstancesI(string sCaseInsensitiveSearch) {
		//	int iReturn=0;
		//	sCaseInsensitiveSearch=sCaseInsensitiveSearch.ToLower();
		//	for (int iNow=0; iNow<iCount; iNow++) {
		//		if (gbarr[iNow].ToLower()==sCaseInsensitiveSearch) iReturn++;
		//	}
		//	return iReturn;
		//}
		//public int CountInstances(string sCaseSensitiveSearch) {//commented for debug only (to remember to use CountInstancesI)
		//	int iReturn=0;
		//	for (int iNow=0; iNow<iCount; iNow++) {
		//		if (gbarr[iNow]==sCaseSensitiveSearch) iReturn++;
		//	}
		//	return iReturn;
		//}
		public GBuffer32BGRAStack() { //Constructor
			int iDefaultSize=256;
			Base.settings.GetOrCreate(ref iDefaultSize,"GBuffer32BGRAStackDefaultStartSize");
			Init(iDefaultSize);
		}
		public GBuffer32BGRAStack(int iSetMax) { //Constructor
			Init(iSetMax);
		}
		private void Init(int iSetMax) { //always called by Constructor
			if (iSetMax<0) Base.Warning("GBuffer32BGRAStack initialized with negative number so it will be set to a default.");
			else if (iSetMax==0) Base.Warning("GBuffer32BGRAStack initialized with zero so it will be set to a default.");
			if (iSetMax<=0) iSetMax=1;
			Maximum=iSetMax;
			iCount=0;
			if (gbarr==null) Base.ShowErr("Stack constructor couldn't initialize gbarr");
		}
		public void Clear() {
			iCount=0;
			for (int iNow=0; iNow<gbarr.Length; iNow++) {
				gbarr[iNow]=null;
			}
		}
		public void ClearFastWithoutFreeingImagesFromMemory() {
			iCount=0;
		}
		public void SetFuzzyMaximumByLocation(int iLoc) {
			Maximum=Base.LocationToFuzzyMaximum(Maximum,iLoc);
		}
		public bool Push(GBuffer32BGRA add) {
			//if (!IsFull) {
			try {
				if (add!=null) {
					if (NewIndex>=Maximum) SetFuzzyMaximumByLocation(NewIndex);
					gbarr[NewIndex]=add;
					iCount++;
				}
				else Base.ShowErr("Cannot push a null GBuffer32BGRA to a stack.","GBuffer32BGRAStack Push","pushing a null GBuffer32BGRA to stack");
				//sLogLine="debug enq iCount="+iCount.ToString();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRAStack Push("+((add==null)?"null GBuffer32BGRA":"non-null")+")","setting gbarr["+NewIndex.ToString()+"]");
				return false;
			}
			return true;
			//}
			//else {
			//	Base.ShowErr("GBuffer32BGRAStack is full, can't push \""+add+"\"! ( "+iCount.ToString()+" GBuffer32BGRAs already used)","GBuffer32BGRAStack Push("+((add==null)?"null GBuffer32BGRA":"non-null")+")");
			//	return false;
			//}
		}
		public GBuffer32BGRA Pop() {
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				//Base.ShowErr("no GBuffer32BGRAs to return so returned null","GBuffer32BGRAStack Pop");
				return null;
			}
			int iReturn = LastIndex;
			iCount--;
			return gbarr[iReturn];
		}
		public GBuffer32BGRA[] ToArrayByReferences() {
			GBuffer32BGRA[] gbarrReturn=null;
			try {
				if (iCount>0) {
					gbarrReturn=new GBuffer32BGRA[iCount];
					for (int iNow=0; iNow<iCount; iNow++) {
						gbarrReturn[iNow]=gbarr[iNow];
					}
				}
				else Base.ShowErr("Cannot copy a zero-length stack.","GBuffer32BGRAStack ToArrayByReferences","copying zero-length stack to array");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRAStack ToArrayByReferences");
			}
			return gbarrReturn;
		}
	}//end GBuffer32BGRAStack created 2007-10-03
}//end namespace