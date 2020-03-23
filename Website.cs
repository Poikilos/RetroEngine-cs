/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 3:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
//using ExpertMultimedia;
using System.Windows.Forms;
using System.IO;

namespace ExpertMultimedia
{
	public class SiteTemplateItem {
		string sUsed;
		string sUnused;
		bool bUsed;
		bool bRequired; //means always looks like the sUsed template
		string sName; //starts with '<' if a tag type
		string sFileUnusedTemplate; //0-Length means unused.
		string sFileUsedTemplate; //0-Length means unused.
		SiteTemplateItem() {
			sName="class=\"headergraphic\"";
			bUsed=false;
			bRequired=false;
			sFileUnusedTemplate="";
			sFileUsedTemplate="";
			//see if the file exists instead//sFilesNonstandard="<sFilesNonstandard youth.html=\"webezpost/template-youth.html\" youngadults.html=\"webezpost/template-youngadults.html\">"
		}
		public string Get() {
			string sReturn;
			if (bUsed||bRequired) {
				sReturn=sUsed; //TODO: call sFileTemplateUsed
			}
			else {
				sReturn=sUnused; //TODO: call sFileTemplateUnused
			}
			return sReturn;
		}
	}
	/// <summary>
	/// The Website object does web page editing through HTMLPage objects.
	/// </summary>
	public class Website {
		/*
		private Ftper ftper;
		public string sFTPSite {
			set {
				try{
					ftper.SetAddress(value);
				}
				catch(Exception exn){
					sFuncNow="\"set sFTPSite\"";
					sLastErr="Exception error.";
				}
			}
			get {
				try{return ftper.GetAddress();}
				catch (Exception exn) {
					sFuncNow="\"get sFTPSite address\"";
					sLastErr="Exception error";
				}
				return "";
			}
		}
		*/
		public string sPageSubdir; //subfolder is in htmlarr[].sSubDir
		//private string sMyDir;
		public GNoder[] pagearr;
		private int iPageArray;//array size
		private int iPages;//pages allocated
		public int iPageNow;
		string sFirstSuffix="(first)"; //filename suffix.  Used for the backup file on first confirmed download (confirm by checking for </html>)
		public Website() {
			Init(100);
		}
		public Website(int iPagesMax) {
			Init(iPagesMax);
		}
		public void Init(int iPagesMax) {
			sFuncNow="Init()";
			try {
				pagearr=new GNoder[iPages];
				for (iPageNow=0; iPageNow<iPages; iPageNow++)
					pagearr[iPageNow]=new GNoder();
				iPageNow=-1;//deselects page since none are loaded
				//ftper=new Ftper();
				//sMyDir=RetroEngine.sDataFolderSlash;
			}
			catch (Exception exn) {
				sLastErr="Exception error while loading Website editor--"+exn.ToString();
			}
		}
		/*
		public bool IsBusy {
			get {
				try {
					return ftper.IsBusy;
				}
				catch (Exception exn) {
					return false;
				}
			}
		}
		*/
		public void CommandChoosePage() {
			//debug NYI OpenFileDlg, set iPageNow=x
		}
		//Command functions are called by user interfaces.
		public void CommandDownload() {
			//debug NYI
		}
		public void CommandLoadCurrent() {
			try {
				pagearr[iPageNow].LoadFile();
				//NYI? now MainForm can grab pagearr[iPageNow].sContent
			}
			catch (Exception exn) {
				sFuncNow="CommandLoadCurrent()";
				sLastErr="Exception error--"+exn.ToString();
			}
		}
		public bool CommandSave() {
			bool bGood=true;
			try {
				//prepares web page file for ftp before sending
				//NYI? MainForm must update pagearr[iPageNow].sData FIRST
				pagearr[iPageNow].SaveFile();
			}
			catch (Exception exn) {
				bGood=false;
				sLastErr="Exception error running CommandSave--"+exn.ToString();
			}
			return bGood;
		}
		public bool CommandPublish() {
			bool bGood=CommandSave();
			if (CommandUpload()==false) bGood=false;
			return bGood;
		}
		public bool CommandUpload() {
			sFuncNow="CommandUpload";
			bool bGood=true;
			try {
				//ftper.sAddress=sFTPSite;
				//ftper.sRemoteFolder=sFTPFolder;
				//ftper.sLocalSubfolder=pagearr[iPageNow].sPath;
				//ftper.SendOne(pagearr[iPageNow].sFile); //NO path included
				try{
					statusq.Enq("Sending file...");
				}
				catch (Exception exn) {
					sLastErr="Exception error--"+exn.ToString();
				}
				//ftper.SendOne(sPageSubdir, sFTPSite);
				//TODO: send the file
				//bGood=ftper.SendFile(s);
				if (bGood) {
					try {
						statusq.Enq("Sending file...done.");
					}
					catch (Exception exn) {
						sLastErr="Exception error accessing statusq--"+exn.ToString();
					}
				}
				else {
					try {
						statusq.Enq("Sending file...failed.");
					}
					catch (Exception exn) {
						sLastErr="Exception error accessing statusq--"+exn.ToString();
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error accessing file transfer protocol--"+exn.ToString();
			}
			return bGood;
		}
		//public void CommandSaveCopy() {
		//	document.SaveCopy(sFileCopy);//debug NYI make sure sFileCopy has the correct value
		//	Environment.CurrentDirectory=sProgDir;
		//}
		//public void CommandLoadCopy() {
		//	document.LoadCopy(sFileCopy);
		//	Environment.CurrentDirectory=sProgDir;
		//}
		/*
		public void CommandResetToFirst() {
			//document.sFile=document.sFile+sFirstSuffix;
			string sTemp=document.sPathFile;
			int iNameEnd=sTemp.LastIndexOf(".");
			if (iNameEnd<=0) iNameEnd=sTemp.Length-1;
			if (iNameEnd<0) iNameEnd=0;
			sTemp=sTemp.Insert(iNameEnd, sFirstSuffix);
			//document.LoadFile();
			document.LoadCopy(sTemp);
			//MessageBox.Show("Loaded backup from "+sTemp);
			//document.sFile=document.sFile.Substring(0,document.sFile.Length-sFirstSuffix.Length);
		}
		*/
	}
}
