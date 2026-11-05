//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Drawing;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.Animations;
using Image = UnityEngine.UI.Image;

namespace UdonVr.DisBridge.Utils
{
    public class NotificationSystem : UdonSharpBehaviour
    {
        public float fadeDuration = 0.2f;
        public float notificationDuration = 3f;
        public float notificationBuffer = 0f;
        private bool isVR = false;
        public Transform notificationDisplay;
        public Transform vrHead;


        #region UI Refrerences

        public CanvasGroup canvasGroup;
        public Image iconImage;
        public TextMeshProUGUI notificationHeader;
        public TextMeshProUGUI notificationText;
        public TextMeshProUGUI notificationTextAlt;

        #endregion
        
        public NotificationSystem_Container[] notificationContainers;

        public Sprite notificationIcon;

        float timeElapsed = 0f;
        bool isFading = false;
        private bool fadeType;

        private int _notificationCount = 0; // the last sent notification
        private int _nextNotification = 0; // current array end for notificationContainers

        public AudioSource audioSource;
        public float volume = .25f;
        public AudioClip genericSound;
        public AudioClip successSound;
        public AudioClip failSound;
        
        private VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            isVR = localPlayer.IsUserInVR();
            canvasGroup.alpha = 0f;
            //_PositionUIElement();
        }

