using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Client.Utils.NativeFunctions;

namespace Client.Utils
{
    class FileIcon
    {
        private static int GetIconIndex(string pszFile)
        {
            SHFILEINFO sfi = new SHFILEINFO();
            SHGetFileInfo(pszFile
                , 0
                , ref sfi
                , (uint)System.Runtime.InteropServices.Marshal.SizeOf(sfi)
                , (uint)(SHGFI.SysIconIndex | SHGFI.LargeIcon | SHGFI.UseFileAttributes));
            return sfi.iIcon;
        }

        // 256 * 256
       private static IntPtr GetJumboIconHandle(int iImage)
        {
            IImageList spiml = null;
            Guid guil = new Guid(IID_IImageList2);//or IID_IImageList

            SHGetImageList(SHIL_JUMBO, ref guil, ref spiml);
            IntPtr hIcon = IntPtr.Zero;
            spiml.GetIcon(iImage, ILD_TRANSPARENT | ILD_IMAGE, ref hIcon);

            return hIcon;
        }

        public static void DisposeIcon(IntPtr handle)
        {
            DestroyIcon(handle);
        }
        // "*.txt"
        public static Icon GetJumboIcon(string extension)
        {
            IntPtr hIcon = GetJumboIconHandle(GetIconIndex(extension));

            Icon ico = (Icon)Icon.FromHandle(hIcon).Clone();
            return ico;
        }
    }
}
