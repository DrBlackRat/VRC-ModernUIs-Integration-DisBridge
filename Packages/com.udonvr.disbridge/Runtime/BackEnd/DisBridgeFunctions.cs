//Copyright (c) 2021 UdonVR LLC

using System;
using System.Collections.Generic;
using UdonSharp;
using UdonVR.DisBridge.Plugins;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class DisBridgeFunctions : UdonSharpBehaviour
    {
        public static bool IsArrayNullOrEmpty(string[] myStringArray)
        {
            return myStringArray == null || myStringArray.Length < 1;
        }
        public static DataList RemoveDuplicateStrings(DataList inputList)
        {
            
            DataList seenTokens = new DataList();

            for (int i = 0; i < inputList.Count; i++)
            {
                string str = inputList[i].ToString();

                //Debug.Log($"[Test]{inputList[i].TokenType} {str}");
                // Add only if it hasn't been seen before
                if (!seenTokens.Contains(str))
                {
                    seenTokens.Add(str);
                    //Debug.Log("Add " + str);
                }
/*                else
                {
                    Debug.Log("Skip " + str);
                }*/
            }

            return seenTokens;
        }
        
        /// <summary>
        /// Custom comparison logic for Semantic Versioning (SemVer).
        /// Follows the SemVer 2.0.0 specification for comparison rules.
        /// </summary>
        /// <param name="ver1"></param>
        /// <param name="ver2"></param>
        /// <returns>
        /// -1 if ver1 is older than ver2.
        ///  0 if ver1 is the same as ver2.
        ///  1 if ver1 is newer than ver2.
        /// </returns>
        public static int CompareSemVer(string ver1, string ver2)
        {
            // Remove build metadata (e.g., +build123), as it does not affect precedence.
            string cleanVer1 = ver1.Split('+')[0];
            string cleanVer2 = ver2.Split('+')[0];

            // 1. Split into core and pre-release parts
            string[] parts1 = cleanVer1.Split('-');
            string[] parts2 = cleanVer2.Split('-');

            // 2. Compare core versions (MAJOR.MINOR.PATCH) manually, without System.Version
            string coreString1 = parts1[0];
            string coreString2 = parts2[0];

            string[] coreParts1 = coreString1.Split('.');
            string[] coreParts2 = coreString2.Split('.');

            int maxCoreLength = Math.Max(coreParts1.Length, coreParts2.Length);

            for (int i = 0; i < maxCoreLength; i++)
            {
                int num1 = 0;
                int num2 = 0;

                // Attempt to parse the current part, default to 0 if part is missing or invalid
                bool hasPart1 = i < coreParts1.Length && int.TryParse(coreParts1[i], out num1);
                bool hasPart2 = i < coreParts2.Length && int.TryParse(coreParts2[i], out num2);

                if (hasPart1 && hasPart2)
                {
                    // Both versions have this core part, compare numerically
                    int partComparison = num1.CompareTo(num2);
                    if (partComparison != 0)
                    {
                        return partComparison; // Found a difference in core versions
                    }
                }
                else if (hasPart1)
                {
                    // ver1 has more core parts than ver2 (e.g., "1.0.0" vs "1.0")
                    // If common parts are equal, the one with more parts is newer.
                    return 1;
                }
                else if (hasPart2)
                {
                    // ver2 has more core parts than ver1 (e.g., "1.0" vs "1.0.0")
                    // If common parts are equal, the one with more parts is newer.
                    return -1;
                }
                // If neither has a part, continue (implies both core versions exhausted and equal up to this point)
            }
            // If loop completes, core versions are identical.

            // 3. If core versions are equal, compare pre-release identifiers
            bool hasPreRelease1 = parts1.Length > 1;
            bool hasPreRelease2 = parts2.Length > 1;

            if (hasPreRelease1 && !hasPreRelease2)
            {
                // A version with a pre-release tag has lower precedence than a normal version
                return -1;
            }
            if (!hasPreRelease1 && hasPreRelease2)
            {
                // A normal version has higher precedence than a version with a pre-release tag
                return 1;
            }
            if (!hasPreRelease1 && !hasPreRelease2)
            {
                // Both are normal versions and equal (since core versions were equal)
                return 0;
            }

            // Both have pre-release tags, compare them lexicographically part by part
            string[] preReleaseIdentifiers1 = parts1[1].Split('.');
            string[] preReleaseIdentifiers2 = parts2[1].Split('.');

            int maxLength = Math.Max(preReleaseIdentifiers1.Length, preReleaseIdentifiers2.Length);
            for (int i = 0; i < maxLength; i++)
            {
                string id1 = i < preReleaseIdentifiers1.Length ? preReleaseIdentifiers1[i] : "";
                string id2 = i < preReleaseIdentifiers2.Length ? preReleaseIdentifiers2[i] : "";

                // Empty identifiers (due to shorter list) have lower precedence
                if (id1 == "" && id2 != "") return -1;
                if (id1 != "" && id2 == "") return 1;
                if (id1 == "" && id2 == "") continue; // Both exhausted

                bool isNumeric1 = int.TryParse(id1, out int num1);
                bool isNumeric2 = int.TryParse(id2, out int num2);

                if (isNumeric1 && isNumeric2)
                {
                    // Numeric identifiers are compared numerically
                    int numericComparison = num1.CompareTo(num2);
                    if (numericComparison != 0) return numericComparison;
                }
                else if (isNumeric1 && !isNumeric2)
                {
                    // Numeric identifiers have lower precedence than non-numeric ones
                    return -1;
                }
                else if (!isNumeric1 && isNumeric2)
                {
                    // Non-numeric identifiers have higher precedence than numeric ones
                    return 1;
                }
                else
                {
                    // Both are non-numeric, compare lexicographically (ASCII sort order)
                    int stringComparison = string.Compare(id1, id2, StringComparison.Ordinal);
                    if (stringComparison != 0) return stringComparison;
                }
            }
            return 0; // Pre-release identifiers are identical
        }
        
        #region AddToArray
        public static GameObject[] AddToArray(GameObject _val, GameObject[] _array)
        {
            GameObject[] _tmpArray = new GameObject[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new GameObject[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static DisBridgeKeypad[] AddToArray(DisBridgeKeypad _val, DisBridgeKeypad[] _array)
        {
            DisBridgeKeypad[] _tmpArray = new DisBridgeKeypad[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new DisBridgeKeypad[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static Texture[] AddToArray(Texture _val, Texture[] _array)
        {
            Texture[] _tmpArray = new Texture[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new Texture[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static RoleContainer[] AddToArray(RoleContainer _val, RoleContainer[] _array)
        {
            RoleContainer[] _tmpArray = new RoleContainer[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new RoleContainer[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static VRCPlayerApi[] AddToArray(VRCPlayerApi _val, VRCPlayerApi[] _array)
        {
            VRCPlayerApi[] _tmpArray = new VRCPlayerApi[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new VRCPlayerApi[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static Transform[] AddToArray(Transform _val, Transform[] _array)
        {
            Transform[] _tmpArray = new Transform[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new Transform[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static string[] AddToArray(string _val, string[] _array)
        {
            string[] _tmpArray = new string[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new string[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static int[] AddToArray(int _val, int[] _array)
        {
            int[] _tmpArray = new int[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new int[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static float[] AddToArray(float _val, float[] _array)
        {
            float[] _tmpArray = new float[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new float[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        public static DisBridgePlugin[] AddToArray(DisBridgePlugin _val, DisBridgePlugin[] _array)
        {
            DisBridgePlugin[] _tmpArray = new DisBridgePlugin[1];
            if (_array == null || _array.Length == 0)
            {
                _tmpArray[0] = _val;
            }
            else
            {
                _tmpArray = new DisBridgePlugin[_array.Length + 1];
                _array.CopyTo(_tmpArray,0);
                _tmpArray[_tmpArray.Length - 1] = _val;
            }
            return _tmpArray;
        }
        #endregion
        //##############################################################################################################

        #region Math

        public static float Remap(float iValue, float iMin, float iMax, float oMin, float oMax)
        {
            return Mathf.Lerp(
                oMin,
                oMax,
                Mathf.InverseLerp(
                    iMin,
                    iMax,
                    iValue
                )
            );
        }
        
        /// <summary>
        /// Converts a float value from a given range into a percentage string.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="min">The minimum possible value.</param>
        /// <param name="max">The maximum possible value.</param>
        /// <returns>A string formatted as a percentage (e.g., "75%").</returns>
        public static string FormatAsPercentage(float value, float min, float max)
        {
            // Handle the case where min and max are the same to avoid division by zero
            if (Mathf.Approximately(min, max))
            {
                return "100%";
            }

            float range = max - min;
            float valueOffset = value - min;
            float normalizedValue = valueOffset / range;

            int percentage = Mathf.RoundToInt(normalizedValue * 100f);

            return percentage + "%";
        }

        #endregion
        //##############################################################################################################
        #region Color Stuff
        
        /// <summary>
        /// Converts a Unity Color to an RRGGBB hex string.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A 6-character hex string (e.g., "FF0000").</returns>
        public static string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            
            return ByteToHex(r) + ByteToHex(g) + ByteToHex(b);
        }

        /// <summary>
        /// Helper function to convert a single integer (0-255) to a 
        /// two-character hex string (e.g., 255 -> "FF").
        /// </summary>
        private const string HEX_CHARS = "0123456789ABCDEF";
        private static string ByteToHex(int n)
        {
            n = Mathf.Clamp(n, 0, 255);
            
            int firstDigit = n / 16;
            int secondDigit = n % 16;
            
            string hexFirst = HEX_CHARS[firstDigit].ToString();
            string hexSecond = HEX_CHARS[secondDigit].ToString();

            return hexFirst + hexSecond;
        }
        /// <summary>
        /// Converts a hex color string to a Unity Color32 object.
        /// </summary>
        /// <param name="hex">The hex string (e.g., "#FF0000", "FF0000AA", "F0C").</param>
        /// <returns>A Color32 object.</returns>
        public static Color32 HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return Color.black;
            }

            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            
            if (hex.Length == 3)
                //3-digit shorthand ("F0C" -> "FF00CC")
            {
                char r = hex[0];
                char g = hex[1];
                char b = hex[2];
                hex = new string(new char[] { r, r, g, g, b, b });
            }
            
            byte rByte = 0;
            byte gByte = 0;
            byte bByte = 0;
            byte aByte = 255;

            if (hex.Length >= 6)
            {
                rByte = HexToByte(hex.Substring(0, 2));
                gByte = HexToByte(hex.Substring(2, 2));
                bByte = HexToByte(hex.Substring(4, 2));
            }

            if (hex.Length == 8)
            {
                aByte = HexToByte(hex.Substring(6, 2));
            }

            return new Color32(rByte, gByte, bByte, aByte);
        }
        private static byte HexToByte(string hexPair)
        {
            if (hexPair == null || hexPair.Length != 2) return 0;

            int firstDigit = HexToDigit(hexPair[0]);
            int secondDigit = HexToDigit(hexPair[1]);
        
            return (byte)((firstDigit * 16) + secondDigit);
        }
        private static int HexToDigit(char hexChar)
        {
            switch (hexChar)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;

                case 'a':
                case 'A': return 10;

                case 'b':
                case 'B': return 11;

                case 'c':
                case 'C': return 12;

                case 'd':
                case 'D': return 13;

                case 'e':
                case 'E': return 14;

                case 'f':
                case 'F': return 15;

                default: return 0;
            }
        }

        
        #endregion
        //##############################################################################################################
        #region Text Stuff

        public static string _SanitizeUserInput(string text)
        {
            text = text.Replace("<align=\"left\">", "").Replace("<align=\"center\">","").Replace("<align=\"right\">","").Replace("<align=\"justified\">", "").Replace("<align=\"flush\">","").Replace("</align>","");
            text = text.Replace("<font=","").Replace("</font>","");
            text = text.Replace("<voffset=","").Replace("</voffset>","");
            text = text.Replace("<line-height=","").Replace("</line-height>","");
            text = text.Replace("<cspace=","").Replace("</cspace>","");
            text = text.Replace("<indent=","").Replace("</indent>","");
            text = text.Replace("<page>",""); 
            text = text.Replace("<sprite=",""); 
            return text;
        }
        
        public static string _TrimString(string originalString, int maxLength)
        {
            if (string.IsNullOrEmpty(originalString) || originalString.Length <= maxLength)
            {
                return originalString;
            }
            return originalString.Substring(0, maxLength);
        }
        
        #endregion
        //##############################################################################################################
        #region Player Functions

        public static bool _TryGetPlayerByDisplayName(string displayName, out VRCPlayerApi player)
        {
            player = null;
            
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].displayName != displayName) continue;
                player = players[i];
                return true;
            }
            
            return false;
        }

        #endregion
        //##############################################################################################################
        #region Input Functions

        public static ScrollType _GetScrollWheel()
        {
            switch (Input.GetAxis("Mouse ScrollWheel"))
            {
                case -0.1f: return ScrollType.Backward;
                case 0.1f: return ScrollType.Forward;
            }

            return ScrollType.None;
        }
        
        #endregion
    }

    public enum ScrollType
    {
        None,
        Forward,
        Backward
    }
}