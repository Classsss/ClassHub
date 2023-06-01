using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassHub.Shared {
    public class LectureMaterial {
        [JsonIgnore]
        private readonly HttpClient? Http;
        [JsonIgnore]
        private List<IBrowserFile>? fileList;

        public int room_id { get; set; } // 강의실 번호
        public int week { get; set; } // 강의자료 활용 주차
        public int material_id { get; set; } // 강의자료 번호
        public string title { get; set; } = string.Empty; // 게시글 제목
        public string author { get; set; } = string.Empty; // 작성자
        public string contents { get; set; } = string.Empty; // 게시글 내용
        public DateTime publish_date { get; set; } = DateTime.Now; // 게시일
        public DateTime up_date { get; set; } = DateTime.Now; // 업데이트일
        public int view_count { get; set; } = 0; // 조회수

        public LectureMaterial() {

        }

        public LectureMaterial(HttpClient httpClient, List<IBrowserFile> fileList) {
            Http = httpClient;
            this.fileList = fileList;
        }

        public async Task PostLectureMaterialAsync() {
            if(Http == null) throw new Exception("Http is null");

            // 게시글 생성 요청
            string requestUri = "/api/classroom/register/lecturematerial";
            string jsonString = JsonSerializer.Serialize(this);
            HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await Http.PostAsync(requestUri, httpContent);
            if(!response.IsSuccessStatusCode) throw new Exception($"Error uploading lecturematerial: {response.StatusCode}");
            else {
                string json = await response.Content.ReadAsStringAsync();
                material_id = JsonSerializer.Deserialize<int>(json);
            }

            await UploadAttachments();
        }

        public async Task PutLectureMaterialAsync() {
            if(Http == null) throw new Exception("Http is null");

            // 게시글 수정 요청
            string requestUri = "/api/classroom/modify/lecturematerial";
            string jsonString = JsonSerializer.Serialize(this);
            HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await Http.PutAsync(requestUri, httpContent);
            if(!response.IsSuccessStatusCode) throw new Exception($"Error modify lecturematerial: {response.StatusCode}");

            await UploadAttachments();
        }

        public async Task DeleteLectureMaterialAsync() {
			if(Http == null) throw new Exception("Http is null");

			// 게시글 수정 요청
			string requestUri = $"/api/classroom/{room_id}/delete/lecturematerial/{material_id}";
			await Http.DeleteAsync(requestUri);
		}

        private async Task UploadAttachments() {
            if(fileList != null && fileList.Count != 0) {
                // 첨부파일 등록 요청
                var content = new MultipartFormDataContent();
                foreach(var file in fileList) {
                    var buffer = new byte[file.Size];
                    await file.OpenReadStream(maxAllowedSize: 30L * 1024 * 1024).ReadAsync(buffer); // 30MB까지 허용
                    content.Add(new ByteArrayContent(buffer), "files", file.Name);
                }

                var response = await Http.PostAsync($"api/classroom/{room_id}/upload/lecturematerial/{material_id}", content);
                if(!response.IsSuccessStatusCode) throw new Exception($"Error uploading file: {response.StatusCode}");
            }
        }
    }
}
