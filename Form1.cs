using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using TEST_PDF;
using System.Net;
using OCRWebServiceREST.Client;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;

namespace TEST_PDF
{
    public partial class Form1 : Form
    {
        string textoMBL;
        string textoHBL;
        private string direccionMBL;
        private string direccionHBL;
        string license_code = "AFC02401-9336-4F3F-BC32-D1B8B4179B4F";//"AFC02401-9336-4F3F-BC32-D1B8B4179B4F";//"490C2031-496D-43C3-857E-AC155599E4E6";
        string user_name = "ANTONIO292002";//"ANTONIO292002";//"isma1721";
        //private string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english&pagerange=1-5&gettext=true&outputformat=doc";
        public Form1()
        {
            InitializeComponent();

            lineasCombo.Items.Add("HAPAG");
            lineasCombo.Items.Add("MAERSK");
            lineasCombo.Items.Add("MSC");
            lineasCombo.DropDownStyle = ComboBoxStyle.DropDownList;

            string[][] carrier = new string[][] {
                new string[] {"valores1Izquierda", "valores1Derecha"},
                new string[] {"valores2", "valores2"},
                new string[] {"valores3Izquierda", "valores3Derecha"}
            };
            string[] fila = new string[] { carrier[1][0], carrier[1][1] }; // reemplaza valor1, valor2, valor3 con los valores que deseas agregar
                                                                           //tablaComparativa.Rows.Insert(0, fila);

            //VALORES DE LOS CAMPOS:
            fechaMBL.ReadOnly = true;
            fechaHBL.ReadOnly = true;
            puertoDescargaHBL.ReadOnly = true;
            puertoDescargaMBL.ReadOnly = true;
            pesoMBL.ReadOnly = true;
            pesoHBL.ReadOnly = true;
            puertoCargaMBL.ReadOnly = true;
            puertoCargaHBL.ReadOnly = true;
            volumenHBL.ReadOnly = true;
            volumenMBL.ReadOnly = true;
            sealHBL.ReadOnly = true;
            sealMBL.ReadOnly = true;
            marcasNumerosHBL.ReadOnly = true;
            marcaNumerosMBL.ReadOnly = true;
        }

