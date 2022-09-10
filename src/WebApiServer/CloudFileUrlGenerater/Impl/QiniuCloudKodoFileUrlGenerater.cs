﻿using Qiniu.Storage;
using Qiniu.Util;

namespace NTMiner.CloudFileUrlGenerater.Impl {
    public class QiniuCloudKodoFileUrlGenerater : ICloudFileUrlGenerater {
        private readonly Mac _mac;
        /// <summary>
        /// 阿里云OSS比七牛Kodo贵一半。
        /// </summary>
        public QiniuCloudKodoFileUrlGenerater() {
            _mac = new Mac(ServerRoot.HostConfig.KodoAccessKey, ServerRoot.HostConfig.KodoSecretKey);
        }

        public string GeneratePresignedUrl(string bucketName, string key) {
            // 注意：Qiniu.dll是github上下载源码编译的，因为Qiniu依赖的Newtonsoft.Json版本不对
            // TODO:搁置，需要开发个工具同时往阿里云oss和七牛kodo上传文件
            string kodoDomain = ServerRoot.HostConfig.KodoDomain;
            if (string.IsNullOrEmpty(kodoDomain)) {
                kodoDomain = "kodo.ntminer.top";
            }
            string domain = $"http://{bucketName}.{kodoDomain}";
            int expireInSeconds = 600;
            string privateUrl = DownloadManager.CreatePrivateUrl(_mac, domain, key, expireInSeconds);
            return privateUrl;
        }
    }
}
