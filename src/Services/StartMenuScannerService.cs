using Nova.Models;
using System.IO;
using Shell32;

namespace Nova.Services
{
    public class StartMenuScannerService
    {
        /*
         * Legacy code to add the Terminal manually.
         */
        private void AddSystemApps(List<ApplicationEntry> apps)
        {
            apps.Add(new ApplicationEntry
            {
                Name = "Terminal",
                Path = "wt"
            });
        }

        private void AddAppsFolderApps(List<ApplicationEntry> apps)
        {
            var shell = new Shell();
            Folder folder = shell.NameSpace("shell:AppsFolder");

            foreach (FolderItem item in folder.Items())
            {
                apps.Add(new ApplicationEntry
                {
                    Name = item.Name,
                    Path = $"shell:AppsFolder\\{item.Path}"
                });
            }
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

                var Files = Directory.GetFiles(
                    ProgramsPath,
                    "*.*",
                    SearchOption.AllDirectories)
                    .Where(File => File.EndsWith(".lnk") || File.EndsWith(".appref-ms"));

                foreach (var File in Files)
                {
                    Results.Add(new ApplicationEntry
                    {
                        Name = Path.GetFileNameWithoutExtension(File),
                        Path = File
                    });
                }
            }

            //AddSystemApps(Results);
            AddAppsFolderApps(Results);

            return Results;
        }
    }
}
