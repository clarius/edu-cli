using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clarius.Edu.Graph
{
    public class ImportedUser
    {
        [Index(0)]
        public string Type { get; set; }
        [Index(1)]
        public string LastName { get; set; }
        [Index(2)]
        public string FirstName { get; set; }
        [Index(3)]
        public string Level { get; set; }
        [Index(4)]
        public string Grade { get; set; }
        [Index(5)]
        public string Division { get; set; }
        [Index(6)]
        public string NationalID { get; set; }
        [Index(7)]
        public string Username { get; set; }
        [Index(8)]
        public string Password { get; set; }
    }

    public class CsvTeamEntry
    {
        public string TeamName { get; set; }
        public string Grade { get; set; }
        public string Step1 { get; set; }
        public string Step2 { get; set; }
        public string Step3  { get; set; }
        public string Level { get; set; }
    }

    public class CsvUserTeamEntry
    {
        public string TeamName { get; set; }
        public string Grade { get; set; }
        public string Step1 { get; set; }
        public string Step2 { get; set; }
        public string Step3 { get; set; }
        public string UserId { get; set; }
    }
}
