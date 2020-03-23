/*
 *  Created by SharpDevelop (To change this template use Tools | Options | Coding | Edit Standard Headers).
 * User: Jake Gustafson (Owner)
 * Date: 11/23/2006
 * Time: 8:03 AM
 * 
 */

using System;

namespace ExpertMultimedia {
	/// <summary>
	/// Description of Mass3D.
	/// </summary>
	public class Mass3D {
		//TODO: make sure all vars below get implemented
		public const int TypeUndefined = 0;
		public const int TypeCamera = 1;
		public const int TypeLight = 2;
		public const int TypeSprite = 3;
		public const int TypeBoneArray = 4;
		public const int TypeParticleArray = 5; //individual particles are in the p3darrFields
		public const int TypeBone = 6;
		public int iType;
		public const int FlagUndefined = 0;
		public const int FlagBooleanOriginal = 1; //i.e. was returned as the non-intersecting original part in the boolean operation
		public const int FlagBooleanOperand = 2; //i.e. was returned as the non-intersecting operand part in the boolean operation
		public const int FlagBooleanBoth = 3; //i.e. the boolean intersect
		public int iFlag;
		public const uint AttribSoftFollowMe = 1; //i.e. when moves, children lag (i.e. fat cells) from centerpoint instead of following precisely
		public uint dwAttrib;
		public int iParent;//parent object in scene3dX.mdGlobal (i.e. Bones is parent of SoftTissue which is parent of Skin
		public int iSelf;//if (iParent==0) then irrelevant else index in parent mdarrGlobal
		public Sprite sprite=null;
		//measurements are in metric
		public FPoint3D p3dLoc;
		public FPoint3D p3dMin;
		public FPoint3D p3dMax;
		public FPoint3D p3dRot;
		//public FPoint3D p3dRotVel;
		public FPoint3D p3dRotMin;
		public FPoint3D p3dRotMax;
		public FPoint3D p3dRotVel;//degrees per second on multiple axes
		public FPoint3D p3dSize;
		public FPoint3D panglesVel;//SINGLE-AXIS direction of velocity
		float fVel; // m/s/s
		float fVelMax; //terminal velocity precalculated from size and weight
		float fShapeRetainFactor;//inverse springyness; how much p3darrFields stays like p3darrFieldsOriginal
		float fKelvinsVel; //heat creation/absorbtion at present (changes state at slighty different temp depending on whether positive or negative, like real life)
		float fKelvinsVelLossPerSec;
		float fKelvins; //heat (i.e. can float if hot and low density etc(?research this))
		float fKelvinsLossPerSec;
		float fKelvinsMinLiquid;
		float fKelvinsMinGas;
		public const int StateUndefined=0;
		public const int StateSolid=1;
		public const int StateLiquid=2;
		public const int StateGas=3;
		public int iState;
		Mass3D[] mdarr=null;
		//contants below are used as the first index of p3d2dParticle
		public const int ParticlesRaw=0;
		public const int ParticlesBoned=1;
		public const int ParticlesWarped=2; //ok for softbody to be warped and not saved to raw, since getting each using 4d geometry
		//absolute locations, for physics reasons:
		public FPoint3D[][] p3d2dParticle; //"force fields" proper, such as matter (or magnetism etc) that act under this mass' properties.
		Mass3D[] m3arrBone=null; //i.e. bones if BoneArray, else allows multiple deformation axes
		//TODO: calculate softness
		float fDensity;
		float fViscosity; //use 1.0 as water
		float fWeight;
		float fStretchFactor; //less than 1 if can shrink, but normally 1 or higher
		float fSurfaceTension;
		
		public Mass3D() {
			//TODO: finish this
			Init(TypeUndefined);
		}
		private void Init(int Mass3D_Type) {
			iType=TypeUndefined;
		}
		//public Mass3D[] Explode(float fMinChunkMeters, float fMaxChunkMeters, Mass3D m3Partial) {
			//TODO: finish this
			//TODO: first, if m3Partial is not null, do a boolean operation
			//TODO: make so if interlocked and can't move, allow dusty cracks to appear (based on density and surface tension)
			//TODO: any dust should be collected into a dust object--free particles should float, and surface tension should be set to zero.
			//TODO: whenever surface tension is zero, particle should float according to atmosphere
		//}
		//public Mass3D[] Slice(FPoint3D fp3dPlane, FPoint3D fp3dPlaneRot) {
			//TODO: finish this
		//}
		//public Mass3D[] BooleanParts(Mass3D m3Operand) {
			//TODO: finish this
			//int[] iarrPointsInOperand
			//int[] iarrPointsOfOperandInThis
			//for (points in this)
			//   if m3Operand.HasPointsSurrounding(m3arr[iPointX])
			//for (points in operand)
			//   if this.HasPointsSurrounding(m3Operand.m3arr[iPointX])
		//}
		
		//float BottomOffset() {
		//	return (zSize*SPRITESIZE_FACTOR);
		//}
	}
}
