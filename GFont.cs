/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PMS
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System; //for UInt32 ...
//using System.IO;

namespace ExpertMultimedia {
	class GFont {
		//public static StatusQ statusq;
		public static int GlyphTypePlain=0;
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
		public bool SaveSeq(string sFileBaseName, string sFileExt, int iGlyphType) {
			bool bGood=false;
			try {
				bGood=animarrGlyphType[iGlyphType].SaveSeq(sFileBaseName, sFileExt);
			}
			catch (Exception exn) {
				sFuncNow="SaveSeq";
				sLastErr="Exception error, can't save font to image sequence--"+exn.ToString();
				bGood=false;
			}
			if (bGood==false) {
				sFuncNow="SaveSeq";
				try {
					sLastErr="Failed to save "+sFileExt+" files named starting with "+sFileBaseName+" "+animarrGlyphType[iGlyphType].ToString(true);
				}
				catch (Exception e2) {
					sLastErr="Exception accessing font glyphs, and failed to save "+sFileExt+" files named starting with "+sFileBaseName+"--"+e2.ToString();
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
				ipDestNow.x=ipDest.x;
				ipDestNow.y=ipDest.y;
				char[] carrText=sText.ToCharArray();
				for (int i=0; i<carrText.Length; i++) {
					animarrGlyphType[iGlyphType].GotoFrame((long)carrText[i]);
					if (false==GBuffer.OverlayNoClipToBigCopyAlpha(ref gbDest, ref ipDestNow, ref animarrGlyphType[iGlyphType].gbFrame,ref gradNow,0)) {
						bGood=false;
						sFuncNow="TypeFast";
						sLastErr="failed to overlay text character#"+((long)carrText[i]).ToString();
					}
					ipDestNow.x+=animarrGlyphType[iGlyphType].gbFrame.iWidth;
				}
			}
			catch (Exception exn) {
				sFuncNow="TypeFast(...,\""+sText+"\")";
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool TypeHTML(ref GBuffer gbDest, ref IPoint ipAt, ref IRect irectReturn, string sText,  bool bVisible) {
			int iNextTag;
			int iLength;
			bool bGood=true;
			try {
				//TODO: finish TypeHTML
				
			}
			catch (Exception exn) {
				sFuncNow="TypeHTML(...,\""+sText+"\",...)";
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool TypeHTML(ref GBuffer gbDest, ref IPoint ipAt, ref IRect irectReturn, string sText) {
			return TypeHTML(ref gbDest, ref ipAt, ref irectReturn, sText, true);
		}
		private bool Init() {
			bool bGood=true;
			try {
				gradNow=new Gradient();
				animarrGlyphType=new Anim[GFont.iGlyphTypes];
				if (false==ResetGlyphTypeArray()) bGood=false;
			}
			catch (Exception exn) {
				sFuncNow="Init";
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool ResetGlyphTypeArray() {
			bool bGood=false;
			try {
				animarrGlyphType=new Anim[iGlyphTypes];
				bGood=true;
			}
			catch (Exception exn) {
				sLastErr="Exception error duringitializing GFont GlyphType Array--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool FromImageValue(string sFile, int iCharWidth, int iCharHeight, int iRows, int iColumns) {
			sFuncNow="FromImageValue(...)";
			bool bGood=false;
			Anim animTemp;
			try {
				animTemp=new Anim();
				bGood=Init();
				if (bGood) {
					bGood=animTemp.SplitFromImage32(sFile, iCharWidth, iCharHeight, iRows, iColumns);
					//animTemp.SaveSeq("0.debug-glyph", "png");
					//GBuffer.RawOverlayNoClipToBig(ref RetroEngine.gbScreenMain, ref ipAt, ref animTemp.gbFrame.byarrData, iCharWidth, iCharHeight, 4);
					if (bGood) {
						animarrGlyphType[GFont.GlyphTypePlain]=animTemp.CopyAsGrey();
						animarrGlyphType[GFont.GlyphTypeBold]=animarrGlyphType[GFont.GlyphTypePlain].Copy();
						animarrGlyphType[GFont.GlyphTypeItalic]=animarrGlyphType[GFont.GlyphTypePlain].Copy();
						animarrGlyphType[GFont.GlyphTypeBoldItalic]=animarrGlyphType[GFont.GlyphTypePlain].Copy();
						//TODO: finish modifying the Glyph Types
					}
					//ShowAsciiTable();
					//sLogLine=".animarrGlyphType[GFont.GlyphTypePlain].ToString(true) "+animarrGlyphType[GFont.GlyphTypePlain].ToString(true);
				}
				else sLastErr="Failed to initialize";
			}
			catch (Exception exn) {
				sFuncNow="FromImageValue(...)";
				sLastErr="Exception error--"+exn.ToString();
			}
			return bGood;
		}
		public void ShowAsciiTable(int xAt, int yAt) {
			try {
				IPoint ipAt=new IPoint(xAt,yAt);
				int lChar=0;
				for (int yChar=0; yChar<16; yChar++) {
					ipAt.x=xAt;
					for (int xChar=0; xChar<16; xChar++) {
						animarrGlyphType[GFont.GlyphTypePlain].GotoFrame(lChar);
						GBuffer.RawOverlayNoClipToBig(ref RetroEngine.gbScreenMain, ref ipAt, 
						                              ref animarrGlyphType[GFont.GlyphTypePlain].gbFrame.byarrData,
						                              animarrGlyphType[GFont.GlyphTypePlain].gbFrame.iWidth,
						                              animarrGlyphType[GFont.GlyphTypePlain].gbFrame.iHeight, 1);
						ipAt.x+=animarrGlyphType[GFont.GlyphTypePlain].gbFrame.iWidth;
						lChar++;
					}
					ipAt.y+=animarrGlyphType[GFont.GlyphTypePlain].gbFrame.iHeight;
				}
			}
			catch (Exception exn) {
				sFuncNow="ShowAsciiTable";
				sLastErr="Exception error--"+exn.ToString();
			}
		}
	}//end class GFont
}//end namespace
