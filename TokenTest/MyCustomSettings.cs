using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenTest
{
    public class MyCustomSettings : ApplicationSettingsBase
    {

        [UserScopedSetting()]
        [DefaultSettingValue(@"https://identityserver.argip.com.pl/connect/token")]
        public string TokenEndpoint
        {
            get
            {
                return ((string)this["TokenEndpoint"]);
            }
            set
            {
                this["TokenEndpoint"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string ClientId
        {
            get
            {
                return ((string)this["ClientId"]);
            }
            set
            {
                this["ClientId"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string ClientSecret
        {
            get
            {
                return ((string)this["ClientSecret"]);
            }
            set
            {
                this["ClientSecret"] = value;
            }
        }
    }
}
