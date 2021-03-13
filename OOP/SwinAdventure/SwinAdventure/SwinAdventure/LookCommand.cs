using System;
namespace SwinAdventure
{
    public class LookCommand : Command
    {
        public LookCommand()
            : base(new string[] { })
        {
        }

        public override string Execute(Player player, string[] text)
        {
            if (text[0] != "look")
            {
                return "error in look input";
            }

            if (text[1] != "at")
            {
                return "What do you want to look at?";
            }

            string thingId = text[2];
            IHasInventory container;

            if (text.Length == 5)
            {
                if (text[3] != "in")
                {
                    return "What do you want to look in?";
                }

                string containerId = text[4];
                container = FetchContainer(player, containerId);

                if (container == null)
                {
                    return string.Format("I cannot find the {0}", containerId);
                }
            }
            else
            {
                container = player;
            }

            return LookAtIn(thingId, container);
        }

        private IHasInventory FetchContainer(Player p, string containerId)
        {
            GameObject container = p.Locate(containerId);
            return container as IHasInventory;
        }

        private string LookAtIn(string thingId, IHasInventory container)
        {
            string response;
            GameObject thing = container.Locate(thingId);

            if (thing != null)
            {
                response = thing.FullDescription;
            }
            else
            {
                response = string.Format("I cannot find the {0}", thingId);
            }

            return response;
        }
    }
}
