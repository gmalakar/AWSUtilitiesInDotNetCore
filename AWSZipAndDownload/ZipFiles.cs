using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace AWSZipAndDownload
{
    internal class ZipFiles
    {
        public ZipFiles()
        {
        }

        public async Task<string> ZipS3Files( FilesToZip filesToZip )
        {
            var downloadKey = GetNewDownloadKey;
            using ( var zipStream = new MemoryStream() )
            {
                using ( var archive = new ZipArchive( zipStream, ZipArchiveMode.Create, true ) )
                {
                    foreach ( var key in filesToZip.FileKeys )
                    {
                        using ( var objectResponse = await s3Client.GetObjectAsync( filesToZip.BucketNameFrom, key ) )
                        {
                            var readmeEntry = archive.CreateEntry(Path.GetFileName( key ) );
                            using ( var entryStream = readmeEntry.Open() )
                            {
                                await objectResponse.ResponseStream.CopyToAsync( entryStream );
                            }
                        }
                    }
                }
                using ( var ut = new TransferUtility( s3Client ) )
                {
                    await ut.UploadAsync( zipStream, filesToZip.BucketToZip, downloadKey );
                }
            }
            return downloadKey;
        }

        public async Task<string> StreamZippedS3Files( FilesToZip filesToZip )
        {
            var base64 = string.Empty;
            using ( var zipStream = new MemoryStream() )
            {
                using ( var archive = new ZipArchive( zipStream, ZipArchiveMode.Create, true ) )
                {
                    foreach ( var key in filesToZip.FileKeys )
                    {
                        using ( var objectResponse = await s3Client.GetObjectAsync( filesToZip.BucketNameFrom, key ) )
                        {
                            var readmeEntry = archive.CreateEntry(Path.GetFileName( key ) );
                            using ( var entryStream = readmeEntry.Open() )
                            {
                                await objectResponse.ResponseStream.CopyToAsync( entryStream );
                            }
                        }
                    }
                }
                base64 = Convert.ToBase64String( zipStream.ToArray() );
            }
            return base64;
        }

        public static string GetNewDownloadKey
        {
            get
            {
                var now = DateTime.UtcNow;

                return $"{now.ToString( "yyyyMMdd" )}/download-{DateTime.Now.ToString( "yyyyMMddTHHmmss" )}.zip";
            }
        }

        public class FilesToZip
        {
            [JsonPropertyName( "bucketfrom" )]
            public string BucketNameFrom { get; set; }

            [JsonPropertyName( "buckettozip" )]
            public string BucketToZip { get; set; }

            [JsonPropertyName( "downloadstream" )]
            public string DownloadStream { get; set; }

            [JsonPropertyName( "filekeys" )]
            public string[] FileKeys { get; set; }
        }

        private static AmazonS3Client s3Client = new Amazon.S3.AmazonS3Client();
    }
}