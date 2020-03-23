/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

/*

TODO (x=done, in order of priority):
x-W3C named colors: http://www.w3schools.com/html/html_colornames.asp
-HTML tag can have attributes! ( i.exn. <HTML LANG=EN-US> )
-when text cursor is inside a tag, draw jagged border around sourcecode to matching tag
-Old vs New html text formatting attributes:
	old <em> is like <i> and old <strong> is like <b>
-Body tag attributes
	<body alink="#00ff00" bgcolor="#ffffff" link="#008000" vlink="#800000">
-font-size 40pt=53px on test system @ 1024x768 w/ WinXP
-TD actually requires <td align="left" valign="top"> or will be valign center or something unpredictable
-PRE - Preformatted text:
	 <PRE>
	 First line
	 Next line
	 </PRE>
-SMALL and BIG tags decrease/increase the font size by about 2.5pt, rounded up, according to my test.
-Basefont, and standard default:
	<BASEFONT SIZE="5"> //the default basefont size is 3, and there are 7 sizes (max is <FONT SIZE="7">M</FONT>)
-No wrapping, html way AND css way 
 //NOBR: IE wraps to screen (or window?) width, Firefox wraps to page width
 <nobr>prevents wrapping<wbr>allows a wrap here</wbr> in nobr tags.</nobr>
 //CSS:
 <span style="white-space:nowrap">prevents wrapping</span>
-Relative font sizes:
	<FONT SIZE="+1">M</FONT>
	<FONT SIZE="-1">M</FONT>
-Horizontal rule:
	<hr width=50% size=10 align=right noshade> //noshade makes a nice solid horz line (rounded ends in firefox)!
-All white-space values:
    - normal (whitespace treated as one space)
    - nowrap (no wrapping except <br>)
    - pre (whitespace treated as literal [preformatted text])
-Always save alt, which is actually required for images:
  <img src="image_name.gif" width="32" height="32" hspace="16" border="0" alt=""> 
-<META HTTP-EQUIV="Pragma" CONTENT="no-cache">
-properties of "td": colspan AND rowspan
-Explicitly-self-closing tags (i.e. <br/>
-A comment DECLARATION starts with <!, followed by zero or more comments, followed by >.
  (Inside a comment declaration) a COMMENT starts and ends with "--", and does not contain any occurrence of "--".
  --TODO: Also account for "//" comments, BUT make sure that //--> still works for ending a full comment
-Title attributes, as opposed to the title tag
  --The title attribute is usually rendered as a "tooltip", but
    can be rendered different ways by different types of browsers:
	<a href="diving.jpg" title="Scuba Diving Photo">Diving</a>
-Conditionals, & nested ones
	<!--[if gte IE 5.5]>
	<![if lt IE 7]>
		non-ie7 code here
	<![endif]>
	<![endif]-->
-Image area map (multiple clickable areas on one image):
 <MAP NAME="goto"> 
  <AREA COORDS="0,0,50,25" HREF="p24font.htm">
  <AREA COORDS="53,0,125,22" HREF="p00index.htm">
  <AREA COORDS="127,0,220,24" HREF="p000gloss.htm">
  <AREA COORDS="35,25,120,48" HREF="fuzzrule.htm">
  <AREA COORDS="121,23,190,48" HREF="js/home.htm" TARGET="_top">
  <AREA COORDS="192,23,240,48" HREF="thankyou.htm"></MAP>
  <IMG SRC="htmmap.jpg" ISMAP USEMAP="#goto" ALIGN="right" WIDTH=250 HEIGHT=50 BORDER=0 ALT="clickity click click....">
-All list types:
 <ol> ordered list (shows numbers not bullets)
  <ul>
 <li>Types of Lists:
	 <ol>
	 <li>Unordered List
	 <li>Ordered List
	 <li>Definition List
	 <li>Nested List
	 </ol>
 <li>Type=Attribute
	 <ul type="square">
	<li>type can be "disc"
	<li>type can be "circle"
	 </ul>
  </ul>
 //Definition list: content of dd should appear indented on next line:
  <dl>
   <dt>Unordered List<dd>defined with bullets
   <dt>Ordered List<dd>alpha or numeric display
   <dt>Definition List<dd>gives a definition
   <dt>Nested List<dd>list inside a list
  </dl>
  //using def list so you can make your own image bullets without style tags:
  <DL>
   <DD><img src="image_name.gif"> Line 1 Text</DD>
   <DD><img src="image_name.gif"> Line 2 Text</DD>
   <DD><img src="image_name.gif"> Line 3 Text</DD>
  </DL> 
--Frames and noframes working together:
	<FRAMESET ROWS="100%" COLS="375,*">  
	 <FRAMESET ROWS="62,*">  
	  <FRAME NAME="Frame0" SRC="frame0.htm" SCROLLING="no" MARGINHEIGHT=2 MARGINWIDTH=0>  
	  <FRAME NAME="Frame1" SRC="frame1.htm" SCROLLING="AUTO">  
	 </FRAMESET>
	 <FRAME NAME="Frame2" SRC="frame2.htm" SCROLLING="AUTO"> 
	 <NOFRAMES>
	<BODY> Whole page for noframes browsers
	</BODY> 
	 </NOFRAMES> 
	</FRAMESET>

-Tags that DON'T seem to work in ANY browser:
  <spacer type="block" width="50" HEIGHT="40" ALIGN="left">
  <spacer type="horizontal" size="70">
  <spacer type="verticle" size="25">
  <MULTICOL COLS="2" GUTTER="25" WIDTH="300">
	The Multicol tag divides text up into newspaper or newsletter type columns. Each column will be of the same width.
	<MULTICOL COLS="2">
	  <FONT SIZE="2">
	  You can use other HTML tags within the Multicol tag, including other Multicol tags.
	  </FONT>
	</MULTICOL>
	The default value for the Gutter attribute is 10.
  </MULTICOL>

 */

