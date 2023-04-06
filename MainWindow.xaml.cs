using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MySqlConnector;
using System.Data;
using System.Configuration;

namespace DB_Project
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        public MainWindow()
        {
            InitializeComponent();
            string selection = "orders";
            LoadGridDataAsync(selection,ordersDataGrid);

            selection = "customers";
            LoadGridDataAsync(selection, customerDataGrid);
        }

        // this enables to point to customer datagrid in the new window
        public DataGrid CustomerDataGrid
        {
            get { return customerDataGrid; }
        }

        public async Task LoadGridDataAsync(string selection, DataGrid datagrid)
        {
            using var connection = new MySqlConnection(connectionString);
            {
                await connection.OpenAsync();

                using var command = new MySqlCommand("SELECT * FROM " + selection, connection);
                using var reader = await command.ExecuteReaderAsync();
                {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        datagrid.ItemsSource = dataTable.DefaultView;
                    
                }
            }
        }

        private async Task RemoveCustomersAsync(int customerId)
        {
            using var connection = new MySqlConnection(connectionString);
            {
                await connection.OpenAsync();

                // First delete the related orders
                using (var command = new MySqlCommand("DELETE FROM orders WHERE customer_id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", customerId);
                    await command.ExecuteNonQueryAsync();
                }

                // Then delete the customer
                using (var command = new MySqlCommand("DELETE FROM customers WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", customerId);
                    int result = await command.ExecuteNonQueryAsync();
                    if (result == 0) // no rows affected by the delete operation
                    {
                        MessageBox.Show("Id not found!");
                    }
                    else
                    { // reload myDatatGrid1 after succesful deletion
                        await LoadGridDataAsync("customers", customerDataGrid);
                        await LoadGridDataAsync("orders", ordersDataGrid);
                    }
                }
            }
        }

        private async Task RemoveOrdersAsync(int orderId)
        {
            using var connection = new MySqlConnection(connectionString);
            {
                await connection.OpenAsync();

                // First delete the related orders
                using var command = new MySqlCommand("DELETE FROM orders WHERE id = @id", connection);
                {
                    command.Parameters.AddWithValue("@id", orderId);
                    int result = await command.ExecuteNonQueryAsync();
                    if (result == 0)
                    {
                        MessageBox.Show("Order Id not found!");
                    }
                    else
                    {
                        await LoadGridDataAsync("orders", ordersDataGrid);
                    }
                }
            }
        }

        private void BtnRemoveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtOrderId.Text, out int orderId))
            {
                MessageBox.Show("invalid order id!");
                return;
            }

            RemoveOrdersAsync(orderId);
            MessageBox.Show("Order removed successfully!");
            txtCustomerId.Clear();

        }

        private async void BtnRemoveCustomer_Click(object sender, RoutedEventArgs e)
        {

            if (!int.TryParse(txtCustomerId.Text, out int customerId))
            {
                MessageBox.Show("Invalid customer id!");
                return;
            }

                await RemoveCustomersAsync(customerId);
                MessageBox.Show("Customer removed successfully!");
            // clear text box input
                txtCustomerId.Clear();
        }

        private void BtnAddNewCustomer_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the newwindow class
            NewWindow newWindow = new NewWindow(this);
            newWindow.ShowDialog();
        }

        private void BtnAddNewOrder_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the new window class
            AddOrderWindow newWindow = new AddOrderWindow(this);
            newWindow.ShowDialog();
        }

    }
}
