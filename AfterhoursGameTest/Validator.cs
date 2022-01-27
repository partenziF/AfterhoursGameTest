using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AfterhoursGameTestLibrary {
    public static class Validator {
        
        private static string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

        


        public static bool isEmail(string value ) {
            if ( value != null ) {
                var r = new Regex( validEmailPattern , RegexOptions.IgnoreCase );
                return r.IsMatch( value.Trim() );
            } else {
                return false;
            }
        }

        public static bool isNotNullOrEmpty(string value ) {
            return !string.IsNullOrEmpty( value );
        }

        


    }
}
