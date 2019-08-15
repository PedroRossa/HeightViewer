#define IN_UNITY

using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Collections.Generic;

#if IN_UNITY
using UnityEngine;
#endif

namespace Atlas.IO
{
	/// <summary>
	/// Contains the .dxm header info's.
	/// </summary>
	struct DXMHeader
	{
		/// <summary>
		/// Extension Type.
		/// </summary>
		public byte nameCharacter0;
		/// <summary>
		/// Extension Type.
		/// </summary>
		public byte nameCharacter1;
		/// <summary>
		/// Extension Type.
		/// </summary>
		public byte nameCharacter2;
		/// <summary>
		/// Extension Type.
		/// </summary>
		public byte nameCharacter3;
		/// <summary>
		/// Major version of the file.
		/// </summary>
		public byte majorVersion;
		/// <summary>
		/// Minor version of the file.
		/// </summary>
		public byte minorVersion;
		/// <summary>
		/// The type of enconding used on the file:
		/// <para>Interleaved: vertex, normal, ...</para>
		/// <para>De Interleaved: all vertex, all normals, all ... (Only supported type in this loader)</para>
		/// <para>Byte Interleaved: encoding that combine all major bytes followed by the combinantion of lesser bytes.</para>
		/// </summary>
		public byte encoding;
		/// <summary>
		/// The type of compression used on the file:
		/// <para>LZ77.</para>
		/// <para>No Compression.</para>
		/// </summary>
		public byte compression;
		/// <summary>
		/// The number of vertices on the file.
		/// </summary>
		public ulong vertexCount;
		/// <summary>
		/// Bit mask with flags that indicate how the vertex is composed. For more information refer to the wiki.
		/// </summary>
		public uint vertexCompositionFlags;
		/// <summary>
		/// The amount of groups on the file.
		/// </summary>
		public ushort groupCount;
		/// <summary>
		/// Index type:
		/// <para>No Index, Triangles, Quads, Triangle Strip, Lines or Line Strip.</para>
		/// </summary>
		public byte indexFormat;
		/// <summary>
		/// The number of indexes on the files.
		/// </summary>
		public byte indexByteCount;
		/// <summary>
		/// The memory address of the vertex table.
		/// </summary>
		public ulong vertexTableAddr;
		/// <summary>
		/// The memory address of the index table.
		/// </summary>
		public ulong indexTableAddr;
	};

	/// <summary>
	/// The type of enconding.
	/// </summary>
	enum DXMEncoding
	{
		Inteleaved = 0,
		DeInterleaved,
		BytePack
	};

	/// <summary>
	/// The compression type.
	/// </summary>
	enum DXMCompression
	{
		NoCompression = 0,
		LZ77
	};

	/// <summary>
	/// The vertex composition flag.
	/// </summary>
	enum DXMVertexFlag
	{
		Vertex_3_F32 = 1 << 0,
		Normal_3_F32 = 1 << 1,
		Texcoord_2_F32 = 1 << 2,
		Color_4_U8 = 1 << 3
	};

	/// <summary>
	/// The type of index.
	/// </summary>
	enum DXMIndexType
	{
		NoIndex = 0,//point cloud
		Triangles,
		Quads,
		TriangleStrip,
		Lines,
		LineStrip
	};

	/// <summary>
	/// The compressed and uncompressed size.
	/// </summary>
	struct DXMData
	{
		public ulong compressedSize;
		public ulong uncompressedSize;
	};

	/// <summary>
	/// The group variables.
	/// </summary>
	struct DXMGroup
	{
		public ulong offset;
		public ulong length;
		public string texture;
		public short[] index16;
		public int[] index32;
	};

	/// <summary>
	/// The complete model variables.
	/// </summary>
	struct DXMModel
	{
		public DXMHeader header;
		public DXMGroup[] groups;   //group offset and length used with index
		public float[] vertex;      //vertex array 3f
		public float[] normal;      //normal array 3f
		public float[] uv;          //uv array 2f
		public byte[] color;        //vertex color 3b

#if IN_UNITY
		public Vector3[] vertexUnity;
		public Vector3[] normalUnity;
		public Vector2[] uvUnity;
		public Color32[] colorUnity;
#endif
	}

