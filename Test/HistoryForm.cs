using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test;
using Test.Properties;

namespace Test
{   
    public partial class HistoryForm : Form
    {
        public string Namepartner { get; set; }
        public HistoryForm(string namepartner)
        {
            InitializeComponent();
            Namepartner = namepartner;
            HistoryForm_Load();
        }
        private void HistoryForm_Load()
        {
            flowLayoutPanel2.Controls.Clear();
            using (NpgsqlConnection connection = new NpgsqlConnection(Resources.connectBD))
            {
                connection.Open();
                MessageBox.Show(Namepartner);
                string query = $@"SELECT p.name_product, pp.count, pp.date  
                               FROM partner_product_test pp
                               JOIN products_test p ON pp.product_id = p.name_product
                               WHERE pp.partner_id = '{Namepartner}'
                                ORDER BY pp.date DESC ";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            History historyItem = new History();
                            historyItem.NameProduct = "Товар: " + reader.GetString(0);
                            historyItem.Quantity = "Количество: " + reader.GetInt32(1).ToString();
                            historyItem.Date = "Дата продажи: " + reader.GetDateTime(2).ToString("dd.MM.yyyy");
                            historyItem.setLabels();
                            flowLayoutPanel2.Controls.Add(historyItem);
                        }
                    }
                }
            }
        }

        private void buttonHistroryBack_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void HistoryForm_Resize(object sender, EventArgs e)
        {
            foreach (Control control in flowLayoutPanel2.Controls)
            {
                control.Width = flowLayoutPanel2.ClientSize.Width - 10;
            }
        }
    }
}
