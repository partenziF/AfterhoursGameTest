using AfterhoursGameTest.Storage;
using AfterhoursGameTestLibrary.Authorization;
using GenericDatabase.Database;
using GenericDatabase.Database.Interface;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using static AfterhoursGameTestLibrary.Startup;

namespace AfterhoursGameTestLibrary {

    public class Startup : FunctionsStartup {
        

        public sealed class FirestoreDb : FirebaseDatabase {
            public FirestoreDb() : base( "afterhours-b8f4b" ) {
            }
        }

        public sealed class Bucket : StorageManager {
            public Bucket( ) : base( "afterhoursgame" ) {
            }
        }


        public override void ConfigureServices( WebHostBuilderContext context , IServiceCollection services ) {
            Environment.SetEnvironmentVariable( "GOOGLE_APPLICATION_CREDENTIALS" , "afterhours-b8f4b-2adeec179317.json" );
            services.AddSingleton<INoSqlDatabase , FirestoreDb>();
            services.AddSingleton<IAuthenticateManager , AuthenticateManager>();
            services.AddSingleton<IStorageManager,Bucket>();

        }

    }

    public interface IFactory<T> {
    }
}
