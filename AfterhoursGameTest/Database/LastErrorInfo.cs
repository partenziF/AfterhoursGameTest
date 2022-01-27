using GenericDatabase.Database.Interface;

namespace GenericDatabase.Database {
    public struct LastErrorInfo : ILastErrorInfo {

        public string mMessage;
        public LastErrorInfo( string message ) {
            mMessage = message;
        }

        public string Message { get => mMessage; set => mMessage = value; }
    }

    /*
    public class NoSqlDatabase : INoSqlDatabase {

        private string mProjectId;
        private bool mIsConnected;
        private FirestoreDb db;

        private ILogger mLogger;

        private CollectionReference referenceCollection;
        private DocumentReference referenceDocument;


        public NoSqlDatabase( string projectId , ILogger logger = null ) {
            mIsConnected = false;
            mProjectId = projectId;
            mLastErrorInfo = new LastErrorInfo();
            mLogger = logger;
        }

        private ILastErrorInfo mLastErrorInfo;
        public ILastErrorInfo LastError { get => this.mLastErrorInfo; set { this.mLastErrorInfo = null; this.mLastErrorInfo = value; } }
        public bool IsConnected { get => mIsConnected; }
        public dynamic Collection => referenceCollection;
        public ILogger Logger { set => mLogger = value; }

        public string ProjectId => mProjectId;

        public dynamic Document => referenceDocument;

        public bool OpenConnection() {

            mLogger?.LogTrace( "{0} {1}" , nameof( OpenConnection ) , mIsConnected );

            if ( !mIsConnected ) {

                try {

                    db = FirestoreDb.Create( mProjectId );
                    mIsConnected = true;
                    mLogger?.LogTrace( "{0} {1}" , nameof( OpenConnection ) , mIsConnected );
                    return true;

                } catch ( Exception e ) {
                    mLogger?.LogError( "{0} {1}" , nameof( OpenConnection ) , e.Message );
                    mIsConnected = false;
                    LastError = new LastErrorInfo( e.Message );
                    return false;
                }

            } else {
                return true;
            }
        }

        public bool CloseConnection() {
            mLogger?.LogTrace( "{0} {1}" , nameof( CloseConnection ) , mIsConnected );
            if ( this.IsConnected ) {
                this.mIsConnected = false;
                return true;
            } else {
                return false;
            }
        }

        public bool OpenCollection( string name ) {
            if ( IsConnected ) {
                try {
                    mLogger?.LogTrace( "{0} {1}" , nameof( OpenCollection ) , mIsConnected );
                    referenceCollection = db.Collection( name );
                    return ( referenceCollection != null );
                } catch {
                    referenceCollection = null;
                    throw;
                }
            } else {
                referenceCollection = null;
                return false;
            }
        }

        public async Task<T> OpenDocument<T>( string id ) where T : Document.Document {

            if ( IsConnected ) {
                try {

                    mLogger?.LogTrace( "{0} {1}" , nameof( OpenDocument ) , id );
                    referenceDocument = referenceCollection?.Document( id ) ?? null;
                    if ( referenceDocument != null ) {

                        var snapshot = await referenceDocument.GetSnapshotAsync();

                        if ( ( snapshot != null ) && ( snapshot.Exists ) ) {
                            T result = snapshot.ConvertTo<T>();
                            result.ID = snapshot.Id;
                            return result;
                        } else {
                            return null;
                        }

                    } else {
                        mLogger?.LogTrace( "Unable to open document {0}" , id );
                        throw new Exception( "Unable to open document" );
                    }
                } catch {
                    referenceDocument = null;
                    throw;
                }

            } else {
                referenceDocument = null;
                mLogger?.LogTrace( "Connection is closed" );
                throw new Exception( "Connection is closed" );
            }
        }

    }
    */
}
