using System;
using System.Text.RegularExpressions;
using System.Collections;//allows full arraylist parameters to be sent out    
//using System.Text; //not needed (RString replaces StringBuilder)
using System.IO;
using System.Net; //for WebClient
using System.Globalization; //for CultureInfo (used for case-insensitive IndexOf)
using System.Text; //for part of FileToHash method
using System.Security.Cryptography; //for hash


namespace ExpertMultimedia {
    public class RString { //like stringbuilder but a little better
        #region variables
        public static readonly string sCSVEDBQUOTE="&exmquo;";
        public static string sDirSep=char.ToString(Path.DirectorySeparatorChar);//{ get {return char.ToString(Path.DirectorySeparatorChar);}    }
        public static int DefaultMaxCapacity=1024;
        //public static bool bGoodString=true;
        //public static DefaultOffset=0;
        private char[] carrData=null;
        private int iStart=0;
        private int iEnder=0;//exclusive ender
        private static readonly char[] carrPossibleNewLineChars=new char[]{'\r','\n'}; //(CR+LF, 0x0D 0x0A, {13,10})
        private static readonly char[] carrPossibleHorizontalSpacingChars=new char[]{' ','\t','\0'};
        public static bool bAllowNewLineInQuotes=false;//TODO: implement this EVERYWHERE
        public static readonly char[] carrWordBreakerInvisible=new char[]{' ','\t'};//formerly called LineBreaker
        public static readonly char[] carrWordBreakerVisible=new char[]{'-',';',':','>'};
        public static readonly string[] sarrDigit=new string[] {"0","1","2","3","4","5","6","7","8","9"};
        public static readonly char[] carrDigit=new char[] {'0','1','2','3','4','5','6','7','8','9'};
        public static readonly char[] carrAlphabetUpper=new char[] {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
        public static readonly string[] sarrAlphabetUpper=new string[] {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"};
        public static readonly char[] carrAlphabetLower=new char[] {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
        public static readonly string[] sarrAlphabetLower=new string[] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"};
        private static int[] iarrDeclPartsTemp=null;
        public static bool IsAlphanumeric(char val) {
            return IsUpper(val)||IsLower(val)||IsDigit(val);
        }
        public static bool IsAlphanumeric(string val, int index) {
            return index>=0&&val!=null&&index<val.Length&&IsAlphanumeric(val[index]);
        }
        //public static string sRemoveSpacingBeforeNewLines1=" "+Environment.NewLine;
        //public static string sRemoveSpacingBeforeNewLines2="\t"+Environment.NewLine;
        public static readonly string[] sarrConsonantLower=new string[] {"b","c","d","f","g","h","j","k","l","m","n","p","q","r","s","t","v","w","x","y","z"};
        public static readonly string[] sarrConsonantUpper=new string[] {"B","C","D","F","G","H","J","K","L","M","N","P","Q","R","S","T","V","W","X","Y","Z"};
        public static readonly string[] sarrVowelLower=new string[] {"a","e","i","o","u"};
        public static readonly string[] sarrVowelUpper=new string[] {"A","E","I","O","U"};
        public static readonly string[] sarrWordDelimiter=new string[] {"...","--",",",";",":","/","&","#","@","$","%","_","+","=","(",")","{","}","[","]","*",">","<","|","\"","'"," ","`","^","~"}; //CAN HAVE "'" since will simply be dealt differently when contraction, after words are split apart
        public static readonly char[] carrSentenceDelimiter=new char[] {'.','!','?'};//formerly string[] sarrSentenceDelimiter
        public const int REGION_BETWEEN=0;
        public const int REGION_NAME=1;
        public const int REGION_OP=2;
        public const int REGION_VALUE=3;
        public static string RegionToString(int RegionNow) {
            if (RegionNow==REGION_BETWEEN) return "between statements";
            else if (RegionNow==REGION_NAME) return "name";
            else if (RegionNow==REGION_OP) return "operator";
            else if (RegionNow==REGION_VALUE) return "value";
            return "";
        }
        //public const int iToLower=32;
        public const char ui16ToLower=(char)32;
        public const int iToUpper=-32;
        public const char ui16UpperA='A';
        public const char ui16UpperZ='Z';
        public const char ui16LowerA='a';
        public const char ui16LowerZ='z';
         private static readonly string[] sarrCSType={      "string","int",  "bool",   "byte","sbyte","char","decimal","double","float", "uint",  "long", "ulong", "object","short","ushort"};
        private static readonly string[] sarrCSTypeMapsTo={"String","Int32","Boolean","Byte","SByte","Char","Decimal","Double","Single","UInt32","Int64","UInt64","Object","Int16","UInt16"};//usually use assumed System.*
        private static string[] sarrCSTypeMapsToFull=null;
        public static CultureInfo cultureinfo = null;
        ///<summary>
        ///Optimized non-lowercase check (one comparison operation)
        ///</summary>
        public static bool IsNotLower(char cNow) {
            return cNow<ui16LowerA;
        }
        public static bool IsUpper(char cNow) {
            return cNow<=ui16UpperZ&&cNow>=ui16UpperA;
        }
        public static bool HasAnyAlphanumericCharacters(string val) {
            bool bReturn=false;
            if (val!=null&&val.Length!=0) {
                for (int iChar=0; iChar<val.Length; iChar++) {
                    if ( IsAlphanumeric(val[iChar]) ) {
                        bReturn=true;
                        break;
                    }
                }
            }
            return bReturn;
        }//end HasAnyAlphanumericCharacters
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
            if (IsUpper(cNow)) return (char)((uint)cNow+ui16ToLower);
            else return cNow;
        }
        ///<summary>
        ///Compares two characters (case-insensitive)
        ///</summary>
        public static bool EqualsI(char c1, char c2) {
            return ToLower(c1)==ToLower(c2);
        }
        /// <summary>
        /// Compares the two strings (I is for case-insensitive overload)
        /// </summary>
        /// <param name="Needle1"></param>
        /// <param name="Needle2"></param>
        /// <returns></returns>
        public static bool EqualsI_AssumingNeedle2IsLower(string Needle1, string Needle2) { //formerly EqualsI_AssumingNeedleIsLower
            return Needle1!=null&&Needle2!=null&&CompareAtI_AssumingNeedleIsLower(Needle1,Needle2,0,Needle2.Length,false);
        }
        public static bool EqualsI(string c1, string c2) {
            return c1!=null&&c2!=null&&CompareAtI(c1,c2,0,c2.Length,false);
        }
        public static char ToUpper(char cNow) {
            if (IsLower(cNow)) return RConvert.ToChar((int)cNow+iToUpper);
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
        private int MaxCapacity {
            get { return carrData!=null?carrData.Length:0; }
            set {
                try {
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
                }
                catch (Exception e) {
                    RReporting.ShowExn(e,"setting RString capacity","rstring set MaxCapacity");
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
        public RString(char[] carrX, int start, int len) {
            From(carrX,start,len);
        }
        public RString(RString rstrX) {
            From(rstrX);
        }
        public void From(string sNow) {
            From(sNow,sNow.Length);
        }
        public void From(string sNow, int iCapacity) {
            Clear();
            Capacity=iCapacity;
            Append(sNow);
        }
        public void From(RString rstrX) {
            MaxCapacity=rstrX.MaxCapacity;
            From(rstrX.carrData,rstrX.iStart,rstrX.Length);
        }
        public void From(char[] carrX, int start, int len) {
            if (carrX!=null){
                if (start+len>carrX.Length) {
                    RReporting.Warning( String.Format("Truncating source from Start:{0}; Length:{1} to fit source character array size {2}",start,len,carrX.Length) );
                    len=carrX.Length-start;
                }
                if (len>0) {
                    MaxCapacity=carrX.Length;
                    Length=0;
                    for (int iNow=0; iNow<carrX.Length; iNow++) {
                        carrData[iEnder]=carrX[iNow];
                        iEnder++;
                    }
                }
                else {
                    RReporting.Warning( String.Format("Getting no data from character array from Start:{0}; Length:{1} and source character array size {2}",start,len,carrX.Length) );
                    Length=0;
                }
            }
            else {
                Length=0;
            }
        }
        static RString() {//static constructor
            sarrCSTypeMapsToFull=new string[sarrCSTypeMapsTo.Length];
            for (int iNow=0; iNow<sarrCSTypeMapsTo.Length; iNow++) sarrCSTypeMapsToFull[iNow]="System."+sarrCSTypeMapsTo[iNow];
        }
        #endregion constructors
        public void ReplaceAny(ref char[] sOld, char cNew) {
            ReplaceAny(ref carrData,sOld,cNew,iStart,iEnder);
        }
        
        public RString Substring(int start, int len) {
            RString rstrReturn=new RString(this);
            rstrReturn.Trim(start,len);
            return rstrReturn;
        }
        public RString Substring(int start) {
            RString rstrReturn=new RString(this);
            rstrReturn.Trim(start);
            return rstrReturn;
        }
        public static bool EndsWith(string Haystack, string Needle) {
            try {
                int iEnder=RString.SafeLength(Haystack);
                int iStart=0;
                if (Haystack!=null && Needle!=null && Needle.Length<=iEnder-iStart && Needle.Length>0) {
                    int iRel=iEnder-1;
                    for (int iNow=Needle.Length-1; iNow>=0; iNow--) {
                        if (Haystack[iRel]!=Needle[iNow]) return false;
                        iRel--;
                    }
                    return true;
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return false;
        }
        public bool EndsWith(string Needle) {
            try {
                if (carrData!=null && Needle!=null && Needle.Length<=iEnder-iStart && Needle.Length>0) {
                    int iRel=iEnder-1;
                    for (int iNow=Needle.Length-1; iNow>=0; iNow--) {
                        if (carrData[iRel]!=Needle[iNow]) return false;
                        iRel--;
                    }
                    return true;
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return false;
        }
        public bool StartsWith(string Needle) {
            try {
                if (carrData!=null && Needle!=null && Needle.Length<=iEnder-iStart && Needle.Length>0) {
                    int iRel=iStart;
                    for (int iNow=0; iNow<Needle.Length; iNow++) {
                        if (carrData[iRel]!=Needle[iNow]) return false;
                        iRel++;
                    }
                    return true;
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return false;
        }//end StartsWith
        public static int CountInstances(string Haystack, string Needle) {
            if (Haystack!=null&&Needle!=null) return CountInstances(Haystack, Needle, 0, RString.SafeLength(Haystack));
            else {
                RReporting.Debug("CountInstances: "+RReporting.StringMessage(Needle,true)+" in "+RReporting.StringMessage(Haystack,false)+" skipped.");
                return 0;
            }
        } //end CountInstances(string,string)
        ///<summary>
        ///Does NOT count redundant instances i.e. CountInstances(rrr,rr,...) finds 1, 
        /// i.e. CountInstances(rrrr,rr,...) finds 2;
        ///</summary>
        public static int CountInstances(string Haystack, string Needle, int iStart, int iEndBefore) {
            int iCount=0;
            //RReporting.Debug("CountInstances: "+RReporting.StringMessage(Needle,true)+" in "+RReporting.StringMessage(Haystack,false)+" substring at "+iStart+" ending before "+iEndBefore);
            try {
                if (Needle!=null&&Needle.Length>0) {
                    while (iStart+Needle.Length<=iEndBefore) {
                        if (CompareAt(Haystack,Needle,iStart)) {
                            iCount++;
                            //RReporting.Debug("CountInstances: "+iCount);
                            iStart+=Needle.Length;
                        }
                        else iStart++;
                    }
                }
                else RReporting.ShowErr(  "Blank string search was skipped", "", 
                  String.Format("CountInstances({0},{1})", RReporting.DebugStyle("in",Haystack ,false,false),RReporting.DebugStyle("search-for",Needle ,false,false))  );
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return iCount;
        } //end CountInstances(string,string,iStart,iEndEx)
        ///<summary>
        ///Does NOT count redundant instances i.e. CountInstances(rrr,rr,...) finds 1, 
        /// i.e. CountInstances(rrrr,rr,...) finds 2;
        ///</summary>
        public static int CountInstancesI_AssumingNeedleIsLower(string Haystack, string Needle, int iStart, int iEndBefore) {
            int iCount=0;
            //RReporting.Debug("CountInstancesI: "+RReporting.StringMessage(Needle,true)+" in "+RReporting.StringMessage(Haystack,false)+" substring at "+iStart+" ending before "+iEndBefore);
            try {
                if (Needle!=null&&Needle.Length!=0&&Haystack!=null) {
                    while (iStart+Needle.Length<=iEndBefore) {
                        if (RString.CompareAtI_AssumingNeedleIsLower(Haystack,Needle,iStart)) {
                            iCount++;
                            //RReporting.Debug("CountInstancesI: "+iCount);
                            iStart+=Needle.Length;
                        }
                        else iStart++;
                    }
                }
                else RReporting.ShowErr(  "Blank string search was skipped", "", 
                          String.Format("CountInstances({0},{1})", RReporting.DebugStyle("in",Haystack ,false,false),RReporting.DebugStyle("search-for",Needle ,false,false))  );
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return iCount;
        } //end CountInstancesI_AssumingNeedleIsLower(string,string,iStart,iEndEx)
        ///<summary>
        ///Does NOT count redundant instances i.e. CountInstances(rrr,rr,...) finds 1, 
        /// i.e. CountInstances(rrrr,rr,...) finds 2;
        ///</summary>
        public static int CountInstancesI(string Haystack, string Needle, int iStart, int iEndBefore) {
            int iCount=0;
            //RReporting.Debug("CountInstancesI: "+RReporting.StringMessage(Needle,true)+" in "+RReporting.StringMessage(Haystack,false)+" substring at "+iStart+" ending before "+iEndBefore+": \""+RString.SafeSubstringByExclusiveEnder(Haystack,iStart,iEndBefore)+"\"");
            try {
                if (Needle.Length!=0) {
                    while (iStart+Needle.Length<=iEndBefore) {
                        if (CompareAtI(Haystack,Needle,iStart)) {
                            iCount++;
                            //RReporting.Debug("CountInstancesI: "+iCount);
                            iStart+=Needle.Length;
                        }
                        else iStart++;
                    }
                }
                else RReporting.ShowErr(  "Blank string search was skipped", "", 
                  String.Format("CountInstances({0},{1})", RReporting.DebugStyle("in",Haystack ,false,false),RReporting.DebugStyle("search-for",Needle ,false,false))  );
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return iCount;
        } //end CountInstancesI(string,string,iStart,iEndEx)
         public static int CountInstances(string Haystack, char Needle) {
             if (Haystack!=null) return CountInstances(Haystack, Needle, 0,Haystack.Length);
             else return 0;
         }
        public static int CountInstances(string Haystack, char Needle, int iStart, int iEndBefore) {
            return CountInstances(Haystack, Needle, true, iStart, iEndBefore);
        }
        public static int CountInstances(string Haystack, char Needle, bool bCountEvenIfInQuotes, int iStart, int iEndBefore) {
            int iCount=0;
            //int iLocNow=0;
            //int iStartNow=0;
            bool bInQuotes=false;
            try {
                if (Haystack!=null&&Haystack!="") {
                    for (int iChar=iStart; iChar<Haystack.Length&&iChar<iEndBefore; iChar++) {
                        if (Haystack[iChar]=='"') bInQuotes=!bInQuotes;
                        else if ( Haystack[iChar]==Needle && (bCountEvenIfInQuotes||!bInQuotes) ) iCount++;
                    }
                }
                else RReporting.ShowErr("Tried to count matching characters in blank string!","","CountInstances(string,char)");
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
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
            catch (Exception e) {
                Console.Error.WriteLine("Error in RString Insert(\""+sNow+"\") {"
                    +"iStart:"+iStart.ToString()+";"
                    +"iEnder:"+iEnder.ToString()+";"
                    +"Length:"+Length.ToString()+";"
                    +"MaxCapacity:"+MaxCapacity.ToString()+";"
                    +"}");
                Console.Error.WriteLine(e.ToString());
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
            catch (Exception e) {
                Console.Error.WriteLine("Error in RString Insert('"+char.ToString(cNow)+"') {"
                    +"iStart:"+iStart.ToString()+";"
                    +"iEnder:"+iEnder.ToString()+";"
                    +"Length:"+Length.ToString()+";"
                    +"MaxCapacity:"+MaxCapacity.ToString()+";"
                    +"}");
                Console.Error.WriteLine(e.ToString());
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
            catch (Exception e) {
                bGood=false;
                Console.Error.WriteLine("RString MoveInternalStringToZero error:");
                Console.Error.WriteLine(e.ToString());
                Console.Error.WriteLine();
            }
            return bGood;
        }
        public static explicit operator string(RString val) { //explicit typecast from RString to string
            return val.ToString();
        }
        public static explicit operator RString(string val) { //explicit typecast from string to RString
            return new RString(val);
        }
        public static string operator +(RString var1, string var2) {
            return var1.ToString()+var2;
        }
        public static bool operator ==(RString a, RString b) {
            //if (a.IsNull||b==null) return false;
            try {
                return a.Equals(b);
            }
            catch {
                
            }
            return false;
        }
        public static bool operator !=(RString a, RString b) {
            if (a==null||b==null) return true;
            return !a.Equals(b);
        }
        public static bool CompareAt(RString rstr1, int iRel1, RString rstr2, int iRel2) {
            try {
                return (rstr1!=null&&rstr2!=null&&rstr1.Length>iRel1&&rstr2.Length>iRel2
                    &&rstr1.carrData[rstr1.iStart+iRel1]==rstr2.carrData[rstr2.iStart+iRel2]);
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"comparing character in RStrings (RString corruption)");
            }
            return false;
        }//end CompareAt(RString,int,RString,int)
        ///<summary>
        ///Fast comparison (works without actually modifying the Haystack param)
        ///Returns true if match AND (iStopBeforeInHaystack-iAtHaystack)==Needle.Length
        ///</summary>
        public static bool CompareAt(string Haystack, string Needle, int iAtHaystack, int iStopBeforeInHaystack, bool bAllowRegionToBeLargerThanMatch) {
            try {
                if ( Haystack!=null && IsNotBlank(Needle)) {
                    if (iAtHaystack>=0 && iAtHaystack<iStopBeforeInHaystack) {
                        if (bAllowRegionToBeLargerThanMatch||((iStopBeforeInHaystack-iAtHaystack)==Needle.Length)) {
                            int iRel=0;
                            if (bAllowRegionToBeLargerThanMatch) {
                                while (iRel<Needle.Length) {
                                    if (iAtHaystack>=iStopBeforeInHaystack || Needle[iRel]!=Haystack[iAtHaystack]) return false;
                                    iAtHaystack++;
                                    iRel++;
                                }
                            }
                            else {
                                while (iAtHaystack<iStopBeforeInHaystack) { //iRel<Needle.Length) {
                                    if (iRel>=Needle.Length || Needle[iRel]!=Haystack[iAtHaystack]) return false;
                                    iAtHaystack++;
                                    iRel++;
                                }
                            }
                            //while (bAllowRegionToBeLargerThanMatch ? (iRel<Needle.Length) : (iAtHaystack<iStopBeforeInHaystack) ) {
                                //(this line is replaced by using CompareAt() changing location>iStopBeforeInHaystack to -1 // if (bAllowRegionToBeLargerThanMatch?(iAtHaystack>=iStopBeforeInHaystack):(iRel>=Needle.Length)) return false;
                            //    if (!CompareAt(Haystack,Needle[iRel],iAtHaystack>=iStopBeforeInHaystack?-1:iAtHaystack)) return false; //if (Needle[iRel]!=Haystack[iAtHaystack]) return false;
                            //    iAtHaystack++;
                            //    iRel++;
                            //}
                            return iRel==Needle.Length;
                        }
                        else return false;
                    }
                    else {
                        if (RReporting.bDebug) RReporting.Warning("Checking location outside of range {"
                                                                  +"iAtHaystack:"+iAtHaystack.ToString()
                                                                  +"; iStopBeforeInHaystack:"+iStopBeforeInHaystack.ToString()
                                                                  +"; Needle:"+RReporting.StringMessage(Needle,true)
                                                                  +"}",
                                                                  "checking location",String.Format("CompareAt({0},{1},{2},{3})",RReporting.StringMessage(Haystack,false),RReporting.StringMessage(Needle,false),iAtHaystack,iStopBeforeInHaystack) );
                    }
                }
                else RReporting.Warning("Received null string","looking for string",String.Format("CompareAt({0},{1},{2},{3})",RReporting.StringMessage(Haystack,false),RReporting.StringMessage(Needle,false),iAtHaystack,iStopBeforeInHaystack) );
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"checking for string in string","CompareAt {"
                                   +"Haystack.Length:"+RReporting.SafeLength(Haystack)
                                   +"; Needle:"+RReporting.StringMessage(Needle,true)
                                   +"; iAtHaystack:"+iAtHaystack.ToString()
                                   +"; iStopBeforeInHaystack:"+iStopBeforeInHaystack.ToString()
                                   +"; bAllowRegionToBeLargerThanMatch:"+RConvert.ToString(bAllowRegionToBeLargerThanMatch)
                                   +"}");
            }
            return false;
        }//end CompareAt(string,string,iStart,iEndEx)
        
        public static bool CompareAtI_AssumingNeedleIsLower(string Haystack, string Needle, int iAtHaystack) {
            return CompareAtI_AssumingNeedleIsLower(Haystack,Needle,iAtHaystack,RString.SafeLength(Haystack),true);
        }
        /// <summary>
        /// Fast comparison (works without actually modifying the Haystack param)
        ///Returns true if match 
        /// </summary>
        /// <param name="Haystack"></param>
        /// <param name="Needle"></param>
        /// <param name="iAtHaystack"></param>
        /// <param name="iStopBeforeInHaystack"></param>
        /// <param name="bAllowRegionToBeLargerThanMatch">if true (iStopBeforeInHaystack-iAtHaystack) does not have to equal Needle.Length</param>
        /// <returns></returns>
        public static bool CompareAtI_AssumingNeedleIsLower(string Haystack, string Needle, int iAtHaystack, int iStopBeforeInHaystack, bool bAllowRegionToBeLargerThanMatch) {
            if (Haystack!=null&&IsNotBlank(Needle)) {
                if (bAllowRegionToBeLargerThanMatch||((iStopBeforeInHaystack-iAtHaystack)==Needle.Length)) {
                    int iRel=0;
                    while (bAllowRegionToBeLargerThanMatch ? (iRel<Needle.Length) : (iAtHaystack<iStopBeforeInHaystack) ) {
                        if (bAllowRegionToBeLargerThanMatch ? (iAtHaystack>=iStopBeforeInHaystack) : (iRel>=Needle.Length) ) return false;
                        if (Needle[iRel]!=
                            ( (Haystack[iAtHaystack]>=ui16UpperA && Haystack[iAtHaystack]<=ui16UpperZ)
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
            return false;
        }//end CompareAtI_AssumingNeedleIsLower(string,string,iStart,iEndEx)
        public static bool CompareAtI(string Haystack, string Needle, int iAtHaystack) {
            return CompareAtI(Haystack,Needle,iAtHaystack,RString.SafeLength(Haystack),true);
        }
        /// <summary>
        /// Case-sensitive overload of CompareAt that only can return true if
        /// match.
        /// </summary>
        /// <param name="Haystack"></param>
        /// <param name="Needle"></param>
        /// <param name="iAtHaystack"></param>
        /// <param name="iStopBeforeInHaystack"></param>
        /// <param name="bAllowRegionToBeLargerThanMatch">if true, iStopBeforeInHaystack-iAtHaystack may be larger than Needle.Length</param>
        /// <returns></returns>
        public static bool CompareAtI(string Haystack, string Needle, int iAtHaystack, int iStopBeforeInHaystack, bool bAllowRegionToBeLargerThanMatch) {
            try {
                if (Haystack!=null&&IsNotBlank(Needle)) {
                    if (bAllowRegionToBeLargerThanMatch||((iStopBeforeInHaystack-iAtHaystack)==Needle.Length)) {
                        int iRel=0;
                        while (bAllowRegionToBeLargerThanMatch ? (iRel<Needle.Length) : (iAtHaystack<iStopBeforeInHaystack) ) {
                            if (bAllowRegionToBeLargerThanMatch ? (iAtHaystack>=iStopBeforeInHaystack) : (iRel>=Needle.Length) ) return false;
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
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return false;
        }//end CompareAtI
        public static bool CompareAt(string Haystack, char Needle, int iAtHaystack) {
            return Haystack!=null&&iAtHaystack>=0&&iAtHaystack<Haystack.Length&&Haystack[iAtHaystack]==Needle;
        }
        //public static bool CompareAt(char[] carrHaystack, char[] carrNeedle, int iAtHaystack) {
        //    bool bReturn=false;
        //    int iAbs=iAtHaystack;
        //    int iMatches=0;
        //    try {
        //        //if (carrHaystack!=null && carrNeedle!=null) {
        //        if (carrNeedle!=null) {
        //            for (int iRel=0; iRel<carrNeedle.Length; iRel++) {
        //                if (carrNeedle[iRel]==carrHaystack[iAbs]) iMatches++;
        //                else break;
        //                iAbs++;
        //            }
        //            if (iMatches==carrNeedle.Length) {
        //                bReturn=true;
        //            }
        //        }
        //        //}
        //    }
        //    catch (Exception e) {
        //        RReporting.ShowExn(e,"comparing text substring");
        //        bReturn=false;
        //    }
        //    return bReturn;
        //}//end CompareAt(char array...);
//         public override bool Equals(RString rstrX) {
//             try {
//                 if (rstrX!=null&&rstrX.Length==Length) {
//                     int iAbsThat=rstrX.iStart;
//                     for (int iAbsMe=iStart; iAbsMe<iEnder; iAbsMe++) {
//                         if (rstrX.carrData[iAbsThat]!=carrData[iAbsMe]) return false;
//                         iAbsThat++;
//                     }
//                     return true;
//                 }
//             }
//             catch (Exeption e) {
//                 RReporting.ShowExn(e,"comparing RStrings (RString corruption)","");
//             }
//             return false;
//         }
        public static bool CompareAt(string Haystack, string Needle, int iAtHaystack) {
            return CompareAt(Haystack,Needle,iAtHaystack,RString.SafeLength(Haystack),true);
            /*
            bool bReturn=false;
            int iAbs=iAtHaystack;
            int iMatches=0;
            try {
                //if (Haystack!=null && Needle!=null) {
                if (Needle!=null&&Needle!="") {
                    for (int iRel=0;  iRel<Needle.Length && iAbs<Haystack.Length;  iRel++) {
                        if (Needle[iRel]==Haystack[iAbs]) iMatches++;
                        else break;
                        iAbs++;
                    }
                    if (iMatches==Needle.Length) bReturn=true;
                }
                //}
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"comparing text substring");
                bReturn=false;
            }
            return bReturn;
            */
        }//end CompareAt(string,string,iAt)
        public static bool StartsWith(string Haystack, string Needle) {
            return (Haystack!=null && CompareAt(Haystack,Needle,0,SafeLength(Haystack),true));
        }
        public static bool StartsWithI(string Haystack, string Needle) {
            return (Haystack!=null && CompareAtI(Haystack,Needle,0,SafeLength(Haystack),true));
        }
        public static bool StartsWithI_AssumingNeedleIsLower(string Haystack, string Needle) {
            return (Haystack!=null && CompareAtI_AssumingNeedleIsLower(Haystack,Needle,0,SafeLength(Haystack),true));
        }

        public override bool Equals(object o) {
            try {
                RString target=((RString)o);
                if (this.Length==target.Length) {
                    int iTarget=target.iStart;
                    for (int i=this.iStart; i<this.iEnder; i++) {
                        if (this.carrData[i]!=target.carrData[iTarget]) return false;
                        iTarget++;
                    }
                    return true;//DOES return true if both are zero
                }
                //return (bool) (this == (RString) o);
            }
            catch {
                
            }
            return false;
        }
        public override int GetHashCode() {
//             int iHash=0;
//             try {
//                 if (carrData!=null) {
//                     for (int iNow=iStart; iNow<iEnder; iNow++) {
//                         iHash=RMath.SafeAddWrappedTowardZero(iHash,(int)carrData[iNow]);
//                     }
//                 }
//                 else iHash=-1;
//             }
//             catch (Exception e) {
//                 RReporting.ShowExn(e);
//                 if (iHash==<1) iHash=-1;
//                 else iHash*=-1;
//             }
//             return iHash;
            return ToString().GetHashCode();
        }
        public override string ToString() {
            char[] carrReturn=RMemory.SubArray(carrData,iStart,Length);
            return new string(carrReturn);
        }
        public string ToString(int start, int len) {
            char[] carrReturn=null;
            if (start+len>Length) {
                RReporting.Warning( String.Format("Warning: ToString is truncating dest from Start:{0}; Length:{1} to fit actual RString size {2}",start,len,Length) );
                len=Length-start;
            }
            if (len>0) carrReturn=RMemory.SubArray(carrData,iStart+start,len);
            return carrReturn!=null?new string(carrReturn):"";
        }
        public void Trim(int start, int len) {
            if (start<Length) {
                if (start+len>Length) {
                    RReporting.Warning( String.Format("Warning: Trim is truncating dest from Start:{0}; Length:{1} to fit actual RString size {2}",start,len,Length) );
                    len=Length-start;
                }
                if (len>0) {
                    iStart+=start;
                    Length=len;
                }
                else Length=0;
            }
            else Length=0;
        }//end Trim
        public void Trim(int start) {
            Trim(start,this.Length-start);
        }
        /// <summary>
        /// Converts raw text to text able to exist as data inside HTML
        /// textarea or HTML property value (in which case quotes would need
        /// to be added manually later if result has spaces)
        /// </summary>
        /// <param name="sData">raw text</param>
        /// <returns>html-safe plain text</returns>
        public static string ToHtmlValue(string sData) {
            string sReturn=RString.Replace(sData,"<","&lt;");
            sReturn=RString.Replace(sReturn,">","&gt;");
            sReturn=RString.Replace(sReturn,"\"","&quot;");
            sReturn=RString.Replace(sReturn,Environment.NewLine,"<br>");
            sReturn=RString.Replace(sReturn,"\n","<br>");
            sReturn=RString.Replace(sReturn,"\r","<br>");
            return sReturn;
        }
        public static string SafeSubstring(string sValue, int start, int iLen) {
            if (sValue==null) return "";
            if (start<0) return "";
            if (iLen<1) return "";
            try {
                if (start<sValue.Length) {
                    if ((start+iLen)<=sValue.Length) return sValue.Substring(start, iLen);
                    else {
                        RReporting.Debug("Tried to return SafeSubstring(\"" + ToOneLine(sValue)+"\"," + start.ToString() + "," + iLen.ToString() + ") (area ending past end of string).");
                        return sValue.Substring(start);
                    }
                       //it is okay that the "else" also handles (start+iLen)==sValue.Length
                }
                else {
                    RReporting.Debug("Tried to return SafeSubstring(\""+sValue+"\","+start.ToString()+","+iLen.ToString()+") (starting past end).");
                    return "";
                }
            }
            catch (Exception e) {
                RReporting.Debug(e);
                return "";
            }
        }//end SafeSubstring(string,int,int)
        public static string SafeSubstring(string sValue, int start) {
            bool bGoodString=false;
            if (sValue==null) return "";
            if (start<0) return ""; 
            try {
                if (start<sValue.Length) {
                    try { sValue=sValue.Substring(start); bGoodString=true; }
                    catch { bGoodString=false; sValue=""; }
                    return sValue;
                }
                else {
                    if (RReporting.bDebug) RReporting.Debug("Tried to return SafeSubstring(\""+RString.ToOneLine(sValue)+"\","+start.ToString()+") (past end).");
                    return "";
                }
            }
            catch (Exception e) {
                RReporting.Debug(e);
                return "";
            }
        }//end SafeSubstring(string,int,int)
        public static string ToOneLine(string val) {
            return (val!=null)?(val.Replace(Environment.NewLine,"").Replace('\r',' ').Replace('\n',' ')):"";
        }
        public static int LastIndexOf_OnlyIfWholeWord(string Haystack, string Needle) {
            return LastIndexOf_OnlyIfWholeWord(Haystack,Needle,0,RString.SafeLength(Haystack));
        }
        ///<summary>
        ///Returns index of Haystack where Needle occurs without other alphanumeric
        /// characters directly before or after it, otherwise -1 if not found or if
        /// Needle is blank.
        ///</summary>
        public static int LastIndexOf_OnlyIfWholeWord(string Haystack, string Needle, int iHaystackStart, int iHaystackCount) {
            int iReturn=-1;
            if (Haystack!=null&&RString.IsNotBlank(Needle)) {
                int iEndEx=iHaystackStart+iHaystackCount;
                int iStartNow=iEndEx-Needle.Length;//start of the last position to check for needle
                while (iStartNow>=iHaystackStart) {
                    if ( CompareAt(Haystack,Needle,iStartNow)
                        && iStartNow+Needle.Length<=Haystack.Length
                        && (iStartNow-1<=iHaystackStart||!IsAlphanumeric(Haystack,iStartNow-1))
                        && (iStartNow+Needle.Length>=iEndEx||!IsAlphanumeric(Haystack,iStartNow+Needle.Length))
                        //ok if beyond range--can be whole word if no characters exist at either end of the word.
                        //--assumes that should be found as whole word even if range is counted as only area being checked
                        ) {
                        iReturn=iStartNow;
                        break;
                    }
                }
            }
            return iReturn;
        }//end LastIndexOf_OnlyIfWholeWord
        ///NOTE: SafeSubstring(RString,...) is NOT needed, because RString.ToString is already safe
        public static bool Contains(string Haystack, string Needle) {
            try {
                return RString.IndexOf(Haystack,Needle) >= 0;//Haystack.IndexOf(Needle) >= 0;
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Contains("+RReporting.StringMessage(Haystack,false)+","+RReporting.StringMessage(Needle,true)+")");
            }
            return false;
        }
        public static bool Contains(string Haystack, char Needle) {
            try {
                return IndexOf(Haystack,Needle,0) >= 0;
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Contains(string,char)");
                return false;
            }
        }
        public static bool ContainsChar(string Haystack, string Needle, int CharIndexInNeedleToFind) {
            try {
                return CharIndexInNeedleToFind>=0 && CharIndexInNeedleToFind<Needle.Length && RString.SafeLength(Needle)>0 && IndexOf(Haystack,Needle[CharIndexInNeedleToFind]) >= 0;
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Contains(string,string,IndexOfCharacterInNeedleToFind)");
                return false;
            }
        }
                
        
        /// <summary>
        /// Checks whether sNow contains cNow
        /// </summary>
        /// <param name="sNow"></param>
        /// <param name="cNow"></param>
        /// <returns></returns>
        public static bool ContainsChar(char[] sNow, char cNow) {
            if (sNow!=null) {
                for (int iNow=0; iNow<sNow.Length; iNow++) {
                    if (sNow[iNow]==cNow) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks whether sNow contains the character StringContainingOneNeedleToUse[iNeedleInStringToUse]
        /// </summary>
        /// <param name="sNow"></param>
        /// <param name="StringContainingOneNeedleToUse"></param>
        /// <param name="iNeedleInStringToUse"></param>
        /// <returns></returns>
        public static bool ContainsChar(char[] sNow, string StringContainingOneNeedleToUse, int iNeedleInStringToUse) {
            if (sNow!=null) {
                for (int iNow=0; iNow<sNow.Length; iNow++) {
                    if (sNow[iNow]==StringContainingOneNeedleToUse[iNeedleInStringToUse]) return true;
                }
            }
            return false;
        }
#region moved from Base
        //public static bool IsSpacingExceptNewLine(char val) {
        //{}
        
        //public static bool IsSpacingExceptNewLine(char val) {//replaced by IsHorizonalSpacingChar
        //    bool bYes=false;
        //    try {
        //        if (val==' ') bYes=true;
        //        else if (val=='\t') bYes=true;
        //    }
        //    catch (Exception e) {
        //        RReporting.ShowExn(e,"checking for spacing","IsSpacingExceptNewLine");
        //    }
        //    return bYes;
        //}
        
        //public static bool IsNewLine(char val) {//replaced by IsNewLineChar
        //    bool bYes=false;
        //    try {
        //        if (val=='\r') bYes=true;
        //        else if (val=='\n') bYes=true;
        //        else if (Contains(Environment.NewLine,val)) bYes=true;
        //    }
        //    catch (Exception e) {
        //        RReporting.ShowExn(e,"checking for newline","IsNewLine");
        //    }
        //    return bYes;
        //}

        
        public static bool LikeWildCard(string input, string pattern, bool bCaseSensitive) { //aka IsLike
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
            catch (Exception e) {
                RReporting.ShowExn(e);
                return false;
            }
        }
        //public static bool SafeCompare(string sValue, string Haystack, int iAtHaystackIndex) { //replaced by CompareAt
        //    bool bFound=false;
        //    try {
        //        if ( sValue==Haystack.Substring(iAtHaystackIndex, sValue.Length) ) {
        //            bFound=true;
        //        }
        //    }
        //    catch (Exception e) {
        //        RReporting.Debug(e);
        //        bFound=false;
        //    }
        //    return bFound;
        //}
        public static string SafeRemove(string sValue, int iExcludeAt, int iExcludeLen) {
            return RString.SafeSubstring(sValue,0,iExcludeAt)+RString.SafeSubstring(sValue,iExcludeAt+iExcludeLen);
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
            //sCumulative=(RString)""; //don't do this--this would set Capacity to zero!
            int iLinesFound=0;
            int CharsReadCount=0;
            if (sDataNow!=""&&sDataNow!=null) {
                int iCursor=0;
                string sLine;
                while (StringReadLine(out sLine, sDataNow, ref iCursor)) {
                    if (iLinesFound==0) {
                        CharsReadCount+=RString.SafeLength(sLine);
                        sCumulative.Append(sLine);
                    }
                    else {
                        sCumulative.Append(" ");
                        sCumulative.Append(sLine);
                        CharsReadCount+=RString.SafeLength(sLine)+1;
                    }
                    iLinesFound++;
                }
                if (sLine==""&&sCumulative.EndsWith(" ")) sCumulative.Trim(0,sCumulative.Length-1); //remove any trailing space that was ADDED ABOVE to a blank line.
            }
            if (!RReporting.IsNotBlank(sCumulative)&&RReporting.IsNotBlank(sDataNow)) RReporting.Debug("ReplaceNewLinesWithSpaces got empty string \""+sCumulative+"\" from used string \""+sDataNow+"\" {iLinesFound:"+iLinesFound+"; sCumulative.Length:"+sCumulative.Length+"; sCumulative.MaxCapacity:"+sCumulative.MaxCapacity+"; CharsReadCount:"+CharsReadCount+" }.");
            sDataNow=(string)sCumulative;
        } //end ReplaceNewLinesWithSpaces
        /// <summary>
        /// Gets the non-null equivalent of a null, empty, or nonempty string.
        /// </summary>
        /// <param name="val"></param>
        /// <returns>If Null Then return is "null-string"; if "" then return is "", otherwise
        /// value of val is returned.</returns>
         public static string SafeString(string val, bool bReturnWhyIfNotSafe) {
             try {
                 return (val!=null)
                     ?val
                     :(bReturnWhyIfNotSafe?"null-string":"");
             }
             catch { //do not report this
                 return bReturnWhyIfNotSafe?"incorrectly-initialized-string":"";
             }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="sFrom"></param>
        /// <param name="sTo"></param>
        /// <returns>Negative if failed, int.MinValue if none done, 0 or positive if good (count done)</returns>
        public static int ReplaceAll(ref string sData, string sFrom, string sTo) {
            int iReturn=0;
            try {
                if (sData.Length==0) {
                    RReporting.Debug("There is no text in which to search for replacement.","replacing text chunk","RString ReplaceAll");
                    //still returns true (0) though
                }
                else {
                    int iPlace=sData.IndexOf(sFrom);
                    //int iReplaced=0;
                    while (iPlace>-1) {
                        sData=sData.Remove(iPlace,sFrom.Length);
                        sData=sData.Insert(iPlace,sTo);
                        //if (iPlace>=0) iReplaced++;
                        iReturn++;
                        iPlace=sData.IndexOf(sFrom);
                    }
                }
            }
            catch (Exception e) {
                if (iReturn>0) iReturn*=-1;
                else iReturn=int.MinValue;
                RReporting.ShowExn(e,"replacing & counting all string instances");
                //still return # of replacements that worked
            }
            return iReturn;
        }//end ReplaceAll
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sFrom"></param>
        /// <param name="sTo"></param>
        /// <param name="sarrHaystack"></param>
        /// <returns>Negative if failed, int.MinValue if none done, 0 or positive if good (count done)</returns>
        public static int ReplaceAll(string sFrom, string sTo, string[] sarrHaystack) {
            int iReturn=0;
            try {
                if (sarrHaystack!=null) {
                    for (int iNow=0; iNow<sarrHaystack.Length; iNow++) {
                        iReturn+=ReplaceAll(ref sarrHaystack[iNow], sFrom, sTo);
                    }
                }
            }
            catch (Exception e) {
                if (iReturn>0) iReturn*=-1;
                else iReturn=int.MinValue;
                RReporting.ShowExn(e,"replacing & counting all string instances in all strings in string array");
            }
            return iReturn;
        }
        /// <summary>
        /// Removes spacing before newlines.
        /// </summary>
        /// <param name="sData"></param>
        /// <returns></returns>
        public static int RemoveSpacingBeforeNewLines(ref string sData) {
            int iFoundThisRound=1;//MUST start >0
            int iFoundTotal=0;
            try {
                while (iFoundThisRound>0) {
                    iFoundThisRound=0;
                    iFoundThisRound+=ReplaceAll(ref sData, " "+Environment.NewLine, Environment.NewLine);
                    iFoundThisRound+=ReplaceAll(ref sData, "\t"+Environment.NewLine, Environment.NewLine);
                    iFoundThisRound+=ReplaceAll(ref sData, " \n","\n");
                    iFoundThisRound+=ReplaceAll(ref sData, "\t\n", "\n");
                    iFoundThisRound+=ReplaceAll(ref sData, " \r","\r");
                    iFoundThisRound+=ReplaceAll(ref sData, "\t\r", "\r");
                    iFoundTotal+=iFoundThisRound;
                    RReporting.Debug("Removing "+iFoundThisRound.ToString()+", "+iFoundTotal.ToString()+" total...");
                }
                RReporting.Debug("Done with "+iFoundTotal.ToString()+" total spacing before newlines removed.");
            }
            catch (Exception e) {
                if (iFoundTotal>0) iFoundTotal*=-1;
                else iFoundTotal=int.MinValue;
                RReporting.ShowExn(e);
            }
            return iFoundTotal;
        }//end RemoveSpacingBeforeNewLines
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sData"></param>
        /// <returns>Negative if failed, int.MinValue if none done, 0 or positive if good (count done)</returns>
        public static int RemoveBlankLines(ref string sData) {
            return RemoveBlankLines(ref sData, false);
        }
        /// <summary>
        /// Returns total
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="bAllowTrailingNewLines"></param>
        /// <returns>Negative if failed, int.MinValue if none done, 0 or positive if good (count done)</returns>
        public static int RemoveBlankLines(ref string sData, bool bAllowTrailingNewLines) {
            int iTotal=0;
            try {//debug performance: use RString instead of string object
                int iFoundNow=1;
                string sRemoveNL=Environment.NewLine+Environment.NewLine;
                string sRemoveN="\n\n";
                string sRemoveR="\r\r";
                while (iFoundNow>0) {
                    iFoundNow=0;
                    iFoundNow+=ReplaceAll(ref sData, sRemoveNL, Environment.NewLine);
                    iFoundNow+=ReplaceAll(ref sData, sRemoveN, "\n");
                    iFoundNow+=ReplaceAll(ref sData, sRemoveR, "\r");
                    iTotal+=iFoundNow;
                    RReporting.Debug("Removing "+iFoundNow.ToString()+", "+iTotal.ToString()+" total blank...");
                }
                if (!bAllowTrailingNewLines) {
                    while (sData.EndsWith(Environment.NewLine)) {
                        sData=sData.Substring(0,sData.Length-Environment.NewLine.Length);
                        iFoundNow++;
                        iTotal++;
                        RReporting.Debug("Removing "+iFoundNow.ToString()+", "+iTotal.ToString()+" total...removed trailing blank line...");
                    }
                }
                RReporting.Debug("Done with "+iTotal.ToString()+" total blank lines removed.");
            }
            catch (Exception e) {
                if (iTotal>0) iTotal*=-1;
                else iTotal=int.MinValue;
                RReporting.ShowExn(e);
            }
            return iTotal;
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
        public static int LastIndexOf(string Haystack, char Needle) {
            if (RString.SafeLength(Haystack)>0) return LastIndexOf(Haystack,Needle,RString.SafeLength(Haystack)-1);
            else return -1;
        }
        public static int LastIndexOf(string Haystack, char Needle, int start) {//formerly IndexInSubstring
            int iReturn=-1;
            
            if (Haystack!=null&&Haystack!=""&&start>=0) {
                if (start>=Haystack.Length) start=Haystack.Length-1;
                for (int iChar=start; iChar>=0; iChar--) {
                    if (Haystack[iChar]==Needle) {
                        iReturn=iChar;
                        break;
                    }
                }
            }
            return iReturn;
        }//end LastIndexOf
        public static int IndexOfChar(string Haystack, string Needle, int start, int IndexOfCharInNeedleToFind) {
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
        public static string[] StringsFromMarkedList(string sField, string sMark) {//formerly DelimitedList
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
            catch (Exception e) {
                RReporting.ShowExn(e);
                sarrNew=new string[1];
                sarrNew[0]=sField;
            }
            return sarrNew;
        }//end StringsFromMarkedList
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
            catch (Exception e) {
                bGood=false;
                RReporting.ShowExn(e,"selecting text",
                    String.Format("SelectTo(iSelLen:{0}, sAllText:{1}, iFirstChar:{2}, iLastCharInclusive:{3}) ",
                            iSelLen,RReporting.StringMessage(sAllText,false),iFirstChar,iLastCharInclusive )
                );
            }
            return bGood;
        }//end SelectTo
        public static bool MoveToOrStayAtSpacingOrString(ref int iMoveMe, string sData, string sFindIfBeforeSpacing) {
            bool bGood=true;
            try {
                int iEOF=sData.Length;
                while (iMoveMe<iEOF) {
                    if (RString.IsWhiteSpace(sData[iMoveMe]))
                        break;
                    else if (CompareAt(sData,sFindIfBeforeSpacing,iMoveMe))
                        break;
                    else iMoveMe++;
                }
                if (iMoveMe>=iEOF) Console.Error.WriteLine("Reached end of page (MoveToOrStayAtSpacingOrString).");
            }
            catch (Exception e) {
                bGood=false;
                RReporting.ShowExn(e);
            }
            return bGood;
        } //MoveToOrStayAtSpacingOrString(ref int iMoveMe, string sData, string sFindIfBeforeSpacing) {
        /// <summary>
        /// Moves past whitespace (spaces, newlines, etc) OR SkipMe
        /// </summary>
        /// <param name="iMoveMe">Lands past the whitespace and SkipMe characters (maximum result is equal to sData.Length).</param>
        /// <param name="sData"></param>
        /// <param name="SkipMe">WhiteSpace and this are skipped</param>
        /// <returns>returns true if iMoveMe is in range (0 to less than 
        /// or equal to sData.Length) and sData is non-null</returns>
        public static bool MovePastWhiteSpaceOrChar(ref int iMoveMe, string sData, char SkipMe) { //formerly MovePastSpacingOrChar, formerly MoveToOrStayAtSkipSpacingOrChar
            bool bGood=false;
            if (sData!=null&&iMoveMe>=0) {
                if (iMoveMe<=sData.Length) bGood=true;
                while (iMoveMe<sData.Length) {
                    if (RString.IsWhiteSpace(sData[iMoveMe])||sData[iMoveMe]==SkipMe) iMoveMe++;
                    else break;
                }
            }
            return bGood;
        }//end MovePastSpacingOrChar
        public static bool MoveBackToOrStayAt(ref int iMoveMe, string sData, string sFind) {
            bool bGood=false;
            try {
                int iEOF=-1;
                while (iMoveMe>iEOF) {
                    if (CompareAt(sData,sFind,iMoveMe)) {
                        bGood=true;
                        break;
                    }
                    else iMoveMe--;
                }
                if (iMoveMe<=iEOF) {
                    RReporting.ShowErr("Reached beginning of string.","MoveBackToOrStayAt sFind");
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
                bGood=false;
            }
            return bGood;
        } //MoveBackToOrStayAt
        public static bool MoveToOrStayAt(ref int iMoveMe, string sData, string sFind) {
            bool bGood=false;
            try {
                int iEOF=sData.Length;
                while (iMoveMe<iEOF) {
                    if (CompareAt(sData,sFind,iMoveMe)) {
                        bGood=true;
                        break;
                    }
                    else iMoveMe++;
                }
                if (iMoveMe>=iEOF) {
                    RReporting.Warning("Reached end of page (MoveBackToOrStayAt).");
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"searching for text","MoveToOrStayAt(cursor,data,string)");
                bGood=false;
            }
            return bGood;
        } //MoveToOrStayAt
        public static bool MoveToOrStayAtSpacing(ref int iMoveMe, string sData) {
            bool bGood=false;
            try {
                int iEOF=sData.Length;
                while (iMoveMe<iEOF) {
                    if (RString.IsWhiteSpace(sData,iMoveMe)) {
                        bGood=true;
                        break;
                    }
                    else iMoveMe++;
                }
                if (iMoveMe>=iEOF) RReporting.Warning("Reached end of string (MoveToOrStayAtSpacing).");
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"searching for text","MoveToOrStayAtSpacing");
            }
            return bGood;
        }
        public static bool PreviousWordBreakerExceptNewLine(string sText, ref int iReturnBreaker_ElseNeg1, out bool bVisibleBreaker) {
            bool bGood=true;
            bVisibleBreaker=false;
            try {
                while (iReturnBreaker_ElseNeg1>-1) {
                    if (IsInvisibleWordBreaker_NoSafeChecks(sText[iReturnBreaker_ElseNeg1])) {
                        bVisibleBreaker=false;
                        return true;//bGood=true;
                    }
                    iReturnBreaker_ElseNeg1--;
                }
                while (iReturnBreaker_ElseNeg1>-1) {
                    if (IsVisibleWordBreaker_Unsafe(sText[iReturnBreaker_ElseNeg1])) {
                        bVisibleBreaker=true;
                        return true;//bGood=true;
                    }
                    iReturnBreaker_ElseNeg1--;
                }
            }
            catch (Exception e) {    
                bGood=false;
                RReporting.ShowExn(e,"looking for previous non-newline break",
                    String.Format("PreviousWordBreakerExceptNewLine(sText:{0},iReturnBreaker_ElseNeg1:{1})",
                    RReporting.StringMessage(sText,false),iReturnBreaker_ElseNeg1)
                );
            }
            return bGood;
        }
        private static bool IsVisibleWordBreaker_Unsafe(char val) {
            int iNow;
            for (iNow=0; iNow<carrWordBreakerVisible.Length; iNow++) {
                if (carrWordBreakerVisible[iNow]==val) {
                    return true;
                }
            }
            return false;
        }
        private static bool IsInvisibleWordBreaker_NoSafeChecks(char val) {
            int iNow;
            for (iNow=0; iNow<carrWordBreakerInvisible.Length; iNow++) {
                if (carrWordBreakerInvisible[iNow]==val) {
                    return true;
                }
            }
            return false;
        }
        public static bool IsWordBreakerExceptNewLine(char val) {
            int iNow;
            for (iNow=0; iNow<carrWordBreakerInvisible.Length; iNow++) {
                if (carrWordBreakerInvisible[iNow]==val) {
                    return true;
                }
            }
            for (iNow=0; iNow<carrWordBreakerVisible.Length; iNow++) {
                if (carrWordBreakerVisible[iNow]==val) {
                    return true;
                }
            }
            return false;
        }
        public static bool IsWordBreaker(char val) {
            return IsWordBreakerExceptNewLine(val)||RString.IsNewLineChar(val);
        }
        public static bool IsWordBreaker(string val, int iChar) {
            return iChar>=0&&val!=null&&iChar<val.Length&&IsWordBreaker(val[iChar]);
        }
        public static bool IsWordBreakerExceptNewLine(string val, int iChar) {
            return iChar>=0&&val!=null&&iChar<val.Length&&IsWordBreakerExceptNewLine(val[iChar]);
        }
        //public static string[] Explode(string val, char delimiter, bool bRemoveEndsWhiteSpace) {
        //    int iCount=RString.CountInstances(val,delimiter)+1;
        //    string[] sarrReturn=new string[iCount];
        //    int iChunk=0;
        //    int iStartNow=0;
        //    if (iCount>1) {
        //        for (int iChar=0; iChar<=val.Length; iChar++) {
        //            if (iChar==val.Length||val[iChar]==delimiter) {
        //                sarrReturn[iChunk]=RString.SafeSubstring(val,iStartNow,iChar-iStartNow);
        //                if (bRemoveEndsWhiteSpace) RString.RemoveEndsWhiteSpace(ref sarrReturn[iChunk]);
        //                iStartNow=iChar+1;
        //                iChunk++;
        //            }
        //        }
        //    }
        //    else sarrReturn[0]=val;
        //    return sarrReturn;
        //}
        public static string[] Explode(string val, char delimiter, bool bRemoveEndsWhiteSpace, string DoNotBreakAtDelimiterIfEnclosedInThis_SetThisToNullToIgnore) {
            //debug performance: conversion to string and using CompareAt in called method
            return Explode(val,char.ToString(delimiter),bRemoveEndsWhiteSpace,DoNotBreakAtDelimiterIfEnclosedInThis_SetThisToNullToIgnore);
        }
        //Version that was created from scratch in 2010 since a computer had old version of RString.cs:
        //public static string[] Explode(string val, string delimiter, bool bRemoveEndsWhiteSpace) {
        //    string[] sarrReturn=null;
        //    try {
        //        int iCount=RString.CountInstances(val,delimiter)+1;
        //        string[] sarrReturn=new string[iCount];
        //        int iChunk=0;
        //        int iStartNow=0;
        //        if (iCount>1) {
        //            for (int iChar=0; iChar<=val.Length; iChar++) {
        //                if (iChar==val.Length||RString.CompareAt(val,delimiter,iChar,val.Length,false) {
        //                    sarrReturn[iChunk]=RString.SafeSubstring(val,iStartNow,iChar-iStartNow);
        //                    if (bRemoveEndsWhiteSpace) RString.RemoveEndsWhiteSpace(ref sarrReturn[iChunk]);
        //                    iStartNow=iChar+delimiter.Length;
        //                    iChunk++;
        //                }
        //            }
        //        }
        //        else sarrReturn[0]=val;
        //    }
        //    catch (Exception e) {
        //        RReporting.ShowExn(e,"dividing string into array","Explode");
        //    }
        //    return sarrReturn;
        //}//end Explode
        public static string[] Explode(string val, string delimiter, bool bRemoveEndsWhiteSpace, string DoNotBreakAtDelimiterIfEnclosedInThis_SetThisToNullToIgnore) {
            int iCount = RString.CountInstances(val, delimiter) + 1;
            string[] sarrReturn = new string[iCount];
            int iChunk = 0;
            int iStartNow = 0;
            bool bInQuotes=false;
            if (iCount > 1) {
                for (int iChar = 0; iChar <= val.Length; iChar++) {
                    if (DoNotBreakAtDelimiterIfEnclosedInThis_SetThisToNullToIgnore!=null
                            && iChar != val.Length
                            && RString.CompareAt(val, DoNotBreakAtDelimiterIfEnclosedInThis_SetThisToNullToIgnore, iChar) ) {
                        bInQuotes=!bInQuotes;
                        iChar+=DoNotBreakAtDelimiterIfEnclosedInThis_SetThisToNullToIgnore.Length-1; //-1 since incremented by 1 at end of loop anyway
                    }
                    else if ( iChar==val.Length
                             || ( RString.CompareAt(val, delimiter, iChar) && !bInQuotes ) ) {
                        sarrReturn[iChunk] = RString.SafeSubstring(val, iStartNow, iChar - iStartNow);
                        if (bRemoveEndsWhiteSpace) RString.RemoveEndsWhiteSpace(ref sarrReturn[iChunk]);
                        iStartNow = iChar + delimiter.Length;
                        iChunk++;
                    }
                }
            }
            else sarrReturn[0] = val;
            return sarrReturn;
        }//end Explode
        //public static bool IsSpacing(string val, int iChar) { //replaced by IsHorizontalSpacingChar
        //    return (IsSpacingExceptNewLine(val[iChar])||IsNewLine(val[iChar]));
        //}
        //public static bool IsSpacing(char val) { //replaced by IsHorizontalSpacingChar
        //    return (IsSpacingExceptNewLine(val)||IsNewLine(val));
        //}
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
            int iReturn=-1;
            if (Haystack!=null&&Haystack!=""&&Needle!=null&&Needle!="") {
                while (start+Needle.Length<=Haystack.Length) {
                    if (CompareAt(Haystack,Needle,start)) {
                        iReturn=start;
                        break;
                    }
                    start++;
                }
            }
            return iReturn;
        }
        public static int SafeIndexOf(string Haystack, char Needle) {
            return SafeIndexOf(Haystack,Needle,0);
        }
        public static int SafeIndexOf(string Haystack, char Needle, int start) {
            int iReturn=-1;
            if (Haystack!=null) {
                while (start<Haystack.Length) {
                    if (Haystack[start]==Needle) {
                        iReturn=start;
                        break;
                    }
                    start++;
                }
            }
            return iReturn;
        }
        public static bool SafeCompare(string Haystack, string[] sarrMatchAny, int iHaystack) {
            bool bFound=false;
            try {
                int iNow=0;
                while (iNow<sarrMatchAny.Length) {
                    if (CompareAt(Haystack, sarrMatchAny[iNow], iHaystack)) {//if (SafeCompare(Haystack, sarrMatchAny[iNow], iHaystack)) {
                        bFound=true;
                        break;
                    }
                }
            }
            catch (Exception e) {
                RReporting.Debug(e,"","SafeCompare(sarrMatchAny...)");//do not report this, just say "no"
                bFound=false;
            }
            return bFound;
        }
        //public static bool SafeCompare(string Haystack, string Needle, int AtHaystackIndex) {
        //    return AtHaystackIndex>=0 && Haystack!=null && Haystack.Length>0 && Needle.Length>0 && AtHaystackIndex+Needle.Length<=Haystack.Length && CompareAt(Haystack, Needle, AtHaystackIndex);
        //}
        //MoveToOrStayAtAttrib: moved to RMemory
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
            catch (Exception e) {
                RReporting.ShowExn(e,"setting fixed width string",
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
            catch (Exception e) {
                RReporting.ShowExn(e,"saving text string to file",
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
                //    swNow=File.AppendText(sFileNow);
                //    swNow.Write(Marker+"MAXIMUM MESSAGES REACHED--This is the last message that will be shown: "+sMsg);
                //    swNow.Close();
                //}
            }//end AppendForeverWrite(sFile,sMsg)
            catch (Exception e) {
                try {
                    RReporting.Debug(e,"Base AppendForeverWrite","trying to append output text file");
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
            catch (Exception e) {
                RReporting.ShowExn(e);
                sToTruncate="";
            }
        }
        public static string ShrinkBy(string sToTruncate, int iBy) {
            string sReturn=sToTruncate;
            ShrinkByRef(ref sReturn,iBy);
            return sReturn;
        }
        ///<summary>
        ///Modifies start and endbefore so that there are no quotes, IF sData[start] and
        /// sData[endbefore-1] are both '"' characters; else DOES NOT MODIFY start nor endbefore
        ///start: the first character in sData
        ///endbefore: the exclusive ender in sData (the character before this is the last one)
        ///</summary>        
        public static bool ShrinkToInsideQuotes(string sData, ref int start, ref int endbefore) {
            bool bGood=false;
            try {//if (start<0) start=0; if (endbefore>sData.Length) endbefore=sData.Length;
                while (endbefore>=0) {
                    if (RString.CompareAt(sData,'"',endbefore-1)) {
                        endbefore--;
                        break;
                    }
                    endbefore--;
                }
                while (start<=endbefore) {
                    if (RString.CompareAt(sData,'"',start)) {
                        start++;
                        break;
                    }
                    start++;
                }
                if (endbefore<start) endbefore=start;
                //if ( ((endbefore-start)>=2) && sData[start]=='"' && sData[endbefore-1]=='"' ) {
                //    start++;
                //    endbefore--;
                //}
                bGood=true;
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
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
            if (sSyntaxErr!=null) RReporting.SourceErr(sSyntaxErr,"",sField);//debug figure out filename
            return sarrReturn;
        }
        public static string[] SplitScopes(string sField, char cBottomLevelScopeSeparator, bool bIncludeTrailingCommaOrSemicolon, out string sSyntaxErr) {
            string[] sarrReturn=null;
            sSyntaxErr=null;
            SplitScopes(ref sarrReturn, sField, cBottomLevelScopeSeparator, bIncludeTrailingCommaOrSemicolon, out sSyntaxErr, 0, RString.SafeLength(sField));
            //return SplitScopes(sField,cBottomLevelScopeSeparator,bIncludeTrailingCommaOrSemicolon,out sSyntaxErr, 0, RString.SafeLength(sField));
            return sarrReturn;
        }
        public static string[] SplitScopes(string sField, char cBottomLevelScopeSeparator, int start, int endbefore) {
            string sSyntaxErr=null;
            string[] sarrReturn=null;//SplitScopes(sField, cBottomLevelScopeSeparator, false, out sSyntaxErr, start, endbefore);
            SplitScopes(ref sarrReturn, sField, cBottomLevelScopeSeparator, false, out sSyntaxErr, 0, RString.SafeLength(sField));//boolean is for bIncludeTrailingCommaOrSemicolon
            if (sSyntaxErr!=null) RReporting.SourceErr(sSyntaxErr,"",sField);//debug figure out filename
            return sarrReturn;
        }

        public static int SplitParams(ref string[] sarrName, ref string[] sarrValue, string sField, char cAssignmentOperator, char cStatementDelimiter) {
            if (sField!=null) return SplitParams(ref sarrName, ref sarrValue, sField, cAssignmentOperator, cStatementDelimiter, 0, sField.Length);
            else return 0;
        }
        private static string[] sarrStatementsTemp=new string[80];
        ///<summary>
        ///Calls SplitScopes for splitting a delimited list of assignments and only 
        /// creates/enlarges arrays if necessary
        ///</summary>
        public static int SplitParams(ref string[] sarrName, ref string[] sarrValue, string sField, char cAssignmentOperator, char cStatementDelimiter, int start, int endbefore) {
            string sSyntaxErr=null;
            int iTest=SplitScopes(ref sarrStatementsTemp,sField,cStatementDelimiter,false, out sSyntaxErr,start,endbefore);
            if (sarrStatementsTemp!=null&&sarrStatementsTemp.Length>0) {
                if (sarrName==null||sarrName.Length<sarrStatementsTemp.Length) sarrName=new string[sarrStatementsTemp.Length];
                if (sarrValue==null||sarrValue.Length<sarrStatementsTemp.Length) sarrValue=new string[sarrStatementsTemp.Length];
                //TODO: check this -- is it done??
                
            }
            return iTest;
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
            try {
            if (sField!=null&&sField!="") {
                if (sarrStatementsOut==null) RMemory.Redim(ref sarrStatementsOut, 25);
                csScope=new CharStack();
                for (int iNow=start; iNow<=endbefore; iNow++) {
                    if (iNow==endbefore || (iNow<endbefore&&(sField[iNow]==cBottomLevelScopeSeparator)&&csScope.Count==0&&!bInQuotes&&!bInSingleQuotes)) {
                        sarrStatementsOut[iCount]=sField.Substring(iStartNow,iNow-iStartNow+(bIncludeTrailingCommaOrSemicolon?1:0));
                        iCount++;
                        if (iCount>=sarrStatementsOut.Length) RMemory.Redim(ref sarrStatementsOut, sarrStatementsOut.Length+sarrStatementsOut.Length/2+1);
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
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"splitting scopes in internal or external script statement","SplitScopes");
            }
            return iCount;
        }//end SplitScopes
        public static string TextSubArray(string sField, int index) {
            return TextSubArray(sField, index, RString.SafeLength(sField));
        }
        /// <summary>
        /// this is actually just a SplitCSV function that also accounts for sStarter and sEnder,
        /// and gets the location of the content of column at index
        /// --for string notation such as: sField="{{1,2,3},a,b,"Hello, There",c}"
        /// (where: sTextDelimiter=Char.ToString('"'); sFieldDelimiter=",";)
        /// --index 4 would return "c"
        /// </summary>
        /// <returns>whether subsection index was found</returns>
        public static string TextSubArray(string sField, int index, int endbefore) {
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
            catch (Exception e) {
                RReporting.ShowExn(e);
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
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return iFound;
        }//end Base SubSections
        /// <summary>
        /// Gets YYYY-MM-DD format where '-' is sDateDelimiter.
        /// Gets YYYYMMDD if sDateDelimiter is null or ""
        /// </summary>
        /// <param name="sDateDelimiter"></param>
        /// <returns></returns>
        public static string DateSixDigitOrDelimited(string sDateDelimiter) {
            if (sDateDelimiter==null) sDateDelimiter="";
            string sReturn="";
            try {
                System.DateTime dtX;
                dtX=DateTime.Now;
                sReturn=dtX.Year+sDateDelimiter;
                if (dtX.Month<10) sReturn+="0";
                sReturn+=dtX.Month.ToString()+sDateDelimiter;
                if (dtX.Day<10) sReturn+="0";
                sReturn+=dtX.Day.ToString();
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"getting current");
                sReturn="UnknownDate";
            }
            return sReturn;
        }//end DateSixDigit
        public static string DateTimePathString(bool bIncludeMilliseconds) {
            return DateTimeString(bIncludeMilliseconds,"_","_at_",".");
        }
        public static string DateTimeString(bool bIncludeMilliseconds, string sDateDelimiter, string sDateTimeSep, string sTimeDelimiter) {
            string sReturn=null;
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
            catch (Exception e) {
                RReporting.ShowExn(e,"getting current date & time");
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
            catch (Exception e) {
                RReporting.ShowExn(e,"getting current date & time");
                sReturn="UnknownTime";
            }
            return sReturn;
        }//end TimeString
        
        public static string SequenceDigits(long lFrame) {
            return SequenceDigits(lFrame, 4);
        }
        public static string SequenceDigits(int lFrame) {
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
        public static string SequenceDigits(int lFrame, int iMinDigits) {
            string sDigits;
            int lFrameDestructible=lFrame;
            int lDigit;
            int lMod=10;
            int lDivisor=1;
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
        
//         msdn:
//         C#
//         //Create a new WebClient instance.
//         WebClient myWebClient = new WebClient();
//         //Download home page data. 
//         Console.Error.WriteLine("Accessing {0} ...",  uriString);
//         //Open a stream to point to the data stream coming from the Web resource.
//         Stream myStream = myWebClient.OpenRead(uriString);
//         Console.Error.WriteLine("\nDisplaying Data :\n");
//         StreamReader sr = new StreamReader(myStream);
//         Console.Error.WriteLine(sr.ReadToEnd());
//         //Close the stream. 
//         myStream.Close();
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
                catch (Exception e) {
                    RReporting.ShowExn(e,"downloading text string from web","DownloadToString("+RReporting.StringMessage(sUrl,true)+",...)");
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"accessing site","DownloadToString("+RReporting.StringMessage(sUrl,true)+",...)");
            }
            //}
            //catch (Exception e) {
            //    RReporting.ShowExn(e,"reading site","DownloadToString("+RReporting.StringMessage(sUrl,true)+",...)");
            //}
            return sReturn;
        }
        public static string FileToString(string sFile) {
    /*
            FileStream fsIn=null;
            BinaryReader brIn=null;
            byte[] byarrData=null;
            try {
                brIn=new BinaryReader(fsIn);
                StreamReader sr;
                
                byarrData=brIn.R
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"converting file to string");
            }
            return FileToString(sFile, Environment.NewLine);
            */
            return FileToString(sFile, Environment.NewLine);
        }
        public static string FileToString(string sFile, string sInsertMeAtNewLine) {
            return FileToString(sFile, sInsertMeAtNewLine, false);
        }
        
        public static string FileToString(string sFile, string sInsertMeAtNewLine, bool bAllowLoadingNewLinesAtEndOfFile) {//formerly StringFromFile
            RReporting.sLastFile=sFile;
            StreamReader sr;
            string sDataX="";
            string sLine;
            try {
                sr=new StreamReader(sFile);
                //bool bFirst=true;
                if (sInsertMeAtNewLine==null) sInsertMeAtNewLine="";
                while ( (sLine=sr.ReadLine()) != null ) {
                    //if (bFirst==true) {
                    //    sDataX=sLine;
                    //    bFirst=false;
                    //}
                    //else
                    if ( sLine.Length>0 && (sLine[sLine.Length-1]=='\r'||sLine[sLine.Length-1]=='\n') ) sDataX+=RString.SafeSubstring(sLine,0,sLine.Length-1)+sInsertMeAtNewLine;
                    else sDataX+=sLine+sInsertMeAtNewLine;
                }
                sDataX=RString.SafeSubstring(sDataX,0,sDataX.Length-(Environment.NewLine.Length));
                if (!bAllowLoadingNewLinesAtEndOfFile) {
                    while (sDataX.EndsWith(Environment.NewLine)) {
                        sDataX=sDataX.Substring(0,sDataX.Length-Environment.NewLine.Length);
                    }
                }
                sr.Close();
                //StringToFile(sFile+".AsLoadedToString.TEST.dmp",sDataX);
                //while (sDataX.EndsWith(Environment.NewLine))
                //    sDataX=sDataX.Substring(0,sDataX.Length-(Environment.NewLine.Length));
                //bFirst=false;
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
                sDataX="";
            }
            return sDataX;
        }
        public static string StringReadLine(string sAllData, ref int iMoveMe) {
            //bool HasALine=false;
            int iNewLineLenNow=0; //bool bNewLine=false;
            string sReturn=null;
            try {
                int iStart=iMoveMe;
                if (iMoveMe<sAllData.Length) {
                    while (iMoveMe<sAllData.Length) { //i.e. could be starting at 0 when length is 1!
                        //string sTemp=SafeSubstring(sAllData,iMoveMe,Environment.NewLine.Length);
                        iNewLineLenNow=IsNewLineAndGetLength(sAllData,iMoveMe);
                        if (iNewLineLenNow>0) {//(CompareAt(sAllData,Environment.NewLine,iMoveMe)) {
                            break;
                        }
                        else iMoveMe++;
                    }
                    if (iNewLineLenNow<1) iMoveMe=sAllData.Length;//run to end if started after last newline (or there is no newline)
                    sReturn=SafeSubstring(sAllData,iStart,iMoveMe-iStart);
                }
                //RReporting.Debug("Base Read line ["+iStart.ToString()+"]toExcludingChar["+iMoveMe.ToString()+"]:"+sReturn);
                if (iNewLineLenNow>0) iMoveMe+=iNewLineLenNow;
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
                sReturn=null;
            }
            return sReturn;//HasALine;
        }//end StringReadLine
        public static bool StringReadLine(out string sReturn, string sAllData, ref int iMoveMe) {//formerly ReadLine
            sReturn=StringReadLine(sAllData,ref iMoveMe);
            return sReturn!=null;
        }//end StringReadLine


#endregion moved from Base

#region array searching moved from Base
        public static int IndexOf(string[] Haystack, string Needle) {
            return IndexOf(Haystack,Needle,0,RReporting.SafeLength(Haystack));
        }
        public static int IndexOf(string[] Haystack, string Needle, int iHaystackStart) {
            return IndexOf(Haystack,Needle,iHaystackStart,RReporting.SafeLength(Haystack));
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
            return IndexOfI(Haystack,Needle,0,RReporting.SafeLength(Haystack));
        }
        public static int IndexOfI(string[] Haystack, string Needle, int iHaystackStart) {
            return IndexOfI(Haystack,Needle,iHaystackStart,RReporting.SafeLength(Haystack));
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
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return iReturn;
        }//end IndexOf(string[],string);
        public static int IndexOfStartsWithI(string[] Haystack, string Needle) {
            int iReturn=-1;
            if (Haystack!=null&&RString.IsNotBlank(Needle)) {
                Needle=Needle.ToLower();
                for (int iNow=0; iNow<Haystack.Length; iNow++) {
                    //OK TO ASSUME NEEDLE IS LOWER SINCE IT WAS JUST MADE LOWER ABOVE
                    if ( RString.SafeLength(Haystack[iNow])>=Needle.Length && CompareAtI_AssumingNeedleIsLower(Haystack[iNow],Needle,0,RString.SafeLength(Haystack[iNow]),true) ) {
                        iReturn=iNow;
                        break;
                    }
                }
            }
            return iReturn;
        }//end IndexOfStartsWithI(string[],string);
        public static int IndexOfStartsWithI_AssumingNeedleIsLower(string[] Haystack, string Needle) {
            int iReturn=-1;
            if (Haystack!=null&&RString.IsNotBlank(Needle)) {
                for (int iNow=0; iNow<Haystack.Length; iNow++) {
                    if (RString.SafeLength(Haystack[iNow])>=Needle.Length && RString.CompareAtI_AssumingNeedleIsLower(Haystack[iNow],Needle,0,RString.SafeLength(Haystack[iNow]),true)) {
                        iReturn=iNow;
                        break;
                    }
                }
            }
            return iReturn;
        }//end IndexOfStartsWithI_AssumingNeedleIsLower((string[],string);

        public static bool Contains(ArrayList Haystack, string Needle) {
            if (Haystack!=null) {
                foreach (string val in Haystack) {
                    if (val==Needle) return true;
                }
            }
            return false;
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
                    if (Haystack[iNow]!=null && CompareAtI(Haystack[iNow],Needle,0,RString.SafeLength(Haystack[iNow]),false)) return true;
                }
            }
            return false;
        }
        public static bool ContainsI_AssumingNeedleIsLower(string[] Haystack, string Needle) {
            if (Haystack!=null&&Needle!=null) {
                for (int iNow=0; iNow<Haystack.Length; iNow++) {
                    if (Haystack[iNow]!=null && CompareAtI_AssumingNeedleIsLower(Haystack[iNow],Needle,0,RString.SafeLength(Haystack[iNow]),false)) return true;
                }
            }
            return false;
        }
        public static bool AnyStartsWithI(string[] Haystack, string Needle) {
            if (Haystack!=null&&Needle!=null) {
                for (int iNow=0; iNow<Haystack.Length; iNow++) {
                    if (StartsWithI(Haystack[iNow],Needle)) return true;
                }
            }
            return false;
        }
        public static bool AnyStartsWithI_AssumingNeedleIsLower(string[] Haystack, string Needle) {
            if (Haystack!=null&&Needle!=null) {
                for (int iNow=0; iNow<Haystack.Length; iNow++) {
                    if (StartsWithI_AssumingNeedleIsLower(Haystack[iNow],Needle)) return true;
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
#endregion array searching moved from Base


        #region utilities
        public static string Capitalized(string val) {
            string sReturn="";
            if (val!=null) {
                if (val.Length>0) {
                    sReturn=RString.SafeSubstring(val,0,1).ToUpper();
                    if (val.Length>1) {
                        sReturn+=RString.SafeSubstring(val,1);
                    }
                }
            }
            return sReturn;
        }
        public static string FileToHash(string FileName) {
            byte[] byarrReturn=null;
            string sReturn=null;
            try {
                RReporting.sParticiple="opening file";
                FileStream streamIn = new FileStream(FileName, FileMode.Open);
                RReporting.sParticiple="computing hash";
                MD5 md5 = new MD5CryptoServiceProvider();
                byarrReturn = md5.ComputeHash(streamIn);
                RReporting.sParticiple="closing file";
                streamIn.Close();
                RReporting.sParticiple="converting hash to string";
                ASCIIEncoding asciiencoding = new ASCIIEncoding();
                sReturn=asciiencoding.GetString(byarrReturn);
            }
            catch (Exception e) {
                RReporting.ShowExn(e,RReporting.sParticiple,"FileToHash");
            }
            return sReturn;
        }//end FileToHash

        public static string SeqFrameToBaseName(out int iGetFrameNumberOfCurrent, out string sFileExtension, string sSequenceImage) {
            //int iLast=0;
            int iDot=RString.LastIndexOf(sSequenceImage,'.');
            int iLastChar=iDot-1;//sOpenedFile.Length;
            sFileExtension="";
            while (iLastChar>=0&&RString.IsDigit(sSequenceImage[iLastChar])) {
                iLastChar--;
            }
            string sReturn=sSequenceImage;
            iGetFrameNumberOfCurrent=-1;
            if (iLastChar<-1) iLastChar=-1;
            if (iDot>-1) {
                sFileExtension=RString.SafeSubstring(sSequenceImage,iDot+1);
                iGetFrameNumberOfCurrent=RConvert.ToInt(RString.SafeSubstring(sSequenceImage,iLastChar+1,iDot-(iLastChar+1)));
                sReturn=RString.SafeSubstring(sSequenceImage,0,iLastChar+1);
            }
            else {
                int iDigits=0;
                int iChar=sSequenceImage.Length-1;
                while (iChar>=0) {
                    if (!RString.IsDigit(sSequenceImage[iChar])) break;
                    iDigits++;
                    iChar--;
                }
                if (iDigits>0) {
                    iGetFrameNumberOfCurrent=RConvert.ToInt(RString.SafeSubstring(sSequenceImage,RString.SafeLength(sSequenceImage)-iDigits,iDigits));
                }
                else iGetFrameNumberOfCurrent=0;
            }
            return sReturn;
        }//end SeqFrameToBaseName

        public static string FolderThenSlash(string sPath, string DirectoryDelimiter) {
            return (sPath!=null&&sPath!="")?(sPath.EndsWith(DirectoryDelimiter)?sPath:sPath+DirectoryDelimiter):DirectoryDelimiter;
        }
        public static string SlashThenFolder(string sPath, string DirectoryDelimiter) {
            return (sPath!=null&&sPath!="")?(sPath.StartsWith(DirectoryDelimiter)?sPath:DirectoryDelimiter+sPath):DirectoryDelimiter;
        }
        public static string FolderThenNoSlash(string sPath, string DirectoryDelimiter) {
            return (sPath!=null&&sPath!="")?(sPath.EndsWith(DirectoryDelimiter)?sPath.Substring(0,sPath.Length-1):sPath):"";
        }
        public static string NoSlashThenFolder(string sPath, string DirectoryDelimiter) {
            return (sPath!=null&&sPath!="")?(sPath.StartsWith(DirectoryDelimiter)?sPath.Substring(1):sPath):"";
        }
        public static string RemoteFolderThenSlash(string sPath) {
            return FolderThenSlash(sPath,"/");
        }
        public static string RemoteFolderThenNoSlash(string sPath) {
            return FolderThenNoSlash(sPath,"/");
        }
        public static string LocalFolderThenSlash(string sPath) {
            return FolderThenSlash(sPath,sDirSep);
        }
        public static string LocalFolderThenNoSlash(string sPath) {
            return FolderThenNoSlash(sPath,sDirSep);
        }

        public static int[] GetNumbers(string data, bool bAllowNegativeSign, bool bAllowDot, bool bAllowComma) {
            int[] arrReturn=null;
            int iCount=0;
            int iChar=0;
            if (RString.SafeLength(data)>0) {
                arrReturn=new int[(RString.SafeLength(data)/2>0)?(RString.SafeLength(data)/2):(1)];
                int iStartNow=0;
                while (iChar<=data.Length) {
                    if (   (iChar==data.Length)
                        ||  (!  ( RString.IsDigit(data[iChar])
                                    ||(bAllowNegativeSign&&(data[iChar]=='-'))
                                    ||(bAllowDot&&(data[iChar]=='.')) 
                                    ||(bAllowComma&&(data[iChar]==',')) )  )
                       )
                         { //if end of data or is non-digit
                        if (iChar-iStartNow>0) {
                            if (bAllowDot) arrReturn[iCount]=RConvert.ToInt(RConvert.ToDecimal(RString.SafeSubstring(data,iStartNow,iChar-iStartNow)));
                            else arrReturn[iCount]=RConvert.ToInt(RString.SafeSubstring(data,iStartNow,iChar-iStartNow));
                            iCount++;
                        }
                        iStartNow=iChar+1;
                    }
                    iChar++;
                }
            }
            if (arrReturn!=null) {
                RMemory.Redim(ref arrReturn,iCount,"RString GetNumbers");
            }
            return arrReturn;
        }//end GetNumbers int[] version
        public static int IndexOf(string Haystack, string Needle) {
            return RString.IndexOf(Haystack,Needle,0,RString.SafeLength(Haystack));
        }
        public static int IndexOf(string Haystack, string Needle, int startInHaystack) {
            return RString.IndexOf(Haystack,Needle,startInHaystack,RString.SafeLength(Haystack));
        }
        public static int IndexOf(string Haystack, string Needle, int startInHaystack, int endbeforeInHaystack) {
            int iReturn=-1;
            try {
                if (endbeforeInHaystack>RString.SafeLength(Haystack)) endbeforeInHaystack=RString.SafeLength(Haystack);
                //IGNORE THIS PROBLEM AND RETURN -1: if (startInHaystack>=RString.SafeLength(Haystack)) startInHaystack=RString.SafeLength(Haystack);
                //if (startInHaystack<0) startInHaystack=RString.SafeLength(Haystack); //THIS PROBLEM IS IGNORED AND RETURNS -1 (SEE NEXT "IF" CLAUSE)
                if (RString.SafeLength(Haystack)>0 && RString.SafeLength(Needle)>0 && startInHaystack>=0) {
                    for (int iStartNow=startInHaystack; iStartNow+Needle.Length<=Haystack.Length; iStartNow++) {
                        iReturn=0;
                        int iHaystack=iStartNow;
                        for (int iNeedle=0; iNeedle<Needle.Length; iNeedle++) {
                            if (Haystack[iHaystack]!=Needle[iNeedle]) iReturn=-1;
                            iHaystack++;
                        }
                        if (iReturn>-1) {
                            iReturn=iStartNow;
                            break;
                        }
                    }
                }
            }
            catch {
                iReturn=-1;
            }
            return iReturn;
        }//end IndexOf(string,string)
        public static int RemoveBetweenAll(ref string data, string startEncloser, string endbeforeEncloser, bool bRemoveEnclosers) {
            int iReturn=0;
            int iLT=RString.IndexOf(data,startEncloser);
            int iGT=RString.IndexOf(data,endbeforeEncloser);
            int iStartNow=0;
            while (iLT>-1&&iGT>iLT) {
                if (bRemoveEnclosers) {
                    data=RString.SafeSubstring(data,0,iLT) + RString.SafeSubstring(data,iGT+endbeforeEncloser.Length);
                    iStartNow=iLT;
                }
                else {
                    data=RString.SafeSubstring(data,0,iLT+startEncloser.Length) + RString.SafeSubstring(data,iGT);
                    iStartNow=iLT+startEncloser.Length+endbeforeEncloser.Length;
                }
                iReturn++;
                iLT=RString.IndexOf(data,startEncloser,iStartNow);
                iGT=RString.IndexOf(data,endbeforeEncloser,iStartNow);
            }
            return iReturn;
        }//end RemoveBetweenAll(ref string, string, string, bRemoveEnclosers)
        /// <summary>
        /// Treats the array as a stack and resizes it by one, then appends element at the new location.
        /// </summary>
        /// <param name="ArrayToRedim">Array where element should be placed (will be created as array[1] if null).</param>
        /// <param name="NewElement"></param>
        /// <returns></returns>
        public static bool Push(ref string[] ArrayToRedim, string NewElement) { //aka Push(string[] array
            bool bGood=false;
            try {
                if (ArrayToRedim==null||ArrayToRedim.Length<1) ArrayToRedim=new string[1];
                else RMemory.Redim(ref ArrayToRedim,ArrayToRedim.Length+1);
                ArrayToRedim[ArrayToRedim.Length-1]=NewElement;
                bGood=true;
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"pushing string to array","RString Push");
            }
            return bGood;
        }
        /// <summary>
        /// version of replace that doesn't throw an exception if the string is empty or null.
        /// </summary>
        /// <param name="Haystack"></param>
        /// <param name="OldNeedle"></param>
        /// <param name="NewNeedle"></param>
        /// <returns></returns>
        public static string Replace(string Haystack, string OldNeedle, string NewNeedle) {
            if (Haystack!=null&&Haystack!=""&&OldNeedle!=""&&OldNeedle!=null) {
                if (NewNeedle==null) NewNeedle="";
                return Haystack.Replace(OldNeedle,NewNeedle);
            }
            return Haystack;
        }
        public static bool StartsWith(string s, char c) {
            return s!=null&&s.Length>0&&s[0]==c;
        }
        public static bool EndsWith(string s, char c) {
            return s!=null&&s.Length>0&&s[s.Length-1]==c;
        }
        public static bool IsNewLineChar(char cNow) {
            return RString.ContainsChar(carrPossibleNewLineChars, cNow);
        }
        public static bool IsNewLineChar(string sNow, int iAt) {
            return RString.ContainsChar(carrPossibleNewLineChars, sNow, iAt);
        }
        public static string[] ToArray(ArrayList alData) {
            string[] arrReturn=null;
            if (alData!=null&&alData.Count>0) {
                arrReturn=new string[alData.Count];
                int i=0;
                foreach (string val in alData) {
                    arrReturn[i]=val;
                    i++;
                }
                if (i!=arrReturn.Length) {
                    RMemory.Redim(ref arrReturn,i);
                }
            }
            return arrReturn;
        }
        public static string ToString(ArrayList alData, string sFieldDelimiter, string sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues) {
            return ToString(ToArray(alData),sFieldDelimiter,sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrData"></param>
        /// <param name="sFieldDelimiter"></param>
        /// <param name="sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues">If null Or "", then the method removes field delimiters from field values</param>
        /// <returns></returns>
        public static string ToString(string[] arrData, string sFieldDelimiter, string sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues) {
            string sReturn="";
            if (arrData!=null) {
                for (int iNow=0; iNow<arrData.Length; iNow++) {
                    if (iNow!=0) sReturn+=sFieldDelimiter;
                    if (sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues!=null&&sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues!="") {
                        if (arrData[iNow]!=null) {
                            //replace "\"" with "\"\"":
                            if (arrData[iNow].Contains(sFieldDelimiter)) sReturn+=sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues+RString.Replace(arrData[iNow],sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues,sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues+sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues)+sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues;
                            else sReturn+=RString.Replace(arrData[iNow],sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues,sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues+sTextDelimiter_IfNullOrBlankThenRemoveFieldDelimitersInValues);
                        }
                        else sReturn+="NULL";//debug: it may be better to do nothing since already added field delimiter
                    }
                    else {//else no text delimiter, so erase field delimiters for each field to prevent extra fields
                        if (arrData[iNow]!=null) {
                            sReturn+=RString.Replace(arrData[iNow],sFieldDelimiter,"");
                        }
                        else sReturn+="null";//debug: it may be better to do nothing since already added field delimiter
                    }
                }
            }
            return sReturn;
        }//end ToString(string[],...)
        //public static readonly char[] carrNewLine=new char[] {'\n', '\r', Environment.NewLine[0], Environment.NewLine[Environment.NewLine.Length-1]};
        public static int IsNewLineAndGetLength(string sText, int iChar) {//moved from base
            if (sText!=null&&iChar>=0&&iChar<sText.Length) {
                if (CompareAt(sText,Environment.NewLine,iChar)) return Environment.NewLine.Length;
                else if (sText[iChar]=='\n') return 1;
                else if (sText[iChar]=='\r') return 1;
            }
            return 0;
        }
        public static bool IsHorizontalSpacingChar(char cNow) {
            return RString.ContainsChar(carrPossibleHorizontalSpacingChars, cNow);
        }
        public static bool IsHorizontalSpacingChar(string sNow, int iAt) {
            return RString.ContainsChar(carrPossibleHorizontalSpacingChars, sNow, iAt);
        }
        public static bool IsWhiteSpace(string sNow, int iAt) {
            return IsNewLineChar(sNow,iAt)||IsHorizontalSpacingChar(sNow,iAt);
        }
        public static bool IsWhiteSpaceOrChar(string sNow, int iAt, char cOrThis) {
            return IsNewLineChar(sNow,iAt)||IsHorizontalSpacingChar(sNow,iAt)||CompareAt(sNow,cOrThis,iAt);
        }
        public static bool IsWhiteSpace(char cNow) {
            return IsNewLineChar(cNow)||IsHorizontalSpacingChar(cNow);
        }
        /// <summary>
        /// Moves to or stays at whitespace starting at "start".
        /// </summary>
        /// <param name="sNow"></param>
        /// <param name="start"></param>
        /// <returns>Index equal or greater than start where whitespace occurs.
        /// Returns -1 if none found in that range.</returns>
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
        public static int IndexOfWhiteSpaceOrChar(string sNow, char cOrThis, int start) {
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
        } //end IndexOfWhiteSpaceOrChar
        public static int IndexOfWhiteSpace(string sNow) {
            return IndexOfWhiteSpace(sNow,0);
        }
        public static int IndexOfNonWhiteSpace(string sNow, int start) {
            int iReturn=-1;
            if (sNow!=null) {
                if (start<0) start=0;
                for (int iNow=start; iNow<sNow.Length; iNow++) {
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
                for (int iNow=start; iNow<sNow.Length; iNow++) {
                    if (!IsWhiteSpace(sNow[iNow])&&sNow[iNow]!=cNorThis) {
                        iReturn=iNow;
                        break;
                    }
                }
            }
            return iReturn;
        }
        public static string RemoveEndsWhiteSpace(string data) {
            int start=0;
            int endbefore=0;
            if (data!=null) {
                endbefore=data.Length;
                while (endbefore-start>0&&IsWhiteSpace(data[start])) start++;
                while (endbefore-start>0&&IsWhiteSpace(data[endbefore-1])) endbefore--;
                data=SafeSubstringByExclusiveEnder(data,start,endbefore);
            }
            return data;
        }
        public static void RemoveEndsWhiteSpace(ref string data) {//formerly RemoveEndsSpacing
            int start=0;
            int endbefore=0;
            if (data!=null) {
                endbefore=data.Length;
                while (endbefore-start>0&&IsWhiteSpace(data[start])) start++;
                while (endbefore-start>0&&IsWhiteSpace(data[endbefore-1])) endbefore--;
                data=SafeSubstringByExclusiveEnder(data,start,endbefore);
            }
        }
        public static void RemoveEndsWhiteSpace(ref RString sVal) {
            if (sVal!=null) {
                while (sVal.Length>0&&IsWhiteSpace(sVal[0])) sVal=sVal.Substring(1);
                while (sVal.Length>0&&IsWhiteSpace(sVal[sVal.Length-1])) sVal=sVal.Substring(0,sVal.Length-1);
            }
        }
        public static bool RemoveEndsHorzSpacing(ref string val) {//formerly RemoveEndsSpacingExceptNewLine
            try {
                if (val==null) val="";
                int iStart=0;
                int iEnder=val.Length-1;
                int iLength=val.Length;
                while (iLength>0&&(val[iStart]=='\t'||val[iStart]==' ')) {iStart++;iLength--;}
                while (iLength>0&&(val[iEnder]=='\t'||val[iEnder]==' ')) {iEnder--;iLength--;}
                if (iLength!=val.Length) val=SafeSubstring(val,iStart,iLength);
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
                return false;
            }
            return true;
//             try {
//                 if (val==null) val="";
//                 while (val.Length>0&&val.StartsWith(" ")) val=val.Substring(0,val.Length-1);
//                 while (val.Length>0&&val.EndsWith(" ")) val=val.Substring(0,val.Length-1);
//                 while (val.Length>0&&val.StartsWith("\t")) val=val.Substring(0,val.Length-1);
//                 while (val.Length>0&&val.EndsWith("\t")) val=val.Substring(0,val.Length-1);
//             }
//             catch (Exception e) {
//                 RReporting.ShowExn(e);
//                 return false;
//             }
//             return true;
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
            catch (Exception e) {
                RReporting.ShowExn(e);
                return false;
            }
            return true;
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
            catch (Exception e) {
                RReporting.ShowExn(e,"replacing characters");
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
            catch (Exception e) {
                RReporting.ShowExn(e,"replacing characters");
            }
            return carrOrig!=null?new string(carrOrig):null;
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
            catch (Exception e) {
                RReporting.ShowExn(e,"replacing characters");
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
            catch (Exception e) {
                RReporting.ShowExn(e,"replacing string characters");
            }
            return sSrc;
        }//end ReplaceAny(string,string,string)
        public static string FindBetweenI(string sDataX, string sOpener, string sCloser, int iStartFrom) {//case-insensitive //aka GetBetween
            int iFoundStartDummy;
            int iFoundLengthDummy;
            return FindBetweenI(out iFoundStartDummy, out iFoundLengthDummy, sDataX, sOpener, sCloser, iStartFrom);
        }
        /// <summary>
        /// Uses RString.cultureinfo to get the text between two case insensitive strings
        /// </summary>
        /// <param name="sDataX"></param>
        /// <param name="sOpener"></param>
        /// <param name="sCloser"></param>
        /// <param name="iStartFrom"></param>
        /// <returns></returns>
        public static string FindBetweenI(out int iFoundStart, out int iFoundLength, string sDataX, string sOpener, string sCloser, int iStartFrom) {//case-insensitive //aka GetBetween
            string sReturn="";
            string sVerb="starting";
            iFoundStart=-1;
            iFoundLength=0;
            try { //string.Compare(sDataX,sOpener,true/*ignore case*/)
                if (cultureinfo==null) cultureinfo = new CultureInfo( "es-ES", false );
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
                            iFoundStart=iOpener;
                            iFoundLength=iCloser-iOpener;
                            sReturn=sDataX.Substring(iOpener,iCloser-iOpener);
                            sVerb="finishing";
                        }
                    }
                    else {
                        Console.Error.WriteLine("Warning: result of search would start beyond data (looking for data after \""+sOpener+"\" at index "+iStartFrom.ToString()+" where data length is "+sDataX.Length.ToString()+")");
                    }
                }//end if any data
                else {
                    Console.Error.WriteLine("Warning: no data to search (looking for data after \""+sOpener+"\")");
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e,sVerb,"FindBetweenI (scraping value)");
            }
            return sReturn;
        }//end FindBetweenI
        public static string DetectNewLine(string sDataX) {
            string sReturn="";
            int iNow;
            //int iLoc;
            int iUsedMethods=0;
            if (sDataX!=null&&sDataX.Length>0&&carrPossibleNewLineChars!=null&&carrPossibleNewLineChars.Length>0) {
                bool[] barrUsed=new bool[carrPossibleNewLineChars.Length];
                for (iNow=0; iNow<barrUsed.Length; iNow++) barrUsed[iNow]=false;
                for (iNow=0; iNow<sDataX.Length; iNow++) {
                    for (int iMethod=0; iMethod<carrPossibleNewLineChars.Length; iMethod++) {
                        if ( sDataX[iNow]==carrPossibleNewLineChars[iMethod] && !barrUsed[iMethod] ) {
                            sReturn+=sDataX[iNow];
                            barrUsed[iMethod]=true;
                            iUsedMethods++;
                            break;
                        }
                    }
                    if (iUsedMethods>=carrPossibleNewLineChars.Length) break;
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
        public static int SplitStyle(out string[] sarrName, out string[] sarrValue, string sStyleWithoutCurlyBraces) {//formerly StyleSplit
            sarrName=null;
            sarrValue=null;
            
            return RString.SplitAssignments(ref sarrName, ref sarrValue, sStyleWithoutCurlyBraces, ':', ';', false);
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
        ///<summary>
        ///Returns true if ends with char or char then whitespace
        /// i.e. if cChar=';' then sVal ending with ";" or or that character followed by any
        /// length of any types of whitespace.
        ///</summary>
        public static bool EndsWithCharOrCharThenWhiteSpace(string sVal, char cChar, int start, int endbefore) {
            bool bReturn=false;
            if (sVal!=null) {
                for (int iNow=endbefore-1; iNow>=start; iNow--) {
                    if (iNow<0||iNow>=RString.SafeLength(sVal)) {//debug only
                        RReporting.ShowErr("Range is beyond string size","detecting end of value",
                                                String.Format("EndsWithCharOrCharThenWhiteSpace({0},{1},{2},{3}) {{CharacterIndex:{4}; called-by:{5}}}",RReporting.StringMessage(sVal,false),char.ToString(cChar),start,endbefore,iNow,(new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name) );
                        break;
                    }
                    else if (!IsWhiteSpace(sVal[iNow])) { //iNow<RString.SafeLength(sVal)&&
                        if (sVal[iNow]==cChar) bReturn=true;
                        else bReturn=false;
                        break;
                    }
                }
            }
            return bReturn;
        }//end EndsWithCharOrCharThenWhiteSpace

        /// <summary>
        /// Gets value locations from a delimited list of assignments
        /// -if sName is null, then all assignment values will be returned, otherwise returns
        /// all values where the variable is named by sName. If the referenced arrays are big
        /// enough they will not be redimensioned.
        /// </summary>
        /// <param name="NamesReturn">becomes a location array ({start, endbefore, ...} where endbefore
        /// is the character after the last character in the value).  It will mark all the 
        /// instances of sName in sDelimitedAssignments, which may not be the same case if
        /// bCaseSensitive is false.</param>
        /// <param name="ValuesReturn">becomes a location array ({start, endbefore, ...} where endbefore
        /// is the character after the last character in the value) of the values matching NamesReturn.  Where a
        /// variable is declared but not assigned, start==endbefore</param>
        /// <param name="sDelimitedAssignments"></param>
        /// <param name="sName">Only find assignments where the variable name matches this (null to find ALL assignments).</param>
        /// <param name="cAssignmentOperator">i.e. an equal sign, or i.e. ':' for style attribute assignment list</param>
        /// <param name="cStatementDelimiter">Separates assignment or declaration statements. If this is
        /// whitespace, valueless variables (i.e. valueless html properties) will be detected if there is
        /// whitespace between the name and the assignment operator.  Otherwise valueless variable declarations
        /// are only detected if cStatementDelimiter occurs before an assignment operator</param>
        /// <param name="iStart"></param>
        /// <param name="iEndBefore"></param>
        /// <param name="bCaseSensitive"></param>
        /// <returns>the count of used incedes in ValuesReturn</returns>
        public static int GetMultipleAssignmentLocations(ref int[] NamesReturn, ref int[] ValuesReturn, string sDelimitedAssignments, string sName, char cAssignmentOperator, char cStatementDelimiter, int iStart, int iEndBefore, bool bCaseSensitive) {
            RReporting.sParticiple="starting to parse assignments";
            string sName_Processed=bCaseSensitive?sName:sName.ToLower();
            int iParsingAt=iStart;
            int iNameStartNow=-1;
            int iNameEnderNow=-1;
            int iValStartNow=-1;
            //int iValEnderNow=-1;
            int TextRegionIndex=REGION_BETWEEN;
            int iCount=0;
            bool bInQuotes=false;
            bool bWhiteSpaceStatementDelimiter=IsWhiteSpace(cStatementDelimiter);
            int iMax =  
                    (sName!=null)
                    ?(bCaseSensitive?CountInstances(sDelimitedAssignments,sName,iStart,iEndBefore):CountInstancesI(sDelimitedAssignments,sName,iStart,iEndBefore))
                    :(bWhiteSpaceStatementDelimiter?(CountWhiteSpaceAreas(sDelimitedAssignments,false,iStart,iEndBefore)+1):(CountInstances(sDelimitedAssignments,cStatementDelimiter,false,iStart,iEndBefore)+1));
            try {
                RReporting.sParticiple="parsing assignments";
                //RReporting.Debug("GetMultipleAssignmentLocations...");
                if (iMax>0) {
                    //RReporting.Debug("GetMultipleAssignmentLocations...Max>0...");
                    if (ValuesReturn==null||ValuesReturn.Length<iMax*2) ValuesReturn=new int[iMax*2];
                    if (NamesReturn==null||NamesReturn.Length<iMax*2) NamesReturn=new int[iMax*2];
                    //RReporting.Debug("GetMultipleAssignmentLocations...Max>0...ParsingAt "+iParsingAt+" to "+iEndBefore+"...");
                    while (iParsingAt<=iEndBefore) {
                        switch (TextRegionIndex) {
                        case REGION_BETWEEN:
                            if (iParsingAt>=iEndBefore ||IsNeitherWhiteSpaceNorChar(sDelimitedAssignments[iParsingAt], cStatementDelimiter)) {
                                iNameStartNow=iParsingAt;
                                TextRegionIndex=REGION_NAME;
                            }
                            break;
                        case REGION_NAME:
                            if ( (iParsingAt>=iEndBefore) || (bWhiteSpaceStatementDelimiter&&IsWhiteSpace(sDelimitedAssignments[iParsingAt])) ||(!bWhiteSpaceStatementDelimiter&&(sDelimitedAssignments[iParsingAt]==cStatementDelimiter)) ) {
                                //if valueless tag
                                iNameEnderNow=iParsingAt;
                                if (sName==null ||(bCaseSensitive?CompareAt(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow,false):CompareAtI_AssumingNeedleIsLower(sDelimitedAssignments,sName_Processed,iNameStartNow,iNameEnderNow,false)) ) {
                                    NamesReturn[iCount*2]=iNameStartNow;
                                    NamesReturn[iCount*2+1]=iNameEnderNow;
                                    ValuesReturn[iCount*2]=iParsingAt;
                                    ValuesReturn[iCount*2+1]=iParsingAt;
                                    iCount++;
                                }
                                iNameStartNow=-1;
                                iNameEnderNow=-1;
                                TextRegionIndex=REGION_BETWEEN;
                            }
                            else if (IsWhiteSpace(sDelimitedAssignments[iParsingAt])||(sDelimitedAssignments[iParsingAt]==cAssignmentOperator)) {
                                iNameEnderNow=iParsingAt;
                                TextRegionIndex=REGION_OP;//allows whitespace after sign, but REGION_OP case does not allow whitespace inside value
                            }
                            break;
                        case REGION_OP:
                            if (iParsingAt>=iEndBefore ||IsNeitherWhiteSpaceNorChar(sDelimitedAssignments[iParsingAt], cAssignmentOperator)) {
                                iValStartNow=iParsingAt;//ok since not a whitespace
                                TextRegionIndex=REGION_VALUE;//ok since only comes here if name ended with an assignment operator
                            }
                            break;
                        case REGION_VALUE:
                            if (iParsingAt<iEndBefore&&sDelimitedAssignments[iParsingAt]=='"') bInQuotes=!bInQuotes;
                            else if ( iParsingAt>=iEndBefore || (bWhiteSpaceStatementDelimiter&&!bInQuotes&&IsWhiteSpace(sDelimitedAssignments[iParsingAt]))
                                    || (!bWhiteSpaceStatementDelimiter&&!bInQuotes&&(IsWhiteSpace(sDelimitedAssignments[iParsingAt])||(sDelimitedAssignments[iParsingAt]==cStatementDelimiter))) ) {
                                //iValEnderNow=iParsingAt;
                                if (sName==null ||(bCaseSensitive?CompareAt(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow,false):CompareAtI(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow,false)) ) {
                                    NamesReturn[iCount*2]=iNameStartNow;
                                    NamesReturn[iCount*2+1]=iNameEnderNow;
                                    ValuesReturn[iCount*2]=iValStartNow;
                                    ValuesReturn[iCount*2+1]=iParsingAt;//iValEnderNow
                                    iCount++;
                                    TextRegionIndex=REGION_BETWEEN;
                                }
                                iNameStartNow=-1;
                                iNameEnderNow=-1;
                                iValStartNow=-1;
                                bInQuotes=false;
                            }
                            break;
                        default:break;
                        }//end switch TextRegionIndex
                        //RReporting.Debug("GetMultipleAssignmentLocations: "+((iParsingAt>=0&&iParsingAt<RString.SafeLength(sDelimitedAssignments))?char.ToString(sDelimitedAssignments[iParsingAt]):"bad location")+" at ["+iParsingAt+"]"+RegionToString(TextRegionIndex));
                        iParsingAt++;
                    }//end while iParsingAt<=iEndBefore
                    if (iCount*2<ValuesReturn.Length) ValuesReturn[iCount*2]=-1; //add terminator if not full
                }//end if any instances
                else if (ValuesReturn!=null) {
                    string sCountNames=bCaseSensitive
                        ?("CountInstances(sDelimitedAssignments,sName,iStart,iEndBefore):"+CountInstances(sDelimitedAssignments,sName,iStart,iEndBefore).ToString())
                        :("CountInstancesI(sDelimitedAssignments,sName,iStart,iEndBefore):"+CountInstancesI(sDelimitedAssignments,sName,iStart,iEndBefore).ToString());
                    //RReporting.Debug("GetMultipleAssignmentLocations...no statements! {sName:"+RReporting.StringMessage(sName,true)+"; iMax:"+iMax+"; "+sCountNames+"; CountWhiteSpaceAreas(sDelimitedAssignments,false,iStart,iEndBefore):"+CountWhiteSpaceAreas(sDelimitedAssignments,false,iStart,iEndBefore)
                    //                +"; CountInstances(sDelimitedAssignments,cStatementDelimiter,false,iStart,iEndBefore):"+CountInstances(sDelimitedAssignments,cStatementDelimiter,false,iStart,iEndBefore)
                    //                +"; sDelimitedAssignments substring("+iStart+","+iEndBefore+"):"+RString.SafeSubstringByExclusiveEnder(sDelimitedAssignments,iStart,iEndBefore)+"}");
                    ValuesReturn[0]=-1;
                }
                else {
                    RReporting.Debug("GetMultipleAssignmentLocations...no statements and null return!");
                }
            }
            catch (Exception e) {
                if (ValuesReturn!=null&&iCount*2<ValuesReturn.Length) ValuesReturn[iCount*2]=-1;
                RReporting.ShowExn(e);
            }
            if (RReporting.bUltraDebug) RReporting.sParticiple="finished parsing assignments";
            //if (iCount<=0) RReporting.Debug("There were no statements in the assignment list (GetMultipleAssignmentValueLocations)");
            return iCount;//qReturn.GetInternalArray();
        }//end GetMultipleAssignmentLocations
        public static int GetMultipleAssignmentLocationsI(ref int[] NamesReturn, ref int[] ValuesReturn, string sDelimitedAssignments, string sName, char cAssignmentOperator, char cStatementDelimiter, int iStart, int iEndBefore) {
            return GetMultipleAssignmentLocations(ref NamesReturn, ref ValuesReturn, sDelimitedAssignments, sName, cAssignmentOperator, cStatementDelimiter, iStart, iEndBefore, false);
        }
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
        public static int GetMultipleAssignmentLocationsI_AssumingNameIsLower(ref int[] NamesReturn, ref int[] ValuesReturn, string sDelimitedAssignments, string sName, char cAssignmentOperator, char cStatementDelimiter, int iStart, int iEndBefore) {
            string sName_Processed=sName;
            int iParsingAt=iStart;
            int iNameStartNow=-1;
            int iNameEnderNow=-1;
            int iValStartNow=-1;
            int iValEnderNow=-1;
            int TextRegionIndex=REGION_BETWEEN;
            int iCount=0;
            bool bInQuotes=false;
            bool bWhiteSpaceStatementDelimiter=IsWhiteSpace(cStatementDelimiter);
            bool bCaseSensitive=false;
            int iMax  =  
                    (sName!=null)
                    ?(RString.CountInstancesI_AssumingNeedleIsLower(sDelimitedAssignments,sName,iStart,iEndBefore))
                    :(bWhiteSpaceStatementDelimiter?(CountWhiteSpaceAreas(sDelimitedAssignments,false,iStart,iEndBefore)+1):(CountInstances(sDelimitedAssignments,cStatementDelimiter,false,iStart,iEndBefore)+1));
            try {
                if (iMax>0) {
                    if (ValuesReturn==null||ValuesReturn.Length<iMax*2) ValuesReturn=new int[iMax*2];
                    if (NamesReturn==null||NamesReturn.Length<iMax*2) NamesReturn=new int[iMax*2];
                    while (iParsingAt<=iEndBefore) {
                        switch (TextRegionIndex) {
                        case REGION_BETWEEN:
                            if (iParsingAt>=iEndBefore ||IsNeitherWhiteSpaceNorChar(sDelimitedAssignments[iParsingAt], cStatementDelimiter)) {
                                iNameStartNow=iParsingAt;
                                TextRegionIndex=REGION_NAME;
                            }
                            break;
                        case REGION_NAME:
                            if ( (iParsingAt>=iEndBefore) ||IsWhiteSpace(sDelimitedAssignments[iParsingAt]) ||(sDelimitedAssignments[iParsingAt]==cStatementDelimiter) ) {
                                iNameEnderNow=iParsingAt;
                                if (sName==null ||(CompareAtI_AssumingNeedleIsLower(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow,false)) ) {
                                    ///return blank for valueless tag:
                                    NamesReturn[iCount*2]=iNameStartNow;
                                    NamesReturn[iCount*2+1]=iNameEnderNow;
                                    ValuesReturn[iCount*2]=iParsingAt;
                                    ValuesReturn[iCount*2+1]=iParsingAt;
                                    iCount++;
                                }
                                iNameStartNow=-1;
                                iNameEnderNow=-1;
                                TextRegionIndex=REGION_BETWEEN;
                            }
                            else if (sDelimitedAssignments[iParsingAt]==cAssignmentOperator) {
                                iNameEnderNow=iParsingAt;
                                TextRegionIndex=REGION_OP;//allows whitespace after sign, but REGION_OP case does not allow whitespace inside value
                            }
                            break;
                        case REGION_OP:
                            if (iParsingAt>=iEndBefore ||IsNeitherWhiteSpaceNorChar(sDelimitedAssignments[iParsingAt], cStatementDelimiter)) {
                                iValStartNow=iParsingAt;//ok since not a whitespace
                                TextRegionIndex=REGION_VALUE;//ok since only comes here if name ended with an assignment operator
                            }
                            break;
                        case REGION_VALUE:
                            if (iParsingAt<iEndBefore&&sDelimitedAssignments[iParsingAt]=='"') bInQuotes=!bInQuotes;
                            else if ( iParsingAt>=iEndBefore ||(bWhiteSpaceStatementDelimiter&&!bInQuotes&&IsWhiteSpaceOrChar(sDelimitedAssignments[iParsingAt], cStatementDelimiter)) 
                            ||(!bWhiteSpaceStatementDelimiter&&!bInQuotes&&(sDelimitedAssignments[iParsingAt]==cStatementDelimiter)) ) {
                                iValEnderNow=iParsingAt;
                                if (sName==null ||(bCaseSensitive?CompareAt(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow,false):CompareAtI(sDelimitedAssignments,sName,iNameStartNow,iNameEnderNow,false)) ) {
                                    NamesReturn[iCount*2]=iNameStartNow;
                                    NamesReturn[iCount*2+1]=iNameEnderNow;
                                    ValuesReturn[iCount*2]=iParsingAt;
                                    ValuesReturn[iCount*2+1]=iParsingAt;//TODO: finish this--is this right??
                                    iCount++;
                                    TextRegionIndex=REGION_BETWEEN;//TODO: finish this--is this right??
                                }
                                iNameStartNow=-1;
                                iNameEnderNow=-1;
                                bInQuotes=false;
                            }
                            break;
                        default:break;
                        }//end switch TextRegionIndex
                        iParsingAt++;
                    }//end while iParsingAt<=iEndBefore
                    if (iCount*2<ValuesReturn.Length) ValuesReturn[iCount*2]=-1; //add terminator if not full
                }//end if any instances
                else if (ValuesReturn!=null) ValuesReturn[0]=-1;
            }
            catch (Exception e) {
                if (ValuesReturn!=null&&iCount*2<ValuesReturn.Length) ValuesReturn[iCount*2]=-1;
                RReporting.ShowExn(e);
            }
            //if (iCount<=0) RReporting.Debug("There were no statements in the assignment list (GetMultipleAssignmentValueLocations)");
            return iCount;//qReturn.GetInternalArray();
        }//end GetMultipleAssignmentLocationsI_AssumingNameIsLower
        public static string LiteralToEscapedCString(string LiteralText, bool bAddQuotes) {
            LiteralText=RString.Replace(LiteralText,"\"","\\\"");
            if (bAddQuotes) LiteralText="\""+LiteralText+"\"";
            return LiteralText;
        }
        public static string EscapedCStringToLiteral(string CStringAssignmentValue) {
            CStringAssignmentValue=RString.Replace(CStringAssignmentValue,"\\\"","\"");
            return CStringAssignmentValue;
        }
        //public static int SplitCParamAssignments(ref string[] sarrName, ref string[] sarrValue, string sStatements) {
            
        //}
        public static int SplitAssignments(ref string[] sarrName, ref string[] sarrValue, string sStatements, char cAssignmentOperator, char cStatementDelimiter, bool bTreatEscapedQuotesAsLiterals) {//formerly StyleSplit
            return SplitAssignments(ref sarrName, ref sarrValue, sStatements, cAssignmentOperator, cStatementDelimiter, bTreatEscapedQuotesAsLiterals, 0, RString.SafeLength(sStatements));
        }
        ///<summary>
        ///Split assignments, accounting for whitespace (only allows whitespace inside non-quoted values--i.e. rgb(64, 64, 0); --if cStatementDelimiter is not a whitespace character i.e. ';' in this example)
        ///sarrName and sarrValue will only be created/recreated if necessary, so the return
        /// count may be smaller than the length of the arrays (this is to reduce reallocation)!
        ///Returns the count of how many variables were found (name and value stored in sarrName
        /// and sarrValue)
        ///</summary>
        public static int SplitAssignments(ref string[] sarrName, ref string[] sarrValue, string sStatements, char cAssignmentOperator, char cStatementDelimiter, bool bTreatEscapedQuotesAsLiterals, int start, int endbefore) {//formerly StyleSplit
            int iFound=0;
            bool bGood=true;//debug-- this is not reported
            try {
                ArrayList alNames=new ArrayList();
                ArrayList alValues=new ArrayList();
                int iChar=0;
                int iRegion=REGION_BETWEEN;
                int Name_start=-1;
                int Name_endbefore=-1;
                int Value_start=-1;
                int Value_endbefore=-1;
                bool bInQuotes=false;
                bool bEndQuote=false;
                while (iChar<=endbefore) {
                    bEndQuote=false;
                    if (iChar!=endbefore) {
                        if ( (sStatements[iChar]=='"') 
                                && (iChar-1<start||!bTreatEscapedQuotesAsLiterals||(sStatements[iChar-1]!='\\')) ) {
                            bInQuotes=!bInQuotes;                                
                            if (!bInQuotes) bEndQuote=true;
                        }
                    }
                    switch (iRegion) {
                        case REGION_BETWEEN:
                            if (iChar==endbefore||RString.IsNeitherWhiteSpaceNorChar(sStatements[iChar],cStatementDelimiter)) {
                                Name_start=iChar;
                                iRegion=REGION_NAME;
                            }
                            break;
                        case REGION_NAME:
                            if (iChar==endbefore||RString.IsWhiteSpaceOrChar(sStatements,iChar,cAssignmentOperator)) {
                                Name_endbefore=iChar;
                                alNames.Add(RString.SafeSubstringByExclusiveEnder(sStatements,Name_start,Name_endbefore));
                                iRegion=REGION_OP;
                                if (iChar==endbefore) alValues.Add("");
                            }
                            break;
                        case REGION_OP:
                            if (iChar==endbefore) {
                                alValues.Add("");
                            }
                            else if (RString.IsNeitherWhiteSpaceNorChar(sStatements[iChar],cAssignmentOperator)) {
                                Value_start=iChar;
                                if (sStatements[iChar]=='"') Value_start=iChar+1;
                                iRegion=REGION_VALUE;
                            }
                            break;
                        case REGION_VALUE:
                            if ( iChar==endbefore || (bEndQuote) || (!bInQuotes&&RString.IsWhiteSpaceOrChar(sStatements,iChar,cStatementDelimiter)) ) {
                                Value_endbefore=iChar;
                                alValues.Add(RString.SafeSubstringByExclusiveEnder(sStatements,Value_start,Value_endbefore));
                                iRegion=REGION_BETWEEN;
                            }
                            break;
                        default:break;
                    }//end iRegion
                    iChar++;
                }//end while iStartNext<sStatements.Length
                if (alNames.Count>0||alValues.Count>0) {
                    if (alNames.Count==alValues.Count) {
                        if (sarrName==null||alNames.Count>sarrName.Length) sarrName=new string[alNames.Count];
                        if (sarrValue==null||alValues.Count>sarrValue.Length) sarrValue=new string[alValues.Count];
                        for (int iPop=0; iPop<alNames.Count; iPop++) {
                            sarrName[iPop]=alNames[iPop].ToString();
                            sarrValue[iPop]=alValues[iPop].ToString();
                        }
                        iFound=alNames.Count;
                    }
                    else {
                        bGood=false;
                        Console.Error.WriteLine( String.Format("SplitAssignements error: Values/Names count do not match--names:{0}; values:{1}; assignments:{2}",RReporting.SafeCount(alNames),RReporting.SafeCount(alValues), iFound) );
                    }
                }
                else {
                    sarrName=null;
                    sarrValue=null;
                    RReporting.ShowErr("No style variables in \""+RString.SafeSubstring(sStatements,start,endbefore-start)+"\"!","SplitAssignements");
                    bGood=false;
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"","SplitAssignements");
                bGood=false;
            }
            //if (!bGood) iFound=-1;
            return iFound;
        }//end SplitAssignments
        public static int SplitAssignmentsSgml(out string[] sarrName, out string[] sarrValue, string sTagPropertiesWithNoTagNorGTSign) {//, char cAssignmentOperator, char cStatementDelimiter) {//formerly StyleSplit
            sarrName=null;
            sarrValue=null;
            return SplitAssignments(ref sarrName, ref sarrValue, sTagPropertiesWithNoTagNorGTSign, '=', ' ',false);//DOES account for other whitespace
        }//end SplitAssignmentsSgml
        public static string SafeString(string val) {
            return val!=null?val:"";
        }
        public static int SafeLength(string sValue) {    
            try {
                if (sValue!=null&&sValue!="") return sValue.Length;
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Base SafeLength(string)");
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
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return carrReturn;
        }
        public static bool IsCSTypeAt(string sData, int start, int endbefore) {
            return CSTypeToInternalTypeIndex(sData,start,endbefore)>-1;
        }
        public static bool IsCSTypeAt(string sData, ref int iCursorToMove) {
            return CSTypeToInternalTypeIndex(sData, ref iCursorToMove)>-1;
        }
        /// <summary>
        /// Checks whether the given substring is a CSharp type (i.e. int, Int32, System.Int32)
        /// </summary>
        /// <param name="sData">C code</param>
        /// <param name="start">character in sData whereat to start parsing</param>
        /// <param name="endbefore">character in sData to end before</param>
        /// <returns>type index if the string with length=endbefore-start is a type</returns>
        public static int CSTypeToInternalTypeIndex(string sData, int start, int endbefore) { //formerly CSTypeAtToInternalTypeIndex
            int iReturn=-1;
            for (int iNow=0; iNow<sarrCSType.Length&&iReturn<0; iNow++) {
                if (RString.CompareAt(sData,sarrCSType[iNow],start,endbefore,false)) iReturn=iNow;
            }
            for (int iNow=0; iNow<sarrCSTypeMapsTo.Length&&iReturn<0; iNow++) {
                if (RString.CompareAt(sData,sarrCSTypeMapsTo[iNow],start,endbefore,false)) iReturn=iNow;
            }
            if (sData.StartsWith("System.")) {
                for (int iNow=0; iNow<sarrCSTypeMapsToFull.Length&&iReturn<0; iNow++) {
                    if (RString.CompareAt(sData,sarrCSTypeMapsToFull[iNow],start,endbefore,false)) iReturn=iNow;
                }
            }
            return iReturn;
        }//end CSTypeToInternalTypeIndex
        ///<summary>
        ///Returns type index if there is a CSharp Type (i.e. int, Int32, System.Int32) at iCursorToMove
        ///iCursorToMove: The location to compare to a CSharp type--the variable will be
        /// changed to the location after the typename ONLY IF there is a typename at iCursorToMove.  
        /// The new location may be a '[' symbol if it is a CS array, or it may be any other character location or the end (==Length).
        ///</summary>
        public static int CSTypeToInternalTypeIndex(string sData, ref int iCursorToMove) {
            int iReturn=-1;
            int iNow;
            for (iNow=0; iNow<sarrCSType.Length&&iReturn<0; iNow++) {
                if (RString.CompareAt(sData,sarrCSType[iNow],iCursorToMove)) {
                    iReturn=iNow;
                    iCursorToMove+=sarrCSType[iReturn].Length;
                }
            }
            for (iNow=0; iNow<sarrCSTypeMapsTo.Length&&iReturn<0; iNow++) {
                if (RString.CompareAt(sData,sarrCSTypeMapsTo[iNow],iCursorToMove)) {
                    iReturn=iNow;
                    iCursorToMove+=sarrCSType[iReturn].Length;
                }
            }
            if (sData.StartsWith("System.")) {
                for (iNow=0; iNow<sarrCSTypeMapsToFull.Length&&iReturn<0; iNow++) {
                    if (RString.CompareAt(sData,sarrCSTypeMapsToFull[iNow],iCursorToMove)) {
                        iReturn=iNow;
                        iCursorToMove+=sarrCSType[iReturn].Length;
                    }
                }
            }
            return iReturn;
        }//end CSTypeToInternalTypeIndex
//         public static readonly string[] sarrCSTypeHtmlTagword={};
//         public static readonly string[] sarrCSTypeHtmlInputType={};
//         public static readonly string[] sarrCSTypeHtmlAdditionalTag={};
//         public static string CSTypeToHtmlFormTagword(string sCSType) {
//         }
//         public static string CSTypeToHtmlFormInputTypeProperty(string sCSType) {
//         }
//         public static bool CSTypeFormInputNeedsLabel(string sCSType) {
//         }


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
                    if (bInSpace&&IsAlphanumeric(sData[iNow])) {
                        bInSpace=false;
                        iCountNames++;
                    }
                    else if (!bInSpace&&!IsAlphanumeric(sData[iNow])) {
                        bInSpace=true;
                    }
                }
            }
            return iCountNames==1&&!IsDigit(sData[iStart]);
        }//end ContainsOneCSymbolAndNothingElse
        private const int CDeclPart_Type=0;//must be 0--this*2 and this*2+1 used as indeces!
        private const int CDeclPart_Name=1;//must be 1--this*2 and this*2+1 used as indeces!
        private const int CDeclPart_Value=2;//must be 2--this*2 and this*2+1 used as indeces!
        private const int CDeclPart_BetweenParts=3;
    
        /// <summary>
        ///sData must be a c or c# declaration in the form "type name_withnospaces=value",
        /// "name_withnospaces=value", or "type name"
        ///iarrParts returns a list of locations in the form {startType, endbeforeType,
        /// startName, endbeforeName, startValue, endbeforeValue}.  If any of the parts
        /// of the assignment don't exist, start will equal endbefore.
        ///Returns count (count*2 is the element count of iarrParts that is used)
        /// NOTE: does NOT handle indexer with space in it
        /// </summary>
        /// <param name="iarrParts">this is set to the halfindeces of the parts of the declaration (use RString.SafeSubstringByExclusiveEnder(s,RString.PartA(CDeclPart_Name),RString.PartB(CDeclPart_Name)) to get name and same for _Name and _Value) </param>
        /// <param name="sData"></param>
        /// <param name="start"></param>
        /// <param name="endbefore"></param>
        public static void CSDeclSplit(ref int[] iarrParts, string sData, int start, int endbefore) {//aka SplitCDecl
            int iState=CDeclPart_Type;
            int iNow=start;
            int endbeforeNow=start;
            bool bInQuotes=false;
            int iBraceDepth=0;
            int iParenDepth=0;
            bool bInSingleQuotes=false;
            bool bFoundBeginningOfTypeArea=false;
            //bool bFinalizedSomethingAtTypeArea=false;
            //bool bFoundType=false;
            if (iarrParts==null||iarrParts.Length<6) iarrParts=new int[6];
            for (int iSetNow=0; iSetNow<6; iSetNow++) { //debug optimization: use memfill if porting to C++
                iarrParts[iSetNow]=-1;
            }
            try {
                if (sData!=null) {
                    while (iNow<=endbefore) {
                        switch (iState) {
                            case CDeclPart_Type:
                                if (iNow>=endbefore) {
                                    bFoundBeginningOfTypeArea=true;
                                    if (iarrParts[0]<0) {
                                        iarrParts[0]=endbefore;
                                        iarrParts[1]=endbefore;
                                    }
                                    else iarrParts[1]=iNow;
                                    iarrParts[2]=endbefore;
                                    iarrParts[3]=endbefore;
                                    iarrParts[4]=endbefore;
                                    iarrParts[5]=endbefore;
                                    //bFinalizedSomethingAtTypeArea=true;
                                    //setting to endbefore causes moving past declaration (especially if iarrParts[5] is being used to move the parsing position)
                                }
                                else if (!RString.IsWhiteSpace(sData[iNow])&&sData[iNow]!='=') { //'=' shouldn't happen unless the type wasn't specified and this is really the name already (see "else" case)
                                    //is part of a word
                                    if (iarrParts[CDeclPart_Type*2]<0) {
                                        bFoundBeginningOfTypeArea=true;
                                        iarrParts[CDeclPart_Type*2]=iNow;
                                        endbeforeNow=iarrParts[CDeclPart_Type*2];
                                        int iDebug_endbeforeNow_1st=endbeforeNow;
                                        int iTypeFound_Temp=CSTypeToInternalTypeIndex(sData, ref endbeforeNow);//DOES move iNow to after type ONLY IF type is there
                                        //iNow=endbeforeNow;//NOTE: iNow is set below as needed (including going past "[]")
                                        //int iDoNothing=(iTypeFound<0)?CTypeToInternalCTypeIndex(sData,ref iNow):-1;
                                        if (iTypeFound_Temp>=0) {//IsCSTypeAt(sData,ref endbeforeNow)) {
                                            //bFoundType=true;
                                            if (endbeforeNow>=endbefore) {
                                                iarrParts[1]=endbefore;//end the endbefore at it's maximum possible position
                                                iarrParts[2]=endbefore;//there is no name
                                                iarrParts[3]=endbefore;//there is no name
                                                iarrParts[4]=endbefore;//there is no value
                                                iarrParts[5]=endbefore;//there is no value
                                            }
                                            else if (sData[endbeforeNow]=='[') {
                                                if (endbeforeNow+1>=endbefore) {
                                                    iarrParts[1]=endbefore;
                                                    RReporting.SourceErr("Expected ']' after '[' in variable declaration, but reached end of statement first","",RString.SafeSubstring(sData,start,endbefore));
                                                    iNow=endbefore;
                                                }
                                                else if (sData[endbeforeNow+1]==']') {
                                                    iarrParts[1]=endbeforeNow+2;
                                                    iNow=endbeforeNow+1;//incremented again at end of loop to go past ']'
                                                }
                                                else {
                                                    iarrParts[1]=iNow;
                                                    RReporting.SourceErr("Expected ']' after '[' in variable declaration (array size specifier is not allowed by engine)","",RString.SafeSubstring(sData,start,endbefore));
                                                    iNow=endbeforeNow;
                                                }
                                                iState=CDeclPart_Name;
                                            }//end if '[' (not end of file)
                                            else if (RString.IsWhiteSpace(sData[endbeforeNow])) {
                                                iarrParts[1]=endbeforeNow;
                                                iState=CDeclPart_Name;
                                                iNow=endbeforeNow;
                                            }
                                            //else it is not really a CS type so do NOT set iNow--instead, read the area as a name (allow "else whitespace" case below to end it)
                                        }//end if Is CS Type
                                        else {
                                            RReporting.Warning("Declaration did not start with a CSharp type {start:"+start.ToString()+"; iNow:"+iNow.ToString()+"; endbeforeNow:"+endbeforeNow.ToString()+"; iDebug_endbeforeNow_1st:"+iDebug_endbeforeNow_1st.ToString()+"}:\""+RString.SafeSubstringByExclusiveEnder(sData,start,endbefore)+"\"","parsing CSharp declaration","CSDeclSplit");
                                            //do nothing and allow "else whitespace" case below end the word (it will be read as a name instead of a type)
                                        }
                                    }//end if FIRST non-whitespace (if iarrParts[0]<0)
                                    //else do nothing and keep looking for a whitespace (see "else whitespace" below) to end the word (it will be read as a name instead of a type)
                                }//end if not whitespace
                                else {//else whitespace or '=' (and not end) -- shouldn't really be '-' but could be if user didn't specify a type
                                    if (bFoundBeginningOfTypeArea) {
                                        bool bTypeAreaHadAlreadyBeenSet=false;
                                        if (iarrParts[CDeclPart_Type*2+1]<0) //if was not set manually above (above should skip this case)
                                            iarrParts[CDeclPart_Type*2+1]=iNow;
                                        else bTypeAreaHadAlreadyBeenSet=true;
                                        if (RString.IsCSTypeAt(sData,iarrParts[CDeclPart_Type*2],iarrParts[CDeclPart_Type*2+1])) {
                                            iState=CDeclPart_Name;
                                            RReporting.ShowErr("Had not found the type that is specified here so parsing continued (this should never happen)","parsing CDeclaration","CDeclSplit");
                                        }
                                        else if (bTypeAreaHadAlreadyBeenSet) {
                                            iState=CDeclPart_Name;
                                            RReporting.ShowErr("Already found type but parsing continued (this should never happen)","parsing CDeclaration","CDeclSplit");
                                        }
                                        else {//else set the type to "", and set the name to the substring being terminated by this whitespace or '='
                                            iarrParts[CDeclPart_Type*2+1]=iarrParts[CDeclPart_Type*2];//set type to zero-length since no type was found
                                            iarrParts[CDeclPart_Name*2]=iarrParts[CDeclPart_Type*2];//set name to the beginning of the substring where the type was supposed to start
                                            iarrParts[CDeclPart_Name*2+1]=iNow;//terminate the name here at this whitespace
                                            RReporting.SourceErr("Expected type specifier before variable","parsing declaration",String.Format("CDeclSplit(){{name:{0}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3])));
                                            iState=CDeclPart_Value;
                                        }
                                        //if (iarrParts[CDeclPart_Type*2]>-1) {//If is not end, is whitespace, if beginning found
                                        //}
                                    //    //if (bFoundType) {//whitespace but type not found, so variable name must have been first
                                    //    //    iState=CDeclPart_Name;
                                    //    //}
                                    //    //else {
                                    //    //it is ok to force this event (and NOT check bFoundType) since iNow is changed when type is changed
                                    //    //}
                                    }
                                    //else do nothing since must be whitespace before a type
                                }//end else whitespace (neither end nor nonwhitespace)
                                break;
                            case CDeclPart_Name:
                                if (iNow>=endbefore) {
                                    if (iarrParts[CDeclPart_Name*2]<0) iarrParts[CDeclPart_Name*2]=iNow;
                                    iarrParts[CDeclPart_Name*2+1]=iNow;
                                    iarrParts[CDeclPart_Value*2]=iNow;
                                    iarrParts[CDeclPart_Value*2+1]=iNow;
                                }//end if ended
                                else {
                                    if (iarrParts[CDeclPart_Name*2]>-1) { //if found beginning
                                        if (RString.IsWhiteSpaceOrChar(sData[iNow],'=')) {
                                            iarrParts[CDeclPart_Name*2+1]=iNow;
                                            iState=CDeclPart_Value;
                                        }//end if end
                                    }//end if found beginning
                                    else if (!RString.IsWhiteSpace(sData[iNow])) {//else have not found beginning
                                        iarrParts[CDeclPart_Name*2]=iNow;
                                        if (RString.IsDigit(sData[iNow])) {
                                            RReporting.SourceErr("Variable name shouldn't have started with digit","parsing declaration", String.Format("CDeclSplit(){{type:{0};digit:{1}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]),char.ToString(sData[iNow])) );
                                        }
                                        else if (sData[iNow]=='=') {//formerly used SourceErr(iAbsoluteLocation,sMsg,sParticiple,sFunction,
                                            RReporting.SourceErr("Expected variable name but found equal sign","",iNow,"parsing declaration", String.Format("CDeclSplit(){{type:{0}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1])) );
                                            iarrParts[CDeclPart_Name*2+1]=iNow;
                                            iState=CDeclPart_Value;
                                        }
                                    }//end else detect as beginning
                                }//end else not ended
                                break;
                            case CDeclPart_Value:
                                if (iNow>=endbefore) {
                                    if (iarrParts[CDeclPart_Value*2]<0) {
                                        iarrParts[CDeclPart_Value*2]=iNow;
                                        RReporting.SourceErr("Expected value but found end of data", "", iNow, "parsing declaration",   String.Format( "CDeclSplit(){{type:{0};name:{1}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]),RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3]) )  );
                                    }
                                    iarrParts[CDeclPart_Value*2+1]=endbefore;
                                }//end if at end
                                else if (iarrParts[CDeclPart_Value*2]>-1) {//if already found start
                                    if (!bInQuotes&&(sData[iNow]=='\''&&(iNow==0||sData[iNow-1]!='\\'))) {
                                        if (bInSingleQuotes) {
                                            bInSingleQuotes=!bInSingleQuotes;
                                            if (iBraceDepth<=0) {
                                                iarrParts[CDeclPart_Value*2+1]=iNow+1;
                                                iNow=endbefore+1;//exits outer loop
                                            }
                                        }
                                    }
                                    else if (!bInQuotes&&!bInSingleQuotes&&sData[iNow]=='}') {
                                        iBraceDepth--;
                                        if (iBraceDepth<=0) {
                                            iarrParts[CDeclPart_Value*2+1]=iNow+1;
                                            iNow=endbefore+1;//exits outer loop
                                        }
                                    }
                                    else if (!bInQuotes&&!bInSingleQuotes&&sData[iNow]=='{') {
                                        iBraceDepth++;
                                    }
                                    else if (!bInSingleQuotes&&sData[iNow]=='"'&&(iNow==0||sData[iNow-1]!='\\')) {
                                        if (!bInQuotes) {
                                            if (iBraceDepth<=0) RReporting.SourceErr("Unexpected quotemark--should only see quotemark if variable began with quotemark, bracket, or single-quote OR if quote is a literal preceded by a backslash","",iNow);
                                        }
                                        else if (iBraceDepth<=0) { //force end if depth=0 and endquote found
                                            iarrParts[CDeclPart_Value*2+1]=iNow+1;
                                            iNow=endbefore+1;//exits outer loop
                                        }
                                        bInQuotes=!bInQuotes;
                                    }
                                    else if (!bInQuotes&&!bInSingleQuotes&&( (iBraceDepth<=0&&(IsWhiteSpace(sData[iNow])||sData[iNow]==','))||sData[iNow]==';')) {
                                        iarrParts[CDeclPart_Value*2+1]=iNow;
                                        if (sData[iNow]==';'&&iBraceDepth>0) {
                                            RReporting.SourceErr("Found semicolon before end of '{' braces", "",  iNow, "parsing declaration",  String.Format( "CDeclSplit(){{type:{0};name:{1}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]),RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3]) )  );
                                        }
                                        else if (sData[iNow]==',') {
                                            RReporting.SourceErr("Should not have received trailing comma in declaration (parser corruption)","",iNow);
                                        }
                                        iNow=endbefore+1;//exits outer loop
                                    }
                                    
                                }//end if already found beginning
                                else if (!RString.IsWhiteSpaceOrChar(sData[iNow],'=')) {
                                    iarrParts[CDeclPart_Value*2]=iNow;
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
                                                        iarrParts[CDeclPart_Value*2+1]=iNow+1;
                                                        iNow=endbefore+1;//exits outer loop
                                                    }
                                                }
                                            }//end while iNow<endbefore finding end of constructor
                                            if (iNow==endbefore) {
                                                RReporting.SourceErr("Incomplete constructor after \"new\"", "", iNow, "parsing declaration",   String.Format( "CDeclSplit(){{type:{0};name:{1}; declaration:{2}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]), RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3]), RString.SafeSubstringByExclusiveEnder(sData,iStart,iParen) )  );
                                                iarrParts[CDeclPart_Value*2+1]=endbefore;
                                                iNow=endbefore+1;//exits outer loop
                                            }
                                        }//end if is a constructor call
                                        else iNow=-1;
                                        if (iNow<0) {
                                            iNow=endbefore+1;//exits outer loop
                                            RReporting.SourceErr("Expected constructor parameters or array notation (i.e. string[] {\"1\",\"2\"}) after \"new\"","",iNow, "parsing declaration",   String.Format( "CDeclSplit(){{type:{0};name:{1}}}",RString.SafeSubstringByExclusiveEnder(sData,iarrParts[0],iarrParts[1]),RString.SafeSubstringByExclusiveEnder(sData,iarrParts[2],iarrParts[3]) )  );
                                        }
                                    }//end if "new " operator
                                }//else detect as beginning (not whitespace && not '=')
                                break;
                            default:
                                break;
                        }//end switch current detected segment type
                        iNow++;
                    }//end while iNow<=endbefore
                }//end if sData!=null
                else RReporting.ShowErr("Tried to split CDecl but text data was null.","parsing declaration","CDeclSplit(...)");
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
        }//end CDeclSplit
        public static string CSDeclTypeSubstring(string sCSharp_Declaration) {
            if (sCSharp_Declaration!=null) {
                if (iarrDeclPartsTemp==null) iarrDeclPartsTemp=new int[6];
                CSDeclSplit(ref iarrDeclPartsTemp, sCSharp_Declaration, 0, RString.SafeLength(sCSharp_Declaration));
                return RString.SafeSubstringByExclusiveEnder(sCSharp_Declaration,iarrDeclPartsTemp[0],iarrDeclPartsTemp[1]);
            }
            return "";
        }
        public static string CSDeclNameSubstring(string sCSharp_Declaration) {
            if (sCSharp_Declaration!=null) {
                if (iarrDeclPartsTemp==null) iarrDeclPartsTemp=new int[6];
                CSDeclSplit(ref iarrDeclPartsTemp, sCSharp_Declaration, 0, RString.SafeLength(sCSharp_Declaration));
                return RString.SafeSubstringByExclusiveEnder(sCSharp_Declaration,iarrDeclPartsTemp[2],iarrDeclPartsTemp[3]);
            }
            return "";
        }
        public static string CSDeclValueSubstring(string sCSharp_Declaration) {
            if (sCSharp_Declaration!=null) {
                if (iarrDeclPartsTemp==null) iarrDeclPartsTemp=new int[6];
                CSDeclSplit(ref iarrDeclPartsTemp, sCSharp_Declaration, 0, RString.SafeLength(sCSharp_Declaration));
                return RString.SafeSubstringByExclusiveEnder(sCSharp_Declaration,iarrDeclPartsTemp[4],iarrDeclPartsTemp[5]);
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
            set { RMemory.Redim(ref carr,value); }
        }
        public CharStack() {
            Init(iDefaultMax);
        }
        public void Init(int iSetMax) {
            try {
                carr=new char[iSetMax];
            }
            catch (Exception e) {
                RReporting.ShowExn( e,"creating character stack",String.Format("Init({0})",iSetMax) );
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
            catch (Exception e) {
                RReporting.ShowExn( e,"adding to character stack",String.Format("Push({0}){{iUsed:{1}; Capacity:{2}}}",valAdd,iUsed,Capacity) );
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
            catch (Exception e) {
                RReporting.ShowExn( e,"getting character from stack",String.Format("Pop(){{iUsed:{0}; Capacity:{1}}}",iUsed,Capacity) );
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
            catch (Exception e) {
                RReporting.ShowExn( e,"viewing character in stack",String.Format("Peek({0}){{iUsed:{1}; Capacity:{2}; iAt:{3}}}",iAt,iUsed,Capacity,iAt) );
            }
            return Nothing;
        }
        public char PeekTop() {
            if (iUsed>0) return Peek(iUsed-1);
            else return Nothing;
        }
    }//end CharStack
}//end namespace
