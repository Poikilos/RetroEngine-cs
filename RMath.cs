using System;
//using REAL = System.Single; //System.Double
using System.IO;

namespace ExpertMultimedia {
    class RMath {
        public const int IntersectionError=-1;
        public const int IntersectionNo=0;
        public const int IntersectionYes=1;
        public const int IntersectionBeyondSegment=2;
        public const int LineRelationshipNone=0;
        public const int LineRelationshipParallelDifferentLine=1;
        public const int LineRelationshipParallelSameLine=2;
        public const int LineRelationshipIntersectionNotTouchingEndsOfLineB=3;
        public const int LineRelationshipLineBPoint1IsOnLineA=4;
        public const int LineRelationshipLineBPoint2IsOnLineA=5;
        public const int LineRelationshipIntersectionOutOfRange=6;
        public const float FPI=3.1415926535897932384626433832795f;
        public const double DPI=3.1415926535897932384626433832795d;
        public const decimal MPI=3.1415926535897932384626433832795m;
        public static byte[][] SubtractBytes=null;//static constructor creates array of 65536 results of NON-WRAPPED (i.e. 0 - 1 = 0) byte subtraction operations
        public static readonly IPoint ipZero=new IPoint(0,0);
        ///<summary>
        ///Calculate value of color using AlphaLook[srcByte][destByte][alphaByte]
        ///-same as Approach(destByte,srcByte,alphaByte) since alphaByte causes source approach
        ///</summary>
        public static byte[][][] AlphaLook=null;
        public static byte[][] AddBytes=null;//static constructor creates array of 65536 results of NON-WRAPPED (i.e. 0 - 1 = 0) byte subtraction operations
        public static byte[][] MultiplyBytes=null;
        private static string AlphaLookFile_FullName="./etc/alphalook.raw";
        static RMath() {
            Console.Error.Write("Loading RMath static constructor"+(File.Exists(AlphaLookFile_FullName)?"":(" (this may take a while--on first run, \""+AlphaLookFile_FullName+"\" is created if does not exist)"))+"...");
            Console.Error.Flush();
            SubtractBytes=new byte[256][];
            AddBytes=new byte[256][];
            MultiplyBytes=new byte[256][];
            AlphaLook=new byte[256][][];
            //int iResult;
            byte[] AlphaLookData=null;
            bool bCached=false;
            try {
                if (!Directory.Exists("etc")) Directory.CreateDirectory("etc");
                if (File.Exists(AlphaLookFile_FullName)) {
                    Console.Error.Write("found AlphaLookData file so loading...");
                    Console.Error.Flush();
                    StreamReader streamIn=new StreamReader(AlphaLookFile_FullName);
                    BinaryReader binIn=new BinaryReader(streamIn.BaseStream);
                    AlphaLookData=binIn.ReadBytes( 256*256*256 );
                    streamIn.Close();
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"reading alphalook file","RMath static constructor {File:\""+AlphaLookFile_FullName+"\"}");
                AlphaLookData=null;
                bCached=false;
            }
            if (AlphaLookData!=null&&AlphaLookData.Length==256*256*256) {
                bCached=true;
            }
            else {
                AlphaLookData=new byte[256*256*256];
                Console.Error.Write("no AlphaLookData so generating...");
                Console.Error.Flush();
            }
            int AlphaLook_RelIndex=0;
            
            for (int i1=0; i1<256; i1++) {//must be int because byte will never get to 256!
                SubtractBytes[i1]=new byte[256];
                AddBytes[i1]=new byte[256];
                MultiplyBytes[i1]=new byte[256];
                AlphaLook[i1]=new byte[256][]; //always create even if cached--will be loaded from 1D array if so
                
                for (int i2=0; i2<256; i2++) {//must be int because byte will never get to 256!
                    SubtractBytes[i1][i2]=SafeSubtract_Math((byte)i1,(byte)i2);//iResult=i1-i2; =(iResult>=0)?(byte)iResult:by0; 
                    AddBytes[i1][i2]=SafeAdd_Math((byte)i1,(byte)i2);
                    MultiplyBytes[i1][i2]=SafeMultiply_Math((byte)i1,(byte)i2);
                    AlphaLook[i1][i2]=new byte[256]; //always create even if cached--will be loaded from 1D array if so
                    
                    if (bCached) {
                        for (int i3=0; i3<256; i3++) {
                            //load from 1D array since cached:
                            AlphaLook[i1][i2][i3]=AlphaLookData[AlphaLook_RelIndex]; 
                            AlphaLook_RelIndex++;
                        }
                    }
                    else {
                        Console.Error.Write("Generating Alpha lookup table...");
                        for (int i3=0; i3<256; i3++) {
                            AlphaLook[i1][i2][i3]=RMath.ByRound(RMath.Approach((double)i2, (double)i1, (double)i3/(double)255)); ///approach works like start,toward,fRatioTowardDest but alpha goes toward start (source) so they are switched (back to linear order) when Approach is used
                            AlphaLookData[AlphaLook_RelIndex]=AlphaLook[i1][i2][i3];
                            AlphaLook_RelIndex++;
                        }
                        Console.Error.WriteLine("OK");
                    }
                }
            }//end for 256-long lookup tables
            StreamWriter streamOut=null;
            
            if (!bCached) {
                Console.Error.Write("Saving "+AlphaLookFile_FullName+"...");
                Console.Error.Flush();
                try {
                    streamOut=new StreamWriter(AlphaLookFile_FullName);
                    BinaryWriter binOut=new BinaryWriter(streamOut.BaseStream);
                    binOut.Write(AlphaLookData);
                    streamOut.Close();
                    streamOut=null;
                }
                catch (Exception e) {
                    RReporting.ShowExn(e,"saving alpha lookup table","RMath static constructor {AlphaLookFile_FullName:\""+AlphaLookFile_FullName+"\"}");
                }
                try {
                    if (streamOut!=null) streamOut.Close();
                }
                catch (Exception e) {
                    RReporting.ShowExn(e,"closing new alpha lookup table","RMath static constructor {AlphaLookFile_FullName:\""+AlphaLookFile_FullName+"\"}");
                }
            }
            Console.Error.WriteLine("OK (RMath static constructor finished)."); //see beginning of constructor for beginning of output line
        }//end static constructor
        public static byte SafeAverage(byte by1, byte by2, byte by3) {
            return (byte)SafeDivideRound((int)by1+(int)by2+(int)by3,3,255);
        }
        public static float SafeAdd(float var1, float var2) {
            if (var1<0) {
                if (var2<0) { // -v1 + -v2
                    if (var2>=float.MinValue-var1) return var1+var2; //less confusing than: if (float.MinValue-var1<=var2) return var1+var2;
                        //--i.e. if MinValue were -1,000,000
                        // and v1==-999,999 and v2==-1.1
                        // ( -1,000,000 - -999,9999 ) == -1.1 
                        // if ( -1.1 >= -1 ) (would be fine if v2 were -1 -- would result in -999,999+-1 = -1,000,000
                        // but truth is ( -1.1 < -1 ) which would cause overflow so return MinValue instead
                    else return float.MinValue;
                }
                else { // -v1 + +v2 (var2>=0)
                    return var1+var2; //impossible to overflow since one is negative
                }
            }
            else { // +v1 (var1>=0)
                if (var2<0) { // +v1 + -v2
                    return var1+var2;
                }
                else { // +v1 + +v2 (var2>=0)
                    if (float.MaxValue-var1>var2) return var1+var2;
                    else return float.MaxValue;
                }
            }
        }//end SafeAdd
        public static double SafeSubtract(double var, double subtract) {
            CropAbsoluteValueToPosMax(ref subtract);
            return SafeAdd(var, -1*subtract);
        }
        public static double SafeAdd(double var1, double var2) {
            if (var1<0) {
                if (var2<0) { // -v1 + -v2
                    if (var2>=double.MinValue-var1) return var1+var2; //less confusing than: if (float.MinValue-var1<=var2) return var1+var2;
                        //--i.e. if MinValue were -1,000,000
                        // and v1==-999,999 and v2==-1.1
                        // ( -1,000,000 - -999,9999 ) == -1.1 
                        // if ( -1.1 >= -1 ) (would be fine if v2 were -1 -- would result in -999,999+-1 = -1,000,000
                        // but truth is ( -1.1 < -1 ) which would cause overflow so return MinValue instead
                    else return double.MinValue;
                }
                else {  // -v1 + +v2 (var2>=0)
                    return var1+var2; //impossible to overflow since one is negative
                }
            }
            else { // +v1 (var1>=0)
                if (var2<0) { // +v1 + -v2
                    return var1+var2;
                }
                else { // +v1 + +v2 (var2>=0)
                    if (double.MaxValue-var1>var2) return var1+var2;
                    else return double.MaxValue;
                }
            }
        }//end SafeAdd
        public static int SafeSubtract(int var, int subtract) {
            CropAbsoluteValueToPosMax(ref subtract);
            return SafeAdd(var, -1*subtract);
        }
        public static int SafeAdd(int var1, int var2) {
            if (var1<0) {
                if (var2<0) {
                    if (var2>=int.MinValue-var1) return var1+var2; //less confusing than: if (float.MinValue-var1<=var2) return var1+var2;
                        //--i.e. if MinValue were -1,000,000
                        // and v1==-999,999 and v2==-1.1
                        // ( -1,000,000 - -999,9999 ) == -1.1 
                        // if ( -1.1 >= -1 ) (would be fine if v2 were -1 -- would result in -999,999+-1 = -1,000,000
                        // but truth is ( -1.1 < -1 ) which would cause overflow so return MinValue instead
                    else return int.MinValue;
                }
                else { //var2>=0
                    return var1+var2; //impossible to overflow since one is negative
                }
            }
            else { //var1>=0
                if (var2<0) {
                    return var1+var2;
                }
                else { //var2>=0
                    if (int.MaxValue-var1>var2) return var1+var2;
                    else return int.MaxValue;
                }
            }
        }//end SafeAdd

