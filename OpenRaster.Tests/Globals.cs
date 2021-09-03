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
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OpenRaster.Tests 
{
    internal class ResourceImageLoader
    {
        static System.Reflection.Assembly assembly = typeof(ResourceImageLoader).Assembly;

        static internal Bitmap Get(string name)
        {
            System.IO.Stream stream = assembly.GetManifestResourceStream("OpenRaster.Tests.Resources." + name);

            if (stream == null)
            {
                Console.WriteLine("Failed to read {0}", name);
                return null;
            }

            return new Bitmap(stream);
        }
    }

}
