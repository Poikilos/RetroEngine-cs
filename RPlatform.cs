//Created 2006-09-26 11:47PM
//Credits:
//-Jake Gustafson www.expertmultimedia.com
//Requirements:
//-Vars.cs (which does have cross-dependency)
using System;
// using System.Collections;
using System.Collections.Generic;  // List
using System.IO;  // Path
// using System.Text.RegularExpressions;

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
		private static readonly char[] carrInvalidPathChars_Windows=new char[] {'\\','/',':','*','?','"','<','>','|'};
		//public static RCallback rcbBlank=null;//formerly mcbNULL
		static List<string> paths = null;
		static List<string> dotExtensions = null;
		public static List<string> Paths {
			get {
				return paths;
			}
		}
		#endregion variables
		
		#region platform
		/*
		// backward-compatible checks:
		
		public static bool IsWindows() {//formerly PlatformIsWindows
			return Environment.OSVersion.ToString().IndexOf("Win")>=0;
			// ^ for example, "Microsoft Windows NT 6.1.7601.0" for Windows 7,
			//   6.2.* for 8, 6.3.* for 8.1, 10 for 10.
		}
		public static bool IsLinux() {
	        int p = (int) Environment.OSVersion.Platform;
	        return (p == 4) || (p == 6) || (p == 128);
		}
		*/
	    private static bool IsWindows()
	    {
	        return Environment.OSVersion.Platform == PlatformID.Win32NT;
	    }
	
	    private static bool IsLinux()
	    {
	        return Environment.OSVersion.Platform == PlatformID.Unix;
	    }
	
	    private static bool IsMacOS()
	    {
	        return Environment.OSVersion.Platform == PlatformID.MacOSX;
	    }		
		public static bool IsValidPathChar_AnyPlatform(char val) {
			bool bReturn=true;
			for (int i=0; i<carrInvalidPathChars_Windows.Length; i++) {
				if (val==carrInvalidPathChars_Windows[i]) {
					bReturn=false;
					break;
				}
			}
			//TODO: add invalid characters for other platforms (??) if needed 
			return bReturn;
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
				else Console.Error.WriteLine("Warning: blocked a system command: "+sLine);
			}
			else RReporting.Warning("Sent a blank string to RPlatform Run(string sSystemCommand)");
			return bGood;
		}//end Run
		
		public static string Which(string filename) {
	        string path = Environment.GetEnvironmentVariable("PATH");
	        if (paths == null) {
	            paths = new List<string>(path.Split(Path.PathSeparator));
	            if (IsWindows()) {
	                paths.Add(@"C:\PortableApps\winbuilds\bin");
	                paths.Add(@"E:\progs\video\gui4ffmpeg");
	            }
	        }
	
	        if (dotExtensions == null) {
	            dotExtensions = new List<string> { ".exe", ".ps1", ".bat" };
	            if (IsWindows()) {
	                dotExtensions.Add(".exe");
	                dotExtensions.Add(".ps1");
	                dotExtensions.Add(".bat");
	            }
	            else if (IsLinux()) {
	                dotExtensions.Add(".sh");
	            }
	            else if (IsMacOS()) {
	                dotExtensions.Add(".command");
	            }
	        }
	
	        List<string> names = new List<string> { filename };
	        if (Path.GetExtension(filename) == "") {
	            // Code to handle when the file has no extension.
	            foreach (string dotExtension in dotExtensions)
	            	names.Add(filename + dotExtension);
	        }
	
	        for (int i = paths.Count - 1; i >= 0; i--) {
	            // Reverse since paths added by programs
	            // are likely where programs are that we need to run in a new project.
	            foreach (string name in names) {
	                string fullpath = Path.Combine(paths[i], name);
	                if (File.Exists(fullpath)) {
	                    return fullpath;
	                }
	            }
	        }
	        // filename does not exist in path
	        return null;
	    }
		#endregion platform
		
	}//end RPlatform partial class
}//end namespace