        public static byte SafeSubtract(byte var, byte subtract) {
            return SubtractBytes[(int)var][(int)subtract];
        }
        public static byte SafeSubtract_Math(byte var1, byte var2) {
            if (var1>var2) return (byte)(var1-var2);
            else return 0;
        }
        public static byte SafeAdd(byte var1, byte var2) {
            return AddBytes[var1][var2];
        }
        public static byte SafeAdd_Math(byte var1, byte var2) {
            if (byte.MaxValue-var1>var2) return (byte)(var1+var2);
            else return byte.MaxValue;
        }//end SafeAdd byte
        public static ushort SafeSubtract(ushort var1, ushort var2) {
            if (var1>var2) return (ushort)(var1-var2);
            else return 0;
        }
        public static ushort SafeAdd(ushort var1, ushort var2) {
            if (ushort.MaxValue-var1>var2) return (ushort)(var1+var2);
            else return ushort.MaxValue;
        }//end SafeAdd ushort
        public static uint SafeSubtract(uint var1, uint var2) {
            if (var1>var2) return var1-var2;
            else return 0;
        }
        public static uint SafeAdd(uint var1, uint var2) {
            if (uint.MaxValue-var1>var2) return var1+var2;
            else return uint.MaxValue;
        }//end SafeAdd uint
        public static ulong SafeSubtract(ulong var1, ulong var2) {
            if (var1>var2) return var1-var2;
            else return 0;
        }
        public static ulong SafeAdd(ulong var1, ulong var2) {
            if (ulong.MaxValue-var1>var2) return var1+var2;
            //i.e. if MaxValue was 10000
            //i.e. 9999 + 2
            // 10000-9999=1 > 2 NOT TRUE so don't do 9999+2 (return 10000)
            //i.e. 9999 + 1
            // 10000-9999=1 > 1 NOT TRUE but that's ok return MaxValue anyway
            //i.e. 9999 + 0
            // 10000-9999=1 > 0 IS TRUE so do 9999+0
            //i.e. 2 + 9999
            // 10000-2=9998 > 9998 NOT TRUE so don't do 2+9998 (return 10000)
            //i.e. 1 + 9998
            // 10000-1=9999 > 9999 NOT TRUE so don't do 1+9999 (return 10000)
            //i.e. 0 + 9999
            // 10000-0=10000 > 9999 IS TRUE so do 0+9999
            else return ulong.MaxValue;
        }//end SafeAdd ulong
        public static byte SafeAddWrapped(byte by1, byte by2) {
            int iReturn=(int)by1+(int)by2;
            if (iReturn>255) iReturn-=256; //i.e. 256 - 256 = 0
            return (byte)iReturn;
        }
        public static byte SafeSubtractWrapped(byte by1, byte by2) {
            int iReturn=(int)by1-(int)by2;
            if (iReturn<0) iReturn+=256; //i.e. -1 + 256 = 255
            return (byte)iReturn;
        }
        public static ulong SafeAddWrapped(ulong var1, ulong var2) {
            if (ulong.MaxValue-var1>var2) return var1+var2;
            else if (var1>var2) {
                //i.e. 9999+2
                //9999>2 so do [10000-9999==1] 2-1
                //i.e. 9998+3
                //9998>3 so do [10000-9998=2] 3-2`
                return var2-(ulong.MaxValue-var1);
            }
            else { //else var1<=var2
                //i.e. 2+9999
                //9999>2 NOT TRUE so do [10000-9999==1] 2-1
                //i.e. 9998+3
                //9998>3 NOT TRUE so do [10000-9998=2] 3-2
                return var1-(ulong.MaxValue-var2);
            }
        }
        public static int SafeAddWrappedTowardZero(int var1, int var2) {
            if (var1<0) {
                if (var2<0) {
                    //if (int.MinValue-var1>var2) return var1+var2;
                    if (var2>=int.MinValue-var1) return var1+var2; //less confusing than: if (float.MinValue-var1<=var2) return var1+var2;
                        //--i.e. if MinValue were -1,000,000
                        // and v1==-999,999 and v2==-1.1
                        // ( -1,000,000 - -999,9999 ) == -1.1 
                        // if ( -1.1 >= -1 ) (would be fine if v2 were -1 -- would result in -999,999+-1 = -1,000,000
                        // but truth is ( -1.1 < -1 ) which would cause overflow so return MinValue instead
                    else return (var1-int.MinValue)+var2;//wrap toward zero
                }
                else { //var2>=0
                    return var1+var2;
                }
            }
            else { //var1>=0
                if (var2<0) {
                    return var1+var2;
                }
                else { //var2>=0
                    if (int.MaxValue-var1>var2) return var1+var2;
                    else {
                        return (var1-int.MaxValue)+var2;//(var1+var2)-(int.MaxValue);
                    }
                }
            }
        }//end SafeAddWrappedTowardZero
        public static float SafeSubtract(float var, float subtract) {
            CropAbsoluteValueToPosMax(ref subtract);
            return SafeAdd(var, -1*subtract);
        }
        public static void SafeSignChangeByRef(ref int val) {
            if (val==int.MinValue) val=int.MaxValue;
            else val*=-1;
        }
        public static void SafeSignChangeByRef(ref long val) {
            if (val==long.MinValue) val=long.MaxValue;
            else val*=-1;
        }
        public static void SafeSignChangeByRef(ref float val) {
            if (val==float.MinValue) val=float.MaxValue;
            else val*=-1;
        }
        public static void SafeSignChangeByRef(ref double val) {
            if (val==double.MinValue) val=double.MaxValue;
            else val*=-1;
        }
        public static void SafeSignChangeByRef(ref decimal val) {
            if (val==decimal.MinValue) val=decimal.MaxValue;
            else val*=-1;
        }
        public static byte SafeMultiply(byte val1, byte val2) {
            return MultiplyBytes[val1][val2];
        }
        public static byte SafeMultiply_Math(byte val1, byte val2) {
            //byte valReturn=0;
            int iVal2=(int)val2;
            if (SafeDivide(255,val2,255)>=val1) {//== allowed since result floored i.e. if SafeMultiply(127,2), 255/127 results in 2 so allow 127*2 (254)
                return RConvert.ToByte(val1*val2);
            }
            else {
                return 255;
                //for (int iNow=0; iNow<iVal2; iNow++) {
                //    valReturn=SafeAdd_Math(valReturn,val1);//i.e. if val2 is 1, this happens once
                //}
            }
            //return valReturn;
        }//end int SafeMultiply_Math
        public static int SafeMultiply(int val1, int val2) {
            int valReturn=0;
            bool bNeg=false;
            if (val1<0) {
                SafeSignChangeByRef(ref val1);
                bNeg=!bNeg;
            }
            if (val2<0) {
                SafeSignChangeByRef(ref val2);
                bNeg=!bNeg;
            }
            if (SafeDivide(int.MaxValue,val2,int.MaxValue)>=val1) {//== allowed since result floored
                valReturn=val1*val2;
            }
            else {//TODO: finish this -- just needs to be tested for overflow
                if (bNeg) {
                    valReturn=int.MinValue;
                    bNeg=false;
                }
                else valReturn=int.MaxValue;
                //for (int iNow=0; iNow<val2; iNow++) {
                //    valReturn=SafeAdd(valReturn, val1);//i.e. if val2 is 1, this happens once
                //}
            }
            if (bNeg) SafeSignChangeByRef(ref valReturn);
            return valReturn;
        }//end int SafeMultiply
        public static float SafeMultiply(float val1, float val2) {
            float valReturn=0;
            bool bNeg=false;
            if (val1<0) {
                SafeSignChangeByRef(ref val1);
                bNeg=!bNeg;
            }
            if (val2<0) {
                SafeSignChangeByRef(ref val2);
                bNeg=!bNeg;
            }
            if (SafeDivide(float.MaxValue,val2,float.MaxValue)>val1) {
                valReturn=val1*val2;
            }
            else {
                if (bNeg) {
                    valReturn=float.MinValue;
                    bNeg=false;
                }
                else valReturn=float.MaxValue;
                //int iLimiter=(int)val2;
                //for (int iNow=0; iNow<iLimiter; iNow++) {
                //    valReturn=SafeAdd(valReturn, val1);//i.e. if val2 is one, this happens once
                //}
                //valReturn=SafeAdd(valReturn, val1*(val2-(float)iLimiter));//remainder
            }
            if (bNeg) SafeSignChangeByRef(ref valReturn);
            return valReturn;
        }//end float SafeMultiply
        public static double SafeMultiply(double val1, double val2) {
            double valReturn=0;
            bool bNeg=false;
            if (val1<0) {
                SafeSignChangeByRef(ref val1);
                bNeg=!bNeg;
            }
            if (val2<0) {
                SafeSignChangeByRef(ref val2);
                bNeg=!bNeg;
            }
            if (SafeDivide(double.MaxValue,val2,double.MaxValue)>val1) {
                valReturn=val1*val2;
            }
            else {
                if (bNeg) {
                    valReturn=double.MinValue;
                    bNeg=false;
                }
                else valReturn=double.MaxValue;
                //int iLimiter=(int)val2;
                //for (int iNow=0; iNow<iLimiter; iNow++) {
                //    valReturn=SafeAdd(valReturn, val1);//i.e. if val2 is one, this happens once
                //}
                //valReturn=SafeAdd(valReturn, val1*(val2-(double)iLimiter));//remainder
            }
            if (bNeg) SafeSignChangeByRef(ref valReturn);
            return valReturn;
        }//end double SafeMultiply


        
        public static float SafeAngle360(float valNow) {
            return Wrap(valNow, 0, 360);
        }
        public static double SafeAngle360(double valNow) {
            return Wrap(valNow, 0, 360);
        }
        public static decimal SafeAngle360(decimal valNow) {
            return Wrap(valNow, 0, 360);
        }
        public static string IntersectionToString(int IntersectionA) {
            if (IntersectionA==IntersectionYes) return "Intersection Found";
            else if (IntersectionA==IntersectionNo) return "No Intersection";
            else if (IntersectionA==IntersectionBeyondSegment) return "Lines Only Intersect Beyond Segment";
            else if (IntersectionA==IntersectionError) return "Intersection Error";
            else return "Unknown Intersection Type #"+IntersectionA.ToString();
        }//IntersectionToString

        #region geometry
        public static void ToRectPoints(ref int x1, ref int y1, ref int x2, ref int y2) {
            if (y2<y1) RMemory.Swap(ref y1, ref y2);
            if (x2<x1) RMemory.Swap(ref x1, ref x2);
        }
        public static bool IsInBox(int x, int y, int x1, int y1, int x2, int y2) {
            ToRectPoints(ref x1, ref y1, ref x2, ref y2);
            return (x>=x1 && x<=x2 && y>=y1 && y<=y2);
        }
        public static bool IsInBoxEx(int x, int y, int x1, int y1, int x2, int y2) {
            ToRectPoints(ref x1, ref y1, ref x2, ref y2);
            return (x>=x1 && x<x2 && y>=y1 && y<y2);
        }
        ///<summary>
        ///makes point 2 to the right (may remain above or below point 1)
        ///</summary>
        public static void OrderPointsLR(ref int x1, ref int y1, ref int x2, ref int y2) {
            //if (y1==y2) {
            //    if (x2<x1) RMemory.Swap(ref x1, ref x2);
            //}
            //else if (x1==x2) {
            //    if (y2<y1) RMemory.Swap(ref y1, ref y2);
            //}
            //else
            if (x2<x1) {
                RMemory.Swap(ref y1, ref y2);
                RMemory.Swap(ref x1, ref x2);
            }
            //else if (y2<y1) {
            //    RMemory.Swap(ref y1, ref y2);
            //    RMemory.Swap(ref x1, ref x2);
            //}
        }//end OrderPointsLR
        public static bool PointIsOnLine(int x, int y, int x1, int y1, int x2, int y2, bool bLinePointOrderHasBeenFixedAlready) {
            return PointIsOnLine(x,y,x1,y1,x2,y2,bLinePointOrderHasBeenFixedAlready,true);
        }
        
        public static bool PointIsOnLine(int x, int y, int x1, int y1, int x2, int y2, bool bLinePointOrderHasBeenFixedAlready, bool bOnlyTrueIfOnSegment) {
            return PointIsOnLine(x, y, x1, y1, x2, y2, bLinePointOrderHasBeenFixedAlready, bOnlyTrueIfOnSegment, 0, 0);
        }
        ///<summary>
        ///send line_r of zero in order to calculate r and theta for relative line polar coords.
        ///</summary>
        public static bool PointIsOnLine(int x, int y, int x1, int y1, int x2, int y2, bool bLinePointOrderHasBeenFixedAlready, bool bOnlyTrueIfOnSegment, int line_r, int line_theta) {
            if (!bLinePointOrderHasBeenFixedAlready) OrderPointsLR(ref x1, ref y1, ref x2, ref y2);
            int relative_r,relative_theta;
            bool bReturn=false;
            if (bOnlyTrueIfOnSegment) {
                if (!IsInBox(x,y,x1,x2,y1,y2))
                    return false;//MUST force return now for this to work
            }
            if (line_r==0) RConvert.RectToPolar(out line_r, out line_theta, x2-x1, y2-y1);
            OrderPointsLR(ref x1, ref y1, ref x, ref y);//makes point 2 to the right (may be above or below)
            RConvert.RectToPolar(out relative_r, out relative_theta, x-x1, y-y1);//this is right --subtraction is right since x was placed AFTER x1
            if (relative_theta==line_theta) bReturn=true;
            return bReturn;
        }//end PointIsOnLine
        public static int Intersection(out int x, out int y, ILine line1, ILine line2) {
            return Intersection(out x, out y, line1.X1, line1.Y1, line1.X2, line1.Y2,
                                                         line2.X1, line2.Y1, line2.X2, line2.Y2);
        }
        public static int Intersection(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2) {
            return Intersection(out x, out y, Line1_x1, Line1_y1, Line1_x2, Line1_y2, Line2_x1, Line2_y1, Line2_x2, Line2_y2, true);
        }
        public static int Intersection(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2, bool bReturn1IfWithinSegmentElseIfNotThen2_IfThisVarIsFalseOrMissingAndDoesIntersectThenReturn1) {
            x=0;
            y=0;
            int iReturn=IntersectionError;
            try {
    
                int Line1_A, Line1_B, Line1_C, Line2_A, Line2_B, Line2_C;//line in format Ax+Bx==C
                Line1_A = Line1_y2-Line1_y1;
                Line1_B = Line1_x1-Line1_x2;
                Line1_C = Line1_A*Line1_x1+Line1_B*Line1_y1;
                Line2_A = Line2_y2-Line2_y1;
                Line2_B = Line2_x1-Line2_x2;
                Line2_C = Line2_A*Line2_x1+Line2_B*Line2_y1;
                int iDeterminant = Line1_A*Line2_B - Line2_A*Line1_B;
                if (iDeterminant==0) {
                    iReturn=IntersectionNo;
                }
                else {
                    x=(Line2_B*Line1_C - Line1_B*Line2_C)/iDeterminant;
                    y=(Line1_A*Line2_C - Line2_A*Line1_C)/iDeterminant;
                    if (bReturn1IfWithinSegmentElseIfNotThen2_IfThisVarIsFalseOrMissingAndDoesIntersectThenReturn1) {
                        if (  IsInBox(x,y,Line1_x1,Line1_y1,Line1_x2,Line1_y2)
                            && IsInBox(x,y,Line2_x1,Line2_y1,Line2_x2,Line2_y2) ) iReturn=IntersectionYes;
                        else iReturn=IntersectionBeyondSegment;
                    }
                    else iReturn=IntersectionYes;
                }
            }
            catch (Exception e) {
                iReturn=IntersectionError;
                RReporting.ShowExn(e,"Base Intersection","checking line intersection {"
                    +"Line1:("+Line1_x1.ToString()+","+Line1_y1.ToString()+")to("+Line1_x2.ToString()+","+Line1_y2.ToString()+"); "
                    +"Line2:("+Line2_x1.ToString()+","+Line2_y1.ToString()+")to("+Line2_x2.ToString()+","+Line2_y2.ToString()+"); "
                    +"}");
            }
            return iReturn;
        }//end Intersection
        public static int SafeIntersection(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2) {
            return SafeIntersection(out x, out y, Line1_x1, Line1_y1, Line1_x2, Line1_y2, Line2_x1, Line2_y1, Line2_x2, Line2_y2, true);
        }
        public static int SafeIntersection(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2, bool bReturn1IfWithinSegmentElseIfNotThen2_IfThisVarIsFalseOrMissingAndDoesIntersectThenReturn1) {
            x=0;
            y=0;
            int iReturn=IntersectionError;//should NEVER happen!
            int Line1_A, Line1_B, Line1_C, Line2_A, Line2_B, Line2_C;//line in format Ax+Bx==C
            Line1_A = SafeSubtract(Line1_y2,Line1_y1);
            Line1_B = SafeSubtract(Line1_x1,Line1_x2);
            Line1_C = SafeAdd(SafeMultiply(Line1_A,Line1_x1),SafeMultiply(Line1_B,Line1_y1));
            Line2_A = SafeSubtract(Line2_y2,Line2_y1);
            Line2_B = SafeSubtract(Line2_x1,Line2_x2);
            Line2_C = SafeAdd(SafeMultiply(Line2_A,Line2_x1),SafeMultiply(Line2_B,Line2_y1));
            int iDeterminant=SafeSubtract(SafeMultiply(Line1_A,Line2_B),SafeMultiply(Line2_A,Line1_B));
            if (iDeterminant==0) {
                iReturn=IntersectionNo;
            }
            else {
                x=SafeDivide( SafeSubtract(SafeMultiply(Line2_B,Line1_C),SafeMultiply(Line1_B,Line2_C)) , iDeterminant,int.MaxValue );
                y=SafeDivide( SafeSubtract(SafeMultiply(Line1_A,Line2_C),SafeMultiply(Line2_A,Line1_C)), iDeterminant,int.MaxValue );
                if (bReturn1IfWithinSegmentElseIfNotThen2_IfThisVarIsFalseOrMissingAndDoesIntersectThenReturn1) {
                    if (  IsInBox(x,y,Line1_x1,Line1_y1,Line1_x2,Line1_y2)
                        && IsInBox(x,y,Line1_x1,Line1_y1,Line1_x2,Line1_y2) ) iReturn=IntersectionYes;
                    else iReturn=IntersectionBeyondSegment;
                }
                else iReturn=IntersectionYes;
            }
            return iReturn;
        }//end SafeIntersection
        
