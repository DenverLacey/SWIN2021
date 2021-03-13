using System;
using System.Collections.Generic;

namespace SwinAdventure
{
    public class IdentifiableObject
    {
        private List<string> identifiers;
        public string FirstId { get => identifiers.Count > 0 ? identifiers[0] : ""; }

        public IdentifiableObject(string[] ids)
        {
            identifiers = new List<string>();

            foreach (var id in ids)
            {
                AddIdentifier(id);
            }
        }

        public bool AreYou(string id)
        {
            return identifiers.IndexOf(id.ToLower()) != -1;
        }

        public void AddIdentifier(string id)
        {
            id = id.ToLower();
            identifiers.Add(id);
        }
    }
}
