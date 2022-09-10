﻿namespace NTMiner.CloudFileUrlGenerater {
    /// <summary>
    /// 抽象阿里云OSS和七牛Kodo，因为阿里云OSS坑爹太贵，七牛便宜一半。
    /// </summary>
    public interface ICloudFileUrlGenerater {
        string GeneratePresignedUrl(string bucketName, string key);
    }
}
