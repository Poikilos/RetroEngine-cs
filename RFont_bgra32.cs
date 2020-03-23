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
	public class RFont {
		#region variables
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
		private RAnim[] animarrGlyphType=null;
		public int iPixelsTab=96;
		public int iPixelsSpace=14;
		public int iLineSpacing=24;//aka descent--baseline to the top of the next line (font height can be greater with fonts that overlap verically (i.e. cursive)
		private RGradient gradNow=null;
		public static RFont rfontDefault=null;//set by static constructor
// 				settings.CreateOrIgnore("DefaultFont","./Library/Fonts/thepixone-12x16");
		private static string sFontDefault="./Library/Fonts/thepixone-12x16";
		private static int iFontHeightDefault=16;
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
		//RImage riOut;
		#endregion variables
		
		#region constructors
		static RFont() {//static constructor
			rfontDefault=new RFont();
			//sFontDefault=settings.GetForcedString("DefaultFont");
			//iFontHeightDefault=settings.GetForcedInt("DefaultFontHeight");
			rfontDefault.FromFixedHeightStaggered(sFontDefault,iFontHeightDefault);
		}
		#endregion constructors
		
		public bool SaveSeq(string sFileBaseName, string sFileExt, int iGlyphType) {
			RReporting.sLastFile=sFileBaseName+"*."+sFileExt;
			bool bGood=false;
			try {
				bGood=animarrGlyphType[iGlyphType].SaveSeq(sFileBaseName, sFileExt);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"accessing font glyph type while saving","RFont SaveSeq {sFileBaseName:"+sFileBaseName+"; sFileExt:"+sFileExt+"; GlyphType:"+GlyphTypeToString(iGlyphType)+"}");
				bGood=false;
			}
			if (!bGood) {
				try {
					RReporting.ShowErr("Failed to save "+sFileExt+" files with names starting on "+sFileBaseName+" "+animarrGlyphType[iGlyphType].ToString(true),"","SaveSeq");
				}
				catch (Exception exn2) {
					RReporting.ShowExn(exn2,"accessing font glyph type while saving error data","RFont SaveSeq("+sFileBaseName+","+sFileExt+","+iGlyphType.ToString()+"){sFileBaseName:"+sFileBaseName+"; sFileExt:"+sFileExt+"; GlyphType:"+GlyphTypeToString(iGlyphType)+"}");
				}
			}
			return bGood;
		}//end SaveSeq
		public bool RenderLine(ref RImage riDest, int xDest, int yDest, string sText, int iGlyphType, ref int iCursor, bool bAllowLineBreaking) {
			//TODO: really, this should return a rect (e.g. html-style stretching of container)
			bool bGood=true;
			bool bSpacing;
			int xNow=xDest;
			int iWidthNow;
			try {
				int iNewLine=RString.IsNewLineAndGetLength(sText,iCursor);
				while (iNewLine==0&&iCursor<sText.Length) {
					iWidthNow=WidthOf(sText[iCursor],iGlyphType);
					if (xNow+iWidthNow<riDest.Width) {
						if (!RString.IsSpacingExceptNewLine(sText[iCursor])) {
							RImage.brushFore.SetRgb(0,0,0);
							riDest.DrawSmallerWithoutCropElseCancel(xNow,yDest,Glyph(sText[iCursor],iGlyphType),RImage.DrawModeAlphaHardEdge);
						}
					}
					else {
						if (bAllowLineBreaking) break;
					}
					xNow+=iWidthNow;
					iCursor++;
					iNewLine=RString.IsNewLineAndGetLength(sText,iCursor);
				}
				iCursor+=iNewLine;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RenderLine(...,\""+RString.ElipsisIfOver(sText,10)+"\")");
				bGood=false;
			}
			return bGood;
		}//end RenderLine
		public bool Render(ref RImage riDest, IRect rectDest, string sText, int iGlyphType, int LineBreaking) { //formerly typefast
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
				//RImage.brushFore.SetRgb(0,0,0);
				//riDest.DrawRectCropped(rectDest);
				//string sDebugReadLineFile="0.debug-Typing.txt";//debug only
				//bool bDebug=!File.Exists(sDebugReadLineFile);
				CalculateSpacing(iGlyphType);
				if (LineBreaking==LineBreakingOnlyWhenEndOfTextLine) {
					while (RenderLine(ref riDest, rectDest.X, yDest, sText, iGlyphType, ref iCursor, false)) { 
						yDest+=iLineHeight;
						if (yDest+iLineHeight>=zone_Bottom) break;//+iLineHeight skips croppable chars
						iLine++;
					}
				}
				else if (LineBreaking==LineBreakingFast) {
					while (RenderLine(ref riDest, rectDest.X, yDest, sText, iGlyphType, ref iCursor, true)) { 
						yDest+=iLineHeight;
						if (yDest+iLineHeight>=zone_Bottom) break;//+iLineHeight skips croppable chars
						iLine++;
					}
				}
				else {//assume LineBreakingSlowAccurate
					while (ReadLine(out sLine, sText, ref iCursor, rectDest, iGlyphType)) { 
						if (yDest<zone_Bottom) TypeOnOneLine(ref riDest, rectDest.X, yDest, sLine, iGlyphType);
						else break;
						yDest+=iLineHeight;//+iLineHeight skips croppable chars
						iLine++;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","Render(...,\""+RString.ElipsisIfOver(sText,10)+"\")");
				bGood=false;
			}
			return bGood;
		}
		public RImage Glyph(char cNow, int iGlyphType) {
			try {
				if (animarrGlyphType[iGlyphType]!=null) return animarrGlyphType[iGlyphType].Frame((long)cNow);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting font glyph","RFont Glyph {cNow:'"+char.ToString(cNow)+"'; ascii:"+((int)cNow).ToString()+"; iGlyphType:"+GlyphTypeToString(iGlyphType)+"}");
			}
			return null;
		}//end Glyph(cNow,iGlyphType)
		public bool TypeOnOneLine(ref RImage riDest, int xDest, int yDest, string sText, int iGlyphType) {
			//TODO: really, this should return a rect (e.g. html-style stretching of container)
			bool bGood=true;
			bool bSpacing;
			int xNow=xDest;
			try {
				int iCursor=0;
				for (int iChar=0; iChar<sText.Length; iChar++) {
					RImage riNow=Glyph(sText[iChar],iGlyphType);
					if (xNow+riNow.Width<riDest.Width) {
						if (!RString.IsSpacing(sText[iChar])) riDest.DrawSmallerWithoutCropElseCancel(xNow,yDest,riNow,RImage.DrawModeAlphaHardEdge);
					}
					else break;
					xNow+=WidthOf(sText[iChar],iGlyphType);
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","TypeOnOneLine(...,\""+RString.ElipsisIfOver(sText,10)+"\")");
				bGood=false;
			}
			return bGood;
		}//end TypeOnOneLine
		public bool Render(ref RImage riDest, IPoint ipDest, string sText) {
			return Render(ref riDest, ipDest, sText, GlyphTypeNormal);
		}
		public bool Render(ref RImage riDest, IZone zoneDest, string sText) {
			try { zoneDest.CopyTo(ref rectDefault); }
			catch (Exception exn) { RReporting.ShowExn(exn,"accessing destination text zone","RFont Render"); riDest.ToRect(ref rectDefault); }
			return Render(ref riDest, rectDefault, sText, GlyphTypeNormal);
		}
		
		public int XPixelToChar(int xPixel, string sLine, int iGlyphTypeParent) {
			int iChar=0;
			int xSparse=0;
			try {
				while (iChar<=sLine.Length&&xSparse<xPixel) {
					//TODO: modify glyphtype for each character if html sets glyph type
					// /2 selects division after character if clicking half way across character
					if (iChar<sLine.Length) xSparse+=this.WidthOf(sLine[iChar],iGlyphTypeParent)/2;
					if (xSparse>=xPixel) break;
					if (iChar<sLine.Length) xSparse+=this.WidthOf(sLine[iChar],iGlyphTypeParent)-this.WidthOf(sLine[iChar],iGlyphTypeParent)/2;
					iChar++;
				}
				Console.WriteLine(String.Format("XPixelToChar{{xPixel:{0};iChar:{1}}}",xPixel,iChar));//debug only
				//iChar--; //so clicking a char (after it starts) selects it
				if (iChar>sLine.Length) iChar=sLine.Length;//select at line end
				if (iChar<0) iChar=0;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return iChar;
		}
		public int YPixelToLine(int yPixel) {
			return yPixel/iLineSpacing;
		}
		public int WidthOf(char cChar, int iGlyphType) {
			int iReturn=0;
			try {
				if ((int)cChar==9) iReturn=iPixelsTab; //tab
				else if (cChar==' ') iReturn=iPixelsSpace; //space
				else if ((int)cChar==13) iReturn=0; //CR (LF is 10, and ignored)
				else {
					//animarrGlyphType[iGlyphType].GotoFrame((long)cChar);
					RImage riNow=Glyph(cChar,iGlyphType);
					if (riNow!=null) iReturn=riNow.iWidth;//animarrGlyphType[iGlyphType].riFrame.iWidth;
				}
			}
			catch (Exception exn) {
				iReturn=0;
				RReporting.ShowExn( exn, "", String.Format("RFont WidthOf({0},{1})",char.ToString(cChar),GlyphTypeToString(iGlyphType)) );
			}
			return iReturn;
		}
		public int WidthOf(char cChar) {
			return WidthOf(cChar,RFont.GlyphTypeNormal);
		}
		public int WidthOf(string sText, int iGlyphType) {
			int iReturn=0;
			try {
				for (int iChar=0; iChar<sText.Length; iChar++) {
					iReturn+=WidthOf(sText[iChar]);
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RFont WidthOf(string,...)");
			}
			return iReturn;
		}
		public int WidthOf(string sText) {
			return WidthOf(sText,RFont.GlyphTypeNormal);
		}
		public int WidthOf(string sText, int iFirst, int iLast) {
			int iReturn=0;
			try {
				for (int iNow=iFirst; iNow<=iLast; iNow++) {
					iReturn+=WidthOf(sText[iNow]);
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RFont WidthOf(char array,iFirst,iLast)");
			}
			return iReturn;
		}
		/*
		public bool WasPushedSinceSpacing(string sText, int iAtChar, ref int iLastReturn) {
			bool bReturn=false;
			try {
				int iSpaceBeforeChar=-1;
				while (iAtChar>0) {
					if (RString.IsSpacing(sText[iAtChar])) {
						iSpaceBeforeChar=iAtChar;
						break;
					}
					iAtChar--;
				}
				if (iLastReturn>=iSpaceBeforeChar) bReturn=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","WasPushedSinceSpacing");
			}
			return bReturn;
		}
		public int PushDown(string sText, int iAtChar, int iCharThatStartsLine, IRect rectDest) {
			int iReturn=0;
			//TODO: finish this -- PushDown
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
				RReporting.ShowExn(exn,"","RFont PushDown");
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
		private IRect rectDefault=new IRect();
		public bool Render(ref RImage riDest, IPoint ipDest, string sText, int iGlyphType) {
			try {
				rectDefault.Set(ipDest.X,ipDest.Y,riDest.iWidth-ipDest.X,riDest.iHeight-ipDest.Y);
				return Render(ref riDest, rectDefault, sText, iGlyphType);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"accessing destination graphics","RFont Render");
			}
			return false;
		}
		public bool Render(ref RImage riDest, int x, int y, string sText) {
			return Render(ref riDest,x,y,sText,RFont.GlyphTypeNormal);
		}
		public bool Render(ref RImage riDest, int x, int y, string sText, int iGlyphType) {
			try {
				rectDefault.Set(x,y,riDest.iWidth-x,riDest.iHeight-y);
				return Render(ref riDest, rectDefault, sText, iGlyphType);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"accessing destination graphics","RFont Render");
			}
			return false;
		}
		public bool NeedsToBreakBeforeLineBreaker(string sText, int iCursor, int xPixel, IRect rectDest, int iGlyphType) {
			bool bReturn=false;
			try {
				if (rectDest!=null) {
					if (sText!=null&&iCursor>=0&&iCursor<sText.Length) {
						int zone_Right=rectDest.X+rectDest.Width;
						while (!RString.IsLineBreaker(sText[iCursor])) {//while (!RString.IsSpacing(sText[iCursor])) {
							xPixel+=WidthOf(sText[iCursor],iGlyphType);
							if (xPixel>=zone_Right) {
								bReturn=true;
								break;
							}
							iCursor++;
						}
					}
				}
				else RReporting.ShowErr("Cannot check line breaking using null rect.","","NeedsToBreakBeforeLineBreaker");
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn, "checking for line break", String.Format("NeedsToBreakBeforeLineBreaker {{{0}{1}{2}rectDest:{3}}}",
					RReporting.DebugStyle("sText",sText,false,true),
					RReporting.DebugStyle("iCursor",iCursor,true),
					RReporting.DebugStyle("xPixel",xPixel,true),
					IRect.ToString(rectDest)) );
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
					if (RString.CompareAt(sText,Environment.NewLine,iMoveMe)) {
						bBreakable=true;
						iSkipper=Environment.NewLine.Length;
						break;
					}
					else if (RString.CompareAt(sText,'\n',iMoveMe)) {
						bBreakable=true;
						iSkipper=1;
						break;
					}
					else if (RString.CompareAt(sText,'\r',iMoveMe)) {
						bBreakable=true;
						iSkipper=1;
						break;
					}
					else if (xPixel+WidthOf(sText[iMoveMe],iGlyphType)>=zone_Right) {
						int iLastBreaker=iMoveMe;
						bool bVisibleBreaker=false;
						bool bTest=RString.PreviousLineBreakerExceptNewLine(sText, ref iLastBreaker, out bVisibleBreaker);
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
					//else if (NeedsToBreakBeforeLineBreaker(sText, iMoveMe+1, xPixel, rectDest, iGlyphType) && RString.IsLineBreakerExceptNewLine(sText,iMoveMe)) {
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
				sLine=RString.SafeSubstring(sText,iStart,iMoveMe-iStart);
				if (bBreakable) iMoveMe+=iSkipper;
			}
			catch (Exception exn) {
				sLine="";
				RReporting.ShowExn(exn,"RFont ReadLine");
			}
			return HasALine;
		}
		public int GlyphHeight(int iGlyphType) {
			int iReturn=0;
			RImage riNow=Glyph('|',iGlyphType);
			if (riNow!=null) iReturn=riNow.Height;
			return iReturn;
		}
		public bool Render(ref RImage riDest, IRect rectDest, string sText, int iGlyphType) {
			return Render(ref riDest, rectDest, sText, iGlyphType, LineBreakingOnlyWhenEndOfTextLine);
		}
		public bool TypeHTML(ref RImage riDest, ref IPoint ipAt, IRect irectReturn, string sText,  bool bVisible) {
			int iNextTag;
			int iLength;
			bool bGood=true;
			try {
				//TODO: finish TypeHTML
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","TypeHTML(...,\""+sText+"\",...)");
				bGood=false;
			}
			return bGood;
		}
		public bool TypeHTML(ref RImage riDest, ref IPoint ipAt, IRect irectReturn, string sText) {
			return TypeHTML(ref riDest, ref ipAt, irectReturn, sText, true);
		}
		private bool Init() {
			bool bGood=true;
			try {
				gradNow=new RGradient();
				gradNow.From(new int[] {0,32,223,255}, new Pixel32Struct[] {new Pixel32Struct(64,64,64,0), new Pixel32Struct(0,0,0,255), new Pixel32Struct(255,255,255,255),new Pixel32Struct(255,255,255,255)});
				animarrGlyphType=new RAnim[RFont.iGlyphTypes];
				if (false==ResetGlyphTypeArray()) bGood=false;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RFont Init");
				bGood=false;
			}
			return bGood;
		}
		public bool ResetGlyphTypeArray() {
			bool bGood=false;
			try {
				animarrGlyphType=new RAnim[iGlyphTypes];
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"initializing glyphtype array","RFont Init");
				bGood=false;
			}
			return bGood;
		}
		private RAnim Normal {
			get {
				return animarrGlyphType[RFont.GlyphTypeNormal];
			}
			set {
				animarrGlyphType[RFont.GlyphTypeNormal]=value;
			}
		}
		private RAnim Bold {
			get {
				return animarrGlyphType[RFont.GlyphTypeBold];
			}
			set {
				animarrGlyphType[RFont.GlyphTypeBold]=value;
			}
		}
		private RAnim Italic {
			get {
				return animarrGlyphType[RFont.GlyphTypeItalic];
			}
			set {
				animarrGlyphType[RFont.GlyphTypeItalic]=value;
			}
		}
		private RAnim BoldItalic {
			get {
				return animarrGlyphType[RFont.GlyphTypeBoldItalic];
			}
			set {
				animarrGlyphType[RFont.GlyphTypeBoldItalic]=value;
			}
		}
		public void CalculateSpacing(int iGlyphType) {
			iPixelsSpace=(int)( (double)WidthOf('|',iGlyphType)*1.5 );
			iPixelsTab=iPixelsSpace*5;
			iLineSpacing=(int)( (double)Height*1.5 );
		}
		public bool FromFixedHeightStaggered(string sFile, int iCharHeight) {//this is named FromFixedHeightStaggered while the function it called anim.SplitFromFixedHeightStaggered
			RReporting.sLastFile=sFile+"...";
			bool bGood=false;
			RAnim animNormal=null;
			bGood=Init();
			try {
				if (!sFile.EndsWith(".png")) {
					animNormal=new RAnim();
					RImage riNormal=new RImage();
					RReporting.sLastFile=sFile+".png";
					if (!riNormal.Load(sFile+".png",4)) { //assumes 32-bit is needed
						RReporting.ShowErr("Cannot load font file","","RAnim FromFixedHeightStaggered(\""+sFile+".png\")");
					}
					
					
					bGood=animNormal.SplitFromFixedHeightStaggered(riNormal, iCharHeight);
					if (bGood) {
						Normal=animNormal;
						if (File.Exists(sFile+"-bold.png")) {
							RReporting.sLastFile=sFile+"-bold.png";
							if (animarrGlyphType[RFont.GlyphTypeBold]==null) Bold=new RAnim();
							RImage riBold=new RImage();
							riBold.Load(sFile+"-bold.png",4);//assumes 32-bit is needed
							Bold.SplitFromFixedHeightStaggered(riBold, iCharHeight);
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
					RReporting.ShowErr("RFont FromFixedHeightStaggered file must not end with extension--must have assumed png extension.");
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"Splitting Proportional Font Glyphs", "FromFixedHeightStaggered("+sFile+","+iCharHeight.ToString()+")");
			}
			return bGood;
		}
		public bool FromImageValue(string sFile, int iCharWidth, int iCharHeight, int iRows, int iColumns) {
			bool bGood=false;
			RAnim animNormal;
			RReporting.sLastFile=sFile;
			try {
				animNormal=new RAnim();
				bGood=Init();
				if (bGood) {
					bGood=animNormal.SplitFromImage32(sFile, iCharWidth, iCharHeight, iRows, iColumns);
					//animNormal.SaveSeq("etc/test/0.debug-glyph", "png");
					//RImage.RawOverlayNoClipToBig(ref riTarget, ref ipAt, ref animNormal.riFrame.byarrData, iCharWidth, iCharHeight, 4);
					if (bGood) {
						Normal=animNormal.CopyAsGray();
						Bold=Normal.Copy();
						Italic=Normal.Copy();
						BoldItalic=Normal.Copy();
						//TODO: finish modifying the Glyph Types -- italics using image manip
					}
					//else RReporting.ShowErr("RFont failed to split image","","RFont FromImageValue");//already shown by anim
					//ShowAsciiTable();
					//Console.WriteLine("Normal.ToString(true):"+Normal.ToString(true));
				}
				else RReporting.ShowErr("Couldn't initialize font glyph graphics buffer","initializing font graphics","RFont FromImageValue");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"initializing font graphics","RFont FromImageValue");
			}
			return bGood;
		}//end FromImageValue
		public bool FromImage(string sFile, int iCharWidth, int iCharHeight, int iRows, int iColumns) {
			bool bGood=false;
			RAnim animNormal;
			RReporting.sLastFile=sFile;//TODO: implement this EVERYWHERE
			try {
				animNormal=new RAnim();
				bGood=Init();
				if (bGood) {
					bGood=animNormal.SplitFromImage32(sFile, iCharWidth, iCharHeight, iRows, iColumns);
					//animNormal.SaveSeq("etc/test/0.debug-glyph", "png");
					//RImage.RawOverlayNoClipToBig(ref riTarget, ref ipAt, ref animNormal.riFrame.byarrData, iCharWidth, iCharHeight, 4);
					if (bGood) {
						Normal=animNormal;
						Bold=Normal.Copy();
						Italic=Normal.Copy();
						BoldItalic=Normal.Copy();
						//TODO: finish modifying the Glyph Types -- italics using image manip
					}
					//else RReporting.ShowErr("RFont failed to split image","","RFont FromImage");//already shown by anim
					//ShowAsciiTable();
					//Console.WriteLine("Normal.ToString(true):"+Normal.ToString(true));
				}
				else RReporting.ShowErr("Couldn't initialize font glyph graphics buffer","initializing font graphics","RFont FromImage");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"initializing font graphics","RFont FromImage");
			}
			return bGood;
		}//end FromImage
		public void ShowAsciiTable(RImage riTarget, int xAt, int yAt) {
			RReporting.sLastFile=Normal.SourceToRegExString();
			try {
				IPoint ipAt=new IPoint(xAt,yAt);
				int lChar=0;
				for (int yChar=0; yChar<16; yChar++) {
					ipAt.X=xAt;
					for (int xChar=0; xChar<16; xChar++) {
						animarrGlyphType[RFont.GlyphTypeNormal].GotoFrame(lChar);
						RImage.RawOverlayNoClipToBig(ref riTarget, ref ipAt, 
						                              ref animarrGlyphType[RFont.GlyphTypeNormal].riFrame.byarrData,
						                              animarrGlyphType[RFont.GlyphTypeNormal].riFrame.iWidth,
						                              animarrGlyphType[RFont.GlyphTypeNormal].riFrame.iHeight, 1);
						ipAt.X+=animarrGlyphType[RFont.GlyphTypeNormal].riFrame.iWidth;
						lChar++;
					}
					ipAt.Y+=animarrGlyphType[RFont.GlyphTypeNormal].riFrame.iHeight;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","ShowAsciiTable");
			}
		}//end ShowAsciiTable
		public void ValueToAlpha() {
			try {
				if (animarrGlyphType!=null) {
					for (int iNow=0; iNow<animarrGlyphType.Length; iNow++) {
						if (animarrGlyphType[iNow]!=null) {
							animarrGlyphType[iNow]=animarrGlyphType[iNow].CopyAsGray();
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","ValueToAlpha");
			}
		}
	}//end class RFont
}//end namespace
