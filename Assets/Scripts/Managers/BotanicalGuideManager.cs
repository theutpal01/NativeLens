using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeLens.Models;
using System.Collections.Generic;
using System.Text;

namespace NativeLens.Managers
{
    /// <summary>
    /// Manages the AI Botanical Guide conversation.
    /// Phase 7: Contextual AI Q&A about identified plants.
    /// </summary>
    public class BotanicalGuideManager : MonoBehaviour
    {
        public static BotanicalGuideManager Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private GameObject guidePanel;
        [SerializeField] private Transform chatContainer;
        [SerializeField] private GameObject userMessagePrefab;
        [SerializeField] private GameObject botMessagePrefab;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI currentPlantTitle;
        [SerializeField] private ScrollRect chatScrollRect;

        [Header("Quick Questions")]
        [SerializeField] private Transform quickQuestionsContainer;
        [SerializeField] private GameObject quickQuestionButtonPrefab;

        [Header("AI Settings")]
        [SerializeField] private bool useMockAI = true;
        [SerializeField] private string aiApiEndpoint = "";
        [SerializeField] private string aiApiKey = "";

        private Plant currentPlant;
        private List<ChatMessage> chatHistory = new List<ChatMessage>();
        private bool isProcessing = false;

        private struct ChatMessage
        {
            public string text;
            public bool isUser;
            public System.DateTime timestamp;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (sendButton != null) sendButton.onClick.AddListener(OnSendClicked);
            if (closeButton != null) closeButton.onClick.AddListener(CloseGuide);
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(_ => OnSendClicked());
            }

            if (guidePanel != null) guidePanel.SetActive(false);
        }

        public void OpenGuideForPlant(Plant plant)
        {
            currentPlant = plant;
            chatHistory.Clear();
            
            if (guidePanel != null) guidePanel.SetActive(true);
            if (currentPlantTitle != null)
                currentPlantTitle.text = $"🌿 Botanical Guide: {plant.commonName}";

            ClearChat();
            SetupQuickQuestions(plant);
            
            // Add welcome message
            AddBotMessage($"Hello! I'm your Botanical Guide for <b>{plant.commonName}</b> (<i>{plant.scientificName}</i>). Ask me anything about this plant!");
        }

        public void CloseGuide()
        {
            if (guidePanel != null) guidePanel.SetActive(false);
            currentPlant = null;
        }

