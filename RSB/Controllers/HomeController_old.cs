using Rajya_Sanik_Board.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Data.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;
//using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;
using ReportManagement;

using CaptchaMvc.HtmlHelpers;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;




namespace Rajya_Sanik_Board.Controllers
{

    public class HomeController : PdfViewController
    {
        public string Constrg = ConfigurationManager.ConnectionStrings["edistrictConnectionString1"].ToString();

        public string Constrg1 = ConfigurationManager.ConnectionStrings["RSBConnectionString1"].ToString();
        Generalform mydata = new Generalform();
        rsbgeneralUPDataContext dta = new rsbgeneralUPDataContext();
        edistrictDataContext ed = new edistrictDataContext();

        //convert password into md5
        //[HttpPost]
        //public JsonResult getmd5(string pswd)
        //{
        //    System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
        //    byte[] bs = System.Text.Encoding.UTF8.GetBytes(pswd);
        //    bs = x.ComputeHash(bs);
        //    System.Text.StringBuilder s = new System.Text.StringBuilder();
        //    foreach (byte b in bs)
        //    {
        //        s.Append(b.ToString("x2").ToLower());
        //    }
        //    string pswd1=s.ToString();
        //     return Json(new {pswd1}, JsonRequestBehavior.AllowGet);;
        //}
        //end
        //public JsonResult DestroySession()
        //{
        //    Session["userid"] = null;
        //    string code = "distroy";
        //    return Json(new { code }, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult Login(string id)
        {
            //Session["userid"] = null;
            //Session["usertype"] = null;
            //Session.Clear();
            //Session.Abandon();
            if (id != null)
            {
                string str = "";
                try
                {
                     str = DecryptKey(id);
                     if (str == "")
                     {
                         return Redirect("http://citizen.edisha.gov.in/");
                     }
                     if (!string.IsNullOrEmpty(str))
                     {
                         bool check = ed.tblCitizens.Any(tbl => tbl.Email == str);
                         if (check == true)
                         {
                             Session["userid"] = str;
                             Session["usertype"] = "0";
                             return RedirectToAction("RSBGeneralInfrm_citizen");
                         }
                         else
                         {
                             return Redirect("http://citizen.edisha.gov.in/");
                         }
                     }
                     else
                     {
                         return Redirect("http://citizen.edisha.gov.in/");
                     }
                  
                }
                catch(Exception ex)
                {

                    return Redirect("http://citizen.edisha.gov.in/");
                }
              
            }
            else
            {
                //return Redirect("http://citizen.edisha.gov.in/");
                return View();
            }

            //return View();

        }
        [HttpPost]
        public JsonResult login1(Generalform mydata)
        {
            Session["citizensearch"] = "";

            if (Session["userid"] != null)
            {
                Session["userid"] = null;
                string code = "yes";
                return Json(new { code }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string captcha = Convert.ToString(Session["Captcha"]);
                if (captcha != null && mydata.Captcha != null)
                {
                    if (captcha == mydata.Captcha)
                    {
                        //string pswd = FormsAuthentication.HashPasswordForStoringInConfigFile(mydata.pswd, "MD5").Trim();
                        bool check = ed.tblLogins.Any(tbl => tbl.UserID == mydata.userid.Trim() && tbl.Password == mydata.pswd);
                        if (check == true)
                        {
                            string output = "login";

                            var userid = from user in ed.tblLogins where user.UserID == mydata.userid.Trim() select user;
                            var id = userid.FirstOrDefault().UserType;
                            var usn = userid.FirstOrDefault().UserID;
                            var dcode = userid.FirstOrDefault().dcode;

                            Session["userid"] = usn;
                            Session["usertype"] = id;
                            if ((string)Session["usertype"] == "72")
                            {
                                Session["dcode"] = dcode;
                                var dname = from dcd in ed.tblDistrictMasters where dcd.dcode == dcode select dcd;
                                Session["dname"] = dname.FirstOrDefault().dname;
                            }
                            else if ((string)Session["usertype"] == "70")
                            {
                                Session["dcode"] = dcode;
                                var dname = from dcd in ed.tblDistrictMasters where dcd.dcode == dcode select dcd;
                                Session["dname"] = dname.FirstOrDefault().dname;
                            }
                            else if ((string)Session["usertype"] == "71")
                            {
                                Session["dcode"] = "notdcode";
                                var dname = from dcd in ed.tblDistrictMasters where dcd.dcode == dcode select dcd;
                                Session["dname"] = dname.FirstOrDefault().dname;
                            }
                            else
                            {
                                Session["dcode"] = "notdcode";
                                Session["aarmyno"] = null;
                            }


                            return Json(new { output, id }, JsonRequestBehavior.AllowGet);
                        }

                        else
                        {
                            string output = "notlgn";
                            return Json(new { output }, JsonRequestBehavior.AllowGet);

                        }
                        return Json(JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        string output = "notmatch";
                        return Json(new { output }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    string output = "notnull";
                    return Json(new { output }, JsonRequestBehavior.AllowGet);
                }

            }
        }

        public ActionResult Logout()
        {
            Session["userid"] = null;
            Session["usertype"] = null;
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }
        public ActionResult changePassword()
        {
            if (Request.UrlReferrer == null)
            {
                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }
            else
            {
                return View();
            }

        }
        public JsonResult changePassword1(Generalform mydata)
        {
            bool check = ed.tblLogins.Any(tbl => tbl.UserID == mydata.userid.Trim());
            if (check == true)
            {
                string output = "login";

                tblLogin ln = new tblLogin();
                Tblpwdhistory pwdhs = new Tblpwdhistory();
                var login1 = (from lng in ed.tblLogins
                              where (lng.UserID == mydata.userid)
                              select lng).ToList();
                if (login1.FirstOrDefault().Password == mydata.oldpswd)
                {


                    if (mydata.hidennewpwd == mydata.hidenre)
                    {
                        //ln = ed.tblLogins.Where(x => x.UserID == mydata.userid).FirstOrDefault();
                        ln.UserChangePwd = 'y';
                        ln.Password = mydata.newpswd;
                        //ed.SubmitChanges();

                        pwdhs.uid = mydata.userid;
                        pwdhs.pwd = mydata.newpswd;
                        pwdhs.pwddatetime = DateTime.Now.Date;
                        //ed.Tblpwdhistories.InsertOnSubmit(pwdhs);
                        //ed.SubmitChanges();
                        var result = dta.changepassword(ln.UserChangePwd, ln.Password, pwdhs.uid, pwdhs.pwd, pwdhs.pwddatetime);
                        string output1 = "match";
                        return Json(new { output1 }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        string output3 = "new";
                        return Json(new { output3 }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    string output2 = "old";
                    return Json(new { output2 }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                string output = "notmatch";
                return Json(new { output }, JsonRequestBehavior.AllowGet);
            }



        }
        [SessionTimeout]
        public ActionResult Index()
        {
            if (Request.UrlReferrer == null)
            {
                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }
            else
            {
                string user = Convert.ToString(Session["userid"]);
                if (user != null && user != "")
                {

                    return View();

                }
                else
                {
                    return RedirectToAction("Login", "Home");
                }
            }


        }
        public JsonResult Index1()
        {
            string user = Convert.ToString(Session["userid"]);
            string usertype = Convert.ToString(Session["usertype"]);
            string dco = Convert.ToString(Session["dcode"]);
            return Json(new { user, usertype, dco }, JsonRequestBehavior.AllowGet);



        }

        public ActionResult Registration()
        {


            return View();
        }

        public ActionResult demo()
        {


            return View();
        }

        [HttpGet]
        public ActionResult martyr()
        {



            return View();
        }

        //validation check on server site!!!!!!!
        protected bool IsValidEmail(string sEmail)
        {
            if (sEmail == null)
            {
                bool val = true;
                return val;
            }
            else
            {
                string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                System.Text.RegularExpressions.Regex orgxEmail = new System.Text.RegularExpressions.Regex(strRegex);
                bool val = orgxEmail.IsMatch(sEmail);
                return val;
            }
        }

        protected bool IsNumber(string number)
        {
            if (number == null)
            {
                bool value = true;
                return value;
            }
            else
            {
                string strRegex = "^[0-9]+$";
                System.Text.RegularExpressions.Regex orgxEmail = new System.Text.RegularExpressions.Regex(strRegex);
                bool value = orgxEmail.IsMatch(number);
                return value;

            }

        }

        protected bool IsName(string name)
        {
            if (name == null)
            {
                bool value = true;
                return value;
            }
            else
            {
                string strRegex = "^[a-zA-Z ]*$";
                System.Text.RegularExpressions.Regex orgxEmail = new System.Text.RegularExpressions.Regex(strRegex);
                bool value = orgxEmail.IsMatch(name);
                return value;
            }
        }

        protected bool Citizen(string cid)
        {
            if (cid == null)
            {
                bool value = true;
                return value;
            }
            else
            {
                string strRegex = "(^[A-Za-z0-9]+$)|(^[0-9]+$)";
                System.Text.RegularExpressions.Regex orgxEmail = new System.Text.RegularExpressions.Regex(strRegex);
                bool value = orgxEmail.IsMatch(cid);
                return value;
            }
        }
        protected bool armynumber(string cid)
        {
            if (cid == null)
            {
                bool value = true;
                return value;
            }
            else
            {
                string strRegex = "(^[A-Za-z0-9-]+$)|(^[0-9]+$)";
                System.Text.RegularExpressions.Regex orgxEmail = new System.Text.RegularExpressions.Regex(strRegex);
                bool value = orgxEmail.IsMatch(cid);
                return value;
            }
        }
        protected bool Pancard(string cid)
        {
            if (cid == null)
            {
                bool value = true;
                return value;
            }
            else
            {
                string strRegex = @"[A-Za-z]{5}\d{4}[A-Za-z]{1}";
                System.Text.RegularExpressions.Regex orgxEmail = new System.Text.RegularExpressions.Regex(strRegex);
                bool value = orgxEmail.IsMatch(cid);
                return value;
            }
        }

        protected bool Decimalchk(string amount)
        {
            if (amount == "" || amount == null)
            {
                bool value = true;
                return value;
            }
            else
            {
                string strRegex = "^[0-9._ ]*$";
                System.Text.RegularExpressions.Regex orgxEmail = new System.Text.RegularExpressions.Regex(strRegex);
                bool value = orgxEmail.IsMatch(amount);
                return value;
            }
        }
        protected bool checkdate(string date)
        {
            if (date == "" || date == null)
            {
                bool value = true;
                return value;
            }
            else
            {

                // string strRegex = "^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[1-9]|[1][0-2])[./-]([0-9]{4}|[0-9]{2})$";  
                //string strRegex = @"^(19|20)\d\d[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])$";
                string strRegex = @"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$";
                System.Text.RegularExpressions.Regex orgxEmail = new System.Text.RegularExpressions.Regex(strRegex);
                bool value = orgxEmail.IsMatch(date);
                return value;
            }
        }
        //verhoff algorithm (aadhar)
        public static class Verhoeff
        {
            private static int[,] d = new int[10, 10]
  {
    {
      0,
      1,
      2,
      3,
      4,
      5,
      6,
      7,
      8,
      9
    },
    {
      1,
      2,
      3,
      4,
      0,
      6,
      7,
      8,
      9,
      5
    },
    {
      2,
      3,
      4,
      0,
      1,
      7,
      8,
      9,
      5,
      6
    },
    {
      3,
      4,
      0,
      1,
      2,
      8,
      9,
      5,
      6,
      7
    },
    {
      4,
      0,
      1,
      2,
      3,
      9,
      5,
      6,
      7,
      8
    },
    {
      5,
      9,
      8,
      7,
      6,
      0,
      4,
      3,
      2,
      1
    },
    {
      6,
      5,
      9,
      8,
      7,
      1,
      0,
      4,
      3,
      2
    },
    {
      7,
      6,
      5,
      9,
      8,
      2,
      1,
      0,
      4,
      3
    },
    {
      8,
      7,
      6,
      5,
      9,
      3,
      2,
      1,
      0,
      4
    },
    {
      9,
      8,
      7,
      6,
      5,
      4,
      3,
      2,
      1,
      0
    }
  };
            private static int[,] p = new int[8, 10]
  {
    {
      0,
      1,
      2,
      3,
      4,
      5,
      6,
      7,
      8,
      9
    },
    {
      1,
      5,
      7,
      6,
      2,
      8,
      3,
      0,
      9,
      4
    },
    {
      5,
      8,
      0,
      3,
      7,
      9,
      6,
      1,
      4,
      2
    },
    {
      8,
      9,
      1,
      6,
      0,
      4,
      3,
      5,
      2,
      7
    },
    {
      9,
      4,
      5,
      3,
      1,
      2,
      6,
      8,
      7,
      0
    },
    {
      4,
      2,
      8,
      6,
      5,
      7,
      3,
      9,
      0,
      1
    },
    {
      2,
      7,
      9,
      3,
      8,
      0,
      6,
      4,
      1,
      5
    },
    {
      7,
      0,
      4,
      6,
      9,
      1,
      3,
      2,
      5,
      8
    }
  };
            private static int[] inv = new int[10]
  {
    0,
    4,
    3,
    2,
    1,
    5,
    6,
    7,
    8,
    9
  };

            public static bool validateVerhoeff(string num)
            {
                int index1 = 0;
                int[] reversedIntArray = Verhoeff.StringToReversedIntArray(num);
                for (int index2 = 0; index2 < reversedIntArray.Length; ++index2)
                    index1 = Verhoeff.d[index1, Verhoeff.p[index2 % 8, reversedIntArray[index2]]];
                return index1 == 0;
            }

            public static string generateVerhoeff(string num)
            {
                int index1 = 0;
                int[] reversedIntArray = Verhoeff.StringToReversedIntArray(num);
                for (int index2 = 0; index2 < reversedIntArray.Length; ++index2)
                    index1 = Verhoeff.d[index1, Verhoeff.p[(index2 + 1) % 8, reversedIntArray[index2]]];
                return Verhoeff.inv[index1].ToString();
            }

            private static int[] StringToReversedIntArray(string num)
            {
                int[] numArray = new int[num.Length];
                for (int startIndex = 0; startIndex < num.Length; ++startIndex)
                    numArray[startIndex] = int.Parse(num.Substring(startIndex, 1));
                Array.Reverse((Array)numArray);
                return numArray;
            }
        }

        [HttpPost]

        public JsonResult Sanik_General_Registration(Generalform mydata)
        {
            string msg;
            bool check = false;
            string statecode = "06";
            DateTime datee = Convert.ToDateTime("01/01/2000");
            bool emailid, UID = false, mobile, landline, sanikname, father, spouse, mother, cid, amountrent, annualincm, anualbudget, dateofenrolment, disabilitypercent, pponumber, pancardnumber;
            if (mydata.emialid == "" || mydata.UID == "" || mydata.mobileno == "" || mydata.Sanik_Name_eng == "" || mydata.Father_Name_eng == "" || mydata.Mother_Name_eng == "" || mydata.Citizen_ID == "" || mydata.ESMIdentitycardnumber == "" || mydata.Gender_code == "")
            {
                string msgerror = "error";
                return Json(new { msgerror }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                emailid = IsValidEmail(mydata.emialid);
                if (mydata.UID == null)
                {
                    UID = true;
                }
                else
                {
                    var uid = mydata.UID;

                    UID = Verhoeff.validateVerhoeff(uid.ToString().Trim());

                }

                int mob = mydata.mobileno.Length;
                if (mob == 10)
                {
                    mobile = IsNumber(mydata.mobileno);
                }
                else
                {
                    string msgerror = "mob";
                    return Json(new { msgerror }, JsonRequestBehavior.AllowGet);
                }
                landline = IsNumber(mydata.landline);
                sanikname = IsName(mydata.Sanik_Name_eng);
                father = IsName(mydata.Father_Name_eng);
                spouse = IsName(mydata.spousename);
                mother = IsName(mydata.Mother_Name_eng);
                // dateofenrolment = checkdate(Convert.ToString(mydata.Date_of_Enrolment));

                disabilitypercent = Decimalchk(Convert.ToString(mydata.Disability_Percentage));
                pancardnumber = Pancard(mydata.Pancard_number);
                //if (mydata.Citizen_ID == null)
                //{
                //    cid = true;
                //}
                //else
                //{
                //    string cid1 = mydata.Citizen_ID.Substring(0, 2);
                //    if (cid1 == "HA")
                //    {
                //        cid = Citizen(mydata.Citizen_ID);
                //    }
                //    else
                //    {
                //        string msgerror = "CIDR not valid";
                //        return Json(new { msgerror }, JsonRequestBehavior.AllowGet);


                //    }

                //}

                amountrent = Decimalchk(Convert.ToString(mydata.Amount_OF_Rent));
                annualincm = Decimalchk(Convert.ToString(mydata.Annual_income));
                anualbudget = Decimalchk(Convert.ToString(mydata.Annual_Budget_for_Maintenance));
                if (emailid == false || UID == false || mobile == false || landline == false || sanikname == false || father == false || spouse == false || mother == false || amountrent == false || annualincm == false || anualbudget == false || disabilitypercent == false || pancardnumber == false)
                {

                    if (emailid == false)
                    {
                        msg = "e";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UID == false)
                    {
                        msg = "U";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (mobile == false)
                    {
                        msg = "M";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (landline == false)
                    {
                        msg = "L";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (sanikname == false)
                    {
                        msg = "Sname";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (father == false)
                    {
                        msg = "Fname";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (spouse == false)
                    {
                        msg = "Sname";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (mother == false)
                    {
                        msg = "Mname";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    //else if (cid == false)
                    //{
                    //    msg = "cid";
                    //    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    //}
                    else if (amountrent == false)
                    {
                        msg = "am";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (annualincm == false)
                    {
                        msg = "annual";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (anualbudget == false)
                    {
                        msg = "annualbdgt";

                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    //else if (dateofenrolment == false)
                    //{ 
                    //    msg = "dateofenrolment";

                    //    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    //}

                    else if (disabilitypercent == false)
                    {
                        msg = "disabilitypercent";

                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (pancardnumber == false)
                    {
                        msg = "pancardnumber";

                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(JsonRequestBehavior.AllowGet);
                }
                else
                {


                    try
                    {
                        //connection open///
                        if (dta.Connection.State == System.Data.ConnectionState.Closed)
                        {
                            dta.Connection.Open();
                        }
                        if (ed.Connection.State == System.Data.ConnectionState.Closed)
                        {
                            ed.Connection.Open();
                        }
                        ////////////
                        //transaction begin
                        dta.Transaction = dta.Connection.BeginTransaction();
                        ed.Transaction = ed.Connection.BeginTransaction();
                        /////////////////////////////

                        //bool check1 = dta.rsbgenerals.Any(tbll => tbll.Citizen_ID != null && tbll.Citizen_ID != "" || tbll.UID != null && tbll.UID != "");
                        //if (check1 == true)
                        //{


                        check = dta.rsbgenerals.Any(tbl => tbl.Army_No == mydata.Army_No || tbl.ESMIdentitycardnumber == mydata.ESMIdentitycardnumber);

                        if (check == true)
                        {
                            string output = "already exist";
                            return Json(new { output }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            rsbgeneral rsb = new rsbgeneral();

                            if (mydata.Photo != null)
                            {
                                rsb.photo = mydata.Photo;


                            }

                            else
                            {

                                if (Session["imagee"] != null)
                                {
                                    rsb.photo = (byte[])Session["imagee"];
                                }
                                else
                                {
                                    rsb.photo = null;
                                }


                            }


                            rsb.Army_No = mydata.Army_No;
                            rsb.ESMIdentitycardnumber = mydata.ESMIdentitycardnumber;
                            rsb.UID = mydata.UID;
                            //rsb.Citizen_ID = null;
                            //new changes 06-03-2018//
                            //***************************************************************//
                            if (rsb.UID != null)
                            {
                                var cid_uidcidrtbl = (from cdr in ed.tblCIDRs where cdr.UID == rsb.UID select cdr.Citizen_ID).FirstOrDefault();
                                if(cid_uidcidrtbl!=null)
                                {
                                    rsb.Citizen_ID = cid_uidcidrtbl.ToString();
                                }
                                else
                                {
                                    int increm = 0;

                                    int maxlen = 0;
                                    string LastSrno = string.Empty;

                                    string LastTwoDigi = string.Empty;
                                    tblCitizenSrno objSrno = new tblCitizenSrno();
                                    if (statecode == "06")
                                    {
                                        LastSrno = Convert.ToString((from srn in ed.tblCitizenSrnos where (srn.scode == "HA" && srn.dcode == mydata.dcode) select srn.lastsrno).Max());
                                    }

                                    int chk = Convert.ToInt32(LastSrno);
                                    if (chk == 0)
                                    {
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + "00000001";
                                        }

                                        objSrno.dcode = mydata.dcode;
                                        objSrno.scode = statecode;
                                        objSrno.lastsrno = "01";
                                        ed.tblCitizenSrnos.InsertOnSubmit(objSrno);
                                        ed.SubmitChanges();
                                    }
                                    else
                                    {
                                        if (chk < 10)
                                        {
                                            increm = chk + 1;
                                            if (statecode == "06")
                                            {
                                                rsb.Citizen_ID = "HA" + mydata.dcode + "0000000" + Convert.ToString(increm);
                                            }

                                        }
                                        if (chk >= 10 && chk <= 98)
                                        {
                                            increm = chk + 1;
                                            if (statecode == "06")
                                            {
                                                rsb.Citizen_ID = "HA" + mydata.dcode + "000000" + Convert.ToString(increm);
                                            }

                                        }
                                        if (chk >= 99 && chk <= 998)
                                        {
                                            increm = chk + 1;
                                            if (statecode == "06")
                                            {
                                                rsb.Citizen_ID = "HA" + mydata.dcode + "00000" + Convert.ToString(increm);
                                            }

                                        }
                                        if (chk >= 999 && chk <= 9998)
                                        {
                                            increm = chk + 1;
                                            if (statecode == "06")
                                            {
                                                rsb.Citizen_ID = "HA" + mydata.dcode + "0000" + Convert.ToString(increm);
                                            }

                                        }

                                        if (chk >= 9999 && chk <= 99998)
                                        {
                                            increm = chk + 1;
                                            if (statecode == "06")
                                            {
                                                rsb.Citizen_ID = "HA" + mydata.dcode + "000" + Convert.ToString(increm);
                                            }

                                        }
                                        if (chk >= 99999 && chk <= 999998)
                                        {
                                            increm = chk + 1;
                                            if (statecode == "06")
                                            {
                                                rsb.Citizen_ID = "HA" + mydata.dcode + "00" + Convert.ToString(increm);
                                            }

                                        }
                                        if (chk >= 999999 && chk <= 9999998)
                                        {
                                            increm = chk + 1;
                                            if (statecode == "06")
                                            {
                                                rsb.Citizen_ID = "HA" + mydata.dcode + "0" + Convert.ToString(increm);
                                            }

                                        }
                                        if (chk >= 9999999 && chk <= 99999998)
                                        {
                                            increm = chk + 1;
                                            if (statecode == "06")
                                            {
                                                rsb.Citizen_ID = "HA" + mydata.dcode + Convert.ToString(increm);
                                            }

                                        }
                                        if (LastSrno.Trim().Length > 0)
                                        {
                                            maxlen = LastSrno.Trim().Length;
                                        }
                                        LastTwoDigi = Convert.ToString(rsb.Citizen_ID.Substring(rsb.Citizen_ID.Length - maxlen));  // can be more than 2

                                        var query1 = from rec in ed.tblCitizenSrnos
                                                     where (rec.scode == "HA") && (rec.dcode == mydata.dcode)
                                                     select rec;

                                        foreach (tblCitizenSrno rec in query1)
                                        {
                                            rec.lastsrno = LastTwoDigi;
                                        }
                                        mydata.Citizen_ID = rsb.Citizen_ID;
                                        ed.SubmitChanges();
                                        Session["cidr1"]=rsb.Citizen_ID;
                                    }
                                }
                            }
                            else
                            {
                                int increm = 0;

                                int maxlen = 0;
                                string LastSrno = string.Empty;

                                string LastTwoDigi = string.Empty;
                                tblCitizenSrno objSrno = new tblCitizenSrno();
                                if (statecode == "06")
                                {
                                    LastSrno = Convert.ToString((from srn in ed.tblCitizenSrnos where (srn.scode == "HA" && srn.dcode == mydata.dcode) select srn.lastsrno).Max());
                                }

                                int chk = Convert.ToInt32(LastSrno);
                                if (chk == 0)
                                {
                                    if (statecode == "06")
                                    {
                                        rsb.Citizen_ID = "HA" + mydata.dcode + "00000001";
                                    }

                                    objSrno.dcode = mydata.dcode;
                                    objSrno.scode = statecode;
                                    objSrno.lastsrno = "01";
                                    ed.tblCitizenSrnos.InsertOnSubmit(objSrno);
                                    ed.SubmitChanges();
                                }
                                else
                                {
                                    if (chk < 10)
                                    {
                                        increm = chk + 1;
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + "0000000" + Convert.ToString(increm);
                                        }

                                    }
                                    if (chk >= 10 && chk <= 98)
                                    {
                                        increm = chk + 1;
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + "000000" + Convert.ToString(increm);
                                        }

                                    }
                                    if (chk >= 99 && chk <= 998)
                                    {
                                        increm = chk + 1;
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + "00000" + Convert.ToString(increm);
                                        }

                                    }
                                    if (chk >= 999 && chk <= 9998)
                                    {
                                        increm = chk + 1;
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + "0000" + Convert.ToString(increm);
                                        }

                                    }

                                    if (chk >= 9999 && chk <= 99998)
                                    {
                                        increm = chk + 1;
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + "000" + Convert.ToString(increm);
                                        }

                                    }
                                    if (chk >= 99999 && chk <= 999998)
                                    {
                                        increm = chk + 1;
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + "00" + Convert.ToString(increm);
                                        }

                                    }
                                    if (chk >= 999999 && chk <= 9999998)
                                    {
                                        increm = chk + 1;
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + "0" + Convert.ToString(increm);
                                        }

                                    }
                                    if (chk >= 9999999 && chk <= 99999998)
                                    {
                                        increm = chk + 1;
                                        if (statecode == "06")
                                        {
                                            rsb.Citizen_ID = "HA" + mydata.dcode + Convert.ToString(increm);
                                        }

                                    }
                                    if (LastSrno.Trim().Length > 0)
                                    {
                                        maxlen = LastSrno.Trim().Length;
                                    }
                                    LastTwoDigi = Convert.ToString(rsb.Citizen_ID.Substring(rsb.Citizen_ID.Length - maxlen));  // can be more than 2

                                    var query1 = from rec in ed.tblCitizenSrnos
                                                 where (rec.scode == "HA") && (rec.dcode == mydata.dcode)
                                                 select rec;

                                    foreach (tblCitizenSrno rec in query1)
                                    {
                                        rec.lastsrno = LastTwoDigi;
                                    }
                                    mydata.Citizen_ID = rsb.Citizen_ID;
                                    ed.SubmitChanges();
                                    Session["cidr2"] = rsb.Citizen_ID;
                                }
                            }
                           
                            //****************************************************************//

                            if (mydata.status == null)
                            {
                                rsb.Status = Convert.ToChar("E");
                            }
                            else
                            {
                                rsb.Status = Convert.ToChar(mydata.status);
                            }

                            rsb.Sanik_Name_eng = mydata.Sanik_Name_eng;
                            rsb.Sanik_Name_hindi = mydata.Sanik_Name_hindi;
                            rsb.Father_Name_eng = mydata.Father_Name_eng;
                            rsb.Father_Name_hindi = mydata.Father_Name_hindi;
                            rsb.Mother_Name_eng = mydata.Mother_Name_eng;
                            rsb.Mother_Name_hindi = mydata.Mother_Name_hindi;
                           
                            //new 03-03-2018//
                            var nn = mydata.Date_of_Enrolment;

                            //new 03-03-2018//
                            if (nn != null)
                            {
                                DateTime ValidDate;
                                //DateTime? ValidDate2;

                                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                {
                                    rsb.Date_of_Enrolment = ValidDate;



                                    mydata.Error = "date comes after parsing";
                                    mydata.PageError = "/View/RSBGeneralInfrm";
                                    mydata.DateError = DateTime.Now;
                                    mydata.DetailError = rsb.Date_of_Enrolment.ToString();
                                    tblTraceError error21 = new tblTraceError();
                                    error21.Error = mydata.Error;
                                    error21.PageError = mydata.PageError;
                                    error21.DateError = mydata.DateError;
                                    error21.DetailError = mydata.DetailError;

                                    ed.tblTraceErrors.InsertOnSubmit(error21);
                                    ed.SubmitChanges();
                                }
                                else
                                {

                                }
                                ///////////////////////////////error table///////////////////////////
                                mydata.Error = "date comes";
                                mydata.PageError = "/View/RSBGeneralInfrm";
                                mydata.DateError = DateTime.Now;
                                mydata.DetailError = nn.ToString();
                                tblTraceError error1 = new tblTraceError();
                                error1.Error = mydata.Error;
                                error1.PageError = mydata.PageError;
                                error1.DateError = mydata.DateError;
                                error1.DetailError = mydata.DetailError;

                                ed.tblTraceErrors.InsertOnSubmit(error1);
                                ed.SubmitChanges();
                                //////////////////////////////////////////////////
                                //string[] formats = { "MM-dd-yyyy" };
                                //mydata.Date_of_Enrolment = DateTime.ParseExact(mydata.Date_of_Enrolment, formats, new CultureInfo("en-US"), DateTimeStyles.None);
                                //rsb.Date_of_Enrolment = Convert.ToDateTime(nn);
                                // rsb.Date_of_Enrolment = Convert.ToDateTime(mydata.Date_of_Enrolment);
                            }


                            else
                            {
                                string doe = "Enter Date of Enrolment !!";
                                return Json(new { doe }, JsonRequestBehavior.AllowGet);
                                ////////////////////////////error table
                                mydata.Error = "error in date";
                                mydata.PageError = "/View/RSBGeneralInfrm";
                                mydata.DateError = DateTime.Now;
                                mydata.DetailError = rsb.Date_of_Enrolment.ToString();
                                tblTraceError error2 = new tblTraceError();
                                error2.Error = mydata.Error;
                                error2.PageError = mydata.PageError;
                                error2.DateError = mydata.DateError;
                                error2.DetailError = mydata.DetailError;

                                ed.tblTraceErrors.InsertOnSubmit(error2);
                                ed.SubmitChanges();
                                /////////////////////////////////

                            }
                            /////////////////////////////////////

                            rsb.Disable = mydata.Disable1;
                            rsb.Disability_Percentage = mydata.Disability_Percentage;
                            rsb.PPO_number = mydata.PPO_number;
                            rsb.Pancard_number = mydata.Pancard_number;
                            //end//
                            //if (mydata.DOB == null || mydata.DOB == "01-01-0001")
                            //{
                            //    string dob = "Enter DOB !!";
                            //    return Json(new { dob }, JsonRequestBehavior.AllowGet);
                            //}
                            if (DateTime.ParseExact(mydata.DOB, "dd/MM/yyyy", null) != DateTime.Now.Date)
                            {
                                if (DateTime.ParseExact(mydata.DOB, "dd/MM/yyyy", null) < datee)
                                {
                                    var dob1 = mydata.DOB;

                                    //new 19-03-2018//
                                    if (dob1 != null)
                                    {
                                        DateTime ValidDate1;
                                        //DateTime? ValidDate2;

                                        if (DateTime.TryParseExact(dob1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
                                        {
                                            rsb.DOB = ValidDate1;
                                            ////////////////////////////error table
                                            mydata.Error = "error in dateof birth";
                                            mydata.PageError = "/View/RSBGeneralInfrm";
                                            mydata.DateError = DateTime.Now;
                                            mydata.DetailError = Convert.ToString(rsb.DOB);
                                            tblTraceError error2 = new tblTraceError();
                                            error2.Error = mydata.Error;
                                            error2.PageError = mydata.PageError;
                                            error2.DateError = mydata.DateError;
                                            error2.DetailError = mydata.DetailError;

                                            ed.tblTraceErrors.InsertOnSubmit(error2);
                                            ed.SubmitChanges();
                                            /////////////////////////////////

                                        }
                                        else
                                        {
                                            mydata.Error = "error in dateof else part birth";
                                            mydata.PageError = "/View/RSBGeneralInfrm";
                                            mydata.DateError = DateTime.Now;
                                            mydata.DetailError = dob1;
                                            tblTraceError error2 = new tblTraceError();
                                            error2.Error = mydata.Error;
                                            error2.PageError = mydata.PageError;
                                            error2.DateError = mydata.DateError;
                                            error2.DetailError = mydata.DetailError;

                                            ed.tblTraceErrors.InsertOnSubmit(error2);
                                            ed.SubmitChanges();
                                        }

                                    }
                                    else
                                    {
                                        string dob = "DOB is enter valid !!";
                                        return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    string dob = "DOB is not valid !!";
                                    return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                string dob = "DOB is not valid !!";
                                return Json(new { dob }, JsonRequestBehavior.AllowGet);
                            }

                            rsb.Gender_code = mydata.Gender;
                            rsb.CategoryCode = mydata.CategoryDesc;
                            rsb.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusDesc);
                            if (Convert.ToString(rsb.MaritalStatusCode) == "2")
                            {
                                rsb.spousename = null;
                                rsb.spousenamehindi = null;
                            }
                            else
                            {
                                rsb.spousename = mydata.spousename;
                                rsb.spousenamehindi = mydata.spousenamehindi;
                            }
                            rsb.Regement_Corps_id = mydata.Regement_Corps_id;
                            rsb.landline = mydata.landline;
                            rsb.mobileno = mydata.mobileno;
                            rsb.emialid = mydata.emialid;
                            rsb.Per_address_eng = mydata.Per_address_eng;
                            rsb.Per_address_hindi = mydata.Per_address_hindi;
                            rsb.Per_Landmark_english = mydata.Per_Landmark_english;
                            rsb.dcode = mydata.dcode;
                            rsb.tcode = mydata.tcode;

                            rsb.Urban_rural = Convert.ToChar(mydata.Urban_rural);
                            if (rsb.Urban_rural == Convert.ToChar("U"))
                            {
                                if (mydata.towncode == null)
                                {
                                    rsb.towncode = null;
                                }
                                else
                                {
                                    rsb.towncode = mydata.towncode;
                                }

                            }
                            else
                            {
                                if (mydata.VCODE == null)
                                {
                                    rsb.VCODE = null;
                                }
                                else
                                {
                                    rsb.VCODE = mydata.VCODE;
                                }
                            }
                            rsb.perchk = mydata.perchk;
                            if (mydata.Per_cors == null)
                            {
                                rsb.Per_cors = null;
                            }
                            else
                            {
                                rsb.Per_cors = Convert.ToChar(mydata.Per_cors);
                            }

                            rsb.statecode = mydata.statecode;
                            rsb.Per_Landmark_Hindi = mydata.Per_Landmark_Hindi;
                            rsb.Pin_code = mydata.Pin_code;
                            rsb.Cors_address = mydata.Cors_address;
                            if (mydata.Amount_OF_Rent == null)
                            {
                                rsb.Amount_OF_Rent = 0;
                            }
                            else
                            {
                                rsb.Amount_OF_Rent = mydata.Amount_OF_Rent;
                            }
                            if (mydata.Annual_Budget_for_Maintenance == null)
                            {
                                rsb.Annual_Budget_for_Maintenance = 0;
                            }
                            else
                            {
                                rsb.Annual_Budget_for_Maintenance = mydata.Annual_Budget_for_Maintenance;
                            }
                            if (mydata.Annual_income == null)
                            {
                                rsb.Annual_income = 0;
                            }
                            else
                            {
                                rsb.Annual_income = mydata.Annual_income;
                            }

                            rsb.RegistrationDate = DateTime.Now;
                            rsb.CreateDate = DateTime.Now;
                            rsb.CreateUser = (String)Session["userid"];

                            dta.rsbgenerals.InsertOnSubmit(rsb);

                            dta.SubmitChanges();

                            //insert into cidr table
                            var cidr1=(string)Session["cidr1"];
                            var cidr2=(string)Session["cidr2"];
                            if(cidr1!=null||cidr2!=null)
                            {
                                try
                                {
                                    tblCIDR cdr = new tblCIDR();
                                    cdr.Citizen_ID = mydata.Citizen_ID;
                                    cdr.Citizen_Name_EN = mydata.Sanik_Name_eng;
                                    cdr.Citizen_Name_LL = mydata.Sanik_Name_hindi;
                                    cdr.Gender = Convert.ToChar(mydata.Gender);
                                    cdr.Marital_Status = mydata.MaritalStatusCode;
                                    cdr.Caste_Category = mydata.CategoryCode;
                                    cdr.Father_Name_EN = mydata.Father_Name_eng;
                                    cdr.Father_Name_LL = mydata.Father_Name_hindi;
                                    cdr.Mother_Name_EN = mydata.Mother_Name_eng;
                                    cdr.Father_Name_LL = mydata.Mother_Name_hindi;
                                    cdr.DOB = rsb.DOB;
                                    cdr.House_Name_No = mydata.Per_address_eng;
                                    cdr.Landmark_Locality_Colony = mydata.Per_Landmark_english;
                                    cdr.Correspondence_Address_EN = mydata.Cors_address;
                                    cdr.RuralUrban = Convert.ToChar(mydata.Urban_rural);
                                    cdr.Village_Town_Code = mydata.VCODE;
                                    cdr.Block_Tehsil_Code = mydata.tcode;
                                    cdr.District_Code = mydata.dcode;
                                    cdr.PIN = mydata.Pin_code;
                                    cdr.Email_id = mydata.emialid;
                                    cdr.Mobile = mydata.mobileno;
                                    cdr.DOM = null;
                                    ed.tblCIDRs.InsertOnSubmit(cdr);
                                    ed.SubmitChanges();
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                    //changes on 13-03-2018
                                    mydata.Error = ex.Message;
                                    mydata.PageError = "/View/RSBGeneralInfrm";
                                    mydata.DateError = DateTime.Now;
                                    mydata.DetailError = ex.ToString();
                                    tblTraceError error = new tblTraceError();
                                    error.Error = mydata.Error;
                                    error.PageError = mydata.PageError;
                                    error.DateError = mydata.DateError;
                                    error.DetailError = mydata.DetailError;

                                    ed.tblTraceErrors.InsertOnSubmit(error);
                                    ed.SubmitChanges();
                                    //end
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                            
                            }
               
                            //transation complete or not
                            ed.Transaction.Commit();
                            dta.Transaction.Commit();
                            //end transation
                            //end
                            Session["imagee"] = null;
                            var result = new { armyno = mydata.Army_No, sanikname = mydata.Sanik_Name_eng };

                            Session["id"] = rsb.id;
                            Session["Armyno"] = rsb.Army_No;
                            Session["aarmyno"] = rsb.Army_No;
                            Session["armynoalrd"] = rsb.Army_No;
                            //connection close///////////////
                            if (dta.Connection.State == System.Data.ConnectionState.Open)
                            {
                                dta.Connection.Close();
                            }
                            if (ed.Connection.State == System.Data.ConnectionState.Open)
                            {
                                ed.Connection.Close();
                            }
                            ////////////////////////////
                            return Json(result, JsonRequestBehavior.AllowGet);


                        }



                    }
                    catch (Exception ex)
                    {

                        // throw ex;
                        //changes on 13-03-2018
                        mydata.Error = ex.Message;
                        mydata.PageError = "/View/RSBGeneralInfrm";
                        mydata.DateError = DateTime.Now;
                        mydata.DetailError = ex.ToString();
                        tblTraceError error = new tblTraceError();
                        error.Error = mydata.Error;
                        error.PageError = mydata.PageError;
                        error.DateError = mydata.DateError;
                        error.DetailError = mydata.DetailError;

                        ed.tblTraceErrors.InsertOnSubmit(error);
                        ed.SubmitChanges();
                        ed.Transaction.Rollback();
                        dta.Transaction.Rollback();
                        //end
                        return Json(JsonRequestBehavior.AllowGet);


                    }



                }
            }

        }
        //new//
        //public DataTable ExecuteDataTable(string cmdText, SqlParameter[] prms, CommandType type)
        //{

        //DataTable dt = default(DataTable);
        //SqlCommand cmd = default(SqlCommand);
        //SqlDataAdapter adpt = default(SqlDataAdapter);
        //SqlConnection conn = default(SqlConnection);
        //    using (conn = new SqlConnection(Constrg))
        //    {

        //        dt = new DataTable();
        //        using (cmd = new SqlCommand(cmdText, conn))
        //        {
        //            cmd.CommandType = type;
        //            if (prms != null)
        //            {
        //                foreach (SqlParameter p in prms)
        //                {
        //                    cmd.Parameters.Add(p);
        //                }
        //            }
        //            adpt = new SqlDataAdapter(cmd);
        //            adpt.Fill(dt);
        //            return dt;
        //        }
        //    }
        //}

        //end//

        public JsonResult update(Generalform mydata)
        {
            DateTime datee = Convert.ToDateTime("01/01/2000");

            try
            {

                //connection open///
                if (dta.Connection.State == System.Data.ConnectionState.Closed)
                {
                    dta.Connection.Open();
                }
                if (ed.Connection.State == System.Data.ConnectionState.Closed)
                {
                    ed.Connection.Open();
                }
                ////////////
                //transaction begin
                dta.Transaction = dta.Connection.BeginTransaction();
                ed.Transaction = ed.Connection.BeginTransaction();
                /////////////////////////////
                rsbgeneral rsb = new rsbgeneral();

                rsb = dta.rsbgenerals.Where(x => x.UID == mydata.UID || x.Citizen_ID == mydata.Citizen_ID || x.Army_No == mydata.Army_No || x.Army_No == ((string)Session["armynoalrd"]) || x.Army_No == ((string)Session["Armyno"])).FirstOrDefault();
                if (rsb != null)
                {
                    if (mydata.Photo != null)
                    {
                        rsb.photo = mydata.Photo;


                    }

                    else
                    {

                        if (Session["imagee"] != null)
                        {
                            rsb.photo = (byte[])Session["imagee"];
                            Session["imagee"] = null;
                        }
                        else
                        {
                            rsb.photo = null;
                        }


                    }

                    rsb.Army_No = mydata.Army_No;
                    rsb.ESMIdentitycardnumber = mydata.ESMIdentitycardnumber;
                    if (mydata.UID == null)
                    {
                        rsb.UID = null;
                    }
                    else
                    {
                        var uid = mydata.UID;

                        bool UID = Verhoeff.validateVerhoeff(uid.ToString().Trim());
                        if (UID == true)
                        {
                            rsb.UID = uid;
                        }
                        else
                        {
                            string msg = "U";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);

                        }

                    }


                    if (rsb.Citizen_ID == null)
                    {
                        rsb.Citizen_ID = null;
                    }
                    else
                    {
                        //string cid1 = mydata.Citizen_ID.Substring(0, 2);
                        //if (cid1 == "HA")
                        //{
                        //    bool cid = Citizen(mydata.Citizen_ID);
                        //    if (cid == false)
                        //    {
                        //        string msgerror = "CIDR nt valid";
                        //        return Json(new { msgerror }, JsonRequestBehavior.AllowGet);

                        //    }
                        //    else
                        //    {
                           rsb.Citizen_ID = rsb.Citizen_ID;
                        //    }
                        //}
                        //else
                        //{
                        //    string msgerror = "CIDR not valid";
                        //    return Json(new { msgerror }, JsonRequestBehavior.AllowGet);

                        //}
                    }


                    rsb.Sanik_Name_eng = mydata.Sanik_Name_eng;
                    rsb.Sanik_Name_hindi = mydata.Sanik_Name_hindi;
                    rsb.Father_Name_eng = mydata.Father_Name_eng;
                    rsb.Father_Name_hindi = mydata.Father_Name_hindi;
                    rsb.Mother_Name_eng = mydata.Mother_Name_eng;
                    rsb.Mother_Name_hindi = mydata.Mother_Name_hindi;
                    rsb.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusDesc);
                    if (Convert.ToString(rsb.MaritalStatusCode) == "2")
                    {
                        rsb.spousename = null;
                        rsb.spousenamehindi = null;
                    }
                    else
                    {
                        rsb.spousename = mydata.spousename;
                        rsb.spousenamehindi = mydata.spousenamehindi;
                    }
                    try
                    {
                        if (DateTime.ParseExact(mydata.DOB, "dd/MM/yyyy", null) != DateTime.Now.Date)
                        {
                            if (DateTime.ParseExact(mydata.DOB, "dd/MM/yyyy", null) < datee)
                            {
                                var dob1 = mydata.DOB;

                                //new 19-03-2018//
                                if (dob1 != null)
                                {
                                    DateTime ValidDate1;
                                    //DateTime? ValidDate2;

                                    if (DateTime.TryParseExact(dob1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
                                    {
                                        rsb.DOB = ValidDate1;
                                        ////////////////////////////error table
                                        mydata.Error = "error in dateof birth";
                                        mydata.PageError = "/View/RSBGeneralInfrm";
                                        mydata.DateError = DateTime.Now;
                                        mydata.DetailError = dob1;
                                        tblTraceError error2 = new tblTraceError();
                                        error2.Error = mydata.Error;
                                        error2.PageError = mydata.PageError;
                                        error2.DateError = mydata.DateError;
                                        error2.DetailError = mydata.DetailError;

                                        ed.tblTraceErrors.InsertOnSubmit(error2);
                                        ed.SubmitChanges();
                                        /////////////////////////////////

                                    }
                                    else
                                    {
                                        mydata.Error = "error in dateof else part birth";
                                        mydata.PageError = "/View/RSBGeneralInfrm";
                                        mydata.DateError = DateTime.Now;
                                        mydata.DetailError = dob1;
                                        tblTraceError error2 = new tblTraceError();
                                        error2.Error = mydata.Error;
                                        error2.PageError = mydata.PageError;
                                        error2.DateError = mydata.DateError;
                                        error2.DetailError = mydata.DetailError;

                                        ed.tblTraceErrors.InsertOnSubmit(error2);
                                        ed.SubmitChanges();
                                    }

                                }
                                else
                                {
                                    string dob = "DOB is enter valid !!";
                                    return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                string dob = "DOB is not valid !!";
                                return Json(new { dob }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            string dob = "DOB is not valid !!";
                            return Json(new { dob }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //changes on 13-03-2018
                        mydata.Error = ex.Message;
                        mydata.PageError = "/View/RSBGeneralInfrm";
                        mydata.DateError = DateTime.Now;
                        mydata.DetailError = ex.ToString();
                        tblTraceError error = new tblTraceError();
                        error.Error = mydata.Error;
                        error.PageError = mydata.PageError;
                        error.DateError = mydata.DateError;
                        error.DetailError = mydata.DetailError;

                        ed.tblTraceErrors.InsertOnSubmit(error);
                        ed.SubmitChanges();
                        //end
                        return Json(JsonRequestBehavior.AllowGet); throw ex;
                    }
                    if (mydata.status == null)
                    {
                        rsb.Status = Convert.ToChar("E");
                    }
                    else if (mydata.status == "F")
                    {
                        rsb.Status = Convert.ToChar("F");
                    }
                    else
                    {
                        rsb.Status = Convert.ToChar(mydata.status);
                    }

                    rsb.Gender_code = mydata.Gender;
                    rsb.CategoryCode = mydata.CategoryDesc;
                    
                    rsb.Regement_Corps_id = mydata.Regement_Corps_id;
                    rsb.landline = mydata.landline;
                    rsb.mobileno = mydata.mobileno;
                    rsb.emialid = mydata.emialid;
                    rsb.Per_address_eng = mydata.Per_address_eng;
                    rsb.Per_address_hindi = mydata.Per_address_hindi;
                    rsb.Per_Landmark_english = mydata.Per_Landmark_english;
                    rsb.Per_Landmark_Hindi = mydata.Per_Landmark_Hindi;
                    rsb.statecode = mydata.statecode;
                    rsb.Urban_rural = Convert.ToChar(mydata.Urban_rural);
                    if (rsb.Urban_rural == Convert.ToChar("U"))
                    {
                        if (mydata.towncode == null)
                        {
                            rsb.towncode = null;
                        }
                        else
                        {
                            rsb.towncode = mydata.towncode;
                        }

                    }
                    else
                    {
                        if (mydata.VCODE == null)
                        {
                            rsb.VCODE = null;
                        }
                        else
                        {
                            rsb.VCODE = mydata.VCODE;
                        }
                    }
                    rsb.Pin_code = mydata.Pin_code;
                    rsb.Cors_address = mydata.Cors_address;
                    if (mydata.perchk == null)
                    {
                        rsb.perchk = null;
                    }
                    else
                    {
                        rsb.perchk = mydata.perchk;
                    }

                    rsb.Per_cors = Convert.ToChar(mydata.Per_cors);
                    if (mydata.Amount_OF_Rent == null)
                    {
                        rsb.Amount_OF_Rent = 0;
                    }
                    else
                    {
                        rsb.Amount_OF_Rent = mydata.Amount_OF_Rent;
                    }
                    if (mydata.Annual_Budget_for_Maintenance == null)
                    {
                        rsb.Annual_Budget_for_Maintenance = 0;
                    }
                    else
                    {
                        rsb.Annual_Budget_for_Maintenance = mydata.Annual_Budget_for_Maintenance;
                    }
                    if (mydata.Annual_income == null)
                    {
                        rsb.Annual_income = 0;
                    }
                    else
                    {
                        rsb.Annual_income = mydata.Annual_income;
                    }
                    var nn = mydata.Date_of_Enrolment;

                    //new 03-03-2018//
                    if (nn != null)
                    {
                        DateTime ValidDate;

                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                        {
                            rsb.Date_of_Enrolment = ValidDate;
                        }
                        ///////////////////////////////error table///////////////////////////
                        mydata.Error = rsb.Date_of_Enrolment.ToString();
                        mydata.PageError = "/View/RSBGeneralInfrm";
                        mydata.DateError = DateTime.Now;
                        mydata.DetailError = rsb.Date_of_Enrolment.ToString();
                        tblTraceError error1 = new tblTraceError();
                        error1.Error = mydata.Error;
                        error1.PageError = mydata.PageError;
                        error1.DateError = mydata.DateError;
                        error1.DetailError = mydata.DetailError;

                        ed.tblTraceErrors.InsertOnSubmit(error1);
                        ed.SubmitChanges();
                        //////////////////////////////////////////////////
                        //string[] formats = { "MM-dd-yyyy" };
                        //mydata.Date_of_Enrolment = DateTime.ParseExact(mydata.Date_of_Enrolment, formats, new CultureInfo("en-US"), DateTimeStyles.None);
                        //rsb.Date_of_Enrolment = Convert.ToDateTime(nn);
                        // rsb.Date_of_Enrolment = Convert.ToDateTime(mydata.Date_of_Enrolment);
                    }
                    else
                    {
                        string doe = "Enter Date of Enrolment !!";
                        return Json(new { doe }, JsonRequestBehavior.AllowGet);
                        ////////////////////////////error table
                        mydata.Error = "error in date";
                        mydata.PageError = "/View/RSBGeneralInfrm";
                        mydata.DateError = DateTime.Now;
                        mydata.DetailError = rsb.Date_of_Enrolment.ToString();
                        tblTraceError error2 = new tblTraceError();
                        error2.Error = mydata.Error;
                        error2.PageError = mydata.PageError;
                        error2.DateError = mydata.DateError;
                        error2.DetailError = mydata.DetailError;

                        ed.tblTraceErrors.InsertOnSubmit(error2);
                        ed.SubmitChanges();
                        /////////////////////////////////

                    }

                    rsb.Disable = mydata.Disable1;

                    if (rsb.Disable == true)
                    {
                        rsb.Disability_Percentage = mydata.Disability_Percentage;
                    }
                    else
                    {
                        rsb.Disability_Percentage = null;
                    }
                    rsb.PPO_number = mydata.PPO_number;
                    rsb.Pancard_number = mydata.Pancard_number;
                    //end//
                    rsb.ChangeUser = (String)Session["userid"];
                    rsb.ChangeDate = DateTime.Now;
                    dta.SubmitChanges();
                    //changes in cidr table
                    if (!string.IsNullOrEmpty(mydata.Citizen_ID) &&  mydata.Citizen_ID!="null")
                    {
                        try
                        {

                            tblCIDR cdr = new tblCIDR();
                            cdr = ed.tblCIDRs.Where(x => x.Citizen_ID == mydata.Citizen_ID).FirstOrDefault();
                            if (cdr != null)
                            {
                                //dr.Citizen_ID = mydata.Citizen_ID;
                                cdr.Citizen_Name_EN = mydata.Sanik_Name_eng;
                                cdr.Citizen_Name_LL = mydata.Sanik_Name_hindi;
                                cdr.Gender = Convert.ToChar(mydata.Gender);
                                cdr.Marital_Status = mydata.MaritalStatusCode;
                                cdr.Caste_Category = mydata.CategoryCode;
                                cdr.Father_Name_EN = mydata.Father_Name_eng;
                                cdr.Father_Name_LL = mydata.Father_Name_hindi;
                                cdr.Mother_Name_EN = mydata.Mother_Name_eng;
                                cdr.Father_Name_LL = mydata.Mother_Name_hindi;
                                cdr.DOB = rsb.DOB;
                                cdr.House_Name_No = mydata.Per_address_eng;
                                cdr.Landmark_Locality_Colony = mydata.Per_Landmark_english;
                                cdr.Correspondence_Address_EN = mydata.Cors_address;
                                cdr.RuralUrban = Convert.ToChar(mydata.Urban_rural);
                                cdr.Village_Town_Code = mydata.VCODE;
                                cdr.Block_Tehsil_Code = mydata.tcode;
                                cdr.District_Code = mydata.dcode;
                                cdr.PIN = mydata.Pin_code;
                                cdr.Email_id = mydata.emialid;
                                cdr.Mobile = mydata.mobileno;
                                cdr.DOM = null;

                                ed.SubmitChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            // throw ex;
                            //changes on 13-03-2018
                            mydata.Error = ex.Message;
                            mydata.PageError = "/View/RSBGeneralInfrm";
                            mydata.DateError = DateTime.Now;
                            mydata.DetailError = ex.ToString();
                            tblTraceError error = new tblTraceError();
                            error.Error = mydata.Error;
                            error.PageError = mydata.PageError;
                            error.DateError = mydata.DateError;
                            error.DetailError = mydata.DetailError;

                            ed.tblTraceErrors.InsertOnSubmit(error);
                            ed.SubmitChanges();
                            //end
                            return Json(JsonRequestBehavior.AllowGet);
                        }
                    }
                    ////end
                    //transation complete or not
                    ed.Transaction.Commit();
                    dta.Transaction.Commit();
                    //end transation
                    Session["Armyno"] = rsb.Army_No;
                    //connection close///////////////
                    if (dta.Connection.State == System.Data.ConnectionState.Open)
                    {
                        dta.Connection.Close();
                    }
                    if (ed.Connection.State == System.Data.ConnectionState.Open)
                    {
                        ed.Connection.Close();
                    }
                    ////////////////////////////
                    return Json(rsb, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string msg1 = "not";
                    return Json(msg1, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //
                //changes on 13-03-2018
                mydata.Error = ex.Message;
                mydata.PageError = "/View/RSBGeneralInfrm";
                mydata.DateError = DateTime.Now;
                mydata.DetailError = ex.ToString();
                tblTraceError error = new tblTraceError();
                error.Error = mydata.Error;
                error.PageError = mydata.PageError;
                error.DateError = mydata.DateError;
                error.DetailError = mydata.DetailError;

                ed.tblTraceErrors.InsertOnSubmit(error);
                ed.SubmitChanges();
                ed.Transaction.Rollback();
                dta.Transaction.Rollback();
                //end
                return Json(JsonRequestBehavior.AllowGet); throw ex;


            }

        }

        [HttpPost]
        public JsonResult Sanik_General_Registrationstep2(Generalform mydata)
        {
            //int datee = Convert.ToDateTime("01/01/2000").Year;
            string msg;
            bool dependent, cid = false, noofchildren;


            dependent = IsName(mydata.Dependent_Name);
            noofchildren = IsNumber(mydata.Number_Of_children);
            if (mydata.UID == null)
            { cid = true; }
            else
            {
                var cid1 = mydata.UID;

                cid = Verhoeff.validateVerhoeff(cid1.ToString().Trim());

            }
            if (cid == false || dependent == false || noofchildren == false)
            {

                if (cid == false)
                {
                    msg = "c";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (dependent == false)
                {
                    msg = "d";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (noofchildren == false)
                {
                    msg = "child";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    DateTime Dependent_DOB, DOM;
                    var nn1 = mydata.Dependent_DOB;
                    DateTime ValidDate1;
                    DateTime ValidDate2;
                    //DateTime? ValidDate2;
                    var nn3 = mydata.DOM;
                    bool check = dta.tblfamilydetails.Any(tbl => tbl.Army_No == mydata.Army_No && tbl.UID == mydata.UID);

                    if (check == true)
                    {
                        string output = "already exist";
                        return Json(new { output, mydata.UID }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        tblfamilydetail fm = new tblfamilydetail();


                        if (Session["imagee"] != null)
                        {
                            fm.imagedept = (byte[])Session["imagee"];
                            Session["imagee"] = null;
                        }
                        else if (mydata.imagedept != null)
                        {

                            var img = Session["imagee"];
                            fm.imagedept = mydata.imagedept;


                        }
                        else
                        {
                            fm.imagedept = null;
                        }

                        if (Session["Armyno"] != null)
                        {
                            fm.Army_No = Session["Armyno"].ToString();
                        }
                        else
                        {
                            fm.Army_No = mydata.Army_No;
                        }


                        fm.Dependent_Name = mydata.Dependent_Name;
                        if (mydata.RelationCode == "0")
                        {
                            string rel = "please select relation";
                            return Json(new { rel }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            fm.RelationCode = mydata.RelationCode;
                        }

                        fm.UID = mydata.UID;
                        if (mydata.Dependent_DOB != null)
                        {

                            if (DateTime.TryParseExact(nn1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
                            {

                                Dependent_DOB = ValidDate1;
                                if (Dependent_DOB != DateTime.Now.Date)
                                {
                                    var nn = mydata.Dependent_DOB;

                                    //new 03-03-2018//
                                    if (nn != null)
                                    {
                                        DateTime ValidDate;
                                        //DateTime? ValidDate2;

                                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                        {
                                            fm.DOB = ValidDate;
                                        }
                                    }
                                    else
                                    {
                                        mydata.Error = "date of dependent error";
                                        mydata.PageError = "/View/RSBGeneralInfrm";
                                        mydata.DateError = DateTime.Now;
                                        mydata.DetailError = nn.ToString();
                                        tblTraceError error1 = new tblTraceError();
                                        error1.Error = mydata.Error;
                                        error1.PageError = mydata.PageError;
                                        error1.DateError = mydata.DateError;
                                        error1.DetailError = mydata.DetailError;

                                        ed.tblTraceErrors.InsertOnSubmit(error1);
                                        ed.SubmitChanges();


                                    }



                                }
                                else
                                {
                                    string dob = "DOB is not valid !!";
                                    return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                string dob = "DOB is not valid !!";
                                return Json(new { dob }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        else
                        {
                            fm.DOB = null;
                        }


                        fm.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusCode);
                        if (Convert.ToString(fm.MaritalStatusCode) == "2")
                        {
                            fm.DOM = null;

                        }
                        else
                        {
                            if (mydata.DOM == null)
                            {
                                string domm = "Date of marriage not null !!";
                                return Json(new { domm }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                if (DateTime.TryParseExact(nn3.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate2))
                                {
                                    DOM = ValidDate2;

                                    if (DOM <= DateTime.Now.Date)
                                    {
                                        if (mydata.Dependent_DOB == mydata.DOM)
                                        {

                                            string des = "Date of marriage and Date of Birth Not Equal !!";
                                            return Json(new { des }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            var nn = mydata.DOM;

                                            //new 03-03-2018//
                                            if (nn != null)
                                            {
                                                DateTime ValidDate;
                                                //DateTime? ValidDate2;

                                                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                                {
                                                    fm.DOM = ValidDate;
                                                }
                                            }
                                            else
                                            {
                                                mydata.Error = "date of dependent edit error";
                                                mydata.PageError = "/View/RSBGeneralInfrm";
                                                mydata.DateError = DateTime.Now;
                                                mydata.DetailError = nn.ToString();
                                                tblTraceError error1 = new tblTraceError();
                                                error1.Error = mydata.Error;
                                                error1.PageError = mydata.PageError;
                                                error1.DateError = mydata.DateError;
                                                error1.DetailError = mydata.DetailError;

                                                ed.tblTraceErrors.InsertOnSubmit(error1);
                                                ed.SubmitChanges();


                                            }

                                        }
                                    }
                                    else
                                    {
                                        string dom = "Date of marriage is not valid !!";
                                        return Json(new { dom }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    string dom = "Date of marriage is not valid !!";
                                    return Json(new { dom }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        fm.Dependent_Id = mydata.Dependent_Id;
                        //chnges 07-03-2018//
                        fm.Wife_recorded_in_PPO_no = mydata.Wife_recorded_in_PPO_no;
                        fm.Wife_OtherName = mydata.Wife_othername;

                        fm.Number_Of_children = Convert.ToInt16(mydata.Number_Of_children);
                        fm.Recorded_in_DO_Part2 = mydata.Recorded_in_DO_Part2;
                        fm.Recorded_in_DO_Part2text = mydata.Recorded_in_DO_Part2text;
                        //end
                        dta.tblfamilydetails.InsertOnSubmit(fm);

                        dta.SubmitChanges();

                        ViewBag.Message = "saved";

                        Session["Idd"] = fm.Dependent_Id;


                        return Json(new { fm }, JsonRequestBehavior.AllowGet);



                    }
                }
                catch (Exception ex)
                {
                    // throw ex;
                    //changes on 13-03-2018
                    mydata.Error = ex.Message;
                    mydata.PageError = "/View/RSBGeneralInfrm";
                    mydata.DateError = DateTime.Now;
                    mydata.DetailError = ex.ToString();
                    tblTraceError error = new tblTraceError();
                    error.Error = mydata.Error;
                    error.PageError = mydata.PageError;
                    error.DateError = mydata.DateError;
                    error.DetailError = mydata.DetailError;

                    ed.tblTraceErrors.InsertOnSubmit(error);
                    ed.SubmitChanges();
                    //end
                    return Json(JsonRequestBehavior.AllowGet);
                }
                return Json(JsonRequestBehavior.AllowGet);
            }




        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep3(Generalform mydata)
        {
            try
            {
                bool check = dta.sanik_otherinformations.Any(tbl => tbl.Army_No == mydata.Army_No);
                if (check == true)
                {
                    string output = "exist";
                    return Json(new { output }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int sr = 1;
                    sanik_otherinformation other = new sanik_otherinformation();
                    if (Session["Armyno"] != null)
                    {
                        other.Army_No = Session["Armyno"].ToString();
                    }
                    else
                    {
                        other.Army_No = mydata.Army_No;
                    }

                    other.Character_Id = mydata.Character_Id;
                    other.medical_ID = mydata.medical_ID;
                    other.Rank_ID = mydata.Rank_ID;
                    var nn = mydata.RetirementDate;

                    //new 03-03-2018//
                    if (nn != null)
                    {
                        DateTime ValidDate;
                        //DateTime? ValidDate2;

                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                        {
                            other.RetirementDate = ValidDate;
                        }
                    }
                    else
                    {
                        other.RetirementDate = null;
                    }

                    other.Force_Cat_ID = mydata.Force_Cat_ID;
                    other.Force_Dept_Id = mydata.Force_Dept_Id;
                    dta.sanik_otherinformations.InsertOnSubmit(other);
                    sr = sr + 1;
                    dta.SubmitChanges();

                    mydata.Error = Convert.ToString(sr);
                    mydata.PageError = "/View/RSBGeneralInfrm/Otherinformation";
                    mydata.DateError = DateTime.Now;
                    mydata.DetailError = null;
                    tblTraceError error = new tblTraceError();
                    error.Error = mydata.Error;
                    error.PageError = mydata.PageError;
                    error.DateError = mydata.DateError;
                    error.DetailError = mydata.DetailError;

                    ed.tblTraceErrors.InsertOnSubmit(error);
                    ed.SubmitChanges();
                    return Json(other, JsonRequestBehavior.AllowGet);
                }




            }
            catch (Exception ex)
            {
                mydata.Error = ex.Message;
                mydata.PageError = "/View/RSBGeneralInfrm/Otherinformation";
                mydata.DateError = DateTime.Now;
                mydata.DetailError = ex.ToString();
                tblTraceError error = new tblTraceError();
                error.Error = mydata.Error;
                error.PageError = mydata.PageError;
                error.DateError = mydata.DateError;
                error.DetailError = mydata.DetailError;

                ed.tblTraceErrors.InsertOnSubmit(error);
                ed.SubmitChanges();
                //end
                return Json(JsonRequestBehavior.AllowGet);

            }

        }
        //update step3
        public JsonResult updatestep3(Generalform mydata)
        {
            try
            {


                sanik_otherinformation other = dta.sanik_otherinformations.Where(x => x.Army_No == (string)Session["armynoalrd"] || x.Army_No == (string)Session["Armyno"]).FirstOrDefault();
                if (other != null)
                {
                    int sr = 1;
                    other.Army_No = mydata.Army_No;
                    other.Character_Id = mydata.Character_Id;
                    other.medical_ID = mydata.medical_ID;
                    other.Rank_ID = mydata.Rank_ID;
                    var nn = mydata.RetirementDate;

                    //new 03-03-2018//
                    if (nn != null)
                    {
                        DateTime ValidDate;
                        //DateTime? ValidDate2;

                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                        {
                            other.RetirementDate = ValidDate;
                        }
                    }
                    else
                    {
                        other.RetirementDate = null;
                    }
                    other.Force_Cat_ID = mydata.Force_Cat_ID;
                    other.Force_Dept_Id = mydata.Force_Dept_Id;
                    sr = sr + 1;
                    dta.SubmitChanges();



                    mydata.Error = Convert.ToString(sr);
                    mydata.PageError = "/View/RSBGeneralInfrm/Otherinformation";
                    mydata.DateError = DateTime.Now;
                    mydata.DetailError = null;
                    tblTraceError error = new tblTraceError();
                    error.Error = mydata.Error;
                    error.PageError = mydata.PageError;
                    error.DateError = mydata.DateError;
                    error.DetailError = mydata.DetailError;

                    ed.tblTraceErrors.InsertOnSubmit(error);
                    ed.SubmitChanges();
                    return Json(other, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    string msg1 = "not";
                    return Json(msg1, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                mydata.Error = ex.Message;
                mydata.PageError = "/View/RSBGeneralInfrm/Otherinformation";
                mydata.DateError = DateTime.Now;
                mydata.DetailError = ex.ToString();
                tblTraceError error = new tblTraceError();
                error.Error = mydata.Error;
                error.PageError = mydata.PageError;
                error.DateError = mydata.DateError;
                error.DetailError = mydata.DetailError;

                ed.tblTraceErrors.InsertOnSubmit(error);
                ed.SubmitChanges();
                //end
                return Json(JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep4(Generalform mydata, string IsEditcort)
        {
            string msg;
            bool caseno, caseyear, courtname;


            caseno = IsNumber(mydata.Case_No);
            caseyear = IsNumber(mydata.Case_Year);
            courtname = IsName(mydata.Court_Name);
            if (caseyear == false || caseno == false || courtname == false)
            {

                if (caseyear == false)
                {
                    msg = "cyear";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (caseno == false)
                {
                    msg = "cse";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (courtname == false)
                {
                    msg = "crt";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                try
                {

                    Force_Court_Case crt = new Force_Court_Case();
                    if (Session["Armyno"] != null)
                    {
                        crt.Army_No = Session["Armyno"].ToString();
                    }
                    else
                    {
                        crt.Army_No = mydata.Army_No;
                    }

                    crt.Case_No = mydata.Case_No;
                    crt.Case_Year = mydata.Case_Year;
                    crt.Court_Name = mydata.Court_Name;
                    crt.Decision = mydata.Decision;
                    dta.Force_Court_Cases.InsertOnSubmit(crt);

                    dta.SubmitChanges();

                    ViewBag.Message = "saved";

                    return Json(JsonRequestBehavior.AllowGet);



                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            return Json(JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep5(Generalform mydata, string IsEditcomplain)
        {
            string msg;
            bool nameofcmpln, Levelofdecision, Pending_With;


            Levelofdecision = Citizen(mydata.Level_of_decision);
            Pending_With = IsName(mydata.Pending_With);
            nameofcmpln = IsName(mydata.Name_of_Complain);
            if (Levelofdecision == false || nameofcmpln == false || Pending_With == false)
            {

                if (Levelofdecision == false)
                {
                    msg = "lvlofdecsn";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (Pending_With == false)
                {
                    msg = "pndwth";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (nameofcmpln == false)
                {
                    msg = "nmecmpln";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {

                    Force_Complaint cmp = new Force_Complaint();
                    if (Session["Armyno"] != null)
                    {
                        cmp.Army_No = Session["Armyno"].ToString();
                    }
                    else
                    {
                        cmp.Army_No = mydata.Army_No;
                    }

                    cmp.Name_of_Complain = mydata.Name_of_Complain;
                    cmp.Level_of_decision = mydata.Level_of_decision;
                    if (mydata.Date_of_Complain != null)
                    {
                        var nn = mydata.Date_of_Complain;

                        //new 03-03-2018//
                        if (nn != null)
                        {
                            DateTime ValidDate;
                            //DateTime? ValidDate2;

                            if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                            {
                                cmp.Date_of_Complain = ValidDate;
                            }
                        }
                        else
                        {
                            cmp.Date_of_Complain = null;
                        }
                    }

                    cmp.Pending_With = mydata.Pending_With;
                    cmp.Decision_Given = mydata.Decision_Given;

                    dta.Force_Complaints.InsertOnSubmit(cmp);

                    dta.SubmitChanges();

                    ViewBag.Message = "saved";






                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return Json(JsonRequestBehavior.AllowGet);
            }



        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep6(Generalform mydata)
        {
            string msg;
            bool outstandingamount, loanamount;


            outstandingamount = Decimalchk(Convert.ToString(mydata.Outstanding_Amount));
            loanamount = Decimalchk(Convert.ToString(mydata.Loan_Amount));

            if (loanamount == false || outstandingamount == false)
            {

                if (outstandingamount == false)
                {
                    msg = "outstnding";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (loanamount == false)
                {
                    msg = "loanamount";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }

                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    Sanik_Loan loan = new Sanik_Loan();
                    if (Session["Armyno"] != null)
                    {
                        loan.Army_No = Session["Armyno"].ToString();
                    }
                    else
                    {
                        loan.Army_No = mydata.Army_No;
                    }

                    loan.Loan_Amount = mydata.Loan_Amount;
                    if (mydata.Date_loan != null)
                    {
                        var nn = mydata.Date_loan;

                        //new 03-03-2018//
                        if (nn != null)
                        {
                            DateTime ValidDate;
                            //DateTime? ValidDate2;

                            if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                            {
                                loan.Date_loan = ValidDate;
                            }
                            else
                            {
                                loan.Date_loan = null;
                            }
                        }
                    }
                    //loan.Date_loan = mydata.Date_loan;
                    loan.Purpose = mydata.Purpose;
                    loan.Remarks = mydata.Remarks;
                    if (mydata.Outstanding_Amount <= mydata.Loan_Amount)
                    {
                        loan.Outstanding_Amount = mydata.Outstanding_Amount;
                        dta.Sanik_Loans.InsertOnSubmit(loan);
                        dta.SubmitChanges();

                    }
                    else
                    {
                        string output = "Enter wrong amount";
                        return Json(new { output }, JsonRequestBehavior.AllowGet);
                    }


                    ViewBag.Message = "saved";

                    return Json(JsonRequestBehavior.AllowGet);




                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }


        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep7(Generalform mydata)
        {
            rsbgeneralUPDataContext dtaNew = new rsbgeneralUPDataContext();
            try
            {
                bool check = dtaNew.rsbgenerals.Any(tbl => tbl.Army_No == mydata.Army_No && tbl.Bank_Acc_no != null);
                if (check == true)
                {
                    string output = "exist";
                    return Json(new { output }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //********************************//
                    using (SqlConnection con = new SqlConnection(Constrg1))
                    {
                        string arm = Convert.ToString(Session["Armyno"]);
                        SqlCommand cmd1 = new SqlCommand("update rsbgeneral set BankID=@BankID,Bank_Acc_no=@Bank_Acc_no,Bank_IFSC=@Bank_IFSC where Army_No=@Army_No OR Army_No='" + arm + "'", con);
                        if (cmd1 != null)
                        {
                            cmd1.Parameters.AddWithValue("@Army_No", mydata.Army_No);
                            cmd1.Parameters.AddWithValue("@BankID", mydata.BankID);
                       
                            cmd1.Parameters.AddWithValue("@Bank_Acc_no", mydata.Bank_Acc_no);
                            if (mydata.Bank_IFSC == null)
                            {
                                cmd1.Parameters.AddWithValue("@Bank_IFSC", DBNull.Value);
                            }
                            else
                            {
                                cmd1.Parameters.AddWithValue("@Bank_IFSC", mydata.Bank_IFSC);
                            }
                           // cmd1.Parameters.AddWithValue("@Bank_IFSC", mydata.Bank_IFSC);
                            con.Open();
                            cmd1.ExecuteNonQuery();
                            con.Close();
                        }
                        //****************************//

                       // rsbgeneral rsb1 = new rsbgeneral();
                        // rsb1 = dtaNew.rsbgenerals.Where(x => x.Army_No == mydata.Army_No).FirstOrDefault();

                       //// var query = from rs in dta.rsbgenerals where rs.Army_No == mydata.Army_No  select rs;
                        // if (rsb1 != null)
                        // {


                       //     rsb1.BankID = mydata.BankID;
                        //     rsb1.Bank_Acc_no = mydata.Bank_Acc_no;
                        //     rsb1.Bank_IFSC = mydata.Bank_IFSC;

                       //        // dta.Refresh(RefreshMode.KeepChanges, rsb1);
                        //         dtaNew.SubmitChanges();


                       // }


                        else
                        {
                            string msg = "no record corresponding to this army number";
                            return Json(msg, JsonRequestBehavior.AllowGet);
                        }

                    }

                    return Json(JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                mydata.Error = ex.Message;
                mydata.PageError = "/View/RSBGeneralInfrm/BAnk";
                mydata.DateError = DateTime.Now;
                mydata.DetailError = ex.ToString();
                tblTraceError error = new tblTraceError();
                error.Error = mydata.Error;
                error.PageError = mydata.PageError;
                error.DateError = mydata.DateError;
                error.DetailError = mydata.DetailError;

                ed.tblTraceErrors.InsertOnSubmit(error);
                ed.SubmitChanges();
                //end
                return Json(JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult updatestep7(Generalform mydata)
        {
            try
            {

                using (SqlConnection con1 = new SqlConnection(Constrg1))
                {
                    string arm = Convert.ToString(Session["Armyno"]);
                 
                    string str = "update rsbgeneral set BankID=@BankID,Bank_Acc_no=@Bank_Acc_no,Bank_IFSC=@Bank_IFSC where Army_No=@Army_No or Army_No='" + arm + "' ";
                    SqlCommand cmd2 = new SqlCommand(str, con1);
                    if (cmd2 != null)
                    {
                        cmd2.Parameters.AddWithValue("@Army_No", mydata.Army_No);
                        cmd2.Parameters.AddWithValue("@BankID", mydata.BankID);
                        if (mydata.Bank_IFSC == null)
                        {
                            cmd2.Parameters.AddWithValue("@Bank_IFSC", DBNull.Value);
                        }
                        else
                        {
                            cmd2.Parameters.AddWithValue("@Bank_IFSC", mydata.Bank_IFSC);
                        }
                        cmd2.Parameters.AddWithValue("@Bank_Acc_no", mydata.Bank_Acc_no);
                       
                        con1.Open();
                        cmd2.ExecuteNonQuery();
                        con1.Close();
                    }
                    //****************************//

                   // rsbgeneral rsb1 = new rsbgeneral();
                    // rsb1 = dtaNew.rsbgenerals.Where(x => x.Army_No == mydata.Army_No).FirstOrDefault();

                   //// var query = from rs in dta.rsbgenerals where rs.Army_No == mydata.Army_No  select rs;
                    // if (rsb1 != null)
                    // {


                   //     rsb1.BankID = mydata.BankID;
                    //     rsb1.Bank_Acc_no = mydata.Bank_Acc_no;
                    //     rsb1.Bank_IFSC = mydata.Bank_IFSC;

                   //        // dta.Refresh(RefreshMode.KeepChanges, rsb1);
                    //         dtaNew.SubmitChanges();


                   // }


                    else
                    {
                        string msg = "no record  corresponding to this army number";
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                }




                return Json(JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep8(Generalform mydata)
        {
            try
            {

                Sanik_award awd = new Sanik_award();
                if ((Session["Armyno"]) != null)
                {
                    awd.Army_No = Session["Armyno"].ToString();
                }
                else
                {
                    awd.Army_No = mydata.Army_No;
                }

                awd.awardID = mydata.awardID;
                if (mydata.award_date == null)
                {
                    awd.award_date = null;
                }
                else
                {
                    var nn = mydata.award_date;

                    //new 03-03-2018//
                    if (nn != null)
                    {
                        DateTime ValidDate;
                        //DateTime? ValidDate2;

                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                        {
                            awd.award_date = ValidDate;
                        }
                        else
                        {
                            awd.award_date = null;
                        }
                    }

                    //awd.award_date = mydata.award_date;
                }

                awd.Perpose = mydata.Perpose;
                dta.Sanik_awards.InsertOnSubmit(awd);

                dta.SubmitChanges();

                return Json(JsonRequestBehavior.AllowGet);





            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public JsonResult RSBdependent()
        {
            List<Generalform> cd = new List<Generalform>();
            var cd2 = (from c in dta.tblfamilydetails
                       //join mrt in ed.tblMaritalStatusMasters on c.MaritalStatusCode equals mrt.MaritalStatusCode
                       join rel in dta.tblsanikRelations on c.RelationCode equals rel.RelationCode
                       where (c.Army_No == Convert.ToString(Session["Armyno"]))
                       select new Generalform
                       {
                           // MaritalStatusDesc = mrt.MaritalStatusDesc,
                           MaritalStatusCode = Convert.ToString(c.MaritalStatusCode),
                           RelationDesc = rel.RelationDesc,
                           Dependent_Id = c.Dependent_Id,
                           Army_No = c.Army_No,
                           Dependent_Name = c.Dependent_Name,
                           UID = c.UID,
                           Dependent_DOB1 = Convert.ToDateTime(c.DOB),
                           DOM1 = Convert.ToDateTime(c.DOM)


                       }).ToList();
            var cd3 = from mat in dta.MaritalStatus
                      select new Generalform
                      {
                          MaritalStatusCode = Convert.ToString(mat.Marital_Code),
                          MaritalStatusDesc = mat.Marital_Status
                      };
            var cd1 = (from f in cd2
                       join m in cd3 on f.MaritalStatusCode equals m.MaritalStatusCode
                       //where f.Army_No==amno
                       select new Generalform
                       {
                           MaritalStatusDesc = m.MaritalStatusDesc,
                           Dependent_Id = f.Dependent_Id,
                           Army_No = f.Army_No,
                           RelationDesc = f.RelationDesc,
                           Dependent_Name = f.Dependent_Name,
                           UID = f.UID,
                           Dependent_DOB1 = Convert.ToDateTime(f.Dependent_DOB1),
                           DOM1 = Convert.ToDateTime(f.DOM),


                       }).ToList();



            if (cd1.Count() > 0)
            {


                foreach (var item in cd1)
                {
                    Generalform gn = new Generalform();
                    var imgg = from c in dta.tblfamilydetails where ((c.Army_No == item.Army_No) && (c.Dependent_Id == item.Dependent_Id)) select c;
                    gn.Dependent_Id = item.Dependent_Id;
                    gn.Army_No = item.Army_No;
                    gn.Dependent_Name = item.Dependent_Name;
                    gn.RelationDesc = item.RelationDesc;
                    var str = item.UID; ;
                    if (str == null)
                    {
                        gn.UID = item.UID;
                    }
                    else
                    {
                        gn.UID = string.Concat("".PadLeft(9, '*'), str.Substring(str.Length - 4));
                    }


                    // gn.UID = item.UID;
                    if (item.Dependent_DOB1 != null)
                    {
                        gn.Dependent_DOB1 = Convert.ToDateTime(item.Dependent_DOB1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Dependent_DOB1 = Convert.ToDateTime(datee);
                    }

                    gn.MaritalStatusDesc = item.MaritalStatusDesc;
                    gn.DOM1 = Convert.ToDateTime(item.DOM);

                    if (imgg.Count() > 0)
                    {
                        if (imgg.FirstOrDefault().imagedept != null)
                        {
                            byte[] array = (imgg.FirstOrDefault().imagedept).ToArray();

                            gn.ImgByte = Convert.ToBase64String(array);



                        }
                        else
                        {


                            gn.ImgByte = null;
                        }

                    }
                    else
                    {
                        gn.ImgByte = null;

                    }

                    cd.Add(gn);


                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(JsonRequestBehavior.AllowGet);
            }





        }
        //already bind
        public JsonResult RSBdependentalready(string amno)
        {
            List<Generalform> cd = new List<Generalform>();


            var cd2 = (from c in dta.tblfamilydetails
                       join rel in dta.tblsanikRelations on c.RelationCode equals rel.RelationCode
                       where (c.Army_No == amno)
                       select new Generalform
                       {
                           MaritalStatusCode = Convert.ToString(c.MaritalStatusCode),
                           RelationDesc = rel.RelationDesc,
                           Dependent_Id = c.Dependent_Id,
                           Army_No = c.Army_No,
                           Dependent_Name = c.Dependent_Name,
                           UID = c.UID,

                           Dependent_DOB1 = c.DOB,
                           DOM1 = c.DOM,
                           Wife_othername = c.Wife_OtherName


                       }).ToList();
            var cd3 = from mat in dta.MaritalStatus
                      select new Generalform
                      {
                          MaritalStatusCode = Convert.ToString(mat.Marital_Code),
                          MaritalStatusDesc = mat.Marital_Status
                      };
            var cd1 = (from f in cd2
                       join m in cd3 on f.MaritalStatusCode equals m.MaritalStatusCode
                       //where f.Army_No==amno
                       select new Generalform
                       {
                           MaritalStatusDesc = m.MaritalStatusDesc,
                           Dependent_Id = f.Dependent_Id,
                           Army_No = f.Army_No,
                           RelationDesc = f.RelationDesc,
                           Dependent_Name = f.Dependent_Name,
                           UID = f.UID,
                           Dependent_DOB1 = f.Dependent_DOB1,
                           DOM1 = f.DOM1,
                           Wife_othername = f.Wife_othername


                       }).ToList();






            if (cd1.Count() > 0)
            {


                foreach (var item in cd1)
                {
                    Generalform gn = new Generalform();


                    gn.Dependent_Id = item.Dependent_Id;
                    var imgg = from c in dta.tblfamilydetails where ((c.Army_No == item.Army_No) && (c.Dependent_Id == gn.Dependent_Id)) select c;
                    gn.Army_No = item.Army_No;
                    gn.Dependent_Name = item.Dependent_Name;
                    gn.RelationDesc = item.RelationDesc;
                    var str = item.UID;
                    if (str == null)
                    {
                        gn.UID = item.UID;
                    }
                    else
                    {
                        gn.UID = string.Concat("".PadLeft(9, '*'), str.Substring(str.Length - 4));
                    }
                    // gn.UID = item.UID;
                    if (item.Dependent_DOB1 != null)
                    {
                        gn.Dependent_DOB1 = Convert.ToDateTime(item.Dependent_DOB1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Dependent_DOB1 = Convert.ToDateTime(datee);
                    }
                    gn.MaritalStatusDesc = item.MaritalStatusDesc;
                    if (item.DOM1 != null)
                    {
                        gn.DOM1 = Convert.ToDateTime(item.DOM1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.DOM1 = Convert.ToDateTime(datee);
                    }



                    if (imgg.FirstOrDefault().imagedept != null)
                    {
                        byte[] array = (imgg.FirstOrDefault().imagedept).ToArray();

                        gn.ImgByte = Convert.ToBase64String(array);



                    }
                    else
                    {


                        gn.ImgByte = null;
                    }

                    gn.Wife_othername = item.Wife_othername;

                    cd.Add(gn);


                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string output = "empty";
                return Json(new { output }, JsonRequestBehavior.AllowGet);
            }





        }

        public JsonResult RSBCourtalready(string amno)
        {


            Force_Court_Case fr = new Force_Court_Case();
            List<Generalform> _test1 = null;


            _test1 = (from force in dta.Force_Court_Cases

                      join rb in dta.rsbgenerals on force.Army_No equals rb.Army_No
                      where (force.Army_No == amno)
                      select new Generalform
                      {
                          Army_No = force.Army_No,
                          Court_Case_Id = force.Court_Case_Id,
                          Case_No = force.Case_No,
                          Case_Year = force.Case_Year,
                          Court_Name = force.Court_Name,
                          Decision = force.Decision
                      }).ToList();

            if (_test1.Count() > 0)
            {
                return Json(_test1, JsonRequestBehavior.AllowGet);
            }


            else
            {
                string output = "empty";
                return Json(new { output }, JsonRequestBehavior.AllowGet);
            }


        }

        public JsonResult RSBComplainalready(string amno)
        {

            List<Generalform> cd = new List<Generalform>();

            var _test2 = (from forc in dta.Force_Complaints

                          join rb in dta.rsbgenerals on forc.Army_No equals rb.Army_No
                          where (forc.Army_No == amno)
                          select new Generalform
                          {
                              Army_No = forc.Army_No,
                              Complain_Id = forc.Complain_Id,
                              Name_of_Complain = forc.Name_of_Complain,
                              Level_of_decision = forc.Level_of_decision,
                              Date_of_Complain1 = forc.Date_of_Complain,
                              Pending_With = forc.Pending_With,
                              Decision_Given = forc.Decision_Given

                          }).ToList();

            if (_test2.Count() > 0)
            {
                foreach (var item in _test2)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.Complain_Id = item.Complain_Id;
                    gn.Name_of_Complain = item.Name_of_Complain;
                    gn.Level_of_decision = item.Level_of_decision;
                    if (item.Date_of_Complain1 != null)
                    {
                        gn.Date_of_Complain1 = Convert.ToDateTime(item.Date_of_Complain1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Date_of_Complain1 = Convert.ToDateTime(datee);
                    }

                    gn.Pending_With = item.Pending_With;
                    gn.Decision_Given = item.Decision_Given;
                    cd.Add(gn);
                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string output = "empty";
                return Json(new { output }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult RSBLoanalready(string amno)
        {

            List<Generalform> cd = new List<Generalform>();
            var _test3 = (from ln in dta.Sanik_Loans

                          join rb in dta.rsbgenerals on ln.Army_No equals rb.Army_No
                          where (ln.Army_No == amno)
                          select new Generalform
                          {
                              Army_No = ln.Army_No,
                              Loan_id = ln.Loan_Id,
                              Loan_Amount = ln.Loan_Amount,
                              Date_loan1 = ln.Date_loan,
                              Purpose = ln.Purpose,
                              Outstanding_Amount = ln.Outstanding_Amount,
                              Remarks = ln.Remarks


                          }).ToList();
            if (_test3.Count() > 0)
            {

                foreach (var item in _test3)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.Loan_id = item.Loan_id;
                    gn.Loan_Amount = item.Loan_Amount;
                    if (item.Date_loan1 != null)
                    {
                        gn.Date_loan1 = item.Date_loan1;
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Date_loan1 = Convert.ToDateTime(datee);
                    }

                    gn.Purpose = item.Purpose;
                    gn.Outstanding_Amount = item.Outstanding_Amount;
                    gn.Remarks = item.Remarks;


                    cd.Add(gn);
                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string output = "empty";
                return Json(new { output }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult RSBawardalready(string amno)
        {

            List<Generalform> cd = new List<Generalform>();
            var _test3 = (from ln in dta.Sanik_awards

                          join rb in dta.rsbgenerals on ln.Army_No equals rb.Army_No
                          join awd in dta.tblawards on ln.awardID equals awd.awardID
                          where (ln.Army_No == amno)
                          select new Generalform
                          {
                              Army_No = ln.Army_No,
                              awardName = awd.awardName,
                              award_date1 = ln.award_date,
                              Sanikawrdid = ln.Sanikawrdid,
                              Perpose = ln.Perpose,

                          }).ToList();
            if (_test3.Count() > 0)
            {
                foreach (var item in _test3)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.awardID = item.awardID;
                    if (item.award_date1 != null)
                    {
                        gn.award_date1 = item.award_date1;
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.award_date1 = Convert.ToDateTime(datee);
                    }

                    gn.Sanikawrdid = item.Sanikawrdid;
                    gn.Perpose = item.Perpose;
                    cd.Add(gn);
                }
                return Json(_test3, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string output = "empty";
                return Json(new { output }, JsonRequestBehavior.AllowGet);
            }



        }
        //end

        [HttpPost]
        public JsonResult RSBdependentupdate(string d)
        {
            List<Generalform> cd = new List<Generalform>();
            var cd2 = (from c in dta.tblfamilydetails
                       // join mrt in ed.tblMaritalStatusMasters on c.MaritalStatusCode equals mrt.MaritalStatusCode
                       join rel in dta.tblsanikRelations on c.RelationCode equals rel.RelationCode
                       where (c.UID == d)
                       select new Generalform
                       {
                           // MaritalStatusDesc = mrt.MaritalStatusDesc,
                           MaritalStatusCode = Convert.ToString(c.MaritalStatusCode),
                           RelationDesc = rel.RelationDesc,
                           Dependent_Id = c.Dependent_Id,
                           Army_No = c.Army_No,
                           Dependent_Name = c.Dependent_Name,
                           UID = c.UID,
                           Dependent_DOB1 = Convert.ToDateTime(c.DOB),
                           DOM1 = Convert.ToDateTime(c.DOM),


                       }).ToList();
            var cd3 = from mat in dta.MaritalStatus
                      select new Generalform
                      {
                          MaritalStatusCode = Convert.ToString(mat.Marital_Code),
                          MaritalStatusDesc = mat.Marital_Status
                      };
            foreach (var item1 in cd2)
            {

                if (item1.MaritalStatusCode != null)
                {
                    var cd1 = (from f in cd2
                               join m in cd3 on f.MaritalStatusCode equals m.MaritalStatusCode

                               select new Generalform
                               {
                                   MaritalStatusDesc = m.MaritalStatusDesc,
                                   Dependent_Id = f.Dependent_Id,
                                   Army_No = f.Army_No,
                                   RelationDesc = f.RelationDesc,
                                   Dependent_Name = f.Dependent_Name,
                                   UID = f.UID,
                                   Dependent_DOB1 = Convert.ToDateTime(f.Dependent_DOB),
                                   DOM1 = Convert.ToDateTime(f.DOM),


                               }).ToList();


                    if (cd1.Count() > 0)
                    {

                        string ImgByte;
                        foreach (var item in cd1)
                        {
                            Generalform gn = new Generalform();
                            var imgg = from c in dta.tblfamilydetails where ((c.Army_No == item.Army_No) && (c.UID == item.UID)) select c;
                            gn.Dependent_Id = item.Dependent_Id;
                            gn.Army_No = item.Army_No;
                            gn.Dependent_Name = item.Dependent_Name;
                            gn.RelationDesc = item.RelationDesc;
                            gn.UID = string.Concat("".PadLeft(9, '*'), item.UID.Substring(item.UID.Length - 4));

                            if (item1.Dependent_DOB != null)
                            {
                                gn.Dependent_DOB1 = Convert.ToDateTime(item.Dependent_DOB);
                            }
                            else
                            {
                                string datee = "01/01/0001";
                                gn.Dependent_DOB1 = Convert.ToDateTime(datee);
                            }

                            gn.MaritalStatusDesc = item.MaritalStatusDesc;
                            if (item.DOM1 != null)
                            {


                                gn.DOM1 = Convert.ToDateTime(item.DOM);

                            }
                            else
                            {
                                string datee = "01/01/0001";
                                gn.DOM1 = Convert.ToDateTime(datee);
                            }




                            if (imgg.FirstOrDefault().imagedept != null)
                            {
                                byte[] array = (imgg.FirstOrDefault().imagedept).ToArray();

                                gn.ImgByte = Convert.ToBase64String(array);



                            }
                            else
                            {


                                gn.ImgByte = null;
                            }



                            cd.Add(gn);


                        }
                        return Json(cd, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        return Json(JsonRequestBehavior.AllowGet);
                    }


                }

                else
                {
                    var cd1 = (from f in cd2

                               select new Generalform
                               {

                                   Dependent_Id = f.Dependent_Id,
                                   Army_No = f.Army_No,
                                   RelationDesc = f.RelationDesc,
                                   Dependent_Name = f.Dependent_Name,
                                   UID = f.UID,
                                   Dependent_DOB1 = Convert.ToDateTime(f.Dependent_DOB),
                                   DOM1 = Convert.ToDateTime(f.DOM),


                               }).ToList();


                    if (cd1.Count() > 0)
                    {

                        string ImgByte;
                        foreach (var item in cd1)
                        {
                            Generalform gn = new Generalform();
                            var imgg = from c in dta.tblfamilydetails where ((c.Army_No == item.Army_No) && (c.UID == item.UID)) select c;
                            gn.Dependent_Id = item.Dependent_Id;
                            gn.Army_No = item.Army_No;
                            gn.Dependent_Name = item.Dependent_Name;
                            gn.RelationDesc = item.RelationDesc;
                            gn.UID = item.UID;
                            if (item.Dependent_DOB != null)
                            {
                                gn.Dependent_DOB1 = Convert.ToDateTime(item.Dependent_DOB);
                            }
                            else
                            {
                                string datee = "01/01/0001";
                                gn.Dependent_DOB1 = Convert.ToDateTime(datee);
                            }

                            gn.MaritalStatusDesc = item.MaritalStatusDesc;
                            if (item.DOM1 != null)
                            {
                                gn.DOM1 = Convert.ToDateTime(item.DOM1);
                            }
                            else
                            {
                                string datee = "01/01/0001";
                                gn.DOM1 = Convert.ToDateTime(datee);
                            }




                            if (imgg.FirstOrDefault().imagedept != null)
                            {
                                byte[] array = (imgg.FirstOrDefault().imagedept).ToArray();

                                gn.ImgByte = Convert.ToBase64String(array);



                            }
                            else
                            {


                                gn.ImgByte = null;
                            }



                            cd.Add(gn);


                        }
                        return Json(cd, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        return Json(JsonRequestBehavior.AllowGet);
                    }

                }
            }


            return Json(JsonRequestBehavior.AllowGet);




        }

        public JsonResult RSBCourt()
        {


            Force_Court_Case fr = new Force_Court_Case();
            List<Generalform> _test1 = null;


            _test1 = (from force in dta.Force_Court_Cases

                      join rb in dta.rsbgenerals on force.Army_No equals rb.Army_No
                      where (force.Army_No == Convert.ToString(Session["Armyno"]))
                      select new Generalform
                      {
                          Army_No = force.Army_No,
                          Court_Case_Id = force.Court_Case_Id,
                          Case_No = force.Case_No,
                          Case_Year = force.Case_Year,
                          Court_Name = force.Court_Name,
                          Decision = force.Decision
                      }).ToList();

            return Json(_test1, JsonRequestBehavior.AllowGet);



        }
        public JsonResult RSBComplain()
        {
            List<Generalform> cd = new List<Generalform>();

            var _test2 = (from forc in dta.Force_Complaints

                          join rb in dta.rsbgenerals on forc.Army_No equals rb.Army_No
                          where (forc.Army_No == Convert.ToString(Session["Armyno"]))
                          select new Generalform
                          {
                              Army_No = forc.Army_No,
                              Complain_Id = forc.Complain_Id,
                              Name_of_Complain = forc.Name_of_Complain,
                              Level_of_decision = forc.Level_of_decision,
                              Date_of_Complain1 = forc.Date_of_Complain,
                              Pending_With = forc.Pending_With,
                              Decision_Given = forc.Decision_Given

                          }).ToList();

            if (_test2.Count() > 0)
            {
                foreach (var item in _test2)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.Complain_Id = item.Complain_Id;
                    gn.Name_of_Complain = item.Name_of_Complain;
                    gn.Level_of_decision = item.Level_of_decision;
                    if (item.Date_of_Complain1 != null)
                    {
                        gn.Date_of_Complain1 = Convert.ToDateTime(item.Date_of_Complain1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Date_of_Complain1 = Convert.ToDateTime(datee);
                    }

                    gn.Pending_With = item.Pending_With;
                    gn.Decision_Given = item.Decision_Given;
                    cd.Add(gn);
                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult RSBLoan()
        {

            List<Generalform> cd = new List<Generalform>();
            var _test3 = (from ln in dta.Sanik_Loans

                          join rb in dta.rsbgenerals on ln.Army_No equals rb.Army_No
                          where (ln.Army_No == Convert.ToString(Session["Armyno"]))
                          select new Generalform
                          {
                              Army_No = ln.Army_No,
                              Loan_id = ln.Loan_Id,
                              Loan_Amount = ln.Loan_Amount,
                              Date_loan1 = ln.Date_loan,
                              Purpose = ln.Purpose,
                              Outstanding_Amount = ln.Outstanding_Amount,
                              Remarks = ln.Remarks


                          }).ToList();
            if (_test3.Count() > 0)
            {

                foreach (var item in _test3)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.Loan_id = item.Loan_id;
                    gn.Loan_Amount = item.Loan_Amount;
                    if (item.Date_loan1 != null)
                    {
                        gn.Date_loan1 = item.Date_loan1;
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Date_loan1 = Convert.ToDateTime(datee);
                    }

                    gn.Purpose = item.Purpose;
                    gn.Outstanding_Amount = item.Outstanding_Amount;
                    gn.Remarks = item.Remarks;


                    cd.Add(gn);
                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonRequestBehavior.AllowGet);
            }



        }
        public JsonResult RSBaward()
        {
            List<Generalform> cd = new List<Generalform>();
            var _test3 = (from ln in dta.Sanik_awards

                          join rb in dta.rsbgenerals on ln.Army_No equals rb.Army_No
                          join awd in dta.tblawards on ln.awardID equals awd.awardID
                          where (ln.Army_No == Convert.ToString(Session["Armyno"]))
                          select new Generalform
                          {
                              Army_No = ln.Army_No,
                              awardName = awd.awardName,

                              award_date1 = ln.award_date,
                              Sanikawrdid = ln.Sanikawrdid,
                              Perpose = ln.Perpose,

                          }).ToList();
            if (_test3.Count() > 0)
            {
                foreach (var item in _test3)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.awardID = item.awardID;
                    if (item.award_date1 != null)
                    {
                        gn.award_date1 = item.award_date1;
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.award_date1 = Convert.ToDateTime(datee);
                    }

                    gn.Sanikawrdid = item.Sanikawrdid;
                    gn.Perpose = item.Perpose;
                    cd.Add(gn);
                }
                return Json(_test3, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonRequestBehavior.AllowGet);
            }



        }
        public JsonResult updatestatus(Generalform mydata)
        {
            try
            {
                rsbgeneral rsb = new rsbgeneral();
                //if (mydata.Army_No == null)
                //{
                //    rsb = dta.rsbgenerals.Where(x => x.Army_No == Session["armynoalrd"].ToString()).FirstOrDefault();

                //}
                //else
                //{
                rsb = dta.rsbgenerals.Where(x => x.Army_No == mydata.Army_No).FirstOrDefault();
                //}
                rsb.Status = Convert.ToChar(mydata.status);
                rsb.ChangeUser = (String)Session["userid"];
                rsb.ChangeDate = DateTime.Now;
                dta.SubmitChanges();
                return Json(rsb, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// View Created (Get)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RSBGeneralInfrm()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                maritalstatus();
                gnder();
                forcename();
                Relationship();
                regm();
                cast();
                state();


                rank();
                medical();
                character();
                banklist();
                awardlist();

                return View();
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();

        }

        public JsonResult RSBGeneralInfrm1()
        {
            string user = Convert.ToString(Session["userid"]);
            string usertype = Convert.ToString(Session["usertype"]);

            string dco = Convert.ToString(Session["dcode"]);
            return Json(new { user, usertype, dco }, JsonRequestBehavior.AllowGet);



        }

        //get dep
        [HttpPost]
        public JsonResult getdep(string cidn, string cidid)
        {
            List<Generalform> cd = new List<Generalform>();
            var list = from c in ed.tblCIDRs
                       where c.UID == cidn
                       select c;

            if (cidn != cidid)
            {
                try
                {
                    if (list.Count() > 0)
                    {
                        string ImgByte;
                        foreach (var item in list)
                        {
                            Generalform gn = new Generalform();

                            gn.Dependent_Name = item.Citizen_Name_EN;
                            gn.RelationCode = mydata.RelationCode;
                            gn.Dependent_DOB1 = Convert.ToDateTime(item.DOB);
                            if (item.Marital_Status != null)
                            {
                                gn.MaritalStatusCode = (item.Marital_Status).ToString();

                            }

                            gn.DOM1 = Convert.ToDateTime(item.DOM);

                            if (item.Photo != null)
                            {

                                gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                            }
                            else
                            {


                                gn.ImgByte = null;
                            }

                            cd.Add(gn);


                        }

                    }


                }
                catch (Exception ex)
                {
                    throw ex;


                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string output = "already exist";
                return Json(new { response = output }, JsonRequestBehavior.AllowGet);
            }



        }
        //get uid
        [HttpPost]
        public JsonResult getuid(string uidnn, string search)
        {
            string usertype = Convert.ToString(Session["usertype"]);
            string decode = Convert.ToString(Session["dcode"]);
            if (usertype == "71")
            {

                try
                {
                    Generalform obj = new Generalform();
                    List<Generalform> cd = new List<Generalform>();

                    int chk = 0;
                    // bool check = dta.rsbgenerals.Any(tbl => tbl.Army_No == uidnn || tbl.ESMIdentitycardnumber == uidnn || tbl.Citizen_ID == uidnn || tbl.UID == uidnn||tbl.mobileno==uidnn);
                    //if (check == true)
                    //{
                    //    string output = "already exist";
                    //    return Json(new { output }, JsonRequestBehavior.AllowGet);
                    //}


                    if (search == "armyno")
                    {
                        bool armyno;
                        //int army = uidnn.Length;
                        armyno = armynumber(uidnn);
                        if (armyno == false)
                        {
                            string msg = "armynoo";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            bool check = dta.rsbgenerals.Any(tbl => tbl.Army_No == uidnn);
                            if (check == true)
                            {
                                string output = "already exist";
                                return Json(new { output }, JsonRequestBehavior.AllowGet);
                            }


                            else
                            {
                                Session["armynoalrd"] = uidnn;
                                var othr = from t in dta.sanik_otherinformations where t.Army_No == uidnn select t;
                                if (othr.Count() > 0)
                                {
                                    var list = from a in dta.rsbgenerals
                                               join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                               //where a.Army_No == uidnn ||((a.Army_No==uidnn &&a.dcode==decode))
                                               where a.Army_No == uidnn
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,

                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Character_Id = (o.Character_Id),
                                                   medical_ID = (o.medical_ID),
                                                   Rank_ID = o.Rank_ID,
                                                   RetirementDate1 = o.RetirementDate,

                                                   Force_Cat_ID = o.Force_Cat_ID,

                                                   Force_Dept_Id = o.Force_Dept_Id,

                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.Army_No == uidnn select r;
                                            gn.Army_No = item.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;

                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.Per_cors != null)
                                            {
                                                gn.Per_cors = (item.Per_cors).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;

                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }

                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//

                                            gn.Character_Id = (item.Character_Id);
                                            gn.medical_ID = (item.medical_ID);
                                            gn.Rank_ID = item.Rank_ID;

                                            gn.RetirementDate1 = item.RetirementDate1;

                                            gn.Force_Cat_ID = item.Force_Cat_ID;

                                            gn.Force_Dept_Id = item.Force_Dept_Id;

                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            Session["aarmyno"] = item.Army_No;
                                            cd.Add(gn);
                                        }
                                    }


                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var list = from a in dta.rsbgenerals
                                               //where a.Army_No == uidnn ||((a.Army_No==uidnn &&a.dcode==decode))
                                               where a.Army_No == uidnn
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,

                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusCode = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.Army_No == uidnn select r;
                                            gn.Army_No = item.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;

                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusCode = (item.MaritalStatusCode).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.Per_cors != null)
                                            {
                                                gn.Per_cors = (item.Per_cors).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }

                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            Session["aarmyno"] = item.Army_No;
                                            cd.Add(gn);
                                        }
                                    }
                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }


                            }

                        }

                    }


                    else if (search == "esmino")
                    {
                        bool esmino;
                        esmino = Citizen(uidnn);
                        //int esmo = uidnn.Length;
                        if (esmino == false)
                        {
                            string msg = "esmino";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            bool check = dta.rsbgenerals.Any(tbl => tbl.ESMIdentitycardnumber == uidnn);
                            if (check == true)
                            {
                                string output = "already exist";
                                return Json(new { output }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var rsb = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn select r;
                                if (rsb.Count() == 0)
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {


                                    var othr = from t in dta.sanik_otherinformations where t.Army_No == rsb.FirstOrDefault().Army_No select t;
                                    if (othr.Count() > 0)
                                    {
                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   //where a.ESMIdentitycardnumber == uidnn ||((a.Army_No==uidnn &&a.dcode==decode))
                                                   where a.ESMIdentitycardnumber == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,

                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn select r;
                                                gn.Army_No = item.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;

                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd.Add(gn);
                                            }
                                        }


                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals
                                                   //where a.ESMIdentitycardnumber == uidnn ||((a.Army_No==uidnn &&a.dcode==decode))
                                                   where a.ESMIdentitycardnumber == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,

                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn select r;
                                                gn.Army_No = item.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;

                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }


                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd.Add(gn);
                                            }
                                        }
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                        }
                    }

                    else if (search == "uid" || search == "cid" || search == "mobileno")
                    {
                        bool uid, cid, mobileno;
                        if (search == "uid")
                        {
                            int length = uidnn.Length;


                            uid = IsNumber(uidnn);
                            if (uid == false || length != 12)
                            {
                                string msg = "uidd";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                bool check = dta.rsbgenerals.Any(tbl => tbl.UID == uidnn);
                                if (check == true)
                                {
                                    string output = "already exist";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    List<Generalform> cd1 = new List<Generalform>();
                                    var rsb = from r in dta.rsbgenerals where r.UID == uidnn select r.Army_No;
                                    if (rsb.Count() == 0)
                                    {
                                        string output = "Not Found";
                                        return Json(new { output }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault() select o;
                                        //changes
                                        if (othr.Count() > 0)
                                        {

                                            var list = from a in dta.rsbgenerals
                                                       join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                       //where a.UID == uidnn ||((a.UID==uidnn && a.dcode==decode))
                                                       where a.UID == uidnn
                                                       select new Generalform
                                                       {
                                                           Army_No = a.Army_No,
                                                           ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                           UID = a.UID,
                                                           Citizen_ID = a.Citizen_ID,
                                                           Sanik_Name_eng = a.Sanik_Name_eng,
                                                           Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                           Father_Name_eng = a.Father_Name_eng,
                                                           Father_Name_hindi = a.Father_Name_hindi,
                                                           Mother_Name_eng = a.Mother_Name_eng,
                                                           Mother_Name_hindi = a.Mother_Name_hindi,
                                                           Gender = a.Gender_code,

                                                           DOB = Convert.ToString(a.DOB),
                                                           CategoryDesc = a.CategoryCode,
                                                           MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                           spousename = a.spousename,
                                                           spousenamehindi = a.spousenamehindi,
                                                           mobileno = a.mobileno,
                                                           landline = a.landline,
                                                           emialid = a.emialid,
                                                           Regement_Corps_id = (a.Regement_Corps_id),
                                                           Per_address_eng = a.Per_address_eng,
                                                           Per_address_hindi = a.Per_address_hindi,
                                                           Per_Landmark_english = a.Per_Landmark_english,
                                                           Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                           statecode = a.statecode,
                                                           dcode = a.dcode,
                                                           tcode = a.tcode,
                                                           VCODE = a.VCODE,
                                                           towncode = a.towncode,
                                                           Urban_rural = Convert.ToString(a.Urban_rural),
                                                           perchk = a.perchk,
                                                           Per_cors = (a.Per_cors).ToString(),
                                                           Pin_code = a.Pin_code,
                                                           Cors_address = a.Cors_address,
                                                           Amount_OF_Rent = a.Amount_OF_Rent,
                                                           Annual_income = a.Annual_income,
                                                           Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                           //changes -04-03-2018//
                                                           Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                           Disable1 = a.Disable,
                                                           Disability_Percentage = a.Disability_Percentage,
                                                           PPO_number = a.PPO_number,
                                                           Pancard_number = a.Pancard_number,
                                                           //end//
                                                           Character_Id = (o.Character_Id),
                                                           medical_ID = (o.medical_ID),
                                                           Rank_ID = o.Rank_ID,
                                                           RetirementDate1 = o.RetirementDate,

                                                           Force_Cat_ID = o.Force_Cat_ID,

                                                           Force_Dept_Id = o.Force_Dept_Id,

                                                           Bank_Acc_no = a.Bank_Acc_no,
                                                           Bank_IFSC = a.Bank_IFSC,
                                                           BankID = a.BankID,
                                                       };

                                            if (list.Count() > 0)
                                            {
                                                foreach (var item in list)
                                                {
                                                    Generalform gn = new Generalform();
                                                    var imgg = from r in dta.rsbgenerals where r.UID == uidnn select r;
                                                    gn.Army_No = item.Army_No;
                                                    gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                    gn.UID = item.UID;
                                                    gn.Citizen_ID = item.Citizen_ID;
                                                    gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                    gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                    gn.Father_Name_eng = item.Father_Name_eng;
                                                    gn.Father_Name_hindi = item.Father_Name_hindi;
                                                    gn.Mother_Name_eng = item.Mother_Name_eng;
                                                    gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                    gn.Gender = item.Gender;

                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                    gn.CategoryDesc = item.CategoryDesc;
                                                    gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                    gn.spousename = item.spousename;
                                                    gn.spousenamehindi = item.spousenamehindi;
                                                    gn.mobileno = item.mobileno;
                                                    gn.landline = item.landline;
                                                    gn.emialid = item.emialid;
                                                    gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                    gn.Per_address_eng = item.Per_address_eng;
                                                    gn.Per_address_hindi = item.Per_address_hindi;
                                                    gn.Per_Landmark_english = item.Per_Landmark_english;
                                                    gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                    gn.statecode = item.statecode;
                                                    gn.dcode = item.dcode;
                                                    gn.tcode = item.tcode;
                                                    gn.VCODE = item.VCODE;
                                                    gn.towncode = item.towncode;
                                                    gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                    gn.perchk = item.perchk;
                                                    if (item.Per_cors != null)
                                                    {
                                                        gn.Per_cors = (item.Per_cors).ToString();
                                                    }
                                                    else
                                                    {
                                                        gn.Per_cors = "";
                                                    }
                                                    gn.Pin_code = item.Pin_code;
                                                    gn.Cors_address = item.Cors_address;
                                                    gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                    gn.Annual_income = item.Annual_income;
                                                    gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                    //changes -04-03-2018//
                                                    gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                    gn.Disable1 = item.Disable1;
                                                    gn.Disability_Percentage = item.Disability_Percentage;
                                                    gn.PPO_number = item.PPO_number;
                                                    gn.Pancard_number = item.Pancard_number;
                                                    //end//
                                                    if (imgg.FirstOrDefault().photo != null)
                                                    {
                                                        byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                        gn.ImgByte = Convert.ToBase64String(array);



                                                    }
                                                    else
                                                    {


                                                        gn.ImgByte = null;
                                                    }



                                                    gn.Character_Id = (item.Character_Id);
                                                    gn.medical_ID = (item.medical_ID);
                                                    gn.Rank_ID = item.Rank_ID;
                                                    gn.RetirementDate1 = item.RetirementDate1;

                                                    gn.Force_Cat_ID = item.Force_Cat_ID;

                                                    gn.Force_Dept_Id = item.Force_Dept_Id;

                                                    gn.Bank_Acc_no = item.Bank_Acc_no;
                                                    gn.Bank_IFSC = item.Bank_IFSC;
                                                    gn.BankID = item.BankID;
                                                    Session["aarmyno"] = item.Army_No;
                                                    cd1.Add(gn);
                                                }
                                            }






                                        }
                                        else
                                        {
                                            var list = from a in dta.rsbgenerals
                                                       //where a.UID == uidnn ||((a.UID==uidnn &&a.dcode==decode))
                                                       where a.UID == uidnn
                                                       select new Generalform
                                                       {
                                                           Army_No = a.Army_No,
                                                           ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                           UID = a.UID,
                                                           Citizen_ID = a.Citizen_ID,
                                                           Sanik_Name_eng = a.Sanik_Name_eng,
                                                           Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                           Father_Name_eng = a.Father_Name_eng,
                                                           Father_Name_hindi = a.Father_Name_hindi,
                                                           Mother_Name_eng = a.Mother_Name_eng,
                                                           Mother_Name_hindi = a.Mother_Name_hindi,
                                                           Gender = a.Gender_code,

                                                           DOB = Convert.ToString(a.DOB),
                                                           CategoryDesc = a.CategoryCode,
                                                           MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                           spousename = a.spousename,
                                                           spousenamehindi = a.spousenamehindi,
                                                           mobileno = a.mobileno,
                                                           landline = a.landline,
                                                           emialid = a.emialid,
                                                           Regement_Corps_id = (a.Regement_Corps_id),
                                                           Per_address_eng = a.Per_address_eng,
                                                           Per_address_hindi = a.Per_address_hindi,
                                                           Per_Landmark_english = a.Per_Landmark_english,
                                                           Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                           statecode = a.statecode,
                                                           dcode = a.dcode,
                                                           tcode = a.tcode,
                                                           VCODE = a.VCODE,
                                                           towncode = a.towncode,
                                                           Urban_rural = Convert.ToString(a.Urban_rural),
                                                           perchk = a.perchk,
                                                           Per_cors = (a.Per_cors).ToString(),
                                                           Pin_code = a.Pin_code,
                                                           Cors_address = a.Cors_address,
                                                           Amount_OF_Rent = a.Amount_OF_Rent,
                                                           Annual_income = a.Annual_income,
                                                           Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                           //changes -04-03-2018//
                                                           Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                           Disable1 = a.Disable,
                                                           Disability_Percentage = a.Disability_Percentage,
                                                           PPO_number = a.PPO_number,
                                                           Pancard_number = a.Pancard_number,
                                                           //end//
                                                           Bank_Acc_no = a.Bank_Acc_no,
                                                           Bank_IFSC = a.Bank_IFSC,
                                                           BankID = a.BankID,
                                                       };

                                            if (list.Count() > 0)
                                            {
                                                foreach (var item in list)
                                                {
                                                    Generalform gn = new Generalform();
                                                    var imgg = from r in dta.rsbgenerals where r.UID == uidnn select r;
                                                    gn.Army_No = item.Army_No;
                                                    gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                    gn.UID = item.UID;
                                                    gn.Citizen_ID = item.Citizen_ID;
                                                    gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                    gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                    gn.Father_Name_eng = item.Father_Name_eng;
                                                    gn.Father_Name_hindi = item.Father_Name_hindi;
                                                    gn.Mother_Name_eng = item.Mother_Name_eng;
                                                    gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                    gn.Gender = item.Gender;

                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                    gn.CategoryDesc = item.CategoryDesc;
                                                    gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                    gn.spousename = item.spousename;
                                                    gn.spousenamehindi = item.spousenamehindi;
                                                    gn.mobileno = item.mobileno;
                                                    gn.landline = item.landline;
                                                    gn.emialid = item.emialid;
                                                    gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                    gn.Per_address_eng = item.Per_address_eng;
                                                    gn.Per_address_hindi = item.Per_address_hindi;
                                                    gn.Per_Landmark_english = item.Per_Landmark_english;
                                                    gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                    gn.statecode = item.statecode;
                                                    gn.dcode = item.dcode;
                                                    gn.tcode = item.tcode;
                                                    gn.VCODE = item.VCODE;
                                                    gn.towncode = item.towncode;
                                                    gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                    gn.perchk = item.perchk;
                                                    if (item.Per_cors != null)
                                                    {
                                                        gn.Per_cors = (item.Per_cors).ToString();
                                                    }
                                                    else
                                                    {
                                                        gn.Per_cors = "";
                                                    }
                                                    gn.Pin_code = item.Pin_code;
                                                    gn.Cors_address = item.Cors_address;
                                                    gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                    gn.Annual_income = item.Annual_income;
                                                    gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                    //changes -04-03-2018//
                                                    gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                    gn.Disable1 = item.Disable1;
                                                    gn.Disability_Percentage = item.Disability_Percentage;
                                                    gn.PPO_number = item.PPO_number;
                                                    gn.Pancard_number = item.Pancard_number;
                                                    //end//
                                                    if (imgg.FirstOrDefault().photo != null)
                                                    {
                                                        byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                        gn.ImgByte = Convert.ToBase64String(array);



                                                    }
                                                    else
                                                    {


                                                        gn.ImgByte = null;
                                                    }

                                                    gn.Bank_Acc_no = item.Bank_Acc_no;
                                                    gn.Bank_IFSC = item.Bank_IFSC;
                                                    gn.BankID = item.BankID;
                                                    Session["aarmyno"] = item.Army_No;
                                                    cd1.Add(gn);
                                                }
                                            }
                                        }


                                        if (cd1.Count() == 0)
                                        {


                                            var list = from c in ed.tblCIDRs
                                                       where c.UID == uidnn
                                                       //where c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn||(((c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn)&& c.dcode==decode ))
                                                       select c;

                                            if (list.Count() > 0)
                                            {

                                                foreach (var item in list)
                                                {
                                                    Generalform gn = new Generalform();
                                                    gn.UID = item.UID;
                                                    gn.Citizen_ID = item.Citizen_ID;
                                                    gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                    gn.Father_Name_eng = item.Father_Name_EN;
                                                    gn.Mother_Name_eng = item.Mother_Name_EN;
                                                    gn.Gender = Convert.ToString(item.Gender);
                                                    if (item.DOB != null)
                                                    {
                                                        gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                    }
                                                    else
                                                    {
                                                        string datee = "01/01/0001";
                                                        gn.DOBnew = Convert.ToDateTime(datee);
                                                    }

                                                    gn.CategoryDesc = item.Caste_Category;
                                                    gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                    gn.mobileno = item.Mobile;
                                                    gn.emialid = item.Email_id;
                                                    gn.Per_address_eng = item.House_Name_No;
                                                    gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                    gn.statecode = item.State;
                                                    gn.dcode = item.District_Code;
                                                    gn.tcode = item.Block_Tehsil_Code;
                                                    gn.VCODE = item.Village_Town_Code;
                                                    gn.towncode = item.Village_Town_Code;
                                                    gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                    gn.Pin_code = item.PIN;
                                                    gn.Cors_address = item.Correspondence_Address_EN;
                                                    if (item.Photo != null)
                                                    {

                                                        gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                    }
                                                    else
                                                    {


                                                        gn.ImgByte = null;
                                                    }
                                                    cd.Add(gn);


                                                }
                                                return Json(cd, JsonRequestBehavior.AllowGet);
                                            }



                                        }


                                        if (cd1.Count() > 0)
                                        {

                                            cd = cd1;
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }
                                        return Json(JsonRequestBehavior.AllowGet);
                                    }
                                }

                            }
                        }
                        else if (search == "cid")
                        {
                            string cid1 = uidnn.Substring(0, 2);
                            int cidlength = uidnn.Length;
                            cid = Citizen(uidnn);


                            if (cid == false || (cid1 != "HA" && cidlength != 12))
                            {
                                string msg = "cidd";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {

                                bool check = dta.rsbgenerals.Any(tbl => tbl.Citizen_ID == uidnn);
                                if (check == true)
                                {
                                    string output = "already exist";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    Session["citizensearch"] = uidnn;
                                    List<Generalform> cd1 = new List<Generalform>();
                                    var rsb = from r in dta.rsbgenerals where r.Citizen_ID == uidnn select r;
                                    if (rsb.Count() == 0)
                                    {
                                        string output = "Not Found";
                                        return Json(new { output }, JsonRequestBehavior.AllowGet);

                                    }

                                    else
                                    {
                                        string armyno = rsb.FirstOrDefault().Army_No;
                                        var othr = from o in dta.sanik_otherinformations where o.Army_No == armyno select o;
                                        //changes
                                        if (othr.Count() > 0)
                                        {

                                            var list = from a in dta.rsbgenerals
                                                       join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                       //where a.Citizen_ID == uidnn || ((a.Citizen_ID == uidnn && a.dcode==decode ))
                                                       where a.Citizen_ID == uidnn
                                                       select new Generalform
                                                       {
                                                           Army_No = a.Army_No,
                                                           ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                           UID = a.UID,
                                                           Citizen_ID = a.Citizen_ID,
                                                           Sanik_Name_eng = a.Sanik_Name_eng,
                                                           Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                           Father_Name_eng = a.Father_Name_eng,
                                                           Father_Name_hindi = a.Father_Name_hindi,
                                                           Mother_Name_eng = a.Mother_Name_eng,
                                                           Mother_Name_hindi = a.Mother_Name_hindi,
                                                           Gender = a.Gender_code,

                                                           DOB = Convert.ToString(a.DOB),
                                                           CategoryDesc = a.CategoryCode,
                                                           MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                           spousename = a.spousename,
                                                           spousenamehindi = a.spousenamehindi,
                                                           mobileno = a.mobileno,
                                                           landline = a.landline,
                                                           emialid = a.emialid,
                                                           Regement_Corps_id = (a.Regement_Corps_id),
                                                           Per_address_eng = a.Per_address_eng,
                                                           Per_address_hindi = a.Per_address_hindi,
                                                           Per_Landmark_english = a.Per_Landmark_english,
                                                           Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                           statecode = a.statecode,
                                                           dcode = a.dcode,
                                                           tcode = a.tcode,
                                                           VCODE = a.VCODE,
                                                           towncode = a.towncode,
                                                           Urban_rural = Convert.ToString(a.Urban_rural),
                                                           perchk = a.perchk,
                                                           Per_cors = (a.Per_cors).ToString(),
                                                           Pin_code = a.Pin_code,
                                                           Cors_address = a.Cors_address,
                                                           Amount_OF_Rent = a.Amount_OF_Rent,
                                                           Annual_income = a.Annual_income,
                                                           Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                           //changes -04-03-2018//
                                                           Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                           Disable1 = a.Disable,
                                                           Disability_Percentage = a.Disability_Percentage,
                                                           PPO_number = a.PPO_number,
                                                           Pancard_number = a.Pancard_number,
                                                           //end//
                                                           Character_Id = (o.Character_Id),
                                                           medical_ID = (o.medical_ID),
                                                           Rank_ID = o.Rank_ID,
                                                           RetirementDate1 = o.RetirementDate,

                                                           Force_Cat_ID = o.Force_Cat_ID,

                                                           Force_Dept_Id = o.Force_Dept_Id,

                                                           Bank_Acc_no = a.Bank_Acc_no,
                                                           Bank_IFSC = a.Bank_IFSC,
                                                           BankID = a.BankID,
                                                       };

                                            if (list.Count() > 0)
                                            {
                                                foreach (var item in list)
                                                {
                                                    Generalform gn = new Generalform();
                                                    var imgg = from r in dta.rsbgenerals where r.Citizen_ID == uidnn select r;
                                                    gn.Army_No = item.Army_No;
                                                    gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                    gn.UID = item.UID;
                                                    gn.Citizen_ID = item.Citizen_ID;
                                                    gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                    gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                    gn.Father_Name_eng = item.Father_Name_eng;
                                                    gn.Father_Name_hindi = item.Father_Name_hindi;
                                                    gn.Mother_Name_eng = item.Mother_Name_eng;
                                                    gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                    gn.Gender = item.Gender;

                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                    gn.CategoryDesc = item.CategoryCode;
                                                    gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                    gn.spousename = item.spousename;
                                                    gn.spousenamehindi = item.spousenamehindi;
                                                    gn.mobileno = item.mobileno;
                                                    gn.landline = item.landline;
                                                    gn.emialid = item.emialid;
                                                    gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                    gn.Per_address_eng = item.Per_address_eng;
                                                    gn.Per_address_hindi = item.Per_address_hindi;
                                                    gn.Per_Landmark_english = item.Per_Landmark_english;
                                                    gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                    gn.statecode = item.statecode;
                                                    gn.dcode = item.dcode;
                                                    gn.tcode = item.tcode;
                                                    gn.VCODE = item.VCODE;
                                                    gn.towncode = item.towncode;
                                                    gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                    gn.perchk = item.perchk;
                                                    if (item.Per_cors != null)
                                                    {
                                                        gn.Per_cors = (item.Per_cors).ToString();
                                                    }
                                                    else
                                                    {
                                                        gn.Per_cors = "";
                                                    }
                                                    gn.Pin_code = item.Pin_code;
                                                    gn.Cors_address = item.Cors_address;
                                                    gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                    gn.Annual_income = item.Annual_income;
                                                    gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                    //changes -04-03-2018//
                                                    gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                    gn.Disable1 = item.Disable1;
                                                    gn.Disability_Percentage = item.Disability_Percentage;
                                                    gn.PPO_number = item.PPO_number;
                                                    gn.Pancard_number = item.Pancard_number;
                                                    //end//
                                                    if (imgg.FirstOrDefault().photo != null)
                                                    {
                                                        byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                        gn.ImgByte = Convert.ToBase64String(array);



                                                    }
                                                    else
                                                    {


                                                        gn.ImgByte = null;
                                                    }



                                                    gn.Character_Id = (item.Character_Id);
                                                    gn.medical_ID = (item.medical_ID);
                                                    gn.Rank_ID = item.Rank_ID;
                                                    gn.RetirementDate1 = item.RetirementDate1;

                                                    gn.Force_Cat_ID = item.Force_Cat_ID;

                                                    gn.Force_Dept_Id = item.Force_Dept_Id;

                                                    gn.Bank_Acc_no = item.Bank_Acc_no;
                                                    gn.Bank_IFSC = item.Bank_IFSC;
                                                    gn.BankID = item.BankID;
                                                    Session["aarmyno"] = item.Army_No;
                                                    cd1.Add(gn);
                                                }
                                            }


                                        }
                                        else
                                        {
                                            var list = from a in dta.rsbgenerals
                                                       //where a.Citizen_ID == uidnn || ((a.Citizen_ID == uidnn && a.dcode==decode ))
                                                       where a.Citizen_ID == uidnn
                                                       select new Generalform
                                                       {
                                                           Army_No = a.Army_No,
                                                           ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                           UID = a.UID,
                                                           Citizen_ID = a.Citizen_ID,
                                                           Sanik_Name_eng = a.Sanik_Name_eng,
                                                           Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                           Father_Name_eng = a.Father_Name_eng,
                                                           Father_Name_hindi = a.Father_Name_hindi,
                                                           Mother_Name_eng = a.Mother_Name_eng,
                                                           Mother_Name_hindi = a.Mother_Name_hindi,
                                                           Gender = a.Gender_code,

                                                           DOB = Convert.ToString(a.DOB),
                                                           CategoryDesc = a.CategoryCode,
                                                           MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                           spousename = a.spousename,
                                                           spousenamehindi = a.spousenamehindi,
                                                           mobileno = a.mobileno,
                                                           landline = a.landline,
                                                           emialid = a.emialid,
                                                           Regement_Corps_id = (a.Regement_Corps_id),
                                                           Per_address_eng = a.Per_address_eng,
                                                           Per_address_hindi = a.Per_address_hindi,
                                                           Per_Landmark_english = a.Per_Landmark_english,
                                                           Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                           statecode = a.statecode,
                                                           dcode = a.dcode,
                                                           tcode = a.tcode,
                                                           VCODE = a.VCODE,
                                                           towncode = a.towncode,
                                                           Urban_rural = Convert.ToString(a.Urban_rural),
                                                           perchk = a.perchk,
                                                           Per_cors = (a.Per_cors).ToString(),
                                                           Pin_code = a.Pin_code,
                                                           Cors_address = a.Cors_address,
                                                           Amount_OF_Rent = a.Amount_OF_Rent,
                                                           Annual_income = a.Annual_income,
                                                           Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                           //changes -04-03-2018//
                                                           Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                           Disable1 = a.Disable,
                                                           Disability_Percentage = a.Disability_Percentage,
                                                           PPO_number = a.PPO_number,
                                                           Pancard_number = a.Pancard_number,
                                                           //end//
                                                           Bank_Acc_no = a.Bank_Acc_no,
                                                           Bank_IFSC = a.Bank_IFSC,
                                                           BankID = a.BankID,
                                                       };

                                            if (list.Count() > 0)
                                            {
                                                foreach (var item in list)
                                                {
                                                    Generalform gn = new Generalform();
                                                    var imgg = from r in dta.rsbgenerals where r.Citizen_ID == uidnn select r;
                                                    gn.Army_No = item.Army_No;
                                                    gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                    gn.UID = item.UID;
                                                    gn.Citizen_ID = item.Citizen_ID;
                                                    gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                    gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                    gn.Father_Name_eng = item.Father_Name_eng;
                                                    gn.Father_Name_hindi = item.Father_Name_hindi;
                                                    gn.Mother_Name_eng = item.Mother_Name_eng;
                                                    gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                    gn.Gender = item.Gender;

                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                    gn.CategoryDesc = item.CategoryDesc;
                                                    gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                    gn.spousename = item.spousename;
                                                    gn.spousenamehindi = item.spousenamehindi;
                                                    gn.mobileno = item.mobileno;
                                                    gn.landline = item.landline;
                                                    gn.emialid = item.emialid;
                                                    gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                    gn.Per_address_eng = item.Per_address_eng;
                                                    gn.Per_address_hindi = item.Per_address_hindi;
                                                    gn.Per_Landmark_english = item.Per_Landmark_english;
                                                    gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                    gn.statecode = item.statecode;
                                                    gn.dcode = item.dcode;
                                                    gn.tcode = item.tcode;
                                                    gn.VCODE = item.VCODE;
                                                    gn.towncode = item.towncode;
                                                    gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                    gn.perchk = item.perchk;
                                                    if (item.Per_cors != null)
                                                    {
                                                        gn.Per_cors = (item.Per_cors).ToString();
                                                    }
                                                    else
                                                    {
                                                        gn.Per_cors = "";
                                                    }
                                                    gn.Pin_code = item.Pin_code;
                                                    gn.Cors_address = item.Cors_address;
                                                    gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                    gn.Annual_income = item.Annual_income;
                                                    gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                    //changes -04-03-2018//
                                                    gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                    gn.Disable1 = item.Disable1;
                                                    gn.Disability_Percentage = item.Disability_Percentage;
                                                    gn.PPO_number = item.PPO_number;
                                                    gn.Pancard_number = item.Pancard_number;
                                                    //end//
                                                    if (imgg.FirstOrDefault().photo != null)
                                                    {
                                                        byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                        gn.ImgByte = Convert.ToBase64String(array);



                                                    }
                                                    else
                                                    {


                                                        gn.ImgByte = null;
                                                    }

                                                    gn.Bank_Acc_no = item.Bank_Acc_no;
                                                    gn.Bank_IFSC = item.Bank_IFSC;
                                                    gn.BankID = item.BankID;
                                                    Session["aarmyno"] = item.Army_No;
                                                    cd1.Add(gn);
                                                }
                                            }
                                        }


                                        if (cd1.Count() == 0)
                                        {


                                            var list = from c in ed.tblCIDRs
                                                       where c.Citizen_ID == uidnn
                                                       //where c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn || (((c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn) && c.dcode==decode ))
                                                       select c;

                                            if (list.Count() > 0)
                                            {

                                                foreach (var item in list)
                                                {
                                                    Generalform gn = new Generalform();
                                                    gn.UID = item.UID;
                                                    gn.Citizen_ID = item.Citizen_ID;
                                                    gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                    gn.Father_Name_eng = item.Father_Name_EN;
                                                    gn.Mother_Name_eng = item.Mother_Name_EN;
                                                    gn.Gender = Convert.ToString(item.Gender);
                                                    if (item.DOB != null)
                                                    {
                                                        gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                    }
                                                    else
                                                    {
                                                        string datee = "01/01/0001";
                                                        gn.DOBnew = Convert.ToDateTime(datee);
                                                    }

                                                    gn.CategoryDesc = item.Caste_Category;
                                                    gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                    gn.mobileno = item.Mobile;
                                                    gn.emialid = item.Email_id;
                                                    gn.Per_address_eng = item.House_Name_No;
                                                    gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                    gn.statecode = item.State;
                                                    gn.dcode = item.District_Code;
                                                    gn.tcode = item.Block_Tehsil_Code;
                                                    gn.VCODE = item.Village_Town_Code;
                                                    gn.towncode = item.Village_Town_Code;
                                                    gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                    gn.Pin_code = item.PIN;
                                                    gn.Cors_address = item.Correspondence_Address_EN;
                                                    if (item.Photo != null)
                                                    {

                                                        gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                    }
                                                    else
                                                    {


                                                        gn.ImgByte = null;
                                                    }
                                                    cd.Add(gn);


                                                }
                                                return Json(cd, JsonRequestBehavior.AllowGet);
                                            }



                                        }


                                        if (cd1.Count() > 0)
                                        {

                                            cd = cd1;
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }
                                        return Json(JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }
                        else
                        {
                            int mob = (uidnn.Trim()).Length;
                            mobileno = IsNumber(uidnn);

                            if (mobileno == false || mob != 10)
                            {
                                string msg = "mobilenoo";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                List<Generalform> cd1 = new List<Generalform>();
                                var rsb = from r in dta.rsbgenerals where r.mobileno == uidnn select r.Army_No;
                                if (rsb.Count() == 0)
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault() select o;
                                    //changes
                                    if (othr.Count() > 0)
                                    {

                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   //where a.mobileno == uidnn || ((a.mobileno == uidnn && a.dcode==decode ))
                                                   where a.mobileno == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,

                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.mobileno == uidnn && r.Army_No == item.Army_No select r;
                                                gn.Army_No = item.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                var img = imgg.FirstOrDefault().photo;
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var list = (from a in dta.rsbgenerals
                                                    //where a.mobileno == uidnn || ((a.mobileno == uidnn && a.dcode==decode ))
                                                    where a.mobileno == uidnn
                                                    select new Generalform
                                                    {
                                                        Army_No = a.Army_No,
                                                        ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                        UID = a.UID,
                                                        Citizen_ID = a.Citizen_ID,
                                                        Sanik_Name_eng = a.Sanik_Name_eng,
                                                        Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                        Father_Name_eng = a.Father_Name_eng,
                                                        Father_Name_hindi = a.Father_Name_hindi,
                                                        Mother_Name_eng = a.Mother_Name_eng,
                                                        Mother_Name_hindi = a.Mother_Name_hindi,
                                                        Gender = a.Gender_code,

                                                        DOB = Convert.ToString(a.DOB),
                                                        CategoryDesc = a.CategoryCode,
                                                        MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                        spousename = a.spousename,
                                                        spousenamehindi = a.spousenamehindi,
                                                        mobileno = a.mobileno,
                                                        landline = a.landline,
                                                        emialid = a.emialid,
                                                        Regement_Corps_id = (a.Regement_Corps_id),
                                                        Per_address_eng = a.Per_address_eng,
                                                        Per_address_hindi = a.Per_address_hindi,
                                                        Per_Landmark_english = a.Per_Landmark_english,
                                                        Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                        statecode = a.statecode,
                                                        dcode = a.dcode,
                                                        tcode = a.tcode,
                                                        VCODE = a.VCODE,
                                                        towncode = a.towncode,
                                                        Urban_rural = Convert.ToString(a.Urban_rural),
                                                        perchk = a.perchk,
                                                        Per_cors = (a.Per_cors).ToString(),
                                                        Pin_code = a.Pin_code,
                                                        Cors_address = a.Cors_address,
                                                        Amount_OF_Rent = a.Amount_OF_Rent,
                                                        Annual_income = a.Annual_income,
                                                        Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                        //changes -04-03-2018//
                                                        Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                        Disable1 = a.Disable,
                                                        Disability_Percentage = a.Disability_Percentage,
                                                        PPO_number = a.PPO_number,
                                                        Pancard_number = a.Pancard_number,
                                                        //end//
                                                        Bank_Acc_no = a.Bank_Acc_no,
                                                        Bank_IFSC = a.Bank_IFSC,
                                                        BankID = a.BankID,
                                                    }).ToList();

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.mobileno == uidnn && r.Army_No == item.Army_No select r;

                                                //  var imgg1 = list.Concat(imgg).ToList();
                                                gn.Army_No = item.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;

                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                //ok
                                                //var img = imgg.FirstOrDefault().photo;
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }
                                    }


                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   where c.Mobile == uidnn
                                                   //where c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn|| (((c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn) && c.dcode==decode ))
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;
                                                gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;
                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }
                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }


                        }


                    }
                    else
                    {

                        return Json(cd, JsonRequestBehavior.AllowGet);

                    }





                }




                catch (Exception ex)
                {
                    string exmsg = ex.Message;
                    return Json(new { exmsg }, JsonRequestBehavior.AllowGet);

                }
            }

            else if (usertype == "72" || (usertype == "70"))
            { //next
                try
                {
                    Generalform obj = new Generalform();
                    List<Generalform> cd = new List<Generalform>();

                    int chk = 0;
                    //bool check = dta.rsbgenerals.Any(tbl => tbl.Army_No == uidnn || tbl.ESMIdentitycardnumber == uidnn || tbl.Citizen_ID == uidnn || tbl.UID == uidnn);
                    //if (check == true)
                    //{
                    //    string output = "already exist";
                    //    return Json(new { output }, JsonRequestBehavior.AllowGet);
                    //}


                    if (search == "armyno")
                    {
                        bool armyno;
                        armyno = armynumber(uidnn);
                        // int army = uidnn.Length;
                        if (armyno == false)
                        {
                            string msg = "armynoo";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            bool check = dta.rsbgenerals.Any(tbl => tbl.Army_No == uidnn);
                            if (check == true)
                            {
                                string output = "already exist";
                                return Json(new { output }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {

                                var othr = from t in dta.sanik_otherinformations where t.Army_No == uidnn select t;
                                if (othr.Count() > 0)
                                {
                                    var list = from a in dta.rsbgenerals
                                               join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                               where ((a.Army_No == uidnn && a.dcode == decode))
                                               // where a.Army_No == uidnn
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,

                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Character_Id = (o.Character_Id),
                                                   medical_ID = (o.medical_ID),
                                                   Rank_ID = o.Rank_ID,
                                                   RetirementDate1 = o.RetirementDate,

                                                   Force_Cat_ID = o.Force_Cat_ID,

                                                   Force_Dept_Id = o.Force_Dept_Id,

                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                                   status = Convert.ToString(a.Status),
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.Army_No == uidnn && r.dcode == decode select r;
                                            gn.Army_No = item.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;

                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.Per_cors != null)
                                            {
                                                gn.Per_cors = (item.Per_cors).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }



                                            gn.Character_Id = (item.Character_Id);
                                            gn.medical_ID = (item.medical_ID);
                                            gn.Rank_ID = item.Rank_ID;
                                            gn.RetirementDate1 = item.RetirementDate1;

                                            gn.Force_Cat_ID = item.Force_Cat_ID;

                                            gn.Force_Dept_Id = item.Force_Dept_Id;

                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            gn.status = item.status;
                                            Session["aarmyno"] = item.Army_No;
                                            cd.Add(gn);
                                        }
                                    }


                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var list = from a in dta.rsbgenerals
                                               where ((a.Army_No == uidnn && a.dcode == decode))
                                               // where a.Army_No == uidnn
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,
                                                   status = Convert.ToString(a.Status),
                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.Army_No == uidnn && r.dcode == decode select r;
                                            gn.Army_No = item.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;
                                            gn.status = item.status;
                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.Per_cors != null)
                                            {
                                                gn.Per_cors = (item.Per_cors).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }

                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            Session["aarmyno"] = item.Army_No;
                                            cd.Add(gn);
                                        }
                                    }
                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }


                            }
                        }


                    }


                    else if (search == "esmino")
                    {
                        bool esmino;
                        esmino = Citizen(uidnn);
                        //int esmlength=uidnn.Length;
                        if (esmino == false)
                        {
                            string msg = "esmino";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            bool check = dta.rsbgenerals.Any(tbl => tbl.ESMIdentitycardnumber == uidnn);
                            if (check == true)
                            {
                                string output = "already exist";
                                return Json(new { output }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var rsb = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn && r.dcode == decode select r;
                                if (rsb.Count() > 0)
                                {
                                    var othr = from t in dta.sanik_otherinformations where t.Army_No == rsb.FirstOrDefault().Army_No select t;

                                    if (othr.Count() > 0)
                                    {
                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   where ((a.Army_No == uidnn && a.dcode == decode))
                                                   //where a.ESMIdentitycardnumber == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn && r.dcode == decode select r;
                                                gn.Army_No = item.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd.Add(gn);
                                            }
                                        }


                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals
                                                   where ((a.Army_No == uidnn && a.dcode == decode))
                                                   //where a.ESMIdentitycardnumber == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn && r.dcode == decode select r;
                                                gn.Army_No = item.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }


                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd.Add(gn);
                                            }
                                        }
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                    }

                    else if (search == "uid" || search == "cid" || search == "mobileno")
                    {
                        bool uid, cid, mobileno;
                        if (search == "uid")
                        {
                            int length = uidnn.Length;

                            uid = IsNumber(uidnn);
                            if (uid == false || length != 12)
                            {
                                string msg = "uidd";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                bool check = dta.rsbgenerals.Any(tbl => tbl.UID == uidnn);
                                if (check == true)
                                {
                                    string output = "already exist";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    List<Generalform> cd1 = new List<Generalform>();
                                    var rsb = from r in dta.rsbgenerals where r.UID == uidnn && r.dcode == decode select r.Army_No;
                                    var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault() select o;
                                    //changes
                                    if (othr.Count() > 0)
                                    {

                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   where ((a.UID == uidnn && a.dcode == decode))
                                                   // where a.UID == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.UID == uidnn && r.dcode == decode select r;
                                                gn.Army_No = item.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }






                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals
                                                   where ((a.UID == uidnn && a.dcode == decode))
                                                   //where a.UID == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.UID == uidnn && r.dcode == decode select r;
                                                gn.Army_No = item.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }
                                    }


                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   //where c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn
                                                   where (((c.UID == uidnn) && c.District_Code == decode))
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;
                                                gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;
                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }
                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        else if (search == "cid")
                        {
                            string cid1 = uidnn.Substring(0, 2);
                            int cidlngth = uidnn.Length;
                            cid = Citizen(uidnn);

                            if (cid == false || (cid1 != "HA" && cidlngth != 12))
                            {
                                string msg = "cidd";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                bool check = dta.rsbgenerals.Any(tbl => tbl.Citizen_ID == uidnn);
                                if (check == true)
                                {
                                    string output = "already exist";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    Session["citizensearch"] = uidnn;
                                    List<Generalform> cd1 = new List<Generalform>();
                                    var rsb = from r in dta.rsbgenerals where r.Citizen_ID == uidnn && r.dcode == decode select r;
                                    if (rsb.Count() > 0)
                                    {


                                        string armyno = rsb.FirstOrDefault().Army_No;
                                        var othr = from o in dta.sanik_otherinformations where o.Army_No == armyno select o;
                                        //changes
                                        if (othr.Count() > 0)
                                        {

                                            var list = from a in dta.rsbgenerals
                                                       join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                       where ((a.Citizen_ID == uidnn && a.dcode == decode))
                                                       //where a.Citizen_ID == uidnn
                                                       select new Generalform
                                                       {
                                                           Army_No = a.Army_No,
                                                           ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                           UID = a.UID,
                                                           Citizen_ID = a.Citizen_ID,
                                                           Sanik_Name_eng = a.Sanik_Name_eng,
                                                           Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                           Father_Name_eng = a.Father_Name_eng,
                                                           Father_Name_hindi = a.Father_Name_hindi,
                                                           Mother_Name_eng = a.Mother_Name_eng,
                                                           Mother_Name_hindi = a.Mother_Name_hindi,
                                                           Gender = a.Gender_code,
                                                           status = Convert.ToString(a.Status),
                                                           DOB = Convert.ToString(a.DOB),
                                                           CategoryDesc = a.CategoryCode,
                                                           MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                           spousename = a.spousename,
                                                           spousenamehindi = a.spousenamehindi,
                                                           mobileno = a.mobileno,
                                                           landline = a.landline,
                                                           emialid = a.emialid,
                                                           Regement_Corps_id = (a.Regement_Corps_id),
                                                           Per_address_eng = a.Per_address_eng,
                                                           Per_address_hindi = a.Per_address_hindi,
                                                           Per_Landmark_english = a.Per_Landmark_english,
                                                           Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                           statecode = a.statecode,
                                                           dcode = a.dcode,
                                                           tcode = a.tcode,
                                                           VCODE = a.VCODE,
                                                           towncode = a.towncode,
                                                           Urban_rural = Convert.ToString(a.Urban_rural),
                                                           perchk = a.perchk,
                                                           Per_cors = (a.Per_cors).ToString(),
                                                           Pin_code = a.Pin_code,
                                                           Cors_address = a.Cors_address,
                                                           Amount_OF_Rent = a.Amount_OF_Rent,
                                                           Annual_income = a.Annual_income,
                                                           Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                           //changes -04-03-2018//
                                                           Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                           Disable1 = a.Disable,
                                                           Disability_Percentage = a.Disability_Percentage,
                                                           PPO_number = a.PPO_number,
                                                           Pancard_number = a.Pancard_number,
                                                           //end//
                                                           Character_Id = (o.Character_Id),
                                                           medical_ID = (o.medical_ID),
                                                           Rank_ID = o.Rank_ID,
                                                           RetirementDate1 = o.RetirementDate,

                                                           Force_Cat_ID = o.Force_Cat_ID,

                                                           Force_Dept_Id = o.Force_Dept_Id,

                                                           Bank_Acc_no = a.Bank_Acc_no,
                                                           Bank_IFSC = a.Bank_IFSC,
                                                           BankID = a.BankID,
                                                       };

                                            if (list.Count() > 0)
                                            {
                                                foreach (var item in list)
                                                {
                                                    Generalform gn = new Generalform();
                                                    var imgg = from r in dta.rsbgenerals where r.Citizen_ID == uidnn && r.dcode == decode select r;
                                                    gn.Army_No = item.Army_No;
                                                    gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                    gn.UID = item.UID;
                                                    gn.Citizen_ID = item.Citizen_ID;
                                                    gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                    gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                    gn.Father_Name_eng = item.Father_Name_eng;
                                                    gn.Father_Name_hindi = item.Father_Name_hindi;
                                                    gn.Mother_Name_eng = item.Mother_Name_eng;
                                                    gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                    gn.Gender = item.Gender;
                                                    gn.status = item.status;
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                    gn.CategoryDesc = item.CategoryCode;
                                                    gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                    gn.spousename = item.spousename;
                                                    gn.spousenamehindi = item.spousenamehindi;
                                                    gn.mobileno = item.mobileno;
                                                    gn.landline = item.landline;
                                                    gn.emialid = item.emialid;
                                                    gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                    gn.Per_address_eng = item.Per_address_eng;
                                                    gn.Per_address_hindi = item.Per_address_hindi;
                                                    gn.Per_Landmark_english = item.Per_Landmark_english;
                                                    gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                    gn.statecode = item.statecode;
                                                    gn.dcode = item.dcode;
                                                    gn.tcode = item.tcode;
                                                    gn.VCODE = item.VCODE;
                                                    gn.towncode = item.towncode;
                                                    gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                    gn.perchk = item.perchk;
                                                    if (item.Per_cors != null)
                                                    {
                                                        gn.Per_cors = (item.Per_cors).ToString();
                                                    }
                                                    else
                                                    {
                                                        gn.Per_cors = "";
                                                    }
                                                    gn.Pin_code = item.Pin_code;
                                                    gn.Cors_address = item.Cors_address;
                                                    gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                    gn.Annual_income = item.Annual_income;
                                                    gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                    //changes -04-03-2018//
                                                    gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                    gn.Disable1 = item.Disable1;
                                                    gn.Disability_Percentage = item.Disability_Percentage;
                                                    gn.PPO_number = item.PPO_number;
                                                    gn.Pancard_number = item.Pancard_number;
                                                    //end//
                                                    if (imgg.FirstOrDefault().photo != null)
                                                    {
                                                        byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                        gn.ImgByte = Convert.ToBase64String(array);



                                                    }
                                                    else
                                                    {


                                                        gn.ImgByte = null;
                                                    }



                                                    gn.Character_Id = (item.Character_Id);
                                                    gn.medical_ID = (item.medical_ID);
                                                    gn.Rank_ID = item.Rank_ID;
                                                    gn.RetirementDate1 = item.RetirementDate1;

                                                    gn.Force_Cat_ID = item.Force_Cat_ID;

                                                    gn.Force_Dept_Id = item.Force_Dept_Id;

                                                    gn.Bank_Acc_no = item.Bank_Acc_no;
                                                    gn.Bank_IFSC = item.Bank_IFSC;
                                                    gn.BankID = item.BankID;
                                                    Session["aarmyno"] = item.Army_No;
                                                    cd1.Add(gn);
                                                }
                                            }


                                        }
                                        else
                                        {
                                            var list = from a in dta.rsbgenerals
                                                       where ((a.Citizen_ID == uidnn && a.dcode == decode))
                                                       //where a.Citizen_ID == uidnn
                                                       select new Generalform
                                                       {
                                                           Army_No = a.Army_No,
                                                           ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                           UID = a.UID,
                                                           Citizen_ID = a.Citizen_ID,
                                                           Sanik_Name_eng = a.Sanik_Name_eng,
                                                           Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                           Father_Name_eng = a.Father_Name_eng,
                                                           Father_Name_hindi = a.Father_Name_hindi,
                                                           Mother_Name_eng = a.Mother_Name_eng,
                                                           Mother_Name_hindi = a.Mother_Name_hindi,
                                                           Gender = a.Gender_code,
                                                           status = Convert.ToString(a.Status),
                                                           DOB = Convert.ToString(a.DOB),
                                                           CategoryDesc = a.CategoryCode,
                                                           MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                           spousename = a.spousename,
                                                           spousenamehindi = a.spousenamehindi,
                                                           mobileno = a.mobileno,
                                                           landline = a.landline,
                                                           emialid = a.emialid,
                                                           Regement_Corps_id = (a.Regement_Corps_id),
                                                           Per_address_eng = a.Per_address_eng,
                                                           Per_address_hindi = a.Per_address_hindi,
                                                           Per_Landmark_english = a.Per_Landmark_english,
                                                           Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                           statecode = a.statecode,
                                                           dcode = a.dcode,
                                                           tcode = a.tcode,
                                                           VCODE = a.VCODE,
                                                           towncode = a.towncode,
                                                           Urban_rural = Convert.ToString(a.Urban_rural),
                                                           perchk = a.perchk,
                                                           Per_cors = (a.Per_cors).ToString(),
                                                           Pin_code = a.Pin_code,
                                                           Cors_address = a.Cors_address,
                                                           Amount_OF_Rent = a.Amount_OF_Rent,
                                                           Annual_income = a.Annual_income,
                                                           Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                           //changes -04-03-2018//
                                                           Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                           Disable1 = a.Disable,
                                                           Disability_Percentage = a.Disability_Percentage,
                                                           PPO_number = a.PPO_number,
                                                           Pancard_number = a.Pancard_number,
                                                           //end//
                                                           Bank_Acc_no = a.Bank_Acc_no,
                                                           Bank_IFSC = a.Bank_IFSC,
                                                           BankID = a.BankID,
                                                       };

                                            if (list.Count() > 0)
                                            {
                                                foreach (var item in list)
                                                {
                                                    Generalform gn = new Generalform();
                                                    var imgg = from r in dta.rsbgenerals where r.Citizen_ID == uidnn && r.dcode == decode select r;
                                                    gn.Army_No = item.Army_No;
                                                    gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                    gn.UID = item.UID;
                                                    gn.Citizen_ID = item.Citizen_ID;
                                                    gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                    gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                    gn.Father_Name_eng = item.Father_Name_eng;
                                                    gn.Father_Name_hindi = item.Father_Name_hindi;
                                                    gn.Mother_Name_eng = item.Mother_Name_eng;
                                                    gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                    gn.Gender = item.Gender;
                                                    gn.status = item.status;
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                    gn.CategoryDesc = item.CategoryDesc;
                                                    gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                    gn.spousename = item.spousename;
                                                    gn.spousenamehindi = item.spousenamehindi;
                                                    gn.mobileno = item.mobileno;
                                                    gn.landline = item.landline;
                                                    gn.emialid = item.emialid;
                                                    gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                    gn.Per_address_eng = item.Per_address_eng;
                                                    gn.Per_address_hindi = item.Per_address_hindi;
                                                    gn.Per_Landmark_english = item.Per_Landmark_english;
                                                    gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                    gn.statecode = item.statecode;
                                                    gn.dcode = item.dcode;
                                                    gn.tcode = item.tcode;
                                                    gn.VCODE = item.VCODE;
                                                    gn.towncode = item.towncode;
                                                    gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                    gn.perchk = item.perchk;
                                                    if (item.Per_cors != null)
                                                    {
                                                        gn.Per_cors = (item.Per_cors).ToString();
                                                    }
                                                    else
                                                    {
                                                        gn.Per_cors = "";
                                                    }
                                                    gn.Pin_code = item.Pin_code;
                                                    gn.Cors_address = item.Cors_address;
                                                    gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                    gn.Annual_income = item.Annual_income;
                                                    gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                    //changes -04-03-2018//
                                                    gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                    gn.Disable1 = item.Disable1;
                                                    gn.Disability_Percentage = item.Disability_Percentage;
                                                    gn.PPO_number = item.PPO_number;
                                                    gn.Pancard_number = item.Pancard_number;
                                                    //end//
                                                    if (imgg.FirstOrDefault().photo != null)
                                                    {
                                                        byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                        gn.ImgByte = Convert.ToBase64String(array);



                                                    }
                                                    else
                                                    {


                                                        gn.ImgByte = null;
                                                    }

                                                    gn.Bank_Acc_no = item.Bank_Acc_no;
                                                    gn.Bank_IFSC = item.Bank_IFSC;
                                                    gn.BankID = item.BankID;
                                                    Session["aarmyno"] = item.Army_No;
                                                    cd1.Add(gn);
                                                }
                                            }
                                        }
                                    }

                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   // where c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn
                                                   where (((c.Citizen_ID == uidnn) && c.District_Code == decode))
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;
                                                gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;
                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }
                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                        else
                        {
                            int mob = uidnn.Length;

                            mobileno = IsNumber(uidnn);

                            if (mobileno == false || mob != 10)
                            {
                                string msg = "mobilenoo";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                List<Generalform> cd1 = new List<Generalform>();
                                var rsb = from r in dta.rsbgenerals where r.mobileno == uidnn && r.dcode == decode select r.Army_No;
                                var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault() select o;
                                //changes
                                if (othr.Count() > 0)
                                {

                                    var list = from a in dta.rsbgenerals
                                               join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                               where ((a.mobileno == uidnn && a.dcode == decode))
                                               //where a.mobileno == uidnn
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,
                                                   status = Convert.ToString(a.Status),
                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Character_Id = (o.Character_Id),
                                                   medical_ID = (o.medical_ID),
                                                   Rank_ID = o.Rank_ID,
                                                   RetirementDate1 = o.RetirementDate,

                                                   Force_Cat_ID = o.Force_Cat_ID,

                                                   Force_Dept_Id = o.Force_Dept_Id,

                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.mobileno == uidnn && r.dcode == decode && r.Army_No == item.Army_No select r;
                                            gn.Army_No = item.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;
                                            gn.status = item.status;
                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.Per_cors != null)
                                            {
                                                gn.Per_cors = (item.Per_cors).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }



                                            gn.Character_Id = (item.Character_Id);
                                            gn.medical_ID = (item.medical_ID);
                                            gn.Rank_ID = item.Rank_ID;
                                            gn.RetirementDate1 = item.RetirementDate1;

                                            gn.Force_Cat_ID = item.Force_Cat_ID;

                                            gn.Force_Dept_Id = item.Force_Dept_Id;

                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            cd1.Add(gn);
                                        }
                                    }

                                }
                                else
                                {
                                    var list = from a in dta.rsbgenerals
                                               where ((a.mobileno == uidnn && a.dcode == decode))
                                               // where a.mobileno == uidnn
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,
                                                   status = Convert.ToString(a.Status),
                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.mobileno == uidnn && r.dcode == decode && r.Army_No == item.Army_No select r;
                                            gn.Army_No = item.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;
                                            gn.status = item.status;
                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.Per_cors != null)
                                            {
                                                gn.Per_cors = (item.Per_cors).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }

                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            Session["aarmyno"] = item.Army_No;
                                            cd1.Add(gn);
                                        }
                                    }
                                }


                                if (cd1.Count() == 0)
                                {


                                    var list = from c in ed.tblCIDRs
                                               // where c.Citizen_ID == uidnn || c.UID == uidnn || c.Mobile == uidnn
                                               where (((c.Mobile == uidnn && c.District_Code == decode)))
                                               select c;

                                    if (list.Count() > 0)
                                    {

                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                            gn.Father_Name_eng = item.Father_Name_EN;
                                            gn.Mother_Name_eng = item.Mother_Name_EN;
                                            gn.Gender = Convert.ToString(item.Gender);
                                            if (item.DOB != null)
                                            {
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);

                                            }
                                            else
                                            {
                                                string datee = "01/01/0001";
                                                gn.DOBnew = Convert.ToDateTime(datee);
                                            }

                                            gn.CategoryDesc = item.Caste_Category;
                                            gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                            gn.mobileno = item.Mobile;
                                            gn.emialid = item.Email_id;
                                            gn.Per_address_eng = item.House_Name_No;
                                            gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                            gn.statecode = item.State;
                                            gn.dcode = item.District_Code;
                                            gn.tcode = item.Block_Tehsil_Code;
                                            gn.VCODE = item.Village_Town_Code;
                                            gn.towncode = item.Village_Town_Code;
                                            gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                            gn.Pin_code = item.PIN;
                                            gn.Cors_address = item.Correspondence_Address_EN;
                                            if (item.Photo != null)
                                            {

                                                gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }
                                            cd.Add(gn);


                                        }
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }



                                }


                                if (cd1.Count() > 0)
                                {

                                    cd = cd1;
                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }
                                return Json(JsonRequestBehavior.AllowGet);
                            }



                        }


                    }
                    else
                    {

                        return Json(cd, JsonRequestBehavior.AllowGet);

                    }





                }




                catch (Exception ex)
                {
                    throw ex;


                }

            }
            else
            {
                string msg = "not";
                return Json(new { msg }, JsonRequestBehavior.AllowGet);

            }


        }
        //get already exist getuid
        public JsonResult searchgetuid(string uidnn, string search)
        {
            string usertype = Convert.ToString(Session["usertype"]);
            string decode = Convert.ToString(Session["dcode"]);
            if (usertype == "71")
            {
                try
                {
                    Generalform obj = new Generalform();
                    List<Generalform> cd = new List<Generalform>();


                    if (search == "armyno")
                    {

                        bool armyno;
                        armyno = armynumber(uidnn);
                        // int army = uidnn.Length;
                        if (armyno == false)
                        {
                            string msg = "armynoo";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Session["armynoalrd"] = uidnn;

                            var othr = from t in dta.sanik_otherinformations where t.Army_No == uidnn select t;
                            if (othr.Count() > 0)
                            {
                                var list = from a in dta.rsbgenerals
                                           join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                           where a.Army_No == uidnn
                                           select new Generalform
                                           {
                                               Army_No = a.Army_No,
                                               ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                               UID = a.UID,
                                               Citizen_ID = a.Citizen_ID,
                                               Sanik_Name_eng = a.Sanik_Name_eng,
                                               Sanik_Name_hindi = a.Sanik_Name_hindi,
                                               Father_Name_eng = a.Father_Name_eng,
                                               Father_Name_hindi = a.Father_Name_hindi,
                                               Mother_Name_eng = a.Mother_Name_eng,
                                               Mother_Name_hindi = a.Mother_Name_hindi,
                                               Gender = a.Gender_code,
                                               status = Convert.ToString(a.Status),
                                               DOB = Convert.ToString(a.DOB),
                                               CategoryDesc = a.CategoryCode,
                                               MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                               spousename = a.spousename,
                                               spousenamehindi = a.spousenamehindi,
                                               mobileno = a.mobileno,
                                               landline = a.landline,
                                               emialid = a.emialid,
                                               Regement_Corps_id = (a.Regement_Corps_id),
                                               Per_address_eng = a.Per_address_eng,
                                               Per_address_hindi = a.Per_address_hindi,
                                               Per_Landmark_english = a.Per_Landmark_english,
                                               Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                               statecode = a.statecode,
                                               dcode = a.dcode,
                                               tcode = a.tcode,
                                               VCODE = a.VCODE,
                                               towncode = a.towncode,
                                               Urban_rural = Convert.ToString(a.Urban_rural),
                                               perchk = a.perchk,
                                               Per_cors = (a.Per_cors).ToString(),
                                               Pin_code = a.Pin_code,
                                               Cors_address = a.Cors_address,
                                               Amount_OF_Rent = a.Amount_OF_Rent,
                                               Annual_income = a.Annual_income,
                                               Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                               //changes -04-03-2018//
                                               Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                               Disable1 = a.Disable,
                                               Disability_Percentage = a.Disability_Percentage,
                                               PPO_number = a.PPO_number,
                                               Pancard_number = a.Pancard_number,
                                               //end//
                                               Character_Id = (o.Character_Id),
                                               medical_ID = (o.medical_ID),
                                               Rank_ID = o.Rank_ID,
                                               RetirementDate1 = o.RetirementDate,

                                               Force_Cat_ID = o.Force_Cat_ID,

                                               Force_Dept_Id = o.Force_Dept_Id,

                                               Bank_Acc_no = a.Bank_Acc_no,
                                               Bank_IFSC = a.Bank_IFSC,
                                               BankID = a.BankID,
                                           };

                                if (list.Count() > 0)
                                {
                                    foreach (var item in list)
                                    {
                                        Generalform gn = new Generalform();
                                        var imgg = from r in dta.rsbgenerals where r.Army_No == uidnn select r;
                                        gn.Army_No = item.Army_No;
                                        gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                        gn.UID = item.UID;
                                        gn.Citizen_ID = item.Citizen_ID;
                                        gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                        gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                        gn.Father_Name_eng = item.Father_Name_eng;
                                        gn.Father_Name_hindi = item.Father_Name_hindi;
                                        gn.Mother_Name_eng = item.Mother_Name_eng;
                                        gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                        gn.Gender = item.Gender;
                                        gn.status = item.status;
                                        gn.DOBnew = Convert.ToDateTime(item.DOB);
                                        gn.CategoryDesc = item.CategoryDesc;
                                        gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                        gn.spousename = item.spousename;
                                        gn.spousenamehindi = item.spousenamehindi;
                                        gn.mobileno = item.mobileno;
                                        gn.landline = item.landline;
                                        gn.emialid = item.emialid;
                                        gn.Regement_Corps_id = (item.Regement_Corps_id);
                                        gn.Per_address_eng = item.Per_address_eng;
                                        gn.Per_address_hindi = item.Per_address_hindi;
                                        gn.Per_Landmark_english = item.Per_Landmark_english;
                                        gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                        gn.statecode = item.statecode;
                                        gn.dcode = item.dcode;
                                        gn.tcode = item.tcode;
                                        gn.VCODE = item.VCODE;
                                        gn.towncode = item.towncode;
                                        gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                        gn.perchk = item.perchk;
                                        if (item.Per_cors != null)
                                        {
                                            gn.Per_cors = (item.Per_cors).ToString();
                                        }
                                        else
                                        {

                                            gn.Per_cors = "";


                                        }

                                        gn.Pin_code = item.Pin_code;
                                        gn.Cors_address = item.Cors_address;
                                        gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                        gn.Annual_income = item.Annual_income;
                                        gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                        //changes -04-03-2018//
                                        gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);

                                        gn.Disable1 = item.Disable1;
                                        gn.Disability_Percentage = item.Disability_Percentage;
                                        gn.PPO_number = item.PPO_number;
                                        gn.Pancard_number = item.Pancard_number;
                                        //end//
                                        if (imgg.FirstOrDefault().photo != null)
                                        {
                                            byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                            gn.ImgByte = Convert.ToBase64String(array);



                                        }
                                        else
                                        {


                                            gn.ImgByte = null;
                                        }



                                        gn.Character_Id = (item.Character_Id);
                                        gn.medical_ID = (item.medical_ID);
                                        gn.Rank_ID = item.Rank_ID;
                                        gn.RetirementDate1 = item.RetirementDate1;

                                        gn.Force_Cat_ID = item.Force_Cat_ID;

                                        gn.Force_Dept_Id = item.Force_Dept_Id;

                                        gn.Bank_Acc_no = item.Bank_Acc_no;
                                        gn.Bank_IFSC = item.Bank_IFSC;
                                        gn.BankID = item.BankID;
                                        Session["aarmyno"] = item.Army_No;
                                        cd.Add(gn);
                                    }
                                }


                                return Json(cd, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var list = from a in dta.rsbgenerals

                                           where a.Army_No == uidnn
                                           select new Generalform
                                           {
                                               Army_No = a.Army_No,
                                               ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                               UID = a.UID,
                                               Citizen_ID = a.Citizen_ID,
                                               Sanik_Name_eng = a.Sanik_Name_eng,
                                               Sanik_Name_hindi = a.Sanik_Name_hindi,
                                               Father_Name_eng = a.Father_Name_eng,
                                               Father_Name_hindi = a.Father_Name_hindi,
                                               Mother_Name_eng = a.Mother_Name_eng,
                                               Mother_Name_hindi = a.Mother_Name_hindi,
                                               Gender = a.Gender_code,
                                               status = Convert.ToString(a.Status),
                                               DOB = Convert.ToString(a.DOB),
                                               CategoryDesc = a.CategoryCode,
                                               MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                               spousename = a.spousename,
                                               spousenamehindi = a.spousenamehindi,
                                               mobileno = a.mobileno,
                                               landline = a.landline,
                                               emialid = a.emialid,
                                               Regement_Corps_id = (a.Regement_Corps_id),
                                               Per_address_eng = a.Per_address_eng,
                                               Per_address_hindi = a.Per_address_hindi,
                                               Per_Landmark_english = a.Per_Landmark_english,
                                               Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                               statecode = a.statecode,
                                               dcode = a.dcode,
                                               tcode = a.tcode,
                                               VCODE = a.VCODE,
                                               towncode = a.towncode,
                                               Urban_rural = Convert.ToString(a.Urban_rural),
                                               perchk = a.perchk,
                                               Per_cors = (a.Per_cors).ToString(),
                                               Pin_code = a.Pin_code,
                                               Cors_address = a.Cors_address,
                                               Amount_OF_Rent = a.Amount_OF_Rent,
                                               Annual_income = a.Annual_income,
                                               Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                               //changes -04-03-2018//
                                               Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                               Disable1 = a.Disable,
                                               Disability_Percentage = a.Disability_Percentage,
                                               PPO_number = a.PPO_number,
                                               Pancard_number = a.Pancard_number,
                                               //end//
                                               Bank_Acc_no = a.Bank_Acc_no,
                                               Bank_IFSC = a.Bank_IFSC,
                                               BankID = a.BankID,
                                           };

                                if (list.Count() > 0)
                                {
                                    foreach (var item in list)
                                    {
                                        Generalform gn = new Generalform();
                                        var imgg = from r in dta.rsbgenerals where r.Army_No == uidnn select r;
                                        gn.Army_No = item.Army_No;
                                        gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                        gn.UID = item.UID;
                                        gn.Citizen_ID = item.Citizen_ID;
                                        gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                        gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                        gn.Father_Name_eng = item.Father_Name_eng;
                                        gn.Father_Name_hindi = item.Father_Name_hindi;
                                        gn.Mother_Name_eng = item.Mother_Name_eng;
                                        gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                        gn.Gender = item.Gender;
                                        gn.status = item.status;
                                        gn.DOBnew = Convert.ToDateTime(item.DOB);
                                        gn.CategoryDesc = item.CategoryDesc;
                                        gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                        gn.spousename = item.spousename;
                                        gn.spousenamehindi = item.spousenamehindi;
                                        gn.mobileno = item.mobileno;
                                        gn.landline = item.landline;
                                        gn.emialid = item.emialid;
                                        gn.Regement_Corps_id = (item.Regement_Corps_id);
                                        gn.Per_address_eng = item.Per_address_eng;
                                        gn.Per_address_hindi = item.Per_address_hindi;
                                        gn.Per_Landmark_english = item.Per_Landmark_english;
                                        gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                        gn.statecode = item.statecode;
                                        gn.dcode = item.dcode;
                                        gn.tcode = item.tcode;
                                        gn.VCODE = item.VCODE;
                                        gn.towncode = item.towncode;
                                        gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                        gn.perchk = item.perchk;
                                        if (item.Per_cors != null)
                                        {
                                            gn.Per_cors = (item.Per_cors).ToString();
                                        }
                                        else
                                        {
                                            gn.Per_cors = "";
                                        }
                                        gn.Pin_code = item.Pin_code;
                                        gn.Cors_address = item.Cors_address;
                                        gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                        gn.Annual_income = item.Annual_income;
                                        gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                        //changes -04-03-2018//
                                        gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                        gn.Disable1 = item.Disable1;
                                        gn.Disability_Percentage = item.Disability_Percentage;
                                        gn.PPO_number = item.PPO_number;
                                        gn.Pancard_number = item.Pancard_number;
                                        //end//
                                        if (imgg.FirstOrDefault().photo != null)
                                        {
                                            byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                            gn.ImgByte = Convert.ToBase64String(array);



                                        }
                                        else
                                        {


                                            gn.ImgByte = null;
                                        }

                                        gn.Bank_Acc_no = item.Bank_Acc_no;
                                        gn.Bank_IFSC = item.Bank_IFSC;
                                        gn.BankID = item.BankID;
                                        Session["aarmyno"] = item.Army_No;
                                        cd.Add(gn);
                                    }
                                }
                                return Json(cd, JsonRequestBehavior.AllowGet);
                            }


                        }



                    }


                    else if (search == "esmino")
                    {
                        bool esmino;
                        esmino = Citizen(uidnn);
                        //int esm = uidnn.Length;
                        if (esmino == false)
                        {
                            string msg = "esmino";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var rsb = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn select r;
                            if (rsb.Count() == 0)
                            {
                                string output = "Not Found";
                                return Json(new { output }, JsonRequestBehavior.AllowGet);

                            }

                            else
                            {
                                var othr = from t in dta.sanik_otherinformations where t.Army_No == rsb.FirstOrDefault().Army_No select t;
                                if (othr.Count() > 0)
                                {
                                    var list = from a in dta.rsbgenerals
                                               join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                               where a.ESMIdentitycardnumber == uidnn
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,
                                                   status = Convert.ToString(a.Status),
                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Character_Id = (o.Character_Id),
                                                   medical_ID = (o.medical_ID),
                                                   Rank_ID = o.Rank_ID,
                                                   RetirementDate1 = o.RetirementDate,

                                                   Force_Cat_ID = o.Force_Cat_ID,

                                                   Force_Dept_Id = o.Force_Dept_Id,

                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn select r;
                                            gn.Army_No = item.Army_No;
                                            Session["armynoalrd"] = gn.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;
                                            gn.status = item.status;
                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.Per_cors != null)
                                            {
                                                gn.Per_cors = (item.Per_cors).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }



                                            gn.Character_Id = (item.Character_Id);
                                            gn.medical_ID = (item.medical_ID);
                                            gn.Rank_ID = item.Rank_ID;
                                            gn.RetirementDate1 = item.RetirementDate1;

                                            gn.Force_Cat_ID = item.Force_Cat_ID;

                                            gn.Force_Dept_Id = item.Force_Dept_Id;

                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            Session["aarmyno"] = item.Army_No;
                                            cd.Add(gn);
                                        }
                                    }


                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var list = from a in dta.rsbgenerals
                                               where a.ESMIdentitycardnumber == uidnn
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,
                                                   status = Convert.ToString(a.Status),
                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn select r;
                                            gn.Army_No = item.Army_No;
                                            Session["armynoalrd"] = gn.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;
                                            gn.status = item.status;
                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.perchk != null)
                                            {
                                                gn.Per_cors = (item.perchk).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }


                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            Session["aarmyno"] = item.Army_No;
                                            cd.Add(gn);
                                        }
                                    }
                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }


                    }

                    else if (search == "uid" || search == "cid" || search == "mobileno")
                    {
                        bool uid, cid, mobileno;
                        if (search == "uid")
                        {

                            int length = uidnn.Length;
                            uid = IsNumber(uidnn);
                            if (uid == false && length != 12)
                            {
                                string msg = "uidd";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                List<Generalform> cd1 = new List<Generalform>();
                                var rsb = from r in dta.rsbgenerals where r.UID == uidnn select r;
                                if (rsb.Count() == 0)
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault().Army_No select o;
                                    //changes
                                    if (othr.Count() > 0)
                                    {

                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   where a.UID == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.UID == uidnn select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;



                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }






                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals

                                                   where a.UID == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.UID == uidnn select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }
                                    }


                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   where c.UID == uidnn
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;

                                                gn.MaritalStatusCode = Convert.ToString(item.Marital_Status);
                                                gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;
                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }
                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                        else if (search == "cid")
                        {
                            string cid1 = uidnn.Substring(0, 2);
                            int cidlength = uidnn.Length;

                            cid = Citizen(uidnn);
                            if (cid == false && cid1 != "HA" && cidlength != 12)
                            {
                                string msg = "cidd";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                Session["citizensearch"] = uidnn;
                                List<Generalform> cd1 = new List<Generalform>();
                                var rsb = from r in dta.rsbgenerals where r.Citizen_ID == uidnn select r;
                                if (rsb.Count() == 0)
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault().Army_No select o;
                                    //changes
                                    if (othr.Count() > 0)
                                    {

                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   where a.Citizen_ID == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.Citizen_ID == uidnn select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.perchk != null)
                                                {
                                                    gn.Per_cors = (item.perchk).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }






                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals

                                                   where a.Citizen_ID == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.Citizen_ID == uidnn select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }
                                    }


                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   where c.Citizen_ID == uidnn
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;
                                                gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;

                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }
                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                        else
                        {
                            int mob = uidnn.Length;
                            mobileno = IsNumber(uidnn);

                            if (mobileno == false && mob != 10)
                            {
                                string msg = "mobilenoo";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                List<Generalform> cd1 = new List<Generalform>();
                                var rsb = from r in dta.rsbgenerals where r.mobileno == uidnn select r.Army_No;
                                if (rsb.Count() == 0)
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault() select o;
                                    //changes
                                    if (othr.Count() > 0)
                                    {

                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   where a.mobileno == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.mobileno == uidnn && r.Army_No == item.Army_No select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }






                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals

                                                   where a.mobileno == uidnn
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.mobileno == uidnn && r.Army_No == item.Army_No select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }
                                    }


                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   where c.Mobile == uidnn
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;
                                                gn.MaritalStatusDesc = Convert.ToString(item.Marital_Status);

                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;
                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }
                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }



                        }


                    }
                    else
                    {

                        return Json(cd, JsonRequestBehavior.AllowGet);

                    }





                }




                catch (Exception ex)
                {
                    throw ex;


                }
            }
            else if (usertype == "72" || usertype == "70")
            //next already
            {
                try
                {
                    Generalform obj = new Generalform();
                    List<Generalform> cd = new List<Generalform>();


                    if (search == "armyno")
                    {
                        bool armyno;
                        armyno = armynumber(uidnn);
                        //int army= uidnn.Length;
                        if (armyno == false)
                        {
                            string msg = "armynoo";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

                            var othr = from t in dta.sanik_otherinformations where t.Army_No == uidnn select t;
                            if (othr.Count() > 0)
                            {
                                var list = from a in dta.rsbgenerals
                                           join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                           where a.Army_No == uidnn && a.dcode == decode
                                           select new Generalform
                                           {
                                               Army_No = a.Army_No,
                                               ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                               UID = a.UID,
                                               Citizen_ID = a.Citizen_ID,
                                               Sanik_Name_eng = a.Sanik_Name_eng,
                                               Sanik_Name_hindi = a.Sanik_Name_hindi,
                                               Father_Name_eng = a.Father_Name_eng,
                                               Father_Name_hindi = a.Father_Name_hindi,
                                               Mother_Name_eng = a.Mother_Name_eng,
                                               Mother_Name_hindi = a.Mother_Name_hindi,
                                               Gender = a.Gender_code,
                                               status = Convert.ToString(a.Status),
                                               DOB = Convert.ToString(a.DOB),
                                               CategoryDesc = a.CategoryCode,
                                               MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                               spousename = a.spousename,
                                               spousenamehindi = a.spousenamehindi,
                                               mobileno = a.mobileno,
                                               landline = a.landline,
                                               emialid = a.emialid,
                                               Regement_Corps_id = (a.Regement_Corps_id),
                                               Per_address_eng = a.Per_address_eng,
                                               Per_address_hindi = a.Per_address_hindi,
                                               Per_Landmark_english = a.Per_Landmark_english,
                                               Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                               statecode = a.statecode,
                                               dcode = a.dcode,
                                               tcode = a.tcode,
                                               VCODE = a.VCODE,
                                               towncode = a.towncode,
                                               Urban_rural = Convert.ToString(a.Urban_rural),
                                               perchk = a.perchk,
                                               Per_cors = (a.Per_cors).ToString(),
                                               Pin_code = a.Pin_code,
                                               Cors_address = a.Cors_address,
                                               Amount_OF_Rent = a.Amount_OF_Rent,
                                               Annual_income = a.Annual_income,
                                               Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                               //changes -04-03-2018//
                                               Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                               Disable1 = a.Disable,
                                               Disability_Percentage = a.Disability_Percentage,
                                               PPO_number = a.PPO_number,
                                               Pancard_number = a.Pancard_number,
                                               //end//
                                               Character_Id = (o.Character_Id),
                                               medical_ID = (o.medical_ID),
                                               Rank_ID = o.Rank_ID,
                                               RetirementDate1 = o.RetirementDate,

                                               Force_Cat_ID = o.Force_Cat_ID,

                                               Force_Dept_Id = o.Force_Dept_Id,

                                               Bank_Acc_no = a.Bank_Acc_no,
                                               Bank_IFSC = a.Bank_IFSC,
                                               BankID = a.BankID,
                                           };

                                if (list.Count() > 0)
                                {
                                    foreach (var item in list)
                                    {
                                        Generalform gn = new Generalform();
                                        var imgg = from r in dta.rsbgenerals where r.Army_No == uidnn && r.dcode == decode select r;
                                        gn.Army_No = item.Army_No;
                                        Session["armynoalrd"] = gn.Army_No;
                                        gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                        gn.UID = item.UID;
                                        gn.Citizen_ID = item.Citizen_ID;
                                        gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                        gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                        gn.Father_Name_eng = item.Father_Name_eng;
                                        gn.Father_Name_hindi = item.Father_Name_hindi;
                                        gn.Mother_Name_eng = item.Mother_Name_eng;
                                        gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                        gn.Gender = item.Gender;
                                        gn.status = item.status;
                                        gn.DOBnew = Convert.ToDateTime(item.DOB);
                                        gn.CategoryDesc = item.CategoryDesc;
                                        gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                        gn.spousename = item.spousename;
                                        gn.spousenamehindi = item.spousenamehindi;
                                        gn.mobileno = item.mobileno;
                                        gn.landline = item.landline;
                                        gn.emialid = item.emialid;
                                        gn.Regement_Corps_id = (item.Regement_Corps_id);
                                        gn.Per_address_eng = item.Per_address_eng;
                                        gn.Per_address_hindi = item.Per_address_hindi;
                                        gn.Per_Landmark_english = item.Per_Landmark_english;
                                        gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                        gn.statecode = item.statecode;
                                        gn.dcode = item.dcode;
                                        gn.tcode = item.tcode;
                                        gn.VCODE = item.VCODE;
                                        gn.towncode = item.towncode;
                                        gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                        gn.perchk = item.perchk;
                                        if (item.Per_cors != null)
                                        {
                                            gn.Per_cors = (item.Per_cors).ToString();
                                        }
                                        else
                                        {

                                            gn.Per_cors = "";


                                        }

                                        gn.Pin_code = item.Pin_code;
                                        gn.Cors_address = item.Cors_address;
                                        gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                        gn.Annual_income = item.Annual_income;
                                        gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                        //changes -04-03-2018//
                                        gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                        gn.Disable1 = item.Disable1;
                                        gn.Disability_Percentage = item.Disability_Percentage;
                                        gn.PPO_number = item.PPO_number;
                                        gn.Pancard_number = item.Pancard_number;
                                        //end//
                                        if (imgg.FirstOrDefault().photo != null)
                                        {
                                            byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                            gn.ImgByte = Convert.ToBase64String(array);



                                        }
                                        else
                                        {


                                            gn.ImgByte = null;
                                        }



                                        gn.Character_Id = (item.Character_Id);
                                        gn.medical_ID = (item.medical_ID);
                                        gn.Rank_ID = item.Rank_ID;
                                        gn.RetirementDate1 = item.RetirementDate1;

                                        gn.Force_Cat_ID = item.Force_Cat_ID;

                                        gn.Force_Dept_Id = item.Force_Dept_Id;

                                        gn.Bank_Acc_no = item.Bank_Acc_no;
                                        gn.Bank_IFSC = item.Bank_IFSC;
                                        gn.BankID = item.BankID;
                                        Session["aarmyno"] = item.Army_No;
                                        cd.Add(gn);
                                    }
                                }


                                return Json(cd, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var list = from a in dta.rsbgenerals

                                           where a.Army_No == uidnn && a.dcode == decode
                                           select new Generalform
                                           {
                                               Army_No = a.Army_No,
                                               ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                               UID = a.UID,
                                               Citizen_ID = a.Citizen_ID,
                                               Sanik_Name_eng = a.Sanik_Name_eng,
                                               Sanik_Name_hindi = a.Sanik_Name_hindi,
                                               Father_Name_eng = a.Father_Name_eng,
                                               Father_Name_hindi = a.Father_Name_hindi,
                                               Mother_Name_eng = a.Mother_Name_eng,
                                               Mother_Name_hindi = a.Mother_Name_hindi,
                                               Gender = a.Gender_code,
                                               status = Convert.ToString(a.Status),
                                               DOB = Convert.ToString(a.DOB),
                                               CategoryDesc = a.CategoryCode,
                                               MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                               spousename = a.spousename,
                                               spousenamehindi = a.spousenamehindi,
                                               mobileno = a.mobileno,
                                               landline = a.landline,
                                               emialid = a.emialid,
                                               Regement_Corps_id = (a.Regement_Corps_id),
                                               Per_address_eng = a.Per_address_eng,
                                               Per_address_hindi = a.Per_address_hindi,
                                               Per_Landmark_english = a.Per_Landmark_english,
                                               Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                               statecode = a.statecode,
                                               dcode = a.dcode,
                                               tcode = a.tcode,
                                               VCODE = a.VCODE,
                                               towncode = a.towncode,
                                               Urban_rural = Convert.ToString(a.Urban_rural),
                                               perchk = a.perchk,
                                               Per_cors = (a.Per_cors).ToString(),
                                               Pin_code = a.Pin_code,
                                               Cors_address = a.Cors_address,
                                               Amount_OF_Rent = a.Amount_OF_Rent,
                                               Annual_income = a.Annual_income,
                                               Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                               //changes -04-03-2018//
                                               Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                               Disable1 = a.Disable,
                                               Disability_Percentage = a.Disability_Percentage,
                                               PPO_number = a.PPO_number,
                                               Pancard_number = a.Pancard_number,
                                               //end//
                                               Bank_Acc_no = a.Bank_Acc_no,
                                               Bank_IFSC = a.Bank_IFSC,
                                               BankID = a.BankID,
                                           };

                                if (list.Count() > 0)
                                {
                                    foreach (var item in list)
                                    {
                                        Generalform gn = new Generalform();
                                        var imgg = from r in dta.rsbgenerals where r.Army_No == uidnn && r.dcode == decode select r;
                                        gn.Army_No = item.Army_No;
                                        Session["armynoalrd"] = gn.Army_No;
                                        gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                        gn.UID = item.UID;
                                        gn.Citizen_ID = item.Citizen_ID;
                                        gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                        gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                        gn.Father_Name_eng = item.Father_Name_eng;
                                        gn.Father_Name_hindi = item.Father_Name_hindi;
                                        gn.Mother_Name_eng = item.Mother_Name_eng;
                                        gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                        gn.Gender = item.Gender;
                                        gn.status = item.status;
                                        gn.DOBnew = Convert.ToDateTime(item.DOB);
                                        gn.CategoryDesc = item.CategoryDesc;
                                        gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                        gn.spousename = item.spousename;
                                        gn.spousenamehindi = item.spousenamehindi;
                                        gn.mobileno = item.mobileno;
                                        gn.landline = item.landline;
                                        gn.emialid = item.emialid;
                                        gn.Regement_Corps_id = (item.Regement_Corps_id);
                                        gn.Per_address_eng = item.Per_address_eng;
                                        gn.Per_address_hindi = item.Per_address_hindi;
                                        gn.Per_Landmark_english = item.Per_Landmark_english;
                                        gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                        gn.statecode = item.statecode;
                                        gn.dcode = item.dcode;
                                        gn.tcode = item.tcode;
                                        gn.VCODE = item.VCODE;
                                        gn.towncode = item.towncode;
                                        gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                        gn.perchk = item.perchk;
                                        if (item.Per_cors != null)
                                        {
                                            gn.Per_cors = (item.Per_cors).ToString();
                                        }
                                        else
                                        {
                                            gn.Per_cors = "";
                                        }
                                        gn.Pin_code = item.Pin_code;
                                        gn.Cors_address = item.Cors_address;
                                        gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                        gn.Annual_income = item.Annual_income;
                                        gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                        //changes -04-03-2018//
                                        gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                        gn.Disable1 = item.Disable1;
                                        gn.Disability_Percentage = item.Disability_Percentage;
                                        gn.PPO_number = item.PPO_number;
                                        gn.Pancard_number = item.Pancard_number;
                                        //end//
                                        Session["aarmyno"] = item.Army_No;
                                        if (imgg.FirstOrDefault().photo != null)
                                        {
                                            byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                            gn.ImgByte = Convert.ToBase64String(array);



                                        }
                                        else
                                        {


                                            gn.ImgByte = null;
                                        }

                                        gn.Bank_Acc_no = item.Bank_Acc_no;
                                        gn.Bank_IFSC = item.Bank_IFSC;
                                        gn.BankID = item.BankID;
                                        cd.Add(gn);
                                    }
                                }
                                return Json(cd, JsonRequestBehavior.AllowGet);
                            }


                        }



                    }


                    else if (search == "esmino")
                    {
                        bool esmino;
                        esmino = Citizen(uidnn);
                        //int esm = uidnn.Length;
                        if (esmino == false)
                        {
                            string msg = "esmino";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var rsb = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn && r.dcode == decode select r;
                            if (rsb.Count() == 0)
                            {
                                string output = "Not Found";
                                return Json(new { output }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                var othr = from t in dta.sanik_otherinformations where t.Army_No == rsb.FirstOrDefault().Army_No select t;
                                if (othr.Count() > 0)
                                {
                                    var list = from a in dta.rsbgenerals
                                               join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                               where a.ESMIdentitycardnumber == uidnn && a.dcode == decode
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,
                                                   status = Convert.ToString(a.Status),
                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Character_Id = (o.Character_Id),
                                                   medical_ID = (o.medical_ID),
                                                   Rank_ID = o.Rank_ID,
                                                   RetirementDate1 = o.RetirementDate,

                                                   Force_Cat_ID = o.Force_Cat_ID,

                                                   Force_Dept_Id = o.Force_Dept_Id,

                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn && r.dcode == decode select r;
                                            gn.Army_No = item.Army_No;
                                            Session["armynoalrd"] = gn.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;
                                            gn.status = item.status;
                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.Per_cors != null)
                                            {
                                                gn.Per_cors = (item.Per_cors).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }



                                            gn.Character_Id = (item.Character_Id);
                                            gn.medical_ID = (item.medical_ID);
                                            gn.Rank_ID = item.Rank_ID;
                                            gn.RetirementDate1 = item.RetirementDate1;

                                            gn.Force_Cat_ID = item.Force_Cat_ID;

                                            gn.Force_Dept_Id = item.Force_Dept_Id;

                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            Session["aarmyno"] = item.Army_No;
                                            cd.Add(gn);
                                        }
                                    }


                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var list = from a in dta.rsbgenerals
                                               where a.ESMIdentitycardnumber == uidnn && a.dcode == decode
                                               select new Generalform
                                               {
                                                   Army_No = a.Army_No,
                                                   ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                   UID = a.UID,
                                                   Citizen_ID = a.Citizen_ID,
                                                   Sanik_Name_eng = a.Sanik_Name_eng,
                                                   Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                   Father_Name_eng = a.Father_Name_eng,
                                                   Father_Name_hindi = a.Father_Name_hindi,
                                                   Mother_Name_eng = a.Mother_Name_eng,
                                                   Mother_Name_hindi = a.Mother_Name_hindi,
                                                   Gender = a.Gender_code,
                                                   status = Convert.ToString(a.Status),
                                                   DOB = Convert.ToString(a.DOB),
                                                   CategoryDesc = a.CategoryCode,
                                                   MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                   spousename = a.spousename,
                                                   spousenamehindi = a.spousenamehindi,
                                                   mobileno = a.mobileno,
                                                   landline = a.landline,
                                                   emialid = a.emialid,
                                                   Regement_Corps_id = (a.Regement_Corps_id),
                                                   Per_address_eng = a.Per_address_eng,
                                                   Per_address_hindi = a.Per_address_hindi,
                                                   Per_Landmark_english = a.Per_Landmark_english,
                                                   Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                   statecode = a.statecode,
                                                   dcode = a.dcode,
                                                   tcode = a.tcode,
                                                   VCODE = a.VCODE,
                                                   towncode = a.towncode,
                                                   Urban_rural = Convert.ToString(a.Urban_rural),
                                                   perchk = a.perchk,
                                                   Per_cors = (a.Per_cors).ToString(),
                                                   Pin_code = a.Pin_code,
                                                   Cors_address = a.Cors_address,
                                                   Amount_OF_Rent = a.Amount_OF_Rent,
                                                   Annual_income = a.Annual_income,
                                                   Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                   //changes -04-03-2018//
                                                   Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                   Disable1 = a.Disable,
                                                   Disability_Percentage = a.Disability_Percentage,
                                                   PPO_number = a.PPO_number,
                                                   Pancard_number = a.Pancard_number,
                                                   //end//
                                                   Bank_Acc_no = a.Bank_Acc_no,
                                                   Bank_IFSC = a.Bank_IFSC,
                                                   BankID = a.BankID,
                                               };

                                    if (list.Count() > 0)
                                    {
                                        foreach (var item in list)
                                        {
                                            Generalform gn = new Generalform();
                                            var imgg = from r in dta.rsbgenerals where r.ESMIdentitycardnumber == uidnn && r.dcode == decode select r;
                                            gn.Army_No = item.Army_No;
                                            Session["armynoalrd"] = gn.Army_No;
                                            gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                            gn.UID = item.UID;
                                            gn.Citizen_ID = item.Citizen_ID;
                                            gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                            gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                            gn.Father_Name_eng = item.Father_Name_eng;
                                            gn.Father_Name_hindi = item.Father_Name_hindi;
                                            gn.Mother_Name_eng = item.Mother_Name_eng;
                                            gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                            gn.Gender = item.Gender;
                                            gn.status = item.status;
                                            gn.DOBnew = Convert.ToDateTime(item.DOB);
                                            gn.CategoryDesc = item.CategoryDesc;
                                            gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                            gn.spousename = item.spousename;
                                            gn.spousenamehindi = item.spousenamehindi;
                                            gn.mobileno = item.mobileno;
                                            gn.landline = item.landline;
                                            gn.emialid = item.emialid;
                                            gn.Regement_Corps_id = (item.Regement_Corps_id);
                                            gn.Per_address_eng = item.Per_address_eng;
                                            gn.Per_address_hindi = item.Per_address_hindi;
                                            gn.Per_Landmark_english = item.Per_Landmark_english;
                                            gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                            gn.statecode = item.statecode;
                                            gn.dcode = item.dcode;
                                            gn.tcode = item.tcode;
                                            gn.VCODE = item.VCODE;
                                            gn.towncode = item.towncode;
                                            gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                            gn.perchk = item.perchk;
                                            if (item.perchk != null)
                                            {
                                                gn.Per_cors = (item.perchk).ToString();
                                            }
                                            else
                                            {
                                                gn.Per_cors = "";
                                            }
                                            gn.Pin_code = item.Pin_code;
                                            gn.Cors_address = item.Cors_address;
                                            gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                            gn.Annual_income = item.Annual_income;
                                            gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                            //changes -04-03-2018//
                                            gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                            gn.Disable1 = item.Disable1;
                                            gn.Disability_Percentage = item.Disability_Percentage;
                                            gn.PPO_number = item.PPO_number;
                                            gn.Pancard_number = item.Pancard_number;
                                            //end//
                                            Session["aarmyno"] = item.Army_No;
                                            if (imgg.FirstOrDefault().photo != null)
                                            {
                                                byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                gn.ImgByte = Convert.ToBase64String(array);



                                            }
                                            else
                                            {


                                                gn.ImgByte = null;
                                            }


                                            gn.Bank_Acc_no = item.Bank_Acc_no;
                                            gn.Bank_IFSC = item.Bank_IFSC;
                                            gn.BankID = item.BankID;
                                            cd.Add(gn);
                                        }
                                    }
                                    return Json(cd, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }


                    }

                    else if (search == "uid" || search == "cid" || search == "mobileno")
                    {
                        bool uid, cid, mobileno;
                        if (search == "uid")
                        {

                            int length = uidnn.Length;
                            uid = IsNumber(uidnn);
                            if (uid == false && length != 12)
                            {
                                string msg = "uidd";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                List<Generalform> cd1 = new List<Generalform>();
                                var rsb = from r in dta.rsbgenerals where r.UID == uidnn && r.dcode == decode select r;
                                if (rsb.Count() == 0)
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault().Army_No select o;
                                    //changes
                                    if (othr.Count() > 0)
                                    {

                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   where a.UID == uidnn && a.dcode == decode
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.UID == uidnn && r.dcode == decode select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }






                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals

                                                   where a.UID == uidnn && a.dcode == decode
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.UID == uidnn && r.dcode == decode select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }
                                    }


                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   where (c.UID == uidnn) && c.District_Code == decode
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;
                                                gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;
                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                        else if (search == "cid")
                        {
                            string cid1 = uidnn.Substring(0, 2);
                            cid = Citizen(uidnn);
                            int cidlngth = uidnn.Length;
                            if (cid == false && cid1 != "HA" && cidlngth != 12)
                            {
                                string msg = "cidd";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                Session["citizensearch"] = uidnn;
                                List<Generalform> cd1 = new List<Generalform>();
                                var rsb = from r in dta.rsbgenerals where r.Citizen_ID == uidnn && r.dcode == decode select r;
                                if (rsb.Count() == 0)
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault().Army_No select o;
                                    //changes
                                    if (othr.Count() > 0)
                                    {

                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   where a.Citizen_ID == uidnn && a.dcode == decode
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.Citizen_ID == uidnn && r.dcode == decode select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.perchk != null)
                                                {
                                                    gn.Per_cors = (item.perchk).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }






                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals

                                                   where a.Citizen_ID == uidnn && a.dcode == decode
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.Citizen_ID == uidnn && r.dcode == decode select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }
                                    }


                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   where (c.Citizen_ID == uidnn) && c.District_Code == decode
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;
                                                gn.MaritalStatusDesc = (item.Marital_Status).ToString();

                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;
                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                        else
                        {
                            int mob = uidnn.Length;
                            mobileno = IsNumber(uidnn);

                            if (mobileno == false && mob != 10)
                            {
                                string msg = "mobilenoo";
                                return Json(new { msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                List<Generalform> cd1 = new List<Generalform>();
                                var rsb = from r in dta.rsbgenerals where r.mobileno == uidnn && r.dcode == decode select r.Army_No;
                                if (rsb.Count() == 0)
                                {
                                    string output = "Not Found";
                                    return Json(new { output }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var othr = from o in dta.sanik_otherinformations where o.Army_No == rsb.FirstOrDefault() select o;
                                    //changes
                                    if (othr.Count() > 0)
                                    {

                                        var list = from a in dta.rsbgenerals
                                                   join o in dta.sanik_otherinformations on a.Army_No equals o.Army_No
                                                   where a.mobileno == uidnn && a.dcode == decode
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Character_Id = (o.Character_Id),
                                                       medical_ID = (o.medical_ID),
                                                       Rank_ID = o.Rank_ID,
                                                       RetirementDate1 = o.RetirementDate,

                                                       Force_Cat_ID = o.Force_Cat_ID,

                                                       Force_Dept_Id = o.Force_Dept_Id,

                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.mobileno == uidnn && r.dcode == decode && r.Army_No == item.Army_No select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }



                                                gn.Character_Id = (item.Character_Id);
                                                gn.medical_ID = (item.medical_ID);
                                                gn.Rank_ID = item.Rank_ID;
                                                gn.RetirementDate1 = item.RetirementDate1;

                                                gn.Force_Cat_ID = item.Force_Cat_ID;

                                                gn.Force_Dept_Id = item.Force_Dept_Id;

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }






                                    }
                                    else
                                    {
                                        var list = from a in dta.rsbgenerals

                                                   where a.mobileno == uidnn && a.dcode == decode
                                                   select new Generalform
                                                   {
                                                       Army_No = a.Army_No,
                                                       ESMIdentitycardnumber = a.ESMIdentitycardnumber,
                                                       UID = a.UID,
                                                       Citizen_ID = a.Citizen_ID,
                                                       Sanik_Name_eng = a.Sanik_Name_eng,
                                                       Sanik_Name_hindi = a.Sanik_Name_hindi,
                                                       Father_Name_eng = a.Father_Name_eng,
                                                       Father_Name_hindi = a.Father_Name_hindi,
                                                       Mother_Name_eng = a.Mother_Name_eng,
                                                       Mother_Name_hindi = a.Mother_Name_hindi,
                                                       Gender = a.Gender_code,
                                                       status = Convert.ToString(a.Status),
                                                       DOB = Convert.ToString(a.DOB),
                                                       CategoryDesc = a.CategoryCode,
                                                       MaritalStatusDesc = (a.MaritalStatusCode).ToString(),
                                                       spousename = a.spousename,
                                                       spousenamehindi = a.spousenamehindi,
                                                       mobileno = a.mobileno,
                                                       landline = a.landline,
                                                       emialid = a.emialid,
                                                       Regement_Corps_id = (a.Regement_Corps_id),
                                                       Per_address_eng = a.Per_address_eng,
                                                       Per_address_hindi = a.Per_address_hindi,
                                                       Per_Landmark_english = a.Per_Landmark_english,
                                                       Per_Landmark_Hindi = a.Per_Landmark_Hindi,
                                                       statecode = a.statecode,
                                                       dcode = a.dcode,
                                                       tcode = a.tcode,
                                                       VCODE = a.VCODE,
                                                       towncode = a.towncode,
                                                       Urban_rural = Convert.ToString(a.Urban_rural),
                                                       perchk = a.perchk,
                                                       Per_cors = (a.Per_cors).ToString(),
                                                       Pin_code = a.Pin_code,
                                                       Cors_address = a.Cors_address,
                                                       Amount_OF_Rent = a.Amount_OF_Rent,
                                                       Annual_income = a.Annual_income,
                                                       Annual_Budget_for_Maintenance = a.Annual_Budget_for_Maintenance,
                                                       //changes -04-03-2018//
                                                       Date_of_Enrolment = Convert.ToString(a.Date_of_Enrolment),
                                                       Disable1 = a.Disable,
                                                       Disability_Percentage = a.Disability_Percentage,
                                                       PPO_number = a.PPO_number,
                                                       Pancard_number = a.Pancard_number,
                                                       //end//
                                                       Bank_Acc_no = a.Bank_Acc_no,
                                                       Bank_IFSC = a.Bank_IFSC,
                                                       BankID = a.BankID,
                                                   };

                                        if (list.Count() > 0)
                                        {
                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                var imgg = from r in dta.rsbgenerals where r.mobileno == uidnn && r.dcode == decode && r.Army_No == item.Army_No select r;
                                                gn.Army_No = item.Army_No;
                                                Session["armynoalrd"] = gn.Army_No;
                                                gn.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Sanik_Name_eng;
                                                gn.Sanik_Name_hindi = item.Sanik_Name_hindi;
                                                gn.Father_Name_eng = item.Father_Name_eng;
                                                gn.Father_Name_hindi = item.Father_Name_hindi;
                                                gn.Mother_Name_eng = item.Mother_Name_eng;
                                                gn.Mother_Name_hindi = item.Mother_Name_hindi;
                                                gn.Gender = item.Gender;
                                                gn.status = item.status;
                                                gn.DOBnew = Convert.ToDateTime(item.DOB);
                                                gn.CategoryDesc = item.CategoryDesc;
                                                gn.MaritalStatusDesc = (item.MaritalStatusDesc).ToString();
                                                gn.spousename = item.spousename;
                                                gn.spousenamehindi = item.spousenamehindi;
                                                gn.mobileno = item.mobileno;
                                                gn.landline = item.landline;
                                                gn.emialid = item.emialid;
                                                gn.Regement_Corps_id = (item.Regement_Corps_id);
                                                gn.Per_address_eng = item.Per_address_eng;
                                                gn.Per_address_hindi = item.Per_address_hindi;
                                                gn.Per_Landmark_english = item.Per_Landmark_english;
                                                gn.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                                                gn.statecode = item.statecode;
                                                gn.dcode = item.dcode;
                                                gn.tcode = item.tcode;
                                                gn.VCODE = item.VCODE;
                                                gn.towncode = item.towncode;
                                                gn.Urban_rural = Convert.ToString(item.Urban_rural);
                                                gn.perchk = item.perchk;
                                                if (item.Per_cors != null)
                                                {
                                                    gn.Per_cors = (item.Per_cors).ToString();
                                                }
                                                else
                                                {
                                                    gn.Per_cors = "";
                                                }
                                                gn.Pin_code = item.Pin_code;
                                                gn.Cors_address = item.Cors_address;
                                                gn.Amount_OF_Rent = item.Amount_OF_Rent;
                                                gn.Annual_income = item.Annual_income;
                                                gn.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                                                //changes -04-03-2018//
                                                gn.Date_of_Enrolment1 = Convert.ToDateTime(item.Date_of_Enrolment);
                                                gn.Disable1 = item.Disable1;
                                                gn.Disability_Percentage = item.Disability_Percentage;
                                                gn.PPO_number = item.PPO_number;
                                                gn.Pancard_number = item.Pancard_number;
                                                //end//
                                                if (imgg.FirstOrDefault().photo != null)
                                                {
                                                    byte[] array = (imgg.FirstOrDefault().photo).ToArray();

                                                    gn.ImgByte = Convert.ToBase64String(array);



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }

                                                gn.Bank_Acc_no = item.Bank_Acc_no;
                                                gn.Bank_IFSC = item.Bank_IFSC;
                                                gn.BankID = item.BankID;
                                                Session["aarmyno"] = item.Army_No;
                                                cd1.Add(gn);
                                            }
                                        }
                                    }


                                    if (cd1.Count() == 0)
                                    {


                                        var list = from c in ed.tblCIDRs
                                                   where (c.Mobile == uidnn) && c.District_Code == decode
                                                   select c;

                                        if (list.Count() > 0)
                                        {

                                            foreach (var item in list)
                                            {
                                                Generalform gn = new Generalform();
                                                gn.UID = item.UID;
                                                gn.Citizen_ID = item.Citizen_ID;
                                                gn.Sanik_Name_eng = item.Citizen_Name_EN;
                                                gn.Father_Name_eng = item.Father_Name_EN;
                                                gn.Mother_Name_eng = item.Mother_Name_EN;
                                                gn.Gender = Convert.ToString(item.Gender);
                                                if (item.DOB != null)
                                                {
                                                    gn.DOBnew = Convert.ToDateTime(item.DOB);

                                                }
                                                else
                                                {
                                                    string datee = "01/01/0001";
                                                    gn.DOBnew = Convert.ToDateTime(datee);
                                                }

                                                gn.CategoryDesc = item.Caste_Category;
                                                gn.MaritalStatusDesc = (item.Marital_Status).ToString();
                                                gn.mobileno = item.Mobile;
                                                gn.emialid = item.Email_id;
                                                gn.Per_address_eng = item.House_Name_No;
                                                gn.Per_Landmark_english = item.Landmark_Locality_Colony;
                                                gn.statecode = item.State;
                                                gn.dcode = item.District_Code;
                                                gn.tcode = item.Block_Tehsil_Code;
                                                gn.VCODE = item.Village_Town_Code;
                                                gn.towncode = item.Village_Town_Code;
                                                gn.Urban_rural = Convert.ToString(item.RuralUrban);
                                                gn.Pin_code = item.PIN;
                                                gn.Cors_address = item.Correspondence_Address_EN;
                                                if (item.Photo != null)
                                                {

                                                    gn.ImgByte = Convert.ToBase64String((item.Photo).ToArray());



                                                }
                                                else
                                                {


                                                    gn.ImgByte = null;
                                                }
                                                cd.Add(gn);


                                            }
                                            return Json(cd, JsonRequestBehavior.AllowGet);
                                        }



                                    }


                                    if (cd1.Count() > 0)
                                    {

                                        cd = cd1;
                                        return Json(cd, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(JsonRequestBehavior.AllowGet);
                                }
                            }


                        }


                    }
                    else
                    {

                        return Json(cd, JsonRequestBehavior.AllowGet);

                    }



                }

                catch (Exception ex)
                {
                    throw ex;


                }
            }
            else
            {
                string msg = "not";
                return Json(new { msg }, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult maritalstatus()
        {
            try
            {
                var mart = (from mar in dta.MaritalStatus select mar);
                SelectList list1 = new SelectList(mart, "Marital_Code", "Marital_Status");
                ViewBag.MaritalStatusList = list1;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }

        }
        //banklist
        public ActionResult banklist()
        {
            try
            {
                var bank = (from bnk in ed.tblBankMasters select bnk);
                SelectList list16 = new SelectList(bank, "Bank_Code", "BankName");
                ViewBag.BankList = list16;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }

        }
        //award list
        public ActionResult awardlist()
        {
            try
            {
                var award = (from awrd in dta.tblawards select awrd);
                SelectList list17 = new SelectList(award, "awardID", "awardName");
                ViewBag.AwardList = list17;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }

        }

        //gender

        public ActionResult gnder()
        {
            try
            {
                var gend = (from gen in dta.Genders select gen);
                SelectList list2 = new SelectList(gend, "Gender_code", "Gender1");
                ViewBag.GenderList = list2;

                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }

        }
        //char list

        public ActionResult character()
        {
            try
            {
                var charc = (from charcat in dta.tblcharacters select charcat);
                SelectList list15 = new SelectList(charc, "Character_Id", "Character_Type");
                ViewBag.CharList = list15;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }

        }
        //medical status
        public ActionResult medical()
        {
            try
            {
                var medical = (from med in dta.tblmedicalcats select med);
                SelectList list14 = new SelectList(medical, "medical_ID", "MedicalCat_Type");
                ViewBag.MedicalList = list14;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }

        }
        //Rank
        public ActionResult rank()
        {
            try
            {
                var rank = (from rnk in dta.tblranks select rnk);
                SelectList list13 = new SelectList(rank, "Rank_ID", "Rank_Type");
                ViewBag.RankList = list13;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }

        }
        //regement corps 
        public ActionResult regm()
        {
            try
            {
                var reg = (from regmnt in dta.tblregement_corps select regmnt);
                SelectList list4 = new SelectList(reg, "regt_CorpsID", "regt_CorpsType");
                ViewBag.RegementCorpsList = list4;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }
        }
        //state
        public ActionResult state()
        {
            try
            {
                var st = (from stat in ed.tblstate_masters select stat);
                SelectList list5 = new SelectList(st, "StateCode", "StateName");
                ViewBag.StateList = list5;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }
        }

        //cast
        public ActionResult cast()
        {
            try
            {
                var cst1 = (from cst in ed.tblCasteCategoryMasters select cst);
                SelectList list = new SelectList(cst1, "CategoryCode", "CategoryDesc");
                ViewBag.CastList = list;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }
        }
        //force name
        public ActionResult forcename()
        {
            try
            {
                var forcenme = (from frnme in dta.Force_Dept_catgries select frnme);
                SelectList list11 = new SelectList(forcenme, "Force_Dept_Id", "Force_Name");
                ViewBag.ForceNameList = list11;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }
        }
        //forcedet name
        [HttpPost]


        public ActionResult forcenamedept(int forcedeptid)
        {

            List<SelectListItem> ForceCatList = new List<SelectListItem>();
            if (forcedeptid != 0)
            {

                List<ForceName> forcename = dta.ForceNames.Where(x => x.Force_Dept_ID == forcedeptid).ToList();
                forcename.ForEach(x =>
                {
                    ForceCatList.Add(new SelectListItem { Text = x.Force_Cat_Name, Value = (x.Force_Cat_ID).ToString() });
                });
            }
            return Json(ForceCatList, JsonRequestBehavior.AllowGet);
        }


        //town
        [HttpPost]


        public ActionResult Town(string tehsilId)
        {

            List<SelectListItem> TownList = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(tehsilId))
            {

                List<tblTownMaster> town = ed.tblTownMasters.Where(x => x.tcode == tehsilId).ToList();
                town.ForEach(x =>
                {
                    TownList.Add(new SelectListItem { Text = x.townname, Value = x.towncode });
                });
            }
            return Json(TownList, JsonRequestBehavior.AllowGet);
        }
        //village
        [HttpPost]


        public ActionResult village(string tehsilId)
        {

            List<SelectListItem> VillageList = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(tehsilId))
            {

                List<tblVillageMaster> village = ed.tblVillageMasters.Where(x => x.TCODE == tehsilId).ToList();
                village.ForEach(x =>
                {
                    VillageList.Add(new SelectListItem { Text = x.VNAME, Value = x.VCODE });
                });
            }
            return Json(VillageList, JsonRequestBehavior.AllowGet);
        }
        //district   
        [HttpPost]


        public ActionResult district(string stateId)
        {

            List<SelectListItem> DistrictList = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(stateId))
            {

                List<tblDistrictMaster> districts = ed.tblDistrictMasters.Where(x => x.statecode == stateId).ToList();
                districts.ForEach(x =>
                {
                    DistrictList.Add(new SelectListItem { Text = x.dname, Value = x.dcode });
                });
            }
            return Json(DistrictList, JsonRequestBehavior.AllowGet);
        }

        //tehsil

        [HttpPost]


        public ActionResult Tehsil(string districtId)
        {

            List<SelectListItem> TehsilList = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(districtId))
            {

                List<tblTehsilMaster> tehsil = ed.tblTehsilMasters.Where(x => x.dcode == districtId).ToList();
                tehsil.ForEach(x =>
                {
                    TehsilList.Add(new SelectListItem { Text = x.tname, Value = x.tcode });
                });
            }
            return Json(TehsilList, JsonRequestBehavior.AllowGet);
        }

        //Relationship
        public ActionResult Relationship()
        {
            try
            {
                var rel = (from realt in dta.tblsanikRelations select realt);
                SelectList list9 = new SelectList(rel, "RelationCode", "RelationDesc");
                ViewBag.RelationshipList = list9;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;


            }
        }

        //upload image and convert into bytes
        [HttpPost]
        public JsonResult FileToUpload()
        {
            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var pic = System.Web.HttpContext.Current.Request.Files["HelpSectionImages"];
            }
            string pathimg = string.Empty;
            for (int i = 0; i < Request.Files.Count; i++)
            {

                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);
                pathimg = Path.Combine(Server.MapPath("~/Imageupload/"), fileName);
                Session["PathImage"] = pathimg;

                file.SaveAs(pathimg);
                byte[] imagee = System.IO.File.ReadAllBytes(pathimg);
                Session["imagee"] = imagee;
            }
            System.Drawing.Image image = System.Drawing.Image.FromFile(pathimg, true);

            byte[] byteImage = imageToByteArray(image);
            string ImageBytestring = Convert.ToBase64String(byteImage);

            return Json(new { ImageBytestring }, JsonRequestBehavior.AllowGet);

        }

        private byte[] imageToByteArray(System.Drawing.Image image)
        {
            using (var ms = new MemoryStream())
            {

                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }


        //For Scheme Page

        [HttpPost]
        public ActionResult Schemes(Generalform mydata, FormCollection frmc)
        {

            try
            {
                if (ModelState.IsValidField("Scheme_Name") && ModelState.IsValidField("Date_of_Published"))
                {
                    tblscheme sc = new tblscheme();
                    sc.Scheme_Name = mydata.Scheme_Name;
                    sc.Date_of_Published = Convert.ToDateTime(mydata.Date_of_Published);

                    if (mydata.Active == true)
                    {
                        sc.Active = true;
                    }
                    else
                    {
                        sc.Active = false;
                    }
                    dta.tblschemes.InsertOnSubmit(sc);
                    dta.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Schemes");


        }

        [HttpGet]
        public ActionResult Schemes()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {
                    List<Generalform> _Scheme = (from sc in dta.tblschemes

                                                 select new Generalform
                                                 {
                                                     Scheme_ID = sc.Scheme_ID,
                                                     Scheme_Name = sc.Scheme_Name,
                                                     Date_of_Published = sc.Date_of_Published,
                                                     Active = sc.Active

                                                 }).ToList();
                    ViewBag.Schemes = _Scheme;

                    return View();
                }
                catch (Exception ex)
                {
                    throw ex;


                }

            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();

        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Generalform ms1 = new Generalform();
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];

                try
                {

                    var sc = dta.tblschemes.Single(s => s.Scheme_ID == id);
                    ms1.Scheme_ID = sc.Scheme_ID;
                    ms1.Scheme_Name = sc.Scheme_Name;
                    ms1.Date_of_Published = sc.Date_of_Published;
                    if (sc.Active == true)
                    {
                        ms1.Active = true;
                    }
                    else
                    {
                        ms1.Active = false;
                    }
                    return View("Edit", ms1);
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }
            return View("Edit", ms1);


        }

        [HttpPost]
        public ActionResult Edit(Generalform mydata, FormCollection frmc, Rajya_Sanik_Board.tblscheme sch)
        {

            try
            {
                if (ModelState.IsValidField("Scheme_Name") && ModelState.IsValidField("Date_of_Published"))
                {


                    tblscheme sc = dta.tblschemes.Where(x => x.Scheme_ID == sch.Scheme_ID).FirstOrDefault();

                    sc.Scheme_Name = sch.Scheme_Name;
                    sc.Date_of_Published = sch.Date_of_Published;
                    if (sch.Active == true)
                    {
                        sc.Active = true;
                    }
                    else
                    {
                        sc.Active = false;
                    }
                    dta.SubmitChanges();

                }


            }
            catch (Exception ex)
            {
                // throw ex;
                //changes on 13-03-2018
                mydata.Error = ex.Message;
                mydata.PageError = "/View/RSBGeneralInfrm";
                mydata.DateError = DateTime.Now;
                mydata.DetailError = ex.ToString();
                tblTraceError error = new tblTraceError();
                error.Error = mydata.Error;
                error.PageError = mydata.PageError;
                error.DateError = mydata.DateError;
                error.DetailError = mydata.DetailError;

                ed.tblTraceErrors.InsertOnSubmit(error);
                ed.SubmitChanges();
                //end
                return Json(JsonRequestBehavior.AllowGet);


            }


            return RedirectToAction("Schemes");



        }
        //edit in dependent

        public JsonResult Editdepen1(int id)
        {
            try
            {
                List<Generalform> cd = new List<Generalform>();
                Generalform ms1 = new Generalform();

                var list = (from fm in dta.tblfamilydetails
                            where fm.Dependent_Id == id
                            select fm);

                if (list.Count() > 0)
                {

                    foreach (var item in list)
                    {
                        Generalform gn = new Generalform();
                        gn.Army_No = item.Army_No;
                        gn.Dependent_Name = item.Dependent_Name;
                        gn.RelationCode = item.RelationCode;
                        if (item.UID == null)
                        {
                            gn.UID = null;
                        }
                        else
                        {
                            gn.UID = item.UID;
                        }
                        if (item.DOB != null)
                        {

                            gn.Dependent_DOB1 = Convert.ToDateTime(item.DOB);


                        }
                        else
                        {
                            string datee = "01/01/0001";
                            gn.Dependent_DOB1 = Convert.ToDateTime(datee);
                        }

                        gn.MaritalStatusCode = (item.MaritalStatusCode).ToString();
                        if (item.DOM != null)
                        {


                            gn.DOM1 = Convert.ToDateTime(item.DOM);


                        }
                        else
                        {
                            string datee = "01/01/0001";
                            gn.DOM1 = Convert.ToDateTime(datee);
                        }


                        if (item.imagedept != null)
                        {

                            gn.ImgByte = Convert.ToBase64String((item.imagedept).ToArray());



                        }
                        //else
                        //{


                        //    gn.ImgByte = null;
                        //}
                        //changes 07-03-2018//
                        gn.Wife_recorded_in_PPO_no = item.Wife_recorded_in_PPO_no;
                        gn.Wife_othername = item.Wife_OtherName;
                        gn.Number_Of_children = Convert.ToString(item.Number_Of_children);
                        gn.Recorded_in_DO_Part2 = item.Recorded_in_DO_Part2;
                        gn.Recorded_in_DO_Part2text = item.Recorded_in_DO_Part2text;
                        //end//
                        cd.Add(gn);


                    }
                    return Json(cd, JsonRequestBehavior.AllowGet);
                }
                return Json(JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                // throw ex;
                //changes on 13-03-2018
                mydata.Error = ex.Message;
                mydata.PageError = "/View/RSBGeneralInfrm";
                mydata.DateError = DateTime.Now;
                mydata.DetailError = ex.ToString();
                tblTraceError error = new tblTraceError();
                error.Error = mydata.Error;
                error.PageError = mydata.PageError;
                error.DateError = mydata.DateError;
                error.DetailError = mydata.DetailError;

                ed.tblTraceErrors.InsertOnSubmit(error);
                ed.SubmitChanges();
                //end
                return Json(JsonRequestBehavior.AllowGet);


            }
        }
        public JsonResult Editdepen(Generalform mydata, int id1)
        {
            DateTime datee = Convert.ToDateTime("01/01/2000");
            string msg;
            bool dependent, cid;

            dependent = IsName(mydata.Dependent_Name);
            cid = IsNumber(mydata.UID);

            if (cid == false || dependent == false)
            {

                if (cid == false)
                {
                    msg = "c";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (dependent == false)
                {
                    msg = "d";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                try
                {
                    DateTime Dependent_DOB, DOM;
                    var nn1 = mydata.Dependent_DOB;
                    DateTime ValidDate1;
                    DateTime ValidDate2;
                    //DateTime? ValidDate2;
                    var nn3 = mydata.DOM;
                    tblfamilydetail sc = dta.tblfamilydetails.Where(x => x.Dependent_Id == id1).FirstOrDefault();
                    if (sc != null)
                    {
                        sc.Dependent_Name = mydata.Dependent_Name;
                        if (mydata.RelationCode == "0")
                        {
                            string rel = "Please select relation";
                            return Json(new { rel }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            sc.RelationCode = mydata.RelationCode;
                        }


                        sc.UID = mydata.UID;

                        if (mydata.Dependent_DOB != null)
                        {

                            if (DateTime.TryParseExact(nn1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
                            {

                                Dependent_DOB = ValidDate1;
                                if (Dependent_DOB != DateTime.Now.Date)
                                {
                                    var nn = mydata.Dependent_DOB;

                                    //new 03-03-2018//
                                    if (nn != null)
                                    {
                                        DateTime ValidDate;
                                        //DateTime? ValidDate2;

                                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                        {
                                            sc.DOB = ValidDate;
                                        }
                                    }
                                    else
                                    {
                                        mydata.Error = "date of dependent error";
                                        mydata.PageError = "/View/RSBGeneralInfrm";
                                        mydata.DateError = DateTime.Now;
                                        mydata.DetailError = nn.ToString();
                                        tblTraceError error1 = new tblTraceError();
                                        error1.Error = mydata.Error;
                                        error1.PageError = mydata.PageError;
                                        error1.DateError = mydata.DateError;
                                        error1.DetailError = mydata.DetailError;

                                        ed.tblTraceErrors.InsertOnSubmit(error1);
                                        ed.SubmitChanges();


                                    }



                                }
                                else
                                {
                                    string dob = "DOB is not valid !!";
                                    return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                string dob = "DOB is not valid !!";
                                return Json(new { dob }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        else
                        {
                            sc.DOB = null;
                        }


                        sc.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusDesc);
                        if (Convert.ToString(sc.MaritalStatusCode) == "2")
                        {
                            sc.DOM = null;

                        }
                        else
                        {
                            if (mydata.DOM == null)
                            {
                                string domm = "Date of marriage not null !!";
                                return Json(new { domm }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {

                                if (DateTime.TryParseExact(nn3.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate2))
                                {
                                    DOM = ValidDate2;

                                    if (DOM <= DateTime.Now.Date)
                                    {
                                        if (mydata.Dependent_DOB == mydata.DOM)
                                        {

                                            string des = "Date of marriage and Date of Birth Not Equal !!";
                                            return Json(new { des }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            var nn = mydata.DOM;

                                            //new 03-03-2018//
                                            if (nn != null)
                                            {
                                                DateTime ValidDate;
                                                //DateTime? ValidDate2;

                                                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                                {
                                                    sc.DOM = ValidDate;
                                                }
                                            }
                                            else
                                            {
                                                mydata.Error = "date of dependent edit error";
                                                mydata.PageError = "/View/RSBGeneralInfrm";
                                                mydata.DateError = DateTime.Now;
                                                mydata.DetailError = nn.ToString();
                                                tblTraceError error1 = new tblTraceError();
                                                error1.Error = mydata.Error;
                                                error1.PageError = mydata.PageError;
                                                error1.DateError = mydata.DateError;
                                                error1.DetailError = mydata.DetailError;

                                                ed.tblTraceErrors.InsertOnSubmit(error1);
                                                ed.SubmitChanges();


                                            }

                                        }
                                    }
                                    else
                                    {
                                        string dom = "Date of marriage is not valid !!";
                                        return Json(new { dom }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    string dom = "Date of marriage is not valid !!";
                                    return Json(new { dom }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                        if (Session["imagee"] != null)
                        {
                            sc.imagedept = (byte[])Session["imagee"];
                            Session["imagee"] = null;
                        }
                        else
                        {
                            //if (mydata.imagedept == null)
                            //{
                            //    sc.imagedept = (byte[])Session["imagee"];
                            //    Session["imagee"] = null;
                            //}
                            //else
                            //{

                            sc.imagedept = mydata.imagedept;
                            // }

                        }
                        //chnges 07-03-2018//
                        sc.Wife_recorded_in_PPO_no = mydata.Wife_recorded_in_PPO_no;
                        sc.Wife_OtherName = mydata.Wife_othername;
                        sc.Number_Of_children = Convert.ToInt16(mydata.Number_Of_children);
                        sc.Recorded_in_DO_Part2 = mydata.Recorded_in_DO_Part2;
                        sc.Recorded_in_DO_Part2text = mydata.Recorded_in_DO_Part2text;
                        sc.Wife_recorded_in_PPO_no = mydata.Wife_recorded_in_PPO_no;
                        //end

                        dta.SubmitChanges();
                        // }

                    }
                    else
                    {
                        string msg1 = "not";
                        return Json(msg1, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;


                }
                return Json(JsonRequestBehavior.AllowGet);

            }
            return Json(JsonRequestBehavior.AllowGet);

        }

        public JsonResult Editcomplain1(int id)
        {
            try
            {
                Generalform gn = new Generalform();
                List<Generalform> cd = new List<Generalform>();

                var _ms2 = (from fm in dta.Force_Complaints
                            where fm.Complain_Id == id
                            select new Generalform
                            {
                                Complain_Id = fm.Complain_Id,
                                Army_No = fm.Army_No,
                                Name_of_Complain = fm.Name_of_Complain,
                                Level_of_decision = fm.Level_of_decision,
                                Date_of_Complain1 = fm.Date_of_Complain,
                                Pending_With = fm.Pending_With,
                                Decision_Given = fm.Decision_Given

                            }).ToList();

                if (_ms2.Count() > 0)
                {
                    foreach (var item in _ms2)
                    {
                        gn.Complain_Id = item.Complain_Id;
                        gn.Army_No = item.Army_No;
                        gn.Name_of_Complain = item.Name_of_Complain;
                        gn.Level_of_decision = item.Level_of_decision;
                        if (item.Date_of_Complain1 != null)
                        {
                            gn.Date_of_Complain1 = item.Date_of_Complain1;

                        }
                        else
                        {
                            string datee = "01/01/0001";
                            gn.Date_of_Complain1 = Convert.ToDateTime(datee);
                        }

                        gn.Pending_With = item.Pending_With;
                        gn.Decision_Given = item.Decision_Given;
                        cd.Add(gn);
                    }
                    return Json(cd, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }
        }

        public JsonResult Editcomplain(Generalform mydata, int id1)
        {
            try
            {

                Force_Complaint sc = dta.Force_Complaints.Where(x => x.Complain_Id == id1).FirstOrDefault();
                if (sc != null)
                {
                    string msg;
                    bool nameofcmpln, Levelofdecision, Pending_With;
                    Levelofdecision = Citizen(mydata.Level_of_decision);
                    Pending_With = IsName(mydata.Pending_With);
                    nameofcmpln = IsName(mydata.Name_of_Complain);
                    if (Levelofdecision == false || nameofcmpln == false || Pending_With == false)
                    {

                        if (Levelofdecision == false)
                        {
                            msg = "lvlofdecsn";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else if (Pending_With == false)
                        {
                            msg = "pndwth";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        else if (nameofcmpln == false)
                        {
                            msg = "nmecmpln";
                            return Json(new { msg }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        sc.Name_of_Complain = mydata.Name_of_Complain;
                        sc.Level_of_decision = mydata.Level_of_decision;
                        if (mydata.Date_of_Complain != null)
                        {
                            var nn = mydata.Date_of_Complain;

                            //new 03-03-2018//
                            if (nn != null)
                            {
                                DateTime ValidDate;
                                //DateTime? ValidDate2;

                                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                {
                                    sc.Date_of_Complain = ValidDate;
                                }
                            }
                            else
                            {
                                sc.Date_of_Complain = null;
                            }
                        }
                        //sc.Date_of_Complain = mydata.Date_of_Complain;
                        sc.Pending_With = mydata.Pending_With;
                        sc.Decision_Given = mydata.Decision_Given;
                        dta.SubmitChanges();
                        // }
                        return Json(JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    string msg1 = "not";
                    return Json(msg1, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;


            }

        }
        //loan table
        public JsonResult EditLoan1(int id)
        {
            try
            {
                Generalform gn = new Generalform();
                List<Generalform> cd = new List<Generalform>();

                var _ms2 = (from fm in dta.Sanik_Loans
                            where fm.Loan_Id == id
                            select new Generalform
                            {
                                Loan_id = fm.Loan_Id,
                                Army_No = fm.Army_No,
                                Loan_Amount = fm.Loan_Amount,
                                Purpose = fm.Purpose,
                                Outstanding_Amount = fm.Outstanding_Amount,
                                Date_loan1 = fm.Date_loan,
                                Remarks = fm.Remarks

                            }).ToList();
                if (_ms2.Count() > 0)
                {
                    foreach (var item in _ms2)
                    {
                        gn.Loan_id = item.Loan_id;
                        gn.Army_No = item.Army_No;
                        gn.Loan_Amount = item.Loan_Amount;
                        gn.Purpose = item.Purpose;
                        gn.Outstanding_Amount = item.Outstanding_Amount;
                        if (item.Date_loan1 != null)
                        {
                            gn.Date_loan1 = item.Date_loan1;

                        }
                        else
                        {
                            string datee = "01/01/0001";
                            gn.Date_loan1 = Convert.ToDateTime(datee);

                        }
                        gn.Remarks = item.Remarks;
                        cd.Add(gn);
                    }
                    return Json(cd, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                throw ex;


            }
        }
        public JsonResult Editloan(Generalform mydata, int id1)
        {
            string msg;
            bool outstandingamount, loanamount;


            outstandingamount = Decimalchk(Convert.ToString(mydata.Outstanding_Amount));
            loanamount = Decimalchk(Convert.ToString(mydata.Loan_Amount));

            if (loanamount == false || outstandingamount == false)
            {

                if (outstandingamount == false)
                {
                    msg = "outstnding";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (loanamount == false)
                {
                    msg = "loanamount";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }

                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {

                    Sanik_Loan sc = dta.Sanik_Loans.Where(x => x.Loan_Id == id1).FirstOrDefault();
                    if (sc != null)
                    {
                        sc.Army_No = mydata.Army_No;
                        sc.Loan_Amount = mydata.Loan_Amount;
                        sc.Purpose = mydata.Purpose;
                        sc.Outstanding_Amount = mydata.Outstanding_Amount;
                        ///////////////
                        if (mydata.Date_loan != null)
                        {
                            var nn = mydata.Date_loan;

                            //new 03-03-2018//
                            if (nn != null)
                            {
                                DateTime ValidDate;
                                //DateTime? ValidDate2;

                                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                {
                                    sc.Date_loan = ValidDate;
                                }
                                else
                                {
                                    sc.Date_loan = null;
                                }
                            }
                        }
                        //sc.Date_loan = mydata.Date_loan;
                        sc.Remarks = mydata.Remarks;
                        dta.SubmitChanges();

                        return Json(JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        string msg1 = "not";
                        return Json(msg1, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }

        }
        //selectionoption
        public ActionResult selectionoption()
        {
            try
            {

                var list = new List<SelectListItem>
                    {
                        new SelectListItem{ Text="Aadhar Id", Value = "uid"},
                        new SelectListItem{ Text="CIDR", Value = "cid" },
                        new SelectListItem{ Text="Mobile No", Value = "mobileno"}, 
                        new SelectListItem{ Text="ArmyNo", Value = "armyno"}, 
                        new SelectListItem{ Text="ESM Identity Cardnumber", Value = "esmino"}
                             
                    };

                ViewData["Selectionoption"] = list;
                return View();

            }
            catch (Exception ex)
            {
                throw ex;


            }
        }
        //award edit

        public JsonResult Editaward1(int id)
        {
            try
            {
                Generalform gn = new Generalform();
                List<Generalform> cd = new List<Generalform>();

                var _ms2 = (from fm in dta.Sanik_awards
                            where fm.Sanikawrdid == id
                            select new Generalform
                            {
                                Sanikawrdid = fm.Sanikawrdid,
                                Army_No = fm.Army_No,
                                awardID = fm.awardID,
                                award_date1 = fm.award_date,
                                Perpose = fm.Perpose,

                            }).ToList();
                if (_ms2.Count() > 0)
                {

                    foreach (var item in _ms2)
                    {
                        gn.Sanikawrdid = item.Sanikawrdid;
                        gn.Army_No = item.Army_No;
                        gn.awardID = item.awardID;
                        if (item.award_date1 != null)
                        {
                            gn.award_date1 = Convert.ToDateTime(item.award_date1);

                        }
                        else
                        {
                            string datee = "01/01/0001";
                            gn.award_date1 = Convert.ToDateTime(datee);
                        }

                        gn.Perpose = item.Perpose;


                        cd.Add(gn);
                    }
                    return Json(cd, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }
        }
        public JsonResult Editaward(Generalform mydata, int id1)
        {
            try
            {

                Sanik_award sc = dta.Sanik_awards.Where(x => x.Sanikawrdid == id1).FirstOrDefault();
                if (sc != null)
                {
                    sc.Army_No = mydata.Army_No;
                    sc.awardID = mydata.awardID;
                    if (mydata.award_date == null)
                    {
                        sc.award_date = null;
                    }
                    else
                    {
                        var nn = mydata.award_date;

                        //new 03-03-2018//
                        if (nn != null)
                        {
                            DateTime ValidDate;
                            //DateTime? ValidDate2;

                            if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                            {
                                sc.award_date = ValidDate;
                            }
                            else
                            {
                                sc.award_date = null;
                            }
                        }
                    }

                    sc.Perpose = mydata.Perpose;
                    dta.SubmitChanges();

                    return Json(sc, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string msg1 = "not";
                    return Json(msg1, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }
        }

        //edit in court case
        public JsonResult Editcourtcase1(int id)
        {

            try
            {
                Generalform ms1 = new Generalform();


                List<Generalform> _ms2 = (from fm in dta.Force_Court_Cases
                                          where fm.Court_Case_Id == id
                                          select new Generalform
                                          {
                                              Army_No = fm.Army_No,
                                              Case_No = fm.Case_No,
                                              Case_Year = fm.Case_Year,
                                              Court_Name = fm.Court_Name,
                                              Decision = fm.Decision,

                                          }).ToList();

                return Json(_ms2, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;


            }
        }
        public JsonResult Editcourtcase(Generalform mydata, int id1)
        {
            string msg;
            bool caseno, caseyear, courtname;


            caseno = IsNumber(mydata.Case_No);
            caseyear = IsNumber(mydata.Case_Year);
            courtname = IsName(mydata.Court_Name);
            if (caseyear == false || caseno == false || courtname == false)
            {

                if (caseyear == false)
                {
                    msg = "cyear";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (caseno == false)
                {
                    msg = "cse";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (courtname == false)
                {
                    msg = "crt";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {

                    Force_Court_Case sc = dta.Force_Court_Cases.Where(x => x.Court_Case_Id == id1).FirstOrDefault();
                    if (sc != null)
                    {
                        sc.Case_No = mydata.Case_No;
                        sc.Case_Year = mydata.Case_Year;
                        sc.Court_Name = mydata.Court_Name;
                        sc.Decision = mydata.Decision;

                        dta.SubmitChanges();
                    }
                    else
                    {
                        string msg1 = "not";
                        return Json(msg1, JsonRequestBehavior.AllowGet);
                    }


                }
                catch (Exception ex)
                {
                    throw ex;


                }
                return Json(JsonRequestBehavior.AllowGet);
            }


        }


        //        /// <summary>
        //        /// Medical Page
        //        /// </summary>
        //        /// <param name="mydata"></param>
        //        /// <param name="frmc"></param>
        //        /// <param name="med"></param>
        //        /// <returns></returns>
        [HttpPost]
        public ActionResult Editmed(Generalform mydata, FormCollection frmc, Rajya_Sanik_Board.tblmedicalcat med)
        {

            try
            {
                //if (ModelState.IsValid)
                if (ModelState.IsValidField("MedicalCat_Type"))
                {

                    tblmedicalcat md = dta.tblmedicalcats.Where(x => x.medical_ID == med.medical_ID).FirstOrDefault();

                    md.MedicalCat_Type = med.MedicalCat_Type;

                    dta.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Medical_Status");


        }

        [HttpGet]
        public ActionResult Editmed(int id)
        {
            string strPreviousPage = "";
            Generalform ms1 = new Generalform();
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];

                try
                {

                    var md = dta.tblmedicalcats.Single(s => s.medical_ID == id);
                    ms1.medical_ID = md.medical_ID;
                    ms1.MedicalCat_Type = md.MedicalCat_Type;
                    return View("Editmed", ms1);
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("Editmed", ms1);


        }
        [HttpPost]
        public ActionResult Medical_Status(Generalform mydata, FormCollection frmc)
        {
            try
            {
                // if (ModelState.IsValid)
                if (ModelState.IsValidField("MedicalCat_Type"))
                {
                    tblmedicalcat md = new tblmedicalcat();
                    md.MedicalCat_Type = mydata.MedicalCat_Type;
                    dta.tblmedicalcats.InsertOnSubmit(md);
                    dta.SubmitChanges();
                }
                // var errors = ModelState.Values.SelectMany(v => v.Errors);
            }

            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Medical_Status");


        }

        [HttpGet]
        public ActionResult Medical_Status()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];

                try
                {
                    List<Generalform> _med = (from md in dta.tblmedicalcats

                                              select new Generalform
                                              {
                                                  medical_ID = md.medical_ID,
                                                  MedicalCat_Type = md.MedicalCat_Type


                                              }).ToList();
                    ViewBag.medical = _med;

                    return View();
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();

        }
        //        // Award Page

        [HttpPost]
        public ActionResult Editawd(Generalform mydata, FormCollection frmc, Rajya_Sanik_Board.tblaward awd)
        {

            try
            {
                //ModelState.Remove("ErrorKey");
                //if (ModelState.IsValid)
                if (ModelState.IsValidField("awardName"))
                {


                    tblaward md = dta.tblawards.Where(x => x.awardID == awd.awardID).FirstOrDefault();

                    md.awardName = awd.awardName;
                    dta.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Award");


        }

        [HttpGet]
        public ActionResult Editawd(int id)
        {
            Generalform ms1 = new Generalform();
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {

                    var awd = dta.tblawards.Single(s => s.awardID == id);
                    ms1.awardID = awd.awardID;
                    ms1.awardName = awd.awardName;
                    return View("Editawd", ms1);
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("Editawd", ms1);


        }
        [HttpPost]
        public ActionResult Award(Generalform mydata, FormCollection frmc)
        {
            try
            {
                if (ModelState.IsValidField("awardName"))
                {

                    tblaward awd = new tblaward();
                    awd.awardName = mydata.awardName;
                    dta.tblawards.InsertOnSubmit(awd);
                    dta.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Award");


        }

        [HttpGet]
        public ActionResult Award()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {
                    List<Generalform> _awd = (from aw in dta.tblawards

                                              select new Generalform
                                              {
                                                  awardID = aw.awardID,
                                                  awardName = aw.awardName


                                              }).ToList();
                    ViewBag.award = _awd;

                    return View();
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();


        }
        //        //Rank 

        [HttpPost]
        public ActionResult Editrank(Generalform mydata, FormCollection frmc, Rajya_Sanik_Board.tblrank rnk)
        {

            try
            {
                //if (ModelState.IsValid)
                if (ModelState.IsValidField("Rank_Type"))
                {


                    tblrank md = dta.tblranks.Where(x => x.Rank_ID == rnk.Rank_ID).FirstOrDefault();

                    md.Rank_Type = rnk.Rank_Type;
                    dta.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Rank");


        }

        [HttpGet]
        public ActionResult Editrank(int id)
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {
                    Generalform ms1 = new Generalform();
                    var awd = dta.tblranks.Single(s => s.Rank_ID == id);
                    ms1.Rank_ID = awd.Rank_ID;
                    ms1.Rank_Type = awd.Rank_Type;
                    return View("Editrank", ms1);
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();

        }
        [HttpPost]
        public ActionResult Rank(Generalform mydata, FormCollection frmc)
        {

            try
            {
                if (ModelState.IsValidField("Rank_Type"))
                {

                    tblrank rnk = new tblrank();
                    rnk.Rank_Type = mydata.Rank_Type;
                    dta.tblranks.InsertOnSubmit(rnk);
                    dta.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Rank");


        }

        [HttpGet]
        public ActionResult Rank()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {
                    List<Generalform> _rnk = (from rk in dta.tblranks

                                              select new Generalform
                                              {
                                                  Rank_ID = rk.Rank_ID,
                                                  Rank_Type = rk.Rank_Type


                                              }).ToList();
                    ViewBag.rank = _rnk;

                    return View();
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }

            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();

        }
        //        //Regement crops
        [HttpPost]
        public ActionResult Editreg(Generalform mydata, FormCollection frmc, Rajya_Sanik_Board.tblregement_corp reg)
        {

            try
            {
                //if (ModelState.IsValid)
                if (ModelState.IsValidField("regt_CorpsType"))
                {

                    //{

                    tblregement_corp md = dta.tblregement_corps.Where(x => x.regt_CorpsID == reg.regt_CorpsID).FirstOrDefault();

                    md.regt_CorpsType = reg.regt_CorpsType;

                    dta.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Regcorps");


        }

        [HttpGet]
        public ActionResult Editreg(int id)
        {
            Generalform ms1 = new Generalform();
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {

                    var reg = dta.tblregement_corps.Single(s => s.regt_CorpsID == id);
                    ms1.regt_CorpsID = reg.regt_CorpsID;
                    ms1.regt_CorpsType = reg.regt_CorpsType;
                    return View("Editreg", ms1);
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("Editreg", ms1);

        }
        [HttpPost]
        public ActionResult Regcorps(Generalform mydata, FormCollection frmc)
        {

            try
            {
                if (ModelState.IsValidField("regt_CorpsType"))
                {
                    tblregement_corp reg = new tblregement_corp();
                    reg.regt_CorpsType = mydata.regt_CorpsType;
                    dta.tblregement_corps.InsertOnSubmit(reg);
                    dta.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Regcorps");


        }

        [HttpGet]
        public ActionResult Regcorps()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {
                    List<Generalform> _reg = (from rg in dta.tblregement_corps

                                              select new Generalform
                                              {
                                                  regt_CorpsID = rg.regt_CorpsID,
                                                  regt_CorpsType = rg.regt_CorpsType


                                              }).ToList();
                    ViewBag.regcorps = _reg;

                    return View();
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();


        }
        //character 
        [HttpPost]
        public ActionResult Editchar(Generalform mydata, FormCollection frmc, Rajya_Sanik_Board.tblcharacter character)
        {

            try
            {
                //if (ModelState.IsValid)
                if (ModelState.IsValidField("Character_Type") && ModelState.IsValidField("Character_Description"))
                {
                    //{

                    tblcharacter charac = dta.tblcharacters.Where(x => x.Character_Id == character.Character_Id).FirstOrDefault();

                    charac.Character_Type = character.Character_Type;
                    charac.Character_Description = character.Character_Description;
                    dta.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Character");


        }
        [HttpGet]
        public ActionResult Editchar(int id)
        {
            Generalform ms1 = new Generalform();
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {

                    var pn = dta.tblcharacters.Single(s => s.Character_Id == id);
                    ms1.Character_Id = pn.Character_Id;
                    ms1.Character_Type = pn.Character_Type;
                    ms1.Character_Description = pn.Character_Description;
                    return View("Editchar", ms1);
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("Editchar", ms1);

        }

        //// pension Page

        [HttpPost]
        public ActionResult Editpen(Generalform mydata, FormCollection frmc, Rajya_Sanik_Board.tblpensiontype pen)
        {

            try
            {
                //if (ModelState.IsValid)
                if (ModelState.IsValidField("Pensionstatus_Type") && ModelState.IsValidField("Description"))
                {
                    //{

                    tblpensiontype pn = dta.tblpensiontypes.Where(x => x.Pensionstatus_ID == pen.Pensionstatus_ID).FirstOrDefault();

                    pn.Pensionstatus_Type = pen.Pensionstatus_Type;
                    pn.Description = pen.Description;
                    dta.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Pension");


        }

        [HttpGet]
        public ActionResult Editpen(int id)
        {
            Generalform ms1 = new Generalform();
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {

                    var pn = dta.tblpensiontypes.Single(s => s.Pensionstatus_ID == id);
                    ms1.Pensionstatus_ID = pn.Pensionstatus_ID;
                    ms1.Pensionstatus_Type = pn.Pensionstatus_Type;
                    ms1.Description = pn.Description;
                    return View("Editpen", ms1);
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("Editpen", ms1);

        }
        [HttpPost]
        public ActionResult Pension(Generalform mydata, FormCollection frmc)
        {
            try
            {
                if (ModelState.IsValidField("Pensionstatus_Type") && ModelState.IsValidField("Description"))
                {
                    tblpensiontype pen = new tblpensiontype();
                    pen.Pensionstatus_Type = mydata.Pensionstatus_Type;
                    pen.Description = mydata.Description;
                    dta.tblpensiontypes.InsertOnSubmit(pen);
                    dta.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Pension");


        }

        [HttpGet]
        public ActionResult Pension()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {
                    List<Generalform> _pen = (from pn in dta.tblpensiontypes

                                              select new Generalform
                                              {
                                                  Pensionstatus_ID = pn.Pensionstatus_ID,
                                                  Pensionstatus_Type = pn.Pensionstatus_Type,
                                                  Description = pn.Description


                                              }).ToList();
                    ViewBag.pension = _pen;

                    return View();
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();


        }
        [HttpPost]
        public ActionResult Character(Generalform mydata, FormCollection frmc)
        {
            try
            {
                if (ModelState.IsValidField("Character_Type") && ModelState.IsValidField("Character_Description"))
                {
                    tblcharacter character = new tblcharacter();
                    character.Character_Type = mydata.Character_Type;
                    character.Character_Description = mydata.Character_Description;
                    dta.tblcharacters.InsertOnSubmit(character);
                    dta.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;


            }


            return RedirectToAction("Character");
        }
        [HttpGet]
        public ActionResult Character()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                try
                {
                    List<Generalform> _char = (from pn in dta.tblcharacters

                                               select new Generalform
                                               {
                                                   Character_Id = pn.Character_Id,
                                                   Character_Type = pn.Character_Type,
                                                   Character_Description = pn.Character_Description


                                               }).ToList();
                    ViewBag.character = _char;

                    return View();
                }
                catch (Exception ex)
                {
                    throw ex;


                }
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View();

        }
        //delete  in dependent id  
        [HttpPost]
        public JsonResult Delete(Generalform model, int id)
        {
            try
            {
                var m = from fm in dta.tblfamilydetails where fm.Dependent_Id == id select fm;
                foreach (var detail in m)
                {
                    dta.tblfamilydetails.DeleteOnSubmit(detail);
                }



                dta.SubmitChanges();
                var data = from fm in dta.tblfamilydetails where fm.Army_No == Session["armynoalrd"] select fm;

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //delete in courtcase
        [HttpPost]
        public JsonResult Deletecourtcase(Generalform model, int id)
        {
            try
            {
                var m = from court in dta.Force_Court_Cases where court.Court_Case_Id == id select court;
                foreach (var detail in m)
                {
                    dta.Force_Court_Cases.DeleteOnSubmit(detail);
                }



                dta.SubmitChanges();
                var data = from court in dta.Force_Court_Cases where court.Army_No == Session["armynoalrd"] select court;
                return Json(data, JsonRequestBehavior.AllowGet); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public JsonResult Deletecomplain(Generalform model, int id)
        {
            try
            {
                var m = from complain in dta.Force_Complaints where complain.Complain_Id == id select complain;
                foreach (var detail in m)
                {
                    dta.Force_Complaints.DeleteOnSubmit(detail);
                }



                dta.SubmitChanges();
                var data = from complain in dta.Force_Complaints where complain.Army_No == Session["armynoalrd"] select complain;
                return Json(data, JsonRequestBehavior.AllowGet); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public JsonResult Deleteloan(Generalform model, int id)
        {
            try
            {
                var m = from loan in dta.Sanik_Loans where loan.Loan_Id == id select loan;
                foreach (var detail in m)
                {
                    dta.Sanik_Loans.DeleteOnSubmit(detail);
                }



                dta.SubmitChanges();
                var data = from loan in dta.Sanik_Loans where loan.Army_No == Session["armynoalrd"] select loan;
                return Json(data, JsonRequestBehavior.AllowGet); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public JsonResult Deleteaward(Generalform model, int id)
        {
            try
            {
                var m = from award in dta.Sanik_awards where award.Sanikawrdid == id select award;
                foreach (var detail in m)
                {
                    dta.Sanik_awards.DeleteOnSubmit(detail);
                }



                dta.SubmitChanges();
                var data = from award in dta.Sanik_awards where award.Army_No == Session["armynoalrd"] select award;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //webcam work

        [HttpPost]
        public ActionResult Upload(string images)
        {
            images = images.Substring("data:image/png;base64,".Length);

            return Json(new { ImageBytesAsString2 = images });
        }

        //Report
        //public ActionResult Call(string images)
        //    {

        //        try
        //        {
        //            if (Session["aarmyno"] == null)
        //            {
        //                ViewBag.message = "Army number Not null";
        //                //return ViewBag();
        //                return Json(new { success = ViewBag.message }, JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {

        //                string armyno = (String)Session["aarmyno"];
        //                return RedirectToAction("CallReport", "Report", new { armyno });
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //            throw ex;
        //        }

        //    }


        //Report through table
        public JsonResult Report()
        {
            var army = (string)Session["aarmyno"];
            var result1 = (dta.GetSanikdetail(army)).ToList();
            var result2 = (dta.GetSanikaward(army)).ToList();
            var result3 = (dta.GetSanikcomp(army)).ToList();
            var result4 = (dta.GetSanikdep(army)).ToList();
            var result5 = (dta.GetSanikloan(army)).ToList();
            var result6 = (dta.GetSanikcourtcase(army)).ToList();
            ViewBag.report1 = result1;
            ViewBag.report2 = result2;
            ViewBag.report3 = result3;
            ViewBag.report4 = result4;
            ViewBag.report5 = result5;
            ViewBag.report6 = result6;
            int res = 1;
            return Json(res, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ReturnReport()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                var army = (string)Session["aarmyno"];
                var result1 = (dta.GetSanikdetail(army)).ToList();
                var result2 = (dta.GetSanikaward(army)).ToList();
                var result3 = (dta.GetSanikcomp(army)).ToList();
                var result4 = (dta.GetSanikdep(army)).ToList();
                var result5 = (dta.GetSanikloan(army)).ToList();
                var result6 = (dta.GetSanikcourtcase(army)).ToList();
                ViewBag.report1 = result1;
                ViewBag.report2 = result2;
                ViewBag.report3 = result3;
                ViewBag.report4 = result4;
                ViewBag.report5 = result5;
                ViewBag.report6 = result6;
                return View("Report");
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("Report");


        }
        //for link
        public ActionResult Report2()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                return View("ArmyWiseReport");
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("ArmyWiseReport");


        }
        public JsonResult ArmyReport(string Army_No)
        {
            if (Session["dcode"] != null && (string)Session["usertype"] == "72" || (string)Session["usertype"] == "70")
            {
                var dcode = (string)Session["dcode"];
                var army = Army_No;
                Session["army"] = Army_No;
                var result1 = (dta.GetSanikdetail1(army, dcode)).ToList();
                if (result1.Count() == 0)
                {
                    Session["army1"] = null;

                }
                else
                {
                    Session["army1"] = result1.FirstOrDefault().Army_No;
                }
                var arm = (string)Session["army1"];
                var result2 = (dta.GetSanikaward(arm)).ToList();
                var result3 = (dta.GetSanikcomp(arm)).ToList();
                var result4 = (dta.GetSanikdep(arm)).ToList();
                var result5 = (dta.GetSanikloan(arm)).ToList();
                var result6 = (dta.GetSanikcourtcase(arm)).ToList();
                ViewBag.report1 = result1;
                ViewBag.report2 = result2;
                ViewBag.report3 = result3;
                ViewBag.report4 = result4;
                ViewBag.report5 = result5;
                ViewBag.report6 = result6;
                return Json(JsonRequestBehavior.AllowGet);

            }
            else
            {
                Session["army"] = Army_No;
                var army = Army_No;
                var result1 = (dta.GetSanikdetail(army)).ToList();
                var result2 = (dta.GetSanikaward(army)).ToList();
                var result3 = (dta.GetSanikcomp(army)).ToList();
                var result4 = (dta.GetSanikdep(army)).ToList();
                var result5 = (dta.GetSanikloan(army)).ToList();
                var result6 = (dta.GetSanikcourtcase(army)).ToList();
                ViewBag.report1 = result1;
                ViewBag.report2 = result2;
                ViewBag.report3 = result3;
                ViewBag.report4 = result4;
                ViewBag.report5 = result5;
                ViewBag.report6 = result6;
                return Json(JsonRequestBehavior.AllowGet);
            }



        }
        public ActionResult ReturnArmyReport()
        {
            Generalform mydata = new Generalform();

            List<Generalform> cd = new List<Generalform>();

            if (Session["dcode"] != null && (string)Session["usertype"] == "72" || (string)Session["usertype"] == "70")
            {
                var dcode = (string)Session["dcode"];
                var army = (string)Session["army"];
                var result1 = (dta.GetSanikdetail1(army, dcode)).ToList();
                if (result1.Count() == 0)
                {
                    Session["army1"] = null;
                    return View("error");

                }
                else
                {
                    Session["army1"] = result1.FirstOrDefault().Army_No;
                }

                var arm = (string)Session["army1"];
                var result2 = (dta.GetSanikaward(arm)).ToList();
                var result3 = (dta.GetSanikcomp(arm)).ToList();
                var result4 = (dta.GetSanikdep(arm)).ToList();
                var result5 = (dta.GetSanikloan(arm)).ToList();
                var result6 = (dta.GetSanikcourtcase(arm)).ToList();
                ViewBag.report1 = result1;
                ViewBag.report2 = result2;
                ViewBag.report3 = result3;
                ViewBag.report4 = result4;
                ViewBag.report5 = result5;
                ViewBag.report6 = result6;
            }
            else
            {

                var army = (string)Session["army"];
                var result1 = (dta.GetSanikdetail(army)).ToList();
                var result2 = (dta.GetSanikaward(army)).ToList();
                var result3 = (dta.GetSanikcomp(army)).ToList();
                var result4 = (dta.GetSanikdep(army)).ToList();
                var result5 = (dta.GetSanikloan(army)).ToList();
                var result6 = (dta.GetSanikcourtcase(army)).ToList();
                ViewBag.report1 = result1;
                ViewBag.report2 = result2;
                ViewBag.report3 = result3;
                ViewBag.report4 = result4;
                ViewBag.report5 = result5;
                ViewBag.report6 = result6;

            }


            return View("ArmyReport");

        }
        public ActionResult ExportPdfmain()
        {

            List<Generalform> cd = new List<Generalform>();
            List<Generalform> cd1 = new List<Generalform>();
            Generalform mydata = new Generalform();

            if (Session["dcode"] != null && (string)Session["usertype"] == "72" || (string)Session["usertype"] == "70")
            {
                string army;
                var dcode = (string)Session["dcode"];
                if (Session["aarmyno"] != null)
                {
                    army = (string)Session["aarmyno"];
                }
                else
                {
                    army = (string)Session["armynoalrd"];
                }


                var result1 = (dta.GetSanikdetail1(army, dcode)).ToList();
                if (result1.Count() == 0)
                {
                    return View("error");
                }
                else
                {
                    Session["army1"] = result1.FirstOrDefault().Army_No;
                    var arm = (string)Session["army1"];
                    var result2 = (dta.GetSanikaward(army)).ToList();
                    var result3 = (dta.GetSanikcomp(army)).ToList();
                    var result4 = (dta.GetSanikdep(army)).ToList();
                    var result5 = (dta.GetSanikloan(army)).ToList();
                    var result6 = (dta.GetSanikcourtcase(army)).ToList();

                    foreach (var item21 in result1)
                    {
                        if (item21.photo != null)
                        {
                            int date = DateTime.Now.Second;
                            Random rnd = new Random();
                            int rnbr = rnd.Next(1000, 150000);
                            string name = "sanik" + date + rnbr + ".jpg";

                            byte[] imageBytes = item21.photo.ToArray();

                            //// Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);


                            byte[] imageByte = Convert.FromBase64String(base64String);
                            MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length);

                            // Convert byte[] to Image
                            ms.Write(imageByte, 0, imageByte.Length);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                            image.Save(Server.MapPath("../imageReport/" + name), System.Drawing.Imaging.ImageFormat.Jpeg);
                            string imgsanik = "/imageReport/" + name;

                            mydata.pathsnk = Server.MapPath(imgsanik);



                        }
                        else
                        {

                            mydata.pathsnk = null;
                            mydata.Photo = null;
                        }


                        mydata.Army_No = item21.Army_No;
                        mydata.ESMIdentitycardnumber = item21.ESMIdentitycardnumber;
                        mydata.UID = item21.UID;
                        mydata.Citizen_ID = item21.Citizen_ID;
                        mydata.Sanik_Name_eng = item21.Sanik_Name_eng;
                        mydata.Sanik_Name_hindi = item21.Sanik_Name_hindi;
                        mydata.Father_Name_eng = item21.Father_Name_eng;
                        mydata.Father_Name_hindi = item21.Father_Name_hindi;
                        mydata.Mother_Name_eng = item21.Mother_Name_eng;
                        mydata.Mother_Name_hindi = item21.Mother_Name_hindi;
                        mydata.Gender = item21.Gender;
                        DateTime dobsanik = Convert.ToDateTime(item21.DOB);
                        mydata.dobsanik = dobsanik.ToShortDateString();
                        mydata.CategoryDesc = item21.CategoryDesc;
                        mydata.MaritalStatusCode = (item21.Marital_Status);
                        mydata.spousename = item21.spousename;
                        mydata.spousenamehindi = item21.spousenamehindi;
                        mydata.mobileno = item21.mobileno;
                        mydata.landline = item21.landline;
                        mydata.emialid = item21.emialid;
                        mydata.regt_CorpsType = (item21.regt_CorpsType);
                        mydata.Per_address_eng = item21.Per_address_eng;
                        mydata.Per_address_hindi = item21.Per_address_hindi;
                        mydata.Per_Landmark_english = item21.Per_Landmark_english;
                        mydata.Per_Landmark_Hindi = item21.Per_Landmark_Hindi;
                        mydata.StateName = item21.StateName;
                        mydata.dname = item21.dname;
                        mydata.townname = item21.townname;
                        mydata.VNAME = item21.VNAME;
                        mydata.townname = item21.townname;
                        mydata.Urban_rural = Convert.ToString(item21.Urban_rural);
                        mydata.perchk = item21.perchk;
                        mydata.Per_cors = (item21.Per_cors).ToString();
                        mydata.Pin_code = item21.Pin_code;
                        mydata.Cors_address = item21.Cors_address;
                        mydata.Amount_OF_Rent = item21.Amount_OF_Rent;
                        mydata.Annual_income = item21.Annual_income;
                        mydata.Annual_Budget_for_Maintenance = item21.Annual_Budget_for_Maintenance;
                        mydata.Character_Type = (item21.Character_Type);
                        mydata.MedicalCat_Type = (item21.MedicalCat_Type);
                        mydata.Rank_Type = item21.Rank_Type;
                        DateTime domretiremnt = Convert.ToDateTime(item21.RetirementDate);
                        mydata.domretiremnt = domretiremnt.ToShortDateString();
                        mydata.Force_Cat_Name = item21.Force_Cat_Name;
                        mydata.Force_Name = item21.Force_Name;
                        mydata.Bank_Acc_no = item21.Bank_Acc_no;
                        mydata.Bank_IFSC = item21.Bank_IFSC;
                        mydata.BankName = item21.BankName;


                    }
                    cd1.Add(mydata);
                    mydata = new Generalform();
                    foreach (var item in result2)
                    {
                        mydata.awardName = item.awardName;
                        DateTime domaward = Convert.ToDateTime(item.award_date);
                        mydata.domaward = domaward.ToShortDateString();

                        mydata.Perpose = item.Perpose;
                        cd.Add(mydata);
                        mydata = new Generalform();
                    }
                    foreach (var item31 in result3)
                    {
                        mydata.Name_of_Complain = item31.Name_of_Complain;
                        mydata.Level_of_decision = item31.Level_of_decision;
                        DateTime dobcmp = Convert.ToDateTime(item31.Date_of_Complain);
                        mydata.dobcmp = dobcmp.ToShortDateString();

                        mydata.Pending_With = item31.Pending_With;
                        mydata.Decision_Given = item31.Decision_Given;
                        cd.Add(mydata);
                        mydata = new Generalform();

                    }

                    foreach (var item41 in result4)
                    {
                        int date = DateTime.Now.Second;
                        Random rnd = new Random();
                        int rnbr = rnd.Next(1000, 150000);
                        string name = "dept" + date + rnbr + ".jpg";
                        if (item41.imagedept != null)
                        {
                            // mydata.imagedept = (item.imagedept).ToArray();
                            byte[] imageBytes = item41.imagedept.ToArray();

                            //// Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);


                            byte[] imageByte = Convert.FromBase64String(base64String);
                            MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length);

                            // Convert byte[] to Image
                            ms.Write(imageByte, 0, imageByte.Length);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                            image.Save(Server.MapPath("../imageReport/" + name), System.Drawing.Imaging.ImageFormat.Jpeg);
                            string imgdep = "/imageReport/" + name;

                            mydata.pathdep = Server.MapPath(imgdep);



                        }
                        else
                        {
                            mydata.pathdep = null;

                            mydata.imagedept = null;
                        }
                        mydata.Dependent_Name = item41.Dependent_Name;
                        mydata.RelationDesc = item41.RelationDesc;
                        DateTime dobdept = Convert.ToDateTime(item41.DOB);
                        mydata.dobdept = dobdept.ToShortDateString();

                        mydata.UID = item41.UID;
                        mydata.MaritalStatusDesc = item41.Marital_Status;
                        DateTime domdept = Convert.ToDateTime(item41.DOM);
                        mydata.domdept = domdept.ToShortDateString();
                        cd.Add(mydata);
                        mydata = new Generalform();

                    }
                    foreach (var item23 in result5)
                    {
                        mydata.Loan_Amount = item23.Loan_Amount;
                        mydata.Purpose = item23.Purpose;
                        mydata.Outstanding_Amount = item23.Outstanding_Amount;
                        DateTime domloan = Convert.ToDateTime(item23.Date_loan);
                        mydata.domloan = domloan.ToShortDateString();
                        cd.Add(mydata);
                        mydata = new Generalform();

                    }
                    foreach (var item12 in result6)
                    {
                        mydata.Case_No = item12.Case_No;
                        mydata.Case_Year = item12.Case_Year;
                        mydata.Court_Name = item12.Court_Name;
                        mydata.Decision = item12.Decision;
                        cd.Add(mydata);
                        mydata = new Generalform();
                    }
                }






            }
            else
            {

                var army = (string)Session["armynoalrd"];
                var result1 = (dta.GetSanikdetail(army)).ToList();
                if (result1.Count() == 0)
                {
                    return View("error");
                }
                else
                {
                    var result2 = (dta.GetSanikaward(army)).ToList();
                    var result3 = (dta.GetSanikcomp(army)).ToList();
                    var result4 = (dta.GetSanikdep(army)).ToList();
                    var result5 = (dta.GetSanikloan(army)).ToList();
                    var result6 = (dta.GetSanikcourtcase(army)).ToList();


                    foreach (var item21 in result1)
                    {
                        if (item21.photo != null)
                        {

                            int date = DateTime.Now.Second;
                            Random rnd = new Random();
                            int rnbr = rnd.Next(1000, 150000);
                            string name = "sanik" + date + rnbr + ".jpg";
                            byte[] imageBytes = item21.photo.ToArray();

                            //// Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);


                            byte[] imageByte = Convert.FromBase64String(base64String);
                            MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length);

                            // Convert byte[] to Image
                            ms.Write(imageByte, 0, imageByte.Length);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                            image.Save(Server.MapPath("../imageReport/" + name), System.Drawing.Imaging.ImageFormat.Jpeg);
                            string imgsanik = "/imageReport/" + name;

                            mydata.pathsnk = Server.MapPath(imgsanik);



                        }
                        else
                        {

                            mydata.pathsnk = null;
                            mydata.Photo = null;
                        }


                        mydata.Army_No = item21.Army_No;
                        mydata.ESMIdentitycardnumber = item21.ESMIdentitycardnumber;
                        mydata.UID = item21.UID;
                        mydata.Citizen_ID = item21.Citizen_ID;
                        mydata.Sanik_Name_eng = item21.Sanik_Name_eng;
                        mydata.Sanik_Name_hindi = item21.Sanik_Name_hindi;
                        mydata.Father_Name_eng = item21.Father_Name_eng;
                        mydata.Father_Name_hindi = item21.Father_Name_hindi;
                        mydata.Mother_Name_eng = item21.Mother_Name_eng;
                        mydata.Mother_Name_hindi = item21.Mother_Name_hindi;
                        mydata.Gender = item21.Gender;
                        DateTime dobsanik = Convert.ToDateTime(item21.DOB);
                        mydata.dobsanik = dobsanik.ToShortDateString();
                        if (mydata.dobsanik != null && mydata.dobsanik != "01-01-0001")
                        {
                            mydata.dobsanik = mydata.dobsanik;

                        }
                        else
                        {
                            mydata.dobsanik = null;
                        }
                        mydata.CategoryDesc = item21.CategoryDesc;
                        mydata.MaritalStatusCode = (item21.Marital_Status);
                        mydata.spousename = item21.spousename;
                        mydata.spousenamehindi = item21.spousenamehindi;
                        mydata.mobileno = item21.mobileno;
                        mydata.landline = item21.landline;
                        mydata.emialid = item21.emialid;
                        mydata.regt_CorpsType = (item21.regt_CorpsType);
                        mydata.Per_address_eng = item21.Per_address_eng;
                        mydata.Per_address_hindi = item21.Per_address_hindi;
                        mydata.Per_Landmark_english = item21.Per_Landmark_english;
                        mydata.Per_Landmark_Hindi = item21.Per_Landmark_Hindi;
                        mydata.StateName = item21.StateName;
                        mydata.dname = item21.dname;
                        mydata.townname = item21.townname;
                        mydata.VNAME = item21.VNAME;
                        mydata.townname = item21.townname;
                        mydata.Urban_rural = Convert.ToString(item21.Urban_rural);
                        mydata.perchk = item21.perchk;
                        mydata.Per_cors = (item21.Per_cors).ToString();
                        mydata.Pin_code = item21.Pin_code;
                        mydata.Cors_address = item21.Cors_address;
                        mydata.Amount_OF_Rent = item21.Amount_OF_Rent;
                        mydata.Annual_income = item21.Annual_income;
                        mydata.Annual_Budget_for_Maintenance = item21.Annual_Budget_for_Maintenance;
                        mydata.Character_Type = (item21.Character_Type);
                        mydata.MedicalCat_Type = (item21.MedicalCat_Type);
                        mydata.Rank_Type = item21.Rank_Type;
                        DateTime domretiremnt = Convert.ToDateTime(item21.RetirementDate);
                        mydata.domretiremnt = domretiremnt.ToShortDateString();
                        if (mydata.domretiremnt != null && mydata.domretiremnt != "01-01-0001")
                        {
                            mydata.domretiremnt = mydata.domretiremnt;

                        }
                        else
                        {
                            mydata.domretiremnt = null;
                        }
                        mydata.Force_Cat_Name = item21.Force_Cat_Name;
                        mydata.Force_Name = item21.Force_Name;
                        mydata.Bank_Acc_no = item21.Bank_Acc_no;
                        mydata.Bank_IFSC = item21.Bank_IFSC;
                        mydata.BankName = item21.BankName;


                    }
                    cd1.Add(mydata);
                    mydata = new Generalform();
                    foreach (var item12 in result2)
                    {
                        mydata.awardName = item12.awardName;
                        DateTime domaward = Convert.ToDateTime(item12.award_date);
                        mydata.domaward = domaward.ToShortDateString();
                        if (mydata.domaward != null && mydata.domaward != "01-01-0001")
                        {
                            mydata.domaward = mydata.domaward;

                        }
                        else
                        {
                            mydata.domaward = null;
                        }
                        mydata.Perpose = item12.Perpose;
                        cd.Add(mydata);
                        mydata = new Generalform();
                    }
                    foreach (var item13 in result3)
                    {
                        mydata.Name_of_Complain = item13.Name_of_Complain;
                        mydata.Level_of_decision = item13.Level_of_decision;
                        DateTime dobcmp = Convert.ToDateTime(item13.Date_of_Complain);
                        mydata.dobcmp = dobcmp.ToShortDateString();
                        if (mydata.dobcmp != null && mydata.dobcmp != "01-01-0001")
                        {
                            mydata.dobcmp = mydata.domaward;

                        }
                        else
                        {
                            mydata.dobcmp = null;
                        }
                        mydata.Pending_With = item13.Pending_With;
                        mydata.Decision_Given = item13.Decision_Given;
                        cd.Add(mydata);
                        mydata = new Generalform();

                    }
                    foreach (var item14 in result4)
                    {
                        if (item14.imagedept != null)
                        {
                            int date = DateTime.Now.Second;
                            Random rnd = new Random();
                            int rnbr = rnd.Next(1000, 150000);
                            string name = "dept" + date + rnbr + ".jpg";
                            // mydata.imagedept = (item.imagedept).ToArray();
                            byte[] imageBytes = item14.imagedept.ToArray();

                            //// Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);


                            byte[] imageByte = Convert.FromBase64String(base64String);
                            MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length);

                            // Convert byte[] to Image
                            ms.Write(imageByte, 0, imageByte.Length);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                            image.Save(Server.MapPath("../imageReport/" + name), System.Drawing.Imaging.ImageFormat.Jpeg);
                            string imgdep = "/imageReport/" + name;

                            mydata.pathdep = Server.MapPath(imgdep);



                        }
                        else
                        {
                            mydata.pathdep = null;

                            mydata.imagedept = null;
                        }
                        mydata.Dependent_Name = item14.Dependent_Name;
                        mydata.RelationDesc = item14.RelationDesc;
                        DateTime dobdept = Convert.ToDateTime(item14.DOB);
                        mydata.dobdept = dobdept.ToShortDateString();
                        if (mydata.dobdept != null && mydata.dobdept != "01-01-0001")
                        {
                            mydata.dobdept = mydata.domaward;

                        }
                        else
                        {
                            mydata.dobdept = null;
                        }
                        mydata.UID = item14.UID;
                        mydata.MaritalStatusDesc = item14.Marital_Status;
                        DateTime domdept = Convert.ToDateTime(item14.DOM);
                        mydata.domdept = domdept.ToShortDateString();
                        if (mydata.domdept != null && mydata.domdept != "01-01-0001")
                        {
                            mydata.domdept = mydata.domdept;

                        }
                        else
                        {
                            mydata.domdept = null;
                        }
                        cd.Add(mydata);
                        mydata = new Generalform();

                    }
                    foreach (var item15 in result5)
                    {
                        mydata.Loan_Amount = item15.Loan_Amount;
                        mydata.Purpose = item15.Purpose;
                        mydata.Outstanding_Amount = item15.Outstanding_Amount;
                        DateTime domloan = Convert.ToDateTime(item15.Date_loan);
                        mydata.domloan = domloan.ToShortDateString();
                        if (mydata.domloan != null && mydata.domloan != "01-01-0001")
                        {
                            mydata.domloan = mydata.domloan;

                        }
                        else
                        {
                            mydata.domloan = null;
                        }
                        cd.Add(mydata);
                        mydata = new Generalform();

                    }
                    foreach (var item16 in result6)
                    {
                        mydata.Case_No = item16.Case_No;
                        mydata.Case_Year = item16.Case_Year;
                        mydata.Court_Name = item16.Court_Name;
                        mydata.Decision = item16.Decision;
                        cd.Add(mydata);
                        mydata = new Generalform();
                    }




                }

            }

            mydata.pdf2 = cd1;
            mydata.pdf = cd;
            string url = "/Content/rsb-header.png";
            mydata.ImageUrl = Server.MapPath(url);
            string deftimg = "/Content/Rajya_Sanik_Board/img/LinkedInNoPic.png";
            mydata.defaultimage = Server.MapPath(deftimg);

            return this.ViewPdf("", "ArmyReportprint", mydata);



        }
        public ActionResult ExportPdf()
        {
            List<Generalform> cd = new List<Generalform>();
            List<Generalform> cd1 = new List<Generalform>();
            var army = (string)Session["army"];
            var result1 = (dta.GetSanikdetail(army)).ToList();
            var result2 = (dta.GetSanikaward(army)).ToList();
            var result3 = (dta.GetSanikcomp(army)).ToList();
            var result4 = (dta.GetSanikdep(army)).ToList();
            var result5 = (dta.GetSanikloan(army)).ToList();
            var result6 = (dta.GetSanikcourtcase(army)).ToList();

            Generalform mydata = new Generalform();

            foreach (var item in result1)
            {
                if (item.photo != null)
                {

                    int date = DateTime.Now.Second;
                    Random rnd = new Random();
                    int rnbr = rnd.Next(1000, 150000);
                    string name = "sanik" + date + rnbr + ".jpg";
                    byte[] imageBytes = item.photo.ToArray();

                    //// Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);


                    byte[] imageByte = Convert.FromBase64String(base64String);
                    MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length);

                    // Convert byte[] to Image
                    ms.Write(imageByte, 0, imageByte.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    image.Save(Server.MapPath("../imageReport/" + name), System.Drawing.Imaging.ImageFormat.Jpeg);
                    string imgsanik = "/imageReport/" + name;

                    mydata.pathsnk = Server.MapPath(imgsanik);



                }
                else
                {

                    mydata.pathsnk = null;
                    mydata.Photo = null;
                }


                mydata.Army_No = item.Army_No;
                mydata.ESMIdentitycardnumber = item.ESMIdentitycardnumber;
                mydata.UID = item.UID;
                mydata.Citizen_ID = item.Citizen_ID;
                mydata.Sanik_Name_eng = item.Sanik_Name_eng;
                mydata.Sanik_Name_hindi = item.Sanik_Name_hindi;
                mydata.Father_Name_eng = item.Father_Name_eng;
                mydata.Father_Name_hindi = item.Father_Name_hindi;
                mydata.Mother_Name_eng = item.Mother_Name_eng;
                mydata.Mother_Name_hindi = item.Mother_Name_hindi;
                mydata.Gender = item.Gender;
                DateTime dobsanik = Convert.ToDateTime(item.DOB);
                mydata.dobsanik = dobsanik.ToShortDateString();
                if (mydata.dobsanik != null && mydata.dobsanik != "01-01-0001")
                {
                    mydata.dobsanik = mydata.dobsanik;

                }
                else
                {
                    mydata.dobsanik = null;
                }
                mydata.CategoryDesc = item.CategoryDesc;
                mydata.MaritalStatusCode = (item.Marital_Status);
                mydata.spousename = item.spousename;
                mydata.spousenamehindi = item.spousenamehindi;
                mydata.mobileno = item.mobileno;
                mydata.landline = item.landline;
                mydata.emialid = item.emialid;
                mydata.regt_CorpsType = (item.regt_CorpsType);
                mydata.Per_address_eng = item.Per_address_eng;
                mydata.Per_address_hindi = item.Per_address_hindi;
                mydata.Per_Landmark_english = item.Per_Landmark_english;
                mydata.Per_Landmark_Hindi = item.Per_Landmark_Hindi;
                mydata.StateName = item.StateName;
                mydata.dname = item.dname;
                mydata.townname = item.townname;
                mydata.VNAME = item.VNAME;
                mydata.townname = item.townname;
                mydata.Urban_rural = Convert.ToString(item.Urban_rural);
                mydata.perchk = item.perchk;
                mydata.Per_cors = (item.Per_cors).ToString();
                mydata.Pin_code = item.Pin_code;
                mydata.Cors_address = item.Cors_address;
                mydata.Amount_OF_Rent = item.Amount_OF_Rent;
                mydata.Annual_income = item.Annual_income;
                mydata.Annual_Budget_for_Maintenance = item.Annual_Budget_for_Maintenance;
                mydata.Character_Type = (item.Character_Type);
                mydata.MedicalCat_Type = (item.MedicalCat_Type);
                mydata.Rank_Type = item.Rank_Type;
                DateTime domretiremnt = Convert.ToDateTime(item.RetirementDate);
                mydata.domretiremnt = domretiremnt.ToShortDateString();
                if (mydata.domretiremnt != null && mydata.domretiremnt != "01-01-0001")
                {
                    mydata.domretiremnt = mydata.domretiremnt;

                }
                else
                {
                    mydata.domretiremnt = null;
                }
                mydata.Force_Cat_Name = item.Force_Cat_Name;
                mydata.Force_Name = item.Force_Name;
                mydata.Bank_Acc_no = item.Bank_Acc_no;
                mydata.Bank_IFSC = item.Bank_IFSC;
                mydata.BankName = item.BankName;


            }
            cd1.Add(mydata);
            mydata = new Generalform();
            foreach (var item in result2)
            {
                mydata.awardName = item.awardName;
                DateTime domaward = Convert.ToDateTime(item.award_date);
                mydata.domaward = domaward.ToShortDateString();
                if (mydata.domaward != null && mydata.domaward != "01-01-0001")
                {
                    mydata.domaward = mydata.domaward;

                }
                else
                {
                    mydata.domaward = null;
                }
                mydata.Perpose = item.Perpose;
                cd.Add(mydata);
                mydata = new Generalform();
            }
            foreach (var item in result3)
            {
                mydata.Name_of_Complain = item.Name_of_Complain;
                mydata.Level_of_decision = item.Level_of_decision;
                DateTime dobcmp = Convert.ToDateTime(item.Date_of_Complain);
                mydata.dobcmp = dobcmp.ToShortDateString();
                if (mydata.dobcmp != null && mydata.dobcmp != "01-01-0001")
                {
                    mydata.dobcmp = mydata.dobcmp;

                }
                else
                {
                    mydata.dobcmp = null;
                }
                mydata.Pending_With = item.Pending_With;
                mydata.Decision_Given = item.Decision_Given;
                cd.Add(mydata);
                mydata = new Generalform();

            }
            foreach (var item in result4)
            {
                if (item.imagedept != null)
                {
                    int date = DateTime.Now.Second;
                    Random rnd = new Random();
                    int rnbr = rnd.Next(1000, 150000);
                    string name = "dept" + date + rnbr + ".jpg";
                    // mydata.imagedept = (item.imagedept).ToArray();
                    byte[] imageBytes = item.imagedept.ToArray();

                    //// Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);


                    byte[] imageByte = Convert.FromBase64String(base64String);
                    MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length);

                    // Convert byte[] to Image
                    ms.Write(imageByte, 0, imageByte.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    image.Save(Server.MapPath("../imageReport/" + name), System.Drawing.Imaging.ImageFormat.Jpeg);
                    string imgdep = "/imageReport/" + name;

                    mydata.pathdep = Server.MapPath(imgdep);



                }
                else
                {
                    mydata.pathdep = null;

                    mydata.imagedept = null;
                }
                mydata.Dependent_Name = item.Dependent_Name;
                mydata.RelationDesc = item.RelationDesc;
                DateTime dobdept = Convert.ToDateTime(item.DOB);
                mydata.dobdept = dobdept.ToShortDateString();
                if (mydata.dobdept != null && mydata.dobdept != "01-01-0001")
                {
                    mydata.dobdept = mydata.dobdept;

                }
                else
                {
                    mydata.dobdept = null;
                }
                mydata.UID = item.UID;
                mydata.MaritalStatusDesc = item.Marital_Status;
                DateTime domdept = Convert.ToDateTime(item.DOM);
                mydata.domdept = domdept.ToShortDateString();
                if (mydata.domdept != null && mydata.domdept != "01-01-0001")
                {
                    mydata.domdept = mydata.domdept;

                }
                else
                {
                    mydata.domdept = null;
                }
                cd.Add(mydata);
                mydata = new Generalform();

            }
            foreach (var item in result5)
            {
                mydata.Loan_Amount = item.Loan_Amount;
                mydata.Purpose = item.Purpose;
                mydata.Outstanding_Amount = item.Outstanding_Amount;
                DateTime domloan = Convert.ToDateTime(item.Date_loan);
                mydata.domloan = domloan.ToShortDateString();
                if (mydata.domloan != null && mydata.domloan != "01-01-0001")
                {
                    mydata.domloan = mydata.domloan;

                }
                else
                {
                    mydata.domloan = null;
                }
                cd.Add(mydata);
                mydata = new Generalform();

            }
            foreach (var item in result6)
            {
                mydata.Case_No = item.Case_No;
                mydata.Case_Year = item.Case_Year;
                mydata.Court_Name = item.Court_Name;
                mydata.Decision = item.Decision;
                cd.Add(mydata);
                mydata = new Generalform();
            }

            mydata.pdf2 = cd1;
            mydata.pdf = cd;
            string url = "/Content/rsb-header.png";
            mydata.ImageUrl = Server.MapPath(url);
            string deftimg = "/Content/Rajya_Sanik_Board/img/LinkedInNoPic.png";
            mydata.defaultimage = Server.MapPath(deftimg);

            return this.ViewPdf("", "ArmyReportprint", mydata);


        }


        //District
        public ActionResult DistrictwiseReport()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                var st = (from stat in ed.tblDistrictMasters where stat.statecode == "06" select stat).OrderBy(x => x.dname);
                SelectList list5 = new SelectList(st, "dcode", "dname");
                ViewBag.StateList = list5;

                return View("DistrictwiseReport");
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("DistrictwiseReport");


        }



        public JsonResult DistrictwiseReport1(string dcode, string sdate, string edate)
        {

            DateTime ValidDate1, ValidDate2, sdate1, edate1;
            string ddcode = (string)Session["dcode"];
            if (ddcode != "notdcode")
            {
                bool sdte, edte;

                if (DateTime.TryParseExact(sdate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
                {
                    sdate1 = ValidDate1;
                }
                else
                {
                    sdate1 = Convert.ToDateTime("01/01/0001");
                }
                if (DateTime.TryParseExact(edate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate2))
                {
                    edate1 = ValidDate2;
                }
                else
                {
                    edate1 = Convert.ToDateTime("01/01/0001");
                }
                //sdte = checkdate(sdate);
                //edte = checkdate(edate);
                //if (sdte == true && edte == true)
                //{

                Session["sdate"] = sdate1;
                Session["edate"] = edate1;
                var result = (dta.Districtwise(ddcode.Trim(), sdate1, edate1)).ToList();

                ViewBag.report = result;
                //    return Json(JsonRequestBehavior.AllowGet);
                //}
                //else
                //{

                //    string res = "a";
                //    return Json(res, ddcode, JsonRequestBehavior.AllowGet);
                //}
                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                var dcd = dcode;
                Session["dcd"] = dcode.Trim();
                bool sdte, edte;
                DateTime ValidDate3, ValidDate4, sdate2, edate2;
                if (DateTime.TryParseExact(sdate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate3))
                {
                    sdate2 = ValidDate3;
                }
                else
                {
                    sdate2 = Convert.ToDateTime("01/01/0001");
                }
                if (DateTime.TryParseExact(edate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate4))
                {
                    edate2 = ValidDate4;
                }
                else
                {
                    edate2 = Convert.ToDateTime("01/01/0001");
                }
                //sdte = checkdate(Convert.ToDateTime(sdate).ToString("dd/mm/yyyy"));
                //edte = checkdate(edate);
                //if (sdte == true && edte == true)
                //{
                Session["sdate"] = sdate2;
                Session["edate"] = edate2;
                var result = (dta.Districtwise(dcd.Trim(), sdate2, edate2).ToList());
                ViewBag.report = result;
                return Json(JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    string res = "a";
                //    return Json(res, JsonRequestBehavior.AllowGet);
                //}

            }









        }
        public ActionResult ReturnDistrictwiseReport()
        {
            try
            {
                string ddcode = (string)Session["dcode"];

                if (ddcode != "notdcode")
                {

                    DateTime sdate = Convert.ToDateTime(Session["sdate"].ToString());
                    DateTime edate = Convert.ToDateTime(Session["edate"].ToString());
                    var result = (dta.Districtwise(ddcode, sdate, edate)).ToList();
                    if (result.Count() == 0)
                    {
                        return View("error");
                    }
                    else
                    {
                        ViewBag.report = result;

                    }

                }
                else
                {
                    var dcd = (string)Session["dcd"];
                    DateTime sdate = Convert.ToDateTime(Session["sdate"].ToString());
                    DateTime edate = Convert.ToDateTime(Session["edate"].ToString());
                    var result = (dta.Districtwise(dcd, sdate, edate)).ToList();
                    if (result.Count() == 0)
                    {
                        return View("error");
                    }
                    else
                    {
                        ViewBag.report = result;

                    }
                }


                return View("DistrictReport");
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }

        public ActionResult ExportPdfdist()
        {
            List<Generalform> cd = new List<Generalform>();
            Generalform mydata = new Generalform();
            string ddcode = (string)Session["dcode"];
            if (ddcode != "notdcode")
            {

                var sdate = (string)Session["sdate"];
                var edate = (string)Session["edate"];
                var result1 = (dta.Districtwise(ddcode, Convert.ToDateTime(sdate), Convert.ToDateTime(edate))).ToList();


                foreach (var item in result1)
                {


                    mydata.Army_No = item.Army_No;
                    mydata.ESMIdentitycardnumber = item.ESMIdentitycardnumber;

                    mydata.Citizen_ID = item.Citizen_ID;
                    mydata.Sanik_Name_eng = item.Sanik_Name_eng;

                    mydata.Father_Name_eng = item.Father_Name_eng;
                    mydata.Gender = item.Gender;
                    DateTime dobcmp = Convert.ToDateTime(item.DOB);
                    mydata.dobsanik = dobcmp.ToShortDateString();
                    if (mydata.dobsanik != null && mydata.dobsanik != "01-01-0001")
                    {
                        mydata.dobsanik = mydata.dobsanik;

                    }
                    else
                    {
                        mydata.dobsanik = null;
                    }

                    mydata.MaritalStatusDesc = (item.Marital_Status);
                    mydata.spousename = item.spousename;


                    mydata.regt_CorpsType = (item.regt_CorpsType);
                    mydata.Per_address_eng = item.Per_address_eng;

                    mydata.Per_Landmark_english = item.Per_Landmark_english;

                    mydata.StateName = item.StateName;

                    mydata.townname = item.townname;
                    mydata.VNAME = item.VNAME;
                    mydata.townname = item.townname;

                    mydata.Pin_code = item.Pin_code;

                    mydata.Rank_Type = item.Rank_Type;


                    mydata.Force_Cat_Name = item.Force_Cat_Name;
                    mydata.Force_Name = item.Force_Name;
                    cd.Add(mydata);
                    mydata = new Generalform();
                }


            }
            else
            {
                var dcd = (string)Session["dcd"];
                var sdate = (string)Session["sdate"];
                var edate = (string)Session["edate"];
                var result1 = (dta.Districtwise(dcd, Convert.ToDateTime(sdate), Convert.ToDateTime(edate))).ToList();


                foreach (var item in result1)
                {


                    mydata.Army_No = item.Army_No;
                    mydata.ESMIdentitycardnumber = item.ESMIdentitycardnumber;

                    mydata.Citizen_ID = item.Citizen_ID;
                    mydata.Sanik_Name_eng = item.Sanik_Name_eng;

                    mydata.Father_Name_eng = item.Father_Name_eng;
                    mydata.Gender = item.Gender;
                    DateTime dobcmp = Convert.ToDateTime(item.DOB);
                    mydata.dobsanik = dobcmp.ToShortDateString();
                    if (mydata.dobsanik != null && mydata.dobsanik != "01-01-0001")
                    {
                        mydata.dobsanik = mydata.dobsanik;

                    }
                    else
                    {
                        mydata.dobsanik = null;
                    }

                    mydata.MaritalStatusDesc = (item.Marital_Status);
                    mydata.spousename = item.spousename;


                    mydata.regt_CorpsType = (item.regt_CorpsType);
                    mydata.Per_address_eng = item.Per_address_eng;

                    mydata.Per_Landmark_english = item.Per_Landmark_english;

                    mydata.StateName = item.StateName;

                    mydata.townname = item.townname;
                    mydata.VNAME = item.VNAME;
                    mydata.townname = item.townname;

                    mydata.Pin_code = item.Pin_code;

                    mydata.Rank_Type = item.Rank_Type;


                    mydata.Force_Cat_Name = item.Force_Cat_Name;
                    mydata.Force_Name = item.Force_Name;
                    cd.Add(mydata);
                    mydata = new Generalform();
                }



            }

            mydata.pdf1 = cd;
            string url = "/Content/rsb-header.png";
            mydata.ImageUrl = Server.MapPath(url);
            string deftimg = "/Content/Rajya_Sanik_Board/img/LinkedInNoPic.png";
            mydata.defaultimage = Server.MapPath(deftimg);

            return this.ViewPdf("", "ArmydistReportprint", mydata);


        }
        [HttpGet]
        public ActionResult error()
        {
            return View();
        }

        //captcha

        public ActionResult CaptchaImage(string prefix, bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question 
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            var captcha = string.Format("{0} + {1} = ?", a, b);

            //store answer 
            Session["Captcha"] = a + b;

            //image stream 
            FileContentResult img = null;

            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage((System.Drawing.Image)bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));

                //add noise 
                if (noisy)
                {
                    int i, r, x, y;
                    var pen = new Pen(Color.Yellow);
                    for (i = 1; i < 10; i++)
                    {
                        pen.Color = Color.FromArgb(
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)));

                        r = rand.Next(0, (130 / 3));
                        x = rand.Next(0, 130);
                        y = rand.Next(0, 30);

                        // gfx.DrawEllipse(pen, x–r,y – r, r, r); 
                        gfx.DrawEllipse(pen, x + r, y + r, r, r);
                    }
                }

                //add question 
                gfx.DrawString(captcha, new System.Drawing.Font("Tahoma", 15), Brushes.Gray, 2, 3);

                //render as Jpeg 
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                img = this.File(mem.GetBuffer(), "image/Jpeg");
            }

            return img;
        }


        public string getcidr(string statecode, string dcode)
        {
            int increm = 0;
            string Citizen_ID="";

            int maxlen = 0;
            string LastSrno = string.Empty;

            string LastTwoDigi = string.Empty;
            tblCitizenSrno objSrno = new tblCitizenSrno();
            if (statecode == "06")
            {
                LastSrno = Convert.ToString((from srn in ed.tblCitizenSrnos where (srn.scode == "HA" && srn.dcode == dcode) select srn.lastsrno).Max());
            }

            int chk = Convert.ToInt32(LastSrno);
            if (chk == 0)
            {
                if (statecode == "06")
                {
                    Citizen_ID = "HA" + mydata.dcode + "00000001";
                }

                objSrno.dcode = mydata.dcode;
                objSrno.scode = statecode;
                objSrno.lastsrno = "01";
                ed.tblCitizenSrnos.InsertOnSubmit(objSrno);
                ed.SubmitChanges();
            }
            else
            {
                if (chk < 10)
                {
                    increm = chk + 1;
                    if (statecode == "06")
                    {
                        Citizen_ID = "HA" + mydata.dcode + "0000000" + Convert.ToString(increm);
                    }

                }
                if (chk >= 10 && chk <= 98)
                {
                    increm = chk + 1;
                    if (statecode == "06")
                    {
                        Citizen_ID = "HA" + mydata.dcode + "000000" + Convert.ToString(increm);
                    }

                }
                if (chk >= 99 && chk <= 998)
                {
                    increm = chk + 1;
                    if (statecode == "06")
                    {
                        Citizen_ID = "HA" + mydata.dcode + "00000" + Convert.ToString(increm);
                    }

                }
                if (chk >= 999 && chk <= 9998)
                {
                    increm = chk + 1;
                    if (statecode == "06")
                    {
                        Citizen_ID = "HA" + mydata.dcode + "0000" + Convert.ToString(increm);
                    }

                }

                if (chk >= 9999 && chk <= 99998)
                {
                    increm = chk + 1;
                    if (statecode == "06")
                    {
                        Citizen_ID = "HA" + mydata.dcode + "000" + Convert.ToString(increm);
                    }

                }
                if (chk >= 99999 && chk <= 999998)
                {
                    increm = chk + 1;
                    if (statecode == "06")
                    {
                        Citizen_ID = "HA" + mydata.dcode + "00" + Convert.ToString(increm);
                    }

                }
                if (chk >= 999999 && chk <= 9999998)
                {
                    increm = chk + 1;
                    if (statecode == "06")
                    {
                        Citizen_ID = "HA" + mydata.dcode + "0" + Convert.ToString(increm);
                    }

                }
                if (chk >= 9999999 && chk <= 99999998)
                {
                    increm = chk + 1;
                    if (statecode == "06")
                    {
                        Citizen_ID = "HA" + mydata.dcode + Convert.ToString(increm);
                    }

                }
                if (LastSrno.Trim().Length > 0)
                {
                    maxlen = LastSrno.Trim().Length;
                }
               
                LastTwoDigi = Convert.ToString(Citizen_ID.Substring(Citizen_ID.Length - maxlen));  // can be more than 2

                var query1 = from rec in ed.tblCitizenSrnos
                             where (rec.scode == "HA") && (rec.dcode == mydata.dcode)
                             select rec;

                foreach (tblCitizenSrno rec in query1)
                {
                    rec.lastsrno = LastTwoDigi;
                }
              
            }
            return Citizen_ID;
        }
        //*****************for citizen regestration********************//
        public String DecryptKey(string textToDecrypt)
        {
            string strToreturn = "no";
            //var urlQueryString = Request.QueryString["clientID"];
           // textToDecrypt = "y6HR8FkBW3fv4H2Y7yGM1ZKftnOSj2k5dwABCS88VJY=";
            //textToDecrypt = urlQueryString;
            string Key = "eDi$Sm76%)";
            byte[] sss = HttpServerUtility.UrlTokenDecode(textToDecrypt);
             textToDecrypt = Encoding.UTF8.GetString(sss);

            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] pwdBytes = GetStringBytes(Key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            strToreturn = Encoding.UTF8.GetString(plainText);
            string[] strArray = strToreturn.Split(',');


            var _citizenEmail = strArray[0];
            var _citizenloginTime = strArray[1];
            var start = DateTime.Now;
            var oldDate = DateTime.Parse(_citizenloginTime);

            if (start.Subtract(oldDate) >= TimeSpan.FromMinutes(10))
            {
                _citizenEmail = "";
                return _citizenEmail;
            }

           return _citizenEmail;
        }
        public byte[] GetStringBytes(String UserKey)
        {
            byte[] buffer;
            try
            {
                int length = (int)UserKey.Length;
                buffer = new byte[length];
                buffer = Encoding.UTF8.GetBytes(UserKey.ToCharArray());
            }
            finally
            {
                //fileStream.Close();
            }
            return buffer;
        }

        //public ActionResult RSBGeneralInfrm_citizen(string id)
        //{
        //    if (id != null)
        //    {
        //        string str = DecryptKey(id);
        //        if (str != null)
        //        {
        //            Session["str"] = str;
        //            return View("RSBGeneralInfrm_citizen");
        //        }
        //        else
        //        {
        //            return Redirect("http://164.100.137.137/edistrictcitizen");
        //        }
        //    }
        //    else
        //    {
        //        return Redirect("http://164.100.137.137/edistrictcitizen");
        //    }
               

        //}
        [HttpGet]
        public ActionResult RSBGeneralInfrm_citizen()
        {
            string str = (string)Session["userid"];
            if (str != "")
            {

                maritalstatus();
                gnder();
                forcename();
                Relationship();
                regm();
                cast();
                state();


                rank();
                medical();
                character();
                banklist();
                awardlist();

                return View();
            }
            else
            {
                return Redirect("http://164.100.137.137/edistrictcitizen");
            }
            //string strPreviousPage = "";
            //if (Request.UrlReferrer != null)
            //{
            //    strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
            //    maritalstatus();
            //    gnder();
            //    forcename();
            //    Relationship();
            //    regm();
            //    cast();
            //    state();


            //    rank();
            //    medical();
            //    character();
            //    banklist();
            //    awardlist();

            //    return View();
            //}
            //if (strPreviousPage == "")
            //{

            //    Session["userid"] = null;
            //    return RedirectToAction("Login", "Home");
            //}

           // return View();

        }
        public JsonResult RSBGeneralInfrm1_citizen()
        {
            string user = Convert.ToString(Session["userid"]);
            string usertype = "0";
           
           // string dco = Convert.ToString(Session["dcode"]);
            return Json(new { user, usertype }, JsonRequestBehavior.AllowGet);



        }
        [HttpPost]

        public JsonResult Sanik_General_Registration_citzen(Generalform mydata)
        {
            string msg;
            bool check = false;
            string statecode = "06";
            DateTime datee = Convert.ToDateTime("01/01/2000");
            bool emailid, UID = false, mobile, landline, sanikname, father, spouse, mother, cid, amountrent, annualincm, anualbudget, dateofenrolment, disabilitypercent, pponumber, pancardnumber;
            if (mydata.emialid == "" || mydata.UID == "" || mydata.mobileno == "" || mydata.Sanik_Name_eng == "" || mydata.Father_Name_eng == "" || mydata.Mother_Name_eng == "" || mydata.Citizen_ID == "" || mydata.ESMIdentitycardnumber == "" || mydata.Gender_code == "")
            {
                string msgerror = "error";
                return Json(new { msgerror }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                emailid = IsValidEmail(mydata.emialid);
                if (mydata.UID == null)
                {
                    UID = true;
                }
                else
                {
                    var uid = mydata.UID;

                    UID = Verhoeff.validateVerhoeff(uid.ToString().Trim());

                }

                int mob = mydata.mobileno.Length;
                if (mob == 10)
                {
                    mobile = IsNumber(mydata.mobileno);
                }
                else
                {
                    string msgerror = "mob";
                    return Json(new { msgerror }, JsonRequestBehavior.AllowGet);
                }
                landline = IsNumber(mydata.landline);
                sanikname = IsName(mydata.Sanik_Name_eng);
                father = IsName(mydata.Father_Name_eng);
                spouse = IsName(mydata.spousename);
                mother = IsName(mydata.Mother_Name_eng);
                // dateofenrolment = checkdate(Convert.ToString(mydata.Date_of_Enrolment));

                disabilitypercent = Decimalchk(Convert.ToString(mydata.Disability_Percentage));
                pancardnumber = Pancard(mydata.Pancard_number);
                //if (mydata.Citizen_ID == null)
                //{
                //    cid = true;
                //}
                //else
                //{
                //    string cid1 = mydata.Citizen_ID.Substring(0, 2);
                //    if (cid1 == "HA")
                //    {
                //        cid = Citizen(mydata.Citizen_ID);
                //    }
                //    else
                //    {
                //        string msgerror = "CIDR not valid";
                //        return Json(new { msgerror }, JsonRequestBehavior.AllowGet);


                //    }

                //}

                amountrent = Decimalchk(Convert.ToString(mydata.Amount_OF_Rent));
                annualincm = Decimalchk(Convert.ToString(mydata.Annual_income));
                anualbudget = Decimalchk(Convert.ToString(mydata.Annual_Budget_for_Maintenance));
                if (emailid == false || UID == false || mobile == false || landline == false || sanikname == false || father == false || spouse == false || mother == false || amountrent == false || annualincm == false || anualbudget == false || disabilitypercent == false || pancardnumber == false)
                {

                    if (emailid == false)
                    {
                        msg = "e";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (UID == false)
                    {
                        msg = "U";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (mobile == false)
                    {
                        msg = "M";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (landline == false)
                    {
                        msg = "L";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (sanikname == false)
                    {
                        msg = "Sname";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (father == false)
                    {
                        msg = "Fname";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (spouse == false)
                    {
                        msg = "Sname";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (mother == false)
                    {
                        msg = "Mname";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    //else if (cid == false)
                    //{
                    //    msg = "cid";
                    //    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    //}
                    else if (amountrent == false)
                    {
                        msg = "am";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (annualincm == false)
                    {
                        msg = "annual";
                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (anualbudget == false)
                    {
                        msg = "annualbdgt";

                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    //else if (dateofenrolment == false)
                    //{ 
                    //    msg = "dateofenrolment";

                    //    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    //}

                    else if (disabilitypercent == false)
                    {
                        msg = "disabilitypercent";

                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (pancardnumber == false)
                    {
                        msg = "pancardnumber";

                        return Json(new { msg }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(JsonRequestBehavior.AllowGet);
                }
                else
                {


                    try
                    {
                        //connection open///
                        if (dta.Connection.State == System.Data.ConnectionState.Closed)
                        {
                            dta.Connection.Open();
                        }
                        if (ed.Connection.State == System.Data.ConnectionState.Closed)
                        {
                            ed.Connection.Open();
                        }
                        ////////////
                        //transaction begin
                        dta.Transaction = dta.Connection.BeginTransaction();
                        ed.Transaction = ed.Connection.BeginTransaction();
                        /////////////////////////////

                        //bool check1 = dta.rsbgenerals.Any(tbll => tbll.Citizen_ID != null && tbll.Citizen_ID != "" || tbll.UID != null && tbll.UID != "");
                        //if (check1 == true)
                        //{

                        
                        check = dta.rsbgeneral_citizen1s.Any(tbl => tbl.Army_No == mydata.Army_No || tbl.ESMIdentitycardnumber == mydata.ESMIdentitycardnumber);
                        bool check2 = false;
                        
                        if(mydata.UID!=null)
                        {
                            check2 = dta.rsbgeneral_citizen1s.Any(tbl => tbl.UID == mydata.UID);
                            
                        }
                        else
                        {
                          check2=false;
                        }
                       
                         if (check == true)
                        {
                            string output = "already exist";
                            return Json(new { output }, JsonRequestBehavior.AllowGet);
                        }
                         else if (check2 == true)
                         {
                             string output = "uid exist exist";
                             return Json(new { output }, JsonRequestBehavior.AllowGet);
                         }
                         else
                         {
                             rsbgeneral_citizen1 rsb = new rsbgeneral_citizen1();

                             if (mydata.Photo != null)
                             {
                                 rsb.photo = mydata.Photo;


                             }

                             else
                             {

                                 if (Session["imagee"] != null)
                                 {
                                     rsb.photo = (byte[])Session["imagee"];
                                 }
                                 else
                                 {
                                     rsb.photo = null;
                                 }


                             }


                             rsb.Army_No = mydata.Army_No;
                             rsb.ESMIdentitycardnumber = mydata.ESMIdentitycardnumber;
                             rsb.UID = mydata.UID;
                             //if (rsb.UID != null && mydata.Citizen_ID==null)
                             //{
                             //    var fetchcidr = (from aw in ed.tblCIDRs where aw.UID == rsb.UID select aw.Citizen_ID).FirstOrDefault();
                             //    if (fetchcidr != null)
                             //    {
                             //        rsb.Citizen_ID = fetchcidr.FirstOrDefault().ToString();
                             //    }
                             //    else
                             //    {
                             //        rsb.Citizen_ID=null;
                             //    }
                             //}

                             //else if (mydata.UID == null && mydata.Citizen_ID != null)
                             //{
                             //    var checkcidr = ed.tblCIDRs.Any(tbl => tbl.Citizen_ID == mydata.Citizen_ID);
                             //    if (checkcidr == true)
                             //    {
                             //        rsb.Citizen_ID = mydata.Citizen_ID;
                             //    }
                             //    else
                             //    {
                             //        string output = "cidr not valid";
                             //        return Json(new { output }, JsonRequestBehavior.AllowGet);
                             //    }
                             //}
                             //else
                             //{
                             rsb.Citizen_ID = null;


                             // }



                             if (mydata.status == null)
                             {
                                 rsb.Status = null;
                             }
                             else
                             {
                                 rsb.Status = null;
                             }

                             rsb.Sanik_Name_eng = mydata.Sanik_Name_eng;
                             rsb.Sanik_Name_hindi = mydata.Sanik_Name_hindi;
                             rsb.Father_Name_eng = mydata.Father_Name_eng;
                             rsb.Father_Name_hindi = mydata.Father_Name_hindi;
                             rsb.Mother_Name_eng = mydata.Mother_Name_eng;
                             rsb.Mother_Name_hindi = mydata.Mother_Name_hindi;

                             //new 03-03-2018//
                             var nn = mydata.Date_of_Enrolment;

                             //new 03-03-2018//
                             if (nn != null)
                             {
                                 DateTime ValidDate;
                                 //DateTime? ValidDate2;

                                 if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                 {
                                     rsb.Date_of_Enrolment = ValidDate;



                                     //mydata.Error = "date comes after parsing";
                                     //mydata.PageError = "/View/RSBGeneralInfrm";
                                     //mydata.DateError = DateTime.Now;
                                     //mydata.DetailError = rsb.Date_of_Enrolment.ToString();
                                     //tblTraceError error21 = new tblTraceError();
                                     //error21.Error = mydata.Error;
                                     //error21.PageError = mydata.PageError;
                                     //error21.DateError = mydata.DateError;
                                     //error21.DetailError = mydata.DetailError;

                                     //ed.tblTraceErrors.InsertOnSubmit(error21);
                                     //ed.SubmitChanges();
                                 }
                                 else
                                 {

                                 }
                                 /////////////////////////////////error table///////////////////////////
                                 //mydata.Error = "date comes";
                                 //mydata.PageError = "/View/RSBGeneralInfrm";
                                 //mydata.DateError = DateTime.Now;
                                 //mydata.DetailError = nn.ToString();
                                 //tblTraceError error1 = new tblTraceError();
                                 //error1.Error = mydata.Error;
                                 //error1.PageError = mydata.PageError;
                                 //error1.DateError = mydata.DateError;
                                 //error1.DetailError = mydata.DetailError;

                                 //ed.tblTraceErrors.InsertOnSubmit(error1);
                                 //ed.SubmitChanges();
                                 //////////////////////////////////////////////////
                                 //string[] formats = { "MM-dd-yyyy" };
                                 //mydata.Date_of_Enrolment = DateTime.ParseExact(mydata.Date_of_Enrolment, formats, new CultureInfo("en-US"), DateTimeStyles.None);
                                 //rsb.Date_of_Enrolment = Convert.ToDateTime(nn);
                                 // rsb.Date_of_Enrolment = Convert.ToDateTime(mydata.Date_of_Enrolment);
                             }


                             else
                             {
                                 string doe = "Enter Date of Enrolment !!";
                                 return Json(new { doe }, JsonRequestBehavior.AllowGet);
                                 ////////////////////////////error table
                                 mydata.Error = "error in date";
                                 mydata.PageError = "/View/RSBGeneralInfrm";
                                 mydata.DateError = DateTime.Now;
                                 mydata.DetailError = rsb.Date_of_Enrolment.ToString();
                                 tblTraceError error2 = new tblTraceError();
                                 error2.Error = mydata.Error;
                                 error2.PageError = mydata.PageError;
                                 error2.DateError = mydata.DateError;
                                 error2.DetailError = mydata.DetailError;

                                 ed.tblTraceErrors.InsertOnSubmit(error2);
                                 ed.SubmitChanges();
                                 /////////////////////////////////

                             }
                             /////////////////////////////////////

                             rsb.Disable = mydata.Disable1;
                             rsb.Disability_Percentage = mydata.Disability_Percentage;
                             rsb.PPO_number = mydata.PPO_number;
                             rsb.Pancard_number = mydata.Pancard_number;
                             //end//
                             //if (mydata.DOB == null || mydata.DOB == "01-01-0001")
                             //{
                             //    string dob = "Enter DOB !!";
                             //    return Json(new { dob }, JsonRequestBehavior.AllowGet);
                             //}
                             if (DateTime.ParseExact(mydata.DOB, "dd/MM/yyyy", null) != DateTime.Now.Date)
                             {
                                 if (DateTime.ParseExact(mydata.DOB, "dd/MM/yyyy", null) < datee)
                                 {
                                     var dob1 = mydata.DOB;

                                     //new 19-03-2018//
                                     if (dob1 != null)
                                     {
                                         DateTime ValidDate1;
                                         //DateTime? ValidDate2;

                                         if (DateTime.TryParseExact(dob1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
                                         {
                                             rsb.DOB = ValidDate1;
                                             //////////////////////////////error table
                                             //mydata.Error = "error in dateof birth";
                                             //mydata.PageError = "/View/RSBGeneralInfrm";
                                             //mydata.DateError = DateTime.Now;
                                             //mydata.DetailError = Convert.ToString(rsb.DOB);
                                             //tblTraceError error2 = new tblTraceError();
                                             //error2.Error = mydata.Error;
                                             //error2.PageError = mydata.PageError;
                                             //error2.DateError = mydata.DateError;
                                             //error2.DetailError = mydata.DetailError;

                                             //ed.tblTraceErrors.InsertOnSubmit(error2);
                                             //ed.SubmitChanges();
                                             ///////////////////////////////////

                                         }
                                         else
                                         {
                                             mydata.Error = "error in dateof else part birth";
                                             mydata.PageError = "/View/RSBGeneralInfrm";
                                             mydata.DateError = DateTime.Now;
                                             mydata.DetailError = dob1;
                                             tblTraceError error2 = new tblTraceError();
                                             error2.Error = mydata.Error;
                                             error2.PageError = mydata.PageError;
                                             error2.DateError = mydata.DateError;
                                             error2.DetailError = mydata.DetailError;

                                             ed.tblTraceErrors.InsertOnSubmit(error2);
                                             ed.SubmitChanges();
                                         }

                                     }
                                     else
                                     {
                                         string dob = "DOB is enter valid !!";
                                         return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                     }
                                 }
                                 else
                                 {
                                     string dob = "DOB is not valid !!";
                                     return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                 }
                             }
                             else
                             {
                                 string dob = "DOB is not valid !!";
                                 return Json(new { dob }, JsonRequestBehavior.AllowGet);
                             }

                             rsb.Gender_code = mydata.Gender;
                             rsb.CategoryCode = mydata.CategoryDesc;
                             rsb.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusDesc);
                             if (Convert.ToString(rsb.MaritalStatusCode) == "2")
                             {
                                 rsb.spousename = null;
                                 rsb.spousenamehindi = null;
                             }
                             else
                             {
                                 rsb.spousename = mydata.spousename;
                                 rsb.spousenamehindi = mydata.spousenamehindi;
                             }
                             rsb.Regement_Corps_id = mydata.Regement_Corps_id;
                             rsb.landline = mydata.landline;
                             rsb.mobileno = mydata.mobileno;
                             rsb.emialid = mydata.emialid;
                             rsb.Per_address_eng = mydata.Per_address_eng;
                             rsb.Per_address_hindi = mydata.Per_address_hindi;
                             rsb.Per_Landmark_english = mydata.Per_Landmark_english;
                             rsb.dcode = mydata.dcode;
                             rsb.tcode = mydata.tcode;

                             rsb.Urban_rural = Convert.ToChar(mydata.Urban_rural);
                             if (rsb.Urban_rural == Convert.ToChar("U"))
                             {
                                 if (mydata.towncode == null)
                                 {
                                     rsb.towncode = null;
                                 }
                                 else
                                 {
                                     rsb.towncode = mydata.towncode;
                                 }

                             }
                             else
                             {
                                 if (mydata.VCODE == null)
                                 {
                                     rsb.VCODE = null;
                                 }
                                 else
                                 {
                                     rsb.VCODE = mydata.VCODE;
                                 }
                             }
                             rsb.perchk = mydata.perchk;
                             if (mydata.Per_cors == null)
                             {
                                 rsb.Per_cors = null;
                             }
                             else
                             {
                                 rsb.Per_cors = Convert.ToChar(mydata.Per_cors);
                             }

                             rsb.statecode = mydata.statecode;
                             rsb.Per_Landmark_Hindi = mydata.Per_Landmark_Hindi;
                             rsb.Pin_code = mydata.Pin_code;
                             rsb.Cors_address = mydata.Cors_address;
                             if (mydata.Amount_OF_Rent == null)
                             {
                                 rsb.Amount_OF_Rent = 0;
                             }
                             else
                             {
                                 rsb.Amount_OF_Rent = mydata.Amount_OF_Rent;
                             }
                             if (mydata.Annual_Budget_for_Maintenance == null)
                             {
                                 rsb.Annual_Budget_for_Maintenance = 0;
                             }
                             else
                             {
                                 rsb.Annual_Budget_for_Maintenance = mydata.Annual_Budget_for_Maintenance;
                             }
                             if (mydata.Annual_income == null)
                             {
                                 rsb.Annual_income = 0;
                             }
                             else
                             {
                                 rsb.Annual_income = mydata.Annual_income;
                             }
                             rsb.statusflag = null;
                             rsb.RegistrationDate = DateTime.Now;
                             rsb.CreateDate = DateTime.Now;
                             rsb.CreateUser = Session["userid"].ToString();
                             dta.rsbgeneral_citizen1s.InsertOnSubmit(rsb);

                             dta.SubmitChanges();
                             //var cidtn=(string)Session["citizenid"];
                             //if (cidtn != null)
                             //{


                             //    ////insert into cidr table
                             //    try
                             //    {
                             //        tblCIDR cdr = new tblCIDR();
                             //        cdr.Citizen_ID = mydata.Citizen_ID;
                             //        cdr.Citizen_Name_EN = mydata.Sanik_Name_eng;
                             //        cdr.Citizen_Name_LL = mydata.Sanik_Name_hindi;
                             //        cdr.Gender = Convert.ToChar(mydata.Gender);
                             //        cdr.Marital_Status = mydata.MaritalStatusCode;
                             //        cdr.Caste_Category = mydata.CategoryCode;
                             //        cdr.Father_Name_EN = mydata.Father_Name_eng;
                             //        cdr.Father_Name_LL = mydata.Father_Name_hindi;
                             //        cdr.Mother_Name_EN = mydata.Mother_Name_eng;
                             //        cdr.Father_Name_LL = mydata.Mother_Name_hindi;
                             //        cdr.DOB = rsb.DOB;
                             //        cdr.House_Name_No = mydata.Per_address_eng;
                             //        cdr.Landmark_Locality_Colony = mydata.Per_Landmark_english;
                             //        cdr.Correspondence_Address_EN = mydata.Cors_address;
                             //        cdr.RuralUrban = Convert.ToChar(mydata.Urban_rural);
                             //        cdr.Village_Town_Code = mydata.VCODE;
                             //        cdr.Block_Tehsil_Code = mydata.tcode;
                             //        cdr.District_Code = mydata.dcode;
                             //        cdr.PIN = mydata.Pin_code;
                             //        cdr.Email_id = mydata.emialid;
                             //        cdr.Mobile = mydata.mobileno;
                             //        cdr.DOM = null;
                             //        cdr.PortalCode = "CE";
                             //        cdr.Source = "0";
                             //        ed.tblCIDRs.InsertOnSubmit(cdr);
                             //        ed.SubmitChanges();
                             //    }
                             //    catch (Exception ex)
                             //    {
                             //        throw ex;
                             //        //changes on 13-03-2018
                             //        mydata.Error = ex.Message;
                             //        mydata.PageError = "/View/RSBGeneralInfrm";
                             //        mydata.DateError = DateTime.Now;
                             //        mydata.DetailError = ex.ToString();
                             //        tblTraceError error = new tblTraceError();
                             //        error.Error = mydata.Error;
                             //        error.PageError = mydata.PageError;
                             //        error.DateError = mydata.DateError;
                             //        error.DetailError = mydata.DetailError;

                             //        ed.tblTraceErrors.InsertOnSubmit(error);
                             //        ed.SubmitChanges();
                             //        //end
                             //        return Json(JsonRequestBehavior.AllowGet);
                             //    }
                             //}
                             //transation complete or not
                             ed.Transaction.Commit();
                             dta.Transaction.Commit();
                             //end transation
                             //end
                             Session["imagee"] = null;
                             var result = new { armyno = mydata.Army_No, sanikname = mydata.Sanik_Name_eng };

                             Session["id"] = rsb.id;
                             Session["Armyno"] = rsb.Army_No;
                             Session["aarmyno"] = rsb.Army_No;
                             Session["armynoalrd"] = rsb.Army_No;
                             //for citizen armynumber//
                             Session["armynumcitizen"] = rsb.Army_No;
                             if (rsb.UID != null)
                             {
                                 Session["uid"] = rsb.UID;
                             }
                             else
                             {
                                 Session["uid"] = null;
                             }
                             //connection close///////////////
                             if (dta.Connection.State == System.Data.ConnectionState.Open)
                             {
                                 dta.Connection.Close();
                             }
                             if (ed.Connection.State == System.Data.ConnectionState.Open)
                             {
                                 ed.Connection.Close();
                             }
                             ////////////////////////////
                             return Json(result, JsonRequestBehavior.AllowGet);


                         }



                    }
                    catch (Exception ex)
                    {

                        // throw ex;
                        //changes on 13-03-2018
                        mydata.Error = ex.Message;
                        mydata.PageError = "/View/RSBGeneralInfrm";
                        mydata.DateError = DateTime.Now;
                        mydata.DetailError = ex.ToString();
                        tblTraceError error = new tblTraceError();
                        error.Error = mydata.Error;
                        error.PageError = mydata.PageError;
                        error.DateError = mydata.DateError;
                        error.DetailError = mydata.DetailError;

                        ed.tblTraceErrors.InsertOnSubmit(error);
                        ed.SubmitChanges();
                        ed.Transaction.Rollback();
                        dta.Transaction.Rollback();
                        //end
                        return Json(JsonRequestBehavior.AllowGet);


                    }



                }
            }

        }
        //new//
        //public DataTable ExecuteDataTable(string cmdText, SqlParameter[] prms, CommandType type)
        //{

        //DataTable dt = default(DataTable);
        //SqlCommand cmd = default(SqlCommand);
        //SqlDataAdapter adpt = default(SqlDataAdapter);
        //SqlConnection conn = default(SqlConnection);
        //    using (conn = new SqlConnection(Constrg))
        //    {

        //        dt = new DataTable();
        //        using (cmd = new SqlCommand(cmdText, conn))
        //        {
        //            cmd.CommandType = type;
        //            if (prms != null)
        //            {
        //                foreach (SqlParameter p in prms)
        //                {
        //                    cmd.Parameters.Add(p);
        //                }
        //            }
        //            adpt = new SqlDataAdapter(cmd);
        //            adpt.Fill(dt);
        //            return dt;
        //        }
        //    }
        //}

        //end//

        //public JsonResult update_citizen(Generalform mydata)
        //{
        //    DateTime datee = Convert.ToDateTime("01/01/2000");

        //    try
        //    {

        //        //connection open///
        //        if (dta.Connection.State == System.Data.ConnectionState.Closed)
        //        {
        //            dta.Connection.Open();
        //        }
        //        if (ed.Connection.State == System.Data.ConnectionState.Closed)
        //        {
        //            ed.Connection.Open();
        //        }
        //        ////////////
        //        //transaction begin
        //        dta.Transaction = dta.Connection.BeginTransaction();
        //        ed.Transaction = ed.Connection.BeginTransaction();
        //        /////////////////////////////
        //        rsbgeneral_citizen rsb = new rsbgeneral_citizen();

        //        rsb = dta.rsbgeneral_citizens.Where(x => x.UID == mydata.UID || x.Citizen_ID == mydata.Citizen_ID || x.Army_No == mydata.Army_No || x.Army_No == ((string)Session["armynoalrd"]) || x.Army_No == ((string)Session["Armyno"])).FirstOrDefault();
        //        if (rsb != null)
        //        {
        //            if (mydata.Photo != null)
        //            {
        //                rsb.photo = mydata.Photo;


        //            }

        //            else
        //            {

        //                if (Session["imagee"] != null)
        //                {
        //                    rsb.photo = (byte[])Session["imagee"];
        //                    Session["imagee"] = null;
        //                }
        //                else
        //                {
        //                    rsb.photo = null;
        //                }


        //            }

        //            rsb.Army_No = mydata.Army_No;
        //            rsb.ESMIdentitycardnumber = mydata.ESMIdentitycardnumber;
        //            if (mydata.UID == null)
        //            {
        //                rsb.UID = null;
        //            }
        //            else
        //            {
        //                var uid = mydata.UID;

        //                bool UID = Verhoeff.validateVerhoeff(uid.ToString().Trim());
        //                if (UID == true)
        //                {
        //                    rsb.UID = uid;
        //                }
        //                else
        //                {
        //                    string msg = "U";
        //                    return Json(new { msg }, JsonRequestBehavior.AllowGet);

        //                }

        //            }


        //            if (rsb.Citizen_ID == null)
        //            {
        //                rsb.Citizen_ID = null;
        //            }
        //            else
        //            {
        //                //string cid1 = mydata.Citizen_ID.Substring(0, 2);
        //                //if (cid1 == "HA")
        //                //{
        //                //    bool cid = Citizen(mydata.Citizen_ID);
        //                //    if (cid == false)
        //                //    {
        //                //        string msgerror = "CIDR nt valid";
        //                //        return Json(new { msgerror }, JsonRequestBehavior.AllowGet);

        //                //    }
        //                //    else
        //                //    {
        //                rsb.Citizen_ID = rsb.Citizen_ID;
        //                //    }
        //                //}
        //                //else
        //                //{
        //                //    string msgerror = "CIDR not valid";
        //                //    return Json(new { msgerror }, JsonRequestBehavior.AllowGet);

        //                //}
        //            }


        //            rsb.Sanik_Name_eng = mydata.Sanik_Name_eng;
        //            rsb.Sanik_Name_hindi = mydata.Sanik_Name_hindi;
        //            rsb.Father_Name_eng = mydata.Father_Name_eng;
        //            rsb.Father_Name_hindi = mydata.Father_Name_hindi;
        //            rsb.Mother_Name_eng = mydata.Mother_Name_eng;
        //            rsb.Mother_Name_hindi = mydata.Mother_Name_hindi;
        //            rsb.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusDesc);
        //            if (Convert.ToString(rsb.MaritalStatusCode) == "2")
        //            {
        //                rsb.spousename = null;
        //                rsb.spousenamehindi = null;
        //            }
        //            else
        //            {
        //                rsb.spousename = mydata.spousename;
        //                rsb.spousenamehindi = mydata.spousenamehindi;
        //            }
        //            try
        //            {
        //                if (DateTime.ParseExact(mydata.DOB, "dd/MM/yyyy", null) != DateTime.Now.Date)
        //                {
        //                    if (DateTime.ParseExact(mydata.DOB, "dd/MM/yyyy", null) < datee)
        //                    {
        //                        var dob1 = mydata.DOB;

        //                        //new 19-03-2018//
        //                        if (dob1 != null)
        //                        {
        //                            DateTime ValidDate1;
        //                            //DateTime? ValidDate2;

        //                            if (DateTime.TryParseExact(dob1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
        //                            {
        //                                rsb.DOB = ValidDate1;
        //                                ////////////////////////////error table
        //                                mydata.Error = "error in dateof birth";
        //                                mydata.PageError = "/View/RSBGeneralInfrm";
        //                                mydata.DateError = DateTime.Now;
        //                                mydata.DetailError = dob1;
        //                                tblTraceError error2 = new tblTraceError();
        //                                error2.Error = mydata.Error;
        //                                error2.PageError = mydata.PageError;
        //                                error2.DateError = mydata.DateError;
        //                                error2.DetailError = mydata.DetailError;

        //                                ed.tblTraceErrors.InsertOnSubmit(error2);
        //                                ed.SubmitChanges();
        //                                /////////////////////////////////

        //                            }
        //                            else
        //                            {
        //                                mydata.Error = "error in dateof else part birth";
        //                                mydata.PageError = "/View/RSBGeneralInfrm";
        //                                mydata.DateError = DateTime.Now;
        //                                mydata.DetailError = dob1;
        //                                tblTraceError error2 = new tblTraceError();
        //                                error2.Error = mydata.Error;
        //                                error2.PageError = mydata.PageError;
        //                                error2.DateError = mydata.DateError;
        //                                error2.DetailError = mydata.DetailError;

        //                                ed.tblTraceErrors.InsertOnSubmit(error2);
        //                                ed.SubmitChanges();
        //                            }

        //                        }
        //                        else
        //                        {
        //                            string dob = "DOB is enter valid !!";
        //                            return Json(new { dob }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        string dob = "DOB is not valid !!";
        //                        return Json(new { dob }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //                else
        //                {
        //                    string dob = "DOB is not valid !!";
        //                    return Json(new { dob }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //changes on 13-03-2018
        //                mydata.Error = ex.Message;
        //                mydata.PageError = "/View/RSBGeneralInfrm";
        //                mydata.DateError = DateTime.Now;
        //                mydata.DetailError = ex.ToString();
        //                tblTraceError error = new tblTraceError();
        //                error.Error = mydata.Error;
        //                error.PageError = mydata.PageError;
        //                error.DateError = mydata.DateError;
        //                error.DetailError = mydata.DetailError;

        //                ed.tblTraceErrors.InsertOnSubmit(error);
        //                ed.SubmitChanges();
        //                //end
        //                return Json(JsonRequestBehavior.AllowGet); throw ex;
        //            }
        //            if (mydata.status == null)
        //            {
        //                rsb.Status = Convert.ToChar("E");
        //            }
        //            else
        //            {
        //                rsb.Status = Convert.ToChar(mydata.status);
        //            }

        //            rsb.Gender_code = mydata.Gender;
        //            rsb.CategoryCode = mydata.CategoryDesc;

        //            rsb.Regement_Corps_id = mydata.Regement_Corps_id;
        //            rsb.landline = mydata.landline;
        //            rsb.mobileno = mydata.mobileno;
        //            rsb.emialid = mydata.emialid;
        //            rsb.Per_address_eng = mydata.Per_address_eng;
        //            rsb.Per_address_hindi = mydata.Per_address_hindi;
        //            rsb.Per_Landmark_english = mydata.Per_Landmark_english;
        //            rsb.Per_Landmark_Hindi = mydata.Per_Landmark_Hindi;
        //            rsb.statecode = mydata.statecode;
        //            rsb.Urban_rural = Convert.ToChar(mydata.Urban_rural);
        //            if (rsb.Urban_rural == Convert.ToChar("U"))
        //            {
        //                if (mydata.towncode == null)
        //                {
        //                    rsb.towncode = null;
        //                }
        //                else
        //                {
        //                    rsb.towncode = mydata.towncode;
        //                }

        //            }
        //            else
        //            {
        //                if (mydata.VCODE == null)
        //                {
        //                    rsb.VCODE = null;
        //                }
        //                else
        //                {
        //                    rsb.VCODE = mydata.VCODE;
        //                }
        //            }
        //            rsb.Pin_code = mydata.Pin_code;
        //            rsb.Cors_address = mydata.Cors_address;
        //            if (mydata.perchk == null)
        //            {
        //                rsb.perchk = null;
        //            }
        //            else
        //            {
        //                rsb.perchk = mydata.perchk;
        //            }

        //            rsb.Per_cors = Convert.ToChar(mydata.Per_cors);
        //            if (mydata.Amount_OF_Rent == null)
        //            {
        //                rsb.Amount_OF_Rent = 0;
        //            }
        //            else
        //            {
        //                rsb.Amount_OF_Rent = mydata.Amount_OF_Rent;
        //            }
        //            if (mydata.Annual_Budget_for_Maintenance == null)
        //            {
        //                rsb.Annual_Budget_for_Maintenance = 0;
        //            }
        //            else
        //            {
        //                rsb.Annual_Budget_for_Maintenance = mydata.Annual_Budget_for_Maintenance;
        //            }
        //            if (mydata.Annual_income == null)
        //            {
        //                rsb.Annual_income = 0;
        //            }
        //            else
        //            {
        //                rsb.Annual_income = mydata.Annual_income;
        //            }
        //            var nn = mydata.Date_of_Enrolment;

        //            //new 03-03-2018//
        //            if (nn != null)
        //            {
        //                DateTime ValidDate;

        //                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
        //                {
        //                    rsb.Date_of_Enrolment = ValidDate;
        //                }
        //                ///////////////////////////////error table///////////////////////////
        //                mydata.Error = rsb.Date_of_Enrolment.ToString();
        //                mydata.PageError = "/View/RSBGeneralInfrm";
        //                mydata.DateError = DateTime.Now;
        //                mydata.DetailError = rsb.Date_of_Enrolment.ToString();
        //                tblTraceError error1 = new tblTraceError();
        //                error1.Error = mydata.Error;
        //                error1.PageError = mydata.PageError;
        //                error1.DateError = mydata.DateError;
        //                error1.DetailError = mydata.DetailError;

        //                ed.tblTraceErrors.InsertOnSubmit(error1);
        //                ed.SubmitChanges();
        //                //////////////////////////////////////////////////
        //                //string[] formats = { "MM-dd-yyyy" };
        //                //mydata.Date_of_Enrolment = DateTime.ParseExact(mydata.Date_of_Enrolment, formats, new CultureInfo("en-US"), DateTimeStyles.None);
        //                //rsb.Date_of_Enrolment = Convert.ToDateTime(nn);
        //                // rsb.Date_of_Enrolment = Convert.ToDateTime(mydata.Date_of_Enrolment);
        //            }
        //            else
        //            {
        //                string doe = "Enter Date of Enrolment !!";
        //                return Json(new { doe }, JsonRequestBehavior.AllowGet);
        //                ////////////////////////////error table
        //                mydata.Error = "error in date";
        //                mydata.PageError = "/View/RSBGeneralInfrm";
        //                mydata.DateError = DateTime.Now;
        //                mydata.DetailError = rsb.Date_of_Enrolment.ToString();
        //                tblTraceError error2 = new tblTraceError();
        //                error2.Error = mydata.Error;
        //                error2.PageError = mydata.PageError;
        //                error2.DateError = mydata.DateError;
        //                error2.DetailError = mydata.DetailError;

        //                ed.tblTraceErrors.InsertOnSubmit(error2);
        //                ed.SubmitChanges();
        //                /////////////////////////////////

        //            }

        //            rsb.Disable = mydata.Disable1;

        //            if (rsb.Disable == true)
        //            {
        //                rsb.Disability_Percentage = mydata.Disability_Percentage;
        //            }
        //            else
        //            {
        //                rsb.Disability_Percentage = null;
        //            }
        //            rsb.PPO_number = mydata.PPO_number;
        //            rsb.Pancard_number = mydata.Pancard_number;
        //            //end//
        //            rsb.ChangeUser = (String)Session["userid"];
        //            rsb.ChangeDate = DateTime.Now;
        //            dta.SubmitChanges();
        //            //changes in cidr table
        //            if (!string.IsNullOrEmpty(mydata.Citizen_ID) && mydata.Citizen_ID != "null")
        //            {
        //                try
        //                {

        //                    tblCIDR cdr = new tblCIDR();
        //                    cdr = ed.tblCIDRs.Where(x => x.Citizen_ID == mydata.Citizen_ID).FirstOrDefault();
        //                    if (cdr != null)
        //                    {
        //                        //dr.Citizen_ID = mydata.Citizen_ID;
        //                        cdr.Citizen_Name_EN = mydata.Sanik_Name_eng;
        //                        cdr.Citizen_Name_LL = mydata.Sanik_Name_hindi;
        //                        cdr.Gender = Convert.ToChar(mydata.Gender);
        //                        cdr.Marital_Status = mydata.MaritalStatusCode;
        //                        cdr.Caste_Category = mydata.CategoryCode;
        //                        cdr.Father_Name_EN = mydata.Father_Name_eng;
        //                        cdr.Father_Name_LL = mydata.Father_Name_hindi;
        //                        cdr.Mother_Name_EN = mydata.Mother_Name_eng;
        //                        cdr.Father_Name_LL = mydata.Mother_Name_hindi;
        //                        cdr.DOB = rsb.DOB;
        //                        cdr.House_Name_No = mydata.Per_address_eng;
        //                        cdr.Landmark_Locality_Colony = mydata.Per_Landmark_english;
        //                        cdr.Correspondence_Address_EN = mydata.Cors_address;
        //                        cdr.RuralUrban = Convert.ToChar(mydata.Urban_rural);
        //                        cdr.Village_Town_Code = mydata.VCODE;
        //                        cdr.Block_Tehsil_Code = mydata.tcode;
        //                        cdr.District_Code = mydata.dcode;
        //                        cdr.PIN = mydata.Pin_code;
        //                        cdr.Email_id = mydata.emialid;
        //                        cdr.Mobile = mydata.mobileno;
        //                        cdr.DOM = null;

        //                        ed.SubmitChanges();
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    // throw ex;
        //                    //changes on 13-03-2018
        //                    mydata.Error = ex.Message;
        //                    mydata.PageError = "/View/RSBGeneralInfrm";
        //                    mydata.DateError = DateTime.Now;
        //                    mydata.DetailError = ex.ToString();
        //                    tblTraceError error = new tblTraceError();
        //                    error.Error = mydata.Error;
        //                    error.PageError = mydata.PageError;
        //                    error.DateError = mydata.DateError;
        //                    error.DetailError = mydata.DetailError;

        //                    ed.tblTraceErrors.InsertOnSubmit(error);
        //                    ed.SubmitChanges();
        //                    //end
        //                    return Json(JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            ////end
        //            //transation complete or not
        //            ed.Transaction.Commit();
        //            dta.Transaction.Commit();
        //            //end transation
        //            Session["Armyno"] = rsb.Army_No;
        //            //connection close///////////////
        //            if (dta.Connection.State == System.Data.ConnectionState.Open)
        //            {
        //                dta.Connection.Close();
        //            }
        //            if (ed.Connection.State == System.Data.ConnectionState.Open)
        //            {
        //                ed.Connection.Close();
        //            }
        //            ////////////////////////////
        //            return Json(rsb, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            string msg1 = "not";
        //            return Json(msg1, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //
        //        //changes on 13-03-2018
        //        mydata.Error = ex.Message;
        //        mydata.PageError = "/View/RSBGeneralInfrm";
        //        mydata.DateError = DateTime.Now;
        //        mydata.DetailError = ex.ToString();
        //        tblTraceError error = new tblTraceError();
        //        error.Error = mydata.Error;
        //        error.PageError = mydata.PageError;
        //        error.DateError = mydata.DateError;
        //        error.DetailError = mydata.DetailError;

        //        ed.tblTraceErrors.InsertOnSubmit(error);
        //        ed.SubmitChanges();
        //        ed.Transaction.Rollback();
        //        dta.Transaction.Rollback();
        //        //end
        //        return Json(JsonRequestBehavior.AllowGet); throw ex;


        //    }

        //}

        [HttpPost]
        public JsonResult Sanik_General_Registrationstep2_citizen(Generalform mydata)
        {
            //int datee = Convert.ToDateTime("01/01/2000").Year;
            string msg;
            bool dependent, cid = false, noofchildren;


            dependent = IsName(mydata.Dependent_Name);
            noofchildren = IsNumber(mydata.Number_Of_children);
            if (mydata.UID == null)
            { cid = true; }
            else
            {
                var cid1 = mydata.UID;

                cid = Verhoeff.validateVerhoeff(cid1.ToString().Trim());

            }
            if (cid == false || dependent == false || noofchildren == false)
            {

                if (cid == false)
                {
                    msg = "c";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (dependent == false)
                {
                    msg = "d";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (noofchildren == false)
                {
                    msg = "child";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    DateTime Dependent_DOB, DOM;
                    var nn1 = mydata.Dependent_DOB;
                    DateTime ValidDate1;
                    DateTime ValidDate2;
                    //DateTime? ValidDate2;
                    var nn3 = mydata.DOM;

                    bool check = dta.tblfamilydetails.Any(tbl => tbl.Army_No == mydata.Army_No && tbl.UID == mydata.UID);

                    if (check == true)
                    {
                        string output = "already exist";
                        return Json(new { output, mydata.UID }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                       
                        string uidsanik=(string)Session["uid"];
                        if (mydata.UID != null)
                        {
                            if (uidsanik == mydata.UID)
                            {
                                 msg = "uid";
                                return Json(new { msg, mydata.UID }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                tblfamilydetails_citizen fm = new tblfamilydetails_citizen();


                                if (Session["imagee"] != null)
                                {
                                    fm.imagedept = (byte[])Session["imagee"];
                                    Session["imagee"] = null;
                                }
                                else if (mydata.imagedept != null)
                                {

                                    var img = Session["imagee"];
                                    fm.imagedept = mydata.imagedept;


                                }
                                else
                                {
                                    fm.imagedept = null;
                                }

                                if (Session["Armyno"] != null)
                                {
                                    fm.Army_No = Session["Armyno"].ToString();
                                }
                                else
                                {
                                    fm.Army_No = mydata.Army_No;
                                }


                                fm.Dependent_Name = mydata.Dependent_Name;
                                if (mydata.RelationCode == "0")
                                {
                                    string rel = "please select relation";
                                    return Json(new { rel }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    fm.RelationCode = mydata.RelationCode;
                                }

                                fm.UID = mydata.UID;
                                if (mydata.Dependent_DOB != null)
                                {

                                    if (DateTime.TryParseExact(nn1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
                                    {

                                        Dependent_DOB = ValidDate1;
                                        if (Dependent_DOB != DateTime.Now.Date)
                                        {
                                            var nn = mydata.Dependent_DOB;

                                            //new 03-03-2018//
                                            if (nn != null)
                                            {
                                                DateTime ValidDate;
                                                //DateTime? ValidDate2;

                                                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                                {
                                                    fm.DOB = ValidDate;
                                                }
                                            }
                                            else
                                            {
                                                mydata.Error = "date of dependent error";
                                                mydata.PageError = "/View/RSBGeneralInfrm";
                                                mydata.DateError = DateTime.Now;
                                                mydata.DetailError = nn.ToString();
                                                tblTraceError error1 = new tblTraceError();
                                                error1.Error = mydata.Error;
                                                error1.PageError = mydata.PageError;
                                                error1.DateError = mydata.DateError;
                                                error1.DetailError = mydata.DetailError;

                                                ed.tblTraceErrors.InsertOnSubmit(error1);
                                                ed.SubmitChanges();


                                            }



                                        }
                                        else
                                        {
                                            string dob = "DOB is not valid !!";
                                            return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        string dob = "DOB is not valid !!";
                                        return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                                else
                                {
                                    fm.DOB = null;
                                }


                                fm.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusCode);
                                if (Convert.ToString(fm.MaritalStatusCode) == "2")
                                {
                                    fm.DOM = null;

                                }
                                else
                                {
                                    if (mydata.DOM == null)
                                    {
                                        string domm = "Date of marriage not null !!";
                                        return Json(new { domm }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        if (DateTime.TryParseExact(nn3.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate2))
                                        {
                                            DOM = ValidDate2;

                                            if (DOM <= DateTime.Now.Date)
                                            {
                                                if (mydata.Dependent_DOB == mydata.DOM)
                                                {

                                                    string des = "Date of marriage and Date of Birth Not Equal !!";
                                                    return Json(new { des }, JsonRequestBehavior.AllowGet);
                                                }
                                                else
                                                {
                                                    var nn = mydata.DOM;

                                                    //new 03-03-2018//
                                                    if (nn != null)
                                                    {
                                                        DateTime ValidDate;
                                                        //DateTime? ValidDate2;

                                                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                                        {
                                                            fm.DOM = ValidDate;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        mydata.Error = "date of dependent edit error";
                                                        mydata.PageError = "/View/RSBGeneralInfrm";
                                                        mydata.DateError = DateTime.Now;
                                                        mydata.DetailError = nn.ToString();
                                                        tblTraceError error1 = new tblTraceError();
                                                        error1.Error = mydata.Error;
                                                        error1.PageError = mydata.PageError;
                                                        error1.DateError = mydata.DateError;
                                                        error1.DetailError = mydata.DetailError;

                                                        ed.tblTraceErrors.InsertOnSubmit(error1);
                                                        ed.SubmitChanges();


                                                    }

                                                }
                                            }
                                            else
                                            {
                                                string dom = "Date of marriage is not valid !!";
                                                return Json(new { dom }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        else
                                        {
                                            string dom = "Date of marriage is not valid !!";
                                            return Json(new { dom }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                                fm.Dependent_Id = mydata.Dependent_Id;
                                //chnges 07-03-2018//
                                fm.Wife_recorded_in_PPO_no = mydata.Wife_recorded_in_PPO_no;
                                fm.Wife_OtherName = mydata.Wife_othername;

                                fm.Number_Of_children = Convert.ToInt16(mydata.Number_Of_children);
                                fm.Recorded_in_DO_Part2 = mydata.Recorded_in_DO_Part2;
                                fm.Recorded_in_DO_Part2text = mydata.Recorded_in_DO_Part2text;
                                //end
                                dta.tblfamilydetails_citizens.InsertOnSubmit(fm);

                                dta.SubmitChanges();

                                ViewBag.Message = "saved";

                                Session["Idd"] = fm.Dependent_Id;


                                return Json(new { fm }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            tblfamilydetails_citizen fm = new tblfamilydetails_citizen();


                            if (Session["imagee"] != null)
                            {
                                fm.imagedept = (byte[])Session["imagee"];
                                Session["imagee"] = null;
                            }
                            else if (mydata.imagedept != null)
                            {

                                var img = Session["imagee"];
                                fm.imagedept = mydata.imagedept;


                            }
                            else
                            {
                                fm.imagedept = null;
                            }

                            if (Session["Armyno"] != null)
                            {
                                fm.Army_No = Session["Armyno"].ToString();
                            }
                            else
                            {
                                fm.Army_No = mydata.Army_No;
                            }


                            fm.Dependent_Name = mydata.Dependent_Name;
                            if (mydata.RelationCode == "0")
                            {
                                string rel = "please select relation";
                                return Json(new { rel }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                fm.RelationCode = mydata.RelationCode;
                            }

                            fm.UID = mydata.UID;
                            if (mydata.Dependent_DOB != null)
                            {

                                if (DateTime.TryParseExact(nn1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
                                {

                                    Dependent_DOB = ValidDate1;
                                    if (Dependent_DOB != DateTime.Now.Date)
                                    {
                                        var nn = mydata.Dependent_DOB;

                                        //new 03-03-2018//
                                        if (nn != null)
                                        {
                                            DateTime ValidDate;
                                            //DateTime? ValidDate2;

                                            if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                            {
                                                fm.DOB = ValidDate;
                                            }
                                        }
                                        else
                                        {
                                            mydata.Error = "date of dependent error";
                                            mydata.PageError = "/View/RSBGeneralInfrm";
                                            mydata.DateError = DateTime.Now;
                                            mydata.DetailError = nn.ToString();
                                            tblTraceError error1 = new tblTraceError();
                                            error1.Error = mydata.Error;
                                            error1.PageError = mydata.PageError;
                                            error1.DateError = mydata.DateError;
                                            error1.DetailError = mydata.DetailError;

                                            ed.tblTraceErrors.InsertOnSubmit(error1);
                                            ed.SubmitChanges();


                                        }



                                    }
                                    else
                                    {
                                        string dob = "DOB is not valid !!";
                                        return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    string dob = "DOB is not valid !!";
                                    return Json(new { dob }, JsonRequestBehavior.AllowGet);
                                }

                            }
                            else
                            {
                                fm.DOB = null;
                            }


                            fm.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusCode);
                            if (Convert.ToString(fm.MaritalStatusCode) == "2")
                            {
                                fm.DOM = null;

                            }
                            else
                            {
                                if (mydata.DOM == null)
                                {
                                    string domm = "Date of marriage not null !!";
                                    return Json(new { domm }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    if (DateTime.TryParseExact(nn3.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate2))
                                    {
                                        DOM = ValidDate2;

                                        if (DOM <= DateTime.Now.Date)
                                        {
                                            if (mydata.Dependent_DOB == mydata.DOM)
                                            {

                                                string des = "Date of marriage and Date of Birth Not Equal !!";
                                                return Json(new { des }, JsonRequestBehavior.AllowGet);
                                            }
                                            else
                                            {
                                                var nn = mydata.DOM;

                                                //new 03-03-2018//
                                                if (nn != null)
                                                {
                                                    DateTime ValidDate;
                                                    //DateTime? ValidDate2;

                                                    if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                                                    {
                                                        fm.DOM = ValidDate;
                                                    }
                                                }
                                                else
                                                {
                                                    mydata.Error = "date of dependent edit error";
                                                    mydata.PageError = "/View/RSBGeneralInfrm";
                                                    mydata.DateError = DateTime.Now;
                                                    mydata.DetailError = nn.ToString();
                                                    tblTraceError error1 = new tblTraceError();
                                                    error1.Error = mydata.Error;
                                                    error1.PageError = mydata.PageError;
                                                    error1.DateError = mydata.DateError;
                                                    error1.DetailError = mydata.DetailError;

                                                    ed.tblTraceErrors.InsertOnSubmit(error1);
                                                    ed.SubmitChanges();


                                                }

                                            }
                                        }
                                        else
                                        {
                                            string dom = "Date of marriage is not valid !!";
                                            return Json(new { dom }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        string dom = "Date of marriage is not valid !!";
                                        return Json(new { dom }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            fm.Dependent_Id = mydata.Dependent_Id;
                            //chnges 07-03-2018//
                            fm.Wife_recorded_in_PPO_no = mydata.Wife_recorded_in_PPO_no;
                            fm.Wife_OtherName = mydata.Wife_othername;

                            fm.Number_Of_children = Convert.ToInt16(mydata.Number_Of_children);
                            fm.Recorded_in_DO_Part2 = mydata.Recorded_in_DO_Part2;
                            fm.Recorded_in_DO_Part2text = mydata.Recorded_in_DO_Part2text;
                            //end
                            dta.tblfamilydetails_citizens.InsertOnSubmit(fm);

                            dta.SubmitChanges();

                            ViewBag.Message = "saved";

                            Session["Idd"] = fm.Dependent_Id;


                            return Json(new { fm }, JsonRequestBehavior.AllowGet);

                        }

                    }
                }
                catch (Exception ex)
                {
                    // throw ex;
                    //changes on 13-03-2018
                    mydata.Error = ex.Message;
                    mydata.PageError = "/View/RSBGeneralInfrm";
                    mydata.DateError = DateTime.Now;
                    mydata.DetailError = ex.ToString();
                    tblTraceError error = new tblTraceError();
                    error.Error = mydata.Error;
                    error.PageError = mydata.PageError;
                    error.DateError = mydata.DateError;
                    error.DetailError = mydata.DetailError;

                    ed.tblTraceErrors.InsertOnSubmit(error);
                    ed.SubmitChanges();
                    //end
                    return Json(JsonRequestBehavior.AllowGet);
                }
                return Json(JsonRequestBehavior.AllowGet);
            }




        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep3_citizen(Generalform mydata)
        {
            try
            {
                //bool check = dta.sanik_otherinformation_citizens.Any(tbl => tbl.Army_No == mydata.Army_No);
                //if (check == true)
                //{
                //    string output = "exist";
                //    return Json(new { output }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                    int sr = 1;
                    sanik_otherinformation_citizen other = new sanik_otherinformation_citizen();
                    if (Session["Armyno"] != null)
                    {
                        other.Army_No = Session["Armyno"].ToString();
                    }
                    else
                    {
                        other.Army_No = mydata.Army_No;
                    }

                    other.Character_Id = mydata.Character_Id;
                    other.medical_ID = mydata.medical_ID;
                    other.Rank_ID = mydata.Rank_ID;
                    var nn = mydata.RetirementDate;

                    //new 03-03-2018//
                    if (nn != null)
                    {
                        DateTime ValidDate;
                        //DateTime? ValidDate2;

                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                        {
                            other.RetirementDate = ValidDate;
                        }
                    }
                    else
                    {
                        other.RetirementDate = null;
                    }

                    other.Force_Cat_ID = mydata.Force_Cat_ID;
                    other.Force_Dept_Id = mydata.Force_Dept_Id;
                 
                    dta.sanik_otherinformation_citizens.InsertOnSubmit(other);
                    sr = sr + 1;
                    dta.SubmitChanges();

                    mydata.Error = Convert.ToString(sr);
                    mydata.PageError = "/View/RSBGeneralInfrm/Otherinformation";
                    mydata.DateError = DateTime.Now;
                    mydata.DetailError = null;
                    tblTraceError error = new tblTraceError();
                    error.Error = mydata.Error;
                    error.PageError = mydata.PageError;
                    error.DateError = mydata.DateError;
                    error.DetailError = mydata.DetailError;

                    ed.tblTraceErrors.InsertOnSubmit(error);
                    ed.SubmitChanges();
                    return Json(other, JsonRequestBehavior.AllowGet);
              //  }




            }
            catch (Exception ex)
            {
                mydata.Error = ex.Message;
                mydata.PageError = "/View/RSBGeneralInfrm/Otherinformation";
                mydata.DateError = DateTime.Now;
                mydata.DetailError = ex.ToString();
                tblTraceError error = new tblTraceError();
                error.Error = mydata.Error;
                error.PageError = mydata.PageError;
                error.DateError = mydata.DateError;
                error.DetailError = mydata.DetailError;

                ed.tblTraceErrors.InsertOnSubmit(error);
                ed.SubmitChanges();
                //end
                return Json(JsonRequestBehavior.AllowGet);

            }

        }
        ////update step3
        //public JsonResult updatestep3_citizen(Generalform mydata)
        //{
        //    try
        //    {


        //        sanik_otherinformation_citizen other = dta.sanik_otherinformation_citizens.Where(x => x.Army_No == (string)Session["armynoalrd"] || x.Army_No == (string)Session["Armyno"]).FirstOrDefault();
        //        if (other != null)
        //        {
        //            int sr = 1;
        //            other.Army_No = mydata.Army_No;
        //            other.Character_Id = mydata.Character_Id;
        //            other.medical_ID = mydata.medical_ID;
        //            other.Rank_ID = mydata.Rank_ID;
        //            var nn = mydata.RetirementDate;

        //            //new 03-03-2018//
        //            if (nn != null)
        //            {
        //                DateTime ValidDate;
        //                //DateTime? ValidDate2;

        //                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
        //                {
        //                    other.RetirementDate = ValidDate;
        //                }
        //            }
        //            else
        //            {
        //                other.RetirementDate = null;
        //            }
        //            other.Force_Cat_ID = mydata.Force_Cat_ID;
        //            other.Force_Dept_Id = mydata.Force_Dept_Id;
        //            sr = sr + 1;
        //            dta.SubmitChanges();



        //            mydata.Error = Convert.ToString(sr);
        //            mydata.PageError = "/View/RSBGeneralInfrm/Otherinformation";
        //            mydata.DateError = DateTime.Now;
        //            mydata.DetailError = null;
        //            tblTraceError error = new tblTraceError();
        //            error.Error = mydata.Error;
        //            error.PageError = mydata.PageError;
        //            error.DateError = mydata.DateError;
        //            error.DetailError = mydata.DetailError;

        //            ed.tblTraceErrors.InsertOnSubmit(error);
        //            ed.SubmitChanges();
        //            return Json(other, JsonRequestBehavior.AllowGet);


        //        }
        //        else
        //        {
        //            string msg1 = "not";
        //            return Json(msg1, JsonRequestBehavior.AllowGet);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        mydata.Error = ex.Message;
        //        mydata.PageError = "/View/RSBGeneralInfrm/Otherinformation";
        //        mydata.DateError = DateTime.Now;
        //        mydata.DetailError = ex.ToString();
        //        tblTraceError error = new tblTraceError();
        //        error.Error = mydata.Error;
        //        error.PageError = mydata.PageError;
        //        error.DateError = mydata.DateError;
        //        error.DetailError = mydata.DetailError;

        //        ed.tblTraceErrors.InsertOnSubmit(error);
        //        ed.SubmitChanges();
        //        //end
        //        return Json(JsonRequestBehavior.AllowGet);
        //    }

        //}
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep4_citizen(Generalform mydata, string IsEditcort)
        {
            string msg;
            bool caseno, caseyear, courtname;


            caseno = IsNumber(mydata.Case_No);
            caseyear = IsNumber(mydata.Case_Year);
            courtname = IsName(mydata.Court_Name);
            if (caseyear == false || caseno == false || courtname == false)
            {

                if (caseyear == false)
                {
                    msg = "cyear";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (caseno == false)
                {
                    msg = "cse";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (courtname == false)
                {
                    msg = "crt";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                try
                {

                    Force_Court_Case_citizen crt = new Force_Court_Case_citizen();
                    if (Session["Armyno"] != null)
                    {
                        crt.Army_No = Session["Armyno"].ToString();
                    }
                    else
                    {
                        crt.Army_No = mydata.Army_No;
                    }

                    crt.Case_No = mydata.Case_No;
                    crt.Case_Year = mydata.Case_Year;
                    crt.Court_Name = mydata.Court_Name;
                    crt.Decision = mydata.Decision;
                    dta.Force_Court_Case_citizens.InsertOnSubmit(crt);

                    dta.SubmitChanges();

                    ViewBag.Message = "saved";

                    return Json(JsonRequestBehavior.AllowGet);



                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            return Json(JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep5_citizen(Generalform mydata, string IsEditcomplain)
        {
            string msg;
            bool nameofcmpln, Levelofdecision, Pending_With;


            Levelofdecision = Citizen(mydata.Level_of_decision);
            Pending_With = IsName(mydata.Pending_With);
            nameofcmpln = IsName(mydata.Name_of_Complain);
            if (Levelofdecision == false || nameofcmpln == false || Pending_With == false)
            {

                if (Levelofdecision == false)
                {
                    msg = "lvlofdecsn";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (Pending_With == false)
                {
                    msg = "pndwth";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (nameofcmpln == false)
                {
                    msg = "nmecmpln";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {

                    Force_Complaints_citizen cmp = new Force_Complaints_citizen();
                    if (Session["Armyno"] != null)
                    {
                        cmp.Army_No = Session["Armyno"].ToString();
                    }
                    else
                    {
                        cmp.Army_No = mydata.Army_No;
                    }

                    cmp.Name_of_Complain = mydata.Name_of_Complain;
                    cmp.Level_of_decision = mydata.Level_of_decision;
                    if (mydata.Date_of_Complain != null)
                    {
                        var nn = mydata.Date_of_Complain;

                        //new 03-03-2018//
                        if (nn != null)
                        {
                            DateTime ValidDate;
                            //DateTime? ValidDate2;

                            if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                            {
                                cmp.Date_of_Complain = ValidDate;
                            }
                        }
                        else
                        {
                            cmp.Date_of_Complain = null;
                        }
                    }

                    cmp.Pending_With = mydata.Pending_With;
                    cmp.Decision_Given = mydata.Decision_Given;

                    dta.Force_Complaints_citizens.InsertOnSubmit(cmp);

                    dta.SubmitChanges();

                    ViewBag.Message = "saved";






                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return Json(JsonRequestBehavior.AllowGet);
            }



        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep6_citizen(Generalform mydata)
        {
            string msg;
            bool outstandingamount, loanamount;


            outstandingamount = Decimalchk(Convert.ToString(mydata.Outstanding_Amount));
            loanamount = Decimalchk(Convert.ToString(mydata.Loan_Amount));

            if (loanamount == false || outstandingamount == false)
            {

                if (outstandingamount == false)
                {
                    msg = "outstnding";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }
                else if (loanamount == false)
                {
                    msg = "loanamount";
                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
                }

                return Json(JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    Sanik_Loans_citizen loan = new Sanik_Loans_citizen();
                    if (Session["Armyno"] != null)
                    {
                        loan.Army_No = Session["Armyno"].ToString();
                    }
                    else
                    {
                        loan.Army_No = mydata.Army_No;
                    }

                    loan.Loan_Amount = mydata.Loan_Amount;
                    if (mydata.Date_loan != null)
                    {
                        var nn = mydata.Date_loan;

                        //new 03-03-2018//
                        if (nn != null)
                        {
                            DateTime ValidDate;
                            //DateTime? ValidDate2;

                            if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                            {
                                loan.Date_loan = ValidDate;
                            }
                            else
                            {
                                loan.Date_loan = null;
                            }
                        }
                    }
                    //loan.Date_loan = mydata.Date_loan;
                    loan.Purpose = mydata.Purpose;
                    loan.Remarks = mydata.Remarks;
                    if (mydata.Outstanding_Amount <= mydata.Loan_Amount)
                    {
                        loan.Outstanding_Amount = mydata.Outstanding_Amount;
                        dta.Sanik_Loans_citizens.InsertOnSubmit(loan);
                        dta.SubmitChanges();

                    }
                    else
                    {
                        string output = "Enter wrong amount";
                        return Json(new { output }, JsonRequestBehavior.AllowGet);
                    }


                    ViewBag.Message = "saved";

                    return Json(JsonRequestBehavior.AllowGet);




                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }


        }
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep7_citizen(Generalform mydata)
        {
            rsbgeneralUPDataContext dtaNew = new rsbgeneralUPDataContext();
            try
            {
                bool check = dtaNew.rsbgenerals.Any(tbl => tbl.Army_No == mydata.Army_No && tbl.Bank_Acc_no != null);
                //if (check == true)
                //{
                //    string output = "exist";
                //    return Json(new { output }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    //********************************//
                    using (SqlConnection con = new SqlConnection(Constrg1))
                    {
                        string arm = Convert.ToString(Session["Armyno"]);
                        SqlCommand cmd1 = new SqlCommand("update rsbgeneral_citizen1 set BankID=@BankID,Bank_Acc_no=@Bank_Acc_no,Bank_IFSC=@Bank_IFSC where Army_No=@Army_No OR Army_No='" + arm + "'", con);
                        if (cmd1 != null)
                        {
                            cmd1.Parameters.AddWithValue("@Army_No", mydata.Army_No);
                            cmd1.Parameters.AddWithValue("@BankID", mydata.BankID);

                            cmd1.Parameters.AddWithValue("@Bank_Acc_no", mydata.Bank_Acc_no);
                            if (mydata.Bank_IFSC == null)
                            {
                                cmd1.Parameters.AddWithValue("@Bank_IFSC", DBNull.Value);
                            }
                            else
                            {
                                cmd1.Parameters.AddWithValue("@Bank_IFSC", mydata.Bank_IFSC);
                            }
                            // cmd1.Parameters.AddWithValue("@Bank_IFSC", mydata.Bank_IFSC);
                            con.Open();
                            cmd1.ExecuteNonQuery();
                            con.Close();
                        }
                        //****************************//

                       // rsbgeneral rsb1 = new rsbgeneral();
                        // rsb1 = dtaNew.rsbgenerals.Where(x => x.Army_No == mydata.Army_No).FirstOrDefault();

                       //// var query = from rs in dta.rsbgenerals where rs.Army_No == mydata.Army_No  select rs;
                        // if (rsb1 != null)
                        // {


                       //     rsb1.BankID = mydata.BankID;
                        //     rsb1.Bank_Acc_no = mydata.Bank_Acc_no;
                        //     rsb1.Bank_IFSC = mydata.Bank_IFSC;

                       //        // dta.Refresh(RefreshMode.KeepChanges, rsb1);
                        //         dtaNew.SubmitChanges();


                       // }


                        else
                        {
                            string msg = "no record corresponding to this army number";
                            return Json(msg, JsonRequestBehavior.AllowGet);
                        }

                    }

                    return Json(JsonRequestBehavior.AllowGet);

               // }

            }
            catch (Exception ex)
            {
                mydata.Error = ex.Message;
                mydata.PageError = "/View/RSBGeneralInfrm/BAnk";
                mydata.DateError = DateTime.Now;
                mydata.DetailError = ex.ToString();
                tblTraceError error = new tblTraceError();
                error.Error = mydata.Error;
                error.PageError = mydata.PageError;
                error.DateError = mydata.DateError;
                error.DetailError = mydata.DetailError;

                ed.tblTraceErrors.InsertOnSubmit(error);
                ed.SubmitChanges();
                //end
                return Json(JsonRequestBehavior.AllowGet);
            }

        }

        //public JsonResult updatestep7_citizen(Generalform mydata)
        //{
        //    try
        //    {

        //        using (SqlConnection con1 = new SqlConnection(Constrg1))
        //        {
        //            string arm = Convert.ToString(Session["Armyno"]);

        //            string str = "update rsbgeneral_citizen set BankID=@BankID,Bank_Acc_no=@Bank_Acc_no,Bank_IFSC=@Bank_IFSC where Army_No=@Army_No or Army_No='" + arm + "' ";
        //            SqlCommand cmd2 = new SqlCommand(str, con1);
        //            if (cmd2 != null)
        //            {
        //                cmd2.Parameters.AddWithValue("@Army_No", mydata.Army_No);
        //                cmd2.Parameters.AddWithValue("@BankID", mydata.BankID);
        //                if (mydata.Bank_IFSC == null)
        //                {
        //                    cmd2.Parameters.AddWithValue("@Bank_IFSC", DBNull.Value);
        //                }
        //                else
        //                {
        //                    cmd2.Parameters.AddWithValue("@Bank_IFSC", mydata.Bank_IFSC);
        //                }
        //                cmd2.Parameters.AddWithValue("@Bank_Acc_no", mydata.Bank_Acc_no);

        //                con1.Open();
        //                cmd2.ExecuteNonQuery();
        //                con1.Close();
        //            }
        //            //****************************//

        //           // rsbgeneral rsb1 = new rsbgeneral();
        //            // rsb1 = dtaNew.rsbgenerals.Where(x => x.Army_No == mydata.Army_No).FirstOrDefault();

        //           //// var query = from rs in dta.rsbgenerals where rs.Army_No == mydata.Army_No  select rs;
        //            // if (rsb1 != null)
        //            // {


        //           //     rsb1.BankID = mydata.BankID;
        //            //     rsb1.Bank_Acc_no = mydata.Bank_Acc_no;
        //            //     rsb1.Bank_IFSC = mydata.Bank_IFSC;

        //           //        // dta.Refresh(RefreshMode.KeepChanges, rsb1);
        //            //         dtaNew.SubmitChanges();


        //           // }


        //            else
        //            {
        //                string msg = "no record  corresponding to this army number";
        //                return Json(msg, JsonRequestBehavior.AllowGet);
        //            }

        //        }




        //        return Json(JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}
        [HttpPost]
        public JsonResult Sanik_General_Registrationstep8_citizen(Generalform mydata)
        {
            try
            {

                Sanik_award_citizen awd = new Sanik_award_citizen();
                if ((Session["Armyno"]) != null)
                {
                    awd.Army_No = Session["Armyno"].ToString();
                }
                else
                {
                    awd.Army_No = mydata.Army_No;
                }

                awd.awardID = mydata.awardID;
                if (mydata.award_date == null)
                {
                    awd.award_date = null;
                }
                else
                {
                    var nn = mydata.award_date;

                    //new 03-03-2018//
                    if (nn != null)
                    {
                        DateTime ValidDate;
                        //DateTime? ValidDate2;

                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
                        {
                            awd.award_date = ValidDate;
                        }
                        else
                        {
                            awd.award_date = null;
                        }
                    }

                    //awd.award_date = mydata.award_date;
                }

                awd.Perpose = mydata.Perpose;
                dta.Sanik_award_citizens.InsertOnSubmit(awd);

                dta.SubmitChanges();

                return Json(JsonRequestBehavior.AllowGet);





            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        ////*****************areadycases**************** case8//
        //public JsonResult RSBawardalready_citizen(string amno)
        //{

        //    List<Generalform> cd = new List<Generalform>();
        //    var _test3 = (from ln in dta.Sanik_award_citizens

        //                  join rb in dta.rsbgeneral_citizens on ln.Army_No equals rb.Army_No
        //                  join awd in dta.tblawards on ln.awardID equals awd.awardID
        //                  where (ln.Army_No == amno)
        //                  select new Generalform
        //                  {
        //                      Army_No = ln.Army_No,
        //                      awardName = awd.awardName,
        //                      award_date1 = ln.award_date,
        //                      Sanikawrdid = ln.Sanikawrdid,
        //                      Perpose = ln.Perpose,

        //                  }).ToList();
        //    if (_test3.Count() > 0)
        //    {
        //        foreach (var item in _test3)
        //        {
        //            Generalform gn = new Generalform();
        //            gn.Army_No = item.Army_No;
        //            gn.awardID = item.awardID;
        //            if (item.award_date1 != null)
        //            {
        //                gn.award_date1 = item.award_date1;
        //            }
        //            else
        //            {
        //                string datee = "01/01/0001";
        //                gn.award_date1 = Convert.ToDateTime(datee);
        //            }

        //            gn.Sanikawrdid = item.Sanikawrdid;
        //            gn.Perpose = item.Perpose;
        //            cd.Add(gn);
        //        }
        //        return Json(_test3, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        string output = "empty";
        //        return Json(new { output }, JsonRequestBehavior.AllowGet);
        //    }



        //}
        // [HttpPost]
        //public JsonResult Editaward1_citizen(int id)
        //{
        //    try
        //    {
        //        Generalform gn = new Generalform();
        //        List<Generalform> cd = new List<Generalform>();

        //        var _ms2 = (from fm in dta.Sanik_award_citizens
        //                    where fm.Sanikawrdid == id
        //                    select new Generalform
        //                    {
        //                        Sanikawrdid = fm.Sanikawrdid,
        //                        Army_No = fm.Army_No,
        //                        awardID = fm.awardID,
        //                        award_date1 = fm.award_date,
        //                        Perpose = fm.Perpose,

        //                    }).ToList();
        //        if (_ms2.Count() > 0)
        //        {

        //            foreach (var item in _ms2)
        //            {
        //                gn.Sanikawrdid = item.Sanikawrdid;
        //                gn.Army_No = item.Army_No;
        //                gn.awardID = item.awardID;
        //                if (item.award_date1 != null)
        //                {
        //                    gn.award_date1 = Convert.ToDateTime(item.award_date1);

        //                }
        //                else
        //                {
        //                    string datee = "01/01/0001";
        //                    gn.award_date1 = Convert.ToDateTime(datee);
        //                }

        //                gn.Perpose = item.Perpose;


        //                cd.Add(gn);
        //            }
        //            return Json(cd, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;


        //    }
        //}
        // public JsonResult Editaward_citizen(Generalform mydata, int id1)
        // {
        //     try
        //     {

        //         Sanik_award_citizen sc = dta.Sanik_award_citizens.Where(x => x.Sanikawrdid == id1).FirstOrDefault();
        //         if (sc != null)
        //         {
        //             sc.Army_No = mydata.Army_No;
        //             sc.awardID = mydata.awardID;
        //             if (mydata.award_date == null)
        //             {
        //                 sc.award_date = null;
        //             }
        //             else
        //             {
        //                 var nn = mydata.award_date;

        //                 //new 03-03-2018//
        //                 if (nn != null)
        //                 {
        //                     DateTime ValidDate;
        //                     //DateTime? ValidDate2;

        //                     if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
        //                     {
        //                         sc.award_date = ValidDate;
        //                     }
        //                     else
        //                     {
        //                         sc.award_date = null;
        //                     }
        //                 }
        //             }

        //             sc.Perpose = mydata.Perpose;
        //             dta.SubmitChanges();

        //             return Json(sc, JsonRequestBehavior.AllowGet);
        //         }
        //         else
        //         {
        //             string msg1 = "not";
        //             return Json(msg1, JsonRequestBehavior.AllowGet);
        //         }

        //     }
        //     catch (Exception ex)
        //     {
        //         throw ex;


        //     }
        // }
        public JsonResult RSBaward_citizen()
        {
            List<Generalform> cd = new List<Generalform>();
            var _test3 = (from ln in dta.Sanik_award_citizens

                          join rb in dta.rsbgeneral_citizen1s on ln.Army_No equals rb.Army_No
                          join awd in dta.tblawards on ln.awardID equals awd.awardID
                          where (ln.Army_No == Convert.ToString(Session["Armyno"]))
                          select new Generalform
                          {
                              Army_No = ln.Army_No,
                              awardName = awd.awardName,

                              award_date1 = ln.award_date,
                              Sanikawrdid = ln.Sanikawrdid,
                              Perpose = ln.Perpose,

                          }).ToList();
            if (_test3.Count() > 0)
            {
                foreach (var item in _test3)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.awardID = item.awardID;
                    if (item.award_date1 != null)
                    {
                        gn.award_date1 = item.award_date1;
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.award_date1 = Convert.ToDateTime(datee);
                    }

                    gn.Sanikawrdid = item.Sanikawrdid;
                    gn.Perpose = item.Perpose;
                    cd.Add(gn);
                }
                return Json(_test3, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonRequestBehavior.AllowGet);
            }



        }
        //[HttpPost]
        //public JsonResult Deleteaward_citizen(Generalform model, int id)
        //{
        //    try
        //    {
        //        var m = from award in dta.Sanik_award_citizens where award.Sanikawrdid == id select award;
        //        foreach (var detail in m)
        //        {
        //            dta.Sanik_award_citizens.DeleteOnSubmit(detail);
        //        }



        //        dta.SubmitChanges();
        //        var data = from award in dta.Sanik_award_citizens where award.Army_No == Session["armynoalrd"] select award;
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        ////*********end*************//

        ////*************case6**************//
        //public JsonResult RSBLoanalready_citizen(string amno)
        //{

        //    List<Generalform> cd = new List<Generalform>();
        //    var _test3 = (from ln in dta.Sanik_Loans_citizens

        //                  join rb in dta.rsbgeneral_citizens on ln.Army_No equals rb.Army_No
        //                  where (ln.Army_No == amno)
        //                  select new Generalform
        //                  {
        //                      Army_No = ln.Army_No,
        //                      Loan_id = ln.Loan_Id,
        //                      Loan_Amount = ln.Loan_Amount,
        //                      Date_loan1 = ln.Date_loan,
        //                      Purpose = ln.Purpose,
        //                      Outstanding_Amount = ln.Outstanding_Amount,
        //                      Remarks = ln.Remarks


        //                  }).ToList();
        //    if (_test3.Count() > 0)
        //    {

        //        foreach (var item in _test3)
        //        {
        //            Generalform gn = new Generalform();
        //            gn.Army_No = item.Army_No;
        //            gn.Loan_id = item.Loan_id;
        //            gn.Loan_Amount = item.Loan_Amount;
        //            if (item.Date_loan1 != null)
        //            {
        //                gn.Date_loan1 = item.Date_loan1;
        //            }
        //            else
        //            {
        //                string datee = "01/01/0001";
        //                gn.Date_loan1 = Convert.ToDateTime(datee);
        //            }

        //            gn.Purpose = item.Purpose;
        //            gn.Outstanding_Amount = item.Outstanding_Amount;
        //            gn.Remarks = item.Remarks;


        //            cd.Add(gn);
        //        }
        //        return Json(cd, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        string output = "empty";
        //        return Json(new { output }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public JsonResult RSBLoan_citizen()
        {

            List<Generalform> cd = new List<Generalform>();
            var _test3 = (from ln in dta.Sanik_Loans_citizens

                          join rb in dta.rsbgeneral_citizen1s on ln.Army_No equals rb.Army_No
                          where (ln.Army_No == Convert.ToString(Session["Armyno"]))
                          select new Generalform
                          {
                              Army_No = ln.Army_No,
                              Loan_id = ln.Loan_Id,
                              Loan_Amount = ln.Loan_Amount,
                              Date_loan1 = ln.Date_loan,
                              Purpose = ln.Purpose,
                              Outstanding_Amount = ln.Outstanding_Amount,
                              Remarks = ln.Remarks


                          }).ToList();
            if (_test3.Count() > 0)
            {

                foreach (var item in _test3)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.Loan_id = item.Loan_id;
                    gn.Loan_Amount = item.Loan_Amount;
                    if (item.Date_loan1 != null)
                    {
                        gn.Date_loan1 = item.Date_loan1;
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Date_loan1 = Convert.ToDateTime(datee);
                    }

                    gn.Purpose = item.Purpose;
                    gn.Outstanding_Amount = item.Outstanding_Amount;
                    gn.Remarks = item.Remarks;


                    cd.Add(gn);
                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonRequestBehavior.AllowGet);
            }



        }
        //public JsonResult EditLoan1_citizen(int id)
        //{
        //    try
        //    {
        //        Generalform gn = new Generalform();
        //        List<Generalform> cd = new List<Generalform>();

        //        var _ms2 = (from fm in dta.Sanik_Loans_citizens
        //                    where fm.Loan_Id == id
        //                    select new Generalform
        //                    {
        //                        Loan_id = fm.Loan_Id,
        //                        Army_No = fm.Army_No,
        //                        Loan_Amount = fm.Loan_Amount,
        //                        Purpose = fm.Purpose,
        //                        Outstanding_Amount = fm.Outstanding_Amount,
        //                        Date_loan1 = fm.Date_loan,
        //                        Remarks = fm.Remarks

        //                    }).ToList();
        //        if (_ms2.Count() > 0)
        //        {
        //            foreach (var item in _ms2)
        //            {
        //                gn.Loan_id = item.Loan_id;
        //                gn.Army_No = item.Army_No;
        //                gn.Loan_Amount = item.Loan_Amount;
        //                gn.Purpose = item.Purpose;
        //                gn.Outstanding_Amount = item.Outstanding_Amount;
        //                if (item.Date_loan1 != null)
        //                {
        //                    gn.Date_loan1 = item.Date_loan1;

        //                }
        //                else
        //                {
        //                    string datee = "01/01/0001";
        //                    gn.Date_loan1 = Convert.ToDateTime(datee);

        //                }
        //                gn.Remarks = item.Remarks;
        //                cd.Add(gn);
        //            }
        //            return Json(cd, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(JsonRequestBehavior.AllowGet);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;


        //    }
        //}
        //public JsonResult Editloan_citizen(Generalform mydata, int id1)
        //{
        //    string msg;
        //    bool outstandingamount, loanamount;


        //    outstandingamount = Decimalchk(Convert.ToString(mydata.Outstanding_Amount));
        //    loanamount = Decimalchk(Convert.ToString(mydata.Loan_Amount));

        //    if (loanamount == false || outstandingamount == false)
        //    {

        //        if (outstandingamount == false)
        //        {
        //            msg = "outstnding";
        //            return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        else if (loanamount == false)
        //        {
        //            msg = "loanamount";
        //            return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //        }

        //        return Json(JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        try
        //        {

        //            Sanik_Loans_citizen sc = dta.Sanik_Loans_citizens.Where(x => x.Loan_Id == id1).FirstOrDefault();
        //            if (sc != null)
        //            {
        //                sc.Army_No = mydata.Army_No;
        //                sc.Loan_Amount = mydata.Loan_Amount;
        //                sc.Purpose = mydata.Purpose;
        //                sc.Outstanding_Amount = mydata.Outstanding_Amount;
        //                ///////////////
        //                if (mydata.Date_loan != null)
        //                {
        //                    var nn = mydata.Date_loan;

        //                    //new 03-03-2018//
        //                    if (nn != null)
        //                    {
        //                        DateTime ValidDate;
        //                        //DateTime? ValidDate2;

        //                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
        //                        {
        //                            sc.Date_loan = ValidDate;
        //                        }
        //                        else
        //                        {
        //                            sc.Date_loan = null;
        //                        }
        //                    }
        //                }
        //                //sc.Date_loan = mydata.Date_loan;
        //                sc.Remarks = mydata.Remarks;
        //                dta.SubmitChanges();

        //                return Json(JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {
        //                string msg1 = "not";
        //                return Json(msg1, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;


        //        }
        //    }

        //}
        //[HttpPost]
        //public JsonResult Deleteloan_citizen(Generalform model, int id)
        //{
        //    try
        //    {
        //        var m = from loan in dta.Sanik_Loans_citizens where loan.Loan_Id == id select loan;
        //        foreach (var detail in m)
        //        {
        //            dta.Sanik_Loans_citizens.DeleteOnSubmit(detail);
        //        }



        //        dta.SubmitChanges();
        //        var data = from loan in dta.Sanik_Loans_citizens where loan.Army_No == Session["armynoalrd"] select loan;
        //        return Json(data, JsonRequestBehavior.AllowGet); ;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        ////*********end******************//
        ////**************case5***************//
        //public JsonResult RSBComplainalready_citizen(string amno)
        //{

        //    List<Generalform> cd = new List<Generalform>();

        //    var _test2 = (from forc in dta.Force_Complaints_citizens

        //                  join rb in dta.rsbgeneral_citizens on forc.Army_No equals rb.Army_No
        //                  where (forc.Army_No == amno)
        //                  select new Generalform
        //                  {
        //                      Army_No = forc.Army_No,
        //                      Complain_Id = forc.Complain_Id,
        //                      Name_of_Complain = forc.Name_of_Complain,
        //                      Level_of_decision = forc.Level_of_decision,
        //                      Date_of_Complain1 = forc.Date_of_Complain,
        //                      Pending_With = forc.Pending_With,
        //                      Decision_Given = forc.Decision_Given

        //                  }).ToList();

        //    if (_test2.Count() > 0)
        //    {
        //        foreach (var item in _test2)
        //        {
        //            Generalform gn = new Generalform();
        //            gn.Army_No = item.Army_No;
        //            gn.Complain_Id = item.Complain_Id;
        //            gn.Name_of_Complain = item.Name_of_Complain;
        //            gn.Level_of_decision = item.Level_of_decision;
        //            if (item.Date_of_Complain1 != null)
        //            {
        //                gn.Date_of_Complain1 = Convert.ToDateTime(item.Date_of_Complain1);
        //            }
        //            else
        //            {
        //                string datee = "01/01/0001";
        //                gn.Date_of_Complain1 = Convert.ToDateTime(datee);
        //            }

        //            gn.Pending_With = item.Pending_With;
        //            gn.Decision_Given = item.Decision_Given;
        //            cd.Add(gn);
        //        }
        //        return Json(cd, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        string output = "empty";
        //        return Json(new { output }, JsonRequestBehavior.AllowGet);
        //    }

        //}
        public JsonResult RSBComplain_citizen()
        {
            List<Generalform> cd = new List<Generalform>();

            var _test2 = (from forc in dta.Force_Complaints_citizens

                          join rb in dta.rsbgeneral_citizen1s on forc.Army_No equals rb.Army_No
                          where (forc.Army_No == Convert.ToString(Session["Armyno"]))
                          select new Generalform
                          {
                              Army_No = forc.Army_No,
                              Complain_Id = forc.Complain_Id,
                              Name_of_Complain = forc.Name_of_Complain,
                              Level_of_decision = forc.Level_of_decision,
                              Date_of_Complain1 = forc.Date_of_Complain,
                              Pending_With = forc.Pending_With,
                              Decision_Given = forc.Decision_Given

                          }).ToList();

            if (_test2.Count() > 0)
            {
                foreach (var item in _test2)
                {
                    Generalform gn = new Generalform();
                    gn.Army_No = item.Army_No;
                    gn.Complain_Id = item.Complain_Id;
                    gn.Name_of_Complain = item.Name_of_Complain;
                    gn.Level_of_decision = item.Level_of_decision;
                    if (item.Date_of_Complain1 != null)
                    {
                        gn.Date_of_Complain1 = Convert.ToDateTime(item.Date_of_Complain1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Date_of_Complain1 = Convert.ToDateTime(datee);
                    }

                    gn.Pending_With = item.Pending_With;
                    gn.Decision_Given = item.Decision_Given;
                    cd.Add(gn);
                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonRequestBehavior.AllowGet);
            }


        }
        //[HttpPost]
        //public JsonResult Deletecomplain_citizen(Generalform model, int id)
        //{
        //    try
        //    {
        //        var m = from complain in dta.Force_Complaints_citizens where complain.Complain_Id == id select complain;
        //        foreach (var detail in m)
        //        {
        //            dta.Force_Complaints_citizens.DeleteOnSubmit(detail);
        //        }



        //        dta.SubmitChanges();
        //        var data = from complain in dta.Force_Complaints_citizens where complain.Army_No == Session["armynoalrd"] select complain;
        //        return Json(data, JsonRequestBehavior.AllowGet); ;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //public JsonResult Editcomplain1_citizen(int id)
        //{
        //    try
        //    {
        //        Generalform gn = new Generalform();
        //        List<Generalform> cd = new List<Generalform>();

        //        var _ms2 = (from fm in dta.Force_Complaints_citizens
        //                    where fm.Complain_Id == id
        //                    select new Generalform
        //                    {
        //                        Complain_Id = fm.Complain_Id,
        //                        Army_No = fm.Army_No,
        //                        Name_of_Complain = fm.Name_of_Complain,
        //                        Level_of_decision = fm.Level_of_decision,
        //                        Date_of_Complain1 = fm.Date_of_Complain,
        //                        Pending_With = fm.Pending_With,
        //                        Decision_Given = fm.Decision_Given

        //                    }).ToList();

        //        if (_ms2.Count() > 0)
        //        {
        //            foreach (var item in _ms2)
        //            {
        //                gn.Complain_Id = item.Complain_Id;
        //                gn.Army_No = item.Army_No;
        //                gn.Name_of_Complain = item.Name_of_Complain;
        //                gn.Level_of_decision = item.Level_of_decision;
        //                if (item.Date_of_Complain1 != null)
        //                {
        //                    gn.Date_of_Complain1 = item.Date_of_Complain1;

        //                }
        //                else
        //                {
        //                    string datee = "01/01/0001";
        //                    gn.Date_of_Complain1 = Convert.ToDateTime(datee);
        //                }

        //                gn.Pending_With = item.Pending_With;
        //                gn.Decision_Given = item.Decision_Given;
        //                cd.Add(gn);
        //            }
        //            return Json(cd, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;


        //    }
        //}
        //public JsonResult Editcomplain_citizen(Generalform mydata, int id1)
        //{
        //    try
        //    {

        //        Force_Complaints_citizen sc = dta.Force_Complaints_citizens.Where(x => x.Complain_Id == id1).FirstOrDefault();
        //        if (sc != null)
        //        {
        //            string msg;
        //            bool nameofcmpln, Levelofdecision, Pending_With;
        //            Levelofdecision = Citizen(mydata.Level_of_decision);
        //            Pending_With = IsName(mydata.Pending_With);
        //            nameofcmpln = IsName(mydata.Name_of_Complain);
        //            if (Levelofdecision == false || nameofcmpln == false || Pending_With == false)
        //            {

        //                if (Levelofdecision == false)
        //                {
        //                    msg = "lvlofdecsn";
        //                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                else if (Pending_With == false)
        //                {
        //                    msg = "pndwth";
        //                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                else if (nameofcmpln == false)
        //                {
        //                    msg = "nmecmpln";
        //                    return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                return Json(JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {
        //                sc.Name_of_Complain = mydata.Name_of_Complain;
        //                sc.Level_of_decision = mydata.Level_of_decision;
        //                if (mydata.Date_of_Complain != null)
        //                {
        //                    var nn = mydata.Date_of_Complain;

        //                    //new 03-03-2018//
        //                    if (nn != null)
        //                    {
        //                        DateTime ValidDate;
        //                        //DateTime? ValidDate2;

        //                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
        //                        {
        //                            sc.Date_of_Complain = ValidDate;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        sc.Date_of_Complain = null;
        //                    }
        //                }
        //                //sc.Date_of_Complain = mydata.Date_of_Complain;
        //                sc.Pending_With = mydata.Pending_With;
        //                sc.Decision_Given = mydata.Decision_Given;
        //                dta.SubmitChanges();
        //                // }
        //                return Json(JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        else
        //        {
        //            string msg1 = "not";
        //            return Json(msg1, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;


        //    }

        //}
        ////************end*********************//
        ////****************case4****************//
        //public JsonResult RSBCourtalready_citizen(string amno)
        //{


        //    Force_Court_Case fr = new Force_Court_Case();
        //    List<Generalform> _test1 = null;


        //    _test1 = (from force in dta.Force_Court_Case_citizens

        //              join rb in dta.rsbgeneral_citizens on force.Army_No equals rb.Army_No
        //              where (force.Army_No == amno)
        //              select new Generalform
        //              {
        //                  Army_No = force.Army_No,
        //                  Court_Case_Id = force.Court_Case_Id,
        //                  Case_No = force.Case_No,
        //                  Case_Year = force.Case_Year,
        //                  Court_Name = force.Court_Name,
        //                  Decision = force.Decision
        //              }).ToList();

        //    if (_test1.Count() > 0)
        //    {
        //        return Json(_test1, JsonRequestBehavior.AllowGet);
        //    }


        //    else
        //    {
        //        string output = "empty";
        //        return Json(new { output }, JsonRequestBehavior.AllowGet);
        //    }


        //}
        public JsonResult RSBCourt_citizen()
        {


            Force_Court_Case_citizen fr = new Force_Court_Case_citizen();
            List<Generalform> _test1 = null;


            _test1 = (from force in dta.Force_Court_Case_citizens

                      join rb in dta.rsbgeneral_citizen1s on force.Army_No equals rb.Army_No
                      where (force.Army_No == Convert.ToString(Session["Armyno"]))
                      select new Generalform
                      {
                          Army_No = force.Army_No,
                          Court_Case_Id = force.Court_Case_Id,
                          Case_No = force.Case_No,
                          Case_Year = force.Case_Year,
                          Court_Name = force.Court_Name,
                          Decision = force.Decision
                      }).ToList();

            return Json(_test1, JsonRequestBehavior.AllowGet);



        }
        //public JsonResult Editcourtcase_citizen(Generalform mydata, int id1)
        //{
        //    string msg;
        //    bool caseno, caseyear, courtname;


        //    caseno = IsNumber(mydata.Case_No);
        //    caseyear = IsNumber(mydata.Case_Year);
        //    courtname = IsName(mydata.Court_Name);
        //    if (caseyear == false || caseno == false || courtname == false)
        //    {

        //        if (caseyear == false)
        //        {
        //            msg = "cyear";
        //            return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        else if (caseno == false)
        //        {
        //            msg = "cse";
        //            return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        else if (courtname == false)
        //        {
        //            msg = "crt";
        //            return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return Json(JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        try
        //        {

        //            Force_Court_Case_citizen sc = dta.Force_Court_Case_citizens.Where(x => x.Court_Case_Id == id1).FirstOrDefault();
        //            if (sc != null)
        //            {
        //                sc.Case_No = mydata.Case_No;
        //                sc.Case_Year = mydata.Case_Year;
        //                sc.Court_Name = mydata.Court_Name;
        //                sc.Decision = mydata.Decision;

        //                dta.SubmitChanges();
        //            }
        //            else
        //            {
        //                string msg1 = "not";
        //                return Json(msg1, JsonRequestBehavior.AllowGet);
        //            }


        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;


        //        }
        //        return Json(JsonRequestBehavior.AllowGet);
        //    }


        //}
        //public JsonResult Editcourtcase1_citizen(int id)
        //{

        //    try
        //    {
        //        Generalform ms1 = new Generalform();


        //        List<Generalform> _ms2 = (from fm in dta.Force_Court_Case_citizens
        //                                  where fm.Court_Case_Id == id
        //                                  select new Generalform
        //                                  {
        //                                      Army_No = fm.Army_No,
        //                                      Case_No = fm.Case_No,
        //                                      Case_Year = fm.Case_Year,
        //                                      Court_Name = fm.Court_Name,
        //                                      Decision = fm.Decision,

        //                                  }).ToList();

        //        return Json(_ms2, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;


        //    }
        //}
        //[HttpPost]
        //public JsonResult Deletecourtcase_citizen(Generalform model, int id)
        //{
        //    try
        //    {
        //        var m = from court in dta.Force_Court_Case_citizens where court.Court_Case_Id == id select court;
        //        foreach (var detail in m)
        //        {
        //            dta.Force_Court_Case_citizens.DeleteOnSubmit(detail);
        //        }



        //        dta.SubmitChanges();
        //        var data = from court in dta.Force_Court_Case_citizens where court.Army_No == Session["armynoalrd"] select court;
        //        return Json(data, JsonRequestBehavior.AllowGet); ;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        ////*********end****************//
        ////********************case2********************//
        //[HttpPost]
        //public JsonResult RSBdependentupdate_citizen(string d)
        //{
        //    List<Generalform> cd = new List<Generalform>();
        //    var cd2 = (from c in dta.tblfamilydetails
        //               // join mrt in ed.tblMaritalStatusMasters on c.MaritalStatusCode equals mrt.MaritalStatusCode
        //               join rel in dta.tblsanikRelations on c.RelationCode equals rel.RelationCode
        //               where (c.UID == d)
        //               select new Generalform
        //               {
        //                   // MaritalStatusDesc = mrt.MaritalStatusDesc,
        //                   MaritalStatusCode = Convert.ToString(c.MaritalStatusCode),
        //                   RelationDesc = rel.RelationDesc,
        //                   Dependent_Id = c.Dependent_Id,
        //                   Army_No = c.Army_No,
        //                   Dependent_Name = c.Dependent_Name,
        //                   UID = c.UID,
        //                   Dependent_DOB1 = Convert.ToDateTime(c.DOB),
        //                   DOM1 = Convert.ToDateTime(c.DOM),


        //               }).ToList();
        //    var cd3 = from mat in dta.MaritalStatus
        //              select new Generalform
        //              {
        //                  MaritalStatusCode = Convert.ToString(mat.Marital_Code),
        //                  MaritalStatusDesc = mat.Marital_Status
        //              };
        //    foreach (var item1 in cd2)
        //    {

        //        if (item1.MaritalStatusCode != null)
        //        {
        //            var cd1 = (from f in cd2
        //                       join m in cd3 on f.MaritalStatusCode equals m.MaritalStatusCode

        //                       select new Generalform
        //                       {
        //                           MaritalStatusDesc = m.MaritalStatusDesc,
        //                           Dependent_Id = f.Dependent_Id,
        //                           Army_No = f.Army_No,
        //                           RelationDesc = f.RelationDesc,
        //                           Dependent_Name = f.Dependent_Name,
        //                           UID = f.UID,
        //                           Dependent_DOB1 = Convert.ToDateTime(f.Dependent_DOB),
        //                           DOM1 = Convert.ToDateTime(f.DOM),


        //                       }).ToList();


        //            if (cd1.Count() > 0)
        //            {

        //                string ImgByte;
        //                foreach (var item in cd1)
        //                {
        //                    Generalform gn = new Generalform();
        //                    var imgg = from c in dta.tblfamilydetails where ((c.Army_No == item.Army_No) && (c.UID == item.UID)) select c;
        //                    gn.Dependent_Id = item.Dependent_Id;
        //                    gn.Army_No = item.Army_No;
        //                    gn.Dependent_Name = item.Dependent_Name;
        //                    gn.RelationDesc = item.RelationDesc;
        //                    gn.UID = string.Concat("".PadLeft(9, '*'), item.UID.Substring(item.UID.Length - 4));

        //                    if (item1.Dependent_DOB != null)
        //                    {
        //                        gn.Dependent_DOB1 = Convert.ToDateTime(item.Dependent_DOB);
        //                    }
        //                    else
        //                    {
        //                        string datee = "01/01/0001";
        //                        gn.Dependent_DOB1 = Convert.ToDateTime(datee);
        //                    }

        //                    gn.MaritalStatusDesc = item.MaritalStatusDesc;
        //                    if (item.DOM1 != null)
        //                    {


        //                        gn.DOM1 = Convert.ToDateTime(item.DOM);

        //                    }
        //                    else
        //                    {
        //                        string datee = "01/01/0001";
        //                        gn.DOM1 = Convert.ToDateTime(datee);
        //                    }




        //                    if (imgg.FirstOrDefault().imagedept != null)
        //                    {
        //                        byte[] array = (imgg.FirstOrDefault().imagedept).ToArray();

        //                        gn.ImgByte = Convert.ToBase64String(array);



        //                    }
        //                    else
        //                    {


        //                        gn.ImgByte = null;
        //                    }



        //                    cd.Add(gn);


        //                }
        //                return Json(cd, JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {

        //                return Json(JsonRequestBehavior.AllowGet);
        //            }


        //        }

        //        else
        //        {
        //            var cd1 = (from f in cd2

        //                       select new Generalform
        //                       {

        //                           Dependent_Id = f.Dependent_Id,
        //                           Army_No = f.Army_No,
        //                           RelationDesc = f.RelationDesc,
        //                           Dependent_Name = f.Dependent_Name,
        //                           UID = f.UID,
        //                           Dependent_DOB1 = Convert.ToDateTime(f.Dependent_DOB),
        //                           DOM1 = Convert.ToDateTime(f.DOM),


        //                       }).ToList();


        //            if (cd1.Count() > 0)
        //            {

        //                string ImgByte;
        //                foreach (var item in cd1)
        //                {
        //                    Generalform gn = new Generalform();
        //                    var imgg = from c in dta.tblfamilydetails where ((c.Army_No == item.Army_No) && (c.UID == item.UID)) select c;
        //                    gn.Dependent_Id = item.Dependent_Id;
        //                    gn.Army_No = item.Army_No;
        //                    gn.Dependent_Name = item.Dependent_Name;
        //                    gn.RelationDesc = item.RelationDesc;
        //                    gn.UID = item.UID;
        //                    if (item.Dependent_DOB != null)
        //                    {
        //                        gn.Dependent_DOB1 = Convert.ToDateTime(item.Dependent_DOB);
        //                    }
        //                    else
        //                    {
        //                        string datee = "01/01/0001";
        //                        gn.Dependent_DOB1 = Convert.ToDateTime(datee);
        //                    }

        //                    gn.MaritalStatusDesc = item.MaritalStatusDesc;
        //                    if (item.DOM1 != null)
        //                    {
        //                        gn.DOM1 = Convert.ToDateTime(item.DOM1);
        //                    }
        //                    else
        //                    {
        //                        string datee = "01/01/0001";
        //                        gn.DOM1 = Convert.ToDateTime(datee);
        //                    }




        //                    if (imgg.FirstOrDefault().imagedept != null)
        //                    {
        //                        byte[] array = (imgg.FirstOrDefault().imagedept).ToArray();

        //                        gn.ImgByte = Convert.ToBase64String(array);



        //                    }
        //                    else
        //                    {


        //                        gn.ImgByte = null;
        //                    }



        //                    cd.Add(gn);


        //                }
        //                return Json(cd, JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {

        //                return Json(JsonRequestBehavior.AllowGet);
        //            }

        //        }
        //    }


        //    return Json(JsonRequestBehavior.AllowGet);




        //}
        //public JsonResult Editdepen1_citizen(int id)
        //{
        //    try
        //    {
        //        List<Generalform> cd = new List<Generalform>();
        //        Generalform ms1 = new Generalform();

        //        var list = (from fm in dta.tblfamilydetails_citizens
        //                    where fm.Dependent_Id == id
        //                    select fm);

        //        if (list.Count() > 0)
        //        {

        //            foreach (var item in list)
        //            {
        //                Generalform gn = new Generalform();
        //                gn.Army_No = item.Army_No;
        //                gn.Dependent_Name = item.Dependent_Name;
        //                gn.RelationCode = item.RelationCode;
        //                if (item.UID == null)
        //                {
        //                    gn.UID = null;
        //                }
        //                else
        //                {
        //                    gn.UID = item.UID;
        //                }
        //                if (item.DOB != null)
        //                {

        //                    gn.Dependent_DOB1 = Convert.ToDateTime(item.DOB);


        //                }
        //                else
        //                {
        //                    string datee = "01/01/0001";
        //                    gn.Dependent_DOB1 = Convert.ToDateTime(datee);
        //                }

        //                gn.MaritalStatusCode = (item.MaritalStatusCode).ToString();
        //                if (item.DOM != null)
        //                {


        //                    gn.DOM1 = Convert.ToDateTime(item.DOM);


        //                }
        //                else
        //                {
        //                    string datee = "01/01/0001";
        //                    gn.DOM1 = Convert.ToDateTime(datee);
        //                }


        //                if (item.imagedept != null)
        //                {

        //                    gn.ImgByte = Convert.ToBase64String((item.imagedept).ToArray());



        //                }
        //                //else
        //                //{


        //                //    gn.ImgByte = null;
        //                //}
        //                //changes 07-03-2018//
        //                gn.Wife_recorded_in_PPO_no = item.Wife_recorded_in_PPO_no;
        //                gn.Wife_othername = item.Wife_OtherName;
        //                gn.Number_Of_children = Convert.ToString(item.Number_Of_children);
        //                gn.Recorded_in_DO_Part2 = item.Recorded_in_DO_Part2;
        //                gn.Recorded_in_DO_Part2text = item.Recorded_in_DO_Part2text;
        //                //end//
        //                cd.Add(gn);


        //            }
        //            return Json(cd, JsonRequestBehavior.AllowGet);
        //        }
        //        return Json(JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        // throw ex;
        //        //changes on 13-03-2018
        //        mydata.Error = ex.Message;
        //        mydata.PageError = "/View/RSBGeneralInfrm";
        //        mydata.DateError = DateTime.Now;
        //        mydata.DetailError = ex.ToString();
        //        tblTraceError error = new tblTraceError();
        //        error.Error = mydata.Error;
        //        error.PageError = mydata.PageError;
        //        error.DateError = mydata.DateError;
        //        error.DetailError = mydata.DetailError;

        //        ed.tblTraceErrors.InsertOnSubmit(error);
        //        ed.SubmitChanges();
        //        //end
        //        return Json(JsonRequestBehavior.AllowGet);


        //    }
        //}
        //public JsonResult Editdepen_citizen(Generalform mydata, int id1)
        //{
        //    DateTime datee = Convert.ToDateTime("01/01/2000");
        //    string msg;
        //    bool dependent, cid;

        //    dependent = IsName(mydata.Dependent_Name);
        //    cid = IsNumber(mydata.UID);

        //    if (cid == false || dependent == false)
        //    {

        //        if (cid == false)
        //        {
        //            msg = "c";
        //            return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        else if (dependent == false)
        //        {
        //            msg = "d";
        //            return Json(new { msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        try
        //        {
        //            DateTime Dependent_DOB, DOM;
        //            var nn1 = mydata.Dependent_DOB;
        //            DateTime ValidDate1;
        //            DateTime ValidDate2;
        //            //DateTime? ValidDate2;
        //            var nn3 = mydata.DOM;
        //            tblfamilydetails_citizen sc = dta.tblfamilydetails_citizens.Where(x => x.Dependent_Id == id1).FirstOrDefault();
        //            if (sc != null)
        //            {
        //                sc.Dependent_Name = mydata.Dependent_Name;
        //                if (mydata.RelationCode == "0")
        //                {
        //                    string rel = "Please select relation";
        //                    return Json(new { rel }, JsonRequestBehavior.AllowGet);
        //                }
        //                else
        //                {
        //                    sc.RelationCode = mydata.RelationCode;
        //                }


        //                sc.UID = mydata.UID;

        //                if (mydata.Dependent_DOB != null)
        //                {

        //                    if (DateTime.TryParseExact(nn1.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate1))
        //                    {

        //                        Dependent_DOB = ValidDate1;
        //                        if (Dependent_DOB != DateTime.Now.Date)
        //                        {
        //                            var nn = mydata.Dependent_DOB;

        //                            //new 03-03-2018//
        //                            if (nn != null)
        //                            {
        //                                DateTime ValidDate;
        //                                //DateTime? ValidDate2;

        //                                if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
        //                                {
        //                                    sc.DOB = ValidDate;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                mydata.Error = "date of dependent error";
        //                                mydata.PageError = "/View/RSBGeneralInfrm";
        //                                mydata.DateError = DateTime.Now;
        //                                mydata.DetailError = nn.ToString();
        //                                tblTraceError error1 = new tblTraceError();
        //                                error1.Error = mydata.Error;
        //                                error1.PageError = mydata.PageError;
        //                                error1.DateError = mydata.DateError;
        //                                error1.DetailError = mydata.DetailError;

        //                                ed.tblTraceErrors.InsertOnSubmit(error1);
        //                                ed.SubmitChanges();


        //                            }



        //                        }
        //                        else
        //                        {
        //                            string dob = "DOB is not valid !!";
        //                            return Json(new { dob }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        string dob = "DOB is not valid !!";
        //                        return Json(new { dob }, JsonRequestBehavior.AllowGet);
        //                    }

        //                }
        //                else
        //                {
        //                    sc.DOB = null;
        //                }


        //                sc.MaritalStatusCode = Convert.ToChar(mydata.MaritalStatusDesc);
        //                if (Convert.ToString(sc.MaritalStatusCode) == "2")
        //                {
        //                    sc.DOM = null;

        //                }
        //                else
        //                {
        //                    if (mydata.DOM == null)
        //                    {
        //                        string domm = "Date of marriage not null !!";
        //                        return Json(new { domm }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    else
        //                    {

        //                        if (DateTime.TryParseExact(nn3.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate2))
        //                        {
        //                            DOM = ValidDate2;

        //                            if (DOM <= DateTime.Now.Date)
        //                            {
        //                                if (mydata.Dependent_DOB == mydata.DOM)
        //                                {

        //                                    string des = "Date of marriage and Date of Birth Not Equal !!";
        //                                    return Json(new { des }, JsonRequestBehavior.AllowGet);
        //                                }
        //                                else
        //                                {
        //                                    var nn = mydata.DOM;

        //                                    //new 03-03-2018//
        //                                    if (nn != null)
        //                                    {
        //                                        DateTime ValidDate;
        //                                        //DateTime? ValidDate2;

        //                                        if (DateTime.TryParseExact(nn.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ValidDate))
        //                                        {
        //                                            sc.DOM = ValidDate;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        mydata.Error = "date of dependent edit error";
        //                                        mydata.PageError = "/View/RSBGeneralInfrm";
        //                                        mydata.DateError = DateTime.Now;
        //                                        mydata.DetailError = nn.ToString();
        //                                        tblTraceError error1 = new tblTraceError();
        //                                        error1.Error = mydata.Error;
        //                                        error1.PageError = mydata.PageError;
        //                                        error1.DateError = mydata.DateError;
        //                                        error1.DetailError = mydata.DetailError;

        //                                        ed.tblTraceErrors.InsertOnSubmit(error1);
        //                                        ed.SubmitChanges();


        //                                    }

        //                                }
        //                            }
        //                            else
        //                            {
        //                                string dom = "Date of marriage is not valid !!";
        //                                return Json(new { dom }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            string dom = "Date of marriage is not valid !!";
        //                            return Json(new { dom }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }
        //                }

        //                if (Session["imagee"] != null)
        //                {
        //                    sc.imagedept = (byte[])Session["imagee"];
        //                    Session["imagee"] = null;
        //                }
        //                else
        //                {
        //                    //if (mydata.imagedept == null)
        //                    //{
        //                    //    sc.imagedept = (byte[])Session["imagee"];
        //                    //    Session["imagee"] = null;
        //                    //}
        //                    //else
        //                    //{

        //                    sc.imagedept = mydata.imagedept;
        //                    // }

        //                }
        //                //chnges 07-03-2018//
        //                sc.Wife_recorded_in_PPO_no = mydata.Wife_recorded_in_PPO_no;
        //                sc.Wife_OtherName = mydata.Wife_othername;
        //                sc.Number_Of_children = Convert.ToInt16(mydata.Number_Of_children);
        //                sc.Recorded_in_DO_Part2 = mydata.Recorded_in_DO_Part2;
        //                sc.Recorded_in_DO_Part2text = mydata.Recorded_in_DO_Part2text;
        //                sc.Wife_recorded_in_PPO_no = mydata.Wife_recorded_in_PPO_no;
        //                //end

        //                dta.SubmitChanges();
        //                // }

        //            }
        //            else
        //            {
        //                string msg1 = "not";
        //                return Json(msg1, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;


        //        }
        //        return Json(JsonRequestBehavior.AllowGet);

        //    }
        //    return Json(JsonRequestBehavior.AllowGet);

        //}
        //[HttpPost]
        //public JsonResult Delete_citizen(Generalform model, int id)
        //{
        //    try
        //    {
        //        var m = from fm in dta.tblfamilydetails_citizens where fm.Dependent_Id == id select fm;
        //        foreach (var detail in m)
        //        {
        //            dta.tblfamilydetails_citizens.DeleteOnSubmit(detail);
        //        }



        //        dta.SubmitChanges();
        //        var data = from fm in dta.tblfamilydetails_citizens where fm.Army_No == Session["armynoalrd"] select fm;

        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public JsonResult RSBdependentalready_citizen(string amno)
        {
            List<Generalform> cd = new List<Generalform>();


            var cd2 = (from c in dta.tblfamilydetails_citizens
                       join rel in dta.tblsanikRelations on c.RelationCode equals rel.RelationCode
                       where (c.Army_No == amno)
                       select new Generalform
                       {
                           MaritalStatusCode = Convert.ToString(c.MaritalStatusCode),
                           RelationDesc = rel.RelationDesc,
                           Dependent_Id = c.Dependent_Id,
                           Army_No = c.Army_No,
                           Dependent_Name = c.Dependent_Name,
                           UID = c.UID,

                           Dependent_DOB1 = c.DOB,
                           DOM1 = c.DOM,
                           Wife_othername = c.Wife_OtherName


                       }).ToList();
            var cd3 = from mat in dta.MaritalStatus
                      select new Generalform
                      {
                          MaritalStatusCode = Convert.ToString(mat.Marital_Code),
                          MaritalStatusDesc = mat.Marital_Status
                      };
            var cd1 = (from f in cd2
                       join m in cd3 on f.MaritalStatusCode equals m.MaritalStatusCode
                       //where f.Army_No==amno
                       select new Generalform
                       {
                           MaritalStatusDesc = m.MaritalStatusDesc,
                           Dependent_Id = f.Dependent_Id,
                           Army_No = f.Army_No,
                           RelationDesc = f.RelationDesc,
                           Dependent_Name = f.Dependent_Name,
                           UID = f.UID,
                           Dependent_DOB1 = f.Dependent_DOB1,
                           DOM1 = f.DOM1,
                           Wife_othername = f.Wife_othername


                       }).ToList();






            if (cd1.Count() > 0)
            {


                foreach (var item in cd1)
                {
                    Generalform gn = new Generalform();


                    gn.Dependent_Id = item.Dependent_Id;
                    var imgg = from c in dta.tblfamilydetails_citizens where ((c.Army_No == item.Army_No) && (c.Dependent_Id == gn.Dependent_Id)) select c;
                    gn.Army_No = item.Army_No;
                    gn.Dependent_Name = item.Dependent_Name;
                    gn.RelationDesc = item.RelationDesc;
                    var str = item.UID;
                    if (str == null)
                    {
                        gn.UID = item.UID;
                    }
                    else
                    {
                        gn.UID = string.Concat("".PadLeft(9, '*'), str.Substring(str.Length - 4));
                    }
                    // gn.UID = item.UID;
                    if (item.Dependent_DOB1 != null)
                    {
                        gn.Dependent_DOB1 = Convert.ToDateTime(item.Dependent_DOB1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Dependent_DOB1 = Convert.ToDateTime(datee);
                    }
                    gn.MaritalStatusDesc = item.MaritalStatusDesc;
                    if (item.DOM1 != null)
                    {
                        gn.DOM1 = Convert.ToDateTime(item.DOM1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.DOM1 = Convert.ToDateTime(datee);
                    }



                    if (imgg.FirstOrDefault().imagedept != null)
                    {
                        byte[] array = (imgg.FirstOrDefault().imagedept).ToArray();

                        gn.ImgByte = Convert.ToBase64String(array);



                    }
                    else
                    {


                        gn.ImgByte = null;
                    }

                    gn.Wife_othername = item.Wife_othername;

                    cd.Add(gn);


                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string output = "empty";
                return Json(new { output }, JsonRequestBehavior.AllowGet);
            }





        }
        ////*****************end*************************//

        //**********************end********************************//

        //////////////***********************approval page********************////
        //public JsonResult approvalpage()
        //{
        //    //var result1;
        //    if (Convert.ToString(Session["dcode"]) == "notdcode")
        //    {
        //       var result1 = (dta.GetSanikalldetail_citizen()).ToList();
        //       ViewBag.approvepage = result1;
        //    }
        //    else
        //    {
        //      var dcode = (string)Session["dcode"];
        //     var result1 = (dta.GetSanikdetail_citizen(dcode)).ToList();
        //     ViewBag.approvepage = result1;
        //    }        

        //    int res = 1;
        //    return Json(res, JsonRequestBehavior.AllowGet);

       // }
        public JsonResult RSBdependent_citizen()
        {
            List<Generalform> cd = new List<Generalform>();
            var cd2 = (from c in dta.tblfamilydetails_citizens
                       //join mrt in ed.tblMaritalStatusMasters on c.MaritalStatusCode equals mrt.MaritalStatusCode
                       join rel in dta.tblsanikRelations on c.RelationCode equals rel.RelationCode
                       where (c.Army_No == Convert.ToString(Session["Armyno"]))
                       select new Generalform
                       {
                           // MaritalStatusDesc = mrt.MaritalStatusDesc,
                           MaritalStatusCode = Convert.ToString(c.MaritalStatusCode),
                           RelationDesc = rel.RelationDesc,
                           Dependent_Id = c.Dependent_Id,
                           Army_No = c.Army_No,
                           Dependent_Name = c.Dependent_Name,
                           UID = c.UID,
                           Dependent_DOB1 = Convert.ToDateTime(c.DOB),
                           DOM1 = Convert.ToDateTime(c.DOM)


                       }).ToList();
            var cd3 = from mat in dta.MaritalStatus
                      select new Generalform
                      {
                          MaritalStatusCode = Convert.ToString(mat.Marital_Code),
                          MaritalStatusDesc = mat.Marital_Status
                      };
            var cd1 = (from f in cd2
                       join m in cd3 on f.MaritalStatusCode equals m.MaritalStatusCode
                       //where f.Army_No==amno
                       select new Generalform
                       {
                           MaritalStatusDesc = m.MaritalStatusDesc,
                           Dependent_Id = f.Dependent_Id,
                           Army_No = f.Army_No,
                           RelationDesc = f.RelationDesc,
                           Dependent_Name = f.Dependent_Name,
                           UID = f.UID,
                           Dependent_DOB1 = Convert.ToDateTime(f.Dependent_DOB1),
                           DOM1 = Convert.ToDateTime(f.DOM),


                       }).ToList();



            if (cd1.Count() > 0)
            {


                foreach (var item in cd1)
                {
                    Generalform gn = new Generalform();
                    var imgg = from c in dta.tblfamilydetails_citizens where ((c.Army_No == item.Army_No) && (c.Dependent_Id == item.Dependent_Id)) select c;
                    gn.Dependent_Id = item.Dependent_Id;
                    gn.Army_No = item.Army_No;
                    gn.Dependent_Name = item.Dependent_Name;
                    gn.RelationDesc = item.RelationDesc;
                    var str = item.UID; ;
                    if (str == null)
                    {
                        gn.UID = item.UID;
                    }
                    else
                    {
                        gn.UID = string.Concat("".PadLeft(9, '*'), str.Substring(str.Length - 4));
                    }


                    // gn.UID = item.UID;
                    if (item.Dependent_DOB1 != null)
                    {
                        gn.Dependent_DOB1 = Convert.ToDateTime(item.Dependent_DOB1);
                    }
                    else
                    {
                        string datee = "01/01/0001";
                        gn.Dependent_DOB1 = Convert.ToDateTime(datee);
                    }

                    gn.MaritalStatusDesc = item.MaritalStatusDesc;
                    gn.DOM1 = Convert.ToDateTime(item.DOM);

                    if (imgg.Count() > 0)
                    {
                        if (imgg.FirstOrDefault().imagedept != null)
                        {
                            byte[] array = (imgg.FirstOrDefault().imagedept).ToArray();

                            gn.ImgByte = Convert.ToBase64String(array);



                        }
                        else
                        {


                            gn.ImgByte = null;
                        }

                    }
                    else
                    {
                        gn.ImgByte = null;

                    }

                    cd.Add(gn);


                }
                return Json(cd, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(JsonRequestBehavior.AllowGet);
            }





        }
       //-----------Table approval--------------//
        public JsonResult returnapprovalpage()
        {
           
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
                if (Convert.ToString(Session["dcode"]) == "notdcode")
                {
                    //var status = (from aw in dta.rsbgeneral_citizen1s select aw.Status).FirstOrDefault();

                     


                        var result1 = (dta.GetSanikalldetail_citizen()).ToList();
                        if (result1.Count() >0)
                        {
                            var approvepage = result1;
                            return Json(approvepage, JsonRequestBehavior.AllowGet);
                        }
                    //}
                        else
                        {
                            string str = "not";
                            return Json(str, JsonRequestBehavior.AllowGet);
                        }
                }
                else
                {
                    var dcode = (string)Session["dcode"];
                    //var status = (from aw in dta.rsbgeneral_citizen1s select aw.Status).FirstOrDefault();
                    //if (status == null )
                    //{
                        var result1 = (dta.GetSanikdetail_citizen(dcode)).ToList();
                        if (result1.Count()>0)
                        {
                            var approvepage = result1;
                            return Json(approvepage, JsonRequestBehavior.AllowGet);
                        }
                        //}
                        else
                        {
                            string str = "not";
                            return Json(str, JsonRequestBehavior.AllowGet);
                        }
                }
                return Json(JsonRequestBehavior.AllowGet);
            }
            return Json(JsonRequestBehavior.AllowGet);
        }
         [HttpGet]
        public ActionResult approvalpage()
        {
             string strPreviousPage = "";
             if (Request.UrlReferrer != null)
             {
                 strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
             }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("approvalpage");

        }
        public JsonResult insertintomaintable(string id)
        {
            int id1 = Convert.ToInt16(id);
           
            Session["idctizen"] = id1;
           
                
                var result1 = (dta.GetSanikdetailfulldetail_citizen(id1)).ToList();
             
                ViewBag.approvepage = result1;
            
        
            int res = 1;
            return Json(res, JsonRequestBehavior.AllowGet);
           
        }
        public ActionResult returnapproved()
        {
            string strPreviousPage = "";
            if (Request.UrlReferrer != null)
            {
                strPreviousPage = Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1];
               
                int id1 =(int)Session["idctizen"];
                var result1 = (dta.GetSanikdetailfulldetail_citizen(id1)).ToList();
                foreach (var item in result1)
                {
                    Session["armynumbercitizen"] = item.Army_No;
                    var army = (string)Session["armynumbercitizen"];
                    var result2 = (dta.GetSanikaward_citizen(army)).ToList();
                    var result3 = (dta.GetSanikcomp_citizen(army)).ToList();
                    var result4 = (dta.GetSanikdep_citizen(army)).ToList();
                    var result5 = (dta.GetSanikloan_citizen(army)).ToList();
                    var result6 = (dta.GetSanikcourtcase_citizen(army)).ToList();
                    ViewBag.report1 = result1;
                    ViewBag.report2 = result2;
                    ViewBag.report3 = result3;
                    ViewBag.report4 = result4;
                    ViewBag.report5 = result5;
                    ViewBag.report6 = result6;
                }
           
               

                return View("approvalpagecitizen");
            }
            if (strPreviousPage == "")
            {

                Session["userid"] = null;
                return RedirectToAction("Login", "Home");
            }

            return View("approvalpagecitizen");
        }

        public ActionResult approvalpagecitizen()
        {
            return View();
        }
        //-----------Approval Page------------//
         public JsonResult inserttemptomaintable()
        {
            int id1 = (int)Session["idctizen"];
            string cidr1 = "";
            var dcode1 = (from tempsanik in dta.rsbgeneral_citizen1s where tempsanik.id == id1 select tempsanik.dcode).FirstOrDefault();
            var uid_cid = from sankitemp in dta.rsbgeneral_citizen1s where sankitemp.id == id1 select sankitemp;
            var cidruid_cid = from cidrtbl in ed.tblCIDRs where cidrtbl.UID == uid_cid.FirstOrDefault().UID select cidrtbl;
            if (uid_cid.FirstOrDefault().Citizen_ID == null && uid_cid.FirstOrDefault().UID != null && cidruid_cid.FirstOrDefault().Citizen_ID != null)
            {
                   cidr1 = cidruid_cid.FirstOrDefault().Citizen_ID;
                   string userid = Session["userid"].ToString();
                   var result1 = (dta.insertintotemptomain(id1,cidr1,userid)).FirstOrDefault();
                   if (result1.Issucess == 1)
                   {
                       string str = "sucess";
                       return Json(str, JsonRequestBehavior.AllowGet);
                   }
                   else
                   {
                       string str = "failed";
                       return Json(str, JsonRequestBehavior.AllowGet);
                   }
               }
 
            else
            {
                cidr1=getcidr("06", dcode1);
                string userid = Session["userid"].ToString();
                var result1 = (dta.insertintotemptomain(id1, cidr1, userid)).FirstOrDefault();
                if (result1.Issucess == 1)
                {
                    string str = "sucess";
                    return Json(str, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string str = "failed";
                    return Json(str, JsonRequestBehavior.AllowGet);
                }
             }

           
            
            return Json(JsonRequestBehavior.AllowGet);
        }
         public JsonResult RejectedCase(string remarks)
         {
             int id1 = (int)Session["idctizen"];
             if (remarks == null)
             {
                 string str = "Remarks";
                 return Json(str, JsonRequestBehavior.AllowGet);
             }
             else
             {
                 var result1 = (dta.Rejectrecord(id1, remarks)).FirstOrDefault();
                 if (result1.Issucess == 1)
                 {
                     string str = "sucess";
                     return Json(str, JsonRequestBehavior.AllowGet);
                 }
                 else
                 {
                     string str = "failed";
                     return Json(str, JsonRequestBehavior.AllowGet);
                 }
             }
             return Json(JsonRequestBehavior.AllowGet);
         }
        //****************end**************************//


    }
}
