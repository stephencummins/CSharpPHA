using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;

namespace RESTMVCWeb.Controllers
{
    public class WelcomeController : ApiController
    {
        public async Task<IHttpActionResult> Get()
        {
            //Retrieve token
            string accessToken = System.Web.HttpContext.Current.Session["AccessToken"].ToString();
            System.Web.HttpContext.Current.Session.Remove("AccessToken");
            string spHostUrl = System.Web.HttpContext.Current.Session["SPHostUrl"].ToString();
            System.Web.HttpContext.Current.Session.Remove("SPHostUrl");

            //Call SharePoint
            StringBuilder requestUri = new StringBuilder()
                .Append(spHostUrl)
                .Append("_api/web/currentuser");

            // Set User Context


            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri.ToString());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            XElement root = XElement.Parse(responseString);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            string title = root.Descendants(d + "Title").First().Value;

            return Ok<string>(title);

        }
    }
}