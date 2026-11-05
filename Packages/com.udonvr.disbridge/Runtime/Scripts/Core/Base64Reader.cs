//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace UdonVR.DisBridge
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Base64Reader : UdonSharpBehaviour
    {
        [Header("Increasing step size decreases decode time but increases frametimes")]
        [SerializeField] 
        private int stepSize = 200;

        [SerializeField] 
        private ReadDataManager dataManager;

        //[Header("Debugging")]
        [SerializeField] 
        private bool debugLogger = false;

        private byte[] b64Bytes;
        private string currentOutputString;
        private System.Diagnostics.Stopwatch stopwatch;
        public bool notDone = true;

        private byte[] _key;
        /*
        public void SetKey(string keyString)
        {
            _key = ToByteArray(keyString);
        }
        private byte[] ToByteArray(string text)
        {
            char[] charArray = text.ToCharArray();
            var g = text[6];
            byte[] byteArray = new byte[charArray.Length];
            for (int c = 0; c < charArray.Length; c++)
            {
                byteArray[c] = Convert.ToByte(charArray[c]);
            }
            return byteArray;
        }

        public void StartDownload(VRCUrl url)
        {
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Decrypt(result.Result);
        }
        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Log($"Error: {result.ErrorCode} > {result.Error} \n\tFor url ({result.Url.Get()})");
            dataManager.fallbackMode = true;
            dataManager.ReadIsDone();
        }
        private int byteValue = 0;
        public void Decrypt(string text)
        {
            //Decrypting
            Log("Decrypting");
            string[] parts = text.Split('\n');
            version = Convert.ToInt32(parts[0]);
            text = parts[1];
            if (debugLogger) Log("[UdonVR]Version: " + version);
            dataManager.PassVersion(version);
            if (string.IsNullOrEmpty(text) || text.Length % 4 > 0)
            {
                return;// null;
            }
            b64Bytes = Convert.FromBase64String(text);
            if (debugLogger)
            {
                string kbytes = string.Empty;
                foreach (byte b in _key)
                {
                    kbytes += $"{b:X2} ";
                }
                Log(kbytes.Trim());
            }
            int keyIndex = 0;
            for (int i = 0; i < b64Bytes.Length; i++)
            {
                if (keyIndex == _key.Length)
                {
                    keyIndex = 0;
                }
                byteValue = b64Bytes[i] - _key[keyIndex];
                if (byteValue < 0)
                {
                    if (debugLogger)
                    {
                        Log("negKey");
                        //Log($"key[{i}][{keyIndex}]: {b64Bytes[i]} - {_key[keyIndex]} = {byteValue}");
                    }
                    byteValue = 256 + byteValue;
                }
                if (debugLogger)
                    Log($"key[{i}][{keyIndex}]: {b64Bytes[i]} - {_key[keyIndex]} = {byteValue}");
                b64Bytes[i] = Convert.ToByte(byteValue);
                keyIndex++;
            }
            StartRead();
        }


        private void StartRead()
        {

            if (debugLogger)
            {
                stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                Log("ReadB64: Starting");
            }
            currentOutputString = "";
            if (debugLogger) Log("[UdonVR]Data length: " + b64Bytes.Length);
            SendCustomEventDelayedFrames(nameof(ReadBytesStep), 2);
        }

        private void Log(string text)
        {
            dataManager.pluginManager.log(text);
        }
        
        private int version;
        private int index = 0;
        private int byteIndex = 0;


        public void ReadBytesStep()
        {
            if (debugLogger) Log($"Reading {byteIndex}\n");

            if (byteIndex < b64Bytes.Length)
            {
                int endIndex = Math.Min(byteIndex + stepSize, b64Bytes.Length); // Determine the ending index of the current step

                for (index = byteIndex; index < endIndex;)
                {
                    currentOutputString += (char)(b64Bytes[index] | (b64Bytes[index + 1] << 8));
                    index += 2;
                }
                byteIndex += stepSize;
                if (byteIndex < b64Bytes.Length)
                {
                    SendCustomEventDelayedFrames(nameof(ReadBytesStep), 1);
                    return;
                }
                else
                {
                    ReadDone();
                    return;
                }
            }
        }
        private void ReadDone()
        {
            notDone = false;
            if (debugLogger)
            {
                stopwatch.Stop();
                Log($"Took: {stopwatch.ElapsedMilliseconds} ms");
            }

            if (debugLogger) Log("Reading Complete: " + currentOutputString);
            if (dataManager != null)
            {
                dataManager._ReadDone(null, currentOutputString);
            }
        }
        */
    }
}
