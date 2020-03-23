/*
 *  Created by SharpDevelop (To change this template use Tools | Options | Coding | Edit Standard Headers).
 * User: Jake Gustafson (Owner)
 * Date: 11/23/2006
 * Time: 8:11 AM
 * 
 */

using System;

namespace ExpertMultimedia
{
	/// <summary>
	/// Description of Sprite.
	/// </summary>
	public class Sprite {
		//sequence constants are used as the first index of i2dSeq
		public const int SeqIdle1=0;
		public const int SeqIdle2=1;
		public const int SeqIdle3=2;
		public const int SeqWalk1=3;
		public const int SeqWalk2=4;
		public const int SeqWalk3=5;
		public Anim anim=null;
		//public IRECT rectRender;
		public float fPixPerMeter;//Pixels per meter in graphic FOR 3D RENDERING
		public float xCenter;
		public float yCenter;
		private int[][] i2dSeq; //values are frames of anim
		public Sprite() {
			//TODO: finish this
		}
	}
}
