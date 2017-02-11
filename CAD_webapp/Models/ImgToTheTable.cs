using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;


namespace CAD_webapp.Models
{
    public class ImageEntity : TableEntity
    {
        public ImageEntity()
        {
            this.PartitionKey = "Images";
            this.RowKey = Guid.NewGuid().ToString();
        }

        public string Full_img { get; set; }
        public string Mini_img { get; set; }
    }

    public class ImgToTheTable
    {     
        public static void CreateIfNotExist(ImageEntity ie)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("cadtable");
            table.CreateIfNotExists();

            TableOperation insertOperation = TableOperation.Insert(ie);
            table.Execute(insertOperation);
         }
    }
}