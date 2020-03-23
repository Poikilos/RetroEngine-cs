// created on 12/30/2006 at 10:53 PM
// by Jake Gustafson (Expert Multimedia)You are looking at a brand new Petmate Foodmate Portion-Controllable Feeder (Holds 25lb). This item is brand new and is being sold at a great price! <i>-- Supplies are limited so please take advantage of this deal while they are available.</i>

// www.expertmultimedia.com

// Simplify processing by using ProcessChunk(string sTagCaseInsensitiveElseContent, int iStart)
//  --that method calls FixSpacing(ref string sTag) IF a tag along with
//  --this may allow removal of this file (because of no need for sDataLower), using Var instead.
//  --OR consider re-adding carrData and carrDataLower and adding string-like editing functions.


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
    - normal (Spacing treated as one space)
    - nowrap (no wrapping except <br>)
    - pre (Spacing treated as literal [preformatted text])
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
using System.IO;
using System.Text.RegularExpressions;
namespace ExpertMultimedia {
	///TODO: make this use a Liner? decide whether to do so, considering the issue of sDataLower
	///	-Liner could have sDataLower instead, which could help with case-insensitive searching and highlighting etc.
	///		-would only be updated at END of a ModGroup
	public class MarkupDoc {//formerly SGMLDoc
		public static readonly string[] sarrMarkupDoctype=new string[]{
			"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">",
			"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">",
			"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Frameset//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd\">"};
		public static readonly string[] sarrDoctypeDescription=new string[] {
			"This doctype allows only pure CSS formatting.",
			"This doctype allows properties outside of css format.",
			"This doctype allows frames."
		}
		//TODO: allow any tag  to be automatically closed if a parent tag is closed. 
		#region known tags (ONLY needed for saving/generating)
		public static int iInlineTagwords;
		public static string[] sarrInlineTagword=null; //tagwords with no text, i.e. img, br, or meta
		public static string[] sarrIndependentTagword=null;
		public static int iSpecialSyntaxTags;
		public static string[] sarrSpecialSyntaxOpener; //just text, like if THIS tag is a "<!--" tag then that is the special opening
		public static string[] sarrSpecialSyntaxCloser; //end of just text, like /-->
		#endregion known tags (ONLY needed for saving/generating)
		
		#region static variables
		public static string[] sarrKnownColor=null;
		public static byte[][] by2dKnownColorBGR=null;
		public static int iKnownColors=0;
		public static int iSetByteDepth=3;
		#endregion static variables
		
		#region variables
		private string sData;
		private string sDataLower;
		public string sFile;//file name.ext only
		public string sPath;//path excluding final slash
		public int iSelCodePos;
		public int iSelCodeLen;
		public bool bUpdateCousin=false; //whether an html renderer needs to re-grab it's text from this doc (i.e. if modified)
		//private int iLines;
		//public bool bShowCode=false; //TODO: use this
		public int Length {
			get {
				return sData.Length;
			}
		}
		public string Data {
			get {
				return sData;
			}
			set {
				SetAll(value,true);
			}
		}
		public string sPathFile {
			get {
				return sPath+char.ToString(System.IO.Path.DirectorySeparatorChar)+sFile;
			}
			set {
				string sTemp=value;
				if ( sTemp.EndsWith(char.ToString(System.IO.Path.DirectorySeparatorChar)) && (sTemp.Length>2) )
					sTemp=sTemp.Substring(0,sTemp.Length-1);
				int iSlashLast=value.LastIndexOf('/');
				sFile=value.Substring(iSlashLast+1);
				sPath=value.Substring(0,iSlashLast);
			}
		}
		#endregion variables

