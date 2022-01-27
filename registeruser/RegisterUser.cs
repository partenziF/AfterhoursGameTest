using AfterhoursGameTest.BusinessLogic;
using AfterhoursGameTest.Request;
using AfterhoursGameTestLibrary;
using AfterhoursGameTestLibrary.Authorization;
using AfterhoursGameTestLibrary.DatabaseModel;
using AfterhoursGameTestLibrary.HttpHelper;
using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace registeruser {

    [FunctionsStartup( typeof( Startup ) )]
    public class RegisterUser : IHttpFunction {

        private readonly ILogger mLogger;
        private readonly FirebaseDatabase db;
        private readonly IAuthenticateManager authenticateManager;

        public RegisterUser( ILogger<RegisterUser> mLogger , INoSqlDatabase db , IAuthenticateManager auth ) {
            this.mLogger = mLogger;
            this.db = ( FirebaseDatabase ) db;
            db.Logger = this.mLogger;
            this.authenticateManager = auth;
        }


        public bool Validate( RegisterUserRequest request , out string message ) {
            message = string.Empty;
            if ( ( request != null ) && ( !Validator.isNotNullOrEmpty( request.AuthToken ) ) ) {
                message = string.Format( "{0} must be not null or empty" , nameof( request.AuthToken ) );
                return false;
            } else {
                return true; ;
            }

        }


        /// <summary>
        /// Logic for your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync( HttpContext context ) {


            if ( context.IsOPTIONS() ) {
                context.EnableCORS( "*" , "GET,POST,PUT,DELETE" , "content-type" );
                context.NoContent();
                return;
            }

            if ( context.IsPOST() ) {

                var request = await HttpRequestMap.ToClassAsync<RegisterUserRequest>( context.Request );

                if ( !Validate( request , out string errorMessage ) ) {
                    await context.BadRequest( errorMessage );
                    return;
                }

                try {

                    var u = new BizLogicUser( db , mLogger );
                    
                    switch ( await u.IsLogged( request.AuthToken , authenticateManager ) ) {

                        case TokenStatus.Verified:

                        if (await u.Update( new User() {  FirstName = request.FirstName, LastName = request.LastName, Nickname = request.Nickname } ) ) {
                            await context.OK( u.User,true );
                        } else {
                            await context.InternalError( "Data not changed" );
                        }

                        return;
                        case TokenStatus.Revoked:
                        await context.BadRequest( "Token is revoked, do a login" );
                        break;
                        case TokenStatus.Invalid:
                        await context.BadRequest( "Invalid token" );
                        break;

                    }

                } catch ( Exception e ) {
                    mLogger?.LogError( "Exception {0}" , e.Message );
                    await context.InternalError( string.Format( "Exception {0}" , e.Message ) );
                    return;
                }

            } else {
                mLogger?.LogInformation( "Method not allowed" );
                context.BadMethod();
                return;
            }
        }
    }
}
