using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AfterhoursGameTestLibrary.Authorization {

    public enum TokenStatus {
        Verified,
        Revoked,
        Invalid

    }

    public interface IAuthenticateManager {


        public dynamic Token { get; }
        public Task<string> CreateToken( string value );

        public Task<TokenStatus> VerifyToken( string idToken );
        public string RefreshToken( string value );
        public Task RevokeToken( string value );
    }
}
