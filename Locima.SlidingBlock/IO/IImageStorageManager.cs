using System;
using System.Windows.Media.Imaging;

namespace Locima.SlidingBlock.IO
{
    public interface IImageStorageManager
    {
        void Initialise();

        WriteableBitmap LoadImage(string imageId);
        WriteableBitmap LoadImage(Uri uri);
    }
}