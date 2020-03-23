/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PMS
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System; //for UInt32 ...
using System.IO;

namespace ExpertMultimedia {
	public class GFont32BGRA {
		public const int LineBreakingOnlyWhenEndOfTextLine=0;
		public const int LineBreakingFast=1;
		public const int LineBreakingSlowAccurate=2;
		//public static StatusQ statusq;
		public static readonly string[] sarrGlyphType=new string[] {"normal","italic","bold","bold+italic"};
		
		public static int GlyphTypeNormal=0;
		public static int GlyphTypeItalic=1;
		public static int GlyphTypeBold=2;
		public static int GlyphTypeBoldItalic=3;
		public static int iGlyphTypes=4;
		Anim32BGRA[] animarrGlyphType=null;
		public int iPixelsTab=96;
		public int iPixelsSpace=14;
		public int iLineSpacing=24;
		Gradient32BGRA gradNow=null;
		//public int Width {
		//	get {
		//		bool bGood=false;
		//		try {
		//			if (animarrGlyphType!=null && animarrGlyphType[GlyphTypeNormal]!=null) return animarrGlyphType[GlyphTypeNormal].Width;
		//			bGood=true;
		//		}
		//		catch (Exception exn) {
		//			bGood=false;
		//		}
		//		return 0;
		//	}
		//}
		public int Height { get { return GlyphHeight(GlyphTypeNormal); } }
		//public int iOutWidth;
		//public int iOutHeight;
		//public int iBytesPP;
		//public byte[] byarrScreen;
		//GBuffer32BGRA gbOut;
		public bool SaveSeq(string sFileBaseName, string sFileExt, int iGlyphType) {
			Base.sLastFile=sFileBaseName+"*."+sFileExt;
			bool bGood=false;
			try {
				bGood=animarrGlyphType[iGlyphType].SaveSeq(sFileBaseName, sFileExt);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA SaveSeq","accessing font glyph type while saving {sFileBaseName:"+sFileBaseName+"; sFileExt:"+sFileExt+"; GlyphType:"+GlyphTypeToString(iGlyphType)+"}");
				bGood=false;
			}
			if (!bGood) {
				try {
					Base.ShowErr("Failed to save "+sFileExt+" files with names starting on "+sFileBaseName+" "+animarrGlyphType[iGlyphType].ToString(true),"SaveSeq");
				}
				catch (Exception exn2) {
					Base.ShowExn(exn2,"GFont32BGRA SaveSeq("+sFileBaseName+","+sFileExt+","+iGlyphType.ToString()+")","accessing font glyph type while saving error data {sFileBaseName:"+sFileBaseName+"; sFileExt:"+sFileExt+"; GlyphType:"+GlyphTypeToString(iGlyphType)+"}");
				}
			}
			return bGood;
		}//end SaveSeq
		public bool Render(ref GBuffer32BGRA gbDest, IPoint ipDest, string sText) {
			return Render(ref gbDest, ipDest, sText, 0);
		}
		
