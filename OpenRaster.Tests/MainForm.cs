#region Copyright(c) 2021 Artemiusz Palla (artemiusz.palla@gmail.com)
//
//  ***********************************************************************
//  Project: OpenRaster.Test
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenRaster.Tests
{
    public partial class MainForm : Form
    {

        public OpenRaster.Content ora = null;
        public MainForm()
        {
            InitializeComponent();
        }

        View openFileDialog1View = View.Details;

        private void button1_Click(object sender, EventArgs e)
        {
            Navigators.Forms.OpenImageFileDialog openFileDialog1 = new Navigators.Forms.OpenImageFileDialog();
            openFileDialog1.Filter = "Open Raster Image|*.ora";
            openFileDialog1.Title = "Open an Image";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.FileDialogUnknownIcon += FileDialogUnknownIcon;
            openFileDialog1.ImageFileDialogUnknownFile += openFileDialog1_ImageFileDialogUnknownFile;
            openFileDialog1.RestoreDirectory = false;
            openFileDialog1.AddExtension = true;
            openFileDialog1.View = openFileDialog1View;


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ora = new OpenRaster.Content();
                ora.Load(openFileDialog1.FileName);
                pictureBox1.Image = ora.Image;




            }
            openFileDialog1View = openFileDialog1.View;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Navigators.Forms.SaveImageFileDialog saveFileDialog1 = new Navigators.Forms.SaveImageFileDialog();
            saveFileDialog1.FileDialogUnknownIcon += FileDialogUnknownIcon;
            saveFileDialog1.ImageFileDialogUnknownFile += openFileDialog1_ImageFileDialogUnknownFile;
            saveFileDialog1.FileName = "Unnamed.ora";
            saveFileDialog1.Filter = "Open Raster|*.ora";
            saveFileDialog1.Title = "Save as graphic";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                ora.Save(saveFileDialog1.FileName);
            }
        }

        public void FileDialogUnknownIcon(object sender, Navigators.Forms.FileDialogUnknownIconEventArgs e)
        {
            switch (e.Extension)
            {
                case ".ora":
                    e.SmallImage = ResourceImageLoader.Get("Image-icon-16.png");
                    e.LargeImage = ResourceImageLoader.Get("Image-icon-32.png");
                    break;
                default:
                    e.LargeImage = null;
                    e.SmallImage = null;
                    break;
            }
        }
        public void openFileDialog1_ImageFileDialogUnknownFile(object sender, Navigators.Forms.ImageFileDialogUnknownFileEventArgs e)
        {
            switch (e.Extension)
            {
                case ".ora":
                    OpenRaster.Content c = new OpenRaster.Content();
                    e.Image = c.LoadThumbnail(e.FullName);
                    break;
                default:
                    e.Image = null;
                    break;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ora = new OpenRaster.Content();
            pictureBox1.Image = null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // lets make some random Commodore 64 like bars :)
            if (ora != null)
            {
                int width = pictureBox1.Width;
                int height = pictureBox1.Height;
                if (ora.Layers.Count > 0)
                {
                    width = ora.Image.Width;
                    height = ora.Image.Height;
                }

                Bitmap bmp = new Bitmap(width, height);
                Random rand = new Random();
                int h = 64;
                decimal rn = 0;
                int ri = 0;
                int a = 0;
                for (int y = 0; y < height; y++)
                {
                    //generate random ARGB value
                    int r = rand.Next(256);
                    int g = rand.Next(256);
                    int b = rand.Next(256);
                    //set ARGB value
                    for (int x = 0; x < width; x++)
                    {
                        rn = Convert.ToDecimal(x) / h;
                        ri = Convert.ToInt32(rn);
                        if (Convert.ToDecimal(ri) == rn)
                        {
                            a = rand.Next(64);
                        }
                        bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                    }
                }

                //create new layer in Open Raster file
                OpenRaster.Layer l = new Layer();
                l.Image = bmp; // add Bars to layer
                l.Name = "Random Bars Layer"; 
                l.Visible = true;
                l.X = 0;
                l.Y = 0;
                ora.Layers.Add(l);
                pictureBox1.Image = ora.Image;

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ora = new OpenRaster.Content();
            pictureBox1.Image = null;
        }
    }
}