﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DbModel;
using DevExpress.XtraBars.Docking2010.Base;
using DevExpress.XtraTreeList;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;
using System.Data.Entity.Validation;
using System.IO;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraTreeList.Columns;
using System.Runtime.InteropServices;
using DevExpress.XtraBars;

namespace ZovTrade
{
    public partial class FrmDealers : Form
    {
        private DbModel.tradeEntities db;
        private bool showDeletedDealers = false;
        private bool exportAllDealers = false;

        public bool ShowDeletedDealers
        {
            get { return showDeletedDealers; }
            set
            {
                showDeletedDealers = value;
                if (treeDealersList.Columns.ColumnByFieldName("isDeleted") != null)
                {
                    treeDealersList.Columns["isDeleted"].Visible = value;
                }
                GetDealersTree();
            }
        }

        public bool ExportAllDealers
        {
            get { return exportAllDealers; }
            set { exportAllDealers = value; }
        }


        public int TreeListFocusedId
        {
            get
            {
                if (treeDealersList == null)
                    return -1;
                var row = treeDealersList.GetDataRecordByNode(treeDealersList.FocusedNode);
                if (row == null)
                    return -1;
                dynamic treeDealer = new { ID = 0, Dealer_ID = 0, dealerZovName = "" };
                treeDealer = row;
                return treeDealer.ID;
            }
            set { treeListFocusedId = value; }
        }


        //  private DbModel.tradeEntities dbTree = new tradeEntities(DbModel.Tools.TradeConnectionString(Properties.Settings.Default.barcodeCS.ToString()));

        public FrmDealers()
        {
            InitializeComponent();


            #region treeDealersListinit
            treeDealersList.OptionsFilter.FilterMode = FilterMode.Extended;
            treeDealersList.KeyFieldName = "ID";
            treeDealersList.ParentFieldName = "Dealer_ID";


            var treeDealersListColumn1 = treeDealersList.Columns.Add();

            treeDealersListColumn1.Name = "colId";
            treeDealersListColumn1.Caption = "ID";
            treeDealersListColumn1.FieldName = "ID";
            treeDealersListColumn1.Visible = false;

            var treeDealersListColumn2 = treeDealersList.Columns.Add();

            treeDealersListColumn2.Name = "colDealerID";
            treeDealersListColumn2.Caption = "Dealer_ID";
            treeDealersListColumn2.FieldName = "Dealer_ID";
            treeDealersListColumn2.Visible = false;

            var treeDealersListColumn3 = treeDealersList.Columns.Add();

            treeDealersListColumn3.Name = "coldealerZovName";
            treeDealersListColumn3.Caption = "Дилер";
            treeDealersListColumn3.FieldName = "dealerZovName";
            treeDealersListColumn3.SortOrder = SortOrder.Ascending;
            treeDealersListColumn3.OptionsColumn.AllowEdit = false;
            treeDealersListColumn3.Visible = true;

            var treeDealersListColumn4 = treeDealersList.Columns.Add();

            treeDealersListColumn4.Name = "coldealerisDeleted";
            treeDealersListColumn4.Caption = "Удален";
            treeDealersListColumn4.FieldName = "isDeleted";
            treeDealersListColumn4.ColumnEdit = repositoryItemCheckEdit;
            treeDealersListColumn4.OptionsColumn.AllowEdit = true;
            treeDealersListColumn4.Visible = false;


            #endregion

            //listBoxSubDealers.ValueMember = "ID";
            //listBoxSubDealers.DisplayMember = "dealerZovName";




        }


        private void FormMain_Load(object sender, EventArgs e)
        {
            db = new tradeEntities(DbModel.Tools.TradeConnectionString(Properties.Settings.Default.barcodeCS.ToString()));
            //              ID StatusName
            //              1   Новый
            //              2   На рассмотрении
            //              3   OK
            //              4   Закрыт
            //              5   Ok не отображать
            //try
            //{

           
            //var sp1 = db.StatusOfPos.Where(x=>x.ID==1).FirstOrDefault();
            //sp1.StatusColorInt = Color.LightGray.ToArgb();
            //sp1.StatusColor = Color.LightGray.Name;
            //var sp2 = db.StatusOfPos.Where(x => x.ID == 2).FirstOrDefault();
            //sp2.StatusColorInt = Color.Yellow.ToArgb();
            //sp2.StatusColor = Color.Yellow.Name;
            //var sp3 = db.StatusOfPos.Where(x => x.ID == 3).FirstOrDefault();
            //sp3.StatusColorInt = Color.LightGreen.ToArgb();
            //sp3.StatusColor = Color.LightGreen.Name;
            //var sp4 = db.StatusOfPos.Where(x => x.ID == 4).FirstOrDefault();
            //sp4.StatusColorInt = Color.OrangeRed.ToArgb();
            //sp4.StatusColor = Color.OrangeRed.Name;
            //db.SaveChanges();
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
            splashScreenManager1.ShowWaitForm();
           
            db.StatusOfPos.Load();
            GetDealersTree();
            splashScreenManager1.CloseWaitForm();


        }

        private int treeListFocusedId = -1;
        private void treeList1_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e)
        {
            var row = treeDealersList.GetDataRecordByNode(treeDealersList.FocusedNode);
            if (row == null)
            {
                TreeListFocusedId = -1;
                GetDealerInfo(0);

            }
            else
            {
                dynamic treeDealer = new {ID = 0, Dealer_ID = 0, dealerZovName = ""};
                treeDealer = row;
                TreeListFocusedId = treeDealer.ID;
                GetDealerInfo(treeDealer.ID);
            }
            

        }

        private void GetDealersTree()

        {
            if (treeDealersList == null)
                return;

            treeDealersList.BeginUpdate();
            try
            {


                //treeDealersList.DataSource = dbContext.Dealers.Local.ToBindingList();
                treeDealersList.BeginSort();
                var ds = db.Dealers.Where(x => ((x.isDeleted == false) || (x.isDeleted & ShowDeletedDealers))).ToList(); // Select(x => new { x.ID, x.Dealer_ID, x.dealerZovName }).ToList();
//                var ds = db.Dealers.Where(x => ((x.isDeleted == false) || (x.isDeleted & ShowDeletedDealers))).Select(x => new { x.ID, x.Dealer_ID, x.dealerZovName }).ToList();
//                var ds = db.Dealers.Select(x => new { x.ID, x.Dealer_ID, x.dealerZovName }).ToList();

                treeDealersList.DataSource = ds;
                
                treeDealersList.EndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Application.Exit();
            }
            treeDealersList.EndUpdate();
           
            
            //treeDealersList.ExpandAll();
          

        }

