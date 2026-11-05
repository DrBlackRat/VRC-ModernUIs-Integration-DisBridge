//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UdonVR.DisBridge")]

namespace UdonVR.DisBridge
{
    public enum DecryptQueueStatus
    {
        NeverRan,
        Waiting,
        Running,
        AwaitingPull,
        Finished,
    }
    public class DisBridgeBackend : UdonSharpBehaviour
    {
        public PluginManager disBridge;

        public TextMeshPro debugText;
        public bool debugMode = false;
        private bool fallbackMode = false;
        
        private int _disbridgeVersion = 4;
        
        [SerializeField] internal float updateRate = 30f;
        private DateTime _lastUpdate;
        private DateTime _nextAllowedManualSync;
        private DateTime _lastAutomatedSync;
        
        public TextMeshProUGUI lastUpdateText;
        public Button syncButton;
        
        public VersionChecker versionChecker;
        
        void Start()
        {
            _lastAutomatedSync = DateTime.Now;
            Log(nameof(Start));
            //disBridge._RuntimeStart();
            _BackEndInit();
            _StartQueue();
        }

        public void _BackEndInit()
        {
            if (!TamperChecks("Start")) return;
            int serverAmount = disBridge._keyUrl.Length;
            _untrustedFailed = new bool[serverAmount];
            _retryUntrusted = new int[serverAmount];
            _queue = new string[serverAmount];
            _queueStatus = new DecryptQueueStatus[serverAmount];
            _version = new int[serverAmount];

            _Moduals = new DataDictionary[serverAmount];
            _Roles = new DataList[serverAmount];
            
            _curUrl = -1;
            queueRCUpdate = false;
                

            for (int i = 0; i < _queueStatus.Length; i++)
            {
                _queueStatus[i] = DecryptQueueStatus.NeverRan;
            }

            if (disBridge._keyUrl == null || disBridge._keyUrl.Length == 0)
            {
                disBridge.logWarning("DisBridge initializing, skipping URLs. Queueing start request.");
                SendCustomEventDelayedSeconds(nameof(_OfflineStart), 5f);
                return;
            }
            __NextUrl();
        }

        public void _RefreshNames()
        {
            _BackEndInit();
            _StartQueue();
        }

