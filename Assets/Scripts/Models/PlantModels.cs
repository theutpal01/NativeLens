namespace NativeLens.Models
{
    /// <summary>
    /// Represents a native plant species with all relevant botanical and ecological information.
    /// </summary>
    [System.Serializable]
    public class Plant
    {
        public string id;
        public string commonName;
        public string tamilName;
        public string scientificName;
        public string family;
        public string nativeRegion;
        public string nativeStatus; // "Native", "Endemic", "Naturalized"
        public string ecologicalImportance;
        public string conservationStatus; // "Least Concern", "Near Threatened", "Vulnerable", "Endangered", "Critically Endangered"
        public string threats;
        public string conservationActions;
        public string identifyingFeatures;
        public string description;
        public string[] imageUrls;
        public string arModelPath; // Optional 3D model for AR
        public string[] commonQuestions; // Pre-defined questions for the Botanical Guide
    }

    /// <summary>
    /// User's discovery state for a specific plant.
    /// </summary>
    [System.Serializable]
    public class PlantDiscoveryState
    {
        public string plantId;
        public DiscoveryState state = DiscoveryState.Locked;
        public System.DateTime? discoveredAt;
        public string discoveryLocation;
        public float discoveryLatitude;
        public float discoveryLongitude;
        public string discoveryPhotoPath;

        public bool IsDiscovered => state == DiscoveryState.Discovered;
        public bool IsIdentified => state >= DiscoveryState.Identified;
    }

    public enum DiscoveryState
    {
        Locked = 0,
        Identified = 1,
        Discovered = 2
    }

    /// <summary>
    /// Result from the AI plant identification service.
    /// </summary>
    [System.Serializable]
    public class IdentificationResult
    {
        public string speciesId;
        public string scientificName;
        public float confidence;
        public bool success;
        public string errorMessage;
    }

    /// <summary>
    /// Field observation record for saving user observations.
    /// </summary>
    [System.Serializable]
    public class FieldObservation
    {
        public string id;
        public string plantId;
        public System.DateTime observedAt;
        public float latitude;
        public float longitude;
        public string locationName;
        public string photoPath;
        public string notes;
    }

    /// <summary>
    /// Application state machine states.
    /// </summary>
    public enum AppState
    {
        Home,
        Scan,
        Analysing,
        Identified,
        ARInformation,
        Details,
        BotanicalGuide,
        Discovery,
        Gallery,
        UnableToIdentify
    }

    /// <summary>
    /// UI navigation tabs.
    /// </summary>
    public enum NavigationTab
    {
        Home = 0,
        Scan = 1,
        Gallery = 2
    }
}