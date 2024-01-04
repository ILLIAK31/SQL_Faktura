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
        SqlTransaction transaction; 
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
            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                string insertInvoiceQuery = "insert into invoice (number, value) values(@Value1, @Value2)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Value1", textBox1.Text);
                cmd.Parameters.AddWithValue("@Value2", "0");
                cmd.Transaction = transaction;
                ExecuteNonQueryWithTransaction(insertInvoiceQuery);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                conn.Close();
                displaydata();
            }
        }
        private void button_delete_Click(object sender, EventArgs e)
        {
            try
            {
                int invoiceId = Convert.ToInt32(textBox3.Text);
                conn.Open();
                transaction = conn.BeginTransaction();
                string posQuery = "DELETE FROM invoice_pos WHERE invoice_id = @Value3";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Value3", invoiceId);
                cmd.Transaction = transaction;
                ExecuteNonQueryWithTransaction(posQuery);
                string invoiceQuery = "DELETE FROM invoice WHERE invoice_id = @Value3";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Value3", invoiceId);
                ExecuteNonQueryWithTransaction(invoiceQuery);
                transaction.Commit();
                MessageBox.Show("Invoice and related records deleted successfully.");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                conn.Close();
                cleardata();
                displaydata();
            }
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
            cmd.CommandText = "update invoice set number='" + textBox1.Text + "' where invoice_id='" + textBox3.Text.ToString() + "' ";
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
            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                cmd.CommandText = $"SELECT SUM(value) AS TotalSum FROM invoice_pos WHERE invoice_id = {invoiceId}";
                cmd.Transaction = transaction;
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("TotalSum")))
                    {
                        decimal totalSum = reader.GetDecimal(reader.GetOrdinal("TotalSum"));
                        reader.Close();
                        cmd.CommandText = $"UPDATE invoice SET value = {totalSum} WHERE invoice_id = {invoiceId}";
                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }
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
                DisplaySumForInvoiceId(invoiceId);
                displaydata();
            }
            else
            {
                MessageBox.Show("Please enter a valid integer for Invoice ID.");
            }
        }
        private DataTable GetInvoicePositions(int invoiceId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(conn.ConnectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM invoice_pos WHERE invoice_id = {invoiceId}";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            return dt;
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                int selectedInvoiceId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["invoice_id"].Value);
                Form positionsForm = new Form();
                DataGridView dataGridViewPositions = new DataGridView();
                dataGridViewPositions.Dock = DockStyle.Fill;
                DataTable dt = GetInvoicePositions(selectedInvoiceId);
                dataGridViewPositions.DataSource = dt;
                positionsForm.Controls.Add(dataGridViewPositions);
                positionsForm.Text = "Invoice Positions";
                positionsForm.Size = new System.Drawing.Size(600, 400);
                positionsForm.ShowDialog();
            }
        }
    }
}
