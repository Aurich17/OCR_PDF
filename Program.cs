/*
        
    Sample project for OCRWebService.com (REST API).
    Extract text from scanned images and convert into editable formats.
    Please create new account with ocrwebservice.com via http://www.ocrwebservice.com/account/signup and get license code

*/

namespace OCRWebServiceREST.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.Net.Security;
    using System.IO;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using System.Text.RegularExpressions;
    using TEST_PDF;
    using System.Windows.Forms;
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());

                // Provide your username and license code

                string input = @"Shipper YIWU CITY YUXUN IMPORT AND EXPORT CO., LTD. ADD: ROOM 2908-A, 29 / F, BUILDING B, FUTIAN GINZA, FUTIAN STREET, YIWU, JINHUA, ZHEJIANG, CHINA TAX ID :91330782MA2M78728L Consignee GRUPO PRO FIT S. R. L. ADDRESS: PSJ. JOSE MIGUEL DE LOS RIOS 127 INT 05. LA VICTORIA-LIMA-PERU RUC: 20607082881 LEGAL REPRESENTATIVE: CINTHIA JHOSELY RUPAY VASQUEZ CEL PHONE: 0051-931483860 Notify party GRUPO PRO FIT S. R. L. ADDRESS: PSJ. JOSE MIGUEL DE LOS RIOS 127 INT 05. LA VICTORIA-LIMA-PERU RUC: 20607082881 LEGAL REPRESENTATIVE: CINTHIA JHOSELY RUPAY VASQUEZ CEL PHONE: 0051-931483860 MB/L Number HB/L Number DLSH24020078 OCEAN BILL OF LADING CIMC WETRANS CIMC Wetrans Delfin Logistics (HK) Co., Limited 'he Goods and inStrUelianS are accepted and dealt wiTh suble<t to the Starsdard Condlrions printed overleaf. Taken in Charge ie apparent good order arid condition, unless otherwise noted herein, At the place of receipt fat transport and delivery as mentioned be One of these Combined transport BrIls of Lo ding Must be Surtendered duty endorsed on exchansie for the foods. In Witness where of the original Combined Transport aril of tadimp an of this tenor and data have been signed in the number stated betaw. one of which being ►C40010#8,144 the ot►eris) to be void. Pre-carriage by I Place of receipt Ocean Vessel Voy No. I Port of loading MANZANILLO EXPRESS V. 2406E SHANGHAI, CHINA Port of discharge CALLAO, PERU IPlace of delivery CALLAO, PERU For Delivery of goods please apply to: DELFIN GROUP CO S. A. C. RUC: 20516667550 CALLE ANTEQUERA 777 PISO 12 SAN ISIDRO LIMA PERU TEL:51 6153535 E-MAIL: OP ERAT IONS@DELF INGROUPCO. COM. PE Container No. ;Seal No. Marks and Numbers N/ M *****IMPORTANT ADVISORY***** CARRIAGE TO PERUVIAN PORTS IS SUBJECT TO LOCAL CHARGES THAT SHALL BE INVOICED AND PAYABLE IN PERU TO THE AGENT OF THE SHIPPING LINE ACTING AS EFFECTIVE CARRIER AND / OR THEIR DESIGNATED EMPTY CONTAINER DEPOT AND TO DELFIN GROUP CO. S. A. C. , WHICH RATES ARE PUBLISHED AT WWW. CALLAOONLINE. COM AND HTTP ://WWW. DELFINGROUPCO. COM. PE, RESPECTIVELY, AND WHICH THE MERCHANT HEREBY EXPRESSLY ACKNOWLEDGES AND ACCEPTS. Number of ; Kind of packages; Description of Goods Containers or packages SAID TO CONTAIN 743 CARTONS RUBBER DUMBBELL BATTLE ROPE MEDICINE BALL AEROBICS STEPPER BAR TRAMPOLINE 1*40RH SZLU9852910/HLG8241424 743CARTONS 24635. 000KGS 48. 940CBM SHIPPER' S LOAD, STOW, COUNT AND SEALED. Gross Weight ; Measurement 24635. 000 KGS 48. 940 CBM OS FEB 2024 * Total Number of Containers or other packages(in words.) SAY SEVEN HUNDRED AND FORTY THREE CARTONS ONLY Freight and Charges FREIGHT COLLECT Revenue tons Rate per Prepaid Collect Exchaner rate Prepaid at Payable at Place and date of issue SHANGHAI, CHINA 2024-02-05 Total prepaid Number of original B(s)IL ONE (1) AS AGENT Date 2024-02-05  Signature";
                // Shipper
                Match match = Regex.Match(input, @"Shipper(.*?)Consignee", RegexOptions.Singleline);
                if (match.Success)
                {
                    Console.WriteLine("Shipper: " + match.Groups[1].Value.Trim());
                }

                // Consignee
                match = Regex.Match(input, @"Consignee(.*?)Notify party", RegexOptions.Singleline);
                if (match.Success)
                {
                    Console.WriteLine("Consignee: " + match.Groups[1].Value.Trim());
                }

                // Notify party
                match = Regex.Match(input, @"Notify party(.*?)MB/L Number", RegexOptions.Singleline);
                if (match.Success)
                {
                    Console.WriteLine("Notify party: " + match.Groups[1].Value.Trim());
                }


                //// Process Document 
                //ProcessDocument(user_name, license_code, "C:\\Users\\USER03\\Desktop\\PDF_A_LEER\\CLL-HBL.pdf");

                //// Get Account information
                //PrintAccountInformation(user_name, license_code);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
        }

        /// <summary>
        /// Process document function
        /// </summary>
        /// <param name="user_name">User Name</param>
        /// <param name="license_code">License code</param>
        /// <param name="file_path">Full source document path</param>
        //private static void ProcessDocument(string user_name, string license_code, string file_path)
        //{
        //    // For SSL using
        //    // ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

        //    // Build your OCR:

        //    // Extraction text with English language
        //    string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english&pagerange=1-5&gettext=true&outputformat=doc";

        //    //string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?gettext=true";

        //    // Extraction text with English and German language using zonal OCR
        //    // ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english,german&zone=0:0:600:400,500:1000:150:400";

        //    // Convert first 5 pages of multipage document into doc and txt
        //    //string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english&pagerange=1-5&outputformat=pdf";

        //    byte[] uploadData = GetUploadedFile(file_path);

        //    HttpWebRequest request = CreateHttpRequest(ocrURL, user_name, license_code, "POST");
        //    request.ContentLength = uploadData.Length;

        //    //  Send request
        //    using (Stream post = request.GetRequestStream())
        //    {
        //        post.Write(uploadData, 0, (int)uploadData.Length);
        //    }

        //    try
        //    {
        //        //  Get response
        //        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //        {
        //            // Parse JSON response
        //            string strJSON = new StreamReader(response.GetResponseStream()).ReadToEnd();
        //            OCRResponseData ocrResponse = JsonConvert.DeserializeObject<OCRResponseData>(strJSON);

        //            PrintOCRData(ocrResponse);

        //            // Download output converted file
        //            if (!string.IsNullOrEmpty(ocrResponse.OutputFileUrl))
        //            {
        //                HttpWebRequest request_get = (HttpWebRequest)WebRequest.Create(ocrResponse.OutputFileUrl);
        //                request_get.Method = "GET";

        //                using (HttpWebResponse result = request_get.GetResponse() as HttpWebResponse)
        //                {
        //                    DownloadConvertedFile(result, "C:\\converted_file.doc");
        //                }
        //            }
        //        }
        //    }
        //    catch (WebException wex)
        //    {
        //        Console.WriteLine(string.Format("OCR API Error. HTTPCode:{0}", ((HttpWebResponse)wex.Response).StatusCode));
        //    }
        //}

        ///// <summary>
        ///// Print OCRWebService.com account information
        ///// </summary>
        ///// <param name="user_name"></param>
        ///// <param name="license_code"></param>
        //private static void PrintAccountInformation(string user_name, string license_code)
        //{
        //    try
        //    {
        //        string address_get = @"http://www.ocrwebservice.com/restservices/getAccountInformation";

        //        HttpWebRequest request_get = CreateHttpRequest(address_get, user_name, license_code, "GET");

        //        using (HttpWebResponse response = request_get.GetResponse() as HttpWebResponse)
        //        {
        //            string strJSON = new StreamReader(response.GetResponseStream()).ReadToEnd();
        //            OCRResponseAccountInfo ocrResponse = JsonConvert.DeserializeObject<OCRResponseAccountInfo>(strJSON);

        //            Console.WriteLine(string.Format("Available pages:{0}", ocrResponse.AvailablePages));
        //            Console.WriteLine(string.Format("Max pages:{0}", ocrResponse.MaxPages));
        //            Console.WriteLine(string.Format("Expiration date:{0}", ocrResponse.ExpirationDate));
        //            Console.WriteLine(string.Format("Last processing time:{0}", ocrResponse.LastProcessingTime));
        //        }

        //    }
        //    catch (WebException wex)
        //    {
        //        Console.WriteLine(string.Format("OCR API Error. HTTPCode:{0}", ((HttpWebResponse)wex.Response).StatusCode));
        //    }
        //}

        //private static byte[] GetUploadedFile(string file_name)
        //{
        //    FileStream streamContent = new FileStream(file_name, FileMode.Open, FileAccess.Read);
        //    byte[] inData = new byte[streamContent.Length];
        //    streamContent.Read(inData, 0, (int)streamContent.Length);
        //    return inData;
        //}

        //private static HttpWebRequest CreateHttpRequest(string address_url, string user_name, string license_code, string http_method)
        //{
        //    Uri address = new Uri(address_url);

        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);

        //    byte[] authBytes = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", user_name, license_code).ToCharArray());
        //    request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);
        //    request.Method = http_method;
        //    request.Timeout = 600000;

        //    // Specify Response format to JSON or XML (application/json or application/xml)
        //    request.ContentType = "application/json";

        //    return request;
        //}

        //private static void PrintOCRData(OCRResponseData ocrResponse)
        //{
        //    // Available pages
        //    Console.WriteLine("Available pages: " + ocrResponse.AvailablePages);

        //    // Extracted text. For zonal OCR: OCRText[z][p]    z - zone, p - pages
        //    for (int zone = 0; zone < ocrResponse.OCRText.Count; zone++)
        //    {
        //        for (int page = 0; page < ocrResponse.OCRText[zone].Count; page++)
        //        {
        //            Console.WriteLine(string.Format("Extracted text from page №{0}, zone №{1} :{2}", page, zone, ocrResponse.OCRText[zone][page]));
        //        }
        //    }
        //}

        //private static void DownloadConvertedFile(HttpWebResponse result, string file_name)
        //{
        //    using (Stream response_stream = result.GetResponseStream())
        //    {
        //        using (Stream output_stream = File.OpenWrite(file_name))
        //        {
        //            response_stream.CopyTo(output_stream);
        //        }
        //    }
        //}
    }
}