        //TODO: test IntersectionAndRelationship
        public static int IntersectionAndRelationship(out int x, out int y, int Line1_x1, int Line1_y1, int Line1_x2, int Line1_y2, int Line2_x1, int Line2_y1, int Line2_x2, int Line2_y2) {
            int Line1_r, Line1_theta, Line2_r, Line2_theta, relative_r, relative_theta;
            int iIntersection=Intersection(out x, out y, Line1_x1, Line1_y1, Line1_x2, Line1_y2, Line2_x1, Line2_y1, Line2_x2, Line2_y2);
            x=0; y=0;
            if (iIntersection!=IntersectionYes) {//Line1_theta==Line2_theta
                if (Line1_x1==Line2_x2&&Line1_y1==Line2_y2) {
                    return LineRelationshipParallelSameLine;
                }
                else {
                    RConvert.RectToPolar(out Line1_r, out Line1_theta, Line1_x2-Line1_x1, Line1_y2-Line1_y1);
                    RConvert.RectToPolar(out relative_r, out relative_theta, Line2_x2-Line1_x1, Line2_y2-Line1_y1);
                    if (relative_theta==Line1_theta) {
                        return LineRelationshipParallelSameLine;//another way of finding if same line
                    }
                }
                return LineRelationshipParallelDifferentLine;
            }
            else {
                OrderPointsLR(ref Line1_x1, ref Line1_y1, ref Line1_x2, ref Line1_y2);
                OrderPointsLR(ref Line2_x1, ref Line2_y1, ref Line2_x2, ref Line2_y2);
                RConvert.RectToPolar(out Line1_r, out Line1_theta, Line1_x2-Line1_x1, Line1_y2-Line1_y1);
                RConvert.RectToPolar(out Line2_r, out Line2_theta, Line2_x2-Line2_x1, Line2_y2-Line2_y1);
                try {
                    if (RMath.PointIsOnLine(Line2_x1,Line2_y1,Line1_x1,Line1_y1,Line1_x2,Line1_y2,true,true,Line1_theta,Line1_r)) {
                        x=Line2_x1;
                        y=Line2_y1;
                        return LineRelationshipLineBPoint1IsOnLineA;
                    }
                    else if (PointIsOnLine(Line2_x2,Line2_y2,Line1_x1,Line1_y1,Line1_x2,Line1_y2,true,true,Line1_theta,Line1_r)) {
                        x=Line2_x2;
                        y=Line2_y2;
                        return LineRelationshipLineBPoint2IsOnLineA;
                    }
                    else if (iIntersection==IntersectionYes) {
                        if (IsInBox(x,y,Line1_x1,Line1_y1,Line1_x2,Line1_y2)&&IsInBox(x,y,Line2_x1,Line2_y1,Line2_x2,Line2_y2)) return LineRelationshipIntersectionNotTouchingEndsOfLineB;
                        else return LineRelationshipIntersectionOutOfRange;
                    }
                    else {
                        return LineRelationshipIntersectionOutOfRange;
                    }
                }
                catch (Exception e) {
                    RReporting.ShowExn(e,"Base IntersectionAndRelationship","checking line relationship {"
                        +"Line1:("+Line1_x1.ToString()+","+Line1_y1.ToString()+")to("+Line1_x2.ToString()+","+Line1_y2.ToString()+"); "
                        +"Line2:("+Line2_x1.ToString()+","+Line2_y1.ToString()+")to("+Line2_x2.ToString()+","+Line2_y2.ToString()+"); "
                        +"}");
                    return LineRelationshipIntersectionOutOfRange;
                }
            }
            return LineRelationshipNone;
        }//end IntersectionAndRelationship
        /// <summary>
        /// Safely returns arctangent of y/x, correcting
        /// for the error domain.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns>arctangent of y/x, correcting
        /// for the error domain</returns>
        public static float SafeAtan2Radians(float y, float x) {
            if (x==0&&y==0) return 0;
            else return (float)Math.Atan2((double)y,(double)x);//debug assumes no need for RConvert.ToFloat since started with float range (?)
        }
        //public static REAL SafeAtan2Radians(REAL y, REAL x) {
        //    if (x==0&&y==0) return 0;
        //    else return (REAL)Math.Atan2((double)y,(double)x);
        //}
        public static double SafeAtan2Radians(double y, double x) {
            if (x==0&&y==0) return 0;
            else return Math.Atan2(y,x);
        }
        #endregion geometry
        
