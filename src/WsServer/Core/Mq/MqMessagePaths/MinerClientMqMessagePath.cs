﻿using NTMiner.Core.MinerServer;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace NTMiner.Core.Mq.MqMessagePaths {
    public class MinerClientMqMessagePath : AbstractMqMessagePath {
        private readonly string _queryClientsForWsResponseRoutingKey;
        private readonly string _autoQueryClientsForWsResponseRoutingKey;

        public override bool IsReadyToBuild {
            get { return true; }
        }

        public MinerClientMqMessagePath(string queue, string thisServerAddress) : base(queue) {
            _queryClientsForWsResponseRoutingKey = string.Format(MqKeyword.QueryClientsForWsResponseRoutingKey, thisServerAddress);
            _autoQueryClientsForWsResponseRoutingKey = string.Format(MqKeyword.AutoQueryClientsForWsResponseRoutingKey, thisServerAddress);
        }

        protected override Dictionary<string, Action<BasicDeliverEventArgs>> GetPaths() {
            return new Dictionary<string, Action<BasicDeliverEventArgs>> {
                [_queryClientsForWsResponseRoutingKey] = ea => {
                    string appId = ea.BasicProperties.AppId;
                    string mqCorrelationId = ea.BasicProperties.CorrelationId;
                    string loginName = ea.BasicProperties.ReadHeaderString(MqKeyword.LoginNameHeaderName);
                    string sessionId = ea.BasicProperties.ReadHeaderString(MqKeyword.SessionIdHeaderName);
                    ea.BasicProperties.ReadHeaderGuid(MqKeyword.StudioIdHeaderName, out Guid studioId);
                    QueryClientsResponse response = MinerClientMqBodyUtil.GetQueryClientsResponseMqReceiveBody(ea.Body);
                    if (response != null) {
                        VirtualRoot.RaiseEvent(new QueryClientsForWsResponseMqEvent(appId, mqCorrelationId, ea.GetTimestamp(), loginName, studioId, sessionId, response));
                    }
                },
                [_autoQueryClientsForWsResponseRoutingKey] = ea => {
                    string appId = ea.BasicProperties.AppId;
                    string mqCorrelationId = ea.BasicProperties.CorrelationId;
                    QueryClientsResponseEx[] responses = MinerClientMqBodyUtil.GetAutoQueryClientsResponseMqReceiveBody(ea.Body);
                    if (responses != null && responses.Length != 0) {
                        var timestamp = ea.GetTimestamp();
                        foreach (var response in responses) {
                            VirtualRoot.RaiseEvent(new QueryClientsForWsResponseMqEvent(appId, mqCorrelationId, timestamp, response.LoginName, response.StudioId, response.SessionId, response));
                        }
                    }
                }
            };
        }
    }
}
