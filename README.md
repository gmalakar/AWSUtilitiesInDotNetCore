# AWSUtilitiesInDotNetCore

Started sharing utlities that I have created for AWS.

1. AWSZipAndDownload : Serverless api to zip files from s3 bucket and then move the zip file to specified zip bucket. Following is the structure of JSON payload to be posted to the api( POST method ). The api invokes a lambda method. To download the zip stream directly instead of copying to zip bucket, pass 'downloadstream' = 1 in payload.

    {
      "bucketfrom": "zipfrom",
      "buckettozip": "zipanddownload",
      "filekeys": [
        "docs/file1.txt",
        "docs/file2.log"
      ]
  }
