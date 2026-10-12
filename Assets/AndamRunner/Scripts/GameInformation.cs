using System;
using UnityEngine;

namespace AndamRunner
{
    [Serializable]
    public sealed class CreditSection
    {
        public string heading;
        public string names;
    }

    [Serializable]
    public sealed class GameInformation
    {
        public string presentedBy = "ANDAM LAPU-LAPU";
        public string theme = "Resilience Starts at Home";
        public string developer = "UP Cebu Center for Environmental Informatics";
        public string partner = "City of Lapu-Lapu Disaster Risk Reduction and Management Department";
        public string description = "A playful coastal adventure with Opong Andam, inspired by Lapu-Lapu City, Cebu. Collect emergency supplies, choose safe routes, and bring one preparedness lesson home to your family.";
        public CreditSection[] credits;

        public static GameInformation Load()
        {
            TextAsset source = Resources.Load<TextAsset>("GameInformation");
            GameInformation information = source != null ? JsonUtility.FromJson<GameInformation>(source.text) : new GameInformation();
            return information ?? new GameInformation();
        }
    }
}
