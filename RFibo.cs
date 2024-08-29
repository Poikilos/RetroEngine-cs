/*
 * Created by SharpDevelop.
 * User: Owner
 * Date: 10/25/2008
 * Time: 11:44 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace ExpertMultimedia {
    /// <summary>
    /// Fibonocci generator.
    /// </summary>
    public class RFibo {
        public static bool bDebug=false;
        private static byte[] byarrFibo=null;
        private static byte[][] by2dUnFibo=null;
        
        #region constructors
        public RFibo() {
        }
        static RFibo() {
            string sDebug="";
            string sParticiple="starting";
            try {
                byarrFibo=new byte[256];
                by2dUnFibo=new byte[256][];
                uint iPosition;
                ulong ulFiboPrev=0;
                ulong ulFibo=1;
                sParticiple="creating descramble subarrays";
                for (int i1=0; i1<256; i1++) {
                    by2dUnFibo[i1]=new byte[256];
                }
                for (iPosition=0; iPosition<256; iPosition++) {
                    sParticiple="setting scramble arrays position "+iPosition.ToString();
                    byarrFibo[iPosition]=(byte)( ulFibo%256 );
                    sParticiple="adding debug info for position "+iPosition.ToString();
                    sDebug+=String.Format(" [{0}]:{1} (was {2}",iPosition,byarrFibo[iPosition],ulFibo);
                    for (ushort iDecoded=0; iDecoded<256; iDecoded++) {
                        sParticiple="adding decoded info for character "+iPosition.ToString();
                        by2dUnFibo[ Scramble((byte)iDecoded,iPosition) ][ iPosition ] = (byte)iDecoded;
                    }
                    ulFiboPrev=ulFibo;
                    sParticiple="incrementing Fibo for position "+iPosition.ToString();
                    ulFibo=RMath.SafeAddWrapped(ulFibo,ulFiboPrev)%256;
                }
            }
            catch (Exception exn) {
                Console.WriteLine();
                Console.WriteLine("RFibo while");
                Console.WriteLine(sParticiple+":");//debug only
                RReporting.ShowExn(exn,sParticiple,"RFibo  static constructor");
            }
            if (bDebug) Console.WriteLine("Debug fibbo:"+sDebug);
        }//end static constructor
        #endregion constructors
        //public static byte SafeIndex(byte[][] arr2dX, int iDim1, int iDim2) {
        //    byte byReturn=0;
        //    return byReturn;
        //}
        public static byte Descramble(byte Src_Value, uint Src_OriginalIndex) {
            try {
                if (by2dUnFibo!=null) {
                    if (by2dUnFibo[Src_Value]!=null) {
                        return by2dUnFibo[Src_Value][Src_OriginalIndex%256];
                    }
                    else Console.Error.WriteLine("Descramble array was not sized correctly");
                }
                else Console.Error.WriteLine("Descramble array not present");
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,String.Format("accessing Descramble array at [{0}][{1}]",Src_Value,Src_OriginalIndex%256),"Descramble");
            }
            return 0;
        }
        public static byte Scramble(byte Src_Value, uint Src_OriginalIndex) {
            return (byte)( RMath.SafeAddWrapped(Src_Value,byarrFibo[Src_OriginalIndex%256]) % 256 );
        }
        public static byte[] Scramble(byte[] byarrData) {
            byte[] byarrReturn=null;
            try {
                if (byarrData!=null&&byarrData.Length>0) {
                    byarrReturn=new byte[byarrData.Length];
                    //int iInKey=0;
                    for (uint iNow=0; iNow<byarrData.Length; iNow++) {
                        byarrReturn[iNow]=Scramble(byarrData[iNow],iNow); //SafeAddWrapped(byarrData[iNow],byarrFibo[iInKey]);
                        //iInKey++;
                        //if (iInKey>=byarrFibo.Length) iInKey=0;
                    }
                }
                else {
                    Console.Error.WriteLine( "Scramble(byte array) error: {0} array",
                            (byarrData==null?"null":"zero-length") );
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn);
            }
            return byarrReturn;
        }//end Scramble
        public static byte[] Descramble(byte[] byarrData) {
            byte[] byarrReturn=null;
            try {
                if (byarrData!=null&&byarrData.Length>0) {
                    byarrReturn=new byte[byarrData.Length];
                    //int iInKey=0;
                    for (uint iNow=0; iNow<byarrData.Length; iNow++) {
                        byarrReturn[iNow]=Descramble(byarrData[iNow],iNow);//SafeSubtractWrapped(byarrData[iNow],byarrFibo[iInKey]);
                        //iInKey++;
                        //if (iInKey>=byarrFibo.Length) iInKey=0;
                    }
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn);
            }
            return byarrReturn;
        }//end Descramble
        public static string Scramble(string sData) {
            return ToString(Scramble(ToByteArray(sData)));
        }
        public static string Descramble(string sData) {
            return ToString(Descramble(ToByteArray(sData)));
        }
        public static byte[] ToByteArray(string sData) {
            byte[] byarrReturn=null;
            try {
                if (sData!=null&&sData.Length>0) {
                    byarrReturn=new byte[sData.Length];
                    for (int iNow=0; iNow<sData.Length; iNow++) {
                        byarrReturn[iNow]=(byte)(sData[iNow]&0xFF);
                    }
                }
                else {
                    if (bDebug) {
                        RReporting.Debug("ToByteArray warning: no data");
                    }
                }
                if (RReporting.SafeLength(sData)!=RReporting.SafeLength(byarrReturn)) {
                    Console.Error.WriteLine( "ToByteArray error: source string length was {0} but result was a {1}-length byte array",
                        RReporting.SafeLength(sData),RReporting.SafeLength(byarrReturn) );
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn);
            }
            return byarrReturn;
        }//end ToByteArray
        public static string ToString(byte[] byarrData) {
            return byarrData!=null?RConvert.SubArrayToString(byarrData,0,byarrData!=null?byarrData.Length:0):"";
        }
    }//end RFibo class
}//end namespace
