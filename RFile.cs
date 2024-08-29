// created on 6/4/2005 at 4:49 AM

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading; //for Sleep
//using System.Windows.Forms; //only for message box (remove)

//using REAL = System.Double;//System.Single //TODO:NEVER IMPLEMENT THIS IN BYTER OR BYTER-FILE-SAVING CHAOS WILL ENSUE!

//variable suffixes:
// U:uint L:long UL:ulong F:float D:double(optional,implied) M:decimal(128-bit)

namespace ExpertMultimedia {
    #region Class Documentation
    /// <summary>
    /// Simple Memory/File Buffering system.
    /// </summary>
    /// <remarks>
    /// Forces little-endian reads/writes to RFile.byarr and disk
    /// <p>Written by Jake Gustafson</p>
    /// <p>Big-endian systems haven't been tested,
    /// so go to www.expertmultimedia.com and notify if problems.</p>
    /// <p>Keep in mind that real Unicode documents usually start with the
    /// number of characters, i.exn. the two bytes {0xFF,0xFE} (255,254)
    /// </p>
    /// </remarks>
    #endregion Class Documentation
    public class RFile {//formerly Byter
        
        #region of variables
        
        public int iLastFileSize;
        public int iLastCount;
        public int iLastCountDesired; //i.exn. =4 if int was read/written
        private int iPlace;
        private int iPlaceTemp;
        public string sFileNow="1.Noname.RFile.raw";
        public char[] carrChunkType=new char[] {'R','I','F','F'};
        public string[] sarrKnownType=null; //TODO: when setting this, automatically change byte[] to byte[numberofbytesgohere] as necessary.  The array would, for example, contain new string[]{uint,uint,ushort,ushort}
        public static readonly RDataSet[] riffKnownLeaf=new RDataSet[]{new RDataSet("FMT ", new string[]{"ushort","ushort","uint","uint","ushort","ushort","ushort"}, new string[]{"AudioMethod","ChannelCount","SampleRate","AverageBytesPerSecond","Chunks Per Sample Slice aka BlockAlign (= SignificantBitsPerSample / 8 * NumChannels)","SignificantBitsPerSample","Number of Method Bytes That Follow (always padded to multiple of 2)"}),new RDataSet("DATA",new string[]{"byte[]"},new string[]{"RawWaveData"})};
        public RDataSet dataset=null;//this class describes an array of raw data
        //all 3 arrays must match REPLACED BY riffKnownLeaf:
        //public static readonly string[] sarrKnownRiffLeaf=new string[]{
        //    "FMT ",
        //    "DATA"
        //};
        //public static readonly string[][] s2dKnownRiffLeafFormat=new string[][] {
        //    {"ushort","ushort","uint","uint","ushort","ushort","ushort"},
        //    {"byte[]"}
        //};
        //public static readonly string[][] s2dKnownRiffLeafEasyName=new string[][] {
        //    {"AudioMethod","ChannelCount","SampleRate","AverageBytesPerSecond","BlockAlign","SignificantBitsPerSample","Unused"},
        //    {"RawWaveData"}
        //};
        
        public static readonly string[] sarrKnownRiffBranch=new string[]{"WAVE"};//TODO: finish this (and implement, considering any leaves and/or branches found under it.
        //TODO: also make constants for WAVE_LENGTHVAR_START WAVE_LENGTHVAR_SIZE in wave chunk relative to file start?
        
        bool bOverflow=false;//TODO: implement this -- tracks whether the last variable saved was too big/small and set to max/min
        public FileStream fsSave=null;
        public FileInfo fiLoad=null;
        public FileStream fsLoad=null;
        public int Position {
            get {
                return iPlace;
            }
            set {
                Seek(value);
            }
        }
        public string ChunkType {
            get {
                string sCumulative="";
                if (carrChunkType!=null) {
                    for (int iNow=0; iNow<4; iNow++) { 
                        if (iNow<carrChunkType.Length) sCumulative+=char.ToString(carrChunkType[iNow]); //ok since length is enforced below
                    }
                }
                else sCumulative="????";//should never happen
                if (sCumulative.Length<4) {
                    int iAdder=4-sCumulative.Length;
                    for (int iNow=0; iNow<iAdder; iNow++) {
                        sCumulative+=" ";
                    }
                }
                return sCumulative;
            }
            set {
                if (carrChunkType==null || carrChunkType.Length!=4) {
                    carrChunkType=new char[4];
                }
                if (value!=null) {
                    for (int iNow=0; iNow<4; iNow++) {
                        if (iNow<value.Length) carrChunkType[iNow]=value[iNow];
                        else carrChunkType[iNow]=' ';
                    }
                }
                else carrChunkType=new char[]{'?','?','?','?'};//should NEVER happen
            }
        }//end ChunkType
        private int iBuffer; //Buffer size as written to the file.
        public byte[] byarr=null; //Buffer
        public RFile[] byterarr=null; //riff chunks (if null assume IsLeaf)
        public bool IsLeaf {//IsRiffLeaf
            get {
                return byterarr==null;
            }
        }
        /// <summary>
        /// The length of the buffer.  If you want to change the maximum
        /// length, save this value, increase it to your maximum,
        /// then change it back.  The length of byarr will become
        /// the highest size.
        /// </summary>
        public int Length {
            get {
                return iBuffer;
            }
            set {
                try {
                    if (value>byarr.Length) {
                        byte[] byarrTemp=new byte[value];
                        if (RMemory.CopyFast(ref byarrTemp, ref byarr,0,0,(byarr.Length<value)?byarr.Length:value)) {
                            byarrTemp[value-1]=byarrTemp[value-1]; //test for exception
                            byarr=byarrTemp;
                            byarr[value-1]=byarr[value-1]; //test for exception
                        }
                        else RReporting.ShowErr("Can't copy old buffer to new buffer.","setting RFile Length","RFile set Length="+value.ToString());
                    }
                    iBuffer=byarr.Length;
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"resizing to byter","set byter Length="+value.ToString());
                }
                
            }
        }
        /// <summary>
        /// Makes sure that the buffer notes that at least 
        /// this many bytes are used.
        /// </summary>
        /// <param name="iPosition"></param>
        public void UsedCountAtLeast(int iBufferLengthNow) {
            if (iBufferLengthNow>iBuffer) {
                iBuffer=iBufferLengthNow;
                if (iBuffer>byarr.Length) {
                    iBuffer=byarr.Length;
                    //TODO: note overflow
                }
            }
        }
        /// <summary>
        /// Makes sure that the buffer notes that the data byte
        /// at the given position is marked as used (by updating
        /// the iBuffer [length] var).
        /// </summary>
        /// <param name="iPosition"></param>
        /// <returns>Returns whether the position is valid.</returns>
        public bool UseAndValidate(int iPosition) {
            bool bGood=true;
            if (iPosition>iBuffer) {
                iBuffer=iPosition;
                if (iBuffer>byarr.Length) {
                    iBuffer=byarr.Length;
                    bGood=false;
                }
            }
            else if (iPosition<0) bGood=false;
            return bGood;
        }
        public bool ValidReadPosition(int iPosition) {
            return (iPosition<iBuffer)?((iPosition>=0)?true:false):false;
        }
        public bool ValidWritePosition(int iPosition) {
            return (iPosition<byarr.Length)?((iPosition>=0)?true:false):false;
        }
        #endregion variables
        
