using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using GenericDatabase.EntityAdapter.Attributes;
using GenericDatabase.Document;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AfterhoursGameTestLibrary.DatabaseModel {

    [CollectionAttribute( "users" )]
    [FirestoreData]
    public class User : Document {

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string FirstName { get; set; }

        [FirestoreProperty]
        public string LastName { get; set; }

        [FirestoreProperty]
        public string Nickname { get; set; }

    }

    [CollectionAttribute( "profile" )]
    [FirestoreData]
    public class Profile : Document {
        [FirestoreProperty]
        public string Filename { get; set; }

        [FirestoreProperty]
        public long ByteSize { get; set; }


        [FirestoreProperty]
        public string Url{ get; set; }

    }


    public class UserDocument : GenericDatabase.DocumentAdapter.DocumentAdapter<User> {

        public UserDocument( FirebaseDatabase Database ) : base( Database ) {

        }
        

    }

}
