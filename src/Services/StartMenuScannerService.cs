using Nova.Models;
using System.IO;

namespace Nova.Services
{
    public class StartMenuScannerService
    {
        public List<ApplicationEntry> Scan()
        {
            var Results = new List<ApplicationEntry>();

            var Folders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
            };

            foreach (var folder in Folders)
            {
                var ProgramsPath = Path.Combine(folder, "Programs");

                if (!Directory.Exists(ProgramsPath))
                    continue;

                var Files = Directory.GetFiles(ProgramsPath, "*.lnk", SearchOption.AllDirectories);

                foreach (var File in Files)
                {
                    Results.Add(new ApplicationEntry
                    {
                        Name = Path.GetFileNameWithoutExtension(File),
                        Path = File
                    });
                }
            }

            return Results;
        }
    }
}
