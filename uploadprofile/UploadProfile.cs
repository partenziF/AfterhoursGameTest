using AfterhoursGameTest;
using AfterhoursGameTest.BusinessLogic;
using AfterhoursGameTest.Request;
using AfterhoursGameTest.Storage;
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

namespace uploadprofile {

    [FunctionsStartup( typeof( Startup ) )]
    public class UploadProfile : IHttpFunction {

        private readonly ILogger mLogger;
        private readonly FirebaseDatabase db;
        private readonly IAuthenticateManager authenticateManager;
        private readonly IStorageManager mStorage;

        public UploadProfile( ILogger<UploadProfile> mLogger , INoSqlDatabase db , IStorageManager storage , IAuthenticateManager auth ) {
            this.mLogger = mLogger;
            this.db = ( FirebaseDatabase ) db;
            db.Logger = this.mLogger;
            this.authenticateManager = auth;
            this.mStorage = storage;
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
        /// Logic for    your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync( HttpContext context ) {

            try {

                if ( context.IsOPTIONS() ) {
                    context.EnableCORS( "*" , "POST" , "content-type" );
                    context.NoContent();
                    return;
                }

                if ( context.IsPOST() ) {

                    IFormFile file = null;

                    try {
                        
                        //First read file
                        var formData = await context.Request.ReadFormAsync();
                        file = context.Request.Form.Files["file"];

                        var request = await HttpRequestMap.ToClassAsync<AuthorizationRequest>( context.Request );

                        if ( !Validate( request , out string errorMessage ) ) {
                            this.mLogger.LogWarning( errorMessage );
                            await context.BadRequest( errorMessage );
                            return;
                        }

                        var u = new BizLogicUser( db , mLogger );

                        switch ( await u.IsLogged( request.AuthToken , authenticateManager ) ) {

                            case TokenStatus.Verified:

                            if ( file != null ) {

                                var extension = System.IO.Path.GetExtension( file.FileName );

                                bool isValidFolder = false;
                                var id = u.User.ID;

                                if ( !mStorage.Exists( $"{id}/" ) ) {
                                    isValidFolder = mStorage.NewFolder( $"{id}/" );
                                } else {
                                    isValidFolder = true;
                                }

                                if ( !isValidFolder ) {
                                    this.mLogger.LogWarning( "Folder not exits" );
                                    await context.InternalError( "Folder not exits" );
                                    return;
                                }

                                using ( System.IO.Stream fs = file.OpenReadStream() ) {

                                    if ( !fs.IsValidImageFile() ) {
                                        this.mLogger.LogWarning( "Invalid image format." );
                                        await context.BadRequest( "Invalid image format." );
                                        return;

                                    }

                                    string Bucketfilename = $"{id}/profile{extension}";

                                    if ( !mStorage.Upload( fs , Bucketfilename ) ) {

                                        this.mLogger.LogWarning( "Can't upload file" );
                                        await context.BadRequest( "Can't upload file" );
                                        return;

                                    }

                                    var mediaUrl = mStorage.GetUrl( Bucketfilename );

                                    if ( !String.IsNullOrWhiteSpace( mediaUrl ) ) {

                                        var profile = new Profile() { Filename = file.FileName , ByteSize = file.Length , Url = mediaUrl };
                                        var r = await u.CreateProfile( profile );

                                        if ( r != null ) {

                                            await context.OK( profile , true );
                                            return;

                                        } else {
                                            this.mLogger.LogWarning( "Can't save profile" );
                                            await context.InternalError( "Can't save profile" );
                                            return;

                                        }


                                    } else {
                                        this.mLogger.LogWarning( "Can't save profile" );
                                        await context.InternalError( "Can't save profile" );
                                        return;

                                    }


                                }


                            } else {
                                this.mLogger.LogWarning( "Invalid file parameter" );
                                await context.BadRequest( "Invalid file parameter" );
                                return;

                            }



                            case TokenStatus.Revoked:
                            this.mLogger.LogWarning( "Token is revoked, do a login" );
                            await context.BadRequest( "Token is revoked, do a login" );
                            break;
                            case TokenStatus.Invalid:
                            this.mLogger.LogWarning( "Invalid token" );
                            await context.Forbidden( "Invalid token" );
                            break;

                        }

                    } catch ( Exception e ) {
                        mLogger?.LogError( "Exception {0}" , e.Message );
                        await context.InternalError( string.Format( "Exception {0}" , e.Message ) );
                        return;
                    }

                } else {
                    mLogger?.LogError( "Method not allowed" );
                    context.BadMethod();
                    return;
                }


            } catch ( Exception e ) {
                await context.InternalError( "Internal Error" );
                mLogger.LogError( e.Message );
                return;

            }

        }

    }
}
