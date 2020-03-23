//File created by Jake Gustafson 2007-08-16
using System;

namespace ExpertMultimedia {
	public class RTextBox {
		public static bool bDebug {
			get { return RReporting.bDebug; }
		}
	
	
	
	///TODO: remake to self-render and inherit from RForm! (when setting parent, always also set rformParent)
	///-then make RGrid, staring with ViewModeForm with self-rendering RTextBoxes (which looks like old-school database form), and eventually add everything up to and including a tile-based map editor (tile-based maps with average height stored in the tile can speed up hit detection for voxel-based landscapes).

		#region vars
		private string[] sarrLine=null;//TextArea version of  sText
		public bool bEndWithNewLine;
		private bool bSuspendChanges=false;
		private RForm rformContainer=null;//use as fake inheritance instead of real inheritance, for the purpose of keeping the rforms.rformarr containing only RForm types and not messy derivatives.
		private int iSelRowStart=0;
		private int iSelColStart=0;
		private int iSelRowEnd=0;//inclusive //TODO: make sure it is inclusive
		private int iSelColEnd=0;//exclusive
		private int iDesiredCol=0;//saves previous column of cursor when moving up/down to a shorter row
		private int iForcedNewLineLength=1;
		private int iUsed=0;
		private int iMods=0;
		private int iModGroups=0;
		private int iWidestLineChars=0;
		private int iWidestLine=0;
		private int iTab=4;//TODO: finish this--implement this
		private const int iMinModGroup=1;//always 1; mod group 0 is loading the file
		private RTextBoxMod[] modarr=null;
		public static int DefaultMaxEditsForUndo=1024;
		public static int DefaultLineBufferSize=1024;
		public bool bChanged=false;
		public bool bMulti=true;//multiline
		//public bool Change
		//public void SetAsSaved() {	
		//	bChanged=false;
		//}
		public string Line(int iLine) {
			return (iLine>=0&&iLine<iUsed)?sarrLine[iLine]:"";
		}
		public int SelRowStart {
			get {
				return iSelRowStart;
			}
		}
		public int SelRowEnd {
			get {
				return iSelRowEnd;
			}
		}
		public int SelColStart {
			get {
				return iSelColStart;
			}
		}
		public int SelColEnd {
			get {
				return iSelColEnd;
			}
		}
		public int SelStart {
			get {
				return RowColToLinear(iSelRowStart,iSelColStart);
			}
		}
		public int SelLength {
			get {
				return RowColToLinear(iSelRowStart,iSelColStart,iSelRowEnd,iSelColEnd);
			}
		}
		public int LineCount {	
			get {
				return iUsed;
			}
		}
		public int RowLength(int iLine) {
			try {
				if (iLine<0) {
					RReporting.ShowErr("Tried to get length of text area line","","RowLength("+iLine.ToString()+")");
					return 0;
				}
				else if (iLine<iUsed) return sarrLine[iLine].Length;
				else return 0;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting length of text area (TextBox corruption)","RTextBox RowLength(row:"+iLine.ToString()+"){Maximum: "+Maximum.ToString()+"}");
			}
			return 0;
		}
		//TODO: public bool GetMonospacedSelPoly(ref IPoly polyReturn, int iCharW, int iCharH) {
		//	try {
		//		if (polyReturn==null) polyReturn=new IPoly();
		//		else polyReturn.Clear();
		//	}
		//	catch (Exception exn) {
		//		RReporting.ShowExn(exn,"","GetSelPoly");
		//	}
		//	return bGood;
		//}
		public int SelLines {
			get {
				return ( (iSelRowStart>iSelRowEnd) ? (iSelRowStart-iSelRowEnd) : (iSelRowEnd-iSelRowStart) )  +  1;
			}
		}
		public bool bWarnedOnZeroLines=false;
		public int Maximum {//formerly MaxLines
			get {
				if (sarrLine!=null) return sarrLine.Length;
				else return 0;
			}
			set {
				try {
					if (bMulti||Maximum==0) {
						int SetMaximum=value;
						if (!bMulti) SetMaximum=1;
						if (SetMaximum<=0) {
							RReporting.Warning("RTextBox Maximum set to 0");
							sarrLine=null;
							bWarnedOnZeroLines=true;
						}
						else if (SetMaximum!=Maximum) {
							//int iMin=(Maximum<SetMaximum)?Maximum:SetMaximum;
							//int iMax=(Maximum>SetMaximum)?Maximum:SetMaximum;
							//do NOT use sarrLine.Length since it is allowed to be null here!
							string[] LinesNew=new string[SetMaximum];
							if (SetMaximum<iUsed) RReporting.Warning("lines being truncated from "+iUsed.ToString()+" used lines to "+SetMaximum.ToString()+" Maximum.");
							for (int iLine=0; iLine<LinesNew.Length; iLine++) {
								LinesNew[iLine]=(iLine<iUsed)?sarrLine[iLine]:"";
							}
							if (bWarnedOnZeroLines) RReporting.Warning("RTextBox Maximum set to "+Maximum.ToString()+" (showing SetMaximum since had been manually set to zero last time)");
							bWarnedOnZeroLines=false;
							sarrLine=LinesNew;
						}
					}
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"resizing text area content array","RTextBox set Maximum");
				}
			}
		}
		bool bWarnedOnZeroEdits=false;
		public int MaxEdits {
			get {
				if (modarr!=null) return modarr.Length;
				else return 0;
			}
			set {
				try {
					if (value<=0) {
						RReporting.Warning("RTextBox MaxEdits set to 0");
						modarr=null;
						bWarnedOnZeroEdits=true;
					}
					else if (value!=MaxEdits&&value>=iMods) {//value!=MaxEdits) {
						//do NOT use modarr.Length since it is allowed to be null here!
						RTextBoxMod[] modarrNew=new RTextBoxMod[value];
						if (value<iMods) RReporting.Warning("undo buffer being truncated from "+iMods.ToString()+" actions history to "+value.ToString()+" MaxEdits.");
						for (int iNow=0; iNow<modarrNew.Length; iNow++) {
							modarrNew[iNow]=(iNow<iMods)?modarr[iNow]:new RTextBoxMod();
						}
						if (bWarnedOnZeroEdits) RReporting.Warning("RTextBox undo buffer MaxEdits set to "+MaxEdits.ToString()+" (showing value since had been manually set to zero last time)");
						bWarnedOnZeroEdits=false;
					}
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"resizing undo buffer","RTextBox set MaxEdits");
				}
			}
		}
		public int Length {
			get {
				int iReturn=0;
				try {
					for (int iNow=0; iNow<iUsed; iNow++) {
						iReturn+=sarrLine[iNow].Length+iForcedNewLineLength;
					}
				}
				catch (Exception exn) {
					iReturn=-1;
					RReporting.ShowExn(exn,"","RTextBox get Chars");
				}
				return iReturn;
			}
		}
		#endregion vars
		#region constructors
		public RTextBox(RForm rformSetContainer) {
			Init(rformSetContainer, DefaultLineBufferSize, true);
		}
		public RTextBox(RForm rformSetContainer, int iSetMaxLines) {
			Init(rformSetContainer, DefaultLineBufferSize, true);
		}
		public RTextBox(RForm rformSetContainer, int iSetMaxLines, bool bAsMulti) {
			Init(rformSetContainer, DefaultLineBufferSize, bAsMulti);
		}
		public bool Init(RForm rformSetContainer, int iSetMaxLines, bool bAsMulti) {
			bool bGood=false;
			bSuspendChanges=true;
			try {
				bMulti=bAsMulti;
				if (!bMulti) iSetMaxLines=1;
				rformContainer=rformSetContainer;
				sarrLine=null;
				Maximum=iSetMaxLines;
				iUsed=0;
				bGood=true;
				modarr=new RTextBoxMod[1024];
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn( exn, "initializing RTextBox", String.Format("RTextBox Init(,,bMultiline:{0})", bMulti?"yes=TextArea":"no=EditBox") );
			}
			bSuspendChanges=false;
			return bGood;
		}
		private bool Init(string[] LinesNew) {
			return SetLines(LinesNew);
			//bool bGood=false;
			//bSuspendChanges=true;
			//if (LinesNew!=null) {
			//	try {
			//		SetFuzzyMaxLinesByLocation(LinesNew.Length);
			//		SetFuzzyMaxEditsByLocation(DefaultMaxEditsForUndo);
			//		for (int iNow=0; iNow<LinesNew.Length; iNow++) {
			//			AddLine(LinesNew[iNow], iNow<LinesNew.Length-1);
			//		}
			//	}
			//	catch (Exception exn) {
			//		bGood=false;
			//		RReporting.ShowExn(exn,"initializing text area","RTextBox Init(string[])");
			//	}
			//}
			//else {
			//	bGood=false;
			//	RReporting.ShowErr("Tried to initialize text area with empty line array","initializing null text area","RTextBox Init(string[])");
			//}
			//bSuspendChanges=false;
			//FindMaxLine();
			//return bGood;
		}//end Init(string[])
		#endregion constructors
		#region utilities
		private void OnChange() { //TODO: make sure this is always called
			if (!bSuspendChanges) {
				FindMaxLine();//debug performance
			}
			bChanged=true;
		}
		private void FindMaxLine() {
			try {
				iWidestLineChars=0;
				iWidestLine=0;
				for (int iNow=0; iNow<iUsed; iNow++) {
					if (sarrLine[iNow].Length>iWidestLineChars) {
						iWidestLineChars=sarrLine[iNow].Length;
						iWidestLine=iNow;
					}
				}
			}
			catch (Exception exn) {
				iWidestLineChars=0;
				iWidestLine=0;
				RReporting.ShowExn(exn,"","FindMaxLine");
			}
		}
		public void SetFuzzyMaxLinesByLocation(int iLoc) {
			if (iLoc+iLoc/2>Maximum) Maximum=iLoc+iLoc/2+1;
			//Maximum=RMath.LocationToFuzzyMaximum(Maximum,iLoc);
		}
		public bool SetFuzzyMaxEditsByLocation(int iLoc) {
			if (iLoc+iLoc/2>MaxEdits) MaxEdits=iLoc+iLoc/2+1;
			//MaxEdits=RMath.LocationToFuzzyMaximum(MaxEdits,iLoc);
			return iLoc<=MaxEdits;
		}
		//public int LongestLine() {
		//	int iReturn=0;
		//	for (int iNow=0; iNow<iUsed; iNow++) {
		//		if (sarrLine[iNow].Length>iReturn) iReturn=sarrLine[iNow].Length;
		//	}
		//	return iReturn;
		//}
		///<summary>
		///Gets difference (end before start is ok).
		///Numbering starts at zero.
		///</summary>
		public int RowColToLinear(int iAtRow, int iAtCol) {
			return RowColToLinear(0,0,iAtRow,iAtCol);
		}
		public bool InRange(ref int iRowStart, ref int iColStart, ref int iRowEnd, ref int iColEnd) {
			bool bWasInRange=true;
			try {
				if (iRowStart<0) {iRowStart=0;bWasInRange=false;}
				if (iRowStart>=iUsed) { iRowStart=iUsed==0?0:iUsed-1; bWasInRange=false;}
				if (iRowEnd<0) {iRowEnd=0;bWasInRange=false;}
				if (iRowEnd>=iUsed) {iRowEnd=iUsed==0?0:iUsed-1; bWasInRange=false;}
				
				if (iColStart<0) {iColStart=0;bWasInRange=false;}
				if (iColStart>Line(iRowStart).Length) {iColStart=Line(iRowStart).Length;bWasInRange=false;}
				if (iColEnd<0) {iColEnd=0;bWasInRange=false;}
				if (iColEnd>Line(iRowEnd).Length) {iColEnd=Line(iRowEnd).Length;bWasInRange=false;}

				
				/*
				if (iRowStart<0) {iRowStart=0;bWasInRange=false;}
				if (iRowStart>=iUsed) {iRowStart=iUsed>0?iUsed-1:0;bWasInRange=false;}
				if (iRowEnd<0) {iRowEnd=0;bWasInRange=false;}
				if (iRowEnd>=iUsed) {iRowEnd=iUsed>0?iUsed-1:0;bWasInRange=false;}
				if (iColStart<0) {iColStart=0;bWasInRange=false;}
				if (iColEnd<0) {iColEnd=0;bWasInRange=false;}
				if (iUsed>0) {
					if (iColStart>sarrLine[iRowStart].Length) {
						iColStart=sarrLine[iRowStart].Length;
						bWasInRange=false;
					}
					if (iColEnd>sarrLine[iRowEnd].Length) {
						iColEnd=sarrLine[iRowEnd].Length;
						bWasInRange=false;
					}
				}
				*/
			}
			catch (Exception exn) {
				bWasInRange=false;
				RReporting.ShowExn(exn,"cropping cursor variables to range","InRange");
			}
			if (!bWasInRange&&bDebug) RReporting.Warning("selection out of range {iRowStart:"+iRowStart.ToString()+"; iColStart:"+iColStart.ToString()+";iRowEnd:"+iRowEnd.ToString()+";iColEnd:"+iColEnd.ToString()+"; Length:"+Length.ToString()+"}");
			return bWasInRange;
		}//end InRange
		public bool IsInRange(int iRowStart, int iColStart, int iRowEnd, int iColEnd) {
			bool bReturn=false; //iSelColStart<iSelColEnd
			try {
				//TODO: fix this--this should NOT be linear
				bReturn=
						   ((iRowStart==0&&iUsed==0) || iRowStart<iUsed)
						&& iColStart<=Line(iRowStart).Length
						&& ((iRowEnd==0&&iUsed==0) || iRowEnd<iUsed)
						&& iColEnd<=Line(iRowEnd).Length;
				//iRowStart<=iUsed to allow typing at end of line
				//bReturn=iRowStart>=0&&iRowStart<=iUsed&&iColStart>=0&&iColEnd>=0&&iRowEnd>=0&&(iRowEnd<iUsed||(iUsed==0&&iRowEnd==0))&&iColEnd<=sarrLine[iRowEnd].Length;
			}
			catch {
				bReturn=false;
			}
			return bReturn;
		}
		public bool IsZeroRange(int iRowStart, int iColStart, int iRowEnd, int iColEnd) {
			return iRowStart==iRowEnd&&iColStart==iColEnd;
		}
		public static void OrderLocations(ref int iRowStart, ref int iColStart, ref int iRowEnd, ref int iColEnd) {
			//do NOT use the more specific RMath.OrderPoints(ref iColStart, ref iRowStart, ref iColEnd, ref iRowEnd);
			if (iRowStart==iRowEnd) {
				if (iColStart>iColEnd) RMemory.Swap(ref iColStart, ref iColEnd);
			}
			else if (iColStart==iColEnd) {
				if (iRowStart>iRowEnd) RMemory.Swap(ref iRowStart, ref iRowEnd);
			}
			else if (iRowStart>iRowEnd) {
				RMemory.Swap(ref iRowStart, ref iRowEnd);
				RMemory.Swap(ref iColStart, ref iColEnd);
			}
		}
		///<summary>
		///Gets difference (end before start is ok).
		///Numbering starts at zero.
		///</summary>
		public int RowColToLinear(int iRowStart, int iColStart, int iRowEnd, int iColEnd) {
			int iCountChars=0;
			int iCharStartNow=0;
			int iCharEndNow=0;
			int iRowNow=0;
			bool bGood=false;
			if (IsInRange(iRowStart,iColStart,iRowEnd,iColEnd)) {
				OrderLocations(ref iRowStart, ref iColStart, ref iRowEnd, ref iColEnd);
				try {
					for (iRowNow=0; iRowNow<=iRowEnd; iRowNow++) {//i.e. runs 0 times if row 0
						iCharStartNow=(iRowNow==iRowStart)?iColStart:0;
						iCharEndNow=(iRowNow==iRowEnd)?iColEnd:sarrLine[iRowNow].Length;
						iCountChars+=iCharEndNow-iCharStartNow;
						if (iRowNow<iRowEnd) iCountChars+=iForcedNewLineLength;
						bGood=true;
					}
				}
				catch (Exception exn) {
					string sMsg="{";
					sMsg+="iRowStart:"+iRowStart.ToString()+"; ";
					sMsg+="iColStart:"+iColStart.ToString()+"; ";
					sMsg+="iRowEnd:"+iRowEnd.ToString()+"; ";
					sMsg+="iColEnd:"+iColEnd.ToString()+"; ";
					sMsg+="iCharStartNow:"+iCharStartNow.ToString()+"; ";
					sMsg+="iCharEndNow:"+iCharEndNow.ToString()+"; ";
					sMsg+="iCountChars:"+iCountChars.ToString()+"; ";
					sMsg+="Maximum:"+Maximum.ToString()+"; ";
					sMsg+="}";
					RReporting.ShowExn(exn,"getting location","LocOfRowCol "+sMsg);
				}
			}
			else RReporting.ShowErr("Invalid row/column locations","","RowColToLinear{row:"+iRowStart+" col:"+iColStart+" rowend: "+iRowEnd+" colend:"+iColEnd+"}");
			return bGood?iCountChars:-1;
		}//end RowColToLinear
		public override string ToString() {
			string sBuild="";
			for (int iNow=0; iNow<iUsed; iNow++) {
				sBuild+=(sBuild=="")?Line(iNow):(Environment.NewLine+Line(iNow));//debug forcing Environment.NewLine
			}
			//Console.Error.WriteLine("rtextbox{count:"+iUsed.ToString()+"; max:"+sarrLine.Length+"} ToString:\""+sBuild+"\"");
			return sBuild;
		}
		public string DumpStyle() {
			string sMsg="{";
			sMsg+="iSelRowStart:"+iSelRowStart.ToString()+"; ";
			sMsg+="iSelColStart:"+iSelColStart.ToString()+"; ";
			sMsg+="iSelRowEnd:"+iSelRowEnd.ToString()+"; ";
			sMsg+="iSelColEnd:"+iSelColEnd.ToString()+"; ";
			sMsg+="Maximum:"+Maximum.ToString()+"; ";
			sMsg+="Maximum:"+Maximum.ToString()+"; ";
			sMsg+="LastModification: "+(iMods-1).ToString();
			sMsg+="}";
			return sMsg;
		}
		public string DumpStyle(int iRowStart, int iColStart, int iRowEnd, int iColEnd) {
			string sMsg="{";
			sMsg+="iRowStart:"+iRowStart.ToString()+"; ";
			sMsg+="iColStart:"+iColStart.ToString()+"; ";
			sMsg+="iRowEnd:"+iRowEnd.ToString()+"; ";
			sMsg+="iRowEnd:"+iRowEnd.ToString()+"; ";
			sMsg+="Maximum:"+Maximum.ToString()+"; ";
			sMsg+="}";
			return sMsg;
		}
		public string ToString(string sInsertThisAtNewLine) {
			string sReturn="";
			if (sInsertThisAtNewLine==null) sInsertThisAtNewLine="";
			for (int iNow=0; iNow<iUsed; iNow++) {
				sReturn+=sarrLine[iNow]+sInsertThisAtNewLine;
			}
			if (sInsertThisAtNewLine!=""&&!bEndWithNewLine&&sReturn.EndsWith(sInsertThisAtNewLine)) {
				sReturn=RString.SafeSubstring(sReturn,0,sReturn.Length-1);
			}
			return sReturn;
		}
		#endregion utilities
		#region undo
		public bool Undo() {
			bool bGood=true;
			try {
				if (iModGroups>0) {
					RReporting.ShowErr("Undo is not yet implemented.");//TODO: finish this
				}
				else {
					bGood=false;
					RReporting.ShowErr("Can't undo.");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RTextBox Undo");
			}
			return bGood;
		}
		private bool AddMod(RTextBoxMod modNow) {//, int iLine, string sOldVal, string sNewVal, int iGroupOfGroup) {
			//RReporting.Debug("("+iModGroups+")AddMod["+iMods+"]=\""+modNow.ToString()+"\"");
			bool bGood=false;
			try {
				if (iMods>MaxEdits) RReporting.ShowErr("Exceeded Undo Buffer (might be out of memory)","adding textbox modification","AddMod(...){iMods:"+iMods.ToString()+"; MaxEdits:"+MaxEdits.ToString()+"}");
				else if (iMods==MaxEdits) {
					SetFuzzyMaxEditsByLocation(iMods);
					if(iMods>=MaxEdits) bGood=false;
				}
				else bGood=true;
				if (bGood) {
					//RReporting.Write("Adding...");
					if (iMods<MaxEdits) {
						//RReporting.Write("Copying...");
						if (modarr[iMods]==null) modarr[iMods]=modNow;
						else modNow.CopyTo(modarr[iMods]);
						iMods++;
						bGood=true;
						//RReporting.Write("Done.  ");
					}
					else {
						RReporting.ShowErr("Could not resize undo buffer (might be out of memory)");
					}
				}//else SetFuzzyMaxEditsByLocation already has already shown error
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"saving undo group","AddMod");
				bGood=false;
			}
			return bGood;
			RApplication.InvalidatePanelIfExists();//RApplication.InvalidateDest();
		}//end AddMod
		private RTextBoxMod LastMod {
			get {
				try {
					if (iMods>0) return modarr[iMods-1];
					else return null;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"accessing undo buffer for saving last modification group","RTextBox get LastMod");
				}
				return null;
			}
		}
		#endregion undo
		#region selection
		public bool SetSelection(int iSetRowStart, int iSetColStart, int iSetRowEnd, int iSetColEnd) {
			iSelRowStart=iSetRowStart;
			iSelColStart=iSetColStart;
			iSelRowEnd=iSetRowEnd;
			iSelColEnd=iSetColEnd;
			bool bGood=InRange(ref iSelRowStart, ref iSelColStart, ref iSelRowEnd, ref iSelColEnd);
			//RReporting.Debug("RTextBox SetSelection (row"+iSelRowStart+", col"+iSelColStart+")-(row"+iSelRowEnd+",col"+iSelColEnd.ToString()+")");// from modified mouse  ("+((int)(xMouse-ActiveNode.XInner)).ToString()+","+((int)(yMouse-ActiveNode.YInner)).ToString()+"), original ("+xMouse.ToString()+","+yMouse.ToString()+")");			//debug only
			return bGood;
		}
		public bool SetSelectionStart(int iSetRowStart, int iSetColStart) {
			return SetSelection(iSetRowStart,iSetColStart,iSelRowEnd,iSelColEnd);
		}
		public bool SetSelectionEnd(int iSetRowEnd, int iSetColEnd) {
			if ((iSelColEnd-iSetColEnd)!=0||iDesiredCol==0) iDesiredCol=iSetColEnd;
			if ((iSelColEnd-iSetColEnd)==0) {
				//if (iDesiredCol>iSelColEnd) {
					int iDisiredColCropped=iDesiredCol;//(iDesiredCol<RowLength(iSelRowEnd))?iDesiredCol:RowLength(iSelRowEnd);
					if (iSetColEnd==iSelColStart) {
						iSetColEnd=iDisiredColCropped;
						iSelColStart=iDisiredColCropped;
					}
					else iSetColEnd=iDisiredColCropped;
				//}
			}
			
			return SetSelection(iSelRowStart,iSelColStart,iSetRowEnd,iSetColEnd);
		}
		public bool Home(bool bWithShiftKey) {
			bool bGood=true;
			try {
				if (bWithShiftKey) bGood=SetSelectionEnd(iSelRowEnd,0);
				else bGood=SetSelection(iSelRowEnd,0,iSelRowEnd,0);
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"setting selection",String.Format("Home({0}) ",bWithShiftKey)+DumpStyle());
			}
			return bGood;
		}
		public bool End(bool bWithShiftKey) {
			bool bGood=true;
			try {
				if (bWithShiftKey) bGood=SetSelectionEnd(iSelRowEnd,Line(iSelRowEnd).Length);
				else bGood=SetSelection(iSelRowEnd,Line(iSelRowEnd).Length,iSelRowEnd,Line(iSelRowEnd).Length);
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"setting selection", String.Format("End({0})",bWithShiftKey)+DumpStyle());
			}
			return bGood;
		}
		public bool GrowSelection(int iRows, int iCols) {
			int iSetColEnd=iSelColEnd+iCols;
			int iSetRowEnd=iSelRowEnd+iRows;
			if (iSetColEnd<0) {
				iSetRowEnd--;
				if (iSetRowEnd>=0) iSetColEnd=RowLength(iSetRowEnd);
				else iSetRowEnd=0;//this is right
			}
			else if (iSetRowEnd<0) {
				iSetRowEnd=0;
			}
			else if (iCols!=0&&iSetColEnd==RowLength(iSetRowEnd)) {
				iSetRowEnd++;
				iSetColEnd=0;
			}
			return SetSelectionEnd(iSetRowEnd,iSetColEnd);
		}
		public bool SetCursor(int iSetRow, int iSetCol) {
			return SetSelection(iSetRow,iSetCol,iSetRow,iSetCol);
		}
		public void OrderLocationsFromSelection(out int iRowStart, out int iColStart, out int iRowEnd, out int iColEnd) {
			iRowStart=iSelRowStart;
			iColStart=iSelColStart;
			iRowEnd=iSelRowEnd;
			iColEnd=iSelColEnd;
			OrderLocations(ref iRowStart, ref iColStart, ref iRowEnd, ref iColEnd);
		}
		public bool SelectionIsInRange() {
			return IsInRange(iSelRowStart,iSelColStart,iSelRowEnd,iSelColEnd);//iSelRowStart>=0&&iSelRowStart<iUsed&&iSelColStart>=0&&iSelColEnd>=0&&iSelRowEnd>=0&&iSelRowEnd<iUsed&&iSelColEnd<=sarrLine[iSelRowStart].Length;
		}
		public bool IsZeroSelection() {
			return IsZeroRange(iSelRowStart,iSelColStart,iSelRowEnd,iSelColEnd);
		}
		public bool SetZeroSelection() {
			iSelRowStart=iSelRowEnd;
			iSelColStart=iSelColEnd;
			return true;
		}
		public bool SetZeroSelection(int iSetRow, int iSetCol) {
			iSelRowStart=iSetRow;
			iSelRowEnd=iSelRowStart;
			iSelColStart=iSetCol;
			iSelColEnd=iSelColStart;
			if (!SelectionIsInRange()) {
				InRange(ref iSelRowStart, ref iSelColStart, ref iSelRowEnd, ref iSelColEnd);
			}
			return true;
		}
		public bool ShiftSelection(int iRows, int iCols, bool bWithShiftKey) {
			bool bGood=false;
			if (bWithShiftKey) bGood=GrowSelection(iRows,iCols);
			else bGood=ShiftSelection(iRows,iCols);//DOES check InRange so actually would be ok to set bGood=true;
			return bGood;
		}
		public bool ShiftSelection(int iRows, int iCols) {
			bool bGood=false;
			//NOTE: all this junk is needed so cursor will wrap to previous/next line correctly
			//int iSelRowEndOriginal=iSelRowEnd+iCols;
			//if (iSelColEnd+iCols<RowLength(iSelRowEnd+iRows)) iDesiredCol=iSelColEnd+iCols;
			if (iCols!=0||iDesiredCol==0) iDesiredCol=iSelColEnd+iCols;
			if (!IsZeroSelection()) SetZeroSelection();
			//if (IsZeroSelection()) {
				try {
					if (iCols!=0) {
						iSelColStart+=iCols;
						iSelColEnd+=iCols;
						if (iSelColStart<0) {
							if (iSelRowStart>=1) {
								iSelRowStart--;
								iSelRowEnd=iSelRowStart;
								iSelColStart=sarrLine[iSelRowStart].Length;//intentionally at Length
								iSelColEnd=iSelColStart;
							}
							else {
								iSelColStart=0;
								iSelRowStart=0;
								iSelRowEnd=iSelRowStart;
								iSelColEnd=iSelColStart;
							}
						}
						else if (iSelColStart>sarrLine[iSelRowStart].Length) {
							if (iSelRowStart<(iUsed-1)) {
								iSelRowStart++;
								iSelRowEnd=iSelRowStart;
								iSelColStart=0;
								iSelColEnd=iSelColStart;
							}
						}
					}
					else {
						iSelRowStart+=iRows;
						iSelRowEnd+=iRows;
					}
					if (iSelRowStart<0) {
						iSelRowStart=0;
						iSelRowEnd=iSelRowStart;
						iSelColEnd=iSelColStart;
					}
					else if (iSelRowStart>=iUsed) {
						iSelRowStart=iUsed-1;
						iSelRowEnd=iSelRowStart;
						iSelColEnd=iSelColStart;
					}
					
					//int iSelColEndOriginal=iSelColEnd;
					if (iCols==0) {
						//if (iDesiredCol>iSelColEnd) {
							int iDisiredColCropped=iDesiredCol;//(iDesiredCol<RowLength(iSelRowEnd))?iDesiredCol:RowLength(iSelRowEnd);
							if (iSelColEnd==iSelColStart) {
								iSelColEnd=iDisiredColCropped;
								iSelColStart=iDisiredColCropped;
							}
							else iSelColEnd=iDisiredColCropped;
						//}
					}
					//iDesiredCol=iSelColEndOriginal;
					//if (iSelColEndOriginal>RowLength(iSelRowEndOriginal)) iDesiredCol=iSelColEndOriginal;
					//else if (iDesiredCol<RowLength(iSelRowEndOriginal)) iSelColEnd=0;
					bGood=SetSelectionInRange();
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"moving cursor","ShiftSelection");
				}
			//}
			//else {//shifting nonzero selection
			//	SetSelection(iSelRowEnd+iRows,iSelColEnd+iCols,iSelRowEnd+iRows,iSelColEnd+iCols);//DOES call InRange
			//}
			return bGood;
		}//end ShiftSelection
		public bool SetSelectionInRange() {
			iDesiredCol=iSelColStart;
			return InRange(ref iSelRowStart, ref iSelColStart, ref iSelRowEnd, ref iSelColEnd);
		}
		#endregion selection
		#region concrete editing (record undo)
		public bool InsertLine(int iLine, int iCol_ForReferenceOnly, string sLine) {
			return InsertLine(iLine, iCol_ForReferenceOnly, sLine, false);
		}
		public bool InsertLine(int iLine, int iCol_ForReferenceOnly, string sLine, bool bWaitForAnotherPartOfGroup) {
			bool bGood=false;
			bool bSuspendChangesPrev=bSuspendChanges;
			bSuspendChanges=true;
			string sParticiple="inserting line (before starting insertion)";
			int iFreed=0;
			try {
				sParticiple="inserting line (adding undo step)";
				AddMod(new RTextBoxMod(RTextBoxMod.TypeRemove, iLine, iCol_ForReferenceOnly, sarrLine[iLine], sLine, iModGroups));
				sParticiple="inserting line (setting line range)";
				if (Maximum<iUsed+1) SetFuzzyMaxLinesByLocation(iUsed);
				sParticiple="inserting line (pushing lines down)";
				if (iUsed>0) {
					for (iFreed=iUsed+1; iFreed>iLine; iFreed--) {
						sarrLine[iFreed]=sarrLine[iFreed-1];
					}
				}
				else iFreed=iLine;
				sParticiple="setting line "+iLine.ToString()+" in "+Maximum.ToString()+"-length array";
				if (iFreed==iLine) {
					bGood=true;
					sarrLine[iLine]=sLine;
					iUsed++;
				}
				else RReporting.ShowErr("Line location error",sParticiple,"RTextBox InsertLine"+" {"
					+"iLine:"+iLine.ToString()+"; "
					+"iFreed:"+iFreed.ToString()+"; "
					+"iUsed:"+iUsed.ToString()+"; "
					+"Maximum:"+Maximum.ToString()+"; "
					+"}");
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,sParticiple, "RTextBox InsertLine {"
					+"iLine:"+iLine.ToString()+"; "
					+"iFreed:"+iFreed.ToString()+"; "
					+"iUsed:"+iUsed.ToString()+"; "
					+"Maximum:"+Maximum.ToString()+"; "
					+"}");
			}
			if (!bWaitForAnotherPartOfGroup) iModGroups++;
			bSuspendChanges=bSuspendChangesPrev;
			if (!bSuspendChanges) OnChange();
			return bGood;
		}//end InsertLine
		private bool RemoveLine(int iLine, int iCol_ForReferenceOnly) {
			return RemoveLine(iLine,iCol_ForReferenceOnly,false);
		}
		private bool RemoveLine(int iLine, int iCol_ForReferenceOnly, bool bWaitForAnotherPartOfGroup) {
			bool bGood=false;
			bool bSuspendChangesPrev=bSuspendChanges;
			bSuspendChanges=true;
			AddMod(new RTextBoxMod(RTextBoxMod.TypeRemove, iLine, iCol_ForReferenceOnly, sarrLine[iLine], "", iModGroups));
			try {
				int iNow=0;
				for (iNow=iLine; iNow<iUsed-1; iNow++) {
					sarrLine[iNow]=sarrLine[iNow+1];
				}
				if (iNow==iUsed-1) {
					bGood=true;
					iUsed--;
					if (iUsed<0) {	
						RReporting.ShowErr("No lines to remove","",String.Format("RemoveLine(){{lines:{0}}}",iUsed));
						iUsed=0;
					}
				}
				else bGood=false;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"","RemoveLine("+iLine.ToString()+") {iUsed:"+iUsed.ToString()+")");
			}
			if (!bWaitForAnotherPartOfGroup) iModGroups++;
			bSuspendChanges=bSuspendChangesPrev;
			if (!bSuspendChanges) OnChange();
			return bGood;
		}
		private bool AddLine(string sLine) {
			return AddLine(sLine,false);
		}
		private bool AddLine(string sLine, bool bWaitForAnotherPartOfGroup) {
			return InsertLine(iUsed,0,sLine,bWaitForAnotherPartOfGroup);
		}
		private bool ChangeLine(int iLine, int iCol_ForReferenceOnly, string sLine) {
			return ChangeLine(iLine,iCol_ForReferenceOnly,sLine,false);
		}
		private bool ChangeLine(int iLine, int iCol_ForReferenceOnly, string sLine, bool bWaitForAnotherPartOfGroup) {
			bool bTest=AddMod(new RTextBoxMod(RTextBoxMod.TypeModify, iLine, iCol_ForReferenceOnly, sarrLine[iLine], sLine, iModGroups));
			sarrLine[iLine]=sLine;
			//Console.Error.WriteLine(sarrLine[iLine]);//debug only 
			if (iLine>=iUsed) iUsed=iLine+1;//TODO: save previous length for undo
			
			if (!bWaitForAnotherPartOfGroup) {
				iModGroups++;
				if (!bSuspendChanges) OnChange();
			}
			return true;
		}
		#endregion concrete editing (record undo)
		#region abstract editing (user-side)
		public bool SetLines(string[] LinesNew) {
			int iTest=0;
			Clear(false);//does call OnChange
			bool bSuspendChangesPrev=bSuspendChanges;
			bSuspendChanges=true;
			SetFuzzyMaxLinesByLocation(LinesNew.Length-1);
			for (int iNow=0; iNow<LinesNew.Length; iNow++) {
				if (AddLine(LinesNew[iNow], iNow<LinesNew.Length-1)) iTest++;
			}
			bSuspendChanges=bSuspendChangesPrev;
			if (!bSuspendChanges) OnChange();//FindMaxLines();
			return iTest==LinesNew.Length;
		}
		public bool SetText(string sVal) { //aka SetData
			return SetLines(RString.StringToLines(sVal));
		}
		public bool SelectAll() {
			return SetSelection(0,0,iUsed==0?0:iUsed-1,RowLength(iUsed==0?0:iUsed-1));
		}
		public bool Insert(string sTyped) {
			if ( RString.Contains(sTyped,Environment.NewLine)
				||RString.Contains(sTyped,'\r')
				||RString.Contains(sTyped,'\n')
				) {
				//TODO: call a newline parsing function to handle this then recurse back to here
				RReporting.ShowErr("Inserting newlines is not possible.  Use Return() instead (input corruption).");
				return false;
			}
			
			bool bGood=false;
			Console.Error.Write(sTyped);//debug only
			try {
				if (!IsZeroSelection()) {
					bGood=Delete();
					SetZeroSelection(iSelRowStart,iSelColStart);
				}
				Console.Error.Write("[.]");//debug only
				if (SelectionIsInRange()) {
					if (!ChangeLine(iSelRowStart,iSelColStart,RString.SafeInsert(sarrLine[iSelRowStart],iSelColStart, sTyped))) {
						bGood=false;
						Console.Error.Write("[-]");//debug only
					}
					else {
						SetZeroSelection(iSelRowStart, iSelColStart+sTyped.Length);
						Console.Error.Write("[+]");//debug only
						bGood=true;
					}
				}
				else {
					Console.Error.Write("[--]");//debug only
					RReporting.ShowErr("Cannot type here since selection is not in range","typing outside of range ","RTextBox Insert()"+DumpStyle());
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"typing","RTextBox Insert(sTyped:"+RReporting.StringMessage(sTyped,true)+"{Selection: (row "+iSelRowStart+", col "+iSelColStart+") to (row "+iSelRowEnd+", col "+iSelColEnd+")}");
			}
			return bGood;
		}//end Insert
		public bool Return() { //enter key
			bool bGood=false;
			bool bSuspendChangesPrev=bSuspendChanges;
			string sSplit="";
			try {
				if (!IsZeroSelection()) {
					bGood=Delete();
					SetZeroSelection(iSelRowStart,iSelColStart);//these should be already fixed by Delete
				}
				if (SelectionIsInRange()) {
					sSplit=sarrLine[iSelRowStart];
					if ( !ChangeLine(iSelRowStart,iSelColStart,RString.SafeSubstring(sSplit,0,iSelColStart))
						||!InsertLine(iSelRowStart+1,iSelColStart,RString.SafeSubstring(sSplit,iSelColStart)) ) {
						bGood=false;
					}
					SetZeroSelection(iSelRowStart+1, 0);
				}
				else RReporting.ShowErr("Cannot enter a line return here since selection is not in range","typing","RTextBox Return "+DumpStyle());
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"typing","RTextBox Return {sTyped:Return; Selection ("+iSelColStart+","+iSelRowStart+") to ("+iSelColEnd+","+iSelRowEnd+")}");
			}
			bSuspendChanges=bSuspendChangesPrev;
			if (!bSuspendChanges) OnChange();
			return bGood;
		}//end Return
		public bool Backspace() {
			bool bGood=false;
			bool bSuspendChangesPrev=bSuspendChanges;
			bSuspendChanges=true;
			string sParticiple="preparing location";
			int iRowStart=iSelRowStart, iRowEnd=iSelRowEnd, iColStart=iSelColStart, iColEnd=iSelColEnd;
			OrderLocations(ref iRowStart, ref iColStart, ref iRowEnd, ref iColEnd);
			try {
				if (IsZeroSelection()) {
					if (SelectionIsInRange()) {
						if (iColStart==0) {//if at beginning of line
							sParticiple="checking row";
							if (iRowStart>0) {
								sParticiple="moving to previous line";
								if(ChangeLine(iRowStart-1,Line(iRowStart-1).Length,Line(iRowStart-1)+Line(iRowStart),true)) {
									sParticiple="removing leftover space";
									if (RemoveLine(iRowStart,0)) {
										sParticiple="resetting selection";
										if (LastMod!=null) SetZeroSelection(iRowStart-1,LastMod.iCol);
										else {
											SetZeroSelection(iRowStart-1,0);
											RReporting.ShowErr("Last modification was not recorded properly! ","typing","Backspace()"+DumpStyle());
										}
									}
								}
							}
							//else do nothing since at top left
						}
						else {
							sParticiple="changing line";
							bGood=ChangeLine(iRowStart,iColStart,RString.SafeRemoveIncludingEnder(Line(iRowStart),iColStart-1,iColEnd-1));
						}
						sParticiple="shifting selection";
						ShiftSelection(0,-1);
					}
					else RReporting.ShowErr("Backspace failed since selection out of range.","typing","Backspace() "+DumpStyle());
					sParticiple="finishing";
				}
				else {//else not IsZeroSelection
					sParticiple="trying Delete method";
					bGood=Delete();
					//if (IsZeroSelection()) ShiftSelection(0,-1);
					//else {
					//	SetZeroSelection(iRowStart,iColStart);
					//}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"deleting before cursor ("+sParticiple+")","RTextBox Backspace() "+DumpStyle());
			}
			bSuspendChanges=bSuspendChangesPrev;
			sParticiple="onchange";
			if (!bSuspendChanges) OnChange();
			sParticiple="finishing";
			return bGood;
		}//end Backspace
		public bool Delete() {
			bool bGood=true;
			bool bSuspendChangesPrev=bSuspendChanges;
			bSuspendChanges=true;
			try {
				if (SelectionIsInRange()) {
					int iRowStart, iColStart, iRowEnd, iColEnd;
					OrderLocationsFromSelection(out iRowStart, out iColStart, out iRowEnd, out iColEnd);
					if (iRowStart==iRowEnd) {//single line
							if (iRowStart<iUsed) {//do NOT check iRowEnd<iUsed (allow delete up to end)
								if (iColStart==iColEnd) { //delete one character
									if (iColStart==Line(iRowStart).Length) {//delete one character at end of line
										if (iRowStart+1<iUsed) {
											if (ChangeLine(iRowStart,iColStart,Line(iRowStart)+Line(iRowStart+1),true)) {
												if (!RemoveLine(iRowStart+1,iColStart)) {
													bGood=false;
													//RReporting.ShowErr("Couldn't remove remnant of next line","deleting line break (shifting following lines)","RTextBox Delete "+DumpStyle());
												}
											}
											else {
												bGood=false;
												iModGroups++;//manually concludes modgroup (undo step)
												RReporting.ShowErr("Couldn't append next line to current line","deleting line break","RTextBox Delete "+DumpStyle());
											}
										}
										//else do nothing
									}//end delete one character at end of line
									else {//delete one character not at end of line
										if (ChangeLine( iRowStart, iColStart, RString.SafeRemove(Line(iRowStart),iColStart,1)) ) {
											SetZeroSelection(iRowStart,iColStart);
										}
										else {
											bGood=false;
											RReporting.ShowErr("Couldn't append next line to current line","deleting letter","RTextBox Delete "+DumpStyle());
										}
									}
								}//end if delete one character 
								else {//delete more than one character
									if (ChangeLine(iRowStart,iColStart,RString.SafeRemoveExcludingEnder(Line(iRowStart),iColStart,iColEnd))) {
										SetZeroSelection(iRowStart,iColStart);
									}
									else bGood=false;
								}
							}
							else if (iRowStart==iUsed) {
								//do nothing, but don't show error if at virtual line after last line
							}
							else RReporting.ShowErr("No text to delete (end of text area)","trying to delete text in unused area","RTextBox Delete "+DumpStyle());
					}
					else {//multirow
						//int iColStartNow;
						int iColEndNow;
						for (int iRow=iRowStart; iRow<=iRowEnd; iRow++) {
							//iColStartNow=0;
							iColEndNow=Line(iRow).Length;
							if (iRow==iRowEnd) iColEndNow=iColEnd;
							//NOTE: below does NOT work if selection is one line (or does it?) (TODO:?) so keep that "if" clause above
							if (iRow==iRowStart) {//first row of deletion
								//iColStartNow=iColStart;
								if (!ChangeLine(iRow,
										iColStart,//iColStartNow,
										RString.SafeSubstring(Line(iRow),0,iColStart)+RString.SafeSubstring(Line(iRowEnd),iColEnd),true)) {
									bGood=false;
									RReporting.ShowErr("can't collapse data to first line","deleting text (removing trailing lines)","RTextBox Delete() "+DumpStyle());
									break;
								}
							}
							else { //remove all lines INCLUDING the last line which was preserved as necessary above
								if(!RemoveLine(iRow,0,true)) {
									bGood=false;
									RReporting.ShowErr("RemoveLine failure","deleting text (removing trailing lines)","RTextBox Delete() "+DumpStyle());
									break;
								}
							}
						}//end for iRow
						iModGroups++;//manually close undo step
						SetZeroSelection(iRowStart,iColStart);
						//RReporting.ShowErr("Deleting multirow areas is not yet implemented.");
					}//end else multirow
				}//end if in range
				else RReporting.ShowErr("Deleting failed since selection out of range.","RTextBox Delete() "+DumpStyle());
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"deleting selected text","RTextBox Delete() "+DumpStyle());
			}
			bSuspendChanges=bSuspendChangesPrev;
			if (!bSuspendChanges) OnChange();
			return bGood;
		}//end Delete
		public bool Clear() {
			return Clear(true);
		}
		private bool Clear(bool bCallOnChange) {
			bool bGood=true;
			bSuspendChanges=true;
			for (int iNow=iUsed-1; iNow>=0; iNow--) {
				if (!RemoveLine(iNow,0,iNow<0)) bGood=false;//<0 is right since starts at end!
			}
			bSuspendChanges=false;
			if (bCallOnChange) OnChange();
			return bGood;
		}
		#endregion abstract editing
		
		#region drawing
		public void Render(RImage gbDest, bool bAsActive) {
			try {
				if (RReporting.bDebug) RReporting.sParticiple="starting to render RTextBox";
				int iCharH=rformContainer.rfont.Height;//int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font
				int yStartLine=rformContainer.zoneInner.Top;
				rformContainer.RenderText(gbDest,ToString());//only breaks on newline
				//for (int iLine=0; iLine<this.LineCount; iLine++) {
				//	rformContainer.rfont.Render(gbDest,rformContainer.zoneInner.Left, yStartLine,this.Line(iLine));//gDest.DrawString(this.Line(iLine), font, rpaint, rformContainer.zoneInner.Left, yStartLine);
				//	yStartLine+=iCharH;
				//}
				int iAsGlyphType=RFont.GlyphTypeNormal;
				int iColStartNow,iColEndNow;
				
				if (bAsActive) {
					int iSelRowStartOrdered=SelRowStart, iSelRowEndOrdered=SelRowEnd, iSelColStartOrdered=SelColStart, iSelColEndOrdered=SelColEnd;
					OrderLocations(ref iSelRowStartOrdered, ref iSelColStartOrdered, ref iSelRowEndOrdered, ref iSelColEndOrdered);
					//int yRel=0; xRel=0;
					for (int iRow=iSelRowStartOrdered; iRow<=iSelRowEndOrdered; iRow++) {
						iColStartNow=0;
						iColEndNow=RowLength(iRow);
						if (iRow==iSelRowStartOrdered) {
							iColStartNow=iSelColStartOrdered;
						}
						if (iRow==iSelRowEndOrdered) {
							iColEndNow=iSelColEndOrdered;
						}
						string sLine=Line(iRow);
						
						//IRect.Set(rectNow,iColStartNow*iCharW, iRow*iCharH, (iColEndNow-iColStartNow)*iCharW, iCharH);
						//rectNow.Inflate( 1, 1 );
						//regionSelection.Union(rectNow);
						
						RForms.DrawSelectionRect(
							gbDest, rformContainer,
							rformContainer.rfont.WidthOf(RString.SafeSubstring(sLine,0,iColStartNow),iAsGlyphType),
							(int)(iRow*iCharH),
							rformContainer.rfont.WidthOf(RString.SafeSubstringByExclusiveEnder(sLine,iColStartNow,iColEndNow)),
							iCharH
							);//DrawSelectionRect(gbDest, iColStartNow*iCharW, iRow*iCharH, (iColEndNow-iColStartNow)*iCharW, iCharH);//DrawSelectionRect(bmpOffscreen, iColStartNow*iCharW, iRow*iCharH, (iColEndNow-iColStartNow)*iCharW, iCharH);//this is right since allows selection to be zero characters wide
					}
					if (RForms.TextCursorVisible) {
						//gOffscreen.FillRectangle(rpaintTextNow, new Rectangle(SelColEnd*iCharW, SelRowEnd*iCharH, RForms.iTextCursorWidth, iCharH));
						RImage.Invert(gbDest, rformContainer.rfont.WidthOf(RString.SafeSubstring(Line(SelRowEnd),0,SelColEnd)), SelRowEnd*iCharH, RForms.iTextCursorWidth, iCharH);//Invert(bmpOffscreen, SelColEnd*iCharW, SelRowEnd*iCharH, RForms.iTextCursorWidth, iCharH);
					}
				}//end if bActive
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"rendering text lines","RForms RenderRTextBox");
			}
		}//end Render
		#endregion drawing
	}//end class RTextBox
	
	
                               ///  RTextBoxMod ///
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public class RTextBoxMod {
		//there should never be more than these types, to keep undo and tracking simple!
		public const int TypeNULL=0;//should never be the case unless unused
		public const int TypeRemove=1;
		public const int TypeInsert=2;
		public const int TypeModify=3;
		public static readonly string[] sarrType=new string[] {"NULL","Remove","Insert","Modify"};//debug must match Type* constants
		public int iType=TypeNULL;
		public int iRow=-1;//which row the Edit affects
		public string sLineOld="";//only present if TypeRemove
		public string sLineNew="";
		public int iGroup=0;//the undo modification group of which this is a part
		
		public int iCol=-1;//where the cursor was before the modification
		public RTextBoxMod() {
			RReporting.Warning("Warning: RTextBoxMod default constructor was called.");
		}
		public bool CopyTo(RTextBoxMod modNow) {
			bool bGood=false;
			try {
				modNow.iType=iType;
				modNow.iRow=iRow;
				modNow.sLineOld=sLineOld;
				modNow.sLineNew=sLineNew;
				modNow.iGroup=iGroup;
				modNow.iCol=iCol;
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"copying undo modification group","RTextBoxMod CopyTo");
			}
			return bGood;
		}
		public RTextBoxMod(int iSetType, int iSetRow, int iCol_ForReferenceOnly, string sSetLineOld, string sSetLineNew, int iSetGroup) {
			Init(iSetType,iSetRow,iCol_ForReferenceOnly,sSetLineOld,sSetLineNew,iSetGroup);
		}
		public override string ToString() {//debug override
			return DumpStyle();
		}
		public static string TypeToString(int TypeNumber) {
			string sReturn="UNKNOWNTYPE";
			try {
				sReturn=sarrType[TypeNumber];
			}
			catch {//do not report this
				sReturn="BADTYPE_"+TypeNumber.ToString();
			}
			return sReturn;
		}
		public string DumpStyle() {
			string sMsg="{";
			sMsg+="iType:"+iType.ToString()+"; ";
			sMsg+="Type:"+TypeToString(iType)+"; ";
			sMsg+="iRow:"+iRow.ToString()+"; ";
			sMsg+="sLineOld:\""+sLineOld.ToString()+"\"; ";
			sMsg+="sLineNew:\""+sLineNew.ToString()+"\"; ";
			sMsg+="iGroup:"+iGroup.ToString()+"; ";
			sMsg+="iCol (normally starts as -1):"+iCol.ToString()+"; ";
			sMsg+="}";
			return sMsg;
		}
		public bool Init(int iSetType, int iSetRow, int iCol_ForReferenceOnly, string sSetLineOld, string sSetLineNew, int iSetGroup) {
			bool bGood=true;
			iType=iSetType;
			iRow=iSetRow;
			iCol=iCol_ForReferenceOnly;
			if (sSetLineOld!=null) sLineOld=sSetLineOld;
			else {
				bGood=false;
				sLineOld="";
				RReporting.ShowErr("null old line value in modification","initializing undo entry","RTextBoxMod Init() "+DumpStyle());
			}
			if (sSetLineNew!=null) sLineNew=sSetLineNew;
			else {
				bGood=false;
				sLineNew="";
				RReporting.ShowErr("null new line value in modification","initializing undo entry","RTextBoxMod Init() "+DumpStyle());
			}
			iGroup=iSetGroup;
			return bGood;
		}
	}//end class RTextBoxMod
}//end namespace
