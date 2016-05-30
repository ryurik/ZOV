using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DbModel;
using DevExpress.Data.WcfLinq.Helpers;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.Diagnostics;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using ZovTrade.Model;
using DevExpress.XtraGrid.Views.Card;
using DevExpress.XtraGrid.Views.Card.ViewInfo;
using ZovTrade.Forms;

namespace ZovTrade
{
    public partial class FrmEditPos : DevExpress.XtraEditors.XtraForm
    {
        private DbModel.tradeEntities db = new tradeEntities(DbModel.Tools.TradeConnectionString(Properties.Settings.Default.barcodeCS.ToString()));
        //   private SplashScreenManager splashScreenManager = new SplashScreenManager();
        private int posId = 0;
        bool isNewPos = false;
        int dealerId = 0;

        public FrmEditPos(int _posId, bool _isNewPos = false, int _dealerId = 0)
        {

            posId = _posId;
            isNewPos = _isNewPos;
            dealerId = _dealerId;

            InitializeComponent();

            layoutControlItem17.Visibility = isNewPos ? DevExpress.XtraLayout.Utils.LayoutVisibility.Never : DevExpress.XtraLayout.Utils.LayoutVisibility.Always;

            if (dealerId == 0)
            {
                var frmSelectDealer = new Forms.FrmDealerChoose();
                if (frmSelectDealer.ShowDialog(this) == DialogResult.OK)
                {
                    dealerId = frmSelectDealer.DealerId;
                }

            }


        }

        private void FrmEditPos_Load(object sender, EventArgs e)
        {
            if (dealerId == 0)
            {
                this.Close();
            }
            //splashScreenManager.
            //    splashScreenManager.ShowWaitForm();
            splashScreenManager1.ShowWaitForm();



            db.DocTypes.Load();
            docTypesBindingSource.DataSource = db.DocTypes.Local.Select(x => new { x.ID, x.DocType }).ToList();

            if (!isNewPos)
            {
                var curpos = from p in db.Pos
                             where p.ID == posId
                             select new { p.legalName, p.Dealers.dealerZovName };

                var firstOrDefault = curpos.FirstOrDefault();
                if (firstOrDefault != null)
                    this.Text = firstOrDefault.dealerZovName + " \\ " + firstOrDefault.legalName;

                var posRating =
                    db.Pos.Where(x => x.ID == posId)
                        .Select(
                            x =>
                                new
                                {
                                    posRating =
                                        x.PosRanks.Any(r => r.ActiveRank == true)
                                            ? x.PosRanks.Where(r => r.ActiveRank == true).Average(r => r.Rank) : 0
                                });
                posRatingTextEdit.EditValue = posRating.First();
                db.PosTypes.Load();
                db.Dealers.Where(x => x.Pos.FirstOrDefault(p => p.ID == posId).Dealers.ID == x.ID).Load();
                db.Pos.Where(x => x.ID == posId).Load();



                //db.Certifications.Where(x => x.Pos.ID == posId).Load();
                db.Samples.Where(x => x.Pos.ID == posId).Load();
                dealerId = db.Dealers.Local.First().ID;

                db.Contacts.Where(x => x.Pos.Where(p => p.ID == posId).Any()).Load();
                db.Docs.Where(x => x.Pos_ID == posId).Load();
                SetXtraTabsVisible();

                
                // db.Attachments.Where(x => x.Certifications.Where(c => c.Pos_ID == posId).Any()).Load();

            }
            else
            {

                this.Text = db.Dealers.Where(x => x.ID == dealerId).Select(x => x.dealerZovName).First() + " \\ Новый магазин";
                db.Dealers.Where(x => x.ID == dealerId).Load();



                posRatingTextEdit.EditValue = 0;
                db.PosTypes.Load();


                var newPos = db.Pos.Create();
                if (dealerId > 0)
                {
                    newPos.dealer_ID = dealerId;
                }
                db.Pos.Add(newPos);


            }


            //db.CertTypes.Load();
            //certTypesBindingSource.DataSource = db.CertTypes.Local.Select(x => new { x.ID, x.DocType }).ToList();
            int parentDealerId = 0;

            if (db.Dealers.Where(x => x.ID == dealerId && x.DealerParent != null).Select(x => x.DealerParent).Any())
            {
                parentDealerId = db.Dealers.Where(x => x.ID == dealerId && x.DealerParent != null).Select(x => x.DealerParent).First().ID;
            }

            var dealerLegalNames = db.DealerLegalNames.Where(x => x.Dealers.ID == dealerId || (parentDealerId != 0 && x.Dealers.ID == parentDealerId)).Select(x => new { x.ID, x.LegalAddress, x.LegalName }).ToList();

            dealerLegalNamesBindingSource.DataSource = dealerLegalNames;

            posTypesBindingSource.DataSource = db.PosTypes.Local.Select(x => new { x.ID, x.posTypeName }).ToList();

            gridLookUpEdit1.Properties.DataSource = db.Dealers.Select(x => new
            {
                x.ID,
                dealerName = x.Dealer_ID != null ? x.DealerParent.dealerZovName + "/" + x.dealerZovName : x.dealerZovName

            })
            .ToList();
            gridLookUpEdit1.Properties.ValueMember = "ID";
            gridLookUpEdit1.Properties.DisplayMember = "dealerName";

            // dealersBindingSource.DataSource = 

            posBindingSource.DataSource = db.Pos.Local.ToBindingList();


            //certificationsBindingSource.DataSource = db.Certifications.Local.ToBindingList();
            samplesBindingSource.DataSource = db.Samples.Local.ToBindingList();
            contactsBindingSource.DataSource = db.Contacts.Local.ToBindingList();
            sampleDetailStatusBindingSource.DataSource = db.SampleDetailStatus.Local.ToBindingList();

            db.SampleDetailStatus.Load();
            db.Sites.Where(x => x.Dealer_ID == dealerId | x.Dealer_ID == parentDealerId).Load();

            statusOfPosBindingSource.DataSource = db.StatusOfPos.Select(x => new { x.ID, x.StatusName, x.StatusColor }).ToList();
            db.StatusOfPos.Load();
            bsDoc.DataSource = db.Docs.Local.ToBindingList();
            LoadPosSites();
            // LoadPosDocs();
            LoadPosRanks();



            //bsPosSites.Filter = "Pos_ID=" + posId;




            splashScreenManager1.CloseWaitForm();
        }
        //private void LoadPosDocs()
        //{

