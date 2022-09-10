﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NTMiner.Gpus;
using System;
using System.Collections.Generic;
using System.Management;

namespace NTMiner {
    [TestClass]
    public class GpuNameTests {
        [TestMethod]
        public void CeilingTest() {
            ulong totalMemory = (ulong)(3.9 * NTKeyword.ULongG);
            ulong totalMemoryGb = (totalMemory + NTKeyword.ULongG - 1) / NTKeyword.ULongG;
            Assert.IsTrue(4 == totalMemoryGb);
            totalMemory = (ulong)(3.1 * NTKeyword.ULongG);
            totalMemoryGb = (totalMemory + NTKeyword.ULongG) / NTKeyword.ULongG;
            Assert.IsTrue(4 == totalMemoryGb);
        }

        [TestMethod]
        public void GpuNameTest() {
            HashSet<GpuName> hashSet = new HashSet<GpuName>();
            GpuName gpuName1 = new GpuName {
                Name = "580 Series",
                TotalMemory = NTKeyword.ULongG * 8
            };
            hashSet.Add(gpuName1);
            Console.WriteLine(gpuName1.ToString());
            GpuName gpuName2 = new GpuName {
                Name = "580 Series",
                TotalMemory = (ulong)(NTKeyword.ULongG * 7.9)
            };
            hashSet.Add(gpuName2);
            Console.WriteLine(gpuName2.ToString());
            Assert.AreEqual(gpuName1.GetHashCode(), gpuName2.GetHashCode());
            Assert.AreEqual(1, hashSet.Count);
        }

        [TestMethod]
        public void IsNCardTest() {
            using (var mos = new ManagementObjectSearcher("SELECT Caption FROM Win32_VideoController")) {
                foreach (ManagementBaseObject item in mos.Get()) {
                    foreach (var property in item.Properties) {
                        if ((property.Value ?? string.Empty).ToString().IgnoreCaseContains("NVIDIA")) {
                            Console.WriteLine(property.Value);
                        }
                    }
                }
            }
        }
    }
}
