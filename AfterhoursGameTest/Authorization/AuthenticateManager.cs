using FirebaseAdmin;
using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AfterhoursGameTestLibrary.Authorization {

    public class AuthenticateManager : IAuthenticateManager {
        private FirebaseToken decodedToken;

        public dynamic Token => ( FirebaseToken ) decodedToken;

        public AuthenticateManager() {
            FirebaseApp.Create();
            this.decodedToken = null;
        }

        public async Task<string> CreateToken( string value ) {
            this.decodedToken = null;
            try {
                return await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync( value );
            } catch {
                throw;
            }
        }

        public string RefreshToken( string value ) {
            throw new NotImplementedException();
        }

        public Task RevokeToken( string value ) {
            throw new NotImplementedException();
        }

        public async Task<TokenStatus> VerifyToken( string idToken ) {
            try {
                this.decodedToken = null;
                bool checkRevoked = true;
                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync( idToken , checkRevoked );
                return TokenStatus.Verified;

            } catch ( FirebaseAuthException ex ) {
                if ( ex.AuthErrorCode == AuthErrorCode.RevokedIdToken ) {
                    // Token has been revoked. Inform the user to re-authenticate or signOut() the user.
                    return TokenStatus.Revoked;
                } else {
                    return TokenStatus.Invalid;
                }
            }
        }

    }


}
