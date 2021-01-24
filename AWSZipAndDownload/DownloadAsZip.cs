using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer( typeof( Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer ) )]

namespace AWSZipAndDownload
{
    public class DownloadAsZip
    {
        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task<APIGatewayProxyResponse> Download( APIGatewayProxyRequest request, ILambdaContext context )
        {
            var success = false;
            var stream = false;
            var response = new APIGatewayProxyResponse();
            response.Headers = new Dictionary<string, string>();
            try
            {
                if ( request.HttpMethod.Equals( "POST", StringComparison.OrdinalIgnoreCase ) )
                {
                    var fileToZip = JsonSerializer.Deserialize<ZipFiles.FilesToZip>( request.Body);
                    if ( string.IsNullOrEmpty( fileToZip.BucketNameFrom ) )
                        response.Body = "Missing from bucket!";
                    else if ( fileToZip.FileKeys == null && fileToZip.FileKeys.Count() == 0 )
                        response.Body = "Missing file keys!";
                    else //download
                    {
                        if ( !string.IsNullOrEmpty( fileToZip.DownloadStream ) && fileToZip.DownloadStream == "1" )
                        {
                            stream = true;
                            response.Body = await new ZipFiles().StreamZippedS3Files( fileToZip );
                            response.Headers.TryAdd( "Content-Type", "application/zip" );
                        }
                        else
                        {
                            if ( string.IsNullOrEmpty( fileToZip.BucketToZip ) )
                                response.Body = "Missing bucket to zip!";
                            else
                                response.Body = await new ZipFiles().ZipS3Files( fileToZip );
                            success = true;
                        }
                    }
                }
                else
                {
                    response.Body = "Invalid http method! Only POST is allowed.";
                }
            }
            catch ( Exception ex )
            {
                response.Body = $"Invalid parameter passed in request body: {ex.Message}";
            }
            response.StatusCode = (int)( success ? HttpStatusCode.OK : HttpStatusCode.BadRequest );
            response.IsBase64Encoded = stream;
            return response;
        }
    }
}