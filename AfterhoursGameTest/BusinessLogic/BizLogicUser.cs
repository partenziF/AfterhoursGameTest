using AfterhoursGameTestLibrary.Authorization;
using AfterhoursGameTestLibrary.DatabaseModel;
using FirebaseAdmin.Auth;
using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using GenericDatabase.Document;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AfterhoursGameTest.BusinessLogic {
    public class BizLogicUser {

        private readonly ILogger mLogger;
        private readonly FirebaseDatabase db;
        private UserDocument userDocument;
        private User mUser;
        public User User { get => mUser; }

        public Task Initialization { get; private set; }


        public BizLogicUser( FirebaseDatabase db , ILogger logger = null ) {
            this.db = db;
            this.mLogger = logger;
            if ( !db.IsConnected ) db.OpenConnection();
            this.userDocument = new UserDocument( db );
            this.mUser = new User() { Email = null , FirstName = null , LastName = null , Nickname = null };
        }

        public BizLogicUser( User user , FirebaseDatabase db , ILogger logger = null ) {
            this.mUser = user;
            this.db = db;
            this.mLogger = logger;
            if ( !db.IsConnected ) db.OpenConnection();
            this.userDocument = new UserDocument( db );
        }

        private async Task Init( string ID ) {

            userDocument = new UserDocument( db );
            mUser = await userDocument.Read( ID );
        }

        public BizLogicUser( string ID , FirebaseDatabase db , ILogger logger = null ) {

            this.db = db;
            this.mLogger = logger;
            if ( !db.IsConnected ) db.OpenConnection();

            Initialization = Init( ID );
        }

        private async Task<string> GetAuthTokenAsync( string ID , IAuthenticateManager authenticateManager ) {

            return await authenticateManager.CreateToken( ID );

        }

        public async Task<TokenStatus> IsLogged( string token , IAuthenticateManager authenticateManager ) {

            mLogger?.LogInformation( String.Format( "Check login with token {0}" , token ) );
            if ( !db.IsConnected ) throw new Exception( "Database not connected" );

            try {

                var result = ( await authenticateManager.VerifyToken( token ) );

                if ( result == TokenStatus.Verified ) {

                    this.mUser = await this.userDocument.Read( ( ( FirebaseToken ) authenticateManager.Token ).Uid );

                    if ( this.mUser == null ) throw new Exception( "Can't open user" );

                }

                return result;

            } catch {
                throw;
            }

        }

        public async Task<int> Exists( string email ) {
            var c = new GenericDatabase.CollectionAdapter.CollectionAdapter<User>( db );
            var l = await c.Select( ( x => x.WhereEqualTo( nameof( User.Email ) , email ) ) );
            return ( l.Count  );
        }

        public async Task<string> LogInAsync( string email , IAuthenticateManager authenticateManager ) {

            mLogger?.LogInformation( String.Format( "LogIn with email {0}" , email ) );
            if ( !db.IsConnected ) throw new Exception( "Database not connected" );
            try {

                //var c = new GenericDatabase.CollectionAdapter.CollectionAdapter<User>( db );
                //var l = await c.Select( ( x => x.WhereEqualTo( nameof( User.Email ) , email ) ) );
                var count = await Exists( email );
                if ( count == 0 ) {
                    //if ( l.Count == 0 ) {
                    mLogger?.LogInformation( "User not exists, register new user" );
                    if ( await RegisterAsync( email ) ) {
                        return await GetAuthTokenAsync( mUser.ID , authenticateManager );
                    } else {
                        return null;
                    }

                } else if ( count == 1 ) {

                    var c = new GenericDatabase.CollectionAdapter.CollectionAdapter<User>( db );
                    var l = await c.Select( ( x => x.WhereEqualTo( nameof( User.Email ) , email ) ) );

                    if ( l.Count == 1 ) {
                        mLogger?.LogInformation( "User exists, read data" );
                        mUser.ID = l[0].ID;
                        mUser.Email = l[0].Email;
                        mUser.FirstName = l[0].FirstName;
                        mUser.LastName = l[0].LastName;
                        mUser.Nickname = l[0].Nickname;

                        return await GetAuthTokenAsync( mUser.ID , authenticateManager );
                    } else {
                        mLogger?.LogError( "Can't read unique email user {0}", email );
                        throw new Exception( "Can't read unique user" );
                    }

                } else {
                    mLogger?.LogError( "Invalid email" );
                    throw new Exception( "Invalid email" );

                }

            } catch {
                throw;
            }
        }

        public async Task<bool> RegisterAsync( string email ) {

            try {

                this.mUser.Email = email;
                return await Save();

            } catch {
                throw;
            }

        }

        public async Task<bool> Update( User newUser ) {
            try {
                mUser.FirstName = newUser.FirstName;
                mUser.LastName = newUser.LastName;
                mUser.Nickname = newUser.Nickname;

                return await Save();

            } catch {
                throw;
            }

        }

        public async Task<U> CreateProfile<U>( U document ) where U : Document {

            if ( document is null ) {
                throw new ArgumentNullException( nameof( document ) );
            }

            try {

                if ( await db.OpenDocument<User>( this.mUser.ID ) != null ) {


                    QuerySnapshot query = await db.Document.Collection( document.CollectionName() ).GetSnapshotAsync();
                    IReadOnlyList<DocumentSnapshot> documents = query.Documents;

                    foreach ( DocumentSnapshot doc in documents ) {
                        Console.WriteLine( "Deleting document {0}" , doc.Id );
                        await doc.Reference.DeleteAsync();
                    }


                    var c = db.Document.Collection( document.CollectionName() );

                    if ( c != null ) {
                        var doc = c.Document();
                        var r = await doc.CreateAsync( document );
                        document.ID = doc.Id;
                        return document;
                    }


                }


            } catch ( Exception e ) {

                throw new Exception( String.Format( "Error while read data {0}" , e.Message ) );
            }
            return null;

            //db.Document.Collection( "profile" ).Document().CreateAsync( profile );

        }

        public async Task<List<U>> GetProfile<U>() where U : Document {

            var result = new List<U>();

            try {

                if ( ( this.mUser != null ) && ( await db.OpenDocument<User>( this.mUser.ID ) != null ) ) {

                    QuerySnapshot query = await db.Document.Collection( Document.CollectionName<U>() ).GetSnapshotAsync();
                    IReadOnlyList<DocumentSnapshot> documents = query.Documents;

                    var c = db.Document.Collection( Document.CollectionName<U>() );

                    if ( c != null ) {

                        foreach ( DocumentSnapshot doc in documents ) {

                            result.Add( doc.ConvertTo<U>() );
                        }
                    }

                }


            } catch ( Exception e ) {

                throw new Exception( String.Format( "Error while read data {0}" , e.Message ) );
            }

            return result;

        }



        public async Task<bool> Save() {

            try {

                if ( string.IsNullOrEmpty( mUser.ID ) ) {

                    if ( await this.userDocument.Create( mUser ) ) {
                        return true;
                    } else {
                        mLogger?.LogError( "Error save data" );
                        return false;
                    }


                } else {

                    if ( await this.userDocument.Update( mUser ) ) {
                        return true;
                    } else {
                        mLogger?.LogError( "Error save data" );
                        return false;
                    }

                }

            } catch {
                throw;
            }

        }


        public async Task<bool> Delete<U>() where U : Document {

            try {

                if ( await db.OpenDocument<User>( this.mUser.ID ) != null ) {

                    QuerySnapshot query = await db.Document.Collection( Document.CollectionName<U>() ).GetSnapshotAsync();
                    IReadOnlyList<DocumentSnapshot> documents = query.Documents;

                    foreach ( DocumentSnapshot doc in documents ) {
                        Console.WriteLine( "Deleting document {0}" , doc.Id );
                        await doc.Reference.DeleteAsync();
                    }


                    if ( await this.userDocument.Delete( mUser ) ) {
                        return true;
                    } else {
                        mLogger?.LogError( "Error save data" );
                        return false;
                    }

                } else {
                    return false;
                }

            } catch {
                throw;
            }


        }

    }

}
