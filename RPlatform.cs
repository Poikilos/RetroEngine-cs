//Created 2006-09-26 11:47PM
//Credits:
//-Jake Gustafson www.expertmultimedia.com
//Requirements:
//-Vars.cs (which does have cross-dependency)
using System;
// using System.IO;
// using System.Text.RegularExpressions;
// using System.Collections;
// using System.Drawing;//ONLY for imageformat
// using System.Drawing.Imaging;//ONLY for imageformat
// using System.Windows.Forms;//ONLY for PictureBox
// using System.Net;
/*
 * TODO: Math stuff to use for fun patterns:
 * -Lissajous curve
 * -Brownian motion
 * -Brownian motion is random(?) (NOT psuedorandom) but accurately simulates particles in a fluid
 * -Fourier series
 *		-Converts any curve to a series of sine functions
 * 		-in other words, approximating a stepped function makes it analog-like
 * 			-called the Gibbs phenomenon
 */
namespace ExpertMultimedia {	
	public partial class RPlatform {//formerly Base
		//TODO:  add the string RPlatform.sThrowTag to somewhere in ALL THROW STATEMENTS.
		#region variables
		public static int iMaxAllocation=268435456; //debug wherever this is not used
			//1MB = 1048576 bytes
		//public static RCallback rcbBlank=null;//formerly mcbNULL
		#endregion variables
		
		#region platform
		public static bool IsWindows() {//formerly PlatformIsWindows
			return Environment.OSVersion.ToString().IndexOf("Win")>=0;
		}
		public static bool Terminate(string sProcessName) {
			if (IsWindows()) {
				RReporting.ShowErr("Terminating processes in Windows is not yet implemented");
			}
			else {
				return Run("killall "+sProcessName);
			}
			return false;
		}
		public static readonly string[] sarrBlockCommand=new string[]{"rm","mv","cp",};
		public static bool Run(string sLine) {
			bool bGood=false;
			string sCommand="";
			string sArgs="";
			int iSpace=-2;
			if (sLine!=null&&sLine.Length>0) {
				bool bBlock=false;
				for (int iNow=0; iNow<sarrBlockCommand.Length; iNow++) {
					if ( sLine.StartsWith(sarrBlockCommand[iNow]+" ") || sLine.Contains(" "+sarrBlockCommand[iNow]+" ") ) //||sLine.Contains("|rm ")||sLine.Contains("&rm ")  )
						bBlock=true;
				}
				if (sLine[0]!='/' &&!sLine.Contains("|") &&!sLine.Contains(">") &&!sLine.Contains("<") &&!sLine.Contains("&") &&!sLine.Contains("xargs") &&!bBlock) {
					try {
						System.Diagnostics.Process proc = new System.Diagnostics.Process();
						proc.EnableRaisingEvents=false;
						iSpace=sLine.IndexOf(" ");
						if (iSpace>-1) {
							sCommand=sLine.Substring(0,iSpace);
							sArgs=sLine.Substring(iSpace+1);
							proc.StartInfo.FileName=sCommand;
							proc.StartInfo.Arguments=sArgs;
						}
						else {
							proc.StartInfo.FileName=sLine;
						}
						proc.Start();
						//proc.WaitForExit();
						bGood=true;
					}
					catch (Exception exn) {
						bGood=false;
						RReporting.ShowExn(exn,"running system command",
							String.Format("Run{sLine:{0}; sCommand:{1}; sArgs:{2}}",
								RReporting.StringMessage(sLine,true),RReporting.StringMessage(sCommand,true),RReporting.StringMessage(sArgs,true)
							)
						);
					}
				}
				else Console.WriteLine("Warning: blocked a system command: "+sLine);
			}
			else RReporting.Warning("Sent a blank string to RPlatform Run(string sSystemCommand)");
			return bGood;
		}//end Run
		#endregion platform
		
	}//end RPlatform partial class
}//end namespace

