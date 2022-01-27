using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AfterhoursGameTest.Storage {
    public class StorageManager : IStorageManager {
        private readonly StorageClient mStorage;
        private string mBucketName;

        public StorageManager(string bucketName) {
            mStorage = StorageClient.Create();
            mBucketName = bucketName;
        }

        public string BucketName { get => mBucketName; set => mBucketName = value; }

        public bool Upload( Stream fileStream , string filename ) {

            if ( string.IsNullOrEmpty( mBucketName ) ) throw new ArgumentNullException( "Invalid bucket name" );

            try {

                Google.Apis.Storage.v1.Data.Object x = mStorage.UploadObject( BucketName , filename , null , fileStream );

            } catch {
                throw;
            }


            return true;


        }

        public bool Upload( string pathToFileTemp , string filename ) {

            if ( string.IsNullOrEmpty( mBucketName ) ) throw new ArgumentNullException( "Invalid bucket name" );

            if ( string.IsNullOrEmpty( pathToFileTemp ) ) {
                throw new ArgumentException( $"'{nameof( pathToFileTemp )}' non può essere null o vuoto." , nameof( pathToFileTemp ) );
            }

            if ( string.IsNullOrEmpty( filename ) ) {
                throw new ArgumentException( $"'{nameof( filename )}' non può essere null o vuoto." , nameof( filename ) );
            }

            if ( !File.Exists( pathToFileTemp ) ) {
                throw new ArgumentException( "Invalid file name" );
            }

            try {

                using FileStream fileStream = File.OpenRead( pathToFileTemp );
                mStorage.UploadObject( BucketName , filename , null , fileStream );

            } catch {
                throw;
            }


            return true;


        }


        public bool NewFolder( string FolderName ) {

            try {

                if ( !FolderName.EndsWith( "/" ) )
                    FolderName += "/";

                var content = Encoding.UTF8.GetBytes( "" );

                var x = mStorage.UploadObject( BucketName , FolderName , "application/x-directory" , new MemoryStream( content ) );

                return true;

            } catch {
                throw;

            }

        }


        public bool Exists( String FileName ) {

            try {


                var storageObject = mStorage.GetObject( BucketName , FileName , new GetObjectOptions { Projection = Projection.Full } );

                return ( storageObject != null );

            } catch  {
                return false;
            }

        }

        public string GetUrl( string Filename ) {
            try {

                var o = mStorage.GetObject( BucketName , Filename );
                return o.MediaLink;

            } catch {
                throw;
            }
        }
    
    
        public bool DeleteFolder(string FilenName ) {
            try {
                if ( !FilenName.EndsWith( "/" ) ) { FilenName += "/"; }


                ListObjectsOptions listObjectsOptions = new ListObjectsOptions {
                    Delimiter = "/"
                };
                foreach ( var obj in mStorage.ListObjects( BucketName, FilenName , listObjectsOptions ) ) {
                    if (obj.ContentType!= "application/x-directory" ) {
                        mStorage.DeleteObject( BucketName , obj.Name , new DeleteObjectOptions { } );
                    }                    
                }

                mStorage.DeleteObject( BucketName , FilenName , new DeleteObjectOptions { } );
                return true;
            }   catch {
                return false;
            }
        }
    }

}
