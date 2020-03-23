/*
 *  Created by SharpDevelop (To change this template use Tools | Options | Coding | Edit Standard Headers).
 * User: Jake Gustafson (Owner)
 * Date: 6/12/2006
 * Time: 6:21 AM
 * Uses some modified code from CodeProject.com
 * Uses info from "The Sonic Spot" : Guide : File Formats : Wave Files
 *
 * Terms:
 * Frame: a complete multichannel audio sample
 * SamplesPerSecond: is actually the Frames per second
 * BitsPerSample: refers to bits per [channel of a] Sample (not frames)
 *
 * Notes:
 * The Format Chunk: may not be the first chunk!
 * Wave is always Little endian (Wave was developed by Microsoft & IBM)
 *
 * Chunk IDs:
 * "fmt " : format
 * "fact"
 * "data" : wave data
 * "cue "
 * "plst"
 * "list"
 * "labl"
 * "ltxt"
 * "note"
 * "smpl"
 * "inst"
 * 
 * Fixing deviant files (from "The Sonic Spot" : Guide : File Formats : Wave Files):
    AvgBytesPerSec = SampleRate * BlockAlign
    BlockAlign = SignificantBitsPerSample / 8 * NumChannels
    ChunkDataSize = 4 + (NumCuePoints * 24)
 # Incorrect Block Alignment value - this can be dealt with by calculating the Block Alignment with the formula mentioned above.
 # Incorrect Average Samples Per Second value - this can be dealt with by calculating the Average Samples Per Second with the formula mentioned above.
 # Missing word alignment padding - this can be difficult to deal with, but can be done by giving the user a warning when unrecognized chunk ID's are encountered where a one byte read offset produces a recognized chunk ID. This is not a concrete solution, but will usually work even if the program doesn't have a comprehensive list of legal IDs. 
 */
//TODO: exception handling
using System;

namespace ExpertMultimedia {
	/// <summary>
	/// Wave manager for Wave files
	/// </summary>
	public class Wave { //streamable

