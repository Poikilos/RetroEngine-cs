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
    /// Class to be used as thread-safe reporting delegate.
    /// </summary>
    public class RCallback {
        public System.Windows.Forms.Form formX=null;
        public System.Windows.Forms.TextBox sbX=null;
        public bool bFreeze=false;//whether to keep message on top
        public RCallback() {
            //formX=null;
            //sbX=null;
            //bFreeze=false
        }
        public string sStatus {
            set {
                UpdateStatus(value);
            }
        }
        public bool UpdateForm() {
            try {
                if (formX!=null) formX.Refresh();
            }
            catch (Exception exn) {
                return false;
            }
            return true;
        }
        public bool UpdateStatus(string sUpdate) {
            bool bGood=true;
            try {
                if (!bFreeze) {
                    if (sbX!=null) sbX.Text=sUpdate;
                    bGood=UpdateStatus();
                }
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
                if (sbX!=null) sReturn=sbX.Text;
            }
            catch (Exception exn) {
                return "Can't get status.";
            }
            return sReturn;
        }

        public bool UpdateStatus() {
            try {
                if (formX!=null) formX.Refresh();
            }
            catch (Exception exn) {
                return false;
            }
            return true;
        }
    }
}
