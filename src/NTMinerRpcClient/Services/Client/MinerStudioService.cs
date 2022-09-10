﻿using NTMiner.Controllers;
using System;
using System.Diagnostics;
using System.IO;

namespace NTMiner.Services.Client {
    public class MinerStudioService {
        private readonly string _controllerName = ControllerUtil.GetControllerName<IMinerStudioController>();
        internal MinerStudioService() {
        }

        /// <summary>
        /// 本机网络调用
        /// </summary>
        /// <param name="callback"></param>
        public void ShowMainWindowAsync(Action<bool, Exception> callback) {
            RpcRoot.JsonRpc.PostAsync<bool>(
                NTKeyword.Localhost,
                NTKeyword.MinerStudioPort,
                _controllerName,
                nameof(IMinerStudioController.ShowMainWindow),
                callback);
        }

        /// <summary>
        /// 本机同步网络调用
        /// </summary>
        public void CloseMinerStudioAsync(Action callback) {
            string location = NTMinerRegistry.GetLocation(NTMinerAppType.MinerStudio);
            if (string.IsNullOrEmpty(location) || !File.Exists(location)) {
                callback?.Invoke();
                return;
            }
            string processName = Path.GetFileNameWithoutExtension(location);
            if (Process.GetProcessesByName(processName).Length == 0) {
                callback?.Invoke();
                return;
            }
            RpcRoot.JsonRpc.PostAsync<ResponseBase>(
                NTKeyword.Localhost,
                NTKeyword.MinerStudioPort,
                _controllerName,
                nameof(IMinerStudioController.CloseMinerStudio),
                new object(),
                callback: (response, e) => {
                    if (!response.IsSuccess()) {
                        try {
                            Windows.TaskKill.Kill(processName, waitForExit: true);
                        }
                        catch (Exception ex) {
                            Logger.ErrorDebugLine(ex);
                        }
                    }
                    callback?.Invoke();
                }, timeountMilliseconds: 2000);
        }
    }
}
