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
///TODO: finish this -- finish implementing brushFore and brushBack by eliminating all modifications to brush in draw methods
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace ExpertMultimedia {
	/// <summary>
	/// For simple graphics buffers used as images, variable-size frames, or graphics surfaces.
	/// </summary>
	public class RImage {
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
		public static RBrush brushFore=new RBrush();//private static byte[] byarrBrushBack=null;
		public static RBrush brushBack=new RBrush();//private static byte[] byarrBrushBack32Copied64=null;
		public string sPathFileBaseName="1.untitled";
		public string sFileExt="raw";
		public static string sPixel32StyleIsAlways { get { return "bgra"; } } //assumes 32-bit
		private static bool bShowGetPixelError=true;
		#region constructors
		public RImage() {
			InitNull();
		}
		///<summary>
		///Loads file as a 32-bit image (will convert bitdepth if necessary)
		///</summary>
		public RImage(string sFileImage) {
			if(!Load(sFileImage,4)) {
				iBytesTotal=0;
				iPixelsTotal=0;
			}
		}
		public RImage(string sFileImage,int iAsBytesPP) {
			if(!Load(sFileImage,iAsBytesPP)) {
				iBytesTotal=0;
				iPixelsTotal=0;
			}
		}
		public RImage(int iWidthNow, int iHeightNow) {
			Init(iWidthNow, iHeightNow, 4, true);
		}
		public RImage(int iWidthNow, int iHeightNow, int iBytesPPNow) {
			Init(iWidthNow, iHeightNow, iBytesPPNow, true);
		}
		public RImage(int iWidthNow, int iHeightNow, int iBytesPPNow, bool bInitializeBuffer) {
			Init(iWidthNow, iHeightNow, iBytesPPNow, bInitializeBuffer);
		}
		public void Init(int iWidthNow, int iHeightNow, int iBytesPPNow, bool bInitializeBuffer) {
			iBytesPP=iBytesPPNow;
			iWidth=iWidthNow;
			iHeight=iHeightNow;
			iStride=iWidth*iBytesPP;
			iBytesTotal=iStride*iHeight;
			iPixelsTotal=iWidth*iHeight;
			if (bInitializeBuffer) {
				try {
					byarrData=new byte[iBytesTotal];
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"","RImage Init");
					iBytesTotal=0;//debug, this is currently used to denote fatal buffer creation errors
					iPixelsTotal=0;
				}
			}
			brushFore.SetArgb(255,255,0,128);
		}//end Init
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
			iWidth=0;
			iHeight=0;
			iBytesTotal=0;
			iPixelsTotal=0;
			iStride=0;
			iBytesPP=0;
		}
		public bool CopyTo(RImage riReturn) {
			bool bGood=false;
			try {
				if (!IsLike(riReturn)) riReturn=new RImage(iWidth,iHeight,iBytesPP);
				for (int iNow=0; iNow<iBytesTotal; iNow++) {
					riReturn.byarrData[iNow]=byarrData[iNow];
				}
				//brushFore=riReturn.brushFore.Copy(); //commented since static
				//brushBack=riReturn.brushBack.Copy(); //commented since static
				riReturn.sPathFileBaseName=sPathFileBaseName+" (Copy)";
				riReturn.sFileExt=sFileExt;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage CopyTo");
				riReturn=null;
			}
			return bGood;
		}//end CopyTo
		public RImage Copy() {
			RImage riReturn;
			bool bTest=false;
			try {
				riReturn=new RImage(iWidth,iHeight,iBytesPP);
				CopyTo(riReturn);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage Copy");
				riReturn=null;
			}
			if (!bTest) riReturn=null;
			return riReturn;
		}//end Copy
		public RImage CreateFromZoneEx(int iFromLeft, int iFromTop, int iFromRight, int iFromBottom) {
			RImage riReturn;
			bool bTest=false;
			try {
				int iWidthNow=iFromRight-iFromLeft;
				int iHeightNow=iFromBottom-iFromTop;
				riReturn=new RImage(iWidthNow,iHeightNow,iBytesPP);
				IRect rectNow=new IRect(iFromLeft,iFromTop,iWidthNow,iHeightNow);
				bTest=riReturn.Draw(riReturn.ToRect(),this,rectNow);
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn,"",String.Format("RImage CreateFromZoneEx(left:{0},top:{1},right:{2},bottom:{3}){{currently:{4}}}",iFromLeft,iFromTop,iFromRight,iFromBottom,Description()) );
				riReturn=null;
			}
			if (!bTest) riReturn=null;
			return riReturn;
		}//end CreateFromZoneEx
		#endregion constructors
		#region file operations
		public unsafe bool Load(string sFile, int iAsBytesPP) {
			bool bGood=true;
			try {
				if (!File.Exists(sFile)) {
					RReporting.ShowErr("Missing resource \""+sFile+"\"","","RImage Load");
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
								RReporting.ShowErr("Failed to load "+(iAsBytesPP*8).ToString()+"-bit buffer.","","RImage Load");
								bGood=false;
								break;
							}
						}//end for x
					}//end for y
				}
				else RReporting.ShowErr("Can't create a "+iWidth.ToString()+"x"+iHeight.ToString()+"x"+iAsBytesPP.ToString()+"-bit buffer","","RImage Load");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage Load");
				bGood=false;
			}
			return bGood;
		}//end Load
		public bool Save(string sSetFile) {
			//TODO:? check for tga extension
			Base.SplitFileName(out sPathFileBaseName, out sFileExt, sSetFile);
			return Save(sPathFileBaseName+"."+sFileExt, Base.ImageFormatFromNameElseCapitalizedPng(ref sPathFileBaseName, out sFileExt));
		}
		public bool Save(string FileNameWithoutExtension, string FileExtension) {
			//TODO:? check for tga extension
			sPathFileBaseName=FileNameWithoutExtension;
			sFileExt=FileExtension;
			return Save(FileNameWithoutExtension+"."+FileExtension, Base.ImageFormatFromExt(FileExtension));
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
							RReporting.ShowErr("Failed to save "+(iBytesPP*8).ToString()+"-bit buffer.","","RImage Save");
							bGood=false;
							break;
						}
						iNow+=iBytesPP;
					}
				}
				bmpLoaded.Save(sFileNow, imageformatNow);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"saving image from 32-bit buffer","RImage Save(\""+sFileNow+"\", "+imageformatNow.ToString()+")");
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
					RReporting.ShowErr("Failed to write raw data to buffer","","RImage SaveRaw("+sFileNow+")");
				}
				if (!byterTemp.Save(sFileNow)) {
					bGood=false;
					RReporting.ShowErr("Failed to save raw data to file","","RImage SaveRaw("+sFileNow+")");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage SaveRaw("+sFileNow+")");
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
				RReporting.ShowExn(exn,"","RImage Save(\""+sFileNow+"\", "+imageformatNow.ToString()+")");
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
				RReporting.ShowExn(exn,"","unsafe RImage Load");
				bGood=false;
			}
			return bGood;
		}//end Load
		*/
		#endregion file operations
		///<summary>
		///Returns true if dimensions, number of channels, and total buffer size are the same as riTest.
		///</summary>
		public bool IsLike(RImage riTest) {//formerly IsSameAs
			bool bReturn=false;
			if (riTest!=null) {
				if ( riTest.iWidth==iWidth
					&& riTest.iHeight==iHeight
					&& riTest.Channels()==Channels()
					&& riTest.iBytesTotal==iBytesTotal )
					bReturn=true;
			}
			return bReturn;
		}
		#region utilities
		///<summary>
		///Returns false if rect is not within riDest
		///</summary>
		public static bool CropRect(ref int RectToModify_X, ref int RectToModify_Y, ref int RectToModify_Width, ref int RectToModify_Height, RImage Boundary) {
			return riDest!=null?RMath.CropRect(ref RectToModify_X,ref RectToModify_Y,ref RectToModify_Width,ref RectToModify_Height, 0,0,Boundary.Width,Boundary.Height):false;
		}
		public static bool InvertRect(RImage riDest, int xAt, int yAt, int iSetWidth, int iSetHeight) {//TODO: move to rtextbox
			bool bGood=false;
			if (RMath.CropRect(riDest, ref xAt, ref yAt, ref iSetWidth, ref iSetHeight)) {
				int xAtRow=xAt;
				int yRel=0;
				int xRel=0;
				int xAbs=xAt;
				int yAbs=yAt;
				Color colorNow;
				try {
					for (yRel=0; yRel<iSetHeight; yRel++) {
						xAbs=xAtRow;
						for (xRel=0; xRel<iSetWidth; xRel++) {
							riDest.InvertPixel(xAbs,yAbs);
							//colorNow=riDest.GetPixel(xAbs,yAbs);
							//riDest.SetPixel(xAbs,yAbs,Color.FromArgb(255,255-colorNow.R,255-colorNow.G,255-colorNow.B));
							//Base.WriteLine("("+colorNow.R.ToString()+","+colorNow.G.ToString()+","+colorNow.B.ToString()+")");
							xAbs++;
						}
						yAbs++;
					}
					bGood=true;
				}
				catch (Exception exn) {
					bGood=false;
					RReporting.ShowExn(exn,"drawing invert rectangle","rform InvertRect {"
						+"xAt:"+xAt.ToString()+"; "
						+"yAt:"+yAt.ToString()+"; "
						+"xAbs:"+xAbs.ToString()+"; "
						+"yAbs:"+yAbs.ToString()+"; "
						+"iSetWidth:"+iSetWidth.ToString()+"; "
						+"iSetHeight:"+iSetHeight.ToString()+"; "
						+"}");
				}
			}
			return bGood;
		}//end InvertRect
		public static bool InvertRect(Bitmap bmpNow, int xAt, int yAt, int iSetWidth, int iSetHeight) {//TODO: move to rtextbox
			bool bGood=false;
			int xAtRow=xAt;
			int yRel=0;
			int xRel=0;
			int xAbs=xAt;
			int yAbs=yAt;
			Color colorNow;
			try {
				for (yRel=0; yRel<iSetHeight; yRel++) {
					xAbs=xAtRow;
					for (xRel=0; xRel<iSetWidth; xRel++) {
						colorNow=bmpNow.GetPixel(xAbs,yAbs);
						bmpNow.SetPixel(xAbs,yAbs,Color.FromArgb(255,255-colorNow.R,255-colorNow.G,255-colorNow.B));
						//Base.WriteLine("("+colorNow.R.ToString()+","+colorNow.G.ToString()+","+colorNow.B.ToString()+")");
						xAbs++;
					}
					yAbs++;
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing invert rectangle","rform InvertRect(Bitmap) {"
					+"xAt:"+xAt.ToString()+"; "
					+"yAt:"+yAt.ToString()+"; "
					+"xAbs:"+xAbs.ToString()+"; "
					+"yAbs:"+yAbs.ToString()+"; "
					+"iSetWidth:"+iSetWidth.ToString()+"; "
					+"iSetHeight:"+iSetHeight.ToString()+"; "
					+"}");
			}
			return bGood;
		}//end InvertRect
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
		public Color GetPixel(int x, int y) {//keep this same to mimic the Bitmap method of the same name
			//TODO: wrapping modes
			int iLoc=XYToLocation(x,y);
			if (iLoc>0) {
				if (iBytesPP==4) return Color.FromArgb(byarrData[iLoc+3], byarrData[iLoc+2], byarrData[iLoc+1], byarrData[iLoc]);
				else if (bShowGetPixelError) {
					 RReporting.Warning("Getting ARGB Color object from non-32bit RImage is not implemented");
					 bShowGetPixelError=false;
				}
			}
			return Color.FromArgb(0,0,0,0);
		}
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
				RReporting.ShowErr("("+x.ToString()+","+y.ToString()+") is outside of "+Width.ToString()+"x"+Height.ToString()+" image.");
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
				RReporting.ShowExn(exn,"","RImage ChannelValue");
			}
			return byReturn;
		}
		///<summary>
		///Synonym for VariableMessage
		///</summary>
		public static string ToString(RImage val) {
			return VariableMessage(val);
		}
		public static string VariableMessage(RImage val) {
			try {
				return (val!=null) ? val.Description() : "null" ;
			}
			catch {//do not report this
				return "incorrectly-initialized-rimage";
			}
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
			return DumpStyle(true);
		}
		public string DumpStyle(bool bIncludeBraces) {
			return String.Format( ((bIncludeBraces)?"{{":"")+"{0}{1}{2}{3}{4}{5}"+((bIncludeBraces)?"}}":""),
			RReporting.DebugStyle("iWidth",iWidth,true),
			RReporting.DebugStyle("iHeight",iHeight,true),
			RReporting.DebugStyle("iBytesPP",iBytesPP,true),
			RReporting.DebugStyle("iStride",iStride,true),
			RReporting.DebugStyle("iBytesTotal",iBytesTotal,true),
			RReporting.DebugStyle("iPixelsTotal",iPixelsTotal,!bIncludeBraces) );
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
				else RReporting.ShowErr("Image not initialized.","","RImage SetGrayPalette");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage SetGrayPalette");
			}
		}
		public static Color Multiply(Color colorX, double multiplier) {
			return Color.FromArgb(colorX.A, RMath.ByRound((double)colorX.R*multiplier), RMath.ByRound((double)colorX.G*multiplier), RMath.ByRound((double)colorX.B*multiplier));
		}
		///<summary>
		///Sets pixel using foreground brush
		///</summary>
		public unsafe void SetPixel(int x, int y) {
			SetPixel(brushFore,x,y);
		}
		///TODO: eliminate assumed brushFore overloads of ALL functions
		public unsafe void SetPixel(RBrush brushX, int x, int y) {
			try {
				if (brushX==null) {
					brushX=new RBrush();
					brushX.SetArgb(255,255,255);
				}
				if (iBytesPP==4) {
					fixed (byte* lpDest=byarrData[XYToLocation(x,y)], lpSrc=brushX.data32) {
						*((UInt32*)lpDest) = *((UInt32*)lpSrc);
					}
				}
				else {
					int iSrc=0;
					int iDest=XYToLocation(x,y);
					while (iSrc<iBytesPP) {
						byarrData[iDest]=brushX.data32[iSrc];
						iSrc++;
						iDest++;
					}
				}//end else iBytesPP!=4
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage SetPixel("+x.ToString()+","+y.ToString()+")");
				return false;
			}
			return true;
		}//end SetPixel(x,y)
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
					RReporting.ShowErr( "No line of pixels exists at location", "checking for transparent line", String.Format("IsTransparentVLine(x:{0},y:{1},count:{2}){found:{3}}",x,y,iPixelCount,iFound) );
				}
			}
			catch (Exception exn) {
				//do not modify bReturn, since exception implies nonexistent pixel
				RReporting.ShowExn( exn, "checking for transparent line", String.Format("IsTransparentVLine(x:{0},y:{1},count:{2}){found:{3}}",x,y,iPixelCount,iFound) );
			}
			return iFound==iPixelCount;
		}//end IsTransparentVLine
		public static int SafeLength(RImage[] val) {	
			int iReturn=0;
			try {
				if (val!=null) iReturn=val.Length;
			}
			catch (Exception exn) {
				iReturn=0;
				RReporting.Debug(exn,"","RImage SafeLength(RImage[])");
			}
			return iReturn;
		}
			
		public static bool Redim(ref RImage[] valarr, int iSetSize, string sSender_ForErrorTracking) {
			bool bGood=false;
			if (iSetSize==0) valarr=null;
			else if (iSetSize>0) {
				if (iSetSize!=SafeLength(valarr)) {
					RImage[] valarrNew=new RImage[iSetSize];
					for (int iNow=0; iNow<iSetSize; iNow++) {
						if (iNow<SafeLength(valarr)) valarrNew[iNow]=valarr[iNow];
						else valarrNew[iNow]=null;
					}
					valarr=valarrNew;
					bGood=true;
				}
			}
			else RReporting.ShowErr("Prevented setting a buffer array to a negative size ", "setting buffer array length", "Redim(rimage array," + iSetSize.ToString() + ",sender:" + sSender_ForErrorTracking+")");
			return bGood;
		}//end Redim(RImage[],...)
		#endregion utilities
				
		#region editing
		public static bool RawCropSafer(ref RImage riDest, ref IPoint ipSrc, ref RImage riSrc) {
			bool bGood=true;
			int iByDest=0;
			int iBySrc;//=ipSrc.Y*riSrc.iStride+ipSrc.X*riSrc.iBytesPP;
			int yDest;
			int xDest;
			if (riDest.iBytesPP!=riSrc.iBytesPP) throw new ApplicationException("Mismatched image bitdepths, couldn't RawCrop!");
			try {
				for (yDest=0; yDest<riDest.iHeight; yDest++) {
					for (xDest=0; xDest<riDest.iWidth; xDest++) {
						iBySrc=(yDest+ipSrc.Y)*riSrc.iStride+(xDest+ipSrc.X)*riSrc.iBytesPP;
						for (int iComponent=0; iComponent<riDest.iBytesPP; iComponent++) {
							if (iBySrc>=0&&iBySrc<riSrc.iBytesTotal) riDest.byarrData[iByDest]=riSrc.byarrData[iBySrc];
							else riDest.byarrData[iByDest]=0;
							iByDest++;
							iBySrc++;
						}
						//iByDest+=riDest.iBytesPP;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage RawCropSafer");
				bGood=false;
			}
			return bGood;
		}
		
		/// <summary>
		/// riDest must be true color 24- or 32-bit for the raw source
		/// to be represented correctly.
		/// </summary>
		/// <param name="byarrSrc"></param>
		/// <param name="riDest"></param>
		/// <param name="iSrcWidth"></param>
		/// <param name="iSrcHeight"></param>
		/// <param name="iSrcBytesPP"></param>
		/// <returns></returns>
		public static bool RawOverlayNoClipToBig(ref RImage riDest, ref IPoint ipAt, ref byte[] byarrSrc, int iSrcWidth, int iSrcHeight, int iSrcBytesPP) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			int iDestAdder;
			try {
				if (iSrcBytesPP==16) {
					RReporting.ShowErr("16-bit source isn't implemented in this function","overlaying 16-bit source image","RImage RawOverlayNoClipToBig");
				}
				iDestByte=ipAt.Y*riDest.iStride+ipAt.X*riDest.iBytesPP;
				RImage riSrc=new RImage(iSrcWidth, iSrcHeight, iSrcBytesPP, false);
				riSrc.byarrData=byarrSrc;
				iDestAdder=riDest.iStride - riSrc.iWidth*riDest.iBytesPP;//intentionally riDest.iBytesPP
				iSrcByte=0;
				int iSlack=(riSrc.iBytesPP>riDest.iBytesPP)?(riSrc.iBytesPP-riDest.iBytesPP):1;
						//offset of next source pixel after loop
				for (int ySrc=0; ySrc<riSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<riSrc.iWidth; xSrc++) {
						for (int iChannel=0; iChannel<riDest.iBytesPP; iChannel++) {
							riDest.byarrData[iDestByte]=riSrc.byarrData[iSrcByte];
							if ((iChannel+1)<riSrc.iBytesPP) iSrcByte++;//don't advance to next pixel
							iDestByte++;
						}
				        iSrcByte+=iSlack;
					}
					iDestByte+=iDestAdder;
				}
				if (!bGood) {
					RReporting.ShowErr("Error copying graphics buffer data","","RImage RawOverlayNoClipToBig");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage RawOverlayNoClipToBig");
				bGood=false;
			}
			return bGood;
		}//end RawOverlayNoClipToBig

		/// <summary>
		/// RGradient as lightmap version of Alpha overlay
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="riSrc"></param>
		/// <param name="ipDest"></param>
		/// <param name="gradNow"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBig(ref RImage riDest, ref IPoint ipDest, ref RImage riSrc, ref RGradient gradNow, int iSrcChannel) {
			int iSrcByte;
			int iDestByte;
			int iDestAdder;
			bool bGood=true;
			try {
				iDestByte=ipDest.Y*riDest.iStride+ipDest.X*riDest.iBytesPP;
				iSrcByte=(iSrcChannel<riSrc.iBytesPP)?iSrcChannel:riSrc.iBytesPP-1;
				iDestAdder=riDest.iStride - riDest.iBytesPP*riSrc.iWidth;//intentionally the dest BytesPP
				for (int ySrc=0; ySrc<riSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<riSrc.iWidth; xSrc++) {
						if (!gradNow.ShadeAlpha(ref riDest.byarrData, iDestByte, riSrc.byarrData[iSrcByte])) {
							bGood=false;
						}
						iSrcByte+=riSrc.iBytesPP;
						iDestByte+=riDest.iBytesPP;
					}
					iDestByte+=iDestAdder;
				}
				if (!bGood) {
					RReporting.ShowErr("Error accessing gradient","","RImage OverlayNoClipToBig gradient to "+IPoint.ToString(ipDest));
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage OverlayNoClipToBig gradient to "+ipDest!=null?ipDest.ToString():"null point");
				bGood=false;
			}
			return bGood;
		}//end OverlayNoClipToBig, using gradient
		
		
		/// <summary>
		/// CopyAlpha (no blending) overlay, using gradient as lightmap
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="riSrc"></param>
		/// <param name="gradNow"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBigCopyAlpha(ref RImage riDest, ref IPoint ipAt, ref RImage riSrc, ref RGradient gradNow, int iSrcChannel) {
			int iSrcByte;
			int iDestByte;
			int iDestAdder;
			bool bGood=true;
			try {
				iDestByte=ipAt.Y*riDest.iStride+ipAt.X*riDest.iBytesPP;
				iSrcByte=(iSrcChannel<riSrc.iBytesPP)?iSrcChannel:riSrc.iBytesPP-1;
				iDestAdder=riDest.iStride - riSrc.iWidth*riDest.iBytesPP;//intentionally the dest BytesPP
				for (int ySrc=0; ySrc<riSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<riSrc.iWidth; xSrc++) {
						if (!gradNow.Shade(ref riDest.byarrData, iDestByte, riSrc.byarrData[iSrcByte])) {
							bGood=false;
						}
						iSrcByte+=riSrc.iBytesPP;
						iDestByte+=riDest.iBytesPP;
					}
					iDestByte+=iDestAdder;
				}
				if (!bGood) {
					RReporting.ShowErr("Error copying graphics buffer data","","RImage OverlayNoClipToBigCopyAlpha gradient");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage OverlayNoClipToBigCopyAlpha gradient");
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlpha gradient
		
		/// <summary>
		/// CopyAlpha overlay function.  
		/// "ToBig" functions must overlay small
		/// image to big image without cropping else unexpected results occur.
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="riSrc"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBigCopyAlpha(ref RImage riDest, ref IPoint ipAt, ref RImage riSrc) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			try {
				iDestByte=ipAt.Y*riDest.iStride+ipAt.X*riDest.iBytesPP;
				iSrcByte=0;
				for (int ySrc=0; ySrc<riSrc.iHeight; ySrc++) {
					if (!RMemory.CopyFast(ref riDest.byarrData, ref riSrc.byarrData, iDestByte, iSrcByte, riSrc.iStride)) {
						bGood=false;
					}
					iSrcByte+=riSrc.iStride;
					iDestByte+=riDest.iStride;
				}
				if (!bGood) {
					RReporting.ShowErr("Error copying graphics buffer data","","RImage OverlayNoClipToBigCopyAlpha");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage OverlayNoClipToBigCopyAlpha");
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlpha
		
		public static bool OverlayNoClipToBigCopyAlphaSafe(ref RImage riDest, ref IPoint ipAt, ref RImage riSrc) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			int iSrcByteNow;
			int iDestByteNow;
			int iPastLine;//the byte location after the end of the line
			try {
				iDestByte=ipAt.Y*riDest.iStride+ipAt.X*riDest.iBytesPP;
				iSrcByte=0;
				for (int ySrc=0; ySrc<riSrc.iHeight; ySrc++) {
					if ((iSrcByte+riSrc.iStride)-1 >= riSrc.iBytesTotal) {
					//Fix overflow:
						iDestByteNow=iDestByte;
						iPastLine=iSrcByte+riSrc.iStride;
						for (iSrcByteNow=iSrcByte; iSrcByteNow<iPastLine; iSrcByteNow++) {
							if (iSrcByteNow>=riSrc.iBytesTotal || iSrcByteNow<0) riDest.byarrData[iDestByteNow]=0;
							else riDest.byarrData[iDestByteNow]=riSrc.byarrData[iSrcByteNow];
						}
					}
					else if (iSrcByte<0) {
					//Fix underflow:
						iDestByteNow=iDestByte;
						iPastLine=iSrcByte+riSrc.iStride;
						for (iSrcByteNow=iSrcByte; iSrcByteNow<iPastLine; iSrcByteNow++) {
							if (iSrcByteNow<0) riDest.byarrData[iDestByteNow]=0;
							else riDest.byarrData[iDestByteNow]=riSrc.byarrData[iSrcByteNow];
						}
					}
					else {//just copy if within bounds
						if (!RMemory.CopyFast(ref riDest.byarrData, ref riSrc.byarrData, iDestByte, iSrcByte, riSrc.iStride)) {
							bGood=false;
						}
					}
					iSrcByte+=riSrc.iStride;
					iDestByte+=riDest.iStride;
				}
				if (!bGood) {
					RReporting.ShowErr("Error copying graphics buffer data","","RImage OverlayNoClipToBigCopyAlpha");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage OverlayNoClipToBigCopyAlpha");
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlphaSafe
		public static bool MaskFromChannel(ref RImage riDest, ref RImage riSrc, int iByteInPixel) {
			int iDestByte=0;
			int iSrcByte=iByteInPixel;
			int iBytesCopy;
			int iBytesPPOffset;
			try {
				if (riDest==null) {
					riDest=new RImage(riSrc.iWidth, riSrc.iHeight, 1);
				}
				iBytesCopy=riDest.iBytesTotal;
				iBytesPPOffset=riSrc.iBytesPP;
				for (iDestByte=0; iDestByte<iBytesCopy; iDestByte++) {
					riDest.byarrData[iDestByte]
						= riSrc.byarrData[iSrcByte];
					iDestByte++;
					iSrcByte+=iBytesPPOffset;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"creating mask","RImage MaskFromChannel "
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
		public static bool MaskFromValue(ref RImage riDest, ref RImage riSrc) {
			int iDestByte=0;
			int iSrcByte=0;
			int iPixels;
			int iBytesPPOffset;
			try {
				if (riDest==null) {
					riDest=new RImage(riSrc.iWidth, riSrc.iHeight, 1);
				}
				iPixels=riSrc.iWidth*riSrc.iHeight;
				iBytesPPOffset=riSrc.iBytesPP;
				for (iDestByte=0; iDestByte<iPixels; iDestByte++) {
					if (riSrc.iBytesPP==4) {
						riDest.byarrData[iDestByte]=(byte)(   
							(  ( (float)riSrc.byarrData[iSrcByte]
								+(float)riSrc.byarrData[iSrcByte+1]
								+(float)riSrc.byarrData[iSrcByte+2] )  /  3.0f  )
								   *   ((float)riSrc.byarrData[iSrcByte+3]/255.0f)   );
					}
					else if (riSrc.iBytesPP==1) {
						riDest.byarrData[iDestByte]=riSrc.byarrData[iSrcByte];
					}
					else {
						riDest.byarrData[iDestByte]=(byte)(((float)riSrc.byarrData[iSrcByte]
								+(float)riSrc.byarrData[iSrcByte+1]
								+(float)riSrc.byarrData[iSrcByte+2])/3.0f);
					}
					iSrcByte+=iBytesPPOffset;
					//if (riDest.byarrData[iDestByte]>0) RReporting.DebugWrite(riDest.byarrData[iDestByte].ToString()+" ");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"creating mask", "MaskFromValue "
					+" (make sure source bitmap is 8-, 24-, or 32-bit) {"+Environment.NewLine
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
		public static bool InterpolatePixel(ref RImage riDest, ref RImage riSrc, int iDest, ref DPoint dpSrc) {
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
				dMaxX=(double)riSrc.iWidth-1.0d;
				dMaxY=(double)riSrc.iHeight-1.0d;
				//iDest=riDest.iStride*ipDest.Y+riDest.iBytesPP*ipDest.X;
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
					RMemory.CopyFastVoid(ref riDest.byarrData, ref riSrc.byarrData, iDest, iSrcRoundY*riSrc.iStride+iSrcRoundX*riSrc.iBytesPP, riDest.iBytesPP);
				}
				else {
					iDestNow=iDest;
					for (iQuad=0; iQuad<4; iQuad++) {
						iarrLocOfQuad[iQuad]=riSrc.iStride*(int)dparrQuad[iQuad].Y + riSrc.iBytesPP*(int)dparrQuad[iQuad].X;
					}
					for (iChan=0; iChan<riSrc.iBytesPP; iChan++, iTotal++) {
						dHeavyChannel=0;
						dWeightTotal=0;
						for (iQuad=0; iQuad<4; iQuad++) {
							dWeightNow=dDiagonalUnit-RMath.Dist(ref dpSrc, ref dparrQuad[iQuad]);
							dWeightTotal+=dWeightNow; //debug performance, this number is always the same theoretically
							dHeavyChannel+=(double)riSrc.byarrData[iarrLocOfQuad[iQuad]+iChan]*dWeightNow;
						}
						riDest.byarrData[iDestNow]=(byte)(dHeavyChannel/dWeightTotal);
						iDestNow++;
					}
				}
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"resampling image","RImage InterpolatePixel");
				bGood=false; //debug show error
			}
			return bGood;
		}//end InterpolatePixel
		/// <summary>
		/// Fakes motion blur.
		///   Using a byDecayTotal of 255 makes the blur trail fade to transparent.
		/// </summary>
		public static bool EffectMoBlurSimModWidth(ref RImage riDest, ref RImage riSrc, int xOffsetTotal, byte byDecayTotal) {
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
					riDest.iWidth=riSrc.iWidth+xLength;
					riDest.iBytesPP=riSrc.iBytesPP;
					riDest.iStride=riSrc.iStride;
					riDest.iHeight=riSrc.iHeight;
					riDest.iBytesTotal=riDest.iStride*riDest.iHeight;
					riDest.iPixelsTotal=riDest.iWidth*riDest.iHeight;
					if (riDest.byarrData==null || (riDest.byarrData.Length!=riDest.iBytesTotal))
						riDest.byarrData=new byte[riDest.iBytesTotal];
				}
				catch (Exception exn) {
					try {
						riDest=new RImage(riSrc.iWidth+xLength, riSrc.iHeight, riSrc.iBytesPP);
					}
					catch (Exception exn2) {
						RReporting.ShowExn(exn2,"trying to recover from previous exception by recreating object","EffectMoBlurSimModWidth { previous error: "+exn.ToString()+" }");
					}
				}
				//int iHeight2=riDest.iHeight;
				//int iWidth2=riDest.iWidth;
				int iHeight1=riSrc.iHeight;//TODO: eliminate this var and use riSrc.iHeight
				//int iWidth1=riSrc.iWidth;
				int iStride=riSrc.iStride;
				int iStride2=riDest.iStride;
				int iSrcByte=0;
				iDestByteStart=0;
				if (xDirection<0) {
					iDestByteStart=xLength;
				}
				int iDestByte=iDestByteStart;
				bool bTest=true;
				int yNow;
				for (yNow=0; yNow<iHeight1; yNow++) {
					bTest=RMemory.CopyFast(ref riDest.byarrData,
								 	ref riSrc.byarrData,
								  	iDestByte, iSrcByte, iStride);
					if (!bTest) {
						RReporting.ShowErr("Error precopying blur data","","RImage EffectMoBlurSimModWidth");
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
						bTest=RImage.EffectLightenOnly(ref riDest.byarrData,
							ref riSrc.byarrData,iDestByte, iSrcByte, iStride, fMultiplier);
						if (!bTest) {
							RReporting.ShowErr("Error overlaying blur data.","","RImage EffectMoBlurSimModWidth");
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
				RReporting.ShowExn(exn,"compositing blur data","RImage EffectMoBlurSimModWidth");
				bGood=false;
			}
			return bGood;
		}//EffectMoBlurSimModWidth
		public static bool EffectSkewModWidth(ref RImage riDest, ref RImage riSrc, int xOffsetBottom) {
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
					riDest.iWidth=riSrc.iWidth+((xOffsetBottom<0)?xOffsetBottom*-1:xOffsetBottom);
					riDest.iBytesPP=riSrc.iBytesPP;
					riDest.iStride=riSrc.iStride;
					riDest.iHeight=riSrc.iHeight;
					riDest.iBytesTotal=riDest.iStride*riDest.iHeight;
					riDest.iPixelsTotal=riDest.iWidth*riDest.iHeight;
					if (riDest.byarrData==null || (riDest.byarrData.Length!=riDest.iBytesTotal))
						riDest.byarrData=new byte[riDest.iBytesTotal];
				}
				catch (Exception exn) {
					try {
						riDest=new RImage(riSrc.iWidth+xOffsetBottom, riSrc.iHeight, riSrc.iBytesPP);
					}
					catch (Exception exn2) {
						RReporting.ShowExn(exn2,"trying to recover from previous exception by recreating destination","RImage EffectSkewModWidth { previous exception:"+exn.ToString()+" }");
					}
				}
				iSrcByte=0;
				iDestByte=0;//iDestByteStart;//TODO: Uncomment, and separate the blur code here and make alpha overlay version
				bool bTest=true;
				iDestLine=0;
				dpSrc=new DPoint();
				dpSrc.Y=0;
				dHeight=(double)riDest.iHeight;
				dWidthDest=(double)riDest.iWidth;
				//dWidthSrc=(double)riSrc.iWidth;
				dMaxY=dHeight-1.0d;
				iDestIndex=0;
				for (yNow=0; yNow<dHeight; yNow+=1.0d) {
					dpSrc.X=(yNow/dMaxY)*xAdd;
					if (xOffsetBottom<0) dpSrc.X=(xAdd-dpSrc.X);
					for (xNow=0; xNow<dWidthDest; xNow+=1.0d) {
						if (dpSrc.X>-1.0d) {
							if (dpSrc.X<dWidthDest)
								bTest=RImage.InterpolatePixel(ref riDest, ref riSrc, iDestIndex, ref dpSrc);
						}
						if (!bTest) {
							bGood=false;
							break;
						}
						iDestIndex+=riSrc.iBytesPP;
					}
					if (!bGood) break;
					//iDestLine+=riDest.iStride;
					dpSrc.Y+=1.0d;
				}
				if (!bGood) {
					RReporting.ShowErr("Error calculating skew data.","interpolating pixel","RImage EffectSkewModWidth");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"calculating skew data","RImage EffectSkewModWidth");
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
				RReporting.ShowExn(exn,"","RImage EffectLightenOnly");
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
				RReporting.ShowExn(exn,"","RImage EffectLightenOnly");
				return false;
			}
			return true;
		}
		#endregion editing
		
		#region Draw Overlay methods
		static bool bFirstRunNeedsToCropToFitInto=true;
		public bool NeedsToCropToFitInto(RImage riDest,int xDest, int yDest) {
			bool bReturn=true;
			if (bFirstRunNeedsToCropToFitInto) RReporting.DebugWrite("check NeedsToCropToFitInto...");
			if (iBytesTotal==0) {
				RReporting.ShowErr("Getting status on zero-length buffer!","","NeedsToCropToFitInto");
			}
			try {
				if (xDest>=0 && yDest>=0
					&& xDest+iWidth<=riDest.iWidth
					&& yDest+iHeight<=riDest.iHeight) bReturn=false;
			}
			catch {bReturn=true;}
			if (bFirstRunNeedsToCropToFitInto) RReporting.DebugWrite(((bReturn)?"yes...":"no..."));
			bFirstRunNeedsToCropToFitInto=false;
			return bReturn;
		}
		static bool bFirstCancellationOfDrawSmallerWithoutCropElseCancel=true;
		
		//public bool DrawToLargerWithoutCropElseCancel(RImage riDest, int xDest, int yDest, int iDrawMode) {
		//	try {
		//		return riDest.DrawSmallerWithoutCropElseCancel(xDest, yDest, this, iDrawMode);
		//	}
		//	catch {
		//	}
		//	return null;
		//}
		public bool DrawSmallerWithoutCropElseCancel(int xDest, int yDest, RImage riSrc) {
			return DrawSmallerWithoutCropElseCancel(xDest,yDest,riSrc,DrawModeCopyAlpha);
		}
		public bool CanFit(RImage riSrc, int xDest, int yDest) {
			bool bReturn=false;
			try {
				if (riSrc!=null) {
					bReturn=xDest>=0&&yDest>=0
						&&xDest+riSrc.Width<=Width
						&&yDest+riSrc.Height<=Height;
				}
			}
			catch {
				bReturn=false;
			}
			return bReturn;
		}
		public bool DrawSmallerWithoutCropElseCancel(int xDest, int yDest, RImage riSrc, int iDrawMode) {
			return DrawSmallerWithoutCropElseCancel(this, xDest, yDest, riSrc, iDrawMode);
		}
		///<summary>
		///Draws riSrc to riDest if riSrc is smaller AND doesn't need to be cropped.
		///uses RImage.brushFore for hue if riSrc is grayscale and riDest is RGB
		///</summary>
		public static bool DrawSmallerWithoutCropElseCancel(RImage riDest, int xDest, int yDest, RImage riSrc, int iDrawMode) {
			bool bGood=true;
			int x=0,y=0;
			if (riDest==null||riDest.byarrData==null) {
				RReporting.ShowErr("Tried to draw to null buffer!","","DrawSmallerWithoutCropElseCancel");
				return false;
			}
			else if (riSrc==null||riSrc.byarrData==null) {
				RReporting.ShowErr("Tried to draw null buffer!","","DrawSmallerWithoutCropElseCancel");
				return false;
			}
			try {
				byte[] riDest_byarrData=riDest.byarrData;
				byte[] riSrc_byarrData=riSrc.byarrData;
				//int riSrc_iBytesPP=riSrc.iBytesPP;
				//int riDest_iBytesPP=riDest.iBytesPP;
				if (!riDest.CanFit(riSrc,xDest,yDest)) {//if (riSrc.NeedsToCropToFitInto(this,xDest,yDest)) {
					if (bFirstCancellationOfDrawSmallerWithoutCropElseCancel) {
						RReporting.DebugWrite("failed since not in bounds("+riSrc.iWidth.ToString()+"x"+riSrc.iHeight.ToString()+" to "+riDest.iWidth.ToString()+"x"+riDest.iHeight.ToString()+" at ("+xDest.ToString()+","+yDest.ToString()+") )...");
						bFirstCancellationOfDrawSmallerWithoutCropElseCancel=false;
					}
					RReporting.Warning("Cancelling DrawSmallerWithoutCropElseCancel {xDest:"+xDest.ToString()+"; yDest:"+yDest.ToString()+"; riSrc:"+riSrc.Description()+"; riDest:"+riDest.Description()+"}");
					bGood=false;
				}
				if (bGood) {
					float fCookedAlpha;
					int iLocDestLine=yDest*riDest.iStride + xDest*riDest.iBytesPP;
					int iDestPix;
					int iLocSrcLine=0;
					int iSrcPix;
					int iSrcBytesPP=riSrc.iBytesPP;
					int iDestBytesPP=riDest.iBytesPP;
					int iMinBytesPP=iSrcBytesPP<iDestBytesPP?iSrcBytesPP:iDestBytesPP;
					
					//byte* lpDestLine=&riDest_byarrData[yDest*riDest.iStride+xDest*riDest.iBytesPP];
					//byte* lpDestPix;
					//byte* lpSrcLine=riSrc_byarrData;
					//byte* lpSrcPix;
					int iStrideMin=(riSrc.iStride<riDest.iStride)?riSrc.iStride:riDest.iStride;
					if (riDest.iBytesPP==4 && riSrc.iBytesPP==4) {
						switch (iDrawMode) {
						case DrawModeCopyAlpha:
							//if (riDest.iStride==riSrc.iStride && riDest.iBytesPP==iBytesPP && xDest==0 && yDest==0) {
							if (riSrc.IsLike(riDest) && xDest==0 && yDest==0) {
								RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, 0, 0, riSrc.iBytesTotal);
							}
							else {
								for (y=0; y<riSrc.iHeight; y++) {
									RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iLocDestLine, iLocSrcLine, iStrideMin);
									iLocDestLine+=riDest.iStride;//lpDestLine+=riDest.iStride;
									iLocSrcLine+=riSrc.iStride;//lpSrcLine+=riSrc.iStride;
								}
							}
							break;
						case DrawModeAlpha:
							//alpha result: ((Source-Dest)*alpha/255+Dest)
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix+3]==0) {
										iSrcPix+=4; iDestPix+=4;//iSrcPix+=riSrc.iBytesPP; iDestPix+=riDest.iBytesPP;//assumes 32-bit
									}
									else if (riSrc_byarrData[iSrcPix+3]==255) {
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData,iDestPix, iSrcPix, 3);
										iSrcPix+=4; iDestPix+=4;//iSrcPix+=riSrc.iBytesPP; iDestPix+=riDest.iBytesPP;//assumes 32-bit
									}
									else {
										fCookedAlpha=(float)riSrc_byarrData[iSrcPix+3]/255.0f;
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(riSrc_byarrData[iSrcPix]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //B
										iSrcPix++; iDestPix++;
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(riSrc_byarrData[iSrcPix]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //G
										iSrcPix++; iDestPix++;
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(riSrc_byarrData[iSrcPix]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //R
										iSrcPix+=2; iDestPix+=2;//assumes 32-bit
									}
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						case DrawModeAlphaQuickEdge:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix+3]<=85) {
										iSrcPix+=4; iDestPix+=4;//assumes 32-bit
									}
									else if (riSrc_byarrData[iSrcPix+3]>170) {
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData, iDestPix, iSrcPix, 3);
										iSrcPix+=4; iDestPix+=4;//assumes 32-bit
									}
									else {
										riDest_byarrData[iDestPix]=(byte)( (riSrc_byarrData[iSrcPix]>>2) + (riDest_byarrData[iDestPix]>>2) ); //B
										iSrcPix++; iDestPix++;
										riDest_byarrData[iDestPix]=(byte)( (riSrc_byarrData[iSrcPix]>>2) + (riDest_byarrData[iDestPix]>>2) ); //G
										iSrcPix++; iDestPix++;
										riDest_byarrData[iDestPix]=(byte)( (riSrc_byarrData[iSrcPix]>>2) + (riDest_byarrData[iDestPix]>>2) ); //R
										iSrcPix+=2; iDestPix+=2;//assumes 32-bit
									}
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						case DrawModeAlphaHardEdge:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix+3]>=128) {
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData, iDestPix, iSrcPix, 3);
									}
									iSrcPix+=4; iDestPix+=4;
								}//end for x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
								Console.Error.Write("-");//debug only
							}//end for y
							break;
						case DrawModeKeepGreaterAlpha:
							//alpha result: ((Source-Dest)*alpha/255+Dest)
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									riDest_byarrData[iDestPix]=(riSrc_byarrData[iSrcPix]>riDest_byarrData[iDestPix])?riSrc_byarrData[iSrcPix]:riDest_byarrData[iDestPix]; //B
									iSrcPix++; iDestPix++;
									
									riDest_byarrData[iDestPix]=(riSrc_byarrData[iSrcPix]>riDest_byarrData[iDestPix])?riSrc_byarrData[iSrcPix]:riDest_byarrData[iDestPix]; //G
									iSrcPix++; iDestPix++;
									
									riDest_byarrData[iDestPix]=(riSrc_byarrData[iSrcPix]>riDest_byarrData[iDestPix])?riSrc_byarrData[iSrcPix]:riDest_byarrData[iDestPix]; //R
									iSrcPix+=2; iDestPix+=2; //assumes 32-bit
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						case DrawModeKeepDestAlpha:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData,iDestPix,iSrcPix,3);
									iDestPix+=4; iSrcPix+=4; //assumes 32-bit
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						}//end switch
					}//end 32-bit to 32-bit
					else if (riSrc.iBytesPP==1&&riDest.iBytesPP==4) {
						//8-bit to 32-bit
						switch (iDrawMode) {
						case DrawModeCopyAlpha:
						//if (riDest.iStride==riSrc.iStride && riDest.iBytesPP==iBytesPP && xDest==0 && yDest==0) {
							for (y=0; y<riSrc.iHeight; y++) {
								int iLocDest=iLocDestLine;
								int iLocSrc=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									RMemory.CopyFast(ref riDest_byarrData, ref brushFore.data32, iLocDest, 0, 3);
									riDest_byarrData[iDestBytesPP+3]=riSrc_byarrData[iLocSrc];//assumes 32-bit; assumes 8-bit grayscale source
									iLocDest+=iDestBytesPP;
									iLocSrc+=iSrcBytesPP;
								}
								iLocDestLine+=riDest.iStride;//lpDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;//lpSrcLine+=riSrc.iStride;
							}
							break;
						case DrawModeAlpha:
								//alpha result: ((Source-Dest)*alpha/255+Dest)
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix]==0) {//assumes 8-bit
										iSrcPix+=iSrcBytesPP; iDestPix+=iDestBytesPP;//iSrcPix+=riSrc.iBytesPP; iDestPix+=riDest.iBytesPP;//assumes 32-bit
									}
									else if (riSrc_byarrData[iSrcPix]==255) {//assumes 8-bit
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData,iDestPix, iSrcPix, 3);
										iSrcPix+=iSrcBytesPP; iDestPix+=iDestBytesPP;//iSrcPix+=riSrc.iBytesPP; iDestPix+=riDest.iBytesPP;//assumes 32-bit
									}
									else {
										fCookedAlpha=(float)riSrc_byarrData[iSrcPix]/255.0f;
										
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(brushFore.data32[0]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //B
										iDestPix++;
										//iSrcPix++; //being commented assumes grayscale
										
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(brushFore.data32[1]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //G
										iDestPix++; 
										//iSrcPix++;  //being commented assumes grayscale
										
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(brushFore.data32[2]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //R
										iSrcPix+=iSrcBytesPP;//assumes not incremented during copy above
										iDestPix+=2; //assumes 32-bit
									}//end else neither 0 nor 255
								}//end x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end y
							break;
						case DrawModeAlphaQuickEdge:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix]<=85) {//assumes 8-bit
										iSrcPix+=iSrcBytesPP; iDestPix+=iDestBytesPP;//assumes 32-bit
									}
									else if (riSrc_byarrData[iSrcPix]>170) {//assumes 8-bit
										RMemory.CopyFast(ref riDest_byarrData,ref brushFore.data32, iDestPix, 0, 3);
										iSrcPix+=iSrcBytesPP; iDestPix+=iDestBytesPP;//assumes 32-bit
									}
									else {
										riDest_byarrData[iDestPix]=(byte)( (brushFore.data32[0]>>2) + (riDest_byarrData[iDestPix]>>2) ); //B //copy from brush assumes 8-bit source
										//iSrcPix++;//commented assumes 8-bit source
										iDestPix++;
										riDest_byarrData[iDestPix]=(byte)( (brushFore.data32[1]>>2) + (riDest_byarrData[iDestPix]>>2) ); //G //copy from brush assumes 8-bit source
										//iSrcPix++;//commented assumes 8-bit source
										iDestPix++;
										riDest_byarrData[iDestPix]=(byte)( (brushFore.data32[2]>>2) + (riDest_byarrData[iDestPix]>>2) ); //R //copy from brush assumes 8-bit source
										iSrcPix+=iSrcBytesPP;//assumes not incremented during copy above
										iDestPix+=2;//assumes 32-bit
									}
								}//end for x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end for y
							break;
						case DrawModeAlphaHardEdge:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix]>=128) {
										RMemory.CopyFast(ref riDest_byarrData,ref brushFore.data32, iDestPix, 0, iDestBytesPP);//copy from brushFore.data32 0 assumes 8-bit source
									}
									iSrcPix+=iSrcBytesPP; iDestPix+=iDestBytesPP;
								}//end for x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end for y
							break;
						case DrawModeKeepGreaterAlpha:
								//alpha result: ((Source-Dest)*alpha/255+Dest)
							byte bySrc;
							byte byDest;
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix]==0) {//assumes 8-bit
										iSrcPix++;//assumes 8-bit
										iDestPix+=iDestBytesPP;//iSrcPix+=riSrc.iBytesPP; iDestPix+=riDest.iBytesPP;//assumes 32-bit
									}
									else if (riSrc_byarrData[iSrcPix]==255) {//assumes 8-bit
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData,iDestPix, iSrcPix, 3);
										riDest_byarrData[iDestPix+3]=255;
										iSrcPix++;//assumes 8-bit
										iDestPix+=iDestBytesPP;//assumes 32-bit
									}
									else {
										fCookedAlpha=(float)riSrc_byarrData[iSrcPix]/255.0f;
										
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(brushFore.data32[0]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //B
										iDestPix++;
										//iSrcPix++; //being commented assumes grayscale
										
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(brushFore.data32[1]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //G
										iDestPix++; 
										//iSrcPix++;  //being commented assumes grayscale
										
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(brushFore.data32[2]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //R
										iDestPix++;
										
										bySrc=riSrc_byarrData[iSrcPix];
										byDest=riDest_byarrData[iDestPix];
										riDest_byarrData[iDestPix]=byDest>bySrc?byDest:bySrc;
										iSrcPix++;//assumes 8-bit
										iDestPix++; //assumes 32-bit
									}//end else neither 0 nor 255
								}//end x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end y
							break;
						case DrawModeKeepDestAlpha:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									RMemory.CopyFast(ref riDest_byarrData, ref brushFore.data32,iDestPix,0,3);//copy from brushFore.data32 0 assumes 8-bit source
									iDestPix+=4; //assumes 32-bit
									iSrcPix++; //assumes 8-bit
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						}//end switch
					}//end 8-bit to 32-bit
					else {
						RReporting.ShowErr("Can't Draw unless both images are 32-bit BGRA, or source is 8-bit and dest is 32-bit.  This method is designed for fast copying between images with similar color formats.","","RImage DrawSmallerWithoutCropElseCancel {"+"riDest.iBytesPP:"+riDest.iBytesPP.ToString()+ "; iBytesPP:"+riSrc.iBytesPP.ToString()+"}");
					}
				}//end if does not need to crop
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"","RImage DrawSmallerWithoutCropElseCancel");
			}
			return bGood;
		}//end DrawSmallerWithoutCropElseCancel
		#endregion Draw Overlay methods
		#region Draw methods
		public bool InvertPixel(int x, int y) {
			bool bGood=false;
			try {
				int iDestNow=XYToLocation(x,y);
				for (int iChan=0; iChan<iBytesPP; iChan++) {
					if (byarrData[iDestNow]==255&&RMath.SubstractBytes[255][(int)byarrData[iDestNow]]!=0) Console.Error.WriteLine("SubtractBytes array is incorrect");//debug only
					byarrData[iDestNow]=RMath.SubtractBytes[255][(int)byarrData[iDestNow]];
					iDestNow++;
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				RReporting.ShowExn(exn,"inverting pixel",String.Format("InvertPixel({0},{1})",x,y));
			}
			return bGood;
		}
		public bool SetPixel(int x, int y, Color colorNow) {
			return SetPixelArgb(x,y,colorNow.A,colorNow.R,colorNow.G,colorNow.B);
		}
		public bool SetPixelRgb(int x, int y, byte r, byte g, byte b) {
			return SetPixelArgb(x,y,255,r,g,b);
		}
		public bool SetPixelRgb_IgnoreAlpha(int x, int y, byte r, byte g, byte b) {
			bool bGood=false;
			try {
				if (iBytesPP>=3) {
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow]=r;
				}
				else {
					byarrData[XYToLocation(x,y)]=RMath.SafeAverage(r,g,b);
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				RReporting.ShowExn( exn, "drawing pixel", String.Format("SetPixelArgb({0},{1},...)",x,y) );
			}
			return bGood;
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
				else RReporting.ShowErr("Wrong bit depth","drawing pixel","SetPixelArgb() {destination:"+iBytesPP.ToString()+"-bit}");
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				RReporting.ShowExn( exn, "drawing pixel", String.Format("SetPixelArgb({0},{1},...)",x,y) );
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
				else RReporting.ShowErr("Wrong bit depth","drawing to channel", String.Format("SetPixelR(...){{destination:{0}-bit}}",iBytesPP) );
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				RReporting.ShowExn(exn,"drawing to channel",String.Format("({0},{1},r)",x,y));
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
				else RReporting.ShowErr("Wrong bit depth","drawing to channel", String.Format("SetPixelG(...){{destination:{0}-bit}}",iBytesPP) );
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				RReporting.ShowExn(exn, "drawing to channel", String.Format("SetPixelG({0},{1},...)",x,y));
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
				else RReporting.ShowErr("Wrong bit depth","drawing to channel", String.Format("SetPixelB(...){{destination:{0}-bit}}",iBytesPP) );
				bGood=true;
			}
			catch (Exception exn) {
				bGood=true;
				RReporting.ShowExn( exn, "drawing to channel", String.Format("SetPixelB({0},{1},...)",x,y) );
			}
			return bGood;
		}//end SetPixelB
		public bool SetPixelHsva(int x, int y, double h, double s, double v, double a0To1) {
			bool bGood=false;
			try {
				if (iBytesPP==4) {
					byte r,g,b;
					//NOTE: Hsv is NOT H<360 it is H<1.0
					RConvert.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow++]=r;
					byarrData[iDestNow]=(byte)(a0To1*255.0);
				}
				else if (iBytesPP==3) {
					byte r,g,b;
					RConvert.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow]=r;
				}
				else if (iBytesPP==1) {
					byarrData[XYToLocation(x,y)]=(byte)(v*255.0);
				}
				else RReporting.ShowErr("Wrong bit depth","drawing from HSV values", String.Format("DrawPixelHSV(...){{destination:{0}-bit}}",iBytesPP) );
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"",String.Format("DrawPixelHSV({0},{1},...)",x,y));
			}
			return bGood;
		}//end SetPixelHsva
		public bool SetPixelHsva(int x, int y, float h, float s, float v, float a0To1) {
			bool bGood=false;
			try {
				if (iBytesPP==4) {
					byte r,g,b;
					//NOTE: Hsv is NOT H<360 it is H<1.0
					RConvert.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow++]=r;
					byarrData[iDestNow]=(byte)(a0To1*255.0);
				}
				else if (iBytesPP==3) {
					byte r,g,b;
					RConvert.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow]=r;
				}
				else if (iBytesPP==1) {
					byarrData[XYToLocation(x,y)]=(byte)(v*255.0);
				}
				else RReporting.ShowErr("Wrong bit depth","drawing from HSV values", String.Format("SetPixelR(...){{destination:{0}-bit}}",iBytesPP) );
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"",String.Format("DrawPixelHSV({0},{1},...)",x,y));
			}
			return bGood;
		}//end SetPixelHsva
		public bool DrawRectStyleCropped(Color colorNow, IRect rectDest) {
			try {
				if (rectDest!=null) return DrawRectStyleCropped(colorNow,rectDest.Left, rectDest.Top, rectDest.Width, rectDest.Height);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return false;
		}
		public RBrush brushTemp=new RBrush();
		public bool DrawRectStyleCropped(RBrush rbX, int rect_Top, int rect_Left, int rect_Width, int rect_Height) {
			RBrush brushForeOrig=brushFore;
			//TODO: UseDrawRectBorderSym (...only for equally thick borders)
			//TODO: CornerRadius
			//rbX was formerly colorNow
			if (rbX==null) rbX=brushFore.Copy();
			brushFore=brushTemp;//rbX.Copy();//TODO: debug performance
			brushFore.Set(rbX);//NOTE: this allows modifying the temporary brushFore while still referring to the original rbX parameter
			bool bGood=false;
			if (brushFore.A==0) {
				brushFore.SetRgb_IgnoreAlpha(128,128,128);
			}
			if (rect_Left<0) {
				rect_Width+=rect_Left;//actually subtracts
				rect_Left=0;
			}
			if (rect_Top<0) {
				rect_Height+=rect_Top;//actually subtracts
				rect_Top=0;
			}
			if (rect_Top+rect_Height>iHeight) rect_Height-=(rect_Top+rectHeight)-iHeight;
			if (rect_Left+rect_Width>iWidth) rect_Width-=(rect_Left+rect_Width)-iWidth;
			if (rect_Width>0&&rect_Height>0) {//if(rect_Left<iWidth&&rect_Left>=0&&rect_Height>=0&&rect_Top+rect_Height-1<rect_Height) 
				if (brushFore.A!=0) {
					DrawRectFilled(rect_Top,rect_Left,rect_Width,rect_Height);
				}
				//TODO: DrawRectStyle - vectorize and make color alpha polygons
				//dark:
				brushFore.SetRgb(SubtractBytes[rbX.R][85],SubtractBytes[rbX.G][85],SubtractBytes[rbX.B][85]);
				DrawHorzLine(rect_Left, rect_Top+rect_Height-1, rect_Width, "DrawRectStyle(){part:B}");
				DrawVertLine(rect_Left, rect_Top, rect_Height, "DrawRectStyle(){part:L}");//if (rect_Left>=0&&rect_Left<iWidth&&rect_Top>=0&&rectTop<iHeight) 
				//light:
				brushFore.SetRgb(AddBytes[rbX.R][85],AddBytes[rbX.G][85],AddBytes[rbX.B][85]);
				DrawHorzLine(rect_Left, rect_Top, rect_Width, "DrawRectStyle(){part:T}");//if (rect_Left<iWidth&&rect_Left>=0&&rect_Height>0)
				DrawVertLine(rect_Left+rect_Width-1,rect_Top,rect_Height,"DrawRectStyle(){part:R}");//if (rect_Left+rect_Width-1<iWidth&&rect_Top>=0&&rect_Width>0)
				bGood=true;
			}
			brushFore=brushForeOrig;
			return bGood;
		}//end DrawRectStyle
		public bool DrawRectCropped(IZone zoneNow) {
			return DrawRectCropped(zoneNow.Left,zoneNow.Top,zoneNow.Width,zoneNow.Height);
		}
		public bool DrawRectCropped(IRect rectNow) {
			return DrawRectCropped(rectNow.X,rectNow.Y,rectNow.Width,rectNow.Height);
		}
		public bool DrawRectCropped(int xDest, int yDest, int iWidth, int iHeight) {
			RMath.CropRect(ref xDest, ref yDest, ref iWidth, ref iHeight, 0, 0, Width, Height);
			if (iWidth>0&&iHeight>0) return DrawRect(xDest,yDest,iWidth,iHeight,"DrawCroppedRect");
			else return true;
		}
		public bool DrawRect(int xDest, int yDest, int iWidth, int iHeight, string sSender_ForErrorTracking) {
			
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
				RReporting.Warning(sSender_ForErrorTracking+" skipped drawing out-of-range rect. {rect:"+IRect.ToString(xDest,yDest,iWidth,iHeight)+"}");
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
		public bool DrawRectFilled(int xDest, int yDest, int iWidth, int iHeight, string sSender_ForErrorTracking) {
			return DrawRectFilled(brushFore,xDest, yDest, iWidth, iHeight, sSender_ForErrorTracking);
		}
		public unsafe bool DrawRectFilled(RBrush brushX, int xDest, int yDest, int iWidth, int iHeight, string sSender_ForErrorTracking) {
			//TODO: implement overlay modes here
			if ((iWidth<1)||(iHeight<1)) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=brushX.data32Copied64) { //keeps GC at bay
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
				RReporting.ShowExn(exn, "drawing filled rectangle", String.Format("RImage DrawRectFilled(x:{0},y:{1},width:{2},height:{3},sender:{4})",xDest,yDest,iWidth,iHeight,sSender_ForErrorTracking) );
			}
			return bGood;
		} //DrawRectFilled
		///<summary>
		///Draws a gradient from top to bottom from Foreground to Background color
		///</summary>
		public bool DrawGradTopDownRectFilled(RBrush brushTop, RBrush brushBottom, int xDest, int yDest, int iWidth, int iHeight, string sSender_ForErrorTracking) {
			//TODO: implement overlay modes here
			if ((iWidth<1)||(iHeight<1)) return false;
			bool bGood=true;
			try {
				int iLineStart=yDest*iStride+xDest*iBytesPP;
				int iFullApproach=iHeight-1;
				byte byApproachNow;
				brushTemp.A=255;
				for (int yNow=0; yNow<iHeight; yNow++) {
					//int iDest=iLineStart;
					byApproachNow=(byte)RMath.SafeDivideRound(RMath.SafeMultiply(yNow,255),iFullApproach);
					brushTemp.B=RMath.AlphaLook[brushBottom.B][brushTop.B][byApproachNow];
					brushTemp.G=RMath.AlphaLook[brushBottom.G][brushTop.G][byApproachNow];
					brushTemp.R=RMath.AlphaLook[brushBottom.R][brushTop.R][byApproachNow];
					DrawHorzLine(brushTemp,xDest,yNow,iWidth,sSender_ForErrorTracking);
				}//end for yNow
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn, "drawing filled rectangle", String.Format("RImage DrawGradTopDownRectFilled(x:{0},y:{1},width:{2},height:{3},sender:{4})",xDest,yDest,iWidth,iHeight,sSender_ForErrorTracking) );
			}
			return bGood;
		} //DrawGradTopDownRectFilled
		public bool DrawRectFilledCropped(int xDest, int yDest, int iWidth, int iHeight) {
			return DrawRectFilledCropped(brushFore,xDest,yDest,iWidth,iHeight);
		}
		public bool DrawRectFilledCropped(RBrush brushX, int xDest, int yDest, int iWidth, int iHeight) {
			//formerly DrawRectCroppedFilled
			RMath.CropRect(ref xDest, ref yDest, ref iWidth, ref iHeight, 0, 0, Width, Height);
			if (iWidth>0&&iHeight>0) return DrawRectFilled(brushX,xDest,yDest,iWidth,iHeight,"DrawRectFilledCropped");
			else return true;
		}
		public bool DrawRectFilled(IRect rectRect, string sSender_ForErrorTracking) {
			return DrawRectFilled(brushFore,rectRect,sSender_ForErrorTracking);
		}
		public bool DrawRectFilled(RBrush brushX, IRect rectRect, string sSender_ForErrorTracking) {
			return DrawRectFilled(brushX, rectRect.X, rectRect.Y, rectRect.Width, rectRect.Height, sSender_ForErrorTracking);
		}
		//public bool DrawRectFilledHsva(IRect rectDest, float h, float s, float v, float a) {
		//	return DrawRectFilledHsva(rectDest.X,rectDest.Y,rectDest.Width,rectDest.Height,h,s,v,a);
		//}
		//public bool DrawRectFilledHsva(IRect rectDest, double h, double s, double v, double a) {
		//	return DrawRectFilledHsva(rectDest.X,rectDest.Y,rectDest.Width,rectDest.Height,h,s,v,a);
		//}
		//public bool DrawRectFilledHsva(int xDest, int yDest, int iWidth, int iHeight, float h, float s, float v, float a) {
		//	brushFore.SetHsva(h,s,v,a);
		//	return DrawRectFilled(xDest,yDest,iWidth,iHeight);
		//}
		//public bool DrawRectFilledHsva(int xDest, int yDest, int iWidth, int iHeight, double h, double s, double v, double a) {
		//	brushFore.SetHsva(h,s,v,a);
		//	return DrawRectFilled(xDest,yDest,iWidth,iHeight);
		//}
		//public bool DrawRectFilledCroppedHsva(int xDest, int yDest, int iWidth, int iHeight, double h, double s, double v, double a) {
		//	brushFore.SetHsva(h,s,v,a);
		//	return DrawRectFilledCropped(xDest,yDest,iWidth,iHeight);
		//}
		/// <summary>
		/// DrawRectBorder horizontally and vertically symmetrical
		/// </summary>
		/// <param name="rectRect"></param>
		/// <param name="rectHole"></param>
		/// <returns></returns>
		public bool DrawRectBorderSym(RBrush brushX, IRect rectRect, IRect rectHole, string sSender_ForErrorTracking) {
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
				bool bTest=DrawRectFilled(brushX, xNow, yNow, iWidthNow, iHeightNow, sSender_ForErrorTracking);//Top full width
				if (!bTest) bGood=false;
				yNow+=rectHole.Height+iHeightNow;
				//would need to change iHeightNow here if asymmetrical
				bTest=DrawRectFilled(brushX, xNow, yNow, iWidthNow, iHeightNow, sSender_ForErrorTracking);//Bottom full width
				if (!bTest) bGood=false;
				yNow-=rectHole.Height;
				iWidthNow=rectHole.X-rectRect.X;
				iHeightNow=rectHole.Height;
				bTest=DrawRectFilled(brushX, xNow, yNow, iWidthNow, iHeightNow, sSender_ForErrorTracking);//Left remaining height
				if (!bTest) bGood=false;
				xNow+=rectHole.Width+iWidthNow;
				//would need to change iWidthNow here if asymmetrical
				bTest=DrawRectFilled(brushX, xNow, yNow, iWidthNow, iHeightNow, sSender_ForErrorTracking);//Right remaining height
				if (!bTest) bGood=false;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"","RImage DrawRectBorderSym");
			}
			return bGood;
		} //end DrawRectBorderSym
		private IRect rectOuterTemp=new IRect();
		private IRect rectInnerTemp=new IRect();
		public bool DrawRectBorder(int xDest, int yDest, int iWidth, int iHeight, int iThick) {
			rectOuterTemp.Set(xDest,yDest,iWidth,iHeight);
			rectInnerTemp.Set(xDest+iThick,yDest+iThick,iWidth-(iThick*2),iHeight-(iThick*2));
			if ((rectInnerTemp.Width<=1) || (rectInnerTemp.Height<=1)) {
				return DrawRectFilled(rectOuterTemp,"DrawRectBorder");
			}
			else return DrawRectBorderSym(rectOuterTemp, rectInnerTemp, "DrawRectBorder");
		}//DrawRectBorder
		//public unsafe bool DrawVertLine(int xDest, int yDest, int iPixelCopies, string sSender_ForErrorTracking) {
		//	return DrawVertLine(xDest, yDest, iPixelCopies, false, sSender_ForErrorTracking);
		//}
		//public unsafe bool DrawVertLine(int xDest, int yDest, int iPixelCopies, bool bBackColor, string sSender_ForErrorTracking) {
		//	return DrawVertLine(bBackColor?brushBack:brushFore, xDest,yDest,iPixelCopies,sSender_ForErrorTracking);
		//}
		public unsafe bool DrawVertLine(RBrush brushX, int xDest, int yDest, int iPixelCopies, string sSender_ForErrorTracking) {
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=brushX.data32) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"drawing line",String.Format("RImage DrawVertLine(x:{0},y:{1},copies:{2},sender:{3})", xDest, yDest, iPixelCopies, sSender_ForErrorTracking) );
					
			}
			return bGood;
		}//DrawVertLine
		//public unsafe bool DrawHorzLine(int xDest, int yDest, int iPixelCopies, string sSender_ForErrorTracking) {
		//	return DrawHorzLine(xDest, yDest, iPixelCopies, false, sSender_ForErrorTracking);
		//}
		//public unsafe bool DrawHorzLine(int xDest, int yDest, int iPixelCopies, bool bBackColor, string sSender_ForErrorTracking) {
		//	return DrawHorzLine(bBackColor?brushBack:brushFore, xDest,yDest,iPixelCopies,sSender_ForErrorTracking);
		//}
		public unsafe bool DrawHorzLine(RBrush brushX, int xDest, int yDest, int iPixelCopies, string sSender_ForErrorTracking) {
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=brushX.data32Copied64) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"drawing line",String.Format("RImage DrawHorzLine(x:{0},y:{1},copies:{2},sender:{3})", xDest, yDest, iPixelCopies, sSender_ForErrorTracking) );
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
				RReporting.ShowExn(exn,"","RImage DrawFractal");
			}
		}
		*/
		#region advanced graphics 
