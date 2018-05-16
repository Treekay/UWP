using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MyList
{
    public class ImgManager
    {
        static public async Task<string> Picker()
        {
            string imgPath = string.Empty;
            Windows.Storage.Pickers.FileOpenPicker picker
                = new Windows.Storage.Pickers.FileOpenPicker
                {
                    ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
                };

            picker.FileTypeFilter.Clear();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile imgFile = await picker.PickSingleFileAsync();

            if (imgFile != null)
            {
                using (IRandomAccessStream fileStream
                    = await imgFile.OpenAsync(FileAccessMode.Read))
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    String Path = localFolder.Path;
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                    
                    StorageFile localFile = await localFolder.CreateFileAsync(imgFile.Name, CreationCollisionOption.ReplaceExisting);
                    await SaveSoftwareBitmapToFile(softwareBitmap, localFile);
                    imgPath = localFile.Path;
                }
            }
            return imgPath;
        }

        /*
        static public async Task<string> Opener(StorageFile file)
        {
            string bitmap;
            using (IRandomAccessStream fileStream
                = await file.OpenAsync(FileAccessMode.Read))
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                String Path = localFolder.Path;
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                StorageFile localFile = await localFolder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting);
                await SaveSoftwareBitmapToFile(softwareBitmap, localFile);

                bitmap = localFile.Path;
            }
            return bitmap;
        }
        */

        static public async Task<string> SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                encoder.SetSoftwareBitmap(softwareBitmap);

                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    switch (err.HResult)
                    {
                        case unchecked((int)0x88982F81):
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw err;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }
                return "";
            }
        }
    }
}
