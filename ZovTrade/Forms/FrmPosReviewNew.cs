using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DbModel;
using System.Data.Entity;
using ZovTrade.Model;
using DevExpress.XtraGrid.Views.Card;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Card.ViewInfo;

namespace ZovTrade.Forms
{
    public partial class FrmPosReviewNew : DevExpress.XtraEditors.XtraForm
    {
        private int _posId;
        private int _posRankID;
        private tradeEntities db =
            new tradeEntities(DbModel.Tools.TradeConnectionString(Properties.Settings.Default.barcodeCS.ToString()));
        public FrmPosReviewNew(int posid=0,int posrankId=0)
        {
            _posId = posid;
            _posRankID = posrankId;
            InitializeComponent();
        }

        private void FrmPosReviewNew_Load(object sender, EventArgs e)
        {

            if (_posRankID > 0)
            {
                db.PosRanks.Where(x => x.ID == _posRankID).Load();
                _posId = (int)db.PosRanks.First(x => x.ID == _posRankID).Pos_ID;
                this.Text = "Изменить отзыв.";
            }
              if (_posId > 0)
            {
               
                lookUpEdit1.Properties.DataSource = db.Pos.Select(x => new
                {
                    x.ID,
                    x.Dealers.dealerZovName,
                    x.legalName,
                    x.yandexAdress
                }
           ).Where(x => x.ID == _posId).ToList();
            }
            else
            {
                lookUpEdit1.Properties.DataSource = db.Pos.Select(x => new
                {
                    x.ID,
                    x.Dealers.dealerZovName,
                    x.legalName,
                    x.yandexAdress
                }
                ).OrderBy(x => x.dealerZovName).ThenBy(x => x.legalName).ToList();
            }
            lookUpEdit1.Properties.DisplayMember = "legalName";
            lookUpEdit1.Properties.ValueMember = "ID";
            lookUpEdit1.Properties.BestFitMode=DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            lookUpEdit1.Properties.ReadOnly = !(_posId == 0);
            
            if (_posRankID == 0)
            {
                var Rank = db.PosRanks.Create();
                Rank.ActiveRank = true;
                if (_posId != 0) Rank.Pos_ID = _posId;
                db.PosRanks.Add(Rank);
            }
            LoadAttachments();
            posRanksBindingSource.DataSource = db.PosRanks.Local.ToBindingList();
            posRanksBindingSource.MoveFirst();
        }
        private bool saveData()
        {
            if (db.PosRanks.Local.First().Pos_ID == null)
            {
                MessageBox.Show("Не все поля определены!!!!");
                return false;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Tools.showDbSaveExceptions(ex);
                return false;
                
            }
            return true;
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {

            if (saveData())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            var posrank = db.PosRanks.Local.First();
            if (posrank.ID==0)
            {
                if (!saveData()) return;
            }
            var file = new FileModel();
            file = Tools.getFileFromPC();
            if (file == null || file.Data.Length==0) return;

            var attachment = db.Attachments.Create();
            attachment.FileName = file.Name;
            attachment.FileExt = file.Extension;
            attachment.FileData = file.Data;
            attachment.PosRanks.Add(posrank);
            posrank.Attachments.Add(attachment);
            if (saveData())
            {
                LoadAttachments();
            }

        }
        


        private void LoadAttachments()
        {
            var posrank = db.PosRanks.Local.First();
            gridControlAttachments.DataSource = "";
            if (posrank == null) return;
            if (posrank.ID > 0)
            {
                var attachments = db.PosRanks.Where(x => x.ID == posrank.ID).Include(x => x.Attachments).First().Attachments.Select(x => new
                {
                    x.ID,
                    x.FileName,
                    x.FileExt
                }).ToList();
                gridControlAttachments.DataSource = attachments;
            }
        }

        private void cardView1_DoubleClick(object sender, EventArgs e)
        {
            CardView view = (CardView)sender;
            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            //    DoRowDoubleClick(view, pt);
            CardHitInfo info = view.CalcHitInfo(pt);
            if (info.InCard) {
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

        private void cardView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;

            CardView view = cardView1 as CardView;
            if (!(view.RowCount>0 && view.FocusedRowHandle >= -1)) return;
            try
            {

            
            int appid = (int)cardView1.GetRowCellValue(view.FocusedRowHandle, "ID");
            var posrankId = db.PosRanks.Local.First().ID;

            db.Attachments.RemoveRange(db.PosRanks.First(x => x.ID == posrankId).Attachments.Where(x => x.ID == appid));
            }
            catch (Exception )
            {
            }

            try
            {
                db.SaveChanges();
                LoadAttachments();
            }
            catch (Exception ex)
            {
                Tools.showDbSaveExceptions(ex);
            }
            
           
        }
    }
}