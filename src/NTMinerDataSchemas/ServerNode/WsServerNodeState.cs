﻿using System.Text;

namespace NTMiner.ServerNode {
    public class WsServerNodeState : IWsServerNode, ISignableData {
        public WsServerNodeState() {
            this.Cpu = new CpuData();
        }

        public string Address { get; set; }

        public string Description { get; set; }

        public int MinerClientWsSessionCount { get; set; }

        public int MinerStudioWsSessionCount { get; set; }

        public int MinerClientSessionCount { get; set; }

        public int MinerStudioSessionCount { get; set; }

        public string OSInfo { get; set; }
        public CpuData Cpu { get; set; }
        public ulong TotalPhysicalMemory { get; set; }

        public double CpuPerformance { get; set; }

        public ulong AvailablePhysicalMemory { get; set; }
        public double ProcessMemoryMb { get; set; }
        public long ThreadCount { get; set; }
        public long HandleCount { get; set; }
        public string AvailableFreeSpaceInfo { get; set; }

        public StringBuilder GetSignData() {
            return this.GetActionIdSign("F9C674C1-247D-45DF-984A-2180AD76F2BB");
        }
    }
}
