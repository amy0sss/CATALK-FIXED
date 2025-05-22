using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaTALK.MVVM.ViewModels.Helpers
{
    public static class PhotoPickerHelper
    {
        public static async Task<string> PickPhotoAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync();

                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var filePath = Path.Combine(FileSystem.CacheDirectory, result.FileName);

                    using var fileStream = File.OpenWrite(filePath);
                    await stream.CopyToAsync(fileStream);

                    return filePath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error picking photo: {ex.Message}");
            }

            return null;
        }
    }
}
