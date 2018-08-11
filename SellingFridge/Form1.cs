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
using System.Configuration;

namespace SellingFridge
{
    public partial class Form1 : Form
    {
        private SqlConnection conn;
        private SqlDataReader readSellers;
        private SqlDataReader readFridges;
        private SqlDataReader readModel;
        private SqlDataReader readPrice;
        private SqlDataReader readSellerId;
        private SqlDataReader readCustomerId;
        private SqlDataReader readFridgeId;
        Timer timer;

        string cs = "";
        public Form1()
        {
            InitializeComponent();
            timer = new Timer();
            timer.Tick += Timer_Tick;
            timer.Start();
            //btnprint.Enabled = false;

            conn = new SqlConnection();
            cs = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
            conn.ConnectionString = cs;
            try
            {
                SqlCommand commSellers = new SqlCommand();
                SqlCommand commFridges = new SqlCommand();
                conn = new SqlConnection(cs);

                conn.Open();
                commSellers = new SqlCommand("Select Sellers.lastname from [Fridges].[dbo].Sellers", conn);
                commFridges = new SqlCommand("Select distinct FridgList.manufacture from [Fridges].[dbo].FridgList", conn);
                readSellers = commSellers.ExecuteReader();
                
                while (readSellers.Read())
                {
                    comboBox1.Items.Add(readSellers[0]);
                }
                readSellers.Close();
                readFridges = commFridges.ExecuteReader();
                while (readFridges.Read())
                {
                    comboBox2.Items.Add(readFridges[0]);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txboxDate.Text = System.DateTime.Now.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            SqlCommand commPrice = new SqlCommand();
            conn = new SqlConnection(cs);

            try
            {
                commPrice = new SqlCommand($"Select FridgList.price, FridgList.quantity from [Fridges].[dbo].FridgList where FridgList.model='{comboBox3.SelectedItem.ToString()}'", conn);
                // txBoxPrice.Text = readPrice["price"].ToString();
                conn.Open();
                readPrice = commPrice.ExecuteReader();
                while (readPrice.Read())
                {
                    txBoxPrice.Text = readPrice["price"].ToString();
                    txBoxQuntity.Text = readPrice["quantity"].ToString();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //MessageBox.Show(comboBox2.SelectedItem.ToString());
            comboBox3.Items.Clear();
            comboBox3.Text = "";
            try
            {
                SqlCommand commModel = new SqlCommand();
                SqlCommand commPrice = new SqlCommand();
                conn = new SqlConnection(cs);

                conn.Open();
                commModel = new SqlCommand($"Select FridgList.model from [Fridges].[dbo].FridgList where FridgList.manufacture='{comboBox2.SelectedItem.ToString()}'", conn);
                readModel = commModel.ExecuteReader();

                while (readModel.Read())
                {
                    comboBox3.Items.Add(readModel["model"]);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

            }
        }

        private void txBoxPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnprint_Click(object sender, EventArgs e)
        {
            string custId = "";
            string SellId = "";
            string FridgId = "";
            using (conn=new SqlConnection(cs))
            {
                SqlCommand commCustom = new SqlCommand();
                SqlCommand commSell = new SqlCommand();
                SqlCommand commFridg = new SqlCommand();
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                SqlCommand command = conn.CreateCommand();
                command.Transaction = transaction;
                
                try
                {
                    
                    command.CommandText = $"Insert into [Fridges].[dbo].Customers(firstname, lastname) values('{textBox1.Text}', '{textBox2.Text}')";
                    command.ExecuteNonQuery();

                    commCustom =new SqlCommand( $"Select Customers.id from [Fridges].[dbo].Customers where Customers.firstname='{textBox1.Text}' and Customers.lastname='{textBox2.Text}'", conn);

                    commCustom.Transaction= transaction;
                    readCustomerId = commCustom.ExecuteReader();

                    while (readCustomerId.Read())  //выбор ID покупателя
                    {
                        custId = readCustomerId[0].ToString();
                    }
                    readCustomerId.Close();

                    commSell = new SqlCommand($"Select Sellers.Id from [Fridges].[dbo].Sellers Where Sellers.lastname='{comboBox1.SelectedItem.ToString()}'", conn);

                    commSell.Transaction = transaction;
                    readSellerId = commSell.ExecuteReader();

                    while (readSellerId.Read())  //выбор ID продавца
                    {
                        SellId = readSellerId[0].ToString();
                    }
                    readSellerId.Close();


                    commFridg = new SqlCommand($"Select FridgList.Id from [Fridges].[dbo].FridgList Where FridgList.manufacture='{comboBox2.SelectedItem.ToString()}' and FridgList.model='{comboBox3.SelectedItem.ToString()}'", conn);

                    commFridg.Transaction = transaction;
                    readFridgeId = commFridg.ExecuteReader();

                    while (readFridgeId.Read())  //выбор ID продавца
                    {
                        FridgId = readFridgeId[0].ToString();
                    }
                    readFridgeId.Close();

                    command.CommandText = $"Insert into [Fridges].[dbo].Selling(sellers_id, customers_id, fridges_id) values({SellId}, {custId}, {FridgId})";
                    command.ExecuteNonQuery();

                    command.CommandText = $"UPDATE [Fridges].[dbo].FridgList SET quantity = quantity-1 WHERE id = {FridgId}"; //удаляем из базы
                    txBoxQuntity.Text = (Int32.Parse(txBoxQuntity.Text) - 1).ToString();

                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show(ex.Message);
                }

                finally
                {
                    conn.Close();
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}
