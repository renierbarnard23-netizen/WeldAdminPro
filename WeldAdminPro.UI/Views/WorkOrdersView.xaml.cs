using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.Views
{
	public partial class WorkOrdersView : UserControl
	{
		private readonly WorkOrderRepository _repository = new WorkOrderRepository();

		public WorkOrdersView()
		{
            InitializeComponent();

            ApplySecurity();

            LoadWorkOrders();
        }

        private void ApplySecurity()
        {
            // =====================================
            // CREATE WORK ORDERS
            // =====================================

            NewWorkOrderButton.IsEnabled =
                PermissionService.HasPermission(
                    SystemPermission.CreateWorkOrders);

            // =====================================
            // EDIT WORK ORDERS
            // =====================================

            EditWorkOrderButton.IsEnabled =
                PermissionService.HasPermission(
                    SystemPermission.EditWorkOrders);

            // =====================================
            // ISSUE MATERIAL
            // =====================================

            IssueMaterialButton.IsEnabled =
                PermissionService.HasPermission(
                    SystemPermission.ApproveStockIssues);

            // =====================================
            // ADD MATERIAL
            // =====================================

            AddMaterialButton.IsEnabled =
                PermissionService.HasPermission(
                    SystemPermission.EditWorkOrders);
        }


        private void NewWorkOrder_Click(
            object sender,
                RoutedEventArgs e)
        {
            if (!PermissionService.HasPermission(
                SystemPermission.CreateWorkOrders))
            {
                MessageBox.Show(
                    "You do not have permission to create work orders.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var window =
                new NewWorkOrderWindow(
                    LoadWorkOrders);

            var result =
                window.ShowDialog();

            if (result == true)
            {
                AuditService.Log(
                    "CREATE WORK ORDER",
                    "Production",
                    "New work order created");

                LoadWorkOrders();
            }
        }

        private void IssueMaterial_Click(
            object sender,
                RoutedEventArgs e)
        {
            if (!PermissionService.HasPermission(
                SystemPermission.ApproveStockIssues))
            {
                MessageBox.Show(
                    "You do not have permission to issue material.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var window =
                new IssueMaterialWindow();

            var result =
                window.ShowDialog();

            if (result == true)
            {
                AuditService.Log(
                    "ISSUE MATERIAL",
                    "Production",
                    "Material issued to work order");
            }
        }

        private void LoadWorkOrders()
        {
            var workOrders =
                _repository
                    .GetAll()
                    .Where(w =>
                        w.Status !=
                        WorkOrderStatus.Completed)
                    .ToList();

            WorkOrdersGrid.ItemsSource =
                workOrders;
        }

        private void AddMaterial_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!PermissionService.HasPermission(
                SystemPermission.EditWorkOrders))
            {
                MessageBox.Show(
                    "You do not have permission to add materials.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (WorkOrdersGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a work order first.");

                return;
            }

            var workOrder =
                (WorkOrder)WorkOrdersGrid.SelectedItem;

            var window =
                new AddWorkOrderMaterialWindow(
                    workOrder.Id);

            var result =
                window.ShowDialog();

            if (result == true)
            {
                AuditService.Log(
                    "ADD MATERIAL",
                    "Production",
                    $"Material added to WO: {workOrder.WorkOrderNumber}");
            }
        }

        private void EditWorkOrder_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!PermissionService.HasPermission(
                SystemPermission.EditWorkOrders))
            {
                MessageBox.Show(
                    "You do not have permission to edit work orders.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (WorkOrdersGrid.SelectedItem == null)
                return;

            var workOrder =
                (WorkOrder)WorkOrdersGrid.SelectedItem;

            var window =
                new EditWorkOrderWindow(
                    workOrder);

            if (window.ShowDialog() == true)
            {
                AuditService.Log(
                    "EDIT WORK ORDER",
                    "Production",
                    $"Edited work order: {workOrder.WorkOrderNumber}");

                LoadWorkOrders();
            }
        }
    }
}