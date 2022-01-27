using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenericDatabase.Database.Interface {

    public interface ILastErrorInfo {

        public string Message { get; set; }

    }

    public interface INoSqlDatabase {
        public ILogger Logger { set; }
        public bool IsConnected { get; }

        public string ProjectId { get; }

        ILastErrorInfo LastError { get; set; }

        public bool OpenConnection();
        public bool CloseConnection();

        public bool OpenCollection( string name );

        public Task<T> OpenDocument<T>( string id ) where T : Document.Document;




    }

}
