﻿using QLibc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace IGI_GraphEditor
{
    class QMemory
    {
        internal static string gameName = "IGI";
        internal static float deltaToGround = 7000.0f;
        internal static IntPtr gtGameBase = (IntPtr)0x00400000; //Game base address.

        internal static void StartGame(string args = "window")
        {
            Process.Start(gameName + "_" + args);
            GT.GT_FindGameProcess(gameName);
        }

        internal static bool FindGame(bool enableLogs = false)
        {
            bool gameFound = false;
            try
            {
                if (enableLogs)
                    GT.GT_EnableLogs();

                var pname = Process.GetProcessesByName(gameName);
                if (pname.Length == 0)
                    gameFound = false;
                else
                {
                    if (GT.GT_FindGameProcess(gameName) != IntPtr.Zero)
                        gameFound = true;
                }
            }
            catch (Exception ex)
            {
                QUtils.ShowError(ex.Message);
            }
            return gameFound;
        }

        internal static int GetCurrentLevel()
        {
            try
            {
                IntPtr levelAddr = (IntPtr)0x00539560;
                long level = GT.GT_ReadInt(levelAddr);
                if (level > QUtils.GAME_MAX_LEVEL) QUtils.ShowSystemFatalError("IGI Editor demo limited to only" + QUtils.GAME_MAX_LEVEL + "levels");
                return (int)level;
            }
            catch (Exception ex) { return 1; }
        }

        internal static void DisableGameWarnings()
        {
            unsafe
            {
                IntPtr disableWarnAddr = (IntPtr)0x00936274;
                int disableWarn = 0;
                GT.GT_WriteAddress(disableWarnAddr, &disableWarn);
            }
        }

        static IntPtr GetGameBaseAddr()
        {
            var pid = GT.GT_GetProcessID();
            return GT.GT_GetGameBaseAddress(pid);
        }

        internal static IntPtr GetHumanHealthAddr()
        {
            var humanAddr = GetHumanBaseAddress() + (int)0x254;
            return humanAddr;
        }

        internal static IntPtr GetHumanBaseAddress(bool addLog = true)
        {
            uint humanStaticPtr = (uint)0x0016E210;
            uint[] humanAddrOffs = { 0x8, 0x7CC, 0x14 };
            IntPtr humanBasePtr = IntPtr.Zero, humanBaseAddr = IntPtr.Zero;

            humanBasePtr = GT.GT_ReadPointerOffset(gtGameBase, humanStaticPtr);
            humanBaseAddr = GT.GT_ReadPointerOffsets(humanBasePtr, humanAddrOffs, (uint)humanAddrOffs.Count() * sizeof(int));

            if (addLog)
            {
                QUtils.AddLog("GetHumanBaseAddress() humanBasePointer 0x" + humanBasePtr);
                QUtils.AddLog("GetHumanBaseAddress () humanBaseAddress  : 0x" + humanBaseAddr);
            }
            return humanBaseAddr;
        }

        internal static IntPtr GetStatusMsgAddr()
        {
            uint statusMsgStaticPointer = (uint)0x001C8A20;
            uint[] statusMsgAddressOffsets = { 0x8, 0x8, 0x8, 0x4E4 };
            IntPtr statusMsgBasePointer = IntPtr.Zero, statusMsgAddress = IntPtr.Zero;

            statusMsgBasePointer = GT.GT_ReadPointerOffset(gtGameBase, statusMsgStaticPointer);
            statusMsgAddress = GT.GT_ReadPointerOffsets(statusMsgBasePointer, statusMsgAddressOffsets, (uint)statusMsgAddressOffsets.Count() * sizeof(int)) + 0x4C;
            return statusMsgAddress;
        }

        internal static bool SetStatusMsgText(string statusMsgTxt)
        {
            var statusMsgAddr = GetStatusMsgAddr();
            return GT.GT_WriteMemory(statusMsgAddr, "string", statusMsgTxt);
        }

        internal static string GetStatusMsgText()
        {
            var statusMsgAddr = GetStatusMsgAddr();
            string statusMsg = GT.GT_ReadString(statusMsgAddr);
            return statusMsg;
        }


        static internal float GetRealAngle()
        {
            IntPtr humanBaseAddress = GetHumanBaseAddress() + (int)0x348;
            var angleAddrH = humanBaseAddress + 0x1C4;
            var angleAddrV = humanBaseAddress + 0xBF4;

            float angle = GT.GT_ReadFloat(angleAddrH);
            QUtils.AddLog("GetRealAngle() Address : 0x" + angleAddrH.ToString());
            QUtils.AddLog("GetRealAngle() Value : " + angle);

            return angle;
        }

    }
}
