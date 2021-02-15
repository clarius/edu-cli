using Microsoft.Graph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using ClosedXML.Excel;

namespace Clarius.Edu.CLI
{
    internal class ListUsersCommand : CommandBase
    {
        public override bool UseCache => true;
        public override bool RequiresUser => false;

        internal ListUsersCommand(string[] args) : base(args)
        {
        }

        async public override Task RunInternal()
        {
            Log.Logger = new LoggerConfiguration()
                                .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
                                .CreateLogger();

            var users = Client.GetUsers(Client.GetUsers(), base.Grade, base.Division, base.EnglishLevel, base.Level, base.Type);

            var list = new List<string>();

            bool excelFormat = string.Equals(Output, "excel", StringComparison.InvariantCultureIgnoreCase);
            if (excelFormat)
            {
                if (string.IsNullOrEmpty(base.OutputFile))
                {
                    Log.Logger.Warning("Output filename has not been specified using /outputfile, value 'default.xlsx' will be used instead");
                }
            }

            int count = 0;
            foreach (var user in users)
            {
                if (RawFormat)
                {
                    list.Add($"{Client.RemoveDomainPart(user.UserPrincipalName)}");
                }
                else if (excelFormat)
                {
                    var usr = Client.GetUser(user);
                    list.Add($"{Client.RemoveDomainPart(user.UserPrincipalName)}|{user.GivenName}|{user.Surname}|{usr.Level}|{usr.Grade}|{usr.Division}");
                }
                else
                {
                    list.Add($"{user.DisplayName}");
                }
                count++;
            }

            list.Sort();

            if (excelFormat)
            {
                WriteExcel(list);
            }
            else
            {
                list.ForEach(Log.Logger.Information);
            }

            if (!RawFormat && !excelFormat)
            {
                Log.Logger.Information("");
                Log.Logger.Information($"Total {count} items found");
            }

        }

        void WriteExcel(List<string> list)
        {
            var filename = base.OutputFile ?? "default.xlsx";
            Log.Logger.Information($"Writing excel file to {base.OutputFile}");

            using (var workbook = new XLWorkbook(XLEventTracking.Disabled))
            {

                var ws = workbook.AddWorksheet("data");
                ws.Cell("A1").Value = "Email";
                //    ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("B1").Value = "Apellido";
                //     ws.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("C1").Value = "Nombre";
                //  ws.Cell("C1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("D1").Value = "Nivel";
                //   ws.Cell("D1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("E1").Value = "Grado";
                //    ws.Cell("E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("F1").Value = "Division";
                //    ws.Cell("F1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.RangeUsed().SetAutoFilter(true);

                int c = 2;

                foreach (var line in list)
                {
                    var parts = line.Split("|");
                    ws.Cell("A" + c).Value = parts[0];
                    ws.Cell("B" + c).Value = parts[1];
                    ws.Cell("C" + c).Value = parts[2];
                    ws.Cell("D" + c).Value = parts[3];
                    ws.Cell("E" + c).Value = parts[4];
                    ws.Cell("F" + c).Value = parts[5];
                    c++;
                }

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);
                //ws.Protect("ijme");

                workbook.SaveAs(filename);
            }

        }

        public override List<string> GetSupportedCommands()
        {
            return new List<string>() { "--listusers", "-lu" };
        }

        public override string Name => "List Users";
        public override string Description => "List users filtered by metadata, i.e. edu.exe -lu /type:Estudiante /level:Inicial";


    }
}
