using System.Collections.Generic;
using System.Drawing;
using System.IO;
using sspack;

namespace SpriteSheetPacker.XnaExporters
{
    public class TpsheetMapExporter : sspack.IMapExporter
    {
        public string MapExtension
        {
            get { return "tpsheet"; }
        }

        public void Save(string filename, Dictionary<string, Rectangle> map, string imagename, int width, int height)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("#");
                writer.WriteLine("# Sprite sheet data for Unity.");
                writer.WriteLine("#");
                writer.WriteLine("# To import these sprites into your Unity project, download \"TexturePackerImporter\":");
                writer.WriteLine("# http://www.codeandweb.com/texturepacker/unity");
                writer.WriteLine("#");
                writer.WriteLine("# Sprite sheet: " + imagename + ".png" + " (" + width +" x " + height + ")");
                writer.WriteLine("# $TexturePacker:SmartUpdate:e55b8c4f3ddfb2256f733ac574c815fe:e45208a77f0123a47f89a2458b671c11:d1481b57c0ea74c51c709758eb0aef6a$");
                writer.WriteLine("#");
                writer.WriteLine("");

                foreach (var entry in map)
                {
                    Rectangle r = entry.Value;
                    writer.WriteLine("{0};{1};{2};{3};{4};0.5;0.5", Constants.PackedFilename(entry.Key), r.X, r.Y, r.Width, r.Height);
                }
            }
        }
    }
}
