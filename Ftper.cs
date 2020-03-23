/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 3:13 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
//using System.Windows.Forms; //debug NYI remove MessageBox and this line

namespace ExpertMultimedia
{
	/// <summary>
	/// reusable ftp class.
	/// </summary>
	public class Ftper {
		//public TextWriter twOut;
		public string sUser;
		public string sPassword;
		private string sAddress;//intentionally private
		/// <summary>
		/// The subdirectory on the ftp server.
		/// </summary>
		private string sSubdirRemote;
		private bool bTransferring=false;
		private string sWorkingDirNoSlash; //chdir here before using sPathFile if relative
		private string sSubdirLocal;
		private string sFile;
		public bool bAutoLogout=false;
		private string sPathFile {
			get {
				return sSubdirLocal+char.ToString(System.IO.Path.DirectorySeparatorChar)+sFile;
			}
		}
		private string sLocalSubfolder {
			get {
				return sSubdirLocal;
			}
			set {
				sSubdirLocal=value;
				while ((sSubdirLocal.Length>1) && (sSubdirLocal.EndsWith(char.ToString(System.IO.Path.DirectorySeparatorChar))==true)) sSubdirLocal=sSubdirLocal.Substring(0,sSubdirLocal.Length-1);
			}
		}
		public bool IsBusy {
			get {
				return bTransferring;
			}
		}
		public string GetAddress() {
			return sAddress;
		}
		/// <summary>
		/// The ftp address.  Full site address ONLY with html subdir
		/// i.exn. ftp://user1:pass@ftp.x.com/www
		/// </summary>
		public string sFtpDirHTML;
		/// <summary>
		/// The ftp address and cgi-bin.  Full site address with ONLY subdir being cgi-bin, 
		/// i.exn. ftp://user1:pass@ftp.x.com/cgi-bin OR ftp://user1:pass@ftp.x.com/docs/cgi-bin
		/// </summary>
		public string sFtpDirCGI;