		//public static Var vColor;
		#region static constructor
		static MarkupDoc() {
			Console.Write("Preparing MarkupDoc...");//debug only
			vColor=null;
			try {
				iInlineTagwords=5;
				sarrInlineTagword=new string[iInlineTagwords];
				sarrInlineTagword[0]="img";
				sarrInlineTagword[1]="br";
				sarrInlineTagword[2]="hr";
				sarrInlineTagword[3]="li";
				sarrInlineTagword[4]="p";
				iIndependentTagwords=4;
				sarrIndependentTagword[0]="link";
				sarrIndependentTagword[1]="meta";
				sarrIndependentTagword[2]="basefont";
				sarrIndependentTagword[3]="!";//catches "!DOCTYPE", and "the empty comment" before it is processed below since 
				iSpecialSyntaxTags=3;
				sarrSpecialSyntaxOpener=new string[iSpecialSyntaxTags]; //just text, like <!-- tags
				sarrSpecialSyntaxOpener[0]="!--"; //OCCURS THIS WAY SOMETIMES: <script language="javascript> <!-- /*lots of code lines*/ //--> /*another newline*/ </script>
				sarrSpecialSyntaxOpener[1]="?php";
				sarrSpecialSyntaxOpener[2]="!"; //i.e. NOT !DOCTYPE, since DOCTYPE is without a closer!
				sarrSpecialSyntaxCloser=new string[iSpecialSyntaxTags]; //end of just text, like //-->
				sarrSpecialSyntaxCloser[0]="-->";
				sarrSpecialSyntaxCloser[1]="?>";
				sarrSpecialSyntaxCloser[2]=">";
				//sarrInlineTagword[8]="<?php";
				ResetKnownColors(47);
			iKnownColors=0;
			AddKnownColor("AliceBlue","#F0F8FF");
			AddKnownColor("AntiqueWhite","#FAEBD7");
			AddKnownColor("Aqua","#00FFFF");
			AddKnownColor("Aquamarine","#7FFFD4");
			AddKnownColor("Azure","#F0FFFF");
			AddKnownColor("Beige","#F5F5DC");
			AddKnownColor("Bisque","#FFE4C4");
			AddKnownColor("Black","#000000");
			AddKnownColor("BlanchedAlmond","#FFEBCD");
			AddKnownColor("Blue","#0000FF");
			AddKnownColor("BlueViolet","#8A2BE2");
			AddKnownColor("Brown","#A52A2A");
			AddKnownColor("BurlyWood","#DEB887");
			AddKnownColor("CadetBlue","#5F9EA0");
			AddKnownColor("Chartreuse","#7FFF00");
			AddKnownColor("Chocolate","#D2691E");
			AddKnownColor("Coral","#FF7F50");
			AddKnownColor("CornflowerBlue","#6495ED");
			AddKnownColor("Cornsilk","#FFF8DC");
			AddKnownColor("Crimson","#DC143C");
			AddKnownColor("Cyan","#00FFFF");
			AddKnownColor("DarkBlue","#00008B");
			AddKnownColor("DarkCyan","#008B8B");
			AddKnownColor("DarkGoldenRod","#B8860B");
			AddKnownColor("DarkGray","#A9A9A9");
			AddKnownColor("DarkGrey","#A9A9A9");
			AddKnownColor("DarkGreen","#006400");
			AddKnownColor("DarkKhaki","#BDB76B");
			AddKnownColor("DarkMagenta","#8B008B");
			AddKnownColor("DarkOliveGreen","#556B2F");
			AddKnownColor("Darkorange","#FF8C00");
			AddKnownColor("DarkOrchid","#9932CC");
			AddKnownColor("DarkRed","#8B0000");
			AddKnownColor("DarkSalmon","#E9967A");
			AddKnownColor("DarkSeaGreen","#8FBC8F");
			AddKnownColor("DarkSlateBlue","#483D8B");
			AddKnownColor("DarkSlateGray","#2F4F4F");
			AddKnownColor("DarkSlateGrey","#2F4F4F");
			AddKnownColor("DarkTurquoise","#00CED1");
			AddKnownColor("DarkViolet","#9400D3");
			AddKnownColor("DeepPink","#FF1493");
			AddKnownColor("DeepSkyBlue","#00BFFF");
			AddKnownColor("DimGray","#696969");
			AddKnownColor("DimGrey","#696969");
			AddKnownColor("DodgerBlue","#1E90FF");
			AddKnownColor("FireBrick","#B22222");
			AddKnownColor("FloralWhite","#FFFAF0");
			AddKnownColor("ForestGreen","#228B22");
			AddKnownColor("Fuchsia","#FF00FF");
			AddKnownColor("Gainsboro","#DCDCDC");
			AddKnownColor("GhostWhite","#F8F8FF");
			AddKnownColor("Gold","#FFD700");
			AddKnownColor("GoldenRod","#DAA520");
			AddKnownColor("Gray","#808080");
			AddKnownColor("Grey","#808080");
			AddKnownColor("Green","#008000");
			AddKnownColor("GreenYellow","#ADFF2F");
			AddKnownColor("HoneyDew","#F0FFF0");
			AddKnownColor("HotPink","#FF69B4");
			AddKnownColor("IndianRed ","#CD5C5C");
			AddKnownColor("Indigo ","#4B0082");
			AddKnownColor("Ivory","#FFFFF0");
			AddKnownColor("Khaki","#F0E68C");
			AddKnownColor("Lavender","#E6E6FA");
			AddKnownColor("LavenderBlush","#FFF0F5");
			AddKnownColor("LawnGreen","#7CFC00");
			AddKnownColor("LemonChiffon","#FFFACD");
			AddKnownColor("LightBlue","#ADD8E6");
			AddKnownColor("LightCoral","#F08080");
			AddKnownColor("LightCyan","#E0FFFF");
			AddKnownColor("LightGoldenRodYellow","#FAFAD2");
			AddKnownColor("LightGray","#D3D3D3");
			AddKnownColor("LightGrey","#D3D3D3");
			AddKnownColor("LightGreen","#90EE90");
			AddKnownColor("LightPink","#FFB6C1");
			AddKnownColor("LightSalmon","#FFA07A");
			AddKnownColor("LightSeaGreen","#20B2AA");
			AddKnownColor("LightSkyBlue","#87CEFA");
			AddKnownColor("LightSlateGray","#778899");
			AddKnownColor("LightSlateGrey","#778899");
			AddKnownColor("LightSteelBlue","#B0C4DE");
			AddKnownColor("LightYellow","#FFFFE0");
			AddKnownColor("Lime","#00FF00");
			AddKnownColor("LimeGreen","#32CD32");
			AddKnownColor("Linen","#FAF0E6");
			AddKnownColor("Magenta","#FF00FF");
			AddKnownColor("Maroon","#800000");
			AddKnownColor("MediumAquaMarine","#66CDAA");
			AddKnownColor("MediumBlue","#0000CD");
			AddKnownColor("MediumOrchid","#BA55D3");
			AddKnownColor("MediumPurple","#9370D8");
			AddKnownColor("MediumSeaGreen","#3CB371");
			AddKnownColor("MediumSlateBlue","#7B68EE");
			AddKnownColor("MediumSpringGreen","#00FA9A");
			AddKnownColor("MediumTurquoise","#48D1CC");
			AddKnownColor("MediumVioletRed","#C71585");
			AddKnownColor("MidnightBlue","#191970");
			AddKnownColor("MintCream","#F5FFFA");
			AddKnownColor("MistyRose","#FFE4E1");
			AddKnownColor("Moccasin","#FFE4B5");
			AddKnownColor("NavajoWhite","#FFDEAD");
			AddKnownColor("Navy","#000080");
			AddKnownColor("OldLace","#FDF5E6");
			AddKnownColor("Olive","#808000");
			AddKnownColor("OliveDrab","#6B8E23");
			AddKnownColor("Orange","#FFA500");
			AddKnownColor("OrangeRed","#FF4500");
			AddKnownColor("Orchid","#DA70D6");
			AddKnownColor("PaleGoldenRod","#EEE8AA");
			AddKnownColor("PaleGreen","#98FB98");
			AddKnownColor("PaleTurquoise","#AFEEEE");
			AddKnownColor("PaleVioletRed","#D87093");
			AddKnownColor("PapayaWhip","#FFEFD5");
			AddKnownColor("PeachPuff","#FFDAB9");
			AddKnownColor("Peru","#CD853F");
			AddKnownColor("Pink","#FFC0CB");
			AddKnownColor("Plum","#DDA0DD");
			AddKnownColor("PowderBlue","#B0E0E6");
			AddKnownColor("Purple","#800080");
			AddKnownColor("Red","#FF0000");
			AddKnownColor("RosyBrown","#BC8F8F");
			AddKnownColor("RoyalBlue","#4169E1");
			AddKnownColor("SaddleBrown","#8B4513");
			AddKnownColor("Salmon","#FA8072");
			AddKnownColor("SandyBrown","#F4A460");
			AddKnownColor("SeaGreen","#2E8B57");
			AddKnownColor("SeaShell","#FFF5EE");
			AddKnownColor("Sienna","#A0522D");
			AddKnownColor("Silver","#C0C0C0");
			AddKnownColor("SkyBlue","#87CEEB");
			AddKnownColor("SlateBlue","#6A5ACD");
			AddKnownColor("SlateGray","#708090");
			AddKnownColor("SlateGrey","#708090");
			AddKnownColor("Snow","#FFFAFA");
			AddKnownColor("SpringGreen","#00FF7F");
			AddKnownColor("SteelBlue","#4682B4");
			AddKnownColor("Tan","#D2B48C");
			AddKnownColor("Teal","#008080");
			AddKnownColor("Thistle","#D8BFD8");
			AddKnownColor("Tomato","#FF6347");
			AddKnownColor("Turquoise","#40E0D0");
			AddKnownColor("Violet","#EE82EE");
			AddKnownColor("Wheat","#F5DEB3");
			AddKnownColor("White","#FFFFFF");
			AddKnownColor("WhiteSmoke","#F5F5F5");
			AddKnownColor("Yellow","#FFFF00");
			AddKnownColor("YellowGreen","#9ACD32");

				/*
				vColor=new Var();//TODO: set max accordingly
				vColor.SetPixByHex("AliceBlue","F0F8FF");
				vColor.SetPixByHex("AntiqueWhite","FAEBD7"); 	 
				vColor.SetPixByHex("Aqua","00FFFF");
				vColor.SetPixByHex("Aquamarine","7FFFD4");
				vColor.SetPixByHex("Azure","F0FFFF"); 
				vColor.SetPixByHex("Beige","F5F5DC");
				vColor.SetPixByHex("Bisque","FFE4C4");
				vColor.SetPixByHex("Black","000000");
				vColor.SetPixByHex("BlanchedAlmond","FFEBCD");
				vColor.SetPixByHex("Blue","0000FF");
				vColor.SetPixByHex("BlueViolet","8A2BE2");
				vColor.SetPixByHex("Brown","A52A2A");
				vColor.SetPixByHex("BurlyWood","DEB887");
				vColor.SetPixByHex("CadetBlue","5F9EA0");
				vColor.SetPixByHex("Chartreuse","7FFF00");
				vColor.SetPixByHex("Chocolate","D2691E");
				vColor.SetPixByHex("Coral","FF7F50");
				vColor.SetPixByHex("CornflowerBlue","6495ED");
				vColor.SetPixByHex("Cornsilk","FFF8DC");
				vColor.SetPixByHex("Crimson","DC143C");
				vColor.SetPixByHex("Cyan","00FFFF");
				vColor.SetPixByHex("DarkBlue","00008B");
				vColor.SetPixByHex("DarkCyan","008B8B");
				vColor.SetPixByHex("DarkGoldenRod","B8860B");
				vColor.SetPixByHex("DarkGray","A9A9A9");
				vColor.SetPixByHex("DarkGrey","A9A9A9");
				vColor.SetPixByHex("DarkGreen","006400");
				vColor.SetPixByHex("DarkKhaki","BDB76B");
				vColor.SetPixByHex("DarkMagenta","8B008B");
				vColor.SetPixByHex("DarkOliveGreen","556B2F");
				vColor.SetPixByHex("Darkorange","FF8C00");
				vColor.SetPixByHex("DarkOrchid","9932CC");
				vColor.SetPixByHex("DarkRed","8B0000");
				vColor.SetPixByHex("DarkSalmon","E9967A");
				vColor.SetPixByHex("DarkSeaGreen","8FBC8F");
				vColor.SetPixByHex("DarkSlateBlue","483D8B");
				vColor.SetPixByHex("DarkSlateGray","2F4F4F");
				vColor.SetPixByHex("DarkSlateGrey","2F4F4F");
				vColor.SetPixByHex("DarkTurquoise","00CED1");
				vColor.SetPixByHex("DarkViolet","9400D3");
				vColor.SetPixByHex("DeepPink","FF1493");
				vColor.SetPixByHex("DeepSkyBlue","00BFFF");
				vColor.SetPixByHex("DimGray","696969");
				vColor.SetPixByHex("DimGrey","696969");
				vColor.SetPixByHex("DodgerBlue","1E90FF");
				vColor.SetPixByHex("FireBrick","B22222");
				vColor.SetPixByHex("FloralWhite","FFFAF0");
				vColor.SetPixByHex("ForestGreen","228B22");
				vColor.SetPixByHex("Fuchsia","FF00FF");
				vColor.SetPixByHex("Gainsboro","DCDCDC");
				vColor.SetPixByHex("GhostWhite","F8F8FF");
				vColor.SetPixByHex("Gold","FFD700");
				vColor.SetPixByHex("GoldenRod","DAA520");
				vColor.SetPixByHex("Gray","808080");
				vColor.SetPixByHex("Grey","808080");
				vColor.SetPixByHex("Green","008000");
				vColor.SetPixByHex("GreenYellow","ADFF2F");
				vColor.SetPixByHex("HoneyDew","F0FFF0");
				vColor.SetPixByHex("HotPink","FF69B4");
				vColor.SetPixByHex("IndianRed ","CD5C5C");
				vColor.SetPixByHex("Indigo ","4B0082");
				vColor.SetPixByHex("Ivory","FFFFF0");
				vColor.SetPixByHex("Khaki","F0E68C");
				vColor.SetPixByHex("Lavender","E6E6FA");
				vColor.SetPixByHex("LavenderBlush","FFF0F5");
				vColor.SetPixByHex("LawnGreen","7CFC00");
				vColor.SetPixByHex("LemonChiffon","FFFACD");
				vColor.SetPixByHex("LightBlue","ADD8E6");
				vColor.SetPixByHex("LightCoral","F08080");
				vColor.SetPixByHex("LightCyan","E0FFFF");
				vColor.SetPixByHex("LightGoldenRodYellow","FAFAD2");
				vColor.SetPixByHex("LightGray","D3D3D3");
				vColor.SetPixByHex("LightGrey","D3D3D3");
				vColor.SetPixByHex("LightGreen","90EE90");
				vColor.SetPixByHex("LightPink","FFB6C1");
				vColor.SetPixByHex("LightSalmon","FFA07A");
				vColor.SetPixByHex("LightSeaGreen","20B2AA");
				vColor.SetPixByHex("LightSkyBlue","87CEFA");
				vColor.SetPixByHex("LightSlateGray","778899");
				vColor.SetPixByHex("LightSlateGrey","778899");
				vColor.SetPixByHex("LightSteelBlue","B0C4DE");
				vColor.SetPixByHex("LightYellow","FFFFE0");
				vColor.SetPixByHex("Lime","00FF00");
				vColor.SetPixByHex("LimeGreen","32CD32");
				vColor.SetPixByHex("Linen","FAF0E6");
				vColor.SetPixByHex("Magenta","FF00FF");
				vColor.SetPixByHex("Maroon","800000");
				vColor.SetPixByHex("MediumAquaMarine","66CDAA");
				vColor.SetPixByHex("MediumBlue","0000CD");
				vColor.SetPixByHex("MediumOrchid","BA55D3");
				vColor.SetPixByHex("MediumPurple","9370D8");
				vColor.SetPixByHex("MediumSeaGreen","3CB371");
				vColor.SetPixByHex("MediumSlateBlue","7B68EE");
				vColor.SetPixByHex("MediumSpringGreen","00FA9A");
				vColor.SetPixByHex("MediumTurquoise","48D1CC");
				vColor.SetPixByHex("MediumVioletRed","C71585");
				vColor.SetPixByHex("MidnightBlue","191970");
				vColor.SetPixByHex("MintCream","F5FFFA");
				vColor.SetPixByHex("MistyRose","FFE4E1");
				vColor.SetPixByHex("Moccasin","FFE4B5");
				vColor.SetPixByHex("NavajoWhite","FFDEAD");
				vColor.SetPixByHex("Navy","000080");
				vColor.SetPixByHex("OldLace","FDF5E6");
				vColor.SetPixByHex("Olive","808000");
				vColor.SetPixByHex("OliveDrab","6B8E23");
				vColor.SetPixByHex("Orange","FFA500");
				vColor.SetPixByHex("OrangeRed","FF4500");
				vColor.SetPixByHex("Orchid","DA70D6");
				vColor.SetPixByHex("PaleGoldenRod","EEE8AA");
				vColor.SetPixByHex("PaleGreen","98FB98");
				vColor.SetPixByHex("PaleTurquoise","AFEEEE");
				vColor.SetPixByHex("PaleVioletRed","D87093");
				vColor.SetPixByHex("PapayaWhip","FFEFD5");
				vColor.SetPixByHex("PeachPuff","FFDAB9");
				vColor.SetPixByHex("Peru","CD853F");
				vColor.SetPixByHex("Pink","FFC0CB");
				vColor.SetPixByHex("Plum","DDA0DD");
				vColor.SetPixByHex("PowderBlue","B0E0E6");
				vColor.SetPixByHex("Purple","800080");
				vColor.SetPixByHex("Red","FF0000");
				vColor.SetPixByHex("RosyBrown","BC8F8F");
				vColor.SetPixByHex("RoyalBlue","4169E1");
				vColor.SetPixByHex("SaddleBrown","8B4513");
				vColor.SetPixByHex("Salmon","FA8072");
				vColor.SetPixByHex("SandyBrown","F4A460");
				vColor.SetPixByHex("SeaGreen","2E8B57");
				vColor.SetPixByHex("SeaShell","FFF5EE");
				vColor.SetPixByHex("Sienna","A0522D");
				vColor.SetPixByHex("Silver","C0C0C0");
				vColor.SetPixByHex("SkyBlue","87CEEB");
				vColor.SetPixByHex("SlateBlue","6A5ACD");
				vColor.SetPixByHex("SlateGray","708090");
				vColor.SetPixByHex("SlateGrey","708090");
				vColor.SetPixByHex("Snow","FFFAFA");
				vColor.SetPixByHex("SpringGreen","00FF7F");
				vColor.SetPixByHex("SteelBlue","4682B4");
				vColor.SetPixByHex("Tan","D2B48C");
				vColor.SetPixByHex("Teal","008080");
				vColor.SetPixByHex("Thistle","D8BFD8");
				vColor.SetPixByHex("Tomato","FF6347");
				vColor.SetPixByHex("Turquoise","40E0D0");
				vColor.SetPixByHex("Violet","EE82EE");
				vColor.SetPixByHex("Wheat","F5DEB3");
				vColor.SetPixByHex("White","FFFFFF");
				vColor.SetPixByHex("WhiteSmoke","F5F5F5");
				vColor.SetPixByHex("Yellow","FFFF00");
				vColor.SetPixByHex("YellowGreen","9ACD32");
				*/
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"MarkupDoc static constructor","setting HTML colors etc.");
			}
			Console.WriteLine("done preparing MarkupDoc.");//debug only
		}
		#endregion static constructor


