using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace SimpleAcceptor
{
    /// <summary>
    /// Just a simple server that will let you connect to it and ignore any
    /// application-level messages you send to it.
    /// Note that this app is *NOT* a message cracker.
    /// </summary>

    public class SimpleAcceptorApp : QuickFix.MessageCracker, QuickFix.IApplication
    {
        private Timer _marketDataTimer;
        #region QuickFix.Application Methods

        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            Console.WriteLine("IN:  " + message);
            Crack(message, sessionID);
        }

        public void ToApp(QuickFix.Message message, SessionID sessionID)
        {
            Console.WriteLine("OUT: " + message);
        }

        public void FromAdmin(QuickFix.Message message, SessionID sessionID)
        {
            Console.WriteLine("IN:  " + message);
            Crack(message, sessionID);
        }

        public void ToAdmin(QuickFix.Message message, SessionID sessionID)
        {
            Console.WriteLine("OUT:  " + message);
        }

        public void OnCreate(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
        #endregion

        public void OnMessage(QuickFix.FIX44.Logon m, SessionID s)
        {
            string password = m.GetString(Tags.Password);
            if(password != "P@ssword") 
            {
                Session.LookupSession(s).Disconnect("invalid password");
            }
        }
        private void OnMarketDataTimerElapsed(object sender, ElapsedEventArgs e)
        {

        }
        public void OnMessage(MarketDataRequest message, SessionID sessionID)
        {
            _marketDataTimer = new Timer(1000); // Trigger every 1000 milliseconds (1 second)
            _marketDataTimer.Elapsed += OnMarketDataTimerElapsed;
            _marketDataTimer.AutoReset = true;
            _marketDataTimer.Enabled = true;

            // Extract details from the MarketDataRequest
            MDReqID mdReqID = new MDReqID();
            SubscriptionRequestType subType = new SubscriptionRequestType();
            MarketDepth marketDepth = new MarketDepth();
            NoMDEntryTypes noMDEntryTypes = new NoMDEntryTypes();
            NoRelatedSym noRelatedSym = new NoRelatedSym();

            message.GetField(mdReqID);
            message.GetField(subType);
            message.GetField(marketDepth);
            message.GetField(noMDEntryTypes);
            message.GetField(noRelatedSym);

            // Example: Process each related symbol (currency pair)
            MarketDataRequest.NoRelatedSymGroup symbolGroup = new MarketDataRequest.NoRelatedSymGroup();
            for (int i = 1; i <= noRelatedSym.getValue(); i++)
            {
                message.GetGroup(i, symbolGroup);
                Symbol symbol = symbolGroup.GetField(new Symbol());

                // Here, implement the logic to handle each symbol
                // For example, fetching the market data for the given symbol
            }

            // Respond to the MarketDataRequest
            SendMarketDataSnapshotFullRefresh(mdReqID, sessionID);
        }

        private void SendMarketDataSnapshotFullRefresh(MDReqID mdReqID, SessionID sessionID)
        {
            // Create and send a MarketDataSnapshotFullRefresh message in response
            // Populate the MarketDataSnapshotFullRefresh with the requested data
            // ...

            // Example:
            MarketDataSnapshotFullRefresh mdSnapshot = new MarketDataSnapshotFullRefresh(mdReqID);
            // Set the required fields and groups (e.g., MDEntries) for the snapshot
            // ...

            Session.SendToTarget(mdSnapshot, sessionID);
        }

      
    }
}