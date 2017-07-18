using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MSA_app
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EasyTables : ContentPage
	{

        MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;

        public EasyTables ()
		{
			InitializeComponent ();
            this.Title = "Table of Misery";
        }

        async void Handle_ClickedAsync(object sender, System.EventArgs e)
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;
            List<deadTable> deadInformation = await AzureManager.AzureManagerInstance.GetInformation();
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
            deadList.ItemsSource = deadInformation;
        }
    }
}