		public VarStack vRoots=null; //only a !DOCTYPE, and html (that would have head and body)
		#region utils
		//SOURCECODE LOCATIONS (any others are relative to these):
		//--------------------------------------------------------
		//iOpening;
		//iOpeningLen; //opening tag 
		//iPostOpeningLen; //text after opening tag and before subtags area (and THEIR post closing text)
		//iContentLen; //INCLUDES iPostOpening text and all subtags and any inner text after that
		//iCloserLen; //closing tag (after subtags and all other inner text)
		//iPostClosingLen; //text after closing tag but before next tag or EOF
		//--------------------------------------------------------
		//TODO (x=done):
		//-Allow NULL attributes to be in quotes (i.e. <!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
		//-Mark as no closer if no closer found (?unless in list of ones requiring it??)
		//-Set bLeaf=true if renderable leaf object (i.e. text&image block)
		public bool Parse(string sAllData) {
			vRoots=new VarStack();
			
			///TODO: use parsing region methods
		} //end Parse

		public bool Save(string sFileX) {
			sFile=sFileX;
			return Save();
		}
		public bool Save() {
			bool bGood=RString.StringToFile(sFile, sData);
			return bGood;
		}
		public bool Load(string sFileX) {
			sFile=sFileX;
			return LoadFile();
		}
		public bool Load() {
			//TODO: bool bGood=SetAll(RString.StringFromFile(sFile), true);
			//TODO: if (bGood) bGood=Parse(sData);
			//TODO: return bGood;
			return SetAll(RString.StringFromFile(sFile), true);
		}
		#endregion utils
		
