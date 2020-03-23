///Jake Gustafson all rights reserved
/// Created 2007-08-28 using Kate
///

using System;

namespace ExpertMultimedia {
	///<summary>
	///This class describes an array of raw data.
	///</summary>
	public class DataSet { //array of these is used for listing known Riff leaves (lowest hierarchical raw datasets)
		public string sID;
		private string[] sarrType;
		private string[] sarrEasyName;
		private int iUsed=0;
		public static int SizeOfType(string sNow) {
			int iReturn=0;
			if (sNow=="byte") iReturn=1;
			else if (sNow=="ushort") iReturn=2;
			else if (sNow=="uint24") iReturn=3;
			else if (sNow=="uint") iReturn=4;
			else if (sNow=="ulong") iReturn=8;
			else if (sNow=="int24") iReturn=3;
			else if (sNow=="int") iReturn=4;
			else if (sNow=="long") iReturn=8;
			else if (sNow=="float") iReturn=4;
			else if (sNow=="double") iReturn=8;
			else if (sNow=="decimal") iReturn=16;
			else if (sNow.StartsWith("byte[")) {
				iReturn=Convert.ToInt32(Base.SafeSubstring(sNow,5,sNow.Length-(5+1)));
			}
			return iReturn;
		}
		public int ItemCount {
			get {
				return iUsed;
			}
		}
		public int ByteCount {
			get {
				int iCount=0;
				if (sarrType!=null) {
					for (int iNow=0; iNow<sarrType.Length; iNow++) {
						iCount+=SizeOfType(sarrType[iNow]);
					}
				}
				return iCount;
			}
		}
		public int MaxItems {
			get {
				return sarrType!=null?sarrType.Length:0;
			}
		}
		public DataSet(string sSetID, string[] sarrSetType, string[] sarrSetEasyName) {
			Init(sSetID,sarrSetType,sarrSetEasyName);
		}
		public bool Init(string sSetID, string[] sarrSetType, string[] sarrSetEasyName) {
			bool bGood=false;
			try {
				int iNow=0;
				sID="";
				for (iNow=0; iNow<4; iNow++) {
					sID+=char.ToString(sSetID[iNow]);
				}
				sarrType=sarrSetType;//sarrType=new string[sarrSetType.Length];
				//for (iNow=0; iNow<sarrType.Length; iNow++) {
				//	sarrType[iNow]=sarrSetType[iNow];
				//}
				sarrEasyName=sarrSetEasyName;//sarrEasyName=new string[sarrSetEasyName.Length];
				//for (iNow=0; iNow<sarrEasyName.Length; iNow++) {
				//	sarrEasyName[iNow]=sarrSetEasyName[iNow];
				//}
				iUsed=sarrType.Length;
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"DataSet Init("+sSetID+",...)","setting dataset description \""+sSetID+"\"");
			}
			return bGood;
		}
	}//end class DataSet
}//end namespace