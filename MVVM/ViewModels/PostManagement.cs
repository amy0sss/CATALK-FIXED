using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CaTALK.MVVM.ViewModels
{
    public class PostManagement : INotifyPropertyChanged
    {
        #region RESTApi Components
        HttpClient client;
        JsonSerializerOptions _serializerOptions;
        string baseUrl = "https://6809e3e61f1a52874cde3be6.mockapi.io/";
        #endregion
        public PostManagement()
        {
            client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions { WriteIndented = true };
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
