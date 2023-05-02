﻿using Data_Access_Entity;
using Data_Access_Entity.Entities;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Waiter_Main;
using static System.Net.Mime.MediaTypeNames;

namespace Waiter_App
{
    /// <summary>
    /// Interaction logic for Orders.xaml
    /// </summary>
    public partial class Orders : Window
    {
        RestaurantContext restaurantContext = new RestaurantContext();
        public Orders()
        {
            InitializeComponent();
            GetCategoriesToComboBox();
            GetTablesToComboBox();
        }
        #region adaptive borderless-window react
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        #endregion
        #region navigation bar buttons

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }

        #endregion

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow menu = new MainWindow();
            this.Close();
            menu.ShowDialog();
        }
        private void ComboBoxCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListBoxProductsFromMenu.Items.Clear();
                Category selectedCategory = restaurantContext.Categories.FirstOrDefault(a => a.Name == (string)ComboBoxCategories.SelectedValue);
                var products = restaurantContext.Products;
                foreach (var item in products)
                {
                    if (item.CategoryId == selectedCategory.ID)
                    {
                        ListBoxProductsFromMenu.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxTables.SelectedValue != null && ListBoxProductsFromMenu.SelectedItem != null)
                {
                    Order thisorder = restaurantContext.Orders.FirstOrDefault(o => o.Active == false && o.TableId.ToString() == ComboBoxTables.SelectedValue);
                    Product selectedvalue = (Product)ListBoxProductsFromMenu.SelectedItem;

                    if (thisorder != null)
                    {
                        restaurantContext.ProductsOrders.Add(new ProductOrder
                        {
                            OrderId = thisorder.ID,
                            ProductId = selectedvalue.ID
                        });
                        restaurantContext.SaveChanges();
                    }
                    else
                    {
                        restaurantContext.Orders.Add(new Order
                        {
                            Active = false,
                            OrderDate = DateTime.Now,
                            TableId = (int)ComboBoxTables.SelectedValue,
                            WaiterId = User.ID
                        });
                        restaurantContext.SaveChanges();

                        var newOrder = restaurantContext.Orders.FirstOrDefault(o => o.TableId == (int)ComboBoxTables.SelectedValue);
                        restaurantContext.ProductsOrders.Add(new ProductOrder
                        {
                            OrderId = newOrder.ID,
                            ProductId = selectedvalue.ID
                        });
                        restaurantContext.SaveChanges();
                    }
                    GetOrderItems();
                }
                else
                    MessageBox.Show($@"PLease, make your choise first");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ComboBoxTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GetOrderItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void GetCategoriesToComboBox()
        {
            try
            {
                var categories = restaurantContext.Categories;
                foreach (var item in categories)
                {
                    ComboBoxCategories.Items.Add(item.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void GetTablesToComboBox()
        {
            try
            {
                var categories = restaurantContext.Tables;
                foreach (var item in categories)
                {
                    ComboBoxTables.Items.Add(item.ID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void GetOrderItems()
        {
            ListBoxProductsFromOrderByTableNumber.Items.Clear();
            Order thisorderTwo = restaurantContext.Orders.FirstOrDefault(o => o.Active == false && o.TableId == (int)ComboBoxTables.SelectedValue);
            if (thisorderTwo != null)
            {
                List<Product> res = (List<Product>)restaurantContext.ProductsOrders.Include(a => a.Product).Where(a => a.OrderId == thisorderTwo.ID).Select(a => a.Product).ToList();
                List<Product> Show = new List<Product>();
                foreach (var item in res)
                    Show.Add(item);
                foreach (var item in Show)
                    ListBoxProductsFromOrderByTableNumber.Items.Add(item);
                ListBoxProductsFromOrderByTableNumber.Items.Refresh();
            }
        }
    }
}