		#region html utilities
		public static bool AddKnownColor(string sName, string sHex) {
			string sFirstValue=sHex;
			bool bGood=false;
			try {
				if (RReporting.IsNotBlank(sName)&&RReporting.IsNotBlank(sHex)) {
					bGood=RConvert.HexColorStringToBGR24(ref by2dKnownColorBGR[iKnownColors], 0, sHex);
					if (bGood) iKnownColors++;
				}
				else RReporting.ShowErr("Can't use empty color/name string.","AddKnownColor","adding blank color without notation {name:"+RReporting.StringMessage(sName,true)+"; hex:"+RReporting.StringMessage(sFirstValue,true)+"}");
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"MarkupDoc AddKnownColor","adding color {name:"+RReporting.StringMessage(sName,true)+"; hex:"+RReporting.StringMessage(sFirstValue,true)+"; iKnownColors:"+iKnownColors.ToString()+"}");
			}
			return bGood;
		}
		public static void ResetKnownColors(int iMaxBufferSize) {
			iKnownColors=0;
			iSetByteDepth=3;
			if (iMaxBufferSize<1) {
				RReporting.ShowErr("Tried to set known colors maximum too low so reverting to 1","ResetKnownColors","setting maximum named colors {iMaxBufferSize:"+iMaxBufferSize.ToString()+"}");
				iMaxBufferSize=1;
			}
			sarrKnownColor=new string[iMaxBufferSize];
			by2dKnownColorBGR=new byte[iMaxBufferSize][];
			for (int iNow=0; iNow<by2dKnownColorBGR.Length; iNow++) {
				by2dKnownColorBGR[iNow]=new byte[iSetByteDepth];
				for (int i2=0; i2<iSetByteDepth; i2++) by2dKnownColorBGR[iNow][i2]=0;
			}
		}
		public static int KnownColors {
			get {
				if (sarrKnownColor!=null&&by2dKnownColorBGR!=null) {
					if (sarrKnownColor.Length==by2dKnownColorBGR.Length) return sarrKnownColor.Length;
					else RReporting.ShowErr("Known color array sizes do not match!");
				}
				return 0;
			}
		}
		//public static int KnownColorIndex(string sColorName) {	
		//	int iReturn=-1;
			//for (int iNow=0; iNow<KnownColors; iNow++) {
			//	if (sarrKnownColor[iNow]==sColorName) {
			//		iReturn=iNow;
			//		break;
			//	}
			//}
		//	if (iReturn==-1) iReturn=KnownColorIndexI(sColorName);
		//	return iReturn;
		//}
		public static int KnownColorIndex(string sColorNameCaseInsensitive) {
			int iReturn=-1;
			string sColorNameToLower=sColorNameCaseInsensitive.ToLower();
			for (int iNow=0; iNow<KnownColors; iNow++) {
				if (sarrKnownColor[iNow].ToLower()==sColorNameToLower) {
					iReturn=iNow;
					break;
				}
			}
			return iReturn;
		}
		public static void KnownColor(ref Color colorReturn, string sColorName) {
			try {
				int iNow=KnownColorIndex(sColorName);
				if (iNow>=0) {
					//if (colorReturn==null) colorReturn=new Color(by2dKnownColorBGR[iNow][2],by2dKnownColorBGR[iNow][1],by2dKnownColorBGR[iNow][0]);
					//else {
						colorReturn=Color.FromArgb(by2dKnownColorBGR[iNow][2],by2dKnownColorBGR[iNow][1],by2dKnownColorBGR[iNow][0]);
						//colorReturn.R=by2dKnownColorBGR[iNow][2];
						//colorReturn.G=by2dKnownColorBGR[iNow][1];
						//colorReturn.B=by2dKnownColorBGR[iNow][0];
					//}
				}
				else {
					//if (colorReturn==null) colorReturn=new Color();
					//else {
						colorReturn=Color.FromArgb(0,0,0);
						//colorReturn.R=0;
						//colorReturn.G=0;
						//colorReturn.B=0;
					//}
				}
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"KnownColor(colorReturn,string)");
			}
		}
		public static void KnownColor(out int iReturnR,out int iReturnG,out int iReturnB, string sColorName) {
			iReturnR=0;
			iReturnG=0;
			iReturnB=0;
			try {
				int iNow=KnownColorIndex(sColorName);
				if (iNow>=0) {
					iReturnR=by2dKnownColorBGR[iNow][2];
					iReturnG=by2dKnownColorBGR[iNow][1];
					iReturnB=by2dKnownColorBGR[iNow][0];
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"KnownColor(int,int,int,string)");
			}
		}
		public static void KnownColor(out byte byReturnR,out byte byReturnG,out byte byReturnB, string sColorName) {
			byReturnR=0;
			byReturnG=0;
			byReturnB=0;
			try {
				int iNow=KnownColorIndex(sColorName);
				if (iNow>=0) {
					byReturnR=by2dKnownColorBGR[iNow][2];
					byReturnG=by2dKnownColorBGR[iNow][1];
					byReturnB=by2dKnownColorBGR[iNow][0];
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"KnownColor(byReturnR,byReturnG,byReturnB,string)");
			}
		}
		#endregion html utilities
		
		#region parsing
		public static string[] MarkupSplitByRef(string sXML) {//, string sObject_DotNotation, string sMemberNameInObjectForWhichToReturnValue) {
			//iParsed=0;
			StringStack sstackParsed=new StringStack();
			string[] sarrLevel=sObject_DotNotation.Split(".");
			string[] sarrTemp=new string[sarrLevel.Length+1];
			for (int iNow=0; iNow<sarrLevel.Length; iNow++) {
				sarrTemp[iNow]=sarrLevel[iNow];
			}
			//sarrTemp[sarrLevel.Length]=sMemberNameInObjectForWhichToReturnValue;//Length used as index intentionally
			sarrLevel=sarrTemp;
			iLevel=-1;//MUST start at -1 to line up with sarrLevel
			//sarrOpener=Array();
			//sarrCloser=Array();
			int iAbsoluteLevel=0;
			int iContentStart=0;
			int iContentEnder=0;
			//for (int iNow=0; iNow<iLevels; iNow++) {
				//sarrOpener[iNow]="<".sarrLevel[iNow].">";
				//sarrCloser="</".sarrLevel[iNow].">";
			//}
			string sTag="";
			string sLastTag="";
			string sContent="";
			///TODO: finish this -- finish checking this -- for a FULL parser, a content stack would have to be created, where higher tags' content includes the subtags themselves and text chunks before/after/between them.
			//-also, "<>", "<<" (second one literal too if not followed by tag), etc, and stray closer would have to be treated as a literals
			for (int iChar=0; iChar<sXML.Length; iChar++) {
				bool bCloser=false;
				bool bOpener=false;
				if (CompareAt(sXML,"<?",iChar)) {
					MoveTo(sXML,iChar,">"); //NOT MovePast, since will be incremented
					iContentStart=iChar+1;
					sTag="";
					sLastTag="";
				}
				else if (CompareAt(sXML,"<!",iChar)) {
					MoveTo(sXML,iChar,">"); //NOT MovePast, since will be incremented
					iContentStart=iChar+1;
					sTag="";
					sLastTag="";
				}
				else if (CompareAt(sXML,"</",iChar)) { //ender
					iContentEnder=iChar;
					MoveTo(sXML,iChar,">"); //NOT MovePast, since will be incremented
					sLastTag=SafeSubstringByExclusiveEnder(sXML,iContentEnder+2,iChar);
					sContent=SafeSubstringByExclusiveEnder(sXML,iContentStart,iContentEnder);
					//iContentStart=-1;//don't do this--may be needed later
					iAbsoluteLevel--;
					bCloser=true;
					if (iAbsoluteLevel<0) RReporting.ShowErr("more closers than openers parsing markup ending near character[iChar] and a tag like \"</"+sLastTag+">\".<br>");
					//do NOT clear sTag, it needs to be known in order to determine that the content is there
				}
				else if (CompareAt(sXML,"<",iChar)) { //opener
					iTagStart=iChar+1;
					iTagEnder=iChar+1;
					MoveTo(sXML,iChar,">"); //NOT MovePast, since will be incremented
					MoveTo(sXML,iTagEnder," ");
					if (iChar<iTagEnder) iTagEnder=iChar; //set to location of ">" (replaces next line since always positive)
					//iTagEnder=LesserPositiveElseNeg1(iChar,iTagEnder);
					sTag=SafeSubstringByExclusiveEnder(sXML,iTagStart,iTagEnder);//replaces next line since always positive
					//sTag=(iEnder>=0)?SafeSubstringByExclusiveEnder(sXML,iTagStart,iTagEnder):"";
					iContentStart=iChar+1;
					if (SafeSubstring(sXML,iContentStart-2,2)!="/>") {
						bOpener=true;
						iAbsoluteLevel++; //if not self-closing tag
					}
					sLastTag="";
				}
				if ( bOpener && ((iLevel+1)<iLevels) && (sTag=sarrLevel[iLevel+1]) ) {//&& CompareAt(sXML,sarrLevel[iLevel],iChar)) {
					iLevel++;
				}
				else if (bCloser && (iLevel>=0) && (sLastTag==sarrLevel[iLevel])) {
					if ( (iLevel==(iLevels-1)) ) {//&& (sTag==sMemberNameInObjectForWhichToReturnValue) ) { //&& (iContentStart>=0) && (iContentEnder>iContentStart) ) {
						sstackParsed.Push(sContent); //SafeSubstringByExclusiveEnder(sXML,iContentStart,iContentEnder);
						//iParsed++;
					}
					iLevel--;
				}
			}//end for iChar
			return sstackParsed.ToStringArray();
		}//end MarkupSplitByRef
		#endregion parsing
		
		#region text manip
		private void FixCode() {
			int iCount=1;
			while (iCount>0) {
				iCount=0;
				iCount+=ReplaceAll("< ","<");
				iCount+=ReplaceAll("<\t","<");
				iCount+=ReplaceAll("<"+Environment.NewLine,"<");
				iCount+=ReplaceAll("<\r","<");
				iCount+=ReplaceAll("<\n","<");
				iCount+=ReplaceAll("<>","");
				iCount+=ReplaceAll("</>","");
				for (int iTextless=0; iTextless<this.iSelfClosingTagwords; iTextless++) {
					//Delete all instances of closings on textless tagwords, even deleting "</p>"
					iCount+=ReplaceAll("</"+sarrSelfClosingTagword[iTextless]+">","");
				}
			}
		}
		private void UpdateLowercaseBuffer() {
			try {
				sDataLower=sData.ToLower();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"MarkupDoc UpdateLowercaseBuffer");
			}
		}
		public string GetAll() {
			return sData;
		}
		public bool SetAll(string sDataSrc) {
			return SetAll(sDataSrc,true);
		}
		public bool SetAll(string sDataSrc, bool bDoSGMLFixAfterLoad) {
			sFuncNow="SetAll";
			bool bGood=true;
			try {
				sData=sDataSrc;
				if (bDoSGMLFixAfterLoad) {
				  FixCode();
				}
				UpdateLowercaseBuffer();
				DetectComments(); //TODO: finish this (create the function, saving comment character indeces to a stack, which is updated by text modification functions along with sDataLower)
				//DO NOT parse now (!!!!)
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"MarkupDoc SetAll(string,bool)");
				bGood=false;
			}
			return bGood;
		}
		public bool MoveToOrStayAtSpacingOrEndingBracket(ref int iMoveMe) {
			return RString.MoveToOrStayAtSpacingOrString(ref iMoveMe, sData, ">");
		}
		public bool MoveToOrStayAtNodeTag(ref int iMoveMe) {
			bool bGood=true;
			//TODO: MUST take into account whether inside a comment area!
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (sData.Substring(iMoveMe,1)=="<") {
						if (IsNodeTagAt(iMoveMe)) {
							bGood=true;
							break;
						}
					}
				}
				sFuncNow=System.Reflection.MethodInfo.GetCurrentMethod().Name;
				if (iMoveMe>=iEOF) RReporting.Warning("Reached end of page in MoveToOrStayAtNodeTag.");
			}
			catch (Exception exn) {
				bGood=false;
			}
			return bGood;
		}
		public bool MoveToOrStayAtClosingTagAndGetTagword(out string sTagword, ref int iMoveMe) {
			sTagword="";
			bool bGood=true;
			//TODO: MUST take into account whether inside a comment area!
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (sData.Substring(iMoveMe,1)=="</") {
						int iEnder=iMoveMe+2;
						if (RString.MoveToOrStayAt(ref iEnder, sData, ">")) {
							sTagword=RString.SafeSubstring(sDataLower, iMoveMe+2, iEnder-(iMoveMe+2));
							bGood=true;
						}
						else bGood=false;
						break;
					}
				}
				sFuncNow=System.Reflection.MethodInfo.GetCurrentMethod().Name;
				if (iMoveMe>=iEOF) RReporting.Warning("Reached end of page in MoveToOrStayAtClosingTagAndGetTagword.");
			}
			catch (Exception exn) {
				bGood=false;
			}
			return bGood;
		}
		public bool IsNodeTagAt(int iAt) {
			bool bNode=false;
			bool bOther=true;
			try {
				for (int iTagword=0; iTagword<sarrSelfClosingTagword.Length; iTagword++) {
					if (!bOther) bOther=IsThatTagwordAtThisOpeningBracket(ref sarrSelfClosingTagword[iTagword], iAt, false);
					else break;
				}
				for (int iTagword=0; iTagword<this.sarrTextTagPrefixes.Length; iTagword++) {
					if (!bOther) bOther=IsThatTagwordAtThisOpeningBracket(ref sarrSelfClosingTagword[iTagword], iAt, true);
					else break;
				}
				if (!bOther) bNode=true;
			}
			catch (Exception exn) {
				//TODO: report this
				bNode=false;
			}
			return bNode;
		}
		public bool IsThatTagwordAtThisOpeningBracket(ref string sMustBeTagwordOnlyAndLowercase, int iLocOfBracketBeforeTagwordInSourceToCompare, bool bAllowPartialTags) {
			bool bFound=false;
			string sTest1;
			string sTest2;
			try {
				if (!bAllowPartialTags) {
					sTest1="<"+sMustBeTagwordOnlyAndLowercase+" ";
					sTest2="<"+sMustBeTagwordOnlyAndLowercase+">";
					
					bFound=RString.CompareAt(sTest1, sDataLower, iLocOfBracketBeforeTagwordInSourceToCompare);
					if (!bFound) bFound=RString.CompareAt(sTest2, sDataLower, iLocOfBracketBeforeTagwordInSourceToCompare);
				}
				else {//allow partial tags (text tags i.e. "!*")
					sTest1="<"+sMustBeTagwordOnlyAndLowercase;
					bFound=RString.CompareAt(sTest1, sDataLower, iLocOfBracketBeforeTagwordInSourceToCompare);
				}
			}
			catch (Exception exn) {
				bFound=false;
			}
			return bFound;
		}
		
		public bool MoveBackToOrStayAt(ref int iMoveMe, char cFind) {
			string sFind="";
			bool bGood=true;
			//if (cFind!="") {
				sFind=char.ToString(cFind);
				bGood=MoveBackToOrStayAt(ref iMoveMe, sFind);
			//}
			//else {
			//	bGood=false;
			//	RReporting.ShowErr("Null char!","MoveBackToOrStayAt","searching backward for character");
			//}
			return bGood;
		}
		public bool MoveBackToOrStayAt(ref int iMoveMe, string sFind) {
			return RString.MoveBackToOrStayAt(ref iMoveMe, sData, sFind);
		}
		/// <summary>
		/// Case-insensitive
		/// </summary>
		/// <param name="iMoveMe"></param>
		/// <param name="cFind"></param>
		/// <returns></returns>
		public bool MoveToOrStayAtI(ref int iMoveMe, char cFind) {
			bool bGood=false;
			string sFind="";
			try {
				//if (cFind!=null) {
					sFind=char.ToString(cFind).ToLower();
					bGood=RString.MoveToOrStayAt(ref iMoveMe, sDataLower, sFind);
				//}
				//else {
					//bGood=false;
					//RReporting.ShowErr("GNoder MoveToOrStayAtI null char!");
				//}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GNoder MoveToOrStayAt(cFind)","searching for text");
				bGood=false;
			}
			return bGood;
		}
		/// <summary>
		/// MoveToOrStayAt a character; insensitive version optimized by using GNoder's lowercase buffer.
		/// </summary>
		/// <param name="iMoveMe">location of sFind, or sFind.Length if sFind doesn't exist in text buffer</param>
		/// <param name="sFind">text to find</param>
		/// <returns>false if doesn't exist</returns>
		public bool MoveToOrStayAtI(ref int iMoveMe, string sFind) {
			return RString.MoveToOrStayAt(ref iMoveMe, sDataLower, sFind.ToLower()); //OK SINCE converts both to lowercase
		}
		public bool MoveToOrStayAtSpacing(ref int iMoveMe) {
			return RString.MoveToOrStayAtSpacing(ref iMoveMe, sData);
		}
		public bool IsNewLineChar(int iAtChar) {
			return RString.IsNewLineChar(RString.SafeSubstring(sData,iAtChar,1));
		}
		public bool IsSpacingChar(int iAtChar) {
			return RString.IsSpacingCharExceptNewLine(RString.SafeSubstring(sData, iAtChar,1));
		}
		public bool IsSpacing(int iChar) {
			return (IsSpacingChar(iChar)||IsNewLineChar(iChar));
		}
		//public bool IsTextTag(string sTagWordX) {
		//	bool bReturn=false;
		//	for (int iNow=0; iNow<this.sarrTextTagPrefixes
		//	return bReturn;
		//}
		//public bool IsNoClosing(string sTagWordX) {
		//
		//}
		public static bool RemoveEndsWhiteSpace(ref string sDataX) {
			bool bGood=RString.RemoveEndsWhiteSpace(ref sDataX);
			//if(RString.RemoveEndsNewLines(ref sDataX)==false) bGood=false;
			return bGood;
		}
		/*public static bool RemoveSpacing(ref string sDataX) {
			bool bGood=RemoveSpacing(ref sDataX);
			if (RemoveNewLines(ref sDataX)==false)bGood=false;
			return bGood;
		}
		public static bool RemoveSpacing(ref string sDataX) {
			int iPlace;
			try {
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf(' ');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\t');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
			}
			catch (Exception exn) {
				return false;
			}
			return true;
		}
		public static bool RemoveNewLines(ref string sDataX) {
			int iPlace;
			try {
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf(Environment.NewLine);
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\r');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\n');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
			}
			catch (Exception exn) {
				return false;
			}
			return true;
		}
		*/
		public int ReplaceAll(string sFrom, string sTo) {
			int iResult=RString.ReplaceAll(ref sData, sFrom, sTo);
			this.UpdateLowercaseBuffer();
			return iResult;
		}
		public void InsertText(string sToInsertAtCursor) {
			if (sToInsertAtCursor==null) {
				sToInsertAtCursor="";
				RReporting.Warning("Tried to insert null string into MarkupDoc, so set inserted value to empty string.");
			}
			sData=RString.SafeSubstring(sData,0,iSelCodePos)+sToInsertAtCursor+RString.SafeSubstring(sData,iSelCodePos+iSelCodeLen);
			this.UpdateLowercaseBuffer();
			//TODO: must shift all variables of all necessary SGMLLoc integers
		}
		#endregion text manip
	}//end class MarkupDoc
	
	public class MarkupChunk { //formerly MarkupNode, formerly SGMLNode
		#region static
		public const int TypeUninitialized=0;
		public const int TypeText=1;
		public const int TypeTag=2;
		#endregion static
		
		#region variables
		//public Var vStyle=null;//make style vars derive manually from source when accessed
		//public Var vProps=null;//make properties derive manually from source when accessed
		
		public int iType;
		
		public int iOpener;//location of "<"
		
		public int iOpenerLen;//"<*>"
		public int iContentLen;//"...<...>...<...>...." (post-opening text AND subtags & trailing text)
		public int iCloserLen;//"</*>"
		#endregion variables
		
		#region calculated-only vars
		public int iContent{ get{return iOpener+iOpenerLen;} } //INCLUDES iPostOpening text and all subtags and any of their post closing text
		public int iClosing{ get{return iContent+iContentLen;} } //closing tag (after subtags & all other inner text)
		public int iCloser{ get{return iOpener+iOpenerLen+iContentLen+iCloserLen-1;} } //location of ">"
		#endregion calculated-only vars
		
		#region constructors
		public MarkupChunk() {
			InitNull();
		}
		public void InitNull() {
			iType=TypeUninitialized;
			iOpener=-1;
			iOpenerLen=0;
			iContentLen=0;
			iCloserLen=0;
		}
		#endregion constructors
		
		public void SetAsTag(int iOpenerTag_StartMark, int iOpenerTag_EndMark, int iCloserTag_StartMark, int iCloserTag_EndMark) {
			iOpener=iOpenerTag_StartMark;
			iOpenerLen=(iOpenerTag_EndMark+1)-iOpenerTag_StartMark;
			iContentLen=(iCloserTag_StartMark)-(iOpenerTag_EndMark+1);
			iCloserLen=(iCloserTag_EndMark+1)-iCloserTag_StartMark;
			iType=TypeTag;
		}
		public void SetAsText(int iFirstChar, int iExclusiveEnder) {
			iOpener=iFirstChar;
			iOpenerLen=0;
			iContentLen=iExclusiveEnder-iFirstChar;
			iCloserLen=0;
			iType=TypeText;
		}
	}///end MarkupChunk
	
}//end namespace
