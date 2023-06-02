using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
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
    public class AssignmentController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        const string blobStorageUri = "https://classhubfilestorage.blob.core.windows.net/";
        const string vaultStorageUri = "https://azureblobsecret.vault.azure.net/";

        // 해당 과제를 보여줍니다.
        [HttpGet("room_id/{room_id}/assignment_id/{assignment_id}/student_id/{student_id}")]
        public Client.Models.Assignment GetAssignment(int room_id, int assignment_id, int student_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            Shared.Assignment dbAssignment = new Shared.Assignment();

            // db에서 해당 과제를 찾습니다
            query = "SELECT * FROM assignment WHERE room_id = @room_id AND assignment_id = @assignment_id;";
            var parametersAssignment = new DynamicParameters();
            parametersAssignment.Add("room_id", room_id);
            parametersAssignment.Add("assignment_id", assignment_id);
            dbAssignment = connection.Query<Shared.Assignment>(query, parametersAssignment).FirstOrDefault();

            // 과제 제출 내역이 있는지 check
            query = "SELECT  COUNT(*) FROM assignmentsubmit WHERE room_id = @room_id AND assignment_id = @assignment_id AND student_id = @student_id;";
            var parametersSubmit = new DynamicParameters();
            parametersSubmit.Add("room_id", dbAssignment.room_id);
            parametersSubmit.Add("assignment_id", dbAssignment.assignment_id);
            parametersSubmit.Add("student_id", student_id);
            bool is_submit = true;
            if (connection.QueryFirstOrDefault<int>(query, parametersSubmit) == 0) { is_submit = false; }

            //이제 practice모델에 필요한 값들을 넣어줍니다.
            Client.Models.Assignment assignment = new Client.Models.Assignment {
                Id = dbAssignment.assignment_id,
                Title = dbAssignment.title,
                Author = dbAssignment.author,
                Description = dbAssignment.contents,
                StartDate = dbAssignment.start_date,
                EndDate = dbAssignment.end_date,
                IsSubmitted = is_submit,
            };

            return assignment;
        }

        // 과제 첨부파일 목록을 불러옵니다.
        [HttpGet("attachments")]
        public List<AssignmentMaterialAttachment> GetAssignmentAttachments([FromQuery] int room_id, [FromQuery] int assignment_id) {

            using var connection = new NpgsqlConnection(connectionString);
            List<AssignmentMaterialAttachment> attachments = new List<AssignmentMaterialAttachment>();
            
            string query = "SELECT * FROM assignmentmaterialattachment WHERE assignment_id = @assignment_id;";
            var parameters = new DynamicParameters();
            parameters.Add("assignment_id", assignment_id);
            var Attachments = connection.Query<AssignmentMaterialAttachment>(query, parameters);

            foreach (AssignmentMaterialAttachment Attachment in Attachments) {
                attachments.Add(Attachment);
            }
            return attachments;
        }

        // 과제를 등록합니다
        [HttpPost("register")]
        public ActionResult RegisterAssignment([FromBody] Shared.Assignment assignment) {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using (var transaction = connection.BeginTransaction()) {
                try {
                    string query1 = // 다음 assignment_id 시퀀스를 가져옴.
                        "SELECT nextval('assignment_assignment_id_seq');";
                    assignment.assignment_id = connection.QuerySingle<int>(sql: query1, transaction: transaction);
                    Console.WriteLine(assignment.assignment_id);
                    string query2 =
                        "INSERT INTO assignment (assignment_id, room_id, title, author, contents, start_date, end_date) " +
                        "VALUES (@assignment_id, @room_id, @title, @author, @contents, @start_date, @end_date);";
                    connection.Execute(query2, assignment);
                    Console.WriteLine(assignment.assignment_id);
                    transaction.Commit();
                } catch (Exception ex) {
                    transaction.Rollback();
                    return BadRequest();
                }
            }
            return new ObjectResult(assignment.assignment_id);

        }

        // 과제 db를 수정합니다
        [HttpPut("{RoomId}/modifydb/{AssignmentId}")]
        public async Task ModifyAssignmentDb(int RoomId, int AssignmentId, Shared.Assignment assignment) {
            using var connection = new NpgsqlConnection(connectionString);
            var modifyQuery = "Update assignment SET title = @title, author = @author, contents = @contents, start_date = @start_date, end_date = @end_date WHERE room_id = @room_id AND assignment_id = @assignment_id";
            var parameters = new DynamicParameters();
            parameters.Add("author", assignment.author);
            parameters.Add("title", assignment.title);
            parameters.Add("contents", assignment.contents);
            parameters.Add("start_date", assignment.start_date);
            parameters.Add("end_date", assignment.end_date);
            parameters.Add("room_id", RoomId);
            parameters.Add("assignment_id", AssignmentId);

            connection.Execute(modifyQuery, parameters);
        }

        // 과제를 생성할때 자료를 blob에 업로드합니다.
        [HttpPost("{room_id}/upload/{assignment_id}")]
        public async Task<IActionResult> UploadLectureMaterialFiles(int room_id, int assignment_id, List<IFormFile> files) {

            // url db를 생성하기위함
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            var blobServiceClient = new BlobServiceClient(
                new Uri(blobStorageUri),
                new DefaultAzureCredential()
            );
            
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("assignmentmaterial");
            await Console.Out.WriteLineAsync($"files count: {files.Count}");

            foreach (var file in files) {
                // blob에 업로드
                using (var memoryStream = new MemoryStream()) {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var blobClient = containerClient.GetBlobClient(file.FileName);
                    await Console.Out.WriteLineAsync($"Blob Name: {blobClient.Name}");
                    var response = await blobClient.UploadAsync(memoryStream, overwrite: true);
                    await Console.Out.WriteLineAsync(response.ToString());
                }

                await Console.Out.WriteLineAsync("Blob Upload Success!");

                // 업로드한 blob의 url을 구한다.
                var secretClient = new SecretClient(
                vaultUri: new Uri(vaultStorageUri),
                credential: new DefaultAzureCredential()
                );
                string secretName = "StorageAccountKey";
                KeyVaultSecret secret = secretClient.GetSecret(secretName);
                var storageAccountKey = secret.Value;

                BlobClient blobClienturl = containerClient.GetBlobClient(file.FileName);

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

                await Console.Out.WriteLineAsync("Blob Get Url Success!");

               // Blob 속성 가져오기
                var properties = await blobClienturl.GetPropertiesAsync();

                // 용량과 업데이트 날짜 이름 등을 잘 설정해서 가져오기
                long fileSize = properties.Value.ContentLength;
                double fileSizeKB = (double)fileSize / 1024.0; // KB로 변환 (소수점 유지)
                DateTime lastModifiedKST = TimeZoneInfo.ConvertTimeFromUtc(properties.Value.LastModified.DateTime, TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"));

                using (var transaction = connection.BeginTransaction()) {
                    try {
                        var insertQuery = "INSERT INTO assignmentmaterialattachment (assignment_id, file_name, file_url, update, file_size) " +
                           "VALUES (@assignment_id, @file_name, @file_url,@update,@file_size);";
                        var parameters = new DynamicParameters();
                        parameters.Add("assignment_id", assignment_id);
                        parameters.Add("file_name", file.FileName);
                        parameters.Add("file_url",downloadUrl);
                        parameters.Add("update",lastModifiedKST);
                        parameters.Add("file_size",fileSizeKB);

                        connection.Execute(insertQuery, parameters); 
                        transaction.Commit();
                    } catch (Exception ex) {
                        transaction.Rollback();
                        return BadRequest();
                    }
                }
                await Console.Out.WriteLineAsync("DB update Success!");
            }
            await Console.Out.WriteLineAsync("All file processing completed!");

            return Ok();
        }

        // 과제를 삭제합니다 (db)
        [HttpDelete("{RoomId}/removedb/{AssignmentId}")]
        public async Task removeAssignmentDB(int RoomId, int AssignmentId) {
            using var connection = new NpgsqlConnection(connectionString);

            // 과제의 파일url db 삭제
            var deleteQuery = "DELETE FROM assignmentmaterialattachment WHERE assignment_id = @assignment_id";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", RoomId);
            parameters.Add("assignment_id", AssignmentId);
            connection.Execute(deleteQuery, parameters);

            // 과제 제출 내역url db 삭제
            var getQuery = "SELECT submit_id FROM assignmentsubmit WHERE room_id = @room_id AND assignment_id = @assignment_id";
            var submit_ids = connection.Query<int>(getQuery, parameters);
            foreach(int submit_id in submit_ids) {
                deleteQuery = "DELETE FROM assignmentsubmitattachment WHERE submit_id = @submit_id";
                var parametersSubmit = new DynamicParameters();
                parametersSubmit.Add("submit_id", submit_id);
                connection.Execute(deleteQuery, parametersSubmit);
            }

            // 과제 제출 내역 삭제
            deleteQuery = "DELETE FROM assignmentsubmit WHERE room_id = @room_id AND assignment_id = @assignment_id";
            connection.Execute(deleteQuery, parameters);

            // 과제 삭제
            deleteQuery = "DELETE FROM assignment WHERE room_id = @room_id AND assignment_id = @assignment_id";
            parameters.Add("room_id", RoomId);
            parameters.Add("assignment_id", AssignmentId);
            connection.Execute(deleteQuery, parameters);
        }

        // 특정 과제 첨부파일의 url db를 삭제합니다
        [HttpDelete("{RoomId}/removedb/{AssignmentId}/AttachmentId/{AttachmentId}")]
        public async Task removeAssignmentUrlDB(int RoomId, int AssignmentId, int AttachmentId) {
            using var connection = new NpgsqlConnection(connectionString);

            // 과제의 파일url db 삭제
            var deleteQuery = "DELETE FROM assignmentmaterialattachment WHERE assignment_id = @assignment_id AND attachment_id = @attachment_id";
            var parameters = new DynamicParameters();
            parameters.Add("assignment_id", AssignmentId);
            parameters.Add("attachment_id", AttachmentId);
            connection.Execute(deleteQuery, parameters);
        }

        // 특정 자료파일을 삭제합니다 (blob)
        [HttpDelete("{RoomId}/removeblob/{AssignmentId}/filename/{*FileName}")]
        public async Task removeAssignmentBlobFileOne(int RoomId, int AssignmentId, string FileName) {
            // blob의 파일 삭제
            var blobServiceClient = new BlobServiceClient(
            new Uri("https://classhubfilestorage.blob.core.windows.net"),
            new DefaultAzureCredential()
            );

            // 과제 자료 파일 삭제
            string folderPath = $"{RoomId}/{AssignmentId}";
            BlobContainerClient containerClientMaterial = blobServiceClient.GetBlobContainerClient("assignmentmaterial");
            BlobClient blobClient = containerClientMaterial.GetBlobClient(FileName);

            await Console.Out.WriteLineAsync("Deleting blob...");
            var response = await blobClient.DeleteAsync();
            await Console.Out.WriteLineAsync($"\tResponse Status: {response.Status}\n");
        }

       

        // 과제 자료파일과 제출자의 파일까지 삭제합니다 (blob)
        [HttpDelete("{RoomId}/removeblob/{AssignmentId}")]
        public async Task removeAssignmentBlobFile(int RoomId, int AssignmentId) {
            // blob의 파일 삭제
            var blobServiceClient = new BlobServiceClient(
            new Uri("https://classhubfilestorage.blob.core.windows.net"),
            new DefaultAzureCredential()
            );


            // 과제 자료 파일 삭제
            string folderPath = $"{RoomId}/{AssignmentId}";
            BlobContainerClient containerClientMaterial = blobServiceClient.GetBlobContainerClient("assignmentmaterial");
            List<BlobClient> blobClientsMaterial = containerClientMaterial.GetBlobs(prefix: folderPath)
                .Select(blobItem => containerClientMaterial.GetBlobClient(blobItem.Name))
                .ToList();

            await Console.Out.WriteLineAsync("Deleting blob...");
            foreach(BlobClient blobClient in blobClientsMaterial) {
                var response = await blobClient.DeleteAsync();
                await Console.Out.WriteLineAsync($"\tResponse Status: {response.Status}\n");
            }

            // 제출 자료 파일 삭제
            BlobContainerClient containerClientSubmit = blobServiceClient.GetBlobContainerClient("assignmentsubmit");
            List<BlobClient> blobClientsSubmit = containerClientSubmit.GetBlobs(prefix: folderPath)
                .Select(blobItem => containerClientSubmit.GetBlobClient(blobItem.Name))
                .ToList();

            await Console.Out.WriteLineAsync("Deleting blob...");
            foreach (BlobClient blobClient in blobClientsSubmit) {
                var response = await blobClient.DeleteAsync();
                await Console.Out.WriteLineAsync($"\tResponse Status: {response.Status}\n");
            }
        }

    }
}
