using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.Properties;
using static System.Net.Mime.MediaTypeNames;

namespace Test
{
    public partial class PanelPartner : UserControl
    {
        public string TypePartner { get; set; }
        public string Namepartner { get; set; }
        public string Director { get; set; }
        public string Phone { get; set; }
        public string Rating { get; set; }
        public string Discount { get; set; }
        public string UrlImage { get; set; }
        public string Inn { get; set; } = "";
        public PanelPartner()
        {
            InitializeComponent();
            // Сохраняем изображение, установленное в дизайнере (логотип по умолчанию)
            labelType.Text = TypePartner;
            labelNamepartner.Text = Namepartner;
            labelDirector.Text = Director;
            labelPhone.Text = Phone;
            labelDiscount.Text = Discount;
            labelRating.Text = Rating;
        }
        public void setLabels()
        {
            labelType.Text = TypePartner;
            labelNamepartner.Text = Namepartner;
            labelDirector.Text = Director;
            labelPhone.Text = Phone;
            labelDiscount.Text = Discount;
            labelRating.Text = Rating;
            // Показываем изображение из папки Images, если задано и существует; иначе показываем дефолтный логотип
            if (!string.IsNullOrEmpty(UrlImage))
            {
                var path = Path.Combine(Environment.CurrentDirectory, "Images", UrlImage);
                if (File.Exists(path))
                {
                    pictureBox1.ImageLocation = path;
                }
                else
                {
                    pictureBox1.ImageLocation = null;
                }
            }
            else
            {
                pictureBox1.ImageLocation = null;
            }
        }
        public int checkDiscount(int totalSales)
        {
            if (totalSales >= 300000) return 15;
            if (totalSales >= 50000) return 10;
            if (totalSales >= 10000) return 5;
            return 0;
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(Resources.connectBD))
            {
                connection.Open();
                string query = $@"DELETE FROM public.partner_test
                               WHERE name_partner_pk = '{labelNamepartner.Text}'; ";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                    {
                        connection.Close();                      
                    }
                    this.Parent.Controls.Remove(this);
                    MessageBox.Show("Партнер удален из базы данных");                   
                }
            }
        }
        private void buttonShowHistory_Click(object sender, EventArgs e)
        {
            HistoryForm historyForm = new HistoryForm(labelNamepartner.Text);
            historyForm.ShowDialog();
        }
                 
        private void buttonRedactPartner_Click(object sender, EventArgs e)
        {
            AddPartner addpartnerForm = new AddPartner((FlowLayoutPanel)this.Parent, true);
            addpartnerForm.NamePartnerToEdit = labelNamepartner.Text;
            addpartnerForm.panelPartner = this;
            addpartnerForm.ShowDialog();
        }

        private void PanelPartner_Load(object sender, EventArgs e)
        {

        }
    }
}

