using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace BussinessErp.BLL
{
    public class BusinessContext
    {
        public decimal TodaySales { get; set; }
        public List<dynamic> TopCustomers { get; set; }
        public List<dynamic> TopSuppliers { get; set; }
        public List<dynamic> SlowMovers { get; set; }
        public string SalesForecast { get; set; }
    }

    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        // Changing to a stable model (gemini-1.5-flash) to avoid 404 errors
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";


        public GeminiService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            // Ensure TLS 1.2 for modern API compatibility
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }

        public async Task<string> SendPromptAsync(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var serializer = new JavaScriptSerializer();
                var jsonRequest = serializer.Serialize(requestBody);

                var content = new StringContent(
                    jsonRequest,
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    ApiUrl + "?key=" + _apiKey,
                    content);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return "Error: " + response.StatusCode + "\n" + jsonResponse;
                }

                var data = serializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                if (data.ContainsKey("candidates"))
                {
                    var candidatesList = (System.Collections.IList)data["candidates"];
                    if (candidatesList.Count > 0)
                    {
                        var candidate = (Dictionary<string, object>)candidatesList[0];
                        var contentObj = (Dictionary<string, object>)candidate["content"];
                        var partsList = (System.Collections.IList)contentObj["parts"];
                        var part = (Dictionary<string, object>)partsList[0];
                        return part["text"].ToString();
                    }
                }

                return "No response from AI.";
            }
            catch (Exception ex)
            {
                return "Exception: " + ex.Message;
            }
        }

        public async Task<string> GetReasoningAsync(string question, BusinessContext context)
        {
            var prompt = $"Analyst Context:\nTotal Today Sales: {context.TodaySales}\nQuestion: {question}";
            return await SendPromptAsync(prompt);
        }
    }
}
