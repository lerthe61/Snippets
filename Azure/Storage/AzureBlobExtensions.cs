using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace Common.Utils
{
    public static class AzureBlobExtensions
    {
        /// <summary>
        /// Move sourceBlob into destBlob
        /// </summary>
        /// <param name="sourceBlob"></param>
        /// <param name="destBlob"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Long running operation! Exception should be replaced by specific class</remarks>
        /// <returns></returns>
        public static async Task MoveBlob(this CloudBlockBlob sourceBlob, CloudBlockBlob destBlob)
        {
            await destBlob.FetchAttributesAsync();
            if (destBlob.Exists()) throw new ArgumentException($"{nameof(destBlob)} already exists: {destBlob.Uri}");

            var copyId = await destBlob.StartCopyAsync(sourceBlob);

            await destBlob.FetchAttributesAsync();
            while (destBlob.CopyState.Status == CopyStatus.Pending)
            {
                await Task.Delay(500);
                await destBlob.FetchAttributesAsync();
            }

            if (destBlob.CopyState.Status != CopyStatus.Success)
                throw new Exception("Rename failed: " + destBlob.CopyState.Status);

            await sourceBlob.DeleteIfExistsAsync();
        }

        // Move sourceBlob into destinationContainer
        // Long running operation!
        public static Task MoveBlob(this CloudBlockBlob sourceBlob, CloudBlobContainer destinationContainer)
        {
            CloudBlockBlob destBlob = destinationContainer.GetBlockBlobReference(sourceBlob.Name);
            return MoveBlob(sourceBlob, destBlob);
        }
    }
}