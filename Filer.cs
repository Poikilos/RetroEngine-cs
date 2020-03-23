/*
 *  Created by SharpDevelop (To change this template use Tools | Options | Coding | Edit Standard Headers).
 * User: Jake Gustafson (Owner)
 * Date: 6/14/2006
 * Time: 6:55 AM
 * 
 */

using System;
using System.IO;

namespace lockdotnet {
	/// <summary>
	/// Description of Filer.
	/// </summary>
	public class Filer {
		FileStream fsNow;
		string sFileNow;
		public Filer() {
			fsNow=null;
		}
		public Filer(string sFile) {
			Open(sFile);
		}
		public bool OpenForWrite(string sFile) {
			try {
				fsNow=new FileStream(sFile, 
					FileMode.Create, FileAccess.Write);
			}
			catch (Exception exn) {
				Msgr.sStatus="Program failed during OpenForWrite: "+exn.ToString();
				bGood=false;
			}
		}
		public bool Write(byte[] byarrData) {
			bool bTest=false;
			try {
				bTest=Write(byarrData, 0, byarrData.Length);
			}
			catch (Exception exn) {
				Msgr.sStatus="Program failed while writing byte array to file: "+exn.ToString();
			}
			return bTest;
		}
		public byte[] ConvertToBytes(long lValue) {
			byte[] byarrData=null;
			try {
				byarrData=new byte[8];
				ConvertToBytes(ref byarrData, 0, lValue);
			}
			catch {
			
			}
			return byarrData;
		}
		/// <summary>
		/// This function forces little endian write to an array.  In order to avoid endian issues, no bit shifts are used.
		/// </summary>
		/// <param name="byarrData"></param>
		/// <param name="iStart">place in byarrData to write the value</param>
		/// <param name="lValue">the value to write to the byte array</param>
		/// <returns></returns>
		public bool ConvertToBytes(ref byte[] byarrData, int iStart, long lValue) {
			bool bGood=false;
			//to change variable type, just change:
			// -iSizeOfValue
			// -first lDivisor
			int iSizeOfValue=8;
			try {
				if (lValue!=0) {
					bool bNeg=(lValue<0);
					if (bNeg) lValue*=-1;
					bGood=true;
					int iNow=iStart+(iSizeOfValue-1);
					long lChomp;
					long lDivisor=(0xFFFFFFFFFF+1);
					//TODO: Finish this and allow multiple files in lockdotnet
					for (int iCount=0; iCount<iSizeOfValue; iCount++) {
						lChomp=lValue/lDivisor;
						byarrData[iNow]=(byte)lChomp;
						if (bNeg) byarrData[iNow] ^= 0xFF;
						lValue-=lChomp*lDivisor;
						iNow--;
						lDivisor/=256;
					}
					//note: 0xFFFF+1 (65536) is the lDivisor for 24 bits
					//(0xFFFFFF which is 16777215)
					//-the resulting byte is saved
					//-the integer is destructively reduced by the byte*lDivisor
					//-
				}
				//else assumes array was zero-initialized
			}
			catch {
			
			}
			return bGood;
		}
		public bool Write(long lValue) {
			bool bTest=false;
			try {
				byte[] byarrData=ConvertToBytes(lValue);
				bTest=Write(byarrData);
			}
			catch (Exception exn) {
				Msgr.sStatus="Program failed while writing byte array to file: "+exn.ToString();
			}
			return bTest;
		}
		public bool Write(byte[] byarrData, int iStart, int iLength) {
			try {
				fsNow.Write(byarrData, iStart, iLength);
			}
			catch (Exception exn) {
				Msgr.sStatus="Program failed while writing to file: "+exn.ToString();
			}
		}
		public bool Close() {
			try {
				fsNow.Close();
			}
			catch (Exception exn) {
				Msgr.sStatus="Program failed while Closing file: "+exn.ToString();
			}
		}
	}
}
