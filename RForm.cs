/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PM
 * 
 */

using System;
using System.Drawing;
using System.Collections;

namespace ExpertMultimedia {
	
	/// <summary>
	/// Graphical node (i.e. to convert from/to a markup tag)
	/// OnClick and other html events are extracted from these objects and processed by RForms
	/// </summary>
	public class RForm {
		public RForms ContainerPage=null;
		public bool bScrollable=false;//TODO: finish this - create scrollbars if exceeds parent.
		public bool bTabStop=true;//whether the node can become active
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
		/// <summary>
		/// The RenderItem constants are only used by the RenderItem method
		/// </summary>
		public const int RenderItemNone=0;//nothing is rendered except non-implicit aspects (such as text, image, etc)
		public const int RenderItemButton=1;
		public const int RenderItemTextBox=2;
		//public const int RenderItemSelect=2;//dropdown box
		//public const int RenderItemSelectMultiple=3;
		//public const int RenderItemTextBox=4;
		public bool bDown=false;//button, input type=button, or select size=1 [drop down] down state //TODO: implement this -- show as pushed down
		public static void RenderItem(RImage riOutput, IRect RectItem, int RenderItemX) {
			RenderItem(riOutput,RectItem,RenderItemX,false);//,0,0,0);//TODO:? Add bDown as the MouseIsDown parameter?
		}
		private const string RenderItem_SenderString="RenderItem";
		private static IRect rectScrollTemp=new IRect();
		/// <summary>
		/// MouseDownPart constants are used internally for telling the RForm
		/// RenderItem method which elements of the RenderItem should be
		/// drawn as to indicate that they are currently being pressed.
		/// </summary>
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
		public int LineHeight {	get { return rfont!=null?rfont.iLineSpacing+(RForms.iControlCornerRadius*2):(RFont.rfontDefault.iLineSpacing); } } //aka FontHeight
		//temporary variables:
		private static int[] AttribNameStarterEnderIndeces=new int[20*2];
		private static int[] AttribValueStarterEnderIndeces=new int[20*2];
		private static int[] PropNameStarterEnderIndeces=new int[20*2];
		private static int[] PropValueStarterEnderIndeces=new int[20*2];
		///<summary>
		///IF necessary, draws scrollbars and modifies rects.
		///rectSrc: the source rectangle of the object to be drawn, made
		/// smaller if scrollbars are necessary.
		///RectAbsolute: the destination rectangle in the output image, NOT modified
		///</summary>
		private void DrawScrollBars(RImage riOutput, IRect RectAbsolute, bool MouseIsDownOnIt, int ScrollBarMouseDownPart_ElseZero) {
			int BarX2=RForms.iControlScrollBarThickness*2;
			if (riOutput!=null&&RectAbsolute!=null&&rectSrc!=null) {
				if (bDropDown) {
					rectScrollTemp.Width=RForms.iControlScrollBarThickness;
					rectScrollTemp.Height=LineHeight;
					rectScrollTemp.Y=RectAbsolute.Y;
					rectScrollTemp.X=(RectAbsolute.Y+RectAbsolute.Width)-RForms.iControlScrollBarThickness;
					rectSrc.Width-=RForms.iControlScrollBarThickness;
					RenderItem(riOutput,rectScrollTemp,RenderItemButton,bOpen||ScrollBarMouseDownPart_ElseZero==MouseDownPartDropDown);

					//TODO: finish this
					// 5 lines alternating between 110.6%,61.5%
					//-expand RectRelative so renderer will know to draw scrollable subitems
					//-draw scrollbars if bDown
					int yNow=rectScrollTemp.Y;//TODO: check this (should arrow start lower?  should that lower yNow be used instead of rectScrollTemp.Y?)
					RenderArrow(riOutput, rectScrollTemp.X, rectScrollTemp.Y, rectScrollTemp.Width, rectScrollTemp.Height, RenderArrowDown);
					yNow+=RForms.iControlArrowLength;
					//stripes below down arrow (drop-down box arrow):
					int yEndBefore=yNow+5;
					int ArrowSideStart=(RForms.iControlScrollBarThickness-RForms.iControlArrowWidth)/2;
					bool bDark=false;
					while (yNow<yEndBefore) {
						riOutput.DrawHorzLine(bDark?RForms.rpaintControlStripeDark:RForms.rpaintControlStripeLight,ArrowSideStart,yNow,RForms.iControlArrowWidth);
						yNow++;
						bDark=!bDark;
					}
				}//end if bDropDown
				if (rectSrc.Width<RectAbsolute.Width) { ///TODO: ok since rectSrc dimensions and RectAbsolute location should be rendered by renderer
					//draw horizontal scrollbar
					//-draw mover block (the moveable rectangle in the scrollbar)
					int iMoverBlockContainerSize=rectSrc.Width-BarX2;//use BarX2 to account for both arrows
					if (iMoverBlockContainerSize<0) iMoverBlockContainerSize=0;
					int iMoverBlockSize=RMath.SafeDivideRound(RMath.SafeMultiply(rectSrc.Width,iMoverBlockContainerSize),RectAbsolute.Width,iMoverBlockContainerSize);
					rectSrc.Height-=RForms.iControlScrollBarThickness;///done LAST (remove scrollbar area)
				}
			}//end if all non-null rects and dest
		}//end DrawScrollBars
		///<summary>
		///Additional vars are used when RenderItemX is RenderItemScrollH or RenderItemScrollV
		///</summary>
		public static RPaint rpaintTemp=new RPaint();
		/// <summary>
		/// RenderArrow constants are used by the arrow drawing method
		/// </summary>
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
					int xDarkNow=(bVertical?(containerbutton_x+(containerbutton_width-RForms.iControlArrowWidth)/2):(containerbutton_y+(containerbutton_height-RForms.iControlArrowWidth)/2));//RForms.iControlScrollBarThickness/4;//truncated not rounded, since size is rounded
					int ArrowSideStart=xDarkNow;
					int xBlackNow=xDarkNow+1;
					int xLightNow=xDarkNow+iBlackWidthNow;
					int iMin=(iArrowWidthNow%2==0)?2:1;
					//next line used to use (bVertical?RectAbsolute.Y:RectAbsolute.X) 
					int yNow=(bVertical?containerbutton_y:containerbutton_x)+RMath.IRound((float)(bVertical?containerbutton_height:containerbutton_width)*0.4375f);//RMath.IRound((float)RectAbsolute.Y*.35f);
					//down arrow:
					while (iArrowWidthNow>=iMin) {
						if (bVertical) {
							riOutput.SetPixel(RForms.rpaintControlSymbolShadowDark,xLightNow,yNow);
							riOutput.DrawHorzLine(RForms.rpaintControlSymbol,xBlackNow,yNow,iBlackWidthNow);
							riOutput.SetPixel(RForms.rpaintControlSymbolShadowLight,xDarkNow,yNow);
						}
						else {
							riOutput.SetPixel(RForms.rpaintControlSymbolShadowDark,yNow,xDarkNow);
							riOutput.DrawVertLine(RForms.rpaintControlSymbol,yNow,xBlackNow,iBlackWidthNow);
							riOutput.SetPixel(RForms.rpaintControlSymbolShadowLight,yNow,xLightNow);
						}
						xDarkNow++;
						xBlackNow++;
						xLightNow--;
						iArrowWidthNow-=2;
						iBlackWidthNow-=2;
						yNow+=iDirAdder;
					} //end while (iArrowWidthNow>=iMin)
				//}//end if vertical
				//else {
				//}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
		}//end RenderArrow
		public static void RenderItem(RImage riOutput, IRect RectItem, int RenderItemX, bool MouseIsDownOnItem) {//, int SizeOuter, int SizeShown, int AmountScroll) {
			RPaint rpaintSwap=rpaintTemp;
			//TODO: cropping
			if (riOutput!=null&&RectItem!=null) {
				//string sDebugMethod="RenderItem(RImage,IRect,RenderItem="+RenderItemX.ToString()+",MouseIsDownOnItem="+RConvert.ToString(MouseIsDownOnItem)+",...)";
				int RectItem_Bottom_Inclusive=RectItem.BottomInclusive;
				int RectItem_Right_Inclusive=RectItem.RightInclusive;
				int LeftShrink1=RectItem.X+1;
				int TopShrink1=RectItem.Y+1;
				//int RectItem_Width_minus_2=RectItem.Width-2;
				if (RenderItemX==RenderItemButton||RenderItemX==RenderItemTextBox) {	//render rounded black outline
					//draw rounded non-antialiased black border (corners are drawn later below):
					riOutput.DrawHorzLine(RForms.rpaintControlLine,RectItem.X+2,RectItem.Y,RectItem.Width-4);
					riOutput.DrawHorzLine(RForms.rpaintControlLine,RectItem.X+2,RectItem_Bottom_Inclusive,RectItem.Width-4);
					riOutput.DrawVertLine(RForms.rpaintControlLine,RectItem.X,RectItem.Y+2,RectItem.Height-4); //Left side
					riOutput.DrawVertLine(RForms.rpaintControlLine,RectItem_Right_Inclusive,RectItem.Y+2,RectItem.Height-4); //Right side
				}//more drawn after cases below
				if (RenderItemX==RenderItemButton) { //rpaintControl 107% to 90%
					if (RenderItemX==RenderItemButton) {
						//draw gradient inner part:
						riOutput.DrawGradTopDownRectFilled(RForms.rpaintControlGradientLight,RForms.rpaintControlGradientDark,LeftShrink1, TopShrink1, RectItem.Width-2, RectItem.Height-2);
					}
					else {//set rpaint to sunken shadow color, and draw inner solid background
						riOutput.DrawRectFilled(RForms.rpaintTextBoxBack,LeftShrink1, TopShrink1, RectItem.Width-2, RectItem.Height-2);
					}
					if (RenderItemX==RenderItemButton&&MouseIsDownOnItem) {
						riOutput.SetPixel(RForms.rpaintControl,LeftShrink1,TopShrink1);
						rpaintSwap=RForms.rpaintControlInsetShadow;
					}
					else {//draw up
						if (RenderItemX==RenderItemButton) rpaintSwap=RForms.rpaintButtonShine;
						else rpaintSwap=RForms.rpaintTextBoxBackShadow;
						riOutput.DrawHorzLine(rpaintSwap,LeftShrink1, RectItem.Y+2, 2); //2 pixel line for left fx
						riOutput.DrawVertLine(rpaintSwap,LeftShrink1, RectItem.Y+3, RectItem.Height-5); //inner left line
						riOutput.SetPixel(rpaintSwap,RectItem.X+2,RectItem_Bottom_Inclusive-1);
					}//end draw up
					//common (for button, whether up or down) lines:
					riOutput.DrawHorzLine(rpaintSwap,RectItem.X+2, TopShrink1, RectItem.Width-4); //inner top line
					riOutput.DrawHorzLine(rpaintSwap,RectItem_Right_Inclusive-2, RectItem.Y+2, 2); //2 pixel line for right fx
					riOutput.DrawVertLine(rpaintSwap,RectItem_Right_Inclusive-1, RectItem.Y+3, RectItem.Height-5); //inner right line
					riOutput.SetPixel(rpaintSwap,RectItem_Right_Inclusive-2,RectItem_Bottom_Inclusive-1);
				}//end if RenderItemButton
				//end RenderItem* types
				if (RenderItemX==RenderItemButton||RenderItemX==RenderItemTextBox) {
					//draw dots on top of shaded area for inner pixel of rounded corners
					riOutput.SetPixel(RForms.rpaintControlLine,LeftShrink1,TopShrink1); //TL dot
					riOutput.SetPixel(RForms.rpaintControlLine,RectItem_Right_Inclusive-1,TopShrink1); //TR dot
					riOutput.SetPixel(RForms.rpaintControlLine,LeftShrink1,RectItem_Bottom_Inclusive-1); //BL dot
					riOutput.SetPixel(RForms.rpaintControlLine,RectItem_Right_Inclusive-1,RectItem_Bottom_Inclusive-1); //BR dot
				}
			}
			else RReporting.ShowErr("Null destination","rendering interface item",String.Format("RenderItem{{RImage:{0}; IRect:{1}}}",RImage.ToString(riOutput),IRect.ToString(RectItem)) );
		}//RenderItem
		public void CalculateDisplayMethodVars() {
			CalculateDisplayMethodVars(this.TagwordLower,this.GetProperty("type",true),this.GetStyleAttrib("display"));
		}
		///<summary>
		///(called by rforms UpdateAll)
		///Calculates display vars such as display attribute index (specified or inferred),
		/// RenderItemBackground (such as RenderItemButton); 
		/// creates textbox (multi- or single-line) if necessary
		///</summary>
		/// <param name="TagwordX">The HTML Tagword</param>
		/// <param name="TypeX">The value of the HTML "TYPE" property</param>
		/// <param name="CSSDisplayX">Overrides the calculated display method (leave this null unless specifying manually)</param>
		public void CalculateDisplayMethodVars(string TagwordX, string TypeX, string CSSDisplayX) {
			DisplayMethodIndex=DisplayUninitialized;
			if (TagwordX!=null) TagwordX=TagwordX.ToLower();
			if (TypeX!=null) TypeX=TypeX.ToLower();
			if (CSSDisplayX!=null) CSSDisplayX=CSSDisplayX.ToLower();
			DisplayMethodIndex=DisplayInline; //default
			RenderItemBackground=RenderItemNone; //default
			if ( TagwordX=="button" || (TagwordX=="input"&&(TypeX=="submit"||TypeX=="button"||TypeX=="reset")) ) {
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
			//TODO: use CSSDisplayX param to override calculated display method
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
		public string sOpener="";//ALL data including opening '<', html property assignments, and closing '>' OR "/>"
		public string sContent="";//text AFTER sOpener & before children //formerly sInnerText
		public string sCloser="";//closing tag after children, & opening '</' & closing '>' IF ANY
		public string sPostText="";//any data after closing tag (or after self-closing sOpener), including newlines,
		//RForm rformParent=null;
		public int iIndex=-1;//index in the RForms.rformarr array
		public int ParentIndex=-1;
		private int iSubNodes=0;//TODO: make sure this is set correctly EVERYWHERE including when node is manually pushed under parent
		//public int indexParent; //index in rformarr (replaced by ParentIndex)
		public bool bLeaf; //whether this can be drawn as text
		/// <summary>
		// Formerly, iType was used, i.e. iType was set to TypeTextArea.
		// Now, the precalculated DisplayMethodIndex is set to a Display*
		// constant for display style, and behavior is determined using
		// the html tagword and other variables.
		/// </summary>
		private int DisplayMethodIndex=DisplayUninitialized; //fixed by CalculateDisplayMethodVars
		#endregion required vars

		#region optional vars
		//public int iComponentIDOfOwner;
		  //which application (i.exn. retroengineclient or website)
		  //owns and maintains this RForm
		public int iLengthChildren=0;
		public int Length {
			get {
				return RString.SafeLength(sOpener)+RString.SafeLength(sContent)+iLengthChildren+RString.SafeLength(sCloser);
			}
			set {
				iLengthChildren=value-(RString.SafeLength(sOpener)+RString.SafeLength(sContent)+RString.SafeLength(sCloser));
				if (iLengthChildren<0) {
					iLengthChildren=0;
					RReporting.ShowErr("Tried to set rform length to but the value must be greater than or equal to the sum of the PreText, Opener, Content, and Closer (the child tags are all that is left, the length of which is set by the value sent here minus the sum of the strings listed above).","","rform set Length="+value.ToString());
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
				return !StyleAttribEqualsI_AssumingNeedleIsLower("visibility","hidden");
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
				RForm rformReturn=null;
				if (ContainerPage!=null) {
					if (iIndex>-1) {
						if (ParentIndex>-1) {
							rformReturn=ContainerPage.Node(ParentIndex);
							if (rformReturn==null) {
								RReporting.ShowErr("Node at parent index is null!","getting parent node via RForm node","RForm get RForm Parent {this.iIndex:"+iIndex.ToString()+"; this.ParentIndex:"+ParentIndex.ToString()+"}");
							}
						}
						else RReporting.Warning("Tried to get parent object from a base node!");
					}
					else RReporting.Warning("Bad saved self-index in an RForm (RForms corruption)--will not be able to get parent object");
				}
				else RReporting.ShowErr("Null  for an RForm (RForms corruption while getting Parent object)");
				return rformReturn;
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
		private string sTagwordLower=null; //generated first time needed using sOpener (see TagwordLower get)
		public string TagwordLower {
			get {
				if (sTagwordLower==null) { //only generated first time needed
					int iAbs=RString.IndexOfNonWhiteSpaceNorChar(sOpener,0,'<');
					if (iAbs>-1) {
						int iEnder=RString.IndexOfWhiteSpaceOrChar(sOpener,'>',iAbs);
						if (iEnder>-1) {
							if (iEnder>0&&sOpener[iEnder-1]=='/') iEnder--;
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
		//private string sValueOrContent=null;//private cache only
		/// Manipulates ValueOrContent without dealing with textbox
		//public string ValueOrContent { //replaced by more comprehensive Text accessor which also accounts for textbox object in addition to sContent or "value" property
		//	get {
		//		if (sValueOrContent==null) {
		//			sValueOrContent=GetProperty("value");
		//			if (!HasProperty("value")) sValueOrContent=sContent;
		//			if (sValueOrContent==null) sValueOrContent="";
		//		}
		//		return sValueOrContent;
		//	}
		//}//end get/set ValueOrContent
		public bool ValueTagIsContent {
			get { return TagwordLower=="input"||TagwordLower=="button"; }
		}
		/// <summary>
		/// Gets/sets value of the "value" property or the tag content where 
		/// appropriate, also dealing with textbox object where appropriate.
		/// </summary>
		public string Text {//formerly sText
			get {
				//TODO: finish this -- make textbox automatically update value or sContent as needed
				if (ValueTagIsContent) {
					//Before getting, set value property to textbox if object exists:
					if (textbox!=null) SetProperty("value",RString.ToHtmlValue(textbox.ToString()));//debug performance
					return GetProperty("value");
				}
				else {//if sContent is content
					if (textbox!=null) sContent=RString.ToHtmlValue(textbox.ToString());//debug performance
					return sContent; //TODO: for sgml: (only used if root !bUpdateHTML, otherwise root node gets it from an sgmlNow.Substring(...))
				}
			}
			set {
				if (textbox!=null) textbox.SetText(value);
				//do this separately:
				if (ValueTagIsContent) SetProperty("value",value);
				else sContent=value;
			}
		}//end Text (get/set via content)
		/*
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
		}//end Text (DEPRECATED, via textbox or sText)
		*/

		#endregion cached variables
		
		//public Variable vParent=null; //TODO: implement this (create Variable [created from HTML] "re-parser") has links to sourcecode etc
		
		#region variables, rendering (framework Graphics) vars
		public Color colorBack=SystemColors.Window;
		//public SolidBrush rpaintBack=new SolidBrush(SystemColors.Window);
		//public System.Drawing.Pen penBack=new System.Drawing.Pen(SystemColors.Window);
		//Color color=Color.Black;
		//SolidBrush rpaint=new SolidBrush(Color.Black);
		public Color colorTextNow=Color.Black;
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
					iReturn=Text.Length;
					//if (textbox!=null) iReturn=textbox.Length; //iType==TypeTextArea) iReturn=textbox.Length; //TODO: make sure textbox is set to null if type changes, and make sure it changes mode if input type changes
					//else return sText.Length;
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
		//				if (value<MAXBRANCHES) RReporting.WriteLine("shrinking "+(IsLeaf?"a leaf":(IsRoot?"root":"a"))+" rform's MAXBRANCHES to "+value.ToString()+" (sub array set to null)");
		//				rformarr=null;
		//			}
		//			else {
		//					RForm[] rformarrNew=new RForm[value];
		//					if (value<MAXBRANCHES) RReporting.WriteLine("shrinking "+(IsLeaf?"a leaf":(IsRoot?"root":"a"))+" rform's MAXBRANCHES");
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
		/// <summary>
		/// Initializes RForm.  See RForm Init method for information on variables.
		/// </summary>
		public RForm() {
			Init(null,0,"","",0,0,0,0,"");
			RReporting.Warning("The default RForm constructor was used--the  has yet to be set!");
		}
		/// <summary>
		/// Initializes RForm.  See RForm Init method for information on variables.
		/// </summary>
		public RForm(RForms SetContainerPage) {
			Init(SetContainerPage,0,"","",0,0,0,0,"");
		}
		//public RForm(RImage riSurface) {
		//	Init(riSurface);
		//}
		/// <summary>
		/// Initializes RForm.  See RForm Init method for information on variables.
		/// </summary>
		public RForm(RForms SetContainerPage, int iSetParentNode,/* int RFormType, */string sSetName, string sSetText, IRect rectSetAbsToCopy) {
			Init(SetContainerPage, iSetParentNode, /*RFormType, */sSetName, sSetText, rectSetAbsToCopy.X,rectSetAbsToCopy.Y,rectSetAbsToCopy.Width,rectSetAbsToCopy.Height, ""); //IndexParent, 
		}
		/// <summary>
		/// Initializes RForm.  See RForm Init method for information on variables.
		/// </summary>
		public RForm(RForms SetContainerPage, int iSetParentNode, /*int RFormType, */string sSetName, string sSetText, int xLoc, int yLoc, int Width, int Height) {
			Init(SetContainerPage, iSetParentNode, /*RFormType, */sSetName, sSetText, xLoc, yLoc, Width, Height, ""); //IndexParent, 
		}
		/// <summary>
		/// Initializes RForm.  See RForm Init method for information on variables.
		/// </summary>
		public RForm(RForms SetContainerPage, int iSetParentNode, /*int RFormType, */string sSetName, string sSetText, int xLoc, int yLoc, int Width, int Height, string HTMLTag) {
			Init(SetContainerPage, iSetParentNode, /*RFormType, */sSetName, sSetText, xLoc, yLoc, Width, Height, HTMLTag); //IndexParent, 
		}
		public void Init(RForms SetContainerPage, int iSetParentNode, /*int RFormType, */string sSetName, string sSetText) {
			Init(SetContainerPage, iSetParentNode, /*RFormType, */sSetName, sSetText,0,0,0,0,"");
		}
		public void Init(RForms SetContainerPage, int iSetParentNode, /*int RFormType, */string sSetName, string sSetText, IRect rectSetAbsToCopy) { //int IndexParent,
			Init(SetContainerPage, iSetParentNode, /*RFormType, */sSetName, sSetText, rectSetAbsToCopy.X, rectSetAbsToCopy.Y, rectSetAbsToCopy.Width, rectSetAbsToCopy.Height, "");
		}
		public void Init(RForms SetContainerPage, int iSetParentNode, /*int RFormType, */string sSetName, string sSetText, int xLoc, int yLoc, int iSetWidth, int iSetHeight) { //int IndexParent,
			Init(SetContainerPage, iSetParentNode, /*RFormType, */sSetName, sSetText, xLoc, yLoc, iSetWidth, iSetHeight, "");
		}
		/// <summary>
		/// Initializes RForm.
		/// </summary>
		/// <param name="iSetParentNode"></param>
		/// <param name="sSetName"></param>
		/// <param name="sSetText">Must be non-null if tag is input and input type=text is desired (otherwise can't input text)!</param>
		/// <param name="xLoc"></param>
		/// <param name="yLoc"></param>
		/// <param name="iSetWidth"></param>
		/// <param name="iSetHeight"></param>
		/// <param name="HTMLTag">The text to go between the &lt; (less than) and &gt; (greater than) signs.  MUST end with '/' (forward slash) if you don't want a closing tag (&lt; then HTMLTag--excluding the first space and following characters if any--then &gt;)</param>
		public void Init(RForms SetContainerPage, int iSetParentNode, /*int RFormType, */string sSetName, string sSetText, int xLoc, int yLoc, int iSetWidth, int iSetHeight, string HTMLTag) { //int IndexParent,
			ContainerPage=SetContainerPage;
			ParentIndex=iSetParentNode;//rformParent=rformSetParent;
			//iType=RFormType;
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
			int iCloserEnder=RString.SafeIndexOf(HTMLTag," ");
			zoneInner=new IZone(rectTemp);
			string HTMLTagLower=HTMLTag.ToLower();
			sOpener="";
			sContent="";
			sCloser="";
			//if (HTMLTagLower=="input"&&sSetText!=null) {
			//}
			//else {
			//	if (HTMLTagLower=="textarea") {
			//	}
			//	else {
					
			//	}
			//	sCloser="</"+((iCloserEnder>-1)?(RString.SafeSubstring(HTMLTag,0,iCloserEnder)):HTMLTag)+>";
			//}
			if (HTMLTagLower=="input"&&sSetText!=null) {
				textbox=new RTextBox(this,1,false);//false: non-multiline
				sOpener="<"+HTMLTag+" type=text value=\""+RString.ToHtmlValue(sSetText)+"\""+((RString.SafeLength(HTMLTag)>0&&HTMLTag[HTMLTag.Length-1]!='/')?"/":"")+">";
				sContent="";
				sCloser="";
			}
			else if (HTMLTagLower=="textarea") {
				textbox=new RTextBox(this,1);
				textbox.SetText(RString.ToHtmlValue(sSetText));
				sOpener="<"+HTMLTag+">";
				
				//ALWAYS add CLOSER if is a textarea:
				sCloser="</"+((iCloserEnder>-1)?(RString.SafeSubstring(HTMLTag,0,iCloserEnder)):HTMLTag)+">";
			}
			else {
				sOpener="<"+HTMLTag+">";
				if (!RString.CompareAt(HTMLTag,'/',RString.SafeLength(HTMLTag)-1)) {
				//add closer only if textarea OR does not have '/'
					sCloser="</"+((iCloserEnder>-1)?(RString.SafeSubstring(HTMLTag,0,iCloserEnder)):HTMLTag)+">";
				}
			}
			//if (RFormType==TypeTextArea) textbox=new RTextBox(this,1);
			//else if (RFormType==TypeTextEdit) textbox=new RTextBox(this,1,false);
			Text=sSetText; //TODO:? not used unless rformrRoot.bUpdateHTML is false??
			//indexParent=IndexParent;
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
		//}//end Init		
		#endregion constructors
		
		#region input
		public bool Backspace() {
			if (textbox!=null) return textbox.Backspace();
			else {
				RReporting.ShowErr("Backspace is only implemented for text input nodes");
				return false;
			}
		}
		public bool Delete() {
			if (textbox!=null) return textbox.Delete();
			else {
				RReporting.ShowErr("Delete is only implemented for text input nodes");
				return false;
			}
		}
		public bool Return() {
			if (textbox!=null) return textbox.Return();
			else {
				RReporting.ShowErr("Entering a line return is only implemented for text input nodes");
				return false;
			}
		}
		//public bool InsertLine(int iLine, string sLine) {
		//	if (textbox!=null) return textbox.InsertLine(iLine,sLine);
		//	else {
		//		RReporting.ShowErr("InsertLine is only implemented for text input nodes");
		//		return false;
		//	}
		//}
		//public bool RemoveLine(int iLine) {
		//	if (textbox!=null) return textbox.RemoveLine(iLine);
		//	else {
		//		RReporting.ShowErr("RemoveLine is only implemented for text input nodes");
		//		return false;
		//	}
		//}
		//public bool AddLine(string sLine) {
		//	if (textbox!=null) return textbox.AddLine(sLine);
		//	else {
		//		RReporting.ShowErr("AddLine is only implemented for text input nodes");
		//		return false;
		//	}
		//}
		public bool Insert(char cToInsertAtCursor) { //formerly EnterText
			//rformrRoot.sgmlNow.InsertText(Char.ToString(cToInsertAtCursor));
			if (textbox!=null) return textbox.Insert(char.ToString(cToInsertAtCursor));
			else {
				RReporting.ShowErr("EnterText(char) is only implemented for text input nodes","typing character in non-textinput node","Insert(char) {RApplication.ActiveTabIndex:"+RApplication.ActiveTabIndex+"; rform index:"+this.ContainerPage.iActiveNode+"}");
				return false;
			}
		}
		public bool Insert(string sToInsertAtCursor) { //formerly EnterText
			//rformrRoot.sgmlNow.InsertText(sToInsertAtCursor); //TODO: must shift all variables of all necessary nodes
			if (textbox!=null) return textbox.Insert(sToInsertAtCursor);
			else {
				RReporting.ShowErr("EnterText(string) is only implemented for text input nodes");
				return false;
			}
		}
		public bool SetTextAreaSelection(int iRowStart, int iColStart, int iRowEnd, int iColEnd) {
			if (textbox!=null) return textbox.SetSelection(iRowStart,iColStart,iRowEnd,iColEnd);
			else {
				RReporting.ShowErr("SetTextAreaSelection(...) is only implemented for text input nodes");
				return false;
			}
		}
		public bool ShiftSelection(int iRows, int iCols, bool bWithShiftKey) {
			if (textbox!=null) return textbox.ShiftSelection(iRows,iCols,bWithShiftKey);
			else {
				RReporting.ShowErr("ShiftSelection(...,bool) is only implemented for text input nodes");
				return false;
			}
		}
		public bool ShiftSelection(int iRows, int iCols) {
			if (textbox!=null) return textbox.ShiftSelection(iRows,iCols);
			else {
				RReporting.ShowErr("ShiftSelection(...) is only implemented for text input nodes");
				return false;
			}
		}
		public bool GrowSelection(int iRows, int iCols) {
			if (textbox!=null) return textbox.GrowSelection(iRows,iCols);
			else {
				RReporting.ShowErr("GrowSelection(...) is only implemented for text input nodes");
				return false;
			}
		}
		public bool Home(bool bWithShiftKey) {
			if (textbox!=null) return textbox.Home(bWithShiftKey);
			else {
				RReporting.ShowErr("Home(...) is only implemented for text input nodes");
				return false;
			}
		}
		public bool End(bool bWithShiftKey) {
			if (textbox!=null) return textbox.End(bWithShiftKey);
			else {
				RReporting.ShowErr("End(...) is only implemented for text input nodes");
				return false;
			}
		}
		public bool SetTextAreaCursor(int iRow, int iCol) {
			if (textbox!=null) return textbox.SetCursor(iRow,iCol);
			else {
				RReporting.ShowErr("SetTextAreaCursor(...) is only implemented for text input nodes");
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
			if (textbox!=null) return textbox.Clear();
			else {
				RReporting.ShowErr("Clear is only implemented for text input nodes");
				return false;
			}
		}
		public bool SetLines(string[] SetLines) {
			if (textbox!=null) return textbox.SetLines(SetLines);
			else {
				RReporting.ShowErr("SetLines is only implemented for text input nodes");
				return false;
			}
		}
		public bool Tab() {
			//TODO: impement this--tab
			//-if selected text push line(s) to the right
			//-else if selected text and shift+tab push line(s) to the left 
			//-else insert a tab character
			RReporting.ShowErr("Tab is not yet implemented","typing tab","rform Tab()");
			return false;
		}
		#endregion input
		
		
		
		#region utilities
		///<summary>
		///Compares the tagword of this node to a substring of val (case-insensitive)
		///</summary>
		public bool TagwordEquals(string val, int startInVal, int endbeforeInVal) {
			return RString.CompareAtI_AssumingNeedleIsLower(val,TagwordLower,startInVal,endbeforeInVal,false);
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
		public static bool FindLastFuzzyStyleAssignment(ref int[] iarrReturn, string Haystack, string sAttribName) {
			return FindLastFuzzyStyleAssignment(ref iarrReturn,Haystack,sAttribName,0,RString.SafeLength(Haystack));
		}
		/// <summary>
		/// Gets the location and length of both the given style attribute 
		/// name and of the associated value following it
		/// </summary>
		/// <param name="iarrReturn">If method returns true, this array will 
		/// contain the following index,length pairs relative to Haystack 
		/// (not relative to iStart): location of sAttribName, length of 
		/// sAttribName, location of associated value, and length of associated 
		/// value (4 values total--using or creating array with 4 or more 
		/// indeces--creates 30 if null or if too small).</param>
		/// <param name="Haystack">The html data, including a list of style assignments</param>
		/// <param name="sAttribName">The attribute name to find inside Haystack</param>
		/// <param name="StartInHaystack">Where in Haystack to start examining</param>
		/// <param name="LengthInHaystack">Number of characters in Haystack to examine</param>
		/// <returns>returns whether iarrReturn is complete and usable</returns>
		public static bool FindLastFuzzyStyleAssignment(ref int[] iarrReturn, string Haystack, string sAttribName, int StartInHaystack, int LengthInHaystack) {
			bool bGood=false;
			if (iarrReturn==null||iarrReturn.Length<2) iarrReturn=new int[30];//uses 2
			iarrReturn[0]=RString.LastIndexOf_OnlyIfWholeWord(Haystack,sAttribName,StartInHaystack,LengthInHaystack);
			iarrReturn[1]=RString.SafeLength(sAttribName);
			iarrReturn[2]=-1;
			int iHaystackEnder=StartInHaystack+LengthInHaystack;
			if (iarrReturn[0]>=0) {
				iarrReturn[2]=iarrReturn[0]+sAttribName.Length;
				bool bInRange=RString.MovePastWhiteSpaceOrChar(ref iarrReturn[2],Haystack,':');
				if (iarrReturn[2]>iHaystackEnder) bInRange=false;
				if (bInRange) {
					int iValEnder=iarrReturn[2];
					bInRange=RString.MoveToOrStayAtSpacing(ref iValEnder, Haystack);
					if (iValEnder>iHaystackEnder) bInRange=false;
					if (bInRange) {
						iarrReturn[3]=iValEnder-iarrReturn[2];
						bGood=true;
					}
					else {
						iarrReturn[3]=0;
						RReporting.Warning("Value not found after style assignment name or range ends before value","finding attribute assignment value","FindLastFuzzyStyleAssignment");
					}
				}
				else RReporting.Warning("This method got an out-of-range starting point, indicating corruption in the calling method","finding attribute assignment name","FindLastFuzzyStyleAssignment");
			}
			return bGood;
		}//FindLastFuzzyStyleAssignment
		//public static int[] iarrAssignTemp=new int[32];
		//public bool SetOrCreateStyle(string sName, string sValue) { //replaced by SetStyleAttrib; formerly StyleSetOrCreate
		//	bool bGood=false;
		//	try {
		//		//TODO: set by modifying sOpener
		//		//-find start and length of set statement
		//		bGood=FindLastFuzzyStyleAssignment(ref iarrAssignTemp,this.sOpener,sName,0);
		//		if (bGood) {
		//			
		//		}
		//		else { //else not found so create
		//		}
		//	}
		//	catch (Exception exn) {
		//		RReporting.ShowExn(exn);
		//	}
			//return bGood;
		//}//end StyleSetOrCreate
		/* TODO:?
		public void EnterTextCommand(char cAsciiCommand) {
			rformrRoot.sgmlNow.InsertTextCommand(cAsciiCommand);
		}
		*/
		// Returns text but only if IsLeaf.
		//public string MyText() {
		//	if (IsLeaf) {
				//if (!rformrRoot.sgmlNow.bUpdateCousin) {
		//			return sText;
				//}
		//	}
		//	return "";
		//}
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
			int iCount=RString.GetMultipleAssignmentLocations(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener, sName, '=', ' ',ContentPropertiesStart(),iContentPropertiesEndEx,false);
			try {
				if (iCount>0) {//at least 0 and 1 are usable
					//found existing property so overwrite last instance of it
					int MatchingPropRelHalfindex=iCount-1;
					if ( RString.IsBlank(sValue) &&((PropValueStarterEnderIndeces[MatchingPropRelHalfindex*2+1]-PropValueStarterEnderIndeces[MatchingPropRelHalfindex*2])>0) ) sValue="\"\""; //ONLY do this if value exists already
					if (RString.IsNotBlank(sValue)) sOpener=RString.SafeSubstring(sOpener,0,PropValueStarterEnderIndeces[MatchingPropRelHalfindex*2])+sValue+RString.SafeSubstring(sOpener,PropValueStarterEnderIndeces[MatchingPropRelHalfindex*2+1]); //ok to only do if not blank, since set to nonblank (i.e. "\"\"") if original value was nonblank
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
			//RReporting.Debug("SetStyleAttrib: sOpener is "+RReporting.StringMessage(sOpener,true));//debug only
			if (RString.IsBlank(sOpener)) sOpener="<style=\"\">";
			if (sOpener=="<>") sOpener="< style=\"\">";//space first to represent that it is a tagword-less tag
			//RReporting.Debug("SetStyleAttrib: sOpener with style  "+RReporting.StringMessage(sOpener,true));//debug only
			int iPropEndEx=ContentPropertiesEndEx();
			int StylePropCount=RString.GetMultipleAssignmentLocations(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener,"style",'=',' ',ContentPropertiesStart(),iPropEndEx,false);
			int iAttribs=0;
			string sStyleOpener=" style=\"";//(sOpener!="<>"?" style=\"":"style=\""); //space first to represent that it is a tagword-less tag
			int LastStylePropRelHalfindex=-1;
			int iAttrib=-1;
			if (RString.IsBlank(sValue)) {
				RReporting.ShowErr("Cannot add blank style value");
				sValue="";
			}
			if (PropValueStarterEnderIndeces==null) {
				PropValueStarterEnderIndeces=new int[10*2];
				PropValueStarterEnderIndeces[0]=-1;
			}
			try {
				if (StylePropCount<1) {//if no properties, add a style property to contain the style attribute
					//RReporting.Debug("SetStyleAttrib: No style property (ContentPropertiesStart():"+ContentPropertiesStart()+"; StylePropCount:"+StylePropCount+"; iPropEndEx:"+iPropEndEx+") in  "+RReporting.StringMessage(sOpener,true));//debug only
					StylePropCount=1;
					//RReporting.DebugWrite(sOpener);
					sOpener=RString.SafeInsert(sOpener,iPropEndEx,sStyleOpener+"\"");
					PropValueStarterEnderIndeces[0]=iPropEndEx;
					PropValueStarterEnderIndeces[1]=iPropEndEx+sStyleOpener.Length+1;//+1 for closing quote
					//RReporting.DebugWriteLine(" becomes "+RReporting.StringMessage(sOpener,true));//debug only
					//if (StylePropCount<1) RReporting.Debug("No style property found even after inserted! (in substring \""+RString.SafeSubstringByExclusiveEnder(sOpener,ContentPropertiesStart(),iPropEndEx)+"\")");
				}
				if (StylePropCount>0) {
					LastStylePropRelHalfindex=StylePropCount-1;//get the last style property (PropValueStarterEnderIndeces only contains style property--see GetMultipleAssignmentLocations above)
					//RReporting.Debug("SetStyleAttrib: style property (including quotes) "+LastStylePropRelHalfindex+" at "+PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2]+" (to before "+PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2+1]+") is "+RString.SafeSubstringByExclusiveEnder(sOpener,PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2],PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2+1]));//debug only
					RString.ShrinkToInsideQuotes(sOpener,ref PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2], ref PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2+1]);
					//RReporting.Debug("SetStyleAttrib: style property (inside quotes) "+LastStylePropRelHalfindex+" at "+PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2]+" (to before "+PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2+1]+") is "+RString.SafeSubstringByExclusiveEnder(sOpener,PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2],PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2+1]));//debug only
					iAttribs=RString.GetMultipleAssignmentLocations(ref AttribNameStarterEnderIndeces, ref AttribValueStarterEnderIndeces, sOpener, sName, ':', ';', PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2], PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2+1], false);
					iAttrib=iAttribs-1;
					if (iAttribs<1||bForceAppend) {
						//if there is a style property but the named attribute needs to be created
						//RReporting.Debug("SetStyleAttrib: found "+iAttribs+" style attribs");//debug only
						iAttribs=1;
						iAttrib=iAttribs-1;
						if (AttribValueStarterEnderIndeces==null) AttribValueStarterEnderIndeces=new int[10*2];
						//RReporting.Debug("SetStyleAttrib: appending to style attrib "+iAttrib+" at "+PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2]+" in "+RReporting.StringMessage(sOpener,true));//debug only
						string sAppend=( ((LastStylePropRelHalfindex==0)||RString.EndsWithCharOrCharThenWhiteSpace(sOpener,';',PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2],PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2+1])) ? "" : "; ") + sName + ":";
						sOpener=RString.SafeInsert(sOpener,PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2],sAppend);
						PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2+1]+=sAppend.Length;
						AttribValueStarterEnderIndeces[iAttrib*2]=PropValueStarterEnderIndeces[LastStylePropRelHalfindex*2]+sAppend.Length;
						AttribValueStarterEnderIndeces[iAttrib*2+1]=AttribValueStarterEnderIndeces[iAttrib*2];//same as start since no value yet
					}//end if appending a new style attribute
					sOpener=RString.SafeSubstring(sOpener,0,AttribValueStarterEnderIndeces[iAttrib*2])+sValue+RString.SafeSubstring(sOpener,AttribValueStarterEnderIndeces[iAttrib*2+1]);
					//RReporting.Debug("SetStyleAttrib: final value is "+RReporting.StringMessage(sOpener,true)); //debug only
				}
				else RReporting.ShowErr("The style was not modified (SetStyleAttrib corruption)");
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn, "setting style attribute" , String.Format("rform SetStyleAttrib(...,bForceAppend={0}) {{sOpener:{1}; LastStylePropRelHalfindex:{2}; StylePropCount:{3}; iAttrib:{4}; iAttribs:{5}; {6}; {7}}}",bForceAppend?"yes":"no",RReporting.StringMessage(sOpener,true),LastStylePropRelHalfindex,StylePropCount,iAttrib,iAttribs,
				                                                                   RReporting.ArrayDebugStyle("PropValueStarterEnderIndeces",PropValueStarterEnderIndeces,false),
				                                                                   RReporting.ArrayDebugStyle("AttribValueStarterEnderIndeces",AttribValueStarterEnderIndeces,false) ) );
			}
			return bGood;
		}//end SetStyleAttrib
		public bool AppendStyleAttrib(string Name, string val) {//formerly AppendStyle
			return SetStyleAttrib(Name, val, true);
		}//end AppendStyleAttrib
		public bool HasStyleAttrib(string sName) { //aka StyleAttributeExists
			bool bFound=false;
			try {
				int StylePropCount=RString.GetMultipleAssignmentLocations(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener, "style", '=', ' ', ContentPropertiesStart(),ContentPropertiesEndEx(),false);
				for (int StylePropertyNowHalfindex=0; StylePropertyNowHalfindex<StylePropCount; StylePropertyNowHalfindex++) {
					if (RString.GetMultipleAssignmentLocations(ref AttribNameStarterEnderIndeces, ref AttribValueStarterEnderIndeces, sOpener, sName, ':', ';', PropValueStarterEnderIndeces[StylePropertyNowHalfindex*2], PropValueStarterEnderIndeces[StylePropertyNowHalfindex*2+1], false)>0) {
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
			return RString.GetMultipleAssignmentLocations(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener, sName, '=', ' ',ContentPropertiesStart(),ContentPropertiesEndEx(),false) > 0;
		}//end HasProperty
		public bool RemoveProperty(string sName) {
			int iIterations=0;
			bool bRemoved=false;
			int iCount=1;
			try {
				while (iCount>0) {
					iCount=RString.GetMultipleAssignmentLocations(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener,sName,'=',' ',ContentPropertiesStart(),ContentPropertiesEndEx(),false);
					if (iCount>0) {
						sOpener=RString.SafeSubstring(sOpener,0,PropNameStarterEnderIndeces[0])+RString.SafeSubstring(sOpener,PropValueStarterEnderIndeces[1]);//intentionally use PropName Start and PropVal EndEx (remove whole property assignment)
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
			int MatchingPropCount=RString.GetMultipleAssignmentLocationsI(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener, sName, '=', ' ',ContentPropertiesStart(),ContentPropertiesEndEx());
			if (MatchingPropCount>0) { //for (int iLastProp=0; iLastProp<MatchingPropCount; iLastProp+=2) {
				int iLastProp=MatchingPropCount-1;
				if (RemoveQuotes) RString.ShrinkToInsideQuotes(sOpener, ref PropValueStarterEnderIndeces[iLastProp], ref PropValueStarterEnderIndeces[iLastProp+1]);
				if (PropValueStarterEnderIndeces[iLastProp+1]-PropValueStarterEnderIndeces[iLastProp]>0) sReturn=RString.SafeSubstring(sOpener,PropValueStarterEnderIndeces[iLastProp],PropValueStarterEnderIndeces[iLastProp+1]-PropValueStarterEnderIndeces[iLastProp]);
			}
			return sReturn;
		}//end GetProperty
		///<summary>
		///get html property value by case-insentitive name
		///</summary>
		public string GetProperty_AssumingNameIsLower(string sName, bool RemoveQuotes) {
			string sReturn="";
			int MatchingPropCount=RString.GetMultipleAssignmentLocationsI_AssumingNameIsLower(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener, sName, '=', ' ',ContentPropertiesStart(),ContentPropertiesEndEx());
			if (MatchingPropCount>0) { //for (int iLastProp=0; iLastProp<MatchingPropCount; iLastProp+=2) {
				int iLastProp=MatchingPropCount-1;
				if (RemoveQuotes) RString.ShrinkToInsideQuotes(sOpener, ref PropValueStarterEnderIndeces[iLastProp], ref PropValueStarterEnderIndeces[iLastProp+1]);
				if (PropValueStarterEnderIndeces[iLastProp+1]-PropValueStarterEnderIndeces[iLastProp]>0) sReturn=RString.SafeSubstring(sOpener,PropValueStarterEnderIndeces[iLastProp],PropValueStarterEnderIndeces[iLastProp+1]-PropValueStarterEnderIndeces[iLastProp]);
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
			try {
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
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple,"InterchangeableCascadeGroupIndex");
			}
			return iReturn;
		}//end InterchangeableCascadeGroup
		///<summary>
		///Applies stylesheet, header style in order of appearance in header, AND ancestor styles
		///</summary>
		private string GetCascadedFuzzyAttribApplyingToTag(string sOpeningTag, string sFuzzyAttrib, int iNodeIndex) {
			string sReturn=null;
			try {
				if (iNodeIndex<0) RReporting.Warning("Bad saved self-index in an RForm (RForms corruption)--will not be able to get parent styles");
				if (ContainerPage!=null) sReturn=ContainerPage.GetCascadedFuzzyAttribApplyingToTag(sOpeningTag,sFuzzyAttrib,iNodeIndex);
				else {
					RReporting.ShowErr("Null  ("+(this.Parent!=null?"OK":"null")+" parent node) for an RForm (RForms corruption)","getting cascaded attrib","GetCascadedFuzzyAttribApplyingToTag(sOpeningTag="+RReporting.StringMessage(sOpeningTag,true)+",sFuzzyAttrib="+RReporting.StringMessage(sFuzzyAttrib,true)+",iNodeIndex="+iNodeIndex+") {name property of this:"+this.GetProperty_AssumingNameIsLower("name",true)+"; this.sTagwordLower:"+this.sTagwordLower+"; this.Index:"+this.Index+"; this.ParentIndex:"+this.ParentIndex+"}");
					sReturn=null;//this is OK since parentpage is allowed to be null (parent NODE is NOT)
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple,"GetCascadedFuzzyAttribApplyingToTag");
			}
			return sReturn;
		}//end GetCascadedFuzzyAttribApplyingToTag
		///<summary>
		///sFuzzyAttrib can be something like "bgcolor" or "background-color" (case-insensitive)
		///</summary>
		public string CascadedFuzzyAttrib(string sFuzzyAttrib) {//formerly CascadedProperty
			ArrayList alPossible=null;
			string sReturn=null;
			string sValNow;
			try {
				if (sFuzzyAttrib!=null) {
					if (RString.HasUpperCharacters(sFuzzyAttrib)) sFuzzyAttrib=sFuzzyAttrib.ToLower();
					if (RReporting.bUltraDebug) RReporting.sParticiple="getting cascaded fuzzy attribute";
					sReturn=GetCascadedFuzzyAttribApplyingToTag(sOpener,sFuzzyAttrib,iIndex);
					if (RReporting.bUltraDebug) RReporting.sParticiple="getting cascade group index from fuzzy attribute";
					int iCascadeGroup=InterchangeableCascadeGroupIndex(sFuzzyAttrib);
					if (iCascadeGroup>=0) {
						if (RReporting.bUltraDebug) RReporting.sParticiple="getting html property using cascade group";
						for (int iProperty=InterchangeableProperty[iCascadeGroup].Length-1; iProperty>=0; iProperty--) {
							sValNow=GetProperty(InterchangeableProperty[iCascadeGroup][iProperty]);
							if (RReporting.IsNotBlank(sValNow)) {
								sReturn=sValNow; //do NOT break, keep going to get more specific values from the cascade group
							}
						}
						if (RReporting.bUltraDebug) RReporting.sParticiple="getting style attribute using cascade group";
						for (int iStyleAttrib=InterchangeableStyleAttribute[iCascadeGroup].Length-1; iStyleAttrib>=0; iStyleAttrib--) {
							if (RReporting.bUltraDebug) RReporting.sParticiple="accessing cascade group ["+iCascadeGroup+"] item ["+iStyleAttrib+"]";
							sValNow=GetStyleAttrib(InterchangeableStyleAttribute[iCascadeGroup][iStyleAttrib]);
							if (RReporting.IsNotBlank(sValNow)) {
								sReturn=sValNow; //do NOT break, keep overwriting to get more specific values from the cascade group
							}
						}
					}//end if fuzzy attribute is part of interchangeable cascade group
					else {
						if (RReporting.bUltraDebug) RReporting.sParticiple="getting non-fuzzy attribute using literal attribute name";
						sValNow=GetProperty(sFuzzyAttrib);
						if (RReporting.IsNotBlank(sValNow)) sReturn=sValNow;
						sValNow=GetStyleAttrib(sFuzzyAttrib);
						if (RReporting.IsNotBlank(sValNow)) sReturn=sValNow;
					}
				}
				else RReporting.ShowErr("Fuzzy attribute was null (RForms corruption)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple);
			}
			return sReturn;
		}//end CascadedFuzzyAttrib
		private int ContentPropertiesStart() {
			int iOpener=RString.SafeIndexOf(sOpener,'<');
			int iBracket=iOpener;
			int iCloser=0;
			if (iOpener>=0) {
				iOpener=RString.IndexOfWhiteSpaceOrChar(sOpener,'>',iOpener);
				iCloser=RString.IndexOf(sOpener,'>');
				if (iOpener>-1) {
					if (iOpener==iCloser&&RString.CompareAt(sOpener,'<',iBracket-1)) {
						RReporting.Warning("Tagword missing in "+sOpener);
						iOpener=iBracket+1;
					}
					//RReporting.Debug("ContentPropertiesStart: IndexOfWhiteSpaceOrChar OK ("+iOpener+")");
					iOpener=RString.IndexOfNonWhiteSpace(sOpener,iOpener);
					if (iOpener<0) {
						RReporting.Debug("ContentPropertiesStart: no nonwhitespace (zero-length opening tag!)");
						iOpener=0;
					}
					else {
						//RReporting.Debug("ContentPropertiesStart: nonwhitespace OK ("+iOpener+")");
					}
				}
				else {
					RReporting.Debug("ContentPropertiesStart: no opening whitespace (tagword is missing)");
					iOpener=0;
				}
			}
			else {
				RReporting.Debug("ContentPropertiesStart: no opening '<' sign {iIndex:"+iIndex.ToString()+"; ParentIndex:"+this.ParentIndex.ToString()+"; sOpener:"+RReporting.StringMessage(sOpener,true)+"; sContent:"+RReporting.StringMessage(sContent,true)+"; sCloser:"+RReporting.StringMessage(sCloser,true)+"}");
				iOpener=0;
			}
			return iOpener;
		}//end ContentPropertiesStart
		/// <summary>
		/// Last index (exclusive ender) in sOpener range of usable properties excluding self-closing tag slash
		/// </summary>
		/// <returns></returns>
		private int ContentPropertiesEndEx() {
			int iEndEx=-1;
			if (sOpener!=null) iEndEx=sOpener.LastIndexOf(">");
			if (iEndEx<0&&sOpener!=null) iEndEx=sOpener.Length;
			if (sOpener!=null && iEndEx<sOpener.Length && sOpener[iEndEx]=='>' && (iEndEx-1>=0) &&sOpener[iEndEx-1]=='/' ) iEndEx--;//excludes self-closing tag slash
			return iEndEx;
		}
		public bool StyleAttribEqualsI_AssumingNeedleIsLower(string NameInHaystack, string NeedleValue) {
			bool bReturn=false;
			int MatchingPropCount=RString.GetMultipleAssignmentLocations(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener,"style",'=',' ',ContentPropertiesStart(),ContentPropertiesEndEx(),false);
			try {
				for (int iProp=MatchingPropCount-1; iProp>=0; iProp--) {
					int iAttribs=RString.GetMultipleAssignmentLocations(ref AttribNameStarterEnderIndeces, ref AttribValueStarterEnderIndeces, sOpener,NameInHaystack,':',';',PropValueStarterEnderIndeces[iProp*2],PropValueStarterEnderIndeces[iProp*2+1],false);
					for (int iAttrib=iAttribs-1; iAttrib>=0; iAttrib--) {
						if ((AttribValueStarterEnderIndeces[iAttrib*2+1]-AttribValueStarterEnderIndeces[iAttrib*2])>0) {
							//name was already checked in GetMultipleAssignmentLocations
							bReturn=RString.CompareAtI(sOpener,NeedleValue,AttribValueStarterEnderIndeces[iAttrib*2],AttribValueStarterEnderIndeces[iAttrib*2+1],false);
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
		///Gets self-cascaded style attribute, NOT including globals [parent, stylesheet, header]
		///</summary>
		public string GetStyleAttrib(string sName) {
			string sReturn=null;
			if (RReporting.bUltraDebug) RReporting.sParticiple="getting style attribute";
			int MatchingPropCount=RString.GetMultipleAssignmentLocations(ref PropNameStarterEnderIndeces, ref PropValueStarterEnderIndeces, sOpener,"style",'=',' ',ContentPropertiesStart(),ContentPropertiesEndEx(),false);
			try {
				for (int iProp=MatchingPropCount-1; iProp>=0; iProp--) {
					int iAttribs=RString.GetMultipleAssignmentLocations(ref AttribNameStarterEnderIndeces, ref AttribValueStarterEnderIndeces, sOpener,sName,':',';',PropValueStarterEnderIndeces[iProp*2],PropValueStarterEnderIndeces[iProp*2+1],false);
					if (RReporting.bUltraDebug) RReporting.sParticiple="accessing assignment indeces";
					for (int iAttrib=iAttribs-1; iAttrib>=0; iAttrib--) {
						if ((AttribValueStarterEnderIndeces[iAttrib*2+1]-AttribValueStarterEnderIndeces[iAttrib*2])>0) {
							//name was already checked in GetMultipleAssignmentLocations
							sReturn=RString.SafeSubstring(sOpener,AttribValueStarterEnderIndeces[iAttrib*2],AttribValueStarterEnderIndeces[iAttrib*2+1]-AttribValueStarterEnderIndeces[iAttrib*2]);
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
					if (bLeft) zoneInner.Left=rectAbs.X+RConvert.ToInt(GetStyleAttrib("margin-left"));
					if (bTop) zoneInner.Top=rectAbs.Y+RConvert.ToInt(GetStyleAttrib("margin-top"));
					if (bRight) zoneInner.Right=(rectAbs.X+rectAbs.Width)-RConvert.ToInt(GetStyleAttrib("margin-right"));
					if (bBottom) zoneInner.Bottom=(rectAbs.Y+rectAbs.Height)-RConvert.ToInt(GetStyleAttrib("margin-bottom"));
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
		//TODO: replace with html-relative-cursor-based rendering
		public void Render(RImage riDest, bool bAsActive) {
			try {
				if (RReporting.bDebug) RReporting.sParticiple="starting to render node";
				if (textbox!=null) textbox.Render(riDest,bAsActive);
				else {
					if (RReporting.bUltraDebug) RReporting.sParticiple="getting node color";
					string sBackgroundColor=CascadedFuzzyAttrib("background-color");
					RPaint rpaintNow=null;
					if (sBackgroundColor!=null) rpaintNow=new RPaint(RConvert.ToColor(sBackgroundColor));//debug performance (cache this)
					else rpaintNow=new RPaint(RForms.colorBackgroundDefault);
					if (RReporting.bUltraDebug) RReporting.sParticiple="accessing node color";
					if (colorBack.A>0||TagwordLower=="button"||TypeLower=="button") {
						rectAbs.Width++;//debug only
						rectAbs.Height++;//debug only
						if (RReporting.bUltraDebug) RReporting.sParticiple="drawing button outline "+rectAbs.ToString()+" with text "+RReporting.StringMessage(Text,true);
						riDest.DrawRectStyleCropped(rpaintNow,rectAbs);
						//RImage.rpaintFore.SetArgb(colorBack.A,colorBack.R,colorBack.G,colorBack.B);//debug only
						//RImage.rpaintFore.SetRgb(255,0,255);//debug only
						//riDest.DrawRectFilledCropped(rectAbs);//debug only
					}
					RenderText(riDest,Text);//rfont.Render(ref riDest, zoneInner, Text);
				}
				RReporting.sParticiple="drawing node outline";
				if (bAsActive) {
					//see also RTextBox.Render
					RPaint rpaintPrev=RImage.rpaintFore;
					RImage.rpaintFore=RForms.rpaintActive;//RImage.rpaintFore.Set(colorActive);
					riDest.DrawRectCropped(rectAbs.X+1,rectAbs.Y+1,rectAbs.Width-2,rectAbs.Height-2);
					RImage.rpaintFore=rpaintPrev;
				}
				else {
					RPaint rpaintPrev=RImage.rpaintFore;
					RImage.rpaintFore=RForms.rpaintControlLine;//RImage.rpaintFore.SetRgb(64,64,64);//debug only
					riDest.DrawRectCropped(rectAbs.X+1,rectAbs.Y+1,rectAbs.Width-2,rectAbs.Height-2);//riDest.DrawRectCropped(rectAbs);//debug only
					RImage.rpaintFore=rpaintPrev;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple,"RForm Render(RImage,bAsActive="+RConvert.ToString(bAsActive)+")");
			}
		}//end Render primary overload
		public void RenderText(RImage riDest, string sText) {
			if (RReporting.bUltraDebug) RReporting.sParticiple="accessing form rfont {this.rfont:"+RFont.ToString(this.rfont)+"}";
			//NOTE: Uses RImage.rpaintFore for color (see rfont RenderLine)
			rfont.Render(ref riDest, zoneInner, sText);//TODO: horizontal and vertical alignment
		}
		#endregion Render
	}//end class RForm
}//end namespace
