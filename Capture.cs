// created on 6/4/2005 at 4:49 AM
//TODO: make stream saving chunkable (i.e. 2GB chunks)
//	-with black at end, then truncate where needed

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExpertMultimedia {
	public class PixelType {
		public static int Raw=0;//signifies raw capture etc.
		public static int Pixel32BGRA=1;
		public static int YUV12=2;
		public static int I420=2;//same as YUV12
		public static int YC16=3;
/* 

Idea: have an array of capture threads (in case one crashes move to the next one) to call unmanaged capture code.

Comments below are from http://service.real.com/help/library/guides/RealProducer10/htmfiles/install.htm
downloaded 2005-06-04:
    * YUV12. This format is also known as I420. It is the native color format used by RealVideo codecs. Using I420 as input improves performance by removing the need to convert the color format before encoding.

    * RGB 15, 16, 24, 322.

    * BGR 15, 16, 24, 32. This is the Macintosh version of RGB.)

    * The following Windows YUV Formats: YUY2, YV12, YVU9, YVYU, CYUV, IYUV, UYNV, UYVY, V422, YUNV.

    * The following Macintosh YUV Formats: 2VUY, YUVS, YVYU, YUVU, YVU9, YUV2, V210.


*/
	}
	public class Capture {
		int iMaxByters;//length of byterarr
		int iByters;//byterarr used buffers
		int iBuffLen;
		int iPixelType;
		int iBitDepth;//i.exn. in case capture was at YUV12 then this is 12
		Byter[] byterarr;
		int iBuffBottom;//bottom index of fake queue
		int iBuffTop;//1st empty index of fake queue
		public bool IsFull() {return (iByters>=iMaxByters);} //if (iBuffTop==iBuffBottom)
		public bool IsEmpty() {return (iByters==0);}
		public bool GrabFromCard() {
			return false;
		}
		public bool ThrowFrameTo() {
			return false;
		}
		public bool ThrowAllTo() {
			return false;
		}
		public bool SetContentType(string sContentType) {
			return false;
		}
		public bool SetPixelType(int iPixelType) {
			return false;
		}
	}
}
