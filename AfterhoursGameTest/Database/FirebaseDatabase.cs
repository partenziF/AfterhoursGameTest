using GenericDatabase.Database.Interface;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GenericDatabase.Database {

    public class FirebaseDatabase : GenericNoSqlDatabase<CollectionReference , DocumentReference> {

        private readonly string mProjectId;
        private bool mIsConnected;
        private FirestoreDb db; 

        private ILogger mLogger;

        private ILastErrorInfo mLastErrorInfo;

        public FirebaseDatabase( string projectId , ILogger logger = null ) {
            mIsConnected = false;
            mProjectId = projectId;
            mLastErrorInfo = new LastErrorInfo();
            mLogger = logger;
        }

        public override ILogger Logger { set => mLogger = value; }

        public override bool IsConnected => mIsConnected;

        public override string ProjectId => mProjectId;

        public override ILastErrorInfo LastError { get => this.mLastErrorInfo; set { this.mLastErrorInfo = null; this.mLastErrorInfo = value; } }


        public override bool OpenConnection() {

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

        public override bool CloseConnection() {
            mLogger?.LogTrace( "{0} {1}" , nameof( CloseConnection ) , mIsConnected );
            if ( this.IsConnected ) {
                this.mIsConnected = false;
                return true;
            } else {
                return false;
            }
        }



        public override bool OpenCollection( string name ) {
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

        public override async Task<T> OpenDocument<T>( string id ) {

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
