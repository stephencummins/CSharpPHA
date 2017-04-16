using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;

namespace CSOMWebFormsWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            Uri redirectUrl;
            switch (SharePointContextProvider.CheckRedirectionStatus(Context, out redirectUrl))
            {
                case RedirectionStatus.Ok:
                    return;
                case RedirectionStatus.ShouldRedirect:
                    Response.Redirect(redirectUrl.AbsoluteUri, endResponse: true);
                    break;
                case RedirectionStatus.CanNotRedirect:
                    Response.Write("An error occurred while processing your request.");
                    Response.End();
                    break;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);

            //Host Web
            using (var hostContext = spContext.CreateUserClientContextForSPHost())
            {
                hostContext.Load(hostContext.Web, web => web.Title);
                hostContext.ExecuteQuery();
                hostWebTitle.Text = hostContext.Web.Title;
            }

            //App Web
            using (var appContext = spContext.CreateUserClientContextForSPAppWeb())
            {
                appContext.Load(appContext.Web, web => web.Title);
                appContext.ExecuteQuery();
                appWebTitle.Text = appContext.Web.Title;
            }

            //PWA Context
            var contextToken = TokenHelper.GetContextTokenFromRequest(Page.Request);
            var hostWeb = Page.Request["SPHostUrl"];
            using (var projectContext = TokenHelper.GetProjectContextWithContextToken(hostWeb, contextToken, Request.Url.Authority))
            {
                // Get the list of projects on the server.
                projectContext.Load(projectContext.Projects);
                projectContext.ExecuteQuery();

                foreach (PublishedProject pubProj in projectContext.Projects)
                {
                    Response.Write(string.Format("{0} :{1} : {2} &lt;/br&gt;", 
                        pubProj.Id.ToString(), pubProj.Name, pubProj.CreatedDate.ToString()
                        ));
                }

                //Project Tasks
                projectContext.Load(projectContext.Projects,
                Pro => Pro.IncludeWithDefaultProperties(projectDetail => projectDetail.StartDate, 
                projectDetail => projectDetail.FinishDate, projectDetail => projectDetail.Tasks, 
                projectDetail => projectDetail.PercentComplete, projectDetail => projectDetail.ProjectResources, 
                projectDetail => projectDetail.CustomFields, projectDetail => projectDetail.EnterpriseProjectType));
                projectContext.ExecuteQuery();
                foreach (PublishedProject Project in projectContext.Projects)
                {
                    foreach (var Multask in Project.Tasks)
                    {
                        var current = DateTime.Now.Date;
                        var previous = Convert.ToDateTime(Multask.Start).Date;

                        if (previous == current)
                        {
                            Response.Write(string.Format("{0} :{1} : {2} &lt;/br&gt;", 
                                Project.Name,
                                Multask.Name, 
                                Multask.Start,
                                Multask.Finish,
                                Multask.PercentComplete
                                ));
                        }
                    }
                }

            }

        }

        }
    }