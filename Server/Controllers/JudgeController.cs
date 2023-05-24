using ClassHub.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace ClassHub.Server.Controllers
{
    namespace ClassHub.Server.Controllers
    {
        [Route("[controller]")]
        [ApiController]
        public class JudgeController : ControllerBase
        {
            private const string CLASSHUB_CODESUBMIT_INSERT = "https://classhub.azurewebsites.net/api/CodeSubmit/insert";
            private const string CLASSHUB_CODESUBMIT_UPDATE = "https://classhub.azurewebsites.net/api/CodeSubmit/update";
            private const string CLASSHUB_CODESUBMIT_FAIL = "https://classhub.azurewebsites.net/api/CodeSubmit/fail";
            private const string JUDGESERVER_ADDRESS = "http://20.196.209.129:5000/Judge";
       
            [HttpPost]
            public async Task<IActionResult> Post([FromBody] RequestSubmitContainer request){
                //튜플에서 데이터 분리
                var judgeData = request.JudgeRequest;
                var submitData = request.CodeSubmit;

                // 초기의 채점 내역을 insert하기 위해 submitController에 요청 
                using var httpClient = new HttpClient();
                var contentSubmit = new StringContent(JsonSerializer.Serialize(submitData), Encoding.UTF8, "application/json");
                var responseSubmit = await httpClient.PostAsync(CLASSHUB_CODESUBMIT_INSERT, contentSubmit);

                // 생성된 submit_id를 받아준다.
                int submitId = await responseSubmit.Content.ReadFromJsonAsync<int>();

                // 채점 서버에 채점 요청한다.
                judgeData.SubmitId = submitId;
                var contentJudge = new StringContent(JsonSerializer.Serialize(judgeData), Encoding.UTF8, "application/json");
                try {
                    var responseJudge = await httpClient.PostAsync(JUDGESERVER_ADDRESS, contentJudge);

                    // Post 요청 및 응답 받기 성공
                    if (responseJudge.IsSuccessStatusCode) {

                        Console.WriteLine("Data posted successfully");

                        JudgeResult? judgeResult = await responseJudge.Content.ReadFromJsonAsync<JudgeResult>();

                        // 채점 성공
                        if (judgeResult != null) {
                            Tuple<JudgeResult, int> result = new Tuple<JudgeResult, int>(judgeResult, submitId);
                            //채점 완료 후 db 업데이트 요청
                            var contentResult = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json");
                            await httpClient.PutAsync(CLASSHUB_CODESUBMIT_UPDATE, contentResult);
                            return Ok(judgeResult);
                        }
                        // 옮지 않은 반응(채점 실패)
                        else {
                            Console.WriteLine("Invalid response");
                            var contentResult = new StringContent(JsonSerializer.Serialize(submitId), Encoding.UTF8, "application/json");
                            await httpClient.PutAsync(CLASSHUB_CODESUBMIT_FAIL, contentResult);
                            return BadRequest("JudgeServer와 통신 실패");
                        }
                    }
                    // Post 요청 실패
                    else {
                        Console.WriteLine("Failed to post data " + responseJudge.StatusCode);
                        var contentResult = new StringContent(JsonSerializer.Serialize(submitId), Encoding.UTF8, "application/json");
                        await httpClient.PutAsync(CLASSHUB_CODESUBMIT_FAIL, contentResult);
                        return BadRequest("JudgeServer와 통신 실패");
                    }
                }catch(Exception ex) {      // 채점 서버 무응답시 실패 처리
                    Console.WriteLine("Failed to post data " + ex);
                    var contentResult = new StringContent(JsonSerializer.Serialize(submitId), Encoding.UTF8, "application/json");
                    await httpClient.PutAsync(CLASSHUB_CODESUBMIT_FAIL, contentResult);
                    return BadRequest("JudgeServer와 통신 실패");
                }
            }
        }
    }
}