        private void SetupQuickQuestions(Plant plant)
        {
            if (quickQuestionsContainer == null || quickQuestionButtonPrefab == null) return;

            ClearContainer(quickQuestionsContainer);

            if (plant.commonQuestions != null)
            {
                foreach (string question in plant.commonQuestions)
                {
                    GameObject btn = Instantiate(quickQuestionButtonPrefab, quickQuestionsContainer);
                    var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null) text.text = question;
                    
                    var button = btn.GetComponent<Button>();
                    if (button != null)
                    {
                        string q = question; // Capture for closure
                        button.onClick.AddListener(() => OnQuickQuestionClicked(q));
                    }
                }
            }
        }

        private void OnQuickQuestionClicked(string question)
        {
            if (inputField != null)
            {
                inputField.text = question;
                OnSendClicked();
            }
        }

        private void OnSendClicked()
        {
            if (inputField == null || string.IsNullOrWhiteSpace(inputField.text) || isProcessing) return;

            string userQuestion = inputField.text.Trim();
            inputField.text = "";
            
            AddUserMessage(userQuestion);
            ProcessQuestion(userQuestion);
        }

        private void AddUserMessage(string text)
        {
            var msg = new ChatMessage { text = text, isUser = true, timestamp = System.DateTime.Now };
            chatHistory.Add(msg);
            InstantiateMessage(userMessagePrefab, text, true);
        }

        private void AddBotMessage(string text)
        {
            var msg = new ChatMessage { text = text, isUser = false, timestamp = System.DateTime.Now };
            chatHistory.Add(msg);
            InstantiateMessage(botMessagePrefab, text, false);
        }

        private void InstantiateMessage(GameObject prefab, string text, bool isUser)
        {
            if (prefab == null || chatContainer == null) return;

            GameObject msgObj = Instantiate(prefab, chatContainer);
            var textComponent = msgObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }

            // Scroll to bottom
            Canvas.ForceUpdateCanvases();
            if (chatScrollRect != null)
            {
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private void ClearChat()
        {
            ClearContainer(chatContainer);
        }

        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        private async void ProcessQuestion(string question)
        {
            isProcessing = true;
            if (sendButton != null) sendButton.interactable = false;

            // Show typing indicator
            GameObject typingIndicator = Instantiate(botMessagePrefab, chatContainer);
            var typingText = typingIndicator.GetComponentInChildren<TextMeshProUGUI>();
            if (typingText != null) typingText.text = "🌿 Thinking...";

            string answer;
            if (useMockAI)
            {
                answer = GenerateMockAnswer(question);
            }
            else
            {
                answer = await CallRealAI(question);
            }

            Destroy(typingIndicator);
            AddBotMessage(answer);

            isProcessing = false;
            if (sendButton != null) sendButton.interactable = true;
        }

        private string GenerateMockAnswer(string question)
        {
            if (currentPlant == null) return "Please select a plant first.";

            string q = question.ToLower();
            var sb = new StringBuilder();

            // Knowledge safety: Use structured plant data as primary context
            if (q.Contains("what") && (q.Contains("plant") || q.Contains("this") || q.Contains("species")))
            {
                sb.AppendLine($"**{currentPlant.commonName}** (<i>{currentPlant.scientificName}</i>)");
                sb.AppendLine($"Family: {currentPlant.family}");
                sb.AppendLine(currentPlant.description);
            }
            else if (q.Contains("native") || q.Contains("vellore") || q.Contains("region") || q.Contains("where"))
            {
                sb.AppendLine($"**Native Status:** {currentPlant.nativeStatus}");
                sb.AppendLine($"**Native Region:** {currentPlant.nativeRegion}");
                sb.AppendLine(currentPlant.nativeStatus == "Endemic to Eastern Ghats" 
                    ? "This species is <b>endemic to the Eastern Ghats</b>, making it a unique treasure of this region!"
                    : "This plant is native to the Indian subcontinent and thrives in the Vellore area.");
            }
            else if (q.Contains("important") || q.Contains("role") || q.Contains("ecolog") || q.Contains("ecosystem"))
            {
                sb.AppendLine("**Ecological Importance:**");
                sb.AppendLine(currentPlant.ecologicalImportance);
            }
            else if (q.Contains("endanger") || q.Contains("threat") || q.Contains("conservation") || q.Contains("vulnerable"))
            {
                sb.AppendLine($"**Conservation Status:** {currentPlant.conservationStatus}");
                sb.AppendLine($"**Threats:** {currentPlant.threats}");
                sb.AppendLine($"**Conservation Actions:** {currentPlant.conservationActions}");
            }
            else if (q.Contains("animal") || q.Contains("bird") || q.Contains("wildlife") || q.Contains("pollinat") || q.Contains("depend"))
            {
                // Extract wildlife info from ecological importance
                sb.AppendLine("Based on its ecological role:");
                sb.AppendLine(currentPlant.ecologicalImportance);
            }
            else if (q.Contains("identif") || q.Contains("feature") || q.Contains("recognize") || q.Contains("distinguish"))
            {
                sb.AppendLine("**Identifying Features:**");
                sb.AppendLine(currentPlant.identifyingFeatures);
            }
            else if (q.Contains("grow") || q.Contains("plant") || q.Contains("cultivat") || q.Contains("home"))
            {
                sb.AppendLine("**Growing Tips:**");
                sb.AppendLine($"{currentPlant.commonName} is well-adapted to the Vellore climate. ");
                sb.AppendLine("It prefers well-drained soil and can tolerate dry conditions once established.");
                sb.AppendLine("Plant during the monsoon season (June-September) for best results.");
            }
            else if (q.Contains("medicinal") || q.Contains("medicine") || q.Contains("use") || q.Contains("benefit"))
            {
                sb.AppendLine("**Traditional Uses:**");
                sb.AppendLine($"{currentPlant.commonName} has been used in traditional medicine for generations. ");
                sb.AppendLine("However, please consult qualified practitioners before any medicinal use.");
            }
            else if (q.Contains("different") || q.Contains("compare") || q.Contains("versus") || q.Contains("vs"))
            {
                sb.AppendLine($"**Distinguishing {currentPlant.commonName}:**");
                sb.AppendLine(currentPlant.identifyingFeatures);
                sb.AppendLine("\nFor detailed comparison with other species, check the Gallery for side-by-side information.");
            }
            else
            {
                // General response with context
                sb.AppendLine($"That's a great question about **{currentPlant.commonName}**!");
                sb.AppendLine(currentPlant.description);
                sb.AppendLine("\nYou can also ask me about:");
                sb.AppendLine("• Its ecological role and importance");
                sb.AppendLine("• Conservation status and threats");
                sb.AppendLine("• How to identify it in the field");
                sb.AppendLine("• Where it grows naturally");
            }

            return sb.ToString();
        }

        private async System.Threading.Tasks.Task<string> CallRealAI(string question)
        {
            // TODO: Implement actual AI API call
            // This would send the question + plant context to your AI service
            await System.Threading.Tasks.Task.Delay(500); // Simulate network
            return GenerateMockAnswer(question);
        }
    }
}