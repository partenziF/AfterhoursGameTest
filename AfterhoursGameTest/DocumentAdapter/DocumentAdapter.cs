using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using GenericDatabase.EntityAdapter.Attributes;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenericDatabase.DocumentAdapter {


    public class DocumentAdapter<T> where T : Document.Document {

        internal FirebaseDatabase db;
        protected DocumentAdapter( FirebaseDatabase Database ) {
            db = Database;
        }


        public string CollectionName() {

            object[] a = typeof( T ).GetCustomAttributes( typeof( CollectionAttribute ) , true );

            if ( ( a != null ) && ( a.Length == 1 ) ) {
                return ( a[0] as CollectionAttribute ).CollectionName;
            } else {
                throw new Exception( "Collection attribute not found" );
            }

        }

        public virtual async Task<bool> Create( T document ) {

            if ( document is null ) {
                throw new ArgumentNullException( nameof( document ) );
            }

            try {

                if ( db.OpenCollection( CollectionName() ) ) {
                    
                    var d = await ( db.Collection )?.AddAsync( document ) ?? null;
                    if ( d != null ) {
                        document.ID = d.Id;
                        return true;
                    }

                    return false;

                } else {
                    return false;
                }

            } catch ( Exception e ) {
                throw new Exception( String.Format( "Error while read data {0}" , e.Message ) );
            }

        }

        public virtual Task<T> Read( string id ) {

            if ( string.IsNullOrWhiteSpace( id ) ) {
                throw new ArgumentException( $"'{nameof( id )}' non può essere Null o uno spazio vuoto." , nameof( id ) );
            }

            try {

                if ( db.OpenCollection( CollectionName() ) ) {
                    return db.OpenDocument<T>( id );

                } else {
                    return null;
                }

            } catch ( Exception e ) {
                throw new Exception( String.Format( "Error while read data {0}" , e.Message ) );
            }

        }


        public async Task<bool> Update( T document ) {

            if ( document is null ) {
                throw new ArgumentNullException( nameof( document ) );
            }


            try {

                if ( db.OpenCollection( CollectionName() ) ) {

                    var documentReference = ( ( CollectionReference ) db.Collection )?.Document( document.ID ) ?? null;

                    if ( documentReference != null ) {

                        if ( await documentReference.SetAsync( document , SetOptions.Overwrite ) != null ) {
                            return true;
                        } else {
                            return false;
                        }

                    }

                    return false;

                } else {
                    return false;
                }

            } catch {
                throw;
            }


        }


        public async Task<bool> Delete( T document ) {

            if ( document is null ) {
                throw new ArgumentNullException( nameof( document ) );
            }

            try {

                if ( db.OpenCollection( CollectionName() ) ) {

                    var documentReference = ( ( CollectionReference ) db.Collection )?.Document( document.ID ) ?? null;

                    if ( documentReference != null ) {

                        if ( await documentReference.DeleteAsync() != null ) {
                            return true;
                        } else {
                            return false;
                        }

                    }

                    return false;

                } else {
                    return false;
                }

            } catch {
                throw;
            }


        }

    }

}
