using Google.Apis.Drive.v3;
using LoginForm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveAPIExample
{
    public class Watcher
    {
        FileSystemWatcher watcher = new FileSystemWatcher();
        static APIService apiservice = new APIService();
        static DriveService service;
        public Watcher(APIService aPIService,DriveService Service)
        {
            service = Service;
            apiservice = aPIService;
            watcher.Path = aPIService.location;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.DirectoryName;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDelete;
            watcher.Renamed += OnRenamed;

        }
        public static void OnCreated(object sender, FileSystemEventArgs e)
        {
            if ( e.Name != "data.txt")
            {
                    if (e.ChangeType == WatcherChangeTypes.Created)
                    {
                        string fn = e.Name;
                        string filetype = Path.GetExtension(e.FullPath);
                        if (filetype == "")
                        {
                            Console.WriteLine("fn = " + fn + " type = " + filetype);
                            Console.WriteLine($"Add a folder in {e.FullPath}");
                            apiservice.createFolder(fn, service, e.FullPath, null);
                        
                        }
                        else
                        {
                            Console.WriteLine("fn = " + fn + " type = " + filetype);
                            Console.WriteLine($"Add a folder in {e.FullPath}");
                            apiservice.uploadFile(service, e.FullPath, null);
                            
                            //apiservice.switchFileBetweenDrive(e.FullPath);        
                        }
                    }
                
            }
        }
        public static void OnChanged(object sender, FileSystemEventArgs e)
        {

        }
        public static void OnDelete(object sender, FileSystemEventArgs e)
        {
            if (e.Name != "data.txt")
            {
                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    var deleteFileName = Path.GetFileName(e.FullPath);
                    apiservice.deleteFile(service, deleteFileName, e.FullPath);
                    Console.WriteLine("Delete file " + e.Name);
                }
            }

        }
        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            string oldFileName = e.OldName;
            string newFileName = e.Name;
            apiservice.renameFile(service, oldFileName, newFileName);

            Console.WriteLine($"Đổi tên tệp tin: {oldFileName} thành {newFileName}");
        }
        public void Start()
        {
            Console.WriteLine("Watching");
            watcher.EnableRaisingEvents = true;
        }
        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
        }
    }
}
