/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Windows.Forms;
//TODO: ADD STATIC CONSTRUCTOR TO EVERY CLASS IN RETROENGINE INDIVIDUALLY
//using ExpertMultimedia;
//TODO: implement form posting to retroengine i.exn. for login screen
//TODO: allow css comments ("//")

namespace ExpertMultimedia {
	/// <summary>
	/// SGMLPage edits and displays local text files.
	/// --i.exn. Used by Website to edit and upload downloaded html files.
	/// --This object has it's own always-32-bit-BGRA buffer for graphics.  The image is whatever size it needs to be for the SGML Data.
	/// </summary>
	public class GNoder {
		public static string sErr="(no error recorded)";
		private static string sFuncNow="(unknown function)";//deprecated, but probably still used in this class
		//TODO:(?) SEPARATE GNODER FROM SGMLPAGE (?)
		//		-so that gnoder can manage nodes and htmlpage text updates
		//		are easier to make optional
		//private static int ROOTSGMLNODE=0;

		
		#region vars
		private bool bRecode; //tells whether to update sourcecode when node is edited
		public int iDpi=96;//TODO: get from GetGlobal(ref sDPI, "ScreenDPI");
		public SGMLDoc sgmlNow=null;//TODO: finish this -- implment this
		//TODO: make this.GetGlobal call IPC channel named "RetroEngine.Globals"
		public GNode[] gnodearr=null;
		public int iNodes;
		public int iActiveNode;
		public int iMaxNodesDefault;
		private int iParseParent;
		public ITarget tgPane; //window or pane relative to the calling program
		public GBuffer gbPage;//whole page including area outside of window
		public int iSelfClosingTagwords;
		//public string[] sarrSelfClosingTagword; //tagwords with no text, i.e. img, br, or meta
		//public int iTextTags;
		//public string[] sarrTextTagPrefixes; //just text, like <!-- tags
		//public string[] sarrTextTagSuffixes; //end of just text, like /-->
		//private string sTemp;
		//private bool bDone=false;
		public int MAXNODES {
			 get {
				 return gnodearr.Length;
			 }
			 set {
				 try {
					 if (gnodearr==null) {
						 gnodearr=new GNode[value];
					 }
					 else {
						 GNode[] gnodearrNew=new GNode[value];
						 for (int i=0; i<gnodearr.Length; i++) {
							gnodearrNew[i]=gnodearr[i];
						 }
						 gnodearr=gnodearrNew;
					 }
				 }
				 catch (Exception exn) {
					 sFuncNow="set MAXNODES";
					 sErr="Exception error--"+exn.ToString();
				 }
			 }
		}
		#endregion vars
	
		#region constructors
		public GNoder() {
			Init(0,0,800,600);
		}
		public void Init(int xAt, int yAt, int iWidth, int iHeight) {
			try {
				iActiveNode=0;
				tgPane=new ITarget();
				tgPane.x=xAt;
				tgPane.y=yAt;
				tgPane.width=iWidth;
				tgPane.height=iHeight;
				iMaxNodesDefault=100000;
				//gbPage=new GBuffer();//whole page including area outside of window
				gnodearr=null;
				sFuncNow="Init()";
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
			}
		}
		#endregion constructors

		#region parsing
		//ROOT LOCATIONS:
		//---------------
		//iOpening; 
		//iOpeningLen; //opening tag 
		//iPostOpeningLen; //text after opening tag and before subtags area (and THEIR post closing text)
		//iInnerLen; //INCLUDES iPostOpening text and all subtags and any inner text after that
		//iClosingLen; //closing tag (after subtags and all other inner text)
		//iPostClosingLen; //text after closing tag but before next tag or EOF
		//---------------
		
