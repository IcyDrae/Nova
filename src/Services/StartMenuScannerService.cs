using Nova.Models;
using System.IO;

namespace Nova.Services
{
    public class StartMenuScannerService
    {
        private void AddSystemApps(List<ApplicationEntry> apps)
        {
            apps.Add(new ApplicationEntry
            {
                Name = "Terminal",
                Path = "wt"
            });
        }

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

            AddSystemApps(Results);

            return Results;
        }
    }
}
