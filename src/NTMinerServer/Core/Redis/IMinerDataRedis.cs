﻿using NTMiner.Core.MinerServer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NTMiner.Core.Redis {
    public interface IMinerDataRedis : IReadOnlyMinerDataRedis {
        Task SetAsync(MinerData data);
        Task UpdateAsync(MinerSign minerSign);
        Task DeleteAsync(IMinerData data);
        Task DeleteAsync(IEnumerable<IMinerData> datas);
    }
}
