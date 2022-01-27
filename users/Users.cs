using AfterhoursGameTest;
using AfterhoursGameTest.BusinessLogic;
using AfterhoursGameTest.Request;
using AfterhoursGameTest.Storage;
using AfterhoursGameTestLibrary;
using AfterhoursGameTestLibrary.DatabaseModel;
using AfterhoursGameTestLibrary.HttpHelper;
using GenericDatabase.CollectionAdapter;
using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace users {


    [FunctionsStartup( typeof( Startup ) )]
    public class Users : IHttpFunction {



        private readonly ILogger mLogger;
        private readonly FirebaseDatabase db;
        private readonly IStorageManager mStorage;
        //private IAuthenticateManager authenticateManager;

        public Users( ILogger<Users> mLogger , INoSqlDatabase db , IStorageManager storage ) {
            this.mLogger = mLogger;
            this.db = ( FirebaseDatabase ) db;
            db.Logger = this.mLogger;
            this.mStorage = storage;

        }

        public bool Validate( UserRequest request , out string message ) {

            message = string.Empty;
            if ( ( request != null ) && ( !Validator.isNotNullOrEmpty( request.ID ) ) ) {
                message = string.Format( "{0} must be not null or empty" , nameof( request.ID ) );
                return false;
            } else {
                return true; ;
            }

        }

        public bool Validate( CreateUserRequest request , out string message ) {

            message = string.Empty;

            if ( ( request != null ) && ( !Validator.isNotNullOrEmpty( request.Email ) ) ) {
                message = string.Format( "{0} must be not null or empty" , nameof( request.Email ) );
                return false;
            }

            if ( !Regex.IsMatch( request.Email , @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z" , RegexOptions.IgnoreCase ) ) {
                message = string.Format( "{0} must be not a valid email" , nameof( request.Email ) );
                return false;
            } else {
                return true;
            }

        }


        /// <summary>
        /// Logic for your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>    
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync( HttpContext context ) {

            try {

                //mLogger.LogInformation( context.Request.Path );
                //mLogger.LogInformation( context.Request.Method );


                if ( context.IsOPTIONS() ) {
                    context.EnableCORS( "*" , "GET,POST,PUT,DELETE" , "content-type" );
                    context.NoContent();
                    return;
                }



                if ( db.OpenConnection() ) {

                    if ( context.CheckRoute( RequestMethod.GET , "/users" ) ) {

                        var limit = 10;
                        var index = 0;
                        var offset = 0;
                        if ( context.Request.Query != null ) {
                            if ( context.Request.Query.ContainsKey( "pageIndex" ) ) {
                                if ( !int.TryParse( context.Request.Query["pageIndex"] , out index ) ) {
                                    index = 0;
                                }
                            }

                            if ( context.Request.Query.ContainsKey( "pageSize" ) ) {
                                if ( !int.TryParse( context.Request.Query["pageSize"] , out limit ) ) {
                                    limit = 10;
                                }

                            }

                            offset = ( index * limit );
                        }

                        var c = await GetAllUsers( context , offset , limit );
                        return;

                    } else if ( context.CheckRoute( RequestMethod.GET , "/users/{id}" , out string id ) ) {

                        await GetUser( context , id );
                        return;
                    } else if ( context.CheckRoute( RequestMethod.POST , "/users/{id}/profile" , out id ) ) {

                        await UploadProfile( context , id );
                        return;

                    } else if ( context.CheckRoute( RequestMethod.GET , "/users/{id}/profile" , out id ) ) {
                        await GetProfile( context , id );
                        return;

                    } else if ( context.CheckRoute( RequestMethod.POST , "/users" ) ) {

                        await CreateUser( context );
                        return;

                    } else if ( context.CheckRoute( RequestMethod.PUT , "/users/{id}" , out id ) ) {

                        await UpdateUser( context , id );
                        return;

                    } else if ( context.CheckRoute( RequestMethod.DELETE , "/users/{id}" , out id ) ) {

                        await DeleteUser( context , id );
                        return;

                    } else {

                        context.BadMethod();
                        return;
                    }

                }

            } catch ( Exception e ) {

                mLogger.LogError( e.Message );
                await context.InternalError( "Internal error" );
            }
        }

        private async Task GetProfile( HttpContext context , string id ) {

            var u = new BizLogicUser( id , db , mLogger );
            await u.Initialization;

            var list = await u.GetProfile<Profile>();

            if ( ( list != null ) && ( list.Count >= 1 ) ) {
                var profile = list.FirstOrDefault();
                await context.OK( profile , true );
                return;

            } else {

                await context.NotFound( "Profile not found" );
                return;
            }

        }

        private async Task UploadProfile( HttpContext context , string id ) {

            var formData = await context.Request.ReadFormAsync();
            var file = context.Request.Form.Files["file"];
            if ( file != null ) {

                var extension = System.IO.Path.GetExtension( file.FileName ); ;

                var u = new BizLogicUser( id , db , mLogger );
                await u.Initialization;


                bool isValidFolder;
                if ( !mStorage.Exists( $"{id}/" ) ) {
                    isValidFolder = mStorage.NewFolder( $"{id}/" );
                } else {
                    isValidFolder = true;
                }

                if ( !isValidFolder ) {
                    await context.InternalError( "Folder not exits" );
                    return;
                }

                using ( System.IO.Stream fs = file.OpenReadStream() ) {

                    if ( !fs.IsValidImageFile() ) {
                        await context.BadRequest( "Invalid image format." );
                        return;

                    }

                    string Bucketfilename = $"{id}/profile{extension}";

                    if ( !mStorage.Upload( fs , Bucketfilename ) ) {

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
                            await context.InternalError( "Can't save profile" );
                            return;

                        }


                    } else {
                        await context.InternalError( "Can't save profile" );
                        return;

                    }


                }


            } else {
                await context.BadRequest( "Invalid file parameter" );
                return;

            }
        }

        private async Task CreateUser( HttpContext context ) {
            var request = await HttpRequestMap.ToClassAsync<CreateUserRequest>( context.Request );

            if ( !Validate( request , out string errorMessage ) ) {
                await context.BadRequest( errorMessage );
                return;
            }

            User user = new User() {
                Email = request.Email ,
                FirstName = request.FirstName ,
                LastName = request.LastName ,
                Nickname = request.Nickname
            };

            var u = new BizLogicUser( user , db );

            if ( await u.Exists( request.Email ) > 0 ) {
                await context.BadRequest( "User already exists" );
                return;
            }

            if ( await u.Save() ) {

                await context.OK( user , true );
                return;
            } else {

                await context.BadRequest( "Data not saved" );
                return;
            }
        }

        private async Task DeleteUser( HttpContext context , string id ) {

            if ( Validate( new UserRequest() { ID = id } , out string message ) ) {

                var u = new BizLogicUser( id , db );
                await u.Initialization;
                if ( u.User != null ) {
                    if ( mStorage.Exists( $"{u.User.ID}/" ) ) {
                        mStorage.DeleteFolder( $"{u.User.ID}" );
                    }

                    if ( await u.Delete<Profile>() ) {

                        await context.OK( u.User , true );
                        return;
                    } else {

                        await context.BadRequest( "Data not saved" );
                        return;
                    }

                } else {
                    await context.InternalError( "User not found" );
                    return;

                }

            } else {

                await context.BadRequest( message );
                return;
            }
        }


        private async Task UpdateUser( HttpContext context , string id ) {

            if ( Validate( new UserRequest() { ID = id } , out string message ) ) {

                var request = await HttpRequestMap.ToClassAsync<UpdateUserRequest>( context.Request );

                var userDocument = new UserDocument( db );

                var user = await userDocument.Read( id );

                if ( user == null ) {

                    await context.InternalError( "Error reading user" );
                    return;
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Nickname = request.Nickname;


                var u = new BizLogicUser( user , db );

                if ( await u.Save() ) {

                    await context.OK( user , true );
                    return;
                } else {

                    await context.BadRequest( "Data not saved" );
                    return;
                }

            } else {

                await context.BadRequest( message );
                return;
            }
        }

        private async Task GetUser( HttpContext context , string id ) {


            if ( Validate( new UserRequest() { ID = id } , out string message ) ) {

                var userDocument = new UserDocument( db );

                var user = await userDocument.Read( id );
                if ( user == null ) {
                    await context.InternalError( "Error reading user" );
                    return;
                }

                await context.OK( user , true );
                return;

            } else {
                await context.BadRequest( message );
                return;
            }
        }

        private async Task<int> GetAllUsers( HttpContext context , int? offset = null , int? limit = null ) {

            var u = new CollectionAdapter<User>( db );

            var count = await u.Count();
            var list = await u.Select( offset , limit );

            //context.Response.Headers.Append( "Access-Control-Expose-Headers" , "X-Pagination" );
            context.Response.Headers.Append( "X-Pagination" , count.ToString() );

            await context.OK( list , true );

            return count;

        }

    }

}
