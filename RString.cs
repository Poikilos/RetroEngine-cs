using System;
using System.Text.RegularExpressions;
//using System.Text; //not neede (RString replaces StringBuilder)

namespace ExpertMultimedia {
	public class RString { //like stringbuilder but a little better
		#region variables
		public static int DefaultMaxCapacity=1024;
		public static bool bGoodString=true;
		//public static DefaultOffset=0;
		private char[] carrData=null;
		private int iStart=0;
		private int iEnder=0;//exclusive ender
		private static readonly char[] sPossibleNewLineChars=new char[]{'\r','\n'};//(CR+LF, 0x0D 0x0A, {13,10})
		private static readonly char[] sPossibleSpacingChars=new char[]{' ','\t','\0'};

		public static readonly char[] carrLineBreakerInvisible=new char[]{' ','\t'};
		public static readonly char[] carrLineBreakerVisible=new char[]{'-',';',':','>'};
		public static readonly string[] sarrDigit=new string[] {"0","1","2","3","4","5","6","7","8","9"};
		public static readonly char[] carrDigit=new char[] {'0','1','2','3','4','5','6','7','8','9'};
		public static readonly char[] carrAlphabetUpper=new char[] {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
		public static readonly string[] sarrAlphabetUpper=new string[] {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"};
		public static readonly char[] carrAlphabetLower=new char[] {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
		public static readonly string[] sarrAlphabetLower=new string[] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"};
		public static bool IsAlphanumeric(char val) {
			return IsUpper(val)||IsLower(val)||IsDigit(val);
		}
		public static string sRemoveSpacingBeforeNewLines1=" "+Environment.NewLine;
		public static string sRemoveSpacingBeforeNewLines2="\t"+Environment.NewLine;
		public static readonly string[] sarrConsonantLower=new string[] {"b","c","d","f","g","h","j","k","l","m","n","p","q","r","s","t","v","w","x","y","z"};
		public static readonly string[] sarrConsonantUpper=new string[] {"B","C","D","F","G","H","J","K","L","M","N","P","Q","R","S","T","V","W","X","Y","Z"};
		public static readonly string[] sarrVowelLower=new string[] {"a","e","i","o","u"};
		public static readonly string[] sarrVowelUpper=new string[] {"A","E","I","O","U"};
		public static readonly string[] sarrWordDelimiter=new string[] {"...","--",",",";",":","/","&","#","@","$","%","_","+","=","(",")","{","}","[","]","*",">","<","|","\"","'"," ","`","^","~"}; //CAN HAVE "'" since will simply be dealt differently when contraction, after words are split apart
		public static readonly char[] carrSentenceDelimiter=new char[] {'.','!','?'};//formerly string[] sarrSentenceDelimiter
		private const int AssignmentBetween=0;
		private const int AssignmentName=1;
		private const int AssignmentOperator=2;
		private const int AssignmentValue=3;
		//public const int iToLower=32;
		public const char ui16ToLower=(char)32;
		public const int iToUpper=-32;
		public const char ui16UpperA='A';
		public const char ui16UpperZ='Z';
		public const char ui16LowerA='a';
		public const char ui16LowerZ='z';
		///<summary>
		///Optimized non-lowercase check (one comparison operation)
		///</summary>
		public static bool IsNotLower(char cNow) {
			return cNow<ui16LowerA;
		}
		public static bool IsUpper(char cNow) {
			return cNow<=ui16UpperZ&&cNow>=ui16UpperA;
		}
		public static bool HasUpperCharacters(string sVal) {
			if (sVal!=null) {
				for (int iNow=0; iNow<sVal.Length; iNow++) {
					if (IsUpper(sVal[iNow])) return true;
				}
			}
			return false;
		}
		///<summary>
		///Optimized non-uppercase check (one comparison operation)
		///</summary>
		public static bool IsNotUpper(char cNow) {
			return cNow>ui16UpperZ;
		}
		public static bool IsLower(char cNow) {
			return cNow<=ui16LowerZ&&cNow>=ui16LowerA;
		}
		public static char ToLower(char cNow) {
			if (IsUpper(cNow)) return cNow+iToLower;
			else return cNow;
		}
		///<summary>
		///Compares two characters (case-insensitive)
		///</summary>
		public static bool EqualsI(char c1, char c2) {
			return ToLower(c1)==ToLower(c2);
		}
		public static bool EqualsI_AssumingNeedleIsLower(string c1, string c2) {
			return c1!=null&&c2!=null&&CompareAtI_AssumingNeedleIsLower(c1,c2,0,c2.Length);
		}
		public static bool EqualsI(string c1, string c2) {
			return c1!=null&&c2!=null&&CompareAtI(c1,c2,0,c2.Length);
		}
		public static char ToUpper(char cNow) {
			if (IsLower(cNow)) return cNow+iToUpper;
			else return cNow;
		}
		public char this [int index] { //indexer
			get { return carrData[index+iStart]; }
			set { carrData[index+iStart]=value; }
		}
		public int Length {
			get { return iEnder-iStart; }
			set { iEnder=iStart+value; } //debug making old data appear again if bigger
		}
		public int Capacity {
			get { return MaxCapacity; }
			set { MaxCapacity=value; }
		}
		public int MaxCapacity {
			get { return carrData!=null?carrData.Length:0; }
			set {
				if (value>0) {
					if (carrData!=null) {
						//MoveInternalStringToMiddle();
						int iNewCapacity=RMath.SafeMultiply(value,2);
						char[] carrOld=carrData;
						carrData=new char[iNewCapacity];
						//int iMin=carrData.Length<carrOld.Length?carrData.Length:carrOld.Length;
						for (int iNow=iStart; iNow<carrData.Length; iNow++) {
							if (iNow<carrOld.Length) carrData[iNow]=carrOld[iNow];
							else break;
						}
						if (iEnder>carrData.Length) iEnder=carrData.Length;
					}
					else {
						carrData=new char[value*2];
						iStart=value;
						Length=0;
					}
				}//end if value>0
				else {
					carrData=null;
					iStart=0;
					Length=0;
					Console.Error.WriteLine("Warning: set Capacity to "+value.ToString()+" (null)");
				}
			}//end set
		}//end MaxCapacity get,set
		#endregion variables

		#region constructors
		public RString(string sNow, int iCapacity) {
			From(sNow,iCapacity);
		}
		public RString(string sNow) {
			From(sNow);
		}
		public RString() {
			MaxCapacity=DefaultMaxCapacity;
		}
		public RString(int iCapacity) {
			MaxCapacity=iCapacity;
		}
		public RString(char[] carrX, int StartX, int LenX) {
			From(carrX,StartX,LenX);
		}
		public RString(RString fstrX) {
			From(fstrX);
		}
		public void From(string sNow) {
			From(sNow,sNow.Length);
		}
		public void From(string sNow, int iCapacity) {
			Clear();
			Capacity=iCapacity;
			Append(sNow);
		}
		public void From(RString fstrX) {
			MaxCapacity=fstrX.MaxCapacity;
			From(fstrX.carrData,fstrX.iStart,fstrX.Length);
		}
		public void From(char[] carrX, int StartX, int LenX) {
			if (carrX!=null){
				if (StartX+LenX>carrX.Length) {
					RReporting.Warning( String.Format("Truncating source from Start:{0}; Length:{1} to fit source character array size {2}",StartX,LenX,carrX.Length) );
					LenX=carrX.Length-StartX;
				}
				if (LenX>0) {
					MaxCapacity=carrX.Length;
					Length=0;
					for (int iNow=0; iNow<carrX.Length; iNow++) {
						carrData[iEnder]=carrX[iNow];
						iEnder++;
					}
				}
				else {
					RReporting.Warning( String.Format("Getting no data from character array from Start:{0}; Length:{1} and source character array size {2}",StartX,LenX,carrX.Length) );
					Length=0;
				}
			}
			else {
				Length=0;
			}
		}
		#endregion constructors

		public void ReplaceAny(char[] sOld, char cNew) {
			ReplaceAny(carrData,sOld,cNew,iStart,iEnder);
		}
		
		public RString Substring(int StartX, int LenX) {
			RString fstrReturn=new RString(this);
			fstrReturn.Trim(StartX,LenX);
			return fstrReturn;
		}
		public bool EndsWith(string sVal) {
			try {
				if (carrData!=null&&sVal!=null&&sVal.Length<=iEnder-iStart&&sVal.Length>0) {
					int iRel=iEnder-1;
					for (int iNow=sVal.Length-1; iNow>=0; iNow--) {
						if (carrData[iRel]!=sVal[iNow]) return false;
						iRel--;
					}
					return true;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return false;
		}
		public bool StartsWith(string sVal) {
			try {
				if (carrData!=null&&sVal!=null&&sVal.Length<=iEnder-iStart&&sVal.Length>0) {
					int iRel=iStart;
					for (int iNow=0; iNow<sVal.Length; iNow++) {
						if (carrData[iRel]!=sVal[iNow]) return false;
						iRel++;
					}
					return true;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return false;
		}//end StartsWith
		public static int CountInstances(string Haystack, string Needle) {
			if (Haystack!=null&&Needle!=null) return CountInstances(Haystack, Needle, 0, RString.SafeLength(Haystack));
			else return 0;
		} //end CountInstances(string,string)
		///<summary>
		///Does NOT count redundant instances i.e. CountInstances(rrr,rr,...) finds 1, 
		/// i.e. CountInstances(rrrr,rr,...) finds 2;
		///</summary>
		public static int CountInstances(string Haystack, string Needle, int iStart, int iEndBefore) {
			int iCount=0;
			try {
				if (Needle.Length!=0) {
					while (iStart+Needle.Length<=iEndBefore) {
						if (CompareAt(Haystack,Needle,iStartNow)) {
							iCount++;
							iStart+=Needle.Length;
						}
						else iStart++;
					}
				}
				else RReporting.ShowErr(  "Blank string search was skipped", "", 
				  String.Format("CountInstances({0},{1})", RReporting.DebugStyle("in",Haystack ,false,false),RReporting.DebugStyle("search-for",Needle ,false,false))  );
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return iCount;
		} //end CountInstances(string,string,iStart,iEndEx)
		///<summary>
		///Does NOT count redundant instances i.e. CountInstances(rrr,rr,...) finds 1, 
		/// i.e. CountInstances(rrrr,rr,...) finds 2;
		///</summary>
		public static int CountInstancesI(string Haystack, string Needle, int iStart, int iEndBefore) {
			int iCount=0;
			try {
				if (Needle.Length!=0) {
					while (iStart+Needle.Length<=iEndBefore) {
						if (CompareAtI(Haystack,Needle,iStartNow)) {
							iCount++;
							iStart+=Needle.Length;
						}
						else iStart++;
					}
				}
				else RReporting.ShowErr(  "Blank string search was skipped", "", 
				  String.Format("CountInstances({0},{1})", RReporting.DebugStyle("in",Haystack ,false,false),RReporting.DebugStyle("search-for",Needle ,false,false))  );
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return iCount;
		} //end CountInstancesI(string,string,iStart,iEndEx)
 		public static int CountInstances(string Haystack, char Needle) {
 			if (Haystack!=null&&Needle!=null) return CountInstances(Haystack, Needle, 0,Haystack.Length);
 			else return 0;
 		}
		public static int CountInstances(string Haystack, char Needle, int iStart, int iEndBefore) {
			int iCount=0;
			int iLocNow=0;
			int iStartNow=0;
			try {
				if (Haystack!=null&&Haystack!="") {
					for (int iChar=iStart; iChar<Haystack.Length&&iChar<iEndBefore; iChar++) {
						if (Haystack[iChar]==Needle) iCount++;
					}
				}
				else RReporting.ShowErr("Tried to count matching characters in blank string!","","CountInstances(string,char)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return iCount;
		}//end CountInstances(string,char,iStart,iEndEx)
		public void Clear() {
			iStart=Capacity/2;
			Length=0;
		}
		public RString Append(char cNow) {
			//bool bGood=false;
			//try {
				if (iEnder+1<=MaxCapacity) {
					carrData[iEnder]=cNow;
					iEnder++;
					//bGood=true;
				}
				else Console.Error.WriteLine("Tried to add '"+char.ToString(cNow)+"' but RString is full--must set Capacity");
			//}
			return this;
		}//end Append(char)
		public RString Append(string sNow) {
			//try {
			if (sNow!=null) {
				int iNewEnder=iEnder+sNow.Length;
				if (iNewEnder<=MaxCapacity) {
					int iRel=0;
					while (iEnder<iNewEnder) {
						carrData[iEnder]=sNow[iRel];//don't use indexer, use literal location
						iEnder++;
						iRel++;
					}
				}
				else Console.Error.WriteLine("Tried to add \""+sNow+"\" but RString is full--must set MaxCapacity");
			}
			else Console.Error.WriteLine("Tried to add null string to RString");
			//}
			return this;
		}//end Append(string)
		public RString Insert(int location, string sNow) {
			try {
				if (sNow!=null&&sNow.Length>0) {
					if ( (iStart-sNow.Length>=0&&location==0)
						  || (iEnder+sNow.Length<=Capacity) ) {
						if (location==0) {
							int iNew=iStart-sNow.Length;
							int iRel=0;
							while (iRel<sNow.Length) {
								carrData[iNew]=sNow[iRel];
								iNew++;
								iRel++;
							}
							iStart-=sNow.Length;
						}
						else if (location==Length) {
							int iNewEnder=iEnder+sNow.Length;
							int iRel=0;
							while (iEnder<iNewEnder) {
								carrData[iEnder]=sNow[iRel];
								iEnder++;
								iRel++;
							}
						}
						else {
							int iRel=sNow.Length-1;
							int iNewEnder=iEnder+sNow.Length;
							int iInsertStart=iStart+location;
							int iInsertEnder=iInsertStart+sNow.Length;
							int iOld=iEnder-1;
							for (int iNow=iNewEnder-1; iNow>=iInsertStart; iNow--) {
								if (iNow>=iInsertEnder) {//never should move to before iInsertStart ||iNow<iInsertStart) {
									carrData[iNow]=carrData[iOld];
									iOld--;
								}
								else {
									carrData[iNow]=sNow[iRel];
									iRel--;
								}
							}
							iEnder=iNewEnder;
						}//else not at one of the ends
					}
					else Console.Error.WriteLine("Tried to insert \""+sNow+"\" but RString is full--must set Capacity {"
						+"iStart:"+iStart.ToString()+";"
						+"iEnder:"+iEnder.ToString()+";"
						+"Length:"+Length.ToString()+";"
						+"MaxCapacity:"+MaxCapacity.ToString()+";"
						+"}");
				}
				else Console.Error.WriteLine("Error in RString Insert("+location.ToString()+",null): Tried to insert null string into RString");
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error in RString Insert(\""+sNow+"\") {"
					+"iStart:"+iStart.ToString()+";"
					+"iEnder:"+iEnder.ToString()+";"
					+"Length:"+Length.ToString()+";"
					+"MaxCapacity:"+MaxCapacity.ToString()+";"
					+"}");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return this;
		}//end Insert(string)
		public RString Insert(int location, char cNow) {
			try {
					if ( (iStart>0&&location==0)
						  || (iEnder<Capacity) ) {
						if (location==0) {
							iStart--;
							carrData[iStart]=cNow;
						}
						else if (location==Length) {
							carrData[iEnder]=cNow;
							iEnder++;
						}
						else {
							int iOld=iEnder;
							int iNew=iEnder+1;
							while (iNew>location) {
								carrData[iNew]=carrData[iOld];
								iOld--;
								iNew--;
							}
							carrData[location]=cNow;
							iEnder++;
						}
					}
					else Console.Error.WriteLine("Tried to insert '"+char.ToString(cNow)+"' but RString is full--must set Capacity {"
						+"iStart:"+iStart.ToString()+";"
						+"iEnder:"+iEnder.ToString()+";"
						+"Length:"+Length.ToString()+";"
						+"MaxCapacity:"+MaxCapacity.ToString()+";"
						+"}");
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error in RString Insert('"+char.ToString(cNow)+"') {"
					+"iStart:"+iStart.ToString()+";"
					+"iEnder:"+iEnder.ToString()+";"
					+"Length:"+Length.ToString()+";"
					+"MaxCapacity:"+MaxCapacity.ToString()+";"
					+"}");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return this;
		}//end Insert(char)
		public RString Remove(int location, int length) {
			if (location>=0) {
				if (location+length>Length) length-=(location+length)-Length;
					//--i.e. 1,2 when Length is 2 yields:  length-=(1+2)-2  ==  length-=1
					//--i.e. 0,2 when Length is 2 yields:  no change
				if (length>0) {
					if (location==0) { //if at beginning
						iStart+=length;
						//that's all since RString class is based on ender
					}
					if (location+length==Length) { //if at end
						iEnder-=length;
					}
					else {
						int iNew=location+iStart;
						int iOld=iNew+length;
						//int iOldLength=Length;
						//int iNewLength=iOldLength-length;
						//int iNew=iStart;
						//int iCutEnder=iOld;
						int iNewEnder=iEnder-length;
						while (iNew<iNewEnder) {
							carrData[iNew]=carrData[iOld];
							iNew++;
							iOld++;
						}
						iEnder=iNewEnder;
					}//end else is not at one of the ends
				}
			}
			else Console.Error.WriteLine("Tried to Remove "+length.ToString()+" characters at "+location.ToString()+" in  RString with Length of "+Length.ToString());
			return this;
		}//end Remove
		public bool MoveInternalStringToMiddle() {
			bool bGood=false;
			try {
				if (Capacity>0) {
					int iMaximize=(Length>(Capacity/2))?RMath.SafeAdd( RMath.SafeMultiply(Length,2), 1):Capacity;
					char[] carrOld=carrData;
					carrData=new char[iMaximize];
					int iDest=iMaximize/2;
					for (int iNow=iStart; iNow<iEnder; iNow++) {
						carrData[iDest]=carrOld[iNow];
						iDest++;
					}
					iEnder+=(iMaximize/2)-iStart;
					iStart=iMaximize/2;
					bGood=true;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Console.Error.WriteLine("RString MoveInternalStringToZero error:");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return bGood;
		}
		public static explicit operator string(RString val) {
			return val.ToString();
		}
		public static explicit operator RString(string val) {
			return new RString(val);
		}
		public static string operator +(RString var1, string var2) {
			return var1.ToString()+var2;
		}
		public static bool operator ==(RString a, RString b) {
			if (a==null||b==null) return false;
			return a.Equals(b);
		}
		public static bool operator !=(RString a, RString b) {
			if (a==null||b==null) return true;
			return !a.Equals(b);
		}
		public static bool CompareAt(RString fstr1, int iRel1, RString fstr2, int iRel2) {
			try {
				return (fstr1!=null&&fstr2!=null&&fstr1.Length>iRel1&&fstr2.Length>iRel2
					&&fstr1.carrData[fstr1.iStart+iRel1]==fstr2.carrData[fstr2.iStart+iRel2]);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"comparing character in RStrings (RString corruption)");
			}
			return false;
		}//end CompareAt(RString,int,RString,int)
		///<summary>
		///Fast comparison (works without actually modifying the Haystack param)
		///Returns true if match AND (iStopBeforeInHaystack-iAtHaystack)==Needle.Length
		///</summary>
		public static bool CompareAt(string Haystack, string Needle, int iAtHaystack, int iStopBeforeInHaystack) {
			if (Haystack!=null&&Needle!=null) {
				if ((iStopBeforeInHaystack-iAtHaystack)==Needle.Length) {
					int iRel=0;
					while (iAtHaystack<iStopBeforeInHaystack) {
						if (Needle[iRel]!=Haystack[iAtHaystack]) return false;
						iAtHaystack++;
						iRel++;
					}
					return true;
				}
				else return false;
			}
			else RReporting.Warning("Received null string","looking for string",String.Format("CompareAt({0},{1},{2},{3})",RReporting.StringMessage(Haystack,false),RReporting.StringMessage(Needle,false),iAtHaystack,iStopBeforeInHaystack) );
		}//end CompareAt(string,string,iStart,iEndEx)
		public static bool CompareAt(string Haystack, string Needle, int iAtHaystack) {
			bool bReturn=false;
			int iAbs=iAtHaystack;
			int iMatches=0;
			try {
				//if (Haystack!=null && Needle!=null) {
				if (Needle!=null&&Needle!="") {
					for (int iRel=0; iRel<Needle.Length&&iAbs<Haystack.Length; iRel++) {
						if (Needle[iRel]==Haystack[iAbs]) iMatches++;
						else break;
						iAbs++;
					}
					if (iMatches==Needle.Length) bReturn=true;
				}
				//}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"comparing text substring");
				bReturn=false;
			}
			return bReturn;
		}//end CompareAt(string,string,iAt)
		public static bool StartsWith(string Haystack, string Needle) {
			return (Haystack!=null && CompareAt(Haystack,Needle,0,Needle.Length));
		}
		public static bool StartsWithI(string Haystack, string Needle) {
			return (Haystack!=null && CompareAtI(Haystack,Needle,0));
		}
		public static bool StartsWithI_AssumingNeedleIsLower(string Haystack, string Needle) {
			return (Haystack!=null && CompareAtI_AssumingNeedleIsLower(Haystack,Needle,0));
		}
		///<summary>
		///Fast comparison (works without actually modifying the Haystack param)
		///Returns true if match AND (iStopBeforeInHaystack-iAtHaystack)==Needle.Length
		///</summary>
		public static bool CompareAtI_AssumingNeedleIsLower(string Haystack, string Needle, int iAtHaystack, int iStopBeforeInHaystack) {
			if (Haystack!=null&&Needle!=null) {
				if ((iStopBeforeInHaystack-iAtHaystack)==Needle.Length) {
					int iRel=0;
					while (iAtHaystack<iStopBeforeInHaystack) {
						if (Needle[iRel]!=
							( (Haystack[iAtHaystack]>=ui16UpperA&&Haystack[iAtHaystack]<=ui16UpperZ)
								?(Haystack[iAtHaystack]+ui16ToLower)
								:(Haystack[iAtHaystack]) )//forces inline ToLower(Haystack[iAtHaystack])
						) return false;
						iAtHaystack++;
						iRel++;
					}
					return true;
				}
				else return false;
			}
			else RReporting.Warning("Sent null string to CompareAtI","looking for string",String.Format("CompareAtI_AssumingNeedleIsLower({0},{1},{2},{3})",RReporting.StringMessage(Haystack,false),RReporting.StringMessage(Needle,false),iAtHaystack,iStopBeforeInHaystack) );
		}//end CompareAtI_AssumingNeedleIsLower(string,string,iStart,iEndEx)
		///<summary>
		///Case-sensitive overload of CompareAt that only can return true if
		/// match AND (iStopBeforeInHaystack-iAtHaystack)==Needle.Length
		///</summary>
		public static bool CompareAtI(string Haystack, string Needle, int iAtHaystack, int iStopBeforeInHaystack) {
			try {
				if (Haystack!=null&&IsNotBlank(Needle)) {
					if ((iStopBeforeInHaystack-iAtHaystack)==Needle.Length) {
						int iRel=0;
						while (iAtHaystack<iStopBeforeInHaystack) {
							if (//Needle[iRel]
								( (Needle[iRel]>=ui16UpperA&&Needle[iRel]<=ui16UpperZ)
									?(Needle[iRel]+ui16ToLower)
									:(Needle[iRel]) ) //forces inline ToLower(Needle[iRel])
								!=
								( (Haystack[iAtHaystack]>=ui16UpperA&&Haystack[iAtHaystack]<=ui16UpperZ)
									?(Haystack[iAtHaystack]+ui16ToLower)
									:(Haystack[iAtHaystack]) )//forces inline ToLower(Haystack[iAtHaystack])
							) return false;
							iAtHaystack++;
							iRel++;
						}
						return true;
					}
					else return false;
				}
				else RReporting.Warning("Sent null string to CompareAtI","looking for string",String.Format("CompareAtI_AssumingNeedleIsLower({0},{1},{2},{3})",RReporting.StringMessage(Haystack,false),RReporting.StringMessage(Needle,false),iAtHaystack,iStopBeforeInHaystack) );
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return false;
		}//end CompareAtI
		//public static bool CompareAt(char[] carrHaystack, char[] carrNeedle, int iAtHaystack) {
		//	bool bReturn=false;
		//	int iAbs=iAtHaystack;
		//	int iMatches=0;
		//	try {
		//		//if (carrHaystack!=null && carrNeedle!=null) {
		//		if (carrNeedle!=null) {
		//			for (int iRel=0; iRel<carrNeedle.Length; iRel++) {
		//				if (carrNeedle[iRel]==carrHaystack[iAbs]) iMatches++;
		//				else break;
		//				iAbs++;
		//			}
		//			if (iMatches==carrNeedle.Length) {
		//				bReturn=true;
		//			}
		//		}
		//		//}
		//	}
		//	catch (Exception exn) {
		//		RReporting.ShowExn(exn,"comparing text substring");
		//		bReturn=false;
		//	}
		//	return bReturn;
		//}//end CompareAt(char array...);
// 		public override bool Equals(RString fstrX) {
// 			try {
// 				if (fstrX!=null&&fstrX.Length==Length) {
// 					int iAbsThat=fstrX.iStart;
// 					for (int iAbsMe=iStart; iAbsMe<iEnder; iAbsMe++) {
// 						if (fstrX.carrData[iAbsThat]!=carrData[iAbsMe]) return false;
// 						iAbsThat++;
// 					}
// 					return true;
// 				}
// 			}
// 			catch (Exeption exn) {
// 				RReporting.ShowExn(exn,"comparing RStrings (RString corruption)","");
// 			}
// 			return false;
// 		}
		public static bool CompareAt(string Haystack, char Needle, int iAtHaystack) {
			return Haystack!=null&&iAtHaystack>=0&&iAtHaystack<Haystack.Length&&Haystack[iAtHaystack]==Needle;
		}

		public override bool Equals(object o) {
			try {
				return (bool) (this == (RString) o);
			}
			catch {
				return false;
			}
		}
		public override int GetHashCode() {
// 			int iHash=0;
// 			try {
// 				if (carrData!=null) {
// 					for (int iNow=iStart; iNow<iEnder; iNow++) {
// 						iHash=RMath.SafeAddWrappedTowardZero(iHash,(int)carrData[iNow]);
// 					}
// 				}
// 				else iHash=-1;
// 			}
// 			catch (Exception exn) {
// 				RReporting.ShowExn(exn);
// 				if (iHash==<1) iHash=-1;
// 				else iHash*=-1;
// 			}
// 			return iHash;
			return ToString().GetHashCode();
		}
		public override string ToString() {
			char[] carrReturn=RMemory.SubArray(carrData,iStart,Length);
			return new string(carrReturn);
		}
		public string ToString(int StartX, int LenX) {
			char[] carrReturn=null;
			if (StartX+LenX>Length) {
				RReporting.Warning( String.Format("Warning: ToString is truncating dest from Start:{0}; Length:{1} to fit actual RString size {2}",StartX,LenX,Length) );
				LenX=Length-StartX;
			}
			if (LenX>0) carrReturn=RMemory.SubArray(carrData,iStart+StartX,LenX);
			return carrReturn!=null?new string(carrReturn):"";
		}
		public void Trim(int StartX, int LenX) {
			if (StartX+LenX>Length) {
				RReporting.Warning( String.Format("Warning: Trim is truncating dest from Start:{0}; Length:{1} to fit actual RString size {2}",StartX,LenX,Length) );
				LenX=Length-StartX;
			}
			if (LenX>0) {
				iStart+=StartX;
				Length=LenX;
			}
			else Length=0;
		}//end Trim
		public static string SafeSubstring(string sValue, int start, int iLen) {
			if (sValue==null) return "";
			if (start<0) return "";
			if (iLen<1) return "";
			try {
				if (start<sValue.Length) {
					if ((start+iLen)<=sValue.Length) return sValue.Substring(start, iLen);
					else {
						RReporting.Debug("Tried to return SafeSubstring(\"" + sValue+"\"," + start.ToString() + "," + iLen.ToString() + ") (area ending past end of string).");
						return sValue.Substring(start);
					}
					   //it is okay that the "else" also handles (start+iLen)==sValue.Length
				}
				else {
					RReporting.Debug("Tried to return SafeSubstring(\""+sValue+"\","+start.ToString()+","+iLen.ToString()+") (starting past end).");
					return "";
				}
			}
			catch (Exception exn) {
				RReporting.Debug(exn);
				return "";
			}
		}//end SafeSubstring(string,int,int)
		public static string SafeSubstring(string sValue, int start) {
			bGoodString=false;
			if (sValue==null) return "";
			if (start<0) return ""; 
			try {
				if (start<sValue.Length) {
					try { sValue=sValue.Substring(start); bGoodString=true; }
					catch { bGoodString=false; sValue=""; }
					return sValue;
				}
				else {
					return "";
					RReporting.Debug("Tried to return SafeSubstring(\""+sValue+"\","+start.ToString()+") (past end).");
				}
			}
			catch (Exception exn) {
				RReporting.Debug(exn);
				return "";
			}
		}//end SafeSubstring(string,int,int)
		public static int IndexOf(string[] Haystack, string Needle) {
			return IndexOf(Haystack,Needle,RString.SafeLength(Haystack));
		}
		///<summary>
		///Returns index of Haystack where Needle occurs, otherwise -1 if not found
		/// or if Needle is blank.
		///</summary>
		public static int IndexOf(string[] Haystack, string Needle, int iHaystackStart, int iHaystackCount) {
			int iReturn=-1;
			if (Haystack!=null&&RString.IsNotBlank(Needle)) {
				for (int iNow=iHaystackStart; iNow<iHaystackCount; iNow++) {
					if (Haystack[iNow]==Needle) {
						iReturn=iNow;
						break;
					}
				}
			}
			return iReturn;
		}//end IndexOf(string[],string);
		public static int IndexOfI(string[] Haystack, string Needle) {
			return IndexOfI(Haystack,Needle,0,RString.SafeLength(Haystack));
		}
		public static int IndexOfI(string[] Haystack, string Needle, int iHaystackStart, int iHaystackCount) {
			int iReturn=-1;
			try {
				if (Haystack!=null&&RString.IsNotBlank(Needle)) {
					for (int iNow=iHaystackStart; iNow<iHaystackCount; iNow++) {
						if (EqualsI(Haystack[iNow],Needle)) {
							iReturn=iNow;
							break;
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return iReturn;
		}//end IndexOf(string[],string);
		public static int IndexOfStartsWithI(string[] Haystack, string Needle) {
			int iReturn=-1;
			if (Haystack!=null&&RString.IsNotBlank(Needle)) {
				for (int iNow=0; iNow<Haystack.Length; iNow++) {
					if (CompareAtI(Haystack[iNow],Needle,0)) {
						iReturn=iNow;
						break;
					}
				}
			}
			return iReturn;
		}//end IndexOfStartsWithI(string[],string);
		///NOTE: SafeSubstring(RString,...) is NOT needed, because RString.ToString is already safe
		public static bool Contains(string Haystack, string Needle) {
			try {
				return Haystack.IndexOf(Needle) >= 0;
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","Contains(string,string)");
				return false;
			}
		}
		public static bool Contains(string Haystack, char Needle) {
			try {
				return IndexOf(Haystack,0,Needle) >= 0;
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","Contains(string,char)");
				return false;
			}
		}
		public static bool Contains(string Haystack, string Needle, int CharIndexInNeedleToFind) {
			try {
				return IndexOf(Haystack,0,Needle,CharIndexInNeedleToFind) >= 0;
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","Contains(string,string,IndexOfCharacterInNeedleToFind)");
				return false;
			}
		}
		public static bool Contains(string[] Haystack, string Needle) {
			if (Haystack!=null) {
				for (int iNow=0; iNow<Haystack.Length; iNow++) {
					if (Haystack[iNow]==Needle) return true;
				}
			}
			return false;
		}
		public static bool ContainsI(string[] Haystack, string Needle) {
			if (Haystack!=null&&Needle!=null) {
				for (int iNow=0; iNow<Haystack.Length; iNow++) {
					if (Haystack[iNow]!=null && CompareAtI(Haystack[iNow],Needle,0,Haystack[iNow].Length)) return true;
				}
			}
			return false;
		}
		public static bool ContainsI_AssumingNeedleIsLower(string[] Haystack, string Needle) {
			if (Haystack!=null&&Needle!=null) {
				for (int iNow=0; iNow<Haystack.Length; iNow++) {
					if (Haystack[iNow]!=null && CompareAtI_AssumingNeedleIsLower(Haystack[iNow],Needle,0,Haystack[iNow].Length)) return true;
				}
			}
			return false;
		}
		public static bool AnyStartsWithI(string[] Haystack, string Needle) {
			if (Haystack!=null&&Needle!=null) {
				for (int iNow=0; iNow<Haystack.Length; iNow++) {
					if (StartsWithI(Haystack[iNow],Needle,0)) return true;
				}
			}
			return false;
		}
		public static bool AnyStartsWithI_AssumingNeedleIsLower(string[] Haystack, string Needle) {
			if (Haystack!=null&&Needle!=null) {
				for (int iNow=0; iNow<Haystack.Length; iNow++) {
					if (StartsWithI_AssumingNeedleIsLower(Haystack[iNow],Needle,0)) return true;
				}
			}
			return false;
		}
		public static bool AnyStartsWith(string[] Haystack, string Needle) {
			if (Haystack!=null&&Needle!=null) {
				for (int iNow=0; iNow<Haystack.Length; iNow++) {
					if (StartsWith(Haystack[iNow],Needle)) return true;
				}
			}
			return false;
		}
		public static bool Contains(char[] sNow, char cNow) {
			if (sNow!=null) {
				for (int iNow=0; iNow<sNow.Length; iNow++) {
					if (sNow[iNow]==cNow) return true;
				}
			}
			return false;
		}
#region moved from Base
		//public static bool IsSpacingExceptNewLine(char val) {
		//{}
		public static bool IsSpacingExceptNewLine(char val) {
			bool bYes=false;
			try {
				if (val==' ') bYes=true;
				else if (val=='\t') bYes=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"checking for spacing","IsSpacingExceptNewLine");
			}
			return bYes;
		}
		//public static readonly char[] carrNewLine=new char[] {'\n', '\r', Environment.NewLine[0], Environment.NewLine[Environment.NewLine.Length-1]};
		public static int IsNewLineAndGetLength(string sText, int iChar) {	
			if (sText!=null&&iChar>=0&&iChar<sText.Length) {
				if (CompareAt(sText,Environment.NewLine,iChar)) return Environment.NewLine.Length;
				else if (sText[iChar]=='\n') return 1;
				else if (sText[iChar]=='\r') return 1;
			}
			return 0;
		}
		public static bool IsNewLine(char val) {
			bool bYes=false;
			try {
				if (val=='\r') bYes=true;
				else if (val=='\n') bYes=true;
				else if (Contains(Environment.NewLine,val)) bYes=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"checking for newline","IsNewLine");
			}
			return bYes;
		}
		public static bool LikeWildCard(string input, string pattern, bool bCaseSensitive) { //IsLike
			return LikeWildCard(input, pattern, bCaseSensitive?RegexOptions.None:RegexOptions.IgnoreCase);
		}
		public static bool LikeWildCard(string input, string pattern, RegexOptions regexoptions) {//aka IsLike
			if (input==null) input="";
			if (pattern==null) pattern="";
			if (input==pattern) return true;
			if (input=="") return false;
			if (pattern=="") return false;
			try {
				return Regex.IsMatch(input, pattern, regexoptions);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
				return false;
			}
		}
		public static bool SafeCompare(string sValue, string Haystack, int iAtHaystackIndex) {
			bool bFound=false;
			try {
				if ( sValue==Haystack.Substring(iAtHaystackIndex, sValue.Length) ) {
					bFound=true;
				}
			}
			catch (Exception exn) {
				RReporting.Debug(exn);
				bFound=false;
			}
			return bFound;
		}
		public static string SafeRemove(string sValue, int iExcludeAt, int iExcludeLen) {
			return SafeSubstring(sValue,0,iExcludeAt)+RString.SafeSubstring(sValue,iExcludeAt+iExcludeLen);
		}
		public static string SafeRemoveExcludingEnder(string sVal, int iExcludeAt, int iExcludeEnder) {
			return SafeRemove(sVal,iExcludeAt,iExcludeEnder-iExcludeAt);
		}
		public static string SafeRemoveIncludingEnder(string sVal, int iExcludeAt, int iAlsoRemoveEnder) {
			return SafeRemoveExcludingEnder(sVal, iExcludeAt, iAlsoRemoveEnder+1);
		}
		public static string SafeInsert(string sValue, int iAt, string sInsert) {
			return RString.SafeSubstring(sValue,0,iAt)+((sInsert==null)?RString.SafeSubstring(sValue,iAt):(sInsert+RString.SafeSubstring(sValue,iAt)));
		}
		public static void ReplaceNewLinesWithSpaces(ref string sDataNow) {
			RString sCumulative=new RString(sDataNow.Length);
			sCumulative=(RString)"";
			if (sDataNow!=""&&sDataNow!=null) {
				int iCursor=0;
				int iCount=0;
				string sLine;
				while (ReadLine(out sLine, sDataNow, ref iCursor)) {
					if (iCount==0) sCumulative.Append(sLine);
					else {
						sCumulative.Append(" ");
						sCumulative.Append(sLine);
					}
				}
				if (sLine==""&&sCumulative.EndsWith(" ")) sCumulative.Trim(0,sCumulative.Length-1); //remove any trailing space that was ADDED ABOVE to a blank line.
			}
			if (!RReporting.IsNotBlank(sCumulative)&&RReporting.IsNotBlank(sDataNow)) RReporting.Debug("ReplaceNewLinesWithSpaces got empty string \""+sCumulative+"\" from used string \""+sDataNow+"\".");
			sDataNow=(string)sCumulative;
		}
		/// <summary>
		/// Gets the non-null equivalent of a null, empty, or nonempty string.
		/// </summary>
		/// <param name="val"></param>
		/// <returns>If Null Then return is "NULLSTRING"; if "" then return is "", otherwise
		/// val is returned.</returns>
// 		public static string SafeString(string val, bool bReturnWhyIfNotSafe) {
// 			try {
// 				return (val!=null)
// 					?val
// 					:(bReturnWhyIfNotSafe?"null-string":"");
// 			}
// 			catch {//do not report this
// 				return bReturnWhyIfNotSafe?"incorrectly-initialized-string":"";
// 			}
// 		}
		public static int ReplaceAll(ref string sData, string sFrom, string sTo) {
			int iReturn=0;
			try {
				if (sData.Length==0) {
					RReporting.ShowErr("There is no text in which to search for replacement.");
					//still returns true (0) though
				}
				else {
					int iPlace=sData.IndexOf(sFrom);
					int iReplaced=0;
					while (iPlace>-1) {
						sData=sData.Remove(iPlace,sFrom.Length);
						sData=sData.Insert(iPlace,sTo);
						if (iPlace>=0) iReplaced++;
						iReturn++;
						iPlace=sData.IndexOf(sFrom);
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"replacing & counting all string instances");
			}
			return iReturn;
		}//end ReplaceAll
		public static int ReplaceAll(string sFrom, string sTo, string[] sarrHaystack) {
			int iReturn=0;
			try {
				if (sarrHaystack!=null) {
					for (int iNow=0; iNow<sarrHaystack.Length; iNow++) {
						iReturn+=ReplaceAll(ref sarrHaystack[iNow], sFrom, sTo);
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"replacing & counting all string instances in all strings in string array");
			}
			return iReturn;
		}
		public static void RemoveSpacingBeforeNewLines(ref string sData) {
			int iFound=1;
			try {
				while (iFound>0) {
					iFound=0;
					iFound+=ReplaceAll(ref sData, sRemoveSpacingBeforeNewLines1, Environment.NewLine);
					iFound+=ReplaceAll(ref sData, sRemoveSpacingBeforeNewLines2, Environment.NewLine);
					RReporting.Debug("Removing "+iFound.ToString()+", "+iFound.ToString()+" total...");
				}
				RReporting.Debug("Done with "+iFound.ToString()+" total spacing before newlines removed.");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
		}
		public static void RemoveBlankLines(ref string sData) {
			RemoveBlankLines(ref sData, false);
		}
		public static void RemoveBlankLines(ref string sData, bool bAllowTrailingNewLines) {
			try {
				int iFoundNow=1;
				string sRemove=Environment.NewLine+Environment.NewLine;
				while (iFoundNow>0) {
					iFoundNow=0;
					iFoundNow+=ReplaceAll(ref sData, sRemove, Environment.NewLine);
					RReporting.Debug("Removing "+iFoundNow.ToString()+", "+iFoundNow.ToString()+" total...");
				}
				if (!bAllowTrailingNewLines) {
					while (sData.EndsWith(Environment.NewLine)) {
						sData=sData.Substring(0,sData.Length-Environment.NewLine.Length);
						iFoundNow++;
						RReporting.Debug("Removing "+iFoundNow.ToString()+", "+iFoundNow.ToString()+" total...removed trailing blank line...");
					}
				}
				RReporting.Debug("Done with "+iFoundNow.ToString()+" total blank lines removed.");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
		}//end RemoveBlankLines
		public static string AlphabeticalByNumber(int iStartingWithZeroAsA) {
			return AlphabeticalByNumber(iStartingWithZeroAsA, false);
		}
		/// <summary>
		/// Gets alphabetical character, starting with 0 as 'a', or 'A' if bUpperCase.
		/// </summary>
		/// <param name="bUpperCase">whether returned char should be uppercase</param>
		/// <param name="iStartingWithZeroAsA">alphabetical index from 0 to 25, otherwise return is a space</param>
		/// <returns>If within range of alphabet (i.e. 0 to 25),
		///  returns alphabetical character (uppercase if bUpperCase); otherwise
		///  returns a [space] character.</returns>
		public static string AlphabeticalByNumber(int iStartingWithZeroAsA, bool bUpperCase) {
			string sReturn=" ";
			if (iStartingWithZeroAsA>-1 && iStartingWithZeroAsA<carrAlphabetUpper.Length) 
				sReturn=char.ToString( (bUpperCase)?
				                      carrAlphabetUpper[iStartingWithZeroAsA]
				                      :carrAlphabetLower[iStartingWithZeroAsA] );
			return sReturn;
		}
		public static int IndexOf(string Haystack, char Needle) {
			return IndexOf(Haystack,Needle,0);
		}
		public static int IndexOf(string Haystack, char Needle, int start) {//formerly IndexInSubstring
			int iReturn=-1;
			if (Haystack!=null&&Haystack!=""&&start<Haystack.Length) {
				if (start<0) start=0;
				for (int iChar=start; iChar<Haystack.Length; iChar++) {
					if (Haystack[iChar]==Needle) {
						iReturn=iChar;
						break;
					}
				}
			}
			return iReturn;
		}
		public static int IndexOf(string Haystack, string Needle, int start, int IndexOfCharInNeedleToFind) {
			int iReturn=-1;
			if (Haystack!=null&&Haystack!=""&&start<Haystack.Length) {
				if (start<0) start=0;
				for (int iChar=start; iChar<Haystack.Length; iChar++) {
					if (Haystack[iChar]==Needle[IndexOfCharInNeedleToFind]) {
						iReturn=iChar;
						break;
					}
				}
			}
			return iReturn;
		}
		public static bool CompareSub(string Haystack, int iAtHaystack, string Needle) {
			int iMatches=0;
			if (RReporting.IsNotBlank(Haystack)&&RReporting.IsNotBlank(Needle)) {
				int iAbs=iAtHaystack;
				for (int iRel=0; iRel<Needle.Length&&iAbs<Haystack.Length; iRel++) {
					if (Haystack[iAbs]==Needle[iRel]) iMatches++;
					else return false;
					iAbs++;
				}
				return iMatches==Needle.Length;
			}
			else return false;
		}
		public static string[] StringToLines(string sVal) {
			string[] sarrReturn=null;
			if (sVal==null) sVal="";
			if (sVal=="") {
				sarrReturn=new string[1];
				sarrReturn[0]="";
			}
			else {
				//ArrayList alNow=new ArrayList();
				//ReplaceAll(ref sVal, Environment.NewLine,"\n");
				//ReplaceAll(ref sVal, "\r","");
				int iLines=1;
				int iTest=0;
				iTest=CountInstances(sVal,Environment.NewLine);
				if (iTest+1>iLines) iLines=iTest+1;
				iTest=CountInstances(sVal,"\n")+CountInstances(sVal,"\r");//this is ok for fault tolerance
				if (iTest+1>iLines) iLines=iTest+1;
				string[] sarrTemp=new string[iLines];//sarrReturn=new string[CountInstances(sVal,'\n')+1];
				int iLineX=0;
				int iStartNow=0;
				for (int iChar=0; iChar<=sVal.Length; iChar++) {
					if (iChar==sVal.Length) {
						sarrTemp[iLineX]=SafeSubstring(sVal,iStartNow,iChar-iStartNow);
						//alNow.Add(SafeSubstring(sVal,iStartNow,iChar-iStartNow));
						////iStartNow=iChar+1;
						iLineX++;
					}
					else if ( (iChar+1<sVal.Length) && (CompareSub(sVal,iChar,Environment.NewLine)) ) { //&&sVal.Substring(iChar, 2)==Environment.NewLine
						sarrTemp[iLineX]=SafeSubstring(sVal,iStartNow,iChar-iStartNow);
						iStartNow=iChar+Environment.NewLine.Length;
						iChar+=Environment.NewLine.Length-1;//this is right since the loop will add another one
						iLineX++;
					}
					else if (sVal[iChar]=='\r') {
						sarrTemp[iLineX]=SafeSubstring(sVal,iStartNow,iChar-iStartNow);
						//alNow.Add(SafeSubstring(sVal,iStartNow,iChar-iStartNow));
						iStartNow=iChar+1;
						iLineX++;
					}
					else if (sVal[iChar]=='\n') {
						sarrTemp[iLineX]=SafeSubstring(sVal,iStartNow,iChar-iStartNow);
						//alNow.Add(SafeSubstring(sVal,iStartNow,iChar-iStartNow));
						iStartNow=iChar+1;
						iLineX++;
					}
				}
				if (iLineX>0) {
					sarrReturn=new string[iLineX];
					for (int iNow=0; iNow<iLineX; iNow++) sarrReturn[iNow]=sarrTemp[iNow];
				}
			}//end else not blank
			return sarrReturn;
		}//end StringToLines
		/// <summary>
		/// Returns a list of values
		/// </summary>
		/// <param name="sField">Values Separated by mark</param>
		/// <param name="sMark">Mark that Separates Values</param>
		/// <returns></returns>
		public static string[] StringsFromMarkedList(string sField, string sMark) {
			string[] sarrNew=null;
			int index=0;
			try {
				if (sField!="") {
					while (sField.StartsWith(sMark)) {
						if (sField==sMark) {
							sField="";
							break;
						}
						else sField=sField.Substring(1);
					}
				}
				//if still !="", continue
				if (sField!="") {
					while (sField.EndsWith(sMark)) {
						if (sField==sMark) {
							sField="";
							break;
						}
						else sField=sField.Substring(0, sField.Length-1);
					}
				}
				//now continue as if we started here:
				if (sField!="") {
					int iMarks=CountInstances(sField, sMark);
					if (iMarks>0) {
						sarrNew=new string[iMarks];
						int iMark=-1;
						int iMarksNow=iMarks;
						while (iMarksNow>0) {
							iMark=sField.IndexOf(sMark);
							sarrNew[index]=sField.Substring(0,iMark);
							index++;
							sField=sField.Substring(iMark+1);
							iMarksNow--;
						}
						sarrNew[index]=sField;
						index++;//not used after this though
					}
					else {
						sarrNew=new string[1];
						sarrNew[0]=sField;
					}
				}
				else {
					sarrNew=new string[1];
					sarrNew[0]=sField;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
				sarrNew=new string[1];
				sarrNew[0]=sField;
			}
			return sarrNew;
		}
		public static bool SelectTo(out int iSelLen, string sAllText, int iFirstChar, int iLastCharInclusive) {
			iSelLen=0;
			bool bGood=false;
			try {
				if (iFirstChar<0) {
					RReporting.ShowErr("Tried to select beyond beginning of file, so selecting nothing instead", "", String.Format("SelectTo(out Length,{0},{1},{2})",
						RReporting.DebugStyle("data:",sAllText,false,false),iFirstChar,iLastCharInclusive) );
					iSelLen=0;
				}
				else if (iFirstChar>=sAllText.Length) {
					throw new ApplicationException("Selection start was past end of data {iFirstChar:"+iFirstChar.ToString()+"; Length:"+sAllText.Length.ToString()+"}");
				}
				else {
					if (iLastCharInclusive<iFirstChar) {
						iSelLen=0;
						throw new ApplicationException("Target is before start of " +
							"selection, so defaulting to iSelLen=0");
					}
					else if (iLastCharInclusive==iFirstChar) iSelLen=0;
					else {
						if (iLastCharInclusive>=sAllText.Length) {
							RReporting.ShowErr("Tried to select beyond end of file, from "
								+iFirstChar+" to "+iLastCharInclusive
								+", so selecting to end by default instead.","SelectTo");
							iLastCharInclusive=sAllText.Length-1;
						}
						iSelLen=(iLastCharInclusive-iFirstChar)+1;
					}
					bGood=true;
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"selecting text",
					String.Format("SelectTo(iSelLen:{0}, sAllText:{1}, iFirstChar:{2}, iLastCharInclusive:{3}) ",
							iSelLen,RReporting.StringMessage(sAllText,false),iFirstChar,iLastCharInclusive )
				);
			}
			return bGood;
		}
		public static bool MoveToOrStayAtSpacingOrString(ref int iMoveMe, string sData, string sFindIfBeforeSpacing) {
			bool bGood=true;
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (RString.IsSpacing(sData[iMoveMe]))
						break;
					else if (sData.Substring(iMoveMe,sFindIfBeforeSpacing.Length)==sFindIfBeforeSpacing)
						break;
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) Console.Error.WriteLine("Reached end of page (MoveToOrStayAtSpacingOrString).");
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn);
			}
			return bGood;
		}
		public static bool MoveBackToOrStayAt(ref int iMoveMe, string sData, string sFind) {
			bool bGood=false;
			try {
				int iEOF=-1;
				while (iMoveMe>iEOF) {
					if (sData.Substring(iMoveMe,sFind.Length)==sFind) {
						bGood=true;
						break;
					}
					else iMoveMe--;
				}
				if (iMoveMe<=iEOF) {
					RReporting.ShowErr("Reached beginning of string.","MoveBackToOrStayAt sFind");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
				bGood=false;
			}
			return bGood;
		}
		public static bool MoveToOrStayAt(ref int iMoveMe, string sData, string sFind) {
			bool bGood=false;
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (sData.Substring(iMoveMe,sFind.Length)==sFind) {
						bGood=true;
						break;
					}
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) {
					RReporting.Warning("Reached end of page (MoveBackToOrStayAt).");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"searching for text","MoveToOrStayAt(cursor,data,string)");
				bGood=false;
			}
			return bGood;
		}
		public static bool MoveToOrStayAtSpacing(ref int iMoveMe, string sData) {
			bool bGood=false;
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (RString.IsSpacing(sData,iMoveMe)) {
						bGood=true;
						break;
					}
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) RReporting.Warning("Reached end of string (MoveToOrStayAtSpacing).");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"searching for text","MoveToOrStayAtSpacing");
			}
			return bGood;
		}
		public static bool PreviousLineBreakerExceptNewLine(string sText, ref int iReturnBreaker_ElseNeg1, out bool bVisibleBreaker) {
			bool bGood=true;
			bVisibleBreaker=false;
			try {
				while (iReturnBreaker_ElseNeg1>-1) {
					if (IsInvisibleLineBreaker_NoSafeChecks(sText[iReturnBreaker_ElseNeg1])) {
						bVisibleBreaker=false;
						return true;//bGood=true;
					}
					iReturnBreaker_ElseNeg1--;
				}
				while (iReturnBreaker_ElseNeg1>-1) {
					if (IsVisibleLineBreaker_Unsafe(sText[iReturnBreaker_ElseNeg1])) {
						bVisibleBreaker=true;
						return true;//bGood=true;
					}
					iReturnBreaker_ElseNeg1--;
				}
			}
			catch (Exception exn) {	
				bGood=false;
				RReporting.ShowExn(exn,"looking for previous non-newline break",
					String.Format("PreviousLineBreakerExceptNewLine(sText:{0},iReturnBreaker_ElseNeg1:{1})",
					RReporting.StringMessage(sText,false),iReturnBreaker_ElseNeg1)
				);
			}
			return bGood;
		}
		private static bool IsVisibleLineBreaker_Unsafe(char val) {
			int iNow;
			for (iNow=0; iNow<carrLineBreakerVisible.Length; iNow++) {
				if (carrLineBreakerVisible[iNow]==val) {
					return true;
				}
			}
			return false;
		}
		private static bool IsInvisibleLineBreaker_NoSafeChecks(char val) {
			int iNow;
			for (iNow=0; iNow<carrLineBreakerInvisible.Length; iNow++) {
				if (carrLineBreakerInvisible[iNow]==val) {
					return true;
				}
			}
			return false;
		}
		public static bool IsLineBreakerExceptNewLine(char val) {
			int iNow;
			for (iNow=0; iNow<carrLineBreakerInvisible.Length; iNow++) {
				if (carrLineBreakerInvisible[iNow]==val) {
					return true;
				}
			}
			for (iNow=0; iNow<carrLineBreakerVisible.Length; iNow++) {
				if (carrLineBreakerVisible[iNow]==val) {
					return true;
				}
			}
			return false;
		}
		public static bool IsLineBreaker(char val) {
			return IsLineBreakerExceptNewLine(val)||IsNewLine(val);
		}
		public static bool IsLineBreaker(string val, int iChar) {
			return iChar>=0&&val!=null&&iChar<val.Length&&IsLineBreaker(val[iChar]);
		}
		public static bool IsLineBreakerExceptNewLine(string val, int iChar) {
			return iChar>=0&&val!=null&&iChar<val.Length&&IsLineBreakerExceptNewLine(val[iChar]);
		}
		public static bool IsSpacing(string val, int iChar) {
			return (IsSpacingExceptNewLine(val[iChar])||IsNewLine(val[iChar]));
		}
		public static bool IsSpacing(char val) {
			return (IsSpacingExceptNewLine(val)||IsNewLine(val));
		}
		public static bool RemoveEndsSpacingExceptNewLine(ref string val) {
			try {
				if (val==null) val="";
				int iStart=0;
				int iEnder=val.Length-1;
				int iLength=val.Length;
				while (iLength>0&&(val[iStart]=='\t'||val[iStart]==' ')) {iStart++;iLength--;}
				while (iLength>0&&(val[iEnder]=='\t'||val[iEnder]==' ')) {iEnder--;iLength--;}
				if (iLength!=val.Length) val=SafeSubstring(val,iStart,iLength);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
				return false;
			}
			return true;
// 			try {
// 				if (val==null) val="";
// 				while (val.Length>0&&val.StartsWith(" ")) val=val.Substring(0,val.Length-1);
// 				while (val.Length>0&&val.EndsWith(" ")) val=val.Substring(0,val.Length-1);
// 				while (val.Length>0&&val.StartsWith("\t")) val=val.Substring(0,val.Length-1);
// 				while (val.Length>0&&val.EndsWith("\t")) val=val.Substring(0,val.Length-1);
// 			}
// 			catch (Exception exn) {
// 				RReporting.ShowExn(exn);
// 				return false;
// 			}
// 			return true;
		}
		public static bool RemoveEndsNewLines(ref string val) {
			try {
				if (val==null) val="";
				int iStart=0;
				int iEnder=val.Length-1;
				int iLength=val.Length;
				int iNewLine=Environment.NewLine.Length;
				while (iLength>0&&CompareAt(val, Environment.NewLine, iStart)) {iStart+=iNewLine; iLength-=iNewLine;}
				while (iLength>0&&CompareAt(val, Environment.NewLine, iEnder-(iNewLine-1))) {iEnder-=iNewLine; iLength-=iNewLine;}
				while (iLength>0&&(val[iStart]=='\n'||val[iStart]=='\r')) {iStart++;iLength--;}
				while (iLength>0&&(val[iEnder]=='\n'||val[iEnder]=='\r')) {iEnder--;iLength--;}
				if (iLength!=val.Length) val=SafeSubstring(val,iStart,iLength);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
				return false;
			}
			return true;
		}
		public static bool RemoveEndsSpacing(ref string val) {
			return RemoveEndsNewLines(ref val) && RemoveEndsSpacingExceptNewLine(ref val);
		}
		/// <summary>
		/// Counts lines assuming Environment.NewLine is used; includes
		/// blank lines at end (i.e. even if last line ends with newline)
		/// </summary>
		/// <param name="sAllData">data in which to search</param>
		/// <returns>how many lines</returns>
		public static int LineCount(string sAllData) {
			return CountInstances(sAllData, Environment.NewLine)+1;
		}
		public static int SafeIndexOf(string Haystack, string Needle) {
			return SafeIndexOf(Haystack, Needle, 0);
		}
		public static int SafeIndexOf(string Haystack, string Needle, int start) {
			int iReturn= (Haystack==null || Haystack.Length<1 || Needle==null || Needle.Length<1) ?
				-1
				:((start>0)?SafeSubstring(Haystack,start):Haystack).IndexOf(Needle);
			if (iReturn>-1) iReturn+=start;
			return iReturn;
		}
		public static bool SafeCompare(string[] sarrMatchAny, string Haystack, int iHaystack) {
			bool bFound=false;
			try {
				int iNow=0;
				while (iNow<sarrMatchAny.Length) {
					if (SafeCompare(sarrMatchAny[iNow], Haystack, iHaystack)) {
						bFound=true;
						break;
					}
				}
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","SafeCompare(sarrMatchAny...)");//do not report this, just say "no"
				bFound=false;
			}
			return bFound;
		}
		public static bool MoveToOrStayAtAttrib(ref int indexToGet, uint[] dwarrAttribToSearch, uint bitsToFindAnyOfThem) {
			bool bFound=false;
			try {
				while (indexToGet<dwarrAttribToSearch.Length) {
					if ((dwarrAttribToSearch[indexToGet]&bitsToFindAnyOfThem)!=0) {
						bFound=true;
						break;
					}
					indexToGet++;
				}
			}
			catch (Exception exn) {
				bFound=false;
				RReporting.ShowExn(exn);
			}
			return bFound;
		}//end MoveToOrStayAtAttrib
		public static string PrecedeByIfNotBlank(string sStringToPrecedeValue_IfValueNotBlank, string sValue) {
			if (sValue!=null&&sValue!="") return sStringToPrecedeValue_IfValueNotBlank+sValue;
			else return "";
		}
		public static string ElipsisIfOver(string sDataOriginal, int iMaxLength) {
			try {
				if (iMaxLength>=3) {
					if (sDataOriginal.Length>iMaxLength) return SafeSubstring(sDataOriginal,0,iMaxLength-3)+"...";
					else return sDataOriginal;
				}
				else {
					if (sDataOriginal.Length>iMaxLength) return SafeSubstring(sDataOriginal,0,iMaxLength-1)+"~";
					else return sDataOriginal;
				}
			}
			catch {
				return "";
			}
		}
		public static string Repeat(string val, int iCount) {
			string sReturn="";
			if (val!=null) {
				for (int iNow=0; iNow<iCount; iNow++) {
					sReturn+=val;
				}
			}
			return sReturn;
		}
		public static string FixedWidth(int val, int iLength) {
			return FixedWidth(val.ToString(),iLength);
		}
		
		public static string FixedWidth(string val, int iLength) {
			return FixedWidth(val,iLength," ");
		}
		public static string FixedWidth(int val, int iLength, string sFillerChar) {
			return FixedWidth(val,iLength,sFillerChar);
		}
		public static string FixedWidth(string val, int iLength, string sFillerChar) {
			try {
				if (sFillerChar.Length<1) sFillerChar=" ";
				else if (sFillerChar.Length>1) sFillerChar=sFillerChar.Substring(0,1);
				if (val.Length>iLength) return ElipsisIfOver(val,iLength);
				else return Repeat(sFillerChar,iLength-val.Length)+val;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"setting fixed width string",
					String.Format("FixedWidth(val:{0},iLength:{1},sFillerChar{2})",
						RReporting.StringMessage(val,false),iLength,RReporting.StringMessage(sFillerChar,false)
					)
				);
			}
			return Repeat("~",iLength);
		}
		public static bool StringToFile(string sFile, string sAllDataX) {
			StreamWriter swX=null;
			bool bGood=false;
			//string sLine;//TODO:? implement this, ensuring newline chars are correct?
			try {
				swX=new StreamWriter(sFile);
				swX.Write(sAllDataX);
				swX.Close();
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"saving text string to file",
						String.Format("StringToFile({0},{1})",
						RReporting.StringMessage(sFile,true),RReporting.StringMessage(sAllDataX,false) )
				);
				bGood=false;
			}
			return bGood;
		}//end StringToFile
		/*public static void AppendForeverWriteLine(string sFileNow) {
			AppendForeverWrite(sFileNow,Environment.NewLine);
		}
		public static void AppendForeverWriteLine(string sFileNow, string sMsg) {
			if (sMsg==null) sMsg="";
			AppendForeverWrite(sFileNow,sMsg+Environment.NewLine);
		}
		public static void AppendForeverWrite(string sFileNow, string sMsg) {
			StreamWriter swNow;
			try {
				//iOutputs++;
				//if (iOutputs<iMaxOutputs) {
					swNow=File.AppendText(sFileNow);
					swNow.Write(sMsg);
					swNow.Close();
				//}
				//else if (iOutputs==iMaxOutputs) {
				//	swNow=File.AppendText(sFileNow);
				//	swNow.Write(Marker+"MAXIMUM MESSAGES REACHED--This is the last message that will be shown: "+sMsg);
				//	swNow.Close();
				//}
			}//end AppendForeverWrite(sFile,sMsg)
			catch (Exception exn) {
				try {
					RReporting.Debug(exn,"Base AppendForeverWrite","trying to append output text file");
					if (!File.Exists(sFileNow)) {
						swNow=File.CreateText(sFileNow);
						swNow.Write(sMsg);
						swNow.Close();
					}
				}
				catch (Exception exn2) {
					RReporting.Debug(exn2,"Base AppendForeverWrite","trying to create new output text file");//ignore since "error error"
				}
			}
		}*/ //end AppendForeverWrite
		public static bool StringWriteLine(ref string sToModify) {
			if (sToModify==null) sToModify="";
			sToModify+=Environment.NewLine;
			return true;
		}
		public static bool StringWriteLine(ref string sToModify, string sDataChunk) {
			if (sToModify==null) sToModify="";
			if (sDataChunk==null) sDataChunk="";
			sToModify+=sDataChunk+Environment.NewLine;
			return true;
		}
		public static bool StringWrite(ref string sToModify, string sDataChunk) {
			if (sToModify==null) sToModify="";
			if (sDataChunk==null) sDataChunk="";
			sToModify+=sDataChunk;
			return true;
		}
		public static void SplitFileName(out string sFirstPart, out string sExtension, string sFileName) {
			try {
				//if (File.Exists(sFileName)) {
					
				//}
				//else {
					int iDot=sFileName.LastIndexOf(".");
					if (iDot<0) {
						sFirstPart=sFileName;
						sExtension="";
					}
					else {
						sFirstPart=SafeSubstring(sFileName,0,iDot);
						sExtension=SafeSubstring(sFileName,iDot+1);
					}
				//}
			}
			catch {
				sFirstPart=sFileName;
				sExtension="";
			}
		}//end SplitFileName
		public static void ShrinkByRef(ref string sToTruncate, int iBy) {
			try {
				if (iBy>=sToTruncate.Length) sToTruncate="";
				else sToTruncate=sToTruncate.Substring(0,sToTruncate.Length-iBy);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
				sToTruncate="";
			}
		}
		public static string ShrinkBy(string sToTruncate, int iBy) {
			string sReturn=sToTruncate;
			ShrinkByRef(ref sReturn,iBy);
			return sReturn;
		}
		///<summary>
		///Modifies start and endbefore so that there are no quotes (if sData[start] and
		/// sData[endbefore-1] are both '"' characters
		///start: the first character in sData
		///endbefore: the exclusive ender in sData (the character before this is the last one)
		///</summary>		
		public static bool ShrinkToInsideQuotes(string sData, ref int start, ref int endbefore) {
			bool bGood=false;
			try {//if (start<0) start=0; if (endbefore>sData.Length) endbefore=sData.Length;
				if ( ((endbefore-start)>=2) && sOpener[start]=='"' && sOpener[endbefore-1]=='"' ) {
					start++;
					endbefore--;
				}
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return bGood;
		}//end ShrinkToInsideQuotes
		public static string SafeSubstringByExclusiveEnder(string sVal, int start, int endbefore) { //formerly SafeSubstringExcludingEnder
			return RString.SafeSubstring(sVal, start, (endbefore-start));
		}
		public static string SafeSubstringByInclusiveEnder(string sVal, int start, int endinclusive) {//formerly SafeSubstringByInclusiveLocations
			return SafeSubstringByExclusiveEnder(sVal,start,endinclusive+1);
		}
		public static string[] SplitScopes(string sField, char cBottomLevelScopeSeparator) {
			string sSyntaxErr=null;
			string[] sarrReturn=SplitScopes(sField, cBottomLevelScopeSeparator, false, out sSyntaxErr);
			if (sSyntaxErr!=null) RReporting.SourceErr(sSyntaxErr);
			return sarrReturn;
		}
		public static string[] SplitScopes(string sField, char cBottomLevelScopeSeparator, bool bIncludeTrailingCommaOrSemicolon, out string sSyntaxErr) {
			return SplitScopes(sField,cBottomLevelScopeSeparator,bIncludeTrailingCommaOrSemicolon,out sSyntaxErr, 0, RString.SafeLength(sField));
		}
		public static string[] SplitScopes(string sField, char cBottomLevelScopeSeparator, int start, int endbefore) {
			string sSyntaxErr=null;
			string[] sarrReturn=SplitScopes(sField, cBottomLevelScopeSeparator, false, out sSyntaxErr, start, endbefore);
			if (sSyntaxErr!=null) RReporting.SourceErr(sSyntaxErr);
			return sarrReturn;
		}

		public static int SplitParams(ref string[] sarrName, ref string[] sarrValue, string sField, char cAssignmentOperator, char cAssignmentDelimiter) {
			if (sField!=null) return SplitParams(ref sarrName, ref sarrValue, sField, cAssignmentOperator, cAssignmentDelimiter, 0, sField.Length);
			else return 0;
		}
		private static string[] sarrStatementsTemp=new string[80];
		///<summary>
		///Calls SplitScopes for splitting a delimited list of assignments and only 
		/// creates/enlarges arrays if necessary
		///</summary>
		public static int SplitParams(ref string[] sarrName, ref string[] sarrValue, string sField, char cAssignmentOperator, char cAssignmentDelimiter, int start, int endbefore) {
			string sSyntaxErr=null;
			int iTest=SplitScopes(ref sarrStatementsTemp,sField,cAssignmentDelimiter,false, out sSyntaxErr,start,endbefore);
			if (sarrStatements!=null&&sarrStatements.Length>0) {
				if (sarrName==null||sarrName.Length<sarrStatements.Length) sarrName=new string[sarrStatements.Length];
				if (sarrValue==null||sarrValue.Length<sarrStatements.Length) sarrValue=new string[sarrStatements.Length];
				
			}
		}//end SplitParams
		///<summary>
		///cBottomLevelScopeSeparator should be ';' if resolving scopes, and ',' if resolving functions,
		/// multiple inline declarations, or multiple incrementors in for-loops.
		///sField should NOT include the top-level braces i.e. "1,true" not "(1,true)" nor "{1,2}"
		/// otherwise returns a one-string-long array.
		///sSyntaxErr returns null if ok, otherwise returns semicolon-separated error messages.
		/// sarrStatementsOut: values are placed in this array--it is only redimensioned as 
		/// necessary, and size may not match return count.
		///Returns count of elements in sarrStatementsOut that are actually used--the rest should
		/// be ignored.
		///</summary>
		public static int SplitScopes(ref string[] sarrStatementsOut, string sField, char cBottomLevelScopeSeparator, bool bIncludeTrailingCommaOrSemicolon, out string sSyntaxErr, int start, int endbefore) {
			int iCount=0;
			sSyntaxErr=null;
			CharStack csScope=null;
			bool bInQuotes=false;
			int iStartNow=0;
			bool bInSingleQuotes=false;
			if (sField!=null&&sField!="") {
				if (sarrStatementsOut==null) Redim(ref sarrStatementsOut, 25, "SplitScopes");
				csScope=new CharStack();
				for (int iNow=start; iNow<=endbefore; iNow++) {
					if (iNow==endbefore || (iNow<endbefore&&(sField[iNow]==cBottomLevelScopeSeparator)&&csScope.Count==0&&!bInQuotes&&!bInSingleQuotes)) {
						sarrStatementsOut[iCount]=sField.Substring(iStartNow,iNow-iStartNow+(bIncludeTrailingCommaOrSemicolon?1:0));
						iCount++;
						if (iCount>=sarrStatementsOut.Length) Redim(ref sarrStatementsOut, sarrStatementsOut.Length+sarrStatementsOut.Length/2+1, "SplitScopes");
						iStartNow=iNow+1;
					}
					else if (!bInSingleQuotes&&sField[iNow]=='"'&&(iNow==0||sField[iNow-1]!='\\')) {
						bInQuotes=!bInQuotes;
					}
					else if (!bInQuotes&&sField[iNow]=='\''&&(iNow==0||sField[iNow-1]!='\\')) {
						bInSingleQuotes=!bInSingleQuotes;
					}
					else if ((sField[iNow]=='{'||sField[iNow]=='(')&&!bInQuotes&&!bInSingleQuotes) {
						csScope.Push(sField[iNow]);
					}
					else if ((sField[iNow]=='}'||sField[iNow]==')')&&!bInQuotes&&!bInSingleQuotes) {
						if (csScope.PeekTop()==sField[iNow]) {
							csScope.Pop();
						}
						else {
							sSyntaxErr=(sSyntaxErr!=null?(sSyntaxErr+"; "):"")+"Expected '"+char.ToString(csScope.PeekTop())+"' {position:"+iNow+";character:"+char.ToString(sField[iNow])+"}";
						}
					}
				}//end for character iNow
				if (sSyntaxErr!=null) RReporting.ShowErr(sSyntaxErr,"parsing code","SplitScopes");
			}//end if not blank
			return iCount;
		}//end SplitScopes
		/// <summary>
		/// this is actually just a SplitCSV function that also accounts for sStarter and sEnder,
		/// and gets the location of the content of column at index
		/// --for string notation such as: sField="{{1,2,3},a,b,"Hello, There",c}"
		/// (where: sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
		/// --index 4 would return "c"
		/// </summary>
		/// <returns>whether subsection index was found</returns>
		public static string TextSubArray(string sField, int index) {
			int iStart;
			int iLen;
			bool bTest=SubSection(out iStart, out iLen, sField, 0, endbefore, "{", "}", index);
			return RString.SafeSubstring(sField, iStart, iLen);
		}
		/// <summary>
		/// this is actually just a SplitCSV function that also accounts for sStarter and sEnder,
		/// and gets the location of the content of column at index
		/// --for string notation such as: sField="{{1,2,3},a,b,"Hello, There",c}"
		/// (where: sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
		/// --index 4 would return integers such that sField.Substring(iReturnStart,iReturnLen)=="c"
		/// </summary>
		/// <returns>whether subsection index was found</returns>
		public static bool TextSubArray(out int iReturnStart, out int iReturnLen, string sField, int index) {
			bool bTest=SubSection(out iReturnStart, out iReturnLen, sField, 0, RString.SafeLength(sField), "{", "}", index);
			return bTest;
		}
		public static bool SubSection(out int iReturnStart, out int iReturnLen, string sData, int start, int iLen, string sStarter, string sEnder, int index) { //debug does NOT allow escape characters!
			return SubSection(out iReturnStart, out iReturnLen, sData, start, iLen, sStarter, sEnder, index, '"', ',');
		}
		/// <summary>
		/// this is actually just a SplitCSV function that also accounts for sStarter and sEnder,
		/// and gets the location of the content of column at index
		/// --for string notation such as: sData="{{1,2,3},a,b,"Hello, There",c}"
		/// (where: sStarter="{"; sEnder="}"; sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
		/// --index 4 would yield integers such that sData.Substring(iReturnStart,iReturnLen)=="c"
		/// </summary>
		/// <returns>whether subsection index was found</returns>
		public static bool SubSection(out int iReturnStart, out int iReturnLen, string sData, int start, int iLen, string sStarter, string sEnder, int index, char cTextDelimiterNow, char cFieldDelimiterNow) { //debug does NOT allow escape characters!
			bool bFound=false;
			int iAbs=start;
			bool bInQuotes=false;
			int indexNow=0;
			int iDepth=0;
			int iStartNow=start;//only changed to the location after a zero-iDepth comma
			iReturnStart=0;
			iReturnLen=0;
			try {
				for (int iRel=0; iRel<=iLen; iRel++) { //necessary special case iRel==iLen IS safe below
					if (  iRel==iLen  ||  (bAllowNewLineInQuotes?(!bInQuotes&&CompareAt(sData,Environment.NewLine,iAbs)) :CompareAt(sData,Environment.NewLine,iAbs))  ) { //if non-text NewLine
						if (index==indexNow) {
							bFound=true;
						}
						else {
							RReporting.ShowErr("Text field subsection was not found on the same line","", "Base SubSection{column-index:"+indexNow.ToString()+"}");
						}
						break;
					}
					else if (sData[iAbs]==cTextDelimiterNow) { //text delimiter
						bInQuotes=!bInQuotes;
					}
					else if ((!bInQuotes) && iDepth==0 && (sData[iAbs]==cFieldDelimiterNow)) { //end field delimiter zero-level only
						if (indexNow==index) {
							bFound=true;
							break;
						}
						else {
							iStartNow=iAbs+1; //i.e. still works if sStarter is not found next
							indexNow++;
						}
					}
					else if ((!bInQuotes) && CompareAt(sData,sStarter,iAbs)) { //opening bracket
						iDepth++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sEnder,iAbs)) { //closing bracket
						iDepth--;
					}
					if (!bFound) iAbs++;
					//TODO: account for starter & ender length here (increment past)???
				}//end for iRel<iLen
				if (bFound) {
					iReturnStart=iStartNow;
					iReturnLen=iAbs-iStartNow;//iLen=iLenNow;//iLenNow=iAbs-iStartNow;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			if (!bFound) {
				iReturnStart=iStartNow;
				iReturnLen=0;
			}
			return bFound;
		}//end Base SubSection
		public static int SubSections(string sData, int start, int iLen, string sStarter, string sEnder) {
			return SubSections(sData, start, iLen, sStarter, sEnder, '"', ',');
		}
		/// <summary>
		/// Uses SplitCSV logic that also accounts for sStarter and sEnder to
		/// get the count of the zero-level array
		/// --for string notation such as: sData="{1,2,3},a,b,"Hello, There",c"
		/// (where: sStarter="{"; sEnder="}"; sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
		/// --would yield 5
		/// </summary>
		/// <returns>number of indeces in sData</returns>
		public static int SubSections(string sData, int start, int iLen, string sStarter, string sEnder, char cTextDelimiterNow, char cFieldDelimiterNow) { //debug does NOT allow escape characters!
			int iFound=0;
			int iAbs=start;
			bool bInQuotes=false;
			int iDepth=0;
			int iStartNow=start;//only changed to the location after a zero-iDepth comma
			try {
				for (int iRel=0; iRel<=iLen; iRel++) { //necessary special case iRel==iLen IS safe below
					if (  iRel==iLen  ||  (bAllowNewLineInQuotes?(!bInQuotes&&CompareAt(sData,Environment.NewLine,iAbs)) :CompareAt(sData,Environment.NewLine,iAbs))  ) { //if non-text NewLine
						iFound++;
						break;
					}
					else if (sData[iAbs]==cTextDelimiterNow) { //text delimiter
						bInQuotes=!bInQuotes;
					}
					else if ((!bInQuotes) && iDepth==0 && (sData[iAbs]==cFieldDelimiterNow)) { //end field delimiter zero-level only
						iFound++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sStarter,iAbs)) { //opening bracket
						iDepth++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sEnder,iAbs)) { //closing bracket
						iDepth--;
					}
					iAbs++;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return iFound;
		}//end Base SubSections
		public static string DateTimePathString(bool bIncludeMilliseconds) {
			return DateTimeString(bIncludeMilliseconds,"_","_at_",".");
		}
		public static string DateTimeString(bool bIncludeMilliseconds, string sDateDelimiter, string sDateTimeSep, string sTimeDelimiter) {
			string sReturn;
			try {
				System.DateTime dtX;
				dtX=DateTime.Now;
				sReturn=dtX.Year+sDateDelimiter;
				if (dtX.Month<10) sReturn+="0";
				sReturn+=dtX.Month.ToString()+sDateDelimiter;
				if (dtX.Day<10) sReturn+="0";
				sReturn+=dtX.Day.ToString()+sDateTimeSep;
				if (dtX.Hour<10) sReturn+="0";
				sReturn+=dtX.Hour.ToString()+sTimeDelimiter;
				if (dtX.Minute<10)sReturn+="0";
				sReturn+=dtX.Minute.ToString()+sTimeDelimiter;
				if (dtX.Second<10)sReturn+="0";
				sReturn+=dtX.Second.ToString()+"s"+sTimeDelimiter;
				if (bIncludeMilliseconds) {
					int iMilNow=dtX.Millisecond;
					if (iMilNow<10) sReturn+="0";
					if (iMilNow<100) sReturn+="0";
					sReturn+=iMilNow.ToString()+"ms";
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting current date & time");
				sReturn="UnknownTime";
			}
			return sReturn;
		}//end DateTimeString
		
		public static string TimeString() {
			return TimeString(false,":",false);
		}
		public static string TimePathString(bool bIncludeMilliseconds) {
			return TimeString(bIncludeMilliseconds,".",true);
		}
		public static string TimeString(bool bIncludeMilliseconds, string sTimeDelimiter, bool bEquidistant) {
			string sReturn="";
			try {
				System.DateTime dtX;
				dtX=DateTime.Now;
				//sReturn=dtX.Year+sDateDelimiter;
				//if (dtX.Month<10) sReturn+="0";
				//sReturn+=dtX.Month.ToString()+sDateDelimiter;
				//if (dtX.Day<10) sReturn+="0";
				//sReturn+=dtX.Day.ToString()+sDateTimeSep;
				if (bEquidistant && dtX.Hour<10) sReturn+="0";
				sReturn+=dtX.Hour.ToString()+sTimeDelimiter;
				if (bEquidistant && dtX.Minute<10)sReturn+="0";
				sReturn+=dtX.Minute.ToString()+sTimeDelimiter;
				if (bEquidistant && dtX.Second<10)sReturn+="0";
				sReturn+=dtX.Second.ToString()+"s"+sTimeDelimiter;
				if (bIncludeMilliseconds) {
					int iMilNow=dtX.Millisecond;
					if (iMilNow<10) sReturn+="0";
					if (iMilNow<100) sReturn+="0";
					sReturn+=iMilNow.ToString()+"ms";
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting current date & time");
				sReturn="UnknownTime";
			}
			return sReturn;
		}//end TimeString
		
		public static string SequenceDigits(long lFrame) {
			return SequenceDigits(lFrame, 4);
		}
		public static string SequenceDigits(long lFrame, int iMinDigits) {
			string sDigits;
			long lFrameDestructible=lFrame;
			long lDigit;
			long lMod=10;
			long lDivisor=1;
			sDigits="";
			while (lFrameDestructible>0) {
				lDigit=lFrameDestructible%lMod;
				lFrameDestructible-=lDigit;
				lDigit/=lDivisor;
				sDigits=lDigit.ToString()+sDigits;
				lMod*=10;
				lDivisor*=10;
			}
			while (sDigits.Length<iMinDigits) sDigits="0"+sDigits;
			return sDigits;
		}
		
// WebClient  Client = new WebClient();
// Client.UploadFile("http://www.csharpfriends.com/Members/index.aspx", 
//      "c:\wesiteFiles\newfile.aspx");
// 
// byte [] image;
// 
// //code to initialise image so it contains all the binary data for some jpg file
// client.UploadData("http://www.csharpfriends.com/Members/images/logocc.jpg", image);
// 		
		
// 		msdn:
// 		C#
// 		//Create a new WebClient instance.
// 		WebClient myWebClient = new WebClient();
// 		//Download home page data. 
// 		Console.WriteLine("Accessing {0} ...",  uriString);                        
// 		//Open a stream to point to the data stream coming from the Web resource.
// 		Stream myStream = myWebClient.OpenRead(uriString);
// 		Console.WriteLine("\nDisplaying Data :\n");
// 		StreamReader sr = new StreamReader(myStream);
// 		Console.WriteLine(sr.ReadToEnd());
// 		//Close the stream. 
// 		myStream.Close();
// 		
		public static string DownloadToString(string sUrl) {
			return DownloadToString(sUrl, Environment.NewLine);
		}
		public static string DownloadToString(string sUrl, string sInsertMeAtNewLine) {
			Stream streamNow=null;
			StreamReader srNow=null;
			//WebRequest wrNow = new WebRequest ();
			WebClient wcNow=null;
			string sReturn="";
			//try {
			try {
				wcNow=new WebClient();
				streamNow=wcNow.OpenRead(sUrl);
				srNow=new StreamReader(streamNow);
				try {
					string sLine="";
					while ( (sLine=srNow.ReadLine()) != null ) {
						sReturn+=sLine+sInsertMeAtNewLine;
					}
					streamNow.Close();
					//wcNow.Close();
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"downloading text string from web","DownloadToString("+RReporting.StringMessage(sUrl,true)+",...)");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"accessing site","DownloadToString("+RReporting.StringMessage(sUrl,true)+",...)");
			}
			//}
			//catch (Exception exn) {
			//	RReporting.ShowExn(exn,"reading site","DownloadToString("+RReporting.StringMessage(sUrl,true)+",...)");
			//}
			return sReturn;
		}
		public static string FileToString(string sFile) {
			return FileToString(sFile, Environment.NewLine);
		}
		public static string FileToString(string sFile, string sInsertMeAtNewLine) {
			return FileToString(sFile, sInsertMeAtNewLine, false);
		}
		
		public static string FileToString(string sFile, string sInsertMeAtNewLine, bool bAllowLoadingEndingNewLines) {//formerly StringFromFile
			RReporting.sLastFile=sFile;
			StreamReader sr;
			string sDataX="";
			string sLine;
			try {
				sr=new StreamReader(sFile);
				//bool bFirst=true;
				while ( (sLine=sr.ReadLine()) != null ) {
					//if (bFirst==true) {
					//	sDataX=sLine;
					//	bFirst=false;
					//}
					//else 
					sDataX+=sLine+sInsertMeAtNewLine;
				}
				sDataX=sDataX.Substring(0,sDataX.Length-(Environment.NewLine.Length));
				if (!bAllowLoadingEndingNewLines) {
					while (sDataX.EndsWith(Environment.NewLine)) {
						sDataX=sDataX.Substring(0,sDataX.Length-Environment.NewLine.Length);
					}
				}
				sr.Close();
				//StringToFile(sFile+".AsLoadedToString.TEST.dmp",sDataX);
				//while (sDataX.EndsWith(Environment.NewLine))
				//	sDataX=sDataX.Substring(0,sDataX.Length-(Environment.NewLine.Length));
				//bFirst=false;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
				sDataX="";
			}
			return sDataX;
		}
		public static bool StringReadLine(out string sReturn, string sAllData, ref int iMoveMe) {//formerly ReadLine
			bool HasALine=false;
			bool bNewLine=false;
			sReturn="";
			try {
				
				int iStart=iMoveMe;
				if (iMoveMe<sAllData.Length) HasALine=true;
				while (iMoveMe<sAllData.Length) {//i.e. could be starting at 0 when length is 1!
					//string sTemp=SafeSubstring(sAllData,iMoveMe,Environment.NewLine.Length);
					if (CompareAt(sAllData,Environment.NewLine,iMoveMe)) {
						bNewLine=true;
						break;
					}
					else iMoveMe++;
				}
				if (!bNewLine) iMoveMe=sAllData.Length;//run to end if started after last newline (or there is no newline)
				sReturn=SafeSubstring(sAllData,iStart,iMoveMe-iStart);
				//RReporting.Debug("Base Read line ["+iStart.ToString()+"]toExcludingChar["+iMoveMe.ToString()+"]:"+sReturn);
				if (bNewLine) iMoveMe+=Environment.NewLine.Length;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return HasALine;
		}//end StringReadLine


#endregion moved from Base
#region utilities
		public static bool IsNewLineChar(char cNow) {
			return RString.Contains(sPossibleNewLineChars, cNow);
		}
		public static bool IsNewLineChar(string sNow, int iAt) {
			return RString.Contains(sPossibleNewLineChars, sNow, iAt);
		}
		public static bool IsSpacingChar(char cNow) {
			return RString.Contains(sPossibleSpacingChars, cNow);
		}
		public static bool IsSpacingChar(string sNow, int iAt) {
			return RString.Contains(sPossibleSpacingChars, sNow, iAt);
		}
		public static bool IsWhiteSpace(string sNow, int iAt) {
			return IsNewLineChar(sNow,iAt)||IsSpacingChar(sNow,iAt);
		}
		public static bool IsWhiteSpaceOrChar(string sNow, int iAt, char cOrThis) {
			return IsNewLineChar(sNow,iAt)||IsSpacingChar(sNow,iAt)||CompareAt(sNow,cOrThis,iAt);
		}
		public static bool IsWhiteSpace(char cNow) {
			return IsNewLineChar(cNow)||IsSpacingChar(cNow);
		}
		public static int IndexOfWhiteSpace(string sNow, int start) {
			int iReturn=-1;
			if (sNow!=null) {
				if (start<0) start=0;
				for (int iNow=0; iNow<sNow.Length; iNow++) {
					if (IsWhiteSpace(sNow[iNow])) {
						iReturn=iNow;
						break;
					}
				}
			}
			return iReturn;
		}
		public static int IndexOfWhiteSpaceOrChar(string sNow, int start, char cOrThis) {
			int iReturn=-1;
			if (sNow!=null) {
				if (start<0) start=0;
				for (int iNow=0; iNow<sNow.Length; iNow++) {
					if (IsWhiteSpaceOrChar(sNow[iNow],cOrThis)) {
						iReturn=iNow;
						break;
					}
				}
			}
			return iReturn;
		}
		public static int IndexOfWhiteSpace(string sNow) {
			return IndexOfWhiteSpace(sNow,0);
		}
		public static int IndexOfNonWhiteSpace(string sNow, int start) {
			int iReturn=-1;
			if (sNow!=null) {
				if (start<0) start=0;
				for (int iNow=0; iNow<sNow.Length; iNow++) {
					if (!IsWhiteSpace(sNow[iNow])) {
						iReturn=iNow;
						break;
					}
				}
			}
			return iReturn;
		}
		public static bool IsNeitherWhiteSpaceNorChar(char cVal, char cNorThis) {
			return (!IsWhiteSpace(cVal)&&(cVal!=cNorThis));
		}
		public static bool IsWhiteSpaceOrChar(char cVal, char cOrThis) {
			return (IsWhiteSpace(cVal)||(cVal==cOrThis));
		}
		public static int IndexOfNonWhiteSpace(string sNow) {
			return IndexOfNonWhiteSpace(sNow,0);
		}
		public static int IndexOfNonWhiteSpaceNorChar(string sNow, int start, char cNorThis) { //aka IndexOfNeitherWhiteSpaceNorChar
			int iReturn=-1;
			if (sNow!=null) {
				if (start<0) start=0;
				for (int iNow=0; iNow<sNow.Length; iNow++) {
					if (!IsWhiteSpace(sNow[iNow])&&sNow[iNow]!=cNorThis) {
						iReturn=iNow;
						break;
					}
				}
			}
			return iReturn;
		}
		public static string RemoveEndsWhiteSpace(string sDataX) {
			if (sDataX!=null) {
				while (sDataX.Length>0&&IsWhiteSpace(sDataX[0])) sDataX=sDataX.Substring(1);
				while (sDataX.Length>0&&IsWhiteSpace(sDataX[sDataX.Length-1])) sDataX=sDataX.Substring(0,sDataX.Length-1);
			}
			return sDataX;
		}
		public static void RemoveEndsWhiteSpace(ref string sDataX) {
			if (sDataX!=null) {
				while (sDataX.Length>0&&IsWhiteSpace(sDataX[0])) sDataX=sDataX.Substring(1);
				while (sDataX.Length>0&&IsWhiteSpace(sDataX[sDataX.Length-1])) sDataX=sDataX.Substring(0,sDataX.Length-1);
			}
		}
		public static void RemoveEndsWhiteSpace(ref RString sVal) {
			if (sVal!=null) {
				while (sVal.Length>0&&IsWhiteSpace(sVal[0])) sVal=sVal.Substring(1);
				while (sVal.Length>0&&IsWhiteSpace(sVal[sVal.Length-1])) sVal=sVal.Substring(0,sVal.Length-1);
			}
		}
		
		public static void ReplaceAny(ref char[] sSrc, char[] sOld, char cNew) {
			if (sSrc!=null) ReplaceAny(ref sSrc,sOld,cNew,0,sSrc.Length);
		}
		public static void ReplaceAny(ref char[] sSrc, char[] sOld, char cNew, int iSrcStart, int iSrcEnderEx) {
			try {
				for (int iNow=iSrcStart; iNow<iSrcEnderEx; iNow++) {
					for (int iOld=0; iOld<sOld.Length; iOld++) {
						if (sSrc[iNow]==sOld[iOld]) {
							sSrc[iNow]=cNew;
							break;
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"replacing characters");
			}
		}
		public static string ReplaceAny(string sSrc, char[] sOld, char cNew) {
			char[] carrOrig=null;
			try {
				if (sSrc!=null&&sOld!=null) {
					carrOrig=ToArray(sSrc);
					ReplaceAny(ref carrOrig, sOld, cNew);
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"replacing characters");
			}
			return ToString(carrOrig);
		}//end ReplaceAny(string, char[], char)
		public static string ReplaceAny_Slow(string sSrc, char[] sOld, char cNew) {
			try {
				if (sSrc!=null&&sOld!=null) {
					for (int iNow=0; iNow<sSrc.Length; iNow++) {
						for (int iOld=0; iOld<sOld.Length; iOld++) {
							if (sSrc[iNow]==sOld[iOld]) {
								if (iNow==0) sSrc=char.ToString(cNew)+sSrc.Substring(iNow+1);
								else if (iNow+1<sSrc.Length) sSrc=sSrc.Substring(0,iNow)+char.ToString(cNew)+sSrc.Substring(iNow+1);
								else sSrc=sSrc.Substring(0,iNow)+char.ToString(cNew);
								break;
							}
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"replacing characters");
			}
			return sSrc;
		}//end ReplaceAny(string, char[], char)
		public static string ReplaceAny(string sSrc, string sOld, string cNew) {
			try {
				if (sSrc!=null&&sOld!=null) {
					for (int iNow=0; iNow<sSrc.Length; iNow++) {
						for (int iOld=0; iOld<sOld.Length; iOld++) {
							if (sSrc[iNow]==sOld[iOld]) {
								if (iNow==0) sSrc=cNew+sSrc.Substring(iNow+1);
								else if (iNow+1<sSrc.Length) sSrc=sSrc.Substring(0,iNow)+cNew+sSrc.Substring(iNow+1);
								else sSrc=sSrc.Substring(0,iNow)+cNew;
								break;
							}
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"replacing string characters");
			}
			return sSrc;
		}//end ReplaceAny(string,string,string)
		public static string sDirSep {
			get {return char.ToString(Path.DirectorySeparatorChar);}
		}
		public static string FindBetweenI(string sDataX, string sOpener, string sCloser, int iStartFrom) {//case-insensitive
			string sReturn="";
			string sVerb="starting";
			try { //string.Compare(sDataX,sOpener,true/*ignore case*/)
				sVerb="checking whether any data";
				if (sDataX!=null&&sDataX.Length>0) {
					sVerb="checking whether beyond range";
					if (iStartFrom+sOpener.Length<sDataX.Length) {
						sVerb="looking for opener at "+iStartFrom.ToString();
						int iOpener=cultureinfo.CompareInfo.IndexOf(sDataX,sOpener,iStartFrom,System.Globalization.CompareOptions.IgnoreCase);
						if (sOpener!=null) sVerb="looking for closer at "+(iOpener+sOpener.Length).ToString();
						else sVerb="looking for closer (opener is null!)";
						int iCloser=cultureinfo.CompareInfo.IndexOf(sDataX,sCloser,iOpener+sOpener.Length,System.Globalization.CompareOptions.IgnoreCase); //sDataX.IndexOf(sCloser,iOpener+sOpener.Length);
						if (iOpener>-1&&iCloser>iOpener) {
							sVerb="getting opener length";
							iOpener+=sOpener.Length;
							sVerb="getting substring between closer and opener";
							sReturn=sDataX.Substring(iOpener,iCloser-iOpener);
							sVerb="finishing";
						}
					}
					else {
						Console.WriteLine("Warning: result of search would start beyond data (looking for data after \""+sOpener+"\" at index "+iStartFrom.ToString()+" where data length is "+sDataX.Length.ToString()+")");
					}
				}//end if any data
				else {
					Console.WriteLine("Warning: no data to search (looking for data after \""+sOpener+"\")");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,sVerb,"FindBetweenI (scraping value)");
			}
			return sReturn;
		}//end FindBetweenI
		public static string DetectNewLine(string sDataX) {
			string sReturn="";
			int iNow;
			int iLoc;
			int iUsedMethods=0;
			if (sDataX!=null&&sDataX.Length>0&&sPossibleNewLineChars!=null&&sPossibleNewLineChars.Length>0) {
				bool[] barrUsed=new bool[sPossibleNewLineChars.Length];
				for (iNow=0; iNow<barrUsed.Length; iNow++) barrUsed[iNow]=false;
				for (iNow=0; iNow<sDataX.Length; iNow++) {
					for (int iMethod=0; iMethod<sPossibleNewLineChars.Length; iMethod++) {
						if ( sDataX[iNow]==sPossibleNewLineChars[iMethod] && !barrUsed[iMethod] ) {
							sReturn+=sDataX[iNow];
							barrUsed[iMethod]=true;
							iUsedMethods++;
							break;
						}
					}
					if (iUsedMethods>=sPossibleNewLineChars.Length) break;
				}
			}
			return sReturn;
		}//this is just to blow your mind
		public static bool IsBlank(RString sNow) {
			return !IsNotBlank(sNow);
		}
		public static bool IsNotBlank(RString sNow) {
			return sNow!=null&&sNow.Length>0;
		}
		public static bool IsBlank(string sNow) {
			return !IsNotBlank(sNow);
		}
		public static bool IsNotBlank(string sNow) {
			return sNow!=null&&sNow.Length>0;
		}
		public static bool SplitStyle(out string[] sarrName, out string[] sarrValue, string sStyleWithoutCurlyBraces) {//formerly StyleSplit
			sarrName=null;
			sarrValue=null;
			return RString.SplitAssignments(out sarrName, out sarrValue, sStyleWithoutCurlyBraces, ':', ';');
		}
		public static int CountWhiteSpaceAreas(string sData, bool bCountEvenIfInQuotes, int iStart, int iEndBefore) {
			int iCount=0;
			int iParsingAt=iStart;
			bool bInQuotes=false;
			bool bPreviousWasWhiteSpace=false;
			if (sData!=null) {
				if (iEndBefore>sData.Length) iEndBefore=sData.Length;
				while (iParsingAt<iEndBefore) {
					if (IsWhiteSpace(sData[iParsingAt])&&(!bInQuotes||bCountEvenIfInQuotes)) {
						if (!bPreviousWasWhiteSpace) iCount++;
						bPreviousWasWhiteSpace=true;
					}
					else {
						if (sData[iParsingAt]=='"') bInQuotes=!bInQuotes;
						bPreviousWasWhiteSpace=false;
					}
					iParsingAt++;
				}
			}
			return iCount;
		}//end CountWhiteSpaceAreas
		public static bool EndsWith(string val, char ch) {
			return val!=null&&val.Length>0&&val[val.Length-1]==ch;
		}
		///<summary>
		///Returns true if ends with char or char then whitespace
		/// i.e. if cChar=';' then sVal ending with ";" or or that character followed by any
		/// length of any types of whitespace.
		///</summary>
		public static bool EndsWithCharOrCharThenWhiteSpace(string sVal, char cChar, int start, int endbefore) {
			bool bReturn=false;
			if (sVal!=null) {
				for (int iNow=endbefore-1; iNow>=start; iNow--) {
					if (!IsWhiteSpace(sVal[iNow])) {
						if (sVal[iNow]==cChar) bReturn=true;
						else bReturn=false;
						break;
					}
				}
			}
			return bReturn;
		}//end EndsWithCharOrCharThenWhiteSpace

		///<summary>
		///Gets value locations from a delimited list of assignments
		/// -if sName is null, then all assignment values will be returned, otherwise returns
		/// all values where the variable is named by sName. If the referenced arrays are big
		/// enough they will not be redimensioned.
		///NamesReturn becomes a location array ({start, endbefore, ...} where endbefore
		/// is the character after the last character in the value).  It will mark all the 
		/// instances of sName in sDelimitedAssignments, which may not be the same case if
		/// bCaseSensitive is false.
		///ValuesReturn becomes a location array of the values matching NamesReturn.  Where a
		/// variable is declared but not assigned, start==endbefore
		///Returns the count of used incedes in ValuesReturn.
		///</summary>
		public static int GetMultipleAssignmentLocations(ref int[] NamesReturn, ref int[] ValuesReturn, string sDelimitedAssignments, string sName, char cAssignmentOperator, char cStatementDelimiter, int iStart, int iEndBefore, bool bCaseSensitive) {
			string sName_Processed=bCaseSensitive?sName:sName.ToLower();
			int iParsingAt=iStart;
			int iNameStartNow=-1;
			int iNameEnderNow=-1;
			int iValStartNow=-1;
			int iValEnderNow=-1;
			int iSection=AssignmentBetween;
			int iCount=0;
			bool bInQuotes=false;
			bool bWhiteSpaceStatementDelimiter=IsWhiteSpace(cStatementDelimiter);
			int iMax  =  
					(sName!=null)
					?(bCaseSensitive?CountInstances(sDelimitedAssignments,sName,iStart,iEndBefore):CountInstancesI(sDelimitedAssignments,sName,iStart,iEndBefore))
					:(bWhiteSpaceStatementDelimiter?(CountWhiteSpaceAreas(sDelimitedAssignments,false)+1):(CountInstances(sDelimitedAssignments,cStatementDelimiter)+1));
			try {
				if (iMax>0) {
					if (ValuesReturn==null||ValuesReturn.Length<iMax*2) ValuesReturn=new int[iMax*2];
					if (NamesReturn==null||NamesReturn.Length<iMax*2) NamesReturn=new int[iMax*2];
					while (iParsingAt<=iEndBefore) {
						switch (iSection) {
						case AssignmentBetween:
							if (iParsingAt>=iEndBefore ||IsNeitherWhiteSpaceNorChar(sDelimitedAssignments[iParsingAt], cStatementDelimiter)) {
								iNameStartNow=iParsingAt;
								iSection=AssignmentName;
							}
							break;
						case AssignmentName:
							if ( (iParsingAt>=iEndBefore) ||IsWhiteSpace(sDelimitedAssignments[iParsingAt]) ||(sDelimitedAssignments[iParsingAt]==cStatementDelimiter) ) {
								iNameEnderNow=iParsingAt;
								if (sName==null ||(bCaseSensitive?CompareAt(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow):CompareAtI(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow)) ) {
									///return blank for valueless tag:
									NamesReturn[iCount*2]=iNameStartNow;
									NamesReturn[iCount*2+1]=iNameEnderNow;
									ValuesReturn[iCount*2]=iParsingAt;
									ValuesReturn[iCount*2+1]=iParsingAt;
									iCount++;
								}
								iNameStartNow=-1;
								iNameEnderNow=-1;
								iSection=AssignmentBetween;
							}
							else if (sDelimitedAssignments[iParsingAt]==cAssignmentOperator) {
								iNameEnderNow=iParsingAt;
								iSection=AssignmentOperator;//allows whitespace after sign, but AssignmentOperator case does not allow whitespace inside value
							}
							break;
						case AssignmentOperator:
							if (iParsingAt>=iEndBefore ||IsNeitherWhiteSpaceNorChar(sDelimitedAssignments[iParsingAt], cStatementDelimiter)) {
								iValStartNow=iParsingAt;//ok since not a whitespace
								iSection=AssignmentValue;//ok since only comes here if name ended with an assignment operator
							}
							break;
						case AssignmentValue:
							if (iParsingAt<iEndBefore&&sDelimitedAssignments[iParsingAt]=='"') bInQuotes=!bInQuotes;
							else if ( iParsingAt>=iEndBefore ||(bWhiteSpaceStatementDelimiter&&!bInQuotes&&IsWhiteSpaceOrChar(sDelimitedAssignments[iParsingAt], cStatementDelimiter)) 
							||(!bWhiteSpaceStatementDelimiter&&!bInQuotes&&(sDelimitedAssignments[iParsingAt]==cStatementDelimiter)) ) {
								iValEnderNow=iParsingAt;
								if (sName==null ||(bCaseSensitive?CompareAt(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow):CompareAtI(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow)) ) {
									NamesReturn[iCount*2]=iNameStartNow;
									NamesReturn[iCount*2+1]=iNameEnderNow;
									ValuesReturn[iCount*2]=iParsingAt;
									ValuesReturn[iCount*2+1]=iParsingAt;
									iCount++;
								}
								iNameStartNow=-1;
								iNameEnderNow=-1;
								bInQuotes=false;
							}
							break;
						default:break;
						}//end switch iSection
						iParsingAt++;
					}//end while iParsingAt<=iEndBefore
					if (iCount*2<ValuesReturn.Length) ValuesReturn[iCount*2]=-1; //add terminator if not full
				}//end if any instances
				else if (ValuesReturn!=null) ValuesReturn[0]=-1;
			}
			catch (Exception exn) {
				if (ValuesReturn!=null&&iCount*2<ValuesReturn.Length) ValuesReturn[iCount*2]=-1;
				RReporting.ShowExn(exn);
			}
			//if (iCount<=0) RReporting.Debug("There were no statements in the assignment list (GetMultipleAssignmentValueLocations)");
			return iCount;//qReturn.GetInternalArray();
		}//end GetMultipleAssignmentLocations
		public static int GetMultipleAssignmentLocationsI(ref int[] NamesReturn, ref int[] ValuesReturn, string sDelimitedAssignments, string sName, char cAssignmentOperator, char cStatementDelimiter, int iStart, int iEndBefore) {
			return GetMultipleAssignmentLocations(ref NamesReturn, ref ValuesReturn, sDelimitedAssignments, sName, cAssignmentOperator, cStatementDelimiter, iStart, iEndBefore, false);
		}


		///<summary>
		///Split assignments, accounting for whitespace (only allows whitespace inside non-quoted values--i.e. rgb(64, 64, 0); --if cStatementDelimiter is not a whitespace character i.e. ';' in this example)
		///sarrName and sarrValue will only be created/recreated if necessary, so the return
		/// count may be smaller than the length of the arrays (this is to reduce reallocation)!
		///Returns the count of how many variables were found (name and value stored in sarrName
		/// and sarrValue)
		///</summary>
		public static int SplitAssignments(ref string[] sarrName, ref string[] sarrValue, string sStatements, char cAssignmentOperator, char cStatementDelimiter) {//formerly StyleSplit
			int iFound=0;
			try {
				int iStartNow;
				int iStartNext=RString.IndexOfNonWhiteSpaceNorChar(sStatements,iValEnder,cStatementDelimiter);
				int iOperator;
				int iValEnder;
				int iValStart;
				string sNameNow="";
				string sValNow="";
				ArrayList alNames=new ArrayList();
				ArrayList alValues=new ArrayList();
				while (iStartNext<sStatements.Length) {
					iStartNow=iStartNext;
					iOperator=sStatements.IndexOf(cAssignmentOperator,iStartNow);
					if (iOperator>-1) {
						iValStart=RString.IndexOfNonWhiteSpace(sStatements, iOperator+1);
						if (iValStart>-1) {
							int iFindValEnd=iValStart+1;
							bool bInQuotes=false;
							iValEnder=-1;
							int iValEndByStatementDelimiter=-1;
							if (!IsWhiteSpace(cStatementDelimiter)) {
								iValEndByStatementDelimiter=IndexOf(sStatements,cStatementDelimiter,iValStart);
								while (iValEnd>=0&&IsWhiteSpaceOrChar(sStatements,iValEndByStatementDelimiter,cStatementDelimiter)) {
									iValEndByStatementDelimiter--; //accounts for space before statement delimiter i.e. "rgb(0,0,0) ;"
								}
								if (iValEndByStatementDelimiter>-1) iValEndByStatementDelimiter++;//after the character found, not on it
							}
							while (iFindValEnd<=sStatements.Length) {
								if (iFindValEnd<sStatements.Length&&sStatements[iFindValEnd]=='"') bInQuotes=!bInQuotes;
								else if ( iFindValEnd==sStatements.Length
									||(!bInQuotes&&sStatements[iFindValEnd]==cStatementDelimiter)
									||(!bInQuotes&&IsWhiteSpace(sStatements[iFindValEnd])) ) {
									iValEnder=iFindValEnd;
									break;
								}
								iFindValEnd++;
							}
							if (iValEndByStatementDelimiter>iValEnder) iValEnder=iValEndByStatementDelimiter;
							if (iValEnder>-1) {
								iStartNext=RString.IndexOfNonWhiteSpaceNorChar(sStatements,iValEnder,cStatementDelimiter);
								if (iStartNext<0) iStartNext=sStatements.Length;
							}//end if found iValEnder
							else {
								iValEnder=iValStart;
								iStartNext=sStatements.Length;
							}
						}//end if found iValStart
						else {
							iValStart=iOperator+1;
							iValEnder=iValStart+1;
							iStartNext=RString.IndexOfNonWhiteSpaceNorChar(sStatements,iValEnder,cStatementDelimiter);
							if (iStartNext<0) iStartNext=sStatements.Length;
						}
					}//end if found operator
					else {
						iOperator=RString.IndexOfWhiteSpaceOrChar(sStatements,iStartNow,cStatementDelimiter);
						if (iOperator<0) {
							iOperator=sStatements.Length; //ok since no whitespace remains
							iStartNext=iOperator;
						}
						else {
							iStartNext=RString.IndexOfNonWhiteSpaceNorChar(sStatements,iStartNow,cStatementDelimiter);
							if (iStartNext<0) iStartNext=sStatements.Length;
						}
						iValStart=iOperator;
						iValEnder=iValStart;
					}
					if (iOperator-iStartNow>0) {
						alNames.Add( SafeSubstring(sStatements,iStartNow,iOperator-iValStart) );
						alValues.Add( SafeSubstring(sStatements,iValStart,iValEnder-iValStart) );
						iFound++;
					}
					else {
						if (iStartNext!=-1) {
							iStartNext=-1;
							RReporting.ShowErr("Variable name expected in: \""+sStatements.Substring(iStartNow)+"\".","SplitAssignements");
						}
					}
				}//end while iStartNext<sStatements.Length
				if (iFound>0) {
					if (alValues.Count==alNames.Count&&alValues.Count==iFound) {
						if (sarrName==null||iFound>sarrName.Length) sarrName=new string[iFound];
						if (sarrValue==null||iFound>sarrValue.Length) sarrValue=new string[iFound];
						for (int iPop=0; iPop<iFound; iPop++) {
							sarrName[iPop]=alNames[iPop].ToString();
							sarrValue[iPop]=alValues[iPop].ToString();
						}
					}
					else {
						bGood=false;
						Console.Error.WriteLine( String.Format("SplitAssignements error: Values/Names count do not match--names:{0}; values:{1}; assignments:{2}",RReporting.SafeCount(alNames),RReporting.SafeCount(alValues), iFound) );
					}
				}
				else {
					sarrName=null;
					sarrValue=null;
					RReporting.ShowErr("No style variables in \""+sStatements+"\"!","SplitAssignements");
					bGood=false;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","SplitAssignements");
				bGood=false;
			}
			return bGood;
		}//end SplitAssignments
		public static bool SplitAssignmentsSgml(out string[] sarrName, out string[] sarrValue, string sTagPropertiesWithNoTagNorGTSign) {//, char cAssignmentOperator, char cStatementDelimiter) {//formerly StyleSplit
			sarrName=null;
			sarrValue=null;
			return SplitAssignments(out sarrName, out sarrValue, sTagPropertiesWithNoTagNorGTSign, '=', ' ');//DOES account for other whitespace
		}//end SplitAssignmentsSgml
		public static string SafeString(string val) {
			return val!=null?val:"";
		}
		public static int SafeLength(string sValue) {	
			try {
				if (sValue!=null&&sValue!="") return sValue.Length;
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","Base SafeLength(string)");
			}
			return 0;
		}
		public static char[] ToArray(string sNow) {
			char[] carrReturn=null;
			try {
				if (sNow!=null&&sNow.Length>0) {
					carrReturn=new char[sNow.Length];
					for (int iNow=0; iNow<carrReturn.Length; iNow++) {
						carrReturn[iNow]=sNow[iNow];
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return carrReturn;
		}
 		private static readonly string[] sarrCSType={"bool","byte","sbyte","char","decimal","double","float","int","uint","long","ulong","object","short","ushort","string"};
		private static readonly string[] sarrCSTypeMapsTo={"Boolean","Byte","SByte","Char","Decimal","Double","Single","Int32","UInt32","Int64","UInt64","Object","Int16","UInt16","String"};//usually use assumed System.*
		private static string[] sarrCSTypeMapsToFull=null;
		public static bool IsCSTypeAt(string sData, int start, int endbefore) {
			return CSTypeAtToInternalTypeIndex(sData,start,endbefore)>-1;
		}
		public static bool IsCSTypeAt(string sData, ref int iCursorToMove) {
			return CSTypeAtToInternalTypeIndex(sData, ref iCursorToMove)>-1;
		}
		///<summary>
		///Checks whether the given substring is a CSharp type (i.e. int, Int32, System.Int32)
		///Returns true if the string with length=endbefore-start is a type
		///</summary>
		public static int CSTypeAtToInternalTypeIndex(string sData, int start, int endbefore) {
			int iReturn=-1;
			if (sarrCSTypeMapsToFull==null) {
				sarrCSTypeMapsToFull=new string[sarrCSTypeMapsTo.Length];
				for (int iNow=0; iNow<sarrCSTypeMapsTo.Length; iNow++) sarrCSTypeMapsToFull[iNow]="System."+sarrCSTypeMapsTo[iNow];
			}
			for (int iNow=0; iNow<sarrCSType.Length&&iFound<0; iNow++) {
				if (RString.CompareAt(sData,sarrCSType[iNow],start,endbefore)) iFound=iNow;
			}
			for (int iNow=0; iNow<sarrCSTypeMapsTo.Length&&iFound<0; iNow++) {
				if (RString.CompareAt(sData,sarrCSTypeMapsTo[iNow],start,endbefore)) iFound=iNow;
			}
			if (sData.StartsWith("System.")) {
				for (int iNow=0; iNow<sarrCSTypeMapsToFull.Length&&iFound<0; iNow++) {
					if (RString.CompareAt(sData,sarrCSTypeMapsToFull[iNow],start,endbefore)) iFound=iNow;
				}
			}
			return bFound;
		}//end IsCSTypeAt
		///<summary>
		///Returns true if there is a CSharp Type (i.e. int, Int32, System.Int32) at iCursorToMove
		///iCursorToMove: The location to compare to a CSharp type--the variable will be
		/// changed to the location after the typename.  The location may be a '[' symbol
		/// or any other character.
		///</summary>
		public static int CSTypeAtToInternalTypeIndex(string sData, ref int iCursorToMove) {
			int iReturn=-1;
			if (sarrCSTypeMapsToFull==null) {
				sarrCSTypeMapsToFull=new string[sarrCSTypeMapsTo.Length];
				for (int iNow=0; iNow<sarrCSTypeMapsTo.Length; iNow++) sarrCSTypeMapsToFull[iNow]="System."+sarrCSTypeMapsTo[iNow];
			}
			for (int iNow=0; iNow<sarrCSType.Length&&iFound<0; iNow++) {
				if (RString.CompareAt(sData,sarrCSType[iNow],iCursorToMove)) {
					iFound=iNow;
					iCursorToMove+=sarrCSType.Length;
				}
			}
			for (int iNow=0; iNow<sarrCSTypeMapsTo.Length&&iFound<0; iNow++) {
				if (RString.CompareAt(sData,sarrCSTypeMapsTo[iNow],iCursorToMove)) {
					iFound=iNow;
					iCursorToMove+=sarrCSType.Length;
				}
			}
			if (sData.StartsWith("System.")) {
				for (int iNow=0; iNow<sarrCSTypeMapsToFull.Length&&iFound<0; iNow++) {
					if (RString.CompareAt(sData,sarrCSTypeMapsToFull[iNow],iCursorToMove)) {
						iFound=iNow;
						iCursorToMove+=sarrCSType.Length;
					}
				}
			}
			return bFound;
		}//end IsCSTypeAt
// 		public static readonly string[] sarrCSTypeHtmlTagword={};
// 		public static readonly string[] sarrCSTypeHtmlInputType={};
// 		public static readonly string[] sarrCSTypeHtmlAdditionalTag={};
// 		public static string CSTypeToHtmlFormTagword(string sCSType) {
// 		}
// 		public static string CSTypeToHtmlFormInputTypeProperty(string sCSType) {
// 		}
// 		public static bool CSTypeFormInputNeedsLabel(string sCSType) {
// 		}


		public static int PartA(int iHalfIndex) {//converts Pair index to index of 1st item in pair
			return iHalfIndex*2;
		}
		public static int PartB(int iHalfIndex) {//converts Pair index to index of 2nd item in pair
			return iHalfIndex*2+1;
		}
		///<summary>
		///Checks whether the substring is a C variable name or type name
		///Returns true if variable/type name (can be preceded and/or followed by whitespace inside
		/// of the substring)
		///</summary>
		public static bool ContainsOneCSymbolAndNothingElse(string sData, int start, int endbefore) {
			bool bInSpace=false;
			int iCountNames=0;
			int iNow=IndexOfNonWhiteSpace(sData,start);
			int iStart=iNow;
			if (iNow>-1) {
				iCountNames++;
				while (iNow<endbefore) {
					if (bInSpace&&IsAlphaNumeric(sData[iNow])) {
						bInSpace=false;
						iCountNames++;
					}
					else if (!bInSpace&&!IsAlphaNumeric(sData[iNow])) {
						bInSpace=true;
					}
				}
			}
			return iCountNames==1&&!IsDigit(sData[iStart]);
		}//end ContainsOneCSymbolAndNothingElse
		private const int CDeclType=0;//must be 1--used as index!
		private const int CDeclName=1;//must be 2--used as index!
		private const int CDeclValue=2;//must be 3--used as index!
		private const int CDeclBetween=4;
		///<summary>
		///sData must be a c or c# declaration in the form "type name_withnospaces=value",
		/// "name_withnospaces=value", or "type name"
		///iarrParts returns a list of locations in the form {startType, endbeforeType,
		/// startName, endbeforeName, startValue, endbeforeValue}.  If any of the parts
		/// of the assignment don't exist, start will equal endbefore.
		///Returns count (count*2 is the element count of iarrParts that is used)
		///</summary>
		public static void CDeclSplit(ref int[] iarrParts, string sData, int start, int endbefore) {//aka SplitCDecl
			int iState=CDeclType;
			int iNow=start;
			int endbeforeNow=0;
			bool bInQuotes=false;
			int iBraceDepth=0;
			bool bInSingleQuotes=false;
			try {
				if (sData!=null) {
					if (iarrParts==null||iarrParts.Length<6) iarrParts=new int[6];
					iarrParts[0]=start;
					for (iNow=1; iNow<6; iNow++) {
						iarrParts[iNow]=-1;
					}
					while (iNow<=endbefore) {
						switch (iState) {
						case CDeclType:
							if (iNow>=endbefore) {
								if (iarrParts[0]<0) iarrParts[0]=endbefore;
								iarrParts[1]=endbefore;
								iarrParts[2]=endbefore;
								iarrParts[3]=endbefore;
								iarrParts[4]=endbefore;
								iarrParts[5]=endbefore;
							}
							else if (!RString.IsWhiteSpace(sData[iNow])) {
								//ok to put all of this in a whitespace area since specific string (typename) is detected
								if (iarrParts[0]<0) iarrParts[0]=iNow;
								if (IsCSTypeAt(sData,ref endbeforeNow)) {
									if (endbeforeNow>=endbefore) {
										iarrParts[1]=endbefore;
										iarrParts[2]=endbefore;
										iarrParts[3]=endbefore;
										iarrParts[4]=endbefore;
										iarrParts[5]=endbefore;
									}
									else if (sData[endbeforeNow]=='[') {
										if (endbeforeNow+1>=endbefore) {
											iarrParts[1]=endbefore;
											RReporting.SourceErr("Expected ']' after '[' in variable declaration but reached end of statement first");
											iNow=endbefore;
										}
										else if (sData[endbeforeNow+1]==']') {
											iarrParts[1]=endbeforeNow+2;
											iNow=endbeforeNow+1;//incremented again below to go past ']'
										}
										else {
											iarrParts[1]=iNow;
											RReporting.SourceErr("Expected ']' after '[' in variable declaration");
											iNow=endbeforeNow;
										}
										iState=CDeclName;
									}//end if '[' (not end of file)
									else if (RString.IsWhiteSpace(sData[endbeforeNow])) {
										iarrParts[1]=endbeforeNow;
										iState=CDeclName;
										iNow=endbeforeNow;
									}
								}//end if Is CS Type
							}//end if not whitespace
							else if (iarrParts[CDeclType*2]>-1) {//if beginning found, is whitespace, but type not found, so variable name must have been first
								iarrParts[CDeclType*2+1]=iarrParts[CDeclType*2];
								iarrParts[CDeclName*2]=iarrParts[CDeclType*2];
								iarrParts[CDeclName*2+1]=iNow;
								RReporting.SourceErr("Expected type specifier before variable","parsing declaration",String.Format("CDeclSplit(){{name:{0}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3])));
								iState=CDeclValue;
							}
							break;
						case CDeclName:
							if (iNow>=endbefore) {
								if (iarrParts[CDeclName*2]<0) iarrParts[CDeclName*2]=iNow;
								iarrParts[CDeclName*2+1]=iNow;
								iarrParts[CDeclValue*2]=iNow;
								iarrParts[CDeclValue*2+1]=iNow;
							}//end if ended
							else {
								if (iarrParts[CDeclName*2]>-1) { //if found beginning
									if (RString.IsWhiteSpaceOrChar(sData[iNow],'=')) {
										iarrParts[CDeclName*2+1]=iNow;
										iState=CDeclValue;
									}//end if end
								}//end if found beginning
								else if (!RString.IsWhiteSpace(sData[iNow])) {//else have not found beginning
									iarrParts[CDeclName*2]=iNow;
									if (RString.IsDigit(sData[iNow])) {
										RReporting.SourceErr("Variable name shouldn't have started with digit","parsing declaration", String.Format("CDeclSplit(){{type:{0};digit:{1}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]),char.ToString(sData[iNow])) );
									}
									else if (sData[iNow]=='=') {
										RReporting.SourceErr(iNow,"Expected variable name but found equal sign","parsing declaration", String.Format("CDeclSplit(){{type:{0}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1])) );
										iarrParts[CDeclName*2+1]=iNow;
										iState=CDeclValue;
									}
								}//end else detect as beginning
							}//end else not ended
							break;
						case CDeclValue:
							if (iNow>=endbefore) {
								if (iarrParts[CDeclValue*2]<0) {
									iarrParts[CDeclValue*2]=iNow;
									RReporting.SourceErr(iNow,"Expected value but found end of data", "parsing declaration",   String.Format( "CDeclSplit(){{type:{0};name:{1}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]),RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3]) )  );
								}
								iarrParts[CDeclValue*2+1]=endbefore;
							}//end if at end
							else if (iarrParts[CDeclValue*2]>-1) {//if already found start
								if (!bInQuotes&&(sData[iNow]=='\''&&(iNow==0||sData[iNow-1]!='\\'))) {
									if (bInSingleQuotes) {
										bInSingleQuotes=!bInSingleQuotes;
										if (iBraceDepth<=0) {
											iarrParts[CDeclValue*2+1]=iNow+1;
											iNow=endbefore+1;//exits outer loop
										}
									}
								}
								else if (!bInQuotes&&!bInSingleQuotes&&sData[iNow]=='}') {
									iBraceDepth--;
									if (iBraceDepth<=0) {
										iarrParts[CDeclValue*2+1]=iNow+1;
										iNow=endbefore+1;//exits outer loop
									}
								}
								else if (!bInQuotes&&!bInSingleQuotes&&sData[iNow]=='{') {
									iBraceDepth++;
								}
								else if (!bInSingleQuotes&&sData[iNow]=='"'&&(iNow==0||sData[iNow-1]!='\\')) {
									if (!bInQuotes) {
										if (iBraceDepth<=0) RReporting.SourceErr(iNow,"Unexpected quotemark--should only see quotemark if variable began with quotemark, bracket, or single-quote OR if quote is a literal preceded by a backslash");
									}
									else if (iBraceDepth<=0) { //force end if depth=0 and endquote found
										iarrParts[CDeclValue*2+1]=iNow+1;
										iNow=endbefore+1;//exits outer loop
									}
									bInQuotes=!bInQuotes;
								}
								else if (!bInQuotes&&!bInSingleQuotes&&( (iBraceDepth<=0&&(IsWhiteSpace(sData[iNow])||sData[iNow]==','))||sData[iNow]==';')) {
									iarrParts[CDeclValue*2+1]=iNow;
									if (sData[iNow]==';'&&iBraceDepth>0) {
										RReporting.SourceErr(iNow,"Found semicolon before end of '{' braces", "parsing declaration",  String.Format( "CDeclSplit(){{type:{0};name:{1}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]),RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3]) )  );
									}
									else if (sData[iNow]==',') {
										RString.SourceErr(iNow,"Should not have received trailing comma in declaration (parser corruption)");
									}
									iNow=endbefore+1;//exits outer loop
								}
								
							}//end if already found beginning
							else if (!RString.IsWhiteSpaceOrChar(sData[iNow],'=')) {
								iarrParts[CDeclValue*2]=iNow;
								if (sData[iNow]=='"') bInQuotes=true;
								else if (sData[iNow]=='{') iBraceDepth=1;
								else if (sData[iNow]=='\'') bInSingleQuotes=true;
								else if (CompareAt(sData,"new ",iNow)) {
									int iParen=RString.IndexOfWhiteSpaceOrChar(sData,'(',iNow);
									int iBrace=RString.IndexOf(sData,'{',iNow);
									int iStart=iNow;
									if (iBrace>-1) {
										iBraceDepth=1;
										iNow=iBrace;//incremented at bottom of loop below
									}
									else if (iParen>-1&&ContainsOneCSymbolAndNothingElse(sData,iNow,iParen)) {
										iParenDepth=1;
										bool bInConstructorQuotes=false;
										bool bInConstructorSingleQuotes=false;
										while (iNow<endbefore) {
											if (!bInConstructorSingleQuotes &&sData[iNow]=='"' &&(iNow==0||sData[iNow-1]!='\\')) bInConstructorQuotes=!bInConstructorQuotes;
											if (!bInConstructorQuotes &&sData[iNow]=='\'' &&(iNow==0||sData[iNow-1]!='\\')) bInConstructorSingleQuotes=!bInConstructorSingleQuotes;
											else if (!bInConstructorQuotes&&!bInConstructorSingleQuotes&&sData[iNow]=='(') iParenDepth++;
											else if (!bInConstructorQuotes&&!bInConstructorSingleQuotes&&sData[iNow]==')') {
												iParenDepth--;
												if (iParenDepth<=0) {
													iarrParts[CDeclValue*2+1]=iNow+1;
													iNow=endbefore+1;//exits outer loop
												}
											}
										}//end while iNow<endbefore finding end of constructor
										if (iNow==endbefore) {
											RReporting.SourceErr(iNow,"Incomplete constructor after \"new\"", "parsing declaration",   String.Format( "CDeclSplit(){{type:{0};name:{1}; declaration:{2}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]), RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3]), RString.SafeSubstringByExclusiveEnder(sData,iStart,iParen) )  );
											iarrParts[CDeclValue*2+1]=endbefore;
											iNow=endbefore+1;//exits outer loop
										}
									}//end if is a constructor call
									else iNow=-1;
									if (iNow<0) {
										iNow=endbefore+1;//exits outer loop
										RReporting.SourceErr(iNow,"Expected constructor parameters or array notation (i.e. string[] {\"1\",\"2\"}) after \"new\"", "parsing declaration",   String.Format( "CDeclSplit(){{type:{0};name:{1}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]),RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3]) )  );
									}
								}//end if "new " operator
							}//else detect as beginning (not whitespace && not '=')
							break;
						default:
							break;
						}
						iNow++;
					}
				}//end if sData!=null
				else RReporting.ShowErr("Tried to split CDecl but text data was null.","parsing declaration","CDeclSplit(...)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
		}//end CDeclSplit
		private static int[] iarrDeclParts=new int[6];
		public static string CDeclTypeSubstring(string sCDeclaration) {
			if (sCDeclaration!=null) {
				CDeclSplit(ref iarrDeclParts, sCDeclaration, 0, sCDeclaration);
				return RString.SafeSubstringByExclusiveEnder(iarrDeclParts[0],iarrDeclParts[1]);
			}
			return "";
		}
		public static string CDeclNameSubstring(string sCDeclaration) {
			if (sCDeclaration!=null) {
				CDeclSplit(ref iarrDeclParts, sCDeclaration, 0, sCDeclaration);
				return RString.SafeSubstringByExclusiveEnder(iarrDeclParts[2],iarrDeclParts[3]);
			}
			return "";
		}
		public static string CDeclValueSubstring(string sCDeclaration) {
			if (sCDeclaration!=null) {
				CDeclSplit(ref iarrDeclParts, sCDeclaration, 0, sCDeclaration);
				return RString.SafeSubstringByExclusiveEnder(iarrDeclParts[4],iarrDeclParts[5]);
			}
			return "";
		}
#endregion utilities

		#region digit functions
		public static bool IsDigit(char cDigit) {
			if ( cDigit=='0'|| cDigit=='1'||cDigit=='2'||cDigit=='3'||cDigit=='4'||cDigit=='5'
				 ||cDigit=='6'||cDigit=='7'||cDigit=='8'||cDigit=='9'
				 //||(bAllowDecimalDelimiter && cDigit='.')
				 //||(bAllowNumberTriadVisualSeparator && cDigit=',')
			   ) return true;
			else return false;
		}
		///<summary>
		///Examines the string to see if it is numeric--i.e. doesn't contain more than one '.'
		///</summary>
		public static bool IsNumeric(string sNumber, bool bAllowDecimalDelimiter, bool bAllowNumberTriadVisualSeparator) {
			char[] carrNow=sNumber.ToCharArray();
			int iDecimalDelimiters=0;
			bool bReturn=true;
			for (int iChar=0; iChar<carrNow.Length; iChar++) {
				if (carrNow[iChar]=='.') {
					if (!bAllowDecimalDelimiter) {
						bReturn=false;
						break;
					}
					iDecimalDelimiters++;
					if (iDecimalDelimiters>1) {
						bReturn=false;
						break;
					}
				}
				else if (carrNow[iChar]==',') {
					if (!bAllowNumberTriadVisualSeparator) {
						bReturn=false;
						break;
					}
				}
				else if (!IsDigit(carrNow[iChar])) {
					bReturn=false;
					break;
				}
			}
			return bReturn;
		}
		#endregion digit functions
		

		

	}//end RString

	public class CharStack {
		private char[] carr=null;
		public static int iDefaultMax=1024;
		public int iUsed=0;
		public const char Nothing=(char)0xFFFF;
		public bool AutoExpand=true;
		public int Capacity {
			get { return carr!=null?carr.Length:0; }
			set { RMemory.Redim(carr,value); }
		}
		public CharStack() {
			Init(iDefaultMax);
		}
		public void Init(int iSetMax) {
			try {
				carr=new char[iSetMax];
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn,"creating character stack",String.Format("Init({0})",iSetMax) );
			}
		}
		public bool Push(char valAdd) {
			bool bGood=false;
			try {
				if (iUsed+1>carr.Length) {
					if (AutoExpand) Capacity=iUsed+iUsed/2+1;
				}
				if (iUsed+1<=Capacity) {
					carr[iUsed]=valAdd;
					iUsed++;
					bGood=true;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn,"adding to character stack",String.Format("Push({0}){{iUsed:{1}; Capacity:{2}}}",valAdd,iUsed,Capacity) );
			}
			return bGood;
		}//end Push
		///<summary>
		///returns value or CharStack.Nothing
		///</summary>
		public char Pop() {
			try {
				if (iUsed>0) {
					return carr[--iUsed];
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn,"getting character from stack",String.Format("Pop({0}){{iUsed:{1}; Capacity:{2}}}",valAdd,iUsed,Capacity) );
			}
			return Nothing;
		}
		public int Count {
			get { return iUsed; }
		}
		public char Peek(int iAt) {
			try {
				return carr[iAt];
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn,"viewing character in stack",String.Format("Peek({0}){{iUsed:{1}; Capacity:{2}; iAt:{3}}}",valAdd,iUsed,Capacity,iAt) );
			}
			return Nothing;
		}
		public char PeekTop() {
			if (iUsed>0) return Peek(iUsed-1);
			else return Nothing;
		}
	}//end CharStack
}//end namespace