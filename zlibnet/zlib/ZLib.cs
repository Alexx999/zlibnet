﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.IO;
using System.Reflection;
using System.Security;

namespace ZLibNet
{
	public static class ZLib
	{
		internal static IntPtr ZLibVersion = Marshal.StringToHGlobalAnsi("1.2.8");
		internal static int ZStreamSize = Marshal.SizeOf(typeof(z_stream));
		internal const int MAX_WBITS = 15; /* 32K LZ77 window */
		internal const int DEF_MEM_LEVEL = 8;
		internal const int Z_DEFAULT_STRATEGY = 0;
		internal const uint VERSIONMADEBY = 0;

		const int Z_DEFLATED = 8;
		/* The deflate compression method (the only one supported in this version) */

		static ZLib()
		{
			DllLoader.Load();
		}

	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "inflateInit2_", ExactSpelling = true, CharSet = CharSet.Ansi)]
		static extern int inflateInit2(ref z_stream strm, int windowBits, IntPtr version, int stream_size);

	 	public static int inflateInit(ref z_stream strm, ZLibOpenType windowBits)
		{
            return inflateInit2(ref strm, (int)windowBits, ZLib.ZLibVersion, ZLib.ZStreamSize);
		}

	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "deflateInit2_", ExactSpelling = true, CharSet = CharSet.Ansi)]
		static extern int deflateInit2(ref z_stream strm, int level, int method, int windowBits,
			int memLevel, int strategy, IntPtr version, int stream_size);

		public static int deflateInit(ref z_stream strm, CompressionLevel level, ZLibWriteType windowBits)
		{
			return deflateInit2(ref strm, (int)level, Z_DEFLATED, (int)windowBits, DEF_MEM_LEVEL,
				Z_DEFAULT_STRATEGY, ZLib.ZLibVersion, ZLib.ZStreamSize);
		}

	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "inflate", ExactSpelling = true)]
		public static extern int inflate(ref z_stream strm, ZLibFlush flush);

	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "deflate", ExactSpelling = true)]
		public static extern int deflate(ref z_stream strm, ZLibFlush flush);

	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "inflateEnd", ExactSpelling = true)]
		public static extern int inflateEnd(ref z_stream strm);

	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "deflateEnd", ExactSpelling = true)]
		public static extern int deflateEnd(ref z_stream strm);

	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "crc32", ExactSpelling = true)]
		public static extern uint crc32(uint crc, IntPtr buffer, uint len);
	}

	public enum ZLibFlush
	{
		NoFlush = 0, //Z_NO_FLUSH
		PartialFlush = 1,
		SyncFlush = 2,
		FullFlush = 3,
		Finish = 4 // Z_FINISH
	}

	enum ZLibCompressionStrategy
	{
		Filtered = 1,
		HuffmanOnly = 2,
		DefaultStrategy = 0
	}

    //enum ZLibCompressionMethod
    //{
    //    Delated = 8
    //}

	public enum ZLibDataType
	{
		Binary = 0,
		Ascii = 1,
		Unknown = 2,
	}

	public enum ZLibOpenType
	{
		//If a compressed stream with a larger window
		//size is given as input, inflate() will return with the error code
		//Z_DATA_ERROR instead of trying to allocate a larger window.
		Deflate = -15, // -8..-15
		ZLib = 15, // 8..15, 0 = use the window size in the zlib header of the compressed stream.
		GZip = 15 + 16,
		Both_ZLib_GZip = 15 + 32,
	}

	public enum ZLibWriteType
	{
		//If a compressed stream with a larger window
		//size is given as input, inflate() will return with the error code
		//Z_DATA_ERROR instead of trying to allocate a larger window.
		Deflate = -15, // -8..-15
		ZLib = 15, // 8..15, 0 = use the window size in the zlib header of the compressed stream.
		GZip = 15 + 16,
		//		Both = 15 + 32,
	}

	public enum CompressionLevel
	{
		NoCompression = 0,
		BestSpeed = 1,
		BestCompression = 9,
		// The "real" default is -1. Currently, zlib interpret -1 as 6, but they are free to change the interpretation.
		// The reason for overriding the default and using 5 is I want this library to match DynaZip's default
		// compression ratio and speed, and 5 was the best match (6 was somewhat slower than dynazip default).
		Default = 5,
		Level0 = 0,
		Level1 = 1,
		Level2 = 2,
		Level3 = 3,
		Level4 = 4,
		Level5 = 5,
		Level6 = 6,
		Level7 = 7,
		Level8 = 8,
		Level9 = 9
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct z_stream
	{
		public IntPtr next_in;  /* next input byte */
		public uint avail_in;  /* number of bytes available at next_in */
		public uint total_in;  /* total nb of input bytes read so far */

		public IntPtr next_out; /* next output byte should be put there */
		public uint avail_out; /* remaining free space at next_out */
		public uint total_out; /* total nb of bytes output so far */

		private IntPtr msg;      /* last error message, NULL if no error */

		private IntPtr state; /* not visible by applications */

		private IntPtr zalloc;  /* used to allocate the internal state */
		private IntPtr zfree;   /* used to free the internal state */
		private IntPtr opaque;  /* private data object passed to zalloc and zfree */

		public ZLibDataType data_type;  /* best guess about the data type: ascii or binary */
		public uint adler;      /* adler32 value of the uncompressed data */
		private uint reserved;   /* reserved for future use */

		public string lasterrormsg
		{
			get
			{
				return Marshal.PtrToStringAnsi(msg);
			}
		}
	}

	public static class ZLibReturnCode
	{
		public const int Ok = 0;
		public const int StreamEnd = 1; //positive = no error
		public const int NeedDictionary = 2; //positive = no error?
		public const int Errno = -1;
		public const int StreamError = -2;
		public const int DataError = -3; //CRC
		public const int MemoryError = -4;
		public const int BufferError = -5;
		public const int VersionError = -6;

		public static string GetMesage(int retCode)
		{
			switch (retCode)
			{
				case ZLibReturnCode.Ok:
					return "No error";
				case ZLibReturnCode.StreamEnd:
					return "End of stream reaced";
				case ZLibReturnCode.NeedDictionary:
					return "A preset dictionary is needed";
				case ZLibReturnCode.Errno: //consult error code
					return "Unknown error " + Marshal.GetLastWin32Error();
				case ZLibReturnCode.StreamError:
					return "Stream error";
				case ZLibReturnCode.DataError:
					return "Data was corrupted";
				case ZLibReturnCode.MemoryError:
					return "Out of memory";
				case ZLibReturnCode.BufferError:
					return "Not enough room in provided buffer";
				case ZLibReturnCode.VersionError:
					return "Incompatible zlib library version";
				default:
					return "Unknown error";
			}
		}
	}


	[Serializable]
	public class ZLibException : ApplicationException
	{
		public ZLibException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ZLibException(int errorCode)
			: base(GetMsg(errorCode, null))
		{

		}

		public ZLibException(int errorCode, string lastStreamError)
			: base(GetMsg(errorCode, lastStreamError))
		{
		}

		private static string GetMsg(int errorCode, string lastStreamError)
		{
			string msg = "ZLib error " + errorCode + ": " + ZLibReturnCode.GetMesage(errorCode);
			if (lastStreamError != null && lastStreamError.Length > 0)
				msg += " (" + lastStreamError + ")";
			return msg;
		}
	}
}
