/*
 * Created by Jake Gustafson.
 * User: Owner
 * Date: 10/20/2006
 * Time: 5:26 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace ExpertMultimedia {
	/// <summary>
	/// Database.
	/// </summary>
	public class Database {
		private Vars[] varsarrTable;
		private int iMode=ModeVars;
		public const int ModeVars=1;//use vars table instead
		public bool ChangeMode(int iDatabase_Mode) {
			//doesn't matter until more than varsarrTable is used
			return false;
		}
		
	}//end class Gradient
}