        #region math
        public static int AlignC(int widthOuter, int widthInner) {//formerly Centered
            return (widthOuter-widthInner)/2;
        }
        public static float AlignC(float widthOuter, float widthInner) {
            return (widthOuter-widthInner)/2.0f;
        }
        public static double AlignC(double widthOuter, double widthInner) {
            return (widthOuter-widthInner)/2.0;
        }
        public static decimal AlignC(decimal widthOuter, decimal widthInner) {
            return (widthOuter-widthInner)/2.0M;
        }
        //TODO: test this--all wrapping methods!
        public static int Length(int iStart, int iInclusiveEnder) {
            return LengthEx(iStart,iInclusiveEnder+1);
        }
        public static int LengthEx(int iStart, int iExclusiveEnder) {
            return iExclusiveEnder-iStart;
        }
        public static int Wrap(int iWrap, int length) {
            return (iWrap>=length) ? (iWrap%=length) : ((iWrap<0)?(length-((-iWrap)%length)):(iWrap)) ;
        }
        public static float Wrap(float val, float start, float endexclusive) {
            float range=endexclusive-start;
            if (val>=endexclusive) {
                val-=RConvert.ToFloat(range*(System.Math.Floor((val-endexclusive)/range)+1.0F));
            }
            else if (val<start) {
                val+=RConvert.ToFloat(range*(System.Math.Floor((start-val)/range)+1.0F)); //i.e. -256.5 from 0 excluding 256 is:
                //-256.5+256*(Floor((0[--a.k.a.+]256.5)/256)+1)==-256.5+256*(Floor(256.5/256)+1)==-256.5+256*(1+1)==-256.5+512==255.5
            }
            return val;
        }
        public static double Wrap(double val, double start, double endexclusive) {
            double range=endexclusive-start;
            if (val>=endexclusive) {
                val-=range*(System.Math.Floor((val-endexclusive)/range)+1);
            }
            else if (val<start) {
                val+=range*(System.Math.Floor((start-val)/range)+1); //i.e. -256.5 from 0 excluding 256 is:
                //-256.5+256*(Floor((0[--a.k.a.+]256.5)/256)+1)==-256.5+256*(Floor(256.5/256)+1)==-256.5+256*(1+1)==-256.5+512==255.5
            }
            return val;
        }
        public static decimal Wrap(decimal val, decimal start, decimal endexclusive) {
            decimal range=endexclusive-start;
            if (val>=endexclusive) {
                val-=range*(Floor((val-endexclusive)/range)+1.0M);
            }
            else if (val<start) {
                val+=range*(Floor((start-val)/range)+1.0M); //i.e. -256.5 from 0 excluding 256 is:
                //-256.5+256*(Floor((0[--a.k.a.+]256.5)/256)+1)==-256.5+256*(Floor(256.5/256)+1)==-256.5+256*(1+1)==-256.5+512==255.5
            }
            return val;
        }
        public static decimal Floor(decimal val) {//debug performance VERY SLOW
            string sVal=val.ToString();
            RMath.RemoveExpNotation(ref sVal);
            int iDot=sVal.IndexOf(".");
            if (iDot>=0) {
                sVal=sVal.Substring(0,iDot);
                return RConvert.ToDecimal(sVal);
            }
            else return val;
        }
        public static byte Dist(byte by1, byte by2) {
            return (byte)( (by1>by2)?by1-by2:by2-by1 );
        }
        public static double Dist(ref DPoint p1, ref DPoint p2) {
            try {
                return SafeSqrt(System.Math.Abs(p2.X-p1.X)+System.Math.Abs(p2.Y-p1.Y));
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"double Dist()");
            }
            return 0;
        }
        /* 
        00002 Copyright (c) 2000-2003, Jelle Kok, University of Amsterdam
        00003 All rights reserved.
        00004 
        00005 Redistribution and use in source and binary forms, with or without
        00006 modification, are permitted provided that the following conditions are met:
        00007 
        00008 1. Redistributions of source code must retain the above copyright notice, this
        00009 list of conditions and the following disclaimer.
        00010 
        00011 2. Redistributions in binary form must reproduce the above copyright notice,
        00012 this list of conditions and the following disclaimer in the documentation
        00013 and/or other materials provided with the distribution.
        00014 
        00015 3. Neither the name of the University of Amsterdam nor the names of its
        00016 contributors may be used to endorse or promote products derived from this
        00017 software without specific prior written permission.
        00018 
        00019 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
        00020 AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
        00021 IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
        00022 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
        00023 FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
        00024 DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
        00025 SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
        00026 CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
        00027 OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
        00028 OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
        00029 */
        int GetCircleIntersectionPoints(ref DPoint p1, ref DPoint p2, double c1_X, double c1_Y, double c1_Radius, double c2_X, double c2_Y, double c2_Radius) { //from "int Circle::getIntersectionPoints( Circle c, VecPosition *p1, VecPosition *p2) {" <http://staff.science.uva.nl/~jellekok/robocup/2003/html/Geometry_8cpp-source.html> 2009-08-14
            double x0, y0, r0;
            double x1, y1, r1;
            
            x0 = c1_X;
            y0 = c1_Y;
            r0 = c1_Radius;
            x1 = c2_X;
            y1 = c2_Y;
            r1 = c2_Radius;
            
            double d, dx, dy, h, a, p2_x, p2_y; //x, y,
            
            // first calculate distance between two centers circles P0 and P1.
            dx = x1 - x0;
            dy = y1 - y0;
            d = Math.Sqrt(dx*dx + dy*dy);
            
            // normalize differences
            dx /= d; dy /= d;
            
            // a is distance between p0 and point that is the intersection point P2
            // that intersects P0-P1 and the line that crosses the two intersection
            // points P3 and P4.
            // Define two triangles: P0,P2,P3 and P1,P2,P3.
            // with distances a, h, r0 and b, h, r1 with d = a + b
            // We know a^2 + h^2 = r0^2 and b^2 + h^2 = r1^2 which then gives
            // a^2 + r1^2 - b^2 = r0^2 with d = a + b ==> a^2 + r1^2 - (d-a)^2 = r0^2
            // ==> r0^2 + d^2 - r1^2 / 2*d
            a = (r0*r0 + d*d - r1*r1) / (2.0 * d);
            
            // h is then a^2 + h^2 = r0^2 ==> h = sqrt( r0^2 - a^2 )
            double arg = r0*r0 - a*a;
            h = (arg > 0.0) ? Math.Sqrt(arg) : 0.0;
            
            // First calculate P2
            p2_x = x0 + a * dx;
            p2_y = y0 + a * dy;
            
            // and finally the two intersection points
            p1.X =  p2_x - h * dy;
            p1.Y =  p2_y + h * dx;
            p2.X =  p2_x + h * dy;
            p2.Y =  p2_y - h * dx;
            
            return (arg < 0.0) ? 0 : ((arg == 0.0 ) ? 1 :  2);
        }//end GetCircleIntersectionPoints
        public static void GetPointOnLineByRatio(ref DPoint dpReturn, DPoint p1, DPoint p2, double dRatioTo1) { //VecPosition VecPosition::getVecPositionOnLineFraction( VecPosition &p,
            // determine point on line that lies at fraction dRatioTo1 of whole line
            // example: this --- 0.25 ---------  p2
            // formula: this + dRatioTo1 * ( p2 - p1 ) = p1 - dRatioTo1 * p1 + dRatioTo1 * p2 =
            //         ( 1 - dRatioTo1 ) * p1 + dRatioTo1 * p2
            
            dpReturn.X=( ( p1.X ) * ( 1.0 - dRatioTo1 ) + ( p2.X * dRatioTo1 ) );
            dpReturn.Y=( ( p1.Y ) * ( 1.0 - dRatioTo1 ) + ( p2.Y * dRatioTo1 ) );
        }
        double GetAreaOfCirclesIntersection(double c1_X, double c1_Y, double c1_Radius, double c2_X, double c2_Y, double c2_Radius) { //from "double Circle::getIntersectionArea( Circle c ) {" <http://staff.science.uva.nl/~jellekok/robocup/2003/html/Geometry_8cpp-source.html> 2009-08-14
            DPoint pos1=new DPoint(), pos2=new DPoint(), pos3=new DPoint();//VecPosition pos1, pos2, pos3;
            double d, h, dArea=0.0;
            double ang;//AngDeg ang;
            try {
                d = RMath.Dist(c1_X,c1_Y,c2_X,c2_Y);//getCenter().getDistanceTo( c2_Center() ); // dist between two centers
                if( d > c2_Radius + c1_Radius )           // larger than sum radii
                    return 0.0;                                   // circles do not intersect
                if( d <= Math.Abs(c2_Radius - c1_Radius ) ) {  // one totally in the other
                    double dR = Math.Min( c2_Radius, c1_Radius );// return area smallest circ
                    return RMath.DPI*dR*dR;
                }
                int iNrSol = GetCircleIntersectionPoints(ref pos1, ref pos2, c1_X,c1_Y,c1_Radius,c2_X,c2_Y,c2_Radius);
                if( iNrSol != 2 )
                    return 0.0;
                // the intersection area of two circles can be divided into two segments:
                // left and right of the line between the two intersection points p1 and p2.
                // The outside area of each segment can be calculated by taking the part
                // of the circle pie excluding the triangle from the center to the
                // two intersection points.
                // The pie equals pi*r^2 * rad(2*ang) / 2*pi = 0.5*rad(2*ang)*r^2 with ang
                // the angle between the center c of the circle and one of the two
                // intersection points. Thus the angle between c and p1 and c and p3 where
                // p3 is the point that lies halfway between p1 and p2.
                // This can be calculated using ang = asin( d / r ) with d the distance
                // between p1 and p3 and r the radius of the circle.
                // The area of the triangle is 2*0.5*h*d.
                GetPointOnLineByRatio(ref pos3, pos1, pos2, 0.5 );//pos3 = pos1.getVecPositionOnLineFraction( pos2, 0.5 );
                d = RMath.Dist(ref pos1,ref pos3);
                h = RMath.Dist(pos3.X, pos3.Y, c1_X, c1_Y); //pos3.getDistanceTo( getCenter() );
                ang = Math.Asin(d/c1_Radius);
                dArea = ang*c1_Radius*c1_Radius;
                dArea = dArea - d*h;
                // and now for the other segment the same story
                h =  RMath.Dist(pos3.X, pos3.Y, c2_X, c2_Y); //pos3.getDistanceTo( c2_Center() );
                ang = Math.Asin(d/c2_Radius);
                dArea = dArea + ang*c2_Radius*c2_Radius;
                dArea = dArea - d*h;
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return dArea;
        }//end GetAreaOfCirclesIntersection

        public static double Dist(double x1, double y1, double x2, double y2) {
            try {
                return SafeSqrt(System.Math.Abs(x2-x1)+System.Math.Abs(y2-y1));
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"double Dist()");
            }
            return 0;
        }
        public static float Dist(float x1, float y1, float x2, float y2) {
            try {
                return SafeSqrt(System.Math.Abs(x2-x1)+System.Math.Abs(y2-y1));
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"double Dist()");
            }
            return 0;
        }
        public static int Dist1D(int x1, int x2) {
            int valReturn=0;
            try {
                valReturn=SafeSubtract(x1,x2);
                if (valReturn<0) valReturn=SafeSubtract(0,valReturn);//still need SafeSubtract since negative and positive limits are not the same
            }
            catch {
                valReturn=int.MaxValue;
            }
            return valReturn;
        }
        public static int Dist(int x1, int y1, int x2, int y2) {
            try {
                return SafeSqrt(System.Math.Abs(x2-x1)+System.Math.Abs(y2-y2));
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"double Dist()");
            }
            return 0;
        }
        public static float Dist(ref FPoint p1, ref FPoint p2) {
            try {
                return SafeSqrt(System.Math.Abs(p2.X-p1.X)+System.Math.Abs(p2.Y-p1.Y));
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"float Dist()");
            }
            return 0;
        }
        public static decimal Dist(ref MPoint p1, ref MPoint p2) {
            try {
                return SafeSqrt(System.Math.Abs(p2.X-p1.X)+System.Math.Abs(p2.Y-p1.Y));
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"Dist(MPoint, MPoint)");
            }
            return 0;
        }
        public static float SafeDivide(float val, float valDivisor, float valMax) {
            return SafeDivide(val,valDivisor,valMax,-valMax);
        }
        public static double SafeDivide(double val, double valDivisor, double valMax) {
            return SafeDivide(val,valDivisor,valMax,-valMax);
        }
        public static decimal SafeDivide(decimal val, decimal valDivisor, decimal valMax) {
            return SafeDivide(val,valDivisor,valMax,-valMax);
        }
        public static float SafeDivide(float val, float valDivisor, float valMax, float valMin) {
            try {
                bool bSameSign=(val<0.0f&&valDivisor<0.0f)?true:((val>=0.0f&&valDivisor>=0.0f)?true:false);
                if (valDivisor==0f) {
                    if (val>0f) return valMax;
                    else if (val<0f) return valMin;
                    else return 0f;
                }
                //replaced by +inf and -inf below //else if (float.IsInfinity(val)) return valMax;
                else if (float.IsPositiveInfinity(val)) {
                    if (float.IsPositiveInfinity(valDivisor)) return 1.0f;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (float.IsNegativeInfinity(valDivisor)) return -1f;
                        else return valMin; //since not same sign
                    }
                }
                else if (float.IsNegativeInfinity(val)) {
                    if (float.IsNegativeInfinity(valDivisor)) return 1.0f;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (float.IsPositiveInfinity(valDivisor)) return -1f;
                        else return valMin; //since not same sign (i.e. -inf/2.0)
                    }
                }
                else if (float.IsPositiveInfinity(valDivisor)) {
                    if (float.IsPositiveInfinity(val)) return 1.0f;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (float.IsNegativeInfinity(val)) return -1f;
                        else return valMin;
                    }
                }
                else if (float.IsNegativeInfinity(valDivisor)) {
                    if (float.IsNegativeInfinity(val)) return 1.0f;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (float.IsNegativeInfinity(val)) return 1f;
                        else return valMin;
                    }
                }
                    //TODO: finish this (cases of inf or -inf denominator)
                //TODO: output error if NaN?
                else if (float.IsNaN(val)) return 0f;
                else if (float.IsNaN(valDivisor)) return 0f;
                else return val/valDivisor;
            }
            catch (Exception e)  {
                RReporting.ShowExn(e,"SafeDivide("+val.ToString()+","+valDivisor.ToString()+","+valMax.ToString()+")");
            }
            return 0f;
        } //end SafeDivide(float,float,float,float)
        public static double SafeDivide(double val, double valDivisor, double valMax, double valMin) {
            try {
                bool bSameSign=(val<0.0&&valDivisor<0.0)?true:((val>=0.0&&valDivisor>=0.0)?true:false);
                if (valDivisor==0.0) {
                    if (val>0.0) return valMax;
                    else if (val<0.0) return valMin;
                    else return 0.0;
                }
                //replaced by +inf and -inf below //else if (double.IsInfinity(val)) return valMax;
                else if (double.IsPositiveInfinity(val)) {
                    if (double.IsPositiveInfinity(valDivisor)) return 1.0;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (double.IsNegativeInfinity(valDivisor)) return -1.0;
                        else return valMin; //since not same sign
                    }
                }
                else if (double.IsNegativeInfinity(val)) {
                    if (double.IsNegativeInfinity(valDivisor)) return 1.0;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (double.IsPositiveInfinity(valDivisor)) return -1.0;
                        else return valMin; //since not same sign (i.e. -inf/2.0)
                    }
                }
                else if (double.IsPositiveInfinity(valDivisor)) {
                    if (double.IsPositiveInfinity(val)) return 1.0;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (double.IsNegativeInfinity(val)) return -1.0;
                        else return valMin;
                    }
                }
                else if (double.IsNegativeInfinity(valDivisor)) {
                    if (double.IsNegativeInfinity(val)) return 1.0;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (double.IsNegativeInfinity(val)) return 1.0;
                        else return valMin;
                    }
                }
                    //TODO: finish this (cases of inf or -inf denominator)
                //TODO: output error if NaN?
                else if (double.IsNaN(val)) return 0.0;
                else if (double.IsNaN(valDivisor)) return 0.0;
                else return val/valDivisor;
            }
            catch (Exception e)  {
                RReporting.ShowExn(e,"SafeDivide("+val.ToString()+","+valDivisor.ToString()+","+valMax.ToString()+")");
            }
            return 0.0;
        } //end SafeDivide(double,double,double,double)
        public static decimal SafeDivide(decimal val, decimal valDivisor, decimal valMax, decimal valMin) {
            try {
                bool bSameSign=(val<0.0M&&valDivisor<0.0M)?true:((val>=0.0M&&valDivisor>=0.0M)?true:false);
                if (valDivisor==0.0M) {
                    if (val>0.0M) return valMax;
                    else if (val<0.0M) return valMin;
                    else return 0.0M;
                }
                //replaced by +inf and -inf below //else if (decimal.IsInfinity(val)) return valMax;
                /*
                else if (decimal.IsPositiveInfinity(val)) {
                    if (decimal.IsPositiveInfinity(valDivisor)) return 1.0M;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (decimal.IsNegativeInfinity(valDivisor)) return -1.0M;
                        else return valMin; //since not same sign
                    }
                }
                else if (decimal.IsNegativeInfinity(val)) {
                    if (decimal.IsNegativeInfinity(valDivisor)) return 1.0M;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (decimal.IsPositiveInfinity(valDivisor)) return -1.0M;
                        else return valMin; //since not same sign (i.e. -inf/2.0)
                    }
                }
                else if (decimal.IsPositiveInfinity(valDivisor)) {
                    if (decimal.IsPositiveInfinity(val)) return 1.0M;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (decimal.IsNegativeInfinity(val)) return -1.0M;
                        else return valMin;
                    }
                }
                else if (decimal.IsNegativeInfinity(valDivisor)) {
                    if (decimal.IsNegativeInfinity(val)) return 1.0M;
                    else if (bSameSign) {
                        return valMax;
                    }
                    else {
                        if (decimal.IsNegativeInfinity(val)) return 1.0M;
                        else return valMin;
                    }
                }
                */
                //TODO: finish this (cases of inf or -inf denominator)
                //TODO: output error if NaN?
                //else if (decimal.IsNaN(val)) return 0.0M;
                //else if (decimal.IsNaN(valDivisor)) return 0.0M;
                else return val/valDivisor;
            }
            catch (Exception e)  {
                RReporting.ShowExn(e,"SafeDivide("+val.ToString()+","+valDivisor.ToString()+","+valMax.ToString()+")");
            }
            return 0.0M;
        } //end SafeDivide(decimal,decimal,decimal,decimal)

        public static int SafeAbs(int val) {
            SafeAbsByRef(ref val);
            return val;
        }
        public static void SafeAbsByRef(ref int val) {
            if (val<0) {
                if (val<int.MaxValue*-1) val=int.MaxValue;
                else val*=-1;
            }
        }
        public static float SafeAbs(float val) {
            SafeAbsByRef(ref val);
            return val;
        }
        public static void SafeAbsByRef(ref float val) {
            if (val<0) {
                if (val<float.MaxValue*-1.0f) val=float.MaxValue;
                else val*=-1;
            }
        }
        public static int Negative(int val) {
            NegativeByRef(ref val);
            return val;
        }
        public static void NegativeByRef(ref int val) {
            if (val>0) {
                val*=-1;
            }
        }
        public static int SafeDivideRound(int val, int valDivisor, int valMax) {
            int iReturn=SafeDivide(val,valDivisor,valMax,0);
            if (valDivisor!=0) {
                bool bNeg=false;
                if (val<0) bNeg=!bNeg;
                if (valDivisor<0) bNeg=!bNeg;
                //now do rounding manually:
                if (!bNeg) {
                    if (val%valDivisor>SafeAbs(SafeDivide(valDivisor,2,valMax,0))) iReturn++;
                }
                else {
                    if (val%valDivisor>Negative(SafeDivide(valDivisor,2,valMax,0))) iReturn--;
                }
            }
            return iReturn;
        }
        public static int SafeDivideRound(int val, int valDivisor, int valMax, int valMin) {
            int iReturn=SafeDivide(val,valDivisor,valMax,valMin);
            if (valDivisor!=0) {
                bool bNeg=false;
                if (val<0) bNeg=!bNeg;
                if (valDivisor<0) bNeg=!bNeg;
                if (!bNeg) {
                    if (val%valDivisor>SafeAbs(SafeDivide(valDivisor,2,valMax,valMin))) iReturn++;
                }
                else {
                    if (val%valDivisor>Negative(SafeDivide(valDivisor,2,valMax,valMin))) iReturn--;
                }
            }
            return iReturn;
        }
        public static int SafeDivide(int val, int valDivisor, int valMax) {
            return SafeDivide(val,valDivisor,valMax,-valMax);
        }
        public static int SafeDivide(int val, int valDivisor, int valMax, int valMin) {
            bool bNeg=false;
            try {
                bool bSameSign=(val<0.0&&valDivisor<0.0)?true:((val>=0.0&&valDivisor>=0.0)?true:false);
                if (valDivisor==0) {
                    if (val>0) return valMax;
                    else if (val<0) return valMin;
                    else return 0;
                }
                else {
                    if (val<0) {
                        val*=-1;
                        bNeg=!bNeg;
                    }
                    if (valDivisor<0) {
                        valDivisor*=-1;
                        bNeg=!bNeg;
                    }
                }
                try {
                    return bNeg?(val/valDivisor*-1):(val/valDivisor);
                }
                catch {
                    return bNeg? ((val>valDivisor?valMax:valMin)*-1) : (val>valDivisor?valMax:valMin);
                }
            }
            catch (Exception e)  {//should NEVER happen
                RReporting.ShowExn(e,"SafeDivide","dividing "+val.ToString()+" by "+valDivisor.ToString()+" (unexpected crash) {min:"+valMin.ToString()+"; max="+valMax.ToString()+"}");
            }
            return 0;
        } //end SafeDivide(int...)
        //public const float MaxFloat=;
        //public const float MinFloatAbsVal=;
        public static float FixFloat(float var) {
            if (float.IsNegativeInfinity(var)) {
                var=float.MinValue;
            }
            else if (float.IsPositiveInfinity(var)) {
                var=float.MaxValue;
            }
            else if (float.IsNaN(var)) {
                var=0;
            }
            return var;
        }
        //public const double MaxDouble;
        public static double FixDouble(double var) {
            if (double.IsNegativeInfinity(var)) {
                var=double.MinValue;
            }
            else if (double.IsPositiveInfinity(var)) {
                var=double.MaxValue;
            }
            else if (double.IsNaN(var)) {
                var=0;
            }
            return var;
        }
        public static uint Bit(int iBit) {
            return SafePow((uint)2,iBit);
        }
        //can't be const, since passed by ref
        private static int i10=10;
        private static long l10=10L;
        private static float f10=10F;
        private static double d10=10D;
        private static decimal m10=10M;
        
        public static int SafeE10I(int exp) {
            return SafePow(ref i10, ref exp);
        }
        public static long SafeE10L(int exp) {
            return SafePow(ref l10, ref exp);
        }
        public static float SafeE10F(ref int exp) {
            return SafePow(ref f10, ref exp);
        }
        public static double SafeE10D(ref int exp) {
            return SafePow(ref d10, ref exp);
        }
        public static decimal SafeE10M(ref int exp) {
            return SafePow(ref m10, ref exp);
        }
        public static int SafePow(int basenum, int exp) {
            return SafePow(ref basenum, ref exp);
        }
        public static int SafePow(ref int basenum, ref int exp) {
            if (basenum==0) return 0;
            int result;
            bool bNeg;
            if (exp<0) {
                bNeg=true;
                exp*=-1;
            }
            if (exp==0) return 1;
            else {
                bNeg=false;
                result=basenum;
                try {
                    for (int iCount=1; iCount<exp; iCount++) {
                        //if (result<int.MaxValue-basenum) //useless
                            result*=basenum;
                        //else return int.MaxValue;
                    }
                }
                catch {
                    return int.MaxValue;
                }
            }
            if (bNeg) {
                result=1/result;
                exp*=-1;//leaves it the way we found it
            }
            return result;
        }
        public static uint SafePow(uint basenum, int exp) {
            return SafePow(ref basenum,ref exp);
        }
        public static uint SafePow(ref uint basenum, ref int exp) {
            if (basenum==0) return 0;
            uint result;
            bool bNeg;
            if (exp<0) {
                bNeg=true;
                exp*=-1;
            }
            if (exp==0) return 1;
            else {
                bNeg=false;
                result=basenum;
                try {
                    for (uint iCount=1; iCount<exp; iCount++) {
                        //if (result<uint.MaxValue-basenum) //useless
                            result*=basenum;
                        //else return uint.MaxValue;
                    }
                }
                catch {
                    return uint.MaxValue;
                }
            }
            if (bNeg) {
                result=1/result;
                exp*=-1;//leaves it the way we found it
            }
            return result;
        }
        public static long SafePow(long basenum, int exp) {
            return SafePow(ref basenum, ref exp);
        }
        public static long SafePow(ref long basenum, ref int exp) {
            if (basenum==0) return 0;
            long result;
            bool bNeg;
            if (exp<0) {
                bNeg=true;
                exp*=-1;
            }
            if (exp==0) return 1;
            else {
                bNeg=false;
                result=basenum;
                try {
                    for (int iCount=1; iCount<exp; iCount++) {
                        //if (result<long.MaxValue-basenum) //useless
                            result*=basenum;
                        //else return long.MaxValue;
                    }
                }
                catch {
                    return long.MaxValue;
                }
            }
            if (bNeg) {
                result=1L/result;
                exp*=-1;//leaves it the way we found it
            }
            return result;
        }
        public static float SafePow(float basenum, int exp) {
            return SafePow(ref basenum, ref exp);
        }
        public static float SafePow(ref float basenum, ref int exp) {
            float result;
            bool bNeg;
            if (exp<0) {
                bNeg=true;
                exp*=-1;
            }
            if (exp==0) return 1;
            else {
                bNeg=false;
                result=basenum;
                try {
                    for (int iCount=1; iCount<exp; iCount++) {
                        //if (result<float.MaxValue-basenum) //useless
                            result*=basenum;
                        //else return float.MaxValue;
                    }
                }
                catch {
                    return float.MaxValue;
                }
            }
            if (bNeg) {
                if (result!=0.0f) result=1F/result; //check now since may have degraded to zero if started at <1.0f
                exp*=-1;//leaves it the way we found it
            }
            return result;
        }
        public static double SafePow(double basenum, int exp) {
            return SafePow(ref basenum, ref exp);
        }
        public static double SafePow(ref double basenum, ref int exp) {
            double result;
            bool bNeg;
            if (exp<0) {
                bNeg=true;
                exp*=-1;
            }
            if (exp==0) return 1;
            else {
                bNeg=false;
                result=basenum;
                try {
                    for (int iCount=1; iCount<exp; iCount++) {
                        //if (result<double.MaxValue-basenum) //useless
                            result*=basenum;
                        //else return double.MaxValue;
                    }
                }
                catch {
                    return double.MaxValue;
                }
            }
            if (bNeg) {
                if (result!=0.0D) result=1D/result; //check now since may have degraded to zero if started at <1.0                
                exp*=-1; //leaves it the way we found it
            }
            return result;
        }
        public static decimal SafePow(decimal basenum, int exp) {
            return SafePow(ref basenum, ref exp);
        }
        public static decimal SafePow(ref decimal basenum, ref int exp) {
            decimal result;
            bool bNeg;
            if (exp<0) {
                bNeg=true;
                exp*=-1;
            }
            if (exp==0) return 1M;
            else {
                bNeg=false;
                result=basenum;
                try {
                    for (int iCount=1; iCount<exp; iCount++) {//start at once since set result=basenum already
                        //if (result<decimal.MaxValue-basenum) //useless
                            result*=basenum;
                        //else return decimal.MaxValue;
                    }
                }
                catch {
                    return decimal.MaxValue;
                }
            }
            if (bNeg) {
                if (result!=0.0M) result=1M/result; //check now since may have degraded to zero if started at <1.0                
                exp*=-1; //leaves it the way we found it
            }
            return result;
        }
        public static void Rotate(ref float xToMove, ref float yToMove, float xCenter, float yCenter, float fRotate) {
            xToMove-=xCenter;
            yToMove-=yCenter;
            float rTemp=RConvert.ROFXY(xToMove,yToMove), thetaTemp=RConvert.THETAOFXY_RAD(xToMove,yToMove);
            thetaTemp+=fRotate;
            xToMove=RConvert.XOFRTHETA_RAD(rTemp,thetaTemp);
            yToMove=RConvert.YOFRTHETA_RAD(rTemp,thetaTemp);
            xToMove+=xCenter;
            yToMove+=yCenter;
        }
        public static void Rotate(ref float xToMove, ref float yToMove, float fRotate) {
            float rTemp=RConvert.ROFXY(xToMove,yToMove), thetaTemp=RConvert.THETAOFXY_RAD(xToMove,yToMove);
            thetaTemp+=fRotate;
            xToMove=RConvert.XOFRTHETA_RAD(rTemp,thetaTemp);
            yToMove=RConvert.YOFRTHETA_RAD(rTemp,thetaTemp);
        }
        public static float Round(float val) {
            return RConvert.ToFloat(Math.Round((double)val));
        }
        ///<summary>
        ///Uses AlphaLook array in switched order to get true approach toward "toward" by factor
        ///Returns AlphaLook[toward][start][factor]
        ///</summary>
        public static byte Approach(byte start, byte toward, byte factor) {
            return AlphaLook[toward][start][factor];//intentionally flipped since alpha causes the value to approach SOURCE color, so AlphaLook's reverse order needs to be accounted for
        }
        public static byte Approach(byte start, byte toward, float factor) {
            return factor<=0.0f?start:(factor>=1.0f?toward:Approach(start,toward,ByRound(factor*255.0f)));
            //try {
            //    return ByRound(((float)start)-(((float)start)-((float)toward))*(factor));
            //}
            //catch (Exception e) {
            //    RReporting.Debug(e,"","Base Approach(byte)");
            //    //TODO: make this more accurate
            //    return toward; //check this--may be correct since overflow means too big in the formula above
            //}
        }//end Approach(byte,byte,float)
        public static byte Approach(byte start, byte toward, double factor) {
            return factor<=0.0?start:(factor>=1.0?toward:Approach(start,toward,ByRound(factor*255.0)));
        }//end Approach(byte,byte,float)        
        public static float Approach(float start, float toward, float factor) {
            try {
                return ((start)-((start)-(toward))*(factor));
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Base Approach");
                //TODO: make this more accurate
                return toward; //check this--may be correct since overflow means too big in the formula above
            }
        }
        public static double Approach(double start, double toward, double factor) {
            try {
                return ((start)-((start)-(toward))*(factor));
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Approach");
                //TODO: make this more accurate
                return toward; //check this--may be correct since overflow means too big in the formula above
            }
        }
        //public static float Mod(float num, float div) {
            //float result=num/div;
            //Floor(ref result);
            //result=num-result;
            //return result;
        //}
        public static float Mod(float val, float divisor) { //formerly FMOD
            return ((val>divisor) ? ( val - RMath.Floor(val/divisor)*divisor) : 0.0f );
        }
        public static double Mod(double val, double divisor) { //formerly DMOD
            return ((val>divisor) ? ( val - System.Math.Floor(val/divisor)*divisor) : 0 );
        }