	/// <summary>
	/// Class which imports .dxm files.
	/// </summary>
	static class DXMImporter
	{
		private static void ReadHeader(ref BinaryReader rb, out DXMHeader header)
		{
			header.nameCharacter0 = rb.ReadByte();
			header.nameCharacter1 = rb.ReadByte();
			header.nameCharacter2 = rb.ReadByte();
			header.nameCharacter3 = rb.ReadByte();
			header.majorVersion = rb.ReadByte();
			header.minorVersion = rb.ReadByte();
			header.encoding = rb.ReadByte();
			header.compression = rb.ReadByte();

			header.vertexCount = rb.ReadUInt64();
			header.vertexCompositionFlags = rb.ReadUInt32();

			header.groupCount = rb.ReadUInt16();
			header.indexFormat = rb.ReadByte();
			header.indexByteCount = rb.ReadByte();

			header.vertexTableAddr = rb.ReadUInt64();
			header.indexTableAddr = rb.ReadUInt64();
		}
		private static void ByteToFloat(ref float[] dst, Byte[] src)
		{
			int count = src.Length / sizeof(float);

			dst = new float[count];

			for (int i = 0; i < count; ++i)
			{
				dst[i] = System.BitConverter.ToSingle(src, i * sizeof(float));
			}
		}
		private static void ByteToInt(ref int[] dst, Byte[] src)
		{
			int count = src.Length / sizeof(int);

			dst = new int[count];

			for (int i = 0; i < count; ++i)
			{
				dst[i] = System.BitConverter.ToInt32(src, i * sizeof(int));
			}
		}
		private static void ByteToShort(ref short[] dst, Byte[] src)
		{
			int count = src.Length / sizeof(short);

			dst = new short[count];

			for (int i = 0; i < count; ++i)
			{
				dst[i] = System.BitConverter.ToInt16(src, i * sizeof(short));
			}
		}

#if IN_UNITY
		private static void FloatToVectorFlipZ(ref Vector3[] dst, float[] src)
		{
			int count = src.Length / 3;

			dst = new Vector3[count];

			for (int i = 0; i < count; ++i)
			{
				dst[i].Set(
					src[i * 3 + 0],
					src[i * 3 + 1],
					-src[i * 3 + 2]
				);
			}
		}
		private static void FloatToVector(ref Vector3[] dst, float[] src)
		{
			int count = src.Length / 3;

			dst = new Vector3[count];

			for (int i = 0; i < count; ++i)
			{
				dst[i].Set(
					src[i * 3 + 0],
					src[i * 3 + 1],
					src[i * 3 + 2]
				);
			}
		}
		private static void FloatToVector(ref Vector2[] dst, float[] src)
		{
			int count = src.Length / 2;

			dst = new Vector2[count];

			for (int i = 0; i < count; ++i)
			{
				dst[i].Set(
					src[i * 2 + 0],
					src[i * 2 + 1]
				);
			}
		}
		private static void ByteToColor32(ref Color32[] dst, Byte[] src)
		{
			int count = src.Length / 4;

			dst = new Color32[count];

			for (int i = 0; i < count; ++i)
			{
				dst[i].r = src[i * 4 + 0];
				dst[i].g = src[i * 4 + 1];
				dst[i].b = src[i * 4 + 2];
				dst[i].a = src[i * 4 + 3];
			}
		}
#endif

