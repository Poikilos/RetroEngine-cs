/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PM
 * 
 */

using System;

namespace ExpertMultimedia {
	
	/// <summary>
	/// Graphical node (i.e. to convert from/to a markup tag)
	/// </summary>
	public class RForm {
		
		#region constants
		public const int TypeUndefined=0;
		public const int TypePlane=1; //background or pane or image, i.e. the root node or an image box
		public const int TypeMarkup=2; //indicates that the type depends on html
		public const int TypeButton=3;
		public const int TypeSliderH=4;
		public const int TypeSliderW=5;
		public const int TypeTextArea=6;
		public const int TypeTextEdit=7;
		public const int TypeSphereNode=8;
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
		  //owns and maintains this RForm
		//public string sPreText;//may include newlines etc.
		//public string sContent;//text AFTER child nodes.
		private Var vProps=null; //TODO: allow valueless properties like "Checked"!
		private Var vStyle=null; //cascaded style--i.e. root RForm derives it from ALL style&class props (in consecutive order)
		public string sToolTip="";
		public bool bSplit=false; //Whether the node was processed by DivideNode (sgml parser)
		public uint bitsAttrib=0;
		public Var vsFriend=null; //the Variable in sgmldoc.vsRoots (or descendant) that this RForm represents
		public int iCursor;
		public int iSelStart;
		public int iSelLen;
		public bool Visible {
			get {
				if (vStyle!=null) {
					if (vStyle.Exists("visibility")) {
						return vStyle.GetForcedString("visibility")=="visible";
					}
					else return true;
				}
				else return true;
			}
			set {
				CreateStyleIfNull();
				vStyle.SetOrCreate("visibility",value?"visible":"hidden");
			}
		}
		#endregion optional vars
		
		
		#region required vars
		//RForm rformParent=null;
		public int iIndex=-1;
		public int Index {//index in rformr
			set {
				if (iIndex<0) iIndex=value;
				else Base.ShowErr("Index of Node "+iIndex.ToString()+" (\""+Name+"\") was already set and can't be set again to "+value.ToString()+".");
			}
			get {
				return iIndex;
			}
		}
		public int iParentNode=-1;
		int iSubNodes=0;
		public IRect rectAbs=null; //absolute screen position derived from vProps width and vStyle width--set by parent;
		public int Width {
			get {
				//if (rectAbs!=null) {
					return rectAbs.Width;
				//}
				//else return 0;
			}
			set {
				//if (rectAbs!=null) {
					rectAbs.Width=value;
				//}
			}
		}
		public int Height {
			get {
				//if (rectAbs!=null) {
					return rectAbs.Height;
				//}
				//else return 0;
			}
			set {
				//if (rectAbs!=null) {
					rectAbs.Height=value;
				//}
			}
		}
		public IZone zoneInner=null; //absolute screen position derived from margin styles (used by child; or by inner text if IsLeaf)
		//public int indexParent; //index in RFormr
		public string sTagword; //tells how to render
		public bool bLeaf; //whether this can be drawn as text
		public int iType;
		private string sText; //TODO: for sgml: (only used if root !bUpdateHTML, otherwise root node gets it from an sgmlNow.Substring(...))
		public Liner linerText=null;//version of sText for TextArea
		public string Text {
			get {
				if (iType==TypeTextArea) {
					if (linerText!=null) return linerText.ToString("\n");//TODO: debug whether to use \n here
					else {
						Base.ShowErr("TextArea liner was null upon trying to get text!");
						return "";
					}
				}
				else return sText;
			}
			set {
				try {
					if (iType==TypeTextArea) {
						if (linerText==null) linerText=new Liner(1);
						linerText.SetText(value);
					}
					else sText=value;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"RForm set Text","setting text to "+(value!=null?(value!=""?"\""+value+"\"":"\"\""):"null")+" for interface node");
				}
			}
		}
		//RForm[] rformarr;//COPY of REFERENCE to objects
		//public int MAXBRANCHES {
		//	get {
		//		try {
		//			if (rformarr==null) return 0;
		//			else return rformarr.Length;
		//		}
		//		catch (Exception exn) {
		//			Base.ShowExn(exn,"node","getting MAXBRANCHES");
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
		//			Base.ShowExn(exn,"node","setting MAXBRANCHES");
		//		}
		//	}
		//}
		#endregion
		//public Variable vParent=null; //TODO: implement this (create Variable [created from HTML] "re-parser") has links to sourcecode etc
		public RForm() {
			Init(0,0,"","",16,16,32,32);
			Base.WriteLine("Warning: Default constructor was used for a RForm");
		}
		//public RForm(GBuffer gbSurface) {
		//	Init(gbSurface);
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
		//public void Init(GBuffer gbSurface) {
		//	int iSetWidth=0;
		//	int iSetHeight=0;
		//	try {
		//		iSetWidth=gbSurface.iWidth;
		//		iSetHeight=gbSurface.iHeight;
		//	}
		//	catch (Exception exn) {
		//		Base.ShowExn(exn,"RForm Init","setting node by surface");
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
		public void Init(int iSetParentNode, int RFormType, string sSetName, string sSetText, int xLoc, int yLoc, int iSetWidth, int iSetHeight, string HTMLTag) { //int IndexParent,
			iParentNode=iSetParentNode;//rformParent=rformSetParent;
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
			vProps=new Var("properties",Var.TypeArray);
			vProps.Root=vProps;
			vProps.SetOrCreate("name",sSetName);
			vStyle=new Var("style",Var.TypeArray);
			vStyle.Root=vProps;
			vProps.SetByRef("style",vStyle);
			linerText=new Liner(1);
			sText=sSetText; //not used unless using !rformrRoot.bUpdateHTML
			//indexParent=IndexParent;
			sTagword=HTMLTag; //tells how to render if RForm.TypeMarkup
			//rformarr=null;
			iSubNodes=0;
			iCursor=0;
			iSelStart=0;
			iSelLen=0;
			//MAXBRANCHES=2;//debug performance since low maximum can cause frequent automatic resize
		}
		public bool Backspace() {
			if (iType==TypeTextArea) return linerText.Backspace();
			else {
				Base.ShowErr("Backspace is only implemented for TextArea");
				return false;
			}
		}
		public bool Delete() {
			if (iType==TypeTextArea) return linerText.Delete();
			else {
				Base.ShowErr("Delete is only implemented for TextArea");
				return false;
			}
		}
		public bool Return() {
			if (iType==TypeTextArea) return linerText.Return();
			else {
				Base.ShowErr("Entering a line return is only implemented for TextArea");
				return false;
			}
		}
		//public bool InsertLine(int iLine, string sLine) {
		//	if (iType==TypeTextArea) return linerText.InsertLine(iLine,sLine);
		//	else {
		//		Base.ShowErr("InsertLine is only implemented for TextArea");
		//		return false;
		//	}
		//}
		//public bool RemoveLine(int iLine) {
		//	if (iType==TypeTextArea) return linerText.RemoveLine(iLine);
		//	else {
		//		Base.ShowErr("RemoveLine is only implemented for TextArea");
		//		return false;
		//	}
		//}
		//public bool AddLine(string sLine) {
		//	if (iType==TypeTextArea) return linerText.AddLine(sLine);
		//	else {
		//		Base.ShowErr("AddLine is only implemented for TextArea");
		//		return false;
		//	}
		//}
		public bool Insert(char cToInsertAtCursor) { //formerly EnterText
			//rformrRoot.sgmlNow.InsertText(Char.ToString(cToInsertAtCursor));
			if (iType==TypeTextArea) return linerText.Insert(char.ToString(cToInsertAtCursor));
			else {
				Base.ShowErr("EnterText(char) is only implemented for TextArea");
				return false;
			}
		}
		public bool Insert(string sToInsertAtCursor) { //formerly EnterText
			//rformrRoot.sgmlNow.InsertText(sToInsertAtCursor); //TODO: must shift all variables of all necessary nodes
			if (iType==TypeTextArea) return linerText.Insert(sToInsertAtCursor);
			else {
				Base.ShowErr("EnterText(string) is only implemented for TextArea");
				return false;
			}
		}
		public bool SetTextAreaSelection(int iRowStart, int iColStart, int iRowEnd, int iColEnd) {
			if (iType==TypeTextArea) return linerText.SetSelection(iRowStart,iColStart,iRowEnd,iColEnd);
			else {
				Base.ShowErr("SetTextAreaSelection(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool ShiftSelection(int iRows, int iCols, bool bWithShiftKey) {
			if (iType==TypeTextArea) return linerText.ShiftSelection(iRows,iCols,bWithShiftKey);
			else {
				Base.ShowErr("ShiftSelection(...,bool) is only implemented for TextArea");
				return false;
			}
		}
		public bool ShiftSelection(int iRows, int iCols) {
			if (iType==TypeTextArea) return linerText.ShiftSelection(iRows,iCols);
			else {
				Base.ShowErr("ShiftSelection(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool GrowSelection(int iRows, int iCols) {
			if (iType==TypeTextArea) return linerText.GrowSelection(iRows,iCols);
			else {
				Base.ShowErr("GrowSelection(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool Home(bool bWithShiftKey) {
			if (iType==TypeTextArea) return linerText.Home(bWithShiftKey);
			else {
				Base.ShowErr("Home(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool End(bool bWithShiftKey) {
			if (iType==TypeTextArea) return linerText.End(bWithShiftKey);
			else {
				Base.ShowErr("End(...) is only implemented for TextArea");
				return false;
			}
		}
		public bool SetTextAreaCursor(int iRow, int iCol) {
			if (iType==TypeTextArea) return linerText.SetCursor(iRow,iCol);
			else {
				Base.ShowErr("SetTextAreaCursor(...) is only implemented for TextArea");
				return false;
			}
			//try {
			//	linerText.SetZeroSelection(iRow,iCol);
			//}
			//catch (Exception exn) {
			//	Base.ShowExn(exn,"SetTextAreaCursor("+iRow.ToString+","+iCol.ToString()+")");
			//}
		}
		public bool Clear() {
			if (iType==TypeTextArea) return linerText.Clear();
			else {
				Base.ShowErr("Clear is only implemented for TextArea");
				return false;
			}
		}
		public bool SetLines(string[] SetLines) {
			if (iType==TypeTextArea) return linerText.SetLines(SetLines);
			else {
				Base.ShowErr("SetLines is only implemented for TextArea");
				return false;
			}
		}
		public int TextLength {
			get {
				int iReturn=0;
				try {
					if (iType==TypeTextArea) iReturn=linerText.Chars;
					else return sText.Length;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"RForm TextLength");
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
				return iParentNode<0;//rformParent==null;
			}
		}
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
			else {//if (rectAbs!=null) {
				return rectAbs.Contains(xAt,yAt);
			}
			//else return false;
		}
		public int XInner {
			get {
				if (zoneInner!=null) return zoneInner.Left;
				else return 0;
			}
		}
		public int YInner {
			get {
				if (zoneInner!=null) return zoneInner.Top;
				else return 0;
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
		public void CreatePropsIfNull() {
			if (vProps==null) {
				vProps=new Var();
				vProps.Root=vProps;
			}
		}
		public void CreateStyleIfNull() {
			CreatePropsIfNull();
			if (vStyle==null) {
				vStyle=vProps.IndexItem("style");
				if (vStyle==null) {
					vStyle=new Var("style",Var.TypeArray);
				}
				vProps.SetByRef("style",vStyle);//DOES set root
			}
		}
		public bool SetProperty(string sName, string sValue) {
			bool bGood=false;
				bGood=vProps.SetOrCreate(sName,sValue);
			return bGood;
		}
		public bool SetStyle(string sName, string sValue) {
			bool bGood=false;
			CreateStyleIfNull();
			bGood=vStyle.SetOrCreate(sName,sValue);
			return bGood;
		}
		public string GetProperty(string sName) {
			string sReturn="";
			if (vProps!=null) {
				Var vProperty=vProps.IndexItem("onmousedown");
				if (vProperty!=null) {
					sReturn=vProperty.ToString();
				}
				vProps.Get(out sReturn, sName);
			}
			return sReturn;
		}
		public string GetStyle(string sName) {
			string sReturn="";
			if (vStyle!=null) vStyle.Get(out sReturn, sName);
			return sReturn;
		}
		//debug NYI UpdateStyle should be in HTMLPage so page size can be determined/accessed first
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
	}//end class RForm
}//end namespace
