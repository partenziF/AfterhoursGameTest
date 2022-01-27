using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AfterhoursGameTest.Storage {
    public interface IStorageManager {

        public string BucketName { get; set; }

        bool DeleteFolder( string FilenName );
        bool Exists( string FileName );
        string  GetUrl( string Filename );
        bool NewFolder( string FolderName );
        public bool Upload( string tempToPath , string filename );
        public bool Upload( Stream fileStream , string filename );

    }
}
