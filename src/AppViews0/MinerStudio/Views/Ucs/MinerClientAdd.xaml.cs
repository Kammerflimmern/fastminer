﻿using NTMiner.MinerStudio.Vms;
using System.Windows;

namespace NTMiner.MinerStudio.Views.Ucs {
    public partial class MinerClientAdd : BlankWindow {
        public static void ShowWindow() {
            MinerClientAddViewModel vm = new MinerClientAddViewModel();
            Window window = new MinerClientAdd(vm);
            window.BuildCloseWindowOnecePath(vm.Id);
            window.MousePosition();
            window.ShowSoftDialog();
        }

        public MinerClientAdd(MinerClientAddViewModel vm) {
            this.DataContext = vm;
            InitializeComponent();
            var owner = WpfUtil.GetTopWindow();
            if (this != owner) {
                this.Owner = owner;
            }
        }
    }
}
