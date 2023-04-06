using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using MySqlConnector;
using System.Data;
using System.Configuration;

namespace DB_Project
{
    /// <summary>
    /// Interaction logic for AddOrderWindow.xaml
    /// </summary>
    public partial class AddOrderWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        readonly private MainWindow _mainWindow;
        public AddOrderWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private async void BtnAddToDatabase_Click(object sender, RoutedEventArgs e)
        {
            if(txtOrderId.Text=="" 
              || txtCustomerId.Text==""
              || txtDate.Text==""
              || txtTotal.Text=="")
            {
                MessageBox.Show("Fill all information!");
            }
            // Create a new order object with the data entered in the window
            else
            {
                int Id = int.Parse(txtOrderId.Text);
                int CustomerId = int.Parse(txtCustomerId.Text);
                DateTime Date = DateTime.Parse(txtDate.Text);
                float Total;

                if (txtTotal.Text.Contains(".") == false)
                {
                    Total = float.Parse(txtTotal.Text);
                }
                else
                {
                    MessageBox.Show("Don't use dot in input!");
                    return;
                }


                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Check if an order with the same ID already exists in the database
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        var result = await command.ExecuteScalarAsync();
                        if ((long)result > 0)
                        {
                            MessageBox.Show("Order ID is already taken.");
                            return;
                        }
                    }

                    // Check if the customer ID exists in the database
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM customers WHERE id = @CustomerId", connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", CustomerId);
                        var result = await command.ExecuteScalarAsync();
                        if ((long)result == 0)
                        {
                            MessageBox.Show("Customer ID not found in the database.");
                            return;
                        }
                    }

                    using (var command = new MySqlCommand("INSERT INTO orders (id, customer_id, order_date, total) VALUES (@Id, @CustomerId, @Date, @Total)", connection))
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        command.Parameters.AddWithValue("@CustomerId", CustomerId);
                        command.Parameters.AddWithValue("@Date", Date);
                        command.Parameters.AddWithValue("@Total", Total);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // update the mainwidow view

                // update customer view
                await _mainWindow.LoadGridDataAsync("orders", _mainWindow.ordersDataGrid);
                this.Close();
            }
        }
    }
}
