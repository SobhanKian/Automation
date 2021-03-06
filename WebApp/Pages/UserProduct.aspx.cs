﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Classes.Pages.Product;
using Utilities;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using WebApp.Classes.Base;
using System.IO;
using Models.Generated.AutomationDB.Automation;
using System.Drawing.Drawing2D;

namespace WebApp.Pages
{
    public partial class UserProduct : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var actions = new List<WebComponent.ComboBox.Actions>();
            actions.Add(new WebComponent.ComboBox.Actions() { gid = "3BCAB3FD-00B6-43F6-BEA3-DC4693693DC6", Title = "ویرایش" });
            actions.Add(new WebComponent.ComboBox.Actions() { gid = "E80794FA-56DD-436B-A635-91FE8F744AFF", Title = "حذف" });
            cmbAction.SetActions(actions);
        }
        public class ProductgDetailPersianDate
        {
            public long ID { get; set; }
            public string Username { get; set; }
            public string RoleTitle { get; set; }
            public string ProductName { get; set; }
            public string ProductModel { get; set; }
            public string ProductStatus { get; set; }
            public string ReceiveTime { get; set; }
            public string SerialNo { get; set; }
            public string UserProductBarcodNo { get; set; }
            public string Description { get; set; }
            public string ProductClassification { get; set; }
            public string ProductBrand { get; set; }
        }
        #region WebMethods
        [WebMethod]
        public static string[] GetTableRows(string Info)
        {
            try
            {
                long ClassificationID = 0;
                switch (Info)
                {
                    case "electro":
                        ClassificationID = 1;
                        break;
                    case "asasie":
                        ClassificationID = 2;
                        break;
                    case "sayer":
                        ClassificationID = 3;
                        break;
                    case "all":
                        ClassificationID = 0;
                        break;
                    default:
                        ClassificationID = 0;
                        break;
                }

                var ProductInfoBiz = Business.FacadeAutomation.GetVwUserProductDetailBusiness();
                var q = ProductInfoBiz.GetAll(300);
                if (ClassificationID != 0)
                    q.And(VwUserProductDetail.Columns.ProductClassificationID, ClassificationID);
                q.OrderBy(VwUserProductDetail.Columns.ID, "DESC");

                var ProductInfo = ProductInfoBiz.Fetch(q).ToList();

                var ProductPersList = new List<ProductgDetailPersianDate>();
                foreach (var item in ProductInfo)
                {
                    var newpro = new ProductgDetailPersianDate();
                    newpro.ID = item.ID;
                    newpro.Username = item.Username;
                    newpro.RoleTitle = item.RoleTitle;
                    if (!item.ProductName.IsNullOrEmpty())
                        newpro.ProductName = item.ProductName;
                    if (!item.ProductModel.IsNullOrEmpty())
                        newpro.ProductModel = item.ProductModel;
                    if (!item.ProductStatus.IsNullOrEmpty())
                        newpro.ProductStatus = item.ProductStatus;
                    if (!item.ReceiveTime.ToString().IsNullOrEmpty())
                        newpro.ReceiveTime = item.ReceiveTime.ToNullablePersianDateTime();
                    if (!item.UserProductBarcodNo.IsNullOrEmpty())
                        newpro.UserProductBarcodNo = item.UserProductBarcodNo;
                    if (!item.SerialNo.IsNullOrEmpty())
                        newpro.SerialNo = item.SerialNo;
                    if (!item.Description.IsNullOrEmpty())
                        newpro.Description = item.Description;
                    if (!item.ProductClassification.IsNullOrEmpty())
                        newpro.ProductClassification = item.ProductClassification;
                    if (!item.ProductBrand.IsNullOrEmpty())
                        newpro.ProductBrand = item.ProductBrand;
                    ProductPersList.Add(newpro);
                }
                var json = JsonConvert.SerializeObject(ProductPersList);

                return new string[2] { "1", json };
            }
            catch (Exception ex)
            {
                return new string[2] { "0", ex.Message };
            }

        }
        [WebMethod]
        public static string[] DeleteUserProduct(int Info)
        {
            try
            {

                var userproductInfo = Business.FacadeAutomation.GetPersonalProductsBusiness().GetByID(Info);
                if (userproductInfo == null)
                    throw new Exception("وسیله کاربر یافت نشد");
                userproductInfo.Delete();

                var json = JsonConvert.SerializeObject(userproductInfo);

                return new string[2] { "1", json };
            }
            catch (Exception ex)
            {
                return new string[2] { "0", ex.Message };
            }

        }

        [WebMethod]
        public static string[] GetInfo(int Info)
        {
            try
            {

                var userproductInfo = Business.FacadeAutomation.GetVwUserProductDetailBusiness().GetByID(Info);
                if (userproductInfo == null)
                    throw new Exception("وسیله کاربر یافت نشد");

                var json = JsonConvert.SerializeObject(userproductInfo);

                return new string[2] { "1", json };
            }
            catch (Exception ex)
            {
                return new string[2] { "0", ex.Message };
            }

        }
        [WebMethod]
        public static string[] SaveInfo(string Info)
        {
            try
            {
                var userProduct = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<UserProductInfo>(Info);

                var userProductInfo = Business.FacadeAutomation.GetPersonalProductsBusiness().GetByID(userProduct.ID.ToLong());
                if (userProductInfo == null)
                {
                    userProductInfo = new Models.Generated.AutomationDB.Automation.PersonalProduct();
                    userProductInfo.ReceiveTime = DateTime.Now;
                    userProductInfo.UserProductBarcodNo = "UserID=" + userProduct.UserID.ToString() + "Date=" + userProductInfo.ReceiveTime.ToNullablePersianDate() + "ProductID=" + userProduct.ProductID.ToString();
                }

                userProductInfo.UserID = userProduct.UserID;
                userProductInfo.ProductID = userProduct.ProductID;
                userProductInfo.Save();
                var productdata = Business.FacadeAutomation.GetVwUserProductDetailBusiness().GetByID(userProductInfo.ID.ToLong());
                var QRData = "شرکت اینتک واحد " + productdata.RoleTitle + " " + productdata.FirstName + " " + productdata.LastName + System.Environment.NewLine + " تاریخ تحویل : " + userProductInfo.ReceiveTime.ToNullablePersianDateforJson() + System.Environment.NewLine + " وسیله : " + productdata.ProductName + System.Environment.NewLine + " مدل و سریال : " + productdata.ProductBrand + productdata.SerialNo;
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRData, QRCodeGenerator.ECCLevel.Q);
                //AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(2);

                var rectf = new RectangleF(40, 128, 10, 10); //rectf for My Text
                using (Graphics g = Graphics.FromImage(qrCodeImage))
                {
                    g.DrawRectangle(new Pen(Color.Red, 1), 40, 128, 10, 10);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString("Hello", new System.Drawing.Font("Tahoma", 4, FontStyle.Bold), Brushes.Black, rectf, sf);
                }


                if (Directory.Exists(@"C:/inetpub/wwwroot/AutomationV2/Publish/Images"))
                    qrCodeImage.Save(@"C:/inetpub/wwwroot/AutomationV2/Publish/Images/" + userProductInfo.UserProductBarcodNo + ".jpg", ImageFormat.Jpeg);
                return new string[2] { "1", "Success" };
            }
            catch (Exception ex)
            {
                return new string[2] { "0", ex.Message };
            }

        }
        [WebMethod]
        public static string[] GetProductDetail(string Info)
        {
            try
            {
                var ProductDetail = Business.FacadeAutomation.GetProductBusiness().GetByType(Info.ToLong());
                var json = JsonConvert.SerializeObject(ProductDetail);


                return new string[2] { "1", json };
            }
            catch (Exception ex)
            {
                return new string[2] { "0", ex.Message };
            }

        }
        [WebMethod]
        public static string[] GetRoleGroupIDDetail(string Info)
        {
            try
            {
                var ProductDetail = Business.FacadeAutomation.GetUserBusiness().GetByRoleGroupID(Info.ToLong());
                var json = JsonConvert.SerializeObject(ProductDetail);


                return new string[2] { "1", json };
            }
            catch (Exception ex)
            {
                return new string[2] { "0", ex.Message };
            }

        }
        #endregion
    }
}