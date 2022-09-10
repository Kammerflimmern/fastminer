﻿using NTMiner.MinerStudio.Vms;
using NTMiner.Views;
using NTMiner.Vms;
using System.Windows;
using System.Windows.Controls;

namespace NTMiner.MinerStudio.Views.Ucs {
    public partial class NTMinerWalletPage : UserControl {
        public static void ShowWindow() {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "NTMiner钱包",
                IconName = "Icon_Wallet",
                Width = 800,
                Height = 400,
                IsMaskTheParent = false,
                IsChildWindow = true,
                CloseVisible = Visibility.Visible
            }, ucFactory: (window) => new NTMinerWalletPage(), fixedSize: true);
        }

        public NTMinerWalletPageViewModel Vm { get; private set; }

        public NTMinerWalletPage() {
            if (WpfUtil.IsInDesignMode) {
                return;
            }
            this.Vm = new NTMinerWalletPageViewModel();
            this.DataContext = this.Vm;
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            WpfUtil.DataGrid_EditRow<NTMinerWalletViewModel>(sender, e);
        }
    }
}
