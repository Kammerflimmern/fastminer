﻿using NTMiner.Core;
using NTMiner.Core.MinerServer;
using System;
using System.Linq;
using System.Web.Http;

namespace NTMiner.Controllers {
    // 注意该控制器不能重命名
    public class KernelOutputKeywordController : ApiControllerBase, IKernelOutputKeywordController {
        [Role.Public]
        [HttpPost]
        public KernelOutputKeywordsResponse KernelOutputKeywords([FromBody]object request) {
            try {
                var data = AppRoot.KernelOutputKeywordSet;
                return KernelOutputKeywordsResponse.Ok(data.AsEnumerable().Select(a => KernelOutputKeywordData.Create(a)).ToList(), Timestamp.GetTimestamp(AppRoot.KernelOutputKeywordTimestamp));
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError<KernelOutputKeywordsResponse>(e.Message);
            }
        }

        [Role.Admin]
        [HttpPost]
        public ResponseBase RemoveKernelOutputKeyword([FromBody]DataRequest<Guid> request) {
            if (request == null || request.Data == Guid.Empty) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                VirtualRoot.Execute(new RemoveKernelOutputKeywordCommand(request.Data));
                AppRoot.UpdateKernelOutputKeywordTimestamp(DateTime.Now);
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError(e.Message);
            }
        }

        [Role.Admin]
        [HttpPost]
        public ResponseBase AddOrUpdateKernelOutputKeyword([FromBody]DataRequest<KernelOutputKeywordData> request) {
            if (request == null || request.Data == null) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                VirtualRoot.Execute(new AddOrUpdateKernelOutputKeywordCommand(request.Data));
                AppRoot.UpdateKernelOutputKeywordTimestamp(DateTime.Now);
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError(e.Message);
            }
        }

        [Role.Admin]
        [HttpPost]
        public ResponseBase ClearByExceptedOutputIds(DataRequest<Guid[]> request) {
            if (request == null || request.Data == null || request.Data.Length == 0) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                VirtualRoot.Execute(new ClearKernelOutputKeywordsCommand(request.Data));
                AppRoot.UpdateKernelOutputKeywordTimestamp(DateTime.Now);
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError(e.Message);
            }
        }
    }
}
