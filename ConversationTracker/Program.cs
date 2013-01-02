using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using System;
using System.Collections.Generic;

namespace ConversationTracker
{
    class ConversationContainer
    {
        public Microsoft.Lync.Model.Conversation.Conversation Conversation { get; set; }
        public DateTime ConversationCreated { get; set; }
    }

    class Program
    {
        static Dictionary<String, ConversationContainer> ActiveConversations = new Dictionary<String, ConversationContainer>();
        
        static void Main(string[] args)
        {
            var client = LyncClient.GetClient();
            client.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
            client.ConversationManager.ConversationRemoved += ConversationManager_ConversationRemoved;
            Console.ReadLine();
        }

        static void ConversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            string ConversationID = e.Conversation.Properties[ConversationProperty.Id].ToString();

            if (e.Conversation.Modalities[ModalityTypes.AudioVideo].State != ModalityState.Disconnected)
            {
                StoreConversation(e.Conversation, ConversationID);
            }
            else
            {
                e.Conversation.Modalities[ModalityTypes.AudioVideo].ModalityStateChanged += Program_ModalityStateChanged;
            }
        }

        static void Program_ModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
           //in this case, any state change will be from Disconnected and will therefore indicate some A/V activity
            var modality = sender as Microsoft.Lync.Model.Conversation.AudioVideo.AVModality;
            
            string ConversationID = modality.Conversation.Properties[ConversationProperty.Id].ToString(); 

            if (!ActiveConversations.ContainsKey(ConversationID))
            {
                StoreConversation(modality.Conversation, ConversationID);
                modality.ModalityStateChanged -= Program_ModalityStateChanged;
            }
        }

        private static void StoreConversation(Conversation conversation, string ConversationID)
        {
            ActiveConversations.Add(ConversationID, new ConversationContainer()
            {
                Conversation = conversation,
                ConversationCreated = DateTime.Now
            });
        }

        static void ConversationManager_ConversationRemoved(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            string ConversationID = e.Conversation.Properties[ConversationProperty.Id].ToString();
            if (ActiveConversations.ContainsKey(ConversationID))
            {
                var container = ActiveConversations[ConversationID];
                TimeSpan conversationLength = DateTime.Now.Subtract(container.ConversationCreated);
                Console.WriteLine("Conversation {0} lasted {1} seconds", ConversationID, conversationLength);
                ActiveConversations.Remove(ConversationID);
            }
        }
       
    }
}
