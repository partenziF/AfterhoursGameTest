using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenericDatabase.DocumentAdapter {
    public interface IDocumentAdapter<T> where T : Document.Document {

        public Task<T> Create( T document );
        public Task<T> Read( string id );
        public Task<bool> Update( T document );
        public Task<bool> Delete( T document );

    }

}
