using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DbModel;
using DevExpress.XtraEditors;

namespace ZovTrade.Forms
{
    public partial class FrmShowSapmplesAttachments : DevExpress.XtraEditors.XtraForm
    {
        private List<SampleAttachments> listSampleAttachmentses;
        private int currentId;

        private byte[] binaryPicture;
        public byte[] BinaryPicture
        {
            get { return binaryPicture; }
            set
            {
                binaryPicture = value;
                if (value != null)
                {
                    MemoryStream stream = new MemoryStream(value);

                    Image img = Image.FromStream(stream);


                    byte[] data = DevExpress.XtraEditors.Controls.ByteImageConverter.ToByteArray(img, img.RawFormat);

                    pictureEdit.EditValue = data;
                }
                else
                    pictureEdit.EditValue = null;
            }
        }

        public List<SampleAttachments> ListSampleAttachmentses
        {
            get { return listSampleAttachmentses; }
            set
            {
                listSampleAttachmentses = value;
                currentId = value != null ? listSampleAttachmentses.First().SampleAttachmentID : 0;
            }
        }

        public int CurrentId
        {
            get {return currentId;}
            set
            {
                currentId = value;
                if (currentId != 0)
                {
                    SampleAttachments sampleAttachments = ListSampleAttachmentses.FirstOrDefault(x => x.SampleAttachmentID == currentId);
                    if (sampleAttachments != null)
                    {
                        BinaryPicture = Tools.FileIsPicture(sampleAttachments.FileExt) ? sampleAttachments.FileData: null;
                        
                        Text = sampleAttachments.FileName + sampleAttachments.FileExt;
                    }
                }
            }
        }

        public FrmShowSapmplesAttachments(byte[] pic, string filename)
        {
            InitializeComponent();
            BinaryPicture = pic;
            Text = filename;
        }

        public FrmShowSapmplesAttachments(List<SampleAttachments> sampleAttachmentses, int currentId)
        {
            InitializeComponent();
            listSampleAttachmentses = sampleAttachmentses;
            CurrentId = currentId;
        }


        public FrmShowSapmplesAttachments()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();    
        }

        public void LoadData(byte[] pic)
        {
            BinaryPicture = pic;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            BinaryPicture = BinaryPicture;
        }

        private int GetPrevId()
        {
            int currentSampleAttachmentsIndex = ListSampleAttachmentses.FindIndex(x => x.SampleAttachmentID == CurrentId);
            if (currentSampleAttachmentsIndex == 0)
                currentSampleAttachmentsIndex = ListSampleAttachmentses.Count;
            return ListSampleAttachmentses[currentSampleAttachmentsIndex - 1].SampleAttachmentID;
        }

        private int GetNextId()
        {
            int currentSampleAttachmentsIndex = ListSampleAttachmentses.FindIndex(x => x.SampleAttachmentID == CurrentId);
            if (currentSampleAttachmentsIndex + 1 == ListSampleAttachmentses.Count)
                currentSampleAttachmentsIndex = -1;
            return ListSampleAttachmentses[currentSampleAttachmentsIndex + 1].SampleAttachmentID;
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            CurrentId = GetPrevId();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            CurrentId = GetNextId();
        }

    }
}