/*
-3        -2        -1         0         1         2
 +--|------+---------+----|----+--|------+----|--|-+
    a                     b       c           d  e

                       a=-2.7  b=-0.5  c=0.3  d=1.5  e=2.8
                       ======  ======  =====  =====  =====
Floor                    -3      -1      0      1      2
Ceiling                  -2       0      1      2      3
Truncate                 -2       0      0      1      2
Round (ToEven)           -3       0      0      2      3
Round (AwayFromZero)     -3      -1      0      2      3

Pax, I think you've got a mistake with: Round(AwayFromZero) -3 -2 1 2 3 Math.Round(-1.2, MidpointRounding.AwayFromZero) == -1 Math.Round(0.3, MidpointRounding.AwayFromZero)==0.0 etc.. – dtroy May 5 at 3:49

Pax Diablo answer for "What’s the difference between Math.Floor() and Math.Truncate() in .NET?" <http://stackoverflow.com/questions/14/whats-the-difference-between-math-floor-and-math-truncate-in-c/580252#580252> 2009-12-12

*/
        ///NOTE: CEILING rounds UP (in positive direction) even if number is negative!
        /// - FLOOR rounds DOWN (in negative direction) even if number is negative!
        /// - TRUNCATE rounds TOWARD ZERO regardless of sign
        public static void Ceiling(ref float val) {
            float valTemp=(float)((long)val);//float has 23 bits for the Significand, plus an 8-bit exponent and 1-bit sign
            if (val>valTemp) val=valTemp+1.0f;
            else val=valTemp;
        }
        public static float CeilingRefToNonRef(ref float val) {
            float valReturn=(float)((long)val);//float has 23 bits for the Significand, plus an 8-bit exponent and 1-bit sign
            if (val>valReturn) return valReturn+1.0f;
            else return valReturn;
        }
        public static float Ceiling(float val) {
            float valReturn=(float)((long)val);//float has 23 bits for the Significand, plus an 8-bit exponent and 1-bit sign
            if (val>valReturn) valReturn+=1.0f;//val=valReturn+1.0f;
            //else val=valReturn;
            return valReturn;
        }
        public static void Ceiling(ref decimal val) {
            if (RReporting.iWarnings<RReporting.iMaxWarnings) {
                Console.Error.WriteLine("Warning: Performance - should have used System.Math.Floor since it is available for this overload");
                RReporting.iWarnings++;
            }
            val=System.Math.Ceiling(val);
            //decimal valReturn=(decimal)((long)val);//TODO: debug overflow of long
            //if (val>valReturn) val=valReturn+1.0f;
            //else val=valReturn;
        }
        public static void Ceiling(ref double val) {
            double valTemp=(double)((long)val);//double has 52 bits for the Significand, plus an 11-bit exponent and 1-bit sign
            if (val>valTemp) val=valTemp+1.0;
            else val=valTemp;
        }
        public static double CeilingRefToNonRef(ref double val) {
            double valReturn=(double)((long)val);//double has 52 bits for the Significand, plus an 11-bit exponent and 1-bit sign
            if (val>valReturn) return valReturn+1.0;
            else return valReturn;
        }
        public static float Floor(float val) {
            Floor(ref val);
            return val;
        }
        public static double Floor(double num) {//use System.Math.Floor, which has double and decimal
            if (RReporting.iWarnings<RReporting.iMaxWarnings) {
                Console.Error.WriteLine("Warning: Performance - should have used System.Math.Floor since it is available for this overload");
                RReporting.iWarnings++;
            }
            return System.Math.Floor(num);
            //RMath.Floor(ref num);
            //return num;
        }
        public static float FloorRefToNonRef(ref float num) {
            float numNew=num;
            Floor(ref numNew);
            return numNew;
        }
        public static void Floor(ref float num) {
            //bool bOverflow=false;//TODO: check for overflow
            if (num!=0F) {
                long whole=(long)num;
                float numTemp=(float)whole;
                if (num<numTemp) num=numTemp-1.0f;
                else num=numTemp;
            }
        }
        
        public static void Floor(ref double num) {
            //Console.Error.WriteLine("Warning: Performance - should have used System.Math.Floor since it is available for this overload");
            num=System.Math.Floor(num);
            /*
            if (num>(double)long.MaxValue) {
                num=(double)long.MaxValue;
            }
            else if (num<(double)long.MinValue) {
                num=(double)long.MinValue;
            }
            else if (num!=0F) {
                long whole=(long)num;
                double numTemp=(double)whole;
                if (num<numTemp) num=numTemp-1.0;
                else num=numTemp;
            }
            */
        }
        public static double FloorRefToNonRef(ref double num) {
            if (RReporting.iWarnings<RReporting.iMaxWarnings) {
                Console.Error.WriteLine("Warning: Performance - should have used System.Math.Floor since it is available for this overload");
                RReporting.iWarnings++;
            }
            return System.Math.Floor(num);
            //double numNew=num;
            //RMath.Floor(ref numNew);
            //return numNew;
        }
        public const uint BIT1        =1;
        public const uint BIT2        =2;
        public const uint BIT3        =4;
        public const uint BIT4        =8;
        public const uint BIT5        =16;
        public const uint BIT6        =32;
        public const uint BIT7        =64;
        public const uint BIT8        =128;
        public const uint BIT9        =256;
        public const uint BIT10        =512;
        public const uint BIT11        =1024;
        public const uint BIT12        =2048;
        public const uint BIT13        =4096;
        public const uint BIT14        =8192;
        public const uint BIT15        =16384;
        public const uint BIT16        =32768;
        public const uint BIT17        =65536;
        public const uint BIT18        =131072;
        public const uint BIT19        =262144;
        public const uint BIT20        =524288;
        public const uint BIT21        =1048576;
        public const uint BIT22        =2097152;
        public const uint BIT23        =4194304;
        public const uint BIT24        =8388608;
        public const uint BIT25        =16777216;
        public const uint BIT26        =33554432;
        public const uint BIT27        =67108864;
        public const uint BIT28        =134217728;
        public const uint BIT29        =268435456;
        public const uint BIT30        =536870912;
        public const uint BIT31        =1073741824;
        public const uint BIT32        =2147483648;
        public static byte LOWNIBBLE(byte by) {
            return (byte)(by%16); //or by&byLowNibble;
        }
        public static byte HIGHNIBBLE(byte by) {
            return (byte)(by/16); //or by>>4;
        }
        public static void SETLOWNIBBLE(ref byte byTarget, byte byNibbleValueMustBeLessthan16) {
            byTarget=(byte)((byte)((byte)(byTarget/16)*16) + byNibbleValueMustBeLessthan16); //assumes by2<16
        }
        public static void SETHIGHNIBBLE(ref byte byTarget, byte byNibbleValueMustBeLessthan16) {
            byTarget=(byte)((byte)(byTarget%16) + (byNibbleValueMustBeLessthan16*16)); //assumes by1<16
        }
        public static byte ByRound(float val) {
            if (val>=254.5f) return 255; //> is okay since it will be truncated below anyway
            else if (val<.5f) return 0;
            return (byte)(val+.5f);
        }
        public static byte ByRound(double val) {
            if (val>=255.5) return 255;
            else if (val<.5) return 0;
            return (byte)(val+.5f);
        }
        public const float f_int_MaxValue=(float)int.MaxValue;
        public const float f_int_MaxValue_minus_point5=(float)int.MaxValue-.5f;
        public const float f_int_MaxValue_minus_1=(float)int.MaxValue-1.0f;
        public const float f_int_MinValue=(float)int.MinValue;
        public const float f_int_MinValue_plus_point5=(float)int.MinValue+.5f;
        public const float f_int_MinValue_plus_1=(float)int.MinValue+1.0f;
        public static int IRound(float val) {
            if (val>=f_int_MaxValue_minus_point5) return int.MaxValue;
            else if (val<f_int_MinValue_plus_point5) return int.MinValue;
            return val<0.0f?(int)(val-.5f):(int)(val+.5f);
        }
        public const double d_int_MaxValue=(double)int.MaxValue;
        public const double d_int_MaxValue_minus_point5=(double)int.MaxValue-.5d;
        public const double d_int_MaxValue_minus_1=(double)int.MaxValue-1.0d;
        public const double d_long_MaxValue_minus_1=(double)long.MaxValue-1.0d;
        public const double d_int_MinValue=(double)int.MinValue;
        public const double d_int_MinValue_plus_point5=(double)int.MinValue+.5d;
        public const double d_int_MinValue_plus_1=(double)int.MinValue+1.0d;
        public const double d_long_MinValue_plus_1=(double)long.MinValue+1.0d;
        public static int IRound(double val) {
            if (val>=d_int_MaxValue_minus_point5) return int.MaxValue;
            else if (val<d_int_MinValue_plus_point5) return int.MinValue;
            return val<0.0d?(int)(val-.5d):(int)(val+.5d);
        }
        public const decimal m_int_MaxValue=(decimal)int.MaxValue;
        public const decimal m_int_MaxValue_minus_point5=(decimal)int.MaxValue-.5m;
        public const decimal m_int_MaxValue_minus_1=(decimal)int.MaxValue-1.0m;
        public const decimal m_long_MaxValue_minus_1=(decimal)long.MaxValue-1.0m;
        public const decimal m_int_MinValue=(decimal)int.MinValue;
        public const decimal m_int_MinValue_plus_point5=(decimal)int.MinValue+.5m;
        public const decimal m_int_MinValue_plus_1=(decimal)int.MinValue+1.0m;
        public const decimal m_long_MinValue_plus_1=(decimal)long.MinValue+1.0m;
        public static int IRound(decimal val) {
            if (val>=m_int_MaxValue_minus_point5) return int.MaxValue;
            else if (val<m_int_MinValue_plus_point5) return int.MinValue;
            return val<0.0M?(int)(val-.5m):(int)(val+.5m);
        }
        public static int ICeiling(float val) {
            if (val>f_int_MaxValue_minus_1) return int.MaxValue;
            else if (val<f_int_MinValue_plus_1) return int.MinValue;
            //since this is ceiling algorithm, it only has to be more than .0 above the figure in order to be rounded UPWARD
            return val>0
                ? ( ((float)val>((float)((int)val)))
                    ? ((int)val+1)
                    : (int)val )
                : ( ((float)val<((float)((int)val)))
                    ? ((int)val-1)
                    : (int)val );
        }
        public static int ICeiling(double val) {
            if (val>d_long_MaxValue_minus_1) return int.MaxValue;
            else if (val<d_long_MinValue_plus_1) return int.MinValue;
            return val>0
                ? ( ((double)val>((double)((int)val)))
                    ? ((int)val+1)
                    : (int)val )
                : ( ((double)val<((int)((int)val)))
                    ? ((int)val-1)
                    : (int)val );
        }
        public static long LCeiling(double val) {
            if (val>d_long_MaxValue_minus_1) return long.MaxValue;
            else if (val<d_long_MinValue_plus_1) return long.MinValue;
            return val>0
                ? ( ((double)val>((double)((long)val)))
                    ? ((long)val+1)
                    : (long)val )
                : ( ((double)val<((long)((long)val)))
                    ? ((long)val-1)
                    : (long)val );
        }
        
        public static void CropAbsoluteValueToPosMax(ref int val) {//formerly PrepareToBePos
        //limits value since IEEE positive range is narrower (PrepareToBeNeg is not needed!)
            if (val<(-1*int.MaxValue)) val=-1*int.MaxValue;
        }
        public static void CropAbsoluteValueToPosMax(ref long val) {
            if (val>(-1*long.MaxValue)) val=-1*long.MaxValue;
        }
        public static void CropAbsoluteValueToPosMax(ref float val) {
            if (val>(-1.0f*float.MaxValue)) val=-1.0f*float.MaxValue;
        }
        public static void CropAbsoluteValueToPosMax(ref double val) {
            if (val>(-1.0*double.MaxValue)) val=-1.0*double.MaxValue;
        }
        public static void CropAbsoluteValueToPosMax(ref decimal val) {
            if (val>(-1.0M*decimal.MaxValue)) val=-1.0M*decimal.MaxValue;
        }
        
        public static float SafeSqrt(float val) {
            if (val>0) return RMath.Sqrt(val);
            else if (val<0) { //debug should not actually return a real number
                CropAbsoluteValueToPosMax(ref val); //avoids overflow when multiplying by -1 below
                return (-1.0f*RMath.Sqrt((-1.0f*val)));
            }
            else return 0;
        }
        public static double SafeSqrt(double val) {
            if (val>0) return Math.Sqrt(val);
            else if (val<0) { //debug should not actually return a real number
                CropAbsoluteValueToPosMax(ref val);
                return -1.0*Math.Sqrt(-1.0D*val);
            }
            else return 0;
        }
        public static float Sqrt(float val) {
            float val1=0.0f;
            float val2;
            while( (val1*val1) <= val ) val1+=0.1f;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0f; val1=val2;
            return val2;
        }//end Sqrt(float)
        public static decimal Sqrt(decimal val) {
            decimal val1=0.0M;
            decimal val2;
            while( (val1*val1) <= val ) val1+=0.1M;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            val2=val; val2/=val1; val2+=val1; val2/=2.0M; val1=val2;
            return val2;
        }//end Sqrt(decimal)
        public static decimal SafeSqrt(decimal val) {
            if (val>0.0M) return RMath.Sqrt(val);
            else if (val<0.0M) { //debug should not actually return a real number
                CropAbsoluteValueToPosMax(ref val);
                return -1.0M*RMath.Sqrt(-1.0M*val);
            }
            else return 0;
        }
        //public static REAL SafeSqrt(REAL val) {
        //    if (val>0) return Math.Sqrt(val);
        //    else if (val<0) {
        //        PrepareToBeNeg(ref val);
        //        return (REAL)-1*Math.Sqrt((double)((REAL)-1*val));
        //    }
        //    else return 0;
        //}
        public static int SafeSqrt(int val) {
            if (val>0) return (int)Math.Sqrt(val);
            else if (val<0) { //debug should not actually return a real number
                CropAbsoluteValueToPosMax(ref val);
                return -1*(int)Math.Sqrt((int)(-1*val));
            }
            else return 0;
        }
        public static long SafeSqrt(long val) {
            if (val>0) return (long)Math.Sqrt(val);
            else if (val<0) { //debug should not actually return a real number
                CropAbsoluteValueToPosMax(ref val);
                return -1L*(long)Math.Sqrt((long)(-1L*val));
            }
            else return 0;
        }
        //public static ulong SafeSqrt(ulong val) {
        //    return (ulong)Math.Sqrt(val);
        //}
        #endregion math

        #region utilities
        
        public static int DaysAbsoluteValueOfDifferenceIgnoringHours(DateTime datetime1, DateTime datetime2) { //formerly DaysAbsoluteDifferenceIgnoringHours
            int iReturn=DaysDifferenceIgnoringHours(datetime1,datetime2);
            if (iReturn<0) iReturn=0-iReturn;
            return iReturn;
        }
        /// <summary>
        /// Gets difference in days (only looks at the day, NOT the time of day), taking leapyear into account.  Not optimized, goes day by day.
        /// </summary>
        /// <param name="datetime1"></param>
        /// <param name="datetime2"></param>
        /// <returns></returns>
        public static int DaysDifferenceIgnoringHours(DateTime datetime1, DateTime datetime2) {
            int iReturn=0;
            bool bNeg=false;
            try {
                DateTime dtDestruct=(DateTime)datetime1;//.ToUniversalTime();
                DateTime dtLast=(DateTime)datetime2;//.ToUniversalTime();
                dtDestruct=dtDestruct.AddMilliseconds(dtLast.Millisecond-dtDestruct.Millisecond);
                dtDestruct=dtDestruct.AddSeconds(dtLast.Second-dtDestruct.Second);
                dtDestruct=dtDestruct.AddMinutes(dtLast.Minute-dtDestruct.Minute);
                dtDestruct=dtDestruct.AddHours(dtLast.Hour-dtDestruct.Hour);
                long lTickDiff=dtDestruct.Ticks-dtLast.Ticks;
                //1 Tick==100ns==1/10,000,000s==1/10,000ms.
                iReturn=(int)(lTickDiff/10000000/60/60/24);//OK to truncate since should be 0 if less than 1 day difference
                //if (dtDestruct.Ticks>dtLast.Ticks) {
                //    DateTime dtTemp=dtDestruct;
                //    dtDestruct=dtLast;
                //    dtLast=dtTemp;
                //    bNeg=true;
                //}
                //while (dtDestruct.Year<dtLast.Year
                //       ||dtDestruct.Month<dtLast.Month
                //       ||dtDestruct.Day<dtLast.Day) {
                //    iReturn++;
                //    dtDestruct.AddDays(1.0d);
                //}
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"getting difference of datetime objects","DaysAbsoluteValueOfDifferenceIgnoringHours");
            }
            if (bNeg) iReturn=0-iReturn;
            return iReturn;
        }
        //public static string ToString(IPoint point) {//see RTypes
        //    return (point!=null)?("("+point.X.ToString()+","+point.Y.ToString()+")"):"null";
        //}
        //public static string ToString(FPoint point) {//see RTypes
        //    return "("+point.X.ToString()+","+point.Y.ToString()+")"; //return (point!=null)?("("+point.X.ToString()+","+point.Y.ToString()+")"):"null";
        //}
        //public static string ToString(DPoint point) {//see RTypes
        //    return "("+point.X.ToString()+","+point.Y.ToString()+")"; //return (point!=null)?("("+point.X.ToString()+","+point.Y.ToString()+")"):"null";
        //}
        public static int IndexOf(int[] arrVal, int valX) {
            if (arrVal!=null) {
                for (int iNow=0; iNow<arrVal.Length; iNow++) {
                    if (arrVal[iNow]==valX) {
                        return iNow;
                    }
                }
            }
            return -1;
        }
        public static string RemoveExpNotation(string sNum) {
            RemoveExpNotation(ref sNum);
            return sNum;
        }
        public static void RemoveExpNotation(ref string sNum) {
            bool bNeg;
            int iExpChar;
            int iDot;
            int iExpOf10;
            try {
                iExpChar=sNum.IndexOf('E');
                if (iExpChar>=0) {
                    string sExp=sNum.Substring(iExpChar+1,sNum.Length-(iExpChar+1));
                    if (sExp.StartsWith("+")) sExp=sExp.Substring(1);
                    iExpOf10=RConvert.ToInt(sExp);
                    sNum=sNum.Substring(0,iExpChar);
                    if (sNum.StartsWith("-")) {
                        sNum=sNum.Substring(1);
                        bNeg=true;
                    }
                    else bNeg=false;
                    iDot=sNum.IndexOf('.');
                    if (iDot>=0) {
                        sNum=sNum.Remove(iDot, 1);
                        //iInitialExp=sNum.Length-iDot;
                        iExpOf10-=(sNum.Length-iDot);
                    }
                    if (iExpOf10>0) {
                        int iInsertion=iExpOf10+sNum.Length;
                        for (int iZero=sNum.Length; iZero<iInsertion; iZero++) {
                            sNum+="0";
                        }
                    }
                    else if (iExpOf10<0) {
                        int iInsertion=iExpOf10+sNum.Length;
                        if (iInsertion>=0) { //could also be > but doesn't matter,
                            //except that this case handles it without doing the
                            //useless check that doesn't use the "for" loop in the
                            //== case.
                            sNum=sNum.Insert(iInsertion,".");
                        }
                        else {
                            for (int iZero=0; iZero>iInsertion; iZero--) {
                                sNum="0"+sNum;
                            }
                            sNum="."+sNum;
                        }
                    }
                    //else do not insert decimal point since exponent of 10 is zero
                    if (bNeg) sNum="-"+sNum;
                } //end if there's notation to remove
                //else no change
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"RemoveExpNotation");
            }
        }//end RemoveExpNotation
