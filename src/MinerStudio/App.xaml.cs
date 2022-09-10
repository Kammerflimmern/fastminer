﻿using NTMiner.MinerStudio;
using NTMiner.View;
using NTMiner.Views;
using NTMiner.Views.Ucs;
using NTMiner.Vms;
using NTMiner.Ws;
using System;
using System.Diagnostics;
using System.Windows;

namespace NTMiner {
    public partial class App : Application {
        public App() {
            Hub.MessagePathHub.SetNotifyProperty();
            // 群控客户端独立一个子目录，从而挖矿客户端和群控客户端放在同一个目录时避免路径重复。
            HomePath.SetHomeDirFullName(System.IO.Path.Combine(HomePath.AppDomainBaseDirectory, "NTMiner"));
            VirtualRoot.SetOut(NotiCenterWindowViewModel.Instance);
            Logger.SetDir(HomePath.HomeLogsDirFullName);
            WpfUtil.Init();
            AppUtil.Init(this);
            InitializeComponent();
        }

        private readonly IAppViewFactory _appViewFactory = new AppViewFactory();

        protected override void OnExit(ExitEventArgs e) {
            VirtualRoot.RaiseEvent(new AppExitEvent());
            RpcRoot.RpcUser?.Logout();
            base.OnExit(e);
            NTMinerConsole.Free();
        }

        protected override void OnStartup(StartupEventArgs e) {
            // 之所以提前到这里是因为升级之前可能需要下载升级器，下载升级器时需要下载器
            VirtualRoot.BuildCmdPath<ShowFileDownloaderCommand>(location: this.GetType(), LogEnum.DevConsole, path: message => {
                FileDownloader.ShowWindow(message.DownloadFileUrl, message.FileTitle, message.DownloadComplete);
            });
            VirtualRoot.BuildCmdPath<UpgradeCommand>(location: this.GetType(), LogEnum.DevConsole, path: message => {
                AppRoot.Upgrade(message.FileName, message.Callback);
            });
            VirtualRoot.BuildCmdPath<ShowSignUpPageCommand>(location: this.GetType(), LogEnum.DevConsole, path: message => {
                UIThread.Execute(() => {
                    SignUpPage.ShowWindow();
                });
            });
            if (AppUtil.GetMutex(NTKeyword.MinerStudioAppMutex)) {
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                // 因为登录窗口会用到VirtualRoot.Out，而Out的延迟自动关闭消息会用到倒计时
                VirtualRoot.StartTimer(new WpfTimingEventProducer());
                NotiCenterWindow.ShowWindow();
                AppRoot.RemoteDesktop = MsRemoteDesktop.OpenRemoteDesktop;
                MinerStudioRoot.Login(() => {
                    MinerStudioRoot.Init(new MinerStudioWsClient());
                    _ = MinerStudioRoot.MinerStudioService;// 访问一下从而提前拉取本地服务数据
                    NTMinerContext.Instance.Init(() => {
                        _appViewFactory.BuildPaths();
                        UIThread.Execute(() => {
                            MinerStudioRoot.MinerClientsWindowVm.OnPropertyChanged(nameof(MinerStudioRoot.MinerClientsWindowVm.NetTypeText));
                            if (RpcRoot.IsOuterNet) {
                                MinerStudioRoot.MinerClientsWindowVm.QueryMinerClients();
                            }
                            else {
                                VirtualRoot.BuildOnecePath<ClientSetInitedEvent>("刷新矿机列表界面", LogEnum.DevConsole, pathId: PathId.Empty, this.GetType(), PathPriority.Normal, path: message => {
                                    MinerStudioRoot.MinerClientsWindowVm.QueryMinerClients();
                                });
                            }
                            AppRoot.NotifyIcon = ExtendedNotifyIcon.Create("群控客户端", isMinerStudio: true);
                            VirtualRoot.Execute(new ShowMinerClientsWindowCommand(isToggle: false));
                            NTMinerConsole.MainUiOk();
                        });
                    });
                }, btnCloseClick: () => {
                    Shutdown();
                });
                #region 处理显示主界面命令
                VirtualRoot.BuildCmdPath<ShowMainWindowCommand>(location: this.GetType(), LogEnum.DevConsole, path: message => {
                    VirtualRoot.Execute(new ShowMinerClientsWindowCommand(isToggle: message.IsToggle));
                });
                #endregion
                HttpServer.Start($"http://{NTKeyword.Localhost}:{NTKeyword.MinerStudioPort.ToString()}");
            }
            else {
                try {
                    _appViewFactory.ShowMainWindow(this);
                }
                catch (Exception) {
                    DialogWindow.ShowSoftDialog(new DialogWindowViewModel(
                        message: "另一个群控客户端正在运行但唤醒失败，请重试。",
                        title: "错误",
                        icon: "Icon_Error"));
                    Process currentProcess = Process.GetCurrentProcess();
                    NTMiner.Windows.TaskKill.KillOtherProcess(currentProcess);
                }
            }
            base.OnStartup(e);
        }
    }
}
