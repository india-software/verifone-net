using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VerifoneIntegration.Verifone;

namespace VerifoneIntegration
{
    class Program
    {

        public static void Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // In order to enable SOAPscope to work through SSL. Refer to FAQ for more details
            ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
            string MessageData = "<payerauthenrollmentcheckrequest><mkaccountid></mkaccountid><cardnumber>4929123123123127</cardnumber><cardexpyear>14</cardexpyear><cardexpmonth>12</cardexpmonth><currencycode>GB</currencycode><currencyexponent>2</currencyexponent><transactionamount>1000</transactionamount></payerauthenrollmentcheckrequest>";
            Message responseMsq = MakeRequest(MessageData);
            if (responseMsq.MsgData.Contains("<ERROR>"))
            {
                string paymentRequest = "<transactionrequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"TXN\"><merchantreference></merchantreference><accountid>4292</accountid><txntype>01</txntype><transactioncurrencycode>826</transactioncurrencycode><apacsterminalcapabilities>776</apacsterminalcapabilities><capturemethod>11</capturemethod><processingidentifier>2</processingidentifier><tokenid>0</tokenid><pan>4929123123123127</pan><track2></track2><csc></csc><avshouse></avshouse><avspostcode></avspostcode><expirydate>1412</expirydate><issuenumber></issuenumber><startdate></startdate><txnvalue>15.00</txnvalue><cashback>0.00</cashback><gratuity>0.00</gratuity><authcode></authcode><transactiondatetime></transactiondatetime><aadarec><data>MDAxMTk4ODEyMjEdMDAyV0lMQ09P</data><key></key></aadarec></transactionrequest>";
                responseMsq = MakeRequest(paymentRequest);
                if (!string.IsNullOrEmpty(responseMsq.MsgData))
                {
                    XDocument dataDoc = XDocument.Parse(responseMsq.MsgData);
                    if (responseMsq.MsgData.Contains("<ERROR>"))
                    {
                        var errorcode = dataDoc.Descendants().FirstOrDefault(y => (y as XElement) != null && (y as XElement).Name.LocalName.ToLower() == "code");
                        var errortxt = dataDoc.Descendants().FirstOrDefault(y => (y as XElement) != null && (y as XElement).Name.LocalName.ToLower() == "msgtxt");
                        string code = errorcode != null ? errorcode.Value : "";
                        string txt = errortxt != null ? errortxt.Value : "";
                    }
                    else
                    {

                        var xresultElement = dataDoc.Descendants().FirstOrDefault(y => (y as XElement) != null && (y as XElement).Name.LocalName.ToLower() == "transactionid");
                        var xerrorElement = dataDoc.Descendants().FirstOrDefault(y => (y as XElement) != null && (y as XElement).Name.LocalName.ToLower() == "errormsg");
                        if (xresultElement != null)
                        {
                            string errorMsg = xerrorElement != null ? xerrorElement.Value : "";
                            string transactionId = xresultElement.Value;
                        }
                    }
                }
            }
        }
        public static Message MakeRequest(string MsgData)
        {
            Message message = new Message();
            message.MsgType = "TXN";
            message.MsgData = MsgData;
            message.ClientHeader = new ClientHeader();
            message.ClientHeader.SystemID = 776;
            message.ClientHeader.SystemGUID = "569c0b6e-2ac2-4028-a18d-c9361d043862";
            message.ClientHeader.Passcode = "12019069";
            message.ClientHeader.SendAttempt = 0;
            // message.ClientHeader.CDATAWrapping = true;
            CommideaGatewaySoapClient cc = new CommideaGatewaySoapClient();

            Message responseMsq = cc.ProcessMsg(message);

            return responseMsq;


        }


    }
    
}
