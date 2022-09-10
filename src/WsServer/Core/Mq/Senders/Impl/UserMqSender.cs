﻿using NTMiner.Cryptography;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace NTMiner.Core.Mq.Senders.Impl {
    public class UserMqSender : IUserMqSender {
        private readonly IMq _mq;
        public UserMqSender(IMq mq) {
            _mq = mq;
        }

        public void SendUpdateUserRSAKey(string loginName, RSAKey key) {
            if (string.IsNullOrEmpty(loginName) || key == null) {
                return;
            }
            _mq.BasicPublish(
                routingKey: MqKeyword.UpdateUserRSAKeyRoutingKey,
                basicProperties: CreateBasicProperties(loginName),
                body: UserMqBodyUtil.GetUpdateUserRSAKeyMqSendBody(key));
        }

        private IBasicProperties CreateBasicProperties(string loginName) {
            var basicProperties = _mq.CreateBasicProperties();
            basicProperties.Persistent = true;
            basicProperties.Expiration = MqKeyword.Expiration60sec;
            basicProperties.Headers = new Dictionary<string, object> {
                [MqKeyword.LoginNameHeaderName] = loginName
            };

            return basicProperties;
        }
    }
}
