using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TodoApps
{
    public class FirebaseService
    {
        private const string FirebaseTasksUrl = "https://todoap-c6fa1-default-rtdb.firebaseio.com/tasks.json";
        private readonly string _apiKey;
        private static readonly HttpClient http = new HttpClient();

        public FirebaseService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<FirebaseAuthResponse?> SignInAsync(string email, string password)
        {
            var payload = new { email, password, returnSecureToken = true };
            var json = JsonSerializer.Serialize(payload);
            var resp = await http.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}",
                new StringContent(json, Encoding.UTF8, "application/json")
            );
            if (!resp.IsSuccessStatusCode) return null;
            var respJson = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FirebaseAuthResponse>(respJson);
        }

        public async Task<bool> SignUpAsync(string email, string password)
        {
            var payload = new { email, password, returnSecureToken = true };
            var json = JsonSerializer.Serialize(payload);
            var resp = await http.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}",
                new StringContent(json, Encoding.UTF8, "application/json")
            );
            return resp.IsSuccessStatusCode;
        }

        public async Task<Dictionary<string, TodoTask>?> GetTasksAsync()
        {
            var response = await http.GetStringAsync(FirebaseTasksUrl);
            if (string.IsNullOrWhiteSpace(response) || response == "null") return null;
            return JsonSerializer.Deserialize<Dictionary<string, TodoTask>>(response);
        }

        public async Task<string?> AddTaskAsync(TodoTask task)
        {
            var json = JsonSerializer.Serialize(task);
            var resp = await http.PostAsync(FirebaseTasksUrl, new StringContent(json, Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode) return null;
            var result = await resp.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            return obj?["name"];
        }

        public async Task<bool> UpdateTaskAsync(TodoTask task)
        {
            var json = JsonSerializer.Serialize(task);
            var resp = await http.PutAsync(
                $"https://todoap-c6fa1-default-rtdb.firebaseio.com/tasks/{task.FirebaseId}.json",
                new StringContent(json, Encoding.UTF8, "application/json")
            );
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTaskAsync(string firebaseId)
        {
            var resp = await http.DeleteAsync(
                $"https://todoap-c6fa1-default-rtdb.firebaseio.com/tasks/{firebaseId}.json"
            );
            return resp.IsSuccessStatusCode;
        }
    }
}