        public void _AssignNewDefaultIcon(Texture2D icon)
        {
            notificationIcon = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero);
        }

        public void _RegisterNotification(string notification, string altText = "", string _headerText = "", AudioClip _sound = null, Sprite image = null)
        {
            int _startIndex = _nextNotification;
            // Basic error checking to prevent issues.
            if (notificationContainers == null)
            {
                Debug.LogError("Array is not initialized or is empty.");
                return; // exit FAIL
            }

            if (_startIndex < 0 || _startIndex >= notificationContainers.Length)
            {
                Debug.LogError($"Invalid start index: {_startIndex}. It must be between 0 and {notificationContainers.Length - 1}.");
                return; // exit FAIL
            }

            // We loop exactly 'dataArray.Length' times to hit every element once.
            for (int i = 0; i < notificationContainers.Length; i++)
            {
                // This is the magic formula.
                // It calculates the current position in the array, wrapping around if necessary.
                int currentIndex = (_startIndex + i) % notificationContainers.Length;
                if (notificationContainers[currentIndex].isLoaded == false)
                {
                    if (notificationContainers[currentIndex]._LoadNotification(notification, altText,_headerText, _sound, image))
                    {
                        _nextNotification = (_nextNotification + 1) % notificationContainers.Length;
                        //_nextNotification++;
                        //if (_nextNotification >= notificationContainers.Length) _nextNotification = 0;
                        _StartLoop();
                        return; // exit PASS
                    }
                }
            }

            Debug.Log("Loop finished.");
            return; // exit FAIL
        }

        public void _SendInstantNotification(string notification, string altText = "", string _headerText = "", AudioClip _clip = null, Sprite image = null)
        {
            _PositionUIElement();
            canvasGroup.alpha = 0f;
            notificationText.text = notification;
            notificationTextAlt.text = altText;
            notificationHeader.text = string.IsNullOrWhiteSpace(_headerText)? "DisBridge": $"DisBridge - {_headerText}";
            iconImage.sprite = Utilities.IsValid(image)?(image):(notificationIcon);
            //notificationMaterial.mainTexture = Utilities.IsValid(image)?(image):(notificationIcon);

            if (_clip != null && audioSource != null)
            {
                audioSource.volume = 0;
                audioSource.Stop();
                audioSource.clip = _clip;
                audioSource.Play();
                audioSource.volume = volume;
            }

            _StartFadeIn();
            SendCustomEventDelayedSeconds(nameof(_StartFadeOut), fadeDuration + notificationDuration);
        }

        public void _StartFadeIn()
        {
            __StartFade(true);
        }

        public void _StartFadeOut()
        {
            __StartFade(false);
        }

        private void __StartFade(bool fadeIn = true)
        {
            if (isFading) return;
            timeElapsed = 0f;
            isFading = true;
            fadeType = fadeIn;
            _FadeLoop();
        }

        public void _FadeLoop()
        {
            if (!isFading) return;
            if (timeElapsed >= fadeDuration)
            {
                canvasGroup.alpha = fadeType ? 1f : 0f;
                isFading = false;
                return;
            }

            canvasGroup.alpha = fadeType
                ? Mathf.Lerp(0f, 1f, timeElapsed / fadeDuration)
                : Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            SendCustomEventDelayedFrames(nameof(_FadeLoop), 0);
        }

        private bool LoopSendNotificationContainers(int _startIndex)
        {
            // Basic error checking to prevent issues.
            if (notificationContainers == null)
            {
                Debug.LogError("Array is not initialized or is empty.");
                return false;
            }

            if (_startIndex < 0 || _startIndex >= notificationContainers.Length)
            {
                Debug.LogError(
                    $"Invalid start index: {_startIndex}. It must be between 0 and {notificationContainers.Length - 1}.");
                return false;
            }

            // We loop exactly 'dataArray.Length' times to hit every element once.
            for (int i = 0; i < notificationContainers.Length; i++)
            {
                // This is the magic formula.
                // It calculates the current position in the array, wrapping around if necessary.
                int currentIndex = (_startIndex + i) % notificationContainers.Length;
                if (notificationContainers[currentIndex]._SendNotification())
                {

                    //_notificationCount++;
                    //if (_notificationCount >= notificationContainers.Length) _notificationCount = 0;
                    return true;
                }
            }

            Debug.Log("Loop finished.");
            return false;
        }

        // #################################################################################################################

        #region Loop

        private bool isLooping = false;

        private void _StartLoop()
        {
            if (isLooping) return;
            isLooping = true;
            _Loop();
        }

        private void _StopLoop()
        {
            if (!isLooping) return;
            isLooping = false;
        }

        public void _Loop()
        {
            Debug.Log($"[UdonVR][Notification] Looping {_notificationCount}");
            if (!isLooping) return;
            if (LoopSendNotificationContainers(_notificationCount))
            {
                _notificationCount = (_notificationCount + 1) % notificationContainers.Length;
                SendCustomEventDelayedSeconds(nameof(_Loop), notificationDuration + notificationBuffer);
            }
            else
            {
                Debug.Log($"[UdonVR][Notification]Looping Stopping");
                _StopLoop();
            }
        }

        #endregion

        // #################################################################################################################

        #region Ui Scaling

        public float uiScale = .25f;
        public float uiScaleVr = .5f;
        public RectTransform canvasRectTransform;
        public RectTransform uiElementToPosition;

        public void _PositionUIElement()
        {
            if (canvasRectTransform == null)
            {
                Debug.LogError("[UdonVR][Notification] Canvas RectTransform is not assigned.", this);
                return;
            }

            if (uiElementToPosition == null)
            {
                Debug.LogError("[UdonVR][Notification] UI Element To Position is not assigned.", this);
                return;
            }
            
            float canvasHeight = canvasRectTransform.rect.height;
            float targetX = 0f;
            float targetY = -canvasHeight * (isVR ? uiScaleVr : uiScale);

            Vector2 targetPosition = new Vector2(targetX, targetY);

            // Get the dimensions of both the canvas and the UI element.
            Rect canvasRect = canvasRectTransform.rect;
            Rect elementRect = uiElementToPosition.rect;

            float canvasMinX = -canvasRect.width / 2.0f;
            float canvasMaxX = canvasRect.width / 2.0f;
            float canvasMinY = -canvasRect.height / 2.0f;
            float canvasMaxY = canvasRect.height / 2.0f;

            float elementScaledHalfWidth = (elementRect.width / 2.0f) * uiElementToPosition.localScale.x;
            float elementScaledHalfHeight = (elementRect.height / 2.0f) * uiElementToPosition.localScale.y;

            float clampedX = Mathf.Clamp(targetPosition.x, canvasMinX + elementScaledHalfWidth,
                canvasMaxX - elementScaledHalfWidth);
            float clampedY = Mathf.Clamp(targetPosition.y, canvasMinY + elementScaledHalfHeight,
                canvasMaxY - elementScaledHalfHeight);

            Vector2 finalPosition = new Vector2(clampedX, clampedY);

            uiElementToPosition.anchoredPosition = finalPosition;

            if (isVR)
            {
                VRCPlayerApi.TrackingData headData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                vrHead.position = headData.position;
                Vector3 headUp = headData.rotation * Vector3.up;
                notificationDisplay.LookAt(vrHead.position, headUp);
                notificationDisplay.rotation *= Quaternion.Euler(0, 180f, 0);
            }
            
            Debug.Log($"UI Element positioned at {finalPosition} within the canvas.", this);
        }

        #endregion

        // #################################################################################################################
    }
}