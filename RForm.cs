/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PM
 * 
 */

using System;
using System.Drawing;

namespace ExpertMultimedia {
	
	/// <summary>
	/// Graphical node (i.e. to convert from/to a markup tag)
	/// OnClick and other html events are extracted from these objects and processed by RForms
	/// </summary>
	public class RForm {
		public RForms ParentPage=null;
		public bool bScrollable=false;//TODO: finish this - create scrollbars if exceeds parent.
		#region constants
		public const int DisplayUninitialized=-1;
		public const int DisplayNone=0;//display:none -- not displayed
		public const int DisplayBlock=1;//block - has forced break before AND after
		public const int DisplayInline=2;//inline 
		public const int DisplayListItem=3; //list-item - displayed in a list
		public const int DisplayRunIn=4;//run-in - block-level or inline depending on context
		public const int DisplayCompact=5;//compact - block-level or inline depending on context
		public const int DisplayMarker=6;//marker (?)
		public const int DisplayTable=7;//table - like <table>; has forced break before AND after
		public const int DisplayInlineTable=8;//inline-table - like <table> but inline
		public const int DisplayTableRowGroup=9;//table-row-group - a group of rows like <tbody>
		public const int DisplayTableHeaderGroup=10;//table-header-group - a group of rows like <thead>
		public const int DisplayTableFooterGroup=11;//table-footer-group - a group of rows like <tfoot>
		public const int DisplayTableRow=12;//table-row - like <tr>
		public const int DisplayTableColumnGroup=13;//table-column-group - like <colgroup>
		public const int DisplayTableColumn=14;//table-column - like <col>
		public const int DisplayTableCell=15;//table-cell - like <td> and <th>
		public const int DisplayTableCaption=16;//table-caption - like <caption>
		public int RenderItemBackground=0;
		public bool bDropDown=false;
		public bool bOpen=false;///TODO: implement this for File Browse, bDropDown
		public const int RenderItemNone=0;//nothing is rendered except non-implicit aspects (such as text, image, etc)
		public const int RenderItemButton=1;
		public const int RenderItemTextBox=2;
		//public const int RenderItemSelect=2;//dropdown box
		//public const int RenderItemSelectMultiple=3;
		//public const int RenderItemTextBox=4;
		public bool bDown=false;//button, input type=button, or select size=1 [drop down] down state //TODO: implement this -- show as pushed down
		public static void RenderItem(RImage riOutput, IRect RectItem, int RenderItemX) {
			RenderItem(riOutput,RectItem,RenderItemX,0,0,0);
		}
		private const string RenderItem_SenderString="RenderItem";
		public static int iScrollBarThickness=16;
		public static int iControlCornerRadius=3;
		private static IRect rectScrollTemp=new IRect();
		public const int MouseDownPartNone=0;
		public const int MouseDownPartUp=1;
		public const int MouseDownPartDown=2;
		public const int MouseDownPartUpFast=3;
		public const int MouseDownPartDownFast=4;
		public const int MouseDownPartSideMid=5;
		public const int MouseDownPartLeft=6;
		public const int MouseDownPartRight=7;
		public const int MouseDownPartLeftFast=8;
		public const int MouseDownPartRightFast=9;
		public const int MouseDownPartBottomMid=10;
		public const int MouseDownPartDropDown=11;
		public int LineHeight {	get { return rfont!=null?rfont.iLineSpacing+(iControlCornerRadius*2):(RFont.rfontDefault.iLineSpacing); } } //aka FontHeight
		///<summary>
		///IF necessary, draws scrollbars and modifies rects.
		///rectSrc: the source rectangle of the object to be drawn, made
		/// smaller if scrollbars are necessary.
		///RectAbsolute: the destination rectangle in the output image, NOT modified
		///</summary>
		private void DrawScrollBars(RImage riOutput, IRect RectAbsolute, bool MouseIsDownOnIt, int ScrollBarMouseDownPart_ElseZero) {
			int BarX2=iScrollBarThickness*2;
			if (riOutput!=null&&RectAbsolute!=null&&rectSrc!=null) {
				if (bDropDown) {
					rectScrollTemp.Width=iScrollBarThickness;
					rectScrollTemp.Height=LineHeight;
					rectScrollTemp.Y=RectAbsolute.Y;
					rectScrollTemp.X=(RectAbsolute.Y+RectAbsolute.Width)-iScrollBarThickness;
					rectSrc.Width-=iScrollBarThickness;
					RenderItem(riOutput,rectScrollTemp,RenderItemButton,bOpen||ScrollBarMouseDownPart_ElseZero==MouseDownPartDropDown);

					//TODO: finish this
					// 5 lines alternating between 110.6%,61.5%
					//-expand RectRelative so renderer will know to draw scrollable subitems
					//-draw scrollbars if bDown
					RenderArrow(riOutput, rectScrollTemp.X, rectScrollTemp.Y, rectScrollTemp.Width, rectScrollTemp.Height, RenderArrowDown);
					yNow+=RForms.iControlArrowLength;
					//stripes below down arrow:
					int yEndBefore=yNow+5;
					int ArrowSideStart=(iScrollBarThickness-RForms.iControlArrowWidth)/2;
					bool bDark=false;
					while (yNow<yEndBefore) {
						riOutput.DrawHorzLine(bDark?RForms.BrushControlStripeDark:RForms.BrushControlStripeLight,ArrowSideStart,yNow,RForms.iControlArrowWidth,"DrawScrollBars");
						yNow++;
						bDark=!bDark;
					}
				}//end if bDropDown
				if (rectSrc.Width<RectAbsolute.Width) { ///TODO: ok since rectSrc dimensions and RectAbsolute location should be rendered by renderer
					//draw horizontal scrollbar
					int iMoverBlockContainerSize=rectSrc.Width-BarX2;
					if (iMoverBlockContainerSize<0) iMoverBlockContainerSize=0;
					int iMoverBlockSize=RMath.SafeDivideRound(RMath.SafeMultiply(rectSrc.Width,iMoverBlockContainerSize),RectAbsolute.Width);
					rectSrc.Height-=iScrollBarThickness;///done LAST (remove scrollbar area)
				}
			}//end if all non-null rects and dest
		}//end DrawScrollBars
		///<summary>
		///Additional vars are used when RenderItemX is RenderItemScrollH or RenderItemScrollV
		///</summary>
		public static RBrush brushTemp=new Brush();
		public const int RenderArrowRight=0;
		public const int RenderArrowUp=1;
		public const int RenderArrowLeft=2;
		public const int RenderArrowDown=3;
		///<summary>
		///RenderArrowDirectionCartesianDividedBy90: angle divided by 90 (must be 0[right], 1[up], 2[left] or 3[down])
		///</summary>
		public static void RenderArrow(RImage riOutput, int containerbutton_x, int containerbutton_y, int containerbutton_width, int containerbutton_height, int RenderArrowDirectionCartesianDividedBy90) {
			try {
				int iDirAdder=(RenderArrowDirectionCartesianDividedBy90==RenderArrowRight||RenderArrowDirectionCartesianDividedBy90==RenderArrowDown)?1:-1;
				bool bVertical=(RenderArrowDirectionCartesianDividedBy90==RenderArrowUp||RenderArrowDirectionCartesianDividedBy90==RenderArrowDown);
				//if (bVertical) {
					int iArrowWidthNow=RForms.iControlArrowWidth;
					int iBlackWidthNow=RForms.iControlArrowBlackWidth;
					int xDarkNow=(bVertical?(containerbutton_x+(containerbutton_width-RForms.iControlArrowWidth)/2):(containerbutton_y+(containerbutton_height-RForms.iControlArrowWidth)/2));//iScrollBarThickness/4;//truncated not rounded, since size is rounded
					int ArrowSideStart=xDarkNow;
					int xBlackNow=xDarkNow+1;
					int xLightNow=xDarkNow+iBlackWidthNow;
					int iMin=(iArrowWidthNow%2==0)?2:1;
					int yNow=(bVertical?RectAbsolute.Y:RectAbsolute.X)+RMath.IRound((float)(bVertical?containerbutton_height:containerbutton_width)*0.4375f);//RMath.IRound((float)RectAbsolute.Y*.35f);
					//down arrow:
					while (iArrowWidthNow>=iMin) {
						if (bVertical) {
							riOutput.SetPixel(RForms.BrushControlSymbolShadowDark,xLightNow,yNow);
							riOutput.DrawHorzLine(RForms.BrushControlSymbol,xBlackNow,yNow,iBlackWidthNow,"DrawScrollBars");
							riOutput.SetPixel(RForms.BrushControlSymbolShadowLight,xDarkNow,yNow);
						}
						else {
							riOutput.SetPixel(RForms.BrushControlSymbolShadowDark,yNow,xDarkNow);
							riOutput.DrawVertLine(RForms.BrushControlSymbol,yNow,xBlackNow,iBlackWidthNow,"DrawScrollBars");
							riOutput.SetPixel(RForms.BrushControlSymbolShadowLight,yNow,xLightNow);
						}
						xDarkNow++;
						xBlackNow++;
						xLightNow--;
						iArrowWidthNow-=2;
						iBlackWidthNow-=2;
						yNow+=iDirAdder;
					}
				//}//end if vertical
				//else {
				//}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
		}//end RenderArrow
		public static void RenderItem(RImage riOutput, IRect RectItem, int RenderItemX, bool MouseIsDownOnItem) {//, int SizeOuter, int SizeShown, int AmountScroll) {
			RBrush brushSwap=brushTemp;
			//TODO: cropping
			if (riOutput!=null&&RectItem!=null) {
				//string sSender="RenderItem(RImage,IRect,RenderItem="+RenderItemX.ToString()+",MouseIsDownOnItem="+RConvert.ToString(MouseIsDownOnItem)+",...)";
				int RectItem_Bottom_Inclusive=RectItem.BottomInclusive;
				int RectItem_Right_Inclusive=RectItem.RightInclusive;
				int LeftShrink1=RectItem.X+1;
				int TopShrink1=RectItem.Y+1;
				//int RectItem_Width_minus_2=RectItem.Width-2;
				if (RenderItemX==RenderItemButton||RenderItemX==RenderItemTextBox) {	//render rounded black outline
					//draw rounded non-antialiased black border (corners are drawn later below):
					riOutput.DrawHorzLine(RForms.brushControlLine,RectItem.X+2,RectItem.Y,RectItem.Width-4,RenderItem_SenderString);
					riOutput.DrawHorzLine(RForms.brushControlLine,RectItem.X+2,RectItem_Bottom_Inclusive,RectItem.Width-4,RenderItem_SenderString);
					riOutput.DrawVertLine(RForms.brushControlLine,RectItem.X,RectItem.Y+2,RectItem.Height-4,RenderItem_SenderString); //Left side
					riOutput.DrawVertLine(RForms.brushControlLine,RectItem_Right_Inclusive,RectItem.Y+2,RectItem.Height-4,RenderItem_SenderString); //Right side
				}//more drawn after cases below
				if (RenderItemX==RenderItemButton) { //brushControl 107% to 90%
					if (RenderItemX==RenderItemButton) {
						//draw gradient inner part:
						riOutput.DrawGradTopDownRectFilled(RForms.brushControlGradientLight,brushControlGradientDark,LeftShrink1, TopShrink1, RectItem.Width-2, RectItem.Height-2, RenderItem_SenderString);
					}
					else {//set brush to sunken shadow color, and draw inner solid background
						riOutput.DrawRectFilled(RForms.brushTextBoxBack,LeftShrink1, TopShrink1, RectItem.Width-2, RectItem.Height-2, RenderItem_SenderString);
					}
					if (RenderItemX==RenderItemButton&&MouseIsDownOnItem) {
						riOutput.SetPixel(RForms.brushControl,LeftShrink1,TopShrink1);
						brushSwap=RForms.brushControlInsetShadow;
					}
					else {//draw up
						if (RenderItemX==RenderItemButton) brushSwap=RForms.brushButtonShine;
						else brushSwap=RForms.brushTextBoxBackShadow;
						riOutput.DrawHorzLine(brushSwap,LeftShrink1, RectItem.Y+2, 2, RenderItem_SenderString); //2 pixel line for left fx
						riOutput.DrawVertLine(brushSwap,LeftShrink1, RectItem.Y+3, RectItem.Height-5, RenderItem_SenderString); //inner left line
						riOutput.SetPixel(brushSwap,RectItem.X+2,RectItem_Bottom_Inclusive-1);
					}//end draw up
					//common (for button, whether up or down) lines:
					riOutput.DrawHorzLine(brushSwap,RectItem.X+2, TopShrink1, RectItem.Width-4, RenderItem_SenderString); //inner top line
					riOutput.DrawHorzLine(brushSwap,RectItem_Right_Inclusive-2, RectItem.Y+2, 2, RenderItem_SenderString); //2 pixel line for right fx
					riOutput.DrawVertLine(brushSwap,RectItem_Right_Inclusive-1, RectItem.Y+3, RectItem.Height-5, RenderItem_SenderString); //inner right line
					riOutput.SetPixel(brushSwap,RectItem_Right_Inclusive-2,RectItem_Bottom_Inclusive-1);
				}//end if RenderItemButton
				//end RenderItem* types
				if (RenderItemX==RenderItemButton||RenderItemX==RenderItemTextBox) {
					//draw dots on top of shaded area for inner pixel of rounded corners
					riOutput.SetPixel(RForms.brushControlLine,LeftShrink1,TopShrink1); //TL dot
					riOutput.SetPixel(RForms.brushControlLine,RectItem_Right_Inclusive-1,TopShrink1); //TR dot
					riOutput.SetPixel(RForms.brushControlLine,LeftShrink1,RectItem_Bottom_Inclusive-1); //BL dot
					riOutput.SetPixel(RForms.brushControlLine,RectItem_Right_Inclusive-1,RectItem_Bottom_Inclusive-1); //BR dot
				}
			}
			else RReporting.ShowErr("Null destination","rendering interface item",String.Format("RenderItem{{RImage:{0}; IRect:{1}}}",RImage.ToString(riOutput),IRect.ToString(RectItem)) );
		}//RenderItem
		///<summary>
		///(called by rforms UpdateAll)
		///Calculates display vars such as display attribute index (specified or inferred),
		/// RenderItemBackground (such as RenderItemButton); 
		/// creates textbox (multi- or single-line) if necessary
		///</summary>
		public void CalculateDisplayMethodVars(string TagwordX, string TypePropertyValue, string DisplayAttribValue) {
			DisplayMethodIndex=DisplayUninitialized;
			if (TagwordX!=null) TagwordX=TagwordX.ToLower();
			if (TypeX!=null) TypeX=TypeX.ToLower();
			if (DisplayAttribValue!=null) DisplayAttribValue=DisplayAttribValue.ToLower();
			DisplayMethodIndex=DisplayInline; //default
			RenderItemBackground=RenderItemNone; //default
			if ( TagwordX=="button" || (TagwordX=="input"&&(TypePropertyValue=="submit"||TypePropertyValue=="button"||TypePropertyValue=="reset")) ) {
				RenderItemBackground=RenderItemButton;
			}
			else if (TagwordX=="input") {
				if (TypeX=="text") {
					RenderItemBackground=RenderItemTextBox;
					textbox=new RTextBox(this,1,false);
				}
				else if (TypeX=="select") {
					string sSize=GetProperty("size");
					int iSize=sSize!=null?RConvert.ToInt(sSize):1;
					RenderItemBackground=RenderItemTextBox;
					if (iSize<2) bDropDown=true;
					//NOTE: dropdown arrow is drawn by DrawScrollBars
					//if (iSize<2) RenderItemBackground=RenderItemSelect;//drop-down
					//else RenderItemBackground=RenderItemSelectMultiple;
				}
			}
			else if (TagwordX=="textarea") {
				RenderItemBackground=RenderItemTextBox;
				textbox=new RTextBox(this,32,true);
			}
		}//end CalculateDisplayMethodVars
/* ///TODO:
//from <http://www.w3.org/TR/REC-html40/interact/forms.html> 2008-11-24
<!ELEMENT FORM - - (%block;|SCRIPT)+ -(FORM) -- interactive form -->
<!ATTLIST FORM
  %attrs;                              -- %coreattrs, %i18n, %events --
  action      %URI;          #REQUIRED -- server-side form handler --
  method      (GET|POST)     GET       -- HTTP method used to submit the form--
  enctype     %ContentType;  "application/x-www-form-urlencoded"
  accept      %ContentTypes; #IMPLIED  -- list of MIME types for file upload --
  name        CDATA          #IMPLIED  -- name of form for scripting --
  onsubmit    %Script;       #IMPLIED  -- the form was submitted --
  onreset     %Script;       #IMPLIED  -- the form was reset --
  accept-charset %Charsets;  #IMPLIED  -- list of supported charsets --
  >

*/
		public const uint bitHasWidth=1;
		public const uint bitHasHeight=2;
		public const uint bitWidthIsPercent=4;
		public const uint bitHeightIsPercent=8;
		public const uint bitOnClick=16;
		public const uint bitOnMouseover=32;
		public const uint bitOnMousedown=64;
		public const uint bitOnMouseup=128;
		public const uint bitOnDragDrop=256;
		#endregion constants

		#region required vars
		//public string sPreText="";//text before the tag area--this was only needed for text before an opening (i.e. "<html") tag--this is no longer needed since Node(0) is now the tagless (no opener, closer, or post-text) root node and contains any data preceding "<html" as it's sContent.
		public string sOpener="";//ALL data including opening '<' and closing '>' OR "/>"
		public string sContent="";//text AFTER sOpener & before children //formerly sInnerText
		public string sCloser="";//closing tag after children, & opening '</' & closing '>' IF ANY
		public string sPostText="";//any data after closing tag (or after self-closing sOpener), including newlines,
		//RForm rformParent=null;
		public int iIndex=-1;//index in the RForms.rformarr array
		public int ParentIndex=-1;
		private int iSubNodes=0;
		//public int indexParent; //index in RFormr
		public bool bLeaf; //whether this can be drawn as text
		private int DisplayMethodIndex=DisplayUninitialized; //fixed by CalculateDisplayMethodVars
		#endregion required vars

		#region optional vars
		//public int iComponentIDOfOwner;
		  //which application (i.exn. retroengineclient or website)
		  //owns and maintains this RForm
		public int iLengthChildren=0;
		public int Length {
			get {
				return RString.SafeLength(sPreText)+RString.SafeLength(sOpener)+RString.SafeLength(sContent)+iLengthChildren+RString.SafeLength(sCloser);
			}
			set {
				iLengthChildren=value-(RString.SafeLength(sPreText)+RString.SafeLength(sOpener)+RString.SafeLength(sContent)+RString.SafeLength(sCloser));
				if (iLengthChildren<0) {
					iLengthChildren=0;
					Base.ShowErr("Tried to set rform length to but the value must be greater than or equal to the sum of the PreText, Opener, Content, and Closer (the child tags are all that is left, the length of which is set by the value sent here minus the sum of the strings listed above).","","rform set Length="+value.ToString());
				}
			}
		}//end Length get/set
		public uint bitsAttrib=0;
		private bool bChangedThis=false;
		public bool bChanged {
			get {
				if (textbox!=null) return textbox.bChanged;
				else return bChangedThis;
			}
			set {
				if (textbox!=null) textbox.bChanged=value;
				else bChangedThis=value;
			}
		}
		public Var vsFriend=null; //the Variable in sgmldoc.vsRoots (or descendant) that this RForm represents
		public int iCursor;
		public int iSelStart;
		public int iSelLen;
		private RTextBox textbox=null;//version of sText for TextArea
		public bool Visible {
			get {
				return StyleAttribEqualsI_AssumingNeedleIsLower("visibility","hidden");
			}
			set {
				SetStyleAttrib("visibility",value?"visible":"hidden");
			}
		}
		public string ParentName {
			get { return Parent!=null?Parent.Name:""; }
		}
		public RForm Parent {
			get {
				if (ParentPage!=null) {
					if (iNodeIndex>-1) return ParentPage.Node(ParentIndex);
					else RReporting.Warning("Bad saved self-index in an RForm (RForms corruption)--will not be able to get parent object");
				}
				else RReporting.ShowErr("Null ParentPage for an RForm (RForms corruption while getting Parent object)");
				return null;
			}
		}
		#endregion optional vars

		#region cached variables
		public int iChildren=0;
		public RFont rfont=null;
		public bool bRendered=false;//flipped each time rendered.
		public string sToolTip=""; //TODO: cache tooltip
		public bool bSplit=false; //Whether was ever processed by DivideNode (parser)
		public IRect rectAbs=null; //absolute screen position derived from CascadedFuzzyAttrib("width") and sqeezed by margin (then content can push it outward)
		public IRect rectSrc=new IRect();///TODO: finish this //the source rect for rendering a scrollable item
		public IZone zoneInner=null; //absolute screen position derived from margin styles (used by child; or by inner text if IsLeaf)
		public Pixel32 pxBack=new Pixel32(0,255,255,255);
		public Pixel32 pxFore=new Pixel32(255,0,0,0);
		private string sTagwordLower=null;
		public string TagwordLower {
			get {
				if (sTagwordLower==null) {
					int iAbs=RString.IndexOfNonWhiteSpaceNorChar(sOpener,0,'<');
					if (iAbs>-1) {
						iEnder=RString.IndexOfWhiteSpaceOrChar(sOpener,iAbs,'>');
						if (iEnder>-1) {
							sTagwordLower=RString.SafeSubstring(sOpener,iAbs,iEnder-iAbs);
						}
					}
					if (sTagwordLower!=null) sTagwordLower=sTagwordLower.ToLower();
					else {
						RReporting.Warning("Set lower tagword cache to blank");
						sTagwordLower="";
					}
				}//end if sTagwordLower==null then create sTagwordLower
				return sTagwordLower;
			}
		}//end get/set TagwordLower
		private string sTypeLower=null;
		public string TypeLower {
			get {
				if (sTypeLower==null) {
					sTypeLower=GetProperty("type");
					if (sTypeLower!=null) sTypeLower=sTypeLower.ToLower();
					else sTypeLower="";
				}//end if sTypeLower==null then create sTypeLower
				return sTypeLower;
			}
		}//end get/set TypeLower
// 		private string sNameLower=null;
// 		public string NameLower {
// 			get {
// 				if (sNameLower==null) {
// 					sNameLower=GetProperty("name");
// 					if (sNameLower!=null) sNameLower=sNameLower.ToLower();
// 					else sNameLower="";
// 				}//end if sNameLower==null then create sNameLower
// 				return sNameLower;
// 			}
// 		}//end get/set NameLower
		private string sValueOrContent=null;
		public string ValueOrContent {
			get {
				if (sValueOrContent==null) {
					sValueOrContent=GetProperty("value");
					if (!HasProperty("value")) sValueOrContent=sContent;
					if (sValueOrContent==null) sValueOrContent="";
				}
				return sValueOrContent;
			}
		}//end get/set ValueOrContent
		public bool ValueTagIsContent {
			get { return TagwordLower=="input"||TagwordLower=="button"; }
		}
		private string Text {//formerly sText
			get {
				//TODO: finish this -- make textbox automatically update value or sContent as needed
				if (ValueTagIsContent) {
					if (textbox!=null) SetProperty("value",RString.ToHtmlValue(textbox.ToString()));
					return GetProperty("value");
				}
				else {
					if (textbox!=null) sContent=RString.ToHtmlValue(textbox.ToString());
					return sContent; //TODO: for sgml: (only used if root !bUpdateHTML, otherwise root node gets it from an sgmlNow.Substring(...))
				}
			}
			set {
				if (textbox!=null) textbox.SetData(value);
				//do this separately:
				if ValueTagIsContent) SetProperty("value",value);
				else sContent=value;
			}
		}
		#endregion cached variables
		
