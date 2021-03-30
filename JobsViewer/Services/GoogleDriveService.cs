using GalaSoft.MvvmLight.Messaging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace JobsViewer.Services
{
    class GoogleDriveService
    {
        private string applicationName;
        private string databaseId;
        private DriveService driveService;
        private GoogleFile databaseFile;

        public GoogleDriveService()
        {
            applicationName = "Jobs Reference";
            databaseId = Properties.Settings.Default.DatabaseFileId;

            Initialise();

            DownloadDatabase();
        }

        private void Initialise()
        {
            UserCredential credential;
            string[] scopes = { DriveService.Scope.DriveReadonly };

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            if (String.IsNullOrEmpty(databaseId))
                FindDatabaseFile();
        }

        private void FindDatabaseFile()
        {
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Q = "name = 'JobsDatabase.sdf'";

            IList<GoogleFile> files = listRequest.Execute().Files;

            if (files != null && files.Count > 0)
                databaseId = files[0].Id;
            else
                Console.WriteLine("No files found.");
        }

        private void DownloadDatabase()
        {

            var memoryStream = new MemoryStream();

            FilesResource.GetRequest getRequest = driveService.Files.Get(databaseId);
            GoogleFile file = getRequest.Execute();

            if (file == null)
            {
                databaseId = null;
                Console.WriteLine("No file found.");
            }
            else
            {
                databaseFile = file;
        
                getRequest.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) => DownloadProgress(progress);
                getRequest.Download(memoryStream);
            }
        }

        private void UploadDatabase()
        {
            GoogleFile fileMetadata = new GoogleFile()
            {
                Name = "JobsDatabase.sdf"
            };

            FilesResource.UpdateMediaUpload uploadRequest;
            using (var uploadStream = new FileStream("/Database/JobsDatabase.sdf", FileMode.Open, FileAccess.Read))
            {
                uploadRequest = driveService.Files.Update(fileMetadata, databaseId, uploadStream, "chemical/x-mdl-sdfile");
                uploadRequest.Fields = "id";
                uploadRequest.ProgressChanged += (Google.Apis.Upload.IUploadProgress progress) => UploadProgress(progress);
                uploadRequest.Upload();
            }
        }

        private void SaveDatabase(MemoryStream memoryStream)
        {
            using (var fileStream = new FileStream("/Database/JobsDatabase.sdf", FileMode.Create, FileAccess.Write))
            {
                memoryStream.WriteTo(fileStream);
            }
        }

        private void DownloadProgress(Google.Apis.Download.IDownloadProgress progress)
        {
            switch (progress.Status)
            {
                case Google.Apis.Download.DownloadStatus.Completed:
                    Messenger.Default.Send(new NotificationMessage("DatabaseDownloadComplete"));
                    break;

                case Google.Apis.Download.DownloadStatus.Failed:
                    Messenger.Default.Send(new NotificationMessage("DatabaseDownloadFailed"));
                    break;
            }
        }

        private void UploadProgress(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case Google.Apis.Upload.UploadStatus.Completed:
                    Messenger.Default.Send(new NotificationMessage("DatabaseUploadComplete"));
                    break;

                case Google.Apis.Upload.UploadStatus.Failed:
                    Messenger.Default.Send(new NotificationMessage("DatabaseUploadFailed"));
                    break;
            }
        }
    }
}
