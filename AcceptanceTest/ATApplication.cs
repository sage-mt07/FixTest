﻿using System.Collections.Generic;
using QuickFix;

namespace AcceptanceTest
{
    public class ATApplication : MessageCracker, IApplication
    {
        public event System.Action? StopMeEvent;

        private readonly HashSet<KeyValuePair<string, SessionID>> _clOrdIDs = new();

        public ATApplication()
        {
        }

        public void OnMessage(QuickFix.FIX40.NewOrderSingle nos, SessionID sessionID)
        {
            ProcessNOS(nos, sessionID);
        }

        public void OnMessage(QuickFix.FIX41.NewOrderSingle nos, SessionID sessionID)
        {
            ProcessNOS(nos, sessionID);
        }

        public void OnMessage(QuickFix.FIX42.NewOrderSingle nos, SessionID sessionID)
        {
            ProcessNOS(nos, sessionID);
        }

        public void OnMessage(QuickFix.FIX42.SecurityDefinition message, SessionID sessionID)
        {
            Echo(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX43.NewOrderSingle nos, SessionID sessionID)
        {
            ProcessNOS(nos, sessionID);
        }

        public void OnMessage(QuickFix.FIX43.SecurityDefinition message, SessionID sessionID)
        {
            Echo(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX44.NewOrderSingle nos, SessionID sessionID)
        {
            ProcessNOS(nos, sessionID);
        }

        public void OnMessage(QuickFix.FIX44.SecurityDefinition message, SessionID sessionID)
        {
            Echo(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX44.QuoteRequest message, SessionID sessionID)
        {
            Echo(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX50.NewOrderSingle nos, SessionID sessionID)
        {
            ProcessNOS(nos, sessionID);
        }

        public void OnMessage(QuickFix.FIX50.SecurityDefinition message, SessionID sessionID)
        {
            Echo(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX50SP1.NewOrderSingle nos, SessionID sessionID)
        {
            ProcessNOS(nos, sessionID);
        }

        public void OnMessage(QuickFix.FIX50SP1.SecurityDefinition message, SessionID sessionID)
        {
            Echo(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX50SP2.NewOrderSingle nos, SessionID sessionID)
        {
            ProcessNOS(nos, sessionID);
        }

        public void OnMessage(QuickFix.FIX50SP2.SecurityDefinition message, SessionID sessionID)
        {
            Echo(message, sessionID);
        }

        protected void Echo(Message message, SessionID sessionID)
        {
            Message echo = new(message);
            Session.SendToTarget(echo, sessionID);
        }

        protected void ProcessNOS(Message message, SessionID sessionID)
        {
            Message echo = new(message);

            bool possResend = false;
            if (message.Header.IsSetField(QuickFix.Fields.Tags.PossResend))
                possResend = message.Header.GetBoolean(QuickFix.Fields.Tags.PossResend);

            KeyValuePair<string, SessionID> pair = new(message.GetString(QuickFix.Fields.Tags.ClOrdID), sessionID);
            if (possResend && _clOrdIDs.Contains(pair))
                return;
            _clOrdIDs.Add(pair);

            Session.SendToTarget(echo, sessionID);
        }


        public void OnMessage(QuickFix.FIX41.News news, SessionID sessionID) { ProcessNews(news, sessionID); }
        public void OnMessage(QuickFix.FIX42.News news, SessionID sessionID) { ProcessNews(news, sessionID); }
        public void OnMessage(QuickFix.FIX43.News news, SessionID sessionID) { ProcessNews(news, sessionID); }
        public void OnMessage(QuickFix.FIX44.News news, SessionID sessionID) { ProcessNews(news, sessionID); }
        public void OnMessage(QuickFix.FIX50.News news, SessionID sessionID) { ProcessNews(news, sessionID); }
        public void OnMessage(QuickFix.FIX50SP1.News news, SessionID sessionID) { ProcessNews(news, sessionID); }
        public void OnMessage(QuickFix.FIX50SP2.News news, SessionID sessionID) { ProcessNews(news, sessionID); }

        public void ProcessNews(Message msg, SessionID sessionID)
        {
            if (msg.IsSetField(QuickFix.Fields.Tags.Headline) && (msg.GetString(QuickFix.Fields.Tags.Headline) == "STOPME"))
            {
                if (this.StopMeEvent != null)
                    StopMeEvent();
            }
            else
                Echo(msg, sessionID);
        }

        public void OnMessage(QuickFix.FIX44.TradeCaptureReportRequest msg, SessionID sessionID)
        {
            // do nothing, just swallow it.
        }

        #region Application Methods

        public void OnCreate(SessionID sessionID)
        {
            Session session = Session.LookupSession(sessionID);

            // Hey QF/N users, don't do this in a real app.
            session?.Reset("AT Session Reset");
        }

        public void OnLogout(SessionID sessionID)
        {
            _clOrdIDs.Clear();
        }

        public void OnLogon(SessionID sessionID)
        { }

        public void FromApp(Message message, SessionID sessionID)
        {
            try
            {
                string msgType = message.Header.GetString(QuickFix.Fields.Tags.MsgType);
                // log_.OnEvent("Got message " + msgType);
                // System.Console.WriteLine("===got message " + msgType);

                Crack(message, sessionID);
            }
            catch (UnsupportedMessageType)
            {
                throw;
            }
            catch (System.Exception e)
            {
                Session.LookupSession(sessionID).Log.OnEvent("Exception during FromApp: " + e.ToString() + "\n while processing msg (" + message.ToString() + ")");
            }
        }

        public void FromAdmin(Message message, SessionID sessionID)
        { }

        public void ToAdmin(Message message, SessionID sessionID) { }
        public void ToApp(Message message, SessionID sessionID) { }

        #endregion
    }
}
