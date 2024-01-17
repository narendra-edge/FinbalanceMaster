using FbEmailService.Api.Models;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FbEmailService.Api.Controllers
{
    public class EmailController : ControllerBase
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly IConfiguration _configuration;

        public EmailController(ISendGridClient sendGridClient, IConfiguration configuration)
        {
            _sendGridClient = sendGridClient;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("send-text-mail")]
        public async Task<IActionResult> SendPlainTextEmail(string toEmail)
        {
            string fromEmail = _configuration.GetSection("SendGridEmailSettings")
                              .GetValue<string>("FromEmail");
            string fromName = _configuration.GetSection("SendGridEmailSettings")
                              .GetValue<string>("FromName");

            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = "Plain Text Email",
                PlainTextContent = "Fibalance Welcomes You !!!!"
            };
            msg.AddTo(toEmail);

            var response = await _sendGridClient.SendEmailAsync(msg);
            string message = response.IsSuccessStatusCode ? "Email send" : "Email Sending Failed";
            return Ok(message);
        }
        private string EmailHTML(HeaderEmail headerEmail)
        {
            return @"<html>
          <head>
            <meta charset=""utf-8"">
            <title>Welcome Mail</title>
            <link href=""https://fonts.googleapis.com/css?family=Lato:400,400i,700,700i"" rel=""stylesheet"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
           <style>
            body {
            font-family: 'Lato', sans-serif;
            margin: 0px;
            padding: 0px;
            font-size: 14px;
            line-height: 18px;
            }
            #main {
            width: 620px;
            margin: 20px auto;
            background: #dedede;
            }
            #container {
            width: 578px;
            padding: 20px;
            border: 1px solid #ddd;
            margin-bottom: 0px;
            background: #fff;
            }
           .header {
            border-bottom: 4px solid #fbb030;
            padding-bottom: 5px;
            margin-bottom: 20px;
            }
           .logo {
            width: 200px;
            float: left;
            padding: 5px 0px;
            }
            .logo img {
                width: 100%;
                padding: 0px;
            }
           .cc {
            width: 200px;
            float: right;
            }
           h1 {
            font-size: 18px;
            font-weight: 400;
            margin-bottom: 30px;
            }
            h2 {
            font-size: 16px;
            font-weight: 400;
            margin-bottom: 20px;
            }
            h1 span {
            font-size: 17px;
            font-weight: 400;
            }
            p {
            font-size: 14px;
            margin-bottom: 20px;
            line-height: 18px;
            }
           ol
            {
            margin-top: 5px;
            margin-bottom: 30px;
            }

            ol li
            {
                padding-left: 5px;
                line-height: 21px;
            }

            ol li span 
            {
                    padding-left: 5px;
            }

           .small {
            font-size: 12px !important;
            line-height: 16px;
            color: #444;
            text-align: justify;
            padding: 10px 20px;
            padding-bottom: 20px;
            }

            a 
            {
            color: #444;
            text-decoration: none;
            }
            #main table
            {
            width: 578px;
            }
           .social ul {
            margin: 0px;
            padding: 0px;
            padding-top: 5px;
            float: right;
            width: 100%;
            clear: both;
            }
            .social ul li {
                list-style-type: none;
                margin: 1px;
                float: right;
            }
           .social ul li a {
           list-style-type: none;
                }
           </style>
         </head>
        <body>
         <section id=""main"">
           <div id=""container"">
            <table border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                    <td class=""header"">
                        <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                            <tr>
                                <td class=""logo""><img src=""http://fintrackindia.com/img/logo-black.png""></td>
                                <td class=""cc"">
                                    <div class=""social"">
                                        <ul>
                                            <li><a href=""https://www.facebook.com/FintrackIndia/"" target=""_blank""><img src=""img/facebook.png""></a></li>
                                            <li><a href=""https://twitter.com/FintrackIndia"" target=""_blank""><img src=""img/twitter.png""></a></li>
                                            <li><a href=""https://www.linkedin.com/company/edge-fintrack-capital-pvt-ltd/"" target=""_blank""><img src=""img/linkedin.png""></a></li>
                                            <li><a href=""https://www.youtube.com/channel/UCfWyIGiNJLY9To9wKDDXHqQ"" target=""_blank""><img src=""img/youtube.png""></a></li>
                                        </ul>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>
                        <h1>Dear <span>{Name}</span></h1>
                        <h2>Congratulation ! </h2>
                        <p>Your KYC is completed  & you are ready for investment</p>

                        <p>Your Account Credentials are</p>
                        <ol>
                            <li>CUSTOMER ID :  <span>{ED51424}</span></li>
                            <li>CKYC NO. : <span>{4514514521}</span></li>
                            <li>CAN NO. :  <span>{ED51424}</span></li>
                            <li>DEMAT A/C NO. : <span>{4514514521}</span></li>
                            <li>PRAN NO :  <span>{ED51424}</span></li>
                        </ol>
                        <p>For all future refrence customer ID will be required.</p>
                        <h2>After Login : </h2>
                        <ol>
                            <li>Track all your investment & insurance through our user friendly <strong>DASHBOARD.</strong></li>
                            <li>Select a product to buy or redeem from <strong> TRANSACTION </strong>.</li>
                            <li>Watch & Track all your investment & insurance through <strong>REPORTS.</strong></li>
                            <li>Plan your budget, set your goal & know  Asset allocation through <strong>MONEY MANAGER.</strong></li>
                        </ol>
                        <br />
                        <p>For any queries or assistance call us at : &nbsp;&nbsp;
                        <p>
                            <strong>Warm Regards</strong> <br />
                            Team Support, <br /> Fintrack India   <br />
                            <span>Ph: <a href=""callto:+120-4118709"">+120-4118709</a>, &nbsp;&nbsp; Email: <a href=""mailto:support@fintrackindia.com"">support@fintrackindia.com</a></span>
                        </p>
                    </td>
                </tr>
                <tr>
                <td></td>
                </tr>
            </table>
             </div>
               <p class=""small""><strong>Disclaimer : </strong>Mutual fund investments are subject to market risks. Please read the scheme information and other related documents carefully before investing. Past performance is not indicative of future returns. Please consider your specific investment requirements before choosing a fund, or designing a portfolio that suits your needs. Edge Fintrack Capital Pvt. Ltd. (with ARN code 143223) makes no warranties or representations, express or implied, on products offered through the platform. It accepts no liability for any damages or losses, however caused, in connection with the use of, or on the reliance of its product or related services. Terms and conditions of the website are applicable. </p>
             </section>
             </body>
              </html>".Replace("{Name}", headerEmail.Name);
        }

        [HttpPost]
        [Route("send-html-mail")]
        public async Task<IActionResult> SendHtmltEmail(HeaderEmail headerEmail)
        {
            string fromEmail = _configuration.GetSection("SendGridEmailSettings")
                              .GetValue<string>("FromEmail");
            string fromName = _configuration.GetSection("SendGridEmailSettings")
                              .GetValue<string>("FromName");

            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = "Html Email",
                HtmlContent = EmailHTML(headerEmail)
            };
            msg.AddTo(headerEmail.ToEmail);

            var response = await _sendGridClient.SendEmailAsync(msg);
            string message = response.IsSuccessStatusCode ? "Email send" : "Email Sending Failed";
            return Ok(message);
        }
        [HttpPost]
        [Route("send-attachment-mail")]
        public async Task<IActionResult> SendFileAttachmentEmail([FromForm]EmailAttachments emailAttachments)
        {
            string fromEmail = _configuration.GetSection("SendGridEmailSettings")
                              .GetValue<string>("FromEmail");
            string fromName = _configuration.GetSection("SendGridEmailSettings")
                              .GetValue<string>("FromName");

            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = "File Attached Email",
                PlainTextContent ="Check Attached File"
            };
            await msg.AddAttachmentAsync(
                emailAttachments.ImageFile.FileName,
                emailAttachments.ImageFile.OpenReadStream(),
                emailAttachments.ImageFile.ContentType,
                "attachment"
                );

            msg.AddTo(emailAttachments.ToEmail);

            var response = await _sendGridClient.SendEmailAsync(msg);
            string message = response.IsSuccessStatusCode ? "Email send" : "Email Sending Failed";
            return Ok(message);
        }

    }

}        
