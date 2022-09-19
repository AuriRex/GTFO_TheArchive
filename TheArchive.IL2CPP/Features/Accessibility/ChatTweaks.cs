using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Accessibility
{
    public class ChatTweaks : Feature
    {
        public override string Name => "Chat Tweaks";

        public static int MaxChatBacklog { get; set; } = 10;

        private static Stack<string> _lastPostedMessages = new Stack<string>(11);
        private static Stack<string> _lastPostedMessagesClone = new Stack<string>();
        private static Stack<string> _poppedMessages = new Stack<string>();
        private static string _cached = string.Empty;
        private static bool _shouldClone = false;

#if MONO
        private static FieldAccessor<PlayerChatManager, string> A_PlayerChatManager_m_currentValue;
        public override void Init()
        {
            A_PlayerChatManager_m_currentValue = FieldAccessor<PlayerChatManager, string>.GetAccessor("m_currentValue");
        }
#endif

        public void Update()
        {
            if (!PlayerChatManager.InChatMode) return;
            
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl))
            {
                if(Input.GetKeyDown(KeyCode.V))
                {
                    var newMsg = GetChatValue() + GUIUtility.systemCopyBuffer;
                    if (newMsg.Length > 50)
                        newMsg = newMsg.Substring(0, 50);
                    SetChatValue(newMsg);
                } 

                if(Input.GetKeyDown(KeyCode.C))
                {
                    GUIUtility.systemCopyBuffer = GetChatValue();
                }
            }

            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                if(_shouldClone)
                {
                    _cached = GetChatValue();
                    _lastPostedMessagesClone?.Clear();
                    _lastPostedMessagesClone = new Stack<string>(_lastPostedMessages.Reverse());
                    _poppedMessages.Clear();
                    _shouldClone = false;
                }

                if(_lastPostedMessagesClone.Count == _lastPostedMessages.Count)
                {
                    _cached = GetChatValue();
                }

                if(_lastPostedMessagesClone.Count > 0)
                {
                    var msg = _lastPostedMessagesClone.Pop();
                    SetChatValue(msg);
                    if (!string.IsNullOrWhiteSpace(msg))
                        _poppedMessages.Push(msg);
                }
            }

            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                if(_poppedMessages.Count > 0)
                {
                    var msg = _poppedMessages.Pop();
                    if (!string.IsNullOrWhiteSpace(msg))
                        _lastPostedMessagesClone.Push(msg);

                    string realMsg = null;
                    if (_poppedMessages.Count > 0)
                        realMsg = _poppedMessages.Peek();
                    if (string.IsNullOrWhiteSpace(realMsg))
                        realMsg = _cached;

                    SetChatValue(realMsg);
                }
                else
                {
                    SetChatValue(_cached);
                }
            }
        }

        public static void SetChatValue(string str)
        {
#if IL2CPP
            PlayerChatManager.Current.m_currentValue = str;
#else
            A_PlayerChatManager_m_currentValue.Set(PlayerChatManager.Current, str);
#endif
        }

        public static string GetChatValue()
        {
            return
#if IL2CPP
                PlayerChatManager.Current.m_currentValue;
#else
                A_PlayerChatManager_m_currentValue.Get(PlayerChatManager.Current);
#endif
        }

        [ArchivePatch(typeof(PlayerChatManager), "PostMessage")]
        public static class PlayerChatManager_PostMessage_Patch
        {

            public static void Prefix()
            {
                _shouldClone = true;
                var msg = GetChatValue();
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    _lastPostedMessages.Push(msg);

                    if (_lastPostedMessages.Count > MaxChatBacklog)
                    {
                        // this is ugly lol
                        _lastPostedMessages = new Stack<string>(_lastPostedMessages.Reverse().Skip(1));
                    }
                }
                    
            }
        }
    }
}
