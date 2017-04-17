using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml.Linq;

namespace RESTMVCWeb.Controllers
{
    public class HomeController : Controller
    {
        [SharePointContextFilter]
        public async Task<ActionResult> Index()
        {



            SharePointAcsContext spContext = (SharePointAcsContext)SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            string accessToken = spContext.UserAccessTokenForSPHost;
            System.Web.HttpContext.Current.Session["AccessToken"] = accessToken;
            System.Web.HttpContext.Current.Session["SPHostUrl"] = spContext.SPHostUrl;

            //Host Web
            string hostToken = spContext.UserAccessTokenForSPHost;

            StringBuilder hostRequestUri = new StringBuilder()
            .Append(spContext.SPHostUrl)
            .Append("_api/web/title");

            ViewBag.hostTitle = await GetWebTitle(hostRequestUri, hostToken);

            //App Web
            string appToken = spContext.UserAccessTokenForSPAppWeb;

            StringBuilder appRequestUri = new StringBuilder()
            .Append(spContext.SPAppWebUrl)
            .Append("_api/web/title");

            ViewBag.appTitle = await GetWebTitle(appRequestUri, appToken);
          
            //Project Context
            using (var projectContext = ContextHelper.GetClientContext(Request.Url))
            {
                // Query the Project Context
                //projectContext.Load(projectContext.Projects);
               // projectContext.ExecuteQuery();

                projectContext.Load(projectContext.Web, web => web.Title);
                projectContext.ExecuteQuery();
                ViewBag.Message = projectContext.Web.Title;

            }
            return View();
        }

        private static async Task<string> GetWebTitle(StringBuilder requestUri, string accessToken)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri.ToString());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            XElement root = XElement.Parse(responseString);
            return root.Value;
        }

    }
}