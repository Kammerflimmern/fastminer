﻿using NTMiner.Core.MinerServer;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace NTMiner.Core.Mq.Senders.Impl {
    public class MinerClientMqSender : IMinerClientMqSender {
        private readonly IMq _mq;
        public MinerClientMqSender(IMq mq) {
            _mq = mq;
        }

        public void SendMinerDataRemoved(string minerId, Guid clientId) {
            if (string.IsNullOrEmpty(minerId)) {
                return;
            }
            var basicProperties = CreateBasicProperties(clientId);
            _mq.BasicPublish(
                routingKey: MqKeyword.MinerDataRemovedRoutingKey,
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetMinerIdMqSendBody(minerId));
        }

        public void SendMinerDatasRemoved(Guid[] clientIds) {
            if (clientIds == null || clientIds.Length == 0) {
                return;
            }
            var basicProperties = CreateBasicProperties();
            _mq.BasicPublish(
                routingKey: MqKeyword.MinerDatasRemovedRoutingKey,
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetClientIdsMqSendBody(clientIds));
        }

        public void SendResponseClientsForWs(
            string wsServerIp, 
            string loginName, 
            Guid studioId,
            string sessionId, 
            string mqCorrelationId, 
            QueryClientsResponse response) {
            if (response == null) {
                return;
            }
            var basicProperties = CreateWsBasicProperties(loginName, studioId, sessionId);
            if (!string.IsNullOrEmpty(mqCorrelationId)) {
                basicProperties.CorrelationId = mqCorrelationId;
            }
            _mq.BasicPublish(
                routingKey: string.Format(MqKeyword.QueryClientsForWsResponseRoutingKey, wsServerIp),
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetQueryClientsResponseMqSendBody(response));
        }

        public void SendResponseClientsForWs(string wsServerIp, string mqCorrelationId, QueryClientsResponseEx[] responses) {
            if (responses == null || responses.Length == 0) {
                return;
            }
            var basicProperties = CreateBasicProperties();
            if (!string.IsNullOrEmpty(mqCorrelationId)) {
                basicProperties.CorrelationId = mqCorrelationId;
            }
            _mq.BasicPublish(
                routingKey: string.Format(MqKeyword.AutoQueryClientsForWsResponseRoutingKey, wsServerIp),
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetAutoQueryClientsResponseMqSendBody(responses));
        }

        private IBasicProperties CreateWsBasicProperties(string loginName, Guid studioId, string sessionId) {
            var basicProperties = _mq.CreateBasicProperties();
            basicProperties.Persistent = false;// 非持久化的
            basicProperties.Expiration = MqKeyword.Expiration36sec;
            basicProperties.Headers = new Dictionary<string, object> {
                [MqKeyword.LoginNameHeaderName] = loginName,
                [MqKeyword.StudioIdHeaderName] = studioId.ToString(),
                [MqKeyword.SessionIdHeaderName] = sessionId
            };

            return basicProperties;
        }

        private IBasicProperties CreateBasicProperties() {
            var basicProperties = _mq.CreateBasicProperties();
            basicProperties.Persistent = false;// 非持久化的
            basicProperties.Expiration = MqKeyword.Expiration36sec;

            return basicProperties;
        }

        private IBasicProperties CreateBasicProperties(Guid clientId) {
            var basicProperties = _mq.CreateBasicProperties();
            basicProperties.Persistent = false;// 非持久化的
            basicProperties.Expiration = MqKeyword.Expiration60sec;
            basicProperties.Headers = new Dictionary<string, object> {
                [MqKeyword.ClientIdHeaderName] = clientId.ToString()
            };

            return basicProperties;
        }
    }
}