        private int dealerId;

        private void cleanDealerData()
        {
            DealerViewDateAdd.EditValue = "";
            DealerViewId.EditValue = "";
            DealerViewDescription.EditValue = "";
            DealerViewFabricName.EditValue = "";
            DealerViewZovName.EditValue = "";
            DealerViewPosCount.EditValue = "";
            DealerViewSubDealersCount.EditValue = "";
            gridControl1.DataSource = null;
        }
        private void GetDealerInfo(int _dealerId)
        {
            //db.Dealers.Local.Clear();
            //db.Dealers.Where(x=>x.Dealer_ID==dealerId).Load();
            dealerId = _dealerId;
            cleanDealerData();
            if (dealerId == 0)
            {
                
                return;
            }
            var curDealer =
                db.Dealers.Where(x => x.ID == dealerId)
                    .Select(
                        x =>
                            new
                            {
                                x.ID,
                                x.dateadd,
                                x.dealerName,
                                x.dealerZovName,
                                x.dealerDescription,
                                posCount = x.Pos.Count,
                                subDealersCount = x.DealerChilds.Count
                            }).FirstOrDefault();
            if (curDealer != null)
            {
                DealerViewDateAdd.EditValue = curDealer.dateadd;
                DealerViewId.EditValue = curDealer.ID;
                DealerViewDescription.EditValue = curDealer.dealerDescription;
                DealerViewFabricName.EditValue = curDealer.dealerZovName;
                DealerViewZovName.EditValue = curDealer.dealerName;
                DealerViewPosCount.EditValue = curDealer.posCount;
                DealerViewSubDealersCount.EditValue = curDealer.subDealersCount;
            }


            var dealerIdList = new List<int>();
            dealerIdList.Add(dealerId);
            loadPosListFromDict(dealerIdList);

            //listBoxSubDealers.DataSource =
            //    db.Dealers.Where(x => x.Dealer_ID == dealerId).Select(d => new {d.ID, d.dealerZovName}).ToList();
            gridControl2.DataSource = db.Contacts.Where(x => x.Dealers.Where(d => d.ID == dealerId).Any()).Select(x => new { x.ContactName, x.ContactPhones, x.ContactOtherData, x.ContactDescription }).ToList();
            gridViewPos.BestFitColumns(true);

        }

        private void treeList1_AfterCheckNode(object sender, NodeEventArgs e)
        {
            Debug.WriteLine(treeDealersList.Nodes.IndexOf(treeDealersList.FocusedNode).ToString());
        }

        private void layoutControlItem4_DoubleClick(object sender, EventArgs e)
        {

        }

        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //var dict= new List<string>();
            db.Dealers.Load();
            db.DealerLegalNames.Load();
            db.PosTypes.Load();
            if (!db.PosTypes.Any(x => x.posTypeName == "не указан"))
            {
                var postType = db.PosTypes.Create();
                postType.posTypeName = "не указан";
                db.PosTypes.Add(postType);
            }
            var oExcelApp =
                (Excel.Application) System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");

