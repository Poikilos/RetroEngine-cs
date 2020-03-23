/*
 * Created by SharpDevelop.
 * User: Owners
 * Date: 7/25/2005
 * Time: 9:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */


using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
//using System.Drawing.Text;

//using REAL = System.Double; //System.Single
using REAL = System.Single; //System.Double

namespace ExpertMultimedia {
	public class Anim {
		//public int iWidth;
		//public int iHeight;
		//public byte[] iPal;//Palette
		//public int iPalByteDepth;
		//public int iPalTransColor;
		//public Uri uriFile; //a framework path object
		//TODO: use these when needed:
			//Sdl.FRAMES_TO_MSF(iFrames, m, s, f);
			//iFrames=Sdl.MSF_TO_FRAMES(m,s,f);.
		private Bitmap bmpLoaded=null;
		private BitmapData bmpdata;
		private GraphicsUnit gunit;
		private RectangleF rectNowF;
		private Rectangle rectNow;
		public long lFramesCached;
		private string sPathFileBaseName="1.untitled";
		private string sFileExt="raw";
		private string sPathFileNonSeq {
			get {
				return sPathFileBaseName+"."+sFileExt;
			}
		}
		public int Width {
			get {
				try {
					if (gbFrame!=null) return gbFrame.iWidth;
				}
				catch (Exception exn) {
				}
				return 0;
			}
		}
		public int Height {
			get {
				try {
					if (gbFrame!=null) return gbFrame.iHeight;
				}
				catch (Exception exn) {
				}
				return 0;
			}
		}
		//private string sPathFile="";
		public bool bFileSequence;//if true, use sPathFileBaseName+digits+"."+sExt, and sPathFile is first frame
		public int iSeqDigitsMin=4;//, 0 if variable (i.exn. frame1.png...frame10.png)
		public long lFrameNow;
		public long lFrames;
		public int iEffects;
		public Image imageOrig;
		private int iMaxEffects;
		private PixelFormat pixelformatNow=PixelFormat.Format32bppArgb;
		/// <summary>
		/// only full if lFramesCached==lFrames
		/// </summary>
		private GBuffer[] gbarrAnim;
		//public byte[] byarrFrame;
		public GBuffer gbFrame;
		//private byte[][] by2dMask;
		//public byte[] byarrMask;
		public Effect[] effectarr;
		public Anim Copy() {
			Anim animReturn=null;
			try {
				animReturn=new Anim();
				animReturn.bmpLoaded=(Bitmap)bmpLoaded.Clone();
				animReturn.gunit=gunit;
				animReturn.rectNowF=new RectangleF(rectNowF.Left,rectNowF.Top,rectNowF.Width,rectNowF.Height);
				animReturn.rectNow=new Rectangle(rectNow.Left,rectNow.Top,rectNow.Width,rectNow.Height);
				animReturn.lFramesCached=lFramesCached;
				animReturn.sPathFileBaseName=sPathFileBaseName;
				animReturn.sFileExt=sFileExt;
				//animReturn.sPathFile=sPathFile;
				animReturn.bFileSequence=bFileSequence;//if use sPathFileBaseName+digits+"."+sExt, and sPathFile is first frame
				animReturn.iSeqDigitsMin=iSeqDigitsMin;//, 0 if variable (i.exn. frame1.png...frame10.png)
				animReturn.lFrameNow=lFrameNow;
				animReturn.lFrames=lFrames;
				animReturn.iEffects=iEffects;
				animReturn.imageOrig=imageOrig;
				animReturn.iMaxEffects=iMaxEffects;
				animReturn.lFramesCached=lFramesCached;
				if (lFramesCached==lFrames) {
					animReturn.gbarrAnim=new GBuffer[lFramesCached];
					for (long lNow=0; lNow<lFrames; lNow++) {
						if (gbarrAnim[lNow]!=null) animReturn.gbarrAnim[lNow]=gbarrAnim[lNow].Copy();
						else {
							RReporting.ShowErr("Trying to copy null frame!","Anim Copy");
						}
					}
				}
				else RReporting.ShowError("Uncached anim not yet implemented","anim Copy");
				animReturn.gbFrame=animReturn.gbarrAnim[lFrameNow];
				if (iEffects>0) {
					animReturn.effectarr=new Effect[iEffects];
					for (int i=0; i<iEffects; i++) {
						animReturn.effectarr[i]=effectarr[i].Copy();
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"anim Copy");
			}
			return animReturn;
		}
		public Anim CopyAsGray() {
			return CopyAsGray(-1);
		}
		/// <summary>
		/// Makes a new Anim object from any channel (or the value) of this one.
		/// </summary>
		/// <param name="iChannelOffset">Pick a channel to copy.  Set to -1 to take average of RGB.</param>
		/// <returns></returns>
		public Anim CopyAsGray(int iChannelOffset) {
			Anim animReturn=null;
			try {
				animReturn=new Anim();
				if (bmpLoaded!=null) animReturn.bmpLoaded=(Bitmap)bmpLoaded.Clone();
				animReturn.gunit=gunit;
				animReturn.rectNowF=new RectangleF(rectNowF.Left,rectNowF.Top,rectNowF.Width,rectNowF.Height);
				animReturn.rectNow=new Rectangle(rectNow.Left,rectNow.Top,rectNow.Width,rectNow.Height);
				animReturn.lFramesCached=lFramesCached;
				animReturn.sPathFileBaseName=sPathFileBaseName;
				animReturn.sFileExt=sFileExt;
				//animReturn.sPathFile=sPathFile;
				animReturn.bFileSequence=bFileSequence;//if use sPathFileBaseName+digits+"."+sExt, and sPathFile is first frame
				animReturn.iSeqDigitsMin=iSeqDigitsMin;//, 0 if variable (i.exn. frame1.png...frame10.png)
				animReturn.lFrameNow=lFrameNow;
				animReturn.lFrames=lFrames;
				animReturn.iEffects=iEffects;
				animReturn.imageOrig=imageOrig;
				animReturn.iMaxEffects=iMaxEffects;
				animReturn.lFramesCached=lFramesCached;
				if (lFramesCached==lFrames) {
					animReturn.gbarrAnim=new GBuffer[lFramesCached];
					if (iChannelOffset<0) {
						for (long l=0; l<lFrames; l++) {
							animReturn.gbarrAnim[l]=null;
							GBuffer.MaskFromValue(ref animReturn.gbarrAnim[l], ref gbarrAnim[l]);
						}
					}
					else {
						for (long l=0; l<lFrames; l++) {
							animReturn.gbarrAnim[l]=null;
							GBuffer.MaskFromChannel(ref animReturn.gbarrAnim[l], ref gbarrAnim[l], iChannelOffset);
						}
					}
				}
				else RReporting.ShowError("Uncached Anim not yet implemented","anim CopyAsGray");
				animReturn.gbFrame=animReturn.gbarrAnim[lFrameNow];
				if (iEffects>0) {
					animReturn.effectarr=new Effect[iEffects];
					for (int i=0; i<iEffects; i++) {
						animReturn.effectarr[i]=effectarr[i].Copy();
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"anim CopyAsGray");
			}
			return animReturn;
		}//end CopyAsGray
		public bool SaveSeq(string sSetFileBase, string sSetFileExt) {
			bool bGood=true;
			//try { //first write debug file
			sFileExt=sSetFileExt;
			sPathFileBaseName=sSetFileBase;
			//	RString.StringToFile(sSetFileBase+".txt", RString.ToString(true)); }
			//catch (Exception exn) {
			//	RReporting.ShowExn(exn,anim SaveSeq","saving dump"); }
			try {
				for (long lFrameSave=0; lFrameSave<lFrames; lFrameSave++) {
					if (!SaveSeqFrame(lFrameSave)) {
						bGood=false;
						RReporting.ShowError("Couldn't Save "+PathFileOfSeqFrame(sSetFileBase, sSetFileExt, lFrameNow, iSeqDigitsMin),"SaveSeq");
					}
				}
			} catch (Exception exn) {RReporting.ShowExn(exn,"SaveSeq");}
			return bGood;
		}//SaveSeq
		
		public override string ToString() {
			return ToString(false);
		}
		public string ToString(bool bDumpVars) {
			string sReturn="";
			try {
				sReturn=(gbFrame!=null)?(gbFrame.DumpStyle()):"null-frame-anim";
				if (bDumpVars) {
					if (sReturn.EndsWith("}")) sReturn=sReturn.Substring(0,sReturn.Length-1);
					sReturn+=" "
						+"lFrames:"+this.lFrames
						+"; lFramesCached:"+this.lFramesCached
						+"; lFrameNow:"+this.lFrameNow
						+"; sFileExt:"+this.sFileExt
						//+"; sPathFile:"+this.sPathFile
						+"; sPathFileBaseName:"+this.sPathFileBaseName
						+"; bFileSequence:"+this.bFileSequence.ToString()
						+"; iEffects:"+this.iEffects
						+"; iMaxEffects:"+this.iMaxEffects
						+"; iSeqDigitsMin:"+this.iSeqDigitsMin;
					if (gbFrame!=null) {
						try {
							sReturn+="; gbFrame.iPixelsTotal:"+gbFrame.iPixelsTotal
								+"; gbFrame.iWidth:"+gbFrame.iWidth
								+"; gbFrame.iHeight:"+gbFrame.iHeight
								+"; gbFrame.iWidth:"+gbFrame.iWidth
								+"; gbFrame.Channels:"+gbFrame.Channels();
						}
						catch (Exception exn) {
							RReporting.ShowExn(exn,"anim ToString(true)","accessing gbFrame");
						}
					}
					sReturn+=";}";
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"ToString("+bDumpVars.ToString()+")","saving dump info");
				sReturn="{Exception error dumping anim vars}";
			}
			return sReturn;
		}
		public bool SaveCurrentSeqFrame() {
			bool bGood=CopyFrameToInternalBitmap();
			if (bGood) bGood=SaveInternalBitmap(PathFileOfSeqFrame(sPathFileBaseName, sFileExt, lFrameNow, iSeqDigitsMin));
			return bGood;
		}
		public bool SaveSeqFrame(long lFrameSave) {
			return SaveSeqFrame(sPathFileBaseName, sFileExt, lFrameSave);
		}
		public bool SaveSeqFrame(string sFileBase, string sSetFileExt, long lFrameSave) {
			bool bGood=true;
			if (!GotoFrame(lFrameSave)) {
				bGood=false;
				RReporting.ShowErr("Failed to goto frame "+lFrameSave.ToString()+" of Anim","SaveSeqFrame");
			}
			else {
				bGood=CopyFrameToInternalBitmap();
				if (bGood) {
					sFileExt=sSetFileExt;
					bGood=SaveInternalBitmap(PathFileOfSeqFrame(sFileBase, sSetFileExt, lFrameSave, iSeqDigitsMin), ImageFormatFromExt());
					if (!bGood) {
						RReporting.ShowErr("Failed to save "+PathFileOfSeqFrame(sFileBase, sSetFileExt, lFrameSave, iSeqDigitsMin),"SaveCurrentSeqFrame");
					}
				}
				else RReporting.ShowErr("Failed to copy data to frame image","SaveCurrentSeqFrame");
			}
			return bGood;
		}
		public ImageFormat ImageFormatFromExt() {
			return RImage.ImageFormatFromExt(sFileExt);
		}
		public bool ResetBitmapUsingFrameNow() {
			bool bGood=true;
			try {
				bmpLoaded=new Bitmap(gbFrame.iWidth, gbFrame.iHeight, PixelFormatNow());
				//bmpLoaded.Save(Manager.sDataFolderSlash+"1.test-blank.png", ImageFormat.Png);
				//sLogLine="Saved blank test PNG for Bitmap debug";
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"ResetBitmapUsingFrameNow","initializing image");
			}
			return bGood;
		}
		public PixelFormat PixelFormatNow() {
			return pixelformatNow;
		}
		public bool CopyFrameToInternalBitmap() {
			bool bGood=true;
			string sVerbNow="resetting internal bitmap using frame type";
			try {
				bGood=ResetBitmapUsingFrameNow();
				if (!bGood) RReporting.ShowErr("Failed to reset internal frame image","CopyFrameToInternalBitmap");
				sVerbNow="getting bounds";
				gunit = GraphicsUnit.Pixel;
				rectNowF = bmpLoaded.GetBounds(ref gunit);
				rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y, (int)rectNowF.Width, (int)rectNowF.Height);
				int iNow=0;
				for (int yNow=0; yNow<rectNow.Height; yNow++) {
					for (int xNow=0; xNow<rectNow.Width; xNow++) {
						byte aNow,rNow,gNow,bNow;
						if (gbFrame.rarrData!=null) aNow=RConvert.DecimalToByte(gbFrame.rarrData[iNow]);
						else aNow=255;
						if (gbFrame.pxarrData!=null) RConvert.HsvToRgb(out rNow, out gNow, out bNow, ref gbFrame.pxarrData[iNow].H, ref gbFrame.pxarrData[iNow].S, ref gbFrame.pxarrData[iNow].Y); //RConvert.YhsToRgb(out rNow, out gNow, out bNow, gbFrame.pxarrData[iNow].Y, gbFrame.pxarrData[iNow].H, gbFrame.pxarrData[iNow].S);
						else {
							rNow=aNow;gNow=aNow;bNow=aNow;
						}
						bmpLoaded.SetPixel( xNow, yNow, Color.FromArgb(aNow,rNow,gNow,bNow) );
						iNow++;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"CopyFrameToInternalBitmap",sVerbNow);
				bGood=false;
			}
			return bGood;
		}
		public bool LoadInternalBitmap(string sFile) {
			bool bGood=true;
			try {
				if (bmpLoaded!=null) bmpLoaded.Dispose();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"LoadInternalBitmap(\""+sFile+"\")","disposing previous frame image");
			}
			try {
				bmpLoaded=new Bitmap(sFile);
				bGood=CopyFrameFromInternalBitmap();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"LoadInternalBitmap(\""+sFile+"\")","working with file type or location");
				bGood=false;
			}
			return bGood;
		}
		public bool CopyFrameFromInternalBitmap() {
			bool bGood=true;
			try {
				if (bmpLoaded==null) {
					RReporting.ShowErr("image not loaded!","CopyFrameFromInternalBitmap");
					bGood=false;
				}
				else if (gbFrame==null) {
					bGood=false;
					RReporting.ShowErr("No frame is selected, cannot continue with operation.","CopyFrameFromInternalBitmap");
				}
				else {
					gunit = GraphicsUnit.Pixel;
					rectNowF = bmpLoaded.GetBounds(ref gunit);
					rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y, (int)rectNowF.Width, (int)rectNowF.Height);
					int iPixelsNew=rectNow.Width*rectNow.Height;
					//gbFrame.iWidth!=rectNow.Width || gbFrameiHeight!=rectNow.Height
					bool bNewSize=(iPixelsNew!=gbFrame.iPixelsTotal);
					gbFrame.iWidth=rectNow.Width;//debug assumes these are the only vars that need to be changed
					gbFrame.iHeight=rectNow.Height;//debug assumes these are the only vars that need to be changed
					if (gbFrame.pxarrData==null) {
						gbFrame.pxarrData=new PixelYhs[iPixelsNew];
						if (gbFrame.rarrData==null || bNewSize) gbFrame.rarrData=new REAL[iPixelsNew];
					}
					else if (gbFrame.rarrData==null) {
						gbFrame.rarrData=new REAL[iPixelsNew];
						if (bNewSize) gbFrame.pxarrData=new PixelYhs[iPixelsNew];
					}
					else if (bNewSize) {
						gbFrame.pxarrData=new PixelYhs[iPixelsNew];
						gbFrame.rarrData=new REAL[iPixelsNew];
					}
					int iNow=0;
					Color pxNow;
					for (int yNow=0; yNow<rectNow.Height; yNow++) {
						for (int xNow=0; xNow<rectNow.Width; xNow++) {
							pxNow=bmpLoaded.GetPixel(xNow,yNow);
							gbFrame.pxarrData[iNow].FromRgb(pxNow.R,pxNow.G,pxNow.B);
							gbFrame.rarrData[iNow]=RConvert.ByteToReal(pxNow.A);
							iNow++;
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"CopyFrameFromInternalBitmap");
				bGood=false;
			}
			return bGood;
		}//end CopyFrameFromInternalBitmap
		public bool SaveInternalBitmap(string sFileName, System.Drawing.Imaging.ImageFormat imageformat) {
			bool bGood=true;
			try {
				bmpLoaded.Save(sFileName, imageformat);
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"SaveInternalBitmap(\""+sFileName+"\","+imageformat.ToString()+")");
			}
			return bGood;
		}
		/// <summary>
		/// Saves the image in the format from which it was loaded.
		/// </summary>
		/// <param name="sFileName">File name, make sure extension is same as loaded file.</param>
		/// <returns>false if exception</returns>
		public bool SaveInternalBitmap(string sFileName) {
			bool bGood=true;
			try {
				bmpLoaded.Save(sFileName);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"SaveInternalBitmap(\""+sFileName+"\")");
				bGood=false;
			}
			return bGood;
		}
		//public bool FromFrames(byte[][] by2dFrames, long lFrames, int iChannels, int iWidthNow, int iHeightNow) {
		//}
		public bool GotoFrame(long lFrameX) {
			//refers to a file if a file is used.\
			bool bGood=true;
			try {
				if (lFramesCached==lFrames) {
					gbFrame=gbarrAnim[lFrameX];
					lFrameNow=lFrameX;
				}
				else {//if ((sPathFile!=null) && (sPathFile.Length>0)) {
					RReporting.ShowErr("GotoFrame of non-cached sequence is not available in this version");//debug NYI
					//image.SelectActiveFrame(image.FrameDimensionsList[lFrameX], (int)lFrameX);
					//debug NYI load from file
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GotoFrame");
				bGood=false;
			}
			return bGood;
		}//end GotoFrame
		public bool DrawFrameOverlay(ref GBuffer gbDest, ref IPoint ipDest, long lFrame) {
			bool bGood=GotoFrame(lFrame);
			if (!DrawFrameOverlay(ref gbDest, ref ipDest)) bGood=false;
			return bGood;
		}
		public bool DrawFrameOverlay(ref GBuffer gbDest, ref IPoint ipDest) {
			return GBuffer.OverlayNoClipToBigCopyAlpha(ref gbDest, ref ipDest, ref gbFrame);
		}
		public int MinDigitsRequired(int iNumber) {
			string sNumber=iNumber.ToString();
			return sNumber.Length;
		}
		public static string PathFileOfSeqFrame(string sFileBaseName1, string sSetFileExt, long lFrameTarget, int iDigitsMin) {
			string sReturn="";
			try {
				sReturn=sFileBaseName1;
				if (iDigitsMin>0) {
					long lDivisor=RMath.SafeE10L((int)(iDigitsMin-1));//returns long since implied base is 10L
					long lDestruct=lFrameTarget;
					while (lDivisor>0) {
						long lResult=lDestruct/lDivisor;
						sReturn+=lResult.ToString();
						lDestruct-=lResult*lDivisor;
						if (lDivisor==1) lDivisor=0;
						else lDivisor/=10;
					}
				}
				else sReturn+=lFrameTarget.ToString();
				sReturn+="."+sSetFileExt;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"PathFileOfSeqFrame");
				sReturn="";
			}
			return sReturn;
		}//end PathFileOfSeqFrame
		public string PathFileOfSeqFrame(long lFrameTarget) {
			string sReturn="";
			try {
				//debug performance:
				//string sSetFileExt=sPathFile.Substring(sPathFile.LastIndexOf(".")+1);
				//int iLengthBase=(sPathFile.Length-iSeqDigitsMin)-(1+sSetFileExt.Length);
				//string sPathFileBaseName1=sPathFile.Substring(iLengthBase); //INCLUDES Path
				//sReturn=PathFileOfSeqFrame(sPathFileBaseName1, sSetFileExt, lFrameTarget, iSeqDigitsMin);
				sReturn=PathFileOfSeqFrame(sPathFileBaseName, sFileExt, lFrameTarget, iSeqDigitsMin);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"PathFileOfSeqFrame("+lFrameTarget.ToString()+")");
				sReturn="";
			}
			return sReturn;
		}
		//public bool FromGifAnim(string sFile) {
		//}
		//public bool ToGifAnim(string sFile) {
			//image.Save(,System.Drawing.Imaging.ImageFormat.Gif);
		//}
		public bool SplitFromImage32(string sFileImage, int iCellWidth, int iCellHeight, int iRows, int iColumns) {
			try {
				return SplitFromImage32(sFileImage, iCellWidth, iCellHeight, iRows, iColumns, null, null);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"SplitFromImage32");
			}
			return false;
		}
		public bool SplitFromImage32(string sFile, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
			GBuffer gbTemp;
			bool bGood=false;
			try {
				gbTemp=new GBuffer(sFile);
				if (gbTemp.iPixelsTotal==0) {
					RReporting.ShowErr("Couldn't load image","SplitFromImage32","loading font table");
					bGood=false;
				}
				else {
					//bmpLoaded=gbTemp.bmpLoaded;//SplitFromImage32 remakes bmpLoaded
					bGood=SplitFromImage32(ref gbTemp, iCellWidth, iCellHeight, iRows, iColumns, ipAsCellSpacing, izoneAsMargins);
					//sLogLine="Saving test bitmap for debug...";
					gbTemp.Save(Manager.sDataFolderSlash+"00.test bitmap for SplitFromImage32 debug.png", ImageFormat.Png);
					//gbTemp.SaveRaw("1.test bitmap for debug.raw");
					//sLogLine="Done saving test Bitmap for debug";
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"SplitFromImage32");
				bGood=false;
			}
			return bGood;
		}//end SplitFromImage32
		public bool SplitFromImage32(ref GBuffer gbSrc, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
			bool bGood=true;
			lFrames=(long)iRows*(long)iColumns;
			lFramesCached=lFrames; //so cached anim will be accessed
			string sVerbNow="creating GBuffer array";
			try {
				gbarrAnim=new GBuffer[lFrames];
				sVerbNow="creating internal Bitmap";
				bmpLoaded=new Bitmap(iCellWidth, iCellHeight, PixelFormatNow());
				sVerbNow="creating frame buffers";
				for (int lFrameX=0; lFrameX<lFrames; lFrameX++) {
					sVerbNow="creating frame buffer "+lFrameX.ToString();
					gbarrAnim[lFrameX]=new GBuffer(iCellWidth, iCellHeight, 4); //assumes 32-bit
				}
				sVerbNow="creating markers";
				if (ipAsCellSpacing==null) {
					ipAsCellSpacing=new IPoint();
				}
				if (izoneAsMargins==null) {
					izoneAsMargins=new IZone();
				}
				long lFrameLoad=0;
				GotoFrame(lFrameLoad);
				sVerbNow="checking gbFrame";
				gbFrame.pxarrData=gbFrame.pxarrData;
				sVerbNow="calculating values for markers";
				int iSrcOfCellTopLeft=izoneAsMargins.top*gbSrc.iWidth + izoneAsMargins.left;
				int iSrcOfCellNow;
				int iSrc;
				int iDest;
				int iHeight=gbFrame.iHeight;
				int iCellOffsetX=(iCellWidth+ipAsCellSpacing.X);
				int iCellOffsetY=gbSrc.iWidth*(iCellHeight+ipAsCellSpacing.Y);
				sVerbNow="checking gbFrame";
				bool bColor=(gbFrame.pxarrData!=null&&gbSrc.pxarrData!=null);
				bool bMask=(gbFrame.rarrData!=null&&gbSrc.rarrData!=null);
				sVerbNow="starting first cell";
				for (int yCell=0; yCell<iRows; yCell++) {
					for (int xCell=0; xCell<iColumns; xCell++) {
						iDest=0;
						iSrcOfCellNow=iSrcOfCellTopLeft + yCell*iCellOffsetY + xCell*iCellOffsetX;
						iSrc=iSrcOfCellNow;
						sVerbNow="Going to frame "+lFrameLoad.ToString();
						GotoFrame(lFrameLoad);
						sVerbNow="reading lines";
						for (int iLine=0; iLine<iHeight; iLine++) {
							int iDestNow=iDest;
							int iSrcNow=iSrc;
							for (int iNow=0; iNow<gbFrame.iWidth; iNow++) { //debug performance (eliminate iNow)
								if (bColor) {
									sVerbNow="accessing pixels";
									//gbFrame.pxarrData[iDestNow]=gbSrc.pxarrData[iSrcNow];//debug performance--allow fast pointer copy (this commented line)
									gbFrame.pxarrData[iDestNow].Y=gbSrc.pxarrData[iSrcNow].Y;
									gbFrame.pxarrData[iDestNow].H=gbSrc.pxarrData[iSrcNow].H;
									gbFrame.pxarrData[iDestNow].S=gbSrc.pxarrData[iSrcNow].S;
								}
								if (bMask) gbFrame.rarrData[iDestNow]=gbSrc.rarrData[iSrcNow];
								iDestNow++;
								iSrcNow++;
							}
							iDest+=gbFrame.iWidth;
							iSrc+=gbSrc.iWidth;
						}
						sVerbNow="finished frame";
						lFrameLoad++;
					}
				}
				bGood=true;
				lFramesCached=lFrames;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"SplitFromImage32","splitting ("+((gbSrc==null)?"null":"non-null "+gbSrc.Description())+") image with specified cell sizes ("+sVerbNow+")");
			}
			return bGood;
		}//end SplitFromImage32
		/// <summary>
		/// Changes the frame order from top to bottom to left to right using
		/// the idea that the frames are currently wrapped to a square whose 
		/// number of rows are specified by the number of output columns you
		/// specify.
		/// </summary>
		/// <param name="iResultCols"></param>
		/// <returns></returns>
		public bool TransposeFramesAsMatrix(int iResultRows, int iResultCols) {
			//TODO: exception handling
			bool bGood=false;
			GBuffer[] gbarrNew;
			string sDebug="starting TranslateFrameOrder"+Environment.NewLine;
			RString.StringToFile("C:\\DOCUME~1\\OWNER\\MYDOCU~1\\DATABOX\\anim.TranslateFrameOrder debug.txt", sDebug);
			try {
				gbarrNew=new GBuffer[(int)lFrames];
				int iFrames=(int)lFrames;
				for (int iFrame=0; iFrame<iFrames; iFrame++) {
					int iNew=(int)(iFrame/iResultCols)+(int)(iFrame%iResultCols)*iResultRows; //switched (must be)
					sDebug+="old:"+iFrame.ToString()+"; new:"+iNew.ToString()+Environment.NewLine;
					gbarrNew[iFrame]=gbarrAnim[iNew];
				}
				gbarrAnim=gbarrNew;
				RString.StringToFile("C:\\Documents and Settings\\Owner\\My Documents\\Databox\\anim.TranslateFrameOrder debug.txt", sDebug);
				gbFrame=gbarrAnim[this.lFrameNow];
				bGood=true;
			}
			catch (Exception exn) {
				sDebug+="Exception!"+exn.ToString()+Environment.NewLine;
				//TODO: handle exception
			}
			sDebug+="Finished.";
			//TODO: in future don't set bGood according to StringToFile
			bGood=RString.StringToFile("C:\\Documents and Settings\\Owner\\My Documents\\Databox\\anim.TranslateFrameOrder debug.txt", sDebug);
			return bGood;
		}//end TranslateFrameOrder
		
		//public GBuffer ToOneImage(int iCellW, int iCellH, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
		//public bool SplitFromImage32(ref GBuffer gbSrc, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
		public GBuffer ToOneImage(int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
			bool bGood=true;
			//int iFrames=iRows*iColumns;
			lFramesCached=lFrames; //so cached anim will be accessed
			GBuffer gbNew=null;
			try {
				//gbarrAnim=new GBuffer[lFrames];
				this.GotoFrame(0);
				gbNew=new GBuffer(iCellWidth*iColumns, iCellHeight*iRows, this.gbFrame.Channels());
				//bmpLoaded=new Bitmap(iCellWidth, iCellHeight, PixelFormatNow());
				//for (int lFrameX=0; lFrameX<lFrames; lFrameX++) {
				//	gbarrAnim[lFrameX]=new GBuffer(iCellWidth, iCellHeight, 4); //assumes 32-bit
				//}
				if (ipAsCellSpacing==null) {
					ipAsCellSpacing=new IPoint();
				}
				if (izoneAsMargins==null) {
					izoneAsMargins=new IZone();
				}
				lFrameNow=0; //TODO: use new var instead of class var
				int iDestOfCellTopLeft=izoneAsMargins.top*gbNew.iWidth + izoneAsMargins.left;
				int iDestOfCellNow;
				int iDest;
				int iSrc;
				int iHeight=gbFrame.iHeight;
				int iCellOffsetX=(iCellWidth+ipAsCellSpacing.X);
				int iCellOffsetY=gbNew.iWidth*(iCellHeight+ipAsCellSpacing.Y);
				long lFrameLoad=0;
				bool bColor=(gbFrame.pxarrData!=null&&gbNew.pxarrData!=null);
				bool bMask=(gbFrame.rarrData!=null&&gbNew.rarrData!=null);
				for (int yCell=0; yCell<iRows; yCell++) {
					for (int xCell=0; xCell<iColumns; xCell++) {
						iSrc=0;
						iDestOfCellNow=iDestOfCellTopLeft + yCell*iCellOffsetY + xCell*iCellOffsetX;
						iDest=iDestOfCellNow;
						GotoFrame(lFrameLoad);
						//gbFrame.Save("debugToOneImage"+RString.SequenceDigits(lFrameLoad)+".png",ImageFormat.Png);
						for (int iLine=0; iLine<iHeight; iLine++) {
							int iSrcNow=iSrc;
							int iDestNow=iDest;
							for (int iNow=0; iNow<gbFrame.iWidth; iNow++) {
								if (bColor) {
									gbNew.pxarrData[iDestNow].Y=gbFrame.pxarrData[iSrcNow].Y;
									gbNew.pxarrData[iDestNow].H=gbFrame.pxarrData[iSrcNow].H;
									gbNew.pxarrData[iDestNow].S=gbFrame.pxarrData[iSrcNow].S;
								}
								if (bMask) gbNew.rarrData[iDestNow]=gbFrame.rarrData[iSrcNow];
								iSrcNow++;
								iDestNow++;
							}
							iSrc+=gbFrame.iWidth;
							iDest+=gbNew.iWidth;
						}
						lFrameLoad++;
					}
				}
				bGood=true;
				lFramesCached=lFrames;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"ToOneImage","combining images to specified cell sizes");
			}
			return gbNew;
		}//end ToOneImage
	}//end class Anim;
	
	public class Clip {
		public int iParent; //index of parent Anim in animarr
		public long lFrameStart;
		public long lFrames;
		public long lFrameNow;
		public Effect[] effectarr; //remember to also process animarr[iParent].effectarr[]
		public int iEffects;
		private int iMaxEffects;
	}
	
	public class Effect {
		public const int TypeBlank = 0;
		public const int TypeAnim = 1;
		public const int TypeClip = 2;
		public const int EffectSkew=1;
		//Vars varsFX; //re-implement this
		public UInt32 bitsAttrib;
		public int iEffect;
		//Remember to prevent circular references in the following ints!!
		public int iDest;
		public int iTypeDest;
		public int iOverlay;
		public int iTypeOverlay;
		public int iMask;
		public int iTypeMask;
		public long lStartFrame;
		public long lFrames;
		public string sScript;
		public Effect Copy() {
			Effect fxReturn=null;
			try {
				fxReturn=new Effect();
				//fxReturn.varsFX=varsFX.Copy(); //TODO: re-implement this
				fxReturn.bitsAttrib=bitsAttrib;
				fxReturn.iEffect=iEffect;
				fxReturn.iDest=iDest;
				fxReturn.iTypeDest=iTypeDest;
				fxReturn.iOverlay=iOverlay;
				fxReturn.iTypeOverlay=iTypeOverlay;
				fxReturn.iMask=iMask;
				fxReturn.iTypeMask=iTypeMask;
				fxReturn.lStartFrame=lStartFrame;
				fxReturn.lFrames=lFrames;
				fxReturn.sScript=sScript;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Effect Copy");
			}
			return fxReturn;
		}//end Copy
		public Effect() {
			Init();
		}
		private void Init() {
			try {
				//varsFX=new Vars(); //TODO: re-implment FX vars
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Effect Init");
			}
		}
		//public bool FromHorzSkew(int iAnim, double dAngle, int iWidthSrc, int iHeightSrc) {
		//	varsFX.SetOrCreate("angle",dAngle);
		//	return false;
		//}
	}
}
