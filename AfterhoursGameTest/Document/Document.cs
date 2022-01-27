using GenericDatabase.EntityAdapter.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericDatabase.Document {
    public abstract class Document : IDocument { 

        private string mId;
        public string ID { get => mId; set => mId = value; }


        public string CollectionName() {
            
            object[] a = this.GetType().GetCustomAttributes( typeof( CollectionAttribute ) , true );

            if ( ( a != null ) && ( a.Length == 1 ) ) {
                return ( a[0] as CollectionAttribute ).CollectionName;
            } else {
                throw new Exception( "Collection attribute not found" );
            }

        }

        public static string CollectionName<T>() {

            object[] a = typeof(T).GetCustomAttributes( typeof( CollectionAttribute ) , true );

            if ( ( a != null ) && ( a.Length == 1 ) ) {
                return ( a[0] as CollectionAttribute ).CollectionName;
            } else {
                throw new Exception( "Collection attribute not found" );
            }
            

        }

    }

}
