using AfterhoursGameTest.Configuration;
using AfterhoursGameTest.Storage;
using AfterhoursGameTestLibrary.Authorization;
using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using static AfterhoursGameTestLibrary.Startup;

namespace AfterhoursGameTestLibrary {



    public class Startup : FunctionsStartup {
        private GeneralConfiguration mGeneralConfiguration;
        public GeneralConfiguration generalConfiguration { get => mGeneralConfiguration; }

        //public sealed class FirestoreDb : FirebaseDatabase {
        //    public FirestoreDb() : base( "afterhours-b8f4b" ) {
        //    }
        //}

        //public sealed class Bucket : StorageManager {
        //    public Bucket() : base( "afterhoursgame" ) {
        //    }
        //}


        public override void ConfigureServices( WebHostBuilderContext context , IServiceCollection services ) {


            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath( AppDomain.CurrentDomain.BaseDirectory ).AddJsonFile( "appsettings.json" ).Build();

            var section = config?.GetSection( nameof( GeneralConfiguration ) );
            if ( section == null ) {
                throw new ArgumentException( "General section is not defined in appsettings.json" );
            }

            mGeneralConfiguration = section?.Get<GeneralConfiguration>();

            if ( string.IsNullOrWhiteSpace( generalConfiguration?.GoogleApplicationCredentials ) ) {
                Environment.SetEnvironmentVariable( "GOOGLE_APPLICATION_CREDENTIALS" , generalConfiguration.GoogleApplicationCredentials );
            }

            if ( string.IsNullOrWhiteSpace( generalConfiguration?.Firestore ) ) {
                services.AddSingleton<INoSqlDatabase>( p => new FirebaseDatabase( generalConfiguration.Firestore ) );
            } else {
                throw new ArgumentException( "Firestore database not defined" );
            }
            services.AddSingleton<IAuthenticateManager , AuthenticateManager>();

            if ( string.IsNullOrWhiteSpace( generalConfiguration?.Firestore ) ) {
                services.AddSingleton<IStorageManager>( p => new StorageManager( generalConfiguration.BucketName ) );
            } else {
                throw new ArgumentException( "Bucket name not defined" );
            }

        }

    }

    public interface IFactory<T> {
    }
}
