using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.Properties;

namespace Test
{
    public partial class History : UserControl
    {
        public string NameProduct { get; set; }
        public string Quantity { get; set; }
        public string Date { get; set; }
        public History()
        {
            InitializeComponent();
            labelNameProduct.Text = NameProduct;
            labelQuantity.Text = Quantity;
            labelDate.Text = Date;
        }
        public void setLabels()
        {             
            labelNameProduct.Text = NameProduct;
            labelQuantity.Text = Quantity;
            labelDate.Text = Date;
        }
        private void History_Load(object sender, EventArgs e)
        {

        }
    }

}

//    {
//        using (NpgsqlConnection connection = new NpgsqlConnection(Resources.connectBD))
//        {
//            connection.Open();
//            string query = $@"DELETE FROM public.partner_test
//                               WHERE name_partner_pk = '{labelNamepartner.Text}'; ";
//            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
//            {
//                command.ExecuteNonQuery();
//                {
//                    connection.Close();
//                }
//                this.Parent.Controls.Remove(this);
//                MessageBox.Show("Партнер удален из базы данных");
//            }
//        }
//    }

//    private void PanelPartner_Load(object sender, EventArgs e)
//    {

//    }
//}