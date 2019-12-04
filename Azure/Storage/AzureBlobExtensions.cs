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
        public static async Task MoveBlob(this CloudBlockBlob sourceBlob, CloudBlobContainer destinationContainer)
        {
            if (!destinationContainer.Exists())
            {
                await destinationContainer.CreateAsync();
            }
            CloudBlockBlob destBlob = destinationContainer.GetBlockBlobReference(sourceBlob.Name);
            await MoveBlob(sourceBlob, destBlob);
        }

        public static Uri GetBlobSasUri(
            CloudBlobContainer container,
            string blobName,
            SharedAccessBlobPermissions permissions,
            DateTime from,
            DateTime to,
            string policyName = null)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            var sasToken = GetBlobSasToken(blob, from, to, permissions, policyName);

            return new Uri(blob.Uri, sasToken);
        }

        public static string GetBlobSasToken(
            CloudBlobContainer container, 
            string blobName, 
            SharedAccessBlobPermissions permissions, 
            DateTime from,
            DateTime to,
            string policyName = null)
        {
            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            return GetBlobSasToken(blob, from, to, permissions, policyName);
        }

        private static string GetBlobSasToken(
            CloudBlockBlob blob,
            DateTime from, 
            DateTime to,
            SharedAccessBlobPermissions permissions,
            string policyName)
        {
            string sasBlobToken;
            if (policyName == null)
            {
                var adHocSas = CreateAdHocSasPolicy(permissions, @from, to);

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSas);
            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
            }

            return sasBlobToken;
        }

        private static SharedAccessBlobPolicy CreateAdHocSasPolicy(SharedAccessBlobPermissions permissions, DateTime from, DateTime to)
        {
            // Create a new access policy and define its constraints.
            // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and 
            // to construct a shared access policy that is saved to the container's shared access policies. 

            return new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = from,
                SharedAccessExpiryTime = to,
                Permissions = permissions
            };
        }
    }
}