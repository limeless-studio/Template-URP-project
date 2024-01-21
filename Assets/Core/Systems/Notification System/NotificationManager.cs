using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace UI
{
    public enum NotificationType
    {
        Title,
        SidePopup,
        Popup
    }
    
    public class Notification
    {
        public string text;
        public float duration;
    }
    
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance;

        [Header("References")] [SerializeField]
        private GameObject titleGameObject;
        [SerializeField] GameObject popupGameObject;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI sidePopupText;
        [SerializeField] TextMeshProUGUI popupText;
        [SerializeField] Animator titleAnimator;
        [SerializeField] Animator sidePopupAnimator;
        [SerializeField] Animator popupAnimator;

        [Space]
        [Header("Settings")]
        [SerializeField] float defaultDuration = 3f;
        
        Queue<Notification> _titleQueue = new Queue<Notification>();
        Queue<Notification> _sidePopupQueue = new Queue<Notification>();
        Queue<Notification> _popupQueue = new Queue<Notification>();
        private static readonly int Show = Animator.StringToHash("Show");

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {

            if (titleText == null) Debug.LogError("Title Text is null");
            if (sidePopupText == null) Debug.LogError("Side Popup Text is null");
            if (popupText == null) Debug.LogError("Popup Text is null");
            
            if (titleAnimator == null) titleAnimator = titleText.GetComponent<Animator>();
            if (sidePopupAnimator == null) sidePopupAnimator = sidePopupText.GetComponentInParent<Animator>();
            if (popupAnimator == null) popupAnimator = popupText.GetComponent<Animator>();
            
            if (titleAnimator == null) Debug.LogError("Title Animator is null");
            if (sidePopupAnimator == null) Debug.LogError("Side Popup Animator is null");
            if (popupAnimator == null) Debug.LogError("Popup Animator is null");
            
            popupText.text = "";
            sidePopupText.text = "";
            titleText.text = "";
            titleGameObject.SetActive(false);
            popupGameObject.SetActive(false);
            
            titleAnimator.SetBool(Show, false);
            sidePopupAnimator.SetBool(Show, false);
            popupAnimator.SetBool(Show, false);
        }

        public void ShowNotification(NotificationType type, string text, float duration = 0f)
        {
            if (duration == 0f) duration = defaultDuration;
            switch (type)
            {
                case NotificationType.Title:
                    _titleQueue.Enqueue(new Notification {text = text, duration = duration});
                    if (_titleQueue.Count == 1) StartCoroutine(ShowTitle(_titleQueue.Dequeue()));
                    break;
                case NotificationType.SidePopup:
                    _sidePopupQueue.Enqueue(new Notification {text = text, duration = duration});
                    if (_sidePopupQueue.Count == 1) StartCoroutine(ShowSidePopup(_sidePopupQueue.Dequeue()));
                    break;
                case NotificationType.Popup:
                    _popupQueue.Enqueue(new Notification {text = text, duration = duration});
                    if (popupText == null) Debug.LogError("Popup Text is null");
                    if (_popupQueue.Count == 1) StartCoroutine(ShowPopup(_popupQueue.Dequeue()));
                    break;
            }
        }
        
        public void ShowTitle(string text)
        {
            ShowNotification(NotificationType.Title, text);
        }
        
        public void ShowSidePopup(string text)
        {
            ShowNotification(NotificationType.SidePopup, text);
        }
        
        public void ShowPopup(string text)
        {
            ShowNotification(NotificationType.Popup, text);
        }
        
        
        public void HideCurrentPopup()
        {
            popupAnimator.SetBool(Show, false);
            popupText.text = "";
            popupGameObject.SetActive(false);
            if (_popupQueue.Count == 0) return;
            StartCoroutine(ShowPopup(_popupQueue.Dequeue()));
        }
        
        public void HideCurrentSidePopup()
        {
            sidePopupAnimator.SetBool(Show, false);
            sidePopupText.text = "";
            if (_sidePopupQueue.Count == 0) return;
            StartCoroutine(ShowSidePopup(_sidePopupQueue.Dequeue()));
        }
        
        public void HideCurrentTitle()
        {
            titleAnimator.SetBool(Show, false);
            titleText.text = "";
            titleGameObject.SetActive(false);
            if (_titleQueue.Count == 0) return;
            StartCoroutine(ShowTitle(_titleQueue.Dequeue()));
        }
        

        IEnumerator ShowTitle(Notification notification)
        {
            if (titleText == null || titleAnimator == null) yield break;
            titleGameObject.SetActive(true);
            titleText.text = notification.text;
            titleAnimator.SetBool(Show, true);
            yield return new WaitForSeconds(notification.duration);
            titleAnimator.SetBool(Show, false);
            yield return new WaitForSeconds(0.5f);
            if (_titleQueue.Count == 0)
            {
                titleText.text = "";
                titleGameObject.SetActive(false);
            }
            else StartCoroutine(ShowTitle(_titleQueue.Dequeue()));
        }
        
        IEnumerator ShowSidePopup(Notification notification)
        {
            if (sidePopupText == null || sidePopupAnimator == null) yield break;
            sidePopupText.text = notification.text;
            sidePopupAnimator.SetBool(Show, true);
            yield return new WaitForSeconds(notification.duration);
            sidePopupAnimator.SetBool(Show, false);
            yield return new WaitForSeconds(0.5f);
            if (_sidePopupQueue.Count == 0) sidePopupText.text = "";
            else StartCoroutine(ShowSidePopup(_sidePopupQueue.Dequeue()));
        }
        
        IEnumerator ShowPopup(Notification notification)
        {
            if (popupText == null || popupAnimator == null) yield break;
            popupGameObject.SetActive(true);
            popupText.text = notification.text;
            popupAnimator.SetBool(Show, true);
            yield return new WaitForSeconds(notification.duration);
            popupAnimator.SetBool(Show, false);
            yield return new WaitForSeconds(0.5f);
            if (_popupQueue.Count == 0) popupText.text = "";
            else StartCoroutine(ShowPopup(_popupQueue.Dequeue()));
        }

        public void HideNotification(NotificationType popup)
        {
            switch (popup)
            {
                case NotificationType.Title:
                    HideCurrentTitle();
                    break;
                case NotificationType.SidePopup:
                    HideCurrentSidePopup();
                    break;
                case NotificationType.Popup:
                    HideCurrentPopup();
                    break;
            }
        }
        
        public void HideAll()
        {
            HideCurrentPopup();
            HideCurrentSidePopup();
            HideCurrentTitle();
        }
        
        public bool IsShowingPopup()
        {
            return popupAnimator.GetBool(Show);
        }
        
        public bool IsShowingSidePopup()
        {
            return sidePopupAnimator.GetBool(Show);
        }
        
        public bool IsShowingTitle()
        {
            return titleAnimator.GetBool(Show);
        }
    }
}