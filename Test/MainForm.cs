using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Test
{ 
    public partial class MainForm : Form 
    {
        public MainForm()
        {
            InitializeComponent();
            LoadPartners();
        }
        private void LoadPartners()
        {
            flowLayoutPanel1.Controls.Clear();
            using (NpgsqlConnection connection = new NpgsqlConnection(Resources.connectBD))
            {
                connection.Open();
                string query = "SELECT p.type_partner_id, p.name_partner_pk, " +
                    "p.director, p.phone_number, p.rating, p.url_image, " +
                    "COALESCE(SUM(pp.count), 0) as total_sales " +
                    "FROM partner_test p " +
                    "LEFT JOIN partner_product_test pp ON p.name_partner_pk = pp.partner_id " +
                    "GROUP BY p.name_partner_pk; ";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PanelPartner panelPartner = new PanelPartner();
                            panelPartner.TypePartner =  reader.GetString(0);
                            panelPartner.Namepartner =  reader.GetString(1);
                            panelPartner.Director = "Директор: " + reader.GetString(2);
                            panelPartner.Phone = "Телефон: +7 " + reader.GetString(3);
                            panelPartner.Rating = "Рейтинг: " + reader.GetInt32(4).ToString();
                            panelPartner.UrlImage = reader.IsDBNull(5) ? null : reader.GetString(5);
                            panelPartner.Discount = "Скидка: " + panelPartner.checkDiscount(reader.GetInt32(6)).ToString() + "%";
                            panelPartner.setLabels();
                            flowLayoutPanel1.Controls.Add(panelPartner);                   
                        }                       
                    }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            AddPartner addPartnerForm = new AddPartner(flowLayoutPanel1,false);
            addPartnerForm.ShowDialog();
        }
        //Изменение размера формы по контроллеру
        private void MainForm_Resize(object sender, EventArgs e)
        {
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                control.Width = flowLayoutPanel1.ClientSize.Width - 10;
            }
        }
    }
}
