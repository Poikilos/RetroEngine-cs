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
	public class Anim32BGRA {
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
		private BitmapData bmpdata=null;
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
		public long lFrameNow=0;
		public long lFrames=0;
		public int iEffects=0;
		public Image imageOrig=null;
		private int iMaxEffects=0;
		private PixelFormat pixelformatNow=PixelFormat.Format32bppArgb;//TODO: debug assumes 32-bit
		/// <summary>
		/// only full if lFramesCached==lFrames
		/// </summary>
		private GBuffer32BGRA[] gbarrAnim=null;
		//public byte[] byarrFrame;
		public GBuffer32BGRA gbFrame=null;
		//private byte[][] by2dMask;
		//public byte[] byarrMask;
		public Effect[] effectarr=null;
		#region constructors
		public Anim32BGRA Copy() {
			Anim32BGRA animReturn=null;
			string sVerbNow="copying anim frame bitmap";
			try {
				animReturn=new Anim32BGRA();
				if (bmpLoaded!=null) animReturn.bmpLoaded=(Bitmap)bmpLoaded.Clone();
				animReturn.gunit=gunit;
				sVerbNow="getting anim rect while copying anim"; 
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
				sVerbNow="copying anim frames";
				if (lFramesCached==lFrames) {
					if (lFramesCached>0) {
						if (gbarrAnim!=null) {
							animReturn.gbarrAnim=new GBuffer32BGRA[lFramesCached];
							for (long lNow=0; lNow<lFramesCached; lNow++) {
								sVerbNow="copying anim frame ["+lNow.ToString()+"] (in "+lFramesCached+"-frame animation)";
								animReturn.gbarrAnim[lNow]=gbarrAnim[lNow].Copy();
							}
						}
						else Base.ShowErr("Copying null animation ("+lFramesCached+" frames).");
					}
					else Base.ShowErr("Copying "+lFramesCached+"-frame animation.");
				}
				else Base.ShowErr("Copying an uncached video is not yet implemented","Anim32BGRA Copy"); //nyi
				sVerbNow="copying anim current frame";
				animReturn.GotoFrame(lFrameNow);
				sVerbNow="copying anim effects";
				if (iEffects>0) {
					animReturn.effectarr=new Effect[iEffects];
					for (int i=0; i<iEffects; i++) {
						animReturn.effectarr[i]=effectarr[i].Copy();
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA Copy",sVerbNow);
			}
			return animReturn;
		}
		public Anim32BGRA CopyAsGray() {
			return CopyAsGray(-1);
		}
		/// <summary>
		/// Makes a new Anim32BGRA object from any channel (or the value) of this one.
		/// </summary>
		/// <param name="iChannelOffset">Pick a channel to copy.  Set to -1 to take average of RGB.</param>
		/// <returns></returns>
		public Anim32BGRA CopyAsGray(int iChannelOffset) {
			Anim32BGRA animReturn=null;
			try {
				animReturn=new Anim32BGRA();
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
					animReturn.gbarrAnim=new GBuffer32BGRA[lFramesCached];
					if (iChannelOffset<0) {
						for (long l=0; l<lFrames; l++) {
							GBuffer32BGRA.MaskFromValue(ref animReturn.gbarrAnim[l], ref gbarrAnim[l]);
						}
					}
					else {
						for (long l=0; l<lFrames; l++) {
							GBuffer32BGRA.MaskFromChannel(ref animReturn.gbarrAnim[l], ref gbarrAnim[l], iChannelOffset);
						}
					}
				}
				else Base.ShowErr("Copying an uncached video to gray is not yet implemented","Anim32BGRA CopyAsGray");
				animReturn.gbFrame=animReturn.gbarrAnim[lFrameNow];
				if (iEffects>0) {
					animReturn.effectarr=new Effect[iEffects];
					for (int i=0; i<iEffects; i++) {
						animReturn.effectarr[i]=effectarr[i].Copy();
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA CopyAsGray","copying animation to grayscale mask");
			}
			return animReturn;
		}//end CopyAsGray
		///<summary>
		///Detects sub-images inside an image using at least one full column of transparent pixels of height iCellHeigt;
		///</summary>
		//public static int iDebug=0;
		public bool SplitFromFixedHeightStaggered(GBuffer32BGRA gbSrc, int iCellHeight) {
			bool bGood=false;
			//Base.ShowErr("SplitFromFixedHeightStaggered is Not Yet Implemented");
			
			GBuffer32BGRAStack gbstackNow=new GBuffer32BGRAStack(256);
			int iRows=Base.ICeiling((double)gbSrc.iHeight/(double)iCellHeight);
			int iLocLine=0;
			int iSrc=iLocLine;
			int iLine=0;
			int x=0,y=0;
			int xStartNow=0;
			bool bFoundStart=false;
			//string sDebugFile="0.SplitFromFixedHeightStaggered"+iDebug.ToString()+".log.txt";
			//string sDebugZonesFile="0.SplitFromFixedHeightStaggered"+iDebug.ToString()+".Zones.log.txt";
			//string sDebugCharsBase="/home/Owner/Projects/RetroEngine/bin/fonts/etc/font"+Base.sarrAlphabetUpper[iDebug];
			//iDebug++;
			//Base.SafeDelete(sDebugFile);
			//Base.SafeDelete(sDebugZonesFile);
			//GBuffer32BGRA gbTemp=gbSrc.Copy();
			for (int iRow=0; iRow<iRows&&iLocLine<gbSrc.iBytesTotal; iRow++) {
				x=0;
				iSrc=iLocLine;
				if (y+iCellHeight>gbSrc.iHeight) {
					//fixes if has odd end
					Base.WriteLine("Warning: fixed odd end in SplitFromFixedHeightStaggered ("+gbSrc.iWidth.ToString()+"x"+gbSrc.iHeight.ToString()+" image's height is not evenly divisible by "+iCellHeight.ToString()+")");
					iCellHeight=gbSrc.iHeight-y;
				}
				while (x<gbSrc.iWidth) {
					//bFoundStart=false;
					while (x<gbSrc.iWidth&&gbSrc.IsTransparentVLine(x,y,iCellHeight)) {
						//Base.AppendForeverWrite(sDebugFile,"0");//debug only
						x++;
					}
					xStartNow=x;
					if (xStartNow<gbSrc.iWidth) {
						//bFoundStart=true;
						while (x<gbSrc.iWidth&&!gbSrc.IsTransparentVLine(x,y,iCellHeight)) {
							//Base.AppendForeverWrite(sDebugFile,"1");//debug only
							x++;
						}
						
						//gbTemp.SetPixelG(xStartNow,y,255);//debug only
						//gbTemp.SetPixelR(x-1,(y+iCellHeight)-1,255);//debug only
						GBuffer32BGRA gbChar=gbSrc.CreateFromZoneEx(xStartNow,y,x,y+iCellHeight);
						//gbChar.Save(sDebugCharsBase+Base.FixedWidth(gbstackNow.Count.ToString(),3,"0")+".png"); //debug only
						gbstackNow.Push(gbChar);
						//Base.AppendForeverWrite(sDebugZonesFile,"("+Base.FixedWidth(xStartNow.ToString(),3)+","+Base.FixedWidth(y.ToString(),3)+","+Base.FixedWidth(x.ToString(),3)+","+Base.FixedWidth((y+iCellHeight).ToString(),3)+")");
						
						
					}
					//Base.AppendForeverWrite(sDebugFile,IZone.Description(xStartNow,y,x,y+iCellHeight));
				}
				y+=iCellHeight;//y++;
				iLocLine+=gbSrc.iStride*iCellHeight;//gbSrc.iStride;
				//Base.AppendForeverWriteLine(sDebugFile);
				//Base.AppendForeverWriteLine(sDebugZonesFile);
			}
			//gbTemp.Save(sDebugZonesFile+".png");//debug only
			if (gbstackNow.Count>0) bGood=true;
			lFrames=(long)gbstackNow.Count;
			lFramesCached=lFrames;
			gbarrAnim=gbstackNow.ToArrayByReferences();
			return bGood;
		}//end SplitFromFixedHeightStaggered
		public bool SplitFromImage32(ref GBuffer32BGRA gbSrc, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
			bool bGood=true;
			lFrames=(long)iRows*(long)iColumns;
			lFramesCached=lFrames; //so cached anim will be accessed
			try {
				gbarrAnim=new GBuffer32BGRA[lFrames];
				bmpLoaded=new Bitmap(iCellWidth, iCellHeight, PixelFormatNow());
				for (int lFrameX=0; lFrameX<lFrames; lFrameX++) {
					gbarrAnim[lFrameX]=new GBuffer32BGRA(iCellWidth, iCellHeight, 4); //assumes 32-bit
				}
				if (ipAsCellSpacing==null) {
					ipAsCellSpacing=new IPoint();
				}
				if (izoneAsMargins==null) {
					izoneAsMargins=new IZone();
				}
				//lFrameNow=0; //not used
				int iSrcByteOfCellTopLeft=izoneAsMargins.Top*gbSrc.iStride + izoneAsMargins.Left*gbSrc.iBytesPP;
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
				int iCellPitchX=gbSrc.iBytesPP*(iCellWidth+ipAsCellSpacing.X);
				int iCellPitchY=gbSrc.iStride*(iCellHeight+ipAsCellSpacing.Y);
				long lFrameLoad=0;
				for (int yCell=0; yCell<iRows; yCell++) {
					for (int xCell=0; xCell<iColumns; xCell++) {
						iDestByte=0;
						iSrcByteOfCellNow=iSrcByteOfCellTopLeft + yCell*iCellPitchY + xCell*iCellPitchX;
						iSrcByte=iSrcByteOfCellNow;
						GotoFrame(lFrameLoad);
						for (int iLine=0; iLine<iHeight; iLine++) {
							if (Memory.CopyFast(ref gbFrame.byarrData, ref gbSrc.byarrData, iDestByte, iSrcByte, iDestStride)==false)
								bGood=false;
							iDestByte+=iDestStride;
							iSrcByte+=iSrcStride;
						}
						lFrameLoad++;
					}
				}
				if (!bGood) Base.ShowErr("There was data copy error while splitting the image with the specified cell sizes","Anim32BGRA SplitFromImage32");
				//else bGood=GrayMapFromPixMapChannel(3);
				bGood=true;
				lFramesCached=lFrames;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA SplitFromImage32","splitting image with specified cell sizes");
			}
			return bGood;
		}//end SplitFromImage32
		#endregion constructors
		
		#region file operations
		public bool SaveSeq(string sFileBase, string sFileExt1) {
			bool bGood=true;
			try { //first write debug file
				sFileExt=sFileExt1;
				sPathFileBaseName=sFileBase;
				Base.StringToFile(sFileBase+".txt", ToString(true));
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA SaveSeq","saving sequence info file");
			}
			for (long lFrameSave=0; lFrameSave<lFrames; lFrameSave++) {
				if (!SaveSeqFrame(lFrameSave)) {
					bGood=false;
					Base.ShowErr("Couldn't save sequence frame", "Anim32BGRA SaveSeq("+PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameNow, iSeqDigitsMin)+")");
				}
			}
			return bGood;
		}
		public bool SaveSeqFrame(long lFrameSave) {
			return SaveSeqFrame(sPathFileBaseName, sFileExt, lFrameSave);
		}
		public bool SaveSeqFrame(string sFileBase, string sFileExt1, long lFrameSave) {
			bool bGood=true;
			if (!GotoFrame(lFrameSave)) {
				bGood=false;
				Base.ShowErr("Error advancing to frame "+lFrameSave.ToString(), "Anim32BGRA SaveSeqFrame");
			}
			else {
				bGood=CopyFrameToInternalBitmap();
				if (bGood) {
					sFileExt=sFileExt1;
					bGood=SaveInternalBitmap(PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameSave, iSeqDigitsMin), ImageFormatFromExt());
					if (!bGood) {
						Base.ShowErr("Failed to save "+PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameSave, iSeqDigitsMin), "Anim32BGRA SaveSeqFrame");
					}
				}
				else Base.ShowErr("Failed to copy data to frame image","Anim32BGRA SaveSeqFrame");
			}
			return bGood;
		}
		public bool SaveInternalBitmap(string sFileName, System.Drawing.Imaging.ImageFormat imageformat) {
			bool bGood=true;
			try {
				bmpLoaded.Save(sFileName, imageformat);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA SaveInternalBitmap(\""+sFileName+"\","+imageformat.ToString()+")");
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
				Base.ShowExn(exn,"Anim32BGRA SaveInternalBitmap(\""+sFileName+"\")");
				bGood=false;
			}
			return bGood;
		}
		public bool SplitFromImage32(string sFile, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
			GBuffer32BGRA gbTemp;
			bool bGood=false;
			try {
				gbTemp=new GBuffer32BGRA(sFile);
				if (gbTemp.iBytesTotal==0) {
					Base.ShowErr("Could not find data in image","Anim32BGRA SplitFromImage32("+sFile+",...)","checking loaded font graphic size");
					bGood=false;
				}
				else {
					//bmpLoaded=gbTemp.bmpLoaded;//SplitFromImage32 remakes bmpLoaded
					bGood=SplitFromImage32(ref gbTemp, iCellWidth, iCellHeight, iRows, iColumns, ipAsCellSpacing, izoneAsMargins);
					//sLogLine="Saving test bitmap for debug...";
					//gbTemp.Save("1.test bitmap for debug.tif", ImageFormat.Tiff);
					//gbTemp.SaveRaw("1.test bitmap for debug.raw");
					//sLogLine="Done saving test Bitmap for debug";
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA SplitFromImage32");
				bGood=false;
			}
			return bGood;
		}//end SplitFromImage32 file overload
		public bool SplitFromImage32(string sFileImage, int iCellWidth, int iCellHeight, int iRows, int iColumns) {
			return SplitFromImage32(sFileImage, iCellWidth, iCellHeight, iRows, iColumns, null, null);
		}//end SplitFromImage32 nospacing overload
		/// <summary>
		/// Changes the frame order from Top to bottom to Left to right using
		/// the idea that the frames are currently wrapped to a square whose 
		/// number of rows are specified by the number of output columns you
		/// specify.
		/// </summary>
		/// <param name="iResultCols"></param>
		/// <returns></returns>
		public bool TransposeFramesAsMatrix(int iResultRows, int iResultCols) {
			//TODO: exception handling
			bool bGood=false;
			GBuffer32BGRA[] gbarrNew;
			string sDebug="starting TranslateFrameOrder()"+Environment.NewLine;
			Base.StringToFile("C:\\DOCUME~1\\OWNER\\MYDOCU~1\\DATABOX\\anim.TranslateFrameOrder debug.txt", sDebug);
			try {
				gbarrNew=new GBuffer32BGRA[(int)lFrames];
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
		
		//public GBuffer32BGRA ToOneImage(int iCellW, int iCellH, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
		//public bool SplitFromImage32(ref GBuffer32BGRA gbSrc, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
		public GBuffer32BGRA ToOneImage(int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
			bool bGood=true;
			int iFrames=iRows*iColumns;
			lFramesCached=lFrames; //so cached anim will be accessed
			GBuffer32BGRA gbNew=null;
			try {
				//gbarrAnim=new GBuffer32BGRA[lFrames];
				this.GotoFrame(0);
				gbNew=new GBuffer32BGRA(iCellWidth*iColumns, iCellHeight*iRows, this.gbFrame.iBytesPP);
				//bmpLoaded=new Bitmap(iCellWidth, iCellHeight, PixelFormatNow());
				//for (int lFrameX=0; lFrameX<lFrames; lFrameX++) {
				//	gbarrAnim[lFrameX]=new GBuffer32BGRA(iCellWidth, iCellHeight, 4); //assumes 32-bit
				//}
				if (ipAsCellSpacing==null) {
					ipAsCellSpacing=new IPoint();
				}
				if (izoneAsMargins==null) {
					izoneAsMargins=new IZone();
				}
				lFrameNow=0; //TODO: use new var instead of class var
				int iDestByteOfCellTopLeft=izoneAsMargins.Top*gbNew.iStride + izoneAsMargins.Left*gbNew.iBytesPP;
				int iDestByteOfCellNow;
				int iDestByte;
				int iSrcByte;
				int iSrcStride=gbFrame.iStride;
				int iHeight=gbFrame.iHeight;
				int iDestStride=gbNew.iStride;
				int iCellPitchX=gbNew.iBytesPP*(iCellWidth+ipAsCellSpacing.X);
				int iCellPitchY=gbNew.iStride*(iCellHeight+ipAsCellSpacing.Y);
				long lFrameLoad=0;
				for (int yCell=0; yCell<iRows; yCell++) {
					for (int xCell=0; xCell<iColumns; xCell++) {
						iSrcByte=0;
						iDestByteOfCellNow=iDestByteOfCellTopLeft + yCell*iCellPitchY + xCell*iCellPitchX;
						iDestByte=iDestByteOfCellNow;
						GotoFrame(lFrameLoad);
						//gbFrame.Save("debugToOneImage"+Base.SequenceDigits(lFrameLoad)+".png",ImageFormat.Png);
						for (int iLine=0; iLine<iHeight; iLine++) {
							if (!Memory.CopyFast(ref gbNew.byarrData, ref gbFrame.byarrData, iDestByte, iSrcByte, iSrcStride))
								bGood=false;
							iSrcByte+=iSrcStride;
							iDestByte+=iDestStride;
						}
						lFrameLoad++;
					}
				}
				if (!bGood) Base.ShowErr("There was data copy error while combining to one image","Anim32BGRA ToOneImage","combining graphic data to single image");
				//else bGood=GrayMapFromPixMapChannel(3);
				bGood=true;
				lFramesCached=lFrames;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA ToOneImage","splitting image with specified cell sizes");
			}
			return gbNew;
		}//end ToOneImage
		#endregion file operations
		
		#region utilities
		public string SourceToRegExString() {
			return sPathFileBaseName+"*."+sFileExt;
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
							Base.ShowExn(exn,"Anim32BGRA ToString("+(bDumpVars?"true":"false")+")","accessing gbFrame values to save to text file");
						}
					}
					sReturn+=";}";
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"Anim32BGRA ToString("+(bDumpVars?"true":"false")+")","dumping values to text file");
				}
				return sReturn;
			}
			else return ToString();
		}//end ToString
		public bool SaveCurrentSeqFrame() {
			bool bGood=CopyFrameToInternalBitmap();
			if (bGood) bGood=SaveInternalBitmap(PathFileOfSeqFrame(sPathFileBaseName, sFileExt, lFrameNow, iSeqDigitsMin));
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
				//bmpLoaded.Save(Manager.sDataFolderSlash+"1.test-blank.png", ImageFormat.Png);
				//sLogLine="Saved blank test PNG for Bitmap debug";
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Anim32BGRA ResetBitmapUsingFrameNow","creating new image using framework");
			}
			return bGood;
		}
		public PixelFormat PixelFormatNow() {
			return pixelformatNow;
		}
		public unsafe bool CopyFrameToInternalBitmap() {
			bool bGood=true;
			bool bLocked=false;
			gunit = GraphicsUnit.Pixel;
			try {
				if (bmpLoaded==null) {
					bGood=ResetBitmapUsingFrameNow();
				}
				if (bmpLoaded==null) {
					Base.ShowErr("Failed to initialize internal frame image","Anim32BGRA CopyFrameToInternalBitmap");
				}
				else {
					rectNowF = bmpLoaded.GetBounds(ref gunit);
					rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
										(int)rectNowF.Width, (int)rectNowF.Height);
					bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.WriteOnly, PixelFormatNow());
					bLocked=true;
					if ((gbFrame.iStride!=bmpdata.Stride)
							|| (gbFrame.iWidth!=rectNow.Width)
							|| (gbFrame.iHeight!=rectNow.Height)
							|| (gbFrame.iBytesPP!=bmpdata.Stride/rectNow.Width)
							|| (gbFrame.iBytesTotal!=(bmpdata.Stride*rectNow.Height))) {
						bmpLoaded.UnlockBits(bmpdata);
						bLocked=false;
						bGood=ResetBitmapUsingFrameNow();
					}
					if (bGood) {
						rectNowF = bmpLoaded.GetBounds(ref gunit);
						rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
											(int)rectNowF.Width, (int)rectNowF.Height);
						//bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.WriteOnly, PixelFormatNow());
						//bLocked=true;
						gunit = GraphicsUnit.Pixel;
						byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
						int iDestPixel=0;
						int iSrcByte=0;
						int iDestByte=0;
						int iDestBytesTotal=bmpdata.Stride*rectNow.Height;
						int iDestBytesPP=bmpdata.Stride/rectNow.Width;
						int iDestPixelsTotal=rectNow.Width*rectNow.Height;
						
						if (bLocked) bmpLoaded.UnlockBits(bmpdata);
						int xNow=0,yNow=0;
						while (iSrcByte<gbFrame.iBytesTotal && iDestPixel<iDestPixelsTotal) {
							if (gbFrame.iBytesPP==1) {
								bmpLoaded.SetPixel(xNow,yNow,Color.FromArgb(255,gbFrame.byarrData[iSrcByte],gbFrame.byarrData[iSrcByte],gbFrame.byarrData[iSrcByte]));
							}
							else if (gbFrame.iBytesPP==4) {
								bmpLoaded.SetPixel(xNow,yNow,Color.FromArgb(gbFrame.byarrData[iSrcByte+3],gbFrame.byarrData[iSrcByte+2],gbFrame.byarrData[iSrcByte+1],gbFrame.byarrData[iSrcByte]));
							}
							else if (gbFrame.iBytesPP==3) {
								bmpLoaded.SetPixel(xNow,yNow,Color.FromArgb(255,gbFrame.byarrData[iSrcByte+2],gbFrame.byarrData[iSrcByte+1],gbFrame.byarrData[iSrcByte]));
							}
							else {
								Base.ShowErr("Cannot copy "+gbFrame.iBytesPP.ToString()+"-channel images to internal bitmap","Anim32BGRA CopyFrameToInternalBitmap");
								break;
							}
							iDestPixel++;
							iSrcByte+=gbFrame.iBytesPP;
							xNow+=1;
							if (xNow==rectNow.Width) {
								xNow=0;
								yNow++;
							}
						}
						//while (iSrcByte<gbFrame.iBytesTotal && iDestByte<iDestBytesTotal) {
						//	*lpbyNow=gbFrame.byarrData[iSrcByte];
						//	lpbyNow+=iDestBytesPP;
						//	iDestByte+=iDestBytesPP;
						//	iSrcByte+=gbFrame.iBytesPP;
						//}
					}
					else {
						Base.ShowErr("Failed to reinitialize internal frame image","Anim32BGRA CopyFrameToInternalBitmap");
						//bLocked=false;
					}
				}//end else not still null
				if (bLocked) bmpLoaded.UnlockBits(bmpdata);
			}
			catch (Exception exn) {
				if (bLocked) {
					try{	bmpLoaded.UnlockBits(bmpdata); }
					catch (Exception exn2) {
						Base.ShowExn(exn2,"Anim32BGRA CopyFrameToInternalBitmap","unlocking bitmap during recovery");
					}
				}
				Base.ShowExn(exn,"Anim32BGRA CopyFrameToInternalBitmap");
				bGood=false;
			}
			return bGood;
		}//end CopyFrameToInternalBitmap
		public bool LoadInternalBitmap(string sFile) {
			bool bGood=true;
			try {
				if (bmpLoaded!=null) bmpLoaded.Dispose();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA LoadInternalBitmap(\""+sFile+"\")","disposing previous frame image");
			}
			try {
				bmpLoaded=new Bitmap(sFile);
				bGood=CopyFrameFromInternalBitmap();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA LoadInternalBitmap(\""+sFile+"\")","accessing image format or file location");
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
				Base.ShowExn(exn,"Anim32BGRA CopyFrameFromInternalBitmap");
				bGood=false;
			}
			return bGood;
		}//end CopyFrameFromInternalBitmap
		public GBuffer32BGRA Frame(long lFrameX) {
			GotoFrame(lFrameX);
			return gbFrame;
		}
		public bool GotoFrame(long lFrameX) {
			//refers to a file if a file is used.\
			bool bGood=true;
			try {
				if (lFramesCached==lFrames) {
					gbFrame=gbarrAnim[lFrameX];
					lFrameNow=lFrameX;
				}
				else {//if ((sPathFile!=null) && (sPathFile.Length>0)) {
					Base.ShowErr("GotoFrame of non-cached sequence is not available in this version","Anim32BGRA GotoFrame");//debug NYI
					//image.SelectActiveFrame(image.FrameDimensionsList[lFrameX], (int)lFrameX);
					//debug NYI load from file
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Anim32BGRA GotoFrame","accessing video index "+lFrameX.ToString());
				bGood=false;
			}
			return bGood;
		}//end GotoFrame
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
				Base.ShowExn(exn,"Anim32BGRA PathFileOfSeqFrame");
				sReturn="";
			}
			return sReturn;
		}
		public string PathFileOfSeqFrame(long lFrameTarget) {
			string sReturn="";
			//try {
				//string sFileExt1=sPathFile.Substring(sPathFile.LastIndexOf(".")+1);
				//int iLengthBase=(sPathFile.Length-iSeqDigitsMin)-(1+sFileExt1.Length);
				//string sPathFileBaseName1=sPathFile.Substring(iLengthBase); //INCLUDES Path
				//sReturn=PathFileOfSeqFrame(sPathFileBaseName1, sFileExt1, lFrameTarget, iSeqDigitsMin);
				sReturn=PathFileOfSeqFrame(sPathFileBaseName, sFileExt, lFrameTarget, iSeqDigitsMin);
			//}
			//catch (Exception exn) {
			//	Base.ShowExn(exn,"Anim32BGRA PathFileOfSeqFrame("+lFrameTarget.ToString()+")");
			//	sReturn="";
			//}
			return sReturn;
		}
		//public bool FromGifAnim(string sFile) {
		//}
		//public bool ToGifAnim(string sFile) {
			//image.Save(,System.Drawing.Imaging.ImageFormat.Gif);
		//}
		#endregion utilities
		//public bool FromFrames(byte[][] by2dFrames, long lFrames, int iBytesPPNow, int iWidthNow, int iHeightNow) {
		//}
		#region draw methods
		public bool DrawFrameOverlay(ref GBuffer32BGRA gbDest, ref IPoint ipDest, long lFrame) {
			bool bGood=GotoFrame(lFrame);
			if (DrawFrameOverlay(ref gbDest, ref ipDest)==false) bGood=false;
			return bGood;
		}
		public bool DrawFrameOverlay(ref GBuffer32BGRA gbDest, ref IPoint ipDest) {
			return GBuffer32BGRA.OverlayNoClipToBigCopyAlpha(ref gbDest, ref ipDest, ref gbFrame);
		}
		#endregion draw methods
	}//end class Anim32BGRA;
	
	public class Clip {
		public int iParent; //index of parent Anim32BGRA in animarr
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
				Base.ShowExn(exn,"Effect Copy");
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
