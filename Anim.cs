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

namespace ExpertMultimedia {
	public class Anim {
		//public int iWidth;
		//public int iHeight;
		//public int iBytesPP;
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
		private string sPathFileBaseName="";
		private string sFileExt="";
		private string sPathFileNonSeq {
			get {
				return sPathFileBaseName+"."+sFileExt;
			}
		}
		private static string sFuncNow="";//TODO: remove this
		private static string sLastErr {
			set {
				Console.Error.WriteLine(sFuncNow+": "+value);
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
					for (long l=0; l<lFrames; l++) {
						animReturn.gbarrAnim[l]=gbarrAnim[l].Copy();
					}
				}
				else sLastErr="Uncached anim not yet implemented";
				animReturn.gbFrame=animReturn.gbarrAnim[lFrameNow];
				if (iEffects>0) {
					animReturn.effectarr=new Effect[iEffects];
					for (int i=0; i<iEffects; i++) {
						animReturn.effectarr[i]=effectarr[i].Copy();
					}
				}
			}
			catch (Exception exn) {
				sFuncNow="Anim.Copy";
				sLastErr="Exception error--"+exn.ToString();
			}
			return animReturn;
		}
		public Anim CopyAsGrey() {
			return CopyAsGrey(-1);
		}
		/// <summary>
		/// Makes a new Anim object from any channel (or the value) of this one.
		/// </summary>
		/// <param name="iChannelOffset">Pick a channel to copy.  Set to -1 to take average of RGB.</param>
		/// <returns></returns>
		public Anim CopyAsGrey(int iChannelOffset) {
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
					if (iChannelOffset<0) {
						for (long l=0; l<lFrames; l++) {
							GBuffer.MaskFromValue(ref animReturn.gbarrAnim[l], ref gbarrAnim[l]);
						}
					}
					else {
						for (long l=0; l<lFrames; l++) {
							GBuffer.MaskFromChannel(ref animReturn.gbarrAnim[l], ref gbarrAnim[l], iChannelOffset);
						}
					}
				}
				else sLastErr="Uncached Anim not yet implemented";
				animReturn.gbFrame=animReturn.gbarrAnim[lFrameNow];
				if (iEffects>0) {
					animReturn.effectarr=new Effect[iEffects];
					for (int i=0; i<iEffects; i++) {
						animReturn.effectarr[i]=effectarr[i].Copy();
					}
				}
			}
			catch (Exception exn) {
				sFuncNow="Copy";
				sLastErr="Exception error--"+exn.ToString();
			}
			return animReturn;
		}
		public bool SaveSeq(string sFileBase, string sFileExt1) {
			bool bGood=true;
			try { //first write debug file
				
				sFileExt=sFileExt1;
				sPathFileBaseName=sFileBase;
				Base.StringToFile(sFileBase+".txt", ToString(true));
			}
			catch (Exception exn) {
				sLastErr="Exception - can't save sequence info file--"+exn.ToString();
			}
			for (long lFrameSave=0; lFrameSave<lFrames; lFrameSave++) {
				if (false==SaveSeqFrame(lFrameSave)) {
					bGood=false;
					sFuncNow="SaveSeq";
					sLastErr="Can't save "+PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameNow, iSeqDigitsMin);
				}
			}
			return bGood;
		}
		public string ToString(bool bDumpVars) {
			if (bDumpVars) {
				string sReturn="";
				try {
					sReturn="{"
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
						sReturn+="; gbFrame.iBytesTotal:"+gbFrame.iBytesTotal
							+"; gbFrame.iWidth:"+gbFrame.iWidth
							+"; gbFrame.iHeight:"+gbFrame.iHeight
							+"; gbFrame.iWidth:"+gbFrame.iWidth
							+"; gbFrame.iBytesPP:"+gbFrame.iBytesPP
							+"; gbFrame.iStride:"+gbFrame.iStride;
						}
						catch (Exception exn) {
							sFuncNow="ToString(true)";
							sLastErr="Exception accessing gbFrame--"+exn.ToString();
						}
					}
					sReturn+=";}";
				}
				catch (Exception exn) {
					sFuncNow="ToString("+bDumpVars.ToString()+")";
					sLastErr="Exception error, can't dump vars--"+exn.ToString();
					sReturn="{Exception error dumping anim vars}";
				}
				return sReturn;
			}
			else return ToString();
		}
		public bool SaveCurrentSeqFrame() {
			bool bGood=CopyFrameToInternalBitmap();
			if (bGood) bGood=SaveInternalBitmap(PathFileOfSeqFrame(sPathFileBaseName, sFileExt, lFrameNow, iSeqDigitsMin));
			return bGood;
		}
		public bool SaveSeqFrame(long lFrameSave) {
			return SaveSeqFrame(sPathFileBaseName, sFileExt, lFrameSave);
		}
		public bool SaveSeqFrame(string sFileBase, string sFileExt1, long lFrameSave) {
			bool bGood=true;
			if (false==GotoFrame(lFrameSave)) {
				bGood=false;
				sLastErr="Failed to goto frame "+lFrameSave.ToString()+" of Anim";
			}
			else {
				bGood=CopyFrameToInternalBitmap();
				if (bGood) {
					sFileExt=sFileExt1;
					bGood=SaveInternalBitmap(PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameSave, iSeqDigitsMin), ImageFormatFromExt());
					if (bGood==false) {
						sFuncNow="SaveCurrentSeqFrame";
						sLastErr="Failed to save "+PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameSave, iSeqDigitsMin);
					}
				}
				else sLastErr="Failed to copy data to frame image";
				
			}
			return bGood;
		}
		public ImageFormat ImageFormatFromExt() {
			return ImageFormatFromExt(sFileExt);
		}
		public ImageFormat ImageFormatFromExt(string sFileExt1) {
			if (sFileExt1.ToLower()=="png") return ImageFormat.Png;
			if (sFileExt1.ToLower()=="jpg") return ImageFormat.Jpeg;
			if (sFileExt1.ToLower()=="gif") return ImageFormat.Gif;
			if (sFileExt1.ToLower().StartsWith("jpe")) return ImageFormat.Jpeg;
			if (sFileExt1.ToLower().StartsWith("exi")) return ImageFormat.Exif;
			if (sFileExt1.ToLower()=="emf") return ImageFormat.Emf;
			if (sFileExt1.ToLower()=="tif") return ImageFormat.Tiff;
			if (sFileExt1.ToLower()=="ico") return ImageFormat.Icon;
			if (sFileExt1.ToLower()=="wmf") return ImageFormat.Wmf;
			else return ImageFormat.Bmp;
		}
		public bool ResetBitmapUsingFrameNow() {
			bool bGood=true;
			try {
				bmpLoaded=new Bitmap(gbFrame.iWidth, gbFrame.iHeight, PixelFormatNow());
				//bmpLoaded.Save(RetroEngine.sDataFolderSlash+"1.test-blank.png", ImageFormat.Png);
				//sLogLine="Saved blank test PNG for Bitmap debug";
			}
			catch (Exception exn) {
				sFuncNow="ResetBitmapUsingFrameNow";
				bGood=false;
				sLastErr="Exception error, can't initialize image--"+exn.ToString();
			}
			return bGood;
		}
		public PixelFormat PixelFormatNow() {
			return pixelformatNow;
		}
		public unsafe bool CopyFrameToInternalBitmap() {
			bool bGood=true;
			try {
				if (bmpLoaded==null) bGood=ResetBitmapUsingFrameNow();
				if (bGood==false) sLastErr="Failed to reset internal frame image";
				gunit = GraphicsUnit.Pixel;
				rectNowF = bmpLoaded.GetBounds(ref gunit);
				rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
									(int)rectNowF.Width, (int)rectNowF.Height);
				bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.WriteOnly, PixelFormatNow());
				if (  (gbFrame.iStride!=bmpdata.Stride)
				   || (gbFrame.iWidth!=rectNow.Width)
				   || (gbFrame.iHeight!=rectNow.Height)
				   || (gbFrame.iBytesPP!=bmpdata.Stride/rectNow.Width)
				   || (gbFrame.iBytesTotal!=(bmpdata.Stride*rectNow.Height)) ) {
					gbFrame.iStride=bmpdata.Stride;
					gbFrame.iWidth=rectNow.Width;
					gbFrame.iHeight=rectNow.Height;
					gbFrame.iBytesPP=gbFrame.iStride/gbFrame.iWidth;
					gbFrame.iBytesTotal=gbFrame.iStride*gbFrame.iHeight;
					gbFrame.byarrData=new byte[gbFrame.iBytesTotal];
				}
				byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
				for (int iBy=0; iBy<gbFrame.iBytesTotal; iBy++) {
					*lpbyNow=gbFrame.byarrData[iBy];
					lpbyNow++;
				}
				bmpLoaded.UnlockBits(bmpdata);
			}
			catch (Exception exn) {
				sFuncNow="CopyFrameToInternalBitmap()";
				sLastErr="Exception Error--"+exn.ToString();
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
				sFuncNow="LoadInternalBitmap(\""+sFile+"\")";
				sLastErr="Exception error disposing previous frame image--"+exn.ToString();
			}
			try {
				bmpLoaded=new Bitmap(sFile);
				bGood=CopyFrameFromInternalBitmap();
			}
			catch (Exception exn) {
				sFuncNow="LoadInternalBitmap(\""+sFile+"\")";
				sLastErr="Exception Error, problem with file type or location--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public unsafe bool CopyFrameFromInternalBitmap() {
			bool bGood=true;
			try {
				gunit = GraphicsUnit.Pixel;
				rectNowF = bmpLoaded.GetBounds(ref gunit);
				rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
									(int)rectNowF.Width, (int)rectNowF.Height);
				bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.ReadOnly, PixelFormatNow());
				if (  (gbFrame.iStride!=bmpdata.Stride)
				   || (gbFrame.iWidth!=rectNow.Width)
				   || (gbFrame.iHeight!=rectNow.Height)
				   || (gbFrame.iBytesPP!=bmpdata.Stride/rectNow.Width)
				   || (gbFrame.iBytesTotal!=(bmpdata.Stride*rectNow.Height)) ) {
					gbFrame.iStride=bmpdata.Stride;
					gbFrame.iWidth=rectNow.Width;
					gbFrame.iHeight=rectNow.Height;
					gbFrame.iBytesPP=gbFrame.iStride/gbFrame.iWidth;
					gbFrame.iBytesTotal=gbFrame.iStride*gbFrame.iHeight;
					gbFrame.byarrData=new byte[gbFrame.iBytesTotal];
				}
				byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
				for (int iBy=0; iBy<gbFrame.iBytesTotal; iBy++) {
					gbFrame.byarrData[iBy]=*lpbyNow;
					lpbyNow++;
				}
				bmpLoaded.UnlockBits(bmpdata);
			}
			catch (Exception exn) {
				sFuncNow="CopyFrameFromInternalBitmap()";
				sLastErr="Exception Error--"+exn.ToString();
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
				sFuncNow="SaveInternalBitmap(\""+sFileName+"\","+imageformat.ToString()+")";
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
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
				sFuncNow="SaveInternalBitmap(\""+sFileName+"\")";
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		//public bool FromFrames(byte[][] by2dFrames, long lFrames, int iBytesPPNow, int iWidthNow, int iHeightNow) {
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
					sLastErr="GotoFrame of non-cached sequence is not available in this version";//debug NYI
					//image.SelectActiveFrame(image.FrameDimensionsList[lFrameX], (int)lFrameX);
					//debug NYI load from file
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}//end GotoFrame
		public bool DrawFrameOverlay(ref GBuffer gbDest, ref IPoint ipDest, long lFrame) {
			bool bGood=GotoFrame(lFrame);
			if (DrawFrameOverlay(ref gbDest, ref ipDest)==false) bGood=false;
			return bGood;
		}
		public bool DrawFrameOverlay(ref GBuffer gbDest, ref IPoint ipDest) {
			return GBuffer.OverlayNoClipToBigCopyAlpha(ref gbDest, ref ipDest, ref gbFrame);
		}
		public int MinDigitsRequired(int iNumber) {
			string sNumber=iNumber.ToString();
			return sNumber.Length;
		}
		public static string PathFileOfSeqFrame(string sFileBaseName1, string sFileExt1, long lFrameTarget, int iDigitsMin) {
			string sReturn="";
			try {
				sReturn=sFileBaseName1;
				if (iDigitsMin>0) {
					long lDivisor=Base.SafeE10L((int)(iDigitsMin-1));//returns long since implied base is 10L
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
				sReturn+="."+sFileExt1;
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				sReturn="";
			}
			return sReturn;
		}
		public string PathFileOfSeqFrame(long lFrameTarget) {
			string sReturn="";
			try {
				//debug performance:
				//string sFileExt1=sPathFile.Substring(sPathFile.LastIndexOf(".")+1);
				//int iLengthBase=(sPathFile.Length-iSeqDigitsMin)-(1+sFileExt1.Length);
				//string sPathFileBaseName1=sPathFile.Substring(iLengthBase); //INCLUDES Path
				//sReturn=PathFileOfSeqFrame(sPathFileBaseName1, sFileExt1, lFrameTarget, iSeqDigitsMin);
				sReturn=PathFileOfSeqFrame(sPathFileBaseName, sFileExt, lFrameTarget, iSeqDigitsMin);
			}
			catch (Exception exn) {
				sFuncNow="PathFileOfSeqFrame("+lFrameTarget.ToString()+")";
				sLastErr="Exception error--"+exn.ToString();
				sReturn="";
			}
			return sReturn;
		}
		//public bool FromGifAnim(string sFile) {
		//}
		//public bool ToGifAnim(string sFile) {
			//image.Save(,System.Drawing.Imaging.ImageFormat.Gif);
		//}
		public bool SplitFromImage32(string sFile, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IRect irectAsMargins) {
			GBuffer gbTemp;
			bool bGood=false;
			try {
				gbTemp=new GBuffer(sFile);
				if (gbTemp.iBytesTotal==0) {
					sLastErr="Failed to write font file to GBuffer";
					bGood=false;
				}
				else {
					//bmpLoaded=gbTemp.bmpLoaded;//SplitFromImage32 remakes bmpLoaded
					bGood=SplitFromImage32(ref gbTemp, iCellWidth, iCellHeight, iRows, iColumns, ipAsCellSpacing, irectAsMargins);
					//sLogLine="Saving test bitmap for debug...";
					//gbTemp.Save("1.test bitmap for debug.tif", ImageFormat.Tiff);
					//gbTemp.SaveRaw("1.test bitmap for debug.raw");
					//sLogLine="Done saving test Bitmap for debug";
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool SplitFromImage32(string sFileImage, int iCellWidth, int iCellHeight, int iRows, int iColumns) {
			try {
				return SplitFromImage32(sFileImage, iCellWidth, iCellHeight, iRows, iColumns, null, null);
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
			}
			return false;
		}
		public bool SplitFromImage32(ref GBuffer gbSrc, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IRect irectAsMargins) {
			bool bGood=true;
			sFuncNow="SplitFromImage32(...)";
			lFrames=(long)iRows*(long)iColumns;
			lFramesCached=lFrames; //so cached anim will be accessed
			try {
				gbarrAnim=new GBuffer[lFrames];
				bmpLoaded=new Bitmap(iCellWidth, iCellHeight, PixelFormatNow());
				for (int lFrameX=0; lFrameX<lFrames; lFrameX++) {
					gbarrAnim[lFrameX]=new GBuffer(iCellWidth, iCellHeight, 4); //assumes 32-bit
				}
				if (ipAsCellSpacing==null) {
					ipAsCellSpacing=new IPoint();
				}
				if (irectAsMargins==null) {
					irectAsMargins=new IRect();
				}
				//lFrameNow=0; //not used
				int iSrcByteOfCellTopLeft=irectAsMargins.top*gbSrc.iStride + irectAsMargins.left*gbSrc.iBytesPP;
				int iSrcByteOfCellNow;
				//int iSrcAdder=gbSrc.iStride-gbSrc.iBytesPP*iCellWidth;
				//int iSrcNextCellAdder=
				//int iSrcStride=iColumns*iWidth*4; //assumes 32-bit source
				int iSrcByte;
				int iDestByte;
				//int iCellNow=0;
				//int iCellStride=iWidth*iBytesPP;
				//int yStrideAdder=iSrcStride*(iHeight-1);
				//int iSrcAdder=iSrcStride-iWidth*iBytesPP;
				int iDestStride=gbarrAnim[0].iStride;
				int iHeight=gbarrAnim[0].iHeight;
				int iSrcStride=gbSrc.iStride;
				int iCellPitchX=gbSrc.iBytesPP*(iCellWidth+ipAsCellSpacing.x);
				int iCellPitchY=gbSrc.iStride*(iCellHeight+ipAsCellSpacing.y);
				long lFrameLoad=0;
				for (int yCell=0; yCell<iRows; yCell++) {
					for (int xCell=0; xCell<iColumns; xCell++) {
						iDestByte=0;
						iSrcByteOfCellNow=iSrcByteOfCellTopLeft + yCell*iCellPitchY + xCell*iCellPitchX;
						iSrcByte=iSrcByteOfCellNow;
						GotoFrame(lFrameLoad);
						for (int iLine=0; iLine<iHeight; iLine++) {
							if (Byter.CopyFast(ref gbFrame.byarrData, ref gbSrc.byarrData, iDestByte, iSrcByte, iDestStride)==false)
								bGood=false;
							iDestByte+=iDestStride;
							iSrcByte+=iSrcStride;
						}
						lFrameLoad++;
					}
				}
				if (bGood==false) sLastErr="There was data copy error while interpreting the GBuffer to a font, make sure the \"iCellWidth\" etc. variables are set correctly.";
				//else bGood=GreyMapFromPixMapChannel(3);
				bGood=true;
				lFramesCached=lFrames;
			}
			catch (Exception exn) {
				sLastErr="Exception error, failed to split image; specified cell sizes may be incorrect for this image--"+exn.ToString();
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
			string sDebug="starting TranslateFrameOrder()"+Environment.NewLine;
			Base.StringToFile("C:\\DOCUME~1\\OWNER\\MYDOCU~1\\DATABOX\\anim.TranslateFrameOrder debug.txt", sDebug);
			try {
				gbarrNew=new GBuffer[(int)lFrames];
				int iFrames=(int)lFrames;
				for (int iFrame=0; iFrame<iFrames; iFrame++) {
					int iNew=(int)(iFrame/iResultCols)+(int)(iFrame%iResultCols)*iResultRows; //switched (must be)
					sDebug+="old:"+iFrame.ToString()+"; new:"+iNew.ToString()+Environment.NewLine;
					gbarrNew[iFrame]=gbarrAnim[iNew];
				}
				gbarrAnim=gbarrNew;
				Base.StringToFile("C:\\Documents and Settings\\Owner\\My Documents\\Databox\\anim.TranslateFrameOrder debug.txt", sDebug);
				gbFrame=gbarrAnim[this.lFrameNow];
				bGood=true;
			}
			catch (Exception exn) {
				sDebug+="Exception!"+exn.ToString()+Environment.NewLine;
				//TODO: handle exception
			}
			sDebug+="Finished.";
			//TODO: in future don't set bGood according to StringToFile
			bGood=Base.StringToFile("C:\\Documents and Settings\\Owner\\My Documents\\Databox\\anim.TranslateFrameOrder debug.txt", sDebug);
			return bGood;
		}//end TranslateFrameOrder
		
		//public GBuffer ToOneImage(int iCellW, int iCellH, int iRows, int iColumns, IPoint ipAsCellSpacing, IRect irectAsMargins) {
		//public bool SplitFromImage32(ref GBuffer gbSrc, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IRect irectAsMargins) {
		public GBuffer ToOneImage(int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IRect irectAsMargins) {
			bool bGood=true;
			sFuncNow="ToOneImage(...)";
			int iFrames=iRows*iColumns;
			lFramesCached=lFrames; //so cached anim will be accessed
			GBuffer gbNew=null;
			try {
				//gbarrAnim=new GBuffer[lFrames];
				this.GotoFrame(0);
				gbNew=new GBuffer(iCellWidth*iColumns, iCellHeight*iRows, this.gbFrame.iBytesPP);
				//bmpLoaded=new Bitmap(iCellWidth, iCellHeight, PixelFormatNow());
				//for (int lFrameX=0; lFrameX<lFrames; lFrameX++) {
				//	gbarrAnim[lFrameX]=new GBuffer(iCellWidth, iCellHeight, 4); //assumes 32-bit
				//}
				if (ipAsCellSpacing==null) {
					ipAsCellSpacing=new IPoint();
				}
				if (irectAsMargins==null) {
					irectAsMargins=new IRect();
				}
				lFrameNow=0; //TODO: use new var instead of class var
				int iDestByteOfCellTopLeft=irectAsMargins.top*gbNew.iStride + irectAsMargins.left*gbNew.iBytesPP;
				int iDestByteOfCellNow;
				int iDestByte;
				int iSrcByte;
				int iSrcStride=gbFrame.iStride;
				int iHeight=gbFrame.iHeight;
				int iDestStride=gbNew.iStride;
				int iCellPitchX=gbNew.iBytesPP*(iCellWidth+ipAsCellSpacing.x);
				int iCellPitchY=gbNew.iStride*(iCellHeight+ipAsCellSpacing.y);
				long lFrameLoad=0;
				for (int yCell=0; yCell<iRows; yCell++) {
					for (int xCell=0; xCell<iColumns; xCell++) {
						iSrcByte=0;
						iDestByteOfCellNow=iDestByteOfCellTopLeft + yCell*iCellPitchY + xCell*iCellPitchX;
						iDestByte=iDestByteOfCellNow;
						GotoFrame(lFrameLoad);
						//gbFrame.Save("debugToOneImage"+Base.SequenceDigits(lFrameLoad)+".png",ImageFormat.Png);
						for (int iLine=0; iLine<iHeight; iLine++) {
							if (Byter.CopyFast(ref gbNew.byarrData, ref gbFrame.byarrData, iDestByte, iSrcByte, iSrcStride)==false)
								bGood=false;
							iSrcByte+=iSrcStride;
							iDestByte+=iDestStride;
						}
						lFrameLoad++;
					}
				}
				if (bGood==false) sLastErr="There was data copy error while interpreting the GBuffer to a font, make sure the \"iCellWidth\" etc. variables are set correctly.";
				//else bGood=GreyMapFromPixMapChannel(3);
				bGood=true;
				lFramesCached=lFrames;
			}
			catch (Exception exn) {
				sLastErr="Exception error, failed to split image; specified cell sizes may be incorrect for this image--"+exn.ToString();
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
		private static string sFuncNow="";//TODO: remove this
		private static string sLastErr {
			set {
				Console.Error.WriteLine(sFuncNow+": "+value);
			}
		}
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
				sFuncNow="Effect Copy";
				sLastErr="Exception error--"+exn.ToString();
			}
			return fxReturn;
		}
		public Effect() {
			Init();
		}
		private void Init() {
			try {
				//varsFX=new Vars(); //TODO: re-implment FX vars
			}
			catch (Exception exn) {}
		}
		//public bool FromHorzSkew(int iAnim, double dAngle, int iWidthSrc, int iHeightSrc) {
		//	varsFX.SetOrCreate("angle",dAngle);
		//	return false;
		//}
	}
}
