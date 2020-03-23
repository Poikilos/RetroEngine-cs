//File created by Jake Gustafson 2007-08-16
using System;

namespace ExpertMultimedia {
	public class Liner {
		#region vars
		private string[] sarrLine=null;//TextArea version of  sText
		public bool bEndWithNewLine;
		private bool bSuspendChanges=false;
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
		private LinerMod[] modarr=null;
		public static int DefaultMaxEditsForUndo=1024;
		public static int DefaultLineBufferSize=1024;
		public bool bChanges=false;
		//public bool Change
		//public void SetAsSaved() {	
		//	bChanges=false;
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
					Base.ShowErr("Tried to get length of text area line "+iLine.ToString());
					return 0;
				}
				if (iLine<iUsed) return sarrLine[iLine].Length;
				else return 0;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Liner get RowLength","getting length of text area line "+iLine.ToString()+" of "+Maximum.ToString()+" (unexpected error)");
			}
			return 0;
		}
		//TODO: public bool GetMonospacedSelPoly(ref IPoly polyReturn, int iCharW, int iCharH) {
		//	try {
		//		if (polyReturn==null) polyReturn=new IPoly();
		//		else polyReturn.Clear();
		//	}
		//	catch (Exception exn) {
		//		Base.ShowExn(exn,"GetSelPoly",);
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
					if (value<=0) {
						Base.WriteLine("Warning: Liner Maximum set to 0");
						sarrLine=null;
						bWarnedOnZeroLines=true;
					}
					else if (value!=Maximum) {
						//int iMin=(Maximum<value)?Maximum:value;
						//int iMax=(Maximum>value)?Maximum:value;
						//do NOT use sarrLine.Length since it is allowed to be null here!
						string[] LinesNew=new string[value];
						if (value<iUsed) Base.WriteLine("Warning: lines being truncated from "+iUsed.ToString()+" used lines to "+value.ToString()+" Maximum.");
						for (int iLine=0; iLine<LinesNew.Length; iLine++) {
							LinesNew[iLine]=(iLine<iUsed)?sarrLine[iLine]:"";
						}
						if (bWarnedOnZeroLines) Base.WriteLine("Liner Maximum set to "+Maximum.ToString()+" (showing value since had been manually set to zero last time)");
						bWarnedOnZeroLines=false;
						sarrLine=LinesNew;
					}
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"Liner set Maximum","resizing text area content array");
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
						Base.WriteLine("Warning: Liner MaxEdits set to 0");
						modarr=null;
						bWarnedOnZeroEdits=true;
					}
					else if (value!=MaxEdits&&value>=iMods) {//value!=MaxEdits) {
						//do NOT use modarr.Length since it is allowed to be null here!
						LinerMod[] modarrNew=new LinerMod[value];
						if (value<iMods) Base.WriteLine("Warning: undo buffer being truncated from "+iMods.ToString()+" actions history to "+value.ToString()+" MaxEdits.");
						for (int iNow=0; iNow<modarrNew.Length; iNow++) {
							modarrNew[iNow]=(iNow<iMods)?modarr[iNow]:new LinerMod();
						}
						if (bWarnedOnZeroEdits) Base.WriteLine("Liner undo buffer MaxEdits set to "+MaxEdits.ToString()+" (showing value since had been manually set to zero last time)");
						bWarnedOnZeroEdits=false;
					}
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"Liner set MaxEdits","resizing undo buffer");
				}
			}
		}
		public int Chars {
			get {
				int iReturn=0;
				try {
					for (int iNow=0; iNow<iUsed; iNow++) {
						iReturn+=sarrLine[iNow].Length+iForcedNewLineLength;
					}
				}
				catch (Exception exn) {
					iReturn=-1;
					Base.ShowExn(exn,"Liner get Chars");
				}
				return iReturn;
			}
		}
		#endregion vars
		#region constructors
		public Liner() {
			Init(DefaultLineBufferSize);
		}
		public Liner(int iSetMaxLines) {
			Init(DefaultLineBufferSize);
		}
		public bool Init(int iSetMaxLines) {
			bool bGood=false;
			bSuspendChanges=true;
			try {
				sarrLine=null;
				Maximum=iSetMaxLines;
				iUsed=0;
				bGood=true;
				modarr=new LinerMod[1024];
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Liner Init(int)","initializing text area");
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
			//		Base.ShowExn(exn,"Liner Init(string[])","initializing text area");
			//	}
			//}
			//else {
			//	bGood=false;
			//	Base.ShowErr("Tried to initialize text area with empty line array","Liner Init(string[])","initializing null text area");
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
			bChanges=true;
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
				Base.ShowExn(exn,"FindMaxLine");
			}
		}
		public void SetFuzzyMaxLinesByLocation(int iLoc) {
			Maximum=Base.LocationToFuzzyMaximum(Maximum,iLoc);
		}
		public void SetFuzzyMaxEditsByLocation(int iLoc) {
			MaxEdits=Base.LocationToFuzzyMaximum(MaxEdits,iLoc);
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
			}
			catch (Exception exn) {
				bWasInRange=false;
				Base.ShowExn(exn,"InRange","cropping cursor variables to range");
			}
			if (!bWasInRange) Base.Warning("selection out of range","{iRowStart:"+iRowStart.ToString()+"; iColStart:"+iColStart.ToString()+";iRowEnd:"+iRowEnd.ToString()+";iColEnd:"+iColEnd.ToString()+";}");
			return bWasInRange;
		}//end InRange
		public bool IsInRange(int iRowStart, int iColStart, int iRowEnd, int iColEnd) {
			bool bReturn=false; //iSelColStart<iSelColEnd
			try { bReturn=iRowStart>=0&&iRowStart<iUsed&&iColStart>=0&&iColEnd>=0&&iRowEnd>=0&&iRowEnd<iUsed&&iColEnd<=sarrLine[iRowEnd].Length;
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
			//do NOT use the more specific Base.OrderPoints(ref iColStart, ref iRowStart, ref iColEnd, ref iRowEnd);
			if (iRowStart==iRowEnd) {
				if (iColStart>iColEnd) Base.Swap(ref iColStart, ref iColEnd);
			}
			else if (iColStart==iColEnd) {
				if (iRowStart>iRowEnd) Base.Swap(ref iRowStart, ref iRowEnd);
			}
			else if (iRowStart>iRowEnd) {
				Base.Swap(ref iRowStart, ref iRowEnd);
				Base.Swap(ref iColStart, ref iColEnd);
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
					Base.ShowExn(exn,"LocOfRowCol "+sMsg,"getting location");
				}
			}
			else Base.ShowErr("row "+iRowStart+" col "+iColStart+" and/or "+iRowEnd+" col "+iColEnd+" are not valid locations");
			return bGood?iCountChars:-1;
		}//end RowColToLinear
		public override string ToString() {
			return ToString(Environment.NewLine);
		}
		public string DumpStyle() {
			string sMsg="{";
			sMsg+="iSelRowStart:"+iSelRowStart.ToString()+"; ";
			sMsg+="iSelColStart:"+iSelColStart.ToString()+"; ";
			sMsg+="iSelRowEnd:"+iSelRowEnd.ToString()+"; ";
			sMsg+="iSelColEnd:"+iSelColEnd.ToString()+"; ";
			sMsg+="Maximum:"+Maximum.ToString()+"; ";
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
				sReturn=Base.SafeSubstring(sReturn,0,sReturn.Length-1);
			}
			return sReturn;
		}
		#endregion utilities
		#region undo
		public bool Undo() {
			bool bGood=true;
			if (iModGroups>0) {
				Base.ShowErr("Undo is not available in this version.");//TODO: finish this
			}
			else {
				bGood=false;
				Base.ShowErr("Can't undo.");
			}
			return bGood;
		}
		private bool AddMod(LinerMod modNow) {//, int iLine, string sOldVal, string sNewVal, int iGroupOfGroup) {
			//Base.WriteLine("("+iModGroups+")AddMod["+iMods+"]=\""+modNow.ToString()+"\"");
			bool bGood=false;
			try {
				if (iMods>MaxEdits) Base.ShowErr("Exceeded Undo Buffer (should not be possible unless there was a computer memory problem) {iMods:"+iMods.ToString()+"; MaxEdits:"+MaxEdits.ToString()+"}");
				else if (iMods==MaxEdits) {Base.ClearErr();SetFuzzyMaxEditsByLocation(iMods);bGood=!Base.HasErr();}
				else bGood=true;
				if (bGood) {
					//Base.Write("Adding...");
					if (iMods<MaxEdits) {
						//Base.Write("Copying...");
						if (modarr[iMods]==null) modarr[iMods]=modNow;
						else modNow.CopyTo(modarr[iMods]);
						iMods++;
						bGood=true;
						//Base.Write("Done.  ");
					}
					else {
						Base.ShowErr("Could not resize undo buffer (should not be possible unless out of memory)");
					}
				}//else SetFuzzyMaxEditsByLocation already has already shown error
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"AddMod","saving undo group");
				bGood=false;
			}
			return bGood;
		}
		private LinerMod LastMod {
			get {
				try {
					if (iMods>0) return modarr[iMods-1];
					else return null;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"Liner get LastMod","accessing undo buffer for saving last modification group");
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
			return (InRange(ref iSelRowStart, ref iSelColStart, ref iSelRowEnd, ref iSelColEnd));
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
				Base.ShowExn(exn,"Home","setting selection "+DumpStyle());
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
				Base.ShowExn(exn,"End","setting selection "+DumpStyle());
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
					Base.ShowExn(exn,"ShiftSelection","moving cursor");
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
			string sVerbNow="inserting line (before starting insertion)";
			int iFreed=0;
			try {
				sVerbNow="inserting line (adding undo step)";
				AddMod(new LinerMod(LinerMod.TypeRemove, iLine, iCol_ForReferenceOnly, sarrLine[iLine], sLine, iModGroups));
				sVerbNow="inserting line (setting line range)";
				if (Maximum<iUsed+1) SetFuzzyMaxLinesByLocation(iUsed);
				sVerbNow="inserting line (pushing lines down)";
				if (iUsed>0) {
					for (iFreed=iUsed+1; iFreed>iLine; iFreed--) {
						sarrLine[iFreed]=sarrLine[iFreed-1];
					}
				}
				else iFreed=iLine;
				sVerbNow="setting line "+iLine.ToString()+" in "+Maximum.ToString()+"-length array";
				if (iFreed==iLine) {
					bGood=true;
					sarrLine[iLine]=sLine;
					iUsed++;
				}
				else Base.ShowErr("Line location error","Liner InsertLine",sVerbNow+" {"
					+"iLine:"+iLine.ToString()+"; "
					+"iFreed:"+iFreed.ToString()+"; "
					+"iUsed:"+iUsed.ToString()+"; "
					+"Maximum:"+Maximum.ToString()+"; "
					+"}");
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Liner InsertLine", sVerbNow+" {"
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
			AddMod(new LinerMod(LinerMod.TypeRemove, iLine, iCol_ForReferenceOnly, sarrLine[iLine], "", iModGroups));
			try {
				int iNow=0;
				for (iNow=iLine; iNow<iUsed-1; iNow++) {
					sarrLine[iNow]=sarrLine[iNow+1];
				}
				if (iNow==iUsed-1) {
					bGood=true;
					iUsed--;
					if (iUsed<0) {	
						Base.ShowErr("iUsed is "+iUsed.ToString()+" in RemoveLine!");
						iUsed=0;
					}
				}
				else bGood=false;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"RemoveLine("+iLine.ToString()+") {iUsed:"+iUsed.ToString()+")");
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
			bool bTest=AddMod(new LinerMod(LinerMod.TypeModify, iLine, iCol_ForReferenceOnly, sarrLine[iLine], sLine, iModGroups));
			sarrLine[iLine]=sLine;
			if (!bWaitForAnotherPartOfGroup) {
				iModGroups++;
				if (!bSuspendChanges) OnChange();
			}
			return true;
		}
		#endregion concrete editing (record undo)
		#region abstract editing
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
		public bool SetText(string sVal) {
			return SetLines(Base.StringToLines(sVal));
		}
		public bool Insert(string sTyped) {
			if (Base.Contains(sTyped,Environment.NewLine)
				||Base.Contains(sTyped,'\r')
				||Base.Contains(sTyped,'\n')
				) {
				Base.ShowErr("Inserting newlines is not possible!  Use Return() instead!");
				return false;
			}
			bool bGood=false;
			try {
				if (!IsZeroSelection()) {
					bGood=Delete();
					SetZeroSelection(iSelRowStart,iSelColStart);
				}
				if (SelectionIsInRange()) {
					if (!ChangeLine(iSelRowStart,iSelColStart,Base.SafeInsert(sarrLine[iSelRowStart],iSelColStart, sTyped))) {
						bGood=false;
					}
					else {
						SetZeroSelection(iSelRowStart, iSelColStart+sTyped.Length);
					}
				}
				else Base.ShowErr("Cannot type here since selection is not in range","Liner Insert","typing outside of range "+DumpStyle());
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Liner Insert","typing {sTyped:\""+sTyped+"\"; Selection ("+iSelColStart+","+iSelRowStart+") to ("+iSelColEnd+","+iSelRowEnd+")}");
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
					if ( !ChangeLine(iSelRowStart,iSelColStart,Base.SafeSubstring(sSplit,0,iSelColStart))
						||!InsertLine(iSelRowStart+1,iSelColStart,Base.SafeSubstring(sSplit,iSelColStart)) ) {
						bGood=false;
					}
					SetZeroSelection(iSelRowStart+1, 0);
				}
				else Base.ShowErr("Cannot enter a line return here since selection is not in range "+DumpStyle());
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Liner Return","typing {sTyped:Return; Selection ("+iSelColStart+","+iSelRowStart+") to ("+iSelColEnd+","+iSelRowEnd+")}");
			}
			bSuspendChanges=bSuspendChangesPrev;
			if (!bSuspendChanges) OnChange();
			return bGood;
		}//end Return
		public bool Backspace() {
			bool bGood=false;
			bool bSuspendChangesPrev=bSuspendChanges;
			bSuspendChanges=true;
			string sVerbNow="preparing location";
			int iRowStart=iSelRowStart, iRowEnd=iSelRowEnd, iColStart=iSelColStart, iColEnd=iSelColEnd;
			OrderLocations(ref iRowStart, ref iColStart, ref iRowEnd, ref iColEnd);
			try {
				if (IsZeroSelection()) {
					if (SelectionIsInRange()) {
						if (iColStart==0) {//if at beginning of line
							sVerbNow="checking row";
							if (iRowStart>0) {
								sVerbNow="moving to previous line";
								if(ChangeLine(iRowStart-1,Line(iRowStart-1).Length,Line(iRowStart-1)+Line(iRowStart),true)) {
									sVerbNow="removing leftover space";
									if (RemoveLine(iRowStart,0)) {
										sVerbNow="resetting selection";
										if (LastMod!=null) SetZeroSelection(iRowStart-1,LastMod.iCol);
										else {
											SetZeroSelection(iRowStart-1,0);
											Base.ShowErr("Last modification ("+(iMods-1).ToString()+") was not recorded properly! "+DumpStyle());
										}
									}
								}
							}
							//else do nothing since at top left
						}
						else {
							sVerbNow="changing line";
							bGood=ChangeLine(iRowStart,iColStart,Base.SafeRemoveIncludingEnder(Line(iRowStart),iColStart-1,iColEnd-1));
						}
						sVerbNow="shifting selection";
						ShiftSelection(0,-1);
					}
					else Base.ShowErr("Backspace failed since selection out of range. "+DumpStyle());
					sVerbNow="finishing";
				}
				else {//else not IsZeroSelection
					sVerbNow="deleting";
					bGood=Delete();
					//if (IsZeroSelection()) ShiftSelection(0,-1);
					//else {
					//	SetZeroSelection(iRowStart,iColStart);
					//}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Liner Backspace","deleting before cursor ("+sVerbNow+") "+DumpStyle());
			}
			bSuspendChanges=bSuspendChangesPrev;
			sVerbNow="onchange";
			if (!bSuspendChanges) OnChange();
			sVerbNow="finishing";
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
													//Base.ShowErr("Couldn't remove remnant of next line","Liner Delete","deleting line break (shifting following lines)"+DumpStyle());
												}
											}
											else {
												bGood=false;
												iModGroups++;//manually concludes modgroup (undo step)
												Base.ShowErr("Couldn't append next line to current line","Liner Delete","deleting line break "+DumpStyle());
											}
										}
										//else do nothing
									}//end delete one character at end of line
									else {//delete one character not at end of line
										if (ChangeLine( iRowStart, iColStart, Base.SafeRemove(Line(iRowStart),iColStart,1)) ) {
											SetZeroSelection(iRowStart,iColStart);
										}
										else {
											bGood=false;
											Base.ShowErr("Couldn't append next line to current line","Liner Delete","deleting letter "+DumpStyle());
										}
									}
								}//end if delete one character 
								else {//delete more than one character
									if (ChangeLine(iRowStart,iColStart,Base.SafeRemoveExcludingEnder(Line(iRowStart),iColStart,iColEnd))) {
										SetZeroSelection(iRowStart,iColStart);
									}
									else bGood=false;
								}
							}
							else if (iRowStart==iUsed) {
								//do nothing, but don't show error if at virtual line after last line
							}
							else Base.ShowErr("No text to delete (end of text area)","Liner Delete","trying to delete text in unused area"+DumpStyle());
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
										Base.SafeSubstring(Line(iRow),0,iColStart)+Base.SafeSubstring(Line(iRowEnd),iColEnd),true)) {
									bGood=false;
									Base.ShowErr("Details: can't collapse data to first line","Liner Delete","deleting text (removing trailing lines) "+DumpStyle());
									break;
								}
							}
							else { //remove all lines INCLUDING the last line which was preserved as necessary above
								if(!RemoveLine(iRow,0,true)) {
									bGood=false;
									Base.ShowErr("Details: failed to remove trailing lines","Delete","deleting text (removing trailing lines) "+DumpStyle());
									break;
								}
							}
						}//end for iRow
						iModGroups++;//manually close undo step
						SetZeroSelection(iRowStart,iColStart);
						//Base.ShowErr("Deleting multirow areas is not yet implemented.");
					}//end else multirow
				}//end if in range
				else Base.ShowErr("Deleting failed since selection out of range. "+DumpStyle());
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Liner Delete","deleting selected text "+DumpStyle());
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
	}//end class Liner
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public class LinerMod {
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
		public LinerMod() {
			Base.WriteLine("Warning: LinerMod default constructor was called.");
		}
		public bool CopyTo(LinerMod modNow) {
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
				Base.ShowExn(exn,"LinerMod CopyTo","copying undo modification group");
			}
			return bGood;
		}
		public LinerMod(int iSetType, int iSetRow, int iCol_ForReferenceOnly, string sSetLineOld, string sSetLineNew, int iSetGroup) {
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
				Base.ShowErr("null old line value in modification "+DumpStyle());
			}
			if (sSetLineNew!=null) sLineNew=sSetLineNew;
			else {
				bGood=false;
				sLineNew="";
				Base.ShowErr("null new line value in modification "+DumpStyle());
			}
			iGroup=iSetGroup;
			return bGood;
		}
	}//end class LinerMod
}//end namespace