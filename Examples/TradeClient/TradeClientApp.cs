using System;
using QuickFix;
using QuickFix.Fields;
using System.Collections.Generic;
using QuickFix.FIX44;

namespace TradeClient
{
    public class TradeClientApp : QuickFix.MessageCracker, QuickFix.IApplication
    {
        Session _session = null;

        // This variable is a kludge for developer test purposes.  Don't do this on a production application.
        public IInitiator MyInitiator = null;

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
        }

        public void OnLogon(SessionID sessionID)        
        {
            
            
            Console.WriteLine("Logon - " + sessionID.ToString());
            var message= QueryMarketDataRequest44();
            Session.SendToTarget(message, sessionID);

        }


        public void OnLogout(SessionID sessionID) { Console.WriteLine("Logout - " + sessionID.ToString()); }

        public void FromAdmin(QuickFix.Message message, SessionID sessionID) {
            Crack(message, sessionID);

        }
        public void ToAdmin(QuickFix.Message message, SessionID sessionID) {


            Crack(message, sessionID);

        }


        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            Console.WriteLine("IN:  " + message.ToString());
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void ToApp(QuickFix.Message message, SessionID sessionID)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(QuickFix.Fields.Tags.PossDupFlag))
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                        message.Header.GetString(QuickFix.Fields.Tags.PossDupFlag)); /// FIXME
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }

            Console.WriteLine();
            Console.WriteLine("OUT: " + message.ToString());
        }
        #endregion


        #region MessageCracker handlers
        public void OnMessage(QuickFix.FIX44.Logon m, SessionID s)
        {
            Console.WriteLine("logon start");
            m.Set(new Password("P@ssword"));
        }
        public void OnMessage(QuickFix.FIX44.Logout m,SessionID s)
        {
            Console.WriteLine("logout");
        }
        public void OnMessage(QuickFix.FIX44.Heartbeat m,SessionID s)
        {
            //to deal with exception
        }
        public void OnMessage(QuickFix.FIX44.BusinessMessageReject m ,SessionID s)
        {
            //to deal with exception

        }
        public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s)
        {
            Console.WriteLine("Received execution report");
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReject m, SessionID s)
        {
            Console.WriteLine("Received order cancel reject");
        }
        public void OnMessage(MarketDataSnapshotFullRefresh message, SessionID sessionID)
        {
            // Process the MarketDataSnapshotFullRefresh message
            var symbol = message.GetString(Symbol.TAG);
            var noMDEntries = message.GetInt(NoMDEntries.TAG);
            Console.WriteLine($"Received MarketDataSnapshotFullRefresh for {symbol} with {noMDEntries} entries");

            // Example: Iterate through MDEntries (Market Data Entries)
            var group = new MarketDataSnapshotFullRefresh.NoMDEntriesGroup();
            for (int i = 1; i <= noMDEntries; i++)
            {
                message.GetGroup(i, group);
                var mdEntryType = group.GetChar(MDEntryType.TAG);
                var mdEntryPx = group.GetDecimal(MDEntryPx.TAG);

                Console.WriteLine($"Entry {i}: Type={mdEntryType}, Price={mdEntryPx}");
            }
        }
        #endregion



        private void SendMessage(QuickFix.Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {
                // This probably won't ever happen.
                Console.WriteLine("Can't send message: session not created.");
            }
        }





        #region Message creation functions




        private QuickFix.FIX44.MarketDataRequest QueryMarketDataRequest44()
        {
            MDReqID mdReqID = new MDReqID("MARKETDATAID");
            SubscriptionRequestType subType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT);
            MarketDepth marketDepth = new MarketDepth(0);

            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup marketDataEntryGroup = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
            marketDataEntryGroup.Set(new MDEntryType(MDEntryType.BID));

            QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup symbolGroup = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
            symbolGroup.Set(new Symbol("LNUX"));

            QuickFix.FIX44.MarketDataRequest message = new QuickFix.FIX44.MarketDataRequest(mdReqID, subType, marketDepth);
            message.AddGroup(marketDataEntryGroup);
            message.AddGroup(symbolGroup);

            return message;
        }
        #endregion

    }
}
