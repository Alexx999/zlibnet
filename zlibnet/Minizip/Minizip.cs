using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Security;

namespace ZLibNet
{

	/// <summary>Support methods for uncompressing zip files.</summary>
	/// <remarks>
	///   <para>This unzip package allow extract file from .ZIP file, compatible with PKZip 2.04g WinZip, InfoZip tools and compatible.</para>
	///   <para>Encryption and multi volume ZipFile (span) are not supported.  Old compressions used by old PKZip 1.x are not supported.</para>
	///   <para>Copyright (C) 1998 Gilles Vollant.  http://www.winimage.com/zLibDll/unzip.htm</para>
	///   <para>C# wrapper by Gerry Shaw (gerry_shaw@yahoo.com).  http://www.organicbit.com/zip/</para>
	///   
	/// ZipLib = MiniZip part of zlib
	/// 
	/// </remarks>
	internal static class Minizip
	{
	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "setOpenUnicode", ExactSpelling = true)]
		static extern int setOpenUnicode(int openUnicode);

		internal static bool setOpenUnicode(bool openUnicode)
		{
			return setOpenUnicode(openUnicode ? 1 : 0) == 1;
		}

		static Minizip()
		{
			DllLoader.Load();
		}

        /*
		 Create a zipfile.
		 pathname contain on Windows NT a filename like "c:\\zlib\\zlib111.zip" or on an Unix computer "zlib/zlib111.zip".
		 if the file pathname exist and append=1, the zip will be created at the end of the file. (useful if the file contain a self extractor code)
		 If the zipfile cannot be opened, the return value is NULL.
		 Else, the return value is a zipFile Handle, usable with other function of this zip package.
	 */
        /// <summary>Create a zip file.</summary>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "zipOpen64", ExactSpelling = true, CharSet = CharSet.Unicode)]
		static extern IntPtr zipOpen(string fileName, int append);

		internal static IntPtr zipOpen(string fileName, bool append)
		{
			setOpenUnicode(true);
            return zipOpen(fileName, append ? 1 : 0);
		}
        /*
			Open a file in the ZIP for writing.
			filename : the filename in zip (if NULL, '-' without quote will be used
			*zipfi contain supplemental information
			if extrafield_local!=NULL and size_extrafield_local>0, extrafield_local contains the extrafield data the the local header
			if extrafield_global!=NULL and size_extrafield_global>0, extrafield_global contains the extrafield data the the local header
			if comment != NULL, comment contain the comment string
			method contain the compression method (0 for store, Z_DEFLATED for deflate)
			level contain the level of compression (can be Z_DEFAULT_COMPRESSION)
		*/
	    [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "zipOpenNewFileInZip4_64", ExactSpelling = true)]
		static extern int zipOpenNewFileInZip4_64(IntPtr handle,
			byte[] entryName,
			ref ZipFileEntryInfo entryInfoPtr,
			byte[] extraField,
			uint extraFieldLength,
			byte[] extraFieldGlobal,
			uint extraFieldGlobalLength,
			byte[] comment,
			int method,
			int level,
			int raw,
			int windowBits,
			int memLevel,
			int strategy,
			byte[] password,
			uint crcForCrypting,
			uint versionMadeBy,
			uint flagBase,
			int zip64);


		public static int zipOpenNewFileInZip4_64(IntPtr handle,
			byte[] entryName,
			ref ZipFileEntryInfo entryInfoPtr,
			byte[] extraField,
			uint extraFieldLength,
			byte[] extraFieldGlobal,
			uint extraFieldGlobalLength,
			byte[] comment,
			int method,
			int level,
			uint flagBase,
			bool zip64
			)
		{
			return zipOpenNewFileInZip4_64(handle, entryName, ref entryInfoPtr, extraField, extraFieldLength,
				extraFieldGlobal, extraFieldGlobalLength, comment, method, level, 0, -ZLib.MAX_WBITS,
				ZLib.DEF_MEM_LEVEL, ZLib.Z_DEFAULT_STRATEGY,
				null, 0, ZLib.VERSIONMADEBY, flagBase, zip64 ? 1 : 0);
		}



        /// <summary>Write data to the zip file.</summary>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "zipWriteInFileInZip", ExactSpelling = true)]
		internal static extern int zipWriteInFileInZip(IntPtr handle, IntPtr buffer, uint count);

        /// <summary>Close the current entry in the zip file.</summary>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "zipCloseFileInZip", ExactSpelling = true)]
		internal static extern int zipCloseFileInZip(IntPtr handle);

        /// <summary>Close the zip file.</summary>
        /// //file comment is for some weird reason ANSI, while entry name + comment is OEM...
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "zipClose", ExactSpelling = true, CharSet = CharSet.Ansi)]
		internal static extern int zipClose(IntPtr handle, string comment);

        /// <summary>Opens a zip file for reading.</summary>
        /// <param name="fileName">The name of the zip to open.</param>
        /// <returns>
        ///   <para>A handle usable with other functions of the ZipLib class.</para>
        ///   <para>Otherwise IntPtr.Zero if the zip file could not e opened (file doen not exist or is not valid).</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzOpen64", ExactSpelling = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr unzOpen(string fileName);

        /// <summary>Closes a zip file opened with unzipOpen.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <remarks>If there are files inside the zip file opened with <see cref="unzOpenCurrentFile"/> these files must be closed with <see cref="unzCloseCurrentFile"/> before call <c>unzClose</c>.</remarks>
        /// <returns>
        ///   <para>Zero if there was no error.</para>
        ///   <para>Otherwise a value less than zero.  See <see cref="ErrorCode"/> for the specific reason.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzClose", ExactSpelling = true)]
		internal static extern int unzClose(IntPtr handle);

        /// <summary>Get global information about the zip file.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <param name="globalInfoPtr">An address of a <see cref="ZipFileInfo"/> struct to hold the information.  No preparation of the structure is needed.</param>
        /// <returns>
        ///   <para>Zero if there was no error.</para>
        ///   <para>Otherwise a value less than zero.  See <see cref="ErrorCode"/> for the specific reason.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzGetGlobalInfo", ExactSpelling = true)]
		internal static extern int unzGetGlobalInfo(IntPtr handle, out ZipFileInfo globalInfoPtr);

        /// <summary>Get the comment associated with the entire zip file.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/></param>
        /// <param name="commentBuffer">The buffer to hold the comment.</param>
        /// <param name="commentBufferLength">The length of the buffer in bytes (8 bit characters).</param>
        /// <returns>
        ///   <para>The number of characters in the comment if there was no error.</para>
        ///   <para>Otherwise a value less than zero.  See <see cref="ErrorCode"/> for the specific reason.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzGetGlobalComment", ExactSpelling = true)]
		internal static extern int unzGetGlobalComment(IntPtr handle, byte[] commentBuffer, uint commentBufferLength);

        /// <summary>Set the current file of the zip file to the first file.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <returns>
        ///   <para>Zero if there was no error.</para>
        ///   <para>Otherwise a value less than zero.  See <see cref="ErrorCode"/> for the specific reason.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzGoToFirstFile", ExactSpelling = true)]
		internal static extern int unzGoToFirstFile(IntPtr handle);

        /// <summary>Set the current file of the zip file to the next file.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <returns>
        ///   <para>Zero if there was no error.</para>
        ///   <para>Otherwise <see cref="ErrorCode.EndOfListOfFile"/> if there are no more entries.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzGoToNextFile", ExactSpelling = true)]
		internal static extern int unzGoToNextFile(IntPtr handle);

        /// <summary>Try locate the entry in the zip file.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <param name="entryName">The name of the entry to look for.</param>
        /// <param name="caseSensitivity">If 0 use the OS default.  If 1 use case sensitivity like strcmp, Unix style.  If 2 do not use case sensitivity like strcmpi, Windows style.</param>
        /// <returns>
        ///   <para>Zero if there was no error.</para>
        ///   <para>Otherwise <see cref="ErrorCode.EndOfListOfFile"/> if there are no more entries.</para>
        /// </returns>
        //[DllImport(ZLibDll.Name, ExactSpelling = true, CharSet = CharSet.Ansi)]
        //public static extern int unzLocateFile(IntPtr handle, string entryName, int caseSensitivity);

        /// <summary>Get information about the current entry in the zip file.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <param name="entryInfoPtr">A ZipEntryInfo struct to hold information about the entry or null.</param>
        /// <param name="entryNameBuffer">An array of sbyte characters to hold the entry name or null.</param>
        /// <param name="entryNameBufferLength">The length of the entryNameBuffer in bytes.</param>
        /// <param name="extraField">An array to hold the extra field data for the entry or null.</param>
        /// <param name="extraFieldLength">The length of the extraField array in bytes.</param>
        /// <param name="commentBuffer">An array of sbyte characters to hold the entry name or null.</param>
        /// <param name="commentBufferLength">The length of theh commentBuffer in bytes.</param>
        /// <remarks>
        ///   <para>If entryInfoPtr is not null the structure will contain information about the current file.</para>
        ///   <para>If entryNameBuffer is not null the name of the entry will be copied into it.</para>
        ///   <para>If extraField is not null the extra field data of the entry will be copied into it.</para>
        ///   <para>If commentBuffer is not null the comment of the entry will be copied into it.</para>
        /// </remarks>
        /// <returns>
        ///   <para>Zero if there was no error.</para>
        ///   <para>Otherwise a value less than zero.  See <see cref="ErrorCode"/> for the specific reason.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzGetCurrentFileInfo64", ExactSpelling = true)]
		internal static extern int unzGetCurrentFileInfo64(
			IntPtr handle,
			out ZipEntryInfo64 entryInfoPtr,
			byte[] entryNameBuffer,
			uint entryNameBufferLength,
			byte[] extraField,
			uint extraFieldLength,
			byte[] commentBuffer,
			uint commentBufferLength);

	    [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
		public static extern uint GetOEMCP();

		public static Encoding OEMEncoding = Encoding.GetEncoding((int)Minizip.GetOEMCP());

        /// <summary>Open the zip file entry for reading.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <returns>
        ///   <para>Zero if there was no error.</para>
        ///   <para>Otherwise a value from <see cref="ErrorCode"/>.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzOpenCurrentFile", ExactSpelling = true)]
		internal static extern int unzOpenCurrentFile(IntPtr handle);

        /// <summary>Close the file entry opened by <see cref="unzOpenCurrentFile"/>.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <returns>
        ///   <para>Zero if there was no error.</para>
        ///   <para>CrcError if the file was read but the Crc does not match.</para>
        ///   <para>Otherwise a value from <see cref="ErrorCode"/>.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzCloseCurrentFile", ExactSpelling = true)]
		internal static extern int unzCloseCurrentFile(IntPtr handle);

        /// <summary>Read bytes from the current zip file entry.</summary>
        /// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
        /// <param name="buffer">Buffer to store the uncompressed data into.</param>
        /// <param name="count">Number of bytes to write from <paramref name="buffer"/>.</param>
        /// <returns>
        ///   <para>The number of byte copied if somes bytes are copied.</para>
        ///   <para>Zero if the end of file was reached.</para>
        ///   <para>Less than zero with error code if there is an error.  See <see cref="ErrorCode"/> for a list of possible error codes.</para>
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ZLibDll.DllName, EntryPoint = "unzReadCurrentFile", ExactSpelling = true)]
		internal static extern int unzReadCurrentFile(IntPtr handle, IntPtr buffer, uint count);

		/// <summary>Give the current position in uncompressed data of the zip file entry currently opened.</summary>
		/// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
		/// <returns>The number of bytes into the uncompressed data read so far.</returns>
		//[DllImport(ZLibDll.Name)]
		//public static extern long unztell(IntPtr handle);

		/// <summary>Determine if the end of the zip file entry has been reached.</summary>
		/// <param name="handle">The zip file handle opened by <see cref="unzOpenCurrentFile"/>.</param>
		/// <returns>
		///   <para>One if the end of file was reached.</para>
		///   <para>Zero if elsewhere.</para>
		/// </returns>
		//[DllImport(ZLibDll.Name)]
		//public static extern int unzeof(IntPtr handle);

	}

	internal static class ZipEntryFlag
	{
		internal const uint UTF8 = 0x800; //1 << 11
	}

	/// <summary>Global information about the zip file.</summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct ZipFileInfo
	{
		/// <summary>The number of entries in the directory.</summary>
		public UInt32 EntryCount;

		/// <summary>Length of zip file comment in bytes (8 bit characters).</summary>
		public UInt32 CommentLength;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ZipFileEntryInfo
	{
		public ZipDateTimeInfo ZipDateTime;
		public UInt32 DosDate;
		public UInt32 InternalFileAttributes; // 2 bytes
		public UInt32 ExternalFileAttributes; // 4 bytes
	}

	/// <summary>Custom ZipLib date time structure.</summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct ZipDateTimeInfo
	{
		/// <summary>Seconds after the minute - [0,59]</summary>
		public UInt32 Seconds;

		/// <summary>Minutes after the hour - [0,59]</summary>
		public UInt32 Minutes;

		/// <summary>Hours since midnight - [0,23]</summary>
		public UInt32 Hours;

		/// <summary>Day of the month - [1,31]</summary>
		public UInt32 Day;

		/// <summary>Months since January - [0,11]</summary>
		public UInt32 Month;

		/// <summary>Years - [1980..2044]</summary>
		public UInt32 Year;

		// implicit conversion from DateTime to ZipDateTimeInfo
		public static implicit operator ZipDateTimeInfo(DateTime date)
		{
			ZipDateTimeInfo d;
			d.Seconds = (uint)date.Second;
			d.Minutes = (uint)date.Minute;
			d.Hours = (uint)date.Hour;
			d.Day = (uint)date.Day;
			d.Month = (uint)date.Month - 1;
			d.Year = (uint)date.Year;
			return d;
		}

		public static implicit operator DateTime(ZipDateTimeInfo date)
		{
			DateTime dt = new DateTime(
				(int)date.Year,
				(int)date.Month + 1,
				(int)date.Day,
				(int)date.Hours,
				(int)date.Minutes,
				(int)date.Seconds);
			return dt;
		}

	}

	/// <summary>Information stored in zip file directory about an entry.</summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct ZipEntryInfo64
	{
		// <summary>Version made by (2 bytes).</summary>
		public UInt32 Version;

		/// <summary>Version needed to extract (2 bytes).</summary>
		public UInt32 VersionNeeded;

		/// <summary>General purpose bit flag (2 bytes).</summary>
		public UInt32 Flag;

		/// <summary>Compression method (2 bytes).</summary>
		public UInt32 CompressionMethod;

		/// <summary>Last mod file date in Dos fmt (4 bytes).</summary>
		public UInt32 DosDate;

		/// <summary>Crc-32 (4 bytes).</summary>
		public UInt32 Crc;

		/// <summary>Compressed size (8 bytes).</summary>
		public UInt64 CompressedSize;

		/// <summary>Uncompressed size (8 bytes).</summary>
		public UInt64 UncompressedSize;

		/// <summary>Filename length (2 bytes).</summary>
		public UInt32 FileNameLength;

		/// <summary>Extra field length (2 bytes).</summary>
		public UInt32 ExtraFieldLength;

		/// <summary>File comment length (2 bytes).</summary>
		public UInt32 CommentLength;

		/// <summary>Disk number start (2 bytes).</summary>
		public UInt32 DiskStartNumber;

		/// <summary>Internal file attributes (2 bytes).</summary>
		public UInt32 InternalFileAttributes;

		/// <summary>External file attributes (4 bytes).</summary>
		public UInt32 ExternalFileAttributes;

		/// <summary>File modification date of entry.</summary>
		public ZipDateTimeInfo ZipDateTime;
	}


	/// <summary>Specifies how the the zip entry should be compressed.</summary>
	public enum CompressionMethod
	{
		/// <summary>No compression.</summary>
		Stored = 0,

		/// <summary>Default and only supported compression method.</summary>
		Deflated = 8
	}

	/// <summary>Type of compression to use for the GZipStream. Currently only Decompress is supported.</summary>
	public enum CompressionMode
	{
		/// <summary>Compresses the underlying stream.</summary>
		Compress,
		/// <summary>Decompresses the underlying stream.</summary>
		Decompress,
	}

	/// <summary>List of possible error codes.
	/// 
	/// </summary>
	internal static class ZipReturnCode
	{
		/// <summary>No error.</summary>
		internal const int Ok = 0;

		/// <summary>Unknown error.</summary>
		internal const int Error = -1;

		/// <summary>Last entry in directory reached.</summary>
		internal const int EndOfListOfFile = -100;

		/// <summary>Parameter error.</summary>
		internal const int ParameterError = -102;

		/// <summary>Zip file is invalid.</summary>
		internal const int BadZipFile = -103;

		/// <summary>Internal program error.</summary>
		internal const int InternalError = -104;

		/// <summary>Crc values do not match.</summary>
		internal const int CrcError = -105;

		public static string GetMessage(int retCode)
		{
			switch (retCode)
			{
				case ZipReturnCode.Ok:
					return "No error";
				case ZipReturnCode.Error:
					return "Unknown error";
				case ZipReturnCode.EndOfListOfFile:
					return "Last entry in directory reached";
				case ZipReturnCode.ParameterError:
					return "Parameter error";
				case ZipReturnCode.BadZipFile:
					return "Zip file is invalid";
				case ZipReturnCode.InternalError:
					return "Internal program error";
				case ZipReturnCode.CrcError:
					return "Crc values do not match";
				default:
					return "Unknown error: " + retCode;
			}
		}
	}


	/// <summary>Thrown whenever an error occurs during the build.</summary>
	[Serializable]
	public class ZipException : ApplicationException
	{

		/// <summary>Constructs an exception with no descriptive information.</summary>
		public ZipException()
			: base()
		{
		}

		/// <summary>Constructs an exception with a descriptive message.</summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public ZipException(String message)
			: base(message)
		{
		}

		public ZipException(String message, int errorCode)
			: base(message + " (" + ZipReturnCode.GetMessage(errorCode) + ")")
		{
		}

		/// <summary>Constructs an exception with a descriptive message and a reference to the instance of the <c>Exception</c> that is the root cause of the this exception.</summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">An instance of <c>Exception</c> that is the cause of the current Exception. If <paramref name="innerException"/> is non-null, then the current Exception is raised in a catch block handling <paramref>innerException</paramref>.</param>
		public ZipException(String message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>Initializes a new instance of the BuildException class with serialized data.</summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		public ZipException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}


}
