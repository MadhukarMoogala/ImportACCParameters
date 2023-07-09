using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace ImportACCParameters
{
    internal class ParameterHandler
    {
        private static readonly string accountId = "489c5e7a-c6c0-4212-81f3-3529a621210b";
        private static readonly string groupName = "My 3nd Sample Group";
        private static readonly string collectionName = "ACC-AutoCAD-Parameters";
        public static List<ParametersResult> ParametersServiceRequest(HttpClient client)
        {

            var task = Task.Run(async () =>
            {
                return await ParametersServiceRequestAsync(client);
            });
            return task.GetAwaiter().GetResult();

        }

        public static async Task<List<ParametersResult>> ParametersServiceRequestAsync(HttpClient client)
        {
            var groupId = string.Empty;
            var collectionId = string.Empty;
            List<ParametersResult> parametersList;
            var groupRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(client.BaseAddress,$"accounts/{accountId}/groups")               
                
            };
            using (var response = await client.SendAsync(groupRequest))
            {
                response.EnsureSuccessStatusCode();
                var groupResponse = await response.Content.ReadAsStringAsync();
                Groups groups = JsonConvert.DeserializeObject<Groups>(groupResponse);
                var group = groups.Results.Where<GroupResult>(item => item.Title == groupName).FirstOrDefault();
                groupId = group.Id;                
            }
            var collectionRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(client.BaseAddress,$"accounts/{accountId}/groups/{groupId}/collections")
            };
            using (var response = await client.SendAsync(collectionRequest))
            {
                response.EnsureSuccessStatusCode();
                var collectionResponse = await response.Content.ReadAsStringAsync();
                ParamCollection paramCollection = JsonConvert.DeserializeObject<ParamCollection>(collectionResponse);
                var collection = paramCollection.Results.Where<CollectionResult>(item => item.Title == collectionName).FirstOrDefault();
                collectionId = collection.Id;                
            }
            var parameterRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(client.BaseAddress,$"accounts/{accountId}/groups/{groupId}/collections/{collectionId}/parameters")
            };

            using (var response = await client.SendAsync(parameterRequest))
            {
                response.EnsureSuccessStatusCode();
                var collectionResponse = await response.Content.ReadAsStringAsync();                
                Parameters parameters = JsonConvert.DeserializeObject<Parameters>(collectionResponse);
                parametersList = new List<ParametersResult>(parameters.Results);                
            }
            return parametersList;

        }
    }

    public class Pagination
    {
        public int Offset { get; set; }
        public int Limit { get; set; }
        public string NextUrl { get; set; }
    }

    public class GroupResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class Groups
    {
        public Pagination Pagination { get; set; }
        public List<GroupResult> Results { get; set; }
    }

    public class Account
    {
        public string Id { get; set; }
    }

    public class Group
    {
        public string Id { get; set; }
    }

    public class CollectionResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Group Group { get; set; }
        public Account Account { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ParamCollection
    {
        public Pagination Pagination { get; set; }
        public List<CollectionResult> Results { get; set; }
    }

    public class Metadata
    {
        public string Id { get; set; }
        public object Value { get; set; }
    }

    public class ParametersResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool ReadOnly { get; set; }
        public string Description { get; set; }
        public string SpecId { get; set; }
        public List<Metadata> Metadata { get; set; }
        public string Creator { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Parameters
    {
        public Pagination Pagination { get; set; }
        public List<ParametersResult> Results { get; set; }
    }
}

