using System;
using System.Collections.Generic;

namespace Org.GraphStream.Stream.File.Images
{
    /// <summary>
    /// Common resolutions.
    /// </summary>
    public enum Resolutions
    {
        QVGA, CGA, VGA, NTSC, PAL, WVGA_5by3, SVGA, WVGA_16by9, WSVGA, XGA, 
        HD720, WXGA_5by3, WXGA_8by5, SXGA, FWXGA, SXGAp, WSXGAp, UXGA, 
        HD1080, WUXGA, TwoK, QXGA, WQXGA, QSXGA, UHD_4K, UHD_8K_16by9, UHD_8K_17by8
    }

    public interface IResolution
    {
        int Width { get; }
        int Height { get; }
    }
}
