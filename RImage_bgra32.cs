/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 8/2/2005
 * Time: 6:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 //TODO (VSHA ideas):
 // - blend src>>1+dest>>1 if 127 OR 128
 // - create RImageVSHA.cs but still call the object a RImage
 // - allow combinations: 88 VA16, 844 VSH16, and 8448 VSHA24
//TODO: (x=done)
//-Scaling methods:
//  -public const int ResizeLinearWithNearestSubPixelDownscaling=1;
//  -public const int ResizePolyCurveSimplification=2;// make LGPL resizing dll (and script-fu version?)
//  -public const int ResizeNearestSubPixel=3;
//  -public const int ResizeNearestPixel=4;
//  -public const int ResizeGaussian=4;
//-blend src>>2+dest>>2 if 127 OR 128
//-create RImageVSHA.cs but still call the object RImage
//-combinations allowed: 88 VA16, 844 VSH16, and 8448 VSHA24
//-finish this -- finish implementing rpaintFore and rpaintBack by eliminating all modifications to rpaint in draw methods
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace ExpertMultimedia {

	//public class DrawMode {
	//	public const int CopyColor_CopyAlpha			= 0; //formerly DrawMode_CopyAlpha
	//	public const int AlphaColor_KeepDestAlpha				= 1; //formerly DrawMode_Alpha //does NOT affect dest alpha but DOES do alpha edge of source (there is no DrawMode_AlphaAlpha)
	//	public const int AlphaQuickEdgeColor_KeepDestAlpha	= 2; //formerly DrawMode_AlphaQuickEdge
	//	public const int AlphaHardEdgeColor_KeepDestAlpha	= 3; //formerly DrawMode_AlphaHardEdge
	//	public const int AlphaColor_KeepGreaterAlpha= 4; //formerly DrawMode_KeepGreaterAlpha
	//	public const int CopyColor_KeepDestAlpha	= 5; //formerly DrawMode_KeepDestAlpha //copies color channels WITHOUT ALPHA and skips alpha
	//}
	/// <summary>
	/// For simple graphics buffers used as images, variable-size frames, or graphics surfaces.
	/// Static overlay (DrawTo) methods are called by the nonstatic (DrawFrom) equivalents
	/// </summary>
	public class RImage {
		public static readonly string[] sarrDrawMode=new string[] {"DrawMode_CopyColor_CopyAlpha", "DrawMode_AlphaColor_KeepDestAlpha","DrawMode_AlphaQuickEdgeColor_KeepDestAlpha","DrawMode_AlphaHardEdgeColor_KeepDestAlpha","DrawMode_GreaterAlpha","DrawMode_CopyColor_KeepDestAlpha"};
		public static RImage MegaDebug_Layer=null;

		//TODO: eliminate DrawMode_ constants in favor of ROverlay (allowing alpha alpha and color alpha separately):
		public const int DrawMode_CopyColor_CopyAlpha			= 0; //formerly DrawMode_CopyAlpha
		public const int DrawMode_AlphaColor_KeepDestAlpha				= 1; //formerly DrawMode_Alpha //does NOT affect dest alpha but DOES do alpha edge of source (there is no DrawMode_AlphaAlpha)
		public const int DrawMode_AlphaQuickEdgeColor_KeepDestAlpha	= 2; //formerly DrawMode_AlphaQuickEdge
		public const int DrawMode_AlphaHardEdgeColor_KeepDestAlpha	= 3; //formerly DrawMode_AlphaHardEdge
		public const int DrawMode_AlphaColor_KeepGreaterAlpha= 4; //formerly DrawMode_KeepGreaterAlpha
		public const int DrawMode_CopyColor_KeepDestAlpha	= 5; //formerly DrawMode_KeepDestAlpha //copies color channels WITHOUT ALPHA and skips alpha
		
		public const int ScaleModeLinear				= 0;
		private const int INDEX_TL=0;
		private const int INDEX_TR=1;
		private const int INDEX_BL=2;
		private const int INDEX_BR=3;
		public const float fDiagonalUnit = 1.4142135623730950488016887242097F;//the square root of 2, dist of diagonal pixel
		public const double dDiagonalUnit = 1.4142135623730950488016887242097D;//the square root of 2 (dist=sqrt(a*a+b*b)), dist of diagonal pixel
		public const decimal mDiagonalUnit = 1.4142135623730950488016887242097M;//the square root of 2, dist of diagonal pixel
		
		public Bitmap bmpLoaded=null;//keep this in order to retain any metadata upon save
		public byte[] byarrData=null;
		public int iWidth;
		public int iHeight;
		public RPaint rpaintTemp=new RPaint();
		public int iBytesPP;
		public int iStride;
		public int iBytesTotal;
		public int iPixelsTotal;
		public static RPaint rpaintFore=new RPaint();//private static byte[] byarrBrushBack=null;
		public static RPaint rpaintBack=new RPaint();//private static byte[] byarrBrushBack32Copied64=null;
		public string sPathFileBaseName="1.untitled";
		public string sFileExt="raw";
		public static string sPixel32StyleIsAlways { get { return "bgra"; } } //assumes 32-bit
		private static bool bShowGetPixelError=true;
		private static bool bFirstCancellationOfDrawSmallerWithoutCropElseCancel=true;
		public static bool bShowDrawSmallerWithoutCropElseCancelMsg=true;//also shows if RReporting.bMegaDebug
		private static bool bFirstRunNeedsToCropToFitInto=true;
		
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
		public static int DrawMode_s {
			get {
				return (sarrDrawMode!=null)?sarrDrawMode.Length:0;
			}
		}
		
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
			Init(iWidthNow, iHeightNow, 4, true,true,true);
		}
		public RImage(int iWidthNow, int iHeightNow, int iBytesPPNow) {
			Init(iWidthNow, iHeightNow, iBytesPPNow, true,true,true);
		}
		public RImage(int iWidthNow, int iHeightNow, int iBytesPPNow, bool bCreateBufferIfNull) {
			Init(iWidthNow, iHeightNow, iBytesPPNow, bCreateBufferIfNull, true,true);
		}
		public void SetDimensions(int iWidthNow, int iHeightNow, int iBytesPPNow, bool ClearIfCreatedBuffer, bool ClearIfDidNotCreateBuffer) {
			Init(iWidthNow, iHeightNow, iBytesPPNow,true,ClearIfCreatedBuffer, ClearIfDidNotCreateBuffer);
		}
		/// <summary>
		/// Only initializes buffer if needed
		/// </summary>
		/// <param name="iWidthNow"></param>
		/// <param name="iHeightNow"></param>
		/// <param name="iBytesPPNow"></param>
		/// <param name="bCreateBuffer"></param>
		/// <param name="bCreateBuffer"></param>
		public void Init(int iWidthNow, int iHeightNow, int iBytesPPNow, bool CreateBufferIfNull, bool ClearIfCreatedBuffer, bool ClearIfDidNotCreateBuffer) {
			if (iWidthNow<=0) {
				RReporting.ShowErr("iWidthNow:"+iWidthNow+" reverting to 1");
				iWidthNow=1;
			}
			if (iHeightNow<=0) {
				RReporting.ShowErr("iHeightNow:"+iHeightNow+" reverting to 1");
				iHeightNow=1;
			}
			if (iBytesPPNow<=0) {
				RReporting.ShowErr("iBytesPPNow:"+iBytesPPNow+" reverting to 1");
				iBytesPPNow=1;
			}
			iBytesPP=iBytesPPNow;
			iWidth=iWidthNow;
			iHeight=iHeightNow;
			iStride=iWidth*iBytesPP;
			iBytesTotal=iStride*iHeight;
			iPixelsTotal=iWidth*iHeight;
			if (CreateBufferIfNull) {
				try {
					bool bBufferWasCreated=false;
					byte byFill=0;
					uint uiFill=0;
					if (iBytesTotal>0&&(iBytesTotal>=iPixelsTotal*iBytesPP)) {
						if (byarrData==null||byarrData.Length!=iBytesTotal) {
							byarrData=new byte[iBytesTotal];
							bBufferWasCreated=true;
						}
						
						if ( (bBufferWasCreated&&ClearIfCreatedBuffer)
							|| (!bBufferWasCreated&&ClearIfDidNotCreateBuffer) ) {
							if (iBytesPP==4) RMemory.Fill(ref byarrData,uiFill,0,iPixelsTotal);
							else RMemory.Fill(ref byarrData,byFill,0,iBytesTotal);
						}
					}
					else {
						byarrData=null;
						RReporting.ShowErr("Fatal bitmap description problem initializing image","creating buffer","rimage Init");
					}
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"initializing an image","RImage Init");
					iBytesTotal=0;//debug, this is currently used to denote fatal buffer creation errors
					iPixelsTotal=0;
				}
			}
			//this.rpaintFore.SetArgb(255,255,0,128);
		}//end Init
		public static bool IsOkImage(RImage rimageSrc) {
			return (rimageSrc!=null)&&rimageSrc.IsOk;
		}
		public bool IsOk {
			get {
				bool bGood=false;
				try {
					if (byarrData!=null) {
						if (byarrData.Length>0) {
							byarrData[0]=byarrData[0];
							bGood=true;
						}
						else bGood=false;
					}
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
		/// <summary>
		/// Copy this image to riReturn, creating riReturn if null or different dimensions or bitdepth.
		///   " (Copy)" will be appended to filename.
		/// </summary>
		/// <param name="riReturn"></param>
		/// <returns></returns>
		public bool CopyTo(ref RImage riReturn) {
			bool bGood=false;
			try {
				if (riReturn==null||!IsInStrideWithAndSameSizeAs(riReturn)) riReturn=new RImage(iWidth,iHeight,iBytesPP);
				for (int iNow=0; iNow<iBytesTotal; iNow++) {
					riReturn.byarrData[iNow]=byarrData[iNow];
				}
				//rpaintFore=riReturn.rpaintFore.Copy(); //commented since static
				//rpaintBack=riReturn.rpaintBack.Copy(); //commented since static
				riReturn.sPathFileBaseName=sPathFileBaseName+" (Copy)";
				riReturn.sFileExt=sFileExt;
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RImage CopyTo");
				riReturn=null;
			}
			return bGood;
		}//end CopyTo
		/// <summary>
		/// Copy this image resized to riReturn, creating riReturn if null or different dimensions or bitdepth.
		///   " (Copy)" will be appended to filename.
		/// </summary>
		/// <param name="riReturn"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public bool CopyResizedTo(ref RImage riReturn, int width, int height) {
			bool bGood=false;
			try {
				if (riReturn==null
					|| riReturn.iBytesTotal!=width*height*this.iBytesPP
				   ) {
					riReturn=new RImage(width,height,this.iBytesPP);
				}
				riReturn.iWidth=width;
				riReturn.iHeight=height;
				riReturn.iBytesPP=this.iBytesPP;
				riReturn.iStride=width*this.iBytesPP;
				riReturn.iPixelsTotal=width*height;
				riReturn.iBytesTotal=riReturn.iStride*height;
				
				riReturn.DrawFrom(0,0,(float)width,(float)height,this,ROverlay.CopyChannels());
				//rpaintFore=riReturn.rpaintFore.Copy(); //commented since static
				//rpaintBack=riReturn.rpaintBack.Copy(); //commented since static
				riReturn.sPathFileBaseName=sPathFileBaseName+" (Copy)";
				riReturn.sFileExt=sFileExt;
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"copying resized image","RImage CopyResizedTo");
				riReturn=null;
			}
			return bGood;
		}//end CopyResizedTo
		public RImage Copy() {
			RImage riReturn;
			bool bTest=false;
			try {
				riReturn=new RImage(iWidth,iHeight,iBytesPP);
				bTest=CopyTo(ref riReturn);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"copying image","RImage Copy");
				riReturn=null;
			}
			if (!bTest) {
				RReporting.ShowErr("Failed to copy to new image--setting riReturn to null","copying image","RImage Copy() {riReturn:"+(riReturn!=null?"non-null":"null")+"}");
				riReturn=null;
			}
			return riReturn;
		}//end Copy
		public RImage CreateFromZoneEx(int zone_Left, int zone_Top, int zone_RightEx, int zone_BottomEx) {
			RImage riReturn=null;
			bool bGood=false;
			try {
				int iWidthNow=zone_RightEx-zone_Left;
				int iHeightNow=zone_BottomEx-zone_Top;
				if (iWidthNow>0&&iHeightNow>0) {
					riReturn=new RImage(iWidthNow,iHeightNow,iBytesPP);
					bGood=(riReturn!=null);
				}
				else {
					RReporting.ShowErr("Tried to create image from zero-size zone: {"+IZone.ToString(zone_Left,zone_Top,zone_RightEx,zone_BottomEx)+"}");
					bGood=false;
				}
				//IRect rectNow=new IRect(zone_Left,zone_Top,iWidthNow,iHeightNow);
				//if (!riReturn.DrawFrom(riReturn.ToRect(),this,rectNow)) {
				//	bGood=false;
				//	RReporting.ShowErr("Unable to draw rectangle from this location","drawing to new image from zone",String.Format("RImage CreateFromZoneEx(left:{0},top:{1},right:{2},bottom:{3}){{currently:{4};width:{5};height:{6};riReturn{7}{8};source-rectNow:{9} }}",zone_Left,zone_Top,zone_RightEx,zone_BottomEx,Description(),iWidthNow,iHeightNow,(riReturn!=null?(".Description:"+RReporting.StringMessage(riReturn.Description(),true)):(":null")),(riReturn!=null?(";riReturn.ToRect():"+riReturn.ToRect().ToString()):""),rectNow.ToString() ));
				//}
				//else bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn,"creating image using specified size",String.Format("RImage CreateFromZoneEx(left:{0},top:{1},right:{2},bottom:{3}){{currently:{4};riReturn:{5}}}",zone_Left,zone_Top,zone_RightEx,zone_BottomEx,Description(),(riReturn!=null?(riReturn.Description()):"null")) );
				riReturn=null;
			}
			//if (!bTest) riReturn=null;
			return riReturn;
		} //end CreateFromZoneEx
		#endregion constructors


		#region Draw methods (all should have nonstatic versions)
		private const int MegaDebugLayer_BytesPPDesired=4;
		private static void PrepareMegaDebugLayer(RImage riThisBigOrBigger) {
			if (riThisBigOrBigger!=null) {
				if (MegaDebug_Layer==null) {
					MegaDebug_Layer=new RImage(riThisBigOrBigger.iWidth,riThisBigOrBigger.iHeight, MegaDebugLayer_BytesPPDesired,true);
				}
				else if (MegaDebug_Layer.Width<riThisBigOrBigger.Width||MegaDebug_Layer.Height<riThisBigOrBigger.Height) {
					int iMaxW=(MegaDebug_Layer.Width>riThisBigOrBigger.Width)?MegaDebug_Layer.Width:riThisBigOrBigger.Width;
					int iMaxH=(MegaDebug_Layer.Height>riThisBigOrBigger.Height)?MegaDebug_Layer.Height:riThisBigOrBigger.Height;
					RImage riTemp=MegaDebug_Layer;
					MegaDebug_Layer=new RImage(iMaxW,iMaxH,MegaDebugLayer_BytesPPDesired,true);
					RImage.DrawToLargerNoClipCopyAlpha(ref MegaDebug_Layer, RMath.ipZero, ref riTemp);
				}
			}
			else {
				RReporting.ShowErr("PrepareMegaDebugLayer: Sample image was null");
				MegaDebug_Layer=new RImage(777,777,MegaDebugLayer_BytesPPDesired);
			}
		}
		/// <summary>
		/// Primary Scaled Drawing Method (float version).
		/// Does NOT check boundaries!  Use DrawFrom for automatic cropping.
		/// Resizing constant: ResizeLinearWithNearestSubPixelDownscaling
		/// to_X and to_Y are absolute sampling coordinates (e.g. top left of first pixel is [-.5,-.5], bottom left of image is [width-.5,height-.5])
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="to_X"></param>
		/// <param name="to_Y"></param>
		/// <param name="to_Width"></param>
		/// <param name="to_Height"></param>
		/// <param name="source"></param>
		/// <param name="from_X"></param>
		/// <param name="from_Y"></param>
		/// <param name="from_Width"></param>
		/// <param name="from_Height"></param>
		/// <param name="iDrawMode"></param>
		/// <returns></returns>
		public static bool DrawTo(RImage destination, float to_X, float to_Y, float to_W, float to_H, RImage source, float from_X, float from_Y, float from_W, float from_H, ROverlay overlayoptions) {
			//TODO: only initialize vars that are used by error output?
			//-OR OPTION 2 make all of these vars static to prevent re-allocation slowness
			bool bGood=false;
			//TODO: (x=done)
			//x-source location MUST be adjusted by the difference between (fxDestWholeNum,fyDestWholeNum) [derived from modified (iDestRect_L,iDestRect_T)] and (to_X,to_Y) [DONE -- see (*DestNowWholeNum-to_*) * *InverseScale]
			float to_R=0.0f;
			float to_B=0.0f;
			float from_R=0.0f;
			float from_B=0.0f;
			int iDestRect_L=0;
			int iDestRect_T=0;
			int iDestRect_R=0;
			int iDestRect_B=0;
			//float xScale=0.0f;
			//float xScale=0.0f;
			float xInverseScale=0.0f;
			float yInverseScale=0.0f;
			float fxSrcNow=0,fySrcNow=0.0f;
			float fxDestWholeNum=0.0f;
			float fyDestWholeNum=0.0f;
			int indexWhenDestLineNowStarts=0;
			int iDest=0;
			//resampling vars:
			//int[] iarrSrcSampleIndex=new int[4];//{TL,TR,BL,BR} source indeces for first channel of each nearest pixel by center
			//FPoint[] 
			//float fPixelWeightTotal=0.0f;
			//int xSrcI=0;
			//int ySrcI=0;
			float fxRoundedSrc=0.0f;
			float fyRoundedSrc=0.0f;
			float fxRoundedSrcNearestL=0.0f;
			float fyRoundedSrcNearestT=0.0f;
			float fxRoundedSrcNearestR=0.0f;
			float fyRoundedSrcNearestB=0.0f;
			//bool bFourCorner=false;
			int indexSrcNearestTL=0;
			int indexSrcNearestTR=0;
			int indexSrcNearestBL=0;
			int indexSrcNearestBR=0;
			int ixRoundedSrc=0;
			int iyRoundedSrc=0;
			//int ixRoundedSrcPrev=0; int ixRoundedSrcNext=0; int iyRoundedSrcPrev=0; int iyRoundedSrcNext=0; //TODO: un-implement these in favor of indeces
			float fxSrcWhenLineStarts=0.0f;
			float fxRoundedSrcWhenLineStarts=0.0f;
			float fxSrcNextness=0.0f;
			float fySrcNextness=0.0f;
			float fxRoundedSrcNearestLWhenLineStarts=0.0f;
			float fxRoundedSrcNearestRWhenLineStarts=0.0f;
			int indexSrcNearestTLWhenSrcLineNowStarts=0;
			float fSrcW=0.0f;
			float fSrcH=0.0f;
			float fDestW=0.0f;
			float fDestH=0.0f;
			float fRectEdgeAlpha_L=0.0f;
			byte byRectEdgeAlpha_L=0;
			float fRectEdgeAlpha_T=0.0f;
			byte byRectEdgeAlpha_T=0;
			float fRectEdgeAlpha_R=0.0f;
			byte byRectEdgeAlpha_R=0;
			float fRectEdgeAlpha_B=0.0f;
			byte byRectEdgeAlpha_B=0;
			float fRowInness=0.0f;
			float fInness=0.0f;
			int iyDestNow=0;
			int ixDestNow=0;
			int indexSrcNearestL_CroppedByNow=0; //bool bFixPrev=false;
			int indexSrcNearestR_CroppedByNow=0;
			int indexSrcNearestT_CroppedByNow=0;
			int indexSrcNearestB_CroppedByNow=0;
			byte byDestA=0;
			byte bySourceA=0;
			int iDestTemp=0;
			int iDestStart=0;
			int iSrcStart_TL=0;
			int iSrcStart_BR=0;
			try {
				RReporting.sParticiple="checking images";
				if ( destination!=null && (destination.iBytesPP!=4||source.iBytesPP!=4) ) {
					if (RReporting.bDebug) RReporting.Warning("Anything other than 32-bit Source and destination for DrawTo Primary Scaled Drawing Method is experimental!");
					//return false;
				}
				if (source==null) throw new ApplicationException("Null source image");
				else if (source.byarrData==null) throw new ApplicationException("Null pixel buffer in source");
				else if (source.byarrData.Length<source.iBytesTotal) throw new ApplicationException("pixel buffer Length in source is 0");
				else if (source.iBytesTotal<source.iWidth*source.iHeight*source.iBytesPP) throw new ApplicationException("pixel buffer Length in source is less than dimensions times channels");
				RReporting.sParticiple="setting primary resampling variables";
				//RImage.rpaintFore.SetArgb(255,0,0,0);
				//destination.DrawRect(RConvert.ToInt(from_X)+2,RConvert.ToInt(from_Y)+2,RConvert.ToInt(from_W)-2,RConvert.ToInt(from_H)-2);
				//RImage.rpaintFore.SetArgb(255,255,0,0);
				//destination.DrawRect(RConvert.ToInt(from_X)+1,RConvert.ToInt(from_Y)+1,RConvert.ToInt(from_W),RConvert.ToInt(from_H));
				//RImage.rpaintFore.SetArgb(255,0,0,0);
				to_R=to_X+to_W;//right (ok to add directly e.g. since x of .5 + width of 1.0 results in 1.5 which is the outer edge of the same pixel [last pixel, which is pixel[0]in a 1px wide image])
				to_B=to_Y+to_H;//bottom
				from_R=from_X+from_W;//right
				from_B=from_Y+from_H;//bottom
				iDestRect_L=(int)(to_X+.5f);
				iDestRect_T=(int)(to_Y+.5f);
				iDestRect_R=(int)Math.Ceiling(to_R-.5f);//right, made INCLUSIVE below
				iDestRect_B=(int)Math.Ceiling(to_B-.5f);//bottom, made INCLUSIVE below
				
				//It is okay to modify iDestRect_ vars since compensation is done further down.
				if (to_X<((float)iDestRect_L-.5f)) iDestRect_L--;//i.e. encroaches on previous pixel; NOT incremented if e.g. .5f
				if (to_Y<((float)iDestRect_T-.5f)) iDestRect_T--;//i.e. encroaches on previous pixel; NOT incremented if e.g. .5f
				if (to_R>((float)iDestRect_R+.5f)) iDestRect_R++;//i.e. if encroaches on next pixel; NOT incremented if e.g. width-.5f
				if (to_B>((float)iDestRect_B+.5f)) iDestRect_B++;//i.e. if encroaches on next pixel; NOT incremented if e.g. height-.5f
				if (iDestRect_L<0) {
					RReporting.ShowErr("Drawing non-precropped dest rect is causing inaccurate out-of-range left edge!","drawing scaled after clipping {iDestRect_L:"+iDestRect_L.ToString()+"}","DrawTo(RImage,float,...)");
					iDestRect_L=0;
				}
				else if (iDestRect_L>=destination.Width) {
					RReporting.ShowErr("Drawing non-precropped dest rect is causing inaccurate out-of-range left edge!","drawing scaled after clipping {iDestRect_L:"+iDestRect_L.ToString()+"}","DrawTo(RImage,float,...)");
					iDestRect_L=destination.Width-1;
				}
				
				if (iDestRect_T<0) {
					RReporting.ShowErr("Drawing non-precropped dest rect is causing inaccurate out-of-range top edge!","drawing scaled after clipping {iDestRect_T:"+iDestRect_T.ToString()+"}","DrawTo(RImage,float,...)");
					iDestRect_T=0;
				}
				else if (iDestRect_T>=destination.Width) {
					RReporting.ShowErr("Drawing non-precropped dest rect is causing inaccurate out-of-range top edge!","drawing scaled after clipping {iDestRect_T:"+iDestRect_T.ToString()+"}","DrawTo(RImage,float,...)");
					iDestRect_T=destination.Width-1;
				}
				
				if (iDestRect_R>=destination.Width) {
					RReporting.ShowErr("Drawing non-precropped dest rect is causing inaccurate out-of-range right edge!","drawing scaled after clipping {iDestRect_R:"+iDestRect_R.ToString()+"}","DrawTo(RImage,float,...)");
					iDestRect_R=destination.Width-1;
				}
				if (iDestRect_R<iDestRect_L) {
					RReporting.ShowErr("Drawing non-precropped dest rect is causing inaccurate out-of-range right edge!","drawing scaled after clipping {iDestRect_R:"+iDestRect_R.ToString()+";iDestRect_L:"+iDestRect_L.ToString()+"}","DrawTo(RImage,float,...)");
					iDestRect_R=iDestRect_L;
				}
				
				
				if (iDestRect_B>=destination.Height) {
					RReporting.ShowErr("Drawing non-precropped dest rect is causing inaccurate out-of-range bottom edge!","drawing scaled after clipping {iDestRect_B:"+iDestRect_B.ToString()+"}","DrawTo(RImage,float,...)");
					iDestRect_B=destination.Height-1;
				}
				if (iDestRect_B<iDestRect_T) {
					RReporting.ShowErr("Drawing non-precropped dest rect is causing inaccurate out-of-range bottom edge!","drawing scaled after clipping {iDestRect_B:"+iDestRect_B.ToString()+"; iDestRect_T:"+iDestRect_T.ToString()+"}","DrawTo(RImage,float,...)");
					iDestRect_B=iDestRect_T;
				}
				
				RReporting.sParticiple="done setting primary resampling variables {iDestRect_L:"+iDestRect_L+"; iDestRect_T:"+iDestRect_T+"; iDestRect_R:"+iDestRect_R+"; iDestRect_B:"+iDestRect_R+"}";
				if (RReporting.bMegaDebug) {
					RReporting.sParticiple="drawing debug marks to RImage.MegaDebug_Layer";
					PrepareMegaDebugLayer(destination);
					RImage.rpaintFore.SetArgb(255,255,0,0);
					int iDebugRect_L=iDestRect_L;//RMath.IRound(to_X);
					int iDebugRect_T=iDestRect_T;//RMath.IRound(to_Y);
					int iDebugRect_R=iDestRect_R;//RMath.IRound(to_R);
					int iDebugRect_B=iDestRect_B;//RMath.IRound(to_B);
					if (iDebugRect_T-1>=0) {
						if (iDebugRect_L-1>=0) {
							//TL
							MegaDebug_Layer.DrawHorzLine(iDebugRect_L-1,iDebugRect_T-1,3);
							MegaDebug_Layer.DrawVertLine(iDebugRect_L-1,iDebugRect_T,2);
						}
						if (iDebugRect_R+1<MegaDebug_Layer.Width) {
							//TR
							MegaDebug_Layer.DrawHorzLine(iDebugRect_R-1,iDebugRect_T-1,3);//R-1 since rect is inclusive and line is 3 long
							MegaDebug_Layer.DrawVertLine(iDebugRect_R+1,iDebugRect_T,2);
						}
					}
					if (iDebugRect_B+1<MegaDebug_Layer.Height) {
						if (iDebugRect_L-1>=0) {
							//BL
							MegaDebug_Layer.DrawHorzLine(iDebugRect_L-1,iDebugRect_B+1,3);
							MegaDebug_Layer.DrawVertLine(iDebugRect_L-1,iDebugRect_B,2);
						}
						if (iDebugRect_R+1<MegaDebug_Layer.Width) {
							//BR
							MegaDebug_Layer.DrawHorzLine(iDebugRect_R-1,iDebugRect_B+1,3);//R-1 since rect is inclusive and line is 3 long
							MegaDebug_Layer.DrawVertLine(iDebugRect_R+1,iDebugRect_B,2);
						}
					}
					RImage.rpaintFore.SetArgb(255,255,255,0);
					float CaddyCorner_L=to_X-.5f;//-.5 to be around not inside
					float CaddyCorner_T=to_Y-.5f;//-.5 to be around not inside
					float CaddyCorner_R=to_R+.5f;//+.5 to be around not inside
					float CaddyCorner_B=to_B+.5f;//+.5 to be around not inside
					
					if (CaddyCorner_L>=0&&CaddyCorner_T>=0) MegaDebug_Layer.DrawVectorDot(CaddyCorner_L,CaddyCorner_T);
					if (CaddyCorner_R<fDestW-.5&&CaddyCorner_B<fDestH-.5) MegaDebug_Layer.DrawVectorDot(CaddyCorner_R,CaddyCorner_B);
				}//end if bMegaDebug
				
				
				RReporting.sParticiple="calculating scale from rectangles {from_W:"+from_W+"; from_H:"+from_H+"; to_W:"+to_W+"; to_H:"+to_H+"}";
				//xScale=to_W/from_W;
				//xScale=to_H/from_H;
				xInverseScale=from_W/to_W;//how much source should be incremented per every 1.0 destination pixel i.e. if upscaling to a bigger width, need to move smaller since result will cover a higher pixel count on the destination
				yInverseScale=from_H/to_H;
				RReporting.sParticiple="calculating initial destination resampling variables";
				fxDestWholeNum=(float)iDestRect_L;
				fyDestWholeNum=(float)iDestRect_T;
				indexWhenDestLineNowStarts=destination.XYToLocation(iDestRect_L,iDestRect_T);
				iDestStart=indexWhenDestLineNowStarts;
				//bFourCorner=false;
				
				RReporting.sParticiple="calculating initial source resampling variables from source size and target size {to_X:"+to_X+"; to_Y:"+to_Y+"; fxDestWholeNum:"+fxDestWholeNum+"; fyDestWholeNum:"+fyDestWholeNum+"}";
				//ixRoundedSrcPrev; int ixRoundedSrcNext; int iyRoundedSrcPrev; int iyRoundedSrcNext; //TODO: un-implement these in favor of indeces
				//ixRoundedSrcPrev=0; ixRoundedSrcNext=0; iyRoundedSrcPrev=0; iyRoundedSrcNext=0; //TODO: start value intelligently so can be incremented intelligently
				
				//On the following two assignments, (*DestNowWholeNum-to_*) is used
				// since source location MUST be adjusted by the difference between
				// (fxDestWholeNum,fyDestWholeNum) [derived from modified (iDestRect_L,iDestRect_T)]
				// and (to_X,to_Y) since the sampling point has been moved from the top left corner
				// of the pixel to the center of the pixel.
				fxSrcWhenLineStarts=from_X-(to_X-fxDestWholeNum)*xInverseScale;//fxDestNowWholeNum was (float)iDestRect_L
				fxSrcNow=fxSrcWhenLineStarts; //done at the beginning of each loop, but set redundantly now too so edge nearest pixels can be calculated before loop starts
				fySrcNow=from_Y-(to_Y-fyDestWholeNum)*yInverseScale;
				//NOTE: source dimensions don't have to be adjusted, because from_R and from_B are absolute and used when traversing source
				RReporting.sParticiple="getting quadrants around initial point {fxSrcNow:"+fxSrcNow+"; fySrcNow:"+fySrcNow+"}";
				//NOTE: rounded source vars are ONLY for finding nearest whole pixel locations:
				//NOTE: -.5 becomes -1 but those (x&y) are cropped later before drawing row and pixel
				fxRoundedSrcWhenLineStarts=RMath.Round(fxSrcWhenLineStarts);//=RMath.Round(fxSrcNow);
				fxRoundedSrc=fxRoundedSrcWhenLineStarts;//RMath.Round(fxSrcWhenLineStarts);
				fyRoundedSrc=RMath.Round(fySrcNow);
				ixRoundedSrc=(int)fxRoundedSrc;//ok to truncate since whole number
				iyRoundedSrc=(int)fyRoundedSrc;//ok to truncate since whole number
				//Choose nearest pixels for averaging:
				if (fxSrcNow>=fxRoundedSrc) {
					fxRoundedSrcNearestL=fxRoundedSrc;
					fxRoundedSrcNearestR=fxRoundedSrc+1.0f;
				}
				else {// (fxSrcNow<fxRoundedSrc) {
					fxRoundedSrcNearestL=fxRoundedSrc-1.0f;
					fxRoundedSrcNearestR=fxRoundedSrc;
				}
				if (fySrcNow>=fyRoundedSrc) {
					fyRoundedSrcNearestT=fyRoundedSrc;
					fyRoundedSrcNearestB=fyRoundedSrc+1.0f;
				}
				else {//if (fySrcNow<fyRoundedSrc) {
					fyRoundedSrcNearestT=fyRoundedSrc-1.0f;
					fyRoundedSrcNearestB=fyRoundedSrc;
				}
				
				fxRoundedSrcNearestLWhenLineStarts=fxRoundedSrcNearestL;
				fxRoundedSrcNearestRWhenLineStarts=fxRoundedSrcNearestR;
				RReporting.sParticiple="getting indeces for initial quadrants {fxSrcNow:"+fxSrcNow+"; fySrcNow:"+fySrcNow+"; fxRoundedSrcNearestL:"+fxRoundedSrcNearestL+"; fxRoundedSrcNearestR:"+fxRoundedSrcNearestR+"; fyRoundedSrcNearestT:"+fyRoundedSrcNearestT+"; fyRoundedSrcNearestB:"+fyRoundedSrcNearestB+"}";
				indexSrcNearestTL=source.XYToLocation((int)fxRoundedSrcNearestL,(int)fyRoundedSrcNearestT);//ok to truncate these floats since they are whole numbers
				indexSrcNearestTLWhenSrcLineNowStarts=indexSrcNearestTL; //modified after each line
				indexSrcNearestTR=indexSrcNearestTL+source.iBytesPP;
				indexSrcNearestBL=indexSrcNearestTL+source.iStride;
				indexSrcNearestBR=indexSrcNearestBL+source.iBytesPP;
				iSrcStart_TL=indexSrcNearestTL;//start location variable only exists for debug output
				iSrcStart_BR=indexSrcNearestBR;//start location variable only exists for debug output
				fSrcW=(float)source.iWidth;
				fSrcH=(float)source.iHeight;
				float fSrc_LastX=fSrcW-1.0f;
				float fSrc_LastY=fSrcH-1.0f;
				fDestW=(float)destination.iWidth;
				fDestH=(float)destination.iHeight;
				//NOTES:
				//-any variable not marked as 'f' is an integer.
				
				//Destination edge blending:
				//-source edge grabbing is a whole different issue from destination edge blending:
				//  for example, if source is an arbitrary rect, and dest is an upscaled size AND starts at a whole number, a whole number causes it to be blended halfway
				//-Must fix integers since for example to_X+to_W should be truncated if lands at a halfway point (for example, especially if lands on dest_W-.5 or will overflow!)
				//-non-centered float dest should ONLY be used for determining alhpa of edge pixels!
				//   -the source rect should NEVER determine the edge alpha, because it should never go outside the source
				//   -the alpha only needs to be calculated ONCE for each edge:
				fRectEdgeAlpha_L=Math.Abs((fxDestWholeNum+.5f)-to_X);//since fxDestWholeNum is (float)iDestRect_L
				byRectEdgeAlpha_L=RConvert.ToByte(fRectEdgeAlpha_L*255.0f);
				//because starts at iDestRect_L assignment was truncated from to_X then changed by: if (to_X<((float)iDestRect_L-.5f)) iDestRect_L--;//i.e. encroaches on previous pixel; NOT incremented if e.g. .5f
				fRectEdgeAlpha_T=Math.Abs((fyDestWholeNum+.5f)-to_Y);
				byRectEdgeAlpha_T=RConvert.ToByte(fRectEdgeAlpha_T*255.0f);
				fRectEdgeAlpha_R=Math.Abs(to_R-((float)iDestRect_R-.5f)); //-.5f since iDestRect_R is now at width if to_R goes past (float)((int)to_R)+.5f
				byRectEdgeAlpha_R=RConvert.ToByte(fRectEdgeAlpha_R*255.0f);
				//because ends inclusively at iDestRect_R and assignment was truncated from to_R then changed by: if (to_R>((float)iDestRect_R+.5f)) iDestRect_R++;//i.e. if encroaches on next pixel; NOT incremented if e.g. width-.5f
				fRectEdgeAlpha_B=Math.Abs(to_B-((float)iDestRect_B-.5f));
				byRectEdgeAlpha_B=RConvert.ToByte(fRectEdgeAlpha_B*255.0f);
				
				//-NOW source location should be adjusted by the difference between the dest location and the dest whole number
				//and the whole number should be used from now on.
				//end Destination edge blending
				if (RReporting.bMegaDebug) RReporting.sParticiple="done setting up all resampling variables {fxRoundedSrcNearestL:"+fxRoundedSrcNearestL+"; (int)fxRoundedSrcNearestL:"+((int)fxRoundedSrcNearestL)+"; fyRoundedSrcNearestT:"+fyRoundedSrcNearestT+"; (int)fyRoundedSrcNearestT:"+((int)fyRoundedSrcNearestT)+"; fxRoundedSrcNearestR:"+fxRoundedSrcNearestR+"; fyRoundedSrcNearestB:"+fyRoundedSrcNearestB+"; indexSrcNearestTL:"+indexSrcNearestTL+"; indexSrcNearestTL/source.iBytesPP:"+(indexSrcNearestTL/source.iBytesPP).ToString()+"; source:"+source.Description()+"; destination:"+destination.Description()+"}";
				//if (RReporting.bUltraDebug) {
				//	string UltraDebug_ImageFileDest_Name="ultradebug-DrawTo(RImage,float,...).dest.png";
				//	RReporting.sParticiple="saving "+UltraDebug_ImageFileDest_Name;
				//	destination.Save(UltraDebug_ImageFileDest_Name,ImageFormat.Png);
				//	string UltraDebug_ImageFileSource_Name="ultradebug-DrawTo(RImage,float,...).source.png";
				//	RReporting.sParticiple="saving "+UltraDebug_ImageFileSource_Name;
				//	source.Save(UltraDebug_ImageFileSource_Name,ImageFormat.Png);
				//}
				RReporting.sParticiple="running main resampling loop";
				bool bUpdateDebugRowStatus=false;
				bool RowNow_HasMsg=bUpdateDebugRowStatus;
				bool bNeedSourceAlpha=overlayoptions.NeedsSourceAlpha;
				int ColorDestModeDetermined=overlayoptions.ColorDestMode;
				int AlphaDestModeDetermined=overlayoptions.AlphaDestMode;
				for (iyDestNow=iDestRect_T; iyDestNow<=iDestRect_B; iyDestNow++) {
					indexSrcNearestT_CroppedByNow=0;
					indexSrcNearestB_CroppedByNow=0;
					fxDestWholeNum=(float)iDestRect_L;
					iDest=indexWhenDestLineNowStarts;
					
					fxSrcNow=fxSrcWhenLineStarts;
					fxRoundedSrcNearestL=fxRoundedSrcNearestLWhenLineStarts;
					fxRoundedSrcNearestR=fxRoundedSrcNearestRWhenLineStarts;
					
					//WHILE PAST ROW BOUNDARY IN SOURCE IMAGE, SKIP SOURCE ROWS:
					//replace while loops (in x as well) with adding ceiling of difference
 					///NOTE: CEILING rounds UP (in positive direction) even if number is negative!
 					/// - FLOOR rounds DOWN (in negative direction) even if number is negative!
 					/// - TRUNCATE rounds TOWARD ZERO regardless of sign
 					/// - The only increment done to source at the end of this loop is fySrcNow+=yInverseScale
					if (fySrcNow>fyRoundedSrcNearestB) { //.5 ONLY used since it is a rounded variable
 						//was using "if (>=) {Floor()+1}" above, but using "if (>) {Ceiling()}" simplifies the math
 						float fNewFar=RMath.Ceiling(fySrcNow); //i.e. the ceiling of .1 is 1, of -.9 is 0
 						float fWholePixAdder=fNewFar-fyRoundedSrcNearestB;
 						int iWholePixelAdder=RMath.IRound(fWholePixAdder);//typecast should also work since whole
						fyRoundedSrcNearestT+=fWholePixAdder;
						fyRoundedSrcNearestB=fNewFar;//+=fWholePixAdder;
						fyRoundedSrc+=fWholePixAdder;
						iyRoundedSrc+=iWholePixelAdder;
						indexSrcNearestTLWhenSrcLineNowStarts+=iWholePixelAdder*source.iStride;
						/// This clause is also adapted for X AXIS
					}
 					/*
					while (fyRoundedSrcNearestB<=fySrcNow) { //only BOTH round AND get previous and next ROWS on change in y (ergo when outer loop iterates)
						//TODO: handle passing more than one (downscaling) more quickly (rather than while loop)
						if (fySrcNow>=fyRoundedSrc) { //else {
							fyRoundedSrcNearestT=fyRoundedSrc;
							fyRoundedSrcNearestB=fyRoundedSrc+1.0f;
						}
						else {//(fySrcNow<fyRoundedSrc) {
							fyRoundedSrcNearestT=fyRoundedSrc-1.0f;
							fyRoundedSrcNearestB=fyRoundedSrc;
						}
						//iyRoundedSrcPrev++;//=(int)fyRoundedSrcNearestT;
						//iyRoundedSrcNext++;//=(int)fyRoundedSrcNearestB;
						indexSrcNearestTLWhenSrcLineNowStarts+=source.iStride;
						///TODO:? indexSrcNearestTLWhenSrcLineNowStarts may only need to be incremented in certain situations, such as when TL increments
						/// (which does NOT happen when NearestT is fyRoundedSrc-1.0f)!
					}
 					*/
					fySrcNextness=fySrcNow-fyRoundedSrcNearestT;//fyRoundedSrcNearestB-fySrcNow;
					indexSrcNearestTL=indexSrcNearestTLWhenSrcLineNowStarts;//indexSrcNearestTLWhenSrcLineNowStarts is modified after each line of SOURCE ONLY
					indexSrcNearestTR=indexSrcNearestTL+source.iBytesPP;
					indexSrcNearestBL=indexSrcNearestTL+source.iStride;
					indexSrcNearestBR=indexSrcNearestBL+source.iBytesPP;
					
					//Fixes indeces that are out of range by truncating them to the nearest existant pixel
					//-this is needed since adjustment to source above may cause out-of range source
					// --in the subpixel level, only the existant part of the image is "used" (because of dest edge limiting)
					// so the outlier issue is not really an error but a math-simplifying optimization)
					//crop Y axis of source resampling quads:
					//NOTE: if floating point math were perfect, (int) could be used instead of RMath.IRound for these clauses
					if (fyRoundedSrcNearestT<0.0f) {
						indexSrcNearestT_CroppedByNow=RMath.IRound(0.0f-fyRoundedSrcNearestT)*source.iStride;//- a negative
						indexSrcNearestTL+=indexSrcNearestT_CroppedByNow;//+ a positive
						indexSrcNearestTR+=indexSrcNearestT_CroppedByNow;//+ a positive
					}
					else if (fyRoundedSrcNearestT>fSrc_LastY) {
						indexSrcNearestT_CroppedByNow=RMath.IRound(fSrc_LastY-fyRoundedSrcNearestT)*source.iStride;//- a larger positive
						indexSrcNearestTL+=indexSrcNearestT_CroppedByNow;//+ a negative
						indexSrcNearestTR+=indexSrcNearestT_CroppedByNow;//+ a negative
					}
					if (fyRoundedSrcNearestB<0.0f) { //do NOT do "else if" because source may be 1px tall
						indexSrcNearestB_CroppedByNow=RMath.IRound(0.0f-fyRoundedSrcNearestB)*source.iStride;//- a negative
						indexSrcNearestBL+=indexSrcNearestB_CroppedByNow;//+ a positive
						indexSrcNearestBR+=indexSrcNearestB_CroppedByNow;//+ a positive
					}
					else if (fyRoundedSrcNearestB>fSrc_LastY) { //>=fSrcW 
						indexSrcNearestB_CroppedByNow=RMath.IRound(fSrc_LastY-fyRoundedSrcNearestB)*source.iStride;//- a larger positive
						indexSrcNearestBL+=indexSrcNearestB_CroppedByNow;//+ a negative
						indexSrcNearestBR+=indexSrcNearestB_CroppedByNow;//+ a negative
					}
					if (bUpdateDebugRowStatus) {
						if (RowNow_HasMsg) {
							RReporting.sParticiple="done cropping variables for first row iyDestNow:"+iyDestNow;
						}
						else {
							RReporting.sParticiple="done cropping variables for row iyDestNow:"+iyDestNow;
							RowNow_HasMsg=true;//show for first pixel of each row
						}
					}
					//if (fyRoundedSrcNearestT<0.0f) {
					//	indexSrcNearestTL+=source.iStride;
					//	indexSrcNearestTR+=source.iStride;
					//}
					//else if (fyRoundedSrcNearestB>=fSrcH) {
					//	indexSrcNearestBL-=source.iStride;
					//	indexSrcNearestBR-=source.iStride;
					//}
					fxRoundedSrc=fxRoundedSrcWhenLineStarts;
					////if (fxSrcNow<0.5f) { //0.5f as opposed to 0.0f is ONLY needed for x, not for y, since fxSrcNow (fxRoundedSourceNext is not calculated yet)
					////else if (fxSrcNow>=(fSrcW-.5f)) { //-.5f is ONLY needed for x, not for y, since fxSrcNow (fxRoundedSourceNext is not calculated yet)
					////>= since fxRoundedSrcNearestR is incremented when fxSrcNow>=fxRoundedSrcNearestR
					////ixRoundedSrc=(int)fxRoundedSrc;
					//if (fxSrcNow<fxRoundedSrc) {
					//	fxRoundedSrcNearestL=fxRoundedSrc-1.0f;
					//	fxRoundedSrcNearestR=fxRoundedSrc;
					//}
					//else {
					//	fxRoundedSrcNearestL=fxRoundedSrc;
					//	fxRoundedSrcNearestR=fxRoundedSrc+1.0f;
					//}
					fRowInness=1.0f;
					if (iyDestNow==iDestRect_T) {
						fRowInness*=fRectEdgeAlpha_T;//TODO: make sure fRectEdgeAlpha_* (set above) handles 1px wide or 1px tall images correctly
					}
					else if (iyDestNow==iDestRect_B) {
						fRowInness*=fRectEdgeAlpha_B;
					}
					//if (RReporting.bUltraDebug) {
					//	string UltraDebug_ImageFileDest_Name="ultradebug-DrawTo(RImage,float,...).dest.png";
					//	RReporting.sParticiple="saving "+UltraDebug_ImageFileDest_Name;
					//	destination.Save(UltraDebug_ImageFileDest_Name,ImageFormat.Png);
					//}
					if (bUpdateDebugRowStatus) RReporting.sParticiple="drawing row with interpolation {fxRoundedSrcNearestL:"+fxRoundedSrcNearestL+"; (int)fxRoundedSrcNearestL:"+(RConvert.ToInt(fxRoundedSrcNearestL))+"; fyRoundedSrcNearestT:"+fyRoundedSrcNearestT+"; (int)fyRoundedSrcNearestT:"	+(RConvert.ToInt(fyRoundedSrcNearestT))+"; fxRoundedSrcNearestR:"+fxRoundedSrcNearestR+"; fyRoundedSrcNearestB:"+fyRoundedSrcNearestB+"; indexSrcNearestTL:"+indexSrcNearestTL+"}";
					for (ixDestNow=iDestRect_L; ixDestNow<=iDestRect_R; ixDestNow++) {
						indexSrcNearestL_CroppedByNow=0;
						indexSrcNearestR_CroppedByNow=0;
					//Linear resample:
						//pSrcNow.X=(fxDestWholeNum-to_X)*xInverseScale+from_X;
						//pSrcNow.Y=(fyDestWholeNum-to_Y)*yInverseScale+from_Y;
						//if (pSrcNow.X>=from_X&&pSrcNow.Y>=from_Y&&pSrcNow.X<=from_R&&pSrcNow.Y<=from_B&&iDest<destination.iBytesTotal)
						//RImage.InterpolatePixel(ref destination,ref source,iDest,ref pSrcNow); //XYToLocation(ixDestNow,iyDestNow)
						
						/*
						//WHILE PASSED COLUMN BOUNDARY IN SOURCE IMAGE, SKIP SOURCE PIXELS:
						while (fxRoundedSrcNearestR<=fxSrcNow) { //only BOTH round AND get previous and next columns when fySrcNow passes fyRoundedSrcNearestB
							//TODO: handle passing more than one (downscaling) more quickly (rather than while loop)
							//fxRoundedSrc=RMath.Round(fxSrcNow);
							//ixRoundedSrc=(int)fxRoundedSrc;
							//if (fxSrcNow<fxRoundedSrc) {
							//	fxRoundedSrcNearestL=fxRoundedSrc-1.0f;
							//	fxRoundedSrcNearestR=fxRoundedSrc;
							//}
							//else {
							//	fxRoundedSrcNearestL=fxRoundedSrc;
							//	fxRoundedSrcNearestR=fxRoundedSrc+1.0f;
							//}
							fxRoundedSrcNearestL+=1.0f;
							fxRoundedSrcNearestR+=1.0f;
							indexSrcNearestBL+=source.iBytesPP;
							indexSrcNearestBR+=source.iBytesPP;
							indexSrcNearestTL+=source.iBytesPP;
							indexSrcNearestTR+=source.iBytesPP;
						}//end while fxSrcNow>=fxRoundedSrcNearestR (increment all "previous" and "next" coordinates for x and indeces)
						*/
						if (fxSrcNow>fxRoundedSrcNearestR) { //.5 ONLY used since it is a rounded variable
	 						//was using "if (>=) {Floor()+1}" above, but using "if (>) {Ceiling()}" simplifies the math
	 						float fNewFar=RMath.Ceiling(fxSrcNow); //i.e. the ceiling of .1 is 1, of -.9 is 0
	 						float fWholePixAdder=fNewFar-fxRoundedSrcNearestR;
	 						int iWholePixelAdder=RMath.IRound(fWholePixAdder);//typecast should also work since whole
							fxRoundedSrcNearestL+=fWholePixAdder;
							fxRoundedSrcNearestR=fNewFar;//+=fWholePixAdder;
							fxRoundedSrc+=fWholePixAdder;
							ixRoundedSrc+=iWholePixelAdder;
							int iWholePixelAdder_TimesSrcBytesPP=iWholePixelAdder*source.iBytesPP;
							indexSrcNearestTL+=iWholePixelAdder_TimesSrcBytesPP;
							indexSrcNearestTR+=iWholePixelAdder_TimesSrcBytesPP;
							indexSrcNearestBL+=iWholePixelAdder_TimesSrcBytesPP;
							indexSrcNearestBR+=iWholePixelAdder_TimesSrcBytesPP;
							/// This clause is also adapted for Y AXIS
						}
						//get nextness BEFORE correcting *RoundedSrc* vars, since *SrcNow vars are uncropped in order to avoid extra calculations
						fxSrcNextness=fxSrcNow-fxRoundedSrcNearestL;//fxRoundedSrcNearestR-fxSrcNow;
						
						//crop X axis of source resampling quads:
						//NOTE: if floating point math were perfect, (int) could be used instead of RMath.IRound for these clauses
						if (fxRoundedSrcNearestL<0.0f) {
							indexSrcNearestL_CroppedByNow=RMath.IRound(0.0f-fxRoundedSrcNearestL)*source.iBytesPP;//- a negative
							indexSrcNearestTL+=indexSrcNearestL_CroppedByNow;//+ a positive
							indexSrcNearestBL+=indexSrcNearestL_CroppedByNow;//+ a positive
						}
						else if (fxRoundedSrcNearestL>fSrc_LastX) {
							indexSrcNearestL_CroppedByNow=RMath.IRound(fSrc_LastX-fxRoundedSrcNearestL)*source.iBytesPP;//- a larger positive
							indexSrcNearestTL+=indexSrcNearestL_CroppedByNow;//+ a negative
							indexSrcNearestBL+=indexSrcNearestL_CroppedByNow;//+ a negative
						}
						if (fxRoundedSrcNearestR<0.0f) {//do NOT do "else if" because source may be 1px tall
							indexSrcNearestR_CroppedByNow=RMath.IRound(0.0f-fxRoundedSrcNearestR)*source.iBytesPP;//- a negative
							indexSrcNearestTR+=indexSrcNearestR_CroppedByNow;//+ a positive
							indexSrcNearestBR+=indexSrcNearestR_CroppedByNow;//+ a positive
						}
						else if (fxRoundedSrcNearestR>fSrc_LastX) { //>=fSrcW 
							indexSrcNearestR_CroppedByNow=RMath.IRound(fSrc_LastX-fxRoundedSrcNearestR)*source.iBytesPP;//- a larger positive
							indexSrcNearestTR+=indexSrcNearestR_CroppedByNow;//+ a negative
							indexSrcNearestBR+=indexSrcNearestR_CroppedByNow;//+ a negative
						}
						if (RowNow_HasMsg) {
							RReporting.sParticiple="done cropping variables for pixel {indexSrcNearestL_CroppedByNow:"+indexSrcNearestL_CroppedByNow+"; indexSrcNearestT_CroppedByNow:"+indexSrcNearestT_CroppedByNow+"; indexSrcNearestR_CroppedByNow:"+indexSrcNearestR_CroppedByNow+"; indexSrcNearestB_CroppedByNow:"+indexSrcNearestB_CroppedByNow+"; indexSrcNearestTL:"+indexSrcNearestTL+"; indexSrcNearestTR:"+indexSrcNearestTR+"; indexSrcNearestBL:"+indexSrcNearestBL+"; indexSrcNearestBR:"+indexSrcNearestBR+"}";
							RowNow_HasMsg=false;
						}
						
						//alpha formula: (foreground*alpha + background*(max-alpha))/max
						//alpha formula: ((Source-Dest)*alpha/255+Dest)
						//This is actually not using alpha, it is using alpha formula to get a subpixel:
						//-Next is treated as the source so that Nextness can be used as the alpha
						//byte byxSrcNextness=RMath.ByRound(fxSrcNextness*255);
						//byte byySrcNextness=RMath.ByRound(fySrcNextness*255);
						//Result is a weighted average (weighted by fySrcNextness) of the 2 row pixels which were blended from 2 pixels per row (weighted by fxSrcNextness)
						
						//ByRound without *255 is okay since 255 is the starting point
						//if (iDest>=0&&(iDest+destination.iBytesPP-1<destination.iBytesTotal)) {
						
						fInness=fRowInness;
						//Inness is ONLY for ALPHA
						//Inness is NOT calculated for source, since even the outer edges don't go beyond (x-.5,y-.5) and (width-.5,height-.5)
						//and therefore the Inness is always 100%, or 1.0
						//NOTE: iDestRect_* vars are inclusive, and are modified above to include any pixels being even partially entered.
						if (ixDestNow==iDestRect_L) {
							fInness*=fRectEdgeAlpha_L;
						}
						else if (ixDestNow==iDestRect_R) {
							fInness*=fRectEdgeAlpha_R;
						}
						
						//if (fInness>0.0f) {
						///if alpha is needed, get the alpha at the source subpixel
						if (source.iBytesPP>3&&bNeedSourceAlpha) {
							bySourceA = RMath.ByRound(  fInness  *  ( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+3]-(float)source.byarrData[indexSrcNearestTL+3])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+3])
												  + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+3]-(float)source.byarrData[indexSrcNearestBL+3])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+3]) )  );
						}
						else bySourceA=255;
						if (destination.iBytesPP>3) {
							//if (destination.iBytesPP>3)
								byDestA=destination.byarrData[iDest+3];
							//else byDestA=255;
						}
						else byDestA=255;
						
						//else if (DrawMode==ROverlay.AlphaDestMode_AlphaBlend) { //aka DrawModeAlphaAlpha
						//	if (destination.iBytesPP>3) {
						//		if (riDest_iBytesPP>3) byDestA=destination.byarrData[iDest+3];
						//		else byDestA=255;
						//		if (source.iBytesPP>3) {
						//			bySourceA = RMath.ByRound(  fInness  *  ( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+3]-(float)source.byarrData[indexSrcNearestTL+3])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+3])
						//								  + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+3]-(float)source.byarrData[indexSrcNearestBL+3])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+3]) )  );
						//		}
						//		else bySourceA=255;
						//		destination.byarrData[iDest+3]=RMath.AlphaLook[bySourceA][byDestA][bySourceA];
						//	}
						//	//iDrawMode=DrawMode_CopyColor_KeepDestAlpha;
						//}//end DrawMode_AlphaColor_KeepDestAlpha (falls through to self: DrawMode_AlphaColor_KeepDestAlpha)
						//TODO: reduce redundant code by using effective submodes determined based on mode and alpha:
						// - then process AlphaOpNow AND ColorOpNow
						// ColorOp_Copy, ColorOp_Skip, ColorOp_Alpha, ColorOp_QuickEdge, ColorOp_HardEdge
						// AlphaOp_Copy, AlphaOp_AlphaAlpha, AlphaOp_Skip, AlphaOp_KeepGreater
						iDestTemp=iDest;
						switch (ColorDestModeDetermined) {
							case ROverlay.ColorDestMode_AlphaQuickEdge://formerly DrawMode_AlphaQuickEdgeColor_KeepDestAlpha:
								if (bySourceA>=170) {
									//B (or gray)
									destination.byarrData[iDestTemp]= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR]-(float)source.byarrData[indexSrcNearestTL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL])
										 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR]-(float)source.byarrData[indexSrcNearestBL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL]) );
									if (destination.iBytesPP>1) {
										iDestTemp++;
										///G
										destination.byarrData[iDestTemp]
											= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+1]-(float)source.byarrData[indexSrcNearestTL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+1])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+1]-(float)source.byarrData[indexSrcNearestBL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+1]) );
										iDestTemp++;
										///R
										destination.byarrData[iDestTemp]
											= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+2]-(float)source.byarrData[indexSrcNearestTL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+2])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+2]-(float)source.byarrData[indexSrcNearestBL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+2]) );
									}
								}
								else if (bySourceA<85) {
									//do nothing
								}
								else {//else do quick (50% via bitshifting) alpha
									///B (or gray)
									destination.byarrData[iDestTemp]
										=RMath.AddBytes[  (( RMath.IRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR]-(float)source.byarrData[indexSrcNearestTL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL])
										 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR]-(float)source.byarrData[indexSrcNearestBL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL]) )
										)>>1) ][ ((destination.byarrData[iDestTemp])>>1)  ];
									if (destination.iBytesPP>1) {
										iDestTemp++;
										///G
										destination.byarrData[iDestTemp]
											=RMath.AddBytes[  (( RMath.IRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+1]-(float)source.byarrData[indexSrcNearestTL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+1])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+1]-(float)source.byarrData[indexSrcNearestBL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+1]) )
											)>>1) ][ ((destination.byarrData[iDestTemp])>>1)  ];
										iDestTemp++;
										///R
										destination.byarrData[iDestTemp]
											=RMath.AddBytes[  (( RMath.IRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+2]-(float)source.byarrData[indexSrcNearestTL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+2])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+2]-(float)source.byarrData[indexSrcNearestBL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+2]) )
											)>>1) ][ ((destination.byarrData[iDestTemp])>>1)  ];
									}
								}
								break;
								
							case ROverlay.ColorDestMode_Copy://formerly DrawMode_CopyColor_CopyAlpha:
								destination.byarrData[iDest]
									 = RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR]-(float)source.byarrData[indexSrcNearestTL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL])
									 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR]-(float)source.byarrData[indexSrcNearestBL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL]) );
								if (destination.iBytesPP>1) {
									destination.byarrData[iDest+1]
										= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+1]-(float)source.byarrData[indexSrcNearestTL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+1])
										+ fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+1]-(float)source.byarrData[indexSrcNearestBL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+1]) );
									destination.byarrData[iDest+2]
										= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+2]-(float)source.byarrData[indexSrcNearestTL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+2])
										+ fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+2]-(float)source.byarrData[indexSrcNearestBL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+2]) );
								}
								break;
				
							//case ColorDestMode_Copy://formerly DrawMode_CopyColor_KeepDestAlpha: //behaves as though source alpha is 255
							//	///B (or gray)
							//	destination.byarrData[iDestTemp] 
							//		 = RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR]-(float)source.byarrData[indexSrcNearestTL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL])
							//		 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR]-(float)source.byarrData[indexSrcNearestBL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL]) );
							//	if (destination.iBytesPP>1) {
							//		iDestTemp++;
							//		///G
							//		destination.byarrData[iDestTemp] 
							//			 = RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+1]-(float)source.byarrData[indexSrcNearestTL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+1])
							//			 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+1]-(float)source.byarrData[indexSrcNearestBL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+1]) );
							//		iDestTemp++;
							//		///R
							//		destination.byarrData[iDestTemp]
							//			 = RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+2]-(float)source.byarrData[indexSrcNearestTL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+2])
							//			 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+2]-(float)source.byarrData[indexSrcNearestBL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+2]) );
							//	}
							//	break;
								
							case ROverlay.ColorDestMode_AlphaBlend://formerly DrawMode_AlphaColor_KeepDestAlpha:
								//if (source.iBytesPP>3) {
								//	bySourceA = RMath.ByRound(  fInness  *  ( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+3]-(float)source.byarrData[indexSrcNearestTL+3])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+3])
								//						  + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+3]-(float)source.byarrData[indexSrcNearestBL+3])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+3]) )  );
								//}
								//else bySourceA=255;
								
								if (bySourceA==255) {
									//B (or gray)
									destination.byarrData[iDestTemp]= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR]-(float)source.byarrData[indexSrcNearestTL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL])
										 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR]-(float)source.byarrData[indexSrcNearestBL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL]) );
									if (destination.iBytesPP>1) {
										iDestTemp++;
										///G
										destination.byarrData[iDestTemp]
											= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+1]-(float)source.byarrData[indexSrcNearestTL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+1])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+1]-(float)source.byarrData[indexSrcNearestBL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+1]) );
										iDestTemp++;
										///R
										destination.byarrData[iDestTemp]
											= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+2]-(float)source.byarrData[indexSrcNearestTL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+2])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+2]-(float)source.byarrData[indexSrcNearestBL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+2]) );
									}
								}
								else if (bySourceA==0) {
									//do nothing
								}
								else { //alpha blend
									///B (or gray)
									destination.byarrData[iDestTemp]
										= RMath.AlphaLook[ RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR]-(float)source.byarrData[indexSrcNearestTL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL])
										 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR]-(float)source.byarrData[indexSrcNearestBL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL]) )
										][destination.byarrData[iDestTemp]][bySourceA];
									if (destination.iBytesPP>1) {
										iDestTemp++;
										///G
										destination.byarrData[iDestTemp]
											= RMath.AlphaLook[ RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+1]-(float)source.byarrData[indexSrcNearestTL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+1])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+1]-(float)source.byarrData[indexSrcNearestBL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+1]) )
											][destination.byarrData[iDestTemp]][bySourceA];
										iDestTemp++;
										///R
										destination.byarrData[iDestTemp]
											= RMath.AlphaLook[ RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+2]-(float)source.byarrData[indexSrcNearestTL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+2])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+2]-(float)source.byarrData[indexSrcNearestBL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+2]) )
											][destination.byarrData[iDestTemp]][bySourceA];
									}
								}
								break;
							case ROverlay.ColorDestMode_AlphaHardEdge://formerly DrawMode_AlphaHardEdgeColor_KeepDestAlpha:
								if (bySourceA>127) {
									//B (or gray)
									destination.byarrData[iDestTemp]= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR]-(float)source.byarrData[indexSrcNearestTL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL])
										 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR]-(float)source.byarrData[indexSrcNearestBL])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL]) );
									if (destination.iBytesPP>1) {
										iDestTemp++;
										///G
										destination.byarrData[iDestTemp]
											= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+1]-(float)source.byarrData[indexSrcNearestTL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+1])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+1]-(float)source.byarrData[indexSrcNearestBL+1])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+1]) );
										iDestTemp++;
										///R
										destination.byarrData[iDestTemp]
											= RMath.ByRound( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+2]-(float)source.byarrData[indexSrcNearestTL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+2])
											 + fySrcNextness*(((float)source.byarrData[indexSrcNearestBR+2]-(float)source.byarrData[indexSrcNearestBL+2])*fxSrcNextness+(float)source.byarrData[indexSrcNearestBL+2]) );
									}
								}
								//else //do nothing
								
								break;
							default:break;
						}//end switch (ColorDestModeDetermined)
						
						if (destination.iBytesPP>3) {
							switch (AlphaDestModeDetermined) {
								case ROverlay.AlphaDestMode_KeepGreater:
									destination.byarrData[iDest+3]=(bySourceA>byDestA)?bySourceA:byDestA;
									break;
								case ROverlay.AlphaDestMode_Copy:
									destination.byarrData[iDest+3] 
										= bySourceA; //RMath.ByRound(  fInness  *  ( (1.0f-fySrcNextness)*(((float)source.byarrData[indexSrcNearestTR+3]-(float)source.byarrData[indexSrcNearestTL+3])*fxSrcNextness+(float)source.byarrData[indexSrcNearestTL+3])
									break;
								case ROverlay.AlphaDestMode_AlphaBlend:
									destination.byarrData[iDest+3] 
										= RMath.AlphaLook[bySourceA][byDestA][bySourceA];
									break;
								//case ROverlay.AlphaDestMode_KeepDestAlpha: break;//do nothing
								default:break;
							}
						}
						//}//if fInness>0.0f (if inside destination at all)
						//}//end if iDest within range
						//else {
						//	RReporting.ShowErr("Skipping pixel out of range (this should never happen)","drawing scaled pixel (vars rounded to hundredth) {iDest:"+iDest.ToString()+"; to_X:"+to_X.ToString()+"; to_Y:"+to_Y.ToString()+"; ixDestNow:"+ixDestNow.ToString()+"; iyDestNow:"+iyDestNow.ToString()+"; fxSrcNow:"+fxSrcNow.ToString()+"; fySrcNow:"+fySrcNow.ToString()+"; fxRoundedSrcNearestL:"+fxRoundedSrcNearestL.ToString()+"; fyRoundedSrcNearestT:"+fyRoundedSrcNearestT.ToString()+"}");
						//}
						//uncrop X axis of source resampling quads:
						if (indexSrcNearestL_CroppedByNow!=0) {
							indexSrcNearestTL-=indexSrcNearestL_CroppedByNow;//- a positive or negative
							indexSrcNearestBL-=indexSrcNearestL_CroppedByNow;//- a positive or negative
						}
						if (indexSrcNearestR_CroppedByNow!=0) {
							indexSrcNearestTR-=indexSrcNearestR_CroppedByNow;//+ a positive or negative
							indexSrcNearestBR-=indexSrcNearestR_CroppedByNow;//+ a positive or negative
						}
						//All of the following lines in the x loop must be done LAST:
						//ONLY do the the following 3 lines if not incremented further down
						iDest+=destination.iBytesPP;
						fxDestWholeNum+=1.0f;
						fxSrcNow+=xInverseScale;//pSrcNow.X+=xInverseScale;
					}//end for ixDestNow
					//uncrop Y axis of source resampling quads:
					if (indexSrcNearestT_CroppedByNow!=0) {
						indexSrcNearestTL-=indexSrcNearestT_CroppedByNow;//- a positive or negative
						indexSrcNearestTR-=indexSrcNearestT_CroppedByNow;//- a positive or negative
					}
					if (indexSrcNearestB_CroppedByNow!=0) {
						indexSrcNearestBL-=indexSrcNearestB_CroppedByNow;//+ a positive or negative
						indexSrcNearestBR-=indexSrcNearestB_CroppedByNow;//+ a positive or negative
					}
					indexWhenDestLineNowStarts+=destination.iStride;
					fyDestWholeNum+=1.0f;
					fySrcNow+=yInverseScale;//pSrcNow.Y+=yInverseScale;
				}//end for iyDestNow
				
				bGood=true;
			}
			catch (Exception exn) {
				string sDestAsStyleAttrib="destination";
				if (destination!=null) {
					if (destination.byarrData!=null) {
						sDestAsStyleAttrib+=".byarrData.Length:"+destination.byarrData.Length.ToString();
					}
					else {
						sDestAsStyleAttrib+=".byarrData:null";
					}
					if (destination.sPathFileBaseName!=null&&destination.sFileExt!=null) {
						sDestAsStyleAttrib+="; DestFile:"+destination.sPathFileBaseName+"."+destination.sFileExt;
					}
				}
				else {
					sDestAsStyleAttrib+=":null";
				}
				string sSrcAsStyleAttrib="source";
				if (source!=null) {
					if (source.byarrData!=null) {
						sSrcAsStyleAttrib+=".byarrData.Length:"+source.byarrData.Length.ToString();
					}
					else {
						sSrcAsStyleAttrib+=".byarrData:null";
					}
					if (source.sPathFileBaseName!=null&&source.sFileExt!=null) {
						sSrcAsStyleAttrib+="; SourceFile:"+source.sPathFileBaseName+"."+source.sFileExt;
					}
				}
				else {
					sSrcAsStyleAttrib+=":null";
				}
				string sDestination_iBytesTotal=(destination!=null)?destination.iBytesTotal.ToString():"null";
				string sSource_iBytesTotal=(source!=null)?source.iBytesTotal.ToString():"null";
				int source_iBytesTotal=(source!=null)?source.iBytesTotal:int.MinValue;
				int destination_iBytesPP=(destination!=null)?destination.iBytesPP:-1;
				int source_iBytesPP=(source!=null)?source.iBytesPP:((destination!=null)?(destination.iBytesPP*-1):-1);
				if (source_iBytesPP==0) source_iBytesPP=int.MinValue;//prevent divide by zero below
				if (destination_iBytesPP==0) destination_iBytesPP=int.MinValue;//prevent divide by zero below
				string sMsg="";
				RString.StringWriteLine(ref sMsg, "drawing to scaled pre-cropped destination ("+RReporting.sParticiple+") {"
					);RString.StringWriteLine(ref sMsg,""
								   +"iDestStart:"+iDestStart.ToString()+"; "
								   +"iDest:"+iDest.ToString()+"; "
								   +"iDestTemp:"+iDestTemp.ToString()+"; "
								   +"destination.iBytesTotal:"+sDestination_iBytesTotal+"; "
								   +sDestAsStyleAttrib+"; "
								   +"destination.Description:"+((destination!=null)?destination.Description():"(null destination object)")+"; "
								   +"destination.iBytesPP:"+((destination_iBytesPP==int.MinValue)?"(zero)":"")+destination_iBytesPP.ToString()+"; "
					);RString.StringWriteLine(ref sMsg,""
								   +"iSrcStart_TL:"+iSrcStart_TL.ToString()+"; "
								   +"iSrcStart_TL/source_iBytesPP:"+(iSrcStart_TL/source_iBytesPP).ToString()+"; "
								   +"iSrcStart_BR:"+iSrcStart_BR.ToString()+"; "
								   +"iSrcStart_BR/source_iBytesPP:"+(iSrcStart_BR/source_iBytesPP).ToString()+"; "
								   +"indexSrcNearestTL:"+indexSrcNearestTL.ToString()+"; "
								   +"indexSrcNearestTR:"+indexSrcNearestTR.ToString()+"; "
								   +"indexSrcNearestBL:"+indexSrcNearestBL.ToString()+"; "
								   +"indexSrcNearestBR:"+indexSrcNearestBR.ToString()+"; "
								   +"source.iBytesTotal:"+sSource_iBytesTotal+"; "
								   +sSrcAsStyleAttrib+"; "
								   +"source.Description:"+((source!=null)?source.Description():"(null source object)")+"; "
								   +"source.iBytesPP:"+((source_iBytesPP==int.MinValue)?"(zero)":"")+source_iBytesPP.ToString()+"; "
								   +"indexSrcNearestTL/iBytesPP:"+(indexSrcNearestTL/source_iBytesPP).ToString()+"; "
								   +"indexSrcNearestTR/iBytesPP:"+(indexSrcNearestTR/source_iBytesPP).ToString()+"; "
								   +"indexSrcNearestBL/iBytesPP:"+(indexSrcNearestBL/source_iBytesPP).ToString()+"; "
								   +"indexSrcNearestBR/iBytesPP:"+(indexSrcNearestBR/source_iBytesPP).ToString()+"; "
								   +"source.iBytesTotal/iBytesPP:"+(source_iBytesTotal/source_iBytesPP).ToString()+"; "
					);RString.StringWriteLine(ref sMsg,""
								   +"to_X:"+to_X.ToString("N")+"; "
								   +"to_Y:"+to_Y.ToString("N")+"; "
								   +"to_W:"+to_W.ToString("N")+"; "
								   +"to_H:"+to_H.ToString("N")+"; "
					);RString.StringWriteLine(ref sMsg,""
								   +"from_X:"+from_X.ToString("N")+"; "
								   +"from_Y:"+from_Y.ToString("N")+"; "
								   +"from_W:"+from_W.ToString("N")+"; "
								   +"from_H:"+from_H.ToString("N")+"; "
								   +"ixDestNow:"+ixDestNow.ToString()+"; "
								   +"iyDestNow:"+iyDestNow.ToString()+"; "
					);RString.StringWriteLine(ref sMsg,""
								   +"fxRoundedSrcNearestL:"+fxRoundedSrcNearestL.ToString("N")+"; "
								   +"fyRoundedSrcNearestT:"+fyRoundedSrcNearestT.ToString("N")+"; "
								   +"fxRoundedSrcNearestR:"+fxRoundedSrcNearestR.ToString("N")+"; "
								   +"fyRoundedSrcNearestB:"+fyRoundedSrcNearestB.ToString("N")+"; "
								   +"fxSrcNow:"+fxSrcNow.ToString("N")+"; "
								   +"fySrcNow:"+fySrcNow.ToString("N")+"; "
								   +"fSrcW:"+fSrcW.ToString("N")+"; "
								   +"fSrcH:"+fSrcH.ToString("N")+"; "
								   +"}"
					);
				
				RReporting.ShowExn(exn,sMsg);
				//RReporting.ShowExn(exn,"drawing to scaled pre-cropped destination {iDest:"+iDest.ToString()+"; to_X:"+to_X.ToString()+"; to_Y:"+to_Y.ToString()+"; ixDestNow:"+ixDestNow.ToString()+"; iyDestNow:"+iyDestNow.ToString()+"; fxSrcNow:"+fxSrcNow.ToString()+"; fySrcNow:"+fySrcNow.ToString()+"; fxRoundedSrcNearestL:"+fxRoundedSrcNearestL.ToString()+"; fyRoundedSrcNearestT:"+fyRoundedSrcNearestT.ToString()+"}");
				//RReporting.ShowExn(exn,"drawing to scaled pre-cropped destination");
				bGood=false;
			}
			return bGood;
		}//end DrawTo

		/// <summary>
		/// Simple DrawFrom [calls DrawTo(RImage,IPoint,RImage,IRect,int)].
		/// Does NOT check cropping rect; STOPS if out of range of EITHER image.
		/// </summary>
		/// <param name="ipDest"></param>
		/// <param name="source"></param>
		/// <param name="rectSrc"></param>
		/// <returns></returns>
		public bool DrawFrom(IPoint ipDest, RImage source, IRect rectSrc) {
			return DrawTo(this, ipDest, source, rectSrc, DrawMode_CopyColor_CopyAlpha);
		}
		/// <summary>
		/// Calls DrawTo(RImage,IPoint,RImage,IRect,int), which DOES NOT automatically crop;
		/// STOPS if beyond range of EITHER image.
		/// </summary>
		/// <param name="ipDest"></param>
		/// <param name="source"></param>
		/// <param name="rectSrc"></param>
		/// <returns></returns>
		public bool DrawFrom(IPoint ipDest, RImage source, IRect rectSrc, int iDrawMode) {
			return DrawTo(this,ipDest,source,rectSrc,iDrawMode);
		}
		
		/// <summary>
		/// Calls DrawFrom(IPoint,RImage,int), which DOES automatically crop, then selects appropriate static method [selects DrawToLargerWithoutCropElseCancel OR DrawTo(RImage,IPoint,RImage,IRect,int)].
		/// --DEFAULTS TO DrawMode_CopyColor_CopyAlpha since no DrawMode_ is specified in this overload.
		/// </summary>
		/// <param name="ipDest"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public bool DrawFrom(IPoint ipDest, RImage source) {
			return DrawFrom(ipDest, source, DrawMode_CopyColor_CopyAlpha);
		}
		/// <summary>
		/// DOES automatically crop, then calls static method [selects DrawToLargerWithoutCropElseCancel OR DrawTo(RImage,IPoint,RImage,IRect,int)].
		/// </summary>
		/// <param name="ipDest"></param>
		/// <param name="source"></param>
		/// <param name="iDrawMode"></param>
		/// <returns></returns>
		public bool DrawFrom(IPoint ipDest, RImage source, int iDrawMode) {
			return DrawTo(this,ipDest,source,iDrawMode);
		}//end DrawFrom

		/// <summary>
		/// DOES automatically crop, primary overload for automatically-cropped version [selects DrawToLargerWithoutCropElseCancel OR DrawTo(RImage,IPoint,RImage,IRect,int)].
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="ipDest"></param>
		/// <param name="source"></param>
		/// <param name="iDrawMode"></param>
		/// <returns></returns>
		public static bool DrawTo(RImage destination, IPoint ipDest, RImage source, int iDrawMode) {
			bool bGood=false;
			try {
				if (destination.CanFit(source,ipDest.X,ipDest.Y)) {
					bGood=RImage.DrawToLargerWithoutCropElseCancel(destination,ipDest.X,ipDest.Y,source,iDrawMode);
				}
				else {
					IRect rectSource = new IRect(0,0,source.Width,source.Height); //for automatic cropping (see below)
					//Console.Error.Write("DrawFrom...cropping....");
					//Console.Error.Flush();
					RImage.CropSourceRectAndGetNewDest(ref rectSource.X, ref rectSource.Y, ref rectSource.Width, ref rectSource.Height, ref ipDest.X, ref ipDest.Y, destination);
					//Console.Error.Write("Drawing image from "+rectSource.ToString()+" to "+ipDest.ToString()+"...");//debug only
					//Console.Error.Flush();
					if (rectSource.Width<=0||rectSource.Height<=0) {
						if (RReporting.bDebug) RReporting.Debug("Image drawing was ignored since source was completely outside of destination.","drawing image","DrawTo(RImage,IPoint,RImage,int)");
						bGood=true;
					}
					else {
						bGood=RImage.DrawTo(destination, ipDest, source, rectSource, iDrawMode);
					}
					//Console.Error.WriteLine(bGood?"OK":"FAIL");
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing cropped image","DrawTo(RImage,IPoint,RImage,int)");
			}
			return bGood;
		}//end DrawTo
		
		public bool DrawFromSmallerWithoutCropElseCancel(int xDest, int yDest, RImage source) {
			return DrawToLargerWithoutCropElseCancel(this, xDest, yDest, source, DrawMode_CopyColor_CopyAlpha);
		}
		public bool DrawFromSmallerWithoutCropElseCancel(int xDest, int yDest, RImage source, int iDrawMode) {
			return DrawToLargerWithoutCropElseCancel(this, xDest, yDest, source, iDrawMode);
		}
		
		///<summary>
		///Draws riSrc to riDest if riSrc is smaller AND doesn't need to be cropped.
		///--if riSrc is Grayscale and riDest is RGB: Uses foreground brush (RImage.rpaintFore) for hue, gray image for alpha
		///</summary>
		public static bool DrawToLargerWithoutCropElseCancel(RImage riDest, int xDest, int yDest, RImage riSrc, int iDrawMode) { //formerly DrawFromSmallerWithoutCropElseCancel
			bool bGood=true;
			int x=0,y=0;
			if (riDest==null||riDest.byarrData==null) {
				RReporting.ShowErr("Tried to draw to null buffer!","","DrawToLargerWithoutCropElseCancel");
				return false;
			}
			else if (riSrc==null||riSrc.byarrData==null) {
				RReporting.ShowErr("Tried to draw null buffer!","","DrawToLargerWithoutCropElseCancel");
				return false;
			}
			try {
				byte[] riDest_byarrData=riDest.byarrData;
				byte[] riSrc_byarrData=riSrc.byarrData;
				//int riSrc_iBytesPP=riSrc.iBytesPP;
				//int riDest_iBytesPP=riDest.iBytesPP;
				if (bShowDrawSmallerWithoutCropElseCancelMsg||RReporting.bMegaDebug) {
					//Console.Error.Write("DrawToLargerWithoutCropElseCancel("+riDest.Description()+",("+xDest.ToString()+","+yDest.ToString()+"),"+riSrc.Description()+","+DrawMode_ToString(iDrawMode)+") "+(RReporting.bMegaDebug?"(bMegaDebug showing all attempts)":"(only showing first attempt)")+"...");//debug only
					//Console.Error.Flush(); //debug only
				}
				if (!riDest.CanFit(riSrc,xDest,yDest)) {//if (riSrc.NeedsToCropToFitInto(this,xDest,yDest)) {
					if (bFirstCancellationOfDrawSmallerWithoutCropElseCancel) {
						RReporting.DebugWrite("failed since not in bounds("+riSrc.iWidth.ToString()+"x"+riSrc.iHeight.ToString()+" to "+riDest.iWidth.ToString()+"x"+riDest.iHeight.ToString()+" at ("+xDest.ToString()+","+yDest.ToString()+") )...");
						bFirstCancellationOfDrawSmallerWithoutCropElseCancel=false;
					}
					RReporting.Warning("Cancelling DrawToLargerWithoutCropElseCancel {xDest:"+xDest.ToString()+"; yDest:"+yDest.ToString()+"; riSrc:"+riSrc.Description()+"; riDest:"+riDest.Description()+"}");
					bGood=false;
					//Console.Error.Write("FAIL (doesn't fit)");//debug only
				}
				if (bGood) {
					float fCookedAlpha;
					int iLocDestLine=yDest*riDest.iStride + xDest*riDest.iBytesPP;
					int iDestPix;
					int iLocSrcLine=0;
					int iSrcPix;
					int riDest_iBytesPP=riDest.iBytesPP;//reduces pointer addition
					int riSrc_iBytesPP=riSrc.iBytesPP;//reduces pointer addition
					int iMinBytesPP=riSrc_iBytesPP<riDest_iBytesPP?riSrc_iBytesPP:riDest_iBytesPP;
					int iDestRToB=riDest.iBytesPP-2;
					if (iDestRToB<0) iDestRToB=0;
					int iSrcRToB=riSrc.iBytesPP-2;
					if (iSrcRToB<0) iSrcRToB=0;
					//byte* lpDestLine=&riDest_byarrData[yDest*riDest.iStride+xDest*riDest.iBytesPP];
					//byte* lpDestPix;
					//byte* lpSrcLine=riSrc_byarrData;
					//byte* lpSrcPix;
					int iStrideMin=(riSrc.iStride<riDest.iStride)?riSrc.iStride:riDest.iStride;
					if (riDest.iBytesPP>=3 && riSrc.iBytesPP>=3) {
						switch (iDrawMode) {
						case DrawMode_CopyColor_CopyAlpha:
							//if (riDest.iStride==riSrc.iStride && riDest.iBytesPP==iBytesPP && xDest==0 && yDest==0) {
							if (riSrc.IsInStrideWithAndSameSizeAs(riDest) && xDest==0 && yDest==0) {
								RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, 0, 0, riSrc.iBytesTotal);
							}
							else {
								if (riDest.iBytesPP==riSrc.iBytesPP) {
									for (y=0; y<riSrc.iHeight; y++) {
										RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iLocDestLine, iLocSrcLine, iStrideMin);
										iLocDestLine+=riDest.iStride;//lpDestLine+=riDest.iStride;
										iLocSrcLine+=riSrc.iStride;//lpSrcLine+=riSrc.iStride;
									}
								}
								else {
									for (y=0; y<riSrc.iHeight; y++) {
										iSrcPix=iLocSrcLine;
										iDestPix=iLocDestLine;
										for (x=0; x<riSrc.iWidth; y++) {
											RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iDestPix, iSrcPix, iMinBytesPP);
											iDestPix+=riDest_iBytesPP;
											iSrcPix+=riSrc_iBytesPP;
										}
										iLocSrcLine+=riSrc.iStride;
										iLocDestLine+=riDest.iStride;
									}
								}
							}
							break;
						case DrawMode_AlphaColor_KeepDestAlpha:
							//alpha formula: ((Source-Dest)*alpha/255+Dest)
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_iBytesPP<4||riSrc_byarrData[iSrcPix+3]==255) {
										//if <4, just copy the same bytes anyway
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData,iDestPix, iSrcPix, iMinBytesPP);
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else if (riSrc_byarrData[iSrcPix+3]==0) {
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else {
										fCookedAlpha=(float)riSrc_byarrData[iSrcPix+3]/255.0f;
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(riSrc_byarrData[iSrcPix]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //B
										iSrcPix++; iDestPix++;
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(riSrc_byarrData[iSrcPix]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //G
										iSrcPix++; iDestPix++;
										riDest_byarrData[iDestPix]=RMath.ByRound(((float)(riSrc_byarrData[iSrcPix]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //R
										iSrcPix+=iSrcRToB; iDestPix+=iDestRToB;
									}
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						case DrawMode_AlphaQuickEdgeColor_KeepDestAlpha:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_iBytesPP<4||riSrc_byarrData[iSrcPix+3]>170) {
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData, iDestPix, iSrcPix, iMinBytesPP);
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else if (riSrc_byarrData[iSrcPix+3]<=85) {
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else {
										riDest_byarrData[iDestPix]=(byte)( (riSrc_byarrData[iSrcPix]>>2) + (riDest_byarrData[iDestPix]>>2) ); //B
										iSrcPix++; iDestPix++;
										riDest_byarrData[iDestPix]=(byte)( (riSrc_byarrData[iSrcPix]>>2) + (riDest_byarrData[iDestPix]>>2) ); //G
										iSrcPix++; iDestPix++;
										riDest_byarrData[iDestPix]=(byte)( (riSrc_byarrData[iSrcPix]>>2) + (riDest_byarrData[iDestPix]>>2) ); //R
										iSrcPix+=iSrcRToB; iDestPix+=iDestRToB;
									}
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						case DrawMode_AlphaHardEdgeColor_KeepDestAlpha:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_iBytesPP<4||riSrc_byarrData[iSrcPix+3]>=128) {
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData, iDestPix, iSrcPix, iMinBytesPP);
									}
									//else do nothing since alpha below threshold
									iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
								}//end for x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
								////Console.Error.Write("-");//debug only
							}//end for y
							break;
						case DrawMode_AlphaColor_KeepGreaterAlpha:
							//alpha result: ((Source-Dest)*alpha/255+Dest)
							byte bySrcA=255;//reduce pointer multiplication in "if" clauses and blending scenario
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_iBytesPP==4) bySrcA=riSrc_byarrData[iSrcPix+3];
									if (bySrcA==255) {
										RMemory.CopyFast(ref riDest_byarrData,ref riSrc_byarrData, iDestPix, iSrcPix, iMinBytesPP);
										//NOTE: ok to copy iMinBytesPP since KeepGreaterAlpha will treat 24-bit source as 0 alpha, and 255 alpha source is always copied over old alpha
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else if (bySrcA==0) {
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else {
										riDest_byarrData[iDestPix]=RMath.AlphaLook[riSrc_byarrData[iSrcPix]][riDest_byarrData[iDestPix]][bySrcA]; //B
										iSrcPix++; iDestPix++;
										
										riDest_byarrData[iDestPix]=RMath.AlphaLook[riSrc_byarrData[iSrcPix]][riDest_byarrData[iDestPix]][bySrcA]; //G
										iSrcPix++; iDestPix++;
										
										riDest_byarrData[iDestPix]=RMath.AlphaLook[riSrc_byarrData[iSrcPix]][riDest_byarrData[iDestPix]][bySrcA]; //R
										iSrcPix++; iDestPix++;
										if (riDest_iBytesPP==4) {
											if (riSrc_iBytesPP==4) {
												riDest_byarrData[iDestPix]=(bySrcA>riDest_byarrData[iDestPix])?bySrcA:riDest_byarrData[iDestPix]; //A
												iSrcPix++;
											}
											//else treat source alpha as if zero for keeping dest alpha, if no source alpha in KeepGreaterAlpha mode
											iDestPix++;
										}
										else if (riSrc_iBytesPP==4) {//dest does not have alpha but source does
											iSrcPix++;
										}
									}
								}//end for x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end for y
							break;
						case DrawMode_CopyColor_KeepDestAlpha:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData,iDestPix,iSrcPix,iMinBytesPP);
									iDestPix+=riDest_iBytesPP; iSrcPix+=riSrc_iBytesPP;
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						}//end switch
						if (bShowDrawSmallerWithoutCropElseCancelMsg||RReporting.bMegaDebug) {
							//Console.Error.WriteLine("OK (32-bit to 32-bit)");//debug only
							bShowDrawSmallerWithoutCropElseCancelMsg=false;
						}
					}//end both at least 24-bit
					else if (riSrc.iBytesPP==1&&riDest.iBytesPP>=3) {
						//8-bit to 32-bit
						int iDestNonAlphaChannels=(riDest_iBytesPP>=3)?3:riDest_iBytesPP;
						switch (iDrawMode) {
						case DrawMode_CopyColor_CopyAlpha:
						//if (riDest.iStride==riSrc.iStride && riDest.iBytesPP==iBytesPP && xDest==0 && yDest==0) {
							for (y=0; y<riSrc.iHeight; y++) {
								int iLocDest=iLocDestLine;
								int iLocSrc=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									RMemory.CopyFast(ref riDest_byarrData, ref rpaintFore.data32, iLocDest, 0, iDestNonAlphaChannels); //assumes data32 has same channels as dest
									if (riDest_iBytesPP==4) riDest_byarrData[riDest_iBytesPP+3]=riSrc_byarrData[iLocSrc];//use gray as alpha
									iLocDest+=riDest_iBytesPP;
									iLocSrc+=riSrc_iBytesPP;
								}
								iLocDestLine+=riDest.iStride;//lpDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;//lpSrcLine+=riSrc.iStride;
							}
							break;
						case DrawMode_AlphaColor_KeepDestAlpha:
							//alpha result: ((Source-Dest)*alpha/255+Dest)
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix]==0) {//assumes 8-bit
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else if (riSrc_byarrData[iSrcPix]==255) {//assumes 8-bit
										RMemory.CopyFast(ref riDest_byarrData,ref rpaintFore.data32,iDestPix, 0, iDestNonAlphaChannels);//assumes data32 has same channels as dest
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else {
										//fCookedAlpha=(float)riSrc_byarrData[iSrcPix]/255.0f;
										
										//the operations below assume data32 has same channels as dest
										
										riDest_byarrData[iDestPix]=RMath.AlphaLook[rpaintFore.data32[0]][riDest_byarrData[iDestPix]][riSrc_byarrData[iSrcPix]];//R; assumes 8-bit gray as alpha //riDest_byarrData[iDestPix]=RMath.ByRound(((float)(rpaintFore.data32[0]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //B
										iDestPix++;
										//iSrcPix++; //being commented assumes grayscale
										
										riDest_byarrData[iDestPix]=RMath.AlphaLook[rpaintFore.data32[1]][riDest_byarrData[iDestPix]][riSrc_byarrData[iSrcPix]];//R; assumes 8-bit gray as alpha //riDest_byarrData[iDestPix]=RMath.ByRound(((float)(rpaintFore.data32[1]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //G
										iDestPix++; 
										//iSrcPix++;  //being commented assumes grayscale
										
										riDest_byarrData[iDestPix]=RMath.AlphaLook[rpaintFore.data32[2]][riDest_byarrData[iDestPix]][riSrc_byarrData[iSrcPix]];//R; assumes 8-bit gray as alpha //riDest_byarrData[iDestPix]=RMath.ByRound(((float)(rpaintFore.data32[2]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //R
										iSrcPix+=riSrc_iBytesPP;//assumes not incremented during copy above
										iDestPix+=iDestRToB;
									}//end else neither 0 nor 255
								}//end x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end y
							break;
						case DrawMode_AlphaQuickEdgeColor_KeepDestAlpha:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix]<=85) {//assumes 8-bit
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else if (riSrc_byarrData[iSrcPix]>170) {//assumes 8-bit
										RMemory.CopyFast(ref riDest_byarrData, ref rpaintFore.data32, iDestPix, 0, iDestNonAlphaChannels); //assumes data32 has same channels as dest
										iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
									}
									else {//blend halfway if in middle third of range
										//the operations below assume data32 has same channels as dest
										riDest_byarrData[iDestPix]=(byte)( (rpaintFore.data32[0]>>2) + (riDest_byarrData[iDestPix]>>2) ); //B //copy from rpaint assumes 8-bit source
										//iSrcPix++;//commented assumes 8-bit source
										iDestPix++;
										riDest_byarrData[iDestPix]=(byte)( (rpaintFore.data32[1]>>2) + (riDest_byarrData[iDestPix]>>2) ); //G //copy from rpaint assumes 8-bit source
										//iSrcPix++;//commented assumes 8-bit source
										iDestPix++;
										riDest_byarrData[iDestPix]=(byte)( (rpaintFore.data32[2]>>2) + (riDest_byarrData[iDestPix]>>2) ); //R //copy from rpaint assumes 8-bit source
										iSrcPix+=riSrc_iBytesPP;//assumes not incremented during copy above
										iDestPix+=iDestRToB;
									}
								}//end for x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end for y
							break;
						case DrawMode_AlphaHardEdgeColor_KeepDestAlpha:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix]>=128) {
										RMemory.CopyFast(ref riDest_byarrData,ref rpaintFore.data32, iDestPix, 0, iDestNonAlphaChannels); //assumes data32 has same channels as dest
									}
									iSrcPix+=riSrc_iBytesPP; iDestPix+=riDest_iBytesPP;
								}//end for x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end for y
							break;
						case DrawMode_AlphaColor_KeepGreaterAlpha:
								//alpha result: ((Source-Dest)*alpha/255+Dest)
							byte bySrc;
							byte byDest;
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									if (riSrc_byarrData[iSrcPix]==0) {//assumes 8-bit
										iSrcPix++;//assumes 8-bit
										iDestPix+=riDest_iBytesPP;
									}
									else if (riSrc_byarrData[iSrcPix]==255) {//assumes 8-bit
										RMemory.CopyFast(ref riDest_byarrData,ref rpaintFore.data32,iDestPix, iSrcPix, iDestNonAlphaChannels); ////assumes data32 has same channels as dest
										if (riDest_iBytesPP==4) riDest_byarrData[iDestPix+3]=255;//use riSrc_byarrData[iSrcPix]==255 (assumes 8-bit gray as alpha)
										iSrcPix++;//assumes 8-bit
										iDestPix+=riDest_iBytesPP;
									}
									else {
										bySrc=riSrc_byarrData[iSrcPix];//used as alpha below://fCookedAlpha=(float)riSrc_byarrData[iSrcPix]/255.0f;
										riDest_byarrData[iDestPix]=RMath.AlphaLook[rpaintFore.data32[0]][riDest_byarrData[iDestPix]][bySrc]; //B; assumes data32 has same channels as dest and using 8-bit gray source as alpha; //riDest_byarrData[iDestPix]=RMath.ByRound(((float)(rpaintFore.data32[0]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //B
										iDestPix++;
										//iSrcPix++; //being commented assumes grayscale
										riDest_byarrData[iDestPix]=RMath.AlphaLook[rpaintFore.data32[1]][riDest_byarrData[iDestPix]][bySrc]; //G; assumes data32 has same channels as dest and using 8-bit gray source as alpha; //riDest_byarrData[iDestPix]=RMath.ByRound(((float)(rpaintFore.data32[1]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //G
										iDestPix++; 
										//iSrcPix++;  //being commented assumes grayscale
										riDest_byarrData[iDestPix]=RMath.AlphaLook[rpaintFore.data32[2]][riDest_byarrData[iDestPix]][bySrc]; //R; assumes data32 has same channels as dest and using 8-bit gray source as alpha; //riDest_byarrData[iDestPix]=RMath.ByRound(((float)(rpaintFore.data32[2]-riDest_byarrData[iDestPix]))*fCookedAlpha+riDest_byarrData[iDestPix]); //R
										iDestPix++;
										if (riDest_iBytesPP==4) {
											//bySrc=riSrc_byarrData[iSrcPix]; //should already be set to this above
											byDest=riDest_byarrData[iDestPix];//dest should now be at A (incremented past R above)
											riDest_byarrData[iDestPix]=byDest>bySrc?byDest:bySrc; //dest should now be at A (incremented past R above)
											iDestPix++; //ok since riDest_iBytesPP==4
										}
										iSrcPix+=riSrc_iBytesPP;
									}//end else neither 0 nor 255
								}//end x
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}//end y
							break;
						case DrawMode_CopyColor_KeepDestAlpha:
							for (y=0; y<riSrc.iHeight; y++) {
								iDestPix=iLocDestLine;
								iSrcPix=iLocSrcLine;
								for (x=0; x<riSrc.iWidth; x++) {
									RMemory.CopyFast(ref riDest_byarrData, ref rpaintFore.data32,iDestPix,0,iDestNonAlphaChannels);//copy from rpaintFore.data32 0 assumes 8-bit source; assumes data32 has same channels as dest
									iDestPix+=riDest_iBytesPP;
									iSrcPix+=riSrc_iBytesPP;
								}
								iLocDestLine+=riDest.iStride;
								iLocSrcLine+=riSrc.iStride;
							}
							break;
						}//end switch
						if (bShowDrawSmallerWithoutCropElseCancelMsg||RReporting.bMegaDebug) {
							//Console.Error.WriteLine("OK");//debug only
							bShowDrawSmallerWithoutCropElseCancelMsg=false;
						}
						if (bShowDrawSmallerWithoutCropElseCancelMsg||RReporting.bMegaDebug) {
							//Console.Error.WriteLine("OK (8-bit to 32-bit)");//debug only
							bShowDrawSmallerWithoutCropElseCancelMsg=false;
						}
					}//end 8-bit to 32-bit
					else {
						if (bShowDrawSmallerWithoutCropElseCancelMsg||RReporting.bMegaDebug) {
							//Console.Error.WriteLine("FAILED (non-implemented bitdepth conversion)");//debug only
							bShowDrawSmallerWithoutCropElseCancelMsg=false;
						}
						RReporting.ShowErr("Can't DrawFrom unless both images are 24-bit BGR or 32-bit BGRA, or source is 8-bit and dest is 24-bit BGR or 32-bit BGRA.  This method is designed for fast copying between images with similar color formats.","","RImage DrawToLargerWithoutCropElseCancel {"+"riDest.iBytesPP:"+riDest.iBytesPP.ToString()+ "; iBytesPP:"+riSrc.iBytesPP.ToString()+"}");
					}
				}//end if does not need to crop
				else {
					if (bShowDrawSmallerWithoutCropElseCancelMsg||RReporting.bMegaDebug) {
						//Console.Error.WriteLine("FAILED (crop needed--should have used cropping method)");//debug only
						bShowDrawSmallerWithoutCropElseCancelMsg=false;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				if (bShowDrawSmallerWithoutCropElseCancelMsg||RReporting.bMegaDebug) {
					//Console.Error.WriteLine("FAILED (could not finish)");//debug only
					bShowDrawSmallerWithoutCropElseCancelMsg=false;
				}
				RReporting.ShowExn(exn,"drawing image to larger skipping if clipped","RImage DrawToLargerWithoutCropElseCancel");
			}
			return bGood;
		}//end DrawToLargerWithoutCropElseCancel
		
		/// <summary>
		/// Bitdepth-Ignorant copy; does not crop, only prevents overflow
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="ipSrc"></param>
		/// <param name="riSrc"></param>
		/// <returns></returns>
		public static bool DrawToSmallerCopyAlphaSaferBitdepthInsensitive(ref RImage riDest, ref IPoint ipSrc, ref RImage riSrc) { //formerly OverlayCropRawSafer formerly RawCropSafer //aka Bitdepth-insensitive
			bool bGood=true;
			int iByDest=0;
			int iBySrc;//=ipSrc.Y*riSrc.iStride+ipSrc.X*riSrc.iBytesPP;
			int yDest;
			int xDest;
			//int src_WhenLineStarts=
			try {
				if (riDest.iBytesPP!=riSrc.iBytesPP) throw new ApplicationException("Mismatched image bitdepths, refusing to copy byte-for-byte!");
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
				RReporting.ShowExn(exn,"copying byte-for-byte","RImage DrawToSmallerCopyAlphaSaferBitdepthInsensitive");
				bGood=false;
			}
			return bGood;
		}//end DrawToSmallerCopyAlphaSaferBitdepthInsensitive
		
		/// <summary>
		/// Bitdepth-Ignorant BYTE ARRAY version: riDest must be true color 24- or 32-bit for the raw source
		/// to be represented correctly.
		/// </summary>
		/// <param name="byarrSrc"></param>
		/// <param name="riDest"></param>
		/// <param name="iSrcWidth"></param>
		/// <param name="iSrcHeight"></param>
		/// <param name="iSrcBytesPP"></param>
		/// <returns></returns>
		public static bool DrawToLargerNoClipBitdepthInsensitive(ref RImage riDest, ref IPoint ipAt, ref byte[] byarrSrc, int iSrcWidth, int iSrcHeight, int iSrcBytesPP) { //formerly OverlayToBigNoClipRaw formerly RawOverlayNoClipToBig
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			int iDestAdder;
			try {
				if (iSrcBytesPP==16) {
					RReporting.ShowErr("16-bit source isn't implemented in this function","overlaying 16-bit source image","RImage DrawToLargerNoClip");
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
					RReporting.ShowErr("Error copying graphics buffer data","","RImage DrawToLargerNoClip");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing to larger array without checking clipping copying all channels","RImage DrawToLargerNoClipBitdepthInsensitive");
				bGood=false;
			}
			return bGood;
		}//end DrawToLargerNoClip

		/// <summary>
		/// Version of Alpha overlay with RGradient as lightmap
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="riSrc"></param>
		/// <param name="ipDest"></param>
		/// <param name="rgradientLightmap"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool DrawToLargerNoClip(ref RImage riDest, ref IPoint ipDest, ref RImage riSrc, ref RGradient rgradientLightmap, int iSrcChannel) { //formerly OverlayToBigNoClip formerly OverlayNoClipToBig
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
						if (!rgradientLightmap.ShadeAlpha(ref riDest.byarrData, iDestByte, riSrc.byarrData[iSrcByte])) {
							bGood=false;
						}
						iSrcByte+=riSrc.iBytesPP;
						iDestByte+=riDest.iBytesPP;
					}
					iDestByte+=iDestAdder;
				}
				if (!bGood) {
					RReporting.ShowErr("Error accessing gradient","","RImage DrawToLargerNoClip gradient to "+IPoint.ToString(ipDest));
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing to larger without checking clipping using gradient","RImage DrawToLargerNoClip gradient to "+ipDest!=null?ipDest.ToString():"null point");
				bGood=false;
			}
			return bGood;
		}//end DrawToLargerNoClip, using gradient
		
		
		/// <summary>
		/// CopyAlpha (no blending) overlay, using gradient as lightmap
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="riSrc"></param>
		/// <param name="rgradientLightmap"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool DrawToLargerNoClipCopyAlpha(ref RImage riDest, ref IPoint ipAt, ref RImage riSrc, ref RGradient rgradientLightmap, int iSrcChannel) { //formerly OverlayToBigNoClipCopyAlpha formerly OverlayNoClipToBigCopyAlpha
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
						if (!rgradientLightmap.Shade(ref riDest.byarrData, iDestByte, riSrc.byarrData[iSrcByte])) {
							bGood=false;
						}
						iSrcByte+=riSrc.iBytesPP;
						iDestByte+=riDest.iBytesPP;
					}
					iDestByte+=iDestAdder;
				}
				if (!bGood) {
					RReporting.ShowErr("Error copying graphics buffer data","","RImage DrawToLargerNoClipCopyAlpha gradient");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing to larger without checking clipping copying alpha","RImage DrawToLargerNoClipCopyAlpha gradient");
				bGood=false;
			}
			return bGood;
		} //end DrawToLargerNoClipCopyAlpha gradient
		
		/// <summary>
		/// CopyAlpha overlay function.  
		/// "ToBig" functions must overlay small
		/// image to big image without cropping else unexpected results occur.
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="riSrc"></param>
		/// <returns></returns>
		public static bool DrawToLargerNoClipCopyAlpha(ref RImage riDest, IPoint ipAt, ref RImage riSrc) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			try {
				iDestByte=ipAt.Y*riDest.iStride+ipAt.X*riDest.iBytesPP;
				iSrcByte=0;
				int iMinStride=riSrc.iStride<riDest.iStride?riSrc.iStride:riDest.iStride;
				if (riDest.iBytesPP==riSrc.iBytesPP) {
					for (int ySrc=0; ySrc<riSrc.iHeight; ySrc++) {
						if (!RMemory.CopyFast(ref riDest.byarrData, ref riSrc.byarrData, iDestByte, iSrcByte, iMinStride)) {
							bGood=false;
						}
						iSrcByte+=riSrc.iStride;
						iDestByte+=riDest.iStride;
					}
					if (!bGood) {
						RReporting.ShowErr("Error copying graphics buffer data","","RImage DrawToLargerNoClipCopyAlpha");
					}
				}
				else {
					bGood=false;
					RReporting.ShowErr("DrawToLargerNoClipCopyAlpha: Nothing to do--bitdepths must match.");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing to larger without checking clipping copying alpha with fast pointers","RImage DrawToLargerNoClipCopyAlpha");
				bGood=false;
			}
			return bGood;
		} //end DrawToLargerNoClipCopyAlpha
		
		/// <summary>
		/// Safe version does NOT use RMemory copy method.
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="riSrc"></param>
		/// <returns></returns>
		public static bool DrawToLargerNoClipCopyAlphaSafe(ref RImage riDest, IPoint ipAt, ref RImage riSrc) { //formerly OverlayToBigNoClipCopyAlphaSafe formerly OverlayNoClipToBigCopyAlphaSafe
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
					RReporting.ShowErr("Error copying graphics buffer data","","RImage DrawToLargerNoClipCopyAlpha");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing to larger copying alpha without checking clipping (using fast array copy if in range)","RImage DrawToLargerNoClipCopyAlphaSafe");
				bGood=false;
			}
			return bGood;
		} //end DrawToLargerNoClipCopyAlphaSafe
		
		/// <summary>
		/// Version of iDrawMode-based Draw using area cropped by rectangle--primary overload of pre-cropped version
		/// --Does not check cropping rect--STOPS if out of range of either image!.
		/// </summary>
		/// <param name="riDest"></param>
		/// <param name="ipDest"></param>
		/// <param name="riSrc"></param>
		/// <param name="rectSrc"></param>
		/// <param name="iDrawMode"></param>
		/// <returns></returns>
		public static bool DrawTo(RImage riDest, IPoint ipDest, RImage riSrc, IRect rectSrc, int iDrawMode) {
			bool bGood=false;
			try {
				int xSrc=rectSrc.X;
				int ySrc=rectSrc.Y;
				int xDest=ipDest.X;
				int yDest=ipDest.Y;
				int xDestRel=0;
				int yDestRel=0;
				int DestLineNow_StartIndex=riDest.XYToLocation(xDest,yDest);
				int SrcLineNow_StartIndex=riSrc.XYToLocation(xSrc,ySrc);
				int iDest=DestLineNow_StartIndex;
				int iSrc=SrcLineNow_StartIndex;
				//Console.Error.WriteLine("debug DrawTo: "+riDest.Description()+"; at:"+ipDest.ToString()+"; from:"+riSrc.Description()+"; from rect:"+rectSrc.ToString()+"; DrawMode_:"+DrawMode_ToString(iDrawMode));//debug only
				if (DestLineNow_StartIndex>=0&&SrcLineNow_StartIndex>=0) {
					bGood=true;
					while (yDestRel<riDest.Height&&(ySrc<rectSrc.Y+rectSrc.Height)) { //OK SINCE yDestRel is incremented when xDestRel is past RIGHT EDGE (see below)!
						if (!DrawPixel(riDest,iDest,riSrc,iSrc,iDrawMode)) {
							RReporting.ShowErr("Could not draw pixel","drawing from image to image","rimage_32bgra DrawFrom(riDest="+(riDest!=null?riDest.Description():"null")+", ipDest="+(ipDest!=null?ipDest.ToString():"null")+", riSrc="+(riSrc!=null?riSrc.Description():"null")+", rectSrc="+(rectSrc!=null?rectSrc.ToString():"null")+", DrawMode_="+RImage.DrawMode_ToString(iDrawMode)+") {xDest:"+xDest+"; yDest:"+yDest+"; xSrc:"+xSrc+"; ySrc:"+ySrc+"; iSrc:"+iSrc+"; iDest:"+iDest+"}");
							bGood=false;
							break;
						}
						xDestRel++;
						xSrc++;
						xDest++;
						iSrc+=riSrc.iBytesPP;
						iDest+=riDest.iBytesPP;
						if (xSrc>=rectSrc.X+rectSrc.Width) {//if (xDestRel>=rectDest.Width) {
							DestLineNow_StartIndex+=riDest.iStride;
							SrcLineNow_StartIndex+=riSrc.iStride;
							iDest=DestLineNow_StartIndex;
							iSrc=SrcLineNow_StartIndex;
							xDestRel=0;
							yDestRel++;
							ySrc++;
							yDest++;
							xSrc=rectSrc.X;
							xDest=ipDest.X;
						}
					}
				}
				else {
					RReporting.ShowErr("Cannot draw image at specified overlay location.","","DrawFrom(riDest,ipDest,riSrc,rectSrc,iDrawMode){ipDest:"+ipDest.ToString()+"; rectSrc:"+IRect.ToString(rectSrc)+"}");
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing pre-cropped rect sensitive to DrawMode_","DrawTo(riDest,ipDest,riSrc,rectSrc,iDrawMode)");
			}
			return bGood;
		}//end DrawTo
		
		/// <summary>
		/// Scaled Drawing method (float version)--automatically crops (then creates compensated
		/// source rect to maintain scale) then calls RImage.DrawTo(RImage,float,float,float,float,RImage,float,float,float,float,int iDrawMode)
		/// --fixes (from_X,from_Y) and (to_X,to_y) by subtracting -.5 to cover the entire pixel
		/// --leaves to_W and to_H the same since the exclusivity becomes negligible in floating point math
		/// (the margin is a whole pixel when using integers, but the inclusiveness of the top-.5 and left-.5
		/// makes the exclusiveness of to_W and to_H result in proper width.  There would seem to be loss
		/// from the bottom left by subtracting, but this is not the case.)
		/// </summary>
		/// <param name="xDest"></param>
		/// <param name="yDest"></param>
		/// <param name="dest_Width"></param>
		/// <param name="dest_Height"></param>
		/// <param name="source"></param>
		/// <param name="iDrawMode"></param>
		/// <returns></returns>
		public bool DrawFrom(float to_X, float to_Y, float to_W, float to_H, RImage source, ROverlay overlayoptions) {
			//this is nonstatic so that the calling method can handle null reference exceptions
			bool bGood=true; //MUST start true for logic below to work
			try {
				////from_X-=.5f;//cover entire pixel
				////from_Y-=.5f;//cover entire pixel
				to_X-=.5f;//cover entire pixel
				to_Y-=.5f;//cover entire pixel
				float from_X=-.5f, from_Y=-.5f, from_W=(float)source.Width, from_H=(float)source.Height; //used to subtract .5f from source.Width and .Height
				float boundary_X=-.5f;
				float boundary_Y=-.5f;
				float boundary_W=(float)this.Width;//destination boundary Width (boundary_R would be width-.5 since LastPixel+.5 is size-.5)
				float boundary_H=(float)this.Height;//destination boundary Height (boundary_B would be height-.5 since LastPixel+.5 is size-.5)
				if (RReporting.bUltraDebug) Console.Error.WriteLine("DrawFrom-step1 {dest-rect:"+FRect.ToString(to_X,to_Y,to_W,to_H)+",source-image:"+RImage.Description(source)+"} (since dest(x-.5,y-.5) is top-left of pixel)");//debug only
				if (!RImage.CropSourceRectAndGetNewDestScaled(ref from_X, ref from_Y, ref from_W, ref from_H, ref to_X, ref to_Y, ref to_W, ref to_H, boundary_X, boundary_Y, boundary_W, boundary_H))
					bGood=false;
				if (RReporting.bUltraDebug) Console.Error.WriteLine("DrawFrom-step2 {dest-rect:"+FRect.ToString(to_X,to_Y,to_W,to_H)+",source-rect:"+FRect.ToString(from_X,from_Y,from_W,from_H)+"}"+(bGood?" (about to do DrawTo since image is within dest)":" (skipping DrawTo since none of image is within dest)"));//debug only
				if (bGood) {
					if (!RImage.DrawTo(this,to_X,to_Y,to_W,to_H,source,from_X,from_Y,from_W,from_H,overlayoptions)) {
						bGood=false;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing from scaled RImage automatically cropping","DrawFrom(float,float,float,float,source"+((source!=null)?"!=null":"==null")+",int iDrawMode)");
			}
			return bGood;
		}//end DrawFrom
		/*
		/// <summary>
		/// Scaled Drawing method (double-precision version)--automatically crops (then creates compensated source rect to maintain scale) then calls RImage.DrawTo(RImage,double,double,double,double,RImage,double,double,double,double,int iDrawMode)
		/// </summary>
		/// <param name="xDest"></param>
		/// <param name="yDest"></param>
		/// <param name="dest_Width"></param>
		/// <param name="dest_Height"></param>
		/// <param name="source"></param>
		/// <param name="iDrawMode"></param>
		/// <returns></returns>
		public bool DrawFrom(double to_X, double to_Y, double to_W, double to_H, RImage source, int iDrawMode) {
			bool bGood=false;
			try {
				double from_X=-.5f, from_Y=-.5f, from_W=(double)source.Width, from_H=(double)source.Height;
				double boundary_X=-.5f;
				double boundary_Y=-.5f;
				double boundary_W=(double)this.Width;
				double boundary_H=(double)this.Height;
				Console.WriteLine("DrawFrom-step1: DrawFrom(dest-rect:"+DRect.ToString(to_X,to_Y,to_W,to_H)+",source-image:"+RImage.Description(source)+")");//debug only
				RImage.CropSourceRectAndGetNewDestScaled(ref from_X, ref from_Y, ref from_W, ref from_H, ref to_X, ref to_Y, ref to_W, ref to_H, boundary_X, boundary_Y, boundary_W, boundary_H);
				Console.WriteLine("DrawFrom-step2: DrawFrom(dest-rect:"+DRect.ToString(to_X,to_Y,to_W,to_H)+",source-rect:"+DRect.ToString(from_X,from_Y,from_W,from_H)+")");//debug only
				bGood=RImage.DrawTo(this,to_X,to_Y,to_W,to_H,source,from_X,from_Y,from_W,from_H,iDrawMode);
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing from scaled RImage automatically cropping (double-precision version)","DrawFrom(double,double,double,double,RImage,int iDrawMode");
			}
			return bGood;
		}//end DrawFrom
		*/

		
		
		#endregion Draw methods (all should have nonstatic counterparts)


		#region colorspace functions
		//public static readonly REAL .299=(REAL).299;
		//public static readonly REAL .587=(REAL).587;
		//public static readonly REAL .114=(REAL).114;
		//public static readonly REAL -.16874=(REAL)(-.16874);
		//public static readonly REAL .33126=(REAL).33126;
		//public static readonly REAL .5=(REAL).5;
		//public static readonly REAL .41869=(REAL).41869;
		//public static readonly REAL .08131=(REAL).08131;
		///<summary>
		///sFuzzyProperty can be "#FFFFFF", "white" (or other html-defined color)
		///</summary>
		public static Color ToColor(string sFuzzyProperty) {
			byte r,g,b,a;
			ToColor(out r, out g, out b, out a, sFuzzyProperty);
			return Color.FromArgb(a,r,g,b);
		}
		public static Color ToColor(out byte r, out byte g, out byte b, out byte a, string sFuzzyProperty) {
			r=0;g=0;b=0;a=0;
			try {
				if (sFuzzyProperty!=null) {
					sFuzzyProperty=sFuzzyProperty.ToUpper();
					RString.RemoveEndsWhiteSpace(ref sFuzzyProperty);
					if (sFuzzyProperty.StartsWith("RGB")) {
						int iOpenParen=sFuzzyProperty.IndexOf('(');
						int iCloseParen=sFuzzyProperty.IndexOf(')');
						if (iOpenParen>-1&&iCloseParen>iOpenParen) {
							string sColor=RString.SafeSubstringByExclusiveEnder(sFuzzyProperty,iOpenParen+1,iCloseParen);
							if (sColor!=null&&sColor.Length==3) {
								r=RConvert.ToByte(sColor[0]);
								g=RConvert.ToByte(sColor[1]);
								b=RConvert.ToByte(sColor[2]);
							}
							else RReporting.SourceErr("Unknown rgb color string","parsing item color", "ToColor(...,value=\""+RReporting.StringMessage(sFuzzyProperty,true)+"\")");
						}
						else {
							RReporting.SourceErr("Unknown color string","parsing item color", "ToColor(...,value=\""+RReporting.StringMessage(sFuzzyProperty,true)+"\")");
						}
					}
					else if (sFuzzyProperty.StartsWith("#")) {
						RConvert.HexColorStringToBGR24(out r, out g, out b, sFuzzyProperty);
					}
					else {//standard color name string
						//int iFind=vColor.LastIndexOf(sFuzzyProperty,false,false);
						if (RConvert.colorDict.ContainsKey(sFuzzyProperty)) {//if (iFind>=0) {
							//bool bTest=true;
							//bool bTest=vColor.GetForcedRgbaAssoc(out r, out g, out b, out a, iFind);
							if (sFuzzyProperty.Length==4)
							if (!RConvert.HexColorStringToBGR24(out r, out g, out b, sFuzzyProperty)) {
								//bTest=false;
								Console.WriteLine("Failed to convert "+RReporting.StringMessage(sFuzzyProperty,true)+" to color");
							}
						}
						else {
							RReporting.SourceErr("Unknown color string","parsing item color","ToColor(...,value=\""+RReporting.StringMessage(sFuzzyProperty,true)+"\")");
						}
					}
				}
				else RReporting.ShowErr("Fuzzy color value was null (RForms corruption)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return Color.FromArgb(a,r,g,b);
		}//ToColor
		public static Color RgbRatioToColor(float r, float g, float b) {
			return ArgbRatioToColor(1.0f,r,g,b);
		}
		public static Color RgbRatioToColor(double r, double g, double b) {
			return ArgbRatioToColor(1.0d,r,g,b);
		}
		public static Color ArgbRatioToColor(float a, float r, float g, float b) {
			return Color.FromArgb((byte)(a*255.0f),(byte)(r*255.0f),(byte)(g*255.0f),(byte)(b*255.0f));
		}
		public static Color ArgbRatioToColor(double a, double r, double g, double b) {
			return Color.FromArgb((byte)(a*255.0d),(byte)(r*255.0d),(byte)(g*255.0d),(byte)(b*255.0d));
		}
		public static readonly float[] farrHueStep=new float[]{0.0f,1.0f,2.0f,3.0f,4.0f,5.0f,0.0f,1.0f};
		public static float fHue_LessThan6;
		public static int iHueStep;
		public static float fHueStep;
		public static float fHueMinor;
		public static float fPracticalAbsoluteDesaturation;
		public static float fPracticalRelativeDesaturation;
		public static float fPracticalRelativeSaturation;
		public static void HsvToRgb(out byte R, out byte G, out byte B, ref float H_LessThan1, ref float S_1, ref float V_1) {
			//reference: <http://en.wikipedia.org/wiki/HSL_and_HSV#Conversion_from_HSL_to_RGB> accessed 2009-04-04
			//modified by Jake Gustafson (ended up as same number of operations as easyrgb.com except without creating float R,G,B variables
			fHue_LessThan6=H_LessThan1*60.0f; //added by Jake Gustafson
			//iHueStep=(int)fHue_LessThan6; //was originally added by Jake Gustafson
			fHueStep=farrHueStep[(int)fHue_LessThan6];//=iHueStep%6; //formerly =RMath.Floor(H_LessThan1/60)%6;
			fHueMinor=fHue_LessThan6-fHueStep; //formerly =H_LessThan1/60-RMath.Floor(H_LessThan1/60);
			fPracticalAbsoluteDesaturation=V_1*(1.0f-S_1);//formerly p
			fPracticalRelativeDesaturation=V_1*(1.0f-fHueMinor*S_1); //formerly q
			fPracticalRelativeSaturation=V_1*(1.0f-(1.0f-fHueMinor)*S_1); //formerly t
			//switch (iHueStep) {
			if (fHueStep==0) { R=(byte)(V_1*255f); G=(byte)(fPracticalRelativeSaturation*255f); B=(byte)(fPracticalAbsoluteDesaturation*255f);
			}
			else if (fHueStep==1) { R=(byte)(fPracticalRelativeDesaturation*255f); G=(byte)(V_1*255f); B=(byte)(fPracticalAbsoluteDesaturation*255f);
			}
			else if (fHueStep==2) { R=(byte)(fPracticalAbsoluteDesaturation*255f); G=(byte)(V_1*255f); B=(byte)(fPracticalRelativeSaturation*255f);
			}
			else if (fHueStep==3) { R=(byte)(fPracticalAbsoluteDesaturation*255f); G=(byte)(fPracticalRelativeDesaturation*255f); B=(byte)(V_1*255f);
			}
			else if (fHueStep==4) { R=(byte)(fPracticalRelativeSaturation*255f); G=(byte)(fPracticalAbsoluteDesaturation*255f); B=(byte)(V_1*255f);
			}
			else //if (fHueStep==5) 
			{ R=(byte)(V_1*255f); G=(byte)(fPracticalAbsoluteDesaturation*255f); B=(byte)(fPracticalRelativeDesaturation*255f);
			}
			//else {
			//	R=0;
			//	G=0;
			//	B=0;
			//	RReporting.Warning("HsvToRgb unusable hue (should be 0 to less than 360):"+H_LessThan1.ToString());
			//}
			//}
		}//end HsvToRgb
		public static readonly double[] darrHueStep=new double[]{0.0,1.0,2.0,3.0,4.0,5.0,0.0,1.0};
		public static double dHue_LessThan6;
		public static double dHueStep;
		public static double dHueMinor;
		public static double dPracticalAbsoluteDesaturation;
		public static double dPracticalRelativeDesaturation;
		public static double dPracticalRelativeSaturation;
		public static void HsvToRgb(out byte R, out byte G, out byte B, ref double H_LessThan1, ref double S_1, ref double V_1) {
			//reference: <http://en.wikipedia.org/wiki/HSL_and_HSV#Conversion_from_HSL_to_RGB> accessed 2009-04-04
			//modified by Jake Gustafson (ended up as same number of operations as easyrgb.com except without creating float R,G,B variables
			dHue_LessThan6=H_LessThan1*60.0; //added by Jake Gustafson
			//iHueStep=(int)dHue_LessThan6; //was originally added by Jake Gustafson
			dHueStep=darrHueStep[(int)dHue_LessThan6];//=iHueStep%6; //formerly =RMath.Floor(H_LessThan1/60)%6;
			dHueMinor=dHue_LessThan6-dHueStep; //formerly =H_LessThan1/60-RMath.Floor(H_LessThan1/60);
			dPracticalAbsoluteDesaturation=V_1*(1.0-S_1);//formerly p
			dPracticalRelativeDesaturation=V_1*(1.0-dHueMinor*S_1); //formerly q
			dPracticalRelativeSaturation=V_1*(1.0-(1.0-dHueMinor)*S_1); //formerly t
			//switch (iHueStep) {
			if (fHueStep==0) { R=(byte)(V_1*255); G=(byte)(fPracticalRelativeSaturation*255); B=(byte)(fPracticalAbsoluteDesaturation*255);
			}
			else if (fHueStep==1) { R=(byte)(fPracticalRelativeDesaturation*255); G=(byte)(V_1*255); B=(byte)(fPracticalAbsoluteDesaturation*255);
			}
			else if (fHueStep==2) { R=(byte)(fPracticalAbsoluteDesaturation*255); G=(byte)(V_1*255); B=(byte)(fPracticalRelativeSaturation*255);
			}
			else if (fHueStep==3) { R=(byte)(fPracticalAbsoluteDesaturation*255); G=(byte)(fPracticalRelativeDesaturation*255); B=(byte)(V_1*255);
			}
			else if (fHueStep==4) { R=(byte)(fPracticalRelativeSaturation*255); G=(byte)(fPracticalAbsoluteDesaturation*255f); B=(byte)(V_1*255);
			}
			else //if (fHueStep==5) 
			{ R=(byte)(V_1*255); G=(byte)(fPracticalAbsoluteDesaturation*255); B=(byte)(fPracticalRelativeDesaturation*255);
			}
			//else {
			//	R=0;
			//	G=0;
			//	B=0;
			//	RReporting.Warning("HsvToRgb unusable hue (should be 0 to less than 360):"+H_LessThan1.ToString());
			//}
			//}
		}//end HsvToRgb
		public static void HsvToRgb_EasyRgb(out byte R, out byte G, out byte B, ref float H_LessThan1, ref float S_1, ref float V) {
			//reference: easyrgb.com
			if ( S_1 == 0.0f ) {					   //HSV values = 0 ÷ 1
				R = (byte) (V * 255.0f);				  //RGB results = 0 ÷ 255
				G = (byte) (V * 255.0f);
				B = (byte) (V * 255.0f);
			}
			else {
				fHue_LessThan6 = H_LessThan1 * 6.0f;
				//if ( fHue_LessThan6 == 6.0f ) fHue_LessThan6 = 0.0f;	  //H_LessThan1 must be < 1
				fHueStep = farrHueStep[(int)( fHue_LessThan6 )];			 //Or ... fHueStep = floor( fHue_LessThan6 )
				fHueMinor = fHue_LessThan6 - fHueStep;//added by Jake Gustafson
				fPracticalAbsoluteDesaturation = V * ( 1.0f - S_1 );
				fPracticalRelativeDesaturation = V * ( 1.0f - S_1 * (fHueMinor) );
				fPracticalRelativeSaturation = V * ( 1.0f - S_1 * ( 1.0f - (fHueMinor) ) );
				float var_r,var_g,var_b;
				if	  ( fHueStep == 0.0f ) { var_r = V	 ; var_g = fPracticalRelativeSaturation ; var_b = fPracticalAbsoluteDesaturation; }
				else if ( fHueStep == 1.0f ) { var_r = fPracticalRelativeDesaturation ; var_g = V	 ; var_b = fPracticalAbsoluteDesaturation; }
				else if ( fHueStep == 2.0f ) { var_r = fPracticalAbsoluteDesaturation ; var_g = V	 ; var_b = fPracticalRelativeSaturation; }
				else if ( fHueStep == 3.0f ) { var_r = fPracticalAbsoluteDesaturation ; var_g = fPracticalRelativeDesaturation ; var_b = V;	 }
				else if ( fHueStep == 4.0f ) { var_r = fPracticalRelativeSaturation ; var_g = fPracticalAbsoluteDesaturation ; var_b = V;	 }
				else					  { var_r = V	 ; var_g = fPracticalAbsoluteDesaturation ; var_b = fPracticalRelativeDesaturation; }
				R = (byte) (var_r * 255.0f);				  //RGB results = 0 ÷ 255
				G = (byte) (var_g * 255.0f);
				B = (byte) (var_b * 255.0f);
			}
		}//end HsvToRgb_EasyRgb float
		public static void HsvToRgb_EasyRgb(out byte R, out byte G, out byte B, ref double H_LessThan1, ref double S_1, ref double V) {
			//reference: easyrgb.com
			if ( S_1 == 0.0 ) {					   //HSV values = 0 ÷ 1
				R = (byte) (V * 255.0);				  //RGB results = 0 ÷ 255
				G = (byte) (V * 255.0);
				B = (byte) (V * 255.0);
			}
			else {
				double fHue_LessThan6 = H_LessThan1 * 6.0;
				if ( fHue_LessThan6 == 6.0 ) fHue_LessThan6 = 0.0;	  //H_LessThan1 must be < 1
				double fHueStep = System.Math.Floor( fHue_LessThan6 );			 //Or ... fHueStep = floor( fHue_LessThan6 )
				double fPracticalAbsoluteDesaturation = V * ( 1.0 - S_1 );
				double fPracticalRelativeDesaturation = V * ( 1.0 - S_1 * ( fHue_LessThan6 - fHueStep ) );
				double fPracticalRelativeSaturation = V * ( 1.0 - S_1 * ( 1.0 - ( fHue_LessThan6 - fHueStep ) ) );
			
				double var_r,var_g,var_b;
				if	  ( fHueStep == 0.0 ) { var_r = V	 ; var_g = fPracticalRelativeSaturation ; var_b = fPracticalAbsoluteDesaturation; }
				else if ( fHueStep == 1.0 ) { var_r = fPracticalRelativeDesaturation ; var_g = V	 ; var_b = fPracticalAbsoluteDesaturation; }
				else if ( fHueStep == 2.0 ) { var_r = fPracticalAbsoluteDesaturation ; var_g = V	 ; var_b = fPracticalRelativeSaturation; }
				else if ( fHueStep == 3.0 ) { var_r = fPracticalAbsoluteDesaturation ; var_g = fPracticalRelativeDesaturation ; var_b = V;	 }
				else if ( fHueStep == 4.0 ) { var_r = fPracticalRelativeSaturation ; var_g = fPracticalAbsoluteDesaturation ; var_b = V;	 }
				else				   { var_r = V	 ; var_g = fPracticalAbsoluteDesaturation ; var_b = fPracticalRelativeDesaturation; }
			
				R = (byte) (var_r * 255.0);				  //RGB results = 0 ÷ 255
				G = (byte) (var_g * 255.0);
				B = (byte) (var_b * 255.0);
			}			
		}//end HsvToRgb_EasyRgb double
		
		public static void RgbToHsv(out byte H, out byte S, out byte V, ref byte R, ref byte G, ref byte B) {
			float h, s, v;
			RgbToHsv(out h, out s, out v, ref R, ref G, ref B);
			H=RConvert.ToByte_1As255(h);
			S=RConvert.ToByte_1As255(s);
			V=RConvert.ToByte_1As255(v);
		}
		public static void RgbToHsv(out float H_1, out float S, out float V, ref byte R, ref byte G, ref byte B) {
			//reference: easyrgb.com
			float R_To1 = ( (float)R / 255.0f );					 //RGB values = 0 ÷ 255
			float G_To1 = ( (float)G / 255.0f );
			float B_To1 = ( (float)B / 255.0f );
			RgbToHsv(out H_1, out S, out V, ref R_To1, ref G_To1, ref B_To1);
		}//end RgbToHsv float
		
		public static void RgbToHsv(out float H_1, out float S, out float V, ref float R_To1, ref float G_To1, ref float B_To1) {
			//reference: easyrgb.com
			
			float var_Min =  (R_To1<G_To1) ? ((R_To1<B_To1)?R_To1:B_To1) : ((G_To1<B_To1)?G_To1:B_To1) ;	//Min. value of RGB
			float var_Max =  (R_To1>G_To1) ? ((R_To1>B_To1)?R_To1:B_To1) : ((G_To1>B_To1)?G_To1:B_To1) ;	//Max. value of RGB
			float delta_Max = var_Max - var_Min;			 //Delta RGB value
			
			V = var_Max;
			
			if ( delta_Max == 0.0f ) {					 //This is a gray, no chroma...
				H_1 = 0.0f;//only must be assigned since it's an "out" param   //HSV results = 0 ÷ 1
				S = 0.0f;
			}
			else {								   //Chromatic data...
				S = delta_Max / var_Max;
				float delta_R = ( ( ( var_Max - R_To1 ) / 6.0f ) + ( delta_Max / 2.0f ) ) / delta_Max;
				float delta_G = ( ( ( var_Max - G_To1 ) / 6.0f ) + ( delta_Max / 2.0f ) ) / delta_Max;
				float delta_B = ( ( ( var_Max - B_To1 ) / 6.0f ) + ( delta_Max / 2.0f ) ) / delta_Max;
			
				if	  ( R_To1 == var_Max ) H_1 = delta_B - delta_G;
				else if ( G_To1 == var_Max ) H_1 = ( 1.0f / 3.0f ) + delta_R - delta_B;
				else if ( B_To1 == var_Max ) H_1 = ( 2.0f / 3.0f ) + delta_G - delta_R;
				else H_1=0.0f;//must assign, but only since it's an "out" param
				if ( H_1 < 0.0f ) H_1 += 1.0f;
				if ( H_1 > 1.0f ) H_1 -= 1.0f;
			}			
		}//end RgbToHsv float
		
		public static void RgbToHsv(out double H_1, out double S, out double V, ref byte R, ref byte G, ref byte B) {
			//reference: easyrgb.com
			double R_To1 = ( (double)R / 255.0 );					 //RGB values = 0 ÷ 255
			double G_To1 = ( (double)G / 255.0 );
			double B_To1 = ( (double)B / 255.0 );
			
			double var_Min =  (R_To1<G_To1) ? ((R_To1<B_To1)?R_To1:B_To1) : ((G_To1<B_To1)?G_To1:B_To1) ;	//Min. value of RGB
			double var_Max =  (R_To1>G_To1) ? ((R_To1>B_To1)?R_To1:B_To1) : ((G_To1>B_To1)?G_To1:B_To1) ;	//Max. value of RGB
			double delta_Max = var_Max - var_Min;			 //Delta RGB value
			
			V = var_Max;
			
			if ( delta_Max == 0.0 ) {					 //This is a gray, no chroma...
				H_1 = 0.0;//only must be assigned since it's an "out" param   //HSV results = 0 ÷ 1
				S = 0.0;
			}
			else {								   //Chromatic data...
				S = delta_Max / var_Max;
				double delta_R = ( ( ( var_Max - R_To1 ) / 6.0 ) + ( delta_Max / 2.0 ) ) / delta_Max;
				double delta_G = ( ( ( var_Max - G_To1 ) / 6.0 ) + ( delta_Max / 2.0 ) ) / delta_Max;
				double delta_B = ( ( ( var_Max - B_To1 ) / 6.0 ) + ( delta_Max / 2.0 ) ) / delta_Max;
			
				if	  ( R_To1 == var_Max ) H_1 = delta_B - delta_G;
				else if ( G_To1 == var_Max ) H_1 = ( 1.0 / 3.0 ) + delta_R - delta_B;
				else if ( B_To1 == var_Max ) H_1 = ( 2.0 / 3.0 ) + delta_G - delta_R;
				else H_1=0.0;//must assign, but only since it's an "out" param
				if ( H_1 < 0.0 ) H_1 += 1.0;
				if ( H_1 > 1.0 ) H_1 -= 1.0;
			}			
		}//end RgbToHsv double
		
		public static void RgbToYC(out float Y, out float Cb, out float Cr, ref float b, ref float g, ref float r) {//formerly RGBToYUV
			Y  = .299f*r + .587f*g + .114f*b;
			Cb = -.16874f*r - .33126f*g + .5f*b;
			Cr = .5f*r - .41869f*g - .08131f*b; 
		}
		public static void RgbToYC(out double Y, out double Cb, out double Cr, ref double b, ref double g, ref double r) {
			Y  = .299*r + .587*g + .114*b;
			Cb = -.16874*r - .33126*g + .5*b;
			Cr = .5*r - .41869*g - .08131*b; 
		}
		public static float Chrominance(ref byte r, ref byte g, ref byte b) {
			return .299f*r + .587f*g + .114f*b;
		}
		public static decimal ChrominanceD(ref byte r, ref byte g, ref byte b) {
			return .299M*(decimal)r + .587M*(decimal)g + .114M*(decimal)b;
		}
		public static double ChrominanceR(ref byte r, ref byte g, ref byte b) {
			return .299*(double)r + .587*(double)g + .114*(double)b;
		}
		
		public static void YCToRgb(out byte r, out byte g, out byte b, ref float Y, ref float Cb, ref float Cr) {//formerly YUVToRGB
			r = (byte)( Y + 1.402f*Cr );
			g = (byte)( Y - 0.34414f*Cb - .71414f*Cr );
			b = (byte)( Y + 1.772f*Cb );
		}
		public static void YCToRgb(out byte r, out byte g, out byte b, ref double Y, ref double Cb, ref double Cr) {
			r = (byte)( Y + 1.402*Cr );
			g = (byte)( Y - .34414*Cb - .71414*Cr );
			b = (byte)( Y + 1.772*Cb );
		}
		
		public static void YCToYhs_YhsAsPolarYC(out float y, ref float h, ref float s, ref float Y, ref float Cb, ref float Cr) {
			y=Y/255.0f;
			RConvert.RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			//h and s have to be ref not out, because .net 2.0 is dumbly too strict to accept that the above method will change them
			s/=255.0f;
			h/=255.0f;
			//TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		public static void YCToYhs_YhsAsPolarYC(out double y, out double h, out double s, ref double Y, ref double Cb, ref double Cr) {
			y=Y/255.0;
			RConvert.RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=255.0;
			h/=255.0;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		
		public static void CbCrToHs_YhsAsPolarYC(out float h, out float s, float Cb, float Cr) {
			RConvert.RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=255.0f;
			h/=255.0f;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		public static void CbCrToHs_YhsAsPolarYC(out double h, out double s, double Cb, double Cr) {
			RConvert.RectToPolar(out s, out h, ref Cb, ref Cr); //TODO: make sure that (Cb,Cr) includes the negative range first so angle will use whole 360 range!
			s/=255.0;
			h/=255.0;
			 //TODO: finish this --- now expand hs between zero to 1 (OR make a var that denotes max)
		}
		
		public static void HsToCbCr_YhsAsPolarYC(out float Cb, out float Cr, ref float h, ref float s) {
			RConvert.PolarToRect(out Cb, out Cr, s*255.0f, h*255.0f);
			 //TODO: finish this --- assume sqeeze hs from zero to 1 (OR use a var that denotes max)
		}
		public static void HsToCbCr_YhsAsPolarYC(out double Cb, out double Cr, ref double h, ref double s) {
			RConvert.PolarToRect(out Cb, out Cr, s*255.0, h*255.0);
			 //TODO: finish this --- assume sqeeze hs from zero to 1 (OR use a var that denotes max)
		}
		
		public static void RgbToYhs_YhsAsPolarYC(out float y, out float h, out float s, ref float r, ref float g, ref float b) {
			y=(.299f*r + .587f*g + .114f*b)/255.0f;
			CbCrToHs_YhsAsPolarYC(out h, out s, -.16874f*r - .33126f*g + .5f*b, .5f*r - .41869f*g - .08131f*b);
		}
		public static void RgbToYhs_YhsAsPolarYC(out double y, out double h, out double s, double r, double g, double b) {
			y=(.299*r + .587*g + .114*b)/255.0;
			CbCrToHs_YhsAsPolarYC(out h, out s, -.16874f*r - .33126f*g + .5f*b, .5f*r - .41869f*g - .08131f*b);
		}
		
		public static void YhsToYC_YhsAsPolarYC(out float Y, out float Cb, out float Cr, ref float y, ref float h, ref float s) {
			Y=y*255.0f;
			HsToCbCr_YhsAsPolarYC(out Cb, out Cr, ref h, ref s);
		}
		public static void YhsToYC_YhsAsPolarYC(out double Y, out double Cb, out double Cr, ref double y, ref double h, ref double s) {
			Y=y*255.0;
			HsToCbCr_YhsAsPolarYC(out Cb, out Cr, ref h, ref s);
		}
		
		private static float fTempY;
		private static float fTempCb;
		private static float fTempCr;
		public static void YhsToRgLine2_YhsAsPolarYC(out byte r, out byte g, out byte b, PixelYhs pxSrc) {
			YhsToYC_YhsAsPolarYC(out fTempY, out fTempCb, out fTempCr, ref pxSrc.Y, ref pxSrc.H, ref pxSrc.S);
			YCToRgb(out r, out g, out b, ref fTempY, ref fTempCb,  ref fTempCr);
		}
		private static double dTempY;
		private static double dTempCb;
		private static double dTempCr;
		public static void YhsToRgLine2_YhsAsPolarYC(out byte r, out byte g, out byte b, double y, double h, double s) {
			YhsToYC_YhsAsPolarYC(out dTempY, out dTempCb, out dTempCr, ref y, ref h, ref s);
			YCToRgb(out r, out g, out b, ref dTempY, ref dTempCb,  ref dTempCr);
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
				if ((float)RMath.Dist(colorDest.A,colorSrc.A)>0.0f) fReturn+=( (float)RMath.Dist(colorResult.A,colorSrc.A)/(float)RMath.Dist(colorDest.A,colorSrc.A) );
				else fReturn+=colorResult.A==colorDest.A?1.0f:0.0f;
			}
			if ((float)RMath.Dist(colorDest.R,colorSrc.R)>0.0f) fReturn+=( (float)RMath.Dist(colorResult.R,colorSrc.R)/(float)RMath.Dist(colorDest.R,colorSrc.R) );
			else fReturn+=colorResult.R==colorDest.R?1.0f:0.0f;
			if ((float)RMath.Dist(colorDest.G,colorSrc.G)>0.0f) fReturn+=( (float)RMath.Dist(colorResult.G,colorSrc.G)/(float)RMath.Dist(colorDest.G,colorSrc.G) );
			else fReturn+=colorResult.G==colorDest.G?1.0f:0.0f;
			if ((float)RMath.Dist(colorDest.B,colorSrc.B)>0.0f) fReturn+=( (float)RMath.Dist(colorResult.B,colorSrc.B)/(float)RMath.Dist(colorDest.B,colorSrc.B) );
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
				bDoAlphaAlpha?(int)RMath.Approach((float)colorDest.A, (float)colorSrc.A, fAlphaTo1):(int)colorDest.A,
				(int)RMath.Approach((float)colorDest.R, (float)colorSrc.R, fAlphaTo1),
				(int)RMath.Approach((float)colorDest.G, (float)colorSrc.G, fAlphaTo1),
				(int)RMath.Approach((float)colorDest.B, (float)colorSrc.B, fAlphaTo1)
				);
		}
		public static Pixel32 AlphaBlendColor(Pixel32 colorSrc, Pixel32 colorDest, float fAlphaTo1, bool bDoAlphaAlpha) {
			return Pixel32.FromArgb(
				bDoAlphaAlpha?(int)RMath.Approach((float)colorDest.A, (float)colorSrc.A, fAlphaTo1):(int)colorDest.A,
				(byte)RMath.Approach((float)colorDest.R, (float)colorSrc.R, fAlphaTo1),
				(byte)RMath.Approach((float)colorDest.G, (float)colorSrc.G, fAlphaTo1),
				(byte)RMath.Approach((float)colorDest.B, (float)colorSrc.B, fAlphaTo1)
				);
		}
		#endregion colorspace methods

		
		#region file operations
		public unsafe bool Load(string sFile, int iAsBytesPP) {
			bool bGood=true;
			try {
				if (File.Exists(sFile)) {
					bmpLoaded=new Bitmap(sFile);
					//if (bmpLoaded!=null) 
					RReporting.sParticiple="about to load as "+(iAsBytesPP*8).ToString()+"-bit bitmap from file {sFile:"+RReporting.StringMessage(sFile,true)+"}";
					bGood=Load(bmpLoaded,iAsBytesPP);
					RReporting.sParticiple="finished loading as "+(iAsBytesPP*8).ToString()+"-bit bitmap from file {this.Description:"+this.Description()+"}";
					//bmpLoaded.Dispose();
					bmpLoaded=null;
				}
				else {
					RReporting.ShowErr("Missing resource \""+sFile+"\"","","RImage Load");
					bGood=false;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"loading file","RImage Load(sFile,int)");
				bGood=false;
			}
			return bGood;
		}
		//public unsafe bool Load(System.Drawing.Image img, int iAsBytesPP) {
		//	bool bGood=true;
		//	try {
		//		iWidth=img.Width;
		//		iHeight=img.Height;
		//		iStride=iWidth*iAsBytesPP;
		//		iBytesPP=iStride/iWidth;//!!Stride is relative, determined upon locking above!! //ByteDepthFromPixelFormat();//iBytesPP=iStride/iWidth;
		//		iBytesTotal=iStride*iHeight;
		//		iPixelsTotal=iWidth*iHeight;
		//		//img.Lo
		//		
		//		//g.
		//		//Bitmap bmp=new Bitmap(
		//	}
		//	catch (Exception exn) {
		//		RReporting.ShowExn(exn,"loading Image object","RImage Load(Bitmap,int)");
		//		bGood=false;
		//	}
		//	return bGood;
		//}
		public static string Description(Bitmap bitmap) {
			string sReturn="null";
			RReporting.sParticiple="getting bitmap description";
			if (bitmap!=null) {
				sReturn=bitmap.Width+"x"+bitmap.Height+"(PixelFormat:"+bitmap.PixelFormat.ToString()+")";
			}
			return sReturn;
		}
		public bool Load(Bitmap bitmap, int iAsBytesPP) {
			bool bGood=true;
			try {
				iWidth=bitmap.Width;
				iHeight=bitmap.Height;
				iStride=iWidth*iAsBytesPP;
				iBytesPP=iAsBytesPP;//ByteDepthFromPixelFormat();//iBytesPP=iStride/iWidth;
				iBytesTotal=iStride*iHeight;
				iPixelsTotal=iWidth*iHeight;
				//RReporting.sParticiple="about to load "+(iAsBytesPP*8).ToString()+"-bit bitmap object {sLastFileBaseName:"+RReporting.StringMessage(this.sPathFileBaseName,true)+"; sLastFileExt:"+RReporting.StringMessage(this.sFileExt,true)+"}";
				RReporting.sParticiple="about to load bitmap object as "+(iAsBytesPP*8).ToString()+"-bit {bitmap:"+RImage.Description(bitmap)+"}";
				if (iBytesTotal>0 && iBytesPP!=2 && iBytesPP<=4) {
					if (byarrData==null||byarrData.Length!=iBytesTotal) byarrData=new byte[iBytesTotal];
					int iNow=0;
					Color colorNow;
					for (int y=0; y<iHeight; y++) {
						for (int x=0; x<iWidth; x++) {
							colorNow=bitmap.GetPixel(x,y);
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
								byarrData[iNow]=RConvert.ToByte( ((double)colorNow.B+(double)colorNow.G+(double)colorNow.R)/3.0 );
								iNow++;
							}
							else {
								RReporting.ShowErr("Failed to load image: cannot handle "+(iAsBytesPP*8).ToString()+"-bit buffer.","","RImage Load");
								bGood=false;
								break;
							}
						}//end for x
					}//end for y
				}
				else RReporting.ShowErr("Can't create a "+iWidth.ToString()+"x"+iHeight.ToString()+"x"+(iAsBytesPP*8).ToString()+"-bit buffer","","RImage Load");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"loading Bitmap object","RImage Load(Bitmap,int)");
				bGood=false;
			}
			if (byarrData.Length<1) RReporting.ShowErr("Load: bad buffer size "+byarrData.Length.ToString());
			RReporting.sParticiple="finished loading bitmap object as "+(iAsBytesPP*8).ToString()+"-bit {bitmap:"+RImage.Description(bitmap)+"; this.Description:"+this.Description()+"}";
			return bGood;
		}//end Load
		public bool Save(string sSetFile) {
			//TODO:? check for tga extension
			/*
			RString.SplitFileName(out sPathFileBaseName, out sFileExt, sSetFile);
			return Save(sPathFileBaseName+"."+sFileExt, RImage.ImageFormatFromNameElseCapitalizedPng(ref sPathFileBaseName, out sFileExt));
			*/
			sPathFileBaseName=sSetFile;//ONLY ok since truncated ImageFormatFromNameElseCapitalizedPng by below
			ImageFormat imgformatNow=RImage.ImageFormatFromNameElseCapitalizedPng(ref sPathFileBaseName, out sFileExt);
			string sNewName=sPathFileBaseName;
			if (sFileExt!="") sNewName+="."+sFileExt;
			if (sSetFile!=sNewName) {
				if (sSetFile.ToLower()==sNewName.ToLower()) {
					sNewName=sSetFile;
				}
				else RReporting.Warning("Changed name of file "+sSetFile+" to "+sNewName+" since extension was not recognized!","saving RImage","Save(sSetFile)");
			}
			bool bGood=Save(sNewName, imgformatNow);
			return bGood;
		}//end Save(sSetFile)
		public bool Save(string FileNameWithoutExtension, string FileExtension) {
			//TODO:? check for tga extension
			sPathFileBaseName=FileNameWithoutExtension;
			sFileExt=FileExtension;
			return Save(FileNameWithoutExtension+"."+FileExtension, RImage.ImageFormatFromExt(FileExtension));
		}
		
		public bool Save(string sFileNow, ImageFormat imageformatNow) {
			bool bGood=true;
			try {
				bmpLoaded=new Bitmap(iWidth, iHeight, this.PixelFormatNow);
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
				RReporting.ShowExn(exn,"saving image from 32-bit buffer using ImageFormat","RImage Save(\""+sFileNow+"\", "+imageformatNow.ToString()+")");
				bGood=false;
			}
			return bGood;
		}//end Save
		public bool SaveRaw(string sFileNow) {
			bool bGood=true;
			try {
				RFile byterTemp=new RFile(iBytesTotal);
				if (!byterTemp.Write(ref byarrData, iBytesTotal)) {
					bGood=false;
					RReporting.ShowErr("Failed to write raw data to buffer","","RImage SaveRaw("+sFileNow+")");
				}
				if (!byterTemp.Save(sFileNow)) {
					bGood=false;
					RReporting.ShowErr("Failed to save raw data to file","","RImage SaveRaw("+sFileNow+")");
				}
				//TODO: save a description text file
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"saving raw image","RImage SaveRaw("+sFileNow+")");
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
				RReporting.ShowExn(exn,"saving image file via format object","RImage Save(\""+sFileNow+"\", "+imageformatNow.ToString()+")");
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
					decimal yMaxSrc=RConvert.ChrominanceD(ref r, ref g, ref b);
					decimal dMaxShort=0xFFFF;
					Byter byterShorts=new Byter(iBytesTotalNew);
					int iDest;
					for (int iDestPix=0; iDestPix<iPixelsTotal; iDestPix++) {
						iSrc=iDestPix*iBytesPP;
						wVal=(ushort)((RConvert.ChrominanceD(ref byarrData[iSrc+2], ref byarrData[iSrc+1], ref byarrData[iSrc])/yMaxSrc)*dMaxShort);
						byterShorts.Write(ref wVal);
					}
					Init(iWidth,iHeight,iAsBytesPP,false);//sets iBytesPP etc
					byarrData=byterShorts.byarr;
				}
				bmpLoaded.UnlockBits(bmpdata);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"loading image using fast pointers","unsafe RImage Load");
				bGood=false;
			}
			return bGood;
		}//end Load
		*/
		#endregion file operations
		
		#region utilities
		/// <summary>
		/// Looks for the pixel closest to (xCenter,yCenter) where the channel iChannel is outside of byThreshold.
		/// </summary>
		/// <param name="xReturn">returns 0 or above if found</param>
		/// <param name="yReturn">returns 0 or above if found</param>
		/// <param name="xCenter">x location of inclusive starting point</param>
		/// <param name="yCenter">y location of inclusive starting point</param>
		/// <param name="iChannel">channel of this image to compare to byThreshold</param>
		/// <param name="byThreshold">The exclusive limit for the desired pixel's color channel</param>
		/// <param name="bGreaterThanThreshold_FalseForLessThan">If true, the method looks for a pixel where it's channel iChannel is greater than byThreshold; if false, less than byThreshold.</param>
		/// <returns>returns true if found</returns>
		public bool FindNearest(out int xReturn, out int yReturn, int xCenter, int yCenter, int iChannel, byte byThreshold, bool bGreaterThanThreshold_FalseForLessThan) {
			bool bFoundAny=false;
			double rDistNow=double.MaxValue;
			double rDistMin=double.MaxValue;
			xReturn=-1;
			yReturn=-1;
			try {
				if (iChannel>=this.iBytesPP) {
					RReporting.ShowErr("Channel index was truncated to range","finding nearest pixel outside channel value threshold");
					iChannel=this.iBytesPP-1;
				}
				int iLineLoc=iChannel;
				int iChanLoc;
				for (int y=0; y<this.iHeight; y++) {
					iChanLoc=iLineLoc;
					for (int x=0; x<this.iWidth; x++) {
						//debug performance: this would be a lot faster if it started from the center and found the first one outside threshold
						if ( bGreaterThanThreshold_FalseForLessThan 
							? (this.byarrData[iChanLoc]>byThreshold) 
							: (this.byarrData[iChanLoc]<byThreshold) ) {
							rDistNow=RMath.Dist((double)x,(double)y,(double)xCenter,(double)yCenter);
							bFoundAny=true;
							if (rDistNow<rDistMin) {
								xReturn=x;
								yReturn=y;
								rDistMin=rDistNow;
							}
						}
						iChanLoc+=this.iBytesPP;
					}
					iLineLoc+=this.iStride;
					if ( (rDistMin<=0.0) //if AT the center (usable channel value found at search center)
						|| ((rDistMin<=1.0)&&(xReturn>=xCenter)&&(yReturn>=yCenter)) ) break; //if below or after (">=" prevents stopping before center)
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"finding nearest pixel outside channel value threshold");
			}
			return bFoundAny;
		}//end FindNearest
		/// <summary>
		/// For every pixel that is transparent at all, change non-alpha channels to color of nearest completely opaque pixel
		/// </summary>
		/// <returns></returns>
		public bool DeBleed() { //aka realpha aka re-alpha
			bool bGood=true;
			byte byThreshold=254;//if alpha greater than this, use as blending pixel if nearest
			int iLineLocAlpha=this.iBytesPP-1;//ONLY ok since iBytesPP=4;
			int iLineLocNow=0;
			//int iSolidFoundMax=0;
			//int iSolidFound=0;
			int iLocNow;
			int iLocAlpha;
			int xSolid=-1;
			int ySolid=-1;
			if (this.iBytesPP==4) {
				try {
					//bool bAny=true;
					for (int y=0; y<this.iHeight; y++) {
						iLocNow=iLineLocNow;
						iLocAlpha=iLineLocAlpha;
						for (int x=0; x<this.iWidth; x++) {
							if (this.byarrData[iLocAlpha]>0&&this.byarrData[iLocAlpha]<255) {
								//0<alpha<255, so debleed this pixel:
								if (FindNearest(out xSolid, out ySolid, x,y,3,byThreshold,true)) {
									int iLocFound=XYToLocation(xSolid,ySolid);
									for (int iChan=0; iChan<3; iChan++) { //less than 3 since NOT copying alpha!
										//copy ONLY color channels:
										this.byarrData[iLocNow+iChan]=this.byarrData[iLocFound+iChan];
									}
								}
								else {
									RReporting.ShowErr("debleed error: no fully solid pixels to use as source color");
									bGood=false;
									break;
								}
							}
							iLocAlpha+=this.iBytesPP;
							iLocNow+=this.iBytesPP;
						}
						if (!bGood) break;
						iLineLocNow+=this.iStride;
						iLineLocAlpha+=this.iStride;
					}//end for y
					//if (iSolidFoundMax<=0) {}//debug -- optionally, detect threshold if no pixels at or above threshold are found (?)
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"removing background bleed on semitransparent areas");
				}
			}//if iBytesPP==4
			else {
				bGood=false;
				if (RReporting.bDebug) RReporting.Warning("Cannot debleed "+(iBytesPP*8)+"-bit image.");
			}
			return bGood;
		}//end DeBleed
		public static void SetGrayPaletteIfUsesPalette(ref Bitmap bmpNow) {
			//TODO: why doesn't this work? SetGrayPaletteIfUsesPalette
			if (bmpNow!=null&&bmpNow.PixelFormat==PixelFormat.Format8bppIndexed) {
				//if (bmpNow.Palette==null) {
				//	bmpNow.Palette=new ColorPalette();
				//}
				//if (bmpNow.Palette.Entries==null||bmpNow.Palette.Entries.Length<=1) {
				//	bmpNow.Palette.Entries=new Color[256];
				//}
				float fMax=(float)(bmpNow.Palette.Entries.Length-1);
				float fI=0.0f;
				byte byVal;
				for (int i=0; i<bmpNow.Palette.Entries.Length; i++) {
					byVal=RMath.ByRound(255.0f*(fI/fMax));
					bmpNow.Palette.Entries[i]=Color.FromArgb(255, byVal, byVal, byVal);
					fI+=1.0f;
				}
			}
		}
		public static void SetGrayPaletteIfUsesPalette(ref Image image) {
			//TODO: why doesn't this work? SetGrayPaletteIfUsesPalette
			if (image!=null&&image.PixelFormat==PixelFormat.Format8bppIndexed) {
				//if (image.Palette==null) {
				//	image.Palette=new ColorPalette();
				//}
				//if (image.Palette.Entries==null||image.Palette.Entries.Length<=1) {
				//	image.Palette.Entries=new Color[256];
				//}
				float fMax=(float)(image.Palette.Entries.Length-1);
				float fI=0.0f;
				byte byVal;
				for (int i=0; i<image.Palette.Entries.Length; i++) {
					byVal=RMath.ByRound(255.0f*(fI/fMax));
					image.Palette.Entries[i]=Color.FromArgb(255, byVal, byVal, byVal);
					fI+=1.0f;
				}
			}
		}
		///<summary>
		///Returns true if dimensions, number of channels, and total buffer size are the same as riTest.
		///</summary>
		public bool IsInStrideWithAndSameSizeAs(RImage riTest) {//formerly IsLike, formerly IsSameAs
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
		/// <summary>
		/// Crops source rectangle so that it fits into destination at the given point in the destination.
		/// --Fixes to_X and to_Y if top/left was cut off from source rect after finished cropping.
		/// </summary>
		/// <param name="from_X"></param>
		/// <param name="from_Y"></param>
		/// <param name="from_W"></param>
		/// <param name="from_H"></param>
		/// <param name="to_X"></param>
		/// <param name="to_Y"></param>
		/// <param name="riDest"></param>
		/// <returns>Returns false if source is not within riDest</returns>
		public static bool CropSourceRectAndGetNewDest(ref int from_X, ref int from_Y, ref int from_W, ref int from_H, ref int to_X, ref int to_Y, RImage riDest) {
			//change to dest coordinates:
			bool bGood=false;
			try {
				int original_from_X=from_X;
				int original_from_Y=from_Y;
				from_X+=to_X;
				from_Y+=to_Y;
				//Console.Error.WriteLine("{"
				//						+"from_X:"+from_X.ToString()
				//						+"; from_Y:"+from_Y.ToString()
				//						+"; from_W:"+from_W.ToString()
				//						+"; from_H:"+from_H.ToString()
				//						+"; riDest.Width:"+riDest.Width.ToString()
				//						+"; riDest.Height:"+riDest.Height.ToString()
				//						+"}");//debug only
				bGood=riDest!=null?RMath.CropRect(ref from_X,ref from_Y,ref from_W,ref from_H, 0,0,riDest.Width,riDest.Height):false;
				//change back to source coordinates:
				from_X-=to_X;
				from_Y-=to_Y;
				//now get new dest position in case source top-left changed:
				to_X+=(from_X-original_from_X);
				to_Y+=(from_Y-original_from_Y);
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"clipping source rectangle","RImage CropSourceRectAndGetNewDest(int,...)");
			}
			return bGood;
		}//end CropSourceRectAndGetNewDest(int,...)
		/// <summary>
		/// Crops source rectangle so that it fits into destination at the given point in the destination (float version).
		/// --Fixes to_X and to_Y if top/left was cut off from source rect after finished cropping.
		/// </summary>
		/// <param name="from_X"></param>
		/// <param name="from_Y"></param>
		/// <param name="from_W"></param>
		/// <param name="from_H"></param>
		/// <param name="to_X"></param>
		/// <param name="to_Y"></param>
		/// <param name="to_W"></param>
		/// <param name="to_H"></param>
		/// <returns>Returns false if source is not within dest</returns>
		public static bool CropSourceRectAndGetNewDestScaled(ref float from_X, ref float from_Y, ref float from_W, ref float from_H, ref float to_X, ref float to_Y, ref float to_W, ref float to_H, float boundary_X, float boundary_Y, float boundary_W, float boundary_H) {
			//change to dest coordinates:
			bool bWithinDest=false;
			try {
				float original_from_X=from_X;
				float original_from_W=from_W;//for debug log only
				float original_from_Y=from_Y;
				float original_from_H=from_H;//for debug log only
				float original_to_X=to_X;
				float original_to_W=to_W;//for debug log only
				float original_to_Y=to_Y;
				float original_to_H=to_H;//for debug log only
				float xInverseScale=from_W/to_W;//float xScale=to_W/from_W;
				float yInverseScale=from_H/to_H;//float yScale=to_H/from_H;
				//Console.WriteLine(" CropSourceRectAndGetNewDestScaled step 1: {" +"to_X:"+to_X.ToString() +"; to_Y:"+to_Y.ToString() +"; to_W:"+to_W.ToString() +"; to_H:"+to_H.ToString() +"; from_X:"+from_X.ToString() +"; from_Y:"+from_Y.ToString() +"; from_W:"+from_W.ToString() +"; from_H:"+from_H.ToString() +"} (before doing anything)");//debug only
				//RMath.CropRect only crops top or left if LESS THAN boundary, only crops bottom or right IF GREATER than edge:
				//bGood=RMath.CropRect(ref to_X, ref to_Y, ref to_W, ref to_H, boundary_X, boundary_Y, boundary_W, boundary_H); //use FULL width&height since measuring from -.5 to last pixel+.5
				//BELOW has to be done as opposed to RMath.CropRect since source width has to be adjusteed ONLY when dest does, not every time it ends up changing due to top-left cropping
				if (to_X<boundary_X) {
					//subtract from width since X must be moved to the right (or vise versa)
					to_W-=(boundary_X-to_X);//i.e. (0 - -1 = 1 ; so subtract 1 from width) OR i.e. 1 - 0 = 1 ; so subtract 1 from width
					to_X=boundary_X;
					from_X+=(to_X-original_to_X)*xInverseScale;
					from_W-=from_X-original_from_X;//do not multiply by inverse scale since these are source values
				}
				if (to_Y<boundary_Y) {
					//subtract from height since Y must be moved down (or vise versa)
					to_H-=(boundary_Y-to_Y);
					to_Y=boundary_Y;
					from_Y+=(to_Y-original_to_Y)*yInverseScale;
					from_H-=from_Y-original_from_Y;//do not multiply by inverse scale since these are source values
				}
				//Now only crop edges if GREATER than width:
				float boundary_R=boundary_X+boundary_W;
				float to_R=to_X+to_W; //ok since to_X AND to_W were modified oppositely above
				if (to_R>boundary_R) {
					to_W-=to_R-boundary_R;
					from_W-=(to_R-boundary_R)*xInverseScale;//(original_to_W-to_W)*xInverseScale;
				}
				float boundary_B=boundary_Y+boundary_H;
				float to_B=to_Y+to_H;  //ok since to_Y AND to_H were modified oppositely above
				if (to_B>boundary_B) {
					to_H-=to_B-boundary_B;
					from_H-=(to_B-boundary_B)*xInverseScale;//(original_to_H-to_H)*xInverseScale;
				}
				bWithinDest=true;
				if (to_W<=0) {
					RReporting.ShowErr("Math fail!","cropping float destination {"
									   +Environment.NewLine
									   +"to_W:"+to_X.ToString()+"; "
									   +"to_W:"+to_W.ToString()+"; "
									   +"from_X:"+from_X.ToString()+"; "
									   +"from_W:"+from_W.ToString()+"; "
									   +"boundary_X:"+boundary_X.ToString()+"; "
									   +"boundary_W:"+boundary_W.ToString()+"; "
									   +"  original{"
									   +Environment.NewLine
									   +"to_X:"+original_to_X.ToString()+"; "
									   +"to_W:"+original_to_W.ToString()+"; "
									   +"from_X:"+original_from_X.ToString()+"; "
									   +"from_W:"+original_from_W.ToString()+"; "
									   +"}");
					to_W=0;
					bWithinDest=false;
				}
				if (to_H<=0) {
					RReporting.ShowErr("Math fail!","cropping float destination {"
									   +Environment.NewLine
									   +"\t"+"to_Y:"+to_Y.ToString()+"; "
									   +"to_H:"+to_H.ToString()+"; "
									   +"from_Y:"+from_Y.ToString()+"; "
									   +"from_H:"+from_H.ToString()+"; "
									   +"boundary_Y:"+boundary_Y.ToString()+"; "
									   +"boundary_H:"+boundary_H.ToString()+"; "
									   +"}"
									   +"  original{"
									   +Environment.NewLine
									   +"\t"+"to_Y:"+original_to_Y.ToString()+"; "
									   +"to_H:"+original_to_H.ToString()+"; "
									   +"from_Y:"+original_from_Y.ToString()+"; "
									   +"from_H:"+original_from_H.ToString()+"; "
									   +"}");
					to_H=0;
					bWithinDest=false;
				}
			}
			catch (Exception exn) {
				bWithinDest=false;
				RReporting.ShowExn(exn,"clipping source rectangle","RImage CropSourceRectAndGetNewDestScaled(float,...)");
			}
			return bWithinDest;
		}//end CropSourceRectAndGetNewDestScaled(float,...)
		
		
		/// <summary>
		/// Crops destination rectangle to destination
		/// </summary>
		/// <param name="to_X"></param>
		/// <param name="to_Y"></param>
		/// <param name="to_Width"></param>
		/// <param name="to_Height"></param>
		/// <param name="riDest"></param>
		/// <returns>Returns false if rect is not within riDest</returns>
		public static bool CropDestRect(ref int to_X, ref int to_Y, ref int to_Width, ref int to_Height, RImage riDest) {
			return riDest!=null?RMath.CropRect(ref to_X,ref to_Y,ref to_Width,ref to_Height, 0,0,riDest.Width,riDest.Height):false;
		}
		public bool NeedsToCropToFitInto(RImage riDest, int xDest, int yDest) {
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
		}//end NeedsToCropToFitInto

		public bool CanFit(RImage source, int xDest, int yDest) {
			bool bReturn=false;
			try {
				if (source!=null) {
					bReturn=xDest>=0&&yDest>=0
						&&xDest+source.Width<=Width
						&&yDest+source.Height<=Height;
				}
			}
			catch {
				bReturn=false;
			}
			return bReturn;
		}//end CanFit
		/// <summary>
		/// Detect image format or non-usability.
		/// </summary>
		/// <param name="sSetFileExt"></param>
		/// <returns>returns image format--if not usable, return.Guid==Guid.Empty </returns>
		public static ImageFormat ImageFormatFromExt(string sSetFileExt) {
			//System.Drawing.Imaging.ImageFormat.MemoryBmp
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
			else if (sLower==("bmp")) return ImageFormat.Bmp;
			else {
				ImageFormat imgfmt=new ImageFormat(Guid.Empty);
				return imgfmt;
			}
		}// ImageFormatFromExt
		/// <summary>
		/// Get lowercase extension of given file IF known image file type, else return blank.
		/// </summary>
		/// <param name="sNameToTruncate"></param>
		/// <param name="sExt"></param>
		/// <returns></returns>
		public static string KnownExtensionFromNameElseBlank(string NameToAnalyze) {
			if (NameToAnalyze!=null) {
				NameToAnalyze=NameToAnalyze.ToLower();
				if (NameToAnalyze.EndsWith(".png")) { return "png"; }
				else if (NameToAnalyze.EndsWith(".jpg")) { return "jpg";}
				else if (NameToAnalyze.EndsWith(".jpe")) { return "jpe";}
				else if (NameToAnalyze.EndsWith(".jpeg")){ return "jpeg";}
				else if (NameToAnalyze.EndsWith(".gif")) { return "gif"; }
				else if (NameToAnalyze.EndsWith(".exi")) { return "exi";}
				else if (NameToAnalyze.EndsWith(".exif")){ return "exif";}
				else if (NameToAnalyze.EndsWith(".emf")) { return "emf"; }
				else if (NameToAnalyze.EndsWith(".tif")) { return "tif";}
				else if (NameToAnalyze.EndsWith(".tiff")){ return "tiff";}
				else if (NameToAnalyze.EndsWith(".ico")) { return "ico";}
				else if (NameToAnalyze.EndsWith(".wmf")) { return "wmf"; }
				else if (NameToAnalyze.EndsWith(".bmp")) { return "bmp"; }
			}
			return "";
		}// KnownExtensionFromNameElseBlank
		public static ImageFormat ImageFormatFromNameElseCapitalizedPng(ref string sNameToTruncate, out string sExt) {
			sExt=KnownExtensionFromNameElseBlank(sNameToTruncate);
			if (sExt=="") sExt="PNG";//return ImageFormat.Png;
			else RString.ShrinkByRef(ref sNameToTruncate, sExt.Length+1);
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
		public IRect ToRect() {
			return new IRect(0,0,Width,Height);
		}
		public void ToRect(ref IRect rectReturn) {
			if (rectReturn==null) rectReturn=new IRect(0,0,Width,Height);
			else rectReturn.Set(0,0,Width,Height);
		}
		/// <summary>
		/// Gets the bitdepth-sensitive buffer location at (x,y)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private int XYToLocation(int x, int y) {
			if (x>=0&&y>=0&&x<Width&&y<Height) return y*iStride+x*iBytesPP;
			else {
				RReporting.Debug("XYToLocation("+x.ToString()+","+y.ToString()+") is outside of "+Width.ToString()+"x"+Height.ToString()+" image.");
				return y*iStride+x*iBytesPP;;
			}
		}
		public static string DrawMode_ToString(int iDrawMode) {
			string sReturn="uninitialized-drawmode";
			try {
				sReturn=sarrDrawMode[iDrawMode];
			}
			catch {
				sReturn="nonexistant-drawmode-\""+iDrawMode.ToString()+"\"";
			}
			return sReturn;
		}
		public static int DrawMode_(string DrawMode_String) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<sarrDrawMode.Length; iNow++) {
					if (DrawMode_String==sarrDrawMode[iNow]) {
						iReturn=iNow;
						break;
					}
				}
			}
			catch {
				iReturn=-1;
			}
			return iReturn;
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
				sData+=RConvert.ToHex(this.byarrData, iLineStart, iStride, iBytesPP);
				sData+=Environment.NewLine;
			}
			if (sData.EndsWith(Environment.NewLine))
				sData=sData.Substring(0,sData.Length-Environment.NewLine.Length);
			RString.StringToFile(sFile, sData);
		}
		public string Description() {
			return iWidth.ToString()+"x"+iHeight.ToString()+"x"+iBytesPP.ToString()+((this.byarrData!=null)?("[BufferLength:"+this.byarrData.Length+"bytes]"):"[NULL-BUFFER]");
		}//"=~iBytesTotal:"+iBytesTotal.ToString()
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
		public PixelFormat PixelFormatNow {
			get {
				if (this.iBytesPP==1) return PixelFormat.Format8bppIndexed;//assumes no grayscale in framework
				else if (this.iBytesPP==3) return PixelFormat.Format24bppRgb;//assumes BGR, though says Rgb
				else if (this.iBytesPP==2) return PixelFormat.Format16bppGrayScale;//assumes no 16bit color allowed but this is a 16-bit deep gray image
				return PixelFormat.Format32bppArgb;
			}
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
				RReporting.ShowExn(exn,"setting palette of Bitmap object to gray","RImage SetGrayPalette");
			}
		}
		public static Color Multiply(Color colorX, double multiplier) {
			return Color.FromArgb(colorX.A, RMath.ByRound((double)colorX.R*multiplier), RMath.ByRound((double)colorX.G*multiplier), RMath.ByRound((double)colorX.B*multiplier));
		}
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
		public static bool Redim(ref RImage[] valarr, int iSetSize) {
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
			else {
				string sCallStack=RReporting.StackTraceToLatinCallStack(new System.Diagnostics.StackTrace());
				RReporting.ShowErr("Prevented setting a buffer array to a negative size ", "setting buffer array length", "Redim(rimage array," + iSetSize.ToString() + ",sender:" + sCallStack+")");
			}
			return bGood;
		}//end Redim(RImage[],...)
