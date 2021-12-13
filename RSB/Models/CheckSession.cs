using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rajya_Sanik_Board.Models
{
    public class SessionTimeoutAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            if (HttpContext.Current.Session["userid"] == null)
            {
                filterContext.Result = new RedirectResult("~/Home/Login");

                return;

            }
            base.OnActionExecuting(filterContext);
        }
    }

}