        #region constructors
        public RFile() {
            Init(1024*1024); //1MB default size
        }
        public RFile(int iBytesTotalDesired) {
            Init(iBytesTotalDesired);
        }
        public static RFile Create(string sFileToLoadCompletelyToMemory) {//formerly FromFile
            RFile byterNew=null;
            //try {
                byterNew=new RFile();
                byterNew.Load(sFileToLoadCompletelyToMemory);
            //}
            //catch (Exception exn) {
                //TODO: handle exception
            //}
            return byterNew;
        }
        public void Init(int iBytesTotalDesired) {
            iPlace=0;
            try {
                byarr=new byte[iBytesTotalDesired];
                iBuffer=0;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Init(iBuffer:"+iBytesTotalDesired.ToString()+")");
            }
        }
        #endregion constructors
        
        #region utility methods
        public char[] ToBase64() {
            return RConvert.ToBase64(byarr);///TODO: Output all Riff chunks
        }
        public bool ResetTo(int iNumOfBytes) {
            try {
                byte[] byarrTemp=new byte[iNumOfBytes];
                byarrTemp[iNumOfBytes-1]=byarrTemp[iNumOfBytes-1]; //test for exception
                byarr=byarrTemp;
                byarr[iNumOfBytes-1]=byarr[iNumOfBytes-1]; //test for exception
                iBuffer=0;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"resizing byter","ResetTo("+iNumOfBytes.ToString()+")");
                return false;
            }
            return true;
        }
        private bool OpenLoad(string sPathFile) {
            bool bGood=false;
            try {
                fiLoad=new FileInfo(sPathFile);
                iLastFileSize=(int)fiLoad.Length; //convert from Long
                if ((long)iLastFileSize!=fiLoad.Length)
                    RReporting.ShowErr("File length exceeded 32-bit integer!");
                fsLoad = new FileStream(sPathFile, 
                    FileMode.Open, FileAccess.Read);
                bGood=true;
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"opening file in byter","OpenLoad(\""+sPathFile+"\")");
            }
            return bGood;
        }
        private bool CloseLoad(string sPathFile) {//argument is for REFERENCE ONLY
            bool bGood=false;
            try {
                fsLoad.Close();
                fsLoad=null;
                bGood=true;
            }
            catch (Exception exn) {
                bGood=false;
                fsLoad=null;
                RReporting.ShowExn(exn,"closing byter file","CloseLoad(\""+sPathFile+"\")");
            }
            return bGood;
        }
        private bool LoadBytes(string sPathFile, int iBytes) {//argument is for REFERENCE ONLY
            bool bGood=false;
            try {
                byarr=new byte[iBytes];//TODO: make this optional???
                int iTest=fsLoad.Read(byarr,0,iBytes);
                if (iTest<iBytes) {
                    bGood=false;
                    RReporting.ShowErr("File appears to be missing data","loading fixed number of bytes",String.Format("LoadBytes({0},{1}){{loaded-bytes:{3}",sPathFile,iBytes,iTest) );
                }
            }
            catch (Exception exn) {
                bGood=false;
                fsLoad=null;
                iLastFileSize=0;
                RReporting.ShowExn(exn,"closing file after reading", "RFile LoadBytes(\""+sPathFile+"\","+iBytes.ToString()+")");
            }
            return bGood;
        }//end LoadBytes
        public bool LoadRiff(string sPathFile) {
            bool bGood=false;
            bool bGoodRiff=false;
            sFileNow=sPathFile;
            if (!File.Exists(sPathFile)) {
                RReporting.ShowErr("File doesn't exist","",String.Format("LoadRiff(\"{0}\")",sPathFile));
                return false;
            }
            try {
                bGood=OpenLoad(sPathFile);
                RFile byterHeader=new RFile(8);
                if (byterHeader.LoadBytes(sPathFile+" (header)",8)) {
                    byterHeader.Position=0;
                    string sType=byterHeader.GetForcedAscii(4);
                    uint dwExpect=byterHeader.GetForcedUint();
                    if (sType.ToUpper()!="RIFF") {
                        RReporting.ShowErr("No riff header.");
                        bGoodRiff=false;
                    }
                    else if (dwExpect!=(uint)iLastFileSize-8) {
                        RReporting.ShowErr("Riff bytes do not match file size","",
                        String.Format("LoadRiff({0}){{Expected:8+{1}; FileSize:{2}}}",
                            sPathFile,dwExpect,iLastFileSize) );
                        bGoodRiff=false;
                    }
                    if (bGoodRiff) {
                        Position=8;
                        bGoodRiff=LoadBytes(sPathFile,iLastFileSize-8);
                    }
                    else {
                        Position=0;
                        RReporting.ShowErr("Loading using old (raw non-riff) method.");
                        bGood=LoadBytes(sPathFile,iLastFileSize);
                    }
                    if (!CloseLoad(sPathFile)) bGood=false;
                }
                else bGoodRiff=false;
                this.Position=0;
                this.iBuffer=iLastFileSize;
            }
            catch (Exception exn) {
                bGood=false;
                bGoodRiff=false;
                byarr=null;
                RReporting.ShowExn(exn,"writing file","RFile LoadRiff("+sPathFile+")");
            }
            return bGoodRiff;
        }//end LoadRiff
        public bool Load(string sPathFile) {
            bool bGood=false;
            sFileNow=sPathFile;
            if (!File.Exists(sPathFile)) {
                RReporting.ShowErr("File doesn't exist","", String.Format("RFile Load(\"{0}\")",sPathFile) );
                return false;
            }
            try {
                if (!OpenLoad(sPathFile)) bGood=false;
                if (!LoadBytes(sPathFile,iLastFileSize)) bGood=false;
                if (!CloseLoad(sPathFile)) bGood=false;
                this.Position=0;
                this.iBuffer=iLastFileSize;
            }
            catch (Exception exn) {
                bGood=false;
                byarr=null;
                RReporting.ShowExn(exn,"writing file","Load("+sPathFile+")");
            }
            return bGood;
        }//end Load
        public bool Save() {
            return Save(sFileNow);
        }
        private bool OpenSaveForceRecreate(string sPathFile) {
            bool bGood=false;
            try {
                if (File.Exists(sPathFile)) File.Delete(sPathFile);
                fsSave = new FileStream(sPathFile, 
                    FileMode.Create, FileAccess.Write);
                bGood=true;
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"open file for writing", "OpenSaveForceRecreate(\""+sPathFile+"\")");
            }
            return bGood;
        }
        private bool CloseSave(string sPathFile) {//argument is for REFERENCE ONLY
            bool bGood=false;
            try {
                fsSave.Close();
                fsSave=null;
                bGood=true;
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"closing file after writing","CloseSave(\""+sPathFile+"\")");
            }
            return bGood;
        }
        private bool SaveBytes(string sPathFile) {//argument is for REFERENCE ONLY
            bool bGood=false;
            try {
                fsSave.Write(byarr,0,iBuffer);
                iLastFileSize=iBuffer;
                bGood=true;
            }
            catch (Exception exn) {
                bGood=false;
                iLastFileSize=0;
                RReporting.ShowExn(exn,"closing file after writing","SaveBytes(\""+sPathFile+"\")");
            }
            return bGood;
        }
        public bool Save(string sPathFile) {
            bool bGood=false;
            try {
                iLastFileSize=0;
                bGood=OpenSaveForceRecreate(sPathFile);//debug overwrites always
                bGood=SaveBytes(sPathFile);
                bGood=CloseSave(sPathFile);
                if (iLastFileSize==0) {
                    RReporting.ShowErr("last file size is zero!","saving file", String.Format("byter Save(\"{0}\")",sPathFile) );
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"writing file","Save("+sPathFile+")");
            }
            return bGood;
        }
        public bool SaveRiffTree(string sPathFile) {//sPathFile is FOR REFERENCE ONLY
            bool bGood=false;
            if (IsLeaf) {
                //save chunk opener
                RFile byterHeader=new RFile(8);
                string sChunkType=ChunkType;
                byterHeader.WriteAscii(ref sChunkType);//i.e. RIFF
                byterHeader.Write((uint)iBuffer);
                byterHeader.fsSave=fsSave;
                bGood=byterHeader.SaveBytes(sPathFile+" (chunk opener)");
                //save data
                bGood=SaveBytes(sPathFile);//DOES set iLastFileSize
                if (iLastFileSize==0) {
                    RReporting.ShowErr("last file size is zero!","saving file chunk","byter SaveRiffTree");
                }
                else if (iLastFileSize!=iBuffer) {
                    RReporting.ShowErr("incorrect file chunk length saved!","saving file chunk", String.Format("byter SaveRiffTree(){{saved:{0}, expected {1}}}", iLastFileSize,iBuffer) );
                }
            }
            else {
                for (int iNow=0; iNow<byterarr.Length; iNow++) {
                    if (!byterarr[iNow].SaveRiffTree(sPathFile)) bGood=false;
                }
            }
            return bGood;
        }
        public bool SaveRiff(string sPathFile) {
            bool bGood=false;
            try {
                iLastFileSize=0;
                bGood=OpenSaveForceRecreate(sPathFile);//debug: overwrites always
                bGood=SaveRiffTree(sPathFile);
                bGood=CloseSave(sPathFile);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"writing file","Save("+sPathFile+")");
            }
            return bGood;
        }
        public bool AppendToFile(string sPathFile) {
            if (!File.Exists(sPathFile)) return false;
            FileStream fsAppend;
            try {
                iLastFileSize=0;
                fsAppend = new FileStream(sPathFile, 
                    FileMode.Append, FileAccess.Write);
                fsAppend.Write(byarr,0,iBuffer);
                iLastFileSize=iBuffer;
                fsAppend.Close();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"writing file","AppendToFile("+sPathFile+")");
                return false;
            }
            return true;
        }//end AppendToFile
        public bool AppendToFileOrCreate(string sPathFile) {
            FileStream fsAppendOrCreate;
            try {
                //FileInfo fiAppend;
                //fiAppend.Position=
                iLastFileSize=0;
                fsAppendOrCreate = new FileStream(sPathFile, 
                    FileMode.Append, FileAccess.Write);
                fsAppendOrCreate.Write(byarr,0,iBuffer);
                iLastFileSize=iBuffer;
                fsAppendOrCreate.Close();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"writing file","AppendOnly("+sPathFile+")");
                return false;
            }
            return true;
        }//end AppendOnly
        public bool Seek(int iByte) {
            try {
                iPlace=iByte;
                byarr[iPlace]=byarr[iPlace]; //test for exception
                return true;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Seek("+iByte.ToString()+")");
            }
            return false;
        }//end Seek
