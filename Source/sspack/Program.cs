#region MIT License

/*
 * Copyright (c) 2009-2010 Nick Gravelyn (nick@gravelyn.com), Markus Ewald (cygon@nuclex.org)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software 
 * is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 * 
 */

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace sspack
{
	public enum FailCode
	{
		Success = 0,
		FailedParsingArguments = 1,
		ImageExporter,
		MapExporter,
		NoImages,
		ImageNameCollision,

		FailedToLoadImage,
		FailedToPackImage,
		FailedToCreateImage,
		FailedToSaveImage,
		FailedToSaveMap
	}

	public class Program
	{
		static int Main(string[] args)
		{
			return (int)Launch(args);
		}

		public static FailCode Launch(string[] args)
		{
			ProgramArguments arguments = ProgramArguments.Parse(args);

			if (arguments == null)
				return FailCode.FailedParsingArguments;
			// make sure we have our list of exporters
			Exporters.Load();

			// try to find matching exporters
			IImageExporter imageExporter = null;
			IMapExporter mapExporter = null;

			string imageExtension = Path.GetExtension(arguments.image).Substring(1).ToLower();
			foreach (var exporter in Exporters.ImageExporters)
			{
				if (exporter.ImageExtension.ToLower() == imageExtension)
				{
					imageExporter = exporter;
					break;
				}
			}

			if (imageExporter == null)
			{
				Console.WriteLine("Failed to find exporters for specified image type.");
				return FailCode.ImageExporter;
			}

			if (!string.IsNullOrEmpty(arguments.map))
			{
				string mapExtension = Path.GetExtension(arguments.map).Substring(1).ToLower();
				foreach (var exporter in Exporters.MapExporters)
				{
					if (exporter.MapExtension.ToLower() == mapExtension)
					{
						mapExporter = exporter;
						break;
					}
				}

				if (mapExporter == null)
				{
					Console.WriteLine("Failed to find exporters for specified map type.");
					return FailCode.MapExporter;
				}
			}

			// compile a list of images
			List<string> images = new List<string>();
			FindImages(arguments, images);

			// make sure we found some images
			if (images.Count == 0)
			{
				Console.WriteLine("No images to pack.");
				return FailCode.NoImages;
			}

			// make sure no images have the same name if we're building a map
			if (mapExporter != null)
			{
				Dictionary<string, string> usedFileNames = new Dictionary<string, string>();

				for (int i = 0; i < images.Count; i++)
				{
					string packedFilename = Constants.PackedFilename(images[i]);

					if (usedFileNames.ContainsKey(packedFilename))
					{
						Console.WriteLine("Two images have the same name: {0} = {1}", images[i], usedFileNames[packedFilename]);
						return FailCode.ImageNameCollision;
					}
					usedFileNames.Add(packedFilename, images[i]);
				}
			}

			// generate our output
			ImagePacker imagePacker = new ImagePacker();
			Bitmap outputImage;
			Dictionary<string, Rectangle> outputMap;

			// pack the image, generating a map only if desired
			FailCode result = imagePacker.PackImage(images, arguments.pow2, arguments.sqr, arguments.mw, arguments.mh, arguments.pad, mapExporter != null, out outputImage, out outputMap);
			if (result != FailCode.Success)
			{
				Console.WriteLine("There was an error making the image sheet: " + result);
				return result;
			}

			// try to save using our exporters
			try
			{
				if (File.Exists(arguments.image))
					File.Delete(arguments.image);
				imageExporter.Save(arguments.image, outputImage);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error saving file: " + e.Message);
				return FailCode.FailedToSaveImage;
			}

			if (mapExporter != null)
			{
				try
				{
					if (File.Exists(arguments.map))
						File.Delete(arguments.map);
					mapExporter.Save(arguments.map, outputMap, Constants.PackedFilename(arguments.image), outputImage.Width, outputImage.Height);
				}
				catch (Exception e)
				{
					Console.WriteLine("Error saving file: " + e.Message);
					return FailCode.FailedToSaveMap;
				}
			}

			return 0;
		}

		private static void FindImages(ProgramArguments arguments, List<string> images)
		{
			List<string> inputFiles = new List<string>();

			if (!string.IsNullOrEmpty(arguments.il))
			{
				using (StreamReader reader = new StreamReader(arguments.il))
				{
					while (!reader.EndOfStream)
					{
						inputFiles.Add(reader.ReadLine());
					}
				}
			}

			if (arguments.input != null)
			{
				inputFiles.AddRange(arguments.input);
			}

			foreach (var str in inputFiles)
			{
				if (MiscHelper.IsImageFile(str))
				{
					images.Add(str);
				}
				else
				{
					Console.WriteLine("WARN: {0} is not an image file", str);
				}
			}
		}
	}
}
