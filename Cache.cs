// created on 11/5/2006 at 11:32 AM
// by Jake Gustafson (Expert Multimedia)
// www.expertmultimedia.com

using System;
using System.IO;
using System.Text.RegularExpressions;
namespace ExpertMultimedia {
	public class Cache {
		public const int TypeEmpty = 0;
		public const int TypeAnim = 1;
		public const int TypeFile = 2;
		public bool bLoaded=false;
		int iType;
		public string sURL="";
		public string sLocalRelPathFile="";
		public string sProtocol="";
		/// <summary>
		/// If (iType=Cache.TypeAnim), iAnim refers to retroengine.animarr[iAnim]
		/// </summary>
		public int iAnim;
		public bool SetURL(string sRaw) {
			sURL=sRaw;
			bool bGood=LocalFromURL();
			return bGood;
		}
		public bool LocalFromURL() {
			bool bGood=true;
			//bool bSite=false;
			int iDelimiter;
			int iCrop;
			try {
				sLocalRelPathFile=sURL;
				iDelimiter=sLocalRelPathFile.IndexOf("://");
				if (iDelimiter>-1) {
					sProtocol=sLocalRelPathFile.Substring(0,iDelimiter);
					iCrop=sLocalRelPathFile.IndexOf('@')+1;
					if (iCrop<=0) iCrop=iDelimiter+3;
					sLocalRelPathFile=sLocalRelPathFile.Substring(iCrop);
					sLocalRelPathFile=sProtocol+char.ToString(System.IO.Path.DirectorySeparatorChar)+sLocalRelPathFile;
				}
				else sProtocol="";
				//iDelimiter=sLocalRelPathFile.LastIndexOf('/');
			}
			catch (Exception exn) {
				sFuncNow="Local From URL";
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
	}
	
	public class Cacher {
		Cache[] cachearr;
		/// <summary>
		/// How many indeces of cachearr are used
		/// </summary>
		int iCaches;
	}
}//end namespace
