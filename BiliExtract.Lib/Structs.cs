using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliExtract.Lib;

public readonly struct Size(double width, double height)
{
    public double Width { get; } = width;
    public double Height { get; } = height;
}
