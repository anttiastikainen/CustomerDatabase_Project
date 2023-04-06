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
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        readonly private MainWindow _mainWindow;

        public NewWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private async void BtnAddToDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (txtCustomerId.Text == "" || txtCustomerName.Text == "" || txtCustomerEmail.Text == "")
            {
                MessageBox.Show("Fill all information!");
            }
            // Create a new customer object with the data entered in the new window
            else
            {
                int Id = int.Parse(txtCustomerId.Text);
                string Name = txtCustomerName.Text;
                string Email = txtCustomerEmail.Text;

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new MySqlCommand("INSERT INTO customers (Id, Name, Email) VALUES (@Id, @Name, @Email)", connection);
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@Email", Email);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // update customer view
                await _mainWindow.LoadGridDataAsync("customers", _mainWindow.CustomerDataGrid);
                // close the window
                this.Close();
            }
        }

    }
}
