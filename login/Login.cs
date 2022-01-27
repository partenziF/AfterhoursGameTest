using AfterhoursGameTest;
using AfterhoursGameTest.BusinessLogic;
using AfterhoursGameTestLibrary;
using AfterhoursGameTestLibrary.Authorization;
using AfterhoursGameTestLibrary.DatabaseModel;
using AfterhoursGameTestLibrary.HttpHelper;
using AfterhoursGameTestLibrary.Request;
using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace login {

    public class LoginResponse {
        public string AuthToken { get; set; }
    }

    [FunctionsStartup( typeof( Startup ) )]
    public class Login : IHttpFunction {

        private readonly ILogger mLogger;
        private readonly FirebaseDatabase db;
        private readonly IAuthenticateManager authenticateManager;

        public Login( ILogger<Login> mLogger , INoSqlDatabase db , IAuthenticateManager auth ) {
            this.mLogger = mLogger;
            this.db = ( FirebaseDatabase ) db;
            db.Logger = this.mLogger;
            this.authenticateManager = auth;
        }

        public bool Validate( LoginRequest request , out string message ) {
            message = string.Empty;
            if ( ( request != null ) && ( !Validator.isEmail( request.Email ) ) ) {
                message = String.Format( "{0} is not valid email" , nameof( request.Email ) );
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

                try {

                    var loginRequest = await HttpRequestMap.ToClassAsync<LoginRequest>( context.Request );

                    if ( !Validate( loginRequest , out string errorMessage ) ) {
                        await context.BadRequest( errorMessage );
                        return;

                    }

                    var u = new BizLogicUser( db , mLogger );

                    var token = await u.LogInAsync( loginRequest.Email , this.authenticateManager );

                    if ( token != null ) {

                        await context.OK( new LoginResponse() { AuthToken = token } , true );
                        return;

                    } else {
                        await context.NotFound( string.Format( "Error can't login with {0}" , loginRequest.Email ) );
                        return;
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