//         public static int LocationToFuzzyMaximum(int iCurrentMax, int iLoc) {
//             if (iLoc<1) iLoc=2;//2 since multiplying by 1.5 later
//             iLoc++;
//             if (iCurrentMax<0) iCurrentMax=0;
//             if (iLoc>iCurrentMax) iCurrentMax=RConvert.ToInt(RMath.SafeMultiply(RConvert.ToDouble(iLoc),(double)1.5));
//             return iCurrentMax;
//         }
        public static float FractionPartOf(float val) {
            return val-Floor(val);
        }
        public static double FractionPartOf(double val) {
            return val-System.Math.Floor(val);
        }
        public static int MinVal(int[] arrVal) {
            int val=int.MaxValue;//error condition
            if (arrVal!=null) {
                for (int iNow=0; iNow<arrVal.Length; iNow++) {
                    if (arrVal[iNow]<val) val=arrVal[iNow];
                }
            }
            return val;
        }//end MinVal
        public static int MaxVal(int[] arrVal) {
            int val=int.MinValue;//error condition
            if (arrVal!=null) {
                for (int iNow=0; iNow<arrVal.Length; iNow++) {
                    if (arrVal[iNow]>val) val=arrVal[iNow];
                }
            }
            return val;
        }//end MaxVal
        public static int MinPosVal(int[] arrVal) {
            int val=int.MaxValue;//error condition
            if (arrVal!=null) {
                for (int iNow=0; iNow<arrVal.Length; iNow++) {
                    if ( arrVal[iNow]>=0 && arrVal[iNow]<val ) val=arrVal[iNow];
                }
            }
            return val;
        }//end MinPosVal
        public static int MaxPosVal(int[] arrVal) {
            int val=0;//error condition
            if (arrVal!=null) {
                for (int iNow=0; iNow<arrVal.Length; iNow++) {
                    if ( /*arrVal[iNow]>=0 && */ arrVal[iNow]>val ) val=arrVal[iNow];
                }
            }
            return val;
        }//end MaxPosVal
        ///// <summary>
        ///// Crops iToChange to iMin and iExclusiveMax, and returns how many linear whole number increments were lost.
        ///// <summary>
        ///// <returns>Loss, if cropped iToChange, else zero.</returns>
        //public static int CropAndGetLossEx(ref int iToChange, int iMin, int iExclusiveMax) {
        //    int iReturn=0;
        //    if (iToChange<iMin) {
        //        iReturn=iMin-iToChange;
        //        iToChange=iMin;
        //    }
        //    else if (iToChange>=iExclusiveMax) {
        //        iReturn=iToChange-(iExclusiveMax-1);
        //        iToChange=iExclusiveMax-1;
        //    }
        //    return iReturn;
        //}
        public static void CropEx(ref int iToChange, int iMin, int iExclusiveMax) {
            if (iToChange<iMin) iToChange=iMin;
            else if (iToChange>=iExclusiveMax) iToChange=iExclusiveMax-1;
        }
        public static void CropZone(ref int zone_Left, ref int zone_Top, ref int zone_Right, ref int zone_Bottom, int Boundary_X, int Boundary_Y, int Boundary_Width, int Boundary_Height) {
            int iTemp;
            int screen_Right=Boundary_X+Boundary_Width;
            int screen_Bottom=Boundary_Y+Boundary_Height;
            if (zone_Left<Boundary_X) {
                iTemp=Boundary_X-zone_Left;
                zone_Left+=iTemp;
                zone_Right-=iTemp;
            }
            if (zone_Top<Boundary_Y) {
                iTemp=Boundary_Y-zone_Top;
                zone_Top+=iTemp;
                zone_Bottom-=iTemp;
            }
            if (zone_Bottom>screen_Bottom) {
                zone_Bottom-=(zone_Bottom-screen_Bottom);
            }
            if (zone_Right>screen_Right) {
                zone_Right-=(zone_Right-screen_Right);
            }
            if (zone_Right<=zone_Left) zone_Right=zone_Left+1;
            if (zone_Bottom<=zone_Top) zone_Bottom=zone_Top+1;
        }
