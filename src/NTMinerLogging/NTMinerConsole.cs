﻿using NTMiner.Core;
using NTMiner.Impl;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace NTMiner {
    public static class NTMinerConsole {
        private static class SafeNativeMethods {
            [DllImport(DllName.Kernel32Dll)]
            private static extern bool AllocConsole();
            [DllImport(DllName.Kernel32Dll)]
            internal static extern bool FreeConsole();
            [DllImport(DllName.Kernel32Dll)]
            internal static extern IntPtr GetConsoleWindow();
            [DllImport(DllName.User32Dll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
            internal static extern void MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
            [DllImport(DllName.User32Dll, SetLastError = true)]
            internal static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
            [DllImport(DllName.Kernel32Dll, SetLastError = true)]
            private static extern IntPtr GetStdHandle(int hConsoleHandle);
            [DllImport(DllName.Kernel32Dll, SetLastError = true)]
            private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);
            [DllImport(DllName.Kernel32Dll, SetLastError = true)]
            private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);

            internal static void DisbleQuickEditMode() {
                const int STD_INPUT_HANDLE = -10;
                const uint ENABLE_PROCESSED_INPUT = 0x0001;
                const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
                const uint ENABLE_INSERT_MODE = 0x0020;

                IntPtr hStdin = GetStdHandle(STD_INPUT_HANDLE);
                GetConsoleMode(hStdin, out uint mode);
                mode &= ~ENABLE_PROCESSED_INPUT;//禁用ctrl+c
                mode &= ~ENABLE_QUICK_EDIT_MODE;//移除快速编辑模式
                mode &= ~ENABLE_INSERT_MODE;    //移除插入模式
                SetConsoleMode(hStdin, mode);
            }

            internal static IntPtr GetOrAlloc(bool disableQuickEditMode) {
                IntPtr console = GetConsoleWindow();
                if (console == IntPtr.Zero) {
                    AllocConsole();
                    if (disableQuickEditMode) {
                        DisbleQuickEditMode();
                    }
                    console = GetConsoleWindow();
                    Console.WriteLine();
                }
                return console;
            }
        }

        /// <summary>
        /// 禁止编辑控制台窗口
        /// </summary>
        public static void DisbleQuickEditMode() {
            SafeNativeMethods.DisbleQuickEditMode();
        }

        public static IntPtr GetOrAlloc(bool disableQuickEditMode = true) {
            return SafeNativeMethods.GetOrAlloc(disableQuickEditMode);
        }

        public static void MoveWindow(int x, int y, int nWidth, int nHeight, bool bRepaint) {
            SafeNativeMethods.MoveWindow(SafeNativeMethods.GetConsoleWindow(), x, y, nWidth, nHeight, bRepaint);
        }

        /// <summary>
        /// 如果程序没有控制台窗口调用没有副作用，应在程序生命周期最末尾调用。
        /// 因为控制台窗口是按需创建的，所以不能在顺序不定的AppExit中释放，以免释放后又按需创建。
        /// </summary>
        public static void Free() {
            IntPtr console = SafeNativeMethods.GetConsoleWindow();
            if (console != IntPtr.Zero) {
                SafeNativeMethods.FreeConsole();
            }
        }

        // 缓存最近的几十条输出行
        public static readonly IConsoleOutLineSet ConsoleOutLineSet = new ConsoleOutLineSet();

        private static int _uiThreadId;

        public static void SetUIThreadId(int value) {
            _uiThreadId = value;
        }

        private static bool _isEnabled = true;
        public static bool IsEnabled {
            get { return _isEnabled; }
        }

        public static void Enable() {
            _isEnabled = true;
        }

        /// <summary>
        /// 禁用Write则可以避免创建出Windows控制台
        /// </summary>
        public static void Disable() {
            _isEnabled = false;
        }

        private static bool _isMainUiOk = false;
        private static bool IsMainUiOk {
            get {
                if (DevMode.IsDevMode) {
                    return true;
                }
                return _isMainUiOk;
            }
        }

        private static readonly List<Tuple<string, ConsoleColor>> _lineBeforeMainUiOk = new List<Tuple<string, ConsoleColor>>();
        private static readonly object _lockForUserLine = new object();
        public static void MainUiOk() {
            _isMainUiOk = true;
            WriteLinesBeforeMainUiOk();
        }

        private static readonly Action<string, ConsoleColor> _userLineMethod = (line, color) => {
            if (!_isEnabled) {
                return;
            }
            if (!IsMainUiOk) {
                _lineBeforeMainUiOk.Add(new Tuple<string, ConsoleColor>(line, color));
                return;
            }
            WriteLinesBeforeMainUiOk();
            lock (_lockForUserLine) {
                InitOnece();
                Console.ForegroundColor = color;
                Console.WriteLine(line);
                Console.ResetColor();
            }
        };

        private static void WriteLinesBeforeMainUiOk() {
            if (!IsMainUiOk) {
                return;
            }
            lock (_lockForUserLine) {
                if (_lineBeforeMainUiOk.Count != 0) {
                    InitOnece();
                    foreach (var item in _lineBeforeMainUiOk) {
                        Console.ForegroundColor = item.Item2;
                        Console.WriteLine(item.Item1);
                        Console.ResetColor();
                    }
                    _lineBeforeMainUiOk.Clear();
                }
            }
        }

        private static readonly object _locker = new object();
        private static bool _isInited = false;
        public static void InitOnece(bool isForce = false, bool initHide = false) {
            if (!isForce && !_isEnabled) {
                return;
            }
            if (!_isInited) {
                lock (_locker) {
                    if (!_isInited) {
                        _isInited = true;
                        var hWnh = GetOrAlloc();
                        if (initHide) {
                            SafeNativeMethods.ShowWindow(hWnh, 0);
                        }
                    }
                }
            }
        }

        public static void UserLine(string text, MessageType messageType = MessageType.Default) {
            if (messageType == MessageType.Debug && !DevMode.IsDevMode) {
                return;
            }
            UserLine($"NTMiner {messageType.ToString(),-10}  {text}", messageType.ToConsoleColor());
        }

        public static void UserLine(string text, string messageType, ConsoleColor color) {
            UserLine($"NTMiner {messageType,-10}  {text}", color);
        }

        public static void UserError(string text) {
            UserLine(text, MessageType.Error);
        }

        public static void UserInfo(string text) {
            UserLine(text, MessageType.Info);
        }

        public static void UserOk(string text) {
            UserLine(text, MessageType.Ok);
        }

        public static void UserWarn(string text) {
            UserLine(text, MessageType.Warn);
        }

        public static void UserFail(string text) {
            UserLine(text, MessageType.Fail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="foreground"></param>
        /// <param name="withPrefix">表示是否在打印的行前附加个时间戳，默认附加</param>
        public static void UserLine(string line, ConsoleColor foreground, bool withPrefix = true) {
            if (!_isEnabled) {
                return;
            }
            if (withPrefix) {
                line = $"{DateTime.Now.ToString("HH:mm:ss fff")}  {(Thread.CurrentThread.ManagedThreadId == _uiThreadId ? "UI" : "  ")} {line}";
            }
            ConsoleOutLineSet.Add(new ConsoleOutLine {
                Timestamp = GetTimestamp(),
                Line = line
            });
            _userLineMethod?.Invoke(line, foreground);
        }

        private static readonly DateTime UnixBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static long GetTimestamp() {
            var span = (DateTime.Now.ToUniversalTime() - UnixBaseTime).TotalSeconds;
            return (long)span;
        }

        public static void DevLine(string text, MessageType messageType = MessageType.Default, ConsoleColor foreground = default) {
            if (!DevMode.IsDevMode) {
                return;
            }
            if (!_isEnabled) {
                return;
            }
            InitOnece();
            text = $"{DateTime.Now.ToString("HH:mm:ss fff")}  {(Thread.CurrentThread.ManagedThreadId == _uiThreadId ? "UI" : "  ")} {messageType.ToString()} {text}";
            ConsoleColor oldColor = Console.ForegroundColor;
            if (foreground != default) {
                Console.ForegroundColor = foreground;
            }
            else {
                Console.ForegroundColor = messageType.ToConsoleColor();
            }
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

        public static void DevException(Exception e) {
            if (!DevMode.IsDevMode) {
                return;
            }
            if (!_isEnabled) {
                return;
            }
            DevLine(e.GetInnerMessage() + e.StackTrace, MessageType.Error);
        }

        public static void DevException(string message, Exception e) {
            if (!DevMode.IsDevMode) {
                return;
            }
            if (!_isEnabled) {
                return;
            }
            DevLine(message + e.StackTrace, MessageType.Error);
        }

        public static void DevError(string text) {
            DevLine(text, MessageType.Error);
        }

        /// <summary>
        /// 延迟参数计算。
        /// 有一个特殊情况需要注意：不要在Rpc Fire回调中使用，因为Rpc Fire回调中访问getHttpResponse.Result.ReasonPhrase的目的就是为了触发计算，所以不能延迟计算。
        /// </summary>
        /// <param name="getText"></param>
        public static void DevError(Func<string> getText) {
            if (!DevMode.IsDevMode) {
                return;
            }
            if (!_isEnabled) {
                return;
            }
            DevLine(getText(), MessageType.Error);
        }

        public static void DevDebug(string text) {
            DevLine(text, MessageType.Debug);
        }

        /// <summary>
        /// 延迟参数计算。
        /// 有一个特殊情况需要注意：不要在Rpc Fire回调中使用，因为Rpc Fire回调中访问getHttpResponse.Result.ReasonPhrase的目的就是为了触发计算，所以不能延迟计算。
        /// </summary>
        /// <param name="getText"></param>
        public static void DevDebug(Func<string> getText, ConsoleColor foreground = default) {
            if (!DevMode.IsDevMode) {
                return;
            }
            if (!_isEnabled) {
                return;
            }
            if (getText == null) {
                return;
            }
            DevLine(getText(), MessageType.Debug, foreground);
        }

        public static void DevOk(string text) {
            DevLine(text, MessageType.Ok);
        }

        /// <summary>
        /// 延迟参数计算。
        /// 有一个特殊情况需要注意：不要在Rpc Fire回调中使用，因为Rpc Fire回调中访问getHttpResponse.Result.ReasonPhrase的目的就是为了触发计算，所以不能延迟计算。
        /// </summary>
        /// <param name="getText"></param>
        public static void DevOk(Func<string> getText) {
            if (!DevMode.IsDevMode) {
                return;
            }
            if (!_isEnabled) {
                return;
            }
            if (getText == null) {
                return;
            }
            DevLine(getText(), MessageType.Ok);
        }

        public static void DevWarn(string text) {
            DevLine(text, MessageType.Warn);
        }

        public static void DevWarn(Func<string> getText) {
            if (!DevMode.IsDevMode) {
                return;
            }
            if (!_isEnabled) {
                return;
            }
            if (getText == null) {
                return;
            }
            DevLine(getText(), MessageType.Warn);
        }

#if DEBUG
        public static void DevTimeSpan(string text) {
            DevLine(text, MessageType.TimeSpan);
        }
#endif

        public static void DevFail(string text) {
            DevLine(text, MessageType.Fail);
        }
    }
}
