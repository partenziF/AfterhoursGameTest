using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using GenericDatabase.EntityAdapter.Attributes;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace GenericDatabase.CollectionAdapter {
    public class CollectionAdapter<T> where T : Document.Document {

        internal FirebaseDatabase db;
        

        public CollectionAdapter( FirebaseDatabase Database ) {
            db = Database;
        
        }


        private string CollectionName() {

            object[] a = typeof( T ).GetCustomAttributes( typeof( CollectionAttribute ) , true );

            if ( ( a != null ) && ( a.Length == 1 ) ) {
                return ( a[0] as CollectionAttribute ).CollectionName;
            } else {
                throw new Exception( "Collection attribute not found" );
            }

        }

        public async Task<int> Count() {
            //var stopwatch = Stopwatch.StartNew();
            db.OpenCollection( CollectionName() );

            CollectionReference query = db.Collection;

            int count = 0;
            var stream = query.StreamAsync();
            await foreach ( var item in stream ) {
                count++;
            }
            //stopwatch.Stop();

            return count;

        }


        public async Task<List<T>> Select( int? offset = null , int? limit = null ) {

            try {

                db.OpenCollection( CollectionName() );

                CollectionReference query = db.Collection;

                QuerySnapshot querySnapshot;
                if ( ( offset != null ) && ( limit != null ) ) {
                    querySnapshot = await query.Offset( ( int ) offset ).Limit( ( int ) limit ).GetSnapshotAsync();
                } else {
                    querySnapshot = await query.GetSnapshotAsync();
                }


                List<T> result = new List<T>();
                foreach ( DocumentSnapshot documentSnapshot in querySnapshot.Documents ) {
                    var e = documentSnapshot.ConvertTo<T>();
                    e.ID = documentSnapshot.Id;
                    result.Add( e );
                }

                return result;

            } catch {
                throw;
            }
        }

        public async Task<List<T>> Select( Func<CollectionReference , Query> filterFunction ) {
            try {

                db.OpenCollection( CollectionName() );
                List<T> result = new List<T>();

                Query query = filterFunction( ( ( CollectionReference ) db.Collection ) );

                QuerySnapshot qSnapshot = await query.GetSnapshotAsync();
                foreach ( DocumentSnapshot document in qSnapshot.Documents ) {
                    var e = document.ConvertTo<T>();
                    e.ID = document.Id;
                    result.Add( e );

                }

                return result;

            } catch {
                throw;
            }
        }


    }

}
