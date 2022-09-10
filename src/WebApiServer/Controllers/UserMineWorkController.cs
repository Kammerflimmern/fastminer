﻿using NTMiner.Core;
using NTMiner.Core.MinerServer;
using NTMiner.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;

namespace NTMiner.Controllers {
    public class UserMineWorkController : ApiControllerBase, IUserMineWorkController {
        #region MineWorks
        [Role.User]
        [HttpPost]
        public DataResponse<List<UserMineWorkData>> MineWorks([FromBody]object request) {
            if (request == null) {
                return ResponseBase.InvalidInput<DataResponse<List<UserMineWorkData>>>("参数错误");
            }
            try {
                var data = AppRoot.MineWorkSet.GetsByLoginName(User.LoginName);
                return DataResponse<List<UserMineWorkData>>.Ok(data);
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError<DataResponse<List<UserMineWorkData>>>(e.Message);
            }
        }
        #endregion

        #region AddOrUpdateMineWork
        [Role.User]
        [HttpPost]
        public ResponseBase AddOrUpdateMineWork([FromBody]DataRequest<MineWorkData> request) {
            if (request == null || request.Data == null) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                AppRoot.MineWorkSet.AddOrUpdate(request.Data.ToUserMineWork(User.LoginName));
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError(e.Message);
            }
        }
        #endregion

        #region RemoveMineWork
        [Role.User]
        [HttpPost]
        public ResponseBase RemoveMineWork([FromBody]DataRequest<Guid> request) {
            if (request == null || request.Data == Guid.Empty) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                IUserMineWork mineWork = AppRoot.MineWorkSet.GetById(request.Data);
                if (mineWork == null) {
                    return ResponseBase.Ok();
                }
                if (mineWork.LoginName != User.LoginName) {
                    return ResponseBase.Forbidden("无权操作");
                }
                if (AppRoot.ClientDataSet.IsAnyClientInWork(request.Data)) {
                    return ResponseBase.ClientError($"作业{mineWork.Name}下有矿机，请先移除矿机再做删除操作");
                }
                AppRoot.MineWorkSet.RemoveById(request.Data);
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError(e.Message);
            }
        }
        #endregion

        #region ExportMineWork
        [Role.User]
        [HttpPost]
        public ResponseBase ExportMineWork([FromBody]ExportMineWorkRequest request) {
            if (request == null) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                IUserMineWork mineWork = AppRoot.MineWorkSet.GetById(request.MineWorkId);
                if (mineWork == null) {
                    return ResponseBase.NotExist();
                }
                if (mineWork.LoginName != User.LoginName) {
                    return ResponseBase.Forbidden("无权操作");
                }
                string localJsonFileFullName = SpecialPath.GetMineWorkLocalJsonFileFullName(request.MineWorkId);
                string serverJsonFileFullName = SpecialPath.GetMineWorkServerJsonFileFullName(request.MineWorkId);
                File.WriteAllText(localJsonFileFullName, request.LocalJson);
                File.WriteAllText(serverJsonFileFullName, request.ServerJson);
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError<ResponseBase>(e.Message);
            }
        }
        #endregion

        #region GetLocalJson
        [Role.User]
        [HttpPost]
        public DataResponse<string> GetLocalJson([FromBody]DataRequest<Guid> request) {
            if (request == null) {
                return ResponseBase.InvalidInput<DataResponse<string>>("参数错误");
            }
            try {
                IUserMineWork mineWork = AppRoot.MineWorkSet.GetById(request.Data);
                if (mineWork == null) {
                    return ResponseBase.NotExist<DataResponse<string>>();
                }
                if (!User.IsAdmin() && mineWork.LoginName != User.LoginName) {
                    return ResponseBase.Forbidden<DataResponse<string>>("无权操作");
                }
                string localJsonFileFullName = SpecialPath.GetMineWorkLocalJsonFileFullName(request.Data);
                string data = string.Empty;
                if (File.Exists(localJsonFileFullName)) {
                    data = File.ReadAllText(localJsonFileFullName);
                }
                return DataResponse<string>.Ok(data);
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError<DataResponse<string>>(e.Message);
            }
        }
        #endregion
    }
}