		//public Variable vParent=null; //TODO: implement this (create Variable [created from HTML] "re-parser") has links to sourcecode etc
		
		#region variables, rendering (framework Graphics) vars
		public Color colorBack=SystemColors.Window;
		//public SolidBrush brushBack=new SolidBrush(SystemColors.Window);
		//public System.Drawing.Pen penBack=new System.Drawing.Pen(SystemColors.Window);
		//Color color=Color.Black;
		//SolidBrush brush=new SolidBrush(Color.Black);
		Color colorTextNow=Color.Black;
		//System.Drawing.Pen pen=new System.Drawing.Pen(Color.Black);
		//System.Drawing.Font font=new Font("Andale Mono",9);//default monospaced font
		//FontFamily fontfamily = new FontFamily("Andale Mono");
		#endregion variables, rendering (framework Graphics) vars
		
		#region variables, other
		public int Left { get {return X; } }
		public int Top { get {return Y; } }
		public int X { get {return (rectAbs!=null)?rectAbs.X:0; } }
		public int Y { get {return (rectAbs!=null)?rectAbs.Y:0; } }
		public int XInner { get { return (zoneInner!=null)?zoneInner.Left:0; } }
		public int YInner { get { return (zoneInner!=null)?zoneInner.Top:0; } }
		public float HeightF { get { return (float)Height; } }
		public float WidthF { get { return (float)Width; } }
		public float HeightInnerF { get { return (float)HeightInner; } }
		public float WidthInnerF { get { return (float)WidthInner; } }
		public int Width {
			get { return (rectAbs!=null)?rectAbs.Width:0; }
			set { if (rectAbs!=null) rectAbs.Width=value; }
		}
		public int Height {
			get { return (rectAbs!=null)?rectAbs.Height:0; }
			set { if (rectAbs!=null) rectAbs.Height=value; }
		}
		public int WidthInner {
			get { return (zoneInner!=null)?(zoneInner.Right-zoneInner.Left):0; }
		}
		public int HeightInner {
			get { return (zoneInner!=null)?(zoneInner.Bottom-zoneInner.Top):0; }
		}
		public float CenterVF {
			get {
				if (zoneInner!=null) return (float)zoneInner.Top+HeightInnerF/2.0f;
				else return HeightF/2.0f;
			}
		}
		public float CenterHF {
			get {
				if (zoneInner!=null) return (float)zoneInner.Left+WidthInnerF/2.0f;
				else return WidthF/2.0f;
			}
		}
		public int Index {//index in rformr
			set {
				if (iIndex<0) iIndex=value;
				else RReporting.ShowErr("Index of Node "+iIndex.ToString()+" (\""+Name+"\") was already set and can't be set again to "+value.ToString()+".");
			}
			get {
				return iIndex;
			}
		}
		public string Name {
			get {
				return GetProperty("name"); //IS case-insensitive
			}
			set {
				SetProperty("name",value);
			}
		}
		public int TextLength {
			get {
				int iReturn=0;
				try {
					if (textbox!=null) iReturn=textbox.Length; //iType==TypeTextArea) iReturn=textbox.Length; //TODO: make sure textbox is set to null if type changes, and make sure it changes mode if input type changes
					else return sText.Length;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"","RForm TextLength");
					iReturn=-1;
				}
				return iReturn;
			}
		}
		public bool IsLeaf {
			get {
				return iSubNodes<=0;//rformarr==null;
			}
		}
		public bool IsRoot {
			get {
				return ParentIndex<0;//rformParent==null;
			}
		}
		public string Text {
			get {
				if (textbox!=null) {//iType==TypeTextArea) {
					//if (textbox!=null)
						return textbox.ToString("\n");//TODO: debug whether to use \n here
					//else {
					//	RReporting.ShowErr("TextArea rtextbox was null upon trying to get text!");
					//	return "";
					//}
				}
				else return sText;
			}
			set {
				//try {
					//if (iType==TypeTextArea) {
						//if (textbox==null) textbox=new RTextBox(this,1);
						if (textbox!=null) textbox.SetText(value);
					//}
					else sText=value;
				//}
				//catch (Exception exn) {
					//RReporting.ShowExn(exn,"setting text","RForm set Text to "+(value!=null?(value!=""?"\""+value+"\"":"\"\""):"null")+" for interface node");
				//}
			}
		}
		public bool Undo() {
			bool bGood=false;
			if (textbox!=null) bGood=textbox.Undo();
			return bGood;
		}
		public bool SelectAll() {
			bool bGood=false;
			if (textbox!=null) bGood=textbox.SelectAll();
			return bGood;
		}
		public int ParentGlyphType {
			get { return RFont.GlyphTypeNormal; } //TODO: implement parent font using parent's font AND cascade THIS one's style attributes overtop of that result
		}
		public bool SetSelectionFromPixels(int xStart, int yStart, int xEnd, int yEnd) {
			bool bGood=false;
			if (textbox!=null&&rfont!=null) {
				int iSetLineStart=rfont.YPixelToLine(yStart);
				int iSetLineEnd=rfont.YPixelToLine(yEnd);
				SetSelection(iSetLineStart,rfont.XPixelToChar(xStart,textbox.Line(iSetLineStart),ParentGlyphType),iSetLineEnd,rfont.XPixelToChar(xEnd,textbox.Line(iSetLineEnd),ParentGlyphType));
				//y then x intentionally -- for row,col ordering
				//Console.Error.WriteLine( String.Format("SetSelection(row {0}, col {1}, rowend {2}, colend{3})",,,,) );//debug only
			}
			return bGood;
		}
		public bool SetSelection(int iSetRowStart, int iSetColStart, int iSetRowEnd, int iSetColEnd) {
			bool bGood=false;
			if (textbox!=null) bGood=textbox.SetSelection(iSetRowStart, iSetColStart, iSetRowEnd, iSetColEnd);
			else RReporting.ShowErr("Tried to set selection but a text node is not activated");
			return bGood;
		}
// 		public string sLastErr=null;
// 		public void ClearErr() {
// 			sLastErr=null;
// 		}
		//RForm[] rformarr;//COPY of REFERENCE to objects
		//public int MAXBRANCHES {
		//	get {
		//		try {
		//			if (rformarr==null) return 0;
		//			else return rformarr.Length;
		//		}
		//		catch (Exception exn) {
		//			RReporting.ShowExn(exn,"getting MAXBRANCHES","");
		//		}
		//	return 0;
		//	}
		//	set {
		//		try {
		//			if (value<=0) {
		//				if (value<MAXBRANCHES) Base.WriteLine("shrinking "+(IsLeaf?"a leaf":(IsRoot?"root":"a"))+" rform's MAXBRANCHES to "+value.ToString()+" (sub array set to null)");
		//				rformarr=null;
		//			}
		//			else {
		//					RForm[] rformarrNew=new RForm[value];
		//					if (value<MAXBRANCHES) Base.WriteLine("shrinking "+(IsLeaf?"a leaf":(IsRoot?"root":"a"))+" rform's MAXBRANCHES");
		//					int iMin=MAXBRANCHES<value?MAXBRANCHES:value;
		//					for (int iNow=0; iNow<value; iNow++) {
		//						if (iNow<iMin) rformarrNew[iNow]=rformarr[iNow];
		//						else rformarrNew[iNow]=null;
		//					}
		//					rformarr=rformarrNew;
		//			}
		//		}
		//		catch (Exception exn) {
		//			MAXBRANCHES=0;
		//			RReporting.ShowExn(exn,"setting MAXBRANCHES","");
		//		}
		//	}
		//}
		#endregion variables, other
		
		
		#region constructors
		public RForm() {
			Init(0,0,"","",0,0,0,0);
			RReporting.Warning("The default RForm constructor was used--the ParentPage must still be set!");
		}
		public RForm(RForms SetParentPage) {
			ParentPage=SetParentPage;
			Init(0,0,"","",0,0,0,0);
		}
		//public RForm(RImage riSurface) {
		//	Init(riSurface);
		//}
		public RForm(int iSetParentNode, int RFormType, string sSetName, string sSetText, IRect rectSetAbsToCopy) {
			Init(iSetParentNode, RFormType, sSetName, sSetText, rectSetAbsToCopy.X,rectSetAbsToCopy.Y,rectSetAbsToCopy.Width,rectSetAbsToCopy.Height, ""); //IndexParent, 
		}
		public RForm(int iSetParentNode, int RFormType, string sSetName, string sSetText, int xLoc, int yLoc, int Width, int Height) {
			Init(iSetParentNode, RFormType, sSetName, sSetText, xLoc, yLoc, Width, Height, ""); //IndexParent, 
		}
		public RForm(int iSetParentNode, int RFormType, string sSetName, string sSetText, int xLoc, int yLoc, int Width, int Height, string HTMLTag) {
			Init(iSetParentNode, RFormType, sSetName, sSetText, xLoc, yLoc, Width, Height, HTMLTag); //IndexParent, 
		}
		public void Init(int iSetParentNode, int RFormType, string sSetName, string sSetText, int xLoc, int yLoc, int iSetWidth, int iSetHeight, string HTMLTag) { //int IndexParent,
			ParentIndex=iSetParentNode;//rformParent=rformSetParent;
			iType=RFormType;
			rectAbs=new IRect();
			rectAbs.X=xLoc;
			rectAbs.Y=yLoc;
			rectAbs.Width=iSetWidth;
			rectAbs.Height=iSetHeight;
			IRect rectTemp=rectAbs.Copy();
			//rectTemp.X++;
			//rectTemp.y++;
			//rectTemp.Width-=2;
			//rectTemp.Height-=2;
			zoneInner=new IZone(rectTemp);
			if (RFormType==TypeTextArea) textbox=new RTextBox(this,1);
			if (RFormType==TypeTextEdit) textbox=new RTextBox(this,1,false);
			sText=sSetText; //not used unless using !rformrRoot.bUpdateHTML
			//indexParent=IndexParent;
			SetTag(sHTMLTag);
			//rformarr=null;
			iSubNodes=0;
			iCursor=0;
			iSelStart=0;
			iSelLen=0;
			rfont=RFont.rfontDefault;
			Visible=true;
			//MAXBRANCHES=2;//debug performance since low maximum can cause frequent automatic resize
		}//end Init(...)
		//public void Init(RImage riSurface) {
		//	int iSetWidth=0;
		//	int iSetHeight=0;
		//	try {
		//		iSetWidth=riSurface.iWidth;
		//		iSetHeight=riSurface.iHeight;
		//	}
		//	catch (Exception exn) {
		//		RReporting.ShowExn(exn,"setting node by surface","RForm Init");
		//	}
		//	Init(-1,RForm.TypePlane,"",0,0,iSetWidth,iSetHeight);
		//}
		public void Init(int iSetParentNode, int RFormType, string sSetName, string sSetText) {
			Init(iSetParentNode,RFormType, sSetName, sSetText,0,0,0,0,"");
		}
		public void Init(int iSetParentNode, int RFormType, string sSetName, string sSetText, IRect rectSetAbsToCopy) { //int IndexParent,
			Init(iSetParentNode, RFormType, sSetName, sSetText, rectSetAbsToCopy.X, rectSetAbsToCopy.Y, rectSetAbsToCopy.Width, rectSetAbsToCopy.Height, "");
		}
		public void Init(int iSetParentNode, int RFormType, string sSetName, string sSetText, int xLoc, int yLoc, int iSetWidth, int iSetHeight) { //int IndexParent,
			Init(iSetParentNode, RFormType, sSetName, sSetText, xLoc, yLoc, iSetWidth, iSetHeight, "");
		}
		
