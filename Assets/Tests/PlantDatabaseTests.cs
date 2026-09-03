using NUnit.Framework;
using UnityEngine.TestTools;
using NativeLens.Data;
using NativeLens.Models;
using System.Collections;
using System.Collections.Generic;

namespace NativeLens.Tests
{
    /// <summary>
    /// Unit tests for PlantDatabase and plant data integrity.
    /// </summary>
    public class PlantDatabaseTests
    {
        private PlantDatabase plantDatabase;

        [SetUp]
        public void Setup()
        {
            plantDatabase = ScriptableObject.CreateInstance<PlantDatabase>();
            plantDatabase.InitializeMVPPlants();
        }

        [TearDown]
        public void Teardown()
        {
            if (plantDatabase != null)
                Object.DestroyImmediate(plantDatabase);
        }

        [Test]
        public void PlantDatabase_HasSevenPlants()
        {
            Assert.AreEqual(7, plantDatabase.TotalPlantCount, "Should have exactly 7 MVP plants");
        }

        [Test]
        public void PlantDatabase_AllPlantsHaveRequiredFields()
        {
            foreach (var plant in plantDatabase.Plants)
            {
                Assert.IsFalse(string.IsNullOrEmpty(plant.id), "Plant ID should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(plant.commonName), "Common name should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(plant.scientificName), "Scientific name should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(plant.family), "Family should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(plant.nativeRegion), "Native region should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(plant.ecologicalImportance), "Ecological importance should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(plant.conservationStatus), "Conservation status should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(plant.description), "Description should not be empty");
            }
        }

        [Test]
        public void PlantDatabase_ContainsExpectedSpecies()
        {
            var expectedIds = new HashSet<string> 
            { 
                "neem", "jamun", "banyan", "konrai", 
                "pungam", "terminalia_pallida", "terminalia_paniculata" 
            };

            foreach (var plant in plantDatabase.Plants)
            {
                Assert.IsTrue(expectedIds.Contains(plant.id), $"Unexpected plant ID: {plant.id}");
                expectedIds.Remove(plant.id);
            }

            Assert.AreEqual(0, expectedIds.Count, "Missing expected plants: " + string.Join(", ", expectedIds));
        }

        [Test]
        public void PlantDatabase_TerminaliaPallida_IsEndemicAndVulnerable()
        {
            var plant = plantDatabase.GetPlantById("terminalia_pallida");
            Assert.IsNotNull(plant);
            Assert.AreEqual("Endemic to Eastern Ghats", plant.nativeStatus);
            Assert.AreEqual("Vulnerable", plant.conservationStatus);
        }

        [Test]
        public void PlantDatabase_GetPlantById_Works()
        {
            var plant = plantDatabase.GetPlantById("neem");
            Assert.IsNotNull(plant);
            Assert.AreEqual("Azadirachta indica", plant.scientificName);
        }

        [Test]
        public void PlantDatabase_GetPlantByScientificName_Works()
        {
            var plant = plantDatabase.GetPlantByScientificName("Syzygium cumini");
            Assert.IsNotNull(plant);
            Assert.AreEqual("jamun", plant.id);
        }

        [Test]
        public void PlantDatabase_AllPlantsHaveTamilNames()
        {
            foreach (var plant in plantDatabase.Plants)
            {
                Assert.IsFalse(string.IsNullOrEmpty(plant.tamilName), 
                    $"Plant {plant.commonName} should have a Tamil name");
            }
        }

        [Test]
        public void PlantDatabase_AllPlantsHaveCommonQuestions()
        {
            foreach (var plant in plantDatabase.Plants)
            {
                Assert.IsNotNull(plant.commonQuestions);
                Assert.GreaterOrEqual(plant.commonQuestions.Length, 3, 
                    $"Plant {plant.commonName} should have at least 3 common questions");
            }
        }
    }
}