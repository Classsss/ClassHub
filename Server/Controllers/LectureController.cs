using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Npgsql;


namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]

    public class LectureController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        const string blobStorageUri = "https://classhubfilestorage.blob.core.windows.net/";
        const string vaultStorageUri = "https://azureblobsecret.vault.azure.net/";

        // 동영상 강의 db를 생성한다
        [HttpPost("{RoomId}/createdb")]
        public async Task<int> UploadLectureDb(int room_id, Shared.Lecture lecture) {

           using var connection = new NpgsqlConnection(connectionString);
           var insertQuery = "INSERT INTO lecture (room_id,week,title,contents,start_date,end_date,video_url,learning_time) " +
                              "VALUES (@room_id, @week,@title,@contents,@start_date,@end_date,@video_url,@learning_time);"+
                               "SELECT lastval();";

            var parameters = new DynamicParameters();
            parameters.Add("room_id", lecture.room_id);
            parameters.Add("week", lecture.week);
            parameters.Add("title", lecture.title);
            parameters.Add("contents", lecture.contents);
            parameters.Add("start_date", lecture.start_date);
            parameters.Add("end_date", lecture.end_date);
            parameters.Add("video_url", "none");
            parameters.Add("learning_time", 0);

            // lecture_id를 반환한다.
            int lecture_id = connection.ExecuteScalar<int>(insertQuery,parameters);
            return lecture_id;
        }

        // 동영상 강의 db를 수정한다
        [HttpPut("{RoomId}/modifydb/{LectureId}")]
        public async Task ModifyLectureDb(int room_id,int lecture_id, Shared.Lecture lecture) {

            using var connection = new NpgsqlConnection(connectionString);
            var modifyQuery = "Update lecture SET week = @week, title = @title, contents = @contents, start_date = @start_date, end_date = @end_date WHERE room_id = @room_id AND lecture_id = @lecture_id ";
            var parameters = new DynamicParameters();
            parameters.Add("week", lecture.week);
            parameters.Add("title", lecture.title);
            parameters.Add("contents", lecture.contents);
            parameters.Add("start_date", lecture.start_date);
            parameters.Add("end_date", lecture.end_date);
            parameters.Add("room_id", lecture.room_id);
            parameters.Add("lecture_id", lecture.lecture_id);
            connection.Execute(modifyQuery, parameters);
        }

        // 동영상 강의를 삭제한다 (db)
        [HttpDelete("{RoomId}/removedb/{LectureId}")]
        public async Task removeLectureDB(int RoomId, int LectureId) {

            // 동영상 강의를 삭제
            using var connection = new NpgsqlConnection(connectionString);

            // 강의 진행률먼저 삭제
            var deleteQuery = "DELETE FROM lectureprogress WHERE room_id = @room_id AND lecture_id = @lecture_id";
            var parametersprogress = new DynamicParameters();
            parametersprogress.Add("room_id", RoomId);
            parametersprogress.Add("lecture_id", LectureId);
            connection.Execute(deleteQuery, parametersprogress);

            //강의 삭제
            deleteQuery = "DELETE FROM lecture WHERE room_id = @room_id AND lecture_id = @lecture_id";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", RoomId);
            parameters.Add("lecture_id", LectureId);
            connection.Execute(deleteQuery, parameters);
        }

        // 동영상 강의를 삭제한다 (blob)
        [HttpDelete("{RoomId}/removeblob/{LectureId}")]
        public async Task removeLectureBlobFile(int RoomId, int LectureId) {
            // blob의 파일 삭제
            var blobServiceClient = new BlobServiceClient(
            new Uri("https://classhubfilestorage.blob.core.windows.net"),
            new DefaultAzureCredential()
            );
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("lecture");
            string folderPath = $"{RoomId}/{LectureId}";
            List<BlobClient> blobClients = containerClient.GetBlobs(prefix: folderPath)
                .Select(blobItem => containerClient.GetBlobClient(blobItem.Name))
                .ToList();

            await Console.Out.WriteLineAsync("Deleting blob...");
            var response = await blobClients[0].DeleteAsync();
            await Console.Out.WriteLineAsync($"\tResponse Status: {response.Status}\n");
        }

        // 수정할때 blob을 임시로 none 삽입하는 작업
        [HttpPut("{RoomId}/insertnone/{LectureId}")]
        public async Task<IActionResult> ModifyLectureFiles(int RoomId, int LectureId) {
            
            // url이 업데이트 되기 전까지는 잠시 보이지 않게 하기위해 none을 삽입
            using var connection = new NpgsqlConnection(connectionString);
            string query = "UPDATE lecture SET video_url = @video_url WHERE room_id = @room_id AND lecture_id = @lecture_id ";
            var parametersNone = new DynamicParameters();
            parametersNone.Add("video_url", "none"); 
            connection.Execute(query, parametersNone);
            return Ok();
        }

        // 생성할때 blob에 동영상을 업로드 및 url작업
        [HttpPost("{RoomId}/upload/{LectureId}")]
        public async Task<IActionResult> UploadLectureFiles(int RoomId, int LectureId, List<IFormFile> files) {

            // blob 업로드하는 작업
            var blobServiceClient = new BlobServiceClient(
                new Uri(blobStorageUri),
                new DefaultAzureCredential()
            );

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("lecture");
            using (var memoryStream = new MemoryStream()) {
                await files[0].CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var blobClient = containerClient.GetBlobClient(files[0].FileName);
                await Console.Out.WriteLineAsync($"Blob Name: {blobClient.Name}");
                var response = await blobClient.UploadAsync(memoryStream, overwrite: true);
                await Console.Out.WriteLineAsync(response.ToString());
            }

            await Console.Out.WriteLineAsync("Blob Upload Success!");

            // url및 동영상 길이 구하고 db에 넣는 작업
                
            var secretClient = new SecretClient(
            vaultUri: new Uri(vaultStorageUri),
            credential: new DefaultAzureCredential()
            );
            string secretName = "StorageAccountKey";
            KeyVaultSecret secret = secretClient.GetSecret(secretName);
            var storageAccountKey = secret.Value;

            BlobClient blobClienturl = containerClient.GetBlobClient(files[0].FileName);

            // 동영상 길이를 구한다. 
            var properties = await blobClienturl.GetPropertiesAsync();
            var contentLengthInBytes = properties.Value.ContentLength; // 파일 크기 (바이트)
            var contentLengthInSeconds = contentLengthInBytes / (1024 * 1024); // 파일 크기를 초 단위로 변환

            var minutes = (int)Math.Ceiling((double)contentLengthInSeconds / 60); // 분 (반올림)


            // url을 구한다.
            BlobSasBuilder sasBuilder = new BlobSasBuilder() {
                BlobContainerName = containerClient.Name,
                BlobName = blobClienturl.Name,
                Resource = "b",
                StartsOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddMonths(3)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(containerClient.AccountName, storageAccountKey)).ToString();

            UriBuilder sasUri = new UriBuilder(blobClienturl.Uri) {
                Query = sasToken
            };
            string downloadUrl = sasUri.ToString();
            string encodedUrl =  Uri.EscapeUriString(downloadUrl);
            Console.WriteLine(downloadUrl);
            await Console.Out.WriteLineAsync("sasBuild and getTime Success!");

            using var connection = new NpgsqlConnection(connectionString);
         
            string query = "UPDATE lecture SET video_url = @video_url, learning_time = @learning_time WHERE room_id = @room_id AND lecture_id = @lecture_id ";
            var parametersUpdate = new DynamicParameters();
            parametersUpdate.Add("room_id", RoomId);
            parametersUpdate.Add("lecture_id", LectureId);
            parametersUpdate.Add("video_url", encodedUrl);
            parametersUpdate.Add("learning_time", minutes);
            connection.Execute(query, parametersUpdate);

            await Console.Out.WriteLineAsync("db update Success!");
            return Ok();
        }

      
        // 학생이 해당 강의 진행률을 가지고 있는지 확인 후 없으면 생성한다.
        [HttpPost("hasprogress/room_id/{room_id}/lecture_id/{lecture_id}/student_id/{student_id}")]
        public void CheckLectureProgress(int room_id, int lecture_id, int student_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query = "SELECT * FROM lectureprogress WHERE lecture_id = @lecture_id AND student_id = @student_id;";
            var parametersProgress = new DynamicParameters();
            parametersProgress.Add("lecture_id", lecture_id);
            parametersProgress.Add("student_id", student_id);
            var lectureProgress = connection.QuerySingleOrDefault<LectureProgress>(query, parametersProgress);

            if (lectureProgress == null) {
                query = "INSERT INTO lectureprogress (room_id, lecture_id, student_id, elapsed_time, is_enroll) VALUES (@room_id, @lecture_id, @student_id, @elapsed_time, @is_enroll);";
                parametersProgress.Add("room_id", room_id);
                parametersProgress.Add("elapsed_time", 0);
                parametersProgress.Add("is_enroll", false);
                connection.Execute(query, parametersProgress);
            }
        }

        // 교수가 학생들의 강의 진행률을 확인한다.
        [HttpGet("progress/{lecture_id}")]
        public List<LectureProgress> CheckLectureProgress(int lecture_id) {

            using var connection = new NpgsqlConnection(connectionString);
            string query = "SELECT * FROM lectureprogress WHERE lecture_id = @lecture_id";
            var parametersProgress = new DynamicParameters();
            parametersProgress.Add("lecture_id", lecture_id);
            List<LectureProgress> lectureProgressList = connection.Query<LectureProgress>(query, parametersProgress).ToList();

            return lectureProgressList;
        }

    }
    public class LectureHubController : Hub {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        private static Dictionary<string, bool> isDatabaseWatcherRunning = new Dictionary<string, bool>();

        // 1분마다 강의수강기록을 업데이트한다.
        public async Task UpdateLectureProgressWatcher(int lecture_id,int student_id) {

            string connectionId = Context.ConnectionId;
            Console.WriteLine(connectionId);
            using var connection = new NpgsqlConnection(connectionString);
            isDatabaseWatcherRunning[connectionId] = true;
            while (isDatabaseWatcherRunning[connectionId]) {
                // 테스트를 위해 1초마다 업데이트 원래는 60초마다 해야함
                await Task.Delay(1000);
                Console.WriteLine("test" + student_id);
                string query = "UPDATE lectureprogress SET elapsed_time = elapsed_time +1 WHERE lecture_id = @lecture_id AND student_id = @student_id";
                var parametersUpdate = new DynamicParameters();
                parametersUpdate.Add("lecture_id", lecture_id);
                parametersUpdate.Add("student_id", student_id);
                connection.Execute(query, parametersUpdate);

               // 수강시간을 넘었는지 확인후 수강완료 처리.
                query = "SELECT learning_time From lecture WHERE lecture_id = @lecture_id";
                int learning_time = connection.Query<int>(query,parametersUpdate).FirstOrDefault();

                query = "SELECT elapsed_time From lectureprogress WHERE lecture_id = @lecture_id AND student_id = @student_id";
                int elapsed_time = connection.Query<int>(query, parametersUpdate).FirstOrDefault();

                if(elapsed_time >= learning_time) {
                   query = "UPDATE lectureprogress SET is_enroll = TRUE WHERE lecture_id = @lecture_id AND student_id = @student_id";
                    connection.Execute(query, parametersUpdate);
                }
            }
            isDatabaseWatcherRunning.Remove(connectionId);
        }

        // 연결이 끊길시 호출
        public override async Task OnDisconnectedAsync(Exception exception) {
            string connectionId = Context.ConnectionId;
            await base.OnDisconnectedAsync(exception);
            // 연결 종료 시 isDatabaseWatcher의 while 루프를 빠져나오게끔 함
            if (isDatabaseWatcherRunning.ContainsKey(connectionId)) {
                isDatabaseWatcherRunning[connectionId] = false;
            }
        }
    }
}