		/// <summary>
		/// Load the .dmx file.
		/// </summary>
		/// <param name="filename">The path of the file.</param>
		/// <param name="model">The DMXModel struct to fill with the file information.</param>
		/// <param name="error">An string that will contain any error it occurred in the loading (will be empty if no error occurred).</param>
		/// <param name="loadProgress">An array of floats holding the load progress for each texture. (Position [0] is equivalent to the model progress)</param>
		/// <param name="uniqueTexturePaths">Returns only the textures unique paths.</param>
		/// <returns>Returns whether the loading was sucessfull.</returns>
		public static bool Load(string filename, ref DXMModel model, ref string error, ref float loadProgress, out string[] uniqueTexturePaths)
		{
			try
			{
				model.header = new DXMHeader();

				FileStream fin;
				fin = File.OpenRead(filename);

				BinaryReader bin = new BinaryReader(fin);

				ReadHeader(ref bin, out model.header);

				int flagMesh = 0, flagPC = 0;

				//validation
				ValidateHeader(model.header, ref flagMesh, ref flagPC);

				float progressVerticesTotal = 0.5f; //contribution of vertice loading to progress.
				float progressElementsTotal = 0.3f; //contribution of indices loading to progress.
				float progressUVTotal = 0.1f; //contribution of unity vertex loading to progress.

				HashSet<string> imagePaths = new HashSet<string>();
				//groups
				model.groups = new DXMGroup[model.header.groupCount];
				for (int i = 0; i < model.header.groupCount; ++i)
				{
					model.groups[i].offset = bin.ReadUInt64();
					model.groups[i].length = bin.ReadUInt64();
					ushort len = bin.ReadUInt16();
					if (len > 0)
					{
						model.groups[i].texture = new string(bin.ReadChars(len - 1));
						bin.ReadByte();
					}
					if (model.groups[i].texture != String.Empty)
					{
						imagePaths.Add(model.groups[i].texture);
					}
				}

				uniqueTexturePaths = new string[imagePaths.Count];
				int pathsItt = 0;
				foreach (string path in imagePaths)
				{
					uniqueTexturePaths[pathsItt] = path;
					pathsItt++;
				}


				loadProgress = 1.0f - progressVerticesTotal - progressElementsTotal - progressUVTotal;//progress.

				int vertcount = (int)model.header.vertexCount * 3 * sizeof(float);
				int texcount = (int)model.header.vertexCount * 2 * sizeof(float);
				int colcount = (int)model.header.vertexCount * 4 * sizeof(byte);

				if (model.header.vertexCompositionFlags == flagPC)
				{
					progressVerticesTotal += progressElementsTotal;
				}

				//vertex data
				DXMData vertchunk = new DXMData();
				vertchunk.compressedSize = bin.ReadUInt64();
				vertchunk.uncompressedSize = bin.ReadUInt64();



				ByteToFloat(ref model.vertex, bin.ReadBytes(vertcount));
				loadProgress += progressVerticesTotal * 0.5f; //Updating vertice progress based on load.

				if (model.header.vertexCompositionFlags == flagMesh)
				{
					ByteToFloat(ref model.normal, bin.ReadBytes(vertcount));
					loadProgress += progressVerticesTotal * 0.25f; //Updating normal progress based on load.

					ByteToFloat(ref model.uv, bin.ReadBytes(texcount));
					loadProgress += progressVerticesTotal * 0.25f; //Updating uv progress based on load.

				}
				else if (model.header.vertexCompositionFlags == flagPC)
				{
					model.color = bin.ReadBytes(colcount);
					loadProgress += progressVerticesTotal * 0.5f; //Updating vertex color progress based on load.
				}

				//index data.
				DXMData indchunk = new DXMData();
				indchunk.compressedSize = bin.ReadUInt64();
				indchunk.uncompressedSize = bin.ReadUInt64();

				for (int i = 0; i < model.header.groupCount; ++i)
				{
					if (model.header.indexByteCount == 2)
					{
						ByteToShort(ref model.groups[i].index16, bin.ReadBytes((int)model.groups[i].length * 2));
						loadProgress += progressElementsTotal; //Updating indices progress based on load.

					}
					else if (model.header.indexByteCount == 4)
					{
						ByteToInt(ref model.groups[i].index32, bin.ReadBytes((int)model.groups[i].length * 4));
						loadProgress += progressElementsTotal; //Updating indices progress based on load.
					}
				}

				fin.Close();

#if IN_UNITY
				FloatToVectorFlipZ(ref model.vertexUnity, model.vertex);
				loadProgress += progressUVTotal * 0.5f; //change from float to a vector and update progress.

				if (model.header.vertexCompositionFlags == flagMesh)
				{
					FloatToVector(ref model.normalUnity, model.normal);
					loadProgress += progressUVTotal * 0.25f; //change from float to a vector and update progress.

					FloatToVector(ref model.uvUnity, model.uv);
					loadProgress += progressUVTotal * 0.25f; //change from float to a vector and update progress.

				}
				else if (model.header.vertexCompositionFlags == flagPC)
				{
					ByteToColor32(ref model.colorUnity, model.color);
					loadProgress += progressUVTotal * 0.5f; //change from float to a vector and update progress.
				}
#endif

			}
			catch (Exception e)
			{
				error = "Could not load file: " + filename + " with error: " + e.Message;
				uniqueTexturePaths = null;
				return false;
			}

			//HACK The progress count of this method is broken, We will have a task to fix this later.
			loadProgress = 1.0f;
			return true;
		}

		/// <summary>
		/// Checks if the file headers matches this DXM loader pattern.
		/// </summary>
		/// <param name="header">The header to checks.</param>
		/// <param name="flagMesh">A reference to the mesh vertex composition flag.</param>
		/// <param name="flagPC">A reference to the point cloud vertex composition flag.</param>
		private static void ValidateHeader(DXMHeader header, ref int flagMesh, ref int flagPC)
		{
			flagMesh |= (int)DXMVertexFlag.Vertex_3_F32;
			flagMesh |= (int)DXMVertexFlag.Normal_3_F32;
			flagMesh |= (int)DXMVertexFlag.Texcoord_2_F32;

			flagPC |= (int)DXMVertexFlag.Vertex_3_F32;
			flagPC |= (int)DXMVertexFlag.Color_4_U8;

			string ident = "" +
					Convert.ToChar(header.nameCharacter0) +
					Convert.ToChar(header.nameCharacter1) +
					Convert.ToChar(header.nameCharacter2) +
					Convert.ToChar(header.nameCharacter3);

			if (ident != "DXM1")
			{
				throw new Exception("Invalid file format of type: " + ident);
			}
			if ((header.majorVersion * 256 + header.minorVersion) < (2 * 256 + 2))
			{
				throw new Exception("Outdated loader.");
			}
			if (header.encoding != (int)DXMEncoding.DeInterleaved)
			{
				throw new Exception("Unsupported encoding.");
			}
			if (header.compression != (int)DXMCompression.NoCompression)
			{
				throw new Exception("Unsupported compression.");
			}
			if (header.vertexCompositionFlags != flagMesh && header.vertexCompositionFlags != flagPC)
			{
				throw new Exception("Unsupported vertex format.");
			}
		}
	}
}