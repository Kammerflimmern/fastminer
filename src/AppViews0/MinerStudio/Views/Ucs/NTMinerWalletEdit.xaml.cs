﻿using NTMiner.MinerStudio.Vms;
using NTMiner.Views;
using NTMiner.Vms;
using System.Windows.Controls;

namespace NTMiner.MinerStudio.Views.Ucs {
    public partial class NTMinerWalletEdit : UserControl {
        public static void ShowWindow(FormType formType, NTMinerWalletViewModel source) {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "NTMiner钱包",
                FormType = formType,
                IsMaskTheParent = true,
                Width = 520,
                CloseVisible = System.Windows.Visibility.Visible,
                IconName = "Icon_Wallet"
            }, ucFactory: (window) => {
                NTMinerWalletViewModel vm = new NTMinerWalletViewModel(source);
                window.BuildCloseWindowOnecePath(vm.Id);
                return new NTMinerWalletEdit(vm);
            }, fixedSize: true);
        }

        public NTMinerWalletViewModel Vm { get; private set; }

        public NTMinerWalletEdit(NTMinerWalletViewModel vm) {
            this.Vm = vm;
            this.DataContext = vm;
            InitializeComponent();
        }
    }
}