		/*
		public bool ParseSGML(ref ITarget tgPaneNow) {
			bool bGood=true;
			iNodes=0;
			int iSplit;
			int iLastNodes=0;
			//create root node:
			tgPane.x=tgPaneNow.x;
			tgPane.y=tgPaneNow.y;
			tgPane.width=tgPaneNow.width;
			tgPane.height=tgPaneNow.height;
			gnodearr=null; //initialized by setting MAXNODES
			MAXNODES=iMaxNodesDefault; //initializes gnodearr if null
			do {//(//(iNodesLast<gnodearr.Length)
			       //&&(gnodearr[iNodes]!=null)
			       //&&
				//    (gnodearr[iNodes].bSplit==false)) {
				iLastNodes=iNodes;
				if (iNodes==0) {
					if (bGood) bGood=CreateRootNode(ref tgPaneNow);
				}
				if (bGood) {
					for (int iNode=iSplitNodes; iNode<iNodes; iNode++) {
						//note: this will always run at least once--because
						// iSplitNodes==iNodes, the previous iSplit would've
						// been 0 and broken the outer do-while.
						iSplit=DivideNode(iNode);
						iSplitNodes++;
					}
				}
				else iSplit=0;
			} while (iSplit>0);
			//TODO: Finish this:
			//TODO: CREATE ATTRIB VARS
			//TODO: CASCADE THE STYLE VARS
			//TODO: SET tgAbs ABSOLUTE POSITIONS
		} //end ParseSGML
		*/
		
		bool CreateRootNode(ref ITarget tgPaneNow) {
			bool bGood=true;
			try {
				iNodes=0;
				if (gnodearr[iNodes]==null) gnodearr[iNodes]=new GNode();
				gnodearr[iNodes].tgAbs=Base.CopyStruct(ref tgPaneNow);
				//gnodearr[iNodes].iOpening=0;
				//gnodearr[iNodes].iOpeningLen=0; //opening tag 
				//gnodearr[iNodes].iPostOpeningLen=0; //text after opening tag and before subtags area (and THEIR post closing text)
				//MoveToOrStayAtNodeTag(ref gnodearr[iNodes].iPostOpeningLen);
				//gnodearr[iNodes].iInnerLen=this.sData.Length; //INCLUDES iPostOpening text and all subtags and any inner text after that
				//gnodearr[iNodes].iClosingLen=0; //closing tag (after subtags and all other inner text)
				//gnodearr[iNodes].iPostClosingLen=0; //text after closing tag but before next tag or EOF
				iNodes=1;
			}
			catch (Exception exn) {
				//TODO: report this
				bGood=false;
			}
			return bGood;
		}

