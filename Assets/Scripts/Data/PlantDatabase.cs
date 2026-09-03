using UnityEngine;
using NativeLens.Models;
using System.Collections.Generic;

namespace NativeLens.Data
{
    /// <summary>
    /// ScriptableObject containing all plant data for the MVP.
    /// This keeps plant data separate from UI code as per the agent rules.
    /// </summary>
    [CreateAssetMenu(fileName = "PlantDatabase", menuName = "NativeLens/Plant Database")]
    public class PlantDatabase : ScriptableObject
    {
        [SerializeField] private List<Plant> plants = new List<Plant>();

        public IReadOnlyList<Plant> Plants => plants;

        public Plant GetPlantById(string id)
        {
            return plants.Find(p => p.id == id);
        }

        public Plant GetPlantByScientificName(string scientificName)
        {
            return plants.Find(p => p.scientificName == scientificName);
        }

        public int TotalPlantCount => plants.Count;

        public int DiscoveredCount(System.Func<string, PlantDiscoveryState> getState)
        {
            int count = 0;
            foreach (var plant in plants)
            {
                var state = getState(plant.id);
                if (state.IsDiscovered) count++;
            }
            return count;
        }

        /// <summary>
        /// Initialize with the 7 MVP species for Vellore region.
        /// Called once to populate the database.
        /// </summary>
        public void InitializeMVPPlants()
        {
            plants.Clear();
            
            plants.Add(new Plant
            {
                id = "neem",
                commonName = "Neem",
                tamilName = "வேப்பம் (Vepam)",
                scientificName = "Azadirachta indica",
                family = "Meliaceae",
                nativeRegion = "Indian subcontinent, Southeast Asia",
                nativeStatus = "Native",
                ecologicalImportance = "Neem is a keystone species in dry deciduous forests. Its flowers attract pollinators like bees and butterflies. Birds such as bulbuls and mynas feed on its fruits. The tree provides shade and improves soil fertility through leaf litter decomposition. Neem leaves and seeds contain natural compounds (azadirachtin) that deter herbivores, making it a natural pesticide source.",
                conservationStatus = "Least Concern",
                threats = "Habitat loss due to urbanization and agricultural expansion. Overharvesting for medicinal and commercial products in some regions. Climate change affecting flowering and fruiting patterns.",
                conservationActions = "Plant neem saplings in community spaces and along roadsides. Support sustainable harvesting practices for neem-based products. Protect mature neem trees in urban areas. Participate in local tree plantation drives.",
                identifyingFeatures = "Medium to large evergreen tree (15-20m). Pinnate leaves with 9-15 serrated leaflets. White fragrant flowers in drooping panicles. Oval drupe fruits (1.5-2cm) turning yellow when ripe. Rough, fissured grey-brown bark.",
                description = "Neem (Azadirachta indica) is one of the most versatile and culturally significant trees in India. Known as the 'village pharmacy', every part of the tree has traditional medicinal uses. It thrives in the semi-arid conditions of Vellore and is commonly found across the VIT campus and surrounding areas.",
                imageUrls = new[] { "neem_1", "neem_2", "neem_3" },
                arModelPath = "Models/neem_ar",
                commonQuestions = new[] 
                { 
                    "Why is neem called the village pharmacy?",
                    "What animals depend on neem?",
                    "Is neem endangered?",
                    "How can I grow neem at home?",
                    "What are the medicinal uses of neem?"
                }
            });

            plants.Add(new Plant
            {
                id = "jamun",
                commonName = "Jamun / Naval",
                tamilName = "நாவல் (Naval)",
                scientificName = "Syzygium cumini",
                family = "Myrtaceae",
                nativeRegion = "Indian subcontinent, Myanmar, Sri Lanka",
                nativeStatus = "Native",
                ecologicalImportance = "Jamun is a vital food source for wildlife. Its fruits are eaten by birds (hornbills, barbets, parakeets), bats (flying foxes), and mammals (civets, jackals). The tree provides nesting sites for birds. Flowers attract bees and other pollinators. Its dense canopy offers shade and reduces soil erosion along water bodies.",
                conservationStatus = "Least Concern",
                threats = "Loss of riparian habitat due to dam construction and water diversion. Urbanization replacing natural groves. Overharvesting of fruits for commercial markets in some areas.",
                conservationActions = "Protect jamun trees near water bodies and in urban areas. Plant jamun along riverbanks and lake margins for erosion control. Avoid excessive fruit collection from wild trees. Support community conservation of sacred groves containing jamun.",
                identifyingFeatures = "Large evergreen tree (20-30m). Glossy, leathery opposite leaves with prominent midrib. White fragrant flowers in clusters. Oblong, dark purple to black fleshy fruits (2-3cm) with a single seed. Smooth grey bark that flakes in patches.",
                description = "Jamun (Syzygium cumini), known as Naval in Tamil, is a beloved fruit tree across India. Its sweet, astringent purple fruits are a seasonal delicacy. The tree is commonly found near water bodies and in temple groves around Vellore. It holds cultural significance and is often planted near homes and temples.",
                imageUrls = new[] { "jamun_1", "jamun_2", "jamun_3" },
                arModelPath = "Models/jamun_ar",
                commonQuestions = new[]
                {
                    "Why is jamun important for wildlife?",
                    "When does jamun fruit in Vellore?",
                    "Can I eat jamun seeds?",
                    "What birds eat jamun fruits?",
                    "How is jamun different from other Syzygium species?"
                }
            });

            plants.Add(new Plant
            {
                id = "banyan",
                commonName = "Indian Banyan",
                tamilName = "ஆலமரம் (Aalamaram)",
                scientificName = "Ficus benghalensis",
                family = "Moraceae",
                nativeRegion = "Indian subcontinent",
                nativeStatus = "Native",
                ecologicalImportance = "The banyan is a keystone species and ecosystem engineer. Its figs feed over 100 species of birds and mammals. Prop roots create microhabitats for reptiles, amphibians, and invertebrates. The massive canopy regulates local temperature and humidity. It's a 'nurse tree' facilitating the growth of other plant species beneath it.",
                conservationStatus = "Least Concern",
                threats = "Urban development removing large trees. Root damage from construction and paving. Pollution affecting pollinator wasps (essential for fig reproduction). Climate change impacting fig-wasp mutualism.",
                conservationActions = "Protect existing banyan trees with legal safeguards (Tree Preservation Orders). Avoid construction near root zones. Support conservation of sacred groves. Plant banyan in large open spaces where it can grow naturally.",
                identifyingFeatures = "Massive spreading tree with prop roots from branches. Large, leathery, elliptical leaves (10-20cm). Small figs (1-2cm) in pairs, red when ripe. Milky latex from all parts. Smooth grey bark. Aerial roots become additional trunks.",
                description = "The Indian Banyan (Ficus benghalensis) is the national tree of India and a symbol of longevity. Known as 'Aalamaram' in Tamil, it can cover acres through its prop roots. The VIT campus and Vellore region have several magnificent specimens. It's a living ecosystem supporting incredible biodiversity.",
                imageUrls = new[] { "banyan_1", "banyan_2", "banyan_3" },
                arModelPath = "Models/banyan_ar",
                commonQuestions = new[]
                {
                    "How does the banyan tree spread?",
                    "What is the fig-wasp relationship?",
                    "Why is banyan considered sacred?",
                    "How long can a banyan live?",
                    "What animals live in a banyan tree?"
                }
            });

            plants.Add(new Plant
            {
                id = "konrai",
                commonName = "Golden Shower / Konrai",
                tamilName = "கொன்றை (Konrai)",
                scientificName = "Cassia fistula",
                family = "Fabaceae",
                nativeRegion = "Indian subcontinent, Southeast Asia",
                nativeStatus = "Native",
                ecologicalImportance = "Konrai is a crucial dry-season resource. Its bright yellow flowers provide nectar for bees, butterflies, and sunbirds when few other plants bloom. The long cylindrical pods are eaten by monkeys, deer, and birds. As a legume, it fixes nitrogen, enriching soil. It's a host plant for several butterfly species.",
                conservationStatus = "Least Concern",
                threats = "Habitat loss in dry deciduous forests. Over-collection of flowers for religious/cultural use. Road widening removing avenue trees. Invasive species competition in degraded habitats.",
                conservationActions = "Plant konrai as avenue trees and in public parks. Protect existing trees during road projects. Grow konrai in home gardens for pollinator support. Participate in native tree plantation drives focusing on dry deciduous species.",
                identifyingFeatures = "Medium deciduous tree (10-15m). Pinnate leaves with 4-8 pairs of leaflets. Spectacular pendulous racemes of bright yellow flowers (30-60cm). Long cylindrical pods (30-60cm) turning dark brown. Smooth grey bark.",
                description = "Konrai (Cassia fistula), the state flower of Kerala, transforms into a cascade of gold during April-May. Known as 'Amaltas' in Hindi, it's a iconic dry-season bloomer across Vellore. The tree is leafless when flowering, making the display even more striking. It's commonly planted as an avenue tree.",
                imageUrls = new[] { "konrai_1", "konrai_2", "konrai_3" },
                arModelPath = "Models/konrai_ar",
                commonQuestions = new[]
                {
                    "Why does konrai flower when leafless?",
                    "What pollinates konrai flowers?",
                    "Is konrai the same as laburnum?",
                    "When is the best time to see konrai bloom?",
                    "Can konrai grow in home gardens?"
                }
            });

            plants.Add(new Plant
            {
                id = "pungam",
                commonName = "Indian Beech / Pungam",
                tamilName = "புங்கை (Pungai)",
                scientificName = "Pongamia pinnata",
                family = "Fabaceae",
                nativeRegion = "Indian subcontinent, Southeast Asia, Australia",
                nativeStatus = "Native",
                ecologicalImportance = "Pungam is a pioneer species in coastal and riparian zones. Its roots stabilize soil and prevent erosion. Flowers attract bees and butterflies. Seeds contain oil used for biofuel and traditional medicine. The tree provides shade for understory plants. Leaf litter enriches soil with nitrogen.",
                conservationStatus = "Least Concern",
                threats = "Coastal development destroying mangrove-associated habitats. Overharvesting seeds for oil extraction. Conversion of wetlands for aquaculture. Climate change and sea-level rise affecting coastal populations.",
                conservationActions = "Plant pungam in coastal restoration projects. Use pungam for biofuel from sustainably managed plantations. Protect natural stands in wetland areas. Promote pungam as a shade tree in agroforestry systems.",
                identifyingFeatures = "Medium evergreen tree (15-20m). Glossy, pinnate leaves with 5-7 leaflets. White/pink/lavender pea-like flowers in racemes. Flat, woody, elliptical pods (3-5cm) with 1-2 seeds. Grey-brown bark with vertical fissures.",
                description = "Pungam (Pongamia pinnata), known as 'Pungai' in Tamil, is a hardy coastal and inland tree. Its seeds yield pongamia oil, a promising biofuel. The tree thrives in Vellore's conditions and is often found near water bodies. It's valued for shade, soil improvement, and traditional medicine.",
                imageUrls = new[] { "pungam_1", "pungam_2", "pungam_3" },
                arModelPath = "Models/pungam_ar",
                commonQuestions = new[]
                {
                    "What is pongamia oil used for?",
                    "Why is pungam good for coastal areas?",
                    "Does pungam fix nitrogen?",
                    "What butterflies use pungam as host plant?",
                    "How is pungam different from other legume trees?"
                }
            });

            plants.Add(new Plant
            {
                id = "terminalia_pallida",
                commonName = "White-leaved Terminalia",
                tamilName = "வெள்ளை இற świeலேட் (Vellai Iruveli)",
                scientificName = "Terminalia pallida",
                family = "Combretaceae",
                nativeRegion = "Eastern Ghats, India (endemic)",
                nativeStatus = "Endemic to Eastern Ghats",
                ecologicalImportance = "Terminalia pallida is an Eastern Ghats endemic species, making it a unique component of the local biodiversity. It contributes to the forest canopy in dry deciduous forests. Its fruits are dispersed by birds and mammals. The tree provides habitat for epiphytes and invertebrates. As an endemic, it has high conservation value.",
                conservationStatus = "Vulnerable",
                threats = "Very restricted range in Eastern Ghats. Habitat fragmentation from mining and infrastructure projects. Fuelwood collection in local communities. Climate change altering dry forest dynamics. Lack of regeneration in disturbed areas.",
                conservationActions = "Protect Eastern Ghats forest fragments containing this species. Support habitat corridor creation between forest patches. Ex-situ conservation in botanical gardens. Community awareness about its endemic status. Monitor populations in protected areas.",
                identifyingFeatures = "Medium tree (10-15m). Distinctive white/greyish underside of leaves. Simple, alternate, elliptical leaves. Small white flowers in spikes. Winged fruits (samaras) for wind dispersal. Grey, rough bark.",
                description = "Terminalia pallida is a rare endemic tree of the Eastern Ghats, the hill range near Vellore. Its distinctive white leaf undersides make it identifiable. This species represents the unique evolutionary history of the Eastern Ghats. Finding it is a special discovery for any biodiversity enthusiast.",
                imageUrls = new[] { "terminalia_pallida_1", "terminalia_pallida_2", "terminalia_pallida_3" },
                arModelPath = "Models/terminalia_pallida_ar",
                commonQuestions = new[]
                {
                    "Why is Terminalia pallida endemic to Eastern Ghats?",
                    "What does 'Vulnerable' conservation status mean?",
                    "How can I help protect this species?",
                    "What makes its leaves distinctive?",
                    "Where else in Vellore can this be found?"
                }
            });

            plants.Add(new Plant
            {
                id = "terminalia_paniculata",
                commonName = "Flowering Murdah",
                tamilName = "பனைமரம் (Panaimaram) / மாத்தி (Mathi)",
                scientificName = "Terminalia paniculata",
                family = "Combretaceae",
                nativeRegion = "Western Ghats, Central India, Eastern Ghats",
                nativeStatus = "Native",
                ecologicalImportance = "Terminalia paniculata is a dominant canopy tree in moist deciduous forests. Its mass flowering events provide massive nectar resources for pollinators. Fruits are eaten by hornbills, imperial pigeons, and mammals. The tree stores significant carbon. Bark and leaves have traditional medicinal uses.",
                conservationStatus = "Least Concern",
                threats = "Habitat loss from conversion to plantations (teak, eucalyptus). Logging for timber in unprotected areas. Fire in dry season affecting regeneration. Climate change altering flowering phenology.",
                conservationActions = "Protect moist deciduous forest patches. Support sustainable forest management. Plant in restoration projects in appropriate habitats. Monitor flowering and fruiting patterns as climate indicators.",
                identifyingFeatures = "Large deciduous tree (20-30m). Simple, alternate leaves clustered at branch ends. Spectacular mass flowering - creamy white spikes covering canopy. Winged fruits (samaras) in dense clusters. Thick, dark, fissured bark.",
                description = "Terminalia paniculata, known as 'Kindal' or 'Matti', is famous for its synchronized mass flowering that turns entire forest canopies white. In the Vellore region, it's found in the Eastern Ghats forests. The flowering spectacle attracts countless pollinators and is a remarkable ecological event.",
                imageUrls = new[] { "terminalia_paniculata_1", "terminalia_paniculata_2", "terminalia_paniculata_3" },
                arModelPath = "Models/terminalia_paniculata_ar",
                commonQuestions = new[]
                {
                    "What triggers mass flowering in Terminalia paniculata?",
                    "Why is mass flowering ecologically important?",
                    "What is the timber used for?",
                    "How does wind dispersal work for its fruits?",
                    "Where can I see mass flowering near Vellore?"
                }
            });
        }
    }
}