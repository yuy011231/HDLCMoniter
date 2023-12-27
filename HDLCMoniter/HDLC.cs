using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceCorpDllWrap;

namespace HDLCMoniter
{
    internal class HDLC
    {
        public IntPtr DeviceHandler;

        /// <summary>
        /// HDLCのポートを開く
        /// </summary>
        /// <param name="DeviceName">デバイス名</param>
        /// <returns>実行結果(成功:true, 失敗:false)</returns>
        public bool OpenPort(string deviceName)
        {
            try
            {
                // 通信条件
                IFCHDLC_ANY.HDLCNPORTINITDATA portConfiguration = new IFCHDLC_ANY.HDLCNPORTINITDATA();

                portConfiguration.InitializeArray();
                portConfiguration = new IFCHDLC_ANY.HDLCNPORTINITDATA
                {
                    Format = IFCHDLC_ANY.HDLC_FORMAT_NRZI,                      // 符号化フォーマットNRZI
                    Fcs = IFCHDLC_ANY.HDLC_FCS_16,                              // FCS 生成多項式ITUT FCS16
                    AddressMode = IFCHDLC_ANY.HDLC_ADDRESS_NONE,                // アドレスを検出しない
                    Address = new UInt32[13],
                    LineMode = IFCHDLC_ANY.HDLC_LINE_HALF                       // 半二重通信
                    | IFCHDLC_ANY.HDLC_ACCEPT_ERRORFRAME,                       // エラーフレームを受信する
                    Txc = IFCHDLC_ANY.HDLC_SCLK_PTC,                            // 送信内部クロック
                    Rxc = IFCHDLC_ANY.HDLC_RCLK_DPLL,                           // 受信DPLL
                    SourceClock = IFCHDLC_ANY.HDLC_CLOCK_19660800,              // 内部クロック19.7MHz
                    BaudRate = IFCHDLC_ANY.HDLC_BAUD_9600,                      // 9600bps
                    Interface = IFCHDLC_ANY.HDLC_INTERFACE_485,                 // RS-485
                    TxcMode = IFCHDLC_ANY.HDLC_STOUT_NONE,                      // クロックを出力しない
                    SendTiming = 104 | IFCHDLC_ANY.HDLC_TIME_MICRO_SEC,         // 送信前切り替え時間 104us
                    CloseTiming = 104 | IFCHDLC_ANY.HDLC_TIME_MICRO_SEC,        // 送信後切り替え時間 104us
                    CSignal = IFCHDLC_ANY.HDLC_CSIG_OFF,                        // RS信号(C信号)をOFFにする
                    WindowHandle = IntPtr.Zero,                                 // データ転送時に送信されるウィンドウハンドラ
                    WindowMessage = 0,                                          // データ転送時に送信されるウィンドウメッセージ
                    CallBackProc = null
                };

                DeviceHandler = IFCHDLC_ANY.HdlcOpen(deviceName, ref portConfiguration);
                if (DeviceHandler.Equals(new IntPtr(-1)))
                {
                    Console.WriteLine("HDLCの指定デバイスを開けません");
                    DeviceHandler = IntPtr.Zero;
                    return false;
                }

                return true;
            }
            catch
            {
                Console.WriteLine("HDLCの指定デバイスを開けません");
                DeviceHandler = IntPtr.Zero;
                return false;
            }
        }

        /// <summary>
        /// ポートを閉じる
        /// </summary>
        /// <returns>実行結果(成功:true, 失敗:false)</returns>
        public bool ClosePort()
        {
            uint ret;

            ret = IFCHDLC_ANY.HdlcClose(DeviceHandler);
            if (ret != IFCHDLC_ANY.FBIHDLC_ERROR_SUCCESS)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 受信バッファをクリアする
        /// </summary>
        /// <returns>実行結果(成功:true, 失敗:false)</returns>
        public bool ClearBuffer()
        {
            uint ret;

            ret = IFCHDLC_ANY.HdlcClearBuffer(DeviceHandler, IFCHDLC_ANY.HDLC_CLEAR_RECEIVE_BUFFER);
            if (ret != IFCHDLC_ANY.FBIHDLC_ERROR_SUCCESS)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// データを送信する
        /// </summary>
        /// <param name="value">送信データ</param>
        /// <returns>実行結果(成功:true, 失敗:false)</returns>
        public bool Send(List<byte> value)
        {
            uint ret;

            IFCHDLC_ANY.HdlcClearBuffer(DeviceHandler, IFCHDLC_ANY.HDLC_CLEAR_RECEIVE_BUFFER);

            // フレームを送信を行する
            ret = IFCHDLC_ANY.HdlcSendFrame(DeviceHandler, value.ToArray(), (uint)value.Count, 0);
            if (ret != IFCHDLC_ANY.FBIHDLC_ERROR_SUCCESS)   // 送信失敗
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// データを受信する
        /// </summary>
        /// <param name="value">受信データ</param>
        /// <returns>実行結果(成功:true, 失敗:false)</returns>
        public bool Receive(ref List<byte> value)
        {
            uint bufferSize = 16384;    // 受信バッファの最大読み出しサイズを16KBとする
            byte[] buffer = new byte[bufferSize];

            uint ret;
            bool flag = false;

            // 受信バッファの先頭のフレームの長さを取得する
            ret = IFCHDLC_ANY.HdlcGetFrameLength(DeviceHandler, out uint frameLength);
            if ((ret == IFCHDLC_ANY.FBIHDLC_ERROR_SUCCESS) && (frameLength > 0) && (frameLength <= bufferSize))
            {
                // 受信バッファを読み出す
                ret = IFCHDLC_ANY.HdlcReceiveFrame(DeviceHandler, buffer, out frameLength);
                if ((ret == IFCHDLC_ANY.FBIHDLC_ERROR_SUCCESS) && (frameLength > 0))
                {
                    // バッファからリスト変数にコピーする
                    for (int j = 0; j < frameLength; j++)
                    {
                        value.Add(buffer[j]);
                    }
                    flag = true;
                }
            }

            if (flag == false)
            {
                return false;
            }

            return true;
        }
    }
}
