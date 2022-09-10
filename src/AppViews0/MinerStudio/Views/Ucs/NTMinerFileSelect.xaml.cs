﻿using NTMiner.MinerStudio.Vms;
using System.Windows.Controls;

namespace NTMiner.MinerStudio.Views.Ucs {
    public partial class NTMinerFileSelect : UserControl {
        public NTMinerFileSelectViewModel Vm { get; private set; }

        public NTMinerFileSelect(NTMinerFileSelectViewModel vm) {
            this.Vm = vm;
            this.DataContext = vm;
            InitializeComponent();
        }

        private void Lb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Vm.OnOk?.Invoke(Vm.SelectedResult);
        }

        private void HideView(object sender, System.Windows.RoutedEventArgs e) {
            Vm.HideView?.Execute(null);
        }
    }
}