		public string sRemoteFolder {
			get {
				return sSubdirRemote;
			}
			set {
				sSubdirRemote=value;
				while ((sSubdirRemote.Length>1) && sSubdirRemote.StartsWith("/")) sSubdirRemote=sSubdirRemote.Substring(1);
				while ((sSubdirRemote.Length>1) && sSubdirRemote.EndsWith("/")) sSubdirRemote=sSubdirRemote.Substring(0,sSubdirRemote.Length-1);
			}
		}
		public Ftper() {
			sFuncNow="initialization";
		}
		private bool SendOne(string sRelPathFile, string sSiteX) {
		//INTENTIONALLY PRIVATE
			//debug NYI call as thread in case locks up, and
			// then set bBusy=false after terminating.
			if (IsBusy) {
				sLastErr="Busy.";
				return false;
			}
			bTransferring=true;
			bool bGood=false;
			int iLoc;
			string sPathFileFtp;
			try {
				iLoc=sRelPathFile.IndexOf('\\');
				while (sRelPathFile.Length>1 && iLoc>-1) {
					sRelPathFile.Replace('\\','/');
					iLoc=sRelPathFile.IndexOf('\\');
				}
				sFuncNow="SendOne("+sRelPathFile+","+sSiteX+")";
				Byter byterX=new Byter(1000000);//debug overflow
				bGood=byterX.Load(sRelPathFile);
				if (bGood) {
					sPathFileFtp="ftp://"+sUser+":"+sPassword+"@"+sSiteX+"/"+sRelPathFile;
					bGood=byterX.Save(sPathFileFtp);
					if (!bGood) sLastErr="Failed to save remote file.";
				}
				else sLastErr="Failed to load "+sRelPathFile+".";
			}
			catch (Exception exn) {
				bGood=false;
				sLastErr="Exception error--"+exn.ToString();
			}
			bTransferring=false;
			return bGood;			
		}
		public bool FileExistsLocal() {
			sFuncNow="FileExistsLocal";
			string sTempDir=Environment.CurrentDirectory;
			Environment.CurrentDirectory=sWorkingDirNoSlash;
			bool bExists=File.Exists(sPathFile);
			Environment.CurrentDirectory=sTempDir;
			return bExists;
		}
		public void Send() {
			bTransferring=true;
			sFuncNow="Send \""+sPathFile+"\"";
			//must be connected
			//starts thread to send sPathFile to ftp folder
			if (bAutoLogout==true) Disconnect();
			bTransferring=false;
			bAutoLogout=false;
		}
		public void Connect() {
			sFuncNow="Connect";
			//debug NYI just make a batch file and run it instead.
		}
		public void Disconnect() {
			sFuncNow="Disconnect";
		}
		public bool BatchSendOne(string sFileNow) {
			string sDir1=Environment.CurrentDirectory;
			sFuncNow="SendOne("+sFileNow+")";
			bool bGood=true;
			sFile=sFileNow;
			if (IsBusy) {
				bGood=false;
				sLastErr="Busy";
			}
			else if (FileExistsLocal()) {
				bTransferring=true; //the thread changes it back when done
				bAutoLogout=true;
				
				
				//Connect();
				//Send(); //start this as a thread
				
				
				//the easy way:
				StreamWriter sw;
				StreamWriter swFTP;
				string sBatch="transfer.bat";
				string sFTPScript="transfer.ftp";
				
				try {
					sFuncNow="SendOne("+sFileNow+") [before opening new batch file]";
					//try {
						swFTP=new StreamWriter(sFTPScript);
					//}
					//catch (Exception exn) {
					//	sLastErr="The program is already trying to transfer the file.";
					//}
					sFuncNow="SendOne("+sFileNow+") [after opening new batch file]";
					
					Environment.CurrentDirectory=sWorkingDirNoSlash;
						swFTP.WriteLine(sUser);
						swFTP.WriteLine(sPassword); //debug security since saved to plain text
						if (sSubdirRemote.Length>0) swFTP.WriteLine("cd "+sSubdirRemote);
						swFTP.WriteLine("send "+sFile);
						swFTP.WriteLine("disconnect");
						swFTP.WriteLine("bye");
						swFTP.Close();
						
						sw=new StreamWriter(sBatch);
						
						sw.WriteLine("cd\\"+sWorkingDirNoSlash);
						sw.WriteLine("del FTPErrorNote.txt");
						if (sSubdirLocal.Length>0) {
							sw.WriteLine("move transfer.ftp "+sSubdirLocal);
							sw.WriteLine("cd "+sSubdirLocal);
						}
						sw.WriteLine("ftp -s:"+sFTPScript+" "+sAddress + " > ftplog.txt"); // > FTPLog.nosearch");
						//sw.WriteLine("ftp -s:"+sFTPScript+" ftp://"+sUser+":"+sPassword+"@"+sAddress); // > FTPLog.nosearch");
						
						//sw.WriteLine("del "+sFTPScript);
						sw.WriteLine("cd\\"+sWorkingDirNoSlash);
						//sw.WriteLine("del "+sBatch);
						//sw.WriteLine("exit");
						sw.Close();
						sFuncNow="SendOne("+sFileNow+") [after closing new batch file]";
						//System.Diagnostics.Process.Start(@"C:\listfiles.bat");
						System.Diagnostics.Process.Start(sBatch);
						
						//System.Diagnostics.ProcessStartInfo psi =
						//	new System.Diagnostics.ProcessStartInfo(@"C:\transfer.bat");
						//psi.RedirectStandardOutput = true;
						//psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
						//psi.UseShellExecute = false;
						//System.Diagnostics.Process procNow;
						//procNow = System.Diagnostics.Process.Start(psi);
						//System.IO.StreamReader srOut = procNow.StandardOutput;
						//procNow.WaitForExit(18000);
						//if (procNow.HasExited) {
						//	string sOut = srOut.ReadToEnd();
						//	MessageBox.Show(sOut);
						//}
						
						
					//}
					
					
				}
				catch (Exception exn) {
					if (sFuncNow.EndsWith("before opening new batch file]"))
					 sLastErr="The transfer script appears to already be running--"+exn.ToString();
					else sLastErr=("There was a problem transferring the file.  Try again and make sure you are connected.  If this continues, contact "+RetroEngine.sContactWhom);
				}
			}
			else bGood=false;
			Environment.CurrentDirectory=sDir1;
			bTransferring=false;//debug NYI remove this line if using a separate thread (put it in the thread).
			return bGood;
		}
	}
}
