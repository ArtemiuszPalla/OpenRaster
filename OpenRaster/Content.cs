#region Copyright(c) 2013 Artemiusz Palla (artemiusz.palla@gmail.com)
//
//  ***********************************************************************
//  Project: OpenRaster
//  GitHub: https://github.com/ArtemiuszPalla/OpenRaster  
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//  ***********************************************************************
//
// if you want to support me, please play my mobile game: https://beatuptime.com
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace OpenRaster
{
    public class Content
    {
        /// <summary>
        /// Get layers list.
        /// </summary>
        public List<Layer> Layers
        {
            get;
            set;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Content()
        {
            Layers = new List<OpenRaster.Layer>();
        }

        /// <summary>
        /// Get open raster layer by name
        /// </summary>
        /// <param name="index">Layer name</param>
        public Layer GetLayer(string name)
        {
            Layer ret = null;
            if (Layers != null)
            {
                foreach (Layer l in Layers)
                {
                    if (l.Name == name)
                    {
                        ret = l;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Get open raster layer by index
        /// </summary>
        /// <param name="index">Layer index</param>
        public Layer GetLayer(int index)
        {
            if (Layers == null) return null;

            if (index >= 0 && index < Layers.Count)
            {
                return Layers[index];
            }
            else
            {
                return null;
            }
        }


        private static string GetAttribute(XmlElement element, string attributeName, string defaultValue)
        {
            attributeName = attributeName.ToLower();

            for (int i = 0; i < element.Attributes.Count; i++)
            {
                if (element.Attributes[i].Name.ToLower() == attributeName)
                {
                    return element.Attributes[i].Value;
                }
            }

            return defaultValue;
        }
        /// <summary>
        /// Load Open Raster image.
        /// </summary>
        /// <param name="filename">Name of the file</param>
        public void Load(string fileName)
        {

            ZipFile file = new ZipFile(fileName);
            XmlDocument stackXml = new XmlDocument();
            stackXml.Load(file.GetInputStream(file.GetEntry("stack.xml")));

            XmlElement imageElement = stackXml.DocumentElement;
            int width = int.Parse(imageElement.GetAttribute("w"));
            int height = int.Parse(imageElement.GetAttribute("h"));

            Size imagesize = new Size(width, height);


            XmlElement stackElement = (XmlElement)stackXml.GetElementsByTagName("stack")[0];
            XmlNodeList layerElements = stackElement.GetElementsByTagName("layer");

            if (layerElements.Count == 0)
                throw new XmlException("No layers found in OpenRaster file");

            Layers = new List<Layer>();



            int LayerCount = layerElements.Count - 1;

            for (int i = LayerCount; i >= 0; i--)
            {
                XmlElement layerElement = (XmlElement)layerElements[i];
                int x = int.Parse(GetAttribute(layerElement, "x", "0"));
                int y = int.Parse(GetAttribute(layerElement, "y", "0"));
                string name = GetAttribute(layerElement, "name", string.Format("Layer {0}", i));
                string visi = GetAttribute(layerElement, "visibility", "visible");

                ZipEntry zf = file.GetEntry(layerElement.GetAttribute("src"));
                Stream s = file.GetInputStream(zf);

                Layer cl = new Layer();
                cl.Image = BitmapFromLayer(x, y, s, width, height);
                cl.Name = name;
                cl.X = x;
                cl.Y = y;
                if (visi == "visible")
                {
                    cl.Visible = true;
                }
                else
                {
                    cl.Visible = false;
                }
                Layers.Add(cl);
            }
            file.Close();

        }

        /// <summary>
        ///  Load Open Raster image thumbnail.
        /// </summary>
        /// <param name="filename">Name of the file</param>
        public Image LoadThumbnail(string fileName)
        {
            Image ret = null;
            try
            {
                ZipFile file = new ZipFile(fileName);
                XmlDocument stackXml = new XmlDocument();
                ZipEntry zf = file.GetEntry("Thumbnails/thumbnail.png");
                Stream s = file.GetInputStream(zf);

                Layer cl = new Layer();
                ret = BitmapFromLayer(0, 0, s);
                file.Close();
            }
            catch
            {
            }
            return ret;
        }

        /// <summary>
        /// Get the image with merged layers.
        /// </summary>
        public System.Drawing.Image Image
        {
            get
            {

                System.Drawing.Bitmap newlayer = null;
                System.Drawing.Graphics pe = null;
                foreach (Layer layer in Layers)
                {
                    if (newlayer == null)
                    {
                        newlayer = new System.Drawing.Bitmap(layer.Image.Width, layer.Image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        pe = System.Drawing.Graphics.FromImage(newlayer);
                        pe.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    }
                    pe.DrawImageUnscaled(layer.Image, 0, 0);
                }
                pe.Dispose();
                return newlayer;
            }
        }


        /// <summary>
        /// Get thumbnail image with merged layers.
        /// </summary>
        public System.Drawing.Image Thumbnail
        {
            get
            {
                System.Drawing.Bitmap newlayer = null;
                System.Drawing.Graphics pe = null;
                Size ts = new Size(256, 256);
                foreach (Layer layer in Layers)
                {
                    if (newlayer == null)
                    {
                        newlayer = new System.Drawing.Bitmap(layer.Image.Width, layer.Image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        pe = System.Drawing.Graphics.FromImage(newlayer);
                        pe.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        ts = GetThumbDimensions(layer.Image.Width, layer.Image.Height);
                    }
                    pe.DrawImage(layer.Image, new System.Drawing.Rectangle(0, 0, ts.Width, ts.Height));
                }
                pe.Dispose();
                return newlayer;
            }
        }
        private const int ThumbMaxSize = 256;
        private Size GetThumbDimensions(int width, int height)
        {
            if (width <= ThumbMaxSize && height <= ThumbMaxSize)
                return new Size(width, height);

            if (width > height)
                return new Size(ThumbMaxSize, (int)((double)height / width * ThumbMaxSize));
            else
                return new Size((int)((double)width / height * ThumbMaxSize), ThumbMaxSize);
        }

        /// <summary>
        /// Save Open Raster file.
        /// </summary>
        /// <param name="filename">Name of the file</param>
        public void Save(string fileName)
        {
            ZipOutputStream stream = new ZipOutputStream(new FileStream(fileName, FileMode.Create));
            stream.UseZip64 = UseZip64.Off;
            ZipEntry mimetype = new ZipEntry("mimetype");
            mimetype.CompressionMethod = CompressionMethod.Stored;

            stream.PutNextEntry(mimetype);

            byte[] databytes = System.Text.Encoding.ASCII.GetBytes("image/openraster");
            stream.Write(databytes, 0, databytes.Length);

            for (int i = 0; i < Layers.Count; i++)
            {
                byte[] buf = new byte[0];
                using (MemoryStream mstream = new MemoryStream())
                {
                    Layers[i].Image.Save(mstream, System.Drawing.Imaging.ImageFormat.Png);
                    mstream.Close();

                    buf = mstream.ToArray();
                }
                stream.PutNextEntry(new ZipEntry("data/layer" + i.ToString() + ".png"));
                stream.Write(buf, 0, buf.Length);
            }

            stream.PutNextEntry(new ZipEntry("stack.xml"));
            databytes = GetLayerXmlData();
            stream.Write(databytes, 0, databytes.Length);

            stream.PutNextEntry(new ZipEntry("Thumbnails/thumbnail.png"));

            byte[] tbuf = new byte[0];
            using (MemoryStream mstream = new MemoryStream())
            {
                Thumbnail.Save(mstream, System.Drawing.Imaging.ImageFormat.Png);
                mstream.Close();
                tbuf = mstream.ToArray();
            }

            stream.Write(tbuf, 0, tbuf.Length);

            stream.Close();
        }


        private byte[] GetLayerXmlData()
        {
            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, System.Text.Encoding.UTF8);
            writer.Formatting = Formatting.Indented;

            writer.WriteStartElement("image");
            writer.WriteAttributeString("w", Layers[0].Image.Width.ToString());
            writer.WriteAttributeString("h", Layers[0].Image.Height.ToString());

            writer.WriteStartElement("stack");
            writer.WriteAttributeString("opacity", "1");
            writer.WriteAttributeString("name", "root");

            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                writer.WriteStartElement("layer");
                writer.WriteAttributeString("opacity", "1.00");
                writer.WriteAttributeString("name", Layers[i].Name);
                writer.WriteAttributeString("src", "data/layer" + i.ToString() + ".png");
                if (Layers[i].Visible == true)
                {
                    writer.WriteAttributeString("visibility", "visible");
                }
                else
                {
                    writer.WriteAttributeString("visibility", "hidden");
                }
                writer.WriteAttributeString("x", Layers[i].X.ToString());
                writer.WriteAttributeString("y", Layers[i].Y.ToString());
                writer.WriteAttributeString("composite-op", "svg:src-over");


                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.Close();
            return ms.ToArray();
        }


        /// <summary>
        /// Gets the bitmap from the Open Raster layer.
        /// </summary>
        /// <param name="x">The x offset of the layer.</param>
        /// <param name="y">The y offset of the layer.</param>
        /// <param name="stream">The input stream containing the layer image.</param>
        private unsafe Bitmap BitmapFromLayer(int x, int y, Stream stream)
        {
            Bitmap Image = null;

            Bitmap Layer = null;

            using (Bitmap BMP = new Bitmap(stream))
            {
                if (Layer == null)
                {
                    Layer = new Bitmap(BMP.Width, BMP.Height);
                }
                BitmapData LayerData = Layer.LockBits(new Rectangle(x, y, BMP.Width, BMP.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                BitmapData BMPData = BMP.LockBits(new Rectangle(0, 0, BMP.Width, BMP.Height), ImageLockMode.ReadOnly, BMP.PixelFormat);

                int bpp = Bitmap.GetPixelFormatSize(BMP.PixelFormat) / 8;

                for (int ya = 0; ya < BMP.Height; ya++)
                {
                    for (int xa = 0; xa < BMP.Width; xa++)
                    {
                        byte* dst = (byte*)LayerData.Scan0.ToPointer() + (ya * LayerData.Stride) + (xa * 4);
                        byte* src = (byte*)BMPData.Scan0.ToPointer() + (ya * BMPData.Stride) + (xa * bpp);

                        dst[0] = src[0]; // B
                        dst[1] = src[1]; // G
                        dst[2] = src[2]; // R

                        if (bpp == 4)
                        {
                            dst[3] = src[3]; // A
                        }
                        else
                        {
                            dst[3] = 255;
                        }
                    }
                }
                BMP.UnlockBits(BMPData);
                Layer.UnlockBits(LayerData);
            }
            if (Layer != null)
            {
                Image = (Bitmap)Layer.Clone();
                Layer.Dispose();
            }
            return Image;
        }



        /// <summary>
        /// Gets the bitmap from the Open Raster layer.
        /// </summary>
        /// <param name="x">The x offset of the layer.</param>
        /// <param name="y">The y offset of the layer.</param>
        /// <param name="stream">The input stream containing the layer image.</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        private unsafe Bitmap BitmapFromLayer(int x, int y, Stream stream, int width, int height)
        {
            Bitmap Image = null;

            using (Bitmap Layer = new Bitmap(width, height))
            {
                using (Bitmap BMP = new Bitmap(stream))
                {
                    BitmapData LayerData = Layer.LockBits(new Rectangle(x, y, BMP.Width, BMP.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    BitmapData BMPData = BMP.LockBits(new Rectangle(0, 0, BMP.Width, BMP.Height), ImageLockMode.ReadOnly, BMP.PixelFormat);

                    int bpp = Bitmap.GetPixelFormatSize(BMP.PixelFormat) / 8;

                    for (int ya = 0; ya < BMP.Height; ya++)
                    {
                        for (int xa = 0; xa < BMP.Width; xa++)
                        {
                            byte* dst = (byte*)LayerData.Scan0.ToPointer() + (ya * LayerData.Stride) + (xa * 4);
                            byte* src = (byte*)BMPData.Scan0.ToPointer() + (ya * BMPData.Stride) + (xa * bpp);

                            dst[0] = src[0]; // B
                            dst[1] = src[1]; // G
                            dst[2] = src[2]; // R

                            if (bpp == 4)
                            {
                                dst[3] = src[3]; // A
                            }
                            else
                            {
                                dst[3] = 255;
                            }
                        }
                    }
                    BMP.UnlockBits(BMPData);
                    Layer.UnlockBits(LayerData);
                }
                Image = (Bitmap)Layer.Clone();
            }
            return Image;
        }





    }
}