//         public static void CropRect(ref int rect_X, ref int rect_Y, ref int rect_Width, ref int rect_Height, int Boundary_X, int Boundary_Y, int Boundary_Width, int Boundary_Height) {
//             int zone_Right=rect_X+rect_Width;
//             int zone_Bottom=rect_Y+rect_Height;
//             CropZone(ref rect_X, ref rect_Y, ref zone_Right, ref zone_Bottom, Boundary_X, Boundary_Y, Boundary_Width, Boundary_Height);
//             rect_Width=(zone_Right-rect_X)-1; //-1 since from exclusive zone
//             rect_Height=(zone_Bottom-rect_Y)-1; //-1 since from exclusive zone
//             if (rect_Width<0) rect_Width=0;
//             if (rect_Height<0) rect_Height=0;
//         }
        //public static bool RectIntersect(out int Return_X, out int Return_Y, out int Return_W, out int Return_H, int to_X, 
        
        ///<summary>
        ///Crops the RectToModify to the Boundary rect INCLUSIVELY
        ///Returns false if there is nothing left of the RectToModify after cropping.
        ///</summary>
        public static bool CropRect(ref int RectToModify_X, ref int RectToModify_Y, ref int RectToModify_Width, ref int RectToModify_Height, int Boundary_X, int Boundary_Y, int Boundary_Width, int Boundary_Height) {
            bool bGood=false;
            try {
                if (RectToModify_X<Boundary_X) {
                    RectToModify_Width-=(Boundary_X-RectToModify_X);//i.e. (0 - -1 = 1 ; so subtract 1 from width) OR i.e. 1 - 0 = 1 ; so subtract 1 from width
                    RectToModify_X=Boundary_X;
                }
                if (RectToModify_Y<Boundary_Y) {
                    RectToModify_Height-=(Boundary_Y-RectToModify_Y);
                    RectToModify_Y=Boundary_Y;
                }
                
                int Boundary_Right=Boundary_X+Boundary_Width;
                int RectToModify_Right=RectToModify_X+RectToModify_Width;
                if (RectToModify_Right>Boundary_Right) {
                    RectToModify_Width-=RectToModify_Right-Boundary_Right;
                }
                int Boundary_Bottom=Boundary_Y+Boundary_Height;
                int RectToModify_Bottom=RectToModify_Y+RectToModify_Height;
                if (RectToModify_Bottom>Boundary_Bottom) {
                    RectToModify_Height-=RectToModify_Bottom-Boundary_Bottom;
                }
                bGood=true;
                if (RectToModify_Width<1) { RectToModify_Width=0; bGood=false;}
                if (RectToModify_Height<1) { RectToModify_Height=0; bGood=false;}
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"clipping rectangle","RMath CropRect(int,...)");
            }
            return bGood;
        }//end CropRect(integers)

        ///<summary>
        ///Crops the RectToModify to the Boundary rect INCLUSIVELY
        ///Returns false if there is nothing left of the RectToModify after cropping.
        ///</summary>
        public static bool CropRect(ref float RectToModify_X, ref float RectToModify_Y, ref float RectToModify_Width, ref float RectToModify_Height, float Boundary_X, float Boundary_Y, float Boundary_Width, float Boundary_Height) {
            bool bGood=false;
            try {
                if (RectToModify_X<Boundary_X) {
                    RectToModify_Width-=(Boundary_X-RectToModify_X);//i.e. (0 - -1 = 1 ; so subtract 1 from width) OR i.e. 1 - 0 = 1 ; so subtract 1 from width
                    RectToModify_X=Boundary_X;
                }
                if (RectToModify_Y<Boundary_Y) {
                    RectToModify_Height-=(Boundary_Y-RectToModify_Y);
                    RectToModify_Y=Boundary_Y;
                }
                
                float Boundary_Right=Boundary_X+Boundary_Width;
                float RectToModify_Right=RectToModify_X+RectToModify_Width;
                if (RectToModify_Right>Boundary_Right) {
                    RectToModify_Width-=RectToModify_Right-Boundary_Right;
                }
                float Boundary_Bottom=Boundary_Y+Boundary_Height;
                float RectToModify_Bottom=RectToModify_Y+RectToModify_Height;
                if (RectToModify_Bottom>Boundary_Bottom) {
                    RectToModify_Height-=RectToModify_Bottom-Boundary_Bottom;
                }
                bGood=true;
                if (RectToModify_Width<=0) { RectToModify_Width=0; bGood=false;}
                if (RectToModify_Height<=0) { RectToModify_Height=0; bGood=false;}
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"clipping rectangle","RMath CropRect(float,...)");
            }
            return bGood;
        }//end CropRect(float,...)
        
        /// <summary>
        ///Crops the ZoneToModify to the Boundary Zone INCLUSIVELY--slightly faster than CropRect because
        /// right&bottom edges don't have to be created or calculated.
        /// </summary>
        /// <param name="ZoneToModify_X"></param>
        /// <param name="ZoneToModify_Y"></param>
        /// <param name="ZoneToModify_Right"></param>
        /// <param name="ZoneToModify_Bottom"></param>
        /// <param name="Boundary_X"></param>
        /// <param name="Boundary_Y"></param>
        /// <param name="Boundary_Right"></param>
        /// <param name="Boundary_Bottom"></param>
        /// <returns>Returns false if there is nothing left of the ZoneToModify after cropping.</returns>
        public static bool CropZone(ref float ZoneToModify_X, ref float ZoneToModify_Y, ref float ZoneToModify_Right, ref float ZoneToModify_Bottom, float Boundary_X, float Boundary_Y, float Boundary_Right, float Boundary_Bottom) {
            bool bGood=false;
            //try {
            if (ZoneToModify_X<Boundary_X) {
                ZoneToModify_Right-=(Boundary_X-ZoneToModify_X);//i.e. (0 - -1 = 1 ; so subtract 1 from width) OR i.e. 1 - 0 = 1 ; so subtract 1 from width
                ZoneToModify_X=Boundary_X;
            }
            if (ZoneToModify_Y<Boundary_Y) {
                ZoneToModify_Bottom-=(Boundary_Y-ZoneToModify_Y);
                ZoneToModify_Y=Boundary_Y;
            }
            if (ZoneToModify_Right>Boundary_Right) {
                ZoneToModify_Right=Boundary_Right;//ZoneToModify_Right-=ZoneToModify_Right-Boundary_Right;
            }
            if (ZoneToModify_Bottom>Boundary_Bottom) {
                ZoneToModify_Bottom=Boundary_Bottom;//ZoneToModify_Height-=ZoneToModify_Bottom-Boundary_Bottom;
            }
            bGood=true;
            if (ZoneToModify_Right<ZoneToModify_X) { ZoneToModify_Right=ZoneToModify_X; bGood=false;}
            if (ZoneToModify_Y<ZoneToModify_Bottom) { ZoneToModify_Bottom=ZoneToModify_Y; bGood=false;}
            //}
            //catch (Exception e) {
            //    RReporting.ShowExn(e,"clipping zone","RMath CropZone(float,...)");
            //}
            return bGood;
        }//end CropZone(float,...)

        ///<summary>
        ///Crops the RectToModify to the Boundary rect INCLUSIVELY
        ///Returns false if there is nothing left of the RectToModify after cropping.
        ///</summary>
        public static bool CropRect(ref double RectToModify_X, ref double RectToModify_Y, ref double RectToModify_Width, ref double RectToModify_Height, double Boundary_X, double Boundary_Y, double Boundary_Width, double Boundary_Height) {
            bool bGood=false;
            try {
                if (RectToModify_X<Boundary_X) {
                    RectToModify_Width-=(Boundary_X-RectToModify_X);//i.e. (0 - -1 = 1 ; so subtract 1 from width) OR i.e. 1 - 0 = 1 ; so subtract 1 from width
                    RectToModify_X=Boundary_X;
                }
                if (RectToModify_Y<Boundary_Y) {
                    RectToModify_Height-=(Boundary_Y-RectToModify_Y);
                    RectToModify_Y=Boundary_Y;
                }
                
                double Boundary_Right=Boundary_X+Boundary_Width;
                double RectToModify_Right=RectToModify_X+RectToModify_Width;
                if (RectToModify_Right>Boundary_Right) {
                    RectToModify_Width-=RectToModify_Right-Boundary_Right;
                }
                double Boundary_Bottom=Boundary_Y+Boundary_Height;
                double RectToModify_Bottom=RectToModify_Y+RectToModify_Height;
                if (RectToModify_Bottom>Boundary_Bottom) {
                    RectToModify_Height-=RectToModify_Bottom-Boundary_Bottom;
                }
                bGood=true;
                if (RectToModify_Width<=0) { RectToModify_Width=0; bGood=false;}
                if (RectToModify_Height<=0) { RectToModify_Height=0; bGood=false;}
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"clipping rectangle","RMath CropRect");
            }
            return bGood;
        }//end CropRect(doubles)

        ///<summary>
        ///Crops the RectToModify to the Boundary rect
        ///Returns false if there is nothing left of the RectToModify after cropping.
        ///</summary>
        public static bool CropRect(ref decimal RectToModify_X, ref decimal RectToModify_Y, ref decimal RectToModify_Width, ref decimal RectToModify_Height, decimal Boundary_X, decimal Boundary_Y, decimal Boundary_Width, decimal Boundary_Height) {
            bool bGood=false;
            try {
                if (RectToModify_X<Boundary_X) {
                    RectToModify_Width-=(Boundary_X-RectToModify_X);//i.e. (0 - -1 = 1 ; so subtract 1 from width) OR i.e. 1 - 0 = 1 ; so subtract 1 from width
                    RectToModify_X=Boundary_X;
                }
                if (RectToModify_Y<Boundary_Y) {
                    RectToModify_Height-=(Boundary_Y-RectToModify_Y);
                    RectToModify_Y=Boundary_Y;
                }
                
                decimal Boundary_Right=Boundary_X+Boundary_Width;
                decimal RectToModify_Right=RectToModify_X+RectToModify_Width;
                if (RectToModify_Right>Boundary_Right) {
                    RectToModify_Width-=RectToModify_Right-Boundary_Right;
                }
                decimal Boundary_Bottom=Boundary_Y+Boundary_Height;
                decimal RectToModify_Bottom=RectToModify_Y+RectToModify_Height;
                if (RectToModify_Bottom>Boundary_Bottom) {
                    RectToModify_Height-=RectToModify_Bottom-Boundary_Bottom;
                }
                bGood=true;
                if (RectToModify_Width<1) { RectToModify_Width=0; bGood=false;}
                if (RectToModify_Height<1) { RectToModify_Height=0; bGood=false;}
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"clipping rectangle","RMath CropRect");
            }
            return bGood;
        }//end CropRect(decimals)
        
        
        /*

        public static void CropRect(ref int rect_X, ref int rect_Y, ref int rect_Width, ref int rect_Height, int Boundary_X, int Boundary_Y, int Boundary_Width, int Boundary_Height) {
            if (rect_Width>0&&rect_Height>0) {
                rect_Width-=CropAndGetLossEx(ref rect_X, Boundary_X, Boundary_X+Boundary_Width);
                rect_Height-=CropAndGetLossEx(ref rect_Y, Boundary_Y, Boundary_Y+Boundary_Height);
                if (rect_Width>0) {
                    if (rect_X+rect_Width>Boundary_Width) {
                        rect_Width-=((rect_X+rect_Width)-Boundary_Width);
                    }
                }
                if (rect_Height>0) {
                    if (rect_Y+rect_Height>Boundary_Height) {
                        rect_Height-=((rect_Y+rect_Height)-Boundary_Height);
                    }
                }
            }
            else {
                rect_Width=0;
                rect_Height=0;
                CropEx(ref rect_X, Boundary_X, Boundary_X+Boundary_Width);
                CropEx(ref rect_Y, Boundary_Y, Boundary_Y+Boundary_Height);
            }
        }
        */
        #endregion utilities
        
        #region UI methods
        public static void AutoSize(out int iWidthInner, out int iHeightInner, int iWidthOuter, int iHeightOuter, float fPercent) {
            iWidthInner=(int)((float)iWidthOuter*fPercent+.5f);//.5f for rounding
            iHeightInner=(int)((float)iHeightOuter*fPercent+.5f);//.5f for rounding
        }
        public static int AutoInnerPosition(int iSizeOuter, float fPercent) {
            return (int)((float)iSizeOuter*fPercent+.5f);//.5f for rounding
        }
        #endregion UI methods

    }//end RMath class
}//end namespace
