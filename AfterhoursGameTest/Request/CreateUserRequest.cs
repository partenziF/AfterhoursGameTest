using System;
using System.Collections.Generic;
using System.Text;

namespace AfterhoursGameTest.Request {
    public class CreateUserRequest {
        public string Email { get; set; }

        public string AuthToken { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Nickname { get; set; }


    }
}