///TODO: try these in Test.cs
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
						this.SetPixelHsva(xDest,yDest,RConvert.ROFXY(xSrc,ySrc)/360.0,1.0,1.0,1.0);
						xSrc+=rScale;
					}
					ySrc+=rScale;
				}
				bGood=true;
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"","DrawRainbowBurst");
			}
			return bGood;
		}//end DrawRainbowBurst
		//TODO: try this in Test.cs
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
						this.SetPixelHsva(xDest,yDest,RConvert.ROFXY(xSrc,ySrc),1.0,.5,1.0);
						xSrc+=rScale;
					}
					ySrc+=rScale;
				}
				bGood=true;
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"","DrawWavyThing");
			}
			return bGood;
		}//end DrawWavyThing
		
		void DrawAlphaPix(int xPix, int yPix, byte r, byte g, byte b, byte a) {
			try {
				int iChannel=yPix*iStride+xPix*iBytesPP;
				//The ++ operators are right:
				//float fAlphaTo1;
				//if ((iChannel+2>=0) && (iChannel+2<iPixelsTotal))
				if ((iChannel+2)<iBytesTotal) {//+2 assumes only touching first 3 channels //if (((iChannel+3)/4)<iPixelsTotal) { //fAlphaTo1=(float)a/255.0f;
					byarrData[iChannel]=RMath.AlphaLook[b][byarrData[iChannel]][a];//(byte)RMath.Approach((float)byarrData[iChannel], (float)b, fAlphaTo1);
					byarrData[++iChannel]=RMath.AlphaLook[g][byarrData[iChannel]][a];//(byte)RMath.Approach((float)byarrData[iChannel], (float)g, fAlphaTo1);
					byarrData[++iChannel]=RMath.AlphaLook[r][byarrData[iChannel]][a];//(byte)RMath.Approach((float)byarrData[iChannel], (float)r, fAlphaTo1);
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing transparent pixel using 4 channel values","DrawAlphaPix");
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
				xNormal=1.0f-RMath.SafeAbs(xDot-xfMin);
				xEccentric=1.0f-RMath.SafeAbs(xDot-xfMax);
				yNormal=1.0f-RMath.SafeAbs(yDot-yfMin);
				yEccentric=1.0f-RMath.SafeAbs(yDot-yfMax);
				DrawAlphaPix(xMin,yMin,pixelColor.R,pixelColor.G,pixelColor.B,RMath.ByRound(pixelColor.A*xNormal*yNormal));
				DrawAlphaPix(xMax,yMin,pixelColor.R,pixelColor.G,pixelColor.B,RMath.ByRound(pixelColor.A*xEccentric*yNormal));
				DrawAlphaPix(xMin,yMax,pixelColor.R,pixelColor.G,pixelColor.B,RMath.ByRound(pixelColor.A*xNormal*yEccentric));
				DrawAlphaPix(xMax,yMax,pixelColor.R,pixelColor.G,pixelColor.B,RMath.ByRound(pixelColor.A*xEccentric*yEccentric));
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing pixel to Vector","DrawVectorDot");
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
				RReporting.ShowExn(exn,"drawing pixel data to Vector","DrawVectorDot");
			}
		}//end DrawVectorDot(float xDot, float yDot, byte* lpbySrcPixel)
		public void DrawVectorLine(ILine line1, Pixel32 pixelStart, Pixel32 pixelEndOrNull, float fSubpixelPrecisionIncrement) {
			DrawVectorLine(RConvert.ToFloat(line1.X1), RConvert.ToFloat(line1.Y1), RConvert.ToFloat(line1.X2), RConvert.ToFloat(line1.Y2), pixelStart, pixelEndOrNull, fSubpixelPrecisionIncrement);
		}
		#endregion advanced graphics

// 		public byte BrushR() {
// 			return brushFore.R;
// 		}
// 		public byte BrushG() {
// 			return brushFore.G;
// 		}
// 		public byte BrushB() {
// 			return brushFore.B;
// 		}
		public bool DrawRectFilledSafe(int xDest, int yDest, int iDestW, int iDestH) {
			bool bGood=false;
			if (byarrData!=null) {
				try {
					int xAbs=xDest;
					int yAbs=yDest;
					int xRel=0;
					//int yRel=0;
					int yDestEnder=yDest+iDestH;
					byte byR=brushFore.R;
					byte byG=brushFore.G;
					byte byB=brushFore.B;
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
									RReporting.Warning("In-range pixel with out-of-range result in DrawRectFilledSafe");
									break;
								}
							}
							else {
								RReporting.Warning("Out-of-range pixel in DrawRectFilledSafe {location:"+IPoint.ToString(xAbs,yAbs)+"; start-location:"+IPoint.ToString(xDest,yDest)+"; total-size:"+IPoint.ToString(iDestW,iDestH)+"; destination-size:"+IPoint.ToString(iWidth,iHeight)+"}");
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
						if (iDestNow==XYToLocation(xDest,yDest)) RReporting.Warning("DrawRectFilledSafe skipped out-of-range rect {location:"+IPoint.ToString(xAbs,yAbs)+"; start-location:"+IPoint.ToString(xDest,yDest)+"; total-size:"+IPoint.ToString(iDestW,iDestH)+"; destination-size:"+IPoint.ToString(iWidth,iHeight)+"}");
						else if (yDest>=yDestEnder) {
							RReporting.Warning("DrawRectFilledSafe skipped 'y' out-of-range rect {location:"+IPoint.ToString(xAbs,yAbs)+"; start-location:"+IPoint.ToString(xDest,yDest)+"; total-size:"+IPoint.ToString(iDestW,iDestH)+"; destination-size:"+IPoint.ToString(iWidth,iHeight)+"; yDestEnder:"+yDestEnder.ToString()+"}");
						}
						else if (!bGood) {
							RReporting.Warning("DrawRectFilledSafe skipped rect for unknown reason. {location:"+IPoint.ToString(xAbs,yAbs)+"; start-location:"+IPoint.ToString(xDest,yDest)+"; total-size:"+IPoint.ToString(iDestW,iDestH)+"; destination-size:"+IPoint.ToString(iWidth,iHeight)+"}");
						}
					}
					else RReporting.Warning("Negative rect skipped in DrawRectFilledSafe.");
				}
				catch {//(Exception exn) {
					RReporting.Warning("DrawRectFilledSafe failed.");
				}
			}
			return bGood;
		}//end DrawRectFilledSafe
		public void DrawVectorLine(float xStart, float yStart, float xEnd, float yEnd,
				Pixel32 pixelStart, Pixel32 pixelEndOrNull, float fSubpixelPrecisionIncrement) {
			int iLoops=0;
			int iStartTick=RPlatform.TickCount;
			int iMaxTicks=50;//i.e. 20 per second minimum (soft minimum because of loop limit below)
			int iMaxLoops=1000000; //debug hard-coded limitation
			float xNow, yNow, xRelMax, yRelMax, rRelMax, theta, rRel;
			bool bLimited=false;
			xNow=xStart;
			yNow=yStart;
			xRelMax=xEnd-xStart;
			yRelMax=yEnd-yStart;
			rRelMax=RConvert.ROFXY(xRelMax,yRelMax);
			theta=RConvert.THETAOFXY_RAD(xRelMax,yRelMax);
			rRel=0;
			rRel-=fSubpixelPrecisionIncrement; //the "0th" value
			Pixel32 pixelColor=new Pixel32(pixelStart);
			while (rRel<rRelMax && iLoops<iMaxLoops) {
				rRel+=fSubpixelPrecisionIncrement;
				if (pixelEndOrNull!=null) {
					pixelColor.R=RMath.Approach(pixelStart.R,pixelEndOrNull.R,rRel/rRelMax);
					pixelColor.G=RMath.Approach(pixelStart.G,pixelEndOrNull.G,rRel/rRelMax);
					pixelColor.B=RMath.Approach(pixelStart.B,pixelEndOrNull.B,rRel/rRelMax);
					pixelColor.A=RMath.Approach(pixelStart.A,pixelEndOrNull.A,rRel/rRelMax);
				}
				xNow=(RConvert.XOFRTHETA_RAD(rRel,theta))+xStart;
				yNow=(RConvert.YOFRTHETA_RAD(rRel,theta))+yStart;
				if (xNow>0&&yNow>0&&xNow<iWidth&&yNow<iHeight)
					DrawVectorDot(xNow, yNow, pixelColor);
				iLoops++;
				if ((iLoops>=iMaxLoops)&&(RPlatform.TickCount-iStartTick>iMaxTicks)) {
					bLimited=true;
					break;//if (iLoops>=iMaxLoops) break;
				}
			}//end while drawing line
			if (bLimited) {
				RReporting.ShowErr("Line drawing loop overflow","drawing beyond line drawing safe limit","DrawVectorLine");
			}
		}//end DrawVectorLine
	
		public void DrawVectorLine(FPoint pointStart, FPoint pointEnd,
				Pixel32 pixelStart, Pixel32 pixelEndOrNull, float fSubpixelPrecisionIncrement) {
			DrawVectorLine(pointStart.X, pointStart.Y, pointEnd.X, pointEnd.Y,
				pixelStart, pixelEndOrNull, fSubpixelPrecisionIncrement);
		}//DrawVectorLine
		public float PixelsPerDegAt(float rPixelsRadius) {
			float x1,y1,x2,y2;
			x1=(RConvert.XOFRTHETA_DEG(rPixelsRadius,0));
			y1=-(RConvert.YOFRTHETA_DEG(rPixelsRadius,0));//negative to flip to non-cartesian monitor loc
			x2=(RConvert.XOFRTHETA_DEG(rPixelsRadius,1));
			y2=-(RConvert.YOFRTHETA_DEG(rPixelsRadius,1));//negative to flip to non-cartesian monitor loc
			return RMath.Dist(x1,y1,x2,y2);
		}
		public double PixelsPerDegAt(double rPixelsRadius) {
			double x1,y1,x2,y2;
			x1=(RConvert.XOFRTHETA_DEG(rPixelsRadius,0));
			y1=-(RConvert.YOFRTHETA_DEG(rPixelsRadius,0));//negative to flip to non-cartesian monitor loc
			x2=(RConvert.XOFRTHETA_DEG(rPixelsRadius,1));
			y2=-(RConvert.YOFRTHETA_DEG(rPixelsRadius,1));//negative to flip to non-cartesian monitor loc
			return RMath.Dist(x1,y1,x2,y2);
		}
		public float PixelsPerRadAt(float rPixelsRadius) {
			float x1,y1,x2,y2;
			x1=(RConvert.XOFRTHETA_RAD(rPixelsRadius,0));
			y1=-(RConvert.YOFRTHETA_RAD(rPixelsRadius,0));//negative to flip to non-cartesian monitor loc
			x2=(RConvert.XOFRTHETA_RAD(rPixelsRadius,1));
			y2=-(RConvert.YOFRTHETA_RAD(rPixelsRadius,1));//negative to flip to non-cartesian monitor loc
			return RMath.Dist(x1,y1,x2,y2);
		}
		public double PixelsPerRadAt(double rPixelsRadius) {
			double x1,y1,x2,y2;
			x1=(RConvert.XOFRTHETA_RAD(rPixelsRadius,0));
			y1=-(RConvert.YOFRTHETA_RAD(rPixelsRadius,0));//negative to flip to non-cartesian monitor loc
			x2=(RConvert.XOFRTHETA_RAD(rPixelsRadius,1));
			y2=-(RConvert.YOFRTHETA_RAD(rPixelsRadius,1));//negative to flip to non-cartesian monitor loc
			return RMath.Dist(x1,y1,x2,y2);
		}
		public void DrawVectorArc(float xCenter, float yCenter, float fRadius, Pixel32 pixelColor) {
			DrawVectorArc(xCenter,yCenter,fRadius,1.0f,0.0f,0.0f,360.0f,pixelColor,1.0f,0.0f);
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
				xNow=(RConvert.XOFRTHETA_DEG(fRadius,fNow));
				yNow=-(RConvert.YOFRTHETA_DEG(fRadius,fNow));//negative to flip to non-cartesian monitor loc
				xNow*=fWidthMultiplier;
				RMath.Rotate(ref xNow,ref yNow,fRotate);
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
				RReporting.ShowErr("DrawVectorArc loop overflow!","drawing arc past time limit performance setting","DrawVectorArc");
			}
		}//DrawVectorArc
		public void Clear(Color colorNow) {//keep this, to mimic the equivalent Graphics object method
			Fill(new byte[] {colorNow.B,colorNow.G,colorNow.R,colorNow.A});
		}
		public void Fill(byte byGrayVal) {
			RMemory.Fill(ref byarrData,byGrayVal,0,iBytesTotal);
		}
		public void Fill(uint dwPixelBGRA) {
			if (iBytesPP==4) {
				RMemory.Fill(ref byarrData,dwPixelBGRA,0,iBytesTotal/4);
			}
			else RReporting.ShowErr("Filling this type of surface with 32-bit color is not implemented","",String.Format("Fill(32-bit){{ImageBitDepth:{0}}}",(iBytesPP*8)));
		}
		public void Fill(byte[] byarrPixel32bit) {
			if (iBytesPP==4) {
				RMemory.Fill4(ref byarrData,ref byarrPixel32bit,0,0,iBytesTotal/4);
			}
			else RReporting.ShowErr("Filling this type of surface with 32-bit color is not implemented","",String.Format("Fill(32-bit){{ImageBitDepth:{0}}}",(iBytesPP*8)));
		}
		public static bool DrawPixel(RImage riDest, int iDestBufferLoc, RImage riSrc, int iSrcBufferLoc, int iDrawMode) {
			bool bGood=true;
			int iBytesPPMin=-1;//must start as -1
			float fCookedAlpha=-1.0f;//must start as -1
			try {
				byte[] riSrc_byarrData=riSrc.byarrData;
				byte[] riDest_byarrData=riDest.byarrData;
				switch (iDrawMode) {
					case DrawModeCopyAlpha:
						iBytesPPMin=(riSrc.iBytesPP<riDest.iBytesPP)?riSrc.iBytesPP:riDest.iBytesPP;
						if (iBytesPPMin>=3) {
							bGood=RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
						}
						else {
							if (riDest.iBytesPP==4&&riSrc.iBytesPP==1) {	
								riDest_byarrData[iDestBufferLoc+3]=riSrc_byarrData[iSrcBufferLoc];
							}
							else if (riDest.iBytesPP==1&&riSrc.iBytesPP==4) {
								riDest_byarrData[iDestBufferLoc]=riSrc_byarrData[iSrcBufferLoc+3];
							}
							else bGood=RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
						}
						break;
					case DrawModeAlpha:
						if (riSrc.iBytesPP==4&&riDest.iBytesPP>=3) {
							fCookedAlpha=(float)riSrc_byarrData[iSrcBufferLoc+3]/255.0f;
							riDest_byarrData[iDestBufferLoc]=RMath.ByRound(((float)(riSrc_byarrData[iSrcBufferLoc]-riDest_byarrData[iDestBufferLoc]))*fCookedAlpha+riDest_byarrData[iDestBufferLoc]); //B
							iSrcBufferLoc++; iDestBufferLoc++;
							riDest_byarrData[iDestBufferLoc]=RMath.ByRound(((float)(riSrc_byarrData[iSrcBufferLoc]-riDest_byarrData[iDestBufferLoc]))*fCookedAlpha+riDest_byarrData[iDestBufferLoc]); //G
							iSrcBufferLoc++; iDestBufferLoc++;
							riDest_byarrData[iDestBufferLoc]=RMath.ByRound(((float)(riSrc_byarrData[iSrcBufferLoc]-riDest_byarrData[iDestBufferLoc]))*fCookedAlpha+riDest_byarrData[iDestBufferLoc]); //R
						}
						else {
							RReporting.ShowErr("Cannot use "+DrawModeToString(iDrawMode)+" overlay mode with images that are not true color or when source is not 32-bit.","","DrawPixel");
						}
						break;
					case DrawModeAlphaQuickEdge:
						if (riSrc.iBytesPP>=4&&riDest.iBytesPP>=3) {
							byte byAlpha=riSrc_byarrData[iSrcBufferLoc+3];
							if (byAlpha<=85) {//do nothing
							}
							else if (byAlpha>170) {
								riDest_byarrData[iDestBufferLoc]=riSrc_byarrData[iSrcBufferLoc];
								riDest_byarrData[iDestBufferLoc+1]=riSrc_byarrData[iSrcBufferLoc+1];
								riDest_byarrData[iDestBufferLoc+2]=riSrc_byarrData[iSrcBufferLoc+2];
							}
							else {
								riDest_byarrData[iDestBufferLoc]=(byte)( (riSrc_byarrData[iSrcBufferLoc]>>2) + (riDest_byarrData[iDestBufferLoc]>>2) ); //B
								iSrcBufferLoc++; iDestBufferLoc++;
								riDest_byarrData[iDestBufferLoc]=(byte)( (riSrc_byarrData[iSrcBufferLoc]>>2) + (riDest_byarrData[iDestBufferLoc]>>2) ); //G
								iSrcBufferLoc++; iDestBufferLoc++;
								riDest_byarrData[iDestBufferLoc]=(byte)( (riSrc_byarrData[iSrcBufferLoc]>>2) + (riDest_byarrData[iDestBufferLoc]>>2) ); //R
							}
						}
						else {
							RReporting.ShowErr("Cannot use "+DrawModeToString(iDrawMode)+" overlay mode with images that are not true color or when source is not 32-bit.","","DrawPixel");
						}
						break;
					case DrawModeAlphaHardEdge:
						if (riSrc.iBytesPP>=4&&riDest.iBytesPP>=3) {
							byte byAlpha=riSrc_byarrData[iSrcBufferLoc+3];
							if (byAlpha<128) {//do nothing
							}
							else {
								riDest_byarrData[iDestBufferLoc]=riSrc_byarrData[iSrcBufferLoc];
								riDest_byarrData[iDestBufferLoc+1]=riSrc_byarrData[iSrcBufferLoc+1];
								riDest_byarrData[iDestBufferLoc+2]=riSrc_byarrData[iSrcBufferLoc+2];
							}
						}
						else {
							RReporting.ShowErr("Cannot use "+DrawModeToString(iDrawMode)+" overlay mode with images that are not true color or when source is not 32-bit.","","DrawPixel");
						}
						break;
					case DrawModeKeepGreaterAlpha:
						if (riSrc.iBytesPP>=4&&riDest.iBytesPP>=3) {
							riDest_byarrData[iDestBufferLoc]=(riSrc_byarrData[iSrcBufferLoc]>riDest_byarrData[iDestBufferLoc])?riSrc_byarrData[iSrcBufferLoc]:riDest_byarrData[iDestBufferLoc]; //B
							iSrcBufferLoc++; iDestBufferLoc++;
							riDest_byarrData[iDestBufferLoc]=(riSrc_byarrData[iSrcBufferLoc]>riDest_byarrData[iDestBufferLoc])?riSrc_byarrData[iSrcBufferLoc]:riDest_byarrData[iDestBufferLoc]; //G
							iSrcBufferLoc++; iDestBufferLoc++;
							riDest_byarrData[iDestBufferLoc]=(riSrc_byarrData[iSrcBufferLoc]>riDest_byarrData[iDestBufferLoc])?riSrc_byarrData[iSrcBufferLoc]:riDest_byarrData[iDestBufferLoc]; //R
						}
						else {
							RReporting.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","","DrawPixel");
						}
						break;
					case DrawModeKeepDestAlpha:
						//if (riSrc.iBytesPP>=4&&riDest.iBytesPP>=3) {
						//	iBytesPPMin=(riSrc.iBytesPP<riDest.iBytesPP)?riSrc.iBytesPP:riDest.iBytesPP;
						//	if (iBytesPPMin>=4) iBytesPPMin=3;
						//	RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData,iDestPix,iSrcPix,iBytesPPMin);
						//else {
						//	RReporting.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","","DrawPixel");
						//}
						//break;
						iBytesPPMin=(riSrc.iBytesPP<riDest.iBytesPP)?riSrc.iBytesPP:riDest.iBytesPP;
						if (iBytesPPMin>=3) {
							if (iBytesPPMin>3) iBytesPPMin=3;//since DrawModeKeepDestAlpha
							bGood=RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
						}
						else {
							if (riDest.iBytesPP==4&&riSrc.iBytesPP==1) {	
								RReporting.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" when source is an alpha mask and destination is 32-bit.","","DrawPixel");
								//riDest_byarrData[iDestBufferLoc+3]=riSrc_byarrData[iSrcBufferLoc];
							}
							else if (riDest.iBytesPP==1) {//&&riSrc.iBytesPP==4) {
								RReporting.ShowErr("Cannot use mode "+DrawModeToString(iDrawMode)+" when destination is an alpha mask.","DrawPixel");
								//riDest_byarrData[iDestBufferLoc]=riSrc_byarrData[iSrcBufferLoc+3];
							}
							else {
								if (iBytesPPMin>3) iBytesPPMin=3;//since DrawModeKeepDestAlpha
								bGood=RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
							}
						}
						break;
					default:
						RReporting.Warning("DrawPixel mode "+iDrawMode.ToString()+" is not implemented");
						bGood=false;
						break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing pixel","DrawPixel {riDest:"+riDest!=null?riDest.Description():"null"
				+"; riSrc:"+riSrc!=null?riSrc.Description():"null"
				+"; iDestBufferLoc:"+iDestBufferLoc.ToString()
				+"; iSrcBufferLoc:"+iSrcBufferLoc.ToString()
				+"; iDrawMode:"+DrawModeToString(iDrawMode)
				+"; iBytesPPMin:"+((iBytesPPMin!=-1)?iBytesPPMin.ToString():"unused")
				+"; fCookedAlpha:"+((fCookedAlpha!=-1.0f)?fCookedAlpha.ToString():"unused")
				+"}"
				);
			}
			return bGood;
		}//end DrawPixel
		public bool DrawTo(Bitmap bmpDest) {//, Graphics Graphics_FromImage_bmpDest) {
			bool bGood=true;
			Exception exn2=null;
			//Pen penNow=new Pen();
			//Color colorNow=new Color();
		BitmapData bmpdataDest = bmpDest.LockBits(new Rectangle(0, 0,
										bmpDest.Width, bmpDest.Height),
										ImageLockMode.WriteOnly, ///take notice
										PixelFormat.Format32bppArgb);
			int y=0, x=0;
			int iLineStart=0;
			int iSrc=-1;
			int iExceptions=0;
			try {
				//debug performance--try: using(Graphics gNow = Graphics.FromImage(bmpDest)) {//do drawing here}
				int iMinStride=iStride<bmpdataDest.Stride?iStride:bmpdataDest.Stride;
				unsafe {
					byte* bypDest = (byte*)bmpdataDest.Scan0;
					fixed (byte* bypSrc=byarrData) {
					byte* bypSrcNow=bypSrc;
						for (y=0; y<Height; y++) {
							//iSrc=iLineStart;
							try {
								RMemory.Copy(bypDest,bypSrcNow,iMinStride);
								bypDest+=bmpdataDest.Stride;
								bypSrcNow+=iStride;
							}
							catch (Exception exn) { exn2=exn; iExceptions++;}
							//iLineStart+=iStride;
						}//end for y
					}//end fixed *
				}//end unsafe
			}
			catch (Exception exn) { exn2=exn; iExceptions++;}
			if (exn2!=null) {
				bGood=false;
				RReporting.ShowExn(exn2,"drawing form bitmap from rimage","RImage DrawTo(Bitmap) {x:"+x.ToString()+"; y:"+y.ToString()+"; iSrc:"+iSrc.ToString()+"; "+DumpStyle(false)+"; iExceptions:"+iExceptions.ToString()+"}");
			}
			try {if (bmpDest!=null) bmpDest.UnlockBits(bmpdataDest);}
			catch {}
			return bGood;
		}//end DrawTo
		public bool DrawToSafe(Bitmap bmpDest) {
			bool bGood=true;
			Exception exn2=null;
			//Pen penNow=new Pen();
			Color colorNow=new Color();
			int y=0, x=0;
			int iLineStart=0;
			int iSrc=-1;
			int iExceptions=0;
			try {
				//debug performance--try: using(Graphics gNow = Graphics.FromImage(bmpDest)) {//do drawing here}
				for (y=0; y<Height; y++) {
					iSrc=iLineStart;
					try {
						for (x=0; x<Width; x++) {
							if (iBytesPP>=4) {
								colorNow=Color.FromArgb(byarrData[iSrc+3],byarrData[iSrc+2],byarrData[iSrc+1],byarrData[iSrc]);
								iSrc+=iBytesPP;
							}
							else if (iBytesPP==3) {
								colorNow=Color.FromArgb(255,byarrData[iSrc+2],byarrData[iSrc+1],byarrData[iSrc]);
								iSrc+=3;
							}
							else {
								colorNow=Color.FromArgb(255,byarrData[iSrc],byarrData[iSrc],byarrData[iSrc]);
								iSrc++;
							} 
							bmpDest.SetPixel(x,y,colorNow);//DrawRectangle(new Pen(colorNow),x,y,1,1);
						}//end for x
					}
					catch (Exception exn) { exn2=exn; iExceptions++;}
					iLineStart+=iStride;
				}//end for y
			}
			catch (Exception exn) { exn2=exn; iExceptions++;}
			if (exn2!=null) {
				bGood=false;
				RReporting.ShowExn(exn2,"drawing form bitmap from rimage","RImage DrawTo(Bitmap){x:"+x.ToString()+"; y:"+y.ToString()+"; iSrc:"+iSrc.ToString()+"; "+DumpStyle(false)+"; iExceptions:"+iExceptions.ToString()+"}");
			}
			return bGood;
		}//end DrawToSafe
		