        public void _OfflineStart()
        {
            disBridge.log("Running OfflineStart");
            disBridge._BackEndInit();
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region UpdateLoop

        private void _StartQueue()
        {
            _lastUpdate = DateTime.Now.AddMinutes(updateRate);
            _nextAllowedManualSync = DateTime.Now.AddMinutes(5);
            if (Utilities.IsValid(lastUpdateText))lastUpdateText.text = $"Last Sync: {DateTime.Now.ToString("hh:mmtt MM/dd")}";
            if (Utilities.IsValid(syncButton))syncButton.interactable = false;
            
            if (queueCheckLoop) return;
            queueCheckLoop = true;
            _QueueCheckLoop();
        }

        private bool queueCheckLoop = false;
        public void _QueueCheckLoop()
        {
            if (!queueCheckLoop) return;
            if (_lastUpdate <= DateTime.Now)
            {
                _RefreshNames();
            }
            if (_nextAllowedManualSync <= DateTime.Now)
            {
                if (Utilities.IsValid(syncButton))syncButton.interactable = true;
            }
            SendCustomEventDelayedSeconds(nameof(_QueueCheckLoop),60f);
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region StringLoading

        public int retryUrlCount = 3;
        private bool[] _untrustedFailed;
        private int[] _retryUntrusted;
        private VRCUrl _url;
        private int _curUrl = -1;
        
        private VRCUrl __GetUrl(int __url = 0)
        {
            Log($"__GetUrl running with [{__url}]");
            if (_untrustedFailed[__url])
            {
                return disBridge._keyUrl[__url];
            }
            if (!string.IsNullOrWhiteSpace(disBridge._primaryUrl[__url].ToString()))
            {
                if (_retryUntrusted[__url] >= retryUrlCount)
                {
                    _untrustedFailed[__url] = true;
                    return disBridge._keyUrl[__url];
                }
                _retryUntrusted[__url]++;
                return disBridge._primaryUrl[__url];
            }
            return disBridge._keyUrl[__url];
        }
        
        private void __StartDownload(int __url = 0)
            // used to start a new download
        {
            Log(nameof(__StartDownload));
            _untrustedFailed[__url] = false;
            _retryUntrusted[__url] = 0;
            __TryDownload(__url);
        }

        private void __TryDownload(int __url = 0)
            // try a download
        {
            Log(nameof(__TryDownload));
            VRCStringDownloader.LoadUrl(__GetUrl(__url), (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            if (result == null || !TamperChecks("OnStringLoadSuccess")) return;
            Log($"OnStringLoadSuccess {_curUrl}");
            __AddToDecryptQueue(result.Result,_curUrl);
            __NextUrl();
        }
        
        public override void OnStringLoadError(IVRCStringDownload result)
        {
            if (result == null || !TamperChecks("OnStringLoadError")) return;
            Log($"OnStringLoadError -- [{result.ErrorCode}] {result.Error}");
            if (debugText != null) debugText.text = $"[{result.ErrorCode}] {result.Error}";
            if (result.Url == disBridge._primaryUrl[0])
            {
                if (result.ErrorCode == 401)
                {
                    _untrustedFailed[_curUrl] = true;
                    __NextUrl();
                }
            }
            SendCustomEventDelayedSeconds(nameof(__TryDownload),5f);
        }

        private void __NextUrl()
        {
            Log(nameof(__NextUrl));
            _curUrl++;
            if (_curUrl == -1 || _curUrl >= disBridge._keyUrl.Length)
            {
                disBridge.logWarning($"{_curUrl} is not a valid url, returning. This is probably normal.");
                return;
            }
            __StartDownload(_curUrl);
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Decrypt

        private int _byteValue = 0;
        private byte[] _b64Bytes;
        private string _currentWorkingString;
        [SerializeField] private int[] _version;
        private int _byteIndex = 0;
        [Tooltip("Increasing step size decreases decode time but increases frametimes")]
        [SerializeField] private int stepSize = 200;

        private string[] _queue;
        //private string[] _queueResults;
        public DecryptQueueStatus[] _queueStatus;
        private bool _isDecrypt;
        private int _whatKeyIsDecrypting;

        private void __AddToDecryptQueue(string _text, int _index)
        {
            Log(nameof(__AddToDecryptQueue));
            if (!TamperChecks("__AddToDecryptQueue")) return;
            if (_index >= _queue.Length || _index <= -1) return;
            _queue[_index] = _text;
            _queueStatus[_index] = DecryptQueueStatus.Waiting;
            __CheckDecryptQueue();
        }

        private void __CheckDecryptQueue()
        {
            Log(nameof(__CheckDecryptQueue));
            if (_isDecrypt || !TamperChecks("__CheckDecryptQueue")) return;
            for (int i = 0; i < _queueStatus.Length; i++)
            {
                if (_queueStatus[i] == DecryptQueueStatus.Waiting)
                {
                    __Decrypt(_queue[i], i);
                    return;
                }
            }
        }

        private void __Decrypt(string _text,int _index)
        {
            Log(nameof(__Decrypt));
            if (!TamperChecks("__Decrypt")) return;
            _isDecrypt = true;
            _queueStatus[_index] = DecryptQueueStatus.Running;
            _whatKeyIsDecrypting = _index;
            //Decrypting

            byte[] _key = __ToByteArray(string.Join("", this.disBridge._keyFragmentArrays[_whatKeyIsDecrypting]));
            
            string[] parts = _text.Split('\n');
            _version[_whatKeyIsDecrypting] = Convert.ToInt32(parts[0]);
            _text = parts[1];
            if (string.IsNullOrEmpty(_text) || _text.Length % 4 > 0)
            {
                return;// null;
            }
            _b64Bytes = Convert.FromBase64String(_text);
            int keyIndex = 0;
            for (int i = 0; i < _b64Bytes.Length; i++)
            {
                if (keyIndex == _key.Length)
                {
                    keyIndex = 0;
                }
                _byteValue = _b64Bytes[i] - _key[keyIndex];
                if (_byteValue < 0)
                {
                    _byteValue = 256 + _byteValue;
                }
                _b64Bytes[i] = Convert.ToByte(_byteValue);
                keyIndex++;
            }
            _currentWorkingString = "";
            _byteIndex = 0;
            SendCustomEventDelayedFrames(nameof(_ReadBytesStep), 2);
        }

        public void _ReadBytesStep()
        {
            //Log(nameof(_ReadBytesStep));
            if (_byteIndex < _b64Bytes.Length)
            {
                int endIndex = Math.Min(_byteIndex + stepSize, _b64Bytes.Length); // Determine the ending index of the current step

                for (int i = _byteIndex; i < endIndex;)
                {
                    _currentWorkingString += (char)(_b64Bytes[i] | (_b64Bytes[i + 1] << 8));
                    i += 2;
                }
                _byteIndex += stepSize;
                if (_byteIndex < _b64Bytes.Length)
                {
                    SendCustomEventDelayedFrames(nameof(_ReadBytesStep), 1);
                    return;
                }
                else
                {
                    __ReadDone();
                    return;
                }
            }
        }
        
        private void __ReadDone()
        {
            Log(nameof(__ReadDone));
            //_queueResults[_whatKeyIsDecrypting] = _currentWorkingString;
            __WriteDataDictionary(_currentWorkingString,_whatKeyIsDecrypting);
            _isDecrypt = false;
            _queueStatus[_whatKeyIsDecrypting] = (_queueStatus[_whatKeyIsDecrypting] == DecryptQueueStatus.Waiting) ? DecryptQueueStatus.Waiting : DecryptQueueStatus.AwaitingPull;
            __CheckDecryptQueue();
            if (__IsQueueReady())
            {
                _CompileMods();
                __QueueRCUpdate();
            }
        }

        private void __WriteDataDictionary(string _string, int _index)
        {
            Log($"nameof(__WriteDataDictionary): {_string}, {_index}");
            if (VRCJson.TryDeserializeFromJson(_string, out DataToken result))
            {
                // Deserialization succeeded! Let's figure out what we've got.
                if (result.TokenType == TokenType.DataDictionary)
                {
                    Log($"Successfully deserialized as a dictionary with {result.DataDictionary.Count} items.");
                    if (result.DataDictionary.TryGetValue("Mods", TokenType.DataDictionary, out DataToken mods))
                    {
                        _Moduals[_index] = mods.DataDictionary;
                    }
                    else
                    {
                        LogActualError($"Failed to get mods dictionary - {mods}");
                    }
                    if (result.DataDictionary.TryGetValue("Roles",TokenType.DataList, out DataToken roles))
                    {
                        _Roles[_index] = roles.DataList;
                        //VRCJson.TrySerializeToJson(roles, JsonExportType.Minify, out DataToken idc);
                        //Log($"Getting roles: {idc}");
                        //for (int i = 0; i < _Roles[_index].Count; i++)
                        //{
                        //    //Log($"{_Roles[_index][i].DataDictionary.Count}");
                        //    //Log($"{_Roles[_index][i].DataDictionary["SuppRoleId"]}");
                        //    //Log($"{_Roles[_index][i].DataDictionary["Members"]}");
                        //}
                    }
                    else
                    {
                        LogActualError($"Failed to get roles list - {roles}");
                    }
                }
                else if (result.TokenType == TokenType.DataList)
                {
                    Log($"Successfully deserialized as a list with {result.DataList.Count} items.");
                }
                else
                {
                    LogActualError("You should never see this. Your Key did not Deserialize correctly.");
                    LogActualError($"Please bring this to an Admin of UdonVR:\nDisbridge version: {_disbridgeVersion} | Key version: {_version[_index]}");
                }
            }
            else
            {
                LogActualError("You should never see this. Your Key did not Decrypt correctly.");
                LogActualError($"Please bring this to an Admin of UdonVR:\nDisbridge version: {_disbridgeVersion} | Key version: {_version[_index]}");
            }
        }

        private bool __IsQueueReady()
        {
            Log(nameof(__IsQueueReady));
            for (int i = 0; i < _queueStatus.Length; i++)
            {
                switch (_queueStatus[i])
                {
                    case DecryptQueueStatus.NeverRan: return false;
                    case DecryptQueueStatus.Waiting: return false;
                    case DecryptQueueStatus.Running: return false;
                    case DecryptQueueStatus.AwaitingPull: continue;
                }
            }
            Log($"{nameof(__IsQueueReady)} Ready");
            return true;
        }
        
        private byte[] __ToByteArray(string text)
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

        private void _CheckVersion(int _version, int _index)
        {
            
            if (_version > _disbridgeVersion)
            {
                LogActualError($"Version mismatch detected on [{_index}]{disBridge._serverNames[_index]}!!\nPlugins might not function because of this. Please update your DisBridge package.");
                LogActualError($"Version {_disbridgeVersion} expected, received version {_version}");
                return;
            }
            if (_version < _disbridgeVersion)
            {
                LogActualError($"Version mismatch detected on [{_index}]{disBridge._serverNames[_index]}!!\nPlugins might not function because of this.");
                LogActualError("It seems your server is running a lower version than expected. Please contact an admin in UdonVR to get your server upgraded.");
                LogActualError($"Version {_disbridgeVersion} expected, received version {_version}");
            }
            
        }

        private void _CompileMods()
        {
            DataDictionary mods = new DataDictionary();
            mods = _Moduals[0];
            for (int i = 1; i < _Moduals.Length; i++)
            {
                for (int j = 0; j < _Moduals[i].Count; j++)
                {
                    VRCJson.TrySerializeToJson(_Moduals[i],JsonExportType.Beautify,out DataToken _token);
                    //Log($"Deserlize mods from server {i} as {_token}");
                    
                    DataList _tmp = _Moduals[i].GetKeys();
                    DataList _vars = _Moduals[i].GetValues();
                    for (int k = 0; k < _tmp.Count; k++)
                    {
                        //Log($"found {_tmp[k]} {_vars[k]}");
                        if (!mods.ContainsKey(_tmp[k]))
                        {
                            //Log($"Adding mod {_tmp[k]} {_vars[k]}");
                            mods.Add(_tmp[k],_vars[k]);
                        }
                    }
                }
            }
            disBridge._InitMods(mods);
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Send to RoleContainers

        private bool queueRCUpdate = false;
        public DataDictionary[] _Moduals;
        public DataList[] _Roles;

        private void __QueueRCUpdate()
        {
            Log("__QueueRCUpdate");
            if (queueRCUpdate)
            {
                Log("queueRCUpdate is True");
                return;
            }
            queueRCUpdate = true;
            for (int i = 0; i < _queueStatus.Length; i++)
            {
                if (_queueStatus[i] != DecryptQueueStatus.AwaitingPull) continue;

                disBridge._BackEndInit();

                //TODO loop over all the RoleContainers using each role provided in _Roles, when a match is found, insert role names and break to the next RoleContainer;
                _queueStatus[i] = DecryptQueueStatus.Finished;
            }
            if (Utilities.IsValid(versionChecker))versionChecker._StartCheck();
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Logging
        private string _logPrefix = "<color=#436b8c>[Backend]</color>";
        private void Log(string _log)
        {
            if (debugMode)
            {
                if (disBridge == null) return;
                disBridge.log($"{_logPrefix}{_log}");
            }
        }
        private void LogWarning(string _log)
        {
            if (debugMode)
            {
                if (disBridge == null) return;
                disBridge.logWarning($"{_logPrefix}{_log}");
            }
        }
        private void LogError(string _log)
        {
            if (debugMode)
            {
                if (disBridge == null) return;
                disBridge.logError($"{_logPrefix}{_log}");
            }
        }
        private void LogActualWarning(string _log)
        {
            if (disBridge != null)
            { disBridge.logWarning($"{_logPrefix} ! Warning ! {_log}"); }
            else
            { Debug.LogWarning($"{_logPrefix} ! Warning ! {_log}"); }
        }
        private void LogActualError(string _log)
        {
            if (disBridge != null)
            { disBridge.logError($"{_logPrefix} !!! SOMETHING BROKE !!! {_log}"); }
            else
            { Debug.LogError($"{_logPrefix} !!! SOMETHING BROKE !!! {_log}"); }
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Automated Requests

        /// <summary>
        /// Request DisBridge to fetch new Data from the servers.
        /// <br></br>This can only be done once every 5 min.
        /// </summary>
        /// <returns>True - Request was Accepted and DisBridge is now fetching new information
        /// <br></br>False - The request was within 5 min from the last requested pull and was Denied.</returns>
        public bool _RequestFetch()
        {
            TimeSpan timeSinceLastAutomatedSync = DateTime.Now - _lastAutomatedSync;

            if (timeSinceLastAutomatedSync > TimeSpan.FromMinutes(5))
            {
                _lastAutomatedSync =  DateTime.Now;
                _StartQueue();
                return true;
            }
            return false;
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool TamperChecks(string _log)
        {
            if (disBridge == null || disBridge._keyFragmentArrays == null)
            {
                LogActualError($"Tamper check failed at {_log} -- disBridge is null");
                return false;
            }
            for (int i = 0; i < _queueStatus.Length; i++)
            {
                if ((int)_queueStatus[i] <= -1 || (int)_queueStatus[i] >= 5)
                {
                    LogActualError($"Tamper check failed at {_log} -- _queueStatus outside of natural bounds [{i}][{(int)_queueStatus[i]}]");
                    return false;
                }
            }
            return true;
        }
    }
}

