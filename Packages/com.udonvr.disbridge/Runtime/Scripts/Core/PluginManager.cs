//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using System;
using System.Collections.Generic;
using TMPro;
using UdonSharp;
using UdonVR.DisBridge.Plugins;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UdonVR.DisBridge.Editors")]

namespace UdonVR.DisBridge
{
    public enum pluginManagerState
    {
        None,
        Start,
        Init,
    }

    public enum debugLogLevel
    {
        ErrorsOnly = 0,
        EditorOnly = 1,
        RuntimeSimple = 2,
        RuntimeDetailed = 3,
    }
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)][DefaultExecutionOrder(-99999)]
    public class PluginManager : UdonSharpBehaviour
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Plugin Manager
        //##############################################################################################################
        #region Vars
        
        [Header("Key")]
        [Tooltip("Your key can be found by running `/guild-status` as an admin.")]

        public debugLogLevel debugLevel = debugLogLevel.EditorOnly;
        [SerializeField] internal string[] _key;
        [SerializeField] internal string[][] _keyFragmentArrays; // note this is a double array since List<T>.Count is not exposed. when it is than it can be changed to List<string[]>
     
        [SerializeField] internal string[] _serverNames;
        [SerializeField] internal VRCUrl[] _primaryUrl;
        [SerializeField] internal VRCUrl[] _keyUrl;

        [Header("Roles")]
        [Tooltip("This should be the parent object for your Role Containers.")]
        public Transform roles;
        [HideInInspector, SerializeField] internal RoleContainer[] _roles;
        [HideInInspector, SerializeField] internal DataDictionary _rolesDictionary;
        [HideInInspector, SerializeField] internal string[] _roleIds;
        private DataDictionary _mods;
        private string[] _modules;
        private string[] _moduleIds;

        [HideInInspector] public DisBridgeDebugger debugger;
        [HideInInspector] public bool hasDebugger = false;
        private string debuggerLog;
        
        
        private GameObject[] plugins;
        internal DisBridgePlugin[] disBridgePlugins;


        //un serialize these
        [Header("Backend")]
        [HideInInspector, SerializeField] internal string[] supporters;
        [HideInInspector, SerializeField] internal string[] staff;
        [HideInInspector, SerializeField] internal string[] boosters;
        [HideInInspector, SerializeField] internal RoleContainer boosterRole;
        [HideInInspector, SerializeField] internal RoleContainer[] supporterRoles;
        [HideInInspector, SerializeField] internal RoleContainer[] staffRoles;
        private bool hasPluginInit = false;
        private bool hasModuleInit = false;
        private int version = 4;

        private bool Init2Ran = false;

        private bool isLocalStaff = false;
        private bool isLocalSupporter = false;
        private bool isLocalBooster = false;

        private UdonBehaviour _tmp_UdonBehaviour;
        private bool _tmp_bool;
        private string _tmp_String;
        private int _tmp_int;
        private int _tmp_int2;
        private float _tmp_float;
        private RoleContainer _tem_RoleContainer;

        public DisBridgeBackend backend;

        public pluginManagerState state = pluginManagerState.None;

        #endregion
        //##############################################################################################################
        #region Is[X]

        /// <summary>
        /// Checks to see if the target player is a member of the specified role.
        /// <br></br>Takes an Index or RoleID
        /// </summary>
        /// <param name="_role">The index of the role you want to check for.</param>
        /// <param name="_player">The VRCPlayerAPI of the player you want to check.</param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsMember(int _role,VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            if (!_HasInit()) return false;
            if (hasDebugger)
            {
                if (debugger.roleOverrideCheck(_role)) return true;
            }
            return _roles[_role].IsMember(_player);
        }
        /// <summary>
        /// Checks to see if the target player is a member of the specified role.
        /// <br></br>Takes an Index or RoleID
        /// </summary>
        /// <param name="_role">The Role ID of the role you want to check for.</param>
        /// <param name="_player">The VRCPlayerAPI of the player you want to check.</param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsMember(string _role,VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            if (!_HasInit()) return false;
            _tem_RoleContainer = GetRoleById(_role);
            if (hasDebugger)
            {
                if (debugger.roleOverrideCheck(_tem_RoleContainer.transform.GetSiblingIndex())) return true;
            }
            return _tem_RoleContainer.IsMember(_player);
        }
        
        /// <summary>
        /// Checks to see if the local player is a Supporter
        /// </summary>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsSupporter()
        {
            return IsSupporter(Networking.LocalPlayer);
        }
        
        /// <summary>
        /// Checks to see if the target player is a Supporter
        /// </summary>
        /// <param name="_player">The VRCPlayerAPI of the player you want to check.</param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsSupporter(VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            if (_player.isLocal && isLocalSupporter) return true;
            
            if (supporters == null)
            {
                logWarning("Supporter array is Null, returning staff check as false!");
                return false;
            }
            if (supporters.Length == 0)
            {
                logWarning("Supporter array is Empty, returning staff check as false!");
                return false;
            }
            if (Array.IndexOf(supporters, _player.displayName) != -1) return true;
            if (hasDebugger) return debugger.supporterOverride;
            return false;
        }
        /// <summary>
        /// Checks to see if the target player is a Supporter
        /// </summary>
        /// <param name="_player">The VRCPlayerAPI of the player you want to check.</param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsSupporter(string _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            if (supporters == null)
            {
                logWarning("Supporter array is Null, returning staff check as false!");
                return false;
            }
            if (supporters.Length == 0)
            {
                logWarning("Supporter array is Empty, returning staff check as false!");
                return false;
            }
            if (Array.IndexOf(supporters, _player) != -1) return true;
            if (hasDebugger) return debugger.supporterOverride;
            return false;
        }

        /// <summary>
        /// Checks to see if the local player is a Staff member
        /// </summary>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsStaff()
        {
            return IsStaff(Networking.LocalPlayer);
        }
        
        /// <summary>
        /// Checks to see if the target player is a Staff member
        /// </summary>
        /// <param name="_player">The VRCPlayerAPI of the player you want to check.</param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsStaff(VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            if (_player.isLocal && isLocalStaff) return true;
            
            if (staff == null)
            {
                logWarning("Staff array is Null, returning staff check as false!");
                return false;
            }
            if (staff.Length == 0)
            {
                logWarning("Staff array is Empty, returning staff check as false!");
                return false;
            }
            if (Array.IndexOf(staff, _player.displayName) != -1) return true;
            if (hasDebugger) return debugger.staffOverride;
            return false;
        }
        /// <summary>
        /// Checks to see if the target player is a Staff member
        /// </summary>
        /// <param name="_player">The VRCPlayerAPI of the player you want to check.</param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsStaff(string _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            if (staff == null)
            {
                logWarning("Staff array is Null, returning staff check as false!");
                return false;
            }
            if (staff.Length == 0)
            {
                logWarning("Staff array is Empty, returning staff check as false!");
                return false;
            }
            if (Array.IndexOf(staff, _player) != -1) return true;
            if (hasDebugger) return debugger.staffOverride;
            return false;
        }

        /// <summary>
        /// Checks to see if the local player is a Server Booster
        /// </summary>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsBooster()
        {
            return isLocalBooster;
        }
        
        /// <summary>
        /// Checks to see if the target player is a Server Booster
        /// </summary>
        /// <param name="_player"></param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsBooster(VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            //if (!_HasInit())
            //{
            //    logError("Plugin Manager has not Initialized.");
            //    return false;
            //}

            if (_player.isLocal)
            {
                return isLocalBooster;
            }
            
            
            if (boosters == null)
            {
                logWarning("boosters array is Null, returning staff check as false!");
                return false;
            }
            if (boosters.Length == 0)
            {
                logWarning("boosters array is Empty, returning staff check as false!");
                return false;
            }
            if (Array.IndexOf(boosters, _player.displayName) != -1) return true;
            return false;
        }
        /// <summary>
        /// Checks to see if the target player is a Server Booster
        /// </summary>
        /// <param name="_player"></param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsBooster(string _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            //if (!_HasInit())
            //{
            //    logError("Plugin Manager has not Initialized.");
            //    return false;
            //}
            
            
            if (boosters == null)
            {
                logWarning("boosters array is Null, returning staff check as false!");
                return false;
            }
            if (boosters.Length == 0)
            {
                logWarning("boosters array is Empty, returning staff check as false!");
                return false;
            }
            if (Array.IndexOf(boosters, _player) != -1) return true;
            return false;
        }
        
        #endregion
        //##############################################################################################################
        #region Get[X]

        /// <summary>
        /// Returns a string array of player display names based of the selected Index.
        /// </summary>
        /// <param name="_role">The index of the role you want to check for.</param>
        /// <returns><b>String[]</b> – Array of player display names.</returns>
        public string[] GetRoleDisplaynames(int _roleIndex)
        {
            if (!_HasInit()) return null;
            if (_roles[_roleIndex] == null) return null;
            return _roles[_roleIndex].GetMembers();
        }

        /// <summary>
        /// Returns a string array with all of your supporter's display names.
        /// </summary>
        /// <returns><b>String[]</b> – List of ALL supporter display names</returns>
        public string[] GetSupporterDisplaynames()
        {
            if (!_HasInit()) return null;
            return supporters;
        }

        /// <summary>
        /// Returns a string array with all of your staff's display names.
        /// </summary>
        /// <returns><b>String[]</b> – List of ALL staff diaplay names</returns>
        public string[] GetStaffDisplaynames()
        {
            if (!_HasInit()) return null;
            return staff;
        }

        /// <summary>
        /// Returns a RoleContainer array with all of your staff RoleContainers.
        /// </summary>
        /// <returns><b>String[]</b> – List of ALL staff RoleContainers</returns>
        public RoleContainer[] GetStaff()
        {
            _RuntimeStart();
            return staffRoles;
        }

        /// <summary>
        /// Returns a RoleContainer array with all of your supporter RoleContainers.
        /// </summary>
        /// <returns><b>String[]</b> – List of ALL supporter RoleContainers</returns>
        public RoleContainer[] GetSupporter()
        {
            _RuntimeStart();
            return supporterRoles;
        }
        
        /// <summary>
        /// Returns a string array of player Display Names.
        /// </summary>
        /// <returns><b>String[]</b> – List of player Display Names</returns>
        public string[] GetBoosterDisplaynames()
        {
            if (!_HasInit()) return null;
            return boosters;
        }
        
        /// <summary>
        /// Returns a list of player display names based of the selected Index.
        /// </summary>
        /// <param name="_role">The index of the role you want to check for.</param>
        /// <returns><b>String[]</b> – Array of player display names.</returns>
        public RoleContainer GetRole(int _roleIndex)
        {
            _RuntimeStart();
            if (_roleIndex >= _roles.Length) return null;
            if (_roles[_roleIndex] == null) return null;
            return _roles[_roleIndex];
        }

        /// <summary>
        /// Returns a list of player display names based of the selected Index.
        /// </summary>
        /// <param name="_role">The index of the role you want to check for.</param>
        /// <returns><b>String[]</b> – Array of player display names.</returns>
        public RoleContainer GetRoleById(string _roleId)
        {
            //var i = Array.IndexOf(_roleIds, _roleId);

            for (int i = 0; i < _roles.Length; i++)
            {
                if (_roles[i].roleId == _roleId) return _roles[i];
            }
            return null;
            //if(_rolesDictionary.TryGetValue(_roleId, out var roleToken))
            //{
            //    return (RoleContainer)roleToken.Reference;
            //}
            //return null;
            ////log($"[UdonVR]GetRoleById({_roleId}) = {i}");
            //if(i == -1) return null;
            //return GetRole(i);
        }

        /// <summary>
        /// Returns a list of all the Role Containers.
        /// </summary>
        /// <returns><b>RoleContainer[]</b> – Array of RoleContainers</returns>
        public RoleContainer[] GetRoles()
        {
            _RuntimeStart();
            return _roles;
        }
        
        public RoleContainer GetPlayerRole(VRCPlayerApi _player)
        {
            for (int i = 0; i < _roles.Length; i++)
            {
                if (_roles[i].IsMember(_player)) return _roles[i];
            }
            return null;
        }
        /// <summary>
        /// Retrieves the data from the module at the specified prefix as an string value.
        /// <br/>You should only call this once cache this value if needed.
        /// </summary>
        /// <param name="modName">The name of the module to retrieve.</param>
        /// <returns><b><see cref="DataDictionary"/></b> - The dictionary of the modules or an empty dictionary if there are no modules</returns>
        public DataDictionary GetModuleDictionary()
        {
            if(_mods == null)
                _mods = new DataDictionary();
            return _mods;
        }
        /// <summary>
        /// Retrieves the data from the module at the specified prefix as an <b><see cref="DataToken"/></b>.
        /// <br/>You should only call this once cache this value if needed.
        /// </summary>
        /// <param name="modName">The name of the module to retrieve.</param>
        /// <returns><b><see cref="DataToken"/></b> - The Data Token of the module at the specified prefix</returns>
        public bool TryGetModule(string modPrefix, out DataToken _out)
        {
            if (_mods == null)
            {
                _mods = new DataDictionary();
                log("[Disbridge][mod][tgm] mods was constructed");
            }
            return _mods.TryGetValue(modPrefix, out _out);
        }
        /// <summary>
        /// Retrieves the data from the module with the specified prefix as an string value.
        /// <br/>
        /// This method should be called only once, and the result should be cached if needed.
        /// </summary>
        /// <param name="modPrefix">The prefix of the module to retrieve.</param>
        /// <param name="_out">The string value of the module data, if found.</param>
        /// <returns><b>true</b> if the module data was successfully retrieved.<br/><b>false</b> if the module failed to be retrieved.</returns>
        public bool TryGetModule(string modPrefix, ref string _out)
        {
            if(_mods == null)
            {
                _out = null; 
                return false;
            }
            if(_mods.TryGetValue(modPrefix, out var mod))
            {
                _out = mod.ToString();
                return true;
            }
            else
            {
                logError($"Failed grab module as string. {modPrefix}:{mod}");
                _out = null;
                return false;
            }

            //if (_moduleIds == null) return false;
            //_tmp_int = Array.IndexOf(_moduleIds, modPrefix);
            //if(_tmp_int == -1) return false;
            //_out = _modules[_tmp_int];
            //return true;
        }
        
        /// <summary>
        /// Retrieves the data from the module with the specified prefix as an int value.
        /// <br/>
        /// This method should be called only once, and the result should be cached if needed.
        /// </summary>
        /// <param name="modPrefix">The prefix of the module to retrieve.</param>
        /// <param name="_out">The int value of the module data, if found.</param>
        /// <returns><b>true</b> if the module data was successfully retrieved.<br/><b>false</b> if the module failed to be retrieved.</returns>

        public bool TryGetModule(string modPrefix, ref int _out)
        {

            if (_mods == null)
            {
                _out = -1;
                return false;
            }
            if (_mods.TryGetValue(modPrefix,TokenType.Int, out var mod))
            {
                _out = mod.Int;
                return true;
            }
            else
            {
                logError($"Failed grab module as int. {modPrefix}:{mod}");
                _out = -1;
                return false;
            }
/*            if (TryGetModule(modPrefix, ref _tmp_String))
            {
                if (int.TryParse(_tmp_String, out _tmp_int2))
                {
                    _out = _tmp_int2;
                    return true;
                }
                else
                {
                    logError($"Failed grab during module parse to int. Received: {modPrefix}:{_tmp_String}");
                    return false;
                }
            }
            else
            {
                return false;
            }*/
        }

        /// <summary>
        /// Retrieves the data from the module with the specified prefix as a float value.
        /// <br/>
        /// This method should be called only once, and the result should be cached if needed.
        /// </summary>
        /// <param name="modPrefix">The prefix of the module to retrieve.</param>
        /// <param name="_out">The float value of the module data, if found.</param>
        /// <returns><b>true</b> if the module data was successfully retrieved.<br/><b>false</b> if the module failed to be retrieved.</returns>

        public bool TryGetModule(string modPrefix, ref float _out)
        {
            if (_mods == null)
            {
                _out = -1;
                return false;
            }
            if (_mods.TryGetValue(modPrefix, TokenType.Float, out var mod))
            {
                _out = mod.Float;
                return true;
            }
            else
            {
                logError($"Failed grab module as float. {modPrefix}:{mod}");
                _out = -1;
                return false;
            }
/*            if (TryGetModule(modPrefix, ref _tmp_String))
            {
                if (float.TryParse(_tmp_String, out _tmp_float))
                {
                    _out = _tmp_float;
                    return true;
                }
                else
                {
                    logError($"Failed grab during module parse to float. Received: {modPrefix}:{_tmp_String}");
                    return false;
                }
            }
            else
            {
                return false;
            }*/
        }
        
        /// <summary>
        /// Retrieves the data from the module with the specified prefix as a bool value.
        /// <br/>
        /// This method should be called only once, and the result should be cached if needed.
        /// </summary>
        /// <param name="modPrefix">The prefix of the module to retrieve.</param>
        /// <param name="_out">The bool value of the module data, if found.</param>
        /// <returns><b>true</b> if the module data was successfully retrieved.<br/><b>false</b> if the module failed to be retrieved.</returns>

        public bool TryGetModule(string modPrefix, ref bool _out)
        {
            if (_mods == null)
            {
                _out = false;
                return false;
            }
            if (_mods.TryGetValue(modPrefix, TokenType.Boolean, out var mod))
            {
                _out = mod.Boolean;
                return true;
            }
            else
            {
                logError($"Failed grab module as bool. {modPrefix}:{mod}");
                _out = false;
                return false;
            }
           /* if (TryGetModule(modPrefix, ref _tmp_String))
            {
                if (bool.TryParse(_tmp_String, out _tmp_bool))
                {
                    _out = _tmp_bool;
                    return true;
                }
                else
                {
                    logError($"Failed grab during module parse to bool. Received: {modPrefix}:{_tmp_String}");
                    return false;
                }
            }
            else
            {
                return false;
            }*/
        }

        /// <summary>
        /// Retrieves the data from the module in a read to use DataDictionary.
        /// <br/>
        /// This method should be called only once, and the result should be cached if needed.
        /// </summary>
        /// <param name="modPrefix">The prefix of the module to retrieve.</param>
        /// <param name="_out">The DataDictionary for the given Module.</param>
        /// <returns><b>true</b> if the module data was successfully retrieved.<br/><b>false</b> if the module failed to be retrieved.</returns>
        public bool TryGetModuleDeserialized(string modPrefix, out DataDictionary _dataToken)
        {
            if (TryGetModule(modPrefix, out DataToken token1))
            {
                if (VRCJson.TryDeserializeFromJson(token1.ToString(), out DataToken token2))
                {
                    _dataToken = (DataDictionary)token2;
                    return true;
                }
            }
            _dataToken = new DataDictionary();
            return false;
        }
        #endregion
        //##############################################################################################################
        #region Requests
        /// <summary>
        /// Request DisBridge to fetch new Data from the servers.
        /// <br></br>This can only be done once every 5 min.
        /// </summary>
        /// <returns>True - Request was Accepted and DisBridge is now fetching new information
        /// <br></br>False - The request was within 5 min from the last requested pull and was Denied.</returns>
        public bool _RequestFetch()
        {
            return backend._RequestFetch();
        }
        #endregion
        //##############################################################################################################
        #region Internals

        public void _SendGroupContainerChange(GroupContainer __groupContainer)
        {
            if (!Utilities.IsValid(disBridgePlugins)) return;
            foreach (var __plugin in disBridgePlugins)
            {
                __plugin._UVR_GroupUpdate(__groupContainer);
            }
        }
        
        public void _RuntimeStart()
        {
            if (state != pluginManagerState.None) return;
            state = pluginManagerState.Start;

            foreach (var _role in _roles)
            {
                _role._RuntimeStart();
            }

            if (boosters == null)
            {
                logError("Boosters array is null");
            }
            else
            {
                string _name = Networking.LocalPlayer.displayName;
                foreach (var _supporter in boosters)
                {
                    if (_supporter == _name)
                    {
                        isLocalBooster = true;
                    }
                }
            }
            
            BuildStaffAndSupporterArrays();
            BuildBoosterRole();
            
            isLocalStaff = IsStaff(Networking.LocalPlayer);
            isLocalSupporter = IsSupporter(Networking.LocalPlayer);
            isLocalBooster = IsBooster(Networking.LocalPlayer);
        }

        public void _BackEndInit()
        {
            if (state == pluginManagerState.None)
            {
                _RuntimeStart();
            }
            log($"_BackEndInit", true, false);
            for (int i = 0; i < _roles.Length; i++)
            {
                log($"Checking Role {_roles[i].roleName} {_roles[i].roleId}", true, false);
                for (int k = 0; k < backend._Roles.Length; k++)
                {
                    log($"checking server [{_serverNames[k]}] with status [{backend._queueStatus[k]}]", true, false);
                    if (backend._queueStatus[k] != DecryptQueueStatus.AwaitingPull)
                    {
                        continue;
                    }
                    for (int j = 0; j < backend._Roles[k].Count; j++)
                    {
                        log($"Checking for {backend._Roles[k][j].DataDictionary["SuppRoleId"]}", true, false);
                        if (backend._Roles[k][j].DataDictionary["SuppRoleId"] == _roles[i].roleId)
                        {
                            log($"Found {backend._Roles[k][j].DataDictionary["SuppRoleId"]} adding to RC {_roles[i].roleName}", true, false);
                            _roles[i]._Init(backend._Roles[k][j].DataDictionary["Members"].DataList,this);
                            break;
                        }
                        else
                        {
                            log($"Not found {backend._Roles[k][j].DataDictionary[0]}", true, false);
                        }
                    }
                    
                }
            }

            for (int i = 0; i < _roles.Length; i++)
            {
                if (_roles[i].status == RoleContainerStatus.NeverInit)
                {
                    _roles[i]._SetRoleState(RoleContainerStatus.LocalOnly);
                    _roles[i]._Init(null,this);
                }
            }

            for (int i = 0; i < backend._Roles.Length; i++)
            {
                if (backend._queueStatus[i] == DecryptQueueStatus.AwaitingPull) backend._queueStatus[i] = DecryptQueueStatus.Finished;
            }
            BuildStaffAndSupporterArrays();
            BuildBoosterRole();
            state = pluginManagerState.Init;
            InitPlugins();
            InitActivePlayers();
        }

        private bool updatingNames = false;
        public void _QueueNameUpdate()
        {
            log("_QueueNameUpdate",true,false);
            if (updatingNames) return;
            updatingNames = true;
            SendCustomEventDelayedFrames(nameof(_UpdatePlugins),0);
        }

        /// <summary>
        /// Internal use only.
        /// </summary>
        public void _InitMods(string[] names, string[] datas)
        {
            if (hasModuleInit) return;
            hasModuleInit = true;
            _moduleIds = names;
            _modules = datas;
        }
        /// <summary>
        /// Internal use only.
        /// </summary>
        public void _InitMods(DataDictionary mods)
        {
            log($"[Disbridge][mod]Init Mods with {mods.Count} items.", false, true);
            hasModuleInit = true;
            _mods = mods;
        }
        /// <summary>
        /// Registers your plugin into the PluginManager for Initialization. Should be used with the local GameObject `gameObject`.
        /// </summary>
        /// <param name="_plugin">This should be the GameObject that your script is attached to. this is just `gameObject` in most cases.</param>
        public void AddPlugin(GameObject _plugin)
        {
            plugins = DisBridgeFunctions.AddToArray(_plugin, plugins);
            if (hasPluginInit)
            {
                _tmp_UdonBehaviour = plugins[plugins.Length -1].GetComponent<UdonBehaviour>();
                _tmp_UdonBehaviour.SendCustomEvent("_UVR_Init");
                _tmp_String = $"{_tmp_String}\n[{plugins.Length -1}]{_tmp_UdonBehaviour.gameObject.name}";
                if (hasDebugger) debugger.LoadPlugins(_tmp_String,plugins);
            }
        }

        public void AddPlugin(DisBridgePlugin _plugin)
        {
            plugins = DisBridgeFunctions.AddToArray(_plugin.gameObject, plugins);
            disBridgePlugins = DisBridgeFunctions.AddToArray(_plugin, disBridgePlugins);
            if (hasPluginInit)
            {
                _tmp_UdonBehaviour = plugins[plugins.Length -1].GetComponent<UdonBehaviour>();
                _tmp_UdonBehaviour.SendCustomEvent("_UVR_Init");
                _tmp_String = $"{_tmp_String}\n[{plugins.Length -1}]{_tmp_UdonBehaviour.gameObject.name}";
                if (hasDebugger) debugger.LoadPlugins(_tmp_String,plugins);
            }
        }


        private bool debugColor = false;
        private string debugColor1 = "#FFFFFF";
        private string debugColor2 = "#cccccc";

        private bool allowLog(bool detailedLog, bool forceLog)
        {
            #if UNITY_EDITOR
            if (debugLevel == debugLogLevel.EditorOnly) return true;
            #endif
            if (forceLog || debugLevel == debugLogLevel.RuntimeDetailed) return true;
            if (debugLevel == debugLogLevel.RuntimeSimple && !detailedLog) return true;
            return false;
        }
        
        /// <summary>
        /// Prints a debug log to the Console and to the Debugger if attached to the PluginManager.
        /// </summary>
        /// <param name="_log">The log you want printed</param>
        /// <param name="_forceLog">Forces the log to print regardless of LogLevel</param>
        public void log(string _log,bool _detailedLog = false, bool _forceLog = false)
        {
            if (!allowLog(_detailedLog,_forceLog)) return;
            
            Debug.Log($"<color=#888888>[UdonVR]</color><color=#436b8c>[DisBridge]</color>{_log}");
            if (hasDebugger)
            {
                debuggerLog = ($"{debuggerLog}\n<{(debugColor ? debugColor1 : debugColor2)}>{_log}");
                debugger.log(debuggerLog);
                debugColor = !debugColor;
            }
        }
        /// <summary>
        /// Prints a debug log Warning to the Console and to the Debugger if attached to the PluginManager.
        /// </summary>
        /// <param name="_log">The log you want printed</param>
        /// /// <param name="_forceLog">Forces the log to print regardless of LogLevel</param>
        public void logWarning(string _log,bool _detailedLog = false, bool _forceLog = false)
        {
            if (!allowLog(_detailedLog,_forceLog)) return;
            
            Debug.LogWarning($"<color=#888888>[UdonVR]</color><color=#436b8c>[DisBridge]</color>{_log}");
            if (hasDebugger)
            {
                debuggerLog = ($"{debuggerLog}\n<{(debugColor ? debugColor1 : debugColor2)}>{_log}");
                debugger.log(debuggerLog);
                debugColor = !debugColor;
            }
        }
        /// <summary>
        /// Prints a debug log Error to the Console and to the Debugger if attached to the PluginManager.
        /// </summary>
        /// <param name="_log">The log you want printed</param>
        /// /// <param name="_forceLog">Errors always log, this is for compatibility with lower log levels, and does nothing for logging Errors.</param>
        public void logError(string _log,bool detailedLog = false, bool _forceLog = false)
        {
            Debug.LogError($"<color=#888888>[UdonVR]</color><color=#436b8c>[DisBridge]</color>{_log}");
            if (hasDebugger)
            {
                debuggerLog = ($"{debuggerLog}\n<{(debugColor ? debugColor1 : debugColor2)}>{_log}");
                debugger.log(debuggerLog);
                debugColor = !debugColor;
            }
        }

        public bool _HasInit()
        {
            log($"Checking Init status [{state}]");
            switch (state)
            {
                case pluginManagerState.None:
                    return false;
                case pluginManagerState.Start:
                    return false;
                case pluginManagerState.Init:
                    return true;
            }
            return false;
        }

        
        private void InitPlugins()
        {
            log("Starting plugin initialization", false, true);
            if (hasPluginInit) return;
            if (plugins == null || plugins.Length == 0)
            {
                hasPluginInit = true;
                return;
            }
            
            _tmp_String = "";
            for (int i = 0; i < plugins.Length; i++)
            {
                _tmp_UdonBehaviour = plugins[i].GetComponent<UdonBehaviour>();
                _tmp_UdonBehaviour.SendCustomEvent("_UVR_Init");
                _tmp_String = $"{_tmp_String}\n[{i}]{_tmp_UdonBehaviour.gameObject.name}";
                log($"Sent Init to plugin [{i}] {_tmp_UdonBehaviour.name}", true, false);
            }
            if (hasDebugger) debugger.LoadPlugins(_tmp_String,plugins);
            hasPluginInit = true;
        }
        /// <summary>
        /// Internal Use Only
        /// Sends Update method to tell all of the plugins know a change has happened.
        /// </summary>
        public void _UpdatePlugins()
        {
            log("_UpdatePlugins - Sending plugin Update", false, true);
            updatingNames = false;
            if (!_HasInit())
            {
                logWarning("plugin Update requested, but plugin manager hasn't started. ignoring request", false, true);
                return;
            }
            if (!hasPluginInit)
            {
                logWarning("plugin Update requested, Plugins have not Inited, Initing plugins.", false, true);
                InitPlugins();
                return;
            }
            
            for (int i = 0; i < plugins.Length; i++)
            {
                _tmp_UdonBehaviour = plugins[i].GetComponent<UdonBehaviour>();
                _tmp_UdonBehaviour.SendCustomEvent("_UVR_Update");
                log($"Sent update to plugin [{i}] {_tmp_UdonBehaviour.name}", true, false);
            }
        }

        private void _SendPluginEvent(string _event)
        {
            for (int i = 0; i < plugins.Length; i++)
            {
                _tmp_UdonBehaviour = plugins[i].GetComponent<UdonBehaviour>();
                _tmp_UdonBehaviour.SendCustomEvent(_event);
                log($"Sent plugin event [{i}] [{_event}] {_tmp_UdonBehaviour.name}", true, false);
            }
        }

        public void _CheckVersion(int _version)
        {
            if (_version == version) return;
            
            logError("Version mismatch detected!!\nPlugins might not function because of this, please update DisBridge.");
            logError($"Version {version} expected, received version {_version}");
            UdonBehaviour _tmp;
            foreach (var _plugin in plugins)
            {
                _tmp = (UdonBehaviour)_plugin.GetComponent(typeof(UdonBehaviour));
                _tmp.SendCustomEvent("_UVR_VersionMismatch");
            }
        }
        
        internal void BuildBoosterRole()
        {
            foreach (var _role in _roles)
            {
                if (_role.IsRoleBooster())
                {
                    log($"setting {_role.name} as boosterRole");
                    boosterRole = _role;
                    break;
                }
            }
            if (boosterRole != null)
            {
                int _tmp = boosterRole.GetMembers().Length;
                boosters = new string[_tmp];
                Array.Copy(boosterRole.GetMembers(),boosters,_tmp);
            }
            else
            {
                logWarning("There is no Booster Role!");
                boosters = new string[0] {};
            }
        }
        internal void BuildStaffAndSupporterArrays()
        {
            log("Building Staff and Supporter Arrays", false, true);
            if (supporterRoles != null)
            {
                foreach (var role in supporterRoles)
                {
                    log($"Adding {role.name} to supporters", true, false);
                    supporters = role.AddMembersToArray(supporters);
                }
            }
            else
            {
                logWarning("Supporter Roles are null!", false,true);
                supporters = new string[1] {""};
            }

            if (staffRoles != null)
            {
                foreach (var role in staffRoles)
                {
                    log($"Adding {role.name} to staff", true, false);
                    staff = role.AddMembersToArray(staff);
                }
            }
            else
            {
                logWarning("Staff Roles are null!", false,true);
                staff = new string[1] {""};
            }
        }
        #endregion
        //##############################################################################################################
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region BuildChecking
        
        private DateTime _syncedBuildTime; // TODO convert to an int and sync
        public DateTime _buildTime_PC;
        public DateTime _buildTime_Quest;
        public bool pauseBuildTime = false;

        private bool hasBeenTampered = false;

        private void StartBuildCheck()
        {
            if (Networking.LocalPlayer.isMaster)
            {
                // if your the master, set the _syncedBuildTime
                _syncedBuildTime = _buildTime_PC;
                RequestSerialization();
            }
            // if you are not, do nothing. wait for network sync
        }
        public override void OnDeserialization()
        {
            if (_syncedBuildTime == DateTime.MaxValue)
            {
                OutOfSync();
                return;
            }
            if (_syncedBuildTime != _buildTime_PC)
            {
                Networking.SetOwner(Networking.LocalPlayer,gameObject);
                _syncedBuildTime = DateTime.MaxValue;
                RequestSerialization();
                OutOfSync();
                return;
            }
        }

        private void OutOfSync()
        {
            logError("!!! BUILD MISSMATCH !!! Networking may become out-of-sync");
        }

        public void _TamperCheck()
        {
            if (hasBeenTampered) return;
            logError("TAMPER CHECK FAILED -- dumping player list");
            //TODO actually create tamper check :)
        }
        
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///
        //##############################################################################################################
        #region Discard

        /// <summary>
        /// Internal use only.
        /// </summary>
        public void _Init()
        {
            Debug.Log("Building Arrays");
            if (_roles == null || _roles.Length == 0)
            {
                _roles = roles.GetComponentsInChildren<RoleContainer>();
            }

            foreach (var _role in _roles)
            {
                if (_role.IsRoleSupporter())
                {
                    supporterRoles = DisBridgeFunctions.AddToArray(_role, supporterRoles);
                }

                if (_role.IsRoleStaff())
                {
                    staffRoles = DisBridgeFunctions.AddToArray(_role, staffRoles);
                }
            }
            InitRoleIds();
            
            BuildStaffAndSupporterArrays();
            BuildBoosterRole();
        }
        /// <summary>
        /// Internal use only.
        /// </summary>
        private void InitRoleIds()
        {
            //TODO move this to a build script
            if (Init2Ran) return;
            Init2Ran = true;
            _roleIds = new string[_roles.Length];
            _rolesDictionary = new DataDictionary();
            for(int i = 0; i< _roles.Length; i++)
            {
                log($"rIndex {i}");
                if (string.IsNullOrWhiteSpace(_roles[i].roleId))
                {
                    logError($"Blank RoleID, replacing with index. role: {_roles[i].name}");
                    _roles[i].roleId = $"{i}_blank";
                }
                _rolesDictionary.Add(_roles[i].roleId, _roles[i]);
                _roleIds[i] = _roles[i].roleId;
                log($"rID[{i}]({_roles[i].name}) = ({_roles[i].roleId})");
            }
        }

        #endregion
        //##############################################################################################################

        #region Active Users

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!_HasInit()) return;
            _CheckActiveUser_Joined(player);
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!_HasInit()) return;
            _CheckActiveUser_Left(player);
        }

        private void InitActivePlayers()
        {
            VRCPlayerApi[] _players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            _players = VRCPlayerApi.GetPlayers(_players);
            foreach (var _player in _players)
            {
                _CheckActiveUser_Joined(_player);
            }
        }
        
        private void _CheckActiveUser_Joined(VRCPlayerApi _player)
        {
            string _playerName = _player.displayName;
            bool isActive = false;
            foreach (var _role in _roles)
            {
                if (_role._AddActiveUser(_playerName))
                {
                    isActive = true;
                }
            }

            if (isActive && Utilities.IsValid(disBridgePlugins))
            {
                for (int i = 0; i < disBridgePlugins.Length; i++)
                {
                    disBridgePlugins[i]._UVR_UserJoined(_player);
                }
            }
        }
        private void _CheckActiveUser_Left(VRCPlayerApi _player)
        {
            string _playerName = _player.displayName;
            bool isActive = false;
            foreach (var _role in _roles)
            {
                if (_role._RemoveActiveUser(_playerName))
                {
                    isActive = true;
                }
            }

            if (isActive && Utilities.IsValid(disBridgePlugins))
            {
                for (int i = 0; i < disBridgePlugins.Length; i++)
                {
                    disBridgePlugins[i]._UVR_UserLeft(_player);
                }
            }
        }

        #endregion
        
    }
}