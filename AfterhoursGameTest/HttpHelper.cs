using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AfterhoursGameTestLibrary.HttpHelper {

    public enum RequestMethod {
        GET,
        POST,
        PUT,
        DELETE,
        OPTIONS

    }

    public static class HttpHelper {

        public static string ReplaceAt( this string str , int index , int length , string replace ) {
            return str.Remove( index , Math.Min( length , str.Length - index ) )
                    .Insert( index , replace );
        }

        public static bool IsPOST( this HttpContext context ) {
            return ( context.Request.Method.ToUpper() == nameof( RequestMethod.POST ) );
        }
        public static bool IsGET( this HttpContext context ) {
            return ( context.Request.Method.ToUpper() == nameof( RequestMethod.GET ) );
        }
        public static bool IsOPTIONS( this HttpContext context ) {
            return ( context.Request.Method.ToUpper() == nameof( RequestMethod.OPTIONS ) );
        }

        public static void EnableCORS( this HttpContext context , string AllowOrigin = "*" , string AllowMethod = null , string ContentType = null ) {
            context.Response.Headers.Append( "Access-Control-Allow-Origin" , AllowOrigin );
            if ( AllowMethod != null ) {
                context.Response.Headers.Append( "Access-Control-Allow-Methods" , AllowMethod );

                if ( ContentType != null ) {
                    context.Response.Headers.Append( "Access-Control-Allow-Headers" , ContentType );

                }
            }

            context.Response.Headers.Append( "Access-Control-Expose-Headers" , "X-Pagination" );

        }

        public static bool CheckRoute( this HttpContext context , RequestMethod requestMethod , string action ) {

            if ( context.Request.Method.ToUpper() != requestMethod.ToString() ) return false;

            if ( context.Request.Path.StartsWithSegments( action , StringComparison.InvariantCultureIgnoreCase , out PathString remainingPath ) ) {

                if ( ( String.IsNullOrWhiteSpace( remainingPath.Value ) ) ) return true;

            } else {

                if ( ( context.Request.Path.HasValue ) ) {

                    return false;

                }

            }

            return false;


        }


        public static bool CheckRoute( this HttpContext context , RequestMethod requestMethod , string action , out string @param ) {

            param = null;

            if ( context.Request.Method.ToUpper() != requestMethod.ToString() ) return false;

            if ( context.Request.Path.StartsWithSegments( action , StringComparison.InvariantCultureIgnoreCase , out PathString remainingPath ) ) {

                if ( ( String.IsNullOrWhiteSpace( remainingPath.Value ) ) ) return false;

                if ( ( remainingPath != null ) && ( !String.IsNullOrWhiteSpace( remainingPath.Value ) ) ) {
                    Regex rx = new Regex( @"/(\w+)$" );
                    var matches = rx.Matches( remainingPath.Value );
                    if ( matches.Count > 0 ) {
                        @param = matches[0].Groups[matches[0].Groups.Count - 1].Value ?? null;
                        return true;
                    }

                }

            } else {

                if ( ( context.Request.Path.HasValue ) ) {

                    Regex rx1 = new Regex( @"/\{id\}" );
                    var matches1 = rx1.Matches( action );
                    var regExParam = "";
                    if ( matches1.Count > 0 ) {
                        if ( matches1[0].Success ) {
                            regExParam = action.ReplaceAt( matches1[0].Index , matches1[0].Length , @"/(\w+)" );
                            regExParam += "$";

                        }

                        Regex rx = new Regex( regExParam );
                        var matches = rx.Matches( context.Request.Path );
                        if ( matches.Count > 0 ) {
                            @param = matches[0].Groups[matches[0].Groups.Count - 1].Value ?? null;
                            return true;
                        }

                    }

                    //Regex rx = new Regex( @"/(\w+)" );
                    //var matches = rx.Matches( context.Request.Path );
                    //if ( matches.Count > 0 ) {
                    //    @param = matches[0].Groups[matches[0].Groups.Count - 1].Value ?? null;
                    //    return true;
                    //}

                }

            }

            return false;

        }

        public static void NoContent( this HttpContext context ) {

            try {
                context.Response.StatusCode = ( int ) HttpStatusCode.NoContent;
            } catch {
                throw;
            }
        }


        public static async Task OK( this HttpContext context , object message , bool enableCORS = false ) {

            try {
                if ( enableCORS )
                    context.EnableCORS();

                context.Response.StatusCode = ( int ) HttpStatusCode.OK;
                await context.Response.WriteAsync( JsonSerializer.Serialize( message ) );

            } catch {
                throw;
            }
        }

        public static async Task OK( this HttpContext context , string message ) {
            context.EnableCORS();
            context.Response.StatusCode = ( int ) HttpStatusCode.OK;
            await context.Response.WriteAsync( message );
        }

        public static async Task NotFound( this HttpContext context , string message ) {
            context.EnableCORS();
            context.Response.StatusCode = ( int ) HttpStatusCode.NotFound;
            await context.Response.WriteAsync( message );
        }


        public static async Task BadRequest( this HttpContext context , string message ) {
            context.EnableCORS();
            context.Response.StatusCode = ( int ) HttpStatusCode.BadRequest;
            await context.Response.WriteAsync( message );
        }

        public static async Task Unauthorized( this HttpContext context , string message ) {
            context.EnableCORS();
            context.Response.StatusCode = ( int ) HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync( message );
        }

        public static async Task Forbidden( this HttpContext context , string message ) {
            context.EnableCORS();
            context.Response.StatusCode = ( int ) HttpStatusCode.Forbidden;
            await context.Response.WriteAsync( message );
        }

        public static async Task InternalError( this HttpContext context , string message ) {
            context.EnableCORS();
            context.Response.StatusCode = ( int ) HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync( string.Format( message ) );
        }

        public static void BadMethod( this HttpContext context ) {
            context.EnableCORS();
            context.Response.StatusCode = ( int ) HttpStatusCode.MethodNotAllowed;

        }

    }
}
