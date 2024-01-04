using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Options;
using System.Xml.Linq;

namespace App10._2
{
    public partial class Form1 : Form
    {
        SqlConnection conn;
        SqlCommand cmd;
        SqlTransaction transaction; //
        public Form1()
        {
            InitializeComponent();
            conn = new SqlConnection(@"Data Source=MYPC\SQLEXPRESS01;Initial Catalog=Lab10;Trusted_Connection=True;Integrated Security=True;MultipleActiveResultSets=True;TrustServercertificate=True");
            cmd = new SqlCommand();
            cmd.Connection = conn;
            displaydata();
        }
        private void ExecuteNonQueryWithTransaction(string query)
        {
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
        }
        private void displaydata()
        {
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from invoice";
            cmd.ExecuteNonQuery();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            conn.Close();
        }
        private void cleardata()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string query = "insert into invoice (number,value) values(@Value1,@Value2)";
            cmd.CommandText = query;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@Value1", textBox1.Text);
            cmd.Parameters.AddWithValue("@Value2", "0");
            conn.Open();
            cmd.ExecuteNonQuery();
            cleardata();
            conn.Close();
            displaydata();
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            //string query = "delete invoice where invoice_id=@Value3";
            //cmd.CommandText = query;
            //cmd.Parameters.Clear();
            //cmd.Parameters.AddWithValue("@Value3", textBox3.Text.ToString());
            //conn.Open();
            //cmd.ExecuteNonQuery();
            //dataGridView1.DataSource = query;
            //cleardata();
            //conn.Close();
            //displaydata();
            int invoiceId = Convert.ToInt32(textBox3.Text);

            // Delete records from invoice_pos table based on invoice_id
            string posQuery = "DELETE FROM invoice_pos WHERE invoice_id = @Value3";
            cmd.CommandText = posQuery;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@Value3", invoiceId);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            // Delete record from invoice table based on invoice_id
            string invoiceQuery = "DELETE FROM invoice WHERE invoice_id = @Value3";
            cmd.CommandText = invoiceQuery;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@Value3", invoiceId);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            cleardata();
            displaydata();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            displaydata();
        }

        private void button_update_Click(object sender, EventArgs e)
        {
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "update invoice set number='" + textBox1.Text + "',value='" + textBox2.Text.ToString() + "' where invoice_id='" + textBox3.Text.ToString() + "' ";
            cmd.ExecuteNonQuery();
            conn.Close();
            displaydata();
            cleardata();
        }
        private bool isSortAscending = true;
        private void button_Sort_Click(object sender, EventArgs e)
        {
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            string sortOrder = isSortAscending ? "ASC" : "DESC";
            cmd.CommandText = $"select * from invoice order by value {sortOrder}";
            cmd.ExecuteNonQuery();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            dataGridView1.DataSource = dt.DefaultView;
            isSortAscending = !isSortAscending;
            conn.Close();
        }
        private void DisplaySumForInvoiceId(int invoiceId)
        {
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;

            // Calculate the sum of values for the specified invoice_id
            cmd.CommandText = $"SELECT SUM(value) AS TotalSum FROM invoice_pos WHERE invoice_id = {invoiceId}";
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("TotalSum")))
                {
                    decimal totalSum = reader.GetDecimal(reader.GetOrdinal("TotalSum"));

                    // Close the existing DataReader
                    reader.Close();

                    // Update the value column in the invoice table
                    cmd.CommandText = $"UPDATE invoice SET value = {totalSum} WHERE invoice_id = {invoiceId}";
                    cmd.ExecuteNonQuery();
                }
            }

            conn.Close();
        }
        private void button_pos_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox3.Text, out int invoiceId))
            {
                string query = "insert into invoice_pos (invoice_id,name,value) values(@Value1,@Value2,@Value3)";
                cmd.CommandText = query;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Value1", textBox3.Text);
                cmd.Parameters.AddWithValue("@Value2", textBox4.Text);
                cmd.Parameters.AddWithValue("@Value3", textBox2.Text.ToString());
                conn.Open();
                cmd.ExecuteNonQuery();
                cleardata();
                conn.Close();
                //int invoiceId = Convert.ToInt32(textBox3.Text);
                DisplaySumForInvoiceId(invoiceId);
                displaydata();
            }
            else
            {
                MessageBox.Show("Please enter a valid integer for Invoice ID.");
            }
        }
    }
}
