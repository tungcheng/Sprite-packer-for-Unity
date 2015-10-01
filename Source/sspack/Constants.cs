using System;
using System.IO;

namespace sspack
{
	public static class Constants
	{
		// our default maximum sprite sheet size
		public const int DefaultMaximumSheetWidth = 4096;
		public const int DefaultMaximumSheetHeight = 4096;

		// our default image padding
		public const int DefaultImagePadding = 1;

		/// <summary>
		/// Returns the file name without the extension.
		/// If the path is absolute then only the filename is included otherwise the relative path is kept
		/// </summary>
		public static string PackedFilename(string filename)
		{
			if (Path.IsPathRooted(filename))
				return Path.GetFileNameWithoutExtension(filename);
			return filename.Substring(0, filename.Length - Path.GetExtension(filename).Length).Replace('\\', '/');
		}
	}
}
