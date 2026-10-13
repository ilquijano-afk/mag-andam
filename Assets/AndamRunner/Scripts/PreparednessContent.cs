using UnityEngine;

namespace AndamRunner
{
    public enum SupplyKind { GoBag, Water, FirstAid, Flashlight, Whistle, Radio, ContactCard, Cash, Food }
    public enum DistractorKind { GymEquipment, Toys, Beer, Makeup, GameConsole }
    public enum HazardKind { Floodwater, PowerLine, Debris, RoadBlock, HangingCable, FloodStreak }
    public enum PowerUpKind { Magnet, DoubleScore, QuadrupleScore, OctupleScore }
    public enum RunState { Splash, Ready, Playing, Paused, Results }
    public enum MenuPage { Splash, Home, Instructions, About, Credits, Leaderboard, Game, Pause, Results }

    // All learning copy and scoring live here so you can edit them without changing movement.
    public static class PreparednessContent
    {
        public const int SupplyCount = 9;
        public const int DistractorCount = 5;
        public const int DistractorPenalty = 5;
        public const int HazardCount = 6;
        public const int PowerUpCount = 4;
        public const float FloodStreakLength = 32f;
        public static readonly string[] SupplyNames =
        {
            "GO-BAG", "WATER", "FIRST AID", "FLASHLIGHT", "WHISTLE", "RADIO", "CONTACT CARD", "CASH", "NON-PERISHABLE FOOD"
        };
        public static readonly string[] SupplySymbols = { "BAG", "H2O", "AID", "LAMP", "SOS", "FM", "CARD", "CASH", "FOOD" };
        public static readonly int[] Points = { 20, 10, 15, 10, 10, 15, 25, 10, 15 };
        public static readonly string[] SupplyTips =
        {
            "Pack a go-bag with an adult. Keep it where everyone can find it.",
            "Store safe drinking water with your emergency supplies.",
            "Know where the first-aid kit is. Ask an adult for help using it.",
            "Keep a flashlight and spare batteries ready for a power outage.",
            "Keep a whistle in your go-bag to signal for help.",
            "Use a battery-powered radio with an adult to hear official updates.",
            "Write family contacts on a card. Keep a copy in your go-bag.",
            "Ask an adult to keep some emergency cash with the family's supplies.",
            "Pack ready-to-eat food that keeps without a fridge. Check dates with an adult."
        };
        public static readonly string[] DistractorNames = { "GYM EQUIPMENT", "BULKY TOY", "BEER", "MAKEUP", "GAME CONSOLE" };
        public static readonly string[] DistractorTips =
        {
            "Heavy gym gear uses bag space. Choose emergency supplies first.",
            "This bulky toy uses bag space. Ask an adult about a small comfort item after essentials.",
            "Beer is not a child's emergency supply. Choose drinking water.",
            "Choose emergency supplies before makeup. Hygiene supplies are different.",
            "A game console needs power. Choose the emergency radio and flashlight first."
        };
        public static readonly string[] HazardNames = { "FLOODWATER", "DOWNED WIRES", "SMALL DEBRIS", "ROAD BARRIER", "LOW CABLE: DUCK", "LONG FLOOD: CHANGE LANES" };
        public static readonly string[] HazardTips =
        {
            "Stay out of floodwater. Move to a safe route with an adult.",
            "Stay far from fallen wires. Tell an adult; never touch them.",
            "Keep paths and exits clear. Ask an adult to remove real debris.",
            "Use an open, safe route. Follow local officials' instructions.",
            "Swipe DOWN for the game's disconnected cable. Stay away from real electrical wires.",
            "Long floods need a lane change. In real life, use a dry, safe route with an adult."
        };
        public static readonly string[] HomeActions =
        {
            "With an adult, pack a go-bag and choose a place everyone can find.",
            "With an adult, write a family contact card and choose a meeting place.",
            "Ask an adult to secure heavy cabinets and shelves. Never move them alone.",
            "With an adult, check that doors and exit paths are clear.",
            "With an adult, practice Drop, Cover, and Hold On for an earthquake.",
            "With an adult, check the flashlight, radio, batteries and drinking water.",
            "Ask an adult how your family receives official local disaster warnings."
        };
        public static readonly Color Ink = new Color32(13, 47, 68, 255);
        public static readonly Color Teal = new Color32(13, 143, 151, 255);
        public static readonly Color Mint = new Color32(143, 221, 188, 255);
        public static readonly Color Orange = new Color32(252, 160, 70, 255);
        public static readonly Color Red = new Color32(218, 75, 63, 255);
        public static readonly Color Cream = new Color32(255, 247, 224, 255);
    }
}
