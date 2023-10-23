using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using LoginForm;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GoogleDriveAPIExample
{
    public class APIService
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "demo 01";
        UserCredential credential;
        List<Container> listOfFiles = new List<Container>();
        public string location;
        string credPath;
        public string userName;
        
        public UserCredential GetCredential()
        {
            UserCredential credential;
            credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credential/drive-dotnet-quickstart.json");

            var fileDataStore = new FileDataStore(credPath, true);
            
                using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes, 
                        "user",
                        CancellationToken.None,
                        fileDataStore
                    ).Result;
                }
            
            return credential;
        }
        public void saveToken(string sourcePath,string destinationPath)
        {

            string fileContent = File.ReadAllText(sourcePath+ "\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");
            JObject jsonObject = JObject.Parse(fileContent);

            string accessToken = jsonObject["access_token"].ToString();
            string refreshToken = jsonObject["refresh_token"].ToString();
            string saveContent = accessToken + "," + refreshToken;
            File.WriteAllText(destinationPath, saveContent);
            Console.WriteLine("Done writing");
            try
            {
                // Kiểm tra xem tệp tin có tồn tại không
                if (File.Exists((sourcePath + "\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user")))
                {
                    // Xóa tệp tin
                    File.Delete((sourcePath + "\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"));
                    Console.WriteLine("Đã xóa tệp tin thành công.");
                }
                else
                {
                    Console.WriteLine("Tệp tin không tồn tại.");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Đã xảy ra lỗi: " + ex.Message);
            }

        }
        public DriveService  startService()
        {
            string fileName = "history.txt";
            credential = GetCredential();
            //start service
            
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
            
            var aboutRequest = service.About.Get();
            aboutRequest.Fields = "user";
            var aboutResponse = aboutRequest.Execute();

            // Lấy tên của người dùng
            userName = aboutResponse.User.EmailAddress;
            userName=userName.Replace("@gmail.com", "");
            // Đường dẫn thư mục mới
            string folderPath = Path.Combine("D:\\", userName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            this.location = folderPath;
            saveToken(credPath, this.location+"\\Token.txt");
            
            

            if (File.Exists(Path.Combine(Environment.CurrentDirectory, fileName )))
            {
                // Đọc nội dung của tệp tin
                string[] lines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, fileName));

                // Kiểm tra xem tên người dùng đã tồn tại trong tệp tin chưa
                if (Array.IndexOf(lines, userName) != -1)
                {
                    Console.WriteLine("Người dùng đã tồn tại trong tệp tin.");
                }
                else
                {
                    // Ghi thêm tên người dùng vào tệp tin
                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, fileName), userName + Environment.NewLine);
                    Console.WriteLine("Đã ghi tên người dùng vào tệp tin.");
                }
            }
            else
            {
                // Tạo tệp tin và ghi tên người dùng vào đó
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, fileName), userName + Environment.NewLine);
                Console.WriteLine("Đã tạo và ghi tên người dùng vào tệp tin.");
            }

            dowloadAllFilesAndFolders(service, this.location);
            return service;
        }
        public DriveService automatic(string userName)
        {
            this.location = $"D:\\{userName}";
            string tokenFilePath = $"D:\\{userName}\\Token.txt";
            string json = File.ReadAllText("client_secret.json");

            // Chuyển đổi chuỗi JSON thành đối tượng JObject
            JObject jsonObject = JObject.Parse(json);

            // Truy cập vào client_id và client_secret
            string clientId = jsonObject["installed"]["client_id"].ToString(); ;
            string clientSecret = jsonObject["installed"]["client_secret"].ToString();
            try
            {
                // Đọc dữ liệu từ tệp tin
                string tokenData = File.ReadAllText(tokenFilePath);

                // Tách chuỗi thành accessToken và refreshToken
                string[] tokens = tokenData.Split(',');

                // Lấy giá trị accessToken và refreshToken
                string accessToken = tokens[0];
                string refreshToken = tokens[1];

                // Tạo đối tượng UserCredential từ accessToken và refreshToken
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        // Thông tin client secrets của bạn
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    Scopes = new[] { DriveService.ScopeConstants.Drive },
                    DataStore = new FileDataStore("StoredCredential")
                });

                // Tạo UserCredential từ AccessToken và RefreshToken
                var credential = new UserCredential(flow, $"{userName}", new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                });

                // Tạo đối tượng DriveService từ UserCredential
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google Drive API Sample"
                });

                // Bây giờ bạn có thể sử dụng service để gọi các phương thức API Google Drive
                // Ví dụ: service.Files.List() để liệt kê các tệp trên Google Drive

                Console.WriteLine("Authentication successful!");
                return service;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Authentication failed: " + ex.Message);
            }
            return null;
        }
     
        public void createFolder(string folderName, DriveService service,string folderPath,string ParentId)
        {
            if (!isNameExist(Path.GetFileName(folderPath)))
            {
                if (ParentId == null)
                {
                    string Directory = Path.GetDirectoryName(folderPath);
                    string fName = Path.GetFileName(Directory);
                    ParentId = getIdFromFile(fName);
                }
                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = Path.GetFileName(folderPath),
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new List<string>() { ParentId }
                };

                // Tạo request để tạo thư mục mới
                var request = service.Files.Create(fileMetadata);
                request.Fields = "id"; // Chỉ lấy trường 'id' của thư mục được tạo

                // Thực hiện request để tạo thư mục mới
                var folder = request.Execute();
                AddDataTofile(Path.GetFileName(folderPath), folder.Id);
                // In thông tin về thư mục mới được tạo
                Console.WriteLine("Thư mục đã được tạo:");
                Console.WriteLine("ID: " + folder.Id);
                ////////
                if (Directory.Exists(folderPath))
                {
                    // Lấy danh sách tất cả các tệp tin và thư mục con trong thư mục đã cho
                    string[] files = Directory.GetFiles(folderPath);
                    string[] directories = Directory.GetDirectories(folderPath);
                    if (directories.Length > 0)
                    {
                        Console.WriteLine("Các thư mục con:");
                        foreach (string directory in directories)
                        {
                            createFolder(Path.GetFileName(directory), service, directory, folder.Id);
                        }
                    }
                    if (files.Length > 0)
                    {
                        Console.WriteLine("Các tệp tin trong thư mục:");
                        foreach (string file in files)
                        {
                            uploadFile(service, file, folder.Id);
                        }
                    }



                    if (files.Length == 0 && directories.Length == 0)
                    {
                        Console.WriteLine("Thư mục không chứa tệp tin hoặc thư mục con.");
                    }
                }
                else
                {
                    Console.WriteLine("Thư mục " +
                        "không tồn tại.");
                }
                Console.WriteLine("-----Thêm hoàn tất-----");
            }
            
        }
        ///
        public void deleteFile(DriveService service, string fileName,string filePath)
        {
            string fileId = getIdFromFile(fileName);
            if (DeleteData(fileId))
            {
                Console.WriteLine("Cập nhật file thành công");
            }
            // Tạo yêu cầu xóa tệp
            var request = service.Files.Delete(fileId);
            request.Execute();
            listOfFiles.Remove(listOfFiles.Where(x => x.id == fileId).FirstOrDefault());
            Console.WriteLine("Tệp đã được xóa thành công.");
            //UpdateDataFile();
            
        }
        ///
        public void renameFile(DriveService service, string currentfileName,string newfileName)
        {

            string fileId =getIdFromFile(currentfileName);
            UpdateData(currentfileName, newfileName);
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = newfileName
            };

            // Tạo yêu cầu cập nhật tên
            var request = service.Files.Update(fileMetadata, fileId);
            request.Fields = "id";
            var updatedFile = request.Execute();

            Console.WriteLine("Tên tệp hoặc thư mục đã được cập nhật thành công. ID của tệp hoặc thư mục là: " + updatedFile.Id);
        }
       
        public void dowloadAllFilesAndFolders(DriveService service, string downloadFolderPath)
        {
             listOfFiles = new List<Container>();
            string rootFolderId = "root";
            // Lấy danh sách tất cả các tệp tin và thư mục trên Google Drive
            var fileListRequest = service.Files.List();
            fileListRequest.Q = $"'{rootFolderId}' in parents and trashed = false";

            var fileList = fileListRequest.Execute();

            // Tải xuống từng mục
            foreach (var file in fileList.Files)
            {
                Container contain = new Container(file.Name, file.Id, file.MimeType,this.userName,this.location);
                listOfFiles.Add(contain);
                if (file.MimeType == "application/vnd.google-apps.folder")
                {
                    
                        // Nếu là thư mục, tải xuống thư mục bằng đệ quy
                        string folderPath = Path.Combine(downloadFolderPath, file.Name);
                        DownloadFolder(service, file.Id, folderPath);
                        Console.WriteLine("Thư mục : " + file.Name + " ID: " + file.Id + " Đã được tải");
                   
                }
                else
                {
                    // Nếu là tệp tin, tải xuống tệp tin
                    string filePath = Path.Combine(downloadFolderPath, file.Name);
                    //DownloadFile(service, file.Id, filePath);
                    File.WriteAllText(filePath, (contain.id+","+contain.name+","+contain.owner+","+contain.root));
                }
            }
            string datapath = downloadFolderPath + "\\data.txt";
            using (StreamWriter writer = new StreamWriter(datapath))
            {
                // Ghi dữ liệu từ danh sách vào tệp tin
                foreach (Container item in listOfFiles)
                {
                    string line = $"{item.id},{item.name}";
                    writer.WriteLine(line);
                }
            }
            Console.WriteLine("Done Dowloading...");

        }
        public void UpdateDataFile()
        {
            string datapath = this.location + "\\data.txt";
            using (StreamWriter writer = new StreamWriter(datapath))
            {
                // Ghi dữ liệu từ danh sách vào tệp tin
                foreach (Container item in listOfFiles)
                {
                    string line = $"{item.id},{item.name}";
                    writer.WriteLine(line);
                }
            }
            Console.WriteLine("Done Updating..");
        }
        public void DownloadFolder(DriveService service, string folderId, string folderPath)
        {
            Directory.CreateDirectory(folderPath);
            var fileListRequest = service.Files.List();
            fileListRequest.Q = $"'{folderId}' in parents";
            fileListRequest.Fields = "files(id, name, mimeType)";
            var fileList = fileListRequest.Execute();

            // Tải xuống từng mục
            foreach (var file in fileList.Files)
            {
                Container contain = new Container(file.Name, file.Id, file.MimeType,this.userName,this.location);
                listOfFiles.Add(contain);
                if (file.MimeType == "application/vnd.google-apps.folder")
                {
                    
                    
                        // Nếu là thư mục, tải xuống thư mục con bằng đệ quy
                        string subfolderPath = Path.Combine(folderPath, file.Name);
                        DownloadFolder(service, file.Id, subfolderPath);
                        Console.WriteLine("Thư mục : " + file.Name + " ID: " + file.Id + " Đã được tải");
                   
                }
                else
                {
                    // Nếu là tệp tin, tải xuống tệp tin
                    string filePath = Path.Combine(folderPath, file.Name);
                    File.WriteAllText(filePath, file.Id);
                    //DownloadFile(service, file.Id, filePath);
                    Console.WriteLine("Tệp " + file.Name + " ID: " + file.Id + " Đã được tải");
                }
            }
        }
        public void DownloadFile(DriveService service, string fileId, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                service.Files.Get(fileId).Download(stream);
            }

        }
        public void uploadFile(DriveService service,string filePath,string parentFolderId)
        {
            string uploadedFileId = null;
            if (!isNameExist(Path.GetFileName(filePath)))
            {
                if (parentFolderId == null)
                {
                    string Directory = Path.GetDirectoryName(filePath);
                    string folderName = Path.GetFileName(Directory);
                    parentFolderId = getIdFromFile(folderName);
                }
              
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = Path.GetFileName(filePath),
                    Parents = new List<string>() { parentFolderId }
                };

                FilesResource.CreateMediaUpload request;
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                {
                    request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
                    request.Fields = "id";
                    request.Upload();
                    var response = request.ResponseBody;

                    // Lấy ID của tệp đã tải lên
                    uploadedFileId = response?.Id;
                    AddDataTofile(fileMetadata.Name, uploadedFileId);
                    

                    //AccessFileContent(service, GetFileById(service,uploadedFileId));
                }
                File.WriteAllText(filePath, (uploadedFileId + "," + Path.GetFileName(filePath) + "," + this.userName + "," + this.location));
                Console.WriteLine("Done Uploading");
            }
        }
        public void readAllFileInfo(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            if (directoryInfo.Exists)
            {
                FileInfo[] files = directoryInfo.GetFiles();
                foreach(var file in files)
                {
                    Console.WriteLine(file.Name+" id: "+File.ReadAllText(file.FullName));
                }
            }
        }
        public string readOneFile(string filepath)
        {
            int count = 1;
            foreach(var line in File.ReadLines(filepath))
            {
                return line;
            }
            return null;
            
        }
        public string getIdFromFile(string Name)
        {
            string filePath = $"{this.location}\\data.txt";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    Console.WriteLine(parts[0]);
                    if (parts.Length == 2 && parts[0] == Name)
                    {
                       
                        return parts[1];
                    }
                }
            }

            return null;
        }
        public bool isNameExist(string Name)
        {
            string filePath = $"{this.location}\\data.txt";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && parts[0] == Name)
                    {

                        return true;
                    }
                }
            }
            return false;
        }
        public void AddDataTofile(string name, string id)
        {
            string filePath = $"{this.location}\\data.txt";
            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine($"{name},{id}");
            }
        }
        public bool DeleteData(string idToDelete)
        {
            string filePath = $"{this.location}\\data.txt";
            string tempFilePath = Path.GetTempFileName();

            using (StreamReader reader = new StreamReader(filePath))
            using (StreamWriter writer = new StreamWriter(tempFilePath))
            {
                string line;
                bool lineDeleted = false;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && parts[1] == idToDelete)
                    {
                        lineDeleted = true;
                        continue;
                    }

                    writer.WriteLine(line);
                }

                if (!lineDeleted)
                {
                    return false;
                }
            }

            File.Delete(filePath);
            File.Move(tempFilePath, filePath);

            return true;
        }
        public bool UpdateData(string name, string newName)
        {
            string filePath = $"{this.location}\\data.txt";
            string tempFilePath = Path.GetTempFileName();

            using (StreamReader reader = new StreamReader(filePath))
            using (StreamWriter writer = new StreamWriter(tempFilePath))
            {
                string line;
                bool nameUpdated = false;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && parts[0] == name)
                    {
                        line = $"{newName},{parts[1]}";
                        nameUpdated = true;
                    }

                    writer.WriteLine(line);
                }

                if (!nameUpdated)
                {
                    return false;
                }
            }

            File.Delete(filePath);
            File.Move(tempFilePath, filePath);

            return true;
        }
        public async Task<Stream> GetContentStreamAsync(DriveService service,Google.Apis.Drive.v3.Data.File fileEntry)
        {
            //Not every file resources in Google Drive has file content.
            //For example, a folder
            if (fileEntry.Size == null)
            {
                Console.WriteLine(fileEntry.Name + " is not a file. Skipped.");
                Console.WriteLine();
                return (null);
            }

            //Generate URI to file resource
            //"alt=media" indicates downloading file content instead of JSON metadata
            Uri fileUri = new Uri("https://www.googleapis.com/drive/v3/files/" + fileEntry.Id + "?alt=media");

            try
            {
                //Use HTTP client in DriveService to obtain response
                Task<Stream> fileContentStream = service.HttpClient.GetStreamAsync(fileUri);
                Console.WriteLine("Downloading file {0}...", fileEntry.Name);

                return (await fileContentStream);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while downloading file: " + e.Message);
                return (null);
            }
        }
        public Google.Apis.Drive.v3.Data.File GetFileById(DriveService service, string fileId)
        {
            try
            {
                // Gửi yêu cầu lấy thông tin tệp với ID cụ thể
                var request = service.Files.Get(fileId);
                request.Fields = "*"; // Lấy tất cả các trường của tệp

                // Thực hiện yêu cầu và trả về kết quả
                return request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("Lỗi khi lấy thông tin tệp: " + e.Message);
                return null;
            }
        }
        public async Task AccessFileContent(DriveService service, Google.Apis.Drive.v3.Data.File fileEntry)
        {
            // Gọi phương thức GetContentStreamAsync để tải nội dung của tệp
            Stream fileContentStream = await GetContentStreamAsync(service, fileEntry);

            if (fileContentStream != null)
            {
                try
                {
                    // Sử dụng đối tượng Stream để truy cập vào nội dung của tệp
                    // Ví dụ: Đọc dữ liệu từ Stream
                    using (StreamReader reader = new StreamReader(fileContentStream))
                    {
                        string fileContent = reader.ReadToEnd();
                        Console.WriteLine("Content of {0}:", fileEntry.Name);
                        Console.WriteLine(fileContent);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while accessing file content: " + e.Message);
                }
                finally
                {
                    // Đảm bảo đóng Stream sau khi sử dụng xong
                    fileContentStream.Close();
                    fileContentStream.Dispose();
                }
            }
        }
        public void LogoutAsync()
        {
            // Xóa thông tin xác thực đã lưu trữ
            UserCredential credential = GetCredential();// Lấy đối tượng UserCredential đã được lưu trữ
            credential.RevokeTokenAsync(CancellationToken.None).Wait();
            credential = null;

            // Xóa các tệp cookie hoặc thông tin xác thực khác liên quan đến đăng nhập
            // Ví dụ: Xóa cookie trong trình duyệt
            // ...
        }
        public void switchFileBetweenDrive(string filePath)
        {
            Console.WriteLine(File.ReadAllText(filePath));
        }
       

    }
}
