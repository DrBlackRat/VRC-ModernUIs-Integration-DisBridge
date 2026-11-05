//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonVR.DisBridge
{
    public enum RoleContainerStatus
    {
        NeverInit,
        //HasChanges,
        Import,
        LocalOnly
    }
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RoleContainer : UdonSharpBehaviour
    {
        public RoleContainerStatus status  = RoleContainerStatus.NeverInit;
        
        public UdonProduct[] ceProducts;

        [SerializeField] private bool allowCustomURL = false;
        public VRCUrl customUrl;
        private string[] customUrlNames;
        
        public PluginManager disBridge;
        private ReadDataManager _readDataManager;
        [Tooltip("The Id of the Role.")]
        [SerializeField]
        public string roleId;
        [Tooltip("The name of the Role itself.\nexample: Moderators")]
        [SerializeField]
        public string roleName;
        [Tooltip("The name used to identify a single member of the role.\nexample: Moderator")]
        [SerializeField]
        public string roleAltName;

        public Sprite roleIcon;
        public Color32 roleColor = new Color32(52,152,219,255);

        [Tooltip("This allows you do add people manually to roles.\n\nAdd their VRChat username to this list\nCasE SeNSitiVE.")]
        [HideInInspector, SerializeField] internal string[] manualMembers;
        [HideInInspector, SerializeField] internal string[] members;
        [Tooltip("This allows you do add people manually to roles.\n\nAdd their VRChat username to this list\nCasE SeNSitiVE.")]
        [HideInInspector, SerializeField] internal DataList _membersList_man;
        [HideInInspector, SerializeField] internal DataList _membersList_ce;
        [HideInInspector, SerializeField] internal DataList _membersList_Import;
        [HideInInspector, SerializeField] internal DataList _membersList;
        [SerializeField] private bool isStaff = false;
        [SerializeField] private bool isSupporter = true;

        [SerializeField] private bool assignDefault;

        [Tooltip("You can only have 1 role marked as the Booster role. if you have more than one, the first one will be used as the booster role.")]
        public bool isBooster = false;

        private int tmp_arrayLegnth;

        private int emptySlots = 0;
        private string[] _rawMembers;

        private VRCPlayerApi localPlayer;

        private bool isLocalInRole;

        private bool hasCeInit;

        [SerializeField] internal bool allowUpdatesToRemoveNames = true;
        
        private DataList activeUsers = new DataList();
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Init Stuff

        private void Start()
        {
            //Debug.LogError($"[DisbridgeCE] debug ogga wooga");
        }

        public void _RuntimeStart()
        {
            localPlayer = Networking.LocalPlayer;
            if (_membersList == null) _membersList = new DataList();
            if (_membersList_ce == null) _membersList_ce = new DataList();
            if (_membersList_Import == null) _membersList_Import = new DataList();
            if (_membersList_man == null) _membersList_man = new DataList();
            if (ceProducts == null)
            {
                ceProducts = new UdonProduct[0];
            }
        }
        public override void OnPurchasesLoaded(IProduct[] products, VRCPlayerApi player)
        {
            if (player.isLocal) _CompileCeList();
        }
        
        /// <summary>
        /// Internal method for initialization. This shouldn't be ran by the end user.
        /// </summary>
        public void _Init(DataList _newMembers, PluginManager _pluginManager)
        {
            switch (status)
            {
                case RoleContainerStatus.NeverInit:
                    disBridge.log($"RoleContainer [{gameObject.name}] Received Update. status is [NeverInit]",true);
                    // initalize;
                    break;
                /*
                case RoleContainerStatus.HasChanges:
                    disBridge.log("RoleContainer [{gameObject.name}] Received Update. status is [HasChanges]",true);
                    //Update
                    return;
                    break;
                    */
                case RoleContainerStatus.Import:
                    disBridge.log($"RoleContainer [{gameObject.name}] Received Update. status is [Import]",true);
                    for (int i = 0; i < _newMembers.Count; i++)
                    {
                        AddImportName(_newMembers[i].String);
                    }
                    if (allowUpdatesToRemoveNames)
                    {
                        for (int i = 0; i < _membersList_Import.Count; i++)
                        {
                            if (!_newMembers.Contains(_membersList_Import[i]))
                            {
                                RemoveImportName(_membersList_Import[i].String);
                                i--;
                            }
                        }
                    }
                    return;
                    break;
                case RoleContainerStatus.LocalOnly:
                    disBridge.log($"RoleContainer [{gameObject.name}] Received Update. status is [LocalOnly]",true);
                    disBridge = _pluginManager;
                    if (allowCustomURL && customUrl != null)
                    {
                        VRCStringDownloader.LoadUrl(customUrl, (IUdonEventReceiver)this);
                    }
                    else
                    {
                        PostInit();
                    }
                    return;
                    break;
            }
            
            _pluginManager.log($"{gameObject.name} init");
            //Debug.LogError($"RoleContainer: {gameObject.name} Init");
            //Debug.Log($"[Test]RoleContainer: {gameObject.name} Count {_newMembers.Count}"); // was 9
            if (_newMembers != null)
                _membersList_Import = _newMembers;
            else
                _membersList_Import = new DataList();
            disBridge = _pluginManager;
            if (String.IsNullOrWhiteSpace(roleId))
            {
                disBridge.logError($"{roleName} has no RoleID, Please assign it a unique ID.");
            }
            //Debug.Log($"RoleContainer: {gameObject.name} Count {_membersList_raw.Count}"); // was 0
            if (ceProducts != null)
                // add the local user to the RoleContainer if they own the product
            {
                //Debug.LogError($"RoleContainer {gameObject.name} -- ceProducts != null");
                if (CeCheck())
                {
                    //Debug.LogError($"RoleContainer {gameObject.name} -- CeCheck == true");
                    _membersList_ce.Add(Networking.LocalPlayer.displayName);
                }
            }

            if (allowCustomURL && customUrl != null)
            {
                VRCStringDownloader.LoadUrl(customUrl, (IUdonEventReceiver)this);
            }
            else
            {
                PostInit();
            }

        }

        public void _SetRoleState(RoleContainerStatus _state)
        {
            if (status != RoleContainerStatus.NeverInit) return;
            status = _state;
        }
        private void PostInit()
        // always runs, used for fallback mode
        {
            //Debug.LogError($"RoleContainer: {gameObject.name} Init2");
            if (DisBridgeFunctions.IsArrayNullOrEmpty(_rawMembers))
            {
                //members = manualMembers;
                ArrayMembers();
                disBridge._QueueNameUpdate();
            }
            else
            {
                ArrayMembers();
                disBridge._QueueNameUpdate();
            }
            isLocalInRole = IsMember(Networking.LocalPlayer);
            _SetRoleState(RoleContainerStatus.Import);
        }

        private bool _queueArrayMembers;
        private void _QueueArrayMembers()
        {
            if (_queueArrayMembers) return;
            _queueArrayMembers = true;
            SendCustomEventDelayedFrames(nameof(_Receive_QueueArrayMembers), 1);
        }

        public void _Receive_QueueArrayMembers()
        {
            if (!_queueArrayMembers) return;
            _queueArrayMembers = false;
            ArrayMembers();
            if (disBridge != null) disBridge._QueueNameUpdate();
        }
        
        private void ArrayMembers()
        {
            //Debug.Log("[ArrayMembers] Start");
            if (_membersList_ce == null) _membersList_ce = new DataList();
            if(manualMembers != null)
            {
                _membersList_man = new DataList();
                for (int i = 0; i < manualMembers.Length; i++)
                {
                    _membersList_man.Add(manualMembers[i]);
                }
            }
            if (_membersList_Import == null) _membersList_Import = new DataList();
            _membersList = new DataList( _membersList_Import.ToArray());
            
            _membersList.AddRange(_membersList_ce);
            _membersList.AddRange(_membersList_man);
            disBridge.log($"{_membersList_Import} + {_membersList_ce.Count} + {_membersList_man.Count} = {_membersList.Count}", true, false);
            _membersList = DisBridgeFunctions.RemoveDuplicateStrings(_membersList);
            disBridge.log($"_membersList count after trim = {_membersList.Count}", true, false);
            members = new string[_membersList.Count];
            for (int i = 0; i < _membersList.Count; i++)
            {
                members[i]= _membersList[i].ToString();
                //Debug.Log($"[ArrayMembers]{i} Add {members[i]}");
            }
            isLocalInRole = _membersList.Contains(Networking.LocalPlayer.displayName);
        }

        private void AddCeName(string _name)
        {
            //_CompileCeList();
            if (_membersList_ce == null) _membersList_ce = new DataList();
            if (_membersList_man == null) _membersList_man = new DataList();
            if (_membersList_Import == null) _membersList_Import = new DataList();
            if (_membersList == null) _membersList = new DataList( _membersList_Import.ToArray());

            if (_membersList_ce.Contains(_name))
            {
                disBridge.log($"[{gameObject.name}] {_name} is already in the list", true, false);
                return;
            }
            _membersList_ce.Add(_name);
            _AddActiveUser(_name);
            disBridge.log($"[{gameObject.name}] added {_name} to the list", true, false);
            _QueueArrayMembers();
        }
        
        private void RemoveCeName(string _name)
        {
            //_CompileCeList();
            if (_membersList_ce == null) _membersList_ce = new DataList();
            if (_membersList_man == null) _membersList_man = new DataList();
            if (_membersList_Import == null) _membersList_Import = new DataList();
            if (_membersList == null) _membersList = new DataList( _membersList_Import.ToArray());

            _RemoveActiveUser(_name);
            if (_membersList_ce.Contains(_name))
            {
                _membersList_ce.Remove(_name);
                disBridge.log($"[{gameObject.name}] {_name} has been removed from the list", true, false);
                _QueueArrayMembers();
            }
        }
        
        private bool AddImportName(string _name)
        {
            //_CompileCeList();
            if (_membersList_ce == null) _membersList_ce = new DataList();
            if (_membersList_man == null) _membersList_man = new DataList();
            if (_membersList_Import == null) _membersList_Import = new DataList();
            if (_membersList == null) _membersList = new DataList( _membersList_Import.ToArray());

            if (_membersList_Import.Contains(_name))
            {
                disBridge.log($"[{gameObject.name}] {_name} is already in the list", true, false);
                return false;
            }
            _membersList_Import.Add(_name);
            disBridge.log($"[{gameObject.name}] added {_name} to the list", true, false);
            _QueueArrayMembers();
            return true;
        }
        
        private bool RemoveImportName(string _name)
        {
            //_CompileCeList();
            if (_membersList_ce == null) _membersList_ce = new DataList();
            if (_membersList_man == null) _membersList_man = new DataList();
            if (_membersList_Import == null) _membersList_Import = new DataList();
            if (_membersList == null) _membersList = new DataList( _membersList_Import.ToArray());

            if (_membersList_Import.Contains(_name))
            {
                _membersList_Import.Remove(_name);
                disBridge.log($"[{gameObject.name}] {_name} has been removed from the list", true, false);
                _QueueArrayMembers();
                return true;
            }
            return false;
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            PostInit();
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            string[] _seperators = new string[3] { "\r\n", "\r", "\n" };
            customUrlNames = result.Result.Split(_seperators, System.StringSplitOptions.RemoveEmptyEntries);
            if (customUrlNames == null)
            {
                PostInit();
                return;
            }
            if (customUrlNames.Length == 1 && String.IsNullOrEmpty(customUrlNames[0]))
            {
                disBridge.logWarning($"Role {gameObject.name} customUrlNames is empty");
                PostInit();
                return;
            }
            
            
            emptySlots = 0; // reset empty slots
        
            foreach (var _member in manualMembers)
                // counting the empty slots
            {
                if (Array.IndexOf(customUrlNames, _member) == -1 && !string.IsNullOrEmpty(_member))
                {
                }
                else // is a duplicate, dont add and mark as an empty slot 
                {
                    emptySlots++;
                }
            }
        
            members = new string[(manualMembers.Length + customUrlNames.Length) - emptySlots]; // combine the array lengths
            Array.Copy(customUrlNames,members,customUrlNames.Length); // copy the old array into the new one, adding empty slots for all indexes
            int i = customUrlNames.Length; // start the member count at the end of the old array
            foreach (var _member in manualMembers)
                // combine the arrays skipping 
            {
                if (Array.IndexOf(customUrlNames, _member) == -1 && !string.IsNullOrEmpty(_member)) // if not a duplicate, add to array
                {
                    members[i] = _member;
                    i++;
                }
            }
            manualMembers = new string[members.Length];
            for (i = 0; i < members.Length; i++)
            {
                manualMembers[i] = members[i];
            }
            PostInit();
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Get

        /// <summary>
        /// Returns a string array with all the members in this role.
        /// </summary>
        /// <returns><b>String[]</b> – Array of player display names</returns>
        public string[] GetMembers()
        {
            return members;
        }
        
        /// <summary>
        /// Returns how many members are in the role.
        /// </summary>
        /// <returns><b>int</b> – number of members</returns>
        public int GetMemberCount()
        {
            if (members == null) return 0;
            return members.Length;
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Is
        
        /// <summary>
        /// Used to check if the local player is a member of this role.
        /// </summary>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsMember()
        {
            return isLocalInRole;
        }

        /// <summary>
        /// Used to check if the player is a member of this role.
        /// </summary>
        /// <param name="_player"> The player you're wanting to check.</param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsMember(VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(_player)) return false;
            if (assignDefault)
            {
                return true;
            }

            /*if (hasInit == false)
            {
                if (disBridge == null)
                {
                    Debug.LogError($"{roleName} Access during Init. returning False");
                }
                else
                {
                    disBridge.logError($"{roleName} Access during Init. returning False");
                }
                return false;
            }*/
            
            for (int i = 0; i < ceProducts.Length; i++)
            {
                if (ceProducts[i] == null) continue;
                if (Store.DoesPlayerOwnProduct(_player, ceProducts[i])) return true;
            }
            string _name = _player.displayName;
            return _membersList.Contains( _name );
        }
        
        /// <summary>
        /// Used to check if the player is a member of this role.
        /// </summary>
        /// <param name="_player"> The player you're wanting to check.</param>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsMember(string _player)
        {
            if (assignDefault)
            {
                return true;
            }
            return _membersList.Contains( _player );

            //VRCPlayerApi __player = null;
            //if (PlayerStringCheck(_player, ref __player))
            //{
            //    for (int i = 0; i < ceProducts.Length; i++)
            //    {
            //        if (ceProducts[i] == null) continue;
            //        if (Store.DoesPlayerOwnProduct(__player, ceProducts[i])) return true;
            //    }
            //    string _name = __player.displayName;
            //    return _membersList.Contains( _name );
            //}
            //else
            //{
            //    return false;
            //}
        }

        private bool PlayerStringCheck(string _playerName, ref VRCPlayerApi _player)
        {
            VRCPlayerApi[] _players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(_players);
            for (int i = 0; i < _players.Length; i++)
            {
                if (Utilities.IsValid(_players[i]) && _players[i].displayName == _playerName)
                {
                    _player = _players[i];
                    return true;
                }
                
            }
            return false;
        }

        /// <summary>
        /// returns if the role is a Staff role.
        /// </summary>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsRoleStaff()
        {
            return isStaff;
        }

        /// <summary>
        /// returns if the role is your Booster role.
        /// <br></br><b>!!YOU CAN ONLY HAVE ONE OF THESE!!</b>
        /// </summary>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsRoleBooster()
        {
            return isBooster;
        }

        /// <summary>
        /// returns if the role is a Supporter role.
        /// </summary>
        /// <returns><b>bool</b> – True/False</returns>
        public bool IsRoleSupporter()
        {
            return isSupporter;
        }
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Add

                /// <summary>
        /// Adds the members to bottom of the target string array and returns the merged string array.
        /// </summary>
        /// <param name="_array">Array you want to add members to.</param>
        /// <returns><b>String[]</b> – Merged array of player display names.</returns>
        public string[] AddMembersToArray(string[] _array)
        {
            //Debug.Log($"{nameof(AddMembersToArray)} on {gameObject.name}");
            //Debug.LogError("_array Start");

            if (_array == null || _array.Length == 0) // if array is null or empty, return members
            {
               return members;
            }
            else if (_array.Length == 1 && String.IsNullOrEmpty(_array[0])) // if the first slot in the array is empty, return members
            {
                Debug.LogWarning($"Role {gameObject.name} _array is empty, retuning members.");
                return members;
            }

            emptySlots = 0;
            if (members == null)
            {
                members = new string[0];
            }
            else
            {
                foreach (var _member in members)
                {
                    if (Array.IndexOf(_array, _member) == -1 && !string.IsNullOrEmpty(_member)) // if not a duplicate, add to array
                    {
                        
                    }
                    else // is a duplicate, dont add and mark as an empty slot 
                    {
                        emptySlots++;
                    }
                }
            }


            string[] _tmpArray;
            _tmpArray = new string[(members.Length + _array.Length) - emptySlots]; // combine the array lengths
            Array.Copy(_array,_tmpArray,_array.Length); // copy the old array into the new one, adding empty slots for all indexes
            int i = _array.Length; // start the member count at the end of the old array
            foreach (var _member in members)
            {
                if (Array.IndexOf(_tmpArray, _member) == -1 && !string.IsNullOrEmpty(_member)) // if not a duplicate, add to array
                {
                    //Debug.LogWarning($"{_member} is not a duplicate, adding");
                    _tmpArray[i] = _member;
                    i++;
                }
            }
            
            return _tmpArray;
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region CE

        private bool CeCheck()
        {
            //Debug.LogError($"RoleContainer: {gameObject.name} -- CeCheck");
            foreach (var product in ceProducts)
            {
                if (product == null) continue;
                if (Store.DoesPlayerOwnProduct(localPlayer, product)) return true;
            }
            //Debug.LogError($"RoleContainer: {gameObject.name} -- CeCheck false");
            return false;
        }
        private void _CompileCeList()
        {
            if (_membersList_ce == null) _membersList_ce = new DataList();
            if (hasCeInit) return;
            hasCeInit = true;
            //VRCPlayerApi[] _players = new VRCPlayerApi[0];
            foreach (var VARIABLE in ceProducts)
            {
                Store.ListProductOwners((IUdonEventReceiver)this,VARIABLE);
            }
        }
        public override void OnPurchaseConfirmed(IProduct product, VRCPlayerApi player, bool purchasedNow)
        {
            for (int i = 0; i < ceProducts.Length; i++)
            {
                if (product.ID == ceProducts[i].ID)
                {
                    AddCeName(player.displayName);
                    return;
                }
            }
        }
        public override void OnPurchaseExpired(IProduct product, VRCPlayerApi player)
        {
            for (int i = 0; i < ceProducts.Length; i++)
            {
                if (product.ID == ceProducts[i].ID)
                {
                    RemoveCeName(player.displayName);
                    return;
                }
            }
        }
        public override void OnListProductOwners(IProduct product, string[] owners)
        {
            if (_membersList_ce == null) _membersList_ce = new DataList();
            for (int i = 0; i < owners.Length; i++)
            {
                AddCeName(owners[i]);
            }
        }
        public override void OnListPurchases(IProduct[] products, VRCPlayerApi player)
        {
            return;
            if (disBridge != null)
            {
                disBridge.log($"[DisbridgeCE] OnListPurchases player: {player.displayName}");
            }
            else
            {
                Debug.Log($"[DisbridgeCE] OnListPurchases player: {player.displayName}");
            }
            
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Active Users

        public DataList _GetActiveUsers()
        {
            return activeUsers.DeepClone();
        }
        
        public bool _AddActiveUser(string _name)
        {
            if (activeUsers.Contains(_name))
            {
                return false;
            }
            activeUsers.Add(_name);
            return true;
        }

        public bool _RemoveActiveUser(string _name)
        {
            if (activeUsers.Contains(_name))
            {
                activeUsers.Remove(_name);
                return true;
            }
            return false;
        }
        
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region UNITY_EDITOR
    #if UNITY_EDITOR

        public bool IsStaff
        {
            get { return isStaff; }
            set { isStaff = value; }
        }
        public bool IsSupporter
        {
            get { return isSupporter; }
            set { isSupporter = value; }
        }
        
        public bool AllowCustomURL
        {
            get { return allowCustomURL; }
            set { allowCustomURL = value; }
        }
        
        /// <summary>
        /// Returs a string array with ONLY the manual members added at editor time.
        /// </summary>
        /// <returns><b>String[]</b> – Array of player display names manually added by the user (Editor Only)</returns>
        public string[] GetManuals()
        {
            return manualMembers;
        }

        /// <summary>
        /// Sets the IsStaff bool
        /// </summary>

        public void SetStaff(bool tf)
        {
            isStaff = tf;
        }

        /// <summary>
        /// Sets the isSupporter bool
        /// </summary>

        public void SetisSupporter(bool tf)
        {
            isSupporter = tf;
        }

        /// <summary>
        /// Initialize string array with a int value of x
        /// </summary>

        public void SetManual(int x)
        {
            manualMembers = new string[x];
        }

        /// <summary>
        /// Sets the value of manualMembers[i] with a string input;
        /// </summary>

        public void SetManualMember(int i, string input)
        {
            manualMembers[i] = input;
        }
    #endif
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        

        
    }

}
