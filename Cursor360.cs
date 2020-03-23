/*
 *  Created by SharpDevelop (To change this template use Tools | Options | Coding | Edit Standard Headers).
 * User: Jake Gustafson (Owner)
 * Date: 11/23/2006
 * Time: 7:59 AM
 * 
 */

using System;

namespace ExpertMultimedia
{
	/// <summary>
	/// Description of Cursor360.
	/// </summary>
	public class Cursor360 {
		private float xRot;
		private float yRot;
		private float zRot;
		public float xRotation {
			get {
				return xRot;
			}
		}
		public float yRotation {
			get {
				return yRot;
			}
		}
		public float zRotation {
			get {
				return zRot;
			}
		}
		public Cursor360() {
			//TODO: finish this
		}
		public bool SetLimits(float xRotate, float yRotate, float zRotate) {
			//TODO: finish this
		}
		public bool SetFrom2d(float fRightness, float fDownness) {
			//TODO: finish this: set by ABSOLUTE client coords as if trackball were the 3d pivot.
		}
	}
}
