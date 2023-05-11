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
       
            [HttpPost]
            public async Task<IActionResult> Post([FromBody] Tuple<JudgeRequest, CodeSubmit> request){
                //튜플에서 데이터 분리
                var judgeData = request.Item1;
                var submitData = request.Item2;

                // 채점중인 레코드를 insert 
                using var httpClient = new HttpClient();
                var contentSubmit = new StringContent(JsonSerializer.Serialize(submitData), Encoding.UTF8, "application/json");
                var responseSubmit = await httpClient.PostAsync("https://localhost:7182/api/CodeSubmit/insert", contentSubmit);

                int submitId = await responseSubmit.Content.ReadFromJsonAsync<int>();

                // 채점 서버에 채점 요청
                judgeData.SubmitId = submitId;
                var contentJudge = new StringContent(JsonSerializer.Serialize(judgeData), Encoding.UTF8, "application/json");
                var responseJudge = await httpClient.PostAsync("https://localhost:7135/Judge", contentJudge);

                // Post 요청 및 응답 받기 성공
                if (responseJudge.IsSuccessStatusCode){

                    Console.WriteLine("Data posted successfully");

                    JudgeResult? judgeResult = await responseJudge.Content.ReadFromJsonAsync<JudgeResult>();

                    // Valid response
                    if (judgeResult != null){
                        Tuple<JudgeResult, int> result = new Tuple<JudgeResult, int>(judgeResult, submitId);
                        //채점 완료 후 db 업데이트 요청
                        var contentResult = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json");
                        await httpClient.PutAsync("https://localhost:7182/api/CodeSubmit/update", contentResult);
                        return Ok(judgeResult);
                    }
                    // Invalid response
                    else{
                        // TODO : JudgeServer로 보낸 Post 요청이 실패했을 때 처리
                        Console.WriteLine("Invalid response");
                        return BadRequest("JudgeServer와 통신 실패");
                    }
                }
                // Post 요청 실패
                else{
                    Console.WriteLine("Failed to post data " + responseJudge.StatusCode);
                    // TODO : JudgeServer로 보낸 Post 요청이 실패했을 때 처리
                    return BadRequest("JudgeServer와 통신 실패");
                }

            }
        }
    }
}