            foreach (Excel.Workbook excelworkbook in oExcelApp.Workbooks)
            {
                if (!excelworkbook.Name.StartsWith("Map.New")) continue;
                foreach (Excel.Worksheet excelsheet in excelworkbook.Worksheets)
                {
                    if (excelsheet.Name.ToString() != "Объекты") continue;
                    int global_row = 2;
                    var r = excelsheet.Cells[global_row, 1] as Excel.Range;
                    var r2 = excelsheet.Cells[global_row, 3] as Excel.Range;
                    while (r != null && r.Value != null)
                    {
                        if (r2 != null && r2.Value != null)
                        {
                            var rRubyId = ((Excel.Range) excelsheet.Cells[global_row, 1]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 1]).Value2.ToString(); //Дилер
                            var rDealer = ((Excel.Range) excelsheet.Cells[global_row, 3]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 3]).Value2.ToString(); //Дилер
                            var rFabricName = ((Excel.Range) excelsheet.Cells[global_row, 4]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 4]).Value2.ToString();
                            //Название на фабрике                        
                            var rLegalName = ((Excel.Range) excelsheet.Cells[global_row, 2]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 2]).Value2.ToString();
                            //Юридическое наименование клиента
                            var rYandexAddress = ((Excel.Range) excelsheet.Cells[global_row, 5]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 5]).Value2.ToString(); //Адрес для Яндекса
                            var rCity = ((Excel.Range) excelsheet.Cells[global_row, 6]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 6]).Value2.ToString(); //Город
                            var rStreetAddress = ((Excel.Range) excelsheet.Cells[global_row, 7]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 7]).Value2.ToString();
                            //Улица и номер дома
                            var rCoordinates = ((Excel.Range) excelsheet.Cells[global_row, 8]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 8]).Value2.ToString(); //Координаты точки
                            var rPosAddress = ((Excel.Range) excelsheet.Cells[global_row, 9]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 9]).Value2.ToString(); //Адрес
                            var rPosName = ((Excel.Range) excelsheet.Cells[global_row, 10]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 10]).Value2.ToString();
                            //Название магазина
                            var rPosPhones = ((Excel.Range) excelsheet.Cells[global_row, 11]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 11]).Value2.ToString(); //Телефоны
                            var rPosArea = ((Excel.Range) excelsheet.Cells[global_row, 12]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 12]).Value2.ToString();
                            //Площадь торговой точки
                            var rPosBrend = ((Excel.Range) excelsheet.Cells[global_row, 13]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 13]).Value2.ToString(); //Бренд
                            var rPosType = ((Excel.Range) excelsheet.Cells[global_row, 14]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 14]).Value2.ToString(); //Тип салона
                            var rPosImagesPath = ((Excel.Range) excelsheet.Cells[global_row, 15]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 15]).Value2.ToString();
                            //Путь к изображениям
                            var rPosSite = ((Excel.Range) excelsheet.Cells[global_row, 16]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 16]).Value2.ToString(); //Сайт
                            var rPosMail = ((Excel.Range) excelsheet.Cells[global_row, 17]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 17]).Value2.ToString(); //Почта
                            var rPosRating = ((Excel.Range) excelsheet.Cells[global_row, 18]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 18]).Value2.ToString(); //Рейтинг
                            var rPosEnable = ((Excel.Range) excelsheet.Cells[global_row, 19]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 19]).Value2.ToString(); //Вкл/Выкл
                            var rPosSorting = ((Excel.Range) excelsheet.Cells[global_row, 20]).Value2 == null
                                ? ""
                                : ((Excel.Range) excelsheet.Cells[global_row, 20]).Value2.ToString();
                            //Порядок сортировки



                            //if (!db.Dealers.Any(x => x.dealerName == rDealer) && !dict.Contains(rDealer))
                            //{

                            //if (db.PosTypes.Any(x => x.posTypeName == rPosType) && rPosType != String.Empty)
                            //{
                            //    var postType = db.PosTypes.First(x => x.posTypeName == rPosType);
                            //}
                            //else
                            if (!db.PosTypes.Any(x => x.posTypeName == rPosType) && rPosType != String.Empty)
                            {
                                var postType = db.PosTypes.Create();
                                postType.posTypeName = rPosType;
                                db.PosTypes.Add(postType);
                            }
                            //else if (!db.PosTypes.Any(x => x.posTypeName == "не указан"))
                            //{
                            //    var postType = db.PosTypes.Create();
                            //    postType.posTypeName = "не указан";
                            //    db.PosTypes.Add(postType);
                            //}
                            //dict.Add(rDealer);




                            var dealer = db.Dealers.Any(x => x.dealerZovName == rFabricName)
                                ? db.Dealers.FirstOrDefault(x => x.dealerZovName == rFabricName)
                                : db.Dealers.Create();
                            if (dealer.dealerZovName == null)
                            {
                               
                                if (rDealer.ToUpper().Trim() != rFabricName.ToUpper().Trim())
                                {
                                    var parDealer = db.Dealers.FirstOrDefault(x => x.dealerName == rDealer);
                                    if (parDealer == null)
                                    {
                                        parDealer= db.Dealers.FirstOrDefault(x => x.dealerZovName == rDealer);
                                    }
                                    dealer.Dealer_ID = parDealer.ID;
                                }
                                dealer.dealerName = rDealer;
                                dealer.dealerZovName = rFabricName;
                                db.Dealers.Add(dealer);
                            }



                            //db.SaveChanges();

                            var legalName = db.DealerLegalNames.Any(x => x.LegalName == rLegalName)
                                ? db.DealerLegalNames.FirstOrDefault(x => x.LegalName == rLegalName)
                                : db.DealerLegalNames.Create();
                            if (legalName.Dealers == null)
                            {
                                legalName.Dealers = dealer;
                                legalName.LegalName = rLegalName;
                                //legalName.LegalAddress = rStreetAddress;
                                db.DealerLegalNames.Add(legalName);
                            }
                            int refInt = 0;
                            var rubyId = int.TryParse(rRubyId, out refInt) ? refInt : 0;
                            if (!db.Pos.Any(x => x.Ruby_Id == rubyId && rubyId > 0))
                            {


                                var pos = db.Pos.Create();
                                pos.Dealers = dealer;
                                pos.yandexAdress = rYandexAddress;
                                pos.legalName = rPosName;
                                pos.city = rCity;
                                pos.DealerLegalNames=(legalName);
                                pos.Ruby_Id = rubyId;

                                pos.locationDescription = rStreetAddress;
                                //pos.DealerLegalNames.Add(legalName);

                                pos.brand = rPosBrend;
                                pos.posStatus_ID = rPosEnable == "0" ? 3 : 4;

                                pos.street = rPosAddress;
                                pos.PosTypes = db.PosTypes.Any(x => x.posTypeName == rPosType)
                                    ? db.PosTypes.FirstOrDefault(x => x.posTypeName == rPosType)
                                    : db.PosTypes.FirstOrDefault(x => x.posTypeName == "не указан");
                                var tempInt = 0;

                                if (Int32.TryParse(rPosArea, out tempInt))
                                {
                                    pos.posArea = tempInt;
                                }

                                if (rCoordinates != "" || rCoordinates.Contains(","))
                                {
                                    pos.coordstextdata = rCoordinates.Trim();
                                    try
                                    {


                                        pos.longitude =
                                            Double.Parse(
                                                rCoordinates.Substring(0,
                                                    rCoordinates.IndexOf(",", StringComparison.InvariantCulture))
                                                    .Trim()
                                                    .Replace(".", ","));
                                        pos.attitude =
                                            Double.Parse(
                                                rCoordinates.Substring(
                                                    rCoordinates.IndexOf(",", StringComparison.InvariantCulture) + 1,
                                                    rCoordinates.Length -
                                                    rCoordinates.IndexOf(",", StringComparison.InvariantCulture) - 1)
                                                    .Trim()
                                                    .Replace(".", ","));


                                    }


                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }

                                db.Pos.Add(pos);

                                foreach (
                                    var mail in
                                        rPosMail.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (mail.Trim() != String.Empty)
                                    {
                                        var posMail = db.PosEmails.Create();
                                        posMail.Email = mail;
                                        posMail.Pos = pos;
                                        db.PosEmails.Add(posMail);
                                    }
                                }

                                foreach (
                                    var site in
                                        rPosSite.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (site.Trim() != String.Empty)
                                    {

                                        var possite = db.Sites.Create();
                                        possite.URL = site.Trim();
                                        //possite.Pos = pos;
                                        possite.Dealers = dealer;
                                        db.Sites.Add(possite);
                                    }
                                }

                               

                                foreach (
                                    var phone in
                                        rPosPhones.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries)
                                    )
                                {
                                    if (phone.Trim() != String.Empty)
                                    {
                                        var posPhone = db.PosPhones.Create();
                                        posPhone.PhoneNumber = phone;
                                        posPhone.Pos = pos;
                                        db.PosPhones.Add(posPhone);
                                    }
                                }
                            }

                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                               Tools.showDbSaveExceptions(ex);

                            }
                            r2.Cells.Interior.Color = ColorTranslator.ToOle(Color.LightGreen);



                        }
                        r = excelsheet.Cells[global_row++, 1] as Excel.Range;
                        r2 = excelsheet.Cells[global_row, 3] as Excel.Range;
                    }




                }
            }
        }
        
        private void barButtonItem9_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            GetDealersTree();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
           // if (!db.Pos.Where(x => x.dealer_ID == dealerId).Include(p => p.PosTypes).Any()) return;

            if (gridViewPos.RowCount == 0) return;
            var posIdList = new List<int>();

            for (int i = 0; i < gridViewPos.RowCount-1; i++)
            {
                posIdList.Add((int)(gridViewPos.GetRowCellValue(i, "ID")));
            }


          

            var dir = Path.GetDirectoryName(Application.ExecutablePath.ToString());
            using (var tradeHtml = File.Open(dir + @"\web\trade.html", FileMode.Truncate))
            {
                tradeHtml.Close();
            }
            var tradeHtmlString = @"<!DOCTYPE html>" +
                                  @"<html>" +
                                  @"<head>" +
                                  @"    <title>ЗовРеклама</title>" +
                                  @"    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />" +
                                  @"    <script src=""https://api-maps.yandex.ru/2.1/?lang=ru_RU"" type=""text/javascript""></script>" +
                                  @"    <script src=""data.js"" type=""text/javascript""></script>" +
                                  @"    <script src=""https://yandex.st/jquery/1.8.0/jquery.min.js"" type=""text/javascript""></script>"+
                                  @"    <link href=""https://yandex.st/bootstrap/2.2.2/css/bootstrap.min.css"" rel=""stylesheet"">" +
                                  @"	<style>" +
                                  @"        html, body, #map {" +
                                  @"            width: 100%; height: 100%; padding: 0; margin: 0;" +
                                  @"        }" +
                                  @"        #my-listbox {            top: auto;            left: auto;        }" +
                                  @"    </style>" +
                                  @"</head>" +
                                  @"<body>" +
                                  @"<div id=""map""></div>" +
                                  @"</body>" +
                                  @"</html>";

            using (StreamWriter outputFile = new StreamWriter(dir + @"\web\trade.html"))
            {
                outputFile.Write(tradeHtmlString);
            }




            var dataJsString = @"ymaps.ready(init);" + Environment.NewLine +
                               @"function init () {" + Environment.NewLine +
                               @"var myMap = new ymaps.Map(""map"", {" + Environment.NewLine +
                               @"center: [55.76, 37.64]," + Environment.NewLine +
                               @"zoom: 10," + Environment.NewLine +
                               @"controls: []"+Environment.NewLine +
                               @"}, {" + Environment.NewLine +
                               @"searchControlProvider: 'yandex#search'" + Environment.NewLine +
                               @"});" + Environment.NewLine +
                               // Создадим собственный макет выпадающего списка." + Environment.NewLine + 
                               @"var ListBoxLayout = ymaps.templateLayoutFactory.createClass(" + Environment.NewLine +
                               @"    ""<button id='my-listbox-header' class='btn btn-success dropdown-toggle' data-toggle='dropdown'>"" +" +
                               Environment.NewLine +
                               @"        ""{{data.title}} <span class='caret'></span>"" +" + Environment.NewLine +
                               @"    ""</button>"" +" + Environment.NewLine +
                               @"    // Этот элемент будет служить контейнером для элементов списка." +
                               Environment.NewLine +
                               @"    // В зависимости от того, свернут или развернут список, этот контейнер будет" +
                               Environment.NewLine +
                               @"    // скрываться или показываться вместе с дочерними элементами." +
                               Environment.NewLine +
                               @"    ""<ul id='my-listbox'"" +" + Environment.NewLine +
                               @"        "" class='dropdown-menu' role='menu' aria-labelledby='dropdownMenu'"" +" +
                               Environment.NewLine +
                               @"        "" style='display: {% if state.expanded %}block{% else %}none{% endif %};'></ul>"", {" +
                               Environment.NewLine +
                               @"" + Environment.NewLine +
                               @"    build: function() {" + Environment.NewLine +
                               @"        // Вызываем метод build родительского класса перед выполнением" +
                               Environment.NewLine +
                               @"        // дополнительных действий." + Environment.NewLine +
                               @"        ListBoxLayout.superclass.build.call(this);" + Environment.NewLine +
                               @"" + Environment.NewLine +
                               @"        this.childContainerElement = $('#my-listbox').get(0);" + Environment.NewLine +
                               @"        // Генерируем специальное событие, оповещающее элемент управления" +
                               Environment.NewLine +
                               @"        // о смене контейнера дочерних элементов." + Environment.NewLine +
                               @"        this.events.fire('childcontainerchange', {" + Environment.NewLine +
                               @"            newChildContainerElement: this.childContainerElement," +
                               Environment.NewLine +
                               @"            oldChildContainerElement: null" + Environment.NewLine +
                               @"        });" + Environment.NewLine +
                               @"    }," + Environment.NewLine +
                               @"" + Environment.NewLine +
                               @"    // Переопределяем интерфейсный метод, возвращающий ссылку на" + Environment.NewLine +
                               @"    // контейнер дочерних элементов." + Environment.NewLine +
                               @"    getChildContainerElement: function () {" + Environment.NewLine +
                               @"        return this.childContainerElement;" + Environment.NewLine +
                               @"    }," + Environment.NewLine +
                               @"" + Environment.NewLine +
                               @"    clear: function () {" + Environment.NewLine +
                               @"        // Заставим элемент управления перед очисткой макета" + Environment.NewLine +
                               @"        // откреплять дочерние элементы от родительского." + Environment.NewLine +
                               @"        // Это защитит нас от неожиданных ошибок," + Environment.NewLine +
                               @"        // связанных с уничтожением dom-элементов в ранних версиях ie." +
                               Environment.NewLine +
                               @"        this.events.fire('childcontainerchange', {" + Environment.NewLine +
                               @"            newChildContainerElement: null," + Environment.NewLine +
                               @"            oldChildContainerElement: this.childContainerElement" + Environment.NewLine +
                               @"        });" + Environment.NewLine +
                               @"        this.childContainerElement = null;" + Environment.NewLine +
                               @"        // Вызываем метод clear родительского класса после выполнения" +
                               Environment.NewLine +
                               @"        // дополнительных действий." + Environment.NewLine +
                               @"        ListBoxLayout.superclass.clear.call(this);" + Environment.NewLine +
                               @"    }" + Environment.NewLine +
                               @"})," + Environment.NewLine +
                               @"" + Environment.NewLine +
                               @"// Также создадим макет для отдельного элемента списка." + Environment.NewLine +
                               @"ListBoxItemLayout = ymaps.templateLayoutFactory.createClass(" + Environment.NewLine +
                               @"    ""<li><a>{{data.content}}</a></li>""" + Environment.NewLine +
                               @");" + Environment.NewLine +
                               @"listBoxItems = [" + Environment.NewLine;
            var poss2 =
                db.Pos.Where(x => x.dealer_ID == dealerId ).Include(p => p.PosTypes).Select(x =>
                    new { ballonInfo = x.Ruby_Id + " " + x.legalName, ballonDescription = x.yandexAdress, x.longitude,x.attitude })
                    .ToList();
            var poss=db.Pos.Where(x=>posIdList.Contains(x.ID)).Select(x =>
                    new { ballonInfo = x.Ruby_Id + " " + x.legalName, ballonDescription = x.yandexAdress, x.longitude, x.attitude })
                    .ToList();
            foreach (var pos in poss)
            {
                
                    dataJsString +=
                        @"    new ymaps.control.ListBoxItem({" + Environment.NewLine +
                        @"        data: {" + Environment.NewLine +
                        @"            content: '"+pos.ballonInfo+" "+pos.ballonDescription+"'," + Environment.NewLine +
                        @"            center: ["+pos.longitude.ToString().Replace(",", ".") + @", " + pos.attitude.ToString().Replace(",", ".") +"]" + Environment.NewLine +
                        @"        }" + Environment.NewLine +
                        @"    })," + Environment.NewLine;

//                                        @"            zoom: 9" + Environment.NewLine +

                
            }

            dataJsString = dataJsString.Substring(0, dataJsString.Length - 1);

            dataJsString += @"]," + Environment.NewLine +
                            @"" + Environment.NewLine +

                            @"// Теперь создадим список" + Environment.NewLine +
                            @"listBox = new ymaps.control.ListBox({" + Environment.NewLine +
                            @"        items: listBoxItems," + Environment.NewLine +
                            @"        data: {" + Environment.NewLine +
                            @"            title: 'Выберите магазин'" + Environment.NewLine +
                            @"        }," + Environment.NewLine +
                            @"        options: {" + Environment.NewLine +
                            @"            // С помощью опций можно задать как макет непосредственно для списка," +
                            Environment.NewLine +
                            @"            layout: ListBoxLayout," + Environment.NewLine +
                            @"            // так и макет для дочерних элементов списка. Для задания опций дочерних" +
                            Environment.NewLine +
                            @"            // элементов через родительский элемент необходимо добавлять префикс" +
                            Environment.NewLine +
                            @"            // 'item' к названиям опций." + Environment.NewLine +
                            @"            itemLayout: ListBoxItemLayout" + Environment.NewLine +
                            @"        }" + Environment.NewLine +
                            @"    });" + Environment.NewLine +
                            @"" + Environment.NewLine +
                            @"listBox.events.add('click', function (e) {" + Environment.NewLine +
                            @"    // Получаем ссылку на объект, по которому кликнули." + Environment.NewLine +
                            @"    // События элементов списка пропагируются" + Environment.NewLine +
                            @"    // и их можно слушать на родительском элементе." + Environment.NewLine +
                            @"    var item = e.get('target');" + Environment.NewLine +
                            @"    // Клик на заголовке выпадающего списка обрабатывать не надо." + Environment.NewLine +
                            @"    if (item != listBox) {" + Environment.NewLine +
                            @"        myMap.setCenter(" + Environment.NewLine +
                            @"            item.data.get('center')," + Environment.NewLine +
                            @"            item.data.get('zoom')" + Environment.NewLine +
                            @"        );" + Environment.NewLine +
                            @"    }" + Environment.NewLine +
                            @"});" + Environment.NewLine +
                            @"" + Environment.NewLine +
                            @"myMap.controls.add(listBox, {float: 'left'});" +
                            Environment.NewLine;

            
            dataJsString += @" myMap.panTo([" + Environment.NewLine +
                poss.First().longitude.ToString().Replace(",", ".")+
                            @"," +
                poss.First().attitude.ToString().Replace(",", ".") +
                            Environment.NewLine +
                            @"], {" + Environment.NewLine +
                            @"  delay: 1500" + Environment.NewLine +
                            @"});";
            foreach (var pos in poss)
            {
                    dataJsString += @"myMap.geoObjects.add(new ymaps.Placemark([" +
                                    pos.longitude.ToString().Replace(",", ".") + @", " + pos.attitude.ToString().Replace(",", ".") + @"], {" + Environment.NewLine +
                                    @"iconContent: '" + pos.ballonInfo + "'," + Environment.NewLine +
                                    @"hintContent: '" + pos.ballonInfo + @"'," + Environment.NewLine +
                                    @"balloonContent: '"+pos.ballonDescription +@"'" + Environment.NewLine +
                                    @"}, {" + Environment.NewLine +
                                    @"preset: 'islands#icon'," + Environment.NewLine +
                                    @"iconColor: '#0095b6'" + Environment.NewLine +
                                    @"}));" + Environment.NewLine;
            }
            dataJsString += @"}";
            using (var dataJs = File.Open(dir + @"\web\data.js", FileMode.Truncate))
            {
                dataJs.Close();
            }
            using (StreamWriter outputFile = new StreamWriter(dir + @"\web\data.js"))
            {
                outputFile.Write(dataJsString);
            }
            Process.Start(dir + @"\web\trade.html");
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }

        private void gridViewPos_DoubleClick(object sender, EventArgs e)
        {
            GridView view = (GridView)sender;
            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            GridHitInfo info = view.CalcHitInfo(pt);
            if ((info.InRow || info.InRowCell) && (!gridViewPos.IsGroupRow(info.RowHandle)))
            {
                string colCaption = info.Column == null ? "N/A" : info.Column.GetCaption();
                int posId = (int)gridViewPos.GetRowCellValue(info.RowHandle, "ID");
                var dealerID = db.Pos.Where(x => x.ID == posId).Select(x => x.Dealers.ID).FirstOrDefault();
                var frmEditPos = new FrmEditPos(posId, false, dealerID);
                

                frmEditPos.ShowDialog(this);
                

            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            treeList1_FocusedNodeChanged(null, null);
        }

        private void treeDealersList_LayoutUpdated(object sender, EventArgs e)
        {
        
            
        }

        private void treeDealersList_EndSorting(object sender, EventArgs e)
        {
            treeDealersList.SetFocusedNode(treeDealersList.Nodes.FirstNode);
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            treeDealersList.MakeNodeVisible(treeDealersList.FocusedNode);
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            var row = treeDealersList.GetDataRecordByNode(treeDealersList.FocusedNode);
            if (row == null)
            {
                TreeListFocusedId = -1;
            }
            else
            {
                dynamic treeDealer = new { ID = 0, Dealer_ID = 0, dealerZovName = "" };
                treeDealer = row;
                var dId = treeDealer.ID;
                var dName = treeDealer.dealerZovName;
                var frmEdit = new FrmEditDealer(dId, false, dName);

                frmEdit.Show(this);
            }

        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var frmEdit = new FrmEditDealer(0, true);
            
            frmEdit.Show(this);
        }

        private void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var frmSendReport = new Forms.FrmSendAppReport();
            
            frmSendReport.ShowDialog(this);
        }

        private void barButtonItem14_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var frmShowReport = new Forms.FrmAppReport();
            
            frmShowReport.ShowDialog(this);
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            var row = treeDealersList.GetDataRecordByNode(treeDealersList.FocusedNode);
            if (row == null)
            {
                return;
            }
            else
            {
                dynamic treeDealer = new { ID = 0, Dealer_ID = 0, dealerZovName = "" };
                treeDealer = row;
                var dId = treeDealer.ID;
                var dName = treeDealer.dealerZovName;
                var frmEdit = new FrmEditPos(0,true, dId);
                frmEdit.Show(this);
            }
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var reviews = new Forms.FrmViewReviews();
            reviews.Show(this);

        }

        private void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
 var frmViewPosList = new Forms.FrmViewPosList();
            frmViewPosList.Show(this);
        }

        private void gridViewPos_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                GridView view = sender as GridView;

                // Some condition
                var colorInt = view.GetRowCellValue(e.RowHandle, view.Columns["posColor"])!=null ? (int)view.GetRowCellValue(e.RowHandle, view.Columns["posColor"]) : 0;
                if (colorInt != 0)
                {
                    e.Appearance.BackColor = Color.FromArgb(colorInt);
                }
            }
        }

        private void btnShowPos_Click(object sender, EventArgs e)
        {
            var dealerIdList = new List<int>();
            dealerIdList.Add(dealerId);
            while (true){
                var subDealers = db.Dealers.Where(x => dealerIdList.Contains((int)x.Dealer_ID) & !dealerIdList.Contains(x.ID)).Select(x => x.ID).ToList();
                if (!subDealers.Any()) break;
                dealerIdList.AddRange(subDealers);
            }
            loadPosListFromDict(dealerIdList);
        }
        
        private void btnShowPosOfDealer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var dealerIdList = new List<int>();
            dealerIdList.Add(dealerId);
            loadPosListFromDict(dealerIdList);
        }
        
        private void btnShowPosOfSubDealers_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var dealerIdList = new List<int>();
            dealerIdList.Add(dealerId);
            while (true)
            {
                var subDealers = db.Dealers.Where(x => dealerIdList.Contains((int)x.Dealer_ID) & !dealerIdList.Contains(x.ID)).Select(x => x.ID).ToList();
                if (!subDealers.Any()) break;
                dealerIdList.AddRange(subDealers);
            }
            if (dealerIdList.Count()>=1){
                dealerIdList.Remove(dealerId);
            }
            loadPosListFromDict(dealerIdList);
        }

        private void loadPosListFromDict(List<int> dealerIdList)
        {
            var poss =
              db.Pos.Where(x => dealerIdList.Contains((int)x.dealer_ID)).Include(p => p.PosTypes).Select(x =>
                  new
                  {
                      x.ID,
                      x.dateadd,
                      x.posArea,
                      x.legalName,
                      posRating = x.PosRanks.Any(r => r.ActiveRank == true) ? x.PosRanks.Where(y => y.ActiveRank == true).Average(y => y.Rank) : 0,
                       //x.posRating,
                       x.locationDescription,
                      x.yandexAdress,
                      x.brand,
                      x.PosTypes.posTypeName,
                      x.Ruby_Id,
                      x.DealerLegalNames.LegalName,
                      posStatus = x.StatusOfPos.StatusName,
                      posColor = x.StatusOfPos.StatusColorInt,
                      colorRow = x.StatusOfPos.StatusColor,
                      posStatusDate = x.posStatusDate == null ? x.dateadd : x.posStatusDate,
                      listPosSites = x.Sites.Select(s => s.URL).ToList(),
                      listContacts = x.Contacts.Select(c => c.ContactName + " " + c.ContactPhones+" "+c.ContactOtherData).ToList()
                  }).ToList().Select(p => new
                  {
                      p.ID,
                      p.dateadd,
                      p.posStatusDate,
                      p.posArea,
                      p.legalName,
                      p.posRating,
                      p.locationDescription,
                      p.yandexAdress,
                      p.brand,
                      p.posTypeName,
                      p.Ruby_Id,
                      p.LegalName,
                      p.posStatus,
                      p.posColor,
                      p.colorRow,
                      PosSites = p.listPosSites.Any() ? p.listPosSites.Aggregate((cur, next) => cur + "\n" + next) : "",
                      PosContacts = p.listContacts.Any() ? p.listContacts.Aggregate((cur, next) => cur + "\n" + next) : ""
                  }
                  );
            gridControl1.DataSource = poss;
            gridViewPos.ExpandAllGroups();
        }

        private void barButtonItem5_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ExportDataToExcel();
        }

        private void ExportDataToExcel()
        {
            splashScreenManager1.ShowWaitForm();

            Excel.Application oExcelApp = null;

            try
            {
                oExcelApp = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            }
            catch (COMException)
            {
                //
            }
            if (oExcelApp == null || !oExcelApp.Visible)
            {
                oExcelApp = new Microsoft.Office.Interop.Excel.Application();
            }


            try
            {

                
                //List<int> goodposstatus = ExportAllDealers ? new List<int> {1, 2, 3, 4, 5} : new List<int> { 3, 4, 5 };
                List<int> goodposstatus = new List<int> { 3, 4, 5 };


                var posList = db.Pos.OrderBy(x => x.Ruby_Id)
                              .Where(x => (ExportAllDealers | goodposstatus.Contains(x.StatusOfPos.ID)) && (!x.Dealers.isDeleted | ShowDeletedDealers))
                              .Select(x => new
                {
                    id = x.Ruby_Id,
                    legalname = x.DealerLegalNames == null ? "" : x.DealerLegalNames.LegalName,
                    dealer = x.Dealers.dealerZovName,
                    dealerFabric = x.Dealers.DealerParent == null ? x.Dealers.dealerZovName : x.Dealers.DealerParent.dealerZovName,
                    yandexaddres = x.yandexAdress,
                    city = x.city,
                    streethouse = x.street,
                    coords = x.coordstextdata,
                    addres = x.locationDescription,
                    x.legalName,
                    area = x.posArea,
                    brand = x.brand,
                    postype = x.PosTypes == null ? "" : x.PosTypes.posTypeName,
                    x.SiteImagePath,
                    listPosSites = x.Sites.Select(s => s.URL).ToList(),
                    rating = x.PosRanks.Any(r => r.ActiveRank == true) ? x.PosRanks.Where(y => y.ActiveRank == true).Average(y => y.Rank) : 0,
                    enable = x.StatusOfPos.ID,
                    sortorder = x.posSortOrder == null ? 0 : x.posSortOrder,
                    x.isOptima,
                    x.Dealers.isDeleted,
                    dealerID = x.Dealers.ID
                }).ToList().Select(p => new
                {
                    p.id,
                    p.legalname,
                    p.dealer,
                    p.dealerFabric,
                    p.yandexaddres,
                    p.city,
                    p.streethouse,
                    p.coords,
                    p.addres,
                    p.legalName,
                    p.area,
                    p.brand,
                    p.postype,
                    p.SiteImagePath,
                    site = p.listPosSites.Any() ? p.listPosSites.Aggregate((cur, next) => cur + ", " + next) : "",
                    p.rating,
                    status = p.enable,
                    p.sortorder,
                    p.isOptima,
                    p.isDeleted,
                    p.dealerID
                });
                Debug.Write(posList.Count());



                oExcelApp.Visible = false;
                oExcelApp.ScreenUpdating = false;
                oExcelApp.DisplayAlerts = false;
                Excel.Workbook excelworkbook = oExcelApp.Workbooks.Add();
                Excel.Worksheet excelsheet = (Excel.Worksheet)excelworkbook.Worksheets.Add();

                int global_row = 1;
                ((Excel.Range)excelsheet.Cells[global_row, 1]).Value2 = "ID";
                ((Excel.Range)excelsheet.Cells[global_row, 2]).Value2 = "Юридическое наименование клиента";
                ((Excel.Range)excelsheet.Cells[global_row, 3]).Value2 = "Дилер";
                ((Excel.Range)excelsheet.Cells[global_row, 4]).Value2 = "Название на фабрике";
                ((Excel.Range)excelsheet.Cells[global_row, 5]).Value2 = "Адрес для Яндекса";
                ((Excel.Range)excelsheet.Cells[global_row, 6]).Value2 = "Город";
                ((Excel.Range)excelsheet.Cells[global_row, 7]).Value2 = "Улица и номер дома";
                ((Excel.Range)excelsheet.Cells[global_row, 8]).Value2 = "Координаты точки";
                ((Excel.Range)excelsheet.Cells[global_row, 9]).Value2 = "Адрес";
                ((Excel.Range)excelsheet.Cells[global_row, 10]).Value2 = "Название магазина";
                ((Excel.Range)excelsheet.Cells[global_row, 11]).Value2 = "Телефоны";
                ((Excel.Range)excelsheet.Cells[global_row, 12]).Value2 = "Площадь торговой точки";
                ((Excel.Range)excelsheet.Cells[global_row, 13]).Value2 = "Бренд";
                ((Excel.Range)excelsheet.Cells[global_row, 14]).Value2 = "Тип салона";
                ((Excel.Range)excelsheet.Cells[global_row, 15]).Value2 = "Путь к изображениям";
                ((Excel.Range)excelsheet.Cells[global_row, 16]).Value2 = "Сайт";
                ((Excel.Range)excelsheet.Cells[global_row, 17]).Value2 = "Почта";
                ((Excel.Range)excelsheet.Cells[global_row, 18]).Value2 = "Рейтинг";
                ((Excel.Range)excelsheet.Cells[global_row, 19]).Value2 = "Вкл/Выкл";
                ((Excel.Range)excelsheet.Cells[global_row, 20]).Value2 = "Порядок сортировки";
                ((Excel.Range)excelsheet.Cells[global_row, 21]).Value2 = "Оптима";
                if (ShowDeletedDealers)
                {
                    ((Excel.Range)excelsheet.Cells[global_row, 22]).Value2 = "Удален";
                    ((Excel.Range)excelsheet.Cells[global_row, 23]).Value2 = "DealerID";
                }

                foreach (var pos in posList)
                {
                    splashScreenManager1.SetWaitFormDescription(String.Format("{0} из {1} - {2:F}%", global_row, posList.Count(), (float)global_row / posList.Count() * 100));
                    global_row += 1;

                    ((Excel.Range)excelsheet.Cells[global_row, 1]).Value2 = pos.id;
                    ((Excel.Range)excelsheet.Cells[global_row, 2]).Value2 = pos.legalname;
                    ((Excel.Range)excelsheet.Cells[global_row, 3]).Value2 = pos.dealerFabric;
                    ((Excel.Range)excelsheet.Cells[global_row, 4]).Value2 = pos.dealer;
                    ((Excel.Range)excelsheet.Cells[global_row, 5]).Value2 = pos.yandexaddres;
                    ((Excel.Range)excelsheet.Cells[global_row, 6]).Value2 = pos.city;
                    ((Excel.Range)excelsheet.Cells[global_row, 7]).Value2 = pos.streethouse;
                    ((Excel.Range)excelsheet.Cells[global_row, 8]).Value2 = pos.coords;
                    ((Excel.Range)excelsheet.Cells[global_row, 9]).Value2 = pos.addres;
                    ((Excel.Range)excelsheet.Cells[global_row, 10]).Value2 = pos.legalName;

                    var posPhones = db.Pos.Where(x => x.Ruby_Id == pos.id).First().Contacts.Where(c => c.ContactPhones != null).Select(c => c.ContactPhones).ToList();

                    ((Excel.Range)excelsheet.Cells[global_row, 11]).Value2 = posPhones.Count == 0 ? "" : posPhones.Aggregate((cur, next) => cur + ", " + next);
                    ((Excel.Range)excelsheet.Cells[global_row, 12]).Value2 = pos.area;
                    ((Excel.Range)excelsheet.Cells[global_row, 13]).Value2 = pos.brand;
                    ((Excel.Range)excelsheet.Cells[global_row, 14]).Value2 = pos.postype;
                    ((Excel.Range)excelsheet.Cells[global_row, 15]).Value2 = pos.SiteImagePath;
                    ((Excel.Range)excelsheet.Cells[global_row, 16]).Value2 = pos.site;

                    var posmails = db.Pos.Where(x => x.Ruby_Id == pos.id).First().Contacts.Where(c => c.ContactName == "e-mail").Select(c => c.ContactOtherData).ToList();
                    ((Excel.Range)excelsheet.Cells[global_row, 17]).Value2 = posmails.Count == 0 ? "" : posmails.Aggregate((cur, next) => cur + ", " + next);
                    ((Excel.Range)excelsheet.Cells[global_row, 18]).Value2 = pos.rating;
                    switch ((Zov.Enums.PosStatus)pos.status)
                    {
                        case Zov.Enums.PosStatus.Ok:
                            ((Excel.Range)excelsheet.Cells[global_row, 19]).Value2 = 1;
                            break;
                        case Zov.Enums.PosStatus.Closed:
                            ((Excel.Range)excelsheet.Cells[global_row, 19]).Value2 = 0;
                            break;
                        case Zov.Enums.PosStatus.Ok_DontShow:
                            ((Excel.Range)excelsheet.Cells[global_row, 19]).Value2 = 0;
                            break;
                        default:
                            ((Excel.Range)excelsheet.Cells[global_row, 19]).Value2 = -1;
                            break;

                    }
                    ((Excel.Range)excelsheet.Cells[global_row, 20]).Value2 = pos.sortorder;
                    ((Excel.Range)excelsheet.Cells[global_row, 21]).Value2 = pos.isOptima.HasValue ? ((bool)pos.isOptima ? 1 : 0) : 0;
                    if (ShowDeletedDealers)
                    {
                        ((Excel.Range)excelsheet.Cells[global_row, 22]).Value2 = pos.isDeleted ? 1 : 0;
                        ((Excel.Range) excelsheet.Cells[global_row, 23]).Value2 = pos.dealerID;
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            finally
            {
                oExcelApp.Visible = true;
                oExcelApp.ScreenUpdating = true;
                oExcelApp.DisplayAlerts = true;
            }
            splashScreenManager1.CloseWaitForm();

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Environment.Exit(0);
        }

        private void FrmDealers_FormClosing(object sender, FormClosingEventArgs e)
        {

            //this.Hide();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = false;
            this.Visible = false;
           
            notifyIcon1.ShowBalloonTip(100);
            #if !DEBUG 
            e.Cancel = true;
            #endif
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            
           
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.Visible)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = false;
                    this.Visible = false;
                    notifyIcon1.ShowBalloonTip(100);
                }
                else
                {

                    this.Visible = true;
                    this.ShowInTaskbar = true;

                }
            }
        }

        private void barButtonDeleteDealer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DeleteSelectedDealer();
        }

        private void DeleteSelectedDealer()
        {
            if (TreeListFocusedId == -1)
            {
                MessageBox.Show("Выделите дилера", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (MessageBox.Show("Удалить дилера?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    GetDealerInfo(TreeListFocusedId);
                    var currentDealer = GetCurrentSelectedDealer(true);
                    if (currentDealer == null)
                        return;
                    currentDealer.isDeleted = true;
                    db.SaveChanges();
                    GetDealersTree();
                };
            }
        }


        private void barButtonShowDeleted_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowDeletedDealers = barButtonShowDeleted.Checked;
        }

        private void contextMenuStripDelete_Opening(object sender, CancelEventArgs e)
        {
            var currentDealer = GetCurrentSelectedDealer();
            if (currentDealer == null)
            {
                toolStripMenuItemDeleteDealer.Visible = false;
                toolStripMenuItemRestoreDealer.Visible = false;
                return;
            }
            toolStripMenuItemDeleteDealer.Visible = !currentDealer.isDeleted;
            toolStripMenuItemRestoreDealer.Visible = currentDealer.isDeleted;
        }

        private void toolStripMenuItemRestoreDealer_Click(object sender, EventArgs e)
        {
            var currentDealer = GetCurrentSelectedDealer(true);
            if (currentDealer == null)
                return;
            if (currentDealer.isDeleted)
            {
                currentDealer.isDeleted = false;
                db.SaveChanges();
                GetDealersTree();
            }
            else
            {
                MessageBox.Show(String.Format("Дилер активен ID={0}, dealerZovName={1}", TreeListFocusedId, currentDealer.dealerZovName), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Dealers GetCurrentSelectedDealer(bool showMessage = false)
        {
            if (!db.Dealers.Select(x => x.ID == TreeListFocusedId).Any())
            {
                if (showMessage)
                    MessageBox.Show(String.Format("Не могу найти дилера с таким ID={0}", TreeListFocusedId), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            var firstOrDefault = db.Dealers.Where(x => x.ID == TreeListFocusedId).FirstOrDefault();
            if ((showMessage) && (firstOrDefault == null))
                MessageBox.Show(String.Format("Не могу найти дилера с таким ID={0}", TreeListFocusedId), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return firstOrDefault;
        }

        private void toolStripMenuItemDeleteDealer_Click(object sender, EventArgs e)
        {
            DeleteSelectedDealer();
        }

        private void barSubItem1_Popup(object sender, EventArgs e)
        {
             var currentDealer = GetCurrentSelectedDealer();
            if (currentDealer == null)
            {
                barButtonDeleteDealer.Visibility = BarItemVisibility.Never;
                barButtonRestoreDealer.Visibility = BarItemVisibility.Never;
                return;
            }
            toolStripMenuItemDeleteDealer.Visible = !currentDealer.isDeleted;
            toolStripMenuItemRestoreDealer.Visible = currentDealer.isDeleted;
           
        }

        private void barCheckItemExportAll_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            ExportAllDealers = barCheckItemExportAll.Checked;
        }

    }
}