		#endregion constructors
		
		#region input
		public bool Backspace() {
			if (iType==TypeTextArea) return textbox.Backspace();
			else {
				RReporting.ShowErr("Backspace is only implemented for TextArea");
				return false;
			}
		}
		public bool Delete() {
			if (iType==TypeTextArea) return textbox.Delete();
			else {
				RReporting.ShowErr("Delete is only implemented for TextArea");
				return false;
			}
		}
		public bool Return() {
			if (iType==TypeTextArea) return textbox.Return();
			else {
				RReporting.ShowErr("Entering a line return is only implemented for TextArea");
				return false;
			}
		}
		//public bool InsertLine(int iLine, string sLine) {
		//	if (iType==TypeTextArea) return textbox.InsertLine(iLine,sLine);
		//	else {
		//		RReporting.ShowErr("InsertLine is only implemented for TextArea");
		//		return false;
		//	}
		//}
		//public bool RemoveLine(int iLine) {
		//	if (iType==TypeTextArea) return textbox.RemoveLine(iLine);
		//	else {
		//		RReporting.ShowErr("RemoveLine is only implemented for TextArea");
		//		return false;
		//	}
		//}
		//public bool AddLine(string sLine) {
		//	if (iType==TypeTextArea) return textbox.AddLine(sLine);
		//	else {
		//		RReporting.ShowErr("AddLine is only implemented for TextArea");
		//		return false;
		//	}
		//}
		public bool Insert(char cToInsertAtCursor) { //formerly EnterText
			//rformrRoot.sgmlNow.InsertText(Char.ToString(cToInsertAtCursor));
			if (iType==TypeTextArea) return textbox.Insert(char.ToString(cToInsertAtCursor));
			else {
				RReporting.ShowErr("EnterText(char) is only implemented for TextArea");
				return false;
			}
		}
		public bool Insert(string sToInsertAtCursor) { //formerly EnterText
			//rformrRoot.sgmlNow.InsertText(sToInsertAtCursor); //TODO: must shift all variables of all necessary nodes
			if (iType==TypeTextArea) return textbox.Insert(sToInsertAtCursor);
			else {
				RReporting.ShowErr("EnterText(string) is only implemented for TextArea");
				return false;
			}
		}
		public bool SetTextAreaSelection(int iRowStart, int iColStart, int iRowEnd, int iColEnd) {
			if (iType==TypeTextArea) return textbox.SetSelection(iRowStart,iColStart,iRowEnd,iColEnd);
			else {
				RReporting.ShowErr("SetTextAreaSelection(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool ShiftSelection(int iRows, int iCols, bool bWithShiftKey) {
			if (iType==TypeTextArea) return textbox.ShiftSelection(iRows,iCols,bWithShiftKey);
			else {
				RReporting.ShowErr("ShiftSelection(...,bool) is only implemented for TextArea");
				return false;
			}
		}
		public bool ShiftSelection(int iRows, int iCols) {
			if (iType==TypeTextArea) return textbox.ShiftSelection(iRows,iCols);
			else {
				RReporting.ShowErr("ShiftSelection(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool GrowSelection(int iRows, int iCols) {
			if (iType==TypeTextArea) return textbox.GrowSelection(iRows,iCols);
			else {
				RReporting.ShowErr("GrowSelection(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool Home(bool bWithShiftKey) {
			if (iType==TypeTextArea) return textbox.Home(bWithShiftKey);
			else {
				RReporting.ShowErr("Home(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool End(bool bWithShiftKey) {
			if (iType==TypeTextArea) return textbox.End(bWithShiftKey);
			else {
				RReporting.ShowErr("End(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool SetTextAreaCursor(int iRow, int iCol) {
			if (iType==TypeTextArea) return textbox.SetCursor(iRow,iCol);
			else {
				RReporting.ShowErr("SetTextAreaCursor(...) is only implemented for TextArea");
				return false;
			}
			//try {
			//	textbox.SetZeroSelection(iRow,iCol);
			//}
			//catch (Exception exn) {
			//	RReporting.ShowExn(exn,"SetTextAreaCursor("+iRow.ToString+","+iCol.ToString()+")");
			//}
		}
		public bool Clear() {
			if (iType==TypeTextArea) return textbox.Clear();
			else {
				RReporting.ShowErr("Clear is only implemented for TextArea");
				return false;
			}
		}
		public bool SetLines(string[] SetLines) {
			if (iType==TypeTextArea) return textbox.SetLines(SetLines);
			else {
				RReporting.ShowErr("SetLines is only implemented for TextArea");
				return false;
			}
		}
		#endregion input
		
		
		
		#region utilities
		///<summary>
		///Compares the tagword of this node to a substring of val (case-insensitive)
		///</summary>
		public bool TagwordEquals(string val, int startInVal, int endbeforeInVal) {
			return RString.CompareAtI_AssumingNeedleIsLower(val,TagwordLower,startInVal,endbeforeInVal);
// 			if (sOpener!=null) {
// 				int iAbs=RString.IndexOfNonWhiteSpaceNorChar(sOpener,0,'<');
// 				if (iAbs>-1) {
// 					iEnder=RString.IndexOfWhiteSpaceOrChar(sOpener,iAbs,'>');
// 					if (iEnder>-1) {
// 						if ((endbeforeInVal-startInVal)==(iEnder-iAbs)) {
// 							for (int iInVal=startInVal; iInVal<endbeforeInVal; iInVal++) {
// 								if (!RString.EqualsI(sOpener[iAbs],val[iInVal])) return false;
// 								iAbs++;
// 							}
// 							return true;
// 						}
// 					}
// 				}
// 			}
// 			return false;
		}//end TagwordEquals(string,start,endbefore)
		public bool TagwordEquals(string val) {
			if (val!=null) return TagwordEquals(val,0,val.Length);
			return false;
		}
		public bool StyleSetOrCreate(string sName, string sValue) {
			try {
				//TODO: set by modifying sTag 
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
		}//end StyleSetOrCreate
		/* TODO:?
		public void EnterTextCommand(char cAsciiCommand) {
			rformrRoot.sgmlNow.InsertTextCommand(cAsciiCommand);
		}
		*/
		/// <summary>
		/// Return
		/// </summary>
		/// <returns>Returns text but only if IsLeaf.</returns>
		public string MyText() {
			if (IsLeaf) {
				//if (!rformrRoot.sgmlNow.bUpdateCousin) {
					return sText;
				//}
			}
			return "";
		}
		public bool Contains(int xAt,int yAt) {
			if (zoneInner!=null) {
				return zoneInner.Contains(xAt,yAt);
			}
			else if (rectAbs!=null) {
				return rectAbs.Contains(xAt,yAt);
			}
			else return false;
		}
		///<summary>
		///sValue: 
		/// -if null and property doesn't exist, inserts a valueless property (i.e. MULTILINE)
		/// -if null OR blank & property already has value, value becomes "\"\"" (i.e. type="")
		/// -if null OR blank & property already is valueless, nothing is done.
		///</summary>
		public bool SetProperty(string sName, string sValue) {
			bool bGood=false;
			int iContentPropertiesEndEx=ContentPropertiesEndEx();
			int iCount=RString.GetMultipleAssignmentLocations(ref iarrPropName, ref iarrProp, sOpener, sName, '=', ' ',ContentPropertiesStart(),iContentPropertiesEndEx,false);
			try {
				if (iCount>0) {//at least 0 and 1 are usable
					//found existing property so overwrite last instance of it
					int iProp=iCount-1;
					if ( RString.IsBlank(sValue) &&((iarrProp[iProp*2+1]-iarrProp[iProp*2])>0) ) sValue="\"\""; //ONLY do this if value exists already
					if (RString.IsNotBlank(sValue)) sOpener=RString.SafeSubstring(sOpener,0,iarrProp[iProp*2])+sValue+RString.SafeSubstring(sOpener,iarrProp[iProp*2+1]); //ok to only do if not blank, since set to nonblank (i.e. "\"\"") if original value was nonblank
				}
				else { //else create the property
					sOpener=sOpener.Insert( iContentPropertiesEndEx, " " + sName + (sName!=null?("="+RConvert.ToSgmlPropertyValue(sName)):"") );
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return bGood;
		}//end SetProperty
		public bool SetStyleAttrib(string name, string value) {
			return SetStyleAttrib(name, value, false);
		}
		public bool SetStyleAttrib(string sName, string sValue, bool bForceAppend) {
			bool bGood=false;
			if (RString.IsBlank(sOpener)) sOpener="<style=\"\">";
			int iPropEndEx=ContentPropertiesEndEx();
			int iProps=RString.GetMultipleAssignmentLocations(ref iarrPropName, ref iarrProp, sOpener,"style",'=',' ',ContentPropertiesStart(),iPropEndEx,false);
			int iAttribs=0;
			string sStyleOpener=" style=\"";//(sOpener!="<>"?" style=\"":"style=\"");
			if (RString.IsBlank(sValue)) {
				RReporting.ShowErr("Cannot add blank style value");
				sValue="";
			}
			if (iarrProp==null) {
				iarrProp=new int[10*2];
				iarrProp[0]=-1;
			}
			try {
				if (iProps<1) {
					iProps=1;
					RString.SafeInsert(sOpener,iPropEndEx,sStyleOpener+"\"");
					iarrProp[0]=iPropEndEx;
					iarrProp[1]=iPropEndEx+sStyleOpener.Length+1;//+1 for closing quote
				}
				if (iProps>0) {
					int iProp=iProps-1;
					RString.ShrinkToInsideQuotes(sOpener,ref iarrProp[iProp*2], ref iarrProp[iProp*2+1]);
					iAttribs=RString.GetMultipleAssignmentLocations(ref iarrStyleAttribName, ref iarrStyleAttrib, sOpener, sName, ':', ';', iarrProp[iProp*2], iarrProp[iProp*2+1], false);
					int iAttrib=iAttribs-1;
					if (iAttribs<1||bForceAppend) {
						//if there is a style property but the named attribute needs to be created
						iAttribs=1;
						iAttrib=iAttribs-1;
						if (iarrStyleAttrib==null) iarrStyleAttrib=new int[10*2];
						string sAppend= (RString.EndsWithCharOrCharThenWhiteSpace(sOpener,';',iarrProp[iProp*2],iarrProp[iProp*2+1])?"":"; ") + sName + ":";
						RString.SafeInsert(sOpener,iarrProp[iProp*2+1],sAppend);
						iarrProp[iProp*2+1]+=sAppend.Length;
						iarrStyleAttrib[iAttrib*2]=iarrProp[iProp*2+1];
						iarrStyleAttrib[iAttrib*2+1]=iarrStyleAttrib[iAttrib*2];
					}
					sOpener=sOpener.Substring(0,iarrStyleAttrib[iAttrib*2])+sValue+sOpener.Substring(iarrStyleAttrib[iAttrib*2+1]);
				}
				else RReporting.ShowErr("The style was not modified (SetStyleAttrib corruption)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return bGood;
		}//end SetStyleAttrib
		public bool AppendStyleAttrib(string Name, string val) {//formerly AppendStyle
			return SetStyleAttrib(Name, val, true);
		}//end AppendStyleAttrib
		public bool HasStyleAttrib(string sName) { //aka StyleAttributeExists
			bool bFound=false;
			try {
				int iProps=RString.GetMultipleAssignmentLocations(ref iarrPropName, ref iarrProp, sOpener, "style", '=', ' ', ContentPropertiesStart(),ContentPropertiesEndEx(),false);
				for (int iProp=0; iProp<iProps; iProp++) {
					if (RString.GetMultipleAssignmentLocations(ref iarrStyleAttribName, ref iarrStyleAttrib, sOpener, sName, ':', ';', iarrProp[iProp*2], iarrProp[iProp*2+1], false)>0) {
						bFound=true;
						break;
					}
				}//end for multiple quoted style properties
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return bFound;
		}//end HasStyleAttrib
		public bool HasProperty(string sName) { //aka PropertyExists
			return RString.GetMultipleAssignmentLocations(ref iarrPropName, ref iarrProp, sOpener, sName, '=', ' ',ContentPropertiesStart(),ContentPropertiesEndEx(),false) > 0;
		}//end HasProperty
		public bool RemoveProperty(string sName) {
			int iIterations=0;
			bool bRemoved=false;
			int iCount=1;
			try {
				while (iCount>0) {
					iCount=RString.GetMultipleAssignmentLocations(ref iarrPropName, ref iarrProp, sOpener,sName,'=',' ',ContentPropertiesStart(),ContentPropertiesEndEx(),false);
					if (iCount>0) {
						sOpener=RString.SafeSubstring(sOpener,0,iarrPropName[0])+RString.SafeSubstring(sOpener,iarrProp[1]);//intentionally use PropName Start and PropVal EndEx (remove whole property assignment)
						bRemoved=true;
					}
					iIterations++;
					if (iIterations>1000) {
						RReporting.Warning("Tried to remove 1000 duplicate properties in same tag, cancelling loop for safety");
						break;
					}
				}//end while matching properties exist
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return bRemoved;
		}//end RemoveProperty
		///<summary>
		///get html property value by case-insentitive name
		///</summary>
		public string GetProperty(string sName, bool RemoveQuotes) {
			string sReturn="";
			int iProps=RString.GetMultipleAssignmentLocations(ref iarrPropName, ref iarrProp, sOpener, sName, '=', ' ',ContentPropertiesStart(),ContentPropertiesEndEx());
			if (iProps>0) {//for (int iProp=0; iProp<iProps; iProp+=2) {
				int iProp=iProps-1;
				if (RemoveQuotes) RString.ShrinkToInsideQuotes(sOpener, ref iarrProp[iProp], ref iarrProp[iProp+1]);
				if (iarrProp[iProp+1]-iarrProp[iProp]>0) sReturn=RString.SafeSubstring(sOpener,iarrProp[iProp],iarrProp[iProp+1]-iarrProp[iProp]);
			}
			return sReturn;
		}//end GetProperty
		public string GetProperty(string sName) {
			return GetProperty(sName,true);
		}
		public const int CascadeGroupBackgroundColor=0;
		//these must be in order of reverse priority
		public static readonly string[][] InterchangeableStyleAttribute=new string[][] {
			new string[]{"background-color","background"},
			new string[]{"text-align"},
			new string[]{"vertical-align"}//TODO: vertical-align aligns inner text like valign only when <td> OR div style="display:table-cell;" (bottom only works here--text-bottom has different behavior and is for display:inline-block)
			//otherwise text-bottom lines up that part of the image (or div/span with display:inline-block) with that part of the text when the inline-block is in a line of text and bottom does nothing.
		};
		public static readonly string[][] InterchangeableProperty=new string[][] {
			new string[]{"bgcolor"},
			new string[]{"align"},
			new string[]{"valign"}
		};
		private static int InterchangeableCascadeGroupIndex(string sFuzzyAttrib) {
			int iReturn=-1;
			if (sFuzzyAttrib!=null) {
				sFuzzyAttrib=sFuzzyAttrib.ToLower();
				for (int iGroup=0; iGroup<InterchangeableStyleAttribute.Length; iGroup++) {
					for (int iSubItem=0; iSubItem<InterchangeableStyleAttribute[iGroup].Length; iSubItem++) {
						if (sFuzzyAttrib==InterchangeableStyleAttribute[iGroup][iSubItem]) {
							iReturn=iGroup;
							break;
						}
					}
					for (int iSubItem=0; iSubItem<InterchangeableProperty[iGroup].Length; iSubItem++) {
						if (sFuzzyAttrib==InterchangeableProperty[iGroup][iSubItem]) {
							iReturn=iGroup;
							break;
						}
					}
					if (iReturn>-1) break;
				}
			}
			return iReturn;
		}//end InterchangeableCascadeGroup
		///<summary>
		///Applies stylesheet, header style in order of appearance in header, AND ancestor styles
		///</summary>
		private string ParentPage_GetCascadedFuzzyAttribApplyingToTag(string sOpeningTag, string sFuzzyAttrib, int iNodeIndex) {
			if (iNodeIndex<0) RReporting.Warning("Bad saved self-index in an RForm (RForms corruption)--will not be able to get parent styles");
			if (ParentPage!=null) return ParentPage.GetCascadedFuzzyAttribApplyingToTag(sOpeningTag,sFuzzyAttrib,iNodeIndex);
			else {
				RReporting.ShowErr("Null ParentPage for an RForm (RForms corruption)");
				return null;
			}
		}//end ParentPage_GetCascadedFuzzyAttribApplyingToTag
		///<summary>
		///sFuzzyAttrib can be something like "bgcolor" or "background-color" (case-insensitive)
		///</summary>
		public string CascadedFuzzyAttrib(string sFuzzyAttrib) {//formerly CascadedProperty
			ArrayList alPossible=null;
			string sReturn=null;
			string sValNow;
			try {
				if (RString.HasUpperCharacters(sFuzzyAttrib)) sFuzzyAttrib=sFuzzyAttrib.ToLower();
				sReturn=ParentPage_GetCascadedFuzzyAttribApplyingToTag(sOpener,sFuzzyAttrib,iIndex);
				int iCascadeGroup=InterchangeableCascadeGroupIndex(sFuzzyAttrib);
				if (iCascadeGroup>-1) {
					for (int iProperty=InterchangeableProperty[iCascadeGroup].Length-1; iProperty>=0; iProperty--) {
						sValNow=GetProperty(InterchangeableProperty[iCascadeGroup][iProperty]);
						if (RReporting.IsNotBlank(sValNow)) {
							sReturn=sValNow; //do NOT break, keep going to get more specific values from the cascade group
						}
					}
					for (int iStyleAttrib=InterchangeableStyleAttrib[iCascadeGroup].Length-1; iStyleAttrib>=0; iStyleAttrib--) {
						sValNow=GetStyleAttrib(InterchangeableStyleAttrib[iCascadeGroup][iStyleAttrib]);
						if (RReporting.IsNotBlank(sValNow)) {
							sReturn=sValNow; //do NOT break, keep overwriting to get more specific values from the cascade group
						}
					}
				}//end if fuzzy attribute is part of interchangeable cascade group
				else {
					sValNow=GetProperty(sFuzzyAttrib);
					if (RReporting.IsNotBlank(sValNow)) sReturn=sValNow;
					sValNow=GetStyleAttrib(sFuzzyAttrib);
					if (RReporting.IsNotBlank(sValNow)) sReturn=sValNow;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return sReturn;
		}//end CascadedFuzzyAttrib
		private int ContentPropertiesStart() {
			int iOpener=sOpener.IndexOf("<");
			if (iOpener>-1) {
				iOpener=RString.IndexOfNonWhiteSpace(sOpener,iOpener);
				if (iOpener>-1) {
					iOpener=RString.IndexOfWhiteSpace(sOpener,iOpener);
					if (iOpener<0) iOpener=0;
				}
				else iOpener=0;
			}
			else iOpener=0;
			return iOpener;
		}//end ContentPropertiesStart
		private int ContentPropertiesEndEx() {
			int iEndEx=-1;
			if (sOpener!=null) iEndEx=sOpener.LastIndexOf(">");
			if (iEndEx<0&&sOpener!=null) iEndEx=sOpener.Length;
			if ( iEndEx<sOpener.Length && sOpener[iEndEx]=='>' && (iEndEx-1>=0) &&sOpener[iEndEx-1]=='/' ) iEndEx--;//excludes self-closing tag slash
		}
		public string StyleAttribEqualsI_AssumingNeedleIsLower(string NameInHaystack, string NeedleValue) {
			bool bReturn=false;
			int iProps=RString.GetMultipleAssignmentLocations(ref iarrPropName, ref iarrProp, sOpener,"style",'=',' ',ContentPropertiesStart(),ContentPropertiesEndEx(),false);
			try {
				for (int iProp=iProps-1; iProp>=0; iProp--) {
					int iAttribs=RString.GetMultipleAssignmentLocations(ref iarrStyleAttribName, ref iarrStyleAttrib, sOpener,NameInHaystack,':',';',iarrProp[iProp*2],iarrProp[iProp*2+1],false);
					for (int iAttrib=iAttribs-1; iAttrib>=0; iAttrib--) {
						if ((iarrStyleAttrib[iAttrib*2+1]-iarrStyleAttrib[iAttrib*2])>0) {
							//name was already checked in GetMultipleAssignmentLocations
							bReturn=RString.CompareAtI(sOpener,NeedleValue,iarrStyleAttrib[iAttrib*2],iarrStyleAttrib[iAttrib*2+1]);
							iProp=-1;
							break;
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return bReturn;
		}//end StyleAttribEqualsI
		///<summary>
		///Gets self-cascaded style attribute, but NOT globally [parent, stylesheet, header]
		///</summary>
		private static int[] iarrStyleAttrib=new int[3*2];
		private static int[] iarrPropName=new int[20*2];
		private static int[] iarrProp=new int[20*2];
		public string GetStyleAttrib(string sName) {
			string sReturn=null;
			int iProps=RString.GetMultipleAssignmentLocations(ref iarrPropName, ref iarrProp, sOpener,"style",'=',' ',ContentPropertiesStart(),ContentPropertiesEndEx(),false);
			try {
				for (int iProp=iProps-1; iProp>=0; iProp--) {
					int iAttribs=RString.GetMultipleAssignmentLocations(ref iarrStyleAttribName, ref iarrStyleAttrib, sOpener,sName,':',';',iarrProp[iProp*2],iarrProp[iProp*2+1],false);
					for (int iAttrib=iAttribs-1; iAttrib>=0; iAttrib--) {
						if ((iarrStyleAttrib[iAttrib*2+1]-iarrStyleAttrib[iAttrib*2])>0) {
							//name was already checked in GetMultipleAssignmentLocations
							sReturn=RString.SafeSubstring(sOpener,iarrStyleAttrib[iAttrib*2],iarrStyleAttrib[iAttrib*2+1]-iarrStyleAttrib[iAttrib*2]);
							iProp=-1;
							break;
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return sReturn;
		}//end GetStyleAttrib
		public void ExpandTo(int xSet, int ySet, int iSetWidth, int iSetHeight, bool bLeft, bool bTop, bool bRight, bool bBottom) {
			try {
				rectAbs.X=xSet;
				rectAbs.Y=ySet;
				rectAbs.Width=iSetWidth;
				rectAbs.Height=iSetHeight;
				//do NOT save to style variables--rectAbs is set according to size of window
				ExpandInnerToOuterUsingMargin();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"expand to rect with margin and custom dock booleans","RForm ExpandTo");
			}
		}
		private void ExpandInnerToOuterUsingMargin() {
			ExpandInnerToOuterUsingMargin(true,true,true,true);
		}
		bool bWarnExpandMissingData=true;
		private void ExpandInnerToOuterUsingMargin(bool bLeft, bool bTop, bool bRight, bool bBottom) {
			//TODO: cascade from html border property and css border properties, in order of usage (do this in GetStyle?)
			try {
				if (rectAbs!=null&&zoneInner!=null) {
					if (bLeft) zoneInner.Left=rectAbs.X+RConvert.ToInt(GetStyle("margin-left"));
					if (bTop) zoneInner.Top=rectAbs.Y+RConvert.ToInt(GetStyle("margin-top"));
					if (bRight) zoneInner.Right=(rectAbs.X+rectAbs.Width)-RConvert.ToInt(GetStyle("margin-right"));
					if (bBottom) zoneInner.Bottom=(rectAbs.Y+rectAbs.Height)-RConvert.ToInt(GetStyle("margin-bottom"));
				}
				else if (bWarnExpandMissingData) {
					RReporting.Warning("Cannot expand--both inner and outer rect must be declared.{rectAbs:"+IRect.ToString(rectAbs)+"; zoneInner:"+IZone.ToString(zoneInner)+"}");
					bWarnExpandMissingData=false;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"expand to outer edge checking margins and custom dock booleans","RForm ExpandInnerToOuterUsingMargin");
			}
		}//end ExpandInnerToOuterUsingMargin
		#endregion utilities
		//debug NYI UpdateStyle should be in HTMLPage so page size can be determined/accessed first
		#region unused methods
	/*
		public bool UpdateSize(int iRForm) {
			string sStyle="";
			iWidth=0;
			iHeight=0;
			//TODO: fix this to check whether values are DECIMAL to denote a percentage.
			vsAttrib.Get(ref sStyle, "style");
			vsAttrib.Get(ref iWidth, "width");
			vsAttrib.Get(ref iHeight, "height");
			
				 vStyle.FromStyle(sStyle);
			vStyle.Get(ref iWidth, "width");
			vStyle.Get(ref iHeight, "height");
			
			iType=RFormType.Undefined;
			string sType="";
			if (sTag=="input") vsAttrib.Get(ref sType, "type");
			iType=RFormType.Markup;
			return iType>0;
		}
		*/
		/*
		public bool ShiftLoc(int iShiftBy, int iStartAt) {
			//(accounts for inner shift which causes expansion)
			//iOpeningLen
			//iPostOpeningLen
			//iInnerLen
			//iClosingLen
			//iPostClosingLen
			if (iStartAt<=iOpening) {
				iOpening+=iShiftBy;
			}
			else {
				if (iStartAt<=iPostOpening) {
					iOpeningLen+=iShiftBy;
					//this corrects iInner location too
				}
				else {
					//if (iStartAt<=iInner) {
					//	iPostOpeningLen+=iShiftBy;
					//}
					//else {
						if (iStartAt<=iClosing) {
							iInnerLen+=iShiftBy;
						}
						else {
							if (iStartAt<=iPostClosing) {
								iClosingLen+=iShiftBy;
							}
							else {
								if (iStartAt<=iPostClosing+iPostClosingLen) {
									iPostClosingLen+=iShiftBy;
								}
								//else change does not affect this rform
							}
						}
					//}
				}
			}
			return ValidateLocVars();
		}*/
		/*
		public bool ValidateLocVars() {
			bool bGood=true;
			if (iOpening<0) {
				iOpening=0;
				bGood=false;
			}
			if (iOpeningLen<0) {
				iOpeningLen=0;
				bGood=false;
			}
			if (iPostOpeningLen<0) {
				iPostOpeningLen=0;
				bGood=false;
			}
			if (iInnerLen<0) {
				iInnerLen=0;
				bGood=false;
			}
			if (iClosingLen<0) {
				iClosingLen=0;
				bGood=false;
			}
			if (iPostClosingLen<0) {
				iPostClosingLen=0;
				bGood=false;
			}
			return bGood;
		}
		*/
		#endregion unused methods
		
		
		#region Render
		//TODO: move to RForms and replace with html-painting-cursor-based method
		public void Render(RImage riDest, bool bAsActive) {
			if (bAsActive) {
				//see also RTextBox.Render
				RImage.brushFore.Set(RForms.colorActive);
				riDest.DrawRectCropped(rectAbs.X,rectAbs.Y,rectAbs.Width-1,rectAbs.Height-1);
			}
			else {
				RImage.brushFore.SetRgb(64,64,64);//debug only
				riDest.DrawRectCropped(rectAbs);//debug only
			}
			if (textbox!=null) textbox.Render(riDest,bAsActive);
			else {
				Color colorBack=RConvert.ToColor(CascadedProperty("background-color"));
				if (colorBack.A>0||TagwordLower=="button"||TypeLower=="button") {
					riDest.DrawRectStyleCropped(colorBack,rectAbs);
					//riDest.brushFore.SetArgb(colorBack.A,colorBack.R,colorBack.G,colorBack.B);
					//riDest.DrawRectCroppedFilled(rectAbs);
				}
				RenderText(riDest,Text);//rfont.Render(ref riDest, zoneInner, Text);
			}
		}//end Render primary overload
		public void RenderText(RImage riDest, string sText) {
			rfont.Render(ref riDest, zoneInner, sText);//TODO: horizontal and vertical alignment
		}
		#endregion Render
	}//end class RForm
}//end namespace