using System;

namespace ExpertMultimedia {
	
	/// <summary>
	/// Graphical node saved as HTML, used by HTMLTool
	/// </summary>
	public class GNode {
		
		#region constants
		public const int TypeUndefined=0;
		public const int TypeMarkup=1;
		public const int TypeButton=2;
		public const int TypeSliderH=3;
		public const int TypeSliderW=4;
		public const int TypeTextArea=5;
		public const int TypeTextEdit=6;
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

		#region optional vars
		//public int iComponentIDOfOwner;
		  //which application (i.exn. retroengineclient or website)
		  //owns and maintains this GNode
		//public string sPreText;//may include newlines etc.
		//public string sContent;//text AFTER child nodes.
		//Var vProps=null; //TODO: allow valueless properties like "Checked"!
		Var vStyle=null; //cascaded style--GNoder derives it from ALL style&class props (in consecutive order)
		public string sToolTip="";
		public bool bSplit=false; //Whether the node was processed by DivideNode
		public uint bitsAttrib;
		public Var vFriend;//the Var in sgmldoc.vsRoots (or descendant) that this GNode represents
		#endregion optional vars
		
		#region required vars
		GNoder gnoderRoot=null;  //TODO: MUST set this
		public ITarget tgAbs; //absolute screen position derived from vProps width and vStyle width--set by parent;
		public IRect rectInner; //absolute screen position derived from margin styles (used by child; or by inner text if bLeaf)
		public int indexParent;
		public string sTagword; //tells how to render
		public bool bLeaf; //whether this can be drawn as text
		public int iType;
		public string sFastText; //TODO: implement this (only used if gnoderRoot.bUpdateHTML==false, otherwise GNoder gets it from sgmlNow.Substring)
		#endregion
		public Var vParent=null; //TODO: implement this (create Var [created from HTML] "re-parser") has links to sourcecode etc
		public GNode() {
			Init();
		}
		public GNode(GNoder gnoderRootX, int GNodeType, int xLoc, int yLoc, int Width, int Height, int IndexParent, bool Leaf, string FastText, string HTMLTag) {
			Init(gnoderRootX, GNodeType, xLoc, yLoc, Width, Height, IndexParent, Leaf, FastText, HTMLTag);
		}
		
		public void Init() {
			Init(null,0,16,16,16,16,0,true,"","");
		}
		public void Init(GNoder gnoderRootX, int GNodeType, int xLoc, int yLoc, int Width, int Height, int IndexParent, bool Leaf, string FastText, string HTMLTag) {
			gnoderRoot=gnoderRootX;
			iType=GNodeType;
			tgAbs=new ITarget();
			tgAbs.x=xLoc;
			tgAbs.y=yLoc;
			tgAbs.width=Width;
			tgAbs.height=Height;
			rectInner=new IRect(tgAbs.y+1, tgAbs.x+1, tgAbs.y+tgAbs.height-2, tgAbs.x+tgAbs.height-2);
			sFastText=FastText; //not used unless using gnoderRoot.bUpdateHTML==false
			indexParent=IndexParent;
			sTagword=HTMLTag; //tells how to render if GNode.TypeMarkup
			bLeaf=Leaf; //whether this can be drawn as text
		}
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
								//else change does not affect this gnode
							}
						}
					//}
				}
			}
			return ValidateLocVars();
		}
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
		public void EnterText(char cToInsertAtCursor) {
			gnoderRoot.sgmlNow.InsertText(Char.ToString(cToInsertAtCursor));
		}
		public void EnterText(string sToInsertAtCursor) {
			gnoderRoot.sgmlNow.InsertText(sToInsertAtCursor); //TODO: must shift all variables of all necessary nodes
		}
		public void EnterTextCommand(char cAsciiCommand) {
			gnoderRoot.sgmlNow.InsertTextCommand(cAsciiCommand);
		}
		/// <summary>
		/// Return
		/// </summary>
		/// <returns>Returns text but only if a leaf node.</returns>
		public string MyText() {
			if (bLeaf) {
				if (gnoderRoot.sgmlNow.bUpdateHTML==false) {
					return sFastText;
				}
			}
			return "";
		}
		//debug NYI UpdateStyle should be in HTMLPage so page size can be determined/accessed first
	/*
		public bool UpdateSize(int iGNode) {
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
			
			iType=GNodeType.Undefined;
			string sType="";
			if (sTag=="input") vsAttrib.Get(ref sType, "type");
			iType=GNodeType.Markup;
			return iType>0;
		}
		*/
	}
}
