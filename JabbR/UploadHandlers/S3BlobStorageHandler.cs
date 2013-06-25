using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using JabbR.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Ninject;
using Amazon.S3.Model;

namespace JabbR.UploadHandlers
{
    public class S3BlobStorageHandler : IUploadHandler
    {
        private readonly IKernel _kernel;

        
        private const string JabbRUploadContainer = "jabbr-uploads";

        [ImportingConstructor]
        public S3BlobStorageHandler(IKernel kernel)
        {
            _kernel = kernel;
        }

        public bool IsValid(string fileName, string contentType)
        {
            //var settings = _kernel.Get<ApplicationSettings>();

            // Blob storage can handle any content
            //return !String.IsNullOrEmpty(settings.S3blobStorageConnectionString);
            return true;
        }

        public async Task<UploadResult> UploadFile(string fileName, string contentType, Stream stream)
        {
            String accessKey = "";
            String accessSecurity = "";

            

            // Randomize the filename everytime so we don't overwrite files
            string randomFile = Path.GetFileNameWithoutExtension(fileName) +
                                "_" +
                                Guid.NewGuid().ToString().Substring(0, 4) + Path.GetExtension(fileName);

            var client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKey, accessSecurity);
            var request = new PutObjectRequest();
            request.WithInputStream(stream);
            request.WithContentType(contentType);
            request.WithBucketName(JabbRUploadContainer);
            request.WithFilePath(randomFile);


            
            await Task.Factory.FromAsync((cb, state) => client.PutObject(request), ar => client.EndPutObject(request), null);

            var result = new UploadResult
            {
                Url = blockBlob.Uri.ToString(),
                Identifier = randomFile
            };

            return result;
        }
    }
}