        private void subeMBL_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "PDF Files|*.pdf";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                containerMBL.Text = filePath;
                direccionMBL = filePath;
                //ProcesaDocumentoMBL(user_name, license_code, filePath);
            }
        }

        private void subeHBL_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "PDF Files|*.pdf";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                containerHBL.Text = filePath;
                direccionHBL = filePath;
            }
        }

        private static void PrintAccountInformation(string user_name, string license_code)
        {
            try
            {
                string address_get = @"http://www.ocrwebservice.com/restservices/getAccountInformation";

                HttpWebRequest request_get = CreateHttpRequest(address_get, user_name, license_code, "GET");

                using (HttpWebResponse response = request_get.GetResponse() as HttpWebResponse)
                {
                    string strJSON = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    OCRResponseAccountInfo ocrResponse = JsonConvert.DeserializeObject<OCRResponseAccountInfo>(strJSON);

                    Console.WriteLine(string.Format("Available pages:{0}", ocrResponse.AvailablePages));
                    Console.WriteLine(string.Format("Max pages:{0}", ocrResponse.MaxPages));
                    Console.WriteLine(string.Format("Expiration date:{0}", ocrResponse.ExpirationDate));
                    Console.WriteLine(string.Format("Last processing time:{0}", ocrResponse.LastProcessingTime));
                }

            }
            catch (WebException wex)
            {
                Console.WriteLine(string.Format("OCR API Error. HTTPCode:{0}", ((HttpWebResponse)wex.Response).StatusCode));
            }
        }

        private static byte[] GetUploadedFile(string file_name)
        {
            FileStream streamContent = new FileStream(file_name, FileMode.Open, FileAccess.Read);
            byte[] inData = new byte[streamContent.Length];
            streamContent.Read(inData, 0, (int)streamContent.Length);
            return inData;
        }

        private static HttpWebRequest CreateHttpRequest(string address_url, string user_name, string license_code, string http_method)
        {
            Uri address = new Uri(address_url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);

            byte[] authBytes = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", user_name, license_code).ToCharArray());
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);
            request.Method = http_method;
            request.Timeout = 600000;

            // Specify Response format to JSON or XML (application/json or application/xml)
            request.ContentType = "application/json";

            return request;
        }

        private static void PrintOCRData(OCRResponseData ocrResponse)
        {
            // Available pages
            Console.WriteLine("Available pages: " + ocrResponse.AvailablePages);

            // Extracted text. For zonal OCR: OCRText[z][p]    z - zone, p - pages
            for (int zone = 0; zone < ocrResponse.OCRText.Count; zone++)
            {
                for (int page = 0; page < ocrResponse.OCRText[zone].Count; page++)
                {
                    Console.WriteLine(string.Format("Extracted text from page №{0}, zone №{1} :{2}", page, zone, ocrResponse.OCRText[zone][page]));
                }
            }
        }

        private static void DownloadConvertedFile(HttpWebResponse result, string file_name)
        {
            using (Stream response_stream = result.GetResponseStream())
            {
                using (Stream output_stream = File.OpenWrite(file_name))
                {
                    response_stream.CopyTo(output_stream);
                }
            }
        }

        private void ProcesaDocumentoMBL(string user_name, string license_code, string file_path)
        {
            string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english&gettext=true&outputformat=xlxs";

            //string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?gettext=true";

            // Extraction text with English and German language using zonal OCR
            // ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english,german&zone=0:0:600:400,500:1000:150:400";

            // Convert first 5 pages of multipage document into doc and txt
            //string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english&pagerange=1-5&outputformat=pdf";

            byte[] uploadData = GetUploadedFile(file_path);

            HttpWebRequest request = CreateHttpRequest(ocrURL, user_name, license_code, "POST");
            request.ContentLength = uploadData.Length;

            //  Send request
            using (Stream post = request.GetRequestStream())
            {
                post.Write(uploadData, 0, (int)uploadData.Length);
            }

            try
            {
                //  Get response
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Parse JSON response
                    string strJSON = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    textoMBL = strJSON;
                    //procesaMBLHapag(strJSON);
                    OCRResponseData ocrResponse = JsonConvert.DeserializeObject<OCRResponseData>(strJSON);
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine(string.Format("OCR API Error. HTTPCode:{0}", ((HttpWebResponse)wex.Response).StatusCode));
            }
        }


        //---------------------------PROCESA DOCUMENTO HBL
        private  void ProcesaDocumentoHBL(string user_name, string license_code, string file_path)
        {
            // For SSL using
            // ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

            // Build your OCR:

            // Extraction text with English language
            string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english&pagerange=1-5&gettext=true&outputformat=xlxs";

            //string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?gettext=true";

            // Extraction text with English and German language using zonal OCR
            // ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english,german&zone=0:0:600:400,500:1000:150:400";

            // Convert first 5 pages of multipage document into doc and txt
            //string ocrURL = @"http://www.ocrwebservice.com/restservices/processDocument?language=english&pagerange=1-5&outputformat=pdf";

            byte[] uploadData = GetUploadedFile(file_path);

            HttpWebRequest request = CreateHttpRequest(ocrURL, user_name, license_code, "POST");
            request.ContentLength = uploadData.Length;

            //  Send request
            using (Stream post = request.GetRequestStream())
            {
                post.Write(uploadData, 0, (int)uploadData.Length);
            }

            try
            {
                //  Get response
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    Form1 formulario = new Form1();
                    // Parse JSON response
                    string strJSON = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    textoHBL = strJSON;
                    //procesaHBL(strJSON);
                    OCRResponseData ocrResponse = JsonConvert.DeserializeObject<OCRResponseData>(strJSON);
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine(string.Format("OCR API Error. HTTPCode:{0}", ((HttpWebResponse)wex.Response).StatusCode));
            }
        }

        private void procesar_Click(object sender, EventArgs e)
        {
            //try {
            //    string valorSeleccionado = lineasCombo.SelectedItem.ToString();
            //    //MessageBox.Show(valorSeleccionado);

            //} catch (Exception ex){
            //    Console.WriteLine(ex);          
            //}

            //------------------------HAPAG-------------------
            textoMBL = "{\"ErrorMessage\":\"\",\"OutputInformation\":null,\"AvailablePages\":13,\"ProcessedPages\":2,\"OCRText\":[[\"Ballindamm 25 - D - 20095 Hamburg VAT-ID - No: DE813960018 carrier: Hapag - Lloyd Aktiengesellschaft, Hamburg Bill of Lading Multimodal Transport or Port to Port Shipment Shipper: CIMC WETRANS DELFIN LOGISTICS(HK) CO., LIMITED UNIT 10, 23F, GLOBAL GATEWAY TOWER, 63 WING HONG STREET, KL, HONG KONG TEL: (852) 3619 9532 Hapag - Lloyd Carrier's Reference: B/L-No.: Page: 16504843 HLCUSZX2402BELA9 2 / 3 Consignee (not negotiable unless consigned to order): DELFIN GROUP CO S.A.C. RUC: 20516667550 CALLE ANTEQUERA 777 PISO 12 SAN ISIDRO LIMA PERU TEL:51 6153535 E-MAIL: OPERATIONS@DELFINGROUPCO.COM.PE Export References: Forwarding Agent: Notify Address (Carrier not responsible for failure to notify; see clause 20 (1) hereof): DELFIN GROUP CO S.A.C. RUC: 20516667550 CALLE ANTEQUERA 777 PISO 12 SAN ISIDRO LIMA PERU TEL:51 6153535 E-MAIL: OPERATIONS@DELFINGROUPCO.COM.PE Consignee's Reference: Vessel(s): Voyage - No.: HONG AN TAI 588 2403010000 Place of Receipt: Port of Loading: NANSHA, CHINA Port of Discharge: CALLAO, PERU Place of Delivery: Container Nos., Seal Nos.; Marks and Nos.HLXU 8121211 SEAL: HLG1933508 MARKS &NOS: OSTER Number and Kind of Packages, Description 1 CONT. 40'X9'6\" 274 CARTONS 274 CARTONS WITH REFRIGERATOR PO 6100011057 of Goods HIGH CUBE CONT. MINIBAR *SLAC = Shipper's Load, Stow, Weight CONSIGNEE'S RUC NUMBER : 20516667550 SHIPPED ON BOARD, DATE : PORT OF LOADING: NANSHA, VESSEL NAME: HONG AN TAI FREIGHT COLLECT Shipper's declared Value [see clause 7(2) and 7(3)] and Count 02/MAR/2024 CHINA 588 VOYAGE: 2403010000 Total No. of Containers received by the Carrier: 1 Packages received by the Carrier: Movement: FCL/FCL Currency: USD Gross Weight: Measurement: L C* 7069.000 71.240 KGS CBM Above Particulars as declared by Shipper. Without responsibility or warranty as to correctness by Carrier [see clause 11] DRAFT Charge Rate Basis Wt/Vol/Val P/C Amount RECEIVED by the Carrier from the Shipper in apparent good order and condition (unless otherwise noted herein) the total number or quantity of Containers or other packages or units indicated in the box opposite entitled \"Total No. of Containers/Packages received by the Carrier\" for Carriage subject to all the terms and conditions hereof (INCLUDING THE TERMS AND CONDITIONS ON THE REVERSE HEREOF AND THE TERMS AND CONDITIONS OF THE CARRIER'S APPLICABLE TARIFF) from the Place of Receipt or the Port of Loading, whichever is applicable, to the Port of Discharge or the Place of Delivery, whichever is applicable. One original Bill of Lading, duly endorsed, must be surrendered by the Merchant to the Carrier in exchange for the Goods or a delivery order. In accepting this Bill of Lading the Merchant expressly accepts and agrees to all its terms and conditions whether printed, stamped or written, or otherwise incorporated, notwithstanding the non-signing of this Bill of Lading by the Merchant. IN WITNESS WHEREOF the number of original Bills of Lading stated below all of this tenor and date has been signed, one of which being accomplished the others to stand void. Place and date of issue: CALLAO, PERU 02/MAR/2024 Freight payable at: Number of original Bs/L: THREE Total Freight Prepaid 280.14 Total Freight Collect 1950.00 Total Freight 2230.14 FOR ABOVE NAMED CARRIER HAPAG-LLOYD PERU S.A.C. (AS AGENT) \",\"Hapag-Lloyd Aktiengesellschaft, Hamburg Page 3 / 3 lrE Hapag-Lloyd B/L-No. HLCUSZX2402BELA9 Cont/Seals/Marks Packages/Description of Goods Weight Measure ALL CHARGES REFLECTED ON BILL OF LADING ARE PART OF TRANSPORT SERVICES FROM UNDER SHIP'S TACKLE TO UNDER SHIP'S TACKLE. FCL/FCL MEANS THAT EACH CONTAINER IS RECEIVED FROM THE SHIPPER PACKED AND CLOSED TO BE CARRIED AND DELIVERED TO THE RECEIVER CLOSED FOR UNPACKING. THE TERM FCL/FCL DOES NOT DEAL WITH WHEN OR WHERE THE CARGO IS RECEIVED OR IS TO BE DELIVERED. THIS, IT DOES NOT ALTER THE TACKLE TO TACKLE PROVISION. Merchants acknowledge and accept that additional charges and service fees related to delivery of cargo and equipment occurring in Peru are applicable. These services are provided and invoiced by local companies and to be paid by merchants. Charge code, Charge Desc, Currency, Rate, Unit, VAT, Invoiced by TD, BL/SWB Fee, USD, 98, Per Document, +18%, Port Agent GDCE, Container fee Expo, USD, 122, Per Box, +18%, Port Agent GDCI, Container fee Impo, USD, 182, Per Box, +18%, Port Agent GATE OUT, Expo empty handling , USD, 133, Per Box, +18%, Depot GATE IN, Impo empty handling , USD, 198, Per Box, +18%, Depot / Port Agent Please check below link to validate detailed PAITA rates Detailed information about these standard local charges can be found inhttps://www.hapag-lloyd.com/perulocalrates Merchants acknowledge and accept that prior approval from the carrier is required for cargo release CHARGE RATE BASIS W/M/V CURR EXPORT SERVICE FEE 64.00 CTR 1 CNY THC ORIGIN 1538.00 CTR 1 CNY DOCUMENT FEE 400.00 BIL 1 CNY ARBITRARY ORIGIN 290.00 CTR 1 USD CARR. SECURITY FEE 15.00 CTR 1 USD MARINEFUEL RECOVER 894.00 CTR 1 USD THC DESTINATION 75.00 CTR 1 USD LUMPSUM USD PREPAID CNY 2002.00 * USD 0.139926 TOTAL PREPAID USD PREPAID COLLECT 64.00 1538.00 400.00 ■,..1290.00 15.00 894.00 75.00 676.00 280.14 280.14 \"]],\"OutputFileUrl\":\"http://147.135.97.123/uploads/_output/bdde_5f837322-afdb-40f3-a60f-8d4ab50ffee2.doc\",\"OutputFileUrl2\":\"\",\"OutputFileUrl3\":\"\",\"Reserved\":[],\"OCRWords\":[],\"TaskDescription\":null}";
            textoHBL = "{\"ErrorMessage\":\"\",\"OutputInformation\":null,\"AvailablePages\":12,\"ProcessedPages\":1,\"OCRText\":[[\"Shipper HOMA APPLIANCES CO., LTD. NORTH SHENGHUI INDUSTRY ZONE, NANTOU, ZHONGSHAN, GUANGDONG, CHINA Consignee AZAFRE PERU S. A. C. RUC: 20563718693 MALECON ARMENDARIZ NRO. 139 DPTO. 1401 MIRAFLORES, LIMA, PERU TEL: 511-4895697 Notify party AZAFRE PERU S. A. C. RUC: 20563718693 MALECON ARMENDARIZ NRO. 139 DPTO. 1401 MIRAFLORES, LIMA, PERU TEL: 511-4895697 MB/L Number HB/L Number DLSZ24030014 OCEAN BILL OF LADING CIMC WETRANS CIMC Wetrans Delfin Logistics (HK) Co., Limited Tn.e Goo-di and inamiliciti are accepled and deals voiih vibjeci. No Ole SgarKfald CorilitHyrt prirrhed cruellest Taken in Charge in appanerg. 1:113.3d order sod borklititirs, unleal diherwise noted herein. At place of retch far transport and delivrry as mentioned below_ One Of Woe- Cornteleil Transport Sills of Loading Must be Su. fencledecl 0utr endefied in fnxch.ango !or the neEmi5. In Witness wirt.pe of 1ha origin.al Combined Trampler 15411 of ladling all of this tenor and date have been signed in die number stated below. one or which berg ac ..x.enob.Peci uhe *11%.2.0.5) so be Pre-carriage by Place of receipt Ocean Vessel Voy No. I Port of loading HONG AN TAI 588 V. 2403010000 NANSHA, CHINA Port of discharge CALLAO, PERU IPlace of delivery CALLAO, PERU For Delivery of goods please apply to: DELFIN GROUP CO S. A. C. RUC: 20516667550 CALLE ANTEQUERA 777 PISO 12 SAN ISIDRO LIMA PERU TEL:51 6153535 E-MAIL: OPERATIONS@DELFINGROUPCO. COM. PE Container No. ;Seal No. Marks and Numbers OSTER *****IMPORTANT ADVISORY*CARRIAGE TO PERUVIAN PORTS IS SUBJECT TO LOCAL CHARGES THAT SHALL BE INVOICED AND PAYABLE IN PERU TO THE AGENT OF THE SHIPPING LINE ACTING AS EFFECTIVE CARRIER AND/OR THEIR DESIGNATED EMPTY CONTAINER DEPOT AND TO DELFIN GROUP CO. S. A. C. , WHICH RATES ARE PUBLISHED AT W. CAL1ACONLINE. COM AND HTTP: // WV. DELFINGROUPCO. COM. PE , RESPECTIVELY, AND WHICH THE MERCHANT HEREBY EXPRESSLY ACKNOWLEDGES AND ACCEPTS. Number of ; Kind of packages; Description of Goods ; Gross Weight Containers or packages SAID TO CONTAIN 274 CARTONS 274 CARTONS WITH MINIBAR REFRIGERATOR PO 6100011057 1*40HC HLXU8121211/HLG1933508 274CARTONS 7069.000KGS 71.240CBM SHIPPER\'S LOAD, STOW, COUNT AND SEALED. ; Measurement 7069. 000 KGS 71. 240 CBM * Total Number of Containers or other packages(in words.) SAY TWO HUNDRED AND SEVENTY FOUR CARTONS ONLY Freight and Charges FREIGHT COLLECT Revenue tons Rate per Prepaid Collect Exchaner rate Prepaid at Payable at DESTINATION Place and date of issue SHENZHEN, CHINA 2024-03-02 Total prepaid Number of original B(s)/L THREE (3) i Date 2024-03-02 1 Signature \"]],\"OutputFileUrl\":\"http://147.135.97.123/uploads/_output/0c3b_07f6253c-b4ff-41f6-9239-0ae1913f1f58.doc\",\"OutputFileUrl2\":\"\",\"OutputFileUrl3\":\"\",\"Reserved\":[],\"OCRWords\":[],\"TaskDescription\":null}";

            //-------------------------MSC--------------------------
            //textoMBL = "{\"ErrorMessage\":\"\",\"OutputInformation\":null,\"AvailablePages\":23,\"ProcessedPages\":2,\"OCRText\":[[\"See websde for large version of the reverse Ver paging Web para terminos y condiciones Cm:iron - re BeC - caCir nrm 03HaKOMfeHtnp C yorionyhf nu II 110110) KeH 1Mtl I 1 -,1114.r.441:4114,itt - Tit490.Mk - www.m9c.com MEDITERRANEAN SHIPPING COMPANY S.A.SC 12 - 14, chemin Rieu -CH - 1208 GENEVA, Switzerland website: www.msc.com SEA WAYBILL No. MEDUEP874872 NOT NEGOTIABLE - COPY \"Port-To-Port\" or \"Combined Transport\"(see Clause 1) NO.& SEQUENCE OF SEA WAYBILLS NO. OF RIDER PAGES One SHIPPER: CIMC WETRANS DELFIN LOGISTICS (HK)CO., LIMITED UNIT 10, 23F, GLOBAL GATEWAY TOWER, 63 WING HONG STREET, KL, HONG KONG TEL:(852) 3619 9532 CONSIGNEE: DELFIN GROUP CO S.A.C. RUC: 20516667550 CALLE ANTEQUERA 777 PISO 12 SAN ISIDRO LIMA PERU TEL:51 6153535 E-MAIL:OPERATIONS@DELFINGROUPCO.COM.PE NOTIFY PARTIES : (No responsibility shall attach to Carder or to his Agent for failure to notify - see Clause 20) DELFIN GROUP CO S.A.C. RUC: 20516667550 CALLE ANTEQUERA 777 PISO 12 SAN ISIDRO LIMA PERU TEL:51 6153535 E-MAIL:OPERATIONS@DELFINGROUPCO.COM.PE CARRIERS AGENTS ENDORSEMENTS: (Include Agent(s) at POD) SHIPPER'S LOAD, STOW AND COUNT SHIPPER'S LOAD, COUNT AND SEALED. Carrier has no liability or responsibility whatsoever for thermal loss or damage to the goods by reason of natural variations M atmospheric temperatures during the winter period, and / or caused by inadequate packing of the Goods for carnage in chywan containers, and / or inherent vice of the Goods, in such temperatures. FCLFCL SAID TO CONTAIN Lloyds/IMO Number: 9406738 The Peruvian local charges '13ESPACHOOOCUMENTARIO\", \"DESPACHO DE CONTENEDOR\" and, ..GATE IWO, are due and payable M destination by the MerchaM M accordance with Carrier's terms and Conditions available M lenvw.msc.com/per/contract-of-caniagehnsc-perwterrns-oonditions. Merchants' attention is brought to the fact that M application of the Peruvian legislative Decree n1492 dated May 10th, 2020, Peruvian customs have full control over cargo delivery after discharge. The Carrier is not in position to control the release process and is therefore not responsible for delivery of cargo without the preseMation of the original bill of lading DUE TO [Continued in the Description section] PORT OF DISCHARGE AGENT: MEDITERRANEAN SHIPPING COMPANY DEL PERU SA.C.Office Callao Av. Nestor Gambeta 358 Callao Phone : +51 1 613 7200 Fax : +51 1 613 7201 Service Contract Number 82174-46-ST IIII ■ I 101 VESSEL AND VOYAGE NO (see Clause 8 & 9) ATHOS - 2409E PORT OF LOADING QINGDAO PLACE OF RECEIPT: (Combined Transport ONLY - see Clause 1 & 5.2) )00000000000000a BOOKING REF. 177SKYKYA2436VFB (or) SHIPPER'S REF. PORT OF DISCHARGE Callao, Peru PLACE OF DELIVERY : (Combined Transport ONLY - see Clause 1 & 5.2) )000000000000000f PARTICULARS FURNISHED BY THE SHIPPER - NOT CHECKED BY CARRIER - CARRIER NOT RESPONSIBLE (seeClausel4) Container Numbers, Seal Numbers and Marks Description of Packages and Goods (Continued on attached Bill of Lading Rider pages(s), if applicable) Gross Cargo Weiaht Measurement continued from Carrier's Agent Endorsements PERUVIAN DECREE #1492, CARRIER'S LIABILITY CEASES AFTER DISCHARGE OF GOODS INTO THE PORT TERMINAL, THE CARRIER WILL NOT BE IN POSITION TO ENSURE CARGO DELIVERY AFTER DISCHARGE AND THEREFORE SHALL NOT BE RESPONSIBLE FOR DELIVERY OF CARGO WITHOUT THE PRESENTATION OF THE ORIGINAL BILL OF LADING THE CONTRACT OF CARRIAGE INCLUDES THE FOLLOWING LOCAL CHARGES IN PERU : DESPACHO DOCUMENTARIO ; DESPACHO DE CONTENEDOR AND GATE IN/OUT, SUBJECT TO THE CARRIER'S TERMS AND CONDITIONS. (www.msc.com/per/contract-of-carriagelmsc-peru-terms-conditions) In application of Peruvian Legislative Decree 1492, Carrier is not allowed to request surrender of an original Bill of Lading by Consignee as a pre-requisite to cargo delivery in Peru. Therefore, Carrier shall not have any liability whatsoever in connection with cargo delivered without prior presentation of an Original Bill of Lading. Please see attached RIDER for Container / Cargo Description(s). 2 x 40' HIGH CUBE Total Items : 37 Total Gross Weight : 36000.000 Kgs. Freight Collect s• . FREIGHT & CHARGES Cargo shall not be delivered unless Freight & Charges are paid(see Clause 16) FREIGHT & CHARGES BASIS RATE PREPAID COLLECT Ocean Freight 2 USD 1,350.00 USD 2,700.00 Terminal Handling Charges 2 USD 65.00 USD 130.00 THC 2 CNY 1,012.00 CNY 2,024.00 At origin-POL BILL Fee 1 CNY 450.00 CNY 450.00 Declared Value : TOTAL FREIGHT & CHARGES USD 2,830.00 CNY 2,474.00 RECEIVED by the Carrier from the Shipper in apparent good order and conditior unless otherwise stated herein the total number or quantity of containers or other packages or units indicated in box entitled \"Carrier's Receipt' for carriage subject to all the term hereof from the Place of Receipt or the Port of Loading, to the Port o Discharge or Place of Delivery, whichever is applicable. IN ACCEPTING THIS SEA WAYBILL THE SHIPPER EXPRESSLY ACCEPTS AND AGREES TO, ON HIS OWN BEHALF AND ON BEHALF OF THE CONSIGNEE, THE OWNER OF GOODS AND THE MERCHANT, AND WARRANTS HE HAS AUTHORITY TO DO SO, ALL THE TERMS AND CONDITIONS WHETHER PRINTED, STAMPED OR OTHERWISE INCORPORATED ON THIS SIDE AND ON THE REVERSE SIDE AND TERMS AND CONDITIONS OF THE CARRIER'S APPLICABLE TARIFF AS IF THEY WERE ALL SIGNED BY THE SHIPPER. Unless instructed otherwise in writing by the Shipper delivery of the Goods will be made only to the Consignee or his authorized representatives. This Sea Waybill is not a document of title to the Goods and delivery will made, after payment of any outstanding Freight and changes, only on provision of proper proof of identity and o authorization at the Port of Discharge or Place of Delivery, as appropriate, without the need to produce or surrender a copy of this Sea Waybill. IN WITNESS WHEREOF the Carrier, Master or their Agent has signed this Sea Waybill. DECLARED VALUE (Only applicable if Ad Valorem charges paid - see Clause 7.3) )000000000000000C CARRIER'S RECEIPT (No. of Cntrs or Pkgs rcvd by Carrier - see Clause 14.1) 2 cntrs PLACE AND DATE OF ISSUE Qingdao, China 02-Mar-2024 SHIPPED ON BOARD DATE 01-Mar-2024 SIGNED on behalf of the Carrier MSC Mediterranean Shipping Company S.A. by As Agent Sea Waybill Standard Edition - 01/2017 TERMS CONTINUED ON REVERSE _t \",\"See website for large version of the reverse I Ver paws Web pars terrrinos y condiciones Cmo-rpii-re seb-caOr Ana 03HaKOrAlleHMA C yCTIOBLIFIMLA t7 110.110;ReHLIRMM I MCSAVA111*.fttiliPLIMM WWW.MSC.COM 1111 MEDITERRANEAN SHIPPING COMPANY S.A. C12-14, chemin Rieu - CH -1208 GENEVA, Switzerland website: www.msc.com SEA WAYBILL No. RIDER PAGE Page 1 of 1 MEDUEP874872 f S CONTINUATION PARTICULARS FURNISHED BY THE SHIPPER - NOT CHECKED BY CARRIER - CARRIER NOT RESPONSIBLE(seeClausel4) Container Numbers, Seal Description of Packages and Goods Gross Cargo Measurement Numbers and Marks (Continued on attached Bill of Ladino Rider oaoes(s). if aoolicable) Weight MEDU8696726 21 Package(s) of GLASS JAR 21,000.000 kgs. 60.000 cu. m. 40' HIGH CUBE Seal Number: FX32685024 Tare Weight: 3,940 kgs. Marks and Numbers: N/M FFAU3796260 16 Package(s) of GLASS JAR 15,000.000 kgs. 45.000 cu. m. 40' HIGH CUBE Seal Number: FX32685099 Tare Weight: 3,700 kgs. Marks and Numbers: N/M Total : 36,000.000 kgs. 105.000 cu. m. %, PLACE AND DATE OF ISSUE Qingdao, China 02-Mar-2024 SHIPPED ON BOARD DATE 01-Mar-2024 SIGNED on behalf of the Carrier MSC Mediterranean Shipping Company S.A. by As Agent Sea Waybill Standard Edition - 01/2017 \"]],\"OutputFileUrl\":\"http://147.135.97.124/uploads/_output/de0c_a01b219c-d7b9-4868-9d6c-6f4b7db49de0.doc\",\"OutputFileUrl2\":\"\",\"OutputFileUrl3\":\"\",\"Reserved\":[],\"OCRWords\":[],\"TaskDescription\":null}";
            //textoHBL = "{\"ErrorMessage\":\"\",\"OutputInformation\":null,\"AvailablePages\":22,\"ProcessedPages\":1,\"OCRText\":[[\"Shipper XLZHOU SELEAD PACKAGING MATERIAL CO. , LTD ADD: 1406, GLOBAL HARBOR, YLEXING GLOBAL CONIMERCIAL CENTER, NO. 8, XLHAI ROAD, XUZHOU CITY, JIANGSU PROVINCE 221003 TEL: +86(516) - 8798 8718 Consignee N &M HOLDING SAC. RUC: 20610764232 ADD: AV MICHAEL FARADAY 729 - ATE - LIMA - PERU Notify party N 8c M HOLDING S. A.C.RUC : 20610764232 ADD: AV MICHAEL FARADAY 729 - ATE - LIMA - PERU MB / L Number HB/ L Number DLQD24020333 OCEAN BILL OF LADING CIMC WETRANS CIMC Wetrans Delfin Logistics(HK) Co., Limited TheCsoa#s and instruclions are accepted and 'leak *A siobjed in Ihe Scandied Corstlitionrs purled averle-af. Taken In-Charge in apparent amid Order and condition, tiniest otherwise noted herein. the place of receipt tar =Ripon araldavtry at mentioned below_ One Of Mei* Combined Tsansport bills al Leading Must be SuRterdeded dirty erk-•sod in exchange for the gond& In Wratnassinhateof Mt original Corrubined Tramporl al Ladling all of this !emir and date haw been titireed in the number stated below_ one or which beam acompintied die uttioni) lo be void. Pre-carriage by I Place of receipt Ocean Vessel ATHOS V. 2409E Voy No. I Port of loading QINGDAO, CHINA Port of discharge CALLAO, PERU Place of delivery CALLAO, PERU For Delivery of goods please apply to: DELFIN GROUP CO S. A. C. RUC: 20516667550 CALLE ANTEQUERA 777 P ISO 12 SAN ISIDRO LIMA PERU TEL:51 6153535 E-MAIL: OPERATIONSHELFINGROUPCO. COM. PE Container No. Seal No. Number of Kind of packages; Description of Goods Marks and Numbers Containers N,M or packages SAID TO CONTAIN 37 PACKAGES *****IMPORTANT ADVISORY=0:*** GLASS JAR CARRIAGE TO PERUVIAN PORTS IS SLBJECT TO LOCAL CHARGES THAT SHALL BE INVOICED AND PAYABLE IN PERU TO THE AGENT OF THE SHIPPING LINE ACTING AS EFFECTIVE CARRIER AND/OR THEIR DESIGNATED EMPTY CONTAINER DEPOT AND TO DELFIN GROLP CO. S. A. C . , WHICH RATES ARE PUBLISHED AT WWW . CALLAOONLINE. COM AND HTTP: // WWW. DELFINGROLP CO. COM. PE , RESPECTIVELY, AND WHICH THE 2*40HC MERCHANT HEREBY EXPRESSLY ACKNOWLEDGES AND ACCEPTS. FFAU3796260/FX32685099 16PACKAGES 15000. 000KGS 45. 000CBM MEDU8696726/FX32685024 21PACKAGES 21000. 000KGS 60.000CBM SHIPPER'S LOAD, STOW, COUNT AND SEALED. :Gross Weight Measurement 36000.000 KGS 105.000 CBM * Total Number of Containers or other packages(in words.) SAY THIRTY SEVEN PACKAGES ONLY Freight and Charges FREIGHT COLLECT Revenue tons Rate per Prepaid Collect Exchaner rate Prepaid at Payable at CAL LAO Place and date of issue QINGDAO, CHINA 2024-03-01 Total prepaid Number of original B(s)IL THREE (3) AS CARRIER Date 9074-03-01 :Signature Sig nature\"]],\"OutputFileUrl\":\"http://147.135.97.123/uploads/_output/6da4_797af61e-aff2-49aa-9278-a4d2c199006d.doc\",\"OutputFileUrl2\":\"\",\"OutputFileUrl3\":\"\",\"Reserved\":[],\"OCRWords\":[],\"TaskDescription\":null}";
            //ProcesaDocumentoMBL(user_name, license_code, direccionMBL);
            //ProcesaDocumentoHBL(user_name, license_code, direccionHBL);
            if (textoMBL != "" && textoHBL != "") {
                switch(lineasCombo.SelectedItem.ToString()){
                    case "HAPAG":
                        procesaHBL(textoHBL);
                        procesaMBLHapag(textoMBL);
                        break;
                    case "MAERSK":
                        //procesaHBL(textoHBL);
                        break;
                    case "MSC":
                        procesaHBL(textoHBL);
                        procesaMBLMSC(textoMBL);
                        break;
                }
            }
            //----ESTO ES LO QUE PROCESA CON EL API OCR ------------------------------------
            //ProcesaDocumentoMBL(user_name, license_code, direccionMBL);
            //ProcesaDocumentoHBL(user_name, license_code, direccionHBL);
            //ESTOS SON LOS PATRONES QUE SE NECESITAN PARA OBTENER LOS VALORES DEL PDF CONVERTIDO A TEXTO



            //Match match = Regex.Match(textoMBL, patronPuertoDescargaHBL, RegexOptions.Singleline);
            //ProcesaDocumentoHBL(user_name, license_code, direccionHBL);

        }

        private void procesaHBL(string texto)
        {
            string patronPuertoDescargaHBL = @"Port of discharge (.*?),";
            string patronMarcasNumerosHBL = @"Marks and Numbers\s(.*?)\s\*";
            string patronPeso = @"\b(\d+(\.\d+)?)\s*KGS";
            string patronVolumen = @"\b(\d+(\.\d+)?)\s*CBM";
            string patronFecha = @"Date\s(\d{4}-\d{2}-\d{2})";
            string patronCantidadContenedor = @"\b\d\*\d{2}[A-Z]+\b";
            string Patrondescripcion = @"SAID TO CONTAIN\s+(.+?)\s+\w+\*";
            List<string> patrones = new List<string>
            { patronPuertoDescargaHBL,
              patronMarcasNumerosHBL,
              patronPeso,
              patronFecha,
              patronVolumen,
              patronCantidadContenedor,
              Patrondescripcion
            };

            for (var i = 0; i < patrones.Count; i++)
            {
                Match match = Regex.Match(texto, patrones[i], RegexOptions.Singleline);
                if (match.Success)
                {
                    switch (patrones[i])
                    {
                        case @"Port of discharge (.*?),":
                            puertoDescargaHBL.Text = match.Groups[1].Value;
                            if (textoMBL.ToUpper().Contains(match.Groups[1].Value.ToUpper()))
                            {
                                puertoDescargaHBL.BackColor = Color.GreenYellow;
                                puertoDescargaMBL.BackColor = Color.GreenYellow;
                                puertoDescargaMBL.Text = match.Groups[1].Value;
                            }
                            break;
                        case @"Marks and Numbers\s(.*?)\s\*":
                            marcasNumerosHBL.Text = match.Groups[1].Value;
                            //Console.WriteLine("Marcas y números: " + match.Groups[1].Value);
                            break;
                        case @"\b(\d+(\.\d+)?)\s*KGS":
                            pesoHBL.Text = match.Groups[1].Value;
                            if(textoMBL.Contains(match.Groups[1].Value)){
                                pesoHBL.BackColor = Color.GreenYellow;
                                pesoMBL.BackColor = Color.GreenYellow;
                                pesoMBL.Text = match.Groups[1].Value;
                            }
                            break;
                        case @"Date\s(\d{4}-\d{2}-\d{2})":
                            string fechaString = match.Groups[1].Value;
                            DateTime fecha = DateTime.ParseExact(fechaString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            string fechaFormateada = fecha.ToString("dd/MMM/yyyy", CultureInfo.InvariantCulture).ToUpper();
                            fechaHBL.Text = fechaFormateada;
                            if (textoMBL.Contains(fechaFormateada))
                            {
                                fechaHBL.BackColor = Color.GreenYellow;
                                fechaMBL.BackColor = Color.GreenYellow;
                                fechaMBL.Text = fechaFormateada;
                            }
                            else {
                                fechaHBL.BackColor = Color.Red;
                            }

                            break;
                        case @"\b(\d+(\.\d+)?)\s*CBM":
                            volumenHBL.Text = match.Groups[1].Value;
                            if (textoMBL.Contains(match.Groups[1].Value))
                            {
                                volumenHBL.BackColor = Color.GreenYellow;
                                volumenMBL.BackColor = Color.GreenYellow;
                                volumenMBL.Text = match.Groups[1].Value;
                            }
                            break;
                        case @"\b\d\*\d{2}[A-Z]+\b":
                            switch (lineasCombo.SelectedItem.ToString())
                            {
                                case "HAPAG":
                                    string primeraParte = "";
                                    string segundaParte = "";
                                    cantidadContenedorHBL.Text = match.Groups[0].Value;
                                    string cantidadContenedor = match.Groups[0].Value;
                                    primeraParte += cantidadContenedor.Substring(0, 1) + " CONT. " + cantidadContenedor.Substring(2, 2);
                                    segundaParte = cantidadContenedor.Substring(4);
                                    //segundaParte += cantidadContenedor.Substring(0, 1) + " CONT." + cantidadContenedor.Substring(2, 2);
                                    switch (segundaParte)
                                    {
                                        case "HC":
                                            segundaParte = "HIGH CUBE";
                                            break;
                                    }
                                    if (textoMBL.Contains(primeraParte))
                                    {
                                        if (textoMBL.Contains(segundaParte))
                                        {
                                            cantidadContenedorHBL.BackColor = Color.GreenYellow;
                                            cantidadContenedorMBL.Text = primeraParte + " " + segundaParte;
                                            cantidadContenedorMBL.BackColor = Color.GreenYellow;
                                        }
                                        else
                                        {
                                            cantidadContenedorHBL.BackColor = Color.Red;
                                        }
                                    }
                                    else
                                    {
                                        cantidadContenedorHBL.BackColor = Color.Red;
                                    }
                                    break;
                                case "MSC":
                                    cantidadContenedorMBL.Text = match.Groups[0].Value.Replace("*", " x ").Replace("HC", "' HIGH CUBE");
                                    cantidadContenedorHBL.Text = match.Groups[0].Value;
                                    if (textoMBL.Contains(match.Groups[0].Value.Replace("*", " x ").Replace("HC", "' HIGH CUBE"))){
                                        cantidadContenedorHBL.BackColor = Color.GreenYellow;
                                        cantidadContenedorMBL.BackColor = Color.GreenYellow;
                                    }
                                    var patronSeal = @"Seal Number:\s([^\s]*)";
                                    Match matchSeal = Regex.Match(textoMBL, patronSeal, RegexOptions.Singleline);
                                    string pattern = @"(\b\w+\b)/"+ matchSeal.Groups[1].Value;
                                    Match matchNumeroContenedor = Regex.Match(textoHBL, pattern, RegexOptions.Singleline);
                                    if(textoHBL.Contains(matchNumeroContenedor.Groups[1].Value)){
                                        numeroContenedorHBL.Text = matchNumeroContenedor.Groups[1].Value;
                                        numeroContenedorMBL.Text = matchNumeroContenedor.Groups[1].Value;
                                        numeroContenedorHBL.BackColor = Color.GreenYellow;
                                        numeroContenedorMBL.BackColor = Color.GreenYellow;
                                    }
                                    break;
                            }
                            break;
                        case @"SAID TO CONTAIN\s+(.+?)\s+\w+\*":
                            descripcionHBL.Text = match.Groups[1].Value.Replace("*****IMPORTANT ADVISORY=0:***","");
                            string descripcion = match.Groups[1].Value.Replace("*****IMPORTANT ADVISORY=0:***", "");
                            string[] palabras = descripcion.Split(' ');
                            var contador = 0;
                            for(var j = 0; j < palabras.Length; j++){
                                if (contador <= 5)
                                {
                                    if (textoMBL.Contains(palabras[j]))
                                    {
                                        contador += 1;
                                    }
                                }
                                else{
                                    //descripcionMBL.Text = "TEXTO DESDE EL ELSE";
                                    descripcionMBL.Text = match.Groups[1].Value.Replace("*****IMPORTANT ADVISORY=0:***", "");
                                    descripcionMBL.BackColor = Color.GreenYellow;
                                    descripcionHBL.BackColor = Color.GreenYellow;
                                }
                            }
                            //string primerasCuatroPalabras = string.Join(" ", palabras.Take(4));
                            //if (textoMBL.Contains(match.Groups[1].Value))
                            //{
                            //    descripcionHBL.BackColor = Color.GreenYellow;
                            //    descripcionHBL.BackColor = Color.GreenYellow;
                            //    descripcionMBL.Text = match.Groups[1].Value;
                            //}
                            break;
                    }
                }
            }
        }

        private void procesaMBLHapag(string texto)
        {
            string patronSealMBL = @"SEAL:\s([^\s]*)";
            string patronNumeroContenedor = @"\b(?:[A-Za-z]+\s+)*\d[\w\s]*?\bSEAL";
            string patronPuertoCargaMBL = @"Port of Loading:(.*?),";
            string patronPuertoDescargaMBL = @"Port of Discharge:(.*?),";
            string patronMarcaNumeroMBL = @"NOS:\s([^\s]*)";
            string PatronDescripcionMBL = @"\\([^ ]+?)(?= of Goods)";
            //string patronMarcasNumerosHBL = @"Mzarks and Numbers\s(.*?)\s\*";
            //string patronPeso = @"\b(\d+(\.\d+)?)\s*KGS";
            //string patronVolumen = @"\b(\d+(\.\d+)?)\s*CBM";
            //string patronFecha = @"Date\s(\d{4}-\d{2}-\d{2})";
            List<string> patrones = new List<string>
            { patronSealMBL,
              patronPuertoCargaMBL,
              patronPuertoDescargaMBL,
              patronNumeroContenedor,
              patronMarcaNumeroMBL,
              PatronDescripcionMBL
            };

            List<string> listaTipoPackete = new List<string>
            {
                "PALLET",
                "CARTON",
                //"PACKAGE",
                "ROLL",
                "BALE",
                "GLASS JAR"
            };
            for (var i = 0; i < patrones.Count; i++)
            {
                Match match = Regex.Match(texto, patrones[i], RegexOptions.Singleline);

                for (var j = 0; j<listaTipoPackete.Count; j++){
                    if (textoHBL.Contains(listaTipoPackete[j])){
                        tipoPaqueteMBL.Text = listaTipoPackete[j];
                        if (textoMBL.Contains(listaTipoPackete[j])){
                            tipoPaqueteMBL.BackColor = Color.GreenYellow;
                            tipoPaqueteHBL.BackColor = Color.GreenYellow;
                            tipoPaqueteHBL.Text = listaTipoPackete[j];
                        }
                    }
                }

                if (match.Success)
                {
                    switch (patrones[i])
                    {
                        case @"SEAL:\s([^\s]*)":
                            sealMBL.Text = match.Groups[1].Value;
                            if(textoHBL.Contains(match.Groups[1].Value)){
                                sealMBL.BackColor = Color.GreenYellow;
                                sealHBL.Text = match.Groups[1].Value;
                                sealHBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        case @"Port of Loading:(.*?),":
                            puertoCargaMBL.Text = match.Groups[1].Value;
                            if (textoHBL.Contains(match.Groups[1].Value))
                            {
                                puertoCargaMBL.BackColor = Color.GreenYellow;
                                puertoCargaHBL.Text = match.Groups[1].Value;
                                puertoCargaHBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        case @"Port of Discharge:(.*?),":
                            puertoDescargaMBL.Text = match.Groups[1].Value;
                            break;
                        case @"NOS:\s([^\s]*)":
                            marcaNumerosMBL.Text = match.Groups[1].Value;
                            if (textoHBL.Contains(match.Groups[1].Value))
                            {
                                marcaNumerosMBL.BackColor = Color.GreenYellow;
                                marcasNumerosHBL.Text = match.Groups[1].Value;
                                marcasNumerosHBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        case @"\b(?:[A-Za-z]+\s+)*\d[\w\s]*?\bSEAL":
                            numeroContenedorMBL.Text = match.Groups[0].Value.Replace("SEAL","").Replace(" ","");
                            //Console.WriteLine("EL NUMERO DEL CONTENEDOR ES:" + match.Groups[0].Value);
                            if (textoHBL.Contains(match.Groups[0].Value.Replace("SEAL", "").Replace(" ", "")))
                            {
                                numeroContenedorMBL.BackColor = Color.GreenYellow;
                                numeroContenedorHBL.Text = match.Groups[0].Value.Replace("SEAL", "").Replace(" ", "");
                                numeroContenedorHBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        case @"\\([^ ]+?)(?= of Goods)":
                            descripcionMBL.Text = match.Groups[0].Value;
                            //Console.WriteLine("EL NUMERO DEL CONTENEDOR ES:" + match.Groups[0].Value);
                            //if (textoHBL.Contains(match.Groups[0].Value))
                            //{
                            //    descripcionMBL.BackColor = Color.GreenYellow;
                            //    descripcionHBL.Text = match.Groups[0].Value;
                            //    descripcionHBL.BackColor = Color.GreenYellow;
                            //}
                            break;
                    }
                }
            }
        }

        private void procesaMBLMSC(string texto){
            string patronFechaMBL = @"DATE (\d{2}-\w{3}-\d{4})";
            string patronSealMBL = @"Seal Number:\s([^\s]*)";
            string patronNumeroContenedor = @"\b(?:[A-Za-z]+\s+)*\d[\w\s]*?\bSEAL";
            string patronPuertoCargaMBL = @"PORT OF LOADING\s(\w*)";
            string patronPuertoDescargaMBL = @"PORT OF DISCHARGE\s(\w*)";
            string patronMarcaNumeroMBL = @"Numbers:\s([^\s]*)";
            string patronPesoMBL = @"Total : (\d+,\d+.\d+) kgs";
            string patronVolumenMBL = @"(\d+.\d+) cu.";
            //string patronMarcasNumerosHBL = @"Mzarks and Numbers\s(.*?)\s\*";
            //string patronPeso = @"\b(\d+(\.\d+)?)\s*KGS";
            //string patronVolumen = @"\b(\d+(\.\d+)?)\s*CBM";
            //string patronFecha = @"Date\s(\d{4}-\d{2}-\d{2})";
            List<string> patrones = new List<string>
            { patronSealMBL,
              patronPuertoCargaMBL,
              patronPuertoDescargaMBL,
              patronNumeroContenedor,
              patronMarcaNumeroMBL,
              patronFechaMBL,
              patronPesoMBL,
              patronVolumenMBL
            };

            List<string> listaTipoPackete = new List<string>
            {
                "PALLET",
                "CARTON",
                //"PACKAGE",
                "ROLL",
                "BALE",
                "GLASS JAR"
            };
            for (var i = 0; i < patrones.Count; i++)
            {
                Match match = Regex.Match(texto, patrones[i], RegexOptions.Singleline);

                for (var j = 0; j < listaTipoPackete.Count; j++)
                {
                    if (textoHBL.Contains(listaTipoPackete[j]))
                    {
                        tipoPaqueteMBL.Text = listaTipoPackete[j];
                        if (textoMBL.Contains(listaTipoPackete[j]))
                        {
                            tipoPaqueteMBL.BackColor = Color.GreenYellow;
                            tipoPaqueteHBL.BackColor = Color.GreenYellow;
                            tipoPaqueteHBL.Text = listaTipoPackete[j];
                        }
                    }
                }

                if (match.Success)
                {
                    switch (patrones[i])
                    {
                        case @"Seal Number:\s([^\s]*)":
                            sealMBL.Text = match.Groups[1].Value;
                            if (textoHBL.Contains(match.Groups[1].Value))           
                            {
                                sealMBL.BackColor = Color.GreenYellow;
                                sealHBL.Text = match.Groups[1].Value;
                                sealHBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        case @"PORT OF LOADING\s(\w*)":
                            puertoCargaMBL.Text = match.Groups[1].Value;
                            if (textoHBL.Contains(match.Groups[1].Value))
                            {
                                puertoCargaMBL.BackColor = Color.GreenYellow;
                                puertoCargaHBL.Text = match.Groups[1].Value;
                                puertoCargaHBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        //case @"PORT OF DISCHARGE\s(\w*)":
                        //    puertoDescargaMBL.Text = match.Groups[1].Value;
                        //    break;
                        case @"Numbers:\s([^\s]*)":
                            marcaNumerosMBL.Text = match.Groups[1].Value;
                            if (textoHBL.Contains(match.Groups[1].Value))
                            {
                                marcaNumerosMBL.BackColor = Color.GreenYellow;
                                marcasNumerosHBL.Text = match.Groups[1].Value;
                                marcasNumerosHBL.BackColor = Color.GreenYellow;
                            }
                            if(textoHBL.Contains(match.Groups[1].Value.Replace("/",","))){
                                marcaNumerosMBL.BackColor = Color.GreenYellow;
                                marcasNumerosHBL.Text = match.Groups[1].Value;
                                marcasNumerosHBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        case @"\b(?:[A-Za-z]+\s+)*\d[\w\s]*?\bSEAL":
                            numeroContenedorMBL.Text = match.Groups[0].Value.Replace("SEAL", "").Replace(" ", "");
                            //Console.WriteLine("EL NUMERO DEL CONTENEDOR ES:" + match.Groups[0].Value);
                            if (textoHBL.Contains(match.Groups[0].Value.Replace("SEAL", "").Replace(" ", "")))
                            {
                                numeroContenedorMBL.BackColor = Color.GreenYellow;
                                numeroContenedorHBL.Text = match.Groups[0].Value.Replace("SEAL", "").Replace(" ", "");
                                numeroContenedorHBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        case @"DATE (\d{2}-\w{3}-\d{4})":
                            fechaMBL.Text = match.Groups[1].Value;
                            DateTime fecha = DateTime.ParseExact(match.Groups[1].Value, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            string fechaConvertida = fecha.ToString("yyyy-MM-dd");
                            if (textoHBL.Contains(fechaConvertida)){
                                fechaHBL.Text = fechaConvertida;
                                fechaHBL.BackColor = Color.GreenYellow;
                                fechaMBL.BackColor = Color.GreenYellow;
                            }
                            break;
                        case @"Total : (\d+,\d+.\d+) kgs":
                            pesoMBL.Text = match.Groups[1].Value;
                            if(textoHBL.Contains(match.Groups[1].Value.Replace(",",""))){
                                pesoMBL.BackColor = Color.GreenYellow;
                                pesoHBL.Text = match.Groups[1].Value;
                                pesoHBL.BackColor = Color.GreenYellow;
                            }
                            Console.WriteLine(match.Groups[1].Value.Replace(".", ""));
                            break;
                        case @"(\d+.\d+) cu.":
                            volumenMBL.Text = match.Groups[1].Value;
                            if (textoHBL.Contains(match.Groups[1].Value)){
                                volumenHBL.Text = match.Groups[1].Value;
                                volumenHBL.BackColor = Color.GreenYellow;
                                volumenMBL.BackColor = Color.GreenYellow;
                            }
                            break;
                    }
                }
            }
        }
    }
}
