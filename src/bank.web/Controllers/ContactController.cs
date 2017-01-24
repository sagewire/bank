using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using bank.web.models;

namespace bank.web.Controllers
{
    public class ContactController : ApplicationController
    {

        public ActionResult General()
        {
            
            ViewBag.Message = "Your contact page.";

            var model = new PopoverViewModel
            {
                
            };

            return View("modals/contact", model);
        }

        public ActionResult Send(string name, string email, string message)
        {
            
            var smtpClient = new SmtpClient();
            var mailMessage = new MailMessage();
            mailMessage.To.Add(Settings.ContactFormRecipients);
            mailMessage.Subject = "Web Contact from " + name;
            mailMessage.ReplyToList.Add(email);
            mailMessage.Body = "From: " + email + "\n\n\n" + message + "\n\n\n\n\n\n\n\n\n\n\n\n";

            using (smtpClient)
            {
                smtpClient.Send(mailMessage);
            }

            return View("modals/send");
        }

    }
}