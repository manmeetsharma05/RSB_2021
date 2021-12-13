using Microsoft.Reporting.WebForms;
using System.Security.Principal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace Rajya_Sanik_Board.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/
        [Serializable]
        public sealed class ReportServerNetworkCredentials : IReportServerCredentials
        {
            /// <summary>
            /// Provides forms authentication to be used to connect to the report server.
            /// </summary>
            /// <param name="authCookie">A Report Server authentication cookie.</param>
            /// <param name="userName">The name of the user.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="authority">The authority to use when authenticating the user, such as a Microsoft Windows domain.</param>
            /// <returns></returns>
            public bool GetFormsCredentials(out System.Net.Cookie authCookie, out string userName, out string password, out string authority)
            {
                authCookie = null;
                userName = null;
                password = null;
                authority = null;
                return false;
            }
            /// <summary>
            /// Specifies the user to impersonate when connecting to a report server.
            /// </summary>
            /// <value></value>
            /// <returns>A WindowsIdentity object representing the user to impersonate.</returns>
            public WindowsIdentity ImpersonationUser
            {
                get { return null; }
            }
            /// <summary>
            /// Returns network credentials to be used for authentication with the report server.
            /// </summary>
            /// <value></value>
            /// <returns>A NetworkCredentials object.</returns>
            public System.Net.ICredentials NetworkCredentials
            {
                get
                {
                    string usid = ConfigurationManager.AppSettings["RS_uid"];
                    string pswd = ConfigurationManager.AppSettings["RS_pwd"];
                    string domainName = ConfigurationManager.AppSettings["RS_DomainNm"].ToString();
                    return new System.Net.NetworkCredential(usid, pswd, domainName);
                }
            }
        }
        public JsonResult CallReport(string armyno, ReportViewer Rptv)
        {
            try
            {
                if (armyno.ToString() != "")
                {
                    Rptv.ServerReport.ReportServerUrl = new System.Uri(System.Configuration.ConfigurationManager.AppSettings["RS_ServerURI"].ToString().Trim());
                    Rptv.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Remote;
                    Rptv.ServerReport.ReportServerCredentials = new ReportServerNetworkCredentials();
                    Rptv.ServerReport.ReportPath = "/RSB/Sanikperdetail";
                    Microsoft.Reporting.WebForms.ReportParameter[] RptParameters;
                    RptParameters = new Microsoft.Reporting.WebForms.ReportParameter[1];
                    RptParameters[0] = new Microsoft.Reporting.WebForms.ReportParameter("Army_No", Convert.ToString(armyno));
                    Rptv.ShowParameterPrompts = false;
                    Rptv.ServerReport.SetParameters(RptParameters);
                    Rptv.ShowParameterPrompts = false;
                    //string mimeType;
                    //string encoding;
                    //string fileNameExtension;
                    //Warning[] warnings;
                    //string[] streamids;
                    //byte[] exportBytes = Rptv.ServerReport.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streamids, out warnings);
                    //HttpContext.Current.Response.Buffer = true;
                    //HttpContext.Current.Response.Clear();
                    //HttpContext.Current.Response.ContentType = mimeType;
                    //HttpContext.Current.Response.AddHeader("content-disposition", "inline; filename=ExportedReport." + fileNameExtension);
                    //HttpContext.Current.Response.BinaryWrite(exportBytes);
                    //HttpContext.Current.Response.Flush();
                    //HttpContext.Current.Response.End();

                }
                return Json(JsonRequestBehavior.AllowGet); 
            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Success = "False", responseText = ex }, JsonRequestBehavior.AllowGet);
                
            }
        }
       
        public ActionResult DistviseReport()
        {
            return View();
        }

    }
}
