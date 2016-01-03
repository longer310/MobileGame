using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using System.IO;
using GPS.CPModule;
using MobileGame.Core;
using MobileGame.Core.Dapper;

namespace GPSSampleProject
{
    public class Settle
    {
        /// <summary>
        /// According to the contract, Content Providers are required to send HttpRequest before receiving funds
        /// </summary>
        public void GetSettle(int type, string orderID, string orderCurrency, decimal orderAmount, string paid)
        {

            #region Configure for Content Provider

            var cfgDict = GPSCfg.Choose(type);
            //Settings for Cotent Provider (for Alpha Enviroment)
            string szCID = cfgDict["MPS_CID"];
            string szKey = cfgDict["MPS_KEY1"];
            string szIV = cfgDict["MPS_KEY2"];
            string szPassword = cfgDict["MPS_PASSWORD"];

            #endregion

            string orderPAID = paid;
            string szERQC = String.Empty;
            string strXmlDoc = String.Empty;

            //Define TransactionKey Class
            TransactionKey key = new TransactionKey() { Key = szKey, IV = szIV, Password = szPassword };

            #region Assemble Data in XML format
            strXmlDoc = String.Empty;
            strXmlDoc += "<?xml version=\"1.0\"?><TRANS>";
            strXmlDoc += "<MSG_TYPE>" + "0500" + "</MSG_TYPE>";
            strXmlDoc += "<PCODE>" + "300000" + "</PCODE>";
            strXmlDoc += "<CID>" + szCID + "</CID>";
            strXmlDoc += "<COID>" + orderID + "</COID>";
            strXmlDoc += "<CUID>" + orderCurrency + "</CUID>";
            strXmlDoc += "<PAID>" + orderPAID + "</PAID>";
            strXmlDoc += "<AMOUNT>" + orderAmount.ToString("0.00") + "</AMOUNT>";
            strXmlDoc += "<ERQC>" + szERQC + "</ERQC>";
            strXmlDoc += "</TRANS>";

            //Put Data in XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXmlDoc);

            //Obtain ERQC for Future Verification
            szERQC = Common.GetERQC(xmlDoc, key);
            xmlDoc.DocumentElement["ERQC"].InnerText = szERQC;

            //Send to CP_Module
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xmlDoc.WriteTo(xw);
            strXmlDoc = sw.ToString();
            xw.Close();
            sw.Close();
            #endregion

            string strSettleXml = "";
            #region Send to CP_Module
            using (GPSSettle.settle oSettle = new GPSSettle.settle())
            {
                oSettle.Url = cfgDict["GASH_CARD_SETTLE_URL"];
                string tmp = oSettle.getResponse(Common.GetSendData(strXmlDoc));
                strSettleXml = Encoding.UTF8.GetString(Convert.FromBase64String(tmp));
                xmlDoc.LoadXml(strSettleXml);
            }
            #endregion

            string settleStatus = "";
            //Check whether the Transaction is correlative
            if (xmlDoc.DocumentElement["MSG_TYPE"].InnerText != "0510" || xmlDoc.DocumentElement["PCODE"].InnerText != "300000")
            {
                //NOT Correlative Transaction
                settleStatus = "E";
            }
            else
            {
                //Verify the ERPC
                if (!Common.VerifyERPC(ref xmlDoc, xmlDoc.DocumentElement["ERPC"].InnerText, key))
                {
                    //ERPC IS NOT Match
                    settleStatus = "E";
                }
                else
                {
                    #region 請款處理
                    // Check Order Number
                    if (orderID != xmlDoc.DocumentElement["COID"].InnerText)
                    {
                        //The Return Order Number IS NOT Matched
                        settleStatus = "E";
                    }
                    else
                    {
                        #region Update Response Result
                        string settleRCode = xmlDoc.DocumentElement["RCODE"].InnerText;  //Response Code
                        settleStatus = (settleRCode == "0000" ? "s" : "f");       //Settle Status
                        #endregion
                    }
                    #endregion
                }
            }

            var sql =
                "update GashOrder set SettleTime=@SettleTime,SettleStatus=@SettleStatus,SettleResponse=@SettleResponse where OrderId=@OrderId";
            using (var conn = Util.ObtainConn(ParamHelper.GameServer))
            {
                conn.Execute(sql, new { SettleTime = DateTime.Now, SettleStatus = settleStatus, SettleResponse = strSettleXml, OrderId = orderID });
            }
        }
    }
}