		public int WidthOf(char cChar, int iGlyphType) {
			int iReturn=0;
			try {
				if ((int)cChar==9) iReturn=iPixelsTab; //tab
				else if (cChar==' ') iReturn=iPixelsSpace; //space
				else if ((int)cChar==13) iReturn=0; //CR (LF is 10, and ignored)
				else {
					//animarrGlyphType[iGlyphType].GotoFrame((long)cChar);
					GBuffer32BGRA gbNow=Glyph(cChar,iGlyphType);
					if (gbNow!=null) iReturn=gbNow.iWidth;//animarrGlyphType[iGlyphType].gbFrame.iWidth;
				}
			}
			catch (Exception exn) {
				iReturn=0;
				Base.ShowExn(exn,"GFont32BGRA WidthOf(char,...)");
			}
			return iReturn;
		}
		public int WidthOf(char cChar) {
			return WidthOf(cChar,GFont32BGRA.GlyphTypeNormal);
		}
		public int WidthOf(string sText, int iGlyphType) {
			int iReturn=0;
			try {
				for (int iChar=0; iChar<sText.Length; iChar++) {
					iReturn+=WidthOf(sText[iChar]);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA WidthOf(string,...)");
			}
			return iReturn;
		}
		public int WidthOf(string sText) {
			return WidthOf(sText,GFont32BGRA.GlyphTypeNormal);
		}
		public int WidthOf(string sText, int iFirst, int iLast) {
			int iReturn=0;
			try {
				for (int iNow=iFirst; iNow<=iLast; iNow++) {
					iReturn+=WidthOf(sText[iNow]);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA WidthOf(char array,iFirst,iLast)");
			}
			return iReturn;
		}
		/*
		public bool WasPushedSinceSpacing(string sText, int iAtChar, ref int iLastReturn) {
			bool bReturn=false;
			try {
				int iSpaceBeforeChar=-1;
				while (iAtChar>0) {
					if (Base.IsSpacing(sText[iAtChar])) {
						iSpaceBeforeChar=iAtChar;
						break;
					}
					iAtChar--;
				}
				if (iLastReturn>=iSpaceBeforeChar) bReturn=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"WasPushedSinceSpacing");
			}
			return bReturn;
		}
		public int PushDown(string sText, int iAtChar, int iCharThatStartsLine, IRect rectDest) {
			int iReturn=0;
			//TODO: finish this
			try {
				int iWidthNow=WidthOf(sText[iAtChar]);
				int iTotalWidth=WidthOf(sText,iCharThatStartsLine,iAtChar);
				//TODO: debug using '\n' here
				if ((int)sText[iAtChar]=='\n') iReturn=iLineSpacing;//if ((int)sText[iAtChar]==13) iReturn=iLineSpacing;
				else if (rectDest.X+iTotalWidth>rectDest.Width&&!WasPushedSinceSpacing(sText,iAtChar)) {
					iReturn=iLineSpacing;
					iLastReturn=iAtChar;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA PushDown");
			}
			return iReturn;
		}
		*/
		public static string GlyphTypeToString(int iGlyphType) {
			string sReturn="uninitialized-glyphtype";
			try {
				sReturn=sarrGlyphType[iGlyphType];
			}
			catch {
				sReturn="nonexistent-glyphtype("+iGlyphType.ToString()+")";
			}
			return sReturn;
		}
		public bool Render(ref GBuffer32BGRA gbDest, IPoint ipDest, string sText, int iGlyphType) {
			try {
				IRect rectDest=new IRect(ipDest.X,ipDest.Y,gbDest.iWidth-ipDest.X,gbDest.iHeight-ipDest.Y);
				return Render(ref gbDest, rectDest, sText, iGlyphType);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA Render","accessing destination graphics");
			}
			return false;
		}
		public bool NeedsToBreakBeforeLineBreaker(string sText, int iCursor, int xPixel, IRect rectDest, int iGlyphType) {
			bool bReturn=false;
			try {
				if (rectDest!=null) {
					if (sText!=null&&iCursor>=0&&iCursor<sText.Length) {
						int zone_Right=rectDest.X+rectDest.Width;
						while (!Base.IsLineBreaker(sText[iCursor])) {//while (!Base.IsSpacing(sText[iCursor])) {
							xPixel+=WidthOf(sText[iCursor],iGlyphType);
							if (xPixel>=zone_Right) {
								bReturn=true;
								break;
							}
							iCursor++;
						}
					}
				}
				else Base.ShowErr("Cannot check line breaking using null rect.","NeedsToBreakBeforeLineBreaker");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"NeedsToBreakBeforeLineBreaker","checking for line break {sText"+Base.VariableMessageStyleOperatorAndValue(sText,false)+"; iCursor:"+iCursor.ToString()+"; xPixel:"+xPixel.ToString()+"; rectDest:"+Base.VariableMessage(rectDest)+"}");
			}
			return bReturn;
		}
		public bool ReadLine(out string sLine, string sText, ref int iMoveMe, IRect rectDest, int iGlyphType) {
			bool HasALine=false;
			bool bBreakable=false;
			int iSkipper=0;
			int iStartLine=iMoveMe;
			try {
				int iStart=iMoveMe;
				int xPixel=rectDest.X;
				int zone_Right=rectDest.Right;
				if (iMoveMe+1<sText.Length) HasALine=true;
				while (iMoveMe+1<sText.Length) {
					//string sTemp=SafeSubstring(sText,iMoveMe,Environment.NewLine.Length);
					if (Base.CompareAt(sText,Environment.NewLine,iMoveMe)) {
						bBreakable=true;
						iSkipper=Environment.NewLine.Length;
						break;
					}
					else if (Base.CompareAt(sText,'\n',iMoveMe)) {
						bBreakable=true;
						iSkipper=1;
						break;
					}
					else if (Base.CompareAt(sText,'\r',iMoveMe)) {
						bBreakable=true;
						iSkipper=1;
						break;
					}
					else if (xPixel+WidthOf(sText[iMoveMe],iGlyphType)>=zone_Right) {
						int iLastBreaker=iMoveMe;
						bool bVisibleBreaker=false;
						bool bTest=Base.PreviousLineBreakerExceptNewLine(sText, ref iLastBreaker, out bVisibleBreaker);
						if (iLastBreaker>iStartLine) {
							if (bVisibleBreaker) {
								iSkipper=0;//says not to skip any characters
								iMoveMe=iLastBreaker+1;//includes it in the substring
							}
							else {//else invisible, so skip it
								iSkipper=1;//says to skip it after substring is taken
								iMoveMe=iLastBreaker;//excludes it from the substring
							}
						}
						else {//else no breaker
							iSkipper=0;
						}
						//NOTE: debug non-compliance: bBreakable and break allow non-html-style forced breakage if no breaker before wrap (when "else" case above occurs)
						bBreakable=true;//says to not skip to end of sText
						break;
					}
					//else if (NeedsToBreakBeforeLineBreaker(sText, iMoveMe+1, xPixel, rectDest, iGlyphType) && Base.IsLineBreakerExceptNewLine(sText,iMoveMe)) {
					//	bBreakable=true;
					//	iSkipper=1;
					//	bTypeableNewLine=true;
					//	break;
					//}
					else {
						xPixel+=WidthOf(sText[iMoveMe],iGlyphType);
						iMoveMe++;
					}
				}
				if (!bBreakable) iMoveMe=sText.Length;
				sLine=Base.SafeSubstring(sText,iStart,iMoveMe-iStart);
				if (bBreakable) iMoveMe+=iSkipper;
			}
			catch (Exception exn) {
				sLine="";
				Base.ShowException(exn,"GFont32BGRA ReadLine");
			}
			return HasALine;
		}
		public int GlyphHeight(int iGlyphType) {
			int iReturn=0;
			GBuffer32BGRA gbNow=Glyph('|',iGlyphType);
			if (gbNow!=null) iReturn=gbNow.Height;
			return iReturn;
		}
		public bool Render(ref GBuffer32BGRA gbDest, IRect rectDest, string sText, int iGlyphType) {
			return Render(ref gbDest, rectDest, sText, iGlyphType, LineBreakingOnlyWhenEndOfTextLine);
		}
		public bool Render(ref GBuffer32BGRA gbDest, IRect rectDest, string sText, int iGlyphType, int LineBreaking) { //formerly typefast
			//TODO: really, this should return a rect (e.g. html-style stretching of container)
			bool bGood=true;
			//IPoint ipDestNow;
			//IPoint ipDestLine;
			bool bSpacing;
			try {
				//ipDestNow=new IPoint();
				//ipDestNow.X=rectDest.X;
				//ipDestNow.Y=rectDest.Y;
				//ipDestLine.Set(ipDestNow);
				int zone_Bottom=rectDest.Bottom;
				string sLine;
				int iCursor=0;
				int yDest=rectDest.Y;
				int iLineHeight=GlyphHeight(iGlyphType);
				int iLine=0;
				//GBuffer32BGRA.SetBrushRgb(0,0,0);
				//gbDest.DrawRectCropped(rectDest);
				//string sDebugReadLineFile="0.debug-Typing.txt";//debug only
				//bool bDebug=!File.Exists(sDebugReadLineFile);
				CalculateSpacing(iGlyphType);
				if (LineBreaking==LineBreakingOnlyWhenEndOfTextLine) {
					while (RenderLine(ref gbDest, rectDest.X, yDest, sText, iGlyphType, ref iCursor, false)) { 
						yDest+=iLineHeight;
						if (yDest+iLineHeight>=zone_Bottom) break;//+iLineHeight skips croppable chars
						iLine++;
					}
				}
				else if (LineBreaking==LineBreakingFast) {
					while (RenderLine(ref gbDest, rectDest.X, yDest, sText, iGlyphType, ref iCursor, true)) { 
						yDest+=iLineHeight;
						if (yDest+iLineHeight>=zone_Bottom) break;//+iLineHeight skips croppable chars
						iLine++;
					}
				}
				else {//assume LineBreakingSlowAccurate
					while (ReadLine(out sLine, sText, ref iCursor, rectDest, iGlyphType)) { 
						if (yDest<zone_Bottom) TypeOnOneLine(ref gbDest, rectDest.X, yDest, sLine, iGlyphType);
						else break;
						yDest+=iLineHeight;//+iLineHeight skips croppable chars
						iLine++;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Render(...,\""+Base.ElipsisIfOver(sText,10)+"\")");
				bGood=false;
			}
			return bGood;
		}
		public GBuffer32BGRA Glyph(char cNow, int iGlyphType) {
			try {
				return animarrGlyphType[iGlyphType].Frame((long)cNow);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA Glyph","getting font glyph {cNow:'"+char.ToString(cNow)+"'; ascii:"+((int)cNow).ToString()+"; iGlyphType:"+GlyphTypeToString(iGlyphType)+"}");
			}
			return null;
		}
		public bool TypeOnOneLine(ref GBuffer32BGRA gbDest, int xDest, int yDest, string sText, int iGlyphType) {
			//TODO: really, this should return a rect (e.g. html-style stretching of container)
			bool bGood=true;
			bool bSpacing;
			int xNow=xDest;
			try {
				int iCursor=0;
				for (int iChar=0; iChar<sText.Length; iChar++) {
					GBuffer32BGRA gbNow=Glyph(sText[iChar],iGlyphType);
					if (xNow+gbNow.Width<gbDest.Width) {
						if (!Base.IsSpacing(sText[iChar])) gbDest.DrawSmallerWithoutCropElseCancel(xNow,yDest,gbNow,GBuffer32BGRA.DrawModeAlphaHardEdge);
					}
					else break;
					xNow+=WidthOf(sText[iChar],iGlyphType);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"TypeOnOneLine(...,\""+Base.ElipsisIfOver(sText,10)+"\")");
				bGood=false;
			}
			return bGood;
		}//end TypeOnOneLine
		public bool RenderLine(ref GBuffer32BGRA gbDest, int xDest, int yDest, string sText, int iGlyphType, ref int iCursor, bool bAllowLineBreaking) {
			//TODO: really, this should return a rect (e.g. html-style stretching of container)
			bool bGood=true;
			bool bSpacing;
			int xNow=xDest;
			int iWidthNow;
			try {
				int iNewLine=Base.IsNewLineAndGetLength(sText,iCursor);
				while (iNewLine==0&&iCursor<sText.Length) {
					iWidthNow=WidthOf(sText[iCursor],iGlyphType);
					if (xNow+iWidthNow<gbDest.Width) {
						if (!Base.IsSpacingExceptNewLine(sText[iCursor])) gbDest.DrawSmallerWithoutCropElseCancel(xNow,yDest,Glyph(sText[iCursor],iGlyphType),GBuffer32BGRA.DrawModeAlphaHardEdge);
					}
					else {
						if (bAllowLineBreaking) break;
					}
					xNow+=iWidthNow;
					iCursor++;
					iNewLine=Base.IsNewLineAndGetLength(sText,iCursor);
				}
				iCursor+=iNewLine;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RenderLine(...,\""+Base.ElipsisIfOver(sText,10)+"\")");
				bGood=false;
			}
			return bGood;
		}//end RenderLine
		public bool TypeHTML(ref GBuffer32BGRA gbDest, ref IPoint ipAt, IRect irectReturn, string sText,  bool bVisible) {
			int iNextTag;
			int iLength;
			bool bGood=true;
			try {
				//TODO: finish TypeHTML
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"TypeHTML(...,\""+sText+"\",...)");
				bGood=false;
			}
			return bGood;
		}
		public bool TypeHTML(ref GBuffer32BGRA gbDest, ref IPoint ipAt, IRect irectReturn, string sText) {
			return TypeHTML(ref gbDest, ref ipAt, irectReturn, sText, true);
		}
		private bool Init() {
			bool bGood=true;
			try {
				gradNow=new Gradient32BGRA();
				gradNow.From(new int[] {0,32,223,255}, new Pixel32Struct[] {new Pixel32Struct(64,64,64,0), new Pixel32Struct(0,0,0,255), new Pixel32Struct(255,255,255,255),new Pixel32Struct(255,255,255,255)});
				animarrGlyphType=new Anim32BGRA[GFont32BGRA.iGlyphTypes];
				if (false==ResetGlyphTypeArray()) bGood=false;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA Init");
				bGood=false;
			}
			return bGood;
		}
		public bool ResetGlyphTypeArray() {
			bool bGood=false;
			try {
				animarrGlyphType=new Anim32BGRA[iGlyphTypes];
				bGood=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA Init","initializing glyphtype array");
				bGood=false;
			}
			return bGood;
		}
		private Anim32BGRA Normal {
			get {
				return animarrGlyphType[GFont32BGRA.GlyphTypeNormal];
			}
			set {
				animarrGlyphType[GFont32BGRA.GlyphTypeNormal]=value;
			}
		}
		private Anim32BGRA Bold {
			get {
				return animarrGlyphType[GFont32BGRA.GlyphTypeBold];
			}
			set {
				animarrGlyphType[GFont32BGRA.GlyphTypeBold]=value;
			}
		}
		private Anim32BGRA Italic {
			get {
				return animarrGlyphType[GFont32BGRA.GlyphTypeItalic];
			}
			set {
				animarrGlyphType[GFont32BGRA.GlyphTypeItalic]=value;
			}
		}
		private Anim32BGRA BoldItalic {
			get {
				return animarrGlyphType[GFont32BGRA.GlyphTypeBoldItalic];
			}
			set {
				animarrGlyphType[GFont32BGRA.GlyphTypeBoldItalic]=value;
			}
		}
		public void CalculateSpacing(int iGlyphType) {
			iPixelsSpace=(int)( (double)WidthOf('|',iGlyphType)*1.5 );
			iPixelsTab=iPixelsSpace*5;
			iLineSpacing=(int)( (double)Height*1.5 );
		}
		public bool FromFixedHeightStaggered(string sFile, int iCharHeight) {//this is named FromFixedHeightStaggered while the function it called anim.SplitFromFixedHeightStaggered
			Base.sLastFile=sFile+"...";
			bool bGood=false;
			Anim32BGRA animNormal=null;
			bGood=Init();
			try {
				if (!sFile.EndsWith(".png")) {
					animNormal=new Anim32BGRA();
					GBuffer32BGRA gbNormal=new GBuffer32BGRA();
					Base.sLastFile=sFile+".png";
					if (!gbNormal.Load(sFile+".png",4)) { //assumes 32-bit is needed
						Base.ShowErr("Cannot load font file \""+sFile+".png\".");
					}
					
					
					bGood=animNormal.SplitFromFixedHeightStaggered(gbNormal, iCharHeight);
					if (bGood) {
						Normal=animNormal;
						if (File.Exists(sFile+"-bold.png")) {
							Base.sLastFile=sFile+"-bold.png";
							if (animarrGlyphType[GFont32BGRA.GlyphTypeBold]==null) Bold=new Anim32BGRA();
							GBuffer32BGRA gbBold=new GBuffer32BGRA();
							gbBold.Load(sFile+"-bold.png",4);//assumes 32-bit is needed
							Bold.SplitFromFixedHeightStaggered(gbBold, iCharHeight);
						}
						else Bold=animNormal.Copy();
						Italic=Normal.Copy();
						BoldItalic=Bold.Copy();
						//TODO: finish this -- modifying the Glyph Types -- italics via image manip
						CalculateSpacing(GlyphTypeNormal);
						bGood=true;
					}
				}
				else {
					bGood=false;
					Base.ShowErr("GFont32BGRA FromFixedHeightStaggered file must not end with extension--must have assumed png extension.");
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"FromFixedHeightStaggered("+sFile+","+iCharHeight.ToString()+")","Splitting Proportional Font Glyphs");
			}
			return bGood;
		}
		public bool FromImageValue(string sFile, int iCharWidth, int iCharHeight, int iRows, int iColumns) {
			bool bGood=false;
			Anim32BGRA animNormal;
			Base.sLastFile=sFile;
			try {
				animNormal=new Anim32BGRA();
				bGood=Init();
				if (bGood) {
					bGood=animNormal.SplitFromImage32(sFile, iCharWidth, iCharHeight, iRows, iColumns);
					//animNormal.SaveSeq("etc/test/0.debug-glyph", "png");
					//GBuffer32BGRA.RawOverlayNoClipToBig(ref gbTarget, ref ipAt, ref animNormal.gbFrame.byarrData, iCharWidth, iCharHeight, 4);
					if (bGood) {
						Normal=animNormal.CopyAsGray();
						Bold=Normal.Copy();
						Italic=Normal.Copy();
						BoldItalic=Normal.Copy();
						//TODO: finish modifying the Glyph Types -- italics using image manip
					}
					//else Base.ShowErr("GFont failed to split image","GFont32BGRA FromImageValue");//already shown by anim
					//ShowAsciiTable();
					//Base.WriteLine("Normal.ToString(true):"+Normal.ToString(true));
				}
				else Base.ShowErr("Couldn't initialize font glyph graphics buffer","GFont32BGRA FromImageValue","initializing font graphics");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA FromImageValue","initializing font graphics");
			}
			return bGood;
		}//end FromImageValue
		public bool FromImage(string sFile, int iCharWidth, int iCharHeight, int iRows, int iColumns) {
			bool bGood=false;
			Anim32BGRA animNormal;
			Base.sLastFile=sFile;//TODO: implement this EVERYWHERE
			try {
				animNormal=new Anim32BGRA();
				bGood=Init();
				if (bGood) {
					bGood=animNormal.SplitFromImage32(sFile, iCharWidth, iCharHeight, iRows, iColumns);
					//animNormal.SaveSeq("etc/test/0.debug-glyph", "png");
					//GBuffer32BGRA.RawOverlayNoClipToBig(ref gbTarget, ref ipAt, ref animNormal.gbFrame.byarrData, iCharWidth, iCharHeight, 4);
					if (bGood) {
						Normal=animNormal;
						Bold=Normal.Copy();
						Italic=Normal.Copy();
						BoldItalic=Normal.Copy();
						//TODO: finish modifying the Glyph Types -- italics using image manip
					}
					//else Base.ShowErr("GFont failed to split image","GFont32BGRA FromImage");//already shown by anim
					//ShowAsciiTable();
					//Base.WriteLine("Normal.ToString(true):"+Normal.ToString(true));
				}
				else Base.ShowErr("Couldn't initialize font glyph graphics buffer","GFont32BGRA FromImage","initializing font graphics");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GFont32BGRA FromImage","initializing font graphics");
			}
			return bGood;
		}//end FromImage
		public void ShowAsciiTable(GBuffer32BGRA gbTarget, int xAt, int yAt) {
			Base.sLastFile=Normal.SourceToRegExString();
			try {
				IPoint ipAt=new IPoint(xAt,yAt);
				int lChar=0;
				for (int yChar=0; yChar<16; yChar++) {
					ipAt.X=xAt;
					for (int xChar=0; xChar<16; xChar++) {
						animarrGlyphType[GFont32BGRA.GlyphTypeNormal].GotoFrame(lChar);
						GBuffer32BGRA.RawOverlayNoClipToBig(ref gbTarget, ref ipAt, 
						                              ref animarrGlyphType[GFont32BGRA.GlyphTypeNormal].gbFrame.byarrData,
						                              animarrGlyphType[GFont32BGRA.GlyphTypeNormal].gbFrame.iWidth,
						                              animarrGlyphType[GFont32BGRA.GlyphTypeNormal].gbFrame.iHeight, 1);
						ipAt.X+=animarrGlyphType[GFont32BGRA.GlyphTypeNormal].gbFrame.iWidth;
						lChar++;
					}
					ipAt.Y+=animarrGlyphType[GFont32BGRA.GlyphTypeNormal].gbFrame.iHeight;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ShowAsciiTable");
			}
		}//end ShowAsciiTable
	}//end class GFont32BGRA
}//end namespace
