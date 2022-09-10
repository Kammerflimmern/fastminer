﻿using NTMiner.Core.MinerServer;
using System;
using System.Collections.Generic;

namespace NTMiner.Core.MinerStudio.Impl {
    public class NTMinerWalletSet : SetBase, INTMinerWalletSet {
        private readonly Dictionary<Guid, NTMinerWalletData> _dicById = new Dictionary<Guid, NTMinerWalletData>();

        public NTMinerWalletSet() {
            VirtualRoot.BuildCmdPath<AddNTMinerWalletCommand>(location: this.GetType(), LogEnum.DevConsole, path: (message) => {
                if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                    throw new ArgumentNullException();
                }
                if (string.IsNullOrEmpty(message.Input.Wallet)) {
                    throw new ValidationException("NTMinerWallet Wallet can't be null or empty");
                }
                if (_dicById.ContainsKey(message.Input.GetId())) {
                    return;
                }
                NTMinerWalletData entity = new NTMinerWalletData().Update(message.Input);
                RpcRoot.OfficialServer.NTMinerWalletService.AddOrUpdateNTMinerWalletAsync(entity, (response, e) => {
                    if (response.IsSuccess()) {
                        _dicById.Add(entity.Id, entity);
                        VirtualRoot.RaiseEvent(new NTMinerWalletAddedEvent(message.MessageId, entity));
                    }
                    else {
                        VirtualRoot.Out.ShowError(response.ReadMessage(e), autoHideSeconds: 4);
                    }
                });
            });
            VirtualRoot.BuildCmdPath<UpdateNTMinerWalletCommand>(location: this.GetType(), LogEnum.DevConsole, path: (message) => {
                if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                    throw new ArgumentNullException();
                }
                if (string.IsNullOrEmpty(message.Input.Wallet)) {
                    throw new ValidationException("minerGroup Wallet can't be null or empty");
                }
                if (!_dicById.TryGetValue(message.Input.GetId(), out NTMinerWalletData entity)) {
                    return;
                }
                NTMinerWalletData oldValue = new NTMinerWalletData().Update(entity);
                entity.Update(message.Input);
                RpcRoot.OfficialServer.NTMinerWalletService.AddOrUpdateNTMinerWalletAsync(entity, (response, e) => {
                    if (!response.IsSuccess()) {
                        entity.Update(oldValue);
                        VirtualRoot.RaiseEvent(new NTMinerWalletUpdatedEvent(message.MessageId, entity));
                        VirtualRoot.Out.ShowError(response.ReadMessage(e), autoHideSeconds: 4);
                    }
                });
                VirtualRoot.RaiseEvent(new NTMinerWalletUpdatedEvent(message.MessageId, entity));
            });
            VirtualRoot.BuildCmdPath<RemoveNTMinerWalletCommand>(location: this.GetType(), LogEnum.DevConsole, path: (message) => {
                if (message == null || message.EntityId == Guid.Empty) {
                    throw new ArgumentNullException();
                }
                if (!_dicById.ContainsKey(message.EntityId)) {
                    return;
                }
                NTMinerWalletData entity = _dicById[message.EntityId];
                RpcRoot.OfficialServer.NTMinerWalletService.RemoveNTMinerWalletAsync(entity.Id, (response, e) => {
                    if (response.IsSuccess()) {
                        _dicById.Remove(entity.Id);
                        VirtualRoot.RaiseEvent(new NTMinerWalletRemovedEvent(message.MessageId, entity));
                    }
                    else {
                        VirtualRoot.Out.ShowError(response.ReadMessage(e), autoHideSeconds: 4);
                    }
                });
            });
        }

        protected override void Init() {
            RpcRoot.OfficialServer.NTMinerWalletService.GetNTMinerWalletsAsync((response, e) => {
                if (response.IsSuccess()) {
                    foreach (var item in response.Data) {
                        if (!_dicById.ContainsKey(item.GetId())) {
                            _dicById.Add(item.GetId(), item);
                        }
                    }
                }
                VirtualRoot.RaiseEvent(new NTMinerWalletSetInitedEvent());
            });
        }

        public bool TryGetNTMinerWallet(Guid id, out INTMinerWallet wallet) {
            InitOnece();
            var r = _dicById.TryGetValue(id, out NTMinerWalletData g);
            wallet = g;
            return r;
        }

        public IEnumerable<INTMinerWallet> AsEnumerable() {
            InitOnece();
            return _dicById.Values;
        }
    }
}
