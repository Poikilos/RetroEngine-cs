/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 7/18/2005
 * Time: 8:01 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 using System;
 using System.IO;
 //TODO: Modify this to remote to a StatusTracker class, using Packets via IPC 
 namespace ExpertMultimedia {
 	/// <summary>
 	/// Queue for sending errors & noticesback to retroengine.
 	/// </summary>
	public class StatusQ {
		public string sFuncNow;
		private string[] sarrMessage;
		public string sToolTip;
		public bool bConsoleOut=false;
		public bool bErrLogOut=false;
 		//public static int IDOther=0;
 		//public static int IDRetroEngine=1;
 		//public static int IDHTMLPage=2;
 		//public static int IDWebsite=3;
		//private int[] iarrToID;
		//private int[] iarrFromID;
		private int iMax; //array size
		private int iMaxExceptions; //max exceptions to track
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int[] iarrExceptionLine;
		private int[] iarrExceptionRepetitions;
		private string[] sarrExceptionFile;
		private int iExceptions;
		public int TrackedErrors {
			get {
				return iExceptions;
			}
		}
		private int iLast {	get { return Wrap(iFirst+iCount-1);	} }
		private int iNew { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		private bool ErrorInErrorQ(string sErrorX) {
			bool bGood=false;
			if (IsFull==false) {
				//TODO: bGood=Enq(HTMLPage.DateTimePathString(true)+" -- "+"Error Queue"+" Problem in "+sFuncNow+": "+sErrorX);
			}
			else {
				if (bConsoleOut) {
					//Console.WriteLine("[Couldn't write ErrorInErrorQ]:");
					//Console.WriteLine(sErrorX);
				}
				if (bErrLogOut) {
					LogToFile("[Couldn't write ErrorInErrorQ]:");
					LogToFile(sErrorX);
				}
				bGood=false;
			}
			return bGood;
		}
		public StatusQ() { //Constructor
			Init(512); 
		}
		public StatusQ(int iMaxMessages) { //Constructor
			Init(iMaxMessages);
		}
		public string DumpTrackedErrorsToStyleNotation() {
			string sReturn="";
			try {
				if (iExceptions>0) {
					for (int i=0; i<iExceptions; i++) {
						sReturn+=sarrExceptionFile[i]+":line#"+iarrExceptionLine[i]
							+" "+iarrExceptionRepetitions[i]+"time(s); ";
					}
					if (sReturn.EndsWith("; ")) sReturn=sReturn.Substring(sReturn.Length-3);
				}
				else sReturn="";
			}
			catch (Exception exn) {
				sFuncNow="DumpTrackedErrorsToStyleNotation";
				ErrorInErrorQ("Exception error--"+exn.ToString());
			}
			return sReturn;
		}
		static int iLog=0;
		static int iMaxLog=100;
		public bool LogToFile(string sValue) {
			//TODO: change to IPC message (eliminate StatusQ class)
			StreamWriter swErr; //TODO: remove this and do something else
			bool bGood=true;
			int iRemove;
			int iRemoves;
			if (iLog>=iMaxLog) return bGood;
			iLog++;
			try {
				iRemoves=Environment.NewLine.Length;
				iRemove=sValue.IndexOf(Environment.NewLine);
				while (iRemove!=-1) {
					sValue=sValue.Remove(iRemove, iRemoves);
					iRemove=sValue.IndexOf(Environment.NewLine);
				}
				swErr=File.AppendText("1.log.txt"); //swErr=File.AppendText(RetroEngine.sPathFileLog);
				swErr.WriteLine(sValue);
				swErr.Close();
			}
			catch (Exception exn) {
				ErrorInErrorQ("Exception error writing LogToFile--"+exn.ToString());
				bGood=false;
			}
			return bGood;
		}
		private void Init(int iMax1) { //always called by Constructor
			sFuncNow="Init()";
			iFirst=0;
			iMax=iMax1;
			iCount = 0;
			iExceptions=0;
			try {
				sarrMessage=new string[iMax];
				iMaxExceptions=iMax*2;
				sarrExceptionFile=new string[iMaxExceptions];
				iarrExceptionLine=new int[iMaxExceptions];
				iarrExceptionRepetitions=new int[iMaxExceptions];
				//iarrToID = new int[iMax];
				//iarrFromID = new int[iMax];
			}
			catch (Exception exn) {
				ErrorInErrorQ("Exception, Error Queue couldn't initialize sarrMessage--"+exn.ToString());
			}
		}
		public void EmptyNOW () {
			sFuncNow="EmptyNOW";
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making sarrMessage circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		public bool Enq(string sNew) { //Enqueue
			if (!IsFull) {
				try {
					if (false==IncrementException(sNew)) {
						//exception string is analyzed by IncrementException
						//sFuncNow="Enq("+sNew+")"; //DON'T set sFuncNow or will overwrite caller function name!
						//if (bConsoleOut) Console.WriteLine(sNew);
						if (bErrLogOut) LogToFile(sNew);
						sarrMessage[iNew]=sNew;
						iCount++;
					}
					return true;
				}
				catch (Exception exn) {
					//if (bConsoleOut) Console.WriteLine("[Error in Error Queue]");
					if (bErrLogOut) LogToFile("[Error in Error Queue]");
					ErrorInErrorQ("Exception error setting sarrMessage["+iNew.ToString()+"]--"+exn.ToString());
				}
				return false;
			}
			else {
				//if (bConsoleOut) {
				//	Console.WriteLine("[queue full]:");
				//	Console.WriteLine(sNew);
				//}
				//sLastErr="  This queue is full -- iCount="+iCount.ToString();
				return false;
			}
		}
		public bool IncrementException(string sLogLine) {
			int iNoteEnder;
			bool bYes=false;
			try {
				iNoteEnder=sLogLine.LastIndexOf(' ');
				if ((iNoteEnder>-1) && (sLogLine.Substring(iNoteEnder-8, 8)==".cs:line") && (iExceptions<iMaxExceptions)) {

					int iPathLen=sLogLine.IndexOf(".cs:line")+3;
					
					string sPathFake=sLogLine.Substring(0,iPathLen);
					FileInfo fi1=new FileInfo(sPathFake);
					string sRight=fi1.Name;
					string sLeft=fi1.FullName.Substring(0,fi1.FullName.Length-sRight.Length);
					int iLastDelim=sLeft.Length-1;//int iLastDelim=sLogLine.LastIndexOf(RetroEngine.PathDelimiter);

					int iNumLength=sLogLine.Length-(iNoteEnder+1);
					int iLineNow=Base.ConvertToInt(sLogLine.Substring(iNoteEnder+1, iNumLength));
					string sLineNow;
					if (iLastDelim>0)
						sLineNow=sLogLine.Substring(iLastDelim+1, (sLogLine.Length-(iNumLength+1))-(iLastDelim+1));
																		//iNumLength+1 includes preceding space
					else sLineNow="?*.cs";
					int iPreviousOccurranceOfException=FindPreviousOccurranceOfException(sLineNow, iLineNow);
					if (iPreviousOccurranceOfException<0) { //exn.g. if didn't occur already
						iarrExceptionLine[iExceptions]=iLineNow;
						sarrExceptionFile[iExceptions]=sLineNow;
						iarrExceptionRepetitions[iExceptions]=1;
						iExceptions++;
					}
					else {
						iarrExceptionRepetitions[iExceptions]++;
						bYes=true;
					}
				}
			}
			catch (Exception exn) {
				sFuncNow="IsException";
				ErrorInErrorQ("Exception error analyzing log line--"+exn.ToString());
			}
			return bYes;
		}
		public int FindPreviousOccurranceOfException(string sFileCS, int iLine) {
			int iReturn=-1;
			try {
				for (int i=0; i<iExceptions; i++) {
					if (iarrExceptionLine[i]==iLine && sarrExceptionFile[i]==sFileCS) {
						iReturn=i;
						break;
					}
				}
			}
			catch (Exception exn) {
				sFuncNow="FindPreviousOccurranceOfException";
				ErrorInErrorQ("Exception error--"+exn.ToString());
			}
			return iReturn;
		}
		public bool IsException(string sLogLine) {
			bool bYes=false;
			try {
				if (sLogLine.Substring(sLogLine.LastIndexOf(' ')-8, 8)==".cs:line")
					bYes=true;
			}
			catch (Exception exn) {
				sFuncNow="IsException";
				ErrorInErrorQ("Exception error analyzing log line--"+exn.ToString());
			}
			return bYes;
		}
		public string Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			//sFunc="Deq()";
			if (IsEmpty) {
				sFuncNow="Deq() (none to return so returned \"\")";
				return "";
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			try {
				return sarrMessage[iReturn];
			}
			catch (Exception exn) {
				sFuncNow="Deq() (Exception error at "+iReturn.ToString()+" so returned \"\")--"+exn.ToString();
			}
			return "";
		}
		public string Peek() {
			if (IsEmpty) {
				sFuncNow="Deq() (none to return so returned \"\")";
				return "";
			}
			try {
				return sarrMessage[iFirst];
			}
			catch (Exception exn) {
				sFuncNow="Deq() (Exception error so returned \"\")--"+exn.ToString();
			}
			return "";
		}
		public string Peek(int iOffset) {
			int iSeek=0;
			try {
				if (IsEmpty) {
					sFuncNow="Peek("+iOffset.ToString()+") (none to return so returned \"\")";
					return "";
				}
				iSeek=Wrap(iFirst+iOffset);
				int iLast=Wrap(Wrap(iFirst+iCount)-1);
				bool bWrapped=(iLast<iFirst)?true:false;
				bool bValid;
				if (bWrapped) bValid=((iSeek<=iLast)||(iSeek>=iFirst));
				else bValid=((iSeek<=iLast)&&(iSeek>=iFirst));
				if (bValid)  {
					return sarrMessage[iSeek];
				}
				else {
					sFuncNow="Peek("+iOffset.ToString()+") (resulted in out-of-bounds status message #"+iSeek.ToString()+")";
					return "";
				}
			}
			catch (Exception exn) {
				sFuncNow="Peek("+iOffset.ToString()+") (Exception error seeking to status message #"+iSeek.ToString()+")";
				ErrorInErrorQ("Exception error while Peeking at error offset#"+iOffset.ToString()+"--"+exn.ToString());
			}
			return "";
		}
		/*
		public string Delete(int iOffset) {
			if (IsEmpty) {
				sFuncNow="Delete("+iOffset.ToString()+") (none to return so returned \"\")";
				return "";
			}
			int iSeek=Wrap(iFirst+iOffset);
			int iLast=Wrap(Wrap(iFirst+iCount)-1);
			bool bWrapped=(iLast<iFirst)?true:false;
			bool bValid;
			string sReturn;
			if (bWrapped) bValid=((iSeek<=iLast)||(iSeek>=iFirst));
			else bValid=((iSeek<=iLast)&&(iSeek>=iFirst));
			if (bValid)  {
				sReturn=sarrMessage[iSeek];
				return sarrMessage[iReturn];
			}
			else {
				sFuncNow="Delete("+iOffset.ToString()+") (resulted in out-of-bounds status message #"+iSeek.ToString()+")";
				return "";
			}
		}
		*/
		//public int MessagesFor(int iFor)
	}//end class StatusQ
 }