        //    this.gridControlCerts.DataSource = db.Pos.First(x => x.ID == posId).Docs.Select(x => new {
        //        x.ID,
        //        x.dateAdd,
        //        x.DocType_ID,
        //        x.DocDescription,
        //        AttachmentsCount = x.Attachments.Any() ? x.Attachments.Count : 0,                
        //    }).ToList();
        //}
        private void LoadPosSites()
        {
            var posSites = db.Pos.Local.FirstOrDefault().Sites.ToList();


            sitesBindingSource.DataSource = db.Sites.Local.Except(posSites).Select(x => new { x.ID, x.URL, x.Dealers.dealerZovName }).ToList();
            bsPosSites.DataSource = db.Pos.Local.FirstOrDefault().Sites.ToList();
        }
        private void SetXtraTabsVisible()
        {
            if (db.Pos.Local.First().ID > 0)
            {
                xtraTabPageDocs.PageVisible = true;
                xtraTabPageReviews.PageVisible = true;
                xtraTabPageSamples.PageVisible = true;
            }
            else
            {
                xtraTabPageDocs.PageVisible = false;
                xtraTabPageReviews.PageVisible = false;
                xtraTabPageSamples.PageVisible = false;
            }
        }
        private bool savedata()
        {
            try
            {
                var pos = db.Pos.Local.First();
                string coords = (pos.longitude.ToString().Replace(",", ".") + ", " + pos.attitude.ToString().Replace(",", "."));
                if (pos.coordstextdata != coords)
                {
                    pos.coordstextdata = coords;
                }
                db.SaveChanges();
                SetXtraTabsVisible();
                return true;
            }
            catch (Exception ex)
            {
                Tools.showDbSaveExceptions(ex);
                return false;
            }
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (savedata())
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Точно удалить???") == DialogResult.OK)
            {


                var curPos = db.Pos.Where(x => x.ID == posId).Select(x => x).ToList();
                if (curPos.Any())
                {
                    db.Pos.Remove(curPos.First());

                    if (Tools.SaveDb(ref db))
                        this.Close();
                }
            }
        }

