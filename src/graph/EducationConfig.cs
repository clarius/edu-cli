using System;
using System.Collections.Generic;
using System.Text;

namespace Clarius.Edu.Graph
{
    public class EducationConfig
    {
        public string Authority { set; get; }
        public string Directory { set; get; }
        public string Application { set; get; }
        public string ClientSecret { set; get; }
        public string DefaultDomainName { get; set; }
        public string UsageLocation { get; set; }
        public string PreferredLanguage { get; set; }
    }
}
