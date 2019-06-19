using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Reminder.Storage.Core;
using Reminder.Storage.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

namespace Reminder.Storage.WebApi.Client
{
	public class ReminderStorageWebApiClient : IReminderStorage
	{
		public string _baseWebApiUrl;
		private System.Net.Http.HttpClient _httpCient;

		public ReminderStorageWebApiClient(string baseWebApiUrl)
		{
			_baseWebApiUrl = baseWebApiUrl;
			_httpCient = HttpClientFactory.Create();
		}

		public Guid Add(ReminderItemRestricted reminder)
		{
			var result = CallWebApi(
				"POST", 
				"/api/reminders", 
				JsonConvert.SerializeObject(new ReminderItemCreateModel(reminder)));

			if (result.StatusCode != System.Net.HttpStatusCode.Created)
			{
				throw CreateException(result);
			}

			string content = result.Content.ReadAsStringAsync().Result;
			return JsonConvert.DeserializeObject<ReminderItemGetModel>(content).Id;
		}

		public ReminderItem Get(Guid id)
		{
			var result = CallWebApi(
				"GET",
				$"/api/reminders/{id}");

			string content = result.Content.ReadAsStringAsync().Result;

			if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}

			if (result.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw CreateException(result);
			}

			return JsonConvert.DeserializeObject<ReminderItemGetModel>(content).ToReminderItem();
		}

		public List<ReminderItem> Get(ReminderItemStatus status)
		{
			var result = CallWebApi(
				"GET",
				$"/api/reminders?[filter]status={(int)status}");

			string content = result.Content.ReadAsStringAsync().Result;

			if (result.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw CreateException(result);
			}

			return JsonConvert.DeserializeObject<List<ReminderItemGetModel>>(content)
				.Select(x => x.ToReminderItem())
				.ToList();
		}

        public void UpdateStatus(IEnumerable<Guid> ids, ReminderItemStatus status)
        {
            var contentModel = new ReminderItemsUpdateModel
            {
                Ids = ids.ToList(),
                PatchDocument = new JsonPatchDocument<ReminderItemUpdateModel>(
                    new List<Operation<ReminderItemUpdateModel>>
                    {
                    new Operation<ReminderItemUpdateModel>
                    {
                        op = "replace",
                        path = "/status",
                        value = (int)status
                    }
                    },
                    new DefaultContractResolver())
            };

			var result = CallWebApi(
				"PATCH",
				"/api/reminders",
				JsonConvert.SerializeObject(contentModel));

			if (result.StatusCode != System.Net.HttpStatusCode.NoContent)
			{
				throw CreateException(result);
			}
		}

		public void UpdateStatus(Guid id, ReminderItemStatus status)
		{
			var patchDocument = new JsonPatchDocument<ReminderItemUpdateModel>(
				new List<Operation<ReminderItemUpdateModel>>
			{
				new Operation<ReminderItemUpdateModel>
				{
					op = "replace",
					path = "/status",
					value = (int)status
				}
			},
				new Newtonsoft.Json.Serialization.DefaultContractResolver());

			var content = JsonConvert.SerializeObject(patchDocument);

			var result = CallWebApi(
				"PATCH",
				"/api/reminders/{id}",
				content);

			if (result.StatusCode != System.Net.HttpStatusCode.NoContent)
			{
				throw CreateException(result);
			}
		}

        public int Count
        {
            get
            {
                var httpResponseMessage = CallWebApi("HEAD", "/api/reminders", null);
                if (httpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw GetException(httpResponseMessage);
                }

                const string totalCountHeaderName = "X-Total-Count";
                if (!httpResponseMessage.Headers.Contains(totalCountHeaderName))
                {
                    throw new Exception($"There is no expected header '{totalCountHeaderName}' found");
                }

                string xTotalCountHeader = httpResponseMessage.Headers
                    .GetValues(totalCountHeaderName)
                    .First();

                return int.Parse(xTotalCountHeader);
            }
        }

        public List<ReminderItem> Get(int count, int startPostion)
        {
            var queryParams = new List<KeyValuePair<string, string>>();

            if (count > 0)
                queryParams.Add(new KeyValuePair<string, string>("[paging]count", count.ToString()));

            if (startPostion > 0)
                queryParams.Add(new KeyValuePair<string, string>("[paging]startPostion", startPostion.ToString()));

            var httpResponseMessage = CallWebApi("GET", "/api/reminders" + BuildQueryString(queryParams));

            if (httpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw GetException(httpResponseMessage);
            }

            var list = JsonConvert.DeserializeObject<List<ReminderItemGetModel>>(
                httpResponseMessage.Content.ReadAsStringAsync().Result);

            if (list == null)
                throw new Exception($"Body cannot be parsed as List<ReminderItemGetModel>.");

            return list
                .Select(m => m.ToReminderItem())
                .ToList();
        }

        private HttpResponseMessage CallWebApi(
			string method,
			string relativeUrl,
			string content = null)
		{
			var request = new HttpRequestMessage(
				new HttpMethod(method),
				_baseWebApiUrl + relativeUrl);

			request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

			if (method == "POST" || method == "PATCH" || method == "PUT")
			{
				request.Content = new StringContent(
					content,
					Encoding.UTF8,
					"application/json");
			}

			return _httpCient.SendAsync(request).Result;
		}

		private Exception CreateException(HttpResponseMessage httpResponseMesage)
		{
			return new Exception($"Error: {httpResponseMesage.StatusCode}, " +
					$"Content: {httpResponseMesage.Content.ReadAsStringAsync().Result}");
		}

        public bool Remove(Guid id)
        {
            var httpResponseMessage = CallWebApi("DELETE", $"/api/reminders/{id}");

            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            if (httpResponseMessage.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw GetException(httpResponseMessage);
            }

            return true;
        }

        private Exception GetException(HttpResponseMessage result)
        {
            return new Exception(
                $"Error: {result.StatusCode}, " +
                $"Content: {result.Content.ReadAsStringAsync().Result}");
        }

        private string BuildQueryString(List<KeyValuePair<string, string>> queryParams)
        {
            if (queryParams?.Count == 0)
                return string.Empty;

            return "?" + string.Join(
                "&",
                queryParams
                    .Select(kvp => kvp.Key + "=" + kvp.Value)
                    .ToArray());
        }
    }
}
