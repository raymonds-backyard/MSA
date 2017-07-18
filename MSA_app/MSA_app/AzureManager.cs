using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using MSA_app;

namespace MSA_app
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<deadTable> deadTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("https://mydeadfriends.azurewebsites.net");
            this.deadTable = this.client.GetTable<deadTable>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<deadTable>> GetInformation()
        {
            return await this.deadTable.ToListAsync();
        }

        public async Task PostInfo(deadTable entry)
        {
            await this.deadTable.InsertAsync(entry);
        }
    }
}