// 		public byte BrushR() {
// 			return rpaintFore.R;
// 		}
// 		public byte BrushG() {
// 			return rpaintFore.G;
// 		}
// 		public byte BrushB() {
// 			return rpaintFore.B;
// 		}
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
		public void Clear(Color colorNow) {//keep this, to mimic the equivalent Graphics object method
			Fill(new byte[] {colorNow.B,colorNow.G,colorNow.R,colorNow.A});
		}
		public void Fill(byte byGrayVal) {
			RMemory.Fill(ref byarrData,byGrayVal,0,iBytesTotal);
		}
		public void Fill(uint dwPixelBGRA_LittleEndian_LeastSignificantByteIsAlpha) {
			if (iBytesPP==4) {
				RMemory.Fill(ref byarrData,dwPixelBGRA_LittleEndian_LeastSignificantByteIsAlpha,0,iBytesTotal/4);
			}
			else {
				RPaint rpaintFill=new RPaint(dwPixelBGRA_LittleEndian_LeastSignificantByteIsAlpha);
				Fill(rpaintFill);
				//RReporting.ShowErr("Filling this type of surface with 32-bit color is not implemented","",String.Format("Fill(32-bit){{ImageBitDepth:{0}}}",(iBytesPP*8)));
			}
		}
		public void Fill(byte[] byarrPixel32bitBGRA) {
			if (iBytesPP==4) {
				RMemory.Fill4ByteChunksByUnitCount(ref byarrData,ref byarrPixel32bitBGRA,0,0,iBytesTotal/4);
			}
			else {
				RPaint rpaintFill=RPaint.FromArgb(byarrPixel32bitBGRA[3],
											 byarrPixel32bitBGRA[2],
											 byarrPixel32bitBGRA[1],
											 byarrPixel32bitBGRA[0]);
				Fill(rpaintFill);
				//RReporting.ShowErr("Filling this type of surface with 32-bit color is not implemented","",String.Format("Fill(32-bit){{ImageBitDepth:{0}}}",(iBytesPP*8)));
			}
		}
		/// <summary>
		/// If this image is grayscale, average will be used as fill color
		/// </summary>
		/// <param name="rpaintFill"></param>
		public void Fill(RPaint rpaintFill) {
			try {
				if (rpaintFill!=null) {
					if (this.iBytesPP==4) {
						RMemory.Fill(ref this.byarrData,rpaintFill.data32Copied64,0,this.iBytesTotal);
					}
					else if (this.iBytesPP==3) {
						RMemory.Fill(ref this.byarrData,rpaintFill.data24Copied48,0,this.iBytesTotal);
					}
					else if (this.iBytesPP==1) {
						byte byVal=RMath.ByRound(((double)rpaintFill.R+(double)rpaintFill.G+(double)rpaintFill.B)/3.0);
						RMemory.Fill(ref this.byarrData,byVal,0,this.iBytesTotal);
					}
					else {
						RReporting.ShowErr("SKIPPED Fill due to unknown bitdepth "+iBytesPP.ToString(),"filling pixel buffer using paint object");
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"filling pixel buffer using paint object");
			}
		}
		#endregion utilities

		#region Pattern Recognition
		/// <summary>
		/// Detects 0 alpha (or 0 gray value if 8-bit).  Will always be false if 24-bit
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="iPixelCount"></param>
		/// <returns></returns>
		public bool IsTransparentVLine(int x, int y, int iPixelCount) {
			int iFound=0;
			try {
				if (iBytesPP!=3) {
					int iAOffset=3;
					if (iBytesPP==1) iAOffset=0;
					int iBuffer=XYToLocation(x,y);
					if (iBuffer>-1&&iBuffer<iBytesTotal) {
						for (int iNow=0; iNow<iPixelCount; iNow++) {
							if (byarrData[iBuffer+iAOffset]==0) iFound++; //+iAOffset assumes 32-bit BGRA or 8-bit value used as alpha
							iBuffer+=iStride;
						}
					}
					else {
						RReporting.ShowErr( "No line of pixels exists at location", "checking for transparent line", String.Format("IsTransparentVLine(x:{0},y:{1},count:{2}){found:{3}}",x,y,iPixelCount,iFound) );
					}
				}
				//else iFound==0 so not transparent
			}
			catch (Exception exn) {
				//do not modify bReturn, since exception implies nonexistent pixel
				RReporting.ShowExn( exn, "checking for transparent line", String.Format("IsTransparentVLine(x:{0},y:{1},count:{2}){found:{3}}",x,y,iPixelCount,iFound) );
			}
			return iFound==iPixelCount;
		}//end IsTransparentVLine
		#endregion Pattern Recognition
		
		#region Get Methods
		public Color GetPixel(int x, int y) {//retain Color as return in order to mimic the Bitmap method of the same name
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
		/// <summary>
		/// Gets pixel.  Will create paintReturn if necessary.  A=255 if this image is 24-bit.  ALL channels will be gray value if this image is 8-bit grayscale.
		/// uses GrayHandling_Overlay.GrayToColorMode
		/// </summary>
		/// <param name="paintReturn">Returns value at (x,y) otherwise transparent.  Is ALWAYS set.  Alpha will be transparent and color will remain same if can't get pixel.</param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="GrayHandling_Overlay_OrNull">ROverlay for gray handling, or null (If null, all channels including alpha will to be set to gray value if this image is gray)</param>
		/// <returns>Returns false if can't get pixel (but paintReturn is always set).</returns>
		public bool GetPixel(ref RPaint paintReturn, int x, int y, ROverlay GrayHandling_Overlay_OrNull) {
			bool bGood=true;
			try {
			if (byarrData!=null) {
				int iLocation=this.XYToLocation(x,y);
				if (this.iBytesPP==4) {
					if (paintReturn==null) paintReturn=RPaint.FromArgb(byarrData[iLocation+3],byarrData[iLocation+2],byarrData[iLocation+1],byarrData[iLocation]);
					else paintReturn.SetArgb(byarrData[iLocation+3],byarrData[iLocation+2],byarrData[iLocation+1],byarrData[iLocation]);
				}
				else if (this.iBytesPP==3) {
					if (paintReturn==null) paintReturn=RPaint.FromArgb(255,byarrData[iLocation+2],byarrData[iLocation+1],byarrData[iLocation]);
					else paintReturn.SetArgb(255,byarrData[iLocation+2],byarrData[iLocation+1],byarrData[iLocation]);
				}
				else if (this.iBytesPP==1) {
					ROverlay.GrayToColor(ref paintReturn, byarrData[iLocation],GrayHandling_Overlay_OrNull);
				}
				else bGood=false;//unknown bitdepth
			}
			if (paintReturn==null) paintReturn=new RPaint(true);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting paint object from pixel");
			}
			return bGood;
		}//end GetPixel(RPaint,...)
		public byte GetAlpha(int x, int y) {
			return GetChannelValue(x,y,iBytesPP-1);//debug silently degrades (fixes) if grayscale (since iBytesPP-1 is 0 which is first channel)
		}
		public byte GetChannelValue(int x, int y, int iChannel) {
			byte byReturn=0;
			try {
				if (iChannel<0) {
					if (RReporting.bDebug) RReporting.Debug("Channel number error: iChannel was less than zero (will be cropped to 0)!  {iChannel:"+iChannel.ToString()+"; iBytesPP:"+iBytesPP.ToString()+ "}.");
					iChannel=0;
				}
				else if (iChannel>=iBytesPP) {
					if (RReporting.bDebug) RReporting.Debug("Channel number error: iChannel was out of range (will be cropped to last channel). {iChannel:"+iChannel.ToString()+"; iBytesPP:"+iBytesPP.ToString()+ "}.");
					iChannel=iBytesPP-1;
				}
				return byarrData[y*iStride+x*iBytesPP+iChannel];
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting a channel value at (x,y)","RImage ChannelValue");
			}
			return byReturn;
		}
		#endregion Get Methods
		
		#region Per-pixel Interpolation Methods
		public static bool InterpolatePixel(ref RImage riDest, ref RImage riSrc, int iDest, ref FPoint pSrc) {
			bool bGood=false;
			bool bOnX;
			bool bOnY;
			float dWeightNow;
			float dWeightTotal;
			float dHeavyChannel;
			FPoint[] dparrQuad; //rounded
			int iSrcRoundX;
			int iSrcRoundY;
			float dSrcRoundX;
			float dSrcRoundY;
			//int iSampleQuadIndex;
			float xMaxSrc;
			float yMaxSrc;
			//float dQuarterWeight;
			int iQuad;
			int iChan;
			int iDestNow;
			int iTotal=0;
			int[] iarrLocOfQuad;
			try {
				iarrLocOfQuad=new int[4];
				xMaxSrc=(float)riSrc.iWidth-1.0f;
				yMaxSrc=(float)riSrc.iHeight-1.0f;
				//iDest=riDest.iStride*ipDest.Y+riDest.iBytesPP*ipDest.X;
				dWeightNow=0;
				dWeightTotal=0;
				dparrQuad=new FPoint[4];
				iSrcRoundX=(int)(pSrc.X+.5f);
				iSrcRoundY=(int)(pSrc.Y+.5f);
				dSrcRoundX=(float)iSrcRoundX;
				dSrcRoundY=(float)iSrcRoundY;
				if (dSrcRoundX<pSrc.X) {
					if (dSrcRoundY<pSrc.Y) {
						//iSampleQuadIndex=0;
						dparrQuad[0].X=dSrcRoundX;		dparrQuad[0].Y=dSrcRoundY;
						dparrQuad[1].X=dSrcRoundX+1.0f;	dparrQuad[1].Y=dSrcRoundY;
						dparrQuad[2].X=dSrcRoundX;		dparrQuad[2].Y=dSrcRoundY+1.0f;
						dparrQuad[3].X=dSrcRoundX+1.0f;	dparrQuad[3].Y=dSrcRoundY+1.0f;
					}
					else {
						//iSampleQuadIndex=2;
						dparrQuad[0].X=dSrcRoundX;		dparrQuad[0].Y=dSrcRoundY-1.0f;
						dparrQuad[1].X=dSrcRoundX+1.0f;	dparrQuad[1].Y=dSrcRoundY-1.0f;
						dparrQuad[2].X=dSrcRoundX;		dparrQuad[2].Y=dSrcRoundY;
						dparrQuad[3].X=dSrcRoundX+1.0f;	dparrQuad[3].Y=dSrcRoundY;
					}
				}
				else {
					if (dSrcRoundY<pSrc.Y) {
						//iSampleQuadIndex=1;
						dparrQuad[0].X=dSrcRoundX-1.0f;	dparrQuad[0].Y=dSrcRoundY;
						dparrQuad[1].X=dSrcRoundX;		dparrQuad[1].Y=dSrcRoundY;
						dparrQuad[2].X=dSrcRoundX-1.0f;	dparrQuad[2].Y=dSrcRoundY+1.0f;
						dparrQuad[3].X=dSrcRoundX;		dparrQuad[3].Y=dSrcRoundY+1.0f;
					}
					else {
						//iSampleQuadIndex=3;
						dparrQuad[0].X=dSrcRoundX-1.0f;	dparrQuad[0].Y=dSrcRoundY-1.0f;
						dparrQuad[1].X=dSrcRoundX;		dparrQuad[1].Y=dSrcRoundY-1.0f;
						dparrQuad[2].X=dSrcRoundX-1.0f;	dparrQuad[2].Y=dSrcRoundY;
						dparrQuad[3].X=dSrcRoundX;		dparrQuad[3].Y=dSrcRoundY;
					}
				}
				if (pSrc.X<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].X<0) dparrQuad[iQuad].X=0;
					}
				}
				else if (pSrc.X>xMaxSrc) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].X>xMaxSrc) dparrQuad[iQuad].X=xMaxSrc;
					}
				}
				if (pSrc.Y<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].Y<0) dparrQuad[iQuad].Y=0;
					}
				}
				else if (pSrc.Y>yMaxSrc) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].Y>yMaxSrc) dparrQuad[iQuad].Y=yMaxSrc;
					}
				}
				if (pSrc.X==(float)iSrcRoundX) bOnX=true;
				else bOnX=false;
				if (pSrc.Y==(float)iSrcRoundY) bOnY=true;
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
							dWeightNow=fDiagonalUnit-RMath.Dist(ref pSrc, ref dparrQuad[iQuad]);//RMath.SafeDivide(fDiagonalUnit,RMath.Dist(ref pSrc, ref dparrQuad[iQuad]),fDiagonalUnit);
							dWeightTotal+=dWeightNow; //debug performance, this number always becomes the same theoretically
							dHeavyChannel+=(float)riSrc.byarrData[iarrLocOfQuad[iQuad]+iChan]*dWeightNow;
						}
						//dQuarterWeight=dWeightTotal/4.0
						riDest.byarrData[iDestNow]=(byte)(dHeavyChannel/dWeightTotal);
						iDestNow++;
					}
				}
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"interpolating pixel","RImage InterpolatePixel(dest="+RImage.Description(riDest)+",source="+RImage.Description(riSrc)+",iDest="+iDest.ToString()+",source-FPoint="+FPoint.ToString(pSrc)+")");
				bGood=false; //debug show error
			}
			return bGood;
		}//end InterpolatePixel(...,FPoint)
		
		public static bool InterpolatePixel(ref RImage riDest, ref RImage riSrc, int iDest, ref DPoint pSrc) {
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
			double xMaxSrc;
			double yMaxSrc;
			int iQuad;
			int iChan;
			int iDestNow;
			int iTotal=0;
			int[] iarrLocOfQuad;
			try {
				iarrLocOfQuad=new int[4];
				xMaxSrc=(double)riSrc.iWidth-1.0d;
				yMaxSrc=(double)riSrc.iHeight-1.0d;
				//iDest=riDest.iStride*ipDest.Y+riDest.iBytesPP*ipDest.X;
				dWeightNow=0;
				dWeightTotal=0;
				dparrQuad=new DPoint[4];
				iSrcRoundX=(int)(pSrc.X+.5);
				iSrcRoundY=(int)(pSrc.Y+.5);
				dSrcRoundX=(double)iSrcRoundX;
				dSrcRoundY=(double)iSrcRoundY;
				if (dSrcRoundX<pSrc.X) {
					if (dSrcRoundY<pSrc.Y) {
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
					if (dSrcRoundY<pSrc.Y) {
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
				if (pSrc.X<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].X<0) dparrQuad[iQuad].X=0;
					}
				}
				else if (pSrc.X>xMaxSrc) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].X>xMaxSrc) dparrQuad[iQuad].X=xMaxSrc;
					}
				}
				if (pSrc.Y<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].Y<0) dparrQuad[iQuad].Y=0;
					}
				}
				else if (pSrc.Y>yMaxSrc) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].Y>yMaxSrc) dparrQuad[iQuad].Y=yMaxSrc;
					}
				}
				if (pSrc.X==(double)iSrcRoundX) bOnX=true;
				else bOnX=false;
				if (pSrc.Y==(double)iSrcRoundY) bOnY=true;
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
							dWeightNow=dDiagonalUnit-RMath.Dist(ref pSrc, ref dparrQuad[iQuad]);
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
				RReporting.ShowExn(exn,"interpolating pixel","RImage InterpolatePixel(dest:"+RImage.Description(riDest)+",source:"+RImage.Description(riSrc)+",iDest"+iDest.ToString()+",source-DPoint:"+DPoint.ToString(pSrc)+")");
				bGood=false; //debug show error
			}
			return bGood;
		}//end InterpolatePixel(...,DPoint)
		
		#endregion Per-pixel Interpolation Methods
		
		public static string Description(RImage rimage) {
			return (rimage!=null)?rimage.Description():"null";
		}

		#region Pixel-based Geometry Raster Drawing
		public bool InvertPixel(int x, int y) {
			bool bGood=false;
			try {
				int iDestNow=XYToLocation(x,y);
				for (int iChan=0; iChan<iBytesPP; iChan++) {
					if (byarrData[iDestNow]==255&&RMath.SubtractBytes[255][(int)byarrData[iDestNow]]!=0) Console.Error.WriteLine("SubtractBytes array is incorrect");//debug only
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
		///<summary>
		///Sets pixel using foreground rpaint
		///</summary>
		public unsafe void SetPixel(int x, int y) {
			SetPixel(rpaintFore,x,y);
		}
		///TODO: eliminate assumed rpaintFore overloads of ALL functions
		public unsafe void SetPixel(RPaint paintSource, int x, int y) {
			try {
				if (paintSource==null) {
					paintSource=new RPaint();
					paintSource.SetRgb(255,255,255);
				}
				if (iBytesPP==4) {
					fixed (byte* lpDest=&byarrData[XYToLocation(x,y)], lpSrc=paintSource.data32) {
						*((UInt32*)lpDest) = *((UInt32*)lpSrc);
					}
				}
				else {
					int iSrc=0;
					int iDest=XYToLocation(x,y);
					while (iSrc<iBytesPP) {
						byarrData[iDest]=paintSource.data32[iSrc];
						iSrc++;
						iDest++;
					}
				}//end else iBytesPP!=4
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"painting pixel using fast pointers","RImage SetPixel("+x.ToString()+","+y.ToString()+")");
				//return false;
			}
			//return true;
		}//end SetPixel(x,y)
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
				RReporting.ShowExn( exn, "drawing pixel leaving alpha unmodified", String.Format("SetPixelArgb({0},{1},...)",x,y) );
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
				RReporting.ShowExn( exn, "drawing pixel ARGB value", String.Format("SetPixelArgb({0},{1},...)",x,y) );
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
				RReporting.ShowExn(exn,"drawing to red channel",String.Format("({0},{1},r)",x,y));
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
				RReporting.ShowExn(exn, "drawing to green channel", String.Format("SetPixelG({0},{1},...)",x,y));
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
				RReporting.ShowExn( exn, "drawing to blue channel", String.Format("SetPixelB({0},{1},...)",x,y) );
			}
			return bGood;
		}//end SetPixelB
		public bool SetPixelHsva(int x, int y, double h, double s, double v, double a0To1) {
			bool bGood=false;
			try {
				if (iBytesPP==4) {
					byte r,g,b;
					//NOTE: Hsv is NOT H<360 it is H<1.0
					RImage.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow++]=r;
					byarrData[iDestNow]=(byte)(a0To1*255.0);
				}
				else if (iBytesPP==3) {
					byte r,g,b;
					RImage.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
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
				RReporting.ShowExn(exn,"setting pixel using HSV copying alpha and converting alpha multiplier to byte (double-precision version)",String.Format("DrawPixelHSV({0},{1},...)",x,y));
			}
			return bGood;
		}//end SetPixelHsva
		public bool SetPixelHsva(int x, int y, float h, float s, float v, float a0To1) {
			bool bGood=false;
			try {
				if (iBytesPP==4) {
					byte r,g,b;
					//NOTE: Hsv is NOT H<360 it is H<1.0
					RImage.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
					//SetPixelArgb(255,byR,byG,byB);
					int iDestNow=XYToLocation(x,y);
					byarrData[iDestNow++]=b;
					byarrData[iDestNow++]=g;
					byarrData[iDestNow++]=r;
					byarrData[iDestNow]=(byte)(a0To1*255.0);
				}
				else if (iBytesPP==3) {
					byte r,g,b;
					RImage.HsvToRgb(out r, out g, out b, ref h, ref s, ref v);
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
				RReporting.ShowExn(exn,"setting pixel using HSV copying alpha and converting alpha multiplier to byte (single-precision)",String.Format("DrawPixelHSV({0},{1},...)",x,y));
			}
			return bGood;
		}//end SetPixelHsva
		//public bool DrawRectStyleCropped(Color colorNow, IRect rectDest) {
		//	try {
		//		RPaint rpaintNow=RPaint.FromArgb(colorNow.A,colorNow.R,colorNow.G,colorNow.B);
		//		if (rectDest!=null) return DrawRectStyleCropped(colorNow,rectDest.X, rectDest.Y, rectDest.Width, rectDest.Height);
		//	}
		//	catch (Exception exn) {
		//		RReporting.ShowExn(exn,"setting paint but calling rectangle draw by Color object");
		//	}
		//	return false;
		//}
		public bool DrawRectStyleCropped(RPaint rpaint, IRect rectDest) {
			try {
				if (rectDest!=null) return DrawRectStyleCropped(rpaint,rectDest.X, rectDest.Y, rectDest.Width, rectDest.Height);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"calling rectangle draw using values from rect");
			}
			return false;
		}
		public bool DrawRectStyleCropped(RPaint paintSource, int rect_Top, int rect_Left, int rect_Width, int rect_Height) {
			RPaint rpaintForeOrig=rpaintFore;
			bool bGood=false;
			try {
				//TODO: UseDrawRectBorderHomogeneous (...only for equally thick borders)
				//TODO: CornerRadius
				//paintSource was formerly colorNow
				if (paintSource==null) paintSource=rpaintFore.Copy();
				rpaintFore=rpaintTemp;//paintSource.Copy();//TODO: debug performance
				rpaintFore.Set(paintSource);//NOTE: this allows modifying the temporary rpaintFore while still referring to the original paintSource parameter
				if (rpaintFore.A==0) {
					rpaintFore.SetRgb_IgnoreAlpha(128,128,128);
				}
				if (rect_Left<0) {
					rect_Width+=rect_Left;//actually subtracts
					rect_Left=0;
				}
				if (rect_Top<0) {
					rect_Height+=rect_Top;//actually subtracts
					rect_Top=0;
				}
				if (rect_Top+rect_Height>iHeight) rect_Height-=(rect_Top+rect_Height)-iHeight;
				if (rect_Left+rect_Width>iWidth) rect_Width-=(rect_Left+rect_Width)-iWidth;
				if (rect_Width>0&&rect_Height>0) {//if(rect_Left<iWidth&&rect_Left>=0&&rect_Height>=0&&rect_Top+rect_Height-1<rect_Height) 
					if (rpaintFore.A!=0) {
						DrawRectFilled(rect_Top,rect_Left,rect_Width,rect_Height,"DrawRectStyle()");
					}
					//TODO: DrawRectStyle - vectorize and make color alpha polygons
					//dark:
					rpaintFore.SetRgb(RMath.SubtractBytes[paintSource.R][85],RMath.SubtractBytes[paintSource.G][85],RMath.SubtractBytes[paintSource.B][85]);
					DrawHorzLine(rect_Left, rect_Top+rect_Height-1, rect_Width);//, "DrawRectStyle(){part:B}");
					DrawVertLine(rect_Left, rect_Top, rect_Height);//, "DrawRectStyle(){part:L}");//if (rect_Left>=0&&rect_Left<iWidth&&rect_Top>=0&&rectTop<iHeight)
					//light:
					rpaintFore.SetRgb(RMath.AddBytes[paintSource.R][85],RMath.AddBytes[paintSource.G][85],RMath.AddBytes[paintSource.B][85]);
					DrawHorzLine(rect_Left, rect_Top, rect_Width);//, "DrawRectStyle(){part:T}");//if (rect_Left<iWidth&&rect_Left>=0&&rect_Height>0)
					DrawVertLine(rect_Left+rect_Width-1,rect_Top,rect_Height);//,"DrawRectStyle(){part:R}");//if (rect_Left+rect_Width-1<iWidth&&rect_Top>=0&&rect_Width>0)
					bGood=true;
				}
				rpaintFore=rpaintForeOrig;
			}
			catch (Exception exn) {
				string sStackFrames=RReporting.StackTraceToLatinCallStack(new System.Diagnostics.StackTrace());
				RReporting.ShowExn(exn,"drawing css rectangle",sStackFrames);
			}
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
			if (iWidth>0&&iHeight>0) return DrawRect(xDest,yDest,iWidth,iHeight);
			else return true;
		}
		//public bool DrawRect(int xDest, int yDest, int iWidth, int iHeight, string sSender_ForErrorTracking) {
			
		//}
		public bool DrawRect(int xDest, int yDest, int iWidth, int iHeight) {
			bool bGood=true;
			if (xDest>=0&&yDest>=0) {
				if (!DrawHorzLine(xDest,yDest,iWidth)) bGood=false;
				iHeight--;
				if (!DrawHorzLine(xDest,yDest+iHeight,iWidth)) bGood=false;
				yDest++;
				iHeight--;//skip the verticle lines' other end too
				if (iHeight>0) {
					if (!DrawVertLine(xDest,yDest,iHeight)) bGood=false;
					iWidth--;
					if (!DrawVertLine(xDest+iWidth,yDest,iHeight)) bGood=false;
				}
			}
			else {
				string sCallStack=RReporting.StackTraceToLatinCallStack(new System.Diagnostics.StackTrace());
				RReporting.Warning(sCallStack+" skipped drawing out-of-range rect. {rect:"+IRect.ToString(xDest,yDest,iWidth,iHeight)+"}");
				bGood=false;
			}
			return bGood;
		}
		public bool DrawRect(ref IZone izoneExclusive) {
			return DrawRect(izoneExclusive.Left, izoneExclusive.Top,
						 izoneExclusive.Right-izoneExclusive.Left,
						 izoneExclusive.Bottom-izoneExclusive.Top);
		}
		public bool DrawRect(IRect rectRect) {
			return DrawRect(rectRect.X, rectRect.Y, rectRect.Width, rectRect.Height);
		}
		public bool DrawRectFilled(int xDest, int yDest, int iWidth, int iHeight, string sSender_ForErrorTracking) {
			return DrawRectFilled(rpaintFore,xDest, yDest, iWidth, iHeight);
		}
		/// <summary>
		/// Fills this image with paintSource.  Uses average of BGR if this image is grayscale.
		/// </summary>
		/// <param name="paintSource"></param>
		/// <param name="xDest"></param>
		/// <param name="yDest"></param>
		/// <param name="iWidth"></param>
		/// <param name="iHeight"></param>
		/// <returns></returns>
		public unsafe bool DrawRectFilled(RPaint paintSource, int xDest, int yDest, int iWidth, int iHeight) {
			//TODO: implement overlay modes here
			if ((iWidth<1)||(iHeight<1)) return false;
			bool bGood=true;
			try {
				if (paintSource!=null) {
					int iDest=yDest*iStride+xDest*iBytesPP;
					if (iBytesPP==4) {
						fixed (byte* lpDest=&byarrData[iDest], lpSrc=paintSource.data32Copied64) { //keeps GC at bay
							byte* lpDestNow;
							byte* lpDestLine=lpDest;
							for (int yNow=0; yNow<iHeight; yNow++) {
								lpDestNow=lpDestLine;
								for (int i=iWidth/2; i!=0; i--) {
									*((UInt64*)lpDestNow) = *((UInt64*)lpSrc);
									lpDestNow+=8;
								}
								if ((iWidth%2)!=0) {
									*((UInt32*)lpDestNow) = *((UInt32*)lpSrc);
								}
								lpDestLine+=iStride;
							}
						}
					}
					else if (iBytesPP==3) {
						RMemory.Fill(ref byarrData, paintSource.data24Copied48,0,this.iBytesTotal);
					}
					else if (iBytesPP==1) {
						RMemory.Fill(ref byarrData, RMath.ByRound( ((double)paintSource.B+(double)paintSource.G+(double)paintSource.R) / 3.0 ),0,this.iBytesTotal);
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				string sCallStack=RReporting.StackTraceToLatinCallStack(new System.Diagnostics.StackTrace());
				RReporting.ShowExn(exn, "drawing filled rectangle", String.Format("RImage DrawRectFilled(x:{0},y:{1},width:{2},height:{3},sender:{4})",xDest,yDest,iWidth,iHeight,sCallStack) );
			}
			return bGood;
		} //DrawRectFilled
		///<summary>
		///Draws a gradient from top to bottom from Foreground to Background color
		///</summary>
		public bool DrawGradTopDownRectFilled(RPaint rpaintTop, RPaint rpaintBottom, int xDest, int yDest, int iWidth, int iHeight) {
			//TODO: implement overlay modes here
			if ((iWidth<1)||(iHeight<1)) return false;
			bool bGood=true;
			try {
				int iLineStart=yDest*iStride+xDest*iBytesPP;
				int iFullApproach=iHeight-1;
				byte byApproachNow;
				rpaintTemp.A=255;
				for (int yNow=0; yNow<iHeight; yNow++) {
					//int iDest=iLineStart;
					byApproachNow=(byte)RMath.SafeDivideRound(RMath.SafeMultiply(yNow,255),iFullApproach,255);
					rpaintTemp.B=RMath.AlphaLook[rpaintBottom.B][rpaintTop.B][byApproachNow];
					rpaintTemp.G=RMath.AlphaLook[rpaintBottom.G][rpaintTop.G][byApproachNow];
					rpaintTemp.R=RMath.AlphaLook[rpaintBottom.R][rpaintTop.R][byApproachNow];
					DrawHorzLine(rpaintTemp,xDest,yNow,iWidth);
				}//end for yNow
			}
			catch (Exception exn) {
				bGood=false;
				string sCallStack=RReporting.StackTraceToLatinCallStack(new System.Diagnostics.StackTrace());
				RReporting.ShowExn(exn, "drawing filled rectangle", String.Format("RImage DrawGradTopDownRectFilled(x:{0},y:{1},width:{2},height:{3},sender:{4})",xDest,yDest,iWidth,iHeight,sCallStack) );
			}
			return bGood;
		} //DrawGradTopDownRectFilled
		public bool DrawRectFilledCropped(int xDest, int yDest, int iWidth, int iHeight) {
			return DrawRectFilledCropped(rpaintFore,xDest,yDest,iWidth,iHeight);
		}
		public bool DrawRectFilledCropped(RPaint paintSource, int xDest, int yDest, int iWidth, int iHeight) {
			//formerly DrawRectCroppedFilled
			RMath.CropRect(ref xDest, ref yDest, ref iWidth, ref iHeight, 0, 0, Width, Height);
			if (iWidth>0&&iHeight>0) return DrawRectFilled(paintSource,xDest,yDest,iWidth,iHeight);
			else return true;
		}
		public bool DrawRectFilledCropped(IRect rectNow) {
			return DrawRectFilledCropped(rpaintFore,rectNow);
		}
		public bool DrawRectFilledCropped(RPaint paintSource, IRect rectNow) {
			//formerly DrawRectCroppedFilled
			int xDest=rectNow.X;
			int yDest=rectNow.Y;
			int iWidth=rectNow.Width;
			int iHeight=rectNow.Height;
			RMath.CropRect(ref xDest, ref yDest, ref iWidth, ref iHeight, 0, 0, Width, Height);
			if (iWidth>0&&iHeight>0) return DrawRectFilled(paintSource,xDest,yDest,iWidth,iHeight);
			else return true;
		}
		public bool DrawRectFilled(IRect rectRect) {
			return DrawRectFilled(rpaintFore,rectRect);
		}
		public bool DrawRectFilled(RPaint paintSource, IRect rectRect) {
			return DrawRectFilled(paintSource, rectRect.X, rectRect.Y, rectRect.Width, rectRect.Height);
		}
		//public bool DrawRectFilledHsva(IRect rectDest, float h, float s, float v, float a) {
		//	return DrawRectFilledHsva(rectDest.X,rectDest.Y,rectDest.Width,rectDest.Height,h,s,v,a);
		//}
		//public bool DrawRectFilledHsva(IRect rectDest, double h, double s, double v, double a) {
		//	return DrawRectFilledHsva(rectDest.X,rectDest.Y,rectDest.Width,rectDest.Height,h,s,v,a);
		//}
		//public bool DrawRectFilledHsva(int xDest, int yDest, int iWidth, int iHeight, float h, float s, float v, float a) {
		//	rpaintFore.SetHsva(h,s,v,a);
		//	return DrawRectFilled(xDest,yDest,iWidth,iHeight);
		//}
		//public bool DrawRectFilledHsva(int xDest, int yDest, int iWidth, int iHeight, double h, double s, double v, double a) {
		//	rpaintFore.SetHsva(h,s,v,a);
		//	return DrawRectFilled(xDest,yDest,iWidth,iHeight);
		//}
		//public bool DrawRectFilledCroppedHsva(int xDest, int yDest, int iWidth, int iHeight, double h, double s, double v, double a) {
		//	rpaintFore.SetHsva(h,s,v,a);
		//	return DrawRectFilledCropped(xDest,yDest,iWidth,iHeight);
		//}
		/// <summary>
		/// DrawRectBorder horizontally and vertically with same thickness
		/// </summary>
		/// <param name="rectRect"></param>
		/// <param name="rectHole"></param>
		/// <returns></returns>
		public bool DrawRectBorderHomogeneous(RPaint paintSource, IRect rectRect, IRect rectHole) {//formerly DrawRectBorderSym
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
				bool bTest=DrawRectFilled(paintSource, xNow, yNow, iWidthNow, iHeightNow);//Top full width
				if (!bTest) bGood=false;
				yNow+=rectHole.Height+iHeightNow;
				//would need to change iHeightNow here if asymmetrical
				bTest=DrawRectFilled(paintSource, xNow, yNow, iWidthNow, iHeightNow);//Bottom full width
				if (!bTest) bGood=false;
				yNow-=rectHole.Height;
				iWidthNow=rectHole.X-rectRect.X;
				iHeightNow=rectHole.Height;
				bTest=DrawRectFilled(paintSource, xNow, yNow, iWidthNow, iHeightNow);//Left remaining height
				if (!bTest) bGood=false;
				xNow+=rectHole.Width+iWidthNow;
				//would need to change iWidthNow here if asymmetrical
				bTest=DrawRectFilled(paintSource, xNow, yNow, iWidthNow, iHeightNow);//Right remaining height
				if (!bTest) bGood=false;
			}
			catch (Exception exn) {
				bGood=false;
				string sCallStack=RReporting.StackTraceToLatinCallStack(new System.Diagnostics.StackTrace());
				RReporting.ShowExn(exn,"drawing rectangle border with continuous thickness",sCallStack);
			}
			return bGood;
		} //end DrawRectBorderHomogeneous
		private IRect rectOuterTemp=new IRect();
		private IRect rectInnerTemp=new IRect();
		public bool DrawRectBorder(RPaint paintSource, int xDest, int yDest, int iWidth, int iHeight, int iThick) {
			rectOuterTemp.Set(xDest,yDest,iWidth,iHeight);
			rectInnerTemp.Set(xDest+iThick,yDest+iThick,iWidth-(iThick*2),iHeight-(iThick*2));
			if ((rectInnerTemp.Width<=2) || (rectInnerTemp.Height<=2)) {
				return DrawRectFilled(paintSource,rectOuterTemp);
			}
			else return DrawRectBorderHomogeneous(paintSource,rectOuterTemp, rectInnerTemp);
		}//DrawRectBorder
		public unsafe bool DrawVertLine(int xDest, int yDest, int iPixelCopies) {
			return DrawVertLine(rpaintFore, xDest, yDest, iPixelCopies);
		}
		public unsafe bool DrawVertLine(int xDest, int yDest, int iPixelCopies, bool bBackColor) {
			return DrawVertLine(bBackColor?rpaintBack:rpaintFore, xDest,yDest,iPixelCopies);
		}
		/// <summary>
		/// Draws vertical line FAST.  Uses average of BGR if this image is grayscale
		/// </summary>
		/// <param name="paintSource"></param>
		/// <param name="xDest"></param>
		/// <param name="yDest"></param>
		/// <param name="iPixelCopies"></param>
		/// <returns></returns>
		public unsafe bool DrawVertLine(RPaint paintSource, int xDest, int yDest, int iPixelCopies) {
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				if (this.iBytesPP==4) {
					fixed (byte* lpDest=&byarrData[iDest], lpSrc=paintSource.data32) { //keeps GC at bay
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
				else {
					byte byVal=0;
					if (this.iBytesPP!=3) byVal=RMath.ByRound( ((double)paintSource.R+(double)paintSource.G+(double)paintSource.R) / 3.0 );
					fixed (byte* lpDest=&byarrData[iDest], lpSrc=paintSource.data32Copied64) { //keeps GC at bay
						byte* lpDestLine=lpDest; //ONLY use this since drawing a VERTICAL line
						byte* lpDestNow;
						byte* lpSrcNow;
						int iChan;
						for (int iRel=0; iRel<iPixelCopies; iRel++) {
							lpSrcNow=lpSrc;
							lpDestNow=lpDestLine;
							if (this.iBytesPP==3) {
								for (iChan=0; iChan<this.iBytesPP; iChan++) {
									*lpDestNow = *lpSrcNow;
									lpDestNow++;
									lpSrcNow++;
								}
							}
							else {
								for (iChan=0; iChan<this.iBytesPP; iChan++) {
									*lpDestNow = byVal;
									lpDestNow++;
								}
							}
							lpDestLine+=iStride;
						}
					}
				}//else this is not 32-bit
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing vertical line using fast pointers",String.Format("RImage DrawVertLine(x:{0},y:{1},copies:{2},sender:{3})", xDest, yDest, iPixelCopies) );
					
			}
			return bGood;
		}//DrawVertLine
		public unsafe bool DrawHorzLine(int xDest, int yDest, int iPixelCopies) {
			return DrawHorzLine(rpaintFore, xDest, yDest, iPixelCopies);
		}
		public unsafe bool DrawHorzLine(int xDest, int yDest, int iPixelCopies, bool bBackColor) {
			return DrawHorzLine(bBackColor?rpaintBack:rpaintFore, xDest,yDest,iPixelCopies);
		}
		public unsafe bool DrawHorzLine(RPaint paintSource, int xDest, int yDest, int iPixelCopies) {
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				if (this.iBytesPP==4) {
					fixed (byte* lpDest=&byarrData[iDest], lpSrc=paintSource.data32Copied64) { //keeps GC at bay
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
				else {
					byte byVal=0;
					if (this.iBytesPP!=3) byVal=RMath.ByRound( ((double)paintSource.R+(double)paintSource.G+(double)paintSource.R) / 3.0 );
					fixed (byte* lpDest=&byarrData[iDest], lpSrc=paintSource.data32Copied64) { //keeps GC at bay
						//byte* lpDestLine=lpDest;
						byte* lpDestNow=lpDest;
						byte* lpSrcNow;
						int iChan;
						for (int iRel=0; iRel<iPixelCopies; iRel++) {
							lpSrcNow=lpSrc;
							//lpDestNow=lpDestLine;
							if (this.iBytesPP==3) {
								for (iChan=0; iChan<this.iBytesPP; iChan++) {
									*lpDestNow = *lpSrcNow;
									lpDestNow++;
									lpSrcNow++;
								}
							}
							else {
								for (iChan=0; iChan<this.iBytesPP; iChan++) {
									*lpDestNow = byVal;
									lpDestNow++;
								}
							}
						}
					}
				}//end else not 32-bit
			}
			catch (Exception exn) {
				bGood=false;
				string sCallStack=RReporting.StackTraceToLatinCallStack(new System.Diagnostics.StackTrace());
				RReporting.ShowExn(exn,"drawing horizonal line using fast pointers",String.Format("RImage DrawHorzLine(x:{0},y:{1},copies:{2},sender:{3})", xDest, yDest, iPixelCopies, sCallStack) );
			}
			return bGood;
		} //end DrawHorzLine
		public bool DrawRectFilledSafe(int xDest, int yDest, int iDestW, int iDestH) {
			bool bGood=false;
			if (byarrData!=null) {
				try {
					int xAbs=xDest;
					int yAbs=yDest;
					int xRel=0;
					//int yRel=0;
					int yDestEnder=yDest+iDestH;
					byte byR=rpaintFore.R;
					byte byG=rpaintFore.G;
					byte byB=rpaintFore.B;
					int DestLineNow_StartIndex=XYToLocation(xAbs,yAbs);
					int iDestNow=DestLineNow_StartIndex;
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
								DestLineNow_StartIndex+=iStride;
								iDestNow=DestLineNow_StartIndex;
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
		//public static bool DrawPixel(RImage riDest, int iDestBufferLoc, RImage riSrc, int iSrcBufferLoc, int iDrawMode) {
		//	bool bGood=false;
		//	if (riSrc!=null) {
		//		bGood=DrawPixel(riDest,iDestBufferLoc,riSrc.byarrData,iSrcBufferLoc,riSrc.iBytesPP,iDrawMode);
		//	}
		//	else {
		//		RReporting.ShowErr("Null source image was sent to DrawPixel!","drawing pixel","RImage DrawPixel");
		//	}
		//	return bGood;
		//}
		//public static bool DrawPixel(RImage riDest, int iDestBufferLoc, byte [] riSrc_byarrData, int iSrcBufferLoc, int riSrc_iBytesPP, int iDrawMode) {
		//}
		public static bool DrawPixel(RImage riDest, int iDestBufferLoc, RImage riSrc, int iSrcBufferLoc, int iDrawMode) {
			bool bGood=true;
			int iBytesPPMin=-1;//must start as -1
			float fCookedAlpha=-1.0f;//must start as -1
			try {
				byte[] riSrc_byarrData=riSrc.byarrData;
				byte[] riDest_byarrData=riDest.byarrData;
				switch (iDrawMode) {
					case DrawMode_CopyColor_CopyAlpha:
						iBytesPPMin=(riSrc.iBytesPP<riDest.iBytesPP)?riSrc.iBytesPP:riDest.iBytesPP;
						if (iBytesPPMin>=3) {
							bGood=RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
							if (!bGood) throw new ApplicationException("CopyFast failed (self-generated exception)");
							//TODO: set alpha to 255 if source 24-bit and dest 32-bit (?)
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
					case DrawMode_AlphaColor_KeepDestAlpha:
						if (riSrc.iBytesPP==4&&riDest.iBytesPP>=3) {
							fCookedAlpha=(float)riSrc_byarrData[iSrcBufferLoc+3]/255.0f;
							riDest_byarrData[iDestBufferLoc]=RMath.ByRound(((float)(riSrc_byarrData[iSrcBufferLoc]-riDest_byarrData[iDestBufferLoc]))*fCookedAlpha+riDest_byarrData[iDestBufferLoc]); //B
							iSrcBufferLoc++; iDestBufferLoc++;
							riDest_byarrData[iDestBufferLoc]=RMath.ByRound(((float)(riSrc_byarrData[iSrcBufferLoc]-riDest_byarrData[iDestBufferLoc]))*fCookedAlpha+riDest_byarrData[iDestBufferLoc]); //G
							iSrcBufferLoc++; iDestBufferLoc++;
							riDest_byarrData[iDestBufferLoc]=RMath.ByRound(((float)(riSrc_byarrData[iSrcBufferLoc]-riDest_byarrData[iDestBufferLoc]))*fCookedAlpha+riDest_byarrData[iDestBufferLoc]); //R
						}
						else {
							RReporting.ShowErr("Cannot use "+DrawMode_ToString(iDrawMode)+" overlay mode with images that are not true color or when source is not 32-bit.","","DrawPixel");
						}
						break;
					case DrawMode_AlphaQuickEdgeColor_KeepDestAlpha:
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
							RReporting.ShowErr("Cannot use "+DrawMode_ToString(iDrawMode)+" overlay mode with images that are not true color or when source is not 32-bit.","","DrawPixel");
						}
						break;
					case DrawMode_AlphaHardEdgeColor_KeepDestAlpha:
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
							RReporting.ShowErr("Cannot use "+DrawMode_ToString(iDrawMode)+" overlay mode with images that are not true color or when source is not 32-bit.","","DrawPixel");
						}
						break;
					case DrawMode_AlphaColor_KeepGreaterAlpha:
						if (riSrc.iBytesPP>=4&&riDest.iBytesPP>=3) {
							riDest_byarrData[iDestBufferLoc]=(riSrc_byarrData[iSrcBufferLoc]>riDest_byarrData[iDestBufferLoc])?riSrc_byarrData[iSrcBufferLoc]:riDest_byarrData[iDestBufferLoc]; //B
							iSrcBufferLoc++; iDestBufferLoc++;
							riDest_byarrData[iDestBufferLoc]=(riSrc_byarrData[iSrcBufferLoc]>riDest_byarrData[iDestBufferLoc])?riSrc_byarrData[iSrcBufferLoc]:riDest_byarrData[iDestBufferLoc]; //G
							iSrcBufferLoc++; iDestBufferLoc++;
							riDest_byarrData[iDestBufferLoc]=(riSrc_byarrData[iSrcBufferLoc]>riDest_byarrData[iDestBufferLoc])?riSrc_byarrData[iSrcBufferLoc]:riDest_byarrData[iDestBufferLoc]; //R
						}
						else {
							RReporting.ShowErr("Cannot use mode "+DrawMode_ToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","","DrawPixel");
						}
						break;
					case DrawMode_CopyColor_KeepDestAlpha:
						//if (riSrc.iBytesPP>=4&&riDest.iBytesPP>=3) {
						//	iBytesPPMin=(riSrc.iBytesPP<riDest.iBytesPP)?riSrc.iBytesPP:riDest.iBytesPP;
						//	if (iBytesPPMin>=4) iBytesPPMin=3;
						//	RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData,iDestPix,iSrcPix,iBytesPPMin);
						//else {
						//	RReporting.ShowErr("Cannot use mode "+DrawMode_ToString(iDrawMode)+" with images that are not true color or when source is not 32-bit.","","DrawPixel");
						//}
						//break;
						iBytesPPMin=(riSrc.iBytesPP<riDest.iBytesPP)?riSrc.iBytesPP:riDest.iBytesPP;
						if (iBytesPPMin>=3) {
							if (iBytesPPMin>3) iBytesPPMin=3;//since DrawMode_CopyColor_KeepDestAlpha
							bGood=RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
						}
						else {
							if (riDest.iBytesPP==4&&riSrc.iBytesPP==1) {	
								RReporting.ShowErr("Cannot use mode "+DrawMode_ToString(iDrawMode)+" when source is an alpha mask and destination is 32-bit.","","DrawPixel");
								//riDest_byarrData[iDestBufferLoc+3]=riSrc_byarrData[iSrcBufferLoc];
							}
							else if (riDest.iBytesPP==1) {//&&riSrc.iBytesPP==4) {
								RReporting.ShowErr("Cannot use mode "+DrawMode_ToString(iDrawMode)+" when destination is an alpha mask.","DrawPixel");
								//riDest_byarrData[iDestBufferLoc]=riSrc_byarrData[iSrcBufferLoc+3];
							}
							else {
								if (iBytesPPMin>3) iBytesPPMin=3;//since DrawMode_CopyColor_KeepDestAlpha
								bGood=RMemory.CopyFast(ref riDest_byarrData, ref riSrc_byarrData, iDestBufferLoc, iSrcBufferLoc, iBytesPPMin);
							}
						}
						break; //end DrawMode_CopyColor_KeepDestAlpha
					default:
						RReporting.Warning("DrawPixel mode "+iDrawMode.ToString()+" is not implemented");
						bGood=false;
						break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing pixel at buffer location sensitive to DrawMode_","DrawPixel {riDest:"+(riDest!=null?riDest.Description():"null")
						+"; riSrc:"+(riSrc!=null?riSrc.Description():"null")
						+"; iDestBufferLoc:"+iDestBufferLoc.ToString()
						+"; iSrcBufferLoc:"+iSrcBufferLoc.ToString()
						+"; iDrawMode:"+DrawMode_ToString(iDrawMode)
						+"; iBytesPPMin:"+((iBytesPPMin!=-1)?iBytesPPMin.ToString():"unused")
						+"; fCookedAlpha:"+((fCookedAlpha!=-1.0f)?fCookedAlpha.ToString():"unused")
						+"}"
					);
			}
			return bGood;
		}//end DrawPixel

		#endregion Pixel-based Geometry Raster Drawing
		
		

		#region Vector Geometry Drawing
		public void DrawVectorDot(float xDot, float yDot) {
			//bool bGood=true;
			try {
				//TODO: finish this (finish Vector accuracy)
				// Begin header fields in order of writing //
				//Targa struct reference:
				//bySizeofID byMapType byTgaType wMapOrigin wMapLength byMapBitDepth
				//xImageOrigin yImageOrigin width height byBitDepth bitsDescriptor sTag
				//*byarrColorMap *byarrData footer;
				//LPIPOINT *lpipointarrNow=malloc(4*sizeof(LPIIPOINT));
	
				//+1 is okay even when exact since that would result in xEccentric==0 and yEccentric==0
				int xMin=(int)xDot;//FLOOR
				int xMax=xMin+1;//(int)System.Math.Ceiling(xDot);//TODO: make sure 1 doesn't return 2???
				int yMin=(int)yDot;//FLOOR
				int yMax=yMin+1;//(int)System.Math.Ceiling(yDot);//TODO: make sure 1 doesn't return 2???
				float xfMin=(float)xMin;
				float xfMax=(float)xMax;
				float yfMin=(float)yMin;
				float yfMax=(float)yMax;
				//float xfMin=RMath.Floor(xDot);
				//float xfMax=xfMin+1.0f;
				//float yfMin=RMath.Floor(yDot);
				//float yfMax=yfMin+1.0f;
				////int iBytesPP=byBitDepth/8;
				////int iStride=iWidth*iBytesPP;
				////int iStart=yMin*iStride+xMin*iBytesPP;
				float xEccentric,yEccentric,xNormal,yNormal;
				xNormal=1.0f-RMath.SafeAbs(xDot-xfMin);
				xEccentric=1.0f-RMath.SafeAbs(xDot-xfMax);
				yNormal=1.0f-RMath.SafeAbs(yDot-yfMin);
				yEccentric=1.0f-RMath.SafeAbs(yDot-yfMax);
				//TODO:(?) add fast version where on column and(on pixel)/or row?
				float rpaintFore_fA=(float)rpaintFore.A;
				DrawAlphaPix(xMin,yMin,rpaintFore.R,rpaintFore.G,rpaintFore.B,RMath.ByRound(rpaintFore_fA*xNormal*yNormal));
				DrawAlphaPix(xMax,yMin,rpaintFore.R,rpaintFore.G,rpaintFore.B,RMath.ByRound(rpaintFore_fA*xEccentric*yNormal));
				DrawAlphaPix(xMin,yMax,rpaintFore.R,rpaintFore.G,rpaintFore.B,RMath.ByRound(rpaintFore_fA*xNormal*yEccentric));
				DrawAlphaPix(xMax,yMax,rpaintFore.R,rpaintFore.G,rpaintFore.B,RMath.ByRound(rpaintFore_fA*xEccentric*yEccentric));
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing rpaintFore as Vector Dot at subpixel location","DrawVectorDot");
			}
		}//end DrawVectorDot
		public void DrawVectorDot(float xDot, float yDot, Pixel32 pixelColor) {
			//bool bGood=true;
			try {
				//TODO: finish this (finish Vector accuracy)
				// Begin header fields in order of writing //
				//Targa struct reference:
				//bySizeofID byMapType byTgaType wMapOrigin wMapLength byMapBitDepth
				//xImageOrigin yImageOrigin width height byBitDepth bitsDescriptor sTag
				//*byarrColorMap *byarrData footer;
				//LPIPOINT *lpipointarrNow=malloc(4*sizeof(LPIIPOINT));
	
				//TODO: is this a problem when floor would be same??? since changed to +1 for max; see below
				int xMin=(int)xDot;//FLOOR
				int xMax=xMin+1;//(int)System.Math.Ceiling(xDot);//TODO: make sure 1 doesn't return 2???
				int yMin=(int)yDot;//FLOOR
				int yMax=yMin+1;//(int)System.Math.Ceiling(yDot);//TODO: make sure 1 doesn't return 2???
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
				//TODO:(?) add case where on column and(on pixel)/or row?
				DrawAlphaPix(xMin,yMin,pixelColor.R,pixelColor.G,pixelColor.B,RMath.ByRound(pixelColor.A*xNormal*yNormal));
				DrawAlphaPix(xMax,yMin,pixelColor.R,pixelColor.G,pixelColor.B,RMath.ByRound(pixelColor.A*xEccentric*yNormal));
				DrawAlphaPix(xMin,yMax,pixelColor.R,pixelColor.G,pixelColor.B,RMath.ByRound(pixelColor.A*xNormal*yEccentric));
				DrawAlphaPix(xMax,yMax,pixelColor.R,pixelColor.G,pixelColor.B,RMath.ByRound(pixelColor.A*xEccentric*yEccentric));
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"drawing Pixel32 as Vector Dot at subpixel location","DrawVectorDot");
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
				RReporting.ShowExn(exn,"drawing array as Vector Dot at subpixel location","DrawVectorDot");
			}
		}//end DrawVectorDot(float xDot, float yDot, byte* lpbySrcPixel)
		public void DrawVectorLine(ILine line1, Pixel32 pixelStart, Pixel32 pixelEndOrNull, float fSubpixelPrecisionIncrement) {
			DrawVectorLine(RConvert.ToFloat(line1.X1), RConvert.ToFloat(line1.Y1), RConvert.ToFloat(line1.X2), RConvert.ToFloat(line1.Y2), pixelStart, pixelEndOrNull, fSubpixelPrecisionIncrement);
		}
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
		
		#endregion Vector Geometry Drawing

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
				RReporting.ShowExn(exn,"drawing fractal (double-precision version)","RImage DrawFractal");
			}
		}
		*/
		
		#region Advanced Vector Graphics Drawing 
///TODO: try these in RetroEngineTest
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
				//double rSpeed;
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
				RReporting.ShowExn(exn,"drawing fractal rainbow burst","DrawRainbowBurst");
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
				//double rSpeed;
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
				RReporting.ShowExn(exn,"drawing wavy fractal","DrawWavyThing");
			}
			return bGood;
		}//end DrawWavyThing
		
		#endregion Advanced Vector Graphics Drawing

		#region Effects
		public static bool Invert(RImage riDest, int xAt, int yAt, int iSetWidth, int iSetHeight) {//TODO: move to rtextbox //formerly InvertRect
			bool bGood=false;
			if (RImage.CropDestRect(ref xAt, ref yAt, ref iSetWidth, ref iSetHeight,riDest)) {
				int xAtRow=xAt;
				int yRel=0;
				int xRel=0;
				int xAbs=xAt;
				int yAbs=yAt;
				//Color colorNow;
				try {
					for (yRel=0; yRel<iSetHeight; yRel++) {
						xAbs=xAtRow;
						for (xRel=0; xRel<iSetWidth; xRel++) {
							riDest.InvertPixel(xAbs,yAbs);
							//colorNow=riDest.GetPixel(xAbs,yAbs);
							//riDest.SetPixel(xAbs,yAbs,Color.FromArgb(255,255-colorNow.R,255-colorNow.G,255-colorNow.B));
							//RReporting.WriteLine("("+colorNow.R.ToString()+","+colorNow.G.ToString()+","+colorNow.B.ToString()+")");
							xAbs++;
						}
						yAbs++;
					}
					bGood=true;
				}
				catch (Exception exn) {
					bGood=false;
					RReporting.ShowExn(exn,"inverting color inside rectangle","rform Invert {"
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
		}//end Invert
		public static bool Invert(Bitmap bmpNow, int xAt, int yAt, int iSetWidth, int iSetHeight) {//TODO: move to rtextbox //formerly InvertRect
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
						//RReporting.WriteLine("("+colorNow.R.ToString()+","+colorNow.G.ToString()+","+colorNow.B.ToString()+")");
						xAbs++;
					}
					yAbs++;
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"inverting color inside rectangle","rform Invert(Bitmap) {"
					+"xAt:"+xAt.ToString()+"; "
					+"yAt:"+yAt.ToString()+"; "
					+"xAbs:"+xAbs.ToString()+"; "
					+"yAbs:"+yAbs.ToString()+"; "
					+"iSetWidth:"+iSetWidth.ToString()+"; "
					+"iSetHeight:"+iSetHeight.ToString()+"; "
					+"}");
			}
			return bGood;
		}//end Invert
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
				RReporting.ShowExn(exn,"creating mask from channel","RImage MaskFromChannel "
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
		}// MaskFromChannel
		public static bool MaskFromValue(ref RImage riDest, ref RImage riSrc) {
			int iDestByte=0;
			int iSrcByte=0;
			int iPixels=0;
			int iBytesPPOffset=0;
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
				RReporting.ShowExn(exn,"creating mask"+((riDest!=null&riSrc!=null)?" (while attempting to copy as 8-, 24-, or 32-bit)":" (with null image(s)) "), "MaskFromValue "
				   +"{"
					//+"  "+"iByteInPixel:"+iByteInPixel.ToString()
					//+"; iDestCharPitch:"+iDestCharPitch.ToString()
					//+"; iChar1:"+iChar1.ToString()
					//+"; iCharNow:"+iCharNow.ToString()
					//+"; yNow:"+yNow.ToString()
					//+"; xNow:"+xNow.ToString()
					+"; riDest:"+(riDest!=null?riDest.Description():"null")
					+"; riSrc:"+(riSrc!=null?riSrc.Description():"null")
					//if (riSrc!=null)
					//	+"; riSrc.iBytesPP:"+riSrc.iBytesPP.ToString()
					+"; iPixels:"+iPixels.ToString()
					+"; iBytesPPOffset:"+iBytesPPOffset.ToString()
					+"; iSrcByte:"+iSrcByte.ToString()
					+"; iDestByte:"+iDestByte.ToString() +"}");
				return false;
			}
			return true;
		}//end MaskFromValue
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
			//int DestLineNow_StartIndex;
			//double xDirection;
			double dHeight;
			double yNow;
			double dWidthDest;
			//double dWidthSrc;
			double xNow;
			double xAdd;
			double yMaxSrc;
			int iDestIndex;
			DPoint dpSrc;
			//int iDestByte;
			//int iSrcByte;
			if (xOffsetBottom<0) {
				//xDirection=-1;
				xAdd=(double)(xOffsetBottom*-1);
			}
			else {
				//xDirection=1;
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
				//iSrcByte=0;
				//iDestByte=0;//iDestByteStart;//TODO: Uncomment, and separate the blur code here and make alpha overlay version
				bool bTest=true;
				//DestLineNow_StartIndex=0;
				dpSrc=new DPoint();
				dpSrc.Y=0;
				dHeight=(double)riDest.iHeight;
				dWidthDest=(double)riDest.iWidth;
				//dWidthSrc=(double)riSrc.iWidth;
				yMaxSrc=dHeight-1.0d;
				iDestIndex=0;
				for (yNow=0; yNow<dHeight; yNow+=1.0d) {
					dpSrc.X=(yNow/yMaxSrc)*xAdd;
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
					//DestLineNow_StartIndex+=riDest.iStride;
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
		
		/// <summary>
		/// Keeps the greater value for each byte
		/// </summary>
		/// <param name="byarrDest"></param>
		/// <param name="byarrSrc"></param>
		/// <param name="iDestByte"></param>
		/// <param name="iSrcByte"></param>
		/// <param name="iBytes"></param>
		/// <returns></returns>
		public static bool EffectLightenOnly(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			try {
				for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
					if (byarrSrc[iSrcByte]>byarrDest[iDestByte]) byarrDest[iDestByte]=byarrSrc[iSrcByte];
					iDestByte++;
					iSrcByte++;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"copying channels to destination only where source has higher value","RImage EffectLightenOnly");
				return false;
			}
			return true;
		}
		
		/// <summary>
		/// Keeps the greater value for each byte, multiplying source by fMultiplySrc
		/// </summary>
		/// <param name="byarrDest"></param>
		/// <param name="byarrSrc"></param>
		/// <param name="iDestByte"></param>
		/// <param name="iSrcByte"></param>
		/// <param name="iBytes"></param>
		/// <param name="fMultiplySrc"></param>
		/// <returns></returns>
		public static bool EffectLightenOnly(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes, float fMultiplySrc) {
			if (fMultiplySrc>1.0f) fMultiplySrc=1.0f;
			byte bySrc;
			float fVal;
			try {
				for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
					fVal=((float)byarrSrc[iSrcByte]*fMultiplySrc);
					if (fVal>255.0) fVal=255;
					bySrc=RMath.ByRound(fVal);
					if (bySrc>byarrDest[iDestByte]) byarrDest[iDestByte]=bySrc;
					iDestByte++;
					iSrcByte++;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"copying channels to destination only where source modified by multiplier has higher value","RImage EffectLightenOnly");
				return false;
			}
			return true;
		}
		#endregion Effects		
		public static int PixelFormatToBytesPP(PixelFormat pixelformat) {
			int iReturn=PixelFormatToBitsPP(pixelformat)/8;
			if (iReturn==0) {
				RReporting.Warning("0 bytes per pixel returned for "+PixelFormatToBitsPP(pixelformat).ToString()+"-bit pixel format! {PixelFormat:"+pixelformat.ToString()+"}");
			}
			return iReturn;
		}
		public static int PixelFormatToBitsPP(PixelFormat pixelformat) {
			int iReturn=0;
			if (pixelformat==PixelFormat.Format16bppArgb1555) iReturn=16;
			else if (pixelformat==PixelFormat.Format16bppGrayScale) iReturn=16;
			else if (pixelformat==PixelFormat.Format16bppRgb555) iReturn=16;
			else if (pixelformat==PixelFormat.Format16bppRgb565) iReturn=16;
			else if (pixelformat==PixelFormat.Format1bppIndexed) iReturn=1; //1/8 bytes per pixel
			else if (pixelformat==PixelFormat.Format24bppRgb) iReturn=24;
			else if (pixelformat==PixelFormat.Format32bppArgb) iReturn=32;
			else if (pixelformat==PixelFormat.Format32bppPArgb) iReturn=32;
			else if (pixelformat==PixelFormat.Format32bppRgb) iReturn=32;
			else if (pixelformat==PixelFormat.Format48bppRgb) iReturn=48;
			else if (pixelformat==PixelFormat.Format4bppIndexed) iReturn=4; //.5 bytes per pixel
			else if (pixelformat==PixelFormat.Format64bppArgb) iReturn=64;
			else if (pixelformat==PixelFormat.Format64bppPArgb) iReturn=64;
			else if (pixelformat==PixelFormat.Format8bppIndexed) iReturn=8;
			return iReturn;
		}
		#region DrawAs other image type
		public bool DrawAs(Bitmap bmpDest, PixelFormat pixelformatDest_MustMatchDest) {//, Graphics Graphics_FromImage_bmpDest) {
			bool bGood=true;
			Exception exn2=null;
			//Pen penNow=new Pen();
			//Color colorNow=new Color();
			
			BitmapData bmpdataDest=null;
			int y=0, x=0;
			//int iLineStart=0;
			int iSrcLineStartNow=int.MinValue;
			int iExceptions=0;
			try {
				if (bmpDest==null) throw new ApplicationException("destination bitmap is NULL");
				if (byarrData==null) throw new ApplicationException("source RImage has NULL BUFFER");
				if (bmpDest.Width!=Width||bmpDest.Height!=Height) throw new ApplicationException("source RImage has DIFFERENT IMAGE SIZE");
				bmpdataDest=bmpDest.LockBits(new Rectangle(0, 0,
											bmpDest.Width, bmpDest.Height),
											ImageLockMode.WriteOnly, ///WRITE ONLY MODE
											pixelformatDest_MustMatchDest);
				int riDest_iBytesPP=RImage.PixelFormatToBytesPP(pixelformatDest_MustMatchDest);
				//debug performance--try: using(Graphics gNow = Graphics.FromImage(bmpDest)) {//do drawing here}
				
				int iMinW=bmpDest.Width<Width?bmpDest.Width:Width;
				int iMinH=bmpDest.Height<Height?bmpDest.Height:Height;
				if (this.iBytesPP==riDest_iBytesPP) {
					int iMinStride=iStride<bmpdataDest.Stride?iStride:bmpdataDest.Stride;
					unsafe {
						fixed (byte* bypSrc=byarrData) {
							//fixed (IntPtr bypDest=bmpdataDest.Scan0) {
								byte* bypSrcNow=bypSrc;
								byte* bypDestLine=(byte*)bmpdataDest.Scan0;//NOTE: bmpdataDest.Scan0 is already fixed
								iSrcLineStartNow=0;
								for (y=0; y<iMinH; y++) {
									try {
										RMemory.Copy(bypDestLine,bypSrcNow,iMinStride);
										bypDestLine+=bmpdataDest.Stride;
										bypSrcNow+=iStride;
										iSrcLineStartNow+=iStride;
									}
									catch (Exception exn) { exn2=exn; iExceptions++;}
									//iLineStart+=iStride;
								}//end for y
							//}//end fixed dest *
						}//end fixed source *
					}//end unsafe
				}
				else {
					unsafe {
						fixed (byte* bypSrc=byarrData) {
							//fixed (IntPtr bypDest=bmpdataDest.Scan0) {
								byte* bypSrcNow;
								byte* bypSrcLine=bypSrc;
								byte* bypDestNow;
								byte* bypDestLine=(byte*)bmpdataDest.Scan0;//NOTE: bmpdataDest.Scan0 is already fixed
								int iChanRel;
								iSrcLineStartNow=0;
								//int riDest_LastChannel=riDest_iBytesPP-1;
								int riSrc_LastChannel=iBytesPP-1;
								int riSrc_ChannelJumpNowLastToNextFirst=iBytesPP-riDest_iBytesPP;
								if (riSrc_ChannelJumpNowLastToNextFirst<1) riSrc_ChannelJumpNowLastToNextFirst=1;
								//int iDestChannelsRemainder=riDest_iBytesPP-iBytesPP;
								//if (iDestChannelsRemainder<0) iDestChannelsRemainder=0;
								//TODO: fix channel overstep problem with src3 dest4
								for (y=0; y<iMinH; y++) {
									bypSrcNow=bypSrcLine;
									bypDestNow=bypDestLine;
									//	RMemory.Copy(bypDest,bypSrcNow,iMinStride);
									//	bypSrcNow+=iStride;
									for (x=0; x<Width; x++) {
										try {
											for (iChanRel=0; iChanRel<riDest_iBytesPP; iChanRel++) {
												if (iChanRel==3&&riDest_iBytesPP==4&&iBytesPP==3) *bypDestNow=255;
												else *bypDestNow=*bypSrcNow;
												bypDestNow++;
												if (iChanRel<riSrc_LastChannel) bypSrcNow++;
											}
										}
										catch (Exception exn) { exn2=exn; iExceptions++;}
										//iLineStart+=iStride;
										bypSrcNow+=riSrc_ChannelJumpNowLastToNextFirst;
									}
									bypDestLine+=bmpdataDest.Stride;
									bypSrcLine+=iStride;
									iSrcLineStartNow+=iStride;
								}//end for y
							//}//end fixed dest *
						}//end fixed source *
					}//end unsafe
				}//else iBytesPP!=4
				
			}
			catch (Exception exn) { exn2=exn; iExceptions++;}
			if (exn2!=null) {
				bGood=false;
				RReporting.ShowExn(exn2,"drawing RImage to Bitmap object using fast pointers","RImage DrawAs(Bitmap) {x:"+x.ToString()+"; y:"+y.ToString()+"; iSrcLineStartNow:"+iSrcLineStartNow.ToString()+"; iSrcLineStartNow/iBytesPP:"+(iSrcLineStartNow/iBytesPP).ToString()+"; "+DumpStyle(false)+"; iExceptions:"+iExceptions.ToString()+"}");
			}
			try {if (bmpDest!=null) bmpDest.UnlockBits(bmpdataDest);}
			catch {}
			return bGood;
		}//end DrawAs
		
		/// <summary>
		/// Safe version does NOT use RMemory copy method.
		/// </summary>
		/// <param name="bmpDest"></param>
		/// <returns></returns>
		public bool DrawAsSafe(Bitmap bmpDest) {
			bool bGood=true;
			Exception exn2=null;
			//Pen penNow=new Pen();
			Color colorNow=new Color();
			int y=0, x=0;
			int iLineStart=0;
			int iSrc=int.MinValue;
			int iExceptions=0;
			try {
				if (bmpDest==null) throw new ApplicationException("destination bitmap is null");
				if (byarrData==null) throw new ApplicationException("source RImage has null buffer");
				//debug performance--try:
				//using(Graphics gNow = Graphics.FromImage(bmpDest)) {//do drawing here}
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
				RReporting.ShowExn(exn2,"drawing RImage to Bitmap object","RImage DrawAs(Bitmap){x:"+x.ToString()+"; y:"+y.ToString()+"; iSrc:"+iSrc.ToString()+"; "+DumpStyle(false)+"; iExceptions:"+iExceptions.ToString()+"}");
			}
			return bGood;
		}//end DrawAsSafe
		
/*		public bool DrawAs(Graphics gDest) { //SLOW
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
				//RReporting.ShowExn(exn2,"drawing RImage to Graphics object","RImage DrawAs(Graphics) {x:"+x.ToString()+"; y:"+y.ToString()+"; iSrc:"+iSrc.ToString()+"; "+DumpStyle(false)+"; iExceptions:"+iExceptions.ToString()+"}");
			}
			return bGood;
		}//end DrawAs
*/	
		
		#endregion DrawAs other image type
	}//end class RImage

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////// RIMageStack /////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public class RImageStack { //pack Stack -- array, order left(First) to right(Last) //formerly GBufferStack
		private RImage[] riarr=null;
		private int Maximum {
			get {
				return (riarr==null)?0:riarr.Length;
			}
			set {
				RImage.Redim(ref riarr,value);
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
		public bool Push(RImage rimageAdd) {
			//if (!IsFull) {
			try {
				if (rimageAdd!=null) {
					if (NewIndex>=Maximum) SetFuzzyMaximumByLocation(NewIndex);
					riarr[NewIndex]=rimageAdd;
					iCount++;
				}
				else RReporting.ShowErr("Cannot push a null RImage to a stack.","pushing RImage to stack","RImageStack Push");
				//sLogLine="debug enq iCount="+iCount.ToString();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"stacking image","RImageStack Push("+((rimageAdd==null)?"null RImage":"non-null")+"){new-location:"+NewIndex.ToString()+"}");
				return false;
			}
			return true;
			//}
			//else {
			//	RReporting.ShowErr("RImageStack is full, can't push \""+rimageAdd+"\"! ( "+iCount.ToString()+" RImages already used)","","RImageStack Push("+((rimageAdd==null)?"null RImage":"non-null")+")");
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
				RReporting.ShowExn(exn,"getting references to stacked images","RImageStack ToArrayByReferences");
			}
			return riarrReturn;
		}
	}//end RImageStack created 2007-10-03
	public class ROverlay {
		public const int ColorDestMode_AlphaBlend		= 0;
		public const int ColorDestMode_AlphaQuickEdge	= 1;
		public const int ColorDestMode_AlphaHardEdge	= 2;
		public const int ColorDestMode_Copy				= 3;
		
		public const int AlphaDestMode_KeepGreater		= 0;
		public const int AlphaDestMode_KeepDestAlpha	= 1;
		public const int AlphaDestMode_AlphaBlend		= 2;
		public const int AlphaDestMode_Copy				= 3;
		
		/// <summary>
		/// NOTE: GrayToColor_ constants are used as byte offset if less than 4
		/// Ignored channels are left same if buffer is initialized,
		/// but left as 0 if was uninitialized and receiving method initializes buffers.
		/// </summary>
		public const int GrayToColor_ToB_IgnoreOtherChannels=0;
		public const int GrayToColor_ToG_IgnoreOtherChannels=1;
		public const int GrayToColor_ToR_IgnoreOtherChannels=2;
		public const int GrayToColor_ToA_IgnoreOtherChannels=3;
		public const int GrayToColor_ToA_SetOtherChannelsTo255=4;
		public const int GrayToColor_ToAllNonAlphaColorChannels_IgnoreA=5;
		public const int GrayToColor_ToAllNonAlphaColorChannels_SetATo255=6;
		public const int GrayToColor_ToAllColorsAndAlpha=7;
		/// <summary>
		/// NOTE: ColorToGray_ constants are used as byte offset if less than 4
		/// </summary>
		public const int ColorToGray_B=0;
		public const int ColorToGray_G=1;
		public const int ColorToGray_R=2;
		public const int ColorToGray_A=3;
		public const int ColorToGray_AverageColor=4;
		
		public int AlphaDestMode;
		public int ColorDestMode;
		public int GrayToColorMode;
		public int ColorToGrayMode;
		public bool NeedsSourceAlpha {
			get {
				return (
					ColorDestMode!=ROverlay.ColorDestMode_Copy
					||AlphaDestMode!=ROverlay.AlphaDestMode_KeepDestAlpha
				);
			}
		}
		public ROverlay() {
			From(ColorDestMode_AlphaBlend,AlphaDestMode_KeepGreater);
		}
		public ROverlay(int AffectDestColorNow, int AffectDestAlphaNow) {
			From(AffectDestColorNow,AffectDestAlphaNow);
		}
		public static ROverlay CopyChannels() {
			return new ROverlay(ROverlay.ColorDestMode_Copy,ROverlay.AlphaDestMode_Copy);
		}
		public static int ToAlphaDestMode(string sVarName) {
			int iReturn=-1;
			if (sVarName=="AlphaDestMode_KeepGreater") iReturn=AlphaDestMode_KeepGreater;
			else if (sVarName=="AlphaDestMode_KeepDestAlpha") iReturn=AlphaDestMode_KeepDestAlpha;
			else if (sVarName=="AlphaDestMode_AlphaBlend") iReturn=AlphaDestMode_AlphaBlend;
			else if (sVarName=="AlphaDestMode_Copy") iReturn=AlphaDestMode_Copy;
			return iReturn;
		}//end ToAlphaDestMode
		public static int ToColorDestMode(string sVarName) {
			int iReturn=-1;
			if (sVarName=="ColorDestMode_AlphaBlend") iReturn=ColorDestMode_AlphaBlend;
			else if (sVarName=="ColorDestMode_AlphaQuickEdge") iReturn=ColorDestMode_AlphaQuickEdge;
			else if (sVarName=="ColorDestMode_AlphaHardEdge") iReturn=ColorDestMode_AlphaHardEdge;
			else if (sVarName=="ColorDestMode_Copy") iReturn=ColorDestMode_Copy;
			return iReturn;
		}//end ToColorDestMode
		public void From(int AffectDestColorNow, int AffectDestAlphaNow) {
			ColorDestMode=AffectDestColorNow;
			AlphaDestMode=AffectDestAlphaNow;
		}

		public void FromLegacyDrawMode(int iDrawMode) {
			AlphaDestMode=AlphaDestMode_KeepDestAlpha;
			if (iDrawMode==RImage.DrawMode_CopyColor_CopyAlpha) {
				AlphaDestMode=AlphaDestMode_Copy;
			}
			else if (iDrawMode==RImage.DrawMode_AlphaColor_KeepGreaterAlpha) {
				AlphaDestMode=AlphaDestMode_KeepGreater;
			}
			//UNUSED in DrawMode version: AlphaDestMode_AlphaBlend;
			
			ColorDestMode=ROverlay.ColorDestMode_AlphaBlend;
			//if (iDrawMode==RImage.DrawMode_AlphaColor_KeepDestAlpha
			//	||iDrawMode==RImage.DrawMode_AlphaColor_KeepGreaterAlpha) ColorDestMode=ROverlay.ColorDestMode_AlphaBlend;
			if (iDrawMode==RImage.DrawMode_CopyColor_CopyAlpha
				||iDrawMode==RImage.DrawMode_CopyColor_KeepDestAlpha) ColorDestMode=ROverlay.ColorDestMode_Copy;
			else if (iDrawMode==RImage.DrawMode_AlphaQuickEdgeColor_KeepDestAlpha) ColorDestMode=ROverlay.ColorDestMode_AlphaQuickEdge;
			else if (iDrawMode==RImage.DrawMode_AlphaHardEdgeColor_KeepDestAlpha) ColorDestMode=ROverlay.ColorDestMode_AlphaHardEdge;
		}
		/// <summary>
		/// Converts GrayToColor using overlay object 
		/// </summary>
		/// <param name="paintReturn">If non-null, values must be set to something already since certain channels could be skipped.  Created and starts at argb(0,0,0,0) if null.</param>
		/// <param name="byGrayValue">The gray value to convert to color</param>
		/// <param name="GrayHandling_Overlay_OrNull">options for converting gray to color.  All channels will be set to byGrayValue if null.</param>
		public static void GrayToColor(ref RPaint paintReturn, byte byGrayValue, ROverlay GrayHandling_Overlay_OrNull) {
			byte a,r,g,b;
			if (paintReturn!=null) {
				a=paintReturn.A; r=paintReturn.R; g=paintReturn.G; b=paintReturn.B;
			}
			else {
				a=0; r=0; g=0; b=0;
			}
			if (GrayHandling_Overlay_OrNull!=null) {
				switch (GrayHandling_Overlay_OrNull.GrayToColorMode) {
					case ROverlay.GrayToColor_ToA_IgnoreOtherChannels:
						a=byGrayValue; break;
					case ROverlay.GrayToColor_ToA_SetOtherChannelsTo255:
						a=byGrayValue; r=255; g=255; b=255; break;
					case ROverlay.GrayToColor_ToAllColorsAndAlpha:
						a=byGrayValue; r=byGrayValue; g=byGrayValue; b=byGrayValue; break;
					case ROverlay.GrayToColor_ToAllNonAlphaColorChannels_IgnoreA:
						r=byGrayValue; g=byGrayValue; b=byGrayValue; break;
					case ROverlay.GrayToColor_ToAllNonAlphaColorChannels_SetATo255:
						a=255; r=byGrayValue; g=byGrayValue; b=byGrayValue; break;
					case ROverlay.GrayToColor_ToB_IgnoreOtherChannels:
						b=byGrayValue; break;
					case ROverlay.GrayToColor_ToG_IgnoreOtherChannels:
						g=byGrayValue; break;
					case ROverlay.GrayToColor_ToR_IgnoreOtherChannels:
						r=byGrayValue; break;
					default:break;
				}//end switch GrayHandling_Overlay_OrNull.GrayToColorMode
			}
			else {//default to GrayToColor_ToAllColorsAndAlpha
				a=byGrayValue; r=byGrayValue; g=byGrayValue; b=byGrayValue;
			}
			if (paintReturn==null) paintReturn=RPaint.FromArgb(a,r,g,b);
			else paintReturn.SetArgb(a,r,g,b);
		}//end GrayToColor(RPaint,...)
		/// <summary>
		/// Converts GrayToColor using overlay object 
		/// </summary>
		/// <param name="a">Must be set to something already since certain channels could be skipped.</param>
		/// <param name="r">Must be set to something already since certain channels could be skipped.</param>
		/// <param name="g">Must be set to something already since certain channels could be skipped.</param>
		/// <param name="b">Must be set to something already since certain channels could be skipped.</param>
		/// <param name="byGrayValue">The gray value to convert to color</param>
		/// <param name="GrayHandling_Overlay_OrNull">options for converting gray to color.  All channels will be set to byGrayValue (GrayToColor_ToAllColorsAndAlpha option will be used) if null.</param>
		public static void GrayToColorArgb(ref byte a, ref byte r, ref byte g, ref byte b, byte byGrayValue, ROverlay GrayHandling_Overlay_OrNull) {
			if (GrayHandling_Overlay_OrNull!=null) {
				switch (GrayHandling_Overlay_OrNull.GrayToColorMode) {
					case ROverlay.GrayToColor_ToA_IgnoreOtherChannels:
						a=byGrayValue; break;
					case ROverlay.GrayToColor_ToA_SetOtherChannelsTo255:
						a=byGrayValue; r=255; g=255; b=255; break;
					case ROverlay.GrayToColor_ToAllColorsAndAlpha:
						a=byGrayValue; r=byGrayValue; g=byGrayValue; b=byGrayValue; break;
					case ROverlay.GrayToColor_ToAllNonAlphaColorChannels_IgnoreA:
						r=byGrayValue; g=byGrayValue; b=byGrayValue; break;
					case ROverlay.GrayToColor_ToAllNonAlphaColorChannels_SetATo255:
						a=255; r=byGrayValue; g=byGrayValue; b=byGrayValue; break;
					case ROverlay.GrayToColor_ToB_IgnoreOtherChannels:
						b=byGrayValue; break;
					case ROverlay.GrayToColor_ToG_IgnoreOtherChannels:
						g=byGrayValue; break;
					case ROverlay.GrayToColor_ToR_IgnoreOtherChannels:
						r=byGrayValue; break;
					default:break;
				}//end switch GrayHandling_Overlay_OrNull.GrayToColorMode
			}
			else {//default to GrayToColor_ToAllColorsAndAlpha
				a=byGrayValue; r=byGrayValue; g=byGrayValue; b=byGrayValue;
			}
		}//end GrayToColor(RPaint,...)
	}//end ROverlay class created 2009-12-23	

	
#region moved from RTypes

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
			RImage.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//RConvert.RgbToYhs(out Y, out H, out S, (float)r, (float)g, (float)b);
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
			RImage.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//RConvert.RgbToYhs(out Y, out H, out S, (float)r, (float)g, (float)b);
			A=RConvert.DecimalToByte(((float)a/255.0f) );
		}
		public void FromRgb(byte r, byte g, byte b) {
			RImage.RgbToHsv(out H, out S, out Y, ref r, ref g, ref b);//RConvert.RgbToYhs(out Y, out H, out S, (float)r, (float)g, (float)b);
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
			RImage.RgbToHsv(out H, out S, out V, ref r, ref g, ref b);
			A=a;
		}
		public void FromRgb(byte r, byte g, byte b) {
			RImage.RgbToHsv(out H, out S, out V, ref r, ref g, ref b);
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
#endregion moved from RTypes

}//end namespace