/*		public bool DrawTo(Graphics gDest) {
			bool bGood=true;
			Exception exn2=null;
			//Pen penNow=new Pen();
			Color colorNow=new Color();
			int y=0, x=0;
			int iLineStart=0;
			int iSrc=-1;
			int iExceptions=0;
			try {
				for (y=0; y<Height; y++) {
					iSrc=iLineStart;
					try {
						for (x=0; x<Width; x++) {
							if (iBytesPP>=4) {
								colorNow=Color.FromArgb(byarrData[iSrc+3],byarrData[iSrc+2],byarrData[iSrc+1],byarrData[iSrc]);
								iSrc+=iBytesPP;
							}
							else if (iBytesPP==3) {
								colorNow=Color.FromArgb(255,byarrData[iSrc+2],byarrData[iSrc+1],byarrData[iSrc]);
								iSrc+=3;
							}
							else {
								colorNow=Color.FromArgb(255,byarrData[iSrc],byarrData[iSrc],byarrData[iSrc]);
								iSrc++;
							} 
							gDest.DrawRectangle(new Pen(colorNow),x,y,1,1);
						}//end for x
					}
					catch (Exception exn) { exn2=exn; iExceptions++;}
					iLineStart+=iStride;
				}//end for y
			}
			catch (Exception exn) { exn2=exn; iExceptions++;}
			if (exn2!=null) {
				bGood=false;
				Console.Error.WriteLine(exn2.ToString());
				//RReporting.ShowExn(exn2,"drawing form graphics from rimage","RImage DrawTo(Graphics) {x:"+x.ToString()+"; y:"+y.ToString()+"; iSrc:"+iSrc.ToString()+"; "+DumpStyle(false)+"; iExceptions:"+iExceptions.ToString()+"}");
			}
			return bGood;
		}//end DrawTo
*/	
		/// <summary>
		/// Non-scaled rect-to-rect draw method
		/// </summary>
		public static bool Draw(RImage riDest, IRect rectDest, RImage riSrc, IRect rectSrc, int iDrawMode) {
			bool bGood=false;
			try {
				int xSrc=rectSrc.X;
				int ySrc=rectSrc.Y;
				int xDest=rectDest.X;
				int yDest=rectDest.Y;
				int xDestRel=0;
				int yDestRel=0;
				int iDestLine=riDest.XYToLocation(xDest,yDest);
				int iSrcLine=riSrc.XYToLocation(xSrc,ySrc);
				int iDest=iDestLine;
				int iSrc=iSrcLine;
				if (iDestLine>=0&&iSrcLine>=0) {
					while (yDestRel<rectDest.Height) {
						if (xDestRel>=rectDest.Width) {
							iDestLine+=riDest.iStride;
							iSrcLine+=riSrc.iStride;
							iDest=iDestLine;
							iSrc=iSrcLine;
							xDestRel=0;
							yDestRel++;
							ySrc++;
							yDest++;
							xSrc=rectSrc.X;
							xDest=rectDest.X;
						}
						DrawPixel(riDest,iDest,riSrc,iSrc,iDrawMode);
						xDestRel++;
						xSrc++;
						xDest++;
						iSrc+=riSrc.iBytesPP;
						iDest+=riDest.iBytesPP;
					}
					bGood=true;
				}
				else {
					RReporting.ShowErr("Cannot draw image at specified overlay location.","","Draw(riDest,rectDest,riSrc,rectSrc,iDrawMode){rectDest:"+IRect.ToString(rectDest)+"; rectSrc:"+IRect.ToString(rectSrc)+"}");
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"","Draw(riDest,rectDest,riSrc,rectSrc,iDrawMode)");
			}
			return bGood;
		}
		
		public bool Draw(IRect rectDest, RImage riSrc, IRect rectSrc) {
			return Draw(this, rectDest, riSrc, rectSrc, DrawModeCopyAlpha);
		}
		public bool Draw(IRect rectDest, RImage riSrc, IRect rectSrc, int iDrawMode) {
			return Draw(this,rectDest,riSrc,rectSrc,iDrawMode);
		}
		public bool Draw(IRect rectDest, RImage riSrc) {
			return Draw(rectDest, riSrc, DrawModeCopyAlpha);
		}
		public bool Draw(IRect rectDest, RImage riSrc, int iDrawMode) {
			bool bGood=false;
			try {
				bGood=DrawSmallerWithoutCropElseCancel(rectDest.X,rectDest.Y,riSrc,iDrawMode);
				//bGood=riSrc.DrawToLargerWithoutCropElseCancel(this,rectDest.X,rectDest.Y,iDrawMode);
				//TODO: finish this--add cropping capability (currently: cancels if no fit)
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"","Draw(RImage riSrc,...)");
			}
			return bGood;
		}
		#endregion Draw methods
	}//end class RImage

	public class RImageStack { //pack Stack -- array, order left(First) to right(Last) //formerly GBufferStack
		private RImage[] riarr=null;
		private int Maximum {
			get {
				return (riarr==null)?0:riarr.Length;
			}
			set {
				RImage.Redim(ref riarr,value,"RImageStack");
			}
		}
		private int iCount;
		private int LastIndex {	get { return iCount-1; } }
		private int NewIndex { get  { return iCount; } }
		//public bool IsFull { get { return (iCount>=Maximum) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public RImage Element(int iElement) {
			return (iElement<iCount&&iElement>=0&&riarr!=null)?riarr[iElement]:null;
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
		//		if (riarr[iNow].ToLower()==sCaseInsensitiveSearch) iReturn++;
		//	}
		//	return iReturn;
		//}
		//public int CountInstances(string sCaseSensitiveSearch) {//commented for debug only (to remember to use CountInstancesI)
		//	int iReturn=0;
		//	for (int iNow=0; iNow<iCount; iNow++) {
		//		if (riarr[iNow]==sCaseSensitiveSearch) iReturn++;
		//	}
		//	return iReturn;
		//}
		public RImageStack() { //Constructor
			int iDefaultSize=256;
			//TODO: settings.GetOrCreate(ref iDefaultSize,"RImageStackDefaultStartSize");
			Init(iDefaultSize);
		}
		public RImageStack(int iSetMax) { //Constructor
			Init(iSetMax);
		}
		private void Init(int iSetMax) { //always called by Constructor
			if (iSetMax<0) RReporting.Warning("RImageStack initialized with negative number so it will be set to a default.");
			else if (iSetMax==0) RReporting.Warning("RImageStack initialized with zero so it will be set to a default.");
			if (iSetMax<=0) iSetMax=1;
			Maximum=iSetMax;
			iCount=0;
			if (riarr==null) RReporting.ShowErr("Stack constructor couldn't initialize riarr");
		}
		public void Clear() {
			iCount=0;
			for (int iNow=0; iNow<riarr.Length; iNow++) {
				riarr[iNow]=null;
			}
		}
		public void ClearFastWithoutFreeingImagesFromRMemory() {
			iCount=0;
		}
		public void SetFuzzyMaximumByLocation(int iLoc) {
			int iNew=iLoc+iLoc/2+1;
			if (iNew>Maximum) Maximum=iNew;
		}
		public bool Push(RImage add) {
			//if (!IsFull) {
			try {
				if (add!=null) {
					if (NewIndex>=Maximum) SetFuzzyMaximumByLocation(NewIndex);
					riarr[NewIndex]=add;
					iCount++;
				}
				else RReporting.ShowErr("Cannot push a null RImage to a stack.","pushing RImage to stack","RImageStack Push");
				//sLogLine="debug enq iCount="+iCount.ToString();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"putting image onto rimage stack","RImageStack Push("+((add==null)?"null RImage":"non-null")+"){new-location:"+NewIndex.ToString()+"}");
				return false;
			}
			return true;
			//}
			//else {
			//	RReporting.ShowErr("RImageStack is full, can't push \""+add+"\"! ( "+iCount.ToString()+" RImages already used)","","RImageStack Push("+((add==null)?"null RImage":"non-null")+")");
			//	return false;
			//}
		}
		public RImage Pop() {
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				//RReporting.ShowErr("no RImages to return so returned null","","RImageStack Pop");
				return null;
			}
			int iReturn = LastIndex;
			iCount--;
			return riarr[iReturn];
		}
		public RImage[] ToArrayByReferences() {
			RImage[] riarrReturn=null;
			try {
				if (iCount>0) {
					riarrReturn=new RImage[iCount];
					for (int iNow=0; iNow<iCount; iNow++) {
						riarrReturn[iNow]=riarr[iNow];
					}
				}
				else RReporting.ShowErr("Cannot copy a zero-length stack.","copying stack to array","RImageStack ToArrayByReferences");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImageStack ToArrayByReferences");
			}
			return riarrReturn;
		}
	}//end RImageStack created 2007-10-03
}//end namespace
