/*
 * Created by SharpDevelop.
 * User: Jake Gustafson, all rights reserved (Owner)
 * Date: 11/26/2005
 * Time: 7:25 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Windows.Forms;

namespace ExpertMultimedia {
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class MyCallback
	{
		public System.Windows.Forms.Form formX;
		public System.Windows.Forms.StatusBar sbX;
		public MyCallback() {
			formX=null;
			sbX=null;
		}
		public bool UpdateForm() {
			try {
				formX.Refresh();
			}
			catch (Exception exn) {
				return false;
			}
			return true;
		}
		public bool UpdateStatus(string sUpdate) {
			bool bGood=true;
			try {
				sbX.Text=sUpdate;
				bGood=UpdateStatus();
			}
			catch (Exception exn) {
				return false;
			}
			if (bGood) bGood=UpdateForm();
			return bGood;
		}
		public string GetStatus() {
			bool bGood=true;
			string sReturn="";
			try {
				sReturn=sbX.Text;
			}
			catch (Exception exn) {
				return "Can't get status.";
			}
			return sReturn;
		}

		public bool UpdateStatus() {
			try {
				sbX.Refresh();
			}
			catch (Exception exn) {
				return false;
			}
			return true;
		}
	}
}
