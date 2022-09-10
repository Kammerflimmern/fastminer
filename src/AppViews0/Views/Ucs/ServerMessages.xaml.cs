﻿using NTMiner.Vms;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class ServerMessages : UserControl {
        public ServerMessagesViewModel Vm { get; private set; }

        public ServerMessages() {
            if (WpfUtil.IsInDesignMode) {
                return;
            }
            this.Vm = new ServerMessagesViewModel();
            this.DataContext = this.Vm;
            InitializeComponent();
            this.OnLoaded(window => {
                if (ClientAppType.IsMinerStudio) {
                    window.BuildEventPath<Per1MinuteEvent>("周期刷新群控客户端的服务器消息集", LogEnum.DevConsole, location: this.GetType(), PathPriority.Normal,
                        path: message => {
                            VirtualRoot.Execute(new LoadNewServerMessageCommand());
                        });
                }
            });
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (!Vm.MainMenu.IsMinerStudioOuterAdmin) {
                return;
            }
            WpfUtil.DataGrid_EditRow<ServerMessageViewModel>(sender, e);
        }
    }
}
