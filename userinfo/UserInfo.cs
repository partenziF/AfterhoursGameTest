using AfterhoursGameTest.BusinessLogic;
using AfterhoursGameTest.Request;
using AfterhoursGameTestLibrary;
using AfterhoursGameTestLibrary.Authorization;
using AfterhoursGameTestLibrary.HttpHelper;
using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace userinfo {

    [FunctionsStartup( typeof( Startup ) )]
    public class UserInfo : IHttpFunction {

        private readonly ILogger mLogger;
        private readonly FirebaseDatabase db;
        private readonly IAuthenticateManager authenticateManager;

        public UserInfo( ILogger<UserInfo> mLogger , INoSqlDatabase db , IAuthenticateManager auth ) {
            this.mLogger = mLogger;
            this.db = ( FirebaseDatabase ) db;
            db.Logger = this.mLogger;
            this.authenticateManager = auth;
        }

        public bool Validate( AuthorizationRequest request , out string message ) {
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
            
            if ( context.IsGET() ) {
            
                var request = await HttpRequestMap.ToClassAsync<AuthorizationRequest>( context.Request );

                if ( !Validate( request , out string errorMessage ) ) {
                    await context.BadRequest( errorMessage );
                    return;
                }

                try {

                    var u = new BizLogicUser( db , mLogger );

                    switch ( await u.IsLogged( request.AuthToken , authenticateManager ) ) {
                        case TokenStatus.Verified:
                        await context.OK( u.User,true );
                        return;
                        case TokenStatus.Revoked:
                        await context.BadRequest( "Token is revoked, do a login" );
                        break;
                        case TokenStatus.Invalid:
                        await context.Forbidden( "Invalid token" );
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
