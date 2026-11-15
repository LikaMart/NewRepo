using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace TravelerNotes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmailComunicationController : ControllerBase
{

    [HttpPost("SendEmail")]
    public bool SendEmail(string too, string subject, string? message)
    {
        SendMessage(too, subject, message);
        return true;
    }

    private void SendMessage(string too,string subject,string? message)
    {
        const string senderEmail = "itstepse@gmail.com";
        const string senderPassword = "mitl tdzy onsi rqrv";
        const string smtpAddress = "smtp.gmail.com"; 
        var portNumber = 587;

        var smtpClient = new SmtpClient(smtpAddress, portNumber);
        smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
        smtpClient.EnableSsl = true;

        var mailMessage = new MailMessage(senderEmail, too);

        if (string.IsNullOrEmpty(message))
        {
            mailMessage.Body = @$"

<!DOCTYPE html>

<html lang=""en"">

<head>

    <meta charset=""UTF-8"">

    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">

    <title>Congratulations</title>

</head>

<body style=""margin: 0; padding: 0; background-color: #f4f6f8; font-family: Arial, Helvetica, sans-serif;"">


    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""           style=""background-color: #f4f6f8; padding: 30px 10px;"">

        <tr>

            <td align=""center"">


                <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0""                       style=""width: 100%; max-width: 600px; background-color: #ffffff;

                              border-radius: 8px; overflow: hidden;"">


                    <!-- Header -->

                    <tr>

                        <td align=""center""                            style=""background-color: #198754; padding: 30px 20px; color: #ffffff;"">

                            <h1 style=""margin: 0; font-size: 26px; font-weight: bold;"">

                                Congratulations!

                            </h1>

                        </td>

                    </tr>


                    <!-- Content -->

                    <tr>

                        <td style=""padding: 35px 30px; color: #333333; font-size: 15px;

                                   line-height: 1.7;"">


                            <p style=""margin-top: 0;"">

                                Hello <strong>Guga</strong>,

                            </p>


                            <p>

                                Congratulations! You have successfully completed

                                the <strong>TIT Step</strong> process.

                            </p>


                            <p>

                                Your confirmation has been registered successfully.

                                Thank you for using our services.

                            </p>


                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""                                   style=""background-color: #f0f9f4; border-left: 4px solid #198754;

                                          margin: 25px 0;"">

                                <tr>

                                    <td style=""padding: 15px 18px;"">

                                        <strong style=""color: #198754;"">

                                            Process completed successfully

                                        </strong>

                                    </td>

                                </tr>

                            </table>


                            <p>

                                If you have any questions, please contact our support team.

                            </p>


                            <p style=""margin-bottom: 0;"">

                                Best regards,<br>

                                <strong>Support Team</strong>

                            </p>


                        </td>

                    </tr>


                    <!-- Footer -->

                    <tr>

                        <td align=""center""                            style=""background-color: #f8f9fa; padding: 18px;

                                   color: #777777; font-size: 12px; line-height: 1.5;"">

                            This is an automated notification. Please do not reply to this email.

                        </td>

                    </tr>


                </table>


            </td>

        </tr>

    </table>


</body>

</html>";
        }
        else
        {
            mailMessage.Body = message;
        }

        mailMessage.Subject = subject;
        mailMessage.IsBodyHtml = true;

        try
        {
            smtpClient.Send(mailMessage);

        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to send: {ex.Message}"); 
        }
        finally
        {
            smtpClient.Dispose();
            mailMessage.Dispose();
        }
    }
}