        private void Добавить_Click(object sender, EventArgs e)
        {
            //var row = gridLookUpEditDealerSites.Properties.View.GetDataRow(gridLookUpEditDealerSites.Properties.View.FocusedRowHandle);
            var curSiteId = gridLookUpEditDealerSites.EditValue;
            if (curSiteId != null)
            {
                var curSite = db.Sites.Where(x => x.ID == (int)curSiteId).FirstOrDefault();
                //var pos = db.Pos.Local.FirstOrDefault();
                var posSites = db.Pos.Local.FirstOrDefault().Sites;
                posSites.Add(curSite);
                LoadPosSites();
            }
        }
        private void remove_siteFromPos(int siteID)
        {
            try
            {
                var curSite = db.Sites.Where(x => x.ID == siteID).FirstOrDefault();
                //var pos = db.Pos.Local.FirstOrDefault();
                var posSites = db.Pos.Local.FirstOrDefault().Sites;
                posSites.Remove(curSite);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            LoadPosSites();
        }
        private void gridControl4_ProcessGridKey(object sender, KeyEventArgs e)
        {
            var grid = sender as GridControl;
            var view = grid.FocusedView as GridView;
            if (e.KeyData == Keys.Delete)
            {
                e.Handled = true;
                int SiteId = (int)view.GetRowCellValue(view.FocusedRowHandle, "ID");

                remove_siteFromPos(SiteId);
            }
        }

        private void BtnAddSample_Click(object sender, EventArgs e)
        {
            var Sample = db.Samples.Create();
            Sample.Pos = db.Pos.Local.FirstOrDefault();
            Sample.sampleStatus = 1;
            db.Samples.Add(Sample);

        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            DeleteFocusedRows(gridViewContacts);
        }
        private void DeleteFocusedRows(DevExpress.XtraGrid.Views.Grid.GridView view)
        {
            if (view.RowCount > 0 && view.IsValidRowHandle(view.FocusedRowHandle) && !view.IsNewItemRow(view.FocusedRowHandle))
            {
                view.BeginSort();
                try
                {
                    view.DeleteRow(view.FocusedRowHandle);

                }
                catch (Exception)
                {
                }
                view.EndSort();
            }
        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            var Contact = db.Contacts.Create();
            //Contact.Dealers.Add(db.Dealers.Local.First());
            db.Contacts.Add(Contact);
            db.Pos.Local.FirstOrDefault().Contacts.Add(Contact);
        }



        private void BtnAddReview_Click(object sender, EventArgs e)
        {
            var posId = db.Pos.Local.First().ID;
            if (posId == 0)
            {
                MessageBox.Show("Сначала нужно записать в базу магазин!!!");
                return;
            }
            var frmRankNew = new Forms.FrmPosReviewNew(posid: posId);
            if (frmRankNew.ShowDialog() == DialogResult.OK)
            {
                db.PosRanks.Where(x => x.Pos.ID == posId).Load();
            }

            //var Review = db.PosRanks.Create();
            ////Contact.Dealers.Add(db.Dealers.Local.First());
            //Review.Pos = db.Pos.Local.FirstOrDefault();
            //Review.Rank = 0;
            //db.PosRanks.Add(Review);
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            DeleteFocusedRows(gridViewCerts);
        }

        private void BtnAddSertif_Click(object sender, EventArgs e)
        {
            var pos = db.Pos.Local.First();

            var doc = db.Docs.Create();
            doc.DocType_ID = db.DocTypes.Local.First().ID;
            doc.Pos_ID = pos.ID;
            db.Docs.Add(doc);
            // var Cert = db.Certifications.Create();
            // Cert.Pos = db.Pos.Local.FirstOrDefault();
            // Cert.CertType_ID = db.CertTypes.Local.Last().ID;
            // db.Certifications.Add(Cert);
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            DeleteFocusedRows(gridViewSamples);
        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {

            var frmSelectDealer = new Forms.FrmDealerChoose();
            if (frmSelectDealer.ShowDialog(this) == DialogResult.OK)
            {
                try
                {

                    var dbNew = new tradeEntities(DbModel.Tools.TradeConnectionString(Properties.Settings.Default.barcodeCS.ToString()));
                    var pos2change = dbNew.Pos.Where(x => x.ID == posId).FirstOrDefault();
                    pos2change.dealer_ID = frmSelectDealer.DealerId;
                    dbNew.SaveChanges();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                    this.Close();
                }
            }


        }

        private void simpleButton11_Click(object sender, EventArgs e)
        {
            var Contact = db.Contacts.Create();
            Contact.ContactName = "e-mail";
            //Contact.Dealers.Add(db.Dealers.Local.First());
            db.Contacts.Add(Contact);
            db.Pos.Local.FirstOrDefault().Contacts.Add(Contact);
        }

        private void gridViewReviews_DoubleClick(object sender, EventArgs e)
        {
            GridView view = (GridView)sender;
            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            //    DoRowDoubleClick(view, pt);
            GridHitInfo info = view.CalcHitInfo(pt);
            if ((info.InRow || info.InRowCell) && (!gridViewReviews.IsGroupRow(info.RowHandle)))
            {
                //string colCaption = info.Column == null ? "N/A" : info.Column.GetCaption();
                //     MessageBox.Show(string.Format("DoubleClick on row: {0}, column: {1}.", info.RowHandle, colCaption));
                int rankid = (int)gridViewReviews.GetRowCellValue(info.RowHandle, "ID");
                var newReviewForm = new Forms.FrmPosReviewNew(posrankId: rankid);
                var dr = newReviewForm.ShowDialog(this);
                if (dr == DialogResult.OK) LoadPosRanks();


            }
        }
        private void simpleButton4_Click(object sender, EventArgs e)
        {
            int focusedRowHandle = gridViewReviews.FocusedRowHandle;
            if (focusedRowHandle > -1 && (!gridViewReviews.IsGroupRow(focusedRowHandle)))
            {
                //string colCaption = info.Column == null ? "N/A" : info.Column.GetCaption();
                //     MessageBox.Show(string.Format("DoubleClick on row: {0}, column: {1}.", info.RowHandle, colCaption));
                int rankid = (int)gridViewReviews.GetRowCellValue(focusedRowHandle, "ID");
                db.PosRanks.Remove(db.PosRanks.First(x => x.ID == rankid));
                db.SaveChanges();
                LoadPosRanks();
            }
        }

        private void LoadPosRanks()
        {
            if (posId > 0)
            {
                posRanksBindingSource.DataSource = db.PosRanks.Where(x => x.Pos.ID == posId).Select(x => new
                {
                    x.ID,
                    x.Rank,
                    x.ActiveRank,
                    x.DateAdd,
                    x.Description,
                }).ToList();
            }
            else
            {
                posRanksBindingSource.DataSource = "";
            }
            ;
        }

        private void simpleButton12_Click(object sender, EventArgs e)
        {
            var pos = db.Pos.Local.First();

            if (pos.ID == 0 || pos.Docs.Any(x => x.ID == 0))
            {
                if (!savedata()) return;
            }
            var docid = 0;
            if (bsDoc.Current != null)
            {
                var  drv = bsDoc.Current as DbModel.Docs;

                docid = drv.ID;
            }
            if (docid == 0) return;
            var file = new FileModel();
            file = Tools.getFileFromPC();
            if (file == null || file.Data.Length == 0) return;

            var attachment = db.Attachments.Create();
            if (file.Name.Length > 250)
            {
                file.Name = file.Name.Substring(0,250);
            }
            attachment.FileName = file.Name;
            attachment.FileExt = file.Extension;
            attachment.FileData = file.Data;
            db.Docs.Find(docid).Attachments.Add(attachment);
            //posrank.Attachments.Add(attachment);
            if (savedata())
            {
                LoadDocAttachments();
            }
        }
        private void LoadDocAttachments()
        {
            var docid = 0;
            gridControlPosDocs.DataSource = "";
            if (bsDoc.Current != null)
            {
                var doc = bsDoc.Current as DbModel.Docs;
                if (doc == null) return;

                docid = doc.ID;
            }
            if (docid == 0) return;
            gridControlPosDocs.DataSource = db.Docs.Where(x => x.ID == docid).Include(x => x.Attachments).First().Attachments.Select(x => new
            {
                x.ID,
                x.FileName,
                x.FileExt
            }).ToList();


            
        }

        private void simpleButton10_Click(object sender, EventArgs e)
        {

        }

        private void cardView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;

            CardView view = cardView1 as CardView;
            if (!(view.RowCount > 0 && view.FocusedRowHandle >= -1)) return;
            try
            {


                int appid = (int)cardView1.GetRowCellValue(view.FocusedRowHandle, "ID");
                db.Attachments.RemoveRange(db.Attachments.Where(x => x.ID == appid));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            try
            {
                db.SaveChanges();
                LoadDocAttachments();
            }
            catch (Exception ex)
            {
                Tools.showDbSaveExceptions(ex);
            }

        }

        private void cardView1_DoubleClick(object sender, EventArgs e)
        {
            CardView view = (CardView)sender;
            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            //    DoRowDoubleClick(view, pt);
            CardHitInfo info = view.CalcHitInfo(pt);
            if (info.InCard)
            {
                int appid = (int)cardView1.GetRowCellValue(info.RowHandle, "ID");
                var attachment = db.Attachments.First(x => x.ID == appid);

                Tools.openFilefromModel(new FileModel
                {
                    Name = attachment.FileName,
                    Extension = attachment.FileExt,
                    Data = attachment.FileData
                });

            }


        }

        private void bsDoc_CurrentChanged(object sender, EventArgs e)
        {
            LoadDocAttachments();
        }

        private void btnSampleAddFile_Click(object sender, EventArgs e)
        {
            // now we must to get current Sample ID
            Samples sample = gridViewSamples.GetFocusedRow() as Samples;
            if (sample == null)
            {
                sampleAttachmentsBindingSource.DataSource = null;
                return;
            }

            int sampleID = sample.ID;
            Trace.WriteLine(string.Format("ID:{0}", sampleID));

            //var sampleAttachmets = db.Samples.FirstOrDefault().SampleAttachments


            if (sampleID == 0) 
                return;


            SampleAttachments sampleAttachments = new SampleAttachments();
            sampleAttachments = Tools.getAttachmetFileFromPc();
            if (sampleAttachments == null || sampleAttachments.FileData.Length == 0) 
                return;

            sampleAttachments.SampleID = sampleID;

            db.SampleAttachments.Add(sampleAttachments);

            if (savedata())
            {
                LoadSamplesAttachments();
            }
        }

        private void LoadSamplesAttachments()
        {
            sampleAttachmentsBindingSource.DataSource = GetSamplesAttachmentsList(); 
        }

        private void gridViewSamples_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            sampleAttachmentsBindingSource.DataSource = GetSamplesAttachmentsList(); // db.SampleAttachments.Where(x => x.SampleID == sampleID).ToList();
        }

        private void viewSamplesAttachmets_DoubleClick(object sender, EventArgs e)
        {
            CardView view = (CardView)sender;
            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            //    DoRowDoubleClick(view, pt);
            CardHitInfo info = view.CalcHitInfo(pt);
            if (info.InCard)
            {
                int sampleAttachmentID = (int)viewSamplesAttachmets.GetRowCellValue(info.RowHandle, "SampleAttachmentID");
                SampleAttachments sampleAttachments = db.SampleAttachments.First(x => x.SampleAttachmentID == sampleAttachmentID);

                if (Tools.FileIsPicture(sampleAttachments.FileExt))
                {
                    //                FrmShowSapmplesAttachments frmShowPicture = new FrmShowSapmplesAttachments(sampleAttachments.FileData, sampleAttachments.FileName + sampleAttachments.FileExt);
                    FrmShowSapmplesAttachments frmShowPicture = new FrmShowSapmplesAttachments(GetSamplesAttachmentsList(), sampleAttachmentID);
                    frmShowPicture.ShowDialog();
                }
                else
                {
                    Tools.openFilefromModel(new FileModel
                    {
                        Name = sampleAttachments.FileName,
                        Extension = sampleAttachments.FileExt,
                        Data = sampleAttachments.FileData
                    }
                    );
                }

            }
        }

        private List<SampleAttachments> GetSamplesAttachmentsList()
        {
            Samples sample = gridViewSamples.GetFocusedRow() as Samples;
            if (sample == null)
            {
                sampleAttachmentsBindingSource.DataSource = null;
                return null;
            }

            int sampleID = sample.ID;
            Trace.WriteLine(string.Format("ID:{0}", sampleID));

            return db.SampleAttachments.Where(x => x.SampleID == sampleID).ToList();
        }

        private void viewSamplesAttachmets_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;

            CardView view = viewSamplesAttachmets as CardView;
            if (!(view.RowCount > 0 && view.FocusedRowHandle >= -1)) return;
            try
            {
                int sampleAttachmentID = (int)viewSamplesAttachmets.GetRowCellValue(view.FocusedRowHandle, "SampleAttachmentID");
                SampleAttachments sampleAttachments = db.SampleAttachments.First(x => x.SampleAttachmentID == sampleAttachmentID);
                if (
                    MessageBox.Show(
                        String.Format("Удалить документ \n{0}\nиз текущего образца?",
                            sampleAttachments.FileName + sampleAttachments.FileExt), "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
                db.SampleAttachments.RemoveRange(db.SampleAttachments.Where(x => x.SampleAttachmentID == sampleAttachmentID));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            try
            {
                db.SaveChanges();
                sampleAttachmentsBindingSource.DataSource = GetSamplesAttachmentsList(); // db.SampleAttachments.Where(x => x.SampleID == sampleID).ToList();
            }
            catch (Exception ex)
            {
                Tools.showDbSaveExceptions(ex);
            }

        }
    }
}