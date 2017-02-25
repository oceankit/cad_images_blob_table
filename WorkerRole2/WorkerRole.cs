using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using CAD_webapp.Models;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace WorkerRole2
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole2 has stopped");
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }


        private CloudBlobContainer GetImageBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=cadwebstorage;AccountKey=XaIkCaArOs/tXSwK1v3jTUUt/+OOoB/pZKVV0QclJotS35eIbp/Cm1fF7xC90oi/qSMCMZBvLv0EnUCdYNkbsA==;");

            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("cadblob");

            container.CreateIfNotExists();

            container.SetPermissions(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
            return container;
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=cadwebstorage;AccountKey=XaIkCaArOs/tXSwK1v3jTUUt/+OOoB/pZKVV0QclJotS35eIbp/Cm1fF7xC90oi/qSMCMZBvLv0EnUCdYNkbsA==;");
            
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            
            CloudQueue queue = queueClient.GetQueueReference("cadqueue");
            
            CloudQueueMessage retrievedMessage = queue.GetMessage();
            string[] msg = retrievedMessage.AsString.Split(' ');

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("cadtable");

            TableOperation retrieveOperation = TableOperation.Retrieve<ImageEntity>("Images", msg[0]);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            var container = GetImageBlobContainer();
            msg[2] = "mini_" + msg[2];
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(msg[2]);
            blockBlob.Properties.ContentType = msg[1];

            Image img;
            var webClient = new WebClient();
            byte[] imgBytes = webClient.DownloadData(((ImageEntity)retrievedResult.Result).Full_img);
            using (var ms = new MemoryStream(imgBytes))
            {
                img = Image.FromStream(ms);
            }
            Image thumb = ScaleImage(img, 220, 160);
            var fileBytes = imageToByteArray(thumb);
            await blockBlob.UploadFromByteArrayAsync(fileBytes, 0, fileBytes.Length);

            ImageEntity ent = (ImageEntity)retrievedResult.Result;
            if (ent != null)
            {
                ent.Mini_img = "https://cadwebstorage.blob.core.windows.net/cadblob/" + msg[2];
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(ent);
                table.Execute(insertOrReplaceOperation);
            }
            else
            {
                Console.WriteLine("Entity could not be retrived.");
            }

            queue.DeleteMessage(retrievedMessage);

            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(10000);
            }
        }
        
    }
}
