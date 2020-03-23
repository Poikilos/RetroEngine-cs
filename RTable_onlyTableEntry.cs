//////////////////////////////////// TableEntry /////////////////////////////////////////////
	
	
	public class TableEntry {
		private string[] fieldarr=null;
		private int iCols=0;
		public bool bFlagForDelete=false;
		public int Columns {
			get {
				RecheckIntegrity();
				return iCols;
			}
			set {
				if (value>Maximum) Maximum=value;
				if (Maximum>=value) iCols=value;
				else {
					Console.Error.WriteLine("Unable to set maximum TableEntry columns to "+value.ToString()+" (buffer of "+Maximum.ToString()+" columns couldn't be increased)");
					iCols=Maximum;
				}
			}
		}
		private int Maximum {
			get { return (fieldarr==null) ? 0 : fieldarr.Length; }
			set {
				try {
					if (value>Maximum) {
						int iSizeNew=value+(int)((double)value*.25)+1;
						if (iSizeNew<30) iSizeNew=30;
						string[] fieldarrNew=new string[iSizeNew];
						if (fieldarr==null) {
							fieldarr=fieldarrNew;
							for (int iNow=0; iNow<Maximum; iNow++) fieldarr[iNow]="";
						}
						else {
							for (int iNow=0; iNow<fieldarrNew.Length; iNow++) {
								if (iNow<Maximum) fieldarrNew[iNow]=fieldarr[iNow];
								else fieldarrNew[iNow]="";
							}
							fieldarr=fieldarrNew;
						}
					}
				}//end try
				catch (Exception exn) {
					Console.Error.WriteLine("Error setting TableEntry field array length:");
					Console.Error.WriteLine(exn.ToString());
				}
			}//end set Maximum
		}//end Maximum
		public TableEntry() {
			Maximum=30;
		}
		public TableEntry Copy() {
			TableEntry tCopy=null;
			RReporting.sParticiple="creating row (during TableEntry copy)";
			tCopy=new TableEntry();
			RReporting.sParticiple="copying fields (during TableEntry copy)";
			for (int iNow=0; iNow<this.Columns; iNow++) {
				tCopy.AppendField(fieldarr[iNow]);
			}
			RReporting.sParticiple="finished TableEntry Copy";
			return tCopy;
		}
		public TableEntry(string[] sarrLiteralFieldsCopyByRef) {
			SetByRef(sarrLiteralFieldsCopyByRef);
		}
		public TableEntry(string[] sarrLiteralFields, bool bCopyByRef) {
			string[] sarrLiteralFieldsCopyByRef;
			if (bCopyByRef) sarrLiteralFieldsCopyByRef=sarrLiteralFields;
			else {
				if (sarrLiteralFields!=null&&sarrLiteralFields.Length>0) {
					sarrLiteralFieldsCopyByRef=new string[sarrLiteralFields.Length];
					for (int iNow=0; iNow<sarrLiteralFields.Length; iNow++) {
						sarrLiteralFieldsCopyByRef[iNow]=sarrLiteralFields[iNow];
					}
				}
				else sarrLiteralFieldsCopyByRef=null;
			}
			SetByRef(sarrLiteralFieldsCopyByRef);
		}//TableEntry
		public bool SetByRef(string[] sarrLiteralFieldsCopyByRef) {
			bool bGood=false;
			if (sarrLiteralFieldsCopyByRef!=null) {
				fieldarr=sarrLiteralFieldsCopyByRef;
				iCols=fieldarr.Length;
				bGood=true;
			}
			else {
				Maximum=1;
				iCols=0;
				bGood=false;
			}
			return bGood;
		}
		
		public int IndexOfI_AssumingNeedleIsLower(string sFieldValueX) {
			int iReturn=-1;
			RecheckIntegrity();
			if (Maximum>0&&iCols>0) {
				for (int iNow=0; iNow<iCols; iNow++) {
					if (fieldarr[iNow].ToLower()==sFieldValueX) {
						iReturn=iNow;
						break;
					}
				}
			}
			else Console.Error.WriteLine("Can't find value in empty TableEntry");
			return iReturn;
		}//end IndexOfI_AssumingNeedleIsLower
		public int IndexOfExactValue(string sFieldValueX) {
			int iReturn=-1;
			RecheckIntegrity();
			if (Maximum>0&&iCols>0) {
				for (int iNow=0; iNow<iCols; iNow++) {
					if (fieldarr[iNow]==sFieldValueX) {
						iReturn=iNow;
						break;
					}
				}
			}
			else Console.Error.WriteLine("Can't find value in empty TableEntry");
			return iReturn;
		}//end IndexOfExactValue
		private void RecheckIntegrity() {
			if (iCols>Maximum) iCols=Maximum;
		}
		public string ToCSVLine(char cFieldDelimX, char cTextDelimX, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
			return ToCSVLine(cFieldDelimX,cFieldDelimX,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty,0,Columns);
		}
		public string ToCSVLine(char cFieldDelimX, char cTextDelimX, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty, int ColumnStart, int ColumnCount) {
			return RTable.RowToCSVLine(fieldarr, cFieldDelimX, cTextDelimX, ColumnStart, ColumnCount, bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty);
		}
		public void AppendField(string sLiteralData) {
			int iColsPrev=Columns;
			Columns++;
			try {
				fieldarr[Columns-1]=sLiteralData;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not finish appending field at column "+iColsPrev.ToString()+" to row:"+exn.ToString());
			}
		}
		public string Field(int AtInternalColumnIndex) {
			string sReturn=null;
			if (this.fieldarr!=null) {
				if (AtInternalColumnIndex<fieldarr.Length) {
					if (AtInternalColumnIndex<iCols) {
						sReturn=fieldarr[AtInternalColumnIndex];
						if (sReturn==null) {
							RReporting.Warning("Getting null column string--converting to zero-length","getting field value","tableEntry.Field");
							sReturn="";
						}
					}
					else RReporting.ShowErr("Field array iCols count for this row is not as wide as internal column index given","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
				}
				else RReporting.ShowErr("Field array maximum for this row is not as wide as internal column index given","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
			}
			else RReporting.ShowErr("Field array is null in this row","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
			
			return sReturn;
		}//end Field
		public bool SetField(int AtInternalColumnIndex, string sValue) {
			bool bGood=false;
			try {
				if (fieldarr!=null) {
					if (AtInternalColumnIndex<fieldarr.Length) {
						if (AtInternalColumnIndex<iCols) {
							fieldarr[AtInternalColumnIndex]=sValue;
							bGood=true;
						}
						else RReporting.ShowErr("Column is out of range of iCols count for this row","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+",...)");
					}
					else RReporting.ShowErr("Column is out of range of internal field array","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+","+RString.SafeString(sValue,false)+")");
				}
				else RReporting.ShowErr("Can't set field--row has null internal field array","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+",...)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"setting field","rtable SetField("+AtInternalColumnIndex+",...){fieldarr.Length:"+RReporting.SafeLength(fieldarr)+"}");
			}
			return bGood;
		}//end SetField
		public void Clear() {
			if (this.fieldarr!=null) {
				for (int i=0; i<this.fieldarr.Length; i++) {
					this.fieldarr[i]="";
				}
			}
		}//end Clear
	}//end TableEntry
}//end namespace
