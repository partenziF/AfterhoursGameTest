using AfterhoursGameTestLibrary.HttpHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AfterhoursGameTestLibrary {
    public static class HttpRequestMap {

        public static object GetDefault( Type t ) {
            Func<object> f = GetDefault<object>;
            return f.Method.GetGenericMethodDefinition().MakeGenericMethod( t ).Invoke( null , null );
        }

        private static T GetDefault<T>() {
            return default( T );
        }
        public static async System.Threading.Tasks.Task<T> ToClassAsync<T>( HttpRequest request , ILogger logger = null ) where T : class {

            try {
                JsonElement json = JsonSerializer.Deserialize<JsonElement>( "{}" );

                if ( ( request.Body != null ) && ( request.Method.ToUpper() != nameof( RequestMethod.GET ) ) ) {
                    using TextReader reader = new StreamReader( request.Body );
                    //
                    if ( reader != null ) {
                        string text = await reader.ReadToEndAsync();
                        if ( ( request.ContentType?.ToLower() == "application/json" ) && ( !String.IsNullOrWhiteSpace( text ) ) ) {
                            try {
                                json = JsonSerializer.Deserialize<JsonElement>( text );
                            } catch ( JsonException parseException ) {
                                logger?.LogError( parseException , "Error parsing JSON request" );
                            }
                        }
                    }
                }

                var result = Activator.CreateInstance<T>();

                foreach ( var p in typeof( T ).GetProperties() ) {
                    if ( request.Query.ContainsKey( p.Name ) ) {
                        if ( p.PropertyType == typeof( string ) ) {
                            p.SetValue( result , request.Query[p.Name].ToString() );
                        } else {
                            throw new NotImplementedException();
                        }
                    } else if ( ( request.Method.ToUpper() != nameof( RequestMethod.GET ) ) && ( json.TryGetProperty( p.Name , out JsonElement jsonElement ) ) ) {
                        switch ( jsonElement.ValueKind ) {
                            case JsonValueKind.Undefined:
                            throw new NotImplementedException( "Not implemented" );

                            case JsonValueKind.Object:
                            throw new NotImplementedException( "Not implemented" );

                            case JsonValueKind.Array:
                            throw new NotImplementedException( "Not implemented" );

                            case JsonValueKind.String:
                            if ( p.PropertyType == typeof( string ) ) {
                                p.SetValue( result , jsonElement.GetString() );
                            } else if ( p.PropertyType == typeof( char ) ) {
                                p.SetValue( result , jsonElement.GetString() );
                            } else {
                                throw new Exception( string.Format( "Invalid value type for {0}" , p.Name ) );
                            }
                            break;
                            case JsonValueKind.Number:
                            if ( p.PropertyType == typeof( byte ) ) {
                                p.SetValue( result , jsonElement.GetByte() );
                            } else if ( p.PropertyType == typeof( sbyte ) ) {
                                p.SetValue( result , jsonElement.GetSByte() );
                            } else if ( p.PropertyType == typeof( Int16 ) ) {
                                p.SetValue( result , jsonElement.GetInt32() );
                            } else if ( p.PropertyType == typeof( Int32 ) ) {
                                p.SetValue( result , jsonElement.GetInt32() );
                            } else if ( p.PropertyType == typeof( Int64 ) ) {
                                p.SetValue( result , jsonElement.GetInt64() );
                            } else if ( p.PropertyType == typeof( UInt16 ) ) {
                                p.SetValue( result , jsonElement.GetUInt16() );
                            } else if ( p.PropertyType == typeof( UInt32 ) ) {
                                p.SetValue( result , jsonElement.GetUInt32() );
                            } else if ( p.PropertyType == typeof( UInt64 ) ) {
                                p.SetValue( result , jsonElement.GetUInt64() );
                            } else if ( p.PropertyType == typeof( decimal ) ) {
                                p.SetValue( result , jsonElement.GetDecimal() );
                            } else if ( p.PropertyType == typeof( float ) ) {
                                p.SetValue( result , jsonElement.GetSingle() );
                            } else if ( p.PropertyType == typeof( double ) ) {
                                p.SetValue( result , jsonElement.GetDouble() );
                            } else {
                                throw new Exception( string.Format( "Invalid value type for {0}" , p.Name ) );
                            }

                            break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                            if ( p.PropertyType == typeof( Boolean ) ) {
                                p.SetValue( result , jsonElement.GetBoolean() );
                            } else {
                                throw new Exception( string.Format( "Invalid value type for {0}" , p.Name ) );
                            }
                            break;
                            case JsonValueKind.Null:
                            if ( ( p.PropertyType.IsGenericType ) && ( p.PropertyType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) ) {
                                p.SetValue( result , null );
                            } else if ( p.PropertyType == typeof( string ) ) {
                                p.SetValue( result , null );
                            } else {
                                throw new Exception( string.Format( "Invalid value type for {0}" , p.Name ) );
                            }

                            break;
                        }

                    } else {
                        p.SetValue( result , GetDefault( p.PropertyType ) );
                    }

                }


                return result;

            } catch ( Exception e ) {
                logger.LogError( "Error mapping class {0}" , e.Message );
                throw new Exception( string.Format( "Error mapping class {0}" , e.Message ) );
            }


        }
    }
}
