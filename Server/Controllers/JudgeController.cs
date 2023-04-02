﻿using ClassHub.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace ClassHub.Server.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class JudgeController : ControllerBase {
        // POST <JudgeController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JudgeRequest request) {
            // 채점 서버에 채점 요청
            HttpClient Http = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await Http.PostAsync("https://classhubjudge.azurewebsites.net/Judge", content);

            // Post 요청 및 응답 받기 성공
            if (response.IsSuccessStatusCode) {
                Console.WriteLine("Data posted successfully");

                JudgeResult? judgeResult = await response.Content.ReadFromJsonAsync<JudgeResult>();

                // Valid response
                if (judgeResult != null) {
                    return Ok(judgeResult);
                }
                // Invalid response
                else {
                    // TODO : JudgeServer로 보낸 Post 요청이 실패했을 때 처리
                    Console.WriteLine("Invalid response");
                    return BadRequest("JudgeServer와 통신 실패");
                }
            } 
            // Post 요청 실패
            else {
                Console.WriteLine("Failed to post data " + response.StatusCode);
                // TODO : JudgeServer로 보낸 Post 요청이 실패했을 때 처리
                return BadRequest("JudgeServer와 통신 실패");
            }
        }
    }
}