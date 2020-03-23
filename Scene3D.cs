/*
 *  Created by SharpDevelop (To change this template use Tools | Options | Coding | Edit Standard Headers).
 * User: Jake Gustafson (Owner)
 * Date: 11/23/2006
 * Time: 8:04 AM
 * 
 */

using System;

namespace ExpertMultimedia {
	/// <summary>
	/// Description of Scene3D.
	/// </summary>
	public class Scene3D {
		private Mass3D mdGlobal; //TODO: should the first be atmosphere?
		//TODO: atmosphere should be a Mass3D so as to allow eddeys of fog, leaves, etc (!)
		//TODO: study http://en.wikipedia.org/wiki/Fire (for different phenomena called fire, such as static/coronal stuff
		//TODO: ALL particles in scene MUST act as a contiguous network for true physics.
		public GBuffer32BGRA gbEnvironmentMap;//GLOBAL ENVIRONMENT MAP
		public Scene3D() {
			mdGlobal=null;
			gbEnvironmentMap=null;
		}
	}
}
