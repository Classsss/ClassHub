using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage;
using Azure.Storage.Blobs;
using System.IO.Compression;
using Azure.Storage.Sas;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using static System.Formats.Asn1.AsnWriter;


namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentSubmitController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        const string blobStorageUri = "https://classhubfilestorage.blob.core.windows.net/";
        const string vaultStorageUri = "https://azureblobsecret.vault.azure.net/";

        private readonly ILogger<AssignmentSubmitController> _logger;

        // 해당 과제의 제출 내역이 있는지 체크합니다. 
        [HttpGet("room_id/{room_id}/assignment_id/{assignment_id}/student_id/{student_id}")]
        public AssignmentSubmit GetAssignmentSubmit(int room_id, int assignment_id, int student_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            // 과제 제출 내역이 있는지 check
            query = "SELECT * FROM assignmentsubmit WHERE room_id = @room_id AND assignment_id = @assignment_id AND student_id = @student_id;";
            var parametersSubmit = new DynamicParameters();
            parametersSubmit.Add("room_id", room_id);
            parametersSubmit.Add("assignment_id", assignment_id);
            parametersSubmit.Add("student_id", student_id);
            var assignmentSubmit = connection.QueryFirstOrDefault<AssignmentSubmit>(query, parametersSubmit);

            //제출 내역 없으면 빈 껍데기를 보여준다.
            if (assignmentSubmit == null) {
                assignmentSubmit = new AssignmentSubmit {
                    submit_id = 0,
                    score = 0
                };
            }
            return assignmentSubmit;
        }

        // 과제를 등록합니다
        [HttpPost("register")]
        public ActionResult RegisterAssignmentSubmit([FromBody] AssignmentSubmit assignmentSubmit) {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using (var transaction = connection.BeginTransaction()) {
                try {
                    string query1 = // 다음 submit_id 시퀀스를 가져옴.
                        "SELECT nextval('assignmentsubmit_submit_id_seq');";
                    assignmentSubmit.submit_id = connection.QuerySingle<int>(sql: query1, transaction: transaction);
                    string query2 =
                        "INSERT INTO assignmentsubmit (submit_id, assignment_id, room_id, student_id, student_name, score, submit_date, message) " +
                        "VALUES (@submit_id, @assignment_id, @room_id, @student_id, @student_name, @score, @submit_date, @message);";
                    connection.Execute(query2, assignmentSubmit);
                    transaction.Commit();
                } catch (Exception ex) {
                    transaction.Rollback();
                    _logger.LogError($"msg :\n{ex.Message}");
                    return BadRequest();
                }
            }
            return new ObjectResult(assignmentSubmit.submit_id);

        }

        // 과제 첨부파일 목록을 불러옵니다.
        [HttpGet("attachments")]
        public List<AssignmentSubmitAttachment> GetAssignmentSubmitAttachments([FromQuery] int room_id, [FromQuery] int submit_id) {

            using var connection = new NpgsqlConnection(connectionString);
            List<AssignmentSubmitAttachment> attachments = new List<AssignmentSubmitAttachment>();
            string query = "SELECT * FROM assignmentsubmitattachment WHERE submit_id = @submit_id;";
            var parameters = new DynamicParameters();
            parameters.Add("submit_id", submit_id);
            var Attachments = connection.Query<AssignmentSubmitAttachment>(query, parameters);

            foreach (AssignmentSubmitAttachment Attachment in Attachments) {
                attachments.Add(Attachment);
            }
            return attachments;
        }

        // 제출 과제를 db를 수정합니다
        [HttpPut("{RoomId}/modifydb/{SubmitId}")]
        public async Task ModifyAssignmentDb(int RoomId, int SubmitId, AssignmentSubmit assignmentSubmit) {
            Console.WriteLine(assignmentSubmit.submit_date);
            using var connection = new NpgsqlConnection(connectionString);
            var modifyQuery = "Update assignmentsubmit SET submit_date = @submit_date WHERE submit_id = @submit_id";
            var parameters = new DynamicParameters();
            parameters.Add("submit_date", assignmentSubmit.submit_date);
            parameters.Add("submit_id", SubmitId);
            connection.Execute(modifyQuery, parameters);
        }

        // 과제를 생성할때 자료를 blob에 업로드합니다.
        [HttpPost("{room_id}/upload/{assignment_id}/submitid/{submit_id}")]
        public async Task<IActionResult> UploadLectureMaterialFiles(int room_id, int assignment_id, int submit_id, List<IFormFile> files) {
            // url db를 생성하기위함
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            var blobServiceClient = new BlobServiceClient(
                new Uri(blobStorageUri),
                new DefaultAzureCredential()
            );
            
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("assignmentsubmit");
            await Console.Out.WriteLineAsync($"files count: {files.Count}");

            foreach (var file in files) {
                var blobClient = containerClient.GetBlobClient(file.FileName);
                var task = Task.Run(async () => {
                    try {
                        using (var memoryStream = new MemoryStream()) {
                            await file.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                            await blobClient.UploadAsync(memoryStream, overwrite: true);
                        }
                    } catch (Exception e) {
                        // 콘솔에 예외 로그를 출력합니다.
                        Console.WriteLine($"File upload failed: {e.Message}");
                    }
                });
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
                        var insertQuery = "INSERT INTO assignmentsubmitattachment (submit_id, file_name, file_url, update, file_size) " +
                           "VALUES (@submit_id, @file_name, @file_url,@update,@file_size);";
                        var parameters = new DynamicParameters();
                        parameters.Add("submit_id", submit_id);
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

        // 특정 과제 첨부파일의 url db를 삭제합니다
        [HttpDelete("{RoomId}/removedb/submitid/{SubmitId}/AttachmentId/{AttachmentId}")]
        public async Task removeAssignmentUrlDB(int RoomId, int SubmitId, int AttachmentId) {
            using var connection = new NpgsqlConnection(connectionString);

            // 과제의 파일url db 삭제
            var deleteQuery = "DELETE FROM assignmentsubmitattachment WHERE submit_id = @submit_id AND attachment_id = @attachment_id";
            var parameters = new DynamicParameters();
            parameters.Add("submit_id", SubmitId);
            parameters.Add("file_name", AttachmentId);
            connection.Execute(deleteQuery, parameters);
        }

        // 특정 자료파일을 삭제합니다 (blob)
        [HttpDelete("{RoomId}/removeblob/{AssignmentId}/submitid/{SubmitId}/filename/{*FileName}")]
        public async Task removeAssignmentBlobFileOne(int RoomId, int AssignmentId, int SubmitId, string FileName) {
            // blob의 파일 삭제
            var blobServiceClient = new BlobServiceClient(
            new Uri("https://classhubfilestorage.blob.core.windows.net"),
            new DefaultAzureCredential()
            );

            // 과제 자료 파일 삭제
            string folderPath = $"{RoomId}/{AssignmentId}/{SubmitId}";
            BlobContainerClient containerClientMaterial = blobServiceClient.GetBlobContainerClient("assignmentsubmit");
            BlobClient blobClient = containerClientMaterial.GetBlobClient(FileName);

            await Console.Out.WriteLineAsync("Deleting blob...");
            var response = await blobClient.DeleteAsync();
            await Console.Out.WriteLineAsync($"\tResponse Status: {response.Status}\n");
        }

        // 해당 과제를 보여줍니다.
        [HttpGet("RoomId/{RoomId}/AssignmentId/{AssignmentId}")]
        public List<AssignmentSubmit> GetAssignmentSubmit(int RoomId, int AssignmentId) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            List<AssignmentSubmit> submitList = new List<AssignmentSubmit>();

            // db에서 해당 과제를 찾습니다
            query = "SELECT * FROM assignmentsubmit WHERE room_id = @room_id AND assignment_id = @assignment_id;";
            var parametersAssignment = new DynamicParameters();
            parametersAssignment.Add("room_id", RoomId);
            parametersAssignment.Add("assignment_id", AssignmentId);
            var assignmentSubmits = connection.Query<AssignmentSubmit>(query, parametersAssignment);
           foreach (AssignmentSubmit assignmentSubmit in assignmentSubmits) {
                submitList.Add(assignmentSubmit);
           }
            return submitList;
        }


        // 제출 과제의 평가를 수정합니다
        [HttpPut("{SubmitId}/score/{Score}/message/{Message}")]
        public async Task ModifyScore(int SubmitId, int Score, string Message) {

            using var connection = new NpgsqlConnection(connectionString);
            var modifyQuery = "Update assignmentsubmit SET score = @score, message = @message WHERE submit_id = @submit_id";
            var parameters = new DynamicParameters();
            parameters.Add("submit_id", SubmitId);
            parameters.Add("score", Score);
            parameters.Add("message", Message);
            connection.Execute(modifyQuery, parameters);
        }
    }
}
