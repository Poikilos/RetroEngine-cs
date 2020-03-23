/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PMS
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 //TODO:
 //-proportional GFont using alpha as guide (i.e. would normally be staggered on right edge)
 
using System; //for UInt32 ...
//using System.IO;

namespace ExpertMultimedia {
	class GFont {
		public static int GlyphTypeNormal=0;
		public static int GlyphTypeItalic=1;
		public static int GlyphTypeBold=2;
		public static int GlyphTypeBoldItalic=3;
		public static int iGlyphTypes=4;
		Anim[] animarrGlyphType;
		Gradient gradNow;
		//public int iOutWidth;
		//public int iOutHeight;
		//public int iBytesPP;
		//public byte[] byarrScreen;
		//GBuffer gbOut;
		public int Width {
			get {
				bool bGood=false;
				try {
					if (animarrGlyphType!=null && animarrGlyphType[GlyphTypeNormal]!=null) return animarrGlyphType[GlyphTypeNormal].Width;
					bGood=true;
				}
				catch (Exception exn) {
					bGood=false;
				}
				return 0;
			}
		}
		public int Height {
			get {
				bool bGood=false;
				try {
					if (animarrGlyphType!=null && animarrGlyphType[GlyphTypeNormal]!=null) return animarrGlyphType[GlyphTypeNormal].Height;
					bGood=true;
				}
				catch (Exception exn) {
					bGood=false;
				}
				return 0;
			}
		}
		public GFont() {
			Init();
		}
		private bool Init() {
			bool bGood=true;
			try {
				gradNow=new Gradient();
				animarrGlyphType=new Anim[GFont.iGlyphTypes];
				if (!ResetGlyphTypeArray()) bGood=false;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GFont Init");
				bGood=false;
			}
			return bGood;
		}
		public bool SaveSeq(string sFileBaseName, string sFileExt, int iGlyphType) {
			bool bGood=false;
			try {
				bGood=animarrGlyphType[iGlyphType].SaveSeq(sFileBaseName, sFileExt);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn, "GFont SaveSeq","saving font to image sequence");
				bGood=false;
			}
			if (!bGood) {
				try {
					RReporting.ShowErr("Failed to save "+sFileExt+" files named starting with "+sFileBaseName+" "+animarrGlyphType[iGlyphType].ToString(true),"GFont SaveSeq");
				}
				catch (Exception exn2) {
					RReporting.ShowExn(exn2,"GFont SaveSeq","accessing font glyphs (saving "+sFileExt+" files named starting with "+sFileBaseName+")");
				}
			}
			return bGood;
		}
		public bool TypeFast(ref GBuffer gbDest, ref IPoint ipDest, string sText) {
			return TypeFast(ref gbDest, ref ipDest, sText, 0);
		}
		public bool TypeFast(ref GBuffer gbDest, ref IPoint ipDest, string sText, int iGlyphType) {
			bool bGood=true;
			IPoint ipDestNow;
			try {
				ipDestNow=new IPoint();
				ipDestNow.X=ipDest.X;
				ipDestNow.Y=ipDest.Y;
				char[] carrText=sText.ToCharArray();
				for (int i=0; i<carrText.Length; i++) {
					animarrGlyphType[iGlyphType].GotoFrame((long)carrText[i]);
					if (!GBuffer.OverlayNoClipToBigCopyAlpha(ref gbDest, ref ipDestNow, ref animarrGlyphType[iGlyphType].gbFrame,ref gradNow)) {
						bGood=false;
						RReporting.ShowErr("failed to overlay text character#"+((long)carrText[i]).ToString(),"TypeFast");
					}
					ipDestNow.X+=animarrGlyphType[iGlyphType].gbFrame.iWidth;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"TypeFast(...,\""+sText+"\")");
				bGood=false;
			}
			return bGood;
		}
		public bool TypeHTML(ref GBuffer gbDest, ref IPoint ipAt, ref IZone izoneReturn, string sText,  bool bVisible) {
			int iNextTag;
			int iLength;
			bool bGood=true;
			try {
				//TODO: finish TypeHTML
				
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"TypeHTML(...,\""+sText+"\",...)");
				bGood=false;
			}
			return bGood;
		}
		public bool TypeHTML(ref GBuffer gbDest, ref IPoint ipAt, ref IZone izoneReturn, string sText) {
			return TypeHTML(ref gbDest, ref ipAt, ref izoneReturn, sText, true);
		}
		public bool ResetGlyphTypeArray() {
			bool bGood=false;
			try {
				animarrGlyphType=new Anim[iGlyphTypes];
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GFont ResetGlyphArray");
				bGood=false;
			}
			return bGood;
		}
		public bool FromImageValue(string sFile, int iCharWidth, int iCharHeight, int iRows, int iColumns) {
			bool bGood=false;
			Anim animTemp;
			try {
				animTemp=new Anim();
				bGood=Init();
				if (bGood) {
					bGood=animTemp.SplitFromImage32(sFile, iCharWidth, iCharHeight, iRows, iColumns);
					//animTemp.SaveSeq(Manager.sDataFolderSlash+"0.debug-initially-loaded-glyph-"+animTemp.gbFrame.Description()+"-", "png");
					//GBuffer.RawOverlayNoClipToBig(ref RetroEngine.gbScreenMain, ref ipAt, ref animTemp.gbFrame.byarrData, iCharWidth, iCharHeight, 4);
					if (bGood) {
						animarrGlyphType[GFont.GlyphTypeNormal]=animTemp.CopyAsGray();
						animarrGlyphType[GFont.GlyphTypeBold]=animarrGlyphType[GFont.GlyphTypeNormal].Copy();
						animarrGlyphType[GFont.GlyphTypeItalic]=animarrGlyphType[GFont.GlyphTypeNormal].Copy();
						animarrGlyphType[GFont.GlyphTypeBoldItalic]=animarrGlyphType[GFont.GlyphTypeNormal].Copy();
						//TODO: finish modifying the Glyph Types
					}
					//ShowAsciiTable();
					//sLogLine=".animarrGlyphType[GFont.GlyphTypeNormal].ToString(true) "+animarrGlyphType[GFont.GlyphTypeNormal].ToString(true);
				}
				else RReporting.ShowErr("Failed to initialize","GFont FromImageValue");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"FromImageValue");
			}
			return bGood;
		}
		//public void ShowAsciiTable(int xAt, int yAt) {
		//	ShowAsciiTable(Manager.gbScreenStatic, xAt, yAt);
		//}
		public void ShowAsciiTable(GBuffer gbDest, int xAt, int yAt) {
			try {
				IPoint ipAt=new IPoint(xAt,yAt);
				int lChar=0;
				for (int yChar=0; yChar<16; yChar++) {
					ipAt.X=xAt;
					for (int xChar=0; xChar<16; xChar++) {
						animarrGlyphType[GFont.GlyphTypeNormal].GotoFrame(lChar);
						GBuffer.OverlayNoClipToBigCopyAlpha(ref gbDest, ref ipAt, ref animarrGlyphType[GFont.GlyphTypeNormal].gbFrame);
						//GBuffer.RawOverlayNoClipToBig(ref RetroEngine.gbScreenMain, ref ipAt, 
						//                              ref animarrGlyphType[GFont.GlyphTypeNormal].gbFrame.byarrData,
						//                              animarrGlyphType[GFont.GlyphTypeNormal].gbFrame.iWidth,
						//                              animarrGlyphType[GFont.GlyphTypeNormal].gbFrame.iHeight, 1);
						ipAt.X+=animarrGlyphType[GFont.GlyphTypeNormal].gbFrame.iWidth;
						lChar++;
					}
					ipAt.Y+=animarrGlyphType[GFont.GlyphTypeNormal].gbFrame.iHeight;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GFont ShowAsciiTable");
			}
		}
	}//end class GFont
}//end namespace