		/*
		/// <summary>
		/// Finds subnodes of iNode and adds them to gnodearr,
		/// then sets the node to bSplit=true.
		/// </summary>
		/// <returns>Number of subtags found, or -1 if error.  If zero,
		/// the method has reached the last iteration and does not need
		/// to be called again.</returns>
		public int DivideNode(int iNode) {
			//TODO: finish this (must refix--OR--OPTION 2: REMOVE and replace with load from a Var [Var.FromSGML] tree)
			int iReturnSubnodes=0;
			bool bGood=true;
			string[] sarrFindOpener;
			iParseParent=0;
			int iFind=0;
			int iSeek;
			//int iClosingStart=0;
			//int iOpeningEnder=0;
			//UpdateLowercaseBuffer();
			try {
				if (sarrTempTagStack==null) {
					sarrTempTagStack=new string[iTagDepthMax];
				}
				iTagDepth=0;
				//TODO: FIND SUBNODES
				//Find each subnode and corresponding iPreOpening, iSubTag, iPreClosing variables
				// -Kill unnecessary closing tags
				// -Account for nested tags (incl. same tagword)
				// -Close LAST child if ANY parent closes.
				int iPrevNode=iNodes-1;
				gnodearr[iNodes].iOpening=0; //TODO: ONLY if iNodes==0
				this.MoveToOrStayAtNodeTag(ref gnodearr[iNodes].iOpening);
				//TODO: handle iOpening==iEOF
				iFind=gnodearr[iNodes].iOpening+1;
				iSeek=iFind;
				if (false==MoveToOrStayAtWhitespaceOrEndingBracket(ref iSeek)) {
					sErr="{index:"+iSeek.ToString()+"; error:\"can't find space/bracket following tagword\"}";
					bGood=false;
					return -1;//fatal error
				}
				//TODO: check this--make sure sDataLower has been set already
				gnodearr[iNodes].sTagword=sDataLower.Substring(iFind, iSeek-iFind);
				gnodearr[iNodes].iOpening=gnodearr[iPrevNode].iInner+gnodearr[iPrevNode].iPostOpeningLen;

				//iSeek=iFind;
				if (!Base.MoveToOrStayAt(ref iSeek, sData, ">")) {
					sErr="{index:"+iSeek.ToString()+"; error:\"can't find '>' bracket for end of opening tag\"}";
					bGood=false;
					return -1;//return bGood;//fatal error
				}
				gnodearr[iNodes].iOpeningLen=(iSeek-gnodearr[iNodes].iOpening)+1;//opening tag
				string sClosingNow;
				iFind=iSeek;
				bool bNoClosing=false;
				bool bTextTag=false;
				int iClosingNow=iFind;
				if (IsTextTag(gnodearr[iNodes].sTagword)) bTextTag=true;
				else if (IsNoClosing(gnodearr[iNodes].sTagword)) bNoClosing=true;
				if (!this.MoveToOrStayAtNodeTag(ref iSeek)) {//find first subtag
					//TODO: Finish this!  If no subnodes, finish tag the easy way
					
					return 0;
				}
				else {
					
				}
				
				//TODO: account for multitier
				bool bEnd=false;
				iClosingNow=iFind;
				int iOpenerNow=iFind;
				int iSelfTags=0;//keeps track of nested tags of same tagword
				//TODO: set root locations:
				sClosingNow="";
				string sOpenerNow="";
				VarStack stack=new VarStack();
				//gnodearr[iNodes].iPostOpeningLen=; //text after opening tag and before subtags area (and THEIR post closing text)
				while (bGood && !bEnd) {
					//if (false==MoveToOrStayAt(ref iSeek, sClosingNow)) {
					if (bGood) bGood=MoveToOrStayAtNodeTagAndGetTagword(ref iOpenerNow, ref sOpenerNow);
					if (bGood) bGood=MoveToOrStayAtClosingTagAndGetTagword(ou		public string[] sarrSelfClosingTagword; //tagwords with no text, i.e. img, br, or meta
		public int iTextTags;
		public string[] sarrTextTagPrefixes; //just text, like <!-- tags
		public string[] sarrTextTagSuffixes; //end of just text, like /-->
t sClosingNow, ref iClosingNow);
					if (bGood) {
						if (iOpenerNow>=Length && iOpenerNow>=Length) {
							//TODO: finish this!
							return x;
						}
						if (iClosingNow<iOpenerNow) {
							
						}
						else { //iClosingNow>iOpenerNow
							stack.Push(Var.Create("x",sOpenerNow));
						}
						if (stack.CountInstances(sClosingNow)>0) {
							
						}
					}
					//TODO: finish this! (infinite loop unless completely finished)
					//iClosingStart=sData.Length-1;
					gnodearr[iNodes].iInnerLen=Data.Length-(gnodearr[iNodes].iOpeningLen+gnodearr[iNodes].iOpening);
					sErr="{index:"+iSeek.ToString()+"; error:\"can't find "+sClosingNow+"\"}";
					gnodearr[iNodes].iClosingLen=0;
					gnodearr[iNodes].iPostClosingLen=0;
					//}
				}
				gnodearr[iNodes].iClosing=iSeek;
				//TODO: increment iNodes for EACH first-degree-child-level node found
				//TODO: set root locations:
				//gnodearr[iNodes].iInnerLen=; //INCLUDES iPostOpening text and all subtags and any inner text after that
				//gnodearr[iNodes].iClosingLen=; //closing tag (after subtags and all other inner text)
				//gnodearr[iNodes].iPostClosingLen=; //text after closing tag but before next tag or EOF
			}
			catch (Exception exn) {
				sFuncNow="DivideNodes";
				sErr="Exception error--"+exn.ToString();
				iReturnSubnodes=-1;
			}
			return iReturnSubnodes;
		} //end DivideNode
		*/
		
		#endregion parsing
	
		#region drawing
	
		#endregion drawing
	
	
	}//end class GNoder
}//end namespace
