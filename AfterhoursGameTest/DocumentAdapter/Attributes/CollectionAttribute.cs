using System;
using System.Collections.Generic;
using System.Text;

namespace GenericDatabase.EntityAdapter.Attributes {

    [AttributeUsage( AttributeTargets.Class , AllowMultiple = false , Inherited = false )]

    public sealed class CollectionAttribute: Attribute {
        readonly string mCollectionName;
        public string CollectionName { get => mCollectionName; }

        public CollectionAttribute( string collectionName ) {

            if ( string.IsNullOrWhiteSpace( collectionName ) ) {
                throw new ArgumentException( $"'{nameof( collectionName )}' non può essere Null o uno spazio vuoto." , nameof( collectionName ) );
            }

            this.mCollectionName = collectionName;
        }


    }

}
