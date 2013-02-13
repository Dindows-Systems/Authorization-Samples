using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeSwank.Authentication.OpenID.Plugins.Extensions
{
    /// <summary>
    /// UserObject data fields that are made available with the SimpleRegistration extension.
    /// </summary>
    public static class SimpleRegistrationFields
    {
        /// <summary>
        /// Any UTF-8 string that the End User wants to use as a nickname.
        /// </summary>
        public static string Nickname
        {
            get
            {
                return "openid.sreg.nickname";
            }
        }
        /// <summary>
        /// The email address of the End User as specified in section 3.4.1 of [RFC2822] (Resnick, P., “Internet Message Format,” .).
        /// </summary>
        public static string Email
        {
            get
            {
                return "openid.sreg.email";
            }
        }
        /// <summary>
        /// UTF-8 string free text representation of the End User's full name.
        /// </summary>
        public static string FullName
        {
            get
            {
                return "openid.sreg.fullname";
            }
        }
        /// <summary>
        /// The End User's date of birth as YYYY-MM-DD. Any values whose representation uses fewer than the specified number of digits should be zero-padded. The length of this value MUST always be 10. If the End User user does not want to reveal any particular component of this value, it MUST be set to zero.
        /// For instance, if a End User wants to specify that his date of birth is in 1980, but not the month or day, the value returned SHALL be "1980-00-00".
        /// </summary>
        public static string DateOfBirth
        {
            get
            {
                return "openid.sreg.dob";
            }
        }
        /// <summary>
        /// The End User's gender, "M" for male, "F" for female.
        /// </summary>
        public static string Gender
        {
            get
            {
                return "openid.sreg.gender";
            }
        }
        /// <summary>
        /// UTF-8 string free text that SHOULD conform to the End User's country's postal system.
        /// </summary>
        public static string PostalCode
        {
            get
            {
                return "openid.sreg.postcode";
            }
        }
        /// <summary>
        /// The End User's country of residence as specified by ISO3166.
        /// </summary>
        public static string Country
        {
            get
            {
                return "openid.sreg.country";
            }
        }
        /// <summary>
        /// End User's preferred language as specified by ISO639.
        /// </summary>
        public static string Language
        {
            get
            {
                return "openid.sreg.language";
            }
        }
        /// <summary>
        /// ASCII string from TimeZone database
        /// For example, "Europe/Paris" or "America/Los_Angeles".
        /// </summary>
        public static string Timezone
        {
            get
            {
                return "openid.sreg.timezone";
            }
        }
    }

}