#endregion utility methods

        #region peek methods
        public void Peek(ref float val) { //Single, float, float32
            iLastCountDesired=4;//SizeOf(val);
            iLastCount=0;
            iPlaceTemp=iPlace;
            try {
                if (BitConverter.IsLittleEndian) {
                    val=BitConverter.ToSingle(byarr, iPlaceTemp);
                }
                else {
                    byte[] byarrNow=RMemory.SubArrayReversed(byarr, iPlaceTemp, iLastCountDesired);
                    val=BitConverter.ToSingle(byarrNow, 0);
                }
                iPlaceTemp+=iLastCountDesired;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek float");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void Peek(ref double val) { //Double, double, float64
            iLastCountDesired=8;//SizeOf(val);
            iLastCount=0;
            iPlaceTemp=iPlace;
            try {
                if (BitConverter.IsLittleEndian) {
                    val=BitConverter.ToDouble(byarr, iPlaceTemp);
                }
                else {
                    byte[] byarrNow=RMemory.SubArrayReversed(byarr, iPlaceTemp, iLastCountDesired);
                    val=BitConverter.ToDouble(byarrNow, 0);
                }
                iPlaceTemp+=iLastCountDesired;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek double");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void PeekInt24(ref int val) {
            iLastCountDesired=3;
            iLastCount=0;
            iPlaceTemp=iPlace;
            bool bCompliment=false;
            try {
                if ((byarr[iPlaceTemp+2]&(byte)0x80) != (byte)0) bCompliment=true;

                val=(bCompliment)?(int)(byarr[iPlaceTemp]^(byte)0xFF):(int)byarr[iPlaceTemp];
                iPlaceTemp++;
                val+=(bCompliment)?((int)(byarr[iPlaceTemp]^(byte)0xFF))*256:((int)byarr[iPlaceTemp])*256;
                iPlaceTemp++;
                val+=(bCompliment)?((int)(byarr[iPlaceTemp]^(byte)0xFF))*65536:((int)byarr[iPlaceTemp])*65536;
                iPlaceTemp++;
                if (bCompliment) {
                    val*=-1;
                    val-=1;
                    //(AFTER loading, so that the resulting absolute value is now one higher than 
                    // the stored absolute value of a negative number,
                    // which is stored in twos compliment)                
                }
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek int24");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void Peek(ref int val) {
            iLastCountDesired=4;
            iLastCount=0;
            iPlaceTemp=iPlace;
            //bool bCompliment=false;
            try {
                if (BitConverter.IsLittleEndian) {
                    val=BitConverter.ToInt32(byarr, iPlaceTemp);
                }
                else {
                    byte[] byarrNow=RMemory.SubArrayReversed(byarr, iPlaceTemp, iLastCountDesired);
                    val=BitConverter.ToInt32(byarrNow, 0);
                }
                iPlaceTemp+=iLastCountDesired;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek int");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void Peek(ref long val) {
            iLastCountDesired=8;//SizeOf(val);
            iLastCount=0;
            iPlaceTemp=iPlace;
            //bool bCompliment=false;
            try {
                if (BitConverter.IsLittleEndian) {
                    val=BitConverter.ToInt64(byarr, iPlaceTemp);
                }
                else {
                    byte[] byarrNow=RMemory.SubArrayReversed(byarr, iPlaceTemp, iLastCountDesired);
                    val=BitConverter.ToInt64(byarrNow, 0);
                }
                iPlaceTemp+=iLastCountDesired;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek long");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void Peek(ref byte val) {
            iLastCountDesired=1;
            iLastCount=0;
            try {
                val=byarr[iPlace];
                iLastCount++;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek byte");
            }
        }
        public void Peek(ref ushort val) {
            iLastCountDesired=2;//SizeOf(val);
            iLastCount=0;
            iPlaceTemp=iPlace;
            try {
                if (BitConverter.IsLittleEndian) {
                    val=BitConverter.ToUInt16(byarr, iPlaceTemp);
                }
                else {
                    byte[] byarrNow=RMemory.SubArrayReversed(byarr, iPlaceTemp, iLastCountDesired);
                    val=BitConverter.ToUInt16(byarrNow, 0);
                }
                iPlaceTemp+=iLastCountDesired;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek ushort");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void PeekUint24(ref uint val) {
            iLastCountDesired=3;//SizeOf(val);
            iLastCount=0;
            iPlaceTemp=iPlace;
            try {
                val=(uint)byarr[iPlaceTemp];
                iPlaceTemp++;
                val+=((uint)byarr[iPlaceTemp])*256U;
                iPlaceTemp++;
                val+=((uint)byarr[iPlaceTemp])*65536U;
                iPlaceTemp++;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek uint24");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void Peek(ref uint val) {
            iLastCountDesired=4;//SizeOf(val);
            iLastCount=0;
            iPlaceTemp=iPlace;
            try {
                if (BitConverter.IsLittleEndian) {
                    val=BitConverter.ToUInt32(byarr, iPlaceTemp);
                }
                else {
                    byte[] byarrNow=RMemory.SubArrayReversed(byarr, iPlaceTemp, iLastCountDesired);
                    val=BitConverter.ToUInt32(byarrNow, 0);
                }
                iPlaceTemp+=iLastCountDesired;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek uint");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void Peek(ref ulong val) {
            iLastCountDesired=8;//SizeOf(val);
            iLastCount=0;
            iPlaceTemp=iPlace;
            try {
                if (BitConverter.IsLittleEndian) {
                    val=BitConverter.ToUInt64(byarr, iPlaceTemp);
                }
                else {
                    byte[] byarrNow=RMemory.SubArrayReversed(byarr, iPlaceTemp, iLastCountDesired);
                    val=BitConverter.ToUInt64(byarrNow, 0);
                }
                iPlaceTemp+=iLastCountDesired;
            }
            catch (Exception exn) {
                 RReporting.ShowExn(exn,"","Peek ulong");
            }
            iLastCount=iPlaceTemp-iPlace;
        }
        public void Peek(ref byte[] byarrVar, int iByteFromByter, int iBytes) {
            iPlaceTemp=iByteFromByter;
            int iCount=0;
            iLastCountDesired=iBytes;
            if (iBytes>byarrVar.Length) iBytes=byarrVar.Length;
            while (iCount<iBytes) {
                if (iPlaceTemp<iBuffer) {
                    byarrVar[iCount]=byarr[iPlaceTemp];
                    iPlaceTemp++;
                }
                else break;
                iCount++;
            }
            iLastCount=iCount;
        }
        public void PeekFast(ref byte[] byarrVar, int iByteFromByter, int iBytes) {
            iLastCountDesired=iBytes;
            if (byarrVar==null) byarrVar=new byte[iBytes];
            if (RMemory.CopyFast(ref byarrVar, ref byarr, 0, iPlace, iBytes)) {
                iLastCount=iBytes;
            }
            else {
                RReporting.ShowErr("Couldn't peek at file location","attempting CopyFast","RFile PeekFast");
                iLastCount=0;
            }
        }
        public bool Peek(ref byte[][][] by3dArray, int iDim1, int iDim2, int iDim3) {
            iPlaceTemp=iPlace;
            int iCount=0;
            iLastCountDesired=iDim1*iDim2*iDim3;
            int iLast=iPlace+iLastCountDesired;
            if (!ValidReadPosition(iLast)) {
                RReporting.ShowErr("Invalid buffer or data not available","", String.Format("RFile Peek(by3dArray,...){{at:{0}}}",iLast) );
            }
            try {
                for (int iD1=0; iD1<iDim1; iD1++) {
                    for (int iD2=0; iD2<iDim2; iD2++) {
                        for (int iD3=0; iD3<iDim3; iD3++) {
                            by3dArray[iD1][iD2][iD3]=byarr[iPlaceTemp];
                            iPlaceTemp++;
                            iCount++;
                        }
                    }
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Peek(by3dArray...){iCount:"+iCount.ToString()+"}");
                return false;
            }
            iLastCount=iCount;
            return (iLastCount==iLastCountDesired)?true:false;
        }
        public string PeekAscii(int iChars) {
            iLastCountDesired=iChars;
            iPlaceTemp=iPlace;
            string sReturn="";
            try {
                for (int iChar=0; iChar<iChars; iChar++) {
                    if (iPlaceTemp>=byarr.Length) {
                        RReporting.ShowErr("PeekAscii is Beyond byarr length","", String.Format("PeekAscii({0})",iChar) );
                        break;
                    }
                    sReturn+=Char.ToString((char)byarr[iPlaceTemp]);
                    iPlaceTemp++;
                }
            }
            catch (Exception exn) {
                RReporting.Debug(exn,"","RFile PeekAscii");//TODO: handle exception
            }
            iLastCount=iPlaceTemp-iPlace;
            return sReturn;
        }
        //TODO: make sure exceptions say the right function: use reflection, and send function name separately to IPC status window
        public string PeekUnicode(int iCharsAreBytesDividedByTwo) {
            bOverflow=false;
            if (iCharsAreBytesDividedByTwo>(int.MaxValue/2)) {
                iCharsAreBytesDividedByTwo=(int.MaxValue/2);
                bOverflow=true;
            }
            iLastCountDesired=iCharsAreBytesDividedByTwo*2;
            iPlaceTemp=iPlace;
            string sReturn="";
            try {
                if (iCharsAreBytesDividedByTwo*2>RMemory.iMaxAllocation) {
                    throw new ApplicationException("Peek Unicode length of "+iCharsAreBytesDividedByTwo.ToString()+" chars times 2 was greater than MaxAllocation of "+RMemory.iMaxAllocation.ToString());
                }
                ushort wChar;
                for (int iChar=0; iChar<iLastCountDesired/2; iChar++) {
                    wChar=(ushort)byarr[iPlaceTemp];
                    iPlaceTemp++;
                    wChar+=(ushort)(((ushort)byarr[iPlaceTemp])*256);
                    iPlaceTemp++;
                    sReturn+=Char.ToString((char)wChar);
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Peek unicode");
            }
            iLastCount=iPlaceTemp-iPlace;
            return sReturn;
        }
        public bool PeekBitmap32BGRA(out Bitmap bmpReturn, int iWidth, int iHeight) {
            bool bGood=false;
            bmpReturn=null;
            try {
                bmpReturn=new Bitmap(iWidth, iHeight, PixelFormat.Format32bppArgb);
                GraphicsUnit unit=GraphicsUnit.Pixel;
                RectangleF rectNowF=bmpReturn.GetBounds(ref unit);
                Rectangle rectNow=new Rectangle((int) rectNowF.X, (int)rectNowF.Y, (int)rectNowF.Width, (int)rectNowF.Height);
                
                int iStride=rectNow.Width*4; //assumes 32-bit
                if (iStride%4!=0) { //assumes 32-bit
                    iStride=4*(iStride/4+1);
                }                //BitmapData bmpdata = bmpReturn.LockBits(rectNow, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb); //assume 32bit
                //int iDest=0; //byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
                iLastCountDesired=(int)rectNowF.Width*(int)rectNowF.Height*4; //assume 32bit
                int iByteEOF=Position+iLastCountDesired;
                int iByteLast=iByteEOF-1;
                iLastCount=0;
                if (!ValidReadPosition(iByteLast)) {
                    bGood=false;
                    RReporting.ShowErr("Data source is not big enough to contain the expected image","trying to read image from data source", String.Format("PeekBitmap32BGRA(...){{EndingPositionExpected:{0}; BufferSize:{1}; Expected:{2}}}",iByteLast,iBuffer,iLastCountDesired) );
                }
                else { //debug performance, use CopyFastVoid?
                    int iBy=Position;
                    for (int yNow=0; yNow<rectNow.Height; yNow++) {
                        for (int xNow=0; xNow<rectNow.Width; xNow++) {
                            //if (iBy>=iByteEOF) { //this was already checked by ValidReadPosition(iByteLast)
                            //    bGood=false;
                            //    break;
                            //}
                            bmpReturn.SetPixel( xNow, yNow, Color.FromArgb( byarr[iBy+3],byarr[iBy+2],byarr[iBy+1],byarr[iBy] ) );
                            iLastCount+=4;
                            iBy+=4;
                            //iDest++;
                        }
                    }
                    bGood=true;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","PeekBitmap32BGRA");
            }
            if (iLastCount!=iLastCountDesired) {
                bGood=false;
                RReporting.ShowErr("The image data was incomplete.","","PeekBitmap32BGRA");
            }
            return bGood;
        }

        #endregion peek methods
    
        #region poke methods
        
        public void Poke(ref byte val) {
            iLastCountDesired=1;
            iLastCount=0;
            try {
                byarr[iPlace]=val;
                iLastCount=1;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke byte");
                iLastCount=0;
            }
            UsedCountAtLeast(iPlace+1);
        }
        public void Poke(ref ushort val) {
            iLastCountDesired=2;//sizeof(val);
            iPlaceTemp=iPlace;
            try {
                byarr[iPlaceTemp]=(byte)(val&0xFF);
                iPlaceTemp++;
                byarr[iPlaceTemp]=(byte)(val>>8);
                iPlaceTemp++;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke ushort");
            }
            UsedCountAtLeast(iPlaceTemp); //OK to call after increment since adjusts a length val
            iLastCount=iPlaceTemp-iPlace;
        }  
        public void PokeUint24(ref uint val) {
            iLastCountDesired=3;//sizeof(val);
            iPlaceTemp=iPlace;
            bOverflow=false;
            try {
                if (val>16777215) {
                    bOverflow=true;
                    val=16777215;
                }
                byarr[iPlaceTemp]=(byte)(val&0xFF);
                iPlaceTemp++;
                byarr[iPlaceTemp]=(byte)((val>>8)&0xFF);
                iPlaceTemp++;
                byarr[iPlaceTemp]=(byte)((val>>16)&0xFF);
                iPlaceTemp++;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"",String.Format("Poke Uint24({0}){{{1}}}",
                    RReporting.DebugStyle("value",val,false),
                    RReporting.DebugStyle("iLastCount",iPlaceTemp-iPlace,false)
                    )
                );
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iPlaceTemp-iPlace;
        }//end Poke Uint24
        public void Poke(ref uint val) {
            iLastCountDesired=4;//sizeof(val);
            iPlaceTemp=iPlace;
            try {
                byte[] byarrNow=BitConverter.GetBytes(val);
                if (BitConverter.IsLittleEndian) {
                    Poke(ref byarrNow);
                }
                else {
                    byarrNow=RMemory.SubArrayReversed(byarrNow, 0, iLastCountDesired);
                    Poke(ref byarrNow);
                }
                //iPlaceTemp+=iLastCountDesired;//done by Poke byte[]
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke uint");
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iPlaceTemp-iPlace;
            if (iLastCount!=iLastCountDesired) { //debug only
                RReporting.Debug(String.Format("Poke uint {{{0};{1}}}",
                    RReporting.DebugStyle("iLastCount",iLastCount,false),
                    RReporting.DebugStyle("iLastCountDesired",iLastCountDesired,false) )
                );
            }
        }//end Poke
        public void Poke(ref ulong val) {
            iLastCountDesired=8;//sizeof(val);
            iPlaceTemp=iPlace;
            try {
                byte[] byarrNow=BitConverter.GetBytes(val);
                if (BitConverter.IsLittleEndian) {
                    Poke(ref byarrNow);
                }
                else {
                    byarrNow=RMemory.SubArrayReversed(byarrNow, 0, iLastCountDesired);
                    Poke(ref byarrNow);
                }
                //iPlaceTemp+=iLastCountDesired;//done by Poke byte[]
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke ulong");
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iPlaceTemp-iPlace;
        }
        public void PokeInt24(ref int val) {
            iLastCountDesired=3;
            iPlaceTemp=iPlace;
            bool bCompliment;
            bOverflow=false;
            try {
                if (val<0) {
                    val*=-1;
                    val-=1;//makes it a twos compliment (since 11111111 11111111 11111111 will ==-1 instead of "-0")
                    //-will later be complimented--0x7FFFFF becomes 0x800000
                    bCompliment=true;
                }
                else if (val>0) bCompliment=false;
                else bCompliment=false;//if zero, still false
                //Example -1:
                //  11111111 11111111 11111111
                //  perform compliment
                //  equals 00000000 00000000 00000000
                //  (insert *-1 here)
                //  minus 1 (twos compliment adjuster)
                //    equals -1
                //Smallest number:
                //  10000000 00000000 00000000
                //  perform compliment
                //  equals 01111111 11111111 11111111
                //  equals 8388607
                //  *-1 = -8388607
                //  minus 1 (twos compliment adjuster)
                //  equals -8388608
                //  (the resulting absolute value is one higher than 
                //   the stored absolute value of a negative number
                //   in twos compliment)
                //Largest number:
                //  01111111 11111111 11111111
                //  equals 8388607
                if (bCompliment && (val>8388608)) {
                    //save smallest number and mark as overflow:
                    bOverflow=true;
                    byarr[iPlaceTemp]=0x80;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=0x00;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=0x00;
                    iPlaceTemp++;
                }
                else if ((!bCompliment) && (val>8388607)) {
                    //save largest number and mark as overflow:
                    bOverflow=true;
                    byarr[iPlaceTemp]=0x7F;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=0xFF;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=0xFF;
                    iPlaceTemp++;
                }
                else { //else within range of signed 24-bit variable
                    //(Sign bit is added by xor operation
                    byarr[iPlaceTemp]=(byte)(val%256);//(byte)(val&255);
                    if (bCompliment) byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^(byte)0xFF);
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)((int)(val/256)%256);//(byte)((int)(val>>8)&255);
                    if (bCompliment) byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^(byte)0xFF);
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(val/65536);//(byte)((int)(val>>16)&255);
                    if (bCompliment) byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^(byte)0xFF);
                    iPlaceTemp++;
                }
                //if (bOverflow) throw new ApplicationException(val.ToString()+ " is to not within the (signed) int24 range (-8388608 to 8388607 inclusively) and was set to the closest value (either the maximum or minimum) instead.");
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke int24");
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iPlaceTemp-iPlace;
        }//end poke int24
        public void Poke(ref int val) {
            iLastCountDesired=4;
            iPlaceTemp=iPlace;
            try {
                byte[] byarrNow=BitConverter.GetBytes(val);
                if (BitConverter.IsLittleEndian) {
                    Poke(ref byarrNow);
                }
                else {
                    byarrNow=RMemory.SubArrayReversed(byarrNow, 0, iLastCountDesired);
                    Poke(ref byarrNow);
                }
                //iPlaceTemp+=iLastCountDesired;//done by Poke byte[]
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke int");
            }
            UsedCountAtLeast(iPlaceTemp);
            //iLastCount=iPlaceTemp-iPlace;//done by Poke byte[]
        }
        public void PokeInt48(ref long val) {
            iLastCountDesired=6;
            iPlaceTemp=iPlace;
            bool bCompliment;
            bOverflow=false;
            ulong uvar=0;
            try {
                if (val<0) {
                    if (val<-140737488355328) {
                        val=-140737488355328;
                        bOverflow=true;
                    }
                    uvar=(ulong)(((long)val+1L)*-1L);
                    //+1 lowers the absolute value to make a twos compliment (since 11111111 11111111 11111111 will ==-1 instead of "-0")
                    //-will later be complimented--0x7FFFFFFF becomes 0x80000000
                    bCompliment=true;
                }
                else if (val>0) {
                    if (val>140737488355327) {//7FFFFFFFFFFF) {
                        val=140737488355327;
                        bOverflow=true;
                    }
                    bCompliment=false;
                }
                else bCompliment=false;//if zero, still false
                //(Sign bit is added by xor operation)
                if (bCompliment) {
                    byarr[iPlaceTemp]=(byte)(uvar%256UL);
                    byarr[iPlaceTemp]^=0xFF;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/256UL%256UL);
                    byarr[iPlaceTemp]^=0xFF;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/65536UL%256UL);
                    byarr[iPlaceTemp]^=0xFF;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/16777216UL%256UL);
                    byarr[iPlaceTemp]^=0xFF;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/4294967296UL%256UL);
                    byarr[iPlaceTemp]^=0xFF;
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/1099511627776UL);
                    byarr[iPlaceTemp]^=0xFF;
                    iPlaceTemp++;
                }
                else {
                    uvar=(ulong)val;
                    byarr[iPlaceTemp]=(byte)(uvar%256UL);//(byte)(val&255);
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/256UL%256UL);//(byte)((int)(val>>8)&255);
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/65536UL%256UL);//(byte)((int)(val>>16)&255);
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/16777216UL%256UL);//(byte)((int)(val>>16)&255);
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/4294967296UL%256UL);//(byte)((int)(val>>16)&255);
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(uvar/1099511627776UL);//(byte)((int)(val>>16)&255);
                    iPlaceTemp++;
                }
                //if (bOverflow) throw new ApplicationException(val.ToString()+ " is to not within the (signed) int24 range (-8388608 to 8388607 inclusively) and was set to the closest value (either the maximum or minimum) instead.");
                //TODO: note overflow as warning (not error)
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke int48");
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iPlaceTemp-iPlace;
        }
        public void Poke(ref long val) {
            iLastCountDesired=8;
            iPlaceTemp=iPlace;
            try {
                byte[] byarrNow=BitConverter.GetBytes(val);
                if (BitConverter.IsLittleEndian) {
                    Poke(ref byarrNow);
                }
                else {
                    byarrNow=RMemory.SubArrayReversed(byarrNow, 0, iLastCountDesired);
                    Poke(ref byarrNow);
                }
                //iPlaceTemp+=iLastCountDesired;//done by Poke byte[]
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke long");
            }
            UsedCountAtLeast(iPlaceTemp);
            //iLastCount=iPlaceTemp-iPlace;//done by Poke byte[]
        }
        public void Poke(ref float val) { //Single, float, float32
            iLastCountDesired=4;//SizeOf(val);
            iPlaceTemp=iPlace;
            try {
                byte[] byarrNow=BitConverter.GetBytes(val);
                if (BitConverter.IsLittleEndian) {
                    Poke(ref byarrNow);
                }
                else {
                    byarrNow=RMemory.SubArrayReversed(byarrNow, 0, iLastCountDesired);
                    Poke(ref byarrNow);
                }
                //iPlaceTemp+=iLastCountDesired;//done by Poke byte[]
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke float");
            }
            UsedCountAtLeast(iPlaceTemp);
            //iLastCount=iPlaceTemp-iPlace;//done by Poke byte[]
        }
        public void Poke(ref double val) { //double, double, float64
            iLastCountDesired=8;//SizeOf(val);
            iPlaceTemp=iPlace;
            try {
                byte[] byarrNow=BitConverter.GetBytes(val);
                if (BitConverter.IsLittleEndian) {
                    Poke(ref byarrNow);
                }
                else {
                    byarrNow=RMemory.SubArrayReversed(byarrNow, 0, iLastCountDesired);
                    Poke(ref byarrNow);
                }
                //iPlaceTemp+=iLastCountDesired;//done by Poke byte[]
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke double");
            }
            UsedCountAtLeast(iPlaceTemp);
            //iLastCount=iPlaceTemp-iPlace;//done by Poke byte[]
        }
        public bool Poke(ref byte[][][] by3dArray, int iDim1, int iDim2, int iDim3) {
            iPlaceTemp=iPlace;
            int iCount=0;
            iLastCountDesired=iDim1*iDim2*iDim3;
            int iLast=iPlace+iLastCountDesired;
            if (!ValidWritePosition(iLast)) {
                RReporting.ShowErr("Invalid buffer or not large enough", String.Format("Poke(by3dArray){{ExpectedEnd:{0};BufferSize:{1}}}",iLast,iBuffer) );
            }
            try {
                for (int iD1=0; iD1<iDim1; iD1++) {
                    for (int iD2=0; iD2<iDim2; iD2++) {
                        for (int iD3=0; iD3<iDim3; iD3++) {
                            byarr[iPlaceTemp]=by3dArray[iD1][iD2][iD3];
                            iPlaceTemp++;
                            iCount++;
                        }
                    }
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"writing 3d array","Poke(by3dArray){location:" +iCount.ToString()+"}");
                return false;
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iCount;
            return (iLastCount==iLastCountDesired)?true:false;
        }
        public void Poke(ref byte[] byarrVar) {
            int iCount=0;
            iPlaceTemp=iPlace;
            iLastCountDesired=byarrVar.Length;
            while (iCount<iLastCountDesired) {
                if (iPlaceTemp<byarr.Length) {
                    byarr[iPlaceTemp]=byarrVar[iCount];
                    iPlaceTemp++;
                }
                else break;
                iCount++;
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iCount;
        }
        public void Poke(ref byte[] byarrVar, int iAtByterLoc, int iBytes) {
            iPlaceTemp=iAtByterLoc;
            int iCount=0;
            iLastCountDesired=iBytes;
            if (iBytes>byarrVar.Length) iBytes=byarrVar.Length;
            while (iCount<iBytes) {
                if (iPlaceTemp<byarr.Length) {
                    byarr[iPlaceTemp]=byarrVar[iCount];
                    iPlaceTemp++;
                }
                else break;
                iCount++;
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iCount;
        }
        public void PokeFast(ref byte[] byarrVar, int iAtByterLoc, int iBytes) {
            iLastCountDesired=iBytes;
            if (RMemory.CopyFast(ref byarr, ref byarrVar, iAtByterLoc, 0, iBytes)) {
                iLastCount=iBytes;
            }
            else {
                RReporting.ShowErr("Couldn't poke at file location","attempting CopyFast","RFile PokeFast");
                iLastCount=0;
            }
            UsedCountAtLeast(iPlaceTemp);
        }
        public void PokeAscii(ref string val) {
            char[] varr=val.ToCharArray();
            PokeAscii(ref varr);
        }
        public void PokeAscii(ref char[] varr) {
            iPlaceTemp=iPlace;
            iLastCountDesired=varr.Length;
            iLastCount=0;
            try {
                for (int iChar=0; iChar<varr.Length; iChar++) {
                    byarr[iPlaceTemp]=(byte)(((int)varr[iChar])%256);
                    iPlaceTemp++;
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke ascii");
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iPlaceTemp-iPlace;
        }
        public void PokeUnicode(ref string val) {
            char[] varr=val.ToCharArray();
            PokeUnicode(ref varr);
        }
        public void PokeUnicode(ref char[] varr) {
            iLastCountDesired=varr.Length*2;
            iPlaceTemp=iPlace;
            try {
                for (int iChar=0; iChar<varr.Length; iChar++) {
                    byarr[iPlaceTemp]=(byte)(((ushort)varr[iChar])%256);
                    iPlaceTemp++;
                    byarr[iPlaceTemp]=(byte)(((ushort)varr[iChar])/256);
                    iPlaceTemp++;
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Poke unicode");
            }
            UsedCountAtLeast(iPlaceTemp);
            iLastCount=iPlaceTemp-iPlace;
        }
        public unsafe bool PokeBitmap32BGRA(string sFile) {
            bool bGood=true;
            Bitmap bmpTemp;
            try {
                bmpTemp=new Bitmap(sFile);
                bGood=PokeBitmap32BGRA(ref bmpTemp);
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","PokeBitmap32BGRA(\""+sFile+"\")");
                bGood=false;
            }
            UsedCountAtLeast(iPlaceTemp);
            return bGood;
        }
        public unsafe bool PokeBitmap32BGRA(ref Bitmap bmpLoaded) {
            //TODO: make safe since raw bitmap data should NOT be transmitted over internet
            try {
                GraphicsUnit unit = GraphicsUnit.Pixel;
                RectangleF rectNowF = bmpLoaded.GetBounds(ref unit);
                Rectangle rectNow = new Rectangle((int) rectNowF.X,
                    (int) rectNowF.Y,
                    (int) rectNowF.Width,
                    (int) rectNowF.Height);
                int iStride = (int) rectNowF.Width * 4; //assumes 32-bit
                if (iStride % 4 != 0) { //assumes 32-bit
                    iStride = 4 * (iStride / 4 + 1);
                }
                BitmapData bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb); //assume 32bit
                
                byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
                iLastCountDesired=(int)rectNowF.Width*(int)rectNowF.Height*4; //assume 32bit
                int iByteEOF=Position+iLastCountDesired;
                int iByteLast=iByteEOF-1;
                iLastCount=0;
                if (!ValidWritePosition(iByteLast)) {
                    RReporting.ShowErr("Could not fit bitmap in buffer (byter corruption)","",String.Format("PokeBitmap32BGRA(){{ExpectedEnd:{0};BufferMaximum:{1}}}",iByteLast,byarr.Length) );
                }
                else {
                    for (int iBy=Position; iBy<iByteEOF; iBy++) {
                        byarr[iBy]=*lpbyNow;
                        lpbyNow++;
                        iLastCount++;
                    }
                }
                bmpLoaded.UnlockBits(bmpdata);
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","PokeBitmap32BGRA");
                return false;
            }
            UsedCountAtLeast(iPlaceTemp);
            if (iLastCount!=iLastCountDesired) RReporting.ShowErr("Not all of bitmapdata was written.");
            return (iLastCount==iLastCountDesired);
        }//end PokeBitmap32BGRA

        #endregion poke methods
        
        #region read methods (get methods)

        //Unsigned types
        public void Read(ref byte val) {
            Peek(ref val);
            iPlace+=1;
        }
        public void Read(ref ushort val) {
            Peek(ref val);
            iPlace+=2;
        }
        public void ReadUint24(ref uint val) {
            PeekUint24(ref val);
            iPlace+=3;
        }
        public void Read(ref uint val) {
            Peek(ref val);
            iPlace+=4;
        }
        public void Read(ref ulong val) {
            Peek(ref val);
            iPlace+=8;
        }
        //Signed types:
        public void ReadInt24(ref int val) {
            PeekInt24(ref val);
            iPlace+=3;
        }
        public void Read(ref int val) {
            Peek(ref val);
            iPlace+=4;
        }
        public unsafe void Read(ref long val) {
            Peek(ref val);
            iPlace+=8;
        }
        public unsafe void Read(ref float val) {//Single, float, float32
            Peek(ref val);
            iPlace+=4;
        }
        public unsafe void Read(ref double val) {
            Peek(ref val);
            iPlace+=8;
        }
        public void ReadFast(ref byte[] byarrVar, int iBytes) {
            iLastCountDesired=iBytes;
            if (RMemory.CopyFast(ref byarr, ref byarrVar, iPlace, 0, iBytes)) {
                iLastCount=iBytes;
            }
            else {
                RReporting.ShowErr("Couldn't read at file location","attempting CopyFast","RFile ReadFast");
                iLastCount=0;
            }
            iPlace+=iLastCount;
        }
        public void Read(ref byte[] byarrVar, int iBytes) {
            Peek(ref byarrVar, iPlace, iBytes);
            iPlace+=iLastCount;
        }
        //public bool ReadBitmap32BGRA(string sFile) {
        //    if (PeekBitmap32BGRA(sFile)) iPlace+=iLastCount;
        //    else return false;
        //    return true;
        //}
        public bool ReadBitmap32BGRA(out Bitmap bmpReturn, int iWidth, int iHeight) {
            if (PeekBitmap32BGRA(out bmpReturn, iWidth, iHeight)) iPlace+=iLastCount;
            else return false;
            return true;
        }
        //non-reference read methods (get methods):
        public uint GetForcedByte() {
            byte valReturn=0;
            Peek(ref valReturn);
            iPlace+=iLastCount;
            return valReturn;
        }
        public ushort GetForcedUshort() {
            ushort valReturn=0;
            Peek(ref valReturn);
            iPlace+=iLastCount;
            return valReturn;
        }
        public uint GetForcedUint24() {
            uint valReturn=0;
            PeekUint24(ref valReturn);
            iPlace+=iLastCount;
            return valReturn;
        }
        public uint GetForcedUint() {
            uint valReturn=0;
            Peek(ref valReturn);
            iPlace+=iLastCount;
            return valReturn;
        }
        public ulong GetForcedUlong() {
            ulong valReturn=0;
            Peek(ref valReturn);
            iPlace+=iLastCount;
            return valReturn;
        }
        //read signed types
        public int GetForcedInt() {
            int valReturn=0;
            Peek(ref valReturn);
            iPlace+=iLastCount;
            return valReturn;
        }
        public int GetForcedInt24() {
            int valReturn=0;
            PeekInt24(ref valReturn);
            iPlace+=iLastCount;
            return valReturn;
        }
        public long GetForcedLong() {
            long valReturn=0;
            Peek(ref valReturn);
            iPlace+=iLastCount;
            return valReturn;
        }
        public byte[] GetForcedBytes(int iBytes) {
            byte[] valReturn=null;
            Peek(ref valReturn, iPlace, iBytes);
            //if (iLastCount!=iBytes) valReturn=null;
            iPlace+=iLastCount;
            return valReturn;
        }
        //TODO: byte[][][], int iDim1, int iDim1, int iDim1
        //text read methods
        public string GetForcedAscii(int iChars) {
            string sReturn=PeekAscii(iChars);
            iPlace+=iLastCount;
            return (sReturn!=null)?sReturn:"";
        }
        public string GetForcedUnicode(int iChars_BytesDividedByTwo) {
            string sReturn=PeekUnicode(iChars_BytesDividedByTwo);
            iPlace+=iLastCount;
            return (sReturn!=null)?sReturn:"";
        }
        
        //
        #endregion read methods (get methods)
        
        #region write methods
        //Unsigned types
        public void Write(ref byte val) {
            Poke(ref val);
            iPlace+=1;
        }
        public void Write(ref ushort val) {        
            Poke(ref val);
            iPlace+=2;
        }
        public void WriteUint24(ref uint val) {
            PokeUint24(ref val);
            iPlace+=3;
        }
        public void Write(ref uint val) {
            Poke(ref val);
            iPlace+=4;
        }
        public void Write(ref ulong val) {        
            Poke(ref val);
            iPlace+=8;
        }
        //Signed types
        public void WriteInt24(ref int val) {
            PokeInt24(ref val);
            iPlace+=3;
        }
        public void Write(ref int val) {
            Poke(ref val);
            iPlace+=4;
        }
        public void Write(ref long val) {
            Poke(ref val);
            iPlace+=8;
        }
        public void Write(ref float val) { //Single, float, float32
            Poke(ref val);
            iPlace+=4;
        }
        public void Write(ref double val) { //Double, double, float64
            Poke(ref val); //debug the function is unsafe so does
                        //the calling function also have to be?
            iPlace+=8;
        }
        public bool Write(ref byte[] byarrVar) {
            Poke(ref byarrVar, iPlace, byarrVar.Length);
            iPlace+=iLastCount;
            return (iLastCount==iLastCountDesired);
        }
        public bool Write(ref byte[] byarrVar, int iBytes) {
            Poke(ref byarrVar, iPlace, iBytes);
            iPlace+=iLastCount;
            return (iLastCount==iLastCountDesired);
        }
        public void WriteFast(ref byte[] byarrVar, int iBytes) {
            PokeFast(ref byarrVar, iPlace, iBytes);
            iPlace+=iLastCount;
        }
        public void WriteUnicode(ref string val) {
            PokeUnicode(ref val);
            iPlace+=iLastCount;
        }
        public void WriteAscii(ref string val) {
            char[] varr=val.ToCharArray();
            //if (val.Length!=varr.Length)
            //    MessageBox.Show(val.Length.ToString()+"(string length) != varr Length"+varr.Length.ToString());//debug only
            //else
            //    MessageBox.Show(val.Length.ToString()+"(string length) == varr Length"+varr.Length.ToString());//debug only
            PokeAscii(ref varr);
            iPlace+=iLastCount;
        }
        public void WriteAscii(ref char[] val) {
            PokeAscii(ref val);
            iPlace+=iLastCount;
        }
        public bool WriteBitmap32BGRA(string sFile) {
            if (PokeBitmap32BGRA(sFile)) iPlace+=iLastCount;
            else return false;
            return true;
        }
        public bool WriteBitmap32BGRA(ref Bitmap bmpSrc) {
            if (PokeBitmap32BGRA(ref bmpSrc)) iPlace+=iLastCount;
            else return false;
            return true;
        }
        #endregion write methods
        
        #region write methods -- not by reference
        
        public void Write(byte val) {
            Poke(ref val);
            iPlace+=1; //iPlace+=iLastCount;
        }
        public void Write(ushort val) {        
            Poke(ref val);
            iPlace+=2;
        }
        public void WriteUint24(uint val) {
            PokeUint24(ref val);
            iPlace+=3;
        }
        public void Write(uint val) {        
            Poke(ref val);
            iPlace+=4;
        }
        public void Write(ulong val) {        
            Poke(ref val);
            iPlace+=8;
        }
        public void WriteInt24(int val) {
            PokeInt24(ref val);
            iPlace+=3;
        }
        public void Write(int val) {
            Poke(ref val);
            iPlace+=4;
        }
        public void Write(long val) {
            Poke(ref val);
            iPlace+=8;
        }
        public void Write(float val) { //Single, float, float32
            Poke(ref val);
            iPlace+=4;
        }
        public void Write(double val) { //Double, double, float64
            Poke(ref val); //debug the function is unsafe so does
                        //the calling function also have to be?
            iPlace+=8;
        }
        public void Write(byte[] byarrVar) {
            Poke(ref byarrVar, iPlace, byarrVar.Length);
            try {
                iPlace+=byarrVar.Length;
            }
            catch (Exception exn) {
                RReporting.Debug(exn,"getting byarrVar Length","Write");
            }
        }
        public void Write(byte[] byarrVar, int iBytes) {
            Poke(ref byarrVar, iPlace, iBytes);
            iPlace+=iBytes;
        }
        public void WriteFast(byte[] byarrVar, int iBytes) {
            PokeFast(ref byarrVar, iPlace, iBytes);
            iPlace+=iBytes;
        }
        public void WriteUnicode(String val) {
            PokeUnicode(ref val);
            iPlace+=val.Length;
        }
        
        #endregion write methods -- not by reference
        
        
        //public void PokeFastNonIEEE(ref float val) {
        //    double mSig=val;
        //    int mExpOf10=0;
        //    //bits: 8(bias=127, max=255) 24 (bias=8388608, max=16777215)
        //    if (mSig==0) {
        //        //save zero here
        //    }
        //    while (mSig>=1.0d) {
        //        mSig/=10.0d;
        //        mExpOf10++;
        //    }
        //}
        //public void PokeFastNonIEEE(ref double val) {
        //    decimal mSig=val;
        //    int mExpOf10=0;
        //    //bits: 11(bias=1024, max=2047) 53(bias=4503599627370496, max=9007199254740991)
        //    if (mSig==0) {
        //        //save zero here
        //    }
        //    while (mSig>=1.0d) {
        //        mSig/=10.0d;
        //        mExpOf10++;
        //    }
        //}
#region utilities
        public static string KnownExtensionFromNameElseBlank(string sName) {
            string sReturn="";
            try {
                if (sName!="" && sName.Length>2) {
                    string sLower=sName.ToLower();
                    if (sName.ToLower().EndsWith(".png")) { sReturn="png"; }
                    else if (sLower.EndsWith(".jpg")) { sReturn="jpg"; }
                    else if (sLower.EndsWith(".jpe")) { sReturn="jpe"; }
                    else if (sLower.EndsWith(".jpeg")){ sReturn="jpeg";}
                    else if (sLower.EndsWith(".gif")) { sReturn="gif"; }
                    else if (sLower.EndsWith(".exi")) { sReturn="exi"; }
                    else if (sLower.EndsWith(".exif")){ sReturn="exif";}
                    else if (sLower.EndsWith(".emf")) { sReturn="emf"; }
                    else if (sLower.EndsWith(".tif")) { sReturn="tif"; }
                    else if (sLower.EndsWith(".tiff")){ sReturn="tiff";}
                    else if (sLower.EndsWith(".ico")) { sReturn="ico"; }
                    else if (sLower.EndsWith(".wmf")) { sReturn="wmf"; }
                    else if (sLower.EndsWith(".bmp")) { sReturn="bmp"; }
                    else if (sLower.EndsWith(".txt")) { sReturn="txt"; }
                    else if (sLower.EndsWith(".raw")) { sReturn="raw"; }
                    else if (sLower.EndsWith(".ogg")) { sReturn="ogg"; }
                    else if (sLower.EndsWith(".mp3")) { sReturn="mp3"; }
                    else if (sLower.EndsWith(".wav")) { sReturn="wav"; }
                    else if (sLower.EndsWith(".ini")) { sReturn="ini"; }
                    else if (sLower.EndsWith(".html")) { sReturn="html"; }
                    else if (sLower.EndsWith(".htm")) { sReturn="htm"; }
                    else if (sLower.EndsWith(".php")) { sReturn="php"; }
                    else if (sLower.EndsWith(".js")) { sReturn="js"; }
                    else sReturn="";
                }
            }
            catch (Exception exn) {
                sReturn="";
                RReporting.ShowExn(exn,"detecting file extension in name string");
            }
            return sReturn;
        }//end KnownExtensionFromNameElseBlank
        public static bool SafeDelete(string sFile) {
            bool bGood=true;
            try {
                if (File.Exists(sFile)) File.Delete(sFile);
            }
            catch {
                bGood=false;
            }
            return bGood;
        }
        public static bool ByteArrayToFile(string sFile, byte[] data) {
            bool bGood=false;
            FileStream fsNow=null;
            BinaryWriter bwNow=null;
            try {
                try {
                    fsNow=new FileStream(sFile, FileMode.CreateNew);
                }
                catch (Exception exn) {
                    RReporting.Debug(exn);
                    fsNow=new FileStream(sFile,FileMode.Create);//TODO: show warning?
                }
                bwNow=new BinaryWriter(fsNow);
                bwNow.Write(data);
                bwNow.Close();
                fsNow.Close();
                bGood=true;
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn);
            }
            return bGood;
        }//end ByteArrayToFile
        public static long FileSize(string sFile) {
            FileInfo fiNow=null;
            long iReturn=-2;
            try {
                fiNow=new FileInfo(sFile);
                iReturn=fiNow.Length;
            }
            catch (Exception exn) {
                iReturn=-3;
                RReporting.ShowExn(exn);
            }
            return iReturn;
        }
        public static long SizeOfFile(string sFile) {
            return FileSize(sFile);
        }
        public static bool FileToByteArray(out byte[] byarrData, string sFile) {
            bool bGood=false;
            byarrData=null;
            FileInfo fiNow=null;
            FileStream fsNow=null;
            BinaryReader brNow=null;
            long numBytes=-2;
            try {
                fiNow=new FileInfo(sFile);
                numBytes=fiNow.Length;
                fsNow=new FileStream(sFile, FileMode.Open, FileAccess.Read);
                brNow=new BinaryReader(fsNow);
                byarrData=brNow.ReadBytes((int)numBytes);
                brNow.Close();
                fsNow.Close();
                bGood=true;
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn);
                byarrData=null;
            }
            return bGood;
        }//end FileToByteArray

#endregion utilities
    } //end class RFile
}
